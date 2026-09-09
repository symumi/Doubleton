using BalartroLike.Battle;
using BalartroLike.Run;
using NUnit.Framework;

namespace BalartroLike.Tests
{
    public sealed class RunCoreTests
    {
        [OneTimeSetUp]
        public void LoadRunConfig()
        {
            ResourcesBattleConfigLoader.LoadIfNeeded();
            ResourcesRunConfigLoader.LoadIfNeeded();
        }

        [Test]
        public void RunConfig_ContainsNineNodes()
        {
            Assert.AreEqual(9, RunConfigDatabase.Nodes.Count);
        }

        [Test]
        public void RunConfig_NodesContainEnemyPools()
        {
            for (int i = 0; i < RunConfigDatabase.Nodes.Count; i++)
            {
                RunNodeDefinition node = RunConfigDatabase.Nodes[i];
                Assert.Greater(node.EnemyIds.Count, 0, node.Id);
                for (int j = 0; j < node.EnemyIds.Count; j++)
                {
                    Assert.IsTrue(BattleConfigDatabase.TryGetEnemy(node.EnemyIds[j], out _), node.EnemyIds[j]);
                }
            }
        }

        [Test]
        public void RunConfig_ContainsTribulations()
        {
            Assert.AreEqual(4, RunConfigDatabase.Tribulations.Count);
            Assert.AreEqual(3, RunConfigDatabase.MaxTribulationLevel);
            Assert.IsTrue(RunConfigDatabase.TryGetTribulation(0, out HeavenTribulationDefinition baseTribulation));
            Assert.AreEqual(100, baseTribulation.EnemyHpPercent);
            Assert.AreEqual(120, RunConfigDatabase.GetTribulation(3).ShopPricePercent);
        }

        [Test]
        public void RunController_SelectedTribulationAppliesEnemyModifiers()
        {
            RunMetaProgressState meta = new RunMetaProgressState
            {
                HighestHeavenTribulation = 3,
                SelectedHeavenTribulation = 3
            };
            RunController run = RunController.CreatePrototype(1234, meta);
            BattleController battle = run.StartBattle();
            EnemyDefinition enemyDefinition = BattleConfigDatabase.GetEnemy(battle.State.Enemy.Id);
            int expectedHp = (enemyDefinition.MaxHp * 140 + 99) / 100;

            Assert.AreEqual(3, run.State.HeavenTribulationLevel);
            Assert.AreEqual(expectedHp, battle.State.Enemy.MaxHp);
            Assert.AreEqual(2, battle.State.Enemy.PowerBonus);

            EnemyIntentDefinition attack = new EnemyIntentDefinition(
                "attack", EnemyIntentType.Attack, 6, StatusId.None, 0, 0, 1, 0, EnemyIntentConditionType.Always, 0, "攻击");
            EnemyState enemy = new EnemyState(
                "test", "测试敌人", ElementType.Wood, 60, 6, EnemyKind.Normal, EnemyIntentMode.Sequence,
                new[] { attack }, EnemyRuleType.None, 2);
            EnemyIntent intent = new EnemyIntentSelector().SelectNext(enemy, 1, new System.Random(1234));

            Assert.AreEqual(8, intent.Power);
        }

        [Test]
        public void RunController_TribulationRaisesShopPrices()
        {
            RunMetaProgressState meta = new RunMetaProgressState
            {
                HighestHeavenTribulation = 2,
                SelectedHeavenTribulation = 2
            };
            RunController run = CreateRunAtFirstShop(meta);
            run.State.ActiveShopOfferIds.Clear();
            run.State.ActiveShopOfferIds.Add("shop01_heal");
            Assert.IsTrue(RunEncounterDatabase.TryGetShopOffer("shop01_heal", out RunShopOfferDefinition offer));
            int price = run.GetShopPrice(offer);
            int stones = run.State.SpiritStones;

            Assert.AreEqual(30, price);
            Assert.IsTrue(run.BuyShopOffer("shop01_heal").Success);
            Assert.AreEqual(stones - price, run.State.SpiritStones);
        }

        [Test]
        public void RunController_VictoryUnlocksNextTribulation()
        {
            RunMetaProgressState meta = new RunMetaProgressState
            {
                HighestHeavenTribulation = 1,
                SelectedHeavenTribulation = 1
            };
            RunController run = RunController.CreatePrototype(1234, meta);
            run.State.Phase = RunPhase.RunResult;
            run.State.Result = RunResultType.Victory;

            Assert.IsTrue(run.ConfirmRunResult().Success);
            Assert.AreEqual(2, meta.HighestHeavenTribulation);
        }

