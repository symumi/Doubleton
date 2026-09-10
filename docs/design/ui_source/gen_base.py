# -*- coding: utf-8 -*-
"""《卦锻》UI 基础素材 SVG 生成器：通用面板 / 按钮 / 边框 / 进度条。

运行：python docs/design/ui_source/gen_base.py
输出：docs/design/ui_source/svg/<Category>/<name>.svg 与 manifest_base.json
后续：由 render_png.ps1 渲染为 PNG 并写入 Assets/Resources/Texture/UI。

风格目标（墨狱异志录）：册页纸感 + 墨晕扩散 + 枯笔边缘 + 符金木牌质感。
每个素材至少三层：底色渐变 → 材质纹理 → 描边/高光/阴影，避免出现纯色块。
九宫格素材的装饰只出现在四角或四条边内，中心区域保持低频纹理，避免拉伸变形。
按钮、卡牌不烘焙文字，文字一律由 TextMeshPro 叠加。
"""

import json
import os

HERE = os.path.dirname(os.path.abspath(__file__))
SVG_ROOT = os.path.join(HERE, "svg")

INK = "#070808"
DEEP_INK = "#04050a"
PANEL_TOP = "#2b2b26"
PANEL_BOTTOM = "#111211"
PAPER = "#C8BFA8"
GOLD = "#C2AA72"
GOLD_LIGHT = "#E9DAA6"
GOLD_DARK = "#8A6E3C"
GOLD_DEEP = "#6B5322"
CINNABAR = "#B63A31"
CINNABAR_LIGHT = "#D9705F"
SPIRIT = "#5EC8C0"
SPIRIT_LIGHT = "#8FE0D8"

FONT_CN = "KaiTi, STKaiti, SimSun, serif"

ITEMS = []


# ---------------------------------------------------------------- 通用滤镜与图案

def linear_grad(gid, c0, c1, o0=1.0, o1=1.0, vertical=True):
    x1, y1, x2, y2 = ("0", "0", "0", "1") if vertical else ("0", "0", "1", "0")
    return (
        f'<linearGradient id="{gid}" x1="{x1}" y1="{y1}" x2="{x2}" y2="{y2}">'
        f'<stop offset="0" stop-color="{c0}" stop-opacity="{o0}"/>'
        f'<stop offset="1" stop-color="{c1}" stop-opacity="{o1}"/>'
        f"</linearGradient>"
    )


def multi_grad(gid, stops, vertical=True):
    """stops: [(offset, color, opacity), ...]，用于做有立体感的四段渐变。"""
    x1, y1, x2, y2 = ("0", "0", "0", "1") if vertical else ("0", "0", "1", "0")
    body = "".join(
        f'<stop offset="{offset}" stop-color="{color}" stop-opacity="{opacity}"/>'
        for offset, color, opacity in stops
    )
    return f'<linearGradient id="{gid}" x1="{x1}" y1="{y1}" x2="{x2}" y2="{y2}">{body}</linearGradient>'


def radial_grad(gid, c0, c1, o0=1.0, o1=0.0, cx=0.5, cy=0.5, r=0.7):
    return (
        f'<radialGradient id="{gid}" cx="{cx}" cy="{cy}" r="{r}">'
        f'<stop offset="0" stop-color="{c0}" stop-opacity="{o0}"/>'
        f'<stop offset="1" stop-color="{c1}" stop-opacity="{o1}"/>'
        f"</radialGradient>"
    )


def blur(fid, radius):
    return (
        f'<filter id="{fid}" x="-50%" y="-50%" width="200%" height="200%">'
        f'<feGaussianBlur stdDeviation="{radius}"/></filter>'
    )


def paper_filter(fid="paper", freq=1.15, octaves=4, seed=7):
    """宣纸纤维：低频到高频噪声，配合 mix-blend-mode 叠加。"""
    return (
        f'<filter id="{fid}" x="0" y="0" width="100%" height="100%">'
        f'<feTurbulence type="fractalNoise" baseFrequency="{freq}" numOctaves="{octaves}" '
        f'seed="{seed}" stitchTiles="stitch" result="n"/>'
        f'<feColorMatrix in="n" type="saturate" values="0"/>'
        f"</filter>"
    )


def rough_filter(fid="rough", freq=0.03, scale=3, seed=5):
    """枯笔边缘：噪声位移让直边变成手绘墨线。"""
    return (
        f'<filter id="{fid}" x="-25%" y="-25%" width="150%" height="150%">'
        f'<feTurbulence type="fractalNoise" baseFrequency="{freq}" numOctaves="3" seed="{seed}" result="n"/>'
        f'<feDisplacementMap in="SourceGraphic" in2="n" scale="{scale}" '
        f'xChannelSelector="R" yChannelSelector="G"/>'
        f"</filter>"
    )


def wood_pattern(pid="grain", base="#000000", dark=0.07, light=0.05, step=7):
    """木牌/纸纹竖纹，随图缩放，横向拉伸只会变宽不会断裂。"""
    return (
        f'<pattern id="{pid}" width="{step}" height="{step}" patternUnits="userSpaceOnUse">'
        f'<rect width="{step}" height="{step}" fill="none"/>'
        f'<rect x="1" width="1.2" height="{step}" fill="{base}" opacity="{dark}"/>'
        f'<rect x="{step * 0.6:.1f}" width="0.9" height="{step}" fill="#ffffff" opacity="{light}"/>'
        f"</pattern>"
    )


def emit(category, name, w, h, body, defs="", border=None, usage="", nine_slice=True, color=""):
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


def corner_brackets(inset, length, thickness, color, opacity, w, h):
    """册页装订角标：四角 L 形折角。"""
    i = inset
    l = length
    t = thickness
    return "".join([
        f'<path d="M{i},{i} L{i + l},{i} L{i + l},{i + t} L{i + t},{i + t} L{i + t},{i + l} L{i},{i + l} Z" '
        f'fill="{color}" opacity="{opacity}"/>',
        f'<path d="M{w - i},{i} L{w - i - l},{i} L{w - i - l},{i + t} L{w - i - t},{i + t} '
        f'L{w - i - t},{i + l} L{w - i},{i + l} Z" fill="{color}" opacity="{opacity}"/>',
        f'<path d="M{i},{h - i} L{i + l},{h - i} L{i + l},{h - i - t} L{i + t},{h - i - t} '
        f'L{i + t},{h - i - l} L{i},{h - i - l} Z" fill="{color}" opacity="{opacity * 0.72:.2f}"/>',
        f'<path d="M{w - i},{h - i} L{w - i - l},{h - i} L{w - i - l},{h - i - t} L{w - i - t},{h - i - t} '
        f'L{w - i - t},{h - i - l} L{w - i},{h - i - l} Z" fill="{color}" opacity="{opacity * 0.72:.2f}"/>',
    ])


