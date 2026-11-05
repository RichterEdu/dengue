"""
Gera todos os sons do Foco Zero sintetizando do zero, só com numpy.
Nada é baixado ou gravado: cada som é uma receita neste arquivo.

Uso:
  python -X utf8 gerar_sons.py <pasta_saida> [--so nome1,nome2] [--listar]

  ex.: python -X utf8 Ferramentas/Audio/gerar_sons.py "Assets/Meus/Assets Gerados/Sons"

Convenções:
  - WAV 16 bits. Efeitos em 44,1 kHz mono; loops longos (ambiente, chuva
    e trilha) em 32 kHz estéreo, pro repositório não inchar.
  - Cada som tem semente fixa (crc32 do nome): regerar dá o MESMO arquivo.
    Sobrescrever o .wav mantém o .meta do Unity, então as referências
    da cena não quebram.
  - Loops (*_loop, trilha_*) são montados em linha do tempo CIRCULAR:
    ruído e filtros por FFT, eventos que passam do fim voltam pro começo,
    tons contínuos com número inteiro de ciclos. Não precisam de crossfade.
  - As 4 camadas da trilha (trilha_base/pulso/tensao/esperanca) têm
    exatamente o mesmo número de amostras: o Unity toca as 4 juntas.
  - O prefixo do nome decide a importação no Unity (ImportadorDeSons.cs):
    mosquito_/foco_/passaro_/latido_/passo_/esvaziar/pneu_picape/suspensao/
    lixeira_ viram mono (som 3D).
"""
import argparse
import os
import sys
import wave
import zlib

import numpy as np

SR = 44100          # efeitos
SR_LONGO = 32000    # loops longos (ambiente e trilha)

BPM = 80.0
BATIDA = 60.0 / BPM             # 0,75 s
DUR_TRILHA = 32 * BATIDA        # 8 compassos 4/4 = 24 s


# ================================================================ básicos

def db(v):
    return 10.0 ** (v / 20.0)


def amostras(dur, sr):
    return int(round(dur * sr))


def tempo(n, sr):
    return np.arange(n) / sr


def midi(m):
    return 440.0 * 2.0 ** ((m - 69) / 12.0)


def suave(x):
    """Smoothstep de 0 a 1 (x fora de [0,1] é cortado)."""
    x = np.clip(x, 0.0, 1.0)
    return x * x * (3.0 - 2.0 * x)


def ruido(rng, n, cor="branco", canais=1):
    """Ruído de desvio 1. Rosa/marrom moldados por FFT (circular: já emenda)."""
    x = rng.standard_normal((canais, n))
    if cor != "branco":
        X = np.fft.rfft(x, axis=-1)
        f = np.arange(X.shape[-1], dtype=float)
        f[0] = 1.0
        X = X / (np.sqrt(f) if cor == "rosa" else f)
        X[..., 0] = 0.0
        x = np.fft.irfft(X, n, axis=-1)
    x = x / (np.std(x, axis=-1, keepdims=True) + 1e-12)
    return x[0] if canais == 1 else x


# ================================================================ filtros (FFT, fase zero)

def _ganho_passa(f, lo, hi, ordem):
    g = np.ones_like(f)
    if hi:
        g = g / np.sqrt(1.0 + (f / hi) ** (2 * ordem))
    if lo:
        pos = f > 0
        r = np.zeros_like(f)
        r[pos] = (lo / f[pos]) ** (2 * ordem)
        g = np.where(pos, g / np.sqrt(1.0 + r), 0.0)
    return g


def _aplicar(x, sr, ganho_de, circular):
    x = np.asarray(x, dtype=float)
    n = x.shape[-1]
    m = n if circular else n + min(n, amostras(0.25, sr))
    X = np.fft.rfft(x, m, axis=-1)
    f = np.fft.rfftfreq(m, 1.0 / sr)
    return np.fft.irfft(X * ganho_de(f), m, axis=-1)[..., :n]


def filtrar(x, sr, lo=None, hi=None, ordem=2, circular=False):
    """Passa-altas (lo) e/ou passa-baixas (hi), resposta tipo Butterworth.
    Filtre o RUÍDO e aplique o envelope depois (fase zero toca antes do ataque)."""
    return _aplicar(x, sr, lambda f: _ganho_passa(f, lo, hi, ordem), circular)


def banda(x, sr, fc, q, circular=False):
    """Passa-banda ressonante centrado em fc."""
    def ganho(f):
        g = np.zeros_like(f)
        pos = f > 0
        g[pos] = 1.0 / np.sqrt(1.0 + q * q * (f[pos] / fc - fc / f[pos]) ** 2)
        return g
    return _aplicar(x, sr, ganho, circular)


# ================================================================ envelopes e osciladores

def env_ad(n, sr, ataque, queda, atraso=0.0):
    """Sobe linear em `ataque` s, cai exponencial com constante `queda` s."""
    t = tempo(n, sr) - atraso
    sobe = t / max(ataque, 1e-6)
    return np.where(t < 0, 0.0, np.where(t < ataque, sobe, np.exp(-np.maximum(t - ataque, 0.0) / queda)))


def env_trapezio(n, sr, ataque, dur, soltura, atraso=0.0):
    """Sobe em `ataque`, segura até `dur`, some em `soltura` (tudo suave)."""
    t = tempo(n, sr) - atraso
    return suave(t / max(ataque, 1e-6)) * suave((dur + soltura - t) / max(soltura, 1e-6))


def fase(freq, n, sr, fase0=0.0):
    freq = np.broadcast_to(np.asarray(freq, dtype=float), (n,))
    return fase0 + 2.0 * np.pi * np.cumsum(freq) / sr


def harmonicos(freq, n, sr, amps, fase0=0.0):
    """Soma de harmônicos (amps[k-1] no k-ésimo); corta o que passar do Nyquist."""
    freq = np.broadcast_to(np.asarray(freq, dtype=float), (n,))
    ph = fase(freq, n, sr, fase0)
    y = np.zeros(n)
    for k, a in enumerate(amps, start=1):
        if a:
            y += a * np.sin(k * ph) * (k * freq < 0.45 * sr)
    return y


def modos(n, sr, rng, freqs, quedas, amps, atraso=0.0):
    """Síntese modal: senos que decaem (batidas em plástico, vidro, metal)."""
    t = tempo(n, sr) - atraso
    ok = t >= 0
    y = np.zeros(n)
    for f, q, a in zip(freqs, quedas, amps):
        if f < 0.45 * sr:
            y += a * np.sin(2 * np.pi * f * t + rng.uniform(0, 2 * np.pi)) * np.exp(-np.maximum(t, 0.0) / q) * ok
    # some nos últimos 30% do buffer: sem isso, um modo que ainda soa é cortado seco (estalo)
    fim = n / sr
    return y * np.clip(t / 0.0004, 0, 1) * suave((fim - tempo(n, sr)) / (0.3 * fim))


def varredura(n, sr, f_ini, f_fim, atraso=0.0, constante=0.03):
    """Frequência que vai de f_ini a f_fim exponencialmente (bumbo, batimento)."""
    t = np.maximum(tempo(n, sr) - atraso, 0.0)
    return f_fim + (f_ini - f_fim) * np.exp(-t / constante)


# ================================================================ espaço

def reverb(x, sr, rng, duracao=1.0, mistura=0.2, brilho=6000.0, pre=0.012, circular=False):
    """Convolução com ruído estéreo decaindo (-60 dB em `duracao`). Devolve estéreo.
    Não circular: o resultado fica mais longo (cauda)."""
    x = np.asarray(x, dtype=float)
    seco = x if x.ndim == 2 else np.vstack([x, x])
    n = seco.shape[-1]
    n_ir = amostras(duracao, sr)
    ir = rng.standard_normal((2, n_ir)) * np.exp(-6.91 * tempo(n_ir, sr) / duracao)
    ir = filtrar(ir, sr, hi=brilho, ordem=1)
    ir[:, :amostras(pre, sr)] = 0.0
    ir /= np.sqrt(np.sum(ir ** 2, axis=-1, keepdims=True)) + 1e-12
    if circular:
        if n_ir > n:
            raise ValueError("reverb circular maior que o loop")
        molhado = np.fft.irfft(np.fft.rfft(seco, n, axis=-1) * np.fft.rfft(ir, n, axis=-1), n, axis=-1)
        return seco + mistura * molhado
    m = n + n_ir
    molhado = np.fft.irfft(np.fft.rfft(seco, m, axis=-1) * np.fft.rfft(ir, m, axis=-1), m, axis=-1)
    return np.pad(seco, ((0, 0), (0, n_ir))) + mistura * molhado


