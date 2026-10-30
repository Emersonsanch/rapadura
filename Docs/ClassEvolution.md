# Evolução de classe (graduações, combo de classe, buffs de grupo, Nível de Mestre)

Inspirado na 2ª/3ª classe e no Master Level do MU Online. Código em `Assets/Scripts/Gameplay/ClassEvolution/`
(regras puras, sem Unity onde dá: o servidor da FASE 12 roda as mesmas chamadas) e `Assets/Scripts/UI/ClassEvolution/`.
Testes: `Assets/Tests/EditMode/ClassEvolutionTests.cs`; orçamento: `tools/balance/power_stack.py` (seção "Evolução de
classe") + `tools/balance/test_class_stack.py`.

## 1. Graduações por missão da família

Cada primo tem **3 graduações** (`ClassEvolutionCatalog.Graduations`). Cada uma é uma missão de história: um membro da
família oferece (tiers 1–2: um primo/irmão; tier 3: **Vó Edna**), o primo caça bichos numa região, volta e entrega.

| Primo | Graduação 1 (nv 8, Areias Flamejantes, 15) | Graduação 2 (nv 14, Grutas do Limiar, 20) | Graduação 3 (nv 25, Serra de Kaggen, 30) |
|---|---|---|---|
| Maithe | Rastreadora do Falcão — "Medidor de Vento" (Ícaro) | Flecheira da Ventania — "Três Meses e Onze Dias" (Maria) | Olho de Liyongo — "O Arco de Damasqueiro" (Vó Edna; o arco que o Vô Jorge entalhou) |
| Maria | Curandeira do Sol — "Quem Cuida do Guardião" (Joaquim) | Guardiã da Aurora — "Luz e Sombra" (Lavine) | Candeia da Glória — "A Medalhinha de Sol" (Vó Edna) |
| Joaquim | Guardião das Raízes — "A Tampa de Barril" (Maria) | Guardião do Tugai — "Saber Sair da Frente" (Lavine) | Mestre das Raízes — "O Escudo na Parede" (Vó Edna) |
| Ícaro | Artífice — "Não Conta pra Maithe" (Maithe) | Mestre das Máquinas — "O Aviso dos Artífices" (Joaquim) | Herdeiro de Kurdalægon — "O Rádio do Vô Jorge" (Vó Edna) |
| Lavine | Feiticeira da Noite Rubra — "Friozinho na Barriga" (Maithe) | Guardiã da Sombra Macia — "A Sombra Também É Macia" (Maria) | Barqueira da Lua — "A Fita Vermelha" (Vó Edna) |

Cada missão tem 4 falas (oferta do mentor, incentivo com a contagem, fala de entrega, resposta do primo) em en + pt-BR
(`ClassEvolutionTexts`). Irmãos: Maithe↔Ícaro, Joaquim↔Maria; Lavine é prima de todos. Os nomes seguem a bíblia do mundo
(`Docs/WorldBible_Maniqueismo.md` §3.19) e o elemento de cada primo (§1.3): Maithe = Vento Vivo, Maria = Luz, Joaquim =
Água Viva, Ícaro = Fogo Vivo (Engenhos da Luz), Lavine = Ar Sereno.

Fluxo (`ClassEvolutionRules`): `Status` → `LevelTooLow` / `Available` / `InProgress` / `ReadyToTurnIn` / `AllDone`;
`Accept` (nível mínimo), `RecordKill` (só abates na região da missão, via `BestiaryCatalog`; chefes contam 1; nunca passa
do alvo), `TurnIn` (tier + 1). `Sanitize` conserta saves ruins.

### 1.3 Visual (honesto)

Nada de modelo novo: `ClassEvolutionVisuals` aplica um **tingimento leve** da cor do tier (MaterialPropertyBlock por
renderer, materiais compartilhados intactos; força 15/25/35%) e **um acessório low-poly de primitivas** por graduação
(gorro de feltro, lenço bordado, pena, fita, faixa, bornal, resplendor), posicionado pelos bounds do modelo. As graduações empilham: no
tier 3 o primo usa os três.

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

`MasterLevelRules`: com a 3ª graduação e o nível máximo (**50**), o XP de abate vai para a barra de mestre em vez do XP
normal (`EnemyRewardGranter` → `ClassEvolutionProgress.TryRouteMasterExperience`). 30 níveis de mestre, XP
25 000 × (1 + 0,15 × nível), 1 ponto por nível. Árvore de Mestre (30 pontos = árvore cheia, só com bônus pequenos):

| Linha 1 | máx | Linha 2 (pede 3 pontos acima) | máx |
|---|---|---|---|
| Couro Curtido: +1% vida/ponto | 5% | Pé no Chão: −0,6% dano recebido/ponto | 3% |
| Mão Firme: +0,5% dano/ponto | 2,5% | Olho Vivo: +0,4% crítico/ponto | 2% |
| Fôlego de Caravaneiro: +2% mana/ponto | 10% | Passo Ligeiro: +1% velocidade/ponto | 5% |

"Começar de novo" devolve todos os pontos. Modificadores aplicados no `BuffController` com a fonte `class_master_tree`.

## 5. Interface

`ClassEvolutionPanel` (no HUD do mapa, botão **Graduação** na Vila, mesmo critério do Patig, o Rastreador): título e graduação do
primo, a missão com as falas da família e **Aceitar**/**Entregar**, o combo da classe e a Árvore de Mestre (+1 ponto,
começar de novo). Textos em `ClassEvolutionPresenter` (puro, testado). Skill, visual e bônus chegam ao herói quando a
próxima cena de região carrega. Auto-save ao fechar.

## 6. Orçamento (tools/balance)

`power_stack.py` lê o catálogo (mantenha cada `Skill(...)`, `Grad(...)`, `Combo(...)` numa linha, posicional) e soma, como
**limite superior**: skills ativas de graduação na rotação no nível mínimo; **todos os 5 buffs de grupo** ligados a luta
inteira (grupo de cinco primos); o melhor combo de dano e o melhor de custo com uptime = duração / recarga da skill do
tier 2. A árvore de mestre cheia entra como teste de estresse no NG+1 (o nível 50 fica além da campanha modelada).

| Regra | Limite |
|---|---|
| r8 + classe: chefe | ≥ 88% do r8 |
| r8 + classe: comuns | ≥ 75% |
| pilha (refeição/relíquia/evento) + classe: chefe / comuns | ≥ 70% / ≥ 55% do r8 |
| nenhum chefe | < 12 s |
| NG+0, só ataque básico | mata em ≥ 75% do tempo que o chefe leva |
| parado, chefe mata em | 12–40 s |
| árvore de mestre cheia por cima | chefe ≥ 95% de sem ela |

Resultado atual: dentro do orçamento (ex.: Grutas do Limiar NG+0 chefe 20,0 → 19,3 → 15,2 s; NG+1 Grutas do Limiar 19,8 → 18,6 → 14,7 s,
árvore cheia 14,5 s).

## 7. Servidor (FASE 12)

Registro por primo `GraduationRecord` (tier, missão aceita, abates, nível/XP de mestre, pontos), salvo sob
`ClassEvolution`; saves antigos carregam no tier 0. Tudo que decide (status, abates, entrega, XP de mestre, gasto de
pontos, combo, alvos do buff de grupo) é estático e sem Unity — o servidor valida com as mesmas funções.
