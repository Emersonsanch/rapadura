#!/usr/bin/env python3
"""Gera os ícones dos itens do Rapadura (PNG 128x128 flat/low-poly) e liga cada um no seu ItemDefinition.

Pipeline (reprodutível, sem o Editor da Unity):
  1. acha todo .asset de ItemDefinition em Assets/Resources/Items (pelo guid do script);
  2. desenha o ícone pela receita do itemId (RECIPES), ou pela família/categoria quando não há receita própria;
  3. grava Assets/Textures/Icons/Items/<itemId>.png + .meta de Sprite com GUID determinístico
     (md5 de "rapadura.item-icon.<itemId>"), então rodar de novo não muda GUID nenhum;
  4. troca `_icon: {fileID: 0}` (ou um ícone gerado antigo) por `{fileID: 21300000, guid: <guid>, type: 3}`.

Estilo: formas de poucos lados (low-poly), duas faces por forma (luz em cima-esquerda, sombra embaixo-direita),
contorno marrom-noite do tema (rap-theme), halo da cor da raridade (ColorblindPaletteMap) a partir de Incomum.

Uso:  python tools/icons/generate_item_icons.py            (gera tudo e liga nos .asset)
      python tools/icons/generate_item_icons.py --check    (só lista itens sem receita/ícone; exit 1 se faltar)
      python tools/icons/generate_item_icons.py --only item_selo_sol item_bolha_ar
Requer Pillow.
"""
import argparse
import io
import hashlib
import math
import os
import re
import sys

from PIL import Image, ImageChops, ImageDraw, ImageFilter

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
ITEMS_DIR = os.path.join(ROOT, "Assets", "Resources", "Items")
OUT_DIR = os.path.join(ROOT, "Assets", "Textures", "Icons", "Items")
OUT_ASSET_DIR = "Assets/Textures/Icons/Items"
ITEM_SCRIPT_META = os.path.join(ROOT, "Assets", "Scripts", "Gameplay", "Items", "ItemDefinition.cs.meta")
SPRITE_FILE_ID = 21300000

SIZE = 128          # tamanho final
SS = 4              # supersampling
W = SIZE * SS

# ---------------------------------------------------------------- paleta (rap-theme + materiais)
NIGHT = (28, 16, 10)
BROWN_DEEP = (46, 27, 16)
BROWN = (86, 50, 26)
BROWN_LIGHT = (128, 78, 40)
CARAMEL = (196, 122, 44)
CARAMEL_LIGHT = (226, 158, 70)
GOLD = (244, 196, 88)
GOLD_LIGHT = (255, 224, 140)
CREAM = (255, 242, 216)
CREAM_DIM = (222, 200, 166)
CANE_GREEN = (112, 146, 62)
CLAY_RED = (178, 72, 40)
LEATHER = (150, 92, 50)
IRON = (150, 158, 166)
IRON_DARK = (92, 98, 108)
STEEL = (196, 204, 212)
BRONZE = (184, 120, 56)
COPPER = (196, 110, 70)
RUST = (150, 74, 40)
BONE = (236, 224, 196)
BONE_DARK = (196, 178, 140)
WOOD = (150, 98, 54)
WOOD_DARK = (104, 66, 36)
LEAF = (112, 166, 70)
LEAF_DARK = (70, 118, 52)
WATER = (90, 170, 210)
TURQUOISE = (63, 167, 163)
SKY = (150, 210, 240)
FLAME = (255, 150, 50)
FLAME_CORE = (255, 230, 120)
RUNE_BLUE = (110, 150, 255)
VIOLET = (140, 90, 200)
REED = (196, 170, 96)
WOOL = (232, 218, 190)

RARITY_GLOW = {  # ColorblindPaletteMap (normal)
    1: (51, 204, 64),
    2: (51, 115, 242),
    3: (166, 64, 230),
    4: (242, 153, 26),
}


def mix(a, b, t):
    return tuple(int(round(a[i] + (b[i] - a[i]) * t)) for i in range(3))


def lighter(c, t=0.28):
    return mix(c, (255, 255, 255), t)


def darker(c, t=0.28):
    return mix(c, (0, 0, 0), t)


# ---------------------------------------------------------------- canvas com formas facetadas
class Icon:
    def __init__(self):
        self.img = Image.new("RGBA", (W, W), (0, 0, 0, 0))
        self.fx = Image.new("RGBA", (W, W), (0, 0, 0, 0))  # brilhos
        self.line = 1.6  # contorno interno, em px do ícone final

    @staticmethod
    def _p(pts):
        return [(x * SS, y * SS) for x, y in pts]

    def _mask(self, draw_fn):
        m = Image.new("L", (W, W), 0)
        draw_fn(ImageDraw.Draw(m))
        return m

    def _paint(self, mask, col, shade=True, outline=True, angle=-45.0):
        bbox = mask.getbbox()
        if not bbox:
            return
        self.img.paste(col + (255,), (0, 0), mask)
        if shade:
            cx, cy = (bbox[0] + bbox[2]) / 2, (bbox[1] + bbox[3]) / 2
            a = math.radians(angle)
            dx, dy = math.cos(a), math.sin(a)  # direção da luz (para cima-esquerda por padrão)
            big = W * 3
            # meio-plano iluminado: lado para onde aponta (dx,dy) a partir do centro
            px, py = -dy, dx
            half = Image.new("L", (W, W), 0)
            ImageDraw.Draw(half).polygon([
                (cx + px * big, cy + py * big),
                (cx + px * big + dx * big, cy + py * big + dy * big),
                (cx - px * big + dx * big, cy - py * big + dy * big),
                (cx - px * big, cy - py * big)], fill=255)
            lit = ImageChops.multiply(mask, half)
            self.img.paste(lighter(col, 0.22) + (255,), (0, 0), lit)
            dark = ImageChops.subtract(mask, half)
            self.img.paste(darker(col, 0.12) + (255,), (0, 0), dark)
        if outline:
            w = max(1, int(self.line * SS))
            edge = ImageChops.subtract(mask, mask.filter(ImageFilter.MinFilter(w * 2 + 1)))
            self.img.paste(NIGHT + (255,), (0, 0), edge)

    # primitivas (coordenadas no espaço 0..128)
    def poly(self, pts, col, **kw):
        p = self._p(pts)
        self._paint(self._mask(lambda d: d.polygon(p, fill=255)), col, **kw)

    def ngon(self, cx, cy, r, n, col, rot=0.0, ry=None, **kw):
        ry = r if ry is None else ry
        pts = [(cx + r * math.cos(math.radians(rot + 360.0 * i / n)),
                cy + ry * math.sin(math.radians(rot + 360.0 * i / n))) for i in range(n)]
        self.poly(pts, col, **kw)

    def rect(self, x0, y0, x1, y1, col, **kw):
        self.poly([(x0, y0), (x1, y0), (x1, y1), (x0, y1)], col, **kw)

    def bar(self, x0, y0, x1, y1, width, col, **kw):
        """Retângulo grosso de (x0,y0) até (x1,y1)."""
        dx, dy = x1 - x0, y1 - y0
        ln = math.hypot(dx, dy) or 1.0
        nx, ny = -dy / ln * width / 2, dx / ln * width / 2
        self.poly([(x0 + nx, y0 + ny), (x1 + nx, y1 + ny), (x1 - nx, y1 - ny), (x0 - nx, y0 - ny)], col, **kw)

    def stroke(self, pts, width, col):
        """Linha de detalhe (sem facetas), largura em px do ícone."""
        d = ImageDraw.Draw(self.img)
        d.line(self._p(pts), fill=col + (255,), width=max(1, int(width * SS)), joint="curve")

    def dot(self, cx, cy, r, col):
        d = ImageDraw.Draw(self.img)
        d.ellipse([(cx - r) * SS, (cy - r) * SS, (cx + r) * SS, (cy + r) * SS], fill=col + (255,))

    def glow(self, cx, cy, r, col, alpha=150):
        """Brilho suave; fica numa camada própria, sob as formas e fora do contorno."""
        m = Image.new("L", (W, W), 0)
        ImageDraw.Draw(m).ellipse([(cx - r) * SS, (cy - r) * SS, (cx + r) * SS, (cy + r) * SS], fill=alpha)
        m = m.filter(ImageFilter.GaussianBlur(r * SS * 0.45))
        layer = Image.new("RGBA", (W, W), col + (0,))
        layer.putalpha(m)
        self.fx = Image.alpha_composite(self.fx, layer)

    def finish(self, rarity):
        # contorno externo grosso (sticker) por dilatação do alfa
        alpha = self.img.split()[3].point(lambda v: 255 if v > 8 else 0)
        ow = int(2.6 * SS)
        outer = alpha.filter(ImageFilter.MaxFilter(ow * 2 + 1))
        base = Image.new("RGBA", (W, W), (0, 0, 0, 0))
        if rarity >= 1:
            halo = Image.new("RGBA", (W, W), (0, 0, 0, 0))
            g = RARITY_GLOW[rarity]
            ImageDraw.Draw(halo).ellipse([24 * SS, 24 * SS, 104 * SS, 104 * SS], fill=g + (110 + 25 * rarity,))
            hm = halo.split()[3].filter(ImageFilter.GaussianBlur(10 * SS))
            halo = Image.new("RGBA", (W, W), g + (0,))
            halo.putalpha(hm)
            base = Image.alpha_composite(base, halo)
        base = Image.alpha_composite(base, self.fx)
        # brilhos somem antes da borda do quadro (sem quinas retas)
        base.putalpha(ImageChops.multiply(base.split()[3], _edge_fade()))
        sticker = Image.new("RGBA", (W, W), NIGHT + (255,))
        sticker.putalpha(outer)
        base = Image.alpha_composite(base, sticker)
        base = Image.alpha_composite(base, self.img)
        return base.resize((SIZE, SIZE), Image.LANCZOS)


