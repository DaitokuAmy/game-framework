using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using GameFramework.Core;
using UnityEngine.Animations;

namespace GameFramework.ActorSystems {
    /// <summary>
    /// Mesh制御クラス
    /// </summary>
    public class MeshComponent : BodyComponent {
        /// <summary>
        /// 追加したメッシュに関係する情報
        /// </summary>
        private class MergedInfo {
            /// <summary>追加時のGameObject</summary>
            public GameObject Target;
            /// <summary>メッシュパーツ</summary>
            public MeshParts MeshParts;
        }

        // 依存コンポーネント
        private Animator _animator;
        private BoneComponent _boneComponent;

        // メッシュを追加する場所
        private Transform _additiveMeshRoot;
        // 追加したメッシュの情報
        private Dictionary<GameObject, MergedInfo> _mergedInfos =
            new();
        // 現在存在するRenderer
        private List<Renderer> _renderers = new();
        // 表示中フラグ
        private bool _isVisible = true;

        /// <summary>更新通知</summary>
        public event Action RefreshedEvent;
        /// <summary>Partsの追加通知</summary>
        public event Action<GameObject> AddedPartsEvent;
        /// <summary>Partsの削除通知</summary>
        public event Action<GameObject> RemovedPartsEvent;
        /// <summary>MeshPartsの追加通知</summary>
        public event Action<MeshParts> AddedMeshPartsEvent;
        /// <summary>MeshPartsの削除通知</summary>
        public event Action<MeshParts> RemovedMeshPartsEvent;

        /// <summary>表示状態の切り替え</summary>
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
        protected override void InitializeInternal(IScope scope) {
            _animator = Body.GetComponent<Animator>();
            _boneComponent = Body.GetBodyComponent<BoneComponent>();

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
        /// <param name="overrideParent">上書き用結合先のTransform親</param>
        public void MergeMeshes(GameObject target, string prefix, Transform overrideParent = null) {
            // 追加済み
            if (_mergedInfos.ContainsKey(target)) {
                Debug.LogWarning($"Already merged target. [{target.name}]");
                return;
            }

            // Layerを同じにする
            GameObjectUtility.SetLayer(target, _additiveMeshRoot.gameObject.layer);

            // 骨のマージ
            if (_boneComponent != null) {
                _boneComponent.MergeBones(target, prefix);
            }

            // ターゲットを移動させる
            target.transform.SetParent(overrideParent != null ? overrideParent : _additiveMeshRoot, false);
            target.transform.localPosition = Vector3.zero;
            target.transform.localRotation = Quaternion.identity;
            target.transform.localScale = Vector3.one;

            // メッシュ情報をキャッシュ
            var meshParts = target.GetComponent<MeshParts>();
            _mergedInfos[target] = new MergedInfo {
                Target = target,
                MeshParts = meshParts,
            };

            // Rendererの回収
            RefreshRenderers();
            
            // AnimatorのRebind
            _animator.UnbindAllStreamHandles();
            _animator.Rebind();

            // 更新通知
            if (meshParts != null) {
                AddedMeshPartsEvent?.Invoke(meshParts);
            }

            AddedPartsEvent?.Invoke(target);
            RefreshedEvent?.Invoke();
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
            if (_boneComponent != null) {
                _boneComponent.DeleteBones(mergedInfo.Target);
            }
            
            // キャッシュクリア
            _mergedInfos.Remove(target);

            // Rendererの回収
            RefreshRenderers();
            
            // AnimatorのRebind
            _animator.UnbindAllStreamHandles();
            _animator.Rebind();

            // 削除通知
            if (mergedInfo.MeshParts != null) {
                RemovedMeshPartsEvent?.Invoke(mergedInfo.MeshParts);
            }

            RemovedPartsEvent?.Invoke(target);
            // 追加したメッシュの削除
            UnityEngine.Object.DestroyImmediate(target.gameObject);
            // 更新通知
            RefreshedEvent?.Invoke();
        }

        /// <summary>
        /// 所持しているMeshPartsのリスト
        /// </summary>
        public MeshParts[] GetMeshPartsList() {
            return _mergedInfos
                .Select(x => x.Value.MeshParts)
                .Where(x => x != null)
                .ToArray();
        }

        /// <summary>
        /// レイヤーの設定（管理対象のRenderer全て）
        /// </summary>
        /// <param name="layer">設定するレイヤー</param>
        public void SetLayer(int layer) {
            foreach (var renderer in _renderers) {
                renderer.gameObject.layer = layer;
            }
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