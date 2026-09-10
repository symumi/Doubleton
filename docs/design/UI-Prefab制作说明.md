# 《卦锻》UI Prefab 制作说明

> 版本：v0.1（2026-09-10）｜范围：阶段 1（基础设施）+ 阶段 2（公共组件）
> 配套代码：`Assets/Scripts/UI/`｜素材：`Assets/Resources/Texture/UI/`｜总纲：[UI制作文档.md](UI制作文档.md)

本文档给出**需要手工制作的 Prefab 清单**：层级树、组件、字段绑定、素材引用和注意事项。代码部分已由我完成，你按本文档搭 Prefab 即可接线。

---

## 1. 本批次已完成的代码

| 文件 | 作用 |
|---|---|
| `Assets/Scripts/UI/UIRoot.cs` | UI 根：六层引用、服务聚合、`Toast()` / `Confirm()` 统一入口 |
| `Assets/Scripts/UI/UIInputBlocker.cs` | 输入锁（引用计数、`BlockedChanged` 事件） |
| `Assets/Scripts/UI/UIPageView.cs` | 一级页面基类（`OnShow` / `OnHide`） |
| `Assets/Scripts/UI/UIPageService.cs` | 页面栈（`Open` / `Replace` / `Back` / `CloseAll`） |
| `Assets/Scripts/UI/UIPopupService.cs` | 弹窗栈（遮罩、输入锁、Esc 关闭） |
| `Assets/Scripts/UI/UITween.cs` | 缓动函数 + 非缩放时间协程播放器 |
| `Assets/Scripts/UI/UIAnimationConfig.cs` | `ui_animation.csv` 解析（缺失 key 回退默认值） |
| `Assets/Scripts/UI/Common/ToastView.cs` | 单条短提示（淡入 → 停留 → 淡出） |
| `Assets/Scripts/UI/Common/UIToastService.cs` | 提示队列（最多同屏 3 条） |
| `Assets/Scripts/UI/Common/TooltipView.cs` | 悬停说明浮层（跟随鼠标 + 边界夹取） |
| `Assets/Scripts/UI/Common/TooltipTrigger.cs` | 悬停触发器（延时 → 请求 Tooltip） |
| `Assets/Scripts/UI/Common/ConfirmPopup.cs` | 通用确认弹窗 |
| `Assets/Scripts/UI/Common/UIProgressBar.cs` | 生命 / 护盾 / 灵力条（含延迟条） |
| `Assets/Resources/Config/UI/ui_animation.csv` | 29 条动效参数（外部可调） |
| `Assets/Tests/EditMode/UICoreTests.cs` | 18 个 EditMode 测试（配置解析、缓动、输入锁、页面栈） |

代码已通过 MSBuild 编译检查，测试工程编译见本轮交付说明。

---

## 2. 通用约定

| 项 | 约定 |
|---|---|
| Prefab 目录 | `Assets/Resources/Prefab/UI/` |
| Canvas | Screen Space - Overlay；CanvasScaler = Scale With Screen Size，1920×1080，Match 0.5 |
| 层节点 | 全屏层用锚点 (0,0)-(1,1)、Left/Top/Right/Bottom 全 0 |
| 文本 | 一律 TextMeshPro（`TextMeshProUGUI`），不要用 legacy `Text` |
| 字体资产 | `TMP_Title`（楷/宋）、`TMP_Body`（黑体）、`TMP_Number`（等宽）——需你先在 Window > TextMeshPro > Font Asset Creator 生成 |
| 九宫格 | 面板/按钮/卡框类 Image 必须 `Type = Sliced`，Border 按 [ui_manifest.md](ui_source/ui_manifest.md) 填写 |
| 平铺素材 | 纸纹、扫描线用 `Type = Tiled`，原始图 Wrap Mode 设 Repeat |
| 命名 | 节点名与字段含义一致（`Fill`、`DelayedFill`、`PlayButton`…），便于查找替换 |

---

## 3. Prefab 清单与制作顺序

| 顺序 | Prefab | 依赖 | 说明 |
|---|---|---|---|
| P1 | `ToastView.prefab` | 无 | 最小，先做，用来验证 UIRoot |
| P2 | `ConfirmPopup.prefab` | 无 | 弹窗模板 |
| P3 | `TooltipView.prefab` | 无 | 悬停浮层 |
| P4 | `UIRoot.prefab` | P1–P3 | 主骨架，把前三个挂进去 |
| P5 | `ProgressBar.prefab` | 无 | 战斗 HUD 会复用，可先做 |

