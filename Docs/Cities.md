# Cidades e zonas seguras

> Decisão do usuário (2026-10-29, TODO.md 12.3a): **toda região tem uma cidade, e dentro da cidade ninguém luta.**
> Bichos não atacam e não entram; jogadores não atacam bichos, outros jogadores nem a si mesmos.

## 1. As quatro cidades

| Região | Cidade | Moradores (conversa por primo) | Lojas | Serviços da Vó |
|---|---|---|---|---|
| Bosque do Zarafshan | **Vila da Vó Edna** | os 5 da história pessoal + Ponto de encontro (diálogos), Vó Edna (no aivan da casa dela), Mariam, Zaku, Nana, Srosh, o Tratador (`tonico_haras`, amigo de estrada do Vô Jorge; não é o Patig, o Rastreador), Mestre Adamas (ferreiro), Mestre Omóforo (marceneiro), Mestra Narisah (costureira), Ramrat, a Guardiã da Lâmpada (`socorro_vila`, guarda o oratório da Bênção), Mestre Xroshtag e Padwaxtag (músicos do Bema), Patig, o Rastreador | Loja Geral, Mercearia do Zaku, Armarinho da Mariam, Feira livre, Estrebaria do Srosh, Ferraria do Mestre Adamas, Marcenaria do Mestre Omóforo, Costureira Mestra Narisah | Baú e Fogão |
| Areias Flamejantes | **Oásis de Qocho** | Vandak (seleiro), Yardena (cisterna), Uzzai, o Pastor, Náfsha, a Oleira (`rosa`, na olaria da rua Oeste) | Selaria do Vandak | — |
| Serra de Kaggen | **Vila de Litema** | Mestre Mihr (forja), Coti (fiandeira), Innaios (tambor e lira) | Forja do Mestre Mihr | — |
| Grutas do Limiar | **Vila das Lamparinas** | Parthenos, a das Lamparinas, Mihrshah (mineiro velho), Aso (causos), Gabryab, o do Pavio | Lamparinas da Parthenos | — |

O **Caravaneiro Ammo** (o caravaneiro das Areias Flamejantes) passa cada dia numa cidade de região aberta; nesse dia ele é morador
e a loja dele entra na lista. Bancada e Barcas de Luz existem em todas as cidades.

### 1.1 Cidades de raça (`CityCatalog.RaceCities`)

Uma segunda cidade dentro de uma região (Docs/Mapas100.md §3.3) fica **fora** de `CityCatalog.Cities` (a lista de
"uma cidade-hub por região": painel do mapa-múndi, respawn por região, lista das Barcas). `CityCatalog.Get`,
`AllCities` e `AllResidents` enxergam as duas listas; `IsRaceCity` diz qual é qual.

| Região | Cidade | Moradores | Loja | Missões |
|---|---|---|---|---|
| Bosque do Zarafshan (mapa 05) | **Copas de Garajonay** (elfos-da-mata, `copas_garajonay`) | Gara, a Guardiã das Copas (salão da praça), Jonay, o Arqueiro, Dácil, a Ervateira, Bencomo, o Velho Chefe, Tinguaro, o Rastreador | **Ervaria da Dácil** (`shop_ervaria_dacil`) | Gara: "A Névoa Que Esqueceu a Água"; Tinguaro: "Rastros no Musgo" |
| Areias Flamejantes (mapas 11 e 12) | **Tambor Rubro** (orcs, `tambor_rubro`) | Gurparanzah, a Chefe-Arqueira (Salão Grande na cabeça da praça), Ashmunikal, a Matriarca dos Tambores (rua Leste, torre do tambor atrás), Kessi, o Caçador (rua Leste), Appu, o Mercador Gentil (rua Sul), Tushratta, o Ferreiro do Tambor (rua Oeste) | **Forja de Tambor** (`shop_forja_tambor`) | Gurparanzah: "Caudas Vermelhas na Mesa"; Kessi: "Gaviões de Brasa" |
| Serra de Kaggen (mapas 17 e 18) | **Salões de Bronze de Napirasu** (anões, `saloes_de_bronze`) | Napirasu, a Mestra Fundidora (salão da forja na cabeça da praça), Untash, o Mestre de Obras (rua Leste), Kidin, o Cozinheiro do Salão (rua Leste, no portão), Puzur, o Guardião das Runas (rua Oeste), Shilhak, o Minerador (rua Sul) | **Portões de Runa do Puzur** (`shop_portoes_runa`) | Untash: "Escória na Escada"; Shilhak: "Morcegos no Veio" |
| Grutas do Limiar (mapas 22 e 23) | **Mercado dos Túneis** (kobolds, `mercado_dos_tuneis`) | Miwnay, a Mestra da Feira (galeria Oeste), Takhsich, o Inventor (galeria Norte), Shayn, a Consertadora (galeria Leste), Farnkhund, o Vagoneiro (galeria Sul) | **Feira da Miwnay** (`shop_feira_miwnay`) | Takhsich: "Teia nos Tubos"; Farnkhund: "Pedras nos Trilhos" |
| Costa das Monções (mapas 26 e 27) | **Aratta, a Cidade dos Chifres** (povo-touro, `aratta`) | Enmerkar, o Chefe Construtor (salão de junco na cabeça da praça), Lugalbanda, o Andarilho (rua Oeste), Shulgi, o Corredor (rua Leste), Ensuhkeshdanna, o Rival Amigo (rua Sul) | **Alforje do Lugalbanda** (`shop_alforje_lugalbanda`) | Enmerkar: "Chacais no Capim Seco"; Shulgi: "O Percurso de Shulgi" |
| Costa das Monções (mapas 28 e 29) | **Cidade-Recife de Kirta** (povo-do-mar, `kirta`) | Kirta, o Rei do Recife (rua Norte), Danel, o Juiz Justo (rua Oeste), Pughat, a Que Cuida de Todos (rua Leste), Aqhat, o Arqueiro (rua Leste) | **Ferramentas de Mergulho do Danel** (`shop_mergulho_danel`) | Kirta: "Lodo no Recife"; Pughat: "Os Mergulhadores Encurralados" |
| Delta de Jhilpur (mapas 33 e 34) | **Palafitas de Tarchon** (trols, `palafitas_tarchon`) | Tarchon, o Fundador (casa comprida na cabeça da praça), Larthia, a Cesteira (rua Oeste), Ramtha, a Curandeira (rua Leste), Thanchvil, a Contadora de Histórias (rua Leste), Vel, o Pescador (rua Sul) | **Cestaria da Larthia** (`shop_cestaria_larthia`) | Tarchon: "Fogos-Fátuos Presos"; Vel: "Garças nas Redes" |
| Planície de Mingcheng (mapa 42; 45 planejado) | **Torres Brancas de Amanirenas** (altos-elfos, `torres_brancas`) | Rainha Amanirenas, a Guardiã (casa na cabeça da praça), Natakamani, o Mestre dos Espelhos (rua Oeste), Amanitore, a Construtora (rua Leste), Arkamani, o que Lê as Estrelas (rua Leste), Shanakdakhete, a que Ensina a Luz (rua Sul) | **Espelhos de Natakamani** (`shop_espelhos_natakamani`) | Amanirenas: "Espelhos Quebrados"; Shanakdakhete: "As Íbis Apagadas" |

