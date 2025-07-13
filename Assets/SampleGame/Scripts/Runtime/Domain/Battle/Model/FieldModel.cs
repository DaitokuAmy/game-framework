using GameFramework;
using GameFramework.Core;

namespace SampleGame.Domain.Battle {
    /// <summary>
    /// 読み取り専用インターフェース
    /// </summary>
    public interface IReadOnlyFieldModel {
        /// <summary>識別子</summary>
        int Id { get; }
        /// <summary>アクター制御用モデル</summary>
        IReadOnlyFieldActorModel ActorModel { get; }
        /// <summary>マスター</summary>
        IFieldMaster Master { get; }
    }

    /// <summary>
    /// フィールドモデル
    /// </summary>
    public sealed class FieldModel: AutoIdModel<FieldModel>, IReadOnlyFieldModel {
        /// <inheritdoc/>
        public IFieldMaster Master { get; private set; }
        
        /// <inheritdoc/>
        public IReadOnlyFieldActorModel ActorModel => ActorModelInternal;
        
        /// <summary>アクター制御用モデル</summary>
        internal FieldActorModel ActorModelInternal { get; private set; }
        
        /// <summary>
        /// 生成処理
        /// </summary>
        protected override void OnCreatedInternal(IScope scope) {
            base.OnCreatedInternal(scope);

            ActorModelInternal = new FieldActorModel(Id).RegisterTo(scope);
        }

        /// <summary>
        /// セットアップ
        /// </summary>
        public void Setup(IFieldMaster master) {
            Master = master;
        }
    }
}
