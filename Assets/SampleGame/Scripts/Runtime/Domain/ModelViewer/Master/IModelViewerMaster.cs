using System.Collections.Generic;

namespace SampleGame.Domain.ModelViewer {
    /// <summary>
    /// モデルビューア用マスター用のインタフェース
    /// </summary>
    public interface IModelViewerMaster {
        /// <summary>初期状態で読み込むActorのId</summary>
        int DefaultActorId { get; }
        /// <summary>初期状態で読み込むEnvironmentのId</summary>
        int DefaultEnvironmentId { get; }
        /// <summary>ActorIdの一覧</summary>
        IReadOnlyList<int> ActorIds { get; }
        /// <summary>EnvironmentIdの一覧</summary>
        IReadOnlyList<int> EnvironmentIds { get; }
    }
}