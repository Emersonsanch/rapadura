# Status dos 100 mapas, região por região

> Levantamento de **2026-11-11**, conferido no código até o commit `8ad5046` (não no CHANGELOG). Fontes das faltas: linhas
> "Falta" / "No jogo" / "Chefe" de `Docs/Mapas100.md` e `Docs/Cities.md`; bichos em `Docs/WorldBible_Maniqueismo.md` §3.2.1;
> montarias em `Docs/Mounts.md` §3.8–3.9; conjuntos em `Docs/Items.md`; móveis em `Docs/Housing/CatalogoCasa.md`.
>
> **Nada desta página foi visto jogando**, salvo onde está escrito. Tudo foi feito por código e testes.
>
> Estado dos testes no Unity:
> - **Rodados e passando:** a suíte EditMode inteira em `068253c` (7.740 casos), em `c5b0a2a` (8.146) e de novo em `8ad5046`.
>   Ela cobre o que antes faltava rodar: loot dos chefes (`24b116a`), troféus (`eca5d21`), balanço 100–500 (`17941ef`),
>   níveis das áreas (`MapZoneLevelConsistencyTests`) e as mecânicas dos mapas 24, 76 e 83 (`6bd5bae`), além de tudo o que
>   entrou depois (Fontes do Lodo, `MessageCards`, economia, mecânicas 72/86/87/89, 6 chefes de mapa, telas no celular e os
>   24 bichos novos). Na PlayMode passaram, uma a uma, a fumaça das 16 regiões (`RegionMechanicsSmokeTests`, `ef7b9f6`) e a
>   história do meio jogada inteira por código (`MidStoryPlaythroughTests`, `37b4820`, 23 s).
> - **Ainda não rodado:** a suíte PlayMode inteira de uma vez.
> - **Em andamento, sem commit** (outros agentes, não contados como prontos): faíscas e canais do Jardim de Mihrshah (06),
>   carrinho das calhas (37), medidor de silêncio das Ermidas (56) e correnteza do Rio de Luz (92), mais ajustes de missões
>   de cidade e do `economy.py`.

## Visão geral

- **Catálogo:** os 100 mapas em `MapCatalog`, todos com `isBuilt: true`. Cada um é uma zona de um mundo contínuo.
- **Mundo:** 16 regiões contínuas, cada uma em `OverworldLayouts.<Região>.cs`, costuradas num mundo só de 7200 × 15600 m
  (`OverworldLayouts.World.cs`) com 2 buracos murados ao lado dos Reinos do Céu. Anda-se de uma região a outra sem portão e
  sem trocar de cena.
- **Cidades:** as 27 cidades do `Docs/Mapas100.md` §1 estão no `CityCatalog`: 4 hubs, 12 de raça e 11 de região tardia.
  Todas ficam dentro do mundo contínuo e usam a própria receita (`CityTownRecipes.*`). Os postos são `K.Outpost`, sem respawn.
- **Bichos próprios:** 154, todos com asset em `Resources/Enemies`: os 130 das tabelas da bíblia §3.2.1, mais 12 das Regiões 1–4
  (`068253c`, `EarlyStandInBeastRecipes`) e mais 12 das Regiões 5–9, na estrada da história do meio (`8ad5046`,
  `MidStandInBeastRecipes`), no lugar dos emprestados mais vistos. Somam-se a eles os 19 bichos de base dos 4 biomas. Ainda
  há cerca de 140 papéis de ficha feitos por bicho emprestado (contagem por região abaixo).
- **Mecânicas:** 86 arquivos commitados em `Gameplay/World/Map/Mechanics/` (mais 13 em andamento), com pelo menos uma
  mecânica por região. São regras puras com driver por região, e o estado vai salvo por slot em `WorldMapProgress`. O
  `MechanicsDriverHub` liga só o driver da região do herói. O botão de toque **Usar** do HUD acende quando há uma interação
  [E] ao alcance. Desde `ef7b9f6` entraram as 5 Fontes do Lodo (35), as jangadas (72), as correntes de ar (86), as comportas
  (87) e a ponte do Novelo (89); os poços de vento (24), a barca de Apep (76) e os portões das covas (83) passaram no Unity.
- **Chefes do mundo:**
  - Regiões 1–4: eventos com horário marcado (`WorldBossRules`), lutados numa arena do mapa-múndi e não no mundo contínuo.
  - Regiões 5–16: 12 chefes no mundo contínuo (`RegionBossRules`/`RegionBossDirector`), cada um com arena plana e alcançável,
    página no Caderno, loot próprio (`24b116a`) e troféu de casa (`casa_trofeu_025–036`). O chefe do mundo paga ×23 o XP de
    um comum do nível (`28b8f39`). O Lodo-que-Engole só surge com as 5 Fontes acesas (`f0991e3`).
    - Regiões 5–9: Hedammu (30), Lodo-que-Engole (35), Aydahar (40), Quatro Gulosos (46) e Ohya (52).
    - Regiões 10–16: Rei-Vendaval, Velho Kul, Glutão de Brasa, Apep, Sonho da Destruição, A Grande Treva (solo) e Āz.
