using System;
using System.Collections.Generic;
using System.Linq;
using GameFramework.Core;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace GameFramework.UISystems {
    /// <summary>
    /// UIViewのPool管理するためのクラス
    /// </summary>
    public class UIViewPool<TView> : IDisposable
        where TView : UIView {
        private class ActiveInfo {
            public TView View;
            public DisposableScope Scope;
        }

        private readonly List<ActiveInfo> _activeInfos = new();
        private readonly ObjectPool<TView> _viewPool;
        private readonly ObjectPool<ActiveInfo> _activeInfoPool;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public UIViewPool(TView template, Func<TView, TView> instantiateFunc) {
            _activeInfoPool = new ObjectPool<ActiveInfo>(() => new ActiveInfo {
                View = null,
                Scope = new DisposableScope()
            });
            _viewPool = new ObjectPool<TView>(
                createFunc: () => {
                    template.gameObject.SetActive(true);
                    var view = instantiateFunc.Invoke(template);
                    view.transform.SetParent(template.transform.parent, false);
                    view.transform.localPosition = Vector3.zero;
                    view.transform.localRotation = Quaternion.identity;
                    view.transform.localScale = Vector3.one;
                    template.gameObject.SetActive(false);
                    view.gameObject.SetActive(false);
                    return view;
                }, actionOnGet: view => {
                    view.gameObject.SetActive(true);
                    view.transform.SetAsLastSibling();
                },
                actionOnRelease: view => view.gameObject.SetActive(false),
                actionOnDestroy: view => { Object.Destroy(view.gameObject); });
            template.gameObject.SetActive(false);
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            Clear();
        }

        /// <summary>
        /// アクティブビューリストの取得
        /// </summary>
        public void GetActiveViews(List<(TView, int)> activeVies) {
            activeVies.Clear();
            activeVies.AddRange(_activeInfos.Select((x, idx) => (x.View, idx)));
        }

        /// <summary>
        /// プールへのから取得
        /// </summary>
        public TView Get(Action<TView, int, IScope> addedAction = null) {
            var activeInfo = _activeInfoPool.Get();
            var view = _viewPool.Get();
            activeInfo.View = view;
            _activeInfos.Add(activeInfo);

            var index = _activeInfos.Count - 1;
            addedAction?.Invoke(view, index, activeInfo.Scope);
            return view;
        }

        /// <summary>
        /// プールへ返却
        /// </summary>
        public void Release(TView view) {
            for (var i = 0; i < _activeInfos.Count; i++) {
                var info = _activeInfos[i];
                if (info.View != view) {
                    continue;
                }

                info.Scope.Clear();
                info.View = null;
                _activeInfos.RemoveAt(i);
                _viewPool.Release(view);
                _activeInfoPool.Release(info);
                break;
            }
        }

        /// <summary>
        /// 全削除
        /// </summary>
        public void Clear() {
            for (var i = 0; i < _activeInfos.Count; i++) {
                var activeInfo = _activeInfos[i];
                var view = activeInfo.View;
                activeInfo.Scope.Clear();
                activeInfo.View = null;
                _viewPool.Release(view);
                _activeInfoPool.Release(activeInfo);
            }

            _activeInfos.Clear();
        }
    }
}