using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.AssetSystem {
    /// <summary>
    /// アセット管理クラス
    /// </summary>
    public class AssetManager {
        private readonly List<IAssetProvider> _providers = new();
        private readonly Dictionary<string, IAssetProvider> _providersByKey = new(StringComparer.Ordinal);

        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <param name="providers">読み込みに使用するAssetProviderのリスト</param>
        public void Initialize(params IAssetProvider[] providers) {
            _providers.Clear();
            _providersByKey.Clear();
            _providers.AddRange(providers);

            foreach (var provider in providers) {
                if (provider == null) {
                    continue;
                }

                if (string.IsNullOrEmpty(provider.Key)) {
                    Debug.LogError($"Provider key is null or empty. [{provider.GetType().Name}]");
                    continue;
                }

                _providersByKey[provider.Key] = provider;
            }
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
        /// <param name="key">Providerのキー</param>
        public IAssetProvider GetProvider(string key) {
            if (string.IsNullOrEmpty(key)) {
                Debug.LogError("Provider key is null or empty.");
                return null;
            }

            if (_providersByKey.TryGetValue(key, out var provider)) {
                return provider;
            }

            Debug.LogError($"Not found provider. [{key}]");
            return null;
        }

        /// <summary>
        /// Providerの取得
        /// </summary>
        /// <param name="key">列挙型のProviderキー</param>
        public IAssetProvider GetProvider<T>(T key)
            where T : Enum {
            return GetProvider(key.ToString());
        }
    }
}
