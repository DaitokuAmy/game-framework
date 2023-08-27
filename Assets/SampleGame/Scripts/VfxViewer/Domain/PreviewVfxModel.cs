using System;
using GameFramework.ModelSystems;
using UniRx;

namespace SampleGame.VfxViewer {
    /// <summary>
    /// 表示用アクターモデル
    /// </summary>
    public class PreviewVfxModel : AutoIdModel<PreviewVfxModel> {
        private readonly ReactiveProperty<PreviewVfxSetupData> _setupData = new();
        private readonly Subject<PreviewVfxSetupData> _onPlaySubject = new();
        private readonly Subject<Unit> _onStopSubject = new();

        /// <summary>初期化データID</summary>
        public string SetupDataId { get; private set; }

        /// <summary>Actor初期化用データ</summary>
        public IReadOnlyReactiveProperty<PreviewVfxSetupData> SetupData => _setupData;
        /// <summary>再生通知</summary>
        public IObservable<PreviewVfxSetupData> OnPlaySubject => _onPlaySubject;
        /// <summary>停止通知</summary>
        public IObservable<Unit> OnStopSubject => _onStopSubject;

        /// <summary>
        /// 削除時処理
        /// </summary>
        protected override void OnDeletedInternal() {
            _onPlaySubject.Dispose();
            _onStopSubject.Dispose();
            _setupData.Dispose();
            
            base.OnDeletedInternal();
        }

        /// <summary>
        /// データの設定
        /// </summary>
        public void Setup(string setupDataId, PreviewVfxSetupData vfxSetupData) {
            // ID記憶
            SetupDataId = setupDataId;

            // SetupData更新
            _setupData.Value = vfxSetupData;
        }

        /// <summary>
        /// エフェクトの再生
        /// </summary>
        public void Play() {
            if (_setupData.Value == null) {
                return;
            }

            _onPlaySubject.OnNext(_setupData.Value);
        }

        /// <summary>
        /// エフェクトの停止
        /// </summary>
        public void Stop() {
            if (_setupData.Value == null) {
                return;
            }

            _onStopSubject.OnNext(Unit.Default);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private PreviewVfxModel(int id)
            : base(id) {
        }
    }
}