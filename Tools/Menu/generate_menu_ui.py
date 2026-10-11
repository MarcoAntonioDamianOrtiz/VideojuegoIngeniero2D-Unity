"""Draw the original pixel-art ornaments used by the additive menu scene.

These are UI-only sprites: the level, characters, and menu text stay separate.
Run with Pillow installed, then rebuild the menu scene in Unity.
"""

from pathlib import Path
import random

from PIL import Image, ImageDraw


ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / "Assets/Sprites/Menu/UI"
OUT.mkdir(parents=True, exist_ok=True)


def save_pixel(image: Image.Image, name: str, scale: int = 2) -> None:
    image.resize((image.width * scale, image.height * scale), Image.Resampling.NEAREST).save(OUT / name)


def box(draw, rect, fill, outline=None, width=1):
    draw.rectangle(rect, fill=fill, outline=outline, width=width)


# Industrial screen. The display face is intentionally quiet so the five
# independent TextMeshPro options can be read and moved without repainting it.
panel = Image.new("RGBA", (270, 360), (0, 0, 0, 0))
d = ImageDraw.Draw(panel)
rng = random.Random(210609)
box(d, (12, 13, 268, 358), "#050914")
box(d, (7, 8, 264, 352), "#151725", "#050911", 3)
box(d, (12, 13, 257, 344), "#756f78", "#b4a0a0", 2)
box(d, (17, 18, 253, 340), "#393a49", "#1c1c2b", 3)
box(d, (23, 26, 247, 337), "#151d2b", "#0a0e1a", 3)

# Raised industrial corners and inset braces.
for x in (8, 240):
    for y in (12, 320):
        box(d, (x, y, x + 22, y + 24), "#85808c", "#0a0d19", 2)
        box(d, (x + 3, y + 3, x + 19, y + 20), "#555360", "#c0aab0")
        d.ellipse((x + 9, y + 9, x + 15, y + 15), fill="#161a26", outline="#c9b7b4")
        d.line((x + 10, y + 13, x + 15, y + 10), fill="#d5c1b4", width=1)

# Header electronics, grilles, cable sockets, red and green pilot lights.
box(d, (30, 12, 237, 56), "#666775", "#10121e", 3)
box(d, (36, 18, 230, 49), "#343b49", "#9d9398", 2)
for x in range(41, 117, 7):
    box(d, (x, 24, x + 3, 43), "#111925", "#8c8790")
box(d, (123, 24, 180, 41), "#1a2532", "#101520")
for x in range(128, 177, 8):
    box(d, (x, 31, x + 3, 35), "#2d5662")
for x, color in ((189, "#8ce55d"), (203, "#58d9b2"), (217, "#ef6e58")):
    box(d, (x - 2, 28, x + 4, 36), "#161d29", "#050b15")
    box(d, (x, 29, x + 2, 33), color)
box(d, (26, 54, 245, 65), "#8a818a", "#181a27", 2)
d.line((31, 57, 237, 57), fill="#c9a9a1", width=1)

# Recessed screen with subtle vertical and horizontal electrical wear.
box(d, (30, 66, 241, 314), "#040914", "#a9a0a6", 3)
box(d, (34, 70, 237, 310), "#111b2a", "#303d4f", 2)
for y in range(75, 307, 16):
    d.line((36, y, 235, y), fill=(53, 77, 94, 17), width=1)
for x in (35, 236):
    d.line((x, 74, x, 306), fill="#476171", width=1)
for y in (111, 160, 209, 258):
    d.line((44, y, 228, y), fill="#284c60", width=1)
    d.line((50, y + 1, 221, y + 1), fill="#132b3b", width=1)

# Worn metal strips, side contacts and bolt heads around the frame.
for x in (18, 250):
    for y in range(70, 308, 23):
        box(d, (x - 2, y, x + 2, y + 7), "#504d5b", "#1a1c29")
        d.point((x, y + 3), fill="#a8a0a5")
for _ in range(125):
    x = rng.randrange(15, 256)
    y = rng.randrange(16, 346)
    if 31 <= x <= 239 and 67 <= y <= 314:
        continue
    d.point((x, y), fill=rng.choice(("#111521", "#b7a39d", "#484a58", "#272938")))

# Lower status port and side cables stay part of the frame sprite.
box(d, (29, 315, 243, 337), "#696773", "#10121e", 3)
box(d, (38, 321, 132, 329), "#151d2a", "#a49ca3")
for x in range(43, 129, 8):
    d.line((x, 323, x + 3, 323), fill="#2a6770", width=1)
for x, color in ((194, "#4d8e88"), (205, "#a8de6f"), (216, "#d2825d")):
    box(d, (x, 322, x + 4, 327), color, "#121a27")
d.line((3, 14, 3, 79, 11, 91, 11, 112), fill="#141723", width=4)
d.line((265, 12, 265, 54, 256, 62), fill="#141723", width=5)
d.line((261, 342, 261, 359), fill="#176b5c", width=3)
d.line((266, 340, 266, 359), fill="#072620", width=3)
save_pixel(panel, "panel_industrial.png")


# The active row is its own translucent highlighted display plate.
active = Image.new("RGBA", (220, 35), (0, 0, 0, 0))
d = ImageDraw.Draw(active)
box(d, (0, 2, 219, 34), (21, 60, 80, 228), (79, 132, 145, 225), 2)
d.line((8, 5, 211, 5), fill=(112, 195, 210, 155), width=1)
d.line((8, 31, 211, 31), fill=(10, 21, 35, 240), width=1)
for x in (6, 213):
    box(d, (x, 11, x + 2, 25), "#447f89")
save_pixel(active, "seleccion_menu.png")


arrow = Image.new("RGBA", (18, 22), (0, 0, 0, 0))
d = ImageDraw.Draw(arrow)
d.polygon(((2, 1), (16, 11), (2, 21)), fill="#06101a")
d.polygon(((4, 3), (14, 11), (4, 19)), fill="#e4faff")
d.line((5, 4, 5, 18), fill="#8bd4e3", width=1)
save_pixel(arrow, "flecha_menu.png")


footer = Image.new("RGBA", (340, 44), (0, 0, 0, 0))
d = ImageDraw.Draw(footer)
box(d, (0, 3, 339, 43), "#06101b", "#11101b", 2)
box(d, (1, 6, 338, 41), "#101b29", "#897982", 1)
d.line((3, 4, 338, 4), fill="#baa4a1", width=2)
d.line((3, 8, 338, 8), fill="#355267", width=1)
for x in range(7, 45, 7):
    d.polygon(((x, 36), (x + 4, 36), (x + 9, 41), (x + 5, 41)), fill="#5b4a38")
for x in (324, 332):
    box(d, (x, 13, x + 2, 18), "#55cf9b")
save_pixel(footer, "barra_inferior_menu.png")


button_a = Image.new("RGBA", (25, 25), (0, 0, 0, 0))
d = ImageDraw.Draw(button_a)
d.ellipse((1, 1, 23, 23), fill="#070f1b", outline="#9acb82", width=2)
d.ellipse((4, 4, 20, 20), outline="#456a5c", width=1)
save_pixel(button_a, "icono_a_menu.png")

print("Generated:", ", ".join(p.name for p in sorted(OUT.glob("*.png"))))