def ink_blots(corners, blur_id):
    """角落墨渍：多层模糊椭圆，产生水墨扩散。"""
    parts = [f'<g filter="url(#{blur_id})">']
    for cx, cy, rx, ry, opacity in corners:
        parts.append(f'<ellipse cx="{cx}" cy="{cy}" rx="{rx}" ry="{ry}" fill="{DEEP_INK}" opacity="{opacity}"/>')
    parts.append("</g>")
    return "".join(parts)


# ---------------------------------------------------------------- 通用面板

def build_panels():
    # 主册页面板
    w = h = 256
    defs = (
        linear_grad("paperG", PANEL_TOP, PANEL_BOTTOM)
        + radial_grad("paperGlow", PAPER, "#000000", 0.21, 0.0, 0.5, 0.4, 0.78)
        + paper_filter()
        + rough_filter()
        + blur("ink", 13)
    )
    body = "".join([
        f'<rect x="3" y="4" width="250" height="250" rx="11" fill="{DEEP_INK}" opacity="0.8"/>',
        f'<rect x="5" y="5" width="246" height="246" rx="10" fill="url(#paperG)"/>',
        f'<rect x="5" y="5" width="246" height="246" rx="10" fill="url(#paperGlow)"/>',
        f'<rect x="5" y="5" width="246" height="246" rx="10" filter="url(#paper)" opacity="0.45" '
        f'style="mix-blend-mode:overlay"/>',
        ink_blots([
            (20, 26, 76, 58, 0.8), (238, 232, 84, 64, 0.72), (246, 16, 54, 42, 0.45),
            (10, 246, 64, 48, 0.5), (128, 4, 96, 26, 0.34), (128, 252, 100, 28, 0.34),
        ], "ink"),
        f'<rect x="5.9" y="5.9" width="244.2" height="244.2" rx="10" fill="none" stroke="{GOLD}" '
        f'stroke-opacity="0.78" stroke-width="2" filter="url(#rough)"/>',
        f'<rect x="12" y="12" width="232" height="232" rx="6" fill="none" stroke="{GOLD}" '
        f'stroke-opacity="0.22" stroke-width="1"/>',
        corner_brackets(13, 33, 5, GOLD, 0.65, w, h),
        f'<rect x="58" y="17" width="30" height="3.5" fill="{CINNABAR}" opacity="0.95"/>',
        f'<rect x="168" y="17" width="30" height="3.5" fill="{SPIRIT}" opacity="0.9"/>',
        f'<g transform="rotate(-8 214 212)">',
        f'<rect x="198" y="196" width="32" height="32" rx="3" fill="none" stroke="{CINNABAR}" '
        f'stroke-width="3" opacity="0.8"/>',
        f'<rect x="205" y="203" width="18" height="4" fill="{CINNABAR}" opacity="0.6"/>',
        f'<rect x="205" y="212" width="12" height="4" fill="{CINNABAR}" opacity="0.6"/>',
        f'<rect x="205" y="221" width="16" height="4" fill="{CINNABAR}" opacity="0.6"/>',
        f"</g>",
    ])
    emit("Common", "common_panel_page_9s", w, h, body, defs, border="48,48,48,48",
         usage="一级页面的主册页面板底（路线/结算/坊市/图鉴/局外成长）", color="宣纸底 + 符金木框 + 朱砂印")

    # 内嵌深色底（卦台、手牌区、日志区）
    defs = (
        linear_grad("insetG", "#1a1c1e", "#08090c")
        + radial_grad("insetGlow", "#2e3a3c", "#000000", 0.3, 0.0, 0.5, 0.5, 0.7)
        + paper_filter(seed=11)
        + blur("ink2", 11)
    )
    body = "".join([
        f'<rect x="4" y="4" width="248" height="248" rx="9" fill="url(#insetG)"/>',
        f'<rect x="4" y="4" width="248" height="248" rx="9" fill="url(#insetGlow)"/>',
        f'<rect x="4" y="4" width="248" height="248" rx="9" filter="url(#paper)" opacity="0.35" '
        f'style="mix-blend-mode:overlay"/>',
        ink_blots([(16, 18, 62, 44, 0.6), (240, 238, 68, 48, 0.5)], "ink2"),
        f'<rect x="4.6" y="4.6" width="246.8" height="246.8" rx="9" fill="none" stroke="{INK}" '
        f'stroke-opacity="0.85" stroke-width="1.2"/>',
        f'<rect x="10" y="10" width="236" height="236" rx="6" fill="none" stroke="{GOLD}" '
        f'stroke-opacity="0.16" stroke-width="1"/>',
        f'<rect x="10" y="10" width="46" height="2.5" fill="{GOLD}" opacity="0.35"/>',
        f'<rect x="200" y="10" width="46" height="2.5" fill="{SPIRIT}" opacity="0.28"/>',
        corner_brackets(12, 24, 3.5, GOLD, 0.3, w, h),
    ])
    emit("Common", "common_panel_inset_9s", w, h, body, defs, border="48,48,48,48",
         usage="内嵌深色区：卦台、手牌区、战斗日志、详情页内容区", color="墨池内嵌 + 灵青微光")

    # 敌人 / 杂乱层面板
    defs = (
        linear_grad("enemyG", "#2a1a18", "#0b0809")
        + radial_grad("enemyGlow", "#6b2018", "#000000", 0.35, 0.0, 0.78, 0.22, 0.72)
        + paper_filter(seed=23)
        + rough_filter(seed=9)
        + blur("ink3", 15)
    )
    body = "".join([
        f'<rect x="3" y="4" width="250" height="250" rx="11" fill="{DEEP_INK}" opacity="0.85"/>',
        f'<rect x="5" y="5" width="246" height="246" rx="10" fill="url(#enemyG)"/>',
        f'<rect x="5" y="5" width="246" height="246" rx="10" fill="url(#enemyGlow)"/>',
        f'<rect x="5" y="5" width="246" height="246" rx="10" filter="url(#paper)" opacity="0.5" '
        f'style="mix-blend-mode:overlay"/>',
        ink_blots([
            (14, 20, 84, 62, 0.85), (246, 12, 66, 50, 0.6), (240, 244, 88, 66, 0.75),
            (10, 250, 70, 54, 0.6),
        ], "ink3"),
        f'<rect x="5.9" y="5.9" width="244.2" height="244.2" rx="10" fill="none" stroke="{CINNABAR}" '
        f'stroke-opacity="0.42" stroke-width="1.7" filter="url(#rough)"/>',
        f'<rect x="44" y="17" width="30" height="3.5" fill="{CINNABAR}" opacity="0.9"/>',
        f'<rect x="182" y="17" width="30" height="3.5" fill="{CINNABAR}" opacity="0.45"/>',
        f'<g opacity="0.5">',
        f'<rect x="186" y="196" width="54" height="2" fill="{CINNABAR}"/>',
        f'<rect x="196" y="203" width="44" height="2" fill="{CINNABAR}" opacity="0.7"/>',
        f'<rect x="206" y="210" width="34" height="2" fill="{CINNABAR}" opacity="0.5"/>',
        f"</g>",
        corner_brackets(13, 30, 4.5, CINNABAR, 0.35, w, h),
    ])
    emit("Common", "common_panel_enemy_9s", w, h, body, defs, border="48,48,48,48",
         usage="敌人信息、外部规则、被污染区域的面板底", color="暗红污染 + 墨晕 + 朱砂刻痕")

    # 小分区面板
    w = h = 192
    defs = (
        linear_grad("secG", "#26251f", "#121110")
        + radial_grad("secGlow", PAPER, "#000000", 0.14, 0.0, 0.5, 0.4, 0.8)
        + paper_filter(seed=31)
        + blur("ink4", 10)
    )
    body = "".join([
        f'<rect x="4" y="4" width="184" height="184" rx="8" fill="url(#secG)"/>',
        f'<rect x="4" y="4" width="184" height="184" rx="8" fill="url(#secGlow)"/>',
        f'<rect x="4" y="4" width="184" height="184" rx="8" filter="url(#paper)" opacity="0.45" '
        f'style="mix-blend-mode:overlay"/>',
        ink_blots([(14, 16, 54, 38, 0.6), (180, 178, 56, 40, 0.5)], "ink4"),
        f'<rect x="4.7" y="4.7" width="182.6" height="182.6" rx="8" fill="none" stroke="{GOLD}" '
        f'stroke-opacity="0.45" stroke-width="1.4"/>',
        f'<rect x="16" y="15" width="22" height="3" fill="{GOLD}" opacity="0.75"/>',
        corner_brackets(11, 22, 3, GOLD, 0.4, w, h),
    ])
    emit("Common", "common_panel_section_9s", w, h, body, defs, border="40,40,40,40",
         usage="法宝栏、符箓栏、附魔槽、小型信息分区", color="宣纸底 + 符金细框")

    # 弹窗
    w = h = 384
    defs = (
        linear_grad("popG", "#2e2d27", "#111110")
        + radial_grad("popGlow", PAPER, "#000000", 0.16, 0.0, 0.5, 0.38, 0.75)
        + paper_filter(seed=41)
        + rough_filter(seed=13)
        + blur("popInk", 18)
        + blur("popShadow", 12)
    )
    body = "".join([
        f'<rect x="8" y="10" width="368" height="368" rx="16" fill="{DEEP_INK}" opacity="0.85" '
        f'filter="url(#popShadow)"/>',
        f'<rect x="12" y="12" width="360" height="360" rx="12" fill="url(#popG)"/>',
        f'<rect x="12" y="12" width="360" height="360" rx="12" fill="url(#popGlow)"/>',
        f'<rect x="12" y="12" width="360" height="360" rx="12" filter="url(#paper)" opacity="0.55" '
        f'style="mix-blend-mode:overlay"/>',
        ink_blots([
            (34, 40, 100, 74, 0.75), (352, 348, 108, 78, 0.7), (356, 30, 70, 54, 0.45),
            (24, 356, 82, 62, 0.5),
        ], "popInk"),
        f'<rect x="12.9" y="12.9" width="358.2" height="358.2" rx="12" fill="none" stroke="{GOLD}" '
        f'stroke-opacity="0.62" stroke-width="1.8" filter="url(#rough)"/>',
        f'<rect x="26" y="26" width="332" height="332" rx="8" fill="none" stroke="{GOLD}" '
        f'stroke-opacity="0.2" stroke-width="1"/>',
        corner_brackets(28, 44, 6, GOLD, 0.55, w, h),
        f'<rect x="76" y="27" width="36" height="3.5" fill="{CINNABAR}" opacity="0.95"/>',
        f'<rect x="272" y="27" width="36" height="3.5" fill="{SPIRIT}" opacity="0.9"/>',
        f'<g transform="rotate(-7 320 320)">',
        f'<rect x="298" y="298" width="44" height="44" rx="4" fill="none" stroke="{CINNABAR}" '
        f'stroke-width="3.5" opacity="0.75"/>',
        f'<rect x="307" y="307" width="26" height="5" fill="{CINNABAR}" opacity="0.55"/>',
        f'<rect x="307" y="318" width="18" height="5" fill="{CINNABAR}" opacity="0.55"/>',
        f'<rect x="307" y="329" width="22" height="5" fill="{CINNABAR}" opacity="0.55"/>',
        f"</g>",
    ])
    emit("Common", "common_popup_bg_9s", w, h, body, defs, border="64,64,64,64",
         usage="确认、详情、奖励、购买确认等弹窗底", color="册页纸底 + 双层符金框 + 朱砂印")

    # Tooltip
    w = h = 256
    defs = (
        linear_grad("tipG", "#1c2326", "#07090c")
        + radial_grad("tipGlow", "#2F5F5C", "#000000", 0.35, 0.0, 0.3, 0.2, 0.8)
        + paper_filter(seed=53)
        + blur("tipInk", 12)
    )
    body = "".join([
        f'<rect x="3" y="4" width="250" height="250" rx="9" fill="{DEEP_INK}" opacity="0.85"/>',
        f'<rect x="6" y="6" width="244" height="244" rx="8" fill="url(#tipG)"/>',
        f'<rect x="6" y="6" width="244" height="244" rx="8" fill="url(#tipGlow)"/>',
        f'<rect x="6" y="6" width="244" height="244" rx="8" filter="url(#paper)" opacity="0.45" '
        f'style="mix-blend-mode:overlay"/>',
        ink_blots([(18, 20, 66, 48, 0.7), (238, 236, 70, 52, 0.6)], "tipInk"),
        f'<rect x="6.8" y="6.8" width="242.4" height="242.4" rx="8" fill="none" stroke="{SPIRIT}" '
        f'stroke-opacity="0.5" stroke-width="1.4"/>',
        f'<rect x="20" y="16" width="26" height="3" fill="{SPIRIT}" opacity="0.85"/>',
        corner_brackets(13, 22, 3.5, SPIRIT, 0.4, w, h),
    ])
    emit("Common", "common_tooltip_bg_9s", w, h, body, defs, border="48,48,48,48",
         usage="悬停说明浮层（卡牌/卦象/状态/法宝/武器）", color="墨底 + 灵青框 + 纸纹")

    # Toast
    w, h = 320, 96
    defs = (
        linear_grad("toastG", "#28271f", "#121110", vertical=False)
        + paper_filter(seed=61)
        + blur("toastInk", 10)
    )
    body = "".join([
        f'<rect x="2" y="3" width="316" height="92" rx="8" fill="{DEEP_INK}" opacity="0.8"/>',
        f'<rect x="6" y="6" width="308" height="84" rx="6" fill="url(#toastG)"/>',
        f'<rect x="6" y="6" width="308" height="84" rx="6" filter="url(#paper)" opacity="0.4" '
        f'style="mix-blend-mode:overlay"/>',
        ink_blots([(16, 18, 46, 34, 0.7), (306, 80, 44, 32, 0.55)], "toastInk"),
        f'<rect x="6.8" y="6.8" width="306.4" height="82.4" rx="6" fill="none" stroke="{GOLD}" '
        f'stroke-opacity="0.45" stroke-width="1.4"/>',
        f'<rect x="12" y="22" width="4.5" height="52" rx="2" fill="{SPIRIT}" opacity="0.85"/>',
        f'<rect x="12" y="16" width="24" height="2.5" fill="{GOLD}" opacity="0.6"/>',
    ])
    emit("Common", "common_toast_bg_9s", w, h, body, defs, border="30,30,30,30",
         usage="界面内短提示（灵力不足、法宝栏已满、操作失败）", color="墨底 + 符金细框 + 灵青竖标")

    # 空槽
    w = h = 128
    defs = (linear_grad("slotG", "#1e1d18", "#0e0e0c")
            + paper_filter(freq=1.4, seed=71) + blur("slotInk", 8))
    body = "".join([
        f'<rect x="6" y="6" width="116" height="116" rx="8" fill="url(#slotG)" opacity="0.95"/>',
        f'<rect x="6" y="6" width="116" height="116" rx="8" filter="url(#paper)" opacity="0.35" '
        f'style="mix-blend-mode:overlay"/>',
        ink_blots([(14, 14, 34, 26, 0.6), (116, 116, 34, 26, 0.5)], "slotInk"),
        f'<rect x="7" y="7" width="114" height="114" rx="7" fill="none" stroke="{GOLD}" '
        f'stroke-opacity="0.4" stroke-width="1.6" stroke-dasharray="11 7"/>',
        f'<g opacity="0.3">',
        f'<rect x="60" y="44" width="8" height="40" rx="2" fill="{GOLD}"/>',
        f'<rect x="44" y="60" width="40" height="8" rx="2" fill="{GOLD}"/>',
        f"</g>",
    ])
    emit("Common", "common_slot_empty_9s", w, h, body, defs, border="28,28,28,28",
         usage="法宝槽、符箓槽、附魔槽、状态位空槽", color="虚线符金框 + 纸底")

    # 分隔线
    defs = (radial_grad("divG", GOLD, GOLD, 0.55, 0.0, 0.5, 0.5, 0.5)
            + rough_filter(seed=17))
    body = "".join([
        f'<rect x="0" y="9" width="512" height="6" rx="3" fill="url(#divG)" opacity="0" filter="url(#rough)"/>',
        f'<rect x="60" y="10.5" width="392" height="3" rx="1.5" fill="{GOLD}" opacity="0.5" filter="url(#rough)"/>',
        f'<rect x="216" y="5" width="80" height="14" rx="3" fill="none" stroke="{GOLD}" '
        f'stroke-opacity="0.45" stroke-width="1.5"/>',
        f'<rect x="248" y="10" width="16" height="4" rx="1" fill="{CINNABAR}" opacity="0.75"/>',
    ])
    emit("Common", "common_divider_ink", 512, 24, body, defs, nine_slice=False,
         usage="区块之间的墨线分隔（区间标题上下）", color="符金枯笔线 + 朱砂中点")

    # 角落墨渍
    defs = blur("bigInk", 22) + rough_filter(freq=0.02, scale=26, seed=29)
    body = "".join([
        f'<g filter="url(#bigInk)">',
        f'<ellipse cx="120" cy="130" rx="190" ry="140" fill="{DEEP_INK}" opacity="0.95"/>',
        f'<ellipse cx="250" cy="70" rx="120" ry="80" fill="{DEEP_INK}" opacity="0.8"/>',
        f'<ellipse cx="70" cy="300" rx="90" ry="110" fill="{DEEP_INK}" opacity="0.7"/>',
        f'<ellipse cx="210" cy="230" rx="150" ry="90" fill="{DEEP_INK}" opacity="0.65"/>',
        f"</g>",
        f'<g filter="url(#rough)" opacity="0.55">',
        f'<ellipse cx="150" cy="150" rx="150" ry="110" fill="{DEEP_INK}"/>',
        f"</g>",
    ])
    emit("Common", "common_decor_ink_corner", 512, 512, body, defs, nine_slice=False,
         usage="面板边缘墨渍溢出（旋转后贴四个角，杂乱层）", color="墨晕扩散 + 枯笔碎边")

    # 朱砂红印
    body = "".join([
        f'<g transform="rotate(-6 96 96)">',
        f'<rect x="24" y="24" width="144" height="144" rx="10" fill="{CINNABAR}" opacity="0.12"/>',
        f'<rect x="24" y="24" width="144" height="144" rx="10" fill="none" stroke="{CINNABAR}" '
        f'stroke-width="9" opacity="0.9"/>',
        f'<rect x="43" y="43" width="106" height="106" fill="none" stroke="{CINNABAR}" '
        f'stroke-width="3.5" opacity="0.75"/>',
        f'<rect x="56" y="56" width="80" height="7" rx="1" fill="{CINNABAR}" opacity="0.72"/>',
        f'<rect x="56" y="76" width="52" height="7" rx="1" fill="{CINNABAR}" opacity="0.72"/>',
        f'<rect x="56" y="96" width="66" height="7" rx="1" fill="{CINNABAR}" opacity="0.72"/>',
        f'<rect x="56" y="116" width="40" height="7" rx="1" fill="{CINNABAR}" opacity="0.72"/>',
        f'<rect x="108" y="76" width="7" height="47" rx="1" fill="{CINNABAR}" opacity="0.65"/>',
        f"</g>",
    ])
    emit("Common", "common_decor_seal_red", 192, 192, body, nine_slice=False,
         usage="渡劫、危险、封印、未解锁标记（叠加在标题或节点上）", color="朱砂印章（带底纹）")

    # 扫描线平铺
    defs = blur("scanSoft", 0.6)
    lines = "".join(
        f'<rect y="{y}" width="64" height="1.4" fill="{SPIRIT}" opacity="{opacity}" filter="url(#scanSoft)"/>'
        for y, opacity in ((4, 0.5), (18, 0.22), (33, 0.44), (47, 0.2), (58, 0.34))
    )
    body = lines + f'<rect x="0" y="0" width="64" height="64" fill="none"/>'
    emit("Common", "common_decor_scanline", 64, 64, body, defs, nine_slice=False,
         usage="灵青扫描线，Tiled 平铺在敌人区/图鉴锁区/标题条（低透明度使用）", color="灵青扫描线")

    # 宣纸纤维平铺
    defs = (
        '<filter id="fiber" x="0" y="0" width="100%" height="100%">'
        '<feTurbulence type="fractalNoise" baseFrequency="0.62" numOctaves="4" seed="3" '
        'stitchTiles="stitch" result="n"/>'
        '<feColorMatrix in="n" type="matrix" values="0 0 0 0 0.784  0 0 0 0 0.749  '
        '0 0 0 0 0.658  0.8 0.8 0.8 0 -0.9"/>'
        "</filter>"
    )
    body = f'<rect width="256" height="256" fill="{INK}" filter="url(#fiber)"/>'
    emit("Common", "common_decor_paper_fiber", 256, 256, body, defs, nine_slice=False,
         usage="宣纸纤维叠加层，Tiled 平铺在页面背景之上（Image.color.a 0.15–0.25）", color="灰纸纤维")

    # “志”字水印
    defs = blur("wmSoft", 1.2)
    body = (
        f'<g filter="url(#wmSoft)">'
        f'<text x="128" y="200" font-family="{FONT_CN}" font-size="220" text-anchor="middle" '
        f'fill="{PAPER}" opacity="0.16">志</text>'
        f'<text x="131" y="203" font-family="{FONT_CN}" font-size="220" text-anchor="middle" '
        f'fill="{CINNABAR}" opacity="0.07">志</text>'
        f"</g>"
    )
    emit("Common", "common_watermark_zhi", 256, 256, body, defs, nine_slice=False,
         usage="册页水印，贴在主面板右下角（低透明度）", color="灰纸水印 + 朱砂错位")

    # 遮罩
    body = f'<rect width="64" height="64" fill="{INK}" fill-opacity="0.78"/>'
    emit("Common", "common_overlay_dim", 64, 64, body, nine_slice=False,
         usage="弹窗遮罩（Image 拉伸铺满，blockRaycast 开启）", color="墨黑 78%")

    # 牌背
    w, h = 256, 384
    defs = (
        linear_grad("backG", "#28271f", "#111110")
        + radial_grad("backGlow", PAPER, "#000000", 0.16, 0.0, 0.5, 0.42, 0.8)
        + paper_filter(seed=83)
        + rough_filter(seed=19)
        + blur("backInk", 14)
    )
    gua_lines = "".join(
        f'<rect x="60" y="{126 + i * 22}" width="136" height="8" rx="3" fill="{GOLD}" opacity="0.6"/>'
        for i in range(6)
    )
    body = "".join([
        f'<rect x="3" y="4" width="250" height="378" rx="16" fill="{DEEP_INK}" opacity="0.85"/>',
        f'<rect x="6" y="6" width="244" height="372" rx="14" fill="url(#backG)"/>',
        f'<rect x="6" y="6" width="244" height="372" rx="14" fill="url(#backGlow)"/>',
        f'<rect x="6" y="6" width="244" height="372" rx="14" filter="url(#paper)" opacity="0.5" '
        f'style="mix-blend-mode:overlay"/>',
        ink_blots([(24, 30, 76, 56, 0.75), (236, 350, 84, 62, 0.7), (240, 24, 56, 44, 0.45),
                   (16, 356, 62, 46, 0.5)], "backInk"),
        f'<rect x="6.9" y="6.9" width="242.2" height="370.2" rx="14" fill="none" stroke="{GOLD}" '
        f'stroke-opacity="0.52" stroke-width="1.7" filter="url(#rough)"/>',
        f'<rect x="20" y="20" width="216" height="344" rx="8" fill="none" stroke="{GOLD}" '
        f'stroke-opacity="0.22" stroke-width="1"/>',
        f'<circle cx="128" cy="192" r="82" fill="none" stroke="{GOLD}" stroke-opacity="0.22" stroke-width="1.5"/>',
        f'<circle cx="128" cy="192" r="74" fill="none" stroke="{CINNABAR}" stroke-opacity="0.14" stroke-width="1"/>',
        gua_lines,
        f'<rect x="34" y="28" width="24" height="3" fill="{CINNABAR}" opacity="0.75"/>',
        f'<rect x="198" y="28" width="24" height="3" fill="{SPIRIT}" opacity="0.65"/>',
        corner_brackets(16, 26, 4, GOLD, 0.45, w, h),
    ])
    emit("Common", "common_card_back", w, h, body, defs, border="40,40,40,40",
         usage="抽牌堆、牌组查看、未翻开的卡牌背面", color="册页牌背 + 六爻符纹 + 朱砂角印")


