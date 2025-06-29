namespace GameFramework.Core {
    /// <summary>
    /// IModelを管理するストレージのインターフェース
    /// </summary>
    public interface IModelStorage {
        /// <summary>
        /// クリア処理(全Modelの削除)
        /// </summary>
        void Clear();
    }
}