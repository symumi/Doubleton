using System.Collections.Generic;

namespace BalartroLike.Battle
{
    public sealed class EnemyState
    {
        public string Id { get; }
        public string DisplayName { get; }
        public ElementType Element { get; }
        public int MaxHp { get; }
        public int Hp { get; private set; }
        public int Shield { get; private set; }
        public int BasePower { get; }
        public List<StatusInstance> Statuses { get; }
        public EnemyIntent CurrentIntent { get; private set; }

        public EnemyState(string id, string displayName, ElementType element, int maxHp, int basePower)
        {
            Id = id;
            DisplayName = displayName;
            Element = element;
            MaxHp = maxHp;
            Hp = maxHp;
            BasePower = basePower;
            Statuses = new List<StatusInstance>();
        }

        public void SetIntent(EnemyIntent intent)
        {
            CurrentIntent = intent;
        }

        public void AddStatus(StatusId id, int stacks, int duration)
        {
            if (id == StatusId.None)
            {
                return;
            }

            StatusInstance status = GetStatus(id);
            if (status == null)
            {
                Statuses.Add(new StatusInstance(id, stacks, duration));
                return;
            }

            status.Add(stacks, duration);
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