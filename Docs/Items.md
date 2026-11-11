# Rapadura — Conjuntos de armadura das Regiões 5–9 e 13–16

Nove conjuntos de **2 peças** (peitoral + elmo), no molde da Armadura de Marfim dos Artífices (`Docs/Dungeons.md` §6):
assets em `Assets/Resources/Items/Sets/`, regras puras em `ArmorSetRules` (`Middle` = Regiões 5–9, `Far` = 13–16, `All` =
os nove), ligação ao combate em `ArmorSetBonuses`, textos das peças em `ContentTexts` e dos avisos em `ArmorSetTexts`, ícones
em `tools/icons/generate_item_icons.py`, testes em `ArmorSetsTests` e `MiddleArmorSetsTests`. Quebrada, a peça não conta para
o conjunto.

> **Nomes:** nas fichas de `Docs/Mapas100.md` e em `Docs/Cities.md`, "Cais do Papiro", "Tijolo Azul" e "Cais de Luz" são
> também conjuntos de **casa** (móveis do `CatalogoCasa`, "Móveis do Cais do Papiro" e outros). Estes aqui são os conjuntos
> de **equipamento** com o mesmo nome. Os das Regiões 5–9 têm nomes próprios, diferentes dos móveis da mesma região
> ("Móveis da Porta Suaíli", "Móveis da Casa nas Raízes", "Móveis do Terraço do Pamir", "Móveis do Pátio dos Nove Anéis",
> "Móveis da Tenda de Feltro", `Docs/Housing.md` §6.2). "Vasos de Bronze" é o "conjunto antigo" da ficha do mapa 46 e o
> "Selo dos Artífices" o da ficha do mapa 90.

## Peças

| Conjunto | Região (nível) | Id | Peça | Raridade | Defesa | Atributo | Dur. | Valor |
|---|---|---|---|---|---|---|---|---|
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
| Cais do Papiro | 13 Rio das Doze Horas (260–320) | `armor_papiro_chest` | Couraça de Papiro Trançado | Rara | 32 | +5% vida máx. | 200 | 6 500 |
| | | `armor_papiro_helm` | Touca de Junco do Remador | Rara | 18 | +30 mana máx. | 140 | 4 700 |
| Tijolo Azul | 14 Margens da Mistura (300–360) | `armor_tijolo_chest` | Peitoral de Azulejos Vidrados | Rara | 35 | +6% vida máx. | 220 | 7 400 |
| | | `armor_tijolo_helm` | Elmo do Leão de Azulejo | Rara | 20 | +3% crítico | 150 | 5 400 |
| Selo dos Artífices | 15 Limiar Profundo (340–440) | `armor_selo_chest` | Cota dos Selos Rompidos | Épica | 39 | +40 mana máx. | 240 | 8 800 |
| | | `armor_selo_helm` | Elmo do Último Selo | Épica | 22 | +4% crítico | 165 | 6 400 |
| Cais de Luz | 16 Reinos do Céu (420–500) | `armor_luz_chest` | Couraça de Luz Sólida | Épica | 43 | +5% crítico | 260 | 10 400 |
| | | `armor_luz_helm` | Diadema dos Remadores de Luz | Épica | 25 | +40 mana máx. | 180 | 7 500 |

## Bônus de 2 peças

| Conjunto | Bônus | Regra |
|---|---|---|
| Coral de Kilwa | Coral que Fecha | ao ser atingido por um bicho, 15% de curar 30% do dano recebido |
| Seda de Lótus | Seiva do Lótus | ao matar, 20% de devolver 5% da mana máxima |
| Lã de Iaque | Fôlego do Pamir | no acerto crítico, 20% de curar 3% da vida máxima |
| Vasos de Bronze | Gongo de Bronze | ao ser atingido por um bicho, 10% de atingir até 3 inimigos a 4 m do herói com 20% do dano recebido (Terra) |
| Ouro dos Kurgans | Galope dos Nartes | no acerto crítico, 10% de atingir até 3 outros inimigos a 4 m com 30% do dano do crítico (Físico) |
| Cais do Papiro | Hora da Barca | ao matar, 25% de curar 5% da vida máxima |
| Tijolo Azul | Reflexo Vidrado | ao ser atingido por um bicho, 20% de devolver 35% do dano ao atacante (Terra) |
| Selo dos Artífices | Selo Reaberto | no acerto crítico, 25% de devolver 8% da mana máxima |
| Cais de Luz | Raio da Coluna | no acerto crítico, 15% de atingir até 4 outros inimigos a 5 m com 40% do dano do crítico (Sagrado) |

O reflexo, o gongo, o galope e os raios são golpes comuns, nunca críticos, então nenhum bônus encadeia. Com dois espaços (peito
e cabeça) só um conjunto fica completo por vez.

## Como obter

Só por drop, nunca em loja: cada abate de um dos **bichos próprios da região** pode largar uma peça (metade para cada), como
peça procedural com afixos e opções, igual ao equipamento das tabelas de bioma. Vale a regra geral: bicho mais de 20 níveis
abaixo do herói não larga equipamento. O drop mora em `ArmorSetBonuses` e não no `_lootItem` do bicho, porque esse campo é
único e é o ouro de todos eles: **nenhum asset de inimigo foi alterado**.

- **Regiões 13–16:** os 12 bichos próprios de cada região, **0,5%** por abate (`DropChance`), ~200 abates por peça.
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

Um bicho só pertence a um conjunto. Alguns (a Gaivota Ladra, o Macaco Ladrão, o Filhote de Nian, o Lobo de Bronze, o Gigante
Sonâmbulo) ainda fazem papel de emprestado em outras regiões e, lá, largam a peça do conjunto da sua região de origem.
`MiddleArmorSetsTests` confere que cada bicho anda na sua região, nos níveis dela.

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
- **Pilha de poder:** `power_stack.py` lê só `Resources/Items/*.asset`, então a subpasta `Sets/` não entra na campanha
  (como a `Dungeon/`). A seção "Faixas 100–500" (`level_500.py`) já conhece os nove: de 100 a 259 o herói de referência
  veste o Ouro dos Kurgans no lugar do Marfim; as lutas de comuns e chefes não mudam de faixa e nada novo sai do orçamento.
  A economia (`economy.py`) lista os nove com os abates esperados.

## Falta

- Listar as peças no Bestiário (`BestiaryCatalog` só conhece o `_lootItem` e a tabela do bioma).
- Os "conjuntos antigos" das fichas que ainda não existem: "Kurdalægon" (Região 12), "Gigantes" (Região 14) e o do jardim
  (Região 1).
- Uma conquista para os conjuntos de equipamento das Regiões 5–9 (como a `ach_house_far_sets` dos móveis): os textos de
  conquista moram em `UI/Achievements`.
