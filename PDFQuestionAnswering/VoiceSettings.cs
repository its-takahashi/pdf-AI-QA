using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace PDFQuestionAnswering
{
	/// <summary>読み上げ音声の設定（%APPDATA%\PDFQuestionAnswering\voice.json に保存）</summary>
	public class VoiceSettings
	{
		public const string EngineEdge = "edge";
		public const string EngineWindows = "windows";
		public const string EngineVoicevox = "voicevox";

		public string Engine { get; set; } = EngineEdge;
		public string EdgeVoice { get; set; } = "ja-JP-NanamiNeural";
		public string WindowsVoice { get; set; } = "";   // 空なら既定の音声
		public int VoicevoxSpeaker { get; set; } = 3;    // VOICEVOXの声のID（3 = ずんだもん（ノーマル））
		public string VoicevoxSpeakerName { get; set; } = "ずんだもん（ノーマル）";
		public int RatePercent { get; set; } = 0;        // -50 ～ +100（%）
		public int PitchHz { get; set; } = 0;            // -50 ～ +50（Hz）
		public bool ReadBasis { get; set; } = true;      // 根拠も読み上げるか
		public bool AutoRead { get; set; } = false;      // 回答が出たら自動で朗読するか

		private static string FilePath =>
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
						 "PDFQuestionAnswering", "voice.json");

		public VoiceSettings Clone() => (VoiceSettings)MemberwiseClone();

		public static VoiceSettings Load()
		{
			try
			{
				if (File.Exists(FilePath))
				{
					var s = JsonConvert.DeserializeObject<VoiceSettings>(File.ReadAllText(FilePath, Encoding.UTF8));
					if (s != null)
					{
						s.RatePercent = Math.Max(-50, Math.Min(100, s.RatePercent));
						s.PitchHz = Math.Max(-50, Math.Min(50, s.PitchHz));
						if (s.Engine != EngineEdge && s.Engine != EngineWindows && s.Engine != EngineVoicevox) s.Engine = EngineEdge;
						// 選べる声はNanamiとKeitaだけ。それ以外が保存されていたらNanamiに戻す
						if (s.EdgeVoice != "ja-JP-NanamiNeural" && s.EdgeVoice != "ja-JP-KeitaNeural") s.EdgeVoice = "ja-JP-NanamiNeural";
						return s;
					}
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("voice.json 読込失敗: " + ex.Message);
			}
			return new VoiceSettings();
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
				System.Diagnostics.Debug.WriteLine("voice.json 保存失敗: " + ex.Message);
			}
		}
	}
}
