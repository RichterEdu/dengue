"""
Gera os focos de criadouro "Lona" e "Pote de Bicho" (low poly, estáticos)
no Blender e exporta FBX + prévias renderizadas.
Mesmas convenções de Ferramentas/Blender/gerar_focos.py: metros, Z pra cima,
pivô na base, partes mexidas pelo Unity como filhos diretos da raiz.

  Lona        -> Esticada, Dobrada, Tijolos, Poca_1..4
  PoteDeBicho -> Pote, Agua_Velha, Sujeira, Agua_Limpa

Uso: blender -b --factory-startup --python gerar_lona_pote.py -- <pasta_saida> [--sem-render]

Depois: copiar Lona.fbx e PoteDeBicho.fbx para Assets/Meus/Assets Gerados/Focos
e rodar Dengue > Criar Lona / Dengue > Criar Pote de Bicho (FocosBlenderBuilder.cs).
As cores da PALETA são as mesmas da Paleta do FocosBlenderBuilder.
"""
import bpy, bmesh, math, sys, os
from mathutils import Vector, Matrix

ARGS = sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else []
OUT = os.path.abspath(ARGS[0]) if ARGS else os.path.abspath('.')
RENDER = '--sem-render' not in ARGS
os.makedirs(OUT, exist_ok=True)

bpy.ops.wm.read_factory_settings(use_empty=True)
scene = bpy.context.scene

def _lin(c):
    return c / 12.92 if c <= 0.04045 else ((c + 0.055) / 1.055) ** 2.4

def hexcol(h):
    h = h.lstrip('#')
    return tuple(_lin(int(h[i:i + 2], 16) / 255) for i in (0, 2, 4)) + (1.0,)

PALETA = {
    'Lona': '#2F6DB3', 'LonaDobra': '#25599A', 'Tijolo': '#B5562F',
    'Agua': '#3E7FA8', 'AguaVelha': '#5F7A3A', 'Sujeira': '#3B3A22',
    'Pote': '#D9472B', 'PoteBase': '#B33A22',
}
MATS = {}
for nome, hx in PALETA.items():
    m = bpy.data.materials.new(nome)
    m.use_nodes = True
    bsdf = m.node_tree.nodes.get('Principled BSDF')
    bsdf.inputs['Base Color'].default_value = hexcol(hx)
    agua = nome in ('Agua', 'AguaVelha')
    bsdf.inputs['Roughness'].default_value = 0.08 if agua else (0.35 if nome.startswith('Pote') else 0.7)
    m.diffuse_color = hexcol(hx)
    m.use_backface_culling = True
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

def caixa(bm, cx, cy, cz, sx, sy, sz, mat=0, giro=0.0):
    r = bmesh.ops.create_cube(bm, size=1.0)
    c, s = math.cos(giro), math.sin(giro)
    for v in r['verts']:
        x, y = v.co.x * sx, v.co.y * sy
        v.co = Vector((cx + x * c - y * s, cy + x * s + y * c, cz + v.co.z * sz))
    for f in {f for v in r['verts'] for f in v.link_faces}:
        f.material_index = mat

def solidificar(ob, esp):
    mod = ob.modifiers.new('Espessura', 'SOLIDIFY')
    mod.thickness = esp
    mod.offset = 0
    bpy.context.view_layer.objects.active = ob
    ob.select_set(True)
    bpy.ops.object.modifier_apply(modifier='Espessura')
    ob.select_set(False)

def disco(nome, pai, pos, raios, mat):
    """Polígono plano (leque) com pivô no centro; raios = lista por ângulo."""
    bm = bmesh.new()
    c = bm.verts.new((0, 0, 0))
    n = len(raios)
    borda = [bm.verts.new((r * math.cos(2 * math.pi * i / n), r * math.sin(2 * math.pi * i / n), 0))
             for i, r in enumerate(raios)]
    for i in range(n):
        bm.faces.new((c, borda[i], borda[(i + 1) % n]))
    ob = objeto(nome, bm, [mat], pai)
    ob.location = pos
    return ob

# ================================================================== LONA
# Lona azul de 2,0 x 1,4 m largada no chão, presa por tijolos, com entulho
# por baixo (calombos) e quatro bacias onde a chuva empoça.
LONA_POS = (-1.4, 0.0, 0.0)
lona = raiz('Lona', LONA_POS)
LX, LY = 1.0, 0.7
BASE = 0.075
CALOMBOS = [(-0.55, 0.38, 0.22, 0.12), (0.05, -0.05, 0.20, 0.10), (0.68, 0.32, 0.20, 0.13),
            (0.75, -0.42, 0.18, 0.09), (-0.80, -0.45, 0.18, 0.09), (-0.05, 0.55, 0.16, 0.07)]
POCAS = [(-0.40, -0.20, 0.19, 0.065), (0.42, -0.08, 0.21, 0.068),
         (-0.02, -0.44, 0.13, 0.055), (0.22, 0.40, 0.14, 0.055)]