# ---------------------------------------------------------------- 按钮

# 每种按钮四种状态：四段渐变 + 边框色 + 外发光 + 文字底色
BUTTON_STYLES = {
    "primary": {
        "normal": ("#EFDFAC", "#CDAE71", "#A8874B", "#7A6033", GOLD_DEEP, "#F0E3BB", 0.0),
        "hover": ("#FBEBC0", "#DEBF80", "#B8975A", "#8A6E3C", GOLD_DARK, SPIRIT, 0.85),
        "pressed": ("#B9975A", "#9A7A42", "#7E6234", "#5E4A26", "#4E3C1E", "#C9B27A", 0.0),
        "disabled": ("#4A4739", "#3A382E", "#2E2C24", "#232219", "#3A382E", "#6A6552", 0.0),
    },
    "secondary": {
        "normal": ("#2E3D35", "#22302A", "#1A2420", "#111815", SPIRIT, "#7FD8D0", 0.0),
        "hover": ("#3A4C42", "#2A3A33", "#202C27", "#16201C", SPIRIT_LIGHT, SPIRIT, 0.7),
        "pressed": ("#1A2420", "#141C19", "#101715", "#0B100E", "#2A4A46", "#5EA9A3", 0.0),
        "disabled": ("#333831", "#2A2F29", "#232722", "#1C1F1B", "#43483F", "#575D52", 0.0),
    },
    "danger": {
        "normal": ("#5E241D", "#451914", "#35110E", "#240C09", CINNABAR, CINNABAR_LIGHT, 0.0),
        "hover": ("#7A2E24", "#5A1F19", "#451713", "#2E0F0C", CINNABAR_LIGHT, CINNABAR, 0.75),
        "pressed": ("#35110E", "#290D0B", "#200A08", "#170706", "#7A2E24", "#A85A4A", 0.0),
        "disabled": ("#37302E", "#2C2624", "#242020", "#1C1A19", "#4A403D", "#5E514D", 0.0),
    },
}

