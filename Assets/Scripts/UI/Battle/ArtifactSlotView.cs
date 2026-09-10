using BalartroLike.Battle;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BalartroLike.Unity
{
    // 单个法宝槽：法宝图标素材待补，当前以名称文本表示。
    public sealed class ArtifactSlotView : MonoBehaviour
    {
        [SerializeField] private Image _background;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private GameObject _emptyState;
        [SerializeField] private TooltipTrigger _tooltip;

        public string ArtifactId { get; private set; }

        public void Bind(ArtifactDefinition artifact)
        {
            bool hasArtifact = artifact != null;
            ArtifactId = hasArtifact ? artifact.Id : string.Empty;

            if (_background != null)
            {
                _background.sprite = UISpriteCatalog.Panel(hasArtifact
                    ? "common_panel_section_9s"
                    : "common_slot_empty_9s");
            }

            if (_nameText != null)
            {
                _nameText.text = hasArtifact ? artifact.DisplayName : string.Empty;
            }

            if (_emptyState != null)
            {
                _emptyState.SetActive(!hasArtifact);
            }

            if (_tooltip != null)
            {
                _tooltip.SetContent(
                    hasArtifact ? artifact.DisplayName : string.Empty,
                    hasArtifact ? BuildTip(artifact) : string.Empty);
            }
        }

        private static string BuildTip(ArtifactDefinition artifact)
        {
            string tip = string.IsNullOrEmpty(artifact.Description) ? string.Empty : artifact.Description + "\n";
            return tip + "触发：" + TriggerLabel(artifact.TriggerType);
        }

        private static string TriggerLabel(ArtifactTriggerType triggerType)
        {
            return triggerType == ArtifactTriggerType.AfterPlay ? "出卦后" : "伤害结算前";
        }
    }
}
