# Balanço das faixas 100–500 (mundo aberto)

> Medido em 2026-11-11 com `python -I tools/balance/level_500.py` (também impresso no fim de
> `python -I tools/balance/power_stack.py`) e travado em `tools/balance/test_level_500.py`. Metas:
> `Docs/Progressao500.md` §4.1. **Sugestões 1 e 2 aplicadas no jogo em 2026-11-11** (§2.1 traz o antes/depois; as
> tabelas da §4 já são o "depois").

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
- **Chefes do mundo**: todos os de `RegionBossRules` com asset (os doze, de Hedammu 72 ao Āz 478) no nível deles, com as habilidades reais (anel, rajada, golpe grande,
  noite, Beber o Rio, Queda do Sonho, Cobiça da Arca, as cinco formas da Grande Treva), × `EnemyLevelScaling`. Tempo para
  matar, tempo para o herói parado morrer, só no básico, golpes do chefe para matar o herói (pior crítico), maior golpe
  em % da vida e golpes básicos do herói no chefe. Os chefes das Regiões 5–9 (Hedammu 72, Lodo-que-Engole 88, Aydahar 98,
  Quatro Gulosos 140, Ohya 150) entram com o golpe-assinatura lido do C#; os três abaixo de 100 usam a mesma regra.
- **"Perigoso"** (`LevelDangerRules`): o bicho no nível da linha e o herói 10, 20 e 30 níveis abaixo (crítico +25% ×1,5
  a partir de +10; de +20 o herói erra 40% e o dano vai de ×1,5 a ×3 em +30).

## 2. Resumo: faixas fora da meta (leitura "real")

### 2.1 Antes → depois das sugestões 1 e 2

| Faixa | Meta | Antes (0,005 e sem piso) | Depois (0 e piso 170 do 100 em diante) |
|---|---|---|---|
| Comuns 100–160: caça de 2 | 4–7 s | 3,1–3,3 s (fora) | 5,5–5,9 s |
| Comuns 180–240: caça de 2 | 4–7 s | 4,0–4,9 s | 6,1–6,6 s |
| Comuns 260–500: caça de 2 | 4–7 s | 2,0–3,5 s (fora) | 6,3–6,7 s |
| Comuns 100–500: só básico | — | 6,1–8,6 s | 9,1–10,3 s |
| Comuns: golpes para o herói morrer | ≥ 3 | 15–32 | 40–53 |
| Chefes 200–478: tempo para matar | 15–22 s | 15,9–18,6 s | 15,9–18,6 s (a vida não mudou) |
| Chefes 200–280: herói parado morre em | 12–40 s | 10,6–11,1 s (fora) | 20,7–24,0 s |
| Chefes 320–478: herói parado morre em | 12–40 s | 4,6–7,9 s (fora) | 14,5–19,2 s |
| Āz (478): golpes / maior golpe | ≥ 3 / < 100% | 3 / 60% | 9 / 19% |
| Chefes 140–150 (Quatro Gulosos, Ohya) | 15–22 s · 12–40 s | sem asset | 15,3 s · 27,4–27,7 s parado |
| Chefes 72–98 (Hedammu, Lodo, Aydahar) | 15–22 s · 12–40 s | fora da tabela | 16,2–17,0 s (vida base ×1,2; Aydahar ×1,18) · 26,7–30,1 s parado |
| Perigoso +30, comuns: golpes para morrer | — | 4–5 | 8–11 |
| Perigoso +30, chefes: golpes / parado | — | 1–2 / 1,3–2,8 s | 2–3 / 4,0–6,6 s |
| Orçamento 15–100 (`level_scaling.py`) | dentro | dentro | dentro |

O que mudou no jogo: `EnemyLevelScaling.HeroToughnessGrowthPerLevel` 0,005 → 0 (×dano do bicho no 25/50/100/500:
1,44/2,70/5,88/63,1 → 1,38/2,32/4,21/19,3; Rh(L)/Rh(15) = (1 + 0,08·(L−1))/2,12) e `EnemyLevelScaling.CommonHealthFloor`
= 170 a partir de `CommonHealthFloorFromLevel` = 100: `HealthFor(base, nível, isBoss)` usa `max(base, 170)` nos comuns
(não nos chefes) antes do multiplicador, aplicado pelo `EnemyLevelScaler`; nenhum asset foi editado. O piso começa no
100 (não no 101) porque com ele a faixa 100 também entra na meta (3,1 → 5,5 s) e o orçamento 15–100 continua dentro.

