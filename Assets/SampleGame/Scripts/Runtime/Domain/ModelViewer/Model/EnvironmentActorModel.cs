using GameFramework.Core;
using Unity.Mathematics;

namespace SampleGame.Domain.ModelViewer {
    /// <summary>
    /// 読み取り専用インターフェース
    /// </summary>
    public interface IReadOnlyEnvironmentModel {
        /// <summary>識別子</summary>
        int Id { get; }
        /// <summary>マスター</summary>
        IModelViewerEnvironmentMaster Master { get; }
        /// <summary>ルート位置</summary>
        float3 RootPosition { get; }
        /// <summary>ルート向き</summary>
        quaternion RootRotation { get; }
    }

    /// <summary>
    /// 環境モデル
    /// </summary>
    public class EnvironmentActorModel : AutoIdModel<EnvironmentActorModel>, IReadOnlyEnvironmentModel {
        private IEnvironmentActorPort _environmentActorPort;

        /// <inheritdoc/>
        public IModelViewerEnvironmentMaster Master { get; private set; }
        /// <inheritdoc/>
        public float3 RootPosition => _environmentActorPort.RootPosition;
        /// <inheritdoc/>
        public quaternion RootRotation => _environmentActorPort.RootRotation;

        /// <summary>有効か</summary>
        private bool IsActive => _environmentActorPort != null;

        /// <summary>
        /// データの設定
        /// </summary>
        public void Setup(IModelViewerEnvironmentMaster master) {
            // マスター設定
            Master = master;
        }

        /// <summary>
        /// 制御ポートの設定
        /// </summary>
        public void SetPort(IEnvironmentActorPort actorPort) {
            _environmentActorPort = actorPort;
        }
    }
}