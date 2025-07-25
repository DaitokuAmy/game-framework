using System.Collections;

namespace GameFramework {
    /// <summary>
    /// 遷移処理解決用インターフェース
    /// </summary>
    public interface ITransitionResolver {
        /// <summary>
        /// 開始処理
        /// </summary>
        void Start();

        /// <summary>
        /// 開始エフェクトコルーチン
        /// </summary>
        IEnumerator EnterEffectRoutine();

        /// <summary>
        /// 終了エフェクトコルーチン
        /// </summary>
        IEnumerator ExitEffectRoutine();

        /// <summary>
        /// 遷移先のロードコルーチン
        /// </summary>
        IEnumerator LoadNextRoutine();

        /// <summary>
        /// 遷移先のアクティベート
        /// </summary>
        void ActivateNext();

        /// <summary>
        /// 遷移先のオープンコルーチン
        /// </summary>
        /// <param name="immediate">即時に開くか(PreOpen/PostOpenのみ)</param>
        IEnumerator OpenNextRoutine(bool immediate = false);

        /// <summary>
        /// 遷移元のクローズコルーチン
        /// </summary>
        /// <param name="immediate">即時に閉じる(PreClose/PostCloseのみ)</param>
        IEnumerator ClosePrevRoutine(bool immediate = false);

        /// <summary>
        /// 遷移元のディアクティベート
        /// </summary>
        void DeactivatePrev();

        /// <summary>
        /// 遷移元の解放コルーチン
        /// </summary>
        IEnumerator UnloadPrevRoutine();

        /// <summary>
        /// 完了処理
        /// </summary>
        void Finish();
    }
}