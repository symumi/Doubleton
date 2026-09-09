using System;
using System.Collections.Generic;

namespace BalartroLike.Battle
{
    public sealed class EnemyIntentSelector
    {
        public EnemyIntent SelectNext(EnemyState enemy, int turn, Random random)
        {
            if (enemy.Intents.Count == 0)
            {
                throw new InvalidOperationException("Enemy intent pool is empty.");
            }

            EnemyIntentDefinition definition = enemy.IntentMode == EnemyIntentMode.Weighted
                ? SelectWeighted(enemy, turn, random)
                : SelectSequence(enemy, turn);
            enemy.RecordIntentSelected(definition, turn);
            int power = definition.Type == EnemyIntentType.Attack ? definition.Power + enemy.PowerBonus : definition.Power;
            string displayText = definition.Type == EnemyIntentType.Attack && enemy.PowerBonus > 0
                ? definition.DisplayText + "（+" + enemy.PowerBonus + "）"
                : definition.DisplayText;
            return new EnemyIntent(
                definition.Type,
                power,
                definition.Status,
                definition.StatusStacks,
                definition.StatusDuration,
                displayText,
                definition.Id);
        }

        private static EnemyIntentDefinition SelectSequence(EnemyState enemy, int turn)
        {
            int count = enemy.Intents.Count;
            for (int offset = 0; offset < count; offset++)
            {
                int index = (enemy.IntentSequenceIndex + offset) % count;
                EnemyIntentDefinition intent = enemy.Intents[index];
                if (!MatchesCondition(enemy, intent, turn))
                {
                    continue;
                }

                enemy.SetIntentSequenceIndex((index + 1) % count);
                return intent;
            }

            enemy.SetIntentSequenceIndex(count > 1 ? 1 : 0);
            return enemy.Intents[0];
        }

        private static EnemyIntentDefinition SelectWeighted(EnemyState enemy, int turn, Random random)
        {
            List<EnemyIntentDefinition> eligible = new List<EnemyIntentDefinition>();
            int totalWeight = 0;
            for (int i = 0; i < enemy.Intents.Count; i++)
            {
                EnemyIntentDefinition intent = enemy.Intents[i];
                if (intent.Weight <= 0 || !MatchesCondition(enemy, intent, turn))
                {
                    continue;
                }

                eligible.Add(intent);
                totalWeight += intent.Weight;
            }

            if (eligible.Count == 0)
            {
                return enemy.Intents[0];
            }

            int roll = random.Next(totalWeight);
            for (int i = 0; i < eligible.Count; i++)
            {
                roll -= eligible[i].Weight;
                if (roll < 0)
                {
                    return eligible[i];
                }
            }

            return eligible[eligible.Count - 1];
        }

        private static bool MatchesCondition(EnemyState enemy, EnemyIntentDefinition intent, int turn)
        {
            if (!enemy.IsIntentReady(intent, turn))
            {
                return false;
            }

            switch (intent.ConditionType)
            {
                case EnemyIntentConditionType.HpBelow:
                    return (long)enemy.Hp * 100 <= (long)enemy.MaxHp * intent.ConditionValue;
                case EnemyIntentConditionType.HpAbove:
                    return (long)enemy.Hp * 100 >= (long)enemy.MaxHp * intent.ConditionValue;
                case EnemyIntentConditionType.TurnAtLeast:
                    return turn >= intent.ConditionValue;
                default:
                    return true;
            }
        }
    }
}