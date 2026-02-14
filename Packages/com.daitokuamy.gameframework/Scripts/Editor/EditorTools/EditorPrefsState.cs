using System;
using UnityEditor;
using UnityEngine;

namespace GameFramework.EditorTools.Editor {
    /// <summary>
    /// EditorPrefsへJSON保存する状態コンテナ
    /// </summary>
    public sealed class EditorPrefsState<T> where T : class, new() {
        private readonly string _editorPrefsKey;

        /// <summary>保持データ</summary>
        public T Value { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EditorPrefsState(string editorPrefsKey) {
            _editorPrefsKey = editorPrefsKey;
            Value = new T();
        }

        /// <summary>
        /// 読込
        /// </summary>
        public void Load() {
            if (!EditorPrefs.HasKey(_editorPrefsKey)) {
                Value = new T();
                return;
            }

            try {
                var json = EditorPrefs.GetString(_editorPrefsKey, "");
                Value = string.IsNullOrEmpty(json) ? new T() : JsonUtility.FromJson<T>(json) ?? new T();
            }
            catch (Exception ex) {
                Debug.LogException(ex);
                Value = new T();
            }
        }

        /// <summary>
        /// 保存
        /// </summary>
        public void Save() {
            try {
                var json = JsonUtility.ToJson(Value, false);
                EditorPrefs.SetString(_editorPrefsKey, json);
            }
            catch (Exception ex) {
                Debug.LogException(ex);
            }
        }

        /// <summary>
        /// 値を書き換えて保存
        /// </summary>
        public void Update(Action<T> updateAction, bool autoSave = true) {
            updateAction?.Invoke(Value);
            if (autoSave) {
                Save();
            }
        }

        /// <summary>
        /// 初期化
        /// </summary>
        public void Reset(bool clearKey = true) {
            Value = new T();
            if (clearKey && EditorPrefs.HasKey(_editorPrefsKey)) {
                EditorPrefs.DeleteKey(_editorPrefsKey);
            }
        }
    }
}
