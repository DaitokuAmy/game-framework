using SampleGame.Domain;
using UnityEngine;

namespace SampleGame.Presentation {
    /// <summary>
    /// TransformをDomainに渡すためのクラス
    /// </summary>
    public class TransformProvider : ITransform {
        private readonly Vector3 _offsetPosition;
        private readonly Quaternion _offsetRotation;

        private Vector3 _position;
        private Quaternion _rotation;

        /// <summary>座標</summary>
        Vector3 ITransform.Position {
            get {
                RefreshCache();
                return _position + ((ITransform)this).Rotation * _offsetPosition;
            }
        }
        /// <summary>向き</summary>
        Quaternion ITransform.Rotation {
            get {
                RefreshCache();
                return _rotation * _offsetRotation;
            }
        }

        /// <summary>参照先のTransform</summary>
        public Transform Reference { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TransformProvider(Transform reference, Vector3 offsetPosition, Quaternion offsetRotation) {
            Reference = reference;
            _offsetPosition = offsetPosition;
            _offsetRotation = offsetRotation;
            RefreshCache();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TransformProvider(Transform reference)
            : this(reference, Vector3.zero, Quaternion.identity) {
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TransformProvider(Vector3 position, Quaternion rotation) {
            Reference = null;
            _position = position;
            _rotation = rotation;
            _offsetPosition = Vector3.zero;
            _offsetRotation = Quaternion.identity;
        }

        /// <summary>
        /// 値キャッシュのリフレッシュ
        /// </summary>
        private void RefreshCache() {
            if (Reference == null) {
                return;
            }

            _position = Reference.position;
            _rotation = Reference.rotation;
        }
    }
}