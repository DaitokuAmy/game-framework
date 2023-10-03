using System;
using System.Collections;
using System.Collections.Generic;
using GameFramework.Core;
using UnityEngine;
using Coroutine = GameFramework.CoroutineSystems.Coroutine;

namespace GameFramework.ActorSystems {
    /// <summary>
    /// ActorActionの再生用クラス
    /// </summary>
    public class ActorActionPlayer : IDisposable {
        /// <summary>
        /// 再生管理用ハンドル
        /// </summary>
        public struct Handle : IProcess, IDisposable {
            private readonly PlayingInfo _playingInfo;

            /// <summary>有効なハンドルか</summary>
            public bool IsValid => _playingInfo != null && _playingInfo.IsValid;
            /// <summary>再生中か</summary>
            public bool IsPlaying => _playingInfo != null && _playingInfo.IsPlaying;
            /// <summary>実行エラーか</summary>
            public Exception Exception => _playingInfo?.Exception;
            /// <summary>再生に使っているResolver</summary>
            public IActorActionResolver Resolver => _playingInfo?.Resolver;
            
            /// <summary>未使用</summary>
            object IEnumerator.Current => null;
            /// <summary>完了しているか</summary>
            bool IProcess.IsDone => !IsPlaying;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            internal Handle(PlayingInfo playingInfo) {
                _playingInfo = playingInfo;
            }

            /// <summary>
            /// IEnumerator判定用
            /// </summary>
            bool IEnumerator.MoveNext() {
                return IsPlaying;
            }

            /// <summary>
            /// 未使用
            /// </summary>
            void IEnumerator.Reset() {
            }

            /// <summary>
            /// 廃棄時処理
            /// </summary>
            public void Dispose() {
                if (!IsValid) {
                    return;
                }
                
                CancelAction();
            }
            
            /// <summary>
            /// アクションを次に遷移
            /// </summary>
            /// <returns>遷移に成功したか</returns>
            public bool NextAction(params object[] args) {
                if (!IsValid) {
                    return false;
                }
                
                return _playingInfo.NextAction(args);
            }

            /// <summary>
            /// アクションのキャンセル
            /// </summary>
            public void CancelAction() {
                if (!IsValid) {
                    return;
                }
                
                _playingInfo.CancelAction();
            }
        }

        /// <summary>
        /// 再生中情報
        /// </summary>
        internal class PlayingInfo {
            private Coroutine _coroutine;
            private Action _onFinishAction;

            /// <summary>有効か</summary>
            public bool IsValid => _coroutine != null;
            /// <summary>再生中か</summary>
            public bool IsPlaying => Resolver != null && Resolver.IsPlaying;
            /// <summary>実行エラー</summary>
            public Exception Exception { get; private set; }
            /// <summary>実行用Resolver</summary>
            public IActorActionResolver Resolver { get; private set; }
            /// <summary>実行用Action</summary>
            private IActorAction Action { get; set; }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public PlayingInfo(IActorAction action, IActorActionResolver resolver, Coroutine coroutine, Action onFinishAction) {
                Action = action;
                Resolver = resolver;
                _coroutine = coroutine;
                _onFinishAction = onFinishAction;
            }

            /// <summary>
            /// 更新処理
            /// </summary>
            public bool Update() {
                if (!IsValid) {
                    return false;
                }

                try {
                    // コルーチン実行
                    if (!((IEnumerator)_coroutine).MoveNext()) {
                        FinishAction();
                        return false;
                    }
                }
                catch (Exception exception) {
                    // エラー発生
                    Exception = exception;
                    Debug.LogException(exception);
                    FinishAction();
                    return false;
                }
                
                return true;
            }

            /// <summary>
            /// アクションを次に遷移させる
            /// </summary>
            public bool NextAction(object[] args) {
                if (!IsValid) {
                    return false;
                }

                return Resolver.NextAction(args);
            }

            /// <summary>
            /// アクションのキャンセル
            /// </summary>
            public void CancelAction() {
                if (!IsValid) {
                    return;
                }
                
                Resolver.CancelAction();
                FinishAction();
            }

            /// <summary>
            /// Actionの終了処理
            /// </summary>
            private void FinishAction() {
                _coroutine = null;
                Resolver = null;
                Action = null;
                _onFinishAction?.Invoke();
                _onFinishAction = null;
            }
        }

        private readonly LayeredTime _layeredTime;
        private readonly List<IActorActionResolver> _resolvers = new();
        
        private PlayingInfo _playingInfo;
        private bool _disposed;

        /// <summary>アクション再生中か</summary>
        public bool IsPlaying => _playingInfo != null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ActorActionPlayer(LayeredTime layeredTime) {
            _layeredTime = layeredTime;
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            if (_disposed) {
                return;
            }

            _disposed = true;
            if (_playingInfo != null) {
                _playingInfo.CancelAction();
                _playingInfo = null;
            }
            _resolvers.Clear();
        }
        
        /// <summary>
        /// Action再生用のResolverの設定
        /// </summary>
        public void AddResolver(IActorActionResolver resolver) {
            if (_disposed) {
                return;
            }
            
            if (resolver == null) {
                return;
            }
            
            resolver.Initialize(_layeredTime);
            _resolvers.Add(resolver);
        }

        /// <summary>
        /// Action再生用のResolverを削除
        /// </summary>
        public void RemoveResolver(IActorActionResolver resolver) {
            if (_disposed) {
                return;
            }
            
            _resolvers.Remove(resolver);
        }

        /// <summary>
        /// 該当のActionが再生可能なResolverを全て削除
        /// </summary>
        public void RemoveResolvers<T>()
            where T : IActorAction {
            if (_disposed) {
                return;
            }
            
            for (var i = _resolvers.Count - 1; i >= 0; i--) {
                if (_resolvers[i].CheckActionable<T>()) {
                    _resolvers.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Action再生用のResolverを全削除
        /// </summary>
        public void RemoveResolvers() {
            if (_disposed) {
                return;
            }
            
            _resolvers.Clear();
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        public void Update() {
            if (_disposed) {
                return;
            }

            if (_playingInfo != null) {
                if (!_playingInfo.Update()) {
                    _playingInfo = null;
                }
            }
        }

        /// <summary>
        /// Actionの再生ルーチン
        /// </summary>
        public Handle Play(IActorAction action, Action<IActorAction> onFinishAction = null, params object[] args) {
            if (_disposed) {
                Debug.LogWarning("Player is disposed.");
                return new Handle();
            }
            
            // 既に再生中の物があれば停止する
            if (_playingInfo != null) {
                _playingInfo.CancelAction();
                _playingInfo = null;
            }
            
            // Resolverを探す
            var foundResolver = FindResolver(action);
            if (foundResolver == null) {
                Debug.LogWarning("Not found resolver.");
                return new Handle();
            }

            // Actionの再生
            foundResolver.PrePlayAction(action, args);
            var coroutine = new Coroutine(foundResolver.PlayActionRoutine(action, args));
            _playingInfo = new PlayingInfo(action, foundResolver, coroutine, () => onFinishAction?.Invoke(action));
            return new Handle(_playingInfo);
        }

        /// <summary>
        /// Resolverを探す
        /// </summary>
        private IActorActionResolver FindResolver(IActorAction action) {
            return _resolvers.Find(x => x.CheckActionable(action));
        }
    }
}