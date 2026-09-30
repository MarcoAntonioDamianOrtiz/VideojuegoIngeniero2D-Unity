import json, random, uuid
from pathlib import Path
from PIL import Image, ImageDraw

REPO=Path(__file__).resolve().parents[2]
ROOT=REPO/'Assets/Technopolis/Nivel1'
specs=[]
def add(sprite,c,r,w,kind='prop',name=None,angle=0,h=0,solid=False):
    folder='05_Edificios' if kind=='building' else '04_Objetos'
    path=ROOT/'Sprites'/folder/(sprite+'.png')
    im=Image.open(path).convert('RGBA'); box=im.getbbox()
    specs.append(dict(name=name or f'{sprite}_{len(specs):03}',sprite=f'Sprites/{folder}/{sprite}.png',c=c,r=r,width=w,height=h,angle=angle,kind=kind,solid=solid,left=box[0],top=box[1],right=box[2],bottom=box[3]))

# Coordinates traced from the reference, expressed in its 64 by 64 grid.
houses=[
('vivienda_parches',21.8,6.3,5.2),('vivienda_techo_lamina',27.4,9.3,5.6),
('vivienda_ladrillo',36.8,9.6,5.6),('vivienda_techo_lamina',21.8,14.6,5.7),
('vivienda_parches',30.4,15,5.5),('vivienda_techo_lamina',25,20.9,5.4),
('vivienda_parches',32.7,20,4.1),('vivienda_techo_lamina',44,19.8,5.7),
('vivienda_parches',54.2,20.4,5.3),('vivienda_ladrillo',59.4,18.1,3.9),
('vivienda_parches',5.5,28.6,5.8),('vivienda_techo_lamina',29,27.5,5.4),
('vivienda_techo_lamina',5.3,37.7,5.2),('vivienda_parches',6,45.5,5.8),
('vivienda_techo_lamina',7.5,51.8,6),('vivienda_ladrillo',20.5,46.9,4.8),
('vivienda_techo_lamina',23.1,51,5.4),('vivienda_ladrillo',57.1,49.4,5.7),
('vivienda_parches',7.5,57.9,5.2),('vivienda_ladrillo',15.7,59.5,4.1),
('vivienda_techo_lamina',20.3,60.4,4.4),('vivienda_parches',27.1,60.3,4.8),
('vivienda_ladrillo',33,60.5,4.2),('vivienda_techo_lamina',44,58.8,5.2),
('vivienda_parches',43.8,55.3,4.8),('vivienda_ladrillo',37.1,56.1,4.4),
('vivienda_ladrillo',5,61.4,4.5),('vivienda_techo_lamina',49.9,60,4.5),
('vivienda_techo_lamina',17.1,4.8,4.4),('vivienda_ladrillo',32.8,6.1,4.2),
('vivienda_parches',16.8,18.4,4.5),('vivienda_ladrillo',38.2,16.3,4.3),
('vivienda_techo_lamina',50.2,15.9,4.5),('vivienda_parches',33.8,25.1,4.2),
('vivienda_ladrillo',17.1,39.3,4.2),('vivienda_techo_lamina',24.4,43.7,4.5),
('vivienda_parches',30.1,57.1,4.2),('vivienda_ladrillo',53.4,57.8,4.4)]
for i,(s,c,r,w) in enumerate(houses): add(s,c,r,w,'building',f'Vivienda_{i+1:02}',solid=True)
add('casa_alex_interior',18.1,31.6,8.8,'building','Casa_Alex',h=8.6,solid=True)
add('panaderia_fachada',42,27.5,10.7,'building','Panaderia_Ramona',h=6.6,solid=True)
add('miscelanea_fachada',58.1,28.1,8.4,'building','Miscelanea',h=7.2,solid=True)
add('taller_fachada',56.1,38.2,10.5,'building','Taller',h=8,solid=True)

