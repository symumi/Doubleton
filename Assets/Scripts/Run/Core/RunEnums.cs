namespace BalartroLike.Run
{
    public enum RunNodeType
    {
        NormalBattle = 0,
        EliteBattle = 1,
        TribulationBattle = 2,
    }

    public enum RunEncounterType
    {
        None = 0,
        Shop = 1,
        Event = 2,
    }

    public enum RunPhase
    {
        Map = 0,
        Battle = 1,
        BattleResult = 2,
        RealmPromotion = 3,
        Shop = 4,
        Event = 5,
        RunResult = 6,
        MetaProgress = 7,
    }

    public enum RunResultType
    {
        None = 0,
        Victory = 1,
        Defeat = 2,
    }
}
