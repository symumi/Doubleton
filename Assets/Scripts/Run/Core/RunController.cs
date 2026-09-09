using System;
using System.Collections.Generic;
using BalartroLike.Battle;

namespace BalartroLike.Run
{
    public sealed class RunController
    {
        private const int ShopRefreshCost = 20;
        private const int ShopRefreshLimit = 2;
        private const int ShopOfferCount = 4;
        private const int ArtifactLimit = 4;
        private const int TalismanLimit = 5;
        private const string ContentPoolShopId = "shop_pool";
        private const int VictoryDaoHeart = 120;
        private const int DefeatDaoHeart = 30;
        private readonly IReadOnlyList<RunNodeDefinition> _nodes;

        public RunState State { get; }
        public RunMetaProgressState MetaProgress { get; }
        public HeavenTribulationDefinition CurrentTribulation
        {
            get { return RunConfigDatabase.GetTribulation(State.HeavenTribulationLevel); }
        }

        public int NodeCount
        {
            get { return _nodes.Count; }
        }

        public RunNodeDefinition CurrentNode
        {
            get { return State.CurrentNodeIndex < _nodes.Count ? _nodes[State.CurrentNodeIndex] : null; }
        }

        public RunController(IReadOnlyList<RunNodeDefinition> nodes, int seed, RunMetaProgressState metaProgress = null)
        {
            if (nodes == null || nodes.Count == 0)
            {
                throw new ArgumentException("Run nodes cannot be empty.", nameof(nodes));
            }

            _nodes = nodes;
            State = new RunState(seed);
            MetaProgress = metaProgress ?? new RunMetaProgressState();
            State.HeavenTribulationLevel = ResolveSelectedTribulation();
            BuildDeck();
            State.WeaponId = BattleConfigDatabase.Default.DefaultWeaponId;
            State.ArtifactIds.AddRange(BattleConfigDatabase.Default.DefaultArtifactIds);
            State.TalismanIds.AddRange(BattleConfigDatabase.Default.DefaultTalismanIds);
        }

        public static RunController CreatePrototype(int seed = -1, RunMetaProgressState metaProgress = null)
        {
            if (!RunConfigDatabase.IsLoaded || !RunEncounterDatabase.IsLoaded)
            {
                throw new InvalidOperationException("Run config database is not loaded.");
            }

            return new RunController(RunConfigDatabase.Nodes, seed >= 0 ? seed : Environment.TickCount, metaProgress);
        }

        public RunCommandResult SelectWeapon(string weaponId)
        {
            if (State.Phase != RunPhase.Map || State.CurrentNodeIndex != 0 || State.CompletedNodeIds.Count > 0)
            {
                return RunCommandResult.Fail("本局已开始，不能更换武器");
            }

            if (!BattleConfigDatabase.TryGetWeapon(weaponId, out _))
            {
                return RunCommandResult.Fail("武器不存在");
            }

            State.WeaponId = weaponId;
            return RunCommandResult.Ok();
        }

        public BattleController StartBattle()
        {
            if (State.Phase != RunPhase.Map || CurrentNode == null)
            {
                throw new InvalidOperationException("Current run node cannot start battle.");
            }

            List<DeckEntryDefinition> deckEntries = new List<DeckEntryDefinition>();
            for (int i = 0; i < State.Deck.Count; i++)
            {
                RunDeckCard card = State.Deck[i];
                deckEntries.Add(new DeckEntryDefinition(card.Trigram, card.Rank, card.Qi, 1));
            }

            BattleController battle = BattleController.CreatePrototype(
                State.Seed + State.CurrentNodeIndex,
                CurrentNode.SelectEnemyId(State.Seed + State.CurrentNodeIndex),
                deckEntries,
                State.MaxHpBonus,
                State.WeaponPowerBonus,
                State.WeaponId,
                State.ArtifactIds,
                State.TalismanIds,
                MetaProgress.HexagramUses,
                CurrentTribulation.EnemyHpPercent,
                CurrentTribulation.EnemyPowerBonus);
            State.Phase = RunPhase.Battle;
            return battle;
        }

        public RunCommandResult UseTalisman(BattleController battle, int index)
        {
            if (State.Phase != RunPhase.Battle)
            {
                return RunCommandResult.Fail("当前不在战斗中");
            }

            if (index < 0 || index >= State.TalismanIds.Count)
            {
                return RunCommandResult.Fail("符箓不存在");
            }

            BattleCommandResult result = battle.UseTalisman(index);
            if (!result.Success)
            {
                return RunCommandResult.Fail(result.Error);
            }

            State.TalismanIds.RemoveAt(index);
            return RunCommandResult.Ok();
        }

