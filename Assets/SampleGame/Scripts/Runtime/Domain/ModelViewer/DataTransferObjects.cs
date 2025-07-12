namespace SampleGame.Domain.ModelViewer {
    /// <summary>
    /// アクター生成通知
    /// </summary>
    public struct ChangedPreviewActorDto {
        public IReadOnlyPreviewActorModel Model;
    }

    /// <summary>
    /// アクター削除通知
    /// </summary>
    public struct DeletedPreviewActorDto {
        public IReadOnlyPreviewActorModel PreviewActorModel;
    }
    
    /// <summary>
    /// 環境生成通知
    /// </summary>
    public struct ChangedEnvironmentActorDto {
        public IReadOnlyEnvironmentModel Model;
    }
    
    /// <summary>
    /// 環境削除通知
    /// </summary>
    public struct DeletedEnvironmentActorDto {
        public IReadOnlyEnvironmentModel EnvironmentModel;
    }
}