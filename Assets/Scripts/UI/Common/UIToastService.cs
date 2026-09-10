using System.Collections.Generic;
using UnityEngine;

namespace BalartroLike.Unity
{
    // 短提示队列：同时最多显示 _maxVisible 条，溢出排队，避免提示互相遮挡关键数值。
    public sealed class UIToastService : MonoBehaviour
    {
        [SerializeField] private RectTransform _toastLayer;
        [SerializeField] private ToastView _toastPrefab;
        [SerializeField] private int _maxVisible = 3;

        private readonly List<ToastView> _active = new List<ToastView>();
        private readonly Queue<string> _pending = new Queue<string>();

        public int ActiveCount => _active.Count;
        public int PendingCount => _pending.Count;

        // 组件默认挂在 ToastLayer 上，未拖引用时直接使用自身层级。
        private void Awake()
        {
            if (_toastLayer == null)
            {
                _toastLayer = transform as RectTransform;
            }
        }

        public void Show(string message)
        {
            if (string.IsNullOrEmpty(message) || _toastPrefab == null || _toastLayer == null)
            {
                return;
            }

            if (_active.Count >= Mathf.Max(1, _maxVisible))
            {
                _pending.Enqueue(message);
                return;
            }

            Spawn(message);
        }

        public void Clear()
        {
            for (int i = 0; i < _active.Count; i++)
            {
                if (_active[i] != null)
                {
                    Destroy(_active[i].gameObject);
                }
            }

            _active.Clear();
            _pending.Clear();
        }

        private void Spawn(string message)
        {
            ToastView view = Instantiate(_toastPrefab, _toastLayer, false);
            _active.Add(view);
            view.Play(message, OnToastFinished);
        }

        private void OnToastFinished(ToastView view)
        {
            _active.Remove(view);
            if (view != null)
            {
                Destroy(view.gameObject);
            }

            if (_pending.Count > 0)
            {
                Spawn(_pending.Dequeue());
            }
        }
    }
}