---

## 4. P1 · ToastView.prefab

```text
ToastView                       [RectTransform 640×72]
  ├─ Image     (common_toast_bg_9s, Sliced, 全屏拉伸)   ← 与 ToastView 同物体即可
  ├─ CanvasGroup
  ├─ ToastView (脚本)
  └─ Message   [TextMeshProUGUI]
```

| 节点 | 组件 | 参数 |
|---|---|---|
| `ToastView` | RectTransform | 宽 640、高 72；LayoutElement 可留空（由 ToastLayer 的布局组控制） |
| | Image | Sprite = `common_toast_bg_9s`，Type = Sliced，Border 30/30/30/30 |
| | CanvasGroup | Alpha = 1（脚本会控制） |
| | ToastView | 见下方绑定 |
| `Message` | TextMeshProUGUI | 字体 `TMP_Body`，字号 24，颜色 `#C8BFA8`，Left/Center 对齐，四周 padding 20/12；**不要**勾选 Raycast Target |

绑定：

| 字段（Inspector 显示） | 拖入 |
|---|---|
| Canvas Group | ToastView 自身的 CanvasGroup |
| Text | `Message` |

注意：ToastView 只控制 Alpha，位置由 `ToastLayer` 的 VerticalLayoutGroup 负责；不要给 ToastView 加 LayoutGroup。

---

## 5. P2 · ConfirmPopup.prefab

```text
ConfirmPopup                    [RectTransform 560×320]
  ├─ Image      (common_popup_bg_9s, Sliced)
  ├─ ConfirmPopup (脚本)
  ├─ Title      [TextMeshProUGUI]
  ├─ Body       [TextMeshProUGUI]
  └─ Buttons    [RectTransform + HorizontalLayoutGroup]
      ├─ ConfirmButton  [Image(button_primary_normal) + Button]
      │   └─ Label      [TextMeshProUGUI]
      └─ CancelButton   [Image(button_secondary_normal) + Button]
          └─ Label      [TextMeshProUGUI]
```

布局建议（1920×1080 基准）：

| 节点 | 锚点 / 尺寸 |
|---|---|
| `ConfirmPopup` | 居中，560×320（Image 用 Sliced，border 64/64/64/64） |
| `Title` | 顶部拉伸：Left 48 / Top 44 / Right 48 / Height 48；字号 32，`TMP_Title`，符金 |
| `Body` | Left 48 / Top 104 / Right 48 / Height 120；字号 22，`TMP_Body`，自动换行 |
| `Buttons` | 底部拉伸：Left 48 / Bottom 40 / Right 48 / Height 72；Spacing 24，Child Alignment = Middle Center |
| `ConfirmButton` / `CancelButton` | LayoutElement Preferred Width 200 / Height 72；Image Type = Sliced（border 40/36/40/36） |

按钮四态：`Button` 的 Transition = **Sprite Swap**，按素材命名对应填：

| 状态 | primary | secondary | danger |
|---|---|---|---|
| Normal | `button_primary_normal` | `button_secondary_normal` | `button_danger_normal` |
| Highlighted | `button_primary_hover` | `button_secondary_hover` | `button_danger_hover` |
| Pressed | `button_primary_pressed` | `button_secondary_pressed` | `button_danger_pressed` |
| Disabled | `button_primary_disabled` | `button_secondary_disabled` | `button_danger_disabled` |

绑定：

| 字段 | 拖入 |
|---|---|
| Title Text | `Title` |
| Body Text | `Body` |
| Confirm Label Text | `ConfirmButton/Label` |
| Cancel Label Text | `CancelButton/Label` |
| Confirm Button | `ConfirmButton` 的 Button |
| Cancel Button | `CancelButton` 的 Button |

注意：弹窗关闭由调用方 `UIRoot.Confirm()` 统一处理，**不要**在按钮上再挂关闭逻辑，否则会重复关闭。

---

## 6. P3 · TooltipView.prefab

