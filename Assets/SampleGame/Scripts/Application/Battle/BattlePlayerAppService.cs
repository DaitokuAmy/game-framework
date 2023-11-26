using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using SampleGame.Domain.Battle;
using SampleGame.Domain.User;
using UnityEngine;

namespace SampleGame.Application.Battle {
    /// <summary>
    /// BattlePlayer用のアプリケーションサービス
    /// </summary>
    public class BattlePlayerAppService : IDisposable {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BattlePlayerAppService() {
        }

        /// <summary>
        /// BattlePlayerの作成
        /// </summary>
        // public async UniTask<int> CreateBattlePlayer(UserPlayerModel userPlayerModel, int battlePlayerMasterId, CancellationToken ct) {
        //     ct.ThrowIfCancellationRequested();
        //
        //     await UniTask.WhenAll();
        // }

        /// <summary>
        /// 汎用アクションの再生
        /// </summary>
        /// <param name="id">BattlePlayerModelのID</param>
        /// <param name="actionIndex">アクションのIndex</param>
        public void PlayGeneralAction(int id, int actionIndex) {
            var model = BattlePlayerModel.Get(id);
            if (model == null) {
                return;
            }
        }

        /// <summary>
        /// ジャンプの再生
        /// </summary>
        /// <param name="id">BattlePlayerModelのID</param>
        public void PlayJumpAction(int id) {
            var model = BattlePlayerModel.Get(id);
            if (model == null) {
                return;
            }
        }

        /// <summary>
        /// Transformの設定
        /// </summary>
        /// <param name="id">BattlePlayerModelのID</param>
        /// <param name="position">座標</param>
        /// <param name="rotation">向き</param>
        public void SetTransform(int id, Vector3 position, Quaternion rotation) {
            var model = BattlePlayerModel.Get(id);
            if (model == null) {
                return;
            }

            if (model.ActorModel is BattleCharacterActorModel actorModel) {
                actorModel.SetPosition(position);
                actorModel.SetRotation(rotation);
            }
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            BattlePlayerModel.Reset();
        }
    }
}