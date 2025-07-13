using UnityEngine;

namespace SampleGame.Domain.ModelViewer {
    /// <summary>
    /// 背景制御クラス
    /// </summary>
    public interface IEnvironmentActorPort {
        /// <summary>ライトTransform</summary>
        Transform LightSlot { get; }
    }
}