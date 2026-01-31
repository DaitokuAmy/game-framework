namespace GameFramework.ActorSystem {
    /// <summary>
    /// 移動挙動のオプション
    /// </summary>
    public readonly struct MoveOptions {
        /// <summary>規定値</summary>
        public static MoveOptions Default => new();
        
        /// <summary>移動に応じて回転を行うかどうか</summary>
        public readonly bool Rotate;
        /// <summary>加速度の上書き値、nullの場合はモジュールの既定値を使用する</summary>
        public readonly float? Accel;
        /// <summary>減速度の上書き値、nullの場合はモジュールの既定値を使用する</summary>
        public readonly float? Decel;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MoveOptions(bool rotate = true, float? accel = null, float? decel = null) {
            Rotate = rotate;
            Accel = accel;
            Decel = decel;
        }
    }
}