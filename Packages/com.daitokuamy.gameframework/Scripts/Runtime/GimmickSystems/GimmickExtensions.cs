namespace GameFramework.GimmickSystems {
    /// <summary>
    /// Gimmick用の拡張メソッド
    /// </summary>
    public static class GimmickExtensions {
        /// <summary>
        /// Activate操作
        /// </summary>
        /// <param name="source">操作対象</param>
        /// <param name="immediate">即時反映か</param>
        public static void Activate(this ActiveGimmick[] source, bool immediate = false) {
            foreach (var gimmick in source) {
                gimmick.Activate(immediate);
            }
        }

        /// <summary>
        /// Deactivate操作
        /// </summary>
        /// <param name="source">操作対象</param>
        /// <param name="immediate">即時反映か</param>
        public static void Deactivate(this ActiveGimmick[] source, bool immediate = false) {
            foreach (var gimmick in source) {
                gimmick.Deactivate(immediate);
            }
        }

        /// <summary>
        /// Play操作
        /// </summary>
        /// <param name="source">操作対象</param>
        /// <param name="reverse">反転再生するか</param>
        /// <param name="immediate">即時反映するか</param>
        public static void Play(this AnimationGimmick[] source, bool reverse = false, bool immediate = false) {
            foreach (var gimmick in source) {
                gimmick.Play(reverse, immediate);
            }
        }

        /// <summary>
        /// Resume操作
        /// </summary>
        /// <param name="source">操作対象</param>
        /// <param name="reverse">反転再生するか</param>
        public static void Resume(this AnimationGimmick[] source, bool reverse = false) {
            foreach (var gimmick in source) {
                gimmick.Resume(reverse);
            }
        }

        /// <summary>
        /// Invoke操作
        /// </summary>
        /// <param name="source">操作対象</param>
        public static void Invoke(this InvokeGimmick[] source) {
            foreach (var gimmick in source) {
                gimmick.Invoke();
            }
        }

        /// <summary>
        /// Change操作
        /// </summary>
        /// <param name="source">操作対象</param>
        /// <param name="val">設定する値</param>
        /// <param name="duration">反映にかける時間</param>
        public static void Change<T>(this ChangeGimmick<T>[] source, T val, float duration = 0.0f) {
            foreach (var gimmick in source) {
                gimmick.Change(val, duration);
            }
        }

        /// <summary>
        /// Change操作
        /// </summary>
        /// <param name="source">操作対象</param>
        /// <param name="stateName">ステート名</param>
        /// <param name="immediate">即時遷移するか</param>
        public static void Change(this StateGimmick[] source, string stateName, bool immediate = false) {
            foreach (var gimmick in source) {
                gimmick.Change(stateName, immediate);
            }
        }
    }
}