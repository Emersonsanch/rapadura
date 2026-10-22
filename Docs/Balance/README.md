# Rapadura — Balanceamento (planilhas)

> Fonte única de verdade da curva de progressão do jogador + planilhas de revisão de
> inimigos e skills. Cobre o item da FASE 3 do `TODO.md`: "Curva de balanceamento
> documentada em planilha".

| Arquivo | Papel | Volta para o jogo? |
|---|---|---|
| `progression.csv` | Curva do jogador, níveis 1..50 — **fonte da verdade** | Sim, via *Import Progression CSV* |
| `enemies.csv` | Os 9 inimigos de `Assets/Resources/Enemies` (+ bioma e peso de spawn) | Não (só revisão) |
| `skills.csv` | As 45 skills de `Assets/Resources/Skills/**` (20 gerais + 25 de herói) | Não (só revisão) |

Outros dados de balanceamento (editados direto no Inspector, sem planilha):
`Assets/Resources/Balance/PlayerCombatBalance.asset` — dano base do ataque básico, chance e multiplicador
de crítico base (ver §3). Apagar esse asset volta o ataque básico ao valor do Inspector da cena (15).

---

## 1. Como editar a curva

1. Abra `Docs/Balance/progression.csv` no Excel / Google Sheets / LibreOffice.
   - Separador **vírgula**, decimal com **ponto** (`1.05`, não `1,05`). No Sheets, importe como
     "CSV" e exporte de volta como "CSV"; no Excel pt-BR, use *Dados > De Texto/CSV* e
     selecione vírgula — e salve como "CSV UTF-8".
2. Edite as colunas:

| Coluna | Significado | Onde é usado |
|---|---|---|
| `level` | Nível (1..N, contíguo — buracos são rejeitados) | — |
| `xp_to_next` | XP para sair deste nível para o próximo (> 0) | `PlayerStats.GetExperienceToNextLevel` |
| `xp_total` | XP acumulado para **chegar** neste nível. Informativo, **ignorado** no import | — |
| `hp` / `mana` / `stamina` | Máximo no nível. Só a **diferença para o nível 1** é somada à base do inspector de `PlayerStats` | `PlayerStats.MaxHealth/MaxMana/MaxStamina` |
| `damage_mult` | Multiplicador de dano de skill por nível do jogador (1 = sem bônus) | `SkillManager` (dano de skills) |
| `attribute_points` | Pontos de atributo ganhos ao **chegar** neste nível | `AttributeSet` |
| `skill_points` | Pontos de skill ganhos ao **chegar** neste nível | `SkillManager` |

   Colunas podem ser reordenadas; linhas em branco e linhas começando com `#` são ignoradas.
   Colunas ausentes assumem o padrão (100 / 1 / 0).
3. No Unity: **Rapadura > Balance > Import Progression CSV**. Grava em
   `Assets/Resources/Balance/ProgressionCurve.asset`. Se algum valor estiver inválido, nada é
   aplicado e um diálogo mostra a linha com problema.
4. Rode os testes EditMode — `ProgressionCurveTests.DocsSpreadsheet_MatchesShippedCurve` falha
   se a planilha e o asset ficarem dessincronizados.

Outros menus:
- **Rapadura > Balance > Export Progression CSV** — regrava a planilha a partir do asset.
- **Rapadura > Balance > Regenerate Progression Table From Formula** — recalcula a tabela pelos
  parâmetros de fórmula do asset (`_baseExperienceToLevel`, `_experienceCurveMultiplier`,
  `_maxHealthPerLevel`, ...). Útil para testar uma curva nova e depois exportar.
- **Rapadura > Balance > Export Enemy and Skill CSVs** — regenera `enemies.csv` e `skills.csv`.

