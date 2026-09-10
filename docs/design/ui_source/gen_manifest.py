# -*- coding: utf-8 -*-
"""合并素材清单，生成 manifest_all.json 与 ui_manifest.md。

运行：python docs/design/ui_source/gen_manifest.py
"""

import json
import os

HERE = os.path.dirname(os.path.abspath(__file__))

CATEGORY_TITLE = {
    "Common": "通用面板与装饰",
    "Button": "按钮",
    "Frame": "边框与卡牌",
    "Bar": "进度条",
    "Icon": "图标",
    "Hexagram": "八卦与爻线",
    "Background": "页面背景",
}


def load(name):
    path = os.path.join(HERE, name)
    if not os.path.exists(path):
        raise SystemExit(f"missing {name}, run gen_base.py / gen_symbols.py first")
    with open(path, encoding="utf-8") as f:
        return json.load(f)


def main():
    items = load("manifest_base.json") + load("manifest_symbols.json")
    with open(os.path.join(HERE, "manifest_all.json"), "w", encoding="utf-8") as f:
        json.dump(items, f, ensure_ascii=False, indent=2)

    lines = [
        "# UI 素材全量清单",
        "",
        "> 由 `docs/design/ui_source/gen_manifest.py` 自动生成，勿手工编辑。",
        f"> 共 {len(items)} 个 PNG，输出目录 `Assets/Resources/Texture/UI/`。",
        "",
        "九宫格边框按 Unity Sprite Editor 的 **L,B,R,T**（左,下,右,上）顺序填写；`none` 表示不切九宫格。",
        "",
    ]
    for category, title in CATEGORY_TITLE.items():
        group = [i for i in items if i["category"] == category]
        if not group:
            continue
        lines += [f"## {title}（{len(group)}）", "", "| 文件 | 尺寸 | 九宫格 | 用途 | 配色 |",
                  "|---|---|---|---|---|"]
        for i in group:
            name = i["name"] + ".png"
            border = i["border"] if i["border"] else "none"
            lines.append(
                f"| `{name}` | {i['width']}×{i['height']} | {border} | {i['usage']} | {i['color']} |"
            )
        lines.append("")

    with open(os.path.join(HERE, "ui_manifest.md"), "w", encoding="utf-8") as f:
        f.write("\n".join(lines))
    print(f"merged {len(items)} items")


if __name__ == "__main__":
    main()