def mono(x):
    return x if x.ndim == 1 else x.mean(axis=0)


def panear(x, p):
    """Pan de potência constante, p de -1 (esquerda) a 1 (direita)."""
    a = (np.clip(p, -1, 1) + 1.0) * np.pi / 4.0
    return np.vstack([x * np.cos(a), x * np.sin(a)])


def somar(dest, som, pos, circular=False):
    """Soma `som` em `dest` a partir da amostra `pos`. Circular: o que passa do fim volta."""
    if dest.ndim == 2 and som.ndim == 1:
        som = np.vstack([som, som])
    n = dest.shape[-1]
    m = som.shape[-1]
    if not circular:
        fim = min(n, pos + m)
        if fim > pos:
            dest[..., pos:fim] += som[..., :fim - pos]
        return
    feito = 0
    while feito < m:
        p = (pos + feito) % n
        k = min(m - feito, n - p)
        dest[..., p:p + k] += som[..., feito:feito + k]
        feito += k


# ================================================================ nível e acabamento

def pico(x):
    return float(np.max(np.abs(x)))


def rms(x):
    return float(np.sqrt(np.mean(np.asarray(x) ** 2)))


def nivelar(x, pico_db=None, rms_db=None, pico_max_db=-1.0):
    g = db(rms_db) / (rms(x) + 1e-12) if rms_db is not None else db(pico_db) / (pico(x) + 1e-12)
    g = min(g, db(pico_max_db) / (pico(x) + 1e-12))
    return x * g


def aparar(x, sr, limiar_db=-60.0, folga=0.02):
    """Corta o silêncio do fim e faz fades curtos nas pontas (efeitos avulsos)."""
    env = np.abs(x) if x.ndim == 1 else np.max(np.abs(x), axis=0)
    idx = np.nonzero(env > db(limiar_db) * env.max())[0]
    fim = min(len(env), (idx[-1] if len(idx) else len(env)) + amostras(folga, sr))
    y = x[..., :fim].copy()
    k = min(amostras(0.006, sr), fim)
    y[..., fim - k:] *= np.linspace(1.0, 0.0, k)
    k = min(amostras(0.0005, sr), fim)
    y[..., :k] *= np.linspace(0.0, 1.0, k)
    return y


def efeito(x, sr=SR, pico_db=-3.0):
    return nivelar(aparar(x, sr), pico_db=pico_db)


# ================================================================ timbres reutilizados

def sino(f, sr, rng, dur=1.2, brilho=1.0):
    n = amostras(dur, sr)
    return modos(n, sr, rng, [f, 2.0 * f, 3.0 * f, 4.17 * f],
                 [0.55 * dur, 0.25 * dur, 0.12 * dur, 0.06 * dur],
                 [1.0, 0.45 * brilho, 0.25 * brilho, 0.12 * brilho])


def marimba(f, sr, rng, dur=1.6):
    n = amostras(dur, sr)
    return modos(n, sr, rng, [f, 3.93 * f, 9.24 * f], [0.34 * dur, 0.08 * dur, 0.025 * dur], [1.0, 0.25, 0.07])


def pluck(f, sr, rng, queda=0.14):
    n = amostras(queda * 5, sr)
    t = tempo(n, sr)
    y = np.zeros(n)
    for k in range(1, 9):
        if k * f < 0.45 * sr:
            y += (1.0 / k ** 1.1) * np.sin(2 * np.pi * k * f * t + rng.uniform(0, 6.28)) * np.exp(-t / (queda / k ** 0.6))
    return y * np.clip(t / 0.002, 0, 1)


def nota_pad(m, dur, sr, rng, brilho=1800.0, ataque=1.0, soltura=1.5, harm=16):
    """Nota de pad estéreo: 3 vozes desafinadas, harmônicos já 'filtrados' em brilho."""
    f = midi(m)
    n = amostras(dur + soltura, sr)
    t = tempo(n, sr)
    env = suave(t / ataque) * suave((dur + soltura - t) / soltura)
    amps = [(1.0 / k ** 1.4) / (1.0 + (k * f / brilho) ** 4) for k in range(1, harm + 1)]
    saida = np.zeros((2, n))
    for cents, p in ((-7, -0.6), (0, 0.0), (7, 0.6)):
        fv = f * 2 ** (cents / 1200.0) * (1 + 0.0025 * np.sin(2 * np.pi * rng.uniform(0.15, 0.4) * t + rng.uniform(0, 6.28)))
        saida += panear(harmonicos(fv, n, sr, amps, rng.uniform(0, 6.28)), p)
    return saida * env / 3.0


def ruido_env(rng, n, sr, lo, hi, ataque, queda, atraso=0.0, ordem=2):
    return filtrar(ruido(rng, n), sr, lo=lo, hi=hi, ordem=ordem) * env_ad(n, sr, ataque, queda, atraso)


def granulos(rng, n, sr, taxa, inicio, dur):
    """Impulsos soltos (grãos de areia, estalos, cascalho)."""
    x = np.zeros(n)
    a, b = amostras(inicio, sr), min(n, amostras(inicio + dur, sr))
    k = max(1, int(taxa * dur))
    idx = rng.integers(a, max(a + 1, b), k)
    np.add.at(x, idx, rng.standard_normal(k) * rng.uniform(0.2, 1.0, k))
    return x


def whoosh(rng, n, sr, f_ini, f_fim, inicio, dur, bandas=12, largura=0.35):
    """Ruído com uma banda que 'varre' de f_ini a f_fim."""
    t = tempo(n, sr)
    base = ruido(rng, n)
    y = np.zeros(n)
    centros = np.geomspace(f_ini, f_fim, bandas)
    for i, fc in enumerate(centros):
        meio = inicio + dur * i / (bandas - 1)
        y += banda(base, sr, fc, 2.0) * np.exp(-((t - meio) / (dur * largura)) ** 2)
    return y


# ================================================================ FOCOS

def s_mosquito(rng):
    """Poucos mosquitos (3), timbre suave: harmônicos caindo rápido e passa-baixas.
    6 s; frequências e LFOs com ciclos inteiros = loop perfeito.
    (Versão 1 tinha 5 insetos bem agudos: o usuário achou mosquito demais.)"""
    L = 6.0
    n = amostras(L, SR)
    t = tempo(n, SR)
    amps = [1, .5, .28, .16, .09, .05]
    y = np.zeros(n)
    for _ in range(3):
        f0 = round(rng.uniform(470, 600) * L) / L
        lfo = np.zeros(n)
        for k, a in ((1, 0.03), (2, 0.02), (5, 0.012), (11, 0.006)):
            lfo += rng.uniform(0.4, 1.0) * a * np.sin(2 * np.pi * k * t / L + rng.uniform(0, 6.28))
        am = 1.0 + 0.25 * np.sin(2 * np.pi * rng.choice([1, 2, 3]) * t / L + rng.uniform(0, 6.28)) \
                 + 0.1 * np.sin(2 * np.pi * rng.choice([7, 9, 13]) * t / L + rng.uniform(0, 6.28))
        y += rng.uniform(0.6, 1.0) * am * harmonicos(f0 * (1 + lfo), n, SR, amps, rng.uniform(0, 6.28))
    y = filtrar(y, SR, lo=250, hi=3500, circular=True)
    return nivelar(y, pico_db=-3)


def bolha(rng, f0, tau):
    """Bolha que estoura (Minnaert): seno curto cuja frequência sobe."""
    n = amostras(tau * 6, SR)
    t = tempo(n, SR)
    f = f0 * (1 + rng.uniform(0.08, 0.22) * t / tau)
    return np.sin(fase(f, n, SR)) * np.exp(-t / tau) * np.clip(t / 0.0008, 0, 1)


