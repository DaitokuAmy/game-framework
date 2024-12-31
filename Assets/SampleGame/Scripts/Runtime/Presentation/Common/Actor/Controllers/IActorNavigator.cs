using UnityEngine;

namespace SampleGame.Presentation {
    /// <summary>
    /// アクターの移動をナビゲーションするためのインターフェース
    /// </summary>
    public interface IActorNavigator {
        /// <summary>有効か</summary>
        bool IsValid { get; }
        /// <summary>移動先座標</summary>
        Vector3 SteeringTarget { get; }
        /// <summary>目的地</summary>
        Vector3 GoalPoint { get; }
        /// <summary>目的地までの残り距離</summary>
        float RemainingDistance { get; }
        /// <summary>タイムアウトする長さ(負の値だと無効)</summary>
        float TimeOutDuration { get; }

        /// <summary>
        /// 移動開始処理
        /// </summary>
        /// <param name="currentPosition">現在座標</param>
        /// <param name="speed">移動速度</param>
        void Start(Vector3 currentPosition, float speed);

        /// <summary>
        /// 移動先座標の設定
        /// </summary>
        /// <param name="position">移動先座標</param>
        void SetNextPosition(Vector3 position);
    }
}
