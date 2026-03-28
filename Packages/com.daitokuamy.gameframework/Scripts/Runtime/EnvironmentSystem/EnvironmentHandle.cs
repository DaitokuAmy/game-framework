using UnityEngine;

namespace GameFramework.EnvironmentSystem {
    /// <summary>
    /// 環境設定適用後のハンドル
    /// </summary>
    public struct EnvironmentHandle {
        // 制御情報
        private EnvironmentManager.EnvironmentInfo _environmentInfo;

        /// <summary>有効なハンドルか</summary>
        public bool IsValid => _environmentInfo != null;
        /// <summary>完了したか</summary>
        public bool IsDone => !IsValid || _environmentInfo.Timer <= 0.0f;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="info">制御情報</param>
        public EnvironmentHandle(EnvironmentManager.EnvironmentInfo info) {
            _environmentInfo = info;
        }

        /// <summary>
        /// コンテキストの取得
        /// </summary>
        public IEnvironmentContext GetContext() {
            if (_environmentInfo == null) {
                return null;
            }

            return _environmentInfo.Context;
        }

        /// <summary>
        /// コンテキスト野設定
        /// </summary>
        public void SetContext(IEnvironmentContext context) {
            if (_environmentInfo == null) {
                return;
            }

            _environmentInfo.Context = context;
            _environmentInfo.Timer = Mathf.Max(0.0f, _environmentInfo.Timer);
        }

        /// <inheritdoc/>
        public override int GetHashCode() {
            return _environmentInfo != null ? _environmentInfo.GetHashCode() : 0;
        }
    }
}
