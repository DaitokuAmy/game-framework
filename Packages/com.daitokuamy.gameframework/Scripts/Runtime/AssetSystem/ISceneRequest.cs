namespace GameFramework.AssetSystem {
    /// <summary>
    /// シーン読み込み要求
    /// </summary>
    public interface ISceneRequest {
        /// <summary>読み込み対象アドレス</summary>
        string Address { get; }

        /// <summary>読み込み後にアクティブ化するか</summary>
        bool ActivateOnLoad { get; }

        /// <summary>有効な要求か</summary>
        bool IsValid { get; }
    }
}
