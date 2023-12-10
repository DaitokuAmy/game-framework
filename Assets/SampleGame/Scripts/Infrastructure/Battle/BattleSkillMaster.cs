using System;
using SampleGame.Domain.Battle;
using UnityEngine;

namespace SampleGame.Infrastructure.Battle {
    /// <summary>
    /// バトル用のスキルマスター
    /// </summary>
    [Serializable]
    public class BattleSkillMaster : IBattleSkillMaster {
        [Tooltip("スキル名")]
        public string skillName = "";
        [Tooltip("再生するアクションキー")]
        public string actionKey = "";

        /// <summary>スキル名称</summary>
        string IBattleSkillMaster.Name => skillName;
        /// <summary>アクションキー</summary>
        string IBattleSkillMaster.ActionKey => actionKey;
    }
}