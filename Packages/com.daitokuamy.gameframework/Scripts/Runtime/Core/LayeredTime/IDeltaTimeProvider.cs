namespace GameFramework.Core {
    /// <summary>
    /// DeltaTime提供インターフェース
    /// </summary>
    public interface IDeltaTimeProvider {
        /// <summary>該当フレームの変位時間</summary>
        float DeltaTime { get; }
    }
}