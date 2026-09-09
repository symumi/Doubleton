using System;
using System.Collections.Generic;

namespace BalartroLike.Battle
{
    public static class TrigramCatalog
    {
        private static readonly Dictionary<TrigramId, TrigramDefinition> Definitions;

        static TrigramCatalog()
        {
            Definitions = new Dictionary<TrigramId, TrigramDefinition>();
            Add(new TrigramDefinition(
                TrigramId.Qian, "乾", "☰", ElementType.Metal, YinYangType.Yang, 2,
                15000, 1, new EffectOperation[0], 20000, false, new EffectOperation[0]));
            Add(new TrigramDefinition(
                TrigramId.Dui, "兑", "☱", ElementType.Metal, YinYangType.Yin, 2,
                10000, 1, new[] { EffectOperation.GainEnergy(1) }, 9000, false,
                new[] { EffectOperation.GainEnergy(1) }));
            Add(new TrigramDefinition(
                TrigramId.Li, "离", "☲", ElementType.Fire, YinYangType.Yang, 1,
                10000, 1, new[] { EffectOperation.ApplyStatus(EffectTarget.Enemy, StatusId.Burn, 2, 2) },
                10000, true, new EffectOperation[0]));
            Add(new TrigramDefinition(
                TrigramId.Zhen, "震", "☳", ElementType.Wood, YinYangType.Yang, 2,
                12000, 2, new EffectOperation[0], 12000, false, new EffectOperation[0]));
            Add(new TrigramDefinition(
                TrigramId.Xun, "巽", "☴", ElementType.Wood, YinYangType.Yin, 2,
                9000, 1, new[] { EffectOperation.ApplyStatus(EffectTarget.Enemy, StatusId.ArmorBreak, 1, 2) },
                10000, false, new EffectOperation[0]));
            Add(new TrigramDefinition(
                TrigramId.Kan, "坎", "☵", ElementType.Water, YinYangType.Yang, 2,
                8000, 1, new[] { EffectOperation.ApplyStatus(EffectTarget.Enemy, StatusId.Chill, 1, 2) },
                10000, false, new[] { EffectOperation.ApplyStatus(EffectTarget.Enemy, StatusId.Chill, 1, 2) }));
            Add(new TrigramDefinition(
                TrigramId.Gen, "艮", "☶", ElementType.Earth, YinYangType.Yang, 2,
                8000, 1, new[] { EffectOperation.GainShield(3) }, 10000, false,
                new[] { EffectOperation.GainShield(2) }));
            Add(new TrigramDefinition(
                TrigramId.Kun, "坤", "☷", ElementType.Earth, YinYangType.Yin, 2,
                8000, 1, new[] { EffectOperation.GainShield(2) }, 9000, false,
                new[] { EffectOperation.DrawCard(1) }));
        }

        public static IEnumerable<TrigramDefinition> All
        {
            get { return Definitions.Values; }
        }

        public static TrigramDefinition Get(TrigramId id)
        {
            if (Definitions.TryGetValue(id, out TrigramDefinition definition))
            {
                return definition;
            }

            throw new ArgumentException("Unknown trigram: " + id);
        }

        private static void Add(TrigramDefinition definition)
        {
            Definitions.Add(definition.Id, definition);
        }
    }
}