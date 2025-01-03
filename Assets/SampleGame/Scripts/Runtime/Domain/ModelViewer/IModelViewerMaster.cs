namespace SampleGame.Domain.ModelViewer {
    /// <summary>
    /// モデルビューア用マスター用のインタフェース
    /// </summary>
    public interface IModelViewerMaster {
        /// <summary>初期状態で読み込むActorAssetKey</summary>
        int DefaultActorAssetKeyIndex { get; }
        /// <summary>ActorAssetKeyリスト</summary>
        string[] ActorAssetKeys { get; }

        /// <summary>初期状態で読み込むEnvironmentAssetKey</summary>
        int DefaultEnvironmentAssetKeyIndex { get; }
        /// <summary>EnvironmentAssetKeyリスト</summary>
        string[] EnvironmentAssetKeys { get; }
    }
}