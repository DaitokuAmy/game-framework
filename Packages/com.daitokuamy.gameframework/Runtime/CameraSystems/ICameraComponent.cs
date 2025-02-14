using System;
using Unity.Cinemachine;

namespace GameFramework.CameraSystems {
    /// <summary>
    /// カメラを操作するためのComponent
    /// </summary>
    public interface ICameraComponent : IDisposable {
        // アクティブ状態
        bool IsActive { get; }

        // 基本になるCinemachineCamera
        ICinemachineCamera BaseCamera { get; }

        /// <summary>
        /// 初期化処理
        /// </summary>
        void Initialize(CameraManager cameraManager);

        /// <summary>
        /// アクティブ化
        /// </summary>
        void Activate();

        /// <summary>
        /// 非アクティブ化
        /// </summary>
        void Deactivate();

        /// <summary>
        /// 更新処理
        /// </summary>
        void Update(float deltaTime);

        /// <summary>
        /// プライオリティのセット(上書き用)
        /// </summary>
        void SetPriority(int priority);

        /// <summary>
        /// プライオリティのリセット
        /// </summary>
        void ResetPriority();
    }
}