# Status dos 100 mapas, região por região

> Levantamento de **2026-11-11**, conferido no código até o commit `6804e2b` (não no CHANGELOG), mais as mecânicas dos mapas
> 24, 76 e 83, que estão no código e ainda não foram commitadas. Fontes das faltas: linhas
> "Falta" / "No jogo" de `Docs/Mapas100.md` e `Docs/Cities.md`; bichos em `Docs/WorldBible_Maniqueismo.md` §3.2.1;
> montarias em `Docs/Mounts.md` §3.8–3.9; conjuntos em `Docs/Items.md`; móveis em `Docs/Housing/CatalogoCasa.md`.
>
> **Nada desta página foi visto jogando**, salvo onde está escrito. Tudo foi feito por código e testes EditMode.
>
> Estado dos testes no Unity:
> - **Rodados e passando:** a suíte EditMode de `84f51a4` (7.384 casos; das 7 falhas, as 2 do código commitado foram corrigidas
>   no próprio commit). Ela cobre os chefes das Regiões 5–9, a janela de mapas, o DriverHub e a performance, o botão Usar e o
>   save do baú e da faísca. Também passaram a história do meio (`7373d95`), a migração de saves (`13c8f95`), os Quatro Gulosos
>   (`6804e2b`) e a rota 01→100 (`ProgressionPathTests`, `89a7c19`).
> - **Ainda não rodados no Unity:** loot dos chefes (`24b116a`), troféus de chefe (`eca5d21`), balanço 100–500 aplicado
>   (`17941ef`, conferido só no Python), níveis das áreas (`d404331`) e as mecânicas dos mapas 24, 76 e 83 (sendo validadas agora).

## Visão geral

- **Catálogo:** os 100 mapas em `MapCatalog`, todos com `isBuilt: true`. Cada um é uma zona de um mundo contínuo.
- **Mundo:** 16 regiões contínuas, cada uma em `OverworldLayouts.<Região>.cs`, costuradas num mundo só de 7200 × 15600 m
  (`OverworldLayouts.World.cs`) com 2 buracos murados ao lado dos Reinos do Céu. Anda-se de uma região a outra sem portão e
  sem trocar de cena.
- **Cidades:** as 27 cidades do `Docs/Mapas100.md` §1 estão no `CityCatalog`: 4 hubs, 12 de raça e 11 de região tardia.
  Todas ficam dentro do mundo contínuo e usam a própria receita (`CityTownRecipes.*`). Os postos são `K.Outpost`, sem respawn.
- **Bichos próprios:** 154, todos com asset em `Resources/Enemies`: os 130 das tabelas da bíblia §3.2.1, mais 12 das Regiões 1–4
  (2026-11-11, `EarlyStandInBeastRecipes`) e mais 12 das Regiões 5–9, na estrada da história do meio (2026-11-11, `MidStandInBeastRecipes`),
  no lugar dos emprestados mais vistos. Somam-se a eles os 19 bichos de base dos 4 biomas.
- **Mecânicas:** 73 arquivos em `Gameplay/World/Map/Mechanics/`, com pelo menos uma mecânica por região. São regras puras
  com driver por região, e o estado vai salvo por slot em `WorldMapProgress`. O `MechanicsDriverHub` liga só o driver da
  região do herói (1 `Update` por frame em vez de 14). O botão de toque **Usar** do HUD acende quando há uma interação [E] ao
  alcance, o que faz as mecânicas funcionarem no celular.
- **Chefes do mundo:**
  - Regiões 1–4: eventos com horário marcado (`WorldBossRules`), lutados numa arena do mapa-múndi e não no mundo contínuo.
  - Regiões 5–16: 12 chefes no mundo contínuo (`RegionBossRules`/`RegionBossDirector`), cada um com arena plana e alcançável,
    página no Caderno, loot próprio (`24b116a`, destrava as 10 peças de roupa de chefe) e troféu de casa feito na Forja depois
    de vencê-lo (`casa_trofeu_025–036`). O estado de volta dos chefes zera em novo jogo e ao carregar, e conta pelo relógio do
    jogo (`RegionBossReturns`).
    - Regiões 5–9: Hedammu (30), Lodo-que-Engole (35), Aydahar (40), Quatro Gulosos (46) e Ohya (52).
    - Regiões 10–16: Rei-Vendaval, Velho Kul, Glutão de Brasa, Apep, Sonho da Destruição, A Grande Treva (solo) e Āz.