```text
TooltipView                     [RectTransform 380×220, Pivot(0,1)]
  ├─ Image       (common_tooltip_bg_9s, Sliced)
  ├─ CanvasGroup
  ├─ TooltipView (脚本)
  ├─ ContentSizeFitter         (Vertical Fit = Preferred Size)
  ├─ VerticalLayoutGroup       (Padding 16, Spacing 6, Child Force Expand Width)
  ├─ Title       [TextMeshProUGUI]
  └─ Body        [TextMeshProUGUI]
```

| 项 | 值 |
|---|---|
| Pivot | **(0, 1)**，这样锚点跟随鼠标右下方向展开 |
| ContentSizeFitter | Vertical Fit = Preferred Size（Horizontal 用 Unconstrained，宽度固定 380） |
| Title | 字号 26，`TMP_Title`，符金 `#C2AA72` |
| Body | 字号 20，`TMP_Body`，灰纸 `#C8BFA8`，自动换行，Raycast Target 关闭 |
| 整个 TooltipView | **所有 TMP 与 Image 的 Raycast Target 全部关闭**，避免浮层挡住鼠标导致闪烁 |

绑定：

| 字段 | 拖入 |
|---|---|
| Canvas Group | 自身 CanvasGroup |
| Title Text | `Title` |
| Body Text | `Body` |
| Cursor Offset | 默认 (18, -18)，按需要调 |

---

## 7. P4 · UIRoot.prefab（主骨架）

```text
UIRoot                          [RectTransform 全屏, Canvas, CanvasScaler, GraphicRaycaster]
  ├─ UIRoot (脚本)
  ├─ UIInputBlocker (脚本)
  ├─ BackgroundLayer            [RectTransform 全屏]
  │   ├─ BackgroundImage        [Image: bg_page (Simple, 全屏拉伸)]
  │   └─ PaperFiber             [Image: common_decor_paper_fiber, Tiled, Color.a = 0.2]
  ├─ PageLayer                  [RectTransform 全屏]
  │   └─ UIPageService (脚本)
  ├─ PopupLayer                 [RectTransform 全屏]
  │   ├─ UIPopupService (脚本)
  │   └─ Overlay                [Image: common_overlay_dim, 全屏拉伸, 默认 SetActive(false)]
  │       └─ Button (可选: 点击遮罩关闭 → UIPopupService.CloseTop)
  ├─ TooltipLayer               [RectTransform 全屏]
  │   └─ TooltipView            (P3 的内容直接展开在这里)
  ├─ ToastLayer                 [RectTransform 顶部居中, VerticalLayoutGroup, ContentSizeFitter]
  │   └─ UIToastService (脚本)
  ├─ AnimationLayer             [RectTransform 全屏]
  └─ InputBlockLayer            [Image 全屏拉伸, Color(0,0,0,0), Raycast Target ✔, 默认 SetActive(false)]
```

### 7.1 各层参数

| 层 | 设置 |
|---|---|
| `BackgroundLayer` | 全屏拉伸；`BackgroundImage` 用 `bg_page`，Type = Simple，Preserve Aspect 关；页面可自行覆盖为 `bg_battle` / `bg_shop` 等 |
| `PaperFiber` | Type = Tiled，Color = `#C8BFA8`，Alpha 0.2；Raycast Target 关 |
| `PageLayer` | 全屏拉伸，只放一级页面 Prefab 实例 |
| `PopupLayer` | 全屏拉伸 |
| `Overlay` | Type = Simple 拉伸；Color 白（素材本身已带 78% 黑）；**默认关闭**，由 `UIPopupService` 控制 |
| `ToastLayer` | 锚点顶部拉伸（Left 0 / Top 80 / Right 0 / Height 自适应）；VerticalLayoutGroup：Spacing 10、Child Alignment = Upper Center、Child Force Expand Width = false、Height = true；ContentSizeFitter Vertical Fit = Preferred Size |
| `TooltipLayer` | 全屏拉伸（浮层内部自己夹边界） |
| `AnimationLayer` | 全屏拉伸，**不挂任何业务脚本**，只放伤害数字/飞行卡牌等临时对象 |
| `InputBlockLayer` | 全屏拉伸；Image Color = (0,0,0,0)（完全透明但可点击）；Raycast Target ✔；默认关闭 |

