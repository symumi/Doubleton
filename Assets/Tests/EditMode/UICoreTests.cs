using System.Collections.Generic;
using BalartroLike.Unity;
using NUnit.Framework;
using UnityEngine;

namespace BalartroLike.Tests
{
    public sealed class UICoreTests
    {
        private readonly List<GameObject> _created = new List<GameObject>();

        [TearDown]
        public void Cleanup()
        {
            for (int i = 0; i < _created.Count; i++)
            {
                if (_created[i] != null)
                {
                    Object.DestroyImmediate(_created[i]);
                }
            }

            _created.Clear();
        }

        private GameObject CreateObject(string name)
        {
            GameObject created = new GameObject(name);
            _created.Add(created);
            return created;
        }

        // ---------------------------------------------------------- UIAnimationConfig

        [Test]
        public void AnimationConfig_ParsesConfiguredEntry()
        {
            UIAnimationConfig config = UIAnimationConfig.Parse(
                "key,duration,easing,offsetY,startScale,endScale,shakeDistance,shakeFrequency,fadeStart,punchScale,delay\n" +
                "card_select,0.14,OutCubic,6,1.03,1,2,3,0.5,1.08,0.02\n");

            UIAnimationEntry entry = config.Get("card_select");

            Assert.AreEqual(1, config.Count);
            Assert.AreEqual(0.14f, entry.Duration, 0.0001f);
            Assert.AreEqual(UITweenEasing.OutCubic, entry.Easing);
            Assert.AreEqual(6f, entry.OffsetY, 0.0001f);
            Assert.AreEqual(1.03f, entry.StartScale, 0.0001f);
            Assert.AreEqual(2f, entry.ShakeDistance, 0.0001f);
            Assert.AreEqual(3f, entry.ShakeFrequency, 0.0001f);
            Assert.AreEqual(0.5f, entry.FadeStart, 0.0001f);
            Assert.AreEqual(1.08f, entry.PunchScale, 0.0001f);
            Assert.AreEqual(0.02f, entry.Delay, 0.0001f);
            Assert.IsTrue(entry.HasFade);
        }

        [Test]
        public void AnimationConfig_UsesDefaultForUnknownKey()
        {
            UIAnimationConfig config = UIAnimationConfig.Parse("key,duration,easing\ndamage_number,0.2,OutBack\n");

            Assert.IsFalse(config.TryGet("missing_key", out _));
            UIAnimationEntry fallback = config.Get("missing_key");

            Assert.AreEqual(0.18f, fallback.Duration, 0.0001f);
            Assert.AreEqual(UITweenEasing.OutCubic, fallback.Easing);
            Assert.IsFalse(fallback.HasFade);
        }

        [Test]
        public void AnimationConfig_ParsesPartialRowWithDefaults()
        {
            UIAnimationConfig config = UIAnimationConfig.Parse("key,duration,easing\npage_open,0.3\n");

            UIAnimationEntry entry = config.Get("page_open");

            Assert.AreEqual(0.3f, entry.Duration, 0.0001f);
            Assert.AreEqual(UITweenEasing.OutCubic, entry.Easing);
            Assert.AreEqual(1f, entry.StartScale, 0.0001f);
        }

        [Test]
        public void AnimationConfig_SkipsCommentsAndBlankLines()
        {
            UIAnimationConfig config = UIAnimationConfig.Parse(
                "# 注释\n\nkey,duration,easing\n# 另一条注释\ntoast_in,0.16,OutQuad\n");

            Assert.AreEqual(1, config.Count);
            Assert.AreEqual(UITweenEasing.OutQuad, config.Get("toast_in").Easing);
        }

        [Test]
        public void AnimationConfig_EmptyCsvReturnsEmptyConfig()
        {
            UIAnimationConfig config = UIAnimationConfig.Parse(string.Empty);

            Assert.AreEqual(0, config.Count);
        }

        [Test]
        public void AnimationConfig_ShippedCsvContainsUiKeys()
        {
            UIAnimationConfig config = UIAnimationConfig.Load();

            Assert.Greater(config.Count, 0);
            Assert.IsTrue(config.TryGet("card_select", out _));
            Assert.IsTrue(config.TryGet("damage_number", out _));
            Assert.IsTrue(config.TryGet("toast_in", out _));
            Assert.IsTrue(config.TryGet("popup_open", out _));
        }

        // ---------------------------------------------------------- UITween

        [Test]
        public void Tween_EasingEndpointsAreStable()
        {
            Assert.AreEqual(0f, UITween.Evaluate(UITweenEasing.OutCubic, 0f), 0.0001f);
            Assert.AreEqual(1f, UITween.Evaluate(UITweenEasing.OutCubic, 1f), 0.0001f);
            Assert.AreEqual(1f, UITween.Evaluate(UITweenEasing.OutBack, 1f), 0.0001f);
            Assert.AreEqual(1f, UITween.Evaluate(UITweenEasing.InOutCubic, 1f), 0.0001f);
        }

