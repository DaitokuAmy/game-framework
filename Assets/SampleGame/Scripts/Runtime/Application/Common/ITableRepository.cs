using System.Threading;
using Cysharp.Threading.Tasks;

namespace SampleGame.Application {
    /// <summary>
    /// TableDataを管理するRepositoryのインタフェース
    /// </summary>
    public interface ITableRepository {
        /// <summary>
        /// 共通テーブルの読み込み
        /// </summary>
        UniTask LoadCommonTableAsync(CancellationToken ct);
    }
}