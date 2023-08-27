using System;
using GameFramework.ActorSystems;
using GameFramework.Core;
using GameFramework.ModelSystems;
using UniRx;
using UnityEngine;

namespace SampleGame.Battle {
    /// <summary>
    /// バトル用プレイヤーモデル
    /// </summary>
    public class BattlePlayerModel : AutoIdModel<BattlePlayerModel> {
        private Subject<(BattlePlayerModel, int)> _onUpdatedHealthSubject = new();
        private Subject<(BattlePlayerModel, int)> _onDamagedSubject = new();
        private Subject<BattlePlayerModel> _onUpdatedSubject = new();
        private Subject<BattlePlayerModel> _onDeadSubject = new();
        private Subject<(BattlePlayerModel, string)> _onAttackSubject = new();

        public string Name { get; private set; } = "";
        public string AssetKey { get; private set; } = "";
        public int Health { get; private set; } = 0;
        public int HealthMax { get; private set; } = 0;
        public bool IsDead => Health <= 0;

        /// <summary>Actor用Model</summary>
        public BattlePlayerActorModel ActorModel { get; private set; }

        /// <summary>体力変化通知</summary>
        public IObservable<(BattlePlayerModel, int)> OnUpdatedHealthSubject => _onUpdatedHealthSubject;
        /// <summary>ダメージ発生通知</summary>
        public IObservable<(BattlePlayerModel, int)> OnDamagedSubject => _onDamagedSubject;
        /// <summary>パラメータ変化通知</summary>
        public IObservable<BattlePlayerModel> OnUpdatedSubject => _onUpdatedSubject;
        /// <summary>死亡通知</summary>
        public IObservable<BattlePlayerModel> OnDeadSubject => _onDeadSubject;
        /// <summary>攻撃発生通知</summary>
        public IObservable<(BattlePlayerModel, string)> OnAttackSubject => _onAttackSubject;

        /// <summary>
        /// 値の更新
        /// </summary>
        public void Update(string name, string assetKey, int healthMax) {
            Name = name;
            AssetKey = assetKey;
            HealthMax = healthMax;
            Health = HealthMax;

            _onUpdatedHealthSubject.OnNext((this, Health));
            _onUpdatedSubject.OnNext(this);
        }

        /// <summary>
        /// ダメージの追加
        /// </summary>
        public void AddDamage(int damage) {
            if (IsDead) {
                return;
            }

            var newHealth = Mathf.Clamp(Health - damage, 0, HealthMax);
            damage = Health - newHealth;
            Health = newHealth;
            _onUpdatedHealthSubject.OnNext((this, Health));
            _onDamagedSubject.OnNext((this, damage));

            if (IsDead) {
                _onDeadSubject.OnNext(this);
            }
        }

        /// <summary>
        /// アクション実行
        /// </summary>
        public void PlayAction(string key) {
            if (IsDead) {
                return;
            }
            
            _onAttackSubject.OnNext((this, key));
        }

        /// <summary>
        /// 生成時処理
        /// </summary>
        /// <param name="scope"></param>
        protected override void OnCreatedInternal(IScope scope) {
            ActorModel = BattlePlayerActorModel.Create()
                .ScopeTo(scope);
        }

        /// <summary>
        /// 削除時処理
        /// </summary>
        protected override void OnDeletedInternal() {
            void SafeDispose<T>(Subject<T> subject) {
                subject.OnCompleted();
                subject.Dispose();
            }
            
            SafeDispose(_onUpdatedHealthSubject);
            SafeDispose(_onDamagedSubject);
            SafeDispose(_onUpdatedSubject);
            SafeDispose(_onDeadSubject);
            SafeDispose(_onAttackSubject);
        }
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        private BattlePlayerModel(int id) : base(id) {
        }
    }
}