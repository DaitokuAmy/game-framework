using GameFramework.PlayableSystems;
using UnityEngine;
using UnityEngine.Playables;

namespace GameFramework.BodySystems {
    /// <summary>
    /// モーション制御用クラス
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class MotionController : SerializedBodyController {
        [SerializeField, Tooltip("モーション更新モード")]
        private DirectorUpdateMode _updateMode = DirectorUpdateMode.GameTime;

        // ルートスケール制御用
        private RootAnimationJobProvider _rootAnimationJobProvider;
        // モーション再生用クラス
        private MotionPlayer _player;

        // Animator
        public Animator Animator { get; private set; }
        // モーション再生レイヤーハンドル
        public MotionHandle Handle => _player.Handle;

        // ルートスケール（座標）
        public Vector3 RootPositionScale {
            get => _rootAnimationJobProvider.PositionScale;
            set => _rootAnimationJobProvider.PositionScale = value;
        }
        // ルート速度オフセット
        public Vector3 RootVelocityOffset {
            get => _rootAnimationJobProvider.VelocityOffset;
            set => _rootAnimationJobProvider.VelocityOffset = value;
        }
        // ルートスケール（回転）
        public Vector3 RootAngleScale {
            get => _rootAnimationJobProvider.AngleScale;
            set => _rootAnimationJobProvider.AngleScale = value;
        }
        // ルート角速度オフセット
        public Vector3 RootAngularVelocityOffset {
            get => _rootAnimationJobProvider.AngularVelocityOffset;
            set => _rootAnimationJobProvider.AngularVelocityOffset = value;
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal() {
            Animator = Body.GetComponent<Animator>();
            _player = new MotionPlayer(Animator, _updateMode);

            // RootScaleJobの初期化
            _rootAnimationJobProvider = new RootAnimationJobProvider();
            _player.JobConnector.SetProvider(_rootAnimationJobProvider);

            // TimeScale監視
            Body.LayeredTime.OnChangedTimeScale += scale => { _player.SetSpeed(scale); };
            _player.SetSpeed(Body.LayeredTime.TimeScale);
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal(float deltaTime) {
            _player.Update();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DisposeInternal() {
            _player.Dispose();
        }

        /// <summary>
        /// 更新モードの変更
        /// </summary>
        public void SetUpdateMode(DirectorUpdateMode updateMode) {
            _player.SetUpdateMode(updateMode);
        }

        /// <summary>
        /// 再生速度の設定
        /// </summary>
        public void SetSpeed(float speed) {
            _player.SetSpeed(speed);
        }

        /// <summary>
        /// 再生対象のPlayableProviderを変更
        /// </summary>
        /// <param name="component">変更対象のPlayableを返すProvider</param>
        /// <param name="blendDuration">ブレンド時間</param>
        public void Change(IPlayableComponent component, float blendDuration = 0.0f) {
            _player.Handle.Change(component, blendDuration);
        }

        /// <summary>
        /// LayerのWeight設定
        /// </summary>
        /// <param name="weight">再生ウェイト</param>
        public void SetWeight(float weight) {
            _player.Handle.SetWeight(weight);
        }

        /// <summary>
        /// 拡張レイヤーの追加
        /// </summary>
        /// <param name="additive">加算モードか</param>
        /// <param name="avatarMask">アバターマスク</param>
        /// <param name="weight">初期設定のWeight</param>
        public MotionHandle AddExtensionLayer(bool additive = false, AvatarMask avatarMask = null, float weight = 1.0f) {
            return _player.RootComponent.AddExtensionLayer(additive, avatarMask, weight);
        }

        /// <summary>
        /// 拡張レイヤーの削除
        /// </summary>
        /// <param name="handle">対象を表すハンドル</param>
        public void RemoveExtensionLayer(MotionHandle handle) {
            _player.RootComponent.RemoveExtensionLayer(handle);
        }

        /// <summary>
        /// 拡張レイヤーの削除
        /// </summary>
        public void RemoveExtensionLayers() {
            _player.RootComponent.RemoveExtensionLayers();
        }

        /// <summary>
        /// 値変化通知
        /// </summary>
        private void OnValidate() {
            _player?.SetUpdateMode(_updateMode);
        }
    }
}