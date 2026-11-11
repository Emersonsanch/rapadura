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

## 5. Economia

> Medido em 2026-11-11 com `python -I tools/balance/economy.py` (também no fim de `python -I tools/balance/power_stack.py`)
> e travado em `tools/balance/test_economy.py`. Tudo lido do C#/assets: curva (`LevelCurveMath`), XP de abate
> (`EnemyLevelScaling.KillExperience`), missões (`MidStoryCatalog`, `LateStoryCatalog`, `CityQuestCatalog`,
> `RegionQuestCatalog`), mapas e passagem (`MapCatalog`, `MapAccessRules`), exigências (`MapRequirementRules` ×
> `CityShopCatalog`), lembranças de chefe (`HouseCraftRules` × LootItem do asset), conjuntos (`ArmorSetRules`) e tabelas de
> bioma.

### 5.1 Resumo

- **XP:** os marcos batem com `Progressao500.md` §1.3 (50: 6,7 h, −16%; 100: 42,8 h; 200: 155 h; 300: 307 h; 400: 511 h;
  500: 857 h; todos dentro de ±20%). Acima do 50 nenhuma faixa de 10 níveis é parede ou queda (passo ×1,02–×1,49 por
  faixa; o ×1,49 é a entrada 51–61, saída da campanha). De 1 a 30 os níveis passam voando (2–4 min/nível, 31–38% de
  missão), o que é a campanha/tutorial; 31–51 sobe ×2,2–2,5 por faixa (curva da campanha, ainda dentro do marco 50).
  Grind mais longo: 481–491 com 4,3 h/nível (42 h a faixa), na meta do doc (860 h no 500).
- **Missões quase não contam acima do 100:** 173 mil XP de missão no jogo todo (cidade 93 mil, Cartas 60 mil, meio
  18 mil) contra 1 bilhão de XP de 100 a 500 → 0% por faixa de 101 em diante (o doc pede 20–30%).
- **Ouro é plano:** ~930–1 130 de ouro/h em **todos** os níveis (item_gold de 1,2/abate + venda das 4 tabelas de bioma da
  campanha, ~3,5–4,5/abate), porque nem o item_gold nem as tabelas crescem com o nível. A passagem cresce 15 por nível
  da barca: do mapa 29 (barca 65) ao 100 ela custa mais de 1 h de caça (no 100: 7 535 = 8 h). Nunca passa do ouro
  acumulado até ali (no 100 o herói juntou 614 mil), então **não trava ninguém**, mas é o único gasto que cresce.
- **Exigências dos mapas:** todas pagáveis no nível de andar; a mais cara em minutos é `item_grampos_escalada` 950 no mapa
  38 (54 min de caça, 17 515 na bolsa). Nenhuma loja vende abaixo do que paga de volta.
- **Loot de chefe:** cada lembrança (`item_chefe_*`) sai em 4,8–13,3 abates do chefe (0,4–1,1 h só de respawn) para tudo o
  que as roupas e troféus da casa gastam; todas são usadas. Conjuntos 13–16: 1 100–1 350 abates na região (5,5–6,8 h).
- **"Perigoso +30":** com a resistência do herói zerada ainda assusta: um par de comuns tira 101–208% da vida numa luta
  sem poção, e parado o herói morre em 6,8–10,2 s (contra 31–37 s no mesmo nível). Nada a mudar.

### 5.2 Erros de dado corrigidos

- Nenhum. Nenhuma passagem passa de todo o ouro possível no nível de entrada, nenhuma exigência comprada passa do ouro na
  bolsa ao chegar e nenhuma linha de loja dá lucro na revenda; `MapCatalog.cs` e `CityShopCatalog.cs` ficaram como estavam.
  A passagem cara (mapas 29–100) não se corrige pelo `Fee` do `MapCatalog` (ele só muda a base; a parte por nível vem de
  `MapAccessRules.WarpFeePerLevel`), por isso fica como sugestão.

### 5.3 Sugestões (não aplicadas)

1. **Teto da passagem:** `CalculateWarpCost` = 50 + min(nível da barca, **57**) × 15 → passagem máxima **905** (≤ 60 min
   de caça na faixa mais pobre, 910 ouro/h no 281–291). Alternativa: o ouro do abate (item_gold e venda da tabela) × nível/50
   acima do 50 → ~9 300 ouro/h no 500, e a passagem atual de 7 535 cabe em ~49 min.
