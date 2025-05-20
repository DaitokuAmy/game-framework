using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.Core;
using SampleGame.Infrastructure;
using UnityEngine.SceneManagement;

namespace SampleGame.Presentation.ModelViewer {
    /// <summary>
    /// 環境の管理用
    /// </summary>
    public class EnvironmentManager : IDisposable {
        private string _currentAssetKey = "";
        private DisposableScope _fieldScope = new();
        private EnvironmentSceneRepository _environmentSceneRepository;

        /// <summary>現在使用中のScene</summary>
        public Scene CurrentScene { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EnvironmentManager() {
            _environmentSceneRepository = Services.Resolve<EnvironmentSceneRepository>();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            RemoveEnvironment();
        }

        /// <summary>
        /// 環境の変更
        /// </summary>
        public async UniTask<Scene> ChangeEnvironmentAsync(string assetKey, CancellationToken ct) {
            ct.ThrowIfCancellationRequested();

            RemoveEnvironment();

            if (string.IsNullOrEmpty(assetKey)) {
                return default;
            }

            if (!string.IsNullOrEmpty(assetKey)) {
                CurrentScene = await LoadEnvironmentAsync(assetKey, ct);
            }

            return CurrentScene;
        }

        /// <summary>
        /// 環境の削除
        /// </summary>
        public void RemoveEnvironment() {
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
        /// 環境の読み込み
        /// </summary>
        private UniTask<Scene> LoadEnvironmentAsync(string assetKey, CancellationToken ct) {
            _currentAssetKey = assetKey;

            if (assetKey.StartsWith("fld")) {
                return _environmentSceneRepository.LoadFieldSceneAsync(assetKey, ct);
            }

            throw new KeyNotFoundException(assetKey);
        }
    }
}