using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.Animations;

namespace GameFramework.BodySystems {
    /// <summary>
    /// Mesh制御クラス
    /// </summary>
    public class MeshController : BodyController {
        // 追加したメッシュに関係する情報
        private class MergedInfo {
            // 追加時のGameObject
            public GameObject target;
            // メッシュパーツ
            public MeshParts meshParts;
        }

        // 依存コンポーネント
        private Animator _animator;
        private BoneController _boneController;

        // メッシュを追加する場所
        private Transform _additiveMeshRoot;
        // 追加したメッシュの情報
        private Dictionary<GameObject, MergedInfo> _mergedInfos =
            new();
        // 現在存在するRenderer
        private List<Renderer> _renderers = new();
        // 表示中フラグ
        private bool _isVisible = true;

        // 更新通知
        public event Action OnRefreshed;
        // Partsの追加通知
        public event Action<GameObject> OnAddedParts;
        // Partsの削除通知
        public event Action<GameObject> OnRemovedParts;
        // MeshPartsの追加通知
        public event Action<MeshParts> OnAddedMeshParts;
        // MeshPartsの削除通知
        public event Action<MeshParts> OnRemovedMeshParts;

        // 表示状態の切り替え
        public bool IsVisible {
            get => _isVisible;
            set {
                if (value == _isVisible) {
                    return;
                }

                _isVisible = value;
                SetVisible(_isVisible);
            }
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal() {
            _animator = Body.GetComponent<Animator>();
            _boneController = Body.GetController<BoneController>();

            // 追加Mesh用の箱を作る
            _additiveMeshRoot = new GameObject("AdditiveMeshRoot").transform;
            _additiveMeshRoot.gameObject.layer = Body.GameObject.layer;
            _additiveMeshRoot.SetParent(Body.Transform, false);

            // Rendererの回収
            RefreshRenderers();
        }

        /// <summary>
        /// GameObjectのMeshをマージする
        /// </summary>
        /// <param name="target">Meshを含むGameObject</param>
        /// <param name="prefix">骨をマージする際につけるPrefix</param>
        public void MergeMeshes(GameObject target, string prefix) {
            // 追加済み
            if (_mergedInfos.ContainsKey(target)) {
                Debug.LogWarning($"Already merged target. [{target.name}]");
                return;
            }

            // Layerを同じにする
            BodyUtility.SetLayer(target, _additiveMeshRoot.gameObject.layer);

            // 骨のマージ
            if (_boneController != null) {
                _boneController.MergeBones(target, prefix);
            }

            // ターゲットを移動させる
            target.transform.SetParent(_additiveMeshRoot, false);
            target.transform.localPosition = Vector3.zero;
            target.transform.localRotation = Quaternion.identity;
            target.transform.localScale = Vector3.one;

            // メッシュ情報をキャッシュ
            var meshParts = target.GetComponent<MeshParts>();
            _mergedInfos[target] = new MergedInfo {
                target = target,
                meshParts = meshParts,
            };

            // Rendererの回収
            RefreshRenderers();
            
            // AnimatorのRebind
            _animator.UnbindAllStreamHandles();
            _animator.Rebind();

            // 更新通知
            if (meshParts != null) {
                OnAddedMeshParts?.Invoke(meshParts);
            }

            OnAddedParts?.Invoke(target);
            OnRefreshed?.Invoke();
        }

        /// <summary>
        /// Merge済みのMeshを削除する
        /// </summary>
        /// <param name="target">Merge済みなGameObject</param>
        public void DeleteMergedMeshes(GameObject target) {
            if (!_mergedInfos.TryGetValue(target, out var mergedInfo)) {
                Debug.LogWarning($"Not merged target. [{target.name}]");
                return;
            }

            // 骨の削除
            if (_boneController != null) {
                _boneController.DeleteBones(mergedInfo.target);
            }
            
            // キャッシュクリア
            _mergedInfos.Remove(target);

            // Rendererの回収
            RefreshRenderers();
            
            // AnimatorのRebind
            _animator.UnbindAllStreamHandles();
            _animator.Rebind();

            // 削除通知
            if (mergedInfo.meshParts != null) {
                OnRemovedMeshParts?.Invoke(mergedInfo.meshParts);
            }

            OnRemovedParts?.Invoke(target);
            // 追加したメッシュの削除
            UnityEngine.Object.DestroyImmediate(target.gameObject);
            // 更新通知
            OnRefreshed?.Invoke();
        }

        /// <summary>
        /// 所持しているMeshPartsのリスト
        /// </summary>
        public MeshParts[] GetMeshPartsList() {
            return _mergedInfos
                .Select(x => x.Value.meshParts)
                .Where(x => x != null)
                .ToArray();
        }

        /// <summary>
        /// 内部で保持しているRendererの更新
        /// </summary>
        private void RefreshRenderers() {
            // Rendererの取得
            _renderers.Clear();
            _renderers.AddRange(Body.GetComponentsInChildren<Renderer>(true));

            // 表示状態の反映
            SetVisible(_isVisible);
        }

        /// <summary>
        /// Rendererの表示状態変更
        /// </summary>
        private void SetVisible(bool visible) {
            foreach (var renderer in _renderers) {
                renderer.enabled = visible;
            }
        }
    }
}