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
| `damage_mult` | Multiplicador de dano por nível do jogador (1 = sem bônus) | `SkillManager` (skills) e `PlayerBasicAttack` (ataque básico) |
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
nível e, desde o r6 (§5), `_damageMultiplierPerLevel` = **0,02** — `damage_mult` = 1 + 0,02 × (nível-1)),
e a tabela é exatamente a gerada por esses parâmetros
(`ProgressionCurveTests.ShippedResourcesCurve_TableMatchesItsOwnFormulaParameters`). Os padrões **do
código** (instância nova do ScriptableObject) continuam sendo a fórmula antiga (1,25, vida fixa) —
para reverter, basta voltar esses parâmetros do asset, rodar *Regenerate Progression Table From
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
- `stun_seconds` = `SkillDefinition._stunDurationSeconds` (atordoamento por alvo atingido, antes da
  redução de boss; ver §7.1).

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

---

## 5. r6 → rebalanceamento (combate fácil demais depois das correções)

> As §2–§4 ficam como histórico. Os exemplos da §4 que citam a Muralha (×0,4, "→ 6,2 DPS sob
> Muralha") usam o valor antigo de 0,6; desde o r6 a Muralha dá ×0,5 (ver abaixo).

### 5.1 O que o playtest r6 mediu

Com Defesa/Velocidade/debuffs funcionando (§4) e as skills deixando de ser engolidas, o playtest
automatizado r6 (5 heróis × 4 biomas, níveis 2/5/8/11, luta normal = 2 comuns, luta de boss = boss + 1
lacaio) mostrou o combate **fácil demais**:

| Bioma | Luta | Tempo médio r5 → r6 | Vida perdida média r5 → r6 | Pior vida mínima r6 | Mortes |
|---|---|---|---|---|---|
| Floresta | normal / boss | 7,5 → 2,1 s / 10,1 → **4,0 s** | 9% → 3% / 67% → 28% | 94% / 68% | 0/10 |
| Deserto | normal / boss | 26,6 → 3,7 s / 18,9 → **7,5 s** | 10% → 4% / 78% → 42% | 95% / 56% | 0/10 |
| Montanha | normal / boss | 9,2 → 5,5 s / 14,0 → **8,4 s** | 30% → 21% / 87% → 59% | 74% / 43% | 0/10 |
| Cavernas | normal / boss | 7,3 → 2,0 s / 13,4 → **7,0 s** | 7% → 0% / 63% → 32% | 100% / 68% | 0/10 |

Bosses caíam em 3–11,5 s, as lutas normais duravam 2–5,5 s em média e ninguém morreu: as skills
defensivas (Muralha, Escudo, curas) não tinham o que defender.

### 5.2 Mudanças aplicadas (antes → depois)

| Onde | Campo | Antes | Depois |
|---|---|---|---|
| Lobo da Mata / Aranha da Floresta | HP | 30 | **70** |
| Escorpião das Dunas | HP | 45 | **85** |
| Golem de Pedra | HP | 75 | **100** |
| Enxame de Morcegos | HP / dano | 28 / 5 | **80 / 7** |
| Lobo Alfa (boss Floresta) | HP / `_attackCooldown` | 120 / 1,1 | **430 / 2,4** |
| Guardião da Ruína (boss Deserto) | HP / `_attackCooldown` | 160 / 1,5 | **505 / 3,3** |
| Senhor da Guerra (boss Montanha) | HP / dano / `_attackCooldown` | 170 / 20 / 1,3 | **580 / 16 / 2,6** |
| Habitante Ancestral (boss Cavernas) | HP / dano / `_attackCooldown` | 200 / 20 / 1,4 | **700 / 18 / 3,0** |
| Purificação (Maria) | dano base / por nível | 15 / 4 | **24 / 5** |
| Muralha de Pedra (Joaquim) | buff de Defense / cooldown | 0,6 / 12 s | **0,5 / 16 s** |
| `ProgressionCurve.asset` | `_damageMultiplierPerLevel` (coluna `damage_mult`) | 0 (1,00 em todo nível) | **0,02** (1,02 no nível 2 … 1,18 no 10 … 1,98 no 50) |

Os HP/dano/cooldown dos inimigos também estão no `EnemyDatabaseSeeder` (a Aranha não é semeada por
ele) e no espelho de `EnemyDatabaseSeederTests`. As skills de herói não têm seeder (só os assets).
`enemies.csv`, `skills.csv` e `progression.csv` foram regenerados com as mesmas colunas/fórmulas do
`BalanceCsvMenu`; a tabela do `ProgressionCurve.asset` continua sendo exatamente a gerada pelos
parâmetros (`ShippedResourcesCurve_TableMatchesItsOwnFormulaParameters`) e igual à planilha
(`DocsSpreadsheet_MatchesShippedCurve`).

Por quê:
- **Bosses: ~3,2–3,6× o HP, menos da metade do DPS.** O boss precisa durar o bastante para o kit inteiro
  (incluindo as skills de 10–16 s de cooldown) entrar em jogo. Cortar o DPS pela metade, sobretudo
  pelo cooldown, deixa cada golpe ainda pesado (16–22) mas legível e esquivável, e mantém o tempo
  para o boss matar o jogador acima de ~14 s.
- **Comuns: +33–186% de HP.** Deixam de morrer numa única rotação de skills; os morcegos (que vêm em
  bando) ganham 7 de dano para continuarem uma ameaça.
- **Purificação 15 → 24.** Era a pior skill de dano por mana entre as de herói (0,83); agora 1,33,
  perto da mediana, para a Maria não ficar só no ataque básico (87% do dano dela no r6).
- **Muralha 0,6/12 s → 0,5/16 s.** O Joaquim passou 100% das lutas sob Muralha no r6 (uptime de
  5 s a cada 12 s com cast curto, mais a Última Resistência). Com 16 s o uptime cai para ~31% e a
  Muralha passa a ser uma resposta aos golpes do boss, não um estado permanente.
- **`damage_mult` +2%/nível** (a alavanca da §3.4). Com bosses de 430–700 HP, sem crescimento de
  dano o fim do jogo ficaria lento: no nível 10 o jogador bate 18% mais forte e, no 50, 98% mais.
  Vale para ataque básico e skills (`PlayerBasicAttack`, `SkillManager`).

### 5.3 Análise depois das mudanças

Mesmo modelo da §3.3 (nível esperado 2/4/6/8, sem esquiva/crítico/dano de contato), agora com o kit:
ataque básico = 10 × `damage_mult` a cada 0,5 s; kit = Σ (dano + DoT) / max(cooldown, cast +
recuperação) das skills ativas de dano do herói, limitado pela mana de uma luta de 20 s (100 + 5/s) —
Maithe 16,9 DPS, Lavine 11,6, Ícaro 8,9, Joaquim 2,5, Maria 1,5 → 2,4 (×`damage_mult`).

| Bioma (nível) | HP do boss | DPS do boss | Boss mata o jogador em | Só ataque básico | Com o kit (pior–melhor herói) | Jogador sob Muralha morre em |
|---|---|---|---|---|---|---|
| Floresta (2) | 120 → **430** | 16,4 → 7,5 | 6,6 → **14,4 s** | 6,0 → 21,1 s | 3,3–5,6 → **11,4–18,8 s** | 16,5 → 28,8 s |
| Deserto (4) | 160 → **505** | 14,7 → 6,7 | 8,5 → **18,6 s** | 8,0 → 23,8 s | 4,3–7,4 → **12,9–21,3 s** | 21,1 → 37,2 s |
| Montanha (6) | 170 → **580** | 15,4 → 6,2 | 9,1 → **22,8 s** | 8,5 → 26,4 s | 4,6–7,9 → **14,3–23,5 s** | 22,8 → 45,5 s |
| Cavernas (8) | 200 → **700** | 14,3 → 6,0 | 10,9 → **26,0 s** | 10,0 → 30,7 s | 5,4–9,3 → **16,7–27,4 s** | 27,3 → 52,0 s |

Comuns (só ataque básico → com o melhor kit): Lobo/Aranha 1,5 → **3,4 s** (0,8 → 1,9 s), Escorpião
2,2 → **4,0 s** (1,2 → 2,2 s), Golem 3,8 → **4,5 s** (2,0 → 2,5 s), Morcegos 1,4 → **3,5 s**
(0,8 → 1,9 s).

Leituras:
- **Só no ataque básico, todo boss vence** (21–31 s para matá-lo contra 14–26 s para ele matar o
  jogador): sem skills nem defesa existe risco real de morte. Antes do r6 era o contrário.
- **Com o kit, uma luta de boss dura ~11–27 s.** Maithe/Lavine (dano) ganham a corrida sem defender;
  Joaquim e Maria (18,8–27,4 s) perdem a corrida pura e dependem de Muralha/Escudo/curas/esquiva —
  com a redução da Muralha qualquer herói vence com folga.
- No jogo real os tempos são maiores que o modelo (deslocamento, golpes que erram, lacaio junto), e o
  playtest r6 rodou em níveis acima do esperado (5/8/11 em vez de 4/6/8); rode o playtest de novo
  para conferir.
- A Floresta é o boss mais agressivo em relação à vida do jogador (14,4 s para matar um jogador de
  nível 2 parado, abaixo da meta de ~20 s): é o primeiro boss e o jogador ainda está aprendendo a
  esquivar. Se ficar duro demais, a alavanca é `_attackCooldown` do Lobo Alfa (2,4 → 3,0 dá 18 s).

### 5.4 Invariantes (`BalanceInvariantsTests`)

A antiga "corrida de DPS só com ataque básico, o jogador tem que vencer" foi substituída (com os
números do r6 ela falharia de propósito: o boss **deve** vencer quem só usa o ataque básico). Agora,
sempre em faixas:

| Teste | Faixa / regra |
|---|---|
| `BossHealth_IsWithinItsBiomeBand` | Floresta 350–550, Deserto 400–650, Montanha 450–750, Cavernas 550–900 |
| `Boss_KillsAnUndefendedPlayerInAFewRotations_NotInstantly` | vida do jogador no nível esperado ÷ DPS do boss em 12–40 s |
| `Boss_WithTheFullKit_IsATenToThirtyFiveSecondFight_ForEveryHero` | HP do boss ÷ (básico + kit) em 10–35 s, para os 5 heróis |
| `Boss_OutlastsABasicAttackOnlyPlayer_SoSkillsAndDefenseMatter` | só no ataque básico o jogador precisa de > 1,05× o tempo que o boss leva para matá-lo |
| `Boss_IsWinnableByEveryHero_WithDefensivePlay` | com a redução de dano da Muralha (lida do asset), todo herói vence com ≥ 5% de margem |
| `Boss_CanBeOutDamagedWithoutDefense_ByTheBestDamageHero` | o herói de maior DPS vence a corrida sem defender |
| `CommonEnemies_TakeSeveralSwings_AndAreNeverOneShotByTheKit` | comuns: 2,5–8 s só no básico, ≥ 1,5 s com o melhor kit |

Continuam valendo: XP até o nível 10/50, vida crescendo por nível, XP por bioma, XP de boss ≥ 5× a
dos comuns, HP de boss não cai entre biomas, DPS do Senhor da Guerra < 20, ultimates ≥ 2× a melhor
ativa e ataque básico < 30 DPS.

### 5.5 Bug corrigido junto: XP perdido logo depois de entrar na Main

O `GameManager` aplica o estado carregado entre cenas (`SaveManager.RestoreCarriedState`) **um frame
depois** do `sceneLoaded`, para os `Start` da cena registrarem os componentes do jogador. XP ganho
nesse intervalo (um abate ou recompensa de missão nos primeiros frames) era apagado: o
`PlayerStats.RestoreState` sobrescrevia nível/XP com o estado da cena anterior. Agora o `PlayerStats`
guarda o XP recebido enquanto ainda existe estado carregado pendente para ele e o reaplica por cima
do estado restaurado (subindo de nível a partir do nível restaurado). Um `Load` de slot continua
deixando o arquivo vencer. Testes: `CarriedStateExperienceTests`.

## 6. r7 — ajuste fino

### 6.1 O que o playtest r7 mediu

Mesmo protocolo do r6 (5 heróis × 4 biomas), já com os números da §5:

- **Lutas normais: 4,3–7,4 s** em média; **bosses: 15–22 s**. As duas faixas batem com a meta da §5.3.
- **Floresta matou heróis frágeis**: o Lobo Alfa (18 de dano a cada 2,4 s = 7,5 DPS) derrubava um
  jogador de nível 2 (108 HP) em ~14 s, rápido demais para o primeiro boss (a alavanca já prevista na §5.3).
- **Cavernas: os morcegos não conseguiam atacar.** O Enxame paira ~0,92 m acima do herói; o teste de
  alcance usava distância 3D, que nunca ficava abaixo de ~1,2 m, então com `_attackRange` 1,1 o
  inimigo orbitava sem nunca entrar no estado de ataque (bug, não balanceamento).

### 6.2 Mudanças aplicadas (antes → depois)

| Onde | Campo | Antes | Depois |
|---|---|---|---|
| `EnemyDetectionUtility.IsWithinAttackRange` (código) | teste de alcance | distância 3D | **distância horizontal (XZ)** com tolerância vertical de 2 m (`AttackVerticalTolerance`) |
| Enxame de Morcegos | `_attackRange` | 1,1 | **1,5** |
| Lobo Alfa (boss Floresta) | `_attackDamage` | 18 | **14** (DPS 7,5 → 5,83) |
| Escorpião das Dunas | `_attackDamage` | 9 | **11** (DPS 6,92 → 8,46) |
| Cura Divina (Maria) | `_cooldownSeconds` | 6 | **9** |

Por quê:
- **Alcance horizontal.** Inimigos de chão se comportam igual (mesma altura = mesma distância); quem
  voa/paira passa a atacar independente da altura. A tolerância vertical evita golpes através de
  andares/plataformas muito acima ou abaixo. O alcance 1,5 dos morcegos fica como dado para o enxame
  não precisar encostar no herói.
- **Lobo Alfa 18 → 14.** O boss passa a matar um jogador parado de nível 2 em **18,5 s** (era 14,4 s),
  perto da meta de ~20 s; sob Muralha, 37,0 s. Só no ataque básico o jogador ainda precisa de 21,1 s,
  então o boss continua vencendo quem não usa skills/defesa (21,1 > 1,05 × 18,5 = 19,4).
- **Escorpião 9 → 11.** Recomendação do r7: mais pressão no comum do Deserto, para a dificuldade
  não afrouxar depois de a Floresta ser aliviada.
- **Cura Divina 6 → 9 s.** Recomendação do r7: com as lutas agora mais longas, a cura da Maria em
  cooldown curto vira um estado permanente; com 9 s ela continua forte, mas passa a ser uma decisão
  de tempo junto do Escudo Sagrado.

Espelhos atualizados: `EnemyDatabaseSeeder` e `EnemyDatabaseSeederTests` (as skills de herói não têm
seeder). `enemies.csv` e `skills.csv` regenerados com as fórmulas do `BalanceCsvMenu`.

### 6.3 Invariantes

Recalculado à mão o modelo do `BalanceInvariantsTests` (§5.3/§5.4): nenhuma faixa muda. O único boss
alterado é o Lobo Alfa — 18,5 s para matar o jogador (faixa 12–40 s), 21,1 s só no básico (> 19,4 s),
11,4–18,8 s com o kit (faixa 10–35 s, inalterado), todo herói vence sob Muralha (19,8 s < 37,0 s) e a
Maithe vence sem defender (11,4 s < 18,5 s). Escorpião e morcegos só entram nas regras de HP dos comuns
(inalteradas); a Cura Divina não causa dano, então não entra no kit. Teste novo de código:
`EnemyDetectionUtilityTests.IsWithinAttackRange_FlyingEnemyHoveringAboveTarget_StillInRange` e
`IsWithinAttackRange_FalseWhenHorizontallyTooFar_OrBeyondVerticalTolerance`.

---

## 7. Atordoamento, velocidade de ataque, espinhos e dano de contato (2026-10-29)

Lacunas de combate fechadas (código + dados). Nenhum número de inimigo mudou, então as invariantes
da §5.4/§6.3 continuam valendo: atordoamento, velocidade de ataque e espinhos são margem extra para o
jogador, e o dano de contato removido nunca tinha sido aplicado.

### 7.1 Atordoamento (`StunRules`, `BuffController.ApplyStun`)

```
aplicado = skill._stunDurationSeconds × (boss ? 0,5 : 1)      // nunca encurta um stun em andamento
```

- Inimigo atordoado (`EnemyController.TickStunned`): não anda (parada seca, sem deslizar), não gira,
  não ataca e **cancela o golpe em preparação**. Ele sai da postura de ataque, então, quando o stun
  acaba, o próximo golpe telegrafa a preparação inteira de novo, em vez de bater na hora com o tempo
  que sobrou.
- Bosses levam 50% (`StunRules.BossStunDurationMultiplier`, via `IStunDurationModifier`): com 3 heróis
  atordoando não dá para travar um boss em cadeia.
- Um golpe que mata não atordoa o corpo. Sinal visual: faíscas azul-claras + "Atordoado!" flutuando
  (`StunAppliedEvent` → `CombatFxManager`).

| Skill | Herói | Recarga | Stun (comum) | Stun (boss) | % do ciclo travado (comum) |
|---|---|---|---|---|---|
| Correntes das Trevas | Lavine | 11 s | **1,0 s** | 0,5 s | 9% |
| Terremoto | Joaquim | 12 s | **0,8 s** | 0,4 s | 7% |
| Granada Elétrica | Ícaro | 8 s | **0,6 s** | 0,3 s | 8% |

Lavine (controle de multidão) tem o stun mais longo; a Granada, que tem a menor recarga, tem o mais curto.
Num boss com `attack_cooldown` de 1,5–3 s, 0,3–0,5 s de stun atrasa no máximo um golpe por conjuração.

### 7.2 Velocidade de ataque (`StatType.AttackSpeed`, `PlayerBasicAttack.ResolveCooldown`)

```
bônus    = clamp(Σ AttackSpeed, -0,5, +1,0)      // Flat e PercentAdditive somam igual (0,1 = +10%)
recarga  = max(0,5 s / (1 + bônus), golpe_pendente)
```

O piso no golpe pendente (0,22 s sem clipe, ou o `AttackStrikeDelay` do clipe) impede começar um
golpe novo antes de o anterior acertar. Sem ele, o golpe pendente seria sobrescrito e perdido, e
atacar mais rápido daria **menos** dano. Com o básico de 15 de dano (30 DPS a cada 0,5 s): +10% →
0,455 s (33 DPS); +20% (teto do `affix_of_frenzy`) → 0,417 s (36 DPS). Ágil + Frenesi na mesma arma
chegam a +32% → 0,379 s (39,6 DPS). O teto de +100% (0,25 s) fica como trava de segurança.

### 7.3 Espinhos (`StatType.Thorns`, `ThornsRules`)

```
fração   = clamp(Σ Thorns, 0, 0,5)
refletido = dano_tomado_após_defesa × fração     // volta para quem bateu, no canal "thorns"
```

- Só golpe corpo a corpo: `EnemyController.ApplyDamageToTarget` (todo ataque de inimigo é corpo a corpo
  hoje) chama `IMeleeDamageTarget.ApplyMeleeDamage` no herói. Dano de outra origem (DoT, skill) não reflete.
- Sem recursão: o refletido entra por `OutgoingDamage.DeliverDamage` (nunca é "corpo a corpo"), e
  `ThornsRules.IsReflecting` ignora qualquer espinho disparado durante uma reflexão.
- O refletido passa pela defesa do inimigo (`Health`), credita o herói (XP/loot se matar) e usa canal
  próprio, então não engole o próximo ataque básico no mesmo inimigo.
- Exemplo: o Senhor da Guerra (16 de dano) num herói com Espinhosa 10% leva 1,6 por golpe. Com o teto de 50% ele leva 8.

### 7.4 Afixos novos (`AffixDatabaseSeeder`, 16 → 20)

| Afixo | Tipo | Slot | Stat | Faixa | Peso |
|---|---|---|---|---|---|
| `affix_nimble` (Ágil / Nimble) | prefixo | Mão principal | AttackSpeed | +6–12% | 45 |
| `affix_of_frenzy` (do Frenesi / of Frenzy) | sufixo | Mão principal | AttackSpeed | +13–20% | 12 |
| `affix_thorned` (Espinhosa / Thorned) | prefixo | Peito | Thorns | 5–10% | 40 |
| `affix_of_the_mandacaru` (do Mandacaru / of the Mandacaru) | sufixo | Peito | Thorns | 11–18% | 12 |

Os assets já estão em `Resources/Affixes` (gerados com os mesmos valores do seeder). Tooltip:
"+10% Velocidade de ataque" e "+15% Espinhos (dano refletido)". A Ficha mostra as linhas
"Velocidade de ataque" e "Espinhos" só quando algum item concede esses stats.

### 7.5 Dano de contato: removido

`EnemyDefinition._contactDamage` era semeado e exportado, mas nunca foi lido. **Foi removido em vez de
ligado** porque:
1. Todo ataque de inimigo já é corpo a corpo e telegrafado (`EnemyAttackState`: preparação ≥ 0,35 s e
   sair do alcance cancela, que é a janela de esquiva). Dano por encostar seria um golpe sem aviso
   em cima disso e quebraria a promessa de legibilidade do combate.
2. Os valores semeados (5–18) são do tamanho do `attack_damage`. Ligar isso a cada 1 s quase dobraria
   o DPS de todo inimigo e invalidaria as faixas de TTK das §5–§6, que não contam dano de contato.
3. No celular o herói anda por toque/clique-para-mover e encosta nos inimigos o tempo todo. O ataque
   básico de todo herói é corpo a corpo (2,5 m), então todos seriam punidos por jogar do jeito certo.

Removido de `EnemyDefinition`, `EnemyDatabaseSeeder`, `BalanceCsvMenu` (coluna `contact_damage`) e dos
9 assets de inimigo. `enemies.csv` foi atualizado sem a coluna, e `skills.csv` ganhou `stun_seconds`.

Testes: `StunThornsAttackSpeedTests` (stun bloqueia o inimigo, cancela a preparação, redução de boss,
skill que atordoa, matemática da velocidade de ataque, espinhos sem recursão, remoção do dano de contato).

---

## 8. Loot de encontro: materiais por bioma (2026-10-29)

Antes, inimigo só soltava o próprio `EnemyDefinition.LootItem` (`item_gold`). As tabelas `_loot` dos
`BiomeDefinition` existiam mas nunca eram roladas. Por isso nenhum objetivo CollectItem de madeira, fibra,
erva ou minério fechava (missões pessoais de Maria, Ícaro e Maithe e as diárias), e as receitas de
`Resources/Recipes` não tinham insumo.

### 8.1 Como uma morte rola loot (`EnemyLootRoller`, chamado por `EnemyLootGranter`)

1. **Loot base**: `LootItem` com `LootDropChance` e `LootQuantityMin..Max`, como antes. Chefe: garantido (`BossBonusRules`).
2. **Tabela do bioma**: o bioma é aquele cujo spawn table lista o inimigo (`BiomeRegistry.FindBiomeOf`;
   cada inimigo está em exatamente um). Inimigo comum: `_lootRollsPerKill` rolagens, cada uma com
   `_lootDropChance` de sair algo. Chefe: `_bossLootRolls` rolagens, **todas garantidas**. Cada rolagem
   escolhe uma entrada pelo `_weight` e dá `_quantityMin.._quantityMax` unidades.
   Equipamento (equipável e não empilhável) vira **uma** peça procedural (`ProceduralItemGenerator`)
   com raridade sorteada e nunca abaixo da raridade do item base:
   comum Comum 65 / Incomum 28 / Raro 7; chefe Incomum 45 / Raro 40 / Épico 15.
3. **Bônus de chefe**: `_bossBonusDropItem` na raridade `_bossBonusDropRarity`, com afixos.

Tudo cai direto no inventário de quem matou (ou do Player, se a morte veio de DoT ou invocação). Um
texto "+2 Madeira" sobe do corpo e sai `EnemyLootDroppedEvent`. O inventário volta ao mapa pelo estado
carregado do `SaveManager`, então a coleta continua entre mapa e encontro. A RNG é injetável:
mesma semente, mesmos drops.

### 8.2 Tabelas (`Assets/Resources/Biomes/*.asset`, espelhadas no `BiomeSeeder`)

Todos os biomas usam `_lootRollsPerKill` 2 e `_bossLootRolls` 4. `_lootDropChance` é 0,65 na Floresta e
0,60 nos outros. "% por rolagem" = chance × peso / peso total. "Por luta" considera 1,5 inimigo comum por
encontro (`WorldMap.unity`: 1–2; encontros aleatórios nunca trazem o chefe). "Chefe" é o esperado das 4 rolagens garantidas.

**Floresta** (Lobo da Mata, Aranha da Floresta; chefe Lobo Alfa), peso total 30

| Item | Peso | Qtd | % por rolagem | Por morte | Por luta | Chefe |
|---|---|---|---|---|---|---|
| `item_wood` | 10 | 1–3 | 21,7% | 0,87 | 1,30 | 2,67 |
| `item_fiber` | 8 | 1–3 | 17,3% | 0,69 | 1,04 | 2,13 |
| `item_herb` | 8 | 1–3 | 17,3% | 0,69 | 1,04 | 2,13 |
| `item_cooked_meat` | 3 | 1–2 | 6,5% | 0,20 | 0,29 | 0,60 |
| `item_wooden_sword` (procedural) | 1 | 1 | 2,2% | 0,04 | 0,07 | 0,13 |

**Deserto** (Escorpião das Dunas; chefe Guardião da Ruína), peso total 30

| Item | Peso | Qtd | % por rolagem | Por morte | Por luta | Chefe |
|---|---|---|---|---|---|---|
| `item_iron_ore` | 10 | 1–2 | 20,0% | 0,60 | 0,90 | 2,00 |
| `item_fiber` (sisal) | 5 | 1–2 | 10,0% | 0,30 | 0,45 | 1,00 |
| `item_iron` (fragmento das ruínas) | 2 | 1 | 4,0% | 0,08 | 0,12 | 0,27 |
| `item_stone` | 3 | 1 | 6,0% | 0,12 | 0,18 | 0,40 |
| `item_gold_coin` | 6 | 2–5 | 12,0% | 0,84 | 1,26 | 2,80 |
| `item_health_potion` | 3 | 1 | 6,0% | 0,12 | 0,18 | 0,40 |
| `item_hunting_bow` (procedural) | 1 | 1 | 2,0% | 0,04 | 0,06 | 0,13 |

**Montanha** (Golem de Pedra; chefe Senhor da Guerra da Montanha), peso total 24

| Item | Peso | Qtd | % por rolagem | Por morte | Por luta | Chefe |
|---|---|---|---|---|---|---|
| `item_iron_ore` | 12 | 1–2 | 30,0% | 0,90 | 1,35 | 3,00 |
| `item_iron` | 2 | 1 | 5,0% | 0,10 | 0,15 | 0,33 |
| `item_stone` | 4 | 1 | 10,0% | 0,20 | 0,30 | 0,67 |
| `item_mana_potion` | 3 | 1 | 7,5% | 0,15 | 0,22 | 0,50 |
| `item_leather_boots` (procedural) | 2 | 1 | 5,0% | 0,10 | 0,15 | 0,33 |
| `item_pickaxe` (procedural) | 1 | 1 | 2,5% | 0,05 | 0,07 | 0,17 |

**Cavernas** (Enxame de Morcegos; chefe Habitante Ancestral), peso total 22

| Item | Peso | Qtd | % por rolagem | Por morte | Por luta | Chefe |
|---|---|---|---|---|---|---|
| `item_iron_ore` (cristal) | 8 | 1–2 | 21,8% | 0,65 | 0,98 | 2,18 |
| `item_herb` (cogumelo) | 8 | 1–2 | 21,8% | 0,65 | 0,98 | 2,18 |
| `item_stone` | 3 | 1 | 8,2% | 0,16 | 0,25 | 0,55 |
| `item_health_potion` | 2 | 1 | 5,5% | 0,11 | 0,16 | 0,36 |
| `item_iron_sword` (procedural) | 1 | 1 | 2,7% | 0,05 | 0,08 | 0,18 |

**Chefes** (loot base garantido + 4 rolagens + bônus):

| Chefe | Ouro (antes 1–3, 60%) | Bônus procedural |
|---|---|---|
| Lobo Alfa | 4–8 | `item_hunting_bow` Raro |
| Guardião da Ruína | 6–10 | `item_leather_helmet` Raro |
| Senhor da Guerra da Montanha | 8–14 | `item_leather_chest` Épico |
| Habitante Ancestral | 10–16 | `item_iron_sword` Épico |

Inimigos comuns continuam soltando `item_gold` 1–3 com 60% (1,2 por morte).

### 8.3 Receitas

| Receita | Insumos | De onde vêm |
|---|---|---|
| `recipe_health_potion` | 3 `item_herb` | Floresta, Cavernas |
| `recipe_iron_ingot` | 2 `item_iron_ore` + 1 `item_wood` | Montanha/Deserto/Cavernas + Floresta |
| `recipe_iron_sword` | 2 `item_iron` + 1 `item_wood` | lingote (receita, ou drop no Deserto/Montanha) + Floresta |

### 8.4 Lutas esperadas por objetivo de coleta (melhor bioma)

Lutas ≈ quantidade ÷ "por luta". `EnemyLootTableTests` exige no máximo 12 lutas por objetivo que cai direto.

| Missão | Objetivo | Melhor bioma | Lutas |
|---|---|---|---|
| `quest_daily_family_campfire` | 6 madeira + 4 fibra | Floresta | ~4,6 (madeira) / ~3,8 (fibra), **~5** juntas |
| `quest_daily_icaro_ore_tinkering` | 4 minério + 1 lingote | Montanha | ~3 (minério); +2 minério e 1 madeira para fundir: **~4,5** |
| `quest_daily_maria_herb_basket` | 5 ervas + 1 poção | Floresta | ~4,8; com a poção feita de 3 ervas, **~7,7** |
| `quest_daily_maria_visit_survivor` | 2 carne assada | Floresta | **~6,9** |
| `quest_maithe_windtrail` | 3 minério | Montanha | ~2,2 (os 4 golens já pedem ~2,7) |
| `quest_maria_fading_light` | 2 minério | Cavernas | ~2 (os 5 morcegos já pedem ~3,3) |
| `quest_icaro_ancient_gears` | 1 lingote | Deserto | drop direto ~8,3; fundindo (2 minério + 1 madeira) **~2,2**; o Guardião da Ruína (objetivo da mesma missão) dá em média 2 minérios |
| `quest_joaquim_echoes_of_home` | 1 espada de ferro | Cavernas | drop raro (~12,5); caminhos principais: forjar (4 minério + 3 madeira), bônus Épico do Habitante Ancestral ou recompensa da 2ª missão da Montanha |
| `quest_region_desert_crossing_reserve` | 12 ouro | Deserto | **~6,7** |

Peso: ver §9 (mochila cheia). Antes o minério pesava 3 e umas 18 lutas na Montanha enchiam a mochila;
hoje são ~43, e o que não cabe nunca some em silêncio.

### 8.5 Testes

`EnemyLootTableTests` cobre o conteúdo real: todo inimigo em um só bioma; todo objetivo CollectItem
(pessoais, diárias e `RegionQuestCatalog`) obtenível por drop de inimigo comum ou por receita feita de
drops; todo insumo de receita obtenível; teto de lutas. Também cobre as regras com RNG injetada:
determinismo por semente, faixa de quantidade, chance 0, equipamento procedural com piso de raridade,
chefe com 4 rolagens garantidas e bônus Épico, e o loot da arena levado ao mapa com os afixos pelo
estado carregado do `SaveManager`.

## 9. Mochila cheia (2026-10-29)

O inventário aguenta **100 de peso e 30 espaços**. Não existe atributo Força (os atributos são
Vitalidade, Espírito, Destreza, Inteligência e Poder Arcano), então não há teto que cresça com atributo.

### 9.1 Pesos (antes → depois)

| Item | Antes | Depois | Por quê |
|---|---|---|---|
| `item_iron_ore` | 3 | **1** | material bruto mais comum de 3 biomas; era o que enchia a mochila |
| `item_iron` (lingote) | 3 | **1,5** | fundir comprime: 2 minério + 1 madeira (2,5) viram 1,5 |
| `item_stone` | 2 | **1** | mesmo raciocínio |
| `item_wood` | 1,5 | **0,5** | perto de fibra (0,1) e erva (0,2) |
| `item_gold`, `item_gold_coin` | 0 | 0 | moeda não pesa (`Currency_IsWeightless`) |

Equipamento não mudou (espada de ferro 3, couro 1–2,5). O `ResourceItemSeeder` regravava o ouro como
Material de peso 2 e pilha 99; agora grava Moeda, peso 0, pilha 9999.

Peso esperado por luta (1,5 inimigo comum, tabelas da §8.2) e lutas até encher 100 sem vender, usar nem fundir:

| Bioma | Antes | Lutas | Depois | Lutas |
|---|---|---|---|---|
| Floresta | 2,52 | ~40 | 1,22 | ~82 |
| Deserto | 3,65 | ~27 | 1,49 | ~67 |
| Montanha | 5,54 | ~18 | 2,31 | **~43** |
| Cavernas | 3,96 | ~25 | 1,75 | ~57 |

Espaços: a Montanha solta ~0,22 equipamento por luta (cada peça ocupa um espaço); em 43 lutas são ~10
peças mais ~6 pilhas, abaixo dos 30. `LootOverflowTests.EveryBiome_TakesAtLeast40Fights_ToFillTheBag`
trava o mínimo de 40.

### 9.2 Regras de entrega (`LootGrantRules`, chamado por `EnemyLootGranter`)

1. Entrada normal (`AddItem`): pilhas, depois espaços livres, limitada pelo peso.
2. O que o peso recusou entra **acima do limite** (`AddItemIgnoringWeight`) quando é moeda ou quando uma
   missão ativa ainda precisa: `QuestManager.GetOutstandingCollectAmount` = maior quantidade pedida
   entre os objetivos de coleta abertos daquele item menos o que já está na mochila. Espaço nunca é
   ignorado. Acima do peso, `AddItem` recusa qualquer coisa que pese até o jogador vender, usar ou fabricar.
3. O resto é perdido e avisado: `LootOverflowEvent` → `BagFullToastController` mostra "Mochila cheia"
   (o que ficou para trás e a dica de vender/usar/fabricar na vila) e um texto laranja "Sem espaço: 2
   Minério de Ferro" sobe do corpo. O "+N" dourado mostra só o que entrou de fato.

Equipamento procedural (`AddGeneratedItem`) só precisa de um espaço livre, como antes.


---

## 9. r8 — progressão real: missões regionais, equipamento, atributos e atordoamento (2026-10-29)

### 9.1 Método

Os playtests r5–r7 foram rodadas automatizadas dentro do Editor (Unity MCP). Esta rodada **não usou o
Editor**: o modelo das §5.3/§5.4 foi portado para Python puro em `tools/balance/` e estendido. Ele lê os
dados direto do projeto (`.asset` YAML de inimigos, biomas, itens, afixos e skills de herói,
`RegionQuestCatalog.cs`, `progression.csv`, `PlayerCombatBalance.asset`), sem Unity:

```
python -I tools/balance/run_r8.py                      # relatório completo (as tabelas abaixo saem dele)
python -I tools/balance/run_r8.py --stun reset         # com o atordoamento como era antes do r8
python -I tools/balance/run_r8.py --candidate x.json   # testa um retune em memória antes de editar assets
```

O que o modelo faz (`balance_model.py`):
- **Luta em passos de 20 ms** (não mais uma razão de DPS): inimigos golpeiam a cada `attack_cooldown`
  depois de telegrafar (`EnemyAttackState`, preparação por arquétipo de `EnemyMotion`); skills de dano
  por prioridade, com custo de mana, recarga e **cast + recuperação bloqueando** o ataque básico; skills
  de área ≥ 2,5 m acertam todos os inimigos da luta; o básico acerta um alvo.
- **Atordoamento**: `StunRules` (chefe ×0,5); o inimigo atordoado não golpeia. O relatório conta os
  golpes do chefe com e sem stun.
- **Equipamento**: Monte Carlo (3 000 campanhas) das tabelas de loot dos biomas, raridades do
  `EnemyLootRoller`, 4 rolagens garantidas + bônus do chefe e sorteio de afixos sem reposição pelos pesos
  do `ProceduralItemGenerator`; o herói equipa a melhor peça por slot. Velocidade de ataque, espinhos,
  crítico, dano plano/percentual, defesa plana e vida entram no combate. Itens-base de recompensa de
  missão (sem afixos) não dão stat nenhum: `_weaponDamage`/`_armorDefense` só são lidos por
  `MeleeWeapon`/`RangedWeapon`, não pelo `PlayerBasicAttack`, e `_equipStatModifiers` está vazio em todos.
- **XP por rota**: "só chefes" = as 3 vitórias que liberam o chefe + chefe (com 1 lacaio); "missões
  regionais" = lutas até cumprir as duas missões da região em sequência (abates no ritmo do peso de
  spawn, 1,5 inimigo por encontro) + chefe. Missões pessoais/convergência/diárias ficam de fora: as NPCs
  delas estão na `Main.unity`, fora do laço mapa → encontro.
