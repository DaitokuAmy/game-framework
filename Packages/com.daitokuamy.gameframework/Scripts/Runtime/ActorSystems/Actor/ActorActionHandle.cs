using System;
using System.Collections;
using GameFramework.Core;

namespace GameFramework.ActorSystems {
    /// <summary>
    /// ActorActionの非同期ハンドリング用
    /// </summary>
    public readonly struct ActorActionHandle : IEventProcess {
        /// <summary>無効なハンドル</summary>
        public static readonly ActorActionHandle Empty = new();

        private readonly ActorActionPlayer.PlayingInfo _playingInfo;

        /// <inheritdoc/>
        Exception IProcess.Exception => null;
        /// <inheritdoc/>
        event Action IEventProcess.ExitEvent {
            add {
                if (_playingInfo != null) {
                    _playingInfo.FinishedEvent += value;
                }
            }
            remove {
                if (_playingInfo != null) {
                    _playingInfo.FinishedEvent -= value;
                }
            }
        }

        /// <summary>完了しているか</summary>
        public bool IsDone => _playingInfo == null || _playingInfo.IsDone;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal ActorActionHandle(ActorActionPlayer.PlayingInfo playingInfo) {
            _playingInfo = playingInfo;
        }

        /// <inheritdoc/>
        object IEnumerator.Current => null;

        /// <inheritdoc/>
        bool IEnumerator.MoveNext() {
            return !IsDone;
        }

        /// <inheritdoc/>
        void IEnumerator.Reset() {
        }
        
        /// <summary>
        /// アクションの遷移
        /// </summary>
        public bool Next(object[] args) {
            if (_playingInfo == null) {
                return false;
            }
            
            return _playingInfo.Next(args);
        } 

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop() {
            if (_playingInfo == null) {
                return;
            }
            
            _playingInfo.Cancel();
        }
    }
}