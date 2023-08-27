namespace GameFramework.EnvironmentSystems {
    /// <summary>
    /// 環境設定のユーザー定義用インターフェース
    /// </summary>
    public interface IEnvironmentResolver {
        /// <summary>
        /// 現在反映されている値を格納したContextを取得
        /// </summary>
        IEnvironmentContext GetCurrent();

        /// <summary>
        /// 値の反映
        /// </summary>
        /// <param name="context">反映対象のコンテキスト</param>
        void Apply(IEnvironmentContext context);

        /// <summary>
        /// 値の線形補間
        /// </summary>
        /// <param name="current">補間元</param>
        /// <param name="target">補間先</param>
        /// <param name="rate">ブレンド率</param>
        IEnvironmentContext Lerp(IEnvironmentContext current, IEnvironmentContext target, float rate);
    }
}