_EDGE_FADE = None


def _edge_fade():
    global _EDGE_FADE
    if _EDGE_FADE is None:
        m = Image.new("L", (W, W), 0)
        ImageDraw.Draw(m).ellipse([10 * SS, 10 * SS, 118 * SS, 118 * SS], fill=255)
        _EDGE_FADE = m.filter(ImageFilter.GaussianBlur(5 * SS))
    return _EDGE_FADE


# ---------------------------------------------------------------- peças reutilizáveis
def draw_rune(ic, cx, cy, s, col):
    ic.stroke([(cx, cy - s), (cx, cy + s)], 2.2, col)
    ic.stroke([(cx, cy - s * 0.4), (cx + s * 0.6, cy - s)], 2.2, col)
    ic.stroke([(cx, cy), (cx - s * 0.6, cy - s * 0.5)], 2.2, col)


def flame(ic, cx, cy, s, col=FLAME, core=FLAME_CORE):
    ic.glow(cx, cy, s * 1.6, col, 120)
    ic.poly([(cx, cy - s * 1.5), (cx + s * 0.75, cy - s * 0.1), (cx + s * 0.5, cy + s * 0.6),
             (cx - s * 0.5, cy + s * 0.6), (cx - s * 0.75, cy - s * 0.1)], col)
    ic.poly([(cx, cy - s * 0.7), (cx + s * 0.35, cy + s * 0.1), (cx, cy + s * 0.5), (cx - s * 0.35, cy + s * 0.1)],
            core, outline=False, shade=False)


def bottle(ic, liquid, glass=(200, 230, 240), neck_col=WOOD, cx=64, round_body=True):
    ic.rect(cx - 8, 22, cx + 8, 40, glass)
    ic.rect(cx - 10, 16, cx + 10, 24, neck_col)
    if round_body:
        ic.ngon(cx, 76, 32, 10, glass, rot=18)
        ic.ngon(cx, 82, 26, 10, liquid, rot=18, outline=False)
        ic.poly([(cx - 26, 74), (cx + 26, 74), (cx + 24, 66), (cx - 24, 66)], lighter(liquid, 0.35), outline=False,
                shade=False)
    else:
        ic.poly([(cx - 24, 42), (cx + 24, 42), (cx + 30, 104), (cx - 30, 104)], glass)
        ic.poly([(cx - 25, 62), (cx + 25, 62), (cx + 29, 101), (cx - 29, 101)], liquid, outline=False)
    ic.poly([(cx - 18, 56), (cx - 12, 52), (cx - 14, 72), (cx - 20, 74)], (255, 255, 255), shade=False,
            outline=False)


def key(ic, metal, bow_shape="ring", gem=None):
    ic.bar(40, 88, 96, 32, 11, metal)
    ic.poly([(84, 44), (96, 56), (104, 48), (92, 36)], metal)  # dente 1
    ic.poly([(74, 54), (84, 64), (90, 58), (80, 48)], metal)   # dente 2
    if bow_shape == "leaf":
        ic.poly([(16, 102), (22, 74), (40, 64), (54, 70), (50, 92), (34, 108)], metal)
        ic.stroke([(22, 100), (46, 72)], 2, darker(metal, 0.35))
    elif bow_shape == "skull":
        ic.ngon(34, 92, 20, 8, metal, rot=22)
        ic.dot(27, 90, 4.5, NIGHT)
        ic.dot(41, 90, 4.5, NIGHT)
        ic.poly([(32, 98), (36, 98), (34, 102)], NIGHT, shade=False, outline=False)
    else:
        ic.ngon(34, 92, 20, 8, metal, rot=22)
        ic.ngon(34, 92, 9, 8, (0, 0, 0), rot=22, shade=False)
        ic.img.paste((0, 0, 0, 0), (0, 0), ic._mask(lambda d: d.regular_polygon((34 * SS, 92 * SS, 7 * SS), 8, 22, fill=255)))
    if gem:
        ic.ngon(34, 92, 6, 6, gem, rot=30)


def coin(ic, metal, holed=True, cx=64, cy=64, r=40, glyph=True):
    ic.ngon(cx, cy, r, 12, metal, rot=15)
    ic.ngon(cx, cy, r - 7, 12, lighter(metal, 0.1), rot=15, shade=False)
    if holed:
        s = r * 0.25
        ic.rect(cx - s - 2, cy - s - 2, cx + s + 2, cy + s + 2, darker(metal, 0.3), shade=False)
        ic.img.paste((0, 0, 0, 0), (0, 0), ic._mask(
            lambda d: d.rectangle([(cx - s) * SS, (cy - s) * SS, (cx + s) * SS, (cy + s) * SS], fill=255)))
    elif glyph:
        draw_rune(ic, cx, cy, r * 0.4, darker(metal, 0.4))


def sword(ic, blade, guard=BRONZE, grip=LEATHER, wood=False):
    ic.poly([(98, 18), (108, 20), (110, 30), (54, 86), (42, 74)], blade)
    if not wood:
        ic.stroke([(104, 24), (50, 78)], 1.6, lighter(blade, 0.45))
    ic.bar(34, 66, 62, 94, 10, guard)
    ic.bar(26, 102, 46, 82, 8, grip)
    ic.ngon(22, 106, 7, 6, guard)


def tool(ic, head, kind):
    ic.bar(30, 110, 82, 34, 9, WOOD)
    if kind == "axe":
        ic.poly([(70, 20), (100, 30), (104, 62), (84, 56), (74, 44)], head)
        ic.stroke([(100, 32), (102, 58)], 2, lighter(head, 0.5))
    elif kind == "pickaxe":
        ic.poly([(40, 30), (64, 18), (100, 26), (110, 52), (96, 38), (70, 32), (48, 42)], head)
    else:  # martelo
        ic.poly([(60, 26), (92, 14), (104, 40), (72, 52)], head)


def scroll(ic, paper=CREAM_DIM, seal=CLAY_RED, rolled_col=None, glyph=None, ribbon=None):
    ic.rect(30, 26, 98, 100, paper)
    ic.ngon(30, 30, 10, 8, rolled_col or darker(paper, 0.12), ry=8)
    ic.ngon(98, 96, 10, 8, rolled_col or darker(paper, 0.12), ry=8)
    for y in (44, 54, 64):
        ic.stroke([(42, y), (86, y)], 2, darker(paper, 0.35))
    if glyph:
        glyph(ic)
    if ribbon:
        ic.poly([(58, 82), (64, 82), (60, 112), (52, 106)], ribbon)
        ic.poly([(66, 82), (72, 82), (78, 106), (70, 112)], ribbon)
    if seal:
        ic.ngon(64, 82, 11, 9, seal, rot=10)


def gem(ic, col, cx=64, cy=66, r=36, sides=6):
    ic.glow(cx, cy, r * 1.1, col, 110)
    ic.ngon(cx, cy, r, sides, col, rot=-90)
    ic.ngon(cx, cy - r * 0.15, r * 0.5, sides, lighter(col, 0.3), rot=-90, outline=False)
    ic.poly([(cx - r * 0.45, cy - r * 0.6), (cx - r * 0.2, cy - r * 0.75), (cx - r * 0.35, cy - r * 0.25)],
            (255, 255, 255), shade=False, outline=False)


def fish(ic, body, belly=None, fin=None, spots=None, whiskers=False, glow_col=None, scale=1.0, plates=False,
         long=False, teeth=False):
    s = scale
    cx, cy = 60, 64
    if glow_col:
        ic.glow(cx, cy, 44, glow_col, 150)
    fin = fin or darker(body, 0.25)
    ic.poly([(cx + 34 * s, cy), (cx + 56 * s, cy - 22 * s), (cx + 50 * s, cy), (cx + 56 * s, cy + 22 * s)], fin)
    ic.poly([(cx - 6 * s, cy - 18 * s), (cx + 14 * s, cy - 32 * s), (cx + 22 * s, cy - 16 * s)], fin)
    rx, ry = (48 * s, 15 * s) if long else (42 * s, 22 * s)
    ic.ngon(cx, cy, rx, 10, body, ry=ry)
    if belly:
        ic.poly([(cx - rx * 0.8, cy + ry * 0.3), (cx + rx * 0.7, cy + ry * 0.3), (cx + rx * 0.5, cy + ry * 0.85),
                 (cx - rx * 0.5, cy + ry * 0.85)], belly, outline=False, shade=False)
    if plates:
        for i in range(4):
            x = cx - 22 * s + i * 14 * s
            ic.poly([(x, cy - ry * 0.75), (x + 6 * s, cy - ry * 1.05), (x + 12 * s, cy - ry * 0.75)], BONE_DARK)
    if spots:
        for (dx, dy) in ((-8, -6), (6, -10), (16, 2), (-2, 6), (24, -6)):
            ic.dot(cx + dx * s, cy + dy * s, 2.6 * s, spots)
    ic.dot(cx - rx * 0.62, cy - ry * 0.2, 4.2 * s, CREAM)
    ic.dot(cx - rx * 0.62, cy - ry * 0.2, 2.2 * s, NIGHT)
    if whiskers:
        ic.stroke([(cx - rx * 0.9, cy + 2), (cx - rx * 1.25, cy + 14 * s)], 2, NIGHT)
        ic.stroke([(cx - rx * 0.9, cy + 4), (cx - rx * 1.1, cy + 20 * s)], 2, NIGHT)
    if teeth:
        for i in range(3):
            x = cx - rx * 0.95 + i * 4
            ic.poly([(x, cy + 2), (x + 3, cy + 2), (x + 1.5, cy + 7)], CREAM, shade=False, outline=False)


