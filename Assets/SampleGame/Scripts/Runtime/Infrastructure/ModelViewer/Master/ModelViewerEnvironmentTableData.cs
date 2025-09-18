using System;
using GameFramework;
using SampleGame.Domain.ModelViewer;
using UnityEngine;

namespace SampleGame.Infrastructure.ModelViewer {
    /// <summary>
    /// ModelViewer用のアクターテーブルデータ
    /// </summary>
    [CreateAssetMenu(fileName = "dat_model_viewer_environment_table.asset", menuName = "Sample Game/Table Data/Model Viewer Environment")]
    public sealed class ModelViewerEnvironmentTableData : ScriptableTableData<int, ModelViewerEnvironmentTableData.Element> {
        /// <summary>
        /// 要素
        /// </summary>
        [Serializable]
        public class Element : ITableElement<int>, IModelViewerEnvironmentMaster {
            [SerializeField, Tooltip("識別子")]
            private int _id;
            [SerializeField, Tooltip("名称")]
            private string _name;
            [SerializeField, Tooltip("アセットキー")]
            private string _assetKey;

            /// <summary>識別子</summary>
            public int Id => _id;
            /// <inheritdoc/>
            public string Name => _name;
            /// <inheritdoc/>
            public string AssetKey => _assetKey;
        }
    }
}
