using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace GameFramework.CutsceneSystems {
    /// <summary>
    /// ランタイム用カットシーン
    /// </summary>
    public sealed class RuntimeCutscene : ICutscene {
        private PlayableDirector _playableDirector;
        private bool _isPlaying;
        private float _speed = 1.0f;

        private List<Object> _bindingTrackKeys = new();

        /// <summary>再生中か</summary>
        bool ICutscene.IsPlaying => _isPlaying;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="playableDirector">再生に使うPlayableDirector</param>
        public RuntimeCutscene(PlayableDirector playableDirector) {
            _playableDirector = playableDirector;
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        void ICutscene.Initialize(bool updateGameTime) {
            if (_playableDirector == null) {
                return;
            }

            if (updateGameTime) {
                _playableDirector.timeUpdateMode = DirectorUpdateMode.GameTime;
            }
            else {
                _playableDirector.timeUpdateMode = DirectorUpdateMode.Manual;
            }

            _playableDirector.playOnAwake = false;
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        void IDisposable.Dispose() {
            if (_playableDirector == null) {
                return;
            }

            if (_isPlaying) {
                ((ICutscene)this).Stop();
            }

            _playableDirector = null;
        }

        /// <summary>
        /// Poolに戻る際の処理
        /// </summary>
        void ICutscene.OnReturn() {
            if (_playableDirector == null) {
                return;
            }

            if (_isPlaying) {
                ((ICutscene)this).Stop();
            }

            foreach (var trackKey in _bindingTrackKeys) {
                _playableDirector.ClearGenericBinding(trackKey);
            }

            _bindingTrackKeys.Clear();
        }

        /// <summary>
        /// 再生処理
        /// </summary>
        void ICutscene.Play() {
            if (_playableDirector == null) {
                return;
            }

            if (_playableDirector.timeUpdateMode == DirectorUpdateMode.Manual) {
                _playableDirector.time = 0.0f;
            }
            else {
                _playableDirector.Play();
            }
            
            // 再生開始時にGraphが生成される可能性があるため、ここで速度を再設定
            if (_playableDirector.playableGraph.IsValid()) {
                _playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(_speed);
            }

            _isPlaying = true;
        }

        /// <summary>
        /// 停止処理
        /// </summary>
        void ICutscene.Stop() {
            if (_playableDirector == null) {
                return;
            }

            _isPlaying = false;
            if (_playableDirector.timeUpdateMode != DirectorUpdateMode.Manual) {
                _playableDirector.Stop();
            }

            _playableDirector.gameObject.SetActive(false);
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        void ICutscene.Update(float deltaTime) {
            if (_playableDirector == null) {
                return;
            }

            var wrapMode = _playableDirector.extrapolationMode;
            if (_playableDirector.timeUpdateMode == DirectorUpdateMode.Manual) {
                var time = _playableDirector.time + deltaTime;
                if (time >= _playableDirector.duration && wrapMode != DirectorWrapMode.Loop) {
                    time = _playableDirector.duration;
                    _isPlaying = wrapMode == DirectorWrapMode.Hold;
                }

                _playableDirector.time = time;
                _playableDirector.Evaluate();
            }
            else {
                if (wrapMode == DirectorWrapMode.None) {
                    if (_playableDirector.state != PlayState.Playing) {
                        _playableDirector.time = _playableDirector.duration;
                        _playableDirector.Stop();
                        _isPlaying = false;
                    }
                }
            }
        }

        /// <summary>
        /// 再生速度の設定
        /// </summary>
        /// <param name="speed">再生速度</param>
        void ICutscene.SetSpeed(float speed) {
            if (_playableDirector == null) {
                return;
            }
            
            _speed = speed;
            if (_playableDirector.playableGraph.IsValid()) {
                _playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(_speed);
            }
        }

        /// <summary>
        /// オブジェクトのバインド
        /// 対象全てに反映
        /// </summary>
        /// <param name="trackName">Track名</param>
        /// <param name="target">バインド対象のオブジェクト</param>
        public void Bind(string trackName, Object target) {
            if (_playableDirector == null) {
                return;
            }

            foreach (var output in _playableDirector.playableAsset.outputs) {
                if (output.streamName != trackName) {
                    continue;
                }
                
                _playableDirector.SetGenericBinding(output.sourceObject, target);
                _bindingTrackKeys.Add(output.sourceObject);
            }
        }

        /// <summary>
        /// オブジェクトのバインド(型指定)
        /// 対象全てに反映
        /// </summary>
        /// <param name="trackName">Track名</param>
        /// <param name="target">バインド対象のオブジェクト</param>
        public void Bind<T>(string trackName, T target)
            where T : Object {
            if (_playableDirector == null) {
                return;
            }

            var targetType = typeof(T);
            foreach (var output in _playableDirector.playableAsset.outputs) {
                if (output.streamName != trackName || output.outputTargetType != targetType) {
                    continue;
                }
                
                _playableDirector.SetGenericBinding(output.sourceObject, target);
                _bindingTrackKeys.Add(output.sourceObject);
            }
        }
    }
}