- **Atributos e pontos de skill** (novos no modelo): 1 Vitalidade + 1 Poder Arcano por nível; 1 ponto de
  skill por nível na skill de dano com maior ganho de DPS (máx. 5), ultimate aprendida no nível 10.
- **Calibração**: com os dados e o protocolo do r7 (níveis 2/5/8/11, sem equipamento, atributos, pontos
  nem stun), "2 comuns" dá 1,9–4,7 s e "chefe + lacaio" 17,9–22,9 s de média, contra 4,3–7,4 s e 15–22 s
  medidos no r7. Fatores jogo/modelo: **comuns ×1,73** (aproximação, fuga e golpes perdidos), **chefes
  ×0,89**. Os tempos "no jogo" abaixo são o modelo × esse fator.

Fora do modelo (como no r5–r7): esquiva, buffs/debuffs de % Defesa e de dano do inimigo, curas.

### 9.2 Diagnóstico (dados do r7 com a progressão de hoje)

| Rota | Região | Lutas | XP abates / missões / chefe | Nível no chefe | Chefe no jogo (média dos 5) | 2 comuns no jogo (média) |
|---|---|---|---|---|---|---|
| só chefes | Floresta | 3 | 45 / 0 / 90 | 1,4 | 15,8 s | 5,2 s |
| só chefes | Deserto | 3 | 81 / 0 / 162 | 3,0 | 13,1 s | 5,1 s |
| só chefes | Montanha | 3 | 126 / 0 / 252 | 5,3 | 13,2 s | 4,9 s |
| só chefes | Cavernas | 3 | 180 / 0 / 360 | 7,9 | 14,4 s | 2,4 s |
| missões | Floresta | 12,4 | 187 / **350** / 90 | **5,5** | 11,6 s | 4,1 s |
| missões | Deserto | 5,3 | 144 / **580** / 162 | **10,0** | 9,5 s | 1,7 s |
| missões | Montanha | 6,7 | 280 / **840** / 252 | **14,5** | 8,2 s | 1,5 s |
| missões | Cavernas | 9,3 | 560 / **1 140** / 360 | **19,0** | 8,3 s | 0,4 s |