- **Passeio da cidade:** a mesma receita `CityTownRecipes.Garajonay` do mapa 05 e do mundo contínuo (mesmo `CityId`,
  mesma geometria; os moradores só ganharam `ResidentId` nos lotes que já existiam). Problema herdado da receita (não
  mexido, outro agente usa a geometria): o banco da praça a 250° encosta na rua Sul (`CityLayoutRules` acusa).
- **Respawn:** derrota nos mapas 05 e 06 acorda na praça das Copas (`MapCatalog.RespawnCityFor` → `CityProgress.ApplyDefeatIn`);
  no mundo contínuo continua valendo o assentamento de retorno da área (`MapModeController.RespawnHere`).
- **Ervaria da Dácil** (diária, 3 vagas que giram): erva, poção de vida e **Corda de Escalada** (exigência do 05) fixas;
  **Chave do Jardim** (exigência do 06) fixa após o chefe da Floresta (como a cópia do Armarinho da Mariam, que continua
  vendendo as duas); giram poção de mana, fibra, madeira, carne, ração de montaria, arco de caça e Amuleto da Sorte
  (este após o chefe da Floresta). O Ammo nunca para numa cidade de raça.
- **Missões da cidade** (`CityQuestCatalog`, textos em `CityQuestTexts`): o morador oferece a próxima missão da corrente
  quando os primos chegam até ele (`CityResidentPost` → `CityQuestCatalog.Talk`), e pergunta como vai enquanto ela roda.
  Gara pede 6 bichos bravos do chão da mata (o Lobo faz o Lagarto-Gigante do Louro; 3 poções de vida + 2 de mana, 320 XP);
  depois Tinguaro pede 5 Javalis de Musgo (Queixada; arco de caça + 25 ouro, 380 XP). Objetivos só de combate, como as
  missões de região. Testes: `CityGarajonayTests`. A bacia da praça foi para a cabeça da praça (0; 5,2): em (4,2; −3,8)
  ela tampava o lugar de quem compra no Ammo (o "Validate Town Layouts" roda toda cidade também com o mascate).

**Tambor Rubro** (orcs, mapa 11; cidade de retorno de 11–12):
- **Passeio da cidade:** receita `CityTownRecipes.TamborRubro` (`CityTownRecipes.TamborRubro.cs`) no estilo novo
  `TownStyles.WarCamp` (`TownArchitecture.WarCamp`, vegetação do deserto, paleta da ficha com células que já existiam:
  barro vermelho/laterita = ocre-rubro, adobe ocre = arenito, teca = couro, ferro, brasa, pano amarelo): acampamento de
  guerra dentro de uma **paliçada de troncos com ponta** (`Palisade`, peça de segmento; pontas para cima, nunca
  troféus) com vãos para as ruas Sul, Leste e Oeste; **Salão Grande** de vigas e couro na cabeça da praça, telhado de
  duas águas muito alto com as varas da cumeeira cruzadas (`TownBuildingSpec.LogHall`: postes de canto, soleira e
  frechal de tronco, paredes de couro tingido atado a ripas, porta de tábua reta, faixa amarela; `TownKit.LogHallRoof`),
  tambores de guerra (`WarDrum`) nos cantos; **torre do tambor** com a Lâmpada Guardiã no topo (`DrumTower`) atrás da
  casa da Ashmunikal; tendas de clã de couro ocre (`ClanTent`), postes de clã com faixas de pano (`ClanPost`; sinal do
  clã, não altar), fogueiras (`Bonfire`, acendem à noite e entram nas luzes da cidade), arena de treino de areia
  (`TrainingRing`, pisável) e portões de tronco com tochas (`TownKit.WarCampGate`). Peças em `TownKit.WarCamp.cs`,
  colisores em `CitySolids`. Tudo em y = 0 e dentro de ±33 m (o mundo contínuo das Areias reserva ±34 de zona segura e
  ±38 de chão plano); `CityLayoutRules.Validate` sem problemas, com e sem o mascate.
- **No mundo contínuo:** `OverworldLayouts.Areias.cs` ainda monta a "primeira versão" própria (`TamborRubroCity()`,
  estilo `Oasis`). Para usar esta receita lá basta trocar a linha do `Place` do 11 por
  `CityTownRecipe tambor = CityTownRecipes.TamborRubro(); tambor.Backdrop = false;` e
  `Place(w, TamborRubroMapId, tambor, TamborRubroOffset, Rect.MinMaxRect(-34f, -34f, 34f, 34f), 38f, true);` (mesmas
  três saídas S/L/O e cais na rua Oeste).
- **Respawn:** derrota nos mapas 11 e 12 acorda na praça do Tambor Rubro (`MapCatalog.RespawnCityFor` → `ApplyDefeatIn`).
- **Forja de Tambor** (semanal, 3 vagas que giram, tema Ferreiro): ferro, minério, poção de vida, Pedra da Bênção e a
  **Teriaga do Oásis** (exigência do 12, Trono de Namrael) fixas; giram martelo, machado, peitoral de couro, arco de
  caça, carne, ração de montaria e espada de ferro (esta após o chefe do Deserto). O 11 não tem exigência. O Ammo
  nunca para lá. (O conserto 20% mais barato e a "Têmpera do Tambor" da ficha ainda faltam.)
- **Missões da cidade:** Gurparanzah pede 6 escorpiões (o Escorpião das Areias faz o Escorpião-Rubro; 4 poções de vida +
  2 Pedras da Bênção + 45 ouro, 460 XP); depois Kessi pede 5 Gaviões de Brasa (o Gavião-de-Penacho faz o papel; Teriaga
  do Oásis + 55 ouro, 540 XP). Testes: `CityTamborRubroTests`.

**Salões de Bronze de Napirasu** (anões, mapa 17; cidade de retorno de 17–18):
- **Passeio da cidade:** receita `CityTownRecipes.BronzeHalls` (`CityTownRecipes.BronzeHalls.cs`) no estilo novo
  `TownStyles.RuneHall` (`TownArchitecture.RuneHall`, linha 9 da `TownPalette`: granito `#6E6A66`, bronze `#B07A3B`,
  cobre esverdeado `#5E9C8A`, brasa fria `#8C3B2E`, luz de runa `#FFE08A` que acende à noite): cidade escavada na
  montanha. Cada casa é uma frente cortada no granito (`TownBuildingSpec.RuneHall` com telhado `Rock`:
  `TownKit.RuneHallHouse`): massa da montanha atrás, cornija de bronze, porta maciça quadrada de bronze com cravos e
  argola sob o **lintel de runa** (traços acesos um sim, um não: "acende uma linha por vez"), janelas-fresta e alpendre
  de granito sobre **pilares quadrados com base e capitel de bronze** (nos `PorchPostsX`, mesmos colisores). Peças:
  `BronzePillar` (pilar solto com faixa de runas), `GreatForge` (a **Grande Forja apagada**: fornalha de granito em
  degraus, boca escura com brasa fria, coifa de bronze até a rocha, o cristal da Luz escuro na garra, fole e duas
  bigornas), `LavaPit` (poço de lava apagada com crosta escura, rachaduras de brasa fria e correntes),
  `ChainLift` (elevador de corrente: armação de bronze, gaiola, roda e correntes) e `RockWall` (parede de rocha de
  segmento: variante 0 granito com nichos, nervuras de bronze e linha de runas), portão de runa nas saídas
  (`TownKit.RuneGate`) e a vitrine de runas do Puzur (`TownShopDressing.Runes`). Peças em `TownKit.RuneHall.cs`,
  colisores em `CitySolids`. Ruas Oeste e Leste até x = ±18 (as trilhas do mundo contínuo chegam em (±24, 0)) e Sul;
  parede de rocha só ao norte e a leste (o lado longe da câmera); tudo em y = 0 dentro de ±19 m (o mundo contínuo da
  Serra reserva x −20…21, z −19…20 de zona segura e ±24 de chão plano); `CityLayoutRules.Validate` sem problemas, com
  e sem o mascate.
