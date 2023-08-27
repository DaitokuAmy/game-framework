namespace GameFramework.EnvironmentSystems {
    /// <summary>
    /// 環境設定のユーザー定義用クラス基底
    /// </summary>
    public abstract class EnvironmentResolver<TContext> : IEnvironmentResolver
        where TContext : IEnvironmentContext {
        IEnvironmentContext IEnvironmentResolver.GetCurrent() {
            return GetCurrentInternal();
        }

        void IEnvironmentResolver.Apply(IEnvironmentContext context) {
            ApplyInternal((TContext)context);
        }

        IEnvironmentContext IEnvironmentResolver.Lerp(IEnvironmentContext current, IEnvironmentContext target,
            float rate) {
            return LerpInternal((TContext)current, (TContext)target, rate);
        }

        /// <summary>
        /// 現在反映されている値を格納したContextを取得
        /// </summary>
        protected abstract TContext GetCurrentInternal();

        /// <summary>
        /// 値の反映
        /// </summary>
        /// <param name="context">反映対象のコンテキスト</param>
        protected abstract void ApplyInternal(TContext context);

        /// <summary>
        /// 値の線形補間
        /// </summary>
        /// <param name="current">補間元</param>
        /// <param name="target">補間先</param>
        /// <param name="ratio">ブレンド率</param>
        protected abstract TContext LerpInternal(TContext current, TContext target, float ratio);
    }
}