- **As missões regionais eram o maior salto de poder**: 2 910 XP, mais que todas as lutas que elas pedem
  somadas aos 4 chefes (~2 000). Quem as fazia chegava aos chefes nos níveis 5,5 / 10 / 14,5 / 19 (o r7
  supunha 2 / 4 / 6 / 8) e encontrava chefes de 8–12 s e comuns que morriam na primeira rotação.
- **Mesmo sem missões**, atributos e pontos de skill (que o r7 não contava) já tiravam os chefes da meta
  (13–16 s).
- **Morte sem skills nem defesa**: só no ataque básico, o jogador da rota com missões vencia todos os
  chefes, então a regra "risco real sem skills/defesa" não valia mais.
- **Equipamento**: na média pesa pouco (+3 de dano plano a partir do Deserto, +2% de velocidade de ataque,
  3% de espinhos nas Cavernas), mas a cauda é grande: Brutal 6–12 + Afiada 2–5 somam até +17 num básico
  de 10. Os 10% com mais sorte (+9,7 de dano plano) matavam os chefes em 12,7–15,3 s.
- **Atordoamento**: no código do r7 o stun tirava o inimigo do estado de ataque e o `Enter` rearmava o
  golpe uma preparação depois do fim do stun. Num chefe de 2,4–3,3 s de recarga com 0,3–0,5 s de stun,
  isso **adiantava** o próximo golpe: com a Granada do Ícaro (recarga 8 s) o Lobo Alfa dava 9 golpes numa
  luta em vez de 7. Bug de fórmula, corrigido em código (§9.3).

