using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.ActorSystems;
using GameFramework.Core;
using SampleGame.Domain.Battle;
using SampleGame.Infrastructure;
using ThirdPersonEngine;

namespace SampleGame.Presentation.Battle {
    /// <summary>
    /// フィールド生成クラス
    /// </summary>
    public class FieldActorFactory : IFieldActorFactory {
        private readonly EnvironmentSceneRepository _environmentSceneRepository;
        private readonly ActorEntityManager _actorEntityManager;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FieldActorFactory() {
            _environmentSceneRepository = Services.Resolve<EnvironmentSceneRepository>();
            _actorEntityManager = Services.Resolve<ActorEntityManager>();
        }

        /// <summary>
        /// フィールド生成
        /// </summary>
        async UniTask<IFieldActorPort> IFieldActorFactory.CreateAsync(IReadOnlyFieldModel model, CancellationToken ct) {
            // scene
            var scene = await _environmentSceneRepository.LoadFieldSceneAsync(model.Master.AssetKey, ct);

            // adapter
            var adapter = new FieldActorAdapter(scene, model);
            adapter.RegisterTask(TaskOrder.Logic);

            // entity
            var entity = _actorEntityManager.CreateEntity(model.Id);
            entity.SetScene(scene);
            entity.AddLogic(adapter);

            return adapter;
        }

        /// <summary>
        /// フィールド削除
        /// </summary>
        void IFieldActorFactory.Destroy(IReadOnlyFieldModel model) {
            if (model == null) {
                return;
            }

            _environmentSceneRepository.UnloadFieldScene(model.Master.AssetKey);
            _actorEntityManager.DestroyEntity(model.Id);
        }
    }
}