- **Montarias:** 30 do catálogo-base (`MountCodex`), mais o Grifo da Coluna e as 8 das Regiões 10–14 (`MountFarRegionCatalog`).
- **Conjuntos de equipamento:** Marfim dos Artífices (Ossuário, mapa 24b) e mais 4, das Regiões 13–16 (`ArmorSetRules`).
- **História:**
  - Campanha dos Atos I–V nas Regiões 1–4, até o nível ~20.
  - Cadeias pessoais dos 5 primos e a convergência.
  - Missões regionais nos 4 hubs (`RegionQuestCatalog`, Regiões 1–4) e 2 missões em cada uma das outras 23 cidades
    (46 `quest_city_*`).
  - **A Lua que Não Enche** (`MidStoryCatalog`, `Docs/HistoriaMeio_20a100.md`): 15 etapas nas Regiões 1–8, do nível 20 ao
    97, do Salão de Tages aos Fossos de Terracota, com fala dos 5 primos e o diário da Vó Edna. A primeira Carta da Estrada
    passa a exigir a lua cheia. Saves antigos: o `MidStorySaveMigration` roda uma vez ao carregar (StorySchema 0 → 1). Quem
    já tocou nas Cartas recebe as 15 etapas como concluídas, sem recompensa repetida. Quem não tocou faz a Lua como conteúdo
    novo, oferecida pela Vó Edna a partir do nível 20.
  - **As Cartas da Estrada** (`LateStoryCatalog`): 17 etapas nas Regiões 9–16, do nível 100 ao 500, até a Varanda do mapa 100.
- **Níveis:** as áreas do mundo contínuo batem com o `MapCatalog` (`d404331`: 22 divergências corrigidas,
  `MapZoneLevelConsistencyTests`). O balanço 100–500 está aplicado (`17941ef`, `Docs/Balanco100a500.md`).
- **Rede:** só o estudo (`Docs/Network.md`: Mirror + KCP, servidor Linux dedicado, Nakama). O protótipo ainda não começou.

Legenda: **feito** = no código, com testes; *(não testado jogando)* vale para tudo; "emprestado" = papel de uma ficha
feito por um bicho de outra região ou do bioma.

---

## Região 1 — Bosque do Zarafshan · mapas 01–06 · nv 1–32

| Item | Estado |
|---|---|
| Cidade | Vila da Vó Edna (hub, 01) e Copas de Garajonay (raça, 05), dentro do mundo |
| Mundo contínuo | Feito, 2400 × 2400 m, 18 áreas (`OverworldLayouts.Zarafshan.cs`). Falta conferir no Play: bordas, densidade, sombras e custo no celular |
| Bichos próprios | Fauna base da Floresta (lobo, aranha, queixada, coruja, lobisomem e Kök-Böri) e mais 7 (2026-11-11): Cegonha Apagada (02), Sapo-Bolha e Mosquito de Fumaça (03), Mariposa de Seda (04), Pombo-do-Louro e Lagarto-Gigante do Louro (05), Pavão de Fumaça (06). Ainda emprestados: Enguia de Lama (03), Lagarta-Casulo e Gralha Ladra (04), Névoa Pegajosa (05), Roseira-Espinho e Lobo de Cinzas (06) |
| Mecânicas feitas | Colheita diária no Pomar (01), vaus de Água Viva que curam até 70% (02) |
| Mecânicas faltantes | Lama ×0,7 fora da passarela e Água Viva limpando poças (03); teias que prendem (04); andar nas plataformas, névoa a cada 3 min (05); libertar 4 faíscas para encher os canais (06); item de damasco próprio (01) |
| Chefe do mundo | Saklas, o Lobo de Cinzas (evento com horário, arena do mapa-múndi); chefe de região Kök-Böri. Falta a Fiandeira Esquecida (04) |
| Montaria | Catálogo-base. Faltam as da ficha: Garça do Brejo (03), Lagarto-do-Louro (05) |
| Conjunto | Nenhum de equipamento (o "do jardim" da ficha está em `Docs/Items.md` Falta) |
| Móveis | Tugai do Zarafshan, Iwan da Vó Edna, Moinho do Pomar, Cozinha da Vó e outros conjuntos de família |

## Região 2 — Areias Flamejantes · mapas 07–12 · nv 5–38

| Item | Estado |
|---|---|
| Cidade | Oásis de Qocho (hub) e Tambor Rubro (raça, 11, estilo `WarCamp`), as duas dentro do mundo com a própria receita |
| Mundo contínuo | Feito, 2400 × 2400 m (`OverworldLayouts.Areias.cs`). Os 4 postos de 09–12 estão no mapa |
| Bichos próprios | 9: 5 de base do Deserto e mais Chacal, Cobra-de-Areia, Orc Coroado de Āz e Lagarto-Espinho (10). Os outros papéis das fichas 09–12 usam bichos existentes |
| Mecânicas feitas | Tempestade de areia em toda a região, com abrigos (`AreiasStormRules`) |
| Mecânicas faltantes | Escolta da caravana e visão de 6 m (07); leitura das inscrições (08); 09, 10 e 12 sem mecânica própria; Ritmo do Tambor e Têmpera do Tambor (11); Tambor Rubro numa mesa |
| Chefe do mundo | Namrael, o Escorpião-Rei (evento com horário); chefe de região O Grande Ushabti (08, luta pelo mapa-múndi) |
| Montaria | Catálogo-base. Falta a da ficha: Lobo de Guerra Ocre (11) |
| Conjunto | Nenhum de equipamento |
| Móveis | Areias Flamejantes, Caravana do Vô Jorge |

## Região 3 — Serra de Kaggen · mapas 13–18 · nv 9–44