### 9.3 Mudanças aplicadas (antes → depois)

| Onde | Campo | Antes | Depois |
|---|---|---|---|
| `RegionQuestCatalog` | XP de `forest_howling_trail` / `forest_spider_webs` | 150 / 200 | **60 / 80** |
| `RegionQuestCatalog` | XP de `desert_stinger_road` / `desert_crossing_reserve` | 260 / 320 | **100 / 130** |
| `RegionQuestCatalog` | XP de `mountain_walking_stones` / `mountain_mine_pass` | 380 / 460 | **150 / 180** |
| `RegionQuestCatalog` | XP de `caves_dark_lamps` / `caves_last_lantern` | 520 / 620 | **200 / 240** |
| Lobo Alfa | HP / dano | 430 / 14 | **520 / 15** (DPS 5,83 → 6,25) |
| Escorpião das Dunas | HP | 85 | **110** |
| Guardião da Ruína | HP / dano | 505 / 22 | **780 / 32** (DPS 6,67 → 9,70) |
| Golem de Pedra | HP | 100 | **140** |
| Senhor da Guerra da Montanha | HP / dano | 580 / 16 | **900 / 30** (DPS 6,15 → 11,54) |
| Enxame de Morcegos | HP | 80 | **170** |
| Habitante Ancestral | HP / dano | 700 / 18 | **1 180 / 42** (DPS 6 → 14) |
| `affix_sharp` (Afiada) | AttackDamage plano | 2–5 | **1–3** |
| `affix_brutal` (Brutal) | AttackDamage plano | 6–12 | **3–6** |
| `EnemyController` / `EnemyAttackState` (código) | golpe depois de stun ou de sair/voltar ao alcance | próximo golpe 1 preparação depois | **a recarga que faltava continua valendo** (congelada durante o stun); a preparação inteira sempre telegrafa |

