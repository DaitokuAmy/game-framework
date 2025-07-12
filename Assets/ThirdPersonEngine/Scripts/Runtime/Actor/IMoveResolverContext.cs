namespace ThirdPersonEngine {
    /// <summary>
    /// 移動解決用の設定値コンテキストのインタフェース
    /// </summary>
    public interface IMoveResolverContext {
        /// <summary>最大速度</summary>
        float MaxSpeed { get; }
        /// <summary>加速度</summary>
        float Acceleration { get; }
        /// <summary>ブレーキ速度</summary>
        float Brake { get; }
        /// <summary>角加速度</summary>
        float AngularSpeed { get; }
    }
}