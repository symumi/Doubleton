using System;
using BalartroLike.Battle;
using TMPro;
using UnityEngine;

namespace BalartroLike.Unity
{
    // 战斗 HUD：持有全部战斗子视图，负责选牌、出卦、符箓、结束回合与反馈播放期间的输入锁。
    // UI 只发命令和展示事件，不计算伤害；预览与实际结算共用 BattleController 的计算路径。
    public sealed class BattleHudView : UIPageView
    {
        [SerializeField] private HexagramAltarView _altar;
        [SerializeField] private HexagramPreviewView _hexagramPreview;
        [SerializeField] private DamagePreviewView _damagePreview;
        [SerializeField] private HandPanelView _handPanel;
        [SerializeField] private ActionPanelView _actionPanel;
        [SerializeField] private PlayerStatusView _playerStatus;
        [SerializeField] private EnemyView _enemyView;
        [SerializeField] private EnemyIntentView _enemyIntentView;
        [SerializeField] private ArtifactPanelView _artifactPanel;
        [SerializeField] private TalismanPanelView _talismanPanel;
        [SerializeField] private WeaponSlotView _weaponSlot;
        [SerializeField] private TMP_Text _turnText;
        [SerializeField] private BattleFeedbackView _feedbackView;

        private BattleController _controller;
        private int _innerUid;
        private int _outerUid;
        private bool _busy;
        private bool _feedbackReady;

        public BattleController Controller => _controller;
        public int InnerUid => _innerUid;
        public int OuterUid => _outerUid;
        public bool IsBusy => _busy;

        // 上层局内流程可注入符箓命令，用 RunController.UseTalisman 同步本局符箓列表。
        // 未注入时直接调用 BattleController.UseTalisman，此时本局列表不会同步。
        public Func<int, BattleCommandResult> TalismanCommand { get; set; }

        public void Bind(BattleController controller)
        {
            _controller = controller;
            _innerUid = 0;
            _outerUid = 0;
            _busy = false;
            if (_actionPanel != null)
            {
                _actionPanel.Bind(HandlePlay, HandleDiscard, HandleEndTurn);
            }

            Refresh();
        }

        public void Unbind()
        {
            if (_busy)
            {
                FinishFeedback(null);
            }

            _controller = null;
        }

        protected override void OnShow()
        {
            Refresh();
        }

        public void Refresh()
        {
            if (_controller == null)
            {
                return;
            }

            BattleState state = _controller.State;
            bool canAct = CanPlayerAct();

            // 出卦或弃牌后手牌变化，清掉已经不在手牌的选牌。
            if (_innerUid != 0 && state.FindHandCard(_innerUid) == null)
            {
                _innerUid = 0;
            }

            if (_outerUid != 0 && state.FindHandCard(_outerUid) == null)
            {
                _outerUid = 0;
            }

            CardInstance inner = _innerUid == 0 ? null : state.FindHandCard(_innerUid);
            CardInstance outer = _outerUid == 0 ? null : state.FindHandCard(_outerUid);
            BattleCalculation preview = BuildPreview();

            if (_turnText != null)
            {
                _turnText.text = "第 " + state.Turn + " 回合";
            }

            if (_altar != null)
            {
                _altar.Render(inner, outer);
            }

            if (_hexagramPreview != null)
            {
                if (preview == null)
                {
                    _hexagramPreview.Clear();
                }
                else
                {
                    _hexagramPreview.Render(preview, HexagramMastery.GetLevel(state.GetHexagramUseCount(preview.Hexagram.Id)));
                }
            }

            if (_damagePreview != null)
            {
                if (preview == null)
                {
                    _damagePreview.Clear();
                }
                else
                {
                    _damagePreview.Render(preview, state.Enemy);
                }
            }

            if (_handPanel != null)
            {
                _handPanel.Render(state.Hand, _innerUid, _outerUid, canAct, HandleCardClicked);
            }

            if (_playerStatus != null)
            {
                _playerStatus.Render(state.Player);
            }

            if (_enemyView != null)
            {
                _enemyView.Render(state.Enemy);
            }

            if (_enemyIntentView != null)
            {
                _enemyIntentView.Render(state.Enemy.CurrentIntent);
            }

            if (_artifactPanel != null)
            {
                _artifactPanel.Render(state.Artifacts);
            }

            if (_talismanPanel != null)
            {
                _talismanPanel.Render(state.Talismans, canAct, HandleTalisman);
            }

            if (_weaponSlot != null)
            {
                _weaponSlot.Render(state.Player.Weapon, WillReplaceOldest(state.Player.Weapon, preview));
            }

            if (_actionPanel != null)
            {
                bool canPlay = canAct && _innerUid != 0 && _outerUid != 0;
                _actionPanel.Render(canPlay, canAct, canAct, BuildHint(canAct, state, inner, outer));
                _actionPanel.SetPlayLabel("出卦");
                _actionPanel.SetDiscardLabel("弃牌 " + state.DiscardsRemaining + "/" + state.DiscardLimit);
            }
        }

        private void HandleCardClicked(BattleCardView card)
        {
            if (card == null || !CanPlayerAct())
            {
                return;
            }

            int uid = card.Uid;
            if (_innerUid == uid)
            {
                _innerUid = 0;
                Refresh();
                return;
            }

            if (_outerUid == uid)
            {
                _outerUid = 0;
                Refresh();
                return;
            }

            if (_innerUid == 0)
            {
                _innerUid = uid;
            }
            else if (_outerUid == 0)
            {
                _outerUid = uid;
            }
            else
            {
                // 内外卦都已选满时，重新从内卦开始选择。
                _innerUid = uid;
                _outerUid = 0;
            }

            Refresh();
        }

