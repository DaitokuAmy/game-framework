using UnityEngine;

namespace ThirdPersonEngine {
    /// <summary>
    /// 移動制御用インターフェース
    /// </summary>
    public interface IMovableActor {
        /// <summary>地面の高さ</summary>
        float GroundHeight { get; }
        /// <summary>地上にいるか</summary>
        bool IsGrounded { get; }
        
        /// <summary>
        /// 座標の取得
        /// </summary>
        Vector3 GetPosition();

        /// <summary>
        /// 座標の設定
        /// </summary>
        void SetPosition(Vector3 position);

        /// <summary>
        /// 向きの取得
        /// </summary>
        Quaternion GetRotation();
        
        /// <summary>
        /// 向きの設定
        /// </summary>
        void SetRotation(Quaternion rotation);
    }
}
