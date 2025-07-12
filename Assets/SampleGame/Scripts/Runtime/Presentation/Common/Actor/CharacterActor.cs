using System;
using System.Collections;
using System.Linq;
using System.Threading;
using ActionSequencer;
using Cysharp.Threading.Tasks;
using GameFramework.ActorSystems;
using GameFramework.Core;
using GameFramework.PlayableSystems;
using SampleGame.Infrastructure;
using UnityEngine;
using UnityEngine.AI;

namespace SampleGame.Presentation {
    /// <summary>
    /// 汎用のキャラアクター
    /// </summary>
    public class CharacterActor : MovableActor {
        private CancellationTokenSource _actionCts;
        private NavMeshAgent _navMeshAgent;

        /// <summary>初期化用データ</summary>
        protected CharacterActorSetupData SetupData { get; private set; }
        /// <summary>基本モーション制御用コンポーネント</summary>
        protected AnimatorControllerPlayableComponent BasePlayableComponent { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CharacterActor(Body body, CharacterActorSetupData setupData) : base(body) {
            SetupData = setupData;
            _navMeshAgent = body.GetComponent<NavMeshAgent>();
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(IScope scope) {
            var baseMotionHandle = CreateBaseMotionHandle();
            BasePlayableComponent = baseMotionHandle.Change(SetupData.baseController, 0.0f, false);
            
            base.ActivateInternal(scope);

            // 移動解決用のクラスを設定
            MoveController.AddResolver(new StraightMoveResolver(SetupData.moveActionInfo));
            scope.ExpiredEvent += () => MoveController.RemoveResolver<StraightMoveResolver>();
            MoveController.AddResolver(new NavigationMoveResolver(SetupData.moveActionInfo));
            scope.ExpiredEvent += () => MoveController.RemoveResolver<NavigationMoveResolver>();
            MoveController.AddResolver(new WarpMoveResolver());
            scope.ExpiredEvent += () => MoveController.RemoveResolver<WarpMoveResolver>();
            MoveController.AddResolver(new DirectionMoveResolver(SetupData.moveActionInfo));
            scope.ExpiredEvent += () => MoveController.RemoveResolver<DirectionMoveResolver>();

            ResetMoveSpeedMultiplier();
        }

        /// <summary>
        /// 座標の更新
        /// </summary>
        protected override void SetPosition(Vector3 position) {
            base.SetPosition(position);

            if (_navMeshAgent != null) {
                _navMeshAgent.nextPosition = position;
            }
        }

        /// <summary>
        /// ActionPlayerResolverの追加
        /// </summary>
        protected override void AddActionPlayerHandlers(ActorActionPlayer actionPlayer, MotionHandle motionHandle) {
            base.AddActionPlayerHandlers(actionPlayer, motionHandle);
            actionPlayer.SetHandler<TriggerStateActorAction, TriggerStateActorActionHandler>(new TriggerStateActorActionHandler(BasePlayableComponent.Playable, (SequenceController)SequenceController));
            actionPlayer.SetHandler<CrossFadeStateActorAction, CrossFadeStateActorActionHandler>(new CrossFadeStateActorActionHandler(BasePlayableComponent.Playable, (SequenceController)SequenceController));
        }
        
        /// <summary>
        /// アクション終了時処理
        /// </summary>
        protected override void FinishedActorAction(IActorAction action, float outBlend) {
            base.FinishedActorAction(action, outBlend);
            ChangeDefaultMotionInternal(outBlend);
        }

        /// <summary>
        /// 基礎となるモーションハンドルの生成
        /// </summary>
        /// <returns></returns>
        protected virtual MotionHandle CreateBaseMotionHandle() {
            return MotionComponent.Handle;
        }

        /// <summary>
        /// デフォルトモーションの設定
        /// </summary>
        public virtual void ChangeDefaultMotion(float blendDuration = 0.0f) { 
            CancelActionCancellationHandle();
            ChangeDefaultMotionInternal(blendDuration);
        }

        /// <summary>
        /// アクションキーの一覧を取得
        /// </summary>
        public string[] GetActionKeys() {
            return SetupData.actionInfos.Select(x => x.actionKey).ToArray();
        }

        /// <summary>
        /// アクションが存在するかチェック
        /// </summary>
        public bool ContainsAction(string actionKey) {
            return FindActionInfo(actionKey) != null;
        }

        /// <summary>
        /// Transform情報の設定
        /// </summary>
        public void SetTransform(Vector3 position, Quaternion rotation) {
            CancelActionCancellationHandle();
            SetPosition(position);
            SetRotation(rotation);
        }

        /// <summary>
        /// 座標移動
        /// </summary>
        public void Warp(Vector3 position, bool immediate = false) {
            ResetMoveSpeedMultiplier();
            MoveController.MoveToPointAsync<WarpMoveResolver>(position);
            if (immediate) {
                MoveController.Skip();
            }
        }

        /// <summary>
        /// 移動キャンセル
        /// </summary>
        protected void CancelMove() {
            ResetMoveSpeedMultiplier();
            MoveController.Cancel();
        }

        /// <summary>
        /// 指定方向に移動し続ける
        /// </summary>
        /// <param name="direction">移動向き</param>
        /// <param name="speedMultiplier">移動速度倍率</param>
        /// <param name="updateRotation">向きを更新するか</param>
        protected void DirectionMove(Vector3 direction, float speedMultiplier = 1.0f, bool updateRotation = true) {
            // 移動値を設定
            SetMoveSpeedMultiplier(speedMultiplier);
            MoveController.MoveToDirection<DirectionMoveResolver>(direction, speedMultiplier, updateRotation);
        }

        /// <summary>
        /// アクションの再生
        /// </summary>
        protected async UniTask PlayActionAsync(string actionKey, Action<ActorActionHandle> onCreatedHandle, CancellationToken ct) {
            ct.ThrowIfCancellationRequested();

            var actionInfo = FindActionInfo(actionKey);
            if (actionInfo == null) {
                return;
            }

            IEnumerator Routine(CancellationToken token) {
                // アクション再生
                yield return PlayActionRoutine(ActionPlayer, actionInfo.action.GetAction(), onCreatedHandle, token);
            }

            await PlayActionAsyncInternal(Routine, null, ct);
        }

        /// <summary>
        /// アクションの再生
        /// </summary>
        protected async UniTask PlayActionAsync(IActorAction actorAction, Action<ActorActionHandle> onCreatedHandle, CancellationToken ct) {
            ct.ThrowIfCancellationRequested();

            IEnumerator Routine(CancellationToken token) {
                // アクション再生
                yield return PlayActionRoutine(ActionPlayer, actorAction, onCreatedHandle, token);
            }

            await PlayActionAsyncInternal(Routine, null, ct);
        }

        /// <summary>
        /// ナビゲーターを使った移動
        /// </summary>
        /// <param name="navigator">移動方法を制御するナビゲーター</param>
        /// <param name="speedMultiplier">移動速度倍率</param>
        /// <param name="ct">非同期キャンセル用トークン</param>
        protected async UniTask NavigationMoveAsync(IActorNavigator navigator, float speedMultiplier = 1.0f, CancellationToken ct = default) {
            ct.ThrowIfCancellationRequested();

            IEnumerator Routine(CancellationToken token) {
                yield return NavigationMoveRoutine(navigator, speedMultiplier, 0.05f, token);
            }

            await PlayActionAsyncInternal(Routine, null, ct);
        }

        /// <summary>
        /// ポイント指定移動(直線)
        /// </summary>
        /// <param name="point">移動先座標</param>
        /// <param name="speedMultiplier">移動速度倍率</param>
        /// <param name="ct">非同期キャンセル用トークン</param>
        protected async UniTask MoveToPointAsync(Vector3 point, float speedMultiplier = 1.0f, CancellationToken ct = default) {
            ct.ThrowIfCancellationRequested();

            IEnumerator Routine(CancellationToken token) {
                yield return StraightMoveRoutine(point, speedMultiplier, 0.05f, token);
            }

            await PlayActionAsyncInternal(Routine, null, ct);
        }

        /// <summary>
        /// 相対向きへの移動(直線)
        /// </summary>
        /// <param name="relativeVector">相対ベクトル</param>
        /// <param name="speedMultiplier">移動速度倍率</param>
        /// <param name="ct">非同期キャンセル用トークン</param>
        protected async UniTask RelativeMoveAsync(Vector3 relativeVector, float speedMultiplier = 1.0f, CancellationToken ct = default) {
            ct.ThrowIfCancellationRequested();

            IEnumerator Routine(CancellationToken token) {
                relativeVector.y = 0.0f;
                var point = Body.Transform.TransformPoint(relativeVector);
                yield return StraightMoveRoutine(point, speedMultiplier, 0.05f, token);
            }

            await PlayActionAsyncInternal(Routine, null, ct);
        }

        /// <summary>
        /// デフォルトモーションの設定(内部用)
        /// </summary>
        protected void ChangeDefaultMotionInternal(float blendDuration) {
            MotionComponent.Handle.Change(BasePlayableComponent, blendDuration);
        }

        /// <summary>
        /// アクション再生ルーチン
        /// </summary>
        protected IEnumerator PlayActionRoutine(ActorActionPlayer actionPlayer, IActorAction action, Action<ActorActionHandle> onCreatedActionHandle, CancellationToken ct) {
            // アクション再生
            var actionHandle = actionPlayer.PlayAction(action);
            onCreatedActionHandle?.Invoke(actionHandle);

            // 終了待ち
            void Finished() {
                actionHandle.Stop();
            }
            using var registration = ct.Register(Finished);
            yield return actionHandle;

            Finished();
        }

        /// <summary>
        /// アクションを探す
        /// </summary>
        private CharacterActorSetupData.ActionInfo FindActionInfo(string actionKey) {
            return SetupData.actionInfos.FirstOrDefault(x => x.actionKey == actionKey);
        }

        /// <summary>
        /// アクションキャンセル用ハンドルの解放
        /// </summary>
        private void CancelActionCancellationHandle() {
            if (_actionCts != null) {
                var cts = _actionCts;
                cts.Cancel();
                cts.Dispose();
                _actionCts = null;
            }
        }

        /// <summary>
        /// アクション再生の内部処理
        /// </summary>
        /// <param name="getCoroutineFunc">コルーチン取得用関数</param>
        /// <param name="onCancelAction">キャンセル時の処理</param>
        /// <param name="ct">キャンセル用トークン</param>
        private async UniTask PlayActionAsyncInternal(Func<CancellationToken, IEnumerator> getCoroutineFunc, Action onCancelAction, CancellationToken ct) {
            // 現在のアクションをキャンセル
            CancelActionCancellationHandle();

            // アクション用のスコープを生成
            _actionCts = new CancellationTokenSource();

            // CancellationTokenの合成
            ct = CancellationTokenSource.CreateLinkedTokenSource(_actionCts.Token, ct).Token;

            var handle = ct.Register(() => {
                onCancelAction?.Invoke();
                _actionCts = null;
            });

            // コルーチンとして処理を実行
            await StartCoroutineAsync(getCoroutineFunc(ct), ct);
            _actionCts = null;

            await handle.DisposeAsync();
        }

        /// <summary>
        /// 向き指定移動用ルーチン
        /// </summary>
        private IEnumerator DirectionMoveRoutine(Vector3 direction, float distance, float speedMultiplier, CancellationToken ct) {
            var position = Body.Position;
            MoveController.MoveToDirectionAsync<DirectionMoveResolver>(direction, speedMultiplier);

            // モーション速度の変更
            if (direction.sqrMagnitude <= 0.001f) {
                ResetMoveSpeedMultiplier();
            }
            else {
                SetMoveSpeedMultiplier(speedMultiplier);
            }

            if (distance < 0) {
                // 距離が0未満の時は無限に移動
                while (true) {
                    yield return null;
                }
            }

            // 指定した距離分を移動するまで待機
            var sprDistance = distance * distance;
            while ((Body.Position - position).sqrMagnitude < sprDistance) {
                yield return null;
            }
        }

        /// <summary>
        /// 直線移動用ルーチン
        /// </summary>
        private IEnumerator StraightMoveRoutine(Vector3 point, float speedMultiplier, float arrivedDistance, CancellationToken ct) {
            void Finished() {
                MoveController.Cancel();
                ResetMoveSpeedMultiplier();
            }

            // 移動する必要なし
            var vector = point - Body.Position;
            vector.y = 0.0f;
            var threshold = SetupData.moveActionInfo.moveThreshold * Body.BaseScale;
            if (vector.sqrMagnitude <= threshold * threshold) {
                yield break;
            }

            using var registration = ct.Register(Finished);

            // モーション速度の変更
            SetMoveSpeedMultiplier(speedMultiplier);

            // 直線移動
            yield return MoveController.MoveToPointAsync<StraightMoveResolver>(point, speedMultiplier * Body.BaseScale, arrivedDistance);

            Finished();
        }

        /// <summary>
        /// ナビゲーター使用の移動ルーチン
        /// </summary>
        private IEnumerator NavigationMoveRoutine(IActorNavigator navigator, float speedMultiplier, float arrivedDistance, CancellationToken ct) {
            void Finished() {
                MoveController.Cancel();
                ResetMoveSpeedMultiplier();
            }

            using var registration = ct.Register(Finished);

            // モーション速度の変更
            SetMoveSpeedMultiplier(speedMultiplier);

            // 移動処理
            yield return MoveController.MoveAsync<NavigationMoveResolver>(navigator, speedMultiplier * Body.BaseScale, arrivedDistance);

            Finished();
        }

        /// <summary>
        /// 移動速度のモーション速度を変更
        /// </summary>
        private void SetMoveSpeedMultiplier(float speedMultiplier) {
            BasePlayableComponent.Playable.SetFloat("MoveSpeedMultiplier", speedMultiplier / Body.BaseScale);
        }

        /// <summary>
        /// 移動速度のモーション速度をリセット
        /// </summary>
        private void ResetMoveSpeedMultiplier() {
            SetMoveSpeedMultiplier(0.0f);
        }
    }
}