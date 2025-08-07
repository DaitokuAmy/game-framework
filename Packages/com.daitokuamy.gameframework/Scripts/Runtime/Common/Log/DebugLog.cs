using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace GameFramework {
    /// <summary>
    /// デバッグログ
    /// </summary>
    public static class DebugLog {
        /// <summary>ログを利用する条件となるDefineSymbol</summary>
        private const string LogConditionalString = "GF_DEBUG";

        private static readonly List<ILogger> Loggers = new() { Debug.unityLogger };

        /// <summary>
        /// ロガーの追加
        /// </summary>
        public static void AddLogger(ILogger logger) {
            if (logger == null) {
                return;
            }

            Loggers.Add(logger);
        }

        /// <summary>
        /// ロガーの削除
        /// </summary>
        public static void RemoveLogger(ILogger logger) {
            if (logger == null) {
                return;
            }

            Loggers.Remove(logger);
        }

        [Conditional(LogConditionalString)]
        public static void Info(string tag, object message, Object context = null) {
            foreach (var logger in Loggers) {
                logger.Log(LogType.Log, tag, message, context);
            }
        }

        [Conditional(LogConditionalString)]
        public static void Info(object message, Object context = null) {
            foreach (var logger in Loggers) {
                logger.Log(LogType.Log, message, context);
            }
        }

        [Conditional(LogConditionalString)]
        public static void InfoFormat(string format, params object[] args) {
            foreach (var logger in Loggers) {
                logger.LogFormat(LogType.Log, format, args);
            }
        }

        [Conditional(LogConditionalString)]
        public static void InfoFormat(Object context, string format, params object[] args) {
            foreach (var logger in Loggers) {
                logger.LogFormat(LogType.Log, context, format, args);
            }
        }

        [Conditional(LogConditionalString)]
        public static void Warning(string tag, object message, Object context = null) {
            foreach (var logger in Loggers) {
                logger.Log(LogType.Warning, tag, message, context);
            }
        }

        [Conditional(LogConditionalString)]
        public static void Warning(object message, Object context = null) {
            foreach (var logger in Loggers) {
                logger.Log(LogType.Warning, message, context);
            }
        }

        [Conditional(LogConditionalString)]
        public static void WarningFormat(string format, params object[] args) {
            foreach (var logger in Loggers) {
                logger.LogFormat(LogType.Warning, format, args);
            }
        }

        [Conditional(LogConditionalString)]
        public static void WarningFormat(Object context, string format, params object[] args) {
            foreach (var logger in Loggers) {
                logger.LogFormat(LogType.Warning, context, format, args);
            }
        }

        [Conditional(LogConditionalString)]
        public static void Error(string tag, object message, Object context) {
            foreach (var logger in Loggers) {
                logger.LogError(tag, message, context);
            }
        }

        [Conditional(LogConditionalString)]
        public static void Error(object message, Object context = null) {
            foreach (var logger in Loggers) {
                logger.Log(LogType.Error, message, context);
            }
        }

        [Conditional(LogConditionalString)]
        public static void ErrorFormat(string format, params object[] args) {
            foreach (var logger in Loggers) {
                logger.LogFormat(LogType.Error, format, args);
            }
        }

        [Conditional(LogConditionalString)]
        public static void ErrorFormat(Object context, string format, params object[] args) {
            foreach (var logger in Loggers) {
                logger.LogFormat(LogType.Error, context, format, args);
            }
        }

        [Conditional(LogConditionalString)]
        public static void Exception(Exception exception, Object context = null) {
            foreach (var logger in Loggers) {
                logger.LogException(exception, context);
            }
        }
    }
}