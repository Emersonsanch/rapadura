# Os 100 mapas da Ilha da Aurora (e além): níveis 1–500

> **Status:** catálogo de design (2026-10-30). **No código (2026-11-11):** os **100 mapas** estão em `MapCatalog`, todos
> `isBuilt: true`, como zonas dos 16 mundos contínuos costurados num mundo só (§0.10); status por região em
> `Docs/Status_100Mapas.md`. O texto abaixo é o registro de 2026-11-03. **No código (2026-11-03):** os 42 mapas ★ estavam em `MapCatalog`
> (faixas, nível a pé/barca, exigências, cidade de retorno; conferidos contra a tabela do §1 por `MapAccessRulesTests`);
> jogáveis em mapa aberto: **01 Pomar da Vó Edna** (barca grátis) e **02 Tugai do Rio Dourado** (a pé do nível 2, barca
> do 5), pela janela M (Barcas de Luz); desde 2026-11-03 também **07, 08, 13, 14, 19 e 20** (`MapLayouts.Areias.cs`,
> `.Kaggen.cs`, `.Limiar.cs`; testes em `MapLayoutsRegionTests`), ligados a pé na ordem da campanha
> Pomar → 07 → 08 → 13 → 14 → 19 → 20. Os 4 biomas de hoje (Floresta, Deserto, Montanha, Cavernas) seguem como os mapas
> 01–02, 07–08, 13–14 e 19–20 no mapa-múndi. **Exigências especiais** (botas, lamparina, manta, asas…) já têm fonte
> real: `MapRequirementRules` lê mochila, equipamento, primo e montarias; os itens das regiões 1–4 estão à venda ou como
> recompensa na cidade da região (tabela em `Docs/MetodologiaMapasERacas.md` §2.1).
> **Pedido do usuário:** "escreva os 100 modelos de mapas no md para irmos fazendo com o tempo", com desenvolvimento
> agora **até o nível 100** (`Docs/Progressao500.md` §5). Depois: "algumas cidades baseadas nas cidades de *Avatar: A
> Lenda de Aang*" (§2) e "umas cidades baseadas nas cidades de Warcraft, com os NPCs sendo orcs, elfos e outras raças que
> tem no Dota" (§3).
> **Fontes de verdade:** tom e regras em `Docs/WorldBible_Maniqueismo.md` (§0 tom, §2 mapa, §2.5 regra de ouro, §3
> tabelas); curva, nível do bicho, respawn e montarias em `Docs/Progressao500.md`; kit 3D das cidades em
> `Docs/Cities.md` §5.
> **Ids:** nenhum id existente muda. Mapas novos usam `map_NNN_<slug>` (ex.: `map_031_jhilpur`), bichos novos
> `enemy_<região>_<slug>`, montarias novas `mount_<slug>`.

Sumário: 0. Regras de todo mapa · 1. Índice · 2. Cidades-capitais inspiradas · 3. Cidades das raças · 4. Modelo de
mapa (para copiar) · 5. Os 100 mapas, por região · 6. Montarias novas propostas · 7. Fontes e [verificar]

---

## 0. Regras de todo mapa

1. **Todo mapa tem uma cidade ou um posto avançado seguro** (regra do usuário). Cidade = zona segura completa, lojas,
   estrebaria, ferreiro, Nicho da Lâmpada, Cais da Barca. Posto = zona segura pequena (fogueira/lâmpada, um vendedor,
   conserto simples, baú de passagem), **sem respawn**.
2. **Respawn sempre na cidade** (`Progressao500.md` §7): cada mapa diz a sua **cidade de retorno**. Ninguém acorda no
   lugar onde caiu nem num posto.
3. **Inimigos só do lado mítico** (bíblia §2.5): bichos apagados, arcontes, gigantes, ushabtis, a sombra, monstros de
   contos antigos. **Nunca** divindade de fé viva, nunca o nome de um povo real, nunca um povo inteiro como vilão.
4. **Vencer é libertar a faísca** (bíblia §0.4): o bicho apagado volta a si; o chefe lembra quem era.
5. **Nível do bicho** = faixa do mapa; vida e dano pela escala do `Progressao500.md` §4. Arquétipos: **[rápido]**
   frágil e veloz, **[tanque]** lento e duro, **[enxame]** muitos fracos, **[distância]** atira de longe, **[elite]**
   raro e forte, **[controle]** lentidão/atordoa.
6. **Kit de código:** tudo é montado pelo kit modular (`TownKit`, `TownPalette` 8×8, receitas). Cada região nova pede
   **um estilo** novo no `TownStyles` e uma linha de paleta. Padrões (bordado, pintura) vão em textura de paleta,
   nunca em geometria.
7. **Tamanho:** P = ~120×120 m (um caminho e 2–3 clareiras), M = ~200×200 m, G = ~300×300 m (mapa aberto do online),
   GG = ~400×400 m (capital + arredores). Toda trilha principal ≥ 3 m; landmarks visíveis da câmera isométrica.
8. **Noite:** todo mapa tem variante noturna (`NightVariantRules`): bichos "(da noite)", um bicho só da noite e a regra
   de visão (`NightVisibilityRules`).
9. ★ = **escopo "até 100"** (construir primeiro). ☆ = depois.
10. **Mundo contínuo por região** (pedido do usuário, 2026-11-03: "o mapa inicial é muito pequeno, tem que conectar com
    outros mapas com a cidade sem ficar se teleportando, um mapa gigantesco com vários tipos de planície, bichos
    diferentes em cada área", e "pelo menos 20 vezes o tamanho"). Uma região inteira é **um mapa só**, sem troca de
    cena entre a cidade e as áreas em volta; os mapas do catálogo viram **zonas** dele. Começou pela **Região 1**:
    - **Tamanho:** 2400 × 2400 m (20× o lado do Pomar antigo, 120 m); a área inicial (Pomar 600 × 450 m + Campos do
      Aryk 600 × 250 m, nível 1–3) tem 600 × 700 m, 29× a área antiga. A Vila fica na praça (0, 0), no centro-sul-oeste.
    - **Zonas** (retângulos que cobrem a região sem sobrepor; a zona do herói sai da posição): Pomar (01, 1–3), Campos
      do Aryk (01, 2–3), Tugai do Rio Dourado (02, 2–5, o mapa feito à mão encaixado em volta do Moinho), Círculo de
      Kök-Böri (02, 4–5), Campinas do Vento Vivo (3–6), Prado das Cegonhas e o lago (4–7), Estepe de Ohrmazd (4–6, portão
      para o 07), Várzea Dourada (5–8), Colinas de Mihryazd (7–11), Altos do Mensageiro (8–12, portão para o 13),
      Ruínas do Aryk Velho (8–12), Mata das Cinco Árvores (10–15), Ravina das Bocas de Luz (11–15, portão para o 19),
      Mata Alta de Zurvan (12–16) e, no extremo leste, cada um com o seu mapa feito à mão encaixado: Brejo (03, 16–22,
      botas), Amoreiras (04, 18–24), Copas de Garajonay com a cidade dos elfos (05, 20–26) e Jardim de Mihrshah (06,
      26–32, chave). A faixa sobe com a distância da Vila (teste). Cada zona tem 2–3 grupos de bichos próprios (bichos de
      hoje: lobo, aranha, javali, escorpião, ushabti, urutau, morcegos e as espécies da noite) espalhados em ~250
      clareiras geradas, com nível que sobe dentro da zona conforme se afasta da Vila.
    - **Passagens:** zonas vizinhas são abertas. Entrar numa zona acima do nível a pé (com o bônus de terreno do herói)
      ou sem a exigência especial empurra o herói um passo para trás com uma mensagem (**barreira suave**, nunca
      teleporte). Os antigos **portões** para 07, 13 e 19 viraram passagens abertas do mundo costurado (abaixo); só o
      círculo de pedras ao lado da Vila sai do mapa (volta ao mapa da ilha).
    - **Relevo** (`OverworldRelief`, altura pura `OverworldRules.HeightAt`/`SurfaceAt`, ruído gerenciado próprio
      `OverworldNoise` — mesmo número em qualquer plataforma e no servidor): cada terreno sobe do seu jeito, misturado
      70 m entre zonas pelo mesmo ruído das bordas de cor — colinas arredondadas com fundos planos nas Colinas de
      Mihryazd (até ~16 m), platôs em degraus nos Altos do Mensageiro (até ~23 m, 3 degraus), planalto cortado por
      gargantas na Ravina (~15 m), ondulações longas na estepe e nas campinas (5–7 m), "tells" nas Ruínas, Campos e
      Pomar quase planos (≤ 3,5 m). Regras de jogo: **cidades, zonas seguras, portões e chegadas em y = 0 exato** (o kit
      da cidade assume y = 0; rampa de 40 m em volta das cidades e 90 m dos portões); **trilhas niveladas na largura**
      (o eixo segue só a parte suave do relevo, cortes e aterros de 5,5 m dos lados; ≤ 30° ao longo); **clareiras de
      bichos e marcos em platôs**; rios em vales com fundo y = 0 (7 m além da margem, encosta de 50 m); rios largos e
      lagoas (≥ 8 m) cavam leito a −1,25 m com a água a −0,3 m e vau raso (−0,5 m); pontes sobre leito cavado são
      maiores (largura + 13 m) e pousam no chão plano; fora dos muros o fundo vira morros. Malha low-poly facetada numa
      grade de 3 m alinhada ao mundo (diagonais alternadas): bordas de pedaço com os mesmos vértices, **sem costura**.
    - **Movimento na altura:** cada pedaço interno tem um `MeshCollider` com as mesmas facetas do chão (é nele que o
      herói, os bichos, o clique, o pé/IK e a câmera batem); a laje antiga virou rede de segurança a −6 m; muros da
      região de −20 a +60 m; `CityModeController.PlaceAt` usa a altura do relevo (`GroundHeightAt`, ligado pelo
      `OverworldView`) — chegada, renascer e empurrão da barreira; `MapMonsterSpawner` põe bichos e waypoints na altura
      do chão; um spawner só acorda com os pedaços sob a clareira prontos e esses pedaços ficam presos enquanto ele roda
      (`OverworldRules.ChunksUnderClearing`); colisores de árvore/pedra/cerca partem 0,4 m abaixo do chão.
    - **Chão:** cada faceta pinta a cor do terreno de um dos 3 cantos (sorteado) — bordas entre áreas pontilhadas, nunca
      retas —; tom por ruído (base/alt/acento) com pequenas manchas de um tom vizinho, faixas de altura (topos dourados
      ou nus, baixadas mais escuras), terra/rocha nas facetas íngremes, terra batida nas clareiras, areia/lama nas
      margens e no leito; trilhas drapeadas sobre as facetas com bordas irregulares (sem os discos octogonais antigos).
    - **Cenário por área** (`OverworldDecorKit`, peças mescladas no material único; peças pequenas são "carimbos"
      gravados uma vez — `TownMeshBuilder.Record/Stamp`): Campos do Aryk com fileiras de verdade por talhão (trigo,
      algodão com capulhos, parreiras em espaldeira, melancias, restolho com fardos, terra arada) e cercas vivas com
      falhas nas divisas; pomar e amoreiras em grade; tamargueiras na margem e choupos (verdes e dourados) atrás em
      todo rio; capim, flores do campo (cores por área), juncos/taboas nas margens, no brejo e na várzea, samambaias na
      mata e nas Copas, seixos, poças; troncos caídos, tocos e pedras com musgo; peça própria: marcos de pedras empilhadas
      nos Altos, colunas de basalto na Ravina, colunas quebradas nas Ruínas, ciprestes no Jardim, árvores mortas no Brejo.
    - **Streaming:** pedaços de 48 m; só os que estão a até 96 m do herói existem (somem a 136 m), no máximo um novo por
      quadro; cada pedaço = 2 malhas mescladas (chão; árvores/pedras/detalhes/marcos/pontes) no material único do kit +
      o colisor do relevo, colisores cápsula/caixa. Os spawners acordam a 85 m e dormem (bichos somem) a 125 m: os bichos
      renascem estilo MU quando o herói volta. Regras puras em `OverworldRules` (servidor autoritativo pode rodar igual).
    - **Orçamento medido** (todos os 2704 pedaços, .NET 6 desktop fora do Editor): ~5,8 mil triângulos em média por
      pedaço, p95 ~8 mil, pior 10,1 mil (meta ≤ 12 mil; o anel de fundo é o mais pesado); geração (conteúdo + 2 malhas)
      ~0,85–1,0 ms em média, p50 ~0,8–0,9 ms, p95 ~1,3–1,7 ms (antes do acabamento: ~0,6 ms com ~3,1 mil triângulos);
      o cozimento do `MeshCollider` (512 triângulos) vem à parte. O teste `OverworldReliefTests` registra os números do
      Editor no console.
    - **Fluxo:** "Mover" para qualquer mapa das Regiões 1–4 (barca, novo jogo) carrega o mundo costurado; morrer
      acorda na praça da cidade de retorno sem recarregar; a Barca de Luz leva a qualquer cais do mundo na mesma cena
      (cais da região atual só depois de pisado a pé). Código: `Assets/Scripts/Gameplay/World/Map/Overworld/` (`OverworldLayout`, `OverworldRules`,
      `OverworldLayouts.Zarafshan`, `OverworldRelief`, `OverworldTerrains`, `OverworldDecorKit`, `OverworldChunkBuilder`,
      `OverworldView`); testes `OverworldTests` e `OverworldReliefTests` (EditMode) e `OverworldWalkSmokeTests` (PlayMode). Os layouts 01–06 continuam existindo sozinhos (servidor e testes) e são
      encaixados na região como estão.
    - **Região 2 — Areias Flamejantes** (2026-11-03): mesmo modelo, 2400 × 2400 m com o Oásis de Qocho dentro, 07 e 08
      encaixados, 09–12 e 15 áreas intermediárias geradas, relevo de deserto (dunas em crista, mesas, cânions) e cenário
      próprio; o portão sul do Zarafshan (para o 07) chega ao portão norte das Areias. Detalhes em §5, "Região 2".
    - **Região 3 — Serra de Kaggen** (2026-11-03): mesmo modelo, 2400 × 2400 m com a Vila de Litema dentro, 13 e 14
      encaixados, 15–18 e 9 áreas intermediárias, relevo de montanha forte (até ~41 m no cume) com trilhas e cidades
      caminháveis; o portão oeste do Zarafshan (para o 13) chega à borda leste da Serra. Detalhes em §5, "Região 3".
    - **Região 4 — Grutas do Limiar** (2026-11-03): mesmo modelo, 2400 × 2400 m de caverna com a Vila das Lamparinas
      dentro, 19 e 20 encaixados, 21–24 e 18 salões intermediários, paredes altas de tufo entre os salões (sem teto real),
      abismos com pontes de corda, lagos, cristais, colunas e fungos que brilham; o portão oeste do Zarafshan (para o 19)
      chega à borda leste do Limiar e o do Hahya à borda oeste. Detalhes em §5, "Região 4".
    - **Região 5 — Costa das Monções** (2026-11-03): mesmo modelo, 2400 × 2400 m (`OverworldLayouts.Costa.cs`, coordenadas
      x −600…1800, z −1200…1200) com Taa Pwani na praia em (0, 0); 25–30 viram zonas (25 a 30 contam como construídos), 25 áreas
      numa grade 5 × 5, relevo de costa (falésias em degraus, ilhotas e baixios rasos, praias suaves junto da água) e cenário
      próprio; o portão oeste da Costa chega à trilha das Amoreiras na borda leste do Zarafshan. Detalhes em §5, "Região 5".
    - **Região 6 — Delta de Jhilpur** (2026-11-03): mesmo modelo, 2400 × 2400 m (`OverworldLayouts.Delta.cs`, coordenadas
      x −1200…1200, z −1800…600) com Jhilpur em (0, 0) na margem norte do seu lago; 31–35 viram zonas (construídos), 25 áreas
      numa grade 5 × 5, 12 terrenos novos, relevo próprio (ilhotas de touceira `Hummocks`, diques `Levees`, gargantas) e
      cenário próprio; preenche o canto murado a leste das Areias. Detalhes em §5, "Região 6".
    - **Região 7 — Teto do Mundo** (2026-11-03): mesmo modelo, 2400 × 2400 m (`OverworldLayouts.Teto.cs`, coordenadas
      x −1200…1200, z −600…1800) com Sangkent em (0, 0) no vale de terraços; 36–40 viram zonas (construídos), 25 áreas numa
      grade 5 × 5, 19 terrenos novos, relevo próprio (`Moraines`, `IceTongue`, `SeracField`, `Peaks`) e cenário próprio;
      costurada ao norte da Serra de Kaggen (o alto da Serra continua no Teto). Detalhes em §5, "Região 7".
    - **Região 8 — Planície de Mingcheng** (2026-11-03): mesmo modelo, 2400 × 2400 m (`OverworldLayouts.Mingcheng.cs`, coordenadas
      x −1200…1200, z −600…1800) com Mingcheng em (0, 0) na planície do rio e as Torres Brancas em (960, 600); 41 a 46 viram zonas
      (construídos), 26 áreas numa grade 5 × 5 (a Cidadela tirada da metade norte dos Picos do Nian), 15 terrenos novos, relevo próprio (`KarstTowers`, `WallRidge`) e cenário próprio;
      preenche o buraco murado ao norte do Zarafshan, entre o Teto e a Costa. Detalhes em §5, "Região 8".
    - **Região 9 — Estepe dos Nartes** (2026-11-03): mesmo modelo, 2400 × 2400 m (`OverworldLayouts.Nartes.cs`, coordenadas
      x −1200…1200, z −600…1800) com o Acampamento de Soslan em (0, 0) junto do Rio dos Nartes, o Fortim em (−480, 1080) e o
      Arvoredo em (−960, 1560); 47–52 entram no catálogo (☆) e viram zonas (construídos), 25 áreas numa grade 5 × 5, 14 terrenos
      novos, relevo próprio (`Kurgans`, `Massif`) e cenário próprio (a Corrente de Amirani deitada pela montanha); preenche o
      último buraco murado, ao norte da Costa e a leste de Mingcheng. Detalhes em §5, "Região 9".
    - **Região 10 — Picos de Farnkesh** (2026-11-06): mesmo modelo, 2400 × 2400 m (`OverworldLayouts.Farnkesh.cs`, coordenadas
      x −1200…1200, z −600…1800) com o Vale dos Sátiros em (0, 0) no Pactolo e Farnkesh em (0, 1080) sobre os pináculos; 53–58 entram
      no catálogo (☆) e viram zonas (construídos), 25 áreas numa grade 5 × 5, 14 terrenos novos, relevo próprio (`Pinnacles`,
      `CloudIsles`, `NarrowGorge`) e cenário próprio (pontes de corda deitadas nas suas linhas); o mundo cresce uma fileira ao norte
      da Estepe. Detalhes em §5, "Região 10".
    - **Região 11 — Mar Gelado** (2026-11-06): mesmo modelo, 2400 × 2400 m (`OverworldLayouts.MarGelado.cs`, coordenadas
      x −1200…1200, z −600…1800) com o Porto da Lua Branca em (0, 0) na beira da baía gelada (receita própria `IceCanal` no chão
      plano de ±30 m) e os postos de 60–64 (Casa de Neve dos Barqueiros (0, 600), Tenda de Yen (−960, 120), Boca do Gelo (960, 600),
      Farol de Gelo (480, 1080), Buraco Grande de Pesca (0, 1560)); 59–64 entram no catálogo (☆) e viram zonas (construídos), 25 áreas
      numa grade 5 × 5, 16 terrenos novos, relevo próprio (`Floes`, `Icebergs`, `PressureRidges`, `FjordWalls`, `IceVaults`) e
      cenário próprio; preenche o buraco murado ao norte de Mingcheng, a oeste dos Picos de Farnkesh. Detalhes em §5, "Região 11".
    - **Região 12 — Arquipélago de Hinokura** (2026-11-09): mesmo modelo, 2400 × 2400 m (`OverworldLayouts.Hinokura.cs`, coordenadas
      x −1680…720, z −600…1800) com Hinokura em (0, 0) na sua ilha (receita própria `LacquerPalace` no chão plano de ±30 m) e os
      postos de 66–70 (Casa de Chá de Pedra (0, 600), Abrigo dos Pastores (−960, −380), Barco-Farol (0, 1080), Portão Vermelho
      (−1330, 120), Borda Fria (−960, 1560)); 65–70 entram no catálogo (☆) e viram zonas (construídos), 25 áreas numa grade 5 × 5,
      20 terrenos novos, relevo próprio (`VolcanoCone`, `LavaFlows`, `GlassReef`, `CurvedRamparts`, `CalderaRim`) e cenário próprio;
      preenche o último buraco murado, ao norte do Teto do Mundo e a oeste do Mar Gelado. Detalhes em §5, "Região 12".
    - **Região 13 — Rio das Doze Horas** (2026-11-09): mesmo modelo, 2400 × 2400 m (`OverworldLayouts.Rio.cs`, coordenadas x −1200…1200,
      z −600…1800) com o Cais de Mesektet em (0, 0) (receita própria `PapyrusQuay` no chão plano de ±30 m) e os postos de 72–76 (Casa de
      Junco (0, 600), Trenó de Areia (−960, 120), Anel Quieto (−480, 1080), Ilha das Palmeiras (800, 1090), Proa Encalhada (0, 1560)); 71–76
      entram no catálogo (☆) e viram zonas (construídos), 25 áreas numa grade 5 × 5, 20 terrenos novos, relevo próprio (`ReedBedGrid`,
      `ZigzagDunes`, `CoilRings`, `ApepBody`) e cenário próprio; fica ao norte do Arquipélago de Hinokura e abre a quinta fileira do mundo
      (limites 7200 × 12000 m, 13 regiões; as outras duas células da fileira, ao norte do Mar Gelado e dos Picos de Farnkesh, são buracos
      murados). Detalhes em §5, "Região 13".
    - **Região 14 — Margens da Mistura** (2026-11-09): mesmo modelo, 2400 × 2400 m (`OverworldLayouts.Mistura.cs`, coordenadas x −600…1800,
      z −1200…1200) com Dur-Jarras em (0, 0) (receita própria `Ziggurat` no chão plano de ±30 m) e os postos de 78–82 (Gnômon Caído (600, 0), Balsa
      de Junco (120, −960), Clareira do Lenhador (120, 960), Ninho no Ombro (1080, 0), Tenda dos Intérpretes (1560, 0)); 77–82 entram no catálogo (☆) e viram
      zonas (construídos), 25 áreas numa grade 5 × 5, 20 terrenos novos (260–279), relevo próprio (`GnomonDial`, `CedarRings`, `GiantShoulder`, `SkinFolds`,
      `DioriteRise`) e cenário próprio; preenche o buraco murado ao norte do Mar Gelado, a leste do Rio das Doze Horas (14 regiões; resta só o buraco ao
      norte dos Picos de Farnkesh). Detalhes em §5, "Região 14".
    - **Região 15 — Limiar Profundo** (2026-11-09): mesmo modelo, 2400 × 2400 m (`OverworldLayouts.Profundo.cs`, coordenadas x −600…1800, z −1200…1200) com a Fortaleza da Última Lâmpada em (0, 0) (receita própria `BasaltFort` no chão plano de ±30 m) e os
      postos de 84–90 (Lâmpada 1 (600, 0), Lâmpada 2 (120, −960), Lâmpada 3 (120, 960), Lâmpada 4 (1080, 0), Lâmpada 5 (1080, −960), Pilar do Meio (1560, −480), Selo dos Artífices (1560, 480)); 83–90 entram no catálogo (☆) e viram zonas (construídos), 25 áreas numa grade 5 × 5,
      25 terrenos novos (280–304), relevo próprio (`SmokeCorridors`, `EmberCells`, `ShaftTerraces`, `DarkLabyrinth`, `NoveloCauseway`) e cenário próprio; preenche o último buraco murado, ao norte dos Picos de Farnkesh (x 1400…3800, z 6200…8600, deslocamento (2000, 7400), 15 regiões, mundo 7200 × 12000 m sem buracos),
      passagens `exit_tabuas_sul` e `exit_tabuas_norte` na borda oeste, levadas às trilhas leste das Margens (nv 344–360). Detalhes em §5, "Região 15".
    - **Região 16 — Reinos do Céu** (2026-11-09): mesmo modelo, 2400 × 3600 m, a maior região (`OverworldLayouts.Ceu.cs`, coordenadas x −1200…1200, z −360…3240) com o Cais da Coluna em (0, 0) (receita própria `SkyQuay` no chão plano de ±30 m) e os postos de 92–100 em fileiras para o norte; 91–100 entram no catálogo (☆) e viram zonas (construídos), 35 áreas numa grade 5 × 7,
      35 terrenos novos (310–344), relevo próprio (`DeckTiers`, `WheelRim`, `OrbitRidges`, `ColumnRings`, `SkyFissures`) e cenário próprio; abre a **sexta fileira** ao norte do Limiar Profundo, só na coluna leste (x 1400…3800, z 8600…12200, deslocamento (2600, 8960), Cais no mundo em (2600, 8960), 16 regiões, mundo 7200 × 15600 m),
      as duas células a oeste (ao norte do Rio e das Margens) ficam `Holes` murados; passagens `exit_portao_oeste` e `exit_portao_leste` na borda sul, levadas ao fim da última estrada do Limiar (uma de cada lado do Portão da Terra da Treva, nv 432–440). Detalhes em §5, "Região 16".
    - **Mundo costurado** (2026-11-03, pedido do usuário: andar entre as regiões sem portão): as quatro primeiras regiões
      formam um quadrado de 4800 × 4800 m (`OverworldLayouts.WorldId`, `OverworldLayouts.World.cs`), 2 × 2, e a **Costa das
      Monções** fica a leste do Zarafshan (x 1400…3800, z −1000…1400, deslocamento (2000, 200)) e o **Delta de Jhilpur** a
      leste das Areias, ao sul da Costa (x 1400…3800, z −3400…−1000, deslocamento (2600, −1600), Jhilpur no mundo em
      (2600, −1600)): o mundo é um **retângulo inteiro** de 7200 × 4800 m, 3 × 2 regiões. (Entre a Costa e o Delta o canto era
      um **buraco murado** — `OverworldLayout.Holes`/`WallDistance` continuam valendo para qualquer canto sem região: muros
      invisíveis por dentro, relevo de fundo, `OverworldRules.Validate` conta a área sem o buraco; hoje não há nenhum.) A
      passagem Zarafshan ↔ Costa: o portão oeste da Costa (Mata da Restinga) é levado à trilha das Amoreiras (x 1380).
      Passagens do Delta: o antigo portão leste das Areias (`exit_rondaveis`, que fechava no muro) encontra o portão oeste do
      Delta (`exit_areias`) na mesma borda (z −1250) e as chegadas viram uma trilha; os dois portões norte do Delta
      (`exit_costa`, x 1940, e `exit_liyongo`, x 3540) são levados às pontas de trilha da Costa logo ao norte (a estrada das
      Falésias Vermelhas e a trilha dos Mangues de Liyongo). O **Teto do Mundo** fica ao norte da Serra de Kaggen (x −3400…−1000,
      z 1400…3800, deslocamento (−2200, 2000), Sangkent no mundo em (−2200, 2000)): os limites do mundo passam a 7200 × 7200 m e os
      cantos ao norte do Zarafshan e da Costa, que nenhuma região cobre, são **buracos murados** (relevo de fundo, muros
      invisíveis). Passagens do Teto: os dois portões da borda sul (`exit_pedra_seca`, x −2350, e `exit_abrigos`, x −1080) são
      levados à trilha mais próxima da Serra logo ao sul (o caminho entre o Recinto Curvo e o Abrigo do Eland, e a ponta de trilha
      norte das Encostas dos Abrigos). A **Planície de Mingcheng** preenche o buraco ao norte do Zarafshan (x −1000…1400,
      z 1400…3800, deslocamento (200, 2000), Mingcheng no mundo em (200, 2000)): sobrava um só buraco murado, ao norte da Costa. O Rio
      Dourado nasce nela e chega à borda sul no mesmo ponto (520, 1400) onde o do Zarafshan começa. Passagens: os dois portões da
      borda sul (`exit_pomar`, x 0, e `exit_tugai`, x 1000) são levados à trilha mais próxima do Zarafshan (a estrada das Ruínas do
      Aryk Velho e a da Mata Alta de Zurvan) e os dois da borda oeste (`exit_calhas`, z 2100, e `exit_forte_ventos`, z 2620) à do
      Teto (a Estação da Calha Alta e a encruzilhada dos cata-ventos). A **Estepe dos Nartes** preenche esse último buraco
      (x 1400…3800, z 1400…3800, deslocamento (2600, 2000), o Acampamento de Soslan no mundo em (2600, 2000)): o mundo vira um
      **quadrado inteiro** de 7200 × 7200 m, 3 × 3 regiões, **sem buraco** (`Holes` vazio). Passagens: os três portões da borda sul
      (`exit_auroques`, x 1700; `exit_meroe`, x 2750; `exit_husuni`, x 3500) são levados à trilha mais próxima da Costa logo ao sul
      (a estrada de Aratta, a do Pátio dos Relevos e a ponta das falésias de Husuni) e os três da borda oeste (`exit_loess`, z 1850;
      `exit_torres`, z 2700; `exit_nian`, z 3420) à de Mingcheng (a trilha das Ravinas de Loess, a das Colinas dos Espelhos e a dos
      Picos do Nian). Rota a pé Vila → Acampamento de Soslan: ~4,0 km pela Costa. Os **Picos de Farnkesh** abrem uma quarta fileira
      ao norte da Estepe (x 1400…3800, z 3800…6200, deslocamento (2600, 4400), o Vale dos Sátiros no mundo em (2600, 4400), Farnkesh
      em (2600, 5480)): os limites passam a **7200 × 9600 m** e as duas células dessa fileira sem região, ao norte do Teto e de
      Mingcheng, voltam a ser **buracos murados** (`Holes` = 2; o lado oeste dos picos é a queda nas nuvens). Passagens: os dois
      portões da borda sul (`exit_degelo`, x 2300, pelo Lago de Degelo do 50, sem exigência; `exit_ohya`, x 3560, pelo Cume de Ohya
      do 52) são levados à trilha mais próxima da Estepe. O Pactolo chega à borda sul em (2880, 3800), onde nasce o Rio dos Nartes.
      Rota a pé Vila → Vale dos Sátiros ~6,7 km, → Farnkesh ~7,7 km (rampa mais íngreme 18°; o fim da estrada norte da Estepe
      na Torre de Vigia foi desviado pelo leste, onde a chegada pelo sul tinha 30°). O **Mar Gelado** preenche a célula da quarta
      fileira ao norte de Mingcheng e a oeste dos picos (x −1000…1400, z 3800…6200, deslocamento (200, 4400), o Porto da Lua Branca
      no mundo em (200, 4400)): escolhida porque é vizinha de Farnkesh (a região anterior, nv até 200) e de Mingcheng, cuja estrada
      norte já terminava atravessando a muralha a 40 m da borda; fica só **um buraco murado**, ao norte do Teto (`Holes` = 1).
      Passagens: `exit_muralha` (x 240, borda sul) levado ao fim da estrada norte de Mingcheng; `exit_pe_dos_picos` (z 3980) e
      `exit_pinaculos` (z 4960), na borda leste, levados às trilhas oeste dos picos (Bosque do Pé dos Picos e Bosque de Pináculos).
      Rota a pé Vila → Porto da Lua Branca ~4,6 km (por Mingcheng, rampa mais íngreme 17°); a rota até Farnkesh passa a ~7,4 km
      (pelo Mar Gelado, rampa 26°). O **Arquipélago de Hinokura** preenche a última célula (x −3400…−1000, z 3800…6200, deslocamento
      (−1720, 4400), Hinokura no mundo em (−1720, 4400)): o mar gelado degela para oeste sobre os vulcões e as geleiras do Teto descem
      até as praias negras; o mundo vira um **retângulo inteiro de 7200 × 9600 m, 12 regiões, nenhum buraco** (`Holes` = 0).
      Passagens: `exit_wakhan` (x −1640) e `exit_cristas` (x −1200), na borda sul, levados aos fins das trilhas norte do Teto;
      `exit_tundra` (z 4700) e `exit_lagos` (z 5000), na borda leste, levados às trilhas oeste do Mar Gelado pelo Estreito do Degelo.
      Rota a pé Vila → Hinokura ~5,9 km (rampa mais íngreme 25°). O **Rio das Doze Horas** abre a quinta fileira ao norte de Hinokura
      (x −3400…−1000, z 6200…8600, deslocamento (−2200, 6800), Cais de Mesektet no mundo em (−2200, 6800)): o único vizinho é o arquipélago, cuja
      cinza corre para o rio da noite; as outras duas células da fileira ficam **muradas** (`Holes` = 2) e o mundo vira **7200 × 12000 m, 13
      regiões**. Passagens: `exit_campos_de_cinza` (x −2320) e `exit_mar_de_cinza` (x −1720), na borda sul do rio, levadas às trilhas norte de
      Hinokura (rota a pé Vila → Cais ~8,5 km). As **Margens da Mistura** preenchem a célula ao norte do Mar Gelado, a leste do rio (x −1000…1400,
      z 6200…8600, deslocamento (−400, 7400), Dur-Jarras no mundo em (−400, 7400); `Holes` = 1, só ao norte dos Picos de Farnkesh; **14 regiões**): passagens
      `exit_beira_fervente_sul` (z 7300) e `exit_beira_fervente_norte` (z 7560), na borda oeste das Margens, levadas à trilha leste do rio (Beira Fervente, nv 292–298,
      onde o rio já chega a nv ~300); a borda com o Mar Gelado (nv 180–240) fica sem passagem, mas é relevo contínuo (a diferença de nível é da barreira suave). O **Limiar Profundo** preenche a última célula, ao norte dos Picos de Farnkesh (`Holes` = 0, **15 regiões**, nenhum buraco): passagens `exit_tabuas_sul` (z 7300) e `exit_tabuas_norte` (z 7560), na borda oeste do Limiar Profundo, levadas à trilha da Tenda dos Intérpretes das Margens (nv 344–360, onde o Limiar começa a 340); a borda sul com Farnkesh (nv ~200) fica sem passagem, relevo contínuo. Quadrado original:
      Serra de Kaggen (x −3400…−1000, z −1000…1400) | Bosque do Zarafshan (x −1000…1400, z −1000…1400) em cima;
      Grutas do Limiar (x −3400…−1000, z −3400…−1000) | Areias Flamejantes (x −1000…1400, z −3400…−1000) embaixo. Os **Reinos do Céu** abrem a sexta fileira, ao norte do Limiar Profundo (`Holes` = 2, as células ao norte do Rio e das Margens; **16 regiões**): passagens `exit_portao_oeste` (x 3380) e `exit_portao_leste` (x 3740), na borda sul, levadas ao fim da trilha do Portão da Terra da Treva, uma de cada lado do portão.
      Cada região guarda o seu layout nas suas coordenadas e é deslocada por um número inteiro de pedaços (Zarafshan
      (0, 0) — a Vila continua na origem —, Areias (200, −1500), Kaggen (−1700, −100), Limiar (−2200, −1300)): as grades
      de pedaço (48 m) e de faceta (3 m) casam, cada ponto pertence a uma só região (retângulos semiabertos).
      - **Passagens:** cada portão entre duas regiões vira passagem aberta: o chão plano em y = 0 fica (núcleo + rampa de
        90 m dos dois lados), os dois portões que se olham na mesma borda ganham uma trilha ligando as chegadas
        (Zarafshan sul ↔ Areias norte, a antiga estrada para o 07; Zarafshan oeste ↔ Kaggen leste, o antigo caminho para
        o 13), e um portão sozinho numa borda é levado à trilha mais próxima do outro lado sem vau nem cidade no meio
        (Zarafshan → Passo do Hahya pelo antigo portão do 19; Kaggen → Campinas do Vento Vivo pelos Altos; Kaggen sul → galeria da Ravina do
        Limiar, antigo portão para o 08; Limiar leste → Erg Dourado das Areias, antigo portão do Zarafshan). Clareiras na
        trilha nova ou a menos de 30 m da passagem saem. Portões cuja região agora está no mundo mas que dão na borda de
        fora (Areias → Rondavéis, Limiar → Hahya) só fecham (a trilha termina no muro). Ficam: os quatro anéis de pedras
        (`exit_island`, `exit_island_areias`, `_kaggen`, `_limiar`), o portal do Ossuário (`exit_ossuario`) e os cais.
      - **Relevo:** um relevo só para o mundo; cada área tem o formato calculado nas coordenadas da sua região
        (`OverworldZone.Origin`), então longe das bordas cada região é idêntica a ela sozinha (teste no Zarafshan), e entre
        regiões as áreas se misturam nos mesmos 70 m de qualquer borda de área: **sem degrau nem buraco** na costura
        (teste: < 0,1 m de diferença em 4 cm ao longo das duas bordas inteiras; pedaços vizinhos com os mesmos vértices).
        A Grutas do Limiar ganha a abóbada de tufo em volta do seu retângulo, aberta nas passagens (entrada da caverna).
      - **Região atual:** a do retângulo sob o herói (`OverworldLayout.PartAt`/`RegionAt`/`VilaAt`/`BiomeAt`): ao cruzar,
        clima e ambiente trocam de bioma e a ficha da ilha muda de região (`OverworldView.RegionChanged`); o mapa (zona)
        segue a área como antes; morrer acorda na cidade de retorno do mapa, senão na vila da região atual. Barreira
        suave, níveis e exigências continuam por área.
      - **Área atual** (2026-11-03): `OverworldAreaTracker` (puro, servidor pode rodar por jogador) dá a área, a região e
        a cidade sob o herói com **histerese** de 6 m (só troca 6 m depois da borda; entrar numa cidade é na borda, sair
        exige os 6 m), e solta o evento puro `AreaChangedEvent` (anterior → nova, região, cidade, `Teleported` após
        chegada/barca/respawn), publicado no `EventBus` pelo `OverworldView` (`AreaTracker`). A barreira suave continua
        com a zona exata. Consumidores: o **letreiro estilo MU** (`UI/HUD/AreaBannerController`, `Resources/UI/AreaBanner.uss`
        com o tema): região, nome da área (ou da cidade ao entrar nela), "Nv 12–16" (ou "Área segura") e aviso em
        laranja se falta o item especial da área (`MapRequirementHeroState`) ou se os bichos estão muito acima do nível;
        aparece no alto/centro com fade por ~3 s, some na pausa, sem fade com "Reduzir Movimento"; e um rótulo pequeno
        permanente com a área atual sob os botões do canto superior direito do HUD. **Música e clima por área**
        (`Audio/AreaAmbienceDirector`): cada terreno tem clima (`OverworldAreaAmbience`: névoa no brejo, mata fechada,
        ravina e torrentes; poeira (tempestade de areia) no erg, Vale das Ossadas, dunas que cantam e ermos; neve no passo
        alto, campos de neve, Estela e cume; umidade (névoa) nos lagos, salões, abismos e ninho das grutas; céu limpo nos
        oásis, salinas, cristais e no mercado; o resto deixa o bioma sortear), aplicado no `WeatherSystem` com a transição
        dele (visuais de clima e `AmbientVfx` seguem); a música sai do humor (cidade × campo × ermo × perigo, dia/noite,
        `Docs/Audio.md` §3; no Teto, neve na geleira, nos seracs, nas cristas, no passo e nos picos, névoa no degelo, no lago e nos
        penhascos, céu limpo na estepe alta e no vale do Wakhan). Testes `OverworldAreaTests` e a caminhada `OverworldWalkSmokeTests` (evento ao entrar nos
        Campos e nas Areias, nada ao ser empurrado na borda do Tugai).
      - **Performance:** nada é carregado inteiro: os pedaços continuam por distância (≤ 96 m), e as cidades agora também
        (constroem a 170 m do chão da cidade, uma por vez; somem a 260 m) — o mundo tem 25. Para os pedaços não pagarem
        pelo mundo 4× maior, trilhas, clareiras e marcos ficam indexados por pedaço, as áreas por balde do relevo e os
        pontos de chegada/cais só valem perto do pedaço: geração por pedaço medida (.NET, fora do Editor) igual ou
        melhor que a da região sozinha (Zarafshan 0,62 ms × 0,67 ms; Areias 0,46 × 0,43; Kaggen 0,59 × 0,58; Limiar
        0,44 × 0,45), triângulos iguais; montar o mundo (4 regiões + costura) ~0,45 s, uma vez por sessão. Coordenadas até
        ±3400 m: precisão de float sobra (sem recentrar).
      - `OverworldLayouts.ForMap` continua dando a região sozinha (servidor, catálogo, testes das regiões); o modo mapa usa
        `OverworldLayouts.WorldOf`. Testes `OverworldWorldTests` (EditMode) e a caminhada `OverworldWalkSmokeTests`
        (PlayMode), que agora segue pela passagem da Estepe até a Estrada das Caravanas sem carregar cena.
    - **Viagem e save dos mapas-zona** (2026-11-03): 09–12, 15–18 e 21–24 não têm layout próprio, só existem como zonas
      do mundo da região, e agora contam como construídos (`MapCatalog` `isBuilt`; `MapLayouts.IsPlayable` = layout próprio
      **ou** zona de overworld, `MapLayouts.IsOverworldOnly`; o teste de catálogo exige `isBuilt ⇔ IsPlayable`). A janela M
      (Barcas de Luz) leva até eles: carrega o mundo da região e põe o herói no Cais da Barca do posto/cidade do mapa
      (`OverworldLayout.ArrivalFor`, `MapModeController.ArrivalPoint`). Regra dos cais: de fora da região, qualquer mapa
      construído (nível, exigência e ouro como sempre); dentro da região, só cais já pisados a pé — a linha aparece com
      cadeado e "Ande até esse cais uma vez antes de pedir a barca" (`MapAccessCheck.DockNotVisited`). Os cais visitados
      (`OverworldProgress`) são salvos com as cidades (`CitySaveState.visitedDocks`, `docksSchema` = 1) e zerados em jogo
      novo; save antigo (sem o campo) ganha o cais da Vila de cada região cuja cidade já tinha sido visitada (01, 07, 13,
      19). O portão gradeado do **Ossuário dos Artífices** no Limiar entra direto na masmorra (ver `Docs/Dungeons.md` §2).
      Testes: `OverworldTravelTests`.

---

## 1. Índice

| # | Mapa (pt) | *en* | Região | Nível | Cidade / posto | Cidade de retorno | Escopo |
|---|---|---|---|---|---|---|---|
| 01 | Pomar da Vó Edna | *Grandma Edna's Orchard* | Bosque do Zarafshan | 1–3 | **cidade:** Vila da Vó Edna | Vila da Vó Edna | ★ |
| 02 | Tugai do Rio Dourado | *Golden River Tugai* | Bosque do Zarafshan | 2–5 | posto: Moinho do Pomar | Vila da Vó Edna | ★ |
| 03 | Brejo do Lodo Escuro | *Dark Mire Marsh* | Bosque do Zarafshan | 16–22 | posto: Ponte do Aryk | Vila da Vó Edna | ★ |
| 04 | Bosque das Amoreiras de Seda | *Silk Mulberry Grove* | Bosque do Zarafshan | 18–24 | posto: Casa do Tear | Vila da Vó Edna | ★ |
| 05 | Copas de Garajonay | *Garajonay Canopy* | Bosque do Zarafshan | 20–26 | **cidade de raça:** elfos-da-mata | Copas de Garajonay | ★ |
| 06 | Jardim Esquecido de Mihrshah | *Mihrshah's Forgotten Garden* | Bosque do Zarafshan | 26–32 | posto: Portão do Jardim | Copas de Garajonay | ★ |
| 07 | Estrada das Caravanas | *Caravan Road* | Areias Flamejantes | 5–7 | posto: Caravançará do Ammo | Oásis de Qocho | ★ |
| 08 | Ruínas dos Artífices | *Artificers' Ruins* | Areias Flamejantes | 6–9 | posto: Acampamento das Pirâmides | Oásis de Qocho | ★ |
| 09 | Galerias do Karez | *Karez Galleries* | Areias Flamejantes | 20–26 | posto: Boca do Karez | Oásis de Qocho | ★ |
| 10 | Dunas que Cantam | *Singing Dunes* | Areias Flamejantes | 22–28 | posto: Poço das Tamareiras | Oásis de Qocho | ★ |
| 11 | Ermos do Tambor Rubro | *Red Drum Badlands* | Areias Flamejantes | 24–32 | **cidade de raça:** orcs | Tambor Rubro | ★ |
| 12 | Trono de Namrael | *Namrael's Throne* | Areias Flamejantes | 30–38 | posto: Torre de Taipa | Tambor Rubro | ★ |
| 13 | Trilha dos Rondavéis | *Rondavel Trail* | Serra de Kaggen | 9–11 | **cidade:** Vila de Litema | Vila de Litema | ★ |
| 14 | Passo do Hahya | *Hahya's Pass* | Serra de Kaggen | 10–13 | posto: Boca da Mina do Mihr | Vila de Litema | ★ |
| 15 | Abrigos Pintados | *Painted Shelters* | Serra de Kaggen | 26–32 | posto: Abrigo do Eland | Vila de Litema | ★ |
| 16 | Muralhas de Pedra Seca | *Drystone Walls* | Serra de Kaggen | 28–34 | posto: Recinto Curvo | Vila de Litema | ★ |
| 17 | Salões de Bronze de Napirasu | *Napirasu's Bronze Halls* | Serra de Kaggen | 30–38 | **cidade de raça:** anões | Salões de Bronze | ★ |
| 18 | Cume dos Mazans | *Mazan Summit* | Serra de Kaggen | 36–44 | posto: Estela Alta | Salões de Bronze | ★ |
| 19 | Lago do Limiar | *Threshold Lake* | Grutas do Limiar | 13–15 | **cidade:** Vila das Lamparinas | Vila das Lamparinas | ★ |
| 20 | Salão de Tages | *Tages' Hall* | Grutas do Limiar | 14–18 | posto: Nicho da Ponte | Vila das Lamparinas | ★ |
| 21 | Campo das Pedras-Dragão | *Dragonstone Field* | Grutas do Limiar | 32–38 | posto: Vishapakar Grande | Vila das Lamparinas | ★ |
| 22 | Mercado dos Túneis | *Tunnel Market* | Grutas do Limiar | 34–42 | **cidade de raça:** kobolds | Mercado dos Túneis | ★ |
| 23 | Cânion da Roda | *Wheel Canyon* | Grutas do Limiar | 38–46 | posto: Torre das Jarras | Mercado dos Túneis | ★ |
| 24 | Ninho de Mahaway | *Mahaway's Roost* | Grutas do Limiar | 42–50 | posto: Última Ponte de Corda | Vila das Lamparinas | ★ |
| 25 | Taa Pwani e o Recife | *Taa Pwani and the Reef* | Costa das Monções | 45–50 | **cidade:** Taa Pwani | Taa Pwani | ★ |
| 26 | Savana dos Auroques | *Aurochs Savanna* | Costa das Monções | 48–55 | **cidade de raça:** povo-touro (Aratta) | Aratta | ★ |
| 27 | Pirâmides Altas de Meroé | *High Pyramids of Meroë* | Costa das Monções | 52–58 | posto: Pátio dos Relevos | Aratta | ★ |
| 28 | Cidade-Recife de Kirta | *Kirta Reef City* | Costa das Monções | 55–62 | **cidade de raça:** povo-do-mar | Kirta | ★ |
| 29 | Kilwa Afundada | *Sunken Kilwa* | Costa das Monções | 60–66 | posto: Farol de Coral | Kirta | ★ |
| 30 | Mangues de Liyongo | *Liyongo Mangroves* | Costa das Monções | 64–72 | posto: Cais das Dhows | Taa Pwani | ★ |
| 31 | Jhilpur, a Cidade nas Raízes | *Jhilpur, City in the Roots* | Delta de Jhilpur | 60–66 | **capital** (Água Viva) | Jhilpur | ★ |
| 32 | Pontes de Raiz Viva | *Living Root Bridges* | Delta de Jhilpur | 64–70 | posto: Plataforma das Garças | Jhilpur | ★ |
| 33 | Palafitas de Tarchon | *Tarchon Stilts* | Delta de Jhilpur | 70–78 | **cidade de raça:** trols | Palafitas de Tarchon | ★ |
| 34 | Charco do Ninki Nanka | *Ninki Nanka Bog* | Delta de Jhilpur | 76–82 | posto: Ilhota das Lanternas | Palafitas de Tarchon | ★ |
| 35 | Coração do Lodo | *Heart of the Mire* | Delta de Jhilpur | 82–88 | posto: Raiz-Mãe | Jhilpur | ★ |
| 36 | Sangkent, a Cidade-Escadaria | *Sangkent, the Stair City* | Teto do Mundo | 70–76 | **capital** (as cinco luzes) | Sangkent | ★ |
| 37 | Calhas de Pedra | *Stone Chutes* | Teto do Mundo | 74–80 | posto: Estação da Calha Alta | Sangkent | ★ |
| 38 | Geleira do Teto do Mundo | *Roof-of-the-World Glacier* | Teto do Mundo | 80–86 | posto: Cabana do Iaque | Sangkent | ★ |
| 39 | Forte dos Ventos | *Fort of the Winds* | Teto do Mundo | 84–92 | posto: o próprio forte | Sangkent | ★ |
| 40 | Passo de Wakhan | *Wakhan Pass* | Teto do Mundo | 90–98 | posto: Caravançará de Pedra | Sangkent | ★ |
| 41 | Mingcheng, a Cidade dos Nove Anéis | *Mingcheng, City of Nine Rings* | Planície de Mingcheng | 90–96 | **capital** (Luz) | Mingcheng | ★ |
| 42 | Torres Brancas de Amanirenas | *White Spires of Amanirenas* | Planície de Mingcheng | 95–102 | **cidade de raça:** altos-elfos | Torres Brancas | ★ |
| 43 | Canal das Lanternas | *Lantern Canal* | Planície de Mingcheng | 102–110 | posto: Eclusa da Lua | Mingcheng | ☆ |
| 44 | Arrozais Espelhados | *Mirror Rice Terraces* | Planície de Mingcheng | 108–116 | posto: Moinho d'Água | Mingcheng | ☆ |
| 45 | Muralha das Mil Torres | *Wall of a Thousand Towers* | Planície de Mingcheng | 115–125 | posto: Torre Quinhentos | Torres Brancas | ☆ |
| 46 | Cidadela dos Quatro Gulosos | *Citadel of the Four Gluttons* | Planície de Mingcheng | 125–140 | posto: Portão do Nono Anel | Mingcheng | ☆ |
| 47 | Acampamento de Soslan | *Soslan's Camp* | Estepe dos Nartes | 100–108 | **cidade de raça:** centauros | Acampamento de Soslan | ☆ |
| 48 | Kurgans de Ouro | *Golden Kurgans* | Estepe dos Nartes | 106–114 | posto: Kurgan Aberto | Acampamento de Soslan | ☆ |
| 49 | Fortim dos Gigantes-da-Colina | *Hill-Giant Fort* | Estepe dos Nartes | 112–120 | **cidade de raça:** gigantes-da-colina | Fortim | ☆ |
| 50 | Arvoredo de Haripushpa | *Haripushpa Grove* | Estepe dos Nartes | 118–126 | **cidade de raça:** povo-tronco | Arvoredo | ☆ |
| 51 | Pradaria dos Mil Cavalos | *Thousand-Horse Prairie* | Estepe dos Nartes | 124–134 | posto: Poço da Taça | Acampamento de Soslan | ☆ |
| 52 | Corrente de Amirani | *Amirani's Chain* | Estepe dos Nartes | 134–150 | posto: Torre de Vigia | Fortim | ☆ |
| 53 | Vale dos Sátiros | *Satyr Vale* | Picos de Farnkesh | 140–148 | **cidade de raça:** sátiros | Vale dos Sátiros | ☆ |
| 54 | Farnkesh, os Ninhos de Pedra | *Farnkesh, the Stone Nests* | Picos de Farnkesh | 145–152 | **capital** (Vento Vivo) | Farnkesh | ☆ |
| 55 | Pontes de Vento | *Wind Bridges* | Picos de Farnkesh | 150–160 | posto: Ermida Baixa | Farnkesh | ☆ |
| 56 | Ermidas Suspensas | *Hanging Hermitages* | Picos de Farnkesh | 158–168 | posto: Pináculo do Sino | Farnkesh | ☆ |
| 57 | Garganta dos Ays | *Gorge of the Ays* | Picos de Farnkesh | 166–180 | posto: Gruta Abrigo | Farnkesh | ☆ |
| 58 | Ninho da Huma | *Huma's Nest* | Picos de Farnkesh | 180–200 | posto: Último Pináculo | Farnkesh | ☆ |
| 59 | Porto da Lua Branca | *White Moon Harbor* | Mar Gelado | 180–188 | **capital** (Ar Sereno) | Porto da Lua Branca | ☆ |
| 60 | Canais de Gelo | *Ice Canals* | Mar Gelado | 186–196 | posto: Casa de Neve dos Barqueiros | Porto da Lua Branca | ☆ |
| 61 | Tundra de Omöl | *Omöl's Tundra* | Mar Gelado | 194–204 | posto: Tenda de Yen | Porto da Lua Branca | ☆ |
| 62 | Galerias dos Mamutes | *Mammoth Galleries* | Mar Gelado | 202–214 | posto: Boca do Gelo | Porto da Lua Branca | ☆ |
| 63 | Mar das Auroras | *Aurora Sea* | Mar Gelado | 212–226 | posto: Farol de Gelo | Porto da Lua Branca | ☆ |
| 64 | Fundo do Velho Kul | *Old Kul's Deep* | Mar Gelado | 226–240 | posto: Buraco Grande de Pesca | Porto da Lua Branca | ☆ |
| 65 | Hinokura, a Ilha-Forja | *Hinokura, the Forge Isle* | Arquipélago de Hinokura | 220–228 | **capital** (Fogo Vivo) | Hinokura | ☆ |
| 66 | Jardins de Lava Laqueada | *Lacquered Lava Gardens* | Arquipélago de Hinokura | 226–236 | posto: Casa de Chá de Pedra | Hinokura | ☆ |
| 67 | Monte Echeyde | *Mount Echeyde* | Arquipélago de Hinokura | 234–246 | posto: Abrigo dos Pastores | Hinokura | ☆ |
| 68 | Recife de Obsidiana | *Obsidian Reef* | Arquipélago de Hinokura | 244–254 | posto: Barco-Farol | Hinokura | ☆ |
| 69 | Castelo das Nove Fornalhas | *Castle of Nine Furnaces* | Arquipélago de Hinokura | 252–264 | posto: Portão Vermelho | Hinokura | ☆ |
| 70 | Caldeira do Fogo Devorador | *Devouring Fire Caldera* | Arquipélago de Hinokura | 264–280 | posto: Borda Fria | Hinokura | ☆ |
| 71 | Cais de Mesektet | *Mesektet Quay* | Rio das Doze Horas | 260–268 | **cidade:** Cais de Mesektet | Cais de Mesektet | ☆ |
| 72 | Campos de Juncos | *Reed Fields* | Rio das Doze Horas | 266–276 | posto: Casa de Junco | Cais de Mesektet | ☆ |
| 73 | Areal Escondido | *Hidden Sands* | Rio das Doze Horas | 274–286 | posto: Trenó de Areia | Cais de Mesektet | ☆ |
| 74 | Anéis de Mehen | *Coils of Mehen* | Rio das Doze Horas | 284–296 | posto: Anel Quieto | Cais de Mesektet | ☆ |
| 75 | Lago de Fogo | *Lake of Fire* | Rio das Doze Horas | 292–306 | posto: Ilha das Palmeiras | Cais de Mesektet | ☆ |
| 76 | Banco de Areia de Apep | *Apep's Sandbank* | Rio das Doze Horas | 304–320 | posto: Proa Encalhada | Cais de Mesektet | ☆ |
| 77 | Dur-Jarras, a Cidade-Zigurate | *Dur-Jarras, the Ziggurat City* | Margens da Mistura | 300–308 | **cidade:** Dur-Jarras | Dur-Jarras | ☆ |
| 78 | Deserto do Relógio | *Desert of the Clock* | Margens da Mistura | 306–316 | posto: Gnômon Caído | Dur-Jarras | ☆ |
| 79 | Poços de Betume | *Bitumen Wells* | Margens da Mistura | 314–326 | posto: Balsa de Junco | Dur-Jarras | ☆ |
| 80 | Cedral de Hobabish | *Hobabish Cedar Forest* | Margens da Mistura | 322–336 | posto: Clareira do Lenhador | Dur-Jarras | ☆ |
| 81 | Ombro de Upelluri | *Upelluri's Shoulder* | Margens da Mistura | 334–346 | posto: Ninho no Ombro | Dur-Jarras | ☆ |
| 82 | Campo das Tábuas do Sonho | *Field of the Dream Tablets* | Margens da Mistura | 344–360 | posto: Tenda dos Intérpretes | Dur-Jarras | ☆ |
| 83 | Fortaleza da Última Lâmpada | *Fortress of the Last Lamp* | Limiar Profundo | 340–350 | **cidade:** Fortaleza | Fortaleza da Última Lâmpada | ☆ |
| 84 | Cova da Fumaça | *Pit of Smoke* | Limiar Profundo | 348–362 | posto: Lâmpada 1 | Fortaleza da Última Lâmpada | ☆ |
| 85 | Cova do Fogo Devorador | *Pit of Devouring Fire* | Limiar Profundo | 360–374 | posto: Lâmpada 2 | Fortaleza da Última Lâmpada | ☆ |
| 86 | Cova do Vento Destruidor | *Pit of Ruinous Wind* | Limiar Profundo | 372–388 | posto: Lâmpada 3 | Fortaleza da Última Lâmpada | ☆ |
| 87 | Cova do Lodo | *Pit of Mire* | Limiar Profundo | 386–402 | posto: Lâmpada 4 | Fortaleza da Última Lâmpada | ☆ |
| 88 | Cova da Treva | *Pit of Darkness* | Limiar Profundo | 400–416 | posto: Lâmpada 5 | Fortaleza da Última Lâmpada | ☆ |
| 89 | Ponte do Novelo | *Bridge of the Bōlos* | Limiar Profundo | 412–428 | posto: Pilar do Meio | Fortaleza da Última Lâmpada | ☆ |
| 90 | Portão da Terra da Treva | *Gate of the Land of Darkness* | Limiar Profundo | 424–440 | posto: Selo dos Artífices | Fortaleza da Última Lâmpada | ☆ |
| 91 | Cais da Coluna | *Column Quay* | Reinos do Céu | 420–430 | **cidade:** Cais da Coluna | Cais da Coluna | ☆ |
| 92 | Rio de Luz | *River of Light* | Reinos do Céu | 428–440 | posto: Boia de Luz | Cais da Coluna | ☆ |
| 93 | Convés da Barca da Lua | *Moon Ship Deck* | Reinos do Céu | 436–450 | posto: Cabine da Proa | Cais da Coluna | ☆ |
| 94 | Roda Celeste das Doze Jarras | *Celestial Twelve-Jar Wheel* | Reinos do Céu | 446–460 | posto: Eixo da Roda | Cais da Coluna | ☆ |
| 95 | Esferas dos Arcontes | *Spheres of the Archons* | Reinos do Céu | 456–470 | posto: Ilhota Parada | Cais da Coluna | ☆ |
| 96 | Jardim de Luz | *Garden of Light* | Reinos do Céu | 466–478 | posto: Pérgola do Jardineiro | Cais da Coluna | ☆ |
| 97 | Convés da Barca do Sol | *Sun Ship Deck* | Reinos do Céu | 474–486 | posto: Cabine da Popa | Cais da Coluna | ☆ |
| 98 | Pés da Coluna da Glória | *Foot of the Column of Glory* | Reinos do Céu | 482–492 | posto: Degrau Um | Cais da Coluna | ☆ |
| 99 | Mirante do que Segura o Céu | *Lookout of the Sky-Holder* | Reinos do Céu | 488–498 | posto: Mão Aberta | Cais da Coluna | ☆ |
| 100 | Varanda do Tempo de Depois | *Porch of the Time After* | Reinos do Céu | 495–500 | posto: a Varanda | Cais da Coluna | ☆ |

**Contas:** 42 mapas no escopo ★ (níveis 1–102), 58 depois ☆. Cidades: 4 de hoje + 6 capitais inspiradas (§2) + 12
cidades de raça (§3) + 4 cidades de região tardia (Taa Pwani, Cais de Mesektet, Dur-Jarras, Fortaleza) + Cais da
Coluna = **27 cidades**, 16 no escopo ★.

**Caminho de nível sugerido (o jogador escolhe, mas a faixa guia):** 1–5 Zarafshan → 5–9 Areias → 9–13 Kaggen →
13–18 Grutas (fim da campanha) → 16–50 "segundo anel" dos quatro biomas (03–06, 09–12, 15–18, 21–24) → 45–72 Costa das
Monções → 60–88 Jhilpur → 70–98 Teto do Mundo → 90–140 Mingcheng → 100–150 Estepe → 140–200 Farnkesh → 180–240 Mar
Gelado → 220–280 Hinokura → 260–320 Rio das Doze Horas → 300–360 Margens → 340–440 Limiar Profundo → 420–500 Céu.

---

## 2. Cidades-capitais inspiradas

### 2.1 A regra (propriedade intelectual)

O usuário pediu cidades "baseadas nas cidades de *Avatar: A Lenda de Aang*". A série é de outra empresa, então aqui
vale só a **inspiração de arquétipo** (o tipo de cidade), e o desenho vem das **mesmas arquiteturas reais** que a série
usou como referência:

- **Nada** de nomes, personagens, símbolos, nações, "dobra" de elementos, história, figurino ou logotipos da série.
  Nenhuma cidade do jogo deve ser reconhecível como cópia de uma cidade da série: a planta, o nome, a história e a
  função no jogo são nossos.
- O elemento de cada capital é um dos **Cinco Elementos de Luz** (bíblia §1.3), que no jogo é **sabor** (cor, falas,
  tema), nunca poder de "dobrar" nada.
- Cada capital tem uma **fonte real** (arquitetura) e um **gancho maniqueísta** (por que a luz está ali).
- Culturas vivas com respeito (bíblia §0.5): arquitetura, cores e ofícios, nunca ritual de fé como minijogo; mosteiros
  de fé viva viram **ermidas/mirantes** sem culto.

| Capital | Mapa | Elemento (primo) | Arquétipo | Arquitetura real de referência | Gancho maniqueísta |
|---|---|---|---|---|---|
| **Jhilpur, a Cidade nas Raízes** | 31 ★ | Água Viva (Joaquim) | cidade num pântano de raízes gigantes | casas de palafita do delta de Bengala (Sundarbans), pontes de raiz viva de Meghalaya, ghats de degraus | a viagem de Mani à Índia (Kephalaia 1): a Água Viva contra o Lodo |
| **Sangkent, a Cidade-Escadaria** | 36 ★ | as cinco juntas (cruzamento) | cidade de terraços de pedra na montanha com calhas e carrinhos entre os níveis | fortes e vilas em terraço do Pamir e de Hunza (Baltit, Altit), casas-torre de Shibam (Iêmen) | a encruzilhada das caravanas que levaram os hinos para o leste |
| **Mingcheng, a Cidade dos Nove Anéis** | 41 ★ | Luz (Maria) | capital imensa de muralhas em anel, bairros concêntricos e cidadela no meio | cidades imperiais muradas da China (planta em grade de Chang'an, a Cidade Proibida), portões de Xi'an | a Mingjiao, "a Doutrina da Luz" chinesa (*Compêndio*, 731; Cao'an em Quanzhou) |
| **Farnkesh, os Ninhos de Pedra** | 54 ☆ | Vento Vivo (Maithe) | ermidas no alto de pináculos ligadas por pontes | dzongs do Butão (Paro Taktsang), mosteiros sobre os rochedos de Meteora (Grécia), como **forma**, sem culto | *farn* (sogdiano/persa), o nimbo de glória que "mora no alto" |
| **Porto da Lua Branca** | 59 ☆ | Ar Sereno (Lavine) | cidade de gelo e canais no norte polar com motivos de lua | casas de neve e de turfa do Ártico (iglu, *qarmaq*), casas de tábua da costa noroeste do Pacífico (sem postes de brasão, que são de clãs vivos), cidades-canal | a Barca da Lua, que junta a luz libertada (a lua cresce); Lavine é a "Barqueira da Lua" |
| **Hinokura, a Ilha-Forja** | 65 ☆ | Fogo Vivo (Ícaro) | capital numa ilha vulcânica com palácios de laca vermelha | castelo de Shuri e as *gusuku* de Ryukyu (laca vermelha, telhas vermelhas, muros curvos de coral), cidades-castelo japonesas (sem santuários) | o celeiro celeste do ferreiro (bíblia §1.6): *hi no kura* = "celeiro do fogo" |

Cada uma tem o bloco completo do mapa em §5. Resumo de construção (kit):

- **Jhilpur:** estilo novo `RootStilt` (pisos de tábua sobre raízes-colunas, telhado de palha curvo em "dorso de
  barco", ghats de tijolo descendo para a água). Paleta: verde-raiz `#4E6B3A`, tijolo `#A4553A`, palha `#C9A35B`,
  água turva `#5F7F6E`, Água Viva `#4FC3B0`, lanternas `#FFC46B`. Ruas = passarelas; "praça" = plataforma redonda em
  volta da **Raiz-Mãe**.
- **Sangkent:** estilo novo `StoneTerrace` (casas de pedra e barro de 2–4 pisos, vigas de madeira à mostra, varandas de
  treliça). **Calhas** de pedra polida e **carrinhos de tábua** que descem entre os 7 terraços (transporte da cidade).
  Paleta: pedra `#8C8173`, barro `#B98B5E`, madeira `#5A3E2B`, neve `#E6EEF2`, damasco em flor `#F2B8A0`, tecido
  `#C0392B`/`#2E86AB`.
- **Mingcheng:** estilo novo `RingWall` (muralha de taipa com face de tijolo cinza, portões de três arcos, torres de
  vigia de telhado curvo, telhas vidradas amarelas só no anel interno). 9 anéis: os 3 de fora são campo e vila, os 3
  do meio bairros de ofício, os 3 de dentro a cidadela (onde fica o mapa 46). Paleta: tijolo cinza `#7D7F86`, laca
  `#B23A2E`, telha dourada `#D9A82B`, jade `#5E9E7E`, papel de lanterna `#F4E3B5`.
- **Farnkesh, Porto da Lua Branca, Hinokura:** ver os mapas 54, 59 e 65.

---

## 3. Cidades das raças

### 3.1 A regra (propriedade intelectual e respeito)

O usuário pediu cidades "baseadas nas cidades de Warcraft, com os NPCs sendo orcs, elfos e outras raças que tem no
Dota". Essas marcas são de outras empresas, então:

- Só o **arquétipo de fantasia genérico** (orc, elfo, anão, trol, centauro, gigante, sátiro, povo-do-mar, povo-tronco,
  kobold, povo-touro), que é de domínio comum (mitologia grega, folclore europeu). **Nada** de nomes, personagens,
  facções, brasões, logotipos, história, armas famosas, falas ou silhuetas reconhecíveis desses jogos. Nada de
  "naga" (figura sagrada viva na Índia): o povo do recife é o **povo-do-mar**.
- **Nenhum povo é mau.** Cada povo **guarda** ou **perdeu** um dos Cinco Elementos de Luz; alguns grupos (nunca o povo
  inteiro) foram **apagados** pela Treva ou **tocados por Āz** (cobiça) e voltam a si quando a faísca é libertada.
- **Sem estereótipo racial:** nenhuma raça fantástica fala com sotaque de povo real, nenhuma é "tribal primitiva",
  nenhuma é avarenta por natureza, e nenhuma é montada com traços de um povo real (roupa sagrada, cocar, pintura de
  rito). Os orcs não são brutos, os trols não são selvagens, os kobolds não são agiotas.
- **NPCs com nomes de mitologias pouco conhecidas**, **uma mitologia diferente por raça** (fonte em cada linha), usando
  nomes de heróis, reis históricos ou personagens de fábula, nunca de divindades de fé viva.
- Moram **misturados**: toda cidade de raça tem humanos da ilha, e toda capital tem gente das raças. A Vó Edna fala de
  todos como "vizinhos de longe".

### 3.2 Resumo

| Raça | Cidade (mapa) | Elemento | Estado | Mitologia dos nomes | Escopo |
|---|---|---|---|---|---|
| Elfos-da-mata | Copas de Garajonay (05) | Água Viva | guardiões | guanche (Canárias) | ★ |
| Orcs | Tambor Rubro (11) | Fogo Vivo | guardiões; clãs tocados por Āz | hurrita (contos de Kessi, Appu, Gurparanzah) | ★ |
| Anões | Salões de Bronze de Napirasu (17) | Luz | **perderam** (a forja de luz apagou) | elamita | ★ |
| Kobolds | Mercado dos Túneis (22) | Ar Sereno | guardiões (engarrafam o ar quieto) | sogdiana (Cartas Antigas) | ★ |
| Povo-touro | Aratta (26) | Fogo Vivo | guardiões | suméria (épicos de Enmerkar e Lugalbanda) | ★ |
| Povo-do-mar | Cidade-Recife de Kirta (28) | Água Viva | **perderam** (o coral apagou) | ugarítica (épicos de Kirta e Aqhat) | ★ |
| Trols | Palafitas de Tarchon (33) | Ar Sereno | metade apagados | etrusca | ★ |
| Altos-elfos | Torres Brancas de Amanirenas (42) | Luz | guardiões | kushita / meroítica | ★ |
| Centauros | Acampamento de Soslan (47) | Vento Vivo | guardiões | osseta (sagas dos Nartes) | ☆ |
| Gigantes-da-colina | Fortim dos Gigantes (49) | Fogo Vivo | arrependidos (ex-apagados) | maniqueia (*Livro dos Gigantes*) | ☆ |
| Povo-tronco | Arvoredo de Haripushpa (50) | Água Viva | guardiões lentos | tocária (reis de Kucha) | ☆ |
| Sátiros | Vale dos Sátiros (53) | Vento Vivo | guardiões; um rei tocado por Āz | frígia | ☆ |

### 3.3 As doze cidades

**Copas de Garajonay — elfos-da-mata (mapa 05 ★)**
- **Povo e luz:** elfos de orelha longa e pele cor de casca, que vivem na mata de louro onde a névoa vira água nas
  folhas ("a chuva que vem de lado"). Guardam a **Água Viva** que a névoa traz.
- **Arquitetura e paleta (estilo novo `Canopy`):** casas redondas de tábua presas nos troncos a 8–14 m, **pontes de
  cipó** e plataformas em anel, escadas em espiral, telhados de folha de louro em camadas, lanternas de casulo. Paleta:
  louro `#3F6B3A`, musgo `#8DB255`, casca `#6E4B32`, névoa `#DDE6E2`, água `#7FD1C7`, flor de urze `#E7B9D9`.
- **Moradores:** **Gara**, guardiã das copas (missões); **Jonay**, arqueiro (treinador de tiro, a Maithe adora);
  **Dácil**, ervateira (loja de ervas e poções de névoa); **Bencomo**, o velho chefe (graduações e convites);
  **Tinguaro**, rastreador (tarefas de caça). Fonte: lenda de Gara e Jonay (La Gomera); Bencomo, Tinguaro e Dácil
  (Tenerife, séc. XV, histórico e lenda).
- **Loja/serviço:** **Coletor de Névoa**: troca fibra por "Gota de Névoa", ingrediente de poção e de comida de montaria.
- **Mecânica única:** a cidade fica no alto: anda-se por pontes; cair da ponte leva à mata de baixo (sem dano, rota
  longa de volta). Montaria só as voadoras e as que pulam (Íbex).

**Tambor Rubro — orcs (mapa 11 ★)**
- **Povo e luz:** orcs altos de pele verde-oliva ou cinza, presas pequenas, guerreiros de **palavra dada**. Os tambores
  deles marcam a hora e "acordam o Fogo Vivo" da forja. Alguns clãs aceitaram as **coroinhas de lata de Āz** e
  brigam por ouro; o resto da cidade quer os primos para trazê-los de volta.
- **Arquitetura (estilo `WarCamp`):** paliçada de troncos com pontas (pontas para fora, nunca troféus), **Salão Grande**
  de vigas e couro com telhado em duas águas muito alto, torres de tambor, tendas de couro tingido de ocre, arena de
  treino de areia. Paleta: ocre-rubro `#A8452B`, arenito `#C9774B`, couro `#7A4A2A`, ferro `#4A4A4F`, brasa
  `#FF8A3D`, faixa de pano `#E8C547`.
- **Moradores:** **Gurparanzah**, chefe-arqueira (missões, graduação G4 do Ícaro passa por aqui); **Kessi**, caçador
  (tarefas de caça); **Appu**, mercador gentil que sonhava com filhos e adota todos os órfãos do acampamento (loja
  geral); **Ashmunikal**, a matriarca dos tambores (Lâmpada Guardiã da cidade). Fonte: contos hurritas de Kessi, o
  caçador, de Appu e do herói Gurparanzah (tabuinhas hititas de Hattusa); Ashmunikal, rainha hitita (histórica).
- **Loja/serviço:** **Forja de Tambor**: conserto 20% mais barato e "Têmpera do Tambor" (encantamento temporário de
  arma, §26 do README, versão de fogo).
- **Mecânica única:** **Ritmo do Tambor**: a cada 10 min de relógio do jogo os tambores tocam e, por 60 s, quem está na
  cidade ganha +10% de recuperação de fôlego e as tarefas de caça dão +1 ponto. Os primos podem tocar junto
  (minijogo de ritmo igual aos Hinos do Bema, sem rito).

**Salões de Bronze de Napirasu — anões (mapa 17 ★)**
- **Povo e luz:** anões de barba trançada com contas de bronze, fundidores. A forja deles era acesa pela **Luz** presa
  num cristal; a sombra a apagou e agora eles forjam no frio. Reacender a Grande Forja é a história do mapa.
- **Arquitetura (estilo `RuneHall`):** cidade escavada na montanha: salões de pilares quadrados, **portões de runa**
  que acendem uma linha por vez, pontes de pedra sobre poços de lava apagada, elevadores de corrente, casas-nicho em
  andares. Paleta: granito `#6E6A66`, bronze `#B07A3B`, cobre esverdeado `#5E9C8A`, brasa fria `#8C3B2E`, luz de runa
  `#FFE08A`.
- **Moradores:** **Napirasu**, mestra fundidora (refino e fusão, a "Adamas da montanha"); **Untash**, mestre de obras
  (missões); **Puzur**, guardião das runas (loja de runas, §26 do README); **Shilhak**, minerador (tarefas); **Kidin**,
  cozinheiro do salão. Fonte: Napirasu (rainha elamita cuja estátua de bronze de ~1 750 kg está no Louvre), Untash-
  Napirisha (construtor do zigurate de Chogha Zanbil), Puzur-Inshushinak (rei da escrita elamita linear), Shilhak-
  Inshushinak, Kidin-Hutran (reis elamitas).
- **Loja/serviço:** **Portões de Runa**: o jogador grava 1 runa por dia de graça; loja de **runas de bronze**.
- **Mecânica única:** **a Grande Forja** acende em 5 etapas pela comunidade (offline: pelas missões; online: pelo
  servidor inteiro entregando "Faíscas da Forja"). Cada etapa acesa dá à cidade um serviço novo (refino +1 de chance,
  fusão mais barata...).

**Mercado dos Túneis — kobolds (mapa 22 ★)**
- **Povo e luz:** pequeninos de focinho, escamas lisas e rabo, mineiros e inventores. Nos túneis o ar é parado; eles
  aprenderam a **engarrafar o Ar Sereno** que desce das frestas e é isso que vendem, junto com sucata consertada.
- **Arquitetura (estilo `TunnelBazaar`):** túneis redondos com barracas encaixadas em nichos, trilhos de vagonete,
  roldanas, cata-ventos dentro de tubos (que "puxam" o ar), lanternas de vidro colorido feitas de garrafa. Paleta:
  basalto `#3B3B44`, cobre `#B06B3A`, vidro verde `#7FBF8F`, vidro âmbar `#E0A040`, lona `#C9B48A`, ar sereno
  `#C9B8F0`.
- **Moradores:** **Miwnay**, a dona do mercado (feira rotativa), e a filha **Shayn**, consertadora (conserto e
  reciclagem); **Farnkhund**, vagoneiro (viagem rápida pelos túneis entre os mapas 19–24); **Takhsich**, o inventor
  (missões do Ícaro). Fonte: *Cartas Antigas* sogdianas (c. 313 d.C.): a carta de Miwnay e de sua filha Shayn;
  Farnkhund e Takhsich, nomes sogdianos das cartas e documentos de Mug **[verificar grafias]**.
- **Loja/serviço:** **Garrafas de Ar Sereno** (consumível: limpa lentidão e dá +20% de fôlego por 60 s) e a **Banca de
  Troca** (troca material por material sem ouro, 3 por 2).
- **Mecânica única:** **mercado que muda de túnel**: a feira se move pelas 4 galerias a cada dia do jogo (o mapa mostra
  onde). Os kobolds tocados por Āz (só alguns) roubam brilho das lanternas; vencê-los devolve as garrafas.

**Aratta, a Cidade dos Chifres — povo-touro (mapa 26 ★)**
- **Povo e luz:** gente grande com cabeça de touro e chifres largos, pastores e corredores das savanas, gentis e
  musicais. Guardam o **Fogo Vivo** do sol: o fogo baixo e controlado que renova o capim.
- **Arquitetura (estilo `ReedHorn`):** grandes casas de **junco amarrado em arcos** (salões de junco dos pântanos da
  Mesopotâmia) cobertas de esteira, cercados redondos de pedra, **postes de chifre** com fitas (marcos de caminho, não
  objetos de culto), lira de cabeça de touro na praça. Paleta: junco `#C8B27A`, esteira `#A88B55`, barro vermelho
  `#9C4A2F`, capim dourado `#D9B44A`, céu de savana `#9CC3E0`, fita `#3F7CC0`.
- **Moradores:** **Enmerkar**, o chefe construtor (missões); **Lugalbanda**, o andarilho que se perdeu e voltou
  (tarefas de exploração, mapas secretos); **Shulgi**, o corredor (treino de corrida: corrida contra o relógio pela
  savana, prêmio de montaria); **Ensuhkeshdanna**, o rival que virou amigo (duelos amistosos). Fonte: épicos sumérios
  de Enmerkar e o Senhor de Aratta e de Lugalbanda; Shulgi de Ur, o rei que correu de Nippur a Ur num dia (Hino de
  Shulgi A).
- **Loja/serviço:** **Lira do Touro**: músico que ensina 1 canção nova por dia (buff de grupo de viagem, +5% de
  velocidade de andar a pé por 5 min, nunca em combate).
- **Mecânica única:** **Corrida de Shulgi**: percurso marcado pela savana; tempo bom dá "Fitas de Corrida" que trocam
  por cosméticos de montaria e pela montaria nova **Auroque de Aratta** (§6).

**Cidade-Recife de Kirta — povo-do-mar (mapa 28 ★)**
- **Povo e luz:** gente de cauda de peixe e de pernas de pescador (as duas formas vivem juntas), que cuidava do coral
  aceso de **Água Viva**. O Lodo escureceu o recife e o coral perdeu a cor.
- **Arquitetura (estilo `CoralReef`):** torres de coral com janelas redondas, metade acima da água, metade abaixo
  (vista em corte pela câmera); pontes de concha; jardins de alga; cúpulas de madrepérola. Paleta: coral `#E8836B`,
  madrepérola `#F1E9DA`, turquesa `#2FB3B3`, alga `#3E7D57`, areia `#E9D3A2`, coral apagado `#9A9590`.
- **Moradores:** **Kirta**, o rei que perdeu a família no mar e reconstruiu tudo (missões); **Pughat**, heroína que
  cuida de todos (missões de resgate); **Danel**, o juiz justo (loja de ferramentas de mergulho); **Aqhat**, o arqueiro
  (treinador). Fonte: épicos ugaríticos de Kirta e de Aqhat (tabuinhas de Ugarit, séc. XIV a.C.).
- **Loja/serviço:** **Bolhas de Ar** (deixam andar no fundo por 3 min), **Apito de Concha** para montarias aquáticas.
- **Mecânica única:** **Reacender o coral**: cada faísca libertada no mapa acende um pedaço do recife; o coral aceso é
  zona de recuperação de vida (fora de combate). Dá a meta coletiva do servidor.

**Palafitas de Tarchon — trols (mapa 33 ★)**
- **Povo e luz:** trols altos e magros, de pele azul-musgo, que se curam rápido e falam devagar. Vivem de pesca e
  cestaria. À noite o pântano enche de **fogos-fátuos**: são faíscas presas (Ar Sereno), e metade dos trols, de tanto
  olhar para elas, ficou **apagada** (esquecida e triste).
- **Arquitetura (estilo `StiltVillage`):** casas compridas de palafita com telhado de folha até o chão, passarelas
  altas, redes de pesca secando, cestos gigantes, ilhas flutuantes de junco. Paleta: musgo-azul `#4F7A82`, madeira
  molhada `#5B4636`, folha `#6F8F3F`, névoa lilás `#B9A9D6`, fogo-fátuo `#D6E8FF`.
- **Moradores:** **Tarchon**, o fundador (missões); **Ramtha**, curandeira (cura e Lâmpada Guardiã); **Larthia**,
  cesteira (loja de bolsas: +1 linha de mochila, versão do Alforje); **Vel**, pescador (pesca); **Thanchvil**,
  contadora de histórias. Fonte: Tarchon, fundador lendário de Tarquínia; Ramtha, Larthia, Vel e Thanchvil, nomes
  etruscos de inscrições e túmulos (Tarquínia, Cerveteri).
- **Loja/serviço:** **Cesto de Lanternas**: compra fogos-fátuos libertados e vende "Lanterna de Fogo-Fátuo" (luz que
  dura a noite toda, pela regra da Noite).
- **Mecânica única:** **Despertar o vizinho**: os trols apagados ficam parados na cidade; levar a cada um uma lanterna
  acesa o acorda (conta para a missão diária). A cidade fica mais cheia e alegre conforme os primos ajudam.

**Torres Brancas de Amanirenas — altos-elfos (mapa 42 ★)**
- **Povo e luz:** elfos altos de pele escura e cabelo branco, astrônomos e arquitetos. As torres captam a **Luz** do
  sol e a passam por espelhos até o chão, de dia e de noite. Orgulhosos, mas gentis com crianças.
- **Arquitetura (estilo `WhiteSpire`):** torres brancas finas com cúpula de vidro, **pontes de folha** (lâminas de
  pedra curvas como folhas) ligando as torres, jardins em terraço, pirâmides baixas e pontudas (eco de Meroé) como
  base das torres. Paleta: cal `#F3EFE6`, ouro claro `#E3C46B`, azul-céu `#7FB2E5`, folha prateada `#A8C5B5`, pedra
  arenito `#C79E6B`, luz `#FFF4C2`.
- **Moradores:** **Amanirenas**, rainha-guardiã (missões; a "candace" de um olho só da história real); **Amanitore**,
  construtora (casa: itens de decoração da cidade); **Natakamani**, mestre dos espelhos (encantamento de luz);
  **Shanakdakhete**, treinadora de magia; **Arkamani**, o que lê as estrelas (Caderno: bestiário do céu). Fonte:
  candaces e reis de Kush/Meroé (Amanirenas, Amanitore, Natakamani, Shanakdakhete, Arkamani I).
- **Loja/serviço:** **Espelhos de Natakamani**: refino com "luz refletida" (custo em Pedra-Faísca menor nas faixas
  +7…+9).
- **Mecânica única:** **Relógio de espelhos**: os raios de luz entre as torres mudam com a hora; pôr os espelhos no
  lugar (quebra-cabeça) abre salas de baú e o atalho para o mapa 45.

**Acampamento de Soslan — centauros (mapa 47 ☆)**
- **Povo e luz:** centauros da estepe, metade cavalo, contadores de saga e corredores do **Vento Vivo**.
- **Arquitetura (estilo `SteppeCamp`):** grandes tendas de feltro redondas sobre carroças (cidade que anda devagar
  pela estepe ao longo da semana), estandartes de crina, cercados de corda, forno de pedra coletivo. Paleta: feltro
  `#D8C9A8`, feltro tingido `#8E2F2F`/`#2F5E8E`, capim `#BFAE5A`, céu `#A7C7E7`, couro `#6E4A2E`.
- **Moradores:** **Soslan**, o herói de pele de aço (missões); **Batradz**, ferreiro-guerreiro (ferraria); **Satana**,
  a sábia da casa (Lâmpada Guardiã e conselhos); **Syrdon**, o trapaceiro (loja de truques e cosméticos, sempre com uma
  pegadinha boa); **Uryzmæg**, o velho chefe. Fonte: sagas dos Nartes (Ossétia).
- **Loja/serviço:** **Selaria Nômade**: selas e mantas para montaria (cosméticos, Mounts §6).
- **Mecânica única:** **cidade que anda**: o acampamento muda de lugar no mapa 47 a cada dia do jogo (3 pontos); o Cais
  da Barca segue junto.

**Fortim dos Gigantes-da-Colina (mapa 49 ☆)**
- **Povo e luz:** gigantes (ogros de conto) descendentes dos que, no *Livro dos Gigantes*, sonharam com o fim e se
  arrependeram. Cozinheiros enormes, guardam o **Fogo Vivo** das lareiras. Fazem tudo grande: os primos sentam em
  colheres.
- **Arquitetura (estilo `HillFort`):** colina com muralha de terra em anéis, casas de pedra redondas gigantes (3× a
  escala), mesas-pedra, panelas do tamanho de casas. Paleta: terra `#8A6A44`, pedra `#9A948A`, fumaça boa `#CFC6B8`,
  brasa `#E2763A`, grama `#7F9A4A`.
- **Moradores:** **Baraqel**, o chefe (missões); **Virogdad**, o sonhador (interpreta sonhos: missões de mapa
  escondido); **Shahmizad**, o patriarca velho e arrependido (história). Fonte: *Livro dos Gigantes* maniqueu
  (Barakʾel, Virōgdād, Šahmīzād, fragmentos médio-persas e de Qumran) **[verificar grafias]**.
- **Loja/serviço:** **Panela Grande**: cozinha comida de grupo (5 porções de uma vez, mesmos efeitos da Cozinha).
- **Mecânica única:** **escala gigante**: dentro do fortim tudo é 3× maior; os primos sobem em móveis para chegar às
  lojas (parkour leve, com escada para quem não quiser).

**Arvoredo de Haripushpa — povo-tronco (mapa 50 ☆)**
- **Povo e luz:** árvores andantes, lentíssimas, que bebem a **Água Viva** do degelo e lembram de tudo. Falam uma
  frase por minuto; os primos aprendem a esperar.
- **Arquitetura (estilo `LivingGrove`):** casas **dentro** de troncos ocos enormes, raízes como escadas, ninhos de
  pássaro como janelas, círculo de pedras com musgo. Paleta: casca `#5E4A3A`, musgo `#6E8F4A`, folha nova `#A9C96B`,
  água `#86C9D6`, flor `#F0D27A`.
- **Moradores:** **Haripushpa**, o mais velho (missões); **Suvarnapushpa**, a jardineira (sementes da casa); um
  herbolário humano sogdiano de passagem. Fonte: Haripuṣpa e Suvarṇapuṣpa, reis de Kucha (Bacia do Tarim), onde se
  falava o tocário B.
- **Loja/serviço:** **Viveiro**: mudas raras para a casa (Housing, plantas).
- **Mecânica única:** **tempo lento**: no arvoredo o relógio do jogo anda pela metade e as plantas da casa crescem 2×
  (só enquanto o primo está lá).

**Vale dos Sátiros (mapa 53 ☆)**
- **Povo e luz:** sátiros de pernas de bode, flautistas e dançarinos, que tocam o **Vento Vivo** nas flautas duplas.
  O rei deles, **Midas**, foi tocado por Āz (tudo o que pega vira ouro, até a comida) e precisa lavar as mãos no rio
  de Água Viva para voltar a si.
- **Arquitetura (estilo `Glade`):** clareiras em anel ligadas por túneis de folhagem, palcos de pedra, casas de vime em
  forma de cabaça, rio de seixos dourados. Paleta: verde-claro `#9CCB6A`, vime `#C9A76A`, pedra `#B7AFA3`, ouro do rio
  `#E3B341`, flor `#E27A9B`.
- **Moradores:** **Marsyas**, mestre de flauta (treino de música: canções de viagem); **Gordias**, o fazendeiro dos nós
  (quebra-cabeças de corda que abrem baús); **Midas**, o rei de mãos douradas (missão de redenção). Fonte: mitos
  frígios de Marsyas, Gordias (o nó górdio) e Midas (o toque de ouro e o rio Pactolo).
- **Loja/serviço:** **Flauta Dupla**: instrumentos de casa e canções.
- **Mecânica única:** **Baile do Vale**: ao entardecer, dança de roda com os sátiros (minijogo de ritmo); a melhor
  nota do dia dá "Pó de Ouro do Pactolo" (material de refino).

---

## 4. Modelo de mapa (para copiar)

```markdown
### NN · Nome em pt / *Name in en* — nv A–B · ★ (ou ☆)
- **Fonte:** (1 linha: de onde vem o nome; mitologia, lugar ou texto) **[verificar]** se preciso
- **Bioma e visual:** bioma base; paleta (3–6 hex); props do kit (o que o TownKit/receita precisa)
- **Layout:** tamanho (P/M/G/GG); entrada e saídas; landmarks; trilhas; **cidade/posto** e **cidade de retorno**
- **Inimigos:** 3–5, cada um com [arquétipo] e uma linha de fonte (só lado mítico)
- **Chefe:** chefe do mapa (nível B+2) ou gancho de chefe do mundo / raide
- **Mecânica:** 1 regra especial do mapa (terreno, tempo, quebra-cabeça)
- **Ganchos:** loot (material/conjunto) · montaria (id existente ou nova, §6) · casa (conjunto do CatalogoCasa)
- **Noite:** o que muda; bicho só da noite; regra de visão
```

---

## 5. Os 100 mapas, por região

### Região 1 — Bosque do Zarafshan (Floresta, Água Viva) · mapas 01–06

Cidade de região: **Vila da Vó Edna** (bíblia §2.1). Estilo `Sogdian`. Bichos-base: Lobo do Tugai, Aranha-Tecelã
Apagada, Javali do Lodo. Chefe do mundo da região: **Saklas** (arconte, mapa 06).

### 01 · Pomar da Vó Edna / *Grandma Edna's Orchard* — nv 1–3 · ★
- **No jogo (2026-11-09): colheita** (`ZarafshanHarvestRules`): 4 árvores nas alamedas do Pomar, fora da Vila, com feixe dourado quando maduras; [E] a até 4 m dá 1 ingrediente da Cozinha (erva ou fibra, alternando) uma vez por dia do jogo e por árvore, salvo em `WorldMapProgress`, com fala do herói ou da Vó Edna. Nada depende dela. Falta: item de damasco próprio (por enquanto as árvores dão ingredientes que já existem).
- **Fonte:** o pomar da Vó, "bonito como o jardim de Mihrshah" (texto médio-persa M 47); vale do Zarafshan (Sogdiana).
- **Bioma e visual:** pomar e beira de tugai; adobe damasco `#D9A066`, choupo `#7FA650`, turquesa `#3FA7A3`; damasqueiro,
  amoreira, romãzeira, latada, canteiro de melão, aryk com pontezinhas, tandir.
- **Layout:** P. A **Vila** (cidade completa, `CityTownRecipes.VilaEdna`) no centro-norte; pomar em volta com 3
  clareiras de treino; saída leste para o 02 e sul para a Estrada das Caravanas (07). Retorno: Vila da Vó Edna.
- **Inimigos:** Lobo do Tugai filhote [rápido] (bicho real); Aranha-Tecelã Apagada pequena [enxame] ("lembra de
  Ananse", diz a Aso); Corvo Ladrão de Damasco [rápido] (bicho real, apagado).
- **Chefe:** nenhum (tutorial). Gancho: o primeiro encontro com o Ari no portão.
- **Mecânica:** **colheita**: as árvores do pomar dão fruta a cada dia do jogo (comida da Cozinha); tutorial de cair e
  levantar na praça (respawn na cidade).
- **Ganchos:** loot fibra, erva, damasco · montaria `mount_pony` (missão "Primeira Sela", Srosh) · casa: **Iwan da Vó
  Edna**, **Horta dos Melões**.
- **Noite:** lanternas de papel acesas; a Coruja-de-Fumaça aparece no pomar (só olha, não ataca abaixo do nível 3);
  visão normal (a Vila é iluminada).

### 02 · Tugai do Rio Dourado / *Golden River Tugai* — nv 2–5 · ★
- **No jogo (2026-11-11): bicho próprio** no mundo contínuo (`EarlyStandInBeastRecipes`, página no Caderno, `EarlyStandInBeastsPlacementTests`): Cegonha Apagada [distância] (`enemy_forest_dimmed_stork`, joga gravetos de 8 m) lidera "cegonhas" no Prado das Cegonhas e "aves_da_varzea" na Várzea Dourada, no lugar do urutau.
- **No jogo (2026-11-09): vaus de Água Viva** (`ZarafshanFordRules`): nos 4 vaus do rio dentro do Tugai (feixe turquesa, 6 m) o herói ferido recupera 3% da vida máxima por segundo, só até 70%, com fala da Vó Edna. O ×0,6 da ficha **não** foi aplicado, de propósito: é o segundo mapa do jogo e os vaus são caminho principal; a mecânica fica só como bônus.
- **Fonte:** *Zarafšān*, "o que espalha ouro"; *tugai*, mata de beira de rio da Ásia Central; Kök-Böri, o lobo celeste
  das lendas túrquicas.
- **Bioma e visual:** floresta ribeirinha; choupo dourado `#C9A23F`, tamargueira rosa `#D98BA6`, junco `#9DAA5A`, água
  `#6FB7B0`; troncos caídos, cegonhas, ninhos.
- **Layout:** M. Rio em S com 3 vaus; **posto: Moinho do Pomar** (roda d'água, vendedora de poção); clareira do chefe ao
  norte (a arena de Kök-Böri de hoje). Retorno: Vila da Vó Edna.
- **Inimigos:** Lobo do Tugai [rápido]; Aranha-Tecelã Apagada [enxame]; Javali do Tugai [tanque] (bicho real); Cegonha
  Apagada [distância, joga gravetos] (bicho real).
- **Chefe:** **Kök-Böri, o Lobo Celeste** (`enemy_forest_alpha_wolf`, de hoje).
- **Mecânica:** **vaus**: atravessar o rio a pé é lento (×0,6); montaria aquática ou ponte de tronco empurrada.
- **Ganchos:** loot couro, fibra, Pedra-Faísca · montaria `mount_capybara`, `mount_maned_wolf` (drop noturno) · casa:
  **Tugai do Zarafshan**.
- **Noite:** **Mardagayl** (o lobo das sete noites) e **Javali do Lodo** em bando; a visão encurta fora da trilha.

### 03 · Brejo do Lodo Escuro / *Dark Mire Marsh* — nv 16–22 · ★
- **No jogo (2026-11-11): bicho próprio** no mundo contínuo (`EarlyStandInBeastRecipes`, página no Caderno, `EarlyStandInBeastsPlacementTests`): Sapo-Bolha do Lodo [distância] (`enemy_forest_bubble_toad`, bolha de lama de 7,5 m) entra em "lodo_escuro" e Mosquito de Fumaça [enxame] (`enemy_forest_smoke_mosquito`) lidera "fumaca", os dois no lugar do Enxame de Morcegos.
- **No jogo (2026-11-03): jogável** (`MapLayouts.Zarafshan.cs`): posto Ponte do Aryk (lâmpada, casa do barqueiro, cais) na ilha norte, dois canais de água preta atravessados só por 3 passarelas de tronco, Poço Turvo no charco oeste (anel da Mãe-Lodo; a luta segue como desafio do mapa-múndi); trilha a pé do Tugai (saída sul do 02) e para o 04; 5 clareiras nv 16–22 (Javali [tanque]; Lobo no lugar da Enguia [rápido]; Aranha e Enxame de Morcegos no lugar dos Mosquitos [enxame]; à noite Javali do Lodo, Faíscas Presas no lugar do Fogo-Fátuo e Mardagayl raro). Falta: lama ×0,7 fora da passarela, Água Viva limpando poças.
- **Fonte:** o **Lodo**, elemento da Treva contrário à Água Viva (Teodoro bar Konai; bíblia §1.3).
- **Bioma e visual:** tugai alagado; lama `#3E3A2F`, água verde `#5F7F5A`, junco seco `#B6A66A`, faísca `#4FC3B0`; troncos
  podres, pegadas de lama, bolhas.
- **Layout:** M. Ilhas de terra ligadas por passarelas de tronco; **posto: Ponte do Aryk** (ponte de pedra com uma
  lâmpada); no centro o Poço Turvo. Retorno: Vila da Vó Edna.
- **Inimigos:** Javali do Lodo [tanque]; Sapo-Bolha do Lodo [distância, cuspe de lama que deixa lento] (invenção);
  Enguia de Lama [rápido, emboscada]; Mosquitos de Fumaça [enxame] (Fumaça, 1º elemento da Treva).
- **Chefe:** **Mãe-Lodo**, sapa gigante apagada que engoliu a fonte; libertada, volta a cuidar do brejo.
- **Mecânica:** **lama**: fora da passarela anda a ×0,7; Água Viva (skills do Joaquim, poção) limpa poças por 20 s.
- **Ganchos:** loot pele de enguia (parte de bicho), Gota de Água Viva · montaria nova **Garça do Brejo** (§6) · casa:
  conjunto novo **Brejo Limpo** (vasos de junco, ponte de tronco em miniatura).
- **Noite:** os mosquitos dobram; **Fogo-Fátuo Preso** [enxame, só à noite] (luz presa perdida); visão curta.

### 04 · Bosque das Amoreiras de Seda / *Silk Mulberry Grove* — nv 18–24 · ★
- **No jogo (2026-11-11): bicho próprio** no mundo contínuo (`EarlyStandInBeastRecipes`, página no Caderno, `EarlyStandInBeastsPlacementTests`): Mariposa de Seda Gigante [distância] (`enemy_forest_silk_moth`, pó de 7,5 m) lidera "gralhas", no lugar do urutau.
- **No jogo (2026-11-03): jogável** (`MapLayouts.Zarafshan.cs`): posto Casa do Tear (oficina, casa da tecelã, cais) no meio de 4 quadras de amoreiras em fileiras (labirinto suave), 4 quintais de tear em ruína (muros, teares/varais, caixas de casulo); oeste para o 03, leste para o 05; 4 clareiras nv 18–24 (Aranha-Tecelã [enxame]; Javali no lugar da Lagarta-Casulo [tanque]; Lobo no lugar da Gralha [rápido]; à noite Coruja-de-Fumaça e Faíscas no lugar das mariposas). Falta: teias que prendem 1,5 s, Fiandeira Esquecida.
- **Fonte:** amoreira e bicho-da-seda da Sogdiana; a lenda da princesa que levou ovos de bicho-da-seda escondidos no
  chapéu para Khotan (Xuanzang; painel de Dandan-Uiliq).
- **Bioma e visual:** pomar abandonado; folha `#6FA34A`, amora `#5B2A4A`, seda `#F4EDE1`, casulo `#EFE3C8`; casas do tear
  em ruína, varais, teares.
- **Layout:** M. Fileiras de amoreiras (labirinto suave) e 4 casas do tear; **posto: Casa do Tear** (tecelã que conserta
  roupa). Retorno: Vila da Vó Edna.
- **Inimigos:** Aranha-Tecelã Apagada [enxame]; Mariposa de Seda Gigante [distância, pó sonolento] (invenção);
  Lagarta-Casulo [tanque]; Gralha Ladra de Fio [rápido] (bicho real).
- **Chefe:** **A Fiandeira Esquecida**, aranha enorme que tece um casulo em volta da faísca; libertada, vira tecelã amiga
  (Aso: "ela era como Ananse, só que esqueceu").
- **Mecânica:** **teias**: fios brilhantes entre as árvores prendem por 1,5 s (`StunRules`); cortáveis com skill.
- **Ganchos:** loot Fio de Seda (roupas da Narisah) · montaria `mount_unicorn` (drop raríssimo) · casa: **Iluminuras e
  Papel Recortado**, conjunto novo **Tear de Seda**.
- **Noite:** as mariposas acendem; a **Coruja-de-Fumaça** caça; visão normal nas fileiras.

### 05 · Copas de Garajonay / *Garajonay Canopy* — nv 20–26 · ★ · cidade de raça (elfos-da-mata)
- **No jogo (2026-11-11): bicho próprio** no mundo contínuo (`EarlyStandInBeastRecipes`, página no Caderno, `EarlyStandInBeastsPlacementTests`): Lagarto-Gigante do Louro [rápido] (`enemy_forest_laurel_lizard`) lidera "louros", no lugar do lobo; Pombo-do-Louro Apagado [distância] (`enemy_forest_laurel_pigeon`, caroço de 8 m) lidera "nevoa" com a aranha, no lugar do urutau.
- **No jogo (2026-11-03): jogável** (`MapLayouts.Zarafshan.cs`, receita `CityTownRecipes.Garajonay`): a cidade dos elfos-da-mata no centro (cidade de retorno, respawn na praça; casas redondas de tábua com telhado de folha de louro, salão da Gara, cais) e as copas: 6 louros gigantes com plataforma em anel a ~8,8 m, casa redonda no alto, escada em espiral no tronco e lanternas de casulo (peça nova `CanopyTree` do kit, com colisor), ligados por 5 pontes de cipó; chão da mata com 3 trilhas, 2 córregos em cascata com pontes e o anel do Velho da Névoa ao norte; oeste para o 04, leste para o 06; 6 clareiras nv 20–26 (Lobo no lugar do Lagarto-Gigante [rápido], Javali de Musgo [tanque], Aranha no lugar da Névoa Pegajosa [controle]; à noite Mardagayl, Faíscas e Coruja). Desde 2026-11-03 é `CitySpec` de cidade de raça (`CityCatalog.RaceCities`): passeio da cidade, respawn dos mapas 05–06 na praça, Gara/Jonay/Dácil/Bencomo/Tinguaro nas portas com visual de elfo e falas, Ervaria da Dácil (Corda de Escalada, Chave do Jardim) e 2 missões da cidade (Docs/Cities.md §1.1). Falta: andar nas plataformas (hoje são cenário alto; anda-se no chão), Pombo-do-Louro [distância], névoa a cada 3 min, Coletor de Névoa, banco da praça encostado na rua Sul (receita).
- **Fonte:** lenda de Gara e Jonay (La Gomera, Canárias); a laurissilva, mata de louro que "bebe a névoa".
- **Bioma e visual:** mata de louro alta e nublada; paleta da cidade em §3.3; urze arbórea, samambaias, musgo pendurado,
  névoa no chão.
- **Layout:** G. **Copas de Garajonay** (§3.3) no alto do centro; chão da mata com 3 trilhas e 2 cascatas; subida por
  escadas em espiral nos troncos. Retorno: Copas de Garajonay.
- **Inimigos:** Lagarto-Gigante do Louro [rápido] (os lagartos-gigantes das Canárias, bicho real); Névoa Pegajosa
  [controle] (Fumaça); Javali de Musgo [tanque]; Pombo-do-Louro Apagado [distância] (bicho real).
- **Chefe:** **O Velho da Névoa**, lagarto do tamanho de uma casa que ficou sem água; libertado, volta a guardar a mata.
- **Mecânica:** **névoa**: a cada 3 min a névoa sobe e esconde bichos a mais de 8 m; o Coletor de Névoa da cidade dá
  gotas por névoa atravessada.
- **Ganchos:** loot Gota de Névoa, madeira de louro · montaria nova **Lagarto-do-Louro** (§6) · casa: conjunto novo
  **Copas de Louro** (cadeira de cipó, lanterna de casulo).
- **Noite:** as pontes acendem; **Mardagayl** na mata baixa; vaga-lumes guiam a trilha.

### 06 · Jardim Esquecido de Mihrshah / *Mihrshah's Forgotten Garden* — nv 26–32 · ★
- **No jogo (2026-11-11): bicho próprio** no mundo contínuo (`EarlyStandInBeastRecipes`, página no Caderno, `EarlyStandInBeastsPlacementTests`): Pavão de Fumaça [distância] (`enemy_forest_smoke_peacock`, olho de brasa de 8 m) entra em "lobos_de_cinzas", no lugar do urutau.
- **No jogo (2026-11-03): jogável** (`MapLayouts.Zarafshan.cs`): posto Portão do Jardim (lâmpada, casa do jardineiro, cais) no portão oeste; o chahar bagh murado (muro de 4 lados com 4 portões, canais secos em cruz, fileiras de ciprestes, roseiras, 4 fontes secas) e o pavilhão de 8 colunas no centro (arena de Saklas; a luta segue como evento do mapa-múndi); oeste de volta ao 05; 4 clareiras, uma por quadrante, nv 26–32 (Aranha no lugar da Roseira-Espinho [controle], Lobo no lugar do Lobo de Cinzas [rápido], Ushabti de Pedra = Jardineiro de Barro [tanque]; à noite Faíscas, Coruja e Mardagayl). Falta: libertar as 4 faíscas para encher os canais e abrir o pavilhão, água que cura.
- **Fonte:** o jardim de luz que Mani mostra a Mihrshah (texto médio-persa M 47); o Velho Mihrshah (NPC das Grutas) o
  viu quando jovem.
- **Bioma e visual:** jardim murado persa (*chahar bagh*: quatro canteiros e canais em cruz) tomado pelo mato; azulejo
  `#3FA7A3`, rosa `#D96C8A`, cipreste `#2F5E3A`, pedra `#CFC2A8`; fontes secas, pavilhão de colunas.
- **Layout:** M. Quatro quadrantes com um canal seco cada; **posto: Portão do Jardim** (lâmpada e jardineiro); o
  pavilhão central é a arena do chefe do mundo. Retorno: Copas de Garajonay.
- **Inimigos:** Roseira-Espinho Apagada [controle] (invenção); Lobo de Cinzas [rápido] (os lobos de Saklas); Pavão de
  Fumaça [distância] (invenção); Jardineiro de Barro [tanque] (ushabti, Kemet).
- **Chefe:** **chefe do mundo: Saklas, o Lobo de Cinzas / o Rei Mardagayl** (arconte, já no código); o mapa é a casa
  dele fora do evento.
- **Mecânica:** **canais**: libertar as 4 faíscas dos quadrantes enche os canais de Água Viva e abre o pavilhão; a água
  acesa cura fora de combate.
- **Ganchos:** loot Pétala de Luz, conjunto antigo do jardim · montaria `mount_kirin` (segredo) · casa: conjunto novo
  **Chahar Bagh** (fonte de azulejo, banco do pavilhão).
- **Noite:** Saklas vira o Rei Mardagayl; as rosas brilham; visão boa só perto dos canais cheios.

### Região 2 — Areias Flamejantes (Deserto, Fogo Vivo) · mapas 07–12

Cidade de região: **Oásis de Qocho** (bíblia §2.2). Estilo `Oasis`. Bichos-base: Escorpião das Areias Flamejantes,
Enxame de Faíscas Presas (noite). Chefe do mundo: **Namrael** (mapa 12).

**No jogo (2026-11-03): mundo contínuo** (§0 item 10, `OverworldLayouts.Areias.cs`): um mapa só de 2400 × 2400 m
(x −1200…1200, z −1900…500) sem troca de cena; "Mover" para 07 ou 08 (ou o portão sul do Zarafshan) carrega a região.
- **Qocho** na praça (0, 0), perto da borda norte, com o lago do oásis a leste e a fila de karez descendo do norte; é a
  cidade de retorno de 07–10 (o **Tambor Rubro** é a de 11–12). O 07 (Caravançará do Ammo) e o 08 (Acampamento das
  Pirâmides, avenida de ushabtis, anel do Grande Ushabti) estão encaixados como estavam; 09–12 ganharam posto gerado
  (Boca do Karez, Poço das Tamareiras, Torre de Taipa) e o Tambor Rubro uma primeira versão da cidade dos orcs (Salão
  Grande com tambores, Lâmpada de Ashmunikal, Forja de Tambor, cais; estilo `Oasis` por enquanto, o `WarCamp` falta).
- **21 áreas** (faixa sobe com a distância de Qocho): Oásis de Qocho (07, 5–6), Estrada das Caravanas (07, 5–7, portão
  para o Pomar na borda norte), Ruínas dos Artífices (08, 6–9), Palmeiral do Karez (6–8), Dunas do Vento Quente (7–10),
  Salinas de Lop (8–11), Planície dos Karez (10–14), Oásis das Tamargueiras (10–14), Erg Dourado (11–15), Cânions
  Vermelhos (11–15, portão para o 13 na borda leste), Vale das Ossadas, Mesas do Fogo e Campo das Pirâmides de Meroé
  (12–16), Erg das Sete Cristas e Cânion do Eco (15–19), Cemitério de Caravanas e Erg do Sul (17–21), Galerias do Karez
  (09, 20–26), Dunas que Cantam (10, 22–28, **Água Fresca**), Ermos do Tambor Rubro (11, 24–32) e Trono de Namrael (12,
  30–38, **Proteção contra Veneno**): exigência e nível a pé pela barreira suave. ~435 clareiras com 2–3 grupos por
  área: Escorpião [tanque], Abutre de Brasa e Lanterna Quebrada [distância], ushabti (golem), Chacal Apagado [rápido],
  Cobra-de-Areia (08), Pedra que Rola e Orc Coroado de Āz [elite] (11), morcegos do karez, Gavião de Brasa (gavião-de-penacho); noite com Faíscas Presas, Mardagayl e Ubır.
- **Relevo próprio** (`OverworldRelief.Desert.cs`): dunas em crista (dorso longo contra o vento oeste, face de
  deslizamento curta, cristas sinuosas; 5 m nas dunas baixas, ~10 m no erg, ~13 m nas Dunas que Cantam), mesas de topo
  chato com paredes a pique (~20 m), platô em camadas cortado por cânions estreitos (cânions e ermos, ~15–17 m),
  três terraços de sal no Trono, "tells" em Meroé, salinas e oásis quase planos. Clareiras cujo platô inclinaria a trilha
  vizinha (dunas/mesas/cânions) são descartadas: trilhas seguem niveladas na largura.
- **Chão e cenário por área** (`OverworldTerrains.Desert.cs`, `OverworldDecorKit.Desert.cs`): areia, arenito vermelho,
  sal, barro do karez, ouro velho e sombra violeta; tamareiras (em fileiras no palmeiral e na beira d'água),
  tamargueiras, choupos-do-deserto e espinheiro; peças novas: ossadas de camelo, carroças quebradas, caravanas
  paradas (camelos, fardos de seda, cântaros), pirâmides de Meroé (11 grandes colocadas + menores espalhadas), colunas
  de lótus e ushabtis, fila de poços de karez, marcos de pedra, chaminés e arcos de arenito, cristais de sal vermelhos e
  o trono de sal, torres de taipa, crosta de sal, tendas de clã com tambor. Rios: lago de Qocho, canal do karez no
  palmeiral (ponte), poças de salmoura, lago das tamargueiras, poços que vertem nas galerias e o **lago em meia-lua**.
- **Orçamento medido** (2704 pedaços, .NET 6 fora do Editor): ~3,0 mil triângulos em média, pior 11,4 mil (anel de
  fundo leste, cinturão de choupos), geração ~0,67 ms em média, p95 ~1,2 ms. Testes: `AreiasOverworldTests`.
- **Falta:** Tambor Rubro numa mesa (a cidade precisa de y = 0). *(Corrigido em 2026-11-11: 09–12 já são `isBuilt: true`
  como zonas do mundo contínuo; o estilo `WarCamp` existe e o mundo contínuo usa a receita `CityTownRecipes.TamborRubro`;
  as regiões 3/4 já viraram mundo e o mundo costurado liga as bordas sem portão.)*

### 07 · Estrada das Caravanas / *Caravan Road* — nv 5–7 · ★
- **No jogo (2026-11-03): jogável** (`MapLayouts.Areias.cs`): estrada de 5 m oeste→leste, Caravançará do Ammo murado (cais, cisterna, camelos, fardos), desvio norte pela fila de karez e sul para o 08; 4 clareiras nv 5–7 (Escorpião [tanque]; Chacal Apagado [rápido]; Faíscas Presas no crepúsculo/noite, Mardagayl raro; Abutre de Brasa [distância], cospe brasa de 8 m). Atiradores [distância] mantêm distância: se o herói chega a ~3,5–4 m, recuam flanqueando até reabrir o alcance; só o projétil fere, cada um com seu som de tiro. **Tempestade de areia** (`AreiasStormRules`, em toda a Região 2): a cada 10 min, aviso de 15 s e 40 s de areia; fora de abrigo (fogueiras, carroças, ruínas, arcos de pedra, raio 14 m, coluna de luz no mais próximo) tira 1,5% da vida/s até o piso de 35%, nunca mata; cidades e os oásis não pegam. Falta: bicho coroado de Āz, visão 6 m e a caravana que pede escolta, o Ammo vendendo.
- **Fonte:** a Rota da Seda e os caravançarás; Ammo, o Caravaneiro (Mār Ammō, texto parto M 2).
- **Bioma e visual:** estepe seca virando areia; areia `#E7C27D`, arenito `#B5482F`, espinheiro `#8DAA3A`; marcos de
  pedra, bocas de karez em fila, carroças quebradas, fardos de seda.
- **Layout:** M. Estrada larga de oeste a leste com 2 desvios; **posto: Caravançará do Ammo** (pátio quadrado; o Ammo
  vende no dia dele); Qocho na ponta leste. Retorno: Oásis de Qocho.
- **Inimigos:** Escorpião das Areias Flamejantes [tanque]; Chacal Apagado [rápido] (bicho real); Abutre de Brasa
  [distância] (bicho real); bicho coroado do bando de Āz [elite] (bíblia §3.4).
- **Chefe:** nenhum; a **Invasão de Āz** passa por aqui.
- **Mecânica:** **tempestade de areia** (evento de hoje) a cada ~10 min: visão 6 m, a caravana para e pede escolta.
- **Ganchos:** loot Moeda Furada de Qocho, fardos · montaria `mount_pack_donkey`, `mount_rhea` · casa: **Caravana do Vô
  Jorge**.
- **Noite:** **Enxame de Faíscas Presas**; frio (o fôlego volta 20% mais devagar fora do caravançará).

### 08 · Ruínas dos Artífices / *Artificers' Ruins* — nv 6–9 · ★
- **No jogo (2026-11-03): jogável** (`MapLayouts.Areias.cs`): Acampamento das Pirâmides na entrada norte, avenida de ushabtis até o anel de colunas de lótus (arena do Grande Ushabti, luta segue como desafio do mapa-múndi), 3 estelas do painel, pirâmides de Meroé; 4 clareiras nv 6–9 (Ushabti de Pedra e Escorpião [tanque]; Cobra-de-Areia Apagada [rápido] entre os ushabtis; de noite variantes "acordadas" e Faíscas; Lanterna Quebrada dos Artífices [distância], raio de luz de 7 m que persegue um pouco). Atiradores [distância] mantêm distância: se o herói chega a ~3,5–4 m, recuam flanqueando até reabrir o alcance; só o projétil fere, cada um com seu som de tiro. Falta: leitura das inscrições.
- **Fonte:** pirâmides de Meroé (Kush) e ushabtis (Kemet, "os que respondem"); o Grande Ushabti (bíblia §3.3).
- **Bioma e visual:** deserto de pedra; ouro velho `#C9A23F`, sombra violeta `#6D4C7D`, arenito; pirâmides pequenas e
  pontudas com capela, colunas de lótus quebradas, ushabtis em fila.
- **Layout:** M. Avenida de ushabtis até a arena; **posto: Acampamento das Pirâmides** (tenda e lâmpada). Retorno:
  Oásis de Qocho.
- **Inimigos:** Ushabti de Areia [tanque]; Escorpião [tanque]; Cobra-de-Areia Apagada [rápido] (bicho real); Lanterna
  Quebrada dos Artífices [distância] (engenho, bíblia §1.6).
- **Chefe:** **O Grande Ushabti** (`enemy_desert_ruin_guardian`, de hoje).
- **Mecânica:** **o painel do aviso**: ler as 3 inscrições dá pistas do selo e o bônus da missão do Ícaro.
- **Ganchos:** loot peças de engenho, ouro velho · montaria `mount_golden_scarab`, `mount_griffin` (drop de chefe) · casa:
  **Areias Flamejantes** (mini-pirâmide de Meroé).
- **Noite:** os ushabtis "acordam" de olhos acesos; Namrael pode aparecer na lua cheia (evento de hoje).

### 09 · Galerias do Karez / *Karez Galleries* — nv 20–26 · ★
- **Fonte:** *karez* (qanat), galerias de água subterrânea de Turfan.
- **Bioma e visual:** túneis de barro sob o deserto; barro `#A97A52`, água `#6FB7B0`, raiz de parreira `#6D5A3A`, luz de
  poço `#F2D27A`; poços verticais com fachos de sol, canais estreitos.
- **Layout:** M (subterrâneo, 3 andares ligados por poços e cordas). **Posto: Boca do Karez** (a cisterna da Yardena).
  Retorno: Oásis de Qocho.
- **Inimigos:** Rato-do-Karez Gigante [enxame]; Escorpião-d'Água Apagado [rápido] (bicho real); Raiz Sedenta [controle]
  (raiz de parreira apagada que suga água, invenção); Ushabti Cavador [tanque].
- **Chefe:** **A Raiz que Bebeu o Poço**, parreira enorme apagada; libertada, devolve a água ao oásis.
- **Mecânica:** **fachos de sol**: só dentro deles o herói regenera mana; os bichos evitam a luz.
- **Ganchos:** loot barro fino, Gota de Água Viva · montaria `mount_giant_armadillo` (escavador: túneis) · casa: conjunto
  novo **Cisterna de Qocho** (cântaro, bica de pedra).
- **Noite:** sem fachos (só lâmpadas); **Ubır** desce pelos poços.

### 10 · Dunas que Cantam / *Singing Dunes* — nv 22–28 · ★
- **No jogo (2026-11-11): bicho próprio** no mundo contínuo (`EarlyStandInBeastRecipes`, página no Caderno, `EarlyStandInBeastsPlacementTests`): Lagarto-Espinho [rápido] (`enemy_desert_thorn_lizard`) lidera "lagartos_da_duna", no lugar do chacal. Falta: Verme de Areia Apagado [elite].
- **Fonte:** as dunas "que cantam" de Mingsha Shan ("montanha da areia que ecoa", perto de Dunhuang).
- **Bioma e visual:** mar de dunas altas; areia `#E7C27D`, sombra `#B98A4E`, céu `#F2A65A`; lago em meia-lua,
  tamareiras, pegadas que somem.
- **Layout:** G. Dunas em ondas, lago em crescente ao sul; **posto: Poço das Tamareiras**. Retorno: Oásis de Qocho.
- **Inimigos:** Escorpião Gigante [tanque]; Verme de Areia Apagado [elite, sai do chão] (invenção); Lagarto-Espinho
  [rápido] (bicho real); Faíscas Presas [enxame].
- **Chefe:** **O Coral da Duna**, verme que "canta" para chamar a luz; libertado, a duna canta bonito.
- **Mecânica:** **canto da duna**: escorregar duna abaixo faz a areia cantar e assusta os comuns por 4 s (a cada 30 s;
  não vale em chefe).
- **Ganchos:** loot Areia que Canta (tintura) · montaria `mount_fire_rhea` (Bema) · casa: **Areias Flamejantes**.
- **Noite:** areia fria e azulada; o Enxame de Faíscas Presas dobra; o lago reflete a Barca da Lua.

### 11 · Ermos do Tambor Rubro / *Red Drum Badlands* — nv 24–32 · ★ · cidade de raça (orcs)
- **No jogo (2026-11-03): cidade de verdade** (`CityTownRecipes.TamborRubro.cs`, estilo `WarCamp`): paliçada de troncos com ponta, Salão Grande de couro com telhado altíssimo e varas cruzadas, torre do tambor com a Lâmpada Guardiã, tendas de clã, postes de clã, fogueiras e arena de areia, tudo em y = 0 dentro de ±33 m; Gurparanzah, Ashmunikal, Kessi, Appu e Tushratta com visual de orc (presinhas, pele oliva/cinza) e falas para cada primo; Forja de Tambor (com a Teriaga do 12); missões "Caudas Vermelhas na Mesa" e "Gaviões de Brasa"; respawn na praça para 11–12. Falta: Ritmo do Tambor, conserto mais barato e Têmpera do Tambor. *(2026-11-11: o mundo contínuo das Areias já usa esta receita.)*
- **Fonte:** as Montanhas Flamejantes de arenito vermelho (Huoyan Shan); os nomes dos orcs vêm dos contos hurritas
  (§3.3).
- **Bioma e visual:** *badlands* de arenito em camadas; paleta do Tambor Rubro (§3.3); canyons, chaminés de pedra,
  arcos naturais, mesas.
- **Layout:** G. **Tambor Rubro** (§3.3) numa mesa no centro, rampas para os canyons; 3 acampamentos de clãs tocados por
  Āz nas bordas. Retorno: Tambor Rubro.
- **Inimigos:** Clã Coroado de Āz (orcs com coroinhas de lata, **nunca** a raça: vencidos, tiram a coroa e voltam para a
  cidade) [elite]; Escorpião-Rubro [tanque]; Gavião de Brasa [rápido] (bicho real); Pedra-Rolante Ushabti [controle].
- **Chefe:** **O Chefe Coroado**, o orc que mais ouro juntou para Āz; vencido, devolve a arca (Arca Resgatada +2) e pede
  desculpas no Salão Grande.
- **Mecânica:** o **Ritmo do Tambor** da cidade (§3.3) e o **eco dos canyons**: bichos ouvem passos a 2× a distância.
- **Ganchos:** loot couro rubro, Ferro de Tambor · montaria nova **Lobo de Guerra Ocre** (§6) · casa: conjunto novo
  **Salão do Tambor**.
- **Noite:** fogueiras nas mesas; os clãs coroados fazem ronda; Enxame de Faíscas Presas nos canyons.

### 12 · Trono de Namrael / *Namrael's Throne* — nv 30–38 · ★
- **Fonte:** Namrael (Namraēl/Nebrōēl), a arconte companheira de Saklas (Teodoro bar Konai).
- **Bioma e visual:** planalto de sal e cristal vermelho; sal `#F0E6DA`, cristal `#C2453A`, sombra violeta; torres de
  taipa, costelas de pedra (fósseis), trono de sal.
- **Layout:** G. Três terraços de sal até o trono; **posto: Torre de Taipa** (vigias de Qocho). Retorno: Tambor Rubro.
- **Inimigos:** Escorpião-Cristal [tanque]; Serpente de Sal [rápido]; Guarda de Namrael (escorpião com coroinha)
  [elite]; Espelho de Sal [distância, reflete projéteis] (invenção).
- **Chefe:** **chefe do mundo: Namrael, o Escorpião-Rei / da Lua Cheia** (já no código).
- **Mecânica:** **sal que ofusca**: ao meio-dia o sal brilha e corta a visão a 10 m (aqui o dia é o perigoso).
- **Ganchos:** loot Cristal de Sal Rubro (refino) · montaria `mount_light_jaguar` (noite) · casa: troféu de Namrael.
- **Noite:** visão boa sob a lua; Namrael só na lua cheia; Ubır nas torres.

### Região 3 — Serra de Kaggen (Montanha, Vento Vivo) · mapas 13–18

Cidade de região: **Vila de Litema** (bíblia §2.3). Estilo `Rondavel`. Bichos-base: Ushabti de Pedra, Ave-Graveto de
Omöl (noite). Chefe do mundo: **Mazan** (mapa 18). Respeito: San e basotho são povos vivos; os inimigos daqui são
gigantes, ushabtis e bichos (bíblia §2.3).

**No jogo (2026-11-03): mundo contínuo** (§0 item 10, `OverworldLayouts.Kaggen.cs`): um mapa só de 2400 × 2400 m
(x −1700…700, z −900…1500) sem troca de cena; "Mover" para 13 ou 14 (ou o portão oeste do Zarafshan) carrega a região.
- **Vila de Litema** na praça (0, 0), perto da borda leste, com a Trilha dos Rondavéis (13) encaixada em volta (cidade
  de retorno de 13–16); o **Passo do Hahya** (14) encaixado a nordeste com a Boca da Mina do Mihr, sua torrente e o
  anel de Hahya. 15–18 ganharam posto gerado no estilo `Rondavel` (Abrigo do Eland, Recinto Curvo, Estela Alta) e os
  **Salões de Bronze de Napirasu** (17) uma primeira versão da cidade dos anões (Grande Forja, casa das runas, casa do
  bronze, cais, estelas; cidade de retorno de 17–18: morrer ali acorda na praça dos Salões). 15–18 seguem sem layout
  próprio no catálogo (`isBuilt` falso): chega-se a pé pela Serra; a barca ainda não vai até eles.
- **15 áreas** (faixa sobe com a distância de Litema): Trilha dos Rondavéis (13, 9–11), Terraços de Litema (10–12,
  portão para o 08 na borda sul), Encosta do Vento (10–12, portão para o Zarafshan na borda leste), Capinzal dos
  Baobás (10–13), Torrentes de Qacha (11–14), Passo do Hahya (14, 10–13, **Manta de Lã**, galeria para o 19 na borda
  leste), Pastos dos Currais (13–17), Passos Estreitos (15–20), Escarpas de Arenito (18–24), Campos de Neve (22–28),
  Abrigos Pintados (15, 26–32), Muralhas de Pedra Seca (16, 28–34), Salões de Bronze (17, 30–38, **Picareta e
  Tocha**), Estela Alta e Cume dos Mazans (18, 36–44, **Resistência à Nevasca**): exigência e nível a pé pela barreira
  suave. ~400 clareiras com 2–3 grupos por área, só bichos de hoje: Ushabti de Pedra [tanque], Gavião-de-Penacho
  [distância], Ave-Graveto (urutau) [distância/controle], Babuíno Apagado [rápido], Pedra que Rola e Neblina do Vento
  Ruim [controle], lobo no lugar do Lince/Borrão [rápido], javali,
  morcegos da mina [enxame]; à noite variantes, Ave-Graveto de Omöl (Mãe-da-Lua), Coruja-de-Fumaça, Mardagayl e
  Morcego-Vampiro.
- **Relevo de montanha** (`OverworldTerrains.Mountain.cs`): colinas douradas suaves em volta de Litema (~12 m),
  terraços agrícolas em 4 degraus, encostas de aloés e passo do Hahya em degraus (~17 m), gargantas das torrentes,
  paredões dos Passos Estreitos (~25 m), escarpas de arenito em 3 degraus (~30 m), campos de neve (~33 m) e o cume
  nevado (~41 m). Onde há cidade, portão ou torrente o relevo fica em ≤ 18 m (rampas e encostas de vale caminháveis);
  trilhas niveladas na largura, nenhum trecho acima de ~25°. Clareiras geradas a menos de 12,5 m de uma trilha ou em
  chão inclinado são descartadas (o platô delas não inclina mais a trilha).
- **Chão e cenário por área** (`OverworldDecorKit.Mountain.cs`, `OverworldChunkBuilder.Mountain.cs`): capim dourado,
  barro vermelho, granito, arenito, líquen, basalto frio e neve; cedros retorcidos, aloés-árvore e baobás (também na
  beira d'água; o fundo além dos muros é só de cedros por orçamento), touceiras de aloé, protea; peças novas: rondavéis
  de pastor com faixa de litema, currais redondos de pedra seca, muros de pedra seca (curvos nas Muralhas), degraus de
  terraço com sorgo, abrigos de arenito com painel de elands (só a imagem do bicho), lajes dos paredões, torres
  cônicas, pilares de runas com capitel de bronze (runa acende à noite), blocos de granito dos Mazans, a estela alta,
  montes de neve e bancos de neblina; marcos de pedras empilhadas. Água: a Torrente de Qacha (3 pontes e um vau) e a
  torrente do Hahya prolongada até a borda sul (ponte na estrada do Zarafshan), três lagoas de degelo.
- **Orçamento medido** (2704 pedaços, .NET 6 fora do Editor): ~5,2 mil triângulos em média, pior 10,7 mil (meta ≤ 12
  mil); geração média ~0,74 ms, p95 ~1,1 ms; montar a região ~0,12 s. Testes em `OverworldKaggenTests` (EditMode).
- **Falta:** pontes de corda que balançam (visual), ar fino no cume, NPCs dos Salões de Bronze. *(Corrigido em 2026-11-11:
  15–18 já são `isBuilt: true` (zonas, barca até elas); ventos da serra e pontes de corda que gastam fôlego estão em
  `KaggenWindRules`; Pedra que Rola e Neblina do Vento Ruim são bichos próprios (bíblia §3.2.1); o mundo costurado liga a
  Serra às Areias sem portão.)*

### 13 · Trilha dos Rondavéis / *Rondavel Trail* — nv 9–11 · ★
- **No jogo (2026-11-03): jogável** (`MapLayouts.Kaggen.cs`): a Vila de Litema inteira no platô (cidade de retorno, respawn na praça), trilha em zigue-zague, 2 currais de pedra seca, abrigos pintados e baobá; 4 clareiras nv 9–11 (Ushabti de Pedra [tanque]; Pedra que Rola [controle]; Babuíno Apagado e Javali [rápido]; de noite Ave-Graveto e Coruja-de-Fumaça; Gavião-de-Penacho [distância], pena-dardo de 9 m). Atiradores [distância] mantêm distância: se o herói chega a ~3,5–4 m, recuam flanqueando até reabrir o alcance; só o projétil fere, cada um com seu som de tiro. **Ventos da serra** (`KaggenWindRules`): nas Encosta do Vento e Passo do Hahya, a cada 25 s, aviso de 3 s e rajada de 2,5 s (empurra 2,4 m/s quem anda; parado passa; a Maithe ignora). Falta: curvar projéteis.
- **Fonte:** *litema* (pintura mural basotho); rondavéis das terras altas; \|Kaggen, o louva-a-deus dos contos \|xam.
- **Bioma e visual:** capim alto dourado e encostas; barro `#A0522D`, cal `#EFE6D8`, palha `#C8A85A`, cobertor
  `#2F4F8F`/`#E3B23C`; aloé, protea.
- **Layout:** M. **Vila de Litema** (cidade) no platô; trilha em zigue-zague; 2 currais. Retorno: Vila de Litema.
- **Inimigos:** Ushabti de Pedra [tanque]; Babuíno Apagado [rápido] (bicho real); Gavião-de-Penacho [distância] (bicho
  real); Pedra que Rola [controle].
- **Chefe:** nenhum (o chefe é o 14).
- **Mecânica:** **ventos da serra**: rajadas laterais curvam um pouco os projéteis; a Maithe (Vento Vivo) não sofre.
- **Ganchos:** loot pedra, lã · montaria `mount_mountain_goat` · casa: **Picos de Litema**.
- **Noite:** **Ave-Graveto de Omöl**; frio leve; vila iluminada.

### 14 · Passo do Hahya / *Hahya's Pass* — nv 10–13 · ★
- **No jogo (2026-11-11): bicho próprio** no mundo contínuo (`EarlyStandInBeastRecipes`, página no Caderno, `EarlyStandInBeastsPlacementTests`): Lince Apagado [rápido] (`enemy_mountain_dimmed_lynx`) lidera "lince" nos Passos Estreitos (dono: 14), no lugar do lobo.
- **No jogo (2026-11-03): jogável** (`MapLayouts.Kaggen.cs`): posto Boca da Mina do Mihr (forja, rondavéis, cais), passo estreito entre muralhas de pedra, torrente com 2 pontes, anel de pedras de Hahya no alto, galeria para o 19; 4 clareiras nv 10–13 (Ushabti [tanque], Lobo no lugar do Lince [rápido], Enxame de Morcegos = Morcego da Mina [enxame], Neblina do Vento Ruim [controle] na garganta; Ubır, Ave-Graveto e Coruja à noite; Gavião-de-Penacho [distância]). Atiradores [distância] mantêm distância: se o herói chega a ~3,5–4 m, recuam flanqueando até reabrir o alcance; só o projétil fere, cada um com seu som de tiro. **Pontes de corda** (`KaggenWindRules`): correr nas 6 pontes da região gasta o dobro do fôlego; a rajada nunca derruba ninguém delas; a Maithe ignora. Falta: neve no chão.
- **Fonte:** Hahya, o gigante do *Livro dos Gigantes* que sonha e se arrepende (bíblia §3.3).
- **Bioma e visual:** passo de pedra com neve; granito `#8A8A84`, neve `#DDE8F0`; ponte de corda, boca de mina, forja.
- **Layout:** M. Passo estreito com 2 pontes; **posto: Boca da Mina do Mihr**; arena do chefe no alto. Retorno: Vila de
  Litema.
- **Inimigos:** Ushabti de Pedra [tanque]; Neblina do Vento Ruim [controle] (Vento Destruidor); Lince Apagado [rápido];
  Morcego da Mina [enxame].
- **Chefe:** **Hahya, o Gigante Guerreiro** (`enemy_mountain_warlord`, de hoje).
- **Mecânica:** **pontes de corda** que balançam com o vento: correr nelas gasta fôlego mais rápido.
- **Ganchos:** loot ferro, minério · montaria `mount_snow_tiger` · casa: **Picos de Litema**.
- **Noite:** neve azul; Ave-Graveto; a boca da mina acende.

### 15 · Abrigos Pintados / *Painted Shelters* — nv 26–32 · ★
- **No jogo (2026-11-11): bicho próprio** no mundo contínuo (`EarlyStandInBeastRecipes`, página no Caderno, `EarlyStandInBeastsPlacementTests`): Borrão [rápido] (`enemy_mountain_smudge`) lidera "borroes", no lugar do lobo (o Babuíno Ladrão segue no grupo).
- **Fonte:** pinturas de rocha com elands e gente dançando (Drakensberg); o eland nos contos \|xam (**só a imagem e o
  conto de bicho, nunca rito**).
- **Bioma e visual:** paredões de arenito com abrigos rasos; arenito `#C88A5A`, ocre `#9E4B2A`, branco de pintura
  `#F2EBDD`, capim; cascatas finas.
- **Layout:** M. Uma trilha ao pé do paredão com 6 abrigos; **posto: Abrigo do Eland**. Retorno: Vila de Litema.
- **Inimigos:** Borrão (a sombra que sai das pinturas e as apaga) [rápido] (invenção); Babuíno Ladrão [enxame]; Ushabti
  de Arenito [tanque]; Águia-Preta Apagada [distância] (bicho real).
- **Chefe:** **O Grande Borrão**, sombra que comeu as cores das paredes; vencido, as pinturas voltam.
- **Mecânica:** **restaurar**: em cada abrigo, tocar os 3 pontos certos (pistas no Caderno) faz o eland pintado "correr"
  e guiar até um baú.
- **Ganchos:** loot ocre (tintura) · montaria nova **Eland do Capim Alto** (§6) · casa: **Picos de Litema** (Pedra pintada
  do eland).
- **Noite:** as pinturas brilham fraco; Ave-Graveto; nada de dança noturna (respeito).

### 16 · Muralhas de Pedra Seca / *Drystone Walls* — nv 28–34 · ★
- **No jogo (2026-11-11): bicho próprio** no mundo contínuo (`EarlyStandInBeastRecipes`, página no Caderno, `EarlyStandInBeastsPlacementTests`): Gralha de Pedra [distância] (`enemy_mountain_stone_jackdaw`, pedra de 8,5 m) lidera "gralhas_de_pedra" com o gavião, no lugar do urutau. Falta: Lagarto-de-Pedra [rápido] (ainda o lobo).
- **Fonte:** muralhas curvas do Grande Zimbábue (shona), com friso em espinha de peixe; estelas de Aksum.
- **Bioma e visual:** colinas de granito com recintos murados; granito, líquen `#9DAA5A`, capim; torre cônica de pedra,
  passagens estreitas.
- **Layout:** G. Recinto grande em elipse com passagens em labirinto; **posto: Recinto Curvo**. Retorno: Vila de Litema.
- **Inimigos:** Ushabti de Granito [tanque]; Mazan Menor [elite] (*mazanān*); Lagarto-de-Pedra [rápido] (bicho real);
  Gralha de Pedra [distância].
- **Chefe:** **O Pedreiro Mazan**, gigante que quer murar a serra inteira; libertado, ajuda a consertar os muros.
- **Mecânica:** **muros móveis**: alavancas de pedra giram trechos de muro e mudam o labirinto (atalhos).
- **Ganchos:** loot Pedra Seca, granito · montaria `mount_woolly_mammoth` (drop raro) · casa: conjunto novo **Recinto de
  Pedra Seca**.
- **Noite:** os muros escondem bichos (visão curta nas curvas); Ave-Graveto.

### 17 · Salões de Bronze de Napirasu / *Napirasu's Bronze Halls* — nv 30–38 · ★ · cidade de raça (anões)
- **No jogo (2026-11-03): cidade de verdade** (`CityTownRecipes.BronzeHalls.cs`, estilo `RuneHall`): salões com a frente escavada no granito, pilares quadrados com base e capitel de bronze, portas maciças de bronze com cravos sob o lintel de runa (uma linha acesa por vez), a Grande Forja apagada (cristal da Luz escuro, fole de bronze, bigornas), poços de lava apagada, elevadores de corrente e a parede de rocha da montanha ao norte e a leste, tudo em y = 0 dentro de ±19 m (o lugar que o mundo contínuo reserva: x −20…21, z −19…20); Napirasu, Untash, Puzur, Shilhak e Kidin com visual de anão (baixos e largos, barba trançada com contas de bronze) e falas para cada primo; Portões de Runa do Puzur (runas de bronze, Picareta e Lamparina do 17, Chá de Gengibre do 18); missões "Escória na Escada" e "Morcegos no Veio"; respawn na praça para 17–18. Falta: gravar 1 runa por dia, as 5 etapas da Grande Forja. *(2026-11-11: o mundo contínuo da Serra já usa esta receita.)*
- **Fonte:** nomes elamitas (§3.3); a estátua de bronze de Napirasu; o ferreiro celeste Kurdalægon como lenda da forja.
- **Bioma e visual:** dentro da montanha; paleta dos Salões (§3.3); lava apagada, pilares, runas.
- **Layout:** G (3 andares). **Salões de Bronze** (§3.3) no andar do meio; minas em cima, a Grande Forja embaixo.
  Retorno: Salões de Bronze.
- **Inimigos:** Golem de Escória [tanque] (ushabti mal fundido); Salamandra Fria [rápido] (fogo apagado, invenção);
  Morcego de Brasa [enxame]; Runa Errada [controle] (runa que acende sozinha e prende).
- **Chefe:** **O Fole Apagado**, o engenho que soprava a forja, tomado pela sombra; libertado, acende a 1ª etapa da
  Grande Forja.
- **Mecânica:** a **Grande Forja** (§3.3): cada etapa acesa abre um serviço.
- **Ganchos:** loot bronze, Faísca da Forja, runas · montaria nova **Carneiro de Bronze** (§6) · casa: conjunto novo
  **Salão de Runas**.
- **Noite:** igual de dia (subterrâneo); na "noite do relógio" os anões cantam e a loja de runas dá desconto.

### 18 · Cume dos Mazans / *Mazan Summit* — nv 36–44 · ★
- **Fonte:** *māzan/mazanān*, gigantes dos textos médio-persas maniqueus; a estela alta de Aksum no topo.
- **Bioma e visual:** pico nevado acima das nuvens; neve, granito, céu `#A9C8E8`; estela de 20 m, blocos gigantes.
- **Layout:** G. Subida em espiral com 3 platôs; **posto: Estela Alta** (abrigo de pastores). Retorno: Salões de Bronze.
- **Inimigos:** Mazan [elite]; Ushabti de Gelo [tanque]; Águia da Tempestade [distância]; Neblina do Vento Ruim
  [controle].
- **Chefe:** **chefe do mundo: Mazan, o Gigante da Serra / da Pedra-Lua** (já no código).
- **Mecânica:** **ar fino**: no platô de cima o fôlego volta 30% mais devagar; a Maithe ignora.
- **Ganchos:** loot Pedra-Lua, troféu de Mazan · montaria `mount_hippogriff` · casa: **Picos de Litema**.
- **Noite:** Mazan da Pedra-Lua; aurora fraca; Ave-Graveto no gelo.

### Região 4 — Grutas do Limiar (Cavernas, Luz) · mapas 19–24

Cidade de região: **Vila das Lamparinas** (bíblia §2.4). Estilo `CaveTuff`. Bichos-base: Enxame de Morcegos do Limiar,
Ubır (noite). Chefe do mundo: **Mahaway** (mapa 24).

**No jogo (2026-11-03): mundo contínuo** (§0 item 10, `OverworldLayouts.Limiar.cs`): um mapa só de 2400 × 2400 m
(x −1200…1200, z −2100…300) sem troca de cena; "Mover" para 19 ou 20 (ou o portão do Zarafshan / do Hahya) carrega a
região. Quanto mais fundo (sul) e mais longe da Vila, mais difícil.
- **Vila das Lamparinas** na praça (0, 0), na parede norte, com o Lago do Limiar (19) encaixado como estava (lago,
  túneis de tufo); o **Salão de Tages** (20) encaixado a leste com o Nicho da Ponte, o riacho e o anel de Tages. 21, 23 e
  24 ganharam posto gerado em tufo (Vishapakar Grande, Torre das Jarras, Última Ponte de Corda) e o **Mercado dos
  Túneis** (22) uma primeira versão da cidade dos kobolds no cruzamento das quatro galerias (barracas, lâmpadas, cais;
  cidade de retorno de 22–23: morrer ali acorda na praça do Mercado; 19–21 e 24 acordam na Vila).
- **27 áreas** em 5 fileiras de salões: Lago do Limiar (19, 13–15), Boca do Hahya (14–16), Galeria do Hahya e Galeria
  da Ravina (14–16, compridas sob a parede norte até os portões: **oeste → 14**, **leste → Zarafshan**), Salão de Tages
  (20, 14–18), Colunata do Lago (15–20, portão gradeado do **Ossuário dos Artífices**: leva ao mapa da ilha, onde a
  masmorra abre), Campo de Cristais e Lago dos Peixes-Lanterna (17–22), Fendas Ocidentais e Terraços de Tufo (19–24),
  Bosque de Cogumelos e Abismo Oriental (20–26), Jardim de Fungos (22–27), Prados de Musgo e Galerias Rosadas (23–28),
  Ravina de Cristal e Fenda do Bronze (27–32), Campo das Pedras-Dragão e Fontes do Vishap (21, 32–38, **Lamparina
  Rúnica**, as 12 vishapakar), Mercado dos Túneis (22, 34–40), Cânion da Roda e Aquedutos da Roda (23, 38–46, **Óleo
  do Fogo Eterno**, a roda de doze jarras no seu poço), Ninho de Mahaway e Ninhos Ocidentais (24, 42–50, **Asas ou Pulo
  Duplo**, o poço gigante com ninhos na borda), Escarpas da Revoada, Aquedutos Profundos e Abismo sem Fundo (42–50).
  Os salões de 19/20 pedem **Tocha** (barreira suave). ~380 clareiras com 2–3 grupos por área, só bichos de hoje:
  Enxame de Morcegos [enxame], **Peixe-Lanterna Apagado** [distância], aranha (Aranha de Gruta/Kobold tocado),
  ushabti (Pedra-Lâmina, Sapo de Pedra, Filho de Ullikummi), Lodo de Gruta (19), Lanterna Quebrada (Jarra Rachada), Corvo-da-Treva (24),
  gavião-de-penacho (Revoada), javali (Toupeira-Gigante), escorpião (Lagarta de Pedra), Faíscas (Fumaça de Mina, esporos);
  à noite variantes e **Ubır** (morcego-vampiro) em toda clareira. Clareiras cujo platô inclinaria uma trilha são
  descartadas (como na Serra).
- **Relevo de caverna** (formas do `OverworldRelief`): degraus de tufo em 4 níveis nos Terraços (~18 m), 5 níveis no
  Ninho (~24 m), fendas/gargantas nos abismos e no cânion (~15 m), montículos de estalagmite no Salão, morros nos
  fungos e na boca, margens quase planas nos lagos e no mercado.
- **Cenário por área** (`OverworldTerrains.Caves.cs`, `OverworldDecorKit.Caves.cs`, `OverworldChunkBuilder.Caves.cs`):
  chão de basalto, ardósia, areia negra, índigo dos cristais, tufo rosa e musgo; **paredes altas de tufo** nas bordas
  entre salões (8–13 m nas norte–sul, ressaltos de 3–5 m nas leste–oeste, do lado da câmera), com vãos em toda trilha,
  rio, clareira e em trechos de ruído; abóbada de paredes de 14–23 m fora das bordas norte/leste/oeste (sem colisor);
  **abismos** (leitos cavados desenhados como vazio negro) com pontes e postes de corda nas duas pontas; estalagmites,
  aglomerados de cristal, colunas naturais, cogumelos gigantes, vishapakar, vagonetes e tubos de ar (22), jarras de
  bronze (23), ninhos (24), nichos de lâmpada à beira das trilhas, a roda das doze jarras e o portal do Ossuário. **Luz**:
  só as células emissivas da paleta (musgo, cogumelo/cristal azul, lâmpadas), que as cidades da região mantêm acesas
  (`AmbientNight` 0,65 em todas).
- **Orçamento medido** (2704 pedaços, .NET 6 fora do Editor): ~3,7 mil triângulos em média, pior 9,6 mil, geração
  ~0,7–1,1 ms em média, p95 ~1,0–1,7 ms. Testes: `OverworldLimiarTests`.
- **Falta:** o Ossuário abre pelo mapa da ilha (portal leva para
  lá: entrada direta pede gancho no `DungeonDirector`); célula emissiva própria para cristal (hoje usa a do cogumelo
  azul); barcos do lago, poços de vento do 24. *(Corrigido em 2026-11-11: 21–24 já são `isBuilt: true`; o Mercado usa a
  receita `TunnelMarket`, estilo `TunnelBazaar`; as lâmpadas que salvam o caminho estão em `LimiarLampRules`.)*

### 19 · Lago do Limiar / *Threshold Lake* — nv 13–15 · ★
- **No jogo (2026-11-03): jogável** (`MapLayouts.Limiar.cs`): a Vila das Lamparinas inteira na parede norte (cidade de retorno), lago escuro com trilha da margem, 2 túneis entre paredes de tufo (oeste → 14, leste → 20); 5 clareiras nv 13–15 (Morcegos [enxame], Aranha-Tecelã no lugar da Aranha de Gruta [rápido], Lodo de Gruta [tanque]; Ubır sai do lago à noite; Peixe-Lanterna Apagado [distância], gota de luz fria de 7,5 m que persegue). Atiradores [distância] mantêm distância: se o herói chega a ~3,5–4 m, recuam flanqueando até reabrir o alcance; só o projétil fere, cada um com seu som de tiro. **Lâmpadas** (`LimiarLampRules`, salvas no save como `LimiarLamps`): 8 pontos com facho (Vila + 7 nos caminhos das fileiras 1–2); [E] acende e fica aceso; de uma acesa, [E] volta à Vila; da Vila, [E] vai até a mais funda acesa (intervalo de 20 s; nada bloqueia quem não acendeu). Falta: barcos.
- **Fonte:** o limiar da Terra da Treva; casas de gruta armênias (Khndzoresk); vishapakar.
- **Bioma e visual:** caverna grande com lago; tufo rosa `#C98F86`, basalto `#3B3B44`, âmbar `#FFB347`, musgo
  fluorescente; ponte suspensa, nichos de lâmpada.
- **Layout:** M. **Vila das Lamparinas** (cidade) na parede norte; lago com barcos; 2 túneis. Retorno: Vila das
  Lamparinas.
- **Inimigos:** Enxame de Morcegos do Limiar [enxame]; Peixe-Lanterna Apagado [distância] (bicho real); Aranha de Gruta
  [rápido]; Lodo de Gruta [tanque].
- **Chefe:** nenhum (o chefe é o 20).
- **Mecânica:** **lâmpadas**: cada lâmpada acesa no caminho fica acesa (salva) e encurta a volta da cidade.
- **Ganchos:** loot óleo, pavio, cristal · montaria `mount_carbuncle` · casa: **Tufo Rosa das Lamparinas**.
- **Noite:** igual (subterrâneo); na noite do relógio, **Ubır** sai do lago.

### 20 · Salão de Tages / *Tages' Hall* — nv 14–18 · ★
- **No jogo (2026-11-11): bicho próprio** no mundo contínuo (`EarlyStandInBeastRecipes`, página no Caderno, `EarlyStandInBeastsPlacementTests`): Ubır Jovem [rápido] (`enemy_cave_young_ubir`) lidera "degraus_rosados" nos Terraços de Tufo (dono: 20), no lugar da aranha. Falta: Sombra Pequena, Pedra-Lâmina.
- **No jogo (2026-11-03): jogável** (`MapLayouts.Limiar.cs`): posto Nicho da Ponte (nicho da lâmpada, casas de tufo, cais, lâmpadas sobre a rua), riacho subterrâneo com ponte e vau, corredor de colunas até o salão e o anel de pedras de Tages (luta segue como desafio do mapa-múndi); 4 clareiras nv 14–18 (Morcegos [enxame], Aranha no lugar da Sombra Pequena [rápido], Ushabti no lugar da Pedra-Lâmina [controle]; Peixe-Lanterna Apagado e Lanterna Quebrada [distância]; Ubır à noite). Atiradores [distância] mantêm distância: se o herói chega a ~3,5–4 m, recuam flanqueando até reabrir o alcance; só o projétil fere, cada um com seu som de tiro. Falta: medo do escuro (apagar/acender lâmpadas).
- **Fonte:** Tages, o ser etrusco de rosto de criança e sabedoria de velho (Cícero, *De Divinatione* II.50).
- **Bioma e visual:** salão de estalactites; cristal `#9FD3E6`, índigo `#232046`, âmbar; colunas naturais, o sulco de
  terra de onde Tages brotou.
- **Layout:** M. Corredor de colunas até o salão; **posto: Nicho da Ponte**. Retorno: Vila das Lamparinas.
- **Inimigos:** Morcegos [enxame]; Ubır jovem [rápido]; Pedra-Lâmina [controle]; Sombra Pequena [rápido].
- **Chefe:** **Tages, o Habitante Ancestral** (`enemy_cave_dweller`, de hoje).
- **Mecânica:** **medo do escuro**: apagar uma lâmpada deixa os bichos de perto mais fortes; acender acalma.
- **Ganchos:** loot cristal, Lâmpada Antiga · montaria `mount_dragon_hatchling` (ovo, segredo) · casa: **Tufo Rosa**.
- **Noite:** igual; o salão "suspira" e as lâmpadas tremem.

### 21 · Campo das Pedras-Dragão / *Dragonstone Field* — nv 32–38 · ★
- **Fonte:** *vishapakar*, pedras-dragão armênias pré-cristãs; vishaps e o **nhang** (serpente do rio), do folclore
  armênio.
- **Bioma e visual:** gruta alta com fontes e prados de musgo; basalto, musgo `#6E8F4A`, água `#7FC6D9`; pedras compridas
  com cabeça de peixe entalhada, em pé e deitadas.
- **Layout:** G. Prado com 12 vishapakar; **posto: Vishapakar Grande** (lâmpada na pedra maior). Retorno: Vila das
  Lamparinas.
- **Inimigos:** Vishap Jovem [elite]; Enguia de Fonte [rápido]; Sapo de Pedra [tanque]; Nhang [controle, puxa para a
  água].
- **Chefe:** **O Vishap Adormecido**, dragão d'água que dormia sob a pedra maior; libertado, abre as fontes.
- **Mecânica:** **pedras que acordam**: tocar uma vishapakar acende a fonte dela (cura), mas acorda um vishap jovem.
- **Ganchos:** loot Escama de Vishap · montaria `mount_river_seahorse` · casa: **Tufo Rosa** (Vishapakar de jardim).
- **Noite:** as fontes ficam escuras; o nhang canta (perto dele o herói anda lento).

### 22 · Mercado dos Túneis / *Tunnel Market* — nv 34–42 · ★ · cidade de raça (kobolds)
- **No jogo (2026-11-03): cidade de verdade** (`CityTownRecipes.TunnelMarket.cs`, estilo `TunnelBazaar`): barracas encaixadas em nichos de basalto sob arco redondo (prateleiras de garrafas, toldo listrado em postes de cobre, lanternas de vidro de garrafa), trilhos de vagonete nas quatro galerias, vagonetes, roldana, canos de ar de cobre com cata-vento dentro do tubo, barraquinhas de nicho e a parede redonda dos túneis (cano de cobre e lâmpadas de garrafa) ao norte e a leste, tudo em y = 0 dentro de ±20 m (o lugar que o mundo contínuo reserva: x −21…22, z −20…21), com as quatro ruas onde chegam as galerias; Miwnay, Shayn, Farnkhund e Takhsich com visual de kobold (pequenos, focinho, escamas ferrugem/ardósia) e falas para cada primo; Feira da Miwnay (diária, com o Óleo do Fogo Eterno do 23, as Asas de Lona do Takhsich do 24 e a Garrafa de Ar Sereno `item_garrafa_ar_sereno`: +20% de recuperação de vigor por 60 s); missões "Teia nos Tubos" (dá 2 Garrafas de Ar Sereno) e "Pedras nos Trilhos"; respawn na praça para 22–23. Falta: a Garrafa limpar lentidão, Banca de Troca e a feira que muda de galeria por dia. *(2026-11-11: o mundo contínuo das Grutas já usa esta receita.)*
- **Fonte:** nomes sogdianos das *Cartas Antigas* (§3.3); minas de lápis-lazúli e de sal das montanhas.
- **Bioma e visual:** rede de túneis com trilhos; paleta do Mercado (§3.3); vagonetes, tubos de ar, barracas em nichos.
- **Layout:** G (4 galerias em cruz). **Mercado dos Túneis** (§3.3) no cruzamento; a feira muda de galeria por dia.
  Retorno: Mercado dos Túneis.
- **Inimigos:** Kobold Tocado por Āz (alguns, nunca o povo) [rápido, rouba brilho]; Toupeira-Gigante [tanque]; Fumaça de
  Mina [controle] (Fumaça); Lagarta de Pedra [enxame].
- **Chefe:** **O Engarrafador Guloso**, kobold que guardou todo o Ar Sereno para Āz; vencido, solta as garrafas e vira
  ajudante da feira.
- **Mecânica:** **ar parado**: longe dos tubos o fôlego cai devagar; as Garrafas de Ar Sereno resolvem.
- **Ganchos:** loot sucata, lápis-lazúli, garrafas · montaria `mount_giant_anteater` · casa: conjunto novo **Bazar dos
  Túneis** (lanterna de garrafa).
- **Noite:** metade do mercado fecha; os tocados por Āz saem em dobro.

### 23 · Cânion da Roda / *Wheel Canyon* — nv 38–46 · ★
- **Fonte:** a roda de doze jarras que tira a luz do mundo (*Acta Archelai* 8); o zigurate elamita de Chogha Zanbil (a
  Torre das Jarras, sala de evento de hoje); Ullikummi, o gigante de diorito (*Canção de Ullikummi*).
- **Bioma e visual:** cânion subterrâneo com a roda d'água de 12 jarras; tufo, bronze, água de luz `#BFE8F0`; aquedutos,
  degraus da torre.
- **Layout:** G. A roda no meio; aquedutos para 12 nichos; **posto: Torre das Jarras** (o Adda; entrada da sala de
  evento). Retorno: Mercado dos Túneis.
- **Inimigos:** Filho de Ullikummi (gigante de diorito menor) [elite]; Ushabti de Bronze [tanque]; Gota de Treva
  [rápido]; Jarra Rachada [distância, espirra água].
- **Chefe:** gancho: a sala de evento **Torre das Jarras** (já no código) passa a ter a entrada aqui.
- **Mecânica:** **encher as jarras**: faíscas libertadas no mapa enchem jarras; a roda gira e muda o nível da água
  (caminhos novos).
- **Ganchos:** loot Jarra de Ouro (sala), bronze · montaria `mount_elder_tortoise` (conquista) · casa: conjunto novo
  **Roda das Doze Jarras** (miniatura).
- **Noite:** a roda brilha; Ubır nos aquedutos.

### 24 · Ninho de Mahaway / *Mahaway's Roost* — nv 42–50 · ★
- **Exigência (no jogo, 2026-11-03):** Asas ou Pulo Duplo: vestir nas costas as **Asas de Lona do Takhsich**
  (`item_asas_lona_takhsich`, Feira da Miwnay no Mercado dos Túneis, 420 ouro: armação de cobre e lona que pega a corrente
  do Ar Sereno e dá o segundo impulso), ou asas do Tacho Encantado, ou o Grifo da Coluna da Glória.
- **Decisão (2026-11-06, dono):** o **Pulo Duplo do Íbex-da-Walia** (`mount_mountain_goat`) **não** abre o 24 — o íbex
  custa 70 de ouro no Curral de Litema e furaria a exigência das asas (Asas de Lona: 420). O "Pulo Duplo" da exigência é
  só o das asas de lona vestidas. Teste: `MapRequirementTests.Ibex_DoubleJump_DoesNotOpenTheNinhoDeMahaway`.
- **Fonte:** Mahaway, o gigante mensageiro que voa (*Livro dos Gigantes*).
- **Bioma e visual:** poço vertical gigante com ninhos nas paredes; basalto, índigo, pedra clara, âmbar; pontes de corda
  em espiral.
- **Layout:** G (vertical). Descida em espiral por pontes até o ninho; **posto: Última Ponte de Corda**. Retorno: Vila
  das Lamparinas.
- **Inimigos:** Revoada de Mahaway [enxame]; Ubır [rápido]; Morcegão de Pedra [tanque]; Corvo-da-Treva [distância].
- **Chefe:** **chefe do mundo: Mahaway, o Rei da Revoada / o Morcegão da Treva** (já no código).
- **Mecânica:** **correntes de ar**: subir pelos poços de vento (planando) é atalho; cair é a rota longa.
- **No jogo (2026-11-11): poços de vento** (`LimiarWindWellRules`, pontos em `OverworldLayouts.LimiarMechanics.cs`, driver `LimiarMechanicsDriver`, teste `LimiarWindWellMechanicsTests`): 6 poços em 3 pares, um em cada borda, por cima do poço de Mahaway (passa pelos ninhos), da fenda das Escarpas da Revoada e do Abismo sem Fundo. Cada poço respira: 8 s soprando, 4 s parado (coluna azul alta ou cinza baixa; cada poço com sua fase). Com asas (`WingsOrDoubleJump`: Asas de Lona, asas do Tacho ou o Grifo; o íbex não) e o vento soprando, [E] (ou "Usar" no toque) a até 4 m plana até a outra borda a 14 m/s, arco de 12 m: 65–95 m em 5–7 s, contra 250–450 m a pé até a ponte de corda (x ±950) ou contornando o poço. A revoada bica quem está no ar: 2% da vida máxima a cada 2 s (3% à noite), nunca abaixo de 30%; a rajada do pouso tira 6% da vida dos bichos a até 8 m (chefe 1%). 3 s entre voos; as duas zonas e as do caminho têm de estar abertas para o herói. Falta: as pontes em espiral e a queda como rota longa de verdade (o mundo é plano: hoje o "longo" é a volta pela ponte).
- **Ganchos:** loot Pena de Mahaway · montaria `mount_pegasus` (conquista longa), `mount_sertao_phoenix` (Bennu,
  lendária de chefe) · casa: troféu de Mahaway.
- **Noite:** Mahaway vira o Morcegão da Treva; luz só nas pontes.

### Região 5 — Costa das Monções (costa suaíli e savana de Kush) · mapas 25–30

A "5ª região africana" que a bíblia deixou para depois (§6, decisão 5). Cidade de região: **Taa Pwani** (suaíli *taa* =
lâmpada, *pwani* = costa: "a Costa da Lâmpada"). Estilo novo `CoralStone`: casas de pedra de coral caiada, portas de
madeira entalhada com pino de latão, telhados planos com terraço, *baraza* (banco de pedra na frente de casa), dhows no
cais. Paleta: coral caiado `#EFE7DA`, madeira de teca `#6B4A2E`, latão `#C9A23F`, mar `#2E9BB0`, vela `#F2E6C9`, laterita
`#B4583A`. Inspiração: Kilwa Kisiwani, Lamu, Gedi (cidades de pedra de coral, séc. XII–XV). Elemento da região: Água
Viva e Fogo Vivo (o sol da savana). Chefe do mundo: **Hedammu** (mapa 30).

### 25 · Taa Pwani e o Recife / *Taa Pwani and the Reef* — nv 45–50 · ★
- **Mundo contínuo (2026-11-03): mapas 25–30 jogáveis a pé.** A região inteira é um mapa de 2400 × 2400 m
  (`OverworldLayouts.Costa`) costurado a leste do Zarafshan, sem portão. Grade 5 × 5 de áreas (colunas x −600/−150/250/750/1250/1800,
  linhas z −1200/−700/−250/250/700/1200), nível subindo com a distância de Taa Pwani a partir de 45:
  - **Perto (45–51):** Praia de Taa Pwani (25, a cidade, Backdrop desligado), Recife de Taa Pwani (25; Passagem de Dhow — a praia
    da cidade nunca pede, o Bazar de Pérolas vende), Dunas das Casuarinas, Mata da Restinga (portão para o Zarafshan), Coqueiral
    de Pwani, Cravais do Vento Leste, Praia dos Dhows, Lagoas de Maré, Baixios do Sul.
  - **Meio (48–60):** Savana dos Auroques e Savana das Acácias (26, **Aratta** = `CityTownRecipes.Aratta()`), Pirâmides Altas de
    Meroé (27, Pátio dos Relevos; Relíquia dos Artífices), Bosque dos Baobás, Falésias Vermelhas, Ruínas de Gedi, Atol de Kirta
    (28, **Kirta** = `CityTownRecipes.Kirta()`, ao sul da lagoa; Concha Respirável, vendida em Taa Pwani), Ilhotas de Coral,
    Lagoa Grande.
  - **Longe (58–72):** Jardins de Alga (28), Kilwa Afundada (29, Farol de Coral; Bolha de Ar) e Palácio de Husuni (29), Falésias
    do Norte, Mangue Raso, Canais de Liyongo (30, Cais das Dhows; Botas de Raiz) e Mangues de Liyongo (30, 66–72).
  - **Água:** o mar aberto fica além do muro leste; dentro, o Canal do Recife (entre a praia e o recife), lagoas, o canal dos
    baixios, a lagoa de Kirta, os canais afogados de Kilwa e os canais do mangue são **água funda que bloqueia** (azul escuro),
    atravessada em **baixios** (vaus rasos, marcados por estacas listradas de vermelho e branco nas margens) e pontes/calçadas
    de coral; o recife, as ilhotas, o atol e os jardins de alga são **raso caminhável** (chão turquesa).
  - **Relevo:** `ReliefShape.Cliffs` (promontório de laterita em dois degraus com escarpas, até ~18 m) e `ReliefShape.Islets`
    (bancos de areia logo acima da água); dunas em crista nas casuarinas; praias suaves pela planície de vale junto da água.
  - **Cenário** (`OverworldDecorKit.Coast.cs`, `OverworldChunkBuilder.Coast.cs`): dhows encalhados, armadilhas de peixe (*wema*),
    redes secando, cabeças de coral, muros, arcos e túmulos de pilar de coral, acácias de copa plana, cupinzeiros, mangues ao longo
    dos canais, cravo secando na esteira, linhas de sargaço; coqueiros inclinados, baobás e casuarinas (as do fundo além do muro,
    a árvore mais leve); pirâmides, colunas de lótus e ushabtis do deserto em Meroé.
  - **Bichos:** Gaivota Ladra e Caranguejo-dos-Coqueiros (praias, recife, coqueiral, atol, mangues), Crocodilo do Mangue Apagado
    (mangue raso, canais e mangues de Liyongo), Leão do Capim Apagado e Rinoceronte-de-Pó (savana dos auroques) com chacal,
    cobra-de-areia (moreias), escorpião, aranha, lobo, queixada, ushabti, abutre de brasa, gavião, babuíno, Pedra que Rola, lodo, corvo da
    treva, peixe-lanterna e as espécies da noite (plâncton/vaga-lume no recife e nos mangues, Mngwa no corpo do lobisomem,
    kongamato no morcego-vampiro); nenhum chefe. Clima: céu limpo nas praias, recife, atol e savana; chuva de monção nos mangues e
    em Kilwa; névoa nas falésias, lagoas e algas; poeira em Meroé.
  - **Orçamento** (.NET fora do Editor, 1 de cada 9 pedaços): ~5,0 mil triângulos em média, pior 10,2 mil (anel de fundo);
    geração ~0,8–1,1 ms em média, p95 ~1,0–1,6 ms. Testes `OverworldCostaTests` e `OverworldWorldTests` (rota a pé Vila → Taa Pwani).
- **No jogo (2026-11-03): cidade pronta** (o mapa veio depois: desde então o 25 é zona do mundo contínuo da Costa, `isBuilt: true`). `CitySpec` de cidade de região tardia
  (`CityCatalog.LateRegionCities`) com passeio próprio (`CityTownRecipes.TaaPwani`, estilo `CoralStone`: casas de coral
  caiada com terraço, ameias, porta de teca entalhada com pinos de latão e baraza; mercado de pérolas na praça; Farol de
  Coral; 2 dhows de vela triangular nas poças de maré; mangues no sudeste; coqueiros e baobás), respawn na praça para
  derrotas nos mapas 25 e 30, Capitão Liyongo, Mwana, Nuru, Zawadi e Mbaraka nas portas com visual e falas próprios,
  Bazar de Pérolas da Mwana (todas as exigências dos mapas 25–30) e 2 missões da cidade (Docs/Cities.md §1.2). *(Corrigido em 2026-11-11: o mapa 25 é jogável
  como zona do mundo contínuo, com a maré do recife e a Gaivota Ladra e o Caranguejo-dos-Coqueiros nas praias e no recife.)*
- **Fonte:** cidades de pedra de coral da costa suaíli (Kilwa, Lamu); dhows e as monções do Oceano Índico; Fumo Liyongo,
  herói-arqueiro e poeta suaíli (já é a G3 da Maithe).
- **Bioma e visual:** praia, recife raso e cidade de coral; paleta acima; coqueiros, mangue na borda, poças de maré.
- **Layout:** G. **Taa Pwani** (cidade) no centro do litoral; recife a leste que aparece na maré baixa; dunas e mato
  a oeste; Cais da Barca e cais de dhows. Retorno: Taa Pwani.
- **Inimigos:** Caranguejo-dos-Coqueiros Gigante [tanque] (bicho real); Moreia Apagada [rápido]; Gaivota Ladra [rápido];
  Ouriço de Fumaça [controle, espinhos lentos] (invenção).
- **No jogo (2026-11-11): bichos próprios no lugar dos emprestados** (`MidStandInBeastRecipes`, `MidStandInBestiaryTexts`, `OverworldLayouts.MidBeasts`; modelo com rig, loot base e página no Caderno): Moreia Apagada (`enemy_forest_dimmed_moray`) nas moreias do Recife de Taa Pwani, no lugar da cobra-de-areia; Ouriço de Fumaça (`enemy_forest_smoke_urchin`) no lodo das Lagoas de Maré, no lugar do lodo de gruta, e junto dos caranguejos dos cascos na Praia dos Dhows, no lugar da aranha (as gaivotas dos dhows, alvo da etapa da história do meio, ficam). Falta: o Caranguejo-dos-Coqueiros Gigante (é o caranguejo comum) e os espinhos lentos do ouriço (hoje golpe corpo a corpo).
- **Chefe:** **O Polvo das Monções**, polvo enorme apagado que segura as velas dos barcos; libertado, a monção volta.
- **Mecânica:** **maré**: a cada 8 min a maré baixa (3 min) e abre trilhas no recife com baús e faíscas; na maré alta só
  montaria aquática atravessa.
- **No jogo (2026-11-09): maré do recife** (`CostaTideRules`, `OverworldLayouts.TideFlats`, `CostaDeltaTetoMechanicsDriver`): relógio UTC de 8 min: 2 min 20 s de maré baixa, 40 s virando (aviso "saia dos baixios"; ainda seco), 5 min de maré alta. 6 baixios de 30–35 m no Recife de Taa Pwani, fora de toda estrada (a estrada leste nunca alaga), com um baú no meio ([E] a 3 m, 1× por maré, só com o baixio seco). Na alta, em cima do baixio: −40% de velocidade e 3% da vida a cada 2 s, nunca abaixo de 30%, com aviso antes da primeira mordida; montaria aquática passa livre. Só anel e feixe de luz, sem água subindo de verdade.
- **Ganchos:** loot coral, madrepérola, sal · montaria nova **Dhow-Tartaruga** (aquática, §6) · casa: conjunto novo
  **Porta Suaíli** (porta entalhada, baraza, lanterna de latão).
- **Noite:** o recife brilha (plâncton); **Mngwa** ("o estranho", gato cinzento do folclore suaíli) [elite, só à noite];
  visão boa na praia, curta no mato.

### 26 · Savana dos Auroques / *Aurochs Savanna* — nv 48–55 · ★ · cidade de raça (povo-touro)
- **Fonte:** nomes sumérios (§3.3); a savana de acácias e baobás; o auroque (boi selvagem antigo); a ave-relâmpago dos
  contos do sul da África (só a ave do trovão, sem a parte de feitiço).
- **Bioma e visual:** capim dourado, acácias de copa plana, baobás; capim `#D9B44A`, acácia `#6E7F3A`, terra vermelha
  `#9C4A2F`, céu `#9CC3E0`; cupinzeiros altos, poças.
- **Layout:** GG. **Aratta** (§3.3) junto a um baobá gigante; percurso da Corrida de Shulgi em volta; 3 poças grandes.
  Retorno: Aratta.
- **Inimigos:** Leão do Capim Apagado [rápido] (bicho real); Rinoceronte-de-Pó [tanque] (bicho real); Abada [elite] (o
  "unicórnio" de dois chifres dos relatos do Congo); Ave-Relâmpago [distância] (impundulu, folclore xhosa e zulu).
- **No jogo (2026-11-11): bichos próprios no lugar dos emprestados** (`MidStandInBeastRecipes`, `MidStandInBestiaryTexts`, `OverworldLayouts.MidBeasts`; modelo com rig, loot base e página no Caderno): Ave-Relâmpago (`enemy_forest_lightning_bird`, [distância], solta a faísca) nas aves das Savanas dos Auroques e das Acácias, no lugar do abutre de brasa, e nos urutaus do Bosque dos Baobás, no lugar do urutau. Os babuínos dos baobás continuam o babuíno da Serra: é o id que a etapa da história do meio nos baobás manda caçar. Falta: Abada [elite].
- **Chefe:** **O Touro de Fumaça**, auroque gigante apagado que pisoteia o capim; libertado, guia o rebanho de Aratta.
- **Mecânica:** **fogo baixo**: o povo-touro queima o capim velho em faixas (Fogo Vivo que renova); as faixas queimadas
  viram trilhas rápidas; os bichos fogem delas.
- **Ganchos:** loot couro de auroque, Fitas de Corrida · montaria nova **Auroque de Aratta** (§6) · casa: conjunto novo
  **Salão de Junco** (lira de cabeça de touro, esteira).
- **Noite:** a Ave-Relâmpago brilha nas nuvens; **Hiena de Fumaça** [enxame, só à noite]; visão longa (céu aberto).
- **No jogo (2026-11-03): cidade de verdade** (`CityTownRecipes.Aratta.cs`, estilo `ReedHorn`): Aratta em y = 0 dentro de um círculo de 22 m (saídas O, L e S a 20 m): salões de junco amarrado em arco (mudhif) cobertos de esteira, com colunas de feixe e chifres sobre a porta; salão do Enmerkar na cabeça da praça, Lira do Touro e fogo baixo do Fogo Vivo na praça, postes de chifre com fita azul (largada da Corrida de Shulgi na rua Sul), cercados redondos de pedra, acácias e baobá; Enmerkar, Lugalbanda, Shulgi e Ensuhkeshdanna com visual de povo-touro (chifres, focinho largo com argola) e falas para cada primo; Alforje do Lugalbanda (Relíquia dos Artífices do 27); missões "Chacais no Capim Seco" e "O Percurso de Shulgi"; respawn na praça para 26–27. Falta: a Corrida de Shulgi com tempo e Fitas de Corrida, a Lira do Touro (canção do dia) e o Auroque de Aratta. *(2026-11-11: o mundo contínuo da Costa já usa esta receita.)*

### 27 · Pirâmides Altas de Meroé / *High Pyramids of Meroë* — nv 52–58 · ★
- **Fonte:** o campo de pirâmides de Meroé (Kush); o templo do leão de Naqa e o grande recinto de Musawwarat es-Sufra
  (com rampas e relevos de elefante); Apedemak só como relevo de pedra (bíblia §3.17).
- **Bioma e visual:** areia laranja e acácias; arenito `#C47A45`, sombra `#6D4C7D`, ouro `#C9A23F`; dezenas de pirâmides
  pontudas, capelas com pilones, rampas largas, relevos de leão e de elefante.
- **Layout:** G. Três campos de pirâmides em semicírculo; o Pátio dos Relevos no meio; **posto: Pátio dos Relevos**.
  Retorno: Aratta.
- **Inimigos:** Ushabti de Bronze Kushita [tanque]; Carneiro de Pedra Apagado (as estátuas de carneiro das avenidas)
  [elite]; Escorpião-Rei Jovem [rápido]; Chacal de Areia Negra [enxame] (bicho real).
- **Chefe:** **O Leão dos Relevos**, a estátua-guardiã do pátio que se soltou da parede; libertado, volta para o relevo e
  "abre um olho" quando os primos passam.
- **Mecânica:** **sombras do sol**: as sombras das pirâmides apontam para capelas; ficar na ponta da sombra na hora certa
  abre a capela (baú).
- **Ganchos:** loot ouro de Kush, peças de engenho · montaria nova **Elefante de Musawwarat** (§6) · casa: **Areias
  Flamejantes** (mini-pirâmide), conjunto novo **Recinto do Elefante**.
- **Noite:** as capelas acendem; o Carneiro de Pedra anda; visão boa sob a lua.

### 28 · Cidade-Recife de Kirta / *Kirta Reef City* — nv 55–62 · ★ · cidade de raça (povo-do-mar)
- **Fonte:** nomes ugaríticos (§3.3).
- **Bioma e visual:** recife profundo; paleta de Kirta (§3.3); câmera em corte (metade acima, metade abaixo da água).
- **Layout:** G. **Kirta** (§3.3) num atol; jardins de alga ao norte, abismo ao sul. Retorno: Kirta.
- **Inimigos:** Peixe-Leão Apagado [distância, espinhos]; Caranguejo-Aranha [tanque] (bicho real); Água-Viva de Lodo
  [controle]; Tubarão-de-Lodo [rápido].
- **Chefe:** **A Moreia do Coral Cinza**, que se enrolou no coral-mãe e apagou a cor dele; libertada, o recife acende.
- **Mecânica:** **reacender o coral** (§3.3) e **Bolhas de Ar** (andar no fundo).
- **Ganchos:** loot pérola, alga, coral aceso · montaria `mount_river_seahorse` (desperta no mar) · casa: conjunto novo
  **Jardim de Coral**.
- **Noite:** o coral aceso brilha; o apagado fica escuro e os tubarões saem; visão curta no fundo.
- **No jogo (2026-11-03): cidade de verdade** (`CityTownRecipes.Kirta.cs`, estilo `CoralReef`): a parte seca do atol em y = 0 dentro de um círculo de 22,5 m (saídas O, L e N a 19 m; ao sul a borda do recife sobre o abismo): torres de coral com base afogada, janelas redondas, cúpula de madrepérola e toldo de vieira; coral-mãe na praça (metade cinza, metade reacesa), pontes de concha sobre a água do recife, jardins de alga e torres de coral soltas; Kirta, Pughat, Danel e Aqhat com visual de povo-do-mar (barbatanas no lugar das orelhas, crista, guelras) e falas para cada primo; Ferramentas de Mergulho do Danel (Concha Respirável do 28, Bolha de Ar do 29); missões "Lodo no Recife" e "Os Mergulhadores Encurralados" (paga com a Bolha de Ar); respawn na praça para 28–29. Falta: câmera em corte e andar no fundo, reacender o coral por faísca (zona de cura) e Apito de Concha. *(2026-11-11: o mundo contínuo da Costa já usa esta receita.)*

### 29 · Kilwa Afundada / *Sunken Kilwa* — nv 60–66 · ★
- **Fonte:** as ruínas do palácio de Husuni Kubwa (Kilwa Kisiwani), em parte tomadas pelo mar; o *dingonek*, bicho
  estranho de rio dos relatos do Quênia (criptídeo de folclore).
- **Bioma e visual:** cidade de coral meio submersa; coral caiado e musgo marinho `#5E8F7A`, água rasa `#4FB3C0`, latão
  verde; arcos, piscina octogonal do palácio, escadarias que entram no mar.
- **Layout:** G. Bairros em ilhotas, pontes de pedra quebradas; o palácio no alto; **posto: Farol de Coral**. Retorno:
  Kirta.
- **Inimigos:** Ushabti de Coral [tanque]; Polvo Ladrão [controle]; Raia de Fumaça [rápido]; Dingonek [elite].
- **Chefe:** **O Caranguejo-Palácio**, caranguejo-ermitão gigante que carrega uma cúpula do palácio como concha;
  libertado, devolve a cúpula (marco da cidade).
- **Mecânica:** **casas alagadas**: entrar numa casa submersa pede Bolha de Ar; dentro, quebra-cabeças de porta
  entalhada.
- **Ganchos:** loot latão antigo, moeda de Kilwa · montaria nova **Dhow-Tartaruga** (Despertar) · casa: **Porta Suaíli**.
- **Noite:** o farol varre o mapa (quem está no facho fica visível de longe); o dingonek ronda.

### 30 · Mangues de Liyongo / *Liyongo Mangroves* — nv 64–72 · ★
- **Fonte:** Fumo Liyongo, herói-arqueiro da épica suaíli; Hedammu, a serpente do mar que comia tudo (*Canção de Hedammu*,
  hurrita); o kongamato ("quebra-barcos"), ave-morcego dos relatos da Zâmbia.
- **Bioma e visual:** manguezal com canais; raiz `#5B4A3A`, lama `#4A4436`, folha `#4F7A3F`, água salobra `#6E8C7A`;
  raízes-escora, garças, caranguejos.
- **Layout:** G. Canais em rede; ilhas de mangue; o "Largo de Hedammu" no centro (arena do chefe do mundo); **posto: Cais
  das Dhows**. Retorno: Taa Pwani.
- **Inimigos:** Crocodilo do Mangue [tanque] (bicho real); Kongamato [distância]; Caranguejo-Violinista [enxame] (bicho
  real); Garça Apagada [rápido].
- **No jogo (2026-11-11): bichos próprios no lugar dos emprestados** (`MidStandInBeastRecipes`, `MidStandInBestiaryTexts`, `OverworldLayouts.MidBeasts`; modelo com rig, loot base e página no Caderno): Caranguejo-Violinista (`enemy_forest_fiddler_crab`, a garra grande desce no golpe) nos violinistas do Mangue Raso, no lugar da aranha (o caranguejo-dos-coqueiros fica de acompanhante), e nos de Liyongo, no lugar do caranguejo-dos-coqueiros; os crocodilos do Mangue Raso, alvo da etapa da história do meio, ficam. Falta: Kongamato (corvo-da-treva) e Garça Apagada (gaivota).
- **Chefe:** **chefe do mundo: Hedammu, a Serpente Gulosa do Mar** (nova; aliada de Āz: "come tudo e nunca enche").
  Vencida, cospe as faíscas que engoliu e volta para o fundo, menor e mais calma.
- **Mecânica:** **canais e dhows**: barcos pequenos de aluguel (2 ouro) andam rápido nos canais; a maré fecha canais.
- **Ganchos:** loot escama de Hedammu, raiz de mangue · montaria nova **Garça do Brejo** (variante de mangue) · casa:
  troféu de Hedammu.
- **Noite:** vaga-lumes nos mangues; o kongamato dobra; os canais ficam escuros (lanterna ajuda).
- **No jogo (2026-11-11): chefe do mundo Hedammu** (`RegionBossRules.MidSpawns`, `RegionBossDirector`, `enemy_mountain_hedammu`, nv 72): arena no anel de pedras do Largo de Hedammu, no fim da trilha do mangue depois do vau (1540, −1150 na região); aparece com o herói a 90 m, some a 125 m (de volta à vida cheia) e volta 5 min depois de cair; acima de 66% golpe e anel de força, 66–33% rajada de 3 tiros a cada 8 s, abaixo de 33% rajada de 5 e o Grande Golpe marcado no chão (2 s, 4,5 m); as clareiras comuns ficam a 22 m ou mais do anel, chão plano e trilha até ele; peso 0 no bioma (nunca sai em encontro). Própria: **mordida gulosa** a partir de 66% (a cada 10 s, 1,8 s de aviso): cone de 50° para cada lado e 9 m na direção em que o herói estava; do lado ou atrás dela não pega. Vencida, cospe as faíscas e volta para o fundo (não fica corpo). Loot próprio (2026-11-11): **Escama de Hedammu** (`item_chefe_escama_hedammu`, rara, 60% por vitória, 1–2, 120 de venda; no lugar do punhado de ouro, o loot do bioma segue igual); vai no Tear/Forja da casa (`HouseCraftRules`): Saia de raios dourados da Maria (Tear, 3) e Saia de aurora (Tear de mestre). Troféu de casa (2026-11-11): **Cabeça entalhada de Hedammu** (`casa_trofeu_025`, épico, parede), feito na Forja da casa com 3 Escama de Hedammu e o chefe já vencido (`HouseCraftRules.WorldBossTrophies`). Falta: a troca de canal pela maré e a Garça do Brejo.

### Região 6 — Delta de Jhilpur (pântano de raízes, Água Viva) · mapas 31–35

- **Mundo contínuo (2026-11-03): mapas 31–35 jogáveis a pé.** A região inteira é um mapa de 2400 × 2400 m
  (`OverworldLayouts.Delta`, `DeltaId`) costurado a leste das Areias e ao sul da Costa das Monções, sem portão. Jhilpur
  (`CityTownRecipes.Jhilpur()`, Backdrop desligado, chão plano de ±30 m em y = 0, saídas O/L/N; ao sul os ghats descem ao lago)
  fica em (0, 0). Grade 5 × 5 de áreas (colunas x −1200/−720/−240/240/720/1200, linhas z −1800/−1320/−840/−360/120/600), nível
  subindo com a distância de Jhilpur a partir de 60:
  - **Perto (60–66):** Lago de Jhilpur (31, a cidade), Bancos de Jhilpur, Diques do Norte (portão para a Costa), Lagoas de Lótus,
    Figueiral das Raízes, Várzea dos Búfalos, Charcos de Lótus.
  - **Meio (63–70):** Pontes de Raiz Viva e Gargantas Esquecidas (32, posto Plataforma das Garças; **Vara de Equilíbrio**),
    Cascatas da Chuva (portão para as Areias) e Floresta de Chuva, Floresta de Maré (portão para os mangues de Liyongo), Ilhas
    de Junco, Pântano Aberto, Brejo das Palmeiras.
  - **Longe (70–88):** Palafitas de Tarchon e Ilhas que Andam (33; **Barco de Junco**), Bancos de Lama, Raízes Afogadas, Mata
    Morta, Charco do Ninki Nanka, Ilhota das Lanternas (posto) e Toca da Dragoa (34; **Amuleto Antiveneno**), Coração do Lodo
    (posto Raiz-Mãe) e Olho do Lodo (35; **Fruta Purificadora**). A Nuhzadag de Jhilpur vende o equipamento de todas.
  - **Tarchon (33):** a cidade dos trols (`CityTownRecipes.Tarchon()`, sem backdrop) fica no chão plano de ±28 m em y = 0 em
    (480, −1080); é a cidade de retorno de 33 e 34 (`MapCatalog` e a tabela do §1).
  - **Água:** o Canal de Jhilpur (do norte, pelo lado leste da cidade, pelo lago e ao sul até o lodo), o Canal do Leste (pelas
    ilhas de junco até o muro leste), os três rios das gargantas, lagoas de lótus, poças do charco e do lodo e os riachos da
    maré são **água funda que bloqueia**, atravessada em **vaus** marcados pelas estacas listradas (como os baixios da Costa)
    e em **pontes de raiz** (uma no canal ao leste da cidade, uma em cada garganta).
  - **Relevo:** `ReliefShape.Hummocks` (ilhotas de touceira e raiz sobre lama plana: junco, ráfia, lodo) e `ReliefShape.Levees`
    (diques longos e baixos sobre os arrozais); gargantas (`Gorges`, até ~15 m) nas Pontes de Raiz Viva; colinas na floresta
    de chuva e no figueiral; o resto quase plano junto da água.
  - **Cenário** (`OverworldDecorKit.Delta.cs`, `OverworldChunkBuilder.Delta.cs`; só cores da paleta já existente): figueiras-de-
    bengala com cortinas de raízes aéreas, arcos de raiz viva, cabanas de pescador em palafita, lanternas de lótus em vara de
    bambu, fontes de Água Viva num anel de raízes (brilham), raízes podres, árvores mortas, diques de terra; detalhe de chão
    carimbado: folhas e flor de lótus na beira d'água, touceiras de arroz, bolhas violeta do Lodo; palmeiras e casuarinas do
    kit da costa (casuarinas na faixa dos muros, a árvore mais leve).
  - **Bichos** (nenhum chefe): peixe-lanterna (Peixe-Saltador de Lodo), os próprios Crocodilo do Mangue Apagado (Gavial do Lodo,
    Crocodilo de Musgo, Ninki Nanka Filhote, Gavial-Sombra), Píton Apagada (também a Cobra-d'Água), Lontra Ladra, Macaco Ladrão e
    Trol Apagado, gaivota (Garça-Lança e Garça-Fantasma), lodo (Sanguessuga, Filhos do Lodo), aranha (Raiz-Laço), golem,
    lanterna quebrada (Fogo-Fátuo Preso), Neblina (Bolha de Fumaça), corvo (Raiz Podre), morcegos (Libélula de Fumaça),
    queixada (Sapo-Boi), gavião, caranguejo, urutau; à noite vaga-lumes/fogos-fátuos, a coruja rasga-mortalha (garça-fantasma),
    o morcego-vampiro e a queixada. Clima: monção nas gargantas, na floresta de chuva e na de maré; névoa no lótus, no junco,
    no brejo, na mata morta e no lodo; o lago, os diques e os bancos deixam o bioma sortear.
  - **Orçamento** (.NET fora do Editor, todos os 2704 pedaços): ~5,7 mil triângulos em média, pior 11,4 mil (faixa dos muros);
    geração ~0,56 ms em média, p95 ~0,9 ms. Testes `OverworldDeltaTests` e `OverworldWorldTests` (rota a pé Vila → Jhilpur,
    ~4,7 km).

Capital **Jhilpur** (§2). *Jhil* = lago/charco (bengali e hindi), *pur* = cidade. Fonte maniqueia: a viagem de Mani à Índia
(Kephalaia 1). Respeito: arquitetura das palafitas de Bengala e das pontes de raiz viva de Meghalaya (povo khasi, vivo),
**só** como forma; nenhum rito, nenhuma divindade da região. Chefe do mundo: **O Lodo-que-Engole** (mapa 35).

### 31 · Jhilpur, a Cidade nas Raízes / *Jhilpur, City in the Roots* — nv 60–66 · ★ · capital (Água Viva)
- **Fonte:** palafitas do delta de Bengala (Sundarbans), ghats de degraus, pontes de raiz viva; NPCs com nomes de
  missionários maniqueus: **Nuhzadag** (o intérprete de Mani; aqui, barqueiro que fala todas as línguas da ilha),
  **Salmaios** (o asceta do Códice de Colônia; aqui, o velho que cuida da Raiz-Mãe) e **Abiesous** (mestre do Códice;
  aqui, professor da escola de barco).
- **Bioma e visual:** figueiras gigantes de raízes aéreas sobre um lago marrom-esverdeado; paleta e estilo `RootStilt`
  (§2.1); lírios-d'água, garças, barcos de proa curva.
- **Layout:** GG. A cidade sobe por 3 andares de plataformas nas raízes; a **Raiz-Mãe** no centro (praça redonda); ghats
  descem para a água; bairros de barqueiros, de cesteiros e de ervas. Retorno: Jhilpur.
- **Inimigos (arredores):** Peixe-Saltador de Lodo [enxame] (bicho real); Gavial do Lodo [tanque] (bicho real); Lontra
  Ladra [rápido]; Garça-Lança Apagada [distância].
- **No jogo (2026-11-11): bichos próprios no lugar dos emprestados** (`MidStandInBeastRecipes`, `MidStandInBestiaryTexts`, `OverworldLayouts.MidBeasts`; modelo com rig, loot base e página no Caderno): Garça-Lança Apagada (`enemy_forest_spear_heron`, [distância]) nas garças do Lago de Jhilpur, dos Diques do Norte, da Várzea dos Búfalos, das Lagoas de Lótus e dos Bancos de Lama, no lugar da gaivota. Falta: Peixe-Saltador de Lodo (peixe-lanterna); a Garça-Fantasma do 33 continua a gaivota.
- **Chefe:** nenhum na capital; evento de cidade **Maré de Lodo** (ondas de Filhos do Lodo sobem os ghats; defender a
  Raiz-Mãe, estilo Crywolf do MU, Referências §2).
- **Mecânica:** **monção**: na estação das chuvas (`SeasonCalendar`) a água sobe um andar; ruas viram canais e a cidade
  muda de rota; a escola de barco do Abiesous ensina a remar.
- **Ganchos:** loot seda de lótus, raiz aérea · montaria nova **Búfalo-d'Água de Jhilpur** (§6) · casa: conjunto novo
  **Casa nas Raízes** (rede de raiz, lanterna de lótus). G4 do Joaquim (Raiz de Jhilpur) é aqui.
- **Noite:** lanternas de lótus boiando; vaga-lumes nas raízes; a Raiz-Mãe brilha verde-água.
- **No jogo (2026-11-03): cidade de verdade** (`CityTownRecipes.Jhilpur.cs`, estilo `RootStilt`, `CityCatalog.LateRegionCities`): em y = 0 dentro de um círculo de 22,5 m (saídas O, L e N a 19 m; ao sul os ghats de tijolo descem para a água): casas de tábua sobre colunas de raiz com telhado de palha em dorso de barco e lanternas de lótus; a Raiz-Mãe na praça redonda (anel de Água Viva no pé), pontes de raiz viva sobre os canais, figueiras de raízes aéreas e lagos de lótus; Salmaios, Nuhzadag, Abiesous e Tehat (das cartas de Kellis) com visual próprio e falas para cada primo; Barcos e Trocas do Nuhzadag com as exigências 32–35; missões "Lodo nos Ghats" e "Corvos nos Remos" (paga com a Fruta Purificadora); respawn na praça para 31, 32 e 35. Falta: a monção, a escola de barco ensinando a remar, a Maré de Lodo e a Casa nas Raízes. *(2026-11-11: o mapa 31 e o mundo contínuo do delta já existem, com esta receita.)*

### 32 · Pontes de Raiz Viva / *Living Root Bridges* — nv 64–70 · ★
- **Fonte:** pontes de raiz viva de Meghalaya (forma); a Água Viva que faz as raízes crescerem (bíblia §1.3).
- **Bioma e visual:** floresta de chuva com rios em garganta; raiz `#6B5236`, musgo `#5E8F3E`, pedra molhada `#6F7470`,
  cascata `#CFE8EF`; pontes de raiz em dois andares, escadas de pedra.
- **Layout:** G. Gargantas paralelas ligadas por pontes; algumas pontes "esquecidas" (desfeitas); **posto: Plataforma
  das Garças**. Retorno: Jhilpur.
- **Inimigos:** Macaco Ladrão [rápido]; Sanguessuga do Lodo [enxame]; Píton Apagada [controle] (bicho real); Raiz-Laço
  [controle].
- **No jogo (2026-11-11): bichos próprios no lugar dos emprestados** (`MidStandInBeastRecipes`, `MidStandInBestiaryTexts`, `OverworldLayouts.MidBeasts`; modelo com rig, loot base e página no Caderno): Sanguessuga do Lodo (`enemy_forest_mire_leech`) nas sanguessugas da Floresta de Chuva, das Lagoas de Lótus e das Gargantas Esquecidas, no lugar do lodo de gruta. Falta: Raiz-Laço (aranha).
- **Chefe:** **A Ponte que Esqueceu**, uma ponte de raiz que virou bicho e anda; libertada, volta a ser ponte (atalho).
- **Mecânica:** **fazer crescer**: regar as pontes desfeitas com Água Viva (skills do Joaquim ou Gota de Água Viva) as
  faz crescer em 10 s; abre caminhos e baús.
- **Ganchos:** loot raiz viva, orquídea · montaria `mount_capybara` (Despertar com raízes) · casa: **Casa nas Raízes**.
- **Noite:** as raízes brilham onde foram regadas; a Píton caça; visão curta.

### 33 · Palafitas de Tarchon / *Tarchon Stilts* — nv 70–78 · ★ · cidade de raça (trols)
- **Fonte:** nomes etruscos (§3.3); os fogos-fátuos como faíscas presas (bíblia §3.5).
- **Bioma e visual:** pântano aberto com ilhas de junco; paleta de Tarchon (§3.3); névoa lilás, redes, cestos.
- **Layout:** G. **Palafitas de Tarchon** (§3.3) no meio do pântano; ilhas flutuantes que mudam de lugar. Retorno:
  Palafitas de Tarchon.
- **Inimigos:** Fogo-Fátuo Preso [enxame]; Trol Apagado (alguns, que vagam esquecidos; libertados, voltam para a casa
  deles na cidade) [tanque]; Crocodilo de Musgo [tanque]; Garça-Fantasma [distância].
- **Chefe:** **O Pescador de Luzes**, bagre gigante que engoliu fogos-fátuos para "ter luz só para ele"; libertado,
  solta uma nuvem de faíscas.
- **Mecânica:** **ilhas que andam**: as ilhas de junco derivam devagar; o mapa muda a cada 15 min de relógio do jogo.
  E o **Despertar o vizinho** da cidade (§3.3).
- **No jogo (2026-11-09): ilhas que andam** (`DeltaIslandRules`, `OverworldLayouts.ReedIslandStations`, `CostaDeltaTetoMechanicsDriver`): 4 ilhas de 14 m num anel de 8 estações na área Ilhas que Andam, ao sul do Canal do Leste; a cada 15 min de relógio UTC cada uma deriva 2 min até a estação seguinte e leva quem está em cima. Em volta, um lodo de 105 m de raio, fora de toda estrada: −30% de velocidade fora das ilhas (Barco de Junco ou montaria aquática anulam). A cada 15 min uma ilha leva uma faísca presa ([E] a 3,5 m, 1× por rodada). Sem dano. As ilhas são marcadas por anéis no chão: o terreno não se mexe, ainda não há malha de ilha.
- **Ganchos:** loot junco, Lanterna de Fogo-Fátuo · montaria nova **Sapo-Gigante de Tarchon** (§6) · casa: conjunto novo
  **Palafita** (cesto gigante, rede de pesca).
- **Noite:** os fogos-fátuos dobram e a névoa sobe; Lanterna de Fogo-Fátuo dá visão normal.
- **No jogo (2026-11-03): cidade de verdade** (`CityTownRecipes.Tarchon.cs`, estilo `StiltVillage`, `CityCatalog.RaceCities`): em y = 0 dentro de um círculo de 22,5 m (saídas O, L e S a 19 m; ao norte a cerca de estacas abre para o pântano): casas compridas sobre estacas altas com a empena para a rua, telhado de folha até o piso, redes secando e lanterna de fogo-fátuo; o Mastro dos Fogos-Fátuos na praça, passarelas de tábua sobre o charco, cestos gigantes, varais de rede e ilhas de junco; Tarchon, Ramtha, Larthia, Vel e Thanchvil (trols altos de pele azul-musgo/verde-casca) com falas para cada primo; Cestaria da Larthia com o Barco de Junco (33), o Amuleto Antiveneno (34) e o Alforje; missões "Fogos-Fátuos Presos" e "Garças nas Redes" (paga com o Amuleto Antiveneno); respawn na praça para 33 e 34. Falta: a Lanterna de Fogo-Fátuo, o Despertar o vizinho e o Sapo-Gigante. *(2026-11-11: o mapa e o mundo contínuo já existem, com esta receita; as ilhas que andam estão na linha acima.)*

### 34 · Charco do Ninki Nanka / *Ninki Nanka Bog* — nv 76–82 · ★
- **Fonte:** o ninki nanka, dragão do pântano do folclore da Senegâmbia ("quem a olha nos olhos fica doente", diz o
  conto; no jogo, só envergonhada).
- **Bioma e visual:** charco de juncos altos e palmeiras-ráfia; junco `#B9A55A`, ráfia `#5E7F3A`, lama, água escura
  `#3F5550`; ilhotas, troncos ocos.
- **Layout:** G. Juncal com trilhas de tábua; **posto: Ilhota das Lanternas**; a toca da dragoa ao fundo. Retorno:
  Palafitas de Tarchon.
- **Inimigos:** Ninki Nanka Filhote [elite]; Sapo-Boi [tanque] (bicho real); Libélula de Fumaça [enxame]; Cobra-d'Água
  [rápido] (bicho real).
- **Chefe:** **Ninki Nanka, a Dragoa do Charco**, apagada e muito tímida; libertada, deixa os primos passarem e dá uma
  escama de presente.
- **Mecânica:** **não olhe**: quando a dragoa (ou um filhote) brilha, quem está **de frente** para ela fica lento; virar
  de costas (seta no HUD) evita. Ela "fica com vergonha".
- **Ganchos:** loot escama de ninki nanka, ráfia · montaria nova **Ninki Nanka Mansa** (épica, §6) · casa: conjunto novo
  **Juncal** (biombo de ráfia).
- **Noite:** os juncos escondem tudo (visão 6 m); as libélulas acendem.

### 35 · Coração do Lodo / *Heart of the Mire* — nv 82–88 · ★
- **Fonte:** o Lodo, a "água turva" da Treva (Teodoro bar Konai; Kephalaia): o 4º dos cinco mundos da Treva, cujo povo
  são os que nadam.
- **Bioma e visual:** o fundo do delta, onde a lama é preta; lama `#2B2A25`, raiz podre `#4A3B2E`, bolhas violeta
  `#6D4C7D`, Água Viva `#4FC3B0` nos pontos limpos; troncos mortos, a Raiz-Mãe velha (seca).
- **Layout:** G. Espiral descendente de ilhas até o "olho" do lodo; 5 fontes a purificar; **posto: Raiz-Mãe** (a raiz
  velha, com um broto). Retorno: Jhilpur.
- **Inimigos:** Filhos do Lodo [enxame] (os nadadores da Treva); Bolha de Fumaça [controle]; Gavial-Sombra [tanque];
  Raiz Podre [distância].
- **Chefe:** **chefe do mundo: o Lodo-que-Engole**, a Treva em forma de lama sem rosto; não se vence de vez: recua quando
  as 5 fontes acendem (como o Príncipe, bíblia §1.4).
- **Mecânica:** **purificar**: cada fonte limpa vira zona segura **temporária** (2 min) e encolhe o lodo; o chefe só
  aparece com as 5 acesas.
- **Ganchos:** loot Pedra da Água Viva, "Lama Limpa" · montaria `mount_elder_tortoise` (Despertar II) · casa: troféu do
  Lodo (um broto num pote).
- **Noite:** o lodo cresce sozinho; os Filhos do Lodo dobram; só as fontes limpas iluminam.
- **No jogo (2026-11-11): chefe do mundo o Lodo-que-Engole** (`enemy_mountain_swallowing_mire`, nv 88): arena no anel de pedras do Olho do Lodo (−900, −1640 na região), a oeste da Raiz-Mãe; aparece com o herói a 90 m, some a 125 m (de volta à vida cheia) e volta 5 min depois de cair; acima de 66% golpe e anel de força, 66–33% rajada de 3 tiros a cada 8 s, abaixo de 33% rajada de 5 e o Grande Golpe marcado no chão (2 s, 4,5 m); as clareiras comuns ficam a 22 m ou mais do anel, chão plano e trilha até ele; peso 0 no bioma (nunca sai em encontro). Própria: **o lodo engole** a partir de 66% (a cada 12 s, 2,4 s de aviso): quem estiver a até 6 m dele (9 m abaixo de 33%) leva o golpe; habilidades +25% à noite (o lodo cresce sozinho). Não se vence de vez: recua e não fica corpo. Loot próprio (2026-11-11): **Lama Limpa do Lodo** (`item_chefe_lama_limpa`, rara, 60% por vitória, 1–2, 140 de venda; no lugar do punhado de ouro, o loot do bioma segue igual); vai no Tear/Forja da casa (`HouseCraftRules`): Saia de aurora da Maria e Grevas do tronco milenar do Joaquim (Tear de mestre). Troféu de casa (2026-11-11): **Pedestal da lama limpa do Lodo-que-Engole** (`casa_trofeu_026`, épico, piso), feito na Forja da casa com 3 Lama Limpa e o chefe já vencido (`HouseCraftRules.WorldBossTrophies`). **As 5 Fontes do Lodo (2026-11-11)** (`LodoFountainRules`, `CostaDeltaTetoMechanicsDriver.Lodo.cs`, pontos em `OverworldLayouts.LodoFountains`): espiral até o Olho, a 170, 140, 110, 80 e 50 m do anel de pedras (355 m de caminhada da 1ª à 5ª, 71 s a pé). O **lodo preto** cobre um círculo de 200 m em volta do Olho (240 m à noite: cresce sozinho), encolhe 1/5 por fonte acesa e some com as 5; dentro dele morde 4% da vida a cada 3 s (nunca abaixo de 30%) e puxa o herói para o Olho a 1,2 m/s (para a 12 m do anel). **[E] a até 4 m** de uma fonte purifica em 3 s parado perto (sair cancela; sem puxão enquanto purifica); limpa, ela queima os bichos a até 10 m (30% da vida; 5% a cada 2 s enquanto acesa; chefes não) e é **zona segura de 10 m por 2 min** (sem mordida nem puxão), com feixe e luz de Água Viva (`#4FC3B0`): à noite só as fontes limpas iluminam; pode ser renovada no último minuto. **O Lodo-que-Engole só aparece com as 5 acesas** (`RegionBossRules.SummonReady`, consultado pelo `RegionBossDirector`); depois de surgir, cada fonte que se apaga deixa as habilidades dele +10% (até +50%, `MireFountainMultiplier`), então vale reacender durante a luta. Prêmio: a primeira vez no dia de jogo que as 5 ficam acesas juntas dá 1 **Pedra da Água Viva** (`item_pedra_vida`, marca diária `delta.lodo.fountains`). O estado das fontes não vai no save (dura 2 min) e zera ao sair do Delta. Falta: os Filhos do Lodo dobrarem à noite (grupos de bichos do bioma) e modelo próprio das fontes (hoje anel no chão, feixe e luz).

### Região 7 — Teto do Mundo (Pamir) · mapas 36–40

- **Mundo contínuo (2026-11-03): mapas 36–40 jogáveis a pé.** A região inteira é um mapa de 2400 × 2400 m
  (`OverworldLayouts.Teto`, `TetoId`) costurado ao norte da Serra de Kaggen, sem portão. Sangkent (`CityTownRecipes.Sangkent()`,
  Backdrop desligado, chão plano de ±30 m em y = 0 cobrindo os terraços e o forte do lado norte; ruas O/L/S, a estrada do norte sai
  da rua leste) fica em (0, 0), 600 m da borda sul. Grade 5 × 5 de áreas (colunas x −1200/−720/−240/240/720/1200, linhas
  z −600/−120/360/840/1320/1800), nível subindo com a distância de Sangkent a partir de 70:
  - **Perto (70–76):** Terraços de Sangkent (36, a cidade), Vale dos Damasqueiros (portão oeste para a Serra), Choupal dos Canais,
    Estepe Alta, Rio de Degelo; acima da cidade os Terraços Altos e os Campos de Neve de Sangkent (36: **Roupa Térmica**, vestida —
    a cidade e as entradas pela Serra não pedem nada, como o Recife de Taa Pwani).
  - **Meio (74–84):** Calhas de Pedra, Estação da Calha Alta (posto) e Calhas Geladas (37, sem exigência), Encosta dos Zimbros
    (portão leste para a Serra), Pastagens dos Iaques, Lago Turquesa, Morenas da Geleira, Torres de Cata-Vento, Vale do Wakhan.
  - **Longe (80–98):** Geleira do Teto do Mundo com a Cabana do Iaque (posto) e Campo de Seracs (38: **Grampos de Escalada**),
    Desfiladeiro dos Ventos e Forte dos Ventos (39, o forte é o posto: **asas**), Alto do Wakhan, Passo de Wakhan com o Caravançará
    de Pedra (posto) e Ninho de Aydahar (40: **Capa de Tempestade**, sem o chefe), Picos Gelados e Cristas do Vento Ruim.
  - **Água:** o Rio do Wakhan desce do passo pelo vale das caravanas, passa a leste da cidade pelas calhas e acaba numa lagoa de
    degelo ao sul (leito cavado, atravessado só em 4 pontes de pedra); Lago Turquesa e lagoas no vale e nas morenas.
  - **Relevo:** `ReliefShape.Moraines` (cristas de cascalho), `IceTongue` (língua de gelo com sulcos de fenda), `SeracField` (blocos
    de gelo sobre um chão alto) e `Peaks` (cristas agudas, até ~42 m nos Picos Gelados); terraços (`Terraced`) na cidade, nos
    damasqueiros, nas calhas, na neve e no passo; gargantas no forte; vales quase planos junto da água.
  - **Cenário** (`OverworldDecorKit.Pamir.cs`, `OverworldChunkBuilder.Pamir.cs`; só linhas da paleta que todo build já tem):
    damasqueiros em flor, zimbros velhos, cabanas de pedra de teto plano, currais (do kit da Serra), calhas de pedra polida sobre
    pilares descendo a encosta e estações de madeira com carrinho, seracs azuis, cata-ventos de madeira em torres de pedra, torres
    de vigia com sinos de vento, bandeirolas coloridas das caravanas (sem rito), escamas caídas de Aydahar; muros de pedra seca nos
    terraços e pastagens; detalhe de chão carimbado: fendas azuis na geleira, tocas de marmota, cristais de gelo, montes de neve e
    névoa (do kit da Serra).
  - **Bichos** (nenhum chefe): os próprios Urso-Pardo do Pamir Apagado, Marmota Gigante, Iaque Selvagem Apagado (também o Argali
    até ter o seu), Almas e Irbis Apagado; corvo (Gralha-de-Bico-Vermelho e Gralha das Neves), pedra-que-rola (Pedra-Rolante), golem
    (Ushabti de Calha e Guarda de Pedra), abutre de brasa (Abutre-Barbudo), neblina do vento ruim, lobo (Lobo de Gelo), lodo
    (Fenda Viva), lanterna quebrada (Pipa de Fumaça), gavião (Gavião-Real), morcegos (Morcego do Vento), orc coroado (Bandoleiro de
    Āz); à noite lobisomem, rasga-mortalha, morcego-vampiro, mãe-da-lua e a queixada. Clima: neve nas calhas, terraços de neve,
    geleira, seracs, cristas, passo e picos; névoa no degelo, no lago e nos penhascos de Aydahar; céu limpo na estepe e no vale.
  - **Orçamento** (.NET fora do Editor, todos os 2704 pedaços): ~3,5 mil triângulos em média, pior 7,6 mil; geração ~0,44 ms em
    média, p95 ~0,7 ms. Testes `OverworldTetoTests` e `OverworldWorldTests` (rota a pé Vila → Sangkent, ~4,6 km pela Serra).

Capital **Sangkent** (§2): persa *sang* = pedra, túrquico *kent* = cidade. Encruzilhada das caravanas: as **cinco luzes**
aparecem juntas (cinco cores nas fitas dos carrinhos). NPCs com nomes maniqueus: **Abā** (missionário; aqui, o chefe das
calhas), **Kustai** (escriba do Códice de Colônia; aqui, cuida do quadro de avisos da caravana), **Baraies** (o mestre do
Códice; aqui, professor e contador de mapas). Chefe do mundo: **Aydahar** (mapa 40).

### 36 · Sangkent, a Cidade-Escadaria / *Sangkent, the Stair City* — nv 70–76 · ★ · capital (as cinco luzes)
- **Fonte:** fortes e vilas em terraço do Pamir e de Hunza (Baltit, Altit), casas-torre de Shibam (Iêmen); a Rota da Seda
  pelo Pamir.
- **Bioma e visual:** vale alto com damasqueiros em flor e geleira ao fundo; estilo `StoneTerrace` e paleta (§2.1);
  canais de irrigação, choupos, pontes de pedra.
- **Layout:** GG. **7 terraços** de casas; **calhas de pedra** e **carrinhos** entre os terraços (transporte grátis,
  1 por vez, fila); praça no 4º terraço; mercado no 1º; o forte no 7º. Retorno: Sangkent.
- **Inimigos (arredores):** Urso-Pardo do Pamir Apagado [tanque] (bicho real); Gralha-de-Bico-Vermelho [rápido] (bicho
  real); Marmota Gigante [enxame]; Pedra-Rolante [controle].
- **No jogo (2026-11-11): bichos próprios no lugar dos emprestados** (`MidStandInBeastRecipes`, `MidStandInBestiaryTexts`, `OverworldLayouts.MidBeasts`; modelo com rig, loot base e página no Caderno): Gralha-de-Bico-Vermelho (`enemy_mountain_red_billed_chough`) nas gralhas dos Terraços de Sangkent, do Vale dos Damasqueiros, do Choupal dos Canais e dos Terraços Altos, no lugar do corvo-da-treva (que segue de acompanhante em outros grupos). Falta: Pedra-Rolante própria (é a Pedra que Rola da Serra).
- **Chefe:** nenhum na capital; evento **Avalanche** (desce a neve, os primos protegem os terraços de baixo).
- **Mecânica:** **calhas**: descer de carrinho é rápido, subir pede escada ou elevador de corda (fila); o carrinho pode
  levar até 5 (o grupo).
- **Ganchos:** loot damasco seco, lã de iaque · montaria nova **Iaque do Teto do Mundo** (§6) · casa: conjunto novo
  **Terraço do Pamir** (fogão de pedra, tapete de feltro).
- **Noite:** os terraços acendem um a um; estrelas fortes; frio (fôlego mais lento fora de casa).
- **No jogo (2026-11-03): cidade de verdade** (`CityTownRecipes.Sangkent.cs`, estilo `StoneTerrace`, `CityCatalog.LateRegionCities`): em y = 0 dentro de um círculo de 22,5 m (saídas O, L e S a 19 m; ao norte os muros de arrimo sobem em três degraus de terraço com o forte do 7º terraço no meio e duas calhas de pedra descendo até os patamares): casas de pedra e barro de dois andares amarradas com vigas de zimbro (pontas à mostra), varanda de treliça, terraço com claraboia e damascos secando; o Marco das Caravanas na praça com as cinco fitas das cinco luzes, carrinhos de tábua com as fitas, canais de irrigação com laje, damasqueiros em flor; Abā, Sisinnios, Kustai e Baraies com visual próprio e falas para cada primo; Agasalhos do Sisinnios com a Roupa Térmica (36), os Grampos (38) e a Capa de Tempestade (40); missões "Pedras nas Calhas" e "Neblina no Quadro" (paga com a Capa de Tempestade); respawn na praça para 36–40. Falta: as calhas e carrinhos andáveis, os 7 terraços de verdade, a Avalanche como evento da cidade, o Iaque e o Terraço do Pamir. *(2026-11-11: o mundo contínuo do Teto já existe, com esta receita; as avalanches das calhas estão no 37.)*

### 37 · Calhas de Pedra / *Stone Chutes* — nv 74–80 · ★
- **Fonte:** invenção (as calhas de Sangkent continuam montanha afora, feitas pelos Artífices para descer minério).
- **Bioma e visual:** encosta íngreme com calhas de pedra polida; pedra `#8C8173`, gelo `#DCEAF0`, madeira; pilares,
  estações de troca de carrinho.
- **Layout:** G (vertical). Rede de 4 calhas com estações; **posto: Estação da Calha Alta**. Retorno: Sangkent.
- **Inimigos:** Ushabti de Calha [tanque]; Abutre-Barbudo Apagado [distância, solta pedras] (bicho real); Marmota
  Gigante [enxame]; Neblina do Vento Ruim [controle].
- **No jogo (2026-11-11): bichos próprios no lugar dos emprestados** (`MidStandInBeastRecipes`, `MidStandInBestiaryTexts`, `OverworldLayouts.MidBeasts`; modelo com rig, loot base e página no Caderno): Abutre-Barbudo Apagado (`enemy_mountain_bearded_vulture`, [distância], solta pedras) nos abutres das Calhas de Pedra, das Calhas Geladas, do Lago Turquesa, do Vale do Wakhan e do Ninho de Aydahar, no lugar do abutre de brasa. Falta: Ushabti de Calha (ushabti de pedra).
- **Chefe:** **O Carrinho Desgovernado**, engenho de minério dos Artífices que desce sozinho; libertado, vira o carrinho
  mais rápido da cidade.
- **Mecânica:** **descida cronometrada**: descer de carrinho pegando faíscas no caminho (corrida; ranking online).
- **No jogo (2026-11-09): avalanches nas calhas** (`TetoAvalancheRules`, `OverworldLayouts.ChuteLanes`, `CostaDeltaTetoMechanicsDriver`; a descida cronometrada continua por fazer): 4 faixas de 18 m nas Calhas de Pedra e Geladas, uma por vez em rodízio, ciclo de 90 s no relógio UTC: 60 s quieto, 20 s de aviso (sino e faixa marcada), 7 s da frente descendo, 3 s quieto. A frente acerta uma vez por passagem: 22% da vida, nunca abaixo de 20%, e só quem foi avisado naquele ciclo. Não deixa neve nem parede: a estrada que cruza uma faixa só corre perigo nos 7 s, e nenhuma faixa passa por ponte.
- **Ganchos:** loot minério de calha, peças de engenho · montaria `mount_giant_armadillo` (Despertar) · casa: miniatura
  de carrinho.
- **Noite:** lanternas nas estações; as calhas congelam (mais rápidas, mais difíceis).

### 38 · Geleira do Teto do Mundo / *Roof-of-the-World Glacier* — nv 80–86 · ★
- **Fonte:** as grandes geleiras do Pamir; o *almas*, "homem selvagem" peludo dos contos mongóis e da Ásia Central.
- **Bioma e visual:** geleira com fendas azuis; gelo `#BFD9E8`, neve `#F2F6F8`, rocha escura `#3E4248`; seracs, pontes de
  neve, morenas.
- **Layout:** G. Uma língua de gelo com 3 ramos; **posto: Cabana do Iaque**. Retorno: Sangkent.
- **Inimigos:** Iaque Selvagem Apagado [tanque] (bicho real); Almas [elite, tímido, joga bolas de neve]; Lobo de Gelo
  [rápido]; Fenda Viva [controle, a fenda que "abre"].
- **No jogo (2026-11-11): bichos próprios no lugar dos emprestados** (`MidStandInBeastRecipes`, `MidStandInBestiaryTexts`, `OverworldLayouts.MidBeasts`; modelo com rig, loot base e página no Caderno): Lobo de Gelo (`enemy_mountain_ice_wolf`) nos lobos do Campo de Seracs, dos Picos Gelados e das Morenas da Geleira, no lugar do lobo da floresta; os Almas dos Picos Gelados, alvo da etapa da história do meio, ficam. Falta: Fenda Viva (lodo de gruta).
- **Chefe:** **O Grande Almas**, peludo gigante que esconde faíscas na neve por medo; libertado, vira o guia da geleira.
- **Mecânica:** **frio**: medidor de frio fora dos abrigos (lentidão leve quando cheio); fogueiras e chá de karkadé zeram.
- **Ganchos:** loot gelo eterno (refino), lã · montaria `mount_snow_tiger` (Despertar), `mount_woolly_mammoth` · casa:
  **Picos de Litema**.
- **Noite:** o gelo brilha azul; o Almas sai; frio dobra.

### 39 · Forte dos Ventos / *Fort of the Winds* — nv 84–92 · ★
- **Fonte:** fortes de Baltit e Altit (Hunza), com torres de madeira e pedra sobre o vale.
- **Bioma e visual:** forte em cima de um rochedo, cata-ventos de madeira gigantes; madeira `#6B4A2E`, cal `#EDE6D6`,
  pedra, céu `#9EC2E3`; varandas de treliça, sinos de vento.
- **Layout:** G. O forte é o posto (zona segura no pátio de cima); 4 torres de cata-vento em volta; desfiladeiro embaixo.
  Retorno: Sangkent.
- **Inimigos:** Pipa de Fumaça [distância] (invenção); Gavião-Real Apagado [rápido] (bicho real); Guarda de Pedra Ushabti
  [tanque]; Morcego do Vento [enxame].
- **Chefe:** **O Senhor do Cata-Vento**, engenho dos Artífices que manda no vento do vale; libertado, gira para o lado
  certo e o vale volta a ter brisa boa.
- **Mecânica:** **direção do vento**: girar os 4 cata-ventos muda o vento do mapa (projéteis a favor vão mais longe;
  planar fica possível em um lado).
- **Ganchos:** loot madeira de nogueira, sinos · montaria `mount_hippogriff` (Despertar) · casa: conjunto novo **Sino de
  Vento**.
- **Noite:** os sinos tocam sozinhos (aviso de bicho perto); visão boa.

### 40 · Passo de Wakhan / *Wakhan Pass* — nv 90–98 · ★
- **Fonte:** o corredor de Wakhan, passo da Rota da Seda entre o Pamir e o Hindu Kush; *aydahar*, o dragão dos contos
  cazaques.
- **Bioma e visual:** passo alto e longo, rio de degelo, neve; rocha `#7A6E62`, neve, rio `#8FC7D6`, bandeirolas de pano
  colorido (das caravanas, sem rito); caravançará de pedra.
- **Layout:** GG. Estrada única com 4 alargamentos; **posto: Caravançará de Pedra** (meio do passo); arena do dragão no
  alto. Retorno: Sangkent.
- **Inimigos:** Argali Apagado [tanque] (bicho real); Irbis Apagado [rápido] (o leopardo-das-neves); Bandoleiro de Āz
  [elite]; Gralha das Neves [distância].
- **Chefe:** **chefe do mundo: Aydahar, o Dragão do Passo** (folclore cazaque): enrolado no passo, guarda o ouro de Āz
  sem saber por quê; libertado, voa embora e o passo abre.
- **Mecânica:** **escolta da caravana**: evento de 20 min levando a caravana do Ammo de ponta a ponta (avalanches,
  emboscadas); recompensa grande.
- **Ganchos:** loot escama de aydahar, seda · montaria nova **Camelo-Bactriano Branco** (§6) · casa: troféu de Aydahar.
- **Noite:** o passo fecha para caravanas (só herói); avalanches mais frequentes; aurora.
- **No jogo (2026-11-11): chefe do mundo Aydahar** (`enemy_mountain_aydahar`, nv 98): arena no anel de pedras no alto da trilha do Caravançará ao Ninho de Aydahar (560, 1640 na região); aparece com o herói a 90 m, some a 125 m (de volta à vida cheia) e volta 5 min depois de cair; acima de 66% golpe e anel de força, 66–33% rajada de 3 tiros a cada 8 s, abaixo de 33% rajada de 5 e o Grande Golpe marcado no chão (2 s, 4,5 m); as clareiras comuns ficam a 22 m ou mais do anel, chão plano e trilha até ele; peso 0 no bioma (nunca sai em encontro). Própria: **sopro do passo** a partir de 66% (a cada 11 s, 2 s de aviso): linha de 16 m × 4 m dele até onde o herói estava (passa da marca); um passo para o lado basta. Libertado, voa embora (não fica corpo). Loot próprio (2026-11-11): **Escama de Aydahar** (`item_chefe_escama_aydahar`, rara, 55% por vitória, 1–2, 160 de venda; no lugar do punhado de ouro, o loot do bioma segue igual); vai no Tear/Forja da casa (`HouseCraftRules`): Saia-calça de relâmpago da Maithe (Tear de mestre). Troféu de casa (2026-11-11): **Estandarte de escamas de Aydahar** (`casa_trofeu_027`, épico, parede), feito na Forja da casa com 3 Escama de Aydahar e o chefe já vencido (`HouseCraftRules.WorldBossTrophies`). Falta: a escolta da caravana e o Camelo-Bactriano Branco.

### Região 8 — Planície de Mingcheng (Luz) · mapas 41–46

Capital **Mingcheng** (§2): chinês *míng* 明 = luz, claro; *chéng* 城 = cidade murada. Gancho: a Mingjiao, a "Doutrina da
Luz", nome chinês do maniqueísmo (*Compêndio*, 731; o santuário de Cao'an em Quanzhou é o último de pé). NPCs: **Mestre
Mōzak** (título dos mestres maniqueus em chinês e sogdiano, *mozhak*; aqui, o guardião das lanternas), **Bögü, o Guarda do
Portão** (Bögü Qaghan, o khagan uigur que adotou o maniqueísmo em 762; aqui, o velho chefe da guarda dos anéis), **Lin
Deng** (maniqueu da era Song em Fujian **[verificar]**; aqui, o mestre do canal). Chefe do mundo: **Os Quatro Gulosos**
(mapa 46).

- **Mundo contínuo (2026-11-03; 43–46 no catálogo em 2026-11-09): mapas 41–46 jogáveis a pé.** A região inteira é um mapa
  de 2400 × 2400 m (`OverworldLayouts.Mingcheng`, `MingchengId`) costurado ao norte do Zarafshan e a leste do Teto do Mundo, sem
  portão. Mingcheng (`CityTownRecipes.Mingcheng()`, Backdrop desligado, chão plano de ±30 m em y = 0; ruas O/L/S, a muralha do 7º
  anel fecha o norte, a estrada do norte sai da estrada leste) fica em (0, 0), 600 m da borda sul; as Torres Brancas
  (`CityTownRecipes.TorresBrancas()`, ±28 m, ruas O/L/S) em (960, 600). Grade 5 × 5 de áreas (colunas x −1200/−720/−240/240/720/1200,
  linhas z −600/−120/360/840/1320/1800), nível subindo com a distância de Mingcheng a partir de 90:
  - **Perto (90–96):** Anéis de Mingcheng e Vilas do Anel (41), Caminho dos Leões (41, a entrada pelo Zarafshan), Campos de Painço,
    Rio dos Salgueiros (o alto Rio Dourado), Eclusa da Lua (43) e Fossos de Terracota (41). **Nada pede item até Mingcheng** (o
    Selo do Sol só é vendido lá).
  - **Meio (95–102):** Colinas de Chá, Canal das Lanternas (43), Arrozais Espelhados e Moinho d'Água (44, entrada pelo Teto),
    Bambuzal do Oeste, Ravinas de Loess, e a leste os Jardins em Terraço e as Torres Brancas (42: **Selo do Sol**).
  - **Longe (104–126):** Picos de Calcário, Cristas dos Pinheiros, Vale dos Guerreiros (terracota), Colinas dos Espelhos e Vale da
    Névoa (o atalho dos espelhos, retorno às Torres), e ao norte a Muralha das Mil Torres (45): Muralha Oeste, Torre Quinhentos,
    a muralha, Portão do Nian e Picos do Nian (retorno às Torres Brancas). 43–46 estão no catálogo: ver "No jogo" de cada ficha.
  - **Água:** o Rio Dourado nasce na cabeceira do canal e desce até a borda sul (continua no Zarafshan); o Canal das Lanternas (14 m)
    sobe para o norte até o Lago da Carpa, sob as colinas brancas; leitos cavados, só pontes de pedra (3); lagoa de lótus nos anéis,
    lagoa do moinho.
  - **Relevo:** planície baixa (≤ 5 m) em volta da cidade, do rio e do canal; `Terraced` nos arrozais (6 degraus), no chá e nas
    colinas brancas; `KarstTowers` (torres de calcário de ~30 m sobre chão plano; trilhas e clareiras no chão); `Peaks` nos
    pinheiros; `WallRidge`, a crista contínua sob a muralha (`OverworldLayouts.GreatWallZ`, nas coordenadas da região); `Gorges`
    no loess; `Mounds` (túmulos) nos fossos.
  - **Cenário** (`OverworldDecorKit.Mingcheng.cs`, `OverworldChunkBuilder.Mingcheng.cs`; só linhas da paleta que todo build já tem):
    salgueiros-chorões, lanternas vermelhas, pavilhões de telhado curvo, leões de pedra, tartarugas com estela, fossos de
    soldados de terracota, a **muralha contínua** pela crista (peças de 6 m numa grade que casa entre pedaços, aberta onde passam
    trilhas, com torre de farol a cada 120 m), bambu, torres de calcário, pinheiros de copa chata, espelhos de bronze, torres
    brancas e pirâmides de Meroé nos jardins, roda-d'água e espantalhos nos arrozais; fileiras de chá e diques seguindo a curva
    de nível; detalhe carimbado: arroz, painço, brotos de bambu, lótus no canal, névoa.
  - **Bichos** (existentes, nenhum chefe): golem (Leão de Pedra, Guardião de Cal), chacal (Raposa de Cauda Lanterna), gavião (Grou
    de Papel, Íbis de Luz), orc coroado (Soldado de Terracota), lanterna quebrada (Arqueiro de Terracota, Espelho Quebrado,
    Espantalho), cobra-da-areia (Carpa-Dragão), peixe-lanterna (Lanterna-Peixe), neblina do vento ruim (Barqueiro de Fumaça, Raio
    Torto), caranguejo-coco (Tartaruga de Pedra), queixada (Búfalo de Lama), gaivota ladra (Pato-Mandarim Ladrão), morcegos
    (Gafanhoto de Fumaça), babuíno (Filhote de Nian, macacos), lobo (Cavalo de Terracota), aranha (bambu), escorpião e abutre
    (loess), pedra-que-rola e corvo (calcário, muralha); à noite lobisomem, rasga-mortalha, morcego-vampiro, mãe-da-lua,
    queixada e vaga-lumes nos arrozais. Clima: névoa no canal, no chá, no bambu, no calcário, nos pinheiros e na muralha; poeira
    no loess e nos fossos; céu limpo no painço, nas colinas brancas e nos jardins.
  - **Orçamento** (.NET fora do Editor, todos os 2704 pedaços): ~3,7 mil triângulos em média, pior 10,6 mil (anel de fundo); geração
    ~0,44 ms em média, p95 ~0,75 ms. Testes `OverworldMingchengTests` e `OverworldWorldTests` (rota a pé Vila → Mingcheng, ~2,1 km
    pelo Caminho dos Leões).

### 41 · Mingcheng, a Cidade dos Nove Anéis / *Mingcheng, City of Nine Rings* — nv 90–96 · ★ · capital (Luz)
- **Fonte:** cidades imperiais muradas da China (planta em grade de Chang'an, Cidade Proibida, muralha de Xi'an).
- **Bioma e visual:** planície de rio com a cidade em 9 anéis; estilo `RingWall` e paleta (§2.1); salgueiros, pontes de
  arco, lanternas de papel.
- **Layout:** GG. Anéis 1–3 (campo e vilas, abertos), 4–6 (bairros de ofício: seda, papel, laca, chá), 7–9 (cidadela,
  fechada até o mapa 46). Portões de 3 arcos em cruz; praça no 6º anel com a **Torre do Tambor e do Sino** (relógio da
  cidade). Retorno: Mingcheng.
- **Inimigos (anéis de fora):** Leão de Pedra Apagado (as estátuas de guarda dos portões) [tanque]; Raposa de Cauda
  Lanterna [rápido] (a raposa esperta dos contos chineses); Grou de Papel [distância] (invenção); Soldado de Terracota
  Apagado [tanque] (o exército de barro de Xi'an como imagem).
- **Chefe:** nenhum na capital; evento **Festa das Lanternas** (bíblia §3.20: Yuanxiao) com charadas nas lanternas.
- **Mecânica:** **portões por hora**: os portões dos anéis abrem e fecham com o tambor (dia) e o sino (noite); dentro
  dos anéis 4–6, Barcas de canal ligam os bairros. G4 da Maria (Lâmpada de Mingcheng) é aqui.
- **Ganchos:** loot papel, laca, chá · montaria nova **Qilin de Jade** (variante, §6) e `mount_kirin` (Despertar) · casa:
  conjunto novo **Pátio dos Nove Anéis** (biombo laqueado, lanterna de seda).
- **Noite:** milhares de lanternas; o sino fecha os anéis de fora (quem está fora fica fora até o tambor); visão ótima.
- **No jogo (2026-11-03): cidade de verdade** (`CityTownRecipes.Mingcheng.cs`, estilo `RingWall`, `CityCatalog.LateRegionCities`): em y = 0 dentro de um círculo de 22,5 m (saídas O, L e S a 19 m; ao norte a muralha do 7º anel em três lances, com o portão de três arcos da cidadela fechado e a torre do portão de telhados dourados no meio, e os telhados dos anéis de dentro atrás): salões de tijolo cinza com colunas de laca vermelha, janelas de treliça, telhado curvo de telha cinza com cantos virados e lanternas de papel no alpendre; a Torre do Tambor e do Sino na praça, canais com pontes de corcunda, salgueiros e armações de lanternas com charadas (uma com a lanterna-carpa); portão de três arcos nas ruas; Bögü, Mestre Mōzak, Lin Deng e Fuduodan com visual próprio e falas para cada primo; Lanternas do Mestre Mōzak com o Selo do Sol (42); missões "Leões nos Portões" e "Peixes no Canal" (paga com o Selo do Sol); respawn na praça para 41 (e 43, 44, 46 quando entrarem no catálogo). Falta: os 9 anéis de verdade, os portões por hora, as Barcas de canal, a Festa das Lanternas, a Lâmpada de Mingcheng (G4 da Maria), o Qilin de Jade e o Pátio dos Nove Anéis.

### 42 · Torres Brancas de Amanirenas / *White Spires of Amanirenas* — nv 95–102 · ★ · cidade de raça (altos-elfos)
- **Fonte:** nomes kushitas (§3.3); pirâmides de Meroé como base das torres.
- **Bioma e visual:** colinas brancas a leste de Mingcheng, terraços de jardim; paleta das Torres (§3.3).
- **Layout:** G. **Torres Brancas** (§3.3) em 7 torres ligadas por pontes de folha; jardins em terraço embaixo; o atalho
  dos espelhos leva ao mapa 45. Retorno: Torres Brancas.
- **Inimigos:** Espelho Quebrado [distância, reflete]; Íbis de Luz Apagada [rápido]; Guardião de Cal [tanque]; Raio Torto
  [controle, luz que cega por 1 s].
- **Chefe:** **O Pequeno Eclipse**, um disco de sombra que gruda nas torres e rouba a luz; libertado, vira o "relógio de
  sombra" da praça.
- **Mecânica:** **Relógio de espelhos** (§3.3).
- **Ganchos:** loot vidro de luz, ouro claro · montaria nova **Cervo de Luz Branca** (§6) · casa: conjunto novo **Torre
  Branca** (espelho de bronze, astrolábio).
- **Noite:** os espelhos passam a luz da lua; as pontes de folha brilham; visão ótima.
- **No jogo (2026-11-03): cidade de verdade** (`CityTownRecipes.TorresBrancas.cs`, estilo `WhiteSpire`, `CityCatalog.RaceCities`): em y = 0 dentro de um círculo de 22,5 m (saídas O, L e S a 19 m; ao norte os jardins em terraço sobem em três lances com as torres brancas, a mais alta no meio com a cúpula de vidro, o espelho grande e pontes de folha até duas torres menores): casas-torre brancas sobre base de arenito, com cornija de ouro claro, janelas altas de luz, torre fina com cúpula de vidro e espelho de bronze passando o raio até a porta, pórtico de colunas brancas; o Relógio de Espelhos na praça, riachos com pontes de folha, postes de espelho, pirâmides pequenas de Meroé; portão de dois pilones com o disco de ouro; Amanirenas, Amanitore, Natakamani, Shanakdakhete e Arkamani (altos-elfos de pele escura e cabelo branco) com falas para cada primo; Espelhos de Natakamani (pedras do refino; o Selo do Sol se compra em Mingcheng); missões "Espelhos Quebrados" e "As Íbis Apagadas" (paga com a Lamparina Rúnica); respawn na praça para 42 (e 45 quando entrar no catálogo). Falta: o Relógio de Espelhos como quebra-cabeça e o atalho para o 45, as 7 torres andáveis, O Pequeno Eclipse, o Cervo de Luz Branca e a Torre Branca.

### 43 · Canal das Lanternas / *Lantern Canal* — nv 102–110 · ☆
- **Fonte:** o Grande Canal da China; a lenda da carpa que pula o Portão do Dragão (conto popular).
- **Bioma e visual:** canal largo entre salgueiros e vilas de água; água `#6FA3A8`, salgueiro `#8DB25A`, telha cinza,
  laca; eclusas de pedra, barcas de carga, pontes de corcunda.
- **Layout:** G. Canal de norte a sul com 4 eclusas; **posto: Eclusa da Lua**. Retorno: Mingcheng.
- **Inimigos:** Carpa-Dragão Apagada [rápido]; Barqueiro de Fumaça [elite]; Lanterna-Peixe [enxame]; Tartaruga de Pedra
  (a tartaruga que carrega estelas) [tanque].
- **No jogo (2026-11-11): bichos próprios no lugar dos emprestados** (`MidStandInBeastRecipes`, `MidStandInBestiaryTexts`, `OverworldLayouts.MidBeasts`; modelo com rig, loot base e página no Caderno): Carpa-Dragão Apagada (`enemy_forest_dragon_carp`) nas carpas da Eclusa da Lua e do Rio dos Salgueiros, no lugar da cobra-de-areia; as tartarugas de pedra da eclusa, alvo da etapa da história do meio, ficam. Falta: Lanterna-Peixe (peixe-lanterna) e Barqueiro de Fumaça (neblina).
- **Chefe:** **A Carpa que Não Pulou**, carpa enorme que desistiu de pular a cascata e ficou apagada; libertada, pula e
  vira um arco de luz.
- **Mecânica:** **eclusas**: abrir e fechar as eclusas muda o nível da água e abre os bairros baixos.
- **Ganchos:** loot escama de carpa, seda de canal · montaria `mount_river_seahorse` · casa: **Festa das Lanternas**
  (Lanterna-peixe).
- **Noite:** lanternas no canal; o Barqueiro de Fumaça rema.
- **No jogo (2026-11-09): zona do mundo contínuo.** A área `canal_das_lanternas` é o mapa (nv 102–110, a pé 98, barca 106); a `eclusa_da_lua` fica como área de entrada dele (nv 92–96, sem subir a faixa, para não fechar a estrada leste às Torres Brancas). O posto Eclusa da Lua, com Cais da Barca, fica em (600, 560), na margem leste dentro da área do canal, ligado à estrada leste e à do norte. Exigência: nenhuma (a Metodologia pede "Barco de Bambu", que não existe nem é vendido em Mingcheng ou nas Torres). Falta: as 4 eclusas que mudam a água, A Carpa que Não Pulou, a montaria e a casa.

### 44 · Arrozais Espelhados / *Mirror Rice Terraces* — nv 108–116 · ☆
- **Fonte:** terraços de arroz em montanha (Yuanyang, Longji) como forma; a Barca da Lua refletida na água (bíblia §1.5).
- **Bioma e visual:** encostas de terraços alagados que espelham o céu; água-espelho `#A9C6D9`, lama, arroz verde
  `#8FC25A`, vila de casas de barro com telhado de palha em cogumelo.
- **Layout:** G. Encosta com 40 terraços; trilhas nas bordas; **posto: Moinho d'Água**. Retorno: Mingcheng.
- **Inimigos:** Búfalo de Lama [tanque]; Pato-Mandarim Ladrão [rápido] (bicho real); Gafanhoto de Fumaça [enxame];
  Espantalho Apagado [controle].
- **Chefe:** **O Espantalho de Palha Dourada**, que espanta até a luz; libertado, espanta só os gafanhotos.
- **Mecânica:** **mundo do reflexo**: olhar a água (botão) mostra o reflexo, onde aparecem trilhas e baús que não
  existem no seco; pisar na trilha refletida funciona por 10 s.
- **Ganchos:** loot arroz, palha dourada · montaria nova **Búfalo-d'Água** (Despertar) · casa: conjunto novo **Arrozal**.
- **Noite:** a lua no espelho dobra as trilhas do reflexo; vaga-lumes.
- **No jogo (2026-11-09): zona do mundo contínuo.** A área `arrozais_espelhados` é o mapa (nv 108–116, a pé 104, barca 112); o `moinho_dagua` fica como área de entrada (nv 97–101), e é por ele que a segunda passagem do Teto do Mundo continua aberta. O posto Moinho d'Água, com Cais, fica em (−960, 240), dentro dos arrozais; a roda-d'água continua na área do moinho, ao norte. Exigência: nenhuma. A primeira passagem do Teto (pelas Calhas) entra direto nos arrozais e passa a pedir nível 104. **Mundo do reflexo (2026-11-09, `MingchengReflectionRules`):** seis poças nos arrozais (`MirrorPools`); [E] olha a água e, por 10 s (20 s à noite; depois 12 s até poder olhar de novo), aparecem a trilha refletida da poça (+25% de velocidade nela, 3 m para cada lado) e um baú no fim dela, que abre uma vez por dia de jogo e dá poção de vida, poção de mana ou erva, em rodízio. Sem dano e sem bloqueio: acabado o reflexo, é só o arrozal. Falta: O Espantalho de Palha Dourada, o Búfalo-d'Água e o conjunto Arrozal.

### 45 · Muralha das Mil Torres / *Wall of a Thousand Towers* — nv 115–125 · ☆
- **Fonte:** a Grande Muralha (forma); o *nian*, a fera do Ano-Novo dos contos chineses, que foge do vermelho e do
  barulho.
- **Bioma e visual:** muralha sobre cristas de montanha; tijolo cinza, laca, pinheiro `#3E6B4A`, névoa; torres de vigia,
  escadarias íngremes, faróis.
- **Layout:** GG (comprido). A muralha é o caminho, com 12 torres; descidas para vales; **posto: Torre Quinhentos**.
  Retorno: Torres Brancas.
- **Inimigos:** Soldado de Terracota [tanque]; Arqueiro de Terracota [distância]; Cavalo de Terracota [rápido]; Filhote de
  Nian [elite].
- **No jogo (2026-11-11): bichos próprios no lugar dos emprestados** (`MidStandInBeastRecipes`, `MidStandInBestiaryTexts`, `OverworldLayouts.MidBeasts`; modelo com rig, loot base e página no Caderno): Cavalo de Terracota (`enemy_forest_terracotta_horse`) nos cavalos dos Fossos de Terracota, do Vale dos Guerreiros e da Torre Quinhentos, no lugar do lobo; os soldados de terracota dos fossos, alvo da etapa da história do meio, ficam. Falta: Arqueiro de Terracota (lanterna quebrada).
- **Chefe:** **Nian, a Fera do Ano**: assusta-se com lanternas vermelhas e com barulho (granada do Ícaro, tambor); a luta
  é espantar, não bater; vencida, vai dormir até o ano que vem.
- **Mecânica:** **faróis**: acender os faróis das torres liga um "corredor de luz" (velocidade +10% a pé dentro dele).
- **Ganchos:** loot tijolo vidrado, lanterna vermelha · montaria nova **Cavalo de Terracota Desperto** (§6) · casa: **Festa
  das Lanternas**.
- **Noite:** os faróis acesos ficam; o nian é mais ativo; névoa nos vales.
- **No jogo (2026-11-09): zonas do mundo contínuo.** Quatro áreas são o mapa: Muralha Oeste, Torre Quinhentos, Muralha das Mil Torres e Portão do Nian (nv 115–124, mapa a pé 110, barca 120); posto Torre Quinhentos com Cais em (−400, 1380). Exigência: **Tocha** (`TorchRequired`, a Lamparina de Óleo do balcão fixo das Lanternas de Mōzak, em Mingcheng), barreira suave em todas as quatro; a estrada norte até o Mar Gelado passa por elas e também pede a lamparina. **Muralha andável e faróis (2026-11-09, `MingchengWallRules`, `MingchengBeaconRules`):** o adarve fica a 3,4 m do chão, de x −1140 a 708, costurado por cima de portões e torres (as torres ganharam passagem aberta); 12 torres com escada (x −720 a 600, de 120 em 120 m): [E] sobe do chão, [E] em cima acende o farol e, com ele aceso, desce. Farol queima 300 s (de dia; à noite fica). Entre duas torres vizinhas acesas, o corredor de luz: +10% a pé, só em cima da muralha. Os lados são abertos: cair são 3,4 m sem dano. Falta: Nian e o Cavalo de Terracota Desperto.

### 46 · Cidadela dos Quatro Gulosos / *Citadel of the Four Gluttons* — nv 125–140 · ☆
- **Fonte:** os "Quatro Perigos" das lendas chinesas antigas (*Zuo Zhuan*, *Shan Hai Jing*): **Taotie** (o guloso, a cara
  de bronze dos vasos antigos), **Hundun** (o caos sem rosto, perfeito para a Mistura), **Qiongqi** (tigre alado que
  inverte o certo e o errado) e **Taowu** (o teimoso que não escuta). Monstros de lenda, não divindades.
- **Bioma e visual:** a cidadela no centro de Mingcheng (anéis 7–9); telha dourada, laca, bronze de vaso antigo
  `#6E7F5E`, jade; 4 palácios, um por guloso, pátios de pedra branca.
- **Layout:** GG. **Posto: Portão do Nono Anel**; 4 palácios nos pontos cardeais; o Salão Central. Retorno: Mingcheng.
- **Inimigos:** Servos de Taotie (vasos de bronze que andam) [tanque]; Bolhas de Hundun [controle]; Filhotes de Qiongqi
  [rápido]; Guarda de Taowu [elite].
- **Chefe:** **chefe do mundo: os Quatro Gulosos**, um por semana em rodízio (online) e todos juntos no evento do fim de
  temporada; cada um é um "apetite" de Āz que ficou sozinho. Vencidos, voltam a ser figuras nos vasos de bronze.
- **Mecânica:** **o que cada um come**: Taotie come o loot do chão (pegar rápido), Hundun apaga o mapa (só lanternas
  mostram), Qiongqi troca os controles por 3 s (aviso grande, opção de acessibilidade desliga), Taowu não se mexe até ser
  "chamado" (Chamado e Resposta, a mecânica do Bema).
- **Ganchos:** loot bronze antigo, conjunto antigo "Vasos de Bronze" · montaria `mount_kirin` (Despertar III) · casa:
  troféu dos Quatro (vaso de bronze).
- **Noite:** os palácios trocam de guloso; Hundun mais forte.
- **No jogo (2026-11-09): zona do mundo contínuo, fora do lugar da ficha.** A área `cidadela_dos_quatro_gulosos` (nv 125–140, a pé 121, barca 132; 480 × 240 m) foi tirada da metade norte dos Picos do Nian, sem aumentar a região, com o posto Portão do Nono Anel e Cais em (960, 1660). A ficha a põe nos anéis 7–9 de Mingcheng, mas a cidade é só a receita de ±30 m e a estrada norte passa colada nela: uma área de nível 125 ali cortaria a região. Exigência: nenhuma. Os bichos são corpos existentes (soldado de terracota, neblina, lobo, leão de pedra). Falta: os 4 palácios, o Salão Central, a mecânica do que cada um come e os Quatro Gulosos.
- **No jogo (2026-11-11): chefe do mundo os Quatro Gulosos** (`enemy_mountain_four_gluttons`, nv 140): um corpo de vaso de bronze com uma cara de cada lado; arena no anel de pedras do Salão Central, a leste do Portão do Nono Anel, no fim de um ramal novo da trilha (1100, 1700 na região); aparece com o herói a 90 m, some a 125 m (de volta à vida cheia) e volta 5 min depois de cair; acima de 66% golpe e anel de força, 66–33% rajada de 3 tiros a cada 8 s, abaixo de 33% rajada de 5 e o Grande Golpe marcado no chão (2 s, 4,5 m); as clareiras comuns ficam a 22 m ou mais do anel, chão plano e trilha até ele; peso 0 no bioma (nunca sai em encontro). Própria: **o apetite da semana** a partir de 66% (a cada 12 s, 2,2 s de aviso): um por semana UTC em rodízio (Taotie, Hundun, Qiongqi, Taowu) e, à noite, o seguinte; Taotie come o chão marcado (sair de 4 m), Hundun apaga tudo menos a marca (ficar a até 3 m dela; +25% à noite), Qiongqi pega quem está a mais de 6 m dele (chegar perto), Taowu pega quem está a até 6 m (ficar longe). Vencidos, caem como os outros. Loot próprio (2026-11-11): **Bronze Antigo dos Vasos** (`item_chefe_bronze_antigo`, épico, 50% por vitória, 260 de venda; no lugar do punhado de ouro, o loot do bioma segue igual); vai no Tear/Forja da casa (`HouseCraftRules`): Saia-calça de relâmpago, Grevas do tronco milenar e Pernas mecânicas assistidas (Tear de mestre). Troféu de casa (2026-11-11): **Vaso de bronze dos Quatro Gulosos** (`casa_trofeu_028`, épico, piso), feito na Forja da casa com 2 Bronze Antigo e o chefe já vencido (`HouseCraftRules.WorldBossTrophies`). **Taotie come o loot do chão e Qiongqi troca os controles (2026-11-11):** o jogo não tem loot no chão (tudo vai direto para a mochila), então na semana de Taotie o vaso derrama **oferendas** de 40 Moedas Furadas (`item_gold_coin`) a 3–7 m dele (a primeira aos 3 s, depois a cada 8 s; até 3 no chão e 6 por luta); a cada 9 s (a primeira aos 6 s) Taotie marca com o anel a oferenda mais perto da boca (até 12 m) e avisa ("Taotie quer a oferenda! Pega ou deixa!"); 2,5 s depois a engole, a menos que o herói tenha passado por cima dela (1,6 m) e pegado 1×. O que ele engoliu volta **em dobro** quando ele cai (e as oferendas que sobraram no chão, 1×); se ele some sem cair, perde-se: pegar já ou arriscar. Qiongqi, no golpe do apetite, **troca os passos** de quem está na luta por 3 s (aviso no anel antes, "Passos trocados!" ao começar e "Os passos voltaram ao normal" ao acabar): teclado/controle/joystick andam ao contrário e o clique/toque no chão leva ao ponto espelhado pelo herói; clique/toque num inimigo continua mirando o inimigo, para quem joga no celular não ficar travado. `Player/HeroControlMirror` guarda só a hora do fim (teto de 4 s), então acaba sozinho com o herói morto, a luta acabada ou a cena trocada (o chefe ainda encerra cedo no OnDisable, na morte do herói e na própria queda). Regras em `RegionBossRules` (Taotie*/Qiongqi*), testes em `FourGluttonsAppetiteTests`. Falta: os 4 palácios e o evento de fim de temporada.

### Região 9 — Estepe dos Nartes (estepe e Cáucaso, Vento Vivo) · mapas 47–52 · ☆

Estepe de capim alto, kurgans e o Cáucaso ao fundo. Culturas: osseta (Nartes), cita (ouros e grifos), georgiana. Três
cidades de raça (centauros, gigantes-da-colina, povo-tronco). Estilo base `SteppeCamp`. Paleta: capim `#BFAE5A`,
feltro `#D8C9A8`, céu `#A7C7E7`, ouro cita `#D4A93A`, montanha `#6E7480`. Chefe do mundo: **Ohya** (mapa 52, o irmão do
Hahya guardado para o online, bíblia §1.4).

- **Mundo contínuo (2026-11-03): mapas 47–52 jogáveis a pé.** Os seis mapas ☆ entram no `MapCatalog` (`MapRegion.EstepeDosNartes`,
  níveis e cidades de retorno de §1; a pé/barca 96/104, 102/110, 108/116, 114/122, 120/128, 128/140; só o 52 pede item: **Grampos
  de Escalada**, vendidos em Sangkent) e a região inteira é um mapa de 2400 × 2400 m (`OverworldLayouts.Nartes`, `NartesId`)
  costurado ao norte da Costa e a leste de Mingcheng, sem portão. As três cidades usam as próprias receitas
  (`CityTownRecipes.Soslan()`, `.Fortim()`, `.Arvoredo()`, estilos `SteppeCamp`, `HillFort`, `LivingGrove`; `CityId` do catálogo
  `acampamento_soslan`, `fortim`, `arvoredo`; Backdrop desligado, raio de 22,5 m sobre o chão plano de ±30 m em y = 0):
  Acampamento de Soslan em (0, 0), 600 m da borda sul (ruas O/L/S); Fortim em (−480, 1080) (ruas O/S; a trilha das torres svan
  sai da estrada sul); Arvoredo em (−960, 1560) (ruas O/L/S); postos Kurgan Aberto (960, 120), Poço da Taça (960, 1080) e Torre
  de Vigia (0, 1560), todos com Cais.
  Grade 5 × 5 de áreas (colunas x −1200/−720/−240/240/720/1200, linhas z −600/−120/360/840/1320/1800), nível subindo com a
  distância do acampamento a partir de 100:
  - **Perto (100–108):** Acampamento de Soslan, Vau Largo (a entrada pela Costa) e Pistas de Corrida (47), Estepe de Soslan,
    Estepe das Saigas e, a leste, os Kurgans de Ouro (48). **Nada pede item no caminho do acampamento nem nas entradas.**
  - **Meio (104–120):** Rio dos Nartes, Pastos dos Gigantes, Campos de Papoula, Kurgan Aberto (48), Lago Salgado, Kurgans do Leste,
    Estepe dos Ventos e Bosque de Bétulas (as entradas por Mingcheng), Vale das Tábuas de Sonho e Fortim dos Gigantes (49), Vale
    das Torres Svan, Pradaria Ondulada.
  - **Longe (118–150):** Taiga de Lariços, Arvoredo de Haripushpa e Lago de Degelo (50), Pradaria dos Mil Cavalos (51) e o
    Cáucaso do 52 (Grampos): Torre de Vigia, Corrente de Amirani e Cume de Ohya.
  - **Água:** o Rio dos Nartes (12 m, leito cavado mas raso) desce das neves sob a corrente, passa pelas torres svan e a leste do
    acampamento até um lago ao sul; dois **vaus largos** (as estradas do acampamento) e uma ponte de pedra sob a Torre de Vigia;
    Lago de Degelo na taiga, lago salgado no absinto, lagoa das manadas.
  - **Relevo:** estepe baixa (≤ 6 m) no acampamento, no vau e no absinto; ondulado nas pradarias (~9–15 m); `Kurgans` (montes
    redondos de ~10 m sobre chão plano; trilhas e clareiras no chão) nos kurgans; `Terraced` nos montes dos gigantes (~14 m);
    `Hills` nas bétulas, no vale das tábuas, na taiga e nos vales svan (~20 m); `Peaks` na corrente (~30 m); `Massif` (estratos
    inclinados em cristas, ~34 m) no Cume de Ohya.
  - **Cenário** (`OverworldDecorKit.Nartes.cs`, `OverworldChunkBuilder.Nartes.cs`; só linhas da paleta que todo build já tem):
    tendas de feltro sobre carroças, estandartes de crina, babas de pedra, cervos de ouro no estilo animal cita, bétulas, lariços
    gigantes, tábuas de sonho, panelas de pedra gigantes, **muralhas de terra** seguindo a curva de nível dos montes, torres svan,
    poços de pedra, a **Corrente de Amirani** (elos de ferro deitados ao longo da linha `AmiraniChain`, de 5 em 5 m, sem colisor:
    é o caminho), salgueiros nos rios; detalhe carimbado: plumas de capim-pluma, papoulas, crosta de sal, neve e névoa.
  - **Bichos** (existentes, nenhum chefe): lobo (Lobo da Estepe, Garanhão Apagado, Lobo-Cinzento, Lince da Taiga, Lobo de Neve),
    gavião (Águia-Dourada, Águia de Ferro, Pica-Pau de Fumaça), abutre de brasa (Grifo Jovem, Grifo Guardião, Abutre da Estepe),
    chacal (Saiga Assustada, Cervo de Ouro, Cabra-Montês Gigante), gaivota ladra (Cisne de Feltro), golem (Lobo de Bronze), orc
    coroado (Gigante Sonâmbulo), queixada (Javali da Colina), corvo (Corvo-Mensageiro de Fumaça), caranguejo-coco (Besouro-Casca),
    lodo (Fungo Apagado), neblina do vento ruim (Tempestade de Capim, pesadelos), babuíno (Ochokochi), lobisomem como corpo do Urso
    do Cáucaso, pedra-que-rola, escorpião (sal), aranha (bétulas), morcegos (kurgans); à noite lobisomem, rasga-mortalha,
    morcego-vampiro, mãe-da-lua, queixada e vaga-lumes nas papoulas. Clima: céu limpo na estepe, no acampamento, nos kurgans, nas
    pradarias e na planície dos cavalos; poeira no absinto; névoa na taiga, no vale das tábuas e nos vales svan; neve na corrente e
    no cume.
  - **Orçamento** (.NET fora do Editor, todos os 2704 pedaços): ~3,9 mil triângulos em média, pior 7,6 mil; geração ~0,44 ms em
    média, p95 ~0,70 ms; pedaços das 25 passagens do mundo: pior 10,7 mil, ~0,6 ms. Testes `OverworldNartesTests` e
    `OverworldWorldTests` (rota a pé Vila → Acampamento de Soslan, ~4,0 km pela Costa).

### 47 · Acampamento de Soslan / *Soslan's Camp* — nv 100–108 · ☆ · cidade de raça (centauros)
- **Fonte:** sagas dos Nartes (§3.3).
- **Bioma e visual:** estepe aberta com rios rasos; paleta da região; tendas sobre carroças, estandartes de crina.
- **Layout:** GG. O **Acampamento de Soslan** (§3.3) anda entre 3 pontos do mapa por dia; pistas de corrida; vau largo.
  Retorno: Acampamento de Soslan.
- **Inimigos:** Lobo da Estepe Apagado [rápido]; Grifo Jovem [elite] (grifos dos ouros citas); Águia-Dourada Apagada
  [distância] (bicho real); Saiga Assustada [enxame] (bicho real).
- **Chefe:** **O Cavalo de Fumaça**, garanhão de sombra que leva os potros embora; libertado, vira cavalo de verdade.
- **Mecânica:** **cidade que anda** (§3.3) e **corrida de estepe** com os centauros (prêmios de sela).
- **Ganchos:** loot feltro, crina · montaria nova **Cavalo Dourado da Estepe** (§6) · casa: conjunto novo **Tenda de
  Feltro**.
- **Noite:** fogueiras do acampamento; Lobo da Estepe em bando; céu estrelado (visão boa).
- **No jogo (2026-11-03): cidade de verdade** (`CityTownRecipes.Soslan.cs`, estilo `SteppeCamp`, `CityCatalog.RaceCities`, id `acampamento_soslan`): em y = 0 dentro de um círculo de 22,5 m (saídas O, L e S a 19 m; ao norte os cercados de corda da manada em três lances): tendas redondas de feltro sobre carroças de quatro rodas, com faixa tingida, aro de ouro, coroa de fumaça e toldo de feltro; o forno coletivo de pedra na praça, estandartes de crina com cervos de ouro, carrinhos de feltro; portão de dois estandartes com flâmulas; Soslan, Batradz, Satana, Syrdon e Uryzmæg (centauros, `NpcRecipe.HorseBody`) com falas para cada primo; Selaria Nômade do Syrdon; missões "Lobos em Volta dos Potros" e "Águias sobre o Acampamento" (paga com a Lamparina Rúnica); respawn na praça para 47, 48 e 51. Falta: a cidade que anda, o Cavalo de Fumaça e o Cavalo Dourado da Estepe.
- **No jogo (2026-11-09): corrida de estepe** (`NartesRaceRules.cs`, `NartesMechanicsDriver`; poste em (440, −210) nas Pistas de Corrida): [E] no poste, 3 s de contagem, 6 portas douradas em ordem e volta ao poste (~1035 m, portas de 9 m); medalha por tempo (ouro ≤ 150 s, prata ≤ 185 s, bronze ≤ 225 s; a pé dá bronze, ouro pede montaria boa); limite 300 s, afastar 500 m da porta abandona, o herói nunca perde os controles. Prêmio: 60/35/20 pepitas, × 3 na primeira medalha do dia UTC. Sem "prêmios de sela" ainda (faltam itens de sela).

### 48 · Kurgans de Ouro / *Golden Kurgans* — nv 106–114 · ☆
- **Fonte:** kurgans de Pazyryk e Issyk (ouro e feltro citas); os grifos que guardam o ouro (Heródoto IV); os **Ouros do
  Céu** que só o caçula consegue pegar (Heródoto IV.5–7; bíblia §1.6).
- **Bioma e visual:** colinas-túmulo de terra e pedra na estepe; terra, pedra, ouro cita, feltro colorido; cervos de ouro
  estilizados (o motivo do "estilo animal").
- **Layout:** G. 9 kurgans; um aberto (posto); **posto: Kurgan Aberto**. Retorno: Acampamento de Soslan.
- **Inimigos:** Grifo Guardião [elite]; Cervo de Ouro Apagado [rápido]; Cisne de Feltro [distância] (as figuras de feltro
  de Pazyryk, invenção); Lobo de Bronze [tanque].
- **No jogo (2026-11-11): bichos próprios no lugar dos emprestados** (`MidStandInBeastRecipes`, `MidStandInBestiaryTexts`, `OverworldLayouts.MidBeasts`; modelo com rig, loot base e página no Caderno): Cisne de Feltro (`enemy_mountain_felt_swan`, [distância]) nos cisnes do Vau Largo, do Lago Salgado, do Kurgan Aberto e do Rio dos Nartes, no lugar da gaivota. Falta: Cervo de Ouro (chacal) e Grifo Guardião (grifo jovem).
- **Chefe:** **O Grifo Velho do Kurgan**, que guarda o ouro há tanto tempo que esqueceu de quem; libertado, deixa um
  presente para cada primo.
- **Mecânica:** **os Ouros do Céu** (segredo): quatro objetos de ouro (arado com canga, machado, taça) quentes demais
  para os maiores; **só a Lavine** consegue pegar ("eu não sou pequena").
- **Ganchos:** loot ouro cita, feltro · montaria `mount_griffin` (Despertar IV) · casa: conjunto novo **Ouro da Estepe**
  (cervo de ouro de enfeite).
- **Noite:** os kurgans brilham por dentro; os grifos voam; visão boa.
- **No jogo (2026-11-09): os Ouros do Céu** (`NartesSkyGoldRules.cs`, `OverworldLayouts.NartesSkyGolds`): arado, canga, machado e taça, quatro colunas de luz no chão dos kurgans (3 nos Kurgans de Ouro, 1 ao norte do Kurgan Aberto), alcance 4 m. Só a Lavine levanta ("Eu não sou pequena!", 45 pepitas por objeto, uma vez por dia UTC); os outros primos só sentem o calor, sem dano. Falta: modelos dos objetos e o Grifo Velho.

### 49 · Fortim dos Gigantes-da-Colina / *Hill-Giant Fort* — nv 112–120 · ☆ · cidade de raça (gigantes-da-colina)
- **Fonte:** *Livro dos Gigantes* maniqueu (§3.3).
- **Bioma e visual:** colinas verdes com muralhas de terra em anéis; paleta do Fortim (§3.3); casas redondas gigantes.
- **Layout:** G. **Fortim** (§3.3) no alto; colinas em volta com rebanhos gigantes; vale das tábuas de sonho (missões do
  Virogdad). Retorno: Fortim.
- **Inimigos:** Gigante Sonâmbulo (gigante que dorme apagado e anda; acordado, volta para casa) [tanque]; Cabra-Montês
  Gigante [rápido]; Javali da Colina [tanque]; Corvo-Mensageiro de Fumaça [distância].
- **Chefe:** **O Pesadelo da Tábua**, a sombra de um sonho ruim dos gigantes que fugiu da tábua; vencido, o sonho vira bom.
- **Mecânica:** **escala gigante** (§3.3); **sonhos**: o Virogdad lê um sonho por dia que aponta um baú escondido no mapa.
- **Ganchos:** loot lã gigante, panela de pedra · montaria nova **Carneiro Gigante da Colina** (§6) · casa: conjunto novo
  **Casa dos Gigantes** (colher-banco).
- **Noite:** os sonâmbulos dobram; lareiras gigantes iluminam as colinas.
- **No jogo (2026-11-03): cidade de verdade** (`CityTownRecipes.Fortim.cs`, estilo `HillFort`, `CityCatalog.RaceCities`, id `fortim`): em y = 0 dentro de um círculo de 22,5 m (só duas saídas, O e S a 19 m; a leste a encosta atrás de uma muralha de terra, ao norte os anéis do fortim em três lances com a torre de vigia): casas redondas gigantes de pedra seca com teto de grama, porta da altura de dois primos e fumaça boa; a panela do tamanho de uma casa sobre o Fogo Vivo na praça, mesas de pedra e colheres gigantes de banco; portão trilito; Baraqel, Virogdad e Shahmizad (gigantes de 1,45–1,55) com falas para cada primo; Panela Grande do Shahmizad com os Grampos de Escalada (exigência do 52); missões "Javalis na Despensa" e "Os Corvos do Sonho Ruim" (paga com os Grampos); respawn na praça para 49 e 52. Falta: a escala gigante andável, os sonhos diários com baús, e o Pesadelo da Tábua (o Carneiro Gigante da Colina já existe, à venda em Dur-Jarras, 2026-11-09).

### 50 · Arvoredo de Haripushpa / *Haripushpa Grove* — nv 118–126 · ☆ · cidade de raça (povo-tronco)
- **Fonte:** reis de Kucha (§3.3); a taiga da borda da estepe.
- **Bioma e visual:** taiga de lariços e pinheiros gigantes; paleta do Arvoredo (§3.3); musgo, rios de degelo.
- **Layout:** G. **Arvoredo** (§3.3) no círculo de pedras; trilhas de casca; lago de degelo. Retorno: Arvoredo.
- **Inimigos:** Besouro-Casca [tanque]; Pica-Pau de Fumaça [distância]; Fungo Apagado [controle, esporos lentos]; Lince
  da Taiga [rápido] (bicho real).
- **Chefe:** **A Serra de Āz**, engenho de serrar que Āz pôs para cortar árvores por ouro; quebrado, as peças viram
  ferramentas da cidade.
- **Mecânica:** **tempo lento** (§3.3).
- **Ganchos:** loot casca antiga, seiva · montaria nova **Alce da Taiga** (§6) · casa: **Viveiro** do Arvoredo (mudas).
- **Noite:** as árvores andantes "dormem" (viram trilha); corujas; visão curta.
- **No jogo (2026-11-03): cidade de verdade** (`CityTownRecipes.Arvoredo.cs`, estilo `LivingGrove`, `CityCatalog.RaceCities`, id `arvoredo`): em y = 0 dentro de um círculo de 22,5 m (saídas O, L e S a 19 m; ao norte a taiga fecha em lariços): casas dentro de troncos ocos gigantes com raízes, porta em arco, janelas de ninho e copa de lariço; o círculo de pedras com musgo na praça, lariços (um velho dormindo, com rosto na casca), riachos de degelo com pontes de tronco; portão de raízes em arco; Haripushpa e Suvarnapushpa (povo-tronco, `NpcRecipe.Twigs`) e o herbolário sogdiano Aryaman Puhr com falas para cada primo; Viveiro da Suvarnapushpa; missões "Os Comedores de Casca" e "Os Pássaros de Fumaça" (paga com Runas de Cura); respawn no círculo de pedras para o 50. Falta: o tempo lento, as mudas raras da casa, e a Serra de Āz (o Alce da Taiga já existe, à venda em Dur-Jarras, 2026-11-09).

### 51 · Pradaria dos Mil Cavalos / *Thousand-Horse Prairie* — nv 124–134 · ☆
- **Fonte:** a Uasamonga, a taça dos Nartes que se enche sozinha para o mais corajoso (bíblia §3.8); os grandes rebanhos
  de cavalos da estepe.
- **Bioma e visual:** pradaria ondulada com flores; capim, papoula `#D9453A`, céu; rebanhos selvagens, poço de pedra.
- **Layout:** GG. Planície aberta; **posto: Poço da Taça**. Retorno: Acampamento de Soslan.
- **Inimigos:** Garanhão Apagado [rápido]; Lobo-Cinzento [enxame]; Tempestade de Capim [controle] (Vento Destruidor);
  Abutre da Estepe [distância].
- **Chefe:** **O Garanhão Cinzento**, o chefe do rebanho que a sombra assustou; libertado, o rebanho segue os primos.
- **Mecânica:** **pastorear**: levar cavalos soltos até o acampamento de Soslan dá pontos e, raramente, uma montaria;
  **banquete da taça** (evento): a Uasamonga se enche para quem fez a melhor ação do dia no servidor.
- **Ganchos:** loot crina, papoula (tintura) · montaria `mount_pinto_horse` (Despertar) · casa: conjunto novo **Banquete
  dos Nartes** (taça de enfeite).
- **Noite:** os rebanhos dormem em roda; os lobos rondam.

### 52 · Corrente de Amirani / *Amirani's Chain* — nv 134–150 · ☆
- **Fonte:** Amirani, o herói da épica georgiana acorrentado a uma montanha do Cáucaso (o conto, sem o herói em cena); o
  *ochokochi*, homem da mata do folclore georgiano; Ohya, gigante do *Livro dos Gigantes*.
- **Bioma e visual:** pico do Cáucaso com uma corrente gigante cravada na rocha; rocha `#5E6470`, neve, ferro antigo
  `#4A4F55`; torres de pedra svan (torres de vigia de vila).
- **Layout:** GG (vertical). A corrente sobe do vale até o pico (os elos são o caminho); **posto: Torre de Vigia** (uma
  torre svan). Retorno: Fortim.
- **Inimigos:** Ochokochi [elite]; Águia de Ferro [distância]; Urso do Cáucaso Apagado [tanque] (bicho real); Lobo de Neve
  [rápido].
- **Chefe:** **chefe do mundo: Ohya, o Gigante Irmão**: o irmão do Hahya, que ainda não voltou a si; se prende à corrente
  "para não brigar com ninguém" e briga mesmo assim. Online: chefe do mundo semanal. Vencido, solta a corrente e desce
  para morar com o Hahya.
- **Mecânica:** **subir pela corrente**: andar nos elos (estreitos, com vento); cair leva ao elo de baixo (não ao vale).
- **Ganchos:** loot ferro antigo, elo de corrente · montaria `mount_woolly_mammoth` (Despertar II) · casa: troféu de Ohya
  (um elo).
- **Noite:** a corrente brilha onde a luz já subiu; neve forte.
- **No jogo (2026-11-09): subir pela corrente** (`NartesChainRules.cs`, ao longo de `AmiraniChain`, ~800 m): faixa de 6 m sobre os elos, argolas a cada 40 m (abrigo de 5 m, acendem conforme o herói sobe). Rajada a cada 16 s (10,5 calmo, 3 de aviso com texto e traço laranja, 2,5 soprando), de lados alternados, empurra 3,5 m/s de través: correndo contra dá para segurar. Quem é jogado para fora cai na argola de baixo (nunca mais longe) e perde 4 % da vida, nunca abaixo de 1. Sair andando fora da rajada não custa nada; montado não sobe (aviso). O alto paga 150 pepitas uma vez por dia UTC. Falta: Ohya e o loot de ferro antigo; o brilho noturno.
- **No jogo (2026-11-11): chefe do mundo Ohya** (`enemy_mountain_ohya`, nv 150): arena no anel de pedras no Cume de Ohya, onde a trilha da corrente encontra a do Poço da Taça (1000, 1680 na região, a ~45 m dos elos); aparece com o herói a 90 m, some a 125 m (de volta à vida cheia) e volta 5 min depois de cair; acima de 66% golpe e anel de força, 66–33% rajada de 3 tiros a cada 8 s, abaixo de 33% rajada de 5 e o Grande Golpe marcado no chão (2 s, 4,5 m); as clareiras comuns ficam a 22 m ou mais do anel, chão plano e trilha até ele; peso 0 no bioma (nunca sai em encontro). Própria: **a corrente gira** a partir de 66% (a cada 10 s, 2 s de aviso): pega quem está entre 4 e 10 m dele; colado nas pernas ou longe, nada. Vencido, cai como os outros. Loot próprio (2026-11-11): **Elo de Ferro Antigo de Ohya** (`item_chefe_ferro_antigo`, épico, 50% por vitória, 280 de venda; no lugar do punhado de ouro, o loot do bioma segue igual); vai no Tear/Forja da casa (`HouseCraftRules`): Grevas de laje do Joaquim (Forja, 2), Grevas do tronco milenar e Pernas mecânicas assistidas (Tear de mestre). Troféu de casa (2026-11-11): **Corrente partida de Ohya** (`casa_trofeu_029`, épico, parede), feito na Forja da casa com 2 Elo de Ferro Antigo e o chefe já vencido (`HouseCraftRules.WorldBossTrophies`). Falta: soltar a corrente e descer para morar com o Hahya (cena).

### Região 10 — Picos de Farnkesh (Vento Vivo) · mapas 53–58 · ☆

Capital **Farnkesh** (§2): *farn* (sogdiano; *xwarrah* em persa), o nimbo de glória e boa sorte; *Kesh*, nome antigo de
Shahrisabz (Sogdiana). Pináculos de pedra acima das nuvens. NPCs com nomes maniqueus: **Shad-Ohrmizd** (fundador de um
ramo do oriente; aqui, guardião dos sinos de vento), **Sisin** (Sisinnios, sucessor de Mani; aqui, mestre das pontes de
corda). Chefe do mundo: **O Rei-Vendaval** (mapa 58).

**No jogo (2026-11-06): mundo contínuo** (`OverworldLayouts.Farnkesh.cs`, §0.10): 2400 × 2400 m ao norte da Estepe dos Nartes,
25 áreas em 14 terrenos novos — Vinhas de Gordias e do Sol, Vau e Rio Pactolo (seixos dourados), Clareiras do Baile e o Vale dos
Sátiros (53, nv 140), Mata das Castanheiras e Bosque do Pé dos Picos, Prados do Vento e dos Sinos, Subida de Farnkesh, Escadaria
dos Cestos e Mar de Nuvens (54), Farnkesh (54, ninhos de pedra), Bosque de Pináculos e Pináculos do Leste, Pontes de Vento e
Ermida Baixa (55, posto), Ermidas Suspensas e Pináculo do Sino (56, posto), Escarpa dos Ventos, Garganta dos Ays e Gruta Abrigo
(57, posto; **Proteção contra tempestade**), Ilhas de Nuvem com o Último Pináculo e Ninho da Huma (58, posto; **asas ou planar**,
até nv 200). Relevo: pináculos de arenito (até ~34 m), paredão das ermidas em degraus, garganta de paredes altas, ilhas de topo
plano sobre o chão de nuvem; vale e mar de nuvens baixos. Cenário: pináculos, ermidas brancas, sinos de vento, pontes de corda,
varais de papel de vento, cestos de corda, vinhas, cabanas-cabaça, palcos de pedra, pedras-abrigo, árvores tortas, o ninho de
ouro; nuvens no chão e seixos dourados. Bichos existentes nos papéis das fichas (iaque = bode/takin, enxame de vaga-lumes =
abelhas douradas, gavião, chacal, corvo = gralha, abutre, morcego-vampiro = morcego-vela, lanterna quebrada = pipa/sino/raio,
névoa do vento ruim = eco/ays/ventania, golem = ushabti de corda, irbis = gato-de-pallas, pedra rolante, urso-pardo, aranha,
marmota, macaco ladrão), variante noturna em toda clareira, nenhum chefe na época (desde 2026-11-09 o chefe do mundo O Rei-Vendaval está no 58). Cidades com as próprias receitas (`Glade`,
`PinnacleNest`) no chão plano de ±30 m. Testes `OverworldFarnkeshTests` e `OverworldWorldTests`. Mecânicas no ar (2026-11-09, `Map/Mechanics/`): rajadas, cestos e correntes; ver as fichas 54, 55, 57 e 58. Falta:
medidor de silêncio, vento contra, os chefes de mapa das fichas (o do mundo já existe).

### 53 · Vale dos Sátiros / *Satyr Vale* — nv 140–148 · ☆ · cidade de raça (sátiros)
- **Fonte:** mitos frígios (§3.3).
- **Bioma e visual:** vale de vinhas e bosques ao pé dos picos; paleta do Vale (§3.3); rio de seixos dourados (o Pactolo).
- **Layout:** G. **Vale dos Sátiros** (§3.3) em clareiras em anel; rio no meio; subida para Farnkesh ao norte. Retorno:
  Vale dos Sátiros.
- **Inimigos:** Bode de Fumaça [tanque]; Abelha Dourada (abelhas que o toque de Midas fez de ouro e que brigam) [enxame];
  Pássaro-Flauta Apagado [distância]; Raposa da Vinha [rápido] (bicho real).
- **Chefe:** **O Ouro de Midas**, uma estátua de ouro que andou sozinha; para em pé quando o Midas lava as mãos no rio.
- **Mecânica:** **dourado**: golpes das abelhas douram o herói (lento); lavar no rio (Água Viva) tira; **Baile do Vale**
  (§3.3).
- **Ganchos:** loot Pó de Ouro do Pactolo, uva · montaria nova **Bode Dourado de Gordias** (§6) · casa: conjunto novo
  **Flauta Dupla**.
- **No jogo (2026-11-09): montaria.** **Bode Dourado de Gordias** (`mount_gordias_goat`, Raro): recompensa de `quest_city_vale_vine_foxes` (Gordias). **Nó de Gordias** desfaz o segredo mais perto a até 14 m.
- **Noite:** o baile; vaga-lumes; os bodes de fumaça dormem.
- **No jogo (2026-11-06): cidade de verdade** (`CityTownRecipes.ValeDosSatiros.cs`, estilo `Glade`, `CityCatalog.RaceCities`, id `vale_dos_satiros`): em y = 0 dentro de um círculo de 22,5 m (saídas O, N e L a 19 m; a rua Norte é a subida para Farnkesh; ao sul o rio e o bosque): casas de vime em forma de cabaça com túnel de folhagem na entrada; o palco de pedra do baile na praça e um menor numa clareira ao sul; o rio de seixos dourados (o Pactolo, Água Viva) atravessando o vale e um braço cruzando a rua Norte, com vau de pedras; fileiras de videiras e carvalhos; Midas, Marsyas e Gordias (sátiros: pernas de bode, chifres pequenos, `NpcRecipe.Satyr`) com falas para cada primo; Flauta Dupla do Marsyas (runas de vento, Ar Sereno); missões "As Abelhas de Ouro" e "Raposas na Vinha"; respawn na praça para 53. Falta: o dourado/lavar no rio, o Baile do Vale (ritmo), e O Ouro de Midas (o Bode Dourado de Gordias já existe, linha acima).

### 54 · Farnkesh, os Ninhos de Pedra / *Farnkesh, the Stone Nests* — nv 145–152 · ☆ · capital (Vento Vivo)
- **Fonte:** dzongs do Butão (Paro Taktsang) e Meteora (Grécia) como **forma** (casas no alto dos rochedos, redes e cestos
  de corda para subir); nada de culto: são casas de quem gosta de vento e silêncio.
- **Bioma e visual:** 9 pináculos de arenito acima das nuvens; estilo novo `PinnacleNest` (casas brancas de parede
  inclinada, janelas de madeira pintada, telhados de tábua com pedra em cima, varandas para o vazio); paleta: cal
  `#F1ECE2`, madeira vermelha `#9A3B2E`, ouro `#D9A82B`, arenito `#B58A60`, nuvem `#EEF3F7`, céu `#8FB8E0`.
- **Layout:** GG. Pináculos ligados por pontes de corda e **cestos de corda** (elevador manual: o Sisin puxa); praça no
  pináculo maior com o sino de vento gigante. Retorno: Farnkesh.
- **Inimigos (pontes de fora):** Gralha-Alpina [rápido] (bicho real); Takin Apagado [tanque] (bicho real); Abutre do Pico
  [distância]; Cabra-Azul [rápido] (bicho real).
- **Chefe:** nenhum na capital; evento **Vendaval** (o vento vira e as pontes balançam: segurar as cordas).
- **Mecânica:** **cestos e pontes**: subir é devagar, descer planando é rápido (voadores e Senmurv). G4 da Maithe (Sopro
  de Farnkesh) é aqui.
- **Ganchos:** loot corda de iaque, papel de vento · montaria `mount_hippogriff` (Senmurv, Despertar) · casa: conjunto novo
  **Ninho de Pedra** (sino de vento, banco de varanda).
- **Noite:** os pináculos acendem como estrelas; vento calmo; visão ótima.
- **No jogo (2026-11-06): cidade de verdade** (`CityTownRecipes.Farnkesh.cs`, estilo `PinnacleNest`, `CityCatalog.LateRegionCities`, id `farnkesh`): em y = 0 dentro de um círculo de 22,5 m (saídas O, L e S a 19 m; a rua Sul é a estrada que sobe do Vale; ao norte os pináculos nas nuvens): casas brancas de parede inclinada sobre pés de arenito, faixa e janelas de madeira vermelha, telhados de tábua com pedras e varandas para o vazio; o sino de vento gigante na praça; pináculos de arenito (alguns com ninho no alto), cestos de corda e vãos de nuvem com pontes de tábua e corda nas ruas Leste e Sul; portão de postes com sininhos; Shad-Ohrmizd, Sisin, Tadi e Ardawan com falas para cada primo (a Maithe escuta o Vento Vivo); Cordas do Sisin com as exigências de 57 (Capa de Tempestade) e 58 (Asas de Lona) fixas; missões "Abutres nos Sinos" e "Vento Ruim nas Pontes" (paga com a Capa de Tempestade); respawn na praça para 54–58. **Cestos de corda (2026-11-09, `SkyRouteRules`, `OverworldLayouts.SkyRoutes`):** 4 linhas com posto de cesto (Escadaria–Farnkesh, Sino, Ermida Baixa, Sino–Gruta; `[E]` no patamar, raio 4 m): sobe a 9 m/s, desce a 9 m/s ou 18 m/s com asas/montaria voadora, arco de 10 m, a barreira suave vale para toda a linha. Falta: pináculos em alturas de verdade, o Vendaval e o G4 da Maithe.

### 55 · Pontes de Vento / *Wind Bridges* — nv 150–160 · ☆
- **Fonte:** invenção (as pontes de corda do Sisin).
- **Bioma e visual:** um labirinto de pontes entre pináculos menores; corda, tábua, nuvem.
- **Layout:** G. 20 pontes e 12 plataformas; **posto: Ermida Baixa**. Retorno: Farnkesh.
- **Inimigos:** Morcego-Vela [rápido]; Pipa-Sombra [distância]; Neblina do Vento Ruim [controle]; Ushabti de Corda
  [tanque].
- **Chefe:** **O Cortador de Cordas**, ave de sombra que corta pontes para ver gente cair; libertada, vira guarda das
  pontes.
- **Mecânica:** **rajadas**: a cada 20 s uma rajada atravessa (aviso no céu); quem está numa ponte para e segura.
- **Ganchos:** loot pena de vento · montaria nova **Filhote de Huma** (desafio) · casa: **Ninho de Pedra**.
- **Noite:** pontes com lanternas; o Morcego-Vela dobra.
- **No jogo (2026-11-09): rajadas** (`WindGustRules`, `FarnkeshMechanicsDriver`): ciclo de 20 s (15,5 calmo + 3 de aviso com riscos no céu e "pare e segure" + 1,5 soprando), a 4,5 m de cada linha de `WindBridgeLines`, de través e cada uma do lado oposto da anterior; quem anda é empurrado 5 m/s (7,5 m por rajada; 3,75 m com a Capa de Tempestade), quem está parado segura e não sai do lugar; nunca em zona segura. Falta: pontes sem chão por baixo (a rajada só desloca, ninguém cai).
- **No jogo (2026-11-11): chefe do mapa O Cortador de Cordas** (`RegionBossRules.MapSpawns`, `enemy_mountain_rope_cutter`, Nv 160): anel de pedra na estrada de Farnkesh às pontes (`CortadorDeCordasArena`), o mesmo diretor e as mesmas fases dos chefes do mundo; habilidade própria **corta a corda** (`RopeCutHits`): aviso de 2 s e, nos últimos 0,6 s, quem anda mais de 0,5 m leva o golpe, quem para e segura não. Loot: Runa do Vento (60 %, 1–2) e peça de roupa; libertado, vira guarda das pontes. Falta: a queda de verdade (a ponte não some).

### 56 · Ermidas Suspensas / *Hanging Hermitages* — nv 158–168 · ☆
- **Fonte:** invenção sobre a forma de Meteora (casas grudadas no paredão).
- **Bioma e visual:** paredão vertical com casas penduradas; cal, madeira, pedra; escadas de mão, sinos.
- **Layout:** G (vertical). Escalada por escadas e cestos; **posto: Pináculo do Sino**. Retorno: Farnkesh.
- **Inimigos:** Eco Apagado [controle, repete o último golpe do herói contra ele]; Gato-de-Pallas Apagado [rápido] (bicho
  real); Sino Rachado [distância, onda de som]; Gralha [enxame].
- **Chefe:** **O Sino que Não Para**, sino gigante que a sombra fez tocar sem fim; parado, o paredão fica em silêncio.
- **Mecânica:** **silêncio**: correr e lutar enchem um medidor de barulho; cheio, os Ecos aparecem. Andar devagar e
  usar skills silenciosas (Lavine) ajuda.
- **Ganchos:** loot bronze de sino · montaria `mount_unicorn` (Shadhavar, Despertar: o chifre toca) · casa: conjunto novo
  **Sino de Bronze**.
- **Noite:** o medidor enche mais devagar (todos dormem); os ecos ficam visíveis.
- **No jogo (2026-11-11): chefe do mapa O Sino que Não Para** (`enemy_mountain_endless_bell`, Nv 168): anel de pedra na estrada oeste logo abaixo do Pináculo do Sino (`SinoQueNaoParaArena`); habilidade própria **badalada com eco** (`TollHits`): bate onde o herói estava e a nota anterior toca de novo onde caiu, então não dá para voltar ao lugar de antes. Loot: Pedra da Alma Viva (50 %); parado, o paredão fica em silêncio. Falta: o medidor de silêncio e os Ecos.

### 57 · Garganta dos Ays / *Gorge of the Ays* — nv 166–180 · ☆
- **Fonte:** os *ays*, espíritos do vento ruim do folclore armênio.
- **Bioma e visual:** garganta estreita onde o vento uiva; rocha cinza, poeira `#C9B79A`, céu estreito; árvores tortas.
- **Layout:** G (comprido). Garganta com abrigos de pedra; **posto: Gruta Abrigo**. Retorno: Farnkesh.
- **Inimigos:** Ays [rápido, invisível dentro do vento]; Redemoinho de Pó [controle]; Águia da Tempestade [distância];
  Pedra Voadora [tanque].
- **Chefe:** **O Rei dos Ays**, o redemoinho maior; vencido, vira brisa.
- **Mecânica:** **vento contra**: andar contra o vento é ×0,7; atrás das pedras-abrigo, ×1; o vento revela os Ays (poeira
  no formato deles).
- **Ganchos:** loot Pó do Vento (tintura) · montaria `mount_sleipnir` (Despertar) · casa: troféu do Rei dos Ays.
- **Noite:** o vento para pela metade; os Ays ficam visíveis como silhuetas.
- **No jogo (2026-11-09):** as rajadas da ficha 55 também sopram em toda a área da Garganta (eixo fixo 0,8/0,6, alternando o sentido); a Capa de Tempestade, que a área já exige, reduz o empurrão à metade. Falta: o vento contra ×0,7, as pedras-abrigo e o Rei dos Ays.

### 58 · Ninho da Huma / *Huma's Nest* — nv 180–200 · ☆
- **Fonte:** a huma, ave persa que nunca pousa e cuja sombra dá sorte (já é o nome das asas da Maithe); o Vento
  Destruidor (bíblia §1.3).
- **Bioma e visual:** ilhas de pedra soltas nas nuvens; nuvem, ouro, branco; um ninho gigante de galhos dourados.
- **Layout:** GG. 12 ilhas flutuantes em volta do ninho; **posto: Último Pináculo**. Retorno: Farnkesh.
- **Inimigos:** Ventania [controle]; Harpia de Fumaça [rápido] (a harpia dos mitos gregos, como bicho); Grifo Selvagem
  [elite]; Raio Preso [distância].
- **Chefe:** **chefe do mundo: o Rei-Vendaval**, o Vento Destruidor em forma de ave sem rosto, que quer o ninho para ele;
  a Huma ajuda na luta. Vencido, recua para as Grutas (como a Treva).
- **Mecânica:** **planar**: entre ilhas só se passa planando (asas, voadores); as correntes mudam por hora.
- **Ganchos:** loot pena de huma · montaria nova **Filhote de Huma** (lendária) · casa: conjunto novo **Ninho de Ouro**.
- **Noite:** as ilhas brilham; o Rei-Vendaval fica mais forte; Barca da Lua passa perto (vista bonita).
- **No jogo (2026-11-09): correntes de planar** (`SkyRouteRules`, `OverworldLayouts.SkyRoutes`): 4 correntes ligam a trilha às ilhas de cima e as ilhas entre si (`[E]`, sino de vento marca o patamar), a 14 m/s com arco de 5 m; pedem asas, Asas de Lona ou montaria voadora; cada corrente descansa 1 hora de jogo em 3 (hora mod 3 = grupo), só na ida: a volta nunca fecha, para ninguém ficar preso numa ilha. Falta: o chão de nuvem ainda se anda (a ilha é atalho, não a única passagem) e a ajuda da Huma na luta. **Chefe do mundo: o Rei-Vendaval** (`RegionBossDirector`, `RegionBossRules`, `enemy_mountain_wind_king`, nv 200): arena no anel de pedras do Ninho (880, 1500 na região); aparece com o herói a 90 m, some a 150 m e volta 5 min depois de cair; acima de 66% golpe e anel de força, 66–33% rajada de 3 penas a cada 8 s, abaixo de 33% rajada de 5 e o Grande Golpe marcado no chão (2 s, 4,5 m); habilidades +25% à noite; as clareiras comuns ficam a 22 m ou mais do anel. Loot próprio (2026-11-11): **Pena do Rei-Vendaval** (`item_chefe_pena_vendaval`, épica, 45% por vitória, 320 de venda; no lugar do punhado de ouro, o loot do bioma segue igual); vai no Tear/Forja da casa (`HouseCraftRules`): Perneiras de vento trançado da Maithe (Tear, 2) e Saia-calça de relâmpago (Tear de mestre). Troféu de casa (2026-11-11): **Leque de penas do Rei-Vendaval** (`casa_trofeu_030`, épico, parede), feito na Forja da casa com 2 Pena do Rei-Vendaval e o chefe já vencido (`HouseCraftRules.WorldBossTrophies`).

### Região 11 — Mar Gelado (Ar Sereno) · mapas 59–64 · ☆

Capital **Porto da Lua Branca** (§2). A Barca da Lua passa aqui e "atraca" na lua cheia. Culturas de inspiração: casas do
Ártico (só forma), costa noroeste do Pacífico (casas de tábua, sem os postes de brasão), folclore komi (Yen e Omöl, o
mamute debaixo da terra, os *kul* da água). NPCs: **Yirkap**, o caçador de esqui (herói dos contos komi), e **Pera**, o
gigante bom (bogatir komi). Respeito: povos do Ártico são vivos; nenhuma figura da fé deles aparece; inimigos são bichos
e a sombra. Chefe do mundo: **o Velho Kul** (mapa 64).

**No jogo (2026-11-06): mundo contínuo** (`OverworldLayouts.MarGelado.cs`, §0.10): 2400 × 2400 m ao norte de Mingcheng, a oeste
dos Picos de Farnkesh, 25 áreas em 16 terrenos novos — Porto da Lua Branca (59, nv 180), Baía Gelada e Baía das Focas, Passo da
Muralha (entrada de Mingcheng) e Colinas de Neve, Pastos de Rena; Canais de Gelo com a Casa de Neve dos Barqueiros, Canais do Oeste
e Gelo Rachado (60, posto); Tundra do Sul, Tundra de Omöl com a Tenda de Yen e Lagos de Omöl (61, posto); Terra Congelada e Galerias
dos Mamutes com a Boca do Gelo (entradas dos picos) e Fiorde das Presas (62, posto); Banquisas, Banquisas do Oeste, Mar das Auroras,
Ilha do Farol com o Farol de Gelo e Fiorde Negro (63, posto; **Resistência à nevasca**); Buracos de Pesca, Fundo do Velho Kul com o
Buraco Grande de Pesca, Lago do Kul, Gelo Negro e Fiorde do Kul (64, posto; **Roupa térmica refinada**, até nv 240). Relevo: placas
de gelo com canais entre elas (`Floes`), icebergs de topo plano sobre o mar gelado (`Icebergs`), cristas de pressão (`PressureRidges`),
fiordes de paredes em degraus (`FjordWalls`, até ~34 m), cúpulas de gelo e terra congelada das galerias (`IceVaults`); porto, baía,
tundra e lago baixos. Água: três canais de água negra saindo do porto (dois com ponte), 7 lagos na tundra, 5 aberturas na banquisa,
7 buracos de pesca e o Buraco Grande. Cenário: cúpulas de neve, casas de tábua com lua pintada (só forma), barcos em meia-lua, arcos
de osso de baleia, blocos de gelo, icebergs, tendas de pastores de rena, pedras com líquen, presas de mamute, lanternas de gelo,
varais de peixe, faróis de gelo, cristais; buracos de pesca, rachaduras e tufos de líquen no chão. Bichos existentes nos papéis das
fichas (lontra = foca-leopardo/narval, búfalo de lama = morsa, gaivota ladra, raposa-lanterna = raposa-do-ártico, peixe-lanterna =
peixe-gelo/lúcio, urso-pardo = urso-polar, cobra da areia = enguia-lanterna, gosma = rachadura/raiz-gelo/peixe-bruxa, urutau =
ave-graveto de Omöl, pedra rolante = lebre-pedra, iaque = rena de barro, enxame de vaga-lumes = mosquito/kul pequeno, rinoceronte de
poeira = mamute, marmota = toupeira-gelo, lanterna quebrada = cristal vivo, lobo = lobo-do-mar, névoa = gelo flutuante, gavião =
gaivota-tempestade, tartaruga de pedra = foca-sombra, irbis nos fiordes), variante noturna em toda clareira, nenhum chefe (o Velho
Kul fica para depois). Testes `OverworldMarGeladoTests` e `OverworldWorldTests`. Falta: estação na tundra, túneis que mudam,
placas que se movem. *(Corrigido em 2026-11-11: fases da lua nos canais, gelo fino e pesca no gelo já estão no jogo, fichas 59,
60 e 64; o chefe do mundo O Velho Kul está no 64.)*

### 59 · Porto da Lua Branca / *White Moon Harbor* — nv 180–188 · ☆ · capital (Ar Sereno)
- **Fonte:** arquitetura de neve e de turfa do Ártico, casas de tábua da costa do Pacífico Norte, cidades de canais; a
  Barca da Lua (Kephalaia) e a Lavine "Barqueira da Lua" (G3).
- **Bioma e visual:** baía gelada com canais; estilo novo `IceCanal` (cúpulas de neve, casas de tábua compridas com
  fachada pintada de luas, pontes de osso de baleia em pedra, barcos em meia-lua); paleta: gelo `#D8E9F2`, neve
  `#F7FAFC`, tábua `#5B4A3E`, lilás `#B7A6E0`, prata `#C8D2DC`, noite `#1E2A4A`.
- **Layout:** GG. Cidade em ilhas de gelo ligadas por pontes e canais; o **Cais da Lua** (onde a Barca encosta) no meio;
  bairro de pesca, de peles e de lanternas. Retorno: Porto da Lua Branca.
- **Inimigos (arredores):** Foca-Leopardo Apagada [rápido] (bicho real); Morsa de Fumaça [tanque]; Gaivota-do-Gelo
  [distância]; Raposa-do-Ártico Ladra [rápido] (bicho real).
- **Chefe:** nenhum na capital; evento **Noite Polar** (dias sem sol: defender as lanternas da cidade).
- **Mecânica:** **fases da lua**: na lua cheia os canais congelam (anda-se a pé), na lua nova abrem (só barco); as rotas e
  as lojas mudam. G4 da Lavine (Lua Branca) é aqui.
- **Ganchos:** loot pele de foca, óleo de lanterna, prata · montaria nova **Rena da Aurora** (§6) · casa: conjunto novo
  **Casa da Lua Branca** (lanterna em meia-lua, tapete de pele).
- **No jogo (2026-11-09): montaria.** **Rena da Aurora** (`mount_aurora_reindeer`, Épica): recompensa de `quest_city_porto_bears_on_the_canals` (Pera). **Trote da Aurora**: 10 s de galope sem cansar e sem lentidão; não pesa no gelo fino.
- **No jogo (2026-11-09): conjunto de casa **Móveis da Casa da Lua Branca**** (móveis, `Docs/Housing/CatalogoCasa.md`): 8 peças à venda em Peles e Barcos do Pamour, 28–420 de ouro; Épica só como especial da semana. Lanterna de meia-lua de prata, Tapete de pele de foca branca, Cama de peles do Cais da Lua.
- **Noite:** aurora no céu; a Barca da Lua passa; visão boa nos canais iluminados.
- **No jogo (2026-11-06): cidade de verdade** (`CityTownRecipes.PortoDaLuaBranca.cs`, estilo `IceCanal`, `CityCatalog.LateRegionCities`, id `porto_da_lua_branca`): em y = 0 dentro de um círculo de 22,5 m (saídas O, L e S a 19 m; a rua Sul é a entrada; ao norte a baía congelada): casas compridas de tábua escura com telhado de neve e a empena pintada com uma lua cheia de prata num anel lilás (sem postes de brasão) e cúpulas de neve com túnel de entrada; o Cais da Lua na praça (crescente de pedra prateada e a lanterna de lua); canais de água negra com pontes de pedra em costela de baleia nas ruas Oeste e Sul; barcos em meia-lua, varais de peixe, cristas de gelo e cúpulas pequenas; portão de costelas de baleia; Yirkap, Pamour, Pera e Makarios com falas para cada primo (o Makarios chama a Lavine de Barqueira da Lua); Peles e Barcos do Pamour com as exigências de 63 (Chá de Gengibre) e 64 (Roupa Térmica) fixas, mais barcos, lamparinas, grampos e capa; missões "Gaivotas nos Varais" e "Ursos nos Canais" (paga com a Roupa Térmica); respawn na praça para 59–64. **Fases da lua (2026-11-09):** ciclo de 8 dias do jogo (dia 1 = nova, dia 5 = cheia); os três canais congelam 3 dias em 8 (dias 4–6) e 5 travessias marcadas com lanterna de gelo levam de uma margem à outra: a pé no gelo (6 m/s), de barco nos outros dias (9 m/s; Barco de Junco, passagem ou montaria aquática); sem barco o aviso conta os dias até congelar; as estradas e pontes de osso seguem abertas, então nenhuma margem prende (`MoonCanalRules`, `MarGeladoMechanicsDriver`). Falta: as lojas mudarem com a lua, a Noite Polar, a Barca da Lua atracando e o G4 da Lavine (a Rena da Aurora já existe, linha acima).

### 60 · Canais de Gelo / *Ice Canals* — nv 186–196 · ☆
- **Fonte:** invenção (os canais que saem do porto para o mar aberto).
- **Bioma e visual:** labirinto de canais entre blocos de gelo; gelo azul, água negra `#1F3A4A`.
- **Layout:** G. Canais com trechos finos de gelo; **posto: Casa de Neve dos Barqueiros**. Retorno: Porto da Lua Branca.
- **Inimigos:** Peixe-Gelo [enxame]; Urso-Polar Apagado [tanque] (bicho real); Enguia-Lanterna [distância]; Rachadura
  [controle].
- **Chefe:** **A Morsa-Rainha**, que quer todos os peixes do canal; libertada, divide.
- **Mecânica:** **gelo fino**: montaria pesada (mamute, auroque) quebra o gelo fino; o mapa mostra a espessura.
- **Ganchos:** loot gelo eterno, marfim de pedra · montaria `mount_woolly_mammoth` (cuidado com o gelo) · casa: **Casa da
  Lua Branca**.
- **Noite:** o gelo racha mais; enguias acendem.
- **No jogo (2026-11-09): gelo fino** (`ThinIceRules`, `OverworldLayouts.ThinIcePatches`): 5 placas (2 sobre a estrada norte e a estrada oeste, 3 em Canais do Oeste e Gelo Rachado), contorno azul a 40 m e vermelho quando range. Só montaria pesada (mamute, tartaruga anciã, boi-bumbá) carrega o gelo: range com 1,2 s, quebra com 4 s (2,5 s de noite); ao quebrar a montaria foge e a água fria tira 8 % da vida, sem nunca matar; a pé o gelo aguenta. Falta: a espessura no mapa da UI.
- **No jogo (2026-11-11): chefe do mapa A Morsa-Rainha** (`enemy_mountain_walrus_queen`, Nv 196, mais forte à noite): anel de pedra na estrada norte depois da ponte de osso de baleia (`MorsaRainhaArena`); habilidade própria **tombo** (`FlopDamageFactor`): cai onde o herói estava, ×1,6 no meio e só ×0,3 na beirada de 7 m. Loot: Pedra da Água Viva (50 %); libertada, divide os peixes.

### 61 · Tundra de Omöl / *Omöl's Tundra* — nv 194–204 · ☆
- **Fonte:** Yen e Omöl, os irmãos criadores do mito komi: Yen fez as coisas boas, Omöl as desajeitadas (a Ave-Graveto
  da noite de hoje é "de Omöl").
- **Bioma e visual:** tundra plana com lagos; musgo `#8E9A6A`, liquen `#C8C29A`, neve, lagos `#6E8FA6`; pedras redondas.
- **Layout:** GG. Planície com 20 lagos; **posto: Tenda de Yen** (uma tenda de pastores de rena). Retorno: Porto da Lua
  Branca.
- **Inimigos:** Criaturas Desajeitadas de Omöl (Ave-Graveto adulta [distância], Lebre-Pedra [rápido], Rena de Barro
  [tanque]); Mosquito da Tundra [enxame].
- **Chefe:** **O Primeiro Desajeitado**, a primeira criatura de Omöl, triste por ser torta; libertada, descobre que é
  bonita do jeito dela (cena da Lavine).
- **Mecânica:** **estação**: no verão a tundra vira brejo (lento), no inverno neve (rápido); `SeasonCalendar`.
- **Ganchos:** loot musgo-de-rena, chifre · montaria nova **Rena da Aurora** · casa: conjunto novo **Tenda da Tundra**.
- **Noite:** aurora longa; as criaturas de Omöl dormem em pé (passar sem acordar).
- **No jogo (2026-11-09):** sem mecânica própria ainda (falta a estação: brejo no verão, neve no inverno); as criaturas de Omöl já rareiam à noite.

### 62 · Galerias dos Mamutes / *Mammoth Galleries* — nv 202–214 · ☆
- **Fonte:** no folclore komi e mansi, o mamute é um bicho que vive debaixo da terra e cava os vales (bíblia §3.17).
- **Bioma e visual:** túneis no gelo e na terra congelada; gelo, terra `#5A4A3A`, cristal; presas enormes nas paredes.
- **Layout:** G (subterrâneo). Túneis que o mamute cava (mudam por semana); **posto: Boca do Gelo**. Retorno: Porto da Lua
  Branca.
- **Inimigos:** Mamute de Baixo da Terra [elite]; Toupeira-Gelo [enxame]; Cristal Vivo [distância]; Raiz-Gelo [controle].
- **Chefe:** **O Mamute Avô**, que esqueceu o caminho para cima; libertado, abre um vale novo.
- **Mecânica:** **túneis que mudam**: o mapa se refaz a cada semana do jogo (semente fixa por semana; online igual para
  todos).
- **Ganchos:** loot marfim de pedra, gelo eterno · montaria `mount_woolly_mammoth` (Despertar III: Mamute de Komi) · casa:
  conjunto novo **Galeria do Mamute**.
- **Noite:** igual (subterrâneo).
- **No jogo (2026-11-09):** sem mecânica própria ainda (faltam os túneis que mudam por semana).
- **No jogo (2026-11-11): chefe do mapa O Mamute Avô** (`enemy_mountain_grandfather_mammoth`, Nv 214): anel de pedra na estrada da Boca do Gelo galerias abaixo (`MamuteAvoArena`); habilidade própria **o teto cai em cruz** (`CeilingFallHits`): pedras no lugar do herói e a 4,5 m na frente, atrás e dos lados; na diagonal é seguro. Loot: Troféu da Fera (60 %, 1–2); libertado, lembra o caminho para cima. Falta: o vale novo que ele abre.

### 63 · Mar das Auroras / *Aurora Sea* — nv 212–226 · ☆
- **Fonte:** invenção (a aurora como luz presa que dança, Ar Sereno).
- **Bioma e visual:** mar de placas de gelo flutuante; gelo, água escura, aurora verde-lilás `#7FE0B0`/`#B98CE0`.
- **Layout:** GG. Placas que se movem; ilha do farol no meio; **posto: Farol de Gelo**. Retorno: Porto da Lua Branca.
- **Inimigos:** Narval Apagado [rápido] (bicho real); Lobo-do-Mar (invenção: lobo que nada) [enxame]; Gelo Flutuante
  [controle]; Gaivota-Tempestade [distância].
- **Chefe:** **A Aurora Presa**, a luz da aurora presa num iceberg, que ataca sem querer; libertada, a aurora volta ao céu.
- **Mecânica:** **pular placas**: as placas se movem; quando a aurora "dança" (a cada 5 min), ela ilumina a rota segura.
- **Ganchos:** loot luz de aurora (refino), gelo · montaria `mount_river_seahorse` (Despertar) · casa: conjunto novo
  **Aurora** (lanterna verde).
- **Noite:** a aurora fica no céu a noite toda (visão ótima); as placas param.
- **No jogo (2026-11-09):** sem mecânica própria ainda (faltam as placas que se movem e a rota da aurora).

### 64 · Fundo do Velho Kul / *Old Kul's Deep* — nv 226–240 · ☆
- **Fonte:** os *kul*, espíritos da água dos contos komi (folclore).
- **Bioma e visual:** lago congelado enorme com um buraco no gelo; gelo, água negra, prata de peixe.
- **Layout:** GG. Lago com 7 buracos de pesca e o Buraco Grande; **posto: Buraco Grande de Pesca**. Retorno: Porto da Lua
  Branca.
- **Inimigos:** Lúcio Gigante [rápido] (bicho real); Kul Pequeno [enxame]; Peixe-Bruxa [controle] (bicho real); Foca-Sombra
  [tanque].
- **Chefe:** **chefe do mundo: o Velho Kul**, espírito d'água rabugento e apagado que prende os peixes; chama-se pescando
  (Pesca de hoje); vencido, solta os peixes e vira o "dono da pesca" (bônus de pesca no servidor).
- **Mecânica:** **pesca no gelo**: iscas certas puxam o chefe; peixes raros do lago.
- **Ganchos:** loot peixe-prata, Lendário **Peixe-Dourado** · montaria nova **Rena da Aurora** (variante prata) · casa:
  troféu do Velho Kul.
- **Noite:** os buracos brilham; o Kul fica mais forte.
- **No jogo (2026-11-09): pesca no gelo** (`IceFishingSession`, `OverworldLayouts.IceFishingHoles`): nos 7 buracos e no Buraco Grande, [E] até 4,5 m da borda (a pé), o peixe puxa em 5–11 s e há 1,4 s para apertar; cedo espanta, tarde escapa, andar recolhe a linha; cada buraco descansa 30 s. Pesos 70/24/5,5/0,5 (Lambari, Cascudo; Piau, Mandi; Surubim; Peixe-Dourado), raro × 2 de noite, Buraco Grande raro × 1,5 e lendário × 3; o peixe entra na Coleção e na mochila. Falta: as iscas e chamar o chefe pescando. **Chefe do mundo: o Velho Kul** (`enemy_mountain_old_kul`, nv 240): arena num anel de pedras novo no gelo atrás do Buraco Grande (0, 1762 na região), sempre ali, ainda sem vir à isca; as mesmas três fases do Rei-Vendaval, com bolas de frio (`LureGlob`); +25% à noite; ainda não solta os peixes nem dá o bônus de pesca. Loot próprio (2026-11-11): **Barbatana-Prata do Velho Kul** (`item_chefe_barbatana_kul`, épica, 45% por vitória, 360 de venda; no lugar do punhado de ouro, o loot do bioma segue igual); vai no Tear/Forja da casa (`HouseCraftRules`): Saia-calça de relâmpago da Maithe e Saia de aurora da Maria (Tear de mestre). Troféu de casa (2026-11-11): **Quadro da barbatana-prata do Velho Kul** (`casa_trofeu_031`, épico, parede), feito na Forja da casa com 2 Barbatana-Prata e o chefe já vencido (`HouseCraftRules.WorldBossTrophies`).

### Região 12 — Arquipélago de Hinokura (Fogo Vivo) · mapas 65–70 · ☆

Capital **Hinokura** (§2): japonês *hi no kura*, "celeiro do fogo", eco do celeiro celeste do ferreiro (bíblia §1.6).
Ilhas vulcânicas com palácios de laca vermelha (forma de Shuri e das *gusuku* de Ryukyu; sem santuários). NPCs com nomes
japoneses comuns escolhidos pelo sentido: **Mestra Akane** ("vermelho vivo", mestra da laca), **Kurogane** ("ferro
preto", ferreiro), **Sango** ("coral", barqueira). Uma ilha (67) é do vulcão guanche **Echeyde**. Chefe do mundo: **o
Glutão de Brasa** (mapa 70).

**No jogo (2026-11-09): mundo contínuo** (`OverworldLayouts.Hinokura.cs`, §0.10): 2400 × 2400 m ao norte do Teto do Mundo, a oeste
do Mar Gelado, 25 áreas em 20 terrenos novos — Hinokura (65, nv 220), Praia de Areia Negra e Costa das Fumarolas (entradas do Teto),
Estreito do Degelo (entrada do Mar Gelado), Canais de Lava e Fontes Termais; Jardins de Lava Laqueada com a Casa de Chá de Pedra,
Pinhais de Nuvem e Pontes Vermelhas (66, posto; segunda entrada do Mar Gelado); Campos de Retama com o Abrigo dos Pastores, Monte
Echeyde e Malpaís de Echeyde (67, posto; **Grampos de Escalada**); Canais Quentes, Recife de Obsidiana com o Barco-Farol e Ilhotas de
Vidro (68, posto; **Barco ou cais**); Muralhas Curvas, Castelo das Nove Fornalhas com o Portão Vermelho, Pátio das Fornalhas e Escória
Velha (69, posto); Campos de Cinza, Cinzas Vivas, Mar de Cinza, Borda Fria (posto), Encosta da Caldeira e Caldeira do Fogo Devorador
(70; **Água fresca**, até nv 280). Relevo: cone do Echeyde em três anéis com cratera (`VolcanoCone`), derrames de lava (`LavaFlows`),
lâminas do recife de obsidiana (`GlassReef`), muralhas curvas em anel com vãos (`CurvedRamparts`), borda da caldeira (`CalderaRim`);
cidade, praia e jardins baixos. Lava que brilha (`IsLava`): dois canais domados com ponte na estrada oeste, poças do malpaís e o lago
da caldeira. Água: fontes termais, lâminas do degelo, lagos dos jardins e das pontes, dois canais quentes e poças do recife. Cenário:
pedras de lava, pedra-pomes, lanternas de pedra, pinheiros em nuvem, portões de laca, fumarolas, lâminas de obsidiana, coral de brasa,
retama, rosas de pedra, pinheiros-canários, torres-fornalha, escória; poças quentes, cascalho rastelado, cinza e rachaduras de lava no
chão. Bichos existentes nos papéis das fichas (caranguejo-do-coco = caranguejo-vulcão, cobra da areia = lagarto de lava e salamandra,
abutre-brasa = gaivota-brasa e bomba vulcânica, pedra rolante = pedra-pomes, leão de pedra = shisa, peixe-lanterna = carpa de brasa,
névoa = bambu de fumaça, fumarola e fole, lontra ladra = tanuki, chacal = tibicena, crocodilo = lagarto-gigante, gosma = polvo e cinza
viva, lúcio gigante = peixe-lâmina, lanterna quebrada = coral de brasa, tartaruga de pedra = ouriço de vidro, soldado de terracota =
ushabti de laca, troll apagado = ogro-fornalha, enxame de vaga-lumes = faísca e chama faminta, golem de pedra = gigante de escória),
variante noturna em toda clareira, nenhum chefe (Guayota, a Tartaruga-Vulcão, o Mestre das Nove Fornalhas e o Glutão ficam para
depois; desde 2026-11-09 o Glutão de Brasa já é o chefe do mundo, no 70). Testes `OverworldHinokuraTests` e `OverworldWorldTests`. Mecânicas (2026-11-09): comportas e Tremor (65), erupção (67) e calor (70), ver as fichas. Falta: cascalho rastelado, resfriar a lava em ponte,
temperatura das fornalhas, forja do dia (bônus de refino), Guayota, a Tartaruga-Vulcão, o Mestre das Nove Fornalhas.

### 65 · Hinokura, a Ilha-Forja / *Hinokura, the Forge Isle* — nv 220–228 · ☆ · capital (Fogo Vivo)
- **Fonte:** castelo de Shuri e as *gusuku* de Ryukyu (laca e telhas vermelhas, muros curvos de pedra de coral); cidades-
  castelo do Japão (forma).
- **Bioma e visual:** ilha vulcânica com lava domada em canais; estilo novo `LacquerPalace` (palácios de laca vermelha
  sobre muros curvos, telhas vermelhas, portões duplos, pátios de pedra preta); paleta: laca `#B23A2E`, telha `#C6492F`,
  pedra vulcânica `#2E2B2B`, coral `#E8D9C0`, brasa `#FF8A3D`, verde de pinheiro `#3E6B4A`.
- **Layout:** GG. Palácio em 3 pátios no alto; cidade-forja em degraus; canais de lava que movem as forjas (Engenhos da
  Luz); porto de pedra. Retorno: Hinokura.
- **Inimigos (arredores):** Caranguejo-Vulcão [tanque]; Lagarto de Lava [rápido]; Gaivota-Brasa [distância];
  Pedra-Pomes Voadora [enxame].
- **Chefe:** nenhum na capital; evento **Tremor** (o vulcão treme e os canais mudam: guiar a lava para as forjas).
- **Mecânica:** **canais de lava**: abrir e fechar comportas muda qual forja funciona (refino, fusão, conserto ficam
  melhores na forja acesa do dia). G4 do Ícaro (Forja de Hinokura) é aqui.
- **Ganchos:** loot laca, ferro preto, obsidiana · montaria nova **Leão-Guardião de Laca** (§6) · casa: conjunto novo
  **Palácio de Laca** (biombo vermelho, lanterna de pedra).
- **No jogo (2026-11-09): montaria.** **Leão-Guardião de Laca** (`mount_lacquer_lion`, Épico): recompensa de `quest_city_hinokura_ember_gulls_on_the_lighthouse` (Sango). **Olho da Guarda** marca bichos e segredos a até 22 m.
- **Noite:** os canais de lava iluminam a ilha; lanternas vermelhas; visão boa.
- **No jogo (2026-11-09): comportas e Tremor** (`HinokuraForgeRules`, `HinokuraMechanicsDriver`): 3 comportas ([E] a até 7 m), duas junto às pontes dos canais de lava e uma no Pátio das Fornalhas (69); cada uma acende sua forja por 20 min. Tremor a cada 12 min no relógio, com aviso de 30 s: apaga uma forja em rodízio e emperra as comportas por 20 s. Facho de luz no local mostra o estado. Falta: o bônus de refino na forja acesa do dia e o G4 do Ícaro.
- **No jogo (2026-11-09): cidade de verdade** (`CityTownRecipes.Hinokura.cs`, estilo `LacquerPalace`, `CityCatalog.LateRegionCities`, id `hinokura`): em y = 0 dentro de um círculo de 22,5 m (saídas O, L e S a 19 m; a rua Sul é a entrada; sem rua ao norte): salões de laca vermelha sobre muros curvos de pedra de coral, telhas vermelhas com cumeeiras brancas e cantos virados; a Forja de Hinokura na praça; canais de lava domada com comportas e pontes de pedra preta, lanternas de pedra, pinheiros podados, rochas de lava, portão de pilares vermelhos, cais de pedra; Mestra Akane, Kurogane, Sango e Hikari com falas para cada primo; Ferro Preto do Kurogane com as exigências de 67 (Grampos), 68 (Passagem de Dhow) e 70 (Odre) fixas, mais Barco de Junco, Corda, Teriaga, Amuleto Antiveneno e Vara de Equilíbrio; missões "Caranguejos nos Varais" e "Gaivotas no Barco-Farol" (paga com Barco de Junco); respawn na praça para 65–70. Falta: o bônus da forja do dia e o G4 do Ícaro (comportas, Tremor e o Leão-Guardião de Laca já existem, linhas acima).

### 66 · Jardins de Lava Laqueada / *Lacquered Lava Gardens* — nv 226–236 · ☆
- **Fonte:** jardins de pedra e cascalho rastelado (forma); as *shisa*, os leões-cão de cerâmica dos telhados de Okinawa,
  como guardiões que ficaram apagados.
- **Bioma e visual:** jardins de pedra vulcânica, musgo, pinheiros podados, pontes vermelhas; preto, musgo, laca.
- **Layout:** G. 6 jardins ligados por pontes; **posto: Casa de Chá de Pedra**. Retorno: Hinokura.
- **Inimigos:** Shisa Apagada [elite] (desceu do telhado e esqueceu de voltar); Carpa de Brasa [rápido]; Bambu de Fumaça
  [controle]; Tanuki Ladrão [rápido] (o texugo brincalhão dos contos japoneses).
- **Chefe:** **O Rei Shisa do Telhado**; libertado, volta para o telhado do palácio e "late" quando vem tremor.
- **Mecânica:** **cascalho**: rastelar os desenhos certos (quebra-cabeça) acalma os jardins e abre a casa de chá secreta.
- **Ganchos:** loot musgo de lava, cerâmica · montaria nova **Leão-Guardião de Laca** · casa: conjunto novo **Jardim de
  Pedra**.
- **Noite:** as lanternas de pedra acendem; os tanukis saem.
- **No jogo (2026-11-11): chefe do mapa O Rei Shisa do Telhado** (`enemy_mountain_shisa_king`, Nv 236): anel de pedra na estrada leste da Casa de Chá, nas Pontes Vermelhas (`ReiShisaArena`); habilidade própria **as duas bocas** (`ShisaMouthFor`, `ShisaHits`): alterna boca aberta (acerta quem está a mais de 6 m) e boca fechada (acerta quem está perto). Loot: Pedra-Faísca (60 %, 2–3); libertado, volta para o telhado. Falta: o cascalho rastelado e a casa de chá secreta.

### 67 · Monte Echeyde / *Mount Echeyde* — nv 234–246 · ☆
- **Fonte:** *Echeyde*, nome guanche do Teide (Tenerife); Guayota, o ser das profundezas do vulcão que, no conto, prendeu
  o sol dentro da montanha; os *tibicenas*, cães negros dele (mito guanche, cultura extinta).
- **Bioma e visual:** vulcão alto com campos de lava e retama em flor; lava `#3A2E2A`, pedra-pomes `#C8BBA8`, retama
  `#E8D37A`, céu limpo; "rosas de pedra", pinheiro-canário.
- **Layout:** GG (vertical). Subida em 3 anéis até a cratera; **posto: Abrigo dos Pastores** (pastores de cabra, fogueira
  de pedra). Retorno: Hinokura.
- **Inimigos:** Tibicena [rápido] (cão de sombra); Lagarto-Gigante de Lava [tanque]; Bomba Vulcânica [distância];
  Fumarola [controle].
- **Chefe:** **Guayota, o que Prendeu o Sol**: uma sombra dentro do vulcão que guarda a luz para ela; vencida, a luz sai
  pela cratera ao amanhecer (cena bonita).
- **Mecânica:** **erupção**: a cada 10 min a cratera cospe; as rotas de lava mudam; os pastores avisam com buzina.
- **Ganchos:** loot pedra-pomes, obsidiana · montaria nova **Cabra-dos-Pastores do Vulcão** (§6) · casa: conjunto novo
  **Retama** (vaso de rosa de pedra).
- **No jogo (2026-11-09): montaria.** **Cabra-dos-Pastores do Vulcão** (`mount_volcano_goat`, Incomum): apito por 145 de ouro em Hinokura (Ferro Preto do Kurogane). **Cascos de Pedra-Pomes** apagam e afastam queimaduras por 8 s. Falta: não anula chão quente, que ainda não existe no mapa.
- **Noite:** os tibicenas dobram; a lava ilumina; céu de estrelas fortíssimo.
- **No jogo (2026-11-09): erupção** (`HinokuraEruptionRules`): a cada 10 min no relógio, 30 s de buzina com a rota marcada, depois 20 s de lava em 1 de 3 rotas de 14 m na encosta (rodízio); dentro dela 4% da vida máxima por segundo, nunca abaixo de 15%. Não bloqueia caminho.

### 68 · Recife de Obsidiana / *Obsidian Reef* — nv 244–254 · ☆
- **Fonte:** invenção (lava que entrou no mar e virou vidro).
- **Bioma e visual:** recife preto e brilhante, vapor; obsidiana `#1E1A1E`, vapor `#E9E4DE`, coral de brasa `#E8613A`.
- **Layout:** G. Ilhotas de vidro e canais quentes; **posto: Barco-Farol** (Sango). Retorno: Hinokura.
- **Inimigos:** Polvo de Obsidiana [controle]; Peixe-Lâmina [rápido]; Coral de Brasa [distância]; Ouriço de Vidro
  [tanque].
- **Chefe:** **A Tartaruga-Vulcão**, que carrega uma ilha nas costas e esquentou demais; libertada, esfria e a ilha vira
  jardim.
- **Mecânica:** **resfriar**: bombas de água (consumível) ou skills de Água Viva endurecem a lava em ponte.
- **Ganchos:** loot obsidiana, vidro de brasa · montaria `mount_elder_tortoise` (Despertar IV: casco de vidro) · casa:
  conjunto novo **Vidro de Brasa**.
- **Noite:** o recife brilha laranja; vapor esconde bichos.

### 69 · Castelo das Nove Fornalhas / *Castle of Nine Furnaces* — nv 252–264 · ☆
- **Fonte:** invenção (a maior forja dos Artífices, dentro de um castelo de laca); o ferreiro celeste Kurdalægon (bíblia
  §1.6) como lenda.
- **Bioma e visual:** castelo de muros curvos com 9 fornalhas em torre; laca, ferro, brasa; correntes, foles gigantes.
- **Layout:** GG. 9 torres em anel e o pátio central; **posto: Portão Vermelho**. Retorno: Hinokura.
- **Inimigos:** Ushabti de Laca [tanque]; Ogro-Fornalha [elite] (invenção); Fole Vivo [controle]; Faísca Raivosa
  [enxame].
- **Chefe:** **O Mestre das Nove Fornalhas**, engenho que forja sem parar para Āz; parado, as fornalhas viram a forja do
  servidor.
- **Mecânica:** **temperatura**: cada fornalha tem um medidor; equilibrar as 9 (quebra-cabeça em grupo) abre o pátio.
- **Ganchos:** loot ferro celeste, conjunto antigo "Kurdalægon" · montaria `mount_dragon_hatchling` (Despertar III) · casa:
  conjunto novo **Fornalha** (bigorna de enfeite).
- **Noite:** as fornalhas ficam mais quentes (mais difícil); faíscas no céu.
- **No jogo (2026-11-11): chefe do mapa O Mestre das Nove Fornalhas** (`enemy_mountain_nine_furnaces_master`, Nv 264, mais forte à noite): anel de pedra na estrada do pátio das fornalhas (`MestreDasFornalhasArena`); habilidade própria **martelada que esquenta** (`ForgeHeatMultiplier`): bate onde o herói estava, cada martelada +8 % sobre a anterior até a nona, que solta o vapor e recomeça. Loot: Pedra da Mistura (40 %); parado, as fornalhas viram a forja de todos. Falta: o medidor de temperatura das 9 fornalhas.

### 70 · Caldeira do Fogo Devorador / *Devouring Fire Caldera* — nv 264–280 · ☆
- **Fonte:** o Fogo Devorador, contraparte da Treva do Fogo Vivo (Teodoro bar Konai; bíblia §1.3): o 2º dos cinco mundos
  da Treva, cujo povo são os de quatro patas.
- **Bioma e visual:** caldeira gigante de lava aberta; lava, cinza `#6A6460`, brasa, céu vermelho-escuro.
- **Layout:** GG. Anel de pedra fria em volta do lago de lava; ilhas no lago; **posto: Borda Fria**. Retorno: Hinokura.
- **Inimigos:** Chama Faminta [enxame]; Salamandra Devoradora [rápido]; Gigante de Escória [tanque]; Cinza Viva [controle].
- **Chefe:** **chefe do mundo: o Glutão de Brasa**, fogo sem rosto que come o próprio fogo das forjas; aliado de Āz.
  Recua quando as forjas de Hinokura ficam acesas (como a Treva).
- **Mecânica:** **calor**: medidor de calor (sobe perto da lava); pedras frias e Chá de Karkadé zeram.
- **Ganchos:** loot Pedra da Mistura, brasa viva · montaria `mount_fire_rhea` (Despertar IV) · casa: troféu do Glutão.
- **Noite:** igual (a caldeira é sempre noite); a lava sobe um pouco.
- **No jogo (2026-11-09): chefe do mundo: o Glutão de Brasa** (`enemy_mountain_ember_glutton`, nv 280): arena num anel de pedras novo na pedra fria ao norte da borda da caldeira (-1440, 1690 na região; o anel de dentro é estreito demais entre a lava e a subida); as mesmas três fases, com rajadas de brasa; não muda à noite. Com as forjas acesas ele recua: a cada 10 s, por forja acesa, −1,5% da vida máxima (nunca abaixo de 40%) e 0,5 s de cambaleio (metade em chefes). Loot próprio (2026-11-11): **Brasa Viva do Glutão** (`item_chefe_brasa_viva`, épica, 45% por vitória, 400 de venda; no lugar do punhado de ouro, o loot do bioma segue igual); vai no Tear/Forja da casa (`HouseCraftRules`): Calça de malha condutora do Ícaro (Forja, 2) e Pernas mecânicas assistidas (Tear de mestre). Troféu de casa (2026-11-11): **Braseiro da brasa viva do Glutão** (`casa_trofeu_032`, épico, piso), feito na Forja da casa com 2 Brasa Viva e o chefe já vencido (`HouseCraftRules.WorldBossTrophies`). Falta: a montaria.
- **No jogo (2026-11-09): medidor de calor** (`HinokuraHeatRules`): 0–100, sobe até 3/s na beira do lago de lava e some a 90 m; cai 12/s longe; 4 pedras frias (7 m) zeram na hora; Chá de Karkadé ativo reduz o ganho à metade (a refeição é feita na vila, então não zera). Avisos em 50 e 80; só em 100 machuca: 1,5% da vida máxima por segundo, nunca abaixo de 20%. Anel em volta do herói mostra o calor.

### Região 13 — Rio das Doze Horas (o rio da noite de Kemet) · mapas 71–76 · ☆

No conto de Kemet, a barca do sol atravessa à noite um rio escuro dividido em **doze horas**, e a serpente Apep tenta
pará-la todas as noites (*Amduat*, *Livro dos Portões*). No jogo é o rio por onde a **Barca da Lua** desce antes de subir
de novo: cada mapa é uma "hora". Cidade: **Cais de Mesektet** (*Mesektet* = a barca da noite). Estilo novo `PapyrusQuay`
(colunas de papiro, barcos de junco, telhados de esteira sobre pilares). Paleta: noite `#1E2050`, ouro `#D4AF37`, papiro
`#8DAA4A`, arenito `#C9A27A`, água escura `#22324A`. NPCs com nomes de **contos** de Kemet (personagens de literatura,
não divindades): **Sinuhe** (o viajante que voltou para casa; aqui, o barqueiro-chefe), **Khunanup** (o "camponês
eloquente"; aqui, o comerciante que negocia falando bonito), **Djedi** (o mágico do Papiro Westcar; aqui, treinador de
magia), **Setne** (o príncipe dos contos demóticos que procurava livros; aqui, o bibliotecário do Caderno). Chefe do
mundo: **Apep** (mapa 76), que nunca morre: recua toda noite.

**No jogo (2026-11-09): mundo contínuo** (`OverworldLayouts.Rio.cs`, §0.10): 2400 × 2400 m ao norte do Arquipélago de Hinokura (abre a quinta
fileira do mundo; as duas outras células da fileira viram buracos murados), 25 áreas em 20 terrenos novos — Cais de Mesektet (71, nv 260), Margem de
Papiro, Foz das Cinzas e Praia dos Remadores (entradas de Hinokura), Píeres das Horas e Canal da Barca; Campos de Juncos com a Casa de Junco, Canteiros
de Juncos, Trigais de Luz e Canais de Sekhet (72, posto; **Barco de Junco**); Areal Escondido com o Trenó de Areia, Portas Fechadas, Zigue-Zague do Areal e
Caminho das Serpentes (73, posto; **Lamparina acesa**); Anel Externo, Anéis de Mehen com o Anel Quieto e Nó Errado (74, posto; **Lamparina Rúnica**); Beira
Fervente, Praia de Brasa e Lago de Fogo com a Ilha das Palmeiras (75, posto; **Óleo de Fogo Eterno**); Banco de Areia de Apep com a Proa Encalhada, Corpo de
Apep, Rio Seco, Escamas Roxas e Areia Movediça (76, posto; **Amuleto Antiveneno**, até nv 320). Todas as exigências estão no balcão fixo das Trocas do
Khunanup. Relevo: canteiros em grade (`ReedBedGrid`), dunas em zigue-zague (`ZigzagDunes`), anéis de escama em volta do Anel Quieto (`CoilRings`), o corpo de
Apep em cristas sinuosas (`ApepBody`); cais e campos baixos. Água: o rio escuro ao norte do cais (duas pontes nas estradas do norte), dois canais nos
campos (três pontes) e seis lagoas; fogo: três lóbulos do Lago de Fogo em volta da Ilha das Palmeiras e duas poças (brilham como a lava de Hinokura). Cenário:
papiros, feixes de trigo de luz, portas de pedra, escaravelhos de ouro, arcos de Mehen, tamareiras, obeliscos da noite, escamas de Apep, cascos de junco; vitórias-régias,
ondulações de areia, poças de fogo e lascas de escama no chão. Bichos existentes nos papéis das fichas (crocodilo-do-mangue = crocodilo do rio escuro e de lodo, gaivota
ladra = íbis apagada e ganso ladrão, besouro-da-casca = escaravelho de sombra, névoa do vento ruim = junco-laço, enxame de vaga-lumes = gafanhoto e chama faminta,
búfalo-do-barro = hipopótamo de lodo, cobra da areia = cobra do junco e salamandra, píton apagada = serpente de três cabeças e filhos de Apep, escorpião = escorpião negro,
ushabti de corda = ushabti de areia e apagado, chacal = mordedor de sombra, gosma = treva rastejante, sombra fervida e areia movediça, soldado de terracota = ushabti
rachado, abutre-brasa na foz), variante noturna em toda clareira, nenhum chefe (Barca Atrasou, Hipopótamo-de-Junco, Grande Serpente, Nó Errado, Guardião do Lago e Apep ficam
para depois; desde 2026-11-09 Apep já é o chefe do mundo, no 76). Testes `OverworldRioTests` e `OverworldWorldTests`. Mecânicas (2026-11-09, `RioMechanicsDriver`): horas da barca (71), anel que gira (74) e fogo que escolhe (75), ver as fichas. Falta: jangadas, puxar o trenó, defender a barca e os chefes de mapa das fichas (o do mundo e a
Íbis Real de Mesektet, montaria do 71, já existem).

### 71 · Cais de Mesektet / *Mesektet Quay* — nv 260–268 · ☆ · cidade
- **Fonte:** a barca *Mesektet* da noite e a *Mandjet* da manhã (textos de Kemet; bíblia §3.16).
- **Bioma e visual:** cais flutuante no rio escuro, margens de papiro; estilo `PapyrusQuay` e paleta acima.
- **Layout:** GG. Cidade sobre balsas e pilares; o Cais da Barca no meio; 12 píeres, um por "hora". Retorno: Cais de
  Mesektet.
- **Inimigos (margens):** Crocodilo do Rio Escuro [tanque] (bicho real); Íbis Apagada [rápido]; Escaravelho de Sombra
  [enxame]; Junco-Laço [controle].
- **Chefe:** nenhum na cidade; evento **A Barca Atrasou** (defender a barca no cais quando a sombra a segura).
- **Mecânica:** **as horas**: a cada 12 min de relógio a barca passa e abre o píer da hora seguinte; os mapas 72–76 abrem
  em sequência (online: horário do servidor).
- **Ganchos:** loot papiro, ouro de Kemet · montaria nova **Íbis Real de Mesektet** (§6) · casa: conjunto novo **Cais do
  Papiro** (coluna de papiro, barquinha).
- **No jogo (2026-11-09): montaria.** **Íbis Real de Mesektet** (`mount_mesektet_ibis`, Rara, voadora): recompensa de `quest_city_mesektet_scorpions_in_the_sands` (Djedi). **Planar de Mesektet**: 16 m em 1,4 s; conta como montaria alada nas exigências de asas.
- **Noite:** sempre noite aqui (o rio é a noite); estrelas no teto do céu como um tecido.
- **No jogo (2026-11-09): cidade de verdade** (`CityTownRecipes.Mesektet.cs`, estilo `PapyrusQuay`, `CityCatalog.LateRegionCities`, id `cais_de_mesektet`), já no mundo contínuo: em y = 0 dentro de um círculo de 22,5 m (saídas O, L e S a 19 m; a rua Sul é a entrada; sem rua ao norte: a margem cai para o rio escuro), salões de arenito sobre balsas, a Mesektet no Cais da Barca, dois píeres das horas com pontes de tábua, colunas de papiro, moitas e barquinhos; Sinuhe, Khunanup, Djedi e Setne; Trocas do Khunanup com as exigências de 72–76 fixas; missões "Crocodilos no Cais" e "Escorpiões do Areal"; respawn na praça para 71–76. Falta: o evento A Barca Atrasou (a Íbis Real já existe, linha acima).
- **No jogo (2026-11-09): horas da barca** (`RioHourRules`, `OverworldLayouts.HourPiers`/`BarqueBerths`): 12 píeres na margem sul do rio escuro (70 m entre si, x −385…385), um por hora; a hora dura **3 min** de relógio UTC (não 12: daria quase uma hora de espera por posto; a roda inteira dá 36 min e todo posto passa ao menos a cada 21 min), a mesma para todos no online. No píer da hora há um facho dourado alto e a barca no rio; `[E]` embarca (2,5 s) e deixa o herói no posto da vez (píer mod 5: Casa de Junco, Trenó de Areia, Anel Quieto, Ilha das Palmeiras, Proa Encalhada), recusando com aviso se a área de destino ainda está fechada para ele. Em cada posto há um cais de volta (facho azul), **sempre** servido, que leva à estrada sul do Cais. Atalho: toda a região continua aberta a pé.
- **No jogo (2026-11-09): conjunto de equipamento Cais do Papiro** (`ArmorSetRules`, Docs/Items.md): Couraça de Papiro Trançado e Touca de Junco do Remador, Defesa 32/18; 2 peças: Hora da Barca (ao matar, 25% de curar 5% da vida). Só por drop dos 12 bichos próprios da Região 13 (0,5% por abate).
- **No jogo (2026-11-09): conjunto de casa **Móveis do Cais do Papiro**** (móveis, `Docs/Housing/CatalogoCasa.md`): 8 peças à venda em Trocas do Khunanup, 30–480 de ouro; Épica só como especial da semana. Coluna de papiro entalhada, Barquinha de junco trançado, Mapa das doze horas.

### 72 · Campos de Juncos / *Reed Fields* — nv 266–276 · ☆
- **Fonte:** os "campos de juncos" (*sekhet-aaru*), o lugar calmo e fértil dos contos de Kemet.
- **Bioma e visual:** campos de trigo e juncos dourados sob um céu escuro estrelado; trigo `#D8B85A`, junco, canais.
- **Layout:** G. Canteiros em grade com canais; **posto: Casa de Junco**. Retorno: Cais de Mesektet.
- **Inimigos:** Gafanhoto de Sombra [enxame]; Hipopótamo de Lodo [tanque] (bicho real); Cobra do Junco [rápido]; Ganso
  Ladrão [rápido] (bicho real).
- **Chefe:** **O Hipopótamo-de-Junco Gigante**, que pisou nos canais; libertado, ajuda a cavar.
- **Mecânica:** **jangadas**: colher junco e montar jangadas (2 min) abre canais longos; a Taweret Hipopotinha
  (`mount_capybara`) passa direto.
- **Ganchos:** loot trigo de luz, junco · montaria `mount_capybara` (Despertar III) · casa: conjunto novo **Campo de
  Juncos**.
- **Noite:** sempre noite; os campos brilham fraco.
- **No jogo (2026-11-09):** a barca das horas para na Casa de Junco.
- **No jogo (2026-11-11): jangadas** (`RioRaftRules`, canais e touceiras em `OverworldLayouts.RioMechanics.cs`, driver novo `RioRaftMechanicsDriver`, teste `RioRaftMechanicsTests`): 8 touceiras de junco nas margens dos 2 canais compridos (z 500 e 760, 840 m dos Canteiros aos Trigais); [E] corta 1 feixe (a touceira cresce de novo em 5 min); 3 feixes amarram uma jangada num embarcadouro (12 m depois de cada ponta), que desce o canal inteiro a 14 m/s (864 m em ~62 s, contra ~2 km pelas estradas da Casa de Junco) e aguenta 2 min para a volta; os hipopótamos de lodo mordem 2% da vida a cada 5 s (3% à noite, nunca abaixo de 30%) e o tranco da chegada tira 6% dos bichos a até 8 m; com a Hipopotinha de Taweret (`mount_capybara`) passa direto, sem junco e sem mordida. Feixes e jangada ficam no save (marcas de tempo `rio.raft.reed.N` e `rio.raft.built`). Falta: o Hipopótamo-de-Junco Gigante.

### 73 · Areal Escondido / *Hidden Sands* — nv 274–286 · ☆
- **Fonte:** a "4ª hora" do *Amduat*, um deserto onde a barca vira serpente para atravessar a areia.
- **Bioma e visual:** areal escuro com caminhos em zigue-zague e portas de pedra; areia `#8A7350`, pedra, ouro.
- **Layout:** G. Caminhos em diagonal com portas fechadas; **posto: Trenó de Areia**. Retorno: Cais de Mesektet.
- **Inimigos:** Serpente de Três Cabeças do Areal [elite] (as serpentes do *Amduat*, como bicho); Escorpião Negro
  [rápido]; Escaravelho de Sombra [enxame]; Ushabti de Areia [tanque].
- **Chefe:** **A Grande Serpente do Areal**, que esqueceu que puxava a barca; libertada, puxa de novo.
- **Mecânica:** **puxar o trenó**: a barca vira trenó e o grupo puxa pela corda (botão segurado juntos); sozinho, devagar.
- **Ganchos:** loot ouro escuro, escama · montaria `mount_golden_scarab` (Despertar III) · casa: **Areias Flamejantes**.
- **Noite:** sempre noite; as portas brilham quando a barca passa.
- **No jogo (2026-11-09):** sem mecânica própria ainda (falta puxar o trenó); a barca das horas para no Trenó de Areia.

### 74 · Anéis de Mehen / *Coils of Mehen* — nv 284–296 · ☆
- **Fonte:** Mehen, a serpente que se enrola em volta do sol para protegê-lo à noite (Kemet; já é a montaria
  `mount_boitata`).
- **Bioma e visual:** um labirinto feito dos anéis de uma serpente enorme de luz; ouro, verde-escamas `#5E8F5A`, noite.
- **Layout:** G. Anéis concêntricos que giram devagar; **posto: Anel Quieto**. Retorno: Cais de Mesektet.
- **Inimigos:** Mordedor de Sombra [rápido] (sombras que mordem Mehen); Treva Rastejante [enxame]; Escorpião [tanque];
  Ushabti Apagado [tanque].
- **Chefe:** **O Nó Errado**, um nó de Treva que amarrou Mehen; desfeito, Mehen se enrola certinho e protege o cais.
- **Mecânica:** **labirinto que gira**: os anéis giram a cada 30 s; as aberturas se alinham por pouco tempo.
- **Ganchos:** loot escama de Mehen · montaria `mount_boitata` (Mehen, Despertar IV) · casa: conjunto novo **Anel de
  Mehen**.
- **Noite:** sempre noite; os anéis brilham onde a sombra foi tirada.
- **No jogo (2026-11-09): anel que gira** (`RioMehenRules`, `OverworldLayouts.MehenGates`): um anel de luz gira sobre a muralha de escamas de raio 144 m, com duas aberturas opostas; a cada 30 s ele gira 45° (18 s) e segura 12 s (fica dourado). Oito portões cruzam a muralha, a 45° um do outro, entre os vales de raio 96 e 192 m: `[E]` só funciona com uma abertura parada na frente (cada portão 12 s a cada 120 s; algum par a cada 30 s, o vizinho fica a 14 s a pé) e atravessa em 6 s por cima da muralha, nos dois sentidos. O relevo não muda: é atalho, e as estradas continuam dando a volta.

### 75 · Lago de Fogo / *Lake of Fire* — nv 292–306 · ☆
- **Fonte:** o lago de fogo dos textos de Kemet, que queima a sombra e refresca quem tem luz, guardado por babuínos (aqui
  NPCs amigos).
- **Bioma e visual:** lago de chamas laranja com ilhas de palmeira; brasa, água-fogo `#FF9A3D`, palmeira, noite.
- **Layout:** G. Lago quadrado com ilhas; **posto: Ilha das Palmeiras** (babuínos que acenam e avisam). Retorno: Cais de
  Mesektet.
- **Inimigos:** Chama Faminta [enxame]; Salamandra de Sombra [rápido]; Sombra Fervida [controle]; Ushabti Rachado [tanque].
- **Chefe:** **O Guardião do Lago**, ushabti gigante que deixou de deixar a luz passar; libertado, abre o caminho.
- **Mecânica:** **fogo que escolhe**: o lago **cura** o herói (fora de combate) e **machuca** bichos apagados; atrair os
  bichos para a beira ajuda.
- **Ganchos:** loot brasa de luz · montaria `mount_sertao_phoenix` (Bennu, Despertar IV) · casa: **Cais do Papiro**.
- **Noite:** sempre noite; o lago ilumina tudo.
- **No jogo (2026-11-09): fogo que escolhe** (`RioFireRules`, `OverworldLayouts.LagoDeFogoPools`): a até 8 m da borda do fogo (os três lóbulos e as duas poças), o herói recebe +2,5% da vida máxima por segundo se ficou 5 s sem apanhar; um círculo de brasa em volta dele marca a beira. Os bichos na mesma faixa tomam fogo de 4% da vida máxima por segundo, nunca abaixo de 20% da vida e nunca os chefes: o fogo amacia, a morte, a experiência e o saque continuam do herói. O fogo nunca machuca o herói.

### 76 · Banco de Areia de Apep / *Apep's Sandbank* — nv 304–320 · ☆
- **Fonte:** Apep (Apófis), a serpente do caos que bebe o rio para encalhar a barca toda noite (Kemet).
- **Bioma e visual:** o rio seco num banco de areia enorme; areia, escama roxa `#5A3A6E`, barca encalhada dourada.
- **Layout:** GG. A barca encalhada no meio; o corpo de Apep enrolado em volta; **posto: Proa Encalhada**. Retorno: Cais
  de Mesektet.
- **Inimigos:** Filhos de Apep [rápido]; Areia Movediça [controle]; Escaravelho de Sombra [enxame]; Crocodilo de Lodo
  [tanque].
- **Chefe:** **chefe do mundo: Apep, a Serpente que Bebe o Rio**. **Nunca morre:** derrotada, cospe o rio e recua até a
  noite seguinte (online: toda noite do servidor). A barca segue.
- **Mecânica:** **defender a barca**: a barca tem vida; o grupo protege enquanto os remadores a desencalham.
- **Ganchos:** loot escama de Apep, Pedra da Mistura · montaria nova **Barquinha de Luz** (lendária, §6) · casa: troféu de
  Apep (escama).
- **Noite:** sempre noite; Apep mais forte perto da meia-noite do relógio.
- **No jogo (2026-11-09): chefe do mundo: Apep** (`enemy_mountain_apep`, nv 320): arena num anel de pedras novo no banco, a leste da Proa Encalhada (170, 1590 na região), ao sul da espinha; as mesmas três fases, com rajadas de lodo. Própria: **beber o rio** (a cada 14 s, da 2ª fase, aviso de 2,5 s: quem estiver a mais de 7 m dela leva 1,1× e ela cura 2%). Mais forte até 1,4× na meia-noite do relógio (±3 h). Não morre: cospe o rio, recua e só volta numa noite seguinte. Loot próprio (2026-11-11): **Escama de Apep** (`item_chefe_escama_apep`, épica, 40% por vitória, 450 de venda; no lugar do punhado de ouro, o loot do bioma segue igual); vai no Tear/Forja da casa (`HouseCraftRules`): Saia de eclipse da Lavine (Tear, 2) e Saia do horizonte apagado (Tear de mestre). Troféu de casa (2026-11-11): **Barca de luz vencendo Apep, em miniatura** (`casa_trofeu_033`, épico, piso), feito na Forja da casa com 2 Escama de Apep e o chefe já vencido (`HouseCraftRules.WorldBossTrophies`). Falta: os ganchos (Pedra da Mistura, Barquinha de Luz).
- **No jogo (2026-11-09):** a barca das horas para na Proa Encalhada.
- **No jogo (2026-11-11): defender a barca** (`RioBarqueDefenseRules` + `RioBarqueDefense`, casco em `OverworldLayouts.RioMechanics.cs`, driver `RioMechanicsDriver`, teste `RioBarqueDefenseMechanicsTests`): a barca encalhada fica na areia lisa a sudeste da Proa (40, 1500 na região), longe da espinha de Apep. [E] (ou "Usar") a até 6 m chama os remadores: eles empurram por 90 s de tempo guardado (herói a até 25 m; 15 s seguidos longe e eles largam os remos). O casco tem 100: cada bicho a até 12 m rói 1,5 por segundo (chefe 4; no máximo 6 contados); Apep bebe o rio aos 20, 40, 60 e 80 s (aviso 3 s antes, anel roxo): tira 12 do casco, ou, se o herói estiver na proa (5 m), cai nele: 4% da vida máxima, nunca abaixo de 30%. A cada 2 s os remos batem nos bichos a até 6 m (4% da vida, chefe 0,5%). Casco a zero ou abandonada: 30 s de descanso. Solta: devolve 20% da vida, a luz tira 12% dos bichos a até 20 m (chefe 2%) e, até o fim do dia do jogo (`WorldMapProgress.SetDailyMark`, chave `rio.barque.freed`), a barca solta navega pelo banco a 55 m/s por cima das dobras de Apep: do casco às duas pontas da estrada (Corpo de Apep, −600, 1530; Rio Seco, 600, 1530) em 10–12 s, e de volta; a pé são 100–115 s subindo e descendo a espinha. Uma vez por dia; sem prêmio em item (o prêmio é o atalho). Falta: grupo de verdade (online) e a ligação com a luta de Apep.

### Região 14 — Margens da Mistura (Elam, Mesopotâmia, terras hurritas; o deserto do Tempo) · mapas 77–82 · ☆

As "Margens da Mistura" (nome das terras de disputa do online, bíblia §3.11) são o lugar onde luz e sombra estão mais
misturadas. Culturas: elamita e mesopotâmica (zigurates, tijolo vidrado, betume), hurrita (Upelluri, Ullikummi) e o
*Livro dos Gigantes* (que traz Hobabish, o Humbaba do conto de Gilgamesh). Cidade: **Dur-Jarras** (acadiano *dūr* =
fortaleza; eco de Dur-Untash, o nome antigo de Chogha Zanbil). Estilo novo `Ziggurat`. Paleta: tijolo cozido `#B07A4A`,
tijolo vidrado azul `#2F5FA8`, betume `#2A2420`, junco, ouro, céu `#E8C690`. NPCs: **Sin-leqi-unninni** (o escriba que
copiou o conto de Gilgamesh; aqui, o bibliotecário), **Atrahasis** (o construtor do barco do dilúvio; aqui, o mestre das
balsas de betume), **Utnapishtim** (o velho que viu o dilúvio; aqui, cuida do Relógio de Zurvan da cidade). Chefe do
mundo: **o Sonho da Destruição** (mapa 82).

**No jogo (2026-11-09): mundo contínuo** (`OverworldLayouts.Mistura.cs`, §0.10): 2400 × 2400 m ao norte do Mar Gelado, a leste do Rio das Doze Horas (preenche um dos
buracos murados da quinta fileira; duas passagens abertas na borda oeste levam à Beira Fervente do rio), 25 áreas em 20 terrenos novos — Dur-Jarras, Portão de Tijolo,
Olarias, Colinas de Tell, Canal das Palmeiras e Terraços de Cevada (77, cidade, nv 300–307); Deserto do Relógio com o Gnômon Caído e os 12 gnômons em círculo, Sombras Atrasadas e
Areia das Horas (78, posto; **Odre de Água Fresca**); Poços de Betume com a Balsa de Junco, Poças de Piche e Juncal de Piche (79, posto; **Barco de Junco**); Cedral de Hobabish com a
Clareira do Lenhador e as 7 lanternas de resina, Beira do Cedral e Cedros Anciãos (80, posto; **Corda de Escalada**); Ombro de Upelluri com o Ninho no Ombro, Dobras de Pedra,
Clavícula do Gigante, Diorito Crescente e Beiral do Céu (81, posto; **Grampos de Escalada**); Campo das Tábuas com a Tenda dos Intérpretes, Jardim Cortado, Água Parada, Tábuas
Quebradas e Tábuas do Sonho (82, posto; **Lamparina acesa**, até nv 360). Todas as exigências estão no balcão fixo do Betume do Atrahasis. Relevo: mostrador de 12 raios (`GnomonDial`), anéis de
raízes (`CedarRings`), a corcova do ombro (`GiantShoulder`), dobras de pele de pedra (`SkinFolds`), o cone de diorito (`DioriteRise`, ~22 m); cidade e deserto baixos. Água: canal de betume
com três pontes e lagoas de piche e de água parada. Cenário: leões vidrados, fornos e pilhas de tijolo, gnômons caídos, pilares-ampulheta, jarras de betume, lanternas de resina, cedros
gigantes, lascas de diorito, tábuas de pedra, troncos cortados; cacos de tijolo, marcas de mostrador, bolhas de piche, gotas de resina e lascas no chão. Bichos existentes nos papéis das
fichas (leão de pedra = leão de tijolo, grifo jovem = mušḫuššu, escorpião = homem-escorpião, besouro-da-casca = gafanhoto de betume, golem = ampulheta viva, lagarto de lava = lagarto do instante,
névoa do vento ruim = sombra atrasada, ushabti de laca = ushabti de relógio, gosma = betume vivo, gaivota-brasa = garça de piche, lúcio = bagre negro, harpia de fumaça = fumaça de piche, troll apagado =
cedro raivoso, irbis = lince, abelha dourada = abelha-resina, lanterna quebrada = manto de brilho, pedra rolante = diorito, águia = ave do ombro, gigante de escória = filho de Ullikummi, soldado de terracota =
tábua quebrada, gigante sonâmbulo = jardim-sombra, coruja = pássaro-sonho, foca de sombra = água que apaga), variante noturna em toda clareira, nenhum chefe (Instante Perdido, Rei do Piche, Hobabish, Ullikummi e
Sonho da Destruição ficam para depois; desde 2026-11-09 o Sonho da Destruição já é o chefe do mundo, no 82). Testes `OverworldMisturaTests` e `OverworldWorldTests`. Falta: degraus do zigurate, os sete mantos e acordar/sonhar. *(Corrigido em 2026-11-11: bolhas de tempo, acender o piche e o ombro que respira estão nas fichas 78, 79 e 81; as montarias Mušḫuššu Manso, Alce da Taiga e Carneiro Gigante da Colina já existem.)*

### 77 · Dur-Jarras, a Cidade-Zigurate / *Dur-Jarras, the Ziggurat City* — nv 300–308 · ☆ · cidade
- **Fonte:** Chogha Zanbil (Elam), zigurates e muralhas de tijolo vidrado da Mesopotâmia.
- **Bioma e visual:** cidade em volta de um zigurate de 5 degraus, canais e palmeiras; estilo `Ziggurat`, paleta acima.
- **Layout:** GG. Cada degrau do zigurate é um bairro (feira, ofícios, biblioteca, relógio, a Torre); canais em volta.
  Retorno: Dur-Jarras.
- **Inimigos (arredores):** Leão de Tijolo Apagado (os leões de tijolo vidrado dos muros) [tanque]; Mušḫuššu Jovem
  (dragão-serpente dos portões da Babilônia, como bicho) [elite]; Homem-Escorpião (*girtablullu*, guarda das montanhas
  no conto de Gilgamesh) [distância]; Gafanhoto de Betume [enxame].
- **Chefe:** nenhum na cidade; evento **Cheia** (o rio sobe: salvar as tábuas da biblioteca).
- **Mecânica:** **degraus**: subir o zigurate libera serviços; do alto se vê a Roda Celeste (mapa 94) girando no céu.
- **Ganchos:** loot tijolo vidrado, tábua de argila · montaria nova **Mušḫuššu Manso** (§6) · casa: conjunto novo **Tijolo
  Azul** (leão de azulejo).
- **No jogo (2026-11-09): montaria.** **Mušḫuššu Manso** (`mount_mushhushshu`, Épico): recompensa de `quest_city_dur_jarras_beetles_on_the_tablets` (Sin-leqi-unninni). **Rugido do Portão**: bichos comuns a até 8 m fogem por 4 s e batem 15% mais fraco; chefe não.
- **Noite:** o zigurate acende de cima para baixo; o Relógio de Zurvan marca a noite.
- **No jogo (2026-11-09): cidade de verdade** (`CityTownRecipes.DurJarras.cs`, estilo `Ziggurat`, `CityCatalog.LateRegionCities`, id `dur_jarras`), já no mundo contínuo: em y = 0 dentro de um círculo de 22,5 m (saídas O, L e S a 19 m; a rua Sul é a entrada; sem rua ao norte: o zigurate fecha a praça), zigurate de cinco degraus, Relógio de Zurvan, canais de betume com ponte de tijolo, palmeiras, leões vidrados e jarras; Utnapishtim, Atrahasis, Sin-leqi-unninni e Siduri; Betume do Atrahasis com as exigências de 78–82 fixas; respawn na praça para 77–82. Falta: os degraus que liberam serviços e o evento da Cheia.
- **No jogo (2026-11-09): conjunto de equipamento Tijolo Azul** (`ArmorSetRules`, Docs/Items.md): Peitoral de Azulejos Vidrados e Elmo do Leão de Azulejo, Defesa 35/20; 2 peças: Reflexo Vidrado (ao ser atingido, 20% de devolver 35% do dano). Só por drop dos 12 bichos próprios da Região 14 (0,5% por abate).
- **No jogo (2026-11-09): conjunto de casa **Móveis de Tijolo Azul**** (móveis, `Docs/Housing/CatalogoCasa.md`): 8 peças à venda em Betume do Atrahasis, 30–540 de ouro; Épica só como especial da semana. Estátua de leão de azulejo vidrado, Mesa de tijolo vidrado azul.

### 78 · Deserto do Relógio / *Desert of the Clock* — nv 306–316 · ☆
- **Fonte:** Zurvan, o "Tempo sem fim" dos contos persas (bíblia §1.1: só como relógio, nunca pessoa).
- **Bioma e visual:** deserto plano com gnômons gigantes caídos e sombras que andam sozinhas; areia clara, pedra, sombra
  violeta.
- **Layout:** GG. 12 gnômons em círculo; **posto: Gnômon Caído**. Retorno: Dur-Jarras.
- **Inimigos:** Ampulheta Viva [tanque]; Lagarto do Instante [rápido]; Sombra Atrasada [controle] (a sombra que chega
  depois do bicho); Ushabti de Relógio [distância].
- **Chefe:** **O Instante Perdido**, um momento que a sombra roubou do relógio; devolvido, o relógio da cidade acerta.
- **Mecânica:** **bolhas de tempo**: áreas onde tudo fica a ×0,7 ou a ×1,3 (herói e bicho igual), marcadas no chão.
- **Ganchos:** loot areia do tempo · montaria `mount_sleipnir` (Despertar IV) · casa: conjunto novo **Relógio de Sol**.
- **Noite:** os gnômons não fazem sombra; as bolhas somem; Sombras Atrasadas dobram.
- **No jogo (2026-11-09): bolhas de tempo** (`MisturaTimeBubbleRules`, `MisturaMechanicsDriver`): 11 círculos de 24–28 m no deserto, a maioria nas estradas, ×0,7 ou ×1,3 na velocidade de andar do herói e dos bichos, com anel azul (lenta) ou âmbar (rápida) no chão; somem à noite. Com montaria, o ×1,3 não acelera (a montaria ignora bônus de velocidade), só o ×0,7 pesa. Falta: acelerar também os ataques, Sombras Atrasadas dobrando à noite e o chefe.

### 79 · Poços de Betume / *Bitumen Wells* — nv 314–326 · ☆
- **Fonte:** o betume de Elam e da Mesopotâmia (cola de tijolo e de barco); Atrahasis e o barco calafetado.
- **Bioma e visual:** pântano com poças de piche e juncos; betume `#2A2420`, junco, água marrom, fogo baixo.
- **Layout:** G. Poças em rede; balsas de junco; **posto: Balsa de Junco**. Retorno: Dur-Jarras.
- **Inimigos:** Betume Vivo [controle, gruda]; Garça de Piche [distância]; Bagre Negro [tanque]; Fumaça de Piche [enxame].
- **Chefe:** **O Rei do Piche**, uma bolha enorme de betume apagado; libertado, vira o betume bom das balsas.
- **Mecânica:** **acender o piche**: Fogo Vivo (skills do Ícaro) queima poças e abre caminho por 30 s; cuidado: queima
  todo mundo perto.
- **Ganchos:** loot betume, junco calafetado · montaria `mount_jhilpur_buffalo` (Despertar) · casa: conjunto novo
  **Balsa de Junco**.
- **Noite:** as poças acesas iluminam; o Betume Vivo sai mais.
- **No jogo (2026-11-09): acender o piche** (`MisturaTarRules`, `MisturaMechanicsDriver`): 9 poças de 8–11 m (6 nas estradas do sul), que deixam o herói a ×0,55 (só atrasam, não bloqueiam). Qualquer skill de fogo do herói acende as poças que alcança: pavio de 3,5 s com aviso e passo normal, 5 s em chamas (3 % da vida do herói por segundo, nunca mata; 4 % nos bichos, e a morte conta para o herói) e 25 s de chão queimado: 30 s de caminho aberto a partir da primeira chama, e depois o piche volta. Falta: Betume Vivo saindo mais à noite e o chefe.

### 80 · Cedral de Hobabish / *Hobabish Cedar Forest* — nv 322–336 · ☆
- **Fonte:** Humbaba (Huwawa), guardião da floresta de cedros no conto de Gilgamesh; no *Livro dos Gigantes* maniqueu ele
  aparece como **Hobabish**. Os "sete mantos de brilho" que o protegiam.
- **Bioma e visual:** floresta de cedros gigantes na montanha; cedro `#3E5A3A`, resina dourada `#D9A23A`, rocha, névoa.
- **Layout:** GG. Cedros em anéis com 7 lanternas de resina; **posto: Clareira do Lenhador**. Retorno: Dur-Jarras.
- **Inimigos:** Cedro Raivoso [tanque]; Lince do Cedro [rápido]; Abelha-Resina [enxame]; Manto de Brilho [controle].
- **Chefe:** **Hobabish, o Guardião do Cedro**: ficou apagado e acha que todo mundo quer cortar a floresta; com as 7
  lanternas de resina acesas, ele se acalma, e os primos **prometem** não cortar (missão de não cortar árvore nenhuma no
  mapa). Volta a ser guardião.
- **Mecânica:** **os sete mantos**: cada lanterna acesa tira um manto do chefe (fica menos protegido).
- **Ganchos:** loot resina de cedro (cola de refino) · montaria nova **Alce da Taiga** (variante cedro) · casa: conjunto
  novo **Cedral** (baú de cedro).
- **No jogo (2026-11-09): montaria.** **Alce da Taiga** (`mount_taiga_elk`, Incomum): apito por 150 de ouro em Dur-Jarras (Betume do Atrahasis). **Passo do Cedral** tira e ignora lentidão por 8 s. Falta: a variante cedro (só o modelo base).
- **Noite:** a resina brilha; os cedros "respiram" névoa.

### 81 · Ombro de Upelluri / *Upelluri's Shoulder* — nv 334–346 · ☆
- **Fonte:** Upelluri, o gigante hurrita que carrega céu e terra no ombro, sem perceber nada; Ullikummi, o gigante de
  diorito que cresceu no ombro dele (*Canção de Ullikummi*).
- **Bioma e visual:** o mapa é o ombro de um gigante enorme coberto de terra e pedra; pedra, musgo, céu; "montanhas" que
  são dobras de pele de pedra.
- **Layout:** GG. O ombro inclina e volta; a pedra de diorito cresce no meio; **posto: Ninho no Ombro**. Retorno:
  Dur-Jarras.
- **Inimigos:** Pedra de Diorito [tanque]; Ave do Ombro [rápido]; Tremor [controle]; Filho de Ullikummi [elite].
- **Chefe:** **Ullikummi, o Que Cresce**: a pedra que cresce sem parar para tapar o céu; vencida, para de crescer e vira
  montanha (a Torre das Jarras tinha um pedaço dele).
- **Mecânica:** **o ombro respira**: a cada 60 s o terreno inclina (tudo escorrega devagar para um lado); usar as pedras
  como apoio.
- **Ganchos:** loot diorito · montaria `mount_woolly_mammoth` (Despertar IV) · casa: troféu de Ullikummi (uma lasca).
- **Noite:** o gigante "dorme" e não inclina; Ullikummi cresce mais.
- **No jogo (2026-11-09): o ombro respira** (`MisturaShoulderRules`, `MisturaMechanicsDriver`): a cada 60 s, 5 s de aviso e 10 s inclinando (norte numa respiração, sul na outra, para o herói não sair da coluna do ombro nem acumular deriva): 1,8 m/s parado, ×0,4 andando (no máximo 18 m), sem dano; a 5 m de uma das 24 pedras de diorito (anel que acende no aviso) o herói não escorrega. À noite não inclina; fora do ombro e no Ninho nada acontece. Falta: Ullikummi crescendo.

### 82 · Campo das Tábuas do Sonho / *Field of the Dream Tablets* — nv 344–360 · ☆
- **Fonte:** no *Livro dos Gigantes*, Ohya e Hahya sonham com uma tábua lavada na água e um jardim de árvores cortadas,
  e procuram quem explique o sonho (bíblia §1.4).
- **Bioma e visual:** campo de tábuas de pedra gigantes fincadas no chão, jardim cortado, água parada; pedra, água,
  troncos.
- **Layout:** GG. Duas versões do mesmo mapa: **acordado** e **sonho** (trocadas na tenda); **posto: Tenda dos
  Intérpretes**. Retorno: Dur-Jarras.
- **Inimigos:** Tábua Quebrada [distância]; Jardim-Sombra (as árvores cortadas do sonho) [tanque]; Pássaro-Sonho [rápido];
  Água que Apaga [controle].
- **Chefe:** **chefe do mundo: o Sonho da Destruição**, a sombra que nasceu do medo dos gigantes; o Hahya e o Ohya (já
  libertados) ajudam na luta. Vencido, o sonho termina bem.
- **Mecânica:** **acordar e sonhar**: dormir na tenda leva para a versão sonho; mudar uma coisa no sonho muda o acordado
  (ponte que aparece, água que some).
- **Ganchos:** loot tábua de sonho (conjunto antigo "Gigantes") · montaria nova **Carneiro Gigante da Colina** (Despertar)
  · casa: conjunto novo **Tábuas do Sonho**.
- **No jogo (2026-11-09): montaria.** **Carneiro Gigante da Colina** (`mount_hill_ram`, Incomum): apito por 150 de ouro em Dur-Jarras (Betume do Atrahasis). **Cabeçada de Pedra**: investe 9 m, joga bichos comuns de lado e quebra segredos no caminho.
- **Noite:** só dá para sonhar à noite; de dia a tenda fica fechada.
- **No jogo (2026-11-09): chefe do mundo: o Sonho da Destruição** (`enemy_mountain_dream_of_destruction`, nv 360): arena no anel de pedras ao norte das Tábuas do Sonho (1640, 980 na região); três fases, com rajadas de tábua. Própria: **o sonho ao contrário** (a cada 11 s, da 2ª fase, aviso de 2,2 s: anel de 2,5–7 m em volta de onde o herói estava; quem ficou parado ou correu longe escapa). Bônus do chefe: peitoral do conjunto Tijolo Azul (o conjunto "Gigantes" ainda não existe). Loot próprio (2026-11-11): **Tábua do Sonho** (`item_chefe_tabua_sonho`, épica, 40% por vitória, 500 de venda; no lugar do punhado de ouro, o loot do bioma segue igual); vai no Tear/Forja da casa (`HouseCraftRules`): Grevas do tronco milenar do Joaquim e Saia do horizonte apagado da Lavine (Tear de mestre). Troféu de casa (2026-11-11): **Painel das Tábuas do Sonho** (`casa_trofeu_034`, épico, parede), feito na Forja da casa com 2 Tábua do Sonho e o chefe já vencido (`HouseCraftRules.WorldBossTrophies`). Falta: Hahya e Ohya lutando junto e a versão sonho do mapa.

### Região 15 — Limiar Profundo (as Cinco Covas da Treva) · mapas 83–90 · ☆

Debaixo das Grutas do Limiar ficam as **cinco covas** da Terra da Treva, uma para cada elemento escuro, cada uma com o seu
tipo de bicho: **Fumaça** (os de duas pernas), **Fogo Devorador** (os de quatro patas), **Vento Destruidor** (os que voam),
**Lodo** (os que nadam) e **Treva** (os que rastejam) (Teodoro bar Konai, *Scholion* XI; Kephalaia). Tom: frio, pegajoso,
barulhento, nunca gore (bíblia §4.3). Cidade: **Fortaleza da Última Lâmpada**, dos guardiões das lâmpadas, irmã da Vila
das Lamparinas. Estilo novo `BasaltFort`. Paleta: basalto `#2C2C33`, âmbar `#FFB347`, cobre `#B06B3A`, branco de roupa
`#F2EEE6`, índigo `#232046`. NPCs com nomes das **cartas de Kellis** (famílias maniqueias do oásis de Dakhla, séc. IV):
**Pamour** (o comandante das lâmpadas), **Tehat** (a tecelã de pavios), **Makarios** (o pai que escreve cartas para os
filhos longe, eco das cartas do Vô Jorge). Raide: **A Grande Treva** (mapa 90).

**No jogo (2026-11-09): mundo contínuo** (`OverworldLayouts.Profundo.cs`, §0.10): 2400 × 2400 m ao norte dos Picos de Farnkesh (preenche o último buraco murado; 15 regiões, mundo sem buracos; duas passagens abertas na borda oeste levam às trilhas leste das Margens da Mistura, nv 344–360),
25 áreas em 25 terrenos novos — Fortaleza, Muralha em Arco, Nichos de Basalto, Cisternas de Óleo, Varais de Pavios e Beira do Abismo (83, cidade, nv 340–350); Cova da Fumaça com a Lâmpada 1, Ventiladores dos Artífices e Corredores de Cinza (84, posto; **Teriaga**, nv 348–362); Cova do Fogo Devorador com a Lâmpada 2,
Chão que Acende e Salões de Cinza Quente (85, posto; **Óleo de Fogo Eterno**, nv 360–374); Cova do Vento Destruidor com a Lâmpada 3 (poço de anéis em degraus), Plataformas Soltas e Correntes Ascendentes (86, posto; **Asas de Lona**, nv 372–388); Cova do Lodo com a Lâmpada 4, Salões Alagados e Comportas dos Artífices (87, posto; **Bolha de Ar**, nv 386–402; lagoas de água preta);
Cova da Treva com a Lâmpada 5 e Túneis Cegos (88, posto; **Lamparina Rúnica**, nv 400–416, labirinto de paredes); Cabeceira da Ponte, Ponte do Novelo com o Pilar do Meio e Lances do Novelo (89, posto; **Vara de Equilíbrio**, nv 412–428: uma calçada elevada reta com dez pilares e o globo escuro do Novelo ao lado);
Praça dos Pedestais com o Selo dos Artífices e cinco pedestais de lâmpada, e Portão da Terra da Treva (90, posto; **Lamparina de Óleo**, nv 424–440, com o portão gigante). Todas as exigências estão no balcão fixo dos Pavios da Tehat. Bichos existentes nos papéis das fichas (mordedor de sombra = Sombra Pequena, névoa do vento ruim = Fumaça/Redemoinho, morcego vampiro = Morcego da Treva, ushabti de laca = Ushabti Apagado, almas = Sombra-Cópia,
gigante sonâmbulo = Gigante de Fumaça, cabra de fumaça = Fumaça Sufocante, soldado de terracota = Arqueiro de Fumaça, lobo de bronze = Lobo de Brasa, iaque = Touro de Cinza, leão de pedra = Leão de Fumaça Quente, enxame de vaga-lumes = Chama Faminta, águia da tempestade = Águia Negra, morcego-vela = Morcego-Lâmina, harpia de fumaça = Harpia de Vento, peixe-lâmina = Peixe da Treva, cobra do junco = Enguia Grande,
gosma = Polvo de Lodo, peixe-lanterna = Cardume Escuro, píton apagada = Serpente Cega, lagarto de lava gigante = Lagarto-Sombra, treva rastejante = Rastejante, serpente de três cabeças = Dragão-Sombra Menor, orc coroado = Arconte Menor, golem de pedra = Ushabti Apagado Gigante), variante noturna em toda clareira, nenhum chefe (Rei da Fumaça, Leoa, Águia de Asas de Treva, Peixe de Cauda Negra, Dragão do Fundo, Guardião do Novelo e A Grande Treva ficam para depois; desde 2026-11-09 A Grande Treva, versão solo, já é o chefe do mundo, no 90). Testes `OverworldProfundoTests` e `OverworldWorldTests`. Mecânicas da região em `Mechanics/Profundo*` (driver `ProfundoMechanicsDriver`, teste `ProfundoMechanicsTests`): rede de lâmpadas e Maré de Treva (83), vento que limpa a fumaça (84) e chão que acende (85); ver "No jogo" de cada ficha. Falta: correntes de ar, comportas e nível da água, dano no escuro, ponte que acende por lances, os 5 portões selados e as montarias.

### 83 · Fortaleza da Última Lâmpada / *Fortress of the Last Lamp* — nv 340–350 · ☆ · cidade
- **Fonte:** a borda da Terra da Treva; cartas de Kellis (nomes); a Lâmpada-Mãe da Parthenos (bíblia §3.10).
- **Bioma e visual:** fortaleza escavada no basalto com mil nichos de lâmpada; estilo `BasaltFort`.
- **Layout:** GG. Muralha em arco sobre o abismo; a **Lâmpada-Mãe Grande** no centro; 5 portões, um para cada cova.
  Retorno: Fortaleza da Última Lâmpada.
- **Inimigos (fora dos portões):** Sombra Pequena [rápido]; Fumaça [controle]; Morcego da Treva [enxame]; Ushabti Apagado
  [tanque].
- **Chefe:** nenhum na cidade; evento **Maré de Treva** (a sombra sobe; manter as lâmpadas da muralha acesas).
- **Mecânica:** **rede de lâmpadas**: lâmpadas acesas pelos jogadores aumentam a zona segura da fortaleza (até 3 anéis);
  apagadas, ela encolhe.
- **Ganchos:** loot pavio eterno, óleo de luz · montaria `mount_carbuncle` (Raposa-Joia, Despertar IV) · casa: conjunto
  novo **Mil Lâmpadas**.
- **Noite:** igual (sem céu); à "noite do relógio" a Maré de Treva é mais forte.
- **No jogo (2026-11-09): cidade de verdade** (`CityTownRecipes.UltimaLampada.cs`, estilo `BasaltFort`, id `fortaleza_ultima_lampada`), já no mundo contínuo: em y = 0 dentro de um círculo de 22,5 m (saídas O, L e S, a S é a entrada; sem rua ao norte: os cinco portões selados fecham a praça), Lâmpada-Mãe Grande, Pamour, Tehat e Makarios; Pavios da Tehat com as exigências de 84–90 fixas; respawn na praça para 83–90.
- **No jogo (2026-11-09): rede de lâmpadas e Maré de Treva** (`ProfundoLampRules`): 12 lâmpadas em 3 anéis de 4 em volta da fortaleza (o anel de dentro já começa aceso); ficar 2 s a até 3,5 m acende; um anel vale com 3 de 4 acesas e os de dentro valendo. Zona segura a partir da praça: 35 / 80 / 130 / 190 m com 0 / 1 / 2 / 3 anéis, e cada lâmpada acesa protege 10 m em volta. A Maré vem a cada 240 s: 180 s de calma, 20 s de aviso (texto e frente de sombra fechando), 40 s subindo, até 320 m da praça (só o mapa 83): apaga uma lâmpada a cada 8 s, de fora para dentro (5 s à noite do relógio) e, fora da luz, tira 2% da vida máxima a cada 2 s (3% à noite), sem passar de 30% da vida; a primeira mordida só avisa. Terminando com um anel de pé, a Lâmpada-Mãe devolve 20% da vida. A praça nunca é atingida.
- **No jogo (2026-11-11): os 5 portões das covas** (`ProfundoGateRules`, pontos em `OverworldLayouts.ProfundoMechanics.cs`, driver novo `ProfundoGateMechanicsDriver`, teste `ProfundoGateMechanicsTests`): os portões da muralha (150°, 120°, 90°, 60°, 30°: Fumaça, Fogo Devorador, Vento Destruidor, Lodo, Treva) estão selados do outro lado; chegar a pé a até 20 m da lâmpada-posto da cova (84–88) rompe o selo para sempre (`WorldMapProgress.SetTimeMark`, chave `profundo.gate.N`). Portão aberto: [E] (ou "Usar") a até 3,5 m na praça leva ao caminho do posto, 24 m fora dele, e de lá (4 m) volta para a frente do portão; a pé seriam ~600 (Fumaça), ~950 (Fogo), ~1080 (Lodo), ~1250 (Vento) e ~1900 m (Treva). Enquanto a Maré de Treva sobe (40 s de cada 240 s) os guardiões trancam todos. Cada passagem: o bafo da cova tira 3% da vida máxima (4% à noite do relógio), nunca abaixo de 30%; saindo na cova, a lâmpada do portão tira 8% dos bichos a até 10 m (chefe 1%). 3 s entre passagens; a zona de chegada tem de estar aberta para o herói. Coluna na cor do elemento quando aberto, cinza baixa quando selado. Falta: abrir o portão com animação (hoje o herói só aparece do outro lado).
- **No jogo (2026-11-09): conjunto de casa **Móveis das Mil Lâmpadas**** (móveis, `Docs/Housing/CatalogoCasa.md`): 8 peças à venda em Pavios da Tehat, 30–600 de ouro; Épica só como especial da semana. Lustre de mil pavios, Nicho de lâmpada de basalto, Lamparina de pavio eterno.

### 84 · Cova da Fumaça / *Pit of Smoke* — nv 348–362 · ☆
- **Fonte:** a Fumaça, 1º mundo da Treva, dos que andam em duas pernas.
- **Bioma e visual:** caverna cheia de fumaça cinza que se mexe; cinza `#7A7680`, preto, âmbar das lâmpadas.
- **Layout:** G. Corredores de fumaça; ventiladores antigos dos Artífices; **posto: Lâmpada 1**. Retorno: Fortaleza.
- **Inimigos:** Sombra-Cópia [rápido, imita o herói]; Gigante de Fumaça [tanque]; Fumaça Sufocante [controle]; Arqueiro de
  Fumaça [distância].
- **Chefe:** **O Rei da Fumaça**, figura de duas pernas feita de fumaça que copia quem chega; vencido com vento, vira ar.
- **Mecânica:** **vento limpa**: ligar os ventiladores (ou skills de Vento Vivo, Garrafas de Ar Sereno) limpa a fumaça por
  30 s.
- **No jogo (2026-11-09): vento limpa** (`ProfundoSmokeRules`): 8 bancos de fumaça nas estradas das três áreas (24–26 m de raio), cada um com um ventilador dos Artífices fora da borda; na fumaça o herói anda a 80% e perde 2% da vida máxima a cada 1,5 s, sem passar de 35% da vida. Ficar 1,5 s a até 4 m do ventilador, ou lançar qualquer habilidade de Vento que alcance o banco, limpa por 30 s, com aviso 4 s antes de voltar. Garrafas de Ar Sereno ainda não entram.
- **Ganchos:** loot cinza de fumaça (Pedra da Mistura) · montaria nenhuma · casa: troféu do Rei da Fumaça.
- **Noite:** igual.

### 85 · Cova do Fogo Devorador / *Pit of Devouring Fire* — nv 360–374 · ☆
- **Fonte:** o Fogo Devorador, 2º mundo da Treva, dos de quatro patas.
- **Bioma e visual:** caverna de pedra quente com chão que acende em desenho; brasa, preto, laranja.
- **Layout:** G. Salões ligados; o chão acende em padrões; **posto: Lâmpada 2**. Retorno: Fortaleza.
- **Inimigos:** Lobo de Brasa [rápido]; Touro de Cinza [tanque]; Leão de Fumaça Quente [elite]; Chama Faminta [enxame].
- **Chefe:** **A Leoa do Fogo Devorador**, que come o fogo bom; vencida, vira brasa mansa.
- **Mecânica:** **chão que acende**: o padrão do chão avisa 1,5 s antes; ficar nas pedras escuras.
- **No jogo (2026-11-09): chão que acende** (`ProfundoFloorRules`): em toda a área Chão que Acende (480 × 480 m) o piso é uma grade de pedras de 8 m; ciclo de 9 s: 5 s quieto, 1,5 s de aviso (pedras em âmbar) e 2,5 s queimando (vermelho), em quatro desenhos que se alternam, sempre com as quatro vizinhas de cada pedra acesa escuras. Na pedra acesa, 4% da vida máxima a cada 0,75 s, sem passar de 25% da vida.
- **Ganchos:** loot brasa devoradora · casa: troféu da Leoa.
- **Noite:** igual.

### 86 · Cova do Vento Destruidor / *Pit of Ruinous Wind* — nv 372–388 · ☆
- **Fonte:** o Vento Destruidor, 3º mundo da Treva, dos que voam.
- **Bioma e visual:** poço enorme com plataformas e correntes de ar para cima; cinza-azul, preto.
- **Layout:** G (vertical). Plataformas soltas; **posto: Lâmpada 3**. Retorno: Fortaleza.
- **Inimigos:** Águia Negra [rápido]; Morcego-Lâmina [enxame]; Harpia de Vento [distância]; Redemoinho [controle].
- **Chefe:** **A Águia de Asas de Treva**: as asas de águia são uma das cinco formas do Príncipe (bíblia §1.4), mas esta é
  uma ave separada, que ficou com as asas dele; vencida, perde as asas escuras e vira ave comum.
- **Mecânica:** **correntes**: pular nas correntes de ar para subir; voadores sobem sem elas.
- **Ganchos:** loot pena de treva · casa: troféu da Águia.
- **Noite:** igual.
- **No jogo (2026-11-11): correntes** (`ProfundoUpdraftRules`, pontos em `OverworldLayouts.ProfundoMechanics.cs`, driver novo `ProfundoPathsMechanicsDriver`, teste `ProfundoPathsMechanicsTests`): 4 correntes a 30 m da Lâmpada 3 sobem o herói a 26 m/s até o anel de fora (Plataformas Soltas e Correntes Ascendentes), 340–410 m em 13–16 s, contra 62–75 s subindo os terraços ou 550–600 m pelas estradas; [E] ou "Usar"; 3 s entre subidas; a Maré de Treva desliga as correntes; as harpias bicam 2% da vida a cada 3 s no ar (3% à noite, nunca abaixo de 30%) e a rajada do pouso tira 6% dos bichos a até 9 m. Falta: voadores subirem sem elas, a Águia de Asas de Treva.

### 87 · Cova do Lodo / *Pit of Mire* — nv 386–402 · ☆
- **Fonte:** o Lodo (a água turva), 4º mundo da Treva, dos que nadam.
- **Bioma e visual:** caverna alagada de água preta; preto-verde, bolhas, âmbar.
- **Layout:** G. Salões alagados em níveis; **posto: Lâmpada 4**. Retorno: Fortaleza.
- **Inimigos:** Peixe da Treva [rápido]; Enguia Grande [controle]; Polvo de Lodo [tanque]; Cardume Escuro [enxame].
- **Chefe:** **O Peixe de Cauda Negra** (a cauda de peixe é outra das cinco formas do Príncipe); vencido, vira peixe-lanterna.
- **Mecânica:** **nível da água**: comportas dos Artífices sobem e descem a água; Bolhas de Ar para os trechos fundos.
- **Ganchos:** loot lodo condensado (Pedra da Água Viva ao purificar) · casa: troféu do Peixe.
- **Noite:** igual.
- **No jogo (2026-11-11): nível da água** (`ProfundoSluiceRules`, pontos em `OverworldLayouts.ProfundoMechanics.cs`, driver `ProfundoPathsMechanicsDriver`, teste `ProfundoPathsMechanicsTests`): 2 trechos fundos de 56 m atravessam a coluna inteira (Salões Alagados e Comportas, nas estradas da Lâmpada 4); com a água alta o herói anda a −50% e, depois de 4 s de Bolha de Ar, a água preta morde 2% da vida a cada 1,5 s (3% à noite, nunca abaixo de 30%): ~20 s e ~22% da vida por trecho; a alavanca da comporta (195 m a leste da estrada, ~390 m de desvio) baixa a água até o fim do dia do jogo (marca diária `profundo.sluice.<id>`) e a correnteza tira 6% dos bichos do trecho. Falta: o Peixe de Cauda Negra.

### 88 · Cova da Treva / *Pit of Darkness* — nv 400–416 · ☆
- **Fonte:** a Treva, 5º mundo, dos que rastejam (corpo de dragão do Príncipe).
- **Bioma e visual:** escuro total; só as lâmpadas e a Luz da Maria mostram as paredes; preto, âmbar, dourado.
- **Layout:** G. Labirinto de túneis; **posto: Lâmpada 5**. Retorno: Fortaleza.
- **Inimigos:** Serpente Cega [rápido]; Lagarto-Sombra [tanque]; Rastejante [enxame]; Dragão-Sombra Menor [elite].
- **Chefe:** **O Dragão do Fundo**, o corpo de dragão do Príncipe que se soltou; vencido, a sombra dele recua e sobra um
  lagartinho de luz que segue os primos (pet cosmético).
- **Mecânica:** **escuro**: bichos no escuro recebem −30% de dano; iluminados (lanterna, Luz da Maria, skills de luz),
  dano normal. É um jogo de luz, não de força (cabe no orçamento: o modelo mede com a luz ligada).
- **Ganchos:** loot escama de treva · casa: troféu do Dragão.
- **Noite:** igual.

### 89 · Ponte do Novelo / *Bridge of the Bōlos* — nv 412–428 · ☆
- **Fonte:** o *bōlos*, o "novelo" ou globo em que a Treva fica fechada no fim do conto (Kephalaia; bíblia §1.5).
- **Bioma e visual:** ponte de luz atravessando um abismo de treva até um globo escuro enorme; dourado, preto, lilás.
- **Layout:** GG (comprido). A ponte em 10 lances; o pilar do meio; **posto: Pilar do Meio**. Retorno: Fortaleza.
- **Inimigos:** os cinco tipos das covas misturados [todos os arquétipos].
- **Chefe:** **O Guardião do Novelo**, ushabti antigo dos Artífices que segura o novelo fechado e está cansado; os primos
  o ajudam (não é luta contra ele: é luta **ao lado dele** contra as ondas que saem do novelo).
- **Mecânica:** **a ponte acende**: cada lance só aparece quando as faíscas do lance anterior são libertadas.
- **Ganchos:** loot fio de novelo · casa: conjunto novo **Novelo** (globo de enfeite).
- **Noite:** igual.
- **No jogo (2026-11-11): a ponte acende** (`NoveloBridgeRules`, gaiolas nos pilones em `OverworldLayouts.ProfundoMechanics.cs`, driver `ProfundoPathsMechanicsDriver`, teste `ProfundoPathsMechanicsTests`): 10 lances de 145 m, o 1º aceso; 9 gaiolas de faíscas, uma no pilone de cada lance; [E] e ficar a até 6 m por 5 s (o sopro do Novelo tira 3% da vida a cada 1,5 s, 4% à noite, nunca abaixo de 30%) acende o lance seguinte para sempre (marca de tempo `profundo.novelo.flight.N`) e queima 8% dos bichos a até 10 m; a treva dos lances apagados (a coluna leste inteira, menos os 40 m do Pilar do Meio) empurra o herói de volta a 7 m/s. Ponte acesa: 1425 m da cabeceira ao Selo; a volta pela coluna do Lodo, ~1900 m. Falta: o Guardião do Novelo.

### 90 · Portão da Terra da Treva / *Gate of the Land of Darkness* — nv 424–440 · ☆
- **Fonte:** o selo dos Artífices na porta da Terra da Treva (bíblia §2.4); o Príncipe das Trevas, com cinco formas juntas
  (cabeça de leão, asas de águia, corpo de dragão, cauda de peixe, pés de demônio), **só em silhueta** (Teodoro bar
  Konai; Kephalaia 6).
- **Bioma e visual:** portão gigante de pedra rachada com o selo; atrás, só escuro; lâmpadas dos Artífices em fila.
- **Layout:** GG. Praça diante do portão com 5 pedestais de lâmpada; **posto: Selo dos Artífices**. Retorno: Fortaleza.
- **Inimigos:** Filhos da Treva (as cinco covas) [todos]; Arconte Menor [elite]; Ushabti Apagado Gigante [tanque].
- **Chefe:** **raide do servidor: A Grande Treva** (bíblia §1.4, §6 decisão 7). O Príncipe **nunca morre**: 5 grupos
  acendem as 5 lâmpadas ao mesmo tempo e o selo é refeito; ele **recua por uma temporada**. A temporada seguinte começa
  com o selo rachando de novo.
- **Mecânica:** **5 lâmpadas juntas**: a raide só avança com as 5 acesas no mesmo minuto (coordenação de servidor).
- **Ganchos:** loot conjunto antigo "Selo dos Artífices", título de temporada · montaria nova **Cavalo-Estrela da Coluna**
  (lendária, recompensa de temporada, §6) · casa: troféu do Selo.
- **Noite:** igual.
- **No jogo (2026-11-09): conjunto antigo Selo dos Artífices** (`ArmorSetRules`, Docs/Items.md): Cota dos Selos Rompidos e Elmo do Último Selo, Defesa 39/22; 2 peças: Selo Reaberto (no crítico, 25% de devolver 8% da mana). Por enquanto vem do drop dos 12 bichos próprios da Região 15 (0,5% por abate), porque a raide A Grande Treva ainda não existe.
- **No jogo (2026-11-09): chefe do mundo: A Grande Treva, versão solo** (`enemy_mountain_great_dark`, nv 440): arena no anel de pedras diante do Portão (1400, 1000 na região); três fases, com rajadas de brasa. Própria: **as cinco formas** (troca a cada 9 s: leão ×1,3, águia +2 tiros, dragão ×1,15, peixe ×0,85, demônio ×1,2). Só em silhueta; não morre, recua atrás do selo e volta em 5 min. O bônus do chefe é o peitoral do conjunto Selo dos Artífices. Loot próprio (2026-11-11): **Lasca do Selo da Treva** (`item_chefe_lasca_selo`, lendária, 30% por vitória, 700 de venda; no lugar do punhado de ouro, o loot do bioma segue igual); vai no Tear/Forja da casa (`HouseCraftRules`): Pernas mecânicas assistidas do Ícaro e Saia do horizonte apagado da Lavine (Tear de mestre). Troféu de casa (2026-11-11): **Estátua do Selo da Treva remendado de luz** (`casa_trofeu_035`, lendário, piso), feito na Forja da casa com 2 Lasca do Selo e o chefe já vencido (`HouseCraftRules.WorldBossTrophies`). Falta a raide de verdade (5 grupos, 5 lâmpadas no mesmo minuto, recuo por temporada, título) e o Cavalo-Estrela: precisa de servidor.

### Região 16 — Reinos do Céu (as Barcas, a Roda e a Coluna) · mapas 91–100 · ☆

A luz libertada sobe pela **Coluna da Glória** e navega nas **Barcas do Sol e da Lua** (bíblia §1.5). Aqui o jogo fica
quieto e bonito: menos bichos, mais luz, a família inteira junta. Cidade: **Cais da Coluna**, onde as barcas encostam.
Estilo novo `SkyQuay` (píeres de luz sólida, pavilhões de vidro e ouro sobre nuvens). Paleta: branco `#FAF7EF`, ouro
`#E8C35A`, lilás `#C7B6EE`, azul-céu `#BFD9F2`, as cinco cores dos elementos. NPCs: **Ammo, o Caravaneiro** (a última
caravana dele chega aqui), **Adda** (aposentado da Torre, cuida da Roda Celeste) e os Remadores (silhuetas de luz, sem
fala). O Pai da Grandeza **não aparece** (bíblia §0.2). Raide: **A Grande Treva** (porta no 100).

**No jogo (2026-11-09): mundo contínuo** (`OverworldLayouts.Ceu.cs`, §0.10): 2400 × 3600 m ao norte do Limiar Profundo, a sexta fileira do mundo (as duas células a oeste ficam muradas; 16 regiões; duas passagens abertas na borda sul levam à trilha do Portão da Terra da Treva, nv 432–440), 35 áreas em 35 terrenos novos — Beira Oeste, Píer da Lua, Cais, Píer do Sol e Fresta das Luzinhas (91, cidade, nv 420–430); Foz Oeste, Ilhas de Nuvem, Rio de Luz com a Boia de Luz, Margens da Corrente e Foz Leste (92, posto, nv 428–440; um rio de luz de verdade, largura 10, três pontes); Proa, Mastro, Convés com a Cabine da Proa, Porão e Popa da Barca da Lua (93, posto, nv 436–450; convés em 3 níveis, mastro grande);
Engrenagens do Oeste e do Leste, Jarras do Oeste e do Leste e Roda Celeste com o Eixo da Roda (94, posto; **Corda de Escalada**, nv 446–460; aro elevado e doze jarras do tamanho de casas); Órbita Vazia do Oeste e do Leste, Órbitas do Oeste e do Leste e Esferas dos Arcontes com a Ilhota Parada (95, posto; **Asas de Lona**, nv 456–470; sete esferas de cristal); Jardim das Flores, Pomar de Luz e Pérgola do Jardineiro (96, posto, nv 466–478); Convés com a Cabine da Popa e Sob a Vela do Sol (97, posto, nv 474–486; vela triangular); Degraus da Coluna e Pés da Coluna da Glória com o Degrau Um (98, posto, nv 482–492; anéis de degraus e o pilar de luz);
Rachaduras do Oeste e Mirante do que Segura o Céu com a Mão Aberta (99, posto; **Capa de Tempestade**, nv 488–498; cinco rachaduras e pilares de luz) e a Varanda do Tempo de Depois com a cadeira de balanço vazia do Vô Jorge (100, posto, nv 495–500). As três exigências são fixas na Última Caravana do Ammo; as fichas não pedem nada, as exigências de 94, 95 e 99 são decisão de jogo (a corda das jarras, as asas de esfera em esfera, a capa do vento do alto), e os mapas 91–93 e 96–98/100 não pedem nada. Bichos: os doze próprios da região nos papéis que as fichas citam e, no resto, bichos já existentes; variante noturna em toda clareira, nenhum chefe (O Remador Perdido, O Clandestino Grande, O Arconte da Roda, Saklas e Namrael, Āz, A Sombra da Vela, O Último Arconte do Degrau e A Rachadura Grande ficam para depois; desde 2026-11-09 Āz, a Capitã Cobiçosa, já é o chefe do mundo, no 96); as Sombras de Saudade do 100 não se lutam (ficam para a mecânica de sentar). Testes `OverworldCeuTests` e `OverworldWorldTests`. Mecânicas prontas (`Mechanics/CeuMechanicsDriver.cs`, regras puras `CeuMoonBarqueRules`, `CeuSkyHoldRules` e `CeuPorchRules`, testes `CeuMechanicsTests`): Barca da Lua só à noite (91 → 93), segurar o céu (99) e sentar na Varanda (100). Falta: pouca gravidade, andar sobre o rio com correnteza, lua crescendo e lanternas, jarras e esferas que giram, chão de plantar luz, carregar lanternas entre as barcas, faíscas escoltadas, a Chegada da Caravana, as Sombras de Saudade e as montarias.

### 91 · Cais da Coluna / *Column Quay* — nv 420–430 · ☆ · cidade
- **Fonte:** as Barcas de Luz que levam as faíscas para a Coluna da Glória (Kephalaia; *Acta Archelai* 8).
- **Bioma e visual:** cidade sobre nuvens com píeres de luz; estilo `SkyQuay`; as 27 cidades da ilha vistas lá embaixo
  como luzinhas.
- **Layout:** GG. Píer do Sol a leste, píer da Lua a oeste; praça redonda com cinco fontes (uma por elemento). Retorno:
  Cais da Coluna.
- **Inimigos (bordas):** Arconte Fujão Menor [elite]; Faísca Confusa [enxame, não ataca, foge]; Ave de Nuvem Apagada
  [rápido]; Sombra Clandestina [controle] (sombra que se escondeu numa barca).
- **Chefe:** nenhum; evento **Chegada da Caravana** (a última caravana do Ammo: trazer presentes para os NPCs de todas as
  cidades).
- **Mecânica:** **pouca gravidade**: pulo 1,5× mais alto; quedas lentas. G5 de todos os primos começa aqui.
- **Ganchos:** loot luz sólida · montaria `mount_pegasus` (Tulpar, Despertar IV) · casa: conjunto novo **Cais de Luz**.
- **Noite:** a Barca da Lua encosta; o céu vira de estrelas por baixo também.
- **No jogo (2026-11-09): cidade de verdade** (`CityTownRecipes.CaisDaColuna.cs`, estilo `SkyQuay`, id `cais_da_coluna`), já no mundo contínuo: em y = 0 dentro de um círculo de 22,5 m (saídas O, L e S, a S é a entrada; sem rua ao norte: o pé da Coluna fecha a praça), as cinco fontes dos elementos, as Barcas do Sol e da Lua atracadas, Ammo e Adda; Última Caravana do Ammo com o equipamento das três exigências fixo; o Cais da Barca (`cais_da_barca`) acrescentado na receita do mundo; respawn na praça para 91–100. **Barca da Lua só à noite:** no Píer da Lua (estrada oeste, x −460), das 20 h às 5 h do relógio do jogo, [E] leva em 2,5 s ao convés (mapa 93, pede o acesso à zona); de dia a fala diz quantas horas faltam, e a volta do convés sai a qualquer hora. Falta: a pouca gravidade e o evento da Chegada da Caravana.
- **No jogo (2026-11-09): conjunto de equipamento Cais de Luz** (`ArmorSetRules`, Docs/Items.md): Couraça de Luz Sólida e Diadema dos Remadores de Luz, Defesa 43/25; 2 peças: Raio da Coluna (no crítico, 15% de atingir até 4 inimigos a 5 m com 40% do dano). Só por drop dos 12 bichos próprios da Região 16 (0,5% por abate).
- **No jogo (2026-11-09): conjunto de casa **Móveis do Cais de Luz**** (móveis, `Docs/Housing/CatalogoCasa.md`): 8 peças à venda em A Última Caravana do Ammo, 30–680 de ouro; Épica só como especial da semana. Coluna de luz sólida, Cama de nuvem firme, Barquinha dos remadores de luz.

### 92 · Rio de Luz / *River of Light* — nv 428–440 · ☆
- **Fonte:** o rio de luz por onde as barcas navegam (bíblia §3.16).
- **Bioma e visual:** um rio de luz dourada correndo entre nuvens; margens de nuvem firme.
- **Layout:** GG (comprido). O rio tem correntes; ilhas de nuvem; **posto: Boia de Luz**. Retorno: Cais da Coluna.
- **Inimigos:** Peixe de Luz Apagado [rápido]; Sombra Nadadora [controle]; Corrente Errada [controle]; Lanterna Fugitiva
  [enxame].
- **Chefe:** **O Remador Perdido**, um remador de luz que a sombra fez esquecer o caminho; libertado, volta para a barca.
- **Mecânica:** **andar no rio**: o herói anda sobre a luz; a correnteza leva (rápido a favor, lento contra).
- **Ganchos:** loot gota de luz · montaria `mount_river_seahorse` (Despertar IV) · casa: **Cais de Luz**.
- **Noite:** o rio fica prateado (Lua).

### 93 · Convés da Barca da Lua / *Moon Ship Deck* — nv 436–450 · ☆
- **Fonte:** a Barca da Lua, que junta a luz (por isso cresce) e a passa para o Sol; a lua-sapato do \|Kaggen, piada da
  Lavine (bíblia §3.16).
- **Bioma e visual:** o convés enorme de uma barca prateada; a serpente Mehen como guarda-corpo; prata, lilás, noite.
- **Layout:** G. Convés em 3 níveis, mastro, porão; **posto: Cabine da Proa**. Retorno: Cais da Coluna.
- **Inimigos:** Clandestino de Treva [rápido]; Faísca Teimosa (faísca que não quer ir para casa; não se bate: se convence)
  [especial]; Sombra do Porão [tanque]; Corda Viva [controle].
- **Chefe:** **O Clandestino Grande**, sombra que se escondeu no porão para chegar ao céu; vencida, desce de volta.
- **Mecânica:** **a lua cresce**: entregar faíscas enche o convés de lanternas (fase da lua do servidor sobe); na lua
  cheia o convés vira festa.
- **Ganchos:** loot prata de lua · montaria `mount_boitata` (Mehen) · casa: conjunto novo **Barca da Lua** (miniatura).
- **Noite:** sempre noite; a Lavine tem falas especiais.
- **No jogo (2026-11-09):** a Barca da Lua atraca à noite no Píer da Lua e deixa o herói na faixa a oeste da Cabine da Proa (x −40, z 1000), de onde a volta ao píer sai a qualquer hora. Falta: a lua crescer com as faíscas, as lanternas e o "sempre noite" do convés.

### 94 · Roda Celeste das Doze Jarras / *Celestial Twelve-Jar Wheel* — nv 446–460 · ☆
- **Fonte:** a roda com doze jarras que sobe a luz (*Acta Archelai* 8); no conto, os doze signos são arcontes presos na
  roda.
- **Bioma e visual:** a roda gigante de verdade, girando no céu; bronze, água de luz, nuvem; 12 jarras do tamanho de casas.
- **Layout:** GG. As 12 jarras são plataformas que giram; o eixo no meio; **posto: Eixo da Roda** (Adda). Retorno: Cais da
  Coluna.
- **Inimigos:** Os Doze da Roda (doze arcontes pequenos, cada um com a forma de um signo, em versão de brinquedo: carneiro,
  touro, caranguejo...) [elite, um por jarra]; Jarra Rachada [distância]; Gota de Treva [rápido].
- **Chefe:** **O Arconte da Roda**, o que trava a roda para ela não girar; vencido, a roda gira e sobe a luz do servidor.
- **Mecânica:** **plataformas que giram**: as jarras giram com a roda; esperar a sua passar.
- **Ganchos:** loot bronze celeste · casa: **Roda das Doze Jarras** (versão de luz).
- **Noite:** a roda gira mais devagar; as jarras brilham.

### 95 · Esferas dos Arcontes / *Spheres of the Archons* — nv 456–470 · ☆
- **Fonte:** no conto, o Espírito Vivo prende os arcontes nas esferas do céu (Teodoro bar Konai). No jogo: presos como
  balões gigantes, resmungando.
- **Bioma e visual:** esferas de cristal flutuando em órbitas; cristal, nuvem, ouro; dentro de cada uma um gigante
  emburrado.
- **Layout:** GG. 7 esferas em órbita; **posto: Ilhota Parada**. Retorno: Cais da Coluna.
- **Inimigos:** Arconte Acorrentado Menor [tanque, empurra]; Fumaça de Esfera [controle]; Vento de Órbita [rápido];
  Gigante-Balão [elite].
- **Chefe:** **chefe do mundo: Saklas e Namrael, de novo** (a dupla de arcontes volta na forma do céu: revanche de fim de
  jogo). Vencidos, voltam para as esferas.
- **Mecânica:** **pular de esfera em esfera**: as órbitas se cruzam por pouco tempo.
- **Ganchos:** loot cristal de esfera · casa: troféu da dupla.
- **Noite:** as esferas acendem como planetas.

### 96 · Jardim de Luz / *Garden of Light* — nv 466–478 · ☆
- **Fonte:** o jardim de luz que Mani mostra a Mihrshah (M 47): o jardim do mapa 06, de verdade.
- **Bioma e visual:** jardim de árvores de luz, flores que tocam música, fontes; branco, ouro, verde-claro, rosa.
- **Layout:** G. Quatro jardins e o pavilhão; **posto: Pérgola do Jardineiro** (o Velho Mihrshah, que enfim volta para
  ver). Retorno: Cais da Coluna.
- **Inimigos:** poucos: Sombra que Pisa as Flores [rápido]; Ladrões de Āz [elite]; Espinho de Fumaça [controle].
- **Chefe:** **chefe do mundo: Āz, a Capitã Cobiçosa, e a Última Arca**: ela quer o jardim inteiro numa arca. Vencida, não
  morre: devolve a arca e "fica com vergonha" (a última Arca Resgatada +5).
- **Mecânica:** **plantar luz**: sementes de luz plantadas aqui aparecem no jardim da casa do jogador (Housing).
- **No jogo (2026-11-09): chefe do mundo da Região 16: Āz, a Capitã Cobiçosa, e a Última Arca** (`enemy_mountain_az_captain`, nv 478; o mapa 100 não tem luta pela ficha): arena num anel de pedras novo no Jardim das Flores (−960, 2570 na região), no fim da estrada do jardim; três fases, com rajadas de luz. Própria: **a cobiça da Arca** (a cada 16 s, da 2ª fase, aviso de 2 s: tira 15% da vida atual do herói, mínimo 1). Não morre: devolve a arca e fica com vergonha, volta em 5 min. Loot próprio (2026-11-11): **Semente de Luz de Āz** (`item_chefe_semente_luz`, lendária, 30% por vitória, 800 de venda; no lugar do punhado de ouro, o loot do bioma segue igual); vai no Tear/Forja da casa (`HouseCraftRules`): Saia de aurora da Maria e Saia do horizonte apagado da Lavine (Tear de mestre). Troféu de casa (2026-11-11): **Estátua de Āz devolvendo a arca** (`casa_trofeu_036`, lendário, piso), feito na Forja da casa com 2 Semente de Luz e o chefe já vencido (`HouseCraftRules.WorldBossTrophies`). Falta: a Arca Resgatada +5.
- **Ganchos:** loot sementes de luz · casa: conjunto novo **Jardim de Luz** (árvore de luz da casa).
- **Noite:** as flores cantam baixinho; vaga-lumes.

### 97 · Convés da Barca do Sol / *Sun Ship Deck* — nv 474–486 · ☆
- **Fonte:** a Barca do Sol, que leva a luz para casa (Kephalaia); a barca *Mandjet* da manhã (Kemet).
- **Bioma e visual:** convés dourado com vela triangular de dhow; ouro, laranja, branco; calor bom.
- **Layout:** G. Convés, mastro alto, vela; **posto: Cabine da Popa**. Retorno: Cais da Coluna.
- **Inimigos:** Clandestino de Brasa [rápido]; Sombra do Meio-Dia (escondida embaixo da vela) [controle]; Gaivota de Luz
  Apagada [distância]; Corda de Fumaça [controle].
- **Chefe:** **A Sombra da Vela**, que faz a vela pegar o vento errado; vencida, a barca anda.
- **Mecânica:** **carregar a luz**: levar lanternas da Barca da Lua (mapa 93) para cá completa a viagem da luz (missão de
  servidor).
- **Ganchos:** loot ouro de sol · montaria nova **Cavalo-Estrela da Coluna** (variante sol) · casa: conjunto novo **Barca
  do Sol** (miniatura).
- **Noite:** não há noite no convés do Sol (sempre dia).

### 98 · Pés da Coluna da Glória / *Foot of the Column of Glory* — nv 482–492 · ☆
- **Fonte:** a Coluna da Glória (*Srōšahrāy*, *Stylos tēs doxēs*; bíblia §1.5).
- **Bioma e visual:** a base da Coluna: um pilar de luz que some no alto; degraus de nuvem firme; branco, as cinco cores.
- **Layout:** GG (vertical). Degraus em espiral; **posto: Degrau Um**. Retorno: Cais da Coluna.
- **Inimigos:** Última Sombra (sombras tentando subir) [rápido]; Arconte Menor [elite]; Vento do Alto [controle].
- **Chefe:** **O Último Arconte do Degrau**, que senta no degrau para ninguém subir; vencido, desce.
- **Mecânica:** **guiar as faíscas**: faíscas perdidas seguem o herói (escolta); levá-las até o alto é a missão da G5.
- **Ganchos:** loot luz da Coluna · casa: conjunto novo **Coluna** (fio de luz de enfeite).
- **Noite:** a Coluna fica mais forte (mais faíscas sobem à noite).

### 99 · Mirante do que Segura o Céu / *Lookout of the Sky-Holder* — nv 488–498 · ☆
- **Fonte:** no conto, um dos filhos do Espírito Vivo, o "Ornamento de Esplendor" (*Splenditenens*), segura o céu
  (Agostinho, *Contra Faustum*; Kephalaia). No jogo ele é **só uma silhueta enorme de luz**, ao longe, segurando o céu:
  não fala, não luta, não é personagem.
- **Bioma e visual:** plataforma alta de onde se vê a silhueta; o céu tem rachaduras por onde escorre sombra; ouro, branco,
  rachaduras pretas.
- **Layout:** GG. Cinco rachaduras em volta; **posto: Mão Aberta** (a plataforma "na mão" da silhueta). Retorno: Cais da
  Coluna.
- **Inimigos:** Rachadura do Céu [controle]; Arconte Fugido [elite]; Vento do Alto [rápido]; Pedra do Céu [tanque].
- **Chefe:** **A Rachadura Grande**, sombra que escorre de uma fenda no céu; selada, o céu fica inteiro.
- **Mecânica:** **segurar o céu**: o grupo carrega pilares de luz até as rachaduras e segura (tempo) enquanto outro grupo
  luta.
- **Ganchos:** loot fragmento de céu · casa: troféu da Rachadura.
- **Noite:** as rachaduras aparecem mais (sombra escorre melhor no escuro).
- **No jogo (2026-11-09): segurar o céu**, versão solo: [E] ergue um pilar numa das 5 rachaduras e basta ficar a até 7 m por 20 s, lutando à vontade; longe, o pilar perde 2 s por segundo e apaga. A cada 6 s a sombra pinga (aviso 1,5 s antes, anel vermelho): 5% da vida máxima, nunca abaixo de 30% dela. Selar cura 30% da vida, tira 15% da vida dos bichos a até 20 m (chefes não) e deixa a rachadura fechada por 10 min no relógio do servidor; cada uma tem a fala de um primo e, com as cinco, a Vó Edna fala no rádio. Falta: carregar os pilares em grupo, a noite mudar as rachaduras e o estado compartilhado entre jogadores.

### 100 · Varanda do Tempo de Depois / *Porch of the Time After* — nv 495–500 · ☆
- **Fonte:** o Tempo de Depois, o fim do conto, "nunca mostrado" (bíblia §1.5); a varanda e a cadeira de balanço vazia do
  Vô Jorge (bíblia §1.7, eco do trono vazio do Bema).
- **Bioma e visual:** uma varanda igual ao iwan da Vó Edna, mas no céu, com o pomar de luz em volta e a **cadeira de
  balanço vazia**; adobe damasco, ouro, lilás; lá embaixo, a ilha inteira acesa.
- **Layout:** M. A varanda (zona segura) e o pomar; a porta da raide **A Grande Treva** no fim do pomar (liga ao 90);
  **posto: a Varanda**. Retorno: Cais da Coluna.
- **Inimigos:** **Sombras de Saudade** (sombrinhas que só querem companhia): **não se luta**, senta-se perto delas no banco
  e elas viram luz. Fora do pomar, os bichos do 99.
- **Chefe:** nenhum. É o lugar de chegar no **nível 500**, receber o primeiro ponto de **Mestre** e o Despertar IV da
  montaria. Gancho da raide de temporada.
- **Mecânica:** **sentar e contar**: sentar na varanda conta uma história do Vô Jorge por dia (texto novo, a família
  inteira junta, a Vó Edna puxa o hino); o Ari deita do lado da cadeira.
- **Ganchos:** loot nenhum (só lembranças) · montaria: a pose de exibição das montarias despertas · casa: conjunto novo
  **Varanda do Tempo de Depois** (a cadeira de balanço do Vô Jorge, versão de luz).
- **Noite:** as lanternas de todas as 27 cidades acendem lá embaixo; o Vô Jorge "aparece" só como uma faísca que sobe.
- **No jogo (2026-11-09): sentar e contar**: [E] a até 4 m da cadeira do Vô Jorge; 5 histórias de 4 falas (6 s cada), uma por dia UTC, com primos, Vó Edna, Vô Jorge de memória e o Ari; à noite, uma fala final com as lanternas e a faísca que sobe. Os controles nunca travam: andar mais de 5 m ou tomar dano encerra; sentado, recupera 3% da vida por segundo. Falta: pose sentada, guardar a história do dia no save e as Sombras de Saudade.

---

## 6. Montarias novas propostas (ganchos dos mapas)

Seguem o `Docs/Mounts.md`: orçamento por raridade (§2.4), teto de velocidade 1,80 (§1), nada de dano (§2.2), Lendária
nunca vendida. Números finais pelo agente de montarias; aqui só o conceito. Crescimento por nível e Despertar:
`Docs/Progressao500.md` §6.

| id (novo) | Nome (pt / en) | Raridade | Tipo | Mapa | Ideia de habilidade | Fonte |
|---|---|---|---|---|---|---|
| `mount_marsh_heron` | Garça do Brejo / *Marsh Heron* | Incomum | terrestre (pernalta) | 03, 30 | anda na água rasa sem lentidão | garça (bicho real) |
| `mount_laurel_lizard` | Lagarto-do-Louro / *Laurel Lizard* | Incomum | terrestre | 05 | sobe troncos marcados (atalho) | lagartos-gigantes das Canárias |
| `mount_ochre_war_wolf` | Lobo de Guerra Ocre / *Ochre War Wolf* | Raro | terrestre, guerra | 11 | uivo: comuns perto fogem 2 s | invenção (orcs do Tambor Rubro) |
| `mount_eland` | Eland do Capim Alto / *Tall-Grass Eland* | Raro | terrestre | 15 | salto longo | eland (bicho real; contos \|xam só como imagem) |
| `mount_bronze_ram` | Carneiro de Bronze / *Bronze Ram* | Raro | terrestre (engenho) | 17 | investida que abre portas de runa | invenção (anões) |
| `mount_dhow_turtle` | Dhow-Tartaruga / *Dhow Turtle* | Épico | aquático | 25, 29 | vela: corre no mar a favor do vento | tartaruga-verde + vela de dhow |
| `mount_aratta_aurochs` | Auroque de Aratta / *Aratta Aurochs* | Raro | terrestre, guerra | 26 | pisão: lentidão em área | auroque (bicho antigo) |
| `mount_musawwarat_elephant` | Elefante de Musawwarat / *Musawwarat Elephant* | Épico | terrestre grande | 27 | leva 1 aliado na garupa | os elefantes de Musawwarat es-Sufra (Kush) |
| `mount_jhilpur_buffalo` | Búfalo-d'Água de Jhilpur / *Jhilpur Water Buffalo* | Incomum | aquático | 31 | nada em lama e água | búfalo-d'água (bicho real) |
| `mount_tarchon_frog` | Sapo-Gigante de Tarchon / *Tarchon Giant Frog* | Incomum | terrestre | 33 | pulo duplo sobre água | invenção |
| `mount_ninki_nanka` | Ninki Nanka Mansa / *Tame Ninki Nanka* | Épico | aquático | 34 | mergulho: some da vista 4 s | folclore da Senegâmbia |
| `mount_roof_yak` | Iaque do Teto do Mundo / *Roof-of-the-World Yak* | Comum | terrestre | 36 | imune ao frio | iaque (bicho real) |
| `mount_white_bactrian` | Camelo-Bactriano Branco / *White Bactrian Camel* | Raro | terrestre | 40 | imune a tempestade de areia e neve | camelo-bactriano (a Teimosa é parente) |
| `mount_white_light_stag` | Cervo de Luz Branca / *White Light Stag* | Épico | terrestre | 42 | rastro de luz que ilumina o escuro | invenção (altos-elfos) |
| `mount_terracotta_horse` | Cavalo de Terracota Desperto / *Awakened Terracotta Horse* | Raro | terrestre, guerra | 45 | não leva atordoamento 3 s | o exército de terracota (Xi'an) como imagem |
| `mount_golden_steppe_horse` | Cavalo Dourado da Estepe / *Golden Steppe Horse* | Raro | terrestre | 47 | galope com fôlego extra | o cavalo de pelo dourado da Ásia Central |
| `mount_hill_ram` | Carneiro Gigante da Colina / *Hill-Giant Ram* | Incomum | terrestre | 49, 82 | empurra pedras (abre caminho) | invenção (gigantes-da-colina) |
| `mount_taiga_elk` | Alce da Taiga / *Taiga Elk* | Incomum | terrestre | 50, 80 | atravessa mato fechado sem lentidão | alce (bicho real) |
| `mount_gordias_goat` | Bode Dourado de Gordias / *Gordias' Golden Goat* | Raro | terrestre | 53 | desata nós (abre baús de corda) | mitos frígios |
| `mount_huma_chick` | Filhote de Huma / *Huma Chick* | Lendário | voador ("nunca pousa") | 55, 58 | voo livre curto; nunca encosta no chão | a huma (Pérsia) |
| `mount_aurora_reindeer` | Rena da Aurora / *Aurora Reindeer* | Épico | terrestre | 59, 61, 64 | corre sobre gelo fino sem quebrar | rena (bicho real) |
| `mount_lacquer_lion` | Leão-Guardião de Laca / *Lacquer Guardian Lion* | Épico | terrestre | 65, 66 | aviso: mostra bichos escondidos | as *shisa* de Okinawa como imagem |
| `mount_volcano_goat` | Cabra-dos-Pastores do Vulcão / *Volcano Herders' Goat* | Incomum | terrestre | 67 | imune a chão quente | cabras dos pastores das Canárias |
| `mount_mesektet_ibis` | Íbis Real de Mesektet / *Royal Ibis of Mesektet* | Raro | voador (planar) | 71 | planar longo | íbis (Kemet) |
| `mount_mushhushshu` | Mušḫuššu Manso / *Tame Mušḫuššu* | Épico | terrestre | 77 | rugido: medo em comuns (não em chefe) | o dragão-serpente da Babilônia |
| `mount_light_skiff` | Barquinha de Luz / *Little Light Skiff* | Lendário | aquático e voador baixo | 76 | anda sobre água e luz; flutua 1 m | as Barcas de Luz (bíblia §3.16) |
| `mount_column_star_horse` | Cavalo-Estrela da Coluna / *Star Horse of the Column* | Lendário | voador | 90, 97 | voo livre; rastro das cinco cores | invenção (recompensa de temporada) |
| (cosmético) | Qilin de Jade / *Jade Qilin* | — | — | 41 | variante visual do `mount_kirin` | qilin (China) |

**No código (2026-11-11):** 8 destas existem (`MountFarRegionCatalog`, `Docs/Mounts.md` §3.9): Bode Dourado de Gordias,
Rena da Aurora, Leão-Guardião de Laca, Cabra-dos-Pastores do Vulcão, Íbis Real de Mesektet, Mušḫuššu Manso, Alce da Taiga
e Carneiro Gigante da Colina. As outras 19 (todas as das Regiões 1–9, a Barquinha de Luz, o Filhote de Huma e o
Cavalo-Estrela) seguem só propostas.

27 espécies novas (o catálogo iria de 30 para 57), espalhadas pelas raridades. A Lendária de temporada
(`mount_column_star_horse`) é **vinculada**, nunca troca nem vende (Mounts §5.3).

---

## 7. Fontes e [verificar]

- **Maniqueísmo:** Teodoro bar Konai, *Liber Scholiorum* XI (os cinco mundos da Treva e os seus bichos; os arcontes presos
  no céu); Kephalaia coptas (Barcas, Coluna, *bōlos*, viagem à Índia no cap. 1); *Acta Archelai* 8 (a roda de doze
  jarras); Códice Maniqueu de Colônia (Salmaios, Abiesous, Baraies); textos de Turfan M 2 (Ammo), M 47 (Mihrshah);
  *Livro dos Gigantes* (Ohya, Hahya, Mahaway, Hobabish, Virōgdād, Barakʾel, Šahmīzād); cartas de Kellis (Pamour, Tehat,
  Makarios); *Compêndio* Mingjiao (731); Bögü Qaghan (762).
- **Rota da Seda e Ásia:** *Cartas Antigas* sogdianas (Miwnay, Shayn); karez de Turfan; Mingsha Shan; Baltit e Altit
  (Hunza); corredor de Wakhan; Chang'an e a Cidade Proibida; os Quatro Perigos (*Zuo Zhuan*, *Shan Hai Jing*); o *nian*;
  a carpa do Portão do Dragão; Kucha (Haripuṣpa, Suvarṇapuṣpa); Shuri e as *gusuku* de Ryukyu; *shisa*; tanuki; *almas*;
  *aydahar* (Cazaquistão).
- **Cáucaso e estepe:** sagas dos Nartes (Soslan, Batradz, Satana, Syrdon, Uryzmæg; Uasamonga); Amirani e o *ochokochi*
  (Geórgia); torres svan; kurgans de Pazyryk e Issyk; Heródoto IV (grifos e os ouros do céu); folclore armênio (vishap,
  nhang, *ays*); folclore komi (Yen e Omöl, o mamute de baixo da terra, os *kul*, Yirkap, Pera).
- **Mesopotâmia, Elam, Anatólia, Levante:** Chogha Zanbil (Dur-Untash), Napirasu, Untash-Napirisha, Puzur-Inshushinak,
  Shilhak-Inshushinak, Kidin-Hutran; épicos sumérios de Enmerkar e Lugalbanda; Shulgi de Ur; o conto de Gilgamesh
  (Humbaba, *girtablullu*, Sin-leqi-unninni, Utnapishtim); Atrahasis; *mušḫuššu*; *Canção de Ullikummi* e *Canção de
  Hedammu* (hurritas); contos de Kessi, Appu e Gurparanzah; Ashmunikal; épicos de Kirta e de Aqhat (Ugarit); mitos frígios
  (Marsyas, Gordias, Midas); etruscos (Tarchon e nomes de inscrições).
- **África antiga e oral:** Kemet (*Amduat*, *Livro dos Portões*, Mesektet e Mandjet, Mehen, Apep, campos de juncos, lago de
  fogo; contos de Sinuhe, do Camponês Eloquente, do Papiro Westcar, de Setne); Kush/Meroé (Amanirenas, Amanitore,
  Natakamani, Shanakdakhete, Arkamani; Naqa e Musawwarat es-Sufra); costa suaíli (Kilwa, Husuni Kubwa, Lamu, Gedi; Fumo
  Liyongo; *mngwa*); *abada* (Congo), *impundulu* (xhosa e zulu), *dingonek* (Quênia), *kongamato* (Zâmbia), *ninki nanka*
  (Senegâmbia); Grande Zimbábue; Aksum; litema; pinturas de rocha do Drakensberg; guanches (Gara e Jonay, Bencomo,
  Tinguaro, Dácil; Echeyde, Guayota, tibicenas).
- **Arquitetura das capitais (§2):** Sundarbans e as pontes de raiz de Meghalaya; Hunza e Shibam; cidades imperiais
  chinesas; Paro Taktsang e Meteora; casas do Ártico e da costa do Pacífico Norte; Shuri.
- **[verificar] antes de pôr em texto do jogo:** grafias de Farnkhund e Takhsich (sogdiano); Lin Deng (maniqueu da era
  Song); Virōgdād/Barakʾel/Šahmīzād (grafias do *Livro dos Gigantes*); Haripuṣpa/Suvarṇapuṣpa como reis de Kucha; o
  *mngwa*, o *dingonek* e o *kongamato* (relatos de criptídeos, conferir a fonte do conto); diacríticos na UI (Mušḫuššu →
  "Mushussu", Āz → "Az" se a fonte do jogo não tiver os sinais).
