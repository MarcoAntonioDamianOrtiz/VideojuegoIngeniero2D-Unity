"""Check real sprite-scene footprints against the player's walking routes."""

import json
from collections import deque
from pathlib import Path


HERE = Path(__file__).resolve().parent
shapes = json.loads((HERE / "colisiones_generadas_fiel.json").read_text())
config = json.loads((HERE / "escena_principal_sprites.json").read_text())
step = 0.25
side = 257
half_width = 0.41
half_height = 0.52
blocked = bytearray(side * side)


def index(x, y):
    return y * side + x


def nearest(c, r):
    return round(c / step), round(r / step)


def mark_box(bounds):
    left, top, right, bottom = bounds
    for gy in range(max(0, int((top - half_height) / step)), min(side, int((bottom + half_height) / step) + 2)):
        r = gy * step
        if not top - half_height <= r <= bottom + half_height:
            continue
        for gx in range(max(0, int((left - half_width) / step)), min(side, int((right + half_width) / step) + 2)):
            c = gx * step
            if left - half_width <= c <= right + half_width:
                blocked[index(gx, gy)] = 1


for shape in shapes:
    if shape["shape"] == "box":
        mark_box(shape["bounds"])
    else:
        cx, cy = shape["center"]
        radius = shape["radius"]
        for gy in range(max(0, int((cy - radius - half_height) / step)), min(side, int((cy + radius + half_height) / step) + 2)):
            for gx in range(max(0, int((cx - radius - half_width) / step)), min(side, int((cx + radius + half_width) / step) + 2)):
                dx = max(abs(gx * step - cx) - half_width, 0)
                dy = max(abs(gy * step - cy) - half_height, 0)
                if dx * dx + dy * dy <= radius * radius:
                    blocked[index(gx, gy)] = 1

mark_box((0, 0, 0.5, 64))
mark_box((63.5, 0, 64, 64))
mark_box((0, 0, 64, 0.5))
mark_box((0, 63.5, 64, 64))

points = {entry["name"]: nearest(entry["c"], entry["r"]) for entry in config["routeChecks"]}
for name, point in points.items():
    assert not blocked[index(*point)], f"Waypoint blocked: {name} {point}"

start = points["Salida_Alex"]
seen = bytearray(side * side)
seen[index(*start)] = 1
queue = deque([start])
while queue:
    x, y = queue.popleft()
    for nx, ny in ((x - 1, y), (x + 1, y), (x, y - 1), (x, y + 1)):
        if 0 <= nx < side and 0 <= ny < side:
            cell = index(nx, ny)
            if not blocked[cell] and not seen[cell]:
                seen[cell] = 1
                queue.append((nx, ny))

unreachable = [name for name, point in points.items() if not seen[index(*point)]]
assert not unreachable, f"Routes blocked: {', '.join(unreachable)}"
assert not seen[index(*nearest(58.5, 59.5))], "Closed commercial exit can be bypassed"
print(f"OK: {len(shapes)} sprite footprints and {len(points)} reachable waypoints; gate sealed")
