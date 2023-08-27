using GameFramework.EnvironmentSystems;
using UnityEngine;

namespace SampleGame {
    /// <summary>
    /// 環境制御用クラス
    /// </summary>
    public class EnvironmentResolver : EnvironmentResolver<IEnvironmentContext> {
        /// <summary>
        /// 現在反映されている値を格納したContextを取得
        /// </summary>
        protected override IEnvironmentContext GetCurrentInternal() {
            return new CurrentEnvironmentContext();
        }

        /// <summary>
        /// 値の反映
        /// </summary>
        /// <param name="context">反映対象のコンテキスト</param>
        protected override void ApplyInternal(IEnvironmentContext context) {
            context.DefaultSettings.Apply();
            RenderSettings.sun = context.Sun;
        }

        /// <summary>
        /// 値の線形補間
        /// </summary>
        /// <param name="current">補間元</param>
        /// <param name="target">補間先</param>
        /// <param name="ratio">ブレンド率</param>
        protected override IEnvironmentContext LerpInternal(IEnvironmentContext current, IEnvironmentContext target,
            float ratio) {
            ratio = Mathf.Clamp01(ratio);

            var result = new EnvironmentContext();
            result.DefaultSettings = current.DefaultSettings.Lerp(target.DefaultSettings, ratio);
            result.Sun = target.Sun;

            return result;
        }
    }
}