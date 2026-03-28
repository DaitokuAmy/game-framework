using UnityEngine;

namespace GameFramework.ProjectileSystem {
    /// <summary>
    /// 飛翔体用制御用インターフェース
    /// </summary>
    public interface IProjectileController {
        /// <summary>
        /// 飛翔開始
        /// </summary>
        void Play();

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        bool Tick(float deltaTime);

        /// <summary>
        /// 飛翔終了
        /// </summary>
        /// <param name="stopPosition">停止時に更新する座標(特に指定なければnull)</param>
        void Stop(Vector3? stopPosition);
    }
}