BUTTON_STATE_LABEL = {"normal": "常态", "hover": "悬停", "pressed": "按下", "disabled": "禁用"}
BUTTON_KIND_LABEL = {
    "primary": "主行动（出卦/确认/购买）",
    "secondary": "次行动（弃牌/返回/取消）",
    "danger": "危险操作（回合结束/放弃本局）",
}
BUTTON_KIND_COLOR = {
    "primary": "符金木牌",
    "secondary": "墨绿木牌 + 灵青边",
    "danger": "暗红木牌 + 朱砂边",
}


def build_buttons():
    w, h = 320, 96
    for kind, states in BUTTON_STYLES.items():
        for state, (top, mid, low, bottom, edge, outer, glow_op) in states.items():
            defs = (
                multi_grad("btnG", [
                    (0, top, 1), (0.42, mid, 1), (0.62, low, 1), (1, bottom, 1),
                ])
                + wood_pattern()
                + blur("btnGlow", 11)
                + paper_filter(freq=1.4, octaves=2, seed=97)
            )
            dy = 2 if state == "pressed" else 0
            parts = []
            if glow_op > 0:
                parts.append(
                    f'<rect x="16" y="{16 + dy}" width="288" height="66" rx="11" fill="{SPIRIT}" '
                    f'filter="url(#btnGlow)" opacity="{glow_op}"/>'
                )
            parts += [
                f'<rect x="8" y="{14 + dy}" width="304" height="74" rx="12" fill="{DEEP_INK}" opacity="0.8"/>',
                f'<rect x="10" y="{12 + dy}" width="300" height="72" rx="10" fill="url(#btnG)"/>',
                f'<rect x="10" y="{12 + dy}" width="300" height="72" rx="10" filter="url(#paper)" '
                f'opacity="0.28" style="mix-blend-mode:overlay"/>',
                f'<rect x="10" y="{12 + dy}" width="300" height="72" rx="10" fill="url(#grain)" opacity="0.38"/>',
            ]
            if state == "pressed":
                parts.append(
                    f'<rect x="10" y="{12 + dy}" width="300" height="30" rx="10" fill="#000000" opacity="0.32"/>'
                )
            parts += [
                f'<rect x="18" y="{16 + dy}" width="284" height="3.5" rx="1.5" fill="#FFF8E0" opacity="0.5"/>',
                f'<rect x="18" y="{76 + dy}" width="284" height="3" rx="1.5" fill="#1B1405" opacity="0.5"/>',
                f'<rect x="17" y="{20 + dy}" width="286" height="56" rx="6" fill="none" stroke="{edge}" '
                f'stroke-opacity="0.45" stroke-width="1"/>',
                f'<rect x="10.9" y="{12.9 + dy}" width="298.2" height="70.2" rx="10" fill="none" '
                f'stroke="{outer}" stroke-opacity="0.8" stroke-width="1.7"/>',
            ]
            for cx, cy, sx, sy in ((18, 22, 1, 1), (302, 22, -1, 1), (18, 74, 1, -1), (302, 74, -1, -1)):
                parts.append(
                    f'<path d="M{cx},{cy + dy} l{sx * 16},0 l0,{sy * 4} l{-sx * 11},0 l0,{sy * 11} '
                    f'l{-sx * 5},0 z" fill="{edge}" opacity="0.5"/>'
                )
            emit("Button", f"button_{kind}_{state}", w, h, "".join(parts), defs,
                 border="40,36,40,36",
                 usage=f"{BUTTON_KIND_LABEL[kind]} - {BUTTON_STATE_LABEL[state]}",
                 color=BUTTON_KIND_COLOR[kind])


