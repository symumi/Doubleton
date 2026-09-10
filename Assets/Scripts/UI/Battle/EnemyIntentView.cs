using BalartroLike.Battle;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BalartroLike.Unity
{
    // 敌人意图：图标、意图文本与预计伤害，必须独立可读，不能放进墨渍背景里。
    public sealed class EnemyIntentView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _text;
        [SerializeField] private TMP_Text _powerText;
        [SerializeField] private GameObject _root;

        public void Render(EnemyIntent intent)
        {
            bool hasIntent = intent != null;
            if (_root != null)
            {
                _root.SetActive(hasIntent);
            }

            if (!hasIntent)
            {
                return;
            }

            if (_icon != null)
            {
                _icon.sprite = UISpriteCatalog.Intent(intent.Type);
            }

            if (_text != null)
            {
                _text.text = intent.DisplayText;
            }

            if (_powerText != null)
            {
                _powerText.text = intent.Type == EnemyIntentType.Attack && intent.Power > 0
                    ? intent.Power.ToString()
                    : string.Empty;
            }
        }
    }
}
