using System.Linq;
using GameFramework.ActorSystems;
using SampleGame.Domain.ModelViewer;
using ThirdPersonEngine.ModelViewer;
using Unity.Mathematics;

namespace SampleGame.Presentation.ModelViewer {
    /// <summary>
    /// 背景制御用のAdapter
    /// </summary>
    public class EnvironmentActorAdapter : ActorEntityLogic, IEnvironmentActorPort {
        private readonly IReadOnlyEnvironmentModel _model;
        private readonly EnvironmentActor _actor;

        /// <inheritdoc/>
        float3 IEnvironmentActorPort.RootPosition => _actor.RootSlot.position;
        /// <inheritdoc/>
        quaternion IEnvironmentActorPort.RootRotation => _actor.RootSlot.rotation;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EnvironmentActorAdapter(IReadOnlyEnvironmentModel model, EnvironmentActor actor) {
            _model = model;
            _actor = actor;
        }
    }
}