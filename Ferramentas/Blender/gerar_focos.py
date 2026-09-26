"""
Gera os focos de criadouro "Ralo" e "Bromélia" (low poly, estáticos) no
Blender e exporta FBX + prévias renderizadas.

Uso:
  blender -b --factory-startup --python gerar_focos.py -- <pasta_saida> [--sem-render]

Convenções:
  - Metros, Z pra cima, pivô na base (z = 0 = chão).
  - Cada foco é um objeto raiz (Empty) com filhos nomeados para o script do
    Unity achar: Ralo -> Caixa, Tampa, Agua, Tela; Bromelia -> Vaso, Folhas, Flor, Agua_Centro, Agua_1..4.
  - Ralo: estado inicial = grelha tirada e encostada ao lado, água à mostra.
  - Bromélia: estado inicial = água parada no copo central e nas axilas das folhas.
"""
import bpy, bmesh, math, sys, os
from mathutils import Vector, Matrix

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

PALETA = {
    'Concreto': '#9A968E', 'ConcretoEscuro': '#6E6A63', 'Fundo': '#2E2B27',
    'Grelha': '#4A4D52', 'Ferrugem': '#7A4A2A', 'Tela': '#2F6B3A',
    'Agua': '#3E7FA8',
    'Folha': '#3F8A3A', 'FolhaClara': '#78B84A', 'Ponta': '#C2412F', 'Flor': '#E0435A',
    'Terracota': '#B25228', 'Terra': '#4A3222',
}
MATS = {}
for nome, hx in PALETA.items():
    m = bpy.data.materials.new(nome)
    m.use_nodes = True
    bsdf = m.node_tree.nodes.get('Principled BSDF')
    bsdf.inputs['Base Color'].default_value = hexcol(hx)
    bsdf.inputs['Roughness'].default_value = 0.08 if nome == 'Agua' else (0.45 if nome == 'Grelha' else 0.8)
    if nome == 'Grelha':
        bsdf.inputs['Metallic'].default_value = 0.6
    m.diffuse_color = hexcol(hx)
    m.use_backface_culling = True  # igual ao URP
    MATS[nome] = m

def objeto(nome, bm, mats, pai=None):
    me = bpy.data.meshes.new(nome)
    bm.to_mesh(me)
    bm.free()
    for m in mats:
        me.materials.append(MATS[m])
    ob = bpy.data.objects.new(nome, me)
    scene.collection.objects.link(ob)
    if pai is not None:
        ob.parent = pai
    return ob

def raiz(nome, pos):
    e = bpy.data.objects.new(nome, None)
    e.location = pos
    scene.collection.objects.link(e)
    return e

def caixa(bm, cx, cy, cz, sx, sy, sz, mat=0):
    r = bmesh.ops.create_cube(bm, size=1.0)
    for v in r['verts']:
        v.co = Vector((cx + v.co.x * sx, cy + v.co.y * sy, cz + v.co.z * sz))
    for f in {f for v in r['verts'] for f in v.link_faces}:
        f.material_index = mat

# ================================================================== RALO
# Caixa de ralo de calçada: moldura de concreto 0,42 m, boca 0,26 m,
# fundo afundado 12 cm, com água parada a 3 cm abaixo da boca.
RALO_POS = (-0.45, 0.0, 0.0)
ralo = raiz('Ralo', RALO_POS)

EXT, BOCA, ALT, PROF = 0.21, 0.13, 0.03, 0.12
bm = bmesh.new()
# moldura (4 barras) — mat 0 concreto
caixa(bm, 0, (EXT + BOCA) / 2, ALT / 2, EXT * 2, EXT - BOCA, ALT)
caixa(bm, 0, -(EXT + BOCA) / 2, ALT / 2, EXT * 2, EXT - BOCA, ALT)
caixa(bm, (EXT + BOCA) / 2, 0, ALT / 2, EXT - BOCA, BOCA * 2, ALT)
caixa(bm, -(EXT + BOCA) / 2, 0, ALT / 2, EXT - BOCA, BOCA * 2, ALT)
# paredes internas do poço — mat 1 concreto escuro
esp = 0.012
for sx, sy, cx, cy in ((BOCA * 2, esp, 0, BOCA - esp / 2), (BOCA * 2, esp, 0, -BOCA + esp / 2),
                       (esp, BOCA * 2, BOCA - esp / 2, 0), (esp, BOCA * 2, -BOCA + esp / 2, 0)):
    caixa(bm, cx, cy, -PROF / 2, sx, sy, PROF, 1)