def s_agua(rng, v):
    dur, nb = [(0.45, 28), (0.55, 36), (0.8, 55)][v]
    n = amostras(dur + 0.25, SR)
    y = np.zeros(n)
    for _ in range(nb):
        f0 = np.exp(rng.uniform(np.log(350), np.log(1700)))
        tau = float(np.clip(9.0 / f0 * rng.uniform(0.8, 1.6), 0.004, 0.03))
        somar(y, bolha(rng, f0, tau) * rng.uniform(0.2, 1.0) * (f0 / 800) ** -0.3, amostras(dur * rng.beta(1.3, 3.0), SR))
    y += 0.35 * ruido_env(rng, n, SR, 500, 4500, 0.004, 0.07)       # respingo
    y += 0.5 * ruido_env(rng, n, SR, 120, 600, 0.01, 0.12)          # "glub" grave
    if v == 2:  # derramando
        y += 0.3 * filtrar(ruido(rng, n), SR, 800, 3000) * env_trapezio(n, SR, 0.05, 0.35, 0.25, 0.1)
    return efeito(mono(reverb(y, SR, rng, 0.35, 0.12, 5000)))


def s_areia(rng, v):
    dur = 0.55 + 0.15 * v
    n = amostras(dur + 0.3, SR)
    graos = filtrar(granulos(rng, n, SR, 2600, 0.0, dur), SR, 1500, 9000)
    graos /= np.std(graos) + 1e-12
    shh = filtrar(ruido(rng, n), SR, 300, 1600)
    trem = filtrar(ruido(rng, n), SR, hi=12, ordem=1)
    trem = 0.8 + 0.2 * trem / (np.std(trem) + 1e-12)
    y = (graos + 0.35 * shh) * env_trapezio(n, SR, 0.06, dur - 0.2, 0.2) * np.clip(trem, 0.3, 1.3)
    return efeito(mono(reverb(y, SR, rng, 0.3, 0.08, 6000)))


def s_tampa(rng, v):
    n = amostras(0.7, SR)
    t = tempo(n, SR)
    raspa = filtrar(ruido(rng, n), SR, 700, 3500) * env_trapezio(n, SR, 0.02, 0.08, 0.04) * (1 + 0.5 * np.sin(2 * np.pi * 38 * t)) * 0.25
    esc = rng.uniform(0.9, 1.1)
    fs = [150 * esc, 355 * esc, 640 * esc, 1020 * esc, 1650 * esc]
    qs = [0.09, 0.07, 0.05, 0.035, 0.02]
    am = [1, .7, .45, .3, .15]
    ti = 0.13
    corpo = modos(n, SR, rng, fs, qs, am, ti)
    quique = 0.3 * modos(n, SR, rng, [f * 1.02 for f in fs], qs, am, ti + rng.uniform(0.05, 0.07))
    clique = 0.6 * ruido_env(rng, n, SR, 1500, None, 0.0005, 0.006, ti)
    return efeito(mono(reverb(raspa + corpo + quique + clique, SR, rng, 0.4, 0.1, 5000)))


def s_garrafa(rng, v):
    n = amostras(0.9, SR)
    if v < 2:  # vidro
        y = np.zeros(n)
        for atraso, forca in ((0.01, 1.0), (rng.uniform(0.1, 0.14), 0.6)):
            b = rng.uniform(1900, 2600)
            y += forca * modos(n, SR, rng, [b, 1.72 * b, 2.51 * b, 3.3 * b, 4.1 * b],
                               [0.28, 0.18, 0.12, 0.08, 0.05], [1, .6, .5, .3, .2], atraso)
            y += forca * 0.5 * modos(n, SR, rng, [rng.uniform(700, 900)], [0.04], [1], atraso)
            y += forca * 0.3 * ruido_env(rng, n, SR, 3000, None, 0.0003, 0.003, atraso)
    else:  # garrafa PET amassando
        y = filtrar(granulos(rng, n, SR, 450, 0.02, 0.3), SR, 2000, 9000)
        y /= np.std(y) + 1e-12
        y = y * env_trapezio(n, SR, 0.02, 0.22, 0.08, 0.02) + 2.0 * modos(n, SR, rng, [420, 980], [0.03, 0.02], [1, .5], 0.03)
    return efeito(mono(reverb(y, SR, rng, 0.4, 0.1, 7000)))


def s_metal(rng, v):
    n = amostras(1.4, SR)
    t = tempo(n, SR)
    raspa = filtrar(ruido(rng, n), SR, 1500, 6000) * env_trapezio(n, SR, 0.02, 0.1, 0.05) * (1 + 0.6 * np.sin(2 * np.pi * 55 * t)) * 0.3
    b = rng.uniform(270, 340)
    fs = [b, 2.63 * b, 4.72 * b, 7.36 * b, 10.3 * b]
    qs = [0.45, 0.32, 0.22, 0.14, 0.09]
    am = [1, .75, .55, .35, .2]
    ti = 0.15
    y = raspa + modos(n, SR, rng, fs, qs, am, ti) + 0.8 * modos(n, SR, rng, [95], [0.05], [1], ti)
    if v == 1:
        y += 0.35 * modos(n, SR, rng, [f * 1.01 for f in fs], qs, am, ti + 0.09)
    return efeito(mono(reverb(y, SR, rng, 0.5, 0.12, 6000)))


def s_eliminado(rng):
    n = amostras(1.4, SR)
    y = 0.5 * whoosh(rng, n, SR, 300, 7000, 0.05, 0.25, largura=0.3)
    for atraso, m in ((0.16, 86), (0.24, 90), (0.32, 93)):   # Ré maior subindo: D6 F#6 A6
        somar(y, 0.5 * sino(midi(m), SR, rng, 0.9), amostras(atraso, SR))
    return efeito(mono(reverb(y, SR, rng, 1.0, 0.18, 7000)))


# ================================================================ HOSPITAL

def bip(f, dur, sr):
    n = amostras(dur + 0.03, sr)
    t = tempo(n, sr)
    env = suave(t / 0.004) * suave((dur + 0.03 - t) / 0.03)
    return (np.sin(2 * np.pi * f * t) + 0.18 * np.sin(4 * np.pi * f * t) + 0.06 * np.sin(6 * np.pi * f * t)) * env


def s_alerta(rng):
    y = np.zeros(amostras(0.6, SR))
    for atraso in (0.0, 0.19):
        somar(y, bip(880, 0.1, SR), amostras(atraso, SR))
    return efeito(mono(reverb(y, SR, rng, 0.4, 0.15, 6000)))


def s_desce(rng):
    y = np.zeros(amostras(1.0, SR))
    somar(y, 0.8 * sino(midi(74), SR, rng, 0.8, 0.6), 0)
    somar(y, sino(midi(81), SR, rng, 0.9, 0.6), amostras(0.13, SR))
    return efeito(mono(reverb(y, SR, rng, 0.8, 0.2, 6000)))


def s_vitoria(rng):
    n = amostras(5.0, SR)
    y = np.zeros((2, n))
    for i, m in enumerate((74, 78, 81, 86)):    # D5 F#5 A5 D6
        somar(y, panear(0.6 * sino(midi(m), SR, rng, 1.6), -0.4 + 0.27 * i), amostras(0.12 * i, SR))
    somar(y, panear(0.35 * sino(midi(93), SR, rng, 1.8), 0.2), amostras(0.55, SR))
    for m in (62, 66, 69, 74):                  # acorde de Ré maior
        somar(y, 0.5 * nota_pad(m, 2.3, SR, rng, 2200, 0.5, 1.6), amostras(0.3, SR))
    return efeito(reverb(y, SR, rng, 2.0, 0.3, 6000))


def s_derrota(rng):
    n = amostras(5.5, SR)
    y = np.zeros((2, n))
    boom = np.sin(fase(varredura(n, SR, 55, 32, 0, 0.35), n, SR)) * env_ad(n, SR, 0.005, 0.9)
    boom += 0.5 * filtrar(ruido(rng, n), SR, hi=150) * env_ad(n, SR, 0.01, 1.2)
    somar(y, boom, 0)
    for atraso, m in ((0.3, 69), (0.9, 65), (1.5, 62)):      # A4 F4 D4 descendo
        somar(y, 0.45 * nota_pad(m, 0.9, SR, rng, 1500, 0.08, 1.2), amostras(atraso, SR))
    for m in (50, 53, 57):                                   # Ré menor grave segurando
        somar(y, 0.4 * nota_pad(m, 2.2, SR, rng, 1200, 0.4, 1.4), amostras(1.5, SR))
    tom = bip(988, 3.0, SR) * np.linspace(1, 0, amostras(3.03, SR)) ** 1.5 * 0.12
    somar(y, panear(tom, 0.3), amostras(1.8, SR))
    return efeito(reverb(y, SR, rng, 2.5, 0.35, 5000))


# ================================================================ JOGADOR

