from pathlib import Path
import uuid

from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'Assets/Technopolis/Nivel1/Sprites/04_Objetos'
FONT_PATH = Path('C:/Windows/Fonts/consola.ttf')
INK = '#292a27'
STONE = '#77746a'
STONE_LIGHT = '#aaa18a'
BLUE = '#4d9cb0'
GOLD = '#f2b64d'
ORANGE = '#ed7333'


def pixel_font(size):
    return ImageFont.truetype(str(FONT_PATH), size) if FONT_PATH.exists() else ImageFont.load_default()


def meta_for(name, guid):
    template = (OUT / 'poste_luz.png.meta').read_text()
    old = 'poste_luz'
    template = template.replace('4af856838c1f02f48a98a59c59ed24e7', guid)
    template = template.replace(old, name)
    template = template.replace('7717c570b98be2170800000000000000', uuid.uuid5(uuid.NAMESPACE_URL, f'technopolis/sprite/{name}').hex)
    return '\n'.join(line.rstrip() for line in template.splitlines()) + '\n'


def save(name, image):
    path = OUT / f'{name}.png'
    image.save(path)
    meta = OUT / f'{name}.png.meta'
    guid = uuid.uuid5(uuid.NAMESPACE_URL, f'technopolis/{name}').hex
    meta.write_text(meta_for(name, guid), newline='\n')


def brick_mural(lines, blue_text=True):
    im = Image.new('RGBA', (192, 64))
    d = ImageDraw.Draw(im)
    paint = BLUE if blue_text else '#b85447'
    font = pixel_font(16)
    y = 6
    for line in lines:
        box = d.textbbox((0, 0), line, font=font, stroke_width=1)
        x = max(18, (192 - (box[2] - box[0])) // 2)
        d.text((x + 1, y + 2), line, font=font, fill='#343831', stroke_width=2, stroke_fill='#343831')
        d.text((x, y), line, font=font, fill=paint, stroke_width=1, stroke_fill='#356d75')
        y += 25
    for x, y in [(14, 13), (178, 8), (23, 55), (168, 49), (8, 35), (184, 31)]:
        d.rectangle((x, y, x + 2, y + 2), fill=paint)
    return im


def cable_segment():
    im = Image.new('RGBA', (256, 96))
    d = ImageDraw.Draw(im)
    for offset, color, width in [(0, '#302f2d', 2), (11, '#5a5347', 1)]:
        points = [(4, 15 + offset), (44, 28 + offset), (88, 40 + offset), (132, 44 + offset),
                  (176, 39 + offset), (216, 26 + offset), (252, 14 + offset)]
        d.line(points, fill=color, width=width, joint='curve')
    for x in (38, 104, 174, 226):
        d.ellipse((x, 44, x + 4, 48), fill='#9a7844')
    return im


def lamp_with_glow():
    im = Image.new('RGBA', (128, 176))
    d = ImageDraw.Draw(im)
    for radius, alpha in [(36, 20), (27, 30), (18, 42), (10, 64)]:
        d.ellipse((64-radius, 31-radius, 64+radius, 31+radius), fill=(255, 178, 62, alpha))
    d.rectangle((59, 62, 68, 164), fill='#4e4437', outline=INK, width=3)
    d.rectangle((55, 156, 72, 166), fill='#75634b', outline=INK, width=3)
    d.line((63, 64, 63, 19, 78, 10, 99, 10), fill=INK, width=8)
    d.line((63, 64, 63, 19, 78, 10, 99, 10), fill='#726047', width=4)
    d.rectangle((94, 8, 113, 19), fill='#524538', outline=INK, width=3)
    d.rectangle((98, 17, 109, 28), fill=GOLD, outline=INK, width=2)
    d.rectangle((101, 20, 106, 26), fill='#fff0a4')
    return im


def road_barrier():
    im = Image.new('RGBA', (192, 128))
    d = ImageDraw.Draw(im)
    d.rectangle((18, 67, 174, 90), fill='#5b4938', outline=INK, width=4)
    d.rectangle((22, 70, 170, 84), fill='#d8c7a3', outline='#332f2a', width=2)
    for x in (26, 68, 110, 152):
        d.polygon([(x, 86), (x+8, 86), (x+4, 110)], fill='#383733', outline=INK)
    for x in (40, 136):
        d.polygon([(x, 29), (x+20, 29), (x+27, 68), (x-7, 68)], fill=ORANGE, outline=INK)
        d.rectangle((x-4, 65, x+24, 71), fill='#312f2b', outline=INK, width=2)
        d.polygon([(x+2, 44), (x+17, 44), (x+19, 51), (x, 51)], fill='#f2ddba')
        d.polygon([(x-2, 56), (x+21, 56), (x+23, 63), (x-4, 63)], fill='#f2ddba')
    for x in (25, 163):
        d.ellipse((x-5, 82, x+5, 92), fill='#f04e3a', outline=INK, width=2)
    return im


def poster_board():
    im = Image.new('RGBA', (128, 112))
    d = ImageDraw.Draw(im)
    d.rectangle((26, 13, 102, 75), fill='#6f5339', outline=INK, width=4)
    d.rectangle((31, 18, 97, 68), fill='#e7d2a6', outline='#302c27', width=2)
    d.rectangle((35, 22, 93, 31), fill='#bd4d39')
    d.text((39, 22), 'AVISO', font=pixel_font(9), fill='#fff0cf')
    d.text((35, 36), 'ZONA', font=pixel_font(9), fill='#38362f')
    d.text((35, 47), 'CERRADA', font=pixel_font(9), fill='#38362f')
    d.rectangle((61, 75, 67, 103), fill='#554333', outline=INK, width=2)
    d.rectangle((48, 101, 80, 108), fill='#453b30', outline=INK, width=2)
    return im


save('mural_estamos_solos', brick_mural(['ESTAMOS', 'SOLOS']))
save('mural_aqui_somos_ciudad', brick_mural(['AQUI TAMBIEN', 'SOMOS CIUDAD']))
save('cableado_aereo_tramo', cable_segment())
save('poste_luz_calida', lamp_with_glow())
save('barrera_conos_calle', road_barrier())
save('aviso_zona_cerrada', poster_board())
print('Generated 6 reference detail sprites with Unity metadata.')
