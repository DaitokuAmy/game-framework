namespace GameFramework.BodySystems {
    /// <summary>
    /// Invoke制御するGimmickの基底
    /// </summary>
    public abstract class InvokeGimmick : Gimmick {
        /// <summary>
        /// 実行
        /// </summary>
        public void Invoke() {
            InvokeInternal();
        }

        /// <summary>
        /// 実行処理
        /// </summary>
        protected abstract void InvokeInternal();
    }
}