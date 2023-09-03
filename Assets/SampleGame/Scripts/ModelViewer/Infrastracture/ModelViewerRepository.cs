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

namespace SampleGame.ModelViewer {
    /// <summary>
    /// モデルビューア用のリポジトリ
    /// </summary>
    public class ModelViewerRepository : IDisposable {
        private readonly AssetManager _assetManager;
        private readonly DisposableScope _unloadScope;
        private readonly PoolAssetStorage<PreviewActorSetupData> _actorSetupDataStorage;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ModelViewerRepository(AssetManager assetManager) {
            _assetManager = assetManager;
            _unloadScope = new DisposableScope();
            _actorSetupDataStorage = new PoolAssetStorage<PreviewActorSetupData>(_assetManager, 2);
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
        public async UniTask<ModelViewerSetupData> LoadSetupDataAsync(CancellationToken ct) {
            var setupData = await new ModelViewerSetupGameRequest()
                .LoadAsync(_assetManager, _unloadScope)
                .ToUniTask(cancellationToken:ct);
            
#if UNITY_EDITOR
            // Editor経由であれば、AssetDatabase経由で該当ファイルの更新を実行する
            var actorSetupDataGuids = AssetDatabase.FindAssets($"t:{nameof(PreviewActorSetupData)}");
            var actorSetupDataIds = new List<string>();
            foreach (var guid in actorSetupDataGuids) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var fileName = Path.GetFileNameWithoutExtension(path);
                var id = fileName.Replace("dat_preview_actor_setup_", "");
                actorSetupDataIds.Add(id);
            }

            setupData.actorDataIds = actorSetupDataIds.ToArray();
            EditorUtility.SetDirty(setupData);
#endif

            return setupData;
        }

        /// <summary>
        /// ActorDataの読み込み
        /// </summary>
        public UniTask<PreviewActorSetupData> LoadActorSetupDataAsync(string setupDataId, CancellationToken ct) {
            return _actorSetupDataStorage.LoadAssetAsync(new PreviewActorSetupDataRequest(setupDataId))
                .ToUniTask(cancellationToken:ct);
        }
    }
}