def pisada_rua(rng, n, atraso, forca, brilho):
    """Tênis no asfalto/terra, abafado: baque grave + corpo curto + um raspão
    baixinho só em médio-grave. Sem ruído agudo (a versão 1, com "cascalho"
    de 2,5–8 kHz, soava como chiado para o usuário)."""
    esc = rng.uniform(0.9, 1.1)
    baque = 0.45 * np.sin(fase(varredura(n, SR, 90 * esc, 60 * esc, atraso, 0.02), n, SR)) * env_ad(n, SR, 0.002, 0.025, atraso)
    corpo = modos(n, SR, rng, [180 * esc * brilho, 340 * esc * brilho, 720 * esc * brilho],
                  [0.02, 0.014, 0.01], [0.6, 0.45, 0.28], atraso)
    raspao = 0.45 * ruido_env(rng, n, SR, 250, 1200, 0.003, 0.02, atraso)
    return forca * (baque + corpo + raspao)


def s_passo_rua(rng, v):
    n = amostras(0.3, SR)
    y = pisada_rua(rng, n, 0.0, 1.0, 1.0) + pisada_rua(rng, n, rng.uniform(0.05, 0.07), rng.uniform(0.4, 0.55), 1.15)
    return efeito(filtrar(y, SR, lo=40, hi=2000))


def s_passo_piso(rng, v):
    """Piso do hospital: mais definido que a rua, mas sem estalo agudo."""
    n = amostras(0.35, SR)
    y = np.zeros(n)
    for atraso, forca in ((0.0, 1.0), (rng.uniform(0.045, 0.065), 0.5)):
        esc = rng.uniform(0.92, 1.08)
        y += forca * modos(n, SR, rng, [700 * esc, 1050 * esc, 1400 * esc], [0.018, 0.012, 0.008], [0.5, 0.3, 0.15], atraso)
        y += forca * np.sin(fase(varredura(n, SR, 140, 90, atraso, 0.02), n, SR)) * env_ad(n, SR, 0.002, 0.025, atraso)
        y += forca * 0.15 * ruido_env(rng, n, SR, 300, 1500, 0.001, 0.008, atraso)
    y = filtrar(y, SR, lo=40, hi=3000)
    return efeito(mono(reverb(y, SR, rng, 0.3, 0.08, 3000)))


def s_pulo(rng):
    n = amostras(0.4, SR)
    pano = filtrar(ruido(rng, n), SR, 500, 3000) * env_ad(n, SR, 0.05, 0.09)
    folego = 0.4 * banda(ruido(rng, n), SR, 650, 1.5) * env_ad(n, SR, 0.02, 0.12, 0.02)
    return efeito(pano + folego)


def s_aterrissagem(rng):
    n = amostras(0.5, SR)
    baque = np.sin(fase(varredura(n, SR, 85, 40, 0, 0.05), n, SR)) * env_ad(n, SR, 0.003, 0.11)
    y = 0.7 * baque + pisada_rua(rng, n, 0.0, 1.0, 1.0) + pisada_rua(rng, n, 0.015, 0.9, 1.1)
    y += 0.4 * ruido_env(rng, n, SR, None, 800, 0.003, 0.08)
    return efeito(filtrar(y, SR, lo=30, hi=2000))


# ================================================================ CELULAR E INTERFACE

def s_notificacao(rng):
    n = amostras(0.7, SR)
    f = 520 + (1150 - 520) * (1 - np.exp(-tempo(n, SR) / 0.012))
    pop = harmonicos(f, n, SR, [1, 0.2]) * env_ad(n, SR, 0.001, 0.03)
    y = pop
    somar(y, 0.7 * sino(midi(91), SR, rng, 0.6, 0.5), amostras(0.06, SR))   # G6
    return efeito(mono(reverb(y, SR, rng, 0.4, 0.1, 8000)))


def s_celular(rng, abrir):
    n = amostras(0.35, SR)
    f_ini, f_fim = (900, 7000) if abrir else (7000, 900)
    y = 0.6 * whoosh(rng, n, SR, f_ini, f_fim, 0.02, 0.13, bandas=10, largura=0.4)
    y += 0.35 * modos(n, SR, rng, [3200], [0.008], [1], 0.15 if abrir else 0.015)
    return efeito(y)


def s_ui_clique(rng):
    n = amostras(0.08, SR)
    y = modos(n, SR, rng, [2400, 4100], [0.012, 0.006], [1, 0.4]) + 0.3 * ruido_env(rng, n, SR, 3000, None, 0.0003, 0.003)
    return efeito(y)


# ================================================================ KIT DO AGENTE (Parte A, 2D)
# Cada batida é gerada no seu próprio buffer curto e somada no lugar: o
# modos() apaga os últimos 30% do buffer, e uma batida tardia num buffer
# longo sairia abafada.

def batida_plastico(rng, forca=1.0, esc=1.0):
    """Batida curta em plástico duro (caixa do kit, frasco): poucos modos + estalo."""
    n = amostras(0.12, SR)
    fs = [620 * esc, 1430 * esc, 2350 * esc, 3600 * esc]
    y = modos(n, SR, rng, fs, [0.035, 0.022, 0.012, 0.006], [1, .6, .35, .15])
    y += 0.4 * ruido_env(rng, n, SR, 1500, 7000, 0.0003, 0.003)
    return forca * y


def baque_borracha(rng, forca=1.0, grave=1.0):
    """Borracha no chão: 'tump' grave que desce + corpo abafado, sem agudo."""
    n = amostras(0.16, SR)
    esc = grave * rng.uniform(0.95, 1.05)
    tump = np.sin(fase(varredura(n, SR, 135 * esc, 70 * esc, 0.0, 0.025), n, SR)) * env_ad(n, SR, 0.003, 0.045)
    corpo = modos(n, SR, rng, [230 * esc, 520 * esc, 840 * esc], [0.03, 0.018, 0.01], [0.55, 0.3, 0.12])
    poeira = 0.3 * ruido_env(rng, n, SR, 200, 1200, 0.002, 0.03)
    return forca * (tump + corpo + poeira)


def s_kit_trocar(rng):
    """Troca de ferramenta: clique de plástico (trava + encaixe), ~80 ms."""
    y = np.zeros(amostras(0.08, SR))
    for atraso, forca in ((0.0, 1.0), (0.022, 0.55)):
        n = amostras(0.05, SR)
        esc = rng.uniform(0.95, 1.05)
        c = modos(n, SR, rng, [1250 * esc, 2700 * esc, 4300 * esc], [0.010, 0.006, 0.003], [1, 0.5, 0.25])
        c += 0.35 * ruido_env(rng, n, SR, 2000, 8000, 0.0003, 0.002)
        somar(y, forca * c, amostras(atraso, SR))
    return efeito(y)


def s_kit_erro(rng):
    """Recusa (ferramenta errada, sem carga, saco cheio): dois bipes graves descendo, ~250 ms."""
    y = np.zeros(amostras(0.25, SR))
    for atraso, f, dur in ((0.0, 330.0, 0.08), (0.12, 247.0, 0.1)):   # E4 → B3
        somar(y, bip(f, dur, SR), amostras(atraso, SR))
    return efeito(filtrar(y, SR, hi=2500))


def s_kit_reabastecer(rng):
    """Kit cheio de novo: zíper da bolsa + 3 batidas de plástico (cargas entrando), ~600 ms."""
    y = np.zeros(amostras(0.62, SR))
    dur = 0.26
    n = amostras(dur, SR)
    t = tempo(n, SR)
    dentes = (0.5 + 0.5 * np.sin(fase(110 + 70 * t / dur, n, SR))) ** 6   # dentes cada vez mais rápidos
    chiado = filtrar(ruido(rng, n), SR, 1800, 7000)
    somar(y, 0.8 * chiado * (0.25 + 0.75 * dentes) * env_trapezio(n, SR, 0.02, dur - 0.04, 0.04), 0)
    for atraso, forca, esc in ((0.31, 1.0, 1.0), (0.41, 0.8, 1.08), (0.50, 0.9, 0.95)):
        somar(y, batida_plastico(rng, forca, esc * rng.uniform(0.97, 1.03)), amostras(atraso, SR))
    return efeito(y)


