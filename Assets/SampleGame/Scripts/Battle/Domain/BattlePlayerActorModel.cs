using System;
using GameFramework.ModelSystems;
using UniRx;

namespace SampleGame.Battle {
    /// <summary>
    /// バトル用プレイヤーのアクター情報をまとめたモデル
    /// </summary>
    public class BattlePlayerActorModel : AutoIdModel<BattlePlayerActorModel> {
        private Subject<BattlePlayerActorModel> _onUpdatedSubject = new();
        
        /// <summary>Actor初期化用データ</summary>
        public BattleCharacterActorSetupData SetupData { get; private set; }

        /// <summary>値変化通知</summary>
        public IObservable<BattlePlayerActorModel> OnUpdatedSubject => _onUpdatedSubject;

        /// <summary>
        /// 値の更新
        /// </summary>
        public void Update(BattleCharacterActorSetupData setupData) {
            SetupData = setupData;
            _onUpdatedSubject.OnNext(this);
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void OnDeletedInternal() {
            void SafeDispose<T>(Subject<T> subject) {
                subject.OnCompleted();
                subject.Dispose();
            }
            
            SafeDispose(_onUpdatedSubject);
        }

        private BattlePlayerActorModel(int id) : base(id) {}
    }
}