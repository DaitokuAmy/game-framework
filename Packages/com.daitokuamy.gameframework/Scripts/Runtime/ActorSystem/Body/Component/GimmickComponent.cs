using System.Collections.Generic;
using GameFramework.GimmickSystem;

namespace GameFramework.ActorSystem {
    /// <summary>
    /// Gimmick制御用コントローラ
    /// </summary>
    public sealed class GimmickComponent : BodyComponent {
        // ギミック再生用クラス
        private GimmickPlayer _gimmickPlayer;

        /// <summary>
        /// ギミックのキー一覧を取得
        /// </summary>
        public string[] GetKeys() {
            return _gimmickPlayer.GetKeys();
        }

        /// <summary>
        /// ギミックのキー一覧を取得
        /// </summary>
        public string[] GetKeys<T>()
            where T : Gimmick {
            return _gimmickPlayer.GetKeys<T>();
        }

        /// <summary>
        /// ギミックの取得
        /// </summary>
        /// <param name="key">取得用のキー</param>
        /// <typeparam name="T">ギミックの型</typeparam>
        public IReadOnlyList<T> GetGimmicks<T>(string key)
            where T : Gimmick {
            return _gimmickPlayer.GetGimmicks<T>(key);
        }

        /// <inheritdoc/>
        protected override void InitializeInternal(IScope scope) {
            _gimmickPlayer = new GimmickPlayer();
            var meshController = Body.GetBodyComponent<MeshComponent>();
            if (meshController != null) {
                meshController.RefreshedEvent += RefreshGimmicks;
                scope.ExpiredEvent += () => { meshController.RefreshedEvent -= RefreshGimmicks; };
            }

            Body.LayeredTime.ChangedTimeScaleEvent += SetSpeed;
            scope.ExpiredEvent += () => { Body.LayeredTime.ChangedTimeScaleEvent -= SetSpeed; };
            RefreshGimmicks();
            SetSpeed(Body.LayeredTime.TimeScale);
        }

        /// <inheritdoc/>
        protected override void UpdateInternal(float deltaTime) {
            _gimmickPlayer.Update(deltaTime);
        }

        /// <inheritdoc/>
        protected override void LateUpdateInternal(float deltaTime) {
            _gimmickPlayer.LateUpdate(deltaTime);
        }

        /// <summary>
        /// ギミック情報の取得
        /// </summary>
        private void RefreshGimmicks() {
            _gimmickPlayer.Setup(Body.GetComponentsInChildren<GimmickGroup>(true));
        }

        /// <summary>
        /// ギミック速度の設定
        /// </summary>
        private void SetSpeed(float speed) {
            _gimmickPlayer.SetSpeed(speed);
        }
    }
}
