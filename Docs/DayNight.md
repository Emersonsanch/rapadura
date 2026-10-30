# Rapadura — Dia e noite: bichos, ondas e visão noturna

> Pedido: "O cenário tem que ter dia e noite, com animais diferentes em cada período, renascendo, e a noite
> sendo mais perigosa." + "À noite o jogador deve ver menos."
> Números de balanceamento: `Docs/Balance/README.md`, seção "Dia e noite".

## 1. Visão geral

O relógio (`TimeOfDayManager`, 20 min reais por dia, salvo, para com a pausa) tem quatro fases:
**Madrugada/Amanhecer** (`Dawn`, 5h–7h), **Dia** (`Day`, 7h–18h), **Entardecer** (`Dusk`, 18h–20h) e
**Noite** (`Night`, 20h–5h). Antes, a noite só aumentava a chance de encontro (×1,25). Agora:

| | Dia | Amanhecer / Entardecer | Noite |
|---|---|---|---|
| Bichos | os de sempre | alguns "noturnos" misturados | maioria noturnos + espécies da noite |
| Ondas na arena | 2 (a do mapa + 1 reforço) | 2 | **3**, maiores e mais rápidas |
| Visão | normal | escurece aos poucos | mais escuro, neblina, horizonte mais curto, lamparina acesa |
| HUD | — | — | "Noite — os bichos estão mais bravos" |

Tudo puro e testado: `NightDangerRules` (perigo), `PeriodSpawnTable` (quem aparece), `NightVariantRules`
(como fica o bicho noturno), `NightVisibilityRules` (o que o jogador enxerga). Código em
`Assets/Scripts/Gameplay/Night/`, testes em `Assets/Tests/EditMode/NightDangerTests.cs`.

## 2. Quem aparece: tabelas região × período (`PeriodSpawnTable`)

Mesma ideia das tabelas de bioma (entradas com peso relativo). **Dia = a tabela do bioma** (os inimigos comuns, mesmo
peso; um teste trava as duas juntas). Chefes nunca entram (as lutas de chefe não mudam). Cada espécie fica na região do
modelo base, para o saque e o XP serem os da região.

| Região | Dia | Amanhecer | Entardecer | Noite |
|---|---|---|---|---|
| Bosque do Zarafshan | Lobo 5, Aranha 3 | + Lobo noturno 1 | Lobo 4, Aranha 3, + noturnos 1/1 | Lobo 2, Aranha 1, **Lobo noturno 4, Aranha noturna 3, Mardagayl 1, Coruja 2, Javalis do Lodo 2 (bando), Vaga-lumes 1** |
| Areias Flamejantes | Escorpião 5 | + noturno 1 | Escorpião 4, + noturno 1, **Vaga-lumes 1** | Escorpião 2, **noturno 5, Vaga-lumes 3** |
| Serra de Kaggen | Ushabti 4 | + noturno 1 | + noturno 1 | Ushabti 2, **noturno 5, Coruja 2, Ave-Graveto de Omöl 1** |
| Grutas do Limiar | Morcegos 6 | Morcegos 5, + noturno 1 | Morcegos 5, + noturno 1 | Morcegos 2, **noturno 4, Ubır 2** |

As Grutas do Limiar são sempre escuras para o olho, mas os bichos seguem o relógio (de dia lá dentro é "de dia" para eles).

### 2.1 Variante noturna (sem modelo novo)

O mesmo bicho, "acordado de noite": cor mais escura e azulada (luar; nenhum canal abaixo de 35%, nunca vira sombra),
**dois olhinhos amarelos brilhando** (esferas sem luz própria, então aparecem no escuro e na neblina como aviso),
10% maior, nome com "(noturno)" (aparece flutuando quando ele chega), mais vida/dano, percebe o herói de mais longe e
ganha mais XP e saque. Feito como o elite dos eventos da estrada (`NightVariantTag`, um quadro depois do spawn).

### 2.2 Espécies da noite (modelo próprio, 2026-10-30)

