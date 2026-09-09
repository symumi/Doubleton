using System.Collections.Generic;

namespace BalartroLike.Battle
{
    public sealed class EnemyDefinition
    {
        public string Id { get; }
        public string DisplayName { get; }
        public EnemyKind Kind { get; }
        public ElementType Element { get; }
        public int MaxHp { get; }
        public int BasePower { get; }
        public EnemyIntentMode IntentMode { get; }
        public IReadOnlyList<EnemyIntentDefinition> Intents { get; }
        public EnemyRuleType RuleType { get; }

        public EnemyDefinition(
            string id,
            string displayName,
            EnemyKind kind,
            ElementType element,
            int maxHp,
            int basePower,
            EnemyIntentMode intentMode,
            IReadOnlyList<EnemyIntentDefinition> intents,
            EnemyRuleType ruleType)
        {
            Id = id;
            DisplayName = displayName;
            Kind = kind;
            Element = element;
            MaxHp = maxHp;
            BasePower = basePower;
            IntentMode = intentMode;
            Intents = intents;
            RuleType = ruleType;
        }
    }
}