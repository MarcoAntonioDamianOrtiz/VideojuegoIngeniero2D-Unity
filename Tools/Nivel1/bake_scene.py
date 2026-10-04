"""Bake the reference plan to a separate Unity scene, using existing serialized component templates."""
import argparse, copy, json, math, re, uuid
from pathlib import Path
from collections import Counter
import yaml
from path_geometry import on_path

parser=argparse.ArgumentParser()
parser.add_argument('--fiel',action='store_true',help='Bake the main level from independent terrain tiles and sprites')
args=parser.parse_args()
repo=Path(__file__).resolve().parents[2];root=repo/'Assets/Technopolis/Nivel1'
data=json.loads((root/'Editor/PlanoReferenciaNivel1.json').read_text())
sprite_config=json.loads((repo/'Tools/Nivel1/escena_principal_sprites.json').read_text()) if args.fiel else None
placements=copy.deepcopy(data['placements'])
if args.fiel:
 from PIL import Image
 for p in placements:
  if p['name'].startswith('Vivienda_'):
   p['width']=round(p['width']*sprite_config['houseWidthScale'],3)
  p.update(sprite_config.get('placementOverrides',{}).get(p['name'],{}))
  replacement=sprite_config['spriteOverrides'].get(p['name'])
  if replacement:
   p['sprite']=replacement
   left,top,right,bottom=Image.open(root/replacement).getchannel('A').point(lambda v:255 if v>160 else 0).getbbox()
   p.update(left=left,top=top,right=right,bottom=bottom)
 for p in sprite_config['extraPlacements']:
  p=copy.deepcopy(p)
  if 'left' not in p:
   p['left'],p['top'],p['right'],p['bottom']=Image.open(root/p['sprite']).getchannel('A').point(lambda v:255 if v>160 else 0).getbbox()
  placements.append(p)
 for group in sprite_config.get('repeatSprites',[]):
  image=Image.open(root/group['sprite']).convert('RGBA').getchannel('A').point(lambda v:255 if v>160 else 0)
  left,top,right,bottom=image.getbbox()
  for index,(c,r) in enumerate(group['points'],1):
   placements.append(dict(name=f"{group['name']}_{index:02}",sprite=group['sprite'],c=c,r=r,
    width=group['width'],height=group.get('height',0),angle=group.get('angle',0),kind=group.get('kind','prop'),solid=group.get('solid',False),
    left=left,top=top,right=right,bottom=bottom))
source_scene=repo/'Assets/Scenes/EscenaNivel1.unity'
if not source_scene.exists():source_scene=repo/'Assets/Scenes/Respaldos/EscenaNivel1.unity'
original=source_scene.read_text()
blocks={}; kinds={}
for m in re.finditer(r'^--- !u!(\d+) &(\d+)(?: stripped)?\n(.*?)(?=^--- !u!|\Z)',original,re.M|re.S):
 ident=int(m[2]);blocks[ident]=yaml.safe_load(m[3]);kinds[ident]=int(m[1])
def inner(ident):return next(iter(blocks[ident].values()))
def named(name):return next(i for i,b in blocks.items() if kinds[i]==1 and inner(i).get('m_Name')==name)
def component(go,kind):return next(v['component']['fileID'] for v in inner(go)['m_Component'] if kinds[v['component']['fileID']]==kind)
def template(kind):
 for i,b in blocks.items():
  if kinds[i]!=kind:continue
  obj=next(iter(b.values()))
  source=obj.get('m_CorrespondingSourceObject',{})
  prefab=obj.get('m_PrefabInstance',{})
  if source.get('fileID',0)==0 and prefab.get('fileID',0)==0:return copy.deepcopy(obj)
 raise ValueError(f'No standalone Unity template found for class {kind}')
def guid(p):return re.search(r'^guid: (\w+)',Path(str(p)+'.meta').read_text(),re.M)[1]
def ref(i):return dict(fileID=i)
def vec(x=0,y=0,z=0):return dict(x=round(x,7),y=round(y,7),z=round(z,7))

grid=named('Grid');grid_t=component(grid,4)
player=named('Jugador');player_t=component(player,4);player_sr=component(player,212)
keep={1,2,3,4}
root_transforms=[]
for name in ['Grid','Jugador','Main Camera','Global Light 2D']:
 go=named(name);keep.add(go)
 for x in inner(go)['m_Component']:keep.add(x['component']['fileID'])
 root_transforms.append(component(go,4))
