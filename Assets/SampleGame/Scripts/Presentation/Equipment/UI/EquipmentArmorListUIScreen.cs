using System.Collections.Generic;
using GameFramework.Core;
using UnityEngine;
using UnityEngine.Pool;

namespace SampleGame.Equipment {
    /// <summary>
    /// 装備画面の防具リスト用UIScreen
    /// </summary>
    public class EquipmentArmorListUIScreen : AnimatableUIScreen {
        [Header("シリアライズ")]
        [SerializeField, Tooltip("防具リスト用のUIViewテンプレート")]
        private ArmorUIView _armorUIViewTemplate;
        [SerializeField, Tooltip("防具詳細情報")]
        private ArmorDetailUIView _armorDetailUIView;

        private ObjectPool<ArmorUIView> _viewPool;
        private List<ArmorUIView> _activatedViews = new();

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal(IScope scope) {
            base.InitializeInternal(scope);

            _viewPool = new ObjectPool<ArmorUIView>(() => {
                    _armorUIViewTemplate.gameObject.SetActive(true);
                    var view = InstantiateView(_armorUIViewTemplate, _armorUIViewTemplate.transform.parent);
                    _armorUIViewTemplate.gameObject.SetActive(false);
                    view.gameObject.SetActive(false);
                    return view;
                },
                view => {
                    view.gameObject.SetActive(true);
                    _activatedViews.Add(view);
                },
                view => {
                    view.gameObject.SetActive(false);
                    _activatedViews.Remove(view);
                }, view => { Destroy(view.gameObject); });
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DisposeInternal() {
            Cleanup();
            _viewPool.Dispose();

            base.DisposeInternal();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Setup(int armorCount) {
            Cleanup();
            
            for (var i = 0; i < armorCount; i++) {
                _viewPool.Get();
            }
        }

        /// <summary>
        /// 解放処理
        /// </summary>
        private void Cleanup() {
            foreach (var view in _activatedViews.ToArray()) {
                _viewPool.Release(view);
            }
        }
    }
}