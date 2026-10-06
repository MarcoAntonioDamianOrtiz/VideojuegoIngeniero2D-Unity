"""Paint original terrace surfaces and retaining edges into the level's ground image.

The base is the project's own soil artwork. Buildings and interactive props stay
as separate sprites in the scene. Stair gaps use the same geometry as collisions.
"""
import json
import math
import random
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageFilter

HERE = Path(__file__).resolve().parent
REPO = HERE.parents[1]
ROOT = REPO / "Assets/Technopolis/Nivel1"
SOURCE = HERE / "terreno_base_antes_relieve.png"
TARGET = ROOT / "Sprites/01_Terreno/terreno_organico_nivel1.png"
LAYOUT = json.loads((HERE / "terrain_relief_layout.json").read_text())
STREETS = json.loads((HERE / "escena_principal_sprites.json").read_text())["pathSegments"]
image = Image.open(SOURCE).convert("RGBA")
SX, SY = image.width / 64, image.height / 64

# Trace the reference's narrow, gently wandering footpaths. The previous
# ground had broad light soil bands; keep its texture but recede the surplus
# soil into dark, overgrown courtyards. No sprite or collider is baked here.
path_mask = Image.new("L", image.size, 0)
path_draw = ImageDraw.Draw(path_mask)
for segment in STREETS:
    x1, y1 = segment["from"]
    x2, y2 = segment["to"]
    dx, dy = x2-x1, y2-y1
    length = math.hypot(dx, dy)
    for step in range(max(1, round(length * 3)) + 1):
        t = step / max(1, round(length * 3))
        x, y = x1 + t*dx, y1 + t*dy
        drift = .10*math.sin(x*2.7+y*1.3) + .045*math.sin(x*6.1-y*3.2)
        x += -dy/length*drift if length else 0
        y += dx/length*drift if length else 0
        radius = segment["width"] * (.46 + .045*math.sin(step*1.7))
        px, py = x*SX, y*SY
        path_draw.ellipse((px-radius*SX,py-radius*SY,px+radius*SX,py+radius*SY), fill=255)

soft_mask = np.asarray(path_mask.filter(ImageFilter.GaussianBlur(5)), dtype=np.float32)/255.0
pixels = np.asarray(image, dtype=np.float32).copy()
yy, xx = np.indices((image.height, image.width), dtype=np.float32)
c, r = xx/SX, yy/SY
reserved = ((c<13.5)&(r<20.5)) | ((c>28.3)&(c<48.5)&(r>29.7)&(r<41.2)) | \
           ((c>50.5)&(r>30)&(r<42)) | ((c>53)&(r>50)) | ((c>34)&(r>27)&(r<29.2))
warm_soil = (pixels[:,:,0]>138)&(pixels[:,:,0]>pixels[:,:,1]*1.22)& \
            (pixels[:,:,1]>65)&(pixels[:,:,0]>pixels[:,:,2]*1.45)&(~reserved)
outside = (1.0-soft_mask)*warm_soil
pixels[:,:,0] *= 1-.18*outside
pixels[:,:,1] *= 1-.10*outside
pixels[:,:,2] *= 1-.07*outside
park = (c>29)&(c<48)&(r>43)&(r<52)
park_grass = park * (1.0-.75*soft_mask) * .70
pixels[:,:,0] = pixels[:,:,0]*(1-park_grass)+80*park_grass
pixels[:,:,1] = pixels[:,:,1]*(1-park_grass)+172*park_grass
pixels[:,:,2] = pixels[:,:,2]*(1-park_grass)+59*park_grass
inner_walk = soft_mask*park
pixels[:,:,0] = pixels[:,:,0]*(1-.22*inner_walk)+216*(.22*inner_walk)
pixels[:,:,1] = pixels[:,:,1]*(1-.22*inner_walk)+160*(.22*inner_walk)
pixels[:,:,2] = pixels[:,:,2]*(1-.22*inner_walk)+93*(.22*inner_walk)
image = Image.fromarray(np.uint8(np.clip(pixels,0,255)), "RGBA")

overlay = Image.new("RGBA", image.size, (0, 0, 0, 0))
draw = ImageDraw.Draw(overlay, "RGBA")
rng = random.Random(20261005)


def segments(start, end, gaps):
    cursor = start
    for a, b in sorted(gaps):
        a, b = max(start, a), min(end, b)
        if a > cursor:
            yield cursor, a
        cursor = max(cursor, b)
    if cursor < end:
        yield cursor, end


for terrace in LAYOUT["terraces"]:
    x1, y1, x2, y2 = terrace["bounds"]
    left, right = round(x1*SX), round(x2*SX)
    top, bottom = round(y1*SY), round(y2*SY)
    # The upper soil retains the native texture, with a warm, readable level shift.
    tone = {
        "tierra": (235, 173, 91, 35),
        "concreto": (211, 191, 155, 29),
        "piedra": (210, 196, 168, 25),
    }[terrace["material"]]
    if terrace["name"] == "Parque":
        tone = (92, 155, 65, 25)
    draw.rectangle((left + 2, top + 2, right - 2, bottom - 2), fill=tone)
    for _ in range(round((x2-x1)*(y2-y1)*0.20)):
        x = rng.randrange(left+3, right-3)
        y = rng.randrange(top+3, bottom-3)
        color = rng.choice([(119, 85, 51, 80), (242, 209, 139, 82), (70, 87, 42, 80)])
        draw.rectangle((x, y, x+rng.choice((1,2,3)), y+rng.choice((1,2))), fill=color)

    by_side = {side: [] for side in ("north", "south", "west", "east")}
    for stair in terrace["stairs"]:
        at = stair["at"]
        width = stair["width"] + 0.25
        by_side[stair["side"]].append((at-width/2, at+width/2))

    # A thin bright crest, dark earthen face and broken stone foot create height.
    for side, p, a, b in (("north", top, x1, x2), ("south", bottom, x1, x2)):
        for s, e in segments(a, b, by_side[side]):
            xa, xb = round(s*SX), round(e*SX)
            draw.rectangle((xa, p-2, xb, p), fill=(210, 168, 103, 162))
            draw.rectangle((xa, p+1, xb, p+7), fill=(80, 61, 44, 222))
            draw.line((xa, p+8, xb, p+8), fill=(45, 43, 37, 185), width=1)
            for x in range(xa+3, xb-2, 9):
                y = p+2+(x*7 % 4)
                draw.line((x,y,x+2,y+2), fill=(176, 126, 74, 190), width=1)
                if x % 3 == 0:
                    draw.rectangle((x+2,p+9,x+4,p+10), fill=(132,110,71,160))
    for side, p, a, b in (("west", left, y1, y2), ("east", right, y1, y2)):
        for s, e in segments(a, b, by_side[side]):
            ya, yb = round(s*SY), round(e*SY)
            draw.rectangle((p-1, ya, p+1, yb), fill=(210, 165, 100, 155))
            draw.rectangle((p+2, ya, p+7, yb), fill=(83, 62, 45, 215))
            draw.line((p+8, ya, p+8, yb), fill=(39, 41, 37, 200), width=1)
            for y in range(ya+4, yb-2, 10):
                draw.line((p+3,y,p+6,y+2), fill=(170,123,71,170), width=1)

Image.alpha_composite(image, overlay).save(TARGET)
print(f"Painted {len(LAYOUT['terraces'])} terraces into {TARGET}")
