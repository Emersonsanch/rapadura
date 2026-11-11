# Status dos 100 mapas, região por região

> Levantamento de **2026-11-11**, conferido no código do commit `4c8bb42` (não no CHANGELOG). Fontes das faltas: linhas
> "Falta" / "No jogo" de `Docs/Mapas100.md` e `Docs/Cities.md`; bichos em `Docs/WorldBible_Maniqueismo.md` §3.2.1;
> montarias em `Docs/Mounts.md` §3.8–3.9; conjuntos em `Docs/Items.md`; móveis em `Docs/Housing/CatalogoCasa.md`.
>
> **Nada desta página foi visto jogando**, salvo onde está escrito. Tudo foi feito por código e testes EditMode. A última suíte
> EditMode passou, menos 6 casos que o `4c8bb42` corrigiu e que ainda não rodaram de novo no Unity.
>
> Trabalho em andamento de outros agentes, **não contado aqui como pronto**: chefes do mundo das Regiões 5–9, janela de
> mapas (tecla M) e balanço dos níveis 100–500.

## Visão geral

- **Catálogo:** os 100 mapas em `MapCatalog`, todos com `isBuilt: true`. Cada um é uma zona de um mundo contínuo.
- **Mundo:** 16 regiões contínuas, cada uma em `OverworldLayouts.<Região>.cs`, costuradas num mundo só de 7200 × 15600 m
  (`OverworldLayouts.World.cs`) com 2 buracos murados ao lado dos Reinos do Céu. Anda-se de uma região a outra sem portão e
  sem trocar de cena.
- **Cidades:** as 27 cidades do `Docs/Mapas100.md` §1 estão no `CityCatalog`: 4 hubs, 12 de raça e 11 de região tardia.
  Todas ficam dentro do mundo contínuo e usam a própria receita (`CityTownRecipes.*`). Os postos são `K.Outpost`, sem respawn.
- **Bichos próprios:** 130 nas tabelas da bíblia §3.2.1, todos com asset em `Resources/Enemies`. Somam-se a eles os 19
  bichos de base dos 4 biomas.
- **Mecânicas:** 65 arquivos em `Gameplay/World/Map/Mechanics/`, com pelo menos uma mecânica por região. São regras puras
  com driver por região, e o estado vai salvo por slot em `WorldMapProgress`.
- **Chefes do mundo:**
  - Regiões 1–4: eventos com horário marcado (`WorldBossRules`), lutados numa arena do mapa-múndi e não no mundo contínuo.
  - Regiões 10–16: chefes no anel de pedras (`RegionBossRules`/`RegionBossDirector`).
  - Regiões 5–9: nenhum ainda.
- **Montarias:** 30 do catálogo-base (`MountCodex`), mais o Grifo da Coluna e as 8 das Regiões 10–14 (`MountFarRegionCatalog`).
- **Conjuntos de equipamento:** Marfim dos Artífices (Ossuário, mapa 24b) e mais 4, das Regiões 13–16 (`ArmorSetRules`).
- **História:**
  - Campanha dos Atos I–V nas Regiões 1–4, até o nível ~20.
  - Cadeias pessoais dos 5 primos e a convergência.
  - Missões regionais nos 4 hubs (`RegionQuestCatalog`, Regiões 1–4) e 2 missões em cada uma das outras 23 cidades
    (46 `quest_city_*`).
  - **As Cartas da Estrada** (`LateStoryCatalog`): 17 etapas nas Regiões 9–16, do nível 100 ao 500, até a Varanda do mapa 100.
  - **Buraco:** não há história principal entre o nível ~20 e o 100.
- **Rede:** só o estudo (`Docs/Network.md`: Mirror + KCP, servidor Linux dedicado, Nakama). O protótipo ainda não começou.

Legenda: **feito** = no código, com testes; *(não testado jogando)* vale para tudo; "emprestado" = papel de uma ficha
feito por um bicho de outra região ou do bioma.

---

## Região 1 — Bosque do Zarafshan · mapas 01–06 · nv 1–32

