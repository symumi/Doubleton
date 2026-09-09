using BalartroLike.Battle;
using NUnit.Framework;

namespace BalartroLike.Tests
{
    public sealed class BattleCoreTests
    {
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
        public void StatusInstance_ExpiresWhenDurationReachesZero()
        {
            StatusInstance status = new StatusInstance(StatusId.Weak, 1, 1);

            status.TickTurn();

            Assert.IsTrue(status.IsExpired());
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
    }
}