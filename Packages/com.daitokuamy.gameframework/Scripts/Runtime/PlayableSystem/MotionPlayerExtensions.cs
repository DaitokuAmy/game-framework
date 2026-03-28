using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace GameFramework.PlayableSystem {
    /// <summary>
    /// MotionPlayer用の拡張メソッド
    /// </summary>
    public static class MotionPlayerExtensions {
        /// <summary>
        /// AnimationClipモーションの設定
        /// </summary>
        public static AnimationClipPlayable Change(this MotionPlayer source, AnimationClip clip, float blendDuration, bool autoDispose = true) {
            return source.Handle.Change(clip, blendDuration, autoDispose);
        }

        /// <summary>
        /// AnimatorControllerモーションの設定
        /// </summary>
        public static AnimatorControllerPlayable Change(this MotionPlayer source, RuntimeAnimatorController controller, float blendDuration, bool autoDispose = true) {
            return source.Handle.Change(controller, blendDuration, autoDispose);
        }

        /// <summary>
        /// Timelineモーションの設定
        /// </summary>
        public static ScriptPlayable<TimelinePlayable> Change(this MotionPlayer source, TimelineAsset timelineAsset, float blendDuration, bool autoDispose = true) {
            return source.Handle.Change(timelineAsset, blendDuration, autoDispose);
        }

        /// <summary>
        /// AnimationClipモーションの設定
        /// </summary>
        public static AnimationClipPlayable Change(this MotionHandle source, AnimationClip clip, float blendDuration, bool autoDispose = true) {
            if (!source.IsValid) {
                return default;
            }

            if (clip == null) {
                source.Change(null, blendDuration, autoDispose);
                return default;
            }

            var playable = AnimationClipPlayable.Create(source.Graph, clip);
            source.Change(playable, blendDuration, autoDispose);
            return playable;
        }

        /// <summary>
        /// AnimatorControllerモーションの設定
        /// </summary>
        public static AnimatorControllerPlayable Change(this MotionHandle source, RuntimeAnimatorController controller, float blendDuration, bool autoDispose = true) {
            if (!source.IsValid) {
                return default;
            }

            if (controller == null) {
                source.Change(null, blendDuration, autoDispose);
                return default;
            }

            var playable = AnimatorControllerPlayable.Create(source.Graph, controller);
            source.Change(playable, blendDuration, autoDispose);
            return playable;
        }

        /// <summary>
        /// Timelineモーションの設定
        /// </summary>
        public static ScriptPlayable<TimelinePlayable> Change(this MotionHandle source, TimelineAsset timelineAsset, float blendDuration, bool autoDispose = true) {
            if (!source.IsValid) {
                return default;
            }

            if (timelineAsset == null) {
                source.Change(null, blendDuration, autoDispose);
                return default;
            }

            var tracks = timelineAsset.GetOutputTracks()
                .OfType<AnimationTrack>();
            var playable = TimelinePlayable.Create(source.Graph, tracks, source.Animator.gameObject, true, false);
            playable.SetDuration(timelineAsset.duration);
            source.Change(playable, blendDuration, autoDispose);
            return playable;
        }
    }
}