        [Test]
        public void Tween_OutBackOvershoots()
        {
            Assert.Greater(UITween.Evaluate(UITweenEasing.OutBack, 0.7f), 1f);
        }

        [Test]
        public void Tween_ClampsOutOfRangeProgress()
        {
            Assert.AreEqual(0f, UITween.Evaluate(UITweenEasing.Linear, -1f), 0.0001f);
            Assert.AreEqual(1f, UITween.Evaluate(UITweenEasing.Linear, 2f), 0.0001f);
        }

        // ---------------------------------------------------------- UIInputBlocker

        [Test]
        public void InputBlocker_CountsNestedLocks()
        {
            UIInputBlocker blocker = CreateObject("InputBlocker").AddComponent<UIInputBlocker>();
            int changeCount = 0;
            blocker.BlockedChanged += _ => changeCount++;

            Assert.IsFalse(blocker.IsBlocked);

            blocker.Push();
            blocker.Push();
            Assert.AreEqual(2, blocker.LockCount);
            Assert.IsTrue(blocker.IsBlocked);

            blocker.Pop();
            Assert.IsTrue(blocker.IsBlocked);

            blocker.Pop();
            Assert.IsFalse(blocker.IsBlocked);
            Assert.AreEqual(2, changeCount);
        }

        [Test]
        public void InputBlocker_PopBelowZeroIsIgnored()
        {
            UIInputBlocker blocker = CreateObject("InputBlocker").AddComponent<UIInputBlocker>();

            blocker.Pop();

            Assert.AreEqual(0, blocker.LockCount);
            Assert.IsFalse(blocker.IsBlocked);
        }

        [Test]
        public void InputBlocker_ResetLocksUnblocks()
        {
            UIInputBlocker blocker = CreateObject("InputBlocker").AddComponent<UIInputBlocker>();
            blocker.Push();
            blocker.Push();

            blocker.ResetLocks();

            Assert.AreEqual(0, blocker.LockCount);
            Assert.IsFalse(blocker.IsBlocked);
        }

        // ---------------------------------------------------------- UIPageService

        private sealed class StubPage : UIPageView
        {
            public int ShowCount { get; private set; }
            public int HideCount { get; private set; }

            protected override void OnShow()
            {
                ShowCount++;
            }

            protected override void OnHide()
            {
                HideCount++;
            }
        }

        private StubPage CreatePage(string name)
        {
            return CreateObject(name).AddComponent<StubPage>();
        }

        [Test]
        public void PageService_OpenHidesPreviousPage()
        {
            UIPageService service = CreateObject("PageService").AddComponent<UIPageService>();
            StubPage first = CreatePage("PageA");
            StubPage second = CreatePage("PageB");

            service.Open(first);
            service.Open(second);

            Assert.AreEqual(2, service.Count);
            Assert.AreSame(second, service.Current);
            Assert.IsTrue(second.IsVisible);
            Assert.IsFalse(first.IsVisible);
            Assert.AreEqual(1, first.ShowCount);
            Assert.AreEqual(1, first.HideCount);
        }

        [Test]
        public void PageService_BackReturnsToPreviousPage()
        {
            UIPageService service = CreateObject("PageService").AddComponent<UIPageService>();
            StubPage first = CreatePage("PageA");
            StubPage second = CreatePage("PageB");
            service.Open(first);
            service.Open(second);

            service.Back();

            Assert.AreEqual(1, service.Count);
            Assert.AreSame(first, service.Current);
            Assert.IsTrue(first.IsVisible);
            Assert.AreEqual(2, first.ShowCount);
            Assert.IsFalse(second.IsVisible);
        }

        [Test]
        public void PageService_ReplaceSwapsCurrentPage()
        {
            UIPageService service = CreateObject("PageService").AddComponent<UIPageService>();
            StubPage first = CreatePage("PageA");
            StubPage second = CreatePage("PageB");
            service.Open(first);

            service.Replace(second);

            Assert.AreEqual(1, service.Count);
            Assert.AreSame(second, service.Current);
            Assert.IsFalse(first.IsVisible);
        }

        [Test]
        public void PageService_CloseAllClearsStack()
        {
            UIPageService service = CreateObject("PageService").AddComponent<UIPageService>();
            StubPage first = CreatePage("PageA");
            StubPage second = CreatePage("PageB");
            service.Open(first);
            service.Open(second);

            service.CloseAll();

            Assert.AreEqual(0, service.Count);
            Assert.IsNull(service.Current);
            Assert.IsFalse(first.IsVisible);
            Assert.IsFalse(second.IsVisible);
        }

        [Test]
        public void PageService_OpenSamePageTwiceIsIgnored()
        {
            UIPageService service = CreateObject("PageService").AddComponent<UIPageService>();
            StubPage first = CreatePage("PageA");

            service.Open(first);
            service.Open(first);

            Assert.AreEqual(1, service.Count);
            Assert.AreEqual(1, first.ShowCount);
        }
    }
}
