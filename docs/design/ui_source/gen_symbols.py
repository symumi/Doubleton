# -*- coding: utf-8 -*-
"""《卦锻》UI 符号素材 SVG 生成器：图标 / 八卦 / 页面背景。

运行：python docs/design/ui_source/gen_symbols.py
输出：docs/design/ui_source/svg/<Category>/<name>.svg 与 manifest_symbols.json

风格目标：图标要有体积与语义（双层底盘 + 主体描边 + 高光），不能是纯线条；
背景要有水墨山水层次（远山/墨晕/纸纹/暗角），不能是一块纯黑。
"""

import json
import os

HERE = os.path.dirname(os.path.abspath(__file__))
SVG_ROOT = os.path.join(HERE, "svg")

INK = "#070808"
DEEP_INK = "#04050a"
PAPER = "#C8BFA8"
GOLD = "#C2AA72"
GOLD_LIGHT = "#E9DAA6"
GOLD_DARK = "#8A6E3C"
CINNABAR = "#B63A31"
CINNABAR_LIGHT = "#D9705F"
SPIRIT = "#5EC8C0"
SPIRIT_LIGHT = "#8FE0D8"
FONT_CN = "KaiTi, STKaiti, SimSun, serif"

ITEMS = []


# ---------------------------------------------------------------- 通用构造

def linear_grad(gid, c0, c1, o0=1.0, o1=1.0, vertical=True):
    x1, y1, x2, y2 = ("0", "0", "0", "1") if vertical else ("0", "0", "1", "0")
    return (
        f'<linearGradient id="{gid}" x1="{x1}" y1="{y1}" x2="{x2}" y2="{y2}">'
        f'<stop offset="0" stop-color="{c0}" stop-opacity="{o0}"/>'
        f'<stop offset="1" stop-color="{c1}" stop-opacity="{o1}"/>'
        f"</linearGradient>"
    )


def radial_grad(gid, c0, c1, o0=1.0, o1=0.0, cx=0.5, cy=0.5, r=0.7):
    return (
        f'<radialGradient id="{gid}" cx="{cx}" cy="{cy}" r="{r}">'
        f'<stop offset="0" stop-color="{c0}" stop-opacity="{o0}"/>'
        f'<stop offset="1" stop-color="{c1}" stop-opacity="{o1}"/>'
        f"</radialGradient>"
    )


def blur(fid, radius):
    return (
        f'<filter id="{fid}" x="-60%" y="-60%" width="220%" height="220%">'
        f'<feGaussianBlur stdDeviation="{radius}"/></filter>'
    )


def rough_filter(fid="rough", freq=0.05, scale=2.5, seed=5):
    return (
        f'<filter id="{fid}" x="-25%" y="-25%" width="150%" height="150%">'
        f'<feTurbulence type="fractalNoise" baseFrequency="{freq}" numOctaves="3" seed="{seed}" result="n"/>'
        f'<feDisplacementMap in="SourceGraphic" in2="n" scale="{scale}" '
        f'xChannelSelector="R" yChannelSelector="G"/>'
        f"</filter>"
    )


def paper_filter(fid="paper", freq=0.85, octaves=4, seed=7):
    return (
        f'<filter id="{fid}" x="0" y="0" width="100%" height="100%">'
        f'<feTurbulence type="fractalNoise" baseFrequency="{freq}" numOctaves="{octaves}" '
        f'seed="{seed}" stitchTiles="stitch" result="n"/>'
        f'<feColorMatrix in="n" type="saturate" values="0"/>'
        f"</filter>"
    )


def emit(category, name, w, h, body, defs="", border=None, usage="", nine_slice=False, color=""):
    folder = os.path.join(SVG_ROOT, category)
    os.makedirs(folder, exist_ok=True)
    with open(os.path.join(folder, name + ".svg"), "w", encoding="utf-8") as f:
        f.write(
            f'<svg xmlns="http://www.w3.org/2000/svg" width="{w}" height="{h}" '
            f'viewBox="0 0 {w} {h}">\n<defs>{defs}</defs>\n{body}\n</svg>\n'
        )
    ITEMS.append({
        "category": category, "name": name, "width": w, "height": h,
        "nine_slice": nine_slice, "border": border, "usage": usage, "color": color,
        "file": f"Assets/Resources/Texture/UI/{category}/{name}.png",
    })


def badge_defs(main0, main1, seed):
    """图标统一底盘的公共定义。"""
    return (
        linear_grad("main", main0, main1)
        + radial_grad("discGlow", "#FFFFFF", "#000000", 0.1, 0.0, 0.5, 0.22, 0.9)
        + radial_grad("discShade", "#000000", "#000000", 0.0, 0.45, 0.5, 0.9, 0.7)
        + rough_filter(seed=seed)
    )


