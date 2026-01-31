using System;
using GameFramework;
using GameFramework;
using GameFramework.NavigationSystem;
using SampleGame.Application;
using SampleGame.Presentation;
using UnityEngine;
using VContainer;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// アプリ内遷移制御クラス
    /// </summary>
    partial class AppNavigator {
        /// <summary>
        /// 登録Id
        /// </summary>
        public static class Id {
            public const int Invalid = NavigationEngine.InvalidNodeId;
            
            public const int Root = 1;
            
            public const int Introduction = 100;
            public const int TitleTop = 101;
            public const int TitleOption = 102;
            
            public const int OutGame = 200;
            public const int Sortie = 201;
            public const int SortieTop = 202;
            public const int SortieRoleSelectTop = 204;
            public const int SortieRoleInformation = 205;
            public const int SortieMissionSelect = 206;
            public const int SortieDifficultySelect = 207;
            
            public const int Battle = 300;
            public const int BattleHud = 301;
            public const int BattlePause = 302;

            public const int ModelViewer = 400;

            public const int UITest = 500;
        }
    }
}