def g(x, y, cx, cy, r):
    return math.exp(-((x - cx) ** 2 + (y - cy) ** 2) / (r * r))

def altura(x, y):
    perto = sum(g(x, y, px, py, pr * 1.6) for px, py, pr, _ in POCAS)
    onda = 0.010 * math.sin(5.0 * x + 1.3 * y) * math.cos(3.4 * y - 0.8 * x) * max(0.0, 1 - perto)
    h = BASE + onda
    h += sum(a * g(x, y, cx, cy, r) for cx, cy, r, a in CALOMBOS)
    h -= sum(d * g(x, y, px, py, pr) for px, py, pr, d in POCAS)
    # as bordas assentam no chão (é onde os tijolos prendem)
    borda = min(LX - abs(x), LY - abs(y))
    t = min(1.0, max(0.0, borda / 0.18))
    h = 0.006 + (h - 0.006) * t * t * (3 - 2 * t)
    # borda levantada num canto (dobra da lona)
    if x > 0.75 and y > 0.45:
        h += 0.10 * ((x - 0.75) / 0.25) * ((y - 0.45) / 0.25)
    return max(0.006, h)

NX, NY = 30, 21
bm = bmesh.new()
grade = [[bm.verts.new((-LX + 2 * LX * i / NX, -LY + 2 * LY * j / NY, 0)) for i in range(NX + 1)]
         for j in range(NY + 1)]
for linha in grade:
    for v in linha:
        v.co.z = altura(v.co.x, v.co.y)
for j in range(NY):
    for i in range(NX):
        f = bm.faces.new((grade[j][i], grade[j][i + 1], grade[j + 1][i + 1], grade[j + 1][i]))
        f.material_index = 1 if (i in (0, NX - 1) or j in (0, NY - 1)) else 0
esticada = objeto('Esticada', bm, ['Lona', 'LonaDobra'], lona)
solidificar(esticada, 0.004)

# poças: nível = fundo da bacia + metade da profundidade; contorno achado
# andando em cada direção até a lona passar do nível.
pocas_lona = []
for k, (px, py, pr, d) in enumerate(POCAS):
    nivel = altura(px, py) + d * 0.42
    raios = []
    for i in range(20):
        a = 2 * math.pi * i / 20
        r = 0.0
        while r < pr * 1.3 and altura(px + math.cos(a) * r, py + math.sin(a) * r) < nivel:
            r += 0.005
        raios.append(max(0.02, r * 0.97))
    pocas_lona.append(disco('Poca_%d' % (k + 1), lona, (px, py, nivel + 0.004), raios, 'Agua'))

# tijolos segurando as pontas (ficam na cena depois de dobrar)
bm = bmesh.new()
for x, y, giro in ((-0.92, 0.62, 0.3), (0.90, -0.62, -0.2), (-0.90, -0.60, 1.2)):
    caixa(bm, x, y, 0.0285 + 0.008, 0.19, 0.09, 0.057, 0, giro)
objeto('Tijolos', bm, ['Tijolo'], lona)

# lona dobrada (estado concluído): pilha de camadas, encostada perto dos tijolos
bm = bmesh.new()
for k in range(4):
    caixa(bm, -0.55 + 0.01 * k, 0.15 - 0.008 * k, 0.012 + 0.022 * k, 0.56 - 0.02 * k, 0.38, 0.02,
          k % 2, 0.03 * (k - 1.5))
# vinco arredondado de um lado
bmesh.ops.create_cone(bm, cap_ends=True, segments=8, radius1=0.045, radius2=0.045, depth=0.36,
                      matrix=Matrix.Translation((-0.83, 0.13, 0.045)) @ Matrix.Rotation(math.pi / 2, 4, 'X'))
dobrada = objeto('Dobrada', bm, ['Lona', 'LonaDobra'], lona)

def lona_estado(estado):  # 'agua', 'seca', 'dobrada'
    for p in pocas_lona:
        p.hide_render = estado != 'agua'
    esticada.hide_render = estado == 'dobrada'
    dobrada.hide_render = estado != 'dobrada'

# ================================================================== POTE DE BICHO
# Vasilha de cachorro de base larga (anti-tombo), com água velha esverdeada
# e sujeira boiando. Concluído: lavado e com água limpa.
POTE_POS = (1.0, 0.0, 0.0)
pote = raiz('PoteDeBicho', POTE_POS)

def torno(perfil, seg, mats_por_trecho):
    """Revolução de um perfil (r, z) do centro de baixo até o centro de dentro."""
    bm = bmesh.new()
    aneis = []
    for r, z in perfil:
        if r == 0:
            aneis.append([bm.verts.new((0, 0, z))])
        else:
            aneis.append([bm.verts.new((r * math.cos(2 * math.pi * i / seg), r * math.sin(2 * math.pi * i / seg), z))
                          for i in range(seg)])
    for k, (a, b) in enumerate(zip(aneis, aneis[1:])):
        for i in range(seg):
            j = (i + 1) % seg
            if len(a) == 1:
                f = bm.faces.new((a[0], b[j], b[i]))
            elif len(b) == 1:
                f = bm.faces.new((a[i], a[j], b[0]))
            else:
                f = bm.faces.new((a[i], a[j], b[j], b[i]))
            f.material_index = mats_por_trecho[k]
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces[:])
    return bm

