using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.AssetSystems;
using GameFramework.Core;
using SampleGame.Application;
using Object = UnityEngine.Object;

namespace SampleGame.Infrastructure {
    /// <summary>
    /// テーブルデータ管理クラス
    /// </summary>
    public partial class TableRepository : IDisposable, ITableRepository {
        private readonly AssetManager _assetManager;
        
        private DisposableScope _scope;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TableRepository(AssetManager assetManager) {
            _assetManager = assetManager;
            _scope = new DisposableScope();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            _scope?.Dispose();
            _scope = null;
        }

        /// <summary>
        /// テーブルデータの読み込み
        /// </summary>
        private UniTask<T> LoadTableAsync<T>(string assetKey, CancellationToken ct)
            where T : Object {
            return new TableDataRequest<T>(assetKey).LoadAsync(_assetManager, _scope, cancellationToken:ct);
        }
    }
}