# fundo — mat 2
caixa(bm, 0, 0, -PROF - 0.005, BOCA * 2, BOCA * 2, 0.01, 2)
objeto('Caixa', bm, ['Concreto', 'ConcretoEscuro', 'Fundo'], ralo)

# água parada (lâmina)
bm = bmesh.new()
# 1 cm abaixo da borda: acima do chão, senão a calçada do jogo (sem buraco) esconde a água
caixa(bm, 0, 0, ALT - 0.01, BOCA * 2, BOCA * 2, 0.004)
agua_ralo = objeto('Agua', bm, ['Agua'], ralo)

# grelha (tampa): quadro + barras, com pivô no centro da face de baixo
G = BOCA + 0.02     # meia-largura, apoia na moldura
bm = bmesh.new()
H = 0.012
caixa(bm, 0, G - 0.012, H / 2, G * 2, 0.024, H)
caixa(bm, 0, -G + 0.012, H / 2, G * 2, 0.024, H)
caixa(bm, G - 0.012, 0, H / 2, 0.024, G * 2, H)
caixa(bm, -G + 0.012, 0, H / 2, 0.024, G * 2, H)
n = 7
for i in range(n):
    x = -G + 0.024 + (i + 0.5) * (2 * G - 0.048) / n
    caixa(bm, x, 0, H / 2, 0.012, G * 2 - 0.02, H, 1 if i % 3 == 1 else 0)
tampa = objeto('Tampa', bm, ['Grelha', 'Ferrugem'], ralo)
TAMPA_FECHADA = (Vector((0, 0, ALT)), (0.0, 0.0, 0.0))
TAMPA_ABERTA = (Vector((EXT + G - 0.02, -0.04, 0.045)), (0.0, math.radians(-18), math.radians(14)))

# tela fina de proteção (aparece quando o foco é resolvido): fica sob a grelha
bm = bmesh.new()
caixa(bm, 0, 0, ALT - 0.002, BOCA * 2 + 0.03, BOCA * 2 + 0.03, 0.003)
for i in range(-5, 6):
    caixa(bm, i * BOCA / 5.5, 0, ALT, 0.003, BOCA * 2 + 0.03, 0.002)
    caixa(bm, 0, i * BOCA / 5.5, ALT, BOCA * 2 + 0.03, 0.003, 0.002)
tela = objeto('Tela', bm, ['Tela'], ralo)

def ralo_estado(fechado):
    pos, rot = TAMPA_FECHADA if fechado else TAMPA_ABERTA
    tampa.location = pos + (Vector((0, 0, 0.004)) if fechado else Vector())
    tampa.rotation_euler = rot
    agua_ralo.hide_render = fechado
    tela.hide_render = not fechado

# ================================================================== BROMÉLIA
BROM_POS = (0.45, 0.0, 0.0)
brom = raiz('Bromelia', BROM_POS)

# vaso de terracota (tronco de cone) + terra
VR0, VR1, VH = 0.10, 0.14, 0.16
bm = bmesh.new()
seg = 14
anel = []
for z, r in ((0.0, VR0), (VH, VR1), (VH, VR1 - 0.015), (VH - 0.02, VR1 - 0.02)):
    anel.append([bm.verts.new((r * math.cos(2 * math.pi * i / seg), r * math.sin(2 * math.pi * i / seg), z))
                 for i in range(seg)])
