using ActionSequencer;
using GameFramework.BodySystems;
using GameFramework.CameraSystems;
using GameFramework.ProjectileSystems;
using GameFramework.Core;
using GameFramework.ActorSystems;
using GameFramework.CollisionSystems;
using GameFramework.VfxSystems;
using SampleGame.Application.Battle;
using SampleGame.Battle;
using SampleGame.Domain.Battle;
using SampleGame.SequenceEvents;
using UniRx;
using UnityEngine;

namespace SampleGame.Presentation.Battle {
    /// <summary>
    /// プレイヤー制御用Presenter
    /// </summary>
    public class BattlePlayerPresenter : ActorEntityLogic, ICollisionListener, IRaycastCollisionListener {
        private IReadOnlyBattlePlayerModel _model;
        private BattleCharacterActor _actor;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BattlePlayerPresenter(IReadOnlyBattlePlayerModel model, BattleCharacterActor actor) {
            _model = model;
            _actor = actor;
        }

        /// <summary>
        /// レイキャストヒット時の処理
        /// </summary>
        void IRaycastCollisionListener.OnHitRaycastCollision(RaycastHitResult result) {
        }

        /// <summary>
        /// コリジョンヒット時の処理
        /// </summary>
        void ICollisionListener.OnHitCollision(HitResult result) {
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(IScope scope) {
            var input = Services.Get<BattleInput>();
            var cameraManager = Services.Get<CameraManager>();
            var playerAppService = Services.Get<BattlePlayerAppService>();
            
            // Camera登録
            if (_model.ActorModel.SetupData.cameraGroupPrefab != null) {
                cameraManager.RegisterCameraGroupPrefab(_model.ActorModel.SetupData.cameraGroupPrefab);
            }
            
            // SequenceEvent登録
            BindSequenceEventHandlers(scope);

            //-- View反映系

            //-- 入力系
            // 攻撃
            input.AttackSubject
                .TakeUntil(scope)
                .Subscribe(_ => {
                    playerAppService.PlayGeneralAction(_model.Id, 0);
                });

            // ジャンプ
            input.JumpSubject
                .TakeUntil(scope)
                .Subscribe(_ => {
                    playerAppService.PlayJumpAction(_model.Id);
                });
        }

        /// <summary>
        /// 非アクティブ時処理
        /// </summary>
        protected override void DeactivateInternal() {
            var cameraManager = Services.Get<CameraManager>();
            
            // Camera登録解除
            if (_model.ActorModel.SetupData.cameraGroupPrefab != null) {
                cameraManager.UnregisterCameraGroup(_model.ActorModel.SetupData.cameraGroupPrefab);
            }
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal() {
            var input = Services.Get<BattleInput>();
            var cameraManager = Services.Get<CameraManager>();
            var camera = cameraManager.OutputCamera;
            var projectileObjectManager = Services.Get<ProjectileObjectManager>();

            // 移動
            var moveVector = input.MoveVector;
            var forward = camera.transform.forward;
            forward.y = 0.0f;
            forward.Normalize();
            var right = forward;
            right.x = forward.z;
            right.z = -forward.x;
            var moveDirection = forward * moveVector.y + right * moveVector.x;
            _actor.MoveDirection(moveDirection);
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        protected override void LateUpdateInternal() {
            base.LateUpdateInternal();
            
            var playerAppService = Services.Get<BattlePlayerAppService>();
            
            // Bodyの位置情報の反映
            playerAppService.SetTransform(_model.Id, _actor.Body.Position, _actor.Body.Rotation);
        }

        /// <summary>
        /// SequenceEventに関する処理登録
        /// </summary>
        private void BindSequenceEventHandlers(IScope scope) {
            var sequenceController = _actor.SequenceController;
            var vfxManager = Services.Get<VfxManager>();
            var cameraManager = Services.Get<CameraManager>();
            var collisionManager = Services.Get<CollisionManager>();
            var projectileObjectManager = Services.Get<ProjectileObjectManager>();
            var gimmickController = _actor.Body.GetController<GimmickController>();
            var hitLayerMask = LayerMask.GetMask("Default");
            
            sequenceController.BindSignalEventHandler<BodyActiveGimmickSingleEvent, BodyActiveGimmickSingleEventHandler>(handler => {
                handler.Setup(gimmickController);
            });
            sequenceController.BindSignalEventHandler<BodyEffectSingleEvent, BodyEffectSingleEventHandler>(handler => {
                handler.Setup(vfxManager, _actor.Body);
            });
            sequenceController.BindRangeEventHandler<BodyEffectRangeEvent, BodyEffectRangeEventHandler>(handler => {
                handler.Setup(vfxManager, _actor.Body);
            });
            sequenceController.BindRangeEventHandler<CameraRangeEvent, CameraRangeEventHandler>(handler => {
                handler.Setup(cameraManager);
            });
            sequenceController.BindRangeEventHandler<MotionCameraRangeEvent, MotionCameraRangeEventHandler>(handler => {
                handler.Setup(cameraManager, _actor.Body.Transform, _actor.Body.LayeredTime);
            });
            sequenceController.BindRangeEventHandler<LookAtMotionCameraRangeEvent, LookAtMotionCameraRangeEventHandler>(handler => {
                handler.Setup(cameraManager, _actor.Body.Transform, _actor.Body.LayeredTime);
            });
            sequenceController.BindRangeEventHandler<RepeatSequenceClipRangeEvent, RepeatSequenceClipRangeEventHandler>(handler => {
                handler.Setup((SequenceController)_actor.SequenceController);
            });
            sequenceController.BindBattleProjectileEvent(collisionManager, projectileObjectManager, this, this, _actor, hitLayerMask, null);
            sequenceController.BindBodyGimmickEvent(_actor.Body);

            scope.OnExpired += () => sequenceController.ResetEventHandlers();
        }
    }
}