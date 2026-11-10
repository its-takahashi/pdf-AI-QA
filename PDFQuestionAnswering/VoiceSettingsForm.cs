using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PDFQuestionAnswering
{
	/// <summary>音声設定ダイアログ（コードのみで構築）。OKで設定を確定する。</summary>
	public class VoiceSettingsForm : Form
	{
		private readonly VoiceSpeaker speaker;
		private readonly ComboBox cmbEngine = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
		private readonly ComboBox cmbVoice = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
		private readonly TrackBar trkRate = new TrackBar { Minimum = -50, Maximum = 100, TickFrequency = 25, SmallChange = 5, LargeChange = 10 };
		private readonly TrackBar trkPitch = new TrackBar { Minimum = -50, Maximum = 50, TickFrequency = 25, SmallChange = 5, LargeChange = 10 };
		private readonly Label lblRate = new Label { AutoSize = true };
		private readonly Label lblPitch = new Label { AutoSize = true };
		private readonly Label lblNote = new Label { AutoSize = false, ForeColor = Color.DimGray };
		private readonly Label lblFail = new Label { AutoSize = false, ForeColor = Color.Red, Font = new Font("メイリオ", 9F, FontStyle.Bold) };
		private string[] windowsVoices = new string[0];
		private List<VvStyle> vvStyles = new List<VvStyle>();
private bool loading;

		public VoiceSettings Result { get; private set; }

		public VoiceSettingsForm(VoiceSettings current, VoiceSpeaker speaker)
		{
			this.speaker = speaker;
			Result = current.Clone();

			Text = "音声設定";
			FormBorderStyle = FormBorderStyle.FixedDialog;
			StartPosition = FormStartPosition.CenterParent;
			MaximizeBox = false; MinimizeBox = false;
			ClientSize = new Size(440, 396);
			Font = new Font("メイリオ", 9F);

			try { windowsVoices = VoiceSpeaker.InstalledWindowsVoices(); } catch { }

			int y = 16;
			AddRow("エンジン", cmbEngine, ref y);
			AddRow("声", cmbVoice, ref y);

			Controls.Add(new Label { Text = "速さ", Left = 16, Top = y + 4, AutoSize = true });
			lblRate.Left = 360; lblRate.Top = y + 4; Controls.Add(lblRate);
			y += 24;
			trkRate.SetBounds(12, y, 416, 45); Controls.Add(trkRate);
			y += 52;

			Controls.Add(new Label { Text = "トーン（高さ）", Left = 16, Top = y + 4, AutoSize = true });
			lblPitch.Left = 360; lblPitch.Top = y + 4; Controls.Add(lblPitch);
			y += 24;
			trkPitch.SetBounds(12, y, 416, 45); Controls.Add(trkPitch);
			y += 52;

			lblNote.SetBounds(16, y, 408, 52); Controls.Add(lblNote);
			lblFail.SetBounds(16, y + 56, 408, 40); Controls.Add(lblFail);

			var btnTest = new Button { Text = "試し聞き", Left = 16, Top = 352, Width = 90 };
			var btnStop = new Button { Text = "停止", Left = 112, Top = 352, Width = 60 };
			var btnReset = new Button { Text = "初期値", Left = 178, Top = 352, Width = 70 };
			var btnOk = new Button { Text = "OK", Left = 270, Top = 352, Width = 75, DialogResult = DialogResult.OK };
			var btnCancel = new Button { Text = "キャンセル", Left = 351, Top = 352, Width = 80, DialogResult = DialogResult.Cancel };
			Controls.AddRange(new Control[] { btnTest, btnStop, btnReset, btnOk, btnCancel });
			AcceptButton = btnOk; CancelButton = btnCancel;

			cmbEngine.Items.Add("Edge-TTS（高品質・ネット接続が必要）");
			cmbEngine.Items.Add("Windows標準（オフライン可）");
			vvInstalled = VoicevoxClient.FindEngine() != null;
			cmbEngine.Items.Add(vvInstalled ? "VOICEVOX（無料・このPC内で読み上げ）" : "VOICEVOX（未インストール・選べません）");

			loading = true;
			if (Result.Engine == VoiceSettings.EngineVoicevox && !vvInstalled) Result.Engine = VoiceSettings.EngineWindows;
			cmbEngine.SelectedIndex = Result.Engine == VoiceSettings.EngineEdge ? 0 : (Result.Engine == VoiceSettings.EngineVoicevox ? 2 : 1);
			prevEngineIndex = cmbEngine.SelectedIndex;
trkRate.Value = Result.RatePercent;
			trkPitch.Value = Result.PitchHz;
			loading = false;
			FillVoices();
			UpdateLabels();

			cmbEngine.SelectedIndexChanged += (s, e) =>
			{
				if (loading) return;
				if (cmbEngine.SelectedIndex == 2 && !vvInstalled)
				{
					loading = true; cmbEngine.SelectedIndex = prevEngineIndex; loading = false;
					lblFail.Text = "VOICEVOXがこのPCに見つからないため、選べません。" + Environment.NewLine + "VOICEVOXをインストールすると選べるようになります。";
					return;
				}
				prevEngineIndex = cmbEngine.SelectedIndex;
				lblFail.Text = "";
				ReadBack(); FillVoices(); UpdateLabels();
			};
			cmbVoice.SelectedIndexChanged += (s, e) => { if (!loading) ReadBack(); };
			trkRate.ValueChanged += (s, e) => { if (!loading) { ReadBack(); UpdateLabels(); } };
			trkPitch.ValueChanged += (s, e) => { if (!loading) { ReadBack(); UpdateLabels(); } };

			btnTest.Click += async (s, e) =>
			{
				ReadBack();
				lblFail.Text = "";
				await speaker.SpeakAsync("こんにちは。これは音声設定の試し聞きです。速さとトーンはいかがですか。", Result);
				// Edge-TTSを選んでいるのに使えなかったときは、赤字で知らせる
				if (Result.Engine != VoiceSettings.EngineWindows && speaker.LastFellBack)
lblFail.Text = VoiceSpeaker.FallbackMessage + (string.IsNullOrEmpty(speaker.LastFallbackReason) ? "" : Environment.NewLine + "（" + speaker.LastFallbackReason + "）");
			};
			btnStop.Click += (s, e) => speaker.Stop();
			btnReset.Click += (s, e) =>
			{
				var d = new VoiceSettings { ReadBasis = Result.ReadBasis, AutoRead = Result.AutoRead };
				loading = true;
				Result = d;
				cmbEngine.SelectedIndex = 0;
				trkRate.Value = d.RatePercent;
				trkPitch.Value = d.PitchHz;
				loading = false;
				FillVoices(); UpdateLabels();
			};
			FormClosing += (s, e) => speaker.Stop();
		}

		private void AddRow(string label, Control c, ref int y)
		{
			Controls.Add(new Label { Text = label, Left = 16, Top = y + 4, AutoSize = true });
			c.SetBounds(110, y, 314, 26);
			Controls.Add(c);
			y += 36;
		}

		private bool vvInstalled;
		private int prevEngineIndex;
		private bool IsEdge => cmbEngine.SelectedIndex == 0;
		private bool IsVoicevox => cmbEngine.SelectedIndex == 2;

		private void FillVoices()
		{
			loading = true;
			cmbVoice.Items.Clear();
			cmbVoice.Enabled = true;
			if (IsVoicevox)
			{
				cmbVoice.Items.Add("（VOICEVOXに接続中…）");
				cmbVoice.SelectedIndex = 0;
				cmbVoice.Enabled = false;
				loading = false;
				_ = LoadVoicevoxVoicesAsync();
				return;
			}
			if (IsEdge)
			{
				int sel = 0;
				for (int i = 0; i < VoiceSpeaker.EdgeVoices.GetLength(0); i++)
				{
					cmbVoice.Items.Add(VoiceSpeaker.EdgeVoices[i, 1]);
					if (VoiceSpeaker.EdgeVoices[i, 0] == Result.EdgeVoice) sel = i;
				}
				cmbVoice.SelectedIndex = sel;
			}
			else
			{
				cmbVoice.Items.Add("（既定の音声）");
				int sel = 0;
				for (int i = 0; i < windowsVoices.Length; i++)
				{
					cmbVoice.Items.Add(windowsVoices[i]);
					if (windowsVoices[i] == Result.WindowsVoice) sel = i + 1;
				}
				cmbVoice.SelectedIndex = sel;
			}
			trkPitch.Enabled = true;
			loading = false;
			ReadBack();
		}

		// VOICEVOXのエンジンに接続（動いていなければ起動）して、声の一覧を出す
		private async System.Threading.Tasks.Task LoadVoicevoxVoicesAsync()
		{
			try
			{
				lblFail.Text = "";
				using (var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(70)))
				{
					await VoicevoxClient.EnsureRunningAsync(cts.Token);
					var list = await VoicevoxClient.GetSpeakersAsync(cts.Token);
					if (!IsVoicevox || IsDisposed) return; // 待っている間に別のエンジンへ切り替えられた
					vvStyles = list;
				}
				loading = true;
				cmbVoice.Items.Clear();
				int sel = 0;
				for (int i = 0; i < vvStyles.Count; i++)
				{
					cmbVoice.Items.Add(vvStyles[i].Display);
					if (vvStyles[i].Id == Result.VoicevoxSpeaker) sel = i;
				}
				if (cmbVoice.Items.Count > 0) cmbVoice.SelectedIndex = sel;
				cmbVoice.Enabled = true;
				loading = false;
				ReadBack();
			}
			catch (Exception ex)
			{
				loading = false;
				if (!IsVoicevox || IsDisposed) return;
				cmbVoice.Items.Clear();
				cmbVoice.Items.Add("（VOICEVOXに接続できませんでした）");
				cmbVoice.SelectedIndex = 0;
				cmbVoice.Enabled = false;
				lblFail.Text = "VOICEVOXに接続できませんでした。" + Environment.NewLine + "（" + ex.Message + "）";
			}
		}

		private void ReadBack()
		{
			Result.Engine = IsEdge ? VoiceSettings.EngineEdge : (IsVoicevox ? VoiceSettings.EngineVoicevox : VoiceSettings.EngineWindows);
			if (IsVoicevox)
			{
				if (cmbVoice.Enabled && cmbVoice.SelectedIndex >= 0 && cmbVoice.SelectedIndex < vvStyles.Count)
				{
					Result.VoicevoxSpeaker = vvStyles[cmbVoice.SelectedIndex].Id;
					Result.VoicevoxSpeakerName = vvStyles[cmbVoice.SelectedIndex].Display;
				}
			}
			else if (cmbVoice.SelectedIndex >= 0)
			{
				if (IsEdge) Result.EdgeVoice = VoiceSpeaker.EdgeVoices[cmbVoice.SelectedIndex, 0];
				else Result.WindowsVoice = cmbVoice.SelectedIndex == 0 ? "" : windowsVoices[cmbVoice.SelectedIndex - 1];
			}
			Result.RatePercent = trkRate.Value;
			Result.PitchHz = trkPitch.Value;
		}

		private void UpdateLabels()
		{
			lblRate.Text = (trkRate.Value >= 0 ? "+" : "") + trkRate.Value + " %";
			lblPitch.Text = (trkPitch.Value >= 0 ? "+" : "") + trkPitch.Value + (IsEdge ? " Hz" : "（目安）");
			lblNote.Text = IsEdge
				? "読み上げる文章はMicrosoftのサーバーに送られます。社外秘の文書では「Windows標準」または「VOICEVOX」を選んでください。"
				: (IsVoicevox
					? "VOICEVOXはこのPCの中で読み上げます（文章は外に出ません）。使うときは「VOICEVOX:キャラクター名」のクレジット表記が必要です。動いていなければ、アプリが自動で起動します。"
					: "Windows標準は端末内で読み上げます（Ichiro・Haruka・Ayumi・Sayakaなど）。");
		}
	}
}
