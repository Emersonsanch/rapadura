# Balanço das faixas 100–500 (mundo aberto)

> Medido em 2026-11-11 com `python -I tools/balance/level_500.py` (também impresso no fim de
> `python -I tools/balance/power_stack.py`) e travado em `tools/balance/test_level_500.py`. Nada no jogo foi mudado:
> este documento traz a medição, as faixas fora da meta e as sugestões com números. Metas: `Docs/Progressao500.md` §4.1.

## 1. O que foi medido

- **Herói de referência no nível L** (100, 120 … 460, 478 e 500): curva de vida/dano (`progression.csv`), 2 pontos de
  atributo por nível (Vitalidade + Poder Arcano), pontos de skill, roda de talentos e treino, skills das graduações
  liberadas até L e o equipamento que dá para ter: o melhor saque da campanha mais um farm longo (Monte Carlo, 120
  corridas) e, na cabeça e no peito, o melhor conjunto **já obtível em L**: Marfim do Ossuário (`Items/Dungeon`, desde o
  nível 35), Cais do Papiro (`Items/Sets`, desde a entrada do Rio, 260), Tijolo Azul (Mistura, 300), Selo dos
  Artífices (Limiar Profundo, 340) e Cais de Luz (Céu, 420).
- **Duas leituras do herói.** "real" = o que o código faz hoje (a armadura base das peças **não** chega ao herói, os
  afixos não crescem com o nível). "doc" = o que o §4.2 do Progressao500 supõe (afixos planos por faixa e armadura das
  peças contada).
- **Bichos comuns**: toda espécie de `Resources/Enemies` que algum layout de região (`OverworldLayouts.*.cs`) põe numa
  faixa que alcança o nível, × `EnemyLevelScaling` (vida × Rd(L)/Rd(15), dano × Rh(L)/Rh(15)). Luta de 2 comuns, em três
  leituras: **caça** (só skills com recarga ≤ 12 s, o farm contínuo; é a que vale para a meta), **abertura** (kit
  inteiro pronto, a suprema incluída: a primeira luta depois de descansar) e **só básico**. Tempos × fator jogo/modelo do
  r7 (comuns ×1,73, chefes ×0,89).
