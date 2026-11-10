namespace PDFQuestionAnswering
{
	partial class Form1
	{
		/// <summary>
		/// 必要なデザイナー変数です。
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.Button btnSelectPDF;
		private System.Windows.Forms.ListBox listBoxPDFs;
		private System.Windows.Forms.Button btnLoadPDFs;
		private System.Windows.Forms.ProgressBar progressBar;
		private System.Windows.Forms.TextBox txtQuestion;
		private System.Windows.Forms.Button btnAskQuestion;
		private System.Windows.Forms.Button btnClearAll;
		private System.Windows.Forms.Label lblPDFFiles;
		private System.Windows.Forms.Label lblQuestion;
		private System.Windows.Forms.Label lblAnswer;
		private System.Windows.Forms.GroupBox groupBoxSetup;
		private System.Windows.Forms.GroupBox groupBoxQuestion;

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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.btnSelectPDF = new System.Windows.Forms.Button();
            this.listBoxPDFs = new System.Windows.Forms.ListBox();
            this.btnLoadPDFs = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.txtQuestion = new System.Windows.Forms.TextBox();
            this.btnAskQuestion = new System.Windows.Forms.Button();
            this.btnClearAll = new System.Windows.Forms.Button();
            this.lblPDFFiles = new System.Windows.Forms.Label();
            this.lblQuestion = new System.Windows.Forms.Label();
            this.lblAnswer = new System.Windows.Forms.Label();
            this.groupBoxSetup = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.lblApiKeyStatus = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBoxQuestion = new System.Windows.Forms.GroupBox();
            this.ritAnswer = new System.Windows.Forms.RichTextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.button5 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.chkStrictMode = new System.Windows.Forms.CheckBox();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.btnPreviewPayload = new System.Windows.Forms.Button();
            this.chkReadBasis = new System.Windows.Forms.CheckBox();
            this.chkAutoRead = new System.Windows.Forms.CheckBox();
            this.chkNoPdf = new System.Windows.Forms.CheckBox();
            this.chkContinue = new System.Windows.Forms.CheckBox();
            this.lblModel = new System.Windows.Forms.Label();
            this.cmbModel = new System.Windows.Forms.ComboBox();
            this.btnModelSettings = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tsslQuota = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsslRate = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.ファイルToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.設定ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aPIキーの設定ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ツールToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBoxSetup.SuspendLayout();
            this.groupBoxQuestion.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnSelectPDF
            // 
            this.btnSelectPDF.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnSelectPDF.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSelectPDF.Font = new System.Drawing.Font("メイリオ", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.btnSelectPDF.Location = new System.Drawing.Point(17, 37);
            this.btnSelectPDF.Margin = new System.Windows.Forms.Padding(4);
            this.btnSelectPDF.Name = "btnSelectPDF";
            this.btnSelectPDF.Size = new System.Drawing.Size(150, 34);
            this.btnSelectPDF.TabIndex = 2;
            this.btnSelectPDF.Text = "① PDFファイル選択";
            this.btnSelectPDF.UseVisualStyleBackColor = false;
            this.btnSelectPDF.Click += new System.EventHandler(this.btnSelectPDF_Click);
            // 
            // listBoxPDFs
            // 
            this.listBoxPDFs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxPDFs.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listBoxPDFs.Font = new System.Drawing.Font("メイリオ", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listBoxPDFs.FormattingEnabled = true;
            this.listBoxPDFs.ItemHeight = 18;
            this.listBoxPDFs.Location = new System.Drawing.Point(180, 46);
            this.listBoxPDFs.Margin = new System.Windows.Forms.Padding(4);
            this.listBoxPDFs.Name = "listBoxPDFs";
            this.listBoxPDFs.Size = new System.Drawing.Size(707, 92);
            this.listBoxPDFs.TabIndex = 4;
            // 
            // btnLoadPDFs
            // 
            this.btnLoadPDFs.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnLoadPDFs.Enabled = false;
            this.btnLoadPDFs.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLoadPDFs.Font = new System.Drawing.Font("メイリオ", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.btnLoadPDFs.Location = new System.Drawing.Point(18, 130);
            this.btnLoadPDFs.Margin = new System.Windows.Forms.Padding(4);
            this.btnLoadPDFs.Name = "btnLoadPDFs";
            this.btnLoadPDFs.Size = new System.Drawing.Size(150, 34);
            this.btnLoadPDFs.TabIndex = 5;
            this.btnLoadPDFs.Text = "② PDF読み込み";
            this.btnLoadPDFs.UseVisualStyleBackColor = false;
            this.btnLoadPDFs.Click += new System.EventHandler(this.btnLoadPDFs_Click);
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(181, 138);
            this.progressBar.Margin = new System.Windows.Forms.Padding(4);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(705, 25);
            this.progressBar.TabIndex = 6;
            this.progressBar.Visible = false;
            // 
            // txtQuestion
            // 
            this.txtQuestion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtQuestion.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.txtQuestion.Location = new System.Drawing.Point(180, 52);
            this.txtQuestion.Margin = new System.Windows.Forms.Padding(4);
            this.txtQuestion.Multiline = true;
            this.txtQuestion.Name = "txtQuestion";
            this.txtQuestion.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtQuestion.Size = new System.Drawing.Size(707, 102);
            this.txtQuestion.TabIndex = 10;
            this.txtQuestion.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtQuestion_KeyDown);
            // 
            // btnAskQuestion
            // 
            this.btnAskQuestion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnAskQuestion.Enabled = false;
            this.btnAskQuestion.Font = new System.Drawing.Font("メイリオ", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.btnAskQuestion.Location = new System.Drawing.Point(17, 88);
            this.btnAskQuestion.Margin = new System.Windows.Forms.Padding(4);
            this.btnAskQuestion.Name = "btnAskQuestion";
            this.btnAskQuestion.Size = new System.Drawing.Size(150, 30);
            this.btnAskQuestion.TabIndex = 11;
            this.btnAskQuestion.Text = "④ AI回答取得";
            this.btnAskQuestion.UseVisualStyleBackColor = false;
            this.btnAskQuestion.Click += new System.EventHandler(this.btnAskQuestion_Click);
            // 
            // btnClearAll
            // 
            this.btnClearAll.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.btnClearAll.Font = new System.Drawing.Font("メイリオ", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.btnClearAll.Location = new System.Drawing.Point(179, 16);
            this.btnClearAll.Margin = new System.Windows.Forms.Padding(4);
            this.btnClearAll.Name = "btnClearAll";
            this.btnClearAll.Size = new System.Drawing.Size(109, 28);
            this.btnClearAll.TabIndex = 7;
            this.btnClearAll.Text = "PDFをクリア";
            this.btnClearAll.UseVisualStyleBackColor = false;
            this.btnClearAll.Click += new System.EventHandler(this.btnClearAll_Click);
            // 
            // lblPDFFiles
            // 
            this.lblPDFFiles.AutoSize = true;
            this.lblPDFFiles.Font = new System.Drawing.Font("メイリオ", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblPDFFiles.Location = new System.Drawing.Point(401, 22);
            this.lblPDFFiles.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPDFFiles.Name = "lblPDFFiles";
            this.lblPDFFiles.Size = new System.Drawing.Size(131, 23);
            this.lblPDFFiles.TabIndex = 3;
            this.lblPDFFiles.Text = "＜選択中のPDF＞";
            // 
            // lblQuestion
            // 
            this.lblQuestion.AutoSize = true;
            this.lblQuestion.Font = new System.Drawing.Font("メイリオ", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblQuestion.Location = new System.Drawing.Point(638, 29);
            this.lblQuestion.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblQuestion.Name = "lblQuestion";
            this.lblQuestion.Size = new System.Drawing.Size(140, 20);
            this.lblQuestion.TabIndex = 9;
            this.lblQuestion.Text = "Ctrl+Enterで実行も可";
            // 
            // lblAnswer
            // 
            this.lblAnswer.AutoSize = true;
            this.lblAnswer.Font = new System.Drawing.Font("メイリオ", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblAnswer.Location = new System.Drawing.Point(442, 185);
            this.lblAnswer.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblAnswer.Name = "lblAnswer";
            this.lblAnswer.Size = new System.Drawing.Size(74, 24);
            this.lblAnswer.TabIndex = 12;
            this.lblAnswer.Text = "＜回答＞";
            // 
            // groupBoxSetup
            // 
            this.groupBoxSetup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxSetup.Controls.Add(this.listBoxPDFs);
            this.groupBoxSetup.Controls.Add(this.progressBar);
            this.groupBoxSetup.Controls.Add(this.label2);
            this.groupBoxSetup.Controls.Add(this.lblApiKeyStatus);
            this.groupBoxSetup.Controls.Add(this.btnSelectPDF);
            this.groupBoxSetup.Controls.Add(this.btnLoadPDFs);
            this.groupBoxSetup.Controls.Add(this.btnClearAll);
            this.groupBoxSetup.Controls.Add(this.label1);
            this.groupBoxSetup.Controls.Add(this.lblPDFFiles);
            this.groupBoxSetup.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.groupBoxSetup.Location = new System.Drawing.Point(13, 72);
            this.groupBoxSetup.Margin = new System.Windows.Forms.Padding(4);
            this.groupBoxSetup.Name = "groupBoxSetup";
            this.groupBoxSetup.Padding = new System.Windows.Forms.Padding(4);
            this.groupBoxSetup.Size = new System.Drawing.Size(902, 175);
            this.groupBoxSetup.TabIndex = 8;
            this.groupBoxSetup.TabStop = false;
            this.groupBoxSetup.Text = "セットアップ(PDF取込み)";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label2.Location = new System.Drawing.Point(180, 137);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(707, 27);
            this.label2.TabIndex = 10;
            // 
            // lblApiKeyStatus
            // 
            this.lblApiKeyStatus.AutoSize = true;
            this.lblApiKeyStatus.Font = new System.Drawing.Font("メイリオ", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblApiKeyStatus.Location = new System.Drawing.Point(635, 24);
            this.lblApiKeyStatus.Name = "lblApiKeyStatus";
            this.lblApiKeyStatus.Size = new System.Drawing.Size(55, 18);
            this.lblApiKeyStatus.TabIndex = 8;
            this.lblApiKeyStatus.Text = "API-Key";
            this.lblApiKeyStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Yu Gothic UI", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label1.ForeColor = System.Drawing.Color.Blue;
            this.label1.Location = new System.Drawing.Point(68, 74);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 46);
            this.label1.TabIndex = 9;
            this.label1.Text = "⇩";
            // 
            // groupBoxQuestion
            // 
            this.groupBoxQuestion.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxQuestion.Controls.Add(this.ritAnswer);
            this.groupBoxQuestion.Controls.Add(this.txtQuestion);
            this.groupBoxQuestion.Controls.Add(this.label3);
            this.groupBoxQuestion.Controls.Add(this.button5);
            this.groupBoxQuestion.Controls.Add(this.button4);
            this.groupBoxQuestion.Controls.Add(this.button3);
            this.groupBoxQuestion.Controls.Add(this.radioButton2);
            this.groupBoxQuestion.Controls.Add(this.chkStrictMode);
            this.groupBoxQuestion.Controls.Add(this.radioButton1);
            this.groupBoxQuestion.Controls.Add(this.button2);
            this.groupBoxQuestion.Controls.Add(this.button1);
            this.groupBoxQuestion.Controls.Add(this.btnPreviewPayload);
            this.groupBoxQuestion.Controls.Add(this.lblQuestion);
            this.groupBoxQuestion.Controls.Add(this.btnAskQuestion);
            this.groupBoxQuestion.Controls.Add(this.lblAnswer);
            this.groupBoxQuestion.Controls.Add(this.chkReadBasis);
            this.groupBoxQuestion.Controls.Add(this.chkAutoRead);
            this.groupBoxQuestion.Font = new System.Drawing.Font("メイリオ", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.groupBoxQuestion.Location = new System.Drawing.Point(13, 287);
            this.groupBoxQuestion.Margin = new System.Windows.Forms.Padding(4);
            this.groupBoxQuestion.Name = "groupBoxQuestion";
            this.groupBoxQuestion.Padding = new System.Windows.Forms.Padding(4);
            this.groupBoxQuestion.Size = new System.Drawing.Size(902, 399);
            this.groupBoxQuestion.TabIndex = 14;
            this.groupBoxQuestion.TabStop = false;
            this.groupBoxQuestion.Text = "質問・回答";
            // 
            // ritAnswer
            // 
            this.ritAnswer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ritAnswer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ritAnswer.Font = new System.Drawing.Font("メイリオ", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ritAnswer.Location = new System.Drawing.Point(17, 211);
            this.ritAnswer.Margin = new System.Windows.Forms.Padding(4);
            this.ritAnswer.Name = "ritAnswer";
            this.ritAnswer.Size = new System.Drawing.Size(869, 170);
            this.ritAnswer.TabIndex = 14;
            this.ritAnswer.Text = "";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("メイリオ", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label3.Location = new System.Drawing.Point(418, 29);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(115, 23);
            this.label3.TabIndex = 11;
            this.label3.Text = "＜質問の内容＞";
            // 
            // button5
            // 
            this.button5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.button5.Font = new System.Drawing.Font("メイリオ", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.button5.Location = new System.Drawing.Point(181, 22);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(100, 28);
            this.button5.TabIndex = 23;
            this.button5.Text = "質問クリア";
            this.toolTip1.SetToolTip(this.button5, "開発用");
            this.button5.UseVisualStyleBackColor = false;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button4
            // 
            this.button4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.button4.Font = new System.Drawing.Font("メイリオ", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.button4.Image = ((System.Drawing.Image)(resources.GetObject("button4.Image")));
            this.button4.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button4.Location = new System.Drawing.Point(17, 45);
            this.button4.Name = "button4";
            this.button4.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.button4.Size = new System.Drawing.Size(150, 40);
            this.button4.TabIndex = 22;
            this.button4.Text = "③ 話して質問";
            this.toolTip1.SetToolTip(this.button4, "開発用");
            this.button4.UseVisualStyleBackColor = false;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button3
            // 
            this.button3.Font = new System.Drawing.Font("メイリオ", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.button3.Location = new System.Drawing.Point(790, 183);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(96, 25);
            this.button3.TabIndex = 21;
            this.button3.Text = "音声ボイス確認";
            this.toolTip1.SetToolTip(this.button3, "開発用");
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Font = new System.Drawing.Font("メイリオ", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.radioButton2.Location = new System.Drawing.Point(736, 186);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(48, 21);
            this.radioButton2.TabIndex = 20;
            this.radioButton2.TabStop = true;
            this.radioButton2.Text = "英語";
            this.radioButton2.UseVisualStyleBackColor = true;
            this.radioButton2.Visible = false;
            // 
            // chkStrictMode
            // 
            this.chkStrictMode.AutoSize = true;
            this.chkStrictMode.Font = new System.Drawing.Font("メイリオ", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.chkStrictMode.Location = new System.Drawing.Point(19, 23);
            this.chkStrictMode.Name = "chkStrictMode";
            this.chkStrictMode.Size = new System.Drawing.Size(151, 22);
            this.chkStrictMode.TabIndex = 15;
            this.chkStrictMode.Text = "厳格モード (引用のみ)";
            this.chkStrictMode.UseVisualStyleBackColor = true;
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Font = new System.Drawing.Font("メイリオ", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.radioButton1.Location = new System.Drawing.Point(671, 187);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(59, 21);
            this.radioButton1.TabIndex = 19;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "日本語";
            this.radioButton1.UseVisualStyleBackColor = true;
            this.radioButton1.Visible = false;
            // 
            // button2
            // 
            this.button2.ForeColor = System.Drawing.Color.Red;
            this.button2.Image = ((System.Drawing.Image)(resources.GetObject("button2.Image")));
            this.button2.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button2.Location = new System.Drawing.Point(104, 178);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(80, 32);
            this.button2.TabIndex = 18;
            this.button2.Text = "停 止";
            this.button2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.Image = ((System.Drawing.Image)(resources.GetObject("button1.Image")));
            this.button1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button1.Location = new System.Drawing.Point(18, 178);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(80, 32);
            this.button1.TabIndex = 17;
            this.button1.Text = "朗 読";
            this.button1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnPreviewPayload
            // 
            this.btnPreviewPayload.Font = new System.Drawing.Font("メイリオ", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.btnPreviewPayload.Location = new System.Drawing.Point(17, 124);
            this.btnPreviewPayload.Name = "btnPreviewPayload";
            this.btnPreviewPayload.Size = new System.Drawing.Size(150, 30);
            this.btnPreviewPayload.TabIndex = 16;
            this.btnPreviewPayload.Text = "送信内容";
            this.toolTip1.SetToolTip(this.btnPreviewPayload, "開発用");
            this.btnPreviewPayload.UseVisualStyleBackColor = true;
            this.btnPreviewPayload.Click += new System.EventHandler(this.btnPreviewPayload_Click);
            // 
            // chkReadBasis
            // 
            this.chkReadBasis.AutoSize = true;
            this.chkReadBasis.Font = new System.Drawing.Font("メイリオ", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.chkReadBasis.Location = new System.Drawing.Point(194, 187);
            this.chkReadBasis.Name = "chkReadBasis";
            this.chkReadBasis.Size = new System.Drawing.Size(93, 24);
            this.chkReadBasis.TabIndex = 31;
            this.chkReadBasis.Text = "根拠も読む";
            this.chkReadBasis.UseVisualStyleBackColor = true;
            // 
            // chkAutoRead
            // 
            this.chkAutoRead.AutoSize = true;
            this.chkAutoRead.Font = new System.Drawing.Font("メイリオ", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.chkAutoRead.Location = new System.Drawing.Point(293, 187);
            this.chkAutoRead.Name = "chkAutoRead";
            this.chkAutoRead.Size = new System.Drawing.Size(119, 24);
            this.chkAutoRead.TabIndex = 32;
            this.chkAutoRead.Text = "回答を自動朗読";
            this.chkAutoRead.UseVisualStyleBackColor = true;
            // 
            // chkNoPdf
            // 
            this.chkNoPdf.AutoSize = true;
            this.chkNoPdf.Font = new System.Drawing.Font("メイリオ", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.chkNoPdf.ForeColor = System.Drawing.Color.Blue;
            this.chkNoPdf.Location = new System.Drawing.Point(42, 253);
            this.chkNoPdf.Name = "chkNoPdf";
            this.chkNoPdf.Size = new System.Drawing.Size(135, 27);
            this.chkNoPdf.TabIndex = 30;
            this.chkNoPdf.Text = "PDFなしで質問";
            this.toolTip1.SetToolTip(this.chkNoPdf, "PDFを使わず、Geminiの一般知識で答えます（根拠は確認できません）");
            this.chkNoPdf.UseVisualStyleBackColor = true;
            // 
            // chkContinue
            // 
            this.chkContinue.AutoSize = true;
            this.chkContinue.Font = new System.Drawing.Font("メイリオ", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.chkContinue.ForeColor = System.Drawing.Color.Blue;
            this.chkContinue.Location = new System.Drawing.Point(190, 253);
            this.chkContinue.Name = "chkContinue";
            this.chkContinue.Size = new System.Drawing.Size(134, 27);
            this.chkContinue.TabIndex = 36;
            this.chkContinue.Text = "続けて質問する";
            this.toolTip1.SetToolTip(this.chkContinue, "直前の質問と回答を引き継いで質問します（「その罰金は？」など）。話題を変えるときは「質問クリア」を押してください。");
            this.chkContinue.UseVisualStyleBackColor = true;
            // 
            // lblModel
            // 
            this.lblModel.AutoSize = true;
            this.lblModel.Location = new System.Drawing.Point(16, 38);
            this.lblModel.Name = "lblModel";
            this.lblModel.Size = new System.Drawing.Size(128, 18);
            this.lblModel.TabIndex = 33;
            this.lblModel.Text = "現在使用中のモデル：";
            // 
            // cmbModel
            // 
            this.cmbModel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbModel.FormattingEnabled = true;
            this.cmbModel.Location = new System.Drawing.Point(150, 34);
            this.cmbModel.Name = "cmbModel";
            this.cmbModel.Size = new System.Drawing.Size(440, 26);
            this.cmbModel.TabIndex = 34;
            // 
            // btnModelSettings
            // 
            this.btnModelSettings.Location = new System.Drawing.Point(600, 33);
            this.btnModelSettings.Name = "btnModelSettings";
            this.btnModelSettings.Size = new System.Drawing.Size(84, 26);
            this.btnModelSettings.TabIndex = 35;
            this.btnModelSettings.Text = "設定…";
            this.toolTip1.SetToolTip(this.btnModelSettings, "モデルの追加・一覧取得・1日の上限回数の変更");
            this.btnModelSettings.UseVisualStyleBackColor = true;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsslQuota,
            this.tsslRate});
            this.statusStrip1.Location = new System.Drawing.Point(0, 681);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(928, 22);
            this.statusStrip1.TabIndex = 15;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tsslQuota
            // 
            this.tsslQuota.Name = "tsslQuota";
            this.tsslQuota.Size = new System.Drawing.Size(212, 17);
            this.tsslQuota.Text = "本日 0/{dailyQuota}（残り {dailyQuota}";
            // 
            // tsslRate
            // 
            this.tsslRate.Name = "tsslRate";
            this.tsslRate.Size = new System.Drawing.Size(118, 17);
            this.tsslRate.Text = "toolStripStatusLabel2";
            // 
            // toolTip1
            // 
            this.toolTip1.AutomaticDelay = 5000;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ファイルToolStripMenuItem,
            this.設定ToolStripMenuItem,
            this.ツールToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(928, 24);
            this.menuStrip1.TabIndex = 16;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // ファイルToolStripMenuItem
            // 
            this.ファイルToolStripMenuItem.Name = "ファイルToolStripMenuItem";
            this.ファイルToolStripMenuItem.Size = new System.Drawing.Size(53, 20);
            this.ファイルToolStripMenuItem.Text = "ファイル";
            // 
            // 設定ToolStripMenuItem
            // 
            this.設定ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aPIキーの設定ToolStripMenuItem});
            this.設定ToolStripMenuItem.Name = "設定ToolStripMenuItem";
            this.設定ToolStripMenuItem.Size = new System.Drawing.Size(43, 20);
            this.設定ToolStripMenuItem.Text = "設定";
            // 
            // aPIキーの設定ToolStripMenuItem
            // 
            this.aPIキーの設定ToolStripMenuItem.Name = "aPIキーの設定ToolStripMenuItem";
            this.aPIキーの設定ToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this.aPIキーの設定ToolStripMenuItem.Text = "APIキーの設定";
            this.aPIキーの設定ToolStripMenuItem.Click += new System.EventHandler(this.aPIキーの設定ToolStripMenuItem_Click);
            // 
            // ツールToolStripMenuItem
            // 
            this.ツールToolStripMenuItem.Name = "ツールToolStripMenuItem";
            this.ツールToolStripMenuItem.Size = new System.Drawing.Size(46, 20);
            this.ツールToolStripMenuItem.Text = "ツール";
            this.ツールToolStripMenuItem.Visible = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(928, 703);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.groupBoxSetup);
            this.Controls.Add(this.groupBoxQuestion);
            this.Controls.Add(this.lblModel);
            this.Controls.Add(this.cmbModel);
            this.Controls.Add(this.btnModelSettings);
            this.Controls.Add(this.chkNoPdf);
            this.Controls.Add(this.chkContinue);
            this.Font = new System.Drawing.Font("メイリオ", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PDF Question Answering with Gemini AI";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBoxSetup.ResumeLayout(false);
            this.groupBoxSetup.PerformLayout();
            this.groupBoxQuestion.ResumeLayout(false);
            this.groupBoxQuestion.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.RichTextBox ritAnswer;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.ToolStripStatusLabel tsslQuota;
		private System.Windows.Forms.ToolStripStatusLabel tsslRate;
		private System.Windows.Forms.CheckBox chkStrictMode;
		private System.Windows.Forms.CheckBox chkNoPdf;
		private System.Windows.Forms.CheckBox chkContinue;
		private System.Windows.Forms.CheckBox chkReadBasis;
		private System.Windows.Forms.CheckBox chkAutoRead;
		private System.Windows.Forms.Label lblModel;
		private System.Windows.Forms.ComboBox cmbModel;
		private System.Windows.Forms.Button btnModelSettings;
		private System.Windows.Forms.Button btnPreviewPayload;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem ファイルToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem 設定ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem aPIキーの設定ToolStripMenuItem;
		private System.Windows.Forms.Label lblApiKeyStatus;
		private System.Windows.Forms.ToolStripMenuItem ツールToolStripMenuItem;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
    }
}