| Item | Estado |
|---|---|
| Cidade | Vila de Litema (hub) e Salões de Bronze (raça, 17), dentro do mundo |
| Mundo contínuo | Feito, 2400 × 2400 m (`OverworldLayouts.Kaggen.cs`). Liga com as Areias sem portão no mundo costurado |
| Bichos próprios | 10: 4 de base da Montanha e mais Pedra que Rola, Neblina do Vento Ruim, Babuíno, Lince Apagado (14), Borrão (15) e Gralha de Pedra (16). Os outros papéis de 15–18 usam bichos existentes (Lagarto-de-Pedra, Águia-Preta, Salamandra Fria…) |
| Mecânicas feitas | Ventos da serra (13–14) e pontes de corda que gastam fôlego dobrado, que a Maithe ignora (`KaggenWindRules`) |
| Mecânicas faltantes | Curvar projéteis (13); neve no chão (14); balanço visual das pontes; ar fino no cume (18); gravar 1 runa por dia e as 5 etapas da Grande Forja (17) |
| Chefe do mundo | Mazan, o Gigante da Serra (evento com horário); chefe de região Hahya |
| Montaria | Catálogo-base. Faltam as da ficha: Eland (15), Carneiro de Bronze (17) |
| Conjunto | Nenhum de equipamento |
| Móveis | Picos de Litema |

## Região 4 — Grutas do Limiar · mapas 19–24 · nv 13–50

| Item | Estado |
|---|---|
| Cidade | Vila das Lamparinas (hub) e Mercado dos Túneis (raça, 22, estilo `TunnelBazaar`), dentro do mundo |
| Mundo contínuo | Feito, 2400 × 2400 m (`OverworldLayouts.Limiar.cs`), com o portão do Ossuário |
| Bichos próprios | 7: 4 de base das Cavernas e mais Lodo de Gruta, Corvo-da-Treva e Ubır Jovem (20, nos Terraços de Tufo). Ainda emprestados: Aranha de Gruta (19), Sombra Pequena e Pedra-Lâmina (20). Ossuário com 5 esqueletos próprios |
| Mecânicas feitas | Lâmpadas salvas que encurtam a volta à Vila (19, `LimiarLampRules`); poços de vento para planar sobre os abismos do Ninho de Mahaway (24, `LimiarWindWellRules`, ainda não validado no Unity) |
| Mecânicas faltantes | Barcos do lago (19); medo do escuro (20); a feira que muda de galeria por dia (22); entrada direta no Ossuário sem passar pelo mapa da ilha |
| Chefe do mundo | Mahaway, o Rei da Revoada (evento com horário); chefe de região Tages; Colosso de Crânios no Ossuário (24b) |
| Montaria | Grifo da Coluna da Glória (missão de nível 40, voadora) |
| Conjunto | Marfim dos Artífices (Ossuário, 2 peças, Toque Espectral) |
| Móveis | Tufo Rosa das Lamparinas |

## Região 5 — Costa das Monções · mapas 25–30 · nv 45–72

| Item | Estado |
|---|---|
| Cidade | Taa Pwani (região tardia), Aratta e Kirta (raça), dentro do mundo com a própria receita |
| Mundo contínuo | Feito, a leste do Zarafshan (`OverworldLayouts.Costa.cs`) |
| Bichos próprios | 9: Gaivota Ladra, Caranguejo-dos-Coqueiros, Crocodilo do Mangue, Leão do Capim, Rinoceronte-de-Pó e mais 4 (2026-11-11): Moreia Apagada e Ouriço de Fumaça (25), Ave-Relâmpago (26), Caranguejo-Violinista (30). Cerca de 11 papéis ainda com bicho de outra região (chacal, escorpião, ushabti, Kongamato, Garça Apagada, Abada…) |
| Mecânicas feitas | Maré do recife de 8 min com 6 baixios e baú (25, `CostaTideRules`) |
| Mecânicas faltantes | Corrida de Shulgi e Lira do Touro (26); câmera em corte, andar no fundo e reacender o coral (28); 27, 29 e 30 sem mecânica própria |
| Chefe do mundo | Hedammu (30, Nv 72): mordida em cone. Loot próprio (Escama de Hedammu) e troféu de casa |
| Montaria | Nenhuma da ficha: Dhow-Tartaruga, Auroque de Aratta, Elefante de Musawwarat |
| Conjunto | Nenhum |
| Móveis | Nenhum conjunto da região |

## Região 6 — Delta de Jhilpur · mapas 31–35 · nv 60–88

| Item | Estado |
|---|---|
| Cidade | Jhilpur (capital) e Palafitas de Tarchon (raça), dentro do mundo |
| Mundo contínuo | Feito, ao sul da Costa (`OverworldLayouts.Delta.cs`) |
| Bichos próprios | 6: Macaco Ladrão, Lontra Ladra, Píton Apagada, Trol Apagado, Garça-Lança Apagada (31) e Sanguessuga do Lodo (32, 2026-11-11), mais o Crocodilo do Mangue da Costa. Cerca de 11 papéis emprestados (Peixe-Saltador, Garça-Fantasma, Raiz-Laço, fogo-fátuo, Sapo-Boi…) |
| Mecânicas feitas | Ilhas de junco que mudam de lugar a cada 15 min (33, `DeltaIslandRules`) |
| Mecânicas faltantes | Monção, escola de barco e Maré de Lodo (31); Lanterna de Fogo-Fátuo e Despertar o vizinho (33); 32, 34 e 35 sem mecânica própria |
| Chefe do mundo | Lodo-que-Engole (35, Nv 88): puxão, mais forte à noite. Loot próprio (Lama Limpa do Lodo) e troféu |
| Montaria | Nenhuma da ficha: Búfalo-d'Água, Sapo-Gigante, Ninki Nanka Mansa |
| Conjunto | Nenhum |
| Móveis | Nenhum |

