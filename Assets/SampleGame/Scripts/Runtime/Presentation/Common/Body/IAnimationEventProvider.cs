using System;

namespace SampleGame.Presentation {
    /// <summary>
    /// アニメーションイベント提供用インターフェース
    /// </summary>
    public interface IAnimationEventProvider {
        /// <summary>足跡イベント</summary>
        event Action<int> FootstepEvent;
        /// <summary>着地イベント</summary>
        event Action<int> LandingEvent;
        /// <summary>ダッシュイベント</summary>
        event Action<int> DashEvent;
        /// <summary>ブレーキイベント</summary>
        event Action<int> BrakingEvent;
    }
}