- **No mundo contínuo:** `OverworldLayouts.Kaggen.cs` ainda monta a "primeira versão" própria (`SaloesDeBronze()`).
  Para usar esta receita basta trocar o `SaloesDeBronze()` do `Place` do 17 por `CityTownRecipes.BronzeHalls()` (mesmo
  `CityId`, mesmo lugar e raio; `Backdrop` já é falso) e apagar o método antigo.
- **Respawn:** derrota nos mapas 17 e 18 acorda na praça dos Salões (`MapCatalog.RespawnCityFor` → `ApplyDefeatIn`).
- **Portões de Runa do Puzur** (semanal, 3 vagas que giram, tema Especialidade): poções de vida e mana, Pedra da
  Bênção, Pedra da Alma (após o chefe da Montanha) e as exigências da região fixas: **Picareta** e **Lamparina**
  (Picareta e Tocha do 17) e **Chá de Gengibre** (Resistência à Nevasca do 18); giram as runas de bronze (Clareza,
  Couro, Cura, Vento, Cura Grande), ferro, minério e martelo. O Ammo nunca para lá. (Gravar 1 runa por dia de graça e
  as 5 etapas da Grande Forja ainda faltam.)
- **Missões da cidade:** Untash pede 5 golens de escória (o Ushabti de Pedra faz o Golem de Escória; 4 poções de vida
  + 2 Pedras da Bênção + 55 ouro, 620 XP); depois Shilhak pede 6 bandos de morcegos de brasa no veio (o Enxame de
  Morcegos faz o papel; Chá de Gengibre + 60 ouro, 700 XP). Testes: `CityBronzeHallsTests`.

**Mercado dos Túneis** (kobolds, mapa 22; cidade de retorno de 22–23):
- **Passeio da cidade:** receita `CityTownRecipes.TunnelMarket` (`CityTownRecipes.TunnelMarket.cs`) no estilo novo
  `TownStyles.TunnelBazaar` (`TownArchitecture.TunnelBazaar`: basalto `#3B3B44`, cobre `#B06B3A`, lona (pano de vela),
  vidro verde `#7FBF8F` e âmbar `#E0A040` e Ar Sereno `#C9B8F0`, que brilham à noite): o mercado no cruzamento das quatro
  galerias. Cada casa é uma barraca num **nicho de basalto sob arco redondo** (`TownBuildingSpec.TunnelStall` com
  telhado `Rock`: `TownKit.TunnelStallHouse`): prateleiras de garrafas brilhando no escuro do nicho, aduelas de cobre,
  balcão do lado do dono, **toldo listrado de lona** em postes de cobre (nos `PorchPostsX`), lanternas de garrafa e um
  cano de ar de cobre subindo a rocha. Peças: `MineRails` (trilhos de vagonete de segmento, pisáveis, um em cada
  galeria), `MineCart` (vagonete de minério ou de garrafas), `AirPipe` (cano de cobre com **cata-vento dentro do
  tubo**, que "puxa" o ar), `NicheStall` (barraquinha sob arco com toldo e garrafas), `Pulley` (roldana com balde de
  minério) e `RockWall` variante 1 (parede redonda de basalto com cano de cobre e lâmpadas de garrafa); portão de túnel
  redondo nas saídas (`TownKit.TunnelGate`); vitrines de garrafas de Ar Sereno (`AirBottles`) e de bancada de
  consertos (`Tinkerer`). Peças em `TownKit.TunnelBazaar.cs`, colisores em `CitySolids`. Quatro ruas até ±18 onde chegam
  as galerias do mundo contínuo; parede só ao norte e a leste; tudo em y = 0 dentro de ±20 m (o mundo contínuo do
  Limiar reserva x −21…22, z −20…21 de zona segura e ±24 de chão plano); `CityLayoutRules.Validate` sem problemas, com
  e sem o mascate.
- **No mundo contínuo:** `OverworldLayouts.Limiar.cs` ainda monta a "primeira versão" própria (`LimiarMarket()`, em
  `CaveTuff`). Para usar esta receita basta trocar o `LimiarMarket()` do `Place` do 22 por
  `CityTownRecipes.TunnelMarket()` (mesmo `CityId`, mesmo lugar e raio, quatro saídas e cais na galeria Oeste) e apagar
  o método antigo (`LimiarTuffHouse` continua usado pelos postos).
- **Respawn:** derrota nos mapas 22 e 23 acorda no cruzamento (`MapCatalog.RespawnCityFor` → `ApplyDefeatIn`); 21 e
  24 seguem acordando na Vila das Lamparinas.
- **Feira da Miwnay** (diária, 3 vagas que giram, tema Feira): poções, Lamparina e o **Óleo do Fogo Eterno** (exigência
  do 23, Cânion da Roda) fixos; giram minério, pedra, lascas de golem, asas de morcego, capacete de couro, amuleto da
  sorte, picareta e a Lamparina Rúnica (após o chefe das Cavernas). Também fixos: as **Asas de Lona do Takhsich** (420 ouro; vestidas nas costas
  dão o pulo duplo da exigência do 24, Ninho de Mahaway) e a **Garrafa de Ar Sereno** (30 ouro; +20% de recuperação de
  vigor por 60 s; 2 vêm também na missão do Takhsich). O 22 não tem exigência. O Ammo nunca para lá. (A Banca de Troca e
  a feira que muda de galeria ainda faltam.)
- **Missões da cidade:** Takhsich pede 6 aranhas tirando as teias dos tubos de ar (a Aranha-Tecelã faz a Lagarta de
  Pedra; 4 poções de mana + Lamparina + 65 ouro, 760 XP); depois Farnkhund pede 4 golens fora dos trilhos (o Ushabti
  de Pedra faz a Toupeira-Gigante; Óleo do Fogo Eterno + 70 ouro, 840 XP). Testes: `CityTunnelMarketTests`.

As duas cidades de raça da Costa das Monções (região 5) não têm bioma no mapa-múndi: o `CitySpec.Region` é emprestado
(Floresta, sempre desbloqueada), como o de Taa Pwani, e por isso o `ApplyDefeatIn` sempre acorda na praça delas. Cada uma
fica em y = 0 dentro de um círculo de ~22 m com 3 saídas de rua, no lugar que o mundo contínuo da região 5 reserva.

