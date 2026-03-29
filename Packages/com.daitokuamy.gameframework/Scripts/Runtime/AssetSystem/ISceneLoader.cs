namespace GameFramework.AssetSystem {
    /// <summary>
    /// 単一バックエンドに対するシーンローダー
    /// </summary>
    public interface ISceneLoader {
        /// <summary>
        /// 読み込み可能か
        /// </summary>
        bool CanLoad(ISceneRequest request);

        /// <summary>
        /// シーンの読み込み
        /// </summary>
        ISceneLoadHandle LoadAsync(ISceneRequest request);
    }
}
