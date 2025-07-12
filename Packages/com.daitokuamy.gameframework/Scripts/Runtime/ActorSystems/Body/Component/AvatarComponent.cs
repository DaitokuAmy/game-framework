using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameFramework.ActorSystems {
    /// <summary>
    /// Avatar制御用クラス
    /// </summary>
    public class AvatarComponent : SerializedBodyComponent {
        /// <summary>
        /// Avatar適用のためのインターフェース
        /// </summary>
        public interface IResolver {
            // 対象キー
            string Key { get; }

            /// <summary>
            /// 適用処理
            /// </summary>
            /// <param name="owner">適用対象のBody</param>
            void Setup(Body owner);

            /// <summary>
            /// 適用解除処理
            /// </summary>
            /// <param name="owner">解除対象のBody</param>
            void Cleanup(Body owner);
        }

        private Dictionary<string, IResolver> _resolvers = new Dictionary<string, IResolver>();

        /// <summary>
        /// パーツキーの一覧を取得
        /// </summary>
        public string[] GetPartKeys() {
            return _resolvers.Keys.ToArray();
        }

        /// <summary>
        /// アバターパーツの変更
        /// </summary>
        /// <param name="resolver">パーツを適用させるためのResolver</param>
        public void ChangePart(IResolver resolver) {
            if (resolver == null) {
                Debug.LogError("resolver is null.");
                return;
            }

            var key = resolver.Key;

            // 既に存在していたら削除
            if (_resolvers.TryGetValue(key, out var currentResolver)) {
                currentResolver.Cleanup(Body);
            }

            resolver.Setup(Body);
            _resolvers[key] = resolver;
        }

        /// <summary>
        /// アバターパーツのリセット
        /// </summary>
        /// <param name="key">リセット対象のキー</param>
        public void ResetPart(string key) {
            if (_resolvers.TryGetValue(key, out var currentResolver)) {
                currentResolver.Cleanup(Body);
                _resolvers.Remove(key);
            }
        }

        /// <summary>
        /// アバターパーツの一括リセット
        /// </summary>
        public void ResetParts() {
            var keys = _resolvers.Keys.ToArray();
            foreach (var key in keys) {
                ResetPart(key);
            }
        }
    }
}