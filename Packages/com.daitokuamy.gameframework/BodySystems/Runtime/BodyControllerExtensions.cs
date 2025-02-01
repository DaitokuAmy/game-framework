using GameFramework.PlayableSystems;
using UnityEngine;
using UnityEngine.Timeline;
using GameFramework.GimmickSystems;

namespace GameFramework.BodySystems {
    /// <summary>
    /// BodyController用の拡張メソッド
    /// </summary>
    public static class BodyControllerExtensions {
        /// <summary>
        /// AnimationClipモーションの設定
        /// </summary>
        public static AnimationClipPlayableComponent Change(this MotionController source, AnimationClip clip, float blendDuration, bool autoDispose = true) {
            if (clip == null) {
                source.Handle.Change(null, blendDuration);
                return null;
            }

            var provider = new AnimationClipPlayableComponent(clip, autoDispose);
            source.Handle.Change(provider, blendDuration);
            return provider;
        }

        /// <summary>
        /// AnimatorControllerモーションの設定
        /// </summary>
        public static AnimatorControllerPlayableComponent Change(this MotionController source, RuntimeAnimatorController controller, float blendDuration, bool autoDispose = true) {
            if (controller == null) {
                source.Handle.Change(null, blendDuration);
                return null;
            }

            var provider = new AnimatorControllerPlayableComponent(controller, autoDispose);
            source.Handle.Change(provider, blendDuration);
            return provider;
        }

        /// <summary>
        /// Timelineモーションの設定
        /// </summary>
        public static TimelinePlayableComponent Change(this MotionController source, TimelineAsset timelineAsset, float blendDuration, bool autoDispose = true) {
            if (timelineAsset == null) {
                source.Handle.Change(null, blendDuration);
                return null;
            }

            var provider = new TimelinePlayableComponent(source.Animator, timelineAsset, autoDispose);
            source.Handle.Change(provider, blendDuration);
            return provider;
        }

        /// <summary>
        /// LayerMixerの設定
        /// </summary>
        public static LayerMixerPlayableComponent ChangeLayerMixer(this MotionController source, float blendDuration, bool autoDispose = true) {
            var provider = new LayerMixerPlayableComponent(source.Animator, autoDispose);
            source.Handle.Change(provider, blendDuration);
            return provider;
        }
        
        /// <summary>
        /// ActiveGimmickを取得
        /// </summary>
        /// <param name="source">操作対象</param>
        /// <param name="key">取得用のキー</param>
        public static ActiveGimmick[] GetActiveGimmicks(this GimmickController source, string key) {
            return source.GetGimmicks<ActiveGimmick>(key);
        }

        /// <summary>
        /// AnimationGimmickを取得
        /// </summary>
        /// <param name="source">操作対象</param>
        /// <param name="key">取得用のキー</param>
        public static AnimationGimmick[] GetAnimationGimmicks(this GimmickController source, string key) {
            return source.GetGimmicks<AnimationGimmick>(key);
        }

        /// <summary>
        /// InvokeGimmickを取得
        /// </summary>
        /// <param name="source">操作対象</param>
        /// <param name="key">取得用のキー</param>
        public static InvokeGimmick[] GetInvokeGimmicks(this GimmickController source, string key) {
            return source.GetGimmicks<InvokeGimmick>(key);
        }

        /// <summary>
        /// ChangeGimmickを取得
        /// </summary>
        /// <param name="source">操作対象</param>
        /// <param name="key">取得用のキー</param>
        public static ChangeGimmick<T>[] GetChangeGimmicks<T>(this GimmickController source, string key) {
            return source.GetGimmicks<ChangeGimmick<T>>(key);
        }

        /// <summary>
        /// StateGimmickを取得
        /// </summary>
        /// <param name="source">操作対象</param>
        /// <param name="key">取得用のキー</param>
        public static StateGimmick[] GetStateGimmicks(this GimmickController source, string key) {
            return source.GetGimmicks<StateGimmick>(key);
        }
    }
}