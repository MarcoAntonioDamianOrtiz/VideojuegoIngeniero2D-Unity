"""Check that the player can traverse the main districts of the painted level."""

import json
from collections import deque
from pathlib import Path


layout = json.loads((Path(__file__).with_name("colisiones_mapa_fiel.json")).read_text())
size = layout["imageSize"]
step = 4
grid = size // step + 1
player_half_width = 9
player_half_height = 11
blocked = bytearray(grid * grid)


def index(x, y):
    return y * grid + x


def nearest(point):
    return (round(point[0] / step), round(point[1] / step))


def mark_rect(x1, y1, x2, y2):
    for gy in range(max(0, int((y1 - player_half_height) // step)), min(grid, int((y2 + player_half_height) // step) + 1)):
        py = gy * step
        if py < y1 - player_half_height or py > y2 + player_half_height:
            continue
        for gx in range(max(0, int((x1 - player_half_width) // step)), min(grid, int((x2 + player_half_width) // step) + 1)):
            px = gx * step
            if x1 - player_half_width <= px <= x2 + player_half_width:
                blocked[index(gx, gy)] = 1


names = set()
for shape in layout["rects"]:
    name = shape["name"]
    x1, y1, x2, y2 = shape["bounds"]
    assert name not in names and 0 <= x1 < x2 <= size and 0 <= y1 < y2 <= size, name
    names.add(name)
    mark_rect(x1, y1, x2, y2)

for shape in layout["circles"]:
    name = shape["name"]
    x, y = shape["center"]
    radius = shape["radius"]
    assert name not in names and 0 < radius < size / 4, name
    names.add(name)
    for gy in range(max(0, int((y - radius - player_half_height) // step)), min(grid, int((y + radius + player_half_height) // step) + 1)):
        for gx in range(max(0, int((x - radius - player_half_width) // step)), min(grid, int((x + radius + player_half_width) // step) + 1)):
            dx = max(abs(gx * step - x) - player_half_width, 0)
            dy = max(abs(gy * step - y) - player_half_height, 0)
            if dx * dx + dy * dy <= radius * radius:
                blocked[index(gx, gy)] = 1

mark_rect(0, 0, 8, size)
mark_rect(size - 8, 0, size, size)
mark_rect(0, 0, size, 8)
mark_rect(0, size - 8, size, size)

waypoints = {item["name"]: nearest(item["point"]) for item in layout["waypoints"]}
for name, point in waypoints.items():
    assert not blocked[index(*point)], f"Waypoint blocked: {name} at {point}"

start = waypoints["Casa_Alex"]
seen = bytearray(grid * grid)
seen[index(*start)] = 1
queue = deque([start])
while queue:
    x, y = queue.popleft()
    for nx, ny in ((x - 1, y), (x + 1, y), (x, y - 1), (x, y + 1)):
        if 0 <= nx < grid and 0 <= ny < grid:
            cell = index(nx, ny)
            if not blocked[cell] and not seen[cell]:
                seen[cell] = 1
                queue.append((nx, ny))

unreachable = [name for name, point in waypoints.items() if not seen[index(*point)]]
assert not unreachable, f"Main routes blocked: {', '.join(unreachable)}"
assert not seen[index(*nearest((1100, 1170)))], "The closed district gate can be bypassed"
print(f"OK: {len(names)} obstacle shapes, {len(waypoints)} reachable landmarks, closed gate sealed")
