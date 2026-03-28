using System;
using System.Collections;
using GameFramework.BootSystem;

namespace GameFramework.Tests {
    /// <summary>
    /// BootSystemTests 用の MainSystem
    /// </summary>
    public sealed class TestBootMainSystem : MainSystemBase {
        public bool ThrowOnStart { get; set; }
        public bool ThrowOnReboot { get; set; }
        public object[] StartArguments { get; private set; }
        public object[] RebootArguments { get; private set; }
        public bool IsWarmingForTest => IsWarming;

        protected override IEnumerator StartRoutineInternal(object[] args) {
            StartArguments = args;
            if (ThrowOnStart) {
                throw new InvalidOperationException("start failed");
            }

            yield break;
        }

        protected override IEnumerator RebootRoutineInternal(object[] args) {
            RebootArguments = args;
            if (ThrowOnReboot) {
                throw new InvalidOperationException("reboot failed");
            }

            yield break;
        }
    }
}