2. **XP de missão proporcional ao nível:** etapa da história do meio = **50%** do XP do nível em que abre, Carta =
   **150%**, missão de cidade acima do 25 = **25%** (nunca menos que hoje). Missão passa a 5–31% das faixas 51–500 (o mínimo
   nas faixas sem Carta); marcos 100: 37 h (−17%), 200: 139 h, 500: 797 h (−7%), ainda dentro de ±20%. Tabela em
   5.4 "Sugestão".
3. **Ralo de ouro depois do 100:** o herói acumula 37 mil no 100 e 616 mil no 500 e só há equipamento de loja até 41 de
   ouro (fora os itens de exigência, ≤ 950). Falta um gasto que cresça (refino, conserto, casa), ou o ouro perde sentido.
4. **Chefe do mundo vale menos que comuns:** acampando (×9, MapBoss, respawn 300 s) dá ~51% do XP/h de caçar comuns do
   mesmo nível. Se o chefe deve compensar, multiplicador **×18** empata e ×30 (WorldBoss, §1.2) dá ~1,7×.

### 5.4 Tabelas (saída do `economy.py`)

#### XP e ouro por faixa de 10 níveis

Ritmo: 200 abates/h de caça ativa, horas de jogo = abate × 1.4 (Progressao500 §1.3); XP de missão gasto primeiro, o resto em comuns do nível do herói. Ouro = item_gold do saque base + tabela do bioma vendida a 50%; o maior gasto é a exigência comprada mais cara da faixa.

