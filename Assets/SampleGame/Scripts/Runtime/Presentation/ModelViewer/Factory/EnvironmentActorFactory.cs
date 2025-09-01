using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.ActorSystems;
using GameFramework.Core;
using SampleGame.Domain.ModelViewer;
using SampleGame.Infrastructure;
using ThirdPersonEngine;

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
            var scene = await _environmentSceneRepository.LoadFieldSceneAsync(model.ActorMaster.AssetKey, ct);
            
            // adapter
            var adapter = new EnvironmentActorAdapter(model, scene);
            adapter.RegisterTask(TaskOrder.Logic);
            
            // entity
            var entity = _actorEntityManager.CreateEntity(model.Id);
            entity.SetScene(scene);
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
            
            _environmentSceneRepository.UnloadFieldScene(model.ActorMaster.AssetKey);
            _actorEntityManager.DestroyEntity(model.Id);
        }
    }
}