> 输入锁与遮罩的分工：`InputBlockLayer` 用于**动画播放期间**（无视觉），`Overlay` 用于**弹窗期间**（有视觉）。两者可同时生效。

### 7.2 组件挂载与字段绑定

`UIRoot` 组件（挂在 UIRoot 根物体）：

| 字段 | 拖入 |
|---|---|
| Background Layer | `BackgroundLayer` |
| Page Layer | `PageLayer` |
| Popup Layer | `PopupLayer` |
| Tooltip Layer | `TooltipLayer` |
| Toast Layer | `ToastLayer` |
| Animation Layer | `AnimationLayer` |
| Input Blocker | 根物体上的 `UIInputBlocker` |
| Page Service | `PageLayer` 上的 `UIPageService` |
| Popup Service | `PopupLayer` 上的 `UIPopupService` |
| Toast Service | `ToastLayer` 上的 `UIToastService` |
| Tooltip | `TooltipLayer/TooltipView` 上的 `TooltipView` |
| Confirm Popup Prefab | `Assets/Resources/Prefab/UI/ConfirmPopup.prefab` |

> 漏拖也能跑：`UIRoot.Awake()` 会按上表的节点名自动补齐层与服务引用；**但 `Confirm Popup Prefab` 必须手工拖**，否则 `Confirm()` 会退化为直接执行确认。

`UIInputBlocker` 组件（挂在 UIRoot 根物体）：

| 字段 | 拖入 |
|---|---|
| Blocker Layer | `InputBlockLayer` |

> 也可以把 `UIInputBlocker` 改挂到 `InputBlockLayer` 自身，此时 `Blocker Layer` 留空会自己禁用自己——**不推荐**，按上表挂在根物体最稳。

`UIPopupService` 组件（挂在 `PopupLayer`）：

| 字段 | 拖入 / 值 |
|---|---|
| Popup Layer | 留空（Awake 自动用自身） |
| Overlay | `PopupLayer/Overlay` |
| Input Blocker | 留空（自动向上查找 UIRoot 上的 `UIInputBlocker`） |
| Close On Escape | ✔ |

`UIToastService` 组件（挂在 `ToastLayer`）：

| 字段 | 拖入 / 值 |
|---|---|
| Toast Layer | 留空（Awake 自动用自身） |
| Toast Prefab | `Assets/Resources/Prefab/UI/ToastView.prefab` |
| Max Visible | 3 |

### 7.3 场景装配

1. 场景里放一个 `EventSystem`（`StandaloneInputModule`）——`UIRoot.prefab` 内不含它。
2. 把 `UIRoot.prefab` 实例化进场景（或运行时 `Resources.Load` 后 Instantiate）。
3. **重要**：现有 `BattlePrototypeBootstrap` 会在运行时自动创建整套原型 UI（`RuntimeInitializeOnLoadMethod`）。验证新 UI 前先临时禁用它（把 `Assets/Scripts/Unity/BattlePrototypeBootstrap.cs` 里的 `AutoCreate` 注释掉，或把它从场景/工程中临时移出），否则两套界面会重叠。
4. 快速验证脚本（可临时用）：

```csharp
// 挂任意物体，进入 Play 后按 H 弹提示、按 C 弹确认框
private void Update()
{
    if (Input.GetKeyDown(KeyCode.H)) UIRoot.Instance.Toast("灵力不足");
    if (Input.GetKeyDown(KeyCode.C)) UIRoot.Instance.Confirm("放弃本局", "当前进度将丢失，确定放弃？", () => Debug.Log("确认"));
}
```

---

## 8. P5 · ProgressBar.prefab（战斗 HUD 复用件）

```text
ProgressBar                     [RectTransform 320×40, UIProgressBar]
  ├─ Track         [Image: bar_track_9s, Sliced, 全屏拉伸]
  ├─ DelayedFill   [Image: bar_fill_hp, Filled/Horizontal, 全屏拉伸(内缩 4px)]
  ├─ Fill          [Image: bar_fill_hp, Filled/Horizontal, 全屏拉伸(内缩 4px)]
  └─ Value         [TextMeshProUGUI, 居中]
```

