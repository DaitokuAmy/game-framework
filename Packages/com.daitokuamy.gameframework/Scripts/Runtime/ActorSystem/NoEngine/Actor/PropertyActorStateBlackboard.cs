using System.Collections.Generic;

namespace GameFramework.ActorSystem {
    /// <summary>
    /// 汎用のキー指定可能なプロパティブラックボード
    /// </summary>
    public sealed class PropertyActorStateBlackboard<TKey> : IActorStateBlackboard {
        private readonly Dictionary<TKey, float> _floatValues = new();
        private readonly Dictionary<TKey, int> _intValues = new();
        private readonly Dictionary<TKey, string> _stringValues = new();
        private readonly Dictionary<TKey, bool> _boolValues = new();
        private readonly Dictionary<TKey, object> _objectValues = new();

        /// <summary>
        /// 小数設定
        /// </summary>
        public void SetFloat(TKey key, float value) {
            _floatValues[key] = value;
        }
        
        /// <summary>
        /// 整数設定
        /// </summary>
        public void SetInt(TKey key, int value) {
            _intValues[key] = value;
        }
        
        /// <summary>
        /// 文字列設定
        /// </summary>
        public void SetString(TKey key, string value) {
            _stringValues[key] = value;
        }

        /// <summary>
        /// 論理値設定
        /// </summary>
        public void SetBool(TKey key, bool value) {
            _boolValues[key] = value;
        }

        /// <summary>
        /// オブジェクト値設定
        /// </summary>
        public void SetObject(TKey key, object value) {
            _objectValues[key] = value;
        }

        /// <summary>
        /// 小数取得
        /// </summary>
        public float GetFloat(TKey key, float defaultValue = 0.0f) {
            return _floatValues.GetValueOrDefault(key, defaultValue);
        }

        /// <summary>
        /// 整数取得
        /// </summary>
        public int GetInt(TKey key, int defaultValue = 0) {
            return _intValues.GetValueOrDefault(key, defaultValue);
        }

        /// <summary>
        /// 文字列取得
        /// </summary>
        public string GetString(TKey key, string defaultValue = "") {
            return _stringValues.GetValueOrDefault(key, defaultValue);
        }

        /// <summary>
        /// 論理値取得
        /// </summary>
        public bool GetBool(TKey key, bool defaultValue = false) {
            return _boolValues.GetValueOrDefault(key, defaultValue);
        }
        
        /// <summary>
        /// オブジェクト値取得
        /// </summary>
        public object GetObject(TKey key, object defaultValue = null) {
            return _objectValues.GetValueOrDefault(key, defaultValue);
        }
    }
}