Sem mudança: recargas dos inimigos, XP por inimigo, tabelas de loot, chances de drop, raridades,
durações de stun (no chefe elas atrasam ~1 golpe por luta; o valor delas é cortar uma preparação na hora
certa, o que o modelo não mede), afixos de velocidade de ataque e espinhos (na média dão +2% e 3%), XP das
missões pessoais/convergência/diárias.

Espelhos: `EnemyDatabaseSeeder` + `EnemyDatabaseSeederTests`, `AffixDatabaseSeeder`, `enemies.csv`. Nos
`.asset` de inimigo só as linhas `_maxHealth`/`_attackDamage` mudaram.

Por quê:
- **XP das missões a ~40%**. Cada missão continua valendo 0,75–2,4× o XP dos próprios abates (antes até
  3×) e a rota com missões ainda chega ~2,5–7 níveis acima da rota só de chefes. A ultimate (nível 10)
  chega na Montanha para quem faz as missões e no fim das Cavernas para quem só enfrenta chefes.
- **Inimigos re-baseados no nível da rota com missões** (3 / 7 / 10 / 15 no chefe): os chefes ganham
  +21% / +54% / +55% / +69% de HP e o dano por golpe sobe para que, parado e sem defesa, o herói morra em
  21–25 s, antes de matar o chefe só no ataque básico (24–40 s). O golpe continua lento e telegrafado
  (recargas iguais). O Lobo Alfa só ganhou +1 de dano: é o chefe tutorial.
