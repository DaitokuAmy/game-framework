namespace ThirdPersonEngine {
    /// <summary>
    /// Update用の更新順番
    /// </summary>
    public enum UpdateOrder {
        /// <summary>無効値</summary>
        None = -1,
        /// <summary>Device情報の解釈</summary>
        Input,
        /// <summary>Logicに対する操作</summary>
        Controller,
        /// <summary>操作やイベント要求に対するLogic前処理</summary>
        Commit,
        /// <summary>ロジック処理</summary>
        Logic,
        /// <summary>Viewへの反映橋渡し処理</summary>
        Presenter,
        /// <summary>View処理（主にActor用）</summary>
        View,
        /// <summary>モーション適用など（Updateタイミング）</summary>
        Body,
        /// <summary>UI処理</summary>
        UI,
    }
    
    /// <summary>
    /// LateUpdate用の更新順番
    /// </summary>
    public enum LateUpdateOrder {
        /// <summary>無効値</summary>
        None = -1,
        /// <summary>コリジョン判定処理</summary>
        Collision,
        /// <summary>IKなどのRootMotion適用後（LateUpdateタイミング）</summary>
        Body,
        /// <summary>イベント検出、集約する処理</summary>
        EventReceiver,
        /// <summary>カメラ制御</summary>
        Camera,
        /// <summary>Vfx処理</summary>
        Vfx,
        /// <summary>サウンド処理</summary>
        Sound,
        /// <summary>UI処理</summary>
        UI,
    }
    
    /// <summary>
    /// FixedUpdate用の更新順番
    /// </summary>
    public enum FixedUpdateOrder {
        /// <summary>無効値</summary>
        None = -1,
    }
}