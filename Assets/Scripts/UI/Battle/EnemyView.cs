using BalartroLike.Battle;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BalartroLike.Unity
{
    // 敌人状态：名字、类型标记、五行、气血、护盾与状态图标。
    public sealed class EnemyView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _kindText;
        [SerializeField] private Image _elementIcon;
        [SerializeField] private TMP_Text _elementText;
        [SerializeField] private UIProgressBar _hpBar;
        [SerializeField] private UIProgressBar _shieldBar;
        [SerializeField] private StatusBarView _statusBar;
        [SerializeField] private Image _portrait;

        public void Render(EnemyState enemy, bool animated = true)
        {
            if (enemy == null)
            {
                return;
            }

            if (_nameText != null)
            {
                _nameText.text = enemy.DisplayName;
            }

            if (_kindText != null)
            {
                _kindText.text = KindLabel(enemy.Kind);
            }

            if (_elementIcon != null)
            {
                _elementIcon.sprite = UISpriteCatalog.Element(enemy.Element);
            }

            if (_elementText != null)
            {
                _elementText.text = ElementLabel(enemy.Element);
            }

            if (_hpBar != null)
            {
                _hpBar.SetValue(enemy.Hp, enemy.MaxHp, animated);
            }

            if (_shieldBar != null)
            {
                // 护盾没有独立上限，按最大气血的比例显示。
                _shieldBar.SetValue(enemy.Shield, enemy.MaxHp, animated);
            }

            if (_statusBar != null)
            {
                _statusBar.Render(enemy.Statuses);
            }
        }

        public void SetPortrait(Sprite sprite)
        {
            if (_portrait != null)
            {
                _portrait.sprite = sprite;
            }
        }

        private static string KindLabel(EnemyKind kind)
        {
            switch (kind)
            {
                case EnemyKind.Elite:
                    return "精英";
                case EnemyKind.Boss:
                    return "渡劫";
                default:
                    return string.Empty;
            }
        }

        private static string ElementLabel(ElementType element)
        {
            switch (element)
            {
                case ElementType.Metal:
                    return "金";
                case ElementType.Wood:
                    return "木";
                case ElementType.Water:
                    return "水";
                case ElementType.Fire:
                    return "火";
                default:
                    return "土";
            }
        }
    }
}
