using System;
using System.Text;
using BalartroLike.Battle;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BalartroLike.Unity
{
    // Prototype-only runtime UI. Replace with prefab-driven BattleView before production UI work.
    public sealed class BattlePrototypeBootstrap : MonoBehaviour
    {
        private static BattlePrototypeBootstrap _instance;

        private BattleController _controller;
        private Text _statusText;
        private Text _previewText;
        private Text _logText;
        private RectTransform _handRoot;
        private Button _playButton;
        private Button _endTurnButton;
        private int? _innerUid;
        private int? _outerUid;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoCreate()
        {
            if (FindObjectOfType<BattlePrototypeBootstrap>() != null)
            {
                return;
            }

            GameObject root = new GameObject("BattlePrototypeBootstrap");
            root.AddComponent<BattlePrototypeBootstrap>();
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            BuildUi();
            RestartBattle();
        }

        private void BuildUi()
        {
            GameObject canvasObject = new GameObject("BattlePrototypeCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            CreateEventSystem();
            CreateBackground(canvasObject.transform);
            _statusText = CreateText("Status", canvasObject.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -88f), new Vector2(-80f, 72f), 28, TextAnchor.MiddleCenter);
            _previewText = CreateText("Preview", canvasObject.transform, new Vector2(0.04f, 0.34f), new Vector2(0.72f, 0.86f), Vector2.zero, Vector2.zero, 26, TextAnchor.MiddleLeft);
            _previewText.alignment = TextAnchor.UpperLeft;
            _logText = CreateText("Log", canvasObject.transform, new Vector2(0.74f, 0.30f), new Vector2(0.96f, 0.54f), Vector2.zero, Vector2.zero, 18, TextAnchor.UpperLeft);
            _logText.color = new Color(0.75f, 0.72f, 0.64f, 1f);

            _handRoot = CreatePanel("Hand", canvasObject.transform, new Vector2(0.04f, 0.04f), new Vector2(0.96f, 0.28f));
            HorizontalLayoutGroup layout = _handRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            _playButton = CreateButton("Play", canvasObject.transform, "出卦", new Vector2(0.75f, 0.70f), new Vector2(0.96f, 0.80f));
            _playButton.onClick.AddListener(OnPlayClicked);
            _endTurnButton = CreateButton("EndTurn", canvasObject.transform, "回合结束", new Vector2(0.75f, 0.58f), new Vector2(0.96f, 0.68f));
            _endTurnButton.onClick.AddListener(OnEndTurnClicked);
            Button restartButton = CreateButton("Restart", canvasObject.transform, "重新开始", new Vector2(0.75f, 0.46f), new Vector2(0.96f, 0.56f));
            restartButton.onClick.AddListener(OnRestartClicked);
        }

        private static void CreateEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            DontDestroyOnLoad(eventSystem);
        }

        private static void CreateBackground(Transform parent)
        {
            RectTransform rect = CreatePanel("Background", parent, Vector2.zero, Vector2.one);
            rect.SetAsFirstSibling();
            rect.GetComponent<Image>().color = new Color(0.055f, 0.045f, 0.035f, 1f);
        }

        private void RestartBattle()
        {
            _controller = BattleController.CreatePrototype(Environment.TickCount);
            _innerUid = null;
            _outerUid = null;
            Refresh();
        }

        private void OnCardClicked(int cardUid)
        {
            if (_innerUid == null)
            {
                _innerUid = cardUid;
            }
            else if (_outerUid == null && _innerUid.Value != cardUid)
            {
                _outerUid = cardUid;
            }
            else
            {
                _innerUid = cardUid;
                _outerUid = null;
            }

            Refresh();
        }

        private void OnPlayClicked()
        {
            if (_innerUid == null || _outerUid == null)
            {
                return;
            }

            BattleCommandResult result = _controller.PlayHexagram(_innerUid.Value, _outerUid.Value);
            if (!result.Success)
            {
                Debug.LogWarning(result.Error);
            }

            _innerUid = null;
            _outerUid = null;
            Refresh();
        }

        private void OnEndTurnClicked()
        {
            _controller.EndTurn();
            _innerUid = null;
            _outerUid = null;
            Refresh();
        }

        private void OnRestartClicked()
        {
            RestartBattle();
        }

        private void Refresh()
        {
            BattleState state = _controller.State;
            _statusText.text = "第 " + state.Turn + " 回合   玩家 HP " + state.Player.Hp + "/" + state.Player.MaxHp
                + "   护盾 " + state.Player.Shield + "   灵力 " + state.Player.Energy + "/" + state.Player.MaxEnergy
                + "   |   敌人 " + state.Enemy.DisplayName + " HP " + state.Enemy.Hp + "/" + state.Enemy.MaxHp
                + "   护盾 " + state.Enemy.Shield + "   意图 " + (state.Enemy.CurrentIntent == null ? "未知" : state.Enemy.CurrentIntent.DisplayText);
            _previewText.text = BuildPreviewText();
            _logText.text = BuildLogText();
            _playButton.interactable = _innerUid != null && _outerUid != null && state.Phase == BattlePhase.PlayerAction;
            _endTurnButton.interactable = state.Phase == BattlePhase.PlayerAction;
            RebuildHand();
        }

        private string BuildPreviewText()
        {
            if (_innerUid == null)
            {
                return "先选择一张手牌作为【内卦】";
            }

            if (_outerUid == null)
            {
                return "再选择一张手牌作为【外卦】";
            }

            BattleCommandResult preview = _controller.PreviewPlay(_innerUid.Value, _outerUid.Value);
            if (!preview.Success)
            {
                return preview.Error;
            }

            BattleCalculation calculation = preview.Calculation;
            return calculation.Hexagram.DisplayName
                + "\n上卦 " + TrigramCatalog.Get(calculation.Hexagram.Outer).DisplayName
                + " / 下卦 " + TrigramCatalog.Get(calculation.Hexagram.Inner).DisplayName
                + "\n伤害：" + calculation.Damage.FinalDamage + "   连击 " + calculation.HitCount
                + "\n" + calculation.Damage.ToFormula();
        }

        private string BuildLogText()
        {
            StringBuilder builder = new StringBuilder();
            int start = Mathf.Max(0, _controller.State.Events.Count - 8);
            for (int i = start; i < _controller.State.Events.Count; i++)
            {
                builder.AppendLine(_controller.State.Events[i].Message);
            }

            return builder.ToString();
        }

        private void RebuildHand()
        {
            for (int i = _handRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(_handRoot.GetChild(i).gameObject);
            }

            for (int i = 0; i < _controller.State.Hand.Count; i++)
            {
                CardInstance card = _controller.State.Hand[i];
                Button button = CreateButton("Card_" + card.Uid, _handRoot, BuildCardText(card), Vector2.zero, Vector2.one);
                int uid = card.Uid;
                button.onClick.AddListener(delegate { OnCardClicked(uid); });
                Image image = button.GetComponent<Image>();
                if (_innerUid == card.Uid)
                {
                    image.color = new Color(0.78f, 0.62f, 0.30f, 1f);
                }
                else if (_outerUid == card.Uid)
                {
                    image.color = new Color(0.32f, 0.64f, 0.62f, 1f);
                }
                else
                {
                    image.color = new Color(0.13f, 0.10f, 0.07f, 1f);
                }
            }
        }

        private static string BuildCardText(CardInstance card)
        {
            TrigramDefinition trigram = TrigramCatalog.Get(card.Trigram);
            return trigram.Symbol + " " + trigram.DisplayName + "\n" + card.Rank + "★\n卦力 " + card.Qi;
        }

        private static RectTransform CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = new Color(0.12f, 0.09f, 0.06f, 0.95f);
            return rect;
        }

        private static Text CreateText(
            string name,
            Transform parent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta,
            int fontSize,
            TextAnchor alignment)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
            Text text = go.GetComponent<Text>();
            text.font = Font.CreateDynamicFontFromOSFont("Microsoft YaHei", fontSize);
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = new Color(0.93f, 0.89f, 0.81f, 1f);
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static Button CreateButton(string name, Transform parent, string label, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Image image = go.GetComponent<Image>();
            image.color = new Color(0.13f, 0.10f, 0.07f, 1f);
            Button button = go.GetComponent<Button>();
            button.targetGraphic = image;

            Text text = CreateText("Label", go.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, 22, TextAnchor.MiddleCenter);
            text.text = label;
            return button;
        }
    }
}