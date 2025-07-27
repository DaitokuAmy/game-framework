using System.Collections.Generic;
using GameFramework;
using GameFramework.UISystems;
using TMPro;
using UnityEngine;

namespace SampleGame.Presentation.UITest {
    /// <summary>
    /// HudUIScreenのPresenter
    /// </summary>
    public class HudUIScreenPresenter : UIScreenLogic<UITestHudUIScreen> {
        private class TestData : RecyclableScrollList.IData {
            public int Id;
        }

        private List<TestData> _testDataList = new();

        /// <summary>
        /// 開く前の処理
        /// </summary>
        protected override void PreOpenInternal() {
            base.PreOpenInternal();

            // リストの初期化
            _testDataList = new List<TestData>();
            for (var i = 0; i < 15; i++) {
                _testDataList.Add(new TestData {
                    Id = i + 1
                });
            }

            void SetupList(RecyclableScrollList list) {
                list.SetInitializer((view, param) => {
                    if (param is TestData testParam) {
                        var text = view.gameObject.GetComponentInChildren<TMP_Text>();
                        text.text = $"Id = {testParam.Id}";
                    }
                });
                list.SetDataList(_testDataList, _ => Random.Range(0, 2) == 0 ? "A" : "B");
            }

            SetupList(Screen.TestScrollVList);
            SetupList(Screen.TestScrollHList);
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal() {
            base.UpdateInternal();

            for (var i = 0; i < 5; i++) {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i)) {
                    Screen.SetScrollSnapperIndex(i, Input.GetKey(KeyCode.LeftShift));
                }
            }
        }
    }
}