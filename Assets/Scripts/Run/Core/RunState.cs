using System.Collections.Generic;
using BalartroLike.Battle;

namespace BalartroLike.Run
{
    public sealed class RunState
    {
        public int Seed { get; }
        public int CurrentNodeIndex { get; set; }
        public int SpiritStones { get; set; }
        public RunPhase Phase { get; set; }
        public RunResultType Result { get; set; }
        public BattleResultType PendingBattleResult { get; set; }
        public int PendingRewardSpiritStones { get; set; }
        public int PendingRealmRewardSpiritStones { get; set; }
        public string PromotionTargetRealmId { get; set; }
        public int DaoHeartReward { get; set; }
        public int MaxHpBonus { get; set; }
        public int WeaponPowerBonus { get; set; }
        public string WeaponId { get; set; }
        public string ActiveShopId { get; set; }
        public int ShopRefreshCount { get; set; }
        public string ActiveEventId { get; set; }
        public bool EventResolved { get; set; }
        public string EventResultText { get; set; }
        public List<string> CompletedNodeIds { get; }
        public List<RunDeckCard> Deck { get; }
        public List<string> ArtifactIds { get; }
        public List<string> TalismanIds { get; }
        public List<string> ActiveShopOfferIds { get; }
        public List<string> PurchasedShopOfferIds { get; }

        public RunState(int seed)
        {
            Seed = seed;
            CompletedNodeIds = new List<string>();
            Deck = new List<RunDeckCard>();
            ArtifactIds = new List<string>();
            TalismanIds = new List<string>();
            ActiveShopOfferIds = new List<string>();
            PurchasedShopOfferIds = new List<string>();
            Phase = RunPhase.Map;
            Result = RunResultType.None;
            PendingBattleResult = BattleResultType.None;
            WeaponId = string.Empty;
            PromotionTargetRealmId = string.Empty;
            EventResultText = string.Empty;
        }
    }
}
