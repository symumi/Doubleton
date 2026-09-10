# 《卦锻》UI 制作文档

> 版本：v0.1（2026-09-10）｜状态：待评审
> 依赖：[视觉设计风格.md](视觉设计风格.md)、[program/04-UI与交互实现.md](../program/04-UI与交互实现.md)、[program/06-模块开发计划与步骤.md](../program/06-模块开发计划与步骤.md)
> 素材目录：`Assets/Resources/Texture/UI/`｜素材源：`docs/design/ui_source/`

---

## 1. 文档用途

本文档用于把「视觉规范」和「UI 功能清单」落成可执行的制作计划，回答四件事：

1. **做什么**：需要制作哪些页面、组件、素材。
2. **按什么顺序做**：阶段划分与每阶段验收。
3. **长什么样**：风格、配色、材质、命名、导入设置。
4. **怎么动**：每个界面元素的动效、时长、触发条件与实现约定。

> **Prefab 制作说明**：需要手工搭建的 Prefab（层级树、组件参数、字段绑定、素材引用、场景装配、自检清单）见 [UI-Prefab制作说明.md](UI-Prefab制作说明.md)。阶段 1 与阶段 2 的代码已就绪，按该说明搭 Prefab 即可接线。

第一批 80 个基础 PNG 素材已经生成并放入 `Assets/Resources/Texture/UI/`，可直接用于搭 Prefab。素材是**程序化绘制的基础件**，用于先打通结构与手感；后续美术按「同名、同尺寸、同九宫格边框」替换即可，不需要改 Prefab。

---

## 2. 风格基线（摘录自视觉设计风格.md）

### 2.1 一句话风格

玩家不是在操作一套干净的修仙 UI，而是在翻阅一本被妖异侵蚀的《异志录》：**玩家可控的区域必须是秩序**（手牌、灵力、按钮、伤害预览），**世界与外部系统必须是污染**（敌人、未知、锁区、渡劫）。

### 2.2 双层模型

| 层级 | 谁在用 | 允许脏 | 必须清晰 |
|---|---|---|---|
| 约束层 | 玩家操作 | 否 | 手牌、灵力、按钮、伤害预览、选中状态 |
| 杂乱层 | 敌人、世界、外部规则 | 是 | HP、意图、五行、危险提示 |

落到素材上的对应关系：

- 约束层用 `common_panel_page_9s` / `common_panel_inset_9s` / `button_*` / `card_frame_*` / `frame_select_*`。
- 杂乱层用 `common_panel_enemy_9s` / `common_decor_ink_corner` / `common_decor_seal_red` / `common_decor_scanline`。

### 2.3 调色板

| 色名 | 色值 | 用途 | 素材中的出现位置 |
|---|---|---|---|
| 墨骨黑 | `#070808` | 页面底、最深阴影、墨池 | 所有素材的底与描边 |
| 夜宣 | `#111210` | 面板底、册页背面 | 面板、卡牌、弹窗底 |
| 灰纸 | `#C8BFA8` | 正文、纸纤维、未强调文字 | 高光线、纸纹、水印 |
| 符金 | `#C2AA72` | 卦纹、边界、品阶 | 面板边、按钮主行动、星标、爻线 |
| 朱砂 | `#B63A31` | 渡劫、危险、未解锁、封印 | 危险按钮、红印、敌人边 |
| 灵青 | `#5EC8C0` | 玩家选中、可控资源、扫描线 | 选中框、灵力、扫描线、封印短线 |

比例：墨黑/黑底 60%、灰纸 20%、符金 10%、朱砂 5%、灵青与其他 5%。**朱砂与灵青是信息色，一次界面里只用于状态变化，不做大面积装饰。**

### 2.4 字体与数字

| 内容 | 字体 | 规则 |
|---|---|---|
| 大标题、卦名、境界名 | 楷体 / 宋体类 | 字距略宽，不做书法长卷 |
| 正文、按钮、说明 | 思源黑体 / 微软雅黑 | 保证小字号可读 |
| 数值、伤害、资源 | 等宽字体 | 数字必须一眼可比较 |
| 未解锁文本 | 黑体或楷体 | 降低对比但保持可读 |

TMP 需要生成 SDF 字体资产：`TMP_Title`（楷/宋）、`TMP_Body`（黑体）、`TMP_Number`（等宽）三套。所有素材**不含文字**，文字一律由 TMP 叠加。

### 2.5 材质与图形语言

- **水墨**：面板内部保持干净，墨渍只出现在外框与四角。
- **卦象**：六爻线横平竖直、粗细一致；预览区优先显示「上卦/下卦」而不是只显示卦名。
- **册页**：面板 = 一页志怪册页，允许印章、编号、右下「志」字水印（`common_watermark_zhi`）。
- **轻微赛博**：只有灵青扫描线（`common_decor_scanline`）、等宽数字、1px 错位；禁止霓虹与全息。
- **微恐**：用「看不清 / 记录缺失 / 异常重复」制造不安，不依赖血腥。

