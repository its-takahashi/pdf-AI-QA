using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PDFQuestionAnswering
{
	/// <summary>
	/// 読み上げ担当。Edge-TTS（Python の edge-tts を外部コマンドで呼ぶ）か Windows標準音声を使う。
	/// Edge-TTSが使えない時（Python無し・オフライン等）は Windows標準音声に切り替える。
	/// </summary>
	public sealed class VoiceSpeaker : IDisposable
	{
		public static readonly string[,] EdgeVoices =
		{
			{ "ja-JP-NanamiNeural",  "Nanami（女性）" },
			{ "ja-JP-KeitaNeural",   "Keita（男性）" },
		};

		private System.Windows.Media.MediaPlayer player;

		private readonly SpeechSynthesizer synth = new SpeechSynthesizer();
		private CancellationTokenSource cts;
		private Process proc;
		private string mp3Path;
		private int launcherIndex = -1; // 動作確認できた起動方法を覚える

		/// <summary>直前の読み上げで、Edge-TTSが使えずWindows標準に切り替えたか</summary>
		public bool LastFellBack { get; private set; }
		public string LastFallbackReason { get; private set; } = "";
		public const string FallbackMessage = "使用できませんでした。Windows標準で再生しています";

		/// <summary>状態メッセージ（UIスレッドから呼ぶこと）</summary>
		public event Action<string> Status;

		/// <summary>Windowsにインストールされた音声名（Ichiroなど「音声」設定の音声も含む）</summary>
		public static string[] InstalledWindowsVoices()
		{
			try
			{
				return Windows.Media.SpeechSynthesis.SpeechSynthesizer.AllVoices
					.OrderByDescending(v => v.Language.StartsWith("ja"))
					.ThenBy(v => v.DisplayName)
					.Select(v => v.DisplayName).Distinct().ToArray();
			}
			catch
			{
				using (var s = new SpeechSynthesizer())
					return s.GetInstalledVoices().Where(v => v.Enabled).Select(v => v.VoiceInfo.Name).ToArray();
			}
		}

		public async Task SpeakAsync(string text, VoiceSettings s)
		{
			Stop();
			LastFellBack = false;
			LastFallbackReason = "";
			if (string.IsNullOrWhiteSpace(text)) return;

			if (s.Engine == VoiceSettings.EngineEdge || s.Engine == VoiceSettings.EngineVoicevox)
			{
				var myCts = new CancellationTokenSource();
				cts = myCts;
				try
				{
					if (s.Engine == VoiceSettings.EngineEdge)
					{
						Status?.Invoke("Edge-TTSで音声を作成中…");
						await SpeakEdgeAsync(text, s, myCts.Token);
						Status?.Invoke("");
					}
					else
					{
						await SpeakVoicevoxAsync(text, s, myCts.Token);
					}
					return;
				}
				catch (OperationCanceledException)
				{
					return;
				}
				catch (Exception ex)
				{
					LastFellBack = true;
					LastFallbackReason = ex.Message;
					Status?.Invoke(FallbackMessage);
				}
			}
			await SpeakWindowsAsync(text, s);
		}

		public void Stop()
		{
			try { cts?.Cancel(); } catch { }
			try { if (proc != null && !proc.HasExited) proc.Kill(); } catch { }
			StopMp3();
			StopVoicevoxPlayback();
			try { synth.SpeakAsyncCancelAll(); } catch { }
		}

		// ---------- VOICEVOX（このPC内の読み上げエンジン。文章は外に出ない） ----------
		private readonly System.Collections.Generic.Queue<string> vvQueue = new System.Collections.Generic.Queue<string>();
		private readonly System.Collections.Generic.List<string> vvTemp = new System.Collections.Generic.List<string>();
		private System.Windows.Media.MediaPlayer vvPlayer;
		private bool vvPlaying;

		private async Task SpeakVoicevoxAsync(string text, VoiceSettings s, CancellationToken ct)
		{
			Status?.Invoke("VOICEVOXを準備中…");
			await VoicevoxClient.EnsureRunningAsync(ct);

			double speed = Math.Max(0.5, Math.Min(2.0, 1.0 + s.RatePercent / 100.0));
			double pitch = Math.Max(-0.15, Math.Min(0.15, s.PitchHz / 333.0));

			bool first = true;
			foreach (var chunk in VoicevoxClient.SplitText(text, 100))
			{
				ct.ThrowIfCancellationRequested();
				byte[] wav = await VoicevoxClient.SynthesizeAsync(chunk, s.VoicevoxSpeaker, speed, pitch, ct);
				ct.ThrowIfCancellationRequested();

				string path = Path.Combine(Path.GetTempPath(), "pdfqa_vv_" + Guid.NewGuid().ToString("N") + ".wav");
				File.WriteAllBytes(path, wav);
				EnqueueVoicevoxWav(path);

				if (first)
				{
					first = false;
					// VOICEVOXの利用規約で必要な、キャラクター名のクレジット表記
					Status?.Invoke("VOICEVOX:" + CreditName(s.VoicevoxSpeakerName));
				}
			}
		}

		private static string CreditName(string display)
		{
			if (string.IsNullOrEmpty(display)) return "";
			int i = display.IndexOf('（');
			return i > 0 ? display.Substring(0, i) : display;
		}

		private void EnqueueVoicevoxWav(string path)
		{
			vvTemp.Add(path);
			vvQueue.Enqueue(path);
			if (!vvPlaying) PlayNextVoicevox();
		}

		private void PlayNextVoicevox()
		{
			if (vvQueue.Count == 0) { vvPlaying = false; return; }
			vvPlaying = true;
			string path = vvQueue.Dequeue();
			var pl = new System.Windows.Media.MediaPlayer();
			vvPlayer = pl;
			pl.MediaEnded += (s2, e2) =>
			{
				if (vvPlayer != pl) return;
				try { pl.Close(); } catch { }
				try { File.Delete(path); vvTemp.Remove(path); } catch { }
				PlayNextVoicevox();
			};
			pl.MediaFailed += (s2, e2) =>
			{
				Status?.Invoke("再生エラー: " + e2.ErrorException?.Message);
				if (vvPlayer != pl) return;
				try { pl.Close(); } catch { }
				PlayNextVoicevox();
			};
			pl.Open(new Uri(path));
			pl.Play();
		}

		private void StopVoicevoxPlayback()
		{
			vvQueue.Clear();
			vvPlaying = false;
			var pl = vvPlayer;
			vvPlayer = null;
			if (pl != null)
			{
				try { pl.Stop(); pl.Close(); } catch { }
			}
			foreach (var f in vvTemp.ToArray())
			{
				try { File.Delete(f); } catch { }
			}
			vvTemp.Clear();
		}

		// ---------- Windows標準音声 ----------
		private async Task SpeakWindowsAsync(string text, VoiceSettings s)
		{
			try
			{
				await SpeakWinRtAsync(text, s);
			}
			catch (OperationCanceledException) { }
			catch (Exception)
			{
				SpeakLegacy(text, s); // WinRTが使えない環境では従来のSystem.Speech
			}
		}

		// Windows 10/11 の音声（Ichiro / Haruka / Ayumi / Sayaka など）。速さとトーンも反映できる
		private async Task SpeakWinRtAsync(string text, VoiceSettings s)
		{
			var myCts = new CancellationTokenSource();
			cts = myCts;
			using (var ws = new Windows.Media.SpeechSynthesis.SpeechSynthesizer())
			{
				if (!string.IsNullOrEmpty(s.WindowsVoice))
				{
					var v = Windows.Media.SpeechSynthesis.SpeechSynthesizer.AllVoices
						.FirstOrDefault(x => x.DisplayName == s.WindowsVoice);
					if (v != null) ws.Voice = v;
				}
				ws.Options.SpeakingRate = Math.Max(0.5, Math.Min(3.0, 1.0 + s.RatePercent / 100.0));
				ws.Options.AudioPitch = Math.Max(0.0, Math.Min(2.0, 1.0 + s.PitchHz / 100.0));

				using (var stream = await ws.SynthesizeTextToStreamAsync(text))
				{
					myCts.Token.ThrowIfCancellationRequested();
					string wav = Path.Combine(Path.GetTempPath(), "pdfqa_" + Guid.NewGuid().ToString("N") + ".wav");
					using (var src = System.IO.WindowsRuntimeStreamExtensions.AsStreamForRead(stream))
					using (var dst = File.Create(wav))
						await src.CopyToAsync(dst);
					myCts.Token.ThrowIfCancellationRequested();
					PlayMp3(wav); // WAVもMediaPlayerで再生できる
				}
			}
		}

		private void SpeakLegacy(string text, VoiceSettings s)
		{
			try
			{
				if (!string.IsNullOrEmpty(s.WindowsVoice) &&
					synth.GetInstalledVoices().Any(v => v.Enabled && v.VoiceInfo.Name == s.WindowsVoice))
					synth.SelectVoice(s.WindowsVoice);
			}
			catch { }
			synth.Rate = Math.Max(-10, Math.Min(10, (int)Math.Round(s.RatePercent / 10.0)));
			synth.SpeakAsync(text);
		}

		// ---------- Edge-TTS ----------
		// 起動方法の候補（先頭ほど優先）。アプリと同じ場所の edge-tts\edge-tts.exe があれば、それが最優先
		private static readonly string[][] Launchers = BuildLaunchers();

		private static string[][] BuildLaunchers()
		{
			var list = new System.Collections.Generic.List<string[]>();
			string bundled = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "edge-tts", "edge-tts.exe");
			if (File.Exists(bundled)) list.Add(new[] { bundled, "" });
			list.Add(new[] { "edge-tts", "" });
			list.Add(new[] { "python", "-m edge_tts " });
			list.Add(new[] { "py", "-m edge_tts " });
			return list.ToArray();
		}

		private async Task SpeakEdgeAsync(string text, VoiceSettings s, CancellationToken ct)
		{
			string txtPath = Path.Combine(Path.GetTempPath(), "pdfqa_" + Guid.NewGuid().ToString("N") + ".txt");
			string outPath = Path.ChangeExtension(txtPath, ".mp3");
			File.WriteAllText(txtPath, text, new UTF8Encoding(false));

			try
			{
				string args =
					"--voice " + s.EdgeVoice +
					" --rate=" + (s.RatePercent >= 0 ? "+" : "") + s.RatePercent + "%" +
					" --pitch=" + (s.PitchHz >= 0 ? "+" : "") + s.PitchHz + "Hz" +
					" --file \"" + txtPath + "\" --write-media \"" + outPath + "\"";

				string lastError = "edge-tts が見つかりません（配布フォルダの edge-tts フォルダを、アプリと同じ場所に置いてください）";
				var order = Enumerable.Range(0, Launchers.Length).ToList();
				if (launcherIndex >= 0) { order.Remove(launcherIndex); order.Insert(0, launcherIndex); }

				bool ok = false;
				foreach (int i in order)
				{
					ct.ThrowIfCancellationRequested();
					var r = await RunAsync(Launchers[i][0], Launchers[i][1] + args, ct);
					if (r.NotFound) { LogEdge(Launchers[i][0], args, r); continue; }
					if (r.ExitCode == 0 && File.Exists(outPath) && new FileInfo(outPath).Length > 0)
					{
						launcherIndex = i; ok = true; break;
					}
					if (!(r.ExitCode == 0 && File.Exists(outPath))) LogEdge(Launchers[i][0], args, r);
					lastError = Shorten(r.StdErr);
					if (r.StdErr != null && r.StdErr.Contains("No module named")) continue;
					break;
				}
				if (!ok) throw new InvalidOperationException(lastError);

				ct.ThrowIfCancellationRequested();
				PlayMp3(outPath);
			}
			finally
			{
				try { File.Delete(txtPath); } catch { }
			}
		}

		// 失敗の詳細を %APPDATA%\PDFQuestionAnswering\edge_tts.log に残す（原因調査用）
		private static void LogEdge(string file, string args, RunResult r)
		{
			try
			{
				string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PDFQuestionAnswering");
				Directory.CreateDirectory(dir);
				File.AppendAllText(Path.Combine(dir, "edge_tts.log"),
					DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  起動: " + file + "\r\n" +
					"  引数: " + args + "\r\n" +
					"  アプリの場所: " + AppDomain.CurrentDomain.BaseDirectory + "\r\n" +
					"  結果: " + (r.NotFound ? "見つからない" : "終了コード " + r.ExitCode) + "\r\n" +
					"  エラー出力: " + (r.StdErr ?? "").Trim() + "\r\n\r\n", Encoding.UTF8);
			}
			catch { }
		}

		private class RunResult { public bool NotFound; public int ExitCode; public string StdErr; }

		private async Task<RunResult> RunAsync(string file, string args, CancellationToken ct)
		{
			var psi = new ProcessStartInfo(file, args)
			{
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardError = true,
				RedirectStandardOutput = true,
				StandardErrorEncoding = Encoding.UTF8,
				StandardOutputEncoding = Encoding.UTF8,
			};
			psi.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";

			Process p;
			try { p = Process.Start(psi); }
			catch (Win32Exception) { return new RunResult { NotFound = true }; }

			proc = p;
			using (p)
			{
				var errTask = p.StandardError.ReadToEndAsync();
				var outTask = p.StandardOutput.ReadToEndAsync();
				var exit = Task.Run(() => p.WaitForExit(60000));
				while (!exit.IsCompleted)
				{
					if (ct.IsCancellationRequested) { try { p.Kill(); } catch { } ct.ThrowIfCancellationRequested(); }
					await Task.Delay(50);
				}
				if (!exit.Result) { try { p.Kill(); } catch { } return new RunResult { ExitCode = -1, StdErr = "タイムアウト（60秒）" }; }
				string err = await errTask; await outTask;
				return new RunResult { ExitCode = p.ExitCode, StdErr = err };
			}
		}

		private static string Shorten(string s)
		{
			if (string.IsNullOrWhiteSpace(s)) return "不明なエラー";
			var lines = s.Trim().Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0).ToArray();
			s = string.Join(" / ", lines.Skip(Math.Max(0, lines.Length - 3)));
			return s.Length > 240 ? s.Substring(0, 240) + "…" : s;
		}

		// ---------- mp3再生（WPF MediaPlayer） ----------
		private void PlayMp3(string path)
		{
			StopMp3();
			mp3Path = path;
			player = new System.Windows.Media.MediaPlayer();
			player.MediaFailed += (s, e) => Status?.Invoke("再生エラー: " + e.ErrorException?.Message);
			player.Open(new Uri(path));
			player.Play();
		}

		private void StopMp3()
		{
			if (player != null)
			{
				try { player.Stop(); player.Close(); } catch { }
				player = null;
			}
			if (mp3Path != null)
			{
				try { File.Delete(mp3Path); } catch { }
				mp3Path = null;
			}
		}

		public void Dispose()
		{
			Stop();
			VoicevoxClient.StopStartedEngine();
			synth.Dispose();
		}
	}
}
