"""Draw two original, compact pixel-art stairs for the terrain terraces."""

import random
from pathlib import Path

from PIL import Image, ImageDraw


ROOT = Path(__file__).resolve().parents[2] / "Assets/Technopolis/Nivel1/Sprites/04_Objetos"
PALETTES = {
    "concreto": {
        "outline": (49, 48, 43), "side": (96, 89, 76),
        "side_light": (149, 136, 110), "top": (153, 145, 125),
        "front": (92, 88, 78), "light": (194, 179, 150),
        "chip": (119, 109, 91), "moss": (78, 100, 55),
    },
    "tierra": {
        "outline": (62, 48, 36), "side": (107, 75, 47),
        "side_light": (169, 124, 74), "top": (159, 116, 69),
        "front": (104, 71, 45), "light": (205, 155, 91),
        "chip": (127, 92, 55), "moss": (89, 111, 48),
    },
}


def draw_stair(style):
    colors = PALETTES[style]
    image = Image.new("RGBA", (80, 48), (0, 0, 0, 0))
    pen = ImageDraw.Draw(image)
    rng = random.Random(20261007 + (0 if style == "concreto" else 1))

    # The side cheeks and four rising treads form a single traversable ramp.
    pen.polygon([(9, 3), (70, 3), (79, 43), (74, 47), (5, 47), (0, 43)],
                fill=colors["outline"])
    pen.polygon([(8, 5), (14, 7), (6, 42), (2, 42)], fill=colors["side"])
    pen.polygon([(71, 5), (76, 41), (73, 43), (65, 7)], fill=colors["side"])
    pen.line([(8, 5), (3, 41)], fill=colors["side_light"], width=2)
    pen.line([(70, 5), (76, 41)], fill=colors["light"], width=1)

    for step in range(5):
        y0 = 5 + step * 8
        y1 = min(45, y0 + 7)
        left = 13 - step * 2
        right = 67 + step * 2
        pen.polygon([(left, y0), (right, y0), (right + 2, y1-2),
                     (left - 2, y1-2)], fill=colors["top"])
        pen.line([(left+2, y0+1), (right-1, y0+1)], fill=colors["light"], width=2)
        pen.polygon([(left-2, y1-2), (right+2, y1-2),
                     (right+2, y1+1), (left-2, y1+1)], fill=colors["front"])
        pen.line([(left-2, y1), (right+2, y1)], fill=colors["outline"], width=1)
        for _ in range(9):
            x = rng.randrange(left+2, right-1)
            y = rng.randrange(y0+3, max(y0+4, y1-2))
            pen.rectangle((x, y, x+rng.choice((1, 2)), y+1), fill=colors["chip"])

    # Small worn corners tie the steps to the soil and retaining wall.
    for x, y in ((5, 37), (7, 21), (72, 13), (75, 37), (13, 43), (66, 44)):
        pen.rectangle((x, y, x+2, y+2), fill=colors["moss"])
    return image


for name in PALETTES:
    output = ROOT / f"escalera_barrio_{name}_nueva.png"
    draw_stair(name).save(output)
    print(f"Drew {output}")
