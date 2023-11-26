using GameFramework.ModelSystems;
using UnityEngine;

namespace SampleGame.Domain.Battle {
    /// <summary>
    /// BattleCharacterActorModelの読み取り専用インターフェース
    /// </summary>
    public interface IReadOnlyBattleCharacterActorModel {
        BattleCharacterActorSetupData SetupData { get; }
        /// <summary>現在位置</summary>
        Vector3 Position { get; }
        /// <summary>現在向き</summary>
        Quaternion Rotation { get; }
    }
    
    /// <summary>
    /// バトル用プレイヤーのアクター情報をまとめたモデル
    /// </summary>
    public class BattleCharacterActorModel : AutoIdModel<BattleCharacterActorModel>, IReadOnlyBattleCharacterActorModel {
        /// <summary>Actor初期化用データ</summary>
        public BattleCharacterActorSetupData SetupData { get; private set; }
        /// <summary>現在座標</summary>
        public Vector3 Position { get; private set; }
        /// <summary>現在向き</summary>
        public Quaternion Rotation { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private BattleCharacterActorModel(int id) : base(id) {}

        /// <summary>
        /// 値の更新
        /// </summary>
        public void Setup(BattleCharacterActorSetupData setupData) {
            SetupData = setupData;
        }

        /// <summary>
        /// 座標の設定
        /// </summary>
        public void SetPosition(Vector3 position) {
            Position = position;
        }

        /// <summary>
        /// 向きの設定
        /// </summary>
        public void SetRotation(Quaternion rotation) {
            Rotation = rotation;
        }
    }
}