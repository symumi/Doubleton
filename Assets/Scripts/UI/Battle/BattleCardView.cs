using System;
using BalartroLike.Battle;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BalartroLike.Unity
{
    // 单张手牌：由 HandPanelView 在运行时实例化并复用，手牌上限 6。
    public sealed class BattleCardView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image _frame;
        [SerializeField] private Image _glyph;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _qiText;
        [SerializeField] private TMP_Text _rankText;
        [SerializeField] private GameObject _innerTag;
        [SerializeField] private GameObject _outerTag;
        [SerializeField] private GameObject _disabledOverlay;

        private Action<BattleCardView> _onClicked;

        public int Uid { get; private set; }
        public CardInstance Card { get; private set; }
        public RectTransform RectTransform => (RectTransform)transform;

        public void Bind(CardInstance card, bool isInner, bool isOuter, bool interactable,
            Action<BattleCardView> onClick)
        {
            Card = card;
            Uid = card == null ? 0 : card.Uid;
            _onClicked = onClick;

            TrigramDefinition trigram = card == null ? null : TrigramCatalog.Get(card.Trigram);
            if (_glyph != null)
            {
                _glyph.sprite = card == null ? null : UISpriteCatalog.Trigram(card.Trigram);
                _glyph.enabled = card != null;
            }

            if (_nameText != null)
            {
                _nameText.text = trigram == null ? string.Empty : trigram.DisplayName;
            }

            if (_qiText != null)
            {
                _qiText.text = card == null ? string.Empty : card.Qi.ToString();
            }

            if (_rankText != null)
            {
                _rankText.text = card == null ? string.Empty : card.Rank + " 星";
            }

            if (_frame != null)
            {
                _frame.sprite = ResolveFrame(card, isInner || isOuter, interactable);
            }

            if (_innerTag != null)
            {
                _innerTag.SetActive(isInner);
            }

            if (_outerTag != null)
            {
                _outerTag.SetActive(isOuter);
            }

            if (_disabledOverlay != null)
            {
                _disabledOverlay.SetActive(!interactable);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _onClicked?.Invoke(this);
        }

        private static Sprite ResolveFrame(CardInstance card, bool selected, bool interactable)
        {
            if (!interactable)
            {
                return UISpriteCatalog.Frame("card_frame_disabled_9s");
            }

            if (selected)
            {
                return UISpriteCatalog.Frame("card_frame_selected_9s");
            }

            return card == null
                ? UISpriteCatalog.Frame("card_frame_normal_9s")
                : UISpriteCatalog.CardFrameForRank(card.Rank);
        }
    }
}