**Aratta, a Cidade dos Chifres** (povo-touro, mapa 26; cidade de retorno de 26–27):
- **Passeio da cidade:** receita `CityTownRecipes.Aratta` (`CityTownRecipes.Aratta.cs`) no estilo novo
  `TownStyles.ReedHorn` (`TownArchitecture.ReedHorn`, linha 10 da `TownPalette`: junco `#C8B27A`, esteira `#A88B55`, capim
  dourado `#D9B44A`, fita azul `#3F7CC0`, mais o barro vermelho que já existia). Cada casa é um **salão de junco**
  (mudhif dos pântanos da Mesopotâmia; `TownBuildingSpec.ReedHall`: `TownKit.ReedHallHouse`): plataforma de barro
  vermelho, abóbada de esteira sobre arcos de feixe de junco a cada ~0,9 m, frente de treliça de junco com a porta alta
  em arco, duas colunas grossas de feixe com a ponta enrolada e fitas, um par de chifres sobre a porta e dois toldos de
  esteira em varas de junco (nos `PorchPostsX`) que deixam o vão da porta livre. Peças: `HornPost` (poste de chifre com
  fitas azuis: marco de caminho, não objeto de culto), `StoneKraal` (cercado redondo de pedra seca aberto para a frente,
  com cocho e manjedoura), `BullLyre` (a **Lira do Touro** na praça: caixa escura com ouro, cabeça de touro de ouro com
  barba azul-lápis), `Acacia` (acácia de copa chata); portão de feixes com chifres e cordão de fitas nas saídas
  (`TownKit.ReedGate`); vitrine do andarilho (`TownShopDressing.Wanderer`: esteira, mapas enrolados, corda, cajado, a
  relíquia). Fogo baixo do Fogo Vivo na praça (`Bonfire`), baobá atrás do salão do Enmerkar. Peças em
  `TownKit.ReedHorn.cs`, colisores em `CitySolids`. Ruas Oeste, Leste e Sul de 20 m (saídas em (±20, 0) e (0, −20), alvo
  de chegada a 19,6 m); os postes de chifre da rua Sul marcam a largada da Corrida de Shulgi; tudo (colisores, lotes e os
  mourões do portão) dentro de um círculo de 22 m, caminhada em ±22; `CityLayoutRules.Validate` sem problemas, com e sem
  o mascate.
- **Respawn:** derrota nos mapas 26 e 27 acorda na praça de Aratta (`MapCatalog.RespawnCityFor` → `ApplyDefeatIn`).
- **Alforje do Lugalbanda** (diário, 3 vagas que giram, tema Especialidade): poção de vida, Odre de Água e a **Relíquia dos
  Artífices** (exigência do 27, Pirâmides de Meroé) fixos; giram corda de escalada, poção de mana, carne, ração de
  montaria, botas de couro, fibra e Amuleto da Sorte. O Ammo nunca para lá. (A Lira do Touro com a canção do dia, as
  Fitas de Corrida e o Auroque de Aratta ainda faltam.)
- **Missões da cidade:** Enmerkar pede 6 chacais no capim seco (o Chacal Apagado faz o Leão do Capim Apagado; 4 poções
  de vida + 3 de mana + 75 ouro, 900 XP); depois Shulgi pede 5 babuínos fora do percurso da corrida (Babuíno Apagado;
  2 Odres de Água + 90 ouro, 980 XP). Testes: `CityArattaTests`.

**Cidade-Recife de Kirta** (povo-do-mar, mapa 28; cidade de retorno de 28–29):
- **Passeio da cidade:** receita `CityTownRecipes.Kirta` (`CityTownRecipes.Kirta.cs`) no estilo novo `TownStyles.CoralReef`
  (`TownArchitecture.CoralReef`, linha 10 da `TownPalette`: coral vivo `#E8836B` (brilha um pouco à noite), turquesa do
  recife `#2FB3B3`, alga `#3E7D57`, coral apagado `#9A9590`, mais madrepérola e areia): a parte seca do atol. Cada casa é
  uma **torre de coral** (`TownBuildingSpec.ReefTower`: `TownKit.ReefTowerHouse`): anexo baixo de coral na frente toda,
  apagado e com cracas abaixo da antiga linha d'água e vivo acima, porta de cabeça redonda com aduelas de concha, janelas
  redondas; a torre redonda sobe atrás até a **cúpula de madrepérola**; alga no pé e **toldo de vieira** (costelas de
  madrepérola e coral) sobre o deque, em postes de madrepérola (nos `PorchPostsX`). Peças: `CoralMother` (o
  **coral-mãe** na poça da praça: metade oeste cinza do Lodo, metade leste reacesa), `CoralSpire` (torre de coral solta),
  `KelpGarden` (jardim de alga num anel de conchas), `ShellBridge` (água do recife atravessando a rua com uma ponte de
  concha no meio; pisável) e `ReefEdge` (borda do recife de segmento: aro de coral apagado com a água funda do abismo
  atrás); portão de pilares de coral com arco de conchas (`TownKit.CoralGate`); vitrine de mergulho do Danel
  (`TownShopDressing.DivingGear`). Peças em `TownKit.CoralReef.cs`, colisores em `CitySolids`. Ruas Oeste, Leste e Norte
  de 19 m (saídas em (±19, 0) e (0, 19), alvo de chegada a 18,6 m; a Norte vai aos jardins de alga da ficha); **sem rua
  ao sul**: ali a borda do recife desce em três degraus (z = −19 e −16) para o abismo; tudo dentro de um círculo de
  22,5 m, caminhada em ±22; `CityLayoutRules.Validate` sem problemas, com e sem o mascate.
- **Respawn:** derrota nos mapas 28 e 29 acorda na praça de Kirta (`MapCatalog.RespawnCityFor` → `ApplyDefeatIn`); 30
  segue acordando em Taa Pwani.
- **Ferramentas de Mergulho do Danel** (diária, 3 vagas que giram, tema Especialidade): poções de vida e mana, a **Concha
  Respirável** (exigência do 28) e a **Bolha de Ar Engarrafada** (exigência do 29, Kilwa Afundada) fixas; giram peixe
  dourado, fibra, carne, Amuleto da Sorte, Pedra da Bênção e arco de caça. Como Kirta fica no próprio mapa 28, a Concha
  continua também no Bazar da Mwana (Taa Pwani) para a ida. O Ammo nunca para lá. (O Apito de Concha das montarias
  aquáticas, andar no fundo e reacender o coral por faísca ainda faltam.)
- **Missões da cidade:** o rei Kirta pede 6 bolotas de lodo no recife (o Lodo de Gruta faz a Água-Viva de Lodo; 4 poções
  de vida + 4 de mana + 90 ouro, 1040 XP); depois Pughat pede 5 peixes-lanterna longe da gruta dos mergulhadores (o
  Peixe-Lanterna faz o Peixe-Leão Apagado; **Bolha de Ar** + 100 ouro, 1120 XP). Testes: `CityKirtaTests`.

