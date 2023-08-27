using System;
using UnityEngine;

namespace SampleGame {
    /// <summary>
    /// 移動処理解決用インターフェース
    /// </summary>
    public interface IMoveResolver {
        /// <summary>移動中か</summary>
        bool IsMoving { get; }
        /// <summary>現在の移動速度</summary>
        Vector3 Velocity { get; }
        
        /// <summary>移動終了イベント(boolは達成したか)</summary>
        event Action<bool> OnMoveEndEvent;

        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <param name="actor">制御対象アクター</param>
        void Initialize(IMovableActor actor);

        /// <summary>
        /// 移動キャンセル
        /// </summary>
        void Cancel();

        /// <summary>
        /// 移動スキップ
        /// </summary>
        void Skip();

        /// <summary>
        /// 更新処理
        /// </summary>
        void Update(float deltaTime, float speedMultiplier);
    }
}
