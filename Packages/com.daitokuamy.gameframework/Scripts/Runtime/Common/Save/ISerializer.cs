namespace GameFramework {
    /// <summary>
    /// 保存に利用するシリアライザー用インターフェース
    /// </summary>
    public interface ISerializer {
        /// <summary>
        /// シリアライズ処理
        /// </summary>
        byte[] Serialize<T>(T data);

        /// <summary>
        /// デシリアライズ処理
        /// </summary>
        T Deserialize<T>(byte[] bytes);
    }
}