Comportamento atual: o asset usa `_experienceCurveMultiplier` = **1,10** e `_maxHealthPerLevel` =
**8** (`round(100 × 1.10^(nível-1))`, vida 100 + 8 por nível, 2 pontos de atributo e 1 de skill por
nível, sem bônus de dano), e a tabela é exatamente a gerada por esses parâmetros
(`ProgressionCurveTests.ShippedResourcesCurve_TableMatchesItsOwnFormulaParameters`). Os padrões **do
código** (instância nova do ScriptableObject) continuam sendo a fórmula antiga (1,25, vida fixa) —
para reverter, basta voltar os dois parâmetros do asset, rodar *Regenerate Progression Table From
Formula* e *Export Progression CSV*. Acima do nível 50 (fim da tabela) a fórmula continua valendo. Se
o asset não existir, `PlayerStats`/`AttributeSet`/`SkillManager` usam os campos antigos do inspector
como fallback.

### Colunas derivadas de `skills.csv`

- `dot_damage` = soma dos ticks dos debuffs (`floor(duração / intervalo) × tickDamageOrHeal`).
  `hot_healing` idem para buffs.
- `dps_lv1` = (`base_damage` + `dot_damage`) / max(`cooldown`, `cast_time` + `recovery`).
- `damage_per_mana_lv1` = (`base_damage` + `dot_damage`) / `mana_cost`.
- `*_max` = o mesmo no nível máximo da skill (`base + damage_per_level × (max_level-1)`).
- Não inclui buffs de `AttackDamage`, crítico, cura nem efeitos de controle (CC).

---

## 2. Análise de sanidade — ANTES do rebalanceamento

> Esta seção é o diagnóstico original, mantido como referência do "antes". Os números atuais estão
> na §3.

Premissas — o jogo **não define** nível esperado por bioma, então foi derivado da economia de
XP: todo inimigo dá 10 XP; a missão do herói dá 450–520 XP (→ nível 4) e a de convergência
1200 XP (→ nível 8 somando as duas). Ordem dos biomas segue `BiomeType`:

| Bioma | Nível esperado (estimado) |
|---|---|
| Floresta | 1–3 |
| Deserto | 3–5 |
| Montanha | 5–7 |
| Cavernas | 7–9 |

Dano do jogador: ataque básico (`PlayerBasicAttack`) = **15 de dano a cada 0,5 s = 30 DPS**,
em área, sem custo. Ele **não escala** com nível, atributos nem buffs. Vida do jogador: 100,
também sem crescimento.

### 2.1 Tempo para matar (TTK), por bioma

TTK = tempo usando só o ataque básico (o primeiro golpe em t=0). "Mata o jogador" = 100 HP ÷
DPS de ataque do inimigo (`attack_damage / attack_cooldown`), sem contar dano de contato, i-frames
nem esquiva.

| Bioma | Inimigo | HP | Golpes | TTK | DPS do inimigo | Mata o jogador em |
|---|---|---|---|---|---|---|
| Floresta | Lobo da Mata | 30 | 2 | 0,5 s | 7,0 | 14,3 s |
| Floresta | Aranha da Floresta | 30 | 2 | 0,5 s | 7,0 | 14,3 s |
| Floresta | **Lobo Alfa (boss)** | 160 | 11 | 5,0 s | 20,0 | **5,0 s** |
| Deserto | Escorpião das Dunas | 26 | 2 | 0,5 s | 6,9 | 14,4 s |
| Deserto | **Guardião da Ruína (boss)** | 200 | 14 | 6,5 s | 14,7 | 6,8 s |
| Montanha | Golem de Pedra | 55 | 4 | 1,5 s | 8,1 | 12,3 s |
| Montanha | **Senhor da Guerra (boss)** | 220 | 15 | 7,0 s | 23,6 | **4,2 s** |
| Cavernas | Enxame de Morcegos | 18 | 2 | 0,5 s | 7,1 | 14,0 s |
| Cavernas | **Habitante Ancestral (boss)** | 180 | 12 | 5,5 s | 14,3 | 7,0 s |

Como nem o dano do jogador nem a vida dos inimigos escalam com o nível, **o TTK é o mesmo em
qualquer nível** — a tabela vale para todo o intervalo esperado de cada bioma. Somar o kit de
skills do herói ajuda pouco (melhor caso, Maithe: +16 DPS no nível 1 → Lobo Alfa em ~3,5 s).

