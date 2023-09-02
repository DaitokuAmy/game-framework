using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.AssetSystems;
using GameFramework.Core;
            
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

namespace SampleGame.VfxViewer {
    /// <summary>
    /// モデルビューア用のリポジトリ
    /// </summary>
    public class VfxViewerRepository : IDisposable {
        private readonly AssetManager _assetManager;
        private readonly DisposableScope _unloadScope;
        private readonly PoolAssetStorage<PreviewVfxSetupData> _actorSetupDataStorage;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public VfxViewerRepository(AssetManager assetManager) {
            _assetManager = assetManager;
            _unloadScope = new DisposableScope();
            _actorSetupDataStorage = new PoolAssetStorage<PreviewVfxSetupData>(_assetManager, 2);
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            _actorSetupDataStorage.Dispose();
            _unloadScope.Dispose();
        }

        /// <summary>
        /// ActorSetupDataの読み込み
        /// </summary>
        public async UniTask<VfxViewerSetupData> LoadSetupDataAsync(CancellationToken ct) {
            var setupData = await new VfxViewerSetupGameRequest()
                .LoadAsync(_assetManager, _unloadScope)
                .ToUniTask(cancellationToken:ct);
            
#if UNITY_EDITOR
            // Editor経由であれば、AssetDatabase経由で該当ファイルの更新を実行する
            var actorSetupDataGuids = AssetDatabase.FindAssets($"t:{nameof(PreviewVfxSetupData)}");
            var actorSetupDataIds = new List<string>();
            foreach (var guid in actorSetupDataGuids) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var fileName = Path.GetFileNameWithoutExtension(path);
                var id = fileName.Replace("dat_preview_actor_setup_", "");
                actorSetupDataIds.Add(id);
            }

            setupData.vfxDataIds = actorSetupDataIds.ToArray();
            EditorUtility.SetDirty(setupData);
#endif

            return setupData;
        }

        /// <summary>
        /// ActorDataの読み込み
        /// </summary>
        public UniTask<PreviewVfxSetupData> LoadActorSetupDataAsync(string setupDataId, CancellationToken ct) {
            return _actorSetupDataStorage.LoadAssetAsync(new PreviewVfxSetupGameRequest(setupDataId))
                .ToUniTask(cancellationToken:ct);
        }
    }
}