def s_cacamba_despejar(rng):
    """Saco de garrafas despejado: baque grave + metal oco, ~700 ms."""
    n = amostras(0.7, SR)
    baque = np.sin(fase(varredura(n, SR, 95, 42, 0.0, 0.04), n, SR)) * env_ad(n, SR, 0.003, 0.09)
    baque += 0.4 * ruido_env(rng, n, SR, None, 500, 0.002, 0.05)
    b = rng.uniform(150, 175)
    fs = [b, 2.31 * b, 3.87 * b, 5.62 * b, 8.1 * b]
    oco = modos(n, SR, rng, fs, [0.22, 0.16, 0.11, 0.07, 0.04], [1, .7, .45, .3, .15], 0.008)
    tranco = 0.35 * ruido_env(rng, n, SR, 800, 4000, 0.001, 0.02, 0.008)
    return efeito(filtrar(0.9 * baque + 0.7 * oco + tranco, SR, lo=30, hi=6000))


def s_pneu_pegar(rng):
    """Pegar o pneu: borracha abafada, um 'tump' e o atrito das mãos, ~250 ms."""
    n = amostras(0.25, SR)
    tump = np.sin(fase(varredura(n, SR, 150, 95, 0.02, 0.03), n, SR)) * env_ad(n, SR, 0.004, 0.04, 0.02)
    corpo = modos(n, SR, rng, [210, 470, 760], [0.03, 0.02, 0.012], [0.6, 0.35, 0.15], 0.02)
    atrito = ruido_env(rng, n, SR, 250, 1400, 0.03, 0.05)
    return efeito(filtrar(0.9 * tump + corpo + 0.5 * atrito, SR, lo=50, hi=2500))


def s_pneu_largar(rng):
    """Pneu largado no chão: baque de borracha + quiques cada vez menores, ~400 ms."""
    y = np.zeros(amostras(0.42, SR))
    for atraso, forca, grave in ((0.0, 1.0, 1.0), (0.17, 0.45, 1.08), (0.28, 0.18, 1.12)):
        somar(y, baque_borracha(rng, forca, grave), amostras(atraso, SR))
    return efeito(filtrar(y, SR, lo=40, hi=2200))


def s_kit_segurar_loop(rng):
    """Mãos trabalhando enquanto segura (qualquer ferramenta): farfalhar suave
    de pano e plástico, sem tom. Loop circular de 2 s: ruído, grãos e tremor
    filtrados por FFT; o gesto tem número inteiro de ciclos."""
    L = 2.0
    n = amostras(L, SR)
    t = tempo(n, SR)
    pano = filtrar(ruido(rng, n, "rosa"), SR, 350, 2500, circular=True)
    pano /= np.std(pano) + 1e-12
    graos = filtrar(granulos(rng, n, SR, 300, 0.0, L), SR, 1500, 6000, circular=True)
    graos /= np.std(graos) + 1e-12
    tremor = filtrar(ruido(rng, n), SR, hi=7, ordem=1, circular=True)
    tremor = np.clip(0.75 + 0.25 * tremor / (np.std(tremor) + 1e-12), 0.3, 1.3)
    gesto = 1 + 0.25 * np.sin(2 * np.pi * 2 * t / L + rng.uniform(0, 6.28))   # 2 movimentos por volta
    y = (pano + 0.25 * graos) * tremor * gesto
    return nivelar(filtrar(y, SR, lo=200, hi=5000, circular=True), pico_db=-3)


# ================================================================ CHUVA, CARGA E DESTINOS (Parte B)
# Mesma regra do kit: batida tardia no seu próprio buffer curto, somada no
# lugar (o modos() apaga os últimos 30% do buffer).

def gota(rng, sr):
    """Uma gota no chão: 'tic' curtíssimo, de bolha mínima que sobe."""
    f0 = np.exp(rng.uniform(np.log(1700), np.log(5200)))
    tau = rng.uniform(0.002, 0.006)
    n = amostras(tau * 6, sr)
    t = tempo(n, sr)
    return np.sin(fase(f0 * (1 + 0.3 * t / tau), n, sr)) * np.exp(-t / tau) * np.clip(t / 0.0004, 0, 1)


def s_chuva_loop(rng):
    """Chuva forte no bairro: ruído rosa (o chiado da chuva e o corpo grave
    dela no chão) com gotas soltas, estéreo. Loop circular de 8 s: filtros por
    FFT, gotas que passam do fim voltam pro começo, onda lenta com ciclos inteiros."""
    L = 8.0
    sr = SR_LONGO
    n = amostras(L, sr)
    t = tempo(n, sr)
    chiado = filtrar(ruido(rng, n, "rosa", 2), sr, 600, 9000, circular=True)
    corpo = filtrar(ruido(rng, n, "rosa", 2), sr, 120, 900, circular=True)
    onda = 1 + 0.1 * np.sin(2 * np.pi * 2 * t / L + rng.uniform(0, 6.28)) + 0.06 * np.sin(2 * np.pi * 5 * t / L + rng.uniform(0, 6.28))
    y = (chiado / np.std(chiado) + 0.55 * corpo / np.std(corpo)) * onda
    gotas = np.zeros((2, n))
    for _ in range(int(90 * L)):
        somar(gotas, panear(gota(rng, sr) * rng.uniform(0.2, 1.0), rng.uniform(-0.9, 0.9)), int(rng.integers(n)), circular=True)
    y += 1.6 * gotas
    return nivelar(filtrar(y, sr, lo=80, hi=11000, circular=True), rms_db=-21)


def s_trovao(rng):
    """Trovão longe: ruído grave com envelope longo e dois roncos, 3 s, estéreo pelo reverb."""
    n = amostras(3.0, SR)
    grave = filtrar(ruido(rng, n, "marrom"), SR, 25, 240)
    grave /= np.std(grave) + 1e-12
    medio = filtrar(ruido(rng, n), SR, 150, 700)
    medio /= np.std(medio) + 1e-12
    tremor = filtrar(ruido(rng, n), SR, hi=9, ordem=1)
    tremor = np.clip(0.8 + 0.35 * tremor / (np.std(tremor) + 1e-12), 0.25, 1.5)
    ronco = env_ad(n, SR, 0.08, 0.75) + 0.7 * env_ad(n, SR, 0.25, 0.5, 0.7)
    y = (grave + 0.18 * medio * env_ad(n, SR, 0.02, 0.35)) * ronco * tremor * suave((3.0 - tempo(n, SR)) / 0.9)
    return efeito(reverb(y, SR, rng, 0.5, 0.25, 900))


def s_esvaziar(rng):
    """Água jogada fora com as mãos: respingo largo + bolhas + 'glub', 0,5 s."""
    dur = 0.3
    n = amostras(0.42, SR)
    y = np.zeros(n)
    for _ in range(42):
        f0 = np.exp(rng.uniform(np.log(400), np.log(2000)))
        tau = float(np.clip(9.0 / f0 * rng.uniform(0.8, 1.6), 0.004, 0.025))
        somar(y, bolha(rng, f0, tau) * rng.uniform(0.2, 1.0), amostras(dur * rng.beta(1.2, 2.2), SR))
    y += 0.9 * ruido_env(rng, n, SR, 700, 6000, 0.012, 0.085)     # respingo
    y += 0.6 * ruido_env(rng, n, SR, 110, 520, 0.012, 0.1)        # "glub" grave
    return efeito(mono(reverb(y, SR, rng, 0.2, 0.1, 5000)))


def s_pneu_picape(rng):
    """Pneu caindo na carroceria: baque de borracha + metal da chapa + um quique, 0,6 s."""
    n = amostras(0.6, SR)
    y = np.zeros(n)
    somar(y, baque_borracha(rng, 1.2, 0.85), 0)
    somar(y, baque_borracha(rng, 0.35, 0.95), amostras(0.2, SR))
    b = rng.uniform(190, 215)
    fs = [b, 2.45 * b, 4.1 * b, 6.3 * b, 9.2 * b]
    y += 0.75 * modos(n, SR, rng, fs, [0.2, 0.15, 0.1, 0.06, 0.035], [1, .75, .5, .3, .15], 0.004)
    y += 0.3 * ruido_env(rng, n, SR, 900, 4500, 0.001, 0.016, 0.004)
    return efeito(filtrar(y, SR, lo=35, hi=6000))


