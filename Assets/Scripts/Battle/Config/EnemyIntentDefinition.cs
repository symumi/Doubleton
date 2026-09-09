namespace BalartroLike.Battle
{
    public sealed class EnemyIntentDefinition
    {
        public string Id { get; }
        public EnemyIntentType Type { get; }
        public int Power { get; }
        public StatusId Status { get; }
        public int StatusStacks { get; }
        public int StatusDuration { get; }
        public int Weight { get; }
        public int Cooldown { get; }
        public EnemyIntentConditionType ConditionType { get; }
        public int ConditionValue { get; }
        public string DisplayText { get; }

        public EnemyIntentDefinition(
            string id,
            EnemyIntentType type,
            int power,
            StatusId status,
            int statusStacks,
            int statusDuration,
            int weight,
            int cooldown,
            EnemyIntentConditionType conditionType,
            int conditionValue,
            string displayText)
        {
            Id = id;
            Type = type;
            Power = power;
            Status = status;
            StatusStacks = statusStacks;
            StatusDuration = statusDuration;
            Weight = weight;
            Cooldown = cooldown;
            ConditionType = conditionType;
            ConditionValue = conditionValue;
            DisplayText = displayText;
        }
    }
}