if args.fiel:root_transforms=root_transforms[1:]+root_transforms[:1]
out={i:copy.deepcopy(blocks[i]) for i in keep}
out_kinds={i:kinds[i] for i in keep}
inner_out=lambda i:next(iter(out[i].values()))
inner_out(grid_t)['m_Children']=[]
if args.fiel:inner_out(grid_t)['m_LocalScale']=vec(1,sprite_config['verticalScale'],1)
inner_out(player_t)['m_LocalPosition']=vec(-40+(18.1 if args.fiel else 19.3),(20-(33.4 if args.fiel else 34.4))*(sprite_config['verticalScale'] if args.fiel else 1))
inner_out(player_sr)['m_SortingOrder']=344
capsule=component(player,70) if any(kinds[x['component']['fileID']]==70 for x in inner(player)['m_Component']) else None
if capsule:
 scale=inner_out(player_t)['m_LocalScale']
 inner_out(capsule)['m_Size']=dict(x=.72/abs(scale['x']),y=.90/abs(scale['y']))
 inner_out(capsule)['m_Offset']=dict(x=0,y=.45/abs(scale['y']))

next_id=3000000000
def put(kind,name,obj):
 global next_id
 i=next_id;next_id+=1;out[i]={name:obj};out_kinds[i]=kind;return i
def group(name,parent,position=None,scale=None,angle=0):
 go=template(1);go.update(m_Component=[],m_Name=name,m_IsActive=1,m_TagString='Untagged')
 g=put(1,'GameObject',go)
 t=template(4);t.update(m_GameObject=ref(g),m_LocalPosition=position or vec(),m_LocalScale=scale or vec(1,1,1),m_Children=[],m_Father=ref(parent),m_LocalEulerAnglesHint=vec(0,0,angle))
 a=math.radians(angle)/2;t['m_LocalRotation']=dict(x=0,y=0,z=math.sin(a),w=math.cos(a))
 tr=put(4,'Transform',t);go['m_Component'].append(dict(component=ref(tr)))
 if parent:inner_out(parent)['m_Children'].append(ref(tr))
 return g,tr
def add_component(go,kind,name,obj):
 obj['m_GameObject']=ref(go);ident=put(kind,name,obj)
 inner_out(go)['m_Component'].append(dict(component=ref(ident)));return ident

_,generated=group('Barrio_Fiel_Referencia' if args.fiel else 'Barrio_Referencia_08',grid_t)
bg,bld=group('Edificios_Sprites' if args.fiel else 'Edificios_Referencia',generated)
og,props=group('Objetos_Sprites' if args.fiel else 'Objetos_Referencia',generated)
sr_template=template(212); box_template=template(61)
if args.fiel:
 camera=named('Main Camera')
 pixel=template(114)
 pixel={k:v for k,v in pixel.items() if k.startswith('m_')}
 pixel.update(m_Script=dict(fileID=11500000,guid='c88f5cead0c0b2a4eb05b5900433f8d1',type=3),m_EditorClassIdentifier='',m_Name='',m_ComponentVersion=1,m_AssetsPPU=32,m_RefResolutionX=640,m_RefResolutionY=360,m_CropFrame=0,m_GridSnapping=2,m_FilterMode=1)
 add_component(camera,114,'MonoBehaviour',pixel)
circles=[b for i,b in blocks.items() if kinds[i]==58]
circle_template=copy.deepcopy(next(iter(circles[0].values()))) if circles else copy.deepcopy(box_template)
for key in ['m_Size','m_EdgeRadius','m_AutoTiling','m_SpriteTilingProperty']:circle_template.pop(key,None)
collisions=[]
def box(go,x,y,w,h):
 b=copy.deepcopy(box_template);b.update(m_Offset=dict(x=x,y=y),m_Size=dict(x=w,y=h),m_IsTrigger=0,m_Enabled=1)
 add_component(go,61,'BoxCollider2D',b)
def faithful_box(go,p,x,y,w,h,name=None):
 box(go,x,y,w,h)
 collisions.append(dict(name=name or p['name'],shape='box',bounds=[round(p['c']+x-w/2,4),round(p['r']-y-h/2,4),round(p['c']+x+w/2,4),round(p['r']-y+h/2,4)]))
