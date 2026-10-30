# Rapadura — Colisão de props do ambiente

O herói é um `CharacterController` (raio 0,35, altura 2, step offset 0,3, layer Default)
movido só por `cc.Move`. Para ele não atravessar pedras, árvores, bancadas etc., todo
prop sólido precisa de um collider **não-trigger**. Um único sistema decide e aplica isso:

| Peça | Arquivo | Papel |
|---|---|---|
| Regras puras | `Assets/Scripts/Gameplay/Environment/SolidPropRules.cs` | tipo do prop pelo nome, forma do collider, medição do tronco, folga entre troncos, estradas |
| Aplicador | `Assets/Scripts/Gameplay/Environment/SolidPropBinder.cs` | cria/reajusta o collider na cena e em props gerados em runtime |
| Zona sem colisão | `Assets/Scripts/Gameplay/Environment/NonSolidPropZone.cs` | marca uma hierarquia que o aplicador ignora (mapa-múndi) |
| Menu do editor | `Assets/Scripts/Editor/SolidColliderMenu.cs` | assa os colliders nos prefabs e na cena aberta |
| Clique travado | `Assets/Scripts/Gameplay/Player/BlockedMoveDetector.cs` | o clique-para-andar desiste quando o herói empurra um obstáculo sem avançar |

Testes: `Assets/Tests/EditMode/SolidPropRulesTests.cs` (regras + aplicador em GameObjects) e
`Assets/Tests/PlayMode/SolidPropCollisionTests.cs` (física real: árvore, pedra e bancada
contra um CharacterController em corrida, no galope mais rápido e num frame de travada).

## Regras por tipo (`SolidPropRules.Classify` / `ShapeFor`)

| Tipo | Nomes (prefixo, case-sensitive) | Collider |
|---|---|---|
| Árvore | `Tree_*` (`Arvore*`) | **Cápsula só do tronco**: medida nos vértices da faixa baixa da malha (3 %–16 % da altura, acima das raízes e abaixo de qualquer copa), raio = percentil 85 + 4 cm, limitado a 0,12–0,75 m; altura = 55 % da árvore (2,2–3,5 m). A copa nunca bloqueia. Sem acesso aos vértices (build, malha não-legível): tronco fino no centro do bounds. |
| Pedra | `Rock*` (não `Rocks`), `ProceduralRock_*`, `Boulder*`, `Pedra*`, `ArenaProp_SnowyRock/Boulder` | **Casco convexo** da própria malha visual (≤ 4000 vértices e legível); senão caixa ajustada. Pedras com menos de 0,25 m de altura **e** 0,6 m de largura não bloqueiam (o herói passa por cima). |
| Caixa | `Workbench`, `Bench`/`Banco`/`Bancada`, `Stall`/`Barraca`, `Cart`/`Carroca`, `Fence`/`Cerca`, `House`/`Casa`, `Crate`/`Caixote`, `Barrel`/`Barril`, `Table`/`Mesa`, `Well`/`Poço`, `Anvil`/`Bigorna`, `Forge`/`Forja`, `Chest`/`Baú`, `Hut`/`Cabana`, `Shed`, `Wagon` | `BoxCollider` nos bounds da malha. |
| Cápsula | `Cactus`/`Cacto`/`Mandacaru`, `ArenaProp_Cactus/Stalagmite`, `Bush_Large`/`BigBush`/`Arbusto`, `Post_`/`Poste`, `Pillar`/`Pilar`, `Column`/`Coluna`, `Totem` | Cápsula vertical: raio = 80 % da metade da largura menor (braços de cacto e franja de arbusto não bloqueiam). |
| Nenhum | `Tuft_*`, malha `GrassTuft`, `Grass_*`, `Pebble_*`, `Twig_*`, `Flower_*`, `Mushroom_*`, `Leaf_*`, `Npc_*`, `Enemy_*`, `Player*`, `ArenaSecret_*` | nada (decoração de chão e personagens). |

Prop novo: dê um nome de uma das listas e ele fica sólido sozinho. Prop que precisa de
forma especial: coloque o collider à mão — colliders autorais **não-trigger** são
respeitados (a única exceção é collider de pedra claramente menor que o visual, < 65 % em
X/Z, que é trocado: era o caso do `MeshCollider` com a malha "Sphere" dos `Rock_*`).
Triggers (ex.: a zona de crafting da `Workbench`) nunca contam como sólido e nunca são mexidos.

## Como o collider é aplicado

* Fica num filho chamado **`SolidCollider`** (estático, sem Rigidbody, mesma layer do prop —
  **Default**; o projeto não tem uma layer "Environment" e criar uma exigiria editar o
  TagManager e todas as máscaras de raycast, então não foi feito). Re-aplicar nunca duplica:
  o filho existente é reaproveitado (e duplicados são removidos).
* Prop com uma única malha: o filho fica sob a transform da malha (encaixe exato, inclusive
  escala não-uniforme). Árvore ou prop de várias peças: sob a raiz do prop.
* Só primitivas (cápsula/caixa) exceto pedras (casco convexo, barato e estático). Nenhum
  `MeshCollider` não-convexo novo.

