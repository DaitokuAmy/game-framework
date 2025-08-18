using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework;
using GameFramework.ActorSystems;
using GameFramework.Core;
using SampleGame.Domain.Battle;
using SampleGame.Infrastructure;
using ThirdPersonEngine;
using UnityEngine;

namespace SampleGame.Presentation.Battle {
    /// <summary>
    /// フィールド生成クラス
    /// </summary>
    public class FieldActorFactory : IFieldActorFactory {
        /// <summary>
        /// Body生成用のBuilder
        /// </summary>
        private class BodyBuilder : IBodyBuilder {
            public void Build(Body body, GameObject gameObject) {
            }
        }
        
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
        async UniTask<IFieldActorPort> IFieldActorFactory.CreateAsync(IReadOnlyFieldModel model, IReadOnlyLayeredTime parentLayeredTime, CancellationToken ct) {
            // scene
            var scene = await _environmentSceneRepository.LoadFieldSceneAsync(model.Master.AssetKey, ct);
            
            // body
            var body = default(Body);
            var bodyRoot = scene.GetRootGameObjects().FirstOrDefault(x => x.CompareTag("BodyRoot"));
            if (bodyRoot != null) {
                body = new Body(bodyRoot, new BodyBuilder());
                body.LayeredTime.SetParent(parentLayeredTime);
                body.RegisterTask(TaskOrder.Body);
            }

            // adapter
            var adapter = new FieldActorAdapter(scene, model);
            adapter.RegisterTask(TaskOrder.Logic);

            // entity
            var entity = _actorEntityManager.CreateEntity(model.Id);
            entity.SetScene(scene);
            entity.SetBody(body);
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