### 2.6 禁止项

- 不要把所有界面做成纯黑加金色。
- 不要让赛博元素变成霓虹科幻。
- 不要让微恐、墨渍遮挡 HP、灵力、价格、意图和按钮。
- 不要用书法字体替代正文。
- 不要为了「水墨」牺牲 64 卦矩阵、手牌和数值比较效率。

---

## 3. 素材体系

### 3.1 目录与命名规范

```text
Assets/Resources/Texture/UI/
  Common/      通用面板、装饰、遮罩、牌背
  Button/      按钮四态（normal / hover / pressed / disabled）
  Frame/       选中框、卡牌底框、内外卦角标
  Bar/         生命 / 护盾 / 灵力条
  Icon/        资源、五行、状态、意图、境界徽记
  Hexagram/    爻线与八卦符号
  Background/  1920×1080 页面背景
```

命名规则（全部小写英文 + 下划线）：

- `<类别前缀>_<对象>[_<变体>][_9s].png`
- `_9s` 后缀 = 九宫格素材（Sliced）；无后缀 = 整图或平铺。
- 状态/等级写在末尾：`button_primary_hover.png`、`card_frame_rank3_9s.png`、`realm_seal_2.png`。
- 素材名与 Prefab 内节点名保持一致，便于查找替换。

### 3.2 素材分类汇总

| 类别 | 数量 | 代表素材 | 说明 |
|---|---:|---|---|
| Common | 16 | `common_panel_page_9s`、`common_popup_bg_9s`、`common_card_back` | 面板/弹窗/提示/遮罩/纸纹/墨渍 |
| Button | 12 | `button_primary_normal` | 主/次/危险 × 常态/悬停/按下/禁用 |
| Frame | 10 | `card_frame_rank2_9s`、`card_tag_inner` | 选中框、卡牌底框、内外卦角标 |
| Bar | 5 | `bar_fill_hp`、`bar_fill_energy` | 条底 + 4 种填充 |
| Icon | 21 | `element_fire`、`status_burn`、`intent_attack` | 资源 4 + 五行 5 + 状态 6 + 意图 3 + 境界 3 |
| Hexagram | 10 | `hex_line_yang`、`bagua_qian` | 阴阳爻 + 8 卦符号 |
| Background | 6 | `bg_battle`、`bg_shop` | 主菜单/路线/战斗/坊市/图鉴/通用 |

**全量清单（80 项，含尺寸、九宫格边框、用途、配色）见 [ui_source/ui_manifest.md](ui_source/ui_manifest.md)。**

**一图速览：** [ui_source/ui_preview_sheet.png](ui_source/ui_preview_sheet.png)（80 张素材缩略图总览，格子背景为棋盘格，用于检查透明边缘）。

重点素材速查：

| 用途 | 素材 | 尺寸 | 九宫格 (L,B,R,T) |
|---|---|---|---|
| 一级页面底板 | `common_panel_page_9s` | 256×256 | 48,48,48,48 |
| 卦台/手牌内嵌底 | `common_panel_inset_9s` | 256×256 | 48,48,48,48 |
| 敌人区底板 | `common_panel_enemy_9s` | 256×256 | 48,48,48,48 |
| 弹窗 | `common_popup_bg_9s` | 384×384 | 64,64,64,64 |
| 悬停说明 | `common_tooltip_bg_9s` | 256×256 | 48,48,48,48 |
| 短提示 | `common_toast_bg_9s` | 320×96 | 30,30,30,30 |
| 主行动按钮 | `button_primary_normal` | 320×96 | 40,36,40,36 |
| 选中外框 | `frame_select_spirit_9s` | 160×160 | 40,40,40,40 |
| 卡牌底框（3 星） | `card_frame_rank3_9s` | 256×384 | 40,40,40,40 |
| 生命条 | `bar_fill_hp` | 160×40 | 20,16,20,16 |

### 3.3 Unity 导入设置

所有 PNG 统一设置：