Cada espécie tem seu `EnemyDefinition` e seu modelo low-poly feito em código (`Docs/Animation.md` §7.2), substituindo os
recolors de lobo/morcego. O asset copia o **inimigo comum da região de casa** (vida/dano/XP de Bosque do Zarafshan 70/7/10,
Areias Flamejantes 110/11/18, Serra de Kaggen 140/13/28, Grutas do Limiar 170/7/40 — assim o balanço e o bestiário não mudam de faixa) e fica na
tabela do bioma de casa com **peso 0** (nunca aparece de dia no mapa; o bioma dá a tabela de saque e a região do
caderno). A noite aplica os multiplicadores da espécie (`NightVariantRules`, via `NightVariantTag`). Uma espécie
**visitante** (coruja na Serra de Kaggen, vaga-lumes no Bosque do Zarafshan) é escalada pela razão entre os comuns das duas regiões
(vida, dano e XP), então é tão dura lá quanto em casa; o saque continua sendo o da casa.

| Espécie (asset) | Casa / onde | Vida | Dano | XP | Saque extra | Teto por onda | Modelo e silhueta no escuro | Olhos |
|---|---|---|---|---|---|---|---|---|
| **Mardagayl, o Lobo-das-Sete-Noites** (`enemy_forest_werewolf`, mini-chefe raro, 1/16 da noite) | Bosque do Zarafshan, noite | ×5 (350) | +60% | ×5 | 3 | 1 | Lobão desgrenhado de ~1,6 m, corcova de juba, orelha caída, língua de fora; corre de quatro e **empina** para a patada. | vermelhos |
| **Ubır, o Morcegão Bebe-Luz** (`enemy_cave_vampire_bat`) | Grutas do Limiar, noite | ×1,8 | +30% | ×1,8 | 1 | 2 | Um morcegão vinho, asas de 2,4 m (o "M" das asas). | vermelhos |
| **Coruja-de-Fumaça** (`enemy_forest_shroud_owl`) | Bosque do Zarafshan; visita a Serra de Kaggen | ×0,8 | +20% | ×1,5 | 1 | 2 | Cara branca de coração que flutua no escuro; gira a cabeça. | amarelos |
| **Enxame de Faíscas Presas** (`enemy_desert_firefly_swarm`) | Areias Flamejantes (entardecer e noite); visita o Bosque do Zarafshan | ×1,05 (3 × 0,35) | ×0,4 | ×1,3 | 1 | 1 | Nuvem de 9 lanternas verde-amarelas que piscam. | amarelos + lanternas |
| **Javali do Lodo** (`enemy_forest_peccary`) | Bosque do Zarafshan, noite | ×1,3 | +30% | ×1,3 | 0 | 3, **bando de 2–3** | Porco escuro com crista de cerdas e queixo branco. | laranja (brasa) |
| **Ave-Graveto de Omöl** (`enemy_mountain_potoo`) | Serra de Kaggen, noite | ×0,6 | −30% | ×2 | 1 | 1 | Ave-graveto no seu galho; bico mínimo que abre num bocão rosa. | amarelos enormes |

- **Bando:** quando o javali sai no sorteio vêm 2–3 de uma vez; um bando pode esticar uma onda pequena até o seu
  mínimo, nunca além de `NightDangerRules.MaxWaveSize` (4). Só sai se o bando mínimo couber.
- **Olhos:** são células da paleta que brilham (emissão), não as esferas do `NightVariantTag` (a espécie tem
  `Tint` branco e `EyeColor` transparente). A variante noturna comum continua com as esferas amarelas.
- **Som:** cada modelo tem um `NightCreatureVoice` (chamado + som de ataque), ver `Docs/Audio.md`.
- **Conquista:** "Lua Cheia, Coragem Cheia" (`ach_night_werewolf`): vencer o Mardagayl.
- **Ainda não feito (comportamento):** uivo que chama 1 lobo noturno, mordida que cura o morcego, grito da coruja que
  deixa lento e a lamparina que a deixa tonta, queimadura dos vaga-lumes, investida em fila dos javalis e o grunhido
  do bando, o canto da Ave-Graveto de Omöl que chama +1 bicho; saques especiais (pelo de mardagayl, asa de Ubır,
  pena de coruja-de-fumaça, luz de vaga-lume, couro de javali, pena de Ave-Graveto de Omöl). Hoje elas lutam com a IA normal
  do arquétipo (quadrúpede/voador) e soltam o saque da região de casa.
