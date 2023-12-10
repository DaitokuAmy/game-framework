using GameFramework.Core;
using GameFramework.ModelSystems;
using UniRx;
using UnityEngine;

namespace SampleGame.Domain.Battle {
    /// <summary>
    /// BattleStatusModelの読み取り専用インターフェース
    /// </summary>
    public interface IReadOnlyBattleStatusModel {
        /// <summary>最大体力</summary>
        int HealthMax { get; }
        /// <summary>現在体力</summary>
        IReadOnlyReactiveProperty<int> Health { get; }
        /// <summary>死んでいるか</summary>
        IReadOnlyReactiveProperty<bool> IsDead { get; }
    }
    
    /// <summary>
    /// バトル用ステータスモデル
    /// </summary>
    public class BattleStatusModel : AutoIdModel<BattleStatusModel>, IReadOnlyBattleStatusModel {
        private ReactiveProperty<int> _health;
        private ReactiveProperty<bool> _dead;

        /// <summary>最大体力</summary>
        public int HealthMax { get; private set; }
        /// <summary>現在体力</summary>
        public IReadOnlyReactiveProperty<int> Health => _health;
        /// <summary>死んでいるか</summary>
        public IReadOnlyReactiveProperty<bool> IsDead => _dead;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        private BattleStatusModel(int id) : base(id) {}

        /// <summary>
        /// 生成時処理
        /// </summary>
        protected override void OnCreatedInternal(IScope scope) {
            base.OnCreatedInternal(scope);
            _health = new ReactiveProperty<int>().ScopeTo(scope);
            _dead = new ReactiveProperty<bool>().ScopeTo(scope);
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Setup(int healthMax) {
            HealthMax = healthMax;
            _health.Value = healthMax;
            ApplyDead();
        }

        /// <summary>
        /// ダメージを与える
        /// </summary>
        public void TakeDamage(int damage) {
            _health.Value = Mathf.Clamp(_health.Value - damage, 0, HealthMax);
            ApplyDead();
        }

        /// <summary>
        /// 死亡状態の反映
        /// </summary>
        private void ApplyDead() {
            _dead.Value = _health.Value <= 0;
        }
    }
}