| Faixa | XP da faixa | XP de missão | Abates | Horas de jogo (h/nível) | Acumulado | Ouro na faixa: abate + missão | Ouro/h | Ouro acumulado | Maior exigência comprada |
|---|---|---|---|---|---|---|---|---|---|
| 1–11 | 1 593 | 606 (38%) | 60 | 0.4 (3 min) | 0.4 h | 302 + 15 | 1 005 | 317 | item_manta_la 42 (mapa 14) |
| 11–21 | 4 133 | 1 534 (37%) | 57 | 0.4 (2 min, ×0.95) | 0.8 h | 300 + 65 | 1 056 | 683 | item_corda_escalada 36 (mapa 5) |
| 21–31 | 10 721 | 3 280 (31%) | 103 | 0.7 (4 min, ×1.81) | 1.5 h | 545 + 325 | 1 060 | 1 553 | item_lamparina_runica 150 (mapa 21) |
| 31–41 | 27 808 | 3 200 (12%) | 255 | 1.8 (11 min, ×2.48) | 3.3 h | 1 454 + 285 | 1 141 | 3 291 | item_asas_lona_takhsich 420 (mapa 24) |
| 41–51 | 72 132 | 4 360 (6%) | 567 | 4.0 (24 min, ×2.22) | 7.3 h | 3 073 + 395 | 1 085 | 6 760 | item_reliquia_artifices 400 (mapa 27) |
| 51–61 | 147 126 | 6 900 (5%) | 845 | 5.9 (35 min, ×1.49) | 13.2 h | 4 393 + 630 | 1 039 | 11 783 |  |
| 61–71 | 241 300 | 7 240 (3%) | 952 | 6.7 (40 min, ×1.13) | 19.9 h | 4 840 + 640 | 1 016 | 17 263 | item_roupa_termica 900 (mapa 36) |
| 71–81 | 369 027 | 3 140 (1%) | 1 058 | 7.4 (44 min, ×1.11) | 27.3 h | 5 611 + 270 | 1 060 | 23 144 | item_grampos_escalada 950 (mapa 38) |
| 81–91 | 535 429 | 7 140 (1%) | 1 136 | 8.0 (48 min, ×1.07) | 35.2 h | 6 085 + 610 | 1 072 | 29 839 |  |
| 91–101 | 745 628 | 14 350 (2%) | 1 205 | 8.4 (51 min, ×1.06) | 43.7 h | 6 269 + 1 200 | 1 040 | 37 308 |  |
| 101–111 | 1 004 748 | 6 900 (1%) | 1 296 | 9.1 (54 min, ×1.08) | 52.7 h | 6 847 + 550 | 1 057 | 44 706 |  |
| 111–121 | 1 317 913 | 4 900 (0%) | 1 372 | 9.6 (58 min, ×1.06) | 62.3 h | 7 250 + 380 | 1 057 | 52 335 |  |
| 121–131 | 1 690 242 | 0 (0%) | 1 448 | 10.1 (61 min, ×1.06) | 72.5 h | 7 651 + 0 | 1 057 | 59 986 |  |
| 131–141 | 2 126 860 | 5 300 (0%) | 1 513 | 10.6 (64 min, ×1.04) | 83.1 h | 8 001 + 400 | 1 058 | 68 387 |  |
| 141–151 | 2 632 886 | 8 700 (0%) | 1 577 | 11.0 (66 min, ×1.04) | 94.1 h | 8 731 + 630 | 1 107 | 77 748 |  |
| 151–161 | 3 213 449 | 3 100 (0%) | 1 645 | 11.5 (69 min, ×1.04) | 106 h | 8 960 + 220 | 1 089 | 86 929 |  |
| 161–171 | 3 873 666 | 0 (0%) | 1 710 | 12.0 (72 min, ×1.04) | 118 h | 9 310 + 0 | 1 089 | 96 239 |  |
| 171–181 | 4 618 660 | 6 100 (0%) | 1 769 | 12.4 (74 min, ×1.03) | 130 h | 9 670 + 440 | 1 094 | 106 349 |  |
| 181–191 | 5 453 557 | 3 200 (0%) | 1 830 | 12.8 (77 min, ×1.03) | 143 h | 10 362 + 230 | 1 133 | 116 940 |  |
| 191–201 | 6 383 476 | 3 300 (0%) | 1 888 | 13.2 (79 min, ×1.03) | 156 h | 10 695 + 230 | 1 133 | 127 865 |  |
| 201–211 | 7 413 542 | 0 (0%) | 1 947 | 13.6 (82 min, ×1.03) | 170 h | 11 554 + 0 | 1 187 | 139 419 |  |
| 211–221 | 8 548 875 | 6 500 (0%) | 2 002 | 14.0 (84 min, ×1.03) | 184 h | 11 859 + 460 | 1 185 | 151 737 |  |
| 221–231 | 9 794 601 | 3 400 (0%) | 2 058 | 14.4 (86 min, ×1.03) | 198 h | 12 013 + 240 | 1 168 | 163 991 |  |
| 231–241 | 11 155 840 | 0 (0%) | 2 113 | 14.8 (89 min, ×1.03) | 213 h | 12 334 + 0 | 1 168 | 176 325 |  |
| 241–251 | 12 637 715 | 0 (0%) | 2 166 | 15.2 (91 min, ×1.03) | 228 h | 12 505 + 0 | 1 155 | 188 830 |  |
| 251–261 | 14 245 352 | 14 000 (0%) | 2 217 | 15.5 (93 min, ×1.02) | 244 h | 12 654 + 980 | 1 142 | 202 464 |  |
| 261–271 | 15 983 867 | 3 700 (0%) | 2 270 | 15.9 (95 min, ×1.02) | 259 h | 11 671 + 260 | 1 028 | 214 395 |  |
| 271–281 | 17 858 387 | 0 (0%) | 2 321 | 16.2 (97 min, ×1.02) | 276 h | 11 936 + 0 | 1 028 | 226 332 |  |
| 281–291 | 19 874 032 | 0 (0%) | 2 371 | 16.6 (100 min, ×1.02) | 292 h | 10 785 + 0 | 910 | 237 117 |  |
| 291–301 | 22 035 929 | 11 100 (0%) | 2 420 | 16.9 (102 min, ×1.02) | 309 h | 11 043 + 760 | 913 | 248 919 |  |
| 301–311 | 24 373 174 | 3 900 (0%) | 2 472 | 17.3 (104 min, ×1.02) | 326 h | 11 625 + 270 | 941 | 260 814 |  |
| 311–321 | 26 987 199 | 0 (0%) | 2 534 | 17.7 (106 min, ×1.03) | 344 h | 11 916 + 0 | 941 | 272 730 |  |
| 321–331 | 29 938 017 | 0 (0%) | 2 608 | 18.3 (110 min, ×1.03) | 362 h | 12 635 + 0 | 969 | 285 364 |  |
| 331–341 | 33 275 353 | 11 700 (0%) | 2 695 | 18.9 (113 min, ×1.03) | 381 h | 13 012 + 790 | 966 | 299 166 |  |
| 341–351 | 37 054 209 | 4 100 (0%) | 2 797 | 19.6 (117 min, ×1.04) | 401 h | 13 109 + 280 | 937 | 312 556 |  |
| 351–361 | 41 335 093 | 0 (0%) | 2 914 | 20.4 (122 min, ×1.04) | 421 h | 13 657 + 0 | 937 | 326 213 |  |
| 361–371 | 46 184 301 | 0 (0%) | 3 046 | 21.3 (128 min, ×1.05) | 443 h | 14 141 + 0 | 928 | 340 354 |  |
| 371–381 | 51 674 155 | 0 (0%) | 3 194 | 22.4 (134 min, ×1.05) | 465 h | 14 829 + 0 | 928 | 355 183 |  |
| 381–391 | 57 883 263 | 0 (0%) | 3 359 | 23.5 (141 min, ×1.05) | 489 h | 15 595 + 0 | 928 | 370 779 |  |
| 391–401 | 64 896 778 | 0 (0%) | 3 542 | 24.8 (149 min, ×1.05) | 513 h | 16 443 + 0 | 928 | 387 221 |  |
| 401–411 | 72 806 656 | 0 (0%) | 3 742 | 26.2 (157 min, ×1.06) | 540 h | 17 374 + 0 | 928 | 404 596 |  |
| 411–421 | 81 711 909 | 8 200 (0%) | 3 961 | 27.7 (166 min, ×1.06) | 567 h | 18 385 + 550 | 928 | 423 531 |  |
| 421–431 | 91 718 860 | 4 200 (0%) | 4 200 | 29.4 (176 min, ×1.06) | 597 h | 19 451 + 280 | 926 | 443 262 |  |
| 431–441 | 102 941 399 | 0 (0%) | 4 458 | 31.2 (187 min, ×1.06) | 628 h | 20 648 + 0 | 926 | 463 910 |  |
| 441–451 | 115 501 245 | 0 (0%) | 4 737 | 33.2 (199 min, ×1.06) | 661 h | 22 045 + 0 | 931 | 485 955 |  |
| 451–461 | 129 528 192 | 0 (0%) | 5 037 | 35.3 (212 min, ×1.06) | 696 h | 23 439 + 0 | 931 | 509 394 |  |
| 461–471 | 145 160 375 | 0 (0%) | 5 358 | 37.5 (225 min, ×1.06) | 734 h | 24 934 + 0 | 931 | 534 329 |  |
| 471–481 | 162 544 524 | 0 (0%) | 5 701 | 39.9 (239 min, ×1.06) | 774 h | 26 532 + 0 | 931 | 560 861 |  |
| 481–491 | 181 836 214 | 4 400 (0%) | 6 067 | 42.5 (255 min, ×1.06) | 816 h | 28 235 + 290 | 931 | 589 385 |  |
| 491–500 | 181 856 128 | 4 600 (0%) | 5 793 | 40.5 (270 min, ×1.06) | 857 h | 26 956 + 300 | 931 | 616 641 |  |

