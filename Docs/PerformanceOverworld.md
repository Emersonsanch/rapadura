# Performance do mundo contínuo (auditoria de código, 2026-11-11)

Escopo: o mundo costurado `overworld_mundo` (7200 × 15600 m, pedaços de 48 m, 16 regiões), os 14 drivers de
mecânicas que `OverworldView.Build` pendura na raiz (`OverworldView.cs:129-145`), o `RegionBossDirector`, o streaming
de pedaços e de clareiras. Auditoria **só de leitura** com o Unity fechado: nada foi medido em aparelho. Metas de
referência em `Docs/PlatformRequirements.md` (celular 30 FPS / ≤ 33 ms; PC 60 FPS / ≤ 16,6 ms; GC por frame ≤ 1 KB,
ideal 0 B).

Caminhos abaixo relativos a `Assets/Scripts/Gameplay/` (`Mechanics/` = `World/Map/Mechanics/`,
`Overworld/` = `World/Map/Overworld/`).

## Base de custo usada nas estimativas

- `OverworldRules.SurfaceAt` = **4 × `HeightAt`** (`Overworld/OverworldRelief.cs:412-422`), e cada `HeightAt` roda o
  `Evaluate` inteiro: mistura de zonas com `Shape` (ruído fBm), clareiras, rios, trilhas, vaus (com buckets espaciais
  de 64 m, já bons). Os testes de pedaço (`Tests/EditMode/Overworld*Tests.cs`, ex. `OverworldCeuTests.cs:617`) aceitam
  até **6 ms por pedaço no Editor** para ~289 amostras + props, o que põe `HeightAt` na faixa de **~5–15 µs (Editor/Mono)**,
  logo `SurfaceAt` ≈ **20–60 µs**. Em IL2CPP no celular médio espere da mesma ordem (IL2CPP ganha do Mono, o ARM perde
  do desktop). **Medir** com um teste EditMode de 10 000 `SurfaceAt` antes de priorizar pelo número absoluto.
- `MapRequirementHeroState.Capture` (`World/Map/MapRequirementHeroState.cs:19-65`) cria um `MapRequirementState` com
  **2 `HashSet` + 1 `Dictionary`** (`World/Map/MapRequirementRules.cs:69-71`), percorre a bolsa por `IReadOnlyList`/
  `IReadOnlyDictionary` (enumeradores em caixa), faz hash de cada `itemId` e busca cada montaria no catálogo:
  **~2–5 KB de lixo e ~10–40 µs por chamada** (cresce com a bolsa). `MapRequirementRules.Lookup` ainda devolve uma
  closure nova (`MapRequirementRules.cs:199`).

## Achados, ordenados por ganho estimado

Legenda: **[certo]** = comprovado lendo o código (a chamada acontece, a alocação existe); **[medir]** = o achado é
certo, mas o tamanho do ganho depende de número que só o Profiler dá.

