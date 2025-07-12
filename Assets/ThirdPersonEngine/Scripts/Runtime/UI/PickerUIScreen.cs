using System;
using System.Collections.Generic;
using System.Linq;
using GameFramework.Core;
using GameFramework.UISystems;
using UnityEngine;
using UnityEngine.Pool;

namespace ThirdPersonEngine {
    /// <summary>
    /// RectTransformPick用のUIScreen
    /// </summary>
    public class PickerUIScreen : UIScreen {
        /// <summary>
        /// ターゲット情報
        /// </summary>
        private struct TargetInfo {
            public RectTransform Target;
            public RectTransform Dummy;
        }

        [SerializeField, Tooltip("ピックアップ中にアクティブ化するObject")]
        private GameObject _activeObject;
        [SerializeField, Tooltip("ピックアップするターゲットを配置するRoot")]
        private RectTransform _targetRoot;
        [SerializeField, Tooltip("開く際のアニメーション")]
        private UIAnimationComponent _openAnimation;
        [SerializeField, Tooltip("閉じる際のアニメーション")]
        private UIAnimationComponent _closeAnimation;

        private ObjectPool<RectTransform> _dummyTransformPool;
        private Transform _dummyRoot;
        private Dictionary<RectTransform, TargetInfo> _activeTargetInfos = new();
        private UIAnimationPlayer _animationPlayer;
        
        /// <summary>オブジェクトをPickUpした状態か</summary>
        public bool IsPicking { get; private set; }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal(IScope scope) {
            base.InitializeInternal(scope);

            var dummyRootObj = new GameObject("DummyRoot", typeof(RectTransform));
            dummyRootObj.transform.SetParent(transform, false);
            _dummyRoot = dummyRootObj.transform;

            _dummyTransformPool = new ObjectPool<RectTransform>(() => {
                var obj = new GameObject("Blank", typeof(RectTransform));
                obj.transform.SetParent(_dummyRoot, false);
                obj.SetActive(false);
                return (RectTransform)obj.transform;
            }, trans => { trans.gameObject.SetActive(true); }, trans => {
                trans.SetParent(_dummyRoot, false);
                trans.gameObject.SetActive(false);
            }, trans => {
                Destroy(trans.gameObject);
            });

            _animationPlayer = new UIAnimationPlayer();

            if (_closeAnimation != null) {
                _animationPlayer.Skip(_closeAnimation);
            }

            if (_activeObject != null) {
                _activeObject.SetActive(false);
            }

            IsPicking = false;
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        protected override void DisposeInternal() {
            UnPickInternal();
            _activeTargetInfos.Clear();
            _dummyTransformPool.Dispose();

            base.DisposeInternal();
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        protected override void LateUpdateInternal(float deltaTime) {
            base.LateUpdateInternal(deltaTime);
            
            _animationPlayer.Update(deltaTime);
        }

        /// <summary>
        /// RectTransformをPickerに設定
        /// </summary>
        /// <param name="targets">Picker内に配置するターゲットのRectTransform</param>
        public void PickUp(params RectTransform[] targets) {
            // 現在の物をReleaseする
            UnPickInternal();
            
            foreach (var target in targets) {
                if (target == null) {
                    continue;
                }
                
                // 該当のRectTransformのダミーを作成
                CreateDummy(target);
            }
            
            if (!IsPicking) {
                // 開くアニメーション再生
                if (_openAnimation != null) {
                    _animationPlayer.Play(_openAnimation);
                }
                
                // アクティブオブジェクトをON
                if (_activeObject != null) {
                    _activeObject.SetActive(true);
                }
            }

            IsPicking = true;
        }

        /// <summary>
        /// PickUp済みのRectTransformをリリースする
        /// </summary>
        public void UnPick() {
            if (!IsPicking) {
                return;
            }
            
            // Targetのリリース
            UnPickInternal();
            
            // 閉じるアニメーション再生
            if (_closeAnimation != null) {
                _animationPlayer.Play(_closeAnimation);
            }
            
            // アクティブオブジェクトのOFF
            if (_activeObject != null) {
                _activeObject.SetActive(false);
            }

            IsPicking = false;
        }

        /// <summary>
        /// 内部用のRelease処理
        /// </summary>
        private void UnPickInternal() {
            var targets = _activeTargetInfos.Keys.ToArray();
            foreach (var target in targets) {
                if (target == null) {
                    continue;
                }
                
                // ダミーを解放
                ReleaseDummy(target);
            }
        }

        /// <summary>
        /// ダミーの生成
        /// </summary>
        private void CreateDummy(RectTransform targetTrans) {
            if (targetTrans == null) {
                return;
            }
            
            var dummy = _dummyTransformPool.Get();
            CopyRectTransform(targetTrans, dummy);
            targetTrans.SetParent(_targetRoot, true);
            _activeTargetInfos.Add(targetTrans, new TargetInfo {
                Target = targetTrans,
                Dummy = dummy
            });
        }

        /// <summary>
        /// ダミーの解放
        /// </summary>
        private void ReleaseDummy(RectTransform targetTrans) {
            if (targetTrans == null) {
                return;
            }
            
            if (!_activeTargetInfos.TryGetValue(targetTrans, out var info)) {
                return;
            }
            
            CopyRectTransform(info.Dummy, info.Target);
            _dummyTransformPool.Release(info.Dummy);
            _activeTargetInfos.Remove(targetTrans);
        }

        /// <summary>
        /// RectTransformのパラメータをコピーする
        /// </summary>
        private void CopyRectTransform(RectTransform source, RectTransform destination) {
            if (source == null || destination == null) {
                return;
            }
            
            destination.SetParent(source.parent, false);
            destination.anchorMin = source.anchorMin;
            destination.anchorMax = source.anchorMax;
            destination.pivot = source.pivot;
            destination.anchoredPosition = source.anchoredPosition;
            destination.sizeDelta = source.sizeDelta;
            destination.SetSiblingIndex(source.GetSiblingIndex());
        }
    }
}