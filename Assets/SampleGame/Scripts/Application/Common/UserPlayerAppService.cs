using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.Core;
using SampleGame.Domain.Common;
using SampleGame.Domain.User;

namespace SampleGame.Application.Common {
    /// <summary>
    /// UserPlayer用のアプリケーションサービス
    /// </summary>
    public class UserPlayerAppService : IDisposable {
        private UserPlayerModel _userPlayerModel;
        private IUserPlayerRepository _userPlayerRepository;
        private IPlayerMasterDataRepository _playerMasterDataRepository;
        private IEquipmentMasterDataRepository _equipmentMasterDataRepository;

        /// <summary>参照用のUserPlayerModel</summary>
        public IReadOnlyUserPlayerModel UserPlayerModel => _userPlayerModel;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public UserPlayerAppService(
            UserPlayerModel model, IUserPlayerRepository repository,
            IPlayerMasterDataRepository playerMasterDataRepository,
            IEquipmentMasterDataRepository equipmentMasterDataRepository) {
            _userPlayerModel = model;
            _userPlayerRepository = repository;
            _playerMasterDataRepository = playerMasterDataRepository;
            _equipmentMasterDataRepository = equipmentMasterDataRepository;
        }

        /// <summary>
        /// UserPlayerデータの読み込み
        /// </summary>
        public async UniTask LoadUserPlayer(CancellationToken ct) {
            ct.ThrowIfCancellationRequested();

            var userPlayerDto = await _userPlayerRepository.LoadUserPlayer()
                .ToUniTask(cancellationToken: ct);

            var tasks = new List<UniTask>();
            tasks.Add(_playerMasterDataRepository.LoadPlayer(userPlayerDto.playerId)
                .ToUniTask(cancellationToken: ct)
                .ContinueWith(result => { _userPlayerModel.PlayerModel.Setup(result); }));
            tasks.Add(_equipmentMasterDataRepository.LoadWeapon(userPlayerDto.weaponId)
                .ToUniTask(cancellationToken: ct)
                .ContinueWith(result => { _userPlayerModel.WeaponModel.Setup(result); }));
            tasks.Add(_equipmentMasterDataRepository.LoadArmor(userPlayerDto.helmArmorId)
                .ToUniTask(cancellationToken: ct)
                .ContinueWith(result => { _userPlayerModel.HelmArmorModel.Setup(result); }));
            tasks.Add(_equipmentMasterDataRepository.LoadArmor(userPlayerDto.bodyArmorId)
                .ToUniTask(cancellationToken: ct)
                .ContinueWith(result => { _userPlayerModel.BodyArmorModel.Setup(result); }));
            tasks.Add(_equipmentMasterDataRepository.LoadArmor(userPlayerDto.armsArmorId)
                .ToUniTask(cancellationToken: ct)
                .ContinueWith(result => { _userPlayerModel.ArmsArmorModel.Setup(result); }));
            tasks.Add(_equipmentMasterDataRepository.LoadArmor(userPlayerDto.legsArmorId)
                .ToUniTask(cancellationToken: ct)
                .ContinueWith(result => { _userPlayerModel.LegsArmorModel.Setup(result); }));

            await UniTask.WhenAll(tasks);
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
        }
    }
}