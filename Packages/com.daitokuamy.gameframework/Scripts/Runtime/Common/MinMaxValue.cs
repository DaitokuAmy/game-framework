using System;
using UnityEngine;
using GameFramework.Core;

namespace GameFramework {
    /// <summary>
    /// 最小値、最大値を扱う値のインターフェース
    /// </summary>
    public interface IMinMaxValue<T> {
        /// <summary>乱数生成時にランダムを使うか(使わない場合、MinValueを使用）</summary>
        bool UseRandom { get; }
        /// <summary>最小値</summary>
        T MinValue { get; }
        /// <summary>最大値</summary>
        T MaxValue { get; }

        /// <summary>
        /// 線形補間
        /// </summary>
        /// <param name="t">補間割合</param>
        T Lerp(float t);

        /// <summary>
        /// ランダム
        /// </summary>
        T Rand(FastRandom random);
    }
    
    /// <summary>
    /// 最小、最大値
    /// </summary>
    [Serializable]
    public struct MinMaxFloat : IMinMaxValue<float> {
        [Tooltip("ランダムを使うか")]
        public bool useRandom;
        [Tooltip("最小値")]
        public float minValue;
        [Tooltip("最大値")]
        public float maxValue;

        bool IMinMaxValue<float>.UseRandom => useRandom;
        float IMinMaxValue<float>.MinValue => minValue;
        float IMinMaxValue<float>.MaxValue => maxValue;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MinMaxFloat(float value) {
            useRandom = false;
            minValue = value;
            maxValue = value;
        }

        /// <summary>
        /// キャスト
        /// </summary>
        public static implicit operator MinMaxFloat(float value) {
            return new MinMaxFloat(value);
        }

        /// <summary>
        /// 線形補間
        /// </summary>
        /// <param name="t">補間割合</param>
        public float Lerp(float t) {
            return Mathf.Lerp(minValue, maxValue, t);
        }
        
        /// <summary>
        /// 線形補間
        /// </summary>
        /// <param name="t">補間割合</param>
        public float LerpUnclamped(float t) {
            return Mathf.LerpUnclamped(minValue, maxValue, t);
        }

        /// <summary>
        /// ランダム
        /// </summary>
        public float Rand(FastRandom random) {
            if (!useRandom) {
                return minValue;
            }
            
            return Lerp(random.Range(0.0f, 1.0f));
        }
    }
    
    /// <summary>
    /// 最小、最大値
    /// </summary>
    [Serializable]
    public struct MinMaxVector2 : IMinMaxValue<Vector2> {
        [Tooltip("ランダムを使うか")]
        public bool useRandom;
        [Tooltip("最小値")]
        public Vector2 minValue;
        [Tooltip("最大値")]
        public Vector2 maxValue;

        bool IMinMaxValue<Vector2>.UseRandom => useRandom;
        Vector2 IMinMaxValue<Vector2>.MinValue => minValue;
        Vector2 IMinMaxValue<Vector2>.MaxValue => maxValue;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MinMaxVector2(Vector2 value) {
            useRandom = false;
            minValue = value;
            maxValue = value;
        }

        /// <summary>
        /// キャスト
        /// </summary>
        public static implicit operator MinMaxVector2(Vector2 value) {
            return new MinMaxVector2(value);
        }
        
        /// <summary>
        /// 線形補間
        /// </summary>
        /// <param name="t">補間割合</param>
        public Vector2 Lerp(float t) {
            return Vector2.Lerp(minValue, maxValue, t);
        }
        
        /// <summary>
        /// 線形補間
        /// </summary>
        /// <param name="t">補間割合</param>
        public Vector2 LerpUnclamped(float t) {
            return Vector2.LerpUnclamped(minValue, maxValue, t);
        }

        /// <summary>
        /// ランダム
        /// </summary>
        public Vector2 Rand(FastRandom random) {
            if (!useRandom) {
                return minValue;
            }
            
            return new Vector2(
                Mathf.Lerp(minValue.x, maxValue.x, random.Range(0.0f, 1.0f)),
                Mathf.Lerp(minValue.y, maxValue.y, random.Range(0.0f, 1.0f)));
        }
    }
    
    /// <summary>
    /// 最小、最大値
    /// </summary>
    [Serializable]
    public struct MinMaxVector3 : IMinMaxValue<Vector3> {
        [Tooltip("ランダムを使うか")]
        public bool useRandom;
        [Tooltip("最小値")]
        public Vector3 minValue;
        [Tooltip("最大値")]
        public Vector3 maxValue;

        bool IMinMaxValue<Vector3>.UseRandom => useRandom;
        Vector3 IMinMaxValue<Vector3>.MinValue => minValue;
        Vector3 IMinMaxValue<Vector3>.MaxValue => maxValue;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MinMaxVector3(Vector3 value) {
            useRandom = false;
            minValue = value;
            maxValue = value;
        }

        /// <summary>
        /// キャスト
        /// </summary>
        public static implicit operator MinMaxVector3(Vector3 value) {
            return new MinMaxVector3(value);
        }

        /// <summary>
        /// 線形補間
        /// </summary>
        /// <param name="t">補間割合</param>
        public Vector3 Lerp(float t) {
            return Vector3.Lerp(minValue, maxValue, t);
        }
        
        /// <summary>
        /// 線形補間
        /// </summary>
        /// <param name="t">補間割合</param>
        public Vector3 LerpUnclamped(float t) {
            return Vector3.LerpUnclamped(minValue, maxValue, t);
        }

