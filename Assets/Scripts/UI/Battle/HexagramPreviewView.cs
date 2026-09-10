using BalartroLike.Battle;
using TMPro;
using UnityEngine;

namespace BalartroLike.Unity
{
    // 卦象预览：卦名、上下卦组合、效果描述、特殊卦与精通标记。
    public sealed class HexagramPreviewView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _compositionText;
        [SerializeField] private TMP_Text _effectText;
        [SerializeField] private TMP_Text _masteryText;
        [SerializeField] private GameObject _specialMark;

        public void Render(BattleCalculation calculation, int masteryLevel)
        {
            if (calculation == null || calculation.Hexagram == null)
            {
                Clear();
                return;
            }

            HexagramDefinition hexagram = calculation.Hexagram;
            if (_nameText != null)
            {
                _nameText.text = hexagram.DisplayName;
            }

            if (_compositionText != null)
            {
                _compositionText.text = "上卦 " + TrigramName(hexagram.Outer) + " ／ 下卦 " + TrigramName(hexagram.Inner);
            }

            if (_effectText != null)
            {
                _effectText.text = string.IsNullOrEmpty(hexagram.Description) ? "基础卦象，无额外效果" : hexagram.Description;
            }

            if (_masteryText != null)
            {
                _masteryText.text = masteryLevel > 0
                    ? "熟练 " + masteryLevel + " 级" + (masteryLevel >= HexagramMastery.MasteredLevel ? "（精通 ×1.20）" : string.Empty)
                    : string.Empty;
            }

            if (_specialMark != null)
            {
                _specialMark.SetActive(hexagram.IsSpecial);
            }
        }

        public void Clear()
        {
            if (_nameText != null)
            {
                _nameText.text = "尚未成卦";
            }

            if (_compositionText != null)
            {
                _compositionText.text = "选择两张手牌组合卦象";
            }

            if (_effectText != null)
            {
                _effectText.text = string.Empty;
            }

            if (_masteryText != null)
            {
                _masteryText.text = string.Empty;
            }

            if (_specialMark != null)
            {
                _specialMark.SetActive(false);
            }
        }

        private static string TrigramName(TrigramId id)
        {
            return TrigramCatalog.Get(id).DisplayName;
        }
    }
}
