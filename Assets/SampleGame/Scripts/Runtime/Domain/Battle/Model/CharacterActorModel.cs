using GameFramework.Core;
using UnityEngine;

namespace SampleGame.Domain.Battle {
    /// <summary>
    /// 読み取り専用インターフェース
    /// </summary>
    public interface IReadOnlyCharacterActorModel {
        /// <summary>識別子</summary>
        public int Id { get; }

        /// <summary>
        /// ルート座標の取得
        /// </summary>
        Vector3 GetRootPosition();

        /// <summary>
        /// ルート向きの取得
        /// </summary>
        Quaternion GetRootRotation();

        /// <summary>
        /// 注視向きの取得
        /// </summary>
        Quaternion GetLookAtRotation();
    }

    /// <summary>
    /// アクターモデル
    /// </summary>
    public class CharacterActorModel : IdLocalModel<int, CharacterActorModel>, IReadOnlyCharacterActorModel {
        /// <summary>制御用ポート</summary>
        internal ICharacterActorPort Port { get; private set; }
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CharacterActorModel(int id) : base(id) {
        }

        /// <inheritdoc/>
        public Vector3 GetRootPosition() {
            return Port.Position;
        }

        /// <inheritdoc/>
        public Quaternion GetRootRotation() {
            return Port.Rotation;
        }

        /// <inheritdoc/>
        public Quaternion GetLookAtRotation() {
            return Port.LookAtRotation;
        }

        /// <summary>
        /// セットアップ
        /// </summary>
        internal void Setup(ICharacterActorPort port) {
            Port = port;
        }
    }
}