        /// <summary>
        /// ランダム
        /// </summary>
        public Vector3 Rand(FastRandom random) {
            if (!useRandom) {
                return minValue;
            }
            
            return new Vector3(
                Mathf.Lerp(minValue.x, maxValue.x, random.Range(0.0f, 1.0f)),
                Mathf.Lerp(minValue.y, maxValue.y, random.Range(0.0f, 1.0f)),
                Mathf.Lerp(minValue.z, maxValue.z, random.Range(0.0f, 1.0f)));
        }

        /// <summary>
        /// 座標変換
        /// </summary>
        public void TransformVector(Transform transform) {
            if (transform == null) {
                return;
            }
            
            minValue = transform.TransformVector(minValue);
            maxValue = transform.TransformVector(maxValue);
        }
    }
    
    /// <summary>
    /// 最小、最大値
    /// </summary>
    [Serializable]
    public struct MinMaxVector4 : IMinMaxValue<Vector4> {
        [Tooltip("ランダムを使うか")]
        public bool useRandom;
        [Tooltip("最小値")]
        public Vector4 minValue;
        [Tooltip("最大値")]
        public Vector4 maxValue;

        bool IMinMaxValue<Vector4>.UseRandom => useRandom;
        Vector4 IMinMaxValue<Vector4>.MinValue => minValue;
        Vector4 IMinMaxValue<Vector4>.MaxValue => maxValue;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MinMaxVector4(Vector4 value) {
            useRandom = false;
            minValue = value;
            maxValue = value;
        }

        /// <summary>
        /// キャスト
        /// </summary>
        public static implicit operator MinMaxVector4(Vector4 value) {
            return new MinMaxVector4(value);
        }

        /// <summary>
        /// 線形補間
        /// </summary>
        /// <param name="t">補間割合</param>
        public Vector4 Lerp(float t) {
            return Vector3.Lerp(minValue, maxValue, t);
        }
        
        /// <summary>
        /// 線形補間
        /// </summary>
        /// <param name="t">補間割合</param>
        public Vector4 LerpUnclamped(float t) {
            return Vector3.LerpUnclamped(minValue, maxValue, t);
        }

        /// <summary>
        /// ランダム
        /// </summary>
        public Vector4 Rand(FastRandom random) {
            if (!useRandom) {
                return minValue;
            }
            
            return new Vector4(
                Mathf.Lerp(minValue.x, maxValue.x, random.Range(0.0f, 1.0f)),
                Mathf.Lerp(minValue.y, maxValue.y, random.Range(0.0f, 1.0f)),
                Mathf.Lerp(minValue.z, maxValue.z, random.Range(0.0f, 1.0f)),
                Mathf.Lerp(minValue.w, maxValue.w, random.Range(0.0f, 1.0f)));
        }
    }
    
    /// <summary>
    /// 最小、最大値カーブ
    /// </summary>
    [Serializable]
    public struct MinMaxAnimationCurve {
        [Tooltip("ランダムを使うか")]
        public bool useRandom;
        [Tooltip("最小値")]
        public AnimationCurve minValue;
        [Tooltip("最大値")]
        public AnimationCurve maxValue;
        
        /// <summary>デフォルトの線形補間割合</summary>
        public float LerpRatio { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MinMaxAnimationCurve(AnimationCurve value) {
            useRandom = false;
            minValue = value;
            maxValue = value;
            LerpRatio = 0.0f;
        }

        /// <summary>
        /// キャスト
        /// </summary>
        public static implicit operator MinMaxAnimationCurve(AnimationCurve value) {
            return new MinMaxAnimationCurve(value);
        }

        /// <summary>
        /// 線形補間付き値のサンプリング
        /// </summary>
        /// <param name="time">Curveの横軸値</param>
        /// <param name="t">線形補間割合</param>
        public readonly float LerpEvaluate(float time, float t) {
            var min = minValue.Evaluate(time);
            var max = maxValue.Evaluate(time);
            return Mathf.Lerp(min, max, t);
        }
        
        /// <summary>
        /// 線形補間付き値のサンプリング
        /// </summary>
        /// <param name="time">Curveの横軸値</param>
        /// <param name="t">線形補間割合</param>
        public readonly float LerpUnclampedEvaluate(float time, float t) {
            var min = minValue.Evaluate(time);
            var max = maxValue.Evaluate(time);
            return Mathf.LerpUnclamped(min, max, t);
        }

        /// <summary>
        /// デフォルト線形補間値を使ったサンプリング
        /// </summary>
        /// <param name="time">Curveの横軸値</param>
        public readonly float Evaluate(float time) {
            return LerpEvaluate(time, LerpRatio);
        }

        /// <summary>
        /// デフォルト線形補間値をランダムに設定
        /// </summary>
        public void RandDefaultRatio() {
            if (useRandom) {
                LerpRatio = RandomUtil.Range(0.0f, 1.0f);
            }
            else {
                LerpRatio = 0.0f;
            }
        }

        /// <summary>
        /// デフォルト線形補間値をランダムに設定
        /// </summary>
        public void RandDefaultRatio(int seed) {
            if (useRandom) {
                var random = new FastRandom(seed);
                LerpRatio = random.Range(0.0f, 1.0f);
            }
            else {
                LerpRatio = 0.0f;
            }
        }
    }
}