Leituras:
- **Inimigos comuns não escalam entre biomas**: todos morrem em 2 golpes, exceto o Golem (4).
  O Escorpião do Deserto (26 HP) é mais fraco que o Lobo da Floresta (30 HP).
- **Bosses são cara-ou-coroa**: o Lobo Alfa e o Senhor da Guerra matam o jogador antes (ou no
  mesmo tempo) que o jogador os mata. O Senhor da Guerra (Montanha) é o pico de dificuldade.
- **XP por inimigo é fixo em 10, inclusive para bosses** (`_bossExperienceMultiplier` = 1 em
  todos). Matar um boss rende o mesmo que matar um morcego; o Enxame de Morcegos é o melhor
  XP/HP do jogo (0,56 contra 0,05 de um boss).

### 2.2 Curva de XP

| Nível | XP para o próximo | XP total para chegar | Inimigos (10 XP) |
|---|---|---|---|
| 5 | 244 | 576 | 58 |
| 10 | 745 | 2 579 | 258 |
| 20 | 6 939 | 27 354 | 2 736 |
| 30 | 64 623 | 258 094 | 25 810 |
| 50 | 5 605 194 | 22 420 374 | 2 242 038 |

O multiplicador 1,25 por nível é exponencial demais para um teto de 50 níveis: depois do ~15 a
progressão praticamente para. As ultimates (exigem nível 10) ficam atrás de ~258 abates se
o jogador não fizer as missões.

### 2.3 Skills de herói — DPS por mana

Entre as 25 skills de herói, 14 causam dano. Mediana: **1,23 de dano por mana** e **2,7 DPS**
no nível 1 (regeneração de mana: 5/s, mana máxima: 100).

| Herói | Skill | Dano/cast | Mana | CD | DPS | Dano/mana | × mediana |
|---|---|---|---|---|---|---|---|
| Maithe | **Flecha Precisa** | 28 | 10 | 3 s | **9,33** | **2,80** | **2,28×** ⬆ |
| Maithe | Disparo Explosivo | 47 (35 + 12 queimadura) | 25 | 10 s | 4,70 | 1,88 | 1,53× |
| Lavine | Orbe Sombria | 26 | 14 | 3,5 s | 7,43 | 1,86 | 1,51× |
| Ícaro | Drone de Ataque | 40 | 26 | 14 s | 2,86 | 1,54 | 1,25× |
| Ícaro | Torreta Automática | 34 | 24 | 12 s | 2,83 | 1,42 | 1,15× |
| Lavine | Explosão Arcana | 40 | 30 | 10 s | 4,00 | 1,33 | 1,08× |
| Ícaro | Granada Elétrica | 26 | 20 | 8 s | 3,25 | 1,30 | 1,06× |
| Ícaro | Exército Mecânico (ult) | 93 | 80 | 75 s | 1,24 | 1,16 | 0,94× |
| Joaquim | Terremoto | 30 | 28 | 12 s | 2,50 | 1,07 | 0,87× |
| Maria | Purificação | 15 | 18 | 10 s | 1,50 | 0,83 | 0,67× |
| Maithe | Chuva de Flechas | 18 | 22 | 8 s | 2,25 | 0,82 | 0,67× |
| Maithe | Tempestade de Mil Flechas (ult) | 55 | 70 | 60 s | 0,92 | 0,79 | 0,64× |
| Lavine | Eclipse Eterno (ult) | 65 | 90 | 70 s | 0,93 | 0,72 | 0,59× |
| Lavine | **Correntes das Trevas** | 12 | 22 | 11 s | 1,09 | **0,55** | **0,45×** ⬇ |

Pontos fora da curva:
- **Flecha Precisa** é o maior outlier: 2,3× a mediana em dano/mana, a maior DPS de herói, e
  gasta só 3,3 mana/s — menos que a regeneração (5/s) — então pode ser usada sem parar.
  Orbe Sombria (4 mana/s) também é praticamente infinita.