- **Chefes de mapa:** 11 feitos, de cerca de 68 pedidos pelas fichas.
  - 5 antigos: Kök-Böri (02), O Grande Ushabti (08), Hahya (14) e Tages (20), lutados pelo mapa-múndi, e o Colosso de
    Crânios no Ossuário (24b).
  - 6 novos no mundo contínuo (`5b6dcb7`, `RegionBossRules.MapSpawns`, Nv 160–264), com golpe próprio, arena na trilha,
    modelo, loot e Caderno, e XP ×18 de um comum do nível: Cortador de Cordas (55), Sino que Não Para (56), Morsa-Rainha
    (60), Mamute Avô (62), Rei Shisa do Telhado (66) e Mestre das Nove Fornalhas (69).
  - Faltam 57 (lista por região abaixo), 31 deles nas Regiões 1–9.
- **Montarias:** 30 do catálogo-base (`MountCodex`), mais o Grifo da Coluna e as 8 das Regiões 10–14 (`MountFarRegionCatalog`).
- **Conjuntos de equipamento:** Marfim dos Artífices (Ossuário, mapa 24b) e mais 4, das Regiões 13–16 (`ArmorSetRules`).
- **História:**
  - Campanha dos Atos I–V nas Regiões 1–4, até o nível ~20.
  - Cadeias pessoais dos 5 primos e a convergência.
  - Missões regionais nos 4 hubs (`RegionQuestCatalog`, Regiões 1–4) e 2 missões em cada uma das outras 23 cidades
    (46 `quest_city_*`).
  - **A Lua que Não Enche** (`MidStoryCatalog`, `Docs/HistoriaMeio_20a100.md`): 15 etapas nas Regiões 1–8, do nível 20 ao
    97, com fala dos 5 primos e o diário da Vó Edna. A primeira Carta da Estrada passa a exigir a lua cheia. Saves antigos
    migram uma vez (`MidStorySaveMigration`, StorySchema 0 → 1). O `MidStoryPlaythroughTests` (PlayMode) joga as 15 etapas
    por código, com save e load no meio, até o Soslan oferecer a primeira Carta no 100. *Não testada jogando.*
  - **As Cartas da Estrada** (`LateStoryCatalog`): 17 etapas nas Regiões 9–16, do nível 100 ao 500, até a Varanda do mapa 100.
  - Bíblia do mundo em dia e textos corrigidos (`704281b`).
- **Níveis e economia:** as áreas batem com o `MapCatalog` (`d404331`, `MapZoneLevelConsistencyTests`). Balanço 100–500
  aplicado (`17941ef`) e economia medida (`73c825b`, `tools/balance/economy.py`) e aplicada (`1a77156`): a passagem da barca
  para de subir no nível 57 (teto de 905 de ouro), o XP de missão acima do 50 paga uma fração do XP do nível (história do
  meio 50%, Carta 150%, cidade 25%) e o ouro acima do 100 tem destino (refino, conserto com teto, reset de talentos), ~37%
  do ouro de 101–500. Ainda assusta: "Perigoso +30" nos comuns (um par tira 101–208% da vida numa luta) e os chefes matam
  em 2–3 golpes (`Docs/Balanco100a500.md` §4.5, §5.4).
- **Telas no celular** (`c5b0a2a`): auditoria com prints em 1280×720 e 2400×1080 no Editor, sobreposições corrigidas, alvos
  de toque com 48 px, `PhoneScreenLayoutTests`. Os cards de mensagem não se sobrepõem mais (`47ab104`, `MessageCards`).
  Nada disso foi visto num aparelho.
- **Rede:** só o estudo (`Docs/Network.md`: Mirror + KCP, servidor Linux dedicado, Nakama). O protótipo ainda não começou.

Legenda: **feito** = no código, com testes passando no Unity; *(não testado jogando)* vale para tudo; "emprestado" = papel
de uma ficha feito por um bicho de outra região ou do bioma.
---

## Região 1 — Bosque do Zarafshan · mapas 01–06 · nv 1–32

