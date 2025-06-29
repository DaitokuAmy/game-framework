namespace GameFramework.Core {
    /// <summary>
    /// メルセンヌ・ツイスタアルゴリズムによる高品質乱数生成器
    /// </summary>
    public class RichRandom {
        private const int N = 624;
        private const int M = 397;
        private const uint MatrixA = 0x9908B0DF;
        private const uint UpperMask = 0x80000000;
        private const uint LowerMask = 0x7FFFFFFF;

        private readonly uint[] _mt = new uint[N];
        private int _mti = N + 1;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="seed">シード値</param>
        public RichRandom(int seed) {
            SetSeed(seed);
        }

        /// <summary>
        /// シード値の設定
        /// </summary>
        /// <param name="seed">シード値</param>
        public void SetSeed(int seed) {
            _mt[0] = (uint)seed;
            for (_mti = 1; _mti < N; _mti++) {
                var prev = _mt[_mti - 1];
                _mt[_mti] = (uint)(1812433253 * (prev ^ (prev >> 30)) + _mti);
            }
        }

        /// <summary>
        /// 次の乱数を返す
        /// </summary>
        public int Next() {
            uint y;

            if (_mti >= N) {
                for (var i = 0; i < N - M; i++) {
                    var a = _mt[i];
                    var b = _mt[i + 1];
                    y = (a & UpperMask) | (b & LowerMask);
                    _mt[i] = _mt[i + M] ^ (y >> 1) ^ ((y & 1) == 0 ? 0U : MatrixA);
                }

                for (var i = N - M; i < N - 1; i++) {
                    var a = _mt[i];
                    var b = _mt[i + 1];
                    y = (a & UpperMask) | (b & LowerMask);
                    _mt[i] = _mt[i + (M - N)] ^ (y >> 1) ^ ((y & 1) == 0 ? 0U : MatrixA);
                }

                var last = (_mt[N - 1] & UpperMask) | (_mt[0] & LowerMask);
                _mt[N - 1] = _mt[M - 1] ^ (last >> 1) ^ ((last & 1) == 0 ? 0U : MatrixA);

                _mti = 0;
            }

            y = _mt[_mti++];

            // Tempering
            y ^= y >> 11;
            y ^= (y << 7) & 0x9D2C5680;
            y ^= (y << 15) & 0xEFC60000;
            y ^= y >> 18;

            return (int)(y & 0x7FFFFFFF);
        }

        /// <summary>
        /// 整数型の範囲指定乱数
        /// </summary>
        /// <param name="min">最小値</param>
        /// <param name="max">この値を含まない最大値</param>
        public int Range(int min, int max) {
            var val = (uint)Next();
            return (int)((uint)min + val % (uint)(max - min));
        }

        /// <summary>
        /// 小数型の範囲指定乱数
        /// </summary>
        /// <param name="min">最小値</param>
        /// <param name="max">最大値</param>
        public float Range(float min, float max) {
            var val = (float)Next() / int.MaxValue;
            return FloatMath.Lerp(min, max, val);
        }
    }
}
