using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
#if USE_ANIMATION_RIGGING
using UnityEngine.Animations.Rigging;
#endif

namespace GameFramework.ActorSystems.Editor {
    /// <summary>
    /// MotionPreview用Window
    /// </summary>
    public class MotionPreviewWindow : EditorWindow {
        // 再生するAnimationClip
        private AnimationClip _previewClip;
        // 骨制御用Animator
        private Animator _animator;
#if USE_ANIMATION_RIGGING
        // Rig制御用のBuilder
        private RigBuilder _builder;
#endif

        // Animation再生用
        private PlayableGraph _graph;
        private AnimationPlayableOutput _output;
        private AnimationClipPlayable _clipPlayable;

        // 再生時間管理用
        private float _seekTime;
        private bool _isPlaying;
        private double _prevTime;
        private bool _loop;

        [MenuItem("Window/GameFramework/Motion Preview")]
        private static void Open() {
            CreateWindow<MotionPreviewWindow>(ObjectNames.NicifyVariableName(nameof(MotionPreviewWindow)));
        }

        [MenuItem("CONTEXT/Animator/GameFramework/Motion Preview")]
        private static void OpenFromContext(MenuCommand command) {
            var window = CreateWindow<MotionPreviewWindow>(ObjectNames.NicifyVariableName(nameof(MotionPreviewWindow)));
            window.Setup(command.context as Animator, null);
        }

        /// <summary>
        /// 外部初期化用関数
        /// </summary>
        private void Setup(Animator animator, AnimationClip clip) {
            _animator = animator;
            _previewClip = clip;
            
            if (_animator != null) {
                titleContent = new GUIContent($"[Preview]{_animator.name}");
            }

            
            SetupGraph();
            SetupPlayable();
            SetupRigBuilder();
        }

        /// <summary>
        /// Playableの初期化
        /// </summary>
        private void SetupPlayable() {
            CleanupPlayable();

            if (!_graph.IsValid() || _previewClip == null) {
                return;
            }

            // 基本アニメーション構築
            _clipPlayable = AnimationClipPlayable.Create(_graph, _previewClip);
            _clipPlayable.SetApplyFootIK(false);
            _clipPlayable.SetTime(0.0f);
            _output.SetSourcePlayable(_clipPlayable, 0);
            _seekTime = 0.0f;
        }

        /// <summary>
        /// Playableのクリーン
        /// </summary>
        private void CleanupPlayable() {
            Stop();

            if (_clipPlayable.IsValid()) {
                _clipPlayable.Destroy();
            }
        }

        /// <summary>
        /// RigBuilderの初期化
        /// </summary>
        private void SetupRigBuilder() {
#if USE_ANIMATION_RIGGING
            CleanupRigBuilder();

            if (!_graph.IsValid() || _animator == null) {
                return;
            }

            _builder = _animator.gameObject.GetComponent<RigBuilder>();

            if (_builder == null) {
                return;
            }

            _builder.StartPreview();
            var playable = _builder.BuildPreviewGraph(_graph, _clipPlayable);
            _output.SetSourcePlayable(playable);
#endif
        }

        /// <summary>
        /// RigBuilderのクリーン
        /// </summary>
        private void CleanupRigBuilder() {
#if USE_ANIMATION_RIGGING
            if (_builder == null) {
                return;
            }

            _builder.Clear();
            _builder = null;
#endif
        }

        /// <summary>
        /// Graphの初期化
        /// </summary>
        private void SetupGraph() {
            CleanupGraph();

            if (_animator == null) {
                return;
            }

            _graph = PlayableGraph.Create($"[{nameof(MotionPreviewWindow)}]{_animator.name}");
            _graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
            _output = AnimationPlayableOutput.Create(_graph, "Output", _animator);
        }

        /// <summary>
        /// Graphのクリーン
        /// </summary>
        private void CleanupGraph() {
            if (_graph.IsValid()) {
                _graph.Destroy();
            }
        }

        /// <summary>
        /// 再生開始
        /// </summary>
        private void Play() {
            if (_isPlaying) {
                return;
            }

            _isPlaying = true;
            _seekTime = 0.0f;
            _prevTime = EditorApplication.timeSinceStartup;
        }

        /// <summary>
        /// 再生停止
        /// </summary>
        private void Stop() {
            if (!_isPlaying) {
                return;
            }

            _isPlaying = false;
        }

        /// <summary>
        /// Seek時間の反映
        /// </summary>
        private void ApplySeekTime() {
            if (!_clipPlayable.IsValid()) {
                return;
            }

            _clipPlayable.SetTime(_seekTime);
        }

