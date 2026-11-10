using PDFQuestionAnswering.Properties;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using Newtonsoft.Json.Linq;     // ← 追加
using System.Threading.Tasks;
using System.Windows.Forms;


namespace PDFQuestionAnswering
{
	//settings.json に保存されたAPIキーは、そのPCにそのユーザーでログインしている状態でのみプログラムが復号できる、
	//安全な状態になっています。
	//「現在ログインしているWindowsユーザーアカウントの情報を使って暗号化・復号する」。
	public class AppSettings
	{
		// --- シングルトンパターン ---
		// アプリケーション全体で設定クラスのインスタンスを1つに保つための仕組み
		private static readonly Lazy<AppSettings> _instance = new Lazy<AppSettings>(() => new AppSettings());
		public static AppSettings Instance => _instance.Value;
		// --- ---

		private readonly string _filePath;
		private static readonly byte[] _entropy = Encoding.Unicode.GetBytes("KTMapSearchSalt"); // 暗号化のための追加情報

		// 設定として保存するプロパティ
		public string GoogleMapsApiKey { get; set; } = string.Empty;
		public string GeminiApiKey { get; set; } = string.Empty;

		// コンストラクタ（クラスが最初に作られるときに一度だけ呼ばれる）
		private AppSettings()
		{
			// 設定ファイルの保存場所を決定（例: C:\Users\ユーザー名\AppData\Roaming\KT_MapSearch\settings.json）
			var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			var appDir = Path.Combine(appDataPath, "KT_MapSearch");
			Directory.CreateDirectory(appDir); // フォルダがなければ作成
			_filePath = Path.Combine(appDir, "settings.json");

			Load(); // アプリ起動時に設定を読み込む
		}

		/// <summary>
		/// 設定をファイルに保存します。APIキーは暗号化されます。
		/// </summary>
		public void Save()
		{
			//try
			//{
			//    // APIキーを暗号化
			//    var encryptedKey = EncryptString(GoogleMapsApiKey);

			//    // 保存するデータを作成
			//    var dataToSave = new { EncryptedGoogleMapsApiKey = encryptedKey };

			//    var json = JsonSerializer.Serialize(dataToSave, new JsonSerializerOptions { WriteIndented = true });
			//    File.WriteAllText(_filePath, json);
			//}
			//catch (Exception ex)
			//{
			//    MessageBox.Show($"設定の保存に失敗しました。\n{ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
			//}
			try
			{
				var encMaps = EncryptString(GoogleMapsApiKey);
				var encGemini = EncryptString(GeminiApiKey);

				var dataToSave = new
				{
					EncryptedGoogleMapsApiKey = encMaps,
					EncryptedGeminiApiKey = encGemini
				};

				var json = JsonConvert.SerializeObject(dataToSave, Formatting.Indented);  // ← Newtonsoft.Json で保存
				File.WriteAllText(_filePath, json, Encoding.UTF8);

			}
			catch (Exception ex)
			{
				MessageBox.Show($"設定の保存に失敗しました。\n{ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		/// <summary>
		/// ファイルから設定を読み込みます。APIキーは復号されます。
		/// </summary>
		private void Load()
		{
			//if (!File.Exists(_filePath)) return; // ファイルがなければ何もしない

			//try
			//{
			//    var json = File.ReadAllText(_filePath);
			//    var data = JsonSerializer.Deserialize<JsonElement>(json);

			//    // 暗号化されたキーを読み込み、復号する
			//    if (data.TryGetProperty("EncryptedGoogleMapsApiKey", out var encryptedKeyProperty) && encryptedKeyProperty.GetString() is string encryptedKey)
			//    {
			//        GoogleMapsApiKey = DecryptString(encryptedKey);
			//    }
			//}
			//catch (Exception ex)
			//{
			//    // 読み込みに失敗してもエラーメッセージは表示せず、デフォルト値のまま続行
			//    // Console.WriteLine($"設定の読み込みに失敗: {ex.Message}");
			//}
			if (!File.Exists(_filePath)) return;

			try
			{
				var json = File.ReadAllText(_filePath, Encoding.UTF8);
				var data = JsonConvert.DeserializeObject<JObject>(json);

				var encMaps = (string)data["EncryptedGoogleMapsApiKey"];
				if (!string.IsNullOrEmpty(encMaps))
				{
					GoogleMapsApiKey = DecryptString(encMaps);
				}

				// ★Geminiキー（新規/任意）
				var encGem = (string)data["EncryptedGeminiApiKey"];
				if (!string.IsNullOrEmpty(encGem))
				{
					GeminiApiKey = DecryptString(encGem);
				}
			}
			catch
			{
				// 壊れていても黙って既定値で続行
			}
		}

		// --- 文字列の暗号化・復号（Windowsの機能を利用） ---
		private string EncryptString(string input)
		{
			if (string.IsNullOrEmpty(input)) return string.Empty;
			var data = Encoding.Unicode.GetBytes(input);
			// 現在のユーザーアカウントの情報を使って暗号化
			var encryptedData = ProtectedData.Protect(data, _entropy, DataProtectionScope.CurrentUser);
			return Convert.ToBase64String(encryptedData);
		}

		private string DecryptString(string encryptedInput)
		{
			if (string.IsNullOrEmpty(encryptedInput)) return string.Empty;
			var encryptedData = Convert.FromBase64String(encryptedInput);
			// 暗号化した時と同じユーザーアカウントでしか復号できない
			var data = ProtectedData.Unprotect(encryptedData, _entropy, DataProtectionScope.CurrentUser);
			return Encoding.Unicode.GetString(data);
		}

	}
}
