using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.Core;

namespace SampleGame.Domain.Battle {
    /// <summary>
    /// キャラアクター生成用インターフェース
    /// </summary>
    public interface ICharacterActorFactory {
        /// <summary>
        /// プレイヤー用のキャラアクターの生成
        /// </summary>
        UniTask<ICharacterActorPort> CreatePlayerAsync(IReadOnlyPlayerModel model, IReadOnlyLayeredTime parentLayeredTime, CancellationToken ct);
        
        /// <summary>
        /// プレイヤー用のキャラアクターの削除
        /// </summary>
        void DestroyPlayer(IReadOnlyPlayerModel model);
    }
}
