using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.Core;
using GameFramework.UISystems;
using UnityEngine;

namespace SampleGame.Equipment {
    /// <summary>
    /// 装備変更画面用のUIWindow
    /// </summary>
    public class EquipmentUIWindow : UIWindow {
        [SerializeField, Tooltip("戻るボタン")]
        private ButtonUIView _backButton;
        [SerializeField, Tooltip("ページ切り替え用コンテナ")]
        private UIPageContainer _pageContainer;

        /// <summary>戻るボタン</summary>
        public ButtonUIView BackButton => _backButton;

        /// <summary>
        /// Top画面に遷移
        /// </summary>
        public async UniTask TransitionTopAsync(CancellationToken ct) {
            await _pageContainer.Transition("Top")
                .ToUniTask(cancellationToken:ct);
        }

        /// <summary>
        /// WeaponList画面に遷移
        /// </summary>
        public async UniTask TransitionWeaponListAsync(CancellationToken ct) {
            await _pageContainer.Transition("WeaponList")
                .ToUniTask(cancellationToken:ct);
        }

        /// <summary>
        /// ArmorList画面に遷移
        /// </summary>
        public async UniTask TransitionArmorListAsync(CancellationToken ct) {
            await _pageContainer.Transition("ArmorList")
                .ToUniTask(cancellationToken:ct);
        }

        /// <summary>
        /// 画面を戻す
        /// </summary
        public async UniTask BackAsync(CancellationToken ct) {
            await _pageContainer.Back()
                .ToUniTask(cancellationToken: ct);
        }
    }
}