| 项 | 值 | 原因 |
|---|---|---|
| Texture Type | Sprite (2D and UI) | uGUI Image 使用 |
| Sprite Mode | Single | 一图一精灵 |
| Mesh Type | **Full Rect** | 九宫格与 Tiled 必须 Full Rect |
| Pixels Per Unit | 100 | 与 CanvasScaler 基准一致 |
| Extrude Edges | 1 | 避免缩放缝隙 |
| Wrap Mode | Clamp（`common_decor_scanline`、`common_decor_paper_fiber` 用 Repeat） | 平铺素材需要 |
| Filter Mode | Bilinear | 平滑缩放 |
| Generate Mip Maps | 关 | UI 不需要 |
| Alpha Is Transparency | 开 | 边缘透明正确 |
| Compression | 面板/按钮/卡牌：High Quality；图标：Normal；背景：Normal | 九宫格边缘避免压缩噪点 |
| Max Size | 面板/卡牌/按钮 512；图标 256；背景 2048 | 控制内存 |

九宫格：在 Sprite Editor 里按清单的 `L,B,R,T` 填写 Border。**背景、`common_divider_ink`、装饰件、图标不切九宫格**（清单中标 `none`）。

平铺素材（扫描线、宣纸纤维）：Image Type 设 `Tiled`，Wrap Mode 用 Repeat，Type 为 Simple 时不要开启 Preserve Aspect。

### 3.4 素材来源与替换

素材由脚本生成，源文件是 SVG，可随时重做：

```powershell
python docs\design\ui_source\gen_base.py        # 面板/按钮/边框/进度条
python docs\design\ui_source\gen_symbols.py     # 图标/八卦/背景
python docs\design\ui_source\gen_manifest.py    # 合并清单，更新 ui_manifest.md
python docs\design\ui_source\gen_preview.py     # 生成素材总览页 preview.html
powershell -NoProfile -File docs\design\ui_source\render_png.ps1 -Force   # 渲染 PNG
powershell -NoProfile -File docs\design\ui_source\verify_png.ps1          # 校验尺寸与透明覆盖
```

替换规则：美术产出新图必须**同名、同尺寸、同九宫格边框**，直接覆盖同名 PNG 即可，Prefab 无需改动。若确需改尺寸或边框，先同步更新 `gen_*` 脚本与清单，再改 Prefab。

### 3.5 页面装配分层

```text
UIRoot (Canvas: Screen Space - Overlay, CanvasScaler 1920×1080, Match 0.5)
  ├─ BackgroundLayer   bg_*.png + common_decor_ink_corner + common_decor_scanline
  ├─ PageLayer         一级页面 Prefab（面板用 common_panel_page_9s 系列）
  ├─ PopupLayer        common_overlay_dim + common_popup_bg_9s
  ├─ TooltipLayer      common_tooltip_bg_9s（层级最高）
  ├─ ToastLayer        common_toast_bg_9s
  └─ AnimationLayer    伤害数字、飞行卡牌、状态图标、屏幕特效（无业务状态）
```

`common_decor_paper_fiber` 铺在 `BackgroundLayer` 之上、`PageLayer` 之下，`Image.color.a` 取 0.15–0.25。`common_decor_scanline` 只在敌人区、图鉴锁区、标题条局部使用，`Image.color.a` 取 0.2–0.4。

### 3.6 素材质量基线（v0.2 重做后生效）

第一版素材是"暗色块 + 细线"，缺少材质与语义，已整体重做。**后续新增或替换素材必须满足**：

| 要求 | 说明 |
|---|---|
| 至少三层 | 底色渐变 → 材质纹理（纸纤维/木纹/墨粒）→ 描边与高光/阴影 |
| 有体积 | 按钮、卡牌、徽记必须有高光与暗边，不能是平涂色块 |
| 边缘是墨不是线 | 使用噪声位移（枯笔）产生轻微毛边，位移量控制在 2–3px，过大会把边框打断成虚线 |
| 纸面可见 | 面板中心要有柔和的灰纸亮面（`#C8BFA8` 低透明度径向），不能是一片纯黑 |
| 背景要有层次 | 远/中/近三层山脊剪影 + 墨晕云雾 + 纸纹 + 暗角，禁止单色渐变铺满 |
| 图标要有语义 | 双层底盘承托主体；攻击=剑刃、防御=盾、减益=破碎，不用纯几何线条代指 |
| 颜色不偏绿 | 面板纸色用中性暖灰（`#2b2b26`→`#111211`），避免出现橄榄绿 |

生成脚本已按该基线实现：`gen_base.py` 的 `paper_filter` / `rough_filter` / `ink_blots` / `corner_brackets` / `wood_pattern`，`gen_symbols.py` 的 `badge_defs` / `disc_base` / `tile_base`。

资源加载路径（Sprite）：

```csharp
Resources.Load<Sprite>("Texture/UI/Common/common_panel_page_9s");
Resources.Load<Sprite>("Texture/UI/Icon/element_fire");
Resources.Load<Sprite>("Texture/UI/Background/bg_battle");
```

---

## 4. UI 制作顺序

顺序原则：**先骨架、再公共件、再战斗、再局内、最后局外与打磨**。每个阶段结束都要能在 Unity 里跑起来看，不允许整阶段只写代码看不到东西。