# Landmarks retain exactly the reference relationships.
add('arbol_plaza_monumental',35.5,35.3,9.1,name='Arbol_Plaza',h=7.8,solid=True)
add('fuente_pequena',37.7,49.1,4.1,name='Fuente_Parque',solid=True)
add('columpios_viejos',34.2,45.2,5.4,name='Columpios',h=2.8,solid=True)
add('resbaladilla_vieja',41.3,46.8,3.6,name='Resbaladilla',h=3.5,solid=True)
add('porton_salida_bloqueada',58.5,56.5,7.8,name='Porton_Salida',solid=True)
add('contenedor_basura_callejon',10.1,17.9,4.7,name='Contenedor_Sur',solid=True)
add('contenedor_basura_callejon',5.7,10.6,3.1,name='Contenedor_Norte',solid=True)
add('carro_viejo',57.7,39.5,2.4,name='Auto_Taller',solid=True)
add('tunel_callejon',7.2,6.6,6,name='Tunel_Callejon',h=5.5)
add('cancha_barrio',43.5,38.6,5.5,kind='floor',name='Cancha_Plaza',h=7.4)
add('vagon_norte',36.2,2.6,14.4,name='Vagon_Borde_Norte',h=2.25)
for c,r in [(48.3,6.2),(52.8,6.2),(51.2,10.1),(56,10.1)]:add('huerto_comunitario',c,r,4.4)
for c,r in [(57.9,6.2),(60.2,9.9),(46.6,9.8),(7.8,31.7),(8.5,40.6),(8.5,43.2),(10,55),(19.8,37.6)]:add('huerto_comunitario',c,r,3.4)
add('puesto_mercado_lona',21.4,47.8,4.3,name='Puesto_Sur')
add('puesto_mercado_lona',55.7,46.4,4.4,name='Puesto_Salida')

def fence(c1,c2,r,gaps=()):
    c=c1+1.1
    while c<c2:
        if not any(a<=c<=b for a,b in gaps):add('cerca_madera',c,r,2.25)
        c+=2.15
def vfence(c,r1,r2,gaps=()):
    r=r1+1.1
    while r<r2:
        if not any(a<=r<=b for a,b in gaps):add('cerca_madera',c,r,2.25,angle=90)
        r+=2.15
# Courtyards and an enclosed park with three unobstructed entrances.
fence(29,48,42.5,[(36,39)])
fence(29,48,52,[(35,39)])
vfence(29,43.5,51.8,[(46.3,49)])
vfence(48,43.5,51.8,[(46,49)])
fence(13.7,23,32.9,[(17,20)])
vfence(13.6,25,32.2);vfence(23.5,25,32)
fence(15.5,31.6,15.6,[(24,27)]);fence(20,36,21.7,[(27,31)])
fence(40,48,20.6,[(43,46)]);fence(47,59,11,[(51,54)])
fence(4,11,30,[(7,9)]);fence(4,10,39,[(6,8)])
fence(1.5,11,52.6,[(5,8)]);fence(13,26,51.9,[(18,21)])
fence(13,28,61.8,[(22,25)]);fence(29,51,62,[(39,42)])
vfence(12.8,44,51,[(46,49)]);vfence(51,44,52,[(47,49)])
fence(29,47.5,30,[(32,39)])
fence(29,47.5,41,[(34,39)])
vfence(28.4,31,40,[(35,38)]);vfence(47.5,31,40,[(34.5,37)])
fence(15,29,10.4,[(22,26)]);fence(30,41,10.4,[(34,37)])
vfence(59.9,3,11,[(6,8)])
for r in [5,8,11,14,17,20]:add('muro_perimetral',13.6,r,3,angle=90,h=.65)