Marcos (horas de jogo acumuladas): 50: 6.7 h (doc 8 h, -16%) · 100: 42.8 h (doc 45 h, -5%) · 200: 155 h (doc 157 h, -1%) · 300: 307 h (doc 310 h, -1%) · 400: 511 h (doc 513 h, -0%) · 500: 857 h (doc 860 h, -0%)

XP e ouro de missão por fonte: meio: 15 missões, 18 330 XP, 1 650 ouro · cartas: 17 missões, 60 500 XP, 4 210 ouro · cidade: 46 missões, 93 080 XP, 7 100 ouro · campanha: 8 missões, 1 140 XP, 15 ouro (total 173 050 XP).

#### XP por bicho

Campanha (asset × tabela no nível da banda): Forest 3: 10 × 13 · Desert 7: 18 × 24 · Mountain 10: 28 × 32 · Caves 15: 40 × 45.

| Chefe do mundo (nível) | XP hoje (×9, MapBoss) | XP se fosse ×30 (WorldBoss, §1.2) | XP/h acampando o chefe (×9) | XP/h caçando comuns |
|---|---|---|---|---|
| Hedammu, a Serpente Gulosa do Mar (72) | 2 764 | 9 213 | 31 291 | 61 400 |
| O Lodo-que-Engole (88) | 4 474 | 14 913 | 50 649 | 99 400 |
| Aydahar, o Dragão do Passo (98) | 5 793 | 19 308 | 65 581 | 128 800 |
| Os Quatro Gulosos (140) | 13 634 | 45 448 | 154 347 | 303 000 |
| Ohya, o Gigante Irmão (150) | 16 090 | 53 632 | 182 151 | 357 600 |
| O Rei-Vendaval (200) | 32 092 | 106 973 | 363 306 | 713 200 |
| O Velho Kul (240) | 49 709 | 165 695 | 562 743 | 1 104 600 |
| O Glutão de Brasa (280) | 71 962 | 239 874 | 814 664 | 1 599 200 |
| Apep, a Serpente que Bebe o Rio (320) | 99 148 | 330 494 | 1 122 430 | 2 203 200 |
| O Sonho da Destruição (360) | 131 538 | 438 459 | 1 489 109 | 2 923 000 |
| A Grande Treva (440) | 212 917 | 709 724 | 2 410 381 | 4 731 400 |
| Āz, a Capitã Cobiçosa (478) | 259 748 | 865 825 | 2 940 543 | 5 772 200 |

