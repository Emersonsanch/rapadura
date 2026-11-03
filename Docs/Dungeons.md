# Rapadura — Masmorra: o Ossuário dos Artífices (Catacumbas de Marfim)

> Pedido (TODO.md, "Nova Masmorra"): masmorra clássica subterrânea no estilo *MU Online Dungeon* / *Tibia* — corredores
> estreitos de pedra, nichos de crânios com fogo-fátuo, portões gradeados trancados por chaves de osso, sarcófagos com
> armadilha, hordas de esqueletos e um chefe. Nomes, textos e arte são nossos.
> Código: `Assets/Scripts/Gameplay/Dungeon/` (regras, diretor, objetos) e `Assets/Scripts/UI/Dungeon/` (botão no mapa, HUD).
> Conteúdo: `Assets/Resources/Dungeons/Ossuario/` (inimigos + tabela de loot), `Assets/Resources/Items/Keys/BoneKey.asset`,
> `Assets/Resources/Items/Dungeon/` (armadura e espólio). Testes: `Assets/Tests/EditMode/Dungeon/`.
> **Números são estimativa (fora do modelo r8/r9 do Balance), como as salas de evento.**

## 1. História (Luz × Trevas)

Os **Artífices** selaram o Limiar nas profundezas das Grutas e deixaram guerreiros voluntários guardando o Selo dentro
de catacumbas de marfim (Docs/MetodologiaMapasERacas.md §4.1). Com o Cristal enfraquecido, os guardiões acordaram
**desmemoriados**: não são maus, só perderam a luz. Vencidos em combate leal, a memória volta por um instante e a faísca
sobe em paz para a Coluna da Glória. O tom é infantil: caveiras "sonolentas", "Buu!", nada sangrento.

Cada primo tem uma fala ao entrar e outra ao vencer o Colosso (`DungeonTexts`, `dungeon.line.*`); os avós citados são
Vó Edna e Vô Jorge.

## 2. Entrada

- No **mapa mundi**, com o token parado nas **Grutas do Limiar** (`DungeonRules.Region = Caves`), aparece o botão
  **"Entrar: Ossuário dos Artífices"** na fileira de ações (`DungeonMapButton`, mesmo visual do "Conversar").
- Exige **nível 12** (`DungeonRules.MinLevel`); abaixo disso uma mensagem curta explica. Não há item de entrada: as
  chaves se acham lá dentro.
- O clique monta um `EncounterPayload` com `dungeonId = "ossuario"` (`DungeonDirector.BuildPayload`) e entra pelo mesmo
  salto das salas de evento (`WorldMapController.TryStartEventRoom`, encontro **dirigido**). Na cena `Main`, o
  `DungeonDirector` se cria sozinho (sem editar cena), a arena veste o chão de caverna e o diretor ergue o salão.
- Morrer ou fugir resolvem pela arena como sempre (volta ao mapa, sem ressuscitar dentro); os portões já abertos
  continuam abertos na próxima visita.

## 3. Planta (gerada em código)

Um corredor de pedra (`DungeonLayout`, ~26 m) do herói até o centro da arena, com três salas:

| Sala | Guardiões | Chave |
|---|---|---|
| **Vestíbulo** | 2 Guerreiros Esqueleto + 2 Caveiras Flutuantes; 2 sarcófagos | o 1º guerreiro carrega a chave do portão da Galeria |
| **Galeria dos Arqueiros** | 2 Arqueiros + 2 Guerreiros + Feiticeiro das Ossadas; 2 sarcófagos; parede de ossos rachada com o nicho secreto | o Feiticeiro carrega a chave do portão do Colosso |
| **Câmara do Colosso** | O Colosso de Crânios (chefe) | — |

Uma sala só acorda quando o herói chega perto **e** os portões antes dela estão abertos (`DungeonDirector.ShouldWake`).

## 4. Objetos

- **Portão gradeado** (`LockedDungeonGate`): sem Chave de Osso nada muda ("Trancado! Precisa de: …"); com a chave, ela é
  gasta e as grades sobem (som de engrenagem). Aberto fica salvo (`DungeonProgress`) e nunca pede chave de novo.
- **Sarcófago** (`SarcophagusInteractable`): abre uma vez por save. **70%** espólio (2 sorteios da tabela das
  catacumbas), **30%** emboscada de **2 Caveiras Flutuantes** com fumaça roxa.
- **Parede de ossos** (`BreakableBoneWall`): cai na hora com impacto contundente — skill de **Terra em área** (Maremoto do
  Joaquim e afins) ou **explosivo** (Granada de Faísca do Ícaro, Explosão) a até raio + 1,5 m — ou com **8 golpes comuns**
  (qualquer primo consegue). Atrás, o **baú lendário**: a Armadura Peitoral de Costelas (no mínimo Épica) + 3 sorteios.

Vencer o Colosso **re-sela** o Ossuário (`DungeonProgress.RecordClear`): portões fecham, sarcófagos se enchem, a parede se
refaz — a próxima visita é uma corrida nova (chaves e espólio de novo), estilo MU/Tibia.

## 5. Bestiário (`Resources/Dungeons/Ossuario/Enemies`, fora das tabelas aleatórias das regiões)