PERFIL = [(0, 0.0), (0.145, 0.0), (0.150, 0.012), (0.128, 0.070), (0.122, 0.080),
          (0.108, 0.080), (0.088, 0.020), (0, 0.020)]
TRECHOS = [1, 1, 0, 0, 0, 0, 0]
objeto('Pote', torno(PERFIL, 24, TRECHOS), ['Pote', 'PoteBase'], pote)

def raio_dentro(z):  # parede interna entre (0.088, 0.020) e (0.108, 0.080)
    return 0.088 + (z - 0.020) * (0.020 / 0.060)

NIVEL = 0.062
agua_velha = disco('Agua_Velha', pote, (0, 0, NIVEL), [raio_dentro(NIVEL) * 0.985] * 20, 'AguaVelha')
agua_limpa = disco('Agua_Limpa', pote, (0, 0, NIVEL + 0.004), [raio_dentro(NIVEL + 0.004) * 0.985] * 20, 'Agua')

# sujeira boiando: folhinhas e bolinhas de ração velha
bm = bmesh.new()
for x, y, giro, s in ((0.03, 0.02, 0.4, 1.0), (-0.04, 0.035, 1.9, 0.8), (0.01, -0.05, 2.7, 0.9)):
    caixa(bm, x, y, 0.0015, 0.026 * s, 0.012 * s, 0.002, 0, giro)
for x, y in ((-0.03, -0.02), (0.05, -0.01), (-0.06, -0.04), (0.035, 0.055)):
    caixa(bm, x, y, 0.002, 0.009, 0.009, 0.005, 0, x * 40)
sujeira = objeto('Sujeira', bm, ['Sujeira'], pote)
sujeira.location = (0, 0, NIVEL)

def pote_estado(limpo):
    agua_velha.hide_render = limpo
    sujeira.hide_render = limpo
    agua_limpa.hide_render = not limpo

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
        axis_forward='-Z', axis_up='Y', bake_space_transform=True,
        use_mesh_modifiers=True, mesh_smooth_type='FACE')
    raiz_ob.location = old
    print('FBX OK', fbx, os.path.getsize(fbx))

lona_estado('agua')
pote_estado(False)
exportar(lona, 'Lona')
exportar(pote, 'PoteDeBicho')
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT, 'FocosNovos.blend'))

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

    def chao(nome, x0, x1, cor):
        bm = bmesh.new()
        bm.faces.new([bm.verts.new(p) for p in ((x0, -2.5, 0), (x1, -2.5, 0), (x1, 2.5, 0), (x0, 2.5, 0))])
        ob = objeto(nome, bm, ['Agua'])
        mat = bpy.data.materials.new(nome + 'Mat')
        mat.use_nodes = True
        mat.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value = hexcol(cor)
        mat.node_tree.nodes['Principled BSDF'].inputs['Roughness'].default_value = 0.95
        ob.data.materials[0] = mat
    chao('ChaoTerra', -4.0, 0.3, '#7A6A4E')      # terreno de quintal sob a lona
    chao('ChaoCimento', 0.3, 3.0, '#B3AFA6')     # cimentado sob o pote

    scene.render.engine = 'BLENDER_EEVEE'
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
        cam.rotation_euler = (Vector(alvo) - Vector(pos)).to_track_quat('-Z', 'Y').to_euler()
        cam.data.lens = lente

    def render(nome):
        scene.render.filepath = os.path.join(OUT, nome + '.png')
        bpy.ops.render.render(write_still=True)
        print('RENDER', nome)

    lp, pp = Vector(LONA_POS), Vector(POTE_POS)
    olhar(lp + Vector((0.9, -2.0, 1.6)), lp + Vector((0, 0, 0.05)), 35)   # olho do jogador
    lona_estado('agua'); render('previa_lona')
    lona_estado('seca'); render('previa_lona_seca')
    lona_estado('dobrada'); render('previa_lona_dobrada')
    olhar(pp + Vector((0.35, -0.75, 0.9)), pp + Vector((0, 0, 0.05)), 45)
    pote_estado(False); render('previa_pote')
    pote_estado(True); render('previa_pote_limpo')
    olhar(pp + Vector((0.0, -0.18, 0.55)), pp + Vector((0, 0, 0.05)), 45)
    pote_estado(False); render('previa_pote_cima')
    lona_estado('agua'); pote_estado(False)
    olhar(Vector((0.0, -3.6, 2.0)), Vector((-0.3, 0, 0.1)), 32)
    render('previa_juntos')