def disc_base(ring=GOLD, ring_op=0.45, radius=50):
    """暗底圆盘 + 双环，给图标一个稳定的承托面。"""
    return "".join([
        f'<circle cx="64" cy="64" r="{radius + 8}" fill="{DEEP_INK}" opacity="0.55"/>',
        f'<circle cx="64" cy="64" r="{radius + 4}" fill="#14150f"/>',
        f'<circle cx="64" cy="64" r="{radius + 4}" fill="url(#discGlow)"/>',
        f'<circle cx="64" cy="64" r="{radius + 4}" fill="url(#discShade)"/>',
        f'<circle cx="64" cy="64" r="{radius + 4}" fill="none" stroke="{ring}" '
        f'stroke-opacity="{ring_op}" stroke-width="2.5"/>',
        f'<circle cx="64" cy="64" r="{radius - 2}" fill="none" stroke="{ring}" '
        f'stroke-opacity="{ring_op * 0.35:.2f}" stroke-width="1"/>',
    ])


def tile_base(ring=GOLD, ring_op=0.4):
    """圆角方底，用于状态图标。"""
    return "".join([
        f'<rect x="4" y="4" width="120" height="120" rx="18" fill="{DEEP_INK}" opacity="0.5"/>',
        f'<rect x="7" y="7" width="114" height="114" rx="15" fill="#14150f"/>',
        f'<rect x="7" y="7" width="114" height="114" rx="15" fill="url(#discGlow)"/>',
        f'<rect x="7" y="7" width="114" height="114" rx="15" fill="url(#discShade)"/>',
        f'<rect x="7" y="7" width="114" height="114" rx="15" fill="none" stroke="{ring}" '
        f'stroke-opacity="{ring_op}" stroke-width="2.5"/>',
        f'<rect x="13" y="13" width="102" height="102" rx="11" fill="none" stroke="{ring}" '
        f'stroke-opacity="{ring_op * 0.35:.2f}" stroke-width="1"/>',
    ])


# ---------------------------------------------------------------- 图标