### 阶段 0：素材与规范（已完成）

- 产出：本文档、80 个 PNG 素材、可复现的生成脚本。
- 验收：素材在 Unity 内导入设置正确，九宫格拉伸不变形，图标透明边缘干净。

### 阶段 1：UI 基础设施

> 代码已完成（`Assets/Scripts/UI/`），Prefab 待制作，见 [UI-Prefab制作说明.md](UI-Prefab制作说明.md) §7。

| 顺序 | 产出 | 依赖素材 | 验收 |
|---|---|---|---|
| 1.1 | `UIRoot` Prefab（六层 + Canvas + CanvasScaler + EventSystem） | 无 | 1920×1080 与 1280×720 下层不错位 |
| 1.2 | `PageService` 页面栈 | 无 | 页面 A → B → 返回 A，状态不丢 |
| 1.3 | `PopupService` 弹窗栈 + 遮罩 | `common_overlay_dim` | 弹窗出现时底层不可点，Esc 可关 |
| 1.4 | `InputBlockLayer` 输入锁 | 无 | 动画/结算期间点击无效 |
| 1.5 | `UITextStyles`（TMP 三套字体 + 字号表） | 无 | 数值等宽、标题可读 |

### 阶段 2：公共组件

> 代码已完成（`ToastView` / `UIToastService` / `TooltipView` / `TooltipTrigger` / `ConfirmPopup` / `UIProgressBar` / `UITween` / `UIAnimationConfig`），Prefab 待制作，见 [UI-Prefab制作说明.md](UI-Prefab制作说明.md) §4–§8。

| 顺序 | 产出 | 依赖素材 | 验收 |
|---|---|---|---|
| 2.1 | `UiPanel`（九宫格面板 + 角标 + 水印） | `common_panel_*_9s`、`common_watermark_zhi` | 任意拉伸不变形 |
| 2.2 | `UiButton`（主/次/危险 × 四态） | `button_*` | 悬停/按下/禁用切换正确 |
| 2.3 | `ConfirmPopup` | `common_popup_bg_9s` | 放弃进度、覆盖存档、退出三类文案 |
| 2.4 | `ToastView`（队列，最多同时 3 条） | `common_toast_bg_9s` | 灵力不足等提示不叠加遮挡 |
| 2.5 | `TooltipView`（跟随鼠标 + 边界翻转） | `common_tooltip_bg_9s` | 屏幕边缘自动翻向内侧 |
| 2.6 | `ProgressBar`（条底 + 填充 + 延迟白条） | `bar_*` | 数值插值正确，护盾优先显示 |

### 阶段 3：战斗 HUD（最大工作量，先做垂直切片）

> View 脚本已完成（`Assets/Scripts/UI/Battle/`，按运行时实例化设计），Prefab 待制作，见 [UI-Prefab制作说明.md](UI-Prefab制作说明.md) §10–§11。弃牌选择模式与伤害数字层留到下一批（代码内已标 TODO）。

| 顺序 | 产出 | 依赖素材 | 验收 |
|---|---|---|---|
| 3.1 | `BattleHudView` 骨架（顶部/卦台/资源/手牌/行动五区） | `common_panel_page_9s`、`common_panel_inset_9s` | 与原型信息结构一致 |
| 3.2 | `BattleCardView` + `HandPanelView` | `card_frame_*_9s`、`card_tag_inner/outer`、`bagua_*` | 手牌 0–6 张间距自适应，内外卦标记清晰 |
| 3.3 | `HexagramAltarView` + `HexagramPreviewView` | `common_panel_inset_9s`、`hex_line_*` | 内外卦槽顺序明确，实时预览 |
| 3.4 | `DamagePreviewView` + `DamageBreakdownView` | `icon_hp`、`element_*` | 预览值与实际结算一致，可斩杀提示出现 |
| 3.5 | `PlayerStatusView` + `EnergyView` | `icon_hp`、`icon_shield`、`icon_energy`、`bar_*` | HP/护盾/灵力变化即时反映 |
| 3.6 | `EnemyView` + `EnemyIntentView` + `StatusBarView` | `common_panel_enemy_9s`、`status_*`、`intent_*`、`common_decor_scanline` | 意图与执行一致，状态层数可读 |
| 3.7 | `ArtifactPanelView` + `TalismanPanelView` | `common_panel_section_9s`、`common_slot_empty_9s` | 法宝栏满/符箓不可用有明确反馈 |
| 3.8 | `ActionPanelView` + `DiscardSelectView` | `button_*`、`frame_select_gold_9s` | 出卦/弃牌/结束回合状态机正确 |
| 3.9 | `DamageNumberLayer` + `AnimationLayer` | 无（用 TMP + 代码） | 伤害数字、飞行卡牌、屏幕震动独立于业务面板 |

