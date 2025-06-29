using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace GameFramework.SaveSystems {
    /// <summary>
    /// ローカル保存用のクラス
    /// </summary>
    public static class LocalSave {
        /// <summary>保存先のフォルダ</summary>
        private static readonly string SaveDirectory = Application.persistentDataPath;
        /// <summary>保存ファイルの拡張子</summary>
        private static readonly string FileExtension = ".sav";

        /// <summary>デフォルトAES鍵</summary>
        private static readonly byte[] DefaultEncryptionKey = Encoding.UTF8.GetBytes("ThisIsA32ByteLongEncryptionKey!!"); // 32バイト
        /// <summary>デフォルトIV</summary>
        private static readonly byte[] DefaultInitializationVector = Encoding.UTF8.GetBytes("ThisIs16ByteIV!!"); // 16バイト
        
        private static byte[] s_encryptionKey = DefaultEncryptionKey;
        private static byte[] s_initializationVector = DefaultInitializationVector;

        /// <summary>
        /// 保存先のパスを取得
        /// </summary>
        private static string GetSaveFilePath(string fileName) {
            return Path.Combine(SaveDirectory, $"{fileName}{FileExtension}");
        }

        /// <summary>
        /// AES暗号のキーを設定
        /// </summary>
        public static void SetAesKeys(string encryptionKey, string initializationVector) {
            s_encryptionKey = Encoding.UTF8.GetBytes(encryptionKey);
            s_initializationVector = Encoding.UTF8.GetBytes(initializationVector);
        }

        /// <summary>
        /// セーブ
        /// </summary>
        public static void Save<T>(T data, string fileName) {
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
        public static T Load<T>(string fileName) where T : new() {
            var path = GetSaveFilePath(fileName);
            if (!File.Exists(path)) {
                Debug.LogWarning($"No found in save file '{fileName}', returning default");
                return new T();
            }

            try {
                var encrypted = File.ReadAllBytes(path);
                var json = Decrypt(encrypted);
                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception e) {
                Debug.LogError($"Failed to load: {e}");
                return new T();
            }
        }

        /// <summary>
        /// ファイルの削除
        /// </summary>
        public static void Delete(string fileName) {
            var path = GetSaveFilePath(fileName);
            if (File.Exists(path)) {
                File.Delete(path);
                Debug.Log($"Deleted save file '{fileName}'");
            }
        }

        /// <summary>
        /// ファイルが存在するかどうか
        /// </summary>
        public static bool Exists(string fileName) {
            return File.Exists(GetSaveFilePath(fileName));
        }


        /// <summary>
        /// 暗号化
        /// </summary>
        private static byte[] Encrypt(string plainText) {
            using var aes = Aes.Create();
            aes.Key = s_encryptionKey;
            aes.IV = s_initializationVector;

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
        private static string Decrypt(byte[] cipherData) {
            using var aes = Aes.Create();
            aes.Key = s_encryptionKey;
            aes.IV = s_initializationVector;

            using var ms = new MemoryStream(cipherData);
            using var cryptoStream = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var reader = new StreamReader(cryptoStream);
            return reader.ReadToEnd();
        }
    }
}