def s_suspensao(rng):
    """Mola da suspensão: dois rangidos curtos, descendo e subindo, 0,45 s."""
    y = np.zeros(amostras(0.45, SR))
    for atraso, f_ini, f_fim, dur, forca in ((0.0, 420, 300, 0.18, 1.0), (0.2, 300, 390, 0.2, 0.65)):
        n = amostras(dur, SR)
        t = tempo(n, SR)
        f = (f_ini + (f_fim - f_ini) * t / dur) * (1 + 0.04 * np.sin(2 * np.pi * 34 * t))
        atrito = 0.75 + 0.25 * np.sign(np.sin(2 * np.pi * 62 * t))   # a mola "trava" e solta
        r = harmonicos(f, n, SR, [1, .7, .5, .4, .3, .22, .15, .1]) * atrito * np.sin(np.pi * t / dur) ** 0.8
        somar(y, forca * r, amostras(atraso, SR))
    return efeito(mono(reverb(filtrar(y, SR, lo=250, hi=3800), SR, rng, 0.15, 0.08, 3500)))


def s_passo_pesado(rng, v):
    """Passo com o pneu nos braços: o mesmo tênis no chão, mais grave, e o baque do peso."""
    n = amostras(0.38, SR)
    esc = rng.uniform(0.94, 1.06)
    baque = np.sin(fase(varredura(n, SR, 80 * esc, 44 * esc, 0.0, 0.03), n, SR)) * env_ad(n, SR, 0.003, 0.065)
    y = 1.1 * baque + pisada_rua(rng, n, 0.0, 1.0, 0.8) + pisada_rua(rng, n, rng.uniform(0.07, 0.095), rng.uniform(0.45, 0.6), 0.9)
    return efeito(filtrar(y, SR, lo=30, hi=1500))


def s_esforco(rng, v):
    """Esforço de carregar peso: só a expiração filtrada (ar pela boca, sem voz)."""
    dur = (0.42, 0.52, 0.36)[v]
    n = amostras(dur + 0.12, SR)
    ar = ruido(rng, n)
    f1, f2 = rng.uniform(520, 680), rng.uniform(1250, 1600)
    y = banda(ar, SR, f1, 2.5) + 0.6 * banda(ar, SR, f2, 3.0) + 0.12 * filtrar(ar, SR, 2500, 5500)
    tremor = filtrar(ruido(rng, n), SR, hi=14, ordem=1)
    tremor = np.clip(0.85 + 0.2 * tremor / (np.std(tremor) + 1e-12), 0.4, 1.3)
    return efeito(y * tremor * env_ad(n, SR, 0.05, dur * 0.4) * suave((dur + 0.12 - tempo(n, SR)) / 0.12))


def batida_vidro(rng, forca):
    """Garrafa batendo em garrafa dentro do saco: vidro curto, abafado pelo plástico."""
    n = amostras(0.2, SR)
    b = rng.uniform(1700, 2500)
    y = modos(n, SR, rng, [b, 1.72 * b, 2.51 * b, 3.3 * b], [0.06, 0.04, 0.03, 0.02], [1, .55, .4, .2])
    y += 0.25 * ruido_env(rng, n, SR, 3000, None, 0.0003, 0.002)
    return forca * y


def s_saco_tilintar(rng):
    """Garrafas chacoalhando no saco a cada passo: 3 batidas de vidro, 0,35 s."""
    y = np.zeros(amostras(0.35, SR))
    for atraso, forca in ((0.0, 1.0), (rng.uniform(0.06, 0.08), 0.6), (rng.uniform(0.13, 0.16), 0.4)):
        somar(y, batida_vidro(rng, forca), amostras(atraso, SR))
    return efeito(filtrar(y, SR, lo=500, hi=9000))


def s_lixeira_tampa(rng):
    """Tampa de plástico da lixeira: dobradiça abrindo e o baque oco, 0,4 s."""
    y = np.zeros(amostras(0.4, SR))
    n = amostras(0.14, SR)
    dobradica = filtrar(ruido(rng, n), SR, 900, 3200) * env_trapezio(n, SR, 0.02, 0.09, 0.03)
    somar(y, 0.3 * dobradica, 0)
    somar(y, batida_plastico(rng, 0.55, 0.55), amostras(0.1, SR))
    somar(y, batida_plastico(rng, 1.0, 0.42), amostras(0.24, SR))
    somar(y, 0.5 * baque_borracha(rng, 0.8, 1.5), amostras(0.24, SR))
    return efeito(filtrar(y, SR, lo=70, hi=6500))


# ================================================================ AMBIENTE (avulsos 3D)

def silaba(f_ini, f_fim, dur, curva=1.0, vib=(0.0, 0.0)):
    n = amostras(dur, SR)
    t = tempo(n, SR)
    f = f_ini + (f_fim - f_ini) * (t / dur) ** curva
    f = f * (1 + vib[1] * np.sin(2 * np.pi * vib[0] * t))
    return harmonicos(f, n, SR, [1, 0.12, 0.04]) * np.sin(np.pi * t / dur) ** 1.5


def canto(rng, partes):
    """partes = [(silaba, pausa_depois), ...]"""
    total = sum(s.shape[-1] for s, _ in partes) + sum(amostras(p, SR) for _, p in partes)
    y = np.zeros(total + amostras(0.1, SR))
    pos = 0
    for s, pausa in partes:
        somar(y, s, pos)
        pos += s.shape[-1] + amostras(pausa, SR)
    y = filtrar(y, SR, hi=7000)
    return efeito(mono(reverb(y, SR, rng, 0.9, 0.25, 6000)))


def s_passaro(rng, v):
    if v == 0:   # bem-te-vi
        return canto(rng, [(silaba(2700, 3200, 0.07), 0.07), (silaba(3400, 3500, 0.07), 0.08),
                           (silaba(3900, 2800, 0.24, 0.8, (30, 0.02)), 0.0)])
    if v == 1:   # pardal
        partes = []
        for _ in range(6):
            j = rng.uniform(0.92, 1.08)
            partes.append((silaba(5200 * j, 3600 * j, 0.045, 0.7), rng.uniform(0.05, 0.09)))
        return canto(rng, partes)
    # assobio tipo sabiá
    notas = [(2300, 2500, .18), (2900, 2850, .15), (2600, 3300, .22), (3100, 2400, .3)]
    return canto(rng, [(silaba(a, b, d, 1.0, (6, 0.01)), 0.06) for a, b, d in notas])


def latido(rng, f0base, dur=0.2):
    n = amostras(dur, SR)
    t = tempo(n, SR)
    f0 = f0base * (0.85 + 0.35 * np.sin(np.pi * t / dur)) * (1 - 0.15 * (t / dur) ** 3)
    ph = fase(f0, n, SR)
    y = np.zeros(n)
    formantes = ((750, 250, 1.0), (1650, 350, 0.6), (2800, 500, 0.3))
    for k in range(1, 26):
        fk = k * f0
        a = sum(g / (1 + ((fk - F) / bw) ** 2) for F, bw, g in formantes)
        y += a * np.sin(k * ph) * (fk < 0.45 * SR) / k ** 0.3
    y += 0.25 * banda(ruido(rng, n), SR, 1200, 1.0) * np.std(y)
    return y * np.clip(t / 0.012, 0, 1) * np.sin(np.pi * t / dur) ** 0.6


def s_latido(rng, v):
    latidos = [(420, 0.28), (460, 0.0)] if v == 0 else [(380, 0.22), (400, 0.3), (370, 0.0)]
    partes = [(latido(rng, f, rng.uniform(0.17, 0.22)), p) for f, p in latidos]
    total = sum(s.shape[-1] + amostras(p, SR) for s, p in partes)
    y = np.zeros(total)
    pos = 0
    for s, p in partes:
        somar(y, s, pos)
        pos += s.shape[-1] + amostras(p, SR)
    y = filtrar(y, SR, 150, 2200)   # longe
    return efeito(mono(reverb(y, SR, rng, 1.4, 0.45, 4000)))


# ================================================================ AMBIENTE (loops)