for c,r in [(30.9,39.4),(41,40),(31.3,32),(45.4,31.7),(31.4,48),(19,34.8),(28,28.6)]:add('banco_viejo',c,r,2.9)
for c,r in [(31,36),(46,38.5),(27.5,29),(28.5,40.2),(49,42),(25,24),(13.5,21.5),(16.5,36),(13.2,42),(27.5,53),(42,54),(54,54.5),(62,54.5),(47,7),(59.8,12.5),(10.5,8),(3,18.7),(60.5,30)]:add('poste_luz',c,r,1.05)
for c,r in [(15,22),(31,22),(48,21),(25,41),(28,54),(50,54),(61,42),(15,4)]:add('poste_electrico',c,r,1.8)
for c,r in [(25,6),(34,18),(18,41),(36,54),(22,62)]:add('tendedero',c,r,4)
for c,r in [(6,6.5),(12,14),(6,19),(1.9,17),(51,36),(61,40),(28,32),(10,34),(23,48),(51,23),(59,45),(35,54)]:add('tambo_azul',c,r,1.2)
for c,r in [(7,14),(4,12),(11,18),(2,7),(53,41),(60,38),(49,28)]:add('tambo_oxidado',c,r,1.15)
for c,r in [(3,11),(8,12.5),(9.7,19),(3.5,15),(12,9),(12,19),(6.4,17),(7,8),(11,6)]:add('bolsas_basura',c,r,2)
for c,r in [(3,8),(12,15),(49,39),(51,41),(61,41)]:add('llanta_vieja',c,r,1.1)
for c,r in [(11,27),(11,35),(11,45),(23,35),(16,40),(24,8),(39,11),(57,12),(50,49)]:
 add('tambo_azul',c,r,1.05);add('caja_madera',c+1,r+.35,.95)
 add('maceta_reutilizada',c-1,r+.2,.8)
for c,r in [(19,8.5),(26,16),(7,32.2),(21,39),(11,48.3),(33,52.6)]:add('tendedero',c,r,3)
for c,r in [(37,27.8),(48,27.9),(24,17),(39,7),(24,30),(8,44),(31,55),(52,30),(60,49),(61,23)]:add('cajas_apiladas',c,r,1.6)
for c,r in [(36,28.8),(39,28.8)]:add('pila_cajas_harina',c,r,1.5)
for c,r in [(19,35),(12,40),(50,19),(61,28.8),(49,43),(24,53)]:add('bicicleta_vieja',c,r,1.8)
for c,r in [(55.3,56.2),(61.6,56.2)]:add('valla_obras',c,r,1.5,solid=True)
add('senal_bloqueo',58.5,57.7,1.6)
for c,r,w in [(13,5,2.6),(31.5,9,3.9),(38.7,8,3),(59,9.5,3.5),(57.5,15,3),(2.1,24.5,3.8),(26.5,27,3.2),(24.9,31,2.4),(29,39.5,3.2),(38.3,40.6,3.5),(30,44.5,2.9),(45.5,44.3,3.6),(47,50.8,3.4),(30,51.7,2.8),(32.4,57,3.1),(39,61.2,3),(48.8,61.6,3.5),(11,60.9,3),(2.5,56.5,3)]:add('arbol_barrio',c,r,w)

# Street-level details that make the residential lanes read as lived-in blocks.
for c,r,w in [(2.5,7.8,1.8),(8.8,15.7,1.7),(11.4,18.8,1.6),(54.7,40.7,1.8),(61,43.2,1.7)]:
    add('bolsa_basura_negra',c,r,w)
for c,r,w in [(16.8,8.7,2.1),(40.8,18.5,2.3),(17.2,42.7,2.2),(50.4,17.7,2.2),(52.5,55.3,2.3)]:
    add('cartel_madera',c,r,w)
for c,r,angle in [(17.2,18.1,0),(39.4,19.6,90),(12.1,48.5,90),(50.4,20.4,0),(46.9,54.2,90)]:
    add('cerca_rota',c,r,2.2,angle=angle)
for c,r in [(10.8,21),(24.2,18.6),(39.5,22.1),(53.5,18.6),(17.2,54.5),(31.5,54.8),(52,52.1)]:
    add('arbusto_seco',c,r,1.5)

# Distinct landmarks and infrastructure taken from the reference illustration.
add('mural_estamos_solos',11.1,6.7,3.8,name='Mural_Estamos_Solos')
add('mural_aqui_somos_ciudad',2,58,4.2,name='Mural_Aqui_Somos_Ciudad')
for c,r,w in [(23,22.1,7),(39.5,22.1,7),(21.8,55.5,7),(37.2,55.5,7)]:
    add('cableado_aereo_tramo',c,r,w,name=f'Cable_Aereo_{len(specs):03}',h=1.9)
