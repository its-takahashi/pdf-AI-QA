namespace PDFQuestionAnswering
{
	partial class PayloadPreviewDialog
	{
		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.RichTextBox rtbMain;
		private System.Windows.Forms.Panel panelBottom;
		private System.Windows.Forms.CheckBox chkWrap;
		private System.Windows.Forms.Button btnCopy;
		private System.Windows.Forms.Button btnSave;
		private System.Windows.Forms.Button btnClose;
		private System.Windows.Forms.SaveFileDialog saveFileDialog1;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
				components.Dispose();
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			this.rtbMain = new System.Windows.Forms.RichTextBox();
			this.panelBottom = new System.Windows.Forms.Panel();
			this.chkWrap = new System.Windows.Forms.CheckBox();
			this.btnCopy = new System.Windows.Forms.Button();
			this.btnSave = new System.Windows.Forms.Button();
			this.btnClose = new System.Windows.Forms.Button();
			this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
			this.panelBottom.SuspendLayout();
			this.SuspendLayout();
			// 
			// rtbMain
			// 
			this.rtbMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rtbMain.Font = new System.Drawing.Font("Consolas", 10F);
			this.rtbMain.HideSelection = false;
			this.rtbMain.Location = new System.Drawing.Point(0, 0);
			this.rtbMain.Name = "rtbMain";
			this.rtbMain.ReadOnly = true;
			this.rtbMain.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
			this.rtbMain.Size = new System.Drawing.Size(920, 632);
			this.rtbMain.TabIndex = 0;
			this.rtbMain.Text = "";
			// 
			// panelBottom
			// 
			this.panelBottom.Controls.Add(this.chkWrap);
			this.panelBottom.Controls.Add(this.btnCopy);
			this.panelBottom.Controls.Add(this.btnSave);
			this.panelBottom.Controls.Add(this.btnClose);
			this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panelBottom.Location = new System.Drawing.Point(0, 632);
			this.panelBottom.Name = "panelBottom";
			this.panelBottom.Size = new System.Drawing.Size(920, 48);
			this.panelBottom.TabIndex = 1;
			// 
			// chkWrap
			// 
			this.chkWrap.AutoSize = true;
			this.chkWrap.Location = new System.Drawing.Point(12, 14);
			this.chkWrap.Name = "chkWrap";
			this.chkWrap.Size = new System.Drawing.Size(65, 16);
			this.chkWrap.TabIndex = 1;
			this.chkWrap.Text = "折り返し";
			this.chkWrap.UseVisualStyleBackColor = true;
			this.chkWrap.CheckedChanged += new System.EventHandler(this.chkWrap_CheckedChanged);
			// 
			// btnCopy
			// 
			this.btnCopy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCopy.Location = new System.Drawing.Point(626, 10);
			this.btnCopy.Name = "btnCopy";
			this.btnCopy.Size = new System.Drawing.Size(90, 27);
			this.btnCopy.TabIndex = 2;
			this.btnCopy.Text = "コピー";
			this.btnCopy.UseVisualStyleBackColor = true;
			this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
			// 
			// btnSave
			// 
			this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnSave.Location = new System.Drawing.Point(722, 10);
			this.btnSave.Name = "btnSave";
			this.btnSave.Size = new System.Drawing.Size(90, 27);
			this.btnSave.TabIndex = 3;
			this.btnSave.Text = "保存...";
			this.btnSave.UseVisualStyleBackColor = true;
			this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
			// 
			// btnClose
			// 
			this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnClose.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnClose.Location = new System.Drawing.Point(818, 10);
			this.btnClose.Name = "btnClose";
			this.btnClose.Size = new System.Drawing.Size(90, 27);
			this.btnClose.TabIndex = 4;
			this.btnClose.Text = "閉じる";
			this.btnClose.UseVisualStyleBackColor = true;
			this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
			// 
			// saveFileDialog1
			// 
			this.saveFileDialog1.FileName = "payload.json";
			this.saveFileDialog1.Filter = "JSON ファイル (*.json)|*.json|テキスト (*.txt)|*.txt|すべてのファイル (*.*)|*.*";
			this.saveFileDialog1.Title = "送信JSONの保存";
			// 
			// PayloadPreviewDialog
			// 
			this.AcceptButton = this.btnClose;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(920, 680);
			this.Controls.Add(this.rtbMain);
			this.Controls.Add(this.panelBottom);
			this.MinimizeBox = false;
			this.Name = "PayloadPreviewDialog";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "送信内容プレビュー";
			this.Load += new System.EventHandler(this.PayloadPreviewDialog_Load);
			this.panelBottom.ResumeLayout(false);
			this.panelBottom.PerformLayout();
			this.ResumeLayout(false);

		}
	}
}
