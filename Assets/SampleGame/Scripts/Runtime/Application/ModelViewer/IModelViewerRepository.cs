using System.Threading;
using Cysharp.Threading.Tasks;
using SampleGame.Domain.ModelViewer;

namespace SampleGame.Application.ModelViewer {
    /// <summary>
    /// ModelViewer用のリポジトリのインタフェース
    /// </summary>
    public interface IModelViewerRepository {
        /// <summary>
        /// ViewerMasterの読み込み
        /// </summary>
        UniTask<IModelViewerMaster> LoadModelViewerMasterAsync(CancellationToken ct);

        /// <summary>
        /// ActorMasterの読み込み
        /// </summary>
        UniTask<IPreviewActorMaster> LoadActorMasterAsync(string setupDataId, CancellationToken ct);
    }
}