        [Test]
        public void RunController_ClampsSelectedTribulationToUnlocked()
        {
            RunMetaProgressState meta = new RunMetaProgressState
            {
                HighestHeavenTribulation = 1,
                SelectedHeavenTribulation = 3
            };
            RunController run = RunController.CreatePrototype(1234, meta);

            Assert.AreEqual(1, run.State.HeavenTribulationLevel);
            Assert.AreEqual(1, meta.SelectedHeavenTribulation);
        }

        [Test]
        public void RunController_StartsAtFirstNode()
        {
            RunController run = RunController.CreatePrototype(1234);

            Assert.AreEqual("node_01", run.CurrentNode.Id);
            Assert.AreEqual(RunPhase.Map, run.State.Phase);
        }

        [Test]
        public void RunController_WeaponCanOnlyChangeBeforeFirstBattle()
        {
            RunController run = RunController.CreatePrototype(1234);

            Assert.AreEqual("weapon_iron_sword", run.State.WeaponId);
            Assert.IsTrue(run.SelectWeapon("weapon_qingfeng").Success);

            BattleController battle = run.StartBattle();

            Assert.AreEqual("weapon_qingfeng", battle.State.Player.Weapon.Id);
            Assert.IsFalse(run.SelectWeapon("weapon_rainstorm").Success);
        }

        [Test]
        public void RunController_VictoryAdvancesAndRewards()
        {
            RunController run = RunController.CreatePrototype(1234);
            run.StartBattle();

            RunCommandResult result = run.CompleteBattle(BattleResultType.Victory);

            Assert.IsTrue(result.Success, result.Error);
            Assert.AreEqual(RunPhase.BattleResult, run.State.Phase);
            Assert.AreEqual(0, run.State.CurrentNodeIndex);
            Assert.AreEqual(0, run.State.SpiritStones);
            Assert.AreEqual(30, run.State.PendingRewardSpiritStones);

            result = run.ConfirmBattleResult();

            Assert.IsTrue(result.Success, result.Error);
            Assert.AreEqual(1, run.State.CurrentNodeIndex);
            Assert.AreEqual(30, run.State.SpiritStones);
            Assert.AreEqual(RunPhase.Map, run.State.Phase);
        }

        [Test]
        public void RunController_TribulationRequiresPromotion()
        {
            RunController run = RunController.CreatePrototype(1234);
            for (int i = 0; i < 3; i++)
            {
                run.StartBattle();
                run.CompleteBattle(BattleResultType.Victory);
                run.ConfirmBattleResult();
            }

            Assert.AreEqual(2, run.State.CurrentNodeIndex);
            Assert.AreEqual(RunPhase.RealmPromotion, run.State.Phase);
            Assert.AreEqual("REALM_FOUNDATION", run.State.PromotionTargetRealmId);
            Assert.AreEqual(30, run.State.PendingRealmRewardSpiritStones);

            RunCommandResult result = run.CompleteRealmPromotion();

            Assert.IsTrue(result.Success, result.Error);
            Assert.AreEqual(170, run.State.SpiritStones);
            Assert.AreEqual(RunPhase.Shop, run.State.Phase);
            Assert.AreEqual("shop_01", run.State.ActiveShopId);

            result = run.CompleteRealmPromotion();

            Assert.IsFalse(result.Success);
            Assert.AreEqual(170, run.State.SpiritStones);

            result = run.LeaveShop();

            Assert.IsTrue(result.Success, result.Error);
            Assert.AreEqual(3, run.State.CurrentNodeIndex);
            Assert.AreEqual(RunPhase.Map, run.State.Phase);
        }

        [Test]
        public void RunController_DefeatEndsRun()
        {
            RunController run = RunController.CreatePrototype(1234);
            run.StartBattle();

            RunCommandResult result = run.CompleteBattle(BattleResultType.Defeat);

            Assert.IsTrue(result.Success, result.Error);
            Assert.AreEqual(RunPhase.BattleResult, run.State.Phase);

            result = run.ConfirmBattleResult();

            Assert.IsTrue(result.Success, result.Error);
            Assert.AreEqual(RunResultType.Defeat, run.State.Result);
            Assert.AreEqual(RunPhase.RunResult, run.State.Phase);
        }