        public RunCommandResult CompleteBattle(BattleResultType result)
        {
            if (State.Phase != RunPhase.Battle)
            {
                return RunCommandResult.Fail("当前不在战斗中");
            }

            if (result != BattleResultType.Victory && result != BattleResultType.Defeat)
            {
                return RunCommandResult.Fail("未知战斗结果");
            }

            State.PendingBattleResult = result;
            State.PendingRewardSpiritStones = result == BattleResultType.Victory ? CurrentNode.RewardSpiritStones : 0;
            State.Phase = RunPhase.BattleResult;
            return RunCommandResult.Ok();
        }

        public RunCommandResult ConfirmBattleResult()
        {
            if (State.Phase != RunPhase.BattleResult)
            {
                return RunCommandResult.Fail("当前不在战斗结算阶段");
            }

            if (State.PendingBattleResult == BattleResultType.Defeat)
            {
                FinalizeRun(RunResultType.Defeat);
                return RunCommandResult.Ok();
            }

            if (State.PendingBattleResult != BattleResultType.Victory)
            {
                return RunCommandResult.Fail("战斗结算结果无效");
            }

            State.SpiritStones += State.PendingRewardSpiritStones;
            State.CompletedNodeIds.Add(CurrentNode.Id);
            if (CurrentNode.Type == RunNodeType.TribulationBattle)
            {
                PrepareRealmPromotion();
                State.Phase = RunPhase.RealmPromotion;
                return RunCommandResult.Ok();
            }

            AdvanceNode();
            return RunCommandResult.Ok();
        }

        public RunCommandResult CompleteRealmPromotion()
        {
            if (State.Phase != RunPhase.RealmPromotion)
            {
                return RunCommandResult.Fail("当前不在渡劫晋升阶段");
            }

            State.SpiritStones += State.PendingRealmRewardSpiritStones;
            State.PendingRealmRewardSpiritStones = 0;
            State.PromotionTargetRealmId = string.Empty;
            AdvanceNode();
            return RunCommandResult.Ok();
        }

        public RunCommandResult ConfirmRunResult()
        {
            if (State.Phase != RunPhase.RunResult)
            {
                return RunCommandResult.Fail("当前不在本局结算阶段");
            }

            MetaProgress.DaoHeart += State.DaoHeartReward;
            MetaProgress.CompletedRunCount++;
            if (State.Result == RunResultType.Victory)
            {
                UnlockNextHeavenTribulation();
            }

            State.Phase = RunPhase.MetaProgress;
            return RunCommandResult.Ok();
        }

        public RunCommandResult BuyShopOffer(string offerId)
        {
            if (State.Phase != RunPhase.Shop)
            {
                return RunCommandResult.Fail("当前不在坊市");
            }

            if (!State.ActiveShopOfferIds.Contains(offerId) || State.PurchasedShopOfferIds.Contains(offerId))
            {
                return RunCommandResult.Fail("商品不可购买");
            }

            if (!RunEncounterDatabase.TryGetShopOffer(offerId, out RunShopOfferDefinition offer))
            {
                return RunCommandResult.Fail("商品不存在");
            }

            int price = GetShopPrice(offer);
            if (State.SpiritStones < price)
            {
                return RunCommandResult.Fail("灵石不足");
            }

            if (offer.EffectType == RunEffectType.DeleteCard && State.Deck.Count == 0)
            {
                return RunCommandResult.Fail("牌组为空");
            }

            if (offer.EffectType == RunEffectType.Artifact)
            {
                if (State.ArtifactIds.Count >= ArtifactLimit)
                {
                    return RunCommandResult.Fail("法宝栏已满");
                }

                if (State.ArtifactIds.Contains(offer.ContentId))
                {
                    return RunCommandResult.Fail("已拥有该法宝");
                }

                if (!BattleConfigDatabase.TryGetArtifact(offer.ContentId, out _))
                {
                    return RunCommandResult.Fail("法宝配置不存在");
                }
            }

            if (offer.EffectType == RunEffectType.Talisman)
            {
                if (State.TalismanIds.Count >= TalismanLimit)
                {
                    return RunCommandResult.Fail("符箓栏已满");
                }

                if (!BattleConfigDatabase.TryGetTalisman(offer.ContentId, out _))
                {
                    return RunCommandResult.Fail("符箓配置不存在");
                }
            }

            State.SpiritStones -= price;
            ApplyEffect(offer.EffectType, offer.EffectValue, offer.ContentId);
            State.PurchasedShopOfferIds.Add(offerId);
            return RunCommandResult.Ok();
        }

