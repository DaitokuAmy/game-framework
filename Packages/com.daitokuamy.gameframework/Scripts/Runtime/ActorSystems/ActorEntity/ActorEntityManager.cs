using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameFramework.ActorSystems {
    /// <summary>
    /// ActorEntity管理用クラス基底
    /// </summary>
    public class ActorEntityManager : IDisposable {
        private readonly Dictionary<int, ActorEntity> _entities;

        private bool _isDisposed;

        /// <summary>Entityリスト</summary>
        public IReadOnlyDictionary<int, ActorEntity> Entities => _entities;

        /// <summary>配置ルート</summary>
        public Transform RootTransform { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ActorEntityManager(string tag = "") {
            RootTransform = new GameObject($"ActorEntityManager{(string.IsNullOrEmpty(tag) ? "" : $"({tag})")}").transform;

            _isDisposed = false;
            _entities = new();
        }

        /// <summary>
        /// 破棄処理
        /// </summary>
        public void Dispose() {
            if (_isDisposed) {
                return;
            }

            _isDisposed = true;

            _entities.Clear();

            if (RootTransform != null) {
                Object.Destroy(RootTransform.gameObject);
                RootTransform = null;
            }
        }

        /// <summary>
        /// エンティティの生成
        /// </summary>
        public ActorEntity CreateEntity(int id, bool active = true) {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(ActorEntityManager));
            }
            
            var key = id;

            // 生成が完了いているのであればそれを返す
            if (_entities.TryGetValue(key, out var entity)) {
                return entity;
            }

            entity = new ActorEntity(id, active);
            _entities.Add(key, entity);
            return entity;
        }

        /// <summary>
        /// エンティティを削除
        /// </summary>
        public void DestroyEntity(int id) {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(ActorEntityManager));
            }
            
            // 生成済みなら削除
            if (_entities.TryGetValue(id, out var entity)) {
                entity.Dispose();
                _entities.Remove(id);
            }
        }

        /// <summary>
        /// Entityの取得
        /// </summary>
        public ActorEntity FindEntity(int id) {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(ActorEntityManager));
            }
            
            return Entities.GetValueOrDefault(id);
        }
    }
}