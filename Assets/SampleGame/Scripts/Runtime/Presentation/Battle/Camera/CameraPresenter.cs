using GameFramework;
using GameFramework.CameraSystems;
using GameFramework.Core;
using SampleGame.Application.Battle;
using UnityEngine;

namespace SampleGame.Presentation.Battle {
    /// <summary>
    /// カメラ用のPresenter
    /// </summary>
    public class CameraPresenter : Logic {
        private readonly CameraManager _cameraManager;
        private readonly BattleAppService _battleAppService;

        private Transform _center;
        private Transform _rootAngle;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CameraPresenter() {
            _cameraManager = Services.Resolve<CameraManager>();
            _battleAppService = Services.Resolve<BattleAppService>();
        }

        /// <inheritdoc/>
        protected override void ActivateInternal(IScope scope) {
            base.ActivateInternal(scope);
            
            // Transform取得
            _center = _cameraManager.GetTargetPoint("Center");
            _rootAngle = _cameraManager.GetTargetPoint("RootAngle");
        }

        /// <inheritdoc/>
        protected override void LateUpdateInternal() {
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