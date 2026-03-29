using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework;
using GameFramework.AssetSystem;
using SampleGame.Domain.ModelViewer;

namespace SampleGame.Infrastructure.ModelViewer {
    /// <summary>
    /// モデルビューア設定の読み込み管理リポジトリ
    /// </summary>
    public class ConfigRepository : System.IDisposable {
        private readonly SimpleAssetStorage _assetStorage;

        private ModelViewerConfigData _configData;

        /// <summary>
        /// 読み込み済み設定
        /// </summary>
        public IModelViewerConfig Config => _configData;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ConfigRepository() {
            _assetStorage = new SimpleAssetStorage(AssetUtility.CreateAssetLoaders());
        }

        /// <summary>
        /// 解放
        /// </summary>
        public void Dispose() {
            UnloadConfig();
            _assetStorage.Dispose();
        }

        /// <summary>
        /// 設定の読み込み
        /// </summary>
        public async UniTask<IModelViewerConfig> LoadConfigAsync(CancellationToken ct) {
            _configData = await _assetStorage
                .LoadAsync<ModelViewerConfigData, ModelViewerConfigDataRequest>(new ModelViewerConfigDataRequest())
                .ToUniTask<ModelViewerConfigData>(cancellationToken: ct);

            return _configData;
        }

        /// <summary>
        /// 設定のアンロード
        /// </summary>
        public void UnloadConfig() {
            if (_configData == null) {
                return;
            }

            _assetStorage.Unload<ModelViewerConfigData, ModelViewerConfigDataRequest>(new ModelViewerConfigDataRequest());
            _configData = null;
        }
    }
}
