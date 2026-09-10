using System.Collections.Generic;
using UnityEngine;

namespace BalartroLike.Unity
{
    // 一级页面栈：同一时间只显示栈顶页面，Back 回到上一个页面。
    public sealed class UIPageService : MonoBehaviour
    {
        private readonly List<UIPageView> _stack = new List<UIPageView>();

        public UIPageView Current => _stack.Count == 0 ? null : _stack[_stack.Count - 1];
        public int Count => _stack.Count;

        public bool Contains(UIPageView page)
        {
            return page != null && _stack.Contains(page);
        }

        // 打开新页面：压栈并隐藏当前页面。
        public void Open(UIPageView page)
        {
            if (page == null || Current == page)
            {
                return;
            }

            if (Current != null)
            {
                Current.Hide();
            }

            _stack.Add(page);
            page.Show();
        }

        // 替换当前页面：用于结算、晋升这类不需要回退的流程节点。
        public void Replace(UIPageView page)
        {
            if (page == null)
            {
                return;
            }

            CloseCurrent();
            _stack.Add(page);
            page.Show();
        }

        public void CloseCurrent()
        {
            if (_stack.Count == 0)
            {
                return;
            }

            int last = _stack.Count - 1;
            UIPageView page = _stack[last];
            _stack.RemoveAt(last);
            page.Hide();
        }

        // 回到上一个页面：栈内不足两页时不做任何事。
        public void Back()
        {
            if (_stack.Count < 2)
            {
                return;
            }

            CloseCurrent();
            Current.Show();
        }

        public void CloseAll()
        {
            for (int i = _stack.Count - 1; i >= 0; i--)
            {
                _stack[i].Hide();
            }

            _stack.Clear();
        }
    }
}
