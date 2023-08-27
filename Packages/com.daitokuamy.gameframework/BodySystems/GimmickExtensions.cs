namespace GameFramework.BodySystems {
    /// <summary>
    /// Gimmick用の拡張メソッド
    /// </summary>
    public static class GimmickExtensions {
        /// <summary>
        /// ActiveGimmickを取得
        /// </summary>
        public static ActiveGimmick[] GetActiveGimmicks(this GimmickController source, string key) {
            return source.GetGimmicks<ActiveGimmick>(key);
        }

        /// <summary>
        /// Activate操作
        /// </summary>
        public static void Activate(this ActiveGimmick[] source) {
            foreach (var gimmick in source) {
                gimmick.Activate();
            }
        }

        /// <summary>
        /// Deactivate操作
        /// </summary>
        public static void Deactivate(this ActiveGimmick[] source) {
            foreach (var gimmick in source) {
                gimmick.Deactivate();
            }
        }

        /// <summary>
        /// AnimationGimmickを取得
        /// </summary>
        public static AnimationGimmick[] GetAnimationGimmicks(this GimmickController source, string key) {
            return source.GetGimmicks<AnimationGimmick>(key);
        }

        /// <summary>
        /// Play操作
        /// </summary>
        public static void Play(this AnimationGimmick[] source, bool reverse = false) {
            foreach (var gimmick in source) {
                gimmick.Play(reverse);
            }
        }

        /// <summary>
        /// Resume操作
        /// </summary>
        public static void Resume(this AnimationGimmick[] source, bool reverse = false) {
            foreach (var gimmick in source) {
                gimmick.Resume(reverse);
            }
        }

        /// <summary>
        /// InvokeGimmickを取得
        /// </summary>
        public static InvokeGimmick[] GetInvokeGimmicks(this GimmickController source, string key) {
            return source.GetGimmicks<InvokeGimmick>(key);
        }

        /// <summary>
        /// Invoke操作
        /// </summary>
        public static void Invoke(this InvokeGimmick[] source) {
            foreach (var gimmick in source) {
                gimmick.Invoke();
            }
        }

        /// <summary>
        /// ChangeGimmickを取得
        /// </summary>
        public static ChangeGimmick<T>[] GetChangeGimmicks<T>(this GimmickController source, string key) {
            return source.GetGimmicks<ChangeGimmick<T>>(key);
        }

        /// <summary>
        /// Change操作
        /// </summary>
        public static void Change<T>(this ChangeGimmick<T>[] source, T val, float duration = 0.0f) {
            foreach (var gimmick in source) {
                gimmick.Change(val, duration);
            }
        }

        /// <summary>
        /// StateGimmickを取得
        /// </summary>
        public static StateGimmick[] GetStateGimmicks(this GimmickController source, string key) {
            return source.GetGimmicks<StateGimmick>(key);
        }

        /// <summary>
        /// Change操作
        /// </summary>
        public static void Change(this StateGimmick[] source, string stateName) {
            foreach (var gimmick in source) {
                gimmick.Change(stateName);
            }
        }
    }
}