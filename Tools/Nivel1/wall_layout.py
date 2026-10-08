"""Expand continuous property-wall runs into independent, collidable sprites."""

import math

from PIL import Image


SPRITE = "Sprites/04_Objetos/muro_patios_ocre.png"


def pieces(start, end, gaps):
    cursor = start
    for left, right in sorted(gaps):
        left, right = max(start, left), min(end, right)
        if left > cursor + .05:
            yield cursor, left
        cursor = max(cursor, right)
    if cursor < end - .05:
        yield cursor, end


def iter_walls(config, relief, root):
    bounds = Image.open(root / SPRITE).convert("RGBA").getchannel("A").point(
        lambda value: 255 if value > 160 else 0
    ).getbbox()
    terraces = {terrace["name"]: terrace for terrace in relief["terraces"]}
    for run in config.get("wallRuns", []):
        if "terrace" in run:
            terrace = terraces[run["terrace"]]
            x1, y1, x2, y2 = terrace["bounds"]
            side = run["side"]
            horizontal = side in ("north", "south")
            fixed = (y1 if side == "north" else y2) if horizontal else (x1 if side == "west" else x2)
            start, end = (x1, x2) if horizontal else (y1, y2)
            gaps = [
                (stair["at"] - stair["width"]/2 - .25,
                 stair["at"] + stair["width"]/2 + .25)
                for stair in terrace["stairs"] if stair["side"] == side
            ]
        else:
            horizontal = run["axis"] == "horizontal"
            fixed, start, end = run["fixed"], run["start"], run["end"]
            gaps = run.get("gaps", [])

        sequence = 0
        for left, right in pieces(start, end, gaps):
            count = math.ceil((right-left) / 3.05)
            for index in range(count):
                a = left + (right-left)*index/count
                b = left + (right-left)*(index+1)/count
                sequence += 1
                name = f"Muro_Parcela_{run['name']}_{sequence:02}"
                yield dict(
                    name=name, sprite=SPRITE,
                    c=(a+b)/2 if horizontal else fixed,
                    r=fixed if horizontal else b,
                    width=b-a+.06, height=run.get("height", 1.04),
                    angle=0 if horizontal else 90,
                    kind="prop", solid=True,
                    left=bounds[0], top=bounds[1], right=bounds[2], bottom=bounds[3],
                )