| Item | Estado |
|---|---|
| Cidade | Vila da Vó Edna (hub, 01) e Copas de Garajonay (raça, 05), dentro do mundo |
| Mundo contínuo | Feito, 2400 × 2400 m, 18 áreas (`OverworldLayouts.Zarafshan.cs`). Falta conferir no Play: bordas, densidade, sombras e custo no celular |
| Bichos próprios | 6 de base da Floresta (lobo, aranha, queixada, coruja, lobisomem e Kök-Böri) e mais 7 (`068253c`): Cegonha Apagada (02), Sapo-Bolha e Mosquito de Fumaça (03), Mariposa de Seda (04), Pombo-do-Louro e Lagarto-Gigante do Louro (05), Pavão de Fumaça (06). Ainda emprestados, 6: Enguia de Lama (03), Lagarta-Casulo e Gralha Ladra (04), Névoa Pegajosa (05), Roseira-Espinho e Lobo de Cinzas (06) |
| Mecânicas feitas | Colheita diária no Pomar (01), vaus de Água Viva que curam até 70% (02) |
| Mecânicas faltantes | Lama ×0,7 fora da passarela e Água Viva limpando poças (03); teias que prendem (04); andar nas plataformas, névoa a cada 3 min (05); item de damasco próprio (01). Em andamento, sem commit: libertar as faíscas para encher os canais (06, `ZarafshanGardenRules`) |
| Chefe do mundo | Saklas, o Lobo de Cinzas (evento com horário, arena do mapa-múndi) |
| Chefe de mapa | Feito: Kök-Böri (02, pelo mapa-múndi). Faltam 3: Mãe-Lodo (03), A Fiandeira Esquecida (04), O Velho da Névoa (05) |
| Montaria | Catálogo-base. Faltam as da ficha: Garça do Brejo (03), Lagarto-do-Louro (05) |
| Conjunto | Nenhum de equipamento (o "do jardim" da ficha está em `Docs/Items.md` Falta) |
| Móveis | Tugai do Zarafshan, Iwan da Vó Edna, Moinho do Pomar, Cozinha da Vó e outros conjuntos de família |

## Região 2 — Areias Flamejantes · mapas 07–12 · nv 5–38

| Item | Estado |
|---|---|
| Cidade | Oásis de Qocho (hub) e Tambor Rubro (raça, 11, estilo `WarCamp`), as duas dentro do mundo com a própria receita |
| Mundo contínuo | Feito, 2400 × 2400 m (`OverworldLayouts.Areias.cs`). Os 4 postos de 09–12 estão no mapa |
| Bichos próprios | 9: 5 de base do Deserto e mais Chacal, Cobra-de-Areia, Orc Coroado de Āz e Lagarto-Espinho (10, `068253c`). Os outros papéis das fichas 09–12 ainda usam bichos existentes (sem contagem por papel) |
| Mecânicas feitas | Tempestade de areia em toda a região, com abrigos (`AreiasStormRules`) |
| Mecânicas faltantes | Escolta da caravana e visão de 6 m (07); leitura das inscrições (08); 09, 10 e 12 sem mecânica própria; Ritmo do Tambor e Têmpera do Tambor (11); Tambor Rubro numa mesa |
| Chefe do mundo | Namrael, o Escorpião-Rei (evento com horário) |
| Chefe de mapa | Feito: O Grande Ushabti (08, pelo mapa-múndi). Faltam 3: A Raiz que Bebeu o Poço (09), O Coral da Duna (10), O Chefe Coroado (11) |
| Montaria | Catálogo-base. Falta a da ficha: Lobo de Guerra Ocre (11) |
| Conjunto | Nenhum de equipamento |
| Móveis | Areias Flamejantes, Caravana do Vô Jorge |

## Região 3 — Serra de Kaggen · mapas 13–18 · nv 9–44

| Item | Estado |
|---|---|
| Cidade | Vila de Litema (hub) e Salões de Bronze (raça, 17), dentro do mundo |
| Mundo contínuo | Feito, 2400 × 2400 m (`OverworldLayouts.Kaggen.cs`). Liga com as Areias sem portão no mundo costurado |
| Bichos próprios | 10: 4 de base da Montanha e mais Pedra que Rola, Neblina do Vento Ruim, Babuíno, Lince Apagado (14), Borrão (15) e Gralha de Pedra (16) (os 3 últimos de `068253c`). Os outros papéis de 15–18 ainda usam bichos existentes (Lagarto-de-Pedra, Águia-Preta, Salamandra Fria…; sem contagem por papel) |
| Mecânicas feitas | Ventos da serra (13–14) e pontes de corda que gastam fôlego dobrado, que a Maithe ignora (`KaggenWindRules`) |
| Mecânicas faltantes | Curvar projéteis (13); neve no chão (14); balanço visual das pontes; ar fino no cume (18); gravar 1 runa por dia e as 5 etapas da Grande Forja (17) |
| Chefe do mundo | Mazan, o Gigante da Serra (evento com horário) |
| Chefe de mapa | Feito: Hahya (14, pelo mapa-múndi). Faltam 3: O Grande Borrão (15), O Pedreiro Mazan (16), O Fole Apagado (17) |
| Montaria | Catálogo-base. Faltam as da ficha: Eland (15), Carneiro de Bronze (17) |
| Conjunto | Nenhum de equipamento |
| Móveis | Picos de Litema |

## Região 4 — Grutas do Limiar · mapas 19–24 · nv 13–50