def wings(ic, col, tier, accent=None, glowy=False):
    accent = accent or lighter(col, 0.35)
    if glowy:
        ic.glow(64, 60, 52, accent, 140)
    for side in (-1, 1):
        def X(x):
            return 64 + side * x
        ic.poly([(X(6), 50), (X(54), 18), (X(58), 40), (X(50), 64), (X(10), 70)], col)
        ic.poly([(X(8), 64), (X(46), 64), (X(44), 86), (X(12), 80)], darker(col, 0.1))
        if tier >= 2:
            ic.poly([(X(30), 30), (X(56), 22), (X(52), 46)], accent)
        if tier >= 3:
            ic.poly([(X(14), 80), (X(40), 88), (X(30), 104), (X(12), 92)], accent)
            ic.poly([(X(54), 18), (X(62), 8), (X(60), 30)], GOLD_LIGHT)
    ic.ngon(64, 64, 9, 6, darker(col, 0.35))
    if tier >= 2:
        ic.ngon(64, 64, 5, 6, GOLD)


def cloak(ic, col, trim=None, bolt=False):
    ic.poly([(46, 18), (82, 18), (106, 108), (22, 108)], col)
    ic.poly([(46, 18), (64, 30), (82, 18), (76, 14), (52, 14)], darker(col, 0.2))
    ic.stroke([(56, 40), (44, 104)], 2, darker(col, 0.3))
    ic.stroke([(72, 40), (84, 104)], 2, darker(col, 0.3))
    if trim:
        ic.poly([(22, 108), (106, 108), (103, 98), (25, 98)], trim)
    ic.ngon(64, 24, 6, 6, GOLD)
    if bolt:
        ic.poly([(70, 42), (54, 70), (64, 70), (56, 94), (78, 62), (66, 62), (74, 42)], FLAME_CORE)


def boot(ic, col, sole=BROWN_DEEP, cuff=None):
    ic.poly([(38, 18), (70, 18), (72, 76), (104, 84), (106, 104), (34, 104)], col)
    ic.rect(32, 100, 108, 110, sole)
    if cuff:
        ic.rect(34, 16, 74, 30, cuff)


def whistle(ic, tassel):
    ic.bar(20, 92, 86, 42, 16, REED)
    for t in (0.3, 0.55):
        x, y = 20 + 66 * t, 92 - 50 * t
        ic.stroke([(x - 6, y - 8), (x + 6, y + 8)], 2, darker(REED, 0.4))
    ic.ngon(64, 58, 4.5, 8, NIGHT, shade=False, outline=False)
    ic.stroke([(86, 42), (98, 30), (106, 46)], 2.4, BROWN)
    ic.poly([(100, 44), (112, 44), (116, 74), (96, 74)], tassel)
    ic.ngon(106, 42, 6, 6, GOLD)


def pot(ic, col, contents=None, label=None):
    ic.poly([(40, 40), (88, 40), (102, 70), (90, 104), (38, 104), (26, 70)], col)
    ic.rect(42, 26, 86, 40, darker(col, 0.15))
    ic.rect(46, 18, 82, 28, contents or CREAM_DIM)
    if label:
        ic.rect(44, 62, 84, 82, label, shade=False)


# ---------------------------------------------------------------- receitas próprias
def r_corda_escalada(ic):
    for r, c in ((44, REED), (32, darker(REED, 0.1)), (20, REED)):
        ic.ngon(60, 66, r, 12, c, rot=8, ry=r * 0.78)
    ic.ngon(60, 66, 9, 10, (0, 0, 0), shade=False, outline=False)
    ic.img.paste((0, 0, 0, 0), (0, 0), ic._mask(lambda d: d.ellipse([51 * SS, 59 * SS, 69 * SS, 73 * SS], fill=255)))
    ic.poly([(98, 30), (110, 22), (114, 40), (102, 50), (96, 44), (104, 38)], STEEL)
    ic.bar(98, 46, 88, 62, 5, REED)


def r_chave_jardim(ic):
    key(ic, mix(BRONZE, LEAF, 0.35), bow_shape="leaf")
    ic.poly([(52, 58), (62, 48), (66, 60)], LEAF)


def r_odre_agua(ic):
    ic.poly([(40, 36), (88, 36), (104, 74), (90, 108), (36, 108), (24, 74)], LEATHER)
    ic.stroke([(30, 76), (98, 76)], 2, darker(LEATHER, 0.35))
    ic.rect(54, 18, 74, 38, darker(LEATHER, 0.2))
    ic.rect(56, 10, 72, 20, WOOD)
    ic.stroke([(40, 40), (20, 20), (60, 14)], 2.4, BROWN)
    ic.poly([(70, 82), (78, 94), (74, 102), (66, 102), (62, 94)], WATER)


def r_teriaga(ic):
    pot(ic, (176, 110, 72), contents=(110, 40, 30), label=CREAM_DIM)
    ic.poly([(56, 66), (72, 66), (64, 80)], CLAY_RED, shade=False)
    ic.glow(64, 22, 16, CANE_GREEN, 90)


def r_manta_la(ic):
    for i, c in enumerate((WOOL, (196, 84, 60), WOOL)):
        y = 34 + i * 22
        ic.poly([(20, y), (104, y - 4), (110, y + 22), (16, y + 26)], c)
    for x in (34, 54, 74, 94):
        ic.stroke([(x, 34), (x - 2, 98)], 1.8, (150, 60, 40))
    for x in range(22, 108, 8):
        ic.stroke([(x, 102), (x - 1, 110)], 1.6, CREAM_DIM)


def r_cha_gengibre(ic):
    for x in (52, 66, 80):
        ic.stroke([(x, 34), (x - 5, 24), (x + 2, 14)], 2.6, CREAM)
    ic.poly([(30, 42), (98, 42), (88, 96), (40, 96)], (178, 96, 60))
    ic.poly([(34, 46), (94, 46), (90, 54), (38, 54)], (214, 160, 70), outline=False, shade=False)
    ic.stroke([(98, 52), (112, 60), (94, 82)], 4, (150, 80, 50))
    ic.poly([(14, 98), (36, 92), (52, 100), (44, 110), (20, 110)], (220, 180, 110))
    ic.rect(28, 96, 100, 104, darker((178, 96, 60), 0.2))


def lamp(ic, body, flame_col, core, rune=None):
    ic.poly([(18, 82), (42, 66), (92, 66), (110, 56), (104, 74), (92, 96), (36, 96)], body)
    ic.rect(50, 58, 80, 68, darker(body, 0.15))
    ic.stroke([(36, 72), (20, 56), (32, 48)], 3, darker(body, 0.3))
    ic.rect(102, 54, 112, 60, darker(body, 0.25))
    flame(ic, 106, 40, 10, flame_col, core)
    if rune:
        ic.glow(64, 82, 14, rune, 160)
        draw_rune(ic, 64, 82, 8, rune)


def r_lamparina_oleo(ic):
    lamp(ic, (176, 104, 64), FLAME, FLAME_CORE)


def r_lamparina_runica(ic):
    lamp(ic, (92, 88, 130), RUNE_BLUE, (210, 230, 255), rune=(140, 230, 255))


def r_oleo_fogo_eterno(ic):
    bottle(ic, (240, 110, 30), round_body=False)
    flame(ic, 64, 82, 12)


def r_passagem_dhow(ic):
    def sail(ic2):
        ic2.poly([(48, 46), (80, 46), (74, 70), (46, 70)], (0, 0, 0), shade=False, outline=False)
    ic.rect(18, 34, 110, 94, CREAM_DIM)
    ic.poly([(18, 34), (110, 34), (110, 46), (18, 46)], TURQUOISE, outline=False)
    for y in (58, 70, 82):
        ic.stroke([(26, y), (54, y)], 2, darker(CREAM_DIM, 0.35))
    ic.poly([(64, 52), (64, 82), (90, 80)], CREAM)
    ic.stroke([(64, 50), (64, 84)], 2, WOOD_DARK)
    ic.poly([(58, 84), (98, 84), (92, 90), (62, 90)], WOOD)
    ic.stroke([(82, 34), (82, 94)], 2, darker(CREAM_DIM, 0.4))


def r_reliquia_artifices(ic):
    ic.ngon(64, 64, 44, 10, BRONZE, rot=0)
    ic.ngon(64, 64, 34, 10, darker(BRONZE, 0.1), rot=18)
    ic.ngon(64, 64, 22, 6, (110, 96, 70), rot=0)
    ic.glow(64, 64, 16, TURQUOISE, 180)
    ic.ngon(64, 64, 11, 4, TURQUOISE, rot=45)
    for i in range(10):
        a = math.radians(i * 36)
        ic.dot(64 + 39 * math.cos(a), 64 + 39 * math.sin(a), 2.2, NIGHT)


