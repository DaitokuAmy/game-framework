using UnityEngine;

namespace GameFramework.Core {
    /// <summary>
    /// 乱数計算用(XorShift)
    /// </summary>
    public class FastRandom {        
        private uint _x;
    
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="seed">シード値</param>
        public FastRandom(int seed) {
            SetSeed(seed);
        }

        /// <summary>
        /// シード値の設定
        /// </summary>
        /// <param name="seed">シード値</param>
        public void SetSeed(int seed) {
            _x = 0xffff0000 | (uint)(seed & 0xffff);
        }

        /// <summary>
        /// 乱数の進行
        /// </summary>
        public int Next() {
            _x ^= _x << 13;
            _x ^= _x >> 17;
            _x ^= _x << 5;
            return (int)_x;
        }

        /// <summary>
        /// 整数型の範囲指定乱数
        /// </summary>
        /// <param name="min">最小値</param>
        /// <param name="max">この値を含まない最大値</param>
        public int Range(int min, int max) {
            Next();
            return (int)((uint)min + _x % (uint)(max - min));
        }

        /// <summary>
        /// 小数型の範囲指定乱数
        /// </summary>
        /// <param name="min">最小値</param>
        /// <param name="max">最大値</param>
        public float Range(float min, float max) {
            Next();
            var t = (float)_x / uint.MaxValue;
            return Mathf.Lerp(min, max, t);
        }
    }
}