| Item | Estado |
|---|---|
| Cidade | Vila das Lamparinas (hub) e Mercado dos Túneis (raça, 22, estilo `TunnelBazaar`), dentro do mundo |
| Mundo contínuo | Feito, 2400 × 2400 m (`OverworldLayouts.Limiar.cs`), com o portão do Ossuário |
| Bichos próprios | 7: 4 de base das Cavernas e mais Lodo de Gruta, Corvo-da-Treva e Ubır Jovem (20, nos Terraços de Tufo, `068253c`). Ainda emprestados, 3: Aranha de Gruta (19), Sombra Pequena e Pedra-Lâmina (20). Ossuário com 5 esqueletos próprios |
| Mecânicas feitas | Lâmpadas salvas que encurtam a volta à Vila (19, `LimiarLampRules`); poços de vento para planar sobre os abismos do Ninho de Mahaway (24, `LimiarWindWellRules`, testes passando no Unity) |
| Mecânicas faltantes | Barcos do lago (19); medo do escuro (20); a feira que muda de galeria por dia (22); entrada direta no Ossuário sem passar pelo mapa da ilha |
| Chefe do mundo | Mahaway, o Rei da Revoada (evento com horário) |
| Chefe de mapa | Feitos: Tages (20, pelo mapa-múndi) e o Colosso de Crânios no Ossuário (24b). Faltam 2: O Vishap Adormecido (21), O Engarrafador Guloso (22). O 23 é gancho da sala de evento Torre das Jarras |
| Montaria | Grifo da Coluna da Glória (missão de nível 40, voadora) |
| Conjunto | Marfim dos Artífices (Ossuário, 2 peças, Toque Espectral) |
| Móveis | Tufo Rosa das Lamparinas |

## Região 5 — Costa das Monções · mapas 25–30 · nv 45–72

| Item | Estado |
|---|---|
| Cidade | Taa Pwani (região tardia), Aratta e Kirta (raça), dentro do mundo com a própria receita |
| Mundo contínuo | Feito, a leste do Zarafshan (`OverworldLayouts.Costa.cs`) |
| Bichos próprios | 9: Gaivota Ladra, Caranguejo-dos-Coqueiros, Crocodilo do Mangue, Leão do Capim, Rinoceronte-de-Pó e mais 4 (2026-11-11): Moreia Apagada e Ouriço de Fumaça (25), Ave-Relâmpago (26), Caranguejo-Violinista (30) (`8ad5046`). Ainda emprestados, cerca de 11 papéis (chacal, escorpião, ushabti, Kongamato, Garça Apagada, Abada, Caranguejo-dos-Coqueiros Gigante…) |
| Mecânicas feitas | Maré do recife de 8 min com 6 baixios e baú (25, `CostaTideRules`) |
| Mecânicas faltantes | Corrida de Shulgi e Lira do Touro (26); câmera em corte, andar no fundo e reacender o coral (28); 27, 29 e 30 sem mecânica própria |
| Chefe do mundo | Hedammu (30, Nv 72): mordida em cone, vida ×1,2 (`068253c`). Loot próprio (Escama de Hedammu) e troféu de casa |
| Chefe de mapa | Nenhum feito. Faltam 5: O Polvo das Monções (25), O Touro de Fumaça (26), O Leão dos Relevos (27), A Moreia do Coral Cinza (28), O Caranguejo-Palácio (29) |
| Montaria | Nenhuma da ficha: Dhow-Tartaruga, Auroque de Aratta, Elefante de Musawwarat |
| Conjunto | Nenhum |
| Móveis | Nenhum conjunto da região |

## Região 6 — Delta de Jhilpur · mapas 31–35 · nv 60–88

| Item | Estado |
|---|---|
| Cidade | Jhilpur (capital) e Palafitas de Tarchon (raça), dentro do mundo |
| Mundo contínuo | Feito, ao sul da Costa (`OverworldLayouts.Delta.cs`) |
| Bichos próprios | 6: Macaco Ladrão, Lontra Ladra, Píton Apagada, Trol Apagado, Garça-Lança Apagada (31) e Sanguessuga do Lodo (32) (`8ad5046`), mais o Crocodilo do Mangue da Costa. Ainda emprestados, cerca de 11 papéis (Peixe-Saltador, Garça-Fantasma, Raiz-Laço, fogo-fátuo, Sapo-Boi…) |
| Mecânicas feitas | Ilhas de junco que mudam de lugar a cada 15 min (33, `DeltaIslandRules`); as 5 Fontes do Lodo (35, `LodoFountainRules`, `f0991e3`): espiral até o Olho do Lodo, lodo preto que morde e puxa e encolhe a cada fonte purificada (3 s com [E]/Usar), fonte limpa vira zona segura por 2 min, prêmio diário de 1 Pedra da Água Viva |
| Mecânicas faltantes | Monção, escola de barco e Maré de Lodo (31); Lanterna de Fogo-Fátuo e Despertar o vizinho (33); 32 e 34 sem mecânica própria |
| Chefe do mundo | Lodo-que-Engole (35, Nv 88): puxão, mais forte à noite, vida ×1,18 (`068253c`). Só surge com as 5 Fontes acesas, e cada fonte apagada na luta o fortalece 10%. Loot próprio (Lama Limpa do Lodo) e troféu |
| Chefe de mapa | Nenhum feito. Faltam 3: A Ponte que Esqueceu (32), O Pescador de Luzes (33), Ninki Nanka (34); e o evento Maré de Lodo (31) |
| Montaria | Nenhuma da ficha: Búfalo-d'Água, Sapo-Gigante, Ninki Nanka Mansa |
| Conjunto | Nenhum |
| Móveis | Nenhum |