- **Chefes do mundo**: os sete de `RegionBossRules` no nível deles, com as habilidades reais (anel, rajada, golpe grande,
  noite, Beber o Rio, Queda do Sonho, Cobiça da Arca, as cinco formas da Grande Treva), × `EnemyLevelScaling`. Tempo para
  matar, tempo para o herói parado morrer, só no básico, golpes do chefe para matar o herói (pior crítico), maior golpe
  em % da vida e golpes básicos do herói no chefe. Os chefes novos das Regiões 5–9 que já estão em `RegionBossRules`
  entram sozinhos quando o asset existir (o golpe-assinatura deles é lido do C#); hoje os dois da faixa, os Quatro
  Gulosos (140) e Ohya (150), ainda não têm asset em `Resources/Enemies` e ficam fora da tabela. Hedammu, Lodo-que-Engole
  e Aydahar (72–98) estão abaixo de 100.
- **"Perigoso"** (`LevelDangerRules`): o bicho no nível da linha e o herói 10, 20 e 30 níveis abaixo (crítico +25% ×1,5
  a partir de +10; de +20 o herói erra 40% e o dano vai de ×1,5 a ×3 em +30).

## 2. Resumo: faixas fora da meta (leitura "real")

| Faixa | Meta | Hoje | Diagnóstico |
|---|---|---|---|
| Comuns 100–160 | 4–7 s | 3,1–3,3 s | rápido demais |
| Comuns 180–240 | 4–7 s | 4,0–4,9 s | dentro (perto do limite de baixo) |
| Comuns 260–500 | 4–7 s | 2,0–3,5 s (as espécies mais frágeis caem em 0,2 s) | rápido demais |
| Os 7 chefes do mundo: tempo para matar | 15–22 s | 15,9–18,6 s | **dentro** |
| Chefes 200–280: herói parado morre em | 12–40 s | 10,6–11,1 s | curto |
| Chefes 320–478: herói parado morre em | 12–40 s | 4,6–7,9 s | muito curto |
| **Āz, a Capitã Cobiçosa (478)** | ≥ 3 golpes, maior golpe < 100% | **3 golpes** matam o herói (maior golpe 60% da vida), parado em **4,6 s** | o pior caso: mata em poucos golpes |
| A Grande Treva (440) / O Sonho da Destruição (360) | idem | 4 golpes, maior golpe 59% / 43% | perto de "poucos golpes" |
| Chefes: golpes do herói para matar | ≥ 8 básicos | 61–74 | nenhum chefe morre em poucos golpes |
| Só no ataque básico | ≥ 75% do tempo parado | 28,8–34,6 s | dentro (o chefe vence quem não usa skills) |

Leituras:

1. **Os chefes morrem no tempo certo, mas matam rápido demais, e cada vez mais rápido com o nível.** A escala de dano usa
   Rh(L) = (1 + 0,08·s)(1 + 0,005·s); o segundo fator é a "defesa por faixa" que o herói real não ganha (a armadura das
   peças não é aplicada e os afixos planos não crescem). Do 100 ao 400 a vida do herói real cresce ×3,9 (1 905 → 7 369),
   o dano do bicho ×7,4. No 478 isso deixa o Āz com 3 golpes para matar.
2. **Os comuns morrem rápido demais por dois motivos.** (a) Muitos bichos das regiões altas usam bases de vida de
   floresta (70–108) e a escala multiplica como se fossem o arquétipo de 170: no 400 a vida base média é 108, ~64% do
   que a tabela do §4.1 supõe. (b) As skills de área com recarga curta (Disparo Explosivo, Chuva de Flechas, Orbe,
   Granada…) crescem com nível de skill e graduação mais do que Rd. Só no ataque básico os comuns ficam em 6,1–8,6 s,
   então a escala de vida está certa para o básico.
3. **A leitura "doc" pende ainda mais para o herói**: se os afixos por faixa e a armadura das peças entrarem como o §4.2
   pede, chefes morrem em 7,3–9 s e comuns em 0,3–2 s. Essa mudança no herói só pode entrar junto de vida ×2,1–2,5 nos
   chefes (tabela da §4.5).
4. **"Perigoso" funciona, mais forte nos chefes do que nos comuns.** +10 já tira metade dos golpes para morrer (chefes
   440+: 2 golpes); +20 dobra o tempo para matar (o herói erra 40%: 30–35 s, fora da meta, de propósito); +30 é "quase
   hit-kill" nos chefes (1–2 golpes, 1,3–2,8 s parado). Nos comuns, +30 ainda pede 4–5 golpes no pior crítico, e o tempo
   para matar só vai de 2–4 s para 5–11 s.

## 3. Sugestões de ajuste (não aplicadas)

Em ordem de prioridade. As duas primeiras são independentes (uma mexe só no dano dos bichos, a outra só na vida dos
comuns) e juntas põem todas as faixas da leitura "real" na meta.

1. **`EnemyLevelScaling.HeroToughnessGrowthPerLevel` 0,005 → 0** (Rh passa a ser só a curva de vida, que é o que o herói
   real ganha). Medido (§4.5, última tabela): o herói parado morre em 14,5–24 s nos sete chefes (Āz: 4,6 → 14,5 s e
   3 → 9 golpes); o orçamento 15–100 do `level_scaling.py` continua dentro; só no básico o chefe continua vencendo.
   0,0025 não basta (chefes 320+ ficam em 6,9–11,1 s). Efeito colateral: os comuns passam a pedir 45–53 golpes para
   matar o herói (hoje 15–32) e o "Perigoso +30" dos comuns sobe de 4–5 para ~7–13 golpes; se ficar manso demais no
   teste, compense no dano dos comuns (não dos chefes) ou em `LevelDangerRules.LethalDamageTaken` 3 → 4.
   Alternativa sem mexer na escala: dano de cada chefe × 0,74 (Rei-Vendaval 200) / 0,71 (Velho Kul 240) / 0,72 (Glutão
   de Brasa 280) / 0,53 (Apep 320) / 0,46 (Sonho da Destruição 360) / 0,39 (Grande Treva 440) / 0,31 (Āz 478), que dá
   15 s parado e 8–11 golpes para morrer.
2. **Piso de vida base 170 nos comuns do mundo aberto acima do nível 100** (o comum-arquétipo da escala; hoje há
   espécies de 70–108 nas regiões de 260 a 500). Medido: caça de 2 comuns em 5,5–6,6 s em 100/200/300/400/500 (só no
   básico 9,2–10,2 s). Pode ir nos assets (`_maxHealth`) ou no `EnemyLevelScaler` (`max(base, 170)` antes de
   multiplicar, só quando o nível passa de 100). Se preferir um fator único por faixa: vida dos comuns × 1,27 (100),
   × 1,15 (200), × 1,47 (300), × 1,45 (400), × 1,44 (500).
3. **Quando (e se) os afixos por faixa e a armadura das peças chegarem ao herói** (leitura "doc"): vida dos chefes × 2,1
   (200–280) até × 2,5 (Āz) e rerodar esta tabela; os comuns na leitura "doc" caem para 0,3–2 s, então o piso da
   sugestão 2 teria de subir junto.
4. **Supremas na abertura**: com o kit inteiro pronto, a primeira luta de comuns acaba em 0,5–2,3 s (as supremas da
   Maithe, Lavine e Ícaro matam o par de uma vez). Não é meta (a suprema tem 60–75 s de recarga), mas se a redução de
   recarga deixar "suprema a cada pacote", olhe a recarga mínima.

Depois de mudar o jogo: rodar `python -I tools/balance/level_500.py`, atualizar as tabelas da §4 e o
`DOCUMENTED_MISSES` de `tools/balance/test_level_500.py`.

## 4. Tabelas (saída do `level_500.py`)

Fator jogo/modelo (r7): comuns ×1.73, chefes ×0.89. "Caça" = só skills com recarga ≤ 12 s; "abertura" = kit inteiro pronto, a suprema incluída.

### 4.1 Herói de referência × comuns do mundo

| Nível | Conjunto obtível | Espécies (vida base média, mín–máx) | ×vida / ×dano | 2 comuns, caça: real (mais rápido–mais lento) | Caça: doc | Abertura (kit todo) | Só básico | Golpes para morrer: real / doc |
|---|---|---|---|---|---|---|---|---|
| 100 | Marfim (Ossuário) | 27 (130, 70–210) | 4.1 / 5.9 | 3.1 s **(fora)** (0.0–8.4) | 1.9 s **(fora)** | 1.2 s | 6.7 s | 32 / 41 |
| 120 | Marfim (Ossuário) | 27 (130, 70–210) | 5.1 / 7.4 | 3.2 s **(fora)** (0.2–8.5) | 1.9 s **(fora)** | 1.3 s | 7.0 s | 30 / 39 |
| 140 | Marfim (Ossuário) | 40 (129, 70–210) | 6.2 / 9.1 | 3.3 s **(fora)** (0.2–8.9) | 1.8 s **(fora)** | 1.3 s | 6.9 s | 30 / 38 |
| 160 | Marfim (Ossuário) | 21 (127, 70–175) | 7.4 / 10.9 | 3.2 s **(fora)** (0.2–6.3) | 1.7 s **(fora)** | 1.2 s | 6.8 s | 29 / 37 |
| 180 | Marfim (Ossuário) | 36 (139, 70–210) | 8.8 / 12.8 | 4.0 s (0.2–9.2) | 2.0 s **(fora)** | 1.5 s | 7.8 s | 26 / 33 |
| 200 | Marfim (Ossuário) | 36 (139, 70–210) | 10.2 / 14.9 | 4.0 s (0.2–9.2) | 1.9 s **(fora)** | 1.5 s | 7.8 s | 25 / 32 |
| 220 | Marfim (Ossuário) | 38 (151, 95–210) | 11.8 / 17.1 | 4.9 s (1.2–9.4) | 2.1 s **(fora)** | 1.9 s | 8.5 s | 21 / 27 |
| 240 | Marfim (Ossuário) | 38 (151, 95–210) | 13.4 / 19.5 | 4.9 s (1.2–9.4) | 2.1 s **(fora)** | 1.9 s | 8.6 s | 20 / 25 |
| 260 | Cais do Papiro | 43 (127, 72–210) | 15.2 / 22.0 | 3.5 s **(fora)** (0.2–9.7) | 1.3 s **(fora)** | 1.4 s | 7.4 s | 23 / 34 |
| 280 | Cais do Papiro | 43 (127, 72–210) | 17.1 / 24.6 | 3.5 s **(fora)** (0.2–9.9) | 1.2 s **(fora)** | 1.4 s | 7.5 s | 22 / 33 |
| 300 | Tijolo Azul | 46 (108, 70–210) | 19.1 / 27.4 | 2.0 s **(fora)** (0.2–9.2) | 0.6 s **(fora)** | 0.8 s | 6.1 s | 23 / 36 |
| 320 | Tijolo Azul | 46 (108, 70–210) | 21.2 / 30.3 | 2.0 s **(fora)** (0.2–9.4) | 0.6 s **(fora)** | 0.8 s | 6.1 s | 22 / 34 |
| 340 | Selo dos Artífices | 47 (107, 70–180) | 23.4 / 33.4 | 2.0 s **(fora)** (0.2–7.2) | 0.5 s **(fora)** | 0.8 s | 6.3 s | 19 / 31 |
| 360 | Selo dos Artífices | 47 (107, 70–180) | 25.7 / 36.6 | 2.1 s **(fora)** (0.2–7.2) | 0.5 s **(fora)** | 0.5 s | 6.3 s | 19 / 30 |
| 380 | Selo dos Artífices | 25 (108, 75–180) | 28.2 / 40.0 | 2.0 s **(fora)** (0.2–7.2) | 0.5 s **(fora)** | 0.5 s | 6.3 s | 18 / 29 |
| 400 | Selo dos Artífices | 25 (108, 75–180) | 30.7 / 43.5 | 2.1 s **(fora)** (0.2–7.2) | 0.4 s **(fora)** | 0.5 s | 6.3 s | 18 / 28 |
| 420 | Cais de Luz | 47 (107, 72–210) | 33.4 / 47.1 | 2.0 s **(fora)** (0.2–9.8) | 0.4 s **(fora)** | 0.5 s | 6.3 s | 17 / 29 |
| 440 | Cais de Luz | 47 (107, 72–210) | 36.1 / 50.9 | 2.0 s **(fora)** (0.2–9.9) | 0.4 s **(fora)** | 0.5 s | 6.3 s | 17 / 28 |
| 460 | Cais de Luz | 27 (108, 72–210) | 39.0 / 54.8 | 2.1 s **(fora)** (0.2–9.9) | 0.4 s **(fora)** | 0.5 s | 6.3 s | 16 / 27 |
| 478 | Cais de Luz | 27 (108, 72–210) | 41.7 / 58.4 | 2.1 s **(fora)** (0.2–9.9) | 0.4 s **(fora)** | 0.5 s | 6.3 s | 16 / 26 |
| 500 | Cais de Luz | 27 (108, 72–210) | 45.1 / 63.0 | 2.1 s **(fora)** (0.2–9.9) | 0.3 s **(fora)** | 0.5 s | 6.3 s | 15 / 26 |

### 4.2 Chefes do mundo (herói no nível do chefe)

| Chefe (nível) | Vida / dano | Mata: real | Mata: doc | Parado morre em: real / doc | Só básico mata em (real) | Golpes do chefe para morrer: real / doc | Maior golpe (% da vida): real / doc | Dano da luta (× vida): real / doc | Básicos do herói no chefe (real) |
|---|---|---|---|---|---|---|---|---|---|
| "O Rei-Vendaval" (200) | 10,613 / 506 | 15.9 s | 8.9 s **(fora)** | 11.1 s **(fora)** / 14.2 s | 28.8 s | 8 / 10 | 27% / 21% | 1.9 / 0.8 | 61 |
| "O Velho Kul" (240) | 14,771 / 740 | 17.0 s | 8.9 s **(fora)** | 10.6 s **(fora)** / 13.6 s | 30.7 s | 7 / 8 | 33% / 25% | 2.2 / 0.9 | 66 |
| "O Glutão de Brasa" (280) | 19,825 / 1,034 | 18.4 s | 9.0 s **(fora)** | 10.7 s **(fora)** / 16.4 s | 33.6 s | 6 / 9 | 30% / 19% | 2.3 / 0.7 | 72 |
| "Apep, a Serpente que Bebe o Rio" (320) | 24,688 / 1,396 | 18.3 s | 8.6 s **(fora)** | 7.9 s **(fora)** / 12.5 s | 32.2 s | 5 / 8 | 35% / 22% | 3.0 / 0.9 | 72 |
| "O Sonho da Destruição" (360) | 30,105 / 1,831 | 18.3 s | 8.2 s **(fora)** | 6.9 s **(fora)** / 11.3 s **(fora)** | 34.1 s | 4 / 7 | 43% / 26% | 3.5 / 1.0 | 72 |
| "A Grande Treva" (440) | 42,447 / 2,747 | 18.3 s | 7.6 s **(fora)** | 5.8 s **(fora)** / 10.0 s **(fora)** | 34.6 s | 4 / 6 | 59% / 34% | 4.3 / 1.1 | 73 |
| "Āz, a Capitã Cobiçosa" (478) | 49,179 / 3,389 | 18.6 s | 7.3 s **(fora)** | 4.6 s **(fora)** / 7.8 s **(fora)** | 34.6 s | 3 / 5 | 60% / 35% | 5.4 / 1.2 | 74 |

Ainda sem asset em `Resources/Enemies` (fora da tabela): `enemy_mountain_four_gluttons` (nível 140)

Ainda sem asset em `Resources/Enemies` (fora da tabela): `enemy_mountain_ohya` (nível 150)

### 4.3 Regra "Perigoso"

Comuns (2 por luta): tempo para matar (caça) · golpes para morrer no pior crítico. O bicho fica no nível da linha; o herói está delta níveis abaixo.

| Bicho | +0 | +10 | +20 | +30 |
|---|---|---|---|---|
| comuns 100 | 3.1 s · 32 | 4.1 s · 20 | 8.9 s · 12 | 11.3 s · 5 |
| comuns 200 | 4.0 s · 25 | 4.7 s · 16 | 9.3 s · 10 | 10.7 s · 5 |
| comuns 300 | 2.0 s · 23 | 2.7 s · 15 | 5.2 s · 10 | 6.1 s · 5 |
| comuns 400 | 2.1 s · 18 | 2.5 s · 12 | 4.8 s · 8 | 5.4 s · 4 |
| comuns 478 | 2.1 s · 16 | 2.4 s · 10 | 4.6 s · 7 | 5.1 s · 4 |

Chefes do mundo: tempo para matar · golpes do chefe para morrer (pior crítico) · herói morre parado em.

| Bicho | +0 | +10 | +20 | +30 |
|---|---|---|---|---|
| "O Rei-Vendaval" (200) | 15.9 s · 8 · 11.1 s | 17.0 s · 5 · 9.4 s | 30.7 s · 4 · 5.9 s | 33.8 s · 2 · 2.8 s |
| "O Velho Kul" (240) | 17.0 s · 7 · 10.6 s | 18.2 s · 4 · 9.0 s | 32.6 s · 3 · 5.8 s | 36.0 s · 2 · 2.8 s |
| "O Glutão de Brasa" (280) | 18.4 s · 6 · 10.7 s | 20.0 s · 4 · 9.2 s | 35.4 s · 3 · 5.9 s | 37.2 s · 2 · 2.7 s |
| "Apep, a Serpente que Bebe o Rio" (320) | 18.3 s · 5 · 7.9 s | 19.5 s · 3 · 6.8 s | 34.2 s · 2 · 4.4 s | 37.7 s · 1 · 2.1 s |
| "O Sonho da Destruição" (360) | 18.3 s · 4 · 6.9 s | 19.2 s · 3 · 6.0 s | 34.3 s · 2 · 3.9 s | 36.1 s · 1 · 2.0 s |
| "A Grande Treva" (440) | 18.3 s · 4 · 5.8 s | 19.3 s · 2 · 5.1 s | 33.9 s · 2 · 3.3 s | 35.7 s · 1 · 1.6 s |
| "Āz, a Capitã Cobiçosa" (478) | 18.6 s · 3 · 4.6 s | 19.3 s · 2 · 4.0 s | 33.9 s · 2 · 2.6 s | 35.5 s · 1 · 1.3 s |

### 4.4 Fora da meta (leitura "real")

- common 100: fast
- common 120: fast
- common 140: fast
- common 160: fast
- common 260: fast
- common 280: fast
- common 300: fast
- common 320: fast
- common 340: fast
- common 360: fast
- common 380: fast
- common 400: fast
- common 420: fast
- common 440: fast
- common 460: fast
- common 478: fast
- common 500: fast
- boss 200: parked_short
- boss 240: parked_short
- boss 280: parked_short
- boss 320: parked_short
- boss 360: parked_short
- boss 440: parked_short
- boss 478: parked_short

### 4.5 Ajustes que levariam à meta (sugestão, não aplicada)

| Nível | 2 comuns, caça: hoje | ×vida dos comuns para 5,5 s | Depois: caça / abertura / só básico | Golpes para morrer |
|---|---|---|---|---|
| 100 | 3.1 s | ×1.27 | 5.5 s / 2.3 s / 8.5 s | 32 |
| 200 | 4.0 s | ×1.15 | 5.5 s / 2.3 s / 8.8 s | 25 |
| 300 | 2.0 s | ×1.47 | 5.5 s / 2.3 s / 9.0 s | 23 |
| 400 | 2.1 s | ×1.45 | 5.6 s / 1.4 s / 9.3 s | 18 |
| 500 | 2.1 s | ×1.44 | 5.5 s / 1.4 s / 9.1 s | 15 |

| Chefe (nível) | ×dano para o herói parado durar 15 s | Golpes para morrer depois | Maior golpe depois | ×vida se o herói "doc" for implementado |
|---|---|---|---|---|
| "O Rei-Vendaval" (200) | ×0.74 | 11 | 20% | ×2.09 |
| "O Velho Kul" (240) | ×0.71 | 9 | 23% | ×2.07 |
| "O Glutão de Brasa" (280) | ×0.72 | 8 | 21% | ×2.05 |
| "Apep, a Serpente que Bebe o Rio" (320) | ×0.53 | 9 | 18% | ×2.14 |
| "O Sonho da Destruição" (360) | ×0.46 | 9 | 20% | ×2.27 |
| "A Grande Treva" (440) | ×0.39 | 8 | 23% | ×2.42 |
| "Āz, a Capitã Cobiçosa" (478) | ×0.31 | 9 | 18% | ×2.54 |

Piso de vida base 170 (o comum-arquétipo da escala) nos comuns do mundo aberto:

| Nível | Caça (mais rápido–mais lento) | Abertura | Só básico | Golpes para morrer |
|---|---|---|---|---|
| 100 | 5.5 s (5.4–8.4) | 2.2 s | 9.2 s | 32 |
| 200 | 6.1 s (5.9–9.2) | 2.5 s | 9.4 s | 25 |
| 300 | 6.4 s (6.3–9.2) | 2.7 s | 9.7 s | 23 |
| 400 | 6.3 s (6.3–7.2) | 1.2 s | 10.1 s | 18 |
| 500 | 6.6 s (6.4–9.9) | 1.3 s | 10.2 s | 15 |

`EnemyLevelScaling.HeroToughnessGrowthPerLevel` (só mexe no dano dos bichos): herói parado morre em · golpes do chefe para morrer (pior crítico):

| Valor | chefe 200 | chefe 240 | chefe 280 | chefe 320 | chefe 360 | chefe 440 | chefe 478 | Golpes de comum p/ morrer (100/200/300/400/500) | Orçamento 15–100 (level_scaling) |
|---|---|---|---|---|---|---|---|---|---|
| 0.005 | 11.1 s **(fora)** · 8 | 10.6 s **(fora)** · 7 | 10.7 s **(fora)** · 6 | 7.9 s **(fora)** · 5 | 6.9 s **(fora)** · 4 | 5.8 s **(fora)** · 4 | 4.6 s **(fora)** · 3 | 32/25/23/18/15 | dentro |
| 0.0025 | 14.3 s · 10 | 14.1 s · 9 | 14.7 s · 8 | 11.1 s **(fora)** · 7 | 9.8 s **(fora)** · 6 | 8.6 s **(fora)** · 5 | 6.9 s **(fora)** · 4 | 37/32/31/25/23 | dentro |
| 0 | 20.7 s · 15 | 21.7 s · 13 | 24.0 s · 13 | 19.2 s · 12 | 18.0 s · 10 | 17.4 s · 9 | 14.5 s · 9 | 45/46/53/49/49 | dentro |