Hedammu, Lodo-que-Engole e Aydahar (72–98) morriam em 13,1–14,2 s (meta 15–22). **Aplicado em 2026-11-11:** vida
base no asset × 1,2 (Hedammu 960 → 1152, Lodo-que-Engole 980 → 1176) e × 1,18 no Aydahar (1000 → 1180, o teto do
`RegionBossTests`: nenhum chefe do mundo acima de Tages, `enemy_cave_dweller`, 1180; a escala por nível faz o resto).
Agora morrem em 16,2 / 16,8 / 17,0 s; nada fica fora da meta.

### 2.2 Antes das sugestões (medição original)

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

## 3. Sugestões de ajuste (1 e 2 aplicadas em 2026-11-11)

Em ordem de prioridade. As duas primeiras são independentes (uma mexe só no dano dos bichos, a outra só na vida dos
comuns) e juntas põem todas as faixas da leitura "real" na meta.

1. **[APLICADA] `EnemyLevelScaling.HeroToughnessGrowthPerLevel` 0,005 → 0** (Rh passa a ser só a curva de vida, que é o que o herói
   real ganha). Medido (§4.5, última tabela): o herói parado morre em 14,5–24 s nos sete chefes (Āz: 4,6 → 14,5 s e
   3 → 9 golpes); o orçamento 15–100 do `level_scaling.py` continua dentro; só no básico o chefe continua vencendo.
   0,0025 não basta (chefes 320+ ficam em 6,9–11,1 s). Efeito colateral: os comuns passam a pedir 45–53 golpes para
   matar o herói (hoje 15–32) e o "Perigoso +30" dos comuns sobe de 4–5 para ~7–13 golpes; se ficar manso demais no
   teste, compense no dano dos comuns (não dos chefes) ou em `LevelDangerRules.LethalDamageTaken` 3 → 4.
   Alternativa sem mexer na escala: dano de cada chefe × 0,74 (Rei-Vendaval 200) / 0,71 (Velho Kul 240) / 0,72 (Glutão
   de Brasa 280) / 0,53 (Apep 320) / 0,46 (Sonho da Destruição 360) / 0,39 (Grande Treva 440) / 0,31 (Āz 478), que dá
   15 s parado e 8–11 golpes para morrer.