#### Sugestão: XP de missão proporcional ao nível

Etapa da história do meio = 50% do XP do nível em que abre, Carta = 150%, missão de cidade (acima do 25) = 25%; nunca menos que hoje:

| Faixa | XP de missão hoje | Com a sugestão | Horas da faixa: hoje → sugestão |
|---|---|---|---|
| 21–31 | 31% | 33% | 0.7 → 0.7 |
| 41–51 | 6% | 13% | 4.0 → 3.7 |
| 51–61 | 5% | 20% | 5.9 → 5.0 |
| 91–101 | 2% | 31% | 8.4 → 6.0 |
| 101–111 | 1% | 25% | 9.1 → 6.9 |
| 151–161 | 0% | 15% | 11.5 → 9.8 |
| 201–211 | 0% | 5% | 13.6 → 13.0 |
| 261–271 | 0% | 14% | 15.9 → 13.5 |
| 301–311 | 0% | 19% | 17.3 → 13.9 |
| 341–351 | 0% | 19% | 19.6 → 15.7 |
| 421–431 | 0% | 15% | 29.4 → 25.0 |
| 491–500 | 0% | 17% | 40.5 → 33.8 |

Marcos com a sugestão: 50: 6.2 h · 100: 37.2 h · 200: 139 h · 300: 278 h · 400: 469 h · 500: 797 h

#### Exigências dos mapas (na ordem do nível de andar)