## Região 7 — Teto do Mundo · mapas 36–40 · nv 70–98

| Item | Estado |
|---|---|
| Cidade | Sangkent (capital), dentro do mundo |
| Mundo contínuo | Feito, ao norte da Serra (`OverworldLayouts.Teto.cs`) |
| Bichos próprios | 8: Urso-Pardo do Pamir, Iaque Selvagem, Irbis, Almas, Marmota Gigante e mais 3 (2026-11-11): Gralha-de-Bico-Vermelho (36), Abutre-Barbudo Apagado (37), Lobo de Gelo (38) (`8ad5046`). Ainda emprestados, cerca de 11 papéis (Argali, ushabtis de calha, Fenda Viva, Gralha das Neves…) |
| Mecânicas feitas | Avalanches com sino de 20 s nas 4 calhas (37, `TetoAvalancheRules`) |
| Mecânicas faltantes | Os 7 terraços (36); 38–40 sem mecânica própria. Em andamento, sem commit: carrinho e descida cronometrada pelas calhas (37, `TetoCartRules`) |
| Chefe do mundo | Aydahar (40, Nv 98): sopro em linha, vida ×1,2 (`068253c`). Loot próprio e troféu |
| Chefe de mapa | Nenhum feito. Faltam 3: O Carrinho Desgovernado (37), O Grande Almas (38), O Senhor do Cata-Vento (39); e o evento Avalanche (36) |
| Montaria | Nenhuma da ficha: Iaque do Teto do Mundo, Camelo-Bactriano Branco |
| Conjunto | Nenhum (Terraço do Pamir só na ficha) |
| Móveis | Nenhum |

## Região 8 — Planície de Mingcheng · mapas 41–46 · nv 90–140

| Item | Estado |
|---|---|
| Cidade | Mingcheng (capital) e Torres Brancas (raça), dentro do mundo |
| Mundo contínuo | Feito, ao norte do Zarafshan (`OverworldLayouts.Mingcheng.cs`). 43–46 fechados no catálogo |
| Bichos próprios | 8: Leão de Pedra, Raposa de Cauda Lanterna, Soldado de Terracota, Tartaruga de Pedra, Búfalo de Lama, Filhote de Nian, Carpa-Dragão Apagada (43) e Cavalo de Terracota (45) (`8ad5046`). Ainda emprestados, cerca de 10: Grou de Papel, Espelho Quebrado, Espantalho… |
| Mecânicas feitas | Muralha andável com faróis e corredor de luz (45); mundo do reflexo nos arrozais (44) |
| Mecânicas faltantes | 9 anéis e portões por hora (41); Relógio de Espelhos (42); 4 eclusas (43); palácios e o que cada um come (46) |
| Chefe do mundo | Quatro Gulosos (46, Nv 140): um apetite por semana e outro à noite. Taotie derrama oferendas de moedas e avisa antes de engolir; Qiongqi inverte os passos por 3 s (`HeroControlMirror`, teto de 4 s). Loot próprio (Bronze Antigo dos Vasos) e troféu |
| Chefe de mapa | Nenhum feito. Faltam 4: O Pequeno Eclipse (42), A Carpa que Não Pulou (43), O Espantalho de Palha Dourada (44), Nian, a Fera do Ano (45); e o evento Festa das Lanternas (41) |
| Montaria | Nenhuma da ficha: Cervo de Luz Branca, Cavalo de Terracota Desperto, Qilin de Jade (cosmético) |
| Conjunto | Nenhum (Vasos de Bronze, Arrozal, Torre Branca e Pátio dos Nove Anéis só nas fichas) |
| Móveis | Nenhum regional (Festa das Lanternas é sazonal) |

## Região 9 — Estepe dos Nartes · mapas 47–52 · nv 100–150

