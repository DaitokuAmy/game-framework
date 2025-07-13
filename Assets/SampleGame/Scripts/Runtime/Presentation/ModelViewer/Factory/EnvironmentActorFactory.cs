using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.ActorSystems;
using GameFramework.Core;
using SampleGame.Infrastructure;
using SampleGame.Presentation.ModelViewer;
using ThirdPersonEngine;

namespace SampleGame.Domain.ModelViewer {
    /// <summary>
    /// 環境生成クラス
    /// </summary>
    public class EnvironmentActorFactory : IEnvironmentActorFactory {
        private readonly EnvironmentSceneRepository _environmentSceneRepository;
        private readonly ActorEntityManager _actorEntityManager;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EnvironmentActorFactory() {
            _environmentSceneRepository = Services.Resolve<EnvironmentSceneRepository>();
            _actorEntityManager = Services.Resolve<ActorEntityManager>();
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