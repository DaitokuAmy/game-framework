using System;

namespace GameFramework.ActorSystem {
    /// <summary>
    /// アクター更新用のインターフェース
    /// </summary>
    internal interface IActorRuntime<TKey> : IDisposable {
        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <param name="id">識別子</param>
        void Initialize(TKey id);
        
        /// <summary>
        /// 終了処理
        /// </summary>
        void Terminate();
        
        /// <summary>
        /// コントローラー更新
        /// </summary>
        void UpdateController(float deltaTime);

        /// <summary>
        /// ステートマシン更新
        /// </summary>
        void UpdateStateMachine(float deltaTime);

        /// <summary>
        /// モデル更新
        /// </summary>
        void UpdateModel(float deltaTime);

        /// <summary>
        /// プレゼンター更新
        /// </summary>
        void UpdatePresenter(float deltaTime);

        /// <summary>
        /// ビュー更新
        /// </summary>
        void UpdateView(float deltaTime);

        /// <summary>
        /// レシーバー更新
        /// </summary>
        void UpdateReceiver(float deltaTime);
    }
}