| Item | Estado |
|---|---|
| Cidade | Vila da Vó Edna (hub, 01) e Copas de Garajonay (raça, 05), dentro do mundo |
| Mundo contínuo | Feito, 2400 × 2400 m, 18 áreas (`OverworldLayouts.Zarafshan.cs`). Falta conferir no Play: bordas, densidade, sombras e custo no celular |
| Bichos próprios | Fauna base da Floresta (lobo, aranha, queixada, coruja, lobisomem e Kök-Böri). Ainda sem bicho próprio, 4 das fichas: Sapo-Bolha (03), Mariposa de Seda (04), Pombo-do-Louro (05), Pavão de Fumaça (06) |
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
| Bichos próprios | 8: 5 de base do Deserto e mais Chacal, Cobra-de-Areia e Orc Coroado de Āz. Os papéis das fichas 09–12 usam bichos existentes |
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
| Bichos próprios | 7: 4 de base da Montanha e mais Pedra que Rola, Neblina do Vento Ruim e Babuíno. Papéis de 15–18 com bichos existentes |
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
| Bichos próprios | 6: 4 de base das Cavernas e mais Lodo de Gruta e Corvo-da-Treva. Ainda sem bicho próprio: Ubır jovem (20). Ossuário com 5 esqueletos próprios |
| Mecânicas feitas | Lâmpadas salvas que encurtam a volta à Vila (19, `LimiarLampRules`) |
| Mecânicas faltantes | Barcos do lago (19); medo do escuro (20); poços de vento para planar (24); a feira que muda de galeria por dia (22); entrada direta no Ossuário sem passar pelo mapa da ilha |
| Chefe do mundo | Mahaway, o Rei da Revoada (evento com horário); chefe de região Tages; Colosso de Crânios no Ossuário (24b) |
| Montaria | Grifo da Coluna da Glória (missão de nível 40, voadora) |
| Conjunto | Marfim dos Artífices (Ossuário, 2 peças, Toque Espectral) |
| Móveis | Tufo Rosa das Lamparinas |

## Região 5 — Costa das Monções · mapas 25–30 · nv 45–72

| Item | Estado |
|---|---|
| Cidade | Taa Pwani (região tardia), Aratta e Kirta (raça), dentro do mundo com a própria receita |
| Mundo contínuo | Feito, a leste do Zarafshan (`OverworldLayouts.Costa.cs`) |
| Bichos próprios | 5: Gaivota Ladra, Caranguejo-dos-Coqueiros, Crocodilo do Mangue, Leão do Capim, Rinoceronte-de-Pó. Cerca de 15 papéis com bicho de outra região (chacal, escorpião, aranha, lobo, ushabti…) |
| Mecânicas feitas | Maré do recife de 8 min com 6 baixios e baú (25, `CostaTideRules`) |
| Mecânicas faltantes | Corrida de Shulgi e Lira do Touro (26); câmera em corte, andar no fundo e reacender o coral (28); 27, 29 e 30 sem mecânica própria |
| Chefe do mundo | Nenhum (outro agente trabalhando nisso, não está pronto) |
| Montaria | Nenhuma da ficha: Dhow-Tartaruga, Auroque de Aratta, Elefante de Musawwarat |
| Conjunto | Nenhum |
| Móveis | Nenhum conjunto da região |

## Região 6 — Delta de Jhilpur · mapas 31–35 · nv 60–88

| Item | Estado |
|---|---|
| Cidade | Jhilpur (capital) e Palafitas de Tarchon (raça), dentro do mundo |
| Mundo contínuo | Feito, ao sul da Costa (`OverworldLayouts.Delta.cs`) |
| Bichos próprios | 4: Macaco Ladrão, Lontra Ladra, Píton Apagada, Trol Apagado, mais o Crocodilo do Mangue da Costa. Cerca de 13 papéis emprestados (Peixe-Saltador, garças, sanguessuga, fogo-fátuo, Sapo-Boi…) |
| Mecânicas feitas | Ilhas de junco que mudam de lugar a cada 15 min (33, `DeltaIslandRules`) |
| Mecânicas faltantes | Monção, escola de barco e Maré de Lodo (31); Lanterna de Fogo-Fátuo e Despertar o vizinho (33); 32, 34 e 35 sem mecânica própria |
| Chefe do mundo | Nenhum (em andamento por outro agente) |
| Montaria | Nenhuma da ficha: Búfalo-d'Água, Sapo-Gigante, Ninki Nanka Mansa |
| Conjunto | Nenhum |
| Móveis | Nenhum |

## Região 7 — Teto do Mundo · mapas 36–40 · nv 70–98

| Item | Estado |
|---|---|
| Cidade | Sangkent (capital), dentro do mundo |
| Mundo contínuo | Feito, ao norte da Serra (`OverworldLayouts.Teto.cs`) |
| Bichos próprios | 5: Urso-Pardo do Pamir, Iaque Selvagem, Irbis, Almas, Marmota Gigante. Cerca de 14 papéis emprestados (Argali, gralhas, ushabtis de calha, abutre-barbudo…) |
| Mecânicas feitas | Avalanches com sino de 20 s nas 4 calhas (37, `TetoAvalancheRules`) |
| Mecânicas faltantes | Descida cronometrada (37); calhas e carrinhos andáveis e os 7 terraços (36); 38–40 sem mecânica própria |
| Chefe do mundo | Nenhum (em andamento por outro agente) |
| Montaria | Nenhuma da ficha: Iaque do Teto do Mundo, Camelo-Bactriano Branco |
| Conjunto | Nenhum (Terraço do Pamir só na ficha) |
| Móveis | Nenhum |

