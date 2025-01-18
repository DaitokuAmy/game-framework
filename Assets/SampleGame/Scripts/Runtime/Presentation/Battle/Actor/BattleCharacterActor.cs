using GameFramework.BodySystems;

namespace SampleGame.Presentation.Game {
    /// <summary>
    /// アクター基底
    /// </summary>
    public class BattleCharacterActor : CharacterActor {
        private BattleCharacterActorSetupData _setupData;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BattleCharacterActor(Body body, BattleCharacterActorSetupData setupData)
            : base(body, setupData) {
            _setupData = setupData;
        }
    }
}