| Id | Nome | Vida | Notas |
|---|---|---|---|
| `enemy_floating_skull` | Caveira Flutuante | 40 | 7,5 m/s; flutua a 1,2 m com balanço senoidal; mergulho e recuo de 2 m; luz azul-esverdeada (1,5 / 3 m) |
| `enemy_skeleton_warrior` | Guerreiro Esqueleto de Cobre | 180 | 3,8 m/s; armadura 25%; a cada 6 s guarda de 2,5 s com escudo de osso (−60% dano frontal, reflete 15% de projéteis); larga Fragmentos de Osso |
| `enemy_skeleton_archer` | Arqueiro Esqueleto do Fosso | 95 | alcance 14 m; recua se o herói chega a < 4 m; flecha de costela com rastro roxo (−20% velocidade por 3 s) |
| `enemy_bone_sorcerer` | Feiticeiro das Ossadas | 220 | esferas espectrais; **Erguer as Ossadas** a cada 12 s (canaliza 2 s): reacende até 2 guerreiros caídos da sala com 40% da vida |
| `boss_bone_colossus` | O Colosso de Crânios | 1400 | chefe lento, sem empurrão; larga o Elmo Crânio de Cobre (Épico) |

**Regra de ouro anti-exploit:** guardião reanimado (`DungeonEnemyTag.IsReanimated`) dá **0 XP e nenhum drop** ao cair de
novo (`IKillExperienceScaler` antes de qualquer multiplicador; `IEnemyLootSource.SuppressLoot`), e nunca carrega chave.

**Colosso — 3 fases** (`DungeonRules.ColossusPhaseFor`): 100–70% pancada da clava com onda de choque radial (afaste 5 m);
70–30% arremesso de 3 Crânios Saltitantes que ricocheteiam nas paredes e explodem; < 30% **Prisão de Costelas**: jaula de
ossos em volta do herói, quebra com 3 golpes; se não quebrar a tempo, o golpe carregado (44) acerta.

Visual: por ora **placeholders** (esfera/cápsula marfim, cobre e lilás) + as luzes e partículas dos comportamentos; os
modelos low-poly procedurais (`ProceduralEnemyModels`) ainda não têm receita de esqueleto.

## 6. Itens e loot

| Id | Nome | Tipo | Notas |
|---|---|---|---|
| `item_bone_key` | Chave de Osso Rúnica | chave (QuestItem) | Rara, peso 0,1, sem durabilidade, empilha 10; só vem dos guardiões-chaveiros |
| `armor_bone_chest` | Armadura Peitoral de Costelas | peito | Defesa 14, +4% crítico; baú do nicho e tabela (raro) |
| `armor_bone_helm` | Elmo Crânio de Cobre | cabeça | Defesa 8, +20 mana máx.; Colosso e tabela (raro) |
| `item_bone_fragment` | Fragmento de Osso Antigo | material | vende/artesanato |
| `item_qocho_holed_coin` | Moeda Furada de Qocho | material | vende |
| `item_rusty_watch_sword` | Espada Enferrujada do Vigia | arma | dano 16 |

**Tabela das catacumbas** (`LootTable_Catacombs`, um `BiomeDefinition` usado só pelas entradas de loot, que não é região):
fragmento 10, moeda 7, espada 3, peitoral 1, elmo 1. Guardiões rolam essa tabela pelo `IEnemyLootSource`.

**Conjunto Armadura de Marfim dos Artífices — "Toque Espectral"** (`SpectralTouchSetBonus`): com as **2 peças** vestidas
(e não quebradas), cada **acerto crítico** do herói tem **15%** de lançar um estilhaço de osso que atinge até 3 inimigos a
4 m por 35% do dano do crítico (os estilhaços nunca são críticos, então não encadeiam). A "Resistência à Sombra" do pedido
original ainda não existe como atributo: o elmo dá Defesa no lugar.

## 7. HUD

`DungeonHud` (UI Toolkit, tema `rap-theme`, `Resources/UI/Dungeon.uss`): nome do Ossuário e da sala, chaves de osso na
mochila, objetivo atual (`DungeonDirector.GoalKey`), barra de vida do Colosso quando ele acorda e a faixa de avisos
(falas dos primos, "Prisão de Costelas!", "+ Chave de Osso Rúnica"). Nunca bloqueia clique.

## 8. Save

`DungeonProgress` (chave `Dungeons`, registrado no `GameManager`, `SaveManager` e `NewSessionReset`): portões abertos,
sarcófagos abertos, parede quebrada, baú pego, visitas e vitórias. Save antigo sem a entrada carrega tudo selado.

## 9. Pendências

- Modelos próprios dos esqueletos/Colosso (hoje placeholders) e sons dedicados (hoje cues procedurais do `DungeonFx`).
- Atributo de resistência a elemento (Sombra) para o elmo.
- Encaixe no catálogo de mapas estilo MU (mapa 24b "Catacumbas de Marfim", nível 35–45 em
  `Docs/MetodologiaMapasERacas.md`): hoje a entrada é pelas Grutas do Limiar com nível 12.
- Teste PlayMode de uma corrida completa (só há EditMode).
