using GameFramework.Core;

namespace SampleGame.Domain.Battle {
    /// <summary>
    /// 読み取り専用インターフェース
    /// </summary>
    public interface IReadOnlyFieldActorModel {
        /// <summary>識別子</summary>
        public int Id { get; }
    }

    /// <summary>
    /// フィールドアクターモデル
    /// </summary>
    public class FieldActorModel : IdLocalModel<int, FieldActorModel>, IReadOnlyFieldActorModel {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FieldActorModel(int id) : base(id) {
        }
    }
}