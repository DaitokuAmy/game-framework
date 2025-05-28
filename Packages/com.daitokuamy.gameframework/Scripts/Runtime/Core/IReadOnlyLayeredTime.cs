using System;

namespace GameFramework.Core {
    /// <summary>
    /// 時間の階層管理用インターフェース
    /// </summary>
    public interface IReadOnlyLayeredTime {
        /// <summary>親階層を考慮しないTimeScale</summary>
        float LocalTimeScale { get; }
        /// <summary>親階層を考慮したTimeScale</summary>
        float TimeScale { get; }
        /// <summary>親のTimeScale</summary>
        float ParentTimeScale { get; }
        /// <summary>現フレームのDeltaTime</summary>
        float DeltaTime { get; }
        /// <summary>親のDeltaTime</summary>
        float ParentDeltaTime { get; }
        
        /// <summary>TimeScaleの変更通知</summary>
        event Action<float> ChangedTimeScaleEvent;
    }
}