        public int GetShopPrice(RunShopOfferDefinition offer)
        {
            return (offer.Price * CurrentTribulation.ShopPricePercent + 99) / 100;
        }

        public RunCommandResult RefreshShop()
        {
            if (State.Phase != RunPhase.Shop)
            {
                return RunCommandResult.Fail("当前不在坊市");
            }

            if (State.ShopRefreshCount >= ShopRefreshLimit)
            {
                return RunCommandResult.Fail("刷新次数已用尽");
            }

            if (State.SpiritStones < ShopRefreshCost)
            {
                return RunCommandResult.Fail("灵石不足");
            }

            State.SpiritStones -= ShopRefreshCost;
            State.ShopRefreshCount++;
            RebuildShopOffers();
            return RunCommandResult.Ok();
        }

        public RunCommandResult LeaveShop()
        {
            if (State.Phase != RunPhase.Shop)
            {
                return RunCommandResult.Fail("当前不在坊市");
            }

            State.ActiveShopId = string.Empty;
            State.ActiveShopOfferIds.Clear();
            State.PurchasedShopOfferIds.Clear();
            State.ShopRefreshCount = 0;
            State.Phase = RunPhase.Map;
            return RunCommandResult.Ok();
        }

        public RunCommandResult ResolveEvent(string optionId)
        {
            if (State.Phase != RunPhase.Event || State.EventResolved)
            {
                return RunCommandResult.Fail("当前不能处理奇遇");
            }

            if (!RunEncounterDatabase.TryGetEventOption(State.ActiveEventId, optionId, out RunEventOptionDefinition option))
            {
                return RunCommandResult.Fail("奇遇选项不存在");
            }

            if (option.EffectType == RunEffectType.DeleteCard && State.Deck.Count == 0)
            {
                return RunCommandResult.Fail("牌组为空");
            }

            ApplyEffect(option.EffectType, option.EffectValue);
            State.EventResolved = true;
            State.EventResultText = option.ResultText;
            return RunCommandResult.Ok();
        }

        public RunCommandResult LeaveEvent()
        {
            if (State.Phase != RunPhase.Event || !State.EventResolved)
            {
                return RunCommandResult.Fail("奇遇尚未结算");
            }

            State.ActiveEventId = string.Empty;
            State.EventResolved = false;
            State.EventResultText = string.Empty;
            State.Phase = RunPhase.Map;
            return RunCommandResult.Ok();
        }

        private void AdvanceNode()
        {
            RunNodeDefinition completedNode = CurrentNode;
            State.CurrentNodeIndex++;
            if (State.CurrentNodeIndex >= _nodes.Count)
            {
                FinalizeRun(RunResultType.Victory);
                return;
            }

            if (completedNode.EncounterType == RunEncounterType.Shop)
            {
                EnterShop(completedNode.EncounterId);
                return;
            }

            if (completedNode.EncounterType == RunEncounterType.Event)
            {
                EnterEvent(completedNode.EncounterId);
                return;
            }

            State.Phase = RunPhase.Map;
        }

        private void PrepareRealmPromotion()
        {
            if (!RunConfigDatabase.TryGetRealm(CurrentNode.RealmId, out RunRealmDefinition realm))
            {
                throw new InvalidOperationException("Current run realm config is missing.");
            }

            State.PromotionTargetRealmId = realm.PromotionTargetRealmId;
            State.PendingRealmRewardSpiritStones = realm.PromotionRewardSpiritStones;
            // TODO: 接入符箓/法宝奖励池后，再补晋升随机奖励。
        }

        private int ResolveSelectedTribulation()
        {
            int maxLevel = RunConfigDatabase.MaxTribulationLevel;
            MetaProgress.HighestHeavenTribulation = Math.Max(0, Math.Min(MetaProgress.HighestHeavenTribulation, maxLevel));
            MetaProgress.SelectedHeavenTribulation = Math.Max(0, Math.Min(MetaProgress.SelectedHeavenTribulation, MetaProgress.HighestHeavenTribulation));
            return MetaProgress.SelectedHeavenTribulation;
        }

        private void UnlockNextHeavenTribulation()
        {
            int nextLevel = Math.Min(RunConfigDatabase.MaxTribulationLevel, State.HeavenTribulationLevel + 1);
            MetaProgress.HighestHeavenTribulation = Math.Max(MetaProgress.HighestHeavenTribulation, nextLevel);
        }

        private void FinalizeRun(RunResultType result)
        {
            State.Result = result;
            State.DaoHeartReward = result == RunResultType.Victory ? VictoryDaoHeart : DefeatDaoHeart;
            State.Phase = RunPhase.RunResult;
        }

