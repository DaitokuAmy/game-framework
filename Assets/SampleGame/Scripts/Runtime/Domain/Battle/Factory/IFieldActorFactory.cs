using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework;
using SampleGame.Domain.ModelViewer;

namespace SampleGame.Domain.Battle {
    /// <summary>
    /// フィールド生成インターフェース
    /// </summary>
    public interface IFieldActorFactory {
        /// <summary>
        /// 背景生成
        /// </summary>
        UniTask<IFieldActorPort> CreateAsync(IReadOnlyFieldModel model, IReadOnlyLayeredTime parentLayeredTime, CancellationToken ct);
        
        /// <summary>
        /// 背景削除
        /// </summary>
        void Destroy(IReadOnlyFieldModel model);
    }
}