## Região 7 — Teto do Mundo · mapas 36–40 · nv 70–98

| Item | Estado |
|---|---|
| Cidade | Sangkent (capital), dentro do mundo |
| Mundo contínuo | Feito, ao norte da Serra (`OverworldLayouts.Teto.cs`) |
| Bichos próprios | 8: Urso-Pardo do Pamir, Iaque Selvagem, Irbis, Almas, Marmota Gigante e mais 3 (2026-11-11): Gralha-de-Bico-Vermelho (36), Abutre-Barbudo Apagado (37), Lobo de Gelo (38). Cerca de 11 papéis emprestados (Argali, ushabtis de calha, Fenda Viva, Gralha das Neves…) |
| Mecânicas feitas | Avalanches com sino de 20 s nas 4 calhas (37, `TetoAvalancheRules`) |
| Mecânicas faltantes | Descida cronometrada (37); calhas e carrinhos andáveis e os 7 terraços (36); 38–40 sem mecânica própria |
| Chefe do mundo | Aydahar (40, Nv 98): sopro em linha. Loot próprio e troféu |
| Montaria | Nenhuma da ficha: Iaque do Teto do Mundo, Camelo-Bactriano Branco |
| Conjunto | Nenhum (Terraço do Pamir só na ficha) |
| Móveis | Nenhum |

## Região 8 — Planície de Mingcheng · mapas 41–46 · nv 90–140

| Item | Estado |
|---|---|
| Cidade | Mingcheng (capital) e Torres Brancas (raça), dentro do mundo |
| Mundo contínuo | Feito, ao norte do Zarafshan (`OverworldLayouts.Mingcheng.cs`). 43–46 fechados no catálogo |
| Bichos próprios | 8: Leão de Pedra, Raposa de Cauda Lanterna, Soldado de Terracota, Tartaruga de Pedra, Búfalo de Lama, Filhote de Nian, Carpa-Dragão Apagada (43) e Cavalo de Terracota (45, 2026-11-11). Ainda emprestados, cerca de 10: Grou de Papel, Espelho Quebrado, Espantalho… |
| Mecânicas feitas | Muralha andável com faróis e corredor de luz (45); mundo do reflexo nos arrozais (44) |
| Mecânicas faltantes | 9 anéis e portões por hora (41); Relógio de Espelhos (42); 4 eclusas (43); palácios e o que cada um come (46) |
| Chefe do mundo | Quatro Gulosos (46, Nv 140): um apetite por semana e outro à noite. Taotie derrama oferendas de moedas e avisa antes de engolir; Qiongqi inverte os passos por 3 s (`HeroControlMirror`, teto de 4 s). Loot próprio (Bronze Antigo dos Vasos) e troféu |
| Montaria | Nenhuma da ficha: Cervo de Luz Branca, Cavalo de Terracota Desperto, Qilin de Jade (cosmético) |
| Conjunto | Nenhum (Vasos de Bronze, Arrozal, Torre Branca e Pátio dos Nove Anéis só nas fichas) |
| Móveis | Nenhum regional (Festa das Lanternas é sazonal) |

## Região 9 — Estepe dos Nartes · mapas 47–52 · nv 100–150

| Item | Estado |
|---|---|
| Cidade | Acampamento de Soslan, Fortim e Arvoredo (raça), dentro do mundo |
| Mundo contínuo | Feito, ao norte da Costa (`OverworldLayouts.Nartes.cs`) |
| Bichos próprios | 7: Grifo Jovem, Saiga, Lobo de Bronze, Gigante Sonâmbulo, Besouro-Casca, Ochokochi e Cisne de Feltro (48, 2026-11-11). Ainda emprestados, cerca de 15: Cervo de Ouro, Cabra-Montês, Lince, Garanhão, lobos e águias da estepe… |
| Mecânicas feitas | Corrida de estepe (47), Ouros do Céu (48, só a Lavine levanta), subida da Corrente de Amirani (52) |
| Mecânicas faltantes | A cidade que anda (47); escala gigante e sonhos com baús (49); tempo lento (50); 51 sem mecânica própria |
| Chefe do mundo | Ohya (52, Nv 150): corrente girando. Loot próprio (Elo de Ferro Antigo de Ohya) e troféu |
| Montaria | Carneiro Gigante da Colina e Alce da Taiga (fichas 49/50) existem, mas vendidos em Dur-Jarras (R14). Faltam Cavalo Dourado da Estepe e Cavalo de Fumaça |
| Conjunto | Nenhum |
| Móveis | Nenhum |

