using System;
using System.Collections.Generic;

namespace BalartroLike.Battle
{
    public sealed class BattleController
    {
        public BattleState State { get; }

        private readonly BattleCalculator _calculator;
        private readonly BattleResolver _resolver;
        private readonly Random _random;

        public BattleController(BattleState state)
        {
            State = state;
            _calculator = new BattleCalculator();
            _resolver = new BattleResolver();
            _random = new Random(state.Seed);
        }

        public static BattleController CreatePrototype(int seed = 20260909)
        {
            WeaponState weapon = new WeaponState("weapon_iron_sword", "玄铁重剑", 6, null, 3);
            PlayerState player = new PlayerState(30, 4, 2, weapon);
            EnemyState enemy = new EnemyState("enemy_wood_demon", "木魈", ElementType.Wood, 48, 6);
            BattleState state = new BattleState(seed, player, enemy);
            BuildDeck(state);

            BattleController controller = new BattleController(state);
            controller.ShuffleDrawPile();
            controller.StartBattle();
            return controller;
        }

        public BattleCommandResult StartBattle()
        {
            if (State.Phase != BattlePhase.BattleStart)
            {
                return BattleCommandResult.Fail("战斗已经开始");
            }

            State.Events.Add(new BattleEvent(BattleEventType.BattleStarted, "战斗开始"));
            StartPlayerTurn();
            SelectEnemyIntent();
            return BattleCommandResult.Ok(null);
        }

        public BattleCommandResult PreviewPlay(int innerCardUid, int outerCardUid)
        {
            if (State.Phase != BattlePhase.PlayerAction)
            {
                return BattleCommandResult.Fail("当前不能出卦");
            }

            if (State.Player.Energy <= 0)
            {
                return BattleCommandResult.Fail("灵力不足");
            }

            if (innerCardUid == outerCardUid)
            {
                return BattleCommandResult.Fail("内卦和外卦不能是同一张牌");
            }

            CardInstance innerCard = State.FindHandCard(innerCardUid);
            CardInstance outerCard = State.FindHandCard(outerCardUid);
            if (innerCard == null || outerCard == null)
            {
                return BattleCommandResult.Fail("选择的牌不在手牌中");
            }

            BattleCalculation calculation = _calculator.CalculatePlay(State, innerCard, outerCard);
            return BattleCommandResult.Ok(calculation);
        }

        public BattleCommandResult PlayHexagram(int innerCardUid, int outerCardUid)
        {
            BattleCommandResult preview = PreviewPlay(innerCardUid, outerCardUid);
            if (!preview.Success)
            {
                return preview;
            }

            State.Phase = BattlePhase.Resolving;
            _resolver.ResolvePlay(State, preview.Calculation, State.Events);
            if (State.Result == BattleResultType.None)
            {
                State.Phase = BattlePhase.PlayerAction;
            }

            return preview;
        }

        public BattleCommandResult DiscardCards(int[] cardUids)
        {
            if (State.Phase != BattlePhase.PlayerAction)
            {
                return BattleCommandResult.Fail("当前不能弃牌");
            }

            if (cardUids == null || cardUids.Length == 0)
            {
                return BattleCommandResult.Fail("没有选择要弃掉的牌");
            }

            if (cardUids.Length > State.DiscardsRemaining)
            {
                return BattleCommandResult.Fail("弃牌次数不足");
            }

            for (int i = 0; i < cardUids.Length; i++)
            {
                if (State.FindHandCard(cardUids[i]) == null)
                {
                    return BattleCommandResult.Fail("选择的牌不在手牌中");
                }

                for (int j = i + 1; j < cardUids.Length; j++)
                {
                    if (cardUids[i] == cardUids[j])
                    {
                        return BattleCommandResult.Fail("不能重复弃掉同一张牌");
                    }
                }
            }

            for (int i = 0; i < cardUids.Length; i++)
            {
                CardInstance card = State.FindHandCard(cardUids[i]);
                State.RemoveHandCard(cardUids[i]);
                State.DiscardPile.Add(card);
                State.Events.Add(new BattleEvent(BattleEventType.CardDiscarded, "弃掉" + TrigramCatalog.Get(card.Trigram).DisplayName, 0, card));
            }

            State.DiscardsRemaining -= cardUids.Length;
            DrawCards(cardUids.Length, State.Events);
            return BattleCommandResult.Ok(null);
        }

