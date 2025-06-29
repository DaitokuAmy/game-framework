namespace GameFramework {
    /// <summary>
    /// TableDataに保持される要素のインターフェース
    /// </summary>
    public interface ITableElement<out TKey> {
        TKey Id { get; }
    }
}