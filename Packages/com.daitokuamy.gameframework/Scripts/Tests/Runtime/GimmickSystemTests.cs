using System;
using System.Collections.Generic;
using System.Reflection;
using GameFramework.ActorSystem;
using GameFramework.GimmickSystem;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameFramework.Tests {
    /// <summary>
    /// GimmickSystem の回帰テスト
    /// </summary>
    public sealed class GimmickSystemTests {
        private readonly List<GameObject> _createdObjects = new();

        /// <summary>
        /// テスト後に生成物を破棄
        /// </summary>
        [TearDown]
        public void TearDown() {
            foreach (var obj in _createdObjects) {
                if (obj != null) {
                    Object.DestroyImmediate(obj);
                }
            }

            _createdObjects.Clear();
        }

        /// <summary>
        /// inactive な Gimmick でも Setup 時に初期化されることを検証
        /// </summary>
        [Test]
        public void GimmickPlayer_Setup_InitializesInactiveGimmick() {
            var groupObject = CreateGameObject("GimmickSystemTests.Group");
            var group = groupObject.AddComponent<GimmickGroup>();

            var gimmickObject = CreateGameObject("GimmickSystemTests.Gimmick", active: false);
            gimmickObject.transform.SetParent(groupObject.transform, false);
            var gimmick = gimmickObject.AddComponent<TestInitializeGimmick>();

            SetPrivateField(group, typeof(GimmickGroup), "_gimmickInfos", new[] {
                new GimmickGroup.GimmickInfo {
                    key = "Initialize",
                    gimmick = gimmick,
                },
            });

            var player = new GimmickPlayer();
            player.Setup(new[] { group });

            Assert.That(gimmick.InitializeCount, Is.EqualTo(1));
        }

        /// <summary>
        /// 同じ型・キーの取得結果がキャッシュされて再利用されることを検証
        /// </summary>
        [Test]
        public void GimmickPlayer_GetGimmicks_ReusesTypedArrayCache() {
            var groupObject = CreateGameObject("GimmickSystemTests.CacheGroup");
            var group = groupObject.AddComponent<GimmickGroup>();

            var firstObject = CreateGameObject("GimmickSystemTests.Cache.First");
            firstObject.transform.SetParent(groupObject.transform, false);
            var first = firstObject.AddComponent<TestInitializeGimmick>();

            var secondObject = CreateGameObject("GimmickSystemTests.Cache.Second");
            secondObject.transform.SetParent(groupObject.transform, false);
            var second = secondObject.AddComponent<TestInitializeGimmick>();

            SetPrivateField(group, typeof(GimmickGroup), "_gimmickInfos", new[] {
                new GimmickGroup.GimmickInfo {
                    key = "Cache",
                    gimmick = first,
                },
                new GimmickGroup.GimmickInfo {
                    key = "Cache",
                    gimmick = second,
                },
            });

            var player = new GimmickPlayer();
            player.Setup(new[] { group });

            var firstResult = player.GetGimmicks<TestInitializeGimmick>("Cache");
            var secondResult = player.GetGimmicks<TestInitializeGimmick>("Cache");

            Assert.That(secondResult, Is.SameAs(firstResult));
            Assert.That(firstResult, Has.Length.EqualTo(2));
        }

        /// <summary>
        /// MaterialStateGimmick の immediate 遷移がその場で反映されることを検証
        /// </summary>
        [Test]
        public void MaterialStateGimmick_ImmediateChange_AppliesImmediately() {
            var gimmick = CreateMaterialStateGimmick();

            gimmick.Change("On", true);

            Assert.That(gimmick.AppliedValues.Count, Is.EqualTo(2));
            Assert.That(gimmick.AppliedValues[^1], Is.EqualTo(10.0f).Within(0.0001f));
        }

        /// <summary>
        /// MaterialStateGimmick の非即時遷移が更新で進むことを検証
        /// </summary>
        [Test]
        public void MaterialStateGimmick_BlendChange_UpdatesOverTime() {
            var gimmick = CreateMaterialStateGimmick();
            gimmick.AppliedValues.Clear();

            gimmick.Change("On", false);

            Assert.That(gimmick.AppliedValues, Is.Empty);

            ((IGimmick)gimmick).LateUpdateGimmick(0.25f);
            Assert.That(gimmick.AppliedValues[^1], Is.EqualTo(5.0f).Within(0.0001f));

            ((IGimmick)gimmick).LateUpdateGimmick(0.25f);
            Assert.That(gimmick.AppliedValues[^1], Is.EqualTo(10.0f).Within(0.0001f));
        }

        /// <summary>
        /// テスト用の MaterialStateGimmick を生成
        /// </summary>
        private TestMaterialStateGimmick CreateMaterialStateGimmick() {
            var gimmickObject = CreateGameObject("GimmickSystemTests.MaterialState", active: false);
            var gimmick = gimmickObject.AddComponent<TestMaterialStateGimmick>();

            SetPrivateField(
                gimmick,
                typeof(StateGimmick),
                "_defaultState",
                "Off");
            SetPrivateField(
                gimmick,
                typeof(StateGimmickBase<MaterialStateGimmick<float>.StateInfo>),
                "_stateInfos",
                new[] {
                    new MaterialStateGimmick<float>.StateInfo {
                        stateName = "Off",
                        materialValue = 0.0f,
                    },
                    new MaterialStateGimmick<float>.StateInfo {
                        stateName = "On",
                        materialValue = 10.0f,
                    },
                });

            ((IGimmick)gimmick).Initialize();
            return gimmick;
        }

        /// <summary>
        /// テスト用 GameObject を生成
        /// </summary>
        private GameObject CreateGameObject(string name, bool active = true) {
            var obj = new GameObject(name);
            obj.SetActive(active);
            _createdObjects.Add(obj);
            return obj;
        }

        /// <summary>
        /// private フィールドを設定
        /// </summary>
        private static void SetPrivateField(object target, Type declaringType, string fieldName, object value) {
            var field = declaringType.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"{declaringType.FullName}.{fieldName} was not found.");
            field.SetValue(target, value);
        }

        /// <summary>
        /// 初期化回数を確認するテスト用 Gimmick
        /// </summary>
        private sealed class TestInitializeGimmick : Gimmick {
            public int InitializeCount { get; private set; }

            protected override void InitializeInternal() {
                InitializeCount++;
            }
        }

        /// <summary>
        /// 反映値を追跡するテスト用 MaterialStateGimmick
        /// </summary>
        private sealed class TestMaterialStateGimmick : MaterialStateGimmick<float> {
            public List<float> AppliedValues { get; } = new();

            protected override void SetValue(float targetValue, float ratio, MaterialHandle materialHandle, int propertyId) {
                var current = AppliedValues.Count > 0 ? AppliedValues[^1] : 0.0f;
                AppliedValues.Add(Mathf.Lerp(current, targetValue, ratio));
            }
        }
    }
}