        /// <summary>
        /// アニメーションの状態をリセットする
        /// </summary>
        private void ResetBones() {
            if (_animator != null) {
                _animator.WriteDefaultValues();
                
                if (PrefabUtility.IsPartOfVariantPrefab(_animator) || PrefabUtility.IsPartOfModelPrefab(_animator)) {
                    var renderers = _animator.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                    var bones = new HashSet<Transform>();
                    foreach (var renderer in renderers) {
                        foreach (var bone in renderer.bones) {
                            bones.Add(bone);
                        }
                    }

                    foreach (var bone in bones) {
                        if (bone == null) {
                            return;
                        }
                        
                        PrefabUtility.RevertObjectOverride(bone.transform, InteractionMode.AutomatedAction);
                    }
                    
                    
                }

                EditorUtility.SetDirty(_animator);
            }
        }

        /// <summary>
        /// Label付け可能なButton
        /// </summary>
        private bool ButtonField(string label, string buttonName) {
            using (new EditorGUILayout.HorizontalScope()) {
                var labelWidth = EditorGUIUtility.labelWidth;
                if (string.IsNullOrEmpty(label)) {
                    GUILayout.Space(labelWidth);
                }
                else {
                    GUILayout.Label(label, GUILayout.Width(labelWidth));
                }

                if (GUILayout.Button(buttonName)) {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// GUI描画
        /// </summary>
        private void OnGUI() {
            using (var scope = new EditorGUI.ChangeCheckScope()) {
                _animator = EditorGUILayout.ObjectField("Target", _animator, typeof(Animator), true) as Animator;
                if (_animator != null) {
                    titleContent = new GUIContent($"[Preview]{_animator.name}");
                }

                if (scope.changed) {
                    SetupGraph();
                    SetupPlayable();
                    SetupRigBuilder();
                }
            }

            using (var scope = new EditorGUI.ChangeCheckScope()) {
                _previewClip = EditorGUILayout.ObjectField("PreviewClip", _previewClip, typeof(AnimationClip), true) as AnimationClip;
                if (scope.changed) {
                    SetupPlayable();
                    SetupRigBuilder();
                }
            }

            // Animation
            var enabledAnimation = _clipPlayable.IsValid();
            using (new EditorGUI.DisabledScope(!enabledAnimation)) {
                var duration = _previewClip != null ? _previewClip.length : 1.0f;
                using (var scope = new EditorGUI.ChangeCheckScope()) {
                    _seekTime = EditorGUILayout.Slider("Time", _seekTime, 0.0f, duration);
                    if (scope.changed) {
                        ApplySeekTime();
                    }
                }

                _loop = EditorGUILayout.Toggle("Loop", _loop);
                if (ButtonField("", _isPlaying ? "Stop" : "Play")) {
                    if (_isPlaying) {
                        Stop();
                    }
                    else {
                        Play();
                    }
                }
            }

            // Setup/Reset
            if (ButtonField("", enabledAnimation ? "Reset" : "Setup")) {
                if (enabledAnimation) {
                    Stop();
                    ResetBones();
                    CleanupRigBuilder();
                    CleanupPlayable();
                    CleanupGraph();
                }
                else {
                    SetupGraph();
                    SetupPlayable();
                    SetupRigBuilder();
                }
            }
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        private void OnDisable() {
            Stop();
            ResetBones();
            CleanupRigBuilder();
            CleanupPlayable();
            CleanupGraph();
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        private void Update() {
            if (Application.isPlaying) {
                return;
            }

            // 再生中
            if (_isPlaying) {
                var deltaTime = (float)(EditorApplication.timeSinceStartup - _prevTime);
                if (_previewClip != null) {
                    if (_loop) {
                        _seekTime = (_seekTime + deltaTime) % _previewClip.length;
                    }
                    else {
                        _seekTime = Mathf.Min(_previewClip.length, _seekTime + deltaTime);
                        if (_seekTime >= _previewClip.length) {
                            Stop();
                        }
                    }

                    ApplySeekTime();
                    Repaint();
                }

                _prevTime = EditorApplication.timeSinceStartup;
            }

            // Graph/Builderの反映
            if (_graph.IsValid()) {
                _graph.Evaluate(0.0f);
#if USE_ANIMATION_RIGGING
                if (_builder != null) {
                    _builder.UpdatePreviewGraph(_graph);
                    foreach (var layer in _builder.layers) {
                        layer.Update();
                    }
                }
#endif
            }
        }
    }
}