for c,r in [(16.2,24.2),(27.4,24),(38.8,23.6),(49,28.8),(28.2,39.2),(47.5,42.4),
            (24.8,53),(40.8,53),(53.1,52.5)]:
    add('poste_luz_calida',c,r,1.8,name=f'Poste_Luz_Calida_{len(specs):03}',h=2.6)
add('barrera_conos_calle',57.1,41.9,5.4,name='Conos_Taller',h=3.6)
add('barrera_conos_calle',58.5,58.5,5.4,name='Conos_Salida',h=3.6)
add('aviso_zona_cerrada',58.5,59.5,2.6,name='Aviso_Salida_Cerrada',h=2.3)

regions=[
dict(material='asfalto',c=1,r=2,w=13,h=18),
dict(material='concreto',c=29,r=30,w=18,h=11),
dict(material='empedrado',c=28,r=30,w=1,h=11),
dict(material='empedrado',c=47,r=30,w=1,h=11),
dict(material='pasto',c=29,r=43,w=19,h=9),
dict(material='grava',c=50,r=30,w=12,h=12),
dict(material='asfalto',c=53,r=36,w=9,h=6),
dict(material='asfalto',c=54,r=50,w=9,h=14),
dict(material='concreto',c=35,r=27,w=28,h=2),
]
lanes=[dict(c1=14,r1=2,c2=14,r2=61,width=2.5),dict(c1=14,r1=11,c2=44,r2=11,width=2.5),dict(c1=35,r1=10,c2=35,r2=22,width=2.8),dict(c1=44,r1=5,c2=44,r2=23,width=2.5),dict(c1=14,r1=22,c2=61,r2=22,width=2.8),dict(c1=25,r1=22,c2=25,r2=43,width=2.5),dict(c1=14,r1=40.5,c2=51,r2=40.5,width=2.5),dict(c1=14,r1=43,c2=14,r2=54,width=2.5),dict(c1=27,r1=41,c2=27,r2=61,width=2.5),dict(c1=14,r1=53.5,c2=53,r2=53.5,width=2.8),dict(c1=49.5,r1=23,c2=49.5,r2=53,width=2.6),dict(c1=37.5,r1=40,c2=37.5,r2=44,width=2.7),dict(c1=37.5,r1=49.5,c2=37.5,r2=54,width=2.7)]

def in_lane(c,r,margin=0):
    for l in lanes:
        if min(l['c1'],l['c2'])-l['width']/2-margin<=c<=max(l['c1'],l['c2'])+l['width']/2+margin and min(l['r1'],l['r2'])-l['width']/2-margin<=r<=max(l['r1'],l['r2'])+l['width']/2+margin:return True
    return False
def building_rect(s):
    hh=s['height'] or s['width']*(s['bottom']-s['top'])/(s['right']-s['left'])
    return s['c']-s['width']/2,s['r']-hh,s['c']+s['width']/2,s['r']
bounds=[building_rect(s) for s in specs if s['kind']=='building']
rng=random.Random(20260929)
# Greenery follows house foundations, fences and perimeter; no random solid obstacles.
for s in (p for p in specs if p['kind']=='building'):
    l,t,rr,b=building_rect(s)
    for c,r in [(l-.35,b-.15),(rr+.4,b-.3),(l+.7,b+.7),(rr-.6,b+.65)]:
        if 1<c<63 and 2<r<62 and not in_lane(c,r,.3):add('maleza_verde',c,r,rng.uniform(1.4,2.2))
for i in range(280):
    c=rng.uniform(1.7,62);r=rng.uniform(2,62)
    if in_lane(c,r,.3) or any(l-.5<c<rr+.5 and t-.3<r<b+.6 for l,t,rr,b in bounds):continue
    if 29<c<48 and 30<r<41:continue
    add('maleza_verde' if i%4 else 'maceta_reutilizada',c,r,rng.uniform(.9,1.6))

