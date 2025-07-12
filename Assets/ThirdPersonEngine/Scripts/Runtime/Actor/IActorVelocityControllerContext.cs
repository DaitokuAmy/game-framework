namespace ThirdPersonEngine {
    /// <summary>
    /// アクター速度制御の設定値用のコンテキストのインタフェース
    /// </summary>
    public interface IActorVelocityControllerContext {
        /// <summary>空中のブレーキ速度</summary>
        float AirBrake { get; }
        /// <summary>地上のブレーキ速度</summary>
        float GroundBrake { get; }
    }
}