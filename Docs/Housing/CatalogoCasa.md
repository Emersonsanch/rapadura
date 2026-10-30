# Catálogo de itens de casa — Rapadura

> Gerado a partir de uma lista curada (cada nome escrito à mão). Design do sistema em [`Docs/Housing.md`](../Housing.md). Tema: a Ilha da Aurora (ver [`Docs/WorldBible_Maniqueismo.md`](../WorldBible_Maniqueismo.md) §3.20); ids, números e colunas não mudaram no re-tema.
> **Este é o conjunto inicial de lançamento da casa, não a lista de todos os itens possíveis.** Novos itens entram por temporada e evento, sempre com ID novo (`casa_<categoria>_<nnn>` continuando a numeração); um ID nunca é reaproveitado.

**Total: 580 itens únicos.** Checagem automática: 580 IDs distintos, 580 nomes distintos, nenhuma duplicata (inclusive ignorando acento, maiúscula e pontuação).

| Raridade | Itens | % |
|---|---|---|
| Comum | 225 | 38,8% |
| Incomum | 149 | 25,7% |
| Raro | 126 | 21,7% |
| Épico | 64 | 11,0% |
| Lendário | 16 | 2,8% |
| **Total** | **580** | 100% |

| Categoria | Itens |
|---|---|
| [Móveis de sala](#sala) | 45 |
| [Quarto](#quarto) | 35 |
| [Cozinha](#cozinha) | 44 |
| [Banheiro](#banheiro) | 21 |
| [Quintal e horta](#quintal) | 39 |
| [Curral e animais](#curral) | 27 |
| [Iluminação](#luz) | 30 |
| [Paredes e quadros](#parede) | 38 |
| [Tapetes](#tapete) | 23 |
| [Cortinas](#cortina) | 18 |
| [Plantas](#planta) | 31 |
| [Instrumentos musicais](#musica) | 23 |
| [Brinquedos](#brinquedo) | 31 |
| [Decoração regional por região](#regional) | 48 |
| [Troféus de chefes](#trofeu) | 24 |
| [Itens sazonais](#sazonal) | 45 |
| [Raros e lendários com história de família](#familia) | 27 |
| [Utilitários (baús, prateleiras, bancadas)](#util) | 31 |

| Origem | Itens |
|---|---|
| Loja | 376 |
| Drop de inimigo/chefe | 80 |
| Missão (regional, pessoal, diária ou da casa) | 34 |
| Conquista | 24 |
| Evento sazonal | 48 |
| Crafting | 12 |
| Segredo do mapa | 6 |

## Como ler

- **Raridade:** os 5 níveis reais do `ItemRarity` (Comum, Incomum, Raro, Épico, Lendário).
- **Preço:** em ouro (`item_gold`), só para itens de loja. Lendário nunca é vendido. "—" = não se compra. Itens de evento mostram o preço em **oferendas** (moeda de evento, ver Housing.md §8.3). Faixas: Comum 8–30, Incomum 35–90, Raro 100–250, Épico 300–700 (ver Housing.md §10).
- **Conforto:** pontos somados ao conforto da casa (Comum 1–2, Incomum 3–4, Raro 5–6, Épico 8–9, Lendário 12–15; utilitários valem metade).
- **Encaixe:** Piso, Parede, Teto, Superfície (em cima de mesa/estante/cômoda) ou Quintal.
- **Origem "Drop":** o item entra no sorteio de itens de casa daquele inimigo (tabela de chances em Housing.md §7.2). Itens de loja também podem cair de qualquer inimigo do bioma da cidade da loja, nunca acima de Raro.

<a id="sala"></a>
## Móveis de sala (45)

| ID | Nome | Raridade | Origem | Preço (ouro) | Conforto | Conjunto | Encaixe |
|---|---|---|---|---|---|---|---|
| `casa_sala_001` | Sofá de vime do iwan | Comum | Loja: Marcenaria do Mestre Omóforo | 24 | 1 | Iwan da Vó Edna | Piso |
| `casa_sala_002` | Banco de madeira comprido | Comum | Loja: Marcenaria do Mestre Omóforo | 27 | 2 | — | Piso |
| `casa_sala_003` | Cadeira de balanço do Vô Jorge | Raro | Missão: A Reunião (`quest_convergence_the_gathering`) | — | 5 | Iwan da Vó Edna | Piso |
| `casa_sala_004` | Mesa de centro de tronco de choupo | Comum | Loja: Marcenaria do Mestre Omóforo | 11 | 1 | Tugai do Zarafshan | Piso |
| `casa_sala_005` | Estante de tábuas de nogueira velha | Incomum | Loja: Marcenaria do Mestre Omóforo | 80 | 4 | — | Piso |
| `casa_sala_006` | Cadeira de cordas de seda colorida | Comum | Loja: Bazar da Nana | 15 | 2 | Bazar da Nana | Piso |
| `casa_sala_007` | Poltrona de couro curtido | Incomum | Loja: Marcenaria do Mestre Omóforo | 45 | 3 | Caravana do Vô Jorge | Piso |
| `casa_sala_008` | Banquinho de três pés | Comum | Loja: Marcenaria do Mestre Omóforo | 12 | 2 | — | Piso |
| `casa_sala_009` | Rede de seda bordada do iwan | Raro | Loja: Costureira Mestra Narisah | 205 | 5 | Iwan da Vó Edna | Parede |
| `casa_sala_010` | Rede de algodão em ikat | Comum | Loja: Costureira Mestra Narisah | 29 | 2 | — | Parede |
| `casa_sala_011` | Mesa de jantar de nogueira | Raro | Loja: Marcenaria do Mestre Omóforo | 170 | 5 | — | Piso |
| `casa_sala_012` | Cristaleira de vidro soprado | Épico | Loja: Marcenaria do Mestre Omóforo | 580 | 8 | — | Piso |
| `casa_sala_013` | Aparador com toalhinha de suzani | Incomum | Loja: Bazar de Linhas da Mariam | 70 | 4 | Iwan da Vó Edna | Piso |
| `casa_sala_014` | Rádio da Vó Edna remontado | Épico | Missão pessoal do Ícaro (`quest_icaro_ancient_gears`) | — | 8 | Oficina do Ícaro | Superfície |
| `casa_sala_015` | Televisão de tubo com antena de fio de cobre | Incomum | Loja: Caravana do Ammo | 85 | 3 | — | Piso |
| `casa_sala_016` | Mocho de couro de camelo | Comum | Loja: Bazar da Nana | 11 | 1 | Caravana do Vô Jorge | Piso |
| `casa_sala_017` | Baú-banco de cedro entalhado | Incomum | Loja: Marcenaria do Mestre Omóforo | 75 | 3 | — | Piso |
| `casa_sala_018` | Mesa de gamão com quatro tamboretes | Incomum | Loja: Marcenaria do Mestre Omóforo | 50 | 4 | — | Piso |
| `casa_sala_019` | Cadeira de lona de caravana | Comum | Loja: Costureira Mestra Narisah | 27 | 1 | — | Piso |
| `casa_sala_020` | Namoradeira de ferro batido | Raro | Loja: Forja do Mestre Mihr | 250 | 5 | — | Piso |
| `casa_sala_021` | Sofá de três lugares de ikat | Incomum | Loja: Costureira Mestra Narisah | 45 | 3 | Bazar da Nana | Piso |
| `casa_sala_022` | Almofada de suzani florida | Comum | Loja: Bazar de Linhas da Mariam | 22 | 2 | Bazar da Nana | Superfície |
| `casa_sala_023` | Almofada de retalhos de seda | Comum | Loja: Bazar de Linhas da Mariam | 12 | 1 | Iwan da Vó Edna | Superfície |
| `casa_sala_024` | Pufe de couro trançado | Incomum | Loja: Bazar da Nana | 50 | 4 | Caravana do Vô Jorge | Piso |
| `casa_sala_025` | Mesa de canto de carretel de seda | Comum | Loja: Marcenaria do Mestre Omóforo | 14 | 1 | — | Piso |
| `casa_sala_026` | Cadeira de vime da Nana | Incomum | Loja: Bazar da Nana | 65 | 3 | — | Piso |
| `casa_sala_027` | Nicho de lâmpada de madeira | Raro | Loja: Marcenaria do Mestre Omóforo | 100 | 6 | Luz da Maria | Parede |
| `casa_sala_028` | Mesinha do bule de chá | Incomum | Loja: Caravana do Ammo | 70 | 4 | — | Piso |
| `casa_sala_029` | Telefone de disco de cobre | Incomum | Loja: Caravana do Ammo | 55 | 3 | — | Superfície |
| `casa_sala_030` | Vitrola de maleta | Raro | Loja: Casa de Música do Mestre Xroshtag | 125 | 5 | — | Superfície |
| `casa_sala_031` | Revisteiro de junco | Comum | Loja: Bazar da Nana | 24 | 1 | Tugai do Zarafshan | Piso |
| `casa_sala_032` | Cabideiro de galhos de tamargueira | Comum | Loja: Marcenaria do Mestre Omóforo | 26 | 1 | Tugai do Zarafshan | Piso |
| `casa_sala_033` | Bandeja de cobre com tigelinhas de chá | Comum | Loja: Forja do Mestre Mihr | 9 | 2 | — | Superfície |
| `casa_sala_034` | Porta-retrato dos cinco primos | Raro | Conquista: Juntos, ou Nada Feito | — | 5 | Iwan da Vó Edna | Superfície |
| `casa_sala_035` | Cadeira de couro de caravaneiro | Raro | Loja: Caravana do Ammo | 115 | 6 | Caravana do Vô Jorge | Piso |
| `casa_sala_036` | Estante de rolos de hinos iluminados | Incomum | Loja: Bazar de Linhas da Mariam | 40 | 4 | Iluminuras e Papel Recortado | Piso |
| `casa_sala_037` | Banquinho de engraxate do bazar | Comum | Loja: Bazar da Nana | 25 | 1 | — | Piso |
| `casa_sala_038` | Sofá de caixotes com colchonete de feltro | Comum | Loja: Marcenaria do Mestre Omóforo | 20 | 1 | — | Piso |
| `casa_sala_039` | Tamborete de amoreira | Incomum | Loja: Marcenaria do Mestre Omóforo | 80 | 4 | — | Piso |
| `casa_sala_040` | Mesa de azulejo amarelo | Comum | Loja: Mercearia do Zaku | 14 | 1 | — | Piso |
| `casa_sala_041` | Lareira de pedra seca da serra | Épico | Crafting: Bancada | — | 8 | Picos de Litema | Piso |
| `casa_sala_042` | Banquinho torto feito à mão | Comum | Conquista: Feito à Mão | — | 1 | — | Piso |
| `casa_sala_043` | Bota calejada de bronze | Incomum | Conquista: Calejado | — | 4 | — | Superfície |
| `casa_sala_044` | Banco de tronco de choupo do Joaquim | Raro | Loja: Marcenaria do Mestre Omóforo | 150 | 5 | Raízes do Joaquim | Piso |
| `casa_sala_045` | Muralha de pedra seca em miniatura | Incomum | Loja: Olaria da Náfsha | 60 | 4 | Raízes do Joaquim | Superfície |

<a id="quarto"></a>
## Quarto (35)

| ID | Nome | Raridade | Origem | Preço (ouro) | Conforto | Conjunto | Encaixe |
|---|---|---|---|---|---|---|---|
| `casa_quarto_001` | Cama de casal de nogueira entalhada | Épico | Loja: Marcenaria do Mestre Omóforo | 530 | 8 | — | Piso |
| `casa_quarto_002` | Cama de solteiro de cabeceira torneada | Incomum | Loja: Marcenaria do Mestre Omóforo | 65 | 3 | — | Piso |
| `casa_quarto_003` | Beliche dos primos | Raro | Conquista: Prima de Todos | — | 6 | Iwan da Vó Edna | Piso |
| `casa_quarto_004` | Berço de embalo antigo | Raro | Loja: Caravana do Ammo | 175 | 6 | Iwan da Vó Edna | Piso |
| `casa_quarto_005` | Colchão de feltro de lã | Comum | Loja: Bazar da Nana | 28 | 2 | — | Piso |
| `casa_quarto_006` | Colcha de retalhos da Vó Edna | Raro | Missão da Casa: Parede de Barro | — | 6 | Iwan da Vó Edna | Superfície |
| `casa_quarto_007` | Colcha de suzani | Incomum | Loja: Costureira Mestra Narisah | 50 | 4 | — | Superfície |
| `casa_quarto_008` | Travesseiro de pétalas de rosa | Comum | Loja: Bazar da Nana | 12 | 1 | — | Superfície |
| `casa_quarto_009` | Criado-mudo com gavetinha | Comum | Loja: Marcenaria do Mestre Omóforo | 17 | 1 | — | Piso |
| `casa_quarto_010` | Penteadeira com espelho oval | Raro | Loja: Marcenaria do Mestre Omóforo | 135 | 6 | — | Piso |
| `casa_quarto_011` | Cômoda de quatro gavetas | Incomum | Loja: Marcenaria do Mestre Omóforo | 45 | 3 | — | Piso |
| `casa_quarto_012` | Mosquiteiro de seda fina | Comum | Loja: Bazar de Linhas da Mariam | 23 | 2 | — | Teto |
| `casa_quarto_013` | Lençol bordado com romãs | Incomum | Loja: Costureira Mestra Narisah | 50 | 4 | — | Superfície |
| `casa_quarto_014` | Cabide de pé de vime | Comum | Loja: Bazar da Nana | 30 | 1 | — | Piso |
| `casa_quarto_015` | Baú de enxoval da bisavó | Raro | Loja: Marcenaria do Mestre Omóforo | 235 | 5 | Iwan da Vó Edna | Piso |
| `casa_quarto_016` | Cama de vento dobrável | Comum | Loja: Armazém Geral | 15 | 2 | — | Piso |
| `casa_quarto_017` | Rede de dormir de punho bordado | Incomum | Loja: Costureira Mestra Narisah | 65 | 3 | — | Parede |
| `casa_quarto_018` | Escrivaninha de estudante | Incomum | Loja: Marcenaria do Mestre Omóforo | 85 | 3 | Oficina do Ícaro | Piso |
| `casa_quarto_019` | Cadeira giratória de engrenagens | Incomum | Loja: Caravana do Ammo | 60 | 4 | Oficina do Ícaro | Piso |
| `casa_quarto_020` | Mural de cortiça com desenhos | Comum | Loja: Bazar de Linhas da Mariam | 14 | 1 | — | Parede |
| `casa_quarto_021` | Cama de dossel de ikat | Épico | Loja: Costureira Mestra Narisah | 560 | 8 | Bazar da Nana | Piso |
| `casa_quarto_022` | Catre de couro do caravaneiro | Raro | Loja: Caravana do Ammo | 200 | 5 | Caravana do Vô Jorge | Piso |
| `casa_quarto_023` | Esteira de junco do tugai | Comum | Loja: Bazar da Nana | 20 | 2 | Tugai do Zarafshan | Piso |
| `casa_quarto_024` | Caixinha de música de bailarina | Raro | Loja: Caravana do Ammo | 220 | 5 | — | Superfície |
| `casa_quarto_025` | Cabeceira entalhada de saxaul | Raro | Loja: Marcenaria do Mestre Omóforo | 170 | 5 | Areias Flamejantes | Piso |
| `casa_quarto_026` | Lençol de estrelinhas que brilham no escuro | Incomum | Loja: Bazar de Linhas da Mariam | 80 | 4 | Noite Rubra da Lavine | Superfície |
| `casa_quarto_027` | Cama da Noite Rubra | Épico | Missão pessoal da Lavine (`quest_lavine_redtide`) | — | 8 | Noite Rubra da Lavine | Piso |
| `casa_quarto_028` | Cama-forte do Joaquim com escudo na cabeceira | Épico | Missão pessoal do Joaquim (`quest_joaquim_echoes_of_home`) | — | 9 | Raízes do Joaquim | Piso |
| `casa_quarto_029` | Cama de dossel de linho branco da Maria | Épico | Missão pessoal da Maria (`quest_maria_fading_light`) | — | 9 | Luz da Maria | Piso |
| `casa_quarto_030` | Cama-oficina do Ícaro com gavetas de parafuso | Épico | Missão pessoal do Ícaro (`quest_icaro_ancient_gears`) | — | 9 | Oficina do Ícaro | Piso |
| `casa_quarto_031` | Cama de toras da Maithe com aljava na cabeceira | Épico | Missão pessoal da Maithe (`quest_maithe_windtrail`) | — | 9 | Ventania da Maithe | Piso |
| `casa_quarto_032` | Cobertor listrado da serra | Incomum | Drop: Ushabti de Pedra | — | 3 | Picos de Litema | Superfície |
| `casa_quarto_033` | Gaveteiro de meias perdidas | Comum | Loja: Marcenaria do Mestre Omóforo | 29 | 2 | — | Piso |
| `casa_quarto_034` | Cabide de gorros de feltro | Comum | Loja: Marcenaria do Mestre Omóforo | 12 | 1 | Caravana do Vô Jorge | Parede |
| `casa_quarto_035` | Espelho de corpo inteiro com moldura de junco | Incomum | Loja: Marcenaria do Mestre Omóforo | 55 | 3 | Tugai do Zarafshan | Piso |

<a id="cozinha"></a>
## Cozinha (44)

| ID | Nome | Raridade | Origem | Preço (ouro) | Conforto | Conjunto | Encaixe |
|---|---|---|---|---|---|---|---|
| `casa_cozinha_001` | Tandir de barro da Vó | Raro | Missão da Casa: Fogo na Cozinha | — | 5 | Cozinha da Vó | Piso |
| `casa_cozinha_002` | Filtro de barro da Vó Edna | Raro | Missão da Casa: Fogo na Cozinha | — | 6 | Cozinha da Vó | Superfície |
| `casa_cozinha_003` | Pote de torrão dourado | Comum | Loja: Mercearia do Zaku | 20 | 1 | Cozinha da Vó | Superfície |
| `casa_cozinha_004` | Talha d'água de barro | Comum | Loja: Olaria da Náfsha | 18 | 1 | — | Piso |
| `casa_cozinha_005` | Jarra de barro com caneca | Comum | Loja: Olaria da Náfsha | 17 | 2 | — | Superfície |
| `casa_cozinha_006` | Panela de barro preta | Comum | Loja: Olaria da Náfsha | 22 | 1 | Cozinha da Vó | Superfície |
| `casa_cozinha_007` | Pilão de madeira | Comum | Loja: Marcenaria do Mestre Omóforo | 17 | 1 | Moinho do Pomar | Piso |
| `casa_cozinha_008` | Gamela de madeira | Comum | Loja: Marcenaria do Mestre Omóforo | 19 | 1 | Moinho do Pomar | Superfície |
| `casa_cozinha_009` | Colher de pau gigante | Comum | Loja: Bazar da Nana | 21 | 2 | — | Parede |
| `casa_cozinha_010` | Vaporeira de cobre para bolinhos | Comum | Loja: Mercearia do Zaku | 20 | 1 | Cozinha da Vó | Superfície |
| `casa_cozinha_011` | Coador de chá com suporte | Comum | Loja: Mercearia do Zaku | 24 | 1 | Cozinha da Vó | Superfície |
| `casa_cozinha_012` | Bule de cerâmica turquesa | Incomum | Loja: Mercearia do Zaku | 70 | 4 | Cozinha da Vó | Superfície |
| `casa_cozinha_013` | Caneca de cobre batido | Comum | Loja: Mercearia do Zaku | 24 | 1 | — | Superfície |
| `casa_cozinha_014` | Lata de biscoitos de gergelim | Comum | Loja: Mercearia do Zaku | 13 | 2 | — | Superfície |
| `casa_cozinha_015` | Réstia de alho | Comum | Loja: Bazar da Nana | 24 | 2 | Bazar da Nana | Parede |
| `casa_cozinha_016` | Réstia de cebola roxa | Comum | Loja: Bazar da Nana | 8 | 2 | Bazar da Nana | Parede |
| `casa_cozinha_017` | Cacho de uvas secando | Comum | Loja: Bazar da Nana | 22 | 2 | Bazar da Nana | Teto |
| `casa_cozinha_018` | Prateleira de temperos | Incomum | Loja: Marcenaria do Mestre Omóforo | 75 | 3 | — | Parede |
| `casa_cozinha_019` | Mesa de cozinha com banco | Comum | Loja: Marcenaria do Mestre Omóforo | 28 | 2 | — | Piso |
| `casa_cozinha_020` | Geladeira de jarras de barro | Raro | Loja: Caravana do Ammo | 225 | 6 | — | Piso |
| `casa_cozinha_021` | Paneleiro de ferro | Incomum | Loja: Forja do Mestre Mihr | 85 | 3 | — | Parede |
| `casa_cozinha_022` | Jogo de panelas de cobre batido | Incomum | Loja: Forja do Mestre Mihr | 45 | 3 | Cozinha da Vó | Superfície |
| `casa_cozinha_023` | Caldeirão de cobre para doce | Raro | Loja: Forja do Mestre Mihr | 105 | 6 | Moinho do Pomar | Piso |
| `casa_cozinha_024` | Fruteira de cerâmica turquesa | Comum | Loja: Olaria da Náfsha | 13 | 1 | — | Superfície |
| `casa_cozinha_025` | Peneira de junco | Comum | Loja: Bazar da Nana | 22 | 2 | Moinho do Pomar | Parede |
| `casa_cozinha_026` | Pano de prato dos dias da semana | Comum | Loja: Bazar de Linhas da Mariam | 20 | 2 | — | Parede |
| `casa_cozinha_027` | Cesto de ovos do pomar | Comum | Loja: Bazar da Nana | 15 | 2 | — | Superfície |
| `casa_cozinha_028` | Jarra de suco de romã | Comum | Loja: Mercearia do Zaku | 24 | 1 | — | Superfície |
| `casa_cozinha_029` | Pote de halva | Comum | Loja: Mercearia do Zaku | 12 | 1 | — | Superfície |
| `casa_cozinha_030` | Vidros de conserva de pimenta | Incomum | Loja: Bazar da Nana | 85 | 3 | — | Superfície |
| `casa_cozinha_031` | Tábua de queijo de ovelha | Incomum | Loja: Mercearia do Zaku | 35 | 3 | Cozinha da Vó | Superfície |
| `casa_cozinha_032` | Ralador de cenoura do plov | Comum | Loja: Forja do Mestre Mihr | 24 | 2 | — | Superfície |
| `casa_cozinha_033` | Moedor de especiarias de manivela | Incomum | Loja: Forja do Mestre Mihr | 85 | 3 | — | Superfície |
| `casa_cozinha_034` | Torrador de gergelim de lata | Incomum | Loja: Forja do Mestre Mihr | 60 | 4 | Moinho do Pomar | Piso |
| `casa_cozinha_035` | Balança de pratos do bazar | Raro | Loja: Caravana do Ammo | 140 | 5 | Bazar da Nana | Superfície |
| `casa_cozinha_036` | Cristaleira de copinhos de chá | Incomum | Loja: Marcenaria do Mestre Omóforo | 85 | 3 | — | Piso |
| `casa_cozinha_037` | Toalha de mesa de ikat | Comum | Loja: Costureira Mestra Narisah | 19 | 2 | — | Superfície |
| `casa_cozinha_038` | Fogareiro de duas bocas | Comum | Loja: Armazém Geral | 22 | 1 | — | Piso |
| `casa_cozinha_039` | Mó de pedra rachada do moinho | Épico | Segredo do mapa (moinho do pomar do Vô Jorge) | — | 9 | Moinho do Pomar | Piso |
| `casa_cozinha_040` | Forno de torrar grãos | Épico | Crafting: Forja | — | 8 | Moinho do Pomar | Piso |
| `casa_cozinha_041` | Bolo de damasco na forma | Comum | Loja: Mercearia do Zaku | 15 | 1 | Iwan da Vó Edna | Superfície |
| `casa_cozinha_042` | Abano de palha do tandir | Comum | Loja: Bazar da Nana | 26 | 2 | Cozinha da Vó | Parede |
| `casa_cozinha_043` | Marmita de cobre do caravaneiro | Incomum | Loja: Caravana do Ammo | 55 | 3 | Caravana do Vô Jorge | Superfície |
| `casa_cozinha_044` | Cesta da Mão Amiga | Comum | Conquista: Mão Amiga | — | 2 | — | Superfície |

<a id="banheiro"></a>
## Banheiro (21)

| ID | Nome | Raridade | Origem | Preço (ouro) | Conforto | Conjunto | Encaixe |
|---|---|---|---|---|---|---|---|
| `casa_banheiro_001` | Bacia de cobre com jarro | Comum | Loja: Mercearia do Zaku | 22 | 2 | — | Superfície |
| `casa_banheiro_002` | Tina de banho de madeira | Incomum | Loja: Marcenaria do Mestre Omóforo | 40 | 4 | — | Piso |
| `casa_banheiro_003` | Banheira de pés de leão | Épico | Loja: Caravana do Ammo | 455 | 9 | — | Piso |
| `casa_banheiro_004` | Espelho de moldura de conchinhas | Incomum | Loja: Bazar da Nana | 75 | 3 | — | Parede |
| `casa_banheiro_005` | Toalheiro de junco | Comum | Loja: Marcenaria do Mestre Omóforo | 11 | 2 | — | Parede |
| `casa_banheiro_006` | Sabonete de erva-doce artesanal | Comum | Loja: Bazar da Nana | 8 | 1 | — | Superfície |
| `casa_banheiro_007` | Sabonetes de hortelã da Mariam | Incomum | Loja: Bazar de Linhas da Mariam | 55 | 3 | Luz da Maria | Superfície |
| `casa_banheiro_008` | Chuveiro de lata furada | Comum | Loja: Forja do Mestre Mihr | 19 | 2 | — | Parede |
| `casa_banheiro_009` | Caixa d'água de cobre | Comum | Loja: Forja do Mestre Mihr | 8 | 1 | — | Teto |
| `casa_banheiro_010` | Pia de cerâmica turquesa com pedestal | Incomum | Loja: Olaria da Náfsha | 45 | 3 | — | Piso |
| `casa_banheiro_011` | Tapete de banheiro de feltro | Comum | Loja: Bazar de Linhas da Mariam | 20 | 1 | — | Piso |
| `casa_banheiro_012` | Cesto de roupa suja de palha | Comum | Loja: Bazar da Nana | 14 | 1 | — | Piso |
| `casa_banheiro_013` | Penico esmaltado com florzinha | Comum | Loja: Mercearia do Zaku | 30 | 1 | — | Piso |
| `casa_banheiro_014` | Kit de barbear do Vô Jorge | Raro | Segredo do mapa (moinho do pomar do Vô Jorge) | — | 5 | Caravana do Vô Jorge | Superfície |
| `casa_banheiro_015` | Armarinho de remédios da Mariam | Comum | Loja: Armazém Geral | 30 | 2 | — | Parede |
| `casa_banheiro_016` | Patinho de borracha amarelo | Comum | Loja: Bazar de Linhas da Mariam | 22 | 2 | — | Superfície |
| `casa_banheiro_017` | Cortina de box de ikat | Comum | Loja: Costureira Mestra Narisah | 25 | 1 | Bazar da Nana | Parede |
| `casa_banheiro_018` | Biombo de junco trançado | Incomum | Loja: Bazar da Nana | 65 | 3 | — | Piso |
| `casa_banheiro_019` | Banquinho de banho de madeira | Comum | Loja: Marcenaria do Mestre Omóforo | 29 | 2 | — | Piso |
| `casa_banheiro_020` | Chuveiro solar de serpentina | Raro | Crafting: Bancada | — | 6 | Oficina do Ícaro | Parede |
| `casa_banheiro_021` | Banheira de tufo rosa de fonte termal | Épico | Drop: Tages, o Habitante Ancestral (chefe) | — | 8 | Tufo Rosa das Lamparinas | Piso |

<a id="quintal"></a>
## Quintal e horta (39)

| ID | Nome | Raridade | Origem | Preço (ouro) | Conforto | Conjunto | Encaixe |
|---|---|---|---|---|---|---|---|
| `casa_quintal_001` | Poço de pedra com sarilho | Raro | Missão da Casa: Água Fresca | — | 6 | Iwan da Vó Edna | Quintal |
| `casa_quintal_002` | Horta de canteiro elevado | Comum | Loja: Viveiro da Nana | 15 | 2 | Horta dos Melões | Quintal |
| `casa_quintal_003` | Canteiro de melões | Comum | Loja: Viveiro da Nana | 24 | 1 | Horta dos Melões | Quintal |
| `casa_quintal_004` | Canteiro de coentro e endro | Comum | Loja: Viveiro da Nana | 26 | 1 | Horta dos Melões | Quintal |
| `casa_quintal_005` | Pé de damasco | Incomum | Loja: Viveiro da Nana | 80 | 4 | — | Quintal |
| `casa_quintal_006` | Pé de romã | Incomum | Loja: Viveiro da Nana | 75 | 3 | — | Quintal |
| `casa_quintal_007` | Pé de tamargueira rosa | Raro | Loja: Viveiro da Nana | 140 | 6 | Areias Flamejantes | Quintal |
| `casa_quintal_008` | Pé de amora | Raro | Loja: Viveiro da Nana | 200 | 6 | — | Quintal |
| `casa_quintal_009` | Pé de pêssego | Incomum | Loja: Viveiro da Nana | 70 | 4 | — | Quintal |
| `casa_quintal_010` | Figueira de quintal | Comum | Loja: Viveiro da Nana | 26 | 1 | — | Quintal |
| `casa_quintal_011` | Pé de espinheiro-marítimo | Comum | Loja: Viveiro da Nana | 12 | 1 | Horta dos Melões | Quintal |
| `casa_quintal_012` | Espantalho de chapéu de palha | Incomum | Loja: Costureira Mestra Narisah | 70 | 4 | Horta dos Melões | Quintal |
| `casa_quintal_013` | Forno tandir de quintal | Incomum | Loja: Olaria da Náfsha | 60 | 4 | Cozinha da Vó | Quintal |
| `casa_quintal_014` | Varal de arame com prendedores de madeira | Comum | Loja: Armazém Geral | 17 | 2 | — | Quintal |
| `casa_quintal_015` | Tanque de lavar de pedra | Comum | Loja: Armazém Geral | 27 | 2 | — | Quintal |
| `casa_quintal_016` | Cerca de varas de choupo | Comum | Loja: Marcenaria do Mestre Omóforo | 10 | 1 | — | Quintal |
| `casa_quintal_017` | Portão de madeira do pomar | Incomum | Loja: Marcenaria do Mestre Omóforo | 60 | 4 | Iwan da Vó Edna | Quintal |
| `casa_quintal_018` | Cata-vento de chapa | Raro | Loja: Forja do Mestre Mihr | 120 | 6 | Oficina do Ícaro | Quintal |
| `casa_quintal_019` | Cisterna de cúpula | Raro | Crafting: Bancada | — | 5 | — | Quintal |
| `casa_quintal_020` | Bica d'água do aryk | Comum | Loja: Marcenaria do Mestre Omóforo | 14 | 2 | Tugai do Zarafshan | Quintal |
| `casa_quintal_021` | Banco de pedra sob a amoreira | Incomum | Loja: Olaria da Náfsha | 35 | 3 | — | Quintal |
| `casa_quintal_022` | Balanço de pneu | Comum | Loja: Armazém Geral | 24 | 1 | — | Quintal |
| `casa_quintal_023` | Fogueira de quintal com toras | Comum | Crafting: Bancada | — | 2 | — | Quintal |
| `casa_quintal_024` | Mesa de piquenique de tora | Comum | Loja: Marcenaria do Mestre Omóforo | 22 | 2 | Tugai do Zarafshan | Quintal |
| `casa_quintal_025` | Caixa de abelhas do pomar | Raro | Loja: Viveiro da Nana | 120 | 5 | Horta dos Melões | Quintal |
| `casa_quintal_026` | Composteira de madeira | Comum | Loja: Viveiro da Nana | 26 | 2 | Horta dos Melões | Quintal |
| `casa_quintal_027` | Regador de lata | Comum | Loja: Forja do Mestre Mihr | 18 | 1 | Horta dos Melões | Quintal |
| `casa_quintal_028` | Carrinho de mão de madeira | Comum | Loja: Forja do Mestre Mihr | 18 | 1 | — | Quintal |
| `casa_quintal_029` | Pergolado com videira | Raro | Loja: Viveiro da Nana | 140 | 5 | — | Quintal |
| `casa_quintal_030` | Chafariz de azulejo turquesa | Épico | Loja: Caravana do Ammo | 335 | 9 | — | Quintal |
| `casa_quintal_031` | Rede entre dois choupos | Raro | Loja: Costureira Mestra Narisah | 180 | 6 | — | Quintal |
| `casa_quintal_032` | Tamareira anã | Incomum | Loja: Viveiro da Nana | 50 | 4 | — | Quintal |
| `casa_quintal_033` | Pedra de amolar de manivela | Incomum | Loja: Forja do Mestre Mihr | 80 | 4 | — | Quintal |
| `casa_quintal_034` | Laguinho de peixes com lótus | Épico | Loja: Viveiro da Nana | 635 | 8 | — | Quintal |
| `casa_quintal_035` | Caminho de pedrinhas | Comum | Loja: Olaria da Náfsha | 11 | 1 | — | Quintal |
| `casa_quintal_036` | Caramanchão de roseira | Raro | Loja: Viveiro da Nana | 150 | 6 | — | Quintal |
| `casa_quintal_037` | Boneco de treino de palha | Comum | Loja: Armazém Geral | 27 | 1 | — | Quintal |
| `casa_quintal_038` | Árvore de raízes expostas para escalar | Épico | Loja: Viveiro da Nana | 320 | 8 | Raízes do Joaquim | Quintal |
| `casa_quintal_039` | Alvo de palha para treino de arco | Incomum | Loja: Armazém Geral | 90 | 4 | Ventania da Maithe | Quintal |

<a id="curral"></a>
## Curral e animais (27)

| ID | Nome | Raridade | Origem | Preço (ouro) | Conforto | Conjunto | Encaixe |
|---|---|---|---|---|---|---|---|
| `casa_curral_001` | Casinha do Ari | Incomum | Missão da Casa: Um Cantinho Seu | — | 4 | Cantinho do Ari | Quintal |
| `casa_curral_002` | Caminha de pano do Ari | Comum | Loja: Bazar de Linhas da Mariam | 20 | 2 | Cantinho do Ari | Piso |
| `casa_curral_003` | Tigela de cobre do Ari | Comum | Loja: Mercearia do Zaku | 20 | 2 | Cantinho do Ari | Piso |
| `casa_curral_004` | Bolinha de feltro babada do Ari | Comum | Loja: Bazar de Linhas da Mariam | 16 | 2 | Cantinho do Ari | Piso |
| `casa_curral_005` | Osso de brinquedo de corda | Comum | Loja: Bazar da Nana | 17 | 1 | Cantinho do Ari | Piso |
| `casa_curral_006` | Placa "Cuidado: cão carinhoso" | Comum | Loja: Bazar de Linhas da Mariam | 17 | 1 | Cantinho do Ari | Quintal |
| `casa_curral_007` | Galinheiro de varas | Comum | Loja: Marcenaria do Mestre Omóforo | 15 | 1 | — | Quintal |
| `casa_curral_008` | Ninho de palha com ovos | Comum | Loja: Bazar da Nana | 30 | 1 | — | Quintal |
| `casa_curral_009` | Chiqueirinho da porca Mimosa | Incomum | Loja: Bazar da Nana | 75 | 3 | — | Quintal |
| `casa_curral_010` | Curral de choupo | Incomum | Loja: Marcenaria do Mestre Omóforo | 65 | 3 | Caravana do Vô Jorge | Quintal |
| `casa_curral_011` | Cocho de madeira escavada | Comum | Loja: Marcenaria do Mestre Omóforo | 17 | 2 | — | Quintal |
| `casa_curral_012` | Bebedouro de pedra | Comum | Loja: Olaria da Náfsha | 28 | 1 | — | Quintal |
| `casa_curral_013` | Sela de couro trabalhada no cavalete | Raro | Loja: Caravana do Ammo | 160 | 6 | Caravana do Vô Jorge | Quintal |
| `casa_curral_014` | Sela de camelo decorativa | Incomum | Loja: Caravana do Ammo | 90 | 4 | Caravana do Vô Jorge | Quintal |
| `casa_curral_015` | Barquinha de luz enfeitada | Épico | Loja: Caravana do Ammo | 395 | 8 | Caravana do Vô Jorge | Quintal |
| `casa_curral_016` | Galinha-d'angola ciscando | Incomum | Loja: Bazar da Nana | 85 | 3 | — | Quintal |
| `casa_curral_017` | Pato marreco do tanque | Comum | Loja: Bazar da Nana | 9 | 1 | — | Quintal |
| `casa_curral_018` | Bode Ioiô de estimação | Raro | Loja: Caravana do Ammo | 165 | 6 | — | Quintal |
| `casa_curral_019` | Asno Sereno no cercado | Raro | Loja: Caravana do Ammo | 115 | 5 | — | Quintal |
| `casa_curral_020` | Papagaio no poleiro que fala "Vó Edna" | Raro | Loja: Caravana do Ammo | 195 | 5 | — | Piso |
| `casa_curral_021` | Gaiola aberta de passarinho liberto | Comum | Loja: Bazar da Nana | 14 | 1 | — | Parede |
| `casa_curral_022` | Comedouro de pintassilgo | Comum | Loja: Viveiro da Nana | 19 | 2 | Horta dos Melões | Quintal |
| `casa_curral_023` | Ninho de cegonha no poste | Incomum | Loja: Viveiro da Nana | 85 | 3 | Tugai do Zarafshan | Quintal |
| `casa_curral_024` | Sino de camelo no prego | Comum | Loja: Forja do Mestre Mihr | 15 | 2 | Caravana do Vô Jorge | Parede |
| `casa_curral_025` | Baia da camela Teimosa | Raro | Segredo do mapa (moinho do pomar do Vô Jorge) | — | 6 | Caravana do Vô Jorge | Quintal |
| `casa_curral_026` | Manjedoura de feno | Comum | Loja: Bazar da Nana | 9 | 2 | — | Quintal |
| `casa_curral_027` | Ferradura da sorte no portão | Incomum | Loja: Forja do Mestre Mihr | 60 | 4 | Caravana do Vô Jorge | Quintal |

<a id="luz"></a>
## Iluminação (30)

| ID | Nome | Raridade | Origem | Preço (ouro) | Conforto | Conjunto | Encaixe |
|---|---|---|---|---|---|---|---|
| `casa_luz_001` | Lampião de óleo de gergelim | Comum | Loja: Lamparinas da Parthenos | 13 | 1 | — | Superfície |
| `casa_luz_002` | Lamparina de lata | Comum | Loja: Lamparinas da Parthenos | 9 | 1 | — | Superfície |
| `casa_luz_003` | Candeeiro de azeite de barro | Comum | Loja: Olaria da Náfsha | 13 | 1 | — | Superfície |
| `casa_luz_004` | Lustre de roda de carroça | Raro | Loja: Forja do Mestre Mihr | 185 | 6 | Caravana do Vô Jorge | Teto |
| `casa_luz_005` | Luminária de cabaça recortada | Incomum | Loja: Bazar da Nana | 35 | 3 | Areias Flamejantes | Teto |
| `casa_luz_006` | Abajur de cúpula de seda | Comum | Loja: Marcenaria do Mestre Omóforo | 18 | 2 | — | Superfície |
| `casa_luz_007` | Varal de lanterninhas de festa | Incomum | Loja: Lamparinas da Parthenos | 50 | 4 | — | Teto |
| `casa_luz_008` | Vela de cera de abelha | Comum | Loja: Viveiro da Nana | 23 | 2 | — | Superfície |
| `casa_luz_009` | Castiçal de três velas de latão | Incomum | Loja: Forja do Mestre Mihr | 55 | 3 | — | Superfície |
| `casa_luz_010` | Lanterna de vagalume | Raro | Drop: Aranha-Tecelã Apagada | — | 5 | Tugai do Zarafshan | Superfície |
| `casa_luz_011` | Luminária de cristal das grutas | Raro | Drop: Enxame de Morcegos do Limiar | — | 5 | Tufo Rosa das Lamparinas | Superfície |
| `casa_luz_012` | Lampião de caravançará | Incomum | Loja: Caravana do Ammo | 90 | 4 | — | Superfície |
| `casa_luz_013` | Lamparina da Parthenos | Raro | Missão: A Última Lamparina | — | 6 | Tufo Rosa das Lamparinas | Superfície |
| `casa_luz_014` | Lanterna de papel de seda | Comum | Loja: Lamparinas da Parthenos | 11 | 2 | — | Teto |
| `casa_luz_015` | Lustre de garrafas coloridas | Incomum | Loja: Lamparinas da Parthenos | 60 | 4 | — | Teto |
| `casa_luz_016` | Luminária de engrenagens | Raro | Crafting: Bancada | — | 6 | Oficina do Ícaro | Superfície |
| `casa_luz_017` | Poste de luz de quintal | Comum | Loja: Lamparinas da Parthenos | 21 | 2 | — | Quintal |
| `casa_luz_018` | Tocha de parede de ferro | Comum | Loja: Forja do Mestre Mihr | 23 | 2 | Picos de Litema | Parede |
| `casa_luz_019` | Arandela de vidro âmbar | Incomum | Loja: Lamparinas da Parthenos | 85 | 3 | — | Parede |
| `casa_luz_020` | Luz noturna de lua minguante | Incomum | Loja: Lamparinas da Parthenos | 80 | 4 | Noite Rubra da Lavine | Parede |
| `casa_luz_021` | Lamparina de azeite do nicho | Comum | Loja: Lamparinas da Parthenos | 8 | 1 | Luz da Maria | Superfície |
| `casa_luz_022` | Lustre de tufo rosa e cristal | Épico | Loja: Lamparinas da Parthenos | 315 | 9 | Tufo Rosa das Lamparinas | Teto |
| `casa_luz_023` | Faísca presa engarrafada | Épico | Drop: Tages, o Habitante Ancestral (chefe) | — | 9 | Noite Rubra da Lavine | Superfície |
| `casa_luz_024` | Lâmpada de pedra-sol | Épico | Conquista: A Luz Volta para Casa | — | 9 | Cristal do Sol | Superfície |
| `casa_luz_025` | Lanterna do Grande Ushabti | Épico | Drop: O Grande Ushabti (chefe) | — | 8 | Areias Flamejantes | Superfície |
| `casa_luz_026` | Lampião de mina do Mestre Mihr | Raro | Missão: A Passagem da Mina | — | 5 | Picos de Litema | Superfície |
| `casa_luz_027` | Holofote de dínamo a manivela | Raro | Crafting: Bancada | — | 5 | Oficina do Ícaro | Piso |
| `casa_luz_028` | Vaso de luz de cogumelo | Incomum | Drop: Enxame de Morcegos do Limiar | — | 4 | Tufo Rosa das Lamparinas | Superfície |
| `casa_luz_029` | Candeeiro de vidro verde de botica | Incomum | Loja: Bazar de Linhas da Mariam | 45 | 3 | — | Superfície |
| `casa_luz_030` | Lanterninha giratória de estrelas | Raro | Loja: Lamparinas da Parthenos | 240 | 5 | Noite Rubra da Lavine | Superfície |

<a id="parede"></a>
## Paredes e quadros (38)

| ID | Nome | Raridade | Origem | Preço (ouro) | Conforto | Conjunto | Encaixe |
|---|---|---|---|---|---|---|---|
| `casa_parede_001` | Relógio de Zurvan de parede | Raro | Loja: Caravana do Ammo | 245 | 6 | — | Parede |
| `casa_parede_002` | Iluminura do pomar em flor | Incomum | Loja: Bazar de Linhas da Mariam | 75 | 3 | Iluminuras e Papel Recortado | Parede |
| `casa_parede_003` | Iluminura da ave Huma | Incomum | Loja: Bazar de Linhas da Mariam | 60 | 4 | Iluminuras e Papel Recortado | Parede |
| `casa_parede_004` | Iluminura de Kök-Böri liberto | Raro | Drop: Kök-Böri, o Lobo Celeste (chefe) | — | 6 | Iluminuras e Papel Recortado | Parede |
| `casa_parede_005` | Folhas de hinos iluminadas no barbante | Comum | Loja: Bazar de Linhas da Mariam | 15 | 1 | Iluminuras e Papel Recortado | Parede |
| `casa_parede_006` | Calendário de parede da mercearia | Comum | Loja: Mercearia do Zaku | 26 | 2 | — | Parede |
| `casa_parede_007` | Espelho de moldura de palha de tamareira | Comum | Loja: Bazar da Nana | 21 | 1 | Areias Flamejantes | Parede |
| `casa_parede_008` | Bordado emoldurado "Lar doce lar" | Comum | Loja: Bazar de Linhas da Mariam | 16 | 1 | Iwan da Vó Edna | Parede |
| `casa_parede_009` | Prato de parede pintado à mão | Comum | Loja: Olaria da Náfsha | 15 | 1 | — | Parede |
| `casa_parede_010` | Proa entalhada da Barca do Sol | Raro | Loja: Caravana do Ammo | 120 | 5 | — | Parede |
| `casa_parede_011` | Mapa da ilha desenhado pela Maithe | Raro | Conquista: Desbravador | — | 5 | Ventania da Maithe | Parede |
| `casa_parede_012` | Quadro de engrenagens de latão | Incomum | Crafting: Bancada | — | 4 | Oficina do Ícaro | Parede |
| `casa_parede_013` | Planta-baixa de invenção maluca | Incomum | Loja: Caravana do Ammo | 40 | 4 | Oficina do Ícaro | Parede |
| `casa_parede_014` | Prateleira de pratos de cobre | Comum | Loja: Mercearia do Zaku | 17 | 1 | Cozinha da Vó | Parede |
| `casa_parede_015` | Chapéu de palha no prego | Comum | Loja: Costureira Mestra Narisah | 10 | 2 | — | Parede |
| `casa_parede_016` | Desenhos dos primos em giz de cera | Comum | Diária: Noite de Fogueira | — | 2 | Iwan da Vó Edna | Parede |
| `casa_parede_017` | Desenho da Lavine com cinco bonecos de palito | Raro | Diária: A Lavine e os Morcegos | — | 6 | Noite Rubra da Lavine | Parede |
| `casa_parede_018` | Tapeçaria de estrelas da Noite Rubra | Épico | Missão pessoal da Lavine (`quest_lavine_redtide`) | — | 9 | Noite Rubra da Lavine | Parede |
| `casa_parede_019` | Arco e aljava de parede da Maithe | Raro | Missão pessoal da Maithe (`quest_maithe_windtrail`) | — | 6 | Ventania da Maithe | Parede |
| `casa_parede_020` | Escudo de madeira decorativo do Joaquim | Raro | Missão pessoal do Joaquim (`quest_joaquim_echoes_of_home`) | — | 6 | Raízes do Joaquim | Parede |
| `casa_parede_021` | Vitral de luz dourada da Maria | Épico | Missão pessoal da Maria (`quest_maria_fading_light`) | — | 9 | Luz da Maria | Parede |
| `casa_parede_022` | Quadro de mosaico de azulejo | Incomum | Loja: Olaria da Náfsha | 85 | 3 | — | Parede |
| `casa_parede_023` | Azulejo turquesa pintado à mão | Comum | Loja: Olaria da Náfsha | 28 | 2 | — | Parede |
| `casa_parede_024` | Painel de fitas nas cinco cores | Comum | Loja: Bazar de Linhas da Mariam | 24 | 1 | — | Parede |
| `casa_parede_025` | Roseta de palha trançada | Incomum | Loja: Bazar da Nana | 75 | 3 | — | Parede |
| `casa_parede_026` | Quadro de borboletas de papel | Comum | Loja: Bazar de Linhas da Mariam | 9 | 1 | Tugai do Zarafshan | Parede |
| `casa_parede_027` | Pôster do Cristal do Sol | Comum | Loja: Armazém Geral | 22 | 2 | Cristal do Sol | Parede |
| `casa_parede_028` | Flâmula dos Remadores do Sol | Comum | Loja: Armazém Geral | 22 | 1 | — | Parede |
| `casa_parede_029` | Placa entalhada com o nome do dono | Comum | Loja: Marcenaria do Mestre Omóforo | 22 | 1 | Iwan da Vó Edna | Parede |
| `casa_parede_030` | Estante de parede de caixotes | Comum | Loja: Marcenaria do Mestre Omóforo | 27 | 2 | — | Parede |
| `casa_parede_031` | Relógio de parede do caravançará | Incomum | Loja: Mercearia do Zaku | 85 | 3 | — | Parede |
| `casa_parede_032` | Móbile de penas de faisão | Comum | Loja: Bazar da Nana | 11 | 2 | — | Parede |
| `casa_parede_033` | Quadro do faisão do tugai | Incomum | Loja: Bazar de Linhas da Mariam | 90 | 4 | — | Parede |
| `casa_parede_034` | Lousa de giz com a receita do dia | Comum | Loja: Mercearia do Zaku | 22 | 2 | Cozinha da Vó | Parede |
| `casa_parede_035` | Ponto-cruz do Ari | Incomum | Loja: Bazar de Linhas da Mariam | 65 | 3 | Cantinho do Ari | Parede |
| `casa_parede_036` | Placa "Primeira faísca" | Comum | Conquista: Primeira Faísca | — | 2 | — | Parede |
| `casa_parede_037` | Medalha de lata do Caçador da Ilha | Incomum | Conquista: Caçador da Ilha da Aurora | — | 3 | — | Parede |
| `casa_parede_038` | Pergaminho do Dom Aprimorado | Raro | Conquista: Dom Aprimorado | — | 6 | — | Parede |

<a id="tapete"></a>
## Tapetes (23)

| ID | Nome | Raridade | Origem | Preço (ouro) | Conforto | Conjunto | Encaixe |
|---|---|---|---|---|---|---|---|
| `casa_tapete_001` | Tapete de retalhos redondo | Comum | Loja: Costureira Mestra Narisah | 14 | 2 | Iwan da Vó Edna | Piso |
| `casa_tapete_002` | Tapete de lã de camelo | Comum | Loja: Bazar da Nana | 15 | 2 | Areias Flamejantes | Piso |
| `casa_tapete_003` | Tapete de feltro cru | Comum | Loja: Bazar de Linhas da Mariam | 25 | 1 | — | Piso |
| `casa_tapete_004` | Tapete de caravaneiro de couro cru | Incomum | Loja: Caravana do Ammo | 90 | 4 | Caravana do Vô Jorge | Piso |
| `casa_tapete_005` | Esteira de palha de tamareira | Comum | Loja: Bazar da Nana | 12 | 1 | — | Piso |
| `casa_tapete_006` | Tapete de ikat florido | Comum | Loja: Costureira Mestra Narisah | 11 | 1 | Bazar da Nana | Piso |
| `casa_tapete_007` | Tapete suzani bordado | Raro | Loja: Costureira Mestra Narisah | 185 | 5 | — | Piso |
| `casa_tapete_008` | Passadeira de juta | Comum | Loja: Bazar da Nana | 24 | 2 | — | Piso |
| `casa_tapete_009` | Tapete de retalhos de seda colorido | Incomum | Loja: Bazar de Linhas da Mariam | 50 | 4 | — | Piso |
| `casa_tapete_010` | Tapete de pelúcia de nuvem | Incomum | Loja: Costureira Mestra Narisah | 50 | 4 | Luz da Maria | Piso |
| `casa_tapete_011` | Tapete de folhas de choupo | Incomum | Loja: Costureira Mestra Narisah | 70 | 4 | Tugai do Zarafshan | Piso |
| `casa_tapete_012` | Tapete de dunas ondulado | Incomum | Loja: Costureira Mestra Narisah | 45 | 3 | Areias Flamejantes | Piso |
| `casa_tapete_013` | Tapete listrado de lã da serra | Raro | Drop: Ushabti de Pedra | — | 6 | Picos de Litema | Piso |
| `casa_tapete_014` | Tapete de musgo das grutas | Raro | Drop: Enxame de Morcegos do Limiar | — | 5 | Tufo Rosa das Lamparinas | Piso |
| `casa_tapete_015` | Tapete de estrelas da meia-noite | Raro | Loja: Costureira Mestra Narisah | 150 | 6 | Noite Rubra da Lavine | Piso |
| `casa_tapete_016` | Tapete de amarelinha pintado | Comum | Loja: Bazar de Linhas da Mariam | 13 | 1 | — | Piso |
| `casa_tapete_017` | Piso de mosaico de azulejo | Incomum | Loja: Olaria da Náfsha | 90 | 4 | — | Piso |
| `casa_tapete_018` | Capacho "Limpe os pés, menino" | Comum | Loja: Armazém Geral | 8 | 1 | Iwan da Vó Edna | Piso |
| `casa_tapete_019` | Tapete de seda do caravançará | Épico | Loja: Caravana do Ammo | 350 | 9 | — | Piso |
| `casa_tapete_020` | Tapete de rosa-dos-ventos | Raro | Loja: Costureira Mestra Narisah | 130 | 5 | Ventania da Maithe | Piso |
| `casa_tapete_021` | Tapete de raízes entrelaçadas | Raro | Crafting: Bancada | — | 5 | Raízes do Joaquim | Piso |
| `casa_tapete_022` | Tapete de circuito impresso | Incomum | Crafting: Bancada | — | 3 | Oficina do Ícaro | Piso |
| `casa_tapete_023` | Tapete do sol nascente dourado | Épico | Drop: Tages, o Habitante Ancestral (chefe) | — | 8 | Cristal do Sol | Piso |

<a id="cortina"></a>
## Cortinas (18)

| ID | Nome | Raridade | Origem | Preço (ouro) | Conforto | Conjunto | Encaixe |
|---|---|---|---|---|---|---|---|
| `casa_cortina_001` | Cortina de ikat florida | Comum | Loja: Costureira Mestra Narisah | 20 | 2 | Bazar da Nana | Parede |
| `casa_cortina_002` | Cortina de seda bordada | Raro | Loja: Costureira Mestra Narisah | 210 | 5 | Iwan da Vó Edna | Parede |
| `casa_cortina_003` | Cortina de retalhos de seda | Incomum | Loja: Bazar de Linhas da Mariam | 40 | 4 | — | Parede |
| `casa_cortina_004` | Cortina de miçangas coloridas | Comum | Loja: Bazar de Linhas da Mariam | 21 | 2 | — | Parede |
| `casa_cortina_005` | Cortina de junco | Comum | Loja: Bazar da Nana | 24 | 1 | — | Parede |
| `casa_cortina_006` | Cortina de voal branco | Comum | Loja: Costureira Mestra Narisah | 12 | 2 | Luz da Maria | Parede |
| `casa_cortina_007` | Cortina de saco de grãos alvejado | Comum | Loja: Bazar da Nana | 27 | 2 | Moinho do Pomar | Parede |
| `casa_cortina_008` | Cortina de tule com estrelas | Incomum | Loja: Costureira Mestra Narisah | 35 | 3 | Noite Rubra da Lavine | Parede |
| `casa_cortina_009` | Cortina de linho cru com barrado | Comum | Loja: Costureira Mestra Narisah | 23 | 1 | — | Parede |
| `casa_cortina_010` | Cortina de algodão listrado | Comum | Loja: Costureira Mestra Narisah | 27 | 2 | — | Parede |
| `casa_cortina_011` | Cortina estampada de folhas de videira | Incomum | Loja: Costureira Mestra Narisah | 75 | 3 | Tugai do Zarafshan | Parede |
| `casa_cortina_012` | Cortina de tecido cor de areia | Incomum | Loja: Costureira Mestra Narisah | 65 | 3 | Areias Flamejantes | Parede |
| `casa_cortina_013` | Cortina de cobertor listrado da serra | Incomum | Loja: Costureira Mestra Narisah | 75 | 3 | Picos de Litema | Parede |
| `casa_cortina_014` | Cortina de contas de cristal | Raro | Loja: Lamparinas da Parthenos | 110 | 6 | Tufo Rosa das Lamparinas | Parede |
| `casa_cortina_015` | Cortina de veludo vinho de teatro | Raro | Loja: Caravana do Ammo | 170 | 6 | — | Parede |
| `casa_cortina_016` | Cortina de fitas do Bema | Incomum | Loja: Bazar de Linhas da Mariam | 50 | 4 | — | Parede |
| `casa_cortina_017` | Persiana de junco | Comum | Loja: Marcenaria do Mestre Omóforo | 25 | 1 | — | Parede |
| `casa_cortina_018` | Cortina blecaute de lona | Comum | Crafting: Bancada | — | 1 | Oficina do Ícaro | Parede |

<a id="planta"></a>
## Plantas (31)

| ID | Nome | Raridade | Origem | Preço (ouro) | Conforto | Conjunto | Encaixe |
|---|---|---|---|---|---|---|---|
| `casa_planta_001` | Samambaia de metro | Comum | Loja: Viveiro da Nana | 24 | 1 | — | Teto |
| `casa_planta_002` | Língua-de-sogra no vaso de lata | Comum | Loja: Viveiro da Nana | 21 | 2 | — | Piso |
| `casa_planta_003` | Pé de hortelã da ervateira | Comum | Loja: Viveiro da Nana | 17 | 1 | Luz da Maria | Piso |
| `casa_planta_004` | Vaso de alecrim | Comum | Loja: Viveiro da Nana | 13 | 1 | Horta dos Melões | Superfície |
| `casa_planta_005` | Saxaul no vaso de barro | Comum | Loja: Viveiro da Nana | 19 | 2 | Areias Flamejantes | Piso |
| `casa_planta_006` | Aloé de flor vermelha | Incomum | Loja: Viveiro da Nana | 50 | 4 | Areias Flamejantes | Piso |
| `casa_planta_007` | Espinheiro-de-camelo | Incomum | Drop: Escorpião das Areias Flamejantes | — | 4 | Areias Flamejantes | Piso |
| `casa_planta_008` | Tamargueira em vaso | Comum | Loja: Viveiro da Nana | 16 | 2 | Areias Flamejantes | Piso |
| `casa_planta_009` | Orquídea do tugai | Raro | Drop: Aranha-Tecelã Apagada | — | 6 | Tugai do Zarafshan | Superfície |
| `casa_planta_010` | Hera de tronco | Incomum | Loja: Viveiro da Nana | 55 | 3 | Tugai do Zarafshan | Parede |
| `casa_planta_011` | Avenca na meia-lua | Comum | Loja: Viveiro da Nana | 22 | 2 | — | Parede |
| `casa_planta_012` | Violeta na lata de damasco seco | Comum | Loja: Viveiro da Nana | 27 | 2 | Bazar da Nana | Superfície |
| `casa_planta_013` | Girassol no balde | Comum | Loja: Viveiro da Nana | 20 | 1 | — | Piso |
| `casa_planta_014` | Lírio-da-paz | Comum | Loja: Viveiro da Nana | 25 | 1 | Luz da Maria | Piso |
| `casa_planta_015` | Romãzeira-anã de vaso | Incomum | Loja: Viveiro da Nana | 45 | 3 | Tugai do Zarafshan | Piso |
| `casa_planta_016` | Bonsai de damasqueiro | Raro | Loja: Viveiro da Nana | 210 | 5 | — | Superfície |
| `casa_planta_017` | Choupo-turanga pequeno | Raro | Loja: Viveiro da Nana | 235 | 5 | — | Piso |
| `casa_planta_018` | Terrário de musgo das grutas | Incomum | Drop: Enxame de Morcegos do Limiar | — | 4 | Tufo Rosa das Lamparinas | Superfície |
| `casa_planta_019` | Cogumelo luminoso de vaso | Raro | Drop: Tages, o Habitante Ancestral (chefe) | — | 6 | Tufo Rosa das Lamparinas | Piso |
| `casa_planta_020` | Protea da serra | Incomum | Drop: Ushabti de Pedra | — | 4 | Picos de Litema | Superfície |
| `casa_planta_021` | Sempre-viva da serra | Raro | Drop: Hahya, o Gigante Guerreiro (chefe) | — | 5 | Picos de Litema | Superfície |
| `casa_planta_022` | Dioneia de vaso | Raro | Loja: Caravana do Ammo | 110 | 5 | — | Superfície |
| `casa_planta_023` | Rainha-da-noite em flor | Épico | Loja: Caravana do Ammo | 600 | 8 | Noite Rubra da Lavine | Piso |
| `casa_planta_024` | Vaso de lavanda | Comum | Loja: Viveiro da Nana | 12 | 2 | — | Superfície |
| `casa_planta_025` | Babosa no pote | Comum | Loja: Viveiro da Nana | 27 | 1 | Luz da Maria | Superfície |
| `casa_planta_026` | Bambu-da-sorte | Comum | Loja: Viveiro da Nana | 23 | 1 | — | Superfície |
| `casa_planta_027` | Jiboia pendente | Comum | Loja: Viveiro da Nana | 21 | 1 | — | Teto |
| `casa_planta_028` | Pimenteira ornamental | Comum | Loja: Viveiro da Nana | 23 | 2 | Horta dos Melões | Piso |
| `casa_planta_029` | Flor de romã em treliça | Incomum | Loja: Viveiro da Nana | 85 | 3 | Horta dos Melões | Parede |
| `casa_planta_030` | Vaso de flores de papel crepom | Comum | Loja: Bazar de Linhas da Mariam | 10 | 2 | — | Superfície |
| `casa_planta_031` | Muda de amoreira | Incomum | Loja: Viveiro da Nana | 80 | 4 | Moinho do Pomar | Piso |

<a id="musica"></a>
## Instrumentos musicais (23)

| ID | Nome | Raridade | Origem | Preço (ouro) | Conforto | Conjunto | Encaixe |
|---|---|---|---|---|---|---|---|
| `casa_musica_001` | Dutar de duas cordas | Raro | Loja: Casa de Música do Mestre Xroshtag | 130 | 5 | Bema | Superfície |
| `casa_musica_002` | Harmônio vermelho de fole | Épico | Loja: Casa de Música do Mestre Xroshtag | 440 | 9 | — | Piso |
| `casa_musica_003` | Triângulo de ferro | Comum | Loja: Casa de Música do Mestre Xroshtag | 19 | 2 | Bema | Parede |
| `casa_musica_004` | Tambor falante de couro | Incomum | Loja: Casa de Música do Mestre Xroshtag | 75 | 3 | Bema | Piso |
| `casa_musica_005` | Daf de platinela | Comum | Loja: Casa de Música do Mestre Xroshtag | 13 | 2 | — | Parede |
| `casa_musica_006` | Alaúde de dez cordas | Raro | Loja: Casa de Música do Mestre Xroshtag | 135 | 6 | — | Parede |
| `casa_musica_007` | Lira de tigela núbia | Raro | Loja: Casa de Música do Mestre Xroshtag | 180 | 6 | — | Parede |
| `casa_musica_008` | Flauta ney de junco | Comum | Loja: Casa de Música do Mestre Xroshtag | 22 | 2 | — | Parede |
| `casa_musica_009` | Arco musical de cabaça | Incomum | Loja: Casa de Música do Mestre Xroshtag | 75 | 3 | — | Parede |
| `casa_musica_010` | Gongo de lata | Comum | Loja: Casa de Música do Mestre Xroshtag | 8 | 1 | Festa das Lanternas | Superfície |
| `casa_musica_011` | Tambor do dragão das lanternas | Raro | Loja: Casa de Música do Mestre Xroshtag | 200 | 6 | Festa das Lanternas | Piso |
| `casa_musica_012` | Chocalho de lata | Comum | Loja: Casa de Música do Mestre Xroshtag | 21 | 1 | — | Superfície |
| `casa_musica_013` | Chocalho de palha | Comum | Loja: Casa de Música do Mestre Xroshtag | 8 | 1 | — | Superfície |
| `casa_musica_014` | Rubab de amoreira | Incomum | Loja: Casa de Música do Mestre Xroshtag | 80 | 4 | — | Parede |
| `casa_musica_015` | Violão de cordas de aço | Incomum | Loja: Casa de Música do Mestre Xroshtag | 50 | 4 | — | Parede |
| `casa_musica_016` | Gaita de boca de lata | Comum | Loja: Casa de Música do Mestre Xroshtag | 16 | 2 | Caravana do Vô Jorge | Superfície |
| `casa_musica_017` | Marimba de garrafas | Raro | Crafting: Bancada | — | 6 | Oficina do Ícaro | Piso |
| `casa_musica_018` | Harpa de vento da Maithe | Épico | Conquista: Olho do Vento | — | 9 | Ventania da Maithe | Quintal |
| `casa_musica_019` | Sininhos de vento de bambu | Comum | Loja: Bazar da Nana | 24 | 1 | — | Teto |
| `casa_musica_020` | Tambor de pedra da montanha | Raro | Drop: Hahya, o Gigante Guerreiro (chefe) | — | 6 | Picos de Litema | Piso |
| `casa_musica_021` | Flauta de cristal que assobia sozinha | Épico | Drop: Tages, o Habitante Ancestral (chefe) | — | 9 | Tufo Rosa das Lamparinas | Superfície |
| `casa_musica_022` | Matraca de madeira | Comum | Loja: Casa de Música do Mestre Xroshtag | 23 | 2 | — | Superfície |
| `casa_musica_023` | Metrônomo de sintonia | Incomum | Conquista: Em Sintonia | — | 4 | — | Superfície |

<a id="brinquedo"></a>
## Brinquedos (31)

| ID | Nome | Raridade | Origem | Preço (ouro) | Conforto | Conjunto | Encaixe |
|---|---|---|---|---|---|---|---|
| `casa_brinquedo_001` | Pião de madeira pintado | Comum | Loja: Bazar da Nana | 20 | 2 | — | Superfície |
| `casa_brinquedo_002` | Bilboquê de lata | Comum | Loja: Bazar da Nana | 12 | 1 | — | Superfície |
| `casa_brinquedo_003` | Boneca de pano de retalhos | Comum | Loja: Costureira Mestra Narisah | 29 | 1 | — | Superfície |
| `casa_brinquedo_004` | Carrinho de lata de sardinha | Comum | Loja: Bazar da Nana | 23 | 2 | — | Piso |
| `casa_brinquedo_005` | Pipa de papel de seda com rabiola | Comum | Loja: Bazar de Linhas da Mariam | 23 | 2 | Ventania da Maithe | Parede |
| `casa_brinquedo_006` | Peteca de penas de faisão | Comum | Loja: Bazar da Nana | 29 | 1 | — | Superfície |
| `casa_brinquedo_007` | Bola de meia remendada | Comum | Loja: Bazar de Linhas da Mariam | 19 | 1 | — | Piso |
| `casa_brinquedo_008` | Cavalinho de pau | Comum | Loja: Marcenaria do Mestre Omóforo | 30 | 1 | — | Piso |
| `casa_brinquedo_009` | Cavalo de balanço de madeira | Incomum | Loja: Marcenaria do Mestre Omóforo | 90 | 4 | — | Piso |
| `casa_brinquedo_010` | Pote de bolinhas de gude | Comum | Loja: Armazém Geral | 17 | 1 | — | Superfície |
| `casa_brinquedo_011` | Jogo de varetas | Comum | Loja: Armazém Geral | 21 | 1 | — | Superfície |
| `casa_brinquedo_012` | Tabuleiro de mancala de madeira | Comum | Loja: Bazar da Nana | 17 | 2 | — | Superfície |
| `casa_brinquedo_013` | Corda de pular com cabo de madeira | Comum | Loja: Bazar da Nana | 28 | 2 | — | Parede |
| `casa_brinquedo_014` | Perna de pau | Comum | Loja: Marcenaria do Mestre Omóforo | 10 | 2 | — | Parede |
| `casa_brinquedo_015` | Estilingue de forquilha de damasqueiro | Comum | Loja: Bazar da Nana | 29 | 1 | — | Parede |
| `casa_brinquedo_016` | Acrobata de madeira de cordinha | Incomum | Loja: Bazar da Nana | 65 | 3 | — | Superfície |
| `casa_brinquedo_017` | Zumbidor de lata | Comum | Loja: Bazar da Nana | 24 | 1 | — | Superfície |
| `casa_brinquedo_018` | Barquinho de papel na bacia | Comum | Loja: Bazar de Linhas da Mariam | 22 | 1 | — | Piso |
| `casa_brinquedo_019` | Cabeça de ushabti consertada | Raro | Conquista: Engrenagens Antigas | — | 6 | Oficina do Ícaro | Superfície |
| `casa_brinquedo_020` | Caravaninha de camelos de madeira | Incomum | Loja: Marcenaria do Mestre Omóforo | 75 | 3 | — | Piso |
| `casa_brinquedo_021` | Casinha de bonecas de adobe | Raro | Loja: Marcenaria do Mestre Omóforo | 125 | 5 | Iwan da Vó Edna | Piso |
| `casa_brinquedo_022` | Teatro de sombras de papel recortado | Épico | Loja: Caravana do Ammo | 515 | 8 | Iluminuras e Papel Recortado | Piso |
| `casa_brinquedo_023` | Bonequinhos de barro de Qocho | Incomum | Loja: Olaria da Náfsha | 55 | 3 | Iluminuras e Papel Recortado | Superfície |
| `casa_brinquedo_024` | Cata-vento de papel colorido | Comum | Loja: Bazar de Linhas da Mariam | 11 | 2 | — | Superfície |
| `casa_brinquedo_025` | Quebra-cabeça do mapa da ilha | Incomum | Loja: Armazém Geral | 90 | 4 | — | Superfície |
| `casa_brinquedo_026` | Telefone de lata com barbante | Comum | Loja: Bazar de Linhas da Mariam | 9 | 1 | Oficina do Ícaro | Parede |
| `casa_brinquedo_027` | Pelúcia de lobinho de olhos sonolentos | Raro | Drop: Lobo do Tugai | — | 5 | Tugai do Zarafshan | Superfície |
| `casa_brinquedo_028` | Pelúcia de escorpião sorridente | Raro | Drop: Escorpião das Areias Flamejantes | — | 6 | Areias Flamejantes | Superfície |
| `casa_brinquedo_029` | Pelúcia de morceguinho de cabeça pra baixo | Raro | Drop: Enxame de Morcegos do Limiar | — | 5 | Tufo Rosa das Lamparinas | Teto |
| `casa_brinquedo_030` | Ushabtizinho de pedra que fica de pé | Raro | Drop: Ushabti de Pedra | — | 5 | Picos de Litema | Superfície |
| `casa_brinquedo_031` | Aranhinha de crochê | Incomum | Drop: Aranha-Tecelã Apagada | — | 3 | Tugai do Zarafshan | Superfície |

<a id="regional"></a>
## Decoração regional por região (48)

| ID | Nome | Raridade | Origem | Preço (ouro) | Conforto | Conjunto | Encaixe |
|---|---|---|---|---|---|---|---|
| `casa_regional_001` | Tronco oco de choupo-turanga | Raro | Drop: Lobo do Tugai | — | 5 | Tugai do Zarafshan | Piso |
| `casa_regional_002` | Escultura de junco entrelaçado | Incomum | Drop: Aranha-Tecelã Apagada | — | 3 | Tugai do Zarafshan | Piso |
| `casa_regional_003` | Banco de raiz de choupo | Incomum | Loja: Marcenaria do Mestre Omóforo | 90 | 4 | Tugai do Zarafshan | Piso |
| `casa_regional_004` | Ninho de passarinho na forquilha | Comum | Drop: Lobo do Tugai | — | 2 | Tugai do Zarafshan | Superfície |
| `casa_regional_005` | Tora com orelha-de-pau | Comum | Drop: Aranha-Tecelã Apagada | — | 2 | Tugai do Zarafshan | Piso |
| `casa_regional_006` | Teia de orvalho emoldurada | Raro | Drop: Aranha-Tecelã Apagada | — | 5 | Tugai do Zarafshan | Parede |
| `casa_regional_007` | Pegada de lobo no gesso | Incomum | Drop: Lobo do Tugai | — | 3 | Tugai do Zarafshan | Parede |
| `casa_regional_008` | Casinha de árvore em miniatura | Raro | Loja: Marcenaria do Mestre Omóforo | 210 | 5 | Tugai do Zarafshan | Piso |
| `casa_regional_009` | Ramo de damasco com frutas | Comum | Loja: Bazar da Nana | 25 | 1 | Tugai do Zarafshan | Superfície |
| `casa_regional_010` | Pote de vagalumes do aryk | Incomum | Drop: Lobo do Tugai | — | 4 | Tugai do Zarafshan | Superfície |
| `casa_regional_011` | Placa da trilha do Zarafshan | Comum | Loja: Armazém Geral | 18 | 2 | Tugai do Zarafshan | Quintal |
| `casa_regional_012` | Pedra de musgo do Bosque | Comum | Drop: Lobo do Tugai | — | 1 | Tugai do Zarafshan | Quintal |
| `casa_regional_013` | Pote de areia colorida em camadas | Comum | Loja: Bazar da Nana | 9 | 2 | Areias Flamejantes | Superfície |
| `casa_regional_014` | Escultura de saxaul seco | Incomum | Drop: Escorpião das Areias Flamejantes | — | 4 | Areias Flamejantes | Piso |
| `casa_regional_015` | Coluna de lótus tombada | Raro | Drop: O Grande Ushabti (chefe) | — | 6 | Areias Flamejantes | Quintal |
| `casa_regional_016` | Painel de engrenagens dos Artífices | Épico | Drop: O Grande Ushabti (chefe) | — | 9 | Areias Flamejantes | Parede |
| `casa_regional_017` | Rosa-do-deserto de areia cristalizada | Raro | Drop: Escorpião das Areias Flamejantes | — | 5 | Areias Flamejantes | Superfície |
| `casa_regional_018` | Cantil de couro do caravaneiro | Incomum | Missão: Reserva pra Travessia | — | 4 | Caravana do Vô Jorge | Parede |
| `casa_regional_019` | Jarro de barro rachado das areias | Comum | Drop: Escorpião das Areias Flamejantes | — | 2 | Areias Flamejantes | Piso |
| `casa_regional_020` | Tenda de feltro de caravana | Raro | Loja: Caravana do Ammo | 145 | 6 | Areias Flamejantes | Quintal |
| `casa_regional_021` | Ampulheta de areia dourada | Incomum | Loja: Caravana do Ammo | 35 | 3 | Areias Flamejantes | Superfície |
| `casa_regional_022` | Placa "Cuidado com os ferrões" | Comum | Missão: Ferrões no Caminho | — | 2 | Areias Flamejantes | Quintal |
| `casa_regional_023` | Suculenta em forma de coração | Incomum | Loja: Viveiro da Nana | 45 | 3 | Areias Flamejantes | Superfície |
| `casa_regional_024` | Mini-pirâmide de Meroé | Raro | Drop: O Grande Ushabti (chefe) | — | 6 | Areias Flamejantes | Piso |
| `casa_regional_025` | Picaretas cruzadas do mineiro | Incomum | Missão: Pedras que Andam | — | 4 | Picos de Litema | Parede |
| `casa_regional_026` | Vagonete de mina com cristais | Raro | Drop: Ushabti de Pedra | — | 5 | Picos de Litema | Piso |
| `casa_regional_027` | Pedra pintada do eland | Comum | Drop: Ushabti de Pedra | — | 2 | Picos de Litema | Quintal |
| `casa_regional_028` | Bandeirola do acampamento do pico | Comum | Loja: Armazém Geral | 26 | 1 | Picos de Litema | Parede |
| `casa_regional_029` | Pedrinha que anda, sonolenta | Épico | Drop: Ushabti de Pedra | — | 8 | Picos de Litema | Piso |
| `casa_regional_030` | Estandarte desbotado de Hahya | Raro | Drop: Hahya, o Gigante Guerreiro (chefe) | — | 6 | Picos de Litema | Parede |
| `casa_regional_031` | Tora de lareira com neve | Comum | Loja: Bazar da Nana | 12 | 1 | Picos de Litema | Piso |
| `casa_regional_032` | Corda de escalada enrolada | Comum | Loja: Forja do Mestre Mihr | 30 | 1 | Picos de Litema | Parede |
| `casa_regional_033` | Barraca de lona do pico | Incomum | Loja: Caravana do Ammo | 75 | 3 | Picos de Litema | Quintal |
| `casa_regional_034` | Mapa dos veios de minério | Incomum | Loja: Forja do Mestre Mihr | 85 | 3 | Picos de Litema | Parede |
| `casa_regional_035` | Geodo de ametista aberto | Raro | Drop: Ushabti de Pedra | — | 6 | Picos de Litema | Superfície |
| `casa_regional_036` | Sino de pedra da serra | Raro | Drop: Hahya, o Gigante Guerreiro (chefe) | — | 6 | Picos de Litema | Teto |
| `casa_regional_037` | Estalactite de cristal suspensa | Raro | Drop: Enxame de Morcegos do Limiar | — | 5 | Tufo Rosa das Lamparinas | Teto |
| `casa_regional_038` | Vishapakar de jardim | Comum | Drop: Enxame de Morcegos do Limiar | — | 2 | Tufo Rosa das Lamparinas | Quintal |
| `casa_regional_039` | Lago do Limiar na bacia | Incomum | Drop: Enxame de Morcegos do Limiar | — | 4 | Tufo Rosa das Lamparinas | Piso |
| `casa_regional_040` | Lamparina apagada reacesa | Incomum | Missão: Lamparinas Apagadas | — | 4 | Tufo Rosa das Lamparinas | Superfície |
| `casa_regional_041` | Concha fóssil de quando ali era mar | Raro | Drop: Tages, o Habitante Ancestral (chefe) | — | 5 | Tufo Rosa das Lamparinas | Superfície |
| `casa_regional_042` | Casinha de morcego de madeira | Comum | Loja: Lamparinas da Parthenos | 28 | 1 | Tufo Rosa das Lamparinas | Quintal |
| `casa_regional_043` | Bolota de luz devolvida | Épico | Drop: Tages, o Habitante Ancestral (chefe) | — | 9 | Tufo Rosa das Lamparinas | Superfície |
| `casa_regional_044` | Pintura rupestre de mãozinhas | Incomum | Drop: Tages, o Habitante Ancestral (chefe) | — | 4 | Tufo Rosa das Lamparinas | Parede |
| `casa_regional_045` | Cristal cantante | Raro | Drop: Enxame de Morcegos do Limiar | — | 5 | Tufo Rosa das Lamparinas | Superfície |
| `casa_regional_046` | Pedra fria de sombra apaziguada | Raro | Drop: Tages, o Habitante Ancestral (chefe) | — | 6 | Noite Rubra da Lavine | Superfície |
| `casa_regional_047` | Mapa dos túneis da Parthenos | Incomum | Loja: Lamparinas da Parthenos | 35 | 3 | Tufo Rosa das Lamparinas | Parede |
| `casa_regional_048` | Raiz que brilha do teto da gruta | Incomum | Drop: Enxame de Morcegos do Limiar | — | 4 | Tufo Rosa das Lamparinas | Teto |

<a id="trofeu"></a>
## Troféus de chefes (24)

| ID | Nome | Raridade | Origem | Preço (ouro) | Conforto | Conjunto | Encaixe |
|---|---|---|---|---|---|---|---|
| `casa_trofeu_001` | Placa "Kök-Böri voltou pro tugai" | Incomum | Drop: Kök-Böri, o Lobo Celeste (chefe) | — | 3 | — | Parede |
| `casa_trofeu_002` | Coleira de couro de Kök-Böri | Raro | Drop: Kök-Böri, o Lobo Celeste (chefe) | — | 6 | — | Parede |
| `casa_trofeu_003` | Concha que guarda o uivo de Kök-Böri | Raro | Drop: Kök-Böri, o Lobo Celeste (chefe) | — | 6 | — | Superfície |
| `casa_trofeu_004` | Pedrinha escura desfeita do Bosque | Épico | Drop: Kök-Böri, o Lobo Celeste (chefe) | — | 8 | — | Superfície |
| `casa_trofeu_005` | Cabeça entalhada de Kök-Böri | Épico | Drop: Kök-Böri, o Lobo Celeste (chefe) | — | 9 | — | Parede |
| `casa_trofeu_006` | Estátua de Kök-Böri dormindo | Lendário | Drop: Kök-Böri, o Lobo Celeste (chefe) | — | 14 | — | Piso |
| `casa_trofeu_007` | Placa "O Grande Ushabti descansa" | Incomum | Drop: O Grande Ushabti (chefe) | — | 3 | — | Parede |
| `casa_trofeu_008` | Elmo rachado do Grande Ushabti | Raro | Drop: O Grande Ushabti (chefe) | — | 5 | — | Parede |
| `casa_trofeu_009` | Frasco de areia dourada do rastro | Raro | Drop: O Grande Ushabti (chefe) | — | 6 | — | Superfície |
| `casa_trofeu_010` | Engrenagem-coração do Grande Ushabti | Épico | Drop: O Grande Ushabti (chefe) | — | 8 | — | Superfície |
| `casa_trofeu_011` | Cabeça entalhada do Grande Ushabti | Épico | Drop: O Grande Ushabti (chefe) | — | 9 | — | Parede |
| `casa_trofeu_012` | Miniatura do Grande Ushabti de vigia | Lendário | Drop: O Grande Ushabti (chefe) | — | 12 | — | Piso |
| `casa_trofeu_013` | Placa "Obrigado, crianças" | Incomum | Drop: Hahya, o Gigante Guerreiro (chefe) | — | 4 | — | Parede |
| `casa_trofeu_014` | Capa remendada de Hahya | Raro | Drop: Hahya, o Gigante Guerreiro (chefe) | — | 6 | — | Parede |
| `casa_trofeu_015` | Elmo com pena da serra | Raro | Drop: Hahya, o Gigante Guerreiro (chefe) | — | 5 | — | Parede |
| `casa_trofeu_016` | Espada embainhada de Hahya | Épico | Drop: Hahya, o Gigante Guerreiro (chefe) | — | 8 | — | Parede |
| `casa_trofeu_017` | Cabeça entalhada de Hahya | Épico | Drop: Hahya, o Gigante Guerreiro (chefe) | — | 9 | — | Parede |
| `casa_trofeu_018` | Estátua de Hahya ajoelhado em paz | Lendário | Drop: Hahya, o Gigante Guerreiro (chefe) | — | 12 | — | Piso |
| `casa_trofeu_019` | Placa "Dá pra dividir" | Incomum | Drop: Tages, o Habitante Ancestral (chefe) | — | 4 | — | Parede |
| `casa_trofeu_020` | Manta de sombra macia | Raro | Drop: Tages, o Habitante Ancestral (chefe) | — | 5 | Noite Rubra da Lavine | Superfície |
| `casa_trofeu_021` | Lanterna que Tages aprendeu a dividir | Raro | Drop: Tages, o Habitante Ancestral (chefe) | — | 6 | — | Superfície |
| `casa_trofeu_022` | Lasquinha do Cristal ainda morna | Épico | Drop: Tages, o Habitante Ancestral (chefe) | — | 8 | Cristal do Sol | Superfície |
| `casa_trofeu_023` | Cabeça entalhada de Tages | Épico | Drop: Tages, o Habitante Ancestral (chefe) | — | 9 | — | Parede |
| `casa_trofeu_024` | Estátua de Tages abraçando a luz | Lendário | Drop: Tages, o Habitante Ancestral (chefe) | — | 15 | Cristal do Sol | Piso |

<a id="sazonal"></a>
## Itens sazonais (45)

| ID | Nome | Raridade | Origem | Preço (ouro) | Conforto | Conjunto | Encaixe |
|---|---|---|---|---|---|---|---|
| `casa_sazonal_001` | Cordão de lanternas de cinco cores | Comum | Evento: Bema | — (7 oferendas) | 1 | Bema | Teto |
| `casa_sazonal_002` | Bema de cinco degraus em miniatura | Incomum | Evento: Bema | — (23 oferendas) | 3 | Bema | Quintal |
| `casa_sazonal_003` | Lanterna de papel enfeitada, sem fogo | Comum | Evento: Bema | — (9 oferendas) | 1 | Bema | Teto |
| `casa_sazonal_004` | Barraca da Pescaria do Bema | Raro | Evento: Bema | — (50 oferendas) | 5 | Bema | Quintal |
| `casa_sazonal_005` | Barraca de halva | Incomum | Evento: Bema | — (25 oferendas) | 4 | Bema | Quintal |
| `casa_sazonal_006` | Barraca de samsa | Incomum | Evento: Bema | — (21 oferendas) | 3 | Bema | Quintal |
| `casa_sazonal_007` | Mastro de fitas nas cinco cores | Raro | Evento: Bema | — (38 oferendas) | 6 | Bema | Quintal |
| `casa_sazonal_008` | Casal de espantalhos do pomar | Incomum | Evento: Bema | — (15 oferendas) | 3 | Bema | Quintal |
| `casa_sazonal_009` | Caixinha de bilhetes de luz | Comum | Evento: Bema | — (10 oferendas) | 2 | Bema | Superfície |
| `casa_sazonal_010` | Chapéu de palha remendado do Bema | Comum | Evento: Bema | — (5 oferendas) | 1 | Bema | Parede |
| `casa_sazonal_011` | Mesa das oferendas de luz | Raro | Evento: Bema | — (32 oferendas) | 6 | Bema | Piso |
| `casa_sazonal_012` | Travessa de damascos secos | Comum | Evento: Bema | — (6 oferendas) | 2 | Bema | Superfície |
| `casa_sazonal_013` | Fatias de melão claro | Comum | Evento: Bema | — (10 oferendas) | 2 | Bema | Superfície |
| `casa_sazonal_014` | Arco de bambu com lanterninhas | Incomum | Evento: Bema | — (24 oferendas) | 3 | Bema | Quintal |
| `casa_sazonal_015` | Palco dos Hinos do Bema | Épico | Evento: Bema | — (86 oferendas) | 8 | Bema | Quintal |
| `casa_sazonal_016` | Boi das Canções de pano | Épico | Evento: Bema | — (85 oferendas) | 9 | Bema | Piso |
| `casa_sazonal_017` | Lira enfeitada de fitas | Raro | Evento: Bema | — (44 oferendas) | 6 | Bema | Parede |
| `casa_sazonal_018` | Posto da Guarda de mentirinha | Incomum | Evento: Bema | — (24 oferendas) | 4 | Bema | Quintal |
| `casa_sazonal_019` | Cena de barro da Noite de Yalda | Raro | Evento: Noite de Yalda | — (43 oferendas) | 6 | Noite de Yalda | Piso |
| `casa_sazonal_020` | Árvore de Yalda com romãs penduradas | Incomum | Evento: Noite de Yalda | — (19 oferendas) | 4 | Noite de Yalda | Piso |
| `casa_sazonal_021` | Estrela da aurora de palha | Comum | Evento: Noite de Yalda | — (9 oferendas) | 1 | Noite de Yalda | Teto |
| `casa_sazonal_022` | Guirlanda de folhas de romãzeira | Comum | Evento: Noite de Yalda | — (5 oferendas) | 1 | Noite de Yalda | Parede |
| `casa_sazonal_023` | Contador de histórias de gorro de feltro | Raro | Evento: Noite de Yalda | — (38 oferendas) | 6 | Noite de Yalda | Piso |
| `casa_sazonal_024` | Meia de Yalda de ikat | Comum | Evento: Noite de Yalda | — (6 oferendas) | 2 | Noite de Yalda | Parede |
| `casa_sazonal_025` | Pisca-pisca de lamparinas | Incomum | Evento: Noite de Yalda | — (24 oferendas) | 3 | Noite de Yalda | Teto |
| `casa_sazonal_026` | Mesa de Yalda com romãs e melancia | Raro | Evento: Noite de Yalda | — (31 oferendas) | 5 | Noite de Yalda | Piso |
| `casa_sazonal_027` | Estandarte da noite mais longa | Incomum | Evento: Noite de Yalda | — (15 oferendas) | 3 | Noite de Yalda | Parede |
| `casa_sazonal_028` | Sino de Yalda de lata | Comum | Evento: Noite de Yalda | — (9 oferendas) | 1 | Noite de Yalda | Teto |
| `casa_sazonal_029` | Carroça de romãs puxada por asno | Épico | Evento: Noite de Yalda | — (86 oferendas) | 9 | Noite de Yalda | Quintal |
| `casa_sazonal_030` | Neve de algodão no peitoril | Comum | Evento: Noite de Yalda | — (10 oferendas) | 2 | Noite de Yalda | Superfície |
| `casa_sazonal_031` | Bonecos da Barca da Lua | Raro | Evento: Noite de Yalda | — (41 oferendas) | 5 | Noite de Yalda | Piso |
| `casa_sazonal_032` | Bolo de romã de Yalda | Comum | Evento: Noite de Yalda | — (6 oferendas) | 2 | Noite de Yalda | Superfície |
| `casa_sazonal_033` | Lanterna-peixe | Comum | Evento: Festa das Lanternas | — (6 oferendas) | 2 | Festa das Lanternas | Parede |
| `casa_sazonal_034` | Boneco gigante de dragão de papel | Épico | Evento: Festa das Lanternas | — (83 oferendas) | 8 | Festa das Lanternas | Piso |
| `casa_sazonal_035` | Estandarte das lanternas | Raro | Evento: Festa das Lanternas | — (36 oferendas) | 5 | Festa das Lanternas | Parede |
| `casa_sazonal_036` | Máscara de leão de papel | Incomum | Evento: Festa das Lanternas | — (18 oferendas) | 3 | Festa das Lanternas | Parede |
| `casa_sazonal_037` | Serpentinas e confetes no lustre | Comum | Evento: Festa das Lanternas | — (7 oferendas) | 1 | Festa das Lanternas | Teto |
| `casa_sazonal_038` | Coroa de papel dourado | Raro | Evento: Festa das Lanternas | — (35 oferendas) | 6 | Festa das Lanternas | Superfície |
| `casa_sazonal_039` | Fantasia de dragão no cabide | Comum | Evento: Festa das Lanternas | — (9 oferendas) | 1 | Festa das Lanternas | Piso |
| `casa_sazonal_040` | Boneca-lanterna de papel | Incomum | Evento: Festa das Lanternas | — (16 oferendas) | 4 | Festa das Lanternas | Superfície |
| `casa_sazonal_041` | Barco-lanterna em miniatura | Raro | Evento: Festa das Lanternas | — (44 oferendas) | 5 | Festa das Lanternas | Superfície |
| `casa_sazonal_042` | Vara de lanterna enfitada | Incomum | Evento: Festa das Lanternas | — (25 oferendas) | 4 | Festa das Lanternas | Parede |
| `casa_sazonal_043` | Bandeira dos "Primos das Lanternas" | Comum | Evento: Festa das Lanternas | — (7 oferendas) | 1 | Festa das Lanternas | Parede |
| `casa_sazonal_044` | Sombrinhas de papel de seda penduradas no teto | Incomum | Evento: Festa das Lanternas | — (18 oferendas) | 3 | Festa das Lanternas | Teto |
| `casa_sazonal_045` | Chapéu de enigmas das lanternas | Raro | Evento: Festa das Lanternas | — (40 oferendas) | 6 | Festa das Lanternas | Parede |

<a id="familia"></a>
## Raros e lendários com história de família (27)

| ID | Nome | Raridade | Origem | Preço (ouro) | Conforto | Conjunto | Encaixe |
|---|---|---|---|---|---|---|---|
| `casa_familia_001` | Retrato de casamento da Vó Edna e do Vô Jorge | Lendário | Missão da Casa: Casa Cheia | — | 14 | Iwan da Vó Edna | Parede |
| `casa_familia_002` | Rede onde a Vó Edna embalou os cinco netos | Lendário | Missão da Casa: Tijolo por Tijolo | — | 12 | Iwan da Vó Edna | Parede |
| `casa_familia_003` | Baú de lata do caravaneiro Vô Jorge | Lendário | Segredo do mapa (moinho do pomar do Vô Jorge) | — | 13 | Caravana do Vô Jorge | Piso |
| `casa_familia_004` | Pote de torrão dourado que nunca esvazia | Lendário | Conquista: Acender de Novo | — | 12 | Cozinha da Vó | Superfície |
| `casa_familia_005` | Livro de receitas escrito à mão da Vó Edna | Épico | Diária: Um Prato Quente | — | 9 | Cozinha da Vó | Superfície |
| `casa_familia_006` | Caixa de costura de madrepérola da Vó Edna | Épico | Diária: A Cesta de Ervas da Maria | — | 9 | Iwan da Vó Edna | Superfície |
| `casa_familia_007` | Medalhão de luz da Maria em redoma | Épico | Conquista: Luz que Não se Apaga | — | 9 | Luz da Maria | Superfície |
| `casa_familia_008` | Raiz-mãe do Escudo das Raízes | Épico | Conquista: Ecos de Casa | — | 8 | Raízes do Joaquim | Piso |
| `casa_familia_009` | Fita vermelha da coragem emoldurada | Épico | Conquista: Coragem da Caçula | — | 8 | Noite Rubra da Lavine | Parede |
| `casa_familia_010` | Drone pintado com as cores do arco | Épico | Conquista: Vento e Engrenagem | — | 9 | Oficina do Ícaro | Teto |
| `casa_familia_011` | Escudo dourado de luz e raiz | Épico | Conquista: Escudo e Luz | — | 8 | Raízes do Joaquim | Parede |
| `casa_familia_012` | Sela de prata da camela Teimosa | Lendário | Drop: O Grande Ushabti (chefe) | — | 12 | Caravana do Vô Jorge | Piso |
| `casa_familia_013` | Carta de amor do Vô Jorge emoldurada | Épico | Segredo do mapa (moinho do pomar do Vô Jorge) | — | 9 | Caravana do Vô Jorge | Parede |
| `casa_familia_014` | Relógio de bolso do Vô Jorge parado às seis | Lendário | Drop: Tages, o Habitante Ancestral (chefe) | — | 15 | Caravana do Vô Jorge | Superfície |
| `casa_familia_015` | Moinho de vento em miniatura do bisavô | Épico | Loja: Caravana do Ammo | 350 | 9 | Moinho do Pomar | Superfície |
| `casa_familia_016` | Primeiro torrão dourado do moinho, petrificado | Lendário | Segredo do mapa (moinho do pomar do Vô Jorge) | — | 13 | Moinho do Pomar | Superfície |
| `casa_familia_017` | Cristal do Sol em miniatura | Lendário | Drop: Tages, o Habitante Ancestral (chefe) | — | 14 | Cristal do Sol | Superfície |
| `casa_familia_018` | Mantinha de bebê da Lavine bordada de lua | Épico | Missão pessoal da Lavine (`quest_lavine_redtide`) | — | 8 | Noite Rubra da Lavine | Superfície |
| `casa_familia_019` | Caderno de invenções do Ícaro | Épico | Missão pessoal do Ícaro (`quest_icaro_ancient_gears`) | — | 8 | Oficina do Ícaro | Superfície |
| `casa_familia_020` | Mochila remendada da Maithe | Épico | Missão: A Reunião (`quest_convergence_the_gathering`) | — | 9 | Ventania da Maithe | Parede |
| `casa_familia_021` | Bolo de damasco eterno de cerâmica | Raro | Loja: Olaria da Náfsha | 165 | 5 | Iwan da Vó Edna | Superfície |
| `casa_familia_022` | Panela de barro centenária da família | Épico | Loja: Caravana do Ammo | 410 | 9 | Cozinha da Vó | Superfície |
| `casa_familia_023` | Chapéu de feltro do bisavô caravaneiro | Épico | Loja: Caravana do Ammo | 550 | 9 | Caravana do Vô Jorge | Parede |
| `casa_familia_024` | Nicho de lâmpada da bisavó com lamparina de cobre | Lendário | Evento: Noite de Yalda | — | 12 | Luz da Maria | Parede |
| `casa_familia_025` | Bandeira do primeiro Bema da Vila | Lendário | Evento: Bema | — | 14 | Bema | Parede |
| `casa_familia_026` | Máscara de lanterna da juventude da Vó Edna | Lendário | Evento: Festa das Lanternas | — | 15 | Festa das Lanternas | Parede |
| `casa_familia_027` | Primeiro arco de damasqueiro da Maithe | Épico | Missão pessoal da Maithe (`quest_maithe_windtrail`) | — | 8 | Ventania da Maithe | Parede |

<a id="util"></a>
## Utilitários (baús, prateleiras, bancadas) (31)

| ID | Nome | Raridade | Origem | Preço (ouro) | Conforto | Conjunto | Encaixe |
|---|---|---|---|---|---|---|---|
| `casa_util_001` | Baú de madeira simples | Comum | Missão da Casa: Um Cantinho Seu | — | 0 | — | Piso |
| `casa_util_002` | Baú de couro tacheado | Incomum | Loja: Marcenaria do Mestre Omóforo | 85 | 1 | Caravana do Vô Jorge | Piso |
| `casa_util_003` | Baú de ferro reforçado | Raro | Loja: Forja do Mestre Mihr | 120 | 2 | — | Piso |
| `casa_util_004` | Arca de cedro com cadeado de segredo | Épico | Loja: Forja do Mestre Mihr | 565 | 4 | — | Piso |
| `casa_util_005` | Baú-correio da Vó Edna | Épico | Missão da Casa: Tijolo por Tijolo | — | 4 | Iwan da Vó Edna | Piso |
| `casa_util_006` | Guarda-roupa de duas portas | Comum | Loja: Marcenaria do Mestre Omóforo | 15 | 0 | — | Piso |
| `casa_util_007` | Guarda-roupa de jacarandá com espelho | Raro | Loja: Marcenaria do Mestre Omóforo | 230 | 3 | — | Piso |
| `casa_util_008` | Cabideiro de provador | Comum | Loja: Costureira Mestra Narisah | 19 | 0 | — | Piso |
| `casa_util_009` | Manequim de costura | Incomum | Loja: Costureira Mestra Narisah | 45 | 1 | — | Piso |
| `casa_util_010` | Espelho de provador de três folhas | Incomum | Loja: Marcenaria do Mestre Omóforo | 65 | 1 | — | Piso |
| `casa_util_011` | Bancada de marceneiro | Incomum | Loja: Marcenaria do Mestre Omóforo | 80 | 2 | — | Piso |
| `casa_util_012` | Bancada de mestre artesão | Épico | Conquista: Mestre Artesão | — | 4 | — | Piso |
| `casa_util_013` | Forja de quintal | Raro | Loja: Forja do Mestre Mihr | 195 | 2 | — | Quintal |
| `casa_util_014` | Mesa de alquimia da ervateira | Raro | Loja: Bazar de Linhas da Mariam | 105 | 2 | Luz da Maria | Piso |
| `casa_util_015` | Tear de seda de madeira | Raro | Loja: Costureira Mestra Narisah | 155 | 3 | — | Piso |
| `casa_util_016` | Bacia de tingimento de índigo | Incomum | Loja: Bazar de Linhas da Mariam | 65 | 1 | — | Piso |
| `casa_util_017` | Prateleira de tábua simples | Comum | Loja: Marcenaria do Mestre Omóforo | 14 | 0 | — | Parede |
| `casa_util_018` | Prateleira flutuante de cedro | Incomum | Loja: Marcenaria do Mestre Omóforo | 70 | 2 | — | Parede |
| `casa_util_019` | Estante do Caderno da Vó | Raro | Conquista: Caderno da Vó, de Capa a Capa | — | 2 | — | Piso |
| `casa_util_020` | Vitrine das Lembranças | Lendário | Conquista: Lembranças do Vô Jorge (`ach_relics_all`) | — | 6 | — | Piso |
| `casa_util_021` | Pedestal dos Gigantes Tombados | Raro | Conquista: Gigante Tombado | — | 3 | — | Piso |
| `casa_util_022` | Parede de troféus de madeira | Incomum | Loja: Marcenaria do Mestre Omóforo | 85 | 1 | — | Parede |
| `casa_util_023` | Livro de visitas de capa de couro | Comum | Loja: Bazar de Linhas da Mariam | 19 | 1 | — | Superfície |
| `casa_util_024` | Caixa de correio de lata | Comum | Loja: Forja do Mestre Mihr | 20 | 1 | — | Quintal |
| `casa_util_025` | Cofrinho de porquinho de barro cheio | Raro | Conquista: Bom de Negócio | — | 3 | — | Superfície |
| `casa_util_026` | Sino do portão | Comum | Loja: Forja do Mestre Mihr | 28 | 1 | Iwan da Vó Edna | Quintal |
| `casa_util_027` | Quadro de avisos da família | Comum | Loja: Bazar de Linhas da Mariam | 20 | 1 | — | Parede |
| `casa_util_028` | Caixote de ferramentas | Comum | Loja: Forja do Mestre Mihr | 17 | 1 | — | Piso |
| `casa_util_029` | Gaveteiro de sementes | Incomum | Loja: Viveiro da Nana | 75 | 1 | Horta dos Melões | Piso |
| `casa_util_030` | Regador do ajudante com placa de horário | Comum | Loja: Viveiro da Nana | 24 | 1 | Horta dos Melões | Quintal |
| `casa_util_031` | Balcão de vendas do ajudante | Raro | Loja: Marcenaria do Mestre Omóforo | 150 | 2 | — | Piso |

<a id="conjuntos"></a>
## Conjuntos (resumo)

Bônus de conjunto em Housing.md §6.2. Peças por conjunto:

| Conjunto | Tema | Peças | Lendárias/Épicas |
|---|---|---|---|
| Iwan da Vó Edna | o alpendre de arco da casa da Vó: tapetes, almofadas e a cadeira do Vô | 26 | 4 |
| Caravana do Vô Jorge | feltro, couro, sino de camelo e lembrança do avô | 25 | 6 |
| Moinho do Pomar | pilão, caldeirão e mó do velho moinho do pomar | 11 | 4 |
| Cozinha da Vó | tandir, cobre e barro | 16 | 3 |
| Oficina do Ícaro | engrenagens, fios e invenções | 17 | 4 |
| Ventania da Maithe | arco, pipa e rosa-dos-ventos | 9 | 4 |
| Luz da Maria | branco, dourado e ervas de cura | 13 | 4 |
| Raízes do Joaquim | madeira de lei, raiz e escudo | 8 | 4 |
| Noite Rubra da Lavine | estrelas, lua e sombra macia | 14 | 6 |
| Cantinho do Ari | tudo do cachorro da família | 7 | 0 |
| Tugai do Zarafshan | Bosque do Zarafshan | 29 | 0 |
| Areias Flamejantes | Areias Flamejantes e Ruínas dos Artífices | 24 | 2 |
| Picos de Litema | Serra de Kaggen | 22 | 2 |
| Tufo Rosa das Lamparinas | Grutas do Limiar | 22 | 4 |
| Bazar da Nana | ikat, réstias e balança de bazar | 12 | 1 |
| Iluminuras e Papel Recortado | hinos iluminados, papel recortado e barro | 7 | 1 |
| Horta dos Melões | canteiros, abelhas e regador | 14 | 0 |
| Cristal do Sol | a luz que voltou para casa | 6 | 5 |
| Bema | o festival do Bema (junho) | 22 | 3 |
| Noite de Yalda | a noite mais longa do ano, quando a luz começa a voltar | 14 | 1 |
| Festa das Lanternas | lanternas de papel de seda | 16 | 2 |

<a id="drops"></a>
## Itens de casa exclusivos de drop, por inimigo

Quantos itens de cada raridade estão no sorteio próprio de cada inimigo (além do sorteio regional da loja do bioma). Chances em Housing.md §7.2.

| Inimigo | ID | Comum | Incomum | Raro | Épico | Lendário | Total |
|---|---|---|---|---|---|---|---|
| Lobo do Tugai | `enemy_forest_wolf` | 2 | 2 | 2 | 0 | 0 | 6 |
| Aranha-Tecelã Apagada | `enemy_forest_spider` | 1 | 2 | 3 | 0 | 0 | 6 |
| Kök-Böri, o Lobo Celeste (chefe) | `enemy_forest_alpha_wolf` | 0 | 1 | 3 | 2 | 1 | 7 |
| Escorpião das Areias Flamejantes | `enemy_desert_scorpion` | 1 | 2 | 2 | 0 | 0 | 5 |
| O Grande Ushabti (chefe) | `enemy_desert_ruin_guardian` | 0 | 1 | 4 | 4 | 2 | 11 |
| Ushabti de Pedra | `enemy_mountain_stone_golem` | 1 | 2 | 4 | 1 | 0 | 8 |
| Hahya, o Gigante Guerreiro (chefe) | `enemy_mountain_warlord` | 0 | 1 | 6 | 2 | 1 | 10 |
| Enxame de Morcegos do Limiar | `enemy_cave_bat_swarm` | 1 | 4 | 5 | 0 | 0 | 10 |
| Tages, o Habitante Ancestral (chefe) | `enemy_cave_dweller` | 0 | 2 | 5 | 7 | 3 | 17 |
