using System;
using System.Collections.Generic;
using BalartroLike.Battle;
using BalartroLike.Run;

namespace BalartroLike.Save
{
    [Serializable]
    public sealed class GameSaveData
    {
        public int save_version;
        public string updated_at;
        public MetaSaveData meta;
        public RunSaveData run;
        public BattleSaveData battle;
    }

    [Serializable]
    public sealed class MetaSaveData
    {
        public int dao_heart;
        public int highest_heaven_tribulation;
        public int selected_heaven_tribulation;
        public int completed_run_count;
        public List<HexagramUseSaveData> hexagram_uses = new List<HexagramUseSaveData>();
    }

    [Serializable]
    public sealed class HexagramUseSaveData
    {
        public string hexagram_id;
        public int use_count;
    }

    [Serializable]
    public sealed class RunSaveData
    {
        public int seed;
        public int current_node_index;
        public int spirit_stones;
        public RunPhase phase;
        public RunResultType result;
        public BattleResultType pending_battle_result;
        public int pending_reward_spirit_stones;
        public int pending_realm_reward_spirit_stones;
        public string promotion_target_realm_id;
        public int dao_heart_reward;
        public int max_hp_bonus;
        public int weapon_power_bonus;
        public int heaven_tribulation_level;
        public string weapon_id;
        public string active_shop_id;
        public int shop_refresh_count;
        public string active_event_id;
        public bool event_resolved;
        public string event_result_text;
        public List<string> completed_node_ids = new List<string>();
        public List<RunDeckCardSaveData> deck = new List<RunDeckCardSaveData>();
        public List<string> artifact_ids = new List<string>();
        public List<string> talisman_ids = new List<string>();
        public List<string> active_shop_offer_ids = new List<string>();
        public List<string> purchased_shop_offer_ids = new List<string>();
    }

    [Serializable]
    public sealed class RunDeckCardSaveData
    {
        public TrigramId trigram;
        public int rank;
        public int qi;
    }

    [Serializable]
    public sealed class BattleSaveData
    {
        public int seed;
        public int turn;
        public int discards_remaining;
        public int discard_limit;
        public int hand_limit;
        public int plays_this_turn;
        public BattlePhase phase;
        public BattleResultType result;
        public PlayerSaveData player;
        public EnemySaveData enemy;
        public WeaponSaveData weapon;
        public List<string> artifact_ids = new List<string>();
        public List<string> talisman_ids = new List<string>();
        public List<CardSaveData> draw_pile = new List<CardSaveData>();
        public List<CardSaveData> hand = new List<CardSaveData>();
        public List<CardSaveData> discard_pile = new List<CardSaveData>();
    }

    [Serializable]
    public sealed class PlayerSaveData
    {
        public int max_hp;
        public int hp;
        public int shield;
        public int max_energy;
        public int energy;
        public int energy_per_turn;
        public List<StatusSaveData> statuses = new List<StatusSaveData>();
    }

    [Serializable]
    public sealed class EnemySaveData
    {
        public string id;
        public int max_hp;
        public int hp;
        public int shield;
        public int base_power;
        public int power_bonus;
        public int intent_sequence_index;
        public bool has_intent;
        public EnemyIntentSaveData intent;
        public List<StatusSaveData> statuses = new List<StatusSaveData>();
    }

    [Serializable]
    public sealed class EnemyIntentSaveData
    {
        public string id;
        public EnemyIntentType type;
        public int power;
        public StatusId status;
        public int status_stacks;
        public int status_duration;
        public string display_text;
    }

    [Serializable]
    public sealed class WeaponSaveData
    {
        public string id;
        public int base_power;
        public int power_bonus;
        public AttackPattern attack_pattern;
        public int hit_count;
        public bool has_element_affinity;
        public ElementType element_affinity;
        public int max_enchant_slots;
        public int next_order;
        public List<EnchantSaveData> enchantments = new List<EnchantSaveData>();
    }

    [Serializable]
    public sealed class CardSaveData
    {
        public int uid;
        public TrigramId trigram;
        public int rank;
        public int qi;
    }

    [Serializable]
    public sealed class EnchantSaveData
    {
        public int order;
        public TrigramId source;
        public EffectType effect_type;
        public EffectTarget target;
        public Battle.ValueType value_type;
        public int value;
        public StatusId status;
        public int stacks;
        public int duration;
        public int remaining_turns;
    }

    [Serializable]
    public sealed class StatusSaveData
    {
        public StatusId id;
        public int stacks;
        public int remaining_turns;
    }
}
