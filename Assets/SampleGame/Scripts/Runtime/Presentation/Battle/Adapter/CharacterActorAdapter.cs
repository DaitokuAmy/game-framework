using ActionSequencer;
using GameFramework.ActorSystems;
using GameFramework.Core;
using SampleGame.Domain.Battle;
using ThirdPersonEngine;
using Unity.Mathematics;
using UnityEngine;

namespace SampleGame.Presentation.Battle {
    /// <summary>
    /// PreviewActor制御用のAdapter
    /// </summary>
    public class CharacterActorAdapter : ActorEntityLogic, ICharacterActorPort {
        private readonly BattleCharacterActor _actor;
        private readonly IReadOnlyCharacterModel _model;

        /// <inheritdoc/>
        float3 ICharacterActorPort.Position => _actor.Position;
        /// <inheritdoc/>
        quaternion ICharacterActorPort.Rotation => _actor.Rotation;
        /// <inheritdoc/>
        quaternion ICharacterActorPort.LookAtRotation => _actor.LookAtRotation;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CharacterActorAdapter(BattleCharacterActor actor, IReadOnlyCharacterModel model) {
            _actor = actor;
            _model = model;
        }

        /// <inheritdoc/>
        void ICharacterActorPort.InputMove(float2 input) {
            _actor.InputMove(input);
        }

        /// <inheritdoc/>
        void ICharacterActorPort.InputLookAt(float2 input) {
            _actor.InputLookAt(input);
        }

        /// <inheritdoc/>
        void ICharacterActorPort.InputSprint(bool sprint) {
            _actor.InputSprint(sprint);
        }

        /// <inheritdoc/>
        void ICharacterActorPort.InputJump() {
            _actor.InputJump();
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(IScope scope) {
            base.ActivateInternal(scope);

            SequenceControllerProvider.SetController(_actor.Body.GameObject, (SequenceController)_actor.SequenceController);
        }

        /// <summary>
        /// 非アクティブ時処理
        /// </summary>
        protected override void DeactivateInternal() {
            SequenceControllerProvider.SetController(_actor.Body.GameObject, null);
            
            base.DeactivateInternal();
        }
    }
}