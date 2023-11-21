namespace GameFramework.SituationSystems {
    /// <summary>
    /// SituationContainerを提供するためのインターフェース
    /// </summary>
    public interface ISituationContainerProvider {
        SituationContainer Container { get; }
    }
}