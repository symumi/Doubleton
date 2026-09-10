using BalartroLike.Battle;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BalartroLike.Unity
{
    // 卦台内外卦槽：显示当前选中的内卦（下）与外卦（上）。
    public sealed class HexagramAltarView : MonoBehaviour
    {
        [SerializeField] private Image _innerGlyph;
        [SerializeField] private TMP_Text _innerNameText;
        [SerializeField] private GameObject _innerEmptyHint;
        [SerializeField] private Image _outerGlyph;
        [SerializeField] private TMP_Text _outerNameText;
        [SerializeField] private GameObject _outerEmptyHint;

        public void Render(CardInstance inner, CardInstance outer)
        {
            ApplySlot(_innerGlyph, _innerNameText, _innerEmptyHint, inner, "请选择内卦");
            ApplySlot(_outerGlyph, _outerNameText, _outerEmptyHint, outer, "请选择外卦");
        }

        private static void ApplySlot(Image glyph, TMP_Text label, GameObject emptyHint, CardInstance card,
            string emptyText)
        {
            bool hasCard = card != null;
            if (glyph != null)
            {
                glyph.sprite = hasCard ? UISpriteCatalog.Trigram(card.Trigram) : null;
                glyph.enabled = hasCard;
            }

            if (label != null)
            {
                label.text = hasCard
                    ? TrigramCatalog.Get(card.Trigram).DisplayName + " · 卦力 " + card.Qi
                    : emptyText;
            }

            if (emptyHint != null)
            {
                emptyHint.SetActive(!hasCard);
            }
        }
    }
}
