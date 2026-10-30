# Cidades e zonas seguras

> Decisão do usuário (2026-10-29, TODO.md 12.3a): **toda região tem uma cidade, e dentro da cidade ninguém luta.**
> Bichos não atacam e não entram; jogadores não atacam bichos, outros jogadores nem a si mesmos.

## 1. As quatro cidades

| Região | Cidade | Moradores (conversa por primo) | Lojas | Serviços da Vó |
|---|---|---|---|---|
| Floresta | **Vila da Vó Zefa** | os 5 da história pessoal + Ponto de encontro (diálogos), Dona Cida, Seu Lázaro, Comadre Neném, Seu Tonico do Haras (`tonico_haras`, amigo de estrada do Vô Jorge; não é o Seu Zé Caçador), Seu Severino (ferreiro), Mestre Damião (marceneiro), Dona Fátima (costureira) | Loja Geral, Mercearia do Seu Lázaro, Armarinho da Dona Cida, Feira livre, Haras da Vila, Ferraria do Seu Severino, Marcenaria do Mestre Damião, Costureira Dona Fátima | Baú e Fogão |
| Deserto | **Povoado de Areia Branca** | Seu Vavá (seleiro), Dona Socorro (cisterna), Tião das Cabras | Selaria do Seu Vavá | — |
| Montanha | **Vila da Serra Alta** | Mestre Tonho (forja), Dona Graça (fiandeira), Seu Quinzinho (sanfona) | Forja do Mestre Tonho | — |
| Cavernas | **Vila das Lamparinas** | Benedita das Lamparinas, Seu Candinho (mineiro velho), Dona Fia (causos), Zezinho do Pavio | Lamparinas da Benedita | — |

O **Mascate Seu Bento** (o tropeiro do Deserto) passa cada dia numa cidade de região aberta; nesse dia ele é morador
e a loja dele entra na lista. Bancada e Carro de boi existem em todas as cidades.

Código: `CityCatalog`, `CityShopCatalog` (dados), `CityTexts` (textos en/pt-BR), `VillageHubButton` (o mesmo painel
para toda cidade), `CityFastTravelPanel` (carro de boi), `CitySafeZoneHud` (aviso), `WorldMapCityPresence` (zona
segura, visitas e o desenho da cidade no mapa), `CityProgress` (save `Cities`). Números em
`Docs/Balance/README.md`, seção 17.

### Por que o Baú e o Fogão ficam só na Vila

- São da Vó Zefa: o baú da casa dela e o fogão dela (as falas, os pratos favoritos e as lições são dela).
- O carro de boi deixa a volta barata (2 ouro por estrada), então a Vila continua sendo "a casa" para onde se volta,
  e o carro de boi ganha um motivo de existir.
- **Viagem rápida** (Docs/Seasons.md, seção 1): o carro só para em regiões cujo chefe já caiu (a Vila sempre),
  não sai durante eventos de estrada/encontros e a viagem é uma transição curta, sem encontros nem cartas de estrada.
- A Fase 15 traz o **Baú da casa** própria. No online, o Baú pode virar um depósito da conta com um NPC em cada cidade
  (como o cofre do MU), sem mudar a Vó.

## 2. Zona segura hoje (offline, mapa-múndi)

