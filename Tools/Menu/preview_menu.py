"""Compare the final Unity Game capture with the user-provided menu reference."""

from pathlib import Path
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[2]
DOC = ROOT / "Documentacion/Menu"
REFERENCE = Path(r"C:/Users/mo27m/Downloads/Gemini_Generated_Image_32hwnd32hwnd32hw.jpeg")
CAPTURE = DOC / "Game_1920x1080.png"

if not REFERENCE.exists() or not CAPTURE.exists():
    raise SystemExit("Missing reference image or Unity capture")

reference = Image.open(REFERENCE).convert("RGB").resize((960, 540), Image.Resampling.LANCZOS)
result = Image.open(CAPTURE).convert("RGB").resize((960, 540), Image.Resampling.LANCZOS)
comparison = Image.new("RGB", (1920, 580), "#0d1520")
comparison.paste(reference, (0, 40))
comparison.paste(result, (960, 40))
draw = ImageDraw.Draw(comparison)
draw.text((18, 11), "REFERENCIA", fill="white")
draw.text((978, 11), "CAPTURA REAL DE UNITY", fill="white")
comparison.save(DOC / "Comparacion_Menu.png")
print("Wrote", DOC / "Comparacion_Menu.png")