def build_icons():
    # 气血
    defs = badge_defs("#D4604F", "#7C1F18", 11)
    body = disc_base(CINNABAR, 0.5) + "".join([
        f'<path d="M64,104 C30,80 18,62 18,46 C18,30 30,21 44,21 C53,21 60,26 64,34 '
        f'C68,26 75,21 84,21 C98,21 110,30 110,46 C110,62 98,80 64,104 Z" fill="url(#main)" '
        f'filter="url(#rough)"/>',
        f'<path d="M64,104 C30,80 18,62 18,46 C18,30 30,21 44,21 C53,21 60,26 64,34 '
        f'C68,26 75,21 84,21 C98,21 110,30 110,46 C110,62 98,80 64,104 Z" fill="none" '
        f'stroke="{CINNABAR_LIGHT}" stroke-width="2.5" stroke-opacity="0.8"/>',
        f'<path d="M44,38 C36,44 32,52 33,60" stroke="#FFFFFF" stroke-opacity="0.35" '
        f'stroke-width="4" stroke-linecap="round" fill="none"/>',
    ])
    emit("Icon", "icon_hp", 128, 128, body, defs, usage="玩家/敌人气血数值前的图标", color="朱砂心形 + 高光")

    # 护盾
    defs = badge_defs("#E4E8E6", "#8A9490", 13)
    body = disc_base(PAPER, 0.45) + "".join([
        f'<path d="M64,20 L104,35 V60 C104,86 87,103 64,110 C41,103 24,86 24,60 V35 Z" '
        f'fill="url(#main)" opacity="0.95" filter="url(#rough)"/>',
        f'<path d="M64,20 L104,35 V60 C104,86 87,103 64,110 C41,103 24,86 24,60 V35 Z" '
        f'fill="none" stroke="{GOLD}" stroke-width="2.5" stroke-opacity="0.85"/>',
        f'<path d="M64,32 V98" stroke="#3A403E" stroke-opacity="0.45" stroke-width="3"/>',
        f'<path d="M40,52 H88" stroke="#3A403E" stroke-opacity="0.3" stroke-width="2.5"/>',
        f'<circle cx="64" cy="44" r="4" fill="{GOLD}" opacity="0.8"/>',
    ])
    emit("Icon", "icon_shield", 128, 128, body, defs, usage="护盾数值前的图标", color="灰纸盾形 + 符金铆钉")

    # 灵力
    defs = badge_defs(SPIRIT_LIGHT, "#2B7A75", 17)
    body = disc_base(SPIRIT, 0.5) + "".join([
        f'<path d="M64,16 L100,64 L64,112 L28,64 Z" fill="url(#main)" opacity="0.9" filter="url(#rough)"/>',
        f'<path d="M64,16 L100,64 L64,112 L28,64 Z" fill="none" stroke="{SPIRIT_LIGHT}" '
        f'stroke-width="2.5" stroke-opacity="0.85"/>',
        f'<path d="M64,30 A34,34 0 0,1 98,64" fill="none" stroke="#062B2A" stroke-opacity="0.55" '
        f'stroke-width="5" stroke-linecap="round"/>',
        f'<path d="M64,98 A34,34 0 0,1 30,64" fill="none" stroke="#062B2A" stroke-opacity="0.55" '
        f'stroke-width="5" stroke-linecap="round"/>',
        f'<circle cx="64" cy="64" r="12" fill="#062B2A" opacity="0.7"/>',
        f'<circle cx="64" cy="64" r="5" fill="{SPIRIT_LIGHT}" opacity="0.9"/>',
    ])
    emit("Icon", "icon_energy", 128, 128, body, defs, usage="灵力数值与前缀图标", color="灵青气旋 + 内环")

    # 灵石
    defs = badge_defs(GOLD_LIGHT, GOLD_DARK, 19)
    body = disc_base(GOLD, 0.5) + "".join([
        f'<circle cx="64" cy="64" r="38" fill="url(#main)" filter="url(#rough)"/>',
        f'<circle cx="64" cy="64" r="38" fill="none" stroke="{GOLD_LIGHT}" stroke-width="2.5" '
        f'stroke-opacity="0.85"/>',
        f'<rect x="46" y="46" width="36" height="36" rx="3" fill="{DEEP_INK}" opacity="0.9"/>',
        f'<rect x="53" y="53" width="22" height="22" rx="2" fill="{GOLD}" opacity="0.45"/>',
        f'<rect x="61" y="12" width="6" height="12" rx="2" fill="{GOLD}" opacity="0.85"/>',
        f'<rect x="61" y="104" width="6" height="12" rx="2" fill="{GOLD}" opacity="0.85"/>',
        f'<rect x="12" y="61" width="12" height="6" rx="2" fill="{GOLD}" opacity="0.85"/>',
        f'<rect x="104" y="61" width="12" height="6" rx="2" fill="{GOLD}" opacity="0.85"/>',
    ])
    emit("Icon", "icon_coin", 128, 128, body, defs, usage="灵石数量图标（坊市价格、奖励、结算）", color="符金铜钱 + 方孔")

    elements = {
        "metal": ("#E2E6E7", "#8D9598", "金", "灰白",
                  '<path d="M64,18 L88,64 L64,110 L40,64 Z" fill="url(#main)" filter="url(#rough)"/>'
                  '<path d="M64,18 L88,64 L64,110 L40,64 Z" fill="none" stroke="#EFF3F4" stroke-width="2.5" stroke-opacity="0.8"/>'
                  '<path d="M64,34 V94" stroke="#2A3033" stroke-opacity="0.6" stroke-width="4"/>'
                  '<path d="M40,52 H30 M88,52 H98 M40,76 H30 M88,76 H98" stroke="#EFF3F4" '
                  'stroke-opacity="0.5" stroke-width="3" stroke-linecap="round"/>'),
        "wood": ("#8FB58A", "#2F4A33", "木", "灰绿",
                 '<path d="M62,108 V44" stroke="url(#main)" stroke-width="11" stroke-linecap="round"/>'
                 '<path d="M62,72 C42,66 32,50 36,32 C54,34 66,48 62,72 Z" fill="url(#main)" filter="url(#rough)"/>'
                 '<path d="M62,88 C82,82 92,66 88,48 C70,50 58,64 62,88 Z" fill="url(#main)" opacity="0.85" filter="url(#rough)"/>'
                 '<path d="M62,44 V104" stroke="#C6E0BE" stroke-opacity="0.35" stroke-width="2"/>'),
        "water": ("#8FD3E6", "#2A5F7C", "水", "冷青",
                  '<path d="M22,46 C38,30 52,62 68,46 C84,30 98,62 112,46" fill="none" '
                  'stroke="url(#main)" stroke-width="9" stroke-linecap="round" filter="url(#rough)"/>'
                  '<path d="M22,72 C38,56 52,88 68,72 C84,56 98,88 112,72" fill="none" '
                  'stroke="url(#main)" stroke-width="9" stroke-linecap="round" opacity="0.8"/>'
                  '<path d="M28,96 C44,84 56,106 72,96 C86,86 98,104 108,96" fill="none" '
                  'stroke="url(#main)" stroke-width="6" stroke-linecap="round" opacity="0.55"/>'),
        "fire": ("#EE9A4F", "#A32F14", "火", "朱橙",
                 '<path d="M64,14 C86,44 100,58 100,76 C100,96 84,112 64,112 C44,112 28,96 28,76 '
                 'C28,58 44,46 52,34 C56,52 68,56 70,42 C72,30 66,24 64,14 Z" fill="url(#main)" filter="url(#rough)"/>'
                 '<path d="M64,56 C76,72 82,80 82,90 C82,100 74,106 64,106 C54,106 46,100 46,90 '
                 'C46,80 54,72 64,56 Z" fill="#F7D79A" opacity="0.75"/>'
                 '<path d="M64,14 C86,44 100,58 100,76 C100,96 84,112 64,112 C44,112 28,96 28,76 '
                 'C28,58 44,46 52,34" fill="none" stroke="#FFC98A" stroke-opacity="0.6" stroke-width="2.5"/>'),
        "earth": ("#D2BC8E", "#7E6A3E", "土", "灰黄",
                  '<path d="M18,104 L50,50 L74,86 L92,60 L114,104 Z" fill="url(#main)" filter="url(#rough)"/>'
                  '<path d="M18,104 L50,50 L74,86 L92,60 L114,104 Z" fill="none" stroke="#E8D7AE" '
                  'stroke-opacity="0.6" stroke-width="2"/>'
                  '<rect x="18" y="106" width="96" height="4" rx="2" fill="#D2BC8E" opacity="0.75"/>'
                  '<rect x="34" y="114" width="64" height="3" rx="1.5" fill="#D2BC8E" opacity="0.5"/>'),
    }
    for key, (c0, c1, label, color, inner) in elements.items():
        defs = badge_defs(c0, c1, 23 + len(key))
        body = disc_base(c0, 0.45) + inner
        emit("Icon", f"element_{key}", 128, 128, body, defs,
             usage=f"五行图标：{label}（敌人五行、卡牌五行、克制提示）", color=color)

    statuses = {
        "burn": ("#EE9A4F", "#A32F14", "灼烧", "朱橙火苗",
                 '<path d="M64,24 C82,50 94,62 94,78 C94,95 81,106 64,106 C47,106 34,95 34,78 '
                 'C34,62 47,54 54,42 C58,58 68,62 70,50 C72,40 66,32 64,24 Z" fill="url(#main)" filter="url(#rough)"/>'
                 '<path d="M64,64 C74,76 78,82 78,90 C78,97 72,102 64,102 C56,102 50,97 50,90 '
                 'C50,82 56,76 64,64 Z" fill="#F7D79A" opacity="0.7"/>'
                 '<circle cx="46" cy="34" r="3" fill="#EE9A4F" opacity="0.7"/>'
                 '<circle cx="82" cy="30" r="2.5" fill="#EE9A4F" opacity="0.55"/>'),
        "chill": ("#B6E8EE", "#2F6B7C", "寒滞", "冷青雪花",
                  '<g stroke="url(#main)" stroke-width="7" stroke-linecap="round" filter="url(#rough)">'
                  '<path d="M64,20 V108"/><path d="M26,42 L102,86"/><path d="M102,42 L26,86"/></g>'
                  '<g stroke="#DFF6F8" stroke-opacity="0.55" stroke-width="2.5" stroke-linecap="round">'
                  '<path d="M50,26 L64,34 L78,26"/><path d="M50,102 L64,94 L78,102"/></g>'
                  '<circle cx="64" cy="64" r="13" fill="#0B1E22" opacity="0.75"/>'
                  '<circle cx="64" cy="64" r="6" fill="#DFF6F8" opacity="0.8"/>'),
        "vulnerable": ("#E0786C", "#7A1F1A", "易伤", "暗红靶环",
                       '<circle cx="64" cy="64" r="36" fill="none" stroke="url(#main)" stroke-width="9" filter="url(#rough)"/>'
                       '<circle cx="64" cy="64" r="20" fill="none" stroke="url(#main)" stroke-width="5" opacity="0.8"/>'
                       '<circle cx="64" cy="64" r="8" fill="url(#main)"/>'
                       '<path d="M64,14 V34 M64,94 V114 M14,64 H34 M94,64 H114" stroke="url(#main)" '
                       'stroke-width="7" stroke-linecap="round"/>'),
        "weak": ("#B5B5A6", "#565648", "虚弱", "灰白断箭",
                 '<path d="M64,20 V68" stroke="url(#main)" stroke-width="11" stroke-linecap="round"/>'
                 '<path d="M38,52 L64,80 L90,52" fill="none" stroke="url(#main)" stroke-width="11" '
                 'stroke-linecap="round" stroke-linejoin="round" filter="url(#rough)"/>'
                 '<path d="M34,94 H94" stroke="#565648" stroke-width="7" stroke-linecap="round" opacity="0.75"/>'
                 '<path d="M44,104 H84" stroke="#565648" stroke-width="5" stroke-linecap="round" opacity="0.45"/>'),
        "shield": ("#E6F5F3", "#6FA8A3", "护盾状态", "青白盾",
                   '<path d="M64,20 L102,35 V60 C102,86 85,102 64,110 C43,102 26,86 26,60 V35 Z" '
                   'fill="url(#main)" opacity="0.95" filter="url(#rough)"/>'
                   '<path d="M64,20 L102,35 V60 C102,86 85,102 64,110 C43,102 26,86 26,60 V35 Z" '
                   'fill="none" stroke="#F2FBFA" stroke-width="2.5" stroke-opacity="0.8"/>'
                   '<path d="M46,52 H82 M46,64 H82 M46,76 H82" stroke="#2C4F4C" stroke-opacity="0.4" '
                   'stroke-width="3"/>'),
        "armor_break": ("#D4604F", "#5E1A13", "破甲", "朱砂裂盾",
                        '<path d="M64,20 L102,35 V60 C102,86 85,102 64,110 C43,102 26,86 26,60 V35 Z" '
                        'fill="url(#main)" opacity="0.9" filter="url(#rough)"/>'
                        '<path d="M64,20 L102,35 V60 C102,86 85,102 64,110 C43,102 26,86 26,60 V35 Z" '
                        'fill="none" stroke="#F0A897" stroke-width="2.5" stroke-opacity="0.8"/>'
                        '<path d="M46,30 L84,88 M82,32 L48,90" stroke="{DEEP_INK}" stroke-width="6" '
                        'stroke-opacity="0.8"/>'.replace("{DEEP_INK}", DEEP_INK)),
    }
    for key, (c0, c1, label, color, inner) in statuses.items():
        defs = badge_defs(c0, c1, 41 + len(key))
        body = tile_base(c0, 0.45) + inner
        emit("Icon", f"status_{key}", 128, 128, body, defs,
             usage=f"状态图标：{label}（层数由 TMP 叠加在右下角）", color=color)

    # 敌人意图（96×96）
    def intent(name, tint0, tint1, ring, inner, usage, color, seed):
        w = h = 96
        defs = badge_defs(tint0, tint1, seed)
        body = (
            f'<path d="M10,6 H86 L86,44 L48,90 L10,44 Z" fill="{DEEP_INK}" opacity="0.6"/>'
            f'<path d="M13,9 H83 L83,42 L48,84 L13,42 Z" fill="#14150f"/>'
            f'<path d="M13,9 H83 L83,42 L48,84 L13,42 Z" fill="url(#discGlow)"/>'
            f'<path d="M13,9 H83 L83,42 L48,84 L13,42 Z" fill="none" stroke="{ring}" '
            f'stroke-opacity="0.5" stroke-width="2.5"/>'
            + inner
        )
        emit("Icon", name, w, h, body, defs, usage=usage, color=color)

    intent(
        "intent_attack", CINNABAR_LIGHT, "#7A1F1A", CINNABAR,
        f'<path d="M48,16 L60,40 V66 L48,80 L36,66 V40 Z" fill="url(#main)" filter="url(#rough)"/>'
        f'<path d="M48,16 L60,40 V66 L48,80 L36,66 V40 Z" fill="none" stroke="{CINNABAR_LIGHT}" '
        f'stroke-width="2" stroke-opacity="0.8"/>'
        f'<rect x="41" y="80" width="14" height="5" rx="2" fill="{CINNABAR}" opacity="0.85"/>'
        f'<path d="M48,22 V74" stroke="#2A0A08" stroke-opacity="0.5" stroke-width="2.5"/>',
        "敌人意图：攻击（数值由 TMP 叠加在图标下方）", "朱砂剑刃", 71,
    )
    intent(
        "intent_defend", "#E6F5F3", "#5F9D98", SPIRIT,
        f'<path d="M48,14 L74,25 V46 C74,64 62,75 48,80 C34,75 22,64 22,46 V25 Z" '
        f'fill="url(#main)" opacity="0.95" filter="url(#rough)"/>'
        f'<path d="M48,14 L74,25 V46 C74,64 62,75 48,80 C34,75 22,64 22,46 V25 Z" fill="none" '
        f'stroke="{SPIRIT_LIGHT}" stroke-width="2" stroke-opacity="0.8"/>'
        f'<path d="M36,40 H60 M36,50 H60" stroke="#20403E" stroke-opacity="0.45" stroke-width="3"/>',
        "敌人意图：防御/获得护盾", "灵青盾", 73,
    )
    intent(
        "intent_debuff", "#C0706A", "#5E1F1C", "#8F3A3A",
        f'<path d="M48,14 V58" stroke="url(#main)" stroke-width="9" stroke-linecap="round"/>'
        f'<path d="M28,46 L48,70 L68,46" fill="none" stroke="url(#main)" stroke-width="9" '
        f'stroke-linecap="round" stroke-linejoin="round" filter="url(#rough)"/>'
        f'<path d="M22,76 C30,70 38,84 48,76 C58,68 66,84 74,76" fill="none" stroke="{CINNABAR}" '
        f'stroke-width="4" opacity="0.7"/>'
        f'<path d="M48,20 V62" stroke="#2A0A08" stroke-opacity="0.45" stroke-width="2.5"/>',
        "敌人意图：减益/施加状态", "暗红破碎", 79,
    )

    # 境界徽记
    for level, label in ((1, "炼气"), (2, "筑基"), (3, "金丹")):
        seed = 83 + level
        defs = (
            radial_grad("sealGlow", CINNABAR, CINNABAR, 0.25, 0.0)
            + radial_grad("sealShade", "#000000", "#000000", 0.0, 0.5, 0.5, 0.9, 0.72)
            + rough_filter(freq=0.06, scale=2.5, seed=seed)
        )
        marks = "".join(
            f'<rect x="46" y="{52 + i * 13}" width="36" height="6.5" rx="1.5" fill="{CINNABAR}" opacity="0.75"/>'
            for i in range(level)
        )
        body = "".join([
            f'<circle cx="64" cy="64" r="56" fill="{DEEP_INK}" opacity="0.5"/>',
            f'<circle cx="64" cy="64" r="53" fill="#1B0E0C"/>',
            f'<circle cx="64" cy="64" r="53" fill="url(#sealGlow)"/>',
            f'<circle cx="64" cy="64" r="53" fill="url(#sealShade)"/>',
            f'<circle cx="64" cy="64" r="49" fill="none" stroke="{CINNABAR}" stroke-width="6" '
            f'stroke-opacity="0.9" filter="url(#rough)"/>',
            f'<circle cx="64" cy="64" r="40" fill="none" stroke="{CINNABAR}" stroke-width="2" '
            f'stroke-opacity="0.55"/>',
            f'<rect x="36" y="36" width="56" height="56" rx="4" fill="{CINNABAR}" opacity="0.14"/>',
            f'<rect x="36" y="36" width="56" height="56" rx="4" fill="none" stroke="{CINNABAR}" '
            f'stroke-width="3" stroke-opacity="0.75"/>',
            marks,
            f'<rect x="27" y="27" width="9" height="9" fill="{CINNABAR}" opacity="0.7"/>',
            f'<rect x="92" y="27" width="9" height="9" fill="{CINNABAR}" opacity="0.7"/>',
            f'<rect x="27" y="92" width="9" height="9" fill="{CINNABAR}" opacity="0.7"/>',
            f'<rect x="92" y="92" width="9" height="9" fill="{CINNABAR}" opacity="0.7"/>',
        ])
        emit("Icon", f"realm_seal_{level}", 128, 128, body, defs, nine_slice=False,
             usage=f"境界徽记：{label}（路线页当前境界、晋升页、局外成长页）", color="朱砂方印 + 圆环")