**Palafitas de Tarchon** (trols, mapa 33; cidade de retorno de 33–34):
- **Passeio da cidade:** receita `CityTownRecipes.Tarchon` (`CityTownRecipes.Tarchon.cs`) no estilo novo
  `TownStyles.StiltVillage` (`TownArchitecture.StiltVillage`, linha 12 da `TownPalette`: azul-musgo `#4F7A82`, madeira
  molhada `#5B4636`, folha `#6F8F3F`, névoa lilás `#B9A9D6`, fogo-fátuo `#D6E8FF` (brilha à noite), água de charco
  `#3F5550`, junco `#B9A55A`, fio de rede `#8E8466`). Cada casa é uma **casa comprida sobre estacas**
  (`TownBuildingSpec.StiltLonghouse`: `TownKit.StiltLonghouseHouse`): água de charco embaixo, oito estacas altas com
  travessas, piso de tábua 1,25 m acima (trol tem 3 m), **empena virada para a rua** com a porta alta e janela acesa,
  **telhado de folha íngreme que desce até o piso** dos dois lados (fiada de junco embaixo, folha em cima, cumeeira com
  varas cruzadas na frente), redes secando num beiral, lanterna de fogo-fátuo pendurada na empena, escada de tábua do
  deque até a porta e toldo de folha em estacas (nos `PorchPostsX`). Peças: `WispMast` (o **Mastro dos Fogos-Fátuos**
  num deque redondo, lanternas em gaiolas lilás nos braços), `Boardwalk` (canal de charco atravessando a rua com
  passarela de tábua no meio; pisável), `SwampEdge` (cerca de estacas de segmento com o pântano aberto atrás),
  `GiantBasket` (cesto gigante: peixe, vazio ou tampado), `NetRack` (rede secando entre duas varas) e `ReedIsland`
  (ilha de junco boiando, às vezes com cabaninha); portão de duas estacas com telhadinho de folha e lanternas
  (`TownKit.StiltGate`); vitrine da cesteira (`TownShopDressing.BasketWeaver`: cestos empilhados, cesto alto, feixes de
  junco, cesto de lanternas). Peças em `TownKit.StiltVillage.cs`, colisores em `CitySolids`. Ruas Oeste (a entrada,
  pelo delta), Leste e Sul de 19 m (saídas em (±19, 0) e (0, −19)); **sem rua ao norte**: atrás da casa do Tarchon a
  cerca de estacas abre para o pântano em três lances (z = 19,5 e 14); tudo dentro de um círculo de 22,5 m (o sólido
  mais longe, a cerca do charco, chega a ~21,7 m), caminhada em ±22.
- **Respawn:** derrota nos mapas 33 e 34 acorda na praça das Palafitas, ao pé do mastro (`MapCatalog.RespawnCityFor` →
  `ApplyDefeatIn`).
- **Cestaria da Larthia** (diária, 3 vagas que giram, tema Especialidade): poções de vida e mana, **Barco de Junco**
  (33, tecido aqui) e **Amuleto Antiveneno** (34) fixos, lamparina a óleo (o Cesto de Lanternas) e o Alforje (as
  "bolsas" da Larthia, um por save, depois de limpar o Zarafshan); giram bagre, traíra, fibra, erva, carne, ração,
  corda e Amuleto da Sorte. O Nuhzadag (Jhilpur) segue vendendo 33 e 34 para a ida. O Ammo nunca para lá. (A Lanterna
  de Fogo-Fátuo, o Despertar o vizinho e o Sapo-Gigante ainda faltam.)
- **Missões da cidade:** o velho Tarchon pede 8 fogos-fátuos presos libertados (o Enxame de Vaga-lumes faz o Fogo-Fátuo
  Preso; 5 poções de vida + 5 de mana + 120 ouro, 1350 XP); depois Vel pede 6 pássaros-fantasma fora das redes (a
  Coruja-Mortalha faz a Garça-Fantasma; **Amuleto Antiveneno** + 130 ouro, 1450 XP). Testes: `CityTarchonTests`.

**Torres Brancas de Amanirenas** (altos-elfos, Planície de Mingcheng, mapa 42; cidade de retorno de 42 e 45):
- **Passeio da cidade:** receita `CityTownRecipes.TorresBrancas` (`CityTownRecipes.TorresBrancas.cs`) no estilo novo
  `TownStyles.WhiteSpire` (`TownArchitecture.WhiteSpire`, linha 15 da `TownPalette`: cal `#F3EFE6`, ouro claro `#E3C46B`,
  azul-céu `#7FB2E5`, folha prateada `#A8C5B5`, arenito de Meroé `#C79E6B`, a luz do sol `#FFF4C2` (brilha à noite: a
  luz que os espelhos passam), bronze de espelho `#B58B4C`, verde de jardim `#5F8F4E`). Cada casa é uma **casa-torre
  branca** (`TownBuildingSpec.SpireHouse`: `TownKit.SpireHouseHouse`): base de arenito em dois degraus, salão de cal com
  faixa e cornija de ouro claro e pirâmides douradas nos cantos, porta em arco de bronze e janelas altas em arco de luz,
  **torre fina** recuada com faixas de ouro, fresta de luz e **cúpula de vidro** (`Dome`), um **espelho de bronze** na
  torre e o raio que ele passa até o deque; pórtico de colunas brancas (nos `PorchPostsX`) sob arquitrave branca com a
  tabuleta. Peças: `SpireTerrace` (muro branco de segmento com **três degraus de jardim** atrás, sebes e **torres
  brancas** com cúpula de vidro ou ponta de ouro; a variante 1 ergue a **torre mais alta** no meio, sobre base de
  pirâmide, com o espelho grande e seu raio até o muro, e duas torres menores ligadas por **pontes de folha**),
  `MirrorClock` (o **Relógio de Espelhos** na praça: degrau redondo com as linhas das horas, obelisco branco com o disco
  de ouro e seis espelhos em postes em volta), `LeafBridge` (riacho de jardim atravessando a rua com uma **ponte de
  folha**, lâmina de pedra pontuda com nervura de ouro; pisável), `MirrorPost` (espelho de bronze num poste branco; a
  variante 1 passa o raio até uma pedra de luz) e `SpirePyramid` (pirâmide pequena e íngreme de arenito com frente de
  pilone e ponta de ouro, a lembrança de Meroé); portão de dois pilones brancos com o disco de ouro no lintel
  (`TownKit.PylonGate`); vitrine do mestre dos espelhos (`TownShopDressing.Mirrors`: espelhos em suportes, caixote de
  pedras polidas, braseiro de luz). Peças em `TownKit.WhiteSpire.cs`, colisores em `CitySolids`. Ruas Oeste (a entrada,
  vindo de Mingcheng), Leste e Sul de 19 m (saídas em (±19, 0) e (0, −19), alvo de chegada a 18,6 m); Cais da Barca na
  rua Oeste; **sem rua ao norte**: os jardins em terraço sobem em três lances (z = 19,5 com a torre mais alta e z = 14
  dos lados) e duas pirâmides ficam em (±13, 12,4); tudo dentro de um círculo de 22,5 m (o sólido mais longe, o muro do
  terraço do meio, chega a 21,7 m), caminhada em ±22; `CityLayoutRules.Validate` sem problemas, com e sem o mascate.
- **Respawn:** derrota nos mapas 42 e 45 acorda na praça das Torres Brancas, ao pé do Relógio de Espelhos
  (`MapCatalog.RespawnCityFor` → `ApplyDefeatIn`; hoje só o 42 está no `MapCatalog`).
- **Espelhos de Natakamani** (diária, 3 vagas que giram, tema Especialidade): poções de vida e mana e as pedras do refino
  com "luz refletida" fixas (Pedra da Bênção, Pedra da Alma, Runa da Clareza; Vida e Caos só caem de bichos); giram
  Lamparina Rúnica, Runa de Cura, erva, carne e Amuleto da Sorte. **Não** vende o Selo do Sol: é a chave da entrada do 42
  e se compra antes, em Mingcheng. O Ammo nunca para lá. (O desconto real de Pedra-Faísca nas faixas +7…+9 ainda falta.)
- **Missões da cidade:** a rainha Amanirenas pede 7 espelhos apagados quebrados (a Lanterna Quebrada faz o Espelho
  Quebrado da ficha; 5 poções de vida + 5 de mana + 160 ouro, 1850 XP); depois Shanakdakhete pede 6 íbis apagadas fora
  dos terraços (o Corvo Sombrio faz a Íbis de Luz Apagada; **Lamparina Rúnica** + 2 Pedras da Alma + 170 ouro, 1950
  XP). Testes: `CityTorresBrancasTests`.
