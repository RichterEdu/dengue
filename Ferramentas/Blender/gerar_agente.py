"""
Gera o personagem "Agente de Saúde" (low poly, rigado e animado) no Blender
e exporta FBX + prévias renderizadas.

Uso:
  blender -b --factory-startup --python gerar_agente.py -- <pasta_saida> [--sem-render]

Convenções:
  - Metros, Z pra cima, personagem olhando pra -Y (esquerda do personagem = +X).
  - Pés no chão em z = 0, altura total ~1,78 m com o boné.
  - Esqueleto Generic com nomes compatíveis com Humanoid do Unity.
  - Animações in-place: Idle, Walk (16 f), Run (15 f), Air (20 f) a 30 fps.
  - Carregando (Parte B): pose estática (1 f) com o pneu de 0,7 m nos braços,
    usada pela camada "Carregando" do Animator (só tronco, braços e mãos).
  - CarregandoSaco (Parte B): pose estática (1 f) com o braço direito erguido à
    frente, usada pela camada "CarregandoSaco" do Animator (só o braço direito).
"""
import bpy, bmesh, math, sys, os
from mathutils import Vector, Matrix, Quaternion

ARGS = sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else []
OUT = os.path.abspath(ARGS[0]) if ARGS else os.path.abspath('.')
RENDER = '--sem-render' not in ARGS
os.makedirs(OUT, exist_ok=True)

bpy.ops.wm.read_factory_settings(use_empty=True)
scene = bpy.context.scene
scene.render.fps = 30

# ------------------------------------------------------------------ cores
def _lin(c):
    return c / 12.92 if c <= 0.04045 else ((c + 0.055) / 1.055) ** 2.4

def hexcol(h):
    h = h.lstrip('#')
    return tuple(_lin(int(h[i:i + 2], 16) / 255) for i in (0, 2, 4)) + (1.0,)

PALETA = {
    'Pele': '#B07A55', 'Boca': '#7A4434', 'Olhos': '#1A1A1A', 'Cabelo': '#2B1D14',
    'Camisa': '#F2F2EE', 'Colete': '#1F5FBF', 'ColeteEscuro': '#174A96', 'Refletivo': '#D8E04A',
    'Calca': '#2E3F5C', 'Tenis': '#34343A', 'Sola': '#EDEDED',
    'Bone': '#1F5FBF', 'Logo': '#FFFFFF',
    'Bolsa': '#9C7B55', 'BolsaEscuro': '#6E5238', 'Alca': '#35353A', 'Metal': '#B8BCC4',
    'Cracha': '#FFFFFF', 'Texto': '#FFFFFF',
}
MATS = {}
for nome, hx in PALETA.items():
    m = bpy.data.materials.new(nome)
    m.use_nodes = True
    bsdf = m.node_tree.nodes.get('Principled BSDF')
    bsdf.inputs['Base Color'].default_value = hexcol(hx)
    bsdf.inputs['Roughness'].default_value = 0.35 if nome == 'Refletivo' else 0.75
    m.diffuse_color = hexcol(hx)
    m.use_backface_culling = True  # igual ao URP: face invertida vira buraco na prévia
    MATS[nome] = m

# ------------------------------------------------------------------ perfil do tronco
# (z, meia-largura X, meia-profundidade Y, deslocamento Y do centro)
TORSO = [
    (0.86, 0.165, 0.108, 0.000), (0.92, 0.185, 0.118, 0.000), (0.98, 0.180, 0.114, 0.000),
    (1.05, 0.158, 0.105, 0.000), (1.13, 0.165, 0.110, -0.003), (1.22, 0.180, 0.118, -0.008),
    (1.31, 0.192, 0.118, -0.006), (1.38, 0.200, 0.106, 0.000), (1.43, 0.155, 0.086, 0.000),
    (1.46, 0.075, 0.060, 0.000),
]

def perfil(z):
    if z <= TORSO[0][0]:
        return TORSO[0][1:]
    for (z0, a0, b0, y0), (z1, a1, b1, y1) in zip(TORSO, TORSO[1:]):
        if z <= z1:
            t = (z - z0) / (z1 - z0)
            return (a0 + (a1 - a0) * t, b0 + (b1 - b0) * t, y0 + (y1 - y0) * t)
    return TORSO[-1][1:]

COLETE_OFF = 0.016

def sup_y(x, z, lado, off=0.0):
    """Y da superfície do tronco (+off) em (x, z). lado=-1 frente, +1 costas."""
    a, b, y0 = perfil(z)
    a += off; b += off
    q = max(0.0, 1.0 - (x / a) ** 2)
    return y0 + lado * b * math.sqrt(q)

# ------------------------------------------------------------------ pesos
CADEIA_J = [1.05, 1.20, 1.42, 1.52]
CADEIA_B = ['Hips', 'Spine', 'Chest', 'Neck', 'Head']

def smooth(t):
    t = min(1.0, max(0.0, t))
    return t * t * (3 - 2 * t)

def pesos_cadeia(z, w=0.04):
    for i, zj in enumerate(CADEIA_J):
        if z < zj - w:
            return {CADEIA_B[i]: 1.0}
        if z <= zj + w:
            t = smooth((z - (zj - w)) / (2 * w))
            return {CADEIA_B[i]: 1 - t, CADEIA_B[i + 1]: t}
    return {CADEIA_B[-1]: 1.0}

def pesos_membro(p, junta, dir_a, dir_b, osso_a, osso_b, w=0.035):
    n = (dir_a.normalized() + dir_b.normalized()).normalized()
    s = (p - junta).dot(n)
    t = smooth((s + w) / (2 * w))
    if t <= 0.0:
        return {osso_a: 1.0}
    if t >= 1.0:
        return {osso_b: 1.0}
    return {osso_a: 1 - t, osso_b: t}

# ------------------------------------------------------------------ construção de peças
PECAS = []

def finalizar(bm, nome, mat, pesos, recalc=True, suave=True):
    """bm -> objeto com material (nome ou função face->nome) e grupos de vértices."""
    if recalc:
        bmesh.ops.recalc_face_normals(bm, faces=bm.faces[:])
    nomes_mat = []
    for f in bm.faces:
        mn = mat(f) if callable(mat) else mat
        if mn not in nomes_mat:
            nomes_mat.append(mn)
        f.material_index = nomes_mat.index(mn)
    me = bpy.data.meshes.new(nome)
    bm.to_mesh(me)
    bm.free()
    for mn in nomes_mat:
        me.materials.append(MATS[mn])
    if suave:
        for pl in me.polygons:
            pl.use_smooth = True
        me.set_sharp_from_angle(angle=math.radians(38))
    ob = bpy.data.objects.new(nome, me)
    scene.collection.objects.link(ob)
    grupos = {}
    for v in me.vertices:
        ws = pesos(Vector(v.co)) if callable(pesos) else {pesos: 1.0}
        for osso, w in ws.items():
            if w <= 1e-4:
                continue
            if osso not in grupos:
                grupos[osso] = ob.vertex_groups.new(name=osso)
            grupos[osso].add([v.index], w, 'REPLACE')
    PECAS.append(ob)
    return ob

def anel_elipse(z, a, b, y0, n, x0=0.0):
    return [Vector((x0 + a * math.cos(-math.pi / 2 + 2 * math.pi * i / n),
                    y0 + b * math.sin(-math.pi / 2 + 2 * math.pi * i / n), z)) for i in range(n)]

def anel_ao_longo(centro, d, u_ref, r, n, r2=None):
    d = d.normalized()
    u = (u_ref - d * u_ref.dot(d)).normalized()
    v = d.cross(u)
    r2 = r if r2 is None else r2
    return [centro + u * (r * math.cos(2 * math.pi * i / n)) + v * (r2 * math.sin(2 * math.pi * i / n))
            for i in range(n)]

