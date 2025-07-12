using System;
using UnityEngine;

namespace GameFramework.ActorSystems {
    /// <summary>
    /// Collider情報保持クラス
    /// </summary>
    [DisallowMultipleComponent]
    public class ColliderParts : MonoBehaviour {
        // ユニーク骨情報
        [Serializable]
        public class Info {
            [Tooltip("コライダーを取得する際のキー")]
            public string key;
            [Tooltip("コライダーリスト")]
            public Collider[] colliders;
        }

        [Tooltip("コライダーの登録情報")]
        public Info[] infos;
    }
}