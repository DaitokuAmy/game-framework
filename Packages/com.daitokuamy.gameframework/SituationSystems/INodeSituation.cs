namespace GameFramework.SituationSystems {
    /// <summary>
    /// SituationTreeNodeに管理されたSituation用インターフェース
    /// </summary>
    public interface INodeSituation {
        /// <summary>
        /// Treeへの登録通知
        /// </summary>
        void OnRegisterTree(SituationFlow flow);

        /// <summary>
        /// Treeからの登録解除通知
        /// </summary>
        void OnUnregisterTree(SituationFlow flow);
    }
}