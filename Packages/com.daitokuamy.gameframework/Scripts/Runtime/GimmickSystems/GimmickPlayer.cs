using System;
using System.Collections.Generic;
using System.Linq;
using GameFramework.GimmickSystems;

namespace GameFramework.ActorSystems {
    /// <summary>
    /// Gimmick再生用クラス
    /// </summary>
    public class GimmickPlayer {
        // キャッシュ用のGimmick情報
        private readonly Dictionary<string, List<IGimmick>> _gimmicks = new();

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
        public T[] GetGimmicks<T>(string key)
            where T : GameFramework.GimmickSystems.Gimmick {
            if (!_gimmicks.TryGetValue(key, out var list)) {
                return Array.Empty<T>();
            }

            return list.OfType<T>().ToArray();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <param name="gimmickGroups">制御対象のGimmickGroupリスト</param>
        public void Setup(IEnumerable<GimmickGroup> gimmickGroups) {
            _gimmicks.Clear();

            foreach (var gimmickGroup in gimmickGroups) {
                if (gimmickGroup == null) {
                    continue;
                }

                var gimmickInfos = gimmickGroup.GimmickInfos;
                foreach (var gimmickInfo in gimmickInfos) {
                    if (!_gimmicks.TryGetValue(gimmickInfo.key, out var list)) {
                        list = new List<GameFramework.GimmickSystems.IGimmick>();
                        _gimmicks[gimmickInfo.key] = list;
                        foreach (var gimmick in list) {
                            gimmick.Initialize();
                        }
                    }

                    list.Add(gimmickInfo.gimmick);
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