## Folga entre troncos e estradas

Depois de aplicar, cada par de árvores próximo é comparado com o diâmetro do herói
(`PassableGap` = 2 × 0,35 + 0,15 de folga = 0,85 m):

* vão ≥ 0,85 m → passa, nada muda;
* vão só um pouco menor → os dois troncos encolhem até 6 cm cada (nunca abaixo de 0,12 m) e o vão abre;
* vão claramente menor → um **`SolidGapSeal[A|B]`** (caixa fina entre os dois troncos) fecha o
  vão, para o clique-para-andar nunca enfiar o herói num "V" entre dois troncos e ficar tremendo.

Estradas: objetos com renderer chamados `Road*`/`Estrada*`/`Trilha*`/`Path_*`/`Caminho*`
(ou corredores registrados em `SolidPropBinder.KeepClear`) viram corredores; um tronco ao lado
nunca invade a estrada (o raio encolhe até o mínimo) e um selo nunca é criado atravessando uma.

## Onde roda

| Origem do prop | Como fica sólido |
|---|---|
| Cena carregada (`Main.unity`: `NaturalTrees`, `Rocks`, `Sandbox/Workbench`...) | `EnvironmentPresentationBootstrap` chama `SolidPropBinder.BindAll()` em todo load (independe do ciclo dia/noite). Depois do menu abaixo, já vem assado na cena e o load é quase no-op. |
| Prefabs de árvore (`Assets/Prefabs/Environment/Tree_*`) | `NaturalTreeSetup.BuildPrefabs` assa a cápsula do tronco no prefab; o menu também atualiza os existentes. |
| Pedras (`NaturalRockSetup`) | `ReplaceRock` remove o collider antigo (Sphere) e ajusta o casco; as espalhadas também passam pelo binder. |
| Arena de bioma (`BiomeArenaDressing`/`BiomeArenaPropFactory`) | a fábrica já põe primitivas ajustadas na peça principal (cacto/estalagmite/pedra) — o binder as respeita; `Bind(prop.Root)` é chamado após posicionar (só com `_propColliders`). Props de cena escondidos pelo perfil são desativados (o collider some junto). |
| Segredos (`ArenaSecretSpawner`), sala da torre (`TorreProps`) | já têm collider sólido próprio (`SphereCollider`/`BoxCollider`); nomes fora das listas, não são mexidos. |
| Casa (`HouseInteriorView`) | vista de arrumação com câmera própria; móveis já têm `BoxCollider` para seleção. Não é tocada. |
| Cadáveres de inimigos | `EnemyDeathVfx` desliga os colliders na morte — nunca bloqueiam. |
| Mapa-múndi (`WorldMapDressing`, `FestaJuninaDecor` e decoração de cidades no mapa) | **sem colisão, de propósito**: o token anda em linha reta entre nós (`MoveTowards`, sem CharacterController) e árvores do mapa não podem engolir o clique nos nós. A raiz do dressing recebe `NonSolidPropZone` e as árvores instanciadas têm os colliders desligados. |

## Movimento rápido (sem atravessar)

Auditoria dos caminhos de movimento do herói:

* `PlayerMotor` (andar, correr, slide, dash de buff +150 %, montaria via `SpeedSource`, galope
  até ×2,5 = 13,75 m/s): tudo passa por `CharacterController.Move`, que é uma varredura
  (sweep) da cápsula — não existe tunelamento contra collider estático, nem num frame longo
  (testado: 0,25 s a 13,75 m/s = 3,4 m num único `Move`).
* `KnockbackReceiver`: `cc.Move` quando há CharacterController (herói/inimigos); o fallback
  `transform.position +=` só vale para objetos sem CharacterController (nenhum herói).
* Montaria: `MountController.TickDash` e `MountWaveAbilities` usam `cc.Move`. O `MountActor`
  (o bicho correndo até o herói ao ser chamado) é só visual, sem collider.
* Teleportes (`PlayerMotor.Teleport`, `CheckpointManager`, `EncounterArenaController`, load de
  save) são saltos intencionais para pontos de spawn; não varrem o caminho. O
  `CharacterController` resolve sobreposição inicial (overlap recovery) no próximo `Move`.
* Clique-para-andar: linha reta, sem NavMesh. O raycast do clique atravessa copas (não têm
  collider) e acerta o chão atrás. Se o herói empurra um tronco/pedra/bancada de frente sem
  chegar mais perto por 0,6 s (contando só frames em que o motor quer andar), o destino é
  abandonado (`BlockedMoveDetector`) — parar como no MU, em vez de correr no lugar. Contornar
  deslizando continua normal (a distância diminui). Não age com o botão segurado (hold-to-walk)
  nem perseguindo inimigo.

## Menu para assar na cena

**`Rapadura/Environment/Apply Solid Colliders To Open Scene`**: atualiza os prefabs de
`Assets/Prefabs/Environment` e reajusta todos os props da(s) cena(s) aberta(s) (refit),
marcando a cena como modificada (salve depois de conferir o log). Há também
`Rapadura/Environment/Apply Solid Colliders To Environment Prefabs` (só os prefabs).
