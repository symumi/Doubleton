using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BalartroLike.Unity
{
    // 行动区：出卦、弃牌换牌、结束回合，以及按钮不可用时的原因提示。
    public sealed class ActionPanelView : MonoBehaviour
    {
        [SerializeField] private Button _playButton;
        [SerializeField] private TMP_Text _playLabel;
        [SerializeField] private Button _discardButton;
        [SerializeField] private TMP_Text _discardLabel;
        [SerializeField] private Button _endTurnButton;
        [SerializeField] private TMP_Text _hintText;

        private Action _onPlay;
        private Action _onDiscard;
        private Action _onEndTurn;

        public void Bind(Action onPlay, Action onDiscard, Action onEndTurn)
        {
            _onPlay = onPlay;
            _onDiscard = onDiscard;
            _onEndTurn = onEndTurn;

            Hook(_playButton, HandlePlay);
            Hook(_discardButton, HandleDiscard);
            Hook(_endTurnButton, HandleEndTurn);
        }

        public void Render(bool canPlay, bool canDiscard, bool canEndTurn, string hint)
        {
            if (_playButton != null)
            {
                _playButton.interactable = canPlay;
            }

            if (_discardButton != null)
            {
                _discardButton.interactable = canDiscard;
            }

            if (_endTurnButton != null)
            {
                _endTurnButton.interactable = canEndTurn;
            }

            if (_hintText != null)
            {
                _hintText.text = hint ?? string.Empty;
            }
        }

        public void SetDiscardLabel(string text)
        {
            if (_discardLabel != null)
            {
                _discardLabel.text = text;
            }
        }

        public void SetPlayLabel(string text)
        {
            if (_playLabel != null)
            {
                _playLabel.text = text;
            }
        }

        private static void Hook(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveListener(action);
            button.onClick.AddListener(action);
        }

        private void HandlePlay()
        {
            _onPlay?.Invoke();
        }

        private void HandleDiscard()
        {
            _onDiscard?.Invoke();
        }

        private void HandleEndTurn()
        {
            _onEndTurn?.Invoke();
        }
    }
}