def faithful_circle(go,p,x,y,radius):
 circle=copy.deepcopy(circle_template)
 circle.update(m_Radius=radius,m_Offset=dict(x=x,y=y),m_IsTrigger=0,m_Enabled=1)
 add_component(go,58,'CircleCollider2D',circle)
 collisions.append(dict(name=p['name'],shape='circle',center=[round(p['c']+x,4),round(p['r']-y,4)],radius=radius))
def place(p,parent):
 path=root/p['sprite'];meta=Path(str(path)+'.meta').read_text()
 ppu=float(re.search(r'spritePixelsToUnits: ([\d.]+)',meta)[1])
 pivot_m=re.search(r'spritePivot: \{x: ([\d.]+), y: ([\d.]+)\}',meta)
 from PIL import Image
 im=Image.open(path);pivot=(float(pivot_m[1])*im.width,float(pivot_m[2])*im.height)
 sx=p['width']*ppu/(p['right']-p['left']);sy=p['height']*ppu/(p['bottom']-p['top']) if p['height'] else sx
 theta=math.radians(p['angle']);cs,ss=math.cos(theta),math.sin(theta)
 corners=[]
 for x in [p['left'],p['right']]:
  for y in [im.height-p['bottom'],im.height-p['top']]:
   a,b=(x-pivot[0])/ppu*sx,(y-pivot[1])/ppu*sy;corners.append((a*cs-b*ss,a*ss+b*cs))
 minx=min(x for x,y in corners);maxx=max(x for x,y in corners);miny=min(y for x,y in corners);maxy=max(y for x,y in corners)
 go,anchor=group(p['name'],parent,vec(-40+p['c'],20-p['r']))
 vgo,visual=group('Sprite',anchor,vec(-(minx+maxx)/2,-miny),vec(sx,sy,1),p['angle'])
 sr=copy.deepcopy(sr_template)
 sr.update(m_Sprite=dict(fileID=21300000,guid=guid(path),type=3),m_Color=dict(r=1,g=1,b=1,a=1),m_SortingOrder=p.get('sortingOrder',-80 if p['kind']=='floor' else round(p['r']*10)),m_SpriteSortPoint=1,m_FlipX=0,m_FlipY=0,m_Size=dict(x=im.width/ppu,y=im.height/ppu))
 add_component(vgo,212,'SpriteRenderer',sr)
 if not p['solid']:return
 w,h=maxx-minx,maxy-miny
 if args.fiel:
  if p['name']=='Casa_Alex':
   wall=.22;half=w*.42;door=1.35;side=(half-door)/2
   faithful_box(go,p,0,h-wall/2,w*.84,wall,'Casa_Alex_Norte')
   faithful_box(go,p,-half,h/2,wall,h-.3,'Casa_Alex_Oeste')
   faithful_box(go,p,half,h/2,wall,h-.3,'Casa_Alex_Este')
   faithful_box(go,p,-door-side/2,wall/2,side,wall,'Casa_Alex_Sur_Oeste')
   faithful_box(go,p,door+side/2,wall/2,side,wall,'Casa_Alex_Sur_Este')
  elif p['kind']=='building':
   depth=min(1.45,max(.65,h*.22))
   faithful_box(go,p,0,depth/2+.08,w*.72,depth)
  elif p['name'].startswith('Arbol_'):
   faithful_circle(go,p,0,.55,.6)
  elif p['name'].startswith(('Poste_','Farol_')):
   faithful_circle(go,p,0,.2,.25)
  elif p['name']=='Fuente_Parque':
   faithful_circle(go,p,0,.55,.85)
  elif p['name']=='Columpios':
   for side in [-1,1]:faithful_box(go,p,side*w*.4,.2,.22,.4,p['name'])
  elif p['name']=='Resbaladilla':faithful_box(go,p,0,.45,.7,.9)
  elif p['name']=='Porton_Salida':faithful_box(go,p,0,4.0,w*.84,.85)
  elif p['name'].startswith('Muro_Salida_'):faithful_box(go,p,0,h/2,.55,h)
  else:faithful_box(go,p,0,.3,w*.72,.6)
  return
 if p['kind']=='building':
  box(go,0,h*.32+.1,w*.86,h*.64)
  collisions.append((p['c']-w*.43,p['r']-h*.64-.1,p['c']+w*.43,p['r']-.1,p['name']))
 elif p['name'].startswith('Arbol_') and circle_template:
  circle=copy.deepcopy(circle_template);circle.update(m_Radius=.85,m_Offset=dict(x=0,y=.8),m_IsTrigger=0,m_Enabled=1)
  add_component(go,58,'CircleCollider2D',circle);collisions.append((p['c']-.85,p['r']-1.65,p['c']+.85,p['r']+.05,p['name']))
 elif p['name']=='Columpios':
  for side in [-1,1]:
   x=side*w*.42;box(go,x,.18,.25,.35)
   collisions.append((p['c']+x-.125,p['r']-.355,p['c']+x+.125,p['r']-.005,p['name']))
 else:
  fh=1.35 if p['name']=='Fuente_Parque' else h*.7 if p['name']=='Auto_Taller' else .45
  box(go,0,fh/2,w*.78,fh)
  collisions.append((p['c']-w*.39,p['r']-fh,p['c']+w*.39,p['r'],p['name']))

