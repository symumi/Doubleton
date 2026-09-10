using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BalartroLike.Unity
{
    // 通用确认弹窗：放弃进度、覆盖存档、退出游戏、离开坊市等二次确认。
    // 关闭动作由调用方（UIRoot.Confirm）统一处理，弹窗自身只负责收集选择。
    public sealed class ConfirmPopup : MonoBehaviour
    {
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _bodyText;
        [SerializeField] private TMP_Text _confirmLabelText;
        [SerializeField] private TMP_Text _cancelLabelText;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _cancelButton;

        private Action _onConfirm;
        private Action _onCancel;

        public void Setup(string title, string body, string confirmLabel, string cancelLabel, Action onConfirm, Action onCancel)
        {
            if (_titleText != null)
            {
                _titleText.text = title ?? string.Empty;
            }

            if (_bodyText != null)
            {
                _bodyText.text = body ?? string.Empty;
            }

            if (_confirmLabelText != null)
            {
                _confirmLabelText.text = string.IsNullOrEmpty(confirmLabel) ? "确认" : confirmLabel;
            }

            if (_cancelLabelText != null)
            {
                _cancelLabelText.text = string.IsNullOrEmpty(cancelLabel) ? "取消" : cancelLabel;
            }

            _onConfirm = onConfirm;
            _onCancel = onCancel;

            if (_confirmButton != null)
            {
                _confirmButton.onClick.RemoveListener(HandleConfirm);
                _confirmButton.onClick.AddListener(HandleConfirm);
            }

            if (_cancelButton != null)
            {
                _cancelButton.onClick.RemoveListener(HandleCancel);
                _cancelButton.onClick.AddListener(HandleCancel);
            }
        }

        private void OnDestroy()
        {
            if (_confirmButton != null)
            {
                _confirmButton.onClick.RemoveListener(HandleConfirm);
            }

            if (_cancelButton != null)
            {
                _cancelButton.onClick.RemoveListener(HandleCancel);
            }
        }

        private void HandleConfirm()
        {
            Action callback = _onConfirm;
            _onConfirm = null;
            _onCancel = null;
            callback?.Invoke();
        }

        private void HandleCancel()
        {
            Action callback = _onCancel;
            _onConfirm = null;
            _onCancel = null;
            callback?.Invoke();
        }
    }
}
