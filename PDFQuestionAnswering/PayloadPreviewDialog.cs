using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace PDFQuestionAnswering
{
	public partial class PayloadPreviewDialog : Form
	{
		private readonly string _title;
		private readonly string _url;
		private readonly string _json;
		private readonly string _rawPrompt; // ★ 追加：生プロンプト（改行実体

		public PayloadPreviewDialog(string title, string urlDisplay, string jsonPretty, string rawPrompt)
		{
			_title = title ?? "";
			_url = urlDisplay ?? "";
			_json = jsonPretty ?? "";
			_rawPrompt = rawPrompt ?? ""; // null回避
			InitializeComponent(); // ← Designer で生成したコントロールを初期化
		}

		// ★ 追加：折り返しとスクロールバーを一括で整える
		private void UpdateWrapUI()
		{
			bool wrap = chkWrap.Checked;                 // チェックあり＝折り返しON
			rtbMain.WordWrap = wrap;
			rtbMain.ScrollBars = wrap
				? RichTextBoxScrollBars.Vertical         // 折り返しON：横スクロール不要
				: RichTextBoxScrollBars.Both;            // 折り返しOFF：横/縦どちらも
		}

		private void PayloadPreviewDialog_Load(object sender, EventArgs e)
		{
			Text = $"送信内容プレビュー - {_title}";
			BuildText();

			// 初期状態（Designer で Checked=true にしている想定）
			UpdateWrapUI();

			rtbMain.SelectionStart = 0;
			rtbMain.ScrollToCaret();
		}

		private void BuildText()
		{
			var sb = new StringBuilder();
			sb.AppendLine("【URL（マスク表示）】");
			sb.AppendLine(_url);
			sb.AppendLine();
			sb.AppendLine("【Payload (JSON)】");
			sb.AppendLine(_json);

			// ★ 追加：可読な“展開テキスト”を併記（\n などの記号は登場しない）
			sb.AppendLine();
			sb.AppendLine("【展開テキスト（text）】");
			sb.AppendLine(string.IsNullOrWhiteSpace(_rawPrompt) ? "（空）" : _rawPrompt);

			rtbMain.Text = sb.ToString();
		}

		private void chkWrap_CheckedChanged(object sender, EventArgs e)
		{
			UpdateWrapUI();
		}

		private void btnCopy_Click(object sender, EventArgs e)
		{
			try { Clipboard.SetText(rtbMain.Text); }
			catch { /* クリップボード失敗は無視 */ }
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			if (saveFileDialog1.ShowDialog(this) == DialogResult.OK)
			{
				try { File.WriteAllText(saveFileDialog1.FileName, _json, Encoding.UTF8); }
				catch (Exception ex)
				{
					MessageBox.Show(this, $"保存に失敗しました: {ex.Message}", "保存エラー",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		private void btnClose_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}
	}
}