for p in placements:place(p,bld if p['kind']=='building' else props)
for i in range(8):
 for name,c,r,angle in [('Muro_Norte',4+i*8,.95,0),('Muro_Sur',4+i*8,64,0),('Muro_Oeste',.475,8+i*8,90),('Muro_Este',63.525,8+i*8,90)]:
  place(dict(name=f'{name}_{i}',sprite='Sprites/04_Objetos/muro_perimetral.png',c=c,r=r,width=8,height=.95,angle=angle,left=0,top=11,right=128,bottom=37,solid=False,kind='prop'),props)

# Keep the baked floors identical to the editor command and its preview.
def lane(c,r):
 if args.fiel:return on_path(c,r,sprite_config['pathSegments'])
 for l in data['lanes']:
  if min(l['c1'],l['c2'])-l['width']/2<=c<=max(l['c1'],l['c2'])+l['width']/2 and min(l['r1'],l['r2'])-l['width']/2<=r<=max(l['r1'],l['r2'])+l['width']/2:return True
 return False
def garden(c,r):
 for p in placements:
  if p['kind']!='building':continue
  h=p['height'] or p['width']*(p['bottom']-p['top'])/(p['right']-p['left']);l=p['c']-p['width']/2;rr=p['c']+p['width']/2;t=p['r']-h
  near=l-2<c<rr+2 and t-.7<r<p['r']+2.1;inside=l+.65<c<rr-.65 and t+.65<r<p['r']-.25
  if near and not inside:return True
 return c<1.7 or c>62.3 or r<1.5 or r>62.5
ground=[];paths=[]
for row in range(64):
 for col in range(64):
  material='tierra'
  for z in data['regions']:
   if z['c']<=col<z['c']+z['w'] and z['r']<=row<z['r']+z['h']:material=z['material']
  if material=='tierra' and garden(col+.5,row+.5) and not lane(col+.5,row+.5) and (col*73+row*29)%10<7:material='pasto'
  ground.append((col,row,material))
  if material=='tierra' and lane(col+.5,row+.5):paths.append((col,row,'tierra'))
def floor(name,entries,order,color):
 go,tr=group(name,generated);tm=template(1839735485);tm.update(m_AnimatedTiles={},m_TileObjectToInstantiateArray=[],m_Color=color,m_Origin=vec(-40,-44),m_Size=vec(64,64,1))
 keys=list(dict.fromkeys((m,r%4,c%4) for c,r,m in entries));counts=Counter((m,r%4,c%4) for c,r,m in entries)
 tm['m_Tiles']=[]
 for c,r,m in sorted(entries,key=lambda x:(-x[1],x[0])):
  k=keys.index((m,r%4,c%4));tm['m_Tiles'].append(dict(first=vec(-40+c,19-r),second=dict(serializedVersion=2,m_TileIndex=k,m_TileSpriteIndex=k,m_TileMatrixIndex=0,m_TileColorIndex=0,m_TileObjectToInstantiateIndex=65535,dummyAlignment=0,m_AllTileFlags=1)))
 tm['m_TileAssetArray']=[];tm['m_TileSpriteArray']=[]
 for m,r,c in keys:
  folder='02_Caminos' if m in ['concreto','empedrado'] else '01_Terreno';path=root/f'Tiles/{folder}/{m}_bloque_f{r}_c{c}.asset'
  tiletext=path.read_text();sprite_match=re.search(r'm_Sprite: \{fileID: (\d+), guid: (\w+), type: (\d+)\}',tiletext)
  tm['m_TileAssetArray'].append(dict(m_RefCount=counts[(m,r,c)],m_Data=dict(fileID=11400000,guid=guid(path),type=2)))
  tm['m_TileSpriteArray'].append(dict(m_RefCount=counts[(m,r,c)],m_Data=dict(fileID=int(sprite_match[1]),guid=sprite_match[2],type=int(sprite_match[3]))))
 tm['m_TileMatrixArray']=copy.deepcopy(tm['m_TileMatrixArray'][:1]);tm['m_TileMatrixArray'][0]['m_RefCount']=len(entries)
 tm['m_TileColorArray']=[dict(m_RefCount=len(entries),m_Data=dict(r=1,g=1,b=1,a=1))]
 add_component(go,1839735485,'Tilemap',tm)
 renderer=template(483693784);renderer.update(m_SortingOrder=order,m_SortingLayer=0,m_SortingLayerID=0)
 add_component(go,483693784,'TilemapRenderer',renderer)
