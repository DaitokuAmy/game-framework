using System;
using GameFramework.SituationSystems;

namespace SampleGame {
    /// <summary>
    /// Situationの初期化処理のインタフェース
    /// </summary>
    public interface ISituationSetup {
        /// <summary>
        /// Situationのタイプ
        /// </summary>
        Type SituationType { get; }

        /// <summary>
        /// Situationのセットアップ処理
        /// </summary>
        void OnSetup(Situation situation);
    }

    /// <summary>
    /// Situationの初期化処理
    /// </summary>
    public class SituationSetup<T> : ISituationSetup where T : Situation {
        Type ISituationSetup.SituationType => typeof(T);
        
        /// <summary>初期化時通知イベント</summary>
        public event Action<T> OnSetupEvent;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SituationSetup(Action<T> onSetup = null) {
            OnSetupEvent = onSetup;
        }

        void ISituationSetup.OnSetup(Situation situation) {
            if (situation is T startSituation) {
                OnSetupEvent?.Invoke(startSituation);
            }
        }
    }
}