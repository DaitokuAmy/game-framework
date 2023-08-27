using System;
using GameFramework.UISystems;
using UniRx;
using UnityEngine;

namespace SampleGame.Field {
    /// <summary>
    /// FieldHud用のHeader
    /// </summary>
    public class FieldHeaderUIView : UIView {
        [SerializeField, Tooltip("プレゼントボタン")]
        private ButtonUIView _presentButton;
        [SerializeField, Tooltip("ランキングボタン")]
        private ButtonUIView _rankingButton;
        [SerializeField, Tooltip("お知らせボタン")]
        private ButtonUIView _noticeButton;

        /// <summary>プレゼントボタン押下通知</summary>
        public IObservable<Unit> OnClickPresentButton => _presentButton.OnClickSubject;
        /// <summary>ランキイングボタン押下通知</summary>
        public IObservable<Unit> OnClickRankingButton => _rankingButton.OnClickSubject;
        /// <summary>お知らせボタン押下通知</summary>
        public IObservable<Unit> OnClickNoticeButton => _noticeButton.OnClickSubject;
    }
}