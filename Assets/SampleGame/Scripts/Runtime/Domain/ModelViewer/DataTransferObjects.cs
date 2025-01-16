namespace SampleGame.Domain.ModelViewer {
    /// <summary>
    /// アクター生成通知
    /// </summary>
    public struct CreatedActorDto {
        public IReadOnlyActorModel ActorModel;
    }

    /// <summary>
    /// アクター削除通知
    /// </summary>
    public struct DeletedActorDto {
        public IReadOnlyActorModel ActorModel;
    }
    
    /// <summary>
    /// 環境生成通知
    /// </summary>
    public struct CreatedEnvironmentDto {
        public IReadOnlyEnvironmentModel EnvironmentModel;
    }
    
    /// <summary>
    /// 環境削除通知
    /// </summary>
    public struct DeletedEnvironmentDto {
        public IReadOnlyEnvironmentModel EnvironmentModel;
    }
}