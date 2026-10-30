# Os 100 mapas da Ilha da Aurora (e além): níveis 1–500

> **Status:** catálogo de design (2026-10-30). **Só documentação:** nada daqui existe no código ainda, além dos 4 biomas
> de hoje (Floresta, Deserto, Montanha, Cavernas), que são os mapas 01–02, 07–08, 13–14 e 19–20.
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
| 84 | Cova da Fumaça | *Pit of Smoke* | Limiar Profundo | 348–362 | posto: Lâmpada 1 | Fortaleza | ☆ |
| 85 | Cova do Fogo Devorador | *Pit of Devouring Fire* | Limiar Profundo | 360–374 | posto: Lâmpada 2 | Fortaleza | ☆ |
| 86 | Cova do Vento Destruidor | *Pit of Ruinous Wind* | Limiar Profundo | 372–388 | posto: Lâmpada 3 | Fortaleza | ☆ |
| 87 | Cova do Lodo | *Pit of Mire* | Limiar Profundo | 386–402 | posto: Lâmpada 4 | Fortaleza | ☆ |
| 88 | Cova da Treva | *Pit of Darkness* | Limiar Profundo | 400–416 | posto: Lâmpada 5 | Fortaleza | ☆ |
| 89 | Ponte do Novelo | *Bridge of the Bōlos* | Limiar Profundo | 412–428 | posto: Pilar do Meio | Fortaleza | ☆ |
| 90 | Portão da Terra da Treva | *Gate of the Land of Darkness* | Limiar Profundo | 424–440 | posto: Selo dos Artífices | Fortaleza | ☆ |
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

### 07 · Estrada das Caravanas / *Caravan Road* — nv 5–7 · ★
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

### 13 · Trilha dos Rondavéis / *Rondavel Trail* — nv 9–11 · ★
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

### 19 · Lago do Limiar / *Threshold Lake* — nv 13–15 · ★
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
- **Fonte:** Mahaway, o gigante mensageiro que voa (*Livro dos Gigantes*).
- **Bioma e visual:** poço vertical gigante com ninhos nas paredes; basalto, índigo, pedra clara, âmbar; pontes de corda
  em espiral.
- **Layout:** G (vertical). Descida em espiral por pontes até o ninho; **posto: Última Ponte de Corda**. Retorno: Vila
  das Lamparinas.
- **Inimigos:** Revoada de Mahaway [enxame]; Ubır [rápido]; Morcegão de Pedra [tanque]; Corvo-da-Treva [distância].
- **Chefe:** **chefe do mundo: Mahaway, o Rei da Revoada / o Morcegão da Treva** (já no código).
- **Mecânica:** **correntes de ar**: subir pelos poços de vento (planando) é atalho; cair é a rota longa.
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
- **Fonte:** cidades de pedra de coral da costa suaíli (Kilwa, Lamu); dhows e as monções do Oceano Índico; Fumo Liyongo,
  herói-arqueiro e poeta suaíli (já é a G3 da Maithe).
- **Bioma e visual:** praia, recife raso e cidade de coral; paleta acima; coqueiros, mangue na borda, poças de maré.
- **Layout:** G. **Taa Pwani** (cidade) no centro do litoral; recife a leste que aparece na maré baixa; dunas e mato
  a oeste; Cais da Barca e cais de dhows. Retorno: Taa Pwani.
- **Inimigos:** Caranguejo-dos-Coqueiros Gigante [tanque] (bicho real); Moreia Apagada [rápido]; Gaivota Ladra [rápido];
  Ouriço de Fumaça [controle, espinhos lentos] (invenção).
- **Chefe:** **O Polvo das Monções**, polvo enorme apagado que segura as velas dos barcos; libertado, a monção volta.
- **Mecânica:** **maré**: a cada 8 min a maré baixa (3 min) e abre trilhas no recife com baús e faíscas; na maré alta só
  montaria aquática atravessa.
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
- **Chefe:** **O Touro de Fumaça**, auroque gigante apagado que pisoteia o capim; libertado, guia o rebanho de Aratta.
- **Mecânica:** **fogo baixo**: o povo-touro queima o capim velho em faixas (Fogo Vivo que renova); as faixas queimadas
  viram trilhas rápidas; os bichos fogem delas.
