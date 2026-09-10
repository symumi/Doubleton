using System.Collections;
using TMPro;
using UnityEngine;

namespace BalartroLike.Unity
{
    // 悬停说明浮层：跟随鼠标并夹在 Canvas 范围内，层级高于所有普通面板。
    public sealed class TooltipView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _bodyText;
        [SerializeField] private Vector2 _cursorOffset = new Vector2(18f, -18f);

        private RectTransform _rectTransform;
        private UIAnimationConfig _config;
        private Coroutine _routine;
        private bool _visible;

        public bool IsVisible => _visible;
        public string Title => _titleText == null ? string.Empty : _titleText.text;
        public string Body => _bodyText == null ? string.Empty : _bodyText.text;

        public void Initialize()
        {
            EnsureReferences();
            SetAlpha(0f);
            gameObject.SetActive(false);
        }

        public void Show(string title, string body, Vector2 screenPosition)
        {
            EnsureReferences();
            if (_titleText != null)
            {
                _titleText.text = title ?? string.Empty;
            }

            if (_bodyText != null)
            {
                _bodyText.text = body ?? string.Empty;
            }

            _visible = true;
            gameObject.SetActive(true);
            Follow(screenPosition);
            PlayFade(1f);
        }

        public void Hide()
        {
            _visible = false;
            StopRoutine();
            SetAlpha(0f);
            gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            if (_visible)
            {
                Follow(Input.mousePosition);
            }
        }

        private void Follow(Vector2 screenPosition)
        {
            if (_rectTransform == null)
            {
                return;
            }

            RectTransform parent = _rectTransform.parent as RectTransform;
            if (parent == null)
            {
                return;
            }

            // Canvas 为 Screen Space - Overlay 时相机传 null。
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPosition, null, out Vector2 local))
            {
                return;
            }

            Vector2 halfParent = parent.rect.size * 0.5f;
            Vector2 halfSelf = _rectTransform.rect.size * 0.5f;
            float x = Mathf.Clamp(local.x + _cursorOffset.x, -halfParent.x + halfSelf.x, halfParent.x - halfSelf.x);
            float y = Mathf.Clamp(local.y + _cursorOffset.y, -halfParent.y + halfSelf.y, halfParent.y - halfSelf.y);
            _rectTransform.anchoredPosition = new Vector2(x, y);
        }

        private void PlayFade(float target)
        {
            StopRoutine();
            UIAnimationEntry entry = _config.Get("tooltip_in");
            float from = _canvasGroup == null ? 0f : _canvasGroup.alpha;
            _routine = StartCoroutine(UITween.Play(entry.Easing, entry.Duration, progress =>
            {
                SetAlpha(Mathf.Lerp(from, target, progress));
            }));
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
            if (_rectTransform == null)
            {
                _rectTransform = GetComponent<RectTransform>();
            }

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
        }
    }
}
