namespace GameFramework.UISystems {    
    /// <summary>
    /// UIAnimation用インターフェース
    /// </summary>
    public interface IUIAnimation {
        /// <summary>トータル時間</summary>
        float Duration { get; }

        /// <summary>
        /// 時間の設定
        /// </summary>
        /// <param name="time">現在時間</param>
        void SetTime(float time);

        /// <summary>
        /// 再生開始時通知
        /// </summary>
        void OnPlay();
    }
}