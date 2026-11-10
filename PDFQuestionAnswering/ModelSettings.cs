using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PDFQuestionAnswering
{
	/// <summary>使うGeminiモデルと、モデルごとの1日の上限回数（%APPDATA%\PDFQuestionAnswering\model.json）</summary>
	public class ModelSettings
	{
		public string Model { get; set; } = "";
		public Dictionary<string, int> Quotas { get; set; } = new Dictionary<string, int>
		{
			// AI Studio の「レート制限」画面で確認した無料枠の1日の上限（変更は「モデル設定」から）
			{ "gemini-3.5-flash-lite", 500 },
			{ "gemini-3.6-flash", 20 },
			{ "gemini-3.8-flash", 20 },
			{ "gemini-3-flash-preview", 20 },
		};

		private static string FilePath =>
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
						 "PDFQuestionAnswering", "model.json");

		public int QuotaFor(string model, int fallback)
		{
			return (model != null && Quotas != null && Quotas.TryGetValue(model, out var q) && q > 0) ? q : fallback;
		}

		public static ModelSettings Load()
		{
			try
			{
				if (File.Exists(FilePath))
				{
					var s = JsonConvert.DeserializeObject<ModelSettings>(File.ReadAllText(FilePath, Encoding.UTF8));
					if (s != null)
					{
						if (s.Quotas == null) s.Quotas = new Dictionary<string, int>();
						if (s.Model == null) s.Model = "";
						return s;
					}
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("model.json 読込失敗: " + ex.Message);
			}
			return new ModelSettings();
		}

		public void Save()
		{
			try
			{
				Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
				File.WriteAllText(FilePath, JsonConvert.SerializeObject(this, Formatting.Indented), Encoding.UTF8);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("model.json 保存失敗: " + ex.Message);
			}
		}
	}
}
