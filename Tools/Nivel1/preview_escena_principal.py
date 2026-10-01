"""Render an inspectable preview from the same tiles and sprites as the main scene."""

import json
from pathlib import Path
from PIL import Image


REPO = Path(__file__).resolve().parents[2]
ROOT = REPO / "Assets/Technopolis/Nivel1"
data = json.loads((ROOT / "Editor/PlanoReferenciaNivel1.json").read_text())
config = json.loads((REPO / "Tools/Nivel1/escena_principal_sprites.json").read_text())
placements = [dict(item) for item in data["placements"]]
for item in placements:
    replacement = config["spriteOverrides"].get(item["name"])
    if replacement:
        item["sprite"] = replacement
        item["left"], item["top"], item["right"], item["bottom"] = Image.open(ROOT / replacement).getchannel("A").getbbox()
placements.extend(config["extraPlacements"])

for i in range(8):
    for name, c, r, angle in (("Norte", 4+i*8, .95, 0), ("Sur", 4+i*8, 64, 0),
                              ("Oeste", .475, 8+i*8, 90), ("Este", 63.525, 8+i*8, 90)):
        placements.append(dict(name=f"Muro_{name}_{i}", sprite="Sprites/04_Objetos/muro_perimetral.png",
                               c=c, r=r, width=8, height=.95, angle=angle, kind="prop",
                               left=0, top=11, right=128, bottom=37))


def lane(c, r):
    return any(min(item["c1"], item["c2"])-item["width"]/2 <= c <= max(item["c1"], item["c2"])+item["width"]/2 and
               min(item["r1"], item["r2"])-item["width"]/2 <= r <= max(item["r1"], item["r2"])+item["width"]/2
               for item in data["lanes"])


def garden(c, r):
    for item in placements:
        if item["kind"] != "building":
            continue
        width = item["width"]
        height = item["height"] or width*(item["bottom"]-item["top"])/(item["right"]-item["left"])
        left, right, top, bottom = item["c"]-width/2, item["c"]+width/2, item["r"]-height, item["r"]
        near = left-2 < c < right+2 and top-.7 < r < bottom+2.1
        inside = left+.65 < c < right-.65 and top+.65 < r < bottom-.25
        if near and not inside:
            return True
    return c < 1.7 or c > 62.3 or r < 1.5 or r > 62.5


scale = 16
canvas = Image.new("RGBA", (64*scale, 64*scale))
for row in range(64):
    for col in range(64):
        material = "tierra"
        for region in data["regions"]:
            if region["c"] <= col < region["c"]+region["w"] and region["r"] <= row < region["r"]+region["h"]:
                material = region["material"]
        if material == "tierra" and garden(col+.5, row+.5) and not lane(col+.5, row+.5) and (col*73+row*29)%10 < 7:
            material = "pasto"
        folder = "02_Caminos" if material in ("concreto", "empedrado") else "01_Terreno"
        tile = Image.open(ROOT / f"Sprites/{folder}/{material}_bloque_f{row%4}_c{col%4}.png").convert("RGBA")
        canvas.alpha_composite(tile.resize((scale, scale), Image.Resampling.NEAREST), (col*scale, row*scale))

for item in sorted(placements, key=lambda value: -100 if value["kind"] == "floor" else value["r"]):
    art = Image.open(ROOT / item["sprite"]).convert("RGBA")
    art = art.crop((item["left"], item["top"], item["right"], item["bottom"]))
    width = item["width"]
    height = item["height"] or width*art.height/art.width
    art = art.resize((max(1, round(width*scale)), max(1, round(height*scale))), Image.Resampling.NEAREST)
    if item["angle"]:
        art = art.rotate(-item["angle"], expand=True)
    x = round(item["c"]*scale-art.width/2)
    y = round(item["r"]*scale-art.height)
    canvas.alpha_composite(art, (x, y))

output = REPO / "Documentacion/Nivel1/Vista_Previa_EscenaPrincipal.png"
canvas.save(output)
print(f"Rendered {output} from {len(placements)} sprites")