- **Ultimates** causam só 1,5–2× o dano de uma skill comum, mas custam 70–90 mana e têm
  CD de 60–90 s. A Tempestade de Mil Flechas (55 por 70 mana) causa menos que um Disparo
  Explosivo (47) mais uma Flecha Precisa (28), que juntos custam 35 mana.
- **Correntes das Trevas** e **Chuva de Flechas** ficam bem abaixo da mediana (a primeira é
  controle, então até é aceitável; a segunda é a skill de área da Maithe e perde para o
  Disparo Explosivo em tudo).
- Entre as gerais, a **Flecha (`skill_arrow`)** custa energia e zero mana: 12 DPS (é a skill
  mais forte de todas no nível 1). A Bola de Fogo (11,25 DPS) vem logo atrás.
- **Todas as skills perdem para o ataque básico** (30 DPS, grátis). Hoje, skill serve para
  utilidade/área, não para dano.

### 2.4 Atributos que não fazem nada (bug, não balanceamento) — **corrigido, ver §3.1**

`AttributeSet` aplica Vitalidade → `MaxHealth`, Espírito → `MaxMana` e Destreza →
`CriticalChance` no `BuffController`, mas **nenhum código lê esses três stats**: `PlayerStats`
usa sua própria vida/mana máxima e não existe cálculo de crítico. Só Inteligência
(`CooldownReduction`) e Poder Arcano (`AttackDamage`, e só para skills) têm efeito. Hoje, o
jogador que distribui seus 2 pontos por nível em Vitalidade/Espírito/Destreza desperdiça esses
pontos. O mesmo vale para o afixo `affix_vital` (MaxHealth) dos itens procedurais.

---

## 3. Rebalanceamento aplicado (antes → depois)

Tudo abaixo é **dado** (assets/planilha), exceto a correção do bug da §3.1 e a ligação do ataque
básico/crítico ao pipeline de dano. Para reverter um número, edite o asset indicado (e rode
*Export Enemy and Skill CSVs* / *Export Progression CSV*). As invariantes que importam (faixa de XP
até o nível 10, bosses vencíveis no nível esperado, XP por bioma, ultimates ≥ 2× a melhor skill...)
estão em `Assets/Tests/EditMode/BalanceInvariantsTests.cs` — elas testam faixas e ordens, não os
números exatos.

### 3.1 Bug dos atributos mortos (código)

- `PlayerStats.MaxHealth`/`MaxMana` agora passam por
  `BuffController.GetModifiedValue(StatType.MaxHealth/MaxMana, base + curva)`. Vitalidade, Espírito,
  equipamentos e o afixo `affix_vital` passam a valer. Quando o máximo muda (atributo, item equipado/
  removido, buff que expira — `BuffController.OnModifiersChanged`), a vida/mana atual é reconciliada:
  barra cheia continua cheia no novo máximo; barra parcial mantém o valor e só é **reduzida** se
  passar do novo máximo (sem cura grátis ao trocar de item).
- `BuffController.ApplyEffect` substituía qualquer efeito do mesmo stat, **de qualquer fonte**: equipar
  um item com `affix_vital` apagava o bônus de Vitalidade, e lançar um buff de `AttackDamage` apagava
  para sempre o de Poder Arcano. Agora só substitui efeitos da mesma fonte (`sourceKey`); fontes
  diferentes acumulam.
- **Crítico** (`Gameplay/Combat/OutgoingDamage.cs`), usado pelo ataque básico **e** pelas skills:
  `chance = base (0) + Σ CriticalChance` (Destreza = 0,5%/ponto; afixos de crítico contam), um
  sorteio por golpe/cast, `multiplicador = 1,5 + Σ CriticalDamage`. O RNG é injetável
  (`PlayerBasicAttack.CriticalRoll`, `SkillManager.CriticalRoll`). O golpe crítico sai com
  `DamageAppliedEvent.IsCritical = true` (FX/áudio de crítico já existentes) sem multiplicar duas vezes
  no `DamageCalculator`. Maithe (5 de Destreza inata) começa com 2,5%.
- Base de crítico e multiplicador: `PlayerCombatBalance.asset` (`_baseCriticalChance` = 0,
  `_baseCriticalMultiplier` = 1,5).

