# -*- coding: utf-8 -*-
"""生成素材总览 HTML，供人工快速检查全部 PNG。

运行：python docs/design/ui_source/gen_preview.py
输出：docs/design/ui_source/preview.html（再用 headless Chrome 截图为 ui_preview_sheet.png）

渲染命令（宽高由脚本末尾打印）：
  chrome --headless=new --screenshot=ui_preview_sheet.png --window-size=W,H \
         --default-background-color=00000000 --allow-file-access-from-files file:///.../preview.html
"""

import json
import os

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, "..", "..", ".."))
PNG_ROOT = os.path.join(ROOT, "Assets", "Resources", "Texture", "UI")

COLS = 8
CELL_W = 210
CELL_H = 230
MARGIN = 30
HEAD_H = 130
GROUP_HEAD_H = 52

CATEGORY_TITLE = {
    "Common": "Common 通用面板与装饰",
    "Button": "Button 按钮",
    "Frame": "Frame 边框与卡牌",
    "Bar": "Bar 进度条",
    "Icon": "Icon 图标",
    "Hexagram": "Hexagram 八卦与爻线",
    "Background": "Background 页面背景",
}


def main():
    with open(os.path.join(HERE, "manifest_all.json"), encoding="utf-8") as f:
        items = json.load(f)

    rows_total = 0
    groups_html = []
    for category, title in CATEGORY_TITLE.items():
        group = [i for i in items if i["category"] == category]
        if not group:
            continue
        rows = (len(group) + COLS - 1) // COLS
        rows_total += rows
        cells = []
        for i in group:
            rel = i["file"].replace("Assets/Resources/Texture/UI/", "")
            url = "file:///" + os.path.join(PNG_ROOT, rel.replace("/", os.sep)).replace("\\", "/")
            cells.append(
                f'<div class="cell"><div class="box"><img src="{url}" alt="{i["name"]}"/></div>'
                f'<div class="name">{i["name"]}</div>'
                f'<div class="meta">{i["width"]}×{i["height"]}'
                f'{" · 9s" + i["border"] if i["border"] else ""}</div></div>'
            )
        groups_html.append(
            f'<div class="group"><h2>{title}（{len(group)}）</h2>'
            f'<div class="row">{"".join(cells)}</div></div>'
        )

    height = HEAD_H + rows_total * CELL_H + len(groups_html) * GROUP_HEAD_H + 60
    width = COLS * CELL_W + MARGIN * 2

    html = f"""<!DOCTYPE html>
<html lang="zh-CN"><head><meta charset="utf-8">
<style>
  html,body {{ margin:0; padding:0; width:{width}px; background:#0a0b09; }}
  body {{ font-family:"Microsoft YaHei","Segoe UI",sans-serif; color:#C8BFA8; }}
  .head {{ padding:24px {MARGIN}px 8px; }}
  .head h1 {{ margin:0; font-size:26px; letter-spacing:2px; color:#C2AA72; }}
  .head p {{ margin:6px 0 0; font-size:13px; color:#8d8874; }}
  .group {{ padding:0 {MARGIN}px; }}
  .group h2 {{ font-size:15px; font-weight:400; color:#5EC8C0; margin:14px 0 10px;
               border-bottom:1px solid rgba(194,170,114,.25); padding-bottom:6px; }}
  .row {{ display:flex; flex-wrap:wrap; gap:12px; }}
  .cell {{ width:{CELL_W - 12}px; }}
  .box {{ height:{CELL_H - 76}px; background:
          linear-gradient(45deg,#14150f 25%,transparent 25%,transparent 75%,#14150f 75%),
          linear-gradient(45deg,#14150f 25%,#0d0e0b 25%,#0d0e0b 75%,#14150f 75%);
          background-size:16px 16px; background-position:0 0,8px 8px;
          border:1px solid rgba(194,170,114,.18); border-radius:6px;
          display:flex; align-items:center; justify-content:center; overflow:hidden; }}
  .box img {{ max-width:94%; max-height:94%; object-fit:contain; }}
  .name {{ font-size:11px; color:#C8BFA8; margin-top:5px; word-break:break-all; }}
  .meta {{ font-size:10px; color:#6f6b5c; margin-top:1px; }}
</style></head><body>
<div class="head">
  <h1>《卦锻》UI 素材总览 · 墨狱异志录</h1>
  <p>共 {len(items)} 个 PNG ｜ 输出目录 Assets/Resources/Texture/UI/ ｜ 格子背景为棋盘格，用于检查透明边缘</p>
</div>
{"".join(groups_html)}
<script>
window.addEventListener('load', function () {{
  var imgs = document.images, bad = 0;
  for (var i = 0; i < imgs.length; i++) {{
    if (!imgs[i].complete || imgs[i].naturalWidth === 0) {{ bad++; }}
  }}
  document.title = 'BAD=' + bad + '/' + imgs.length;
}});
</script>
</body></html>
"""

    out = os.path.join(HERE, "preview.html")
    with open(out, "w", encoding="utf-8") as f:
        f.write(html)
    print(f"preview.html written, window-size={width},{height}")


if __name__ == "__main__":
    main()
