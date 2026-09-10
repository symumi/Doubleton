using System;
using System.Collections.Generic;
using BalartroLike.Battle;
using UnityEngine;

namespace BalartroLike.Unity
{
    // 符箓栏：运行时实例化 TalismanButtonView，数量随本局符箓列表变化（MVP 上限 5）。
    public sealed class TalismanPanelView : MonoBehaviour
    {
        [SerializeField] private RectTransform _buttonRoot;
        [SerializeField] private TalismanButtonView _buttonPrefab;

        private readonly List<TalismanButtonView> _buttons = new List<TalismanButtonView>();

        public void Render(IReadOnlyList<TalismanDefinition> talismans, bool interactable, Action<int> onUse)
        {
            int count = talismans == null ? 0 : talismans.Count;
            EnsureCount(count);

            for (int i = 0; i < _buttons.Count; i++)
            {
                bool used = i < count;
                _buttons[i].gameObject.SetActive(used);
                if (used)
                {
                    _buttons[i].Bind(i, talismans[i], interactable, onUse);
                }
            }
        }

        private void EnsureCount(int count)
        {
            if (_buttonPrefab == null || _buttonRoot == null)
            {
                return;
            }

            while (_buttons.Count < count)
            {
                TalismanButtonView button = Instantiate(_buttonPrefab, _buttonRoot, false);
                button.gameObject.SetActive(false);
                _buttons.Add(button);
            }
        }
    }
}
