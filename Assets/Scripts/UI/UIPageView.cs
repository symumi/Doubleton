using UnityEngine;

namespace BalartroLike.Unity
{
    // 一级页面基类：只负责显隐与自身刷新，不直接修改战斗或局内状态。
    public abstract class UIPageView : MonoBehaviour
    {
        public bool IsVisible => gameObject.activeSelf;

        public void Show()
        {
            gameObject.SetActive(true);
            OnShow();
        }

        public void Hide()
        {
            OnHide();
            gameObject.SetActive(false);
        }

        protected virtual void OnShow()
        {
        }

        protected virtual void OnHide()
        {
        }

        // TODO: 页面转场动画（ui_animation.csv 的 page_open）尚未接入，当前为直接切换。
    }
}