### 阶段 4：局内页面

| 顺序 | 产出 | 依赖素材 | 验收 |
|---|---|---|---|
| 4.1 | `BattleResultView` | `common_panel_page_9s`、`icon_coin` | 奖励只结算一次 |
| 4.2 | `RunMapView` | `bg_route`、`realm_seal_*`、`common_decor_seal_red` | 节点状态（已通关/当前/未解锁）正确 |
| 4.3 | `ShopView` + `ShopPurchaseConfirmPopup` | `bg_shop`、`button_primary_*`、`icon_coin` | 已售出态、价格、上限提示正确 |
| 4.4 | `EventView` | `bg_page`、`common_panel_page_9s` | 选项只结算一次 |
| 4.5 | `RealmPromotionView` | `realm_seal_*`、`common_decor_seal_red` | 晋升奖励只发一次 |
| 4.6 | `RunResultView` | `bg_page`、`icon_coin` | 道行计算与展示一致 |
| 4.7 | `DeckViewerPopup` + `RewardSelectPopup` | `card_frame_*_9s`、`frame_select_spirit_9s` | 删牌/选牌写回正确 |
| 4.8 | `WeaponSelectView` | `common_panel_page_9s` | 开战后武器锁定 |

### 阶段 5：局外页面

| 顺序 | 产出 | 依赖素材 | 验收 |
|---|---|---|---|
| 5.1 | `MainMenuView` | `bg_main_menu`、`button_primary_*` | 有存档才显示「继续游戏」 |
| 5.2 | `CodexView`（8×8 矩阵 + 详情） | `bg_codex`、`bagua_*`、`frame_select_spirit_9s` | 未发现显示 `？？？`，熟练度/精通可见 |
| 5.3 | `MetaProgressView` | `common_panel_page_9s`、`icon_coin` | 天劫档位切换与解锁状态正确 |
| 5.4 | `SettingsView` + `PauseView` | `common_popup_bg_9s` | 战斗中暂停不丢状态 |
| 5.5 | `SaveRecoveryPopup` | `common_popup_bg_9s` | 主存档损坏时走备份 |
| 5.6 | `Boot/LoadingView` | `bg_page` | 加载失败有可读反馈 |

### 阶段 6：打磨

| 顺序 | 内容 | 验收 |
|---|---|---|
| 6.1 | `ui_animation.csv` 外部化全部动效参数 | 改 CSV 不改代码即可调节奏 |
| 6.2 | 逐事件 `BattleEvent` 播放队列 | 对齐、法宝、状态、抽牌按序播放 |
| 6.3 | 1280×720 / 超宽屏适配 | 无关键文字裁切、无按钮重叠 |
| 6.4 | 手柄导航（如进入 MVP） | Input System + 明确导航顺序 |
| 6.5 | 移除 `BattlePrototypeBootstrap` | 原型代码清零，测试全绿 |

**MVP 最小闭环**：阶段 1 + 2.1–2.3 + 阶段 3 全部 + 4.1–4.6 + 5.1、5.2。

---

## 5. 功能说明

### 5.1 一级页面

| 界面 | 展示信息 | 主要交互 | 关键素材 | 数据来源 |
|---|---|---|---|---|
| `Boot/LoadingView` | 加载进度、失败原因 | 重试 | `bg_page` | 配置/存档加载结果 |
| `MainMenuView` | 继续游戏、开始修行、图鉴、设置、退出 | 点击进入；有存档才显示继续 | `bg_main_menu`、`button_primary_*` | `SaveService` 存档存在性 |
| `WeaponSelectView` | 武器名、攻击模式、力道、五行亲和 | 三选一切换、确认开局 | `common_panel_page_9s` | `weapons.csv` |
| `RunMapView` | 9 节点、当前境界、灵石、天劫档位 | 开始战斗、查看节点详情 | `bg_route`、`realm_seal_*` | `RunState` / `run_nodes.csv` |
| `BattleHudView` | 见 5.3 | 出卦、弃牌、符箓、结束回合 | 全套战斗素材 | `BattleState` / `BattleEvent` |
| `BattleResultView` | 胜负、节点、灵石奖励 | 领取奖励 | `icon_coin` | `RunState.PendingBattleResult` |
| `ShopView` | 商品 4 位、价格、已购、刷新次数 | 购买、刷新、离开（二次确认） | `bg_shop`、`icon_coin` | `run_shop_offers.csv` / `RunState` |
| `EventView` | 事件文本、2–3 选项、结果 | 选择 → 结果 → 返回 | `bg_page` | `RunEncounterDatabase` |
| `RealmPromotionView` | 境界名、晋升奖励 | 确认晋升 | `realm_seal_*` | `run_realms.csv` |
| `RunResultView` | 胜/败、道行、统计 | 进入局外成长 | `bg_page` | `RunState.MetaProgress` |
| `MetaProgressView` | 累计道行、完成局数、天劫档位 | 上一档/下一档/重新开始 | `common_panel_page_9s` | `RunMetaProgressState` |
| `CodexView` | 8×8 卦阵、已发现数、详情面板 | 点格查看详情、筛选 | `bg_codex`、`bagua_*` | `HexagramCatalog` + 熟练度 |
| `SettingsView` | 音量、分辨率、语言、键位 | 修改即保存 | `common_popup_bg_9s` | 设置存档 |
| `PauseView` | 继续、设置、返回主菜单 | 暂停/恢复 | `common_overlay_dim` | 当前页面状态 |

