namespace SituationFlowSample {
    /// <summary>
    /// サンプル用のLeafSituation基底
    /// </summary>
    public abstract class SampleLeafSituation : SampleSituation, ISampleLeafSituation {
        /// <summary>表示名</summary>
        string ISampleLeafSituation.DisplayName => GetType().Name.Replace("SampleLeafSituation", "");
    }
}