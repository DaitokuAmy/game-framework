using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameFramework.BodySystems {
    /// <summary>
    /// Body用のCollider制御クラス
    /// </summary>
    [AddComponentMenu("")]
    public class ColliderController : SerializedBodyController {
        // キャッシュ用のMaterial情報リスト
        private Dictionary<string, List<Collider>> _colliderInfos = new Dictionary<string, List<Collider>>();

        // Collider情報のリフレッシュ
        public event Action OnRefreshed;

        // Trigger系イベント通知
        public event Action<Collider> OnTriggerEnterEvent;
        public event Action<Collider> OnTriggerStayEvent;
        public event Action<Collider> OnTriggerExitEvent;

        // Collision系イベント通知
        public event Action<Collision> OnCollisionEnterEvent;
        public event Action<Collision> OnCollisionStayEvent;
        public event Action<Collision> OnCollisionExitEvent;

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
        protected override void InitializeInternal() {
            var meshController = Body.GetController<MeshController>();
            meshController.OnRefreshed += () => {
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

            OnRefreshed?.Invoke();
        }

        /// <summary>
        /// Trigger判定時通知
        /// </summary>
        private void OnTriggerEnter(Collider other) {
            OnTriggerEnterEvent?.Invoke(other);
        }

        private void OnTriggerStay(Collider other) {
            OnTriggerStayEvent?.Invoke(other);
        }

        private void OnTriggerExit(Collider other) {
            OnTriggerExitEvent?.Invoke(other);
        }

        /// <summary>
        /// Collision判定時通知
        /// </summary>
        private void OnCollisionEnter(Collision collision) {
            OnCollisionEnterEvent?.Invoke(collision);
        }

        private void OnCollisionStay(Collision collision) {
            OnCollisionStayEvent?.Invoke(collision);
        }

        private void OnCollisionExit(Collision collision) {
            OnCollisionExitEvent?.Invoke(collision);
        }
    }
}