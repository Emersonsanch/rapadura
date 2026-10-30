# Temporadas e Viagem rápida

Dois sistemas pequenos que nasceram juntos: o **Sistema de temporadas** (eventos pelo calendário, começando pela
Festa Junina) e a **Viagem rápida** do carro de boi. Tudo é regra pura em C# (testável no EditMode e rodável no
servidor do online, ver `project_multiplayer_mu_online`), com a UI construída em código (UI Toolkit), sem edição de cena.

Testes: `Assets/Tests/EditMode/SeasonsTests.cs`.

## 1. Viagem rápida (carro de boi)

Não é um sistema novo: é o **carro de boi** que já existia (`CityFastTravelPanel`, `FastTravelRules`, Docs/Cities.md,
2 ouro por estrada) com as regras novas:

| Regra | Onde |
|---|---|
| Só vai para cidades **visitadas** e regiões desbloqueadas (já existia) | `FastTravelRules.Check` |
| Só para numa região cujo **chefe já foi vencido**; a Vila (Floresta, `HomeRegion`) é a casa e está sempre aberta | `FastTravelRules.IsStopOpen`, `RegionBossProgress.IsRegionCleared` |
| **Bloqueado durante eventos**: carta de evento de estrada aberta, encontro pendente ou o token andando | `FastTravelRules.IsEventActive`, `CityFastTravelPanel.IsEventActive` |
| Custo pequeno: `GoldPerRoad` (2) × estradas do caminho mais curto (BFS no grafo) | `FastTravelRules.Cost` |
| Transição curta e só cosmética: tela escurece (0,35 s), "O carro de boi vai rangendo pela estrada até …" (0,6 s), clareia (0,35 s) | `FastTravelTransition` |

Ordem das mensagens ao jogador: mesma cidade → evento acontecendo → fora da cidade → não visitada → bloqueada →
chefe não vencido → sem rota → sem ouro.

**Por que não quebra o mapa.** O token não percorre a estrada: no meio da transição (tela coberta) a região atual é
trocada (`MapProgress.SetCurrentRegion`) e `WorldMapController.SyncWithProgress` só reposiciona o token. Não há
sorteio de encontro, carta de evento de estrada (`WorldEventCardController`) nem falas de estrada. A transição segura
`GameplayPause` durante o fade e roda em tempo não escalado; com "Reduzir Movimento" (ou sem HUD) a chegada é
instantânea, então os testes de PlayMode (`WorldMapFlowTestBase`) não esperam animação. O bug antigo do congelamento
(evento de estrada cancelando a viagem, `GameLoadedEvent.fromCarriedState`) não é tocado: o carro não sai enquanto há
evento.

## 2. Sistema de temporadas

`SeasonCalendar` decide pelo **dia do aparelho** (sem servidor, sem sorteio): junho inteiro, todo ano, é Festa Junina.
`SeasonCalendar.Clock` é trocado por um relógio fixo nos testes.

**Debug:** menu `Rapadura/Debug/Festa Junina: forçar (fora de junho)` (`SeasonDebugMenu`, guardado em EditorPrefs e
reaplicado a cada domain reload) e `Rapadura/Debug/Festa Junina: +50 prendas (play mode)`.

No online, o calendário vira hora do servidor; a regra continua a mesma.

## 3. Festa Junina — "Arraiá da Vó Edna"

### Decoração (Vila)

`FestaJuninaDecor` se pendura sozinho no `WorldMapDressing` a cada cena carregada: 8 postes em volta do limite da Vila
com cordões de **bandeirinhas** (cores da paleta do mapa, com caimento) e uma **fogueira** com luz piscando no lado mais
aberto da praça. Os postes desviam dos portões das estradas (`PoleAngles`), a fogueira fica no maior vão (`FireAngle`).
Aparece enquanto a festa está ativa e some quando acaba (ou quando o debug é desligado).

### Entrada

`FestaJuninaButton` (auto-bootstrap como o `FishingSpotButton`) põe o botão **Arraiá** na barra de ações do HUD do mapa,
só durante a festa, com o token parado dentro da Vila, sem diálogo ou pausa. Abre `FestaJuninaPanel`, que entra nas
telas do HUD (Esc fecha). Estilo: `Resources/UI/FestaJunina.uss` (tema registrado em `ThemeAdoptionTests`).

### Quadrilha (minijogo de ritmo)

`QuadrilhaSession`: o marcador chama 16 passos, um a cada 0,9 s depois de 2 s de entrada.

| Chamada | Tecla | Controle |
|---|---|---|
| "Anarriê!" | ← / A | D-pad ← |
| "Balancê!" | ↑ / W | D-pad ↑ |
| "Olha a cobra!" | ↓ / S | D-pad ↓ |
| "Caminho da roça!" | → / D | D-pad → |

Também há os quatro botões na tela. Janela: perfeito ≤ 0,12 s (2 pontos), bom ≤ 0,28 s (1 ponto); passo errado ou
deixado passar = erro. A sequência vem da semente do dia (a dança do dia é igual em todo replay) e nunca repete o mesmo
passo três vezes. **Dança tranquila** (ou Reduzir Movimento): batida de 1,15 s e janelas 1,6× maiores.

### Prendas (moeda do evento)

`FestaJuninaProgress` (save `FestaJunina`, registrado no `GameManager`; as prendas ficam de um junho para o outro).

| Fonte | Prendas |
|---|---|
| Quadrilha | até 10 pela pontuação (proporcional), +2 sem erro, +5 na primeira dança do dia |
| Pescaria do arraiá | 1 por peixe pescado durante a festa, até 10 por dia (`FishCaughtEvent`) |

Fora da festa nada paga.

### Barraca de Prendas

Vende os **18 itens sazonais da Festa Junina** de `Docs/Housing/CatalogoCasa.md` (`casa_sazonal_*`,
`HouseSeason.Junina`, fonte `event_festa_junina`, preço = `EventPrice` em prendas, do mais barato ao mais caro).
O prêmio vai para o **Depósito** da casa (`HouseLayout.GrantToDepot`): nunca se perde, com ou sem casa.

O Lendário **"Bandeira do primeiro São João do sítio"** (`casa_familia_025`) não é vendido: vem das **tarefas do
arraiá** — 5 danças, 1 dança sem erro e 6 prêmios diferentes.

### Falas dos primos

`FestaJuninaTexts`: cada primo (Maithe, Maria, Joaquim, Ícaro, Lavine) tem fala de chegada, de volta, de dança ótima /
ok / ruim e da barraca, lembrando a Vó Edna puxando a quadrilha e o Vô Jorge acendendo a fogueira. Tudo em en + pt-BR
(registrado em `LocalizationCoverageTests.Sources()`).

## Próximas temporadas (ideias)

Natal (presépio na Vila, lapinha), Carnaval (bloco no mapa), Dia das Crianças. Cada uma é um valor novo de
`SeasonalEventId`, uma janela de datas em `SeasonCalendar` e os itens sazonais já previstos no CatalogoCasa.md.
