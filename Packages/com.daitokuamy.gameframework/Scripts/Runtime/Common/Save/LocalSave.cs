using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameFramework {
    /// <summary>
    /// ローカル保存用のクラス
    /// </summary>
    public sealed class LocalSave : IDisposable {
        /// <summary>暗号化データヘッダー先頭識別子1</summary>
        private const byte HeaderMagic1 = (byte)'L';

        /// <summary>暗号化データヘッダー先頭識別子2</summary>
        private const byte HeaderMagic2 = (byte)'S';

        /// <summary>暗号化データバージョン</summary>
        private const byte EncryptionFormatVersion = 1;

        /// <summary>暗号化データヘッダーサイズ</summary>
        private const int HeaderSize = 4;

        /// <summary>保存先のフォルダ</summary>
        private static readonly string SaveDirectory = Path.GetFullPath(Application.persistentDataPath);

        /// <summary>保存ファイルの拡張子</summary>
        private static readonly string FileExtension = ".sav";

        /// <summary>デフォルトAES鍵</summary>
        private static readonly string DefaultEncryptionKey = "ThisIsA32ByteLongEncryptionKey!!";

        private readonly string _saveDirectoryPath;
        private readonly byte[] _encryptionKey;
        private readonly ISerializer _serializer;
        private readonly SemaphoreSlim _fileOperationSemaphore = new(1, 1);
        private volatile bool _isDisposed;
        private int _disposeStarted;

        /// <summary>
        /// 保存先のパスを取得
        /// </summary>
        private string GetSaveFilePath(string fileName) {
            ValidateFileName(fileName);
            return Path.Combine(_saveDirectoryPath, $"{fileName}{FileExtension}");
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="encryptionKey">暗号化キー</param>
        /// <param name="serializer">シリアライザー</param>
        /// <param name="folderPath">フォルダパス</param>
        public LocalSave(string encryptionKey, ISerializer serializer = null, string folderPath = "") {
            _saveDirectoryPath = ResolveSaveDirectory(folderPath);
            Directory.CreateDirectory(_saveDirectoryPath);
            _encryptionKey = Encoding.UTF8.GetBytes(encryptionKey);
            _serializer = serializer ?? new JsonSerializer();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LocalSave()
            : this(DefaultEncryptionKey, null, string.Empty) {
        }

        /// <summary>
        /// リソースを破棄する
        /// </summary>
        public void Dispose() {
            if (Interlocked.Exchange(ref _disposeStarted, 1) != 0) {
                return;
            }

            _isDisposed = true;
            _fileOperationSemaphore.Wait();
            _fileOperationSemaphore.Dispose();
        }

        /// <summary>
        /// セーブ
        /// </summary>
        public void Save<T>(T data, string fileName) {
            WaitSemaphore();
            try {
                Directory.CreateDirectory(_saveDirectoryPath);
                var bytes = _serializer.Serialize(data);
                var encrypted = Encrypt(bytes);
                File.WriteAllBytes(GetSaveFilePath(fileName), encrypted);
            }
            finally {
                _fileOperationSemaphore.Release();
            }
        }

        /// <summary>
        /// セーブ
        /// </summary>
        public async UniTask SaveAsync<T>(T data, string fileName, CancellationToken ct = default) {
            try {
                await WaitSemaphoreAsync(ct);
                try {
                    ct.ThrowIfCancellationRequested();
                    Directory.CreateDirectory(_saveDirectoryPath);
                    var bytes = _serializer.Serialize(data);
                    var path = GetSaveFilePath(fileName);
                    await UniTask.RunOnThreadPool(() => {
                        ct.ThrowIfCancellationRequested();
                        var encrypted = Encrypt(bytes);
                        File.WriteAllBytes(path, encrypted);
                    }, configureAwait: false, cancellationToken: ct);
                }
                finally {
                    _fileOperationSemaphore.Release();
                }
            }
            finally {
                await UniTask.SwitchToMainThread();
            }
        }

        /// <summary>
        /// セーブ
        /// </summary>
        public void Save(byte[] bytes, string fileName) {
            WaitSemaphore();
            try {
                Directory.CreateDirectory(_saveDirectoryPath);
                var encrypted = Encrypt(bytes);
                File.WriteAllBytes(GetSaveFilePath(fileName), encrypted);
            }
            finally {
                _fileOperationSemaphore.Release();
            }
        }

        /// <summary>
        /// セーブ
        /// </summary>
        public async UniTask SaveAsync(byte[] bytes, string fileName, CancellationToken ct = default) {
            try {
                await WaitSemaphoreAsync(ct);
                try {
                    ct.ThrowIfCancellationRequested();
                    Directory.CreateDirectory(_saveDirectoryPath);
                    var path = GetSaveFilePath(fileName);
                    await UniTask.RunOnThreadPool(() => {
                        ct.ThrowIfCancellationRequested();
                        var encrypted = Encrypt(bytes);
                        File.WriteAllBytes(path, encrypted);
                    }, configureAwait: false, cancellationToken: ct);
                }
                finally {
                    _fileOperationSemaphore.Release();
                }
            }
            finally {
                await UniTask.SwitchToMainThread();
            }
        }

        /// <summary>
        /// ロード
        /// </summary>
        public T Load<T>(string fileName, T defaultValue = default) {
            WaitSemaphore();
            try {
                var path = GetSaveFilePath(fileName);
                if (!File.Exists(path)) {
                    return defaultValue;
                }

                var encrypted = File.ReadAllBytes(path);
                var bytes = Decrypt(encrypted);
                return _serializer.Deserialize<T>(bytes);
            }
            finally {
                _fileOperationSemaphore.Release();
            }
        }

        /// <summary>
        /// ロード
        /// </summary>
        public async UniTask<T> LoadAsync<T>(string fileName, T defaultValue = default, CancellationToken ct = default) {
            try {
                await WaitSemaphoreAsync(ct);
                try {
                    ct.ThrowIfCancellationRequested();
                    var path = GetSaveFilePath(fileName);
                    if (!File.Exists(path)) {
                        return defaultValue;
                    }

                    ct.ThrowIfCancellationRequested();
                    var bytes = await UniTask.RunOnThreadPool(() => {
                        ct.ThrowIfCancellationRequested();
                        var encrypted = File.ReadAllBytes(path);
                        return Decrypt(encrypted);
                    }, configureAwait: false, cancellationToken: ct);

                    ct.ThrowIfCancellationRequested();
                    return _serializer.Deserialize<T>(bytes);
                }
                finally {
                    _fileOperationSemaphore.Release();
                }
            }
            finally {
                await UniTask.SwitchToMainThread();
            }
        }

        /// <summary>
        /// ロード
        /// </summary>
        public byte[] LoadForBytes(string fileName) {
            WaitSemaphore();
            try {
                var path = GetSaveFilePath(fileName);
                if (!File.Exists(path)) {
                    return Array.Empty<byte>();
                }

                var encrypted = File.ReadAllBytes(path);
                return Decrypt(encrypted);
            }
            finally {
                _fileOperationSemaphore.Release();
            }
        }

        /// <summary>
        /// ロード
        /// </summary>
        public async UniTask<byte[]> LoadForBytesAsync(string fileName, CancellationToken ct = default) {
            try {
                await WaitSemaphoreAsync(ct);
                try {
                    ct.ThrowIfCancellationRequested();
                    var path = GetSaveFilePath(fileName);
                    if (!File.Exists(path)) {
                        return Array.Empty<byte>();
                    }

                    return await UniTask.RunOnThreadPool(() => {
                        ct.ThrowIfCancellationRequested();
                        var encrypted = File.ReadAllBytes(path);
                        return Decrypt(encrypted);
                    }, configureAwait: false, cancellationToken: ct);
                }
                finally {
                    _fileOperationSemaphore.Release();
                }
            }
            finally {
                await UniTask.SwitchToMainThread();
            }
        }

        /// <summary>
        /// ファイルの削除
        /// </summary>
        public void Delete(string fileName) {
            WaitSemaphore();
            try {
                var path = GetSaveFilePath(fileName);
                if (File.Exists(path)) {
                    File.Delete(path);
                }
            }
            finally {
                _fileOperationSemaphore.Release();
            }
        }

        /// <summary>
        /// ファイルが存在するかどうか
        /// </summary>
        public bool Exists(string fileName) {
            WaitSemaphore();
            try {
                return File.Exists(GetSaveFilePath(fileName));
            }
            finally {
                _fileOperationSemaphore.Release();
            }
        }

        /// <summary>
        /// セマフォ取得
        /// </summary>
        private void WaitSemaphore() {
            ThrowIfDisposed();
            _fileOperationSemaphore.Wait();
        }

        /// <summary>
        /// セマフォ取得
        /// </summary>
        private async UniTask WaitSemaphoreAsync(CancellationToken ct) {
            ThrowIfDisposed();
            await _fileOperationSemaphore.WaitAsync(ct);
        }

        /// <summary>
        /// 破棄済みかどうかを確認する
        /// </summary>
        private void ThrowIfDisposed() {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(LocalSave));
            }
        }

        /// <summary>
        /// 暗号化
        /// </summary>
        private byte[] Encrypt(byte[] plainData) {
            using var aes = Aes.Create();
            aes.Key = _encryptionKey;
            aes.GenerateIV();

            using var ms = new MemoryStream();
            using var cryptoStream = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
            cryptoStream.Write(plainData, 0, plainData.Length);
            cryptoStream.FlushFinalBlock();

            var encrypted = ms.ToArray();
            var payload = new byte[HeaderSize + aes.IV.Length + encrypted.Length];
            payload[0] = HeaderMagic1;
            payload[1] = HeaderMagic2;
            payload[2] = EncryptionFormatVersion;
            payload[3] = (byte)aes.IV.Length;
            Buffer.BlockCopy(aes.IV, 0, payload, HeaderSize, aes.IV.Length);
            Buffer.BlockCopy(encrypted, 0, payload, HeaderSize + aes.IV.Length, encrypted.Length);
            return payload;
        }

        /// <summary>
        /// 複合化
        /// </summary>
        private byte[] Decrypt(byte[] cipherData) {
            var (initializationVector, encryptedBody) = ParseCurrentFormatPayload(cipherData);

            using var aes = Aes.Create();
            aes.Key = _encryptionKey;
            aes.IV = initializationVector;

            using var ms = new MemoryStream(encryptedBody);
            using var cryptoStream = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var output = new MemoryStream();
            cryptoStream.CopyTo(output);
            return output.ToArray();
        }

        /// <summary>
        /// 暗号化済みペイロードを解析する
        /// </summary>
        private static (byte[] initializationVector, byte[] encryptedBody) ParseCurrentFormatPayload(byte[] cipherData) {
            if (cipherData.Length < HeaderSize) {
                throw new CryptographicException("Encrypted payload header is invalid");
            }

            if (cipherData[0] != HeaderMagic1 || cipherData[1] != HeaderMagic2) {
                throw new CryptographicException("Encrypted payload header is invalid");
            }

            if (cipherData[2] != EncryptionFormatVersion) {
                throw new CryptographicException($"Unsupported encryption format version: {cipherData[2]}");
            }

            var initializationVectorLength = cipherData[3];
            var encryptedOffset = HeaderSize + initializationVectorLength;
            if (initializationVectorLength <= 0 || encryptedOffset > cipherData.Length) {
                throw new CryptographicException("Encrypted payload header is invalid");
            }

            var initializationVector = new byte[initializationVectorLength];
            var encryptedBody = new byte[cipherData.Length - encryptedOffset];
            Buffer.BlockCopy(cipherData, HeaderSize, initializationVector, 0, initializationVectorLength);
            Buffer.BlockCopy(cipherData, encryptedOffset, encryptedBody, 0, encryptedBody.Length);
            return (initializationVector, encryptedBody);
        }

        /// <summary>
        /// 保存先ディレクトリを解決
        /// </summary>
        private static string ResolveSaveDirectory(string folderPath) {
            if (string.IsNullOrWhiteSpace(folderPath)) {
                return SaveDirectory;
            }

            var fullPath = Path.GetFullPath(Path.Combine(SaveDirectory, folderPath));
            if (!IsPathWithinDirectory(fullPath, SaveDirectory)) {
                throw new ArgumentException($"Invalid folder path. [{folderPath}]", nameof(folderPath));
            }

            return fullPath;
        }

        /// <summary>
        /// 保存ファイル名を検証
        /// </summary>
        private static void ValidateFileName(string fileName) {
            if (string.IsNullOrWhiteSpace(fileName)) {
                throw new ArgumentException("File name is empty.", nameof(fileName));
            }

            if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) {
                throw new ArgumentException($"Invalid file name. [{fileName}]", nameof(fileName));
            }

            if (fileName.Contains(Path.DirectorySeparatorChar) || fileName.Contains(Path.AltDirectorySeparatorChar)) {
                throw new ArgumentException($"File name must not include path separators. [{fileName}]", nameof(fileName));
            }
        }

        /// <summary>
        /// 指定パスが保存先配下かどうか
        /// </summary>
        private static bool IsPathWithinDirectory(string path, string rootDirectory) {
            var relativePath = Path.GetRelativePath(rootDirectory, path);
            if (Path.IsPathRooted(relativePath)) {
                return false;
            }

            return relativePath != ".." && !relativePath.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !relativePath.StartsWith($"..{Path.AltDirectorySeparatorChar}", StringComparison.Ordinal);
        }
    }
}