## Região 10 — Picos de Farnkesh · mapas 53–58 · nv 140–200

| Item | Estado |
|---|---|
| Cidade | Farnkesh (capital) e Vale dos Sátiros (raça), dentro do mundo |
| Mundo contínuo | Feito, ao norte da Estepe (`OverworldLayouts.Farnkesh.cs`) |
| Bichos próprios | 12. Ainda emprestados, cerca de 10 papéis: Abutre do Pico, Pipa-Sombra, Sino Rachado, Raio Preso, Eco Apagado, Ays, Redemoinho, Ventania, Pedra Voadora… |
| Mecânicas feitas | Rajadas nas pontes e na Garganta (55, 57); cestos de corda (54–57); correntes de planar às ilhas de nuvem (58) |
| Mecânicas faltantes | Dourado e Baile do Vale (53); pináculos em alturas de verdade (54); medidor de silêncio (56); vento contra ×0,7 e pedras-abrigo (57); chão de nuvem e ajuda da Huma (58); pontes sem chão |
| Chefe do mundo | O Rei-Vendaval (58). Faltam os chefes de mapa das fichas (Cortador de Cordas, Rei dos Ays…) |
| Montaria | Bode Dourado de Gordias (53). Falta o Filhote de Huma |
| Conjunto | Nenhum (Ninho de Pedra só na ficha) |
| Móveis | Nenhum |

## Região 11 — Mar Gelado · mapas 59–64 · nv 180–240

| Item | Estado |
|---|---|
| Cidade | Porto da Lua Branca (capital), dentro do mundo |
| Mundo contínuo | Feito, ao norte de Mingcheng (`OverworldLayouts.MarGelado.cs`) |
| Bichos próprios | 12. Ainda emprestados, 9: Enguia-Lanterna, Rachadura, Raiz-Gelo, Peixe-Bruxa, Mosquito da Tundra, Kul Pequeno, Toupeira-Gelo, Cristal Vivo, Gelo Flutuante |
| Mecânicas feitas | Canais que mudam com a fase da lua (59); gelo fino (60); pesca no gelo (64) |
| Mecânicas faltantes | Lojas que mudam com a lua e Noite Polar (59); espessura do gelo no mapa da UI (60); estação na tundra (61); túneis que mudam (62); placas que se movem e rota da aurora (63); iscas para chamar o chefe (64) |
| Chefe do mundo | O Velho Kul (64), que ainda não vem à isca |
| Montaria | Rena da Aurora (59) |
| Conjunto | Nenhum de equipamento |
| Móveis | Móveis da Casa da Lua Branca (8 peças) |

## Região 12 — Arquipélago de Hinokura · mapas 65–70 · nv 220–280

| Item | Estado |
|---|---|
| Cidade | Hinokura (capital), dentro do mundo |
| Mundo contínuo | Feito, ao norte do Teto (`OverworldLayouts.Hinokura.cs`) |
| Bichos próprios | 14 (12 e mais a Salamandra Devoradora e a Bomba Vulcânica). Ainda emprestados, cerca de 10: Pedra-Pomes Voadora, Carpa de Brasa, Bambu de Fumaça, Fumarola, Fole Vivo, Polvo de Obsidiana, Cinza Viva, Ouriço de Vidro, Faísca Raivosa, Chama Faminta |
| Mecânicas feitas | Comportas e Tremor com 3 forjas (65/69); erupção do Echeyde (67); medidor de calor da Caldeira (70) |
| Mecânicas faltantes | Bônus de refino da forja do dia e G4 do Ícaro (65); cascalho rastelado (66); resfriar a lava em ponte (68); temperatura das fornalhas (69); chão quente para a Cabra anular |
| Chefe do mundo | O Glutão de Brasa (70). Faltam Guayota, a Tartaruga-Vulcão, o Mestre das Nove Fornalhas e a montaria do chefe (loot próprio e troféu já existem) |
| Montaria | Leão-Guardião de Laca (65), Cabra-dos-Pastores do Vulcão (67) |
| Conjunto | Nenhum (Kurdalægon e Palácio de Laca só nas fichas) |
| Móveis | Nenhum |

## Região 13 — Rio das Doze Horas · mapas 71–76 · nv 260–320

| Item | Estado |
|---|---|
| Cidade | Cais de Mesektet, dentro do mundo |
| Mundo contínuo | Feito, ao norte de Hinokura (`OverworldLayouts.Rio.cs`) |
| Bichos próprios | 15 (12 e mais Ganso Ladrão, Filho de Apep e Areia Movediça). Ainda emprestados, 7: Chama Faminta, Salamandra de Sombra, Escorpião [tanque] do 74, Ushabti Apagado, Ushabti Rachado, Sombra Fervida, Crocodilo de Lodo |
| Mecânicas feitas | Horas da barca com 12 píeres e hora de 3 min (71); anel de Mehen que gira (74); fogo que escolhe (75); defender a barca encalhada enquanto os remadores a desencalham (76, `RioBarqueDefenseRules`, ainda não validado no Unity) |
| Mecânicas faltantes | Evento A Barca Atrasou e noite permanente (71); jangadas (72); puxar o trenó (73) |
| Chefe do mundo | Apep (76), que recua até a noite seguinte. Loot próprio e troféu (Barca de luz vencendo Apep). Faltam os chefes de mapa e a Pedra da Mistura |
| Montaria | Íbis Real de Mesektet (71, voadora). Falta a Barquinha de Luz (76) |
| Conjunto | Cais do Papiro (2 peças, por drop) |
| Móveis | Móveis do Cais do Papiro (8 peças) |