| Mapa (andar / barca) | Exigência | Como (primo sem o dom) | Item | Custo | Ouro na bolsa ao chegar | Custo em min de caça |
|---|---|---|---|---|---|---|
| 14 (9 / 14) | WoolCloak | compra | item_manta_la | 42 | 265 | 2 min |
| 19 (12 / 16) | TorchRequired | compra | item_lamparina_oleo | 27 | 300 | 2 min |
| 20 (13 / 18) | TorchRequired | já tinha | item_lamparina_oleo | — | 313 | — |
| 03 (14 / 18) | LeatherBoots (Joaquim passa) | missão | item_leather_boots | — | 355 | — |
| 05 (18 / 22) | ElevatedPlatforms | compra | item_corda_escalada | 36 | 485 | 2 min |
| 10 (20 / 25) | FreshWater | compra | item_odre_agua | 28 | 509 | 2 min |
| 06 (24 / 28) | GardenKey | missão | item_chave_jardim | — | 754 | — |
| 12 (28 / 32) | PoisonProtection (Lavine passa) | missão | item_teriaga | — | 1 082 | — |
| 17 (28 / 34) | PickaxeTorch | compra | item_pickaxe | 18 | 1 082 | 1 min |
| 21 (30 / 35) | RunicLamp | compra | item_lamparina_runica [Cleared(BiomeType.Caves)] | 150 | 1 275 | 8 min |
| 18 (34 / 40) | BlizzardResistance | missão | item_cha_gengibre | — | 1 651 | — |
| 23 (36 / 42) | EternalFireOil | missão | item_oleo_fogo_eterno | — | 1 950 | — |
| 24 (40 / 46) | WingsOrDoubleJump | compra | item_asas_lona_takhsich | 420 | 2 729 | 21 min |
| 25 (42 / 48) | BoatOrDock | missão | item_passagem_dhow | — | 2 818 | — |
| 27 (50 / 56) | ArtificerRelic | compra | item_reliquia_artifices | 400 | 5 604 | 23 min |
| 28 (52 / 60) | BreathingShell | missão | item_concha_respiravel | — | 6 079 | — |
| 29 (58 / 65) | AirBubble | missão | item_bolha_ar | — | 8 996 | — |
| 30 (60 / 70) | RootBoots (Joaquim passa) | missão | item_botas_raiz | — | 10 110 | — |
| 32 (62 / 68) | RootBridgeBalance (Maithe passa) | missão | item_vara_equilibrio | — | 11 138 | — |
| 33 (66 / 74) | SaddleRequired | compra | item_barco_junco | 600 | 13 179 | 36 min |
| 36 (68 / 75) | ThermalClothes | compra | item_roupa_termica | 900 | 13 775 | 53 min |
| 34 (72 / 80) | AntiVenomAmulet | missão | item_amuleto_antiveneno | — | 15 182 | — |
| 38 (76 / 84) | ClimbingCrampons | compra | item_grampos_escalada | 950 | 17 515 | 54 min |
| 35 (78 / 85) | PurifyingFruit | missão | item_fruta_purificadora | — | 17 708 | — |
| 39 (80 / 88) | WingsRequired | sem compra | asas do Tacho / montaria voadora | — | 18 985 | — |
| 40 (86 / 94) | StormProtection | missão | item_capa_tempestade | — | 22 575 | — |
| 42 (92 / 100) | SunSeal | missão | item_selo_sol | — | 26 892 | — |
| 45 (110 / 120) | TorchRequired | missão | item_lamparina_oleo | — | 40 428 | — |
| 52 (128 / 140) | ClimbingCrampons | missão | item_grampos_escalada | — | 54 082 | — |
| 57 (162 / 170) | StormProtection | missão | item_capa_tempestade | — | 84 273 | — |
| 58 (176 / 186) | WingsOrDoubleJump | já tinha | item_asas_lona_takhsich | — | 97 448 | — |
| 63 (208 / 216) | BlizzardResistance | missão | item_cha_gengibre | — | 132 346 | — |
| 64 (222 / 230) | ThermalClothes | missão | item_roupa_termica | — | 149 354 | — |
| 67 (230 / 238) | ClimbingCrampons | missão | item_grampos_escalada | — | 159 204 | — |
| 68 (240 / 248) | BoatOrDock | missão | item_passagem_dhow | — | 171 507 | — |
| 70 (260 / 268) | FreshWater | missão | item_odre_agua | — | 197 740 | — |
| 72 (262 / 270) | SaddleRequired | missão | item_barco_junco | — | 200 048 | — |
| 73 (270 / 278) | TorchRequired | missão | item_lamparina_oleo | — | 209 645 | — |
| 74 (280 / 288) | RunicLamp | missão | item_lamparina_runica | — | 221 555 | — |
| 75 (288 / 296) | EternalFireOil | missão | item_oleo_fogo_eterno | — | 230 286 | — |
| 76 (300 / 308) | AntiVenomAmulet | missão | item_amuleto_antiveneno | — | 243 941 | — |
| 78 (302 / 310) | FreshWater | missão | item_odre_agua | — | 246 500 | — |
| 79 (310 / 318) | SaddleRequired | missão | item_barco_junco | — | 256 068 | — |
| 80 (318 / 326) | ElevatedPlatforms | já tinha | item_corda_escalada | — | 265 550 | — |
| 81 (330 / 338) | ClimbingCrampons | missão | item_grampos_escalada | — | 280 512 | — |
| 82 (340 / 348) | TorchRequired | missão | item_lamparina_oleo | — | 294 043 | — |
| 84 (344 / 352) | PoisonProtection (Lavine passa) | missão | item_teriaga | — | 299 475 | — |
| 85 (356 / 364) | EternalFireOil | missão | item_oleo_fogo_eterno | — | 315 740 | — |
| 86 (368 / 376) | WingsOrDoubleJump | já tinha | item_asas_lona_takhsich | — | 332 472 | — |
| 87 (382 / 390) | AirBubble | missão | item_bolha_ar | — | 353 136 | — |
| 88 (396 / 404) | RunicLamp | missão | item_lamparina_runica | — | 375 318 | — |
| 89 (408 / 416) | RootBridgeBalance (Maithe passa) | missão | item_vara_equilibrio | — | 395 709 | — |
| 90 (420 / 428) | TorchRequired | missão | item_lamparina_oleo | — | 418 077 | — |
| 94 (442 / 450) | ElevatedPlatforms | já tinha | item_corda_escalada | — | 462 483 | — |
| 95 (452 / 460) | WingsOrDoubleJump | já tinha | item_asas_lona_takhsich | — | 484 663 | — |
| 99 (484 / 492) | StormProtection | missão | item_capa_tempestade | — | 565 866 | — |

