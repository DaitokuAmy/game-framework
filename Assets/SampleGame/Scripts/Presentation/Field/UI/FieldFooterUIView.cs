using System;
using GameFramework.UISystems;
using UniRx;
using UnityEngine;

namespace SampleGame.Field {
    /// <summary>
    /// FieldHud用のFooter
    /// </summary>
    public class FieldFooterUIView : UIView {
        [SerializeField, Tooltip("装備ボタン")]
        private ButtonUIView _equipmentButton;
        [SerializeField, Tooltip("ガチャボタン")]
        private ButtonUIView _gachaButton;
        [SerializeField, Tooltip("クエストボタン")]
        private ButtonUIView _questButton;

        /// <summary>装備ボタン押下通知</summary>
        public IObservable<Unit> OnClickEquipmentButtonSubject => _equipmentButton.OnClickSubject;
        /// <summary>ガチャボタン押下通知</summary>
        public IObservable<Unit> OnClickGachaButtonSubject => _gachaButton.OnClickSubject;
        /// <summary>クエストボタン押下通知</summary>
        public IObservable<Unit> OnClickQuestButtonSubject => _questButton.OnClickSubject;
    }
}