def loft(bm, aneis, tampa0=False, tampa1=False):
    vs = [[bm.verts.new(p) for p in anel] for anel in aneis]
    n = len(aneis[0])
    for r0, r1 in zip(vs, vs[1:]):
        for i in range(n):
            bm.faces.new((r0[i], r0[(i + 1) % n], r1[(i + 1) % n], r1[i]))
    if tampa0:
        c = bm.verts.new(sum(aneis[0], Vector()) / n)
        for i in range(n):
            bm.faces.new((vs[0][(i + 1) % n], vs[0][i], c))
    if tampa1:
        c = bm.verts.new(sum(aneis[-1], Vector()) / n)
        for i in range(n):
            bm.faces.new((vs[-1][i], vs[-1][(i + 1) % n], c))
    return vs

def tubo_grosso(bm, externos, internos):
    """Casca fechada (tipo toro): anéis externos + internos ligados nas pontas."""
    ve = loft(bm, externos)
    vi = loft(bm, internos)
    n = len(externos[0])
    for a, b in ((ve[0], vi[0]), (ve[-1], vi[-1])):
        for i in range(n):
            bm.faces.new((a[i], a[(i + 1) % n], b[(i + 1) % n], b[i]))

def caixa(bm, centro, tam, eixos=None, chanfro=0.0):
    """Caixa (opcionalmente chanfrada) com eixos locais (colunas da matriz 3x3)."""
    tmp = bmesh.new()
    bmesh.ops.create_cube(tmp, size=1.0)
    for v in tmp.verts:
        v.co = Vector((v.co.x * tam[0], v.co.y * tam[1], v.co.z * tam[2]))
    if chanfro > 0:
        bmesh.ops.bevel(tmp, geom=tmp.verts[:] + tmp.edges[:], offset=chanfro, segments=1,
                        affect='EDGES', profile=0.5)
    R = eixos if eixos is not None else Matrix.Identity(3)
    for v in tmp.verts:
        v.co = centro + R @ v.co
    _mesclar(bm, tmp)

def esfera(bm, centro, raios, u=12, v=8, filtro=None, rot=None):
    tmp = bmesh.new()
    bmesh.ops.create_uvsphere(tmp, u_segments=u, v_segments=v, radius=1.0)
    for vt in tmp.verts:
        p = Vector((vt.co.x * raios[0], vt.co.y * raios[1], vt.co.z * raios[2]))
        vt.co = centro + (rot @ p if rot is not None else p)
    if filtro is not None:
        apagar = [f for f in tmp.faces if not filtro(f.calc_center_median())]
        bmesh.ops.delete(tmp, geom=apagar, context='FACES')
    _mesclar(bm, tmp)

def _mesclar(bm, tmp):
    mapa = {}
    for v in tmp.verts:
        mapa[v] = bm.verts.new(v.co)
    for f in tmp.faces:
        bm.faces.new([mapa[v] for v in f.verts])
    tmp.free()

def fita(bm, pontos, largura, espessura, normal_fn):
    """Tira retangular (alça/cordão) seguindo pontos; normal_fn(p) = 'pra fora'."""
    aneis = []
    for i, p in enumerate(pontos):
        t = (pontos[min(i + 1, len(pontos) - 1)] - pontos[max(i - 1, 0)]).normalized()
        nrm = normal_fn(p)
        nrm = (nrm - t * nrm.dot(t)).normalized()
        lat = t.cross(nrm).normalized()
        h, e = largura / 2, espessura / 2
        aneis.append([p + lat * h + nrm * e, p - lat * h + nrm * e, p - lat * h - nrm * e, p + lat * h - nrm * e])
    loft(bm, aneis, tampa0=True, tampa1=True)

def normal_tronco(p):
    _, _, y0 = perfil(p.z)
    return Vector((p.x, p.y - y0, 0.0)).normalized()

def envolver_texto(bm, lado, centro_y, off):
    """Faz o texto acompanhar a curvatura do colete (lado -1 frente, +1 costas)."""
    for v in bm.verts:
        local = v.co.y - centro_y
        v.co.y = sup_y(v.co.x, v.co.z, lado, off) + lado * 0.0017 + local

def ponto_no_tronco(x, z, lado, off):
    return Vector((x, sup_y(x, z, lado, off), z))

def texto(bm, corpo, tam, centro, girar_z=0.0, extrusao=0.0015):
    cu = bpy.data.curves.new('txt', 'FONT')
    cu.body = corpo
    cu.size = tam
    cu.align_x = 'CENTER'
    cu.align_y = 'CENTER'
    cu.extrude = extrusao
    ob = bpy.data.objects.new('txt', cu)
    scene.collection.objects.link(ob)
    dg = bpy.context.evaluated_depsgraph_get()
    me = bpy.data.meshes.new_from_object(ob.evaluated_get(dg))
    tmp = bmesh.new()
    tmp.from_mesh(me)
    R = Matrix.Rotation(girar_z, 3, 'Z') @ Matrix.Rotation(math.radians(90), 3, 'X')
    for v in tmp.verts:
        v.co = centro + R @ v.co
    _mesclar(bm, tmp)
    bpy.data.objects.remove(ob)
    bpy.data.curves.remove(cu)
    bpy.data.meshes.remove(me)

# ------------------------------------------------------------------ esqueleto (posições)
OMBRO = {s: Vector((s * 0.182, 0.0, 1.385)) for s in (1, -1)}
DIR_BRACO = {s: Vector((s * math.cos(math.radians(45)), 0.0, -math.sin(math.radians(45)))) for s in (1, -1)}
COTOVELO = {s: OMBRO[s] + DIR_BRACO[s] * 0.27 for s in (1, -1)}
PULSO = {s: COTOVELO[s] + DIR_BRACO[s] * 0.25 for s in (1, -1)}
MAO_FIM = {s: PULSO[s] + DIR_BRACO[s] * 0.17 for s in (1, -1)}
QUADRIL = {s: Vector((s * 0.095, 0.0, 0.93)) for s in (1, -1)}
JOELHO = {s: Vector((s * 0.100, -0.005, 0.50)) for s in (1, -1)}
TORNOZELO = {s: Vector((s * 0.100, 0.010, 0.09)) for s in (1, -1)}
PONTA_PE = {s: Vector((s * 0.100, -0.140, 0.03)) for s in (1, -1)}
LADO = {1: 'Left', -1: 'Right'}

# ------------------------------------------------------------------ 1. tronco (camisa + calça)
bm = bmesh.new()
aneis = [anel_elipse(z, a, b, y0, 16) for (z, a, b, y0) in TORSO]
loft(bm, aneis, tampa0=True, tampa1=False)
finalizar(bm, 'Tronco', lambda f: 'Calca' if f.calc_center_median().z < 0.965 else 'Camisa',
          lambda p: pesos_cadeia(p.z))

# ------------------------------------------------------------------ 2. pescoço
bm = bmesh.new()
loft(bm, [anel_elipse(z, r, r * 0.96, 0.004, 10) for z, r in ((1.40, 0.058), (1.46, 0.057), (1.50, 0.055), (1.55, 0.053))],
     tampa0=True, tampa1=True)
finalizar(bm, 'Pescoco', 'Pele', lambda p: pesos_cadeia(p.z))

# ------------------------------------------------------------------ 3. cabeça
CAB_C = Vector((0.0, -0.010, 1.628))
CAB_R = (0.108, 0.118, 0.136)

def frente_cab(dx, dz, emb=0.0):
    """Ponto na frente da cabeça (dx, dz relativos ao centro); emb>0 afunda pra dentro."""
    q = max(0.0, 1 - (dx / CAB_R[0]) ** 2 - (dz / CAB_R[2]) ** 2)
    return Vector((dx, CAB_C.y - CAB_R[1] * math.sqrt(q) + emb, CAB_C.z + dz))

bm = bmesh.new()
esfera(bm, CAB_C, CAB_R, u=16, v=12)
for v in bm.verts:  # queixo/mandíbula um pouco mais estreitos
    if v.co.z < CAB_C.z:
        k = 1.0 - 0.16 * ((CAB_C.z - v.co.z) / CAB_R[2])
        v.co.x *= k
