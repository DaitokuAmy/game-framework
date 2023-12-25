using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace GameFramework.UISystems {
    /// <summary>
    /// 色一括変更
    /// </summary>
    public class ColorGroup : MonoBehaviour {
        /// <summary>
        /// ターゲット情報
        /// </summary>
        private class TargetInfo {
            public Graphic target;
            public Color baseColor;
        }

        [SerializeField, Tooltip("変更対象から除外するGraphic")]
        private Graphic[] _ignoreTargets;

        private TargetInfo[] _targetInfos;
        private Color _color;

        /// <summary>設定する色</summary>
        public Color Color {
            get => _color;
            set => SetColor(value);
        }

        /// <summary>
        /// 状態のリセット
        /// </summary>
        public void Reset() {
            ResetTargets();
        }

        /// <summary>
        /// 色の設定
        /// </summary>
        private void SetColor(Color color) {
            _color = color;
            
            if (_targetInfos == null) {
                RefreshTargets();
            }
            
            SetTargetColors(color);
        }

        /// <summary>
        /// ターゲット情報の取得
        /// </summary>
        private void RefreshTargets() {
            ResetTargets();

            _targetInfos = GetComponentsInChildren<Graphic>(true)
                .Where(x => !_ignoreTargets.Contains(x))
                .Select(x => new TargetInfo {
                    target = x,
                    baseColor = x.color,
                })
                .ToArray();
        }

        /// <summary>
        /// ターゲットのリセット
        /// </summary>
        private void ResetTargets() {
            if (_targetInfos == null) {
                return;
            }

            // 色情報を戻す
            for (var i = 0; i < _targetInfos.Length; i++) {
                if (_targetInfos[i].target == null) {
                    continue;
                }

                _targetInfos[i].target.color = _targetInfos[i].baseColor;
            }

            // 要素をクリア
            _targetInfos = Array.Empty<TargetInfo>();
        }

        /// <summary>
        /// 色の設定
        /// </summary>
        private void SetTargetColors(Color color) {
            if (_targetInfos == null) {
                return;
            }

            // 色情報を変更
            for (var i = 0; i < _targetInfos.Length; i++) {
                if (_targetInfos[i].target == null) {
                    continue;
                }

                _targetInfos[i].target.color = _targetInfos[i].baseColor * color;
            }
        }
    }
}