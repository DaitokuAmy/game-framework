using GameFramework.Core;
using UnityEngine;

namespace SampleGame.Domain.ModelViewer {
    /// <summary>
    /// 読み取り専用インターフェース
    /// </summary>
    public interface IReadOnlyEnvironmentModel {
        /// <summary>識別子</summary>
        int Id { get; }
        /// <summary>マスター</summary>
        IEnvironmentActorMaster ActorMaster { get; }
        /// <summary>ライトTransform</summary>
        Transform LightSlot { get; }
    }

    /// <summary>
    /// 環境モデル
    /// </summary>
    public class EnvironmentActorModel : AutoIdModel<EnvironmentActorModel>, IReadOnlyEnvironmentModel {
        private IEnvironmentActorPort _environmentActorPort;

        /// <summary>マスター</summary>
        public IEnvironmentActorMaster ActorMaster { get; private set; }
        /// <inheritdoc/>
        public Transform LightSlot => _environmentActorPort?.LightSlot;

        /// <summary>有効か</summary>
        private bool IsActive => _environmentActorPort != null;

        /// <summary>
        /// データの設定
        /// </summary>
        public void Setup(IEnvironmentActorMaster actorMaster) {
            // マスター設定
            ActorMaster = actorMaster;
        }

        /// <summary>
        /// 制御ポートの設定
        /// </summary>
        public void SetPort(IEnvironmentActorPort actorPort) {
            _environmentActorPort = actorPort;
        }
    }
}