finalizar(bm, 'Cabeca', 'Pele', 'Head')

bm = bmesh.new()  # orelhas + nariz
for s in (1, -1):
    esfera(bm, Vector((s * (CAB_R[0] - 0.004), 0.004, CAB_C.z - 0.006)), (0.020, 0.034, 0.046), u=8, v=6)
esfera(bm, frente_cab(0.0, -0.014, 0.010), (0.018, 0.022, 0.028), u=8, v=6)
finalizar(bm, 'Orelhas', 'Pele', 'Head')

bm = bmesh.new()
for s in (1, -1):
    esfera(bm, frente_cab(s * 0.040, 0.018, 0.005), (0.014, 0.009, 0.019), u=8, v=6)
finalizar(bm, 'Olhos', 'Olhos', 'Head')

bm = bmesh.new()
for s in (1, -1):
    R = Matrix.Rotation(math.radians(-s * 8), 3, 'Y')
    caixa(bm, frente_cab(s * 0.041, 0.050, 0.004), (0.038, 0.009, 0.010), R, 0.002)
finalizar(bm, 'Sobrancelhas', 'Cabelo', 'Head')

bm = bmesh.new()
caixa(bm, frente_cab(0.0, -0.052, 0.003), (0.036, 0.007, 0.007), None, 0.0015)
finalizar(bm, 'Boca', 'Boca', 'Head')

# cabelo: casca atrás/dos lados, embaixo do boné
bm = bmesh.new()
esfera(bm, CAB_C + Vector((0, 0.004, 0.0)), (CAB_R[0] * 1.05, CAB_R[1] * 1.05, CAB_R[2] * 1.04), u=16, v=12,
       filtro=lambda c: c.y > -0.05 and c.z > CAB_C.z - 0.07)
finalizar(bm, 'Cabelo', 'Cabelo', 'Head', recalc=False)

# boné
BONE_C = CAB_C + Vector((0.0, 0.002, 0.027))
BONE_R = (0.119, 0.130, 0.118)
bm = bmesh.new()
esfera(bm, BONE_C, BONE_R, u=16, v=10, filtro=lambda c: c.z > BONE_C.z + 0.013)
borda = [e for e in bm.edges if e.is_boundary]
ret = bmesh.ops.extrude_edge_only(bm, edges=borda)
for v in [e for e in ret['geom'] if isinstance(e, bmesh.types.BMVert)]:
    v.co = BONE_C + Vector(((v.co.x - BONE_C.x) * 0.9, (v.co.y - BONE_C.y) * 0.9, v.co.z - BONE_C.z + 0.004))
bmesh.ops.recalc_face_normals(bm, faces=bm.faces[:])
aba = []
n_aba = 11
for i in range(n_aba):
    ang = math.radians(-75 + 150 * i / (n_aba - 1))
    ext = Vector((0.123 * math.sin(ang), BONE_C.y - 0.004 - 0.228 * math.cos(ang), 0.0))
    inn = Vector((0.110 * math.sin(ang), BONE_C.y - 0.124 * math.cos(ang), 0.0))
    aba.append((ext, inn))
tilt = Matrix.Rotation(math.radians(-10), 3, 'X')
base = Vector((0.0, 0.0, BONE_C.z + 0.020))
topo, fundo = [], []
for ext, inn in aba:
    for lista, dz in ((topo, 0.006), (fundo, -0.006)):
        lista.append((bm.verts.new(base + tilt @ (ext + Vector((0, 0, dz)))),
                      bm.verts.new(base + tilt @ (inn + Vector((0, 0, dz))))))
for i in range(n_aba - 1):
    (e0, i0), (e1, i1) = topo[i], topo[i + 1]
    (f0, j0), (f1, j1) = fundo[i], fundo[i + 1]
    bm.faces.new((e0, e1, i1, i0))
    bm.faces.new((f0, j0, j1, f1))
    bm.faces.new((e0, f0, f1, e1))
    bm.faces.new((i0, i1, j1, j0))
for k in (0, n_aba - 1):
    e, i_ = topo[k]; f, j = fundo[k]
    bm.faces.new((e, i_, j, f))
bmesh.ops.recalc_face_normals(bm, faces=bm.faces[:])
esfera(bm, BONE_C + Vector((0, 0, BONE_R[2] - 0.004)), (0.013, 0.013, 0.008), u=8, v=4)
finalizar(bm, 'Bone', 'Bone', 'Head', recalc=False)

def normal_elipsoide(c, r, p):
    d = p - c
    return Vector((d.x / r[0] ** 2, d.y / r[1] ** 2, d.z / r[2] ** 2)).normalized()

zl = BONE_C.z + 0.072
yl = BONE_C.y - BONE_R[1] * math.sqrt(1 - ((zl - BONE_C.z) / BONE_R[2]) ** 2)
pl = Vector((0.0, yl, zl))
nl = normal_elipsoide(BONE_C, BONE_R, pl)
eixo_z = nl
eixo_x = Vector((1, 0, 0))
eixo_y = eixo_z.cross(eixo_x).normalized()
R_logo = Matrix((eixo_x, eixo_y, eixo_z)).transposed()
bm = bmesh.new()
tmp = bmesh.new()
bmesh.ops.create_cone(tmp, cap_ends=True, segments=12, radius1=0.028, radius2=0.028, depth=0.004)
for v in tmp.verts:
    v.co = pl + R_logo @ v.co
_mesclar(bm, tmp)
finalizar(bm, 'BoneLogo', 'Logo', 'Head', suave=False)
bm = bmesh.new()
caixa(bm, pl + nl * 0.0035, (0.032, 0.010, 0.003), R_logo, 0.0)
caixa(bm, pl + nl * 0.0035, (0.010, 0.032, 0.003), R_logo, 0.0)
finalizar(bm, 'BoneCruz', 'Colete', 'Head', suave=False)

