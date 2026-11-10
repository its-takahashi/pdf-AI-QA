using System;
using System.Drawing;
using System.Windows.Forms;

namespace PDFQuestionAnswering
{
	partial class GeminiApiKeyForm
	{
		/// <summary>
		/// デザイナーで必要な変数です。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 使用中のリソースをすべてクリーンアップします。
		/// </summary>
		/// <param name="disposing">マネージ リソースを破棄する場合は true、アンマネージ リソースを破棄する場合は false です。</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows フォーム デザイナーで生成されたコード

		/// <summary>
		/// デザイナー サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディターで変更しないでください。
		/// </summary>
		private void InitializeComponent()
		{
			this.lblTitle = new System.Windows.Forms.Label();
			this.lblInfo = new System.Windows.Forms.Label();
			this.txtGemini = new System.Windows.Forms.TextBox();
			this.chkShow = new System.Windows.Forms.CheckBox();
			this.btnSave = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// lblTitle
			// 
			this.lblTitle.AutoSize = true;
			this.lblTitle.Font = new System.Drawing.Font("メイリオ", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.lblTitle.Location = new System.Drawing.Point(19, 24);
			this.lblTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblTitle.Name = "lblTitle";
			this.lblTitle.Size = new System.Drawing.Size(516, 20);
			this.lblTitle.TabIndex = 0;
			this.lblTitle.Text = "Gemini API Key を入力してください（このPCのこのユーザーでのみ復号されます）";
			// 
			// lblInfo
			// 
			this.lblInfo.AutoSize = true;
			this.lblInfo.Location = new System.Drawing.Point(21, 66);
			this.lblInfo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblInfo.Name = "lblInfo";
			this.lblInfo.Size = new System.Drawing.Size(521, 18);
			this.lblInfo.TabIndex = 1;
			this.lblInfo.Text = "保存時は Windows の DPAPI（CurrentUser）で暗号化され、settings.json に保存されます。";
			// 
			// txtGemini
			// 
			this.txtGemini.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtGemini.Location = new System.Drawing.Point(23, 117);
			this.txtGemini.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.txtGemini.Name = "txtGemini";
			this.txtGemini.Size = new System.Drawing.Size(548, 25);
			this.txtGemini.TabIndex = 2;
			this.txtGemini.UseSystemPasswordChar = true;
			// 
			// chkShow
			// 
			this.chkShow.AutoSize = true;
			this.chkShow.Location = new System.Drawing.Point(23, 162);
			this.chkShow.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.chkShow.Name = "chkShow";
			this.chkShow.Size = new System.Drawing.Size(231, 22);
			this.chkShow.TabIndex = 3;
			this.chkShow.Text = "表示する（入力内容をマスクしない）";
			this.chkShow.UseVisualStyleBackColor = true;
			this.chkShow.CheckedChanged += new System.EventHandler(this.chkShow_CheckedChanged);
			// 
			// btnSave
			// 
			this.btnSave.Location = new System.Drawing.Point(350, 195);
			this.btnSave.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.btnSave.Name = "btnSave";
			this.btnSave.Size = new System.Drawing.Size(105, 34);
			this.btnSave.TabIndex = 4;
			this.btnSave.Text = "保存";
			this.btnSave.UseVisualStyleBackColor = true;
			this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(467, 195);
			this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(105, 34);
			this.btnCancel.TabIndex = 5;
			this.btnCancel.Text = "キャンセル";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// GeminiApiKeyForm
			// 
			this.AcceptButton = this.btnSave;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 18F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(607, 243);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnSave);
			this.Controls.Add(this.chkShow);
			this.Controls.Add(this.txtGemini);
			this.Controls.Add(this.lblInfo);
			this.Controls.Add(this.lblTitle);
			this.Font = new System.Drawing.Font("メイリオ", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "GeminiApiKeyForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Gemini API Key 設定";
			this.Load += new System.EventHandler(this.GeminiApiKeyForm_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label lblTitle;
		private System.Windows.Forms.Label lblInfo;
		private System.Windows.Forms.TextBox txtGemini;
		private System.Windows.Forms.CheckBox chkShow;
		private System.Windows.Forms.Button btnSave;
		private System.Windows.Forms.Button btnCancel;
	}
}
