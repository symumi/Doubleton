using System;
using System.Collections.Generic;
using System.IO;
using BalartroLike.Battle;
using BalartroLike.Run;
using BalartroLike.Save;
using NUnit.Framework;

namespace BalartroLike.Tests
{
    public sealed class SaveCoreTests
    {
        [Test]
        public void SaveService_RunAndBattleRoundTrip()
        {
            RunController run = RunController.CreatePrototype(1234);
            run.SelectWeapon("weapon_qingfeng");
            BattleController battle = run.StartBattle();
            string[] handUids = GetCardUids(battle.State.Hand);
            string[] drawUids = GetCardUids(battle.State.DrawPile);
            string intentId = battle.State.Enemy.CurrentIntent.Id;

            GameSaveData data = SaveService.Capture(run, battle);
            bool restored = SaveService.TryRestore(data, out RunController restoredRun, out BattleController restoredBattle, out _, out string error);

            Assert.IsTrue(restored, error);
            Assert.AreEqual(run.State.Seed, restoredRun.State.Seed);
            Assert.AreEqual("weapon_qingfeng", restoredRun.State.WeaponId);
            Assert.AreEqual(battle.State.Turn, restoredBattle.State.Turn);
            Assert.AreEqual(battle.State.Player.Energy, restoredBattle.State.Player.Energy);
            Assert.AreEqual(intentId, restoredBattle.State.Enemy.CurrentIntent.Id);
            CollectionAssert.AreEqual(handUids, GetCardUids(restoredBattle.State.Hand));
            CollectionAssert.AreEqual(drawUids, GetCardUids(restoredBattle.State.DrawPile));
        }

        [Test]
        public void SaveService_RestoreBattleDoesNotRepeatTurnStart()
        {
            RunController run = RunController.CreatePrototype(1234);
            BattleController battle = run.StartBattle();
            int turn = battle.State.Turn;
            int energy = battle.State.Player.Energy;
            int handCount = battle.State.Hand.Count;

            GameSaveData data = SaveService.Capture(run, battle);
            Assert.IsTrue(SaveService.TryRestore(data, out _, out BattleController restoredBattle, out _, out string error), error);

            Assert.AreEqual(turn, restoredBattle.State.Turn);
            Assert.AreEqual(energy, restoredBattle.State.Player.Energy);
            Assert.AreEqual(handCount, restoredBattle.State.Hand.Count);
            Assert.AreEqual(BattlePhase.PlayerAction, restoredBattle.State.Phase);
        }

        [Test]
        public void SaveService_ShopStateRoundTrip()
        {
            RunController run = CreateRunAtFirstShop();
            run.State.ActiveShopOfferIds.Clear();
            run.State.ActiveShopOfferIds.Add("shop01_heal");
            Assert.IsTrue(run.BuyShopOffer("shop01_heal").Success);
            Assert.IsTrue(run.RefreshShop().Success);
            string[] offers = run.State.ActiveShopOfferIds.ToArray();

            GameSaveData data = SaveService.Capture(run, null);
            Assert.IsTrue(SaveService.TryRestore(data, out RunController restoredRun, out _, out _, out string error), error);

            Assert.AreEqual(run.State.SpiritStones, restoredRun.State.SpiritStones);
            Assert.AreEqual(run.State.ShopRefreshCount, restoredRun.State.ShopRefreshCount);
            CollectionAssert.AreEqual(offers, restoredRun.State.ActiveShopOfferIds);
            CollectionAssert.AreEqual(run.State.PurchasedShopOfferIds, restoredRun.State.PurchasedShopOfferIds);
            Assert.AreEqual(run.State.MaxHpBonus, restoredRun.State.MaxHpBonus);
        }

        [Test]
        public void SaveService_EventStateRoundTrip()
        {
            RunController run = CreateRunAtEvent();
            Assert.IsTrue(run.ResolveEvent("option_a").Success);
            int spiritStones = run.State.SpiritStones;
            string resultText = run.State.EventResultText;

            GameSaveData data = SaveService.Capture(run, null);
            Assert.IsTrue(SaveService.TryRestore(data, out RunController restoredRun, out _, out _, out string error), error);

            Assert.AreEqual("event_01", restoredRun.State.ActiveEventId);
            Assert.IsTrue(restoredRun.State.EventResolved);
            Assert.AreEqual(resultText, restoredRun.State.EventResultText);
            Assert.AreEqual(spiritStones, restoredRun.State.SpiritStones);
        }
        [Test]
        public void SaveService_MetaOnlyRoundTrip()
        {
            RunMetaProgressState meta = new RunMetaProgressState
            {
                DaoHeart = 320,
                HighestHeavenTribulation = 2,
                SelectedHeavenTribulation = 2,
                CompletedRunCount = 4
            };
            meta.HexagramUses.Add("qian_qian", 12);

            GameSaveData data = new GameSaveData
            {
                save_version = SaveService.CurrentVersion,
                meta = new MetaSaveData
                {
                    dao_heart = meta.DaoHeart,
                    highest_heaven_tribulation = meta.HighestHeavenTribulation,
                    selected_heaven_tribulation = meta.SelectedHeavenTribulation,
                    completed_run_count = meta.CompletedRunCount,
                    hexagram_uses = new List<HexagramUseSaveData>
                    {
                        new HexagramUseSaveData { hexagram_id = "qian_qian", use_count = 12 }
                    }
                }
            };

            Assert.IsTrue(SaveService.TryRestore(data, out RunController run, out BattleController battle, out RunMetaProgressState restoredMeta, out string error), error);
            Assert.IsNull(run);
            Assert.IsNull(battle);
            Assert.AreEqual(meta.DaoHeart, restoredMeta.DaoHeart);
            Assert.AreEqual(meta.HighestHeavenTribulation, restoredMeta.HighestHeavenTribulation);
            Assert.AreEqual(meta.SelectedHeavenTribulation, restoredMeta.SelectedHeavenTribulation);
            Assert.AreEqual(meta.CompletedRunCount, restoredMeta.CompletedRunCount);
            Assert.AreEqual(12, restoredMeta.HexagramUses["qian_qian"]);
        }

