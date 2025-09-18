using System.Threading;
using Cysharp.Threading.Tasks;

namespace SampleGame.Domain.ModelViewer {
    /// <summary>
    /// TableDataを管理するRepositoryのインタフェース
    /// </summary>
    public interface IModelViewerTableRepository {
        /// <summary>
        /// 関連テーブルの読み込み
        /// </summary>
        UniTask LoadTablesAsync(CancellationToken ct);

        /// <summary>
        /// ModelViewerActorのId一覧を取得
        /// </summary>
        /// <returns></returns>
        int[] GetModelViewerActorIds();

        /// <summary>
        /// ModelViewerEnvironmentのId一覧を取得
        /// </summary>
        /// <returns></returns>
        int[] GetModelViewerEnvironmentIds();

        /// <summary>
        /// PreviewActorMasterの取得
        /// </summary>
        IModelViewerActorMaster FindModelViewerActorById(int id);

        /// <summary>
        /// EnvironmentActorMasterの取得
        /// </summary>
        IModelViewerEnvironmentMaster FindModelViewerEnvironmentById(int id);
    }
}