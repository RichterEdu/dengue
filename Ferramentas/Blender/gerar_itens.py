"""
Gera os itens do kit na mão do agente de saúde (low poly, estáticos) e exporta
FBX, texturas e prévias: balde de areia, tela com moldura e borrifador.

Uso:
  blender -b --factory-startup --python gerar_itens.py -- <pasta_saida> [--sem-render]

Convenções:
  - Metros, Z pra cima, faces chapadas (como o saco).
  - Balde: Ø 22 cm, 17 cm de altura. Pivô no MEIO DA ALÇA, com a boca 15 cm
    abaixo dele. Objetos "Balde" e "Areia" (some quando a areia acaba).
  - Tela: 29 × 29 cm, moldura de 1,7 cm e 0,8 cm de espessura, deitada no plano
    XY, com o centro no pivô. Objetos "Moldura" e "Malha". O mesmo modelo vai
    para a mão (escala 0,67) e para o ralo (escala 1, sob a grelha).
  - Borrifador: ~20 cm de altura, pivô no gargalo, bico para +Y. Objeto único.
  - Saída: Balde.fbx, Tela.fbx, Borrifador.fbx, Tela_Malha.png e Particula.png
    (copiar para Assets/Meus/Assets Gerados/Itens/) e as prévias previa_*.png.
"""
import bpy, bmesh, math, os, struct, sys, zlib
from mathutils import Vector

ARGS = sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else []
OUT = os.path.abspath(ARGS[0]) if ARGS else os.path.abspath('.')
RENDER = '--sem-render' not in ARGS
os.makedirs(OUT, exist_ok=True)

bpy.ops.wm.read_factory_settings(use_empty=True)
scene = bpy.context.scene

# ------------------------------------------------------------------ cores
def _lin(c):
    return c / 12.92 if c <= 0.04045 else ((c + 0.055) / 1.055) ** 2.4

def hexcol(h):
    h = h.lstrip('#')
    return tuple(_lin(int(h[i:i + 2], 16) / 255) for i in (0, 2, 4)) + (1.0,)

def material(nome, cor, rugosidade=0.6):
    mat = bpy.data.materials.new(nome)
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get('Principled BSDF')
    bsdf.inputs['Base Color'].default_value = hexcol(cor)
    bsdf.inputs['Roughness'].default_value = rugosidade
    mat.diffuse_color = hexcol(cor)
    mat.use_backface_culling = True  # igual ao URP
    return mat

# ------------------------------------------------------------------ peças
def torno(bm, perfil, seg=14, ox=1.0, oy=1.0, mat=0, tampa_topo=False, tampa_fundo=False):
    """Perfil (z, raio) de cima para baixo, girado em volta do Z, com as faces
    para fora. ox/oy achatam a seção."""
    aneis = []
    for z, r in perfil:
        anel = []
        for i in range(seg):
            a = 2 * math.pi * i / seg
            anel.append(bm.verts.new((r * ox * math.cos(a), r * oy * math.sin(a), z)))
        aneis.append(anel)
    faces = []
    for a, b in zip(aneis, aneis[1:]):
        for i in range(seg):
            j = (i + 1) % seg
            faces.append(bm.faces.new((a[i], b[i], b[j], a[j])))
    if tampa_topo:
        faces.append(bm.faces.new(aneis[0]))
    if tampa_fundo:
        faces.append(bm.faces.new(list(reversed(aneis[-1]))))
    for f in faces:
        f.material_index = mat

def casco(bm, pontos, mat=0):
    """Casca convexa dos pontos, com as faces para fora."""
    vs = [bm.verts.new(p) for p in pontos]
    r = bmesh.ops.convex_hull(bm, input=vs)
    faces = [g for g in r['geom'] if isinstance(g, bmesh.types.BMFace)]
    bmesh.ops.recalc_face_normals(bm, faces=faces)
    for f in faces:
        f.material_index = mat

def extruda(bm, poligono_yz, espessura, mat=0):
    """Silhueta (y, z) convexa, com espessura em X."""
    e = espessura / 2
    casco(bm, [Vector((s * e, y, z)) for y, z in poligono_yz for s in (1, -1)], mat)

