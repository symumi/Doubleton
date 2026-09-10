using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace BalartroLike.Unity
{
    // 单条界面内短提示：淡入 → 停留 → 淡出。位置由 UIToastService 的布局组管理。
    public sealed class ToastView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TMP_Text _text;

        private UIAnimationConfig _config;
        private Coroutine _routine;
        private Action<ToastView> _onFinished;

        public void Play(string message, Action<ToastView> onFinished)
        {
            EnsureReferences();
            StopRoutine();
            _onFinished = onFinished;
            if (_text != null)
            {
                _text.text = message ?? string.Empty;
            }

            gameObject.SetActive(true);
            _routine = StartCoroutine(Animate());
        }

        public void Hide()
        {
            StopRoutine();
            gameObject.SetActive(false);
        }

        private IEnumerator Animate()
        {
            UIAnimationEntry fadeIn = _config.Get("toast_in");
            UIAnimationEntry hold = _config.Get("toast_hold");
            UIAnimationEntry fadeOut = _config.Get("toast_out");

            yield return UITween.Play(fadeIn.Easing, fadeIn.Duration, progress => SetAlpha(progress));
            SetAlpha(1f);

            if (hold.Duration > 0f)
            {
                yield return new WaitForSecondsRealtime(hold.Duration);
            }

            yield return UITween.Play(fadeOut.Easing, fadeOut.Duration, progress => SetAlpha(1f - progress));
            SetAlpha(0f);

            _routine = null;
            Action<ToastView> callback = _onFinished;
            _onFinished = null;
            callback?.Invoke(this);
        }

        private void SetAlpha(float value)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = Mathf.Clamp01(value);
            }
        }

        private void EnsureReferences()
        {
            if (_config == null)
            {
                _config = UIAnimationConfig.Load();
            }

            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                {
                    _canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }
        }

        private void StopRoutine()
        {
            if (_routine != null)
            {
                StopCoroutine(_routine);
                _routine = null;
            }

            _onFinished = null;
        }
    }
}