## Região 8 — Planície de Mingcheng · mapas 41–46 · nv 90–140

| Item | Estado |
|---|---|
| Cidade | Mingcheng (capital) e Torres Brancas (raça), dentro do mundo |
| Mundo contínuo | Feito, ao norte do Zarafshan (`OverworldLayouts.Mingcheng.cs`). 43–46 fechados no catálogo |
| Bichos próprios | 6: Leão de Pedra, Raposa de Cauda Lanterna, Soldado de Terracota, Tartaruga de Pedra, Búfalo de Lama, Filhote de Nian. Ainda emprestados, cerca de 12: Carpa-Dragão, Cavalo de Terracota, Grou de Papel, Espelho Quebrado, Espantalho… |
| Mecânicas feitas | Muralha andável com faróis e corredor de luz (45); mundo do reflexo nos arrozais (44) |
| Mecânicas faltantes | 9 anéis e portões por hora (41); Relógio de Espelhos (42); 4 eclusas (43); palácios e o que cada um come (46) |
| Chefe do mundo | Nenhum (em andamento por outro agente) |
| Montaria | Nenhuma da ficha: Cervo de Luz Branca, Cavalo de Terracota Desperto, Qilin de Jade (cosmético) |
| Conjunto | Nenhum (Vasos de Bronze, Arrozal, Torre Branca e Pátio dos Nove Anéis só nas fichas) |
| Móveis | Nenhum regional (Festa das Lanternas é sazonal) |

## Região 9 — Estepe dos Nartes · mapas 47–52 · nv 100–150

| Item | Estado |
|---|---|
| Cidade | Acampamento de Soslan, Fortim e Arvoredo (raça), dentro do mundo |
| Mundo contínuo | Feito, ao norte da Costa (`OverworldLayouts.Nartes.cs`) |
| Bichos próprios | 6: Grifo Jovem, Saiga, Lobo de Bronze, Gigante Sonâmbulo, Besouro-Casca, Ochokochi. Ainda emprestados, cerca de 16: Cisne de Feltro, Cervo de Ouro, Cabra-Montês, Lince, Garanhão, lobos e águias da estepe… |
| Mecânicas feitas | Corrida de estepe (47), Ouros do Céu (48, só a Lavine levanta), subida da Corrente de Amirani (52) |
| Mecânicas faltantes | A cidade que anda (47); escala gigante e sonhos com baús (49); tempo lento (50); 51 sem mecânica própria |
| Chefe do mundo | Nenhum (em andamento por outro agente) |
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
| Chefe do mundo | O Glutão de Brasa (70). Faltam Guayota, a Tartaruga-Vulcão, o Mestre das Nove Fornalhas, troféu e montaria do chefe |
| Montaria | Leão-Guardião de Laca (65), Cabra-dos-Pastores do Vulcão (67) |
| Conjunto | Nenhum (Kurdalægon e Palácio de Laca só nas fichas) |
| Móveis | Nenhum |

## Região 13 — Rio das Doze Horas · mapas 71–76 · nv 260–320

| Item | Estado |
|---|---|
| Cidade | Cais de Mesektet, dentro do mundo |
| Mundo contínuo | Feito, ao norte de Hinokura (`OverworldLayouts.Rio.cs`) |
| Bichos próprios | 15 (12 e mais Ganso Ladrão, Filho de Apep e Areia Movediça). Ainda emprestados, 7: Chama Faminta, Salamandra de Sombra, Escorpião [tanque] do 74, Ushabti Apagado, Ushabti Rachado, Sombra Fervida, Crocodilo de Lodo |
| Mecânicas feitas | Horas da barca com 12 píeres e hora de 3 min (71); anel de Mehen que gira (74); fogo que escolhe (75) |
| Mecânicas faltantes | Evento A Barca Atrasou e noite permanente (71); jangadas (72); puxar o trenó (73); defender a barca (76) |
| Chefe do mundo | Apep (76), que recua até a noite seguinte. Faltam os chefes de mapa e os ganchos (escama, Pedra da Mistura, troféu) |
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
| Mecânicas feitas | Rede de 12 lâmpadas com a Maré de Treva (83); vento limpa a fumaça (84); chão que acende (85); limite comum de dano que nunca mata (`ProfundoHazardRules`) |
| Mecânicas faltantes | Os 5 portões das covas (83); correntes de ar (86); comportas e nível da água (87); dano no escuro (88); ponte que acende por lances (89); a raide de 5 grupos (90, precisa de servidor) |
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
| Chefe do mundo | Āz, a Capitã Cobiçosa (96). Faltam a Arca Resgatada +5 e o troféu |
| Montaria | Nenhuma (o Cavalo-Estrela da ficha é de temporada e online) |
| Conjunto | Cais de Luz (2 peças, por drop) |
| Móveis | Móveis do Cais de Luz (8 peças) |

