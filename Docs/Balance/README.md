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

**Seções** (cada sistema novo acrescenta a sua no fim, com o próximo número livre; cite como "Balance §N"):

| § | Assunto |
|---|---|
| 1 | Como editar a curva |
| 2–3 | Análise de sanidade e o primeiro rebalanceamento (bug dos atributos mortos) |
| 4 | Defesa, velocidade e stats mortos |
| 5–6 | Rodadas r6 e r7 (playtests no Editor) |
| 7 | Atordoamento, velocidade de ataque, espinhos, dano de contato; **7.6** stats de fração com base 0 |
| 8–9 | Loot de encontro por bioma; mochila cheia |
| 10 | r8: progressão real e o modelo em Python (`tools/balance/`) |
| 11 | Nova Jornada+ |
| 12 | Cozinha da Vó Zefa (refeições) |
| 13 | Eventos aleatórios do mapa |
| 14 | Pilha de poder: refeição + relíquias + evento + carinho, NG+0 e NG+1 (r9) |

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

### 7.6 Stats de fração com base 0: percentual não pode sumir (bug, 2026-10-29)

`BuffController.GetModifiedValue(stat, base)` = (base + Σ plano) × (1 + Σ percentual). Com base 0, todo
modificador PercentAdditive some: (0 + 0) × 1,05 = 0. Por isso os stats que já são frações de 0 a 1 são
lidos com `BuffController.GetAdditiveFractionTotal` (plano e percentual somam igual, "+0,05 = 5%"):

| Stat | Leitor | Antes | Fontes PercentAdditive afetadas |
|---|---|---|---|
| CriticalChance / CriticalDamage | `OutgoingDamage` | já somava | `affix_lucky` |
| AttackSpeed | `PlayerBasicAttack`, Ficha | já somava | `affix_nimble`, `affix_of_frenzy`, Rapadura |
| Thorns | `ThornsRules` | já somava | `affix_thorned`, `affix_of_the_mandacaru`, Queijo coalho |
| **CooldownReduction** | `SkillManager.ResolveCooldownReduction`, Ficha | **`GetModifiedValue(CDR, 0)`** | **`affix_of_haste` (3–8%) não fazia nada**; e um plano ainda era multiplicado pelo percentual |
| **ResourceCostReduction** | `SkillManager.ResolveResourceCostReduction` | **`GetModifiedValue(RCR, 0)`** | nenhuma hoje (combo, afixo e relíquia são planos); latente |

Os outros stats têm base > 0 e o percentual funciona como percentual: MaxHealth/MaxMana/MaxStamina
(curva), HealthRegen 1/s, ManaRegen 5/s, StaminaRegen 10/s, MoveSpeed (multiplicador base 1),
AttackDamage (dano do golpe); Defense separa plano e percentual (`DamageCalculator.GetDefense`, §4).
A Ficha (`CharacterSheetPresenter`) usa o mesmo `ResolveCooldownReduction`. Afixo, refeição e relíquia
continuam autorados como estavam (CDR plano na Cozinha e nas relíquias); os dois jeitos valem agora.
Testes: `ZeroBaseFractionStatTests` (cada stat de fração conta plano e percentual; todo afixo de fração
de `Resources/Affixes` tem efeito no mínimo da rolagem; Ficha = recarga real).

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

## 10. r8 — progressão real: missões regionais, equipamento, atributos e atordoamento (2026-10-29)

### 10.1 Método

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

### 10.2 Diagnóstico (dados do r7 com a progressão de hoje)

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
  luta em vez de 7. Bug de fórmula, corrigido em código (§10.3).

### 10.3 Mudanças aplicadas (antes → depois)

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

### 10.4 Resultado (r7 → r8)

Tempos "no jogo" = modelo × fator de calibração (§10.1); pior / **média** / melhor dos 5 heróis. Meta:
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

### 10.5 Testes

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

---

## 11. Nova Jornada+ (New Game+) (2026-10-29)

Depois do epílogo e dos créditos, o botão **Nova Jornada+** aparece na Vila (Floresta) do mapa
(`NewGamePlusHud`). Ele recomeça a campanha com o mesmo herói: **mantém** nível, atributos, skills,
equipamento, mochila, ouro e o Baú da Vó Zefa; **recomeçam** a história (`StoryProgress`), o mapa (só a
Floresta liberada), os chefes (`DefeatedEnemyRegistry`) e as 8 missões regionais (`RegionQuestCatalog`).
Missões pessoais, de convergência e diárias, construções, receitas, relógio e clima ficam como estão. O
ciclo é salvo na chave `NewGamePlus` (`NewGamePlusProgress`); save antigo, sem a chave, carrega como ciclo 0.

### 11.1 Regras (`NewGamePlusRules`, testadas em `NewGamePlusTests`)

Ciclo `c` = 0 na primeira campanha, 1 na Nova Jornada+ 1, ... teto **5** (jornadas depois da 5ª usam os
números da 5ª). Com `c = 0` todo multiplicador é exatamente 1 e o `EnemyLootRoller` não sorteia nenhum
número a mais (mesma semente, mesmos drops de antes).

| O quê | Fórmula | NG+1 | NG+2 | NG+3 | NG+5 | Onde |
|---|---|---|---|---|---|---|
| Vida dos inimigos (chefes inclusos), por região | 1 + bônus da região + 0,15·c | Floresta ×2,2 · Deserto ×1,5 · Montanha ×1,35 · Cavernas ×1,15 | +0,15 | +0,3 | +0,6 (×2,8 / 2,1 / 1,95 / 1,75) | `EnemyController.ApplyDefinitionToHealth` (arredondada) |
| Dano por golpe (antes de buffs/debuffs), por região | 1 + bônus da região + 0,25·c | ×2,7 · ×1,8 · ×1,6 · ×1,45 | +0,25 | +0,5 | +1,0 (×3,7 / 2,8 / 2,6 / 2,45) | `EnemyController.EffectiveAttackDamage` + hitbox |
| XP por abate (depois do bônus de chefe) | 1 + 0,5·c | ×1,5 | ×2,0 | ×2,5 | ×3,5 | `EnemyRewardGranter` |
| Quantidade do loot base (ouro) | 1 + 0,25·c | ×1,25 | ×1,5 | ×1,75 | ×2,25 | `EnemyLootRoller` (passo 1) |
| Chance de cada peça de equipamento subir 1 raridade | 0,1·c | 10% | 20% | 30% | 50% | `EnemyLootRoller` (passo 2; nunca passa de Lendário) |

Bônus da região (de NG+1 em diante, uma vez): vida Floresta +1,05 / Deserto +0,35 / Montanha +0,2 /
Cavernas 0; dano +1,45 / +0,55 / +0,35 / +0,2. A região do inimigo é a da tabela de spawn que o lista
(`BiomeRegistry.FindBiomeOf`, só consultada fora do ciclo 0); inimigo sem região usa os números do Deserto.
O HUD mostra a faixa ("vida ×1,15–2,2, dano ×1,45–2,7").

Não escalam: XP/recompensas de missões, chance de drop e quantidades da tabela do bioma, o bônus de chefe
(`_bossBonusDropRarity` fixa), recargas e velocidade dos inimigos.

### 11.2 Por que esses números (r9, ajustados no modelo, §14.4)

- Quem termina a campanha pela rota com missões chega ao chefe das Cavernas no nível ~15 (§10.4) e entra na
  NG+1 por volta do 16; durante a NG+1 ele sobe até ~22. A campanha espera 3 / 7 / 10 / 15 nos chefes,
  então a distância de nível é enorme na Floresta (+14) e pequena nas Cavernas (+7).
- **Antes (r8, ×1,8 vida / ×1,5 dano em todo lugar)**: a NG+1 era trivial na Floresta (chefe 14 s, comuns
  2 s, o chefe levava ~45 s para matar o herói parado) e arrastada nas Cavernas (chefe 33 s).
- **Agora** o bônus da região compensa a distância de nível uma vez, e o passo por ciclo (+0,15 vida,
  +0,25 dano) é igual para todas. Com isso todo chefe da NG+1 fica em 18,5–19,8 s, e na NG+2/NG+3 em
  ~16–21 s (o modelo mantém o equipamento das Cavernas da NG+0, então nos ciclos altos é conservador).
- **Dano cresce mais que vida**: a vida do herói cresce a cada nível (curva + Vitalidade), e só o dano faz o
  chefe matar o herói parado em ~25 s, como na NG+0 (21–27 s). Lobo Alfa NG+1: 1 144 de vida / 40,5 de dano;
  Habitante Ancestral NG+1: 1 357 / 60,9.
- Escala por região (e não pelo nível do herói) porque é determinística, igual para todo mundo, sem
  "elástico" que pune quem subiu de nível, e não depende de quem está no grupo.
- XP ×1,5 por ciclo compensa a curva (`xp_to_next` cresce 10% por nível): sem isso, a NG+1 inteira daria
  poucos níveis. O ouro e a chance de raridade são a recompensa visível de jogar de novo.

### 11.3 Testes

`NewGamePlusTests`: ciclo 0 não muda nada em nenhuma região (nem arredonda), os números do NG+1 batem com a
tabela (Floresta e Cavernas), todos crescem até o teto e param, cada região cresce a cada ciclo e as regiões
iniciais sempre escalam mais, cada chefe é achado na região certa pelo `BiomeRegistry`, o HUD mostra a faixa
("vida ×1,15–2,2"), XP/loot nunca ficam abaixo da base, o loot
do ciclo 0 é idêntico ao de antes com a mesma semente, a NG+5 dá mais ouro e equipamento melhor no chefe da
Montanha, o reset/manutenção do estado, save/load, save antigo = ciclo 0 e a conquista `ach_new_journey_plus`.

## 12. Cozinha da Vó Zefa: refeições do fogão a lenha (2026-10-29)

Serviço novo da Vila no mapa (Vila → Serviços → **Fogão a lenha**). O primo cozinha um prato com materiais
de bioma + "mantimentos da venda" (pagos em `item_gold`, a moeda da loja) e come na hora. Dados em
`MealCatalog`, regras em `CookingRules` (puras, `CookingTests`), estado salvo por slot em `MealService`
(chave `VillageKitchen`), efeito aplicado na arena por `MealEffectApplier`.

### 12.1 Regras

- **Uma refeição por vez.** Mesmo prato de novo → volta para a duração cheia (não soma). Outro prato →
  substitui o anterior. Cozinhar que falha (falta ingrediente/ouro) não gasta nada.
- **Duração em lutas, não em tempo.** Todo encontro resolvido (vitória, derrota ou fuga) gasta 1, no
  `EncounterEndedEvent` — antes do auto-save, então a contagem vai junto. Fugir também conta: a refeição
  não é um jeito de pular lutas.
- **Efeitos = modificadores comuns no `BuffController`.** Fonte `meal/effects` (duração 0 = a luta inteira,
  some quando a cena da arena descarrega) e `meal/burst` (uma vez por luta, com duração própria). Fontes
  diferentes empilham com skills, atributos e equipamento; refeição nunca empilha com refeição. Os tetos
  existentes continuam valendo (defesa % ≤ 80%, velocidade de ataque ≤ +100%, espinhos ≤ 50%, recarga ≤ 100%).
- **Aprender.** Cuscuz e café coado desde o começo. Os outros: a 1ª missão regional da região do prato
  (qualquer primo) **ou** a Vó ensina ao primo que ama o prato quando ele abre o fogão, se a região já
  estiver aberta. Save antigo sabe o que as missões já concluídas ensinam.

### 12.2 Pratos

Ouro por luta ≈ 1,8 (1,5 inimigo × 60% × 1–3 de `item_gold`, §8). Cada prato custa ~1–2 lutas de ouro
e dura 3–4 lutas, então comer sempre custa ~⅓–½ do ouro que a luta rende. Os ingredientes são os
materiais comuns de §8 (madeira é a lenha; a loja também vende madeira e pedra).

| Prato | Efeito (luta inteira) | Lutas | Ingredientes | Mantimentos | Aprende com |
|---|---|---|---|---|---|
| Cuscuz | StaminaRegen +30% (10 → 13/s) | 3 | madeira 1, erva 1 | 2 ouro | início |
| Café coado | CooldownReduction +0,06 (Flat) | 3 | madeira 1, fibra 1 | 2 ouro | início |
| Pamonha | MaxHealth +10% | 3 | madeira 2, fibra 2 | 3 ouro | `quest_region_forest_howling_trail` / Vó → Maria |
| Rapadura | AttackSpeed +12% **nos primeiros 20 s** (era +25%, §14.3) | 4 | madeira 2, pedra 1 | 3 ouro | `quest_region_desert_stinger_road` / Vó → Ícaro (Deserto aberto) |
| Baião de dois | Defense +10% (percentual) | 3 | madeira 2, erva 2, carne assada 1 | 4 ouro | `quest_region_mountain_walking_stones` / Vó → Joaquim (Montanha aberta) |
| Queijo coalho assado | Thorns 8% | 3 | madeira 1, minério 1, erva 1 | 4 ouro | `quest_region_caves_dark_lamps` / Vó → Lavine (Cavernas abertas) |

Por que esses números: cada efeito fica na faixa de **um afixo incomum** (afixos de §7: velocidade de
ataque +6–12%, espinhos 5–10%; café ≈ 15 pontos de Inteligência a 0,4%), porque a refeição ocupa um
"espaço" que todo herói tem, sem raridade. A Rapadura vale só nos primeiros 20 s (o "Prepare-se!" dura
1,75 s e um comum morre em 2,5–8 s, §10.5), por isso dura uma luta a mais. Ela começou com +25%, acima de
qualquer afixo incomum: com chefe de 15–20 s cobria a luta inteira e era a melhor refeição para quase
todo herói na pilha de poder (§14); ficou em +12%, o teto do `affix_nimble`.

### 12.3 Favoritos (bônus extra e fala da Vó)

| Primo | Prato | Bônus extra |
|---|---|---|
| Maithe | Café coado | CriticalChance +3% |
| Maria | Pamonha | HealthRegen +1 HP/s (a base é 1) |
| Joaquim | Baião de dois | MaxHealth +5% |
| Ícaro | Rapadura | MoveSpeed +8% durante o embalo de 20 s |
| Lavine | Queijo coalho assado | ManaRegen +20% (5 → 6/s) |

O bônus é decidido pelo herói que está na arena, não por quem cozinhou. Cuscuz é de todo mundo e de ninguém.

### 12.4 Testes

`CookingTests`: catálogo (2 iniciais, cada primo com exatamente um favorito, ingredientes existem em
`Resources/Items`, CDR em Flat, burst com duração), validação e consumo (falha não gasta), empilhamento
(renova sem somar, troca substitui), contagem por luta (vitória/fuga/derrota), aprender por missão/lição
com trava de região, save JSON, efeitos no `BuffController` e textos do painel/HUD.

## 13. Eventos aleatórios do mapa (2026-10-29)

Cartas de evento que aparecem **no meio de uma viagem** entre regiões do mapa-múndi. Dados em
`WorldEventCatalog` (código, como `RegionQuestCatalog`), regras puras em `WorldEventRules` /
`WorldEventDirector` (`WorldEventTests`), estado salvo por slot em `WorldEventState` (chave `WorldEvents`),
carta em `WorldEventCardController`, efeitos de luta aplicados na arena por `WorldEventArenaHooks`.

### 13.1 Frequência (`WorldEventSettings`)

- **Um sorteio por viagem**, não por passo: uma viagem tem ~10 passos de encontro (nós a 10 m, 1 passo/m,
  carência de 8), então um sorteio por passo cairia quase sempre no fim do caminho. A carta abre entre 30% e
  70% do trajeto.
- Chance por viagem: **25%**, +10% a cada viagem elegível sem evento, teto **50%**.
- **Recarga: 2 viagens** depois de um evento. Média medida (3000 viagens, semente fixa): **1 evento a cada
  ~4,7 viagens**. Explorar (tocar a região atual) não é viagem e não sorteia.
- Uma viagem cortada por um encontro antes da carta não gasta a recarga nem zera o "azar acumulado".
- **Nunca o mesmo evento duas vezes seguidas** (se houver outro possível). Toda região, em toda hora do dia,
  tem pelo menos 2 eventos possíveis (teste).
- **Desligado no prólogo/tutorial:** precisa do prólogo visto, nenhuma cena de história pendente, **1 vitória**
  (a primeira luta é o tutorial), HUD visível, nada pausado e nenhuma conversa aberta.

### 13.2 Eventos e pesos

Peso relativo entre os elegíveis na região/hora. Noite = fase `Night` do `TimeOfDayManager` (20h–5h).