# ------------------------------------------------------------------ 4. braços
for s in (1, -1):
    L = LADO[s]
    S, E, W, d = OMBRO[s], COTOVELO[s], PULSO[s], DIR_BRACO[s]
    uref = Vector((0, 1, 0))
    pos_r = [(S - d * 0.02, 0.058), (S + d * 0.12, 0.057), (E - d * 0.035, 0.050), (E, 0.048),
             (E + d * 0.035, 0.047), (E + d * 0.12, 0.049), (W - d * 0.01, 0.040)]
    bm = bmesh.new()
    loft(bm, [anel_ao_longo(p, d, uref, r, 10) for p, r in pos_r], tampa0=True, tampa1=True)
    finalizar(bm, L + 'Braco', 'Pele',
              lambda p, E=E, d=d, L=L: pesos_membro(p, E, d, d, L + 'UpperArm', L + 'LowerArm'))

    # manga (casca grossa) + ombro arredondado (sem "ombreira")
    bm = bmesh.new()
    ext = [anel_ao_longo(S - d * 0.03, d, uref, 0.078, 10), anel_ao_longo(S + d * 0.08, d, uref, 0.075, 10),
           anel_ao_longo(S + d * 0.15, d, uref, 0.072, 10)]
    inn = [anel_ao_longo(S - d * 0.03, d, uref, 0.064, 10), anel_ao_longo(S + d * 0.08, d, uref, 0.061, 10),
           anel_ao_longo(S + d * 0.15, d, uref, 0.059, 10)]
    tubo_grosso(bm, ext, inn)
    esfera(bm, S + Vector((-s * 0.018, 0, -0.008)), (0.066, 0.066, 0.056), u=10, v=8)
    finalizar(bm, L + 'Manga', 'Camisa',
              lambda p, S=S, d=d, L=L: ({'Chest': 0.35, L + 'UpperArm': 0.65}
                                        if (p - S).dot(d) < 0.02 else {L + 'UpperArm': 1.0}))

    # mão: palma + dedos (levemente curvados) + polegar
    eixo_l = d
    eixo_w = Vector((0, 1, 0))
    eixo_t = eixo_l.cross(eixo_w).normalized()   # esquerda: dorso; direita: palma
    dorso = eixo_t * s
    R_mao = Matrix((eixo_t, eixo_w, eixo_l)).transposed()  # x=espessura, y=largura, z=comprimento
    bm = bmesh.new()
    caixa(bm, W + d * 0.047, (0.035, 0.084, 0.090), R_mao, 0.010)
    curva = Matrix.Rotation(math.radians(-s * 16), 3, eixo_w) @ R_mao
    caixa(bm, W + d * 0.122 - dorso * 0.005, (0.028, 0.080, 0.075), curva, 0.009)
    R_pol = Matrix.Rotation(math.radians(s * 28), 3, eixo_t) @ R_mao
    caixa(bm, W + d * 0.052 + eixo_w * (-0.048) - dorso * 0.006, (0.026, 0.028, 0.060), R_pol, 0.008)
    finalizar(bm, L + 'Mao', 'Pele', L + 'Hand')

    if s == 1:  # relógio no pulso esquerdo (aparece em primeira pessoa)
        bm = bmesh.new()
        c0, c1 = W - d * 0.052, W - d * 0.026
        tubo_grosso(bm, [anel_ao_longo(c0, d, uref, 0.047, 12), anel_ao_longo(c1, d, uref, 0.046, 12)],
                    [anel_ao_longo(c0, d, uref, 0.041, 12), anel_ao_longo(c1, d, uref, 0.040, 12)])
        finalizar(bm, 'RelogioPulseira', 'Alca', L + 'LowerArm')
        bm = bmesh.new()
        R_rel = Matrix((eixo_w, d, dorso)).transposed()  # z = pra fora do dorso
        caixa(bm, W - d * 0.039 + dorso * 0.047, (0.034, 0.032, 0.010), R_rel, 0.003)
        finalizar(bm, 'RelogioCaixa', 'Metal', L + 'LowerArm', suave=False)
        bm = bmesh.new()
        caixa(bm, W - d * 0.039 + dorso * 0.0525, (0.026, 0.024, 0.002), R_rel, 0.0)
        finalizar(bm, 'RelogioVisor', 'Olhos', L + 'LowerArm', suave=False)

# ------------------------------------------------------------------ 5. pernas (calça) + tênis
for s in (1, -1):
    L = LADO[s]
    H, K, A = QUADRIL[s], JOELHO[s], TORNOZELO[s]
    da, db = (K - H), (A - K)
    uref = Vector((1, 0, 0))
    pos_r = [(H + Vector((0, 0, 0.05)), 0.080), (H, 0.092), (H.lerp(K, 0.2), 0.095), (K - da.normalized() * 0.045, 0.077),
             (K, 0.073), (K + db.normalized() * 0.045, 0.071), (K.lerp(A, 0.4), 0.073), (A + Vector((0, 0, 0.06)), 0.064),
             (A + Vector((0, 0, 0.015)), 0.068)]
    bm = bmesh.new()
    loft(bm, [anel_ao_longo(p, (db if i >= 4 else da), uref, r, 10) for i, (p, r) in enumerate(pos_r)],
         tampa0=True, tampa1=True)
    finalizar(bm, L + 'Perna', 'Calca',
              lambda p, K=K, da=da, db=db, L=L: pesos_membro(p, K, da, db, L + 'UpperLeg', L + 'LowerLeg', 0.04))

    bm = bmesh.new()
    secoes = [(0.070, 0.092, 0.098), (0.022, 0.104, 0.108), (-0.050, 0.082, 0.114),
              (-0.120, 0.062, 0.108), (-0.168, 0.045, 0.090), (-0.190, 0.032, 0.062)]
    aneis_t = []
    for y, h, w in secoes:
        zb, zt = 0.024, 0.024 + h
        c = 0.25
        pts2 = [(-w / 2, zb), (w / 2, zb), (w / 2, zt - h * c), (w / 2 * (1 - c), zt),
                (-w / 2 * (1 - c), zt), (-w / 2, zt - h * c)]
        aneis_t.append([Vector((A.x + px, y, pz)) for px, pz in pts2])
    loft(bm, aneis_t, tampa0=True, tampa1=True)
    finalizar(bm, L + 'Tenis', 'Tenis', L + 'Foot')

    bm = bmesh.new()
    caixa(bm, Vector((A.x, -0.060, 0.013)), (0.122, 0.285, 0.026), None, 0.009)
    caixa(bm, Vector((A.x + s * 0.058, -0.025, 0.062)), (0.004, 0.11, 0.013),
          Matrix.Rotation(math.radians(-18), 3, 'X'), 0.0)
    caixa(bm, Vector((A.x, -0.028, 0.116)), (0.042, 0.075, 0.006), Matrix.Rotation(math.radians(-17), 3, 'X'), 0.002)
    finalizar(bm, L + 'Sola', 'Sola', L + 'Foot', suave=False)

# ------------------------------------------------------------------ 6. colete (casca grossa) + alças
Z_COLETE = [0.925, 0.98, 1.05, 1.13, 1.22, 1.31, 1.365]
bm = bmesh.new()
ext = []
inn = []
for z in Z_COLETE:
    a, b, y0 = perfil(z)
    ext.append(anel_elipse(z, a + COLETE_OFF, b + COLETE_OFF, y0, 16))
    inn.append(anel_elipse(z, a + 0.002, b + 0.002, y0, 16))
tubo_grosso(bm, ext, inn)
finalizar(bm, 'Colete', 'Colete', lambda p: pesos_cadeia(p.z))

bm = bmesh.new()
for s in (1, -1):
    x = s * 0.108
    pts = [ponto_no_tronco(x, 1.35, -1, COLETE_OFF - 0.004), ponto_no_tronco(x, 1.41, -1, 0.012),
           Vector((x, 0.0, 1.452)), ponto_no_tronco(x, 1.41, 1, 0.012), ponto_no_tronco(x, 1.35, 1, COLETE_OFF - 0.004)]
    pts[2].z = 1.447
    fita(bm, pts, 0.055, 0.012, lambda p: normal_tronco(p) if abs(p.y) > 0.02 else Vector((0, 0, 1)))
finalizar(bm, 'ColeteAlcas', 'Colete', 'Chest')

# zíper/debrum na frente + lapelas dos bolsos
bm = bmesh.new()
pts = [Vector((0.0, sup_y(0.0, z, -1, COLETE_OFF) - 0.0015, z)) for z in (0.93, 1.0, 1.08, 1.16, 1.24, 1.32, 1.362)]
fita(bm, pts, 0.014, 0.004, normal_tronco)
for s in (1, -1):
    xb, zb = s * 0.078, 1.0
    yb = sup_y(xb, zb, -1, COLETE_OFF)
    nb = normal_tronco(Vector((xb, yb, zb)))
    lat = Vector((0, 0, 1)).cross(nb).normalized()
    R_bolso = Matrix((lat, nb, Vector((0, 0, 1)))).transposed()
    caixa(bm, Vector((xb, yb, zb + 0.036)) + nb * 0.004, (0.080, 0.009, 0.022), R_bolso, 0.003)
finalizar(bm, 'ColeteDetalhes', 'ColeteEscuro', lambda p: pesos_cadeia(p.z), suave=False)
bm = bmesh.new()
for s in (1, -1):
    xb, zb = s * 0.078, 1.0
    yb = sup_y(xb, zb, -1, COLETE_OFF)
    nb = normal_tronco(Vector((xb, yb, zb)))
    lat = Vector((0, 0, 1)).cross(nb).normalized()
    R_bolso = Matrix((lat, nb, Vector((0, 0, 1)))).transposed()
    caixa(bm, Vector((xb, yb, zb)) + nb * 0.002, (0.076, 0.008, 0.066), R_bolso, 0.003)