---

## As 10 coisas mais importantes para jogar do começo ao fim

1. **Rodar de novo a suíte EditMode (e a PlayMode) no Unity e deixá-la verde.**
   - Por quê: o `4c8bb42` corrigiu 6 casos que ainda não rodaram. Tudo o que vem depois se apoia em testes, e qualquer
     coisa construída sobre uma suíte vermelha esconde regressão.
2. **Jogar de verdade do nível 1 ao fim do Ato V** (Vila, Areias, Serra e Grutas, níveis ~1–20, com Tages e o selo).
   - Por quê: é o trecho que todo jogador vê. Nenhum mundo contínuo, mecânica ou chefe novo foi visto rodando. Bordas de
     área, densidade, colisão, zona segura, respawn, cais e o novo diálogo dos lojistas só se conferem jogando.
3. **Medir o mundo de 7200 × 15600 m num celular Android.**
   - Por quê: o jogo é mobile, e o streaming por pedaços só foi medido fora do Editor (.NET).
   - Ponto de partida: a auditoria só de leitura em `Docs/PerformanceOverworld.md` (2026-11-11), que nada mediu em aparelho.
   - 🙋 Depende do dono: instalar o **Android Build Support** e ter um aparelho para teste.
4. **História principal entre o nível ~20 e o 100.**
   - Por quê: a campanha (Atos I–V) termina por volta do nível 20, e As Cartas da Estrada só começam no 100 (Soslan, mapa
     47). Entre os dois só há missões de cidade.
   - Ficam sem fio condutor o segundo anel das Regiões 1–4 e as Regiões 5–8 inteiras, ou seja, cerca de 80 níveis em que o
     jogador não sabe para onde ir.
5. **Um teste de rota de ponta a ponta**, que leve um herói do mapa 01 ao 100 checando nível a pé e de barca, exigências
   especiais (asas, lamparina, corda, capa…), onde cada item é comprado ou ganho, e cais visitados.
   - Por quê: são 100 zonas com exigências. Basta uma exigência sem fonte antes do nível do mapa para o jogo travar no meio.
   - A janela M está com outro agente, mas o teste é independente dela.
6. **Balanço de 100 a 500 aplicado e conferido com o conteúdo real** (XP por bicho, curva, dano dos chefes, loot e preço
   das exigências).
   - Por quê: sem isso o fim fica inalcançável ou trivial.
   - O balanço está com outro agente. Falta validá-lo contra as 16 regiões e os chefes 10–16.
7. **Chefes do mundo das Regiões 5–9 e chefes de mapa das fichas.**
   - Por quê: há um vão de chefes entre o Mahaway (R4) e o Rei-Vendaval (R10), justo nos níveis 45–150. Além disso, quase
     nenhuma ficha tem o próprio chefe de mapa.
   - Os chefes de 5–9 estão com outro agente.
8. **Decisões do dono sobre o online** (`Docs/Network.md` §8, `TODO.md` 12.1):
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
9. **Trocar os bichos emprestados mais vistos e fechar as mecânicas que mudam o percurso.**
   - Por quê: são cerca de 150 papéis de ficha com bicho de outra região, e a sensação de "região nova" depende deles.
   - Mecânicas que mudam o percurso: poços de vento do 24, barcos do lago, defender a barca do 76, os 5 portões do 83.
   - Pesa menos que jogar e balancear, mas é o que faz as 16 regiões parecerem 16.
10. **Arte e áudio finais no lugar dos placeholders procedurais.**
    - Por quê: quem joga do começo ao fim vê centenas de modelos e ouve sons sintetizados.
    - 🙋 Depende do dono:
      - créditos do Meshy e licença comercial do plano;
      - ícones por geração de imagem;
      - música e efeitos reais (`Docs/Audio.md`);
      - rig da Maithe (Meshy ou Mixamo).

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
