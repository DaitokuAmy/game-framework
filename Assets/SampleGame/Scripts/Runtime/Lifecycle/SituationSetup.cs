using System;
using GameFramework.NavigationSystems;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// NavNodeの初期化処理のインタフェース
    /// </summary>
    public interface INavNodeSetup {
        /// <summary>Nodeのタイプ</summary>
        Type NodeType { get; }

        /// <summary>Nodeのセットアップ処理</summary>
        void OnSetup(INavNode node);
    }

    /// <summary>
    /// NavNodeの初期化処理
    /// </summary>
    public class NavNodeSetup<T> : INavNodeSetup
        where T : IScreenNode {
        /// <inheritdoc/>
        Type INavNodeSetup.NodeType => typeof(T);

        private readonly Action<T> _setupAction;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public NavNodeSetup(Action<T> setupAction = null) {
            _setupAction = setupAction;
        }

        /// <inheritdoc/>
        void INavNodeSetup.OnSetup(INavNode node) {
            if (node is T startNode) {
                _setupAction?.Invoke(startNode);
            }
        }
    }
}