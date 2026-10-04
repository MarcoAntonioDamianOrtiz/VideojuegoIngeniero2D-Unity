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
    if item["name"].startswith("Vivienda_"):
        item["width"] = round(item["width"] * config["houseWidthScale"], 3)
    item.update(config.get("placementOverrides", {}).get(item["name"], {}))
    replacement = config["spriteOverrides"].get(item["name"])
    if replacement:
        item["sprite"] = replacement
        item["left"], item["top"], item["right"], item["bottom"] = Image.open(ROOT / replacement).getchannel("A").point(lambda v: 255 if v > 160 else 0).getbbox()
for item in config["extraPlacements"]:
    item = dict(item)
    if "left" not in item:
        item["left"], item["top"], item["right"], item["bottom"] = Image.open(ROOT / item["sprite"]).getchannel("A").point(lambda v: 255 if v > 160 else 0).getbbox()
    placements.append(item)
for group in config.get("repeatSprites", []):
    left, top, right, bottom = Image.open(ROOT / group["sprite"]).convert("RGBA").getchannel("A").point(lambda v: 255 if v > 160 else 0).getbbox()
    for index, (c, r) in enumerate(group["points"], 1):
        placements.append(dict(name=f"{group['name']}_{index:02}", sprite=group["sprite"], c=c, r=r,
                               width=group["width"], height=group.get("height", 0), angle=group.get("angle", 0),
                               kind=group.get("kind", "prop"), solid=group.get("solid", False),
                               left=left, top=top, right=right, bottom=bottom))

for i in range(8):
    for name, c, r, angle in (("Norte", 4+i*8, .95, 0), ("Sur", 4+i*8, 64, 0),
                              ("Oeste", .475, 8+i*8, 90), ("Este", 63.525, 8+i*8, 90)):
        placements.append(dict(name=f"Muro_{name}_{i}", sprite="Sprites/04_Objetos/muro_perimetral.png",
                               c=c, r=r, width=8, height=.95, angle=angle, kind="prop",
                               left=0, top=11, right=128, bottom=37))


scale_x = 16
scale_y = round(scale_x * config["verticalScale"])
canvas = Image.open(ROOT / config["terrainSprite"]).convert("RGBA").resize(
    (64*scale_x, 64*scale_y), Image.Resampling.NEAREST)

for item in sorted(placements, key=lambda value: value.get("sortingOrder", -80 if value["kind"] == "floor" else round(value["r"]*10))):
    art = Image.open(ROOT / item["sprite"]).convert("RGBA")
    art = art.crop((item["left"], item["top"], item["right"], item["bottom"]))
    width = item["width"]
    height = item["height"] or width*art.height/art.width
    art = art.resize((max(1, round(width*scale_x)), max(1, round(height*scale_y))), Image.Resampling.NEAREST)
    if item["angle"]:
        art = art.rotate(-item["angle"], expand=True)
    x = round(item["c"]*scale_x-art.width/2)
    y = round(item["r"]*scale_y-art.height)
    canvas.alpha_composite(art, (x, y))

output = REPO / "Documentacion/Nivel1/Vista_Previa_EscenaPrincipal.png"
canvas.save(output)
print(f"Rendered {output} from {len(placements)} sprites")
