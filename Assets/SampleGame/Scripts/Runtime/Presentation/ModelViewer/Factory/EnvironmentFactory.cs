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
        private EnvironmentController _environmentController;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EnvironmentFactory() {
            _environmentManager = Services.Resolve<EnvironmentManager>();
        }
        
        /// <summary>
        /// 背景生成
        /// </summary>
        async UniTask<IEnvironmentController> IEnvironmentFactory.CreateAsync(IReadOnlyEnvironmentModel model, CancellationToken ct) {
            if (_environmentController != null) {
                _environmentController.Dispose();
                _environmentController = null;
            }
            
            var scene = await _environmentManager.ChangeEnvironmentAsync(model.Master.AssetKey, ct);
            var controller = new EnvironmentController(model, scene);
            controller.RegisterTask(TaskOrder.Logic);
            controller.Activate();
            _environmentController = controller;
            return controller;
        }

        /// <summary>
        /// 背景削除
        /// </summary>
        void IEnvironmentFactory.Destroy(int id) {
            if (_environmentController != null) {
                _environmentController.Dispose();
                _environmentController = null;
            }
            
            _environmentManager.RemoveEnvironment();
        }
    }
}