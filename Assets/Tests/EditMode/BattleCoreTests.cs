using BalartroLike.Battle;
using NUnit.Framework;

namespace BalartroLike.Tests
{
    public sealed class BattleCoreTests
    {
        [OneTimeSetUp]
        public void LoadBattleConfig()
        {
            ResourcesBattleConfigLoader.LoadIfNeeded();
        }

        [Test]
        public void HexagramCatalog_Contains64Combinations()
        {
            Assert.AreEqual(64, HexagramCatalog.Count);
        }

        [Test]
        public void HexagramOrder_ChangesDefinition()
        {
            HexagramDefinition tai = HexagramCatalog.Get(TrigramId.Kun, TrigramId.Qian);
            HexagramDefinition pi = HexagramCatalog.Get(TrigramId.Qian, TrigramId.Kun);

            Assert.AreEqual("地天泰", tai.DisplayName);
            Assert.AreEqual("天地否", pi.DisplayName);
            Assert.AreNotEqual(tai.Id, pi.Id);
        }

        [Test]
        public void ElementRules_ReturnsExpectedMultipliers()
        {
            Assert.AreEqual(ElementRules.RestrainsMultiplier, ElementRules.GetDamageMultiplier(ElementType.Metal, ElementType.Wood));
            Assert.AreEqual(ElementRules.RestrainedMultiplier, ElementRules.GetDamageMultiplier(ElementType.Wood, ElementType.Metal));
            Assert.AreEqual(ElementRules.SameElementEnemyMultiplier, ElementRules.GetDamageMultiplier(ElementType.Fire, ElementType.Fire));
        }

        [Test]
        public void Calculator_PreviewDoesNotMutateState()
        {
            BattleController controller = BattleController.CreatePrototype(1234);
            BattleState state = controller.State;
            CardInstance inner = state.Hand[0];
            CardInstance outer = state.Hand[1];
            int handCount = state.Hand.Count;
            int energy = state.Player.Energy;
            int enemyHp = state.Enemy.Hp;

            BattleCalculator calculator = new BattleCalculator();
            BattleCalculation calculation = calculator.CalculatePlay(state, inner, outer);

            Assert.IsNotNull(calculation);
            Assert.AreEqual(handCount, state.Hand.Count);
            Assert.AreEqual(energy, state.Player.Energy);
            Assert.AreEqual(enemyHp, state.Enemy.Hp);
        }

        [Test]
        public void Controller_PlayHexagramConsumesEnergyAndCards()
        {
            BattleController controller = BattleController.CreatePrototype(1234);
            BattleState state = controller.State;
            CardInstance inner = state.Hand[0];
            CardInstance outer = state.Hand[1];
            int energy = state.Player.Energy;

            BattleCommandResult result = controller.PlayHexagram(inner.Uid, outer.Uid);

            Assert.IsTrue(result.Success, result.Error);
            Assert.AreEqual(energy - 1, state.Player.Energy);
            Assert.IsNull(state.FindHandCard(inner.Uid));
            Assert.IsNull(state.FindHandCard(outer.Uid));
        }

        [Test]
        public void Calculator_PreviewMatchesExecutionDamage()
        {
            BattleController controller = BattleController.CreatePrototype(1234);
            CardInstance inner = controller.State.Hand[0];
            CardInstance outer = controller.State.Hand[1];
            BattleCommandResult preview = controller.PreviewPlay(inner.Uid, outer.Uid);
            BattleCommandResult execution = controller.PlayHexagram(inner.Uid, outer.Uid);

            Assert.IsTrue(preview.Success, preview.Error);
            Assert.IsTrue(execution.Success, execution.Error);
            Assert.AreEqual(preview.Calculation.Damage.RawDamage, execution.Calculation.Damage.RawDamage);
            Assert.AreEqual(preview.Calculation.Damage.FinalDamage, execution.Calculation.Damage.FinalDamage);
        }

