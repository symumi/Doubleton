using System;
using UnityEngine;

namespace BalartroLike.Unity
{
    // 动画、结算和弹窗播放期间统一拦截 UI 输入，按引用计数避免提前解锁。
    // blockerLayer 是独立的全屏透明 Image（Raycast Target 开启），隐藏时不影响其他面板。
    public sealed class UIInputBlocker : MonoBehaviour
    {
        [SerializeField] private GameObject _blockerLayer;

        private int _lockCount;

        public bool IsBlocked => _lockCount > 0;
        public int LockCount => _lockCount;

        public event Action<bool> BlockedChanged;

        private void Awake()
        {
            Apply();
        }

        public void Push()
        {
            SetCount(_lockCount + 1);
        }

        public void Pop()
        {
            SetCount(Mathf.Max(0, _lockCount - 1));
        }

        public void ResetLocks()
        {
            SetCount(0);
        }

        private void SetCount(int value)
        {
            if (_lockCount == value)
            {
                return;
            }

            bool wasBlocked = IsBlocked;
            _lockCount = value;
            Apply();
            if (wasBlocked != IsBlocked)
            {
                BlockedChanged?.Invoke(IsBlocked);
            }
        }

        private void Apply()
        {
            if (_blockerLayer != null)
            {
                _blockerLayer.SetActive(_lockCount > 0);
            }
        }
    }
}