### 5.2 弹窗与浮层

| 界面 | 用途 | 确认策略 | 关键素材 |
|---|---|---|---|
| `ConfirmPopup` | 放弃进度、覆盖存档、退出游戏 | 二次确认 | `common_popup_bg_9s`、`button_danger_*` |
| `ToastView` | 灵力不足、法宝栏已满、符箓不可用 | 自动消失 1.6s | `common_toast_bg_9s` |
| `TooltipView` | 卡牌/卦象/状态/法宝/武器说明 | 悬停 0.4s 后出现 | `common_tooltip_bg_9s` |
| `CardDetailPopup` | 牌面完整信息与来源 | 右键/长按 | `card_frame_*_9s` |
| `HexagramDetailPopup` | 卦象效果、倍率、五行、熟练度 | 长按卦台 | `hex_line_*`、`element_*` |
| `DamageBreakdownPopup` | 基础/卦象/五行/法宝/会心/防御 | 点击伤害数字 | `icon_hp`、`element_*` |
| `StatusDetailPopup` | 层数、持续回合、触发时机 | 悬停状态图标 | `status_*` |
| `RewardSelectPopup` | 三选一奖励、牌组奖励 | 选一确认 | `frame_select_spirit_9s` |
| `ShopPurchaseConfirmPopup` | 价格、拥有上限 | 确认购买 | `common_popup_bg_9s` |
| `DeckViewerPopup` | 牌组、排序、删牌模式 | 删牌二次确认 | `card_frame_*_9s` |
| `SettingsPopup` | 战斗中设置 | 即时生效 | `common_popup_bg_9s` |
| `SaveRecoveryPopup` | 主存档损坏时用备份恢复 | 明确选择 | `common_popup_bg_9s` |
| `TutorialPromptPopup` | 首次战斗/坊市引导 | 可跳过 | `common_popup_bg_9s` |

### 5.3 战斗 HUD 组件

| 组件 | 展示信息 | 交互与反馈 | 关键素材 |
|---|---|---|---|
| `PlayerStatusView` | HP、护盾、灵力、状态列表 | 数值变化动画；悬停状态看详情 | `icon_hp`、`icon_shield`、`icon_energy`、`bar_*`、`status_*` |
| `EnemyView` | 名称、HP、护盾、五行、精英/Boss 标记 | 受击抖动、死亡淡出 | `common_panel_enemy_9s`、`element_*` |
| `EnemyIntentView` | 意图类型、预计伤害、特殊规则 | 扫描线掠过提示刷新 | `intent_*`、`common_decor_scanline` |
| `RunRouteStripView` | 当前境界、节点进度、天劫档位 | 悬停看规则 | `realm_seal_*` |
| `HexagramAltarView` | 内卦槽、外卦槽 | 点击卡牌填入；点槽位可取消 | `common_panel_inset_9s`、`bagua_*`、`hex_line_*` |
| `HexagramPreviewView` | 上卦/下卦、卦名、效果、特殊卦标记 | 选择变化即时刷新 | `hex_line_*`、`common_panel_inset_9s` |
| `DamagePreviewView` | 预计伤害、段数、可斩杀、五行克制 | 可斩杀绿色标签；克制/被克配色不同 | `element_*`、`icon_hp` |
| `DamageBreakdownView` | 基础 × 卦象 × 五行 × 法宝 × 会心 = 最终 | 折叠/展开 | `common_divider_ink` |
| `HexagramAnimationView` | 内外卦合拢、卦名浮现、六爻亮起 | 播放期间锁输入 | `hex_line_*` |
| `ArtifactPanelView` | 法宝槽 ×4、触发提示 | 悬停看触发条件；触发时高亮 | `common_panel_section_9s`、`common_slot_empty_9s` |
| `TalismanPanelView` | 符箓 ×5、可用状态 | 点击使用（部分需确认） | `common_slot_empty_9s`、`button_secondary_*` |
| `StatusBarView` | 状态图标、层数、剩余回合 | 悬停详情；回合开始递减动画 | `status_*` |
| `EnergyView` | 灵力 x/y、逐格消耗 | 出卦扣灵动画 | `icon_energy`、`bar_fill_energy` |
| `HandPanelView` | 手牌 0–6 张 | 数量变化只调间距，不缩放文字 | `card_frame_*_9s` |
| `BattleCardView` | 八卦符号、品阶、卦力、内外卦标记 | 点击选/取消，悬停详情 | `bagua_*`、`card_frame_rank*_9s`、`card_tag_*` |
| `ActionPanelView` | 出卦、弃牌、结束回合 | 不可用时变灰并给原因 | `button_primary_*`、`button_secondary_*`、`button_danger_*` |
| `DiscardSelectView` | 弃牌数量、确认/取消 | 选择上限校验 | `frame_select_gold_9s`、`button_secondary_*` |
| `BattleLogView` | 最近事件文本 | 滚动查看 | `common_panel_inset_9s` |
| `WeaponSlotView` | 武器、攻击模式、附魔 3 槽 | 槽满时提示顶替最旧附魔 | `common_panel_section_9s`、`common_slot_empty_9s` |