        [Test]
        public void WeaponConfig_ContainsDistinctAttackPatterns()
        {
            WeaponDefinition ironSword = BattleConfigDatabase.GetWeapon("weapon_iron_sword");
            WeaponDefinition qingfeng = BattleConfigDatabase.GetWeapon("weapon_qingfeng");
            WeaponDefinition rainstorm = BattleConfigDatabase.GetWeapon("weapon_rainstorm");

            Assert.AreEqual(AttackPattern.Single, ironSword.AttackPattern);
            Assert.AreEqual(AttackPattern.MultiHit, qingfeng.AttackPattern);
            Assert.AreEqual(3, qingfeng.HitCount);
            Assert.IsTrue(qingfeng.ElementAffinity.HasValue);
            Assert.AreEqual(ElementType.Wood, qingfeng.ElementAffinity.Value);
            Assert.AreEqual(AttackPattern.AllTargets, rainstorm.AttackPattern);
            Assert.AreEqual(2, rainstorm.HitCount);
        }

        [Test]
        public void Calculator_WeaponAffinityBoostsMatchingElement()
        {
            DeckEntryDefinition[] deck =
            {
                new DeckEntryDefinition(TrigramId.Xun, 1, 1, 1),
                new DeckEntryDefinition(TrigramId.Qian, 1, 1, 1),
            };
            BattleController controller = BattleController.CreatePrototype(
                1234,
                deckEntries: deck,
                weaponId: "weapon_qingfeng",
                artifactIds: new string[0],
                talismanIds: new string[0]);
            BattleState state = controller.State;
            CardInstance inner = FindCard(state, TrigramId.Xun);
            CardInstance outer = FindOtherCard(state, inner.Uid);
            BattleCalculation calculation = new BattleCalculator().CalculatePlay(state, inner, outer);

            Assert.AreEqual(AttackPattern.MultiHit, calculation.AttackPattern);
            Assert.AreEqual(12500, calculation.Damage.WeaponAffinityMultiplier);
            Assert.AreEqual(3, calculation.HitCount);
        }

        [Test]
        public void Controller_MultiHitDealsEachHit()
        {
            DeckEntryDefinition[] deck =
            {
                new DeckEntryDefinition(TrigramId.Xun, 1, 1, 1),
                new DeckEntryDefinition(TrigramId.Qian, 1, 1, 1),
            };
            BattleController controller = BattleController.CreatePrototype(
                1234,
                deckEntries: deck,
                weaponId: "weapon_qingfeng",
                artifactIds: new string[0],
                talismanIds: new string[0]);
            BattleState state = controller.State;
            CardInstance inner = FindCard(state, TrigramId.Xun);
            CardInstance outer = FindOtherCard(state, inner.Uid);
            BattleCommandResult preview = controller.PreviewPlay(inner.Uid, outer.Uid);
            int hpBefore = state.Enemy.Hp;

            BattleCommandResult result = controller.PlayHexagram(inner.Uid, outer.Uid);

            Assert.IsTrue(result.Success, result.Error);
            Assert.AreEqual(3, preview.Calculation.HitCount);
            Assert.AreEqual(
                hpBefore - preview.Calculation.Damage.RawDamage * preview.Calculation.HitCount,
                state.Enemy.Hp);
        }

        [Test]
        public void Calculator_AllTargetsPatternIsPreserved()
        {
            DeckEntryDefinition[] deck =
            {
                new DeckEntryDefinition(TrigramId.Xun, 1, 1, 1),
                new DeckEntryDefinition(TrigramId.Qian, 1, 1, 1),
            };
            BattleController controller = BattleController.CreatePrototype(1234, deckEntries: deck, weaponId: "weapon_rainstorm");
            BattleState state = controller.State;
            CardInstance inner = FindCard(state, TrigramId.Xun);
            CardInstance outer = FindOtherCard(state, inner.Uid);
            BattleCalculation calculation = new BattleCalculator().CalculatePlay(state, inner, outer);

            Assert.AreEqual(AttackPattern.AllTargets, calculation.AttackPattern);
            Assert.AreEqual(2, calculation.HitCount);
        }

