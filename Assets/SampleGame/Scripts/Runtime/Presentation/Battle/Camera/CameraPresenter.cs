using GameFramework;
using GameFramework.CameraSystems;
using GameFramework.Core;
using SampleGame.Application.Battle;
using UnityEngine;
using VContainer;

namespace SampleGame.Presentation.Battle {
    /// <summary>
    /// カメラ用のPresenter
    /// </summary>
    public class CameraPresenter : UpdatableLogic {
        [Inject]
        private CameraManager _cameraManager;
        [Inject]
        private BattleAppService _battleAppService;

        private Transform _center;
        private Transform _rootAngle;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CameraPresenter() {
        }

        /// <inheritdoc/>
        protected override void ActivateInternal(IScope scope) {
            base.ActivateInternal(scope);
            
            // Transform取得
            _center = _cameraManager.GetTargetPoint("Center");
            _rootAngle = _cameraManager.GetTargetPoint("RootAngle");
        }

        /// <inheritdoc/>
        protected override void UpdateInternal() {
            base.UpdateInternal();
            
            // TargetPointの更新
            var playerActorModel = _battleAppService.BattleModel.PlayerModel.ActorModel;
            var playerPos = playerActorModel.GetRootPosition();
            var playerRot = playerActorModel.GetRootRotation();
            var lookAtRot = playerActorModel.GetLookAtRotation();

            _center.position = playerPos;
            _center.rotation = playerRot;
            _rootAngle.position = playerPos;
            _rootAngle.rotation = lookAtRot;
        }
    }
}