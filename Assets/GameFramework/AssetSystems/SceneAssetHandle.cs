using System;
using GameFramework.Core;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace GameFramework.AssetSystems {
    /// <summary>
    /// シーンアセットリクエスト用ハンドル
    /// </summary>
    public struct SceneAssetHandle : IProcess {
        // 無効なSceneAssetHandle
        public static readonly SceneAssetHandle Empty = new SceneAssetHandle();

        // 読み込み情報
        private ISceneAssetInfo _info;

        // 読み込み完了しているか
        public bool IsDone => _info == null || _info.IsDone;
        // シーンインスタンス
        public SceneInstance SceneInstance => _info?.SceneInstance ?? new SceneInstance();
        // エラー
        public Exception Exception => _info?.Exception ?? null;
        // 有効なハンドルか
        public bool IsValid => _info != null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="info">読み込み管理用情報</param>
        public SceneAssetHandle(ISceneAssetInfo info) {
            _info = info;
        }

        /// <summary>
        /// 読み込んだアセットの解放
        /// </summary>
        internal void Release() {
            if (_info == null) {
                return;
            }

            _info.Dispose();
            _info = null;
        }
    }
}