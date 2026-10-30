# Temporadas e Viagem rápida

Dois sistemas pequenos que nasceram juntos: o **Sistema de temporadas** (eventos pelo calendário, começando pelo
Bema, a antiga Festa Junina) e a **Viagem rápida** do carro de boi. Tudo é regra pura em C# (testável no EditMode e rodável no
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

`SeasonCalendar` decide pelo **dia do aparelho** (sem servidor, sem sorteio): junho inteiro, todo ano, é o **Bema**
(id `SeasonalEventId.FestaJunina`; junho fica porque é perto do solstício, "o mês de mais luz do ano").
`SeasonCalendar.Clock` é trocado por um relógio fixo nos testes.

**Debug:** menu `Rapadura/Debug/Bema: forçar (fora de junho)` (`SeasonDebugMenu`, guardado em EditorPrefs e
reaplicado a cada domain reload) e `Rapadura/Debug/Bema: +50 oferendas (play mode)`.

No online, o calendário vira hora do servidor; a regra continua a mesma.

## 3. O Bema — "O Bema da Vó Edna"

O Bema substitui a antiga Festa Junina (fonte: `Docs/WorldBible_Maniqueismo.md` §3.15). **Regra de ferro:** ids, chaves
de localização (`ui.festa.*`, `festa.line.*`), nomes de classe e arquivo (`FestaJunina*`, `QuadrilhaSession`,
`QuadrilhaMove.Anarrie`...), enums, a chave de save `FestaJunina` e o campo `Prendas` **não mudam**. Só mudam os textos
que o jogador vê (en + pt-BR), as cores e a decoração.

**O que é, em linguagem de criança:** uma vez por ano a vila monta no meio da praça uma **cadeira grande e vazia no alto
de cinco degraus**, coberta de panos, flores e lâmpadas. "É para lembrar de quem ensinou os hinos antigos", diz a Vó
Edna, "e de todo mundo que já não está aqui para cantar com a gente." Ninguém senta nela. Todo mundo canta em volta e
traz **oferendas de luz**. A cadeira de balanço do Vô Jorge vai para a praça nesse dia. O jogo **não nomeia** a pessoa
homenageada (en: *the Bema, the festival of the empty seat*).

### Decoração (Vila)

`FestaJuninaDecor` se pendura sozinho no `WorldMapDressing` a cada cena carregada, tudo em malha de código com a paleta
do mapa:

- **8 postes** em volta do limite da Vila, cada um com **cinco fitas** nas cores das cinco luzes, e entre eles cordões
  com **7 lanternas de papel** cada (com caimento), alternando as cores. A paleta do mapa não tem lilás, então o Ar
  Sereno usa o prateado (`Fog`); as outras: Vento `CloudShade`, Luz `Gold`, Água `Crystal`, Fogo `Lantern`.
  (`AppendBandeirinhas`: o nome do método ficou.)
- **O Bema** no lado mais aberto da praça (o antigo lugar da fogueira, mesmo `FireAngle` e `FireRadius`): estrado de
  **cinco degraus** de adobe com uma passadeira bordada, a **cadeira vazia** no alto com um pano branco, flores nos
  cantos de trás, **lamparinas** nos cantos da frente de cada degrau (as oferendas de luz) e, ao lado, a **cadeira de
  balanço do Vô Jorge** com uma lâmpada alta (a "primeira lâmpada do Bema"). A frente fica virada para a câmera
  (`BemaYaw`). As chamas piscam de leve e uma luz quente fica sobre a cadeira. (`AppendFireLogs` = o estrado,
  `AppendFlame` = as chamas das lamparinas, `LampPositions` = onde ficam.)

Os postes desviam dos portões das estradas (`PoleAngles`), o Bema fica no maior vão (`FireAngle`). Aparece enquanto o
Bema está ativo e some quando acaba (ou quando o debug é desligado).

### Entrada

`FestaJuninaButton` (auto-bootstrap como o `FishingSpotButton`) põe o botão **Bema** na barra de ações do HUD do mapa,
só durante o Bema, com o token parado dentro da Vila, sem diálogo ou pausa. Abre `FestaJuninaPanel`, que entra nas
telas do HUD (Esc fecha). Estilo: `Resources/UI/FestaJunina.uss` (tema registrado em `ThemeAdoptionTests`).

### Hinos do Bema (minijogo de ritmo: Chamado e Resposta)

`QuadrilhaSession`: o **Mestre Xroshtag** chama 16 vezes, uma a cada 0,9 s depois de 2 s de entrada, e o jogador
**responde**. O tambor falante e a lira acompanham.

| Chamada | Tecla | Controle | Cor da nota |
|---|---|---|---|
| "Luz!" | ← / A | D-pad ← | dourado |
| "Vento!" | ↑ / W | D-pad ↑ | branco-azulado |
| "Água!" | ↓ / S | D-pad ↓ | verde-água |
| "Fogo!" | → / D | D-pad → | laranja-cobre |

O quinto elemento, o **Ar Sereno**, é o "passo perfeito": a resposta perfeita brilha lilás (só visual, sem botão novo).
Também há os quatro botões na tela. Janela: perfeito ≤ 0,12 s (2 pontos), bom ≤ 0,28 s (1 ponto); resposta errada ou
deixada passar = erro. A sequência vem da semente do dia (o hino do dia é igual em todo replay) e nunca repete a mesma
chamada três vezes. **Hino tranquilo** (ou Reduzir Movimento): batida de 1,15 s e janelas 1,6× maiores.

### Oferendas (moeda do evento)

`FestaJuninaProgress` (save `FestaJunina`, campo `Prendas`, registrado no `GameManager`; as oferendas ficam de um junho
para o outro). Ícone: lamparininha.

| Fonte | Oferendas |
|---|---|
| Hinos do Bema | até 10 pela pontuação (proporcional), +2 sem erro, +5 no primeiro hino do dia |
| Pescaria do Bema | 1 por peixe pescado durante o Bema, até 10 por dia (`FishCaughtEvent`) |

Fora do Bema nada paga.

### Tenda das Oferendas

Vende os **18 itens sazonais do evento** de `Docs/Housing/CatalogoCasa.md` (`casa_sazonal_*`,
`HouseSeason.Junina`, fonte `event_festa_junina`, preço = `EventPrice` em oferendas, do mais barato ao mais caro).
O prêmio vai para o **Depósito** da casa (`HouseLayout.GrantToDepot`): nunca se perde, com ou sem casa. Os nomes dos
itens vêm do catálogo da casa (onda 2A).

O Lendário **"Estandarte do primeiro Bema da Vila"** (`casa_familia_025`) não é vendido: vem das **tarefas do Bema**:
5 hinos, 1 hino sem erro e 6 enfeites diferentes.

### Falas dos primos

`FestaJuninaTexts`: cada primo (Maithe, Maria, Joaquim, Ícaro, Lavine) tem fala de chegada, de volta, de hino ótimo /
ok / ruim e da tenda, lembrando a **Vó Edna puxando o primeiro hino** e o **Vô Jorge que acendia a primeira lâmpada do
Bema**, e a cadeira de balanço dele na praça ("ninguém senta, a gente só canta pertinho"). Tudo em en + pt-BR
(registrado em `LocalizationCoverageTests.Sources()`).

Instrumentos (sabor): tambor falante (*dùndún*), lira de tigela da Núbia (*kissar*), dutar uigur, flauta ney.

## Próximas temporadas (ideias)

Outras festas da Ilha da Aurora, cada uma um valor novo de `SeasonalEventId`, uma janela de datas em `SeasonCalendar` e
os itens sazonais já previstos no CatalogoCasa.md.