### 5.4 反馈规则

| 状态 | 反馈 |
|---|---|
| 可出卦 | 出卦按钮高亮（`button_primary_hover` 视觉） |
| 灵力不足 | 按钮禁用 + Toast 说明原因 |
| 只选一张牌 | 提示「请选择外卦」 |
| 同属性 | 显示 `×0.85` |
| 克制 | 显示 `×1.5` + 克制提示 |
| 被克 | 显示 `×0.67` + 受制提示 |
| 可斩杀 | 绿色标签 + 预览强化 |
| 回合结算中 | 隐藏或禁用交互按钮 + 输入锁 |
| 战斗结束 | 结算面板，不可继续操作 |

错误提示**不弹系统对话框**，优先界面内 Toast 或按钮旁短提示。

---

## 6. 动效说明

### 6.1 通用约定

- 所有动效位于 `AnimationLayer`，独立于业务面板，不承载状态。
- 动画播放期间由 `InputBlockLayer` 拦截点击，避免重复出牌或重复结算。
- 时长、位移、缩放、抖动、淡出**全部外部化**到 `Assets/Resources/Config/UI/ui_animation.csv`（沿用已有 `battle_feedback_animation.csv` 的字段风格）。
- 不允许循环闪烁、全屏故障、遮挡点击的动画。
- 必须提供 2× 快放与「点击跳过」（渡劫、晋升、结算类动画尤其需要）。

### 6.2 动效清单

| 场景 | 动效 | 时长 | 缓动 | 触发 |
|---|---|---:|---|---|
| 选中卦牌 | 灵青边缘点亮 + 上浮 6px + 缩放 1.03 | 140ms | OutCubic | 点击手牌 |
| 取消选中 | 回落 + 光灭 | 110ms | OutQuad | 再次点击 |
| 内外卦入槽 | 卡牌飞向卦台 + 旋转归正 | 260ms | InOutCubic | 选定第一/第二张 |
| 合成卦象 | 内外卦向中间合拢 + 墨晕扩散 | 280ms | OutCubic | 双卦就绪 |
| 卦名浮现 | 淡入 + 上移 12px | 220ms | OutQuad | 卦象确定 |
| 六爻亮起 | 六条爻线自上而下依次点亮 | 60ms/条 | Linear | 特殊卦 / 图鉴解锁 |
| 伤害数字 | 数字短促放大 1.35 → 1.0 + 朱砂残影 | 200ms | OutBack | 结算事件 |
| 会心数字 | 额外放大到 1.6 + 停留 120ms | 320ms | OutBack | 会心触发 |
| 护盾吸收 | 灵青数字 + 盾图标闪烁 | 180ms | OutQuad | 护盾抵挡 |
| 敌人意图刷新 | 红色扫描线掠过一次 | 220ms | Linear | 回合开始 |
| 受击抖动 | 横向 ±8px 抖动 2 次 | 180ms | OutQuad | 敌人/玩家受伤 |
| 血条变化 | 宽度插值 + 延迟白条 | 260ms | OutQuad | HP 变化 |
| 灵力消耗 | 逐格变暗 | 40ms/格 | Linear | 出卦 |
| 状态层数变化 | 图标缩放脉冲 + 层数滚动 | 160ms | OutQuad | 状态增减 |
| 弃牌 | 滑向弃牌堆 + 淡出 | 220ms | InCubic | 确认弃牌 |
| 抽牌 | 从抽牌堆飞入 + 扇形展开 | 240ms | OutCubic | 抽牌事件 |
| 卡牌不可用 | 1–2 次横向错位 ±6px | 130ms | Linear | 无效操作 |
| 面板展开 | 缩放 0.94 → 1 + 淡入 | 180ms | OutCubic | 页面切换 |
| 弹窗出现 | 缩放 0.9 → 1 + 淡入 + 遮罩淡入 | 200ms | OutBack | 打开弹窗 |
| 弹窗关闭 | 缩放 1 → 0.96 + 淡出 | 140ms | InQuad | 关闭弹窗 |
| Toast | 上移淡入 → 停留 1.6s → 淡出 | 160/160ms | OutQuad | 短提示 |
| Tooltip | 淡入（悬停 400ms 后） | 120ms | Linear | 悬停 |
| 坊市刷新 | 商品签条翻面 + 轻微错位 | 240ms | OutCubic | 点击刷新 |
| 购买成功 | 灵石数字上浮 + 商品变「已售出」 | 260ms | OutQuad | 购买成功 |
| 解锁新卦 | 墨迹退去 + 六爻线显现 | 380ms | OutCubic | 首次合成 |
| 回合切换 | 横幅扫过 + 敌人意图扫描线 | 300ms | Linear | 回合开始 |
| 渡劫 / 晋升 | 朱砂红印落下 + 六爻亮起 + 境界徽记 | 900ms（可跳过） | OutCubic | 渡劫胜利 |