### 3.2 Tabela de mudanças

| Onde | Campo | Antes | Depois |
|---|---|---|---|
| `ProgressionCurve.asset` | `_experienceCurveMultiplier` | 1,25 | **1,10** |
| `ProgressionCurve.asset` | `_maxHealthPerLevel` (coluna `hp`) | 0 | **8** |
| `PlayerCombatBalance.asset` (novo) | dano do ataque básico | 15 (Inspector da cena) | **10**, e agora passa por `AttackDamage`, `damage_mult` e crítico |
| Lobo da Mata / Aranha (Floresta) | XP | 10 | 10 |
| Escorpião das Dunas (Deserto) | HP / XP | 26 / 10 | **45 / 18** |
| Golem de Pedra (Montanha) | HP / XP | 55 / 10 | **75 / 28** |
| Enxame de Morcegos (Cavernas) | HP / XP | 18 / 10 | **28 / 40** |
| 4 bosses | `_bossExperienceMultiplier` | 1 | **8** (XP base = a dos comuns do bioma: 80 / 144 / 224 / 320) |
| Lobo Alfa (boss Floresta) | HP / `_attackCooldown` | 160 / 0,9 | **120 / 1,1** |
| Guardião da Ruína (boss Deserto) | HP | 200 | **160** |
| Senhor da Guerra (boss Montanha) | HP / dano / cooldown | 220 / 26 / 1,1 | **170 / 20 / 1,3** |
| Habitante Ancestral (boss Cavernas) | HP | 180 | **200** |
| Flecha Precisa (Maithe) | mana | 10 | **15** |
| Orbe Sombria (Lavine) | cooldown | 3,5 s | **4 s** |
| Chuva de Flechas (Maithe) | dano base / por nível | 18 / 4 | **26 / 6** |
| Correntes das Trevas (Lavine) | mana | 22 | **16** |
| Tempestade de Mil Flechas (ult) | dano base / por nível | 55 / 14 | **130 / 32** |
| Eclipse Eterno (ult) | dano base / por nível | 65 / 16 | **150 / 36** |
| Exército Mecânico (ult) | dano base / por nível | 45 / 12 | **100 / 26** |
| Flecha (`skill_arrow`, geral) | mana | 0 | **6** (mantém 8 de energia) |

Os HP/dano dos inimigos também foram atualizados no `EnemyDatabaseSeeder` (e `skill_arrow` no
`SkillDatabaseSeeder`), para que rodar os seeders de novo não desfaça o rebalanceamento.

Por que os bosses perderam HP mesmo com "biomas mais difíceis": o ataque básico caiu de 30 para
20 DPS, então o tempo para matar cada boss **subiu** (5–7 s → 5,5–9,5 s), crescendo de bioma em
bioma, enquanto cada boss ainda perde a corrida de DPS para um jogador do nível esperado.

### 3.3 Análise depois das mudanças

Mesmas premissas da §2 (só ataque básico, primeiro golpe em t=0, sem esquiva/dano de contato).
Ataque básico = **10 a cada 0,5 s = 20 DPS**. Vida do jogador no meio da faixa esperada de cada
bioma: Floresta nível 2 = 108, Deserto nível 4 = 124, Montanha nível 6 = 140, Cavernas nível 8 = 156.

