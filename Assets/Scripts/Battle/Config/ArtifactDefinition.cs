namespace BalartroLike.Battle
{
    public sealed class ArtifactDefinition
    {
        public string Id { get; }
        public string DisplayName { get; }
        public ArtifactTriggerType TriggerType { get; }
        public ArtifactConditionType ConditionType { get; }
        public string ConditionValue { get; }
        public EffectOperation[] Effects { get; }
        public string Description { get; }

        public ArtifactDefinition(
            string id,
            string displayName,
            ArtifactTriggerType triggerType,
            ArtifactConditionType conditionType,
            string conditionValue,
            EffectOperation[] effects,
            string description)
        {
            Id = id;
            DisplayName = displayName;
            TriggerType = triggerType;
            ConditionType = conditionType;
            ConditionValue = conditionValue;
            Effects = effects;
            Description = description;
        }
    }
}