        [Test]
        public void StatusInstance_ExpiresWhenDurationReachesZero()
        {
            StatusInstance status = new StatusInstance(StatusId.Weak, 1, 1);

            status.TickTurn();

            Assert.IsTrue(status.IsExpired());
        }

        [Test]
        public void WeaponState_FourthEnchantReplacesOldest()
        {
            WeaponState weapon = new WeaponState("test", "测试武器", 1, AttackPattern.Single, 1, null, 3);
            weapon.AddEnchant(TrigramId.Li, EffectOperation.ApplyStatus(EffectTarget.Enemy, StatusId.Burn, 2, 2));
            weapon.AddEnchant(TrigramId.Xun, EffectOperation.ApplyStatus(EffectTarget.Enemy, StatusId.ArmorBreak, 1, 2));
            weapon.AddEnchant(TrigramId.Kan, EffectOperation.ApplyStatus(EffectTarget.Enemy, StatusId.Chill, 1, 2));
            weapon.AddEnchant(TrigramId.Qian, EffectOperation.ApplyStatus(EffectTarget.Enemy, StatusId.Vulnerable, 1, 2));

            Assert.AreEqual(3, weapon.Enchantments.Count);
            Assert.AreEqual(TrigramId.Xun, weapon.Enchantments[0].Source);
            Assert.AreEqual(TrigramId.Kan, weapon.Enchantments[1].Source);
            Assert.AreEqual(TrigramId.Qian, weapon.Enchantments[2].Source);
        }

        [Test]
        public void WeaponState_TickTurnExpiresEnchant()
        {
            WeaponState weapon = new WeaponState("test", "测试武器", 1, AttackPattern.Single, 1, null, 3);
            weapon.AddEnchant(TrigramId.Li, EffectOperation.ApplyStatus(EffectTarget.Enemy, StatusId.Burn, 2, 2));

            weapon.TickTurn();
            Assert.AreEqual(1, weapon.Enchantments[0].RemainingTurns);

            weapon.TickTurn();
            Assert.AreEqual(0, weapon.Enchantments.Count);
        }

        [Test]
        public void Resolver_DurationEffectWritesEnchantSlot()
        {
            DeckEntryDefinition[] deck =
            {
                new DeckEntryDefinition(TrigramId.Li, 1, 1, 1),
                new DeckEntryDefinition(TrigramId.Qian, 1, 1, 1),
            };
            BattleController controller = BattleController.CreatePrototype(1234, deckEntries: deck);
            BattleState state = controller.State;
            CardInstance inner = FindCard(state, TrigramId.Li);
            CardInstance outer = FindOtherCard(state, inner.Uid);

            BattleCommandResult result = controller.PlayHexagram(inner.Uid, outer.Uid);

            Assert.IsTrue(result.Success, result.Error);
            Assert.AreEqual(1, state.Player.Weapon.Enchantments.Count);
            Assert.AreEqual(TrigramId.Li, state.Player.Weapon.Enchantments[0].Source);
            Assert.AreEqual(StatusId.Burn, state.Player.Weapon.Enchantments[0].Effect.Status);
            Assert.AreEqual(2, state.Player.Weapon.Enchantments[0].RemainingTurns);
        }

        [Test]
        public void Artifact_AppliesAdditiveBeforeMultiplicative()
        {
            DeckEntryDefinition[] deck =
            {
                new DeckEntryDefinition(TrigramId.Li, 1, 1, 1),
                new DeckEntryDefinition(TrigramId.Qian, 1, 1, 1),
            };
            string[] artifacts = { "artifact_first_strike", "artifact_fire_lord" };
            BattleController controller = BattleController.CreatePrototype(
                1234,
                deckEntries: deck,
                artifactIds: artifacts,
                talismanIds: new string[0]);
            BattleState state = controller.State;
            CardInstance inner = FindCard(state, TrigramId.Li);
            CardInstance outer = FindOtherCard(state, inner.Uid);

            BattleCalculation calculation = new BattleCalculator().CalculatePlay(state, inner, outer);

            Assert.AreEqual(20000, calculation.Damage.ArtifactAdditive);
            Assert.AreEqual(15000, calculation.Damage.ArtifactMultiplicative);
            Assert.AreEqual(48, calculation.Damage.RawDamage);
        }

