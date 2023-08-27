using GameFramework.CutsceneSystems;
using UnityEngine;

namespace SampleGame {
    /// <summary>
    /// バトルアクション用カットシーン
    /// </summary>
    public class BattleActionCutscene : Cutscene
    {
        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <param name="owner">自キャラのAnimator</param>
        public void Setup(Animator owner) {
            Bind("Owner", owner);
        }
    }
}