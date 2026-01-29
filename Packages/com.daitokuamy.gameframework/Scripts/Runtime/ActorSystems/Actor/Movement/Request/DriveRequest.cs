using UnityEngine;

namespace GameFramework.ActorSystems {
    /// <summary>
    /// 継続入力によって駆動される移動リクエスト
    /// </summary>
    public readonly struct DriveRequest : IMoveRequest {
        /// <summary>入力ベクトル（-1〜1 正規化想定）</summary>
        public readonly Vector2 Move;
        /// <summary>走行フラグ</summary>
        public readonly bool Run;
        /// <summary>速度倍率</summary>
        public readonly float SpeedMultiplier;
        /// <summary>オプション</summary>
        public readonly MoveOptions Options;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public DriveRequest(Vector2 move, bool run, float speedMultiplier, MoveOptions options) {
            Move = move;
            Run = run;
            SpeedMultiplier = speedMultiplier;
            Options = options;
        }
    }
}