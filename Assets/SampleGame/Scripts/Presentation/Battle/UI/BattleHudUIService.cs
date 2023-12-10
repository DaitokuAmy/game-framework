using GameFramework.UISystems;
using UnityEngine;
using UnityEngine.UI;

namespace SampleGame.Battle {
    /// <summary>
    /// BattleHud用のUIService
    /// </summary>
    public class BattleHudUIService : UIService {
        [SerializeField, Tooltip("テスト用ボタン")]
        private Button _button;

        /// <summary>テスト用ボタン</summary>
        public Button Button => _button;
    }
}