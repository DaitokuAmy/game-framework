using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace GameFramework.SaveSystems {
    /// <summary>
    /// ローカル保存用のクラス
    /// </summary>
    public class LocalSave {
        /// <summary>保存先のフォルダ</summary>
        private static readonly string SaveDirectory = Application.persistentDataPath;
        /// <summary>保存ファイルの拡張子</summary>
        private static readonly string FileExtension = ".sav";

        /// <summary>デフォルトAES鍵</summary>
        private static readonly string DefaultEncryptionKey = "ThisIsA32ByteLongEncryptionKey!!";
        /// <summary>デフォルトIV</summary>
        private static readonly string DefaultInitializationVector = "ThisIs16ByteIV!!";

        private readonly string _saveDirectoryPath;
        private readonly byte[] _encryptionKey;
        private readonly byte[] _initializationVector;

        /// <summary>
        /// 保存先のパスを取得
        /// </summary>
        private string GetSaveFilePath(string fileName) {
            return Path.Combine(_saveDirectoryPath, $"{fileName}{FileExtension}");
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="encryptionKey">暗号化キー</param>
        /// <param name="initializationVector">暗号化IV</param>
        /// <param name="folderPath">フォルダパス</param>
        public LocalSave(string encryptionKey, string initializationVector, string folderPath = "") {
            _saveDirectoryPath = string.IsNullOrWhiteSpace(folderPath) ? SaveDirectory : Path.Combine(SaveDirectory, folderPath);
            _encryptionKey = Encoding.UTF8.GetBytes(encryptionKey);
            _initializationVector = Encoding.UTF8.GetBytes(initializationVector);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LocalSave()
            : this(DefaultEncryptionKey, DefaultInitializationVector, string.Empty) {
        }

        /// <summary>
        /// セーブ
        /// </summary>
        public void Save<T>(T data, string fileName) {
            try {
                var json = JsonUtility.ToJson(data, true);
                var encrypted = Encrypt(json);
                File.WriteAllBytes(GetSaveFilePath(fileName), encrypted);
                Debug.Log($"Saved file '{fileName}'");
            }
            catch (Exception e) {
                Debug.LogError($"Failed to save: {e}");
            }
        }

        /// <summary>
        /// ロード
        /// </summary>
        public T Load<T>(string fileName, T defaultValue = default) where T : new() {
            var path = GetSaveFilePath(fileName);
            if (!File.Exists(path)) {
                return defaultValue;
            }

            try {
                var encrypted = File.ReadAllBytes(path);
                var json = Decrypt(encrypted);
                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception e) {
                Debug.LogError($"Failed to load: {e}");
                return defaultValue;
            }
        }

        /// <summary>
        /// ファイルの削除
        /// </summary>
        public void Delete(string fileName) {
            var path = GetSaveFilePath(fileName);
            if (File.Exists(path)) {
                File.Delete(path);
                Debug.Log($"Deleted save file '{fileName}'");
            }
        }

        /// <summary>
        /// ファイルが存在するかどうか
        /// </summary>
        public bool Exists(string fileName) {
            return File.Exists(GetSaveFilePath(fileName));
        }

        /// <summary>
        /// 暗号化
        /// </summary>
        private byte[] Encrypt(string plainText) {
            using var aes = Aes.Create();
            aes.Key = _encryptionKey;
            aes.IV = _initializationVector;

            using var ms = new MemoryStream();
            using var cryptoStream = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
            using var writer = new StreamWriter(cryptoStream);
            writer.Write(plainText);
            writer.Close();
            return ms.ToArray();
        }

        /// <summary>
        /// 複合化
        /// </summary>
        private string Decrypt(byte[] cipherData) {
            using var aes = Aes.Create();
            aes.Key = _encryptionKey;
            aes.IV = _initializationVector;

            using var ms = new MemoryStream(cipherData);
            using var cryptoStream = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var reader = new StreamReader(cryptoStream);
            return reader.ReadToEnd();
        }
    }
}