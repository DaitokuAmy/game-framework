using System;
using System.Threading;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameFramework.Core {
    /// <summary>
    /// IScope用の拡張メソッド
    /// </summary>
    public static class ScopeExtensions {
        /// <summary>
        /// スコープが有効か
        /// </summary>
        public static bool IsValid(this IScope source) {
            return source != null && !source.Token.IsCancellationRequested;
        }

        /// <summary>
        /// CancellationTokenのScope変換
        /// </summary>
        public static IScope ToScope(this CancellationToken source) {
            var scope = new DisposableScope();
            source.Register(() => { scope.Dispose(); });
            return scope;
        }

        /// <summary>
        /// IDisposableのScope登録
        /// </summary>
        public static T RegisterTo<T>(this T source, IScope scope)
            where T : IDisposable {
            if (scope == null) {
                return source;
            }

            scope.ExpiredEvent += source.Dispose;
            return source;
        }

        /// <summary>
        /// GameObjectのScope登録
        /// </summary>
        public static GameObject RegisterTo(this GameObject source, IScope scope, bool immediate = false) {
            scope.ExpiredEvent += () => {
                if (source != null) {
                    if (immediate) {
                        Object.DestroyImmediate(source);
                    }
                    else {
                        Object.Destroy(source);
                    }
                }
            };
            return source;
        }

        /// <summary>
        /// ComponentのScope登録
        /// </summary>
        public static T RegisterTo<T>(this Component source, IScope scope, bool immediate = false)
            where T : Component {
            scope.ExpiredEvent += () => {
                if (source != null) {
                    if (immediate) {
                        Object.DestroyImmediate(source);
                    }
                    else {
                        Object.Destroy(source);
                    }
                }
            };
            return source as T;
        }
    }
}