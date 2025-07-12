using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework;

namespace SampleGame.Domain.ModelViewer {
    /// <summary>
    /// プレビュー用のActor生成インターフェース
    /// </summary>
    public interface IActorFactory {
        /// <summary>
        /// アクターの生成
        /// </summary>
        UniTask<IActorPort> CreateAsync(IReadOnlyActorModel model, LayeredTime layeredTime, CancellationToken ct);
        
        /// <summary>
        /// アクターの削除
        /// </summary>
        void Destroy(int id);
    }
}