#### Passagem da barca

Passagem = 50 + nível da barca × 15 (`MapAccessRules.CalculateWarpCost`; o dobro com a mochila pesada).

| Mapa | Nível da barca | Passagem | Ouro acumulado no nível (sem vender nada) | Ouro/h no nível | Passagem em min de caça |
|---|---|---|---|---|---|
| 02 | 5 | 125 | 160 (39) | 1 025 | 7 min |
| 03 | 18 | 320 | 554 (141) | 1 060 | 18 min |
| 13 | 12 | 230 | 342 (93) | 1 042 | 13 min |
| 25 | 48 | 770 | 5 553 (2 057) | 1 042 | 44 min |
| 41 | 95 | 1 475 | 32 805 (10 341) | 1 045 | 85 min **(fora)** |
| 47 | 104 | 1 610 | 39 336 (12 382) | 1 057 | 91 min **(fora)** |
| 59 | 184 | 2 810 | 109 424 (29 989) | 1 133 | 149 min **(fora)** |
| 71 | 264 | 4 010 | 206 196 (52 011) | 1 028 | 234 min **(fora)** |
| 83 | 344 | 5 210 | 303 046 (77 642) | 937 | 333 min **(fora)** |
| 91 | 424 | 6 410 | 429 246 (110 827) | 926 | 415 min **(fora)** |
| 100 | 499 | 7 535 | 613 571 (159 047) | 931 | 486 min **(fora)** |

Acima de 60 min de caça: 72 de 99 mapas, do 29 (barca 65) ao 100. Passagem acima de todo o ouro acumulado até o nível da barca: nenhuma; acima do ouro sem vender o saque: 02, 03, 04, 05, 06, 07, 08, 09, 10, 11, 13, 14, 15, 19, 20.

Teto sugerido: a passagem segue o nível da barca só até o 57 (passagem máxima 905, ≤ 60 min de caça na faixa mais pobre).

#### Loot

Lembranças dos chefes do mundo (`item_chefe_*`, LootItem do asset; o chefe volta em 300 s):

| Chefe (nível) | Item | Chance × qtd | Valor (venda) | Receitas que usam / quantidade gasta | Abates esperados | Horas (só esperando o respawn) |
|---|---|---|---|---|---|---|
| Hedammu, a Serpente Gulosa do Mar (72) | item_chefe_escama_hedammu | 60% × 1.5 | 120 (60) | 3 / 7 | 7.8 | 0.6 |
| O Lodo-que-Engole (88) | item_chefe_lama_limpa | 60% × 1.5 | 140 (70) | 3 / 5 | 5.6 | 0.5 |
| Aydahar, o Dragão do Passo (98) | item_chefe_escama_aydahar | 55% × 1.5 | 160 (80) | 2 / 4 | 4.8 | 0.4 |
| Os Quatro Gulosos (140) | item_chefe_bronze_antigo | 50% × 1 | 260 (130) | 4 / 5 | 10.0 | 0.8 |
| Ohya, o Gigante Irmão (150) | item_chefe_ferro_antigo | 50% × 1 | 280 (140) | 4 / 6 | 12.0 | 1.0 |
| O Rei-Vendaval (200) | item_chefe_pena_vendaval | 45% × 1 | 320 (160) | 3 / 5 | 11.1 | 0.9 |
| O Velho Kul (240) | item_chefe_barbatana_kul | 45% × 1 | 360 (180) | 3 / 4 | 8.9 | 0.7 |
| O Glutão de Brasa (280) | item_chefe_brasa_viva | 45% × 1 | 400 (200) | 3 / 5 | 11.1 | 0.9 |
| Apep, a Serpente que Bebe o Rio (320) | item_chefe_escama_apep | 40% × 1 | 450 (225) | 3 / 5 | 12.5 | 1.0 |
| O Sonho da Destruição (360) | item_chefe_tabua_sonho | 40% × 1 | 500 (250) | 3 / 4 | 10.0 | 0.8 |
| A Grande Treva (440) | item_chefe_lasca_selo | 30% × 1 | 700 (350) | 3 / 4 | 13.3 | 1.1 |
| Āz, a Capitã Cobiçosa (478) | item_chefe_semente_luz | 30% × 1 | 800 (400) | 3 / 4 | 13.3 | 1.1 |