| 项 | 值 |
|---|---|
| Track | Border 20/16/20/16；Type = Sliced |
| DelayedFill / Fill | Image Type = **Filled**，Fill Method = Horizontal，Fill Origin = Left，Fill Amount = 1 |
| DelayedFill 颜色 | `#C8BFA8`，Alpha 0.55（露出"刚失去的部分"） |
| Fill 颜色 | 白色（用素材原色）；不同用途换素材：气血 `bar_fill_hp`、敌方 `bar_fill_enemy_hp`、护盾 `bar_fill_shield`、灵力 `bar_fill_energy` |
| Value | 字号 20，`TMP_Number`，灰纸色 |

绑定：

| 字段 | 拖入 | 说明 |
|---|---|---|
| Fill | `Fill` | 主填充 |
| Delayed Fill | `DelayedFill` | 可留空，留空则不显示延迟条 |
| Value Text | `Value` | 可留空 |
| Value Format | `{0}/{1}` | 默认值 |

用法（战斗 HUD 里）：

```csharp
_hpBar.SetValue(state.Player.Hp, state.Player.MaxHp);        // 带过渡动画
_hpBar.SetValue(state.Player.Hp, state.Player.MaxHp, false); // 立即跳变（恢复存档等）
_shieldBar.SetNormalized(0.4f);
```

---

## 9. 自检清单

搭完每个 Prefab 后逐条确认：

- [ ] 所有 Image 的 Raycast Target 只有可点击元素为开（文字、装饰、纸纹、Tooltip 全部关闭）。
- [ ] 面板/按钮/弹窗 Image 的 Type = Sliced，拉伸任意尺寸不变形。
- [ ] 按钮用 Sprite Swap 配齐四态；禁用态素材是 `*_disabled`。
- [ ] `ProgressBar` 的填充 Image 是 Filled/Horizontal，不是 Simple。
- [ ] `TooltipView` 的 Pivot 是 (0,1)，ContentSizeFitter 只开 Vertical。
- [ ] `ToastLayer` 用 VerticalLayoutGroup 排布，`ToastView` 自身不带布局组。
- [ ] `UIRoot` 的 `Confirm Popup Prefab` 已指向 `ConfirmPopup.prefab`。
- [ ] 场景里有 EventSystem，且原型 `BattlePrototypeBootstrap` 已临时禁用。
- [ ] Play 后：按 H 出 Toast（无遮挡、1.6 秒后淡出）；按 C 出确认框（出现时底层不可点）；Esc 关闭弹窗。

---

## 10. 阶段 3 · 战斗 HUD 代码状态

战斗 HUD 的 View 脚本已全部完成（`Assets/Scripts/UI/Battle/`），按**运行时实例化**设计：手牌、状态图标、法宝槽、符箓按钮都由对应 Panel 在运行时实例化模板 Prefab 并复用。

| 脚本 | 作用 |
|---|---|
| `BattleHudView.cs` | 总控：选牌、出卦、符箓、结束回合、反馈播放与输入锁 |
| `BattleCardView.cs` | 单张手牌（运行时实例化） |
| `HandPanelView.cs` | 手牌区布局与实例复用 |
| `HexagramAltarView.cs` | 内卦 / 外卦槽 |
| `HexagramPreviewView.cs` | 卦名、上下卦、效果、特殊卦与精通 |
| `DamagePreviewView.cs` | 最终伤害、段数、五行提示、可斩杀、伤害明细 |
| `PlayerStatusView.cs` | 玩家气血 / 护盾 / 灵力 / 状态 |
| `EnemyView.cs` | 敌人名称 / 类型 / 五行 / 气血 / 护盾 / 状态 |
| `EnemyIntentView.cs` | 敌人意图图标、文本与预计伤害 |
| `StatusBarView.cs` + `StatusChipView.cs` | 状态图标条（运行时实例化 Chip） |
| `ActionPanelView.cs` | 出卦 / 弃牌 / 结束回合 / 提示文本 |
| `ArtifactPanelView.cs` + `ArtifactSlotView.cs` | 法宝栏（运行时实例化槽位） |
| `TalismanPanelView.cs` + `TalismanButtonView.cs` | 符箓栏（运行时实例化按钮） |
| `WeaponSlotView.cs` | 武器与 3 个附魔槽（固定槽位，不实例化） |
| `UISpriteCatalog.cs` | 枚举 → 素材路径映射与缓存 |

