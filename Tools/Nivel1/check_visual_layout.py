"""Report significant building overlaps in the assembled sprite scene."""

import json
from pathlib import Path

from PIL import Image
from reference_layout import keep_placement, reference_house


repo = Path(__file__).resolve().parents[2]
root = repo / "Assets/Technopolis/Nivel1"
plan = json.loads((root / "Editor/PlanoReferenciaNivel1.json").read_text())
config = json.loads((repo / "Tools/Nivel1/escena_principal_sprites.json").read_text())
buildings = []

for source in plan["placements"] + config["extraPlacements"]:
    if source["kind"] != "building":
        continue
    if not keep_placement(source):
        continue
    item = dict(source)
    if item["name"].startswith("Vivienda_"):
        item["width"] *= config["houseWidthScale"] if source in plan["placements"] else config.get("extraHouseWidthScale", 1)
    item.update(config.get("placementOverrides", {}).get(item["name"], {}))
    reference_house(item)
    if item["name"] in config["spriteOverrides"] and not item["name"].startswith("Vivienda_"):
        item["sprite"] = config["spriteOverrides"][item["name"]]
    if "left" not in item or item["sprite"] != source["sprite"]:
        item["left"], item["top"], item["right"], item["bottom"] = Image.open(root / item["sprite"]).getchannel("A").point(lambda v: 255 if v > 160 else 0).getbbox()
    width = item["width"]
    height = item["height"] or width * (item["bottom"] - item["top"]) / (item["right"] - item["left"])
    buildings.append((item["name"], (item["c"]-width/2, item["r"]-height, item["c"]+width/2, item["r"])))

overlaps = []
for i, (name_a, a) in enumerate(buildings):
    for name_b, b in buildings[i+1:]:
        iw = max(0, min(a[2], b[2])-max(a[0], b[0]))
        ih = max(0, min(a[3], b[3])-max(a[1], b[1]))
        if iw and ih:
            area = iw * ih
            smaller = min((a[2]-a[0])*(a[3]-a[1]), (b[2]-b[0])*(b[3]-b[1]))
            if area / smaller > .22:
                overlaps.append((round(area / smaller, 2), name_a, name_b))

for fraction, a, b in sorted(overlaps, reverse=True):
    print(f"{fraction:.0%} {a} / {b}")
print(f"{len(buildings)} buildings; {len(overlaps)} major overlaps")
if overlaps:
    raise SystemExit(1)