| Item | Estado |
|---|---|
| Cidade | Acampamento de Soslan, Fortim e Arvoredo (raça), dentro do mundo |
| Mundo contínuo | Feito, ao norte da Costa (`OverworldLayouts.Nartes.cs`) |
| Bichos próprios | 7: Grifo Jovem, Saiga, Lobo de Bronze, Gigante Sonâmbulo, Besouro-Casca, Ochokochi e Cisne de Feltro (48, `8ad5046`). Ainda emprestados, cerca de 15: Cervo de Ouro, Grifo Guardião, Cabra-Montês, Lince, Garanhão, lobos e águias da estepe… |
| Mecânicas feitas | Corrida de estepe (47), Ouros do Céu (48, só a Lavine levanta), subida da Corrente de Amirani (52) |
| Mecânicas faltantes | A cidade que anda (47); escala gigante e sonhos com baús (49); tempo lento (50); 51 sem mecânica própria |
| Chefe do mundo | Ohya (52, Nv 150): corrente girando. Loot próprio (Elo de Ferro Antigo de Ohya) e troféu |
| Chefe de mapa | Nenhum feito. Faltam 5: O Cavalo de Fumaça (47), O Grifo Velho do Kurgan (48), O Pesadelo da Tábua (49), A Serra de Āz (50), O Garanhão Cinzento (51) |
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
| Mecânicas faltantes | Dourado e Baile do Vale (53); pináculos em alturas de verdade (54); vento contra ×0,7 e pedras-abrigo (57); chão de nuvem e ajuda da Huma (58); pontes sem chão. Em andamento, sem commit: medidor de silêncio das Ermidas (56, `FarnkeshSilenceRules`) |
| Chefe do mundo | O Rei-Vendaval (58) |
| Chefe de mapa | Feitos 2 (`5b6dcb7`, no mundo contínuo): O Cortador de Cordas (55, Nv 160) e O Sino que Não Para (56, Nv 168, badalada com eco). Faltam 2: O Ouro de Midas (53), O Rei dos Ays (57); e o evento Vendaval (54) |
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
| Chefe de mapa | Feitos 2 (`5b6dcb7`): A Morsa-Rainha (60) e O Mamute Avô (62, Nv 214, o teto cai em cruz). Faltam 2: O Primeiro Desajeitado (61), A Aurora Presa (63); e o evento Noite Polar (59) |
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
| Chefe do mundo | O Glutão de Brasa (70), com loot próprio e troféu. Falta a montaria do chefe |
| Chefe de mapa | Feitos 2 (`5b6dcb7`): O Rei Shisa do Telhado (66, Nv 236, as duas bocas) e O Mestre das Nove Fornalhas (69, Nv 264, mais forte à noite). Faltam 2: Guayota (67), A Tartaruga-Vulcão (68); e o evento Tremor (65) |
| Montaria | Leão-Guardião de Laca (65), Cabra-dos-Pastores do Vulcão (67) |
| Conjunto | Nenhum (Kurdalægon e Palácio de Laca só nas fichas) |
| Móveis | Nenhum |

## Região 13 — Rio das Doze Horas · mapas 71–76 · nv 260–320

| Item | Estado |
|---|---|
| Cidade | Cais de Mesektet, dentro do mundo |
| Mundo contínuo | Feito, ao norte de Hinokura (`OverworldLayouts.Rio.cs`) |
| Bichos próprios | 15 (12 e mais Ganso Ladrão, Filho de Apep e Areia Movediça). Ainda emprestados, 7: Chama Faminta, Salamandra de Sombra, Escorpião [tanque] do 74, Ushabti Apagado, Ushabti Rachado, Sombra Fervida, Crocodilo de Lodo |
| Mecânicas feitas | Horas da barca com 12 píeres e hora de 3 min (71); jangadas de junco nos canais (72, `RioRaftRules`, `075b4ef`: 864 m em ~62 s em vez de ~2 km pelas estradas); anel de Mehen que gira (74); fogo que escolhe (75); defender a barca encalhada enquanto os remadores a desencalham (76, `RioBarqueDefenseRules`) |
| Mecânicas faltantes | Evento A Barca Atrasou e noite permanente (71); puxar o trenó (73) |
| Chefe do mundo | Apep (76), que recua até a noite seguinte. Loot próprio e troféu (Barca de luz vencendo Apep). Falta a Pedra da Mistura |
| Chefe de mapa | Nenhum feito. Faltam 4: O Hipopótamo-de-Junco Gigante (72), A Grande Serpente do Areal (73), O Nó Errado (74), O Guardião do Lago (75); e o evento A Barca Atrasou (71) |
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
| Chefe do mundo | O Sonho da Destruição (82). Faltam Hahya e Ohya juntos e a versão sonho do mapa |
| Chefe de mapa | Nenhum feito. Faltam 4: O Instante Perdido (78), O Rei do Piche (79), Hobabish (80), Ullikummi (81); e o evento Cheia (77) |
| Montaria | Mušḫuššu Manso (77), Alce da Taiga (80), Carneiro Gigante da Colina (82) |
| Conjunto | Tijolo Azul (2 peças, por drop) |
| Móveis | Móveis de Tijolo Azul (8 peças) |

## Região 15 — Limiar Profundo · mapas 83–90 · nv 340–440

