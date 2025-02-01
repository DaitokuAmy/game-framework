using System;
using System.Collections;
using GameFramework.Core;
using UnityEngine.SceneManagement;

namespace GameFramework.AssetSystems {
    /// <summary>
    /// シーンアセットリクエスト用ハンドル
    /// </summary>
    public struct SceneAssetHandle : IProcess<Scene> {
        // 無効なSceneAssetHandle
        public static readonly SceneAssetHandle Empty = new SceneAssetHandle();

        // 読み込み情報
        private ISceneAssetInfo _info;

        // 読み込み完了しているか
        public bool IsDone => _info == null || _info.IsDone;
        // シーン
        public Scene Scene => _info?.Scene ?? new Scene();
        // エラー
        public Exception Exception => _info?.Exception ?? null;
        // 有効なハンドルか
        public bool IsValid => _info != null;
        // 結果
        Scene IProcess<Scene>.Result => Scene;
        // IEnumerator用
        object IEnumerator.Current => null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="info">読み込み管理用情報</param>
        public SceneAssetHandle(ISceneAssetInfo info) {
            _info = info;
        }

        /// <summary>
        /// アクティブ化
        /// </summary>
        public AsyncOperationHandle ActivateAsync() {
            if (_info == null) {
                return AsyncOperationHandle.CanceledHandle;
            }

            var asyncOperation = _info.ActivateAsync();
            if (asyncOperation == null) {
                return AsyncOperationHandle.CompletedHandle;
            }

            var asyncOperator = new AsyncOperator();
            asyncOperation.completed += _ => asyncOperator.Completed();
            return asyncOperator;
        }

        /// <summary>
        /// 読み込んだアセットの解放
        /// </summary>
        public void Release() {
            if (_info == null) {
                return;
            }

            _info.Dispose();
            _info = null;
        }

        /// <inheritdoc/>
        bool IEnumerator.MoveNext() {
            return !IsDone;
        }

        /// <inheritdoc/>
        void IEnumerator.Reset() {
            throw new NotImplementedException();
        }
    }
}