Conjuntos das regiões 13–16 (`ArmorSetRules.DropChance`, só as feras do conjunto soltam):

| Conjunto | Feras / espécies da região | Abates de fera p/ as 2 peças | Abates caçando a região | Horas de abate |
|---|---|---|---|---|
| cais_do_papiro | 12 / 22 | 600 | 1 100 | 5.5 |
| tijolo_azul | 12 / 24 | 600 | 1 200 | 6.0 |
| selo_dos_artifices | 12 / 25 | 600 | 1 250 | 6.2 |
| cais_de_luz | 12 / 27 | 600 | 1 350 | 6.8 |

Saque de bioma por faixa (o valor por abate, venda a 50%; só existem as 4 tabelas da campanha):

| Nível | Espécies | Bioma das espécies | item_gold por abate | Venda da tabela por abate | Total/abate | Ouro/h |
|---|---|---|---|---|---|---|
| 5 | 19 | Forest 37%, Desert 32%, Mountain 26%, Caves 5% | 1.20 | 3.92 | 5.12 | 1025 |
| 25 | 24 | Forest 29%, Mountain 29%, Desert 25%, Caves 17% | 1.20 | 4.10 | 5.30 | 1060 |
| 50 | 15 | Forest 33%, Desert 27%, Caves 20%, Mountain 20% | 1.20 | 4.01 | 5.21 | 1042 |
| 100 | 27 | Mountain 37%, Forest 33%, Desert 19%, Caves 11% | 1.20 | 4.08 | 5.28 | 1057 |
| 200 | 36 | Mountain 72%, Forest 17%, Desert 8%, Caves 3% | 1.20 | 4.46 | 5.66 | 1133 |
| 300 | 46 | Forest 72%, Mountain 20%, Desert 7%, Caves 2% | 1.20 | 3.50 | 4.70 | 941 |
| 400 | 25 | Forest 76%, Mountain 12%, Caves 8%, Desert 4% | 1.20 | 3.44 | 4.64 | 928 |
| 500 | 27 | Forest 74%, Mountain 15%, Desert 7%, Caves 4% | 1.20 | 3.45 | 4.65 | 931 |

Equipamento à venda nas lojas (preço em ouro): item_leather_boots 9, item_wooden_sword 10, item_leather_helmet 11, item_hammer 15, item_leather_chest 18, item_pickaxe 18, item_axe 18, item_hunting_bow 33, item_iron_sword 41, item_asas_lona_takhsich 420, item_botas_raiz 520, item_amuleto_antiveneno 700, item_roupa_termica 900.

#### "Perigoso +30" nos comuns

| Comuns (nível) | Herói | Caça de 2 | Golpes p/ morrer (pior crítico, média / mín.) | Parado morre em | Vida gasta numa luta |
|---|---|---|---|---|---|
| 100 | +0 (100) | 5.5 s | 44 / 28 | 32.3 s | 13% |
| 100 | +30 (70) | 18.3 s | 8 / 5 | 6.8 s | 208% |
| 200 | +0 (200) | 6.1 s | 46 / 27 | 30.9 s | 16% |
| 200 | +30 (170) | 16.0 s | 9 / 6 | 7.8 s | 160% |
| 300 | +0 (300) | 6.4 s | 53 / 31 | 37.4 s | 13% |
| 300 | +30 (270) | 15.5 s | 11 / 6 | 9.9 s | 123% |
| 400 | +0 (400) | 6.5 s | 48 / 31 | 34.8 s | 15% |
| 400 | +30 (370) | 14.6 s | 10 / 7 | 9.5 s | 120% |
| 478 | +0 (478) | 6.6 s | 48 / 33 | 36.9 s | 14% |
| 478 | +30 (448) | 13.0 s | 10 / 7 | 10.2 s | 101% |

"Vida gasta numa luta" = dano esperado dos dois comuns (os dois batem na primeira metade da caça de 2, um na segunda) em % da vida, sem poção nem cura; acima de 100% o herói morre antes de matar o par.

#### Fora do orçamento

- fare expensive: 72 (29 a 100)
