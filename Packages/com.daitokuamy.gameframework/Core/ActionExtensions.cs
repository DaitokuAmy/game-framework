using System;

namespace GameFramework.Core {
    /// <summary>
    /// Action用の拡張メソッド
    /// </summary>
    public static class ActionExtensions {
        /// <summary>
        /// Actionの逆順実行
        /// </summary>
        public static void InvokeDescending(this Action source) {
            var methods = source.GetInvocationList();
            for (var i = methods.Length - 1; i >= 0; i--) {
                var action = (Action)methods[i];
                action();
            }
        }
    }
}