**本批未接线**（代码里已标 `TODO`）：弃牌选择模式（`HandleDiscard` 只弹提示）、逐事件 `BattleEvent` 播放队列、伤害数字层、战斗日志。

### 10.1 运行时实例化的模板 Prefab

这几个 Prefab 只作为模板存在，**不要**直接放进场景：

| 模板 | 被谁实例化 | 数量 |
|---|---|---|
| `BattleCardView.prefab` | `HandPanelView` | 手牌上限 6 |
| `StatusChipView.prefab` | `StatusBarView`（玩家 / 敌人各一份） | 按状态数 |
| `ArtifactSlotView.prefab` | `ArtifactPanelView` | 至少 4 个槽 |
| `TalismanButtonView.prefab` | `TalismanPanelView` | 按符箓数 |

---

## 11. 阶段 3 · 战斗 HUD Prefab 制作说明

### 11.1 P6 · BattleCardView.prefab（手牌模板）

```text
BattleCardView                  [RectTransform 200×300, BattleCardView]
  ├─ Frame      [Image card_frame_rank1_9s, Sliced, 全屏拉伸]
  ├─ Glyph      [Image bagua_qian, 居中 120×120]
  ├─ NameText   [TextMeshProUGUI, 顶部]
  ├─ QiText     [TextMeshProUGUI, 中下]
  ├─ RankText   [TextMeshProUGUI, 右上]
  ├─ InnerTag   [Image card_tag_inner, 左上角]  ← 默认关闭
  ├─ OuterTag   [Image card_tag_outer, 右上角]  ← 默认关闭
  └─ DisabledOverlay [Image 全屏拉伸, Color(0,0,0,0.45)] ← 默认关闭
```

| 字段 | 拖入 |
|---|---|
| Frame | `Frame` 的 Image |
| Glyph | `Glyph` 的 Image |
| Name Text | `NameText` |
| Qi Text | `QiText` |
| Rank Text | `RankText` |
| Inner Tag | `InnerTag` 对象 |
| Outer Tag | `OuterTag` 对象 |
| Disabled Overlay | `DisabledOverlay` 对象 |

要点：

- 帧图由代码按「禁用 / 选中 / 品阶」自动换：`card_frame_disabled_9s`、`card_frame_selected_9s`、`card_frame_rank{1,2,3}_9s`，**不要把 frame 写死**。
- 卡牌自身要能接收点击：`Frame` 或根节点挂 `Image` + **Raycast Target 开启**，由 `BattleCardView` 实现 `IPointerClickHandler`。
- **不要**在这张卡上加 `Button` 组件，点击由脚本处理（避免与 Button 的 interactable 冲突）。

### 11.2 P7 · StatusChipView.prefab（状态图标模板）

```text
StatusChipView                  [RectTransform 64×64, StatusChipView]
  ├─ Background  [Image common_slot_empty_9s, Sliced, 全屏拉伸]
  ├─ Icon        [Image status_burn, 居中 44×44]
  ├─ StacksText  [TextMeshProUGUI, 右下角, 字号 18]
  ├─ TurnsText   [TextMeshProUGUI, 左下角, 字号 14]
  └─ TooltipTrigger (脚本, 可选)
```

| 字段 | 拖入 |
|---|---|
| Icon | `Icon` 的 Image |
| Stacks Text | `StacksText` |
| Turns Text | `TurnsText` |
| Tooltip | 同物体上的 `TooltipTrigger`（可留空） |

### 11.3 P8 · ArtifactSlotView.prefab / TalismanButtonView.prefab

```text
ArtifactSlotView                [RectTransform 120×120, ArtifactSlotView]
  ├─ Background  [Image common_slot_empty_9s, Sliced, 全屏拉伸]
  ├─ NameText    [TextMeshProUGUI, 居中, 字号 18, 自动换行]
  ├─ EmptyState  [TextMeshProUGUI “空” , 默认关闭]  ← 可留空
  └─ TooltipTrigger (脚本, 可选)
```

```text
TalismanButtonView              [RectTransform 150×64, TalismanButtonView]
  └─ Button      [Image button_secondary_normal + Button(Sprite Swap 四态)]
      └─ Label   [TextMeshProUGUI, 字号 22]
```

