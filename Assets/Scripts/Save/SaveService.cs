using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using BalartroLike.Battle;
using BalartroLike.Run;

namespace BalartroLike.Save
{
    public static class SaveService
    {
        public const int CurrentVersion = 1;

        public static GameSaveData Capture(RunController run, BattleController battle)
        {
            return new GameSaveData
            {
                save_version = CurrentVersion,
                updated_at = DateTime.UtcNow.ToString("o"),
                meta = CaptureMeta(run.MetaProgress),
                run = CaptureRun(run.State),
                battle = battle == null ? null : CaptureBattle(battle.State)
            };
        }

        public static bool TrySave(string path, RunController run, BattleController battle, out string error)
        {
            if (run == null)
            {
                error = "局内状态不存在。";
                return false;
            }

            return SaveFileStorage.TryWrite(path, Serialize(Capture(run, battle)), out error);
        }

        public static bool TrySaveMeta(string path, RunMetaProgressState meta, out string error)
        {
            if (meta == null)
            {
                error = "局外状态不存在。";
                return false;
            }

            GameSaveData data = new GameSaveData
            {
                save_version = CurrentVersion,
                updated_at = DateTime.UtcNow.ToString("o"),
                meta = CaptureMeta(meta)
            };
            return SaveFileStorage.TryWrite(path, Serialize(data), out error);
        }

        public static bool TryLoad(string path, out GameSaveData data, out string error)
        {
            if (TryLoadFile(path, out data, out string mainError))
            {
                error = string.Empty;
                return true;
            }

            string backupPath = path + ".bak";
            if (TryLoadFile(backupPath, out data, out string backupError))
            {
                error = string.Empty;
                return true;
            }

            data = null;
            error = "主存档读取失败：" + mainError + "；备份读取失败：" + backupError;
            return false;
        }

        public static bool TryRestore(
            GameSaveData data,
            out RunController run,
            out BattleController battle,
            out RunMetaProgressState meta,
            out string error)
        {
            run = null;
            battle = null;
            meta = null;
            error = string.Empty;

            if (data == null)
            {
                error = "存档为空。";
                return false;
            }

            if (data.save_version != CurrentVersion)
            {
                error = "存档版本不兼容：" + data.save_version;
                return false;
            }

            Normalize(data);
            meta = new RunMetaProgressState();
            ApplyMeta(meta, data.meta);
            if (data.run == null)
            {
                return true;
            }

            if (!RunConfigDatabase.IsLoaded || !RunEncounterDatabase.IsLoaded || !BattleConfigDatabase.IsLoaded)
            {
                error = "运行配置尚未加载。";
                return false;
            }

            run = RunController.CreatePrototype(data.run.seed, meta);
            ApplyRun(run.State, data.run);
            if (data.run.phase != RunPhase.Battle)
            {
                return true;
            }

            if (data.battle == null)
            {
                error = "战斗存档缺失。";
                return false;
            }

            return TryRestoreBattle(run, data.battle, out battle, out error);
        }

