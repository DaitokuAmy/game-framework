using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Experimental.Animations;
using UnityEngine.Playables;

namespace GameFramework.BodySystems {
    /// <summary>
    /// AnimationClipを使ったStateGimmick
    /// </summary>
    public class ClipAnimationStateGimmick : StateGimmickBase<ClipAnimationStateGimmick.StateInfo> {
        // 更新モード
        private enum UpdateMode {
            Update,
            LateUpdate,
        }

        /// <summary>
        /// ステート情報基底
        /// </summary>
        [Serializable]
        public class StateInfo : StateInfoBase {
            [Tooltip("反映させるクリップ")]
            public AnimationClip clip;
        }

        [SerializeField, Tooltip("更新モード")]
        private UpdateMode _updateMode = UpdateMode.LateUpdate;
        [SerializeField, Tooltip("再生させるAnimator")]
        private Animator _animator;
        [SerializeField, Tooltip("再生に使用するAvatarMask")]
        private AvatarMask _avatarMask;
        [SerializeField, Tooltip("ブレンド時間")]
        private float _blendDuration = 0.2f;

        private PlayableGraph _graph;
        private AnimationLayerMixerPlayable _layerMixerPlayable;
        private AnimationMixerPlayable _mixerPlayable;
        private Dictionary<StateInfo, int> _stateInfoToInputIndices = new();
        private float _blendTimer = -1.0f;
        private int _currentIndex = -1;

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal() {
            base.InitializeInternal();

            // キーに対応するClipを元にPlayableを構築
            _graph = PlayableGraph.Create($"{nameof(ClipAnimationStateGimmick)}-{name}");
            _layerMixerPlayable = AnimationLayerMixerPlayable.Create(_graph, 1, true);
            _mixerPlayable = AnimationMixerPlayable.Create(_graph);
            var output = AnimationPlayableOutput.Create(_graph, "output", _animator);
            output.SetAnimationStreamSource(AnimationStreamSource.PreviousInputs);
            for (var i = 0; i < StateInfos.Count; i++) {
                var stateInfo = StateInfos[i];
                if (stateInfo.clip == null) {
                    continue;
                }

                var playable = AnimationClipPlayable.Create(_graph, stateInfo.clip);
                var index = _mixerPlayable.AddInput(playable, 0);
                _mixerPlayable.SetInputWeight(index, 0.0f);
                _stateInfoToInputIndices[stateInfo] = index;
            }

            _layerMixerPlayable.ConnectInput(0, _mixerPlayable, 0);
            if (_avatarMask != null) {
                _layerMixerPlayable.SetLayerMaskFromAvatarMask(0, _avatarMask);
            }

            _layerMixerPlayable.SetInputWeight(0, 1.0f);
            _layerMixerPlayable.SetLayerAdditive(0, false);

            output.SetSourcePlayable(_layerMixerPlayable);
            _graph.Play();
            _graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
        }

        /// <summary>
        /// ステートの変更処理
        /// </summary>
        /// <param name="prev">変更前のステート</param>
        /// <param name="current">変更後のステート</param>
        /// <param name="immediate">即時遷移するか</param>
        protected override void ChangeState(StateInfo prev, StateInfo current, bool immediate) {
            _blendTimer = !immediate && _currentIndex >= 0 ? _blendDuration : 0.0f;

            if (_stateInfoToInputIndices.TryGetValue(current, out var index)) {
                _currentIndex = index;
                if (!current.clip.isLooping) {
                    _mixerPlayable.GetInput(index).SetTime(immediate ? current.clip.length : 0.0f);
                }
            }
            else {
                _currentIndex = -1;
            }
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected sealed override void UpdateInternal(float deltaTime) {
            if (_updateMode == UpdateMode.Update) {
                UpdateAnimation(deltaTime);
            }
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        protected sealed override void LateUpdateInternal(float deltaTime) {
            if (_updateMode == UpdateMode.LateUpdate) {
                UpdateAnimation(deltaTime);
            }
        }

        /// <summary>
        /// アニメーション更新
        /// </summary>
        private void UpdateAnimation(float deltaTime) {
            // ブレンド処理
            if (_blendTimer >= 0.0f) {
                _blendTimer -= deltaTime;
                var rate = _blendTimer > 0.0001f ? Mathf.Min(1, deltaTime / _blendTimer) : 1.0f;
                foreach (var pair in _stateInfoToInputIndices) {
                    var index = pair.Value;
                    var currentWeight = _mixerPlayable.GetInputWeight(index);
                    var targetWeight = _currentIndex == index ? 1.0f : 0.0f;
                    _mixerPlayable.SetInputWeight(index, Mathf.Lerp(currentWeight, targetWeight, rate));
                }
            }

            _graph.Evaluate(deltaTime);
        }
    }
}