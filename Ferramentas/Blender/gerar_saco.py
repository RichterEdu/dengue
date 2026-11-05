"""
Gera o "Saco" de lixo preto (low poly, estático) que o agente de saúde leva
na mão direita com as garrafas recolhidas (Parte B), e exporta FBX + prévia.

Uso:
  blender -b --factory-startup --python gerar_saco.py -- <pasta_saida> [--sem-render]

Convenções:
  - Metros, Z pra cima. O pivô (0, 0, 0) é a BOCA do saco, amarrada: o saco
    pende para baixo dele (~0,35 m até o fundo) e as duas pontas do nó sobem
    uns 4 cm, para dentro da mão. No Unity ele fica filho do osso RightHand
    (PoseDeCarga.cs), com a escala do FeedbackDeCarga (0,5 quase vazio a 1,0
    cheio).
  - Um objeto só ("Saco"), material "SacoPlastico" (plástico preto, um pouco
    brilhante), faces chapadas: os vincos do plástico viram facetas.
  - Saída: <pasta_saida>/Saco.fbx (copiar para Assets/Meus/Assets Gerados/Saco/).
"""
import bpy, bmesh, math, sys, os
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

mat = bpy.data.materials.new('SacoPlastico')
mat.use_nodes = True
bsdf = mat.node_tree.nodes.get('Principled BSDF')
bsdf.inputs['Base Color'].default_value = hexcol('#161618')
bsdf.inputs['Roughness'].default_value = 0.32
mat.diffuse_color = hexcol('#161618')
mat.use_backface_culling = True  # igual ao URP

# ------------------------------------------------------------------ corpo
# Perfil de cima (boca) para baixo: (z, raio). O saco é um pouco achatado na
# frente-trás (ACHATADO) e os vincos (VINCOS dobras em volta) são mais fundos
# perto do gargalo, onde o plástico foi franzido pelo nó.
PERFIL = [
    (-0.010, 0.020), (-0.040, 0.026), (-0.075, 0.034), (-0.110, 0.070),
    (-0.160, 0.112), (-0.215, 0.132), (-0.270, 0.134), (-0.315, 0.118),
    (-0.340, 0.082), (-0.352, 0.035),
]
SEGMENTOS = 12
ACHATADO = 0.82
VINCOS = 6

def vinco(z, ang):
    """Fator do raio: dobras fundas no gargalo, quase lisas no fundo."""
    fundo = max(0.0, min(1.0, (-z - 0.04) / 0.20))
    amplitude = 0.22 * (1.0 - fundo) + 0.05
    # o mesmo padrão nunca se repete inteiro: fica com cara de plástico amassado
    return 1.0 + amplitude * math.cos(VINCOS * ang + 1.7 * z * 10.0) * (0.6 + 0.4 * math.sin(3 * ang + 1.0))

bm = bmesh.new()
aneis = []
for z, r in PERFIL:
    anel = []
    for i in range(SEGMENTOS):
        ang = 2 * math.pi * i / SEGMENTOS
        rr = r * vinco(z, ang)
        anel.append(bm.verts.new((rr * math.cos(ang), rr * math.sin(ang) * ACHATADO, z)))
    aneis.append(anel)
for a, b in zip(aneis, aneis[1:]):
    for i in range(SEGMENTOS):
        j = (i + 1) % SEGMENTOS
        bm.faces.new((a[i], a[j], b[j], b[i]))
bm.faces.new(list(reversed(aneis[0])))   # tampa da boca (dentro do nó, não aparece)
bm.faces.new(aneis[-1])                  # fundo

# ------------------------------------------------------------------ nó
# Bolinha do nó em volta da boca e as duas pontas ("orelhas") do plástico.
r = bmesh.ops.create_icosphere(bm, subdivisions=1, radius=0.028)
for v in r['verts']:
    v.co = Vector((v.co.x, v.co.y * 0.85, v.co.z * 0.8 - 0.012))
for lado in (1, -1):
    # cada orelha é uma cunha fina (casca convexa de 8 pontos)
    base = Vector((lado * 0.010, 0.0, 0.004))
    ponta = Vector((lado * 0.032, lado * 0.006, 0.042))
    largura = Vector((0.0, 0.016, 0.0))
    espessura = Vector((0.003, 0.0, 0.0))
    pts = [base - largura, base + largura, ponta + largura * 0.4, ponta - largura * 0.4]
    vs = [bm.verts.new(p + e) for p in pts for e in (espessura, -espessura)]
    bmesh.ops.convex_hull(bm, input=vs)

bmesh.ops.recalc_face_normals(bm, faces=bm.faces[:])
me = bpy.data.meshes.new('Saco')
bm.to_mesh(me)
bm.free()
me.materials.append(mat)
for p in me.polygons:
    p.use_smooth = False
saco = bpy.data.objects.new('Saco', me)
scene.collection.objects.link(saco)
print('TRIS Saco', sum(len(p.vertices) - 2 for p in me.polygons))

# ------------------------------------------------------------------ exporta FBX
bpy.ops.object.select_all(action='DESELECT')
saco.select_set(True)
bpy.context.view_layer.objects.active = saco
fbx = os.path.join(OUT, 'Saco.fbx')
bpy.ops.export_scene.fbx(
    filepath=fbx, use_selection=True, object_types={'MESH'},
    apply_unit_scale=True, apply_scale_options='FBX_SCALE_ALL',
    axis_forward='-Z', axis_up='Y', bake_space_transform=True,  # eixos já gravados na malha
    use_mesh_modifiers=True, mesh_smooth_type='FACE')
print('FBX OK', fbx, os.path.getsize(fbx))

# ------------------------------------------------------------------ prévia
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
    scene.render.resolution_x, scene.render.resolution_y = 700, 900
    try:
        scene.view_settings.view_transform = 'Standard'
    except Exception:
        pass
    cam = bpy.data.objects.new('Cam', bpy.data.cameras.new('Cam'))
    scene.collection.objects.link(cam)
    scene.camera = cam
    cam.location = (0.45, -0.75, 0.05)
    d = Vector((0, 0, -0.16)) - cam.location
    cam.rotation_euler = d.to_track_quat('-Z', 'Y').to_euler()
    cam.data.lens = 50
    scene.render.filepath = os.path.join(OUT, 'previa_saco.png')
    bpy.ops.render.render(write_still=True)
    print('RENDER previa_saco')
