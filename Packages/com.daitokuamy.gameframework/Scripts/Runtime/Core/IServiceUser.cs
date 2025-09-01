namespace GameFramework.Core {
    /// <summary>
    /// Serviceを利用するためのインターフェース
    /// </summary>
    public interface IServiceUser {
        /// <summary>
        /// Serviceの取得処理
        /// </summary>
        /// <param name="serviceResolver">Service提供用のインターフェース</param>
        void ImportService(IServiceResolver serviceResolver);
    }
}