using GameFramework.Core;

namespace SampleGame.Domain.ModelViewer {
    /// <summary>
    /// 読み取り専用インターフェース
    /// </summary>
    public interface IReadOnlyEnvironmentModel {
        /// <summary>識別子</summary>
        int Id { get; }
        
        /// <summary>マスター</summary>
        IEnvironmentMaster Master { get; }
        /// <summary>ディレクショナルライトのY角度</summary>
        float DirectionalLightAngleY { get; }
    }

    /// <summary>
    /// 環境モデル
    /// </summary>
    public class EnvironmentModel : AutoIdModel<EnvironmentModel>, IReadOnlyEnvironmentModel {
        private IEnvironmentPort _environmentPort;

        /// <summary>有効か</summary>
        public bool IsActive => _environmentPort != null;

        /// <summary>マスター</summary>
        public IEnvironmentMaster Master { get; private set; }
        /// <summary>ディレクショナルライトのY角度</summary>
        public float DirectionalLightAngleY => _environmentPort?.LightAngleY ?? 0.0f;

        /// <summary>
        /// データの設定
        /// </summary>
        public void Setup(IEnvironmentMaster master) {
            // マスター設定
            Master = master;
        }

        /// <summary>
        /// 制御ポートの設定
        /// </summary>
        public void SetPort(IEnvironmentPort port) {
            _environmentPort = port;
        }

        /// <summary>
        /// ディレクショナルライトのY角度を設定
        /// </summary>
        public void SetLightAngleY(float angleY) {
            if (!IsActive) {
                return;
            }
            
            _environmentPort.SetLightAngleY(angleY);
        }
    }
}