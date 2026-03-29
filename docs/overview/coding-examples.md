# Coding Examples

このファイルは [coding-guidelines.md](/d:/GitHub/Private/unity-mobile-haptics/docs/overview/coding-guidelines.md) の補助サンプルです。  
実装例は `.editorconfig` を意識した書き方の叩き台として管理します。

## Class Example

```csharp
namespace Project.Sample {
    /// <summary>
    /// サンプルの実装ルールを説明するクラス
    /// 命名、修飾子、コメント配置の例をまとめたもの
    /// </summary>
    public sealed class SampleInteractor : IDisposable, ISampleUseCase {
        /// <summary>変更通知デリゲート</summary>
        public delegate void ValueChangedHandler(int value);

        /// <summary>既定容量</summary>
        private const int DefaultCapacity = 8;

        /// <summary>最大容量</summary>
        private static readonly int MaxCapacity = 64;

        /// <summary>
        /// 内部状態
        /// </summary>
        private enum State {
            /// <summary>待機中</summary>
            Idle,
            /// <summary>実行中</summary>
            Running,
        }

        /// <summary>
        /// 補助処理用クラス
        /// </summary>
        private sealed class InternalHelper {
        }

        private static int s_globalCounter;

        private readonly InternalHelper _helper = new();

        [SerializeField, Tooltip("設定値")]
        private int _settingValue;

        private int _currentValue;
        private int _previousValue;

        /// <summary>現在値</summary>
        public int CurrentValue => _currentValue;
        /// <summary>直前値</summary>
        public int PreviousValue => _previousValue;

        /// <summary>値変更時に発火されるイベント</summary>
        public event Action Changed;
        /// <summary>値変更時に発火されるデリゲートイベント</summary>
        public event ValueChangedHandler ValueChanged;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SampleInteractor() {
        }

        /// <summary>
        /// 終了処理
        /// </summary>
        public void Dispose() {
        }

        /// <summary>
        /// 全体カウンタを初期化
        /// </summary>
        private static void ResetGlobalCounter() {
            s_globalCounter = 0;
        }

        /// <inheritdoc/>
        void ISampleUseCase.Execute() {
            OnValueChanged();
        }

        /// <summary>
        /// 値変更時の共通処理
        /// </summary>
        protected virtual void OnValueChanged() {
            ApplyValue();
        }

        /// <inheritdoc/>
        public override string ToString() {
            return _currentValue.ToString();
        }

        /// <summary>
        /// 値を更新
        /// </summary>
        /// <param name="nextValue">更新後の値</param>
        /// <param name="raiseEvent">イベントを発火する場合は true</param>
        public void SetValue(int nextValue, bool raiseEvent) {
            _previousValue = _currentValue;
            _currentValue = nextValue;

            if (raiseEvent) {
                Changed?.Invoke();
                ValueChanged?.Invoke(_currentValue);
            }
        }

        /// <summary>
        /// 値をリセット
        /// </summary>
        internal void Reset() {
            _previousValue = _currentValue;
            _currentValue = 0;
        }

        /// <summary>
        /// 設定値を適用
        /// </summary>
        private void ApplyValue() {
            if (_settingValue > 0) {
                _currentValue = _settingValue;
            }
        }
    }
}
```

同じブロックに属する宣言は空行を挟まず連続で配置します。  
別ブロックの宣言をまたぐ場合だけ空行を入れます。  
例: `const` と `static readonly`、`private readonly` フィールドと `private` フィールド、`[SerializeField] private` フィールドと通常の `private` フィールド、プロパティと `event` は別ブロックです。