for a, b in zip(anel, anel[1:]):
    for i in range(seg):
        j = (i + 1) % seg
        bm.faces.new((a[i], a[j], b[j], b[i]))
f = bm.faces.new(list(reversed(anel[0])))
t = bm.faces.new(anel[-1])
t.material_index = 1
bmesh.ops.recalc_face_normals(bm, faces=bm.faces[:])
objeto('Vaso', bm, ['Terracota', 'Terra'], brom)

BASE_Z = VH - 0.02

def folha(bm, ang, comp, larg, sobe, inclina, mat_ponta=True):
    """Folha larga em calha (seção em V), arqueando pra fora e caindo na ponta."""
    d = Vector((math.cos(ang), math.sin(ang), 0))
    lado = Vector((-d.y, d.x, 0))
    N = 8
    linhas = []
    for k in range(N + 1):
        t = k / N
        r = 0.015 + comp * t
        z = BASE_Z + sobe * math.sin(t * math.pi * 0.55) - inclina * t * t
        w = larg * (math.sin(math.pi * min(1.0, 0.15 + t * 0.95)) ** 0.7) * (1 - t * 0.85) + 0.004
        centro = d * r + Vector((0, 0, z))
        calha = w * 0.45
        linhas.append([bm.verts.new(centro - lado * w + Vector((0, 0, calha))),
                       bm.verts.new(centro),
                       bm.verts.new(centro + lado * w + Vector((0, 0, calha)))])
    for k in range(N):
        a, b = linhas[k], linhas[k + 1]
        for i in range(2):
            fc = bm.faces.new((a[i], a[i + 1], b[i + 1], b[i]))
            fc.material_index = 2 if (mat_ponta and k >= N - 2) else (1 if k == 0 else 0)

bm = bmesh.new()
aneis = [  # (quantidade, comprimento, largura, subida, queda, giro)
    (7, 0.34, 0.050, 0.10, 0.12, 0.0),
    (6, 0.27, 0.045, 0.16, 0.06, 0.45),
    (5, 0.18, 0.038, 0.20, 0.00, 0.9),
]
for qtd, comp, larg, sobe, queda, giro in aneis:
    for i in range(qtd):
        folha(bm, giro + 2 * math.pi * i / qtd, comp, larg, sobe, queda)
folhas_me = objeto('Folhas', bm, ['Folha', 'FolhaClara', 'Ponta'], brom)
sol = folhas_me.modifiers.new('Espessura', 'SOLIDIFY')
sol.thickness = 0.005
sol.offset = 0
bpy.context.view_layer.objects.active = folhas_me
folhas_me.select_set(True)
bpy.ops.object.modifier_apply(modifier='Espessura')
folhas_me.select_set(False)

# flor central (espiga) pra ficar reconhecível
bm = bmesh.new()
bmesh.ops.create_cone(bm, cap_ends=True, segments=8, radius1=0.018, radius2=0.006, depth=0.11,
                      matrix=Matrix.Translation((0, 0, BASE_Z + 0.12)))
objeto('Flor', bm, ['Flor'], brom)

# água: copo central + pocinhas nas axilas do anel de dentro (cada uma um filho,
# para o Unity esvaziar uma por clique). Ficam direto na raiz: dentro de um Empty
# o export com bake_space_transform gira os filhos 90°. Pivô no centro da poça,
# para ela encolher no lugar ao secar.
pocas_brom = []
def poca(nome, x, y, z, r):
    bm = bmesh.new()
    bmesh.ops.create_circle(bm, cap_ends=True, segments=10, radius=r)
    ob = objeto(nome, bm, ['Agua'], brom)
    ob.location = (x, y, z)
    pocas_brom.append(ob)
poca('Agua_Centro', 0, 0, BASE_Z + 0.075, 0.032)
for i in range(4):
    a = 0.9 + 2 * math.pi * i / 5 + 0.2
    poca('Agua_%d' % (i + 1), math.cos(a) * 0.055, math.sin(a) * 0.055, BASE_Z + 0.05, 0.018)

