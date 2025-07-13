using GameFramework.Core;

namespace SampleGame.Domain.Battle {
    /// <summary>
    /// 読み取り専用インターフェース
    /// </summary>
    public interface IReadOnlyCharacterActorModel {
        /// <summary>識別子</summary>
        public int Id { get; }
    }

    /// <summary>
    /// アクターモデル
    /// </summary>
    public class CharacterActorModel : IdLocalModel<int, CharacterActorModel>, IReadOnlyCharacterActorModel {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CharacterActorModel(int id) : base(id) {
        }
    }
}