Regras em `SafeZoneRules` (C# puro, sem Unity: o servidor compila o mesmo arquivo).

- A cidade de cada região é o círculo de `SafeZoneRules.MapCityRadius` (1,5 m, a praça do nó) em volta do nó. No mapa
  ela aparece como um **anel tracejado dourado** no chão, aberto onde cada estrada sai, com uma **porteira** (dois
  mourões) em cada estrada, e um grupinho de casas da região ao lado do nó (taipa e juazeiro no Deserto, pedra e neve
  na Serra, casas com candeias nas Cavernas; na Floresta a vila já é o marco).
- Com o token dentro do anel: **nenhum encontro aleatório e nenhum evento de estrada** (`WorldMapController.SafeZoneProbe`:
  os passos nem contam), e o HUD mostra **"Você está em zona segura · <cidade>"**.
- "Explorar" e "Enfrentar o chefe" são saídas de propósito da cidade (a luta acontece na arena, fora dela).
- **Derrota:** o herói acorda na cidade da região onde perdeu (`SafeZoneRules.RespawnRegion`, aplicado por
  `CityProgress` no `EncounterEndedEvent`, antes do auto-save), com aviso no HUD.
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
| Armarinho da Dona Cida (`cida`) | semanal, 2 + 2 de casa | fibra, couro, arco; itens de casa "Loja: Armarinho" | arco após o Lobo Alfa; **Bornal de couro** na freguesia Freguês |
| Ferraria do Seu Severino (`severino`) | semanal, 2 + especial | ferro, martelo, Pedra da Bênção fixos; machado, picareta, espada de pau, minério, Pedra da Alma | Pedra da Alma após o Deserto; espada de ferro = especial da semana para Freguês de casa |
| Feira livre (Comadre Neném, `nenem`) | diária, 3 + 3 de casa | erva, madeira, fibra, pedra, carne, minério | minério com a Montanha aberta |
| Mercearia do Seu Lázaro (`lazaro`) | diária, 2 + 2 de casa | carne, poções, erva, madeira | mana com o Deserto aberto |
| Marcenaria do Mestre Damião (`damiao`) | diária, 1 + **10 móveis** + 1 Épico por semana | madeira, ferramentas; os móveis "Loja: Marcenaria" do `CatalogoCasa.md` | por raridade (abaixo) |
| Costureira Dona Fátima (`fatima`) | diária, **6 roupas + 6 de casa** | fibra, couro; as 38 peças "Loja: Costureira" do `CatalogoRoupas.md` (placeholders até o guarda-roupa existir; enquanto isso o couro preenche) | Incomum após o Lobo Alfa, Raro após o Guardião |
| Mascate Seu Bento (`bento`, itinerante) | diária, 3 + 2 de casa + especial | poções, curiosidades, Patuá, convites; Bornal de couro (180) | vários por chefe |

Também ganharam itens de casa a Forja do Mestre Tonho e as Lamparinas da Benedita (2 por período). Selaria e Haras
não têm itens de casa no catálogo.

**Itens de casa** (`CityShopCatalog.WithHouse`): todo item do `HouseCatalog` cuja origem é "Loja: <esta loja>", ao
preço do catálogo. Comum desde o início (estoque 5), Incomum após o Lobo Alfa (3), Raro após o Guardião (1), Épico após
o Senhor da Guerra e só como especial da semana (1); Lendário e sem preço nunca. A compra passa pela mochila por um
instante (um `ItemDefinition` de tempo de execução, `CityShopService.HouseVoucher`, 1×1) e vai para o **depósito da
casa** (`CityShopPurchases`, `HouseLayout.GrantToDepot`, nunca se perde). Com a mochila cheia não dá para comprar.

### 4.2 Freguesia (reputação por loja)

`ShopReputation` (C# puro): 1 ponto por ouro gasto naquela loja temática; salvo por loja em `CityProgress`
(`reputationShopIds`/`reputationPoints`). Faixas: **Freguês novo** 0, **Freguês** 50, **Freguês de casa** 150,
**Compadre** 400. Subir de faixa publica `CityShopPurchaseAppliedEvent` (`ReputationUp`); textos em `CityShopTexts`.
**[online]** os mesmos pontos por conta e loja, no servidor.

### 4.3 Bornal de couro

`item_bornal_couro` (Resources/Items, valor 150): comprar adiciona **+2 linhas** à mochila em grade para sempre
(`InventoryManager.AddBagRows`, salvo em `extraRows`) e o bornal sai da mochila, como o apito de montaria. Vendido
uma vez por save (`CityProgress.bornaisBought`): fixo no Armarinho da Dona Cida a 150 para quem é Freguês, ou no
rodízio do Mascate a 180 depois do Lobo Alfa. Não confundir com o acessório cosmético "Bornal de couro cru"
(`acess_018`, guarda-roupa).

**Avisos**: `CityShopPurchaseAppliedEvent` vira toast (`CityShopToastRelay`, mesma fila do `SecretToastController`):
mochila maior, item enviado ao depósito, nova faixa de freguesia.

**Conserto e refino**: o balcão do Seu Severino vende; consertar (`RepairService`) e refinar ficam na bancada
"Ferreiro: refinar" do hub (`VillageRefinePanel`), a mesma oficina.

### 4.4 Ainda não

- As roupas da Dona Fátima continuam fora do balcão até alguém ligar `CityShopService.OutfitResolver` ao guarda-roupa
  (`WardrobeCatalog` já tem os dados; falta o item de compra e o desbloqueio da peça).
- Olaria da Dona Rosa, Viveiro, Casa de Música e as filiais do Armazém Geral ainda não têm balcão (seus itens de casa
  já estão no catálogo).