### 6.3 参数文件建议结构

```csv
key,duration,easing,offsetY,startScale,endScale,shakeDistance,shakeFrequency,fadeStart,punchScale,delay
card_select,0.14,OutCubic,6,1.03,1,0,0,0,1.08,0
hexagram_merge,0.28,OutCubic,0,1.06,1,0,0,0,1.02,0
damage_number,0.2,OutBack,72,1.35,1,0,0,0.6,1,0
enemy_intent_scan,0.22,Linear,0,1,1,0,0,0,1,0
...
```

字段与已有 `BattleFeedbackAnimationConfig` 保持同名同义，避免出现两套解析器。

---

## 7. 适配与验收

### 7.1 适配规则

- 设计分辨率 1920×1080，16:9；CanvasScaler `Scale With Screen Size`，Match 0.5。
- 最小验证分辨率 1280×720。
- 顶部与底部面板固定高度；中部卦台弹性布局。
- 所有面板用锚点定位，禁止绝对坐标硬定位。
- 手牌数量变化只调间距，不缩放文字。
- TMP 自动换行，关键数值不被裁切。
- `TooltipLayer` 层级高于所有普通面板。

### 7.2 界面验收清单

- 1920×1080 下与原型图信息结构一致。
- 1280×720 下无关键文字裁切、无按钮重叠。
- 内外卦选择顺序清晰，预览与实际伤害一致。
- 出卦、弃牌、结束回合状态切换正确。
- 战斗结束后面板不可继续操作。
- 快速连续点击不会重复出牌或重复结算。
- 弹窗出现时底层不可交互；Toast 不遮挡关键数值。
- 素材拉伸不变形（九宫格正确），图标在 60%–150% 缩放下清晰。

---

## 8. 后续待补素材（不在本批）

| 类别 | 说明 |
|---|---|
| 敌人立绘 | 6 小怪 + 3 精英 + 3 渡劫 Boss，建议 512×512 或 1024×1024，暗红眼点/墨污风格 |
| 玩家立绘 | 主角按境界 3 套 |
| 卡面插画 | 8 卦 × 3 星，可先用八卦符号 + 品阶框过渡 |
| 法宝 / 符箓图标 | 20 + 8 张，128×128，复用 `common_slot_empty_9s` 作底 |
| 敌人意图扩展 | 召唤、蓄力、改变环境（当前只有攻击/防御/减益） |
| 粒子与特效 | 墨晕扩散、符箓燃烧、雷劫落雷 |
| 动画帧 | 若改用 `AnimationClip`/`Animator` 替代程序化 Tween |
| 字体授权 | 思源宋体/黑体、等宽数字字体的商用授权与 TMP SDF 资产 |

---

## 9. 版本记录

| 版本 | 日期 | 说明 |
|---|---|---|
| v0.2 | 2026-09-10 | 素材质量重做：面板/按钮/卡框/背景全部重画，加入宣纸纤维、墨晕扩散、枯笔边缘、立体渐变与高光；图标改为双层底盘 + 具象主体；新增 §3.6 素材质量基线 |
| v0.1 | 2026-09-10 | 初稿：确定素材体系与命名规范、80 个基础 PNG、六阶段制作顺序、功能与动效清单 |
