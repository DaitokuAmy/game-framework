namespace GameFramework.ActorSystems {
    /// <summary>
    /// Body用の拡張メソッド
    /// </summary>
    public static class BodyExtensions {
        /// <summary>
        /// 見た目のLayerを変更
        /// </summary>
        /// <param name="source">制御対象</param>
        /// <param name="layer">設定するレイヤー</param>
        /// <param name="setRoot">ルートのGameObjectも反映させるか</param>
        public static void SetVisualLayer(this Body source, int layer, bool setRoot = true) {
            var meshComponent = source.GetBodyComponent<MeshComponent>();
            if (meshComponent != null) {
                meshComponent.SetLayer(layer);
            }

            if (setRoot) {
                source.GameObject.layer = layer;
            }
        }
    }
}