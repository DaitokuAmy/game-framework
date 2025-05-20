using GameFramework.Core;
using GameFramework.EnvironmentSystems;
using UnityEngine;

namespace SampleGame.Presentation {
    /// <summary>
    /// 環境設定
    /// </summary>
    [ExecuteAlways]
    public class EnvironmentSettings : MonoBehaviour {
        [SerializeField, Tooltip("反映対象のデータ")]
        private EnvironmentContextData _data;
        [SerializeField, Tooltip("平行光源")]
        private Light _sun;
        [SerializeField, Tooltip("ポスプロ")]
        private VolumeFader _volumeFader;
        [SerializeField, Tooltip("ブレンド時間")]
        private float _blendDuration;

        private EnvironmentHandle _handle;

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
                var manager = Services.Resolve<EnvironmentManager>();
                if (manager != null) {
                    manager.ForceApply();
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
            if (!UnityEngine.Application.isPlaying) {
                ApplyEnvironment();
                return;
            }

            var manager = Services.Resolve<EnvironmentManager>();
            if (manager != null) {
                var context = _data.CreateContext();
                context.Sun = _sun;
                _handle = manager.Push(context, _blendDuration);
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
                var manager = Services.Resolve<EnvironmentManager>();
                if (manager != null) {
                    manager.Remove(_handle);
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