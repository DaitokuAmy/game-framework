using GameFramework.ActorSystems;
using SampleGame.Domain.Battle;
using ThirdPersonEngine;
using UnityEngine;

namespace SampleGame.Presentation.Battle {
    /// <summary>
    /// PreviewActor制御用のAdapter
    /// </summary>
    public class CharacterActorAdapter : ActorEntityLogic, ICharacterActorPort {
        private readonly BattleCharacterActor _actor;
        private readonly IReadOnlyCharacterModel _model;
        
        /// <inheritdoc/>
        Vector3 ICharacterActorPort.Position => _actor.Position;
        /// <inheritdoc/>
        Quaternion ICharacterActorPort.Rotation => _actor.Rotation;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CharacterActorAdapter(BattleCharacterActor actor, IReadOnlyCharacterModel model) {
            _actor = actor;
            _model = model;
        }

        /// <inheritdoc/>
        void ICharacterActorPort.InputMove(Vector2 input) {
            _actor.InputMove(input);
        }

        /// <inheritdoc/>
        void ICharacterActorPort.InputJump() {
            _actor.InputJump();
        }
    }
}