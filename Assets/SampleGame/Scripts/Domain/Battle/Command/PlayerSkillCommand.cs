using System.Collections;
using GameFramework.Core;
using UnityEngine;

namespace SampleGame.Domain.Battle {
    /// <summary>
    /// プレイヤーのスキルを実行するコマンド
    /// </summary>
    public class PlayerSkillCommand : CoroutineCommand {
        private BattlePlayerModel _battlePlayerModel;
        private int _skillIndex;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PlayerSkillCommand(BattlePlayerModel battlePlayerModel, int skillIndex) {
            _battlePlayerModel = battlePlayerModel;
            _skillIndex = skillIndex;
        }

        /// <summary>
        /// 実行ルーチン
        /// </summary>
        protected override IEnumerator ExecuteRoutine(IScope scope) {
            if (_skillIndex < 0 || _skillIndex >= _battlePlayerModel.SkillModels.Count) {
                Debug.LogError($"Invalid skill index. [{_skillIndex}]");
                yield break;
            }

            var skillModel = (BattleSkillModel)_battlePlayerModel.SkillModels[_skillIndex];
            if (!skillModel.CanExecute) {
                Debug.LogError($"Skill can not execute. [{_skillIndex}]");
                yield break;
            }
            
            // スキル実行
            if (!skillModel.Invoke()) {
                Debug.LogError($"Skill can not invoke. [{_skillIndex}]");
                yield break;
            }
            
            // アクションの再生
            var actorModel = (BattleCharacterActorModel)_battlePlayerModel.ActorModel;
            yield return actorModel.PlayAction(skillModel.ActionKey);
        }
    }
}