        private void BuildDeck()
        {
            if (!BattleConfigDatabase.IsLoaded)
            {
                throw new InvalidOperationException("Battle config database is not loaded.");
            }

            IReadOnlyList<DeckEntryDefinition> entries = BattleConfigDatabase.Deck;
            for (int i = 0; i < entries.Count; i++)
            {
                DeckEntryDefinition entry = entries[i];
                for (int copy = 0; copy < entry.Count; copy++)
                {
                    State.Deck.Add(new RunDeckCard(entry.Trigram, entry.Rank, entry.Qi));
                }
            }
        }

        private void EnterShop(string shopId)
        {
            State.ActiveShopId = shopId;
            State.ShopRefreshCount = 0;
            State.ActiveShopOfferIds.Clear();
            State.PurchasedShopOfferIds.Clear();
            RebuildShopOffers();
            State.Phase = RunPhase.Shop;
        }

        private void EnterEvent(string eventId)
        {
            State.ActiveEventId = eventId;
            State.EventResolved = false;
            State.EventResultText = string.Empty;
            State.Phase = RunPhase.Event;
        }

        private void RebuildShopOffers()
        {
            List<string> purchasedOffers = new List<string>();
            for (int i = 0; i < State.ActiveShopOfferIds.Count; i++)
            {
                string offerId = State.ActiveShopOfferIds[i];
                if (State.PurchasedShopOfferIds.Contains(offerId))
                {
                    purchasedOffers.Add(offerId);
                }
            }

            State.ActiveShopOfferIds.Clear();
            State.ActiveShopOfferIds.AddRange(purchasedOffers);

            AddRandomOffers(RunEncounterDatabase.GetShopOffers(State.ActiveShopId), 1, null);
            IReadOnlyList<RunShopOfferDefinition> contentPool = RunEncounterDatabase.GetShopOffers(ContentPoolShopId);
            AddRandomOffers(contentPool, 2, RunEffectType.Artifact);
            AddRandomOffers(contentPool, 1, RunEffectType.Talisman);
        }

        private void AddRandomOffers(
            IReadOnlyList<RunShopOfferDefinition> candidates,
            int targetCount,
            RunEffectType? effectType)
        {
            int currentCount = 0;
            for (int i = 0; i < State.ActiveShopOfferIds.Count; i++)
            {
                if (!RunEncounterDatabase.TryGetShopOffer(State.ActiveShopOfferIds[i], out RunShopOfferDefinition activeOffer))
                {
                    continue;
                }

                if (!effectType.HasValue || activeOffer.EffectType == effectType.Value)
                {
                    currentCount++;
                }
            }

            if (currentCount >= targetCount)
            {
                return;
            }

            List<RunShopOfferDefinition> eligible = new List<RunShopOfferDefinition>();
            for (int i = 0; i < candidates.Count; i++)
            {
                RunShopOfferDefinition candidate = candidates[i];
                if (State.ActiveShopOfferIds.Contains(candidate.Id) || State.PurchasedShopOfferIds.Contains(candidate.Id))
                {
                    continue;
                }

                if (effectType.HasValue && candidate.EffectType != effectType.Value)
                {
                    continue;
                }

                if (candidate.EffectType == RunEffectType.Artifact && State.ArtifactIds.Contains(candidate.ContentId))
                {
                    continue;
                }

                eligible.Add(candidate);
            }

            Random random = new Random(State.Seed + State.CurrentNodeIndex * 397 + State.ShopRefreshCount * 7919 + (effectType.HasValue ? (int)effectType.Value : 0));
            while (currentCount < targetCount && eligible.Count > 0)
            {
                int index = random.Next(eligible.Count);
                State.ActiveShopOfferIds.Add(eligible[index].Id);
                eligible.RemoveAt(index);
                currentCount++;
            }
        }

        private void ApplyEffect(RunEffectType effectType, int value, string contentId = null)
        {
            switch (effectType)
            {
                case RunEffectType.MaxHp:
                    State.MaxHpBonus += value;
                    break;
                case RunEffectType.WeaponPower:
                    State.WeaponPowerBonus += value;
                    break;
                case RunEffectType.DeleteCard:
                    State.Deck.RemoveAt(0);
                    break;
                case RunEffectType.SpiritStones:
                    State.SpiritStones += value;
                    break;
                case RunEffectType.Artifact:
                    State.ArtifactIds.Add(contentId);
                    break;
                case RunEffectType.Talisman:
                    State.TalismanIds.Add(contentId);
                    break;
            }
        }
    }
}
