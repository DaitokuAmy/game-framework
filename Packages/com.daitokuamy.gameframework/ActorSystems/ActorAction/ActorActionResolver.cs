using System.Collections;
using GameFramework.Core;

namespace GameFramework.ActorSystems {
    /// <summary>
    /// ActorAction再生用のインターフェース
    /// </summary>
    public interface IActorActionResolver {
        /// <summary>再生中か</summary>
        bool IsPlaying { get; }

        /// <summary>
        /// 初期化処理
        /// </summary>
        void Initialize(LayeredTime layeredTime);
        
        /// <summary>
        /// ActorActionを再生可能か
        /// </summary>
        bool CheckActionable(IActorAction action);
        
        /// <summary>
        /// ActorActionを再生可能か
        /// </summary>
        bool CheckActionable<T>()
            where T : IActorAction;

        /// <summary>
        /// Action再生直前処理
        /// </summary>
        void PrePlayAction(IActorAction rawAction, object[] args);

        /// <summary>
        /// Actionの再生ルーチン
        /// </summary>
        IEnumerator PlayActionRoutine(IActorAction rawAction, object[] args);

        /// <summary>
        /// Actionの遷移
        /// </summary>
        bool NextAction(object[] args);

        /// <summary>
        /// Actionのキャンセル
        /// </summary>
        void CancelAction();
    }
    
    /// <summary>
    /// ActorAction再生制御用クラス基底
    /// </summary>
    public abstract class ActorActionResolver<TActorAction> : IActorActionResolver
        where TActorAction : class, IActorAction {
        private bool _initialized;
        private bool _isPlaying;
        
        /// <summary>時間管理用</summary>
        protected LayeredTime LayeredTime { get; private set; }

        /// <summary>再生中か</summary>
        bool IActorActionResolver.IsPlaying => _isPlaying;
        
        /// <summary>
        /// ActorActionを再生可能か
        /// </summary>
        bool IActorActionResolver.CheckActionable(IActorAction action) {
            return action is TActorAction;
        }

        /// <summary>
        /// ActorActionを再生可能か
        /// </summary>
        bool IActorActionResolver.CheckActionable<T>() {
            var type = typeof(T);
            var selfType = typeof(TActorAction);
            return type == selfType || type.IsSubclassOf(selfType);
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        void IActorActionResolver.Initialize(LayeredTime layeredTime) {
            if (_initialized) {
                return;
            }
            
            _initialized = true;
            LayeredTime = layeredTime;
        }

        /// <summary>
        /// Action再生直前処理
        /// </summary>
        void IActorActionResolver.PrePlayAction(IActorAction rawAction, object[] args) {
            if (_isPlaying) {
                ((IActorActionResolver)this).CancelAction();
            }

            _isPlaying = true;
        }

        /// <summary>
        /// Actionの再生ルーチン
        /// </summary>
        IEnumerator IActorActionResolver.PlayActionRoutine(IActorAction rawAction, object[] args) {
            var action = rawAction as TActorAction;
            if (action != null) {
                // Actionの再生
                yield return PlayActionRoutineInternal(action, args);
            }
            
            _isPlaying = false;
        }

        /// <summary>
        /// アクションの遷移
        /// </summary>
        bool IActorActionResolver.NextAction(object[] args) {
            if (!_isPlaying) {
                return false;
            }

            return NextActionInternal(args);
        }
        
        /// <summary>
        /// アクションのキャンセル
        /// </summary>
        void IActorActionResolver.CancelAction(){
            if (!_isPlaying) {
                return;
            }

            _isPlaying = false;

            CancelActionInternal();
        }

        /// <summary>
        /// アクションクリップの再生コルーチン
        /// </summary>
        protected virtual IEnumerator PlayActionRoutineInternal(TActorAction action, object[] args) {
            yield break;
        }

        /// <summary>
        /// アクションの遷移
        /// </summary>
        protected virtual bool NextActionInternal(object[] args) {
            return false;
        }

        /// <summary>
        /// アクションのキャンセル処理
        /// </summary>
        protected virtual void CancelActionInternal() {
        }
    }
}