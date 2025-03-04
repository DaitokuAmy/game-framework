using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.BodySystems;
using GameFramework.Core;
using GameFramework.ActorSystems;
using SampleGame.Infrastructure;
using SampleGame.Infrastructure.Battle;
using R3;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SampleGame.Presentation.Battle {
    /// <summary>
    /// Actor管理用クラス
    /// </summary>
    public class ActorEntityManager : IDisposable {
        private readonly DisposableScope _scope;
        private readonly Dictionary<int, ActorEntity> _createdEntities;
        
        private readonly BodyManager _bodyManager;
        private readonly BodyPrefabRepository _bodyBodyPrefabRepository;
        private readonly BattleCharacterAssetRepository _battleCharacterAssetRepository;
        
        private bool _isDisposed;

        /// <summary>配置ルート</summary>
        public Transform RootTransform { get; private set; }
        /// <summary>ActorEntityリスト</summary>
        public IReadOnlyDictionary<int, ActorEntity> Entities => _createdEntities;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ActorEntityManager(BodyManager bodyManager) {
            RootTransform = new GameObject(nameof(ActorEntityManager)).transform;
            
            _isDisposed = false;
            _scope = new();
            _createdEntities = new();

            _bodyManager = bodyManager;
            _bodyBodyPrefabRepository = Services.Get<BodyPrefabRepository>();
            _battleCharacterAssetRepository = Services.Get<BattleCharacterAssetRepository>();
        }

        /// <summary>
        /// 破棄処理
        /// </summary>
        public void Dispose() {
            if (_isDisposed) {
                return;
            }

            _isDisposed = true;

            _scope.Dispose();
            _createdEntities.Clear();

            if (RootTransform != null) {
                Object.Destroy(RootTransform.gameObject);
                RootTransform = null;
            }
        }

        /// <summary>
        /// バトルキャラアクター用のアクターエンティティ生成
        /// </summary>
        public UniTask<ActorEntity> CreateBattleCharacterActorEntityAsync(int id, string assetKey, string actorAssetKey, LayeredTime parentLayeredTime, CancellationToken ct) {
            return CreateBattleCharacterActorEntityAsyncInternal(id, assetKey, actorAssetKey, parentLayeredTime, ct);
        }

        /// <summary>
        /// エンティティを削除
        /// </summary>
        public void DestroyEntity(int id) {
            // 生成済みなら削除
            if (_createdEntities.TryGetValue(id, out var entity)) {
                entity.Dispose();
                _createdEntities.Remove(id);
            }
        }

        /// <summary>
        /// Prefabからゲームオブジェクトを生成する
        /// </summary>
        private GameObject InstantiatePrefab(GameObject prefab) {
            var obj = Object.Instantiate(prefab, Vector3.zero, Quaternion.identity, RootTransform);
            obj.transform.localScale = Vector3.one;
            return obj;
        }

        /// <summary>
        /// アクターエンティティの生成
        /// </summary>
        private async UniTask<ActorEntity> CreateBattleCharacterActorEntityAsyncInternal(int id, string assetKey, string actorAssetKey, LayeredTime parentLayeredTime, CancellationToken ct) {
            var key = id;

            // 生成が完了いているのであればそれを返す
            if (_createdEntities.TryGetValue(key, out var entity)) {
                return entity;
            }
            
            // todo:生成中のエラーハンドリング

            entity = new ActorEntity().ScopeTo(_scope);

            // Body生成
            var prefab = await LoadBodyPrefabAsync(assetKey, ct);
            var body = _bodyManager.CreateFromGameObject(InstantiatePrefab(prefab));
            RenameBodyGameObject(id, body);
            
            // 座標の初期化
            body.LocalPosition = Vector3.zero;
            body.LocalRotation = Quaternion.identity;

            // Actorの生成
            var setupData = await _battleCharacterAssetRepository.LoadSetupDataAsync(actorAssetKey, ct);
            var actor = new BattleCharacterActor(body, setupData);

            // 初期化
            body.UserId = key;
            body.LayeredTime.SetParent(parentLayeredTime);
            actor.RegisterTask(TaskOrder.Actor);

            entity.SetBody(body);
            entity.AddActor(actor);

            _createdEntities.Add(key, entity);
            return entity;
        }

        /// <summary>
        /// BodyGameObjectのリネーム
        /// </summary>
        private void RenameBodyGameObject(int id, Body body) {
            body.GameObject.name = $"{id}:{body.GameObject.name.Replace("(Clone)", "")}";
        }

        /// <summary>
        /// BodyPrefabの読み込み
        /// </summary>
        private async UniTask<GameObject> LoadBodyPrefabAsync(string assetKey, CancellationToken ct) {
            if (assetKey.StartsWith("ch")) {
                return await _bodyBodyPrefabRepository.LoadCharacterPrefabAsync(assetKey, ct);
            }

            return null;
        }
    }
}