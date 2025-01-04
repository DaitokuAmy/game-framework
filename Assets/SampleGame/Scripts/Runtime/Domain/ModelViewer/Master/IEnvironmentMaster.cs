using UnityEngine;

namespace SampleGame.Domain.ModelViewer {
    /// <summary>
    /// 背景マスター用インターフェース
    /// </summary>
    public interface IEnvironmentMaster {
        /// <summary>名前</summary>
        string Name { get; }
        /// <summary>背景読み込み用のアセットキー</summary>
        string AssetKey { get; }
        /// <summary>ビューアのルート座標</summary>
        Vector3 RootPosition { get; }
        /// <summary>ビューアのルート向き</summary>
        Vector3 RootAngles { get; }
    }
}