- **Ganchos:** loot couro de auroque, Fitas de Corrida · montaria nova **Auroque de Aratta** (§6) · casa: conjunto novo
  **Salão de Junco** (lira de cabeça de touro, esteira).
- **Noite:** a Ave-Relâmpago brilha nas nuvens; **Hiena de Fumaça** [enxame, só à noite]; visão longa (céu aberto).

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
- **Chefe:** **chefe do mundo: Hedammu, a Serpente Gulosa do Mar** (nova; aliada de Āz: "come tudo e nunca enche").
  Vencida, cospe as faíscas que engoliu e volta para o fundo, menor e mais calma.
- **Mecânica:** **canais e dhows**: barcos pequenos de aluguel (2 ouro) andam rápido nos canais; a maré fecha canais.
- **Ganchos:** loot escama de Hedammu, raiz de mangue · montaria nova **Garça do Brejo** (variante de mangue) · casa:
  troféu de Hedammu.
- **Noite:** vaga-lumes nos mangues; o kongamato dobra; os canais ficam escuros (lanterna ajuda).

### Região 6 — Delta de Jhilpur (pântano de raízes, Água Viva) · mapas 31–35

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
- **Chefe:** nenhum na capital; evento de cidade **Maré de Lodo** (ondas de Filhos do Lodo sobem os ghats; defender a
  Raiz-Mãe, estilo Crywolf do MU, Referências §2).
- **Mecânica:** **monção**: na estação das chuvas (`SeasonCalendar`) a água sobe um andar; ruas viram canais e a cidade
  muda de rota; a escola de barco do Abiesous ensina a remar.
- **Ganchos:** loot seda de lótus, raiz aérea · montaria nova **Búfalo-d'Água de Jhilpur** (§6) · casa: conjunto novo
  **Casa nas Raízes** (rede de raiz, lanterna de lótus). G4 do Joaquim (Raiz de Jhilpur) é aqui.
- **Noite:** lanternas de lótus boiando; vaga-lumes nas raízes; a Raiz-Mãe brilha verde-água.

### 32 · Pontes de Raiz Viva / *Living Root Bridges* — nv 64–70 · ★
- **Fonte:** pontes de raiz viva de Meghalaya (forma); a Água Viva que faz as raízes crescerem (bíblia §1.3).
- **Bioma e visual:** floresta de chuva com rios em garganta; raiz `#6B5236`, musgo `#5E8F3E`, pedra molhada `#6F7470`,
  cascata `#CFE8EF`; pontes de raiz em dois andares, escadas de pedra.
- **Layout:** G. Gargantas paralelas ligadas por pontes; algumas pontes "esquecidas" (desfeitas); **posto: Plataforma
  das Garças**. Retorno: Jhilpur.
- **Inimigos:** Macaco Ladrão [rápido]; Sanguessuga do Lodo [enxame]; Píton Apagada [controle] (bicho real); Raiz-Laço
  [controle].
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
- **Ganchos:** loot junco, Lanterna de Fogo-Fátuo · montaria nova **Sapo-Gigante de Tarchon** (§6) · casa: conjunto novo
  **Palafita** (cesto gigante, rede de pesca).
- **Noite:** os fogos-fátuos dobram e a névoa sobe; Lanterna de Fogo-Fátuo dá visão normal.

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

### Região 7 — Teto do Mundo (Pamir) · mapas 36–40

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
- **Chefe:** nenhum na capital; evento **Avalanche** (desce a neve, os primos protegem os terraços de baixo).
- **Mecânica:** **calhas**: descer de carrinho é rápido, subir pede escada ou elevador de corda (fila); o carrinho pode
  levar até 5 (o grupo).
- **Ganchos:** loot damasco seco, lã de iaque · montaria nova **Iaque do Teto do Mundo** (§6) · casa: conjunto novo
  **Terraço do Pamir** (fogão de pedra, tapete de feltro).
- **Noite:** os terraços acendem um a um; estrelas fortes; frio (fôlego mais lento fora de casa).

