using System;
using GameFramework.Core;
using SampleGame.Domain.User;
using SampleGame.Application.User;
using UnityEngine;

namespace SampleGame.Infrastructure.User {
    /// <summary>
    /// ユーザープレイヤー情報管理用リポジトリ
    /// </summary>
    public class PrefsUserPlayerRepository : IUserPlayerRepository {
        private const string PrefsKey = "UserPlayer";
        
        /// <summary>
        /// プレイヤー情報の読み込み
        /// </summary>
        IProcess<UserPlayerDto> IUserPlayerRepository.LoadUserPlayer() {
            var op = new AsyncOperator<UserPlayerDto>();
            var dto = Load();
            op.Completed(dto);
            return op.GetHandle();
        }

        /// <summary>
        /// 装備武器の保存
        /// </summary>
        IProcess<bool> IUserPlayerRepository.SaveWeapon(int weaponId) {
            var op = new AsyncOperator<bool>();
            Save(dto => {
                dto.weaponId = weaponId;
                return dto;
            });
            op.Completed(true);
            return op.GetHandle();
        }

        /// <summary>
        /// 装備頭防具の保存
        /// </summary>
        IProcess<bool> IUserPlayerRepository.SaveHelmArmor(int armorId) {
            var op = new AsyncOperator<bool>();
            Save(dto => {
                dto.helmArmorId = armorId;
                return dto;
            });
            op.Completed(true);
            return op.GetHandle();
        }

        /// <summary>
        /// 装備体防具の保存
        /// </summary>
        IProcess<bool> IUserPlayerRepository.SaveBodyArmor(int armorId) {
            var op = new AsyncOperator<bool>();
            Save(dto => {
                dto.bodyArmorId = armorId;
                return dto;
            });
            op.Completed(true);
            return op.GetHandle();
        }

        /// <summary>
        /// 装備腕防具の保存
        /// </summary>
        IProcess<bool> IUserPlayerRepository.SaveArmsArmor(int armorId) {
            var op = new AsyncOperator<bool>();
            Save(dto => {
                dto.armsArmorId = armorId;
                return dto;
            });
            op.Completed(true);
            return op.GetHandle();
        }

        /// <summary>
        /// 装備脚防具の保存
        /// </summary>
        IProcess<bool> IUserPlayerRepository.SaveLegsArmor(int armorId) {
            var op = new AsyncOperator<bool>();
            Save(dto => {
                dto.legsArmorId = armorId;
                return dto;
            });
            op.Completed(true);
            return op.GetHandle();
        }

        /// <summary>
        /// 情報の書き込み
        /// </summary>
        private void Save(Func<UserPlayerDto, UserPlayerDto> writer) {
            var dto = Load();
            dto = writer.Invoke(dto);
            PlayerPrefs.SetString(PrefsKey, JsonUtility.ToJson(dto));
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 情報の読み込み
        /// </summary>
        private UserPlayerDto Load() {
            var json = PlayerPrefs.GetString(PrefsKey, "");
            return !string.IsNullOrEmpty(json) ? JsonUtility.FromJson<UserPlayerDto>(json) : new UserPlayerDto {
                playerId = 1,
                helmArmorId = 1,
                bodyArmorId = 2,
                armsArmorId = 3,
                legsArmorId = 4,
                weaponId = 101,
            };
        }
    }
}