def r_concha_respiravel(ic):
    ic.poly([(64, 100), (20, 54), (30, 30), (64, 20), (98, 30), (108, 54)], (240, 190, 170))
    for x in (34, 48, 64, 80, 94):
        ic.stroke([(64, 98), (x, 26 + abs(64 - x) * 0.25)], 2, darker((240, 190, 170), 0.3))
    ic.rect(52, 96, 76, 110, (220, 160, 140))
    for (x, y, r) in ((92, 18, 7), (104, 34, 5), (84, 8, 4)):
        ic.ngon(x, y, r, 8, SKY)


def r_bolha_ar(ic):
    bottle(ic, (200, 236, 250), glass=(170, 220, 240))
    for (x, y, r) in ((54, 74, 7), (72, 84, 9), (64, 62, 5), (78, 70, 4)):
        ic.ngon(x, y, r, 8, (240, 252, 255))


def r_asas_lona_takhsich(ic):
    # Asas de Lona do Takhsich: armação de cobre e lona encerada (pulo duplo do Ninho de Mahaway, mapa 24).
    canvas = (201, 180, 138)
    ic.glow(64, 58, 50, (201, 184, 240), 110)
    for side in (-1, 1):
        def X(x):
            return 64 + side * x
        ic.poly([(X(8), 52), (X(56), 22), (X(58), 70), (X(12), 74)], canvas)
        ic.poly([(X(30), 38), (X(56), 22), (X(57), 46)], darker(canvas, 0.12), outline=False)
        for (x0, y0, x1, y1) in ((8, 52, 56, 22), (10, 63, 58, 46), (12, 74, 58, 70)):
            ic.stroke([(X(x0), y0), (X(x1), y1)], 3, COPPER)
    ic.ngon(64, 62, 11, 8, COPPER)
    ic.ngon(64, 62, 5, 8, (201, 184, 240))


def r_garrafa_ar_sereno(ic):
    # Garrafa de Ar Sereno dos kobolds: vidro verde com ar quieto e estrelado.
    bottle(ic, (201, 184, 240), glass=(127, 191, 143), neck_col=COPPER)
    for (x, y, r) in ((56, 80, 4), (72, 72, 3), (66, 92, 3), (78, 86, 2)):
        ic.ngon(x, y, r, 4, (255, 250, 230), rot=45)


def r_botas_raiz(ic):
    boot(ic, (110, 84, 48), sole=WOOD_DARK, cuff=LEAF)
    ic.stroke([(44, 40), (52, 70), (46, 96)], 3, LEAF_DARK)
    ic.stroke([(62, 36), (58, 64), (70, 90)], 3, LEAF_DARK)
    ic.poly([(70, 12), (84, 4), (80, 20)], LEAF)
    ic.stroke([(34, 110), (24, 118)], 3, WOOD_DARK)
    ic.stroke([(100, 110), (110, 118)], 3, WOOD_DARK)


def r_vara_equilibrio(ic):
    ic.bar(12, 82, 116, 46, 7, WOOD)
    ic.ngon(14, 82, 11, 6, CLAY_RED)
    ic.ngon(114, 46, 11, 6, CLAY_RED)
    ic.rect(56, 58, 72, 72, LEATHER)


def r_barco_junco(ic):
    ic.poly([(8, 56), (24, 82), (104, 82), (120, 56), (100, 70), (28, 70)], REED)
    for x in (34, 54, 74, 94):
        ic.stroke([(x, 70), (x, 82)], 2.2, darker(REED, 0.45))
    ic.stroke([(64, 70), (64, 18)], 3, WOOD_DARK)
    ic.poly([(66, 22), (98, 56), (66, 60)], CREAM)
    ic.poly([(20, 92), (108, 92), (100, 100), (28, 100)], WATER, outline=False)


def r_amuleto_antiveneno(ic):
    ic.stroke([(30, 20), (64, 50), (98, 20)], 2.6, BROWN)
    ic.ngon(64, 76, 30, 6, BRONZE, rot=-90)
    gem(ic, (90, 200, 120), 64, 76, 18, 6)
    ic.stroke([(54, 70), (60, 66), (66, 74), (72, 70)], 2.4, NIGHT)


def r_fruta_purificadora(ic):
    ic.glow(64, 70, 46, (200, 250, 255), 160)
    ic.ngon(64, 72, 36, 9, (190, 236, 240), rot=10)
    ic.ngon(56, 62, 12, 7, (240, 255, 255), outline=False)
    ic.bar(64, 38, 70, 20, 5, WOOD_DARK)
    ic.poly([(70, 26), (96, 14), (90, 34)], LEAF)


def r_roupa_termica(ic):
    tunic(ic, (200, 110, 60), trim=WOOL, quilted=True)


def tunic(ic, col, trim=None, quilted=False):
    ic.poly([(40, 18), (88, 18), (114, 40), (102, 56), (92, 48), (92, 108), (36, 108), (36, 48), (26, 56),
             (14, 40)], col)
    ic.poly([(52, 18), (76, 18), (64, 34)], darker(col, 0.4), shade=False)
    if quilted:
        for y in (54, 70, 86):
            ic.stroke([(38, y), (90, y)], 2, darker(col, 0.3))
        ic.stroke([(64, 36), (64, 106)], 2, darker(col, 0.3))
    if trim:
        ic.poly([(36, 100), (92, 100), (92, 108), (36, 108)], trim)
        ic.poly([(40, 18), (88, 18), (84, 24), (44, 24)], trim)


def r_grampos_escalada(ic):
    for (dx, rot) in ((-18, -12), (18, 12)):
        cx = 64 + dx
        ic.poly([(cx - 6, 20), (cx + 6, 20), (cx + 4, 90), (cx, 110), (cx - 4, 90)], STEEL)
        ic.ngon(cx, 26, 10, 8, IRON_DARK, rot=rot)
        ic.ngon(cx, 26, 5, 8, (0, 0, 0), shade=False, outline=False)
    ic.img.paste((0, 0, 0, 0), (0, 0), ic._mask(lambda d: (
        d.ellipse([41 * SS, 21 * SS, 51 * SS, 31 * SS], fill=255),
        d.ellipse([77 * SS, 21 * SS, 87 * SS, 31 * SS], fill=255))))


def r_capa_tempestade(ic):
    cloak(ic, (52, 66, 120), trim=(90, 110, 170), bolt=True)


def r_selo_sol(ic):
    ic.glow(64, 64, 52, GOLD, 170)
    for i in range(12):
        a = math.radians(i * 30)
        ic.poly([(64 + 30 * math.cos(a - 0.2), 64 + 30 * math.sin(a - 0.2)),
                 (64 + 54 * math.cos(a), 64 + 54 * math.sin(a)),
                 (64 + 30 * math.cos(a + 0.2), 64 + 30 * math.sin(a + 0.2))], GOLD)
    ic.ngon(64, 64, 32, 12, (230, 160, 40), rot=15)
    ic.ngon(64, 64, 22, 12, GOLD_LIGHT, rot=0)
    ic.dot(57, 61, 2.6, BROWN)
    ic.dot(71, 61, 2.6, BROWN)
    ic.stroke([(56, 70), (64, 75), (72, 70)], 2.4, BROWN)


# ---- Ossuário
def r_armor_bone_chest(ic):
    tunic(ic, (120, 86, 60))
    for i, y in enumerate((44, 58, 72, 86)):
        w = 22 - i * 2
        ic.poly([(64, y - 3), (64 + w, y), (64 + w - 2, y + 6), (64, y + 3)], BONE)
        ic.poly([(64, y - 3), (64 - w, y), (64 - w + 2, y + 6), (64, y + 3)], BONE)
    ic.rect(61, 38, 67, 96, BONE_DARK)


def r_armor_bone_helm(ic):
    ic.poly([(26, 70), (30, 38), (50, 18), (78, 18), (98, 38), (102, 70), (90, 100), (38, 100)], BONE)
    ic.poly([(26, 64), (102, 64), (102, 74), (26, 74)], COPPER)
    ic.ngon(48, 54, 9, 6, NIGHT, shade=False)
    ic.ngon(80, 54, 9, 6, NIGHT, shade=False)
    ic.poly([(64, 60), (70, 78), (58, 78)], NIGHT, shade=False)
    for x in (46, 56, 66, 76):
        ic.rect(x, 86, x + 7, 98, CREAM)
    ic.poly([(56, 16), (64, 4), (72, 16)], COPPER)


# ---- conjuntos das Regiões 13–16 (Docs/Items.md)
PAPYRUS = (206, 186, 118)
GLAZED_BLUE = (47, 95, 168)
SEAL_BRONZE = (110, 84, 60)
SOLID_LIGHT = (250, 240, 200)
LIGHT_LILAC = (199, 182, 238)


def r_armor_papiro_chest(ic):
    tunic(ic, PAPYRUS, trim=darker(PAPYRUS, 0.3))
    for x in (46, 56, 66, 76, 84):
        ic.stroke([(x, 34), (x, 98)], 2, darker(PAPYRUS, 0.3))
    for y in (52, 70, 88):
        ic.stroke([(38, y), (90, y)], 1.6, darker(PAPYRUS, 0.2))


def r_armor_papiro_helm(ic):
    ic.poly([(26, 82), (34, 42), (52, 22), (76, 22), (94, 42), (102, 82)], PAPYRUS)
    for x in (42, 54, 66, 78, 88):
        ic.stroke([(x, 30), (x + (x - 64) * 0.2, 82)], 2, darker(PAPYRUS, 0.3))
    ic.rect(24, 80, 104, 92, darker(PAPYRUS, 0.3))
    ic.poly([(58, 22), (64, 8), (70, 22)], LEAF_DARK)


