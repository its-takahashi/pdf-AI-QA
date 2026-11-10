using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Text;
//using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Path = System.IO.Path;


namespace PDFQuestionAnswering
{
	public partial class Form1 : Form
	{
		// ====== 設定・状態 ======
		private string geminiApiKey = "";
		private string geminiModel = "gemini-3.5-flash-lite";

		private readonly List<string> pdfContents = new List<string>();
		private readonly List<string> pdfFilePaths = new List<string>();

		// HttpClientは使い回し（Disposeパターン対応）
		private static readonly HttpClient Http = new HttpClient
		{
			Timeout = TimeSpan.FromSeconds(60)
		};

		// ====== クォータ・使用回数の簡易管理（PT基準） ======
		private int defaultQuota = 20; // モデルごとの上限が未設定のときの既定。App.configで上書き
		private ModelSettings modelSettings = ModelSettings.Load();
		private int dailyQuota => modelSettings.QuotaFor(geminiModel, defaultQuota);
		// 使用回数はモデルごとに数える（上限がモデルごとに違うため）
		private Dictionary<string, int> usageCounts = new Dictionary<string, int>();
		private int usageCountToday
		{
			get { return usageCounts.TryGetValue(geminiModel, out var c) ? c : 0; }
			set { usageCounts[geminiModel] = value; }
		}
		private string usageDatePacific = "";

		// ====== リトライ設定（429/5xx） ======
		private int retryMaxAttempts = 3;
		private int retryBaseDelayMs = 1000;
		private double retryBackoffFactor = 2.0;
		private readonly Random _rng = new Random();

		// ====== 正規表現キャッシュ（パフォーマンス改善） ======
		private static readonly ConcurrentDictionary<string, Regex> RegexCache =
			new ConcurrentDictionary<string, Regex>();

		// 英数字だけで構成される語かを判定するための事前コンパイル済みRegex（高速化用）
		private static readonly Regex AsciiRegex = new Regex(@"^[A-Za-z0-9]+$", RegexOptions.Compiled);



		// 使用状況保存ファイル（%LOCALAPPDATA%）
		private string UsageFilePath =>
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
						"PDFQuestionAnswering", "usage.json");

		// 「PDF外は絶対に表示しない」厳格抽出モード（既定ON）
		private bool strictExtractiveMode = true;

        // ====== 音声認識・合成 ======
        private VoiceSpeaker speaker;
        private VoiceSettings voiceSettings;
        private UiSettings uiSettings;
        private bool refreshingModelCombo;
        private bool armSubmitOnEnter; // 音声入力をEnterで止めた直後だけ true（次のEnterで質問）

        //音声認識エンジン
        private SpeechRecognitionEngine recognizer;
        private bool isListening = false;

        SpeechRecognitionEngine engine;

        // Win32 API定数
        private const byte VK_LWIN = 0x5B;
        private const byte VK_H = 0x48;
        private const uint KEYEVENTF_KEYUP = 0x0002;
        // Win32 API宣言
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        private static extern IntPtr GetShellWindow();
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        public Form1()
		{
			InitializeComponent();

			Http.DefaultRequestHeaders.Accept.Clear();
			Http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			speaker = new VoiceSpeaker();
			speaker.Status += msg =>
			{
				if (tsslRate == null) return;
				tsslRate.Text = msg;
				tsslRate.ForeColor = msg.StartsWith("使用できませんでした") ? System.Drawing.Color.Red : System.Drawing.SystemColors.ControlText;
			};
			voiceSettings = VoiceSettings.Load();
		}

