using UnityEngine;

namespace GameFramework.BodySystems {
    /// <summary>
    /// Mesh結合用のAvatarResolver
    /// </summary>
    public class MeshAvatarResolver : AvatarController.IResolver {
        private string _key;
        private GameObject _prefab;
        private GameObject _partObject;

        // 識別キー
        string AvatarController.IResolver.Key => _key;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="key">識別キー</param>
        /// <param name="prefab">パーツのPrefab</param>
        public MeshAvatarResolver(string key, GameObject prefab) {
            _key = key;
            _prefab = prefab;
        }

        /// <summary>
        /// パーツ適用時処理
        /// </summary>
        void AvatarController.IResolver.Setup(Body owner) {
            if (_prefab == null) {
                return;
            }

            _partObject = Object.Instantiate(_prefab);
            var meshController = owner.GetController<MeshController>();
            meshController.MergeMeshes(_partObject, _key);
        }

        /// <summary>
        /// パーツ適用解除処理
        /// </summary>
        void AvatarController.IResolver.Cleanup(Body owner) {
            if (_partObject == null) {
                return;
            }

            var meshController = owner.GetController<MeshController>();
            meshController.DeleteMergedMeshes(_partObject);
            _partObject = null;
        }
    }
}