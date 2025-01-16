using System;
using GameFramework.UISystems;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace SampleGame.Presentation.Introduction {
    /// <summary>
    /// TitleTop
    /// </summary>
    public class TitleTopUIService : UIService {
        [SerializeField, Tooltip("タイトル用スクリーン")]
        private AnimatableUIScreen _titleScreen;
        [SerializeField, Tooltip("タイトル開始用ボタン")]
        private Button _startButton;

        /// <summary>開始ボタン押下時通知</summary>
        public IObservable<Unit> OnClickedStartButtonSubject => _startButton.OnClickAsObservable();
    }
}