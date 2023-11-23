namespace GameFramework.SituationSystems {
    /// <summary>
    /// SituationTreeNodeに管理されたSituation用インターフェース
    /// </summary>
    public interface INodeSituation {
        /// <summary>
        /// SituationFlowへの登録通知
        /// </summary>
        void OnRegisterFlow(SituationFlow flow);

        /// <summary>
        /// SituationFlowからの登録解除通知
        /// </summary>
        void OnUnregisterFlow(SituationFlow flow);
    }
}