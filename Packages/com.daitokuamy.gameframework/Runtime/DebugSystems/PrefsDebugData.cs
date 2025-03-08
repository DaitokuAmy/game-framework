using System;
using UnityEngine;

namespace GameFramework.DebugSystems {
    /// <summary>
    /// PlayerPrefs保存型のDebugデータ
    /// </summary>
    [Serializable]
    public abstract class PrefsDebugData<T> where T : PrefsDebugData<T>, new() {
        private static T s_instance;

        private static string Key => $"DebugData__{typeof(T).FullName}";
        protected static T Instance {
            get {
                if (s_instance == null) {
                    s_instance = new T();
                    LoadValues();
                }

                return s_instance;
            }
        }

        /// <summary>
        /// インスタンスの値をPlayerPrefsに保存
        /// </summary>
        protected static void SaveValues() {
            if (s_instance == null) {
                return;
            }

            PlayerPrefs.SetString(Key, JsonUtility.ToJson(s_instance));
            PlayerPrefs.Save();
        }

        /// <summary>
        /// インスタンスの値をPlayerPrefsから読み込み
        /// </summary>
        protected static void LoadValues() {
            if (s_instance == null) {
                return;
            }

            JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(Key), s_instance);
        }
    }
}