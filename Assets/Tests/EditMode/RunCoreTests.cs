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

        private static RunController CreateRunAtFirstShop()
        {
            RunController run = RunController.CreatePrototype(1234);
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