| # | Onde | Por que custa | Ganho estimado | Correção (uma linha) |
|---|------|---------------|----------------|----------------------|
| 1 | `OverworldView.cs:436-469` (BuildChunk, 1 por frame em `Update` 361-394) | Monta `OverworldChunkContent` (17×17 `Sample` + 16 `Scatter*` + props), as duas malhas e **assa o MeshCollider no thread principal**. No Editor até 6 ms/pedaço só de geração (sem o bake); andando, cada 48 m atravessados pedem ~5–7 pedaços novos, um por frame. | **10–25 ms por frame de pico** no celular durante a caminhada (o maior risco de engasgo) **[medir]** | Gerar `Content` + vértices num `Task`/job (o relevo e `OverworldNoise` são C# puro, sem API do Unity) e no thread principal só subir a `Mesh` e chamar `Physics.BakeMesh` num job antes de ligar o collider. |
| 2 | **[aplicado]** `Mechanics/CostaDeltaTetoMechanicsDriver.cs:715-742` (DrawRing), chamado de `:496-501` (Delta) e `:340-343` (Costa) a cada `RedrawSeconds = 0.15 s` | Cada anel tem 40 pontos e **cada ponto chama `SurfaceAt`** (4 `HeightAt`). No Delta são 6 anéis sempre visíveis (4 ilhas + faísca + brejo) = **240 `SurfaceAt` = 960 `HeightAt` por redesenho**; na Costa 3 anéis por planície próxima. O anel do brejo (`ReedBogCenter`) nem se move. | **5–15 ms a cada ~9 frames** no Delta (pico periódico); 2–6 ms na Costa **[medir o ms; a contagem é certa]** | Amostrar o chão uma vez por anel (`Ground(center)` como o Céu faz em `CeuMechanicsDriver.cs:702`) ou guardar a altura por ponto na criação e só redesenhar anéis que mudaram de centro/raio; brejo construído uma vez. |
| 3 | **[aplicado]** `Mechanics/KaggenMechanicsDriver.cs:80`; `FarnkeshMechanicsDriver.cs:159` (durante rajada) e `:218` (junto a estação); `NartesMechanicsDriver.cs:576` (junto a um Ouro); `MarGeladoMechanicsDriver.cs:270` (junto a travessia); `ZoneOpen` de Céu `:290`, Limiar `:182`, Nartes `:150`, Rio `:164`/`:333` | `MapRequirementHeroState.Capture` **todo frame** (o Kaggen o chama em toda a região só para ler `.Hero`). Ver base de custo. | **~120–300 KB/s de GC** e 0,6–2,4 ms/s de CPU em Kaggen; idem nos estados de proximidade das outras regiões. Sozinho estoura a meta de 1 KB/frame **[certo]** | Um `HeroRequirementCache` com o `MapRequirementState` refeito só em evento (inventário, equipamento, montaria, troca de primo) ou a 1 Hz (como `CostaDeltaTetoMechanicsDriver.cs:187-191` já faz); Kaggen lê `CharacterSelectionState.Selected` direto. |
| 4 | **[aplicado]** `Overworld/OverworldView.cs:560-628` (UpdateSpawners, a cada `StreamTick = 0.2 s`) | Percorre **todas** as clareiras do mundo (`Layout.World.Zones`): ~392 áreas com grade de 70–100 m em ~95 km² dão **da ordem de 8–12 mil clareiras**, cada uma com `Dictionary<string>.ContainsKey` (hash de um id de ~30 caracteres) + `Vector2.Distance`. O índice por pedaço já existe e não é usado aqui (`Overworld/OverworldChunkBuilder.Index.cs:12-60`, `ChunkIndex.Spawns`). | **0,5–2 ms a cada 12 frames** no celular **[medir `Layout.World.Zones.Count`]** | Iterar só as clareiras de `ChunkIndex.Spawns` dos pedaços em `_wanted` (raio 96 m) em vez do mundo inteiro. |
| 5 | **[aplicado]** Altura de beacons fixos refeita todo frame: `ZarafshanMechanicsDriver.cs:242`, `LimiarMechanicsDriver.cs:222`, `AreiasMechanicsDriver.cs:199`, `CeuMechanicsDriver.cs:521` (5 fendas) e `:702`, `HinokuraMechanicsDriver.cs:581` (13 pontos da rota de erupção), `MarGeladoMechanicsDriver.cs:248-252` (5 cantos + `new[]`), `FarnkeshMechanicsDriver.cs:383-384` | Os pontos são estáticos (arrays `static readonly` dos layouts), mas cada frame chama `SurfaceAt` de novo para cada beacon visível. | **0,1–0,8 ms/frame** dentro da região (pior: Hinokura em alerta, Limiar com várias lâmpadas à vista) **[medir o ms]** | Calcular o `y` uma vez ao criar a linha (ou num `float[]` paralelo ao array do layout) e por frame só mexer em cor/altura. |
| 6 | `Overworld/OverworldLayouts.cs:50-60` (ForMap → `Get(regionId)`) + `OverworldLayouts.World.cs:155-165` (WorldOf) + `:172-189` (BuildWorld refaz as 16 regiões do zero); chamado também por `MapLayouts.IsPlayable` (`World/Map/Areas/MapModeController.cs:442`) | O mundo é construído com 16 regiões **novas**, mas `WorldOf`/`IsPlayable` antes disso já fazem cache da região avulsa: até **17 layouts vivos** (mundo + regiões tocadas), cada um com clareiras, props fixos, caminhos e, se consultado, o próprio `OverworldRelief`. Carregar o mapa paga a geração da região duas vezes. | **Memória: estimado +30–100 % do layout do mundo** (dezenas de MB gerenciados); **carregamento: +1 região (até +16) de tempo de build** **[medir]**: `GC.GetTotalMemory(true)` e `Stopwatch` em volta de `Get(WorldId)` num teste EditMode | `ForMap` responder pela `MapCatalog` (região do mapa) sem construir a região quando o mundo a contém, ou `BuildWorld` montar a partir de `Get(regionId)` e depois `Cache.Remove(regionId)`. |
| 7 | **[aplicado]** `Mechanics/ZarafshanMechanicsDriver.cs:87` → `ShowBeacons` `:181-191` | Roda **em qualquer ponto do mundo** (até 15 regiões longe): para cada árvore, `MarkKey` concatena string (`ZarafshanHarvestRules.cs:56`) + `GetDailyMark`, e 7 `SetBeacon` que só fazem `enabled = false`. | **~250 B/frame = ~15 KB/s de GC** fora do Zarafshan, ~5 µs/frame **[certo]** | `if (!inRegion) { if (_shown) HideAll(); return; }` antes de `ShowBeacons`, e chaves pré-montadas num `static readonly string[]`. |
| 8 | **[aplicado]** Os 14 `Update` fora da região: `Ceu:118` (Quiet → 10 `Hide`), `Hinokura:95` (HideFeedback, todos os beacons + ResetHeat), `Rio:113`, `Nartes:104` (3 funções de limpeza), `Kaggen:76` (8 riscos), `Areias:75` (11 linhas), `Farnkesh:99`, `MarGelado:120`, `Limiar:94`, `Zarafshan` (item 7) | Cada driver lê `transform.position` (nativo), testa o seu `Rect` e, como só Mingcheng/Mistura/Profundo guardam `_inRegion`, os outros **reescrevem `enabled = false` em todas as suas linhas a cada frame** (60–100 chamadas nativas). | **~30–80 µs/frame** no celular, constante **[medir; ordem de grandeza certa]** | `DriverHub` (abaixo) ou, no mínimo, uma flag `_shown` em cada `Hide*` como já tem `LeaveRegion` de Mingcheng (`MingchengMechanicsDriver.cs:154-169`). |
| 9 | LineRenderers simultâneos: `ProfundoMechanicsDriver.cs:481-512` (até **49** ladrilhos da Câmara de Fogo) + anéis de 72 pontos; Costa 3 anéis + beacon por planície; Mistura anéis de bolha/piche/pedra; cores reatribuídas todo frame em quase todos os drivers | Cada LineRenderer ativo é uma malha dinâmica e **um draw call** (não agrupa); reatribuir `startColor`/`SetPosition` com o mesmo valor ainda suja a malha. Pico realista numa região cheia: **20–60 linhas**. | **20–60 draw calls e ~0,5–2 ms de CPU de render** no celular, só na região de pico **[medir no Frame Debugger]** | Só escrever cor/posição quando mudar; ladrilhos do Profundo num único `Mesh` (ou um LineRenderer por fase); 14 materiais `Mat_*Line_Runtime` → um material compartilhado. |
| 10 | `World/Map/Areas/MapMonsterSpawner.cs:144` e `Enemies/RegionBossDirector.cs:165` | `Resources.Load<EnemyDefinition>($"...")` a **cada nascimento**: string interpolada + busca no índice de Resources. | ~0,05–0,3 ms + ~100 B por bicho nascido; picos quando várias clareiras acordam juntas **[medir]** | `static Dictionary<string, EnemyDefinition>` de cache (ou resolver o `EnemyDefinition` no `Configure`). |
| 11 | `Enemies/RegionBossDirector.cs:73-150` → `RegionBossRules.ArenaIn` (`Enemies/RegionBossRules.cs:329-346`) | 1 Hz, por chefe (8): `new[] { ... }` + até 2 `ZoneAt` lineares nas ~392 áreas do mundo. O resultado nunca muda para o mesmo layout. | ~20–40 µs/s e 8 alocações/s — pequeno **[certo]** | Guardar a arena resolvida no `Slot` na primeira vez. |
| 12 | **[aplicado em parte]** Alocações miúdas: `OverworldView.cs:420` (`new[]` por pedaço descarregado), `:439` (nome do pedaço concatenado), `MingchengMechanicsDriver.cs:517` (chave do baú por frame perto do baú), `NartesMechanicsDriver.cs:575` (chave do Ouro por frame perto), `MarGeladoMechanicsDriver.cs:248` (cantos `new[]` por frame no gelo fino), `ProfundoMechanicsDriver.cs:529` (só quando o ladrilho muda), `OverworldView.cs:610` (nome do spawner) | Cada uma é pequena, mas somam GC contínuo nos estados de proximidade. | ~50–200 B/frame em proximidade **[certo]** | Chaves pré-montadas (`static readonly string[]`), buffers de campo reutilizados, `foreach` sobre os três campos sem array. |
| 13 | `Overworld/OverworldChunkBuilder.cs:186-199` (16 `Scatter*` por pedaço) | Cada `Scatter*` varre as 289 células do `Grid.Info` atrás do seu terreno; 15 de 16 varrem tudo e saem. | ~10–20 µs por pedaço — irrelevante perto do item 1 **[certo]** | Máscara de terrenos presentes calculada uma vez em `BuildGrid`. |
| 14 | `GameObject.FindGameObjectWithTag` em 14 drivers + `OverworldView.cs:633` + spawners + chefe | Só repetido **enquanto o herói não existe** (carregamento, transição): ~20 buscas por frame nesses momentos. `FindAnyObjectByType` só aparece em caminho frio (`RegionBossDirector.cs:157`, uma vez por prefab; `MapModeController.cs:197,226` na montagem). | Desprezível em jogo; alguns µs/frame no carregamento **[certo]** | Opcional: o `DriverHub` passa o `Transform` do herói aos drivers. |

## Aplicado (2026-11-11)

Sem mudar regra de jogo (as regras puras não mudaram); testes em `Tests/EditMode/OverworldPerformanceFixTests.cs`.

- **Item 3**: `Mechanics/HeroRequirementCache.cs` guarda um `MapRequirementState` (e a sua `Lookup`) por driver; captura de
  novo só após 1 s, quando a bolsa muda ou num `InventoryChangedEvent` / `MountCollectionChangedEvent` /
  `CharacterSelectedEvent` / `CharacterSelectionRestoredEvent` (assina no `OnEnable`, solta no `OnDisable`). Usado em
  Farnkesh, Nartes, Mar Gelado, Céu, Limiar e Rio (ZoneOpen, estações, rajada, Ouro). Kaggen lê
  `CharacterSelectionState.Selected` direto. Ganho: de ~120–300 KB/s de GC a < 5 KB/s (uma captura por segundo).
- **Item 5**: `Mechanics/StaticGroundCache.cs` (altura por ponto fixo, uma amostra) no `Ground` do Céu e do Hinokura (fendas,
  cais, anéis, os 13 pontos da rota de erupção); Zarafshan e Limiar guardam o `y` num `float[]` ao criar a linha; Areias só
  recoloca o farol quando o abrigo mais próximo muda; Mar Gelado só refaz o contorno (sem `new[]`) quando o gelo fino mais
  próximo muda; a corda do Farnkesh só é redesenhada quando a rota muda. Ganho: 0 `SurfaceAt` por frame em regime.
- **Item 7**: Zarafshan esconde os faróis uma vez e sai antes de `ShowBeacons` fora da região; chaves `MarkKey` montadas no
  `Start`. Fim dos ~15 KB/s de GC fora do Zarafshan.
- **Item 2**: `DrawRing` do `CostaDeltaTetoMechanicsDriver` só reposiciona um anel quando muda centro ou raio; anel fixo
  (planície, baú do recife, brejo) segue o chão ponto a ponto **uma vez**; anel que se move (ilhas, faísca, borda da água)
  amostra o chão **uma vez, no centro**. Delta: de 240 `SurfaceAt` por redesenho para 5 (as ilhas e a faísca). A borda da
  água e as ilhas agora ficam planas na altura do centro (diferença visual mínima: água e ilha são planas).
- **Item 4**: `OverworldChunkBuilder.SpawnsNear` lê `ChunkIndex.Spawns` dos pedaços a ≤ 85 m do herói (ordenado, sem
  repetição); `UpdateSpawners` testa só essas clareiras. Prova: uma clareira acorda com a borda a ≤ 85 m e está registrada
  em todo pedaço que a borda + 17 m toca, logo bastariam 68 m. Teste compara com a varredura completa em ~2 000 posições.
  Ganho: de ~1 600 testes (hash de id + distância) a algumas dezenas por `StreamTick`.
- **Item 8 / DriverHub**: `Mechanics/MechanicsDriverHub.cs` + interface `IRegionMechanics` (`MechanicsAreas` = `*Bounds` +
  origem, `MechanicsBusy` = viagem do Farnkesh / Mar Gelado / Céu / Rio, corrida ou escalada dos Nartes). A 10 Hz liga só o
  driver com o herói a ≤ 32 m das suas áreas (ou ocupado); driver que se desligou no `Start` (região fora do layout) fica em
  paz. Cada driver ganhou no `OnDisable` a limpeza do seu ramo "fora da região" (esconder linhas, `LeaveRegion`/`Quiet`/
  `HideFeedback`), com checagem de nulo para o desmonte da cena. Ganho: 1 `Update` (2 numa borda) em vez de 14.
- **Item 12 (parte)**: chave do Ouro dos Nartes pré-montada; cantos do gelo fino sem `new[]`.

Pulado: item 1 (construir pedaço em thread/job) e os itens 6, 9, 10, 11, 13, 14 — precisam de medição ou estão fora dos
arquivos deste passo (`Enemies/**`, `MapMonsterSpawner`).

## Verificado e sem problema

- **`OverworldLayouts.TideFlats` e os demais arrays de mecânica** (`LimiarLamps`, `ZarafshanHarvestSpots`,
  `NartesSkyGolds`, `SkyCracks`, `*Bounds` ...) são `static readonly` (`Overworld/OverworldLayouts.CostaMechanics.cs:16`
  e afins): **nenhuma propriedade recria array por frame**. As únicas propriedades `=>` com `new` são as listas de
  textos (`*MechanicsTexts.Entries`), já preguiçosas com `??=`.
- **Raios de streaming** independem do tamanho do mundo: `LoadRadius = 96`, `UnloadRadius = 136`
  (`Overworld/OverworldRules.cs:25-28`) com pedaço de 48 m → ~21 pedaços pedidos e ≤ ~40 vivos, contra 48 750
  (150 × 325) no mundo. Cidades com 170/260 m (`OverworldView.cs:43-46`). Não precisam mudar com o crescimento.
- **`foreach` em `Dictionary`** no `UnloadFar`/`UpdateSpawners` usa o enumerador struct (sem alocação). Os `foreach` em
  `.Values` de Mistura/Profundo só rodam uma vez ao sair da região (guardados por `_inRegion`).
- **Relevo**: `OverworldRelief` já tem buckets espaciais para zonas, planos, clareiras, rios e trilhas
  (`Overworld/OverworldRelief.cs:860-900`); `ZoneAt`/`SettlementAt`/`PartAt` são lineares (~392 áreas, 16 partes), mas só
  rodam a 10 Hz no `TrackZone` (`OverworldView.cs:660`) e nos estados de proximidade, < 5 µs cada.
- **Bichos/clareiras ativos**: acordam a ≤ 85 m (`SpawnerActiveRadius`), dormem com histerese e prendem os pedaços sob
  si; o custo por spawner é um tick a `TickSeconds`. O gargalo é a varredura do item 4, não o número ativo.
- **Mingcheng, Mistura e Profundo** já fazem o certo fora da região (`LeaveRegion` só uma vez); e a Costa já
  atualiza o estado do herói a 1 Hz (`StateRefresh`).

## DriverHub: vale a pena (barato e tira três problemas de uma vez)

Ganho direto ~30–80 µs/frame (item 8), mais o fim do lixo do item 7, mais um ponto único para passar herói, layout e
estado de requisitos já em cache (item 3). Proposta mínima, ~60 linhas, sem mudar as regras puras:

1. Uma interface `IRegionMechanics { Rect WorldRect { get; } bool Busy { get; } }` em cada driver (`WorldRect` = os
   `*Bounds` + `_origin`; o `CostaDeltaTetoMechanicsDriver` devolve a união das três ou implementa três vezes).
   `Busy` = viagem em curso (`_ride`/`_riding` de Farnkesh, Rio, Céu e Mar Gelado), que pode cruzar a borda.
2. Um `MechanicsDriverHub` na mesma raiz, criado em `OverworldView.Build` depois dos drivers: a 10 Hz (o mesmo
   `ZoneTick` do `TrackZone`, ou assinando `OverworldView.RegionChanged`, `OverworldView.cs:106`) liga
   `driver.enabled = driver.Busy || Inflate(driver.WorldRect, 32 m).Contains(heroi)` e desliga os demais.
3. Cada driver troca o ramo "fora da região" do `Update` por `OnDisable` → `HideFeedback()/Quiet()/LeaveRegion()`,
   que passa a rodar **uma vez** na saída. Os `Update` desligados não custam nada ao Unity.

Resultado: só 1 driver (2 numa borda) roda por frame, em vez de 14. A margem de 32 m cobre o caso de o `*Bounds` do
driver não bater exatamente com a `OverworldPart` (as regras usam coordenadas locais da região).

## Ordem sugerida de trabalho

1. Itens 3, 7, 11, 12 (GC certo, correção de poucas linhas cada).
2. DriverHub (item 8), que já resolve o item 7 de graça.
3. Itens 2 e 5 (alturas em cache): ganho grande e certo em contagem, sem risco visual.
4. Item 4 (clareiras pelo índice por pedaço).
5. Medir no aparelho (Profiler + Frame Debugger) antes de mexer nos itens 1, 6 e 9, os que mais podem render e os
   que mais custam para mudar. Para o 1 e o 6, um teste EditMode com `Stopwatch`/`GC.GetTotalMemory` em volta de
   `OverworldLayouts.Get(WorldId)` e de 50 `BuildChunk` já dá a primeira ordem de grandeza sem o celular.
