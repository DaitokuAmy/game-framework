using GameFramework.UISystems;
using UnityEngine;

namespace SampleGame.Presentation.Battle {
    /// <summary>
    /// 出撃画面用のUIService
    /// </summary>
    public class SortieUIService : UIService {
        [SerializeField, Tooltip("基礎スクリーン")]
        private SortieBaseUIScreen _baseScreen;
        [SerializeField, Tooltip("トップ")]
        private SortieTopUIScreen _topScreen;
        [SerializeField, Tooltip("兵科選択")]
        private SortieRoleSelectUIScreen _roleSelectScreen;
        [SerializeField, Tooltip("兵科情報")]
        private SortieRoleInformationUIScreen _roleInformationScreen;
        [SerializeField, Tooltip("ミッション選択")]
        private SortieMissionSelectUIScreen _missionSelectScreen;
        [SerializeField, Tooltip("難易度選択")]
        private SortieDifficultySelectUIScreen _difficultySelectScreen;
        
        /// <summary>基礎スクリーン</summary>
        public SortieBaseUIScreen BaseScreen => _baseScreen;
        /// <summary>トップスクリーン</summary>
        public SortieTopUIScreen TopScreen => _topScreen;
        /// <summary>兵科選択スクリーン</summary>
        public SortieRoleSelectUIScreen RoleSelectScreen => _roleSelectScreen;
        /// <summary>兵科情報スクリーン</summary>
        public SortieRoleInformationUIScreen RoleInformationScreen => _roleInformationScreen;
        /// <summary>ミッション選択スクリーン</summary>
        public SortieMissionSelectUIScreen MissionSelectScreen => _missionSelectScreen;
        /// <summary>難易度選択スクリーン</summary>
        public SortieDifficultySelectUIScreen DifficultySelectScreen => _difficultySelectScreen;
    }
}