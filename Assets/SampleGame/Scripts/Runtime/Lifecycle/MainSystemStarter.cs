using System;
using GameFramework;
using GameFramework.SituationSystems;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// メインシステム起動用のStarter基底
    /// </summary>
    public abstract class MainSystemStarter : GameFramework.BootSystems.MainSystemStarter {
        /// <summary>MainSystem開始引数の取得</summary>
        public sealed override object[] GetArguments() => new object[] { CreateStartArgs() };
        
        /// <summary>開始時に再生するSituationType</summary>
        protected abstract Type SituationType { get; }
        /// <summary>開始時の遷移タイプ</summary>
        protected virtual SituationService.TransitionType TransitionType => SituationService.TransitionType.SceneDefault;

        /// <summary>
        /// Situationセットアップ処理
        /// </summary>
        protected virtual void OnSituationSetup(Situation situation) {}

        /// <summary>
        /// 開始引数の生成
        /// </summary>
        private MainSystem.StartArgs CreateStartArgs() {
            return new MainSystem.StartArgs {
                SituationType = SituationType,
                SetupAction = OnSituationSetup,
                TransitionType = TransitionType
            };
        }
    }
}