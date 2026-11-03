# Evolução de classe (graduações, combo de classe, buffs de grupo, Nível de Mestre)

Inspirado na 2ª/3ª classe e no Master Level do MU Online. Código em `Assets/Scripts/Gameplay/ClassEvolution/`
(regras puras, sem Unity onde dá: o servidor da FASE 12 roda as mesmas chamadas) e `Assets/Scripts/UI/ClassEvolution/`.
Testes: `Assets/Tests/EditMode/ClassEvolutionTests.cs`; orçamento: `tools/balance/power_stack.py` (seção "Evolução de
classe") + `tools/balance/test_class_stack.py`.

## 1. Graduações por missão da família

Cada primo tem **5 graduações** (`ClassEvolutionCatalog.Graduations`, `MaxTier = 5`): as 3 da campanha (8/14/25) e as 2
do teto 500 (G4 no 150, G5 no 350, Progressao500 §3). Cada uma é uma missão de história: um membro da família oferece
(tiers 1, 2 e 4: um primo/irmão, cada par mentor→primo uma vez só; tier 3: **Vó Edna** em pessoa; tier 5: Vó Edna pelo
rádio do Vô Jorge), o primo caça bichos, volta e entrega.

| Primo | Graduação 1 (nv 8, Areias Flamejantes, 15) | Graduação 2 (nv 14, Grutas do Limiar, 20) | Graduação 3 (nv 25, Serra de Kaggen, 30) |
|---|---|---|---|
| Maithe | Rastreadora do Falcão — "Medidor de Vento" (Ícaro) | Flecheira da Ventania — "Três Meses e Onze Dias" (Maria) | Olho de Liyongo — "O Arco de Damasqueiro" (Vó Edna; o arco que o Vô Jorge entalhou) |
| Maria | Curandeira do Sol — "Quem Cuida do Guardião" (Joaquim) | Guardiã da Aurora — "Luz e Sombra" (Lavine) | Candeia da Glória — "A Medalhinha de Sol" (Vó Edna) |
| Joaquim | Guardião das Raízes — "A Tampa de Barril" (Maria) | Guardião do Tugai — "Saber Sair da Frente" (Lavine) | Mestre das Raízes — "O Escudo na Parede" (Vó Edna) |
| Ícaro | Artífice — "Não Conta pra Maithe" (Maithe) | Mestre das Máquinas — "O Aviso dos Artífices" (Joaquim) | Herdeiro de Kurdalægon — "O Rádio do Vô Jorge" (Vó Edna) |
| Lavine | Feiticeira da Noite Rubra — "Friozinho na Barriga" (Maithe) | Guardiã da Sombra Macia — "A Sombra Também É Macia" (Maria) | Barqueira da Lua — "A Fita Vermelha" (Vó Edna) |

**G4 "do Elemento" (nv 150, 40 bichos) e G5 "da Coluna" (nv 350, 50 bichos)** — `CampaignMaxTier = 3` separa as duas:

| Primo | G4 — capital do elemento (mentor) | G5 — Reinos do Céu, mapa 91 (Vó Edna pelo rádio) |
|---|---|---|
| Maithe | Sopro de Farnkesh — "Os Ninhos de Pedra" (Lavine; Picos de Farnkesh, mapa 54) | Vento da Coluna — "Contar Todo Mundo" |
| Maria | Lâmpada de Mingcheng — "Os Nove Anéis" (Ícaro; Planície de Mingcheng, 41) | Aurora da Coluna — "A Primeira Luz" |
| Joaquim | Raiz de Jhilpur — "A Cidade nas Raízes" (Maithe; Delta de Jhilpur, 31) | Rio da Coluna — "O Rio de Luz" |
| Ícaro | Forja de Hinokura — "A Ilha-Forja" (Maria; Arquipélago de Hinokura, 65) | Engenho da Coluna — "A Roda das Doze Jarras" |
| Lavine | Lua Branca — "O Porto da Lua Branca" (Joaquim; Mar Gelado, 59) | Silêncio da Coluna — "A Noite que se Cala" |

Os mapas 31–100 ainda não existem, então G4/G5 contam **bichos de nível ≥ 120 / ≥ 320 em qualquer mapa**
(`GraduationSpec.MissionMinCreatureLevel`, `KillCounts`; o nível vem do `EnemyLevelScaler` ou de
`EnemyLevelScaling.ResolveLevel`); o lugar (`MissionMapId`, textos `ui.class.place.map_NN`) é o da história e aparece no
status: "Os Nove Anéis: 12/40 bichos de nível 120+ (Planície de Mingcheng)". Quando os mapas entrarem, dá para trocar o
filtro pelo mapa sem mexer no save.

Cada missão tem 4 falas (oferta do mentor, incentivo com a contagem, fala de entrega, resposta do primo) em en + pt-BR
(`ClassEvolutionTexts`). Irmãos: Maithe↔Ícaro, Joaquim↔Maria; Lavine é prima de todos. Os nomes seguem a bíblia do mundo
(`Docs/WorldBible_Maniqueismo.md` §3.19) e o elemento de cada primo (§1.3): Maithe = Vento Vivo, Maria = Luz, Joaquim =
Água Viva, Ícaro = Fogo Vivo (Engenhos da Luz), Lavine = Ar Sereno.

Fluxo (`ClassEvolutionRules`): `Status` → `LevelTooLow` / `Available` / `InProgress` / `ReadyToTurnIn` / `AllDone`;
`Accept` (nível mínimo), `RecordKill` (tiers 1–3: só abates na região da missão, via `BestiaryCatalog`; G4/G5: pelo nível
do bicho; chefes contam 1; nunca passa do alvo), `TurnIn` (tier + 1). `Sanitize` conserta saves ruins (tier 0..5).

### 1.3 Visual (honesto)

Nada de modelo novo: `ClassEvolutionVisuals` aplica um **tingimento leve** da cor do tier (MaterialPropertyBlock por
renderer, materiais compartilhados intactos; força 15/25/35%) e **um acessório low-poly de primitivas** por graduação
(gorro de feltro, lenço bordado, pena, fita, faixa, bornal, resplendor; G4 um **manto** curto nas costas — de penas,
dos nove anéis, de raízes, avental de forja, da lua branca; G5 um **anel de luz nos pés**, o primeiro degrau da Coluna da
Glória), posicionado pelos bounds do modelo. Tingimento 38% (G4) e 40% (G5). As graduações empilham: no tier 5 o primo usa
os cinco.

## 2. Skills de graduação e buffs de grupo

`GraduationSkillSpec` (dados puros) → `GraduationSkillFactory` monta um `SkillDefinition` em runtime (custo, recarga,
dano, atordoamento, cura, buff/debuff) que passa pelo `SkillManager` normal. `CharacterSkillSet.FindSkill` também
resolve essas ids, então o save mantém o nível delas.

- **Tier 1 = buff de grupo** (raio 10): Olhar do Falcão (+2% crítico), Calor da Lâmpada (+1 vida/s), Raiz que
  Segura (−4% dano recebido), Engenho de Reforço (−4% recarga), Lua Guardiã (+10% mana). O conjurador ganha pela
  própria skill; `ClassEvolutionProgress.SpreadGroupBuff` espalha a cópia para até **4 aliados vivos** no raio, os mais
  próximos primeiro (`PartySupportRules`, grupo de 5 como no MU). Hoje o herói joga sozinho (a dupla é silhueta), então só ele recebe.
- **Tier 2 = skill da classe** que fecha o combo (alvo único ou área pequena, com lentidão/atordoamento).
- **Tier 3 = skill de maestria** em área (recarga 22 s).
- **Tier 4 = passiva** (`GraduationUpgradeSpec`, `ClassEvolutionCatalog.Upgrades`): o buff de grupo do tier 1 vai de 10 m
  para **14 m** e a skill do tier 2 ganha **controle, nunca dano** — Vento Mais Longe (Flecha da Ventania +0,4 s de
  atordoamento), Lâmpada dos Nove Anéis (Candeia que Acende −30% de velocidade por 3 s), Raízes de Jhilpur (Pancada da
  Correnteza −30% por 3 s), Fole de Hinokura (Bobina de Cata-Vento −25% por 3 s), Luar do Porto (Fita Vermelha +0,4 s).
  `ClassEvolutionCatalog.EffectiveSkill(spec, tier)` dá a versão que o primo usa; `GraduationSkillFactory.Get(id, tier)`
  reescreve **o mesmo** `SkillDefinition` em cache quando o tier cruza o G4 (a skill aprendida mantém nível, slot e save).
- **Tier 5 = 2ª skill de maestria** em área (recarga 30 s) com **o mesmo orçamento por segundo** da do tier 3 (dano,
  dano/nível, atordoamento, cura e tempo de lentidão por segundo ≤ os do tier 3; área ≤): Sopro da Coluna (Maithe), Primeira
  Luz (Maria, cura 40), Rio de Luz (Joaquim, atordoa 1,3 s e −30% por 5 s), Roda das Doze Jarras (Ícaro, −30% por 5 s),
  Noite que se Cala (Lavine).

Ao aplicar (`ApplyTo`, em `CharacterSelected`/`CharacterSelectionRestored`/`GameLoaded`), as skills de graduação dos
outros primos são esquecidas e as do primo atual aprendidas; vão para o primeiro slot livre da barra (o 6º, com a
ultimate no 5º) — as demais ficam aprendidas para trocar na árvore de skills.

## 3. Combo de classe

`ClassComboSpec` + `ClassComboTracker` (puro, relógio explícito): duas skills da classe + a skill do tier 2, até 3 s entre
cada; errar reinicia. Liberado na graduação 2. Bônus: Maithe/Lavine +4% dano por 5 s, Ícaro +3% por 6 s, Maria/Joaquim
−10% custo de mana por 6 s.
Nomes: Rastro, Chuva e Ventania (Maithe), Luz Completa (Maria), Muro d'Água (Joaquim), Experimento Completo (Ícaro),
Noite Rubra (Lavine).

## 4. Nível de Mestre

`MasterLevelRules`: com a 5ª graduação (a última, `ClassEvolutionCatalog.MaxTier`; era a 3ª antes do G4/G5) e o nível máximo (**500**,
`LevelCurveMath.MaxLevel`; era 50 antes do teto 500, `Docs/Progressao500.md` §3.3), o XP de abate vai para a barra de mestre
em vez do XP normal (`EnemyRewardGranter` → `ClassEvolutionProgress.TryRouteMasterExperience`). **100 níveis de mestre**
(`MaxMasterLevel`, = pontos da árvore inteira), XP `xp(500) × 0,5 × (1 + 0,05 × nível)` (10 672 000 no mestre 0; era
25 000 × (1 + 0,15 × nível)), 1 ponto por nível.

**Migração de save (Mestre do tempo do nível 50):** nada no registro é reescrito. Um primo com 3ª graduação ou mais (`LegacyRequiredTier`), nível ≥ 50 e
nível de mestre ≥ 1 vira "mestre guardado" (`MasterLevelRules.HasKeptMasterProgress`): os bônus da árvore continuam
ligados, os pontos já ganhos podem ser gastos e "começar de novo" funciona (`CanUseTree`), o XP da barra fica como está.
O XP de abate volta a ir para o nível normal (51, 52, …) até o 500; lá, **com a G5 entregue**, a barra de mestre volta a
encher de onde parou (os limiares novos são maiores que os antigos, então o `Sanitize` não corta o XP guardado). Painel: "Nível de Mestre N
(guardado). A barra de mestre volta a encher no nível 500." Testes: `Level500Tests` (gate, migração por JSON antigo).

Árvore de Mestre: 4 linhas, **100 pontos** = árvore cheia (`MasterTreeBranch`; o painel mostra um título por linha). Na
**Raiz** (os 30 pontos de antes, ids mantidos):

| Linha 1 | máx | Linha 2 (pede 3 pontos acima) | máx |
|---|---|---|---|
| Couro Curtido: +1% vida/ponto | 5% | Pé no Chão: −0,6% dano recebido/ponto | 3% |
| Mão Firme: +0,5% dano/ponto | 2,5% | Olho Vivo: +0,4% crítico/ponto | 2% |
| Fôlego de Caravaneiro: +2% mana/ponto | 10% | Passo Ligeiro: +1% velocidade/ponto | 5% |

As **3 linhas novas** (Progressao500 §3.3) nunca dão dano, crítico, velocidade de ataque, recarga ou custo; cada coluna
abre livre e o nó seguinte pede 3 pontos no anterior:

| Apoio (25) | máx | Utilidade (25) | máx | Laço (20) | máx |
|---|---|---|---|---|---|
| Luz Partilhada: buff de grupo +0,4 m/ponto | +2 m | Passo de Peregrino: +2% fôlego máx. | 10% | Laço da Sela: +2% fôlego da montaria | 10% |
| Hino do Chamado: buff dura +4% nos aliados | 20% | Fôlego do Ouvinte: +2% recuperação de fôlego | 10% | Rédea de Luz: +3% XP de combate da montaria | 15% |
| Água Viva: +0,2 vida/s | 1/s | Taça do Bema: +1% mana máx. | 5% | Faro do Ari: Ari percebe bichos +6% mais longe | 30% |
| Manto de Luz: −0,4% dano recebido | 2% | Memória do Ouvinte: +2% XP de mestre | 10% | Ari Ligeiro: Ari busca o que cai +6% mais longe | 30% |
| Abraço da Família: +0,5% vida máx. | 2,5% | Fonte Serena: +1% recuperação de mana | 5% | | |

Nós de atributo do herói viram modificadores no `BuffController` (fonte `class_master_tree`); os especiais
(`MasterNodeEffect`, `MasterNodeSpec.Special`) são lidos por quem usa: `SpreadGroupBuff` (raio/duração do buff nos aliados),
`MasterLevelRules.AddExperience` (XP de mestre), `CarameloCompanion` (raios do Ari, via
`ClassEvolutionProgress.ActiveEffectBonus`). **Montaria:** `MountStamina`/`MountExperience` já estão no registro e em
`ActiveEffectBonus`, mas o código das montarias (`Gameplay/Mounts`, de outro agente) ainda não lê — até lá esses dois nós
não fazem nada no jogo. "Começar de novo" devolve todos os pontos.

## 5. Interface

`ClassEvolutionPanel` (no HUD do mapa, botão **Graduação** na Vila, mesmo critério do Patig, o Rastreador): título e graduação do
primo, a missão com as falas da família e **Aceitar**/**Entregar**, o combo da classe e a Árvore de Mestre (+1 ponto,
começar de novo). Textos em `ClassEvolutionPresenter` (puro, testado). Skill, visual e bônus chegam ao herói quando a
próxima cena de região carrega. Auto-save ao fechar.

## 6. Orçamento (tools/balance)

`power_stack.py` lê o catálogo (mantenha cada `Skill(...)`, `Grad(...)`, `Combo(...)` numa linha, posicional) e soma, como
**limite superior**: skills ativas de graduação na rotação no nível mínimo; **todos os 5 buffs de grupo** ligados a luta
inteira (grupo de cinco primos); o melhor combo de dano e o melhor de custo com uptime = duração / recarga da skill do
tier 2. A árvore de mestre cheia (100 pontos; só os nós de atributo contam na luta) entra como teste de estresse no NG+1 (o
nível 500 fica muito além da campanha modelada). G4/G5 abrem no 150/350, fora das lutas da campanha e da escala "até 100"
(`level_scaling.py`), então entram por regra própria: `high_tier_violations` (G4 só raio ≤ 14 m e controle — atordoamento
≤ 0,5 s ou lentidão —, nunca dano; G5 com recarga 30 s e dano, dano/nível, atordoamento, cura e tempo de lentidão por
segundo ≤ os do tier 3; árvore = `MaxMasterLevel` pontos; linhas novas sem atributo ofensivo). Mantenha cada `Upgrade(...)`,
`new MasterNodeSpec(...)` e `MasterNodeSpec.Special(...)` numa linha.

| Regra | Limite |
|---|---|
| r8 + classe: chefe | ≥ 88% do r8 |
| r8 + classe: comuns | ≥ 75% |
| pilha (refeição/relíquia/evento) + classe: chefe / comuns | ≥ 70% / ≥ 55% do r8 |
| nenhum chefe | < 12 s |
| NG+0, só ataque básico | mata em ≥ 75% do tempo que o chefe leva |
| parado, chefe mata em | 12–40 s |
| árvore de mestre cheia por cima | chefe ≥ 95% de sem ela |
| G4 / G5 / linhas novas da árvore | `high_tier_violations` vazio |

Resultado atual: dentro do orçamento (ex.: Grutas do Limiar NG+0 chefe 20,0 → 19,3 → 15,2 s; NG+1 Grutas do Limiar 19,8 → 18,6 → 14,7 s,
árvore cheia de 100 pontos 14,5 s — igual à de 30, as linhas novas não mexem no tempo de matar; G4/G5 sem violação).

## 7. Servidor (FASE 12)

Registro por primo `GraduationRecord` (tier 0..5, missão aceita, abates, nível/XP de mestre, pontos), salvo sob
`ClassEvolution`; saves antigos carregam no tier 0. Tudo que decide (status, abates, entrega, XP de mestre, gasto de
pontos, combo, alvos do buff de grupo) é estático e sem Unity — o servidor valida com as mesmas funções.
