using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework;
using GameFramework.AssetSystem;
using SampleGame.Domain.ModelViewer;
using Object = UnityEngine.Object;

namespace SampleGame.Infrastructure.ModelViewer {
    /// <summary>
    /// モデルビューアー用テーブルデータ管理クラス
    /// </summary>
    public partial class ModelViewerTableRepository : IModelViewerTableRepository, IDisposable {
        private readonly SimpleAssetStorage _assetStorage;

        private ModelViewerActorTableData _modelViewerActorTableData;
        private ModelViewerEnvironmentTableData _modelViewerEnvironmentTableData;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ModelViewerTableRepository() {
            _assetStorage = new SimpleAssetStorage(AssetUtility.CreateAssetLoaders());
        }

        /// <summary>
        /// 破棄処理
        /// </summary>
        public void Dispose() {
            _assetStorage.Dispose();
        }

        /// <inheritdoc/>
        UniTask IModelViewerTableRepository.LoadTablesAsync(CancellationToken ct) {
            return UniTask.WhenAll(
                LoadTableAsync<ModelViewerActorTableData>("model_viewer_actor", ct)
                    .ContinueWith(x => _modelViewerActorTableData = x),
                LoadTableAsync<ModelViewerEnvironmentTableData>("model_viewer_environment", ct)
                    .ContinueWith(x => _modelViewerEnvironmentTableData = x)
            );
        }

        /// <inheritdoc/>
        int[] IModelViewerTableRepository.GetModelViewerActorIds() {
            return _modelViewerActorTableData.elements.Select(x => x.Id).ToArray();
        }

        /// <inheritdoc/>
        int[] IModelViewerTableRepository.GetModelViewerEnvironmentIds() {
            return _modelViewerEnvironmentTableData.elements.Select(x => x.Id).ToArray();
        }

        /// <inheritdoc/>
        IModelViewerActorMaster IModelViewerTableRepository.FindModelViewerActorById(int id) {
            return _modelViewerActorTableData.FindById(id);
        }

        /// <inheritdoc/>
        IModelViewerEnvironmentMaster IModelViewerTableRepository.FindModelViewerEnvironmentById(int id) {
            return _modelViewerEnvironmentTableData.FindById(id);
        }

        /// <summary>
        /// テーブルデータの読み込み
        /// </summary>
        private UniTask<T> LoadTableAsync<T>(string assetKey, CancellationToken ct)
            where T : Object {
            return _assetStorage.LoadAsync<T, TableDataRequest<T>>(new TableDataRequest<T>(assetKey)).ToUniTask<T>(cancellationToken: ct);
        }
    }
}
