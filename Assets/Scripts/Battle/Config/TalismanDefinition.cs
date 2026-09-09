namespace BalartroLike.Battle
{
    public sealed class TalismanDefinition
    {
        public string Id { get; }
        public string DisplayName { get; }
        public EffectOperation[] Effects { get; }
        public string Description { get; }

        public TalismanDefinition(string id, string displayName, EffectOperation[] effects, string description)
        {
            Id = id;
            DisplayName = displayName;
            Effects = effects;
            Description = description;
        }
    }
}