        private void HandlePlay()
        {
            if (!CanPlayerAct() || _innerUid == 0 || _outerUid == 0)
            {
                Toast("请选择内卦与外卦");
                return;
            }

            BattleState state = _controller.State;
            int enemyHpBefore = state.Enemy.Hp;
            int enemyShieldBefore = state.Enemy.Shield;
            BattleCommandResult result = _controller.PlayHexagram(_innerUid, _outerUid);
            _innerUid = 0;
            _outerUid = 0;

            if (!result.Success)
            {
                Toast(result.Error);
                Refresh();
                return;
            }

            int hpDamage = Math.Max(0, enemyHpBefore - state.Enemy.Hp);
            int shieldDamage = Math.Max(0, enemyShieldBefore - state.Enemy.Shield);
            string feedback = result.Calculation.Hexagram.DisplayName + "\n-" + hpDamage;
            if (shieldDamage > 0)
            {
                feedback += "\n护盾 -" + shieldDamage;
            }

            Color color = state.Result == BattleResultType.Victory
                ? new Color(1f, 0.72f, 0.28f, 1f)
                : new Color(1f, 0.88f, 0.55f, 1f);
            PlayFeedback(feedback, color, Refresh);
        }

        private void HandleEndTurn()
        {
            if (!CanPlayerAct())
            {
                return;
            }

            BattleState state = _controller.State;
            int playerHpBefore = state.Player.Hp;
            int playerShieldBefore = state.Player.Shield;
            int enemyHpBefore = state.Enemy.Hp;
            BattleCommandResult result = _controller.EndTurn();
            if (!result.Success)
            {
                Toast(result.Error);
                return;
            }

            int playerDamage = Math.Max(0, playerHpBefore - state.Player.Hp);
            int playerShieldDamage = Math.Max(0, playerShieldBefore - state.Player.Shield);
            int enemyDamage = Math.Max(0, enemyHpBefore - state.Enemy.Hp);
            string feedback = "回合结算";
            if (enemyDamage > 0)
            {
                feedback += "\n敌方 -" + enemyDamage;
            }

            if (playerDamage > 0)
            {
                feedback += "\n我方 -" + playerDamage;
            }

            if (playerShieldDamage > 0)
            {
                feedback += "\n护盾 -" + playerShieldDamage;
            }

            PlayFeedback(feedback, new Color(0.85f, 0.82f, 0.7f, 1f), Refresh);
        }

        private void HandleDiscard()
        {
            // TODO: 弃牌选择模式（DiscardSelectView）尚未接入，当前只提示；
            // 接入后应进入选择态并调用 BattleController.DiscardCards。
            Toast("弃牌模式待接入");
        }

        private void HandleTalisman(int index)
        {
            if (!CanPlayerAct() || index < 0 || index >= _controller.State.Talismans.Count)
            {
                return;
            }

            string talismanName = _controller.State.Talismans[index].DisplayName;
            BattleCommandResult result = TalismanCommand != null
                ? TalismanCommand(index)
                : _controller.UseTalisman(index);

            if (!result.Success)
            {
                Toast(result.Error);
                Refresh();
                return;
            }

            PlayFeedback("符箓\n" + talismanName, new Color(0.45f, 0.85f, 0.85f, 1f), Refresh);
        }

        private void PlayFeedback(string message, Color color, Action onComplete)
        {
            _busy = true;
            SetInputBlocked(true);
            Refresh();

            if (_feedbackView == null)
            {
                FinishFeedback(onComplete);
                return;
            }

            EnsureFeedbackReady();
            _feedbackView.Play(message, color, () => FinishFeedback(onComplete));
        }

        private void FinishFeedback(Action onComplete)
        {
            _busy = false;
            SetInputBlocked(false);
            onComplete?.Invoke();
        }

        private void EnsureFeedbackReady()
        {
            if (_feedbackReady || _feedbackView == null)
            {
                return;
            }

            _feedbackView.Initialize();
            _feedbackReady = true;
        }

        private void SetInputBlocked(bool blocked)
        {
            UIRoot root = UIRoot.Instance;
            if (root == null || root.InputBlocker == null)
            {
                return;
            }

            if (blocked)
            {
                root.InputBlocker.Push();
            }
            else
            {
                root.InputBlocker.Pop();
            }
        }

        private void Toast(string message)
        {
            UIRoot root = UIRoot.Instance;
            if (root != null)
            {
                root.Toast(message);
            }
        }

        private bool CanPlayerAct()
        {
            return _controller != null
                && !_busy
                && _controller.State.Phase == BattlePhase.PlayerAction
                && _controller.State.Result == BattleResultType.None;
        }

        private BattleCalculation BuildPreview()
        {
            if (_controller == null || _innerUid == 0 || _outerUid == 0)
            {
                return null;
            }

            BattleCommandResult result = _controller.PreviewPlay(_innerUid, _outerUid);
            return result.Success ? result.Calculation : null;
        }

        private static string BuildHint(bool canAct, BattleState state, CardInstance inner, CardInstance outer)
        {
            if (!canAct)
            {
                return state.Phase == BattlePhase.BattleEnd ? "战斗结束" : "结算中…";
            }

            if (state.Player.Energy <= 0)
            {
                return "灵力不足";
            }

            if (inner == null)
            {
                return "请选择内卦";
            }

            return outer == null ? "请选择外卦" : string.Empty;
        }

        private static bool WillReplaceOldest(WeaponState weapon, BattleCalculation calculation)
        {
            if (weapon == null || calculation == null || calculation.Effects == null)
            {
                return false;
            }

            if (weapon.Enchantments.Count < weapon.MaxEnchantSlots)
            {
                return false;
            }

            for (int i = 0; i < calculation.Effects.Count; i++)
            {
                if (calculation.Effects[i].Duration > 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