		// ====== フォーム起動時 ======
		private void Form1_Load(object sender, EventArgs e)
		{
			// 音声設定はメニューから行う（旧：日本語/英語ラジオと音声ボイス確認は不要になったので隠す）
			// 横は自由、縦は回答欄だけ伸縮（初期サイズより小さくはしない）
			MinimumSize = Size;
			btnAskQuestion.Enabled = true; // PDFが無いときは、押したらメッセージを出す
			radioButton1.Visible = false;
			radioButton2.Visible = false;
			button3.Visible = false;
			chkReadBasis.Checked = voiceSettings.ReadBasis;
			chkReadBasis.CheckedChanged += (s2, e2) => { voiceSettings.ReadBasis = chkReadBasis.Checked; voiceSettings.Save(); };
			// 厳格モードのチェックも次の起動時まで覚える
			uiSettings = UiSettings.Load();
			chkStrictMode.Checked = uiSettings.StrictMode;
			chkStrictMode.CheckedChanged += (s2, e2) => { uiSettings.StrictMode = chkStrictMode.Checked; uiSettings.Save(); };
			chkContinue.Checked = uiSettings.ContinueChat;
			chkContinue.CheckedChanged += (s2, e2) =>
			{
				uiSettings.ContinueChat = chkContinue.Checked;
				uiSettings.Save();
				if (!chkContinue.Checked) ResetContinue();
			};
			chkAutoRead.Checked = voiceSettings.AutoRead;
			chkAutoRead.CheckedChanged += (s2, e2) => { voiceSettings.AutoRead = chkAutoRead.Checked; voiceSettings.Save(); };
			chkNoPdf.CheckedChanged += (s2, e2) =>
			{
				ResetContinue();
				if (tsslRate != null) tsslRate.Text = chkNoPdf.Checked ? "PDFなし質問モード（一般回答・根拠なし）" : "";
			};
			var voiceMenu = new ToolStripMenuItem("音声設定(&V)...");
			voiceMenu.Click += (s2, e2) => ShowVoiceSettings();
			menuStrip1.Items.Add(voiceMenu);
			// モデル選択の行の部品は、デザイナー（Form1.Designer.cs）にある。ここでは動作だけをつなぐ
			// 回答欄のURLをクリックしたら、ブラウザで開く（http / https のみ）
			ritAnswer.LinkClicked += (s2, e2) =>
			{
				Uri uri;
				if (Uri.TryCreate(e2.LinkText, UriKind.Absolute, out uri) &&
					(uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
				{
					try { System.Diagnostics.Process.Start(uri.AbsoluteUri); }
					catch (Exception ex) { MessageBox.Show(this, "リンクを開けませんでした: " + ex.Message); }
				}
			};
			btnModelSettings.Click += (s2, e2) => ShowModelSettings();
			cmbModel.SelectedIndexChanged += (s2, e2) => OnModelComboChanged();

			//txtFlag.Text = "0"; // 既定値を0に設定

            // ボタンの色を設定
            button4.BackColor = System.Drawing.Color.FromArgb(255, 255, 192);

            // 音声入力(Win+H)は質問欄フォーカス時(txtQuestion_Enter)に起動する

            // フォームをアクティブにする
            IntPtr handle = FindWindow(null, this.Text);
			if (handle != IntPtr.Zero)
            {
				SetForegroundWindow(handle);
            }

			txtQuestion.Focus();




            try
            {
				// 初期表示（フォント/メッセージ）
				SetupAnswerTextBox();

				//settings.json を読み込む（APIキー/モデル/クォータ/リトライ設定）
				try
				{
					var savedKey = AppSettings.Instance.GeminiApiKey;
					if (!string.IsNullOrWhiteSpace(savedKey))
					{
						// App.config より settings.json を優先して採用
						geminiApiKey = savedKey.Trim();
						if (lblApiKeyStatus != null)
						{
							lblApiKeyStatus.Text = "API Key: settings.jsonから読込";
							lblApiKeyStatus.ForeColor = System.Drawing.Color.Green;
						}
					}
				}
				catch (Exception ex)
				{
					// 読み込み失敗は無視（App.configや手動入力にフォールバック）
					MessageBox.Show($"設定読み込みエラー: {ex.Message}", "エラー",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
				}

				// 本日(PT)の使用状況を読み込み → 表示更新
				LoadAppConfig();
				RefreshModelCombo();
				LoadUsageState();
				UpdateQuotaLabel();
				LoadQaPatterns();

				if (tsslRate != null)
					tsslRate.Text = "";

				// App.config／settings.json のどちらにも無い場合のみ促す
				if (string.IsNullOrEmpty(geminiApiKey))
				{
					MessageBox.Show("最初にGemini API Keyを設定してください。", "API Key設定",
						MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"初期化エラー: {ex.Message}", "エラー",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

        // 音声が認識されたときに呼ばれるイベントハンドラー
        private void Recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            // 認識結果のテキストをテキストボックスに追加
            txtQuestion.AppendText(e.Result.Text + " ");
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{

		}
		// ====== メニュー：APIキー設定 ======
		private void aPIキーの設定ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using (var dlg = new GeminiApiKeyForm())
			{
				if (dlg.ShowDialog(this) == DialogResult.OK)
				{
					// AppSettings に保存済みの値を、実行中のフィールドへ反映
					geminiApiKey = AppSettings.Instance.GeminiApiKey ?? "";
					if (lblApiKeyStatus != null)
					{
						lblApiKeyStatus.Text = "API Key: 設定済み（settings.json）";
						lblApiKeyStatus.ForeColor = System.Drawing.Color.Green;
					}
				}
			}

		}

		// ====== UI：回答欄 初期化 ======
		private void SetupAnswerTextBox()
		{
			ritAnswer.Font = new System.Drawing.Font("メイリオ", 9F, System.Drawing.FontStyle.Regular);
			ritAnswer.BackColor = System.Drawing.Color.FromArgb(248, 249, 250);
			ritAnswer.ForeColor = System.Drawing.Color.FromArgb(33, 37, 41);
			ritAnswer.WordWrap = true;
			ritAnswer.DetectUrls = true; // 回答に含まれるURLをクリックで開けるようにする
			SetInitialMessage();
		}

		private void SetInitialMessage()
		{
			ritAnswer.Clear();
			ritAnswer.SelectionFont = new System.Drawing.Font("メイリオ", 11F, System.Drawing.FontStyle.Bold);
			ritAnswer.SelectionColor = System.Drawing.Color.FromArgb(0, 123, 255);
			ritAnswer.AppendText("📋 PDF質問応答システム");

			ritAnswer.AppendText("\n\n");
			ritAnswer.SelectionFont = new System.Drawing.Font("メイリオ", 10F, System.Drawing.FontStyle.Regular);
			ritAnswer.SelectionColor = System.Drawing.Color.FromArgb(108, 117, 125);
			ritAnswer.AppendText("ここに回答が表示されます。\n\n使用方法：\n");

			ritAnswer.SelectionFont = new System.Drawing.Font("メイリオ", 10F, System.Drawing.FontStyle.Regular);
			ritAnswer.SelectionColor = System.Drawing.Color.FromArgb(33, 37, 41);
			ritAnswer.AppendText("1. API Keyを設定\n");
			ritAnswer.AppendText("2. PDFファイルを選択・読み込み\n");
			ritAnswer.AppendText("3. 質問を入力して「質問する」をクリック");
		}



		// ====== クォータ表示更新 ======
		private void UpdateQuotaLabel()
		{
			int remaining = Math.Max(0, dailyQuota - usageCountToday);
			if (tsslQuota != null)
				tsslQuota.Text = $"{geminiModel}　本日 {usageCountToday}/{dailyQuota}（残り {remaining}）";
		}

		// ====== PT（太平洋時間）ユーティリティ ======
		private static TimeZoneInfo GetPacificTz()
		{
			try
			{
				return TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
			}
			catch
			{
				try
				{
					return TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles");
				}
				catch
				{
					// フォールバック: UTC-8として扱う
					return TimeZoneInfo.CreateCustomTimeZone("Pacific", TimeSpan.FromHours(-8), "Pacific", "Pacific");
				}
			}
		}

		private static string GetTodayPacificString()
		{
			var tz = GetPacificTz();
			var nowPt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
			return nowPt.ToString("yyyy-MM-dd");
		}

		// ====== 使用状況の読み書き（改良版：例外処理強化） ======
		private void LoadUsageState()
		{
			try
			{
				if (File.Exists(UsageFilePath))
				{
					var json = File.ReadAllText(UsageFilePath, Encoding.UTF8);
					var state = JsonConvert.DeserializeObject<UsageState>(json);
					if (state != null)
					{
						usageDatePacific = state.DatePacific ?? "";
						usageCounts = state.Counts != null
							? state.Counts.Where(kv => kv.Value > 0).ToDictionary(kv => kv.Key, kv => kv.Value)
							: new Dictionary<string, int>();
					}
				}
			}
			catch (FileNotFoundException)
			{
				// ファイルが存在しない場合は正常（初回起動）
			}
			catch (Newtonsoft.Json.JsonException)
			{
				// JSON破損の場合はリセット
				usageDatePacific = "";
				usageCounts.Clear();
			}
			catch (Exception)
			{
				// その他のエラーも同様にリセット
				usageDatePacific = "";
				usageCounts.Clear();
			}

			var todayPt = GetTodayPacificString();
			if (!string.Equals(usageDatePacific, todayPt, StringComparison.Ordinal))
			{
				usageDatePacific = todayPt;
				usageCounts.Clear();
				SaveUsageState();
			}
		}

		private void SaveUsageState()
		{
			try
			{
				var dir = Path.GetDirectoryName(UsageFilePath);
				if (!Directory.Exists(dir))
					Directory.CreateDirectory(dir);

				var state = new UsageState
				{
					DatePacific = usageDatePacific,
					Counts = usageCounts
				};
				var json = JsonConvert.SerializeObject(state);
				File.WriteAllText(UsageFilePath, json, Encoding.UTF8);
			}
			catch (DirectoryNotFoundException)
			{
				// ディレクトリ作成に失敗した場合も無視
			}
			catch (UnauthorizedAccessException)
			{
				// アクセス権限がない場合も無視
			}
			catch (IOException)
			{
				// ファイルI/Oエラーも無視
			}
		}

		private bool CanConsumeRequest()
		{
			var todayPt = GetTodayPacificString();
			if (!string.Equals(usageDatePacific, todayPt, StringComparison.Ordinal))
			{
				usageDatePacific = todayPt;
				usageCounts.Clear();
				SaveUsageState();
			}
			return usageCountToday < dailyQuota;
		}

		// ====== Liteモデル使用中の注意（回答の末尾に表示。読み上げには含めない） ======
		private const string LiteNote = "※ 精度が低い場合はLiteではないモデルを選択してください。";

		private bool lastAnswerWasNoPdf; // 直近の回答が「PDFなし質問」のものか

		private bool IsLiteModel => geminiModel.IndexOf("lite", StringComparison.OrdinalIgnoreCase) >= 0;

		private void AppendLiteNoteIfNeeded()
		{
			if (!IsLiteModel) return;
			ritAnswer.ReadOnly = false;
			ritAnswer.SelectionStart = ritAnswer.TextLength;
			ritAnswer.SelectionLength = 0;
			ritAnswer.SelectionFont = new System.Drawing.Font(ritAnswer.Font.FontFamily, 9F, System.Drawing.FontStyle.Regular);
			ritAnswer.SelectionColor = System.Drawing.Color.FromArgb(253, 126, 20);
			ritAnswer.AppendText(Environment.NewLine + Environment.NewLine + LiteNote);
			ritAnswer.ReadOnly = true;
			ritAnswer.SelectionStart = 0;
			ritAnswer.ScrollToCaret();
		}

		// 今のモデルが上限に達したとき、まだ回数が残っている次のモデルへの切り替えを確認する。
		// 切り替えたら true。候補が無い・断られたら false。
		private bool OfferSwitchToNextModel()
		{
			string next = modelSettings.Quotas.Keys
				.FirstOrDefault(m => m != geminiModel &&
					(usageCounts.TryGetValue(m, out var c) ? c : 0) < modelSettings.QuotaFor(m, defaultQuota));
			if (next == null) return false;

			int used = usageCounts.TryGetValue(next, out var u) ? u : 0;
			int quota = modelSettings.QuotaFor(next, defaultQuota);
			var r = MessageBox.Show(this,
				$"{geminiModel} は本日の上限（{dailyQuota}回）に達しました。" + Environment.NewLine +
				$"{next}（本日 {used}/{quota}）に切り替えて続けますか？" + Environment.NewLine + Environment.NewLine +
				"※モデルが変わると、回答の質や傾向が変わることがあります。",
				"モデルの切り替え", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (r != DialogResult.Yes) return false;

			geminiModel = next;
			modelSettings.Model = next;
			modelSettings.Save();
			UpdateQuotaLabel();
			RefreshModelCombo();
			return true;
		}

		private void ConsumeRequest()
		{
			usageCountToday++;
			SaveUsageState();
			UpdateQuotaLabel();
		}

		// ====== 厳格抽出モード用：JSONスキーマ ======
		private class StrictCitation
		{
			public string file { get; set; }
			public int page { get; set; }
			public string quote { get; set; }
		}

		private class StrictOutput
		{
			public List<StrictCitation> citations { get; set; }
		}

		// ====== ヘルパー：JSON抽出 ======
		private static string TryExtractJson(string s)
		{
			if (string.IsNullOrWhiteSpace(s)) return null;
			s = s.Trim();

			// コードフェンス ```json ... ``` に対応
			var fenceRegex = GetOrCreateRegex("```json\\s*(\\{[\\s\\S]*?\\})\\s*```");
			var m = fenceRegex.Match(s);
			if (m.Success) return m.Groups[1].Value;

			// 最初の '{' から最後の '}' までを大雑把に抽出
			int i = s.IndexOf('{');
			int j = s.LastIndexOf('}');
			if (i >= 0 && j >= i) return s.Substring(i, j - i + 1);

			return null;
		}

		private static string NormalizeForSearch(string x)
		{
			if (x == null) return "";
			// 空白系を単一化してノイズに強く
			x = x.Replace('\u00A0', ' ');
			var spaceRegex = GetOrCreateRegex(@"[ \t]+");
			x = spaceRegex.Replace(x, " ");
			var lineRegex = GetOrCreateRegex(@"\r\n|\r|\n");
			x = lineRegex.Replace(x, "\n");
			return x;
		}

		// ====== 正規表現キャッシュ（パフォーマンス改善） ======
		private static Regex GetOrCreateRegex(string pattern, RegexOptions options = RegexOptions.None)
		{
			var key = $"{pattern}|{options}";
			return RegexCache.GetOrAdd(key, k => new Regex(pattern, options | RegexOptions.Compiled));
		}

		// 引用が文脈に完全一致で含まれるか
		private static bool QuoteExistsInContext(string quote, string context)
		{
			if (string.IsNullOrWhiteSpace(quote)) return false;
			quote = NormalizeForSearch(quote).Trim();
			context = NormalizeForSearch(context);
			return context.Contains(quote);
		}

		// 文脈を「### <file> p.<n>」単位に分割
		private static Dictionary<(string file, int page), string> SplitSections(string combined)
		{
			var dict = new Dictionary<(string, int), string>();
			if (string.IsNullOrEmpty(combined)) return dict;

			var lines = combined.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
			string curFile = null;
			int curPage = -1;
			var sb = new StringBuilder();

			void Flush()
			{
				if (curFile != null && curPage > 0)
					dict[(curFile, curPage)] = sb.ToString();
				sb.Clear();
			}

			var headingRegex = GetOrCreateRegex(@"^###\s+(.+?)\s+p\.(\d+)\s*$");
			foreach (var line in lines)
			{
				var m = headingRegex.Match(line);
				if (m.Success)
				{
					Flush();
					curFile = m.Groups[1].Value.Trim();
					if (int.TryParse(m.Groups[2].Value, out var pageNum))
					{
						curPage = pageNum;
					}
				}
				else
				{
					sb.AppendLine(line);
				}
			}
			Flush();
			return dict;
		}

		// ====== 厳格表示のビルド：検証を通ったものだけを表示 ======
		private string BuildStrictAnswerFromCitations(string combinedContent, StrictOutput o)
		{
			if (o?.citations == null || o.citations.Count == 0)
				return "■ 結論\n文書に記載されていません。\n\n■ 根拠\n（該当引用なし）";

			var sections = SplitSections(combinedContent);

			var valid = new List<StrictCitation>();
			foreach (var c in o.citations)
			{
				// 入力検証強化
				if (string.IsNullOrWhiteSpace(c.file) || c.page <= 0 || string.IsNullOrWhiteSpace(c.quote))
					continue;

				// 引用の長さ制限（異常に長い引用を防ぐ）
				if (c.quote.Length > 1000)
					continue;

				if (sections.TryGetValue((c.file, c.page), out var sectionText) &&
					QuoteExistsInContext(c.quote, sectionText))
				{
					valid.Add(c);
				}
			}

			if (valid.Count == 0)
				return "■ 結論\n文書に記載されていません。\n\n■ 根拠\n（該当引用なし）";

			var sb = new StringBuilder();
			sb.AppendLine("■ 結論");
			sb.AppendLine("以下は文書からの“抜き出し引用”のみを表示します。推測・要約は含みません。");
			sb.AppendLine();
			sb.AppendLine("■ 根拠（引用）");
			foreach (var c in valid)
				sb.AppendLine($"- <{c.file} p.{c.page}>: \"{c.quote}\"");
			sb.AppendLine();
			sb.AppendLine("■ 注意点/不明点");
			sb.AppendLine("- 上記に含まれない内容は文書で未確認のため表示しません。");
			return sb.ToString();
		}

		// ====== PDF選択 → 読み込み ======
		private void btnSelectPDF_Click(object sender, EventArgs e)
		{
			ResetContinue();
			using (var dialog = new OpenFileDialog())
			{
				dialog.Filter = "PDF Files (*.pdf)|*.pdf";
				dialog.Multiselect = true;
				dialog.Title = "PDFファイルを選択してください";

				if (dialog.ShowDialog() == DialogResult.OK)
				{
					// ファイルサイズチェック（セキュリティ強化）
					var oversizedFiles = new List<string>();
					const long maxFileSize = 50 * 1024 * 1024; // 50MB制限

					foreach (string filePath in dialog.FileNames)
					{
						try
						{
							var fileInfo = new FileInfo(filePath);
							if (fileInfo.Length > maxFileSize)
							{
								oversizedFiles.Add(Path.GetFileName(filePath));
							}
						}
						catch (Exception)
						{
							// ファイル情報取得失敗は無視
						}
					}

					if (oversizedFiles.Count > 0)
					{
						MessageBox.Show($"以下のファイルはサイズが大きすぎます（50MB制限）:\n{string.Join("\n", oversizedFiles)}",
							"ファイルサイズエラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
						return;
					}

					pdfFilePaths.Clear();
					pdfContents.Clear();
					listBoxPDFs.Items.Clear();

					foreach (string filePath in dialog.FileNames)
					{
						pdfFilePaths.Add(filePath);
						listBoxPDFs.Items.Add(Path.GetFileName(filePath));
					}

					btnLoadPDFs.Enabled = true;
				}
			}
		}

		private async void btnLoadPDFs_Click(object sender, EventArgs e)
		{
			if (pdfFilePaths.Count == 0)
			{
				MessageBox.Show("PDFファイルが選択されていません。", "エラー",
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			if (progressBar != null)
			{
				progressBar.Visible = true;
				progressBar.Value = 0;
				progressBar.Maximum = pdfFilePaths.Count;
			}
			btnLoadPDFs.Enabled = false;

			try
			{
				pdfContents.Clear();

				for (int i = 0; i < pdfFilePaths.Count; i++)
				{
					string content = await Task.Run(() => ExtractTextFromPDF(pdfFilePaths[i]));
					pdfContents.Add(content);
					if (progressBar != null)
						progressBar.Value = i + 1;
					await Task.Delay(1); // UI更新のための小さな遅延
				}

				MessageBox.Show($"{pdfFilePaths.Count}個のPDFファイルを読み込みました。",
					"完了", MessageBoxButtons.OK, MessageBoxIcon.Information);

				btnAskQuestion.Enabled = true;
			}
			catch (Exception ex)
			{
				MessageBox.Show($"PDFの読み込み中にエラーが発生しました: {ex.Message}",
					"エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				if (progressBar != null)
					progressBar.Visible = false;
				btnLoadPDFs.Enabled = true;
			}
		}

		// ====== PDFページごとのテキスト抽出（見出しを付ける） ======
		private static string NormalizeText(string s)
		{
			if (string.IsNullOrEmpty(s)) return string.Empty;
			s = s.Replace('\u00A0', ' ');
			var spaceRegex = GetOrCreateRegex(@"[ \t]+");
			s = spaceRegex.Replace(s, " ");
			var lineRegex = GetOrCreateRegex(@"\r\n|\r|\n");
			s = lineRegex.Replace(s, "\n");
			return s.Trim();
		}

		private string ExtractTextFromPDF(string filePath)
		{
			var sb = new StringBuilder();
			try
			{
				using (var reader = new PdfReader(filePath))
				{
					for (int page = 1; page <= reader.NumberOfPages; page++)
					{
						var strategy = new LocationTextExtractionStrategy();
						string pageText = PdfTextExtractor.GetTextFromPage(reader, page, strategy);
						pageText = NormalizeText(pageText);

						sb.AppendLine($"### {Path.GetFileName(filePath)} p.{page}");
						sb.AppendLine(pageText);
						sb.AppendLine();
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"PDFファイル '{Path.GetFileName(filePath)}' の読み込みに失敗しました: {ex.Message}");
			}
			return sb.ToString();
		}


		private string NormalizeToMarkdown(string s)
		{
			if (string.IsNullOrWhiteSpace(s)) return "（内容がありません）";
			s = s.Replace("\r\n", "\n").Replace("\r", "\n");
			s = s.Replace("■ 結論", "### 結論")
				 .Replace("■ 根拠", "### 根拠")
				 .Replace("■ 注意点/不明点", "### 注意点/不明点");
			return s;
		}

		// ========== Markdown風 → RichTextBox レンダラ（外部ライブラリなし） ==========
		private void RenderAnswerToRichText(string rawAnswer, bool useStrict)
		{
			// 0) ベース設定
			var baseFont = ritAnswer.Font;
			var monoFont = new System.Drawing.Font("Consolas", Math.Max(9f, baseFont.Size - 1), System.Drawing.FontStyle.Regular);
			var h1 = new System.Drawing.Font(baseFont.FontFamily, baseFont.Size + 4, System.Drawing.FontStyle.Bold);
			var h2 = new System.Drawing.Font(baseFont.FontFamily, baseFont.Size + 2, System.Drawing.FontStyle.Bold);
			var h3 = new System.Drawing.Font(baseFont.FontFamily, baseFont.Size + 1, System.Drawing.FontStyle.Bold);

			ritAnswer.SuspendLayout();
			ritAnswer.Clear();
			ritAnswer.ReadOnly = false;
			ritAnswer.DetectUrls = true; // 回答に含まれるURLをクリックで開けるようにする
			ritAnswer.SelectionIndent = 0;
			ritAnswer.SelectionHangingIndent = 0;
			ritAnswer.SelectionRightIndent = 0;

			// 1) "■ 見出し" を Markdown風に正規化
			string md = NormalizeToMarkdown(rawAnswer ?? string.Empty);

			// 2) 行単位パース（簡易Markdown）
			var lines = md.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
			bool inCodeBlock = false;
			var codeBuf = new StringBuilder();
			var listBuf = new List<string>();
			bool inList = false;

			void FlushList()
			{
				if (!inList || listBuf.Count == 0) return;
				AppendBulletList(listBuf, baseFont, monoFont);
				listBuf.Clear();
				inList = false;
			}

			void FlushCode()
			{
				if (!inCodeBlock) return;
				AppendCodeBlock(codeBuf.ToString().TrimEnd('\n'), monoFont);
				codeBuf.Clear();
				inCodeBlock = false;
			}

			for (int i = 0; i < lines.Length; i++)
			{
				string line = lines[i];

				// 2-1) フェンスドコード
				if (IsFenceStartOrEnd(line))
				{
					if (!inCodeBlock)
					{
						FlushList();
						inCodeBlock = true;
						continue;
					}
					else
					{
						FlushCode();
						continue;
					}
				}

				if (inCodeBlock)
				{
					codeBuf.AppendLine(line);
					continue;
				}

				// 2-2) 水平線
				var hrRegex = GetOrCreateRegex(@"^---+$");
				if (hrRegex.IsMatch(line.Trim()))
				{
					FlushList();
					AppendHr(baseFont);
					continue;
				}

				// 2-3) 見出し
				var h1Regex = GetOrCreateRegex(@"^\s*#\s+");
				var h2Regex = GetOrCreateRegex(@"^\s*##\s+");
				var h3Regex = GetOrCreateRegex(@"^\s*###\s+");

				if (h1Regex.IsMatch(line))
				{
					FlushList();
					AppendHeading(line.Replace("#", "").Trim(), h1);
					continue;
				}
				if (h2Regex.IsMatch(line))
				{
					FlushList();
					AppendHeading(line.Trim().TrimStart('#').Trim(), h2);
					continue;
				}
				if (h3Regex.IsMatch(line))
				{
					FlushList();
					AppendHeading(line.Trim().TrimStart('#').Trim(), h3);
					continue;
				}

				// 2-4) 箇条書き
				var bulletRegex = GetOrCreateRegex(@"^(-|\*)\s+");
				if (bulletRegex.IsMatch(line.TrimStart()))
				{
					if (!inList) { inList = true; listBuf.Clear(); }
					var item = bulletRegex.Replace(line.TrimStart(), "");
					listBuf.Add(item);
					continue;
				}
				else
				{
					FlushList();
				}

				// 2-5) 引用
				var quoteRegex = GetOrCreateRegex(@"^>\s+");
				if (quoteRegex.IsMatch(line.TrimStart()))
				{
					var quoted = quoteRegex.Replace(line.TrimStart(), "");
					AppendBlockquote(quoted, baseFont);
					continue;
				}

				// 2-6) 空行
				if (string.IsNullOrWhiteSpace(line))
				{
					ritAnswer.AppendText(Environment.NewLine);
					continue;
				}

				// 2-7) 通常段落
				AppendParagraphWithInline(line, baseFont, monoFont);
			}

			FlushList();
			FlushCode();

			ritAnswer.ReadOnly = true;
			ritAnswer.SelectionStart = 0;
			ritAnswer.ScrollToCaret();
			ritAnswer.ResumeLayout();
		}

		// ========== ユーティリティ群 ==========
		private static bool IsFenceStartOrEnd(string line)
		{
			if (line == null) return false;
			var t = line.Trim();
			return t.StartsWith("```");
		}

		private void AppendHeading(string text, System.Drawing.Font font)
		{
			ritAnswer.SelectionFont = font;
			ritAnswer.SelectionColor = System.Drawing.Color.FromArgb(20, 23, 26);
			ritAnswer.AppendText(text + Environment.NewLine);
			ritAnswer.AppendText(Environment.NewLine);
		}

		private void AppendHr(System.Drawing.Font baseFont)
		{
			ritAnswer.SelectionFont = baseFont;
			ritAnswer.SelectionColor = System.Drawing.Color.FromArgb(180, 180, 180);
			ritAnswer.AppendText(new string('─', 36) + Environment.NewLine + Environment.NewLine);
			ritAnswer.SelectionColor = System.Drawing.Color.FromArgb(33, 37, 41);
		}

		private void AppendBulletList(List<string> items, System.Drawing.Font baseFont, System.Drawing.Font monoFont)
		{
			ritAnswer.SelectionIndent = 10;
			ritAnswer.SelectionHangingIndent = 10;
			ritAnswer.SelectionBullet = true;

			foreach (var raw in items)
			{
				AppendParagraphWithInline(raw, baseFont, monoFont);
			}

			ritAnswer.SelectionBullet = false;
			ritAnswer.SelectionIndent = 0;
			ritAnswer.SelectionHangingIndent = 0;
			ritAnswer.AppendText(Environment.NewLine);
		}

		private void AppendBlockquote(string text, System.Drawing.Font baseFont)
		{
			ritAnswer.SelectionIndent = 12;
			ritAnswer.SelectionColor = System.Drawing.Color.FromArgb(90, 99, 110);
			ritAnswer.SelectionBackColor = System.Drawing.Color.FromArgb(245, 247, 250);
			ritAnswer.SelectionFont = new System.Drawing.Font(baseFont, System.Drawing.FontStyle.Italic);

			ritAnswer.AppendText(text + Environment.NewLine + Environment.NewLine);

			ritAnswer.SelectionIndent = 0;
			ritAnswer.SelectionColor = System.Drawing.Color.FromArgb(33, 37, 41);
			ritAnswer.SelectionBackColor = ritAnswer.BackColor;
			ritAnswer.SelectionFont = baseFont;
		}

		private void AppendCodeBlock(string code, System.Drawing.Font monoFont)
		{
			ritAnswer.SelectionBackColor = System.Drawing.Color.FromArgb(245, 245, 245);
			ritAnswer.SelectionColor = System.Drawing.Color.FromArgb(50, 56, 62);
			ritAnswer.SelectionFont = monoFont;

			ritAnswer.SelectionIndent = 10;
			ritAnswer.AppendText(code.Replace("\t", "    ") + Environment.NewLine);
			ritAnswer.AppendText(Environment.NewLine);

			ritAnswer.SelectionIndent = 0;
			ritAnswer.SelectionBackColor = ritAnswer.BackColor;
			ritAnswer.SelectionColor = System.Drawing.Color.FromArgb(33, 37, 41);
		}

		private void AppendParagraphWithInline(string raw, System.Drawing.Font baseFont, System.Drawing.Font monoFont)
		{
			// インライン code の処理
			var parts = new List<(string text, bool code)>();
			int idx = 0;
			while (idx < (raw ?? "").Length)
			{
				int s = raw.IndexOf('`', idx);
				if (s < 0)
				{
					parts.Add((raw.Substring(idx), false));
					break;
				}
				int e = raw.IndexOf('`', s + 1);
				if (e < 0)
				{
					parts.Add((raw.Substring(idx), false));
					break;
				}
				if (s > idx) parts.Add((raw.Substring(idx, s - idx), false));
				parts.Add((raw.Substring(s + 1, e - s - 1), true));
				idx = e + 1;
			}
			if (parts.Count == 0) parts.Add((raw ?? "", false));

			// 各パートを描画（bold/italic）
			foreach (var p in parts)
			{
				if (p.code)
				{
					ritAnswer.SelectionFont = monoFont;
					ritAnswer.SelectionBackColor = System.Drawing.Color.FromArgb(245, 245, 245);
					ritAnswer.SelectionColor = System.Drawing.Color.FromArgb(50, 56, 62);
					ritAnswer.AppendText(p.text);
					ritAnswer.SelectionBackColor = ritAnswer.BackColor;
					ritAnswer.SelectionColor = System.Drawing.Color.FromArgb(33, 37, 41);
					ritAnswer.SelectionFont = baseFont;
					continue;
				}

				string t = p.text ?? "";
				int pos = 0;
				while (pos < t.Length)
				{
					int iBold = IndexOfOrdinal(t, "**", pos);
					int iItal = t.IndexOf('*', pos);

					int next = -1;
					bool isBold = false;
					if (iBold >= 0 && (iItal < 0 || iBold <= iItal))
					{
						next = iBold;
						isBold = true;
					}
					else if (iItal >= 0)
					{
						next = iItal;
						isBold = false;
					}

					if (next < 0)
					{
						if (pos < t.Length)
						{
							ritAnswer.SelectionFont = baseFont;
							ritAnswer.AppendText(t.Substring(pos));
						}
						break;
					}

					if (next > pos)
					{
						ritAnswer.SelectionFont = baseFont;
						ritAnswer.AppendText(t.Substring(pos, next - pos));
					}

					if (isBold)
					{
						int close = t.IndexOf("**", next + 2, StringComparison.Ordinal);
						if (close >= 0)
						{
							string inner = t.Substring(next + 2, close - (next + 2));
							ritAnswer.SelectionFont = new System.Drawing.Font(baseFont, System.Drawing.FontStyle.Bold);
							ritAnswer.AppendText(inner);
							pos = close + 2;
						}
						else
						{
							ritAnswer.SelectionFont = baseFont;
							ritAnswer.AppendText(t.Substring(next));
							break;
						}
					}
					else
					{
						if (next + 1 < t.Length && t[next + 1] == '*')
						{
							ritAnswer.SelectionFont = baseFont;
							ritAnswer.AppendText("*");
							pos = next + 1;
						}
						else
						{
							int close = t.IndexOf('*', next + 1);
							if (close >= 0)
							{
								string inner = t.Substring(next + 1, close - (next + 1));
								ritAnswer.SelectionFont = new System.Drawing.Font(baseFont, System.Drawing.FontStyle.Italic);
								ritAnswer.AppendText(inner);
								pos = close + 1;
							}
							else
							{
								ritAnswer.SelectionFont = baseFont;
								ritAnswer.AppendText(t.Substring(next));
								break;
							}
						}
					}
				}
				ritAnswer.SelectionFont = baseFont;
			}
			ritAnswer.AppendText(Environment.NewLine);
		}

		// .NET Framework互換のIndexOf
		private static int IndexOfOrdinal(string s, string value, int startIndex)
		{
			if (s == null || value == null) return -1;
			if (startIndex < 0) startIndex = 0;
			int count = s.Length - startIndex;
			if (count <= 0) return -1;
			return s.IndexOf(value, startIndex, count, StringComparison.Ordinal);
		}

		// ====== 続けて質問する（直前の1往復だけ引き継ぐ） ======
		private string lastQuestionForContinue;
		private string lastAnswerForContinue;

		private bool ContinueOn =>
			chkContinue != null && chkContinue.Checked && !string.IsNullOrEmpty(lastQuestionForContinue);

		private void ResetContinue()
		{
			lastQuestionForContinue = null;
			lastAnswerForContinue = null;
		}

		// 前の質問の言葉も使って、PDFから関連ページを探す
		private string RetrievalQuery(string question)
		{
			return ContinueOn ? lastQuestionForContinue + " " + question : question;
		}

		// AIに送る「直前のやりとり」。厳格モードは、引用をPDFからだけ取るので、前の質問だけを渡す
		private string HistoryBlock(bool includeAnswer)
		{
			if (!ContinueOn) return "";
			var sb = new StringBuilder();
			sb.AppendLine("【直前のやりとり（今の質問が「その」「それ」など、直前の内容を指しているときだけ参考にする）】");
			sb.AppendLine("前の質問: " + lastQuestionForContinue);
			if (includeAnswer && !string.IsNullOrWhiteSpace(lastAnswerForContinue))
				sb.AppendLine("前の回答: " + Truncate(lastAnswerForContinue, 1500));
			sb.AppendLine();
			return sb.ToString();
		}

		// ====== PDFが使える状態か（「PDFなし質問」のときは不要） ======
		private bool IsPdfReady()
		{
			return (chkNoPdf != null && chkNoPdf.Checked) || (pdfFilePaths.Count > 0 && pdfContents.Count > 0);
		}

		// 使えないときは、状況に合わせたメッセージを出して false を返す
		private bool EnsurePdfReady()
		{
			if (chkNoPdf != null && chkNoPdf.Checked) return true;
			if (pdfFilePaths.Count == 0)
			{
				MessageBox.Show(this, "PDFが選択されていません。" + Environment.NewLine +
					"先に「① PDFファイル選択」でPDFを選んでください。",
					"PDFが未選択", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return false;
			}
			if (pdfContents.Count == 0)
			{
				MessageBox.Show(this, "PDFが読み込まれていません。" + Environment.NewLine +
					"「② PDF読み込み」を押してください。",
					"PDFが未読込", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return false;
			}
			return true;
		}

		// ====== 質問クリック（クォータ確認→実行） ======
		private async void btnAskQuestion_Click(object sender, EventArgs e)
		{
			// 入力検証
			if (string.IsNullOrEmpty(geminiApiKey))
			{
				MessageBox.Show("Gemini API Keyが設定されていません。", "エラー",
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}
			if (!EnsurePdfReady()) return;

			string question = txtQuestion.Text?.Trim() ?? "";
			if (string.IsNullOrEmpty(question))
			{
				MessageBox.Show("質問を入力してください。", "エラー",
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			// 質問の長さ制限（セキュリティ強化）
			if (question.Length > 1000)
			{
				MessageBox.Show("質問が長すぎます（1000文字以内にしてください）。", "入力エラー",
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			// 残り回数チェック
			if (!CanConsumeRequest() && !OfferSwitchToNextModel())
			{
				MessageBox.Show("本日のリクエスト上限に達しています（PT基準）。他に使えるモデルもありません。時間を空けて再度お試しください。",
					"レート制限", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			ConsumeRequest();

			btnAskQuestion.Enabled = false;
			ritAnswer.Text = "回答を生成中...";

			try
			{
				bool useStrict = (chkStrictMode != null) ? chkStrictMode.Checked : strictExtractiveMode;
				if (tsslRate != null)
					tsslRate.Text = (chkNoPdf != null && chkNoPdf.Checked) ? "PDFなし質問モード" : $"厳格モード: {(useStrict ? "ON（引用のみ）" : "OFF（要約OK）")}";
				if (ContinueOn && tsslRate != null) tsslRate.Text += "　／直前の質問を引き継ぎ中";

				bool noPdf = chkNoPdf != null && chkNoPdf.Checked;
				lastAnswerWasNoPdf = noPdf;
					string answer = noPdf
						? await GetGeneralAnswer(question)
						: await GetAnswerFromGemini(question, useStrict);
					lastQuestionForContinue = question;
					lastAnswerForContinue = answer;
				RenderAnswerToRichText(answer, useStrict);
				AppendLiteNoteIfNeeded();
				if (chkAutoRead != null && chkAutoRead.Checked)
					_ = ReadAnswerAloudAsync(false);
			}
			catch (HttpRequestException ex)
			{
				ritAnswer.Text = $"ネットワークエラーが発生しました: {ex.Message}";
			}
			catch (TaskCanceledException ex)
			{
				ritAnswer.Text = $"タイムアウトエラー: {ex.Message}";
			}
			catch (Newtonsoft.Json.JsonException ex)
			{
				ritAnswer.Text = $"レスポンス解析エラー: {ex.Message}";
			}
			catch (Exception ex)
			{
				ritAnswer.Text = $"エラーが発生しました: {ex.Message}";
			}
			finally
			{
				btnAskQuestion.Enabled = true;
				if (tsslRate != null)
					tsslRate.Text = "";
			}
		}

		// ====== App.config の設定を反映（無い・不正な値は既定値のまま） ======
		private void LoadAppConfig()
		{
			try
			{
				var cfg = ConfigurationManager.AppSettings;
				var model = cfg["GeminiModel"];
				if (!string.IsNullOrWhiteSpace(model)) geminiModel = model.Trim();
				// 「モデル設定」で選んだモデルを優先する
				if (!string.IsNullOrWhiteSpace(modelSettings.Model)) geminiModel = modelSettings.Model;

				if (int.TryParse(cfg["DailyRequestQuota"], out var q) && q > 0) defaultQuota = q;
				if (int.TryParse(cfg["RetryMaxAttempts"], out var r) && r >= 1) retryMaxAttempts = r;
				if (int.TryParse(cfg["RetryBaseDelayMs"], out var d) && d >= 0) retryBaseDelayMs = d;
				if (double.TryParse(cfg["RetryBackoffFactor"], System.Globalization.NumberStyles.Float,
						System.Globalization.CultureInfo.InvariantCulture, out var f) && f >= 1.0)
					retryBackoffFactor = f;
			}
			catch (ConfigurationErrorsException ex)
			{
				System.Diagnostics.Debug.WriteLine("App.config 読込失敗: " + ex.Message);
			}
		}

		// ====== 質問タイプ別パターン（qa_patterns.json・任意） ======
		private class QaPatterns
		{
			public Dictionary<string, List<string>> keywords { get; set; }
			public Dictionary<string, List<string>> entities { get; set; }
			public List<string> file_boost { get; set; }
			public List<string> file_penalty { get; set; }
		}

		private QaPatterns qaPatterns;
		private readonly Dictionary<string, List<Regex>> qaEntityRegexes = new Dictionary<string, List<Regex>>();

		private void LoadQaPatterns()
		{
			try
			{
				var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "qa_patterns.json");
				if (!File.Exists(path)) return;
				var loaded = JsonConvert.DeserializeObject<QaPatterns>(File.ReadAllText(path, Encoding.UTF8));
				if (loaded == null) return;

				qaEntityRegexes.Clear();
				if (loaded.entities != null)
				{
					foreach (var kv in loaded.entities)
					{
						var list = new List<Regex>();
						foreach (var pat in kv.Value ?? new List<string>())
						{
							try { list.Add(new Regex(pat, RegexOptions.Compiled, TimeSpan.FromMilliseconds(200))); }
							catch (ArgumentException) { /* 不正な正規表現は無視 */ }
						}
						qaEntityRegexes[kv.Key] = list;
					}
				}
				qaPatterns = loaded;
			}
			catch (Exception ex)
			{
				// パターン無しでも動作する（検索は通常のBM25風のみ）
				System.Diagnostics.Debug.WriteLine("qa_patterns.json 読込失敗: " + ex.Message);
				qaPatterns = null;
			}
		}

		// 質問タイプ（金額・日付など）に合うページ／ファイル名を加点・減点する
		private double PatternBonus(string block, string question)
		{
			if (qaPatterns == null) return 0.0;
			double bonus = 0.0;

			if (qaPatterns.keywords != null)
			{
				foreach (var kv in qaPatterns.keywords)
				{
					if (!kv.Value.Any(k => question.Contains(k))) continue;
					if (!qaEntityRegexes.TryGetValue(kv.Key, out var regs)) continue;
					foreach (var r in regs)
					{
						try
						{
							if (r.IsMatch(block)) { bonus += 1.0; break; }
						}
						catch (RegexMatchTimeoutException) { }
					}
				}
			}

			if (TryParseBlockHeader(block, out var file, out _))
			{
				if (qaPatterns.file_boost != null && qaPatterns.file_boost.Any(w => file.Contains(w)))
					bonus += 0.5;
				if (qaPatterns.file_penalty != null &&
					qaPatterns.file_penalty.Any(w => file.Contains(w) && !question.Contains(w)))
					bonus -= 0.5;
			}
			return bonus;
		}

		// ====== BM25風：問い合わせ語抽出 ======
		//質問から検索クエリ語を作成（英数字=大小無視、日本語=バイグラム）
		private static List<string> ExtractQueryTerms(string q)
		{
			var terms = new List<string>();

			// 英数字（2文字以上）
			var englishRegex = GetOrCreateRegex(@"[A-Za-z0-9]{2,}");
			foreach (Match m in englishRegex.Matches(q))
				terms.Add(m.Value.ToLowerInvariant());

			// CJK文字のみ抽出してバイグラム
			var cjkRegex = GetOrCreateRegex(@"[^\p{IsHiragana}\p{IsKatakana}\p{IsCJKUnifiedIdeographs}]");
			string jp = cjkRegex.Replace(q, "");
			if (jp.Length >= 2)
			{
				for (int i = 0; i < jp.Length - 1; i++)
					terms.Add(jp.Substring(i, 2));
			}

			return terms.Distinct().ToList();
		}

		// ====== BM25風：文脈構築補助 ======
		//ページ見出し ### <file> p.<n> ごとにページ単位ブロックへ分割（検索の単位づくり）
		private static List<string> SplitBlocks(IEnumerable<string> contentsJoined)
		{
			var all = string.Join("\n\n", contentsJoined ?? Enumerable.Empty<string>());
			var lines = all.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');

			var blocks = new List<string>();
			var headingRegex = GetOrCreateRegex(@"^###\s+.+?\s+p\.\d+\s*$");
			var sb = new StringBuilder();

			foreach (var line in lines)
			{
				if (headingRegex.IsMatch(line))
				{
					if (sb.Length > 0) { blocks.Add(sb.ToString()); sb.Clear(); }
					sb.AppendLine(line);
				}
				else
				{
					sb.AppendLine(line);
				}
			}
			if (sb.Length > 0) blocks.Add(sb.ToString());
			return blocks;
		}


		private static int CountTerm(string text, string term)
		{
			if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(term)) return 0;

			// 英数字の場合は大小文字を無視
			var isEnglish = GetOrCreateRegex(@"^[A-Za-z0-9]+$");
			if (isEnglish.IsMatch(term))
			{
				var regex = GetOrCreateRegex(Regex.Escape(term), RegexOptions.IgnoreCase);
				return regex.Matches(text).Count;
			}

			// 日本語等はそのまま
			var exactRegex = GetOrCreateRegex(Regex.Escape(term));
			return exactRegex.Matches(text).Count;
		}


		// BM25風スコアリング（改良版：計算最適化）
		private static double ScoreBlockBM25ish(
			string block,
			List<string> terms,
			Dictionary<string, int> docFreq,
			int docCount,
			double avgLen)
		{
			if (terms.Count == 0) return 0.0;

			const double k1 = 1.2;
			const double b = 0.75;

			double len = Math.Max(1, block.Length);
			double score = 0.0;

			foreach (var t in terms)
			{
				//int tf = CountTerm(block, t);
				int tf = CountTermFast(block, t);   // ← 置換（CountTerm → CountTermFast）／ここを修正
				if (tf == 0) continue;

				int df = docFreq.TryGetValue(t, out var d) ? d : 0;

				// idf計算の安全性向上
				double idf = Math.Log(Math.Max(0.1, (docCount - df + 0.5) / (df + 0.5)) + 1.0);
				double denom = tf + k1 * (1.0 - b + b * (len / Math.Max(1.0, avgLen)));
				score += idf * ((tf * (k1 + 1.0)) / denom);
			}

			// 長さボーナス（控えめ）
			score += Math.Min(len / 1000.0, 1.5);
			return score;
		}


		private static bool TryParseBlockHeader(string block, out string file, out int page)
		{
			file = null; page = -1;
			if (string.IsNullOrEmpty(block)) return false;

			var firstLine = block.Replace("\r\n", "\n").Replace("\r", "\n")
								 .Split('\n').FirstOrDefault() ?? "";
			var m = GetOrCreateRegex(@"^###\s+(.+?)\s+p\.(\d+)\s*$").Match(firstLine);
			if (!m.Success) return false;

			file = m.Groups[1].Value.Trim();
			return int.TryParse(m.Groups[2].Value, out page);
		}

		/// <summary>
		/// 用語出現回数を高速に数える（英数字は大小無視、CJK等は厳密）
		/// Regex.Matches よりも低コストにするため IndexOf を反復使用
		/// </summary>
		private static int CountTermFast(string text, string term)
		{
			if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(term)) return 0;

			// 英数字のみの語なら、大文字小文字を無視して検索（OrdinalIgnoreCase）
			bool ascii = AsciiRegex.IsMatch(term);

			int count = 0;
			int index = 0;
			while (true)
			{
				index = ascii
					? text.IndexOf(term, index, StringComparison.OrdinalIgnoreCase)
					: text.IndexOf(term, index, StringComparison.Ordinal); // 日本語等は厳密一致
				if (index < 0) break;
				count++;
				index += term.Length; // オーバーラップなし
			}
			return count;
		}

		// 文脈構築（改良版：メモリ効率向上）
		//検索ロジックそのものは BuildContext と
		//ScoreBlockBM25ish（＋ CountTermFast/CountTerm）のセットです。
		//それ以外（ExtractTextFromPDF, GetStrictAnswer, GetNormalAnswer など）は
		//抽出や表示・API通信で、検索の判断はしていません。
		private string BuildContext(string question, int maxChars = 28000, int neighborRadius = 1)
		{
			var blocks = SplitBlocks(pdfContents);
			if (blocks.Count == 0) return string.Empty;

			var terms = ExtractQueryTerms(question);

			if (terms.Count == 0)
			{
				var sb0 = new StringBuilder();
				foreach (var blk in blocks)
				{
					if (sb0.Length + blk.Length + 2 > maxChars) break;
					sb0.AppendLine(blk);
					sb0.AppendLine();
				}
				return sb0.ToString();
			}

			int N = blocks.Count;
			double avgLen = blocks.Average(b => (double)b.Length);

			// 文書頻度計算
			var docFreq = new Dictionary<string, int>();
			foreach (var t in terms)
			{
				int df = 0;
				foreach (var blk in blocks)
					if (CountTermFast(blk, t) > 0) df++;    // ← 置換（CountTerm → CountTermFast）／ここを修正
				docFreq[t] = df;
			}

			//スコアリング（BM25風）＋並べ替え ScoreBlockBM25ish が検索ランキングの心臓部
			var scored = blocks
				.Select(b => new { Block = b, Score = ScoreBlockBM25ish(b, terms, docFreq, N, avgLen) + PatternBonus(b, question) })
				.OrderByDescending(x => x.Score)
				.ThenBy(x => x.Block.Length)
				.ToList();

			// (file,page) → block の辞書
			var index = new Dictionary<(string file, int page), string>();
			foreach (var b in blocks)
				if (TryParseBlockHeader(b, out var f, out var p))
					index[(f, p)] = b;

			var added = new HashSet<(string file, int page)>();
			var sb = new StringBuilder();

			void TryAdd(string f, int p)
			{
				var key = (f, p);
				if (added.Contains(key)) return;
				if (index.TryGetValue(key, out var blk))
				{
					if (sb.Length + blk.Length + 2 > maxChars) return;
					sb.AppendLine(blk);
					sb.AppendLine();
					added.Add(key);
				}
			}

			foreach (var item in scored)
			{
				if (!TryParseBlockHeader(item.Block, out var f, out var p)) continue;

				for (int d = -neighborRadius; d <= neighborRadius; d++)
				{
					TryAdd(f, p + d);
					if (sb.Length >= maxChars) break;
				}
				if (sb.Length >= maxChars) break;
			}

			if (sb.Length == 0 && pdfContents.Count > 0)
			{
				var all = string.Join("\n\n", pdfContents);
				return all.Substring(0, Math.Min(maxChars, all.Length));
			}

			return sb.ToString();
		}


		// ====== Gemini 呼び出し（改良版：エラーハンドリング強化） ======
		private async Task<string> GetAnswerFromGemini(string question, bool useStrict)
		{
			string combinedContent = BuildContext(RetrievalQuery(question), 28000);
			if (string.IsNullOrWhiteSpace(combinedContent))
				throw new InvalidOperationException("文脈抽出に失敗しました。PDFの読み込み内容をご確認ください。");

			if (useStrict)
			{
				return await GetStrictAnswer(question, combinedContent);
			}
			else
			{
				return await GetNormalAnswer(question, combinedContent);
			}
		}

		private async Task<string> GetStrictAnswer(string question, string combinedContent)
		{
			string prompt = $@"次の「文脈」に含まれるテキストの“連続する一部”だけを引用し、JSONで返してください。
書式は厳密にこれのみ：{{""citations"":[{{""file"":""<ファイル名>"",""page"":<数値>,""quote"":""<引用>""}}, ...]}}
制約：
- quote は「文脈からの連続文字列」をそのまま貼り付けること（言い換え・要約・省略・追加は禁止）
- 文脈に無い内容は一切生成しないこと
- 文脈に該当が無ければ空配列（{{""citations"":[]}}）を返すこと
- JSON以外の文字は出力しないこと

【文脈】
{combinedContent}

{HistoryBlock(false)}【質問】
{question}";

			var requestBody = new
			{
				contents = new[]
				{
					new { parts = new[] { new { text = prompt } } }
				},
				generationConfig = new
				{
					temperature = 0.0,
					maxOutputTokens = 4096
				}
			};

			string jsonReq = JsonConvert.SerializeObject(requestBody);
			string url = $"https://generativelanguage.googleapis.com/v1beta/models/{geminiModel}:generateContent";

			Func<HttpContent> contentFactory = () => new StringContent(jsonReq, Encoding.UTF8, "application/json");

			using (var response = await PostWithRetriesAsync(url, contentFactory))
			{
				string respText = await response.Content.ReadAsStringAsync();
				if (!response.IsSuccessStatusCode)
					throw new HttpRequestException($"API呼び出しエラー: {(int)response.StatusCode} - {Truncate(respText, 800)}");

				var result = JsonConvert.DeserializeObject<GeminiResponse>(respText);
				var rawText = result?.candidates?.FirstOrDefault()
								   ?.content?.parts?.FirstOrDefault()
								   ?.text;

				var jsonOnly = TryExtractJson(rawText ?? "");
				StrictOutput parsed = null;
				if (!string.IsNullOrWhiteSpace(jsonOnly))
				{
					try
					{
						parsed = JsonConvert.DeserializeObject<StrictOutput>(jsonOnly);
					}
					catch (Newtonsoft.Json.JsonException)
					{
						// JSON解析失敗は不回答扱い
					}
				}

				if (parsed?.citations == null || parsed.citations.Count == 0)
					return "■ 結論\n文書に記載されていません。\n\n■ 根拠\n（該当引用なし）";

				return BuildStrictAnswerFromCitations(combinedContent, parsed);
			}
		}

		private async Task<string> GetNormalAnswer(string question, string combinedContent)
		{
			string prompt = $@"あなたは与えられたPDFの内容【のみ】を根拠に日本語で簡潔に回答します。
文書に無いことは「文書に記載されていません」と述べてください。
必ず根拠として、下記の見出し（例：`ファイル名 p.12`）を列挙してください。

【文書断片（重要度順・一部抜粋）】
{combinedContent}

{HistoryBlock(true)}【質問】
{question}

【出力フォーマット】
■ 結論
■ 根拠
- <ファイル名 p.X>: （該当箇所の要約または短い抜粋）
■ 注意点/不明点
";

			var requestBody = new
			{
				contents = new[]
				{
					new { parts = new[] { new { text = prompt } } }
				},
				generationConfig = new
				{
					temperature = 0.3,
					maxOutputTokens = 8192
				}
			};

			string jsonReq = JsonConvert.SerializeObject(requestBody);
			string url = $"https://generativelanguage.googleapis.com/v1beta/models/{geminiModel}:generateContent";

			Func<HttpContent> contentFactory = () => new StringContent(jsonReq, Encoding.UTF8, "application/json");

			using (var response = await PostWithRetriesAsync(url, contentFactory))
			{
				string respText = await response.Content.ReadAsStringAsync();
				if (!response.IsSuccessStatusCode)
					throw new HttpRequestException($"API呼び出しエラー: {(int)response.StatusCode} - {Truncate(respText, 800)}");

				var result = JsonConvert.DeserializeObject<GeminiResponse>(respText);
				var text = result?.candidates?.FirstOrDefault()
							   ?.content?.parts?.FirstOrDefault()
							   ?.text;

				if (!string.IsNullOrWhiteSpace(text))
					return text;

				throw new InvalidOperationException("APIから有効な回答を取得できませんでした。");
			}
		}

		// ====== PDFなしの一般質問（PDFは使わない・根拠は確認できない） ======
		private async Task<string> GetGeneralAnswer(string question)
		{
			string prompt = $@"あなたは日本語で簡潔に答えるアシスタントです。
この質問ではPDFなどの文書は使いません。あなたの一般的な知識で答えてください。
確かでないこと、最新情報が必要なことは、その旨を正直に書いてください。

{HistoryBlock(true)}【質問】
{question}

【出力フォーマット】
■ 結論
■ 補足
■ 注意点/不明点
";

			var requestBody = new
			{
				contents = new[]
				{
					new { parts = new[] { new { text = prompt } } }
				},
				generationConfig = new
				{
					temperature = 0.3,
					maxOutputTokens = 8192
				}
			};

			string jsonReq = JsonConvert.SerializeObject(requestBody);
			string url = $"https://generativelanguage.googleapis.com/v1beta/models/{geminiModel}:generateContent";
			Func<HttpContent> contentFactory = () => new StringContent(jsonReq, Encoding.UTF8, "application/json");

			using (var response = await PostWithRetriesAsync(url, contentFactory))
			{
				string respText = await response.Content.ReadAsStringAsync();
				if (!response.IsSuccessStatusCode)
					throw new HttpRequestException($"API呼び出しエラー: {(int)response.StatusCode} - {Truncate(respText, 800)}");

				var result = JsonConvert.DeserializeObject<GeminiResponse>(respText);
				var text = result?.candidates?.FirstOrDefault()?.content?.parts?.FirstOrDefault()?.text;
				if (string.IsNullOrWhiteSpace(text))
					throw new InvalidOperationException("APIから有効な回答を取得できませんでした。");

				return "> ※PDFを使わない一般回答です。" + "\n\n" + text;
			}
		}

		// ====== リトライ処理（改良版） ======
		private int ParseRetryAfterSeconds(HttpResponseMessage resp)
		{
			if (resp.Headers?.RetryAfter != null)
			{
				if (resp.Headers.RetryAfter.Delta.HasValue)
					return (int)Math.Ceiling(resp.Headers.RetryAfter.Delta.Value.TotalSeconds);
				if (resp.Headers.RetryAfter.Date.HasValue)
				{
					var sec = (int)Math.Ceiling((resp.Headers.RetryAfter.Date.Value - DateTimeOffset.UtcNow).TotalSeconds);
					return Math.Max(0, sec);
				}
			}
			return -1;
		}

		private async Task<HttpResponseMessage> PostWithRetriesAsync(string url, Func<HttpContent> contentFactory)
		{
			int attempts = 0;
			int delayMs = retryBaseDelayMs;

			while (true)
			{
				attempts++;
				using (var content = contentFactory())
				{
					HttpResponseMessage resp = null;
					try
					{
						using (var req = new HttpRequestMessage(HttpMethod.Post, url) { Content = content })
						{
							// APIキーはURLではなくヘッダーで送る（ログ・例外メッセージへの漏えい防止）
							req.Headers.Add("x-goog-api-key", geminiApiKey);
							resp = await Http.SendAsync(req);
						}
						if (resp.IsSuccessStatusCode)
							return resp;

						int code = (int)resp.StatusCode;
						if (code == 429 || code >= 500)
						{
							if (attempts >= retryMaxAttempts)
								return resp;

							int retryAfter = ParseRetryAfterSeconds(resp);
							int waitMs = retryAfter >= 0
										   ? retryAfter * 1000
										   : (int)(delayMs * (1.0 + _rng.NextDouble() * 0.25));

							if (tsslRate != null)
								tsslRate.Text = $"レート制限中：{attempts}/{retryMaxAttempts} 回目リトライまで {waitMs / 1000.0:F1}s 待機…";

							resp.Dispose(); // リトライ前にレスポンスを破棄
							await Task.Delay(waitMs);

							if (retryAfter < 0)
								delayMs = (int)Math.Min(delayMs * retryBackoffFactor, 30000);

							continue;
						}

						return resp;
					}
					catch (Exception)
					{
						resp?.Dispose();

						if (attempts >= retryMaxAttempts)
							throw;

						await Task.Delay(delayMs);
						delayMs = (int)Math.Min(delayMs * retryBackoffFactor, 30000);
					}
				}
			}
		}

		private static string Truncate(string s, int max)
			=> string.IsNullOrEmpty(s) || s.Length <= max ? s : s.Substring(0, max) + "...";

		// ====== 入力補助・クリア ======
		private void txtQuestion_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter && e.Control)
			{
				armSubmitOnEnter = false;
				btnAskQuestion_Click(sender, e);
				e.Handled = true;
				return;
			}

			if (e.KeyCode == Keys.Enter && !e.Shift && !e.Alt)
			{
				// 音声入力中のEnter → 音声入力を止める（改行は入れない）
				if (isListening)
				{
					StopVoiceInput();
					armSubmitOnEnter = true;
					e.Handled = true;
					e.SuppressKeyPress = true;
					return;
				}

				// 音声入力を止めた直後のEnter → AI回答取得へ
				if (armSubmitOnEnter)
				{
					armSubmitOnEnter = false;
					e.Handled = true;
					e.SuppressKeyPress = true;
					if (btnAskQuestion.Enabled)
						btnAskQuestion_Click(sender, e);
					return;
				}
			}
			else if (e.KeyCode != Keys.ShiftKey && e.KeyCode != Keys.ControlKey && e.KeyCode != Keys.Menu &&
					 e.KeyCode != Keys.LWin && e.KeyCode != Keys.RWin)
			{
				// 他のキーを押したら「次のEnterで質問」は取り消す
				armSubmitOnEnter = false;
			}
		}

		// 音声入力（Windows の Win+H）を止める。Win+H は開閉のトグルなので、もう一度送る
		private void StopVoiceInput()
		{
			keybd_event(VK_LWIN, 0, 0, UIntPtr.Zero);
			keybd_event(VK_H, 0, 0, UIntPtr.Zero);
			keybd_event(VK_H, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
			keybd_event(VK_LWIN, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
			SetListeningUi(false);
		}

		private void SetListeningUi(bool on)
		{
			isListening = on;
			button4.Text = on ? "音声入力停止" : "③ 話して質問";
			button4.BackColor = on
				? System.Drawing.Color.FromArgb(255, 192, 192)
				: System.Drawing.Color.FromArgb(255, 255, 192);
		}

		private void btnClearAll_Click(object sender, EventArgs e)
		{
			ResetContinue();
			pdfFilePaths.Clear();
			pdfContents.Clear();
			listBoxPDFs.Items.Clear();
			txtQuestion.Clear();

			SetInitialMessage();

			btnLoadPDFs.Enabled = false;

			if (tsslRate != null)
				tsslRate.Text = "";
		}

		// ====== ペイロードプレビュー（デバッグ用） ======
		private static string MaskKey(string key)
		{
			if (string.IsNullOrEmpty(key)) return "<未設定>";
			if (key.Length <= 8) return new string('*', key.Length);
			return key.Substring(0, 4) + new string('*', key.Length - 8) + key.Substring(key.Length - 4, 4);
		}

		private void btnPreviewPayload_Click(object sender, EventArgs e)
		{
			if (pdfContents.Count == 0)
			{
				MessageBox.Show(this, "PDFを読み込んでから実行してください。", "未読込",
					MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			var question = txtQuestion.Text?.Trim() ?? "";
			if (string.IsNullOrEmpty(question))
			{
				MessageBox.Show(this, "質問を入力してください。", "未入力",
					MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			try
			{
				bool useStrict = (chkStrictMode != null) ? chkStrictMode.Checked : true;
				string combinedContent = BuildContext(RetrievalQuery(question), 28000);

				if (string.IsNullOrWhiteSpace(combinedContent))
				{
					MessageBox.Show(this, "文脈抽出に失敗しました。PDFの読み込み内容をご確認ください。", "エラー",
						MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}

				string promptText;
				object requestBody;

				if (useStrict)
				{
					promptText = $@"次の「文脈」に含まれるテキストの“連続する一部”だけを引用し、JSONで返してください。
書式は厳密にこれのみ：{{""citations"":[{{""file"":""<ファイル名>"",""page"":<数値>,""quote"":""<引用>""}}, ...]}}
制約：
- quote は「文脈からの連続文字列」をそのまま貼り付けること（言い換え・要約・省略・追加は禁止）
- 文脈に無い内容は一切生成しないこと
- 文脈に該当が無ければ空配列（{{""citations"":[]}}）を返すこと
- JSON以外の文字は出力しないこと

【文脈】
{combinedContent}

{HistoryBlock(false)}【質問】
{question}";

					requestBody = new
					{
						contents = new[] { new { parts = new[] { new { text = promptText } } } },
						generationConfig = new { temperature = 0.0, maxOutputTokens = 4096 }
					};
				}
				else
				{
					promptText = $@"あなたは与えられたPDFの内容【のみ】を根拠に日本語で簡潔に回答します。
文書に無いことは「文書に記載されていません」と述べてください。
必ず根拠として、下記の見出し（例：`ファイル名 p.12`）を列挙してください。

【文書断片（重要度順・一部抜粋）】
{combinedContent}

{HistoryBlock(true)}【質問】
{question}

【出力フォーマット】
■ 結論
■ 根拠
- <ファイル名 p.X>: （該当箇所の要約または短い抜粋）
■ 注意点/不明点
";
					requestBody = new
					{
						contents = new[] { new { parts = new[] { new { text = promptText } } } },
						generationConfig = new { temperature = 0.3, maxOutputTokens = 8192 }
					};
				}

				string urlDisplay = $"https://generativelanguage.googleapis.com/v1beta/models/{geminiModel}:generateContent  (ヘッダー x-goog-api-key: {MaskKey(geminiApiKey)})";
				string pretty = JsonConvert.SerializeObject(requestBody, Formatting.Indented);

				using (var dlg = new PayloadPreviewDialog(useStrict ? "Strict" : "通常", urlDisplay, pretty, promptText))
				{
					dlg.ShowDialog(this);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, $"プレビュー生成エラー: {ex.Message}", "エラー",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		// ====== リソース解放（IDisposable パターン） ======
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				// HttpClient は static なので、アプリケーション終了時に解放
				components?.Dispose();
			}
			base.Dispose(disposing);
		}

		// ====== 内部型 ======
		private class UsageState
		{
			public string DatePacific { get; set; }
			public int Count { get; set; } // 旧形式（互換用）
			public Dictionary<string, int> Counts { get; set; }
		}


		// ====== Gemini レスポンス型 ======
		public class GeminiResponse
		{
			public List<Candidate> candidates { get; set; }
		}

		public class Candidate
		{
			public Content content { get; set; }
		}

		public class Content
		{
			public List<Part> parts { get; set; }
		}

		public class Part
		{
			public string text { get; set; }
		}

		private async void button1_Click(object sender, EventArgs e)
		{
			await ReadAnswerAloudAsync(true);
		}

		// 回答欄を朗読する（「根拠も読む」の設定に従う）。showWarning=false は自動朗読用
		private async Task ReadAnswerAloudAsync(bool showWarning)
		{
			if (string.IsNullOrWhiteSpace(ritAnswer.Text))
			{
				if (showWarning)
					MessageBox.Show("読み上げるテキストがありません。", "エラー",
						MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}
			string text = (chkReadBasis != null && !chkReadBasis.Checked)
				? RemoveBasisSection(ritAnswer.Text)
				: ritAnswer.Text;
			text = text.Replace(LiteNote, "");
			// Edge-TTSは読み上げる文章をMicrosoftのサーバーへ送るため、実行前に確認する
			//   はい → Edge-TTS / いいえ → Windows標準（今回だけ） / キャンセル → 中止
			var settingsForThisRead = voiceSettings;
			// PDFなしの一般回答には、PDFの機密情報が含まれないので、確認は出さない
			if (voiceSettings.Engine == VoiceSettings.EngineEdge && !lastAnswerWasNoPdf)
			{
				var r = MessageBox.Show(this,
					"選択中PDFは機密情報ですか？\nEdge-TTSを選択した場合は外部に漏れます！\n\n" +
					"はい　　　　：Edge-TTSで読み上げる\n" +
					"いいえ　　　：Windows標準で読み上げる（今回だけ）\n" +
					"キャンセル　：読み上げを中止",
					"Edge-TTSの確認", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
				if (r == DialogResult.Cancel) return;
				if (r == DialogResult.No)
				{
					// 保存済みの設定は変えず、今回の読み上げだけWindows標準にする
					settingsForThisRead = voiceSettings.Clone();
					settingsForThisRead.Engine = VoiceSettings.EngineWindows;
				}
			}
			await speaker.SpeakAsync(text, settingsForThisRead);
		}

		private void button2_Click(object sender, EventArgs e)
		{
			speaker.Stop();
		}

		// 「根拠」の見出しから次の見出し（注意点など）の前までを除いたテキスト
		private static string RemoveBasisSection(string text)
		{
			var lines = (text ?? "").Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
			var basisHead = new Regex(@"^\s*根拠(（.*）)?\s*$");
			var nextHead = new Regex(@"^\s*(注意点|不明点|結論)");
			var sb = new StringBuilder();
			bool skipping = false;
			foreach (var line in lines)
			{
				if (basisHead.IsMatch(line)) { skipping = true; continue; }
				if (skipping && nextHead.IsMatch(line)) skipping = false;
				if (!skipping) sb.AppendLine(line);
			}
			return sb.ToString();
		}

		// ====== 現在使用中のモデル（メニュー下のリスト） ======
		private string ModelItemText(string model)
		{
			return $"{model}　（1日{modelSettings.QuotaFor(model, defaultQuota)}回使用可能）";
		}

		private void RefreshModelCombo()
		{
			if (cmbModel == null) return;
			refreshingModelCombo = true;
			try
			{
				var models = modelSettings.Quotas.Keys.ToList();
				if (!string.IsNullOrWhiteSpace(geminiModel) && !models.Contains(geminiModel))
					models.Insert(0, geminiModel);

				cmbModel.Items.Clear();
				foreach (var m in models) cmbModel.Items.Add(ModelItemText(m));
				int idx = models.IndexOf(geminiModel);
				cmbModel.SelectedIndex = idx >= 0 ? idx : 0;
				cmbModel.Tag = models; // 表示順のモデル名
			}
			finally
			{
				refreshingModelCombo = false;
			}
		}

		private void OnModelComboChanged()
		{
			if (refreshingModelCombo) return;
			var models = cmbModel.Tag as List<string>;
			if (models == null || cmbModel.SelectedIndex < 0 || cmbModel.SelectedIndex >= models.Count) return;
			string chosen = models[cmbModel.SelectedIndex];
			if (chosen == geminiModel) return;

			geminiModel = chosen;
			modelSettings.Model = chosen;
			modelSettings.Save();
			UpdateQuotaLabel();
		}

		private void ShowModelSettings()
		{
			using (var dlg = new ModelSettingsForm(geminiModel, modelSettings, defaultQuota, geminiApiKey))
			{
				if (dlg.ShowDialog(this) != DialogResult.OK) return;
				geminiModel = dlg.SelectedModel;
				modelSettings.Model = dlg.SelectedModel;
				modelSettings.Quotas[dlg.SelectedModel] = dlg.SelectedQuota;
				modelSettings.Save();
				UpdateQuotaLabel();
				RefreshModelCombo();
			}
		}

		private void ShowVoiceSettings()
		{
			using (var dlg = new VoiceSettingsForm(voiceSettings, speaker))
			{
				if (dlg.ShowDialog(this) == DialogResult.OK)
				{
					voiceSettings = dlg.Result;
					voiceSettings.ReadBasis = chkReadBasis.Checked;
					voiceSettings.AutoRead = chkAutoRead.Checked;
					voiceSettings.Save();
				}
			}
		}

		private void button3_Click(object sender, EventArgs e)
		{
			// 音声合成オブジェクトを作成する
			SpeechSynthesizer synth = new SpeechSynthesizer();

			//インストールされた音声の列挙
			foreach (InstalledVoice voice in synth.GetInstalledVoices())
			{
				string Voice = "";
				VoiceInfo vinfo = voice.VoiceInfo;
				//音声様式情報をテキストボックスに表示
				Voice += "Name:    " + vinfo.Name + "\r\n";
				Voice += "Culture:    " + vinfo.Culture + "\r\n";
				Voice += "Age:    " + vinfo.Age + "\r\n";
				Voice += "Gender:    " + vinfo.Gender + "\r\n";
				DialogResult dr = MessageBox.Show(Voice + "次の音声を確認しますか？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				if (dr == DialogResult.No)
				{
					synth.SelectVoice(vinfo.Name);        //音声の設定
					break; ;
				}
			}
			// オブジェクトを解放する（ループ内で破棄すると2件目以降で例外になるため外へ）
			synth.Dispose();
		}

        private void button4_Click(object sender, EventArgs e)
        {
            // 「PDFなし質問」でないときは、PDFを読み込んでいないと話して質問できない
            if (!isListening && !EnsurePdfReady()) return;

            if (isListening)
            {
                //SendKeys.SendWait("{ESC}");

                // 音声入力（Win+Hのポップアップ）を停止
                recognizer?.RecognizeAsyncCancel();
                StopVoiceInput();
                armSubmitOnEnter = false;
            }
            else
            {
                // 音声認識を開始（非同期的に連続認識）
                //recognizer.RecognizeAsync(RecognizeMode.Multiple);
                //isListening = true;

                //engine = new SpeechRecognitionEngine();
                //engine = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("ja-JP"));
                //engine.SpeechRecognized += EngineSpeechRecognized;//認識処理
                //engine.SpeechHypothesized += EngineSpeechHypothesized;//推定処理
                //engine.LoadGrammar(new DictationGrammar());//ディクテーション用の辞書
                //engine.SetInputToDefaultAudioDevice();//エンジンの入力
                //engine.RecognizeAsync(RecognizeMode.Multiple);//開始



                isListening = true;
                // 新しい質問を話すので、前の質問を消す
                txtQuestion.Clear();
                armSubmitOnEnter = false;
                button4.Text = "音声入力停止";
                //背景色を255, 192, 192に変更
                button4.BackColor = System.Drawing.Color.FromArgb(255, 192, 192);
				txtQuestion.Focus();
				txtQuestion_Enter(this, EventArgs.Empty); // フォーカス取得時の処理を呼び出し

            }
        }
        private void EngineSpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            //Console.Write("Hypo: ");
            //Console.WriteLine(e.Result.Text);
        }

        private void EngineSpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            //txtQuestionに認識結果を表示
            txtQuestion.Text = e.Result.Text;
            
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            speaker?.Dispose();
            if (recognizer != null)
            {
                recognizer.Dispose();
            }
        }

		private void button5_Click(object sender, EventArgs e)
		{
			ResetContinue();
			//質問欄をクリア
			txtQuestion.Text = "";
			//音声認識を停止
			if (isListening)
			{
				//SendKeys.SendWait("{ESC}");
				// 音声認識を停止
				recognizer?.RecognizeAsyncCancel();
				isListening = false;
				button4.Text = "話して質問";
				//button4の背景色を255, 255, 192に変更
				button4.BackColor = System.Drawing.Color.FromArgb(255, 255, 192);
				//音声入力機能が起動していたら終了させる
			}
		}
        // ====== 質問欄フォーカス時に Windows+H 送信 ======
        private async void txtQuestion_Enter(object sender, EventArgs e)
        {
            if (!IsPdfReady()) return; // PDFが使えない間は、音声入力を自動で始めない
            await EnsureCompleteFocus();

            try
            {
                System.Diagnostics.Debug.WriteLine("音声入力を起動します...");
                await StartVoiceInputWithRetry();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"エラー: {ex.Message}");
                MessageBox.Show($"エラー: {ex.Message}");
            }
        }
        // 最もシンプルで直接的なWin+H送信
        private void SendWinH_Direct()
        {
            txtQuestion.Focus();
            System.Diagnostics.Debug.WriteLine("SendWinH_Direct 実行開始");            
            try
            {
                // 方法1: keybd_event（最もダイレクト）
                keybd_event(VK_LWIN, 0, 0, UIntPtr.Zero);
                keybd_event(VK_H, 0, 0, UIntPtr.Zero);                
                keybd_event(VK_H, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                keybd_event(VK_LWIN, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);                
                System.Diagnostics.Debug.WriteLine("keybd_event送信完了");                
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SendWinH_Direct エラー: {ex.Message}");
            }
            //完全に起動するまで待機
			System.Diagnostics.Debug.WriteLine("SendWinH_Direct 実行終了");			
        }
        // 完全なフォーカス確保
        private async Task EnsureCompleteFocus()
        {
            try
            {
                // 最小化状態から復元
                if (this.WindowState == FormWindowState.Minimized)
                {
                    this.WindowState = FormWindowState.Normal;
                }

                // 最前面に移動してフォーカスを設定
                this.TopMost = true;
                await Task.Delay(50);
                this.TopMost = false;

                this.Activate();
                this.BringToFront();
                txtQuestion.Focus();

                // システム処理完了待ち
                for (int i = 0; i < 5; i++)
                {
                    Application.DoEvents();
                    await Task.Delay(10);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"フォーカス確保エラー: {ex.Message}");
            }
        }
        // デスクトップをアクティブにする
        private void ActivateDesktop()
        {
            try
            {
                IntPtr desktopHandle = GetShellWindow();
                if (desktopHandle != IntPtr.Zero)
                {
                    SetForegroundWindow(desktopHandle);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"デスクトップアクティブ化エラー: {ex.Message}");
            }
        }
        // 音声入力ウィンドウの存在確認（簡素化）
        private async Task<bool> IsVoiceInputVisible()
        {
            for (int i = 0; i < 5; i++) // 最大1秒間チェック
            {
                string[] classNames = {
                    "Windows.UI.Core.CoreWindow",
                    "ApplicationFrameWindow",
                    "Windows.UI.Input.InputSite.WindowClass",
                    "Microsoft.VoiceTyping"
                };

                foreach (string className in classNames)
                {
                    IntPtr hwnd = FindWindow(className, null);
                    if (hwnd != IntPtr.Zero && IsWindowVisible(hwnd))
                    {
                        System.Diagnostics.Debug.WriteLine($"音声入力ウィンドウ発見: {className}");
                        return true;
                    }
                }

                await Task.Delay(200);
            }
            return false;
        }
        private async Task StartVoiceInputWithRetry()
        {
            for (int attempt = 0; attempt < 3; attempt++) // 最大3回リトライ
            {
                SendWinH_Direct(); // Win+Hを送信
                await Task.Delay(1000);

                if (await IsVoiceInputVisible())
                {
                    System.Diagnostics.Debug.WriteLine("音声入力が正常に起動しました。");
                    SetListeningUi(true);
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"音声入力の起動に失敗しました。リトライ {attempt + 1}/3");
            }

            MessageBox.Show("音声入力の起動に失敗しました。手動でWin+Hキーを押してください。");
        }

    }
}