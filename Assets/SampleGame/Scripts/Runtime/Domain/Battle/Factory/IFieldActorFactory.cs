using System.Threading;
using Cysharp.Threading.Tasks;

namespace SampleGame.Domain.Battle {
    /// <summary>
    /// フィールド生成インターフェース
    /// </summary>
    public interface IFieldActorFactory {
        /// <summary>
        /// 背景生成
        /// </summary>
        UniTask<IFieldActorFactory> CreateAsync(IReadOnlyFieldActorModel model, CancellationToken ct);
        
        /// <summary>
        /// 背景削除
        /// </summary>
        void Destroy(IReadOnlyFieldActorModel model);
    }
}