| Evento | Onde / quando | Peso | Escolha A | Escolha B (sempre grátis) |
|---|---|---|---|---|
| Mascate ambulante | todas, de dia | 10 | -18 ouro → +2 Poção de Vida (loja: 30) | nada |
| Chuva de caju | Floresta, Deserto, de dia | 8 | cura 35% da vida no início da próxima luta | +3 vida/s na próxima luta |
| Romeiros na estrada | todas, de dia | 8 | -1 Poção de Vida → +8 ouro, +15 XP* | +10% defesa na próxima luta |
| Emboscada de elite | todas, qualquer hora | 6 (×1,5 à noite) | luta com elite + 1 comum | +1 h no relógio |
| Cantoria ao pé da fogueira | todas, **só à noite** | 10 | +8% dano na próxima luta, +1 h (era +15%, §14.3) | cura 25% na próxima luta |
| Tempestade de areia | Deserto | 12 | viagem 50% mais lenta até chegar | +2 h no relógio |
| Neblina na serra | Montanha | 12 | +3 h no relógio | viagem 40% mais lenta, +2 minério de ferro |
| Eco na caverna | Cavernas | 12 | luta comum agora, +20 XP* nela | +5% crítico na próxima luta |

\* XP × região, na mesma razão do XP dos inimigos (10/18/28/40): Floresta ×1, Deserto ×1,8, Montanha ×2,8,
Cavernas ×4. Romeiros = 1,5 inimigo de XP, eco = 2. **Ouro não escala:** os inimigos soltam 1–3 de ouro em
qualquer região (~1,8 por luta, §8), então 8 ouro ≈ 4 lutas e o desconto do mascate (12 ouro) ≈ 7 lutas.

**"Próxima luta":** o mapa não tem `PlayerStats`/`BuffController` vivos, então XP, cura e buffs ficam na fila
de `WorldEventState` (salva) e entram dois quadros depois que a Main carrega (depois do estado carregado do
herói, que senão sobrescreveria vida/XP). Buffs usam a fonte `world_event/<stat>` com duração 0 (a luta
inteira) e empilham com refeição, skills e equipamento; o mesmo stat de dois eventos não soma (substitui).
Curas somam até 100%.

### 13.3 Emboscada de elite (`WorldEventElite`)

| | Valor |
|---|---|
| Grupo | 1 elite (comum da região, nunca chefe) + 1 comum |
| Vida do elite | ×2 |
| Dano do elite | +35% (AttackDamage percentual no `BuffController`) |
| Tamanho | ×1,25 |
| Bônus ao matar o elite | +10 ouro, +2 rolagens extras do loot dele (ouro + tabela do bioma), +20 XP × região |

Ou seja: o elite vale ~3 inimigos de XP e ~3× o loot de um comum, por uma luta ~2× mais longa contra ele.

### 13.4 Testes

`WorldEventTests`: travas (prólogo, cena pendente, 0 vitórias, modal, conversa), elegibilidade por
região/hora, peso noturno, distribuição dos pesos, sem repetição, recarga/azar acumulado/teto, semente
reproduzível, frequência média 4–6 viagens, conteúdo (itens existem, B grátis, ids únicos), efeitos na mochila
real (custo recusado não muda nada), fila de bônus aplicada em `PlayerStats`/`BuffController`, elite (grupo,
vida/dano/tamanho), save (ida e volta, save antigo, novo jogo, registro no GameManager) e textos nos dois idiomas.

## 14. Pilha de poder: refeição + relíquias + evento + carinho, NG+0 e NG+1 (r9, 2026-10-29)

Depois do r8 entraram cinco fontes de poder, todas modificadores no mesmo `BuffController`, cada uma
balanceada sozinha: refeições (§12), relíquias (`RelicDefinition`/`RelicPassiveApplier`), buffs de evento
da estrada (§13), o carinho no Caramelo (`CarameloCuddle`) e a Nova Jornada+ (§11). O bestiário não dá
poder. Golpe em dupla e as lojas novas ainda estavam sendo escritos e ficaram de fora desta rodada.

### 14.1 Método (`tools/balance/power_stack.py`)

```
python -I tools/balance/power_stack.py                    # relatório (as tabelas abaixo saem dele)
uv run --no-project --with pytest python -m pytest -q -p no:cacheprovider tools/balance   # testes do modelo
```

- Os números de cada fonte são lidos do C# (`MealCatalog`, `RelicDefinition`, `WorldEventCatalog`,
  `CarameloCuddle`, `NewGamePlusRules`), então o relatório acompanha o código.
- Herói de referência: o do r8 (rota com missões, nível, atributos, pontos de skill e equipamento médio do
  Monte Carlo, §10.1), com os mesmos fatores de calibração jogo/modelo.
- **Pilha completa** = a melhor refeição para aquele herói e luta (com o bônus de favorito) + todas as
  relíquias das regiões já abertas (3 / 6 / 9 / 12 na NG+0; as 12 na NG+1; ×2 para o dono) + o melhor buff
  de evento + o carinho (só MoveSpeed: 0 em combate). Na prática o evento escolhido é sempre a Cantoria.
  Toda refeição do `MealCatalog` entra na disputa, inclusive as da pesca (tilápia: ManaRegen +25%; caldo
  de piaba: +1,5 HP/s), tratadas como disponíveis desde o início.
- **NG+1**: inimigos com a escala por região do §11.1 (lida de `NewGamePlusRules`), XP de abate ×1,5,
  missões regionais de novo (XP sem escala); o herói começa com o nível do fim da NG+0 e o equipamento das
  Cavernas (drops novos ignorados: conservador).
- O modelo ganhou: Defense e MaxHealth percentuais, ResourceCostReduction, ManaRegen percentual e um bônus
  de velocidade de ataque só no começo da luta. Sem nenhuma dessas fontes ele dá exatamente a saída do r8.
- Limite: o passo de 20 ms arredonda o intervalo do básico, então +2% e +3% de velocidade de ataque dão o
  mesmo resultado.

### 14.2 Diagnóstico (antes do ajuste)

Média dos 5 heróis, segundos "no jogo". Meta do r8: comuns 4–7 s, chefes 15–22 s.

| Ciclo | Bioma | Chefe: r8 → pilha | Comuns: r8 → pilha | Só básico com a pilha: mata / chefe mata em |
|---|---|---|---|---|
| NG+0 | Floresta | 16,5 → **12,2** (−26%) | 4,8 → 3,4 | 17,4 / 21,8 |
| NG+0 | Deserto | 18,2 → **12,4** (−32%) | 6,1 → 3,4 | 16,8 / 23,0 |
| NG+0 | Montanha | 18,4 → **12,2** (−34%) | 6,4 → 3,9 | 18,0 / 25,2 |
| NG+0 | Cavernas | 20,0 → **13,2** (−34%) | 4,6 → 2,7 | 20,9 / 29,6 |
| NG+1 | Floresta | 14,3 → 9,4 (−34%) | 2,2 → 1,0 | 16,0 / 50,0 |
| NG+1 | Cavernas | 33,4 → 23,3 (−30%) | 7,4 → 4,9 | 35,3 / 25,9 |

- A pilha deixava **todo chefe abaixo de 15 s** e quem só usava o ataque básico vencia com folga (a regra
  do r8 "só no básico o chefe mata primeiro" caía em todos os biomas).
- Três peças faziam quase tudo (efeito sozinho no chefe da NG+0): **Canivete do Vô Chico** +2 de dano
  plano (−10 a −13%: num básico de 10, +2 é +20%), **Cantoria** +15% de dano (−12 a −14%) e **Rapadura**
  +25% de velocidade de ataque por 20 s (−11 a −13%: cobre a luta inteira de um chefe). As outras
  relíquias somadas pesam ~4%; café, pamonha, baião e cuscuz quase nada no tempo para matar.
- O lado defensivo não quebra nada: com a pilha mais defensiva (baião/pamonha + chuva de caju/romeiros)
  o chefe leva 37–66 s para matar o herói parado, mas é exatamente para isso que defesa serve.

### 14.3 Orçamento e mudanças aplicadas (antes → depois)

Orçamento da pilha completa (`power_stack.violations`, travado no pytest): no máximo **~22% mais rápida**
contra chefes e **~30%** contra comuns do que o herói do r8 (o p90 de sorte no loot já dá ~15%, §10.4); na
NG+0, quem só usa o básico vence no máximo "por um fio" (mata em ≥ 90% do tempo que o chefe leva para matá-lo);
o chefe mata o herói parado em 12–40 s.

| Onde | Campo | Antes | Depois |
|---|---|---|---|
| `RelicDefinition` (`relic_canivete_vo_chico`) | AttackDamage plano | +2 | **+1** (o mínimo de um afixo Afiada) |
| `WorldEventCatalog` (Cantoria ao pé da fogueira) | AttackDamage % na próxima luta | +15% | **+8%** |
| `MealCatalog` (Rapadura) | AttackSpeed nos primeiros 20 s | +25% | **+12%** (o teto do `affix_nimble`) |

Sem mudança: as outras 11 relíquias e o ×2 do dono, as outras refeições e favoritos, os outros eventos
(crítico +5% do eco só existe nas Cavernas e perde para a Cantoria), o carinho, os coeficientes da NG+
(§14.4) e todos os inimigos (as invariantes do r8 continuam valendo sem pilha). Textos: `SecretTexts`,
`KitchenTexts`; o texto da carta do evento sai do valor. Testes: `WorldEventTests` ("+8% de dano").

### 14.4 Resultado (depois)

**Orçamento de poder** (média dos 5 heróis, segundos "no jogo"; pilha = melhor refeição + relíquias + Cantoria):

| Ciclo | Bioma (níveis comuns / chefe) | Relíquias | Chefe: r8 → só relíquias → pilha | Comuns: r8 → pilha | Só básico com a pilha: mata / chefe mata em | Refeição mais usada |
|---|---|---|---|---|---|---|
| NG+0 | Floresta (2 / 3) | 3 | 16,5 → 15,3 → **13,8** (−16%) | 4,8 → **3,8** (−22%) | 19,9 / 21,8 | café coado |
| NG+0 | Deserto (5 / 7) | 6 | 18,2 → 17,3 → **14,9** (−18%) | 6,1 → **4,8** (−22%) | 22,4 / 23,0 | rapadura |
| NG+0 | Montanha (9 / 10) | 9 | 18,4 → 16,6 → **14,4** (−22%) | 6,4 → **4,9** (−24%) | 23,4 / 25,2 | rapadura |
| NG+0 | Cavernas (13 / 15) | 12 | 20,0 → 18,4 → **16,2** (−19%) | 4,6 → **3,5** (−24%) | 27,4 / 29,6 | rapadura |
| NG+1 | Floresta (16 / 17) | 12 | 18,5 → 16,4 → **14,6** (−21%) | 3,3 → **2,5** | 25,0 / 26,7 | queijo coalho |
| NG+1 | Deserto (17 / 18) | 12 | 18,5 → 16,4 → **14,5** (−22%) | 3,6 → **2,9** | 24,7 / 26,9 | rapadura |
| NG+1 | Montanha (19 / 20) | 12 | 18,5 → 16,4 → **14,5** (−22%) | 4,1 → **2,9** (−29%) | 24,6 / 27,5 | rapadura |
| NG+1 | Cavernas (21 / 22) | 12 | 19,8 → 17,9 → **15,9** (−20%) | 4,0 → **2,9** | 26,8 / 26,9 | rapadura |

NG+1 com a NG+ do r8 (×1,8 / ×1,5 em todo lugar), para comparar: chefe sem pilha 14,3 / 23,0 / 25,9 / 33,4 s,
comuns 2,2 / 4,6 / 6,7 / 7,4 s, herói parado morre em 45 / 30 / 27 / 24 s.

- **NG+0**: a pilha completa vale −16 a −22% no chefe e −22 a −24% nos comuns, dentro do orçamento. Os
  chefes ficam em 13,8–16,2 s (a meta absoluta é 15–22 s para o herói sem pilha; quem junta tudo fica um
  pouco abaixo, como quem tira uma arma de sorte). Só no básico, o herói com tudo vence por um fio (mata em
  92–97% do tempo que o chefe leva) em vez de folgado (71–80% antes). A Montanha fica a 0,5 pp do teto
  do orçamento: a próxima fonte de dano precisa sair de algum lugar.
- **NG+1 (escala por região, §11.1)**: sem pilha todo chefe fica em 18,5–19,8 s e o herói parado morre em
  25–26 s em qualquer região (antes: 14–33 s e 24–45 s). Com a pilha, −20 a −22%, dentro do orçamento. Os
  comuns ficam curtos (3,3–4,1 s sem pilha): em dupla, as skills de área do nível ~20 os pegam juntos; subir
  mais a vida deles alongaria os chefes também.
- Golpe em dupla e lojas novas: rodar `power_stack.py` de novo quando entrarem. Uma fonte nova de dano
  plano ou percentual deve caber no orçamento acima (o pytest falha se não couber).

### 14.5 Testes

`tools/balance/test_power_stack.py` (pytest, via `uv`): fontes lidas do C# (6 refeições, 12 relíquias,
dono ×2, eventos, NG+), os valores do ajuste, carinho sem stat de combate, frações com base 0 (§7.6),
vida/defesa percentuais, bônus de abertura só nos seus segundos, relíquias por região e bônus do dono,
refeições por região, escala de inimigos da NG+ por região, **chefes da NG+1 e NG+2 em 15–22 s e a no máximo
4 s um do outro**, herói parado da NG+1 na faixa de 12–40 s, a pilha nunca alonga uma luta, **orçamento da pilha na
NG+0 e na NG+1** e chefe com a pilha ≥ 13,5 s na NG+0.
C#: `ZeroBaseFractionStatTests` (§7.6) e `WorldEventTests` com o novo texto da Cantoria.

---

## 15. Golpe em dupla (parceiro de encontro) (2026-10-29)

Um primo chamado da vila entra na luta para **um** golpe combinado por encontro. Regras puras em
`DuoStrikeRules` / `DuoStrikeMeter` (`Assets/Scripts/Gameplay/Duo/`), combos em `DuoComboCatalog`, testes em
`DuoStrikeTests`. Executor na arena: `DuoStrikeController` (auto-bootstrap, sem editar cena).

### 15.1 Regras

```
carga   += min(0,25, dano causado / vida máx. do alvo × (chefe ? 2,0 : 0,5))   // 50% de um chefe, ou 200% de um comum
carga   += min(0,25, dano sofrido / vida máx. do herói × 1,25)                // perder 80% da vida enche
pronto   = carga ≥ 1  e  2,5 s depois do início do encontro  e  ainda não usado neste encontro
dano     = OutgoingDamage.Resolve(60 × combo.mult, damage_mult(nível), buffs do herói)   // mesmo pipeline das skills
stun     = combo.stun × (chefe ? 0,5 : 1)       // StunRules, via BuffController.ApplyStun (IStunDurationModifier)
empurrão = combo.força × (chefe ? 0,35 : 1)     // KnockbackReceiver; força 5 ≈ 1,6 m num comum, 0,2 m num chefe
cura     = combo.cura × vida máx. do herói
```

| Constante | Valor | Por quê |
|---|---|---|
| `BaseDamage` | **60** | Entre a skill ativa mais forte (Explosão Arcana, 40) e a Ultimate mais fraca (Exército Mecânico, 100): ~1,5–2 ativas, ~60% de uma Ultimate. Teste: `Damage_SitsBetweenTheActiveSkillsAndTheUltimates`. |
| `MaxUsesPerEncounter` | **1** | É um momento da luta, não uma skill de rotação. A carga zera ao usar e não volta a subir. |
| `ArmDelaySeconds` | 2,5 s | Cobre o "Prepare-se!" (inimigos parados ~2 s): não dá para abrir a luta com ele. |
| `CooldownSeconds` | 45 s | Só vale se um modo liberar mais de um uso (NG+, grupo online). |
| `MaxChargePerHit` | 0,25 | Um crítico enorme ou um golpe de chefe não enchem a barra sozinhos. |
| `BossKnockbackMultiplier` | 0,35 | Mesma ideia do stun ×0,5: chefe quase não sai do lugar. |
| Duração | 1,5 s (impacto aos 0,45 s) | O parceiro aparece, golpeia e some; o herói continua jogável. |

A carga é **relativa à vida do alvo/herói**, não a números absolutos, então o momento em que ela enche não muda
com nível, equipamento nem NG+.

### 15.2 Quando fica pronto