def r_armor_tijolo_chest(ic):
    tunic(ic, GLAZED_BLUE, trim=GOLD)
    for y in (50, 66, 82):
        for x in range(42, 88, 14):
            ic.rect(x, y, x + 12, y + 13, lighter(GLAZED_BLUE, 0.2 if (x + y) % 4 else 0.05))


def r_armor_tijolo_helm(ic):
    ic.poly([(26, 72), (30, 40), (50, 20), (78, 20), (98, 40), (102, 72), (90, 104), (38, 104)], GLAZED_BLUE)
    for (x, y) in ((22, 44), (20, 66), (28, 88), (106, 44), (108, 66), (100, 88)):
        ic.ngon(x, y, 11, 6, GOLD)
    ic.ngon(48, 56, 8, 6, GOLD_LIGHT, shade=False)
    ic.ngon(80, 56, 8, 6, GOLD_LIGHT, shade=False)
    ic.poly([(64, 62), (72, 80), (56, 80)], darker(GLAZED_BLUE, 0.4), shade=False)
    ic.rect(50, 88, 78, 96, CREAM, shade=False)


def r_armor_selo_chest(ic):
    tunic(ic, SEAL_BRONZE, quilted=True)
    for (x, y) in ((50, 54), (78, 54), (64, 72), (50, 90), (78, 90)):
        ic.ngon(x, y, 8, 8, FLAME)
        ic.stroke([(x - 4, y - 3), (x + 1, y + 1), (x - 1, y + 5)], 1.6, NIGHT)


def r_armor_selo_helm(ic):
    ic.poly([(26, 74), (30, 40), (50, 20), (78, 20), (98, 40), (102, 74), (90, 104), (38, 104)], SEAL_BRONZE)
    ic.rect(26, 62, 102, 74, darker(SEAL_BRONZE, 0.3))
    ic.rect(40, 64, 88, 72, NIGHT, shade=False)
    ic.glow(64, 44, 14, FLAME, 180)
    ic.ngon(64, 44, 12, 8, FLAME_CORE)
    ic.stroke([(60, 38), (66, 44), (62, 52)], 2, NIGHT)


def r_armor_luz_chest(ic):
    ic.glow(64, 64, 52, SOLID_LIGHT, 160)
    tunic(ic, SOLID_LIGHT, trim=GOLD)
    ic.stroke([(64, 36), (64, 104)], 3, GOLD)
    ic.ngon(64, 62, 10, 4, LIGHT_LILAC, rot=45)


def r_armor_luz_helm(ic):
    ic.glow(64, 58, 44, SOLID_LIGHT, 170)
    ic.poly([(24, 84), (32, 62), (96, 62), (104, 84), (90, 94), (38, 94)], GOLD)
    for x, h in ((36, 28), (50, 40), (64, 52), (78, 40), (92, 28)):
        ic.poly([(x - 7, 64), (x, 64 - h), (x + 7, 64)], SOLID_LIGHT)
    ic.ngon(64, 78, 9, 4, LIGHT_LILAC, rot=45)


def r_item_bone_fragment(ic):
    ic.bar(30, 92, 94, 36, 14, BONE)
    for (x, y) in ((24, 92), (34, 102), (90, 26), (100, 38)):
        ic.ngon(x, y, 10, 7, BONE)
    ic.poly([(70, 52), (80, 46), (76, 60)], BONE_DARK, outline=False)


def r_item_qocho_holed_coin(ic):
    coin(ic, BRONZE)


def r_item_rusty_watch_sword(ic):
    sword(ic, (150, 116, 90), guard=RUST)
    for (x, y) in ((86, 38), (72, 54), (62, 66)):
        ic.dot(x, y, 3, RUST)


def r_item_bone_key(ic):
    key(ic, BONE, bow_shape="skull")
    ic.glow(84, 50, 10, (140, 230, 255), 160)
    draw_rune(ic, 70, 58, 5, (90, 200, 240))


# ---- demais itens existentes
def r_axe(ic):
    tool(ic, IRON, "axe")


def r_pickaxe(ic):
    tool(ic, IRON, "pickaxe")


def r_hammer(ic):
    tool(ic, IRON_DARK, "hammer")


def r_iron_sword(ic):
    sword(ic, STEEL)


def r_wooden_sword(ic):
    sword(ic, WOOD, guard=WOOD_DARK, wood=True)


def r_hunting_bow(ic):
    ic.stroke([(36, 14), (98, 104)], 1.6, CREAM)
    ic.poly([(32, 10), (44, 12), (78, 34), (100, 70), (102, 106), (92, 104), (88, 74), (68, 42)], WOOD)
    ic.rect(76, 52, 88, 66, LEATHER)
    ic.bar(16, 108, 94, 30, 3, WOOD_DARK)
    ic.poly([(94, 30), (102, 20), (98, 36)], STEEL)


def r_leather_boots(ic):
    boot(ic, LEATHER, cuff=darker(LEATHER, 0.2))


def r_leather_chest(ic):
    tunic(ic, LEATHER, trim=darker(LEATHER, 0.25))


def r_leather_helmet(ic):
    ic.poly([(24, 78), (30, 40), (50, 20), (78, 20), (98, 40), (104, 78), (88, 70), (40, 70)], LEATHER)
    ic.rect(22, 72, 106, 84, darker(LEATHER, 0.25))
    ic.stroke([(64, 22), (64, 70)], 2.4, darker(LEATHER, 0.4))


def r_health_potion(ic):
    bottle(ic, (220, 50, 60))


def r_mana_potion(ic):
    bottle(ic, (60, 100, 230))


def r_cooked_meat(ic):
    ic.bar(78, 50, 108, 20, 9, BONE)
    ic.ngon(104, 18, 8, 6, BONE)
    ic.ngon(112, 26, 8, 6, BONE)
    ic.ngon(56, 74, 38, 9, (150, 70, 40), rot=20, ry=30)
    ic.ngon(50, 70, 20, 7, (190, 100, 56), rot=20, outline=False)


def r_mount_feed(ic):
    ic.poly([(30, 40), (98, 40), (108, 108), (20, 108)], (196, 170, 120))
    ic.poly([(40, 20), (88, 20), (98, 40), (30, 40)], (176, 150, 100))
    for (x, y) in ((50, 22), (64, 18), (78, 22)):
        ic.ngon(x, y, 6, 6, GOLD)
    ic.stroke([(30, 42), (98, 42)], 3, BROWN)
    ic.poly([(56, 64), (72, 64), (76, 84), (52, 84)], CLAY_RED, shade=False)


def r_wood(ic):
    for (y, x0) in ((80, 14), (54, 22)):
        ic.rect(x0, y - 12, x0 + 80, y + 12, WOOD)
        ic.ngon(x0 + 80, y, 13, 8, (214, 170, 110))
        ic.ngon(x0 + 80, y, 6, 8, (180, 130, 80), outline=False)
    ic.rect(30, 30, 98, 52, WOOD)
    ic.ngon(98, 41, 12, 8, (214, 170, 110))


def r_stone(ic):
    ic.poly([(20, 90), (30, 50), (60, 30), (96, 40), (110, 76), (92, 104), (40, 106)], (140, 136, 128))
    ic.poly([(60, 30), (76, 60), (110, 76), (96, 40)], (166, 160, 150), outline=False)


def r_iron_ore(ic):
    r_stone(ic)
    for (x, y) in ((44, 70), (70, 52), (80, 84), (58, 92), (94, 64)):
        ic.ngon(x, y, 6, 5, (200, 120, 80), rot=x)


def r_iron(ic):
    for (dx, dy) in ((0, 18), (-14, -6), (14, -6)):
        cx, cy = 64 + dx, 64 + dy
        ic.poly([(cx - 26, cy + 10), (cx - 18, cy - 10), (cx + 18, cy - 10), (cx + 26, cy + 10)], IRON)


def r_gold(ic):
    for (x, y, r) in ((44, 82, 20), (80, 84, 18), (62, 60, 20), (84, 56, 12), (40, 56, 12)):
        ic.ngon(x, y, r, 6, GOLD, rot=x)
    ic.glow(62, 60, 20, GOLD_LIGHT, 100)


def r_gold_coin(ic):
    coin(ic, GOLD, cx=50, cy=72, r=30)
    coin(ic, GOLD, cx=78, cy=54, r=30)


def r_herb(ic):
    ic.stroke([(64, 112), (64, 40)], 3, LEAF_DARK)
    for (x, y, s) in ((40, 60, -1), (88, 52, 1), (44, 86, -1), (86, 82, 1), (64, 26, 0)):
        if s == 0:
            ic.poly([(64, 8), (76, 30), (64, 44), (52, 30)], LEAF)
        else:
            ic.poly([(64, y + 10), (x, y - 10), (x - 10 * s, y + 6)], LEAF)


def r_fiber(ic):
    for i in range(-3, 4):
        ic.stroke([(64 + i * 3, 110), (64 + i * 9, 18)], 2.6, mix(REED, LEAF, 0.35 + 0.05 * i))
    ic.rect(46, 64, 82, 76, BROWN)


def r_fruta_mandacaru(ic):  # Melão de Luz
    ic.glow(64, 70, 44, GOLD_LIGHT, 140)
    ic.ngon(64, 70, 40, 10, (236, 210, 90), ry=32)
    for x in (44, 64, 84):
        ic.stroke([(x, 42), (x + (x - 64) * 0.2, 100)], 2, (200, 170, 60))
    ic.poly([(64, 38), (84, 22), (80, 36)], LEAF)