def caixa(bm, centro, tamanho, mat=0):
    cx, cy, cz = centro
    sx, sy, sz = (t / 2 for t in tamanho)
    casco(bm, [Vector((cx + a * sx, cy + b * sy, cz + c * sz))
               for a in (1, -1) for b in (1, -1) for c in (1, -1)], mat)

def objeto(nome, bm, mats, pai=None):
    me = bpy.data.meshes.new(nome)
    bm.to_mesh(me)
    bm.free()
    for m in mats:
        me.materials.append(m)
    for p in me.polygons:
        p.use_smooth = False
    ob = bpy.data.objects.new(nome, me)
    scene.collection.objects.link(ob)
    print('TRIS', nome, sum(len(p.vertices) - 2 for p in me.polygons))
    return ob

# ------------------------------------------------------------------ BALDE
ZINCO = material('Zinco', '#9EA8B3', 0.45)
ALCA = material('Alca', '#4D4F54', 0.5)
AREIA = material('Areia', '#D6B87A', 0.9)

BOCA_Z, ALTURA, R_BOCA, R_FUNDO, ESP = -0.15, 0.17, 0.11, 0.094, 0.006

bm = bmesh.new()
fundo_z = BOCA_Z - ALTURA
# parede de fora, com a borda enrolada; fundo fechado
torno(bm, [(BOCA_Z + 0.004, R_BOCA + 0.007), (BOCA_Z - 0.008, R_BOCA + 0.007),
           (BOCA_Z - 0.010, R_BOCA), (fundo_z, R_FUNDO)], tampa_fundo=True)
# borda de cima e parede de dentro: o balde vazio é oco
torno(bm, [(BOCA_Z + 0.004, R_BOCA + 0.007), (BOCA_Z + 0.004, R_BOCA - ESP)], mat=0)
perfil_dentro = [(fundo_z + 0.012, R_FUNDO - ESP), (BOCA_Z + 0.004, R_BOCA - ESP)]
seg = 14
aneis = []
for z, r in perfil_dentro:
    aneis.append([bm.verts.new((r * math.cos(2 * math.pi * i / seg), r * math.sin(2 * math.pi * i / seg), z))
                  for i in range(seg)])
for i in range(seg):
    j = (i + 1) % seg
    bm.faces.new((aneis[0][i], aneis[1][i], aneis[1][j], aneis[0][j]))
bm.faces.new(list(reversed(aneis[0]))[::-1])     # chão de dentro, virado para cima
# alça: arco fino, da borda até o meio, que é o pivô
PASSOS = 10
for i in range(PASSOS):
    t0, t1 = math.pi * i / PASSOS, math.pi * (i + 1) / PASSOS
    pts = []
    for t in (t0, t1):
        centro = Vector((0.112 * math.cos(t), 0.0, BOCA_Z + 0.15 * math.sin(t)))
        for a in (1, -1):
            for b in (1, -1):
                pts.append(centro + Vector((0.0035 * a, 0.006 * b, 0.0035 * a * b)))
    casco(bm, pts, 1)
balde = objeto('Balde', bm, [ZINCO, ALCA])

bm = bmesh.new()
torno(bm, [(BOCA_Z - 0.008, 0.025), (BOCA_Z - 0.014, 0.060), (BOCA_Z - 0.024, 0.090),
           (BOCA_Z - 0.032, R_BOCA - ESP - 0.001)], tampa_topo=True)
areia = objeto('Areia', bm, [AREIA])

# ------------------------------------------------------------------ TELA
MOLDURA = material('TelaMoldura', '#17805F', 0.55)
MALHA = material('TelaMalha', '#293635', 0.8)
MEIA, BARRA, ESPESSURA, TILE = 0.145, 0.017, 0.008, 0.05

