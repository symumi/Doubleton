using System.Collections.Generic;

namespace BalartroLike.Battle
{
    public sealed class BattleResolver
    {
        public void ResolvePlay(BattleState state, BattleCalculation calculation, List<BattleEvent> events)
        {
            CardInstance innerCard = state.FindHandCard(calculation.InnerCardUid);
            CardInstance outerCard = state.FindHandCard(calculation.OuterCardUid);
            state.RemoveHandCard(calculation.InnerCardUid);
            state.RemoveHandCard(calculation.OuterCardUid);
            state.Player.TrySpendEnergy(1);

            events.Add(BattleEvent.HexagramFormed(calculation.Hexagram));
            int absorbed = state.Enemy.ApplyDamage(calculation.Damage.RawDamage);
            events.Add(BattleEvent.DamageDealt(calculation.Damage.RawDamage));
            if (absorbed > 0)
            {
                events.Add(new BattleEvent(BattleEventType.ShieldChanged, "敌方护盾吸收 " + absorbed, absorbed));
            }

            ApplyEffects(state, calculation.Effects, events);
            state.DiscardPile.Add(innerCard);
            state.DiscardPile.Add(outerCard);
            CheckBattleEnd(state, events);
        }

        public void ResolveEndTurn(BattleState state, List<BattleEvent> events)
        {
            StatusInstance burn = state.Enemy.GetStatus(StatusId.Burn);
            if (burn != null && burn.Stacks > 0)
            {
                int damage = burn.Stacks;
                state.Enemy.ApplyDamage(damage);
                burn.ConsumeStacks(damage);
                events.Add(new BattleEvent(BattleEventType.DamageDealt, "灼烧造成 " + damage + " 点伤害", damage));
            }

            state.Enemy.TickStatuses();
            state.Player.TickStatuses();
            CheckBattleEnd(state, events);
        }

        public void ResolveEnemyIntent(BattleState state, List<BattleEvent> events)
        {
            EnemyIntent intent = state.Enemy.CurrentIntent;
            if (intent == null)
            {
                return;
            }

            if (intent.Type == EnemyIntentType.Attack)
            {
                int absorbed = state.Player.ApplyDamage(intent.Power);
                events.Add(new BattleEvent(BattleEventType.EnemyActed, "敌人攻击 " + intent.Power, intent.Power));
                if (absorbed > 0)
                {
                    events.Add(new BattleEvent(BattleEventType.ShieldChanged, "护盾吸收 " + absorbed, absorbed));
                }
            }
            else if (intent.Type == EnemyIntentType.Defend)
            {
                state.Enemy.AddShield(intent.Power);
                events.Add(new BattleEvent(BattleEventType.EnemyActed, "敌人获得 " + intent.Power + " 点护盾", intent.Power));
            }
            else if (intent.Type == EnemyIntentType.Debuff)
            {
                state.Player.AddStatus(intent.Status, intent.StatusStacks, intent.StatusDuration);
                events.Add(new BattleEvent(BattleEventType.StatusChanged, "敌人施加状态", intent.StatusStacks));
            }

            CheckBattleEnd(state, events);
        }

        private static void ApplyEffects(BattleState state, List<EffectOperation> effects, List<BattleEvent> events)
        {
            for (int i = 0; i < effects.Count; i++)
            {
                EffectOperation effect = effects[i];
                if (effect.Type == EffectType.ApplyStatus)
                {
                    if (effect.Target == EffectTarget.Enemy)
                    {
                        state.Enemy.AddStatus(effect.Status, effect.Stacks, effect.Duration);
                        events.Add(new BattleEvent(BattleEventType.StatusChanged, "敌方获得状态", effect.Stacks));
                    }
                    else
                    {
                        state.Player.AddStatus(effect.Status, effect.Stacks, effect.Duration);
                        events.Add(new BattleEvent(BattleEventType.StatusChanged, "玩家获得状态", effect.Stacks));
                    }
                }
                else if (effect.Type == EffectType.GainShield)
                {
                    state.Player.AddShield(effect.Value);
                    events.Add(new BattleEvent(BattleEventType.ShieldChanged, "获得 " + effect.Value + " 点护盾", effect.Value));
                }
                else if (effect.Type == EffectType.GainEnergy)
                {
                    state.Player.GainEnergy(effect.Value);
                    events.Add(BattleEvent.EnergyChanged(effect.Value));
                }
                else if (effect.Type == EffectType.DrawCard)
                {
                    DrawCards(state, effect.Value, events);
                }
            }
        }

        // TODO: 抽牌效果暂不处理弃牌堆重洗，P1 接入统一牌堆服务后补齐。
        private static void DrawCards(BattleState state, int count, List<BattleEvent> events)
        {
            for (int i = 0; i < count; i++)
            {
                if (state.Hand.Count >= 6 || state.DrawPile.Count == 0)
                {
                    return;
                }

                int index = state.DrawPile.Count - 1;
                CardInstance card = state.DrawPile[index];
                state.DrawPile.RemoveAt(index);
                state.Hand.Add(card);
                events.Add(BattleEvent.CardDrawn(card));
            }
        }

        private static void CheckBattleEnd(BattleState state, List<BattleEvent> events)
        {
            if (state.Enemy.Hp <= 0)
            {
                state.Result = BattleResultType.Victory;
                state.Phase = BattlePhase.BattleEnd;
                events.Add(BattleEvent.BattleEnded(BattleResultType.Victory));
                return;
            }

            if (state.Player.Hp <= 0)
            {
                state.Result = BattleResultType.Defeat;
                state.Phase = BattlePhase.BattleEnd;
                events.Add(BattleEvent.BattleEnded(BattleResultType.Defeat));
            }
        }
    }
}