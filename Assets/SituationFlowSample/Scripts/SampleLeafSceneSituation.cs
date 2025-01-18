namespace SituationFlowSample {
    /// <summary>
    /// サンプル用のLeafSituation基底
    /// </summary>
    public abstract class SampleLeafSceneSituation : SampleSceneSituation, ISampleLeafSituation {
        /// <summary>表示名</summary>
        string ISampleLeafSituation.DisplayName => GetType().Name.Replace("SampleLeafSceneSituation", "");
    }
}