## Região 14 — Margens da Mistura · mapas 77–82 · nv 300–360

| Item | Estado |
|---|---|
| Cidade | Dur-Jarras, dentro do mundo |
| Mundo contínuo | Feito, ao norte do Mar Gelado (`OverworldLayouts.Mistura.cs`) |
| Bichos próprios | 15 (12 e mais Homem-Escorpião, Lince do Cedro e Abelha-Resina). Ainda emprestados, 9: Lagarto do Instante, Ushabti de Relógio, Bagre Negro, Fumaça de Piche, Pedra de Diorito, Ave do Ombro, Tremor, Pássaro-Sonho, Água que Apaga |
| Mecânicas feitas | Bolhas de tempo (78); acender o piche (79); o ombro que respira (81) |
| Mecânicas faltantes | Degraus do zigurate e evento da Cheia (77); ataques acelerados nas bolhas (78); os sete mantos (80); Ullikummi crescendo (81); acordar/sonhar (82) |
| Chefe do mundo | O Sonho da Destruição (82). Faltam Hahya e Ohya juntos, a versão sonho do mapa e os chefes de 78–81 |
| Montaria | Mušḫuššu Manso (77), Alce da Taiga (80), Carneiro Gigante da Colina (82) |
| Conjunto | Tijolo Azul (2 peças, por drop) |
| Móveis | Móveis de Tijolo Azul (8 peças) |

## Região 15 — Limiar Profundo · mapas 83–90 · nv 340–440

| Item | Estado |
|---|---|
| Cidade | Fortaleza da Última Lâmpada, dentro do mundo |
| Mundo contínuo | Feito, ao norte de Farnkesh (`OverworldLayouts.Profundo.cs`) |
| Bichos próprios | 15 (12 e mais Morcego-Lâmina, Enguia Grande e Touro de Cinza). Ainda emprestados, cerca de 11: Sombra Pequena, Fumaça, Morcego da Treva, Ushabti Apagado, Fumaça Sufocante, Chama Faminta, Redemoinho, Cardume Escuro, Serpente Cega, Rastejante, Ushabti Apagado Gigante |
| Mecânicas feitas | Rede de 12 lâmpadas com a Maré de Treva (83); vento limpa a fumaça (84); chão que acende (85); limite comum de dano que nunca mata (`ProfundoHazardRules`); os 5 portões das covas na muralha da Fortaleza, que abrem ao chegar à lâmpada de cada cova (83, `ProfundoGateRules` e o novo `ProfundoGateMechanicsDriver`, ainda não validado no Unity) |
| Mecânicas faltantes | Correntes de ar (86); comportas e nível da água (87); dano no escuro (88); ponte que acende por lances (89); a raide de 5 grupos (90, precisa de servidor) |
| Chefe do mundo | A Grande Treva, versão solo (90). Faltam a raide e os chefes das covas |
| Montaria | Nenhuma. A Raposa-Joia e o Cavalo-Estrela (precisa de servidor) faltam |
| Conjunto | Selo dos Artífices (2 peças, por drop até existir a raide) |
| Móveis | Móveis das Mil Lâmpadas (8 peças) |

## Região 16 — Reinos do Céu · mapas 91–100 · nv 420–500

| Item | Estado |
|---|---|
| Cidade | Cais da Coluna, dentro do mundo |
| Mundo contínuo | Feito, 2400 × 3600 m, sexta fileira (`OverworldLayouts.Ceu.cs`), 35 áreas |
| Bichos próprios | 13 (12 e mais Corda de Fumaça; os arcontes reaproveitam o Arconte Menor da R15). Ainda emprestados, cerca de 16: Faísca Confusa, Sombra Clandestina, Sombra que Pisa as Flores, Sombra Nadadora, Corrente Errada, Rachadura do Céu, Lanterna Fugitiva, Gota de Treva, Vento de Órbita… |
| Mecânicas feitas | Barca da Lua só à noite (93); segurar o céu nas 5 rachaduras (99); sentar ao lado da cadeira do Vô Jorge, uma história por dia (100) |
| Mecânicas faltantes | Pouca gravidade e Chegada da Caravana (91); correnteza no Rio de Luz (92); lua crescendo e lanternas (93); jarras e esferas que giram (94–95); plantar luz (96); carregar lanternas entre as barcas (97); faíscas escoltadas; pilares em grupo (99, online); pose sentada e história do dia no save (100) |
| Chefe do mundo | Āz, a Capitã Cobiçosa (96). A Arca nunca mata (tira no máximo vida − 1). Loot próprio (Semente de Luz de Āz) e troféu (Estátua de Āz devolvendo a arca). Falta a Arca Resgatada +5 |
| Montaria | Nenhuma (o Cavalo-Estrela da ficha é de temporada e online) |
| Conjunto | Cais de Luz (2 peças, por drop) |
| Móveis | Móveis do Cais de Luz (8 peças) |