# ---------------------------------------------------------------- 八卦与爻线

def build_hexagram():
    defs = (
        linear_grad("line", GOLD_LIGHT, GOLD_DARK)
        + radial_grad("lineShine", "#FFF6DA", "#000000", 0.5, 0.0, 0.5, 0.2, 0.8)
        + rough_filter(freq=0.08, scale=1.6, seed=91)
        + blur("lineShadow", 2)
    )
    body = (
        f'<rect x="0" y="9" width="128" height="8" rx="4" fill="{DEEP_INK}" opacity="0.7"/>'
        f'<rect x="2" y="6" width="124" height="12" rx="4" fill="url(#line)" filter="url(#rough)"/>'
        f'<rect x="2" y="6" width="124" height="12" rx="4" fill="url(#lineShine)" opacity="0.35"/>'
    )
    emit("Hexagram", "hex_line_yang", 128, 24, body, defs, nine_slice=False,
         usage="阳爻（实线），64 卦图形由六条爻线拼合", color="符金实线 + 高光")

    body = (
        f'<rect x="0" y="9" width="52" height="8" rx="4" fill="{DEEP_INK}" opacity="0.7"/>'
        f'<rect x="76" y="9" width="52" height="8" rx="4" fill="{DEEP_INK}" opacity="0.7"/>'
        f'<rect x="2" y="6" width="50" height="12" rx="4" fill="url(#line)" filter="url(#rough)"/>'
        f'<rect x="76" y="6" width="50" height="12" rx="4" fill="url(#line)" filter="url(#rough)"/>'
        f'<rect x="2" y="6" width="50" height="12" rx="4" fill="url(#lineShine)" opacity="0.3"/>'
        f'<rect x="76" y="6" width="50" height="12" rx="4" fill="url(#lineShine)" opacity="0.3"/>'
    )
    emit("Hexagram", "hex_line_yin", 128, 24, body, defs, nine_slice=False,
         usage="阴爻（断线），64 卦图形由六条爻线拼合", color="符金断线 + 高光")

    bagua = {
        "qian": ([1, 1, 1], "乾 天"),
        "dui": ([1, 1, 0], "兑 泽"),
        "li": ([1, 0, 1], "离 火"),
        "zhen": ([1, 0, 0], "震 雷"),
        "xun": ([0, 1, 1], "巽 风"),
        "kan": ([0, 1, 0], "坎 水"),
        "gen": ([0, 0, 1], "艮 山"),
        "kun": ([0, 0, 0], "坤 地"),
    }
    for key, (lines, label) in bagua.items():
        parts = [
            f'<circle cx="64" cy="64" r="58" fill="{DEEP_INK}" opacity="0.45"/>',
            f'<circle cx="64" cy="64" r="56" fill="#15160f"/>',
            f'<circle cx="64" cy="64" r="56" fill="url(#discGlow)"/>',
            f'<circle cx="64" cy="64" r="56" fill="none" stroke="{GOLD}" stroke-opacity="0.35" '
            f'stroke-width="2"/>',
        ]
        for idx, yang in enumerate(reversed(lines)):
            y = 28 + idx * 28
            if yang:
                parts.append(
                    f'<rect x="20" y="{y + 2}" width="88" height="14" rx="4" fill="{DEEP_INK}" opacity="0.6"/>'
                    f'<rect x="20" y="{y}" width="88" height="14" rx="4" fill="url(#line)" filter="url(#rough)"/>'
                    f'<rect x="20" y="{y}" width="88" height="5" rx="2.5" fill="#FFF6DA" opacity="0.3"/>'
                )
            else:
                for x in (20, 74):
                    parts.append(
                        f'<rect x="{x}" y="{y + 2}" width="34" height="14" rx="4" fill="{DEEP_INK}" opacity="0.6"/>'
                        f'<rect x="{x}" y="{y}" width="34" height="14" rx="4" fill="url(#line)" filter="url(#rough)"/>'
                        f'<rect x="{x}" y="{y}" width="34" height="5" rx="2.5" fill="#FFF6DA" opacity="0.26"/>'
                    )
        defs_g = (
            linear_grad("line", GOLD_LIGHT, GOLD_DARK)
            + radial_grad("discGlow", "#FFFFFF", "#000000", 0.08, 0.0, 0.5, 0.22, 0.9)
            + rough_filter(freq=0.09, scale=1.6, seed=97)
        )
        emit("Hexagram", f"bagua_{key}", 128, 128, "".join(parts), defs_g, nine_slice=False,
             usage=f"八卦符号：{label}（手牌、卦台内/外卦槽、图鉴行列头）", color="符金卦画 + 暗底承托")


