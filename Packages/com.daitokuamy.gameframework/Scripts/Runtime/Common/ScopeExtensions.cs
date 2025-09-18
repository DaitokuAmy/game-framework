using System;
using System.Threading;
using UnityEngine;
using Object = UnityEngine.Object;
using GameFramework.Core;

namespace GameFramework {
    /// <summary>
    /// IScope用の拡張メソッド
    /// </summary>
    public static class ScopeExtensions {
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