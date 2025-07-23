namespace ThirdPersonEngine {
    /// <summary>
    /// アクター制御レイヤータイプ
    /// </summary>
    public enum ActorControlLayerType {
        /// <summary>システムレイヤー</summary>
        System,
        /// <summary>自身で制御する</summary>
        Self,
        /// <summary>外部で制御する</summary>
        Other,
    }
}