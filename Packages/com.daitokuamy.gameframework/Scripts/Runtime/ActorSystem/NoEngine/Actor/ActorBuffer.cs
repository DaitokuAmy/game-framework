using System;
using System.Collections.Generic;
using GameFramework.Pooling;

namespace GameFramework.ActorSystem {
    /// <summary>
    /// アクター用のバッファ
    /// </summary>
    internal sealed class ActorBuffer<TContent> : IDisposable
        where TContent : class, IActorBufferContent, new() {
        /// <summary>
        /// IActorBufferContentのソート用
        /// </summary>
        private class Comparer : IComparer<IActorBufferContent> {
            /// <inheritdoc/>
            int IComparer<IActorBufferContent>.Compare(IActorBufferContent x, IActorBufferContent y) {
                if (x == null || y == null) {
                    return 0;
                }

                // 低い方優先
                return x.Order.CompareTo(y.Order);
            }
        }

        private readonly Dictionary<Type, ObjectPool<TContent>> _poolMap = new();
        private readonly List<TContent> _bufferContents = new();
        private readonly List<TContent> _gotContents = new();
        private readonly Comparer _comparer = new();

        private bool _disposed;

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            if (_disposed) {
                return;
            }

            _disposed = true;

            foreach (var content in _bufferContents) {
                var pool = _poolMap[content.GetType()];
                pool.Release(content);
            }

            _bufferContents.Clear();

            foreach (var content in _gotContents) {
                _poolMap[content.GetType()].Release(content);
            }

            _gotContents.Clear();

            foreach (var pool in _poolMap.Values) {
                pool.Dispose();
            }

            _poolMap.Clear();
        }

        /// <summary>
        /// BufferContentの生成(Pool管理)
        /// </summary>
        public T CreateContent<T>()
            where T : TContent, new() {
            var type = typeof(T);
            if (!_poolMap.TryGetValue(type, out var pool)) {
                pool = new ObjectPool<TContent>(() => {
                    var instance = new T();
                    instance.OnCreated();
                    return instance;
                }, actionOnRelease: content => content.OnReleased());
                _poolMap.Add(type, pool);
            }

            var instance = (T)pool.Get();
            _gotContents.Add(instance);
            return instance;
        }

        /// <summary>
        /// BufferContentの追加
        /// </summary>
        /// <param name="content">追加対象のContent</param>
        public void AddContent(TContent content) {
            _bufferContents.Add(content);
            _gotContents.Remove(content);
        }

        /// <summary>
        /// Bufferの中身を取得
        /// </summary>
        public void GetBufferedContents(List<TContent> contents) {
            contents.Clear();
            
            foreach (var content in _bufferContents) {
                var pool = _poolMap[content.GetType()];
                pool.Release(content);
                contents.Add(content);
            }

            contents.Sort(_comparer);
            _bufferContents.Clear();
        }
    }
}
