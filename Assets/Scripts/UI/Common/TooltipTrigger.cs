using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BalartroLike.Unity
{
    // 挂在可悬停对象上：延时后向 UIRoot 的 TooltipView 请求显示说明。
    public sealed class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private string _title = string.Empty;
        [SerializeField, TextArea] private string _body = string.Empty;
        [SerializeField] private float _delay = 0.4f;

        private Coroutine _routine;
        private bool _hovering;

        public void SetContent(string title, string body)
        {
            _title = title ?? string.Empty;
            _body = body ?? string.Empty;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_title.Length == 0 && _body.Length == 0)
            {
                return;
            }

            _hovering = true;
            StopRoutine();
            _routine = StartCoroutine(ShowAfterDelay());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Hide();
        }

        private void OnDisable()
        {
            Hide();
        }

        private IEnumerator ShowAfterDelay()
        {
            if (_delay > 0f)
            {
                yield return new WaitForSecondsRealtime(_delay);
            }

            _routine = null;
            if (!_hovering)
            {
                yield break;
            }

            UIRoot root = UIRoot.Instance;
            if (root == null || root.Tooltip == null)
            {
                yield break;
            }

            root.Tooltip.Show(_title, _body, Input.mousePosition);
        }

        private void Hide()
        {
            _hovering = false;
            StopRoutine();
            UIRoot root = UIRoot.Instance;
            if (root != null && root.Tooltip != null)
            {
                root.Tooltip.Hide();
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
