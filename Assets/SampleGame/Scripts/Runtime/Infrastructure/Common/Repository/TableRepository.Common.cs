using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace SampleGame.Infrastructure {
    /// <summary>
    /// テーブルデータ管理クラス(共通用)
    /// </summary>
    partial class TableRepository {
        /// <summary>
        /// 共通テーブルの読み込み
        /// </summary>
        public async UniTask LoadCommonTableAsync(CancellationToken ct) {
            ct.ThrowIfCancellationRequested();
            await UniTask.WhenAll(
            );
        }
    }
}