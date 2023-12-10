using System;
using System.Collections;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.BodySystems;
using GameFramework.Core;
using GameFramework.ActorSystems;
using GameFramework.PlayableSystems;
using SampleGame.Domain.Battle;
using UnityEngine;

namespace SampleGame {
    /// <summary>
    /// バトルキャラ制御用アクター
    /// </summary>
    public class BattleCharacterActor : CharacterActor, IAimTarget, IAimableActor {
        // 行動キャンセル判定用トークン
        private CancellationTokenSource _actionCts = new();
        // AnimatorController制御用
        private AnimatorControllerPlayableComponent _basePlayableComponent;
        // モーション制御クラス
        private MotionController _motionController;
        // 加算モーション用のレイヤーハンドル
        private MotionHandle _additiveMotionHandle;
        // 半径
        private float _radius;
        
        /// <summary>半径</summary>
        float IAimTarget.Radius => _radius;
        
        /// <summary>ターゲットアクター</summary>
        public BattleCharacterActor TargetCharacterActor { get; private set; }
        /// <summary>セットアップ用データ</summary>
        private BattleCharacterActorSetupData Data { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BattleCharacterActor(Body body, BattleCharacterActorSetupData setupData)
            : base(body) {
            Data = setupData;
            _motionController = body.GetController<MotionController>();
            _radius = 1.0f;
        }

        /// <summary>
        /// ターゲットのエイム処理
        /// </summary>
        void IAimableActor.Aim(Vector3 targetPoint) {
            // 対象の方に向く
            var pos = Body.Position;
            var direction = targetPoint - pos;
            direction.y = 0.0f;
            direction.Normalize();

            var rotation = Quaternion.LookRotation(direction);
            rotation = Quaternion.Slerp(Body.Rotation, rotation, Data.aimDamping);
            ((IMovableActor)this).SetRotation(rotation);
        }

        /// <summary>
        /// エイム座標の取得
        /// </summary>
        Vector3 IAimTarget.GetPosition() {
            return Body.Locators["Hips"].position;
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(IScope scope) {
            base.ActivateInternal(scope);

            // 移動解決用のクラスを設定
            MoveController.AddResolver(new StraightMoveResolver(Data.moveActionInfo.speed, Data.moveActionInfo.acceleration, Data.moveActionInfo.brake, Data.moveActionInfo.angularSpeed));
            scope.OnExpired += () => MoveController.RemoveResolver<StraightMoveResolver>();
            MoveController.AddResolver(new WarpMoveResolver());
            scope.OnExpired += () => MoveController.RemoveResolver<WarpMoveResolver>();
            MoveController.AddResolver(new DirectionMoveResolver(Data.moveActionInfo.speed, Data.moveActionInfo.angularSpeed));
            scope.OnExpired += () => MoveController.RemoveResolver<DirectionMoveResolver>();
            
            // 基本モーションの設定
            _basePlayableComponent = _motionController.Change(Data.controller, 0.0f, false);
            
            // 加算モーションレイヤーの追加
            _additiveMotionHandle = _motionController.AddExtensionLayer(true);
            _additiveMotionHandle.ScopeTo(scope);
        }
        
        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DeactivateInternal() {
            CancelAction();
            
            base.DeactivateInternal();
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal() {
            base.UpdateInternal();
            
            UpdateAnimationProperties();
        }

        /// <summary>
        /// 死亡状態の設定
        /// </summary>
        public void SetDeathFlag(bool death) {
            CancelAction();
            _basePlayableComponent.Playable.SetBool("death", death);
        }

        /// <summary>
        /// 行動キャンセル
        /// </summary>
        public void CancelAction() {
            _actionCts.Cancel();
            _actionCts = new CancellationTokenSource();
        }

        /// <summary>
        /// 指定方向への移動
        /// </summary>
        public void MoveDirection(Vector3 direction, float speedRate = 1.0f) {
            MoveController.MoveDirection<DirectionMoveResolver>(direction, speedRate);
        }

        /// <summary>
        /// 振動の再生
        /// </summary>
        public void Vibrate() {
            _additiveMotionHandle.Change(Data.damageActionInfo.vibrateClip, 0.0f);
        }

        /// <summary>
        /// ポイント指定移動
        /// </summary>
        /// <param name="point">移動先座標</param>
        /// <param name="ct">非同期キャンセル用トークン</param>
        public async UniTask MoveToPointAsync(Vector3 point, CancellationToken ct = default) {
            ct.ThrowIfCancellationRequested();

            IEnumerator Routine(CancellationToken token) {
                yield return MoveRoutine(point, token);
            }

            await PlayActionAsyncInternal(Routine, null, ct);
        }
        
        /// <summary>
        /// ジャンプアクションの再生
        /// </summary>
        public async UniTask JumpActionAsync(CancellationToken ct = default) {
            ct.ThrowIfCancellationRequested();

            IEnumerator Routine(CancellationToken token) {
                // アクション再生
                yield return PlayActionRoutine(Data.jumpActionInfo.action, token);
            }

            await PlayActionAsyncInternal(Routine, null, ct);
        }

        /// <summary>
        /// ダメージアクションの再生
        /// </summary>
        /// <param name="ct">非同期キャンセル用トークン</param>
        public async UniTask PlayDamageActionAsync(CancellationToken ct = default) {
            ct.ThrowIfCancellationRequested();

            IEnumerator Routine(CancellationToken token) {
                // アクション再生
                yield return PlayActionRoutine(Data.damageActionInfo.knockBackAction, token);
            }

            await PlayActionAsyncInternal(Routine, () => { MotionController.RootPositionScale = Vector3.one; }, ct);
        }

        /// <summary>
        /// スキルアクションの再生
        /// </summary>
        /// <param name="key">アクションを表すキー</param>
        /// <param name="ct">非同期キャンセル用トークン</param>
        public async UniTask PlaySkillActionAsync(string key, CancellationToken ct = default) {
            ct.ThrowIfCancellationRequested();

            var skillActionInfo = FindSkillActionInfo(key);
            if (skillActionInfo == null) {
                Debug.LogWarning($"Not found skill action. [{Data.name}{key}]");
                return;
            }

            IEnumerator Routine(CancellationToken token) {
                // アクション再生
                yield return PlayActionRoutine(skillActionInfo.action.GetAction(), token);
            }

            await PlayActionAsyncInternal(Routine, () => { MotionController.RootPositionScale = Vector3.one; }, ct);
        }

        /// <summary>
        /// デフォルトモーションに変更
        /// </summary>
        private void ChangeDefaultMotion(IActorAction action) {
            _motionController.Change(_basePlayableComponent, action.OutBlend);
        }

        /// <summary>
        /// 移動用ルーチン
        /// </summary>
        private IEnumerator MoveRoutine(Vector3 point, CancellationToken ct) {
            void Finished() {
                MoveController.Cancel();
            }

            var registration = ct.Register(Finished);

            // 移動処理
            yield return MoveController.MoveToPointAsync<StraightMoveResolver>(point);

            registration.Dispose();
            Finished();
        }

        /// <summary>
        /// アクション再生ルーチン
        /// </summary>
        private IEnumerator PlayActionRoutine(IActorAction action, CancellationToken ct) {
            var actionHandle = default(ActorActionPlayer.Handle);

            void Finished() {
                actionHandle.Dispose();
            }

            var registration = ct.Register(Finished);

            // アクション再生
            actionHandle = ActionPlayer.Play(action, ChangeDefaultMotion);

            yield return actionHandle;

            registration.Dispose();
        }

        /// <summary>
        /// アクション再生の内部処理
        /// </summary>
        /// <param name="getCoroutineFunc">コルーチン取得用関数</param>
        /// <param name="onCancelAction">キャンセル時の処理</param>
        /// <param name="ct">キャンセル用トークン</param>
        private async UniTask PlayActionAsyncInternal(Func<CancellationToken, IEnumerator> getCoroutineFunc, Action onCancelAction, CancellationToken ct) {
            // 現在のアクションをキャンセル
            ResetActionCancellationHandle();

            // CancellationTokenの合成
            ct = CancellationTokenSource.CreateLinkedTokenSource(_actionCts.Token, ct).Token;

            var handle = ct.Register(() => onCancelAction?.Invoke());

            // コルーチンとして処理を実行
            await StartCoroutineAsync(getCoroutineFunc(ct), ct);

            await handle.DisposeAsync();
        }

        /// <summary>
        /// アクションキャンセル用ハンドルのリセット
        /// </summary>
        private void ResetActionCancellationHandle() {
            if (_actionCts != null) {
                _actionCts.Cancel();
                _actionCts.Dispose();
            }

            _actionCts = new CancellationTokenSource();
        }

        /// <summary>
        /// アニメーション用プロパティの反映
        /// </summary>
        private void UpdateAnimationProperties() {
            _basePlayableComponent.Playable.SetFloat("speed", MoveController.Velocity.magnitude / Data.moveActionInfo.speed);
        }

        /// <summary>
        /// スキルアクション情報を探す
        /// </summary>
        private BattleCharacterActorSetupData.SkillActionInfo FindSkillActionInfo(string key) {
            return Data.skillActionInfos.FirstOrDefault(x => x.key == key);
        }
    }
}