def r_fruta_umbu(ic):  # Romã
    ic.ngon(64, 72, 38, 9, (190, 40, 60), rot=10)
    ic.poly([(54, 34), (58, 24), (64, 32), (70, 24), (74, 34)], (150, 30, 50))
    for (x, y) in ((50, 70), (60, 80), (74, 66), (70, 86), (82, 78)):
        ic.ngon(x, y, 4, 6, (255, 140, 160), outline=False)


def r_runa(col, big=False):
    def f(ic):
        scroll(ic, paper=CREAM, seal=CLAY_RED if big else None, ribbon=GOLD if big else None)
        ic.glow(64, 58, 16, col, 150)
        draw_rune(ic, 64, 58, 10, col)
    return f


def r_pedra(col, sides=6):
    return lambda ic: gem(ic, col, sides=sides)


def r_pedra_bencao(ic):
    ic.ngon(64, 70, 34, 8, (200, 196, 184), ry=26)
    ic.glow(64, 66, 18, GOLD_LIGHT, 220)
    ic.poly([(64, 52), (68, 62), (78, 66), (68, 70), (64, 80), (60, 70), (50, 66), (60, 62)], GOLD_LIGHT,
            shade=False)


def r_pedra_caos(ic):
    gem(ic, (110, 60, 160))
    ic.poly([(64, 66), (95, 48), (95, 84), (64, 102)], (240, 220, 130), outline=False)
    ic.poly([(64, 66), (33, 84), (64, 102)], (40, 30, 60), outline=False)


def r_parte_asa_morcego(ic):
    ic.poly([(18, 40), (64, 30), (110, 40), (98, 62), (86, 56), (78, 74), (64, 64), (50, 74), (42, 56), (30, 62)],
            (90, 70, 90))
    for x in (30, 50, 78, 98):
        ic.stroke([(64, 32), (x, 64)], 1.8, (60, 44, 60))


def r_parte_ferrao_escorpiao(ic):
    ic.poly([(30, 100), (40, 70), (62, 46), (88, 30), (104, 14), (98, 40), (74, 60), (52, 90)], (176, 120, 60))
    ic.poly([(100, 16), (116, 8), (110, 26)], (90, 200, 120))


def r_parte_lasca_golem(ic):
    ic.poly([(30, 100), (40, 40), (70, 16), (96, 48), (88, 108)], (190, 150, 100))
    ic.poly([(40, 40), (70, 16), (64, 60)], (214, 180, 130), outline=False)
    draw_rune(ic, 66, 76, 10, TURQUOISE)


def r_parte_presa_lobo(ic):
    ic.poly([(40, 20), (88, 20), (82, 50), (70, 96), (64, 112), (58, 96), (46, 50)], BONE)
    ic.rect(40, 18, 88, 30, BONE_DARK)


def r_parte_seda_aranha(ic):
    ic.ngon(64, 64, 40, 10, (236, 236, 244))
    for r in (28, 16):
        ic.ngon(64, 64, r, 10, (220, 220, 232), outline=False)
    for i in range(5):
        a = math.radians(i * 72 + 10)
        ic.stroke([(64, 64), (64 + 38 * math.cos(a), 64 + 38 * math.sin(a))], 1.6, (170, 170, 190))


def r_parte_trofeu_fera(ic):
    ic.poly([(36, 52), (44, 30), (84, 30), (92, 52), (84, 92), (64, 104), (44, 92)], BONE)
    ic.poly([(40, 36), (14, 22), (10, 6), (30, 18), (48, 30)], CREAM_DIM)
    ic.poly([(88, 36), (114, 22), (118, 6), (98, 18), (80, 30)], CREAM_DIM)
    ic.ngon(52, 56, 7, 6, NIGHT, shade=False)
    ic.ngon(76, 56, 7, 6, NIGHT, shade=False)
    ic.glow(64, 80, 10, GOLD, 200)


def r_amuleto_sorte(ic):  # Bes de barro
    ic.ngon(64, 40, 26, 9, (190, 120, 80))
    ic.poly([(40, 60), (88, 60), (98, 108), (30, 108)], (176, 108, 70))
    ic.dot(55, 38, 3.2, NIGHT)
    ic.dot(73, 38, 3.2, NIGHT)
    ic.stroke([(52, 50), (64, 56), (76, 50)], 2.6, NIGHT)
    for x in (48, 64, 80):
        ic.poly([(x - 6, 16), (x, 2), (x + 6, 16)], GOLD)


def r_patua(ic):  # Tumar da Vó Edna
    ic.stroke([(36, 16), (64, 36), (92, 16)], 2.6, BROWN)
    ic.poly([(24, 40), (104, 40), (64, 110)], (180, 70, 60))
    ic.poly([(34, 46), (94, 46), (64, 98)], (220, 170, 80), outline=False)
    ic.ngon(64, 60, 8, 6, TURQUOISE)


def r_bornal_couro(ic):
    ic.poly([(24, 44), (104, 44), (100, 108), (28, 108)], (150, 120, 90))
    ic.poly([(24, 44), (104, 44), (96, 76), (32, 76)], darker((150, 120, 90), 0.15))
    ic.stroke([(28, 46), (44, 14), (84, 14), (100, 46)], 3, BROWN)
    ic.ngon(64, 72, 7, 6, GOLD)
    ic.stroke([(36, 92), (92, 92)], 2, CLAY_RED)


def r_chave_engenho(ic):
    key(ic, IRON, gem=CLAY_RED)


def r_convite_terreiro(ic):
    ic.poly([(16, 36), (112, 36), (112, 100), (16, 100)], CREAM)
    ic.poly([(16, 36), (64, 74), (112, 36)], CREAM_DIM)
    ic.ngon(64, 72, 12, 9, CLAY_RED, rot=10)


def r_whistle(col):
    return lambda ic: whistle(ic, col)


def r_asas(col, tier, accent=None, glowy=False):
    return lambda ic: wings(ic, col, tier, accent, glowy)


def r_capa(tier):
    trims = {1: None, 2: CARAMEL, 3: GOLD}
    return lambda ic: cloak(ic, (110, 74, 48), trim=trims[tier])


def r_fish(**kw):
    return lambda ic: fish(ic, **kw)


# ---------------------------------------------------------------- lembranças dos chefes do mundo (HouseCraftRules)
def scale_plate(ic, col, edge, glint=None, cx=64, cy=62):
    """Escama: gota invertida facetada com nervuras."""
    if glint:
        ic.glow(cx, cy, 40, glint, 120)
    ic.poly([(cx, cy - 44), (cx + 34, cy - 18), (cx + 30, cy + 22), (cx, cy + 46), (cx - 30, cy + 22), (cx - 34, cy - 18)], col)
    ic.poly([(cx, cy - 30), (cx + 20, cy - 10), (cx, cy + 30), (cx - 20, cy - 10)], lighter(col, 0.18), outline=False)
    for dx in (-14, 0, 14):
        ic.stroke([(cx + dx * 0.4, cy - 34), (cx + dx, cy + 30)], 1.6, edge)


def r_chefe_escama_hedammu(ic):
    scale_plate(ic, (70, 140, 120), (40, 90, 80), glint=GOLD_LIGHT)
    for x, y in ((50, 48), (76, 60), (60, 80)):
        ic.dot(x, y, 2.6, GOLD_LIGHT)


def r_chefe_lama_limpa(ic):
    ic.ngon(64, 74, 42, 11, (110, 92, 66), ry=28)
    ic.ngon(60, 66, 28, 9, (134, 112, 80), ry=18, outline=False)
    ic.glow(64, 64, 18, (150, 230, 220), 200)
    ic.poly([(64, 46), (74, 62), (64, 74), (54, 62)], (150, 230, 220), shade=False)


def r_chefe_escama_aydahar(ic):
    scale_plate(ic, (150, 190, 214), (90, 120, 150), glint=SKY)
    ic.stroke([(40, 30), (88, 94)], 1.6, (240, 250, 255))


def r_chefe_bronze_antigo(ic):
    ic.poly([(22, 40), (60, 22), (104, 34), (98, 92), (56, 106), (28, 88)], BRONZE)
    ic.poly([(22, 40), (60, 22), (104, 34), (64, 48)], lighter(BRONZE, 0.2), outline=False)
    ic.stroke([(36, 62), (48, 54), (60, 62), (72, 54), (86, 62)], 2.4, (70, 130, 110))
    ic.stroke([(36, 78), (48, 70), (60, 78), (72, 70), (86, 78)], 2.4, (70, 130, 110))


def r_chefe_ferro_antigo(ic):
    ic.ngon(64, 64, 44, 10, IRON_DARK, ry=30, rot=-30)
    ic.img.paste((0, 0, 0, 0), (0, 0), ic._mask(lambda d: d.ellipse([40 * SS, 50 * SS, 88 * SS, 78 * SS], fill=255)))
    ic.dot(36, 54, 3, RUST)
    ic.dot(90, 76, 3.5, RUST)


def r_chefe_pena_vendaval(ic):
    ic.glow(64, 64, 34, SKY, 110)
    ic.poly([(30, 106), (44, 70), (66, 38), (100, 16), (94, 48), (74, 78), (40, 104)], (220, 230, 240))
    ic.poly([(66, 38), (100, 16), (94, 48)], GOLD, outline=False)
    ic.stroke([(28, 110), (96, 22)], 2.2, BROWN)


