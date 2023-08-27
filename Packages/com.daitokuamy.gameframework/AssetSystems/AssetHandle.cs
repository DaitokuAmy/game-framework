using System;
using System.Collections;
using GameFramework.Core;
using Object = UnityEngine.Object;

namespace GameFramework.AssetSystems {
    /// <summary>
    /// アセット用ハンドル
    /// </summary>
    public struct AssetHandle<T> : IProcess<T>
        where T : Object {
        // 無効なAssetHandle
        public static readonly AssetHandle<T> Empty = new AssetHandle<T>();
        // IEnumerator用
        object IEnumerator.Current => null;

        // 読み込みリクエスト情報
        private IAssetInfo<T> _info;

        // 読み込み完了しているか
        public bool IsDone => _info == null || _info.IsDone;
        // 読み込んだアセット
        public T Asset => _info?.Asset;
        // エラー
        public Exception Exception => _info?.Exception;
        // 有効なハンドルか
        public bool IsValid => _info != null;
        // 結果
        T IProcess<T>.Result => Asset;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="info">読み込み管理用情報</param>
        public AssetHandle(IAssetInfo<T> info) {
            _info = info;
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