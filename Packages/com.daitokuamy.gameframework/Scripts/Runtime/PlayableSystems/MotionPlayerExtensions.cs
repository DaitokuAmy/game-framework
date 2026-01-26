using UnityEngine;
using UnityEngine.Timeline;

namespace GameFramework.PlayableSystems {
    /// <summary>
    /// MotionPlayer用の拡張メソッド
    /// </summary>
    public static class MotionPlayerExtensions {
        /// <summary>
        /// AnimationClipモーションの設定
        /// </summary>
        public static AnimationClipPlayableComponent Change(this MotionPlayer source, AnimationClip clip, float blendDuration, bool autoDispose = true) {
            if (clip == null) {
                source.Handle.Change(null, blendDuration, autoDispose);
                return null;
            }

            var component = new AnimationClipPlayableComponent(clip);
            source.Handle.Change(component, blendDuration, autoDispose);
            return component;
        }

        /// <summary>
        /// AnimatorControllerモーションの設定
        /// </summary>
        public static AnimatorControllerPlayableComponent Change(this MotionPlayer source, RuntimeAnimatorController controller, float blendDuration, bool autoDispose = true) {
            if (controller == null) {
                source.Handle.Change(null, blendDuration, autoDispose);
                return null;
            }

            var component = new AnimatorControllerPlayableComponent(controller);
            source.Handle.Change(component, blendDuration, autoDispose);
            return component;
        }

        /// <summary>
        /// Timelineモーションの設定
        /// </summary>
        public static TimelinePlayableComponent Change(this MotionPlayer source, TimelineAsset timelineAsset, float blendDuration, bool autoDispose = true) {
            if (timelineAsset == null) {
                source.Handle.Change(null, blendDuration, autoDispose);
                return null;
            }

            var component = new TimelinePlayableComponent(source.Animator, timelineAsset);
            source.Handle.Change(component, blendDuration, autoDispose);
            return component;
        }

        /// <summary>
        /// LayerMixerの設定
        /// </summary>
        public static LayerMixerPlayableComponent ChangeLayerMixer(this MotionPlayer source, float blendDuration, bool autoDispose = true) {
            var component = new LayerMixerPlayableComponent(source.Animator);
            source.Handle.Change(component, blendDuration, autoDispose);
            return component;
        }
        
        /// <summary>
        /// AnimationClipモーションの設定
        /// </summary>
        public static AnimationClipPlayableComponent Change(this MotionHandle source, AnimationClip clip, float blendDuration, bool autoDispose = true) {
            if (!source.IsValid) {
                return null;
            }
            
            if (clip == null) {
                source.Change(null, blendDuration, autoDispose);
                return null;
            }

            var component = new AnimationClipPlayableComponent(clip);
            source.Change(component, blendDuration, autoDispose);
            return component;
        }

        /// <summary>
        /// AnimatorControllerモーションの設定
        /// </summary>
        public static AnimatorControllerPlayableComponent Change(this MotionHandle source, RuntimeAnimatorController controller, float blendDuration, bool autoDispose = true) {
            if (!source.IsValid) {
                return null;
            }
            
            if (controller == null) {
                source.Change(null, blendDuration, autoDispose);
                return null;
            }

            var component = new AnimatorControllerPlayableComponent(controller);
            source.Change(component, blendDuration, autoDispose);
            return component;
        }

        /// <summary>
        /// Timelineモーションの設定
        /// </summary>
        public static TimelinePlayableComponent Change(this MotionHandle source, TimelineAsset timelineAsset, float blendDuration, bool autoDispose = true) {
            if (!source.IsValid) {
                return null;
            }
            
            if (timelineAsset == null) {
                source.Change(null, blendDuration, autoDispose);
                return null;
            }

            var component = new TimelinePlayableComponent(source.CrossFader.Animator, timelineAsset);
            source.Change(component, blendDuration, autoDispose);
            return component;
        }

        /// <summary>
        /// LayerMixerの設定
        /// </summary>
        public static LayerMixerPlayableComponent ChangeLayerMixer(this MotionHandle source, float blendDuration, bool autoDispose = true) {
            if (!source.IsValid) {
                return null;
            }

            var component = new LayerMixerPlayableComponent(source.CrossFader.Animator);
            source.Change(component, blendDuration, autoDispose);
            return component;
        }
    }
}