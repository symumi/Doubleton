using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BalartroLike.Battle;
using BalartroLike.Run;
using BalartroLike.Save;
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
        private RunController _runController;
        private readonly RunMetaProgressState _metaProgress = new RunMetaProgressState();
        private Text _statusText;
        private Text _previewText;
        private Text _logText;
        private BattleFeedbackView _battleFeedbackView;
        private bool _battleInputLocked;
        private RectTransform _handRoot;
        private RectTransform _talismanRoot;
        private Button _playButton;
        private Button _endTurnButton;
        private Button _continueButton;
        private Button _restartButton;
        private RectTransform _routeRoot;
        private Text _routeTitleText;
        private Text _routeStatusText;
        private Text _routeDetailText;
        private Button _startBattleButton;
        private Button _weaponButton;
        private RectTransform _battleResultRoot;
        private Text _battleResultTitleText;
        private Text _battleResultDetailText;
        private Button _confirmBattleResultButton;
        private RectTransform _shopRoot;
        private Text _shopTitleText;
        private Text _shopStatusText;
        private Button _shopRefreshButton;
        private Button _shopLeaveButton;
        private RectTransform _eventRoot;
        private Text _eventTitleText;
        private Text _eventDescriptionText;
        private Button _eventLeaveButton;
        private RectTransform _promotionRoot;
        private Text _promotionTitleText;
        private Text _promotionDetailText;
        private Button _promotionConfirmButton;
        private RectTransform _runResultRoot;
        private Text _runResultTitleText;
        private Text _runResultDetailText;
        private Button _runResultConfirmButton;
        private RectTransform _mainMenuRoot;
        private Text _mainMenuTitleText;
        private Text _mainMenuStatusText;
        private Button _continueGameButton;
        private Button _newGameButton;
        private RectTransform _metaProgressRoot;
        private Text _metaProgressTitleText;
        private Text _metaProgressDetailText;
        private Button _metaProgressRestartButton;
        private Button _metaProgressPrevButton;
        private Button _metaProgressNextButton;
        private RectTransform _codexRoot;
        private Text _codexTitleText;
        private Text _codexStatusText;
        private Text _codexDetailText;
        private RectTransform _codexGridRoot;
        private Button _codexBackButton;
        private Button _mainMenuCodexButton;
        private HexagramDefinition _selectedHexagram;
        private readonly List<Button> _codexCellButtons = new List<Button>();
        private readonly List<Text> _codexCellLabels = new List<Text>();
        private bool _hasRunSave;
        private bool _confirmingNewGame;
        private int? _innerUid;
        private int? _outerUid;
        private readonly List<Button> _cardButtons = new List<Button>();
        private readonly List<Text> _cardLabels = new List<Text>();
        private readonly List<int> _cardUids = new List<int>();
        private readonly List<Button> _talismanButtons = new List<Button>();
        private readonly List<Text> _talismanLabels = new List<Text>();
        private readonly List<Button> _routeNodeButtons = new List<Button>();
        private readonly List<Text> _routeNodeLabels = new List<Text>();
        private readonly List<Button> _shopOfferButtons = new List<Button>();
        private readonly List<Text> _shopOfferLabels = new List<Text>();
        private readonly List<Button> _eventOptionButtons = new List<Button>();
        private readonly List<Text> _eventOptionLabels = new List<Text>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoCreate()
        {
            //if (FindObjectOfType<BattlePrototypeBootstrap>() != null)
            //{
            //    return;
            //}

            //GameObject root = new GameObject("BattlePrototypeBootstrap");
            //root.AddComponent<BattlePrototypeBootstrap>();
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            ResourcesBattleConfigLoader.LoadIfNeeded();
            ResourcesRunConfigLoader.LoadIfNeeded();
            BuildUi();
            ShowMainMenu();
        }

        private void BuildUi()
        {
            CreateEventSystem();
            GameObject canvasObject = new GameObject("BattlePrototypeCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            CreateBackground(canvasObject.transform);
            _statusText = CreateText("Status", canvasObject.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -88f), new Vector2(-80f, 72f), 28, TextAnchor.MiddleCenter);
            _previewText = CreateText("Preview", canvasObject.transform, new Vector2(0.04f, 0.34f), new Vector2(0.72f, 0.86f), Vector2.zero, Vector2.zero, 26, TextAnchor.MiddleLeft);
            _previewText.alignment = TextAnchor.UpperLeft;
            _logText = CreateText("Log", canvasObject.transform, new Vector2(0.74f, 0.30f), new Vector2(0.96f, 0.54f), Vector2.zero, Vector2.zero, 18, TextAnchor.UpperLeft);
            _logText.color = new Color(0.75f, 0.72f, 0.64f, 1f);
            Text battleFeedbackText = CreateText("BattleFeedback", canvasObject.transform, new Vector2(0.34f, 0.56f), new Vector2(0.66f, 0.72f), Vector2.zero, Vector2.zero, 46, TextAnchor.MiddleCenter);
            battleFeedbackText.fontStyle = FontStyle.Bold;
            battleFeedbackText.raycastTarget = false;
            _battleFeedbackView = battleFeedbackText.gameObject.AddComponent<BattleFeedbackView>();
            _battleFeedbackView.Initialize();

            _handRoot = CreatePanel("Hand", canvasObject.transform, new Vector2(0.04f, 0.04f), new Vector2(0.96f, 0.28f));
            HorizontalLayoutGroup layout = _handRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            BuildTalismanUi(canvasObject.transform);

            _playButton = CreateButton("Play", canvasObject.transform, "出卦", new Vector2(0.75f, 0.70f), new Vector2(0.96f, 0.80f));
            _playButton.onClick.AddListener(OnPlayClicked);
            _endTurnButton = CreateButton("EndTurn", canvasObject.transform, "回合结束", new Vector2(0.75f, 0.58f), new Vector2(0.96f, 0.68f));
            _endTurnButton.onClick.AddListener(OnEndTurnClicked);
            _continueButton = CreateButton("Continue", canvasObject.transform, "继续", new Vector2(0.75f, 0.34f), new Vector2(0.96f, 0.44f));
            _continueButton.onClick.AddListener(OnContinueClicked);
            _restartButton = CreateButton("Restart", canvasObject.transform, "重新开始", new Vector2(0.75f, 0.46f), new Vector2(0.96f, 0.56f));
            _restartButton.onClick.AddListener(OnRestartClicked);
            BuildRouteUi(canvasObject.transform);
            BuildBattleResultUi(canvasObject.transform);
            BuildShopUi(canvasObject.transform);
            BuildEventUi(canvasObject.transform);
            BuildRealmPromotionUi(canvasObject.transform);
            BuildRunResultUi(canvasObject.transform);
            BuildMetaProgressUi(canvasObject.transform);
            BuildCodexUi(canvasObject.transform);
            BuildMainMenuUi(canvasObject.transform);
        }

        private void BuildTalismanUi(Transform parent)
        {
            _talismanRoot = CreatePanel("Talismans", parent, new Vector2(0.04f, 0.29f), new Vector2(0.72f, 0.335f));
            HorizontalLayoutGroup layout = _talismanRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
        }

        private void BuildRouteUi(Transform parent)
        {
            _routeRoot = CreatePanel("Route", parent, Vector2.zero, Vector2.one);
            _routeRoot.GetComponent<Image>().color = new Color(0.055f, 0.045f, 0.035f, 1f);
            _routeTitleText = CreateText("RouteTitle", _routeRoot, new Vector2(0.06f, 0.88f), new Vector2(0.94f, 0.97f), Vector2.zero, Vector2.zero, 42, TextAnchor.MiddleLeft);
            _routeStatusText = CreateText("RouteStatus", _routeRoot, new Vector2(0.06f, 0.80f), new Vector2(0.94f, 0.88f), Vector2.zero, Vector2.zero, 22, TextAnchor.MiddleLeft);
            _routeStatusText.color = new Color(0.75f, 0.72f, 0.64f, 1f);

            RectTransform nodeRoot = CreatePanel("RouteNodes", _routeRoot, new Vector2(0.06f, 0.16f), new Vector2(0.64f, 0.78f));
            GridLayoutGroup layout = nodeRoot.gameObject.AddComponent<GridLayoutGroup>();
            layout.padding = new RectOffset(18, 18, 18, 18);
            layout.spacing = new Vector2(18f, 18f);
            layout.cellSize = new Vector2(300f, 150f);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 3;

            for (int i = 0; i < RunConfigDatabase.Nodes.Count; i++)
            {
                Button nodeButton = CreateButton("RouteNode_" + i, nodeRoot, string.Empty, Vector2.zero, Vector2.one);
                nodeButton.interactable = false;
                nodeButton.transition = Selectable.Transition.None;
                Text label = nodeButton.GetComponentInChildren<Text>();
                label.fontSize = 20;
                _routeNodeButtons.Add(nodeButton);
                _routeNodeLabels.Add(label);
            }

            _routeDetailText = CreateText("RouteDetail", _routeRoot, new Vector2(0.67f, 0.28f), new Vector2(0.94f, 0.78f), Vector2.zero, Vector2.zero, 24, TextAnchor.UpperLeft);
            _weaponButton = CreateButton("SelectWeapon", _routeRoot, "切换武器", new Vector2(0.70f, 0.27f), new Vector2(0.91f, 0.36f));
            _weaponButton.onClick.AddListener(OnWeaponClicked);
            _startBattleButton = CreateButton("StartBattle", _routeRoot, "开始战斗", new Vector2(0.70f, 0.16f), new Vector2(0.91f, 0.25f));
            _startBattleButton.onClick.AddListener(OnStartBattleClicked);
            Button routeRestartButton = CreateButton("RouteRestart", _routeRoot, "重新开始", new Vector2(0.70f, 0.05f), new Vector2(0.91f, 0.14f));
            routeRestartButton.onClick.AddListener(OnRestartClicked);
        }

        private void BuildBattleResultUi(Transform parent)
        {
            _battleResultRoot = CreatePanel("BattleResult", parent, Vector2.zero, Vector2.one);
            _battleResultRoot.GetComponent<Image>().color = new Color(0.055f, 0.045f, 0.035f, 1f);
            _battleResultTitleText = CreateText("BattleResultTitle", _battleResultRoot, new Vector2(0.18f, 0.62f), new Vector2(0.82f, 0.82f), Vector2.zero, Vector2.zero, 52, TextAnchor.MiddleCenter);
            _battleResultDetailText = CreateText("BattleResultDetail", _battleResultRoot, new Vector2(0.24f, 0.36f), new Vector2(0.76f, 0.62f), Vector2.zero, Vector2.zero, 28, TextAnchor.MiddleCenter);
            _confirmBattleResultButton = CreateButton("ConfirmBattleResult", _battleResultRoot, "领取奖励", new Vector2(0.38f, 0.22f), new Vector2(0.62f, 0.32f));
            _confirmBattleResultButton.onClick.AddListener(OnConfirmBattleResultClicked);
            _battleResultRoot.gameObject.SetActive(false);
        }

        private void BuildShopUi(Transform parent)
        {
            _shopRoot = CreatePanel("Shop", parent, Vector2.zero, Vector2.one);
            _shopRoot.GetComponent<Image>().color = new Color(0.055f, 0.045f, 0.035f, 1f);
            _shopTitleText = CreateText("ShopTitle", _shopRoot, new Vector2(0.08f, 0.86f), new Vector2(0.92f, 0.96f), Vector2.zero, Vector2.zero, 42, TextAnchor.MiddleLeft);
            _shopStatusText = CreateText("ShopStatus", _shopRoot, new Vector2(0.08f, 0.79f), new Vector2(0.92f, 0.86f), Vector2.zero, Vector2.zero, 22, TextAnchor.MiddleLeft);
            _shopStatusText.color = new Color(0.75f, 0.72f, 0.64f, 1f);

            RectTransform offerRoot = CreatePanel("ShopOffers", _shopRoot, new Vector2(0.10f, 0.24f), new Vector2(0.90f, 0.77f));
            VerticalLayoutGroup layout = offerRoot.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 14f;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            for (int i = 0; i < 4; i++)
            {
                int capturedIndex = i;
                Button button = CreateButton("ShopOffer_" + i, offerRoot, string.Empty, Vector2.zero, Vector2.one);
                button.onClick.AddListener(delegate { OnShopOfferClicked(capturedIndex); });
                _shopOfferButtons.Add(button);
                _shopOfferLabels.Add(button.GetComponentInChildren<Text>());
            }

            _shopRefreshButton = CreateButton("ShopRefresh", _shopRoot, "刷新货架（20 灵石）", new Vector2(0.24f, 0.12f), new Vector2(0.48f, 0.21f));
            _shopRefreshButton.onClick.AddListener(OnShopRefreshClicked);
            _shopLeaveButton = CreateButton("ShopLeave", _shopRoot, "离开坊市", new Vector2(0.52f, 0.12f), new Vector2(0.76f, 0.21f));
            _shopLeaveButton.onClick.AddListener(OnShopLeaveClicked);
            _shopRoot.gameObject.SetActive(false);
        }

        private void BuildEventUi(Transform parent)
        {
            _eventRoot = CreatePanel("Event", parent, Vector2.zero, Vector2.one);
            _eventRoot.GetComponent<Image>().color = new Color(0.055f, 0.045f, 0.035f, 1f);
            _eventTitleText = CreateText("EventTitle", _eventRoot, new Vector2(0.10f, 0.78f), new Vector2(0.90f, 0.92f), Vector2.zero, Vector2.zero, 42, TextAnchor.MiddleCenter);
            _eventDescriptionText = CreateText("EventDescription", _eventRoot, new Vector2(0.14f, 0.52f), new Vector2(0.86f, 0.76f), Vector2.zero, Vector2.zero, 26, TextAnchor.MiddleCenter);
            _eventDescriptionText.color = new Color(0.80f, 0.76f, 0.68f, 1f);

            RectTransform optionRoot = CreatePanel("EventOptions", _eventRoot, new Vector2(0.20f, 0.24f), new Vector2(0.80f, 0.50f));
            VerticalLayoutGroup layout = optionRoot.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 12f;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            for (int i = 0; i < 3; i++)
            {
                int capturedIndex = i;
                Button button = CreateButton("EventOption_" + i, optionRoot, string.Empty, Vector2.zero, Vector2.one);
                button.onClick.AddListener(delegate { OnEventOptionClicked(capturedIndex); });
                _eventOptionButtons.Add(button);
                _eventOptionLabels.Add(button.GetComponentInChildren<Text>());
            }

            _eventLeaveButton = CreateButton("EventLeave", _eventRoot, "返回路线", new Vector2(0.38f, 0.12f), new Vector2(0.62f, 0.21f));
            _eventLeaveButton.onClick.AddListener(OnEventLeaveClicked);
            _eventLeaveButton.gameObject.SetActive(false);
            _eventRoot.gameObject.SetActive(false);
        }

        private void BuildRealmPromotionUi(Transform parent)
        {
            _promotionRoot = CreatePanel("RealmPromotion", parent, Vector2.zero, Vector2.one);
            _promotionRoot.GetComponent<Image>().color = new Color(0.055f, 0.045f, 0.035f, 1f);
            _promotionTitleText = CreateText("RealmPromotionTitle", _promotionRoot, new Vector2(0.12f, 0.62f), new Vector2(0.88f, 0.84f), Vector2.zero, Vector2.zero, 52, TextAnchor.MiddleCenter);
            _promotionDetailText = CreateText("RealmPromotionDetail", _promotionRoot, new Vector2(0.20f, 0.36f), new Vector2(0.80f, 0.62f), Vector2.zero, Vector2.zero, 28, TextAnchor.MiddleCenter);
            _promotionConfirmButton = CreateButton("RealmPromotionConfirm", _promotionRoot, "确认晋升", new Vector2(0.38f, 0.22f), new Vector2(0.62f, 0.32f));
            _promotionConfirmButton.onClick.AddListener(OnRealmPromotionConfirmClicked);
            _promotionRoot.gameObject.SetActive(false);
        }

        private void BuildRunResultUi(Transform parent)
        {
            _runResultRoot = CreatePanel("RunResult", parent, Vector2.zero, Vector2.one);
            _runResultRoot.GetComponent<Image>().color = new Color(0.055f, 0.045f, 0.035f, 1f);
            _runResultTitleText = CreateText("RunResultTitle", _runResultRoot, new Vector2(0.12f, 0.62f), new Vector2(0.88f, 0.84f), Vector2.zero, Vector2.zero, 52, TextAnchor.MiddleCenter);
            _runResultDetailText = CreateText("RunResultDetail", _runResultRoot, new Vector2(0.20f, 0.32f), new Vector2(0.80f, 0.62f), Vector2.zero, Vector2.zero, 28, TextAnchor.MiddleCenter);
            _runResultConfirmButton = CreateButton("RunResultConfirm", _runResultRoot, "进入局外成长", new Vector2(0.36f, 0.18f), new Vector2(0.64f, 0.28f));
            _runResultConfirmButton.onClick.AddListener(OnRunResultConfirmClicked);
            _runResultRoot.gameObject.SetActive(false);
        }

        private void BuildMetaProgressUi(Transform parent)
        {
            _metaProgressRoot = CreatePanel("MetaProgress", parent, Vector2.zero, Vector2.one);
            _metaProgressRoot.GetComponent<Image>().color = new Color(0.055f, 0.045f, 0.035f, 1f);
            _metaProgressTitleText = CreateText("MetaProgressTitle", _metaProgressRoot, new Vector2(0.12f, 0.66f), new Vector2(0.88f, 0.84f), Vector2.zero, Vector2.zero, 48, TextAnchor.MiddleCenter);
            _metaProgressDetailText = CreateText("MetaProgressDetail", _metaProgressRoot, new Vector2(0.20f, 0.30f), new Vector2(0.80f, 0.64f), Vector2.zero, Vector2.zero, 28, TextAnchor.MiddleCenter);
            _metaProgressPrevButton = CreateButton("MetaProgressPrev", _metaProgressRoot, "上一档", new Vector2(0.20f, 0.17f), new Vector2(0.34f, 0.27f));
            _metaProgressRestartButton = CreateButton("MetaProgressRestart", _metaProgressRoot, "重新开始", new Vector2(0.38f, 0.17f), new Vector2(0.62f, 0.27f));
            _metaProgressNextButton = CreateButton("MetaProgressNext", _metaProgressRoot, "下一档", new Vector2(0.66f, 0.17f), new Vector2(0.80f, 0.27f));
            _metaProgressPrevButton.onClick.AddListener(OnMetaProgressPrevClicked);
            _metaProgressRestartButton.onClick.AddListener(OnMetaProgressRestartClicked);
            _metaProgressNextButton.onClick.AddListener(OnMetaProgressNextClicked);
            _metaProgressRoot.gameObject.SetActive(false);
        }

        private void BuildCodexUi(Transform parent)
        {
            _codexRoot = CreatePanel("Codex", parent, Vector2.zero, Vector2.one);
            _codexRoot.GetComponent<Image>().color = new Color(0.055f, 0.045f, 0.035f, 1f);
            _codexTitleText = CreateText("CodexTitle", _codexRoot, new Vector2(0.04f, 0.88f), new Vector2(0.66f, 0.97f), Vector2.zero, Vector2.zero, 42, TextAnchor.MiddleLeft);
            _codexTitleText.text = "卦象图鉴";
            _codexStatusText = CreateText("CodexStatus", _codexRoot, new Vector2(0.04f, 0.80f), new Vector2(0.66f, 0.88f), Vector2.zero, Vector2.zero, 22, TextAnchor.MiddleLeft);
            _codexStatusText.color = new Color(0.75f, 0.72f, 0.64f, 1f);

            _codexGridRoot = CreatePanel("CodexGrid", _codexRoot, new Vector2(0.04f, 0.08f), new Vector2(0.66f, 0.79f));
            GridLayoutGroup layout = _codexGridRoot.gameObject.AddComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(126f, 78f);
            layout.spacing = new Vector2(5f, 5f);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 8;
            layout.childAlignment = TextAnchor.UpperLeft;

            for (int i = 0; i < 64; i++)
            {
                int index = i;
                Button button = CreateButton("CodexCell_" + i, _codexGridRoot, string.Empty, Vector2.zero, Vector2.zero);
                button.GetComponent<RectTransform>().sizeDelta = layout.cellSize;
                button.onClick.AddListener(() => OnCodexCellClicked(index));
                Text label = button.GetComponentInChildren<Text>();
                label.fontSize = 16;
                label.resizeTextForBestFit = true;
                label.resizeTextMinSize = 10;
                label.resizeTextMaxSize = 16;
                _codexCellButtons.Add(button);
                _codexCellLabels.Add(label);
            }

            _codexDetailText = CreateText("CodexDetail", _codexRoot, new Vector2(0.70f, 0.08f), new Vector2(0.96f, 0.79f), Vector2.zero, Vector2.zero, 19, TextAnchor.UpperLeft);
            _codexDetailText.color = new Color(0.93f, 0.89f, 0.81f, 1f);
            _codexBackButton = CreateButton("CodexBack", _codexRoot, "返回主界面", new Vector2(0.77f, 0.02f), new Vector2(0.93f, 0.075f));
            _codexBackButton.onClick.AddListener(OnCodexBackClicked);
            _codexRoot.gameObject.SetActive(false);
        }

        private void OnMainMenuCodexClicked()
        {
            HideAllRoots();
            _codexRoot.gameObject.SetActive(true);
            _selectedHexagram = null;
            RefreshCodex();
        }

        private void OnCodexBackClicked()
        {
            ShowMainMenu();
        }

        private void OnCodexCellClicked(int index)
        {
            TrigramId outer = (TrigramId)(index / 8);
            TrigramId inner = (TrigramId)(index % 8);
            _selectedHexagram = HexagramCatalog.Get(outer, inner);
            RefreshCodexDetail();
        }

        private void RefreshCodex()
        {
            SetBattleVisible(false);
            _mainMenuRoot.gameObject.SetActive(false);
            _codexRoot.gameObject.SetActive(true);

            int discovered = 0;
            int mastered = 0;
            for (int i = 0; i < _codexCellButtons.Count; i++)
            {
                HexagramDefinition hexagram = HexagramCatalog.Get((TrigramId)(i / 8), (TrigramId)(i % 8));
                int useCount = _metaProgress.HexagramUses.TryGetValue(hexagram.Id, out int count) ? count : 0;
                bool known = useCount > 0;
                if (known)
                {
                    discovered++;
                }

                if (HexagramMastery.IsMastered(useCount))
                {
                    mastered++;
                }

                _codexCellLabels[i].text = known
                    ? hexagram.DisplayName + "\n" + BuildMasteryText(useCount)
                    : "？？？\n○ ○ ○ ○ ○";
                Image image = _codexCellButtons[i].GetComponent<Image>();
                image.color = known
                    ? hexagram.IsSpecial
                        ? new Color(0.30f, 0.20f, 0.10f, 1f)
                        : new Color(0.13f, 0.16f, 0.13f, 1f)
                    : new Color(0.09f, 0.08f, 0.07f, 1f);
                _codexCellLabels[i].color = known
                    ? new Color(0.93f, 0.89f, 0.81f, 1f)
                    : new Color(0.42f, 0.40f, 0.36f, 1f);
            }

            _codexStatusText.text = "已发现 " + discovered + "/64   精通 " + mastered + "   点击卦格查看详情";
            RefreshCodexDetail();
        }

        private void RefreshCodexDetail()
        {
            if (_selectedHexagram == null)
            {
                _codexDetailText.text = "点击左侧卦格查看卦象详情。\n\n已发现的卦会显示效果、五行与熟练度。";
                return;
            }

            int useCount = _metaProgress.HexagramUses.TryGetValue(_selectedHexagram.Id, out int count) ? count : 0;
            if (useCount == 0)
            {
                _codexDetailText.text = "？？？\n\n尚未参悟此卦。\n使用对应内外卦组合后可解锁图鉴。";
                return;
            }

            TrigramDefinition inner = TrigramCatalog.Get(_selectedHexagram.Inner);
            TrigramDefinition outer = TrigramCatalog.Get(_selectedHexagram.Outer);
            _codexDetailText.text = _selectedHexagram.DisplayName + "  " + _selectedHexagram.Id
                + "\n上卦 " + outer.DisplayName + " " + outer.Symbol
                + " / 下卦 " + inner.DisplayName + " " + inner.Symbol
                + "\n五行 " + GetElementDisplayName(inner.Element)
                + "\n类型 " + (_selectedHexagram.IsSpecial ? "特殊卦" : "模板卦")
                + "\n熟练度 " + HexagramMastery.GetLevel(useCount) + "/5"
                + (HexagramMastery.IsMastered(useCount) ? " · 精通伤害 ×1.20" : string.Empty)
                + "\n使用次数 " + useCount
                + "\n\n" + BuildHexagramDescription(_selectedHexagram)
                + "\n\n" + BuildHexagramEffectText(_selectedHexagram);
        }

        private static string BuildMasteryText(int useCount)
        {
            int level = HexagramMastery.GetLevel(useCount);
            StringBuilder builder = new StringBuilder();
            for (int i = 1; i <= HexagramMastery.MasteredLevel; i++)
            {
                if (i > 1)
                {
                    builder.Append(' ');
                }

                builder.Append(level >= i ? '●' : '○');
            }

            return builder.ToString();
        }

        private static string BuildHexagramDescription(HexagramDefinition hexagram)
        {
            if (!string.IsNullOrWhiteSpace(hexagram.Description))
            {
                return hexagram.Description;
            }

            return "内卦行为：" + TrigramCatalog.Get(hexagram.Inner).DisplayName
                + "；外卦修饰：" + TrigramCatalog.Get(hexagram.Outer).DisplayName;
        }

        private static string BuildHexagramEffectText(HexagramDefinition hexagram)
        {
            StringBuilder builder = new StringBuilder("效果：");
            if (hexagram.DamageMultiplierOverride.HasValue)
            {
                builder.Append("卦倍率 ×").Append((hexagram.DamageMultiplierOverride.Value / 10000f).ToString("0.00")).Append("；");
            }

            if (hexagram.GuaranteedCriticalOverride == true)
            {
                builder.Append("必定会心；");
            }

            if (hexagram.AdditionalEffects.Length == 0)
            {
                builder.Append("沿用内卦与外卦模板。");
                return builder.ToString();
            }

            for (int i = 0; i < hexagram.AdditionalEffects.Length; i++)
            {
                if (i > 0)
                {
                    builder.Append("；");
                }

                builder.Append(GetEffectDisplayName(hexagram.AdditionalEffects[i]));
            }

            return builder.ToString();
        }

        private void BuildMainMenuUi(Transform parent)
        {
            _mainMenuRoot = CreatePanel("MainMenu", parent, Vector2.zero, Vector2.one);
            _mainMenuRoot.GetComponent<Image>().color = new Color(0.055f, 0.045f, 0.035f, 1f);
            _mainMenuTitleText = CreateText("MainMenuTitle", _mainMenuRoot, new Vector2(0.18f, 0.68f), new Vector2(0.82f, 0.84f), Vector2.zero, Vector2.zero, 58, TextAnchor.MiddleCenter);
            _mainMenuTitleText.text = "卦锻 · 修行历劫";
            _mainMenuStatusText = CreateText("MainMenuStatus", _mainMenuRoot, new Vector2(0.18f, 0.57f), new Vector2(0.82f, 0.67f), Vector2.zero, Vector2.zero, 24, TextAnchor.MiddleCenter);
            _mainMenuStatusText.color = new Color(0.75f, 0.72f, 0.64f, 1f);
            _continueGameButton = CreateButton("ContinueGame", _mainMenuRoot, "继续游戏", new Vector2(0.34f, 0.45f), new Vector2(0.66f, 0.54f));
            _continueGameButton.onClick.AddListener(OnContinueGameClicked);
            _newGameButton = CreateButton("NewGame", _mainMenuRoot, "开始修行", new Vector2(0.34f, 0.33f), new Vector2(0.66f, 0.42f));
            _newGameButton.onClick.AddListener(OnNewGameClicked);
            _mainMenuCodexButton = CreateButton("Codex", _mainMenuRoot, "卦象图鉴", new Vector2(0.34f, 0.21f), new Vector2(0.66f, 0.30f));
            _mainMenuCodexButton.onClick.AddListener(OnMainMenuCodexClicked);
            _mainMenuRoot.gameObject.SetActive(false);
            _codexRoot.gameObject.SetActive(false);
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

        private static string SavePath
        {
            get { return Path.Combine(Application.persistentDataPath, "balartrolike.save.json"); }
        }

        private void ShowMainMenu()
        {
            HideAllRoots();
            _mainMenuRoot.gameObject.SetActive(true);
            _runController = null;
            _controller = null;
            _innerUid = null;
            _outerUid = null;
            _confirmingNewGame = false;
            _hasRunSave = false;
            _selectedHexagram = null;
            _newGameButton.GetComponentInChildren<Text>().text = "开始修行";

            bool hasSaveFile = File.Exists(SavePath) || File.Exists(SavePath + ".bak");
            if (!hasSaveFile)
            {
                _continueGameButton.gameObject.SetActive(false);
                _mainMenuStatusText.text = "尚无修行记录，开始新的历劫。\n当前天劫 " + _metaProgress.SelectedHeavenTribulation + " / 最高 " + _metaProgress.HighestHeavenTribulation;
                return;
            }

            if (!SaveService.TryLoad(SavePath, out GameSaveData data, out string error))
            {
                _continueGameButton.gameObject.SetActive(false);
                _mainMenuStatusText.text = "存档不可用：" + error;
                return;
            }

            ApplyMetaToRuntime(data.meta);
            _hasRunSave = data.run != null;
            _continueGameButton.gameObject.SetActive(_hasRunSave);
            _newGameButton.GetComponentInChildren<Text>().text = _hasRunSave ? "开始新游戏" : "开始修行";
            _mainMenuStatusText.text = (_hasRunSave ? "检测到未完成的修行进度。" : "尚无进行中的修行。")
                + "\n当前天劫 " + _metaProgress.SelectedHeavenTribulation + " / 最高 " + _metaProgress.HighestHeavenTribulation;
        }

        private void ApplyMetaToRuntime(MetaSaveData meta)
        {
            if (meta == null)
            {
                return;
            }

            _metaProgress.DaoHeart = meta.dao_heart;
            _metaProgress.HighestHeavenTribulation = meta.highest_heaven_tribulation;
            _metaProgress.SelectedHeavenTribulation = meta.selected_heaven_tribulation;
            _metaProgress.CompletedRunCount = meta.completed_run_count;
            _metaProgress.HexagramUses.Clear();
            if (meta.hexagram_uses != null)
            {
                for (int i = 0; i < meta.hexagram_uses.Count; i++)
                {
                    HexagramUseSaveData entry = meta.hexagram_uses[i];
                    if (!string.IsNullOrEmpty(entry.hexagram_id) && entry.use_count > 0)
                    {
                        _metaProgress.HexagramUses[entry.hexagram_id] = entry.use_count;
                    }
                }
            }
        }

        private void OnContinueGameClicked()
        {
            if (!SaveService.TryLoad(SavePath, out GameSaveData data, out string loadError))
            {
                _mainMenuStatusText.text = "继续失败：" + loadError;
                return;
            }

            if (!SaveService.TryRestore(data, out RunController run, out BattleController battle, out _, out string restoreError))
            {
                _mainMenuStatusText.text = "继续失败：" + restoreError;
                return;
            }

            if (run == null)
            {
                _mainMenuStatusText.text = "没有可继续的局内进度。";
                return;
            }

            _runController = run;
            _controller = battle;
            ApplyMetaToRuntime(data.meta);
            _innerUid = null;
            _outerUid = null;
            Refresh();
        }

        private void OnNewGameClicked()
        {
            if (_hasRunSave && !_confirmingNewGame)
            {
                _confirmingNewGame = true;
                _mainMenuStatusText.text = "再次点击将放弃当前进度并开始新一局。";
                _newGameButton.GetComponentInChildren<Text>().text = "确认放弃进度";
                return;
            }

            RestartRun();
        }

        private void SaveCurrentProgress()
        {
            if (_runController == null)
            {
                return;
            }

            if (!SaveService.TrySave(SavePath, _runController, _controller, out string error))
            {
                Debug.LogWarning("保存失败：" + error);
            }
        }

        private void SaveMetaProgress()
        {
            RunMetaProgressState meta = _runController.MetaProgress;
            if (!SaveService.TrySaveMeta(SavePath, meta, out string error))
            {
                Debug.LogWarning("保存失败：" + error);
            }

            _metaProgress.DaoHeart = meta.DaoHeart;
            _metaProgress.HighestHeavenTribulation = meta.HighestHeavenTribulation;
            _metaProgress.SelectedHeavenTribulation = meta.SelectedHeavenTribulation;
            _metaProgress.CompletedRunCount = meta.CompletedRunCount;
            _metaProgress.HexagramUses.Clear();
            foreach (KeyValuePair<string, int> entry in meta.HexagramUses)
            {
                _metaProgress.HexagramUses[entry.Key] = entry.Value;
            }
        }

        private void HideAllRoots()
        {
            HideBattleFeedback();
            SetBattleVisible(false);
            _routeRoot.gameObject.SetActive(false);
            _battleResultRoot.gameObject.SetActive(false);
            _shopRoot.gameObject.SetActive(false);
            _eventRoot.gameObject.SetActive(false);
            _promotionRoot.gameObject.SetActive(false);
            _runResultRoot.gameObject.SetActive(false);
            _metaProgressRoot.gameObject.SetActive(false);
            _mainMenuRoot.gameObject.SetActive(false);
            _codexRoot.gameObject.SetActive(false);
        }
        private void RestartRun()
        {
            HideBattleFeedback();
            _runController = RunController.CreatePrototype(Environment.TickCount, _metaProgress);
            _controller = null;
            _innerUid = null;
            _outerUid = null;
            _confirmingNewGame = false;
            SaveCurrentProgress();
            Refresh();
        }

        private void StartCurrentBattle()
        {
            if (_runController.State.Phase != RunPhase.Map)
            {
                return;
            }

            _controller = _runController.StartBattle();
            _innerUid = null;
            _outerUid = null;
            SaveCurrentProgress();
            Refresh();
        }

        private void OnStartBattleClicked()
        {
            StartCurrentBattle();
        }

        private void OnWeaponClicked()
        {
            int currentIndex = 0;
            for (int i = 0; i < BattleConfigDatabase.Weapons.Count; i++)
            {
                if (BattleConfigDatabase.Weapons[i].Id == _runController.State.WeaponId)
                {
                    currentIndex = i;
                    break;
                }
            }

            int nextIndex = (currentIndex + 1) % BattleConfigDatabase.Weapons.Count;
            RunCommandResult result = _runController.SelectWeapon(BattleConfigDatabase.Weapons[nextIndex].Id);
            if (!result.Success)
            {
                Debug.LogWarning(result.Error);
                return;
            }

            SaveCurrentProgress();
            Refresh();
        }

        private void OnCardClicked(int cardUid)
        {
            if (_battleInputLocked)
            {
                return;
            }

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
            if (_battleInputLocked || _innerUid == null || _outerUid == null)
            {
                return;
            }

            BattleState state = _controller.State;
            int enemyHpBefore = state.Enemy.Hp;
            int enemyShieldBefore = state.Enemy.Shield;
            BattleCommandResult result = _controller.PlayHexagram(_innerUid.Value, _outerUid.Value);
            if (!result.Success)
            {
                Debug.LogWarning(result.Error);
                ShowBattleFeedback(result.Error, new Color(0.95f, 0.45f, 0.35f, 1f));
            }
            else
            {
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
                ShowBattleFeedback(feedback, color);
                if (state.Phase == BattlePhase.BattleEnd)
                {
                    SaveCurrentProgress();
                }
            }

            _innerUid = null;
            _outerUid = null;
            Refresh();
        }

        private void OnTalismanClicked(int index)
        {
            if (_battleInputLocked || index < 0 || index >= _controller.State.Talismans.Count)
            {
                return;
            }

            string talismanName = _controller.State.Talismans[index].DisplayName;
            RunCommandResult result = _runController.UseTalisman(_controller, index);
            if (!result.Success)
            {
                Debug.LogWarning(result.Error);
                ShowBattleFeedback(result.Error, new Color(0.95f, 0.45f, 0.35f, 1f));
            }
            else
            {
                ShowBattleFeedback("符箓\n" + talismanName, new Color(0.45f, 0.85f, 0.85f, 1f));
                if (_controller.State.Phase == BattlePhase.BattleEnd)
                {
                    SaveCurrentProgress();
                }
            }

            Refresh();
        }

        private void OnEndTurnClicked()
        {
            if (_battleInputLocked)
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
                Debug.LogWarning(result.Error);
                ShowBattleFeedback(result.Error, new Color(0.95f, 0.45f, 0.35f, 1f));
            }
            else
            {
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
                    feedback += "\n玩家 -" + playerDamage;
                }

                if (playerShieldDamage > 0)
                {
                    feedback += "\n护盾 -" + playerShieldDamage;
                }

                Color color = playerDamage > 0
                    ? new Color(0.95f, 0.45f, 0.35f, 1f)
                    : new Color(0.95f, 0.78f, 0.45f, 1f);
                ShowBattleFeedback(feedback, color);
            }

            _innerUid = null;
            _outerUid = null;
            SaveCurrentProgress();
            Refresh();
        }

        private void ShowBattleFeedback(string message, Color color)
        {
            _battleInputLocked = true;
            RefreshBattleInputState();
            _battleFeedbackView.Play(message, color, OnBattleFeedbackComplete);
        }

        private void OnBattleFeedbackComplete()
        {
            _battleInputLocked = false;
            RefreshBattleInputState();
        }

        private void HideBattleFeedback()
        {
            _battleFeedbackView.Hide();
            _battleInputLocked = false;
        }

        private void RefreshBattleInputState()
        {
            if (_controller == null)
            {
                return;
            }

            bool playerCanAct = CanPlayerAct();
            _playButton.interactable = playerCanAct && _innerUid != null && _outerUid != null;
            _endTurnButton.interactable = playerCanAct;
            for (int i = 0; i < _cardButtons.Count; i++)
            {
                _cardButtons[i].interactable = playerCanAct;
            }

            for (int i = 0; i < _talismanButtons.Count; i++)
            {
                _talismanButtons[i].interactable = playerCanAct;
            }
        }

        private void OnRestartClicked()
        {
            RestartRun();
        }

        private void OnContinueClicked()
        {
            if (_controller.State.Phase != BattlePhase.BattleEnd)
            {
                return;
            }

            RunCommandResult result = _runController.CompleteBattle(_controller.State.Result);
            if (!result.Success)
            {
                Debug.LogWarning(result.Error);
                return;
            }

            _controller = null;
            SaveCurrentProgress();
            Refresh();
        }

        private void OnConfirmBattleResultClicked()
        {
            RunCommandResult result = _runController.ConfirmBattleResult();
            if (!result.Success)
            {
                Debug.LogWarning(result.Error);
                return;
            }

            SaveCurrentProgress();
            Refresh();
        }

        private void OnRealmPromotionConfirmClicked()
        {
            RunCommandResult result = _runController.CompleteRealmPromotion();
            if (!result.Success)
            {
                Debug.LogWarning(result.Error);
                return;
            }

            SaveCurrentProgress();
            Refresh();
        }

        private void OnRunResultConfirmClicked()
        {
            RunCommandResult result = _runController.ConfirmRunResult();
            if (!result.Success)
            {
                Debug.LogWarning(result.Error);
                return;
            }

            SaveMetaProgress();
            Refresh();
        }

        private void OnMetaProgressRestartClicked()
        {
            RestartRun();
        }

        private void OnMetaProgressPrevClicked()
        {
            ChangeSelectedTribulation(-1);
        }

        private void OnMetaProgressNextClicked()
        {
            ChangeSelectedTribulation(1);
        }

        private void ChangeSelectedTribulation(int delta)
        {
            RunMetaProgressState meta = _runController.MetaProgress;
            int maxLevel = Math.Min(meta.HighestHeavenTribulation, RunConfigDatabase.MaxTribulationLevel);
            int nextLevel = Math.Max(0, Math.Min(meta.SelectedHeavenTribulation + delta, maxLevel));
            if (nextLevel == meta.SelectedHeavenTribulation)
            {
                return;
            }

            meta.SelectedHeavenTribulation = nextLevel;
            SaveMetaProgress();
            Refresh();
        }

        private void OnShopOfferClicked(int index)
        {
            if (index >= _runController.State.ActiveShopOfferIds.Count)
            {
                return;
            }

            RunCommandResult result = _runController.BuyShopOffer(_runController.State.ActiveShopOfferIds[index]);
            if (!result.Success)
            {
                Debug.LogWarning(result.Error);
            }
            else
            {
                SaveCurrentProgress();
            }

            Refresh();
        }

        private void OnShopRefreshClicked()
        {
            RunCommandResult result = _runController.RefreshShop();
            if (!result.Success)
            {
                Debug.LogWarning(result.Error);
            }
            else
            {
                SaveCurrentProgress();
            }

            Refresh();
        }

        private void OnShopLeaveClicked()
        {
            RunCommandResult result = _runController.LeaveShop();
            if (!result.Success)
            {
                Debug.LogWarning(result.Error);
                return;
            }

            SaveCurrentProgress();
            Refresh();
        }

        private void OnEventOptionClicked(int index)
        {
            IReadOnlyList<RunEventOptionDefinition> options = RunEncounterDatabase.GetEventOptions(_runController.State.ActiveEventId);
            if (index >= options.Count)
            {
                return;
            }

            RunCommandResult result = _runController.ResolveEvent(options[index].OptionId);
            if (!result.Success)
            {
                Debug.LogWarning(result.Error);
            }
            else
            {
                SaveCurrentProgress();
            }

            Refresh();
        }

        private void OnEventLeaveClicked()
        {
            RunCommandResult result = _runController.LeaveEvent();
            if (!result.Success)
            {
                Debug.LogWarning(result.Error);
                return;
            }

            SaveCurrentProgress();
            Refresh();
        }

        private void Refresh()
        {
            _mainMenuRoot.gameObject.SetActive(false);
            if (_runController.State.Phase == RunPhase.Battle)
            {
                RefreshBattle();
                return;
            }

            if (_runController.State.Phase == RunPhase.BattleResult)
            {
                RefreshBattleResult();
                return;
            }

            if (_runController.State.Phase == RunPhase.Shop)
            {
                RefreshShop();
                return;
            }

            if (_runController.State.Phase == RunPhase.Event)
            {
                RefreshEvent();
                return;
            }

            if (_runController.State.Phase == RunPhase.RealmPromotion)
            {
                RefreshRealmPromotion();
                return;
            }

            if (_runController.State.Phase == RunPhase.RunResult)
            {
                RefreshRunResult();
                return;
            }

            if (_runController.State.Phase == RunPhase.MetaProgress)
            {
                RefreshMetaProgress();
                return;
            }

            RefreshRoute();
        }

        private void RefreshBattle()
        {
            SetBattleVisible(true);
            _routeRoot.gameObject.SetActive(false);
            _battleResultRoot.gameObject.SetActive(false);
            _shopRoot.gameObject.SetActive(false);
            _eventRoot.gameObject.SetActive(false);
            _promotionRoot.gameObject.SetActive(false);
            _runResultRoot.gameObject.SetActive(false);
            _metaProgressRoot.gameObject.SetActive(false);
            RunState runState = _runController.State;
            string runText = "第 " + (runState.CurrentNodeIndex + 1) + "/" + _runController.NodeCount + " 关 "
                + _runController.CurrentNode.DisplayName + "   灵石 " + runState.SpiritStones;
            BattleState state = _controller.State;
            _statusText.text = runText + "   |   第 " + state.Turn + " 回合   玩家 HP " + state.Player.Hp + "/" + state.Player.MaxHp
                + "   护盾 " + state.Player.Shield + "   灵力 " + state.Player.Energy + "/" + state.Player.MaxEnergy
                + "   |   敌人 " + state.Enemy.DisplayName + " HP " + state.Enemy.Hp + "/" + state.Enemy.MaxHp
                + "   护盾 " + state.Enemy.Shield + "   意图 " + (state.Enemy.CurrentIntent == null ? "未知" : state.Enemy.CurrentIntent.DisplayText);
            _previewText.text = BuildPreviewText();
            _logText.text = BuildLogText();
            bool playerCanAct = CanPlayerAct();
            _playButton.interactable = _innerUid != null && _outerUid != null && playerCanAct;
            _endTurnButton.interactable = playerCanAct;
            _continueButton.interactable = state.Phase == BattlePhase.BattleEnd;
            RebuildHand();
            RebuildTalismans();
        }

        private void RefreshBattleResult()
        {
            SetBattleVisible(false);
            _routeRoot.gameObject.SetActive(false);
            _battleResultRoot.gameObject.SetActive(true);
            _shopRoot.gameObject.SetActive(false);
            _eventRoot.gameObject.SetActive(false);
            _promotionRoot.gameObject.SetActive(false);
            _runResultRoot.gameObject.SetActive(false);
            _metaProgressRoot.gameObject.SetActive(false);

            RunState state = _runController.State;
            RunNodeDefinition node = _runController.CurrentNode;
            bool victory = state.PendingBattleResult == BattleResultType.Victory;
            _battleResultTitleText.text = victory ? "战斗胜利" : "战斗失败";
            _battleResultDetailText.text = victory
                ? node.DisplayName + "\n\n灵石 +" + state.PendingRewardSpiritStones
                : node.DisplayName + "\n\n本次修行结束";
            _confirmBattleResultButton.GetComponentInChildren<Text>().text = victory ? "领取奖励" : "查看结算";
        }

        private void RefreshShop()
        {
            SetBattleVisible(false);
            _routeRoot.gameObject.SetActive(false);
            _battleResultRoot.gameObject.SetActive(false);
            _shopRoot.gameObject.SetActive(true);
            _eventRoot.gameObject.SetActive(false);
            _promotionRoot.gameObject.SetActive(false);
            _runResultRoot.gameObject.SetActive(false);
            _metaProgressRoot.gameObject.SetActive(false);

            RunState state = _runController.State;
            _shopTitleText.text = "坊市";
            _shopStatusText.text = "灵石 " + state.SpiritStones + "   刷新 " + state.ShopRefreshCount + "/2";
            for (int i = 0; i < _shopOfferButtons.Count; i++)
            {
                bool hasOffer = i < state.ActiveShopOfferIds.Count;
                _shopOfferButtons[i].gameObject.SetActive(hasOffer);
                if (!hasOffer)
                {
                    continue;
                }

                string offerId = state.ActiveShopOfferIds[i];
                RunEncounterDatabase.TryGetShopOffer(offerId, out RunShopOfferDefinition offer);
                bool purchased = state.PurchasedShopOfferIds.Contains(offerId);
                int price = _runController.GetShopPrice(offer);
                _shopOfferLabels[i].text = offer.DisplayName + "\n" + offer.Description + "\n价格 " + price + " 灵石";
                _shopOfferButtons[i].interactable = !purchased && state.SpiritStones >= price;
                _shopOfferLabels[i].text += purchased ? "\n已购买" : string.Empty;
            }

            _shopRefreshButton.interactable = state.ShopRefreshCount < 2 && state.SpiritStones >= 20;
        }

        private void RefreshEvent()
        {
            SetBattleVisible(false);
            _routeRoot.gameObject.SetActive(false);
            _battleResultRoot.gameObject.SetActive(false);
            _shopRoot.gameObject.SetActive(false);
            _eventRoot.gameObject.SetActive(true);
            _promotionRoot.gameObject.SetActive(false);
            _runResultRoot.gameObject.SetActive(false);
            _metaProgressRoot.gameObject.SetActive(false);

            RunState state = _runController.State;
            IReadOnlyList<RunEventOptionDefinition> options = RunEncounterDatabase.GetEventOptions(state.ActiveEventId);
            _eventTitleText.text = options.Count > 0 ? options[0].Title : "奇遇";
            _eventDescriptionText.text = state.EventResolved
                ? state.EventResultText
                : (options.Count > 0 ? options[0].Description : string.Empty);
            for (int i = 0; i < _eventOptionButtons.Count; i++)
            {
                bool hasOption = i < options.Count;
                _eventOptionButtons[i].gameObject.SetActive(hasOption);
                if (!hasOption)
                {
                    continue;
                }

                _eventOptionLabels[i].text = options[i].OptionText;
                _eventOptionButtons[i].interactable = !state.EventResolved;
            }

            _eventLeaveButton.gameObject.SetActive(state.EventResolved);
        }

        private void RefreshRealmPromotion()
        {
            SetBattleVisible(false);
            _routeRoot.gameObject.SetActive(false);
            _battleResultRoot.gameObject.SetActive(false);
            _shopRoot.gameObject.SetActive(false);
            _eventRoot.gameObject.SetActive(false);
            _promotionRoot.gameObject.SetActive(true);
            _runResultRoot.gameObject.SetActive(false);
            _metaProgressRoot.gameObject.SetActive(false);

            RunState state = _runController.State;
            string targetName = GetRealmDisplayName(state.PromotionTargetRealmId);
            _promotionTitleText.text = "渡劫成功 · 晋升" + targetName;
            _promotionDetailText.text = "境界奖励\n灵石 +" + state.PendingRealmRewardSpiritStones
                + (state.PromotionTargetRealmId == "REALM_NASCENT_SOUL"
                    ? "\n\n元婴渡劫完成，即将进入本局结算。"
                    : "\n\n继续修行，前往下一境界。");
        }

        private void RefreshRunResult()
        {
            SetBattleVisible(false);
            _routeRoot.gameObject.SetActive(false);
            _battleResultRoot.gameObject.SetActive(false);
            _shopRoot.gameObject.SetActive(false);
            _eventRoot.gameObject.SetActive(false);
            _promotionRoot.gameObject.SetActive(false);
            _runResultRoot.gameObject.SetActive(true);
            _metaProgressRoot.gameObject.SetActive(false);

            RunState state = _runController.State;
            _runResultTitleText.text = state.Result == RunResultType.Victory ? "本局通关" : "本局失败";
            _runResultDetailText.text = "已通过 " + state.CompletedNodeIds.Count + "/" + _runController.NodeCount
                + "\n累计灵石 " + state.SpiritStones
                + "\n道行 +" + state.DaoHeartReward;
        }

        private void RefreshMetaProgress()
        {
            SetBattleVisible(false);
            _routeRoot.gameObject.SetActive(false);
            _battleResultRoot.gameObject.SetActive(false);
            _shopRoot.gameObject.SetActive(false);
            _eventRoot.gameObject.SetActive(false);
            _promotionRoot.gameObject.SetActive(false);
            _runResultRoot.gameObject.SetActive(false);
            _metaProgressRoot.gameObject.SetActive(true);

            RunMetaProgressState meta = _runController.MetaProgress;
            int discovered = 0;
            int mastered = 0;
            foreach (KeyValuePair<string, int> entry in meta.HexagramUses)
            {
                discovered++;
                if (HexagramMastery.IsMastered(entry.Value))
                {
                    mastered++;
                }
            }

            HeavenTribulationDefinition tribulation = RunConfigDatabase.GetTribulation(meta.SelectedHeavenTribulation);
            _metaProgressTitleText.text = "局外成长";
            _metaProgressDetailText.text = "累计道行 " + meta.DaoHeart
                + "\n完成修行 " + meta.CompletedRunCount + " 次"
                + "\n当前天劫 " + tribulation.DisplayName + "   最高 " + meta.HighestHeavenTribulation
                + "\n" + tribulation.Description
                + "\n图鉴发现 " + discovered + "/64   精通 " + mastered
                + "\n\n道行解锁商店与天劫 4+ 词条将在后续接入。";
            _metaProgressPrevButton.interactable = meta.SelectedHeavenTribulation > 0;
            _metaProgressNextButton.interactable = meta.SelectedHeavenTribulation < Math.Min(meta.HighestHeavenTribulation, RunConfigDatabase.MaxTribulationLevel);
        }

        private void RefreshRoute()
        {
            SetBattleVisible(false);
            _routeRoot.gameObject.SetActive(true);
            _battleResultRoot.gameObject.SetActive(false);
            _shopRoot.gameObject.SetActive(false);
            _eventRoot.gameObject.SetActive(false);
            _promotionRoot.gameObject.SetActive(false);
            _runResultRoot.gameObject.SetActive(false);
            _metaProgressRoot.gameObject.SetActive(false);

            RunState state = _runController.State;
            RunNodeDefinition node = _runController.CurrentNode;
            _routeTitleText.text = "修行历劫";
            _routeStatusText.text = "进度 " + (state.CurrentNodeIndex + 1) + "/" + _runController.NodeCount
                + "   累计灵石 " + state.SpiritStones + "   天劫 " + state.HeavenTribulationLevel;
            _routeDetailText.text = "当前节点\n" + node.DisplayName
                + "\n\n境界  " + GetRealmDisplayName(node.RealmId)
                + "\n类型  " + GetNodeTypeDisplayName(node.Type)
                + "\n敌人  " + BattleConfigDatabase.GetEnemy(node.SelectEnemyId(state.Seed + state.CurrentNodeIndex)).DisplayName
                + "\n奖励  灵石 +" + node.RewardSpiritStones
                + "\n武器  " + BattleConfigDatabase.GetWeapon(state.WeaponId).DisplayName;
            _weaponButton.GetComponentInChildren<Text>().text = "切换武器：" + BattleConfigDatabase.GetWeapon(state.WeaponId).DisplayName;
            _weaponButton.interactable = state.CurrentNodeIndex == 0 && state.CompletedNodeIds.Count == 0;
            _startBattleButton.interactable = true;

            RefreshRouteNodes(state);
        }

        private void RefreshRouteNodes(RunState state)
        {
            for (int i = 0; i < _routeNodeButtons.Count; i++)
            {
                RunNodeDefinition node = RunConfigDatabase.Nodes[i];
                bool completed = i < state.CurrentNodeIndex;
                bool current = state.Phase == RunPhase.Map && i == state.CurrentNodeIndex;
                _routeNodeLabels[i].text = (completed ? "✓ " : current ? "▶ " : "◇ ") + node.DisplayName
                    + "\n" + GetNodeTypeDisplayName(node.Type) + "   灵石 " + node.RewardSpiritStones;

                Image image = _routeNodeButtons[i].GetComponent<Image>();
                image.color = completed
                    ? new Color(0.18f, 0.32f, 0.23f, 1f)
                    : current
                        ? new Color(0.65f, 0.45f, 0.17f, 1f)
                        : new Color(0.10f, 0.09f, 0.075f, 1f);
                _routeNodeLabels[i].color = completed || current
                    ? new Color(0.93f, 0.89f, 0.81f, 1f)
                    : new Color(0.48f, 0.45f, 0.40f, 1f);
            }
        }

        private void SetBattleVisible(bool visible)
        {
            _statusText.gameObject.SetActive(visible);
            _previewText.gameObject.SetActive(visible);
            _logText.gameObject.SetActive(visible);
            _handRoot.gameObject.SetActive(visible);
            _talismanRoot.gameObject.SetActive(visible);
            _playButton.gameObject.SetActive(visible);
            _endTurnButton.gameObject.SetActive(visible);
            _continueButton.gameObject.SetActive(visible);
            _restartButton.gameObject.SetActive(visible);
        }

        private static string GetRealmDisplayName(string realmId)
        {
            switch (realmId)
            {
                case "REALM_QI_REFINING":
                    return "炼气";
                case "REALM_FOUNDATION":
                    return "筑基";
                case "REALM_GOLDEN_CORE":
                    return "金丹";
                case "REALM_NASCENT_SOUL":
                    return "元婴";
                default:
                    return realmId;
            }
        }

        private static string GetElementDisplayName(ElementType element)
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

        private static string GetAttackPatternDisplayName(AttackPattern pattern)
        {
            switch (pattern)
            {
                case AttackPattern.MultiHit:
                    return "多段";
                case AttackPattern.AllTargets:
                    return "群攻";
                default:
                    return "单体";
            }
        }

        private static string GetNodeTypeDisplayName(RunNodeType type)
        {
            switch (type)
            {
                case RunNodeType.EliteBattle:
                    return "精英";
                case RunNodeType.TribulationBattle:
                    return "渡劫";
                default:
                    return "普通";
            }
        }

        private string BuildPreviewText()
        {
            string stateText = "\n\n" + BuildArtifactText() + "\n" + BuildEnchantText();
            if (_innerUid == null)
            {
                return "先选择一张手牌作为【内卦】" + stateText;
            }

            if (_outerUid == null)
            {
                return "再选择一张手牌作为【外卦】" + stateText;
            }

            BattleCommandResult preview = _controller.PreviewPlay(_innerUid.Value, _outerUid.Value);
            if (!preview.Success)
            {
                return preview.Error + stateText;
            }

            BattleCalculation calculation = preview.Calculation;
            return calculation.Hexagram.DisplayName
                + "\n上卦 " + TrigramCatalog.Get(calculation.Hexagram.Outer).DisplayName
                + " / 下卦 " + TrigramCatalog.Get(calculation.Hexagram.Inner).DisplayName
                + "\n模式 " + GetAttackPatternDisplayName(calculation.AttackPattern)
                + "   每段 " + calculation.Damage.FinalDamage + "   连击 " + calculation.HitCount + " 段"
                + "\n熟练度 " + HexagramMastery.GetLevel(_controller.State.GetHexagramUseCount(calculation.Hexagram.Id)) + "/5"
                + (calculation.Damage.MasteryMultiplier > 10000 ? "   精通加成 ×1.20" : string.Empty)
                + "\n" + calculation.Damage.ToFormula()
                + stateText
                + BuildEnchantWarning(calculation)
                + BuildKillHint(calculation);
        }

        private string BuildArtifactText()
        {
            StringBuilder builder = new StringBuilder("法宝：");
            if (_controller.State.Artifacts.Count == 0)
            {
                return builder.Append("无").ToString();
            }

            for (int i = 0; i < _controller.State.Artifacts.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append("、");
                }

                builder.Append(_controller.State.Artifacts[i].DisplayName);
            }

            return builder.ToString();
        }

        private string BuildEnchantText()
        {
            List<EnchantInstance> enchantments = _controller.State.Player.Weapon.Enchantments;
            StringBuilder builder = new StringBuilder("附魔槽：");
            if (enchantments.Count == 0)
            {
                return builder.Append("空").ToString();
            }

            for (int i = 0; i < enchantments.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append("  |  ");
                }

                EnchantInstance enchantment = enchantments[i];
                builder.Append(TrigramCatalog.Get(enchantment.Source).DisplayName)
                    .Append("·")
                    .Append(GetEffectDisplayName(enchantment.Effect))
                    .Append(" ")
                    .Append(enchantment.RemainingTurns)
                    .Append("回合");
            }

            return builder.ToString();
        }

        private string BuildEnchantWarning(BattleCalculation calculation)
        {
            if (_controller.State.Player.Weapon.Enchantments.Count < _controller.State.Player.Weapon.MaxEnchantSlots)
            {
                return string.Empty;
            }

            for (int i = 0; i < calculation.Effects.Count; i++)
            {
                if (calculation.Effects[i].Duration > 0)
                {
                    return "\n附魔槽已满：本次写入将顶替最旧附魔";
                }
            }

            return string.Empty;
        }

        private string BuildKillHint(BattleCalculation calculation)
        {
            return calculation.CanDefeat(_controller.State.Enemy)
                ? "\n斩杀提示：本次出卦可击杀"
                : string.Empty;
        }

        private bool CanPlayerAct()
        {
            return _controller != null
                && _controller.State.Phase == BattlePhase.PlayerAction
                && !_battleInputLocked;
        }

        private static string GetEffectDisplayName(EffectOperation effect)
        {
            switch (effect.Type)
            {
                case EffectType.ApplyStatus:
                    return GetStatusDisplayName(effect.Status);
                case EffectType.GainShield:
                    return "护盾";
                case EffectType.GainEnergy:
                    return "回灵";
                case EffectType.DrawCard:
                    return "抽牌";
                default:
                    return effect.Type.ToString();
            }
        }

        private static string GetStatusDisplayName(StatusId status)
        {
            switch (status)
            {
                case StatusId.Burn:
                    return "灼烧";
                case StatusId.Vulnerable:
                    return "易伤";
                case StatusId.Weak:
                    return "虚弱";
                case StatusId.Shield:
                    return "护盾";
                case StatusId.ArmorBreak:
                    return "破甲";
                case StatusId.Chill:
                    return "寒滞";
                default:
                    return "状态";
            }
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
            for (int i = 0; i < _controller.State.Hand.Count; i++)
            {
                EnsureCardButton(i);
                CardInstance card = _controller.State.Hand[i];
                _cardUids[i] = card.Uid;
                _cardLabels[i].text = BuildCardText(card);
                _cardButtons[i].gameObject.SetActive(true);
                _cardButtons[i].interactable = CanPlayerAct();

                Image image = _cardButtons[i].GetComponent<Image>();
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

            for (int i = _controller.State.Hand.Count; i < _cardButtons.Count; i++)
            {
                _cardButtons[i].gameObject.SetActive(false);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(_handRoot);
        }

        private void RebuildTalismans()
        {
            for (int i = 0; i < _controller.State.Talismans.Count; i++)
            {
                EnsureTalismanButton(i);
                TalismanDefinition talisman = _controller.State.Talismans[i];
                _talismanLabels[i].text = talisman.DisplayName + "  " + GetTalismanEffectText(talisman);
                _talismanButtons[i].interactable = CanPlayerAct();
                _talismanButtons[i].gameObject.SetActive(true);
            }

            for (int i = _controller.State.Talismans.Count; i < _talismanButtons.Count; i++)
            {
                _talismanButtons[i].gameObject.SetActive(false);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(_talismanRoot);
        }

        private void EnsureTalismanButton(int index)
        {
            if (index < _talismanButtons.Count)
            {
                return;
            }

            int capturedIndex = _talismanButtons.Count;
            Button button = CreateButton("Talisman_" + capturedIndex, _talismanRoot, string.Empty, Vector2.zero, Vector2.one);
            button.onClick.AddListener(delegate { OnTalismanClicked(capturedIndex); });
            _talismanButtons.Add(button);
            _talismanLabels.Add(button.GetComponentInChildren<Text>());
        }

        private static string GetTalismanEffectText(TalismanDefinition talisman)
        {
            if (talisman.Effects.Length == 0)
            {
                return string.Empty;
            }

            EffectOperation effect = talisman.Effects[0];
            switch (effect.Type)
            {
                case EffectType.GainEnergy:
                    return "灵力+" + effect.Value;
                case EffectType.GainShield:
                    return "护盾+" + effect.Value;
                case EffectType.DrawCard:
                    return "抽" + effect.Value + "张";
                default:
                    return GetEffectDisplayName(effect);
            }
        }

        private void EnsureCardButton(int index)
        {
            if (index < _cardButtons.Count)
            {
                return;
            }

            int capturedIndex = _cardButtons.Count;
            Button button = CreateButton("Card_" + capturedIndex, _handRoot, string.Empty, Vector2.zero, Vector2.one);
            button.onClick.AddListener(delegate { OnCardClicked(_cardUids[capturedIndex]); });
            _cardButtons.Add(button);
            _cardLabels.Add(button.GetComponentInChildren<Text>());
            _cardUids.Add(0);
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
