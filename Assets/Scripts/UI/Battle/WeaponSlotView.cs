using BalartroLike.Battle;
using TMPro;
using UnityEngine;

namespace BalartroLike.Unity
{
    // 武器与附魔槽：MVP 固定 3 个附魔槽，槽位文本在 Prefab 里预置（不运行时实例化）。
    public sealed class WeaponSlotView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _patternText;
        [SerializeField] private TMP_Text[] _enchantTexts = new TMP_Text[0];
        [SerializeField] private GameObject _replaceHint;

        public void Render(WeaponState weapon, bool willReplaceOldest)
        {
            if (weapon == null)
            {
                return;
            }

            if (_nameText != null)
            {
                _nameText.text = weapon.DisplayName;
            }

            if (_patternText != null)
            {
                _patternText.text = PatternLabel(weapon.AttackPattern) + " · 力道 " + weapon.BasePower;
            }

            for (int i = 0; i < _enchantTexts.Length; i++)
            {
                if (_enchantTexts[i] == null)
                {
                    continue;
                }

                _enchantTexts[i].text = i < weapon.Enchantments.Count
                    ? DescribeEnchant(weapon.Enchantments[i])
                    : "空槽";
            }

            if (_replaceHint != null)
            {
                _replaceHint.SetActive(willReplaceOldest);
            }
        }

        private static string DescribeEnchant(EnchantInstance enchant)
        {
            if (enchant == null || enchant.Effect == null)
            {
                return "空槽";
            }

            return TrigramCatalog.Get(enchant.Source).DisplayName
                + " · " + EffectLabel(enchant.Effect.Type)
                + " · 剩 " + enchant.RemainingTurns + " 回合";
        }

        private static string EffectLabel(EffectType type)
        {
            switch (type)
            {
                case EffectType.Damage:
                    return "追加伤害";
                case EffectType.ApplyStatus:
                    return "施加状态";
                case EffectType.GainShield:
                    return "获得护盾";
                case EffectType.DrawCard:
                    return "抽牌";
                case EffectType.GainEnergy:
                    return "回灵";
                default:
                    return type.ToString();
            }
        }

        private static string PatternLabel(AttackPattern pattern)
        {
            switch (pattern)
            {
                case AttackPattern.MultiHit:
                    return "连击";
                case AttackPattern.AllTargets:
                    return "群攻";
                default:
                    return "直伤";
            }
        }
    }
}
