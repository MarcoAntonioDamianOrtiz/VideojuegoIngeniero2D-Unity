import json
import re
from pathlib import Path

from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
LEVEL = ROOT / 'Assets/Technopolis/Nivel1'
plan = json.loads((LEVEL / 'Editor/PlanoReferenciaNivel1.json').read_text())
scene = (ROOT / 'Assets/Scenes/EscenaNivel1_Referencia.unity').read_text()
missing = [p['sprite'] for p in plan['placements'] if not (LEVEL / p['sprite']).is_file()]
names = ['mural_estamos_solos', 'mural_aqui_somos_ciudad', 'cableado_aereo_tramo',
         'poste_luz_calida', 'barrera_conos_calle', 'aviso_zona_cerrada']
guids = []
sprite_ids = []
for name in names:
    path = LEVEL / 'Sprites/04_Objetos' / f'{name}.png'
    meta = Path(f'{path}.meta').read_text()
    guid = re.search(r'^guid: (\w+)$', meta, re.M).group(1)
    sprite_id = re.search(r'^\s+spriteID: (\w+)$', meta, re.M).group(1)
    image = Image.open(path).convert('RGBA')
    if image.getbbox() is None or image.getchannel('A').getextrema()[0] != 0:
        raise SystemExit(f'{name}: expected visible pixels and transparent margins')
    if guid not in scene:
        raise SystemExit(f'{name}: scene does not reference its Unity GUID')
    guids.append(guid)
    sprite_ids.append(sprite_id)
if missing:
    raise SystemExit(f'Missing sprite assets: {missing}')
if len(set(guids)) != len(guids) or len(set(sprite_ids)) != len(sprite_ids):
    raise SystemExit('Duplicate sprite GUID or sprite ID')
print(f'OK: {len(plan["placements"])} placements, all assets found, six transparent sprites referenced with unique IDs.')