- **Falta:** o mundo contínuo da Planície de Mingcheng vai encaixar a cidade; até lá o passeio só abre pelo menu de
  prévia do editor (`Preview Walkable Town/Torres Brancas`). O Relógio de Espelhos como quebra-cabeça (raios que mudam
  com a hora, salas de baú, atalho para o 45), as 7 torres andáveis com as pontes de folha, O Pequeno Eclipse, o Cervo
  de Luz Branca e o conjunto Torre Branca ainda faltam.

### 1.2 Cidades das regiões tardias (`CityCatalog.LateRegionCities`)

Cidades das regiões 5–8 (Docs/Mapas100.md), que ainda não têm bioma no mapa-múndi. Ficam fora de `Cities` (sem painel
no mapa-múndi, sem Ammo, fora da lista das Barcas) e de `RaceCities`; `IsLateRegionCity` as reconhece e `IsHubCity`
separa a hub da região (só nela o Ammo para). O `CitySpec.Region` é emprestado (Floresta: sempre desbloqueada, então o
`CityProgress.ApplyDefeatIn` sempre acorda na praça delas). Estilo próprio escolhido na receita (`CityTownRecipe.Style`).

| Região | Cidade | Moradores | Loja | Missões |
|---|---|---|---|---|
| Planície de Mingcheng (mapa 41; 43, 44 e 46 planejados) | **Mingcheng, a Cidade dos Nove Anéis** (capital, `mingcheng`) | Bögü, o Chefe da Guarda dos Anéis (salão na cabeça da praça), Mestre Mōzak, o Guardião das Lanternas (rua Oeste), Lin Deng, o Mestre do Canal (rua Leste), Fuduodan, o Leitor (rua Leste) | **Lanternas do Mestre Mōzak** (`shop_lanternas_mozak`) | Bögü: "Leões nos Portões"; Lin Deng: "Peixes no Canal" |
| Teto do Mundo (mapas 36–40) | **Sangkent, a Cidade-Escadaria** (capital, `sangkent`) | Abā, o Chefe das Calhas (casa na cabeça da praça), Sisinnios, o Agasalheiro (rua Oeste), Kustai, o Escriba (rua Leste), Baraies, o Professor (rua Leste) | **Agasalhos do Sisinnios** (`shop_agasalhos_sisinnios`) | Abā: "Pedras nas Calhas"; Kustai: "Neblina no Quadro" |
| Delta de Jhilpur (mapas 31, 32 e 35) | **Jhilpur, a Cidade nas Raízes** (capital, `jhilpur`) | Salmaios, o Guardião da Raiz-Mãe (rua Norte), Nuhzadag, o Barqueiro (rua Oeste), Abiesous, o Mestre da Escola de Barco (rua Leste), Tehat, a Erveira (rua Leste) | **Barcos e Trocas do Nuhzadag** (`shop_trocas_nuhzadag`) | Salmaios: "Lodo nos Ghats"; Abiesous: "Corvos nos Remos" |
| Costa das Monções (mapas 25 e 30) | **Taa Pwani** (marinheiros e mercadores suaílis, `taa_pwani`) | Capitão Liyongo, o Nakhoda (casa de 2 andares na rua Leste), Mwana, a Mercadora de Pérolas (rua Oeste), Nuru, a Guardiã do Farol (cabeça da praça, ao pé do Farol de Coral), Zawadi, o Trançador de Raízes (rua Leste), Mbaraka, o Navegador das Estrelas (rua Oeste) | **Bazar de Pérolas da Mwana** (`shop_bazar_mwana`) | Liyongo: "Ladras de Vela"; Zawadi: "Garras no Mangue" |

- **Passeio da cidade:** receita `CityTownRecipes.TaaPwani` (`CityTownRecipes.TaaPwani.cs`) no estilo `TownStyles.Coast`
  (`TownArchitecture.CoralStone`, `TownVegetation.Coast`, linha 8 da paleta da cidade: coral caiado, teca, mar, vela,
  laterita, mangue, raiz, madrepérola): casas de pedra de coral caiada com terraço e ameias escalonadas, porta retangular
  de teca entalhada com pinos de latão e *baraza* (banco de pedra) na fachada (`TownBuildingSpec.CoralStone`,
  `TownKit.SwahiliFront`); mercado de pérolas na praça (barracas variante 3 com bandejas de pérola e balança), Farol de
  Coral (torre-lâmpada de pedra de coral, peça `CoralTower`; é farol, não templo), dois dhows de vela triangular nas
  poças de maré no fim da rua Leste (`Dhow`), mangues no sudeste (`Mangrove`), coqueiros e baobás nos quintais, varais
  de lanternas nas ruas; Cais da Barca na rua Oeste. Peças em `TownKit.Coast.cs`, colisores em `CitySolids`.
  `CityLayoutRules.Validate` sem problemas.
- **Respawn:** derrota nos mapas 25 e 30 acorda na praça de Taa Pwani (`MapCatalog.RespawnCityFor` → `ApplyDefeatIn`).
- **Bazar de Pérolas da Mwana** (diário, 3 vagas que giram): poção de vida e as exigências do caminho que sai de Taa
  Pwani, fixas desde o primeiro dia: Passagem de Dhow (25), Concha Respirável (28: Kirta fica no próprio 28, então a
  concha se compra antes) e Botas de Raiz Viva (30). A Relíquia dos Artífices (27) passou para o Alforje do Lugalbanda
  (Aratta) e a Bolha de Ar (29) para as Ferramentas de Mergulho do Danel (Kirta); giram poção de mana, carne, ração de
  montaria, fibra, madeira e Amuleto da Sorte. O Ammo nunca para lá.
- **Missões da cidade:** Liyongo pede 6 Gaivotas Ladras (`enemy_forest_thief_gull`; 4 poções de vida + 3 de mana +
  60 ouro, 700 XP); depois Zawadi pede 5 Caranguejos Gigantes (`enemy_forest_coconut_crab`; Botas de
  Raiz Viva + 80 ouro, 820 XP). Testes: `CityTaaPwaniTests`.
- **Falta:** o mapa 25 ainda não é jogável, então o passeio só abre pelo menu de prévia do editor até o mapa existir. As
  outras cidades da região, Aratta (26) e Kirta (28), estão em §1.1 (cidades de raça).

