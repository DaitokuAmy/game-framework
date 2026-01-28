using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameFramework.ActorSystems {
    /// <summary>
    /// プロパティ管理クラス
    /// </summary>
    public class PropertyParts : MonoBehaviour {
        [Serializable]
        private class FloatPropertyInfo {
            public string key;
            public float value;
        }

        [Serializable]
        private class IntPropertyInfo {
            public string key;
            public int value;
        }

        [Serializable]
        private class VectorPropertyInfo {
            public string key;
            public Vector4 value;
        }

        [Serializable]
        private class ColorPropertyInfo {
            public string key;
            public Color value;
        }

        [Serializable]
        private class StringPropertyInfo {
            public string key;
            public string value;
        }

        [Serializable]
        private class BooleanPropertyInfo {
            public string key;
            public bool value;
        }

        [Serializable]
        private class ObjectPropertyInfo {
            public string key;
            public Object value;
        }

        [SerializeField, Tooltip("FloatProperty情報")]
        private FloatPropertyInfo[] _floatPropertyInfos = Array.Empty<FloatPropertyInfo>();
        [SerializeField, Tooltip("IntProperty情報")]
        private IntPropertyInfo[] _intPropertyInfos = Array.Empty<IntPropertyInfo>();
        [SerializeField, Tooltip("VectorProperty情報")]
        private VectorPropertyInfo[] _vectorPropertyInfos = Array.Empty<VectorPropertyInfo>();
        [SerializeField, Tooltip("ColorProperty情報")]
        private ColorPropertyInfo[] _colorPropertyInfos = Array.Empty<ColorPropertyInfo>();
        [SerializeField, Tooltip("StringProperty情報")]
        private StringPropertyInfo[] _stringPropertyInfos = Array.Empty<StringPropertyInfo>();
        [SerializeField, Tooltip("BooleanProperty情報")]
        private BooleanPropertyInfo[] _booleanPropertyInfos = Array.Empty<BooleanPropertyInfo>();
        [SerializeField, Tooltip("ObjectProperty情報")]
        private ObjectPropertyInfo[] _objectPropertyInfos = Array.Empty<ObjectPropertyInfo>();

        private bool _initialized;

        // プロパティ情報
        private Dictionary<string, float> _floatProperties;
        private Dictionary<string, int> _intProperties;
        private Dictionary<string, Vector4> _vectorProperties;
        private Dictionary<string, Color> _colorProperties;
        private Dictionary<string, string> _stringProperties;
        private Dictionary<string, bool> _booleanProperties;
        private Dictionary<string, Object> _objectProperties;

        /// <summary>
        /// Floatパラメータ用のキー
        /// </summary>
        public IEnumerable<string> GetFloatKeys() {
            Initialize();
            return _floatProperties.Keys;
        }

        /// <summary>
        /// Intパラメータ用のキー
        /// </summary>
        public IEnumerable<string> GetIntKeys() {
            Initialize();
            return _intProperties.Keys;
        }

        /// <summary>
        /// Vectorパラメータ用のキー
        /// </summary>
        public IEnumerable<string> GetVectorKeys() {
            Initialize();
            return _vectorProperties.Keys;
        }

        /// <summary>
        /// Colorパラメータ用のキー
        /// </summary>
        public IEnumerable<string> GetColorKeys() {
            Initialize();
            return _colorProperties.Keys;
        }

        /// <summary>
        /// Booleanパラメータ用のキー
        /// </summary>
        public IEnumerable<string> GetBooleanKeys() {
            Initialize();
            return _booleanProperties.Keys;
        }

        /// <summary>
        /// Stringパラメータ用のキー
        /// </summary>
        public IEnumerable<string> GetStringKeys() {
            Initialize();
            return _stringProperties.Keys;
        }

        /// <summary>
        /// Objectパラメータ用のキー
        /// </summary>
        public IEnumerable<string> GetObjectKeys() {
            Initialize();
            return _objectProperties.Keys;
        }

        /// <summary>
        /// Floatパラメータの取得
        /// </summary>
        /// <param name="key">取得キー</param>
        /// <param name="value">値</param>
        public bool TryGetFloatProperty(string key, out float value) {
            Initialize();
            return _floatProperties.TryGetValue(key, out value);
        }

        /// <summary>
        /// Intパラメータの取得
        /// </summary>
        /// <param name="key">取得キー</param>
        /// <param name="value">値</param>
        public bool TryGetIntProperty(string key, out int value) {
            Initialize();
            return _intProperties.TryGetValue(key, out value);
        }

        /// <summary>
        /// Vectorパラメータの取得
        /// </summary>
        /// <param name="key">取得キー</param>
        /// <param name="value">値</param>
        public bool TryGetVectorProperty(string key, out Vector4 value) {
            Initialize();
            return _vectorProperties.TryGetValue(key, out value);
        }

        /// <summary>
        /// Colorパラメータの取得
        /// </summary>
        /// <param name="key">取得キー</param>
        /// <param name="value">値</param>
        public bool TryGetColorProperty(string key, out Color value) {
            Initialize();
            return _colorProperties.TryGetValue(key, out value);
        }

        /// <summary>
        /// Stringパラメータの取得
        /// </summary>
        /// <param name="key">取得キー</param>
        /// <param name="value">値</param>
        public bool TryGetStringProperty(string key, out string value) {
            Initialize();
            return _stringProperties.TryGetValue(key, out value);
        }

        /// <summary>
        /// Booleanパラメータの取得
        /// </summary>
        /// <param name="key">取得キー</param>
        /// <param name="value">値</param>
        public bool TryGetBooleanProperty(string key, out bool value) {
            Initialize();
            return _booleanProperties.TryGetValue(key, out value);
        }

        /// <summary>
        /// Objectパラメータの取得
        /// </summary>
        /// <param name="key">取得キー</param>
        /// <param name="value">値</param>
        public bool TryGetObjectProperty(string key, out Object value) {
            Initialize();
            return _objectProperties.TryGetValue(key, out value);
        }

        /// <summary>
        /// スクリプトリロード
        /// </summary>
        private void OnValidate() {
            _initialized = false;
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        private void Initialize() {
            if (_initialized) {
                return;
            }

            _initialized = true;
            _floatProperties = _floatPropertyInfos.ToDictionary(x => x.key, x => x.value);
            _intProperties = _intPropertyInfos.ToDictionary(x => x.key, x => x.value);
            _vectorProperties = _vectorPropertyInfos.ToDictionary(x => x.key, x => x.value);
            _colorProperties = _colorPropertyInfos.ToDictionary(x => x.key, x => x.value);
            _stringProperties = _stringPropertyInfos.ToDictionary(x => x.key, x => x.value);
            _booleanProperties = _booleanPropertyInfos.ToDictionary(x => x.key, x => x.value);
            _objectProperties = _objectPropertyInfos.ToDictionary(x => x.key, x => x.value);
        }
    }
}