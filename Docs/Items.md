# Rapadura — Conjuntos de armadura das Regiões 1–16

Vinte conjuntos de **2 peças** (peitoral + elmo), no molde da Armadura de Marfim dos Artífices (`Docs/Dungeons.md` §6):
assets em `Assets/Resources/Items/Sets/`, regras puras em `ArmorSetRules` (`Early` = Regiões 1–4, `Middle` = 5–9, `High` =
10–12, `Far` = 13–16, `Ancient` = o segundo conjunto de cada Região 13–16, `All` = os vinte), ligação ao combate em `ArmorSetBonuses`,
textos das peças em `ContentTexts` e dos avisos em `ArmorSetTexts`, ícones em `tools/icons/generate_item_icons.py`, testes em
`ArmorSetsTests`, `EarlyArmorSetsTests`, `MiddleArmorSetsTests`, `HighArmorSetsTests` e `AncientArmorSetsTests`. Quebrada, a peça não
conta para o conjunto.

> **Nomes:** nas fichas de `Docs/Mapas100.md` e em `Docs/Cities.md`, "Cais do Papiro", "Tijolo Azul" e "Cais de Luz" são
> também conjuntos de **casa** (móveis do `CatalogoCasa`, "Móveis do Cais do Papiro" e outros). Estes aqui são os conjuntos
> de **equipamento** com o mesmo nome. Os das Regiões 5–9 têm nomes próprios, diferentes dos móveis da mesma região
> ("Móveis da Porta Suaíli", "Móveis da Casa nas Raízes", "Móveis do Terraço do Pamir", "Móveis do Pátio dos Nove Anéis",
> "Móveis da Tenda de Feltro", `Docs/Housing.md` §6.2). O mesmo vale para as Regiões 1–4 ("Móveis do Tear de Seda", "Móveis da
> Cisterna de Qocho", "Móveis do Rondavel", "Móveis da Casa na Rocha"). As Regiões 10 e 12 também ("Móveis do Ninho de
> Pedra", "Móveis do Palácio de Laca"). "Jardim de Mihrshah" é o "conjunto antigo do jardim" da ficha do mapa 06, "Kurdalægon"
> o da ficha do mapa 69, "Vasos de Bronze" o da ficha do mapa 46, o "Selo dos Artífices" o da ficha do mapa 90 e "Gigantes" o da ficha do
> mapa 82. Os segundos conjuntos das Regiões 13–16 também não colidem com os segundos móveis dessas regiões ("Móveis do Campo de
> Juncos", "Móveis do Relógio de Sol", "Móveis do Novelo", "Móveis da Barca da Lua").

## Peças

| Conjunto | Região (nível) | Id | Peça | Raridade | Defesa | Atributo | Dur. | Valor |
|---|---|---|---|---|---|---|---|---|
| Jardim de Mihrshah | 1 Bosque do Zarafshan (1–32) | `armor_jardim_chest` | Colete do Jardineiro de Mihrshah | Incomum | 10 | +10 mana máx. | 100 | 360 |
| | | `armor_jardim_helm` | Grinalda de Pétalas de Luz | Incomum | 5 | +2% crítico | 70 | 260 |
| Couro Rubro | 2 Areias Flamejantes (5–38) | `armor_rubro_chest` | Gibão de Couro Rubro | Incomum | 11 | +2% crítico | 105 | 470 |
| | | `armor_rubro_helm` | Turbante de Seda da Caravana | Incomum | 6 | +10 mana máx. | 75 | 340 |
| Pedra Seca | 3 Serra de Kaggen (9–44) | `armor_pedra_chest` | Couraça de Pedra Seca | Incomum | 12 | +15 mana máx. | 110 | 590 |
| | | `armor_pedra_helm` | Elmo de Granito em Espinha | Incomum | 7 | +3% crítico | 80 | 420 |
| Pavio do Limiar | 4 Grutas do Limiar (13–50) | `armor_pavio_chest` | Túnica Branca do Lamparineiro | Incomum | 13 | +3% crítico | 115 | 700 |
| | | `armor_pavio_helm` | Capuz de Tufo Rosa | Incomum | 8 | +15 mana máx. | 85 | 500 |
| Coral de Kilwa | 5 Costa das Monções (45–72) | `armor_coral_chest` | Couraça de Pedra de Coral | Rara | 17 | +20 mana máx. | 135 | 1 300 |
| | | `armor_coral_helm` | Elmo de Madrepérola | Rara | 10 | +4% crítico | 95 | 900 |
| Seda de Lótus | 6 Delta de Jhilpur (60–88) | `armor_lotus_chest` | Gibão de Seda de Lótus | Rara | 18 | +4% crítico | 140 | 1 700 |
| | | `armor_lotus_helm` | Capuz de Raiz Aérea | Rara | 10 | +20 mana máx. | 95 | 1 200 |
| Lã de Iaque | 7 Teto do Mundo (70–98) | `armor_iaque_chest` | Casaco de Lã de Iaque | Rara | 19 | +4% crítico | 140 | 1 900 |
| | | `armor_iaque_helm` | Gorro de Feltro do Pamir | Rara | 10 | +25 mana máx. | 100 | 1 400 |
| Vasos de Bronze | 8 Planície de Mingcheng (90–140) | `armor_bronze_chest` | Couraça de Bronze Antigo | Rara | 21 | +25 mana máx. | 150 | 2 600 |
| | | `armor_bronze_helm` | Elmo do Vaso Ritual | Rara | 12 | +4% crítico | 105 | 1 900 |
| Ouro dos Kurgans | 9 Estepe dos Nartes (100–150) | `armor_kurgan_chest` | Couraça de Escamas de Ouro | Rara | 21 | +4% crítico | 155 | 2 800 |
| | | `armor_kurgan_helm` | Elmo de Feltro e Ouro | Rara | 12 | +25 mana máx. | 105 | 2 000 |
| Sino de Vento | 10 Picos de Farnkesh (140–200) | `armor_sino_chest` | Gibão de Corda de Iaque | Rara | 24 | +25 mana máx. | 170 | 3 800 |
| | | `armor_sino_helm` | Elmo do Sino de Vento | Rara | 14 | +4% crítico | 115 | 2 750 |
| Prata da Aurora | 11 Mar Gelado (180–240) | `armor_aurora_chest` | Parka de Pele de Foca | Rara | 27 | +4% crítico | 180 | 4 700 |
| | | `armor_aurora_helm` | Capuz de Prata da Aurora | Rara | 15 | +30 mana máx. | 125 | 3 400 |
| Kurdalægon | 12 Arquipélago de Hinokura (220–280) | `armor_ferro_chest` | Couraça de Ferro Celeste | Rara | 29 | +5% vida máx. | 190 | 5 600 |
| | | `armor_ferro_helm` | Elmo da Bigorna de Kurdalægon | Rara | 17 | +30 mana máx. | 130 | 4 050 |
| Cais do Papiro | 13 Rio das Doze Horas (260–320) | `armor_papiro_chest` | Couraça de Papiro Trançado | Rara | 32 | +5% vida máx. | 200 | 6 500 |
| | | `armor_papiro_helm` | Touca de Junco do Remador | Rara | 18 | +30 mana máx. | 140 | 4 700 |
| Tijolo Azul | 14 Margens da Mistura (300–360) | `armor_tijolo_chest` | Peitoral de Azulejos Vidrados | Rara | 35 | +6% vida máx. | 220 | 7 400 |
| | | `armor_tijolo_helm` | Elmo do Leão de Azulejo | Rara | 20 | +3% crítico | 150 | 5 400 |
| Selo dos Artífices | 15 Limiar Profundo (340–440) | `armor_selo_chest` | Cota dos Selos Rompidos | Épica | 39 | +40 mana máx. | 240 | 8 800 |
| | | `armor_selo_helm` | Elmo do Último Selo | Épica | 22 | +4% crítico | 165 | 6 400 |
| Cais de Luz | 16 Reinos do Céu (420–500) | `armor_luz_chest` | Couraça de Luz Sólida | Épica | 43 | +5% crítico | 260 | 10 400 |
| | | `armor_luz_helm` | Diadema dos Remadores de Luz | Épica | 25 | +40 mana máx. | 180 | 7 500 |
| Escamas de Mehen | 13 Rio das Doze Horas (260–320) | `armor_mehen_chest` | Couraça de Escamas de Mehen | Rara | 31 | +30 mana máx. | 200 | 6 500 |
| | | `armor_mehen_helm` | Elmo da Serpente Enrolada | Rara | 17 | +5% vida máx. | 140 | 4 700 |
| Gigantes | 14 Margens da Mistura (300–360) | `armor_gigantes_chest` | Couraça das Tábuas do Sonho | Rara | 34 | +3% crítico | 220 | 7 400 |
| | | `armor_gigantes_helm` | Elmo de Ohya | Rara | 19 | +6% vida máx. | 150 | 5 400 |
| Cinco Luzes | 15 Limiar Profundo (340–440) | `armor_cinco_chest` | Couraça das Cinco Luzes | Épica | 38 | +4% crítico | 240 | 8 800 |
| | | `armor_cinco_helm` | Elmo do Homem Primordial | Épica | 21 | +40 mana máx. | 165 | 6 400 |
| Coluna da Glória | 16 Reinos do Céu (420–500) | `armor_coluna_chest` | Manto da Coluna da Glória | Épica | 42 | +40 mana máx. | 260 | 10 400 |
| | | `armor_coluna_helm` | Coroa do Homem Perfeito | Épica | 24 | +5% crítico | 180 | 7 500 |

## Bônus de 2 peças

| Conjunto | Bônus | Regra |
|---|---|---|
| Jardim de Mihrshah | Orvalho do Jardim | ao matar, 15% de curar 3% da vida máxima |
| Couro Rubro | Areia que Queima | ao ser atingido por um bicho, 10% de devolver 20% do dano ao atacante (Fogo) |
| Pedra Seca | Rajada do Kaggen | no acerto crítico, 10% de atingir até 2 outros inimigos a 3 m com 20% do dano do crítico (Vento) |
| Pavio do Limiar | Pavio Aceso | ao matar, 15% de devolver 4% da mana máxima |
| Coral de Kilwa | Coral que Fecha | ao ser atingido por um bicho, 15% de curar 30% do dano recebido |
| Seda de Lótus | Seiva do Lótus | ao matar, 20% de devolver 5% da mana máxima |
| Lã de Iaque | Fôlego do Pamir | no acerto crítico, 20% de curar 3% da vida máxima |
| Vasos de Bronze | Gongo de Bronze | ao ser atingido por um bicho, 10% de atingir até 3 inimigos a 4 m do herói com 20% do dano recebido (Terra) |
| Ouro dos Kurgans | Galope dos Nartes | no acerto crítico, 10% de atingir até 3 outros inimigos a 4 m com 30% do dano do crítico (Físico) |
| Sino de Vento | Badalada das Ermidas | no acerto crítico, 12% de atingir até 3 outros inimigos a 4,5 m com 35% do dano do crítico (Vento) |
| Prata da Aurora | Calor da Lanterna | ao matar, 22% de curar 5% da vida máxima |
| Kurdalægon | Brasa da Bigorna | ao ser atingido por um bicho, 20% de devolver 30% do dano ao atacante (Fogo) |
| Cais do Papiro | Hora da Barca | ao matar, 25% de curar 5% da vida máxima |
| Tijolo Azul | Reflexo Vidrado | ao ser atingido por um bicho, 20% de devolver 35% do dano ao atacante (Terra) |
| Selo dos Artífices | Selo Reaberto | no acerto crítico, 25% de devolver 8% da mana máxima |
| Cais de Luz | Raio da Coluna | no acerto crítico, 15% de atingir até 4 outros inimigos a 5 m com 40% do dano do crítico (Sagrado) |
| Escamas de Mehen | Anel de Mehen | ao ser atingido por um bicho, 18% de curar 35% do dano recebido |
| Gigantes | Tremor dos Gigantes | ao ser atingido por um bicho, 15% de atingir até 4 inimigos a 4,5 m do herói com 30% do dano recebido (Terra) |
| Cinco Luzes | Luz que Volta | ao matar, 20% de curar 4% da vida máxima e devolver 4% da mana máxima |
| Coluna da Glória | Subida da Coluna | no acerto crítico, 20% de curar 5% da vida máxima |

O reflexo, a areia, o gongo, o tremor, a rajada, o galope e os raios são golpes comuns, nunca críticos, então nenhum bônus encadeia. Com dois espaços (peito
e cabeça) só um conjunto fica completo por vez.

## Como obter

Só por drop, nunca em loja: cada abate de um dos **bichos próprios da região** pode largar uma peça (metade para cada), como
peça procedural com afixos e opções, igual ao equipamento das tabelas de bioma. Vale a regra geral: bicho mais de 20 níveis
abaixo do herói não larga equipamento. O drop mora em `ArmorSetBonuses` e não no `_lootItem` do bicho, porque esse campo é
único e é o ouro de todos eles: **nenhum asset de inimigo foi alterado**.

- **Regiões 13–16:** os 12 bichos próprios de cada região, **0,5%** por abate (`DropChance`), ~200 abates por peça.
- **Segundos conjuntos das Regiões 13–16** (`ArmorSetRules.Ancient`): os 3 bichos próprios que o primeiro conjunto da região deixou
  de fora, **2%** por abate (`AncientDropChance`), ~50 abates por peça. São 3 no meio de 21–25 espécies: a 0,5% o par custaria ~5 000
  abates caçando a região; a 2% fica em 1 050–1 250, dentro dos 1 500 do orçamento.
  - Escamas de Mehen: Filho de Apep, Areia Movediça, Ganso Ladrão.
  - Gigantes: Homem-Escorpião, Lince do Cedro, Abelha-Resina.
  - Cinco Luzes: Touro de Cinza, Morcego-Lâmina, Enguia Grande.
  - Coluna da Glória: Gota de Treva, Faísca Confusa, Corda de Fumaça, Última Sombra e Vento do Alto (desde a décima oitava leva de bichos, 2026-11-13: as dez espécies novas do Céu levavam o par a 1 500 abates; com cinco, 900). O Cais de Luz ganhou a Faísca Teimosa pelo mesmo motivo (1 385 abates).
- **Regiões 10–12:** os 12 bichos próprios de cada região, **0,75%** por abate (`HighDropChance`), ~133 abates por peça. Têm 12
  próprios como o norte, mas no meio de 17–32 espécies (os Picos de Farnkesh são os que mais emprestam): a 0,5% o Sino de Vento
  passaria dos 1 500 abates do orçamento (1 600); a 0,75% fica em 570–1 070 caçando a região.
  - Sino de Vento: Bode de Fumaça, Abelha Dourada, Pássaro-Flauta Apagado, Raposa da Vinha, Gralha-Alpina, Takin Apagado,
    Cabra-Azul, Morcego-Vela, Ushabti de Corda, Gato-de-Pallas Apagado, Águia da Tempestade, Harpia de Fumaça.
  - Prata da Aurora: Foca-Leopardo Apagada, Morsa de Fumaça, Gaivota-do-Gelo, Raposa-do-Ártico Ladra, Urso-Polar Apagado,
    Rena de Barro, Lebre-Pedra, Ave-Graveto do gelo, Mamute de Baixo da Terra, Lobo-do-Mar, Lúcio Gigante, Foca-Sombra.
  - Kurdalægon: Caranguejo-Vulcão, Lagarto de Lava, Gaivota-Brasa, Shisa Apagada, Tanuki Ladrão, Tibicena, Lagarto-Gigante de Lava,
    Peixe-Lâmina, Coral de Brasa, Ushabti de Laca, Ogro-Fornalha, Gigante de Escória. A Salamandra Devoradora e a Bomba
    Vulcânica, também próprias, ficaram de fora pelo teto de 12 por conjunto; a Chama Faminta (70, 75, 85) não é de conjunto.
- **Regiões 5–9:** os bichos próprios de cada região (Docs/Status_100Mapas.md), **1%** por abate (`MiddleDropChance`), ~100
  abates por peça. São só 6–9 por região, no meio de muitos emprestados; a 0,5% o par passaria dos 1 500 abates do orçamento
  da economia (`tools/balance/economy.py`, `BUDGET_SET_KILLS`); a 1% fica em 780–975 abates caçando a região inteira.
  - Coral de Kilwa: Gaivota Ladra, Caranguejo-dos-Coqueiros, Crocodilo do Mangue, Leão do Capim, Rinoceronte-de-Pó, Moreia
    Apagada, Ouriço de Fumaça, Ave-Relâmpago, Caranguejo-Violinista.
  - Seda de Lótus: Macaco Ladrão, Lontra Ladra, Píton Apagada, Trol Apagado, Garça-Lança Apagada, Sanguessuga do Lodo.
  - Lã de Iaque: Urso-Pardo do Pamir, Iaque Selvagem, Irbis, Almas, Marmota Gigante, Gralha-de-Bico-Vermelho, Abutre-Barbudo
    Apagado, Lobo de Gelo.
  - Vasos de Bronze: Leão de Pedra, Raposa de Cauda Lanterna, Soldado de Terracota, Tartaruga de Pedra, Búfalo de Lama,
    Filhote de Nian, Carpa-Dragão Apagada, Cavalo de Terracota.
  - Ouro dos Kurgans: Grifo Jovem, Saiga, Lobo de Bronze, Gigante Sonâmbulo, Besouro-Casca, Ochokochi, Cisne de Feltro.
- **Regiões 1–4 (as regiões-sede):** os bichos próprios de cada uma (os de base do bioma, as espécies da noite e os novos de
  `EarlyBeasts`), **0,5%** por abate (`EarlyDropChance`), ~200 abates por peça. Aqui quase tudo o que anda na região é dela,
  e o herói passa os níveis 1–50 caçando ali: a 1% o par sairia bem antes do chefe do mundo. A economia mede 600–1 030 abates
  caçando a região inteira (só conta as espécies escritas por extenso no arquivo da região, então é o caso pior).
  - Jardim de Mihrshah: Lobo do Tugai, Aranha-Tecelã Apagada, Javali do Lodo, Coruja-de-Fumaça, Mardagayl, Cegonha Apagada,
    Sapo-Bolha do Lodo, Mosquito de Fumaça, Mariposa de Seda, Pombo-do-Louro, Lagarto-Gigante do Louro, Pavão de Fumaça.
  - Couro Rubro: Escorpião das Areias Flamejantes, Abutre de Brasa, Lanterna Quebrada dos Artífices, Enxame de Faíscas Presas,
    Chacal, Cobra-de-Areia, Orc Coroado de Āz, Lagarto-Espinho.
  - Pedra Seca: Ushabti de Pedra, Pedra que Rola, Gavião-de-Penacho, Ave-Graveto de Omöl, Neblina do Vento Ruim, Babuíno
    Apagado, Lince Apagado, Borrão, Gralha de Pedra.
  - Pavio do Limiar: Enxame de Morcegos do Limiar, Ubır (o Morcegão Bebe-Luz), Peixe-Lanterna Apagado, Lodo de Gruta,
    Corvo-da-Treva, Ubır Jovem.

Um bicho só pertence a um conjunto. Os de base das Regiões 1–4 (lobo, aranha, javali, escorpião, ushabti, morcegos, Ubır,
Ave-Graveto) andam emprestados por quase toda a ilha e, em qualquer lugar, largam a peça da sua região de origem (com a regra
dos 20 níveis valendo). Alguns (a Gaivota Ladra, o Macaco Ladrão, o Filhote de Nian, o Lobo de Bronze, o Gigante
Sonâmbulo) ainda fazem papel de emprestado em outras regiões e, lá, largam a peça do conjunto da sua região de origem.
`MiddleArmorSetsTests` e `EarlyArmorSetsTests` conferem que cada bicho anda na sua região, nos níveis dela.

## Como os números foram medidos

Não há nível de item no jogo: quem segura o espólio é o nível da região.

- **Defesa:** a base do Marfim antes da faixa (peitoral 14, elmo 8) × `level_scaling.gear_defense_band` no meio da região,
  (1 + 0,005·(L−1)) / 1,07: nível 58 ×1,20, 74 ×1,28, 84 ×1,32, 115 ×1,47, 125 ×1,51, 290 ×2,29, 330 ×2,47, 390 ×2,75,
  460 ×3,08. Nas Regiões 5–9 a defesa nunca cai de uma região para a seguinte (as faixas se sobrepõem: 7 e 9 repetem o elmo
  e o peitoral da anterior) e fica sempre entre o Marfim (16/9) e o Cais do Papiro (32/18). O elmo da Lã de Iaque ficou em
  10, não 11, para o herói parado diante de Hedammu (72) não passar dos 40 s do modelo do doc no `power_stack.py`.
- **Atributos e bônus:** percentuais acompanham o herói e ficam perto do Marfim (crítico +4%, Toque Espectral 15% / 35%).
  A mana da curva é fixa em 100 mais Espírito, então os +20/+25/+30/+40 valem o mesmo em nível alto. Os conjuntos das
  Regiões 5–9 só dão crítico (no máximo +4%, o do Marfim) e mana (no máximo +25, abaixo dos +30 do Cais do Papiro); vida %
  ficou para o norte, para não alongar as lutas de chefe da faixa 72–150.
- **Bônus das Regiões 5–9:** o valor esperado por gatilho fica abaixo do bônus de mesmo tipo do norte (chance × fração):
  Coral que Fecha 4,5% contra os 7% do Reflexo Vidrado; Seiva do Lótus 1% contra 1,25% da Hora da Barca; Fôlego do Pamir
  0,6% contra 2% do Selo Reaberto; Gongo de Bronze 6% (3 alvos) contra 7% do Reflexo; Galope dos Nartes 9% (3 alvos a 4 m)
  contra 24% do Raio da Coluna (4 alvos a 5 m). `MiddleArmorSetsTests` trava essas desigualdades.
- **Valor:** peitoral 900 do Marfim (nível 40) proporcional ao nível do meio da região; elmo a 72% do peitoral, a mesma razão.
- **Regiões 1–4:** **Incomuns**, uma raridade abaixo das Raras do meio. A defesa sobe um ponto por região entre o couro das lojas
  (peitoral 8, elmo 4) e o Marfim (16/9): 10/5, 11/6, 12/7, 13/8, sempre abaixo do Coral de Kilwa (17/10). Atributos: crítico +2%
  ou +3% e mana +10 ou +15, abaixo do menor valor do mesmo atributo nos conjuntos do meio (+4%, +20). Bônus com valor esperado por
  gatilho abaixo do bônus de mesmo tipo do meio: Orvalho do Jardim 0,45% contra 1% da Seiva do Lótus (e 0,6% do Fôlego do Pamir);
  Areia que Queima 2% contra 4,5% do Coral que Fecha; Rajada do Kaggen 4% (2 alvos a 3 m) contra 9% do Galope dos Nartes; Pavio
  Aceso 0,6% contra 1% da Seiva do Lótus. Durabilidade 100–115 / 70–85. `EarlyArmorSetsTests` trava essas desigualdades. Abaixo
  do nível 100, então a seção "Faixas 100–500" (`level_500.py`) não precisa deles; a economia (`economy.py`) lista todos.
- **Regiões 10–12:** **Raras**, entre o Ouro dos Kurgans (21/12) e o Cais do Papiro (32/18): defesa pela mesma faixa de nível
  (peitoral 14 × 1,72/1,91/2,10 no meio da região) = 24/14, 27/15, 29/17. Atributos dentro do que o norte dá (crítico +4%, mana
  +25/+30, vida +5%; a vida % entra aqui porque a faixa já é 220+). Valor 900 × nível do meio / 40; durabilidade 170–190 / 115–130.
  Bônus com valor esperado entre o do meio e o do norte, do mesmo tipo: Badalada das Ermidas 12,6% (3 alvos a 4,5 m) entre o
  Galope dos Nartes (9%) e o Raio da Coluna (24%); Calor da Lanterna 1,1% entre a Seiva do Lótus (1%) e a Hora da Barca (1,25%);
  Brasa da Bigorna 6% entre o Coral que Fecha (4,5%) e o Reflexo Vidrado (7%). `HighArmorSetsTests` trava essas desigualdades.
- **Segundos conjuntos das Regiões 13–16:** uma escolha ao lado, nunca um passo acima. Cada peça tem 1 de defesa a menos que a peça
  do primeiro conjunto da região no mesmo espaço (31/17, 34/19, 38/21, 42/24), a mesma raridade, o mesmo valor e a mesma durabilidade,
  e os dois atributos do primeiro conjunto trocados de espaço (o Cais do Papiro dá vida no peito e mana na cabeça; as Escamas de Mehen,
  mana no peito e vida na cabeça). Bônus com valor esperado acima do bônus de mesmo tipo do meio e no máximo o do norte: Anel de Mehen
  6,3% entre o Coral que Fecha (4,5%) e o Reflexo Vidrado (7%); Tremor dos Gigantes 18% (4 alvos a 4,5 m) entre o Gongo de Bronze (6%)
  e o Raio da Coluna (24%); Luz que Volta 0,8% de vida e 0,8% de mana, abaixo da Hora da Barca (1,25%) e da Seiva do Lótus (1%) cada;
  Subida da Coluna 1% entre o Fôlego do Pamir (0,6%) e a Hora da Barca (1,25%). `AncientArmorSetsTests` trava essas regras.
- **Pilha de poder:** `power_stack.py` lê só `Resources/Items/*.asset`, então a subpasta `Sets/` não entra na campanha
  (como a `Dungeon/`). A seção "Faixas 100–500" (`level_500.py`) conhece os doze de 5–16: o herói de referência veste o
  Ouro dos Kurgans de 100 a 139, o Sino de Vento de 140, a Prata da Aurora de 180 e o Kurdalægon de 220 a 259 (a Brasa da Bigorna
  entra como espinho, como o Reflexo Vidrado); as lutas de comuns e chefes não mudam de faixa e nada novo sai do orçamento.
  Os segundos conjuntos de 13–16 entram na lista com o nível de entrada da região, mas o herói de referência nunca os escolhe (a
  defesa é menor e os atributos são os mesmos), e `test_level_500.py` confere isso. A economia (`economy.py`) lista os vinte com os
  abates esperados.

## Falta

- Listar as peças no Bestiário (`BestiaryCatalog` só conhece o `_lootItem` e a tabela do bioma).
- O bônus do chefe do Sonho da Destruição (82) ainda é o peitoral do Tijolo Azul, não o dos Gigantes (asset do inimigo, não mexido).
- Uma conquista para os conjuntos de equipamento das Regiões 1–12 e para os segundos de 13–16 (como a `ach_house_far_sets` dos móveis): os textos de
  conquista moram em `UI/Achievements`.
