using System;
using System.Collections.Generic;

namespace GameFramework.TweenSystem {
    /// <summary>
    /// Tween再生を管理（MonoBehaviour不要/Tick駆動/Pool内蔵）
    /// </summary>
    public sealed class TweenPlayer : IDisposable {
        /// <summary>
        /// 再生管理用エントリ
        /// </summary>
        private struct Entry {
            /// <summary>世代</summary>
            public int Version;
            /// <summary>生存中かどうか</summary>
            public bool Alive;
            /// <summary>対象Tween</summary>
            public Tween Tween;
        }

        private readonly TweenPoolRegistry _registry = new();

        private readonly List<Entry> _entries = new();
        private readonly List<int> _playingIds = new();
        private readonly Stack<int> _freeIds = new();
        private readonly Stack<Tween> _returnStack = new();
        private readonly HashSet<Tween> _returned = new();

        private bool _disposed;

        /// <inheritdoc/>
        public void Dispose() {
            if (_disposed) {
                return;
            }

            _disposed = true;

            for (var i = _playingIds.Count - 1; i >= 0; i--) {
                var id = _playingIds[i];
                if (_entries[id].Alive) {
                    _entries[id].Tween.Kill();
                    CleanupEntry(id);
                }
            }

            _playingIds.Clear();
            _freeIds.Clear();
            _entries.Clear();
            _registry.Clear();
        }

        /// <summary>
        /// Sequenceを生成
        /// </summary>
        public Sequence CreateSequence() {
            var seq = _registry.Get<Sequence>();
            seq.Setup(this);
            return seq;
        }

        /// <summary>
        /// Tweenerを生成
        /// </summary>
        public TTweener CreateTweener<TTweener>()
            where TTweener : Tweener, new() {
            var t = _registry.Get<TTweener>();
            return t;
        }

        /// <summary>
        /// Tweenを再生
        /// </summary>
        public TweenHandle Play(Tween tween) {
            if (_disposed) {
                throw new ObjectDisposedException(nameof(TweenPlayer));
            }

            if (tween == null) {
                throw new ArgumentNullException(nameof(tween));
            }

            if (!tween.IsIdleInternal) {
                throw new InvalidOperationException("Tween is already active.");
            }

            tween.BeginInternal();

            var id = AllocateEntry(tween);
            _playingIds.Add(id);

            var version = _entries[id].Version;
            return new TweenHandle(this, id, version);
        }

        /// <summary>
        /// 更新
        /// </summary>
        public void Tick(float deltaTime) {
            if (_disposed) {
                return;
            }

            for (var i = _playingIds.Count - 1; i >= 0; i--) {
                var id = _playingIds[i];
                var entry = _entries[id];

                if (!entry.Alive) {
                    _playingIds.RemoveAt(i);
                    continue;
                }

                var tween = entry.Tween;

                if (tween.IsKilled) {
                    _playingIds.RemoveAt(i);
                    CleanupEntry(id);
                    continue;
                }

                if (!tween.IsComplete && !tween.IsPaused) {
                    tween.TickInternal(deltaTime);
                }

                if (tween.IsComplete) {
                    _playingIds.RemoveAt(i);

                    if (tween.AutoKill) {
                        CleanupEntry(id);
                    }
                }
            }
        }

        /// <summary>
        /// Handleの有効性を判定
        /// </summary>
        public bool IsHandleValid(int id, int version) {
            if ((uint)id >= (uint)_entries.Count) {
                return false;
            }

            var e = _entries[id];
            return e.Alive && e.Version == version && e.Tween != null;
        }

        /// <summary>
        /// 指定Handleが完了しているかを判定
        /// </summary>
        public bool IsCompleted(int id, int version) {
            if (!IsHandleValid(id, version)) {
                return true;
            }

            return _entries[id].Tween.IsComplete;
        }

        /// <summary>
        /// 指定HandleのTweenをキャンセル（Kill）
        /// </summary>
        internal void Cancel(int id, int version) {
            if (!IsHandleValid(id, version)) {
                return;
            }

            _entries[id].Tween.Kill();
        }

        /// <summary>
        /// 指定HandleのTweenを強制完了
        /// </summary>
        internal void ForceComplete(int id, int version) {
            if (!IsHandleValid(id, version)) {
                return;
            }

            _entries[id].Tween.ForceComplete();
        }

        /// <summary>
        /// 再生中の全Tweenをキャンセル（Kill）
        /// </summary>
        public void CancelAll() {
            for (var i = _playingIds.Count - 1; i >= 0; i--) {
                var id = _playingIds[i];
                var entry = _entries[id];
                if (!entry.Alive) {
                    _playingIds.RemoveAt(i);
                    continue;
                }

                entry.Tween.Kill();
                _playingIds.RemoveAt(i);
                CleanupEntry(id);
            }
        }

        /// <summary>
        /// 再生中の全Tweenを強制完了
        /// </summary>
        public void ForceCompleteAll() {
            for (var i = _playingIds.Count - 1; i >= 0; i--) {
                var id = _playingIds[i];
                var entry = _entries[id];
                if (!entry.Alive) {
                    _playingIds.RemoveAt(i);
                    continue;
                }

                var tween = entry.Tween;
                tween.ForceComplete();
                _playingIds.RemoveAt(i);

                if (tween.AutoKill || tween.IsKilled) {
                    CleanupEntry(id);
                }
            }
        }

        /// <summary>
        /// 指定HandleのTweenを一時停止
        /// </summary>
        public void Pause(int id, int version) {
            if (!IsHandleValid(id, version)) {
                return;
            }

            _entries[id].Tween.Pause();
        }

        /// <summary>
        /// 指定HandleのTweenを再開
        /// </summary>
        public void Resume(int id, int version) {
            if (!IsHandleValid(id, version)) {
                return;
            }

            _entries[id].Tween.Resume();
        }

        /// <summary>
        /// 再生エントリを確保
        /// </summary>
        private int AllocateEntry(Tween tween) {
            int id;

            if (_freeIds.Count > 0) {
                id = _freeIds.Pop();
                var e = _entries[id];
                e.Alive = true;
                e.Tween = tween;
                e.Version++;
                _entries[id] = e;
                return id;
            }

            id = _entries.Count;
            _entries.Add(new Entry { Alive = true, Tween = tween, Version = 1 });
            return id;
        }

        /// <summary>
        /// 再生エントリを回収し、TweenをPoolに返却
        /// </summary>
        private void CleanupEntry(int id) {
            var tween = _entries[id].Tween;

            ReturnTween(tween);

            var e = _entries[id];
            e.Alive = false;
            e.Tween = null!;
            _entries[id] = e;

            _freeIds.Push(id);
        }

        /// <summary>
        /// TweenをPoolに返却（子→親の順）
        /// </summary>
        private void ReturnTween(Tween root) {
            _returnStack.Push(root);

            try {
                while (_returnStack.Count > 0) {
                    var t = _returnStack.Pop();
                    if (!_returned.Add(t)) {
                        continue;
                    }

                    if (t is Sequence seq) {
                        var count = seq.GetChildCount();
                        for (var i = 0; i < count; i++) {
                            var child = seq.GetChild(i);
                            if (child != null) {
                                _returnStack.Push(child);
                            }
                        }

                        seq.ResetInternal();
                        _registry.Release(seq);
                        continue;
                    }

                    t.ResetInternal();
                    _registry.Release(t);
                }
            } finally {
                _returnStack.Clear();
                _returned.Clear();
            }
        }
    }
}
