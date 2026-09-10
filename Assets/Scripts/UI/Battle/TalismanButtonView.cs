using System;
using BalartroLike.Battle;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BalartroLike.Unity
{
    // 符箓按钮：由 TalismanPanelView 在运行时实例化。
    public sealed class TalismanButtonView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private TooltipTrigger _tooltip;

        private Action<int> _onUse;
        private int _index;

        public string TalismanId { get; private set; }

        public void Bind(int index, TalismanDefinition talisman, bool interactable, Action<int> onUse)
        {
            _index = index;
            _onUse = onUse;
            TalismanId = talisman == null ? string.Empty : talisman.Id;

            if (_label != null)
            {
                _label.text = talisman == null ? string.Empty : talisman.DisplayName;
            }

            if (_button != null)
            {
                _button.onClick.RemoveListener(HandleClick);
                _button.onClick.AddListener(HandleClick);
                _button.interactable = interactable;
            }

            if (_tooltip != null)
            {
                _tooltip.SetContent(
                    talisman == null ? string.Empty : talisman.DisplayName,
                    talisman == null ? string.Empty : talisman.Description);
            }
        }

        private void HandleClick()
        {
            _onUse?.Invoke(_index);
        }
    }
}
