using System;
using System.Collections.Generic;
using System.Linq;
using GameFramework.GimmickSystems;

namespace GameFramework.BodySystems {
    /// <summary>
    /// Gimmick制御用コントローラ
    /// </summary>
    public class GimmickController : BodyController {
        // キャッシュ用のGimmick情報
        private Dictionary<string, List<GameFramework.GimmickSystems.IGimmick>> _gimmicks = new();

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
            where T : GameFramework.GimmickSystems.Gimmick {
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
        protected override void InitializeInternal() {
            var meshController = Body.GetController<MeshController>();
            meshController.OnRefreshed += RefreshGimmicks;
            RefreshGimmicks();
        }

        /// <summary>
        /// ギミックの更新
        /// </summary>
        protected override void UpdateInternal(float deltaTime) {
            foreach (var gimmickList in _gimmicks.Values) {
                foreach (var gimmick in gimmickList) {
                    gimmick.UpdateGimmick(deltaTime);
                }
            }
        }

        /// <summary>
        /// ギミックの更新
        /// </summary>
        protected override void LateUpdateInternal(float deltaTime) {
            foreach (var gimmickList in _gimmicks.Values) {
                foreach (var gimmick in gimmickList) {
                    gimmick.LateUpdateGimmick(deltaTime);
                }
            }
        }

        /// <summary>
        /// ギミック情報の取得
        /// </summary>
        private void RefreshGimmicks() {
            _gimmicks.Clear();

            var gimmickPartsList = Body.GetComponentsInChildren<GimmickGroup>(true);
            foreach (var gimmickParts in gimmickPartsList) {
                if (gimmickParts == null) {
                    continue;
                }

                var gimmickInfos = gimmickParts.GimmickInfos;
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
    }
}