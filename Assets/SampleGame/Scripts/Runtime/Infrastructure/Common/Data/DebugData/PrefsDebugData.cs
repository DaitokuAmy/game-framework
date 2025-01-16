using System;
using UnityEngine;

namespace SampleGame.Infrastructure {
    /// <summary>
    /// PlayerPrefs保存型のDebugデータ
    /// </summary>
    [Serializable]
    public class PrefsDebugData<T> where T : PrefsDebugData<T>, new() {
        private static T _instance;

        private static string Key => $"DebugData__{typeof(T).FullName}";
        protected static T Instance {
            get {
                if (_instance == null) {
                    _instance = new T();
                    LoadValues();
                }

                return _instance;
            }
        }

        /// <summary>
        /// インスタンスの値をPlayerPrefsに保存
        /// </summary>
        protected static void SaveValues() {
            if (_instance == null) {
                return;
            }

            PlayerPrefs.SetString(Key, JsonUtility.ToJson(_instance));
            PlayerPrefs.Save();
        }

        /// <summary>
        /// インスタンスの値をPlayerPrefsから読み込み
        /// </summary>
        protected static void LoadValues() {
            if (_instance == null) {
                return;
            }

            JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(Key), _instance);
        }
    }
}