- **Comuns**: Escorpião +29%, Golem +40%, Morcegos +113%. Os morcegos eram o caso pior: no nível 13
  qualquer skill de área matava os dois na abertura.
- **Afixos de dano plano pela metade**: o básico tem 10 de dano; dois afixos na mesma arma agora somam no
  máximo +9 (antes +17). O p90 de sorte passa a encurtar o chefe em ~15% (antes ~27%).

### 9.4 Resultado (r7 → r8)

Tempos "no jogo" = modelo × fator de calibração (§9.1); pior / **média** / melhor dos 5 heróis. Meta:
lutas comuns 4–7 s, chefes 15–22 s, morte possível sem skills nem defesa.

| Bioma | Rota | Nível no chefe r7 → r8 | 2 comuns: média r7 → r8 (pior / **média** / melhor) | Chefe: média r7 → r8 (pior / **média** / melhor) | Só básico, r8: mata o chefe / morre em |
|---|---|---|---|---|---|
| Floresta | missões | 5,5 → **4,0** | 4,1 → 7,5 / **4,8** / 1,8 s | 11,6 → 20,0 / **16,5** / 12,3 s | 24,0 / 22,2 s |
| Deserto | missões | 10,0 → **7,1** | 1,7 → 8,7 / **6,1** / 3,8 s | 9,5 → 22,8 / **18,2** / 12,7 s | 28,0 / 20,6 s |
| Montanha | missões | 14,5 → **10,9** | 1,5 → 10,4 / **6,4** / 3,8 s | 8,2 → 24,6 / **18,4** / 12,0 s | 29,5 / 21,6 s |
| Cavernas | missões | 19,0 → **15,0** | 0,4 → 12,2 / **4,6** / 0,0 s | 8,3 → 27,1 / **20,0** / 13,0 s | 33,5 / 24,8 s |
| Floresta | só chefes | 1,4 | 5,2 → **5,2** s | 15,8 → **19,3** s | 25,5 / 14,9 s |
| Deserto | só chefes | 3,0 | 5,1 → **8,5** s | 13,1 → **22,5** s | 31,5 / 13,9 s |
| Montanha | só chefes | 5,3 | 4,9 → **9,8** s | 13,2 → **22,9** s | 34,0 / 13,7 s |
| Cavernas | só chefes | 7,9 | 2,4 → **10,0** s | 14,4 → **28,2** s | 41,0 / 15,7 s |