- **Chefe** (lutas médias de 16,5–20 s, §9.4): só causando dano, enche com 50% da vida do chefe (~8–10 s);
  apanhando junto, ~35–45% da luta. Pensado para ser o "agora!" do meio da luta.
- **2 comuns** (4,6–6,4 s): enche perto do fim (200% da vida de um comum = os dois inteiros), mais cedo se o herói
  apanhar. Vira um finalizador; em grupos de 3 (elite, emboscada) fica pronto no meio.

### 15.3 Dano contra o chefe da região (rota com missões, §9.4)

`damage_mult` no nível do chefe (3 / 7 / 10 / 15) = 1,04 / 1,12 / 1,18 / 1,28. Combos: 0,7× (mais fraco),
1,0× (típico) e 1,4× (Flecha-Trovão). Sem Poder Arcano/refeição/crítico (esses somam igual às skills).

| Chefe | Vida | 0,7× | 1,0× | 1,4× | Tempo médio de luta tirado (1,0×) |
|---|---|---|---|---|---|
| Lobo Alfa | 520 | 44 (8%) | 62 (12%) | 87 (17%) | ~2,0 s de 16,5 s |
| Guardião da Ruína | 780 | 47 (6%) | 67 (9%) | 94 (12%) | ~1,6 s de 18,2 s |
| Senhor da Guerra | 900 | 50 (6%) | 71 (8%) | 99 (11%) | ~1,5 s de 18,4 s |
| Habitante Ancestral | 1 180 | 54 (5%) | 77 (7%) | 108 (9%) | ~1,3 s de 20,0 s |

Mesmo com o golpe, a média do chefe fica em 14,5–18,7 s (meta 15–22 s; piso da pilha da §14: 13,5 s). Contra
comuns (70–170 de vida) ele mata ou quase mata a área toda — é o papel de finalizador.

### 15.4 Os 10 combos (um por par, busca sem ordem: Maithe+Ícaro = Ícaro+Maithe)

Falas: o primo mais velho fala primeiro, depois o mais novo (ordem de nascimento), seja quem for o herói.

| Par | Combo | Elem. | Dano | Raio | Stun (comum/chefe) | Cura | Empurrão | Buff / debuff |
|---|---|---|---|---|---|---|---|---|
| **Maithe + Ícaro** (irmãos) | **Flecha-Trovão** | Elétrico | 1,4× | 6 m | 1,2 / 0,6 s | — | 5 | herói +15% crítico 6 s |
| **Maria + Joaquim** (irmãos) | **Bastião Sagrado** | Sagrado | 1,0× | 5,5 m | — | 30% | 7 | herói +40% defesa 8 s |
| Maithe + Maria | Flechas da Aurora | Sagrado | 1,0× | 5 m | — | 15% | — | — |
| Maithe + Joaquim | Salto da Muralha | Terra | 1,15× | 4,5 m | — | — | 5 | herói +20% crítico 6 s |
| Maithe + Lavine | Lua Caçadora | Trevas | 1,2× | 4,5 m | — | — | — | inimigos −40% velocidade 4 s |
| Maria + Ícaro | Drone Abençoado | Elétrico | 0,7× | 4,5 m | — | 20% | — | herói +3 vida/s 6 s |
| Maria + Lavine | Crepúsculo Sereno | Arcano | 0,9× | 4,5 m | 1,0 / 0,5 s | 15% | — | — |
| Joaquim + Ícaro | Terremoto Elétrico | Terra | 1,1× | 5 m | 1,0 / 0,5 s | — | 4 | — |
| Joaquim + Lavine | Correntes de Pedra | Trevas | 0,9× | 4,5 m | 1,4 / 0,7 s | — | — | herói +25% defesa 6 s |
| Ícaro + Lavine | Curto-Circuito Arcano | Arcano | 1,3× | 5 m | — | — | — | herói +20% vel. de ataque 6 s |

**Laço de irmãos:** os dois pares de irmãos têm o maior "poder" (`DuoComboSpec.PowerScore`, teste exige ≥ 15% acima
do melhor combo de primos), anel de efeito duplo (o segundo na cor do parceiro), silhueta quase dourada, tremor de
câmera mais forte e a faixa "Laço de irmãos!". Os stuns ficam ≤ 1,5 s (no chefe ≤ 0,75 s, abaixo das recargas de
2,4–3,3 s dos chefes) e as curas ≤ 30% (Cura Divina nível 1 = 35 de vida).

Parceiro padrão (save antigo / Novo jogo): o irmão ou irmã; Lavine chama a Maria. A escolha fica no menu de pausa
(mapa e arena), uma por herói, salva em `DuoPartnerState` (chave `"DuoPartner"`).

### 15.5 Acessibilidade e desempenho

- **Reduzir Flashes:** sem sprite de flash no anel (`SkillVfxFlashPolicy`), silhueta mais apagada e sem brilho aditivo,
  sem pulso dourado, sem hit-stop, faixa sem fade.
- **Reduzir Movimento:** o parceiro aparece e some no lugar (sem avanço nem braços), sem tremor de câmera, sem hit-stop,
  faixa sem fade.
- **Silhueta em vez do modelo do primo:** 5 malhas primitivas com 1 material unlit compartilhado, sem sombra, sem
  skinning, sem `Resources.Load` no meio da luta. Um segundo herói riggado custaria um FBX carregado no clímax da luta,
  mais um SkinnedMeshRenderer + animador procedural (~2× o custo do herói por 1,5 s) no celular.

### 15.6 Online (Fase 12)

No online o "parceiro" vira um **membro real do grupo**: a barra de carga é por jogador; quando dois membros com a
barra cheia estão a ≤ 12 m, o primeiro aperta "Dupla" e o outro tem ~2 s para confirmar (o mesmo botão). O servidor
valida a dupla, resolve o combo pelo par de personagens dos dois (`DuoComboCatalog.Find`), aplica dano/stun/cura com
as mesmas regras e consome um uso de cada um. Sem confirmação (ou jogando sozinho) cai no golpe com o primo-fantasma
de hoje. Regras que precisam ir para o servidor: `DuoStrikeRules` inteiro (carga relativa, limite por encontro,
stun ×0,5 e empurrão ×0,35 em chefe); o cliente só mostra a silhueta / o modelo real do companheiro de grupo.

---

## 16. Dia e noite: ondas que renascem, bichos noturnos e visão noturna (2026-10-29)

Pedido: dia e noite com bichos diferentes, que renascem, e a noite mais perigosa (e com menos visão). Desenho completo
e tabela das espécies futuras em `Docs/DayNight.md`. Código em `Assets/Scripts/Gameplay/Night/`; testes em
`NightDangerTests`. **Estimativa, ainda não passou pelo modelo do r8/r9** (`tools/balance/balance_model.py`).

### 16.1 Multiplicadores por fase (`NightDangerRules`)

| Fase | Ondas (total) | Intervalo entre ondas | Tamanho do reforço | Chance de variante (1ª onda) |
|---|---|---|---|---|
| Dia (7h–18h) | 2 (mapa + 1) | 4,0 s | ×1,0 | 0% |
| Amanhecer (5h–7h) | 2 | 3,5 s | ×1,0 | 10% |
| Entardecer (18h–20h) | 2 | 3,5 s | ×1,0 | 15% |
| Noite (20h–5h) | **3** (mapa + 2) | **2,5 s** (ritmo ×1,6) | **×1,5** (arred., 1–4) | **50%** |

Reforço = nº de comuns da 1ª onda × tamanho, entre 1 e 4. A fase usada é a do momento em que a onda anterior cai
(troca de fase no meio da luta vale na próxima onda). Segura de ataque do reforço: 1,75 s × 1,0 (dia) / × 0,6 (noite).
**Sem reforços:** desafio de chefe, grupo com chefe, emboscada de elite e a 1ª luta (tutorial). Chefes: sem mudança.

### 16.2 Variante noturna e espécies (`NightVariantRules`)

| | Vida | Dano (AttackDamage %) | Raio de percepção | Tamanho | XP ao matar | Saque extra |
|---|---|---|---|---|---|---|
| Variante "(noturno)" | ×1,35 | +20% | ×1,4 | ×1,1 | ×1,5 | +1 rolagem (ouro + tabela do bioma) |
| Lobisomem-do-sertão (Floresta, base Lobo da Mata) | ×5 (350) | +60% | ×1,6 | ×1,6 | ×5 (+40) | +3 rolagens |
| Morcego-vampiro gigante (Cavernas, base Enxame) | ×1,8 (306) | +30% | ×1,5 | ×1,35 | ×1,8 (+32) | +1 rolagem |

O XP/saque normal do inimigo base continua vindo de `EnemyRewardGranter`/`EnemyLootGranter`; o bônus é pago à parte
(`NightVariantTag`, como o elite do §13.3). Os stats da variante são sempre os da noite, mesmo se ela surgir no
amanhecer/entardecer.

### 16.3 Impacto esperado por encontro (1ª onda de 1–2 comuns, média 1,5)

| | Dia (antes) | Dia (agora) | Noite (agora) |
|---|---|---|---|
| Inimigos por encontro | 1,5 | ~3,0 | ~6,5 (1,5 + 2 × 2,5) |
| Fração de variantes/espécies | 0 | 0 | ~50% na 1ª onda, ~70% nos reforços |
| Vida total a derrubar (em "comuns") | 1,5 | ~3,0 | ~9 |
| XP por encontro (em "kills comuns") | 1,5 | ~3,0 | ~8,5 |
| Encontros por passo no mapa | ×1 | ×1 | ×1,25 (já existia) |

Ou seja: **o dia agora rende ~2× XP/saque por encontro em ~2× o tempo** (XP por minuto de luta ≈ igual), e a noite
rende ~5–6× por encontro em ~6× o tempo, com mais dano recebido (mais bichos ao mesmo tempo, +20% de dano e chegada
mais rápida). Para o r10: rodar o modelo com as ondas; se a curva de XP andar rápido demais, o ajuste mais simples é
`Day.ReinforcementWaves = 0` (dia volta a ser 1 onda) ou reduzir `ExperienceMultiplier` da variante para ×1,25.
Interruptor de emergência no Inspector: `EncounterArenaController._reinforcementWaves`.

### 16.4 Visão noturna (`NightVisibilityRules`) — não muda números de combate

| | Normal | "Noite mais clara" | Cavernas |
|---|---|---|---|
| Luz ambiente na noite cheia | ×0,70 | ×0,90 | ×0,88 |
| Neblina extra (arena) | +0,022 | +0,010 | +0,008 |
| Corte da câmera (arena) | 70 m | 120 m | 70 m |
| Lamparina | 8 m (teto 16 m com itens/montaria) | igual | sempre acesa (≥60%) |

Piso absoluto da luz ambiente ×0,6; a câmera nunca corta antes de (distância até o herói + 40 m).

### 16.5 Testes

`NightDangerTests`: noite mais perigosa que o dia em todos os eixos, amanhecer/entardecer no meio, temporização das
ondas (incl. troca de fase no meio da luta), tamanho com teto, travas (chefe, elite, tutorial), sorteio de variante
determinístico (~50% à noite, 0 de dia), toda região com entradas em toda fase, toda entrada resolve um inimigo comum
da própria região, tabela do dia = tabela do bioma, noite majoritariamente noturna, espécies só à noite e com teto por
onda, regras de XP/nome "(noturno)", `NightVariantTag` (vida, dano, tamanho, olhos, recolor sem mexer no material
compartilhado, idempotente), visibilidade (luz, neblina, câmera, lamparina, opção "Noite mais clara" salva) e a regra
do indicador do HUD.

## 17. Cidades: lojas temáticas, rotação e carro de boi (2026-10-29)

Toda região tem uma cidade (TODO 12.3a): Vila da Vó Zefa (Floresta), Povoado de Areia Branca (Deserto), Vila da
Serra Alta (Montanha) e Vila das Lamparinas (Cavernas). Dados em `CityCatalog`/`CityShopCatalog` (código), regras puras
em `SafeZoneRules`, `ShopRotation` e `FastTravelRules` (`CityTests`), estado salvo por slot em `CityProgress`
(chave `Cities`). Projeto e regras de zona segura em `Docs/Cities.md`.

### 17.1 Preços das lojas

Só itens que já existem (`Resources/Items`). Preço sempre entre **80% e 125%** do `ItemDefinition.Value` (teste):
a especialidade da cidade vende o seu tema ~10% mais barato, o Mascate cobra ~20% a mais (vem de longe), a Feira
vende comida um pouco mais barata. A loja paga 50% na revenda, como a Loja Geral.

| Loja (cidade) | Tema | Rotação | Sempre no balcão | Sorteados (vagas) | Destravam com a campanha |
|---|---|---|---|---|---|
| Mercearia do Seu Lázaro (Vila) | mercearia | diária | carne assada 6, Poção de Vida 15 | erva 3, madeira 2, fibra 1 (2) | Poção de Mana 16 — Deserto aberto |
| Armarinho da Dona Cida (Vila) | armarinho | semanal | fibra 1, botas de couro 10 | capacete 12, peitoral 20, erva 3 (2) | Arco de Caça 38 — chefe da Floresta |
| Feira livre (Vila) | feira | diária | erva 3 | madeira 2, fibra 1, pedra 1, carne assada 5 (3) | minério de ferro 6 — Montanha aberta |
| Selaria do Seu Vavá (Areia Branca) | especialidade: couro | semanal | botas 9, capacete 11, peitoral 18 | Poção de Vida 16, carne 7, fibra 1 (2) | Arco de Caça 33 — chefe do Deserto |
| Forja do Mestre Tonho (Serra Alta) | ferreiro | semanal | ferro 7, minério 5, picareta 18 | martelo 15, machado 18, espada de madeira 10, Poção de Vida 17 (2) | Espada de Ferro 41 — chefe da Montanha |
| Lamparinas da Benedita (Lamparinas) | especialidade: botica | diária | Poção de Vida 15, Poção de Mana 13 | erva 3, minério 6, pedra 1, capacete 12 (2) | Espada de Ferro 45 — chefe das Cavernas |
| Mascate Seu Bento (uma cidade por dia) | mascate | diária | — | poções 18, carne 7, peitoral 24, martelo 18 (3) | Arco 42 (chefe da Floresta), Espada de Ferro 54 (chefe do Deserto) |

A Loja Geral antiga (`shop_general_store`: madeira, pedra, ferro) continua na Vila, sem mudança.

Estoque: cada linha tem um estoque por período (o "do dia"), e quando o período vira o estoque da loja volta ao
cheio (`ShopManager.RestockShop`); a reposição geral do amanhecer do jogo (§ "Reposição" do `ShopManager`) continua
valendo também. Com ~1,8 ouro por luta (§8), uma Poção de Vida custa ~8 lutas e a Espada de Ferro ~23: as lojas
completam o loot, não substituem.

### 17.2 Rotação (`ShopRotation`)

- Período = dia do calendário (diária) ou semana de segunda a domingo (semanal), contado a partir de 05/01/2026.
- Balcão = linhas fixas destravadas + N sorteadas entre as rotativas destravadas, com semente `FNV(loja) × período`
  (mesmo dia, mesmo balcão em qualquer aparelho; o servidor online pode usar a mesma conta).
- Relógio atrasado não sorteia de novo um balcão antigo: vale o maior período já visto (salvo em `Cities`).
- O Mascate fica, a cada dia, em uma das cidades com região aberta (sorteio pela data); nesse dia ele aparece como
  morador e a loja dele entra na lista de serviços.

### 17.3 Carro de boi (`FastTravelRules`)

- Só entre cidades **já visitadas** (chegar a pé com o token parado no nó; a Vila conta desde o início; save antigo
  conta toda região onde já houve vitória) e de região **aberta**.
- Custo: **2 ouro por estrada** do grafo do mapa (Floresta → Deserto 2, Floresta → Cavernas 6 ≈ 3 lutas de ouro).
  Sem encontro e sem evento de estrada no caminho; o jogo salva ao chegar.

### 17.4 Zona segura no mapa e efeito nos encontros

- A cidade é o círculo de **1,5 m** (`SafeZoneRules.MapCityRadius`, a praça plana do nó) em volta de cada nó.
  Ali dentro os passos não contam e não sai encontro aleatório nem evento de estrada.
- Uma viagem entre nós vizinhos tem ~10 m, então ~3 m (início e fim) agora são seguros: **~30% menos passos de
  encontro por viagem**. "Explorar" e "Enfrentar o chefe" não mudam (são saídas da cidade de propósito), então o ritmo
  de lutas de quem explora não muda. Se a progressão ficar lenta para quem só viaja, o botão é `MapCityRadius`.