| Item | Estado |
|---|---|
| Cidade | Fortaleza da Última Lâmpada, dentro do mundo |
| Mundo contínuo | Feito, ao norte de Farnkesh (`OverworldLayouts.Profundo.cs`) |
| Bichos próprios | 15 (12 e mais Morcego-Lâmina, Enguia Grande e Touro de Cinza). Ainda emprestados, cerca de 11: Sombra Pequena, Fumaça, Morcego da Treva, Ushabti Apagado, Fumaça Sufocante, Chama Faminta, Redemoinho, Cardume Escuro, Serpente Cega, Rastejante, Ushabti Apagado Gigante |
| Mecânicas feitas | Rede de 12 lâmpadas com a Maré de Treva (83); vento limpa a fumaça (84); chão que acende (85); limite comum de dano que nunca mata (`ProfundoHazardRules`); os 5 portões das covas na muralha da Fortaleza, que abrem ao chegar à lâmpada de cada cova (83, `ProfundoGateRules`, `ProfundoGateMechanicsDriver`). Desde `075b4ef` (`ProfundoPathsMechanicsDriver`): correntes de ar do poço (86, sobem 340–410 m em 13–16 s), comportas da coluna (87, atravessar a água funda perdendo vida ou baixar a água o dia todo pela alavanca) e a ponte do Novelo (89, 9 gaiolas acendem os lances para sempre) |
| Mecânicas faltantes | Dano no escuro (88); a raide de 5 grupos (90, precisa de servidor) |
| Chefe do mundo | A Grande Treva, versão solo (90). Falta a raide (precisa de servidor) |
| Chefe de mapa | Nenhum feito. Faltam 6, os chefes das covas e da ponte: O Rei da Fumaça (84), A Leoa do Fogo Devorador (85), A Águia de Asas de Treva (86), O Peixe de Cauda Negra (87), O Dragão do Fundo (88), O Guardião do Novelo (89); e o evento Maré de Treva (83) |
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
| Mecânicas faltantes | Pouca gravidade e Chegada da Caravana (91); lua crescendo e lanternas (93); jarras e esferas que giram (94–95); plantar luz (96); carregar lanternas entre as barcas (97); faíscas escoltadas; pilares em grupo (99, online); pose sentada e história do dia no save (100). Em andamento, sem commit: andar sobre a luz e correnteza do Rio de Luz com boias (92, `CeuRiverRules`) |
| Chefe do mundo | Āz, a Capitã Cobiçosa (96). A Arca nunca mata (tira no máximo vida − 1). Loot próprio (Semente de Luz de Āz) e troféu (Estátua de Āz devolvendo a arca). Falta a Arca Resgatada +5. Falta a revanche de Saklas e Namrael na forma do céu (95) |
| Chefe de mapa | Nenhum feito. Faltam 6: O Remador Perdido (92), O Clandestino Grande (93), O Arconte da Roda (94), A Sombra da Vela (97), O Último Arconte do Degrau (98), A Rachadura Grande (99); e o evento Chegada da Caravana (91) |
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
Lua, ficam de fora de propósito. Esse teste passa no Unity (suíte EditMode de `068253c` em diante).

---

## As 10 coisas mais importantes para jogar do começo ao fim

Revisto em 2026-11-11, no fim do dia (código até `8ad5046`). Saíram da lista, porque ficaram prontos com testes passando no
Unity:
- rodar no Unity o que faltava: a suíte EditMode inteira passou (8.146 casos em `c5b0a2a`), com loot, troféus, balanço,
  níveis das áreas e as mecânicas 24, 76 e 83;
- o balanço e a economia 100–500 medidos e aplicados (`17941ef`, `1a77156`, `28b8f39`);
- jangadas (72), correntes (86), comportas (87), ponte do Novelo (89) e as Fontes do Lodo (35);
- a primeira leva de 24 bichos próprios nas Regiões 1–9.

Subiram: os chefes de mapa (só 11 de ~68), as montarias, conjuntos e móveis das Regiões 1–9 (que estavam fora da lista) e
a PlayMode inteira, que nunca rodou de uma vez.

1. **Jogar de verdade do nível 1 ao começo da Lua** (Vila, Areias, Serra e Grutas, níveis ~1–30, com Tages, o selo e a
   primeira etapa de "A Lua que Não Enche").
   - Por quê: é o trecho que todo jogador vê, e nada do mundo contínuo, das mecânicas ou dos chefes foi visto rodando por
     uma pessoa. Bordas de área, densidade, colisão, zona segura, respawn, cais, a lamparina na Forja do Tonho, os 12 bichos
     novos das Regiões 1–4, os cards de mensagem em fila e a oferta da Vó Edna só se conferem jogando.
2. **Jogar a faixa do meio, níveis 20–150:** as 15 etapas da Lua, as Fontes do Lodo e os chefes Hedammu, Lodo-que-Engole,
   Aydahar, Quatro Gulosos e Ohya.
   - Já feito por código: o `MidStoryPlaythroughTests` joga as 15 etapas com save e load no meio e passa no Unity.
   - Falta: o ritmo ("sei para onde ir?"), as arenas, os avisos de fase, as oferendas do Taotie, os passos trocados do
     Qiongqi, a espiral das Fontes e o XP de missão novo (50% do nível) só se julgam jogando. Vale carregar um save antigo
     para ver a migração da Lua.
   - Junto: conferir o "Perigoso +30" (um par de comuns tira 101–208% da vida numa luta) e os chefes que ainda matam em 2–3
     golpes (`Docs/Balanco100a500.md` §4.5).
