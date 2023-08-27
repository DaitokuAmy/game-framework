using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.AssetSystems {
    /// <summary>
    /// アセット管理クラス
    /// </summary>
    public class AssetManager {
        private List<IAssetProvider> _providers = new List<IAssetProvider>();

        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <param name="providers">読み込みに使用するAssetProviderのリスト</param>
        public void Initialize(params IAssetProvider[] providers) {
            _providers.Clear();
            _providers.AddRange(providers);
        }

        /// <summary>
        /// Providerの取得
        /// </summary>
        /// <param name="index">ProviderのIndex</param>
        public IAssetProvider GetProvider(int index) {
            if (index < 0 || index >= _providers.Count) {
                Debug.LogError($"Not found provider. [{index}]");
                return null;
            }

            return _providers[index];
        }

        /// <summary>
        /// Providerの取得
        /// </summary>
        /// <param name="index">列挙型のProviderタイプ</param>
        public IAssetProvider GetProvider<T>(T index)
            where T : Enum {
            return GetProvider(Convert.ToInt32(index));
        }
    }
}