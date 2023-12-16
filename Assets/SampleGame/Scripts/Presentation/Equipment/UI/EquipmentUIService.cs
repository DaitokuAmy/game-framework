using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.Core;
using GameFramework.UISystems;
using UnityEngine;

namespace SampleGame.Equipment {
    /// <summary>
    /// 装備変更画面用のUIService
    /// </summary>
    public class EquipmentUIService : UIService {
        [SerializeField, Tooltip("戻るボタン")]
        private ButtonUIView _backButton;
        [SerializeField, Tooltip("シート切り替え用コンテナ")]
        private UISheetContainer _sheetContainer;
        
        [Header("ページスクリーン")]
        [SerializeField, Tooltip("Top用スクリーン")]
        private EquipmentTopUIScreen _topUIScreen;
        [SerializeField, Tooltip("武器リスト用スクリーン")]
        private EquipmentWeaponListUIScreen _weaponListUIScreen;
        [SerializeField, Tooltip("防具リスト用スクリーン")]
        private EquipmentArmorListUIScreen _armorListUIScreen;

        /// <summary>戻るボタン</summary>
        public ButtonUIView BackButton => _backButton;
        /// <summary>トップ画面</summary>
        public EquipmentTopUIScreen TopScreen => _topUIScreen;
        /// <summary>武器リスト画面</summary>
        public EquipmentWeaponListUIScreen WeaponScreen => _weaponListUIScreen;
        /// <summary>防具リスト画面</summary>
        public EquipmentArmorListUIScreen ArmorScreen => _armorListUIScreen;

        /// <summary>
        /// Top画面に遷移
        /// </summary>
        public async UniTask ChangeTopAsync(CancellationToken ct) {
            await _sheetContainer.Change("Top", new CrossUiTransition())
                .ToUniTask(cancellationToken:ct);
        }

        /// <summary>
        /// Top画面に遷移
        /// </summary>
        public void ChangeTopImmediate() {
            _sheetContainer.Change("Top", immediate:true);
        }

        /// <summary>
        /// WeaponList画面に遷移
        /// </summary>
        public async UniTask ChangeWeaponListAsync(CancellationToken ct) {
            await _sheetContainer.Change("WeaponList", new CrossUiTransition())
                .ToUniTask(cancellationToken:ct);
        }

        /// <summary>
        /// WeaponList画面に遷移
        /// </summary>
        public void ChangeWeaponListImmediate() {
            _sheetContainer.Change("WeaponList", immediate:true);
        }

        /// <summary>
        /// ArmorList画面に遷移
        /// </summary>
        public async UniTask ChangeArmorListAsync(CancellationToken ct) {
            await _sheetContainer.Change("ArmorList", new CrossUiTransition())
                .ToUniTask(cancellationToken:ct);
        }

        /// <summary>
        /// ArmorList画面に遷移
        /// </summary>
        public void ChangeArmorListImmediate() {
            _sheetContainer.Change("ArmorList", immediate:true);
        }

        /// <summary>
        /// 画面を戻す
        /// </summary>
        public async UniTask BackAsync(CancellationToken ct) {
            await _sheetContainer.Clear()
                .ToUniTask(cancellationToken: ct);
        }
    }
}