def r_chefe_barbatana_kul(ic):
    ic.glow(64, 64, 36, (200, 220, 240), 130)
    ic.poly([(20, 100), (40, 34), (70, 14), (108, 40), (86, 66), (100, 100)], (200, 208, 220))
    for x in (40, 58, 76):
        ic.stroke([(30, 98), (x, 30 + (x - 40) * 0.2)], 1.6, (150, 160, 180))


def r_chefe_brasa_viva(ic):
    ic.ngon(64, 76, 30, 7, (60, 40, 34), rot=12)
    ic.poly([(46, 70), (64, 58), (82, 70), (70, 88), (56, 88)], (220, 90, 40), outline=False)
    flame(ic, 64, 54, 18)


def r_chefe_escama_apep(ic):
    scale_plate(ic, (70, 60, 90), (40, 30, 60), glint=(150, 110, 210))
    ic.stroke([(64, 30), (64, 96)], 2, (170, 140, 220))


def r_chefe_tabua_sonho(ic):
    ic.poly([(30, 22), (98, 18), (102, 104), (26, 108)], (190, 150, 110))
    for y in (36, 50, 64, 78, 92):
        for x in range(38, 92, 10):
            ic.poly([(x, y), (x + 5, y - 3), (x + 6, y + 3)], (130, 96, 66), shade=False, outline=False)
    ic.poly([(98, 18), (102, 46), (86, 30)], (160, 120, 84))


def r_chefe_lasca_selo(ic):
    ic.glow(64, 64, 38, GOLD_LIGHT, 170)
    ic.poly([(28, 92), (40, 36), (76, 16), (100, 52), (82, 104)], (60, 56, 70))
    ic.ngon(64, 62, 12, 8, GOLD_LIGHT)
    ic.stroke([(40, 36), (60, 58), (82, 104)], 2, GOLD)


def r_chefe_semente_luz(ic):
    ic.glow(64, 64, 42, GOLD_LIGHT, 220)
    ic.ngon(64, 70, 26, 10, (250, 236, 170), ry=34)
    ic.poly([(64, 34), (70, 22), (82, 18), (74, 30)], LEAF)
    ic.poly([(54, 56), (60, 50), (58, 70)], (255, 255, 255), shade=False, outline=False)


RECIPES = {
    # exigências de mapa (MapRequirementRules)
    "item_corda_escalada": r_corda_escalada,
    "item_chave_jardim": r_chave_jardim,
    "item_odre_agua": r_odre_agua,
    "item_teriaga": r_teriaga,
    "item_manta_la": r_manta_la,
    "item_cha_gengibre": r_cha_gengibre,
    "item_lamparina_oleo": r_lamparina_oleo,
    "item_lamparina_runica": r_lamparina_runica,
    "item_oleo_fogo_eterno": r_oleo_fogo_eterno,
    "item_passagem_dhow": r_passagem_dhow,
    "item_reliquia_artifices": r_reliquia_artifices,
    "item_concha_respiravel": r_concha_respiravel,
    "item_bolha_ar": r_bolha_ar,
    "item_botas_raiz": r_botas_raiz,
    "item_vara_equilibrio": r_vara_equilibrio,
    "item_barco_junco": r_barco_junco,
    "item_amuleto_antiveneno": r_amuleto_antiveneno,
    "item_fruta_purificadora": r_fruta_purificadora,
    "item_roupa_termica": r_roupa_termica,
    "item_grampos_escalada": r_grampos_escalada,
    "item_capa_tempestade": r_capa_tempestade,
    "item_selo_sol": r_selo_sol,
    "item_asas_lona_takhsich": r_asas_lona_takhsich,
    "item_garrafa_ar_sereno": r_garrafa_ar_sereno,
    # Ossuário dos Artífices
    "armor_bone_chest": r_armor_bone_chest,
    "armor_bone_helm": r_armor_bone_helm,
    "item_bone_fragment": r_item_bone_fragment,
    "item_qocho_holed_coin": r_item_qocho_holed_coin,
    "item_rusty_watch_sword": r_item_rusty_watch_sword,
    "item_bone_key": r_item_bone_key,
    # Conjuntos das Regiões 13–16
    "armor_papiro_chest": r_armor_papiro_chest,
    "armor_papiro_helm": r_armor_papiro_helm,
    "armor_tijolo_chest": r_armor_tijolo_chest,
    "armor_tijolo_helm": r_armor_tijolo_helm,
    "armor_selo_chest": r_armor_selo_chest,
    "armor_selo_helm": r_armor_selo_helm,
    "armor_luz_chest": r_armor_luz_chest,
    "armor_luz_helm": r_armor_luz_helm,
    # ferramentas / armas / armaduras
    "item_axe": r_axe,
    "item_pickaxe": r_pickaxe,
    "item_hammer": r_hammer,
    "item_iron_sword": r_iron_sword,
    "item_wooden_sword": r_wooden_sword,
    "item_hunting_bow": r_hunting_bow,
    "item_leather_boots": r_leather_boots,
    "item_leather_chest": r_leather_chest,
    "item_leather_helmet": r_leather_helmet,
    # consumíveis
    "item_health_potion": r_health_potion,
    "item_mana_potion": r_mana_potion,
    "item_cooked_meat": r_cooked_meat,
    "item_mount_feed": r_mount_feed,
    "item_fruta_mandacaru": r_fruta_mandacaru,
    "item_fruta_umbu": r_fruta_umbu,
    "item_runa_clareza": r_runa((110, 170, 255)),
    "item_runa_couro": r_runa(BROWN_LIGHT),
    "item_runa_cura": r_runa((220, 60, 70)),
    "item_runa_cura_grande": r_runa((230, 60, 70), big=True),
    "item_runa_vento": r_runa(TURQUOISE),
    # materiais
    "item_wood": r_wood,
    "item_stone": r_stone,
    "item_iron_ore": r_iron_ore,
    "item_iron": r_iron,
    "item_gold": r_gold,
    "item_gold_coin": r_gold_coin,
    "item_herb": r_herb,
    "item_fiber": r_fiber,
    # refino / partes de bicho
    "item_pedra_alma": r_pedra((80, 130, 220)),
    "item_pedra_bencao": r_pedra_bencao,
    "item_pedra_caos": r_pedra_caos,
    "item_pedra_vida": r_pedra((90, 200, 160)),
    "item_parte_asa_morcego": r_parte_asa_morcego,
    "item_parte_ferrao_escorpiao": r_parte_ferrao_escorpiao,
    "item_parte_lasca_golem": r_parte_lasca_golem,
    "item_parte_presa_lobo": r_parte_presa_lobo,
    "item_parte_seda_aranha": r_parte_seda_aranha,
    "item_parte_trofeu_fera": r_parte_trofeu_fera,
    # lembranças dos chefes do mundo (Regiões 5–16)
    "item_chefe_escama_hedammu": r_chefe_escama_hedammu,
    "item_chefe_lama_limpa": r_chefe_lama_limpa,
    "item_chefe_escama_aydahar": r_chefe_escama_aydahar,
    "item_chefe_bronze_antigo": r_chefe_bronze_antigo,
    "item_chefe_ferro_antigo": r_chefe_ferro_antigo,
    "item_chefe_pena_vendaval": r_chefe_pena_vendaval,
    "item_chefe_barbatana_kul": r_chefe_barbatana_kul,
    "item_chefe_brasa_viva": r_chefe_brasa_viva,
    "item_chefe_escama_apep": r_chefe_escama_apep,
    "item_chefe_tabua_sonho": r_chefe_tabua_sonho,
    "item_chefe_lasca_selo": r_chefe_lasca_selo,
    "item_chefe_semente_luz": r_chefe_semente_luz,
    # amuletos / bolsas / chaves / convites
    "item_amuleto_sorte": r_amuleto_sorte,
    "item_patua_vo_zefa": r_patua,
    "item_bornal_couro": r_bornal_couro,
    "item_chave_engenho": r_chave_engenho,
    "item_convite_terreiro": r_convite_terreiro,
    # peixes
    "item_fish_bagre_cego": r_fish(body=(220, 210, 220), belly=CREAM, whiskers=True),
    "item_fish_cascudo": r_fish(body=(120, 110, 90), plates=True, long=True),
    "item_fish_curimata": r_fish(body=(110, 100, 60), long=True, scale=1.05),
    "item_fish_lambari": r_fish(body=(170, 170, 120), belly=CREAM_DIM, whiskers=True, scale=0.85),
    "item_fish_mandi": r_fish(body=(200, 170, 90), belly=CREAM, whiskers=True, scale=0.9),
    "item_fish_pacu": r_fish(body=(170, 180, 190), belly=(220, 140, 80)),
    "item_fish_peixe_dourado": r_fish(body=GOLD, belly=GOLD_LIGHT, fin=(230, 150, 40), glow_col=GOLD_LIGHT),
    "item_fish_piaba": r_fish(body=(170, 200, 210), belly=CREAM, scale=0.75),
    "item_fish_piaba_lanterna": r_fish(body=(80, 120, 160), belly=(170, 240, 255), glow_col=(150, 230, 255),
                                       scale=0.85),
    "item_fish_piau": r_fish(body=(120, 150, 130), belly=CREAM_DIM, spots=(200, 80, 70)),
    "item_fish_surubim": r_fish(body=(110, 110, 120), belly=CREAM_DIM, whiskers=True, spots=NIGHT, scale=1.1,
                                long=True),
    "item_fish_tilapia": r_fish(body=(150, 160, 150), belly=(220, 220, 210)),
    "item_fish_traira": r_fish(body=(110, 120, 80), belly=(200, 190, 140), teeth=True, long=True),
    "item_fish_tucunare": r_fish(body=(170, 170, 70), belly=(240, 150, 60), spots=NIGHT, scale=1.05),
    # asas (Tacho) e capas
    "item_asas_asa_branca_1": r_asas(BRONZE, 1),
    "item_asas_asa_branca_2": r_asas(BRONZE, 2),
    "item_asas_asa_branca_3": r_asas(BRONZE, 3),
    "item_asas_beija_flor_1": r_asas(TURQUOISE, 1),
    "item_asas_beija_flor_2": r_asas(TURQUOISE, 2),
    "item_asas_beija_flor_3": r_asas(TURQUOISE, 3),
    "item_asas_carcara_1": r_asas((170, 120, 70), 1, accent=GOLD),
    "item_asas_carcara_2": r_asas((170, 120, 70), 2, accent=GOLD),
    "item_asas_carcara_3": r_asas((170, 120, 70), 3, accent=GOLD),
    "item_asas_vaga_lume_1": r_asas((120, 150, 70), 1, accent=(230, 250, 120), glowy=True),
    "item_asas_vaga_lume_2": r_asas((120, 150, 70), 2, accent=(230, 250, 120), glowy=True),
    "item_asas_vaga_lume_3": r_asas((120, 150, 70), 3, accent=(230, 250, 120), glowy=True),
    "item_capa_vaqueiro_1": r_capa(1),
    "item_capa_vaqueiro_2": r_capa(2),
    "item_capa_vaqueiro_3": r_capa(3),
    # apitos de montaria (cor da fita = bicho)
    "item_whistle_mount_big_dog": r_whistle((230, 220, 200)),
    "item_whistle_mount_capybara": r_whistle((170, 120, 150)),
    "item_whistle_mount_giant_anteater": r_whistle((120, 100, 80)),
    "item_whistle_mount_mountain_goat": r_whistle((150, 140, 130)),
    "item_whistle_mount_pack_donkey": r_whistle((140, 140, 150)),
    "item_whistle_mount_pinto_horse": r_whistle(SKY),
    "item_whistle_mount_pony": r_whistle((170, 110, 60)),
    "item_whistle_mount_rhea": r_whistle((60, 60, 60)),
}


