using GameFramework.ModelSystems;

namespace SampleGame.Domain.Equipment {
    /// <summary>
    /// 装備モデル用の読み取り専用インターフェース
    /// </summary>
    public interface IReadOnlyEquipmentModel {
        /// <summary>名称</summary>
        string Name { get; }
    }
    
    /// <summary>
    /// 装備品のドメインモデル基底
    /// </summary>
    public abstract class EquipmentModel : IdModel<int, EquipmentModel> {
        /// <summary>名称</summary>
        public string Name { get; private set; }
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        protected EquipmentModel(int id) : base(id) {
        }
    }
}