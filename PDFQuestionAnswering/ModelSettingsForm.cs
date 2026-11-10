using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Windows.Forms;

namespace PDFQuestionAnswering
{
	/// <summary>モデル設定ダイアログ。使うモデルと、そのモデルの1日の上限回数を決める（コードのみで構築）。</summary>
	public class ModelSettingsForm : Form
	{
		private class ModelItem
		{
			public string Id; public string Display;
			public override string ToString() => string.IsNullOrEmpty(Display) || Display == Id ? Id : Id + "  （" + Display + "）";
		}

		private static readonly HttpClient Http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };

		private readonly string apiKey;
		private readonly ModelSettings settings;
		private readonly int defaultQuota;
		private readonly ComboBox cmbModel = new ComboBox { DropDownStyle = ComboBoxStyle.DropDown };
		private readonly NumericUpDown numQuota = new NumericUpDown { Minimum = 1, Maximum = 100000 };
		private readonly Button btnFetch = new Button { Text = "一覧を取得" };
		private readonly Label lblInfo = new Label { AutoSize = false, ForeColor = Color.DimGray };
		private bool loading;

		public string SelectedModel { get; private set; }
		public int SelectedQuota { get; private set; }

		public ModelSettingsForm(string currentModel, ModelSettings settings, int defaultQuota, string apiKey)
		{
			this.settings = settings;
			this.defaultQuota = defaultQuota;
			this.apiKey = apiKey;
			SelectedModel = currentModel;

			Text = "モデル設定";
			FormBorderStyle = FormBorderStyle.FixedDialog;
			StartPosition = FormStartPosition.CenterParent;
			MaximizeBox = false; MinimizeBox = false;
			ClientSize = new Size(520, 250);
			Font = new Font("メイリオ", 9F);

			Controls.Add(new Label { Text = "モデル", Left = 16, Top = 20, AutoSize = true });
			cmbModel.SetBounds(100, 16, 300, 26);
			btnFetch.SetBounds(410, 15, 100, 28);
			Controls.Add(cmbModel); Controls.Add(btnFetch);

			Controls.Add(new Label { Text = "1日の上限", Left = 16, Top = 62, AutoSize = true });
			numQuota.SetBounds(100, 58, 100, 26);
			Controls.Add(numQuota);
			Controls.Add(new Label { Text = "回（モデルごとに覚えます）", Left = 206, Top = 62, AutoSize = true });

			lblInfo.SetBounds(16, 100, 488, 100);
			lblInfo.Text =
				"・上限の回数は、AI Studio の「レート制限」画面（https://aistudio.google.com/rate-limit）のRPDの数字を入れてください。\r\n" +
				"・「一覧を取得」を押すと、APIキーで使えるモデルの名前を取得します（文章を作れるモデルだけを表示）。\r\n" +
				"・モデル名は直接入力もできます（例: gemini-2.5-flash-lite）。";
			Controls.Add(lblInfo);

			var btnOk = new Button { Text = "OK", Left = 340, Top = 206, Width = 80, DialogResult = DialogResult.OK };
			var btnCancel = new Button { Text = "キャンセル", Left = 426, Top = 206, Width = 84, DialogResult = DialogResult.Cancel };
			Controls.Add(btnOk); Controls.Add(btnCancel);
			AcceptButton = btnOk; CancelButton = btnCancel;

			loading = true;
			foreach (var id in new[] { "gemini-3.5-flash-lite", "gemini-3.6-flash", "gemini-3.8-flash", "gemini-3-flash-preview", "gemini-3.1-flash-lite", "gemini-2.5-flash", "gemini-2.5-flash-lite" })
				cmbModel.Items.Add(new ModelItem { Id = id });
			if (!string.IsNullOrWhiteSpace(currentModel) && !cmbModel.Items.Cast<ModelItem>().Any(m => m.Id == currentModel))
				cmbModel.Items.Insert(0, new ModelItem { Id = currentModel });
			cmbModel.Text = currentModel;
			numQuota.Value = Clamp(settings.QuotaFor(currentModel, defaultQuota));
			loading = false;

			cmbModel.TextChanged += (s, e) => { if (!loading) ShowQuotaForText(); };
			cmbModel.SelectedIndexChanged += (s, e) => { if (!loading) ShowQuotaForText(); };
			btnFetch.Click += async (s, e) => await FetchModelsAsync();

			FormClosing += (s, e) =>
			{
				if (DialogResult != DialogResult.OK) return;
				string id = CurrentId();
				if (string.IsNullOrWhiteSpace(id))
				{
					MessageBox.Show(this, "モデル名を入力してください。", "確認", MessageBoxButtons.OK, MessageBoxIcon.Information);
					e.Cancel = true;
					return;
				}
				SelectedModel = id;
				SelectedQuota = (int)numQuota.Value;
			};
		}

