using System.Collections.Generic;
using System.Linq;
using GameFramework.Core;
using UniRx;
using UnityEditor;
using UnityEngine;

namespace SampleGame.ModelViewer.Editor {
    /// <summary>
    /// ModelViewerのAvatarパネル
    /// </summary>
    partial class ModelViewerWindow {
        /// <summary>
        /// AvatarPanel
        /// </summary>
        private class AvatarPanel : PanelBase {
            public override string Title => "Avatar";

            private Dictionary<string, FoldoutList<GameObject>> _meshAvatarFoldoutLists = new();
            private Dictionary<string, GameObject[]> _meshAvatarPrefabLists = new();

            /// <summary>
            /// 初期化処理
            /// </summary>
            protected override void InitializeInternal(IScope scope) {
                var model = ModelViewerModel.Get().PreviewActorModel;
                model.CurrentMeshAvatars
                    .ObserveAdd()
                    .TakeUntil(scope)
                    .Subscribe(x => {
                        var list = new FoldoutList<GameObject>(x.Key);
                        _meshAvatarFoldoutLists[x.Key] = list;
                        _meshAvatarPrefabLists[x.Key] = model.SetupData.Value.meshAvatarInfos
                            .First(y => y.key == x.Key).prefabs
                            .Concat(new GameObject[] { null })
                            .ToArray();
                    });
                model.CurrentMeshAvatars
                    .ObserveRemove()
                    .TakeUntil(scope)
                    .Subscribe(x => {
                        _meshAvatarFoldoutLists.Remove(x.Key);
                        _meshAvatarPrefabLists.Remove(x.Key);
                    });
                model.CurrentMeshAvatars
                    .ObserveReset()
                    .TakeUntil(scope)
                    .Subscribe(x => {
                        _meshAvatarFoldoutLists.Clear();
                        _meshAvatarPrefabLists.Clear();
                    });
                
                foreach (var pair in model.CurrentMeshAvatars) {
                    var list = new FoldoutList<GameObject>(pair.Key);
                    _meshAvatarFoldoutLists[pair.Key] = list;
                    _meshAvatarPrefabLists[pair.Key] = model.SetupData.Value.meshAvatarInfos
                        .First(y => y.key == pair.Key).prefabs
                        .Concat(new GameObject[] { null })
                        .ToArray();
                }
            }

            /// <summary>
            /// GUI描画
            /// </summary>
            protected override void OnGUIInternal() {
                var model = ModelViewerModel.Get().PreviewActorModel;

                var prevColor = GUI.color;

                var keys = _meshAvatarFoldoutLists.Keys.ToArray();
                foreach (var key in keys) {
                    if (!_meshAvatarFoldoutLists.TryGetValue(key, out var list)) {
                        continue;
                    }

                    GUI.color = prevColor;
                    
                    var prefabs = _meshAvatarPrefabLists[key];
                    list.OnGUI(prefabs, (prefab, index) => {
                        model.CurrentMeshAvatars.TryGetValue(key, out var currentPrefab);
                        var current = currentPrefab == prefab;
                        GUI.color = current ? Color.green : Color.gray;
                        
                        if (prefab != null) {
                            if (GUILayout.Button(prefab.name)) {
                                model.ChangeMeshAvatar(key, index);
                            }
                        }
                        else {
                            if (GUILayout.Button("Remove")) {
                                model.ChangeMeshAvatar(key, -1);
                            }
                        }
                    });
                }

                GUI.color = prevColor;
            }
        }
    }
}