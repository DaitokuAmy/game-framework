using System;
using System.Collections.Generic;
using System.Linq;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.ActorSystems {
    /// <summary>
    /// Body用のCollider制御クラス
    /// </summary>
    [AddComponentMenu("")]
    public class ColliderComponent : SerializedBodyComponent {
        // キャッシュ用のMaterial情報リスト
        private readonly Dictionary<string, List<Collider>> _colliderInfos = new();

        /// <summary>Collider情報のリフレッシュ</summary>
        public event Action RefreshedEvent;

        /// <summary>TriggerEnterイベント通知</summary>
        public event Action<Collider> TriggerEnterEvent;
        /// <summary>TriggerStayイベント通知</summary>
        public event Action<Collider> TriggerStayEvent;
        /// <summary>TriggerExitイベント通知</summary>
        public event Action<Collider> TriggerExitEvent;

        /// <summary>CollisionEnterイベント通知</summary>
        public event Action<Collision> CollisionEnterEvent;
        /// <summary>CollisionStayイベント通知</summary>
        public event Action<Collision> CollisionStayEvent;
        /// <summary>CollisionExitイベント通知</summary>
        public event Action<Collision> CollisionExitEvent;
        
        /// <summary>ControllerColliderHitイベント通知</summary>
        public event Action<ControllerColliderHit> ControllerColliderHitEvent;

        /// <summary>
        /// 制御キーの一覧を取得
        /// </summary>
        public string[] GetKeys() {
            return _colliderInfos.Keys.ToArray();
        }

        /// <summary>
        /// コライダー情報の取得
        /// </summary>
        /// <param name="key">制御用キー</param>
        public IReadOnlyList<Collider> GetColliders(string key) {
            if (_colliderInfos.TryGetValue(key, out var result)) {
                return result;
            }

            return Array.Empty<Collider>();
        }

        /// <summary>
        /// レイヤーの設定
        /// </summary>
        /// <param name="key">制御キー</param>
        /// <param name="layer">設定するレイヤー</param>
        public void SetLayer(string key, int layer) {
            var colliders = GetColliders(key);
            foreach (var collider in colliders) {
                collider.gameObject.layer = layer;
            }
        }

        /// <summary>
        /// レイヤーの設定（管理対象のコライダー全て）
        /// </summary>
        /// <param name="layer">設定するレイヤー</param>
        public void SetLayer(int layer) {
            foreach (var list in _colliderInfos.Values) {
                foreach (var collider in list) {
                    collider.gameObject.layer = layer;
                }
            }
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal(IScope scope) {
            var meshController = Body.GetBodyComponent<MeshComponent>();
            meshController.RefreshedEvent += () => {
                // Collider情報の回収
                CreateColliderInfos();
            };
            CreateColliderInfos();
        }

        /// <summary>
        /// コライダー情報の生成
        /// </summary>
        private void CreateColliderInfos() {
            _colliderInfos.Clear();

            var partsList = Body.GetComponentsInChildren<ColliderParts>(true);
            for (var i = 0; i < partsList.Length; i++) {
                var parts = partsList[i];
                for (var j = 0; j < parts.infos.Length; j++) {
                    var info = parts.infos[j];

                    if (!_colliderInfos.TryGetValue(info.key, out var list)) {
                        list = new List<Collider>(info.colliders.Where(x => x != null));
                        _colliderInfos.Add(info.key, list);
                    }
                    else {
                        list.AddRange(info.colliders.Where(x => x != null));
                    }
                }
            }

            RefreshedEvent?.Invoke();
        }

        /// <summary>
        /// Trigger判定時通知
        /// </summary>
        private void OnTriggerEnter(Collider other) {
            TriggerEnterEvent?.Invoke(other);
        }

        private void OnTriggerStay(Collider other) {
            TriggerStayEvent?.Invoke(other);
        }

        private void OnTriggerExit(Collider other) {
            TriggerExitEvent?.Invoke(other);
        }

        /// <summary>
        /// Collision判定時通知
        /// </summary>
        private void OnCollisionEnter(Collision collision) {
            CollisionEnterEvent?.Invoke(collision);
        }

        private void OnCollisionStay(Collision collision) {
            CollisionStayEvent?.Invoke(collision);
        }

        private void OnCollisionExit(Collision collision) {
            CollisionExitEvent?.Invoke(collision);
        }

        private void OnControllerColliderHit(ControllerColliderHit hit) {
            ControllerColliderHitEvent?.Invoke(hit);
        }
    }
}