def s_bairro(rng):
    """Cidade pequena parada, tarde quente: vento, folhas, cigarras. Sem carros."""
    L = 45.0
    sr = SR_LONGO
    n = amostras(L, sr)
    t = tempo(n, sr)

    def lfo(ks):
        return sum(a * np.sin(2 * np.pi * k * t / L + rng.uniform(0, 6.28)) for k, a in ks)

    rajada = np.clip(0.55 + lfo([(3, 0.25), (7, 0.15), (13, 0.08)]), 0.15, 1.2)
    comum = ruido(rng, n, "rosa")
    vento = filtrar(0.6 * np.vstack([comum, comum]) + 0.8 * ruido(rng, n, "marrom", 2), sr, 40, 450, circular=True)
    vento = vento / np.std(vento) * rajada

    folhas = filtrar(ruido(rng, n, canais=2), sr, 1800, 7000, circular=True)
    tremor = filtrar(ruido(rng, n), sr, hi=6, ordem=1, circular=True)
    tremor = 0.5 + 0.5 * tremor / (np.max(np.abs(tremor)) + 1e-12)
    folhas = folhas / np.std(folhas) * np.clip(rajada - 0.45, 0, None) ** 2 * tremor * 0.9

    y = 0.5 * vento + folhas

    coro = banda(ruido(rng, n, canais=2), sr, 4400, 3.0, circular=True)
    y += 0.05 * coro / np.std(coro) * (0.6 + 0.4 * np.sin(2 * np.pi * 2 * t / L))

    for _ in range(3):   # cigarras que crescem e somem
        dur = rng.uniform(7, 12)
        m = amostras(dur, sr)
        tt = tempo(m, sr)
        portadora = banda(ruido(rng, m), sr, rng.uniform(3800, 5200), 5.0)
        portadora /= np.std(portadora) + 1e-12
        pulso = (0.5 + 0.5 * np.sin(2 * np.pi * rng.uniform(70, 110) * tt)) ** 3
        forma = suave(tt / (0.35 * dur)) * suave((dur - tt) / (0.25 * dur)) * (1 + 0.15 * np.sin(2 * np.pi * 5 * tt))
        chamado = filtrar(portadora * (0.3 + 0.7 * pulso) * forma, sr, hi=7500)
        somar(y, panear(rng.uniform(0.25, 0.45) * chamado, rng.uniform(-0.7, 0.7)), amostras(rng.uniform(0, L), sr), circular=True)

    y = filtrar(y, sr, hi=9000, circular=True)
    return nivelar(y, rms_db=-27)


def voz_murmurio(rng, n, sr):
    """Uma pessoa falando baixinho: sílabas com formantes de vogal, em frases."""
    vogais = ((750, 1200), (450, 1900), (300, 2300), (500, 900), (330, 800))
    y = np.zeros(n)
    pos = amostras(rng.uniform(0, 3), sr)
    fim = pos + n
    while pos < fim:
        f0 = float(np.clip(rng.normal(150, 40), 95, 240))
        for _ in range(rng.integers(5, 13)):
            dur = rng.uniform(0.08, 0.25)
            m = amostras(dur, sr)
            tt = tempo(m, sr)
            f = f0 * rng.uniform(0.9, 1.12) * (1 - 0.08 * tt / dur)
            f1, f2 = vogais[rng.integers(len(vogais))]
            ph = fase(f, m, sr)
            s = np.zeros(m)
            for k in range(1, int(3000 / f0)):
                fk = k * f
                a = 1 / (1 + ((fk - f1) / 120) ** 2) + 0.6 / (1 + ((fk - f2) / 200) ** 2)
                s += a * np.sin(k * ph) / k ** 0.5
            somar(y, s * np.sin(np.pi * tt / dur) ** 2 * rng.uniform(0.5, 1.0), pos, circular=True)
            pos += m + amostras(rng.uniform(0.0, 0.05), sr)
        pos += amostras(rng.uniform(0.5, 2.0), sr)
    return y


def s_hospital(rng):
    """Recepção: lâmpada fluorescente, ar-condicionado, murmúrio na sala ao lado, monitor longe."""
    L = 30.0
    sr = SR_LONGO
    n = amostras(L, sr)
    t = tempo(n, sr)
    hum = np.tanh(1.5 * (np.sin(2 * np.pi * 120 * t) + 0.5 * np.sin(2 * np.pi * 240 * t) + 0.25 * np.sin(2 * np.pi * 360 * t)))
    hum = filtrar(hum, sr, 80, 2000, circular=True)
    y = 0.12 * np.vstack([hum, hum])

    ar = filtrar(ruido(rng, n, "rosa", 2), sr, 60, 900, circular=True)
    y += 0.35 * ar / np.std(ar)

    vozes = sum(voz_murmurio(rng, n, sr) for _ in range(4))
    vozes = filtrar(vozes, sr, 120, 900, circular=True)
    vozes = reverb(vozes / (np.std(vozes) + 1e-12), sr, rng, 1.2, 0.6, 3000, circular=True)
    y += 0.3 * vozes

    periodo = L / 36.0   # 72 bpm, cabe inteiro no loop
    monitor = np.zeros(n)
    b = bip(1000, 0.09, sr)
    for i in range(36):
        somar(monitor, b, amostras(i * periodo, sr), circular=True)
    y += reverb(panear(0.05 * monitor, 0.55), sr, rng, 0.9, 0.5, 5000, circular=True)
    return nivelar(y, rms_db=-29)


# ================================================================ TRILHA (80 BPM, Ré menor)

ACORDES = [   # (fundamental grave, notas do pad) — 2 compassos cada
    (38, [50, 53, 57, 62]),   # Dm
    (34, [46, 50, 53, 58]),   # Bb
    (31, [43, 50, 55, 58]),   # Gm
    (33, [45, 52, 57, 61]),   # A (com C#: puxa de volta pro Ré)
]
BATIDAS_POR_ACORDE = 8


def trilha_vazia():
    return np.zeros((2, amostras(DUR_TRILHA, SR_LONGO)))


def em(batida):
    return amostras(batida * BATIDA, SR_LONGO)


def s_trilha_base(rng):
    sr = SR_LONGO
    y = trilha_vazia()
    dur = BATIDAS_POR_ACORDE * BATIDA
    for i, (grave, notas) in enumerate(ACORDES):
        for m in notas:
            somar(y, nota_pad(m, dur, sr, rng, 1800, 1.2, 1.6), em(i * BATIDAS_POR_ACORDE) - amostras(0.4, sr), circular=True)
        n = amostras(dur + 1.5, sr)
        tt = tempo(n, sr)
        sub = harmonicos(midi(grave + 12), n, sr, [1, 0.3, 0.1]) * suave(tt / 0.8) * suave((dur + 1.5 - tt) / 1.5)
        somar(y, 0.5 * sub, em(i * BATIDAS_POR_ACORDE) - amostras(0.4, sr), circular=True)
    y = reverb(y, sr, rng, 2.5, 0.35, 4000, circular=True)
    return nivelar(y, rms_db=-21)


def batimento(sr, rng):
    n = amostras(0.5, sr)

    def golpe(atraso, f_ini, f_fim, amp, queda):
        s = np.sin(fase(varredura(n, sr, f_ini, f_fim, atraso, 0.03), n, sr)) * env_ad(n, sr, 0.004, queda, atraso)
        s += 0.3 * filtrar(ruido(rng, n), sr, hi=200) * env_ad(n, sr, 0.002, 0.03, atraso)
        return amp * s

    return golpe(0.0, 95, 48, 1.0, 0.08) + golpe(0.2, 110, 58, 0.65, 0.06)