| Bioma | Inimigo | HP antes → depois | Golpes | TTK antes → depois | DPS do inimigo | Mata o jogador em (antes, 100 HP → depois) |
|---|---|---|---|---|---|---|
| Floresta | Lobo da Mata / Aranha | 30 → 30 | 3 | 0,5 → 1,0 s | 7,0 | 14,3 → 15,4 s |
| Floresta | **Lobo Alfa (boss)** | 160 → 120 | 12 | 5,0 → **5,5 s** | 20,0 → 16,4 | 5,0 → **6,6 s** |
| Deserto | Escorpião das Dunas | 26 → 45 | 5 | 0,5 → 2,0 s | 6,9 | 14,4 → 17,9 s |
| Deserto | **Guardião da Ruína (boss)** | 200 → 160 | 16 | 6,5 → **7,5 s** | 14,7 | 6,8 → **8,5 s** |
| Montanha | Golem de Pedra | 55 → 75 | 8 | 1,5 → 3,5 s | 8,1 | 12,3 → 17,2 s |
| Montanha | **Senhor da Guerra (boss)** | 220 → 170 | 17 | 7,0 → **8,0 s** | 23,6 → 15,4 | 4,2 → **9,1 s** |
| Cavernas | Enxame de Morcegos | 18 → 28 | 3 | 0,5 → 1,0 s | 7,1 | 14,0 → 21,8 s (vêm em bando) |
| Cavernas | **Habitante Ancestral (boss)** | 180 → 200 | 20 | 5,5 → **9,5 s** | 14,3 | 7,0 → **10,9 s** |

Leituras:
- Os comuns pedem 3 / 5 / 8 / 3 golpes (morcegos compensam em número), em vez de 2 em quase todo
  bioma.
- Todo boss agora é vencível só no ataque básico no nível esperado, com margem de 13–20%
  (Senhor da Guerra: 8,0 s contra 9,1 s). Antes, Lobo Alfa e Senhor da Guerra matavam primeiro.
  Skills, crítico, Poder Arcano e esquiva aumentam essa margem.
- O ataque básico agora escala: +1% por ponto de Poder Arcano, Berserk/combos (`AttackDamage`),
  coluna `damage_mult` e crítico (Destreza).

**Curva de XP** (multiplicador 1,10):

| Nível | XP para o próximo (antes → depois) | XP total para chegar (antes → depois) | Inimigos de 10 XP | Inimigos de 28 XP |
|---|---|---|---|---|
| 5 | 244 → 146 | 576 → 464 | 47 | 17 |
| 10 | 745 → 236 | 2 579 → **1 357** | 136 | 49 |
| 20 | 6 939 → 612 | 27 354 → 5 114 | 512 | 183 |
| 30 | 64 623 → 1 586 | 258 094 → 14 861 | 1 487 | 531 |
| 50 | 5 605 194 → 10 672 | 22 420 374 → **105 715** | 10 572 | 3 776 |

Com XP por bioma, avançar dois níveis em cada bioma custa: Floresta 1→3 (210 XP) ≈ 13 comuns + o boss
(80); Deserto 3→5 (254) ≈ 6 comuns + boss (144); Montanha 5→7 (307) ≈ 3 comuns + boss (224); Cavernas
7→9 (372) ≈ 2 comuns + boss (320). As missões de herói (450–520 XP) e de convergência (1 200) adiantam
o jogador — o que só deixa os bosses mais fáceis que o "nível esperado" da tabela acima. Ultimates
(nível 10) chegam com ~1 357 XP em vez de ~2 579.

**Skills de herói — DPS por mana** (nível 1; mediana dano/mana **1,23 → 1,48**, DPS 2,7 → 2,8):

