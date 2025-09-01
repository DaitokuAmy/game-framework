using System.Collections.Generic;
using System.Linq;
using GameFramework;
using GameFramework.Core;
using GameFramework.DebugSystems.Editor;
using R3;
using SampleGame.Application.ModelViewer;
using SampleGame.Domain.ModelViewer;
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
            public override string Label => "Avatar";

            private Dictionary<string, FoldoutList<GameObject>> _meshAvatarFoldoutLists = new();
            private Dictionary<string, GameObject[]> _meshAvatarPrefabLists = new();

            /// <summary>
            /// 初期化処理
            /// </summary>
            protected override void StartInternal(ModelViewerWindow window, IScope scope) {
                var viewerModel = window.Resolver.Resolve<ModelViewerAppService>().DomainService.ModelViewerModel;

                // Actor生成監視
                viewerModel.ChangedPreviewActorSubject
                    .TakeUntil(scope)
                    .Prepend(() => new ChangedPreviewActorDto { Model = viewerModel.PreviewActorModel })
                    .Subscribe(dto => {
                        var actorModel = dto.Model;
                        if (actorModel == null) {
                            _meshAvatarFoldoutLists.Clear();
                            _meshAvatarPrefabLists.Clear();
                            return;
                        }

                        foreach (var pair in actorModel.CurrentMeshAvatars) {
                            var list = new FoldoutList<GameObject>(pair.Key);
                            _meshAvatarFoldoutLists[pair.Key] = list;
                            _meshAvatarPrefabLists[pair.Key] = actorModel.Master.MeshAvatarInfos
                                .First(y => y.Key == pair.Key).Prefabs
                                .Concat(new GameObject[] { null })
                                .ToArray();
                        }
                    });

                // PreviewActor削除監視
                viewerModel.DeletedPreviewActorSubject
                    .TakeUntil(scope)
                    .Subscribe(dto => {
                        _meshAvatarFoldoutLists.Clear();
                        _meshAvatarPrefabLists.Clear();
                    });
            }

            /// <summary>
            /// GUI描画
            /// </summary>
            protected override void DrawGuiInternal(ModelViewerWindow window) {
                var appService = window.Resolver.Resolve<ModelViewerAppService>();
                var actorModel = appService.DomainService.PreviewActorModel;
                var prevColor = GUI.color;

                var keys = _meshAvatarFoldoutLists.Keys.ToArray();
                foreach (var key in keys) {
                    if (!_meshAvatarFoldoutLists.TryGetValue(key, out var list)) {
                        continue;
                    }

                    GUI.color = prevColor;

                    var prefabs = _meshAvatarPrefabLists[key];
                    list.OnGUI(prefabs, (prefab, index) => {
                        actorModel.CurrentMeshAvatars.TryGetValue(key, out var currentPrefab);
                        var current = currentPrefab == prefab;
                        GUI.color = current ? Color.green : Color.gray;

                        if (prefab != null) {
                            if (GUILayout.Button(prefab.name)) {
                                appService.ChangeMeshAvatar(key, index);
                            }
                        }
                        else {
                            if (GUILayout.Button("Remove")) {
                                appService.ChangeMeshAvatar(key, -1);
                            }
                        }
                    });
                }

                GUI.color = prevColor;
            }
        }
    }
}