def s_trilha_pulso(rng):
    sr = SR_LONGO
    y = trilha_vazia()
    bat = batimento(sr, rng)
    for b in range(32):
        somar(y, bat, em(b), circular=True)
    acentos = [1.0, 0.55, 0.8, 0.55]
    for c in range(64):   # baixo em colcheias
        grave = ACORDES[(c // 2) // BATIDAS_POR_ACORDE][0]
        n = amostras(0.4, sr)
        nota = harmonicos(midi(grave + 12), n, sr, [1, 0.5, 0.25, 0.12, 0.06]) * env_ad(n, sr, 0.006, 0.22)
        somar(y, 0.45 * acentos[c % 4] * nota, em(c * 0.5), circular=True)
    y = reverb(y, sr, rng, 0.8, 0.12, 3000, circular=True)
    return nivelar(y, rms_db=-20)


def s_trilha_tensao(rng):
    sr = SR_LONGO
    y = trilha_vazia()
    celulas = [[74, 69, 77, 69], [74, 70, 77, 70], [74, 70, 79, 70], [73, 69, 76, 69]]
    acentos = [1.0, 0.6, 0.8, 0.6]
    for s in range(128):   # semicolcheias
        acorde = (s // 4) // BATIDAS_POR_ACORDE
        nota = pluck(midi(celulas[acorde][s % 4]), sr, rng)
        somar(y, panear(0.5 * acentos[s % 4] * nota, 0.35 if s % 2 else -0.35), em(s * 0.25), circular=True)
    for b in range(32):   # tique-taque
        n = amostras(0.1, sr)
        f = 3300 if b % 2 == 0 else 2500
        tic = modos(n, sr, rng, [f, f * 1.9], [0.012, 0.006], [1, 0.4]) + 0.3 * ruido_env(rng, n, sr, 2500, None, 0.0003, 0.003)
        somar(y, 0.35 * tic, em(b), circular=True)
    y = reverb(y, sr, rng, 1.2, 0.22, 5000, circular=True)
    return nivelar(y, rms_db=-24)


MELODIA_ESPERANCA = [(0, 69), (1.5, 74), (2, 76), (3, 77), (5, 76),
                     (8, 74), (9.5, 72), (10, 74), (11, 77), (13, 74),
                     (16, 74), (17.5, 70), (18, 72), (19, 74), (21, 79), (22, 77),
                     (24, 76), (25.5, 73), (26, 76), (27, 81), (29, 76), (30, 73)]


def s_trilha_esperanca(rng):
    sr = SR_LONGO
    y = trilha_vazia()
    eco = 0.75 * BATIDA   # colcheia pontuada
    for b, m in MELODIA_ESPERANCA:
        nota = marimba(midi(m), sr, rng)
        somar(y, panear(0.6 * nota, 0.0), em(b), circular=True)
        somar(y, panear(0.21 * nota, 0.6), em(b) + amostras(eco, sr), circular=True)
        somar(y, panear(0.09 * nota, -0.6), em(b) + amostras(2 * eco, sr), circular=True)
    dur = BATIDAS_POR_ACORDE * BATIDA
    for i, (_, notas) in enumerate(ACORDES):   # "ar" agudo bem baixinho, pra colar
        for m in notas[1:]:
            somar(y, 0.12 * nota_pad(m + 12, dur, sr, rng, 2500, 1.5, 1.5), em(i * BATIDAS_POR_ACORDE) - amostras(0.4, sr), circular=True)
    y = reverb(y, sr, rng, 2.2, 0.3, 5000, circular=True)
    return nivelar(y, rms_db=-24)


def s_trilha_menu(rng):
    """Fim de tarde: pad calmo com acordes abertos e notas soltas. 32 s."""
    sr = SR_LONGO
    L = 32.0
    y = np.zeros((2, amostras(L, sr)))
    acordes = [(38, [50, 57, 62, 64, 65]),    # Dm9
               (34, [46, 53, 57, 60, 62]),    # Bbmaj9
               (31, [43, 50, 53, 58, 62]),    # Gm7
               (33, [45, 52, 55, 61, 64])]    # A7
    for i, (grave, notas) in enumerate(acordes):
        inicio = amostras(i * 8.0 - 0.8, sr)
        for m in notas:
            somar(y, 0.8 * nota_pad(m, 8.0, sr, rng, 1400, 2.5, 3.0), inicio, circular=True)
        somar(y, 0.6 * nota_pad(grave + 12, 8.0, sr, rng, 400, 2.0, 3.0), inicio, circular=True)
    for seg, m in ((1, 76), (3, 74), (9, 77), (11, 72), (17, 74), (19, 70), (25, 73), (27, 76)):
        somar(y, panear(0.35 * marimba(midi(m), sr, rng, 2.4), rng.uniform(-0.3, 0.3)), amostras(seg, sr), circular=True)
    y = reverb(y, sr, rng, 3.2, 0.45, 3500, circular=True)
    return nivelar(y, rms_db=-21)


# ================================================================ catálogo

SONS = {
    # nome: (função, sr)
    "mosquito_zumbido_loop": (s_mosquito, SR),
    "foco_eliminado": (s_eliminado, SR),
    "ocupacao_alerta": (s_alerta, SR),
    "ocupacao_desce": (s_desce, SR),
    "vitoria": (s_vitoria, SR),
    "derrota": (s_derrota, SR),
    "pulo": (s_pulo, SR),
    "aterrissagem": (s_aterrissagem, SR),
    "celular_notificacao": (s_notificacao, SR),
    "celular_abrir": (lambda r: s_celular(r, True), SR),
    "celular_fechar": (lambda r: s_celular(r, False), SR),
    "ui_clique": (s_ui_clique, SR),
    "kit_trocar": (s_kit_trocar, SR),
    "kit_erro": (s_kit_erro, SR),
    "kit_reabastecer": (s_kit_reabastecer, SR),
    "cacamba_despejar": (s_cacamba_despejar, SR),
    "pneu_pegar": (s_pneu_pegar, SR),
    "pneu_largar": (s_pneu_largar, SR),
    "kit_segurar_loop": (s_kit_segurar_loop, SR),
    "chuva_loop": (s_chuva_loop, SR_LONGO),
    "trovao": (s_trovao, SR),
    "esvaziar": (s_esvaziar, SR),
    "pneu_picape": (s_pneu_picape, SR),
    "suspensao": (s_suspensao, SR),
    "saco_tilintar": (s_saco_tilintar, SR),
    "lixeira_tampa": (s_lixeira_tampa, SR),
    "ambiente_bairro_loop": (s_bairro, SR_LONGO),
    "ambiente_hospital_loop": (s_hospital, SR_LONGO),
    "trilha_base": (s_trilha_base, SR_LONGO),
    "trilha_pulso": (s_trilha_pulso, SR_LONGO),
    "trilha_tensao": (s_trilha_tensao, SR_LONGO),
    "trilha_esperanca": (s_trilha_esperanca, SR_LONGO),
    "trilha_menu": (s_trilha_menu, SR_LONGO),
}
for _nome, _func, _qtd in (("foco_agua", s_agua, 3), ("foco_areia", s_areia, 2), ("foco_tampa", s_tampa, 2),
                           ("foco_garrafa", s_garrafa, 3), ("foco_metal", s_metal, 2),
                           ("passo_rua", s_passo_rua, 4), ("passo_piso", s_passo_piso, 4),
                           ("passo_pesado", s_passo_pesado, 3), ("esforco", s_esforco, 3),
                           ("passaro", s_passaro, 3), ("latido", s_latido, 2)):
    for _v in range(_qtd):
        SONS[f"{_nome}_{_v + 1}"] = ((lambda f, v: (lambda r: f(r, v)))(_func, _v), SR)


def salvar(caminho, x, sr):
    x = np.asarray(x, dtype=float)
    canais = 1 if x.ndim == 1 else x.shape[0]
    dados = x if x.ndim == 1 else x.T.reshape(-1)
    pcm = np.round(np.clip(dados, -1.0, 1.0) * 32767.0).astype("<i2")
    with wave.open(caminho, "wb") as w:
        w.setnchannels(canais)
        w.setsampwidth(2)
        w.setframerate(sr)
        w.writeframes(pcm.tobytes())


def emenda(x):
    """Quão 'pulada' é a volta do fim pro começo, em relação ao passo típico entre amostras."""
    x = np.atleast_2d(x)
    salto = np.max(np.abs(x[:, 0] - x[:, -1]))
    tipico = np.median(np.abs(np.diff(x, axis=-1))) + 1e-12
    return salto / tipico


def main():
    ap = argparse.ArgumentParser(description="Gera os sons do Foco Zero.")
    ap.add_argument("pasta", nargs="?", default=".")
    ap.add_argument("--so", default="", help="nomes separados por vírgula")
    ap.add_argument("--listar", action="store_true")
    a = ap.parse_args()

    if a.listar:
        print("\n".join(SONS))
        return

    so = [s.strip() for s in a.so.split(",") if s.strip()]
    desconhecidos = [s for s in so if s not in SONS]
    if desconhecidos:
        sys.exit("Sons desconhecidos: " + ", ".join(desconhecidos))

    os.makedirs(a.pasta, exist_ok=True)
    print(f"{'som':<26} {'dur':>6} {'can':>3} {'Hz':>6} {'pico':>7} {'rms':>7}  emenda")
    for nome, (func, sr) in SONS.items():
        if so and nome not in so:
            continue
        rng = np.random.default_rng(zlib.crc32(nome.encode("utf-8")))
        x = func(rng)
        salvar(os.path.join(a.pasta, nome + ".wav"), x, sr)
        loop = nome.endswith("_loop") or nome.startswith("trilha_")
        can = 1 if x.ndim == 1 else x.shape[0]
        info = f"{emenda(x):6.1f}" if loop else "     -"
        print(f"{nome:<26} {x.shape[-1] / sr:6.2f} {can:>3} {sr:>6} {20 * np.log10(pico(x) + 1e-12):7.1f} {20 * np.log10(rms(x) + 1e-12):7.1f}  {info}")


if __name__ == "__main__":
    main()
