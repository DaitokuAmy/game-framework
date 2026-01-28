using UnityEngine;

namespace GameFramework.ActorSystems {
    /// <summary>
    /// Mesh結合用のAvatarResolver
    /// </summary>
    public class MeshAvatarResolver : AvatarComponent.IResolver {
        private string _key;
        private GameObject _prefab;
        private string _parentLocatorName;
        private GameObject _partObject;

        // 識別キー
        string AvatarComponent.IResolver.Key => _key;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="key">識別キー</param>
        /// <param name="prefab">パーツのPrefab</param>
        /// <param name="parentLocatorName">結合親のロケーター名(未指定でデフォルト)</param>
        public MeshAvatarResolver(string key, GameObject prefab, string parentLocatorName = null) {
            _key = key;
            _prefab = prefab;
            _parentLocatorName = parentLocatorName;
        }

        /// <summary>
        /// パーツ適用時処理
        /// </summary>
        void AvatarComponent.IResolver.Setup(Body owner) {
            if (_prefab == null) {
                return;
            }

            _partObject = Object.Instantiate(_prefab);
            var meshController = owner.GetBodyComponent<MeshComponent>();
            meshController.MergeMeshes(_partObject, _key, string.IsNullOrEmpty(_parentLocatorName) ? null : owner.Locators[_parentLocatorName]);
        }

        /// <summary>
        /// パーツ適用解除処理
        /// </summary>
        void AvatarComponent.IResolver.Cleanup(Body owner) {
            if (_partObject == null) {
                return;
            }

            var meshController = owner.GetBodyComponent<MeshComponent>();
            meshController.DeleteMergedMeshes(_partObject);
            _partObject = null;
        }
    }
}