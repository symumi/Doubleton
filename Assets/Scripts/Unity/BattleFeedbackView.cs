using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BalartroLike.Unity
{
    public sealed class BattleFeedbackView : MonoBehaviour
    {
        private Text _text;
        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private BattleFeedbackAnimationConfig _config;
        private Coroutine _routine;
        private Action _onComplete;

        public void Initialize()
        {
            _text = GetComponent<Text>();
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            _config = BattleFeedbackAnimationConfig.Load();
            _canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        public void Play(string message, Color color, Action onComplete)
        {
            StopAnimation(false);
            _onComplete = onComplete;
            _text.text = message;
            _text.color = color;
            _canvasGroup.alpha = 1f;
            _rectTransform.anchoredPosition = Vector2.zero;
            _rectTransform.localScale = Vector3.one * _config.StartScale;
            gameObject.SetActive(true);
            _routine = StartCoroutine(Animate());
        }

        public void Hide()
        {
            StopAnimation(true);
            gameObject.SetActive(false);
        }

        private IEnumerator Animate()
        {
            float duration = Mathf.Max(0.01f, _config.Duration);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                float rise = _config.RiseDistance * EaseOutCubic(progress);
                float shake = Mathf.Sin(progress * _config.ShakeFrequency * Mathf.PI * 2f) * _config.ShakeDistance * (1f - progress);
                _rectTransform.anchoredPosition = new Vector2(shake, rise);
                _rectTransform.localScale = Vector3.one * GetScale(progress);
                _canvasGroup.alpha = GetAlpha(progress);
                yield return null;
            }

            CompleteAnimation();
            gameObject.SetActive(false);
        }

        private float GetScale(float progress)
        {
            if (progress < _config.PunchTime)
            {
                float punchProgress = _config.PunchTime <= 0f ? 1f : progress / _config.PunchTime;
                return Mathf.Lerp(_config.StartScale, _config.PunchScale, EaseOutCubic(punchProgress));
            }

            float settleProgress = _config.PunchTime >= 1f ? 1f : (progress - _config.PunchTime) / (1f - _config.PunchTime);
            return Mathf.Lerp(_config.PunchScale, _config.EndScale, Mathf.SmoothStep(0f, 1f, settleProgress));
        }

        private float GetAlpha(float progress)
        {
            if (progress <= _config.FadeStart)
            {
                return 1f;
            }

            float fadeProgress = _config.FadeStart >= 1f ? 1f : (progress - _config.FadeStart) / (1f - _config.FadeStart);
            return Mathf.Clamp01(1f - fadeProgress);
        }

        private void CompleteAnimation()
        {
            _routine = null;
            Action callback = _onComplete;
            _onComplete = null;
            if (callback != null)
            {
                callback();
            }
        }

        private void StopAnimation(bool invokeComplete)
        {
            if (_routine != null)
            {
                StopCoroutine(_routine);
                _routine = null;
            }

            Action callback = _onComplete;
            _onComplete = null;
            if (invokeComplete && callback != null)
            {
                callback();
            }
        }

        private static float EaseOutCubic(float value)
        {
            float inverse = 1f - Mathf.Clamp01(value);
            return 1f - inverse * inverse * inverse;
        }
    }
}