| Herói | Skill | Dano/cast | Mana | CD | DPS | Dano/mana (antes → depois) | × mediana | Mana/s |
|---|---|---|---|---|---|---|---|---|
| Maithe | Disparo Explosivo | 47 | 25 | 10 s | 4,70 | 1,88 | 1,27× | 2,5 |
| Maithe | Flecha Precisa | 28 | 10 → **15** | 3 s | 9,33 | 2,80 → **1,87** | 1,26× | 3,3 → **5,0** (= regeneração) |
| Lavine | Orbe Sombria | 26 | 14 | 3,5 → **4 s** | 7,43 → **6,50** | 1,86 | 1,26× | 4,0 → 3,5 |
| Maithe | Tempestade de Mil Flechas (ult) | 55 → **130** | 70 | 60 s | 0,92 → **2,17** | 0,79 → **1,86** | 1,26× | 1,2 |
| Ícaro | Exército Mecânico (ult) | 93 → **148** | 80 | 75 s | 1,24 → **1,97** | 1,16 → **1,85** | 1,25× | 1,1 |
| Lavine | Eclipse Eterno (ult) | 65 → **150** | 90 | 70 s | 0,93 → **2,14** | 0,72 → **1,67** | 1,13× | 1,3 |
| Ícaro | Drone de Ataque | 40 | 26 | 14 s | 2,86 | 1,54 | 1,04× | 1,9 |
| Ícaro | Torreta Automática | 34 | 24 | 12 s | 2,83 | 1,42 | 0,96× | 2,0 |
| Lavine | Explosão Arcana | 40 | 30 | 10 s | 4,00 | 1,33 | 0,90× | 3,0 |
| Ícaro | Granada Elétrica | 26 | 20 | 8 s | 3,25 | 1,30 | 0,88× | 2,5 |
| Maithe | Chuva de Flechas | 18 → **26** | 22 | 8 s | 2,25 → **3,25** | 0,82 → **1,18** | 0,80× | 2,75 |
| Joaquim | Terremoto | 30 | 28 | 12 s | 2,50 | 1,07 | 0,72× | 2,3 |
| Maria | Purificação | 15 | 18 | 10 s | 1,50 | 0,83 | 0,56× | 1,8 |
| Lavine | Correntes das Trevas (controle) | 12 | 22 → **16** | 11 s | 1,09 | 0,55 → **0,75** | 0,51× | 1,45 |

- Nenhuma skill de herói passa de 1,3× a mediana em dano/mana (antes: Flecha Precisa a 2,3×).
- Cada ultimate de dano agora bate ~3,7× a melhor skill ativa do herói por cast (antes, ~1,5×).
- `skill_arrow` (geral): 6 de mana + 8 de energia, 3,0 de dano/mana (4 mana/s). Ainda é a skill de
  maior DPS bruto (12), mas deixou de ser grátis.
- O ataque básico (20 DPS) continua acima de qualquer skill isolada, mas a diferença caiu de ~11× a
  mediana de DPS de skill de herói para ~7×; skill + básico é o que leva os bosses em tempo razoável.

### 3.4 Ainda não aplicado (fica para a próxima rodada)

- `skill_meteor_shower` (ultimate geral, 60 de dano/80 de mana) não entrou no ×2,5 das ultimates de
  herói.
- `damage_mult` continua 1 em todos os níveis; +2%/nível é a próxima alavanca se o fim do jogo ficar
  lento (os bosses ficam mais fáceis conforme o jogador passa do nível esperado, só pela vida).
- Purificação (Maria) e Terremoto (Joaquim) seguem abaixo da mediana; são skills de suporte/controle.

---

## 4. Defesa, velocidade e stats mortos (bug, não balanceamento)

Até aqui **nenhum código lia `StatType.Defense` nem `StatType.MoveSpeed`**, e os inimigos nem tinham
`BuffController` — então ~20 skills (Campo Magnético, Granada, Drone, Escudo das Raízes, Muralha,
Provocação, Última Resistência, Terremoto, Escudo Sagrado, Milagre, Correntes, Eclipse, Névoa, Chuva de
Flechas, Passo do Vento, Tempestade, shield, thorns, berserk, dash, freeze, ice_shard, invisibility)
tinham metade do efeito (ou todo) jogado fora. **Todo debuff em inimigo** (lentidão, defesa, dano e as
DoTs de Drone/Torreta/Exército/Disparo Explosivo/fireball/poison) era descartado.

### 4.1 Fórmula de defesa (`DamageCalculator`, jogador e inimigos)

Os dados usam Defense de dois jeitos, e cada um tem seu termo:

- **Flat** (só os afixos `affix_sturdy` +2..5 e `affix_reinforced` +6..10, mais `Health._defense` /
  `PlayerStats._baseDefense`, hoje 0 em todo lugar) = pontos de armadura na curva que já existia:
  `armadura = 1 − def / (def + 100)` (100 pontos = metade do dano).
