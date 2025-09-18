using System;
using GameFramework.SituationSystems;

namespace SampleGame.Lifecycle {
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
        
        private readonly Action<T> _setupAction;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SituationSetup(Action<T> setupAction = null) {
            _setupAction = setupAction;
        }

        void ISituationSetup.OnSetup(Situation situation) {
            if (situation is T startSituation) {
                _setupAction?.Invoke(startSituation);
            }
        }
    }
}