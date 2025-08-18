using System;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.Core;

namespace GameFramework {
    /// <summary>
    /// 時間の階層管理用クラス
    /// </summary>
    public class LayeredTime : IReadOnlyLayeredTime, IDisposable {
        private readonly List<LayeredTime> _children = new(32);

        /// <summary>
        /// Unity用のDeltaTimeProvider
        /// </summary>
        private class UnityDeltaTimeProvider : IDeltaTimeProvider {
            float IDeltaTimeProvider.DeltaTime => Time.deltaTime;
        }

        private readonly IDeltaTimeProvider _deltaTimeProvider;

        private float _localTimeScale = 1.0f;

        /// <summary>親要素</summary>
        public IReadOnlyLayeredTime Parent { get; private set; }
        /// <summary>親階層を考慮しないTimeScale</summary>
        public float LocalTimeScale {
            get => _localTimeScale;
            set {
                var clamped = FloatMath.Max(value, 0.0f);
                if (FloatMath.Approximately(_localTimeScale, clamped)) {
                    return;
                }

                _localTimeScale = FloatMath.Max(0.0f, clamped);
                OnChangedTimeScale(TimeScale);
            }
        }
        /// <summary>親階層を考慮したTimeScale</summary>
        public float TimeScale => ParentTimeScale * _localTimeScale;
        /// <summary>親のTimeScale</summary>
        public float ParentTimeScale => Parent?.TimeScale ?? 1.0f;
        /// <summary>現フレームのDeltaTime</summary>
        public float DeltaTime => BaseDeltaTime * TimeScale;
        /// <summary>親のDeltaTime</summary>
        public float ParentDeltaTime => BaseDeltaTime * ParentTimeScale;
        /// <summary>ベースとなるDeltaTime</summary>
        private float BaseDeltaTime => _deltaTimeProvider.DeltaTime;

        /// <summary>TimeScaleの変更通知</summary>
        public event Action<float> ChangedTimeScaleEvent;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="parent">親となるTimeLayer, 未指定の場合UnityEngine.Timeに直接依存</param>
        /// <param name="deltaTimeProvider">基礎となるDeltaTimeを提供するクラス、nullだとTime.deltaTime</param>
        public LayeredTime(LayeredTime parent = null, IDeltaTimeProvider deltaTimeProvider = null) {
            _deltaTimeProvider = deltaTimeProvider ?? new UnityDeltaTimeProvider();
            SetParent(parent);
        }

        /// <summary>
        /// 親のTimeLayerの設定
        /// </summary>
        /// <param name="parent">親となるTimeLayer, 未指定の場合UnityEngine.Timeに直接依存</param>
        public void SetParent(IReadOnlyLayeredTime parent) {
            if (IsDescendantOf(parent)) {
                throw new InvalidOperationException("Cannot set parent: Circular reference detected.");
            }

            if (Parent != null) {
                ((LayeredTime)Parent)._children.Remove(this);
                Parent = null;
            }

            Parent = parent;

            if (Parent != null) {
                ((LayeredTime)Parent)._children.Add(this);
            }

            OnChangedParentTimeScale(ParentTimeScale);
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            if (Parent != null) {
                ((LayeredTime)Parent)._children.Remove(this);
                Parent = null;
            }

            foreach (var child in _children) {
                child.Parent = null;
            }

            _children.Clear();
        }

        /// <summary>
        /// 該当のLayeredTimeが自身に含まれているか
        /// </summary>
        private bool IsDescendantOf(IReadOnlyLayeredTime target) {
            if (target == null) {
                return false;
            }

            if (target == this) {
                return true;
            }

            foreach (var child in _children) {
                if (child.IsDescendantOf(target)) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 親のTimeScale変換通知
        /// </summary>
        private void OnChangedParentTimeScale(float timeScale) {
            timeScale *= _localTimeScale;

            for (var i = 0; i < _children.Count; i++) {
                _children[i].OnChangedParentTimeScale(timeScale);
            }

            ChangedTimeScaleEvent?.Invoke(timeScale);
        }

        /// <summary>
        /// TimeScale変更通知
        /// </summary>
        private void OnChangedTimeScale(float timeScale) {
            for (var i = 0; i < _children.Count; i++) {
                _children[i].OnChangedParentTimeScale(timeScale);
            }

            ChangedTimeScaleEvent?.Invoke(timeScale);
        }
    }
}