using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BalartroLike.Unity
{
    // 生命 / 护盾 / 灵力条：主填充平滑过渡，延迟条在停顿后追赶，便于看出本次变化量。
    // 填充 Image 需设置 Image Type = Filled、Fill Method = Horizontal。
    public sealed class UIProgressBar : MonoBehaviour
    {
        [SerializeField] private Image _fill;
        [SerializeField] private Image _delayedFill;
        [SerializeField] private TMP_Text _valueText;
        [SerializeField] private string _valueFormat = "{0}/{1}";

        private UIAnimationConfig _config;
        private Coroutine _fillRoutine;
        private Coroutine _delayedRoutine;
        private float _normalized = 1f;
        private float _delayed = 1f;

        public float Normalized => _normalized;

        private void Awake()
        {
            _config = UIAnimationConfig.Load();
            ApplyImmediate(1f);
        }

        public void SetValue(int current, int max, bool animated = true)
        {
            int clampedMax = Mathf.Max(0, max);
            int clampedCurrent = Mathf.Clamp(current, 0, clampedMax);
            if (_valueText != null)
            {
                _valueText.text = string.Format(_valueFormat, clampedCurrent, clampedMax);
            }

            SetNormalized(clampedMax == 0 ? 0f : (float)clampedCurrent / clampedMax, animated);
        }

        public void SetNormalized(float value, bool animated = true)
        {
            float target = Mathf.Clamp01(value);
            if (!animated || Mathf.Approximately(target, _normalized))
            {
                StopRoutines();
                ApplyImmediate(target);
                return;
            }

            StopRoutines();
            UIAnimationEntry entry = _config.Get("bar_fill");
            float fillFrom = _normalized;
            _normalized = target;
            if (_fill != null)
            {
                _fillRoutine = StartCoroutine(UITween.Play(entry.Easing, entry.Duration, progress =>
                {
                    _fill.fillAmount = Mathf.Lerp(fillFrom, target, progress);
                }));
            }

            if (_delayedFill == null)
            {
                return;
            }

            // 回血时延迟条不落后，直接对齐目标值。
            if (target >= _delayed)
            {
                _delayed = target;
                _delayedFill.fillAmount = target;
                return;
            }

            _delayedRoutine = StartCoroutine(PlayDelayedBar(target, entry));
        }

        private IEnumerator PlayDelayedBar(float target, UIAnimationEntry entry)
        {
            float from = _delayed;
            _delayed = target;
            if (entry.Delay > 0f)
            {
                yield return new WaitForSecondsRealtime(entry.Delay);
            }

            yield return UITween.Play(entry.Easing, entry.Duration, progress =>
            {
                if (_delayedFill != null)
                {
                    _delayedFill.fillAmount = Mathf.Lerp(from, target, progress);
                }
            });

            if (_delayedFill != null)
            {
                _delayedFill.fillAmount = target;
            }
        }

        private void ApplyImmediate(float value)
        {
            _normalized = value;
            _delayed = value;
            if (_fill != null)
            {
                _fill.fillAmount = value;
            }

            if (_delayedFill != null)
            {
                _delayedFill.fillAmount = value;
            }
        }

        private void StopRoutines()
        {
            if (_fillRoutine != null)
            {
                StopCoroutine(_fillRoutine);
                _fillRoutine = null;
            }

            if (_delayedRoutine != null)
            {
                StopCoroutine(_delayedRoutine);
                _delayedRoutine = null;
            }
        }
    }
}