        public BattleCommandResult EndTurn()
        {
            if (State.Phase != BattlePhase.PlayerAction)
            {
                return BattleCommandResult.Fail("当前不能结束回合");
            }

            State.Phase = BattlePhase.EnemyTurn;
            _resolver.ResolveEndTurn(State, State.Events);
            if (State.Result != BattleResultType.None)
            {
                return BattleCommandResult.Ok(null);
            }

            _resolver.ResolveEnemyIntent(State, State.Events);
            if (State.Result != BattleResultType.None)
            {
                return BattleCommandResult.Ok(null);
            }

            StartPlayerTurn();
            SelectEnemyIntent();
            return BattleCommandResult.Ok(null);
        }

        private void StartPlayerTurn()
        {
            State.Turn++;
            State.Phase = BattlePhase.PlayerTurnStart;
            State.Player.GainEnergy(State.Player.EnergyPerTurn);
            State.DiscardsRemaining = State.DiscardLimit;
            State.Player.Weapon.TickTurn();
            DrawCards(6 - State.Hand.Count, State.Events);
            State.Phase = BattlePhase.PlayerAction;
            State.Events.Add(new BattleEvent(BattleEventType.PlayerTurnStarted, "第 " + State.Turn + " 回合", State.Turn));
        }

        private void DrawCards(int count, List<BattleEvent> events)
        {
            for (int i = 0; i < count; i++)
            {
                if (State.Hand.Count >= 6)
                {
                    return;
                }

                if (State.DrawPile.Count == 0)
                {
                    if (State.DiscardPile.Count == 0)
                    {
                        return;
                    }

                    State.DrawPile.AddRange(State.DiscardPile);
                    State.DiscardPile.Clear();
                    ShuffleDrawPile();
                }

                int index = State.DrawPile.Count - 1;
                CardInstance card = State.DrawPile[index];
                State.DrawPile.RemoveAt(index);
                State.Hand.Add(card);
                events.Add(BattleEvent.CardDrawn(card));
            }
        }

        private void ShuffleDrawPile()
        {
            for (int i = State.DrawPile.Count - 1; i > 0; i--)
            {
                int swapIndex = _random.Next(i + 1);
                CardInstance card = State.DrawPile[i];
                State.DrawPile[i] = State.DrawPile[swapIndex];
                State.DrawPile[swapIndex] = card;
            }
        }

        // TODO: 当前为原型交替意图，后续替换为 EnemyDefinition 意图池。
        private void SelectEnemyIntent()
        {
            EnemyIntent intent;
            if (State.Turn % 2 == 0)
            {
                intent = new EnemyIntent(
                    EnemyIntentType.Debuff,
                    State.Enemy.BasePower,
                    StatusId.Weak,
                    1,
                    1,
                    "虚弱：下回合造成伤害降低");
            }
            else
            {
                intent = new EnemyIntent(
                    EnemyIntentType.Attack,
                    State.Enemy.BasePower,
                    StatusId.None,
                    0,
                    0,
                    "攻击 " + State.Enemy.BasePower);
            }

            State.Enemy.SetIntent(intent);
            State.Events.Add(new BattleEvent(BattleEventType.EnemyIntentChanged, intent.DisplayText, intent.Power));
        }

        private static void BuildDeck(BattleState state)
        {
            int[] ranks = { 1, 1, 2, 3 };
            int[] qi = { 1, 1, 2, 4 };
            int uid = 1;
            for (int trigram = 0; trigram < 8; trigram++)
            {
                for (int copy = 0; copy < ranks.Length; copy++)
                {
                    state.DrawPile.Add(new CardInstance(uid, (TrigramId)trigram, ranks[copy], qi[copy]));
                    uid++;
                }
            }
        }
    }
}