**Jhilpur, a Cidade nas Raízes** (capital do Delta de Jhilpur, região 6, mapa 31; cidade de retorno de 31, 32 e 35):
- **Passeio da cidade:** receita `CityTownRecipes.Jhilpur` (`CityTownRecipes.Jhilpur.cs`) no estilo novo
  `TownStyles.RootStilt` (`TownArchitecture.RootStilt`, linha 11 da `TownPalette`: verde-raiz `#4E6B3A`, tijolo de ghat
  `#A4553A`, palha `#C9A35B`, água turva `#5F7F6E`, Água Viva `#4FC3B0` e lanternas de lótus `#FFC46B` (brilham à noite),
  lótus `#E8A0B4`, casca de raiz `#6B5236`). Cada casa fica **sobre colunas de raiz** (`TownBuildingSpec.RootStilt`:
  `TownKit.RootStiltHouse`): base de tijolo com água turva, colunas de raiz aérea torcidas com raízes-escora, piso de
  tábua 0,85 m acima, paredes de palha trançada entre postes de casca de raiz com duas faixas de tijolo, porta e
  janelinhas acesas, **telhado de palha curvo em "dorso de barco"** (do-chala do delta de Bengala: cumeeira e beirais
  caindo nas pontas), escada de tábua do deque até a porta, toldo de palha em postes de raiz (nos `PorchPostsX`) e
  lanternas de lótus nos cantos do beiral. Peças: `MotherRoot` (a **Raiz-Mãe** numa plataforma redonda de tijolo com
  anel de Água Viva no pé, tronco de raízes fundidas, galhos, copa larga e cortinas de raiz aérea; árvore, não
  santuário), `Banyan` (figueira com raízes aéreas em anel), `LotusPond` (lago de lótus com lanterna boiando),
  `RootBridge` (canal atravessando a rua com uma ponte de raiz viva no meio; pisável) e `Ghat` (ghat de tijolo de
  segmento: parapeito, degraus descendo para a água atrás); portão de pilares de raiz torcida com arco de raiz viva,
  folhas e lanternas (`TownKit.RootGate`); vitrine de trocas do Nuhzadag (`TownShopDressing.BoatTrader`: barco de junco
  emborcado, varas de equilíbrio, amuletos num cordão, cesto de fruta). Peças em `TownKit.RootStilt.cs`, colisores em
  `CitySolids`. Ruas Oeste (a entrada, pelo delta), Leste e Norte de 19 m (saídas em (±19, 0) e (0, 19), alvo de
  chegada a 18,6 m); **sem rua ao sul**: ali os ghats descem para a água em três lances (z = −19 e −16); tudo dentro de
  um círculo de 22,5 m (o sólido mais longe, o ghat, chega a 21,6 m), caminhada em ±22; `CityLayoutRules.Validate` sem
  problemas, com e sem o mascate.
- **Respawn:** derrota nos mapas 31, 32 e 35 acorda na praça de Jhilpur, ao pé da Raiz-Mãe (`MapCatalog.RespawnCityFor`
  → `ApplyDefeatIn`). 33 e 34 acordam nas Palafitas de Tarchon (§1.1).
- **Barcos e Trocas do Nuhzadag** (diária, 3 vagas que giram, tema Especialidade): poções de vida e mana e **todas as
  exigências do delta**, fixas desde o primeiro dia: Vara de Equilíbrio (32), Barco de Junco (33), Amuleto Antiveneno
  (34) e Fruta Purificadora (35) (as de Tarchon descem os canais até ele; a Cestaria da Larthia vende 33 e 34 também); giram corda
  de escalada, peixe dourado, carne, ração de montaria, fibra, Amuleto da Sorte e Pedra da Bênção. O Ammo nunca para lá.
- **Missões da cidade:** o velho Salmaios pede 7 bolotas de lodo nos ghats antes que subam até a Raiz-Mãe (o Lodo de
  Gruta faz os Filhos do Lodo da Maré de Lodo; 5 poções de vida + 4 de mana + 110 ouro, 1200 XP); depois Abiesous pede
  6 corvos fora do telhado da escola de barco (o Corvo-da-Treva faz a Garça-Lança Apagada que rouba os remos; **Fruta
  Purificadora** + 120 ouro, 1300 XP). Testes: `CityJhilpurTests`.
- **Falta:** o mapa 31 e o mundo contínuo do delta ainda não existem, então o passeio só abre pelo menu de prévia do
  editor (`Preview Walkable Town/Jhilpur`); a monção que sobe um andar, a escola de barco ensinando a remar, a Maré de
  Lodo e a Casa nas Raízes ainda faltam.

**Sangkent, a Cidade-Escadaria** (capital do Teto do Mundo, região 7, mapa 36; cidade de retorno de 36–40):
- **Passeio da cidade:** receita `CityTownRecipes.Sangkent` (`CityTownRecipes.Sangkent.cs`) no estilo novo
  `TownStyles.StoneTerrace` (`TownArchitecture.StoneTerrace`, `TownVegetation.Mountain`, linha 13 da `TownPalette`: pedra
  de terraço `#8C8173`, barro `#B98B5E`, zimbro `#5A3E2B`, neve `#E6EEF2`, damasco em flor `#F2B8A0`, seda vermelha
  `#C0392B` e azul `#2E86AB` das fitas das caravanas, brasa de lareira `#FFB866` (brilha à noite: os terraços acendem
  um por um)). Cada casa é uma **casa de pedra e barro do Pamir** de dois andares (`TownBuildingSpec.StoneTerrace`,
  `Floors = 2`: `TownKit.StoneTerraceHouse`): soco de pedra escura, térreo de pedra, andar de cima de barro (ou pedra),
  vigas de zimbro amarrando as paredes com as **pontas das vigas à mostra** na fachada, porta de moldura pesada e
  janelinhas fundas embaixo, janela acesa em cima, **varanda de treliça** (a de Baltit) do lado oposto à porta,
  terraço plano com parapeito, a claraboia quadrada da casa pamiri, damascos secando e lenha; deque de lajes com postes
  de zimbro (nos `PorchPostsX`) e alpendre plano na altura do primeiro andar, a tabuleta pendurada nele. Peças:
  `TerraceWall` (muro de arrimo de pedra seca de segmento com **três degraus de terraço** atrás, damasqueiros e
  casinhas de pedra com janela acesa; a variante 1 põe o **forte do 7º terraço** no meio: torre de pedra e barro com
  vigas, varanda acesa e as cinco fitas), `StoneChute` (pé de uma **calha de pedra polida** descendo os terraços,
  patamar de lajes com para-choque de zimbro, carrinho esperando e lanterna da estação), `ChuteCart` (carrinho de tábua
  em patins de pedra com as **cinco fitas das cinco luzes**; damascos ou fardos de feltro), `ApricotTree` (damasqueiro
  num anel de pedras; em flor ou com fruta), `IrrigationChannel` (canal de pedra com água atravessando a rua e laje de
  pedra no meio; pisável) e `CaravanStone` (o **Marco das Caravanas** na praça: marco alto num degrau redondo, travessa
  com as cinco fitas e lanterna, cocho de água de degelo); portão de pilares de pedra com vigas de zimbro, lintel com
  telhadinho, as cinco fitas e lanternas (`TownKit.TerraceGate`); vitrine do agasalheiro (`TownShopDressing.Outfitter`:
  arara de casacos de feltro acolchoado, grampos pendurados, capas dobradas, rolo de corda). As fitas são as cores das
  caravanas (as cinco luzes se encontraram nesta encruzilhada), não rito. Peças em `TownKit.StoneTerrace.cs`, colisores
  em `CitySolids`. Ruas Oeste (a entrada, a estrada das caravanas), Leste e Sul de 19 m (saídas em (±19, 0) e (0, −19),
  alvo de chegada a 18,6 m); Cais da Barca na rua Oeste; **sem rua ao norte**: ali os terraços sobem em três lances
  (z = 19,5 com o forte e z = 14 dos lados) e as duas calhas descem até os patamares em (±13, 12,4); tudo dentro de um
  círculo de 22,5 m (o sólido mais longe, o muro de arrimo do meio, chega a 21,7 m), caminhada em ±22; a cidade fica
  plana em y = 0 (os terraços de cima e o forte são cenário atrás dos muros); `CityLayoutRules.Validate` sem problemas,
  com e sem o mascate. Casas com 600–850 triângulos, peças com 70–560.