- Derrota: o herói acorda na cidade da região onde perdeu (a região atual vira essa), com aviso no HUD.
- A arena (Main.unity) não tem cidade: lá tudo é "fora da cidade" e nada mudou.

### 17.5 Testes

`CityTests`: matriz de ataque/dano/perseguição da `SafeZoneRules` (cidade × fora, NPC, PvP, a si mesmo), trava de
PvP, renascimento; cidade por região com 3–4 moradores e loja; textos em pt-BR e inglês com fala por primo; lojas
temáticas com dono, itens existentes e preço na faixa; períodos, semente, rotação, travas de campanha, relógio
atrasado, Mascate por data; balcão de hoje no `ShopManager` com reposição na virada; carro de boi (estradas, custo,
ordem das checagens, linhas da janela); visitas, save antigo, ida e volta do save, registro no GameManager e novo jogo;
botão da cidade, indicador do HUD e a geometria do anel/casas no mapa.

## 18. Mochila em grade (estilo MU) e queda de item ao morrer (2026-10-30)

O limite de **peso** da mochila saiu; o limite agora é a **grade** de 8 colunas × 8 linhas (64 casas), como o
inventário do MU Online. Um item só entra se houver uma área livre do tamanho dele (ou uma pilha comum dele com
espaço). Código: `ItemFootprint`, `BagGrid`, `BagGridRules`, `BagFitSimulator` (`Gameplay/Inventory/BagGrid.cs`) e
`InventoryManager`; queda ao morrer em `Gameplay/Inventory/DeathDrop.cs`.

### 18.1 Tamanho dos itens (por categoria; `_gridWidth`/`_gridHeight` no item sobrescrevem)

| Categoria | Casas | Exemplos atuais |
|---|---|---|
| Madeira, Pedra, Metal, Ouro, Erva, Fibra | 1×1 | madeira, pedra, ferro, minério, erva, fibra (pilha 50–99) |
| Comida, Poção | 1×1 | carne assada, poção de vida/mana (pilha 20); peixes/ingredientes entram aqui |
| Acessório | 1×1 | anel, amuleto |
| Moeda, Item de missão, Planta, Diversos | 1×1 | ouro (pilha 9999) |
| Arma corpo a corpo | 1×3 | espada de madeira, espada de ferro |
| Arma à distância | 2×4 | arco de caça |
| Arma mágica | 1×4 | (cajados futuros) |
| Cabeça, Pernas, Botas | 2×2 | chapéu de couro, botas de couro |
| Peito | 2×3 | gibão de couro |
| Ferramenta de coleta | 1×3 | machado, picareta |
| Ferramenta de construção | 1×2 | martelo |
| Escudo (sem categoria própria) | 2×2 | use o override (`ItemFootprint.Shield`) |

Regra de pilha: tudo que empilha ocupa 1 casa por pilha (até `MaxStack`). Item gerado (raridade/afixos) nunca
empilha com o comum. Nada gira (o MU também não gira): **rotação ficou de fora** de propósito.

Ordem de busca: da casa de cima à esquerda, linha por linha (MU). Arrastar/soltar: área livre = move; pilha igual =
junta; um item no caminho = troca (o outro vai para onde o primeiro estava, ou a primeira área livre); dois ou mais
itens no caminho = não cabe (fantasma vermelho). "Organizar": ordena (tipo → raridade → nome) e empacota a partir do
canto; se não couber nessa ordem, tenta maior-primeiro; se nem assim, não mexe.

### 18.2 Peso: dormente, não apagado

`ItemDefinition.Weight` e `InventoryManager.GetTotalWeight()` continuam (dados e saves intactos, conta só informativa).
`HasWeightCapacityFor` e `AddItemIgnoringWeight` viraram nomes antigos para o novo encaixe (`CanFit` / `AddItem`).
`LootGrantResult.KeptOverWeight` fica sempre 0 (o "item de missão passa do peso" não existe numa grade). Motivo:
apagar o campo mexeria em 20 assets e em código de outros sistemas sem ganho; deixá-lo dormente é o caminho de menor
risco e permite, no futuro, uma regra de "carga" (ex.: lentidão) se o design quiser.

API única de encaixe (todo "cabe?" passa por ela): `InventoryManager.CanFit(item, qtd)`, `GetFittableAmount`,
`CanFitAll(lista)`, `CanFitCopy(entrada)`, `TryAddCopy(entrada)`. Usada por: loot (`LootGrantRules`, toast "Mochila
cheia"), compra na loja (`ShopManager.BuyItem`), baú (`VillageChestTransfer`), criação (`CraftingManager`, que agora
desfaz o resultado parcial antes de devolver os ingredientes), recompensas de evento (`WorldEventOutcomes` →
`LootGrantRules`), segredos (ouro pendente). Recompensa de missão que não couber vai para o Baú.

Quanto enche: materiais empilham, então um bioma enche a grade em centenas de lutas; quem enche é equipamento (arco 8
casas, peitoral 6). `LootOverflowTests.EveryBiome_TakesAtLeast40Fights_ToFillTheGrid` guarda o mínimo de 40 lutas.

### 18.3 Save e migração

Cada entrada salva `gridX/gridY`; `InventorySaveState.gridVersion = 1` marca saves da grade, `extraRows` a expansão.
Save antigo (`gridVersion` 0, lista de 30 espaços sem posição): o jogo **auto-posiciona** tudo (ouro e itens de missão
primeiro, depois os maiores, desempate pela ordem antiga). O que não couber vai para o **Baú da Vó Zefa** — nunca
some. Como o baú carrega depois da mochila no mesmo load, ele recebe numa "caixa de entrada"
(`VillageChestStorage.EnqueueOverflow`) salva junto com o baú e despejada nos espaços livres no próprio load, ao salvar,
ao abrir o baú e ao tirar algo dele. Save de grade com posição inválida (ex.: o tamanho de um item mudou) reposiciona
só o item quebrado.

### 18.4 Expansão (opcional)

`InventoryManager.AddBagRows(n)`: até +2 linhas (`MaxExtraRows`), salvo em `extraRows`. Vendido como **Bornal de couro**
(`item_bornal_couro`, +2 linhas, uma vez por save): 150 no Armarinho da Dona Cida (freguesia Freguês) ou 180 no Mascate
(após o Lobo Alfa). Ver `Docs/Cities.md` §4.3.

### 18.5 Queda de item ao morrer (9%)

- `DeathDropRules.DropChance = 0.09`: **um sorteio por morte** (tela de morte), semente injetável (testado).
- Sucesso: **uma** entrada da mochila elegível, sorteada por igual, perde **exatamente 1 unidade**
  (`DeathDropRules.DropQuantity = 1`): de uma pilha de 6 poções cai 1 e ficam 5; um item único sai inteiro.
- Nunca cai: equipado (não está na grade), ouro/moeda, item de missão/história, relíquias (hoje nem são itens; id
  `relic_*` reservado) e item marcado **vinculado** (`_isBound`).
- **Patuá da Vó Edna** e **Bênção da Romaria** cancelam o sorteio (0%, `DeathPenaltyRules.AllowsItemDrop`, §19).
- **Decisão do usuário (2026-10-30): item perdido não se recupera.** Offline a unidade sai da mochila **de vez** e a
  tela diz "Na pressa, você deixou cair: <item>" + "Perdeu de vez. Um Patuá da Vó Edna ou a Bênção da Romaria teria
  guardado." Não existe mais resgate ("Achados e Perdidos" foi removido). Saves antigos que ainda trazem a lista
  `lostAndFound` no save do baú carregam normalmente; a lista é ignorada (`DeathDropTests.OldChestSave_...`).
- Em média: 1 unidade a cada ~11 mortes desprotegidas. Morrer não custa ouro.

### 18.6 Online (FASE 12) — decidido

- **Decisão do usuário: qualquer jogador pode pegar o item que caiu de outro (estilo MU).** A unidade cai no chão
  onde o herói morreu (servidor autoritativo roda as mesmas regras puras: os 9%, a entrada sorteada e só 1 unidade;
  `ItemDroppedOnDeathEvent` vira o drop no chão).
- Proposta de detalhe (a confirmar no servidor): o dono tem prioridade de alguns segundos (ex.: 10 s) antes de
  liberar para todos; o item some do chão depois de N minutos (ex.: 2 min) — e aí está perdido de vez, como offline.
  Itens vinculados nunca caem.
- Zona segura da cidade: morrer lá não acontece (sem combate), então nada cai em cidade.

## 19. Perda de XP ao morrer, Bênção da Romaria e Patuá da Vó Zefa (2026-10-30)

Decisão do usuário (Docs/Referencias_MMO.md §6): **morrer perde XP**, pouco, e a **Bênção da Romaria** e o **Patuá da Vó
Zefa** protegem. Código: `DeathXpLossRules`, `DeathPenaltyRules`, `DeathPenaltyService` (`Gameplay/Inventory/DeathPenalty.cs`),
`DeathProtectionState`, `RomariaBlessingRules`, `RomariaOratory`, `PatuaRules` (`Gameplay/Inventory/RomariaBlessing.cs`);
testes em `DeathPenaltyTests`. Aplicado pela tela de morte (`DeathScreenController`), junto com a queda de 9% (§18.5).

### 19.1 Quanto se perde

- **5% do XP que o nível atual pede** (`xp_to_next`), arredondado. **Nunca desce de nível** (decisão para crianças):
  a perda só sai do XP já ganho dentro do nível; a barra pode esvaziar, o número do nível não volta.
- **Sem perda abaixo do nível 3** (primeiras lutas/tutorial: quem ainda está aprendendo não é punido).
- **Sem perda no desafio do chefe da região na primeira campanha** (NG+0). Motivo: o chefe é onde a criança mais
  morre e tenta de novo; perder XP a cada tentativa vira castigo. Na Nova Jornada+ (ciclo ≥ 1) o chefe custa XP
  normalmente. A queda de 9% continua valendo no chefe (já decidida em §18).
- Sem a tela de morte (respawn instantâneo antigo) não há perda, igual à queda de item.

| Nível | `xp_to_next` | Perda (5%) | Com Bênção (−60%) | XP por encontro na época (§10) | Perda em encontros |
|---|---|---|---|---|---|
| 1–2 | 100–110 | 0 (carência) | 0 | 15 | 0 |
| 5 | 146 | 7 | 3 | 27 | ~0,3 |
| 9 | 214 | 11 | 4 | 42 | ~0,25 |
| 13 | 314 | 16 | 6 | 60 | ~0,25 |
| 20 | 612 | 31 | 12 | — | — |
| 50 | 10 672 | 534 | 214 | — | — |

Em média uma morte custa **~¼ de encontro** de XP: sente-se, mas não desanima.

### 19.2 Bênção da Romaria (oratório de cada cidade)

- Pedida no **oratório** de cada cidade (hub da cidade → Serviços): Capelinha da **Dona Fé** (Vila da Vó Zefa),
  Oratório da Cisterna (Dona Socorro, Areia Branca), Capela de Pedra (Dona Graça, Serra Alta), Oratório das
  Lamparinas (Benedita). Dado aditivo por cidade em `RomariaOratory.Oratories` (ids do `CityCatalog`).
- Dura **até a próxima morte** (não por tempo). Corta **60% da perda de XP** e põe a queda de item em **0%**.
- Preço: **5 + nível** de ouro, teto 60. Com ~1,8 ouro por luta (§8): nível 1 = 6 (≈3 lutas), nível 5 = 10 (≈6),
  nível 10 = 15 (≈8), nível 20 = 25 (≈14). Comparação: a Poção de Vida custa 15. Não acumula (comprar de novo com
  a bênção ativa não cobra). Ícone "Bênção" no HUD (`DeathProtectionIndicator`).
- Salva por slot (`DeathProtectionState`, chave `DeathProtection`; save antigo carrega sem bênção; "Novo jogo" zera).

### 19.3 Patuá da Vó Zefa (`item_patua_vo_zefa`)

- Item de mochila **1×1** (Diversos, override `_gridWidth/_gridHeight` = 1), pilha 5, raro, valor 100.
- **Gasta-se ao morrer** (1 unidade) e impede **toda** a perda de XP **e** a queda de item.
- **Precedência: o Patuá se gasta primeiro**; a Bênção fica para a próxima morte (nenhum dos dois se desperdiça e a
  regra é fácil de explicar: "com patuá na mochila, nada se perde").
- Proteções se gastam em qualquer morte da tela de morte, mesmo nas isentas de XP (carência/chefe NG+0), porque a
  queda de 9% continua possível lá.
- De onde vem: **presente da Vó Zefa** — 1 Patuá na primeira vez que cada chefe de região cai (4 por campanha, de
  novo a cada Nova Jornada+); vai para a mochila se couber (`InventoryManager.CanFit`), senão para o Baú da Vó Zefa
  (nunca some). **À venda** com o Mascate Seu Bento por **120 ouro** (rotativo, depois do chefe da Floresta; ≈ 67
  lutas de ouro). Queda rara em loot: ainda não (fica para o dono das tabelas; sugestão 0,5% em chefes).

### 19.4 Online (FASE 12)

- **Mesmas regras no servidor** (autoritativo): ele calcula a perda, gasta o Patuá/Bênção e sorteia os 9%.
- O item que cai **vai para o chão e qualquer jogador pode pegar** (já decidido em §18.6).
- A bênção é por personagem/conta no servidor; o preço segue o nível do personagem.

---

## 20. Refino de item +0 a +15: Pedras do Sertão (2026-10-30)

Refino estilo MU Online (FASE 16). Regras puras em `RefineRules` (`Assets/Scripts/Gameplay/Refining`), testadas em
`RefineTests`; aplicação no inventário em `RefineService`; janela "Ferreiro: refinar" no hub de toda cidade
(`VillageRefinePanel`, `Resources/UI/Refine.uss`). O nível fica na instância do item
(`InventorySlotData.refineLevel` / `refineOptionLevel`, também no `EquippedEntry` do save); save antigo carrega como +0.
**Decisão do usuário:** refino que falha volta um nível; o item nunca some.

### 20.1 Tabela de chance e custo (`RefineRules`)

| De → para | Pedra | Ouro | Chance | Com Amuleto da Sorte | Se falhar |
|---|---|---|---|---|---|
| +0 → +6 (cada) | Bênção | 0 | 100% | (não gasta) | — |
| +6 → +7 | Alma | 0 | 80% | 90% | fica +6 |
| +7 → +8 | Alma | 0 | 75% | 85% | volta +6 |
| +8 → +9 | Alma | 0 | 70% | 80% | volta +7 |
| +9 → +10 | Alma | 10 | 60% | 70% | volta +8 |
| +10 → +11 | Alma | 15 | 55% | 65% | volta +9 |
| +11 → +12 | Alma | 20 | 50% | 60% | volta +10 |
| +12 → +13 | Alma | 30 | 45% | 55% | volta +11 |
| +13 → +14 | Alma | 40 | 40% | 50% | volta +12 |
| +14 → +15 | Alma | 50 | 35% | 45% | volta +13 |

- Amuleto da Sorte: +10 pontos (teto 95%), gasto na tentativa; nunca usado em rolagem certa.
- Falha: −1 nível acima de +6 (+6 é o piso seguro, porque a Bênção sobe até lá sem falhar).
- Pedra da Vida: opção extra 0→4, 60% por pedra; falhar não muda nada.
- Pedra do Caos: só definida (item e drop); vai para o Tacho Encantado depois.
- Rolagem com semente (`RefineRules.Roll`, SplitMix64): mesmo resultado em qualquer plataforma. **Online, o servidor
  sorteia** (`RefineRules.Attempt` com a semente dele) e o cliente só mostra a resposta.

Custo esperado do +0 (cadeia de Markov com a volta de nível, `RefineRules.ExpectedCostTo`): +6 = 6 Bênçãos;
+9 ≈ 6 Bênçãos + 5,2 Almas; +12 ≈ 25 pedras e 138 de ouro (com amuleto em toda rolagem: 18 e 99); +15 ≈ 88 pedras e
1.440 de ouro (com amuleto: 40 e 706). +12 a +15 é meta de longo prazo (online), não da campanha.

### 20.2 O que cada nível dá

