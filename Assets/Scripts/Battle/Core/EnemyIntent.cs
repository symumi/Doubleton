namespace BalartroLike.Battle
{
    public sealed class EnemyIntent
    {
        public string Id { get; }
        public EnemyIntentType Type { get; }
        public int Power { get; }
        public StatusId Status { get; }
        public int StatusStacks { get; }
        public int StatusDuration { get; }
        public string DisplayText { get; }

        public EnemyIntent(
            EnemyIntentType type,
            int power,
            StatusId status = StatusId.None,
            int statusStacks = 0,
            int statusDuration = 0,
            string displayText = "",
            string id = "")
        {
            Id = id;
            Type = type;
            Power = power;
            Status = status;
            StatusStacks = statusStacks;
            StatusDuration = statusDuration;
            DisplayText = displayText;
        }
    }
}