using System;
using GameFramework.BootSystem;

namespace GameFramework.Tests {
    /// <summary>
    /// BootSystemTests 用の Starter
    /// </summary>
    public sealed class TestBootMainSystemStarter : MainSystemStarter {
        public object[] Arguments { get; set; } = Array.Empty<object>();
        public int StartInternalCount { get; private set; }

        public override object[] GetArguments() {
            return Arguments;
        }

        protected override void StartInternal() {
            StartInternalCount++;
        }
    }
}