| Prefab | 字段 | 拖入 |
|---|---|---|
| ArtifactSlotView | Background / Name Text / Empty State / Tooltip | 对应节点 |
| TalismanButtonView | Button / Label / Tooltip | `Button` 组件、`Label`、`TooltipTrigger` |

法宝与符箓图标素材尚未制作（见 [UI制作文档.md](UI制作文档.md) §8），当前用名称文本表示；图标补上后在 `ArtifactSlotView` 增加一个 `Image _icon` 字段即可。

### 11.4 P9 · BattleHudView.prefab（战斗主界面）

```text
BattleHudView                   [RectTransform 全屏, BattleHudView]
  ├─ TopBar                     [RectTransform 顶部拉伸, 高 150]
  │   ├─ PlayerStatus           [Image common_panel_page_9s, PlayerStatusView]
  │   │   ├─ Name               [TMP]
  │   │   ├─ HpBar              [UIProgressBar]
  │   │   ├─ ShieldBar          [UIProgressBar]
  │   │   ├─ EnergyBar          [UIProgressBar]
  │   │   └─ StatusBar          [StatusBarView + HorizontalLayoutGroup + SlotRoot]
  │   ├─ TurnText               [TMP, 居中]
  │   └─ EnemyPanel             [Image common_panel_enemy_9s]
  │       ├─ EnemyView          [EnemyView]
  │       │   ├─ Portrait       [Image, 可留空]
  │       │   ├─ Name           [TMP]
  │       │   ├─ KindText       [TMP]
  │       │   ├─ ElementIcon    [Image]
  │       │   ├─ ElementText    [TMP]
  │       │   ├─ HpBar          [UIProgressBar]
  │       │   ├─ ShieldBar      [UIProgressBar]
  │       │   └─ StatusBar      [StatusBarView]
  │       └─ EnemyIntent        [EnemyIntentView]
  │           ├─ Root           [RectTransform]
  │           ├─ Icon           [Image]
  │           ├─ Text           [TMP]
  │           └─ PowerText      [TMP]
  ├─ CenterArea                 [RectTransform 中部弹性]
  │   ├─ Altar                  [Image common_panel_inset_9s, HexagramAltarView]
  │   │   ├─ InnerSlot          [RectTransform]
  │   │   │   ├─ Glyph          [Image]
  │   │   │   ├─ NameText       [TMP]
  │   │   │   └─ EmptyHint      [TMP “请选择内卦”]
  │   │   └─ OuterSlot          [RectTransform]（同上三件）
  │   ├─ HexagramPreview        [Image common_panel_inset_9s, HexagramPreviewView]
  │   │   ├─ NameText / CompositionText / EffectText / MasteryText [TMP]
  │   │   └─ SpecialMark        [Image common_decor_seal_red, 默认关闭]
  │   └─ DamagePreview          [DamagePreviewView]
  │       ├─ DamageText         [TMP, 大号等宽]
  │       ├─ HitCountText       [TMP]
  │       ├─ ElementHintText    [TMP]
  │       ├─ LethalMark         [TMP “可斩杀”, 默认关闭]
  │       ├─ BreakdownRoot      [RectTransform, 默认关闭]
  │       │   └─ FormulaText    [TMP]
  │       └─ ToggleBreakdownButton [Button, 可留空]
  ├─ ResourceArea               [RectTransform 中部下方]
  │   ├─ ArtifactPanel          [ArtifactPanelView + HorizontalLayoutGroup]
  │   │   └─ SlotRoot           [RectTransform]
  │   ├─ TalismanPanel          [TalismanPanelView + HorizontalLayoutGroup]
  │   │   └─ ButtonRoot         [RectTransform]
  │   └─ WeaponSlot             [Image common_panel_section_9s, WeaponSlotView]
  │       ├─ WeaponName / PatternText [TMP]
  │       ├─ Enchant1 / Enchant2 / Enchant3 [TMP]
  │       └─ ReplaceHint        [TMP “将顶替最旧附魔”, 默认关闭]
  └─ BottomBar                  [RectTransform 底部拉伸, 高 320]
      ├─ HandPanel              [HandPanelView]
      │   └─ CardRoot           [RectTransform + HorizontalLayoutGroup(Spacing 12, Middle Center)]
      └─ ActionPanel            [ActionPanelView]
          ├─ PlayButton         [Image button_primary_normal + Button(Sprite Swap)]
          │   └─ Label          [TMP]
          ├─ DiscardButton      [Image button_secondary_normal + Button]
          │   └─ Label          [TMP]
          ├─ EndTurnButton      [Image button_danger_normal + Button]
          │   └─ Label          [TMP]
          └─ HintText           [TMP]
```

