namespace BalartroLike.Battle
{
    public sealed class EnemyDefinition
    {
        public string Id { get; }
        public string DisplayName { get; }
        public ElementType Element { get; }
        public int MaxHp { get; }
        public int BasePower { get; }

        public EnemyDefinition(string id, string displayName, ElementType element, int maxHp, int basePower)
        {
            Id = id;
            DisplayName = displayName;
            Element = element;
            MaxHp = maxHp;
            BasePower = basePower;
        }
    }
}