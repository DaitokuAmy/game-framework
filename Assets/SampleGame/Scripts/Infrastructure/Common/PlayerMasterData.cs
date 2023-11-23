using SampleGame.Domain.Common;
using UnityEngine;

namespace Opera.Infrastructure.Common {
    /// <summary>
    /// PlayerMasterData
    /// </summary>
    [CreateAssetMenu(fileName = "dat_master_player_0000.asset", menuName = "SampleGame/Master Data/Player")]
    public class PlayerMasterData : ScriptableObject, IPlayerMasterData {
        [Tooltip("名前")]
        public new string name;
        [Tooltip("表示用PrefabのAssetKey")]
        public string prefabAssetKey;

        /// <inheritdoc/>
        string IPlayerMasterData.Name => name;
        /// <inheritdoc/>
        string IPlayerMasterData.PrefabAssetKey => prefabAssetKey;
    }
}