# Rapadura — Conjuntos de armadura das Regiões 13–16

Quatro conjuntos de **2 peças** (peitoral + elmo), no molde da Armadura de Marfim dos Artífices (`Docs/Dungeons.md` §6):
assets em `Assets/Resources/Items/Sets/`, regras puras em `ArmorSetRules`, ligação ao combate em `ArmorSetBonuses`, textos
das peças em `ContentTexts` e dos avisos em `ArmorSetTexts`, ícones em `tools/icons/generate_item_icons.py`, testes em
`ArmorSetsTests`. Quebrada, a peça não conta para o conjunto.

> **Nomes:** nas fichas de `Docs/Mapas100.md` e em `Docs/Cities.md`, "Cais do Papiro", "Tijolo Azul" e "Cais de Luz" são
> também conjuntos de **casa** (móveis do `CatalogoCasa`). Estes aqui são os conjuntos de **equipamento** com o mesmo nome;
> os móveis continuam faltando. O "Selo dos Artífices" é o único que a ficha (mapa 90) pede como loot.

## Peças

| Conjunto | Região (nível) | Id | Peça | Raridade | Defesa | Atributo | Dur. | Valor |
|---|---|---|---|---|---|---|---|---|
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
| Cais do Papiro | Hora da Barca | ao matar, 25% de curar 5% da vida máxima |
| Tijolo Azul | Reflexo Vidrado | ao ser atingido por um bicho, 20% de devolver 35% do dano ao atacante (Terra) |
| Selo dos Artífices | Selo Reaberto | no acerto crítico, 25% de devolver 8% da mana máxima |
| Cais de Luz | Raio da Coluna | no acerto crítico, 15% de atingir até 4 outros inimigos a 5 m com 40% do dano do crítico (Sagrado) |

O reflexo e os raios são golpes comuns, nunca críticos, então nenhum bônus encadeia.

## Como obter

Só por drop, nunca em loja: cada abate de um dos **12 bichos próprios da região** tem **0,5%** de largar uma peça (metade
para cada), como peça procedural com afixos e opções, igual ao equipamento das tabelas de bioma. Vale a regra geral: bicho
mais de 20 níveis abaixo do herói não larga equipamento. O drop mora em `ArmorSetBonuses` e não no `_lootItem` do bicho,
porque esse campo é único e é o ouro de todos eles: **nenhum asset de inimigo foi alterado**. São ~200 abates por peça.

## Como os números foram medidos

Não há nível de item no jogo: quem segura o espólio é o nível da região.

- **Defesa:** a base do Marfim antes da faixa (peitoral 14, elmo 8) × `level_scaling.gear_defense_band` no meio da região,
  (1 + 0,005·(L−1)) / 1,07: nível 290 ×2,29, 330 ×2,47, 390 ×2,75, 460 ×3,08.
- **Atributos e bônus:** percentuais acompanham o herói e ficam perto do Marfim (crítico +4%, Toque Espectral 15% / 35%).
  A mana da curva é fixa em 100 mais Espírito, então os +30/+40 valem o mesmo em nível alto.
- **Valor:** peitoral 900 do Marfim (nível 40) proporcional ao nível do meio da região; elmo a 72% do peitoral, a mesma razão.
- **Pilha de poder:** `power_stack.py` lê só `Resources/Items/*.asset`, então a subpasta `Sets/` não entra na campanha
  (como a `Dungeon/`). O relatório e o `pytest tools/balance` seguem dentro do orçamento, mas **não medem estes conjuntos**:
  o orçamento hoje só vai até o nível 100.

## Falta

- Listar as peças no Bestiário (`BestiaryCatalog` só conhece o `_lootItem` e a tabela do bioma).
- Os conjuntos de casa de mesmo nome e os "conjuntos antigos" das fichas: "Vasos de Bronze" (Região 8),
  "Kurdalægon" (Região 12), "Gigantes" (Região 14) e o do jardim (Região 1).
