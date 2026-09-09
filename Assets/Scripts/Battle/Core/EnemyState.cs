using System.Collections.Generic;

namespace BalartroLike.Battle
{
    public sealed class EnemyState
    {
        private readonly Dictionary<string, int> _intentLastUsedTurns = new Dictionary<string, int>();

        public string Id { get; }
        public string DisplayName { get; }
        public EnemyKind Kind { get; }
        public ElementType Element { get; }
        public int MaxHp { get; }
        public int Hp { get; private set; }
        public int Shield { get; private set; }
        public int BasePower { get; }
        public int PowerBonus { get; }
        public EnemyIntentMode IntentMode { get; }
        public IReadOnlyList<EnemyIntentDefinition> Intents { get; }
        public EnemyRuleType RuleType { get; }
        public int IntentSequenceIndex { get; private set; }
        public List<StatusInstance> Statuses { get; }
        public EnemyIntent CurrentIntent { get; private set; }

        public EnemyState(
            string id,
            string displayName,
            ElementType element,
            int maxHp,
            int basePower,
            EnemyKind kind = EnemyKind.Normal,
            EnemyIntentMode intentMode = EnemyIntentMode.Sequence,
            IReadOnlyList<EnemyIntentDefinition> intents = null,
            EnemyRuleType ruleType = EnemyRuleType.None,
            int powerBonus = 0)
        {
            Id = id;
            DisplayName = displayName;
            Kind = kind;
            Element = element;
            MaxHp = maxHp;
            Hp = maxHp;
            BasePower = basePower;
            PowerBonus = powerBonus;
            IntentMode = intentMode;
            Intents = intents ?? new EnemyIntentDefinition[0];
            RuleType = ruleType;
            Statuses = new List<StatusInstance>();
        }

        public void Restore(int hp, int shield, int intentSequenceIndex, EnemyIntent currentIntent, IReadOnlyList<StatusInstance> statuses)
        {
            Hp = hp > MaxHp ? MaxHp : hp < 0 ? 0 : hp;
            Shield = shield < 0 ? 0 : shield;
            IntentSequenceIndex = intentSequenceIndex < 0 ? 0 : intentSequenceIndex;
            CurrentIntent = currentIntent;
            Statuses.Clear();
            if (statuses != null)
            {
                Statuses.AddRange(statuses);
            }
        }
        public void SetIntent(EnemyIntent intent)
        {
            CurrentIntent = intent;
        }

        public bool IsIntentReady(EnemyIntentDefinition intent, int turn)
        {
            if (!_intentLastUsedTurns.TryGetValue(intent.Id, out int lastUsedTurn))
            {
                return true;
            }

            return turn - lastUsedTurn > intent.Cooldown;
        }

        public void RecordIntentSelected(EnemyIntentDefinition intent, int turn)
        {
            _intentLastUsedTurns[intent.Id] = turn;
        }

        public void SetIntentSequenceIndex(int index)
        {
            IntentSequenceIndex = index;
        }

        public bool AddStatus(StatusId id, int stacks, int duration)
        {
            if (id == StatusId.None || RuleType == EnemyRuleType.StatusImmune)
            {
                return false;
            }

            StatusInstance status = GetStatus(id);
            if (status == null)
            {
                Statuses.Add(new StatusInstance(id, stacks, duration));
                return true;
            }

            status.Add(stacks, duration);
            return true;
        }

        public StatusInstance GetStatus(StatusId id)
        {
            for (int i = 0; i < Statuses.Count; i++)
            {
                if (Statuses[i].Id == id)
                {
                    return Statuses[i];
                }
            }

            return null;
        }

        public void AddShield(int amount)
        {
            Shield += amount;
        }

        public int ApplyDamage(int amount)
        {
            int absorbed = Shield >= amount ? amount : Shield;
            Shield -= absorbed;
            int hpDamage = amount - absorbed;
            Hp -= hpDamage;
            if (Hp < 0)
            {
                Hp = 0;
            }

            return absorbed;
        }

        public void TickStatuses()
        {
            for (int i = Statuses.Count - 1; i >= 0; i--)
            {
                Statuses[i].TickTurn();
                if (Statuses[i].IsExpired())
                {
                    Statuses.RemoveAt(i);
                }
            }
        }
    }
}