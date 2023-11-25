namespace GameFramework.Core {    
    /// <summary>
    /// AsyncOperationHandle用の拡張メソッド
    /// </summary>
    public static class AsyncOperationHandleExtensions {      
        /// <summary>
        /// Awaiterの取得
        /// </summary>
        public static AsyncOperationHandleAwaiter GetAwaiter(this AsyncOperationHandle source) {
            return new AsyncOperationHandleAwaiter(source);
        }

        /// <summary>
        /// Awaiterの取得
        /// </summary>
        public static AsyncOperationHandleAwaiter<T> GetAwaiter<T>(this AsyncOperationHandle<T> source) {
            return new AsyncOperationHandleAwaiter<T>(source);
        }
    }
}