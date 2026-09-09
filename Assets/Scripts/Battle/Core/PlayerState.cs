using System.Collections.Generic;

namespace BalartroLike.Battle
{
    public sealed class PlayerState
    {
        public int MaxHp { get; }
        public int Hp { get; private set; }
        public int Shield { get; private set; }
        public int MaxEnergy { get; }
        public int Energy { get; private set; }
        public int EnergyPerTurn { get; }
        public WeaponState Weapon { get; }
        public List<StatusInstance> Statuses { get; }

        public PlayerState(int maxHp, int maxEnergy, int energyPerTurn, WeaponState weapon)
        {
            MaxHp = maxHp;
            Hp = maxHp;
            MaxEnergy = maxEnergy;
            Energy = energyPerTurn;
            EnergyPerTurn = energyPerTurn;
            Weapon = weapon;
            Statuses = new List<StatusInstance>();
        }

        public bool TrySpendEnergy(int amount)
        {
            if (Energy < amount)
            {
                return false;
            }

            Energy -= amount;
            return true;
        }

        public void GainEnergy(int amount)
        {
            Energy += amount;
            if (Energy > MaxEnergy)
            {
                Energy = MaxEnergy;
            }
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