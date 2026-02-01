using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework;
using GameFramework.ActorSystem;
using GameFramework.CollisionSystem;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace SampleGameEngine {
    /// <summary>
    /// キャラ制御用ビュー
    /// </summary>
    public class CharacterActorView : MovableActorView, IReceiveCollider {
        private static readonly int SpeedXPropId = Animator.StringToHash("speed.x");
        private static readonly int SpeedZPropId = Animator.StringToHash("speed.z");
        private static readonly int DirectionXPropId = Animator.StringToHash("direction.x");
        private static readonly int DirectionZPropId = Animator.StringToHash("direction.z");
        private static readonly int SpeedPropId = Animator.StringToHash("speed");
        private static readonly int SpeedScalePropId = Animator.StringToHash("speed_scale");
        private static readonly int IsFallingPropId = Animator.StringToHash("is_falling");
        private static readonly int LastTagHash = Animator.StringToHash("Last");

        /// <summary>
        /// Playable初期化時のクロージャ回避用インターフェース
        /// </summary>
        private interface IPlayableSetup<in TPlayable>
            where TPlayable : IPlayable {
            void Apply(TPlayable playable);
        }
        
        /// <summary>
        /// 初期化無しの時の実装
        /// </summary>
        private readonly struct NullSetup<TPlayable> : IPlayableSetup<TPlayable>
            where TPlayable : IPlayable {
            void IPlayableSetup<TPlayable>.Apply(TPlayable playable) { }
        }

        /// <summary>
        /// Knockback用Playableの初期化処理用
        /// </summary>
        private readonly struct KnockbackSetup : IPlayableSetup<AnimatorControllerPlayable> {
            private readonly float _directionX;
            private readonly float _directionZ;

            public KnockbackSetup(float directionX, float directionZ) {
                _directionX = directionX;
                _directionZ = directionZ;
            }

            void IPlayableSetup<AnimatorControllerPlayable>.Apply(AnimatorControllerPlayable playable) {
                playable.SetFloat(DirectionXPropId, _directionX);
                playable.SetFloat(DirectionZPropId, _directionZ);
            }
        }

        private readonly CharacterActorData _data;
        private readonly CharacterController _characterController;
        private readonly RootMotionHandler _rootMotionHandler;
        
        private AnimatorControllerPlayable _locomotionControllerPlayable;
        private Vector2 _movementValue;
        private Vector3 _aimPoint;

        /// <inheritdoc/>
        Vector3 IReceiveCollider.Start => Body.Position + Vector3.up * 0.5f;
        /// <inheritdoc/>
        Vector3 IReceiveCollider.End => Body.Position + Vector3.up * 1.5f;
        /// <inheritdoc/>
        float IReceiveCollider.Radius => 0.5f;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CharacterActorView(Body body, CharacterActorData data)
            : base(body) {
            _data = data;
            _characterController = Body.GetComponent<CharacterController>();
            _rootMotionHandler = Body.GetComponent<RootMotionHandler>();
        }

        /// <inheritdoc/>
        protected override void ActivateInternal(IScope scope) {
            base.ActivateInternal(scope);
            
            _locomotionControllerPlayable = MotionComponent.Change(_data.LocomotionController, 0.0f, false);
        }

        /// <inheritdoc/>
        protected override void UpdateInternal(float deltaTime) {
            base.UpdateInternal(deltaTime);
            
            // AnimatorProperty更新
            UpdateAnimatorProperties(deltaTime);
        }

        /// <summary>
        /// 方向指示移動
        /// </summary>
        /// <param name="x">X軸移動値</param>
        /// <param name="z">Z軸移動値</param>
        public void DriveMove(float x, float z) {
            Mover.Drive(x, z);
        }

        /// <summary>
        /// 正面の設定
        /// </summary>
        /// <param name="angleY">正面向きを表すY軸角度</param>
        public void SetForward(float angleY) {
            var eulerAngles = Transform.eulerAngles;
            eulerAngles.y = angleY;
            ApplyRotation(Quaternion.Euler(eulerAngles));
        }

        /// <summary>
        /// 攻撃再生
        /// </summary>
        /// <param name="index">攻撃Index</param>
        /// <param name="ct">非同期キャンセル用</param>
        public UniTask PlayAttackAsync(int index, CancellationToken ct) {
            var actions = _data.AttackActions;
            if (index < 0 || index >= actions.Length) {
                return UniTask.CompletedTask;
            }

            DriveMove(0, 0);
            return PlayClipActionAsync(actions[index], ct);
        }

        /// <summary>
        /// ジャンプ再生
        /// </summary>
        /// <param name="ct">非同期キャンセル用</param>
        public UniTask PlayJumpAsync(CancellationToken ct) {
            return PlayClipActionAsync(_data.JumpAction, ct);
        }

        /// <summary>
        /// ノックバック再生
        /// </summary>
        /// <param name="damageDirection">ダメージ向き(その方向にノックバックする)</param>
        /// <param name="ct">非同期キャンセル用</param>
        public UniTask PlayKnockbackAsync(Vector3 damageDirection, CancellationToken ct) {
            var localDir = Transform.InverseTransformDirection(damageDirection);
            return PlayControllerActionAsync(_data.KnockbackAction, new KnockbackSetup(localDir.x, localDir.z), ct);
        }

        /// <summary>
        /// 汎用AnimationClipアクションの再生
        /// </summary>
        private async UniTask PlayClipActionAsync(CharacterActorData.ClipActionInfo action, CancellationToken ct) {
            MotionComponent.Change(action.Clip, action.InBlend);
            if (action.SequenceClip != null) {
                PlaySequenceClip(action.SequenceClip);
            }

            var duration = action.Clip.length - action.OutBlend;
            await UniTask.Delay((int)(duration * 1000), cancellationToken: ct);
            MotionComponent.Change(_locomotionControllerPlayable, action.OutBlend);
        }

        /// <summary>
        /// 汎用AnimatorControllerアクションの再生
        /// </summary>
        private async UniTask PlayControllerActionAsync<TSetup>(CharacterActorData.ControllerActionInfo action, TSetup setup, CancellationToken ct)
            where TSetup : struct, IPlayableSetup<AnimatorControllerPlayable> {
            var playable = MotionComponent.Change(action.Controller, action.InBlend);
            setup.Apply(playable);

            if (action.SequenceClip != null) {
                PlaySequenceClip(action.SequenceClip);
            }

            while (true) {
                ct.ThrowIfCancellationRequested();

                var info = playable.GetCurrentAnimatorStateInfo(0);
                if (info.tagHash != LastTagHash) {
                    await UniTask.Yield(PlayerLoopTiming.Update);
                    continue;
                }

                var duration = info.length;
                var currentTime = info.normalizedTime * duration;
                if (currentTime >= (duration - action.OutBlend)) {
                    break;
                }

                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            MotionComponent.Change(_locomotionControllerPlayable, action.OutBlend);
        }

        /// <summary>
        /// 汎用AnimatorControllerアクションの再生
        /// </summary>
        private UniTask PlayControllerActionAsync(CharacterActorData.ControllerActionInfo action, CancellationToken ct) {
            return PlayControllerActionAsync(action, default(NullSetup<AnimatorControllerPlayable>), ct);
        }

        /// <summary>
        /// AnimatorのProperty更新
        /// </summary>
        private void UpdateAnimatorProperties(float deltaTime) {
            void SetFloatBlend(int id, float target, float t) {
                var current = _locomotionControllerPlayable.GetFloat(id);
                _locomotionControllerPlayable.SetFloat(id, Mathf.Lerp(current, target, t));
            }

            // 移動値を相対方向に変換
            var localMovement = new Vector3(_movementValue.x, 0.0f, _movementValue.y);
            localMovement = Transform.InverseTransformVector(localMovement);

            // Motion用プロパティ更新
            SetFloatBlend(SpeedXPropId, localMovement.x, 0.2f);
            SetFloatBlend(SpeedZPropId, localMovement.z, 0.2f);
            SetFloatBlend(SpeedPropId, localMovement.magnitude, 0.2f);
        }
    }
}