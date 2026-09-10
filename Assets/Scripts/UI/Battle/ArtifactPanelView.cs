using System.Collections.Generic;
using BalartroLike.Battle;
using UnityEngine;

namespace BalartroLike.Unity
{
    // 法宝栏：运行时实例化 ArtifactSlotView，槽位数由 RunController 上限决定（MVP 为 4）。
    public sealed class ArtifactPanelView : MonoBehaviour
    {
        [SerializeField] private RectTransform _slotRoot;
        [SerializeField] private ArtifactSlotView _slotPrefab;
        [SerializeField] private int _minSlots = 4;

        private readonly List<ArtifactSlotView> _slots = new List<ArtifactSlotView>();

        public void Render(IReadOnlyList<ArtifactDefinition> artifacts)
        {
            int owned = artifacts == null ? 0 : artifacts.Count;
            int visible = Mathf.Max(_minSlots, owned);
            EnsureCount(visible);

            for (int i = 0; i < _slots.Count; i++)
            {
                bool used = i < visible;
                _slots[i].gameObject.SetActive(used);
                if (used)
                {
                    _slots[i].Bind(i < owned ? artifacts[i] : null);
                }
            }
        }

        private void EnsureCount(int count)
        {
            if (_slotPrefab == null || _slotRoot == null)
            {
                return;
            }

            while (_slots.Count < count)
            {
                ArtifactSlotView slot = Instantiate(_slotPrefab, _slotRoot, false);
                slot.gameObject.SetActive(false);
                _slots.Add(slot);
            }
        }
    }
}