### 37 · Calhas de Pedra / *Stone Chutes* — nv 74–80 · ★
- **Fonte:** invenção (as calhas de Sangkent continuam montanha afora, feitas pelos Artífices para descer minério).
- **Bioma e visual:** encosta íngreme com calhas de pedra polida; pedra `#8C8173`, gelo `#DCEAF0`, madeira; pilares,
  estações de troca de carrinho.
- **Layout:** G (vertical). Rede de 4 calhas com estações; **posto: Estação da Calha Alta**. Retorno: Sangkent.
- **Inimigos:** Ushabti de Calha [tanque]; Abutre-Barbudo Apagado [distância, solta pedras] (bicho real); Marmota
  Gigante [enxame]; Neblina do Vento Ruim [controle].
- **Chefe:** **O Carrinho Desgovernado**, engenho de minério dos Artífices que desce sozinho; libertado, vira o carrinho
  mais rápido da cidade.
- **Mecânica:** **descida cronometrada**: descer de carrinho pegando faíscas no caminho (corrida; ranking online).
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

### Região 8 — Planície de Mingcheng (Luz) · mapas 41–46

Capital **Mingcheng** (§2): chinês *míng* 明 = luz, claro; *chéng* 城 = cidade murada. Gancho: a Mingjiao, a "Doutrina da
Luz", nome chinês do maniqueísmo (*Compêndio*, 731; o santuário de Cao'an em Quanzhou é o último de pé). NPCs: **Mestre
Mōzak** (título dos mestres maniqueus em chinês e sogdiano, *mozhak*; aqui, o guardião das lanternas), **Bögü, o Guarda do
Portão** (Bögü Qaghan, o khagan uigur que adotou o maniqueísmo em 762; aqui, o velho chefe da guarda dos anéis), **Lin
Deng** (maniqueu da era Song em Fujian **[verificar]**; aqui, o mestre do canal). Chefe do mundo: **Os Quatro Gulosos**
(mapa 46).

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

### 43 · Canal das Lanternas / *Lantern Canal* — nv 102–110 · ☆
- **Fonte:** o Grande Canal da China; a lenda da carpa que pula o Portão do Dragão (conto popular).
- **Bioma e visual:** canal largo entre salgueiros e vilas de água; água `#6FA3A8`, salgueiro `#8DB25A`, telha cinza,
  laca; eclusas de pedra, barcas de carga, pontes de corcunda.
- **Layout:** G. Canal de norte a sul com 4 eclusas; **posto: Eclusa da Lua**. Retorno: Mingcheng.
- **Inimigos:** Carpa-Dragão Apagada [rápido]; Barqueiro de Fumaça [elite]; Lanterna-Peixe [enxame]; Tartaruga de Pedra
  (a tartaruga que carrega estelas) [tanque].
- **Chefe:** **A Carpa que Não Pulou**, carpa enorme que desistiu de pular a cascata e ficou apagada; libertada, pula e
  vira um arco de luz.
- **Mecânica:** **eclusas**: abrir e fechar as eclusas muda o nível da água e abre os bairros baixos.
- **Ganchos:** loot escama de carpa, seda de canal · montaria `mount_river_seahorse` · casa: **Festa das Lanternas**
  (Lanterna-peixe).
- **Noite:** lanternas no canal; o Barqueiro de Fumaça rema.

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

### 45 · Muralha das Mil Torres / *Wall of a Thousand Towers* — nv 115–125 · ☆
- **Fonte:** a Grande Muralha (forma); o *nian*, a fera do Ano-Novo dos contos chineses, que foge do vermelho e do
  barulho.
- **Bioma e visual:** muralha sobre cristas de montanha; tijolo cinza, laca, pinheiro `#3E6B4A`, névoa; torres de vigia,
  escadarias íngremes, faróis.
- **Layout:** GG (comprido). A muralha é o caminho, com 12 torres; descidas para vales; **posto: Torre Quinhentos**.
  Retorno: Torres Brancas.
- **Inimigos:** Soldado de Terracota [tanque]; Arqueiro de Terracota [distância]; Cavalo de Terracota [rápido]; Filhote de
  Nian [elite].
