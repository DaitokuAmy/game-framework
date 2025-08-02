using System;
using UnityEngine;

namespace GameFramework {
    // <summary>
    // NullableなEnumシリアライズ用のクラス
    // </summary>
    [Serializable]
    public struct NullableEnum<TEnum> : IEquatable<NullableEnum<TEnum>>
        where TEnum : struct, Enum {
        /// <summary>無効値の定数</summary>
        public static readonly NullableEnum<TEnum> None = new();

        [SerializeField, Tooltip("値が存在するか")]
        private bool _hasValue;
        [SerializeField, Tooltip("シリアライズ用の値")]
        private TEnum _value;

        /// <summary>値</summary>
        public TEnum Value => _hasValue ? _value : throw new InvalidOperationException($"Value is null in {nameof(NullableEnum<TEnum>)}");
        /// <summary>値が存在するか</summary>
        public bool HasValue => _hasValue;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public NullableEnum(TEnum value) {
            _hasValue = true;
            _value = value;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) {
            return obj is NullableEnum<TEnum> other && this == other;
        }

        /// <inheritdoc/>
        public override int GetHashCode() {
            return _hasValue ? _value.GetHashCode() : 0;
        }

        /// <inheritdoc/>
        public override string ToString() {
            return _hasValue ? _value.ToString() : "None";
        }

        /// <summary>
        /// 値を取得またはデフォルト値
        /// </summary>
        public TEnum GetValueOrDefault(TEnum defaultValue = default) {
            return _hasValue ? _value : defaultValue;
        }

        /// <summary>
        /// 値比較
        /// </summary>
        public bool Equals(NullableEnum<TEnum> other) {
            return this == other;
        }

        /// <summary>
        /// 明示的なnull解除演算子
        /// </summary>
        public static explicit operator TEnum(NullableEnum<TEnum> optional) {
            return optional.Value;
        }

        /// <summary>
        /// 暗黙のEnum→NullableEnum変換
        /// </summary>
        public static implicit operator NullableEnum<TEnum>(TEnum value) {
            return new NullableEnum<TEnum>(value);
        }

        /// <summary>
        /// 暗黙のNullableEnum→Enum?変換
        /// </summary>
        public static implicit operator TEnum?(NullableEnum<TEnum> value) {
            return value._hasValue ? value._value : null;
        }

        /// <summary>
        /// 暗黙のEnum?→NullableEnum変換
        /// </summary>
        public static implicit operator NullableEnum<TEnum>(TEnum? value) {
            return value.HasValue ? new(value.Value) : None;
        }

        /// <summary>
        /// 比較演算子 ==
        /// </summary>
        public static bool operator ==(NullableEnum<TEnum> left, NullableEnum<TEnum> right) {
            if (!left._hasValue && !right._hasValue) {
                return true;
            }

            if (left._hasValue != right._hasValue) {
                return false;
            }

            return left._value.Equals(right._value);
        }

        /// <summary>
        /// 比較演算子 ==
        /// </summary>
        public static bool operator ==(NullableEnum<TEnum> left, TEnum? right) {
            if (!left._hasValue && !right.HasValue) {
                return true;
            }

            if (left._hasValue != right.HasValue) {
                return false;
            }

            return left._value.Equals(right.Value);
        }

        /// <summary>
        /// 比較演算子 ==
        /// </summary>
        public static bool operator ==(TEnum? left, NullableEnum<TEnum> right) {
            return right == left;
        }

        /// <summary>
        /// 比較演算子 !=
        /// </summary>
        public static bool operator !=(NullableEnum<TEnum> left, NullableEnum<TEnum> right) {
            return !(left == right);
        }

        /// <summary>
        /// 比較演算子 !=
        /// </summary>
        public static bool operator !=(NullableEnum<TEnum> left, TEnum? right) {
            return !(left == right);
        }

        /// <summary>
        /// 比較演算子 !=
        /// </summary>
        public static bool operator !=(TEnum? left, NullableEnum<TEnum> right) {
            return !(right == left);
        }
    }
}