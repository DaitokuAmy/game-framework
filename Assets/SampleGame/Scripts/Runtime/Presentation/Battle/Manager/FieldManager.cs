using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.Core;
using SampleGame.Infrastructure;
using UnityEngine.SceneManagement;

namespace SampleGame.Presentation.Battle {
    /// <summary>
    /// フィールド管理用
    /// </summary>
    public class FieldManager : IDisposable {
        private string _currentAssetKey = "";
        private DisposableScope _fieldScope = new();
        private EnvironmentSceneRepository _environmentSceneRepository;

        /// <summary>現在使用中のScene</summary>
        public Scene CurrentScene { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FieldManager() {
            _environmentSceneRepository = Services.Get<EnvironmentSceneRepository>();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            RemoveField();
        }

        /// <summary>
        /// フィールドの変更
        /// </summary>
        public async UniTask<Scene> ChangeFieldAsync(string assetKey, CancellationToken ct) {
            ct.ThrowIfCancellationRequested();

            RemoveField();

            if (string.IsNullOrEmpty(assetKey)) {
                return default;
            }

            if (!string.IsNullOrEmpty(assetKey)) {
                CurrentScene = await LoadFieldAsync(assetKey, ct);
            }

            return CurrentScene;
        }

        /// <summary>
        /// 環境の削除
        /// </summary>
        public void RemoveField() {
            if (!CurrentScene.IsValid()) {
                return;
            }

            if (_currentAssetKey.StartsWith("fld")) {
                _environmentSceneRepository.UnloadFieldScene(_currentAssetKey);
            }

            _fieldScope.Clear();
            _currentAssetKey = "";
            CurrentScene = default;
        }

        /// <summary>
        /// フィールドの読み込み
        /// </summary>
        private UniTask<Scene> LoadFieldAsync(string assetKey, CancellationToken ct) {
            _currentAssetKey = assetKey;

            if (assetKey.StartsWith("fld")) {
                return _environmentSceneRepository.LoadFieldSceneAsync(assetKey, ct);
            }

            throw new KeyNotFoundException(assetKey);
        }
    }
}