- **Fora de propósito:** a Mula-sem-cabeça (assustadora demais para a faixa etária).

## 3. Ondas que renascem (`EncounterArenaController` + `NightDangerRules`)

- A primeira onda é o grupo sorteado no mapa (como antes). Ao limpar uma onda, se a fase **naquele momento** pede outra,
  ela chega depois do intervalo da fase, sorteada da tabela região × período. A luta é vitória quando a última onda cai.
- **Mudança de fase durante a luta vale na próxima onda**: a decisão (se vem mais uma, o intervalo, o tamanho e quem
  vem) usa a fase do momento em que a onda anterior foi limpa.
- Onda de reforço = primeira onda (sem chefes) × multiplicador da fase, de 1 a 4 bichos.
- Reforços seguram o ataque por menos tempo à noite (1,05 s em vez de 1,75 s) e aparecem com o aviso "Vem mais bicho!"
  ("…do escuro!" à noite).
- **Sem reforços e sem variantes:** desafio de chefe, grupo com chefe e a primeira luta do jogo (tutorial). Na
  emboscada de elite não há reforços e o elite não vira variante (o capanga pode).
- Interruptor no Inspector: `EncounterArenaController._reinforcementWaves` (desligado = uma onda só, como antes).

## 4. Menos visão à noite (`NightVisibilityRules`)

Legível e infantil: **nunca breu**. A escuridão é suave e acompanha o `DaylightFactor` do relógio (rampa no amanhecer e
no entardecer).

| | Normal | "Noite mais clara" | Grutas do Limiar (já escuras) |
|---|---|---|---|
| Luz ambiente na noite cheia | ×0,70 | ×0,90 | ×0,88 |
| Neblina extra (só na arena) | +0,022 | +0,010 | +0,008 |
| Distância da câmera (só na arena) | até 70 m | até 120 m | 70 m |
| Lamparina do herói | 8 m, acesa com a noite | igual | **sempre acesa** (mín. 60%) |

- Piso absoluto da luz ambiente: ×0,6, aconteça o que acontecer.
- A distância da câmera nunca fica menor que (distância câmera→herói + 40 m) nem maior que a da cena.
- **Olhos brilhando** dos bichos noturnos não usam luz da cena: continuam visíveis no escuro, servem de aviso.
- **Lamparina:** luz pontual quente, sem sombra (barata no celular), 8 m de raio; `NightVisibility.LightRadiusBonus`
  é o gancho para itens (lamparina melhor, Luz de vaga-lume) e a habilidade de montaria "luz da caverna"; teto 16 m.
- No mapa-múndi só a luz ambiente escurece (sem neblina, sem cortar a câmera, sem lamparina), para o mapa continuar claro.
- **Acessibilidade:** "Noite mais clara" (Configurações > Acessibilidade, `AccessibilitySettings.ClearerNight`)
  limita todo o escurecimento. O perigo (ondas, variantes) não muda, só o que se vê.

## 5. Avisos ao jogador

- HUD: pílula "Noite — os bichos estão mais bravos" no topo, enquanto for noite (arena e mapa; some na pausa).
- Dica do tutorial da primeira noite (`TutorialSteps.Night`): agora explica ondas maiores e mais rápidas, olhos
  brilhando, mais XP/saque e a lamparina.

## 6. Online (mundo aberto compartilhado, futuro)

As mesmas regras valem **por mapa**, com a fase vinda do **relógio do servidor** (não do `TimeOfDayManager` local).
Tudo em `NightDangerRules`/`PeriodSpawnTable` é função pura da fase (+ semente do encontro), então servidor e clientes
concordam desde que concordem na fase. Cada cliente aplica só a visibilidade (`NightVisibilityRules`) e a opção
"Noite mais clara" localmente — ela nunca muda o perigo, então não dá vantagem.
