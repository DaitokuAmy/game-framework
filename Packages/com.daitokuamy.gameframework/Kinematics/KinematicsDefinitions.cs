using System;

namespace GameFramework.Kinematics {
    /// <summary>
    /// Transform制御ターゲット用マスク
    /// </summary>
    [Flags]
    public enum TransformMasks {
        Position = 1 << 0,
        Rotation = 1 << 1,
        Scale = 1 << 2,
    }

    /// <summary>
    /// 軸用マスク
    /// </summary>
    [Flags]
    public enum AxisMasks {
        X = 1 << 0,
        Y = 1 << 1,
        Z = 1 << 2,
    }

    /// <summary>
    /// 定数定義
    /// </summary>
    public static class KinematicsDefinitions {
        public const TransformMasks TransformMasksAll = TransformMasks.Position | TransformMasks.Rotation | TransformMasks.Scale;
        public const AxisMasks AxisMasksAll = AxisMasks.X | AxisMasks.Y | AxisMasks.Z;
    }
}