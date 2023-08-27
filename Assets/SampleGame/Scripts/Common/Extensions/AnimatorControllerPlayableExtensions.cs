using System.Collections;
using UnityEngine.Animations;

namespace SampleGame {
    /// <summary>
    /// AnimatorControllerPlayableの拡張メソッド
    /// </summary>
    public static class AnimatorControllerPlayableExtensions {
        /// <summary>
        /// AnimatorControllerが特定Stateになるのを待つ
        /// </summary>
        public static IEnumerator WaitAnimatorControllerState(this AnimatorControllerPlayable source, string stateName, int layerIndex = 0) {
            while (true) {
                var stateInfo = source.GetNextAnimatorStateInfo(layerIndex);
                if (stateInfo.fullPathHash == 0) {
                    stateInfo = source.GetCurrentAnimatorStateInfo(layerIndex);
                }

                if (stateInfo.IsName(stateName)) {
                    yield break;
                }

                yield return null;
            }
        }

        /// <summary>
        /// AnimatorControllerが特定Stateになるのを待つ(Tag指定)
        /// </summary>
        public static IEnumerator WaitAnimatorControllerStateByTag(this AnimatorControllerPlayable source, string tagName, int layerIndex = 0) {
            while (true) {
                var stateInfo = source.GetNextAnimatorStateInfo(layerIndex);
                if (stateInfo.fullPathHash == 0) {
                    stateInfo = source.GetCurrentAnimatorStateInfo(layerIndex);
                }

                if (stateInfo.IsTag(tagName)) {
                    yield break;
                }

                yield return null;
            }
        }

        /// <summary>
        /// AnimatorControllerが特定Stateを抜けるを待つ
        /// </summary>
        public static IEnumerator WaitWithoutAnimatorControllerState(this AnimatorControllerPlayable source, string stateName, int layerIndex = 0) {
            while (true) {
                var stateInfo = source.GetNextAnimatorStateInfo(layerIndex);
                if (stateInfo.fullPathHash == 0) {
                    stateInfo = source.GetCurrentAnimatorStateInfo(layerIndex);
                }

                if (!stateInfo.IsName(stateName)) {
                    yield break;
                }

                yield return null;
            }
        }

        /// <summary>
        /// AnimatorControllerが特定Stateを抜けるを待つ
        /// </summary>
        public static IEnumerator WaitWithoutAnimatorControllerStateByTag(this AnimatorControllerPlayable source, string tagName, int layerIndex = 0) {
            while (true) {
                var stateInfo = source.GetNextAnimatorStateInfo(layerIndex);
                if (stateInfo.fullPathHash == 0) {
                    stateInfo = source.GetCurrentAnimatorStateInfo(layerIndex);
                }

                if (!stateInfo.IsTag(tagName)) {
                    yield break;
                }

                yield return null;
            }
        }
    }
}