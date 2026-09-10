using System.Collections.Generic;
using UnityEngine;

namespace BalartroLike.Unity
{
    // 弹窗栈：统一管理遮罩、层级和输入锁，禁止各页面自建遮罩与关闭逻辑。
    public sealed class UIPopupService : MonoBehaviour
    {
        [SerializeField] private RectTransform _popupLayer;
        [SerializeField] private GameObject _overlay;
        [SerializeField] private UIInputBlocker _inputBlocker;
        [SerializeField] private bool _closeOnEscape = true;

        private readonly List<GameObject> _stack = new List<GameObject>();

        public GameObject Current => _stack.Count == 0 ? null : _stack[_stack.Count - 1];
        public int Count => _stack.Count;
        public bool HasPopup => _stack.Count > 0;

        // 组件默认挂在 PopupLayer 上：层级取自身，输入锁向上查找 UIRoot 上的 UIInputBlocker。
        private void Awake()
        {
            if (_popupLayer == null)
            {
                _popupLayer = transform as RectTransform;
            }

            if (_inputBlocker == null)
            {
                _inputBlocker = GetComponentInParent<UIInputBlocker>();
            }

            if (_overlay == null)
            {
                Transform found = transform.Find("Overlay");
                if (found != null)
                {
                    _overlay = found.gameObject;
                }
            }
        }

        public T Show<T>(T popupPrefab) where T : Component
        {
            if (popupPrefab == null || _popupLayer == null)
            {
                return null;
            }

            T instance = Instantiate(popupPrefab, _popupLayer, false);
            Push(instance.gameObject);
            return instance;
        }

        public GameObject Show(GameObject popupPrefab)
        {
            if (popupPrefab == null || _popupLayer == null)
            {
                return null;
            }

            GameObject instance = Instantiate(popupPrefab, _popupLayer, false);
            Push(instance);
            return instance;
        }

        public void CloseTop()
        {
            if (_stack.Count == 0)
            {
                return;
            }

            int last = _stack.Count - 1;
            GameObject instance = _stack[last];
            _stack.RemoveAt(last);
            if (instance != null)
            {
                Destroy(instance);
            }

            if (_inputBlocker != null)
            {
                _inputBlocker.Pop();
            }

            SetOverlayActive(_stack.Count > 0);
        }

        public void CloseAll()
        {
            for (int i = _stack.Count - 1; i >= 0; i--)
            {
                if (_stack[i] != null)
                {
                    Destroy(_stack[i]);
                }

                if (_inputBlocker != null)
                {
                    _inputBlocker.Pop();
                }
            }

            _stack.Clear();
            SetOverlayActive(false);
        }

        private void Push(GameObject instance)
        {
            _stack.Add(instance);
            if (_inputBlocker != null)
            {
                _inputBlocker.Push();
            }

            SetOverlayActive(true);
        }

        private void SetOverlayActive(bool active)
        {
            if (_overlay != null)
            {
                _overlay.SetActive(active);
            }
        }

        private void Update()
        {
            // TODO: 引入 Input System 后改为 UI Action Map，当前沿用工程现有的旧 Input Manager。
            if (_closeOnEscape && _stack.Count > 0 && Input.GetKeyDown(KeyCode.Escape))
            {
                CloseTop();
            }
        }
    }
}