# Fallback por categoria (ItemCategory) para itens futuros sem receita própria; o teste EditMode
# continua exigindo que todo item tenha ícone, então basta rodar o script de novo.
CATEGORY_FALLBACK = {
    0: r_wood, 1: r_stone, 2: r_iron, 3: r_gold, 4: r_herb, 5: r_fiber,
    6: r_cooked_meat, 7: r_health_potion, 8: r_iron_sword, 9: r_hunting_bow, 10: r_runa(VIOLET),
    11: r_leather_helmet, 12: r_leather_chest, 13: r_leather_chest, 14: r_leather_boots,
    15: r_amuleto_antiveneno, 16: r_pickaxe, 17: r_hammer, 18: r_chave_engenho, 19: r_gold_coin,
    21: r_bornal_couro,
}


# ---------------------------------------------------------------- Unity glue
META_TEMPLATE = """fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
    flipGreenChannel: 0
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMipmapLimit: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 1
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 0
  alignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spritePixelsToUnits: 100
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  spriteGenerateFallbackPhysicsShape: 0
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 8
  textureShape: 1
  singleChannelComponent: 0
  flipbookRows: 1
  flipbookColumns: 1
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  ignorePngGamma: 0
  applyGammaDecoding: 0
  swizzle: 50462976
  cookieLightType: 0
  platformSettings:
  - serializedVersion: 4
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 128
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 0
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    ignorePlatformSupport: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
  spriteSheet:
    serializedVersion: 2
    sprites: []
    outline: []
    customData:
    physicsShape: []
    bones: []
    spriteID: {sprite_id}
    internalID: 0
    vertices: []
    indices:
    edges: []
    weights: []
    secondaryTextures: []
    spriteCustomMetadata:
      entries: []
    nameFileIdTable: {{}}
  mipmapLimitGroupName:
  pSDRemoveMatte: 0
  userData:
  assetBundleName:
  assetBundleVariant:
"""

FOLDER_META = """fileFormatVersion: 2
guid: {guid}
folderAsset: yes
DefaultImporter:
  externalObjects: {{}}
  userData:
  assetBundleName:
  assetBundleVariant:
"""


def stable_guid(key):
    return hashlib.md5(key.encode("utf-8")).hexdigest()


def icon_guid(item_id):
    return stable_guid("rapadura.item-icon." + item_id)


def write_if_changed(path, data, binary=False):
    mode = "rb" if binary else "r"
    if os.path.exists(path):
        with open(path, mode, **({} if binary else {"encoding": "utf-8", "newline": ""})) as fh:
            if fh.read() == data:
                return False
    with open(path, "wb" if binary else "w", **({} if binary else {"encoding": "utf-8", "newline": "\n"})) as fh:
        fh.write(data)
    return True


def ensure_folder(path):
    """Cria as pastas que faltam sob Assets/ com .meta de GUID estável."""
    rel = os.path.relpath(path, ROOT).replace("\\", "/")
    parts = rel.split("/")
    cur = ROOT
    for i, part in enumerate(parts):
        cur = os.path.join(cur, part)
        if not os.path.isdir(cur):
            os.makedirs(cur)
        if i > 0:  # não cria meta para Assets
            meta = cur + ".meta"
            if not os.path.exists(meta):
                write_if_changed(meta, FOLDER_META.format(guid=stable_guid("rapadura.folder." + "/".join(parts[:i + 1]))))


def item_script_guid():
    with open(ITEM_SCRIPT_META, encoding="utf-8") as fh:
        return re.search(r"guid:\s*([0-9a-f]{32})", fh.read()).group(1)


def find_items():
    sg = item_script_guid()
    items = []
    for dirpath, _, files in os.walk(ITEMS_DIR):
        for name in sorted(files):
            if not name.endswith(".asset"):
                continue
            path = os.path.join(dirpath, name)
            with open(path, encoding="utf-8", newline="") as fh:
                text = fh.read()
            if "guid: " + sg not in text:
                continue

            def field(k, default="0"):
                m = re.search(r"^\s*" + k + r":\s*(.*)$", text, re.M)
                return m.group(1).strip() if m else default
            items.append({
                "path": path, "text": text, "id": field("_itemId", ""),
                "category": int(field("_category")), "rarity": int(field("_rarity")),
                "icon": field("_icon", "{fileID: 0}"),
            })
    return items


def recipe_for(item):
    if item["id"] in RECIPES:
        return RECIPES[item["id"]], True
    return CATEGORY_FALLBACK.get(item["category"], r_bornal_couro), False


ICON_LINE = re.compile(r"^(\s*_icon:\s*)\{[^}]*\}\s*$", re.M)


def link_icon(item, guid):
    ref = "{fileID: %d, guid: %s, type: 3}" % (SPRITE_FILE_ID, guid)
    new_text, n = ICON_LINE.subn(lambda m: m.group(1) + ref, item["text"], count=1)
    if n != 1:
        raise RuntimeError("sem linha _icon em " + item["path"])
    return write_if_changed(item["path"], new_text)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--check", action="store_true")
    ap.add_argument("--only", nargs="*")
    ap.add_argument("--preview", help="só grava uma folha de contato PNG neste caminho (não mexe no projeto)")
    args = ap.parse_args()

    items = find_items()
    missing_recipe = [it["id"] for it in items if it["id"] not in RECIPES]
    if args.check:
        no_icon = [it["id"] for it in items if it["icon"].startswith("{fileID: 0")]
        for i in missing_recipe:
            print("sem receita própria (usa a da categoria):", i)
        for i in no_icon:
            print("sem ícone:", i)
        return 1 if no_icon else 0

    if args.preview:
        cols = 12
        sheet = Image.new("RGBA", (cols * (SIZE + 8), ((len(items) + cols - 1) // cols) * (SIZE + 8)), (60, 40, 28, 255))
        for n, it in enumerate(items):
            ic = Icon()
            recipe_for(it)[0](ic)
            img = ic.finish(it["rarity"])
            sheet.alpha_composite(img, ((n % cols) * (SIZE + 8) + 4, (n // cols) * (SIZE + 8) + 4))
        sheet.save(args.preview)
        print("folha:", args.preview)
        return 0

    ensure_folder(OUT_DIR)
    changed = 0
    for it in items:
        if args.only and it["id"] not in args.only:
            continue
        fn, _ = recipe_for(it)
        ic = Icon()
        fn(ic)
        img = ic.finish(it["rarity"])
        guid = icon_guid(it["id"])
        png = os.path.join(OUT_DIR, it["id"] + ".png")
        buf = io.BytesIO()
        img.save(buf, format="PNG", optimize=True)
        changed += write_if_changed(png, buf.getvalue(), binary=True)
        changed += write_if_changed(png + ".meta", META_TEMPLATE.format(guid=guid, sprite_id=stable_guid("rapadura.item-icon-sprite." + it["id"])))
        changed += link_icon(it, guid)
    print("%d itens, %d arquivos alterados, %d sem receita própria%s" % (
        len(items), changed, len(missing_recipe), (": " + ", ".join(missing_recipe)) if missing_recipe else ""))
    return 0


if __name__ == "__main__":
    sys.exit(main())
