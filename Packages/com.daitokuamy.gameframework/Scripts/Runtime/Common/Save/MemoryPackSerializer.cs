#if USE_MEMORY_PACK
using System;

namespace GameFramework {
    /// <summary>
    /// MemoryPackを使ったシリアライザー
    /// </summary>
    public sealed class MemoryPackSerializer : ISerializer {
        /// <summary>
        /// シリアライズ処理
        /// </summary>
        byte[] ISerializer.Serialize<T>(T data) {
            return MemoryPack.MemoryPackSerializer.Serialize(data);
        }

        /// <summary>
        /// デシリアライズ処理
        /// </summary>
        T ISerializer.Deserialize<T>(byte[] bytes) {
            if (bytes == null || bytes.Length == 0) {
                throw new ArgumentException("Serialized data is empty.", nameof(bytes));
            }

            return MemoryPack.MemoryPackSerializer.Deserialize<T>(bytes);
        }
    }
}
#endif
