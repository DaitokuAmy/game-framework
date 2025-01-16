using System.Threading;
using Cysharp.Threading.Tasks;

namespace SampleGame.Domain.ModelViewer {
    /// <summary>
    /// プレビュー用のActor生成インターフェース
    /// </summary>
    public interface IActorFactory {
        /// <summary>
        /// アクターの生成
        /// </summary>
        UniTask<IActorController> CreateAsync(IReadOnlyActorModel model, CancellationToken ct);
        
        /// <summary>
        /// アクターの削除
        /// </summary>
        void Destroy(int id);
    }
}