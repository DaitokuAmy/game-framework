using UnityEngine;

namespace GameFramework.Core {
    /// <summary>
    /// Easing用の拡張メソッド
    /// </summary>
    public static class EasingExtensions {
        /// <summary>
        /// 補間値計算
        /// </summary>
        /// <param name="source">補間タイプ</param>
        /// <param name="start">開始値</param>
        /// <param name="end">終了値</param>
        /// <param name="ratio">割合</param>
        public static float Evaluate(this EaseType source, float start, float end, float ratio) {
            return Easing.Evaluate(source, start, end, ratio);
        }
        /// <summary>
        /// 補間値計算(0～1)
        /// </summary>
        /// <param name="source">補間タイプ</param>
        /// <param name="ratio">割合</param>
        public static float Evaluate(this EaseType source, float ratio) {
            return Easing.Evaluate(source, ratio);
        }

        /// <summary>
        /// 補間値計算(Vector2)
        /// </summary>
        /// <param name="source">補間タイプ</param>
        /// <param name="start">開始値</param>
        /// <param name="end">終了値</param>
        /// <param name="ratio">割合</param>
        public static Vector2 Evaluate(this EaseType source, Vector2 start, Vector2 end, float ratio) {
            var t = Easing.Evaluate(source, 0.0f, 1.0f, ratio);
            return Vector2.LerpUnclamped(start, end, t);
        }

        /// <summary>
        /// 補間値計算(Vector3)
        /// </summary>
        /// <param name="source">補間タイプ</param>
        /// <param name="start">開始値</param>
        /// <param name="end">終了値</param>
        /// <param name="ratio">割合</param>
        public static Vector3 Evaluate(this EaseType source, Vector3 start, Vector3 end, float ratio) {
            var t = Easing.Evaluate(source, 0.0f, 1.0f, ratio);
            return Vector3.LerpUnclamped(start, end, t);
        }

        /// <summary>
        /// 補間値計算(Vector4)
        /// </summary>
        /// <param name="source">補間タイプ</param>
        /// <param name="start">開始値</param>
        /// <param name="end">終了値</param>
        /// <param name="ratio">割合</param>
        public static Vector4 Evaluate(this EaseType source, Vector4 start, Vector4 end, float ratio) {
            var t = Easing.Evaluate(source, 0.0f, 1.0f, ratio);
            return Vector4.LerpUnclamped(start, end, t);
        }

        /// <summary>
        /// 補間値計算(Color)
        /// </summary>
        /// <param name="source">補間タイプ</param>
        /// <param name="start">開始値</param>
        /// <param name="end">終了値</param>
        /// <param name="ratio">割合</param>
        public static Color Evaluate(this EaseType source, Color start, Color end, float ratio) {
            var t = Easing.Evaluate(source, 0.0f, 1.0f, ratio);
            return Color.LerpUnclamped(start, end, t);
        }

        /// <summary>
        /// 補間値計算(Color32)
        /// </summary>
        /// <param name="source">補間タイプ</param>
        /// <param name="start">開始値</param>
        /// <param name="end">終了値</param>
        /// <param name="ratio">割合</param>
        public static Color32 Evaluate(this EaseType source, Color32 start, Color32 end, float ratio) {
            var t = Easing.Evaluate(source, 0.0f, 1.0f, ratio);
            return Color32.LerpUnclamped(start, end, t);
        }
    }
}