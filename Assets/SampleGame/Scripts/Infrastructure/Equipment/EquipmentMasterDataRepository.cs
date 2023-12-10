using System;
using GameFramework.AssetSystems;
using GameFramework.Core;
using Opera.Infrastructure.Equipment;
using SampleGame.Domain.Common;
using SampleGame.Domain.Equipment;

namespace SampleGame.Infrastructure.Common {
    /// <summary>
    /// ユーザープレイヤー情報管理用リポジトリ
    /// </summary>
    public class EquipmentMasterDataRepository : IEquipmentMasterDataRepository, IDisposable {
        private DisposableScope _unloadScope;
        private AssetManager _assetManager;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EquipmentMasterDataRepository(AssetManager assetManager) {
            _unloadScope = new DisposableScope();
            _assetManager = assetManager;
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            _assetManager = null;
            _unloadScope?.Dispose();
            _unloadScope = null;
        }
        
        /// <summary>
        /// 武器マスターの読み込み
        /// </summary>
        IProcess<IWeaponMasterData> IEquipmentMasterDataRepository.LoadWeapon(int id) {
            var masterData = new WeaponMasterData {
                name = "Test",
                prefabAssetKey = "",
                elementType = ElementType.None,
                physicalAttack = 100,
                magicalAttack = 100,
            };

            return new AsyncStatusProvider<IWeaponMasterData>(() => true, () => null, () => masterData).GetHandle();
        }

        /// <summary>
        /// 防具マスターの読み込み
        /// </summary>
        IProcess<IArmorMasterData> IEquipmentMasterDataRepository.LoadArmor(int id) {
            var masterData = new ArmorMasterData {
                name = "Test",
                prefabAssetKey = "",
                armorType = ArmorType.Helm + (id - 1) % 4,
                physicalDefense = 100,
                magicalDefense = 100,
            };

            return new AsyncStatusProvider<IArmorMasterData>(() => true, () => null, () => masterData).GetHandle();
        }
    }
}