bm = bmesh.new()
caixa(bm, (0, MEIA - BARRA / 2, 0), (MEIA * 2, BARRA, ESPESSURA))
caixa(bm, (0, -MEIA + BARRA / 2, 0), (MEIA * 2, BARRA, ESPESSURA))
caixa(bm, (MEIA - BARRA / 2, 0, 0), (BARRA, MEIA * 2 - BARRA * 2, ESPESSURA))
caixa(bm, (-MEIA + BARRA / 2, 0, 0), (BARRA, MEIA * 2 - BARRA * 2, ESPESSURA))
moldura = objeto('Moldura', bm, [MOLDURA])

bm = bmesh.new()
m = MEIA - BARRA
uv = bm.loops.layers.uv.new('UVMap')
face = bm.faces.new([bm.verts.new((x, y, 0)) for x, y in ((-m, -m), (m, -m), (m, m), (-m, m))])
for loop in face.loops:
    loop[uv].uv = ((loop.vert.co.x + m) / TILE, (loop.vert.co.y + m) / TILE)
malha = objeto('Malha', bm, [MALHA])

# ------------------------------------------------------------------ BORRIFADOR
FRASCO = material('Frasco', '#EDF0EB', 0.45)
ROTULO = material('Rotulo', '#1C54AD', 0.7)
FAIXA = material('Faixa', '#F5CC33', 0.7)
CABECA = material('Cabeca', '#17805F', 0.55)
GATILHO = material('Gatilho', '#0D4D3D', 0.65)

E = 0.74            # escala da prévia aprovada: ~20 cm de altura
ACHATADO = 0.78     # frasco oval, mais fino de frente

def esc(pares):
    return [(a * E, b * E) for a, b in pares]

bm = bmesh.new()
torno(bm, esc([(-0.006, 0.017), (-0.032, 0.019), (-0.044, 0.030), (-0.060, 0.044), (-0.075, 0.048),
               (-0.198, 0.046), (-0.205, 0.034)]), oy=ACHATADO, mat=0, tampa_fundo=True)
torno(bm, esc([(-0.095, 0.0505), (-0.175, 0.0495)]), oy=ACHATADO, mat=1)
torno(bm, esc([(-0.125, 0.0525), (-0.143, 0.052)]), oy=ACHATADO, mat=2)
torno(bm, esc([(0.012, 0.018), (0.012, 0.0245), (-0.012, 0.0245), (-0.012, 0.023)]), seg=10, mat=4,
      tampa_topo=True)
extruda(bm, esc([(-0.030, 0.012), (-0.038, 0.044), (-0.022, 0.064), (0.040, 0.066), (0.088, 0.058),
                 (0.094, 0.046), (0.090, 0.034), (0.040, 0.030), (0.022, 0.012)]), 0.036 * E, 3)
extruda(bm, esc([(0.040, 0.032), (0.056, 0.032), (0.058, 0.006), (0.050, -0.030), (0.040, -0.036),
                 (0.038, -0.006)]), 0.024 * E, 4)
# bico: tronco de cone deitado, na ponta da cabeça
bico = []
for y, r in ((0.092, 0.015), (0.108, 0.014), (0.110, 0.009), (0.116, 0.006)):
    for i in range(8):
        a = 2 * math.pi * i / 8
        bico.append(Vector((r * E * math.cos(a), y * E, (0.046 + r * math.sin(a)) * E)))
casco(bm, bico, 4)
borrifador = objeto('Borrifador', bm, [FRASCO, ROTULO, FAIXA, CABECA, GATILHO])

# ------------------------------------------------------------------ texturas
def salvar_png(caminho, largura, altura, rgba):
    """PNG RGBA de 8 bits, sem passar por conversão de cor do Blender."""
    linhas = b''.join(b'\0' + bytes(rgba[y * largura * 4:(y + 1) * largura * 4]) for y in range(altura))
    def bloco(tipo, dados):
        return (struct.pack('>I', len(dados)) + tipo + dados
                + struct.pack('>I', zlib.crc32(tipo + dados) & 0xffffffff))
    with open(caminho, 'wb') as f:
        f.write(b'\x89PNG\r\n\x1a\n')
        f.write(bloco(b'IHDR', struct.pack('>IIBBBBB', largura, altura, 8, 6, 0, 0, 0)))
        f.write(bloco(b'IDAT', zlib.compress(linhas, 9)))
        f.write(bloco(b'IEND', b''))
    print('PNG OK', caminho)

