using GameFramework.OldModelSystems;

namespace SampleGame.Domain.ModelViewer {
    /// <summary>
    /// 読み取り専用インターフェース
    /// </summary>
    public interface IReadOnlyEnvironmentModel {
        /// <summary>マスター</summary>
        IEnvironmentMaster Master { get; }
        /// <summary>ディレクショナルライトのY角度</summary>
        float DirectionalLightAngleY { get; }
    }

    /// <summary>
    /// 環境モデル
    /// </summary>
    public class EnvironmentModel : AutoIdModel<EnvironmentModel>, IReadOnlyEnvironmentModel {
        private IEnvironmentController _environmentController;

        /// <summary>有効か</summary>
        public bool IsActive => _environmentController != null;

        /// <summary>マスター</summary>
        public IEnvironmentMaster Master { get; private set; }
        /// <summary>ディレクショナルライトのY角度</summary>
        public float DirectionalLightAngleY => _environmentController?.LightAngleY ?? 0.0f;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private EnvironmentModel(int id)
            : base(id) {
        }

        /// <summary>
        /// データの設定
        /// </summary>
        public void Setup(IEnvironmentMaster master) {
            // マスター設定
            Master = master;
        }

        /// <summary>
        /// コントローラーの設定
        /// </summary>
        public void SetController(IEnvironmentController controller) {
            _environmentController = controller;
        }

        /// <summary>
        /// ディレクショナルライトのY角度を設定
        /// </summary>
        public void SetLightAngleY(float angleY) {
            if (!IsActive) {
                return;
            }
            
            _environmentController.SetLightAngleY(angleY);
        }
    }
}