---

## Buracos de progressão

Teste de rota de ponta a ponta: `Assets/Tests/EditMode/ProgressionPathTests.cs`, com as regras puras em
`ProgressionPathRules` (`Gameplay/World/Map/ProgressionPathRules.cs`). Um herói sem dom de primo sai da Vila no nível 1 e
anda o mundo contínuo a pé. Sobe de nível só com os bichos das zonas que alcançou, até 5 níveis acima do mais forte deles.
Compra nas lojas das cidades onde entrou (`CityShopCatalog`, inclusive as linhas rotativas e as "depois do chefe") e faz as
missões de região, de cidade, das Cartas da Estrada e da história do meio (`MidStoryCatalog`, contada só quando existe).
Também pega o loot dos bichos e dos chefes do mundo que encontrou (`RegionBossRules.AllSpawns`) e faz as receitas. A Barca só
liga cais já visitados a pé, então não abre nada novo. Depois, cada um dos 100 mapas, em ordem de nível de entrada, é
conferido assim:

- **(a)** o nível de entrada é alcançável com os bichos dos mapas anteriores, e o mapa abre na simulação até 5 níveis depois
  da entrada;
- **(b)** cada exigência (do mapa e das zonas dele) é atendida por uma fonte real até 5 níveis depois da entrada;
- **(c)** a cidade de retorno existe e é alcançada, e o posto ou cais do mapa fica numa zona alcançada;
- **(d)** a fonte de cada exigência não depende de um mapa posterior.

Nenhum mapa pede nível de reset, então não há o que conferir nisso.

Resultado em **2026-11-11**: o herói chega ao nível 500 e abre os 100 mapas, e o `ProgressionPathTests` **passa no Unity**
(`89a7c19`). Não há buraco aberto.

Buraco fechado:

1. **Mapas 19 (Lago do Limiar) e 20 (Salão de Tages): a Lamparina (`TorchRequired`) só chegava no nível 32.**
   - A entrada desses mapas é no nível 12–13, e todas as zonas deles pedem lamparina. A Lamparina de Óleo só era vendida
     dentro das próprias Grutas (Vila das Lamparinas), nos Salões de Bronze (mapa 17, que também pede lamparina) e no Mercado
     dos Túneis (entrada 32). Isso atrasava as Grutas ~20 níveis, com o fim da campanha e o primeiro passo da história do meio.
   - Conserto (`89a7c19`): a Forja do Tonho, na Vila de Litema (mapa 13), vende a lamparina. Toda galeria que desce do Passo
     do Hahya já pede lamparina, então ela fica à venda antes da primeira descida.

As áreas do mundo contínuo também batem com o catálogo: `d404331` corrigiu 22 divergências (entrada a pé baixada até a do mapa
em 19 mapas, topo da faixa subido em 7) e o `MapZoneLevelConsistencyTests` vigia isso. As áreas de passagem, como a Eclusa da
Lua, ficam de fora de propósito. Esse teste ainda não rodou no Unity.

---

## As 10 coisas mais importantes para jogar do começo ao fim

Revisto em 2026-11-11, à noite. Saíram da lista, porque ficaram prontos: o teste de rota de 01 a 100 (passando, buraco da
lamparina fechado), a história do meio (escrita, no código, com migração de saves) e os chefes do mundo das Regiões 5–9 (com
loot e troféu). O balanço 100–500 foi aplicado e desceu de prioridade: agora falta só conferi-lo.

1. **Rodar no Unity o que ainda não rodou e deixar EditMode e PlayMode verdes.**
   - O que falta rodar: loot dos chefes (`24b116a`), troféus (`eca5d21`), balanço aplicado (`17941ef`), níveis das áreas
     (`MapZoneLevelConsistencyTests`) e as mecânicas dos mapas 24, 76 e 83. Falta também a PlayMode inteira, que o
     `PlayModeReloadLock` agora protege do falso vermelho por domain reload no meio da suíte.
   - Por quê: tudo o que vem depois se apoia em testes, e o que é construído sobre uma suíte vermelha esconde regressão.
2. **Jogar de verdade do nível 1 ao começo da Lua** (Vila, Areias, Serra e Grutas, níveis ~1–30, com Tages, o selo e a
   primeira etapa de "A Lua que Não Enche").
   - Por quê: é o trecho que todo jogador vê, e nada do mundo contínuo, das mecânicas ou dos chefes foi visto rodando. Bordas
     de área, densidade, colisão, zona segura, respawn, cais, a lamparina na Forja do Tonho e a oferta da Vó Edna só se
     conferem jogando.
3. **Medir o mundo de 7200 × 15600 m num celular Android, com toque de verdade.**
   - Já feito por código: o `MechanicsDriverHub` (1 driver ligado por vez), o fim do lixo das exigências e as amostras de chão
     reduzidas (`ea20744`, `Docs/PerformanceOverworld.md`). Nada disso foi medido em aparelho.
   - O botão Usar, a janela de mapas (M) com o botão Mover empilhado e os controles invertidos do Qiongqi só se conferem num
     aparelho.
   - 🙋 Depende do dono: instalar o **Android Build Support** e ter um aparelho para teste.