# ---------------------------------------------------------------- 边框

def build_frames():
    # 选中框（灵青）
    w = h = 160
    defs = blur("selGlow", 9) + rough_filter(freq=0.05, scale=4, seed=31) + paper_filter(freq=1.2, octaves=2, seed=37)
    body = "".join([
        f'<rect x="16" y="16" width="128" height="128" rx="12" fill="{SPIRIT}" opacity="0.1"/>',
        f'<rect x="16" y="16" width="128" height="128" rx="12" fill="none" stroke="{SPIRIT}" '
        f'stroke-width="7" opacity="0.55" filter="url(#selGlow)"/>',
        f'<rect x="16" y="16" width="128" height="128" rx="12" fill="none" stroke="{SPIRIT}" '
        f'stroke-width="3.5" opacity="0.95" filter="url(#rough)"/>',
        f'<rect x="21" y="21" width="118" height="118" rx="9" fill="none" stroke="{SPIRIT_LIGHT}" '
        f'stroke-opacity="0.35" stroke-width="1"/>',
        f'<path d="M14,14 L52,14 L52,20 L20,20 L20,52 L14,52 Z" fill="{SPIRIT}" opacity="0.9"/>',
        f'<path d="M146,14 L108,14 L108,20 L140,20 L140,52 L146,52 Z" fill="{SPIRIT}" opacity="0.9"/>',
        f'<path d="M14,146 L52,146 L52,140 L20,140 L20,108 L14,108 Z" fill="{SPIRIT}" opacity="0.65"/>',
        f'<path d="M146,146 L108,146 L108,140 L140,140 L140,108 L146,108 Z" fill="{SPIRIT}" opacity="0.65"/>',
    ])
    emit("Frame", "frame_select_spirit_9s", w, h, body, defs, border="40,40,40,40",
         usage="当前选中态外框（选中卡牌、选中商品、选中图鉴格）", color="灵青实线 + 光晕 + 四角折标")

    # 待确认框（符金）
    defs = blur("goldGlow", 8) + rough_filter(freq=0.05, scale=4, seed=43)
    body = "".join([
        f'<rect x="16" y="16" width="128" height="128" rx="12" fill="{GOLD}" opacity="0.1"/>',
        f'<rect x="16" y="16" width="128" height="128" rx="12" fill="none" stroke="{GOLD}" '
        f'stroke-width="6" opacity="0.6" filter="url(#goldGlow)"/>',
        f'<rect x="16" y="16" width="128" height="128" rx="12" fill="none" stroke="{GOLD_LIGHT}" '
        f'stroke-width="3" opacity="0.95" filter="url(#rough)"/>',
        f'<rect x="21" y="21" width="118" height="118" rx="9" fill="none" stroke="{GOLD}" '
        f'stroke-opacity="0.3" stroke-width="1"/>',
        f'<path d="M14,14 L52,14 L52,20 L20,20 L20,52 L14,52 Z" fill="{GOLD}" opacity="0.9"/>',
        f'<path d="M146,146 L108,146 L108,140 L140,140 L140,108 L146,108 Z" fill="{GOLD}" opacity="0.9"/>',
    ])
    emit("Frame", "frame_select_gold_9s", w, h, body, defs, border="40,40,40,40",
         usage="待确认 / 已选作内外卦但未出卦的外框", color="符金实线 + 光晕")

    # 卡牌底框
    w, h = 256, 384

    def card_body(edge, edge_op, face_top, face_bottom, selected=False, disabled=False, stars=0,
                  glow=True):
        defs = (
            linear_grad("cardG", face_top, face_bottom)
            + radial_grad("cardGlow", PAPER, "#000000", 0.13, 0.0, 0.5, 0.3, 0.85)
            + paper_filter(seed=101)
            + rough_filter(seed=53)
            + blur("cardInk", 13)
            + blur("cardHalo", 9)
        )
        parts = [
            f'<rect x="3" y="4" width="250" height="378" rx="16" fill="{DEEP_INK}" opacity="0.85"/>',
            f'<rect x="6" y="6" width="244" height="372" rx="14" fill="url(#cardG)"/>',
            f'<rect x="6" y="6" width="244" height="372" rx="14" fill="url(#cardGlow)"/>',
            f'<rect x="6" y="6" width="244" height="372" rx="14" filter="url(#paper)" opacity="0.5" '
            f'style="mix-blend-mode:overlay"/>',
            ink_blots([(22, 26, 68, 50, 0.65), (238, 356, 72, 54, 0.6)], "cardInk"),
        ]
        if selected and glow:
            parts.append(
                f'<rect x="6" y="6" width="244" height="372" rx="14" fill="none" stroke="{SPIRIT}" '
                f'stroke-width="6" opacity="0.65" filter="url(#cardHalo)"/>'
            )
        parts += [
            f'<rect x="6.9" y="6.9" width="242.2" height="370.2" rx="14" fill="none" stroke="{edge}" '
            f'stroke-opacity="{edge_op}" stroke-width="2" filter="url(#rough)"/>',
            f'<rect x="20" y="20" width="216" height="344" rx="8" fill="none" stroke="{GOLD}" '
            f'stroke-opacity="0.2" stroke-width="1"/>',
            # 卡图位（略亮）
            f'<rect x="34" y="44" width="188" height="196" rx="8" fill="{PAPER}" opacity="0.05"/>',
            f'<rect x="34" y="44" width="188" height="196" rx="8" fill="none" stroke="{GOLD}" '
            f'stroke-opacity="0.18" stroke-width="1"/>',
            # 玄纹角标
            f'<path d="M22,22 l18,0 l0,3.5 l-14.5,0 l0,14.5 l-3.5,0 z" fill="{edge}" opacity="{edge_op * 0.8:.2f}"/>',
            f'<path d="M234,22 l-18,0 l0,3.5 l14.5,0 l0,14.5 l3.5,0 z" fill="{edge}" opacity="{edge_op * 0.8:.2f}"/>',
            f'<rect x="34" y="252" width="188" height="1.5" fill="{GOLD}" opacity="0.3"/>',
            f'<rect x="34" y="330" width="188" height="1.5" fill="{GOLD}" opacity="0.22"/>',
        ]
        if disabled:
            parts.append(
                f'<rect x="6" y="6" width="244" height="372" rx="14" fill="{DEEP_INK}" opacity="0.55"/>'
            )
            parts.append(
                '<g opacity="0.14">'
                + "".join(
                    f'<rect x="-60" y="{30 + i * 34}" width="420" height="7" fill="{PAPER}" '
                    f'transform="rotate(-26 128 192)"/>' for i in range(12)
                )
                + "</g>"
            )
        for i in range(stars):
            cx = 224 - i * 26
            parts.append(
                f'<path d="M{cx},36 l6.6,13.6 15,2 -10.8,10.5 2.6,14.9 -13.4,-7.1 -13.4,7.1 '
                f'2.6,-14.9 -10.8,-10.5 15,-2 Z" fill="{GOLD_LIGHT}" opacity="0.95"/>'
            )
        return defs, "".join(parts)

    defs, body = card_body(GOLD, 0.62, "#2c2b25", "#111110")
    emit("Frame", "card_frame_normal_9s", w, h, body, defs, border="40,40,40,40",
         usage="手牌、牌组、弃牌预览的基础卡牌底框", color="宣纸卡面 + 符金细框")

    defs, body = card_body(SPIRIT, 0.9, "#1e2f2b", "#0a1210", selected=True)
    emit("Frame", "card_frame_selected_9s", w, h, body, defs, border="40,40,40,40",
         usage="已选为内卦/外卦的卡牌底框（灵青高亮）", color="灵青粗边 + 外光晕")

    defs, body = card_body("#6B6552", 0.4, "#1f1e1a", "#0e0e0c", disabled=True)
    emit("Frame", "card_frame_disabled_9s", w, h, body, defs, border="40,40,40,40",
         usage="不可用卡牌（灵力不足、结算中锁定、禁用）", color="灰褐 + 斜纹压暗")

    rank_face = {1: ("#2a2923", "#101010"), 2: ("#33301f", "#121110"), 3: ("#3d3218", "#141210")}
    rank_edge = {1: (GOLD, 0.55), 2: (GOLD, 0.75), 3: (GOLD_LIGHT, 0.95)}
    for rank in (1, 2, 3):
        top, bottom = rank_face[rank]
        edge, edge_op = rank_edge[rank]
        defs, body = card_body(edge, edge_op, top, bottom, stars=rank)
        emit("Frame", f"card_frame_rank{rank}_9s", w, h, body, defs, border="40,40,40,40",
             usage=f"{rank} 星品阶卡牌底框（右上角 {rank} 颗星标）", color="符金边 + 金星标")

    # 内外卦角标
    w, h = 128, 64
    defs = (multi_grad("tagInner", [(0, SPIRIT_LIGHT, 1), (0.5, SPIRIT, 1), (1, "#2B6F6A", 1)])
            + rough_filter(freq=0.06, scale=3, seed=61))
    body = "".join([
        f'<path d="M6,6 L122,6 L122,36 L64,60 L6,36 Z" fill="url(#tagInner)" filter="url(#rough)"/>',
        f'<path d="M12,12 L116,12 L116,34 L64,54 L12,34 Z" fill="none" stroke="{INK}" '
        f'stroke-opacity="0.35" stroke-width="1.5"/>',
        f'<path d="M46,24 L64,42 L82,24" fill="none" stroke="{INK}" stroke-width="6" '
        f'stroke-opacity="0.8" stroke-linecap="round"/>',
    ])
    emit("Frame", "card_tag_inner", w, h, body, defs, nine_slice=False,
         usage="内卦（下卦）角标形状，文字“内卦”由 TMP 叠加在形状中央", color="灵青木牌角标")

    defs = (multi_grad("tagOuter", [(0, "#F0E3BB", 1), (0.5, GOLD, 1), (1, GOLD_DARK, 1)])
            + rough_filter(freq=0.06, scale=3, seed=67))
    body = "".join([
        f'<path d="M6,58 L122,58 L122,28 L64,4 L6,28 Z" fill="url(#tagOuter)" filter="url(#rough)"/>',
        f'<path d="M12,52 L116,52 L116,30 L64,10 L12,30 Z" fill="none" stroke="{INK}" '
        f'stroke-opacity="0.35" stroke-width="1.5"/>',
        f'<path d="M46,40 L64,22 L82,40" fill="none" stroke="{INK}" stroke-width="6" '
        f'stroke-opacity="0.8" stroke-linecap="round"/>',
    ])
    emit("Frame", "card_tag_outer", w, h, body, defs, nine_slice=False,
         usage="外卦（上卦）角标形状，文字“外卦”由 TMP 叠加在形状中央", color="符金木牌角标")


