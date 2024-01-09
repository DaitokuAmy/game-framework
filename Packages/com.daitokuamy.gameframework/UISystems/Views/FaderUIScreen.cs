using System;
using System.Collections.Generic;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.UISystems {
    /// <summary>
    /// Fader用のスクリーン
    /// </summary>
    public abstract class FaderUIScreen : UIScreen {
        /// <summary>
        /// フェーダー情報
        /// </summary>
        [Serializable]
        private class FaderInfo {
            [Tooltip("制御用ラベル")]
            public string label;
            [Tooltip("制御対象のView")]
            public FaderUIView view;
        }

        [SerializeField, Tooltip("フェーダー情報リスト")]
        private FaderInfo[] _faderInfos;

        private Dictionary<string, FaderUIView> _faders;
        private FaderUIView _currentFader;

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal(IScope scope) {
            base.InitializeInternal(scope);

            _faders = new Dictionary<string, FaderUIView>();
            foreach (var info in _faderInfos) {
                _faders[info.label] = info.view;
            }
        }

        /// <summary>
        /// フェードイン
        /// </summary>
        /// <param name="label">フェーダーラベル</param>
        /// <param name="duration">フェードイン時間</param>
        public AsyncOperationHandle FadeInAsync(string label, float duration) {
            if (!_faders.TryGetValue(label, out var fader)) {
                return AsyncOperationHandle.CanceledHandle;
            }
            
            // 違うFaderだった場合、即完了させて入れ替える
            if (_currentFader != null && _currentFader != fader) {
                var color = _currentFader.Color;
                _currentFader.FadeInImmediate();
                fader.FadeOutImmediate(color);
            }
            
            _currentFader = fader;
            return fader.FadeInAsync(duration);
        }

        /// <summary>
        /// フェードアウト
        /// </summary>
        /// <param name="label">フェーダーラベル</param>
        /// <param name="color">フェードアウト色</param>
        /// <param name="duration">フェードイン時間</param>
        public AsyncOperationHandle FadeOutAsync(string label, Color color, float duration) {
            if (!_faders.TryGetValue(label, out var fader)) {
                return AsyncOperationHandle.CanceledHandle;
            }

            // 違うFaderだった場合、即完了させる
            if (_currentFader != null && _currentFader != fader) {
                _currentFader.FadeInImmediate();
            }

            _currentFader = fader;
            return fader.FadeOutAsync(color, duration);
        }
    }
}
