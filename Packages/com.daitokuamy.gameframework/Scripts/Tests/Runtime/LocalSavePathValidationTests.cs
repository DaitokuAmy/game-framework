using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace GameFramework.Tests {
    /// <summary>
    /// LocalSave のパス検証テスト
    /// </summary>
    public sealed class LocalSavePathValidationTests {
        private const string DefaultEncryptionKey = "ThisIsA32ByteLongEncryptionKey!!";

        private string _relativeSaveDirectory;
        private string _fullSaveDirectory;

        [SetUp]
        public void SetUp() {
            _relativeSaveDirectory = Path.Combine("LocalSaveTests", Guid.NewGuid().ToString("N"));
            _fullSaveDirectory = Path.GetFullPath(Path.Combine(Application.persistentDataPath, _relativeSaveDirectory));

            if (Directory.Exists(_fullSaveDirectory)) {
                Directory.Delete(_fullSaveDirectory, true);
            }
        }

        [TearDown]
        public void TearDown() {
            if (Directory.Exists(_fullSaveDirectory)) {
                Directory.Delete(_fullSaveDirectory, true);
            }
        }

        /// <summary>
        /// 正常な相対フォルダ配下へ保存できることを検証
        /// </summary>
        [Test]
        public void Save_WithValidRelativeFolderPath_WritesUnderPersistentDataPath() {
            using var localSave = new LocalSave(DefaultEncryptionKey, null, _relativeSaveDirectory);

            var data = new SavePayload { Value = 42 };
            localSave.Save(data, "profile");

            var saveFilePath = Path.Combine(_fullSaveDirectory, "profile.sav");
            Assert.That(File.Exists(saveFilePath), Is.True);

            var loaded = localSave.Load<SavePayload>("profile");
            Assert.That(loaded, Is.Not.Null);
            Assert.That(loaded.Value, Is.EqualTo(42));
        }

        /// <summary>
        /// 保存先から逸脱する folderPath を拒否することを検証
        /// </summary>
        [Test]
        public void Constructor_WithEscapingFolderPath_ThrowsArgumentException() {
            var escapingFolderPath = Path.Combine("..", Guid.NewGuid().ToString("N"));

            Assert.Throws<ArgumentException>(() => {
                using var _ = new LocalSave(DefaultEncryptionKey, null, escapingFolderPath);
            });
        }

        /// <summary>
        /// パス区切りを含む fileName を拒否することを検証
        /// </summary>
        [TestCase("nested/file")]
        [TestCase("nested\\file")]
        public void Save_WithPathSeparatedFileName_ThrowsArgumentException(string fileName) {
            using var localSave = new LocalSave(DefaultEncryptionKey, null, _relativeSaveDirectory);

            Assert.Throws<ArgumentException>(() => localSave.Save(new SavePayload { Value = 1 }, fileName));
        }

        /// <summary>
        /// テスト用保存データ
        /// </summary>
        [Serializable]
        private sealed class SavePayload {
            public int Value;
        }
    }
}
