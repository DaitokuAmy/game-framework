using System;
using System.Collections.Generic;

namespace GameFramework.Core {
    /// <summary>
    /// レイヤー管理可能スケール(floatの乗算)
    /// </summary>
    public struct LayeredScale : IComparable<float>, IEquatable<LayeredScale> {
        /// <summary>1.0fを表すScale</summary>
        public static readonly LayeredScale One = new();
        
        private List<float> _scales;

        /// <summary>内部的なScaleリスト</summary>
        public IReadOnlyList<float> Scales {
            get {
                if (_scales == null) {
                    _scales = new List<float>();
                }

                return _scales;
            }
        }
        /// <summary>スケール値</summary>
        public float Value {
            get {
                var result = 1.0f;
                for (var i = 0; i < Scales.Count; i++) {
                    result *= _scales[i];
                }

                return result;
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="scale">初期値</param>
        public LayeredScale(LayeredScale scale)
            : this() {
            _scales = new List<float>(scale._scales);
        }

        /// <summary>
        /// 等価比較演算
        /// </summary>
        public static bool operator ==(LayeredScale lhs, LayeredScale rhs) {
            if (lhs.Scales.Count != rhs.Scales.Count) {
                return false;
            }
            
            var count = lhs.Scales.Count;
            var result = 0.0;
            for (var i = 0; i < count; i++) {
                var val = lhs.Scales[i] - rhs.Scales[i];
                result += val * val;
            }

            return result < 9.999999439624929E-11;
        }

        /// <summary>
        /// 等価比較演算
        /// </summary>
        public static bool operator ==(LayeredScale lhs, float rhs) {
            return lhs.Value.Equals(rhs);
        }

        /// <summary>
        /// 等価比較演算
        /// </summary>
        public static bool operator !=(LayeredScale lhs, LayeredScale rhs) {
            return !(lhs == rhs);
        }

        /// <summary>
        /// 等価比較演算
        /// </summary>
        public static bool operator !=(LayeredScale lhs, float rhs) {
            return !(lhs == rhs);
        }

        /// <summary>
        /// 比較演算
        /// </summary>
        public static bool operator >(LayeredScale lhs, float rhs) {
            return lhs.Value > rhs;
        }

        /// <summary>
        /// 比較演算
        /// </summary>
        public static bool operator >(LayeredScale lhs, LayeredScale rhs) {
            return lhs.Value > rhs.Value;
        }

        /// <summary>
        /// 比較演算
        /// </summary>
        public static bool operator <(LayeredScale lhs, float rhs) {
            return lhs.Value < rhs;
        }

        /// <summary>
        /// 比較演算
        /// </summary>
        public static bool operator <(LayeredScale lhs, LayeredScale rhs) {
            return lhs.Value < rhs.Value;
        }

        /// <summary>
        /// 比較演算
        /// </summary>
        public static bool operator >=(LayeredScale lhs, float rhs) {
            return lhs.Value >= rhs;
        }

        /// <summary>
        /// 比較演算
        /// </summary>
        public static bool operator >=(LayeredScale lhs, LayeredScale rhs) {
            return lhs.Value >= rhs.Value;
        }

        /// <summary>
        /// 比較演算
        /// </summary>
        public static bool operator <=(LayeredScale lhs, float rhs) {
            return lhs.Value <= rhs;
        }

        /// <summary>
        /// 比較演算
        /// </summary>
        public static bool operator <=(LayeredScale lhs, LayeredScale rhs) {
            return lhs.Value <= rhs.Value;
        }

        /// <summary>
        /// boolキャスト
        /// </summary>
        public static implicit operator float(LayeredScale scale) {
            return scale.Value;
        }

        /// <summary>
        /// 文字列化
        /// </summary>
        public override string ToString() {
            return Value.ToString("0.00");
        }

        /// <summary>
        /// Hash計算
        /// </summary>
        public override int GetHashCode() {
            return Value.GetHashCode();
        }

        /// <summary>
        /// 等価判定
        /// </summary>
        public override bool Equals(object obj) {
            return obj is LayeredScale other && Equals(other);
        }

        /// <summary>
        /// 等価判定
        /// </summary>
        public bool Equals(LayeredScale other) {
            return Value.Equals(other.Value);
        }

        /// <summary>
        /// float比較
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(float other) {
            return Value.CompareTo(other);
        }

        /// <summary>
        /// Scaleの設定
        /// </summary>
        /// <param name="index">設定したいIndex</param>
        /// <param name="value">設定する値</param>
        public void Set(int index, float value) {
            RebuildScales(index + 1);
            _scales[index] = value;
        }

        /// <summary>
        /// Scaleの設定
        /// </summary>
        /// <param name="enumValue">設定したいIndexを表すEnum</param>
        /// <param name="value">設定する値</param>
        public void Set(Enum enumValue, float value) {
            Set(Convert.ToInt32(enumValue), value);
        }

        /// <summary>
        /// Flagの設定
        /// </summary>
        /// <param name="value">設定する値</param>
        /// <param name="indices">設定したいIndexリスト</param>
        public void Set(float value, params int[] indices) {
            for (var i = 0; i < indices.Length; i++) {
                Set(indices[i], value);
            }
        }

        /// <summary>
        /// Flagの設定
        /// </summary>
        /// <param name="value">設定する値</param>
        /// <param name="enumValues">設定したいIndexを表すEnumリスト</param>
        public void Set(float value, params Enum[] enumValues) {
            for (var i = 0; i < enumValues.Length; i++) {
                Set(Convert.ToInt32(enumValues[i]), value);
            }
        }

        /// <summary>
        /// Scaleの一括設定
        /// </summary>
        public void SetAll(float value) {
            if (_scales == null) {
                return;
            }

            for (var i = 0; i < _scales.Count; i++) {
                _scales[i] = value;
            }
        }

        /// <summary>
        /// Scaleの加算
        /// </summary>
        /// <param name="index">加算したいIndex</param>
        /// <param name="value">加算する値</param>
        public void Add(int index, float value) {
            RebuildScales(index + 1);
            _scales[index] += value;
        }

        /// <summary>
        /// Scaleの加算
        /// </summary>
        /// <param name="enumValue">加算したいIndexを表すEnum</param>
        /// <param name="value">加算する値</param>
        public void Add(Enum enumValue, float value) {
            Add(Convert.ToInt32(enumValue), value);
        }

        /// <summary>
        /// Scaleの乗算
        /// </summary>
        /// <param name="index">乗算したいIndex</param>
        /// <param name="value">乗算する値</param>
        public void Mul(int index, float value) {
            RebuildScales(index + 1);
            _scales[index] *= value;
        }

        /// <summary>
        /// Scaleの乗算
        /// </summary>
        /// <param name="enumValue">乗算したいIndexを表すEnum</param>
        /// <param name="value">乗算する値</param>
        public void Mul(Enum enumValue, float value) {
            Mul(Convert.ToInt32(enumValue), value);
        }

        /// <summary>
        /// Scaleの除算
        /// </summary>
        /// <param name="index">除算したいIndex</param>
        /// <param name="value">除算する値</param>
        public void Div(int index, float value) {
            RebuildScales(index + 1);
            _scales[index] /= value;
        }

        /// <summary>
        /// Scaleの除算
        /// </summary>
        /// <param name="enumValue">除算したいIndexを表すEnum</param>
        /// <param name="value">除算する値</param>
        public void Div(Enum enumValue, float value) {
            Mul(Convert.ToInt32(enumValue), value);
        }

        /// <summary>
        /// Scaleのリセット(全要素に1.0を設定)
        /// </summary>
        public void Reset() {
            SetAll(1.0f);
        }

        /// <summary>
        /// Scaleの状態をクリア
        /// </summary>
        public void Clear() {
            _scales.Clear();
        }

        /// <summary>
        /// Index指定のScale取得
        /// </summary>
        /// <param name="index">取得対象のIndex</param>
        public float GetValue(int index) {
            if (index >= Scales.Count) {
                return 1.0f;
            }

            return _scales[index];
        }

        /// <summary>
        /// Index指定のScale取得
        /// </summary>
        /// <param name="enumValue">取得対象のIndexを表すEnum</param>
        public float GetValue(Enum enumValue) {
            return GetValue(Convert.ToInt32(enumValue));
        }

        /// <summary>
        /// Scale値のリビルド
        /// </summary>
        /// <param name="count">要素数</param>
        private void RebuildScales(int count) {
            if (count < 0) {
                return;
            }

            if (_scales == null) {
                _scales = new();
            }

            while (_scales.Count < count) {
                _scales.Add(1.0f);
            }
        }
    }
}