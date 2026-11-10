using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using ImgEncoder = System.Drawing.Imaging.Encoder;

namespace PDFQuestionAnswering
{
	/// <summary>
	/// Google Nano Banana (Gemini 2.5 Flash Image) 画像編集クライアント
	/// 世界最高評価の画像編集モデルを使用
	/// </summary>
	public partial class frmNanoBananaImageEdit : Form
	{
		// ===== 状態管理 =====
		private string originalImagePath;
		private string apiKey = "YOUR_GEMINI_API_KEY";
		private readonly HttpClient http;
		private string _uploadedFileUri = null;

		// Nano Banana専用の最適化設定
		private const bool USE_FILE_API = true;
		private const int MAX_RETRIES = 5;
		private static DateTime lastRequestTime = DateTime.MinValue;
		private const int MIN_REQUEST_INTERVAL_MS = 90000; // 1.5分間隔（保守的）

		// Nano Banana特有の機能フラグ
		private CheckBox chkMaintainLikeness;
		private CheckBox chkMultiTurnEdit;
		private ComboBox cmbEditingMode;
		private Label lblNanoBananaInfo;

		public frmNanoBananaImageEdit(string apikey = null)
		{
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
			ServicePointManager.Expect100Continue = false;

			var handler = new HttpClientHandler
			{
				AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
			};
			http = new HttpClient(handler)
			{
				Timeout = TimeSpan.FromMinutes(3)
			};

			InitializeComponent();
			InitializeNanoBananaControls();

			if (!string.IsNullOrEmpty(apikey)) apiKey = apikey;
		}

		// Nano Banana専用UI初期化
		private void InitializeNanoBananaControls()
		{
			this.Text = "Nano Banana 画像編集 (世界最高評価モデル)";

			// 情報ラベル
			lblNanoBananaInfo = new Label()
			{
				Text = "🍌 Google Nano Banana - 世界最高評価の画像編集AI",
				Location = new Point(20, 10),
				Size = new Size(400, 25),
				Font = new Font("Arial", 10, FontStyle.Bold),
				ForeColor = Color.DarkGreen
			};
			this.Controls.Add(lblNanoBananaInfo);

			// 編集モード選択
			var lblMode = new Label()
			{
				Text = "編集モード:",
				Location = new Point(20, 40),
				Size = new Size(80, 20)
			};
			this.Controls.Add(lblMode);

			cmbEditingMode = new ComboBox()
			{
				Location = new Point(100, 38),
				Size = new Size(200, 25),
				DropDownStyle = ComboBoxStyle.DropDownList
			};
			cmbEditingMode.Items.AddRange(new string[] {
				"人物編集（顔・服装変更）",
				"ペット編集（動物特化）",
				"背景変更・合成",
				"スタイル転送",
				"汎用編集"
			});
			cmbEditingMode.SelectedIndex = 0;
			this.Controls.Add(cmbEditingMode);

			// 人物・ペットの類似性維持
			chkMaintainLikeness = new CheckBox()
			{
				Text = "人物・ペットの外見を維持する (Nano Banana特有機能)",
				Location = new Point(20, 70),
				Size = new Size(350, 20),
				Checked = true
			};
			this.Controls.Add(chkMaintainLikeness);

			// マルチターン編集
			chkMultiTurnEdit = new CheckBox()
			{
				Text = "段階的編集モード（複数回の細かい調整）",
				Location = new Point(20, 95),
				Size = new Size(300, 20),
				Checked = false
			};
			this.Controls.Add(chkMultiTurnEdit);

			//// 既存コントロールの位置調整
			//if (BtnLoad != null) BtnLoad.Location = new Point(20, 125);
			//if (btnEditImage != null)
			//{
			//	btnEditImage.Location = new Point(BtnLoad.Right + 20, 125);
			//	btnEditImage.Text = "🍌 Nano Banana で編集";
			//}

			//if (picOriginal != null)
			//{
			//	picOriginal.Location = new Point(20, 160);
			//	picOriginal.SizeMode = PictureBoxSizeMode.Zoom;
			//}
			//if (picEdited != null)
			//{
			//	picEdited.Location = new Point(picOriginal.Right + 20, 160);
			//	picEdited.SizeMode = PictureBoxSizeMode.Zoom;
			//}

			//if (txtEdit != null)
			//{
			//	txtEdit.Location = new Point(20, picOriginal.Bottom + 20);
			//	txtEdit.Multiline = true;
			//	txtEdit.Height = 60;
			//}

			btnEditImage.Enabled = false;
		}

