using System.Threading;
using Cysharp.Threading.Tasks;

namespace SampleGame.Domain.ModelViewer {
    /// <summary>
    /// 背景生成インターフェース
    /// </summary>
    public interface IEnvironmentActorFactory {
        /// <summary>
        /// 背景生成
        /// </summary>
        UniTask<IEnvironmentActorPort> CreateAsync(IReadOnlyEnvironmentModel model, CancellationToken ct);
        
        /// <summary>
        /// 背景削除
        /// </summary>
        void Destroy(IReadOnlyEnvironmentModel model);
    }
}