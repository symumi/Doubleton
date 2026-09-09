using System;

namespace BalartroLike.Battle
{
    public static class ArtifactRules
    {
        private const int BasisPoints = 10000;

        public static void CollectDamageModifiers(
            BattleState state,
            HexagramDefinition hexagram,
            out int additiveMultiplier,
            out int multiplicativeMultiplier)
        {
            additiveMultiplier = BasisPoints;
            multiplicativeMultiplier = BasisPoints;
            for (int i = 0; i < state.Artifacts.Count; i++)
            {
                ArtifactDefinition artifact = state.Artifacts[i];
                if (artifact.TriggerType != ArtifactTriggerType.BeforeDamage || !Matches(state, artifact, hexagram))
                {
                    continue;
                }

                for (int j = 0; j < artifact.Effects.Length; j++)
                {
                    EffectOperation effect = artifact.Effects[j];
                    if (effect.Type != EffectType.ModifyDamage)
                    {
                        continue;
                    }

                    if (effect.ValueType == ValueType.Percent)
                    {
                        additiveMultiplier += effect.Value;
                    }
                    else if (effect.ValueType == ValueType.Multiplier)
                    {
                        multiplicativeMultiplier = Multiply(multiplicativeMultiplier, effect.Value);
                    }
                }
            }
        }

        public static bool Matches(BattleState state, ArtifactDefinition artifact, HexagramDefinition hexagram)
        {
            switch (artifact.ConditionType)
            {
                case ArtifactConditionType.Always:
                    return true;
                case ArtifactConditionType.FirstPlayEachTurn:
                    return state.PlaysThisTurn == 0;
                case ArtifactConditionType.InnerElement:
                    return Enum.TryParse(artifact.ConditionValue, true, out ElementType element)
                        && TrigramCatalog.Get(hexagram.Inner).Element == element;
                case ArtifactConditionType.Hexagram:
                    return string.Equals(hexagram.Id, artifact.ConditionValue, StringComparison.OrdinalIgnoreCase);
                default:
                    return false;
            }
        }

        private static int Multiply(int left, int right)
        {
            return (int)(((long)left * right + BasisPoints / 2) / BasisPoints);
        }
    }
}