floor('Suelo_Tiles' if args.fiel else 'Suelo_Referencia',ground,-1000 if args.fiel else -100,dict(r=1,g=1,b=1,a=1))
floor('Senderos_Tiles' if args.fiel else 'Senderos_Referencia',paths,-900 if args.fiel else -90,dict(r=1.2,g=1.13,b=1.02,a=1))
_,limits=group('Limites_Referencia',generated)
for name,x,y,w,h in [('Oeste',-40.2,-12,.4,64),('Este',24.2,-12,.4,64),('Norte',-8,20.2,64,.4),('Sur',-8,-44.2,64,.4)]:
 go,_=group(name,limits,vec(x,y));box(go,0,0,w,h)
depth=template(114)
depth={k:v for k,v in depth.items() if k.startswith('m_')}
depth.update(m_Script=dict(fileID=11500000,guid=guid(repo/'Assets/Scripts/TechnopolisOrdenBarrio.cs'),type=3),m_EditorClassIdentifier='Assembly-CSharp::TechnopolisOrdenBarrio',jugador=ref(player_sr),escalaVertical=sprite_config['verticalScale'] if args.fiel else 1,m_Name='')
add_component(inner_out(generated)['m_GameObject']['fileID'],114,'MonoBehaviour',depth)
if args.fiel:
 _,terrain=group('Terreno_Base_Sprite',generated)
 terrain_path=sprite_config['terrainSprite']
 terrain_image=Image.open(root/terrain_path)
 place(dict(name='Terreno_Organico',sprite=terrain_path,c=32,r=64,width=64,height=64,
  angle=0,kind='floor',solid=False,left=0,top=0,right=terrain_image.width,bottom=terrain_image.height,
  sortingOrder=-850),terrain)
out[9223372036854775807]={'SceneRoots':dict(m_ObjectHideFlags=0,m_Roots=[ref(i) for i in root_transforms])};out_kinds[9223372036854775807]=1660057539

class Dumper(yaml.SafeDumper):pass
def dict_rep(dumper,d):
 flow=set(d).issubset({'fileID','guid','type'}) or set(d) in [set('xyz'),set('xyzw'),set('rgba'),set('xy')]
 return dumper.represent_mapping('tag:yaml.org,2002:map',d,flow_style=flow)
Dumper.add_representer(dict,dict_rep)
text='%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n'
for i,b in out.items():text+=f'--- !u!{out_kinds[i]} &{i}\n'+yaml.dump(b,Dumper=Dumper,sort_keys=False,allow_unicode=True,width=120)
target=repo/('Assets/Scenes/EscenaNivel1_FielReferencia.unity' if args.fiel else 'Assets/Scenes/EscenaNivel1_Referencia.unity');target.write_text(text,newline='\n')
meta=Path(str(target)+'.meta')
if not meta.exists():meta.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\nDefaultImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n')
footprints_path=repo/('Tools/Nivel1/colisiones_generadas_fiel.json' if args.fiel else 'Documentacion/Nivel1/Huellas_Colisiones.json')
footprints_path.write_text(json.dumps(collisions,indent=2 if args.fiel else None),newline='\n')
footprint_count=len(collisions)
print('Baked',target,len(out),'serialized objects;',len(ground),'ground cells;',len(paths),'path cells;',footprint_count,'footprints')