3. **Rodar a suíte PlayMode inteira de uma vez e fechar o que está em andamento.**
   - A PlayMode só passou em partes (fumaça das 16 regiões, história do meio). Falta a suíte inteira, protegida do falso
     vermelho pelo `PlayModeReloadLock`.
   - Em andamento, sem commit: faíscas e canais do 06, carrinho do 37, silêncio do 56 e correnteza do 92. Precisam de teste
     passando no Unity e de commit antes de entrar como feitos.
4. **Chefes de mapa das fichas: faltam 57.**
   - Por quê: cada ficha promete um chefe que "se liberta", e hoje só há 11 (Kök-Böri, Grande Ushabti, Hahya, Tages, o
     Colosso e os 6 de `5b6dcb7` nas Regiões 10–12). O sistema de `RegionBossRules.MapSpawns` já existe e é só reusar.
   - Começar pelas Regiões 1–9, onde faltam 31 e por onde passa a história do meio: Mãe-Lodo (03), Fiandeira Esquecida (04),
     Velho da Névoa (05), Polvo das Monções (25), Ninki Nanka (34), Carrinho Desgovernado (37), Nian (45), Grifo Velho (48)…
     Depois os 6 das covas do Profundo (84–89) e os 6 do Céu.
5. **Mecânicas que ainda mudam o percurso.**
   - Na estrada das Regiões 1–8: lama ×0,7 (03), teias que prendem (04), barcos do lago (19), medo do escuro (20), andar no
     fundo do recife (28), escola de barco e monção (31), 9 anéis e portões por hora (41), as 4 eclusas (43).
   - Mais tarde: túneis que mudam (62), placas que se movem (63), degraus do zigurate (77), dano no escuro (88), trenó (73),
     pouca gravidade (91) e lanternas entre as barcas (97).
   - Mapas sem mecânica própria nenhuma: 09, 10, 12, 27, 29, 30, 32, 34, 38–40 e 51.
6. **Trocar os bichos emprestados que sobram (cerca de 140 papéis).**
   - Por quê: a sensação de "região nova" depende deles. A primeira leva tirou 24; os maiores buracos agora são as Regiões
     16 (~16), 9 (~15), 5, 6, 7 e 15 (~11 cada). Seguir pela estrada da história: 9, depois 5–7.
7. **Montarias, conjuntos e móveis das Regiões 1–9.**
   - Por quê: as Regiões 5–9 não têm nenhum dos três, e as 1–4 só têm o Grifo da Coluna, o Marfim dos Artífices e os móveis
     de família. São 19 montarias propostas no `Mapas100.md` §6 (Garça do Brejo, Lobo de Guerra Ocre, Eland, Dhow-Tartaruga,
     Búfalo-d'Água, Iaque do Teto, Cervo de Luz Branca, Cavalo Dourado da Estepe…) e os conjuntos das fichas (Terraço do
     Pamir, Vasos de Bronze…).
8. **Medir o mundo de 7200 × 15600 m num celular Android, com toque de verdade.**
   - Já feito por código: o `MechanicsDriverHub`, o fim do lixo das exigências, as amostras de chão reduzidas e a auditoria
     de telas com prints em 1280×720 e 2400×1080 (`c5b0a2a`, alvos de toque de 48 px). Nada disso foi medido em aparelho.
   - O botão Usar, a janela de mapas (M), os controles invertidos do Qiongqi e o [E] das Fontes do Lodo só se conferem num
     aparelho.
   - 🙋 Depende do dono: instalar o **Android Build Support** e ter um aparelho para teste.
9. **Decisões do dono sobre o online** (`Docs/Network.md` §8, `TODO.md` 12.1):
   - campanha offline separada ou tudo online;
   - escala (MMO ou salas de 4–8);
   - os 5 primos como classes ou NPCs;
   - modelo de negócio;
   - plataformas e cross-play;
   - hospedagem, banda, DDoS e banco;
   - contas Google e Apple;
   - LGPD.

   Por quê: essas decisões definem o que é "o fim". A raide da Grande Treva, os pilares em grupo do 99, o ranking do
   carrinho do 37 e o Cavalo-Estrela só existem com servidor, e o save, a pausa e os eventos mudam de forma.
10. **Arte e áudio finais no lugar dos placeholders procedurais.**
    - Por quê: quem joga do começo ao fim vê centenas de modelos (agora 154 bichos próprios e 11 chefes de mapa) e ouve
      sons sintetizados.
    - 🙋 Depende do dono:
      - créditos do Meshy e licença comercial do plano;
      - ícones por geração de imagem;
      - música e efeitos reais (`Docs/Audio.md`);
      - rig da Maithe (Meshy ou Mixamo).

Fora da lista, ainda faltando: os eventos de cidade das fichas (Maré de Lodo, Avalanche, Festa das Lanternas, Vendaval,
Noite Polar, Tremor, A Barca Atrasou, Cheia, Maré de Treva, Chegada da Caravana), a entrada direta no Ossuário e as
montarias de chefe.

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