- **Chefe:** **Nian, a Fera do Ano**: assusta-se com lanternas vermelhas e com barulho (granada do Ícaro, tambor); a luta
  é espantar, não bater; vencida, vai dormir até o ano que vem.
- **Mecânica:** **faróis**: acender os faróis das torres liga um "corredor de luz" (velocidade +10% a pé dentro dele).
- **Ganchos:** loot tijolo vidrado, lanterna vermelha · montaria nova **Cavalo de Terracota Desperto** (§6) · casa: **Festa
  das Lanternas**.
- **Noite:** os faróis acesos ficam; o nian é mais ativo; névoa nos vales.

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

### Região 9 — Estepe dos Nartes (estepe e Cáucaso, Vento Vivo) · mapas 47–52 · ☆

Estepe de capim alto, kurgans e o Cáucaso ao fundo. Culturas: osseta (Nartes), cita (ouros e grifos), georgiana. Três
cidades de raça (centauros, gigantes-da-colina, povo-tronco). Estilo base `SteppeCamp`. Paleta: capim `#BFAE5A`,
feltro `#D8C9A8`, céu `#A7C7E7`, ouro cita `#D4A93A`, montanha `#6E7480`. Chefe do mundo: **Ohya** (mapa 52, o irmão do
Hahya guardado para o online, bíblia §1.4).

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

### 48 · Kurgans de Ouro / *Golden Kurgans* — nv 106–114 · ☆
- **Fonte:** kurgans de Pazyryk e Issyk (ouro e feltro citas); os grifos que guardam o ouro (Heródoto IV); os **Ouros do
  Céu** que só o caçula consegue pegar (Heródoto IV.5–7; bíblia §1.6).
- **Bioma e visual:** colinas-túmulo de terra e pedra na estepe; terra, pedra, ouro cita, feltro colorido; cervos de ouro
  estilizados (o motivo do "estilo animal").
- **Layout:** G. 9 kurgans; um aberto (posto); **posto: Kurgan Aberto**. Retorno: Acampamento de Soslan.
- **Inimigos:** Grifo Guardião [elite]; Cervo de Ouro Apagado [rápido]; Cisne de Feltro [distância] (as figuras de feltro
  de Pazyryk, invenção); Lobo de Bronze [tanque].
- **Chefe:** **O Grifo Velho do Kurgan**, que guarda o ouro há tanto tempo que esqueceu de quem; libertado, deixa um
  presente para cada primo.
- **Mecânica:** **os Ouros do Céu** (segredo): quatro objetos de ouro (arado com canga, machado, taça) quentes demais
  para os maiores; **só a Lavine** consegue pegar ("eu não sou pequena").
- **Ganchos:** loot ouro cita, feltro · montaria `mount_griffin` (Despertar IV) · casa: conjunto novo **Ouro da Estepe**
  (cervo de ouro de enfeite).
- **Noite:** os kurgans brilham por dentro; os grifos voam; visão boa.

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

### Região 10 — Picos de Farnkesh (Vento Vivo) · mapas 53–58 · ☆

Capital **Farnkesh** (§2): *farn* (sogdiano; *xwarrah* em persa), o nimbo de glória e boa sorte; *Kesh*, nome antigo de
Shahrisabz (Sogdiana). Pináculos de pedra acima das nuvens. NPCs com nomes maniqueus: **Shad-Ohrmizd** (fundador de um
ramo do oriente; aqui, guardião dos sinos de vento), **Sisin** (Sisinnios, sucessor de Mani; aqui, mestre das pontes de
corda). Chefe do mundo: **O Rei-Vendaval** (mapa 58).

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
- **Noite:** o baile; vaga-lumes; os bodes de fumaça dormem.

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

### Região 11 — Mar Gelado (Ar Sereno) · mapas 59–64 · ☆

Capital **Porto da Lua Branca** (§2). A Barca da Lua passa aqui e "atraca" na lua cheia. Culturas de inspiração: casas do
Ártico (só forma), costa noroeste do Pacífico (casas de tábua, sem os postes de brasão), folclore komi (Yen e Omöl, o
mamute debaixo da terra, os *kul* da água). NPCs: **Yirkap**, o caçador de esqui (herói dos contos komi), e **Pera**, o
gigante bom (bogatir komi). Respeito: povos do Ártico são vivos; nenhuma figura da fé deles aparece; inimigos são bichos
e a sombra. Chefe do mundo: **o Velho Kul** (mapa 64).

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
- **Noite:** aurora no céu; a Barca da Lua passa; visão boa nos canais iluminados.

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