- **Rota com missões (a de referência)**: todas as médias dentro da meta (comuns 4,6–6,4 s, chefes
  16,5–20,0 s), e só no ataque básico o chefe mata primeiro em todos os biomas.
- **Rota só de chefes**: fica mais difícil de propósito (2,5–7 níveis abaixo). Chefes de 19–28 s e comuns
  de 8,5–10 s; sem defesa o chefe mata em 14–16 s, então vencer pede Muralha/Escudo/curas/esquiva ou
  algumas lutas a mais. Derrota volta ao mapa sem perda.
- **Os mesmos dados do r8 no protocolo do r7** (níveis 2/4/6/8, sem atributos, pontos nem equipamento)
  dariam chefes de 19–42 s: o r8 assume que o jogador gasta atributos e pontos de skill. Um playtest no
  Editor deve usar o nível e os pontos da rota com missões.

**Tempo para subir de nível** (rota com missões; encontro ≈ luta comum + 20 s de mapa/introdução):

| Região | XP por encontro | Nível típico r7 → r8 | XP para o próximo | Encontros por nível (só abates) | Min/nível só abates | Min/nível com missões |
|---|---|---|---|---|---|---|
| Floresta | 15 | 3 → 2 | 121 → 110 | 8,1 → 7,3 | 3,1 → 2,9 | 1,1 → 1,6 |
| Deserto | 27 | 8 → 5 | 195 → 146 | 7,2 → 5,4 | 2,7 → 2,3 | 0,5 → 0,9 |
| Montanha | 42 | 12 → 9 | 285 → 214 | 6,8 → 5,1 | 2,4 → 2,2 | 0,6 → 1,0 |
| Cavernas | 60 | 17 → 13 | 459 → 314 | 7,7 → 5,2 | 2,6 → 2,1 | 0,9 → 1,2 |

