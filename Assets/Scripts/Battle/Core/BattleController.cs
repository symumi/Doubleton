using System;
using System.Collections.Generic;

namespace BalartroLike.Battle
{
    public sealed class BattleController
    {
        public BattleState State { get; }

        private readonly BattleCalculator _calculator;
        private readonly BattleResolver _resolver;
        private readonly EnemyIntentSelector _intentSelector;
        private readonly Random _random;

        public BattleController(BattleState state)
        {
            State = state;
            _calculator = new BattleCalculator();
            _resolver = new BattleResolver();
            _intentSelector = new EnemyIntentSelector();
            _random = new Random(state.Seed);
        }

        public static BattleController CreatePrototype(
            int seed = -1,
            string enemyId = null,
            IReadOnlyList<DeckEntryDefinition> deckEntries = null,
            int playerMaxHpBonus = 0,
            int weaponPowerBonus = 0,
            string weaponId = null,
            IReadOnlyList<string> artifactIds = null,
            IReadOnlyList<string> talismanIds = null,
            Dictionary<string, int> hexagramUses = null,
            int enemyHpPercent = 100,
            int enemyPowerBonus = 0)
        {
            if (!BattleConfigDatabase.IsLoaded)
            {
                throw new InvalidOperationException("Battle config database is not loaded.");
            }

            BattleDefaultDefinition defaults = BattleConfigDatabase.Default;
            WeaponDefinition weaponDefinition = BattleConfigDatabase.GetWeapon(weaponId ?? defaults.DefaultWeaponId);
            WeaponState weapon = new WeaponState(
                weaponDefinition.Id,
                weaponDefinition.DisplayName,
                weaponDefinition.BasePower + weaponPowerBonus,
                weaponDefinition.AttackPattern,
                weaponDefinition.HitCount,
                weaponDefinition.ElementAffinity,
                weaponDefinition.MaxEnchantSlots);

            PlayerState player = new PlayerState(
                defaults.PlayerMaxHp + playerMaxHpBonus,
                defaults.PlayerMaxEnergy,
                defaults.EnergyPerTurn,
                weapon);

            EnemyDefinition enemyDefinition = BattleConfigDatabase.GetEnemy(enemyId ?? defaults.DefaultEnemyId);
            EnemyState enemy = new EnemyState(
                enemyDefinition.Id,
                enemyDefinition.DisplayName,
                enemyDefinition.Element,
                ScalePercent(enemyDefinition.MaxHp, enemyHpPercent),
                enemyDefinition.BasePower,
                enemyDefinition.Kind,
                enemyDefinition.IntentMode,
                enemyDefinition.Intents,
                enemyDefinition.RuleType,
                enemyPowerBonus);

            BattleState state = new BattleState(seed >= 0 ? seed : defaults.DefaultSeed, player, enemy, hexagramUses);
            state.DiscardLimit = defaults.DiscardLimit;
            state.HandLimit = defaults.HandSize;
            BuildDeck(state, deckEntries ?? BattleConfigDatabase.Deck);
            BuildArtifacts(state, artifactIds ?? defaults.DefaultArtifactIds);
            BuildTalismans(state, talismanIds ?? defaults.DefaultTalismanIds);

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

        public BattleCommandResult UseTalisman(int index)
        {
            if (State.Phase != BattlePhase.PlayerAction)
            {
                return BattleCommandResult.Fail("当前不能使用符箓");
            }

            if (index < 0 || index >= State.Talismans.Count)
            {
                return BattleCommandResult.Fail("符箓不存在");
            }

            TalismanDefinition talisman = State.Talismans[index];
            if (!CanUseTalisman(talisman))
            {
                return BattleCommandResult.Fail("符箓当前无法生效");
            }

            State.Phase = BattlePhase.Resolving;
            _resolver.ResolveTalisman(State, talisman, State.Events);
            State.Talismans.RemoveAt(index);
            if (State.Result == BattleResultType.None)
            {
                State.Phase = BattlePhase.PlayerAction;
            }

            return BattleCommandResult.Ok(null);
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
            State.PlaysThisTurn = 0;
            State.Phase = BattlePhase.PlayerTurnStart;
            State.Player.GainEnergy(State.Player.EnergyPerTurn);
            State.DiscardsRemaining = State.DiscardLimit;
            State.Player.Weapon.TickTurn();
            DrawCards(State.HandLimit - State.Hand.Count, State.Events);
            State.Phase = BattlePhase.PlayerAction;
            State.Events.Add(new BattleEvent(BattleEventType.PlayerTurnStarted, "第 " + State.Turn + " 回合", State.Turn));
        }

        private void DrawCards(int count, List<BattleEvent> events)
        {
            for (int i = 0; i < count; i++)
            {
                if (State.Hand.Count >= State.HandLimit)
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

        private void SelectEnemyIntent()
        {
            EnemyIntent intent = _intentSelector.SelectNext(State.Enemy, State.Turn, _random);
            State.Enemy.SetIntent(intent);
            State.Events.Add(new BattleEvent(BattleEventType.EnemyIntentChanged, intent.DisplayText, intent.Power));
        }

        private static int ScalePercent(int value, int percent)
        {
            return (value * percent + 99) / 100;
        }

        private static void BuildDeck(BattleState state, IReadOnlyList<DeckEntryDefinition> entries)
        {
            int uid = 1;
            for (int i = 0; i < entries.Count; i++)
            {
                DeckEntryDefinition entry = entries[i];
                for (int copy = 0; copy < entry.Count; copy++)
                {
                    state.DrawPile.Add(new CardInstance(uid, entry.Trigram, entry.Rank, entry.Qi));
                    uid++;
                }
            }
        }

        private static void BuildArtifacts(BattleState state, IReadOnlyList<string> artifactIds)
        {
            for (int i = 0; i < artifactIds.Count; i++)
            {
                if (BattleConfigDatabase.TryGetArtifact(artifactIds[i], out ArtifactDefinition artifact))
                {
                    state.Artifacts.Add(artifact);
                }
            }
        }

        private static void BuildTalismans(BattleState state, IReadOnlyList<string> talismanIds)
        {
            for (int i = 0; i < talismanIds.Count; i++)
            {
                if (BattleConfigDatabase.TryGetTalisman(talismanIds[i], out TalismanDefinition talisman))
                {
                    state.Talismans.Add(talisman);
                }
            }
        }

        private bool CanUseTalisman(TalismanDefinition talisman)
        {
            for (int i = 0; i < talisman.Effects.Length; i++)
            {
                EffectOperation effect = talisman.Effects[i];
                if (effect.Type == EffectType.DrawCard
                    && (State.Hand.Count >= State.HandLimit || State.DrawPile.Count == 0))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
