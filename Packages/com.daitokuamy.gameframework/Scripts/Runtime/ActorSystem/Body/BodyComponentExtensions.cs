using GameFramework.PlayableSystem;
using UnityEngine;
using UnityEngine.Timeline;
using GameFramework.GimmickSystem;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GameFramework.ActorSystem {
    /// <summary>
    /// BodyComponent用の拡張メソッド
    /// </summary>
    public static class BodyComponentExtensions {
        /// <summary>
        /// AnimationClipモーションの設定
        /// </summary>
        public static AnimationClipPlayable Change(this MotionComponent source, AnimationClip clip, float blendDuration, bool autoDispose = true) {
            return source.Handle.Change(clip, blendDuration, autoDispose);
        }

        /// <summary>
        /// AnimatorControllerモーションの設定
        /// </summary>
        public static AnimatorControllerPlayable Change(this MotionComponent source, RuntimeAnimatorController controller, float blendDuration, bool autoDispose = true) {
            return source.Handle.Change(controller, blendDuration, autoDispose);
        }

        /// <summary>
        /// Timelineモーションの設定
        /// </summary>
        public static ScriptPlayable<TimelinePlayable> Change(this MotionComponent source, TimelineAsset timelineAsset, float blendDuration, bool autoDispose = true) {
            return source.Handle.Change(timelineAsset, blendDuration, autoDispose);
        }

        /// <summary>
        /// ActiveGimmickを取得
        /// </summary>
        /// <param name="source">操作対象</param>
        /// <param name="key">取得用のキー</param>
        public static ActiveGimmick[] GetActiveGimmicks(this GimmickComponent source, string key) {
            return source.GetGimmicks<ActiveGimmick>(key);
        }

        /// <summary>
        /// AnimationGimmickを取得
        /// </summary>
        /// <param name="source">操作対象</param>
        /// <param name="key">取得用のキー</param>
        public static AnimationGimmick[] GetAnimationGimmicks(this GimmickComponent source, string key) {
            return source.GetGimmicks<AnimationGimmick>(key);
        }

        /// <summary>
        /// InvokeGimmickを取得
        /// </summary>
        /// <param name="source">操作対象</param>
        /// <param name="key">取得用のキー</param>
        public static InvokeGimmick[] GetInvokeGimmicks(this GimmickComponent source, string key) {
            return source.GetGimmicks<InvokeGimmick>(key);
        }

        /// <summary>
        /// ChangeGimmickを取得
        /// </summary>
        /// <param name="source">操作対象</param>
        /// <param name="key">取得用のキー</param>
        public static ChangeGimmick<T>[] GetChangeGimmicks<T>(this GimmickComponent source, string key) {
            return source.GetGimmicks<ChangeGimmick<T>>(key);
        }

        /// <summary>
        /// StateGimmickを取得
        /// </summary>
        /// <param name="source">操作対象</param>
        /// <param name="key">取得用のキー</param>
        public static StateGimmick[] GetStateGimmicks(this GimmickComponent source, string key) {
            return source.GetGimmicks<StateGimmick>(key);
        }
    }
}