# ---------------------------------------------------------------- 进度条

def build_bars():
    w, h = 160, 40

    def bar(name, colors, track=False, usage="", color=""):
        if track:
            defs = (linear_grad("trackG", "#15161a", "#05060a")
                    + paper_filter(freq=1.2, octaves=2, seed=113))
            body = "".join([
                f'<rect x="2" y="6" width="156" height="28" rx="9" fill="{DEEP_INK}" opacity="0.85"/>',
                f'<rect x="4" y="8" width="152" height="24" rx="7" fill="url(#trackG)"/>',
                f'<rect x="4" y="8" width="152" height="24" rx="7" filter="url(#paper)" opacity="0.35" '
                f'style="mix-blend-mode:overlay"/>',
                f'<rect x="4.8" y="8.8" width="150.4" height="22.4" rx="7" fill="none" stroke="{GOLD}" '
                f'stroke-opacity="0.3" stroke-width="1.3"/>',
                f'<rect x="10" y="11" width="26" height="2.5" rx="1" fill="{PAPER}" opacity="0.16"/>',
            ])
        else:
            top, mid, bottom, edge = colors
            defs = (multi_grad("fillG", [(0, top, 1), (0.5, mid, 1), (1, bottom, 1)])
                    + paper_filter(freq=1.6, octaves=2, seed=127))
            body = "".join([
                f'<rect x="2" y="6" width="156" height="28" rx="9" fill="{DEEP_INK}" opacity="0.8"/>',
                f'<rect x="4" y="8" width="152" height="24" rx="7" fill="url(#fillG)"/>',
                f'<rect x="4" y="8" width="152" height="24" rx="7" filter="url(#paper)" opacity="0.28" '
                f'style="mix-blend-mode:overlay"/>',
                f'<rect x="8" y="10.5" width="142" height="4" rx="2" fill="#FFFFFF" opacity="0.22"/>',
                f'<rect x="4.8" y="8.8" width="150.4" height="22.4" rx="7" fill="none" stroke="{edge}" '
                f'stroke-opacity="0.6" stroke-width="1.3"/>',
                f'<rect x="4" y="27" width="152" height="5" rx="2.5" fill="#000000" opacity="0.25"/>',
            ])
        emit("Bar", name, w, h, body, defs, border="20,16,20,16", usage=usage, color=color)

    bar("bar_track_9s", None, track=True,
        usage="生命/护盾/灵力条的底槽（放在填充条下方）", color="墨池底槽 + 纸纹")
    bar("bar_fill_hp", ("#D4604F", "#A82F26", "#6E1712", CINNABAR_LIGHT),
        usage="玩家气血填充（Image Type = Filled, Horizontal）", color="朱砂渐变 + 高光")
    bar("bar_fill_enemy_hp", ("#9B3742", "#6B1F28", "#3E0E14", "#8A4A54"),
        usage="敌人气血填充（比玩家更暗，避免与玩家混淆）", color="暗红渐变")
    bar("bar_fill_shield", ("#EAF7F5", "#8FD8D1", "#3F8C86", SPIRIT_LIGHT),
        usage="护盾填充（护盾优先于气血显示）", color="灵青白渐变")
    bar("bar_fill_energy", ("#78C8C0", "#3E8C86", "#1B4744", SPIRIT),
        usage="灵力填充（每点灵力一格，段数与上限一致）", color="灵青渐变")


def main():
    build_panels()
    build_buttons()
    build_frames()
    build_bars()
    with open(os.path.join(HERE, "manifest_base.json"), "w", encoding="utf-8") as f:
        json.dump(ITEMS, f, ensure_ascii=False, indent=2)
    print(f"generated {len(ITEMS)} base svg")


if __name__ == "__main__":
    main()