LADO, CELULA, FIO = 64, 8, 2
fio = bytes.fromhex('293635')
pix = bytearray()
for y in range(LADO):
    for x in range(LADO):
        tem_fio = x % CELULA < FIO or y % CELULA < FIO
        pix += fio + (b'\xff' if tem_fio else b'\x00')
salvar_png(os.path.join(OUT, 'Tela_Malha.png'), LADO, LADO, pix)

LADO = 32
pix = bytearray()
for y in range(LADO):
    for x in range(LADO):
        d = math.hypot(x + 0.5 - LADO / 2, y + 0.5 - LADO / 2) / (LADO / 2)
        a = max(0.0, min(1.0, (1.0 - d) * 2.0))
        pix += b'\xff\xff\xff' + bytes((int(a * a * (3 - 2 * a) * 255),))
salvar_png(os.path.join(OUT, 'Particula.png'), LADO, LADO, pix)

# ------------------------------------------------------------------ exporta FBX
def exportar(obs, nome):
    bpy.ops.object.select_all(action='DESELECT')
    for ob in obs:
        ob.select_set(True)
    bpy.context.view_layer.objects.active = obs[0]
    fbx = os.path.join(OUT, nome + '.fbx')
    bpy.ops.export_scene.fbx(
        filepath=fbx, use_selection=True, object_types={'MESH'},
        apply_unit_scale=True, apply_scale_options='FBX_SCALE_ALL',
        axis_forward='-Z', axis_up='Y', bake_space_transform=True,  # eixos já gravados na malha
        use_mesh_modifiers=True, mesh_smooth_type='FACE')
    print('FBX OK', fbx, os.path.getsize(fbx))

GRUPOS = {'Balde': [balde, areia], 'Tela': [moldura, malha], 'Borrifador': [borrifador]}
for nome, obs in GRUPOS.items():
    exportar(obs, nome)

# ------------------------------------------------------------------ prévias
if RENDER:
    world = bpy.data.worlds.new('W')
    scene.world = world
    world.use_nodes = True
    world.node_tree.nodes['Background'].inputs['Color'].default_value = (0.62, 0.66, 0.72, 1)
    world.node_tree.nodes['Background'].inputs['Strength'].default_value = 0.9
    luz = bpy.data.objects.new('Sol', bpy.data.lights.new('Sol', 'SUN'))
    luz.data.energy = 3.2
    luz.data.angle = math.radians(8)
    luz.rotation_euler = (math.radians(50), math.radians(-10), math.radians(-35))
    scene.collection.objects.link(luz)
    scene.render.engine = 'BLENDER_EEVEE'
    scene.render.resolution_x, scene.render.resolution_y = 700, 700
    scene.render.film_transparent = False
    try:
        scene.view_settings.view_transform = 'Standard'
    except Exception:
        pass
    cam = bpy.data.objects.new('Cam', bpy.data.cameras.new('Cam'))
    scene.collection.objects.link(cam)
    scene.camera = cam
    cam.data.lens = 50
    VISTAS = {'Balde': ((0.55, -0.75, 0.25), (0, 0, -0.16)),
              'Tela': ((0.35, -0.55, 0.45), (0, 0, 0)),
              'Borrifador': ((0.55, 0.55, 0.10), (0, 0.01, -0.05))}
    for nome, (posicao, alvo) in VISTAS.items():
        for g, obs in GRUPOS.items():
            for ob in obs:
                ob.hide_render = g != nome
        cam.location = posicao
        d = Vector(alvo) - cam.location
        cam.rotation_euler = d.to_track_quat('-Z', 'Y').to_euler()
        scene.render.filepath = os.path.join(OUT, 'previa_' + nome.lower() + '.png')
        bpy.ops.render.render(write_still=True)
        print('RENDER', nome)
