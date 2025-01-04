using System.Threading;
using Cysharp.Threading.Tasks;

namespace SampleGame.Domain.ModelViewer {
    /// <summary>
    /// ModelViewer用のリポジトリのインタフェース
    /// </summary>
    public interface IModelViewerRepository {
        /// <summary>
        /// ActorMasterの読み込み
        /// </summary>
        UniTask<IActorMaster> LoadActorMasterAsync(string assetKey, CancellationToken ct);

        /// <summary>
        /// EnvironmentMasterの読み込み
        /// </summary>
        UniTask<IEnvironmentMaster> LoadEnvironmentMasterAsync(string assetKey, CancellationToken ct);
    }
}