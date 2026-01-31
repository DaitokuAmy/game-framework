using System.Collections;
using System.Collections.Generic;

#if USE_VCONTAINER
using VContainer;
#endif

namespace GameFramework.NavigationSystems {
    /// <summary>
    /// ナビゲーションシステムで管理されるノードの基本インターフェース
    /// </summary>
    public interface INavNode {
        /// <summary>Loadを並列で実行可能か</summary>
        bool IsParallelLoading { get; }
        /// <summary>識別Id</summary>
        int NodeId { get; }
        /// <summary>接続されている親</summary>
        INavNode Parent { get; }
        /// <summary>接続している子のリスト</summary>
        IReadOnlyList<INavNode> Children { get; }
        /// <summary>アクティブ状態</summary>
        bool IsActive { get; }
#if USE_VCONTAINER
        /// <summary>VContainer用のResolver</summary>
        IObjectResolver ObjectResolver { get; }
#endif

        /// <summary>
        /// フォーカスの設定
        /// </summary>
        /// <param name="focus">フォーカス状態</param>
        void SetFocus(bool focus);

        /// <summary>
        /// 遷移方法を上書きする処理
        /// </summary>
        /// <param name="nextNode">遷移先のNode</param>
        /// <param name="transition">現在の遷移手法</param>
        ITransition OverrideTransition(INavNode nextNode, ITransition transition);

#if USE_VCONTAINER
        /// <summary>
        /// 親の設定
        /// </summary>
        /// <param name="nodeId">登録された識別Id</param>
        /// <param name="parent">親要素にあたるNode</param>
        /// <param name="parentObjectResolver">VContainer用の親Resolver</param>
        void Setup(int nodeId, INavNode parent, IObjectResolver parentObjectResolver);
#else
        /// <summary>
        /// 親の設定
        /// </summary>
        /// <param name="nodeId">登録された識別Id</param>
        /// <param name="parent">親要素にあたるNode</param>
        void SetParent(int nodeId, INavNode parent);
#endif

        /// <summary>
        /// スタンバイ処理
        /// </summary>
        /// <param name="engine">Navigation制御用エンジン</param>
        void Standby(NavigationEngine engine);

        /// <summary>
        /// 読み込み処理
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        IEnumerator LoadRoutine(TransitionHandle<INavNode> handle);

        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        IEnumerator InitializeRoutine(TransitionHandle<INavNode> handle);

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        void Activate(TransitionHandle<INavNode> handle);

        /// <summary>
        /// 更新処理
        /// </summary>
        void Update();

        /// <summary>
        /// 非アクティブ時処理
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        void Deactivate(TransitionHandle<INavNode> handle);

        /// <summary>
        /// 終了処理
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        void Terminate(TransitionHandle<INavNode> handle);

        /// <summary>
        /// アンロード処理
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        void Unload(TransitionHandle<INavNode> handle);

        /// <summary>
        /// 廃棄
        /// </summary>
        void Release();

        /// <summary>
        /// 強制終了
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        void Shutdown(TransitionHandle<INavNode> handle);
    }
}