        [Test]
        public void RunController_CompletesAfterFinalTribulation()
        {
            RunController run = RunController.CreatePrototype(1234);
            for (int i = 0; i < RunConfigDatabase.Nodes.Count; i++)
            {
                run.StartBattle();
                run.CompleteBattle(BattleResultType.Victory);
                run.ConfirmBattleResult();
                if (run.State.Phase == RunPhase.RealmPromotion)
                {
                    run.CompleteRealmPromotion();
                }

                LeaveEncounter(run);
            }

            Assert.AreEqual(RunResultType.Victory, run.State.Result);
            Assert.AreEqual(RunPhase.RunResult, run.State.Phase);
            Assert.AreEqual(RunConfigDatabase.Nodes.Count, run.State.CurrentNodeIndex);
            Assert.AreEqual(120, run.State.DaoHeartReward);

            RunCommandResult result = run.ConfirmRunResult();

            Assert.IsTrue(result.Success, result.Error);
            Assert.AreEqual(RunPhase.MetaProgress, run.State.Phase);
            Assert.AreEqual(120, run.MetaProgress.DaoHeart);
            Assert.AreEqual(1, run.MetaProgress.CompletedRunCount);
            Assert.AreEqual(1, run.MetaProgress.HighestHeavenTribulation);
        }

        [Test]
        public void RunController_DefeatSettlesDaoHeart()
        {
            RunController run = RunController.CreatePrototype(1234);
            run.StartBattle();
            run.CompleteBattle(BattleResultType.Defeat);
            run.ConfirmBattleResult();

            Assert.AreEqual(RunResultType.Defeat, run.State.Result);
            Assert.AreEqual(RunPhase.RunResult, run.State.Phase);
            Assert.AreEqual(30, run.State.DaoHeartReward);

            RunCommandResult result = run.ConfirmRunResult();

            Assert.IsTrue(result.Success, result.Error);
            Assert.AreEqual(RunPhase.MetaProgress, run.State.Phase);
            Assert.AreEqual(30, run.MetaProgress.DaoHeart);
            Assert.AreEqual(1, run.MetaProgress.CompletedRunCount);
            Assert.AreEqual(0, run.MetaProgress.HighestHeavenTribulation);

            result = run.ConfirmRunResult();

            Assert.IsFalse(result.Success);
            Assert.AreEqual(30, run.MetaProgress.DaoHeart);
        }

        [Test]
        public void RunController_ConfirmsBattleRewardOnlyOnce()
        {
            RunController run = RunController.CreatePrototype(1234);
            run.StartBattle();
            run.CompleteBattle(BattleResultType.Victory);
            run.ConfirmBattleResult();

            RunCommandResult result = run.ConfirmBattleResult();

            Assert.IsFalse(result.Success);
            Assert.AreEqual(30, run.State.SpiritStones);
            Assert.AreEqual(1, run.State.CompletedNodeIds.Count);
        }

        [Test]
        public void RunController_ShopPurchasePersistsToNextBattle()
        {
            RunController run = CreateRunAtFirstShop();
            int deckCount = run.State.Deck.Count;
            run.State.ActiveShopOfferIds.Clear();
            run.State.ActiveShopOfferIds.Add("shop01_heal");
            run.State.ActiveShopOfferIds.Add("shop01_delete");

            RunCommandResult result = run.BuyShopOffer("shop01_heal");

            Assert.IsTrue(result.Success, result.Error);
            Assert.AreEqual(145, run.State.SpiritStones);
            Assert.AreEqual(6, run.State.MaxHpBonus);

            result = run.BuyShopOffer("shop01_delete");

            Assert.IsTrue(result.Success, result.Error);
            Assert.AreEqual(deckCount - 1, run.State.Deck.Count);

            result = run.RefreshShop();

            Assert.IsTrue(result.Success, result.Error);
            Assert.AreEqual(1, run.State.ShopRefreshCount);
            Assert.AreEqual(100, run.State.SpiritStones);

            result = run.LeaveShop();
            Assert.IsTrue(result.Success, result.Error);

            BattleController battle = run.StartBattle();
            Assert.AreEqual(BattleConfigDatabase.Default.PlayerMaxHp + 6, battle.State.Player.MaxHp);
            Assert.AreEqual(deckCount - 1, battle.State.Hand.Count + battle.State.DrawPile.Count + battle.State.DiscardPile.Count);
        }