        private static string Serialize(GameSaveData data)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(GameSaveData));
                serializer.WriteObject(stream, data);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        private static GameSaveData Deserialize(string json)
        {
            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(GameSaveData));
                return (GameSaveData)serializer.ReadObject(stream);
            }
        }
        private static bool TryLoadFile(string path, out GameSaveData data, out string error)
        {
            data = null;
            if (!SaveFileStorage.TryRead(path, out string json, out error))
            {
                return false;
            }

            try
            {
                data = Deserialize(json);
            }
            catch (Exception exception)
            {
                error = exception.Message;
                return false;
            }

            if (data == null)
            {
                error = "JSON 内容无效。";
                return false;
            }

            if (data.save_version != CurrentVersion)
            {
                error = "存档版本不兼容：" + data.save_version;
                return false;
            }

            Normalize(data);
            error = string.Empty;
            return true;
        }

        private static MetaSaveData CaptureMeta(RunMetaProgressState meta)
        {
            return new MetaSaveData
            {
                dao_heart = meta.DaoHeart,
                highest_heaven_tribulation = meta.HighestHeavenTribulation,
                completed_run_count = meta.CompletedRunCount
            };
        }

        private static RunSaveData CaptureRun(RunState state)
        {
            RunSaveData data = new RunSaveData
            {
                seed = state.Seed,
                current_node_index = state.CurrentNodeIndex,
                spirit_stones = state.SpiritStones,
                phase = state.Phase,
                result = state.Result,
                pending_battle_result = state.PendingBattleResult,
                pending_reward_spirit_stones = state.PendingRewardSpiritStones,
                pending_realm_reward_spirit_stones = state.PendingRealmRewardSpiritStones,
                promotion_target_realm_id = state.PromotionTargetRealmId,
                dao_heart_reward = state.DaoHeartReward,
                max_hp_bonus = state.MaxHpBonus,
                weapon_power_bonus = state.WeaponPowerBonus,
                weapon_id = state.WeaponId,
                active_shop_id = state.ActiveShopId,
                shop_refresh_count = state.ShopRefreshCount,
                active_event_id = state.ActiveEventId,
                event_resolved = state.EventResolved,
                event_result_text = state.EventResultText
            };

            data.completed_node_ids.AddRange(state.CompletedNodeIds);
            data.artifact_ids.AddRange(state.ArtifactIds);
            data.talisman_ids.AddRange(state.TalismanIds);
            data.active_shop_offer_ids.AddRange(state.ActiveShopOfferIds);
            data.purchased_shop_offer_ids.AddRange(state.PurchasedShopOfferIds);
            for (int i = 0; i < state.Deck.Count; i++)
            {
                RunDeckCard card = state.Deck[i];
                data.deck.Add(new RunDeckCardSaveData
                {
                    trigram = card.Trigram,
                    rank = card.Rank,
                    qi = card.Qi
                });
            }

            return data;
        }

        private static BattleSaveData CaptureBattle(BattleState state)
        {
            BattleSaveData data = new BattleSaveData
            {
                seed = state.Seed,
                turn = state.Turn,
                discards_remaining = state.DiscardsRemaining,
                discard_limit = state.DiscardLimit,
                hand_limit = state.HandLimit,
                plays_this_turn = state.PlaysThisTurn,
                phase = state.Phase,
                result = state.Result,
                player = new PlayerSaveData
                {
                    max_hp = state.Player.MaxHp,
                    hp = state.Player.Hp,
                    shield = state.Player.Shield,
                    max_energy = state.Player.MaxEnergy,
                    energy = state.Player.Energy,
                    energy_per_turn = state.Player.EnergyPerTurn
                },
                enemy = new EnemySaveData
                {
                    id = state.Enemy.Id,
                    max_hp = state.Enemy.MaxHp,
                    hp = state.Enemy.Hp,
                    shield = state.Enemy.Shield,
                    base_power = state.Enemy.BasePower,
                    intent_sequence_index = state.Enemy.IntentSequenceIndex,
                    has_intent = state.Enemy.CurrentIntent != null
                },
                weapon = CaptureWeapon(state.Player.Weapon)
            };

            for (int i = 0; i < state.Player.Statuses.Count; i++)
            {
                data.player.statuses.Add(CaptureStatus(state.Player.Statuses[i]));
            }

            for (int i = 0; i < state.Enemy.Statuses.Count; i++)
            {
                data.enemy.statuses.Add(CaptureStatus(state.Enemy.Statuses[i]));
            }

            if (state.Enemy.CurrentIntent != null)
            {
                EnemyIntent intent = state.Enemy.CurrentIntent;
                data.enemy.intent = new EnemyIntentSaveData
                {
                    id = intent.Id,
                    type = intent.Type,
                    power = intent.Power,
                    status = intent.Status,
                    status_stacks = intent.StatusStacks,
                    status_duration = intent.StatusDuration,
                    display_text = intent.DisplayText
                };
            }

            for (int i = 0; i < state.Artifacts.Count; i++)
            {
                data.artifact_ids.Add(state.Artifacts[i].Id);
            }

            for (int i = 0; i < state.Talismans.Count; i++)
            {
                data.talisman_ids.Add(state.Talismans[i].Id);
            }

            CaptureCards(data.draw_pile, state.DrawPile);
            CaptureCards(data.hand, state.Hand);
            CaptureCards(data.discard_pile, state.DiscardPile);
            return data;
        }

        private static WeaponSaveData CaptureWeapon(WeaponState weapon)
        {
            WeaponSaveData data = new WeaponSaveData
            {
                id = weapon.Id,
                base_power = weapon.BasePower,
                attack_pattern = weapon.AttackPattern,
                hit_count = weapon.HitCount,
                has_element_affinity = weapon.ElementAffinity.HasValue,
                element_affinity = weapon.ElementAffinity.HasValue ? weapon.ElementAffinity.Value : ElementType.Metal,
                max_enchant_slots = weapon.MaxEnchantSlots,
                next_order = weapon.NextOrder
            };

            for (int i = 0; i < weapon.Enchantments.Count; i++)
            {
                EnchantInstance enchant = weapon.Enchantments[i];
                data.enchantments.Add(new EnchantSaveData
                {
                    order = enchant.Order,
                    source = enchant.Source,
                    effect_type = enchant.Effect.Type,
                    target = enchant.Effect.Target,
                    value_type = enchant.Effect.ValueType,
                    value = enchant.Effect.Value,
                    status = enchant.Effect.Status,
                    stacks = enchant.Effect.Stacks,
                    duration = enchant.Effect.Duration,
                    remaining_turns = enchant.RemainingTurns
                });
            }

            return data;
        }

        private static StatusSaveData CaptureStatus(StatusInstance status)
        {
            return new StatusSaveData
            {
                id = status.Id,
                stacks = status.Stacks,
                remaining_turns = status.RemainingTurns
            };
        }

        private static void CaptureCards(List<CardSaveData> target, List<CardInstance> cards)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                CardInstance card = cards[i];
                target.Add(new CardSaveData
                {
                    uid = card.Uid,
                    trigram = card.Trigram,
                    rank = card.Rank,
                    qi = card.Qi
                });
            }
        }

        private static void ApplyMeta(RunMetaProgressState meta, MetaSaveData data)
        {
            if (data == null)
            {
                return;
            }

            meta.DaoHeart = data.dao_heart;
            meta.HighestHeavenTribulation = data.highest_heaven_tribulation;
            meta.CompletedRunCount = data.completed_run_count;
        }

        private static void ApplyRun(RunState state, RunSaveData data)
        {
            state.CurrentNodeIndex = data.current_node_index < 0
                ? 0
                : data.current_node_index > RunConfigDatabase.Nodes.Count ? RunConfigDatabase.Nodes.Count : data.current_node_index;
            state.SpiritStones = data.spirit_stones;
            state.Phase = data.phase;
            state.Result = data.result;
            state.PendingBattleResult = data.pending_battle_result;
            state.PendingRewardSpiritStones = data.pending_reward_spirit_stones;
            state.PendingRealmRewardSpiritStones = data.pending_realm_reward_spirit_stones;
            state.PromotionTargetRealmId = data.promotion_target_realm_id ?? string.Empty;
            state.DaoHeartReward = data.dao_heart_reward;
            state.MaxHpBonus = data.max_hp_bonus;
            state.WeaponPowerBonus = data.weapon_power_bonus;
            state.WeaponId = string.IsNullOrEmpty(data.weapon_id) ? BattleConfigDatabase.Default.DefaultWeaponId : data.weapon_id;
            if (!BattleConfigDatabase.TryGetWeapon(state.WeaponId, out _))
            {
                Trace.TraceWarning("存档中的武器不存在，已回退默认武器：" + state.WeaponId);
                state.WeaponId = BattleConfigDatabase.Default.DefaultWeaponId;
            }
            state.ActiveShopId = data.active_shop_id ?? string.Empty;
            state.ShopRefreshCount = data.shop_refresh_count;
            state.ActiveEventId = data.active_event_id ?? string.Empty;
            state.EventResolved = data.event_resolved;
            state.EventResultText = data.event_result_text ?? string.Empty;

            state.CompletedNodeIds.Clear();
            state.CompletedNodeIds.AddRange(data.completed_node_ids);
            state.ArtifactIds.Clear();
            state.ArtifactIds.AddRange(data.artifact_ids);
            state.TalismanIds.Clear();
            state.TalismanIds.AddRange(data.talisman_ids);
            state.ActiveShopOfferIds.Clear();
            state.ActiveShopOfferIds.AddRange(data.active_shop_offer_ids);
            state.PurchasedShopOfferIds.Clear();
            state.PurchasedShopOfferIds.AddRange(data.purchased_shop_offer_ids);
            state.Deck.Clear();
            for (int i = 0; i < data.deck.Count; i++)
            {
                RunDeckCardSaveData card = data.deck[i];
                state.Deck.Add(new RunDeckCard(card.trigram, card.rank, card.qi));
            }
        }

        private static bool TryRestoreBattle(RunController run, BattleSaveData data, out BattleController battle, out string error)
        {
            battle = null;
            if (data.player == null || data.enemy == null || data.weapon == null)
            {
                error = "战斗存档结构不完整。";
                return false;
            }

            if (string.IsNullOrEmpty(data.weapon.id) || !BattleConfigDatabase.TryGetWeapon(data.weapon.id, out WeaponDefinition weaponDefinition))
            {
                error = "存档中的武器不存在：" + data.weapon.id;
                return false;
            }

            if (string.IsNullOrEmpty(data.enemy.id) || !BattleConfigDatabase.TryGetEnemy(data.enemy.id, out EnemyDefinition enemyDefinition))
            {
                error = "存档中的敌人不存在：" + data.enemy.id;
                return false;
            }

            WeaponState weapon = new WeaponState(
                weaponDefinition.Id,
                weaponDefinition.DisplayName,
                data.weapon.base_power > 0 ? data.weapon.base_power : weaponDefinition.BasePower,
                data.weapon.attack_pattern,
                data.weapon.hit_count > 0 ? data.weapon.hit_count : weaponDefinition.HitCount,
                data.weapon.has_element_affinity ? (ElementType?)data.weapon.element_affinity : weaponDefinition.ElementAffinity,
                data.weapon.max_enchant_slots > 0 ? data.weapon.max_enchant_slots : weaponDefinition.MaxEnchantSlots);

            PlayerState player = new PlayerState(
                data.player.max_hp > 0 ? data.player.max_hp : BattleConfigDatabase.Default.PlayerMaxHp,
                data.player.max_energy > 0 ? data.player.max_energy : BattleConfigDatabase.Default.PlayerMaxEnergy,
                data.player.energy_per_turn > 0 ? data.player.energy_per_turn : BattleConfigDatabase.Default.EnergyPerTurn,
                weapon);

            EnemyState enemy = new EnemyState(
                enemyDefinition.Id,
                enemyDefinition.DisplayName,
                enemyDefinition.Element,
                data.enemy.max_hp > 0 ? data.enemy.max_hp : enemyDefinition.MaxHp,
                data.enemy.base_power,
                enemyDefinition.Kind,
                enemyDefinition.IntentMode,
                enemyDefinition.Intents,
                enemyDefinition.RuleType);

            BattleState state = new BattleState(data.seed, player, enemy)
            {
                Turn = data.turn,
                DiscardsRemaining = data.discards_remaining,
                DiscardLimit = data.discard_limit,
                HandLimit = data.hand_limit,
                PlaysThisTurn = data.plays_this_turn,
                Phase = data.phase,
                Result = data.result
            };

            BuildCards(state.Hand, data.hand);
            BuildCards(state.DrawPile, data.draw_pile);
            BuildCards(state.DiscardPile, data.discard_pile);
            player.Restore(data.player.hp, data.player.shield, data.player.energy, BuildStatuses(data.player.statuses));
            enemy.Restore(
                data.enemy.hp,
                data.enemy.shield,
                data.enemy.intent_sequence_index,
                BuildIntent(data.enemy),
                BuildStatuses(data.enemy.statuses));
            weapon.Restore(data.weapon.next_order, BuildEnchants(data.weapon.enchantments));
            BuildArtifacts(state, data.artifact_ids);
            BuildTalismans(state, data.talisman_ids);

            int battleCardCount = state.Hand.Count + state.DrawPile.Count + state.DiscardPile.Count;
            if (battleCardCount != run.State.Deck.Count)
            {
                error = "战斗牌组数量不一致：" + battleCardCount + "/" + run.State.Deck.Count;
                return false;
            }

            battle = new BattleController(state);
            error = string.Empty;
            return true;
        }

        private static void BuildCards(List<CardInstance> target, List<CardSaveData> source)
        {
            for (int i = 0; i < source.Count; i++)
            {
                CardSaveData card = source[i];
                target.Add(new CardInstance(card.uid, card.trigram, card.rank, card.qi));
            }
        }

        private static List<StatusInstance> BuildStatuses(List<StatusSaveData> source)
        {
            List<StatusInstance> statuses = new List<StatusInstance>();
            for (int i = 0; i < source.Count; i++)
            {
                StatusSaveData status = source[i];
                if (status.id != StatusId.None)
                {
                    statuses.Add(new StatusInstance(status.id, status.stacks, status.remaining_turns));
                }
            }

            return statuses;
        }

        private static EnemyIntent BuildIntent(EnemySaveData data)
        {
            if (!data.has_intent || data.intent == null)
            {
                return null;
            }

            return new EnemyIntent(
                data.intent.type,
                data.intent.power,
                data.intent.status,
                data.intent.status_stacks,
                data.intent.status_duration,
                data.intent.display_text ?? string.Empty,
                data.intent.id ?? string.Empty);
        }

        private static List<EnchantInstance> BuildEnchants(List<EnchantSaveData> source)
        {
            List<EnchantInstance> enchantments = new List<EnchantInstance>();
            for (int i = 0; i < source.Count; i++)
            {
                EnchantSaveData enchant = source[i];
                EffectOperation effect = new EffectOperation(
                    enchant.effect_type,
                    enchant.target,
                    enchant.value_type,
                    enchant.value,
                    enchant.status,
                    enchant.stacks,
                    enchant.duration);
                EnchantInstance instance = new EnchantInstance(enchant.order, enchant.source, effect);
                instance.RestoreRemainingTurns(enchant.remaining_turns);
                enchantments.Add(instance);
            }

            return enchantments;
        }

        // TODO: 内容更新导致法宝缺失时按配置折算灵石；当前仅忽略并告警。
        private static void BuildArtifacts(BattleState state, List<string> artifactIds)
        {
            for (int i = 0; i < artifactIds.Count; i++)
            {
                if (BattleConfigDatabase.TryGetArtifact(artifactIds[i], out ArtifactDefinition artifact))
                {
                    state.Artifacts.Add(artifact);
                }
                else
                {
                    Trace.TraceWarning("存档中的法宝不存在，已忽略：" + artifactIds[i]);
                }
            }
        }

        // TODO: 内容更新导致符箓缺失时按配置折算灵石；当前仅忽略并告警。
        private static void BuildTalismans(BattleState state, List<string> talismanIds)
        {
            for (int i = 0; i < talismanIds.Count; i++)
            {
                if (BattleConfigDatabase.TryGetTalisman(talismanIds[i], out TalismanDefinition talisman))
                {
                    state.Talismans.Add(talisman);
                }
                else
                {
                    Trace.TraceWarning("存档中的符箓不存在，已忽略：" + talismanIds[i]);
                }
            }
        }

        private static void Normalize(GameSaveData data)
        {
            data.meta = data.meta ?? new MetaSaveData();
            if (data.run == null)
            {
                return;
            }

            data.run.completed_node_ids = data.run.completed_node_ids ?? new List<string>();
            data.run.deck = data.run.deck ?? new List<RunDeckCardSaveData>();
            data.run.artifact_ids = data.run.artifact_ids ?? new List<string>();
            data.run.talisman_ids = data.run.talisman_ids ?? new List<string>();
            data.run.active_shop_offer_ids = data.run.active_shop_offer_ids ?? new List<string>();
            data.run.purchased_shop_offer_ids = data.run.purchased_shop_offer_ids ?? new List<string>();
            if (data.battle == null)
            {
                return;
            }

            data.battle.artifact_ids = data.battle.artifact_ids ?? new List<string>();
            data.battle.talisman_ids = data.battle.talisman_ids ?? new List<string>();
            data.battle.draw_pile = data.battle.draw_pile ?? new List<CardSaveData>();
            data.battle.hand = data.battle.hand ?? new List<CardSaveData>();
            data.battle.discard_pile = data.battle.discard_pile ?? new List<CardSaveData>();
            if (data.battle.player != null)
            {
                data.battle.player.statuses = data.battle.player.statuses ?? new List<StatusSaveData>();
            }

            if (data.battle.enemy != null)
            {
                data.battle.enemy.statuses = data.battle.enemy.statuses ?? new List<StatusSaveData>();
            }

            if (data.battle.weapon != null)
            {
                data.battle.weapon.enchantments = data.battle.weapon.enchantments ?? new List<EnchantSaveData>();
            }
        }
    }
}