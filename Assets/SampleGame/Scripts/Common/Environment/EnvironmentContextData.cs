using GameFramework.EnvironmentSystems;
using UnityEngine;

namespace SampleGame {
    /// <summary>
    /// 環境設定用のアセット
    /// </summary>
    [CreateAssetMenu(fileName = "dat_env_hoge.asset", menuName = "Sample/Environment Context Data")]
    public class EnvironmentContextData : ScriptableObject {
        [SerializeField]
        public EnvironmentDefaultSettings defaultSettings;

        /// <summary>
        /// コンテキストの作成
        /// </summary>
        public EnvironmentContext CreateContext() {
            return new EnvironmentContext {
                DefaultSettings = defaultSettings
            };
        }
    }
}