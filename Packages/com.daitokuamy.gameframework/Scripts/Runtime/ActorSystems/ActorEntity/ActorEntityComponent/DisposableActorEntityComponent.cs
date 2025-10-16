using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace GameFramework.ActorSystems {
    /// <summary>
    /// Disposableなオブジェクトをエンティティに紐づけるコンポーネント
    /// </summary>
    [Preserve]
    public sealed class DisposableActorEntityComponent : ActorEntityComponent {
        private readonly List<IDisposable> _disposables = new();

        /// <summary>
        /// Disposableの追加
        /// </summary>
        public ActorEntity Add(IDisposable disposable) {
            if (_disposables.Contains(disposable)) {
                return Entity;
            }

            _disposables.Add(disposable);
            return Entity;
        }

        /// <summary>
        /// Disposableの除外
        /// </summary>
        public ActorEntity Remove(IDisposable disposable, bool dispose = true) {
            if (_disposables.Remove(disposable)) {
                disposable.Dispose();
            }

            return Entity;
        }

        /// <summary>
        /// 廃棄処理(override用)
        /// </summary>
        protected override void DisposeInternal() {
            for (var i = _disposables.Count - 1; i >= 0; i--) {
                _disposables[i].Dispose();
            }

            _disposables.Clear();
        }
    }
}