        [Test]
        public void Artifact_FirstPlayOnlyAppliesOncePerTurn()
        {
            DeckEntryDefinition[] deck =
            {
                new DeckEntryDefinition(TrigramId.Li, 1, 1, 1),
                new DeckEntryDefinition(TrigramId.Qian, 1, 1, 1),
            };
            string[] artifacts = { "artifact_first_strike" };
            BattleController controller = BattleController.CreatePrototype(
                1234,
                deckEntries: deck,
                artifactIds: artifacts,
                talismanIds: new string[0]);
            BattleState state = controller.State;
            CardInstance inner = FindCard(state, TrigramId.Li);
            CardInstance outer = FindOtherCard(state, inner.Uid);
            BattleCalculator calculator = new BattleCalculator();

            BattleCalculation first = calculator.CalculatePlay(state, inner, outer);
            state.PlaysThisTurn = 1;
            BattleCalculation second = calculator.CalculatePlay(state, inner, outer);

            Assert.AreEqual(20000, first.Damage.ArtifactAdditive);
            Assert.AreEqual(10000, second.Damage.ArtifactAdditive);
        }

        [Test]
        public void Resolver_AfterPlayArtifactAppliesEffects()
        {
            DeckEntryDefinition[] deck =
            {
                new DeckEntryDefinition(TrigramId.Qian, 1, 1, 1),
                new DeckEntryDefinition(TrigramId.Kun, 1, 1, 1),
            };
            string[] artifacts = { "artifact_tai_truth" };
            BattleController controller = BattleController.CreatePrototype(
                1234,
                deckEntries: deck,
                artifactIds: artifacts,
                talismanIds: new string[0]);
            BattleState state = controller.State;
            CardInstance inner = FindCard(state, TrigramId.Qian);
            CardInstance outer = FindOtherCard(state, inner.Uid);
            state.Player.TrySpendEnergy(3);

            BattleCommandResult result = controller.PlayHexagram(inner.Uid, outer.Uid);

            Assert.IsTrue(result.Success, result.Error);
            Assert.AreEqual(2, state.Player.Energy);
            Assert.AreEqual(1, state.PlaysThisTurn);
        }

        [Test]
        public void Controller_TalismanAppliesAndConsumes()
        {
            string[] talismans = { "talisman_shield" };
            BattleController controller = BattleController.CreatePrototype(
                1234,
                artifactIds: new string[0],
                talismanIds: talismans);
            int shieldBefore = controller.State.Player.Shield;

            BattleCommandResult result = controller.UseTalisman(0);

            Assert.IsTrue(result.Success, result.Error);
            Assert.AreEqual(shieldBefore + 6, controller.State.Player.Shield);
            Assert.AreEqual(0, controller.State.Talismans.Count);
            Assert.IsFalse(controller.UseTalisman(0).Success);
        }

        [Test]
        public void EnemyConfig_ContainsIntentPool()
        {
            EnemyDefinition enemy = BattleConfigDatabase.GetEnemy("enemy_wood_demon");

            Assert.AreEqual(EnemyKind.Normal, enemy.Kind);
            Assert.AreEqual(EnemyIntentMode.Sequence, enemy.IntentMode);
            Assert.AreEqual(4, enemy.Intents.Count);
            Assert.AreEqual("intent_wood_attack", enemy.Intents[0].Id);
        }

