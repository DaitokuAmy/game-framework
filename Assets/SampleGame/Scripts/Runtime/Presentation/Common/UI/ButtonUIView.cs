using System;
using GameFramework.Core;
using GameFramework.UISystems;
using TMPro;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace SampleGame.Presentation {
    /// <summary>
    /// ボタン用のUIView
    /// </summary>
    public class ButtonUIView : UIView {
        [Header("シリアライズ")]
        [SerializeField, Tooltip("押下判定用ボタン")]
        private Button _button;
        [SerializeField, Tooltip("表示用テキスト")]
        private TextMeshProUGUI _text;
        
        [Header("アニメーション")]
        [SerializeField, Tooltip("タッチ時アニメーション")]
        private TouchAnimation _touchAnimation;
        
        [Header("色")]
        [SerializeField, Tooltip("色一括変更用グループ")]
        private ColorGroup _colorGroup;
        [SerializeField, Tooltip("無効状態に反映する色")]
        private Color _disableColor;
        [SerializeField, Tooltip("無効状態の時にONにするオブジェクトリスト")]
        private GameObject[] _disableObjects;

        private bool _disableStatus;

        /// <summary>ボタン押下通知</summary>
        public Observable<Unit> ClickedSubject => _button.OnClickAsObservable();
        /// <summary>表示テキスト</summary>
        public string Text {
            get => _text != null ? _text.text : "";
            set {
                if (_text != null) {
                    _text.text = value;
                }
            }
        }
        /// <summary>操作可能か</summary>
        public bool Interactable {
            get => _button != null ? _button.interactable : false;
            set {
                if (_button != null) {
                    _button.interactable = value;
                }
            }
        }
        /// <summary>無効状態</summary>
        public bool DisableStatus {
            get => _disableStatus;
            set {
                if (value == _disableStatus) {
                    return;
                }
                
                SetDisableStatus(value);
            }
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal(IScope scope) {
            if (_touchAnimation != null) {
                _touchAnimation.UpdateType = UpdateType.Manual;
            }

            SetDisableStatus(_disableStatus);
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        protected override void LateUpdateInternal(float deltaTime) {
            if (_touchAnimation != null) {
                _touchAnimation.ManualUpdate(deltaTime);
            }
        }

        /// <summary>
        /// 非アクティブ状態の反映
        /// </summary>
        private void SetDisableStatus(bool status) {
            _disableStatus = status;

            if (_colorGroup != null) {
                if (_disableStatus) {
                    _colorGroup.Color = _disableColor;
                }
                else {
                    _colorGroup.Color = Color.white;
                }
            }

            if (_disableObjects != null) {
                foreach (var obj in _disableObjects) {
                    if (obj == null) {
                        continue;
                    }
                    
                    obj.SetActive(_disableStatus);
                }
            }
        }
    }
}
