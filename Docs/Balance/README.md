# Rapadura — Balanceamento (planilhas)

> Fonte única de verdade da curva de progressão do jogador + planilhas de revisão de
> inimigos e skills. Cobre o item da FASE 3 do `TODO.md`: "Curva de balanceamento
> documentada em planilha".

| Arquivo | Papel | Volta para o jogo? |
|---|---|---|
| `progression.csv` | Curva do jogador, níveis 1..50 — **fonte da verdade** | Sim, via *Import Progression CSV* |
| `enemies.csv` | Os 9 inimigos de `Assets/Resources/Enemies` (+ bioma e peso de spawn) | Não (só revisão) |
| `skills.csv` | As 45 skills de `Assets/Resources/Skills/**` (20 gerais + 25 de herói) | Não (só revisão) |

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

Comportamento atual: os valores padrão reproduzem **exatamente** a fórmula antiga
(`round(100 × 1.25^(nível-1))`, 2 pontos de atributo e 1 de skill por nível, vitais fixos em
100, sem bônus de dano). Acima do nível 50 (fim da tabela) a fórmula continua valendo. Se o
asset não existir, `PlayerStats`/`AttributeSet`/`SkillManager` usam os campos antigos do
inspector como fallback.

### Colunas derivadas de `skills.csv`

- `dot_damage` = soma dos ticks dos debuffs (`floor(duração / intervalo) × tickDamageOrHeal`).
  `hot_healing` idem para buffs.
- `dps_lv1` = (`base_damage` + `dot_damage`) / max(`cooldown`, `cast_time` + `recovery`).
- `damage_per_mana_lv1` = (`base_damage` + `dot_damage`) / `mana_cost`.
- `*_max` = o mesmo no nível máximo da skill (`base + damage_per_level × (max_level-1)`).
- Não inclui buffs de `AttackDamage`, crítico, cura nem efeitos de controle (CC).

---

## 2. Análise de sanidade (estado atual dos dados)

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

### 2.4 Atributos que não fazem nada (bug, não balanceamento)

`AttributeSet` aplica Vitalidade → `MaxHealth`, Espírito → `MaxMana` e Destreza →
`CriticalChance` no `BuffController`, mas **nenhum código lê esses três stats**: `PlayerStats`
usa sua própria vida/mana máxima e não existe cálculo de crítico. Só Inteligência
(`CooldownReduction`) e Poder Arcano (`AttackDamage`, e só para skills) têm efeito. Hoje, o
jogador que distribui seus 2 pontos por nível em Vitalidade/Espírito/Destreza desperdiça esses
pontos. O mesmo vale para o afixo `affix_vital` (MaxHealth) dos itens procedurais.

---

## 3. Sugestões de rebalanceamento (não aplicadas)

Nada abaixo foi aplicado. A ordem é por impacto.

1. **Ligar os atributos mortos** (código): `PlayerStats.MaxHealth/MaxMana` devem passar por
   `BuffController.GetModifiedValue(StatType.MaxHealth/MaxMana, base)`, e o dano (skills + ataque
   básico) deve rolar `CriticalChance`/`CriticalDamage`. Sem isso, 3 dos 5 atributos são
   armadilhas.
2. **Curva de XP**: trocar `_experienceCurveMultiplier` de 1,25 por **1,10** (total até o nível
   50: ~106 mil em vez de 22,4 milhões; nível 10 com 1 357 XP) — ou editar `xp_to_next` à mão
   na planilha com um polinômio (`100 × nível^1.5`: 690 mil no total). Os testes atuais
   ("default = fórmula antiga") devem ser atualizados junto.
3. **XP por bioma e por boss**: `_experienceReward` Floresta 10 → Deserto 18 → Montanha 28 →
   Cavernas 40; `_bossExperienceMultiplier` = **8** nos 4 bosses (80–320 XP por boss).
4. **Crescimento de vida do jogador** (já suportado pela planilha): coluna `hp` com +8 por nível
   (nível 5 = 132, nível 10 = 172). Junto com isso, Senhor da Guerra `_attackDamage` 26 → **20**
   (18 DPS, ~7,7 s para matar um jogador nível 6 com 140 de vida) e Lobo Alfa
   `_attackCooldown` 0,9 → **1,1** (16 DPS) — é o primeiro boss e hoje é o segundo mais letal.
5. **Escala de HP dos comuns por bioma**: Escorpião 26 → **45**, Golem 55 → **75**, Enxame de
   Morcegos 18 → **28** (eles vêm em bando), mantendo os da Floresta em 30. Cada bioma passa a
   pedir 2, 3, 5 e 2–3 golpes, com um degrau de dificuldade real.
6. **Ataque básico vs. skills**: reduzir `PlayerBasicAttack._damage` 15 → **10** (20 DPS) e fazer
   o básico passar pelo `BuffController` (`AttackDamage`), para Poder Arcano, Berserk e os combos
   também valerem nele. Outra opção é multiplicar o dano das skills por ~2. Também dá para usar a
   coluna `damage_mult` (+2%/nível) para o jogador sentir que ficou mais forte.
7. **Flecha Precisa**: `_manaCost` 10 → **15** (dano/mana 1,87, gasto 5 mana/s = igual à
   regeneração) ou `_cooldownSeconds` 3 → **4**. **Orbe Sombria**: CD 3,5 → **4**.
8. **Ultimates ~2,5× mais fortes por cast**: Eclipse Eterno 65 → **150**, Tempestade de Mil
   Flechas 55 → **130**, Exército Mecânico 45 → **100** de dano base (mantendo CD/mana).
9. **Chuva de Flechas**: base 18 → **26** (é área; dano/mana passa a 1,18 ≈ mediana).
   **Correntes das Trevas**: mana 22 → **16** (mantém o papel de controle; dano/mana 0,75, ~0,6× a mediana).
10. **`skill_arrow` (geral)**: dar a ela um custo de mana de 6 ou subir o CD para 2,5 s. Hoje ela
    é grátis e é a skill com maior DPS.

Depois de qualquer mudança em inimigos/skills: **Rapadura > Balance > Export Enemy and Skill
CSVs** e confira os números de novo nesta análise.
