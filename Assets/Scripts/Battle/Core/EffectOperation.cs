namespace BalartroLike.Battle
{
    public sealed class EffectOperation
    {
        public EffectType Type { get; }
        public EffectTarget Target { get; }
        public ValueType ValueType { get; }
        public int Value { get; }
        public StatusId Status { get; }
        public int Stacks { get; }
        public int Duration { get; }

        public EffectOperation(
            EffectType type,
            EffectTarget target,
            ValueType valueType,
            int value,
            StatusId status = StatusId.None,
            int stacks = 0,
            int duration = 0)
        {
            Type = type;
            Target = target;
            ValueType = valueType;
            Value = value;
            Status = status;
            Stacks = stacks;
            Duration = duration;
        }

        public static EffectOperation ApplyStatus(EffectTarget target, StatusId status, int stacks, int duration)
        {
            return new EffectOperation(EffectType.ApplyStatus, target, ValueType.Stacks, stacks, status, stacks, duration);
        }

        public static EffectOperation GainShield(int value)
        {
            return new EffectOperation(EffectType.GainShield, EffectTarget.Self, ValueType.Flat, value);
        }

        public static EffectOperation GainEnergy(int value)
        {
            return new EffectOperation(EffectType.GainEnergy, EffectTarget.Self, ValueType.Flat, value);
        }

        public static EffectOperation DrawCard(int value)
        {
            return new EffectOperation(EffectType.DrawCard, EffectTarget.Self, ValueType.Count, value);
        }
    }
}