finalizar(bm, 'ColeteBolsos', 'Colete', lambda p: pesos_cadeia(p.z), suave=False)

# faixas refletivas
bm = bmesh.new()
for z0, z1 in ((1.085, 1.12), (1.165, 1.20)):
    ext, inn = [], []
    for z in (z0, z1):
        a, b, y0 = perfil(z)
        ext.append(anel_elipse(z, a + COLETE_OFF + 0.005, b + COLETE_OFF + 0.005, y0, 16))
        inn.append(anel_elipse(z, a + COLETE_OFF - 0.003, b + COLETE_OFF - 0.003, y0, 16))
    tubo_grosso(bm, ext, inn)
finalizar(bm, 'Refletivo', 'Refletivo', lambda p: pesos_cadeia(p.z))

# "ACS" no peito direito (x negativo), virado pra frente
bm = bmesh.new()
xt, zt = -0.092, 1.283
yt = sup_y(xt, zt, -1, COLETE_OFF)
texto(bm, 'ACS', 0.040, Vector((xt, yt, zt)))
envolver_texto(bm, -1, yt, COLETE_OFF)
finalizar(bm, 'TextoACS', 'Texto', lambda p: pesos_cadeia(p.z), recalc=False, suave=False)

# "AGENTE DE SAÚDE" nas costas, deslocado pro lado direito pra fugir da alça da bolsa
bm = bmesh.new()
texto(bm, 'AGENTE DE' + chr(10) + 'SAÚDE', 0.030, Vector((-0.035, 0.0, 1.315)), girar_z=math.pi)
envolver_texto(bm, 1, 0.0, COLETE_OFF)
finalizar(bm, 'TextoCostas', 'Texto', lambda p: pesos_cadeia(p.z), recalc=False, suave=False)

# ------------------------------------------------------------------ 7. crachá (peito esquerdo, preso por clipe)
xc, zc = 0.108, 1.262
yc = sup_y(xc, zc, -1, COLETE_OFF) - 0.004
nrm = normal_tronco(Vector((xc, yc, zc)))
eixo_n = nrm
eixo_up = Vector((0, 0, 1))
eixo_lat = eixo_up.cross(eixo_n).normalized()
R_c = Matrix((eixo_lat, eixo_n, eixo_up)).transposed()  # x=lateral, y=normal, z=cima
Cc = Vector((xc, yc, zc))
bm = bmesh.new()
caixa(bm, Cc, (0.052, 0.004, 0.072), R_c, 0.0015)
finalizar(bm, 'Cracha', 'Cracha', 'Chest', suave=False)
bm = bmesh.new()
caixa(bm, Cc + R_c @ Vector((0, 0.0025, 0.026)), (0.052, 0.002, 0.018), R_c, 0.0)
finalizar(bm, 'CrachaFaixa', 'Colete', 'Chest', suave=False)
bm = bmesh.new()
caixa(bm, Cc + R_c @ Vector((-0.012, 0.0025, -0.004)), (0.017, 0.002, 0.021), R_c, 0.0)
finalizar(bm, 'CrachaFoto', 'Pele', 'Chest', suave=False)
bm = bmesh.new()
for dz in (-0.002, -0.012):
    caixa(bm, Cc + R_c @ Vector((0.011, 0.0025, dz)), (0.020, 0.002, 0.004), R_c, 0.0)
caixa(bm, Cc + R_c @ Vector((0, 0.0, 0.041)), (0.012, 0.006, 0.012), R_c, 0.001)  # clipe
finalizar(bm, 'CrachaDetalhes', 'Alca', 'Chest', suave=False)

# ------------------------------------------------------------------ 8. bolsa tiracolo (ombro esquerdo -> quadril direito)
BOLSA_C = Vector((-0.212, -0.015, 0.935))
def pontos_alca(lado):
    base = [(0.112, 1.458), (0.115, 1.40), (0.085, 1.33), (0.035, 1.25), (-0.02, 1.17),
            (-0.075, 1.10), (-0.125, 1.04), (-0.165, 1.005)]
    pts = []
    for x, z in base:
        a, _, _ = perfil(z)
        if abs(x) < (a + COLETE_OFF) * 0.98:
            pts.append(ponto_no_tronco(x, z, lado, COLETE_OFF + 0.007))
        else:
            pts.append(Vector((x, lado * 0.02, z)))
    topo = Vector((0.112, 0.0, 1.462))
    pts[0] = topo
    pts.append(Vector((BOLSA_C.x + 0.02, BOLSA_C.y + lado * 0.07, BOLSA_C.z + 0.07)))
    return pts

bm = bmesh.new()
frente = pontos_alca(-1)
costas = pontos_alca(1)
caminho = list(reversed(frente)) + costas[1:]
fita(bm, caminho, 0.034, 0.006,
     lambda p: normal_tronco(p) if abs(p.y) > 0.03 and p.z > 1.0 else Vector((0, 0, 1)) if p.z > 1.44 else Vector((-1, 0, 0)))
finalizar(bm, 'BolsaAlca', 'Alca', lambda p: pesos_cadeia(p.z) if p.z > 0.99 else {'Hips': 1.0})

R_b = Matrix.Rotation(math.radians(8), 3, 'Z')
bm = bmesh.new()
caixa(bm, BOLSA_C, (0.068, 0.20, 0.15), R_b, 0.012)
finalizar(bm, 'Bolsa', 'Bolsa', 'Hips')
bm = bmesh.new()
caixa(bm, BOLSA_C + R_b @ Vector((-0.036, 0.0, 0.03)), (0.008, 0.204, 0.095), R_b, 0.003)
caixa(bm, BOLSA_C + R_b @ Vector((0.0, 0.0, 0.077)), (0.074, 0.204, 0.008), R_b, 0.003)
finalizar(bm, 'BolsaTampa', 'BolsaEscuro', 'Hips')
bm = bmesh.new()
caixa(bm, BOLSA_C + R_b @ Vector((-0.042, 0.0, -0.005)), (0.006, 0.03, 0.022), R_b, 0.002)
finalizar(bm, 'BolsaFivela', 'Metal', 'Hips', suave=False)
bm = bmesh.new()
tc = BOLSA_C + R_b @ Vector((-0.0405, 0.0, 0.04))
texto(bm, 'SAÚDE', 0.028, Vector((0, 0, 0)), girar_z=0.0, extrusao=0.001)
R_txt = R_b @ Matrix.Rotation(math.radians(-90), 3, 'Z')  # texto olhando pra -X (lado de fora)
for v in bm.verts:
    v.co = tc + R_txt @ v.co
finalizar(bm, 'BolsaTexto', 'Texto', 'Hips', recalc=False, suave=False)

# ------------------------------------------------------------------ junta tudo (corpo + cabeça separada)
# A cabeça vira uma malha à parte: no jogo (primeira pessoa) ela fica "Shadows Only" —
# some da câmera mas continua projetando sombra, sem precisar encolher o osso.
NOMES_CABECA = {'Cabeca', 'Orelhas', 'Olhos', 'Sobrancelhas', 'Boca', 'Cabelo', 'Bone', 'BoneLogo', 'BoneCruz'}

def juntar(obs, nome):
    bpy.ops.object.select_all(action='DESELECT')
    for ob in obs:
        ob.select_set(True)
    bpy.context.view_layer.objects.active = obs[0]
    bpy.ops.object.join()
    ob = bpy.context.view_layer.objects.active
    ob.name = nome
    ob.data.name = nome
    bpy.ops.object.mode_set(mode='EDIT')
    bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.mesh.remove_doubles(threshold=0.00001)
    bpy.ops.object.mode_set(mode='OBJECT')
    print('TRIS', nome, sum(len(pl.vertices) - 2 for pl in ob.data.polygons))
    return ob

pecas_corpo = [ob for ob in PECAS if ob.name not in NOMES_CABECA]
pecas_cabeca = [ob for ob in PECAS if ob.name in NOMES_CABECA]
corpo = juntar(pecas_corpo, 'AgenteSaude_Corpo')
cabeca = juntar(pecas_cabeca, 'AgenteSaude_Cabeca')

