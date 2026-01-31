using UnityEngine;

namespace GameFramework.ActorSystem {
    /// <summary>
    /// ルートモーション駆動を抽象化するインターフェース
    /// </summary>
    public interface IRootMotionDriver {
        /// <summary>
        /// ロコモーション入力を設定する
        /// </summary>
        /// <param name="localMove">ローカル空間の移動入力（-1〜1想定）</param>
        /// <param name="speedMultiplier">速度倍率</param>
        /// <param name="run">走行フラグ</param>
        void SetLocomotion(Vector2 localMove, float speedMultiplier, bool run);

        /// <summary>
        /// ロコモーション入力をリセットする
        /// </summary>
        void ResetLocomotion();
    }
}