using UnityEngine;
using UnityEngine.Timeline;

namespace GameFramework.PlayableSystems {
    /// <summary>
    /// PlayablePlayer用の拡張メソッド
    /// </summary>
    public static class PlayablePlayerExtensions {
        /// <summary>
        /// AnimationClipモーションの設定
        /// </summary>
        public static AnimationClipPlayableComponent Change(this MotionPlayer source, AnimationClip clip, float blendDuration, bool autoDispose = true) {
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
        public static AnimatorControllerPlayableComponent Change(this MotionPlayer source, RuntimeAnimatorController controller, float blendDuration, bool autoDispose = true) {
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
        public static TimelinePlayableComponent Change(this MotionPlayer source, TimelineAsset timelineAsset, float blendDuration, bool autoDispose = true) {
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
        public static LayerMixerPlayableComponent ChangeLayerMixer(this MotionPlayer source, float blendDuration, bool autoDispose = true) {
            var provider = new LayerMixerPlayableComponent(source.Animator, autoDispose);
            source.Handle.Change(provider, blendDuration);
            return provider;
        }
        
        /// <summary>
        /// AnimationClipモーションの設定
        /// </summary>
        public static AnimationClipPlayableComponent Change(this MotionHandle source, AnimationClip clip, float blendDuration, bool autoDispose = true) {
            if (!source.IsValid) {
                return null;
            }
            
            if (clip == null) {
                source.Change(null, blendDuration);
                return null;
            }

            var provider = new AnimationClipPlayableComponent(clip, autoDispose);
            source.Change(provider, blendDuration);
            return provider;
        }

        /// <summary>
        /// AnimatorControllerモーションの設定
        /// </summary>
        public static AnimatorControllerPlayableComponent Change(this MotionHandle source, RuntimeAnimatorController controller, float blendDuration, bool autoDispose = true) {
            if (!source.IsValid) {
                return null;
            }
            
            if (controller == null) {
                source.Change(null, blendDuration);
                return null;
            }

            var provider = new AnimatorControllerPlayableComponent(controller, autoDispose);
            source.Change(provider, blendDuration);
            return provider;
        }

        /// <summary>
        /// Timelineモーションの設定
        /// </summary>
        public static TimelinePlayableComponent Change(this MotionHandle source, TimelineAsset timelineAsset, float blendDuration, bool autoDispose = true) {
            if (!source.IsValid) {
                return null;
            }
            
            if (timelineAsset == null) {
                source.Change(null, blendDuration);
                return null;
            }

            var provider = new TimelinePlayableComponent(source.CrossFader.Animator, timelineAsset, autoDispose);
            source.Change(provider, blendDuration);
            return provider;
        }

        /// <summary>
        /// LayerMixerの設定
        /// </summary>
        public static LayerMixerPlayableComponent ChangeLayerMixer(this MotionHandle source, float blendDuration, bool autoDispose = true) {
            if (!source.IsValid) {
                return null;
            }

            var provider = new LayerMixerPlayableComponent(source.CrossFader.Animator, autoDispose);
            source.Change(provider, blendDuration);
            return provider;
        }
    }
}