# ------------------------------------------------------------------ armature
arm_data = bpy.data.armatures.new('AgenteSaude_Rig')
arm = bpy.data.objects.new('AgenteSaude', arm_data)
scene.collection.objects.link(arm)
bpy.context.view_layer.objects.active = arm
bpy.ops.object.mode_set(mode='EDIT')
eb = arm_data.edit_bones

def osso(nome, cabeca, cauda, pai=None, conectado=False, roll_ref=None):
    b = eb.new(nome)
    b.head = cabeca
    b.tail = cauda
    if pai:
        b.parent = eb[pai]
        b.use_connect = conectado
    if roll_ref is not None:
        b.align_roll(roll_ref)
    return b

osso('Hips', (0, 0, 0.95), (0, 0, 1.05), roll_ref=Vector((0, -1, 0)))
osso('Spine', (0, 0, 1.05), (0, 0, 1.20), 'Hips', True, Vector((0, -1, 0)))
osso('Chest', (0, 0, 1.20), (0, 0, 1.42), 'Spine', True, Vector((0, -1, 0)))
osso('Neck', (0, 0, 1.42), (0, 0, 1.52), 'Chest', True, Vector((0, -1, 0)))
osso('Head', (0, 0, 1.52), (0, 0, 1.80), 'Neck', True, Vector((0, -1, 0)))
for s in (1, -1):
    L = LADO[s]
    osso(L + 'UpperArm', OMBRO[s], COTOVELO[s], 'Chest', False, Vector((0, -1, 0)))
    osso(L + 'LowerArm', COTOVELO[s], PULSO[s], L + 'UpperArm', True, Vector((0, -1, 0)))
    osso(L + 'Hand', PULSO[s], MAO_FIM[s], L + 'LowerArm', True, Vector((0, -1, 0)))
    osso(L + 'UpperLeg', QUADRIL[s], JOELHO[s], 'Hips', False, Vector((0, -1, 0)))
    osso(L + 'LowerLeg', JOELHO[s], TORNOZELO[s], L + 'UpperLeg', True, Vector((0, -1, 0)))
    osso(L + 'Foot', TORNOZELO[s], PONTA_PE[s], L + 'LowerLeg', True, Vector((0, 0, 1)))
bpy.ops.object.mode_set(mode='OBJECT')

for ob in (corpo, cabeca):
    ob.parent = arm
    mod = ob.modifiers.new('Armature', 'ARMATURE')
    mod.object = arm

# ------------------------------------------------------------------ animação
REST = {b.name: b.matrix_local.to_3x3() for b in arm_data.bones}

def Rx(g): return Matrix.Rotation(math.radians(g), 3, 'X')
def Ry(g): return Matrix.Rotation(math.radians(g), 3, 'Y')
def Rz(g): return Matrix.Rotation(math.radians(g), 3, 'Z')

def eixo_rot(eixo, g):
    return Matrix.Rotation(math.radians(g), 3, eixo.normalized())

def q_local(nome, R):
    M = REST[nome]
    return (M.inverted() @ R @ M).to_quaternion()

EIXO_COTOVELO = {s: DIR_BRACO[s].cross(Vector((0, -1, 0))).normalized() for s in (1, -1)}

def interp(chaves, p):
    """Interpolação cosseno periódica entre chaves [(fase, valor)...] com fase 0..1."""
    p = p % 1.0
    for (p0, v0), (p1, v1) in zip(chaves, chaves[1:]):
        if p0 <= p <= p1:
            t = (p - p0) / (p1 - p0) if p1 > p0 else 0.0
            t = (1 - math.cos(math.pi * t)) / 2
            return v0 + (v1 - v0) * t
    return chaves[-1][1]

def aplicar(frame, poses, hips_loc=Vector()):
    """poses: nome -> Matrix3 (rotação no espaço da armature, relativa ao repouso)."""
    for pb in arm.pose.bones:
        pb.rotation_mode = 'QUATERNION'
        R = poses.get(pb.name, Matrix.Identity(3))
        pb.rotation_quaternion = q_local(pb.name, R)
        pb.keyframe_insert('rotation_quaternion', frame=frame)
    hb = arm.pose.bones['Hips']
    hb.location = REST['Hips'].inverted() @ hips_loc
    hb.keyframe_insert('location', frame=frame)

def pose_perna(s, coxa_fwd, joelho, pe_mundo):
    L = LADO[s]
    # pe_mundo: inclinação do pé em relação ao chão (+ = ponta pra baixo)
    pe_local = pe_mundo - ((-coxa_fwd) + joelho)
    return {L + 'UpperLeg': Rx(-coxa_fwd), L + 'LowerLeg': Rx(joelho), L + 'Foot': Rx(pe_local)}

def pose_braco(s, abaixar_ate, frente, cotovelo, lateral=0.0):
    """abaixar_ate: ângulo final em relação à vertical (graus); frente: balanço pra frente."""
    L = LADO[s]
    phi = s * (45 - abaixar_ate - lateral)
    return {L + 'UpperArm': Rx(-frente) @ Ry(phi),
            L + 'LowerArm': eixo_rot(EIXO_COTOVELO[s], cotovelo),
            L + 'Hand': eixo_rot(EIXO_COTOVELO[s], cotovelo * 0.15)}

REPOUSO_BRACO = {1: 12.0, -1: 20.0}  # direito um pouco aberto por causa da bolsa

def nova_acao(nome, n_frames):
    act = bpy.data.actions.new(nome)
    act.use_fake_user = True
    arm.animation_data_create()
    arm.animation_data.action = act
    return act

acoes = []

# --- Idle (3 s)
acoes.append(nova_acao('Idle', 90))
N = 90
for f in range(0, N + 1, 3):
    p = f / N
    b = math.sin(2 * math.pi * p)
    b2 = math.sin(4 * math.pi * p + 0.6)
    ps = {}
    ps.update(pose_perna(1, 1.0 + 0.6 * b, 3.0, 0.0))
    ps.update(pose_perna(-1, -1.0 - 0.6 * b, 4.0, 0.0))
    ps.update(pose_braco(1, REPOUSO_BRACO[1] + 0.8 * b, 4.0 + 1.0 * b2, 12 + 2 * b))
    ps.update(pose_braco(-1, REPOUSO_BRACO[-1] - 0.8 * b, 3.0 - 1.0 * b2, 11 + 2 * b))
    ps['Hips'] = Ry(1.2 * b)
    ps['Spine'] = Rx(1.0 + 0.8 * b2) @ Ry(-0.8 * b)
    ps['Chest'] = Rx(-1.2 * math.sin(2 * math.pi * p * 2))
    ps['Neck'] = Rx(1.5)
    ps['Head'] = Rz(4 * math.sin(2 * math.pi * p + 1.0)) @ Rx(-1.0 * b)
    aplicar(f, ps, Vector((0.006 * b, 0, -0.004 - 0.003 * (1 - math.cos(2 * math.pi * p)))))

