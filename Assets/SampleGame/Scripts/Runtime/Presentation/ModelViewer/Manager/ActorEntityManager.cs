using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.BodySystems;
using GameFramework.Core;
using GameFramework.ActorSystems;
using SampleGame.Application.ModelViewer;
using SampleGame.Domain.ModelViewer;
using R3;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SampleGame.Presentation.ModelViewer {
    /// <summary>
    /// Actor管理用クラス
    /// </summary>
    public class ActorEntityManager : IDisposable {
        private readonly DisposableScope _scope;
        private readonly Dictionary<int, ActorEntity> _createdEntities;
        
        private readonly BodyManager _bodyManager;
        
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
        /// プレビューアクター用のアクターエンティティ生成
        /// </summary>
        public UniTask<ActorEntity> CreatePreviewActorEntityAsync(int id, GameObject prefab, LayeredTime parentLayeredTime, CancellationToken ct) {
            return CreatePreviewActorEntityAsyncInternal(id, prefab, parentLayeredTime, ct);
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
        private async UniTask<ActorEntity> CreatePreviewActorEntityAsyncInternal(int id, GameObject prefab, LayeredTime parentLayeredTime, CancellationToken ct) {
            var key = id;

            // 生成が完了いているのであればそれを返す
            if (_createdEntities.TryGetValue(key, out var entity)) {
                return entity;
            }

            entity = new ActorEntity().RegisterTo(_scope);

            // Body生成
            var body = _bodyManager.CreateFromGameObject(InstantiatePrefab(prefab));

            // Bodyのリネーム
            RenameBodyGameObject(id, body);

            // Actorのセットアップ
            var actor = new PreviewActor(body);

            // 初期化
            body.UserId = key;
            body.LayeredTime.SetParent(parentLayeredTime);
            actor.RegisterTask(TaskOrder.Actor);

            entity.SetBody(body);
            entity.AddActor(actor);

            _createdEntities.Add(key, entity);
            
            await UniTask.CompletedTask;
            return entity;
        }

        /// <summary>
        /// BodyGameObjectのリネーム
        /// </summary>
        private void RenameBodyGameObject(int id, Body body) {
            body.GameObject.name = $"{id}:{body.GameObject.name.Replace("(Clone)", "")}";
        }
    }
}