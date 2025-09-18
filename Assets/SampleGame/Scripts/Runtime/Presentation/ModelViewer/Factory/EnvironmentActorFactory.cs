using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.ActorSystems;
using GameFramework.Core;
using SampleGame.Domain.ModelViewer;
using SampleGame.Infrastructure;
using ThirdPersonEngine;
using ThirdPersonEngine.ModelViewer;

namespace SampleGame.Presentation.ModelViewer {
    /// <summary>
    /// 環境生成クラス
    /// </summary>
    public class EnvironmentActorFactory : IEnvironmentActorFactory, IServiceUser {
        private EnvironmentSceneRepository _environmentSceneRepository;
        private ActorEntityManager _actorEntityManager;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EnvironmentActorFactory() {
        }

        /// <inheritdoc/>
        void IServiceUser.ImportService(IServiceResolver resolver) {
            _environmentSceneRepository = resolver.Resolve<EnvironmentSceneRepository>();
            _actorEntityManager = resolver.Resolve<ActorEntityManager>();
        }

        /// <summary>
        /// 背景生成
        /// </summary>
        async UniTask<IEnvironmentActorPort> IEnvironmentActorFactory.CreateAsync(IReadOnlyEnvironmentModel model, CancellationToken ct) {
            // scene
            var scene = await _environmentSceneRepository.LoadFieldSceneAsync(model.Master.AssetKey, ct);

            // body
            var rootObjects = scene.GetRootGameObjects();
            var body = default(Body);
            foreach (var obj in rootObjects) {
                if (obj.CompareTag("BodyRoot")) {
                    body = new Body(obj);
                    break;
                }
            }

            if (body == null || rootObjects.Length > 0) {
                body = new Body(rootObjects[0]);
            }

            body.RegisterTask(TaskOrder.Body);

            // actor
            var actor = new EnvironmentActor(body);
            actor.RegisterTask(TaskOrder.Actor);

            // adapter
            var adapter = new EnvironmentActorAdapter(model, actor);
            adapter.RegisterTask(TaskOrder.Logic);

            // entity
            var entity = _actorEntityManager.CreateEntity(model.Id);
            entity.SetScene(scene);
            entity.SetBody(body);
            entity.AddActor(actor);
            entity.AddLogic(adapter);

            return adapter;
        }

        /// <summary>
        /// 背景削除
        /// </summary>
        void IEnvironmentActorFactory.Destroy(IReadOnlyEnvironmentModel model) {
            if (model == null) {
                return;
            }

            _environmentSceneRepository.UnloadFieldScene(model.Master.AssetKey);
            _actorEntityManager.DestroyEntity(model.Id);
        }
    }
}