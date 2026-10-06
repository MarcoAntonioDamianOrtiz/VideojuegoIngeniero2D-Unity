"""Shared path geometry for the baked scene and its visual preview."""

import math


def on_path(c, r, segments):
    for segment in segments:
        x1, y1 = segment["from"]
        x2, y2 = segment["to"]
        dx, dy = x2 - x1, y2 - y1
        t = max(0.0, min(1.0, ((c - x1) * dx + (r - y1) * dy) / (dx * dx + dy * dy)))
        distance = math.hypot(c - (x1 + t * dx), r - (y1 + t * dy))
        taper = 0.85 + 0.15 * r / 64.0
        rough_edge = 0.10 * math.sin(c * 2.9 + r * 1.7) + 0.06 * math.sin(c * 5.1 - r * 2.3)
        if distance <= segment["width"] * taper / 2 + rough_edge:
            return True
    return False