2. **[APLICADA, do nível 100 em diante, no `EnemyLevelScaling.HealthFor`] Piso de vida base 170 nos comuns do mundo aberto acima do nível 100** (o comum-arquétipo da escala; hoje há
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
`DOCUMENTED_MISSES` de `tools/balance/test_level_500.py` (hoje: vazio).

## 4. Tabelas (saída do `level_500.py`, depois das sugestões 1 e 2)

Fator jogo/modelo (r7): comuns ×1.73, chefes ×0.89. "Caça" = só skills com recarga ≤ 12 s; "abertura" = kit inteiro pronto, a suprema incluída.

### 4.1 Herói de referência × comuns do mundo

| Nível | Conjunto obtível | Espécies (vida base média, mín–máx) | ×vida / ×dano | 2 comuns, caça: real (mais rápido–mais lento) | Caça: doc | Abertura (kit todo) | Só básico | Golpes para morrer: real / doc |
|---|---|---|---|---|---|---|---|---|
| 100 | Marfim (Ossuário) | 27 (130, 70–210) | 4.1 / 4.2 | 5.5 s (5.4–8.5) | 3.5 s **(fora)** | 2.2 s | 9.2 s | 44 / 57 |
| 120 | Marfim (Ossuário) | 27 (130, 70–210) | 5.1 / 5.0 | 5.8 s (5.6–8.5) | 3.5 s **(fora)** | 2.4 s | 9.2 s | 45 / 57 |
| 140 | Marfim (Ossuário) | 40 (129, 70–210) | 6.2 / 5.7 | 5.8 s (5.8–8.9) | 3.4 s **(fora)** | 2.4 s | 9.2 s | 47 / 60 |
| 160 | Marfim (Ossuário) | 21 (127, 70–175) | 7.4 / 6.5 | 5.9 s (5.8–6.3) | 3.2 s **(fora)** | 2.4 s | 9.1 s | 49 / 62 |
| 180 | Marfim (Ossuário) | 36 (139, 70–210) | 8.8 / 7.2 | 6.1 s (5.9–9.2) | 3.3 s **(fora)** | 2.5 s | 9.4 s | 46 / 58 |
| 200 | Marfim (Ossuário) | 36 (139, 70–210) | 10.2 / 8.0 | 6.1 s (5.9–9.2) | 3.3 s **(fora)** | 2.5 s | 9.4 s | 46 / 58 |
| 220 | Marfim (Ossuário) | 38 (151, 95–210) | 11.8 / 8.7 | 6.6 s (6.3–9.4) | 3.0 s **(fora)** | 2.8 s | 9.6 s | 40 / 52 |
| 240 | Marfim (Ossuário) | 38 (151, 95–210) | 13.4 / 9.5 | 6.6 s (6.3–9.4) | 3.0 s **(fora)** | 2.8 s | 10.3 s | 40 / 52 |
| 260 | Cais do Papiro | 43 (127, 72–210) | 15.2 / 10.2 | 6.7 s (6.4–9.7) | 3.0 s **(fora)** | 2.8 s | 10.2 s | 48 / 74 |
| 280 | Cais do Papiro | 43 (127, 72–210) | 17.1 / 11.0 | 6.7 s (6.4–9.9) | 2.9 s **(fora)** | 2.8 s | 10.3 s | 48 / 74 |
| 300 | Tijolo Azul | 46 (108, 70–210) | 19.1 / 11.8 | 6.4 s (6.3–9.4) | 2.8 s **(fora)** | 2.7 s | 10.1 s | 53 / 83 |
| 320 | Tijolo Azul | 46 (108, 70–210) | 21.2 / 12.5 | 6.4 s (6.3–9.5) | 2.8 s **(fora)** | 2.7 s | 10.1 s | 53 / 83 |
| 340 | Selo dos Artífices | 47 (107, 70–180) | 23.4 / 13.3 | 6.3 s (6.3–7.2) | 2.2 s **(fora)** | 2.7 s | 10.1 s | 48 / 78 |
| 360 | Selo dos Artífices | 47 (107, 70–180) | 25.7 / 14.0 | 6.3 s (6.3–7.2) | 2.2 s **(fora)** | 1.2 s | 10.1 s | 48 / 79 |
| 380 | Selo dos Artífices | 25 (108, 75–180) | 28.2 / 14.8 | 6.3 s (6.3–7.2) | 2.0 s **(fora)** | 1.2 s | 10.1 s | 48 / 79 |
| 400 | Selo dos Artífices | 25 (108, 75–180) | 30.7 / 15.5 | 6.5 s (6.4–7.2) | 2.0 s **(fora)** | 1.2 s | 10.1 s | 48 / 79 |
| 420 | Cais de Luz | 47 (107, 72–210) | 33.4 / 16.3 | 6.4 s (6.3–9.9) | 2.0 s **(fora)** | 1.2 s | 10.2 s | 48 / 83 |
| 440 | Cais de Luz | 47 (107, 72–210) | 36.1 / 17.0 | 6.4 s (6.3–9.9) | 2.0 s **(fora)** | 1.2 s | 10.2 s | 48 / 83 |
| 460 | Cais de Luz | 27 (108, 72–210) | 39.0 / 17.8 | 6.6 s (6.4–9.9) | 1.7 s **(fora)** | 1.3 s | 10.2 s | 48 / 82 |
| 478 | Cais de Luz | 27 (108, 72–210) | 41.7 / 18.5 | 6.6 s (6.4–9.9) | 1.7 s **(fora)** | 1.3 s | 10.2 s | 48 / 82 |
| 500 | Cais de Luz | 27 (108, 72–210) | 45.1 / 19.3 | 6.6 s (6.4–9.9) | 1.7 s **(fora)** | 1.3 s | 10.2 s | 48 / 82 |

### 4.2 Chefes do mundo (herói no nível do chefe)

| Chefe (nível) | Vida / dano | Mata: real | Mata: doc | Parado morre em: real / doc | Só básico mata em (real) | Golpes do chefe para morrer: real / doc | Maior golpe (% da vida): real / doc | Dano da luta (× vida): real / doc | Básicos do herói no chefe (real) |
|---|---|---|---|---|---|---|---|---|---|
| "Hedammu, a Serpente Gulosa do Mar" (72) | 3,267 / 82 | 16.2 s | 12.5 s **(fora)** | 30.1 s / 38.5 s | 28.3 s | 18 / 23 | 9% / 7% | 0.7 / 0.4 | 60 |
| "O Lodo-que-Engole" (88) | 4,129 / 105 | 16.8 s | 12.4 s **(fora)** | 28.2 s / 36.1 s | 29.3 s | 17 / 21 | 12% / 10% | 0.8 / 0.5 | 63 |
| "Aydahar, o Dragão do Passo" (98) | 4,684 / 124 | 17.0 s | 12.2 s **(fora)** | 26.7 s / 34.1 s | 30.3 s | 16 / 20 | 10% / 8% | 0.8 / 0.5 | 64 |
| "Os Quatro Gulosos" (140) | 6,315 / 183 | 15.3 s | 9.5 s **(fora)** | 27.4 s / 35.1 s | 26.9 s | 15 / 19 | 11% / 9% | 0.7 / 0.4 | 58 |
| "Ohya, o Gigante Irmão" (150) | 6,995 / 207 | 15.3 s | 9.4 s **(fora)** | 27.7 s / 35.5 s | 27.9 s | 14 / 18 | 12% / 9% | 0.7 / 0.4 | 59 |
| "O Rei-Vendaval" (200) | 10,613 / 271 | 15.9 s | 8.9 s **(fora)** | 20.7 s / 26.5 s | 28.8 s | 15 / 18 | 14% / 11% | 1.0 / 0.4 | 61 |
| "O Velho Kul" (240) | 14,771 / 361 | 17.1 s | 8.9 s **(fora)** | 21.7 s / 27.8 s | 31.2 s | 13 / 17 | 16% / 12% | 1.1 / 0.4 | 66 |
| "O Glutão de Brasa" (280) | 19,825 / 462 | 18.4 s | 9.0 s **(fora)** | 24.0 s / 36.8 s | 33.6 s | 13 / 19 | 13% / 9% | 1.0 / 0.3 | 72 |
| "Apep, a Serpente que Bebe o Rio" (320) | 24,688 / 575 | 18.6 s | 8.6 s **(fora)** | 19.2 s / 30.4 s | 33.1 s | 12 / 18 | 14% / 9% | 1.3 / 0.4 | 72 |
| "O Sonho da Destruição" (360) | 30,105 / 701 | 18.3 s | 8.2 s **(fora)** | 18.0 s / 29.6 s | 34.1 s | 10 / 16 | 17% / 10% | 1.4 / 0.4 | 72 |
| "A Grande Treva" (440) | 42,447 / 920 | 18.4 s | 7.6 s **(fora)** | 17.4 s / 29.7 s | 34.6 s | 9 / 16 | 20% / 11% | 1.5 / 0.4 | 73 |
| "Āz, a Capitã Cobiçosa" (478) | 49,179 / 1,071 | 18.6 s | 7.3 s **(fora)** | 14.5 s / 24.8 s | 35.1 s | 9 / 15 | 19% / 11% | 1.8 / 0.4 | 74 |

### 4.3 Regra "Perigoso"

Comuns (2 por luta): tempo para matar (caça) · golpes para morrer no pior crítico. O bicho fica no nível da linha; o herói está delta níveis abaixo.

| Bicho | +0 | +10 | +20 | +30 |
|---|---|---|---|---|
| comuns 100 | 5.5 s · 44 | 6.8 s · 27 | 14.7 s · 16 | 18.3 s · 8 |
| comuns 200 | 6.1 s · 46 | 7.2 s · 29 | 14.1 s · 19 | 16.0 s · 9 |
| comuns 300 | 6.4 s · 53 | 7.3 s · 34 | 13.9 s · 22 | 15.5 s · 11 |
| comuns 400 | 6.5 s · 48 | 7.2 s · 31 | 12.6 s · 21 | 14.6 s · 10 |
| comuns 478 | 6.6 s · 48 | 7.1 s · 32 | 12.7 s · 21 | 13.0 s · 10 |

Chefes do mundo: tempo para matar · golpes do chefe para morrer (pior crítico) · herói morre parado em.

| Bicho | +0 | +10 | +20 | +30 |
|---|---|---|---|---|
| "Hedammu, a Serpente Gulosa do Mar" (72) | 16.2 s · 18 · 30.1 s | 18.5 s · 11 · 23.3 s | 36.7 s · 6 · 13.2 s | 44.2 s · 3 · 5.4 s |
| "O Lodo-que-Engole" (88) | 16.8 s · 17 · 28.2 s | 19.0 s · 10 · 22.4 s | 37.4 s · 6 · 13.1 s | 43.9 s · 3 · 5.6 s |
| "Aydahar, o Dragão do Passo" (98) | 17.0 s · 16 · 26.7 s | 19.2 s · 10 · 21.4 s | 37.4 s · 6 · 12.7 s | 43.4 s · 3 · 5.6 s |
| "Os Quatro Gulosos" (140) | 15.3 s · 15 · 27.4 s | 16.6 s · 10 · 22.7 s | 30.5 s · 6 · 14.0 s | 34.9 s · 3 · 6.4 s |
| "Ohya, o Gigante Irmão" (150) | 15.3 s · 14 · 27.7 s | 16.8 s · 9 · 23.1 s | 30.8 s · 6 · 14.3 s | 35.2 s · 3 · 6.6 s |
| "O Rei-Vendaval" (200) | 15.9 s · 15 · 20.7 s | 17.1 s · 9 · 17.5 s | 30.7 s · 6 · 11.1 s | 33.8 s · 3 · 5.2 s |
| "O Velho Kul" (240) | 17.1 s · 13 · 21.7 s | 18.2 s · 9 · 18.5 s | 32.6 s · 6 · 11.8 s | 36.0 s · 3 · 5.6 s |
| "O Glutão de Brasa" (280) | 18.4 s · 13 · 24.0 s | 20.1 s · 8 · 20.6 s | 35.4 s · 5 · 13.2 s | 37.2 s · 3 · 6.1 s |
| "Apep, a Serpente que Bebe o Rio" (320) | 18.6 s · 12 · 19.2 s | 20.1 s · 8 · 16.6 s | 35.2 s · 5 · 10.7 s | 38.0 s · 3 · 5.1 s |
| "O Sonho da Destruição" (360) | 18.3 s · 10 · 18.0 s | 19.3 s · 7 · 15.6 s | 34.3 s · 5 · 10.1 s | 37.5 s · 3 · 5.2 s |
| "A Grande Treva" (440) | 18.4 s · 9 · 17.4 s | 19.3 s · 6 · 15.1 s | 33.9 s · 4 · 9.8 s | 35.8 s · 2 · 4.8 s |
| "Āz, a Capitã Cobiçosa" (478) | 18.6 s · 9 · 14.5 s | 19.4 s · 6 · 12.6 s | 33.9 s · 4 · 8.2 s | 35.7 s · 2 · 4.0 s |

### 4.4 Fora da meta (leitura "real")

- nada

### 4.5 Ajustes (piso de vida 170 e HeroToughnessGrowthPerLevel = 0 já aplicados; o resto é sugestão)

| Nível | 2 comuns, caça: hoje | ×vida dos comuns para 5,5 s | Depois: caça / abertura / só básico | Golpes para morrer |
|---|---|---|---|---|
| 100 | 5.5 s | ×0.97 | 5.4 s / 2.2 s / 8.3 s | 44 |
| 200 | 6.1 s | ×0.94 | 5.6 s / 2.1 s / 9.2 s | 46 |
| 300 | 6.4 s | ×0.92 | 5.4 s / 2.1 s / 9.2 s | 53 |
| 400 | 6.5 s | ×0.93 | 5.7 s / 1.0 s / 9.2 s | 48 |
| 500 | 6.6 s | ×0.92 | 5.6 s / 1.0 s / 9.2 s | 48 |

| Chefe (nível) | ×dano para o herói parado durar 15 s | Golpes para morrer depois | Maior golpe depois | ×vida se o herói "doc" for implementado |
|---|---|---|---|---|
| "Hedammu, a Serpente Gulosa do Mar" (72) | ×1.00 | 18 | 9% | ×1.48 |
| "O Lodo-que-Engole" (88) | ×1.00 | 17 | 12% | ×1.49 |
| "Aydahar, o Dragão do Passo" (98) | ×1.00 | 16 | 10% | ×1.51 |
| "Os Quatro Gulosos" (140) | ×1.00 | 15 | 11% | ×1.94 |
| "Ohya, o Gigante Irmão" (150) | ×1.00 | 14 | 12% | ×1.96 |
| "O Rei-Vendaval" (200) | ×1.00 | 15 | 14% | ×2.09 |
| "O Velho Kul" (240) | ×1.00 | 13 | 16% | ×2.07 |
| "O Glutão de Brasa" (280) | ×1.00 | 13 | 13% | ×2.05 |
| "Apep, a Serpente que Bebe o Rio" (320) | ×1.00 | 12 | 14% | ×2.14 |
| "O Sonho da Destruição" (360) | ×1.00 | 10 | 17% | ×2.27 |
| "A Grande Treva" (440) | ×1.00 | 9 | 20% | ×2.42 |
| "Āz, a Capitã Cobiçosa" (478) | ×0.97 | 9 | 18% | ×2.54 |

Piso de vida base 170 nos comuns do mundo aberto (aplicado no jogo do nível 100 em diante: `EnemyLevelScaling.CommonHealthFloor`):

| Nível | Caça (mais rápido–mais lento) | Abertura | Só básico | Golpes para morrer |
|---|---|---|---|---|
| 100 | 5.5 s (5.4–8.5) | 2.2 s | 9.2 s | 44 |
| 200 | 6.1 s (5.9–9.2) | 2.5 s | 9.4 s | 46 |
| 300 | 6.4 s (6.3–9.4) | 2.7 s | 10.1 s | 53 |
| 400 | 6.5 s (6.4–7.2) | 1.2 s | 10.1 s | 48 |
| 500 | 6.6 s (6.4–9.9) | 1.3 s | 10.2 s | 48 |

`EnemyLevelScaling.HeroToughnessGrowthPerLevel` (só mexe no dano dos bichos): herói parado morre em · golpes do chefe para morrer (pior crítico):

| Valor | chefe 72 | chefe 88 | chefe 98 | chefe 140 | chefe 150 | chefe 200 | chefe 240 | chefe 280 | chefe 320 | chefe 360 | chefe 440 | chefe 478 | Golpes de comum p/ morrer (100/200/300/400/500) | Orçamento 15–100 (level_scaling) |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| 0 | 30.1 s · 18 | 28.2 s · 17 | 26.7 s · 16 | 27.4 s · 15 | 27.7 s · 14 | 20.7 s · 15 | 21.7 s · 13 | 24.0 s · 13 | 19.2 s · 12 | 18.0 s · 10 | 17.4 s · 9 | 14.5 s · 9 | 44/46/53/48/48 | dentro |
| 0.005 | 23.8 s · 14 | 21.0 s · 13 | 19.2 s · 12 | 17.3 s · 10 | 17.0 s · 9 | 11.1 s **(fora)** · 8 | 10.6 s **(fora)** · 7 | 10.7 s **(fora)** · 6 | 7.9 s **(fora)** · 5 | 6.9 s **(fora)** · 4 | 5.8 s **(fora)** · 4 | 4.6 s **(fora)** · 3 | 32/25/23/17/15 | dentro |
| 0.0025 | 26.5 s · 16 | 24.0 s · 14 | 22.2 s · 13 | 21.1 s · 12 | 20.9 s · 11 | 14.3 s · 10 | 14.1 s · 9 | 14.7 s · 8 | 11.1 s **(fora)** · 7 | 9.8 s **(fora)** · 6 | 8.6 s **(fora)** · 5 | 6.9 s **(fora)** · 4 | 37/32/31/25/22 | dentro |
