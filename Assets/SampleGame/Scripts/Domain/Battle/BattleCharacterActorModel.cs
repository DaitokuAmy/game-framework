using System;
using GameFramework.Core;
using GameFramework.ModelSystems;
using UniRx;
using UnityEngine;

namespace SampleGame.Domain.Battle {
    /// <summary>
    /// BattleCharacterActorModelの読み取り専用インターフェース
    /// </summary>
    public interface IReadOnlyBattleCharacterActorModel {
        BattleCharacterActorSetupData SetupData { get; }
        /// <summary>現在位置</summary>
        Vector3 Position { get; }
        /// <summary>現在向き</summary>
        Quaternion Rotation { get; }
        
        /// <summary>アクション再生通知</summary>
        IObservable<CharacterActorActionDto> ActionSubject { get; }
    }

    /// <summary>
    /// キャラアクターのアクション情報
    /// </summary>
    public struct CharacterActorActionDto {
        public string actionKey;
        public ActionHandle actionHandle;
    }
    
    /// <summary>
    /// バトル用プレイヤーのアクター情報をまとめたモデル
    /// </summary>
    public class BattleCharacterActorModel : AutoIdModel<BattleCharacterActorModel>, IReadOnlyBattleCharacterActorModel {
        private Subject<CharacterActorActionDto> _ActionSubject;
        private ActionInfo _currentActionInfo;
        
        /// <summary>Actor初期化用データ</summary>
        public BattleCharacterActorSetupData SetupData { get; private set; }
        /// <summary>現在座標</summary>
        public Vector3 Position { get; private set; }
        /// <summary>現在向き</summary>
        public Quaternion Rotation { get; private set; }
        
        /// <summary>アクション再生通知</summary>
        public IObservable<CharacterActorActionDto> ActionSubject => _ActionSubject;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private BattleCharacterActorModel(int id) : base(id) {}

        /// <summary>
        /// 生成時処理
        /// </summary>
        protected override void OnCreatedInternal(IScope scope) {
            _ActionSubject = new Subject<CharacterActorActionDto>().ScopeTo(scope);
        }

        /// <summary>
        /// 値の更新
        /// </summary>
        public void Setup(BattleCharacterActorSetupData setupData) {
            SetupData = setupData;
        }

        /// <summary>
        /// 座標の設定
        /// </summary>
        public void SetPosition(Vector3 position) {
            Position = position;
        }

        /// <summary>
        /// 向きの設定
        /// </summary>
        public void SetRotation(Quaternion rotation) {
            Rotation = rotation;
        }

        /// <summary>
        /// アクションの再生
        /// </summary>
        public IProcess PlayAction(string actionKey) {
            CancelAction();
            
            _currentActionInfo = new ActionInfo();
            
            var dto = new CharacterActorActionDto {
                actionKey = actionKey,
                actionHandle = new ActionHandle(_currentActionInfo),
            };

            _ActionSubject.OnNext(dto);
            return _currentActionInfo;
        }

        /// <summary>
        /// アクションのキャンセル
        /// </summary>
        public void CancelAction() {
            if (_currentActionInfo == null) {
                return;
            }
            
            _currentActionInfo.Cancel();
            _currentActionInfo = null;
        }
    }
}