using System;
using UnityEngine;

namespace BalartroLike.Unity
{
    // UI 根：持有六个层级和服务引用，作为页面、弹窗、提示的统一入口。
    // 层结构：BackgroundLayer / PageLayer / PopupLayer / TooltipLayer / ToastLayer / AnimationLayer。
    public sealed class UIRoot : MonoBehaviour
    {
        public static UIRoot Instance { get; private set; }

        [SerializeField] private RectTransform _backgroundLayer;
        [SerializeField] private RectTransform _pageLayer;
        [SerializeField] private RectTransform _popupLayer;
        [SerializeField] private RectTransform _tooltipLayer;
        [SerializeField] private RectTransform _toastLayer;
        [SerializeField] private RectTransform _animationLayer;
        [SerializeField] private UIInputBlocker _inputBlocker;
        [SerializeField] private UIPageService _pageService;
        [SerializeField] private UIPopupService _popupService;
        [SerializeField] private UIToastService _toastService;
        [SerializeField] private TooltipView _tooltip;
        [SerializeField] private ConfirmPopup _confirmPopupPrefab;

        public RectTransform BackgroundLayer => _backgroundLayer;
        public RectTransform PageLayer => _pageLayer;
        public RectTransform PopupLayer => _popupLayer;
        public RectTransform TooltipLayer => _tooltipLayer;
        public RectTransform ToastLayer => _toastLayer;
        public RectTransform AnimationLayer => _animationLayer;
        public UIInputBlocker InputBlocker => _inputBlocker;
        public UIPageService Pages => _pageService;
        public UIPopupService Popups => _popupService;
        public UIToastService Toasts => _toastService;
        public TooltipView Tooltip => _tooltip;
        public bool IsInputBlocked => _inputBlocker != null && _inputBlocker.IsBlocked;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            EnsureReferences();
            if (_tooltip != null)
            {
                _tooltip.Initialize();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void Toast(string message)
        {
            if (_toastService != null)
            {
                _toastService.Show(message);
            }
        }

        public void Confirm(string title, string body, Action onConfirm, Action onCancel = null,
            string confirmLabel = "确认", string cancelLabel = "取消")
        {
            if (_popupService == null || _confirmPopupPrefab == null)
            {
                // 弹窗资源未接入时直接执行确认，避免流程被卡住。
                onConfirm?.Invoke();
                return;
            }

            ConfirmPopup popup = _popupService.Show(_confirmPopupPrefab);
            if (popup == null)
            {
                onConfirm?.Invoke();
                return;
            }

            popup.Setup(title, body, confirmLabel, cancelLabel,
                () =>
                {
                    _popupService.CloseTop();
                    onConfirm?.Invoke();
                },
                () =>
                {
                    _popupService.CloseTop();
                    onCancel?.Invoke();
                });
        }

        // Prefab 上漏拖引用时按固定节点名补齐，减少手写 Prefab 的接线错误。
        private void EnsureReferences()
        {
            _backgroundLayer = ResolveLayer(_backgroundLayer, "BackgroundLayer");
            _pageLayer = ResolveLayer(_pageLayer, "PageLayer");
            _popupLayer = ResolveLayer(_popupLayer, "PopupLayer");
            _tooltipLayer = ResolveLayer(_tooltipLayer, "TooltipLayer");
            _toastLayer = ResolveLayer(_toastLayer, "ToastLayer");
            _animationLayer = ResolveLayer(_animationLayer, "AnimationLayer");

            if (_inputBlocker == null)
            {
                _inputBlocker = GetComponentInChildren<UIInputBlocker>(true);
            }

            if (_pageService == null)
            {
                _pageService = GetComponentInChildren<UIPageService>(true);
            }

            if (_popupService == null)
            {
                _popupService = GetComponentInChildren<UIPopupService>(true);
            }

            if (_toastService == null)
            {
                _toastService = GetComponentInChildren<UIToastService>(true);
            }

            if (_tooltip == null)
            {
                _tooltip = GetComponentInChildren<TooltipView>(true);
            }
        }

        private RectTransform ResolveLayer(RectTransform current, string childName)
        {
            if (current != null)
            {
                return current;
            }

            return transform.Find(childName) as RectTransform;
        }
    }
}
