using System;
using System.Collections.Generic;
using GameFramework.Core;
using UnityEngine;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace GameFramework.CutsceneSystems {
    /// <summary>
    /// カットシーンクラス
    /// </summary>
    public class Cutscene : MonoBehaviour, ICutscene, INotificationReceiver {
        private PlayableDirector _playableDirector;
        private bool _isPlaying;
        private float _speed = 1.0f;
        private DisposableScope _scope;

        private readonly Dictionary<PlayableDirector, List<Object>> _bindingTrackKeys = new();

        /// <summary>再生中か</summary>
        bool ICutscene.IsPlaying => _isPlaying;

        /// <summary>
        /// 初期化処理
        /// </summary>
        void ICutscene.Initialize(bool updateGameTime) {
            _playableDirector = GetComponent<PlayableDirector>();

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

            _scope = new DisposableScope();
            InitializeInternal(_scope);
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

            DisposeInternal();
            _scope.Dispose();
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

            foreach (var pair in _bindingTrackKeys) {
                foreach (var key in pair.Value) {
                    pair.Key.ClearGenericBinding(key);
                }
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
            PlayInternal();
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

            gameObject.SetActive(false);
            StopInternal();
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

            UpdateInternal(deltaTime);
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

            SetSpeedInternal(speed);
        }

        /// <summary>
        /// 再生位置のシーク
        /// </summary>
        /// <param name="time">シーク時間</param>
        void ICutscene.Seek(float time) {
            if (_playableDirector == null) {
                return;
            }

            _playableDirector.time = time;
        }

        /// <summary>
        /// 通知の受信
        /// </summary>
        /// <param name="origin">発生元のPlayable</param>
        /// <param name="notification">通知内容</param>
        /// <param name="context">通知用ユーザー定義データ</param>
        void INotificationReceiver.OnNotify(Playable origin, INotification notification, object context) {
            OnNotifyInternal(origin, notification, context);
        }

        /// <summary>
        /// オブジェクトのバインド
        /// 対象全てに反映
        /// </summary>
        /// <param name="playableDirector">対象のPlayableDirector</param>
        /// <param name="trackName">Track名</param>
        /// <param name="target">バインド対象のオブジェクト</param>
        protected void Bind(PlayableDirector playableDirector, string trackName, Object target) {
            if (playableDirector == null) {
                return;
            }

            if (!_bindingTrackKeys.TryGetValue(playableDirector, out var list)) {
                list = new();
                _bindingTrackKeys[playableDirector] = list;
            }

            foreach (var output in playableDirector.playableAsset.outputs) {
                if (output.streamName != trackName) {
                    continue;
                }

                playableDirector.SetGenericBinding(output.sourceObject, target);
                list.Add(output.sourceObject);
            }
        }

        /// <summary>
        /// オブジェクトのバインド(型指定)
        /// 対象全てに反映
        /// </summary>
        /// <param name="playableDirector">対象のPlayableDirector</param>
        /// <param name="trackName">Track名</param>
        /// <param name="target">バインド対象のオブジェクト</param>
        protected void Bind<T>(PlayableDirector playableDirector, string trackName, T target)
            where T : Object {
            if (playableDirector == null) {
                return;
            }

            if (!_bindingTrackKeys.TryGetValue(playableDirector, out var list)) {
                list = new();
                _bindingTrackKeys[playableDirector] = list;
            }

            var targetType = typeof(T);
            foreach (var output in playableDirector.playableAsset.outputs) {
                if ((!string.IsNullOrEmpty(trackName) && output.streamName != trackName) || output.outputTargetType != targetType) {
                    continue;
                }

                playableDirector.SetGenericBinding(output.sourceObject, target);
                list.Add(output.sourceObject);
            }
        }

        /// <summary>
        /// オブジェクトのバインド(型指定)
        /// 対象全てに反映
        /// </summary>
        /// <param name="playableDirector">対象のPlayableDirector</param>
        /// <param name="target">バインド対象のオブジェクト</param>
        protected void Bind<T>(PlayableDirector playableDirector, T target)
            where T : Object {
            Bind(playableDirector, null, target);
        }

        /// <summary>
        /// オブジェクトのバインド
        /// 対象全てに反映
        /// </summary>
        /// <param name="trackName">Track名</param>
        /// <param name="target">バインド対象のオブジェクト</param>
        protected void Bind(string trackName, Object target) {
            Bind(_playableDirector, trackName, target);
        }

        /// <summary>
        /// オブジェクトのバインド(型指定)
        /// 対象全てに反映
        /// </summary>
        /// <param name="trackName">Track名</param>
        /// <param name="target">バインド対象のオブジェクト</param>
        protected void Bind<T>(string trackName, T target)
            where T : Object {
            Bind(_playableDirector, trackName, target);
        }

        /// <summary>
        /// オブジェクトのバインド(型指定)
        /// 対象全てに反映
        /// </summary>
        /// <param name="target">バインド対象のオブジェクト</param>
        protected void Bind<T>(T target)
            where T : Object {
            Bind(_playableDirector, target);
        }

        /// <summary>
        /// 初期化時処理(Override用)
        /// </summary>
        protected virtual void InitializeInternal(IScope scope) {
        }

        /// <summary>
        /// 破棄時処理(Override用)
        /// </summary>
        protected virtual void DisposeInternal() {
        }

        /// <summary>
        /// 再生時処理(Override用)
        /// </summary>
        protected virtual void PlayInternal() {
        }

        /// <summary>
        /// 停止時処理(Override用)
        /// </summary>
        protected virtual void StopInternal() {
        }

        /// <summary>
        /// 更新時処理(Override用)
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        protected virtual void UpdateInternal(float deltaTime) {
        }

        /// <summary>
        /// 再生速度の設定(Override用)
        /// </summary>
        /// <param name="speed">再生速度</param>
        protected virtual void SetSpeedInternal(float speed) {
        }


        /// <summary>
        /// 再生位置のシーク(Override用)
        /// </summary>
        /// <param name="time">シーク時間</param>
        protected virtual void SeekInternal(float time) {
        }

        /// <summary>
        /// Markerの受信処理(Override用)
        /// </summary>
        /// <param name="origin">発生元のPlayable</param>
        /// <param name="notification">通知内容</param>
        /// <param name="context">通知用ユーザー定義データ</param>
        protected virtual void OnNotifyInternal(Playable origin, INotification notification, object context) {
        }
    }
}