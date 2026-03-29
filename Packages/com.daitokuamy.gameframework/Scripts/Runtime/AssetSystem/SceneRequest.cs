namespace GameFramework.AssetSystem {
    /// <summary>
    /// シーン読み込み要求
    /// Additive読み込み専用
    /// </summary>
    public readonly struct SceneRequest : ISceneRequest {
        /// <summary>読み込み対象アドレス</summary>
        public string Address { get; }
        /// <summary>読み込み後にアクティブ化するか</summary>
        public bool ActivateOnLoad { get; }

        /// <summary>有効な要求か</summary>
        public bool IsValid => !string.IsNullOrEmpty(Address);

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SceneRequest(string address, bool activateOnLoad = true) {
            Address = address;
            ActivateOnLoad = activateOnLoad;
        }

        /// <summary>
        /// 文字列から読み込み要求へ変換
        /// </summary>
        public static implicit operator SceneRequest(string address) {
            return new SceneRequest(address);
        }

        /// <inheritdoc/>
        public override string ToString() {
            return Address ?? string.Empty;
        }
    }
}
