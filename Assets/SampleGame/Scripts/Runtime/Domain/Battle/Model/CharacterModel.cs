using GameFramework;
using GameFramework.Core;

namespace SampleGame.Domain.Battle {
    /// <summary>
    /// 読み取り専用インターフェース
    /// </summary>
    public interface IReadOnlyCharacterModel {
        /// <summary>識別子</summary>
        int Id { get; }
        /// <summary>アクター制御用モデル</summary>
        IReadOnlyCharacterActorModel ActorModel { get; }
        /// <summary>マスター</summary>
        ICharacterMaster Master { get; }
    }

    /// <summary>
    /// キャラモデル
    /// </summary>
    public abstract class CharacterModel : AutoIdModel<CharacterModel>, IReadOnlyCharacterModel {
        /// <inheritdoc/>
        public abstract ICharacterMaster Master { get; }
        
        /// <inheritdoc/>
        public IReadOnlyCharacterActorModel ActorModel => ActorModelInternal;
        
        /// <summary>アクター制御用モデル</summary>
        internal CharacterActorModel ActorModelInternal { get; private set; }
        
        /// <summary>
        /// 生成処理
        /// </summary>
        protected override void OnCreatedInternal(IScope scope) {
            base.OnCreatedInternal(scope);

            ActorModelInternal = new CharacterActorModel(Id).RegisterTo(scope);
        }
    }
}
