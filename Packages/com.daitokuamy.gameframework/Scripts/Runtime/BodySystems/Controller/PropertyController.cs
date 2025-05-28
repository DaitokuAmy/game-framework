using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameFramework.BodySystems {
    /// <summary>
    /// プロパティ管理クラス
    /// </summary>
    public class PropertyController : BodyController {
        // プロパティ管理クラスのリスト
        private List<PropertyParts> _propertyPartsList = new();

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal() {
            var meshController = Body.GetController<MeshController>();
            meshController.OnRefreshed += RefreshPropertyPartsList;
            RefreshPropertyPartsList();
        }

        /// <summary>
        /// Floatパラメータキーの一覧を取得
        /// </summary>
        public string[] GetFloatKeys() {
            return _propertyPartsList.SelectMany(x => x.GetFloatKeys()).Distinct().ToArray();
        }

        /// <summary>
        /// Intパラメータキーの一覧を取得
        /// </summary>
        public string[] GetIntKeys() {
            return _propertyPartsList.SelectMany(x => x.GetIntKeys()).Distinct().ToArray();
        }

        /// <summary>
        /// Vectorパラメータキーの一覧を取得
        /// </summary>
        public string[] GetVectorKeys() {
            return _propertyPartsList.SelectMany(x => x.GetVectorKeys()).Distinct().ToArray();
        }

        /// <summary>
        /// Colorパラメータキーの一覧を取得
        /// </summary>
        public string[] GetColorKeys() {
            return _propertyPartsList.SelectMany(x => x.GetColorKeys()).Distinct().ToArray();
        }

        /// <summary>
        /// Stringパラメータキーの一覧を取得
        /// </summary>
        public string[] GetStringKeys() {
            return _propertyPartsList.SelectMany(x => x.GetStringKeys()).Distinct().ToArray();
        }

        /// <summary>
        /// Objectパラメータキーの一覧を取得
        /// </summary>
        public string[] GetObjectKeys() {
            return _propertyPartsList.SelectMany(x => x.GetObjectKeys()).Distinct().ToArray();
        }

        /// <summary>
        /// Floatパラメータの取得
        /// </summary>
        /// <param name="key">取得キー</param>
        /// <param name="defaultValue">見つからなかったときの値</param>
        public float GetFloatProperty(string key, float defaultValue = 0.0f) {
            for (var i = 0; i < _propertyPartsList.Count; i++) {
                if (_propertyPartsList[i].TryGetFloatProperty(key, out var val)) {
                    return val;
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// Intパラメータの取得
        /// </summary>
        /// <param name="key">取得キー</param>
        /// <param name="defaultValue">見つからなかったときの値</param>
        public int GetIntProperty(string key, int defaultValue = 0) {
            for (var i = 0; i < _propertyPartsList.Count; i++) {
                if (_propertyPartsList[i].TryGetIntProperty(key, out var val)) {
                    return val;
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// Vectorパラメータの取得
        /// </summary>
        /// <param name="key">取得キー</param>
        /// <param name="defaultValue">見つからなかったときの値</param>
        public Vector4 GetVectorProperty(string key, Vector4 defaultValue) {
            for (var i = 0; i < _propertyPartsList.Count; i++) {
                if (_propertyPartsList[i].TryGetVectorProperty(key, out var val)) {
                    return val;
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// Vectorパラメータの取得
        /// </summary>
        /// <param name="key">取得キー</param>
        public Vector4 GetVectorProperty(string key) {
            return GetVectorProperty(key, Vector4.zero);
        }

        /// <summary>
        /// Colorパラメータの取得
        /// </summary>
        /// <param name="key">取得キー</param>
        /// <param name="defaultValue">見つからなかったときの値</param>
        public Color GetColorProperty(string key, Color defaultValue) {
            for (var i = 0; i < _propertyPartsList.Count; i++) {
                if (_propertyPartsList[i].TryGetColorProperty(key, out var val)) {
                    return val;
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// Colorパラメータの取得
        /// </summary>
        /// <param name="key">取得キー</param>
        public Color GetColorProperty(string key) {
            return GetColorProperty(key, Color.white);
        }

        /// <summary>
        /// Stringパラメータの取得
        /// </summary>
        /// <param name="key">取得キー</param>
        /// <param name="defaultValue">見つからなかったときの値</param>
        public string GetStringProperty(string key, string defaultValue = "") {
            for (var i = 0; i < _propertyPartsList.Count; i++) {
                if (_propertyPartsList[i].TryGetStringProperty(key, out var val)) {
                    return val;
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// Booleanパラメータの取得
        /// </summary>
        /// <param name="key">取得キー</param>
        /// <param name="defaultValue">見つからなかったときの値</param>
        public bool GetBooleanProperty(string key, bool defaultValue = false) {
            for (var i = 0; i < _propertyPartsList.Count; i++) {
                if (_propertyPartsList[i].TryGetBooleanProperty(key, out var val)) {
                    return val;
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// Objectパラメータの取得
        /// </summary>
        /// <param name="key">取得キー</param>
        /// <param name="defaultValue">見つからなかったときの値</param>
        public Object GetObjectProperty(string key, Object defaultValue = null) {
            for (var i = 0; i < _propertyPartsList.Count; i++) {
                if (_propertyPartsList[i].TryGetObjectProperty(key, out var val)) {
                    return val;
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// PropertyPartsリストのリフレッシュ
        /// </summary>
        private void RefreshPropertyPartsList() {
            _propertyPartsList.Clear();
            _propertyPartsList.AddRange(Body.GetComponentsInChildren<PropertyParts>(true));
        }
    }
}