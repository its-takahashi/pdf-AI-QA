using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;          // ← 追加
using Newtonsoft.Json.Linq;     // ← 追加
								// using System.Text.Json;      // ← 使わない場合はコメントアウト推奨（衝突回避）


namespace PDFQuestionAnswering
{
	public partial class GeminiApiKeyForm : Form
	{
		public GeminiApiKeyForm()
		{
			InitializeComponent();
		}

		private void GeminiApiKeyForm_Load(object sender, EventArgs e)
		{
			try
			{
				var current = AppSettings.Instance.GeminiApiKey ?? string.Empty;
				this.txtGemini.Text = current;
				this.txtGemini.UseSystemPasswordChar = !this.chkShow.Checked;
			}
			catch
			{
				// 読み込み失敗は無視（空で続行）
			}
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			var key = (this.txtGemini.Text ?? string.Empty).Trim();

			if (key.Length == 0)
			{
				var r = MessageBox.Show(this,
					"API Key が未入力です。空のまま保存しますか？",
					"確認",
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Question);
				if (r != DialogResult.Yes) return;
			}

			try
			{
				AppSettings.Instance.GeminiApiKey = key; // 平文はインメモリ
				AppSettings.Instance.Save();             // DPAPI で暗号化 → settings.json

				MessageBox.Show(this, "保存しました。", "完了",
					MessageBoxButtons.OK, MessageBoxIcon.Information);

				this.DialogResult = DialogResult.OK;
				this.Close();
			}
			catch (Exception ex)
			{
				MessageBox.Show(this,
					"保存中にエラーが発生しました。\n" + ex.Message,
					"エラー",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
			}
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void chkShow_CheckedChanged(object sender, EventArgs e)
		{
			this.txtGemini.UseSystemPasswordChar = !this.chkShow.Checked;
		}
	}
}
