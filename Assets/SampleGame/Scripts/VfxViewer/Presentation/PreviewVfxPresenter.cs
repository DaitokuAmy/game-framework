using GameFramework.Core;
using GameFramework.ActorSystems;

namespace SampleGame.VfxViewer {
    /// <summary>
    /// PreviewVfx制御用のPresenter
    /// </summary>
    public class PreviewVfxPresenter : ActorEntityLogic {
        private PreviewVfxModel _model;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PreviewVfxPresenter(PreviewVfxModel model) {
            _model = model;
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(IScope scope) {
            base.ActivateInternal(scope);
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal() {
            base.UpdateInternal();
        }
    }
}