using System.Collections;

namespace GameFramework.SituationSystems {
    /// <summary>
    /// 遷移処理用インターフェース
    /// </summary>
    public interface ITransition {
        /// <summary>
        /// 遷移処理
        /// </summary>
        /// <param name="resolver">遷移処理解決者</param>
        IEnumerator TransitRoutine(ITransitionResolver resolver);
    }

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
        /// 遷移先のオープンコルーチン
        /// </summary>
        IEnumerator OpenNextRoutine();

        /// <summary>
        /// 遷移元のクローズコルーチン
        /// </summary>
        IEnumerator ClosePrevRoutine();

        /// <summary>
        /// 遷移先のアクティベート
        /// </summary>
        void ActivateNext();

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