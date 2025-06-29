using System;
using GameFramework.SituationSystems;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// メインシステム起動用のStarter基底
    /// </summary>
    public abstract class MainSystemStarter : GameFramework.MainSystemStarter {
        /// <summary>
        /// MainSystem開始引数の取得
        /// </summary>
        public sealed override object[] GetArguments() => new object[] { GetSituationSetup(), GetStartTransitionEffects() };

        /// <summary>
        /// 開始Situationのセットアップ処理の取得
        /// </summary>
        protected abstract ISituationSetup GetSituationSetup();

        /// <summary>
        /// 開始遷移エフェクトの取得
        /// </summary>
        protected virtual ITransitionEffect[] GetStartTransitionEffects() {
            return Array.Empty<ITransitionEffect>();
        }
    }
}