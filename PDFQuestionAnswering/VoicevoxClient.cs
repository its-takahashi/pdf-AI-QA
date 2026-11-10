using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PDFQuestionAnswering
{
	/// <summary>VOICEVOXの声（キャラクターとスタイル）</summary>
	public class VvStyle
	{
		public int Id;
		public string Display; // 例: ずんだもん（ノーマル）
		public override string ToString() { return Display; }
	}

	/// <summary>
	/// VOICEVOX（このPC内で動く読み上げエンジン）と話す部品。
	/// 文章はこのPCの中（127.0.0.1）だけで処理され、外には出ない。
	/// </summary>
	public static class VoicevoxClient
	{
		private const string BaseUrl = "http://127.0.0.1:50021";
		private static readonly HttpClient Http = new HttpClient { Timeout = TimeSpan.FromSeconds(120) };
		private static readonly object Gate = new object();
		private static Process started; // このアプリが起動したエンジン（自分で起動したものだけ、終了時に止める）

		/// <summary>VOICEVOXのエンジン（run.exe）の場所。見つからなければ null</summary>
		public static string FindEngine()
		{
			string[] roots =
			{
				Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "VOICEVOX"),
				Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "VOICEVOX"),
				Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "VOICEVOX"),
			};
			foreach (var r in roots)
			{
				string exe = Path.Combine(r, "vv-engine", "run.exe");
				if (File.Exists(exe)) return exe;
			}
			return null;
		}

		public static async Task<bool> PingAsync(CancellationToken ct)
		{
			try
			{
				using (var r = await Http.GetAsync(BaseUrl + "/version", ct))
					return r.IsSuccessStatusCode;
			}
			catch (OperationCanceledException) { throw; }
			catch { return false; }
		}

		/// <summary>エンジンが動いていなければ、起動して待つ</summary>
		public static async Task EnsureRunningAsync(CancellationToken ct, int timeoutSeconds = 60)
		{
			if (await PingAsync(ct)) return;

			string exe = FindEngine();
			if (exe == null)
				throw new InvalidOperationException("VOICEVOXが見つかりません（インストールされていないか、場所が違います）");

			lock (Gate)
			{
				if (started == null || started.HasExited)
				{
					var psi = new ProcessStartInfo(exe, "--host 127.0.0.1 --port 50021")
					{
						UseShellExecute = false,
						CreateNoWindow = true,
						WorkingDirectory = Path.GetDirectoryName(exe),
					};
					started = Process.Start(psi);
				}
			}

			var until = DateTime.UtcNow.AddSeconds(timeoutSeconds);
			while (DateTime.UtcNow < until)
			{
				ct.ThrowIfCancellationRequested();
				if (await PingAsync(ct)) return;
				if (started != null && started.HasExited)
					throw new InvalidOperationException("VOICEVOXのエンジンが起動できませんでした");
				await Task.Delay(500, ct);
			}
			throw new TimeoutException("VOICEVOXのエンジンの起動を待ちましたが、時間切れでした");
		}

		/// <summary>アプリが起動したエンジンだけを止める（ユーザーが自分で起動したVOICEVOXは止めない）</summary>
		public static void StopStartedEngine()
		{
			try
			{
				lock (Gate)
				{
					if (started != null && !started.HasExited) started.Kill();
					started = null;
				}
			}
			catch { }
		}

		public static async Task<List<VvStyle>> GetSpeakersAsync(CancellationToken ct)
		{
			string json;
			using (var r = await Http.GetAsync(BaseUrl + "/speakers", ct))
			{
				r.EnsureSuccessStatusCode();
				json = await r.Content.ReadAsStringAsync();
			}
			var list = new List<VvStyle>();
			foreach (var sp in JArray.Parse(json))
			{
				string name = (string)sp["name"];
				foreach (var st in (JArray)sp["styles"] ?? new JArray())
					list.Add(new VvStyle { Id = (int)st["id"], Display = name + "（" + (string)st["name"] + "）" });
			}
			return list;
		}

		/// <summary>1つの文章を音声（WAV）にする。speed: 1.0が標準、pitch: 0が標準（±0.15程度）</summary>
		public static async Task<byte[]> SynthesizeAsync(string text, int speakerId, double speed, double pitch, CancellationToken ct)
		{
			string query;
			string url = BaseUrl + "/audio_query?speaker=" + speakerId + "&text=" + Uri.EscapeDataString(text);
			using (var req = new HttpRequestMessage(HttpMethod.Post, url) { Content = new StringContent("") })
			using (var r = await Http.SendAsync(req, ct))
			{
				r.EnsureSuccessStatusCode();
				query = await r.Content.ReadAsStringAsync();
			}

			var q = JObject.Parse(query);
			q["speedScale"] = speed;
			q["pitchScale"] = pitch;

			using (var content = new StringContent(q.ToString(Formatting.None), Encoding.UTF8, "application/json"))
			using (var r = await Http.PostAsync(BaseUrl + "/synthesis?speaker=" + speakerId, content, ct))
			{
				r.EnsureSuccessStatusCode();
				return await r.Content.ReadAsByteArrayAsync();
			}
		}

		/// <summary>長い文章を、読点・句点・改行で区切って、max文字くらいのかたまりに分ける（読み始めを早くするため）</summary>
		public static List<string> SplitText(string text, int max)
		{
			var result = new List<string>();
			var sb = new StringBuilder();
			foreach (char c in text ?? "")
			{
				sb.Append(c);
				bool end = c == '。' || c == '！' || c == '？' || c == '!' || c == '?' || c == '\n';
				if ((end && sb.Length >= max / 2) || sb.Length >= max)
				{
					AddIfSpeakable(result, sb.ToString());
					sb.Clear();
				}
			}
			AddIfSpeakable(result, sb.ToString());
			return result;
		}

		private static void AddIfSpeakable(List<string> list, string s)
		{
			if (!string.IsNullOrWhiteSpace(s)) list.Add(s.Trim());
		}
	}
}
