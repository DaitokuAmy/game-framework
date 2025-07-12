using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace GameFramework.CoroutineSystems {
    /// <summary>
    /// コルーチン実行クラス
    /// </summary>
    public class CoroutineRunner : IDisposable {
        /// <summary>
        /// コルーチン情報
        /// </summary>
        private class CoroutineInfo {
            public Coroutine Coroutine;
            public IDisposable Disposable;

            public event Action<Exception> ErrorEvent;
            public event Action CanceledEvent;
            public event Action CompletedEvent;

            private bool _isCanceled;
            private Exception _exception;
            private bool _isCompleted;

            public bool IsDone => _isCanceled || _exception != null || _isCompleted;

            /// <summary>
            /// 完了処理
            /// </summary>
            public void Complete() {
                if (IsDone) {
                    return;
                }

                var onCompletedInternal = CompletedEvent;

                _isCompleted = true;
                CanceledEvent = null;
                ErrorEvent = null;
                CompletedEvent = null;
                Disposable?.Dispose();
                Disposable = null;

                onCompletedInternal?.Invoke();
            }

            /// <summary>
            /// キャンセル処理
            /// </summary>
            public void Cancel() {
                if (IsDone) {
                    return;
                }

                var onCanceledInternal = CanceledEvent;

                _isCanceled = true;
                CanceledEvent = null;
                ErrorEvent = null;
                CompletedEvent = null;
                Disposable?.Dispose();
                Disposable = null;

                onCanceledInternal?.Invoke();
            }

            /// <summary>
            /// 例外処理
            /// </summary>
            /// <param name="error">例外内容</param>
            public void Abort(Exception error) {
                if (IsDone) {
                    return;
                }

                var onErrorInternal = ErrorEvent;

                _exception = error;
                CanceledEvent = null;
                ErrorEvent = null;
                CompletedEvent = null;
                Disposable?.Dispose();
                Disposable = null;

                onErrorInternal?.Invoke(error);
            }
        }

        // 制御中のコルーチン
        private readonly List<CoroutineInfo> _coroutineInfos = new();
        // 更新が完了したコルーチンのIDを保持するリスト
        private readonly List<int> _cachedRemoveIndices = new();

        /// <summary>
        /// コルーチンの開始
        /// </summary>
        /// <param name="enumerator">実行する非同期処理</param>
        /// <param name="onCompleted">完了時の通知</param>
        /// <param name="onCanceled">キャンセル時の通知</param>
        /// <param name="onError">エラー時の通知</param>
        /// <param name="cancellationToken">キャンセルハンドリング用</param>
        public Coroutine StartCoroutine(IEnumerator enumerator, Action onCompleted = null,
            Action onCanceled = null, Action<Exception> onError = null, CancellationToken cancellationToken = default) {
            if (enumerator == null) {
                Debug.LogError("Invalid coroutine func.");
                return null;
            }

            // コルーチンの追加
            var coroutineInfo = new CoroutineInfo {
                Coroutine = new Coroutine(enumerator)
            };

            // イベント登録
            coroutineInfo.CompletedEvent += onCompleted;
            coroutineInfo.CanceledEvent += onCanceled;

            // すでにキャンセル済
            if (cancellationToken.IsCancellationRequested) {
                coroutineInfo.Cancel();
                return coroutineInfo.Coroutine;
            }

            // キャンセルの監視
            coroutineInfo.Disposable = cancellationToken.Register(() => coroutineInfo.Cancel());

            // エラーハンドリング用のアクションが無い時はログを出力するようにする
            coroutineInfo.ErrorEvent += onError ?? Debug.LogException;

            _coroutineInfos.Add(coroutineInfo);

            return coroutineInfo.Coroutine;
        }

        /// <summary>
        /// コルーチンの強制停止
        /// </summary>
        /// <param name="coroutine">停止させる対象のCoroutine</param>
        public void StopCoroutine(Coroutine coroutine) {
            if (coroutine == null) {
                Debug.LogError("Invalid coroutine instance.");
                return;
            }

            if (coroutine.IsDone) {
                return;
            }

            var foundIndex = _coroutineInfos.FindIndex(x => x.Coroutine == coroutine);
            if (foundIndex < 0) {
                Debug.LogError("Not found coroutine.");
                return;
            }

            // キャンセル処理
            var info = _coroutineInfos[foundIndex];
            info.Cancel();
        }

        /// <summary>
        /// コルーチンの全停止
        /// </summary>
        public void StopAllCoroutines() {
            // 降順にキャンセルしていく
            for (var i = _coroutineInfos.Count - 1; i >= 0; i--) {
                var info = _coroutineInfos[i]; // キャンセル処理
                info.Cancel();
            }

            _coroutineInfos.Clear();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            // 全コルーチン停止
            StopAllCoroutines();
        }

        /// <summary>
        /// コルーチン更新処理
        /// </summary>
        public void Update() {
            // コルーチンの更新
            _cachedRemoveIndices.Clear();

            // 昇順で更新
            for (var i = 0; i < _coroutineInfos.Count; i++) {
                var coroutineInfo = _coroutineInfos[i];
                var coroutine = coroutineInfo.Coroutine;

                // すでに完了しているCoroutineの場合はそのまま除外
                if (coroutineInfo.IsDone) {
                    _cachedRemoveIndices.Add(i);
                    continue;
                }

                if (!coroutine.MoveNext()) {
                    if (coroutine.Exception != null) {
                        // エラー終了通知
                        coroutineInfo.Abort(coroutine.Exception);
                        _cachedRemoveIndices.Add(i);
                    }
                    else {
                        // 完了通知
                        coroutineInfo.Complete();
                        _cachedRemoveIndices.Add(i);
                    }
                }
            }

            // 降順でインスタンスクリア
            for (var i = _cachedRemoveIndices.Count - 1; i >= 0; i--) {
                var removeIndex = _cachedRemoveIndices[i];
                if (removeIndex < 0 || removeIndex >= _coroutineInfos.Count) {
                    continue;
                }

                _coroutineInfos.RemoveAt(removeIndex);
            }
        }
    }
}