Curva no atributo base (dano da arma / defesa da armadura), acumulada: +1..+6 = 4% por nível (24%), +7..+9 = 6% por
nível (42%), depois 50 / 58 / 67 / 77 / 88 / **100%** (+15 dobra o atributo). O nome mostra o nível ("Espada de Ferro
+7"), a dica mostra o atributo já refinado e a linha "Refino +7: +30% no atributo base".

Em combate (`InventoryManager.ApplyRefineEffects`, chaves `equip_<slot>/refine/*`):
- **arma**: AttackDamage % = bônus da curva × **0,10** (`WeaponAttackShare`: a arma é só uma parte do golpe) + 1% por
  nível de opção. +9 com opção 2 = +6,2%.
- **armadura**: Defense fixa = a parte refinada da defesa da peça (couro 4/8/3 em +9: +6,3 no total) e vida máxima
  +0,5% por nível de opção, por peça.
- Brilho em +7 / +9 / +11 / +13 / +15 (5 níveis): só na interface (título da dica com sombra dourada, borda da linha
  na janela), porque nenhum item tem modelo no mundo ainda.

### 20.3 De onde vêm as pedras

Quarta fonte de loot (`RefineStoneDrops`, rolada pelo `EnemyLootGranter` depois das outras, então as sementes antigas
continuam iguais). Comum: chance por morte; chefe: chance × quantidade. NG+ aumenta a chance dos comuns em 25% por ciclo.

| Item (valor) | Floresta | Deserto | Montanha | Cavernas | Chefe (F / D / M / C) |
|---|---|---|---|---|---|
| Pedra da Bênção (12) | 6% | 8% | 9% | 10% | 2 / 2 / 3 / 3, sempre |
| Pedra da Alma (35) | — | 1,5% | 2,5% | 4% | — / 1 / 1 / 2, sempre |
| Pedra da Vida (40) | — | — | 1% | 1,5% | — / 25% / 35% / 50% |
| Pedra do Caos (50) | — | — | — | 1% | — / — / 15% / 25% |
| Amuleto da Sorte (25) | — | 1% | 1% | 1,5% | 10% / 15% / 20% / 25% |

Só na rota com missões (§9: ~19 / 8 / 10 / 14 comuns + 1 chefe por região), o esperado acumulado é: Bênção 3,1 → 5,8 →
9,7 → 14,1; Alma 0 → 1,1 → 2,4 → 4,9; Vida 1,4 e Amuleto 1,1 ao fim das Cavernas. Ou seja, a rota leva a arma a ~+6
no Deserto e ~+8 nas Cavernas; quem farma um pouco ou compra chega aos níveis do orçamento abaixo.

Lojas (preço entre 80% e 125% do valor, como §17): Forja do Mestre Tonho — Bênção 11 (sempre, 5/semana), Alma 32
(depois do chefe da Montanha, 2/semana); Lamparinas da Benedita — Amuleto 25 (rotativo); Mascate — Bênção 14 (Deserto
aberto) e Amuleto 30 (chefe do Deserto). Vida e Caos só caem. Com ~1,8 ouro por luta, uma Bênção comprada custa ~6
lutas: a loja completa o drop, não substitui.

### 20.4 Orçamento (`tools/balance/power_stack.py`, `test_refine_stack.py`)

Nível que o modelo põe em **todas** as peças (arma + chapéu + peitoral + botas) ao chegar em cada região — teto de
quem farmou: Floresta **+4**, Deserto **+6** (opção 1), Montanha **+7** (opção 1), Cavernas **+9** (opção 2); NG+1
mantém o equipamento das Cavernas, então +9 (opção 2). Regras:

- r8 + refino: chefe ≥ 92% do tempo do r8; 2 comuns ≥ 80% (comuns morrem em 3–6 golpes: um golpe a menos já é −15%).
- pilha completa (§14) por cima do herói refinado: mesmo orçamento do §14.3 contra o r8 refinado (chefe ≥ 78%), com a
  borda "só básico" afrouxada para 75% (o herói refinado também aguenta mais).
- pilha + refino contra o r8 sem refino: chefe ≥ 74%, comuns ≥ 55%, e nenhum chefe abaixo de 12 s; parado, o chefe
  ainda mata em 12–40 s.

Resultado (r9 + refino, 3000 jogadas):

| Ciclo | Bioma | Refino (opção) | Chefe: r8 → r8+refino → pilha+refino | 2 comuns: r8 → r8+refino → pilha+refino | Parado, chefe mata em (pilha+refino) |
|---|---|---|---|---|---|
| NG+0 | Floresta | +4 (0) | 16,5 → 16,3 → **13,6** s | 4,8 → 4,5 → **3,8** s | 22,3 s |
| NG+0 | Deserto | +6 (1) | 18,1 → 17,6 → **14,4** s | 6,1 → 5,6 → **4,6** s | 24,2 s |
| NG+0 | Montanha | +7 (1) | 18,4 → 17,3 → **14,0** s | 6,4 → 6,3 → **4,7** s | 26,8 s |
| NG+0 | Cavernas | +9 (2) | 20,0 → 19,1 → **15,3** s | 4,6 → 4,0 → **2,9** s | 32,4 s |
| NG+1 | Floresta | +9 (2) | 18,5 → 17,2 → **13,7** s | 3,3 → 3,2 → **2,3** s | 29,2 s |
| NG+1 | Deserto | +9 (2) | 18,5 → 17,3 → **13,8** s | 3,6 → 3,3 → **2,5** s | 29,4 s |
| NG+1 | Montanha | +9 (2) | 18,5 → 17,4 → **13,8** s | 4,0 → 4,0 → **3,1** s | 30,1 s |
| NG+1 | Cavernas | +9 (2) | 19,8 → 18,8 → **15,2** s | 4,0 → 4,0 → **2,9** s | 29,4 s |

Orçamento do refino e orçamento da pilha (§14.3): **dentro**; pytest de `tools/balance` verde.

Ajuste feito no caminho: com 25% do bônus da arma indo para o AttackDamage (primeira versão), a pilha + refino passava
do orçamento em todo NG+1 (chefe a 12,6 s, 69% do r8); caiu para **10%**, opção da arma 1,5% → 1% e opção da
armadura 1% → 0,5%. A tabela de chance também ficou mais branda acima de +9 (antes 50% → 25% com 20–100 de ouro: +15
custava ~400 pedras e 9.000 de ouro).

### 20.5 Testes

`RefineTests` (EditMode): tabela de chance e pedras, amuleto (só onde pode falhar, teto 95%), falha −1 acima de +6 e
item nunca destruído, checagens (pedra, ouro acima de +9, amuleto), curva e brilho, nome "+N", sementes reproduzíveis e
uniformes, custo esperado, drops por região/chefe com semente, itens nas lojas, serviço gastando pedra/ouro/amuleto,
dados que viajam por equipar/copiar/salvar (save antigo = +0) e textos da janela. `LocalizationCoverageTests` cobre
`RefineTexts`; `ThemeAdoptionTests` cobre `Refine.uss`. Python: `test_refine_stack.py` (curva lida do C#, refino nunca
alonga luta, orçamento NG+0/NG+1).

---

## 21. Caça: tarefas do Seu Zé, presa do dia, bicho destaque e amuletos do Caderno (2026-10-30)

Inspirado no Tibia (Referencias_MMO.md §3), com nomes e tema nossos. Regras puras em `HuntingRules` e
`HuntingCharmCatalog` (Gameplay/Hunting), estado em `HuntingProgress` (save `Hunting`), testes em
`HuntingTests`. **Nada disso vale contra chefe**: tarefa, presa, destaque e amuleto só contam para bichos comuns.

### 21.1 Tarefas de caça (Seu Zé Caçador, Vila)

Uma tarefa por vez, "derrote N de um bicho" entre os bichos comuns com página no Caderno (no começo, os da
Floresta). Desistir não dá nada e libera outra na hora. Recompensa entregue no abate que conclui a tarefa
(XP para o herói, ouro na mochila pelo encaixe da grade; o que não couber fica guardado para o próximo abate).

| Tamanho | Abates | XP bônus | Ouro | Pontos de Caça | Ex.: lobo (10 XP) / morcegos (28 XP) |
|---|---|---|---|---|---|
| Pequena | 10 | abates × XP do bicho × 0,5 | 15 | 1 | 50 / 140 XP |
| Média | 25 | × 0,6 | 45 | 3 | 150 / 420 XP |
| Grande | 60 | × 0,75 | 120 | 8 | 450 / 1260 XP |

- O bônus de XP é **metade a três quartos** da XP que os próprios abates já deram: acelera, não substitui.
- Limite: **5 tarefas concluídas por dia**.
- Pontos de Caça: título com o Seu Zé (Aprendiz 0, Mateiro 5, Rastreador 20, Caçador do Sertão 50 pontos ganhos) e
  troca **5 Pontos de Caça → 1 Ponto do Caderno**.

### 21.2 Presa do dia e bicho destaque do dia

- **Presa do dia**: 3 bichos comuns elegíveis por dia, cada um com um bônus sorteado; escolhe-se um, que vale até
  o dia virar. **Uma troca por dia, por 15 de ouro** (sorteia outras 3 e limpa a escolha).

| Bônus da presa | Valor |
|---|---|
| XP | +25% de XP de abate |
| Saque | +25% de chance de uma rolagem inteira extra de saque |
| Couro grosso | −10% de dano recebido daquele bicho |

- **Bicho destaque do dia**: um bicho comum (posicionado no mapa) por dia, **+50% de XP e +50% de chance de
  rolagem extra de saque**; anunciado no HUD do mapa (bloco da região) e no topo do Caderno.
- Presa de XP + destaque no mesmo bicho somam: ×1,75 de XP; saque: 75% de chance de rolagem extra.
- Sorteio: FNV-1a("aaaa-mm-dd|propósito") + LCG, sobre os ids em ordem ordinal (o mesmo em qualquer aparelho e
  ordem de carga). O dia salvo é uma marca d'água: relógio voltado para trás não vira o dia.

### 21.3 Pontos do Caderno e amuletos

Cada página dá Pontos do Caderno ao chegar em **10 / 50 / 200 vitórias (1 + 2 + 3 = 6 por página)**. Os pontos
não são salvos: são as vitórias do Caderno + as trocas com o Seu Zé − o custo dos amuletos comprados (save antigo
já chega com seus pontos). Compra-se uma vez; **o cordão da Vó aguenta 2 amuletos** de cada vez.

| Amuleto | Efeito (só bichos comuns) | Custo |
|---|---|---|
| Dente de Onça | −8% de dano recebido de bichos de quatro patas | 4 |
| Casca de Besouro | −8% de dano recebido de bichos de muitas patas | 4 |
| Pena de Carcará | −8% de dano recebido de bichos que voam | 4 |
| Pedra de Corisco | −8% de dano recebido de bichos pesados | 4 |
| Figa de Guiné | 3% de chance de desviar do golpe | 6 |
| Raminho de Arruda | cura 2% da vida máxima a cada bicho vencido | 5 |
| Nó de Vaqueiro | atordoamentos duram +10% | 5 |

Proteção somada contra um bicho (amuleto do tipo + presa "Couro grosso") limitada a 30% (hoje no máximo 18%).
Total dos 7 amuletos: 32 pontos (~5–6 páginas completas).

### 21.4 Por que os amuletos protegem em vez de bater mais forte (pilha de poder, §14)

`power_stack.py` agora lê `HuntingCharmCatalog`/`HuntingRules` e soma o melhor cordão + a presa como
AttackDamage % **contra os comuns** (nunca chefes), conservador: em todos os comuns da luta.

| Dano extra contra comuns (cordão + presa) | Resultado do orçamento (NG+0 / NG+1) |
|---|---|
| 0 (desenho final) | dentro do orçamento |
| +2% (só amuleto ou só presa) | NG+0 Cavernas: comuns 3,1 s < 70% de 4,6 s |
| +4% + 3% (proposta inicial) | NG+0 Montanha 4,1 s < 4,5 s; Cavernas 3,1 s < 3,2 s |

Os comuns das Cavernas estão num **limiar de golpes**: com a pilha atual, qualquer +2% de dano tira um golpe da
luta e passa do teto de 30%. Por isso os efeitos de combate da caça viraram **proteção** (dano recebido, desvio,
cura no abate, atordoamento), que não muda nenhum tempo para matar do orçamento; chefes ficam de fora, então o
"herói parado" também não muda. O pytest (`test_hunting_is_parsed_and_adds_no_damage` e
`test_hunting_damage_would_break_the_commons_budget`) falha se um amuleto de dano ou uma presa de dano voltar.
Resultado com o código atual: **pilha completa dentro do orçamento** (tabela de §14.4 inalterada).

XP e saque (presa, destaque, tarefas) mudam o ritmo de nível e de loot, não o TTK; com as metas de missão
por região o herói chega ao chefe no nível de §10. Teto prático diário: 5 tarefas + 1 presa + 1 destaque.

### 21.5 Testes

`HuntingTests` (EditMode): tamanhos e recompensas, progresso e conclusão única, uma tarefa por vez e limite diário,
elegíveis, sorteio determinístico e independente da ordem, destaque nunca chefe, ofertas distintas e troca,
virada de dia (relógio para trás ignorado; a tarefa sobrevive), troca paga, bônus empilhando só no bicho certo e
nunca em chefe, rolagens extras de saque, marcos 10/50/200, custos/efeitos/cordão de 2, troca 5→1, ganchos
neutros sem o serviço, round-trip do save em JSON, save vazio/antigo e saneamento.
`tools/balance/test_power_stack.py`: os dois testes de §21.4.

### 21.6 Online (FASE 12)

- **Sorteios pelo relógio do servidor**: o dia (`aaaa-mm-dd`, fuso do servidor) e as sementes da presa e do
  destaque vêm do servidor; o cliente só exibe. O destaque é o mesmo para todos no dia.
- **Antifarm** validado no servidor: progresso de tarefa só por abate dado pelo personagem (ou grupo) no
  encontro, com o bicho do nível do mapa; 5 tarefas/dia por conta; 1 troca de presa/dia; bônus de XP/saque não
  valem em bichos de "Ajudante de Caça" (caça automática) nem em mapas 10+ níveis abaixo; pontos do Caderno e
  amuletos por personagem, recalculados das vitórias no servidor.

---

## 22. Salas de evento com horário: Terreiro das Assombrações e Torre do Engenho Velho (2026-10-30)

Inspiração: Devil Square e Blood Castle do MU (Docs/Referencias_MMO.md §2, FASE 16). Primeiro offline. Regras puras
em `EventRoomRules` (`Assets/Scripts/Gameplay/EventRooms/`), estado salvo em `EventRoomProgress`, sala rodada por
`EventRoomDirector` (Main.unity, sem editar cena: a arena entra em "modo dirigido" pela flag `EncounterPayload.eventRoom`).
Testes: `EventRoomTests`. **Estimativa, ainda não passou pelo modelo do r8/r9.**

### 22.1 Horário (`EventRoomSchedule`, relógio do jogo)

| Sala | Abre | Janela de entrada | Item de entrada | Nível mínimo |
|---|---|---|---|---|
| Terreiro das Assombrações | 0h, 4h, 8h, 12h, 16h, 20h | 1 h de jogo (~50 s reais no dia de 20 min) | Convite do Terreiro | 3 |
| Torre do Engenho Velho | 2h, 6h, 10h, 14h, 18h, 22h | 1 h de jogo | Chave do Engenho | 6 |

- As duas nunca abrem juntas. **Uma entrada por abertura** (o índice da janela fica no save); o item só é gasto no
  clique "Entrar" do mapa e volta se a viagem falhar.
- "10 minutos de janela" do pedido virou 1 h de jogo: 10 min de jogo seriam ~8 s reais, curto demais para criança.
  O horário fica no save (`EventRoomSaveState.schedules`); online o servidor manda o dele.
- Entrada: queda rara fora das salas (comum 1,5% Convite / 0,75% Chave; chefe 25% / 15%; Chave só a partir do nível
  6, nada antes do nível 3, nada dentro das salas) ou o Mascate Seu Bento (36 / 54 de ouro, 120% do valor, no rodízio
  depois do chefe da Floresta / do Deserto). Os dois itens são `QuestItem`: não ocupam a grade nem caem ao morrer.

### 22.2 Faixas de nível e escala dos bichos

Os bichos vêm de **todas** as regiões (só comuns) e são ajustados à faixa do herói (`EventRoomEnemyScaler`).

| Faixa | Níveis | Referência | Vida e XP do comum | Extra |
|---|---|---|---|---|
| 0 | 1–4 | Floresta | 70 / 10 | — |
| 1 | 5–8 | Deserto | 110 / 18 | — |
| 2 | 9–12 | Montanha | 140 / 28 | — |
| 3 | 13–15 | Cavernas | 170 / 40 | — |
| 4 | 16+ (NG+) | Cavernas | 221 / 52 | ×1,3 |

Vida e XP ×(referência ÷ comum da região de origem), vida presa em ×0,35–3,5; dano `√(vida) − 1` (−40% a +80%,
mais suave porque vêm vários juntos). XP por abate vira a da faixa (gancho `IKillExperienceScaler` no
`EnemyRewardGranter`), então um herói nível 3 não ganha XP de Cavernas matando morcego enfraquecido. Ouro e
materiais continuam os do bicho. Guardião da Torre: Golem de Pedra (comum, sem registro de chefe) na escala da
faixa ×4 vida/XP, +25% de dano, ×1,5 de tamanho. Portão: 6 comuns da faixa de vida (420 → 1.326).

### 22.3 Ritmo

| | Terreiro | Torre |
|---|---|---|
| Tempo | 3 min | 4 min |
| Ondas | 3, 4, 5, 6, 6… a cada 20 s ou com ≤ 1 vivo; teto 8 vivos | Ponte: 2, 3, 4, 4… a cada 15 s (teto 6) até **10 abates**; portão/guardião: 2 a cada 18 s (teto 3); pegar a Moenda acorda +2 |
| Fim | tempo acaba (vencido) ou morte | entregar a Moenda ao Seu Malaquias (vencido), tempo acaba ou morte |

### 22.4 Pontuação e medalhas

- Abate 100 pontos, Guardião 1.000, × combo: +0,1 por abate encadeado em até 4 s, teto ×2,0 (11 seguidos).
- Bônus: Terreiro chegar vivo ao fim +1.000; Torre ponte +500, portão +1.000, entrega +2.000 + 10 por segundo restante.
- Medalhas (as duas salas): **Bronze 1.000, Prata 2.500, Ouro 4.500, Mestre 7.000.** Torre sem entrega: no máximo
  Bronze. Terreiro com morte: sem teto (a morte só encerra antes).
- Esperado (estimativa, ~5 s por comum como no r8, combo médio ×1,3): Terreiro ~30 abates ≈ 3.900 + 1.000 = **Prata/
  Ouro**; Mestre pede ~45 abates com combos longos. Torre vencida com ~90 s sobrando ≈ 1.300 + 500 + 1.000 + 1.300 +
  2.000 + 900 ≈ **Ouro**; Mestre pede pressa e combo.

### 22.5 Recompensas por medalha (`EventRoomRules.RewardFor`)

XP = abates-equivalentes × XP do comum da faixa (×1,3 na faixa 4); ouro × (1 + 0,5 × faixa); pedras do refino (§20)
pela grade normal da mochila (`LootGrantRules`; o que não couber é avisado).

| Medalha | XP (abates) | Faixa 0 | Faixa 2 | Faixa 4 | Ouro (faixa 0 / 2 / 4) | Pedras |
|---|---|---|---|---|---|---|
| Bronze | 5 | 50 | 140 | 260 | 8 / 16 / 24 | — |
| Prata | 10 | 100 | 280 | 520 | 15 / 30 / 45 | 1 Bênção |
| Ouro | 16 | 160 | 448 | 832 | 25 / 50 / 75 | 2 Bênção |
| Mestre | 25 | 250 | 700 | 1.300 | 40 / 80 / 120 | 2 Bênção + 1 Alma |

Somando o XP dos abates (~30 comuns da faixa), uma sala Ouro rende ~46 abates-equivalentes em 3–4 min: ~1,5× o XP
por minuto de uma luta normal de dia (§16.3), compensado por 6 entradas/dia no máximo por sala, item gasto e a
janela de 1 h de jogo. Não mexe na pilha de poder (§14): nada de buff, só XP/ouro/pedras. Se a curva andar rápido
demais no r10, cortar `TierExperienceKills` (5/10/16/25) antes de mexer nas ondas.

### 22.6 Conquistas e save

- `ach_room_terreiro_clear` "Ninguém Assusta a Gente" (vencer o Terreiro), `ach_room_torre_clear` "O Engenho Canta
  de Novo" (entregar a Moenda), `ach_room_master` "Mestre das Salas" (7.000 pontos em qualquer sala). Tipos novos
  `EventRoomCleared = 50` (ids distintos) e `EventRoomBestScore = 51` (máximo).
- Save `EventRooms`: melhor pontuação e medalha, partidas, vitórias, última janela usada e horário por sala. Save
  antigo carrega vazio com o horário padrão.

### 22.7 Online (FASE 12)

Sala vira instância compartilhada de até 5 jogadores (grupo); pontuação, medalha e recompensa **por jogador**
(abate conta para quem matou, bônus de fase para todos presentes); o servidor é dono do horário, das janelas e da
entrada (gasta o item do lado do servidor); ranking por abertura no servidor.

---

## 23. Fôlego diário e recompensa diária em sequência (2026-10-30)

Duas decisões do usuário (TODO Fase 16, `Docs/Referencias_MMO.md` §6): o **fôlego diário** (bônus nas
primeiras horas e lembrete gentil contra o excesso de jogo) e a **recompensa diária em sequência** (presente a
cada dia novo, numa trilha de 7 dias). Código em `Assets/Scripts/Gameplay/Daily/` e `Assets/Scripts/UI/Daily/`.

### 23.1 Fôlego: faixas, descanso e relógio (`DailyStaminaRules`, testado em `DailyStaminaTests`)

| Faixa | Tempo de jogo ativo | Loot (moedas e materiais) | XP de abate (ritmo de campanha, §23.3) |
|---|---|---|---|
| **Fôlego cheio** | primeiras 3 h | ×1,3 | ×1,00 (= XP do r8) |
| Fôlego normal | de 3 h a 5 h | ×1 | ×0,77 (cheio é +30% sobre ele) |
| Cansado | depois de 5 h | ×0,5 | ×0,38 (−50% sob o normal) |

- **Dois baldes** gastos em ordem pelo jogo ativo: 3 h de fôlego cheio, depois 2 h de normal. Depois disso o
  herói fica "cansado", sem piso de tempo; **o jogo nunca é bloqueado**.
- **Descanso**: cada segundo longe do jogo devolve um segundo, **primeiro no fôlego cheio** (1 hora fora = 1 hora
  de fôlego cheio), depois no normal; 5 h fora enchem tudo. Não há virada à meia-noite: uma noite de sono enche
  tudo, e jogar atravessando a meia-noite não ganha 3 h novas.
- **Só jogo ativo conta** (`ClassifyTick`): cena de jogo (mapa ou arena), sem pausa, app em foco e alguma entrada
  (tecla, mouse, toque, controle) nos últimos **3 min**. Título, pausa, app em segundo plano e ocioso contam como
  descanso. Os 3 primeiros minutos de um período ocioso ainda contam como jogo (só se sabe que ficou ocioso depois).
- **Relógio** (offline): o tempo da sessão vem do relógio monotônico (`Time.unscaledDeltaTime`, quadros acima de
  5 s vão pelo caminho offline); o descanso offline é o relógio UTC menos uma **marca d'água** que nunca volta
  (relógio atrasado = 0 de descanso), limitado a 5 h. Um salto de data com o jogo aberto (relógio de parede ×
  monotônico com diferença > 2 min) é guardado como "desvio" e descontado no próximo cálculo offline, então
  adiantar a data não vira descanso. **Online (Fase 12) o servidor é dono do fôlego**; o cliente só mostra.
- Guardado **por aparelho** (PlayerPrefs `Daily.Folego.State`), não no slot: é sobre a criança, e um jogo novo não
  reabastece o fôlego.
- **Lembrete** "Hora de descansar, a Vó Zefa chamou para o café!" ao entrar no cansaço e a cada 30 min de jogo
  ativo enquanto cansado (toast no HUD do mapa e da arena).
- **Onde se aplica (um lugar cada)**: XP de abate em `EnemyRewardGranter` (depois de chefe/presa do dia/sala de
  evento, antes da NG+); loot em `EnemyLootGranter` logo depois das rolagens do inimigo (base + bioma + rolagens
  extras), via `StaminaLootRules`: só pilhas simples (ouro, materiais, itens de missão) escalam, com arredondamento
  sorteado (2 de ouro ×1,3 = 2 ou 3, média 2,6); **equipamento gerado, bônus de chefe e Pedras do Sertão não mudam**.
  XP de missão, de evento da estrada e de variante noturna também não.

### 23.2 Pilha de poder com fôlego cheio (`power_stack.py`, seção "Fôlego cheio")

`load_stamina()` lê os multiplicadores e o ritmo do C#; `evaluate(..., stamina_mult=)` sobe o herói da rota com
missões com aquela XP de abate (níveis maiores) e compara com o herói do r8 nos níveis de sempre.

| Ciclo | Bioma (níveis comuns / chefe) | Relíquias | 2 comuns: r8 → pilha | Chefe: r8 → relíquias → pilha |
|---|---|---|---|---|
| NG+0 | Floresta (2 / 3) | 3 | 4,8 → **3,8** s | 16,5 → 15,3 → **13,8** s |
| NG+0 | Deserto (5 / 7) | 6 | 6,1 → **4,8** s | 18,1 → 17,3 → **14,9** s |
| NG+0 | Montanha (9 / 10) | 9 | 6,4 → **4,9** s | 18,4 → 16,5 → **14,4** s |
| NG+0 | Cavernas (13 / 15) | 12 | 4,6 → **3,5** s | 20,0 → 18,4 → **16,1** s |
| NG+1 | Floresta (16 / 17) | 12 | 3,3 → **2,5** s | 18,5 → 16,4 → **14,6** s |
| NG+1 | Deserto (17 / 18) | 12 | 3,6 → **2,9** s | 18,5 → 16,4 → **14,5** s |
| NG+1 | Montanha (19 / 20) | 12 | 4,0 → **2,9** s | 18,5 → 16,4 → **14,5** s |
| NG+1 | Cavernas (21 / 22) | 12 | 4,0 → **2,9** s | 19,8 → 17,9 → **15,9** s |

**Dentro do orçamento** (§14.3) na NG+0 e na NG+1: com o ritmo de campanha o fôlego cheio é exatamente o r8.

### 23.3 Por que o "ritmo de campanha" (`DailyStaminaRules.KillXpPace` = 1 / 1,3)

O fôlego não mexe no tempo para matar diretamente, mas XP vira nível. A rota com missões tem número fixo de lutas,
então +30% de XP de abate por cima do r8 leva o herói **1 nível acima** em cada chefe na NG+0 (Floresta 3 → 4,
Cavernas 15 → 16) e **1–2 na NG+1** (Cavernas 22 → 24). Isso, somado à pilha completa, quebra o orçamento
(saída informativa do `power_stack.py`, não aplicada no jogo):

| Ciclo | Bioma (níveis) | 2 comuns: r8 → pilha | Chefe: r8 → pilha |
|---|---|---|---|
| NG+0 | Montanha (10 / 11) | 6,4 → **2,9** s (−55%) | 18,4 → **14,3** s (−22,3%) |
| NG+0 | Cavernas (14 / 16) | 4,6 → **2,9** s | 20,0 → **15,6** s |
| NG+1 | Floresta (17 / 18) | 3,3 → 2,5 s | 18,5 → **14,0** s |
| NG+1 | Cavernas (23 / 24) | 4,0 → 3,1 s | 19,8 → **15,1** s |

Além disso, na NG+0 quem só usa o básico venceria folgado em Floresta, Montanha e Cavernas. Como uma criança joga
quase sempre menos de 3 h por dia (fôlego cheio), o r8 foi tratado como o ritmo do **fôlego cheio**: a XP de
abate é `base × faixa × KillXpPace`. As proporções decididas ficam (+30% sobre o normal, −50% sob ele) e o
orçamento não muda. Quem joga de 3 a 5 h sobe ~23% mais devagar do que hoje; cansado, ~62%.
**Decisão para o usuário**: se a ideia for +30% por cima da XP de hoje, troque `KillXpPace` para `1f` e corte
~1 nível de poder em outro lugar (o pytest `test_stamina_on_top_of_r8_without_the_pace_would_break_the_budget`
mostra o estouro). O loot não tem ritmo (moedas e materiais estão fora do modelo de combate).

### 23.4 Recompensa diária em sequência (`DailyLoginRules`, testado em `DailyLoginTests`)

| Dia | Presente |
|---|---|
| 1 | 20 de ouro |
| 2 | 2 poções de vida |
| 3 | 3 de ferro (material) |
| 4 | 40 de ouro + 2 poções de mana |
| 5 | 1 Convite do Terreiro (`item_convite_terreiro`, salas de evento; 30 de ouro se o item sumir) |
| 6 | 3 poções de vida + 5 ervas |
| 7 | peça rara surpresa (couro: elmo → botas → peitoral a cada ciclo, afixos Raro, semente pela data) + 50 de ouro |

- O primeiro login de cada **novo dia do calendário local** recebe o presente; depois do dia 7 a trilha volta ao 1.
- **Faltou um dia: volta um passo** a partir de onde continuaria (pegou o dia 4, faltou → dia 4 de novo; pegou o 7
  → dia 1). Uma ausência longa também é um passo só. Nunca zera.
- Relógio: a data do último presente é marca d'água (relógio atrasado não ganha nada); adiantar a data só conta
  como "faltou" (um passo para trás), nunca como progresso. Online, o servidor é dono.
- Por **slot** (`SaveManager.Register`, chave "DailyLogin"); saves antigos e jogo novo começam no dia 1. O que não
  cabe na Mochila (API de encaixe da grade) vai para o Baú da Vó Zefa.
- Valor da semana: ~110 de ouro + 5 poções de vida + 2 de mana + materiais + 1 convite + 1 peça Rara. A peça Rara de
  couro fica dentro do p90 de sorte do equipamento (§10.4); não entra na pilha.
- Popup no mapa (abre sozinho quando há presente; Esc/Start fecha) e faixa no título (prévia pela cópia em
  PlayerPrefs `Daily.Login.Preview`, sem save carregado).

### 23.5 Acessibilidade / pais

Configurações → Acessibilidade → **"Lembrete de tempo de jogo (para os pais)"**: desligado (padrão) ou a cada 30,
45, 60, 90 ou 120 min de jogo ativo na sessão, um toast "Você já jogou N minutos. Que tal esticar as pernas e beber
uma água?". Guardado por aparelho (PlayerPrefs `Parental.PlayTimeReminderMinutes`). Nunca pausa nem bloqueia.

### 23.6 Testes

C#: `DailyStaminaTests` (faixas, multiplicadores, ritmo de XP, loot com arredondamento sorteado, baldes de
descanso, ocioso/pausa/título, marca d'água, salto de data na sessão, persistência, lembretes) e
`DailyLoginTests` (trilha, passo para trás, relógio, tabela, itens no banco, convite/fallback, peça rara,
Mochila cheia → Baú, save antigo → dia 1, prévia do título). Python: `test_power_stack.py` (fôlego lido do C#,
pilha com fôlego cheio dentro do orçamento, e o estouro sem o ritmo).

---

## 24. Tacho Encantado da Vó Zefa: asas, Proteção do Caos e Frutas do Sertão (2026-10-30)

A "máquina de combinar" do MU (Chaos Machine), com nomes e tema nossos (FASE 16, `Docs/Referencias_MMO.md` §2).
Regras puras em `TachoRules` / `WingCatalog` / `FruitRules` (`Assets/Scripts/Gameplay/Tacho`), testadas em `TachoTests`;
aplicação na Mochila em `TachoService`; janela "Tacho Encantado da Vó" no hub da **Vila** (`VillageTachoPanel`,
`Resources/UI/Tacho.uss`). **Quem cuida:** a Vó Zefa, não o Mestre Tonho — a Pedra do Caos já dizia "a Vó guarda essas
para o Tacho Encantado", tacho é coisa de cozinha (é ela quem tem o fogão a lenha e o Baú), e o ferreiro fica com o que é
de bigorna (refino). O Tacho só existe onde a Vó mora (`CitySpec.HasGrandmaServices`), o que também garante o Baú para o
que não couber na Mochila.

**Regra infantil (todas as receitas):** se falhar, os materiais, pedras e ouro se gastam, mas o **item principal fica
com você do jeitinho que estava** (a peça +4 não vira pó, a asa não perde nível, a peça protegida não perde nível).
A janela mostra chance, custo e essa frase antes de cada mistura. **Online, o servidor sorteia** (`TachoRules.Attempt`
com a semente dele, mesma rolagem SplitMix64 do refino) e o cliente só mostra a resposta.

### 24.1 Receitas e chances (`TachoRules`)

| Receita | Item principal (fica se falhar) | Materiais | Ouro | Chance |
|---|---|---|---|---|
| Asas nível 1 (5 estilos) | arma/armadura refinada **+4 ou mais** (vira as asas no sucesso) | 1 Pedra do Caos + materiais do estilo | 60 | 50% em +4, +5 pontos por nível, teto 75% (+9) |
| Asas 1 → 2 | as asas nível 1 | 2 Caos + 1 Alma + 4 minério de ferro + 4 pedras | 100 | 50% |
| Asas 2 → 3 | as asas nível 2 | 3 Caos + 2 Alma + 1 Vida + 6 ferro + 6 pedras | 150 | 35% |
| Asas nível 4 (lendária) | — | só documentada (sem item e sem receita): stats ×2 do nível 1, absorve 3% | — | — |
| Proteção do Caos | peça em +7..+14 | a pedra do refino normal (Alma) + 1 Pedra do Caos | o do refino (0–50) | a do refino (80% → 35%) |
| Fruta do Mandacaru | — | 1 Caos + 1 Vida + 5 ervas | 40 | 60% |
| Fruta do Umbu | — | 2 Bênção + 3 ervas | 20 | 80% |

Materiais do estilo (nível 1), um estilo por primo (qualquer primo pode usar qualquer asa; a janela destaca o do primo em
jogo):

| Estilo | Primo | Materiais | Nível 1: dano / defesa / velocidade |
|---|---|---|---|
| Asas de Carcará | Maithe | 6 fibras + 3 pedras | +1,5% / +1% / +3% |
| Capa de Vaqueiro Encantada | Joaquim | 8 fibras + 2 ferros | +1% / +1,5% / +2% |
| Asas de Beija-flor | Maria | 6 ervas + 4 fibras | +1% / +1% / +5% |
| Asas de Asa-branca | Ícaro | 6 fibras + 3 ferros | +1% / +1% / +3% |
| Asas de Vaga-lume | Lavine | 5 ervas + 3 pedras | +1,5% / +0,5% / +4% |

O pedido listava quatro asas; a quinta (Vaga-lume, da Lavine) fecha "uma por primo". Proteção do Caos não aparece em
+0..+6: lá o refino normal já não perde nível ("use o Ferreiro"). Custo esperado: asas 1 a partir de +4 ≈ 2 tentativas
(2 Caos, 120 de ouro); 1 → 2 ≈ 4 Caos + 2 Almas; 2 → 3 ≈ 8,6 Caos + 5,7 Almas + 2,9 Vidas. Com a Pedra do Caos caindo só do
chefe da Montanha (15%) e nas Cavernas (1% / chefe 25%), asas 3 são meta de longo prazo (NG+ / online).

### 24.2 Asas no equipamento (slot novo "Costas/Asas")

- `EquipmentSlot.Wings = 9`, **acrescentado no fim** do enum: assets e saves guardam o slot como int, então saves
  antigos (Mochila em grade e `EquippedEntry`) carregam iguais; `tools/balance/rapadura_data.py` ganhou o nome na lista.
- Itens `item_asas_carcara_1..3`, `item_capa_vaqueiro_1..3`, `item_asas_beija_flor_1..3`, `item_asas_asa_branca_1..3`,
  `item_asas_vaga_lume_1..3`: Armor/Accessory, 2×2 na grade, **presos ao herói** (nunca caem ao morrer), sem
  durabilidade e sem refino (não têm atributo base).
- Bônus (`WingCatalog.ModifiersFor`, aplicados pelo `InventoryManager` em `equip_Wings/wing/*`): AttackDamage %,
  Defense % (menos dano recebido, `DamageCalculator`) e MoveSpeed %, = nível 1 × **1 / 1,25 / 1,5** (nível 4: ×2).
  **Absorção** a partir do nível 2: **1% / 2%** (lendária 3%). No Rapadura, Defesa percentual já é "menos dano
  recebido", então a absorção usa esse mesmo canal (soma com os outros percentuais, teto de 80% do `DamageCalculator`),
  com linha própria na dica ("Absorve 2% do dano").
- Visual: malha procedural low-poly (`WingMeshBuilder`, paleta dos bichos, ≤ 96 triângulos por lado) presa no osso do
  peito do modelo do herói (`WingBackVisual`), batendo devagar pelas molas do motion feel (`MotionFeelMath.StepSpring`) e
  dobrando para trás na corrida. Com **"Reduzir Movimento"** a batida parada desliga e a resposta cai pela metade.

### 24.3 Frutas do Sertão (`FruitRules`, `AttributeSet`)

- **Fruta do Mandacaru:** +1 ponto de atributo para distribuir, **até 2 por jornada** (`FruitRules.MaxGrowthPoints`,
  contado no save do `AttributeSet`, `growthFruitPoints`; save antigo = 0).
- **Fruta do Umbu:** devolve 1 ponto do atributo com mais pontos distribuídos (empate: ordem da ficha) para distribuir
  de novo.
- Come-se na Mochila durante uma aventura (onde está a ficha do herói). Fruta que não faria nada (teto atingido, nada
  para devolver, ou no mapa sem ficha) **não é comida**: continua na Mochila.

### 24.4 Orçamento (`tools/balance/power_stack.py`, `test_wings_stack.py`)

O modelo põe, em todas as lutas ao chegar em cada região, o refino de §20.4 **mais** as asas e frutas de quem farmou as
Pedras do Caos (teto): Floresta e Deserto nada (não cai Caos), Montanha asas 1, Cavernas asas 1 + 2 frutas (≈3 Caos até
o fim das Cavernas: o gasto mais ofensivo), NG+1 asas 3 + 2 frutas (teto). Conservador: soma o **maior dano** de
qualquer estilo com a **maior defesa** de qualquer estilo (+ absorção) na mesma asa, e as frutas como Poder Arcano (+1%
de dano cada). Velocidade fica fora (o modelo não anda).

Regras: herói r8 refinado + asas + frutas: chefe ≥ **95%** do r8 refinado, comuns ≥ **75%**; tudo junto (pilha §14 +
refino + asas + frutas) contra o r8 puro: chefe ≥ **70%**, comuns ≥ 50%, nenhum chefe abaixo de 12 s; NG+0 "só
básico" ≥ 75% do tempo que o chefe leva; parado, o chefe ainda mata em 12–40 s.

Resultado (3000 jogadas):

| Ciclo | Bioma | Asas (frutas) | Chefe: r8 → r8+refino → r8+refino+asas → tudo | 2 comuns: r8 → r8+refino+asas → tudo | Parado, chefe mata em (tudo) |
|---|---|---|---|---|---|
| NG+0 | Floresta | — (0) | 16,5 → 16,3 → 16,3 → **13,6** s | 4,8 → 4,5 → **3,8** s | 22,3 s |
| NG+0 | Deserto | — (0) | 18,2 → 17,6 → 17,6 → **14,4** s | 6,1 → 5,6 → **4,6** s | 24,2 s |
| NG+0 | Montanha | 1 (0) | 18,4 → 17,5 → 17,0 → **13,6** s | 6,4 → 5,8 → **4,4** s | 27,2 s |
| NG+0 | Cavernas | 1 (2) | 20,0 → 19,1 → 18,6 → **14,8** s | 4,6 → 4,0 → **3,1** s | 32,9 s |
| NG+1 | Floresta | 3 (2) | 18,5 → 17,3 → 16,5 → **13,4** s | 3,3 → 2,6 → **2,3** s | 30,6 s |
| NG+1 | Deserto | 3 (2) | 18,5 → 17,4 → 16,8 → **13,5** s | 3,6 → 3,3 → **2,4** s | 30,8 s |
| NG+1 | Montanha | 3 (2) | 18,5 → 17,4 → 16,7 → **13,6** s | 4,1 → 3,8 → **2,9** s | 31,5 s |
| NG+1 | Cavernas | 3 (2) | 19,8 → 18,8 → 18,1 → **14,4** s | 4,0 → 3,6 → **2,9** s | 30,8 s |

Orçamento das asas: **dentro**; pytest de `tools/balance` verde.

Ajustes feitos no caminho (antes → depois): dano do nível 1 3% → **1,5%** (Carcará), defesa 3% → **1,5%** (Capa),
escala dos níveis ×1/1,5/2 → **×1/1,25/1,5**, absorção 2/4% → **1/2%**, teto do Mandacaru 5 → **2**, e as Cavernas do
NG+0 passaram a supor asas 1 + 2 frutas (o que ~3 Pedras do Caos pagam) em vez de asas 2 + 3 frutas. Com os números
antigos o NG+1 ficava em 91% do r8 refinado (chefe) e o "tudo junto" em 67%; o "só básico" das Cavernas passava do
limite pela defesa. Comuns: no NG+1 da Floresta eles morrem em 2–3 golpes e +4% de dano já tira um golpe (−21%,
degrau, não tendência), por isso o piso dos comuns é 75% (o refino usa 80% pelo mesmo motivo).

### 24.5 Conquistas, testes e online

- Conquistas: **"Bateu Asas"** (`ach_wings_first`, primeiro par de asas, `AchievementConditionType.WingsCrafted = 60`)
  e **"Por Cima do Sertão"** (`ach_wings_level3`, asas nível 3, `WingLevelReached = 61`, máximo), alimentadas pelo
  `TachoService`.
- `TachoTests` (EditMode): receitas e itens existentes, slot novo (int 9), asas presas/2×2/sem refino, stats por nível,
  chances, checagens, sementes reproduzíveis, falha que nunca rebaixa nada, serviço (asas feitas consomem a peça; falha
  mantém a peça e o +N; asa equipada sobe de nível e volta equipada com os bônus; proteção que segura o nível; Mochila
  cheia → Baú), frutas (teto, save antigo, Umbu no maior atributo, mapa sem ficha), malha low-poly por estilo/nível,
  batida com "Reduzir Movimento", textos e dica. `LocalizationCoverageTests` cobre `TachoTexts`; `ThemeAdoptionTests`
  cobre `Tacho.uss`. Python: `test_wings_stack.py`.
- Online (FASE 12): o servidor valida materiais, sorteia, aplica e devolve o `TachoOutcome`; o cliente nunca rola.

## 24. Casa: melhorias N1→N5, conforto e "Descansado" (2026-10-30)

Fases H4 e H6 de `Docs/Housing.md` (§2.2, §6, §7.3, §11.9). Código em `Assets/Scripts/Gameplay/Housing/`
(`HouseUpgradeRules`, `HouseComfortRules`, `HouseRestedRules`, `HousePlantRules`, `HouseVisitRules`,
`HouseQuestChain`, `HouseService.Life.cs`) e `Assets/Scripts/UI/Housing/` (`HouseLifePanel`, `HouseLifeTexts`);
testes em `HouseUpgradeTests` e `HouseLifeRulesTests`.

### 24.1 Subidas da casa (preços provisórios, decisão do usuário pendente)

Uma subida por vez, paga com o que está na **mochila + Baú da casa** (tudo ou nada). Nada se perde: o arranjo é
revalidado na grade maior e o que não couber (por exemplo, um banquinho na frente da porta nova) vai para o Depósito.

| Subida | Nível do primo | Chefe libertado | Ouro | Materiais | Lutas de ouro (~6/luta, §10.1 de Housing) |
|---|---|---|---|---|---|
| N1 → N2 | 8 | Lobo Alfa | 150 | 30 madeira, 15 fibra, 5 ferro | ~25 |
| N2 → N3 | 18 | Guardião da Ruína | 400 | 40 madeira, 30 pedra, 20 fibra | ~65 |
| N3 → N4 | 28 | Senhor da Guerra | 1.000 | 60 pedra, 20 ferro, 30 madeira | ~165 |
| N4 → N5 | 38 | Habitante Ancestral | 2.500 | 100 pedra, 40 ferro, 60 madeira + 1 lembrança de cada chefe (não é gasta) | ~415 |

- A rota com missões termina a NG+0 no nível ~16 (§10): na primeira campanha a casa chega a **N2**; N3–N5 são metas
  da Nova Jornada+ e do pós-jogo, como o Housing.md prevê (dreno de ouro de longo prazo).
- Subir de nível não dá atributo nenhum: muda grade, limite de itens, Baú (20/30/45/60/90) e o casco 3D.

### 24.2 Conforto e faixas

Conforto = soma do catálogo das peças colocadas (cada ID conta no máximo 2 vezes; Depósito não conta; planta murcha
perde metade, arredondado para baixo) + conjuntos (3 peças distintas +5, 6 peças +12, todas +25; vale a maior).
Estimativa só com o catálogo (sem considerar encaixe): N1 com Comuns/Incomuns ≈ 160, melhor caso ≈ 530; N3 ≈ 385/1.130.

| Faixa | Conforto | Nome | "Descansado" (XP de abate) | Lutas | XP extra máxima por noite |
|---|---|---|---|---|---|
| 0 | 0–19 | Casa arrumadinha | +2% | 10 | 0,2 luta |
| 1 | 20–59 | Aconchegante | +2% | 15 | 0,3 luta |
| 2 | 60–149 | Caprichada | +3% | 20 | 0,6 luta |
| 3 | 150–299 | Casa de Vó | +3% | 25 | 0,75 luta |
| 4 | 300+ | Lar dos sonhos | +3% | 30 | 0,9 luta |

- **Mudança em relação ao Housing.md §6.1** (que propunha +2/3/5/7/10%): o ganho para em **+3%** e as faixas altas
  passam a durar mais. Motivo abaixo (§24.3). Luz da Maria completa: +5 lutas.
- Duas noites com bônus por dia de calendário (relógio das diárias); a que sobra não acumula; dormir de novo
  substitui o bônus (não soma). Conta uma luta por encontro terminado (`EncounterEndedEvent`, qualquer resultado).
- **Só XP**: nada de dano, vida ou loot. Entra em `EnemyRewardGranter` como mais um multiplicador da cadeia
  (chefe → presa do dia → sala de evento → fôlego → **descansado** → NG+), cada um arredondado, então eles se compõem
  (ex.: 100 × 1,3 → 130 × 1,03 → 134).

### 24.3 Ritmo de nível e pilha de poder

A rota com missões tem só ~34 lutas na NG+0 (Floresta 12, Deserto 5, Montanha 7, Cavernas 9), então **uma noite
cobre uma região inteira**: quem dorme em casa antes de cada saída fica descansado o tempo todo. O teste foi feito
assim (generoso): NG+0 com faixa 0 na Floresta, 1 no Deserto, 2 na Montanha, 3 nas Cavernas; NG+1 inteira na faixa 4,
rodando `power_stack.evaluate` com a XP de abate por região (script ad hoc, sem mudar a ferramenta):

| Tabela de % por faixa | Resultado |
|---|---|
| 2/3/5/7/10 (proposta do Housing.md) | NG+0 dentro; **NG+1 Deserto: comuns 2,3 s < 70% de 3,6 s** (o herói chega um nível acima) |
| 1/2/3/4/5 e 1/2/3/4/4 | mesmo estouro na NG+1 (qualquer coisa ≥ +4% a campanha toda) |
| **2/2/3/3/3 (adotada)** | **dentro do orçamento**, mesmos níveis da pilha sem descanso em todos os biomas |

Com XP global: ×1,02 fica dentro; ×1,03 já empurra o chefe da Floresta de nível 3 para 4 — mas na Floresta a casa
acabou de chegar (conforto ~6, faixa 0 = +2%, e só depois das 3 primeiras vitórias), então não acontece no jogo.
`power_stack.py` (sem o descanso, que é só XP) segue verde em todas as seções e `test_power_stack.py` passa (21).

### 24.4 Plantas, visitas e Missões da Casa

- Plantas: felizes até 3 dias de jogo sem água, "com sede" no 3º, murchas depois (metade do conforto), **nunca
  morrem nem somem**; o regador da Vó rega tudo de uma vez; chuva ao chegar em casa rega o quintal.
- Livro de visitas: no máximo 1 visita por dia de jogo, só com o livro colocado; quem visita: os 4 primos que não
  estão jogando, Vó Zefa, Dona Cida e, conforme os chefes caem, Seu Bento, Mestre Tonho e Benedita. Primos que ainda
  não assinaram têm prioridade em dias alternados ("Casa Cheia" sai em ~1–2 semanas de dias de jogo). Guarda 100.
- Missões da Casa 2–6 dão só itens de casa (Depósito); as roupas de "Parede de Barro" e "Casa Cheia" ficam
  anotadas para o guarda-roupa (H5). Entregas: Água Fresca 10 pedra + 5 madeira; Fogo na Cozinha 3 carne assada +
  8 madeira (depois do Lobo Alfa). Madeira e carne saem da Floresta; a pedra, do Deserto e da Montanha, então
  "Água Fresca" costuma fechar depois da primeira ida ao Deserto (nada trava: a missão espera).

---

## 25. Durabilidade e conserto; opções de item estilo MU: excelente, sorte, conjunto antigo e engaste (2026-10-30)

Primeira versão, pequena e coerente com os afixos e o refino (§20). Regras puras: `DurabilityRules` / `RepairService`
(`Gameplay/Items/Durability/`) e `ItemOptionCatalog` / `ItemOptionRules` / `EngraveService` (`Gameplay/Items/Options/`),
sem UnityEngine nas regras e com semente passada de fora: online (FASE 12) o servidor roda os mesmos números.

### 25.1 Durabilidade e conserto (`DurabilityRules`, testado em `DurabilityTests`)

Só itens com `HasDurability` (armas e armaduras dos assets) se desgastam; o valor já existia no save
(`InventorySlotData.currentDurability` e `EquippedEntry.durability`), então saves antigos carregam iguais.

| Desgaste | Quanto | Espada de ferro (120) / peitoral de couro (90) |
|---|---|---|
| Arma, por ataque básico (`PlayerAttackPerformedEvent`) | 0,1 | ~1.200 ataques |
| Cada peça de armadura, por golpe recebido (`DamageAppliedEvent`) | 0,15 | ~600 golpes |
| Tudo o que está vestido, por morte (`PlayerDiedEvent`) | 10% do máximo | 10 mortes |

`EquipmentWearTracker` é posto em tempo de execução pelo `InventoryManager` no objeto com a tag "Player" (sem mudar
cena/prefab). Abaixo de 25% o tooltip avisa "Gasto". Em **0** a peça fica **quebrada**: não dá nenhum status (base,
afixos, refino, opções, nem conta para o conjunto antigo), mas **nunca some, nunca cai e não é desequipada à força** — só
espera o ferreiro. O conserto devolve os status na hora.

Conserto (ouro): `cheio = valor × 25% × raridade × (1 + 10% por nível de refino) × (1,5 se tiver opções)`, raridade
Comum 1 / Incomum 1,25 / Raro 1,5 / Épico 2 / Lendário 2,5; o preço é `cheio × fração que falta` (arredonda para cima,
mínimo 1) e uma peça quebrada paga **×1,25** (o castigo de deixar quebrar). Espada de ferro (45, Incomum, +0): conserto
completo 15 de ouro, quebrada 19; a mesma espada +10 com opções, ~45/56 — barato perto do que se ganha numa ida.

Onde consertar: a janela do **Ferreiro** da Vila (`VillageRefinePanel`, a do refino) ganhou "Consertar tudo" (tudo ou
nada) e um "Consertar" por peça. `RepairService.Quote/TryRepair/TryRepairAll/ListWorn` são públicos e puros sobre um
`InventoryManager`, para o ferreiro das cidades (dono das lojas) chamar sem copiar regra.

### 25.2 Opções de item (`ItemOptionCatalog`, testado em `ItemOptionTests`)

Só peças refináveis (arma com dano, armadura com defesa) têm opções; ficam no slot (`excellentMask`, `luck`,
`ancientSetId`, `sockets`), viajam por equipar/copiar/baú/save, e saves antigos carregam como peça comum.

- **Excelente** (linhas verdes no tooltip): arma — +1,5% dano, +1,5% velocidade de ataque ou +1,5% crítico; armadura —
  +2% vida, 1,5% menos dano recebido ou 1% de espinhos. 1 linha de bicho comum, 1–2 de chefe.
- **Com Sorte**: +5% de chance nos refinos que podem falhar (`RefineRules.LuckBonus`, soma com o amuleto, teto 95%) e
  +0,5% de crítico.
- **Conjunto antigo** (só de chefe, nunca excelente junto, como no MU): um por primo — Penas do Carcará Antigo
  (Maithe), Couro do Vaqueiro Antigo (Joaquim), Renda da Benzedeira Antiga (Maria), Engrenagens do Inventor Antigo
  (Ícaro), Luz do Vaga-lume Antigo (Lavine). Bônus com 2, 3 e 4 peças vestidas (somam), cada um ~1,5–2% (ou +2 de
  defesa / +4% de dano crítico).
- **Engaste**: 1 encaixe de bicho comum, 1–2 de chefe; o Ferreiro grava um material que já cai no jogo. Ofensivos só em
  arma (ferro: 2 ferro + 10 ouro, +1% dano; caos: 1 Pedra do Caos + 25 ouro, +1,5% crítico), defensivos só em armadura
  (pedra: 3 pedras + 10 ouro, +1 defesa; erva: 3 ervas + 10 ouro, +1,5% vida) — assim 8 encaixes de armadura nunca
  empilham dano. Regravar um encaixe troca o engaste (o item nunca se perde).

Chance por peça de equipamento que cai (`EnemyLootRoller`; semente própria derivada da semente da peça,
`ItemOptionRules.OptionSeed`, então o roller não sorteia nenhum número a mais e as sementes antigas dão os mesmos drops):

| | Bicho comum | Chefe |
|---|---|---|
| Antigo | — | 4% |
| Excelente (se não for antigo) | 1,5% | 8% |
| Com Sorte | 5% | 15% |
| Engaste | 4% (1 encaixe) | 12% (1–2) |

### 25.3 Orçamento (`tools/balance/power_stack.py`, `test_options_stack.py`)

Teto de quem farmou: peças com opções **perfeitas** (2 linhas excelentes, sorte e 2 engastes com o melhor material, ou
o conjunto antigo mais ofensivo) — 1 peça na Floresta, 2 no Deserto, 3 na Montanha, 4 nas Cavernas e no NG+1 — por
cima de **tudo** (pilha §14 + refino §20 + asas e frutas §24). Regras: opções perfeitas tiram no máximo 10% do chefe e
30% dos comuns (os comuns do NG+1 morrem em 2–3 golpes: um golpe a menos já é −26%); tudo junto mantém o chefe ≥ 66%
do r8 sem nada e ≥ 12 s; parado, o chefe ainda mata em 12–40 s.

Resultado (3000 jogadas):

| Ciclo | Bioma | Peças | Chefe: r8 → tudo → tudo+excelente / tudo+antigo | 2 comuns: tudo → excelente / antigo | Parado, chefe mata em (excelente / antigo) |
|---|---|---|---|---|---|
| NG+0 | Floresta | 1 | 16,5 → 13,6 → **13,2** / **13,5** s | 3,8 → **3,4** / **3,8** s | 22,3 / 22,3 s |
| NG+0 | Deserto | 2 | 18,2 → 14,4 → **13,9** / **14,0** s | 4,6 → **4,3** / **4,3** s | 25,1 / 24,6 s |
| NG+0 | Montanha | 3 | 18,4 → 13,6 → **13,2** / **13,5** s | 4,4 → **4,1** / **4,1** s | 29,4 / 28,3 s |
| NG+0 | Cavernas | 4 | 20,0 → 14,8 → **14,0** / **14,2** s | 3,1 → **2,9** / **2,9** s | 36,9 / 34,8 s |
| NG+1 | Floresta | 4 | 18,5 → 13,4 → **12,4** / **12,3** s | 2,3 → **1,7** / **1,7** s | 34,2 / 32,3 s |
| NG+1 | Deserto | 4 | 18,5 → 13,5 → **12,4** / **12,4** s | 2,4 → **2,3** / **2,3** s | 34,4 / 32,6 s |
| NG+1 | Montanha | 4 | 18,5 → 13,6 → **12,4** / **12,7** s | 2,9 → **2,5** / **2,5** s | 35,2 / 33,3 s |
| NG+1 | Cavernas | 4 | 19,8 → 14,4 → **14,0** / **14,0** s | 2,9 → **2,3** / **2,3** s | 34,4 / 32,5 s |

Dentro do orçamento; pytest de `tools/balance` verde (41). Ajuste feito no caminho: a primeira versão (linhas de 2%,
sorte +2% de crítico, qualquer engaste em qualquer peça) dava +20% de crítico com 4 peças e levava o chefe do NG+1 a
11,1 s (60% do r8). Caiu para linhas de arma 1,5%, espinhos 1%, sorte 0,5% de crítico, conjuntos ~1,5%, e engastes
ofensivos só na arma. O NG+1 fica perto do piso de 12 s: qualquer bônus novo de dano precisa passar por este modelo.

### 25.4 Testes e online

`DurabilityTests` (regras, desgaste por ataque/golpe/morte, peça quebrada sem status e nunca desequipada, conserto
tudo-ou-nada, save) e `ItemOptionTests` (sementes reproduzíveis, frequências do catálogo, antigo nunca excelente,
conjuntos por peças que funcionam, sorte no refino, engaste respeitando arma/armadura, linhas verdes do tooltip, save
antigo = peça comum). `LocalizationCoverageTests` cobre `DurabilityTexts` e `ItemOptionTexts`. Online: desgaste,
sorteio das opções, conserto e engaste rodam no servidor com os mesmos números; o cliente só mostra.

---

## 26. Crafting avançado: níveis de ofício, partes dos bichos, benzedura de arma, forja de fusão e runas (2026-10-30)

Código: `Assets/Scripts/Gameplay/Crafting/Advanced/` (regras puras, RNG passada por quem chama — online roda no
servidor). Modelo: `python -I tools/balance/crafting_stack.py` (+ `test_crafting_stack.py`). Testes:
`AdvancedCraftingTests` (EditMode). Textos en + pt-BR: `CraftingTexts`. Ainda sem janela (a UI fica para depois:
`AdvancedCraftingService` já expõe checar / fazer / fundir / benzer na mochila viva).

### 26.1 Partes dos bichos (`CreatureParts`, `CreaturePartDrops`)

Quinta fonte de loot, rolada **por último** em `EnemyLootGranter` (sementes antigas repetem os mesmos drops). Comum:
**30%** de 1 parte (+5 pp por ciclo de NG+); chefe: **3** partes garantidas + **50%** de 1 Troféu da Fera.

| Parte | Raridade | Quem solta |
|---|---|---|
| Presa de Lobo | Incomum | comuns e chefe da Floresta (menos a aranha) |
| Seda de Aranha | Incomum | Aranha da Floresta |
| Ferrão de Escorpião | Incomum | Deserto |
| Lasca de Golem | Incomum | Montanha |
| Asa de Morcego | Incomum | Cavernas |
| Troféu da Fera | Raro | só chefes (50%) |

### 26.2 Nível de ofício e qualidade do material (`CraftTierRules`, `AdvancedRecipeCatalog`)

- **Nível de ofício** = 1 + região mais funda aberta (Floresta 1 … Cavernas 4); NG+ abre os 4. Receita/benzedura de
  nível acima fica bloqueada.
- **Qualidade** = raridade média das matérias-primas (ponderada pela quantidade): < 0,25 Rústico, < 0,6 Bom, < 1,0
  Fino, senão Primoroso; o Troféu da Fera (opcional, gasto) sobe um degrau.
- Peça feita rola a raridade por qualidade (C/I/R/É): Rústico 60/30/10/0, Bom 30/45/22/3, Fino 0/45/40/15, Primoroso
  0/20/50/30 — **nunca abaixo do item base e nunca acima do teto do nível: Incomum, Incomum, Incomum, Raro**. Afixos
  por `ProceduralItemGenerator` (semente tirada da RNG). Peça feita **não tem opção MU** (excelente/sorte/antigo).
- 6 receitas de equipamento (chapéu e botas nível 1; gibão e arco 2; espada de ferro 3 e 4) e 5 de runas.

### 26.3 Benzedura de arma do Vô Jorge (`ImbueCatalog`, `ImbueRules`, `ImbueEffectApplier`)

4 partes + ouro → um bônus na arma da mão por **8 lutas**. Uma de cada vez (nova substitui). **Divide a vaga da bênção
da estrada**: luta que já tem bênção (`world_event`) pausa a benzedura, e luta sem arma na mão também; só conta luta
em que ela valeu (salvo no `CraftingManager`).

| Benzedura | Nível | Parte | Ouro | Efeito |
|---|---|---|---|---|
| Fio da Presa | 1 | Presa de Lobo ×4 | 10 | dano +6% |
| Ligeireza da Aranha | 1 | Seda ×4 | 10 | velocidade de ataque +6% |
| Veneno do Ferrão | 2 | Ferrão ×4 | 20 | crítico +4 pp |
| Casca de Pedra | 3 | Lasca ×4 | 30 | defesa +10% |
| Sede do Morcego | 4 | Asa ×4 | 40 | +2 de vida/s ("roubo de vida" do Tibia, como regeneração) |

### 26.4 Forja de fusão de Adamas (`FusionRules`)

2 ou 3 cópias do **mesmo** item (da mochila, sem opção MU) → a melhor (raridade, depois refino) sobe **um degrau** com
afixos novos e mantém o refino. Chance: 2 peças 50%, 3 peças 75%; −15 pp para Épico; Troféu +15 pp; teto 90%. Ouro:
15 × raridade-alvo. **Nunca perde tudo**: se falhar, a melhor volta igual e cada cópia derretida vira 2 de ferro.
Mesmo teto de raridade do 26.2.

### 26.5 Runas (Lavine e Maria)

Consumíveis comuns (`Resources/Items/item_runa_*`, usados da mochila como poção por qualquer primo); **só Lavine e
Maria desenham** (`AdvancedRecipeSpec.Crafters`). Cura 60 · Cura Grande 120 (nível 4) · Clareza 60 de mana · Couro Duro
defesa +15% por 15 s (nível 2) · Vento velocidade +20% por 12 s (nível 3). Nenhuma runa ofensiva: consumível sem
recarga empilharia sem limite e o modelo de TTK não as vê (como as poções).

### 26.6 Orçamento (`crafting_stack.py`)

- **Benzedura**: entra como mais uma candidata da vaga do evento em **todas** as regiões (limite superior). Nenhuma
  supera a Cantoria (dano +8%), então a pilha do §14 não muda em nada.
- **Peças feitas/fundidas**: limite superior = ao chegar em cada região o herói ganha uma peça no teto do nível em
  cada slot das receitas (arma, chapéu, peito, botas), como se toda fusão desse certo, por cima do Monte Carlo do r8.
  Orçamentos: sozinho ≥ 92% do r8 no chefe / 80% nos comuns; a pilha do §14 no herói com as peças; pilha + feitas +
  refino ≥ 12 s; e **tudo** (pilha + refino + asas + frutas + peças excelentes perfeitas + peças da forja nos slots que
  sobram — slot é excelente *ou* forjado) dentro do orçamento global das opções (≥ 66% do r8, ≥ 12 s).
- Tetos testados: Raro/Raro/Épico/Épico e Incomum/Incomum/Raro/Raro estouram (Montanha e NG+1: chefe < 92% / tudo < 12 s);
  **Incomum/Incomum/Incomum/Raro** fica dentro. Ou seja: a forja é piso confiável, não teto novo — o melhor ainda vem
  do chefe.

| Ciclo | Bioma | Chefe: r8 → pilha → pilha c/ benzedura | Chefe: r8 → r8 c/ peças feitas → pilha+feitas+refino | 2 comuns: r8 → pilha c/ benzedura → r8 c/ feitas |
|---|---|---|---|---|
| NG+0 | Floresta | 16,5 → 13,8 → **13,8** s | 16,5 → 15,6 → **13,2** s | 4,8 → 3,7 → 4,5 s |
| NG+0 | Deserto | 18,2 → 14,9 → **14,9** s | 18,2 → 17,6 → **13,8** s | 6,1 → 4,7 → 5,6 s |
| NG+0 | Montanha | 18,4 → 14,4 → **14,4** s | 18,4 → 17,2 → **13,4** s | 6,4 → 4,9 → 6,3 s |
| NG+0 | Cavernas | 20,0 → 16,2 → **16,2** s | 20,0 → 19,0 → **14,5** s | 4,6 → 3,4 → 4,0 s |
| NG+1 | Floresta | 18,5 → 14,6 → **14,6** s | 18,5 → 17,1 → **12,9** s | 3,3 → 2,5 → 3,3 s |
| NG+1 | Deserto | 18,5 → 14,5 → **14,5** s | 18,5 → 17,2 → **13,0** s | 3,6 → 2,8 → 3,3 s |
| NG+1 | Montanha | 18,5 → 14,5 → **14,5** s | 18,5 → 17,4 → **13,0** s | 4,1 → 2,9 → 4,0 s |
| NG+1 | Cavernas | 19,8 → 15,9 → **15,9** s | 19,8 → 18,8 → **14,0** s | 4,0 → 2,9 → 4,0 s |

Resultado: **dentro do orçamento**; `pytest tools/balance` verde (51).
