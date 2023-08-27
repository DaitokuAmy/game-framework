using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameFramework.BodySystems {
    /// <summary>
    /// Transform管理クラス
    /// </summary>
    public class ParentController : BodyController {
        // スケール適用タイプ
        public enum ScaleType {
            Self, // 自身のScaleを適用
            ParentBody, // 親のScaleを適用
            ParentTransform, // 接続したTransformのScaleを適用
        }

        // 親のParentController
        private ParentController _parent;
        // 子のParentControllerリスト
        private List<ParentController> _children = new List<ParentController>();

        // 追従するための各種情報
        private Transform _targetTransform;
        private Vector3 _offsetPosition;
        private Quaternion _offsetRotation;
        private ScaleType _scaleType;

        // 処理順番
        public override int ExecutionOrder => 1000;

        /// <summary>
        /// 親の設定
        /// </summary>
        /// <param name="parentBody">親のBody</param>
        /// <param name="targetTransform">追従Transform(nullだとparentのroot)</param>
        /// <param name="offsetPosition">オフセット座標(Local)</param>
        /// <param name="offsetRotation">オフセット回転(Local)</param>
        /// <param name="scaleType">スケール反映タイプ</param>
        public void SetParent(Body parentBody, Transform targetTransform, Vector3 offsetPosition,
            Quaternion offsetRotation, ScaleType scaleType) {
            if (_parent != null) {
                // 既に接続されていた場合、接続を解除
                _parent.RemoveChild(this);
                _parent = null;
                _targetTransform = null;
            }

            if (parentBody == null) {
                return;
            }

            var parent = parentBody.GetController<ParentController>();
            if (parent == null) {
                return;
            }

            // 追従設定初期化
            _targetTransform = targetTransform != null ? targetTransform : parentBody.Transform;
            _offsetPosition = offsetPosition;
            _offsetRotation = offsetRotation;
            _scaleType = scaleType;

            // 子として設定
            parent.AddChild(this);
            _parent = parent;

            // RootからTransformを再更新
            GetRoot().UpdateTransform();
        }

        /// <summary>
        /// 子のBodyを取得
        /// </summary>
        /// <param name="recursive">再帰的に取得するか</param>
        public Body[] GetChildren(bool recursive = true) {
            if (recursive) {
                var children = new List<Body>();
                foreach (var child in _children) {
                    children.Add(child.Body);
                    children.AddRange(child.GetChildren());
                }

                return children.ToArray();
            }

            return _children.Select(x => x.Body).ToArray();
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        protected override void LateUpdateInternal(float deltaTime) {
            if (_parent != null) {
                // 親が無効になっていたら外れる
                if (!_parent.IsValid) {
                    SetParent(null, null, Vector3.zero, Quaternion.identity, ScaleType.Self);
                }

                // 有効な親が存在する場合、更新は親に任せる
                if (IsActiveParent()) {
                    return;
                }
            }

            // Transformの更新
            UpdateTransform();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DisposeInternal() {
            // 子要素の追従を全部クリアする
            foreach (var child in _children) {
                if (!child.IsValid) {
                    continue;
                }

                child.RemoveChild(null);
            }

            _children.Clear();
        }

        /// <summary>
        /// Transformの再帰的な更新処理
        /// </summary>
        private void UpdateTransform() {
            // Transform更新
            var parentBody = _parent?.Body;
            if (parentBody != null && parentBody.IsValid) {
                var parentPosition = _targetTransform.position;
                var parentRotation = _targetTransform.rotation;
                var position = parentRotation * (_offsetPosition * parentBody.BaseScale);
                var rotation = parentRotation * _offsetRotation;
                Body.Position = position + parentPosition;
                Body.Rotation = rotation;

                switch (_scaleType) {
                    case ScaleType.Self:
                        break;

                    case ScaleType.ParentBody:
                        Body.BaseScale = parentBody.BaseScale;
                        break;

                    case ScaleType.ParentTransform:
                        Body.LocalScale = _targetTransform.lossyScale;
                        break;
                }
            }

            // 子供の更新を呼び出す
            foreach (var child in _children) {
                if (!child.IsValid) {
                    continue;
                }

                child.UpdateTransform();
            }
        }

        /// <summary>
        /// ParentのRootになっているParentControllerを取得
        /// </summary>
        private ParentController GetRoot() {
            var root = this;
            while (root._parent != null) {
                root = root._parent;
            }

            return root;
        }

        /// <summary>
        /// 親が生きているか
        /// </summary>
        private bool IsActiveParent() {
            var root = this;
            while (root._parent != null) {
                root = root._parent;
                if (root.IsValid && root.Body.IsActive) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 子を追加
        /// </summary>
        /// <param name="child">追加対象のParentController</param>
        private void AddChild(ParentController child) {
            if (!_children.Contains(child)) {
                _children.Add(child);
            }
        }

        /// <summary>
        /// 子を外す
        /// </summary>
        /// <param name="child">外す対象のParentController</param>
        private void RemoveChild(ParentController child) {
            _children.Remove(child);
        }
    }
}