namespace GameFramework.TweenSystem {
    /// <summary>
    /// 再生中Tweenの操作ハンドル
    /// </summary>
    public readonly struct TweenHandle {
        private readonly TweenPlayer _owner;
        private readonly int _id;
        private readonly int _version;
        
        /// <summary>有効なハンドルかどうか</summary>
        public bool IsValid => _owner != null && _owner.IsHandleValid(_id, _version);

        /// <summary>完了済みかどうか</summary>
        public bool IsCompleted => _owner != null && _owner.IsCompleted(_id, _version);

        /// <summary>
        /// コンストラクタ（Playerからのみ生成）
        /// </summary>
        public TweenHandle(TweenPlayer owner, int id, int version) {
            _owner = owner;
            _id = id;
            _version = version;
        }

        /// <summary>
        /// キャンセル（Kill）
        /// </summary>
        public void Cancel() {
            _owner?.Cancel(_id, _version);
        }

        /// <summary>
        /// 強制完了
        /// </summary>
        public void ForceComplete() {
            _owner?.ForceComplete(_id, _version);
        }

        /// <summary>
        /// 一時停止
        /// </summary>
        public void Pause() {
            _owner?.Pause(_id, _version);
        }

        /// <summary>
        /// 再開
        /// </summary>
        public void Resume() {
            _owner?.Resume(_id, _version);
        }
    }
}
