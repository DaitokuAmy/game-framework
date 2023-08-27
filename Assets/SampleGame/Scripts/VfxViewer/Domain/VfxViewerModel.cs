using GameFramework.Core;
using GameFramework.ModelSystems;

namespace SampleGame.VfxViewer {
    /// <summary>
    /// モデルビューア用のルートモデル
    /// </summary>
    public class VfxViewerModel : SingleModel<VfxViewerModel> {
        // 基本データ
        public VfxViewerSetupData SetupData { get; private set; }
        // 表示用VFXのモデル
        public PreviewVfxModel PreviewVfxModel { get; private set; }
        // 環境用モデル
        public EnvironmentModel EnvironmentModel { get; private set; }
        // 録画用モデル
        public RecordingModel RecordingModel { get; private set; }
        // 設定用モデル
        public SettingsModel SettingsModel { get; private set; }

        /// <summary>
        /// 初期化
        /// </summary>
        public void Setup(VfxViewerSetupData setupData) {
            SetupData = setupData;
        }

        /// <summary>
        /// 生成時処理
        /// </summary>
        protected override void OnCreatedInternal(IScope scope) {
            PreviewVfxModel = PreviewVfxModel.Create()
                .ScopeTo(scope);
            EnvironmentModel = EnvironmentModel.Create()
                .ScopeTo(scope);
            RecordingModel = RecordingModel.Create()
                .ScopeTo(scope);
            SettingsModel = SettingsModel.Create()
                .ScopeTo(scope);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private VfxViewerModel(object empty) 
            : base(empty) {}
    }
}
