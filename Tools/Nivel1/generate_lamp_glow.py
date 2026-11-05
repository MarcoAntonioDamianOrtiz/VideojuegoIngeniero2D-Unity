"""Dibuja desde cero el resplandor discreto de las farolas del barrio."""

from pathlib import Path

from PIL import Image, ImageDraw


ROOT = Path(__file__).resolve().parents[2]
OUTPUT = ROOT / "Assets/Technopolis/Nivel1/Sprites/04_Objetos/luz_farola_pixel.png"

image = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
draw = ImageDraw.Draw(image)

# Anillos de píxeles definidos, sin desenfoque ni interpolación.
for radius, alpha in ((31, 6), (27, 9), (23, 14), (19, 19), (15, 27), (11, 34), (7, 42)):
    draw.ellipse((32 - radius, 32 - radius, 32 + radius - 1, 32 + radius - 1), fill=(255, 178, 80, alpha))

OUTPUT.parent.mkdir(parents=True, exist_ok=True)
image.save(OUTPUT)
print(OUTPUT)
