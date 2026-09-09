namespace BalartroLike.Run
{
    public sealed class RunEventOptionDefinition
    {
        public string EventId { get; }
        public string Title { get; }
        public string Description { get; }
        public string OptionId { get; }
        public string OptionText { get; }
        public RunEffectType EffectType { get; }
        public int EffectValue { get; }
        public string ResultText { get; }

        public RunEventOptionDefinition(
            string eventId,
            string title,
            string description,
            string optionId,
            string optionText,
            RunEffectType effectType,
            int effectValue,
            string resultText)
        {
            EventId = eventId;
            Title = title;
            Description = description;
            OptionId = optionId;
            OptionText = optionText;
            EffectType = effectType;
            EffectValue = effectValue;
            ResultText = resultText;
        }
    }
}
