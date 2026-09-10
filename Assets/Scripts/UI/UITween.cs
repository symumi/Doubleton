using System;
using System.Collections;
using UnityEngine;

namespace BalartroLike.Unity
{
    public enum UITweenEasing
    {
        Linear,
        InQuad,
        OutQuad,
        InCubic,
        OutCubic,
        InOutCubic,
        OutBack,
    }

    // UI 动效统一使用非缩放时间播放，避免暂停或时间缩放影响界面反馈。
    public static class UITween
    {
        public static float Evaluate(UITweenEasing easing, float progress)
        {
            float value = Mathf.Clamp01(progress);
            switch (easing)
            {
                case UITweenEasing.InQuad:
                    return value * value;
                case UITweenEasing.OutQuad:
                    return 1f - (1f - value) * (1f - value);
                case UITweenEasing.InCubic:
                    return value * value * value;
                case UITweenEasing.OutCubic:
                    return OutCubic(value);
                case UITweenEasing.InOutCubic:
                    return value < 0.5f
                        ? 4f * value * value * value
                        : 1f - Mathf.Pow(-2f * value + 2f, 3f) * 0.5f;
                case UITweenEasing.OutBack:
                    return OutBack(value);
                default:
                    return value;
            }
        }

        public static float OutCubic(float progress)
        {
            float inverse = 1f - Mathf.Clamp01(progress);
            return 1f - inverse * inverse * inverse;
        }

        public static float OutBack(float progress)
        {
            float value = Mathf.Clamp01(progress);
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            float shifted = value - 1f;
            return 1f + c3 * shifted * shifted * shifted + c1 * shifted * shifted;
        }

        public static IEnumerator Play(UITweenEasing easing, float duration, Action<float> onUpdate, Action onComplete = null)
        {
            float length = Mathf.Max(0f, duration);
            if (length <= 0f)
            {
                onUpdate?.Invoke(1f);
                onComplete?.Invoke();
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < length)
            {
                elapsed += Time.unscaledDeltaTime;
                onUpdate?.Invoke(Evaluate(easing, Mathf.Clamp01(elapsed / length)));
                yield return null;
            }

            onUpdate?.Invoke(1f);
            onComplete?.Invoke();
        }
    }
}
