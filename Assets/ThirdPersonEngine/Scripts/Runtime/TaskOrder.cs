namespace ThirdPersonEngine {
    /// <summary>
    /// Task実行順
    /// </summary>
    public enum TaskOrder {
        PreSystem,
        Input,
        PreLogic,
        Model,
        Logic,
        AILogic,
        PostLogic,
        Actor,
        Body,
        Event,
        Cutscene,
        Collision,
        Camera,
        Sound,
        Effect,
        UI,
        PostSystem,
    }
}