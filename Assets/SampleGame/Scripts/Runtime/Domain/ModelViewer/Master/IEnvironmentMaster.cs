using UnityEngine;

namespace SampleGame.Domain.ModelViewer {
    /// <summary>
    /// 背景マスター用インターフェース
    /// </summary>
    public interface IEnvironmentMaster {
        /// <summary>表示名</summary>
        string DisplayName { get; }
        /// <summary>背景読み込み用のアセットキー</summary>
        string AssetKey { get; }
        /// <summary>アクター配置ルート座標</summary>
        Vector3 RootPosition { get; }
        /// <summary>アクター配置ルート向き</summary>
        Vector3 RootAngles { get; }
    }
}