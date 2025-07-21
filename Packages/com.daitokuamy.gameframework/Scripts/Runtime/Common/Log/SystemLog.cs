using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameFramework {
    /// <summary>
    /// システムログ
    /// </summary>
    public static class SystemLog {
        private static readonly List<ILogHandler> LogHandlers = new() { Debug.unityLogger };

        /// <summary>
        /// ログ出力用のハンドラーを追加
        /// </summary>
        public static void AddHandler(ILogger handler) {
            if (handler == null) {
                return;
            }

            LogHandlers.Add(handler);
        }

        /// <summary>
        /// ログ出力用のハンドラーを削除
        /// </summary>
        public static void RemoveHandler(ILogger handler) {
            if (handler == null) {
                return;
            }

            LogHandlers.Remove(handler);
        }

        public static void Info(string tag, object message, Object context = null) {
            object output = GetString(message);
            foreach (var handler in LogHandlers) {
                handler.LogFormat(LogType.Log, context, "{0}: {1}", tag, output);
            }
        }

        public static void Info(object message, Object context = null) {
            object output = GetString(message);
            foreach (var handler in LogHandlers) {
                handler.LogFormat(LogType.Log, context, "{0}", output);
            }
        }
        
        public static void InfoFormat(string format, params object[] args) {
            foreach (var handler in LogHandlers) {
                handler.LogFormat(LogType.Log, null, format, args);
            }
        }
        
        public static void InfoFormat(Object context, string format, params object[] args) {
            foreach (var handler in LogHandlers) {
                handler.LogFormat(LogType.Log, context, format, args);
            }
        }
        
        public static void Warning(string tag, object message, Object context = null) {
            object output = GetString(message);
            foreach (var handler in LogHandlers) {
                handler.LogFormat(LogType.Warning, context, "{0}: {1}", tag, output);
            }
        }

        public static void Warning(object message, Object context = null) {
            object output = GetString(message);
            foreach (var handler in LogHandlers) {
                handler.LogFormat(LogType.Warning, context, "{0}", output);
            }
        }

        public static void WarningFormat(string format, params object[] args) {
            foreach (var handler in LogHandlers) {
                handler.LogFormat(LogType.Warning, null, format, args);
            }
        }

        public static void WarningFormat(Object context, string format, params object[] args) {
            foreach (var handler in LogHandlers) {
                handler.LogFormat(LogType.Warning, context, format, args);
            }
        }
        
        public static void Error(string tag, object message, Object context) {
            object output = GetString(message);
            foreach (var handler in LogHandlers) {
                handler.LogFormat(LogType.Error, context, "{0}: {1}", tag, output);
            }
        }

        public static void Error(object message, Object context = null) {
            object output = GetString(message);
            foreach (var handler in LogHandlers) {
                handler.LogFormat(LogType.Error, context, "{0}", output);
            }
        }

        public static void ErrorFormat(string format, params object[] args) {
            foreach (var handler in LogHandlers) {
                handler.LogFormat(LogType.Error, null, format, args);
            }
        }

        public static void ErrorFormat(Object context, string format, params object[] args) {
            foreach (var handler in LogHandlers) {
                handler.LogFormat(LogType.Error, context, format, args);
            }
        }

        public static void Exception(Exception exception, Object context = null) {
            foreach (var handler in LogHandlers) {
                handler.LogException(exception, context);
            }
        }

        /// <summary>
        /// object型のメッセージを文字列に変換
        /// </summary>
        private static string GetString(object message) {
            if (message == null) {
                return "Null";
            }

            return message is IFormattable formattable ? formattable.ToString(null, CultureInfo.InvariantCulture) : message.ToString();
        }
    }
}