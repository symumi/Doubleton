using System;
using System.Collections.Generic;
using BalartroLike.Battle;
using UnityEngine;

namespace BalartroLike.Unity
{
    // 手牌区：运行时实例化 BattleCardView 并复用，配合 HorizontalLayoutGroup 自动排布。
    public sealed class HandPanelView : MonoBehaviour
    {
        [SerializeField] private RectTransform _cardRoot;
        [SerializeField] private BattleCardView _cardPrefab;

        private readonly List<BattleCardView> _views = new List<BattleCardView>();

        public int VisibleCount { get; private set; }

        public void Render(IReadOnlyList<CardInstance> hand, int innerUid, int outerUid, bool interactable,
            Action<BattleCardView> onClick)
        {
            int count = hand == null ? 0 : hand.Count;
            VisibleCount = count;
            EnsureCount(count);

            for (int i = 0; i < _views.Count; i++)
            {
                bool used = i < count;
                _views[i].gameObject.SetActive(used);
                if (!used)
                {
                    continue;
                }

                CardInstance card = hand[i];
                _views[i].Bind(card, card.Uid == innerUid, card.Uid == outerUid, interactable, onClick);
            }
        }

        private void EnsureCount(int count)
        {
            if (_cardPrefab == null || _cardRoot == null)
            {
                return;
            }

            while (_views.Count < count)
            {
                BattleCardView view = Instantiate(_cardPrefab, _cardRoot, false);
                view.gameObject.SetActive(false);
                _views.Add(view);
            }
        }
    }
}