        [Test]
        public void RunController_ShopContentPoolAddsArtifactAndTalisman()
        {
            RunController run = CreateRunAtFirstShop();
            string artifactOfferId = null;
            string artifactId = null;
            string talismanOfferId = null;
            string talismanId = null;
            for (int i = 0; i < run.State.ActiveShopOfferIds.Count; i++)
            {
                Assert.IsTrue(RunEncounterDatabase.TryGetShopOffer(run.State.ActiveShopOfferIds[i], out RunShopOfferDefinition offer));
                if (artifactOfferId == null && offer.EffectType == RunEffectType.Artifact)
                {
                    artifactOfferId = offer.Id;
                    artifactId = offer.ContentId;
                }
                else if (talismanOfferId == null && offer.EffectType == RunEffectType.Talisman)
                {
                    talismanOfferId = offer.Id;
                    talismanId = offer.ContentId;
                }
            }

            Assert.IsNotNull(artifactOfferId);
            Assert.IsNotNull(talismanOfferId);
            Assert.IsTrue(run.BuyShopOffer(artifactOfferId).Success);
            Assert.IsTrue(run.State.ArtifactIds.Contains(artifactId));
            Assert.IsTrue(run.BuyShopOffer(talismanOfferId).Success);
            Assert.IsTrue(run.State.TalismanIds.Contains(talismanId));
            Assert.IsTrue(run.LeaveShop().Success);

            BattleController battle = run.StartBattle();

            Assert.IsTrue(battle.State.Artifacts.Exists(artifact => artifact.Id == artifactId));
            Assert.IsTrue(battle.State.Talismans.Exists(talisman => talisman.Id == talismanId));
        }

        [Test]
        public void RunController_EventChoicePersistsReward()
        {
            RunController run = CreateRunAtEvent();
            int stones = run.State.SpiritStones;

            RunCommandResult result = run.ResolveEvent("option_a");

            Assert.IsTrue(result.Success, result.Error);
            Assert.IsTrue(run.State.EventResolved);
            Assert.AreEqual(stones + 30, run.State.SpiritStones);

            result = run.LeaveEvent();

            Assert.IsTrue(result.Success, result.Error);
            Assert.AreEqual(RunPhase.Map, run.State.Phase);
        }

        [Test]
        public void RunController_TalismanConsumesFromRunLoadout()
        {
            RunController run = RunController.CreatePrototype(1234);
            BattleController battle = run.StartBattle();
            int index = run.State.TalismanIds.IndexOf("talisman_shield");
            int shieldBefore = battle.State.Player.Shield;

            RunCommandResult result = run.UseTalisman(battle, index);

            Assert.IsTrue(result.Success, result.Error);
            Assert.AreEqual(shieldBefore + 6, battle.State.Player.Shield);
            Assert.IsFalse(run.State.TalismanIds.Contains("talisman_shield"));
            Assert.IsFalse(battle.State.Talismans.Exists(talisman => talisman.Id == "talisman_shield"));
        }

        private static RunController CreateRunAtFirstShop(RunMetaProgressState meta = null)
        {
            RunController run = RunController.CreatePrototype(1234, meta);
            for (int i = 0; i < 3; i++)
            {
                run.StartBattle();
                run.CompleteBattle(BattleResultType.Victory);
                run.ConfirmBattleResult();
            }

            Assert.AreEqual(RunPhase.RealmPromotion, run.State.Phase);
            Assert.IsTrue(run.CompleteRealmPromotion().Success);
            Assert.AreEqual(RunPhase.Shop, run.State.Phase);
            return run;
        }

        private static RunController CreateRunAtEvent()
        {
            RunController run = RunController.CreatePrototype(1234);
            for (int i = 0; i < 5; i++)
            {
                run.StartBattle();
                run.CompleteBattle(BattleResultType.Victory);
                run.ConfirmBattleResult();
                if (run.State.Phase == RunPhase.RealmPromotion)
                {
                    run.CompleteRealmPromotion();
                }

                if (run.State.Phase == RunPhase.Event)
                {
                    break;
                }

                LeaveEncounter(run);
            }

            Assert.AreEqual(RunPhase.Event, run.State.Phase);
            return run;
        }

        private static void LeaveEncounter(RunController run)
        {
            if (run.State.Phase == RunPhase.Shop)
            {
                Assert.IsTrue(run.LeaveShop().Success);
            }
            else if (run.State.Phase == RunPhase.Event)
            {
                if (!run.State.EventResolved)
                {
                    Assert.IsTrue(run.ResolveEvent("option_c").Success);
                }

                Assert.IsTrue(run.LeaveEvent().Success);
            }
        }
    }
}