def brom_estado(seca):
    for ob in pocas_brom:
        ob.hide_render = seca

# ------------------------------------------------------------------ exporta FBX
def exportar(raiz_ob, nome):
    bpy.ops.object.select_all(action='DESELECT')
    old = raiz_ob.location.copy()
    raiz_ob.location = (0, 0, 0)
    raiz_ob.select_set(True)
    for c in raiz_ob.children_recursive:
        c.select_set(True)
    bpy.context.view_layer.update()
    fbx = os.path.join(OUT, nome + '.fbx')
    bpy.ops.export_scene.fbx(
        filepath=fbx, use_selection=True, object_types={'EMPTY', 'MESH'},
        apply_unit_scale=True, apply_scale_options='FBX_SCALE_ALL',
        axis_forward='-Z', axis_up='Y', bake_space_transform=True,  # eixos já gravados na malha: o Unity funde o nó raiz
        use_mesh_modifiers=True, mesh_smooth_type='FACE')
    raiz_ob.location = old
    print('FBX OK', fbx, os.path.getsize(fbx))

ralo_estado(False)
brom_estado(False)
exportar(ralo, 'Ralo')
exportar(brom, 'Bromelia')
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT, 'Focos.blend'))

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

    # calçada com buraco onde está o ralo
    bm = bmesh.new()
    x0, x1 = RALO_POS[0] - EXT, RALO_POS[0] + EXT
    y0, y1 = -EXT, EXT
    S = 2.0
    for (a, b, c, d) in (((-S, -S), (S, -S), (S, y0), (-S, y0)), ((-S, y1), (S, y1), (S, S), (-S, S)),
                         ((-S, y0), (x0, y0), (x0, y1), (-S, y1)), ((x1, y0), (S, y0), (S, y1), (x1, y1))):
        bm.faces.new([bm.verts.new((p[0], p[1], 0)) for p in (a, b, c, d)])
    chao = objeto('Chao', bm, ['Concreto'])
    chao.data.materials[0] = bpy.data.materials.new('ChaoMat')
    chao.data.materials[0].use_nodes = True
    chao.data.materials[0].node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value = hexcol('#B3AFA6')

    for eng in ('BLENDER_EEVEE', 'BLENDER_EEVEE_NEXT'):
        try:
            scene.render.engine = eng
            break
        except Exception:
            pass
    scene.render.resolution_x, scene.render.resolution_y = 900, 700
    try:
        scene.view_settings.view_transform = 'Standard'
    except Exception:
        pass

    cam = bpy.data.objects.new('Cam', bpy.data.cameras.new('Cam'))
    scene.collection.objects.link(cam)
    scene.camera = cam

    def olhar(pos, alvo, lente=50):
        cam.location = pos
        d = Vector(alvo) - Vector(pos)
        cam.rotation_euler = d.to_track_quat('-Z', 'Y').to_euler()
        cam.data.lens = lente

    def render(nome):
        scene.render.filepath = os.path.join(OUT, nome + '.png')
        bpy.ops.render.render(write_still=True)
        print('RENDER', nome)

    rp = Vector(RALO_POS)
    bp = Vector(BROM_POS)
    # visão de quem está em pé (olho ~1,6 m), como o jogador vê
    olhar(rp + Vector((0.55, -0.95, 1.05)), rp + Vector((0.05, 0, 0)), 45)
    ralo_estado(False); render('previa_ralo')
    ralo_estado(True); render('previa_ralo_fechado')
    olhar(bp + Vector((0.55, -0.9, 0.85)), bp + Vector((0, 0, 0.18)), 45)
    brom_estado(False); render('previa_bromelia')
    brom_estado(True); render('previa_bromelia_seca')
    olhar(bp + Vector((0.0, -0.25, 0.75)), bp + Vector((0, 0, 0.15)), 40)
    brom_estado(False); render('previa_bromelia_cima')
    ralo_estado(False)
    olhar(Vector((0, -2.1, 1.3)), Vector((0, 0, 0.1)), 40)
    render('previa_juntos')
