using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SampleGame.VfxViewer {
    /// <summary>
    /// 環境の管理用
    /// </summary>
    public class EnvironmentManager : IDisposable {
        // Field生成中のScope
        private DisposableScope _fieldScope = new();

        // 現在使用中のFieldScene
        public Scene CurrentFieldScene { get; private set; }
        // 現在使用中のLight
        public Light CurrentLight { get; private set; }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            RemoveEnvironment();
        }

        /// <summary>
        /// 環境の変更
        /// </summary>
        public async UniTask ChangeEnvironmentAsync(string assetId, CancellationToken ct) {
            ct.ThrowIfCancellationRequested();

            RemoveEnvironment();

            CurrentFieldScene = await new FieldSceneAssetRequest(assetId)
                .LoadAsync(true, _fieldScope, ct);
            CurrentLight = FindDirectionalLight(CurrentFieldScene);
        }

        /// <summary>
        /// 環境の削除
        /// </summary>
        public void RemoveEnvironment() {
            if (!CurrentFieldScene.IsValid()) {
                return;
            }

            SceneManager.UnloadSceneAsync(CurrentFieldScene);
            _fieldScope.Clear();
            CurrentFieldScene = new Scene();
            CurrentLight = null;
        }

        /// <summary>
        /// DirectionalLightを探す
        /// </summary>
        private Light FindDirectionalLight(Scene scene) {
            foreach (var rootObj in scene.GetRootGameObjects()) {
                var light = rootObj.GetComponentsInChildren<Light>()
                    .FirstOrDefault(x => x.type == LightType.Directional);
                if (light == null) {
                    continue;
                }

                return light;
            }

            return null;
        }
    }
}