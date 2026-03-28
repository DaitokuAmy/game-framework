using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace GameFramework {
    /// <summary>
    /// JsonUtilityを使ったシリアライザー
    /// </summary>
    public sealed class JsonSerializer : ISerializer {
        private static readonly Encoding Utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        /// <summary>
        /// シリアライズ処理
        /// </summary>
        byte[] ISerializer.Serialize<T>(T data) {
            ValidateRootType(data?.GetType() ?? typeof(T));
            var json = JsonUtility.ToJson(data);
            return Utf8.GetBytes(json);
        }

        /// <summary>
        /// デシリアライズ処理
        /// </summary>
        T ISerializer.Deserialize<T>(byte[] bytes) {
            if (bytes == null || bytes.Length == 0) {
                throw new ArgumentException("Serialized data is empty.", nameof(bytes));
            }

            ValidateRootType(typeof(T));
            var json = Utf8.GetString(bytes);
            return JsonUtility.FromJson<T>(json);
        }

        /// <summary>
        /// JsonUtilityで扱えるルート型かどうかを検証する
        /// </summary>
        private static void ValidateRootType(Type type) {
            if (type == null) {
                throw new NotSupportedException("JsonSerializer does not support null type metadata.");
            }

            if (type.IsArray || TryGetListElementType(type, out _) || IsDirectlySupported(type) || typeof(UnityEngine.Object).IsAssignableFrom(type)) {
                throw new NotSupportedException($"JsonSerializer requires a [Serializable] root object type. [{type.Name}]");
            }

            var visitedTypes = new HashSet<Type>();
            ValidateTypeRecursive(type, type.Name, visitedTypes);
        }

        /// <summary>
        /// JsonUtilityで扱える型かどうかを再帰的に検証する
        /// </summary>
        private static void ValidateTypeRecursive(Type type, string path, HashSet<Type> visitedTypes) {
            if (type == null) {
                throw new NotSupportedException($"JsonSerializer does not support null type metadata. [{path}]");
            }

            if (IsDirectlySupported(type)) {
                return;
            }

            if (type.IsArray) {
                if (type.GetArrayRank() != 1) {
                    throw new NotSupportedException($"JsonSerializer does not support multidimensional arrays. [{path}]");
                }

                ValidateTypeRecursive(type.GetElementType(), $"{path}[]", visitedTypes);
                return;
            }

            if (TryGetListElementType(type, out var elementType)) {
                ValidateTypeRecursive(elementType, $"{path}[]", visitedTypes);
                return;
            }

            if (type.IsGenericType) {
                throw new NotSupportedException($"JsonSerializer does not support generic types other than List<T>. [{path}]");
            }

            if (type.IsAbstract || type.IsInterface) {
                throw new NotSupportedException($"JsonSerializer does not support abstract or interface types. [{path}]");
            }

            if (!type.IsSerializable) {
                throw new NotSupportedException($"JsonSerializer requires [Serializable] types. [{path}]");
            }

            if (!visitedTypes.Add(type)) {
                return;
            }

            try {
                foreach (var field in GetSerializableFields(type)) {
                    ValidateTypeRecursive(field.FieldType, $"{path}.{field.Name}", visitedTypes);
                }
            }
            finally {
                visitedTypes.Remove(type);
            }
        }

        /// <summary>
        /// JsonUtilityが直接扱える型かどうか
        /// </summary>
        private static bool IsDirectlySupported(Type type) {
            if (type.IsEnum || type.IsPrimitive || type == typeof(string)) {
                return true;
            }

            return typeof(UnityEngine.Object).IsAssignableFrom(type);
        }

        /// <summary>
        /// Unityシリアライズ対象フィールドを取得する
        /// </summary>
        private static IEnumerable<FieldInfo> GetSerializableFields(Type type) {
            const BindingFlags BindingFlagsMask = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

            for (var current = type; current != null && current != typeof(object); current = current.BaseType) {
                var fields = current.GetFields(BindingFlagsMask);
                for (var i = 0; i < fields.Length; i++) {
                    var field = fields[i];
                    if (field.IsStatic || field.IsInitOnly || field.IsNotSerialized) {
                        continue;
                    }

                    if (field.IsPublic || field.GetCustomAttribute<SerializeField>() != null) {
                        yield return field;
                    }
                }
            }
        }

        /// <summary>
        /// List型の要素型を取得する
        /// </summary>
        private static bool TryGetListElementType(Type type, out Type elementType) {
            elementType = null;

            if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(List<>)) {
                return false;
            }

            elementType = type.GetGenericArguments()[0];
            return true;
        }
    }
}
