using System;
using UnityEngine;

namespace SampleGameEngine {
    /// <summary>
    /// アニメーションイベント受信用クラス
    /// </summary>
    public sealed class AnimationEventReceiver : MonoBehaviour, IAnimationEventProvider {
        /// <summary>足跡イベント</summary>
        public event Action<int> FootstepEvent;
        /// <summary>着地イベント</summary>
        public event Action<int> LandingEvent;
        /// <summary>ダッシュイベント</summary>
        public event Action<int> DashEvent;
        /// <summary>ブレーキイベント</summary>
        public event Action<int> BrakingEvent;
        
        /// <summary>
        /// 足跡タイミング
        /// </summary>
        public void Footstep(AnimationEvent animationEvent) {
            if (animationEvent.animatorClipInfo.weight > 0.5f) {
                FootstepEvent?.Invoke(animationEvent.intParameter);
            }
        }
        
        /// <summary>
        /// 着地タイミング
        /// </summary>
        public void Landing(AnimationEvent animationEvent) {
            LandingEvent?.Invoke(animationEvent.intParameter);
        }
        
        /// <summary>
        /// ダッシュタイミング
        /// </summary>
        public void Dash(AnimationEvent animationEvent) {
            if (animationEvent.animatorClipInfo.weight > 0.5f) {
                DashEvent?.Invoke(animationEvent.intParameter);
            }
        }
        
        /// <summary>
        /// ブレーキングタイミング
        /// </summary>
        public void Braking(AnimationEvent animationEvent) {
            if (animationEvent.animatorClipInfo.weight > 0.5f) {
                BrakingEvent?.Invoke(animationEvent.intParameter);
            }
        }
    }
}