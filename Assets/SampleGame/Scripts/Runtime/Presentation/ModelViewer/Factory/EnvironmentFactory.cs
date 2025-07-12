using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.Core;
using SampleGame.Presentation;
using SampleGame.Presentation.ModelViewer;

namespace SampleGame.Domain.ModelViewer {
    /// <summary>
    /// 環境生成クラス
    /// </summary>
    public class EnvironmentFactory : IEnvironmentFactory {
        private readonly EnvironmentManager _environmentManager;
        private EnvironmentPort _environmentPort;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EnvironmentFactory() {
            _environmentManager = Services.Resolve<EnvironmentManager>();
        }
        
        /// <summary>
        /// 背景生成
        /// </summary>
        async UniTask<IEnvironmentPort> IEnvironmentFactory.CreateAsync(IReadOnlyEnvironmentModel model, CancellationToken ct) {
            if (_environmentPort != null) {
                _environmentPort.Dispose();
                _environmentPort = null;
            }
            
            var scene = await _environmentManager.ChangeEnvironmentAsync(model.Master.AssetKey, ct);
            var controller = new EnvironmentPort(model, scene);
            controller.RegisterTask(TaskOrder.Logic);
            controller.Activate();
            _environmentPort = controller;
            return controller;
        }

        /// <summary>
        /// 背景削除
        /// </summary>
        void IEnvironmentFactory.Destroy(int id) {
            if (_environmentPort != null) {
                _environmentPort.Dispose();
                _environmentPort = null;
            }
            
            _environmentManager.RemoveEnvironment();
        }
    }
}