		private static decimal Clamp(int v) => Math.Max(1, Math.Min(100000, v));

		private string CurrentId()
		{
			if (cmbModel.SelectedItem is ModelItem mi && cmbModel.Text == mi.ToString()) return mi.Id;
			var t = (cmbModel.Text ?? "").Trim();
			int i = t.IndexOf("  （", StringComparison.Ordinal);
			return (i > 0 ? t.Substring(0, i) : t).Trim();
		}

		private void ShowQuotaForText()
		{
			string id = CurrentId();
			if (string.IsNullOrEmpty(id)) return;
			numQuota.Value = Clamp(settings.QuotaFor(id, defaultQuota));
		}

		private async System.Threading.Tasks.Task FetchModelsAsync()
		{
			if (string.IsNullOrWhiteSpace(apiKey))
			{
				MessageBox.Show(this, "先にAPIキーを設定してください。", "確認", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			btnFetch.Enabled = false;
			string keep = CurrentId();
			try
			{
				var items = new List<ModelItem>();
				string pageToken = null;
				do
				{
					string url = "https://generativelanguage.googleapis.com/v1beta/models?pageSize=200" +
								 (pageToken != null ? "&pageToken=" + Uri.EscapeDataString(pageToken) : "");
					using (var req = new HttpRequestMessage(HttpMethod.Get, url))
					{
						req.Headers.Add("x-goog-api-key", apiKey);
						using (var resp = await Http.SendAsync(req))
						{
							string body = await resp.Content.ReadAsStringAsync();
							if (!resp.IsSuccessStatusCode)
								throw new HttpRequestException((int)resp.StatusCode + " " + resp.ReasonPhrase);
							var j = JObject.Parse(body);
							foreach (var m in (JArray)j["models"] ?? new JArray())
							{
								string name = ((string)m["name"] ?? "").Replace("models/", "");
								var methods = ((JArray)m["supportedGenerationMethods"] ?? new JArray()).Select(x => (string)x).ToList();
								if (!methods.Contains("generateContent")) continue;
								string low = name.ToLowerInvariant();
								if (new[] { "tts", "image", "live", "embedding", "audio", "aqa", "imagen", "veo", "robotics", "computer-use", "native" }.Any(low.Contains)) continue;
								items.Add(new ModelItem { Id = name, Display = (string)m["displayName"] });
							}
							pageToken = (string)j["nextPageToken"];
						}
					}
				} while (!string.IsNullOrEmpty(pageToken));

				items = items.OrderBy(i => i.Id).ToList();
				loading = true;
				cmbModel.Items.Clear();
				foreach (var it in items) cmbModel.Items.Add(it);
				var sel = items.FirstOrDefault(i => i.Id == keep);
				if (sel != null) cmbModel.SelectedItem = sel; else cmbModel.Text = keep;
				loading = false;
				MessageBox.Show(this, items.Count + " 件のモデルを取得しました。ドロップダウンから選んでください。", "取得完了",
					MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			catch (Exception ex)
			{
				loading = false;
				MessageBox.Show(this, "モデル一覧を取得できませんでした: " + ex.Message, "エラー",
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			finally
			{
				btnFetch.Enabled = true;
			}
		}
	}
}