# --- Walk (16 frames = 0,533 s; passo ~0,74 m -> ~2,8 m/s)
WALK_COXA = [(0, 26), (0.12, 18), (0.25, 2), (0.5, -24), (0.62, -15), (0.75, 5), (0.88, 24), (1, 26)]
WALK_JOELHO = [(0, 4), (0.12, 14), (0.25, 6), (0.45, 10), (0.6, 50), (0.75, 62), (0.88, 20), (1, 4)]
WALK_PE = [(0, -16), (0.1, 0), (0.35, 0), (0.5, 25), (0.6, 30), (0.75, 5), (0.9, -8), (1, -16)]
WALK_Z = [(0, -0.05), (0.25, 0.0), (0.5, -0.05), (0.75, 0.0), (1, -0.05)]
acoes.append(nova_acao('Walk', 16))
N = 16
for f in range(N + 1):
    p = f / N
    c = math.cos(2 * math.pi * p)
    ps = {}
    for s, ph in ((1, p), (-1, p + 0.5)):
        ps.update(pose_perna(s, interp(WALK_COXA, ph), interp(WALK_JOELHO, ph), interp(WALK_PE, ph)))
    for s, sinal in ((1, -1), (-1, 1)):
        fr = 20 * sinal * c
        ps.update(pose_braco(s, REPOUSO_BRACO[s] + 2, fr, 14 + 12 * max(0.0, fr / 20)))
    ps['Hips'] = Rz(-6 * c) @ Ry(2.5 * math.sin(2 * math.pi * p))
    ps['Spine'] = Rx(4) @ Rz(3 * c)
    ps['Chest'] = Rz(6 * c)
    ps['Neck'] = Rx(-2)
    ps['Head'] = Rz(-2 * c) @ Rx(-2)
    aplicar(f, ps, Vector((0, 0, interp(WALK_Z, p))))

# --- Run (15 frames = 0,5 s; passada ~1,45 m -> ~5,8 m/s)
RUN_COXA = [(0, 40), (0.184, 25), (0.3, 5), (0.45, -20), (0.6, -10), (0.78, 20), (0.9, 42), (1, 40)]
RUN_JOELHO = [(0, 60), (0.184, 20), (0.3, 40), (0.45, 18), (0.6, 70), (0.78, 105), (0.95, 75), (1, 60)]
RUN_PE = [(0, -10), (0.184, -5), (0.3, 0), (0.45, 40), (0.6, 50), (0.78, 10), (0.95, -10), (1, -10)]
acoes.append(nova_acao('Run', 15))
N = 15
for f in range(N + 1):
    p = f / N
    c = math.cos(2 * math.pi * (p - 0.05))
    ps = {}
    for s, ph in ((1, p), (-1, p + 0.5)):
        ps.update(pose_perna(s, interp(RUN_COXA, ph), interp(RUN_JOELHO, ph), interp(RUN_PE, ph)))
    for s, sinal in ((1, -1), (-1, 1)):
        fr = 30 * sinal * c
        ps.update(pose_braco(s, REPOUSO_BRACO[s] + 6, fr + 4, 76 + 10 * (fr / 30)))
    ps['Hips'] = Rz(-8 * c) @ Rx(4)
    ps['Spine'] = Rx(6) @ Rz(4 * c)
    ps['Chest'] = Rx(3) @ Rz(9 * c)
    ps['Neck'] = Rx(-6)
    ps['Head'] = Rx(-4) @ Rz(-3 * c)
    dz = -0.05 - 0.035 * math.cos(4 * math.pi * (p - 0.3))
    aplicar(f, ps, Vector((0, -0.01, dz)))

# --- Air (no ar, 20 frames, loop sutil)
acoes.append(nova_acao('Air', 20))
N = 20
for f in range(0, N + 1, 2):
    p = f / N
    b = math.sin(2 * math.pi * p)
    ps = {}
    ps.update(pose_perna(1, 38 + 3 * b, 62 + 4 * b, 12))
    ps.update(pose_perna(-1, 8 - 2 * b, 34 - 3 * b, 18))
    ps.update(pose_braco(1, 32 + 3 * b, 18, 38))
    ps.update(pose_braco(-1, 36 - 3 * b, -6, 30))
    ps['Hips'] = Rx(-3)
    ps['Spine'] = Rx(5)
    ps['Chest'] = Rx(2)
    ps['Head'] = Rx(-6)
    aplicar(f, ps, Vector((0, 0, 0.0)))

# --- Carregando (Parte B: pneu nos braços; pose estática, 1 frame de duração)
# No Unity a camada "Carregando" do Animator (máscara: Spine, Chest, braços e mãos)
# põe esta pose por cima da locomoção. O pneu (disco de 0,7 m, banda de ~0,2 m) fica
# de pé na frente do agente, centrado em CARGA_CENTRO, com o topo CARGA_INCLINACAO
# graus para a frente. As mãos pegam a banda dos dois lados, CARGA_PEGA graus acima
# da linha do centro (como num volante): é o que as põe nos cantos inferiores da tela
# em primeira pessoa sem subir o pneu até a mira. A palma fica virada para o centro
# do pneu, com os dedos por cima da banda, para a frente. Os braços saem por IK de
# dois ossos, com os cotovelos para fora e para baixo. Spine e Chest inclinam um
# pouco para trás (contrapeso).
CARGA_CENTRO = Vector((0.0, -0.46, 1.20))
CARGA_INCLINACAO = 8.0                     # graus: topo do pneu para a frente
CARGA_PEGA = 45.0                          # graus acima da linha do centro do pneu
CARGA_PALMA_R = 0.37                       # raio do disco (0,35) + meia palma
CARGA_POLO = Vector((1.0, 0.30, -0.50))    # para onde vai o cotovelo (lado esquerdo; x espelha)
CARGA_SPINE, CARGA_CHEST = -3.0, -2.0      # graus em X (negativo = para trás)
PIVO_SPINE, PIVO_CHEST = Vector((0, 0, 1.05)), Vector((0, 0, 1.20))
PALMA_NA_MAO = 0.047                       # do pulso ao centro da palma (malha da mão)

def espelha(v, s):
    return Vector((v.x * s, v.y, v.z))

def no_peito(p):
    """Ponto do mundo -> referencial de repouso do Chest (desfaz Spine e Chest)."""
    q = Rx(CARGA_SPINE).inverted() @ (p - PIVO_SPINE) - (PIVO_CHEST - PIVO_SPINE)
    return PIVO_CHEST + Rx(CARGA_CHEST).inverted() @ q

def dir_no_peito(v):
    return Rx(CARGA_CHEST).inverted() @ (Rx(CARGA_SPINE).inverted() @ v)

def base(comprimento, normal):
    """Base ortonormal (colunas: comprimento, normal, comprimento × normal)."""
    l = comprimento.normalized()
    n = (normal - normal.dot(l) * l).normalized()
    return Matrix((l, n, l.cross(n))).transposed()

def braco_ik(s, palma, dedos, normal_palma, polo):
    """IK de dois ossos até a palma. Tudo no referencial de repouso do Chest."""
    S, D = OMBRO[s], DIR_BRACO[s]
    a, b = (COTOVELO[s] - S).length, (PULSO[s] - COTOVELO[s]).length
    dedos = dedos.normalized()
    pulso = palma - dedos * PALMA_NA_MAO
    d = pulso - S
    dist = min(d.length, a + b - 1e-3)
    n = d.normalized()
    x = (a * a - b * b + dist * dist) / (2 * dist)
    h = math.sqrt(max(a * a - x * x, 0.0))
    polo = (polo - polo.dot(n) * n).normalized()
    cotovelo = S + n * x + polo * h
    Ru = D.rotation_difference((cotovelo - S).normalized()).to_matrix()
    Rl = D.rotation_difference(Ru.inverted() @ (S + n * dist - cotovelo).normalized()).to_matrix()
    # Mão: no repouso o comprimento é D e a palma olha para o corpo (-s X, para baixo).
    palma_repouso = Vector((-s * 0.707, 0.0, -0.707))
    Rmao = base(dedos, normal_palma) @ base(D, palma_repouso).inverted()
    Rh = (Ru @ Rl).inverted() @ Rmao
    L = LADO[s]
    return {L + 'UpperArm': Ru, L + 'LowerArm': Rl, L + 'Hand': Rh}

def braco_carregando(s):
    giro = math.radians(CARGA_INCLINACAO)
    pega = math.radians(CARGA_PEGA)
    frente = Vector((0.0, -math.cos(giro), -math.sin(giro)))    # eixo da roda, para a frente
    topo = Vector((0.0, -math.sin(giro), math.cos(giro)))       # do centro ao topo do pneu
    fora = Vector((s * math.cos(pega), 0.0, 0.0)) + topo * math.sin(pega)  # do centro à mão
    palma = CARGA_CENTRO + fora * CARGA_PALMA_R
    dedos = frente * 0.85 - fora * 0.50
    return braco_ik(s, no_peito(palma), dir_no_peito(dedos), dir_no_peito(-fora),
                    dir_no_peito(espelha(CARGA_POLO, s)))