        [Test]
        public void EnemyIntentSelector_SkipsConditionalIntentUntilThreshold()
        {
            EnemyIntentDefinition burst = new EnemyIntentDefinition(
                "burst",
                EnemyIntentType.Attack,
                9,
                StatusId.None,
                0,
                0,
                1,
                0,
                EnemyIntentConditionType.HpBelow,
                50,
                "蓄力重击");
            EnemyIntentDefinition attack = new EnemyIntentDefinition(
                "attack",
                EnemyIntentType.Attack,
                6,
                StatusId.None,
                0,
                0,
                1,
                0,
                EnemyIntentConditionType.Always,
                0,
                "攻击");
            EnemyState enemy = new EnemyState(
                "test_enemy",
                "测试敌人",
                ElementType.Wood,
                60,
                6,
                EnemyKind.Normal,
                EnemyIntentMode.Sequence,
                new[] { burst, attack });
            EnemyIntentSelector selector = new EnemyIntentSelector();

            EnemyIntent first = selector.SelectNext(enemy, 1, new System.Random(1234));
            enemy.ApplyDamage(31);
            enemy.SetIntentSequenceIndex(0);
            EnemyIntent second = selector.SelectNext(enemy, 2, new System.Random(1234));

            Assert.AreEqual("attack", first.Id);
            Assert.AreEqual("burst", second.Id);
        }

        [Test]
        public void EnemyIntentSelector_WeightedModeIsDeterministic()
        {
            EnemyIntentDefinition attack = new EnemyIntentDefinition(
                "attack", EnemyIntentType.Attack, 6, StatusId.None, 0, 0, 1, 0, EnemyIntentConditionType.Always, 0, "攻击");
            EnemyIntentDefinition defend = new EnemyIntentDefinition(
                "defend", EnemyIntentType.Defend, 4, StatusId.None, 0, 0, 1, 0, EnemyIntentConditionType.Always, 0, "防御");
            EnemyState firstEnemy = new EnemyState(
                "test_enemy", "测试敌人", ElementType.Wood, 60, 6, EnemyKind.Elite, EnemyIntentMode.Weighted, new[] { attack, defend });
            EnemyState secondEnemy = new EnemyState(
                "test_enemy", "测试敌人", ElementType.Wood, 60, 6, EnemyKind.Elite, EnemyIntentMode.Weighted, new[] { attack, defend });
            EnemyIntentSelector selector = new EnemyIntentSelector();

            EnemyIntent first = selector.SelectNext(firstEnemy, 1, new System.Random(1234));
            EnemyIntent second = selector.SelectNext(secondEnemy, 1, new System.Random(1234));

            Assert.AreEqual(first.Id, second.Id);
        }

        [Test]
        public void EnemyState_StatusImmuneBlocksStatus()
        {
            EnemyState enemy = new EnemyState(
                "test_boss",
                "测试 Boss",
                ElementType.Wood,
                60,
                6,
                EnemyKind.Boss,
                EnemyIntentMode.Sequence,
                new EnemyIntentDefinition[0],
                EnemyRuleType.StatusImmune);

            bool applied = enemy.AddStatus(StatusId.Weak, 1, 1);

            Assert.IsFalse(applied);
            Assert.IsNull(enemy.GetStatus(StatusId.Weak));
        }
        [Test]
        public void Controller_EndTurnRunsEnemyIntent()
        {
            BattleController controller = BattleController.CreatePrototype(1234);
            int hpBefore = controller.State.Player.Hp;
            int enemyPower = controller.State.Enemy.CurrentIntent.Power;

            BattleCommandResult result = controller.EndTurn();

            Assert.IsTrue(result.Success, result.Error);
            Assert.AreEqual(hpBefore - enemyPower, controller.State.Player.Hp);
            Assert.AreEqual(2, controller.State.Turn);
            Assert.AreEqual(BattlePhase.PlayerAction, controller.State.Phase);
        }

        private static CardInstance FindCard(BattleState state, TrigramId trigram)
        {
            for (int i = 0; i < state.Hand.Count; i++)
            {
                if (state.Hand[i].Trigram == trigram)
                {
                    return state.Hand[i];
                }
            }

            return null;
        }

        private static CardInstance FindOtherCard(BattleState state, int uid)
        {
            for (int i = 0; i < state.Hand.Count; i++)
            {
                if (state.Hand[i].Uid != uid)
                {
                    return state.Hand[i];
                }
            }

            return null;
        }
    }
}
