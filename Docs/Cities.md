# Cidades e zonas seguras

> Decisão do usuário (2026-10-29, TODO.md 12.3a): **toda região tem uma cidade, e dentro da cidade ninguém luta.**
> Bichos não atacam e não entram; jogadores não atacam bichos, outros jogadores nem a si mesmos.

## 1. As quatro cidades

| Região | Cidade | Moradores (conversa por primo) | Lojas | Serviços da Vó |
|---|---|---|---|---|
| Floresta | **Vila da Vó Zefa** | os 5 da história pessoal + Ponto de encontro (diálogos), Dona Cida, Seu Lázaro, Comadre Neném | Loja Geral, Mercearia do Seu Lázaro, Armarinho da Dona Cida, Feira livre | Baú e Fogão |
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

- Balcão do dia = linhas fixas + sorteio pela data (`ShopRotation`, semente estável por loja e período). Diária ou
  semanal (segunda a domingo). Relógio atrasado não reabre um balcão antigo.
- Algumas linhas só entram depois de abrir uma região ou vencer o chefe dela (`ShopUnlock`).
- O balcão vira um `ShopDefinition` em tempo de execução (`CityShopService`) e passa pelo mesmo `ShopManager` e pela
  mesma tela de loja. Quando o período vira, o estoque daquela loja volta ao cheio.
- Nomes de loja e de dono seguem `Docs/Housing/CatalogoCasa.md` (Armarinho da Dona Cida, Mercearia do Seu Lázaro,
  Feira livre, Forja do Mestre Tonho, Lamparinas da Benedita, Mascate Seu Bento). As outras lojas do catálogo da casa
  (Marcenaria, Olaria, Costureira, Viveiro, Casa de Música, Armazém Geral) entram quando houver itens de casa.
