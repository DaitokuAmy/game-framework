using System;
using System.Collections.Generic;

namespace GameFramework.Core {
    /// <summary>
    /// Service状況監視されるためのインターフェース
    /// </summary>
    public interface IMonitoredServiceContainer {
        /// <summary>表示用ラベル</summary>
        string Label { get; }
        /// <summary>子階層のServiceContainer</summary>
        IReadOnlyList<IMonitoredServiceContainer> Children { get; }

        /// <summary>
        /// 登録済みサービス情報の取得
        /// </summary>
        void GetRegisteredServiceInfos(List<(Type type, object instance)> list);
    }
}