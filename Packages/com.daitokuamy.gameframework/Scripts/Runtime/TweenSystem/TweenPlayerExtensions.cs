using UnityEngine;
using UnityEngine.UI;

namespace GameFramework.TweenSystem {
    /// <summary>
    /// TweenPlayerのTweener生成ヘルパー
    /// </summary>
    public static class TweenPlayerExtensions {
        /// <summary>
        /// DelayTweenerを生成して初期化
        /// </summary>
        public static Tweener Delay(this TweenPlayer source, float seconds) {
            return source.CreateTweener<DelayTweener>().Setup(seconds);
        }

        /// <summary>
        /// MoveToTweenerを生成して初期化
        /// </summary>
        public static MoveToTweener MoveTo(this TweenPlayer source, Transform target, Vector3 to, float duration, Space space = Space.World) {
            return source.CreateTweener<MoveToTweener>().Setup(target, to, duration, space);
        }

        /// <summary>
        /// ScaleToTweenerを生成して初期化
        /// </summary>
        public static ScaleToTweener ScaleTo(this TweenPlayer source, Transform target, Vector3 to, float duration) {
            return source.CreateTweener<ScaleToTweener>().Setup(target, to, duration);
        }

        /// <summary>
        /// RotateToTweenerを生成して初期化
        /// </summary>
        public static RotateToTweener RotateTo(this TweenPlayer source, Transform target, Vector3 toEuler, float duration, Space space = Space.World) {
            return source.CreateTweener<RotateToTweener>().Setup(target, toEuler, duration, space);
        }

        /// <summary>
        /// RectTransformAnchoredPositionTweenerを生成して初期化
        /// </summary>
        public static RectTransformAnchoredPositionTweener AnchoredPositionTo(this TweenPlayer source, RectTransform target, Vector2 to, float duration) {
            return source.CreateTweener<RectTransformAnchoredPositionTweener>().Setup(target, to, duration);
        }

        /// <summary>
        /// RectTransformSizeDeltaTweenerを生成して初期化
        /// </summary>
        public static RectTransformSizeDeltaTweener SizeDeltaTo(this TweenPlayer source, RectTransform target, Vector2 to, float duration) {
            return source.CreateTweener<RectTransformSizeDeltaTweener>().Setup(target, to, duration);
        }

        /// <summary>
        /// GraphicColorTweenerを生成して初期化
        /// </summary>
        public static GraphicColorTweener GraphicColorTo(this TweenPlayer source, Graphic target, Color to, float duration) {
            return source.CreateTweener<GraphicColorTweener>().Setup(target, to, duration);
        }

        /// <summary>
        /// GraphicAlphaTweenerを生成して初期化
        /// </summary>
        public static GraphicAlphaTweener GraphicAlphaTo(this TweenPlayer source, Graphic target, float to, float duration) {
            return source.CreateTweener<GraphicAlphaTweener>().Setup(target, to, duration);
        }

        /// <summary>
        /// CanvasGroupAlphaTweenerを生成して初期化
        /// </summary>
        public static CanvasGroupAlphaTweener CanvasGroupAlphaTo(this TweenPlayer source, CanvasGroup target, float to, float duration) {
            return source.CreateTweener<CanvasGroupAlphaTweener>().Setup(target, to, duration);
        }

        /// <summary>
        /// ImageFillAmountTweenerを生成して初期化
        /// </summary>
        public static ImageFillAmountTweener ImageFillAmountTo(this TweenPlayer source, Image target, float to, float duration) {
            return source.CreateTweener<ImageFillAmountTweener>().Setup(target, to, duration);
        }

        /// <summary>
        /// SliderValueTweenerを生成して初期化
        /// </summary>
        public static SliderValueTweener SliderValueTo(this TweenPlayer source, Slider target, float to, float duration) {
            return source.CreateTweener<SliderValueTweener>().Setup(target, to, duration);
        }

        /// <summary>
        /// ScrollRectNormalizedPositionTweenerを生成して初期化
        /// </summary>
        public static ScrollRectNormalizedPositionTweener ScrollNormalizedPositionTo(this TweenPlayer source, ScrollRect target, Vector2 to, float duration) {
            return source.CreateTweener<ScrollRectNormalizedPositionTweener>().Setup(target, to, duration);
        }
    }
}