# ---------------------------------------------------------------- 页面背景

FAR_RIDGE = ("M0,612 L96,566 L192,604 L288,548 L384,592 L480,536 L576,586 L672,528 L768,580 "
             "L864,524 L960,574 L1056,520 L1152,572 L1248,532 L1344,592 L1440,540 L1536,596 "
             "L1632,548 L1728,600 L1824,556 L1920,606 L1920,1080 L0,1080 Z")
MID_RIDGE = ("M0,706 L128,650 L256,694 L384,632 L512,684 L640,624 L768,678 L896,626 L1024,682 "
             "L1152,634 L1280,690 L1408,640 L1536,696 L1664,648 L1792,700 L1920,656 L1920,1080 L0,1080 Z")
NEAR_RIDGE = ("M0,842 L160,790 L320,832 L480,772 L640,822 L800,764 L960,816 L1120,768 L1280,822 "
              "L1440,778 L1600,828 L1760,786 L1920,834 L1920,1080 L0,1080 Z")


def build_backgrounds():
    w, h = 1920, 1080

    def bg_defs(accent, seed):
        return (
            radial_grad("base", "#37342b", "#06070a", 1.0, 1.0, 0.5, 0.34, 0.94)
            + radial_grad("accent", accent, accent, 0.2, 0.0, 0.5, 0.5, 0.6)
            + radial_grad("vig", INK, INK, 0.0, 0.92, 0.5, 0.5, 0.82)
            + paper_filter(freq=0.8, octaves=4, seed=seed)
            + blur("soft", 22)
            + blur("mist", 60)
            + rough_filter(freq=0.01, scale=16, seed=seed + 7)
            + (
                '<pattern id="grid" width="64" height="64" patternUnits="userSpaceOnUse">'
                f'<rect width="64" height="64" fill="none" stroke="{PAPER}" stroke-opacity="0.045" '
                'stroke-width="1"/></pattern>'
            )
            + (
                '<pattern id="scan" width="8" height="8" patternUnits="userSpaceOnUse">'
                f'<rect y="3" width="8" height="1" fill="{SPIRIT}" opacity="0.05"/></pattern>'
            )
        )

    def ridge_paths(near_opacity=0.95):
        return "".join([
            f'<path d="{FAR_RIDGE}" fill="#2C2D28" opacity="0.9" filter="url(#mist)"/>',
            f'<path d="{MID_RIDGE}" fill="#191A16" opacity="0.92" filter="url(#soft)"/>',
            f'<path d="{NEAR_RIDGE}" fill="#0A0B0D" opacity="{near_opacity}" filter="url(#soft)"/>',
        ])

    def clouds():
        return "".join([
            f'<g filter="url(#mist)" opacity="0.6">',
            f'<ellipse cx="420" cy="600" rx="460" ry="66" fill="#0A0B0D"/>',
            f'<ellipse cx="1360" cy="560" rx="520" ry="74" fill="#0A0B0D"/>',
            f'<ellipse cx="900" cy="660" rx="620" ry="58" fill="#0A0B0D" opacity="0.8"/>',
            f"</g>",
        ])

    def base_body(accent_pos, extra="", near_opacity=0.95):
        ax, ay = accent_pos
        return "".join([
            f'<rect width="{w}" height="{h}" fill="url(#base)"/>',
            f'<ellipse cx="{ax}" cy="{ay}" rx="900" ry="600" fill="url(#accent)"/>',
            ridge_paths(near_opacity),
            clouds(),
            extra,
            f'<rect width="{w}" height="{h}" fill="url(#grid)"/>',
            f'<rect width="{w}" height="{h}" fill="url(#scan)"/>',
            f'<rect width="{w}" height="{h}" filter="url(#paper)" opacity="0.4" '
            f'style="mix-blend-mode:overlay"/>',
            f'<rect width="{w}" height="{h}" fill="url(#vig)"/>',
        ])

    emit("Background", "bg_main_menu", w, h,
         base_body((960, 280), "".join([
             f'<g filter="url(#mist)" opacity="0.5">',
             f'<ellipse cx="960" cy="260" rx="700" ry="280" fill="{SPIRIT}" opacity="0.14"/>',
             f"</g>",
             f'<circle cx="960" cy="300" r="230" fill="none" stroke="{GOLD}" stroke-opacity="0.12" stroke-width="2"/>',
             f'<circle cx="960" cy="300" r="300" fill="none" stroke="{GOLD}" stroke-opacity="0.07" stroke-width="1.5"/>',
             f'<rect x="820" y="196" width="120" height="4" fill="{CINNABAR}" opacity="0.5"/>',
             f'<rect x="980" y="196" width="120" height="4" fill="{SPIRIT}" opacity="0.4"/>',
         ])), bg_defs(SPIRIT, 131), usage="主界面背景（1920×1080，可等比缩放）",
         color="远山剪影 + 灵青顶光 + 卦纹圆环")

    ink_path = "".join([
        f'<g filter="url(#soft)" opacity="0.9">',
        f'<path d="M120,900 C420,780 360,520 700,440 C1020,364 1180,300 1800,180" fill="none" '
        f'stroke="{DEEP_INK}" stroke-width="170" stroke-linecap="round"/>',
        f'<path d="M120,900 C420,780 360,520 700,440 C1020,364 1180,300 1800,180" fill="none" '
        f'stroke="{GOLD}" stroke-width="4" stroke-opacity="0.2"/>',
        f"</g>",
        f'<circle cx="700" cy="440" r="13" fill="{CINNABAR}" opacity="0.55"/>',
        f'<circle cx="1400" cy="266" r="11" fill="{CINNABAR}" opacity="0.4"/>',
        f'<circle cx="130" cy="892" r="11" fill="{SPIRIT}" opacity="0.4"/>',
    ])
    emit("Background", "bg_route", w, h, base_body((1460, 880), ink_path, 0.9),
         bg_defs(GOLD, 137), usage="修行路线页背景（节点连线叠加在其上）",
         color="远山 + 符金路线墨迹 + 朱砂节点")

    emit("Background", "bg_battle", w, h,
         base_body((1540, 220), "".join([
             f'<g filter="url(#mist)" opacity="0.6">',
             f'<ellipse cx="1560" cy="240" rx="580" ry="320" fill="{CINNABAR}" opacity="0.18"/>',
             f'<ellipse cx="960" cy="580" rx="760" ry="340" fill="#05060A" opacity="0.65"/>',
             f"</g>",
             f'<rect x="0" y="168" width="{w}" height="2" fill="{CINNABAR}" opacity="0.16"/>',
             f'<rect x="0" y="912" width="{w}" height="2" fill="{CINNABAR}" opacity="0.12"/>',
             f'<g opacity="0.25">',
             f'<rect x="1500" y="120" width="70" height="3" fill="{CINNABAR}"/>',
             f'<rect x="1520" y="131" width="50" height="2" fill="{CINNABAR}"/>',
             f"</g>",
         ])), bg_defs(CINNABAR, 139), usage="战斗页背景（中部留白给卦台，敌人区偏暗红）",
         color="远山 + 朱砂暗压 + 中央墨池")

    emit("Background", "bg_shop", w, h,
         base_body((1380, 420), "".join([
             f'<g filter="url(#mist)" opacity="0.55">',
             f'<ellipse cx="1380" cy="420" rx="560" ry="440" fill="{GOLD}" opacity="0.16"/>',
             f"</g>",
             "".join(
                 f'<rect x="220" y="{318 + i * 86}" width="1480" height="2" fill="{GOLD}" '
                 f'opacity="0.1"/>' for i in range(6)
             ),
             f'<rect x="220" y="300" width="104" height="4" fill="{CINNABAR}" opacity="0.45"/>',
             f'<rect x="340" y="300" width="60" height="3" fill="{GOLD}" opacity="0.4"/>',
         ])), bg_defs(GOLD, 149), usage="坊市页面背景（货架横线 + 暖光）",
         color="远山 + 符金暖光 + 货架横线")

    grid_strong = "".join(
        f'<rect x="0" y="{i * 108}" width="{w}" height="1.5" fill="{PAPER}" opacity="0.05"/>'
        for i in range(10)
    )
    emit("Background", "bg_codex", w, h,
         base_body((960, 520), "".join([
             f'<g filter="url(#mist)" opacity="0.5">',
             f'<ellipse cx="960" cy="520" rx="860" ry="520" fill="{PAPER}" opacity="0.08"/>',
             f"</g>",
             grid_strong,
             f'<rect x="120" y="120" width="1680" height="840" fill="none" stroke="{PAPER}" '
             f'stroke-opacity="0.1" stroke-width="2"/>',
             f'<rect x="120" y="120" width="140" height="4" fill="{CINNABAR}" opacity="0.4"/>',
         ])), bg_defs(PAPER, 151), usage="卦象图鉴背景（网格更强，8×8 矩阵叠在其上）",
         color="远山 + 灰纸网格 + 方框")

    emit("Background", "bg_page", w, h,
         base_body((960, 260), "".join([
             f'<g filter="url(#mist)" opacity="0.45">',
             f'<ellipse cx="960" cy="260" rx="700" ry="280" fill="{GOLD}" opacity="0.1"/>',
             f"</g>",
             f'<rect x="180" y="152" width="1560" height="2" fill="{GOLD}" opacity="0.14"/>',
             f'<rect x="180" y="152" width="120" height="4" fill="{CINNABAR}" opacity="0.35"/>',
         ])), bg_defs(GOLD, 157), usage="通用页面背景（战斗结算/奇遇/晋升/一局结算/局外成长/弹窗全屏层）",
         color="远山 + 符金顶光")


def main():
    build_icons()
    build_hexagram()
    build_backgrounds()
    with open(os.path.join(HERE, "manifest_symbols.json"), "w", encoding="utf-8") as f:
        json.dump(ITEMS, f, ensure_ascii=False, indent=2)
    print(f"generated {len(ITEMS)} symbol svg")


if __name__ == "__main__":
    main()
