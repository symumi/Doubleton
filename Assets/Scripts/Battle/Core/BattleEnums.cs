namespace BalartroLike.Battle
{
    public enum TrigramId
    {
        Qian = 0,
        Dui = 1,
        Li = 2,
        Zhen = 3,
        Xun = 4,
        Kan = 5,
        Gen = 6,
        Kun = 7,
    }

    public enum ElementType
    {
        Metal = 0,
        Wood = 1,
        Water = 2,
        Fire = 3,
        Earth = 4,
    }

    public enum YinYangType
    {
        Yin = 0,
        Yang = 1,
    }

    public enum BattlePhase
    {
        None = 0,
        BattleStart = 1,
        PlayerTurnStart = 2,
        PlayerAction = 3,
        Resolving = 4,
        EnemyTurn = 5,
        BattleEnd = 6,
    }

    public enum BattleResultType
    {
        None = 0,
        Victory = 1,
        Defeat = 2,
    }

    public enum BattleCommandType
    {
        PlayHexagram = 0,
        DiscardCards = 1,
        EndTurn = 2,
    }

    public enum BattleEventType
    {
        BattleStarted = 0,
        PlayerTurnStarted = 1,
        CardDrawn = 2,
        CardDiscarded = 3,
        HexagramFormed = 4,
        DamageDealt = 5,
        ShieldChanged = 6,
        StatusChanged = 7,
        EnergyChanged = 8,
        EnemyIntentChanged = 9,
        EnemyActed = 10,
        BattleEnded = 11,
    }

    public enum EnemyIntentType
    {
        Attack = 0,
        Defend = 1,
        Debuff = 2,
    }

    public enum StatusId
    {
        None = 0,
        Burn = 1,
        Vulnerable = 2,
        Weak = 3,
        Shield = 4,
        ArmorBreak = 5,
        Chill = 6,
    }

    public enum EffectType
    {
        None = 0,
        Damage = 1,
        ApplyStatus = 2,
        GainShield = 3,
        DrawCard = 4,
        GainEnergy = 5,
        ModifyDamage = 6,
        ExtraTrigger = 7,
        ChangeIntent = 8,
    }

    public enum EffectTarget
    {
        Self = 0,
        Enemy = 1,
    }

    public enum ValueType
    {
        Flat = 0,
        Percent = 1,
        Multiplier = 2,
        Stacks = 3,
        Count = 4,
    }
}