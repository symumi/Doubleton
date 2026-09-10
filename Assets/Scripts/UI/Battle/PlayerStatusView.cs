using BalartroLike.Battle;
using TMPro;
using UnityEngine;

namespace BalartroLike.Unity
{
    // 玩家状态：名字、气血、护盾、灵力与状态图标。
    public sealed class PlayerStatusView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private UIProgressBar _hpBar;
        [SerializeField] private UIProgressBar _shieldBar;
        [SerializeField] private UIProgressBar _energyBar;
        [SerializeField] private StatusBarView _statusBar;

        public void Render(PlayerState player, bool animated = true)
        {
            if (player == null)
            {
                return;
            }

            if (_nameText != null)
            {
                _nameText.text = player.Weapon == null ? "修士" : player.Weapon.DisplayName;
            }

            if (_hpBar != null)
            {
                _hpBar.SetValue(player.Hp, player.MaxHp, animated);
            }

            if (_shieldBar != null)
            {
                // 护盾没有独立上限，按最大气血的比例显示。
                _shieldBar.SetValue(player.Shield, player.MaxHp, animated);
            }

            if (_energyBar != null)
            {
                _energyBar.SetValue(player.Energy, player.MaxEnergy, animated);
            }

            if (_statusBar != null)
            {
                _statusBar.Render(player.Statuses);
            }
        }
    }
}
