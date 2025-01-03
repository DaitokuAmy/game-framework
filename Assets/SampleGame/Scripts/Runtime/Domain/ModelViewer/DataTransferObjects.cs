namespace SampleGame.Domain.ModelViewer {
    /// <summary>
    /// アクター生成通知
    /// </summary>
    public struct CreatedPreviewActorDto {
        public IReadOnlyPreviewActorModel ActorModel;
    }

    /// <summary>
    /// アクター削除通知
    /// </summary>
    public struct DeletedPreviewActorDto {
        public IReadOnlyPreviewActorModel ActorModel;
    }
}