using System;
using GameFramework.NavigationSystems;
using SampleGame.Application;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// メインシステム起動用のStarter基底
    /// </summary>
    public abstract class MainSystemStarter : GameFramework.BootSystems.MainSystemStarter {
        /// <summary>MainSystem開始引数の取得</summary>
        public sealed override object[] GetArguments() => new object[] { CreateStartArgs() };
        
        /// <summary>開始時に再生するNodeId</summary>
        protected abstract int NodeId { get; }

        /// <summary>
        /// NavNodeセットアップ処理
        /// </summary>
        protected virtual void OnNodeSetup(INavNode navNode) {}

        /// <summary>
        /// 開始引数の生成
        /// </summary>
        private MainSystem.StartArgs CreateStartArgs() {
            return new MainSystem.StartArgs {
                NodeId = NodeId,
                SetupAction = OnNodeSetup
            };
        }
    }
}