`BattleHudView` 字段绑定：

| 字段 | 拖入 |
|---|---|
| Altar | `CenterArea/Altar` |
| Hexagram Preview | `CenterArea/HexagramPreview` |
| Damage Preview | `CenterArea/DamagePreview` |
| Hand Panel | `BottomBar/HandPanel` |
| Action Panel | `BottomBar/ActionPanel` |
| Player Status | `TopBar/PlayerStatus` |
| Enemy View | `TopBar/EnemyPanel/EnemyView` |
| Enemy Intent View | `TopBar/EnemyPanel/EnemyIntent` |
| Artifact Panel | `ResourceArea/ArtifactPanel` |
| Talisman Panel | `ResourceArea/TalismanPanel` |
| Weapon Slot | `ResourceArea/WeaponSlot` |
| Turn Text | `TopBar/TurnText` |
| Feedback View | UIRoot 的 `AnimationLayer/BattleFeedbackView` |

各子 View 的字段绑定在其自身章节（§8 与本文各条）已给出；`PlayerStatusView`、`EnemyView`、`StatusBarView` 的绑定与前文 `UIProgressBar`（§8）一致：Bar 的 `Fill` / `DelayedFill` / `ValueText` 三个字段。

### 11.5 装配顺序与注意

1. 先做模板 Prefab（§10.1 的四个），再搭 `BattleHudView`，最后把 `BattleCardView.prefab` 拖到 `HandPanel` 的 **Card Prefab** 字段。
2. `HandPanelView` 的 `CardRoot` 要有 `HorizontalLayoutGroup`（Spacing 12、Child Alignment = Middle Center、Child Force Expand 关），位置由布局组负责，代码不设置坐标。
3. `BattleCardView` 的模板 Prefab 在实例化时会先 `SetActive(false)`，由 `HandPanelView` 按手牌数量启用。
4. `BattleFeedbackView` 放在 `UIRoot/AnimationLayer` 下，需要 `Text` + `RectTransform`（脚本会自动补 `CanvasGroup`），并把它拖到 `BattleHudView` 的 **Feedback View**。
5. 界面用法（由上层局内流程调用）：

```csharp
// 拿到 BattleController 后一次性绑定，之后命令与刷新都在 HUD 内部完成
UIRoot.Instance.Pages.Open(battleHudView);
battleHudView.Bind(battleController);
battleHudView.TalismanCommand = index => runController.UseTalisman(battleController, index); // 同步本局符箓列表
```

### 11.6 战斗 HUD 自检清单

- [ ] 手牌 0–6 张时只改变间距，卡牌尺寸与文字不变。
- [ ] 点第一张牌标内卦（灵青 + 内卦角标），点第二张标外卦；再点已选的牌可取消。
- [ ] 内外卦都选好后，预览区显示卦名、上下卦、最终伤害与五行提示；可斩杀时出绿标。
- [ ] 出卦后伤害数字与预览一致，反馈浮层播放期间按钮变灰且不能重复点。
- [ ] 灵力为 0 时出卦按钮禁用，提示区显示「灵力不足」。
- [ ] 结束回合后敌人按意图行动，回合结算浮层区分敌方、我方与护盾变化。
- [ ] 状态图标层数与剩余回合正确，法宝栏空槽与已持有状态区分清楚。
- [ ] 附魔槽满且本手会写入附魔时显示「将顶替最旧附魔」。

---

## 12. 变更记录

| 版本 | 日期 | 说明 |
|---|---|---|
| v0.2 | 2026-09-10 | 追加阶段 3：战斗 HUD 脚本清单、四个模板 Prefab（P6–P8）、`BattleHudView`（P9）层级树与字段绑定、装配顺序与自检清单 |
| v0.1 | 2026-09-10 | 初稿：阶段 1+2 代码清单、P1–P5 Prefab 制作说明、场景装配与自检清单 |
