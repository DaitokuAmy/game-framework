using GameFramework.PlayableSystem;
using UnityEngine;
using UnityEngine.Playables;

namespace GameFramework.ActorSystem {
    /// <summary>
    /// モーション制御用クラス
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public sealed class MotionComponent : SerializedBodyComponent {
        [SerializeField, Tooltip("モーション更新モード")]
        private DirectorUpdateMode _updateMode = DirectorUpdateMode.GameTime;

        // ルートスケール制御用
        private RootAnimationJobComponent _rootAnimationJobComponent;
        // 腰高さ調整用
        private AdjustHeightAnimationJobComponent _adjustHeightAnimationJobComponent;
        // モーション再生用クラス
        private MotionPlayer _player;
        // 内部的に保持する速度
        private float _localSpeed = 1.0f;

        /// <summary>制御に使用しているAnimator</summary>
        public Animator Animator { get; private set; }
        /// <summary>モーション再生ハンドル</summary>
        public MotionHandle Handle => _player.Handle;

        /// <summary>ルートスケール（座標）</summary>
        public Vector3 RootPositionScale {
            get => _rootAnimationJobComponent.PositionScale;
            set => _rootAnimationJobComponent.PositionScale = value;
        }
        /// <summary>ルート速度オフセット</summary>
        public Vector3 RootVelocityOffset {
            get => _rootAnimationJobComponent.VelocityOffset;
            set => _rootAnimationJobComponent.VelocityOffset = value;
        }
        /// <summary>ルートスケール（回転）</summary>
        public Vector3 RootAngleScale {
            get => _rootAnimationJobComponent.AngleScale;
            set => _rootAnimationJobComponent.AngleScale = value;
        }
        /// <summary>ルート角速度オフセット</summary>
        public Vector3 RootAngularVelocityOffset {
            get => _rootAnimationJobComponent.AngularVelocityOffset;
            set => _rootAnimationJobComponent.AngularVelocityOffset = value;
        }
        /// <summary>腰高さスケール(BoneControllerの指定がないと無効)</summary>
        public float HeightScale {
            get => _adjustHeightAnimationJobComponent?.HeightScale ?? 1.0f;
            set {
                if (_adjustHeightAnimationJobComponent == null) {
                    return;
                }

                _adjustHeightAnimationJobComponent.HeightScale = value;
            }
        }

        /// <inheritdoc/>
        protected override void InitializeInternal(IScope scope) {
            Animator = Body.GetComponent<Animator>();
            _player = new MotionPlayer(Animator, _updateMode);

            // RootAnimationJobの初期化
            _rootAnimationJobComponent = new RootAnimationJobComponent();
            _player.JobConnector.AddComponent(_rootAnimationJobComponent);

            // BoneControllerがある場合、高さ調整用のJobComponentを追加
            var boneController = Body.GetBodyComponent<BoneComponent>();
            if (boneController != null) {
                if (boneController.Root != null && boneController.Hips != null) {
                    _adjustHeightAnimationJobComponent = new AdjustHeightAnimationJobComponent(boneController.Root, boneController.Hips);
                    _player.JobConnector.AddComponent(_adjustHeightAnimationJobComponent, 1);
                }
            }

            // TimeScale監視
            Body.LayeredTime.ChangedTimeScaleEvent += OnChangedTimeScale;
            ApplySpeed();
        }

        /// <inheritdoc/>
        protected override void UpdateInternal(float deltaTime) {
            _player.Update();
        }

        /// <inheritdoc/>
        protected override void DisposeInternal() {
            Body.LayeredTime.ChangedTimeScaleEvent -= OnChangedTimeScale;
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
            _localSpeed = Mathf.Max(0, speed);
            ApplySpeed();
        }

        /// <summary>
        /// Playableの変更
        /// </summary>
        /// <param name="playable">再生するPlayable</param>
        /// <param name="blendDuration">ブレンド時間</param>
        /// <param name="autoDispose">再生終了時に自動廃棄するか</param>
        public void Change(Playable? playable, float blendDuration, bool autoDispose) {
            _player.Handle.Change(playable, blendDuration, autoDispose);
        }

        /// <summary>
        /// 拡張レイヤーの生成
        /// </summary>
        /// <param name="additive">加算レイヤーか</param>
        /// <param name="avatarMask">アバターマスク</param>
        /// <param name="weight">初期ウェイト</param>
        public MotionHandle CreateExtensionLayer(bool additive = false, AvatarMask avatarMask = null, float weight = 1.0f) {
            return _player.CreateExtensionLayer(additive, avatarMask, weight);
        }

        /// <summary>
        /// 拡張レイヤーの取得
        /// </summary>
        /// <param name="index">拡張レイヤーのIndex</param>
        public MotionHandle GetExtensionLayer(int index) {
            return _player.GetExtensionLayer(index);
        }

        /// <summary>
        /// 拡張レイヤーの削除
        /// </summary>
        /// <param name="handle">対象を表すハンドル</param>
        public void RemoveExtensionLayer(MotionHandle handle) {
            _player.RemoveExtensionLayer(handle);
        }

        /// <summary>
        /// 拡張レイヤーの削除
        /// </summary>
        public void RemoveExtensionLayers() {
            _player.RemoveExtensionLayers();
        }

        /// <summary>
        /// 値変化通知
        /// </summary>
        private void OnValidate() {
            _player?.SetUpdateMode(_updateMode);
        }

        /// <summary>
        /// 速度の反映
        /// </summary>
        private void ApplySpeed() {
            _player.SetSpeed(_localSpeed * Body.LayeredTime.TimeScale);
        }

        /// <summary>
        /// 時間の変更監視
        /// </summary>
        private void OnChangedTimeScale(float timeScale) {
            ApplySpeed();
        }
    }
}