acoes.append(nova_acao('Carregando', 1))
ps = {}
ps.update(pose_perna(1, 1.0, 3.0, 0.0))
ps.update(pose_perna(-1, -1.0, 4.0, 0.0))
ps.update(braco_carregando(1))
ps.update(braco_carregando(-1))
ps['Spine'] = Rx(CARGA_SPINE)
ps['Chest'] = Rx(CARGA_CHEST)
ps['Neck'] = Rx(1.5)
for f in (0, 1):
    aplicar(f, ps, Vector((0, 0, -0.004)))

# --- CarregandoSaco (Parte B: saco de garrafas na mão direita; pose estática, 1 frame)
# No Unity a camada "CarregandoSaco" do Animator (máscara: só o braço direito) ergue
# o braço à frente e para o lado, para o saco, que pende da mão, entrar no canto
# inferior direito da tela em primeira pessoa. A mão segura a boca do saco, com os
# dedos para a frente e para baixo e a palma para dentro; o cotovelo fica para fora
# e para baixo. O tronco não entra (é o da locomoção).
SACO_PALMA = Vector((-0.30, -0.50, 1.555))  # centro da palma (mão direita: x negativo)
SACO_DEDOS = Vector((0.10, -0.85, -0.50))   # direção dos dedos
SACO_NORMAL = Vector((1.0, 0.0, 0.0))       # para onde a palma olha
SACO_POLO = Vector((-1.0, 0.30, -0.70))     # para onde vai o cotovelo

acoes.append(nova_acao('CarregandoSaco', 1))
ps = {}
ps.update(pose_perna(1, 1.0, 3.0, 0.0))
ps.update(pose_perna(-1, -1.0, 4.0, 0.0))
ps.update(braco_ik(-1, SACO_PALMA, SACO_DEDOS, SACO_NORMAL, SACO_POLO))
for f in (0, 1):
    aplicar(f, ps, Vector((0, 0, -0.004)))

# pose padrão do FBX = Idle no frame 0 (sem isso sai a última pose calculada, a do pulo)
for pb in arm.pose.bones:
    pb.location = (0, 0, 0)
    pb.rotation_quaternion = (1, 0, 0, 0)
arm.animation_data.action = bpy.data.actions['Idle']
scene.frame_start, scene.frame_end = 0, 90
scene.frame_set(0)

# ------------------------------------------------------------------ exporta FBX
# O Unity (com Bake Axis Conversion) recebe o personagem de -Y do Blender olhando
# pra -Z; girando o objeto do esqueleto 180° ele entra olhando pra +Z (frente do
# Unity). Só o objeto gira — ossos, pesos e animações ficam iguais.
arm.rotation_euler = (0.0, 0.0, math.pi)
bpy.context.view_layer.update()
bpy.ops.object.select_all(action='SELECT')
fbx = os.path.join(OUT, 'AgenteSaude.fbx')
bpy.ops.export_scene.fbx(
    filepath=fbx, use_selection=False, object_types={'ARMATURE', 'MESH'},
    apply_unit_scale=True, apply_scale_options='FBX_SCALE_ALL',
    axis_forward='-Z', axis_up='Y', use_mesh_modifiers=False, mesh_smooth_type='FACE',
    add_leaf_bones=False, primary_bone_axis='Y', secondary_bone_axis='X', armature_nodetype='NULL',
    use_armature_deform_only=True, bake_anim=True, bake_anim_use_all_bones=True,
    bake_anim_use_nla_strips=False, bake_anim_use_all_actions=True, bake_anim_force_startend_keying=True,
    bake_anim_step=1.0, bake_anim_simplify_factor=0.0)
arm.rotation_euler = (0.0, 0.0, 0.0)
bpy.context.view_layer.update()
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT, 'AgenteSaude.blend'))
print('FBX OK', fbx, os.path.getsize(fbx))

# ------------------------------------------------------------------ prévias
if RENDER:
    world = bpy.data.worlds.new('W')
    scene.world = world
    world.use_nodes = True
    world.node_tree.nodes['Background'].inputs['Color'].default_value = (0.62, 0.66, 0.72, 1)
    world.node_tree.nodes['Background'].inputs['Strength'].default_value = 0.9
    sol = bpy.data.objects.new('Sol', bpy.data.lights.new('Sol', 'SUN'))
    sol.data.energy = 3.2
    sol.data.angle = math.radians(8)
    sol.rotation_euler = (math.radians(50), math.radians(-10), math.radians(-35))
    scene.collection.objects.link(sol)
    chao_me = bpy.data.meshes.new('Chao')
    bmc = bmesh.new()
    bmesh.ops.create_grid(bmc, x_segments=1, y_segments=1, size=4)
    bmc.to_mesh(chao_me)
    chao = bpy.data.objects.new('Chao', chao_me)
    mc = bpy.data.materials.new('ChaoMat')
    mc.use_nodes = True
    mc.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value = hexcol('#8C9096')
    chao_me.materials.append(mc)
    scene.collection.objects.link(chao)

    scene.render.engine = 'BLENDER_EEVEE'
    scene.render.resolution_x, scene.render.resolution_y = 720, 900
    scene.render.film_transparent = False
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

    def render(nome, acao, frame):
        arm.animation_data.action = bpy.data.actions[acao]
        scene.frame_set(frame)
        scene.render.filepath = os.path.join(OUT, nome + '.png')
        bpy.ops.render.render(write_still=True)
        print('RENDER', nome)

    olhar((1.5, -2.7, 1.35), (0, 0, 0.92))
    render('previa_frente', 'Idle', 0)
    olhar((-1.4, 2.7, 1.45), (0, 0, 0.95))
    render('previa_costas', 'Idle', 0)
    olhar((3.2, -0.3, 1.0), (0, -0.05, 0.85))
    render('previa_andando', 'Walk', 3)
    render('previa_correndo', 'Run', 4)

    # visão em primeira pessoa: câmera pivota no pescoço, como a cabeça (mesmo modelo do CameraJogador)
    scene.render.resolution_x, scene.render.resolution_y = 1280, 720
    cam.data.sensor_fit = 'VERTICAL'
    cam.data.angle = math.radians(60)
    cam.data.clip_start = 0.03
    PIVO = Vector((0.0, 0.0, 1.50))
    OLHO = Vector((0.0, -0.10, 0.13))  # frente, cima (em relação ao pivô, olhando reto)
    for acao, fr, pitch, nome in (('Walk', 2, 35, 'previa_fps_35'), ('Walk', 2, 62, 'previa_fps_62'),
                                  ('Idle', 0, 75, 'previa_fps_75')):
        arm.animation_data.action = bpy.data.actions[acao]
        scene.frame_set(fr)
        cabeca.hide_render = True  # no jogo: Shadows Only
        bpy.context.view_layer.update()
        cam.location = PIVO + Rx(pitch) @ OLHO
        cam.rotation_euler = (math.radians(90 - pitch), 0, math.pi)
        scene.render.filepath = os.path.join(OUT, nome + '.png')
        bpy.ops.render.render(write_still=True)
        print('RENDER', nome)
    cabeca.hide_render = False

    # close da barra do colete (conferir furos/normais)
    scene.render.resolution_x, scene.render.resolution_y = 900, 700
    cam.data.sensor_fit = 'AUTO'
    arm.animation_data.action = bpy.data.actions['Idle']
    scene.frame_set(0)
    olhar((0.35, -0.9, 1.05), (0.0, 0.0, 1.05), 50)
    scene.render.filepath = os.path.join(OUT, 'previa_close_colete.png')
    bpy.ops.render.render(write_still=True)
    print('RENDER close')
