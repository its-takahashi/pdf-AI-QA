namespace PDFQuestionAnswering
{
	partial class frmNanoBananaImageEdit
	{
		/// <summary>
		/// 必要なデザイナー変数です。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 使用中のリソースをすべてクリーンアップします。
		/// </summary>
		/// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
		//protected override void Dispose(bool disposing)
		//{
		//	if (disposing && (components != null))
		//	{
		//		components.Dispose();
		//	}
		//	base.Dispose(disposing);
		//}

		#region Windows フォーム デザイナーで生成されたコード

		/// <summary>
		/// デザイナー サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディターで変更しないでください。
		/// </summary>
		private void InitializeComponent()
		{
			this.BtnLoad = new System.Windows.Forms.Button();
			this.btnEditImage = new System.Windows.Forms.Button();
			this.picOriginal = new System.Windows.Forms.PictureBox();
			this.picEdited = new System.Windows.Forms.PictureBox();
			this.txtEdit = new System.Windows.Forms.TextBox();
			this.lblOriginal = new System.Windows.Forms.Label();
			this.lblEdited = new System.Windows.Forms.Label();
			this.lblPrompt = new System.Windows.Forms.Label();
			this.panelControls = new System.Windows.Forms.Panel();
			this.groupBoxSettings = new System.Windows.Forms.GroupBox();
			this.btnSaveEdited = new System.Windows.Forms.Button();
			this.btnClearEdited = new System.Windows.Forms.Button();
			this.panelImages = new System.Windows.Forms.Panel();
			this.groupBoxImages = new System.Windows.Forms.GroupBox();
			this.groupBoxPrompt = new System.Windows.Forms.GroupBox();
			this.statusStrip = new System.Windows.Forms.StatusStrip();
			this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripProgressBar = new System.Windows.Forms.ToolStripProgressBar();
			((System.ComponentModel.ISupportInitialize)(this.picOriginal)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.picEdited)).BeginInit();
			this.panelControls.SuspendLayout();
			this.panelImages.SuspendLayout();
			this.groupBoxImages.SuspendLayout();
			this.groupBoxPrompt.SuspendLayout();
			this.statusStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// BtnLoad
			// 
			this.BtnLoad.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.BtnLoad.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.BtnLoad.Location = new System.Drawing.Point(20, 125);
			this.BtnLoad.Name = "BtnLoad";
			this.BtnLoad.Size = new System.Drawing.Size(120, 35);
			this.BtnLoad.TabIndex = 0;
			this.BtnLoad.Text = "  画像読込";
			this.BtnLoad.UseVisualStyleBackColor = true;
			this.BtnLoad.Click += new System.EventHandler(this.BtnLoad_Click);
			// 
			// btnEditImage
			// 
			this.btnEditImage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(193)))), ((int)(((byte)(7)))));
			this.btnEditImage.Enabled = false;
			this.btnEditImage.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnEditImage.ForeColor = System.Drawing.Color.DarkSlateGray;
			this.btnEditImage.Location = new System.Drawing.Point(160, 125);
			this.btnEditImage.Name = "btnEditImage";
			this.btnEditImage.Size = new System.Drawing.Size(180, 35);
			this.btnEditImage.TabIndex = 1;
			this.btnEditImage.Text = "🍌 Nano Banana で編集";
			this.btnEditImage.UseVisualStyleBackColor = false;
			this.btnEditImage.Click += new System.EventHandler(this.btnEditImage_Click);
			// 
			// picOriginal
			// 
			this.picOriginal.BackColor = System.Drawing.Color.WhiteSmoke;
			this.picOriginal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.picOriginal.Location = new System.Drawing.Point(15, 25);
			this.picOriginal.Name = "picOriginal";
			this.picOriginal.Size = new System.Drawing.Size(320, 240);
			this.picOriginal.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.picOriginal.TabIndex = 2;
			this.picOriginal.TabStop = false;
			// 
			// picEdited
			// 
			this.picEdited.BackColor = System.Drawing.Color.WhiteSmoke;
			this.picEdited.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.picEdited.Location = new System.Drawing.Point(355, 25);
			this.picEdited.Name = "picEdited";
			this.picEdited.Size = new System.Drawing.Size(320, 240);
			this.picEdited.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.picEdited.TabIndex = 3;
			this.picEdited.TabStop = false;
			// 
			// txtEdit
			// 
			this.txtEdit.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtEdit.Location = new System.Drawing.Point(15, 45);
			this.txtEdit.Multiline = true;
			this.txtEdit.Name = "txtEdit";
			this.txtEdit.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtEdit.Size = new System.Drawing.Size(660, 80);
			this.txtEdit.TabIndex = 4;
			// 
			// lblOriginal
			// 
			this.lblOriginal.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblOriginal.Location = new System.Drawing.Point(15, 275);
			this.lblOriginal.Name = "lblOriginal";
			this.lblOriginal.Size = new System.Drawing.Size(320, 20);
			this.lblOriginal.TabIndex = 5;
			this.lblOriginal.Text = "📸 元画像";
			this.lblOriginal.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// lblEdited
			// 
			this.lblEdited.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblEdited.Location = new System.Drawing.Point(355, 275);
			this.lblEdited.Name = "lblEdited";
			this.lblEdited.Size = new System.Drawing.Size(320, 20);
			this.lblEdited.TabIndex = 6;
			this.lblEdited.Text = "🍌 Nano Banana 編集結果";
			this.lblEdited.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// lblPrompt
			// 
			this.lblPrompt.AutoSize = true;
			this.lblPrompt.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblPrompt.Location = new System.Drawing.Point(15, 25);
			this.lblPrompt.Name = "lblPrompt";
			this.lblPrompt.Size = new System.Drawing.Size(146, 15);
			this.lblPrompt.TabIndex = 7;
			this.lblPrompt.Text = "🎯 編集指示（プロンプト）:";
			// 
			// panelControls
			// 
			this.panelControls.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
			this.panelControls.Controls.Add(this.groupBoxSettings);
			this.panelControls.Controls.Add(this.BtnLoad);
			this.panelControls.Controls.Add(this.btnEditImage);
			this.panelControls.Controls.Add(this.btnSaveEdited);
			this.panelControls.Controls.Add(this.btnClearEdited);
			this.panelControls.Dock = System.Windows.Forms.DockStyle.Top;
			this.panelControls.Location = new System.Drawing.Point(0, 0);
			this.panelControls.Name = "panelControls";
			this.panelControls.Size = new System.Drawing.Size(1000, 175);
			this.panelControls.TabIndex = 8;
			// 
			// groupBoxSettings
			// 
			this.groupBoxSettings.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.groupBoxSettings.Location = new System.Drawing.Point(20, 10);
			this.groupBoxSettings.Name = "groupBoxSettings";
			this.groupBoxSettings.Size = new System.Drawing.Size(950, 105);
			this.groupBoxSettings.TabIndex = 13;
			this.groupBoxSettings.TabStop = false;
			this.groupBoxSettings.Text = "🍌 Nano Banana 設定（世界最高評価の画像編集AI）";
			// 
			// btnSaveEdited
			// 
			this.btnSaveEdited.Enabled = false;
			this.btnSaveEdited.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnSaveEdited.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.btnSaveEdited.Location = new System.Drawing.Point(360, 125);
			this.btnSaveEdited.Name = "btnSaveEdited";
			this.btnSaveEdited.Size = new System.Drawing.Size(100, 35);
			this.btnSaveEdited.TabIndex = 11;
			this.btnSaveEdited.Text = "  保存";
			this.btnSaveEdited.UseVisualStyleBackColor = true;
			// 
			// btnClearEdited
			// 
			this.btnClearEdited.Enabled = false;
			this.btnClearEdited.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnClearEdited.Location = new System.Drawing.Point(480, 125);
			this.btnClearEdited.Name = "btnClearEdited";
			this.btnClearEdited.Size = new System.Drawing.Size(80, 35);
			this.btnClearEdited.TabIndex = 12;
			this.btnClearEdited.Text = "クリア";
			this.btnClearEdited.UseVisualStyleBackColor = true;
			// 
			// panelImages
			// 
			this.panelImages.BackColor = System.Drawing.Color.White;
			this.panelImages.Controls.Add(this.groupBoxImages);
			this.panelImages.Controls.Add(this.groupBoxPrompt);
			this.panelImages.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelImages.Location = new System.Drawing.Point(0, 175);
			this.panelImages.Name = "panelImages";
			this.panelImages.Size = new System.Drawing.Size(1000, 465);
			this.panelImages.TabIndex = 9;
			// 
			// groupBoxImages
			// 
			this.groupBoxImages.Controls.Add(this.picOriginal);
			this.groupBoxImages.Controls.Add(this.picEdited);
			this.groupBoxImages.Controls.Add(this.lblOriginal);
			this.groupBoxImages.Controls.Add(this.lblEdited);
			this.groupBoxImages.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.groupBoxImages.Location = new System.Drawing.Point(20, 15);
			this.groupBoxImages.Name = "groupBoxImages";
			this.groupBoxImages.Size = new System.Drawing.Size(950, 310);
			this.groupBoxImages.TabIndex = 14;
			this.groupBoxImages.TabStop = false;
			this.groupBoxImages.Text = "📸 画像比較";
			// 
			// groupBoxPrompt
			// 
			this.groupBoxPrompt.Controls.Add(this.lblPrompt);
			this.groupBoxPrompt.Controls.Add(this.txtEdit);
			this.groupBoxPrompt.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.groupBoxPrompt.Location = new System.Drawing.Point(20, 340);
			this.groupBoxPrompt.Name = "groupBoxPrompt";
			this.groupBoxPrompt.Size = new System.Drawing.Size(950, 140);
			this.groupBoxPrompt.TabIndex = 15;
			this.groupBoxPrompt.TabStop = false;
			this.groupBoxPrompt.Text = "🎯 Nano Banana 編集指示";
			// 
			// statusStrip
			// 
			this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel,
            this.toolStripProgressBar});
			this.statusStrip.Location = new System.Drawing.Point(0, 640);
			this.statusStrip.Name = "statusStrip";
			this.statusStrip.Size = new System.Drawing.Size(1000, 22);
			this.statusStrip.TabIndex = 10;
			this.statusStrip.Text = "statusStrip1";
			// 
			// toolStripStatusLabel
			// 
			this.toolStripStatusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
			this.toolStripStatusLabel.Name = "toolStripStatusLabel";
			this.toolStripStatusLabel.Size = new System.Drawing.Size(220, 17);
			this.toolStripStatusLabel.Text = "🍌 Nano Banana 画像編集AI - 準備完了";
			// 
			// toolStripProgressBar
			// 
			this.toolStripProgressBar.Name = "toolStripProgressBar";
			this.toolStripProgressBar.Size = new System.Drawing.Size(100, 16);
			this.toolStripProgressBar.Visible = false;
			// 
			// frmNanoBananaImageEdit
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(1000, 662);
			this.Controls.Add(this.panelImages);
			this.Controls.Add(this.panelControls);
			this.Controls.Add(this.statusStrip);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.MinimumSize = new System.Drawing.Size(1016, 700);
			this.Name = "frmNanoBananaImageEdit";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "🍌 Nano Banana 画像編集 - Google最高評価AI";
			((System.ComponentModel.ISupportInitialize)(this.picOriginal)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.picEdited)).EndInit();
			this.panelControls.ResumeLayout(false);
			this.panelImages.ResumeLayout(false);
			this.groupBoxImages.ResumeLayout(false);
			this.groupBoxPrompt.ResumeLayout(false);
			this.groupBoxPrompt.PerformLayout();
			this.statusStrip.ResumeLayout(false);
			this.statusStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button BtnLoad;
		private System.Windows.Forms.Button btnEditImage;
		private System.Windows.Forms.PictureBox picOriginal;
		private System.Windows.Forms.PictureBox picEdited;
		private System.Windows.Forms.TextBox txtEdit;
		private System.Windows.Forms.Label lblOriginal;
		private System.Windows.Forms.Label lblEdited;
		private System.Windows.Forms.Label lblPrompt;
		private System.Windows.Forms.Panel panelControls;
		private System.Windows.Forms.Panel panelImages;
		private System.Windows.Forms.StatusStrip statusStrip;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
		private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar;
		private System.Windows.Forms.Button btnSaveEdited;
		private System.Windows.Forms.Button btnClearEdited;
		private System.Windows.Forms.GroupBox groupBoxSettings;
		private System.Windows.Forms.GroupBox groupBoxImages;
		private System.Windows.Forms.GroupBox groupBoxPrompt;
	}
}