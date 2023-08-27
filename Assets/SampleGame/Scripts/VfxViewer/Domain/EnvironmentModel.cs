using GameFramework.ModelSystems;
using UniRx;

namespace SampleGame.VfxViewer {
    /// <summary>
    /// 環境情報用モデル
    /// </summary>
    public class EnvironmentModel : AutoIdModel<EnvironmentModel> {
        private ReactiveProperty<string> _assetId = new();

        // 環境情報用のアセットID
        public IReadOnlyReactiveProperty<string> AssetId => _assetId;
        
        /// <summary>
        /// アセットIDの切り替え
        /// </summary>
        public void SetAssetId(string assetId) {
            _assetId.Value = assetId;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private EnvironmentModel(int id) 
            : base(id) {}
    }
}
