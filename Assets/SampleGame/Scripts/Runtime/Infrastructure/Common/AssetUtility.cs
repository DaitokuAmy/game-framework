using System.Collections.Generic;
using GameFramework.AssetSystem;

namespace SampleGame.Infrastructure {
    /// <summary>
    /// AssetSystem構築用ユーティリティ
    /// </summary>
    public static class AssetUtility {
        /// <summary>
        /// AssetLoader群の生成
        /// </summary>
        public static IAssetLoader[] CreateAssetLoaders() {
            var loaders = new List<IAssetLoader>();
#if UNITY_EDITOR
            loaders.Add(new AssetDatabaseAssetLoader());
#endif
            loaders.Add(new AddressablesAssetLoader());
            loaders.Add(new ResourcesAssetLoader());
            return loaders.ToArray();
        }

        /// <summary>
        /// SceneLoader群の生成
        /// </summary>
        public static ISceneLoader[] CreateSceneLoaders() {
            var loaders = new List<ISceneLoader>();
#if UNITY_EDITOR
            loaders.Add(new AssetDatabaseSceneLoader());
#endif
            loaders.Add(new AddressablesSceneLoader());
            return loaders.ToArray();
        }
    }
}