- **PercentAdditive** (todas as skills — teste `EverySkillExpressesDefenseAndMoveSpeedAsPercent`) =
  mudança direta do dano recebido: `+0,25` = 25% a menos, `−0,2` = 20% a mais. As fontes somam e a
  soma é limitada a **[−1; +0,8]**: buffs empilhados nunca passam de 80% de redução (Última
  Resistência sozinha já chega lá), debuffs empilhados no máximo **dobram** o dano.

```
dano_recebido = dano × (1 − def/(def+100)) × (1 − clamp(Σ%Defense, −1, 0,8))
```

- Inimigos/`Health`: dentro de `ComputeDamage` (depois vem resistência elemental, crítico, arredondamento
  e dano mínimo 1, como antes).
- Jogador/`PlayerStats`: em `ApplyDamageInternal` (todas as entradas de dano), **sem** arredondar nem
  dano mínimo — sem defesa o multiplicador é exatamente 1 e nada muda.

Exemplos: Campo Magnético → jogador recebe ×0,75; Muralha ×0,4; Última Resistência ×0,2; Drone no
inimigo → ×1,2; Eclipse + Correntes + Drone → ×1,65; Berserk "Exposto" → ×1,3 nos alvos.

### 4.2 Velocidade (`BuffController.GetMoveSpeedMultiplier`)

`mult = GetModifiedValue(MoveSpeed, 1)` limitado a **[0,1; 2,5]**; aplicado a toda movimentação do
`PlayerMotor` (andar/correr/agachar/slide/clique-para-mover/auto-run) e a patrulha/perseguição/fuga do
`EnemyController` (`MoveInDirection`). O "Speed" do Animator (via `HorizontalVelocity`) e o
`EnemyProceduralAnimator` (deslocamento real) seguem a velocidade efetiva.

- 0,1 é o "Congelado" do `skill_freeze` (−90%); 2,5 é o dash (+150%).
- **Enraizado** (`IsRooted`, mult 0): só quando **um único** efeito tem MoveSpeed ≤ −100%. Lentidões
  parciais empilhadas (Correntes −70% + Eclipse −50%) param em 0,1 e não prendem o boss para sempre.
  Enraizado ainda gira no lugar. Nenhuma skill atual enraíza.

### 4.3 Outros stats ligados nesta passada

| Stat | Antes | Agora |
|---|---|---|
| `AttackDamage` (inimigo) | debuffs de Provocação −25%, Névoa −30%, Purificação −20% ignorados | `EnemyController.EffectiveAttackDamage` no golpe |
| `HealthRegen` / `ManaRegen` / `StaminaRegen` | afixo `affix_of_vigor` (+0,5..1,5 HP/s) ignorado | `PlayerStats.*RegenPerSecond` (as DoT/HoT usam HealthRegen com valor 0 e não mudam a regen) |
| `MaxStamina` | afixo `affix_of_stamina` (+8..18) ignorado | `PlayerStats.MaxStamina` |

`EnemyController.Awake` agora adiciona um `BuffController` (sem editar prefab/cena) e `ResetForSpawn`
limpa efeitos/stun da vida anterior (pool).

### 4.4 Impacto no balanceamento

As invariantes de `BalanceInvariantsTests` (corrida de DPS só com ataque básico, sem skills) **não
mudam**: inimigos e jogador continuam com defesa base 0, então o multiplicador é 1 nesse modelo.
Tudo o que entrou é margem extra a favor do jogador — buffs de defesa reduzem o DPS dos bosses
(Senhor da Guerra 15,4 → 11,5 DPS sob Campo Magnético, → 6,2 sob Muralha), debuffs de defesa e as DoTs
que agora funcionam aceleram o TTK, lentidões dão espaço para fugir/kitar. Nenhum dado foi alterado.
Se os bosses ficarem fáceis demais no playtest, as alavancas são: o teto de 0,8 (`DamageCalculator.MaxPercentDefense`)
e o piso de −1 (`MinPercentDefense`), o 0,1 de `MinMoveSpeedMultiplier`, ou `_defense` base nos bosses.

Depois de qualquer mudança em inimigos/skills: **Rapadura > Balance > Export Enemy and Skill
CSVs** e confira os números de novo nesta análise (e rode `BalanceInvariantsTests`).
