"""Check that every intended stair opening is passable and cliff segments block shortcuts."""
import json
from pathlib import Path

import check_fiel_collisions as grid

layout = json.loads((Path(__file__).parent / "terrain_relief_layout.json").read_text())


def occupied(c, r):
    gx, gy = grid.nearest(c, r)
    return bool(grid.blocked[grid.index(gx, gy)])


def blockers(c, r):
    found = []
    for shape in grid.shapes:
        if shape["shape"] == "box":
            left, top, right, bottom = shape["bounds"]
            hit = left-.41 <= c <= right+.41 and top-.52 <= r <= bottom+.52
        else:
            cx, cy = shape["center"]
            dx = max(abs(c-cx)-.41, 0)
            dy = max(abs(r-cy)-.52, 0)
            hit = dx*dx+dy*dy <= shape["radius"]**2
        if hit:
            found.append(shape["name"])
    return found


issues = []
for terrace in layout["terraces"]:
    x1, y1, x2, y2 = terrace["bounds"]
    for stair in terrace["stairs"]:
        side, at = stair["side"], stair["at"]
        if side in ("north", "south"):
            edge = y1 if side == "north" else y2
            samples = [(at, edge + delta) for delta in (-.8, 0, .8)]
        else:
            edge = x1 if side == "west" else x2
            samples = [(edge + delta, at) for delta in (-.8, 0, .8)]
        blocked = [occupied(*point) for point in samples]
        if any(blocked):
            issues.append(f"Stair {terrace['name']} {side} {at}: blocked {blocked}; " +
                          str([blockers(*point) for point in samples]))
        reachable = [bool(grid.seen[grid.index(*grid.nearest(*point))]) for point in samples]
        if not all(reachable):
            issues.append(f"Stair {terrace['name']} {side} {at}: disconnected {reachable}")

        # An ordinary border point, away from the opening, must stop crossing.
        start, end = (x1, x2) if side in ("north", "south") else (y1, y2)
        candidate = start + 1.0
        if all(abs(candidate - s["at"]) > s["width"]/2 + 1 for s in terrace["stairs"] if s["side"] == side):
            crossing = (candidate, edge) if side in ("north", "south") else (edge, candidate)
            if not occupied(*crossing):
                issues.append(f"Cliff {terrace['name']} {side}: no collision at {crossing}")

if issues:
    raise SystemExit("\n".join(issues))
print(f"OK: {sum(len(t['stairs']) for t in layout['terraces'])} stair openings and terrace borders")
