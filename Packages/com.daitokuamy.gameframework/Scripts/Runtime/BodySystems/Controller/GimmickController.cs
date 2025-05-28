using GameFramework.GimmickSystems;

namespace GameFramework.BodySystems {
    /// <summary>
    /// Gimmick制御用コントローラ
    /// </summary>
    public class GimmickController : BodyController {
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
        public T[] GetGimmicks<T>(string key)
            where T : Gimmick {
            return _gimmickPlayer.GetGimmicks<T>(key);
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal() {
            _gimmickPlayer = new GimmickPlayer();
            var meshController = Body.GetController<MeshController>();
            meshController.OnRefreshed += RefreshGimmicks;
            Body.LayeredTime.ChangedTimeScaleEvent += SetSpeed;
            RefreshGimmicks();
            SetSpeed(Body.LayeredTime.TimeScale);
        }

        /// <summary>
        /// ギミックの更新
        /// </summary>
        protected override void UpdateInternal(float deltaTime) {
            _gimmickPlayer.Update(deltaTime);
        }

        /// <summary>
        /// ギミックの更新
        /// </summary>
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