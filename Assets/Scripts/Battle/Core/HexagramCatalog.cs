using System;
using System.Collections.Generic;

namespace BalartroLike.Battle
{
    public static class HexagramCatalog
    {
        private static readonly Dictionary<TrigramId, Dictionary<TrigramId, HexagramDefinition>> Definitions;

        static HexagramCatalog()
        {
            Definitions = new Dictionary<TrigramId, Dictionary<TrigramId, HexagramDefinition>>();
            AddRow(TrigramId.Qian, "乾为天", "天泽履", "天火同人", "天雷无妄", "天风姤", "天水讼", "天山遁", "天地否");
            AddRow(TrigramId.Dui, "泽天夬", "兑为泽", "泽火革", "泽雷随", "泽风大过", "泽水困", "泽山咸", "泽地萃");
            AddRow(TrigramId.Li, "火天大有", "火泽睽", "离为火", "火雷噬嗑", "火风鼎", "火水未济", "火山旅", "火地晋");
            AddRow(TrigramId.Zhen, "雷天大壮", "雷泽归妹", "雷火丰", "震为雷", "雷风恒", "雷水解", "雷山小过", "雷地豫");
            AddRow(TrigramId.Xun, "风天小畜", "风泽中孚", "风火家人", "风雷益", "巽为风", "风水涣", "风山渐", "风地观");
            AddRow(TrigramId.Kan, "水天需", "水泽节", "水火既济", "水雷屯", "水风井", "坎为水", "水山蹇", "水地比");
            AddRow(TrigramId.Gen, "山天大畜", "山泽损", "山火贲", "山雷颐", "山风蛊", "山水蒙", "艮为山", "山地剥");
            AddRow(TrigramId.Kun, "地天泰", "地泽临", "地火明夷", "地雷复", "地风升", "地水师", "地山谦", "坤为地");

            Override(TrigramId.Kun, TrigramId.Qian, 12000, null,
                new[] { EffectOperation.GainEnergy(1) });
            Override(TrigramId.Qian, TrigramId.Kun, 18000, null,
                new[] { EffectOperation.ApplyStatus(EffectTarget.Enemy, StatusId.Vulnerable, 1, 2) });
            Override(TrigramId.Li, TrigramId.Kan, null, null,
                new[] { EffectOperation.ApplyStatus(EffectTarget.Enemy, StatusId.Vulnerable, 1, 2) });
        }

        public static HexagramDefinition Get(TrigramId outer, TrigramId inner)
        {
            if (Definitions.TryGetValue(outer, out Dictionary<TrigramId, HexagramDefinition> row)
                && row.TryGetValue(inner, out HexagramDefinition definition))
            {
                return definition;
            }

            throw new ArgumentException("Unknown hexagram: outer=" + outer + ", inner=" + inner);
        }

        public static int Count
        {
            get
            {
                int count = 0;
                foreach (Dictionary<TrigramId, HexagramDefinition> row in Definitions.Values)
                {
                    count += row.Count;
                }

                return count;
            }
        }

        private static void AddRow(
            TrigramId outer,
            string qian,
            string dui,
            string li,
            string zhen,
            string xun,
            string kan,
            string gen,
            string kun)
        {
            Dictionary<TrigramId, HexagramDefinition> row = new Dictionary<TrigramId, HexagramDefinition>();
            Definitions.Add(outer, row);
            Add(row, outer, TrigramId.Qian, qian);
            Add(row, outer, TrigramId.Dui, dui);
            Add(row, outer, TrigramId.Li, li);
            Add(row, outer, TrigramId.Zhen, zhen);
            Add(row, outer, TrigramId.Xun, xun);
            Add(row, outer, TrigramId.Kan, kan);
            Add(row, outer, TrigramId.Gen, gen);
            Add(row, outer, TrigramId.Kun, kun);
        }

        private static void Add(
            Dictionary<TrigramId, HexagramDefinition> row,
            TrigramId outer,
            TrigramId inner,
            string displayName)
        {
            string id = outer.ToString().ToLowerInvariant() + "_" + inner.ToString().ToLowerInvariant();
            row.Add(inner, new HexagramDefinition(id, displayName, outer, inner));
        }

        private static void Override(
            TrigramId outer,
            TrigramId inner,
            int? damageMultiplier,
            bool? guaranteedCritical,
            EffectOperation[] additionalEffects)
        {
            HexagramDefinition current = Definitions[outer][inner];
            Definitions[outer][inner] = new HexagramDefinition(
                current.Id,
                current.DisplayName,
                current.Outer,
                current.Inner,
                damageMultiplier,
                guaranteedCritical,
                additionalEffects);
        }
    }
}