using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework;
using GameFramework.Core;
using R3;
using ThirdPersonEngine;
using UnityEngine;
using UnityEngine.UI;

namespace SampleGame.Presentation {
    /// <summary>
    /// 汎用ダイアログ用の基底スクリーン
    /// </summary>
    public abstract class DialogUIScreen : AnimatableUIScreen {
        [SerializeField, Tooltip("背面タッチ判定用のボタン")]
        private Button _backgroundButton;

        private Subject<int> _selectedSubject = new();

        /// <summary>背面ボタンを使うか</summary>
        public bool UseBackgroundButton { get; set; } = true;
        /// <summary>選択時通知</summary>
        public Observable<int> SelectedSubject => _selectedSubject;

        /// <summary>背面タッチをした時に返却するIndex</summary>
        protected virtual int BackgroundButtonIndex => 0;

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal(IScope scope) {
            base.InitializeInternal(scope);

            _selectedSubject = new Subject<int>();
        }

        /// <summary>
        /// 開始処理
        /// </summary>
        protected override void StartInternal(IScope scope) {
            base.StartInternal(scope);

            // 閉じておく
            CloseAsync(immediate: true);
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DisposeInternal() {
            _selectedSubject.OnCompleted();
            _selectedSubject.Dispose();

            base.DisposeInternal();
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(IScope scope) {
            base.ActivateInternal(scope);

            if (_backgroundButton != null) {
                _backgroundButton.OnClickAsObservable()
                    .TakeUntil(scope)
                    .Where(_ => UseBackgroundButton)
                    .Subscribe(_ => { Select(BackgroundButtonIndex); });
            }
        }

        /// <summary>
        /// ダイアログを開く処理
        /// </summary>
        public async UniTask<int> OpenDialogAsync(CancellationToken ct = default) {
            ct.ThrowIfCancellationRequested();

            // 開く
            await OpenAsync().ToUniTask(cancellationToken: ct);

            // 選択待ち
            var result = -1;
            _selectedSubject
                .TakeUntil(ct.ToScope())
                .Subscribe(x => result = x);

            // 閉じるのを監視
            await UniTask.WaitWhile(() => CurrentOpenStatus == OpenStatus.Opened, cancellationToken: ct);

            // 閉じていない状態だったら閉じる
            if (CurrentOpenStatus == OpenStatus.Opening || CurrentOpenStatus == OpenStatus.Opened) {
                CloseAsync().ToUniTask(cancellationToken: ct).Forget();
            }

            return result;
        }

        /// <summary>
        /// 選択
        /// </summary>
        /// <param name="index">通知用Index</param>
        protected void Select(int index) {
            if (CurrentOpenStatus != OpenStatus.Opened) {
                Debug.LogWarning("Dialogを開いてない状態で結果選択が行われました");
                return;
            }

            _selectedSubject.OnNext(index);
            CloseAsync();
        }
    }
}