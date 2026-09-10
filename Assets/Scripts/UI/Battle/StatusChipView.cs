using BalartroLike.Battle;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BalartroLike.Unity
{
    // 单个状态图标：层数在右下、剩余回合在左下，具体位置由 Prefab 决定。
    public sealed class StatusChipView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _stacksText;
        [SerializeField] private TMP_Text _turnsText;
        [SerializeField] private TooltipTrigger _tooltip;

        public StatusId StatusId { get; private set; }

        public void Bind(StatusInstance status)
        {
            StatusId = status == null ? StatusId.None : status.Id;
            if (status == null)
            {
                return;
            }

            if (_icon != null)
            {
                _icon.sprite = UISpriteCatalog.Status(status.Id);
            }

            if (_stacksText != null)
            {
                _stacksText.text = status.Stacks > 0 ? status.Stacks.ToString() : string.Empty;
            }

            if (_turnsText != null)
            {
                _turnsText.text = status.RemainingTurns > 0 ? status.RemainingTurns + "回合" : string.Empty;
            }

            if (_tooltip != null)
            {
                _tooltip.SetContent(status.Id.ToString(), "层数 " + status.Stacks + "，剩余 " + status.RemainingTurns + " 回合");
            }
        }
    }
}