4. **Jogar a faixa do meio, níveis 20–150:** as 15 etapas da Lua e os chefes Hedammu, Lodo-que-Engole, Aydahar, Quatro
   Gulosos e Ohya.
   - Por quê: a história e os chefes estão no código, mas o ritmo ("sei para onde ir?"), as arenas, os avisos de fase e as
     regras novas (oferendas do Taotie, passos trocados do Qiongqi) só se julgam jogando. Vale carregar um save antigo para
     ver a migração da Lua.
5. **Chefes de mapa das fichas.**
   - Por quê: agora há um chefe do mundo por região, mas quase nenhuma ficha tem o próprio chefe de mapa (Fiandeira Esquecida,
     Cortador de Cordas, Rei dos Ays, Guayota, Hobabish, os chefes das covas do Profundo…).
6. **Fechar as mecânicas que ainda mudam o percurso.**
   - Feitas hoje e ainda não validadas: poços de vento do 24, defender a barca do 76, os 5 portões do 83, jangadas (72), correntes (86),
     comportas (87) e a ponte que acende (89).
   - Faltam: barcos do lago (19), medo do escuro (20), trenó (73) e pouca gravidade (91), além das 09, 10, 12, 27, 29, 30, 32, 34, 35, 38–40 e 51 sem mecânica
     própria.
7. **Trocar os bichos emprestados mais vistos.**
   - Por quê: são cerca de 150 papéis de ficha com bicho de outra região, e a sensação de "região nova" depende deles. Começar
     pelas Regiões 5–9, que o jogador agora atravessa com a história do meio.
8. **Conferir o balanço 100–500 com o conteúdo real.**
   - Aplicado (`17941ef`): a resistência do herói por nível foi zerada, os comuns têm piso de vida 170 a partir do 100, a caça
     de comuns fica em 5,5–6,7 s e o herói parado dura 14,5–24 s diante dos chefes 200–478.
   - Falta: rodar no Unity, ver jogando, e o que o simulador não mede, como XP por bicho, preço das exigências, loot de chefe
     e Perigoso +30 (chefes ainda matam em 2–3 golpes). Ver `Docs/Balanco100a500.md` §4.5.
9. **Decisões do dono sobre o online** (`Docs/Network.md` §8, `TODO.md` 12.1):
   - campanha offline separada ou tudo online;
   - escala (MMO ou salas de 4–8);
   - os 5 primos como classes ou NPCs;
   - modelo de negócio;
   - plataformas e cross-play;
   - hospedagem, banda, DDoS e banco;
   - contas Google e Apple;
   - LGPD.

   Por quê: essas decisões definem o que é "o fim". A raide da Grande Treva, os pilares em grupo do 99 e o Cavalo-Estrela
   só existem com servidor, e o save, a pausa e os eventos mudam de forma.
10. **Arte e áudio finais no lugar dos placeholders procedurais.**
    - Por quê: quem joga do começo ao fim vê centenas de modelos e ouve sons sintetizados.
    - 🙋 Depende do dono:
      - créditos do Meshy e licença comercial do plano;
      - ícones por geração de imagem;
      - música e efeitos reais (`Docs/Audio.md`);
      - rig da Maithe (Meshy ou Mixamo).

Fora da lista, ainda faltando: montarias, conjuntos e móveis das Regiões 1–9 (as Regiões 5–9 não têm nenhum dos três).

### Só o dono decide (resumo)

- **Online:**
  - campanha offline separada;
  - escala;
  - primos como classe;
  - negócio;
  - plataformas;
  - hospedagem e orçamento (Vultr SP ~US$ 60/mês, dedicado ~US$ 150–200/mês, banda até ~US$ 1.500/mês no pior caso);
  - DDoS;
  - banco (licença do CockroachDB);
  - contas Google e Apple;
  - LGPD;
  - opcionais: FishNet Pro, uMMORPG, teste no servidor do WhaTicket.

  Fonte: `Docs/Network.md` §8 e `TODO.md` 12.1.
- **Build e loja:**
  - Android Build Support;
  - bundle id (sugestão `com.engenhoestudio.rapadura`) e ícone;
  - secrets do CI;
  - contas de desenvolvedor;
  - testes em aparelho;
  - fixar a versão do `com.coplaydev.unity-mcp`.

  Fonte: `TODO.md`, "Decisões do usuário".
- **Arte e licenças:**
  - créditos e licença do Meshy;
  - rig da Maithe;
  - origem do `Tex_HeroCloth.png`;
  - apagar ou manter `Models/Characters/Joaquim/` e `Joaquim_Child/`.
- **Casa** (🙋 `TODO.md` FASE 15, `Docs/Housing.md` §14):
  - conta da lenha;
  - ajudante que vende offline;
  - nomes das 3 cidades novas;
  - moeda "prendas";
  - loja paga só cosmética.