### Região 12 — Arquipélago de Hinokura (Fogo Vivo) · mapas 65–70 · ☆

Capital **Hinokura** (§2): japonês *hi no kura*, "celeiro do fogo", eco do celeiro celeste do ferreiro (bíblia §1.6).
Ilhas vulcânicas com palácios de laca vermelha (forma de Shuri e das *gusuku* de Ryukyu; sem santuários). NPCs com nomes
japoneses comuns escolhidos pelo sentido: **Mestra Akane** ("vermelho vivo", mestra da laca), **Kurogane** ("ferro
preto", ferreiro), **Sango** ("coral", barqueira). Uma ilha (67) é do vulcão guanche **Echeyde**. Chefe do mundo: **o
Glutão de Brasa** (mapa 70).

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
- **Noite:** os canais de lava iluminam a ilha; lanternas vermelhas; visão boa.

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
- **Noite:** os tibicenas dobram; a lava ilumina; céu de estrelas fortíssimo.

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
- **Noite:** sempre noite aqui (o rio é a noite); estrelas no teto do céu como um tecido.

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

### Região 14 — Margens da Mistura (Elam, Mesopotâmia, terras hurritas; o deserto do Tempo) · mapas 77–82 · ☆

As "Margens da Mistura" (nome das terras de disputa do online, bíblia §3.11) são o lugar onde luz e sombra estão mais
misturadas. Culturas: elamita e mesopotâmica (zigurates, tijolo vidrado, betume), hurrita (Upelluri, Ullikummi) e o
*Livro dos Gigantes* (que traz Hobabish, o Humbaba do conto de Gilgamesh). Cidade: **Dur-Jarras** (acadiano *dūr* =
fortaleza; eco de Dur-Untash, o nome antigo de Chogha Zanbil). Estilo novo `Ziggurat`. Paleta: tijolo cozido `#B07A4A`,
tijolo vidrado azul `#2F5FA8`, betume `#2A2420`, junco, ouro, céu `#E8C690`. NPCs: **Sin-leqi-unninni** (o escriba que
copiou o conto de Gilgamesh; aqui, o bibliotecário), **Atrahasis** (o construtor do barco do dilúvio; aqui, o mestre das
balsas de betume), **Utnapishtim** (o velho que viu o dilúvio; aqui, cuida do Relógio de Zurvan da cidade). Chefe do
mundo: **o Sonho da Destruição** (mapa 82).

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
- **Noite:** o zigurate acende de cima para baixo; o Relógio de Zurvan marca a noite.

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
- **Noite:** só dá para sonhar à noite; de dia a tenda fica fechada.

### Região 15 — Limiar Profundo (as Cinco Covas da Treva) · mapas 83–90 · ☆

Debaixo das Grutas do Limiar ficam as **cinco covas** da Terra da Treva, uma para cada elemento escuro, cada uma com o seu
tipo de bicho: **Fumaça** (os de duas pernas), **Fogo Devorador** (os de quatro patas), **Vento Destruidor** (os que voam),
**Lodo** (os que nadam) e **Treva** (os que rastejam) (Teodoro bar Konai, *Scholion* XI; Kephalaia). Tom: frio, pegajoso,
barulhento, nunca gore (bíblia §4.3). Cidade: **Fortaleza da Última Lâmpada**, dos guardiões das lâmpadas, irmã da Vila
das Lamparinas. Estilo novo `BasaltFort`. Paleta: basalto `#2C2C33`, âmbar `#FFB347`, cobre `#B06B3A`, branco de roupa
`#F2EEE6`, índigo `#232046`. NPCs com nomes das **cartas de Kellis** (famílias maniqueias do oásis de Dakhla, séc. IV):
**Pamour** (o comandante das lâmpadas), **Tehat** (a tecelã de pavios), **Makarios** (o pai que escreve cartas para os
filhos longe, eco das cartas do Vô Jorge). Raide: **A Grande Treva** (mapa 90).

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

### 84 · Cova da Fumaça / *Pit of Smoke* — nv 348–362 · ☆
- **Fonte:** a Fumaça, 1º mundo da Treva, dos que andam em duas pernas.
- **Bioma e visual:** caverna cheia de fumaça cinza que se mexe; cinza `#7A7680`, preto, âmbar das lâmpadas.
- **Layout:** G. Corredores de fumaça; ventiladores antigos dos Artífices; **posto: Lâmpada 1**. Retorno: Fortaleza.
- **Inimigos:** Sombra-Cópia [rápido, imita o herói]; Gigante de Fumaça [tanque]; Fumaça Sufocante [controle]; Arqueiro de
  Fumaça [distância].
- **Chefe:** **O Rei da Fumaça**, figura de duas pernas feita de fumaça que copia quem chega; vencido com vento, vira ar.
- **Mecânica:** **vento limpa**: ligar os ventiladores (ou skills de Vento Vivo, Garrafas de Ar Sereno) limpa a fumaça por
  30 s.
- **Ganchos:** loot cinza de fumaça (Pedra da Mistura) · montaria nenhuma · casa: troféu do Rei da Fumaça.
- **Noite:** igual.

### 85 · Cova do Fogo Devorador / *Pit of Devouring Fire* — nv 360–374 · ☆
- **Fonte:** o Fogo Devorador, 2º mundo da Treva, dos de quatro patas.
- **Bioma e visual:** caverna de pedra quente com chão que acende em desenho; brasa, preto, laranja.
- **Layout:** G. Salões ligados; o chão acende em padrões; **posto: Lâmpada 2**. Retorno: Fortaleza.
- **Inimigos:** Lobo de Brasa [rápido]; Touro de Cinza [tanque]; Leão de Fumaça Quente [elite]; Chama Faminta [enxame].
- **Chefe:** **A Leoa do Fogo Devorador**, que come o fogo bom; vencida, vira brasa mansa.
- **Mecânica:** **chão que acende**: o padrão do chão avisa 1,5 s antes; ficar nas pedras escuras.
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

### 87 · Cova do Lodo / *Pit of Mire* — nv 386–402 · ☆
- **Fonte:** o Lodo (a água turva), 4º mundo da Treva, dos que nadam.
- **Bioma e visual:** caverna alagada de água preta; preto-verde, bolhas, âmbar.
- **Layout:** G. Salões alagados em níveis; **posto: Lâmpada 4**. Retorno: Fortaleza.
- **Inimigos:** Peixe da Treva [rápido]; Enguia Grande [controle]; Polvo de Lodo [tanque]; Cardume Escuro [enxame].
- **Chefe:** **O Peixe de Cauda Negra** (a cauda de peixe é outra das cinco formas do Príncipe); vencido, vira peixe-lanterna.
- **Mecânica:** **nível da água**: comportas dos Artífices sobem e descem a água; Bolhas de Ar para os trechos fundos.
- **Ganchos:** loot lodo condensado (Pedra da Água Viva ao purificar) · casa: troféu do Peixe.
- **Noite:** igual.

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

### Região 16 — Reinos do Céu (as Barcas, a Roda e a Coluna) · mapas 91–100 · ☆

A luz libertada sobe pela **Coluna da Glória** e navega nas **Barcas do Sol e da Lua** (bíblia §1.5). Aqui o jogo fica
quieto e bonito: menos bichos, mais luz, a família inteira junta. Cidade: **Cais da Coluna**, onde as barcas encostam.
Estilo novo `SkyQuay` (píeres de luz sólida, pavilhões de vidro e ouro sobre nuvens). Paleta: branco `#FAF7EF`, ouro
`#E8C35A`, lilás `#C7B6EE`, azul-céu `#BFD9F2`, as cinco cores dos elementos. NPCs: **Ammo, o Caravaneiro** (a última
caravana dele chega aqui), **Adda** (aposentado da Torre, cuida da Roda Celeste) e os Remadores (silhuetas de luz, sem
fala). O Pai da Grandeza **não aparece** (bíblia §0.2). Raide: **A Grande Treva** (porta no 100).

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