        [Test]
        public void SaveService_TribulationRoundTripPreservesRunAndBattle()
        {
            RunMetaProgressState meta = new RunMetaProgressState
            {
                HighestHeavenTribulation = 3,
                SelectedHeavenTribulation = 3
            };
            RunController run = RunController.CreatePrototype(1234, meta);
            BattleController battle = run.StartBattle();
            GameSaveData data = SaveService.Capture(run, battle);

            Assert.IsTrue(SaveService.TryRestore(data, out RunController restoredRun, out BattleController restoredBattle, out RunMetaProgressState restoredMeta, out string error), error);
            Assert.AreEqual(3, restoredMeta.SelectedHeavenTribulation);
            Assert.AreEqual(3, restoredRun.State.HeavenTribulationLevel);
            Assert.AreEqual(battle.State.Enemy.MaxHp, restoredBattle.State.Enemy.MaxHp);
            Assert.AreEqual(2, restoredBattle.State.Enemy.PowerBonus);
        }

        [Test]
        public void SaveService_FileRoundTripPreservesBattle()
        {
            string directory = Path.Combine(Path.GetTempPath(), "BalartroLikeSaveTests", Guid.NewGuid().ToString("N"));
            string path = Path.Combine(directory, "save.json");
            try
            {
                RunController run = RunController.CreatePrototype(1234);
                BattleController battle = run.StartBattle();
                string[] handUids = GetCardUids(battle.State.Hand);
                string[] drawUids = GetCardUids(battle.State.DrawPile);

                Assert.IsTrue(SaveService.TrySave(path, run, battle, out string saveError), saveError);
                Assert.IsTrue(SaveService.TryLoad(path, out GameSaveData data, out string loadError), loadError);
                Assert.IsTrue(SaveService.TryRestore(data, out _, out BattleController restoredBattle, out _, out string restoreError), restoreError);
                CollectionAssert.AreEqual(handUids, GetCardUids(restoredBattle.State.Hand));
                CollectionAssert.AreEqual(drawUids, GetCardUids(restoredBattle.State.DrawPile));
                Assert.AreEqual(battle.State.Enemy.CurrentIntent.Id, restoredBattle.State.Enemy.CurrentIntent.Id);
            }
            finally
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
        }
        [Test]
        public void SaveService_LoadsBackupWhenMainIsCorrupt()
        {
            string directory = Path.Combine(Path.GetTempPath(), "BalartroLikeSaveTests", Guid.NewGuid().ToString("N"));
            string path = Path.Combine(directory, "save.json");
            try
            {
                RunController firstRun = RunController.CreatePrototype(1111);
                firstRun.State.SpiritStones = 11;
                Assert.IsTrue(SaveService.TrySave(path, firstRun, null, out string firstError), firstError);

                RunController secondRun = RunController.CreatePrototype(2222);
                secondRun.State.SpiritStones = 22;
                Assert.IsTrue(SaveService.TrySave(path, secondRun, null, out string secondError), secondError);

                File.WriteAllText(path, "{ broken", System.Text.Encoding.UTF8);
                Assert.IsTrue(SaveService.TryLoad(path, out GameSaveData loaded, out string loadError), loadError);
                Assert.IsTrue(SaveService.TryRestore(loaded, out RunController restoredRun, out _, out _, out string restoreError), restoreError);
                Assert.AreEqual(1111, restoredRun.State.Seed);
                Assert.AreEqual(11, restoredRun.State.SpiritStones);
            }
            finally
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
        }

        private static string[] GetCardUids(System.Collections.Generic.List<CardInstance> cards)
        {
            string[] result = new string[cards.Count];
            for (int i = 0; i < cards.Count; i++)
            {
                result[i] = cards[i].Uid + ":" + cards[i].Trigram + ":" + cards[i].Rank + ":" + cards[i].Qi;
            }

            return result;
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

                if (run.State.Phase == RunPhase.Shop)
                {
                    run.LeaveShop();
                }
            }

            return run;
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
    }
}