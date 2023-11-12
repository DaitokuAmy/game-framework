using System.Collections.Generic;
using GameFramework.Core;
using GameFramework.UISystems;
using UnityEngine;
using UnityEngine.Pool;

namespace SampleGame.Equipment {
    /// <summary>
    /// 装備画面の武器リスト用UIScreen
    /// </summary>
    public class EquipmentWeaponListUIScreen : UIScreen {
        [SerializeField, Tooltip("武器リスト用のUIViewテンプレート")]
        private WeaponUIView _weaponUIViewTemplate;
        [SerializeField, Tooltip("武器詳細情報")]
        private WeaponDetailUIView _weaponDetailUIView;

        private ObjectPool<WeaponUIView> _viewPool;
        private List<WeaponUIView> _activatedViews = new();

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal(IScope scope) {
            base.InitializeInternal(scope);

            _viewPool = new ObjectPool<WeaponUIView>(() => {
                    _weaponUIViewTemplate.gameObject.SetActive(true);
                    var view = InstantiateView(_weaponUIViewTemplate, _weaponUIViewTemplate.transform.parent);
                    _weaponUIViewTemplate.gameObject.SetActive(false);
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
        public void Setup(int weaponCount) {
            Cleanup();
            
            for (var i = 0; i < weaponCount; i++) {
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