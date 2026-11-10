using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace PDFQuestionAnswering
{
	/// <summary>画面の設定（%APPDATA%\PDFQuestionAnswering\ui.json）。厳格モードのチェックを覚える。</summary>
	public class UiSettings
	{
		public bool StrictMode { get; set; } = false;
		public bool ContinueChat { get; set; } = false;

		private static string FilePath =>
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
						 "PDFQuestionAnswering", "ui.json");

		public static UiSettings Load()
		{
			try
			{
				if (File.Exists(FilePath))
				{
					var s = JsonConvert.DeserializeObject<UiSettings>(File.ReadAllText(FilePath, Encoding.UTF8));
					if (s != null) return s;
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("ui.json 読込失敗: " + ex.Message);
			}
			return new UiSettings();
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
				System.Diagnostics.Debug.WriteLine("ui.json 保存失敗: " + ex.Message);
			}
		}
	}
}
