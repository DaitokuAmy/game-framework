using System;
using System.Collections.Generic;
using System.Linq;

namespace GameFramework.GimmickSystem {
    /// <summary>
    /// Gimmick再生用クラス
    /// </summary>
    public sealed class GimmickPlayer {
        // キャッシュ用のGimmick情報
        private readonly Dictionary<string, List<IGimmick>> _gimmicks = new();
        // 型ごとのGimmick取得キャッシュ
        private readonly Dictionary<string, Dictionary<Type, object>> _typedGimmicks = new();

        /// <summary>
        /// ギミックのキー一覧を取得
        /// </summary>
        public string[] GetKeys() {
            return _gimmicks.Keys.ToArray();
        }

        /// <summary>
        /// ギミックのキー一覧を取得
        /// </summary>
        public string[] GetKeys<T>()
            where T : Gimmick {
            return _gimmicks
                .Where(x => x.Value.Exists(y => y is T))
                .Select(x => x.Key)
                .ToArray();
        }

        /// <summary>
        /// ギミックの取得
        /// </summary>
        /// <param name="key">取得用のキー</param>
        /// <typeparam name="T">ギミックの型</typeparam>
        public IReadOnlyList<T> GetGimmicks<T>(string key)
            where T : Gimmick {
            if (!_gimmicks.TryGetValue(key, out var list)) {
                return Array.Empty<T>();
            }

            if (!_typedGimmicks.TryGetValue(key, out var typeDict)) {
                typeDict = new Dictionary<Type, object>();
                _typedGimmicks[key] = typeDict;
            }

            var type = typeof(T);
            if (!typeDict.TryGetValue(type, out var cached)) {
                cached = Array.AsReadOnly(list.OfType<T>().ToArray());
                typeDict[type] = cached;
            }

            return (IReadOnlyList<T>)cached;
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <param name="gimmickGroups">制御対象のGimmickGroupリスト</param>
        public void Setup(IEnumerable<GimmickGroup> gimmickGroups) {
            _gimmicks.Clear();
            _typedGimmicks.Clear();

            foreach (var gimmickGroup in gimmickGroups) {
                if (gimmickGroup == null) {
                    continue;
                }

                var gimmickInfos = gimmickGroup.GimmickInfos;
                foreach (var gimmickInfo in gimmickInfos) {
                    if (gimmickInfo?.gimmick is not IGimmick gimmick) {
                        continue;
                    }

                    if (!_gimmicks.TryGetValue(gimmickInfo.key, out var list)) {
                        list = new List<IGimmick>();
                        _gimmicks[gimmickInfo.key] = list;
                    }

                    list.Add(gimmick);
                    gimmick.Initialize();
                }
            }
        }

        /// <summary>
        /// ギミックの更新
        /// </summary>
        public void Update(float deltaTime) {
            foreach (var gimmickList in _gimmicks.Values) {
                foreach (var gimmick in gimmickList) {
                    gimmick.UpdateGimmick(deltaTime);
                }
            }
        }

        /// <summary>
        /// ギミックの後更新
        /// </summary>
        public void LateUpdate(float deltaTime) {
            foreach (var gimmickList in _gimmicks.Values) {
                foreach (var gimmick in gimmickList) {
                    gimmick.LateUpdateGimmick(deltaTime);
                }
            }
        }

        /// <summary>
        /// ギミック速度の設定
        /// </summary>
        public void SetSpeed(float speed) {
            foreach (var pair in _gimmicks) {
                foreach (var gimmick in pair.Value) {
                    gimmick.SetSpeed(speed);
                }
            }
        }
    }
}