**Poder na entrada de cada região** (rota com missões, r8; equipamento médio do Monte Carlo):

| Região | Nível | Vida | Ataque básico | Kit Maithe / Joaquim | Equipamento médio | Chefe da região mata o herói parado em |
|---|---|---|---|---|---|---|
| Floresta | 1 | 100 | 20,0 DPS | 17,3 / 2,5 DPS | nada | 16,0 s |
| Deserto | 4 | 154 | 26,0 DPS | 26,5 / 4,8 DPS | +1,6 dano plano, +2% vel., +1% crítico | 15,9 s |
| Montanha | 8 | 226 | 29,4 DPS | 35,0 / 6,2 DPS | +1,7 dano plano, +2% vel., +2,5% crítico | 19,6 s |
| Cavernas | 11 | 286 | 31,8 DPS | 43,0 / 6,7 DPS | idem + 3% espinhos, +3,6 defesa, +6 vida | 21,2 s |

(r7, mesma rota: níveis 1 / 6 / 10,7 / 15,2, básico 20 / 30,4 / 34,4 / 38,6 DPS, chefe matava em
17 / 28,5 / 42,6 / 61,8 s.)

**Sorte no loot** (p90 do dano plano, rota com missões): Deserto +4,8 → chefe 18,2 → 15,4 s; Montanha
+4,9 → 18,4 → 15,5 s; Cavernas +5,1 → 20,0 → 17,4 s (antes: +9,7 → 12,7 s no Deserto).

**Atordoamento nos chefes** (golpes do chefe numa luta: sem stun → código r7 → r8): Granada do Ícaro no
Lobo Alfa 7 → 9 → 7; Correntes da Lavine no Guardião 5 → 6 → 5 e na Habitante 6 → 7 → 6. Com a correção o
stun nunca adianta um golpe; no chefe custa 0–1 golpe por luta, nos comuns (0,6–1,0 s contra recargas de
0,7–1,6 s) cerca de 1 golpe por conjuração.

### 9.5 Testes

`BalanceInvariantsTests` re-baseado no r8 (justificativa nos comentários do teste):
- nível esperado por bioma 2/4/6/8 → **3/7/10/15** (o chefe da rota com missões), com atributos
  (Vitalidade/Poder Arcano) e pontos de skill gulosos no modelo analítico;
- faixas de HP de chefe: Floresta 450–650, Deserto 650–950, Montanha 750–1100, Cavernas 1000–1400;
- as outras regras não mudaram: chefe mata o jogador parado em 12–40 s, luta com kit 10–35 s para os 5
  heróis, só no básico o chefe vence, com defesa todos vencem, a Maithe vence sem defender, comuns 2,5–8 s
  só no básico e ≥ 1,5 s com o melhor kit (r8: 21–25 s, 12–33 s, 24–40 s contra 21–25 s, 3,3–5,8 s);
- novos: `RegionQuestRoute_ReachesTheExpectedBossLevels` (a economia de XP real leva aos níveis acima),
  `BossRushRoute_IsBelowTheQuestRoute`, `RegionQuestXp_StaysInProportionToTheKillsItAsksFor` (total
  800–1 500; cada missão 0,5–2,5× o XP dos próprios abates) e `FlatAttackDamageAffixes_NeverDoubleTheBasicAttack`.

`StunThornsAttackSpeedTests.StunRightAfterAStrike_DelaysTheNextSwing_InsteadOfReArmingItEarly` cobre a
correção do atordoamento. `EnemyDatabaseSeederTests` espelha os novos HP/dano.
