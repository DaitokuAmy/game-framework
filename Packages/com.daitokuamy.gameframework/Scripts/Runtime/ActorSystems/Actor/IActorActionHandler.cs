using System.Collections;

namespace GameFramework.ActorSystems
{
    /// <summary>
    /// アクターアクション用のインターフェース
    /// </summary>
    public interface IActorActionHandler
    {
        /// <summary>
        /// DeltaTimeの設定
        /// </summary>
        void SetDeltaTime(float deltaTime);

        /// <summary>
        /// アクション再生用のルーチン
        /// </summary>
        IEnumerator PlayRoutine(IActorAction action);

        /// <summary>
        /// 終了処理
        /// </summary>
        void Exit(IActorAction action);

        /// <summary>
        /// キャンセル処理
        /// </summary>
        void Cancel(IActorAction action);

        /// <summary>
        /// アクションの遷移
        /// </summary>
        bool Next(object[] args);
        
        /// <summary>
        /// 戻り時のブレンド時間取得
        /// </summary>
        float GetOutBlendDuration(IActorAction action);
    }
}