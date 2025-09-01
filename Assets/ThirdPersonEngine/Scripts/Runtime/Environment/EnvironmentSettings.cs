using GameFramework.Core;
using GameFramework.EnvironmentSystems;
using UnityEngine;

namespace ThirdPersonEngine {
    /// <summary>
    /// 環境設定
    /// </summary>
    [ExecuteAlways]
    public class EnvironmentSettings : MonoBehaviour, IServiceUser {
        [SerializeField, Tooltip("反映対象のデータ")]
        private EnvironmentContextData _data;
        [SerializeField, Tooltip("平行光源")]
        private Light _sun;
        [SerializeField, Tooltip("ポスプロ")]
        private VolumeFader _volumeFader;
        [SerializeField, Tooltip("ブレンド時間")]
        private float _blendDuration;

        private EnvironmentHandle _handle;
        private EnvironmentManager _environmentManager;

        /// <inheritdoc/>
        void IServiceUser.ImportService(IServiceResolver serviceResolver) {
            _environmentManager = serviceResolver.Resolve<EnvironmentManager>();
            enabled = false;
            enabled = true;
        }

        /// <summary>
        /// 強制的に環境を上書きする(Debug用)
        /// </summary>
        public void ApplyEnvironment() {
            if (_handle.IsValid) {
                var context = _data.CreateContext();
                context.Sun = _sun;
                _handle.SetContext(context);

                if (_sun != null) {
                    _sun.enabled = true;
                }

                if (_volumeFader != null) {
                    _volumeFader.Fade(1.0f, 0.0f);
                }
            }
            else {
                if (_environmentManager != null) {
                    _environmentManager.ForceApply();
                }
                else {
                    var resolver = (IEnvironmentResolver)new EnvironmentResolver();
                    if (_data != null) {
                        var context = _data.CreateContext();
                        context.Sun = _sun;
                        resolver.Apply(context);
                    }

                    if (_sun != null) {
                        _sun.enabled = true;
                    }

                    if (_volumeFader != null) {
                        _volumeFader.Fade(1.0f, 0.0f);
                    }
                }
            }
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        private void OnEnable() {
            if (_data == null) {
                return;
            }

            // 非再生中はActiveになった瞬間に反映
            if (!Application.isPlaying) {
                ApplyEnvironment();
                return;
            }

            if (_environmentManager != null) {
                var context = _data.CreateContext();
                context.Sun = _sun;
                _handle = _environmentManager.Push(context, _blendDuration);
            }

            if (_sun != null) {
                _sun.enabled = true;
            }

            if (_volumeFader != null) {
                _volumeFader.Fade(1.0f, _blendDuration);
            }
        }

        /// <summary>
        /// 非アクティブ時処理
        /// </summary>
        private void OnDisable() {
            if (_handle.IsValid) {
                if (_environmentManager != null) {
                    _environmentManager.Remove(_handle);
                }
            }

            if (_sun != null) {
                _sun.enabled = false;
            }

            if (_volumeFader != null) {
                _volumeFader.Fade(0.0f, _blendDuration);
            }
        }
    }
}