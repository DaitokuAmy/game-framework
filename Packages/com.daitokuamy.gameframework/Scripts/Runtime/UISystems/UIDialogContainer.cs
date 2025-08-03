using System;
using System.Collections.Generic;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.UISystems {
    public struct DialogHandle<TResult> {
    }

    /// <summary>
    /// Dialogコンテナクラス（未実装）
    /// </summary>
    public abstract class UIDialogContainer : UIScreen {
        /// <summary>
        /// テンプレート情報
        /// </summary>
        [Serializable]
        protected class TemplateInfo {
            [Tooltip("ダイアログを表すキー")]
            public string key;
            [Tooltip("複製用テンプレート")]
            public UIScreen template;
        }

        [SerializeField, Tooltip("テンプレート情報リスト")]
        private List<TemplateInfo> _templateInfos = new();

        private readonly Dictionary<string, TemplateInfo> _templateInfoMap = new();
        private readonly Dictionary<string, UIViewPool<UIScreen>> _dialogViewPools = new();

        /// <inheritdoc/>
        protected override void InitializeInternal(IScope scope) {
            base.InitializeInternal(scope);

            // テンプレートマップの初期化
            foreach (var info in _templateInfos) {
                if (string.IsNullOrEmpty(info.key) || info.template == null) {
                    continue;
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

        /// <summary>
        /// ダイアログを開く処理
        /// </summary>
        public DialogHandle<TResult> OpenDialog<TScreen, TResult>(string key, Action<TScreen> setupAction = null)
            where TScreen : UIScreen {
            if (!_dialogViewPools.TryGetValue(key, out var pool)) {
                // todo:Empty
                return default;
            }

            // Poolから取得して初期化
            var screen = pool.Add();
            if (screen is TScreen s) {
                setupAction?.Invoke(s);
            }
            
            // 描画順のコントロール
            screen.transform.SetAsLastSibling();
            
            // todo: ダイアログを開いて閉じるまで監視
            screen.OpenAsync();

            return default;
        }

        /// <summary>
        /// 開いているダイログを一階層閉じる
        /// </summary>
        public bool BackDialog() {
            return true;
        }
    }
}