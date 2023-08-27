using Cysharp.Threading.Tasks;
using GameFramework.BodySystems;
using GameFramework.CameraSystems;
using GameFramework.ProjectileSystems;
using GameFramework.Core;
using GameFramework.ActorSystems;
using GameFramework.CollisionSystems;
using GameFramework.VfxSystems;
using SampleGame.SequenceEvents;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace SampleGame.Battle {
    /// <summary>
    /// プレイヤー制御用Presenter
    /// </summary>
    public class BattlePlayerPresenter : ActorEntityLogic, IRaycastCollisionListener {
        private BattleCharacterActor _actor;
        private BattlePlayerModel _model;
        private GimmickController _gimmickController;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BattlePlayerPresenter(BattleCharacterActor actor, BattlePlayerModel model) {
            _actor = actor;
            _model = model;

            _gimmickController = _actor.Body.GetController<GimmickController>();
        }

        /// <summary>
        /// レイキャストヒット時の処理
        /// </summary>
        void IRaycastCollisionListener.OnHitRaycastCollision(RaycastHitResult result) {
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(IScope scope) {
            var ct = scope.Token;
            var input = Services.Get<BattleInput>();
            var vfxManager = Services.Get<VfxManager>();
            var cameraManager = Services.Get<CameraManager>();
            
            var body = _actor.Body;
            var context = new VfxManager.Context {
                prefab = _model.ActorModel.SetupData.auraPrefab,
                constraintPosition = true,
                constraintRotation = true,
                localScale = Vector3.one,
            };
            
            // Camera登録
            if (_model.ActorModel.SetupData.cameraGroupPrefab != null) {
                cameraManager.RegisterCameraGroup(_model.ActorModel.SetupData.cameraGroupPrefab);
            }
            
            // SequenceEvent登録
            BindSequenceEventHandlers(scope);

            //-- View反映系
            // 攻撃
            _model.OnAttackSubject
                .TakeUntil(scope)
                .Subscribe(x => {
                    _actor.PlaySkillActionAsync(x.Item2, ct).Forget();
                });
            
            // ダメージ再生
            _model.OnDamagedSubject
                .TakeUntil(scope)
                .Subscribe(x => {
                    _actor.PlayDamageActionAsync(ct).Forget();
                });

            // 死亡
            _model.OnDeadSubject
                .TakeUntil(scope)
                .Subscribe(_ => { _actor.SetDeathFlag(true); });

            //-- 入力系
            // 攻撃
            var skillInfos = _model.ActorModel.SetupData.skillActionInfos;
            input.AttackSubject
                .TakeUntil(scope)
                .Subscribe(_ => {
                    _model.PlayAction(skillInfos[Random.Range(0, skillInfos.Length)].key);
                });

            // ジャンプ
            input.JumpSubject
                .TakeUntil(scope)
                .Subscribe(_ => { _actor.JumpActionAsync(ct).Forget(); });
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
            
            // テスト用アクション入力
            var skillActionInfos = _model.ActorModel.SetupData.skillActionInfos;
            for (var i = 0; i < skillActionInfos.Length; i++) {
                if (Keyboard.current[Key.Digit1 + i].wasPressedThisFrame) {
                    _model.PlayAction(skillActionInfos[i].key);
                }
            }

            // テスト用にダメージ発生
            if (Keyboard.current.qKey.wasPressedThisFrame) {
                _model.AddDamage(1);
            }

            if (Keyboard.current.vKey.wasPressedThisFrame) {
                _actor.Vibrate();
            }

            if (Keyboard.current.zKey.wasPressedThisFrame) {
                _actor.Body.IsVisible ^= true;
            }

            // テスト用にTimeScale変更
            if (Keyboard.current.lKey.isPressed) {
                _actor.Body.LayeredTime.LocalTimeScale = 0.25f;
            }
            else {
                _actor.Body.LayeredTime.LocalTimeScale = 1.0f;
            }

            // テスト用にギミック再生
            if (Keyboard.current.tKey.wasPressedThisFrame) {
                _gimmickController.GetAnimationGimmicks("Test").Resume();
            }

            if (Keyboard.current.yKey.wasPressedThisFrame) {
                _gimmickController.GetAnimationGimmicks("Test").Resume(true);
            }

            if (Keyboard.current.gKey.wasPressedThisFrame) {
                _gimmickController.GetActiveGimmicks("Sphere").Activate();
            }

            if (Keyboard.current.hKey.wasPressedThisFrame) {
                _gimmickController.GetActiveGimmicks("Sphere").Deactivate();
            }

            if (Keyboard.current.fKey.wasPressedThisFrame) {
                _gimmickController.GetAnimationGimmicks("Damage").Play();
            }
        }

        /// <summary>
        /// SequenceEventに関する処理登録
        /// </summary>
        private void BindSequenceEventHandlers(IScope scope) {
            var sequenceController = _actor.SequenceController;
            var vfxManager = Services.Get<VfxManager>();
            var cameraManager = Services.Get<CameraManager>();
            var projectileObjectManager = Services.Get<ProjectileObjectManager>();
            var gimmickController = _actor.Body.GetController<GimmickController>();
            
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
            sequenceController.BindSignalEventHandler<BattleCurveProjectileSignalEvent, BattleCurveProjectileSignalEventHandler>(handler => {
                handler.Setup(projectileObjectManager, this, _actor, -1, null);
            });
            sequenceController.BindSignalEventHandler<BattleHomingBulletProjectileSignalEvent, BattleHomingBulletProjectileSignalEventHandler>(handler => {
                handler.Setup(projectileObjectManager, this, _actor, -1, null);
            });
            sequenceController.BindSignalEventHandler<BattleStraightProjectileSignalEvent, BattleStraightProjectileSignalEventHandler>(handler => {
                handler.Setup(projectileObjectManager, this, _actor, -1, null);
            });
            sequenceController.BindSignalEventHandler<BattleShotBulletProjectileSignalEvent, BattleShotBulletProjectileSignalEventHandler>(handler => {
                handler.Setup(projectileObjectManager, this, _actor, -1, null);
            });
            sequenceController.BindSignalEventHandler<BattleCustomBulletProjectileSignalEvent, BattleCustomBulletProjectileSignalEventHandler>(handler => {
                handler.Setup(projectileObjectManager, this, _actor, -1, null);
            });
            sequenceController.BindSignalEventHandler<BattleThrowableProjectileSignalEvent, BattleThrowableProjectileSignalEventHandler>(handler => {
                handler.Setup(projectileObjectManager, this, _actor, -1, null);
            });
            sequenceController.BindRangeEventHandler<BattleBeamProjectileRangeEvent, BattleBeamProjectileRangeEventHandler>(handler => {
                handler.Setup(projectileObjectManager, this, _actor, -1, null);
            });

            scope.OnExpired += () => sequenceController.ResetEventHandlers();
        }
    }
}