using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.ActorSystems;
using GameFramework.Core;
using SampleGame.Domain.Battle;
using SampleGame.Infrastructure;
using SampleGame.Infrastructure.Battle;
using ThirdPersonEngine;
using UnityEngine;

namespace SampleGame.Presentation.Battle {
    /// <summary>
    /// CharacterActor生成クラス
    /// </summary>
    public class CharacterActorFactory : ICharacterActorFactory, IServiceUser {
        /// <summary>
        /// Body生成用のBuilder
        /// </summary>
        private class BodyBuilder : IBodyBuilder {
            public void Build(Body body, GameObject gameObject) {
            }
        }

        private IServiceResolver _serviceResolver;
        private BodyPrefabRepository _bodyPrefabRepository;
        private BattleCharacterAssetRepository _characterAssetRepository;
        private ActorEntityManager _actorEntityManager;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CharacterActorFactory() {
        }

        /// <inheritdoc/>
        void IServiceUser.ImportService(IServiceResolver resolver) {
            _serviceResolver = resolver;
            _bodyPrefabRepository = resolver.Resolve<BodyPrefabRepository>();
            _characterAssetRepository = resolver.Resolve<BattleCharacterAssetRepository>();
            _actorEntityManager = resolver.Resolve<ActorEntityManager>();
        }

        /// <inheritdoc/>
        async UniTask<ICharacterActorPort> ICharacterActorFactory.CreatePlayerAsync(IReadOnlyPlayerModel model, IReadOnlyLayeredTime parentLayeredTime, CancellationToken ct) {
            if (model == null) {
                return null;
            }

            // body生成
            var prefab = await _bodyPrefabRepository.LoadCharacterPrefabAsync(model.Master.AssetKey, ct);
            var body = new Body(Object.Instantiate(prefab, _actorEntityManager.RootTransform), new BodyBuilder());
            body.LayeredTime.SetParent(parentLayeredTime);
            body.RegisterTask(TaskOrder.Body);

            // actor生成
            var actorData = await _characterAssetRepository.LoadActorDataAsync(model.Master.ActorAssetKey, ct);
            var actor = new BattleCharacterActor(body, actorData);
            actor.RegisterTask(TaskOrder.Actor);

            // adapter生成
            var adapter = new CharacterActorAdapter(actor, model);
            adapter.RegisterTask(TaskOrder.Logic);
            
            // controller生成
            var controller = new PlayerInputController(model);
            _serviceResolver.Import(controller);
            controller.RegisterTask(TaskOrder.Input);

            // entity構築
            var entity = _actorEntityManager.CreateEntity(model.Id);
            entity.SetBody(body);
            entity.AddActor(actor);
            entity.AddLogic(adapter);
            entity.AddLogic(controller);

            return adapter;
        }

        /// <inheritdoc/>
        void ICharacterActorFactory.DestroyPlayer(IReadOnlyPlayerModel model) {
            _actorEntityManager.DestroyEntity(model.Id);
        }
    }
}