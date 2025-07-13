using GameFramework.Core;
using SampleGame.Domain.ModelViewer;

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
        /// <summary>制御用ポート</summary>
        internal IFieldActorPort Port { get; private set; }
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FieldActorModel(int id) : base(id) {
        }

        /// <summary>
        /// セットアップ
        /// </summary>
        internal void Setup(IFieldActorPort port) {
            Port = port;
        }
    }
}