# Cidades e zonas seguras

> Decisão do usuário (2026-10-29, TODO.md 12.3a): **toda região tem uma cidade, e dentro da cidade ninguém luta.**
> Bichos não atacam e não entram; jogadores não atacam bichos, outros jogadores nem a si mesmos.

## 1. As quatro cidades

| Região | Cidade | Moradores (conversa por primo) | Lojas | Serviços da Vó |
|---|---|---|---|---|
| Bosque do Zarafshan | **Vila da Vó Edna** | os 5 da história pessoal + Ponto de encontro (diálogos), Mariam, Zaku, Nana, Srosh, o Tratador (`tonico_haras`, amigo de estrada do Vô Jorge; não é o Patig, o Rastreador), Mestre Adamas (ferreiro), Mestre Omóforo (marceneiro), Mestra Narisah (costureira) | Loja Geral, Mercearia do Zaku, Armarinho da Mariam, Feira livre, Estrebaria do Srosh, Ferraria do Mestre Adamas, Marcenaria do Mestre Omóforo, Costureira Mestra Narisah | Baú e Fogão |
| Areias Flamejantes | **Oásis de Qocho** | Vandak (seleiro), Yardena (cisterna), Uzzai, o Pastor | Selaria do Vandak | — |
| Serra de Kaggen | **Vila de Litema** | Mestre Mihr (forja), Coti (fiandeira), Innaios (tambor e lira) | Forja do Mestre Mihr | — |
| Grutas do Limiar | **Vila das Lamparinas** | Parthenos, a das Lamparinas, Mihrshah (mineiro velho), Aso (causos), Gabryab, o do Pavio | Lamparinas da Parthenos | — |

O **Caravaneiro Ammo** (o caravaneiro das Areias Flamejantes) passa cada dia numa cidade de região aberta; nesse dia ele é morador
e a loja dele entra na lista. Bancada e Barcas de Luz existem em todas as cidades.

Código: `CityCatalog`, `CityShopCatalog` (dados), `CityTexts` (textos en/pt-BR), `VillageHubButton` (o mesmo painel
para toda cidade), `CityFastTravelPanel` (Barcas de Luz), `CitySafeZoneHud` (aviso), `WorldMapCityPresence` (zona
segura, visitas e o desenho da cidade no mapa), `CityProgress` (save `Cities`). Números em
`Docs/Balance/README.md`, seção 17.

### Por que o Baú e o Fogão ficam só na Vila

- São da Vó Edna: o baú da casa dela e o fogão dela (as falas, os pratos favoritos e as lições são dela).
- As Barcas de Luz deixam a volta barata (2 ouro por estrada), então a Vila continua sendo "a casa" para onde se volta,
  e as Barcas de Luz ganham um motivo de existir.
- **Viagem rápida** (Docs/Seasons.md, seção 1): o carro só para em regiões cujo chefe já caiu (a Vila sempre),
  não sai durante eventos de estrada/encontros e a viagem é uma transição curta, sem encontros nem cartas de estrada.
- A Fase 15 traz o **Baú da casa** própria. No online, o Baú pode virar um depósito da conta com um NPC em cada cidade
  (como o cofre do MU), sem mudar a Vó.

## 2. Zona segura hoje (offline, mapa-múndi)

