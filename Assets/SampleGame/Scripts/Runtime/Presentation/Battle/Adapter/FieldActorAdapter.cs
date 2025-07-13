using GameFramework.ActorSystems;
using SampleGame.Domain.Battle;
using UnityEngine.SceneManagement;

namespace SampleGame.Presentation.Battle {
    /// <summary>
    /// 背景制御用のAdapter
    /// </summary>
    public class FieldActorAdapter : ActorEntityLogic, IFieldActorPort {
        private readonly Scene _scene;
        private readonly IReadOnlyFieldModel _model;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FieldActorAdapter(Scene scene, IReadOnlyFieldModel model) {
            _scene = scene;
            _model = model;
        }
    }
}