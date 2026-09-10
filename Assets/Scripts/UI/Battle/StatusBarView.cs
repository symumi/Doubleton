using System.Collections.Generic;
using BalartroLike.Battle;
using UnityEngine;

namespace BalartroLike.Unity
{
    // 状态图标条：运行时实例化 StatusChipView，用于玩家、敌人和法宝触发提示。
    public sealed class StatusBarView : MonoBehaviour
    {
        [SerializeField] private RectTransform _chipRoot;
        [SerializeField] private StatusChipView _chipPrefab;
        [SerializeField] private GameObject _emptyHint;

        private readonly List<StatusChipView> _chips = new List<StatusChipView>();

        public void Render(IReadOnlyList<StatusInstance> statuses)
        {
            int count = statuses == null ? 0 : statuses.Count;
            EnsureCount(count);

            for (int i = 0; i < _chips.Count; i++)
            {
                bool used = i < count;
                _chips[i].gameObject.SetActive(used);
                if (used)
                {
                    _chips[i].Bind(statuses[i]);
                }
            }

            if (_emptyHint != null)
            {
                _emptyHint.SetActive(count == 0);
            }
        }

        private void EnsureCount(int count)
        {
            if (_chipPrefab == null || _chipRoot == null)
            {
                return;
            }

            while (_chips.Count < count)
            {
                StatusChipView chip = Instantiate(_chipPrefab, _chipRoot, false);
                chip.gameObject.SetActive(false);
                _chips.Add(chip);
            }
        }
    }
}
