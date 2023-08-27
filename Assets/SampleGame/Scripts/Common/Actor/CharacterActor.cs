using System.Collections;
using System.Threading;
using ActionSequencer;
using Cysharp.Threading.Tasks;
using GameFramework.BodySystems;
using GameFramework.Core;
using GameFramework.CoroutineSystems;
using GameFramework.ActorSystems;
using UnityEngine;

namespace SampleGame {
    /// <summary>
    /// キャラアクター基底
    /// </summary>
    public class CharacterActor : GameFramework.ActorSystems.Actor, IMovableActor {
        private SequenceController _sequenceController;
        private CoroutineRunner _coroutineRunner;

        /// <summary>外部公開用のシーケンス制御クラス</summary>
        public IReadOnlySequenceController SequenceController => _sequenceController;
        /// <summary>移動制御用クラス</summary>
        protected ActorMoveController MoveController { get; private set; }
        /// <summary>モーション制御クラス</summary>
        protected MotionController MotionController { get; private set; }
        /// <summary>アクション再生用クラス</summary>
        protected ActorActionPlayer ActionPlayer { get; private set; }
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        protected CharacterActor(Body body) : base(body) {
            MotionController = body.GetController<MotionController>();

            ActionPlayer = new ActorActionPlayer(body.LayeredTime);
            MoveController = new ActorMoveController(this);
            
            _sequenceController = new SequenceController();
            _coroutineRunner = new CoroutineRunner();
        }

        /// <summary>
        /// 現在座標の取得
        /// </summary>
        Vector3 IMovableActor.GetPosition() {
            return Body.Position;
        }

        /// <summary>
        /// 現在座標の更新
        /// </summary>
        void IMovableActor.SetPosition(Vector3 position) {
            Body.Position = position;
        }

        /// <summary>
        /// 現在向きの取得
        /// </summary>
        Quaternion IMovableActor.GetRotation() {
            return Body.Rotation;
        }

        /// <summary>
        /// 現在向きの更新
        /// </summary>
        void IMovableActor.SetRotation(Quaternion rotation) {
            Body.Rotation = rotation;
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DisposeInternal() {
            base.DisposeInternal();
            
            _coroutineRunner.Dispose();
            _sequenceController.Dispose();
            MoveController.Dispose();
            ActionPlayer.Dispose();
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(IScope scope) {
            base.ActivateInternal(scope);
            
            // ActionPlayerResolverの登録
            AddActionPlayerResolvers(ActionPlayer);
        }

        /// <summary>
        /// 非アクティブ時処理
        /// </summary>
        protected override void DeactivateInternal() {
            ActionPlayer.RemoveResolvers();
            MoveController.Cancel();
            
            base.DeactivateInternal();
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal() {
            base.UpdateInternal();
            
            var deltaTime = Body.DeltaTime;
            
            // コルーチンの更新
            _coroutineRunner.Update();
            // 移動の更新
            MoveController.Update(deltaTime);
            // アクションの更新
            ActionPlayer.Update();
            // シーケンスの再生
            _sequenceController.Update(deltaTime);
        }

        /// <summary>
        /// 現在座標の更新
        /// </summary>
        protected virtual void SetPosition(Vector3 position) {
            Body.Position = position;
        }

        /// <summary>
        /// 現在向きの更新
        /// </summary>
        protected virtual void SetRotation(Quaternion rotation) {
            Body.Rotation = rotation;
        }

        /// <summary>
        /// シーケンスクリップの再生
        /// </summary>
        protected SequenceHandle PlaySequenceAsync(SequenceClip clip, float startOffset = 0.0f) {
            return _sequenceController.Play(clip, startOffset);
        }

        /// <summary>
        /// コルーチンの開始
        /// </summary>
        protected UniTask StartCoroutineAsync(IEnumerator routine, CancellationToken ct) {
            return _coroutineRunner.StartCoroutineAsync(routine, ct);
        }

        /// <summary>
        /// ActionPlayerResolverの追加
        /// </summary>
        private void AddActionPlayerResolvers(ActorActionPlayer actionPlayer) {
            actionPlayer.AddResolver(new ClipActorActionResolver(MotionController.Handle, _sequenceController));
            actionPlayer.AddResolver(new ControllerActorActionResolver(MotionController.Handle, _sequenceController));
            actionPlayer.AddResolver(new TimelineActorActionResolver(MotionController.Handle, _sequenceController));
            actionPlayer.AddResolver(new SequentialClipActorActionResolver(MotionController.Handle, _sequenceController));
        }
    }
}