Regras em `SafeZoneRules` (C# puro, sem Unity: o servidor compila o mesmo arquivo).

- A cidade de cada região é o círculo de `SafeZoneRules.MapCityRadius` (1,5 m, a praça do nó) em volta do nó. No mapa
  ela aparece como um **anel tracejado dourado** no chão, aberto onde cada estrada sai, com uma **porteira** (dois
  mourões) em cada estrada, e um grupinho de casas da região ao lado do nó (taipa e tamareira nas Areias Flamejantes, pedra e neve
  na Serra, casas com candeias nas Grutas do Limiar; no Bosque do Zarafshan a vila já é o marco).
- Com o token dentro do anel: **nenhum encontro aleatório e nenhum evento de estrada** (`WorldMapController.SafeZoneProbe`:
  os passos nem contam), e o HUD mostra **"Você está em zona segura · <cidade>"**.
- "Explorar" e "Enfrentar o chefe" são saídas de propósito da cidade (a luta acontece na arena, fora dela).
- **Derrota (sempre na cidade, Docs/Progressao500.md §7):** **toda** derrota acorda o herói na praça da cidade da
  região onde perdeu (`SafeZoneRules.RespawnRegion`; a forma geral para os 100 mapas é
  `SafeZoneRules.RespawnCity(mapId, returnCityOf, fallback)`). Nunca no lugar onde caiu, nunca num checkpoint da arena;
  posto avançado (zona segura sem cidade) **não** é ponto de renascer (`SafeZoneRules.IsRespawnPoint`).
  - Derrota num encontro (arena, sala de evento): `CityProgress` aplica no `EncounterEndedEvent`, antes do auto-save;
    o token volta na cidade e o HUD avisa.
  - Derrota fora de encontro: a tela de morte (`DeathScreenController.RespawnInCity`) chama
    `CityProgress.ApplyDefeatHere` e carrega a cidade em **modo renascer** (`BeginCityVisit(cityId, respawn: true)`):
    o herói acorda na praça, com a fala do Ari (`CityWalkTexts.RespawnedAtPlaza`). Dentro da cidade andável, acorda na
    praça dela (`CityModeController.RespawnAtPlaza`). Reviver no checkpoint da cena fica só como rede de segurança de
    cena de teste (sem `CityProgress`/`SceneTransitionService`).
  - Caiu montado: a montaria fica cansada (vai para casa) e o herói acorda a pé.
  - Continua igual: queda de 9%, durabilidade −10% e perda de XP, que agora cai por faixa (5% até o 100, 4% 101–200,
    3% 201–300, 2% acima; nunca desce de nível; `DeathXpLossRules.LossPercentFor`).
- **Checkpoints:** os `SpawnPoint` são só **pontos de entrada** da cena (o inicial posiciona o herói; passar por outro
  faz auto-save) e não mudam mais o lugar de renascer. O único "checkpoint de morte" que sobra é a praça da cidade
  andável (`CheckpointManager.SetCityRespawn`).
- **Arena (Main.unity):** não tem cidade. Nada lá chama as regras de cidade, e as regras dizem "fora da cidade" para
  tudo, que é exatamente o combate de hoje. Nada mudou.

## 3. Quando as cidades virarem áreas 3D andáveis

1. Cada cidade ganha um volume (trigger) com o `CityId`; um componente lê em qual zona cada ator está
   (`ZoneKind.City` dentro, `ZoneKind.Wilds` fora). No online quem decide é o servidor, pela posição validada.
2. **Ataque:** antes de cada golpe, skill ou projétil, `SafeZoneRules.CanAttack(atacante, zonaDoAtacante, alvo,
   zonaDoAlvo, alvoÉElePróprio, pvpPermitido)`. Dentro da cidade é sempre "não": nem bicho, nem jogador, nem a si mesmo.
3. **Dano:** antes de aplicar qualquer dano (inclusive DoT e espinhos), `CanTakeDamage(ator, zona)`.
4. **Skills:** a hotbar desabilita skills de dano com `CanUseDamagingSkill(zona)`; andar, conversar, trocar, loja e
   casa continuam.
5. **IA dos bichos:** o passo seguinte do caminho só é aceito se `CanEnemyPursueInto(zonaDoPróximoPonto)`; se o alvo
   entrou na cidade, `ShouldDropTarget` faz o bicho largar o alvo e voltar ao ponto de origem. Bicho nunca nasce dentro.
6. **Entrar na cidade:** `ShouldClearDebuffsOnEntry(de, para)` limpa veneno/lentidão/atordoamento. Como o bicho larga o
   alvo na divisa e ninguém ataca de dentro, voltar para a cidade no meio da luta **perde o alvo** (não dá para se
   esconder e continuar batendo).
7. **PvP (futuro):** quem atacou outro jogador há menos de `PlayerCombatTagSeconds` (10 s) não entra na cidade
   (`CanEnterCity`, o "PZ lock" do Tibia). Bicho nunca trava a entrada.
8. **Renascer:** `RespawnRegion` continua valendo: o ponto de renascimento é a praça da cidade da região.

## 4. Lojas e rotação

- Balcão do dia = linhas fixas + sorteio pela data (`ShopRotation`, semente estável por loja e período, FNV-1a, nunca
  RNG do cliente: o servidor roda o mesmo arquivo e todo mundo vê o mesmo balcão). Diária ou semanal (segunda a
  domingo). Relógio atrasado não reabre um balcão antigo.
- Cada linha que gira cai num de três **sorteios** (`ShopPool`), cada um com sua semente e suas vagas:
  `Main` (o período da loja, `RotatingSlots`), `House` (itens de casa, `HouseSlots`) e `Weekly` (o **especial da
  semana**, `WeeklySlots`, sempre por semana, mesmo numa loja diária).
- **Travas** (`ShopUnlock`): abrir uma região, vencer o chefe dela, ou a **freguesia** da loja (`Reputation`, abaixo).
- **Disponibilidade** (não é trava, é "dá para vender agora?", `CityShopService.IsAvailable`): item de casa só com a
  casa própria; roupa só quando o guarda-roupa tiver dados (`CityShopService.OutfitResolver`); o Bornal de couro uma
  vez por save. Linha indisponível sai antes do sorteio e nunca ocupa vaga.
- O balcão vira um `ShopDefinition` em tempo de execução (`CityShopService`) e passa pelo mesmo `ShopManager` e pela
  mesma tela de loja. Quando o período vira, o estoque daquela loja volta ao cheio.

### 4.1 Lojas da Vila

| Loja (dono, `residentId`) | Período | Vende | Travas |
|---|---|---|---|
| Armarinho da Mariam (`cida`) | semanal, 2 + 2 de casa | fibra, couro, arco; itens de casa "Loja: Armarinho" | arco após o Kök-Böri; **Bornal de couro** na freguesia Freguês |
| Ferraria do Mestre Adamas (`severino`) | semanal, 2 + especial | ferro, martelo, Pedra-Faísca fixos; machado, picareta, espada de pau, minério, Pedra da Alma Viva | Pedra da Alma Viva após as Areias Flamejantes; espada de ferro = especial da semana para Amigo da Casa |
| Feira livre (Nana, `nenem`) | diária, 3 + 3 de casa | erva, madeira, fibra, pedra, carne, minério | minério com a Serra de Kaggen aberta |
| Mercearia do Zaku (`lazaro`) | diária, 2 + 2 de casa | carne, poções, erva, madeira | mana com as Areias Flamejantes abertas |
| Marcenaria do Mestre Omóforo (`damiao`) | diária, 1 + **10 móveis** + 1 Épico por semana | madeira, ferramentas; os móveis "Loja: Marcenaria" do `CatalogoCasa.md` | por raridade (abaixo) |
| Costureira Mestra Narisah (`fatima`) | diária, **6 roupas + 6 de casa** | fibra, couro; as 38 peças "Loja: Costureira" do `CatalogoRoupas.md` (placeholders até o guarda-roupa existir; enquanto isso o couro preenche) | Incomum após o Kök-Böri, Raro após o Guardião |
| Caravaneiro Ammo (`bento`, itinerante) | diária, 3 + 2 de casa + especial | poções, curiosidades, Tumar, convites; Bornal de couro (180) | vários por chefe |

Também ganharam itens de casa a Forja do Mestre Mihr e as Lamparinas da Parthenos (2 por período). Selaria e Haras
não têm itens de casa no catálogo.

**Itens de casa** (`CityShopCatalog.WithHouse`): todo item do `HouseCatalog` cuja origem é "Loja: <esta loja>", ao
preço do catálogo. Comum desde o início (estoque 5), Incomum após o Kök-Böri (3), Raro após o Guardião (1), Épico após
o Senhor da Guerra e só como especial da semana (1); Lendário e sem preço nunca. A compra passa pela mochila por um
instante (um `ItemDefinition` de tempo de execução, `CityShopService.HouseVoucher`, 1×1) e vai para o **depósito da
casa** (`CityShopPurchases`, `HouseLayout.GrantToDepot`, nunca se perde). Com a mochila cheia não dá para comprar.

### 4.2 Freguesia (reputação por loja)

`ShopReputation` (C# puro): 1 ponto por ouro gasto naquela loja temática; salvo por loja em `CityProgress`
(`reputationShopIds`/`reputationPoints`). Faixas: **Visitante** 0, **Freguês** 50, **Amigo da Casa** 150,
**Companheiro de Caravana** 400. Subir de faixa publica `CityShopPurchaseAppliedEvent` (`ReputationUp`); textos em `CityShopTexts`.
**[online]** os mesmos pontos por conta e loja, no servidor.

### 4.3 Bornal de couro

`item_bornal_couro` (Resources/Items, valor 150): comprar adiciona **+2 linhas** à mochila em grade para sempre
(`InventoryManager.AddBagRows`, salvo em `extraRows`) e o bornal sai da mochila, como o apito de montaria. Vendido
uma vez por save (`CityProgress.bornaisBought`): fixo no Armarinho da Mariam a 150 para quem é Freguês, ou no
rodízio do Caravaneiro a 180 depois do Kök-Böri. Não confundir com o acessório cosmético "Bornal de couro cru"
(`acess_018`, guarda-roupa).

**Avisos**: `CityShopPurchaseAppliedEvent` vira toast (`CityShopToastRelay`, mesma fila do `SecretToastController`):
mochila maior, item enviado ao depósito, nova faixa de freguesia.

**Conserto e refino**: o balcão do Mestre Adamas vende; consertar (`RepairService`) e refinar ficam na bancada
"Ferreiro: refinar" do hub (`VillageRefinePanel`), a mesma oficina.

### 4.4 Ainda não

- As roupas da Mestra Narisah continuam fora do balcão até alguém ligar `CityShopService.OutfitResolver` ao guarda-roupa
  (`WardrobeCatalog` já tem os dados; falta o item de compra e o desbloqueio da peça).
- Olaria da Náfsha, Viveiro, Casa de Música e as filiais do Armazém Geral ainda não têm balcão (seus itens de casa
  já estão no catálogo).

## 5. Cidades andáveis (modo cidade)

> Feito em 2026-10-30, já no re-tema da `Docs/WorldBible_Maniqueismo.md` (§2): a Vila da Vó Edna é **sogdiana** (adobe cor
> de damasco, teto plano com parapeito, aivan fundo em colunas entalhadas, cúpula turquesa), não da Rota da Seda. Ids não mudam.

### 5.1 Como se entra e se sai

- No painel da cidade (mapa, `VillageHubButton`) há **"Andar pela cidade"** (`CityWalkTexts.WalkIn`). Ele chama
  `SceneTransitionService.BeginCityVisit(cityId)`, que carrega **Main.unity em modo cidade** (sem encontro, sem inimigos).
- `CityModeController` (auto-bootstrap no `sceneLoaded`, sem editar cena) esconde a arena (`HiddenSceneRoots`: `Sandbox`,
  `Ground`, `GroundDetail`, `NaturalTrees`, `Rocks`, `Rock1`, `Rock2`), monta a cidade (`CityTownView`), põe o herói na
  estrada que vem do mapa (`CityLayout.Arrival`), marca a **praça** como ponto de renascer (`CheckpointManager.SetCityRespawn`; uma visita de renascer já põe o herói na praça), troca
  clima e ambiente para a região e deixa o dia/noite correndo.
- **Saídas:** o portal de adobe no fim de cada rua de saída volta ao mapa (`EndCityVisit`); o **Cais da Barca**
  (`CityLotKind.ShipDock`, §3.16 da bíblia) volta ao mapa **com a janela das Barcas de Luz aberta**
  (`OpenFastTravelOnMapReturn`, consumido pelo hub). O token continua no nó da cidade; auto-save na saída.

### 5.2 Kit modular (código, `Gameplay/World/Cities/Town`)

| Peça | Arquivo | O quê |
|---|---|---|
| Paleta | `TownPalette` | 64 cores (8×8) num só material URP Lit (instancing, SRP batcher); a emissão de janelas, lanternas e brasas acende à noite (`SetNightGlow`). |
| Construtor | `TownMeshBuilder` | caixas, troncos, cúpulas, placas de telhado com enrolamento automático (`QuadFacing`/`Plate`). Puro até `ToMesh`. |
| Arquitetura | `TownKit` | plinto, faixa de reboco, enxaimel; **porta** com arco ogival e folhas de nogueira entalhadas com rosetas; **janela** com treliça (*panjara*) que acende à noite; janela redonda; **nicho de lâmpada**; **teto plano** com parapeito, cimalha de cal, vigas aparentes e bicas; **cúpula** em tambor; empena de telha (estrebaria); quarto alto no terraço; **aivan** (piso de tijolo, **colunas sogdianas** com capitel em leque, viga, forro de caibros); **pishtaq**; torre da lâmpada; chaminé; ninho de cegonha; letreiro; *suzani*. |
| Props | `TownKit.Props` | poço com cântaros, *supa/tapchan* (banco-estrado), barracas com toldo listrado, lampião de bronze, **Relógio de Zurvan**, **tandir**, **latada de uva**, muro de quintal (adobe; **pedra seca** com espinha de peixe em Litema e Lamparinas), portal da cidade, fardo, caixotes, canteiro, **estela** (Litema), as figuras dos donos das lojas. |
| Vegetação | `TownKit.Tree` | Zarafshan: choupo verde e dourado, tamargueira rosa, pomar (damasco, romã). Qocho: tamareira, tamargueira. Litema: cedro retorcido, babosa-de-árvore. Lamparinas: cogumelo azul que brilha, estalagmite com musgo. |
| Chão | `TownGroundBuilder` | campo em duas cores, ruas de terra com meio-fio de pedra e sulcos, calçadas de lajota, **aryk** (canal) com pontezinhas nas portas, praça em anéis com estrela *girih* no centro. |
| Prédios | `TownBuildingBuilder` | casa, loja, oficina, estrebaria, casa da Vó, casa da lâmpada e cais a partir do `TownBuildingSpec`; **rondavel** (paredes redondas, teto cônico de palha) e **casa no tufo** prontas para Litema e Lamparinas. |
| Estilos | `TownStyles` | material e forma por região: `Sogdian` (teto plano), `Oasis` (taipa rosa), `Rondavel` (cone), `CaveTuff` (rocha). |

Variação sem repetição óbvia: cada lote escolhe parede, remate e madeira por hash FNV-1a do id (nunca RNG); janelas
acesas 2 em 3; quarto alto e ninho por hash. Prédios iguais compartilham a malha (`TownMeshCache`).

### 5.3 Layout (dados): `CityTownRecipe` → `CityLayoutGenerator` → `CityLayout`

- Receita = praça (raio), ruas (braços N/L/S/O com comprimento e saída), lotes (rua, lado, distância; ou livres com centro
  e frente), props da praça. O gerador calcula pegada, porta, **ponto de chegada à porta** (na calçada) e **lugar do dono**
  (no aivan, ao lado da porta, nunca na frente dela), raio de interação, cais, saídas, chegada e renascer; põe sozinho
  lampiões entre os lotes, quintais com muro e árvores, canteiros, portais e a mata de fundo, e descarta o que bater.
- **Vila da Vó Edna** (`CityTownRecipes.VilaEdna`): praça com a **casa da lâmpada** (pishtaq, cúpula, torre) na cabeça e o
  Relógio de Zurvan na frente, poço, *supas* e a feira da Nana; rua **Oeste** (Zaku/Mercearia, Mariam/Armarinho, Cais da
  Barca), rua **Leste** (Loja Geral em dois pisos, **casa da Vó Edna** com aivan comprido, tandir, latada e cegonha; a
  estrebaria do Srosh fechando a rua, com piquete), rua **Sul** (a estrada do mapa: ferraria do Adamas, marcenaria do
  Omóforo, costura da Narisah). As frentes de loja olham para a câmera isométrica (lado norte das ruas O/L, lado leste da S).
- **Outras cidades:** `CityTownRecipes.Default(city)` já monta uma cidade andável com os moradores e lojas de cada uma, no
  estilo da região. Para a versão final, escrever uma receita como `VilaEdna()` (e, se preciso, módulos novos no
  `TownKit`); nenhum outro código muda. O **Ammo** (caravaneiro) ganha uma barraca na praça no dia dele.

### 5.4 Regras (testadas: `CityLayoutRules`, `Tests/EditMode/CityTownTests.cs`)

- Nenhum prédio sobre rua, calçada ou praça; nenhum lote sobre outro.
- Toda porta, saída e o ponto de chegada alcançáveis da praça por corredores de **≥ 1,2 m** (grade de 0,2 m, folga de
  0,6 m de cada collider; o herói é uma cápsula de 0,35 m).
- Dono de loja a ≥ 0,35 m de qualquer collider, com o ponto de chegada à porta dentro do raio de interação dele.
- O volume da zona segura cobre a cidade inteira (lotes, donos, saídas, chegada, renascer).
- Orçamentos: < 6.000 triângulos por prédio, < 160.000 a Vila inteira (com a mata de fundo), < 60 renderers, nenhum
  `MeshCollider`. PlayMode (`Tests/PlayMode/CityWalkSmokeTests.cs`): do mapa entra na Vila, anda com o clique-para-andar
  real da praça até a porta de cada loja (dentro do alcance do dono) e sai pela porta sul de volta ao mapa.

### 5.5 Desempenho (mobile) e colisão

- Um material para tudo; uma malha por prédio (culling individual), props fundidos em 4 quadrantes + 1 fundo sem
  sombra, chão numa malha só: ~30 renderers por cidade. Malhas em cache na sessão; nada vira asset (o build não cresce).
- Colliders num só objeto, saídos de `CitySolids` (a mesma lista que os testes usam): caixas nas paredes (até 2,6 m,
  para a câmera isométrica não "puxar" em cada beiral), barracas e muros; cápsulas nas colunas, postes e troncos; pisos
  de aivan como degraus de 0,16 m; um chão; quatro paredes invisíveis. A raiz recebe `NonSolidPropZone` (o
  `SolidPropBinder` de `Docs/Collision.md` não mexe nela).
- Noite: só 6 luzes pontuais sem sombra (lanternas da praça, da casa da lâmpada e do cais); o resto é emissão da paleta.

### 5.6 Zona segura ligada (§3)

- `CitySafeZoneVolume` (gatilho com o `CityId`) registra a caixa em `CitySafeZones`; `CitySafeZones.ZoneAt(pos)` dá o
  `ZoneKind`. **Dano:** `Health` recusa qualquer dano dentro (`SafeZoneRules.CanTakeDamage`), sem custo fora de cidade.
  **Entrada:** limpa veneno, lentidão e atordoamento (`ShouldClearDebuffsOnEntry` → `BuffController.RemoveDebuffs/ClearStun`)
  e publica `CityZoneChangedEvent`.
- Ainda não ligados (não há bichos na cidade hoje): `CanAttack` antes do golpe, a hotbar com `CanUseDamagingSkill` e a IA
  com `CanEnemyPursueInto`/`ShouldDropTarget`. Quando uma cidade tiver borda com o mato, ligar lendo `CitySafeZones.ZoneAt`.

### 5.7 Ver no editor

`Rapadura ▸ Cities ▸ Preview Walkable Town ▸ <cidade>` monta a cidade na cena aberta como objeto que não é salvo (use uma
cena vazia), com a câmera da Scene no ângulo do jogo; `Toggle Night` liga a noite; `Clear Preview` limpa.
`Rapadura ▸ Cities ▸ Validate Town Layouts` roda as regras e escreve no Console.

