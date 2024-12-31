using System;
using System.Collections.Generic;
using System.Linq;
using GameFramework.Core;
using GameFramework.UISystems;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace SampleGame.Presentation {
    /// <summary>
    /// GraphicsRaycastをブロックするUIScreen
    /// </summary>
    public class BlockUIScreen : UIScreen {
        /// <summary>
        /// ブロックのハンドル
        /// </summary>
        public struct Handle : IDisposable {
            private BlockInfo _blockInfo;
            private BlockUIScreen _blockUIScreen;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public Handle(BlockUIScreen blockUIScreen, BlockInfo blockInfo) {
                _blockInfo = blockInfo;
                _blockUIScreen = blockUIScreen;
            }

            /// <summary>
            /// ブロックを止める
            /// </summary>
            public void Dispose() {
                if (_blockUIScreen != null && _blockInfo != null) {
                    _blockUIScreen.DecrementBlockCount(_blockInfo);
                    _blockUIScreen = null;
                    _blockInfo = null;
                }
            }
        }

        /// <summary>
        /// ブロック情報
        /// </summary>
        public class BlockInfo {
            public int BlockOrder;
        }

        [SerializeField, Tooltip("押下検知用ボタン")]
        private Button _button;
        [SerializeField, Tooltip("オーダー変更用")]
        private Canvas _canvas;

        // ブロック情報
        private readonly List<BlockInfo> _blockInfos = new();

        /// <summary>ブロック中のタップ検知</summary>
        public IObservable<Unit> OnClickedSubject => _button != null ? _button.OnClickAsObservable() : Observable.Empty<Unit>();

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal(IScope scope) {
            base.InitializeInternal(scope);
            if (CanvasGroup != null) {
                CanvasGroup.alpha = 0;
                CanvasGroup.interactable = true;
            }

            UpdateBlocking();
        }

        /// <summary>
        /// ブロックを開始する
        /// </summary>
        public Handle StartBlocking(int order = 1000) {
            var blockInfo = IncrementBlockCount(order);
            return new Handle(this, blockInfo);
        }

        /// <summary>
        /// ブロックを止める
        /// </summary>
        public void ClearBlockingAll() {
            _blockInfos.Clear();
            UpdateBlocking();
        }

        /// <summary>
        /// ブロックしている回数を増やす
        /// </summary>
        private BlockInfo IncrementBlockCount(int order) {
            var blockInfo = new BlockInfo {
                BlockOrder = order
            };
            _blockInfos.Add(blockInfo);
            UpdateBlocking();
            return blockInfo;
        }

        /// <summary>
        /// ブロックしている回数を減らす
        /// </summary>
        private void DecrementBlockCount(BlockInfo blockInfo) {
            if (!_blockInfos.Remove(blockInfo)) {
                return;
            }

            UpdateBlocking();
        }

        /// <summary>
        /// ブロック状態を更新する
        /// </summary>
        private void UpdateBlocking() {
            var blocking = _blockInfos.Count > 0;
            if (CanvasGroup != null) {
                CanvasGroup.blocksRaycasts = blocking;
            }

            if (_button != null) {
                _button.enabled = blocking;
            }

            if (_canvas != null && blocking) {
                var order = _blockInfos.Max(x => x.BlockOrder);
                _canvas.sortingOrder = order;
            }
        }
    }
}