- **Respawn:** derrota nos mapas 36–40 acorda na praça de Sangkent, ao pé do Marco das Caravanas
  (`MapCatalog.RespawnCityFor` → `ApplyDefeatIn`).
- **Agasalhos do Sisinnios** (diária, 3 vagas que giram, tema Especialidade): poções de vida e mana e **todas as
  exigências do Teto do Mundo que um item atende**, fixas desde o primeiro dia: Roupa Térmica Refinada (36, vestida no
  slot Peito), Grampos de Escalada (38) e Capa de Tempestade (40); Manta de Lã e Chá de Gengibre para o frio dos passos;
  giram corda, carne, ração de montaria, fibra, erva, Amuleto da Sorte e Pedra da Bênção. O Forte dos Ventos (39) pede
  asas de verdade ou montaria voadora (Tacho Encantado, Grifo da Coluna): nenhum balcão vende. O Ammo nunca para lá.
- **Missões da cidade:** Abā pede 7 pedras-rolantes fora das calhas (`enemy_mountain_rolling_stone`, a Pedra-Rolante
  da ficha; 5 poções de vida + 5 de mana + 130 ouro, 1500 XP); depois Kustai pede 6 neblinas do vento ruim fora do
  quadro de avisos das caravanas (`enemy_mountain_ill_wind_mist`; **Capa de Tempestade** + 140 ouro, 1600 XP). Testes:
  `CitySangkentTests`.
- **Falta:** o mundo contínuo do Teto do Mundo (outro trabalho) vai encaixar a cidade; até lá o passeio só abre pelo
  menu de prévia do editor (`Preview Walkable Town/Sangkent`). As calhas e os carrinhos andáveis (transporte da cidade,
  fila de 1 por vez), os 7 terraços de verdade, o evento Avalanche, o Iaque do Teto do Mundo e o conjunto Terraço do
  Pamir ainda faltam.

**Mingcheng, a Cidade dos Nove Anéis** (capital da Planície de Mingcheng, região 8, mapa 41; cidade de retorno de 41, 43,
44 e 46):
- **Passeio da cidade:** receita `CityTownRecipes.Mingcheng` (`CityTownRecipes.Mingcheng.cs`) no estilo novo
  `TownStyles.RingWall` (`TownArchitecture.RingWall`, linha 14 da `TownPalette`: tijolo cinza `#7D7F86`, laca `#B23A2E`,
  telha dourada `#D9A82B`, jade `#5E9E7E`, papel de lanterna `#F4E3B5` (brilha à noite: as milhares de lanternas),
  telha cinza `#4E525A`, salgueiro `#8DB25A`, água de canal `#6FA3A8`). Cada casa é um **salão de pátio das cidades
  muradas** (`TownBuildingSpec.RingWallHall`: `TownKit.RingWallHallHouse`): base de pedra, paredes de tijolo cinza com
  rodapé escuro, friso de laca com faixa pintada de jade, porta dupla de laca vermelha com cravos de ouro, janelas de
  treliça acesas, **telhado curvo** de telha cinza (`CurvedRoof`: cada água em duas placas, a de baixo mais deitada, beiral
  que se abre, cumeeira com ornamentos nas pontas e **cantos virados para cima**) cobrindo o salão e o alpendre; deque de
  lajes com **colunas de laca vermelha** (nos `PorchPostsX`) e mísulas douradas, duas **lanternas de papel** entre elas e
  a tabuleta como placa de laca sobre a porta. Peças: `RingWall` (muralha de taipa com face de tijolo cinza, de segmento,
  com ameias e, atrás, duas fileiras de telhados dos anéis de dentro, cinza e depois **dourados** (telha dourada só no
  anel interno); a variante 1 abre o **portão de três arcos do 7º anel**, fechado, com folhas de laca e cravos de ouro,
  sob a torre do portão de dois telhados dourados: a cidadela fica fechada até o mapa 46), `BellDrumTower` (a **Torre do
  Tambor e do Sino** na praça: base de tijolo com passagem em arco, pavilhão de laca com o tambor e o sino de bronze,
  telhado de quatro águas com cantos virados, segundo telhado e remate de ouro), `Willow` (salgueiro chorão num anel de
  tijolo; variante 1 com lanterna), `CanalBridge` (canal atravessando a rua com **ponte de corcunda** de pedra e
  parapeitos baixos; pisável) e `LanternFrame` (armação de laca com três lanternas de papel e as **charadas** penduradas,
  a Festa das Lanternas; variante 1 com a **lanterna-carpa** dourada); portão de **três arcos** com quatro pilares de laca,
  o vão do meio mais alto, telhado curvo em cada vão, placa de ouro e lanternas (`TownKit.TripleArchGate`); vitrine do
  lanterneiro (`TownShopDressing.LanternMaker`: armação de lanternas, papel, pote de cola, lanterna-peixe). Peças em
  `TownKit.RingWall.cs`, colisores em `CitySolids`. Ruas Oeste (a entrada, pela estrada do rio), Leste e Sul de 19 m
  (saídas em (±19, 0) e (0, −19), alvo de chegada a 18,6 m); Cais da Barca na rua Oeste; **sem rua ao norte**: ali a
  muralha do 7º anel fica em três lances (z = 19,5 com o portão da cidadela e z = 14 dos lados) e duas armações de
  lanternas ficam em (±13, 12,4); tudo dentro de um círculo de 22,5 m (o sólido mais longe, a muralha do meio, chega a
  21,9 m), caminhada em ±22; `CityLayoutRules.Validate` sem problemas, com e sem o mascate.
- **Respawn:** derrota no mapa 41 (e no 43, 44 e 46 quando entrarem no `MapCatalog`) acorda na praça de Mingcheng, ao pé
  da Torre do Tambor e do Sino (`MapCatalog.RespawnCityFor` → `ApplyDefeatIn`).
- **Lanternas do Mestre Mōzak** (diária, 3 vagas que giram, tema Especialidade): poções de vida e mana e **a única
  exigência da região que um item atende**, fixa desde o primeiro dia: o **Selo do Sol** (42, preso ao herói, 1500);
  lamparina a óleo e Lamparina Rúnica para a noite dos anéis de fora; giram carne, ração, fibra, erva, corda, Amuleto da
  Sorte e Pedra da Bênção. O Ammo nunca para lá.
- **Missões da cidade:** Bögü pede 7 leões de pedra apagados fora dos portões (o Golem de Pedra faz o Leão de Pedra
  Apagado da ficha; 5 poções de vida + 5 de mana + 150 ouro, 1700 XP); depois Lin Deng pede 6 peixes-lanterna fora do
  canal (a Lanterna-Peixe do Canal das Lanternas, 43; **Selo do Sol** + 150 ouro, 1800 XP). Testes: `CityMingchengTests`.
- **Falta:** o mundo contínuo da Planície de Mingcheng (outro trabalho) vai encaixar a cidade; até lá o passeio só abre
  pelo menu de prévia do editor (`Preview Walkable Town/Mingcheng`). Os 9 anéis de verdade, os portões por hora (tambor e
  sino), as Barcas de canal entre os bairros 4–6, a Festa das Lanternas com charadas, a Lâmpada de Mingcheng (G4 da
  Maria), o Qilin de Jade e o conjunto Pátio dos Nove Anéis ainda faltam.

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

