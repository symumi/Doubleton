using BalartroLike.Battle;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BalartroLike.Unity
{
    // 伤害预览：最终伤害、连击段数、五行克制提示、可斩杀标记与伤害明细。
    // 数值来自 BattleCalculation，保证与结算使用同一条计算路径。
    public sealed class DamagePreviewView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _damageText;
        [SerializeField] private TMP_Text _hitCountText;
        [SerializeField] private TMP_Text _elementHintText;
        [SerializeField] private TMP_Text _formulaText;
        [SerializeField] private GameObject _lethalMark;
        [SerializeField] private GameObject _breakdownRoot;
        [SerializeField] private Button _toggleBreakdownButton;
        [SerializeField] private Color _advantageColor = new Color(0.37f, 0.78f, 0.75f, 1f);
        [SerializeField] private Color _disadvantageColor = new Color(0.95f, 0.45f, 0.35f, 1f);
        [SerializeField] private Color _neutralColor = new Color(0.78f, 0.75f, 0.66f, 1f);

        private bool _breakdownVisible;

        public void Render(BattleCalculation calculation, EnemyState enemy)
        {
            if (calculation == null || calculation.Damage == null)
            {
                Clear();
                return;
            }

            DamageBreakdown damage = calculation.Damage;
            if (_damageText != null)
            {
                _damageText.text = damage.RawDamage.ToString();
            }

            if (_hitCountText != null)
            {
                _hitCountText.text = calculation.HitCount > 1 ? "× " + calculation.HitCount + " 段" : string.Empty;
            }

            if (_formulaText != null)
            {
                _formulaText.text = damage.ToFormula();
            }

            if (_lethalMark != null)
            {
                _lethalMark.SetActive(enemy != null && enemy.Hp > 0 && calculation.CanDefeat(enemy));
            }

            ApplyElementHint(damage.ElementMultiplier);
            SetBreakdownVisible(_breakdownVisible);
        }

        public void Clear()
        {
            if (_damageText != null)
            {
                _damageText.text = "--";
            }

            if (_hitCountText != null)
            {
                _hitCountText.text = string.Empty;
            }

            if (_elementHintText != null)
            {
                _elementHintText.text = string.Empty;
            }

            if (_formulaText != null)
            {
                _formulaText.text = string.Empty;
            }

            if (_lethalMark != null)
            {
                _lethalMark.SetActive(false);
            }

            SetBreakdownVisible(false);
        }

        public void ToggleBreakdown()
        {
            SetBreakdownVisible(!_breakdownVisible);
        }

        private void SetBreakdownVisible(bool visible)
        {
            _breakdownVisible = visible;
            if (_breakdownRoot != null)
            {
                _breakdownRoot.SetActive(visible);
            }
        }

        private void ApplyElementHint(int elementMultiplier)
        {
            if (_elementHintText == null)
            {
                return;
            }

            if (elementMultiplier == ElementRules.RestrainsMultiplier)
            {
                _elementHintText.text = "五行克制 ×1.50";
                _elementHintText.color = _advantageColor;
                return;
            }

            if (elementMultiplier == ElementRules.RestrainedMultiplier)
            {
                _elementHintText.text = "五行受制 ×0.67";
                _elementHintText.color = _disadvantageColor;
                return;
            }

            if (elementMultiplier == ElementRules.SameElementEnemyMultiplier)
            {
                _elementHintText.text = "同属性 ×0.85";
                _elementHintText.color = _neutralColor;
                return;
            }

            _elementHintText.text = string.Empty;
        }
    }
}
