using System;
using System.Collections.Generic;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.UISystems {
    /// <summary>
    /// Dialogコンテナクラス
    /// </summary>
    public sealed class UIDialogContainer : UIScreen {
        /// <summary>
        /// アクティブ中ダイアログ情報
        /// </summary>
        internal class DialogInfo : IDisposable {
            private readonly Action<DialogInfo> _selectedIndexAction;

            /// <summary>設定されているDialog</summary>
            public IDialog Dialog { get; private set; }
            /// <summary>選択結果</summary>
            public int Result { get; private set; }
            /// <summary>完了済みか</summary>
            public bool IsDone { get; private set; }
            /// <summary>バック可能か</summary>
            public bool CanBack { get; private set; }

            /// <summary>Index選択通知</summary>
            public event Action<int> SelectedIndexEvent {
                add {
                    if (Dialog != null) {
                        Dialog.SelectedIndexEvent += value;
                    }
                }
                remove {
                    if (Dialog != null) {
                        Dialog.SelectedIndexEvent -= value;
                    }
                }
            }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public DialogInfo(Action<DialogInfo> selectedIndexAction, IDialog dialog, bool canBack) {
                _selectedIndexAction = selectedIndexAction;
                Dialog = dialog;
                Result = -1;
                CanBack = canBack;
                IsDone = false;

                Dialog.SelectedIndexEvent += OnSelectedIndex;
            }

            /// <summary>
            /// 廃棄時処理
            /// </summary>
            public void Dispose() {
                if (IsDone) {
                    return;
                }

                IsDone = true;
            }

            /// <summary>
            /// 選択時処理
            /// </summary>
            protected virtual void OnSelectedIndex(int index) {
                if (IsDone) {
                    return;
                }

                Result = index;
                IsDone = true;
                Dialog.SelectedIndexEvent -= OnSelectedIndex;
                _selectedIndexAction?.Invoke(this);
            }
        }

        /// <summary>
        /// アクティブ中ダイアログ情報
        /// </summary>
        internal class DialogInfo<TResult> : DialogInfo {
            private Action<TResult> _selectedAction;

            /// <summary>結果通知</summary>
            public event Action<TResult> SelectedEvent {
                add => _selectedAction += value;
                remove => _selectedAction -= value;
            }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public DialogInfo(Action<DialogInfo> selectedIndexAction, IDialog dialog, bool canBack)
                : base(selectedIndexAction, dialog, canBack) {
                _selectedAction = null;
            }

            /// <summary>
            /// 選択時処理
            /// </summary>
            protected override void OnSelectedIndex(int index) {
                base.OnSelectedIndex(index);

                if (Dialog is IDialog<TResult> dialog) {
                    _selectedAction?.Invoke(dialog.GetResult(index));
                }
                else {
                    _selectedAction?.Invoke(default);
                }
            }

            /// <summary>
            /// 結果の取得
            /// </summary>
            public TResult GetResult() {
                if (Dialog is IDialog<TResult> dialog) {
                    return dialog.GetResult(Result);
                }

                return default;
            }
        }

        /// <summary>
        /// 生成中のScreen情報管理用
        /// </summary>
        private class ScreenInfo {
            public string Key;
            public UIScreen Screen;
            public IUIScreenHandler Handler;
            public DialogInfo DialogInfo;
        }

        /// <summary>
        /// テンプレート情報
        /// </summary>
        [Serializable]
        private class TemplateInfo {
            [Tooltip("ダイアログを表すキー")]
            public string key;
            [Tooltip("複製用テンプレート")]
            public UIScreen template;
        }

        [SerializeField, Tooltip("テンプレート情報リスト")]
        private List<TemplateInfo> _templateInfos = new();

        private readonly Dictionary<string, TemplateInfo> _templateInfoMap = new();
        private readonly Dictionary<string, UIViewPool<UIScreen>> _dialogViewPools = new();
        private readonly List<DialogInfo> _activeDialogInfos = new();
        private readonly Dictionary<string, Func<IUIScreenHandler>> _createHandlerFunctions = new();
        private readonly List<ScreenInfo> _activeScreenInfos = new();

        /// <inheritdoc/>
        protected override void InitializeInternal(IScope scope) {
            base.InitializeInternal(scope);

            // テンプレートマップの初期化
            foreach (var info in _templateInfos) {
                if (string.IsNullOrEmpty(info.key) || info.template == null) {
                    continue;
                }

                if (info.template is not IDialog) {
                    throw new Exception($"Template is not a dialog. key:{info.key} value:{info.template.name}");
                }

                _templateInfoMap.TryAdd(info.key, info);
            }

            // プールの初期化
            foreach (var pair in _templateInfoMap) {
                var key = pair.Key;
                var info = pair.Value;

                var pool = new UIViewPool<UIScreen>(info.template, template => InstantiateView(template, template.transform.parent))
                    .RegisterTo(scope);
                _dialogViewPools.Add(key, pool);
            }
        }

        /// <inheritdoc/>
        protected override void DisposeInternal() {
            // アクティブ情報を全部削除
            for (var i = _activeDialogInfos.Count - 1; i >= 0; i--) {
                var info = _activeDialogInfos[i];
                info.Dispose();
                _activeDialogInfos.RemoveAt(i);
            }

            // Poolに返却
            for (var i = _activeScreenInfos.Count - 1; i >= 0; i--) {
                var screenInfo = _activeScreenInfos[i];
                if (screenInfo.Handler != null) {
                    screenInfo.Screen.UnregisterHandler(screenInfo.Handler);
                }

                var pool = _dialogViewPools[screenInfo.Key];
                pool.Release(screenInfo.Screen);
                _activeScreenInfos.RemoveAt(i);
            }

            base.DisposeInternal();
        }

        /// <inheritdoc/>
        protected override void LateUpdateInternal(float deltaTime) {
            base.LateUpdateInternal(deltaTime);

            // 閉じ終わったScreenをPoolに回収
            for (var i = _activeScreenInfos.Count - 1; i >= 0; i--) {
                var screenInfo = _activeScreenInfos[i];
                if (!screenInfo.DialogInfo.IsDone) {
                    continue;
                }

                // ステータスが閉じていたらプールに戻す
                if (screenInfo.Screen.CurrentOpenStatus == OpenStatus.Closed) {
                    if (screenInfo.Handler != null) {
                        screenInfo.Screen.UnregisterHandler(screenInfo.Handler);
                    }

                    var pool = _dialogViewPools[screenInfo.Key];
                    pool.Release(screenInfo.Screen);
                    _activeScreenInfos.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// ダイアログを開く処理(戻り値が明示的な場合)
        /// </summary>
        public DialogHandle<TResult> OpenDialog<TScreen, TResult>(string key, Action<TScreen> setupAction = null, bool canBack = true)
            where TScreen : UIScreen, IDialog<TResult> {
            var dialogInfo = OpenDialogInternal(
                key,
                (d, b) => new DialogInfo<TResult>(OnSelectedIndex, d, b),
                setupAction, canBack);
            if (dialogInfo == null) {
                return DialogHandle<TResult>.Empty;
            }

            return new DialogHandle<TResult>((DialogInfo<TResult>)dialogInfo);
        }

        /// <summary>
        /// ダイアログを開く処理(戻り値が選択Indexの場合)
        /// </summary>
        public DialogHandle OpenDialog<TScreen>(string key, Action<TScreen> setupAction = null, bool canBack = true)
            where TScreen : UIScreen, IDialog {
            var dialogInfo = OpenDialogInternal(
                key,
                (d, b) => new DialogInfo(OnSelectedIndex, d, b),
                setupAction, canBack);
            if (dialogInfo == null) {
                return DialogHandle.Empty;
            }

            return new DialogHandle(dialogInfo);
        }

        /// <summary>
        /// 開いているダイログを一階層閉じる
        /// </summary>
        public bool BackDialog() {
            // 戻る物がない
            if (_activeDialogInfos.Count <= 0) {
                return false;
            }

            var currentInfo = _activeDialogInfos[^1];
            // 戻りを無効化されている場合はtrueだけ返す
            if (!currentInfo.CanBack) {
                return true;
            }

            // キャンセルクローズする
            currentInfo.Dialog.Cancel();
            return true;
        }

        /// <summary>
        /// ハンドラーの設定
        /// </summary>
        public void SetHandler(string key, Func<IUIScreenHandler> createHandlerFunc) {
            ResetHandler(key);

            _createHandlerFunctions[key] = createHandlerFunc;

            // 現在対象になるハンドラーを設定
            foreach (var info in _activeScreenInfos) {
                if (info.Key != key) {
                    continue;
                }

                info.Handler = createHandlerFunc();
                info.Screen.RegisterHandler(info.Handler);
            }
        }

        /// <summary>
        /// ハンドラーのリセット
        /// </summary>
        public void ResetHandler(string key) {
            if (!_createHandlerFunctions.ContainsKey(key)) {
                return;
            }

            // 現在使用されているハンドラーを削除
            foreach (var info in _activeScreenInfos) {
                if (info.Key != key) {
                    continue;
                }

                info.Screen.UnregisterHandler(info.Handler);
                info.Handler = null;
            }
        }

        /// <summary>
        /// 内部用ダイアログを開く処理
        /// </summary>
        private DialogInfo OpenDialogInternal<TScreen>(string key, Func<IDialog, bool, DialogInfo> createDialogInfoFunc, Action<TScreen> setupAction, bool canBack)
            where TScreen : UIScreen, IDialog {
            if (!_dialogViewPools.TryGetValue(key, out var pool)) {
                DebugLog.Warning($"Not found dialog key. [{key}]");
                return null;
            }

            // Poolから取得して初期化
            var screen = pool.Get();
            if (screen is TScreen s) {
                setupAction?.Invoke(s);
            }

            // 現在開いている物があれば閉じる
            if (_activeDialogInfos.Count > 0) {
                var currentInfo = _activeDialogInfos[^1];
                ((UIScreen)currentInfo.Dialog).CloseAsync(TransitionDirection.Back);
            }

            // Handlerの生成と登録
            var handler = default(IUIScreenHandler);
            if (_createHandlerFunctions.TryGetValue(key, out var func)) {
                handler = func();
                screen.RegisterHandler(handler);
            }

            // ダイアログ情報の作成
            var dialogInfo = createDialogInfoFunc((IDialog)screen, canBack);
            _activeDialogInfos.Add(dialogInfo);

            // アクティブスクリーン情報に追加
            var screenInfo = new ScreenInfo {
                Key = key,
                Screen = screen,
                Handler = handler,
                DialogInfo = dialogInfo
            };
            _activeScreenInfos.Add(screenInfo);

            // 描画順のコントロール
            screen.transform.SetAsLastSibling();

            // 開く
            screen.OpenAsync();

            return dialogInfo;
        }

        /// <summary>
        /// Dialogが選択された時の通知
        /// </summary>
        private void OnSelectedIndex(DialogInfo info) {
            // 対象のDialogを閉じてアクティブダイアログを更新
            var screen = (UIScreen)info.Dialog;
            screen.CloseAsync(TransitionDirection.Back);

            // アクティブなダイアログ情報から除外
            _activeDialogInfos.Remove(info);
            info.Dispose();

            // スタックされたDialog情報があれば、そのDialogを開く
            if (_activeDialogInfos.Count > 0) {
                var currentInfo = _activeDialogInfos[^1];
                var currentScreen = (UIScreen)currentInfo.Dialog;
                currentScreen.OpenAsync();
            }
        }
    }
}