for c in range(2,63,2):
 for r in [2,62.5]:
  if not in_lane(c,r,.1):add('maleza_verde',c,r,1.6)
for r in range(5,61,2):
 for c in [1.5,62.5]:add('maleza_verde',c,r,1.5)
for c,r in [(29,43),(31,43),(43,43),(46,43),(29,45),(29,50.5),(31,51.5),(33,51.5),(41,51.5),(43,51.5),(45,51.5),(48,44),(48,46),(48,50),(45,4),(47,4),(50,4),(54,4),(57,4),(60,4),(47,11),(49,11),(55,11),(59,11),(12,6),(12,10),(12,18),(13,21),(3,20),(5,20),(7,20)]:add('maleza_verde',c,r,1.7)

data=dict(version=1,placements=specs,regions=regions,lanes=lanes)
out=ROOT/'Editor/PlanoReferenciaNivel1.json';out.write_text(json.dumps(data,ensure_ascii=False,indent=2),newline='\n')
meta=Path(str(out)+'.meta')
if not meta.exists():meta.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\nTextScriptImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n')
print(len(specs),'placements;',len(houses)+4,'buildings')

# Deterministic scene preview using the same sprite geometry and floor coordinates.
S=16; canvas=Image.new('RGBA',(64*S,64*S))
def material(c,r):
    m='tierra'
    for z in regions:
        if z['c']<=c<z['c']+z['w'] and z['r']<=r<z['r']+z['h']:m=z['material']
    return m
def garden(c,r):
    for l,t,rr,b in bounds:
        near=l-2<c<rr+2 and t-.7<r<b+2.1
        inside=l+.65<c<rr-.65 and t+.65<r<b-.25
        if near and not inside:return True
    return c<1.7 or c>62.3 or r<1.5 or r>62.5
for r in range(64):
 for c in range(64):
    m=material(c+.5,r+.5)
    if m=='tierra' and garden(c+.5,r+.5) and not in_lane(c+.5,r+.5) and (c*73+r*29)%10<7:m='pasto'
    folder='02_Caminos' if m in ['concreto','empedrado'] else '01_Terreno'
    tile=Image.open(ROOT/f'Sprites/{folder}/{m}_bloque_f{r%4}_c{c%4}.png').convert('RGBA').resize((S,S),Image.Resampling.NEAREST)
    canvas.alpha_composite(tile,(c*S,r*S))
    if in_lane(c+.5,r+.5) and m=='tierra':
        import numpy as np
        a=np.array(tile);a[:,:,:3]=np.clip(a[:,:,:3].astype(float)*[1.2,1.13,1.02],0,255).astype('uint8');canvas.alpha_composite(Image.fromarray(a),(c*S,r*S))
preview_specs=specs.copy()
for i in range(8):
 for name,c,r,angle in [('MuroN',4+i*8,.95,0),('MuroS',4+i*8,64,0),('MuroO',.475,8+i*8,90),('MuroE',63.525,8+i*8,90)]:
  preview_specs.append(dict(sprite='Sprites/04_Objetos/muro_perimetral.png',left=0,top=11,right=128,bottom=37,width=8,height=.95,c=c,r=r,angle=angle,kind='prop'))
for s in sorted(preview_specs,key=lambda s:-100 if s['kind']=='floor' else s['r']):
 im=Image.open(ROOT/s['sprite']).convert('RGBA').crop((s['left'],s['top'],s['right'],s['bottom']))
 h=s['height'] or s['width']*im.height/im.width
 im=im.resize((max(1,round(s['width']*S)),max(1,round(h*S))),Image.Resampling.NEAREST)
 if s['angle']:im=im.rotate(-s['angle'],expand=True)
 x=round(s['c']*S-im.width/2);y=round(s['r']*S-im.height)
 canvas.alpha_composite(im,(x,y))
canvas.save(REPO/'Documentacion/Nivel1/Vista_Previa_Referencia.png')