		private void BtnLoad_Click(object sender, EventArgs e)
		{
			using (var ofd = new OpenFileDialog()
			{
				Filter = "画像ファイル|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.webp",
				Title = "Nano Bananaで編集する画像を選択"
			})
			{
				if (ofd.ShowDialog() != DialogResult.OK) return;

				try
				{
					originalImagePath = ofd.FileName;
					_uploadedFileUri = null;

					if (picOriginal.Image != null) picOriginal.Image.Dispose();
					picOriginal.Image = LoadImageNoLock(originalImagePath);

					if (picEdited.Image != null) { picEdited.Image.Dispose(); picEdited.Image = null; }

					btnEditImage.Enabled = true;

					lblNanoBananaInfo.Text = "🍌 Nano Banana 準備完了 - 世界最高品質で編集します！";
					lblNanoBananaInfo.ForeColor = Color.DarkOrange;
				}
				catch (Exception ex)
				{
					MessageBox.Show($"画像読み込みエラー: {ex.Message}", "エラー",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		private async void btnEditImage_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(originalImagePath))
			{
				MessageBox.Show("画像を選択してください", "警告",
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}
			if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_GEMINI_API_KEY")
			{
				MessageBox.Show("Gemini APIキーが未設定です", "エラー",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			btnEditImage.Enabled = false;
			var oldText = btnEditImage.Text;
			this.Cursor = Cursors.WaitCursor;

			try
			{
				using (var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5)))
				{
					await WaitForMinimumInterval(cts.Token);

					var result = await CallNanoBananaWithRetry(cts.Token);

					if (picEdited.Image != null) picEdited.Image.Dispose();
					picEdited.Image = result;
					picEdited.Refresh();

					lblNanoBananaInfo.Text = "🍌 Nano Banana 編集完了！世界最高品質の結果です";
					lblNanoBananaInfo.ForeColor = Color.Green;

					MessageBox.Show("Nano Bananaによる画像編集が完了しました！\n\n" +
						"この結果は世界最高評価の画像編集AIによるものです。",
						"🍌 Nano Banana 成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
			}
			catch (Exception ex)
			{
				lblNanoBananaInfo.Text = "🍌 Nano Banana エラー - 少し待って再試行してください";
				lblNanoBananaInfo.ForeColor = Color.Red;

				MessageBox.Show($"Nano Banana エラー: {ex.Message}\n\n" +
					"推奨対策:\n" +
					"1. 2-3分待ってから再実行\n" +
					"2. 画像サイズを小さくする\n" +
					"3. プロンプトをシンプルにする\n" +
					"4. Gemini APIキーの利用制限を確認",
					"🍌 Nano Banana エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			finally
			{
				this.Cursor = Cursors.Default;
				btnEditImage.Text = oldText;
				btnEditImage.Enabled = true;
			}
		}

		// 最低間隔待機
		private async Task WaitForMinimumInterval(CancellationToken ct)
		{
			var elapsed = DateTime.Now - lastRequestTime;
			var remaining = MIN_REQUEST_INTERVAL_MS - (int)elapsed.TotalMilliseconds;

			if (remaining > 0)
			{
				int waitSeconds = (remaining / 1000) + 1;
				lblNanoBananaInfo.Text = $"🍌 Nano Banana 待機中...あと{waitSeconds}秒";

				for (int i = waitSeconds; i > 0; i--)
				{
					btnEditImage.Text = $"🍌 待機中...({i}秒)";
					await Task.Delay(1000, ct);
				}

				btnEditImage.Text = "🍌 Nano Banana で編集";
			}
		}

		// Nano Banana APIコール（リトライ付き）
		private async Task<Image> CallNanoBananaWithRetry(CancellationToken ct)
		{
			Exception lastException = null;

			for (int attempt = 1; attempt <= MAX_RETRIES; attempt++)
			{
				try
				{
					btnEditImage.Text = $"🍌 Nano Banana 処理中...({attempt}/{MAX_RETRIES})";
					lblNanoBananaInfo.Text = $"🍌 世界最高AIが画像を分析中...試行 {attempt}";

					var result = await CallNanoBananaOnce(ct);
					lastRequestTime = DateTime.Now;
					return result;
				}
				catch (Exception ex)
				{
					lastException = ex;
					lastRequestTime = DateTime.Now;

					if (ex.Message.Contains("429") || ex.Message.Contains("レート制限"))
					{
						if (attempt < MAX_RETRIES)
						{
							// 指数バックオフ（2分 → 4分 → 8分...）
							int delayMinutes = (int)Math.Pow(2, attempt);
							lblNanoBananaInfo.Text = $"🍌 レート制限中...{delayMinutes}分待機";

							int totalSeconds = delayMinutes * 60;
							for (int i = totalSeconds; i > 0; i -= 30)
							{
								ct.ThrowIfCancellationRequested();
								btnEditImage.Text = $"🍌 レート制限...あと{i / 60}:{i % 60:D2}";
								await Task.Delay(Math.Min(30000, i * 1000), ct);
							}
							continue;
						}
					}
					else if (attempt < MAX_RETRIES)
					{
						await Task.Delay(15000, ct); // その他エラーは15秒待機
						continue;
					}
				}
			}

			throw lastException ?? new Exception("Nano Bananaの呼び出しに失敗しました");
		}

		// Nano Banana 1回呼び出し
		private async Task<Image> CallNanoBananaOnce(CancellationToken ct)
		{
			// Nano Banana用に最適化（高品質維持）
			var prep = PrepareImageForNanoBanana(originalImagePath);

			// Nano Banana専用プロンプト構築
			string prompt = BuildNanoBananaPrompt();

			string json;

			if (USE_FILE_API)
			{
				if (string.IsNullOrEmpty(_uploadedFileUri))
				{
					lblNanoBananaInfo.Text = "🍌 Nano Banana用ファイルアップロード中...";
					_uploadedFileUri = await UploadToFileAPI(prep.rawBytes, prep.mime, ct);
				}
				json = BuildNanoBananaJsonWithFile(prompt, prep.mime, _uploadedFileUri);
			}
			else
			{
				json = BuildNanoBananaJsonInline(prompt, prep.mime, prep.base64);
			}

			// Nano Banana APIエンドポイント
			var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-image-preview:generateContent?key={apiKey}";

			using (var req = new HttpRequestMessage(HttpMethod.Post, url))
			{
				req.Headers.TryAddWithoutValidation("Accept", "application/json");
				req.Headers.TryAddWithoutValidation("User-Agent", "NanoBananaClient/1.0");
				req.Content = new StringContent(json, Encoding.UTF8, "application/json");

				lblNanoBananaInfo.Text = "🍌 Nano Banana AI が創作中...";

				using (var resp = await http.SendAsync(req, HttpCompletionOption.ResponseContentRead, ct))
				{
					string body = await resp.Content.ReadAsStringAsync();

					if (resp.IsSuccessStatusCode)
					{
						var img = ExtractImageFromResponse(body);
						if (img != null) return img;

						var textResponse = ExtractTextFromResponse(body);
						throw new Exception($"Nano Bananaが画像を生成できませんでした。応答: {textResponse}");
					}

					if ((int)resp.StatusCode == 429)
					{
						var rateLimitInfo = AnalyzeRateLimit(body, resp);
						throw new Exception($"Nano Banana レート制限: {rateLimitInfo}");
					}

					var errorInfo = ExtractErrorInfo(body);
					throw new Exception($"Nano Banana API エラー ({(int)resp.StatusCode}): {errorInfo}");
				}
			}
		}

		// Nano Banana専用プロンプト構築
		private string BuildNanoBananaPrompt()
		{
			var basePrompt = txtEdit?.Text ?? "画像を編集してください";
			var mode = cmbEditingMode.SelectedItem?.ToString() ?? "汎用編集";
			var maintainLikeness = chkMaintainLikeness.Checked;
			var multiTurn = chkMultiTurnEdit.Checked;

			var promptBuilder = new StringBuilder();

			// Nano Banana特有の指示
			promptBuilder.AppendLine("You are Nano Banana, the world's top-rated image editing AI from Google DeepMind.");

			if (maintainLikeness)
			{
				promptBuilder.AppendLine("CRITICAL: Maintain exact facial features, body proportions, and character likeness. This is your specialty.");
			}

			// モード別最適化
			switch (mode)
			{
				case "人物編集（顔・服装変更）":
					promptBuilder.AppendLine("Focus on person editing while preserving identity, facial features, and natural appearance.");
					break;
				case "ペット編集（動物特化）":
					promptBuilder.AppendLine("Specialize in pet/animal editing while maintaining breed characteristics and personality.");
					break;
				case "背景変更・合成":
					promptBuilder.AppendLine("Focus on background changes and photo blending while maintaining subject integrity.");
					break;
				case "スタイル転送":
					promptBuilder.AppendLine("Apply artistic styles while preserving the core essence of the subject.");
					break;
			}

			if (multiTurn)
			{
				promptBuilder.AppendLine("Prepare for potential follow-up edits. Make subtle, refineable changes.");
			}

			promptBuilder.AppendLine($"\nUser Request: {basePrompt}");
			promptBuilder.AppendLine("\nGenerate only the edited image as output. Maintain highest quality standards.");

			return promptBuilder.ToString();
		}

		// Nano Banana用画像準備（高品質重視）
		private (byte[] rawBytes, string base64, string mime) PrepareImageForNanoBanana(string path)
		{
			const int MAX_SIZE = 1024; // Nano Bananaは高解像度対応
			const int QUALITY = 85;    // 高品質維持

			var bytes = File.ReadAllBytes(path);
			using (var msIn = new MemoryStream(bytes))
			using (var src = Image.FromStream(msIn))
			{
				int w = src.Width, h = src.Height;

				// アスペクト比保持でリサイズ
				if (Math.Max(w, h) > MAX_SIZE)
				{
					double scale = (double)MAX_SIZE / Math.Max(w, h);
					w = (int)(w * scale);
					h = (int)(h * scale);
				}

				using (var bmp = new Bitmap(w, h, PixelFormat.Format24bppRgb))
				using (var g = Graphics.FromImage(bmp))
				{
					// Nano Banana用高品質設定
					g.SmoothingMode = SmoothingMode.HighQuality;
					g.InterpolationMode = InterpolationMode.HighQualityBicubic;
					g.PixelOffsetMode = PixelOffsetMode.HighQuality;
					g.CompositingQuality = CompositingQuality.HighQuality;

					g.DrawImage(src, new Rectangle(0, 0, w, h));

					using (var msOut = new MemoryStream())
					{
						var enc = ImageCodecInfo.GetImageEncoders().First(x => x.MimeType == "image/jpeg");
						var ep = new EncoderParameters(1);
						ep.Param[0] = new EncoderParameter(ImgEncoder.Quality, (long)QUALITY);
						bmp.Save(msOut, enc, ep);

						var outBytes = msOut.ToArray();
						return (outBytes, Convert.ToBase64String(outBytes), "image/jpeg");
					}
				}
			}
		}

		// Nano Banana用JSON（File API版）
		private string BuildNanoBananaJsonWithFile(string prompt, string mimeType, string fileUri)
		{
			var json = $@"{{
  ""contents"": [
    {{
      ""role"": ""user"",
      ""parts"": [
        {{ ""text"": ""{EscapeJson(prompt)}"" }},
        {{ ""file_data"": {{ ""file_uri"": ""{EscapeJson(fileUri)}"", ""mime_type"": ""{EscapeJson(mimeType)}"" }} }}
      ]
    }}
  ],
  ""generationConfig"": {{ 
    ""temperature"": 0.2,
    ""maxOutputTokens"": 4096,
    ""topP"": 0.8
  }},
  ""safetySettings"": [
    {{ ""category"": ""HARM_CATEGORY_HARASSMENT"", ""threshold"": ""BLOCK_ONLY_HIGH"" }},
    {{ ""category"": ""HARM_CATEGORY_HATE_SPEECH"", ""threshold"": ""BLOCK_ONLY_HIGH"" }},
    {{ ""category"": ""HARM_CATEGORY_SEXUALLY_EXPLICIT"", ""threshold"": ""BLOCK_ONLY_HIGH"" }},
    {{ ""category"": ""HARM_CATEGORY_DANGEROUS_CONTENT"", ""threshold"": ""BLOCK_ONLY_HIGH"" }}
  ]
}}";
			return json;
		}

		// Nano Banana用JSON（Inline版）
		private string BuildNanoBananaJsonInline(string prompt, string mimeType, string base64Image)
		{
			var json = $@"{{
  ""contents"": [
    {{
      ""role"": ""user"",
      ""parts"": [
        {{ ""text"": ""{EscapeJson(prompt)}"" }},
        {{ ""inline_data"": {{ ""mime_type"": ""{EscapeJson(mimeType)}"", ""data"": ""{base64Image}"" }} }}
      ]
    }}
  ],
  ""generationConfig"": {{ 
    ""temperature"": 0.2,
    ""maxOutputTokens"": 4096,
    ""topP"": 0.8
  }}
}}";
			return json;
		}

		// ファイルAPIアップロード
		private async Task<string> UploadToFileAPI(byte[] imageBytes, string mime, CancellationToken ct)
		{
			var url = $"https://generativelanguage.googleapis.com/upload/v1beta/files?key={apiKey}";
			using (var content = new ByteArrayContent(imageBytes))
			{
				content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mime);
				using (var resp = await http.PostAsync(url, content, ct))
				{
					string body = await resp.Content.ReadAsStringAsync();
					if (!resp.IsSuccessStatusCode)
					{
						if ((int)resp.StatusCode == 429)
							throw new Exception("File API レート制限中");
						throw new Exception($"Nano Banana ファイルアップロード失敗: {body}");
					}

					var ser = new JavaScriptSerializer();
					var root = ser.DeserializeObject(body) as Dictionary<string, object>;
					var file = root?["file"] as Dictionary<string, object>;
					var uri = file?["uri"] as string;

					if (string.IsNullOrEmpty(uri))
						throw new Exception("Nano BananaファイルアップロードでURIを取得できませんでした");

					return uri;
				}
			}
		}

		// レート制限分析
		private string AnalyzeRateLimit(string errorJson, HttpResponseMessage resp)
		{
			var info = new StringBuilder("Nano Banana レート制限詳細:\n");

			// Retry-Afterヘッダ
			if (resp.Headers.TryGetValues("Retry-After", out var retryAfter))
			{
				var seconds = retryAfter.FirstOrDefault();
				if (int.TryParse(seconds, out var sec))
				{
					info.AppendLine($"推奨待機: {sec}秒 ({sec / 60}分)");
				}
			}

			// JSON詳細
			try
			{
				var ser = new JavaScriptSerializer();
				var root = ser.DeserializeObject(errorJson) as Dictionary<string, object>;
				if (root?.ContainsKey("error") == true)
				{
					var error = root["error"] as Dictionary<string, object>;
					if (error?.ContainsKey("message") == true)
					{
						info.AppendLine($"メッセージ: {error["message"]}");
					}
				}
			}
			catch { }

			info.AppendLine("対策: 2-3分待機後に再実行してください");
			return info.ToString();
		}

		// 応答画像抽出
		private Image ExtractImageFromResponse(string json)
		{
			try
			{
				var ser = new JavaScriptSerializer();
				var root = ser.DeserializeObject(json) as Dictionary<string, object>;
				var candidates = root?["candidates"] as object[];

				foreach (var candObj in candidates ?? new object[0])
				{
					var cand = candObj as Dictionary<string, object>;
					var content = cand?["content"] as Dictionary<string, object>;
					var parts = content?["parts"] as object[];

					foreach (var partObj in parts ?? new object[0])
					{
						var part = partObj as Dictionary<string, object>;

						// inline_data または inlineData をチェック
						var inlineData = part?.ContainsKey("inline_data") == true ?
							part["inline_data"] as Dictionary<string, object> :
							part?.ContainsKey("inlineData") == true ?
							part["inlineData"] as Dictionary<string, object> : null;

						if (inlineData?.ContainsKey("data") == true)
						{
							var base64Data = inlineData["data"] as string;
							if (!string.IsNullOrEmpty(base64Data))
							{
								var bytes = Convert.FromBase64String(base64Data);
								using (var ms = new MemoryStream(bytes))
								using (var temp = Image.FromStream(ms))
								{
									// Nano Bananaの高品質結果を保持
									var result = new Bitmap(temp.Width, temp.Height, PixelFormat.Format32bppArgb);
									using (var g = Graphics.FromImage(result))
									{
										g.DrawImageUnscaled(temp, 0, 0);
									}
									return result;
								}
							}
						}
					}
				}
			}
			catch { }
			return null;
		}

		// テキスト応答抽出
		private string ExtractTextFromResponse(string json)
		{
			try
			{
				var ser = new JavaScriptSerializer();
				var root = ser.DeserializeObject(json) as Dictionary<string, object>;
				var candidates = root?["candidates"] as object[];

				foreach (var candObj in candidates ?? new object[0])
				{
					var cand = candObj as Dictionary<string, object>;
					var content = cand?["content"] as Dictionary<string, object>;
					var parts = content?["parts"] as object[];

					foreach (var partObj in parts ?? new object[0])
					{
						var part = partObj as Dictionary<string, object>;
						if (part?.ContainsKey("text") == true)
						{
							return part["text"] as string;
						}
					}
				}
			}
			catch { }
			return "応答を解析できませんでした";
		}

		// エラー情報抽出
		private string ExtractErrorInfo(string json)
		{
			try
			{
				var ser = new JavaScriptSerializer();
				var root = ser.DeserializeObject(json) as Dictionary<string, object>;
				var error = root?["error"] as Dictionary<string, object>;

				var message = error?.ContainsKey("message") == true ? error["message"] as string : "";
				var code = error?.ContainsKey("code") == true ? error["code"].ToString() : "";
				var status = error?.ContainsKey("status") == true ? error["status"] as string : "";

				return $"[{code}] {status}: {message}";
			}
			catch { }
			return json.Length > 200 ? json.Substring(0, 200) + "..." : json;
		}

		// ユーティリティ
		private static Image LoadImageNoLock(string path)
		{
			var bytes = File.ReadAllBytes(path);
			using (var ms = new MemoryStream(bytes))
			using (var tmp = Image.FromStream(ms))
			{
				var copy = new Bitmap(tmp.Width, tmp.Height, PixelFormat.Format32bppArgb);
				using (var g = Graphics.FromImage(copy)) g.DrawImageUnscaled(tmp, 0, 0);
				return copy;
			}
		}

		private static string EscapeJson(string s)
		{
			if (string.IsNullOrEmpty(s)) return "";
			return s.Replace("\\", "\\\\").Replace("\"", "\\\"")
					.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");
		}

		// リソース解放
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				http?.Dispose();
				picOriginal?.Image?.Dispose();
				picEdited?.Image?.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}