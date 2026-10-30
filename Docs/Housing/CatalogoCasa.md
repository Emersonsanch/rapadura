# Catálogo de itens de casa — Rapadura

> Gerado a partir de uma lista curada (cada nome escrito à mão). Design do sistema em [`Docs/Housing.md`](../Housing.md).
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
| [Decoração regional por bioma](#regional) | 48 |
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
- **Preço:** em ouro (`item_gold`), só para itens de loja. Lendário nunca é vendido. "—" = não se compra. Itens de evento mostram o preço em **prendas** (moeda de evento, ver Housing.md §8.3). Faixas: Comum 8–30, Incomum 35–90, Raro 100–250, Épico 300–700 (ver Housing.md §10).
- **Conforto:** pontos somados ao conforto da casa (Comum 1–2, Incomum 3–4, Raro 5–6, Épico 8–9, Lendário 12–15; utilitários valem metade).
- **Encaixe:** Piso, Parede, Teto, Superfície (em cima de mesa/estante/cômoda) ou Quintal.
- **Origem "Drop":** o item entra no sorteio de itens de casa daquele inimigo (tabela de chances em Housing.md §7.2). Itens de loja também podem cair de qualquer inimigo do bioma da cidade da loja, nunca acima de Raro.

<a id="sala"></a>
## Móveis de sala (45)

| ID | Nome | Raridade | Origem | Preço (ouro) | Conforto | Conjunto | Encaixe |
|---|---|---|---|---|---|---|---|
| `casa_sala_001` | Sofá de palhinha da varanda | Comum | Loja: Marcenaria do Mestre Damião | 24 | 1 | Varanda da Vó Zefa | Piso |
| `casa_sala_002` | Banco de madeira comprido | Comum | Loja: Marcenaria do Mestre Damião | 27 | 2 | — | Piso |
| `casa_sala_003` | Cadeira de balanço da Vó Zefa | Raro | Missão: A Reunião (`quest_convergence_the_gathering`) | — | 5 | Varanda da Vó Zefa | Piso |
| `casa_sala_004` | Mesa de centro de tronco | Comum | Loja: Marcenaria do Mestre Damião | 11 | 1 | Mata Fechada | Piso |
| `casa_sala_005` | Estante de tábua de demolição | Incomum | Loja: Marcenaria do Mestre Damião | 80 | 4 | — | Piso |
| `casa_sala_006` | Cadeira de macarrão colorida | Comum | Loja: Feira livre | 15 | 2 | Feira Livre | Piso |
| `casa_sala_007` | Poltrona de couro curtido | Incomum | Loja: Marcenaria do Mestre Damião | 45 | 3 | Tropeiro do Vô Chico | Piso |
| `casa_sala_008` | Banquinho de três pés | Comum | Loja: Marcenaria do Mestre Damião | 12 | 2 | — | Piso |
| `casa_sala_009` | Rede de varanda bordada | Raro | Loja: Costureira Dona Fátima | 205 | 5 | Varanda da Vó Zefa | Parede |
| `casa_sala_010` | Rede de algodão listrada | Comum | Loja: Costureira Dona Fátima | 29 | 2 | — | Parede |
| `casa_sala_011` | Mesa de jantar de jatobá | Raro | Loja: Marcenaria do Mestre Damião | 170 | 5 | — | Piso |
| `casa_sala_012` | Cristaleira de vidro bisotado | Épico | Loja: Marcenaria do Mestre Damião | 580 | 8 | — | Piso |
| `casa_sala_013` | Aparador com toalhinha de crochê | Incomum | Loja: Armarinho da Dona Cida | 70 | 4 | Varanda da Vó Zefa | Piso |
| `casa_sala_014` | Rádio da Vó Zefa remontado | Épico | Missão pessoal do Ícaro (`quest_icaro_ancient_gears`) | — | 8 | Oficina do Ícaro | Superfície |
| `casa_sala_015` | Televisão de tubo com antena de palha de aço | Incomum | Loja: Mascate Seu Bento | 85 | 3 | — | Piso |
| `casa_sala_016` | Mocho de couro de bode | Comum | Loja: Feira livre | 11 | 1 | Tropeiro do Vô Chico | Piso |
| `casa_sala_017` | Baú-banco de cedro | Incomum | Loja: Marcenaria do Mestre Damião | 75 | 3 | — | Piso |
| `casa_sala_018` | Mesa de dominó com quatro tamboretes | Incomum | Loja: Marcenaria do Mestre Damião | 50 | 4 | — | Piso |
| `casa_sala_019` | Cadeira de lona de diretor | Comum | Loja: Costureira Dona Fátima | 27 | 1 | — | Piso |
| `casa_sala_020` | Namoradeira de ferro batido | Raro | Loja: Forja do Mestre Tonho | 250 | 5 | — | Piso |
| `casa_sala_021` | Sofá de três lugares de chita | Incomum | Loja: Costureira Dona Fátima | 45 | 3 | Feira Livre | Piso |
| `casa_sala_022` | Almofada de chita florida | Comum | Loja: Armarinho da Dona Cida | 22 | 2 | Feira Livre | Superfície |
| `casa_sala_023` | Almofada de fuxico | Comum | Loja: Armarinho da Dona Cida | 12 | 1 | Varanda da Vó Zefa | Superfície |
| `casa_sala_024` | Pufe de couro trançado | Incomum | Loja: Feira livre | 50 | 4 | Tropeiro do Vô Chico | Piso |
| `casa_sala_025` | Mesa de canto de carretel | Comum | Loja: Marcenaria do Mestre Damião | 14 | 1 | — | Piso |
| `casa_sala_026` | Cadeira de vime da comadre | Incomum | Loja: Feira livre | 65 | 3 | — | Piso |
| `casa_sala_027` | Oratório de madeira | Raro | Loja: Marcenaria do Mestre Damião | 100 | 6 | Luz da Maria | Parede |
| `casa_sala_028` | Mesinha do telefone | Incomum | Loja: Mascate Seu Bento | 70 | 4 | — | Piso |
| `casa_sala_029` | Telefone de disco vermelho | Incomum | Loja: Mascate Seu Bento | 55 | 3 | — | Superfície |
| `casa_sala_030` | Vitrola de maleta | Raro | Loja: Casa de Música do Seu Bené | 125 | 5 | — | Superfície |
| `casa_sala_031` | Revisteiro de cipó | Comum | Loja: Feira livre | 24 | 1 | Mata Fechada | Piso |
| `casa_sala_032` | Cabideiro de galhos secos | Comum | Loja: Marcenaria do Mestre Damião | 26 | 1 | Mata Fechada | Piso |
| `casa_sala_033` | Bandeja de flandres com xícaras | Comum | Loja: Forja do Mestre Tonho | 9 | 2 | — | Superfície |
| `casa_sala_034` | Porta-retrato dos cinco primos | Raro | Conquista: Juntos, ou Nada Feito | — | 5 | Varanda da Vó Zefa | Superfície |
| `casa_sala_035` | Cadeira de couro de vaqueiro | Raro | Loja: Mascate Seu Bento | 115 | 6 | Tropeiro do Vô Chico | Piso |
| `casa_sala_036` | Estante de folhetos de cordel | Incomum | Loja: Armarinho da Dona Cida | 40 | 4 | Cordel e Xilogravura | Piso |
| `casa_sala_037` | Banquinho de engraxate | Comum | Loja: Feira livre | 25 | 1 | — | Piso |
| `casa_sala_038` | Sofá de paletes com futon | Comum | Loja: Marcenaria do Mestre Damião | 20 | 1 | — | Piso |
| `casa_sala_039` | Tamborete de pau-ferro | Incomum | Loja: Marcenaria do Mestre Damião | 80 | 4 | — | Piso |
| `casa_sala_040` | Mesa de fórmica amarela | Comum | Loja: Mercearia do Seu Lázaro | 14 | 1 | — | Piso |
| `casa_sala_041` | Lareira de pedra da serra | Épico | Crafting: Bancada | — | 8 | Pico Nevado | Piso |
| `casa_sala_042` | Banquinho torto feito à mão | Comum | Conquista: Feito à Mão | — | 1 | — | Piso |
| `casa_sala_043` | Bota calejada de bronze | Incomum | Conquista: Calejado | — | 4 | — | Superfície |
| `casa_sala_044` | Banco de tronco de ipê do Joaquim | Raro | Loja: Marcenaria do Mestre Damião | 150 | 5 | Raízes do Joaquim | Piso |
| `casa_sala_045` | Muralha de pedrinhas em miniatura | Incomum | Loja: Olaria da Dona Rosa | 60 | 4 | Raízes do Joaquim | Superfície |

<a id="quarto"></a>
## Quarto (35)

| ID | Nome | Raridade | Origem | Preço (ouro) | Conforto | Conjunto | Encaixe |
|---|---|---|---|---|---|---|---|
| `casa_quarto_001` | Cama de casal de jacarandá | Épico | Loja: Marcenaria do Mestre Damião | 530 | 8 | — | Piso |
| `casa_quarto_002` | Cama de solteiro de cabeceira torneada | Incomum | Loja: Marcenaria do Mestre Damião | 65 | 3 | — | Piso |
| `casa_quarto_003` | Beliche dos primos | Raro | Conquista: Prima de Todos | — | 6 | Varanda da Vó Zefa | Piso |
| `casa_quarto_004` | Berço de embalo antigo | Raro | Loja: Mascate Seu Bento | 175 | 6 | Varanda da Vó Zefa | Piso |
| `casa_quarto_005` | Colchão de palha de milho | Comum | Loja: Feira livre | 28 | 2 | — | Piso |
| `casa_quarto_006` | Colcha de retalhos da Vó Zefa | Raro | Missão da Casa: Parede de Barro | — | 6 | Varanda da Vó Zefa | Superfície |
| `casa_quarto_007` | Colcha de fuxico | Incomum | Loja: Costureira Dona Fátima | 50 | 4 | — | Superfície |
| `casa_quarto_008` | Travesseiro de macela | Comum | Loja: Feira livre | 12 | 1 | — | Superfície |
| `casa_quarto_009` | Criado-mudo com gavetinha | Comum | Loja: Marcenaria do Mestre Damião | 17 | 1 | — | Piso |
| `casa_quarto_010` | Penteadeira com espelho oval | Raro | Loja: Marcenaria do Mestre Damião | 135 | 6 | — | Piso |
| `casa_quarto_011` | Cômoda de quatro gavetas | Incomum | Loja: Marcenaria do Mestre Damião | 45 | 3 | — | Piso |
| `casa_quarto_012` | Mosquiteiro de filó | Comum | Loja: Armarinho da Dona Cida | 23 | 2 | — | Teto |
| `casa_quarto_013` | Lençol bordado de richelieu | Incomum | Loja: Costureira Dona Fátima | 50 | 4 | — | Superfície |
| `casa_quarto_014` | Cabide de pé de vime | Comum | Loja: Feira livre | 30 | 1 | — | Piso |
| `casa_quarto_015` | Baú de enxoval da bisavó | Raro | Loja: Marcenaria do Mestre Damião | 235 | 5 | Varanda da Vó Zefa | Piso |
| `casa_quarto_016` | Cama de vento dobrável | Comum | Loja: Armazém Geral | 15 | 2 | — | Piso |
| `casa_quarto_017` | Rede de dormir de punho bordado | Incomum | Loja: Costureira Dona Fátima | 65 | 3 | — | Parede |
| `casa_quarto_018` | Escrivaninha de estudante | Incomum | Loja: Marcenaria do Mestre Damião | 85 | 3 | Oficina do Ícaro | Piso |
| `casa_quarto_019` | Cadeira giratória de escritório | Incomum | Loja: Mascate Seu Bento | 60 | 4 | Oficina do Ícaro | Piso |
| `casa_quarto_020` | Mural de cortiça com desenhos | Comum | Loja: Armarinho da Dona Cida | 14 | 1 | — | Parede |
| `casa_quarto_021` | Cama de dossel de chita | Épico | Loja: Costureira Dona Fátima | 560 | 8 | Feira Livre | Piso |
| `casa_quarto_022` | Catre de couro do tropeiro | Raro | Loja: Mascate Seu Bento | 200 | 5 | Tropeiro do Vô Chico | Piso |
| `casa_quarto_023` | Esteira de taboa | Comum | Loja: Feira livre | 20 | 2 | Mata Fechada | Piso |
| `casa_quarto_024` | Caixinha de música de bailarina | Raro | Loja: Mascate Seu Bento | 220 | 5 | — | Superfície |
| `casa_quarto_025` | Cabeceira entalhada de mandacaru | Raro | Loja: Marcenaria do Mestre Damião | 170 | 5 | Duna Dourada | Piso |
| `casa_quarto_026` | Lençol de estrelinhas que brilham no escuro | Incomum | Loja: Armarinho da Dona Cida | 80 | 4 | Noite Rubra da Lavine | Superfície |
| `casa_quarto_027` | Cama da Noite Rubra | Épico | Missão pessoal da Lavine (`quest_lavine_redtide`) | — | 8 | Noite Rubra da Lavine | Piso |
| `casa_quarto_028` | Cama-forte do Joaquim com escudo na cabeceira | Épico | Missão pessoal do Joaquim (`quest_joaquim_echoes_of_home`) | — | 9 | Raízes do Joaquim | Piso |
| `casa_quarto_029` | Cama de dossel de linho branco da Maria | Épico | Missão pessoal da Maria (`quest_maria_fading_light`) | — | 9 | Luz da Maria | Piso |
| `casa_quarto_030` | Cama-oficina do Ícaro com gavetas de parafuso | Épico | Missão pessoal do Ícaro (`quest_icaro_ancient_gears`) | — | 9 | Oficina do Ícaro | Piso |
| `casa_quarto_031` | Cama de toras da Maithe com aljava na cabeceira | Épico | Missão pessoal da Maithe (`quest_maithe_windtrail`) | — | 9 | Ventania da Maithe | Piso |
| `casa_quarto_032` | Cobertor de lã de ovelha da serra | Incomum | Drop: Golem de Pedra | — | 3 | Pico Nevado | Superfície |
| `casa_quarto_033` | Gaveteiro de meias perdidas | Comum | Loja: Marcenaria do Mestre Damião | 29 | 2 | — | Piso |
| `casa_quarto_034` | Cabide de chapéus de parede | Comum | Loja: Marcenaria do Mestre Damião | 12 | 1 | Tropeiro do Vô Chico | Parede |
| `casa_quarto_035` | Espelho de corpo inteiro com moldura de cipó | Incomum | Loja: Marcenaria do Mestre Damião | 55 | 3 | Mata Fechada | Piso |

<a id="cozinha"></a>
## Cozinha (44)

| ID | Nome | Raridade | Origem | Preço (ouro) | Conforto | Conjunto | Encaixe |
|---|---|---|---|---|---|---|---|
| `casa_cozinha_001` | Fogão a lenha de tijolo | Raro | Missão da Casa: Fogo na Cozinha | — | 5 | Cozinha da Vó | Piso |
| `casa_cozinha_002` | Filtro de barro da Vó Zefa | Raro | Missão da Casa: Fogo na Cozinha | — | 6 | Cozinha da Vó | Superfície |
| `casa_cozinha_003` | Pote de rapadura | Comum | Loja: Mercearia do Seu Lázaro | 20 | 1 | Cozinha da Vó | Superfície |
| `casa_cozinha_004` | Talha d'água de barro | Comum | Loja: Olaria da Dona Rosa | 18 | 1 | — | Piso |
| `casa_cozinha_005` | Moringa de barro com caneca | Comum | Loja: Olaria da Dona Rosa | 17 | 2 | — | Superfície |
| `casa_cozinha_006` | Panela de barro preta | Comum | Loja: Olaria da Dona Rosa | 22 | 1 | Cozinha da Vó | Superfície |
| `casa_cozinha_007` | Pilão de madeira | Comum | Loja: Marcenaria do Mestre Damião | 17 | 1 | Casa de Farinha | Piso |
| `casa_cozinha_008` | Gamela de madeira | Comum | Loja: Marcenaria do Mestre Damião | 19 | 1 | Casa de Farinha | Superfície |
| `casa_cozinha_009` | Colher de pau gigante | Comum | Loja: Feira livre | 21 | 2 | — | Parede |
| `casa_cozinha_010` | Cuscuzeira de alumínio | Comum | Loja: Mercearia do Seu Lázaro | 20 | 1 | Cozinha da Vó | Superfície |
| `casa_cozinha_011` | Coador de pano com suporte | Comum | Loja: Mercearia do Seu Lázaro | 24 | 1 | Cozinha da Vó | Superfície |
| `casa_cozinha_012` | Bule de ágata azul | Incomum | Loja: Mercearia do Seu Lázaro | 70 | 4 | Cozinha da Vó | Superfície |
| `casa_cozinha_013` | Caneca de ágata com borda preta | Comum | Loja: Mercearia do Seu Lázaro | 24 | 1 | — | Superfície |
| `casa_cozinha_014` | Lata de biscoito de goma | Comum | Loja: Mercearia do Seu Lázaro | 13 | 2 | — | Superfície |
| `casa_cozinha_015` | Réstia de alho | Comum | Loja: Feira livre | 24 | 2 | Feira Livre | Parede |
| `casa_cozinha_016` | Réstia de cebola roxa | Comum | Loja: Feira livre | 8 | 2 | Feira Livre | Parede |
| `casa_cozinha_017` | Cacho de bananas pendurado | Comum | Loja: Feira livre | 22 | 2 | Feira Livre | Teto |
| `casa_cozinha_018` | Prateleira de temperos | Incomum | Loja: Marcenaria do Mestre Damião | 75 | 3 | — | Parede |
| `casa_cozinha_019` | Mesa de cozinha com banco | Comum | Loja: Marcenaria do Mestre Damião | 28 | 2 | — | Piso |
| `casa_cozinha_020` | Geladeira antiga arredondada | Raro | Loja: Mascate Seu Bento | 225 | 6 | — | Piso |
| `casa_cozinha_021` | Paneleiro de ferro | Incomum | Loja: Forja do Mestre Tonho | 85 | 3 | — | Parede |
| `casa_cozinha_022` | Jogo de panelas de ferro | Incomum | Loja: Forja do Mestre Tonho | 45 | 3 | Cozinha da Vó | Superfície |
| `casa_cozinha_023` | Tacho de cobre para doce | Raro | Loja: Forja do Mestre Tonho | 105 | 6 | Casa de Farinha | Piso |
| `casa_cozinha_024` | Fruteira de cerâmica | Comum | Loja: Olaria da Dona Rosa | 13 | 1 | — | Superfície |
| `casa_cozinha_025` | Peneira de taquara | Comum | Loja: Feira livre | 22 | 2 | Casa de Farinha | Parede |
| `casa_cozinha_026` | Pano de prato dos dias da semana | Comum | Loja: Armarinho da Dona Cida | 20 | 2 | — | Parede |
| `casa_cozinha_027` | Cesto de ovos caipira | Comum | Loja: Feira livre | 15 | 2 | — | Superfície |
| `casa_cozinha_028` | Jarra de suco de cajá | Comum | Loja: Mercearia do Seu Lázaro | 24 | 1 | — | Superfície |
| `casa_cozinha_029` | Pote de doce de leite | Comum | Loja: Mercearia do Seu Lázaro | 12 | 1 | — | Superfície |
| `casa_cozinha_030` | Vidros de conserva de pimenta | Incomum | Loja: Feira livre | 85 | 3 | — | Superfície |
| `casa_cozinha_031` | Tábua de queijo coalho | Incomum | Loja: Mercearia do Seu Lázaro | 35 | 3 | Cozinha da Vó | Superfície |
| `casa_cozinha_032` | Ralador de milho de lata | Comum | Loja: Forja do Mestre Tonho | 24 | 2 | — | Superfície |
| `casa_cozinha_033` | Moedor de café de manivela | Incomum | Loja: Forja do Mestre Tonho | 85 | 3 | — | Superfície |
| `casa_cozinha_034` | Torrador de café de lata | Incomum | Loja: Forja do Mestre Tonho | 60 | 4 | Casa de Farinha | Piso |
| `casa_cozinha_035` | Balança de pratos da venda | Raro | Loja: Mascate Seu Bento | 140 | 5 | Feira Livre | Superfície |
| `casa_cozinha_036` | Cristaleira de copos de requeijão | Incomum | Loja: Marcenaria do Mestre Damião | 85 | 3 | — | Piso |
| `casa_cozinha_037` | Toalha de mesa xadrez | Comum | Loja: Costureira Dona Fátima | 19 | 2 | — | Superfície |
| `casa_cozinha_038` | Fogareiro de duas bocas | Comum | Loja: Armazém Geral | 22 | 1 | — | Piso |
| `casa_cozinha_039` | Mó de pedra rachada da casa de farinha | Épico | Segredo do mapa (casa de farinha do Vô Chico) | — | 9 | Casa de Farinha | Piso |
| `casa_cozinha_040` | Forno de torrar farinha | Épico | Crafting: Forja | — | 8 | Casa de Farinha | Piso |
| `casa_cozinha_041` | Bolo de milho na forma | Comum | Loja: Mercearia do Seu Lázaro | 15 | 1 | Varanda da Vó Zefa | Superfície |
| `casa_cozinha_042` | Abano de palha do fogão | Comum | Loja: Feira livre | 26 | 2 | Cozinha da Vó | Parede |
| `casa_cozinha_043` | Marmita de alumínio do vaqueiro | Incomum | Loja: Mascate Seu Bento | 55 | 3 | Tropeiro do Vô Chico | Superfície |
| `casa_cozinha_044` | Cesta da Mão Amiga | Comum | Conquista: Mão Amiga | — | 2 | — | Superfície |

<a id="banheiro"></a>
## Banheiro (21)

| ID | Nome | Raridade | Origem | Preço (ouro) | Conforto | Conjunto | Encaixe |
|---|---|---|---|---|---|---|---|
| `casa_banheiro_001` | Bacia de ágata com jarro | Comum | Loja: Mercearia do Seu Lázaro | 22 | 2 | — | Superfície |
| `casa_banheiro_002` | Tina de banho de madeira | Incomum | Loja: Marcenaria do Mestre Damião | 40 | 4 | — | Piso |
| `casa_banheiro_003` | Banheira de pés de leão | Épico | Loja: Mascate Seu Bento | 455 | 9 | — | Piso |
| `casa_banheiro_004` | Espelho de moldura de conchinhas | Incomum | Loja: Feira livre | 75 | 3 | — | Parede |
| `casa_banheiro_005` | Toalheiro de bambu | Comum | Loja: Marcenaria do Mestre Damião | 11 | 2 | — | Parede |
| `casa_banheiro_006` | Sabonete de erva-doce artesanal | Comum | Loja: Feira livre | 8 | 1 | — | Superfície |
| `casa_banheiro_007` | Sabonetes de arruda da Dona Cida | Incomum | Loja: Armarinho da Dona Cida | 55 | 3 | Luz da Maria | Superfície |
| `casa_banheiro_008` | Chuveiro de lata furada | Comum | Loja: Forja do Mestre Tonho | 19 | 2 | — | Parede |
| `casa_banheiro_009` | Caixa d'água de zinco | Comum | Loja: Forja do Mestre Tonho | 8 | 1 | — | Teto |
| `casa_banheiro_010` | Pia de louça com pedestal | Incomum | Loja: Olaria da Dona Rosa | 45 | 3 | — | Piso |
| `casa_banheiro_011` | Tapete de banheiro de crochê | Comum | Loja: Armarinho da Dona Cida | 20 | 1 | — | Piso |
| `casa_banheiro_012` | Cesto de roupa suja de palha | Comum | Loja: Feira livre | 14 | 1 | — | Piso |
| `casa_banheiro_013` | Penico esmaltado com florzinha | Comum | Loja: Mercearia do Seu Lázaro | 30 | 1 | — | Piso |
| `casa_banheiro_014` | Kit de barbear do Vô Chico | Raro | Segredo do mapa (casa de farinha do Vô Chico) | — | 5 | Tropeiro do Vô Chico | Superfície |
| `casa_banheiro_015` | Armarinho de remédio com cruz vermelha | Comum | Loja: Armazém Geral | 30 | 2 | — | Parede |
| `casa_banheiro_016` | Patinho de borracha amarelo | Comum | Loja: Armarinho da Dona Cida | 22 | 2 | — | Superfície |
| `casa_banheiro_017` | Cortina de box de chita | Comum | Loja: Costureira Dona Fátima | 25 | 1 | Feira Livre | Parede |
| `casa_banheiro_018` | Biombo de palha trançada | Incomum | Loja: Feira livre | 65 | 3 | — | Piso |
| `casa_banheiro_019` | Banquinho de banho de madeira | Comum | Loja: Marcenaria do Mestre Damião | 29 | 2 | — | Piso |
| `casa_banheiro_020` | Chuveiro solar de serpentina | Raro | Crafting: Bancada | — | 6 | Oficina do Ícaro | Parede |
| `casa_banheiro_021` | Banheira de pedra de fonte termal | Épico | Drop: Habitante Ancestral (chefe) | — | 8 | Cristal das Cavernas | Piso |

<a id="quintal"></a>
## Quintal e horta (39)

| ID | Nome | Raridade | Origem | Preço (ouro) | Conforto | Conjunto | Encaixe |
|---|---|---|---|---|---|---|---|
| `casa_quintal_001` | Poço de pedra com sarilho | Raro | Missão da Casa: Água Fresca | — | 6 | Varanda da Vó Zefa | Quintal |
| `casa_quintal_002` | Horta de canteiro elevado | Comum | Loja: Viveiro da Comadre Neném | 15 | 2 | Horta Caprichada | Quintal |
| `casa_quintal_003` | Canteiro de couve | Comum | Loja: Viveiro da Comadre Neném | 24 | 1 | Horta Caprichada | Quintal |
| `casa_quintal_004` | Canteiro de coentro e cebolinha | Comum | Loja: Viveiro da Comadre Neném | 26 | 1 | Horta Caprichada | Quintal |
| `casa_quintal_005` | Pé de cajá | Incomum | Loja: Viveiro da Comadre Neném | 80 | 4 | — | Quintal |
| `casa_quintal_006` | Pé de caju | Incomum | Loja: Viveiro da Comadre Neném | 75 | 3 | — | Quintal |
| `casa_quintal_007` | Pé de umbu | Raro | Loja: Viveiro da Comadre Neném | 140 | 6 | Duna Dourada | Quintal |
| `casa_quintal_008` | Pé de jabuticaba | Raro | Loja: Viveiro da Comadre Neném | 200 | 6 | — | Quintal |
| `casa_quintal_009` | Pé de manga-rosa | Incomum | Loja: Viveiro da Comadre Neném | 70 | 4 | — | Quintal |
| `casa_quintal_010` | Bananeira de quintal | Comum | Loja: Viveiro da Comadre Neném | 26 | 1 | — | Quintal |
| `casa_quintal_011` | Pé de acerola | Comum | Loja: Viveiro da Comadre Neném | 12 | 1 | Horta Caprichada | Quintal |
| `casa_quintal_012` | Espantalho de chapéu de palha | Incomum | Loja: Costureira Dona Fátima | 70 | 4 | Horta Caprichada | Quintal |
| `casa_quintal_013` | Forno de barro de quintal | Incomum | Loja: Olaria da Dona Rosa | 60 | 4 | Cozinha da Vó | Quintal |
| `casa_quintal_014` | Varal de arame com prendedores de madeira | Comum | Loja: Armazém Geral | 17 | 2 | — | Quintal |
| `casa_quintal_015` | Tanque de lavar de cimento | Comum | Loja: Armazém Geral | 27 | 2 | — | Quintal |
| `casa_quintal_016` | Cerca de varas de bambu | Comum | Loja: Marcenaria do Mestre Damião | 10 | 1 | — | Quintal |
| `casa_quintal_017` | Porteira de madeira de sítio | Incomum | Loja: Marcenaria do Mestre Damião | 60 | 4 | Varanda da Vó Zefa | Quintal |
| `casa_quintal_018` | Cata-vento de chapa | Raro | Loja: Forja do Mestre Tonho | 120 | 6 | Oficina do Ícaro | Quintal |
| `casa_quintal_019` | Cisterna de placas | Raro | Crafting: Bancada | — | 5 | — | Quintal |
| `casa_quintal_020` | Bica d'água de bambu | Comum | Loja: Marcenaria do Mestre Damião | 14 | 2 | Mata Fechada | Quintal |
| `casa_quintal_021` | Banco de pedra sob a mangueira | Incomum | Loja: Olaria da Dona Rosa | 35 | 3 | — | Quintal |
| `casa_quintal_022` | Balanço de pneu | Comum | Loja: Armazém Geral | 24 | 1 | — | Quintal |
| `casa_quintal_023` | Fogueira de quintal com toras | Comum | Crafting: Bancada | — | 2 | — | Quintal |
| `casa_quintal_024` | Mesa de piquenique de tora | Comum | Loja: Marcenaria do Mestre Damião | 22 | 2 | Mata Fechada | Quintal |
| `casa_quintal_025` | Caixa de abelhas jandaíra | Raro | Loja: Viveiro da Comadre Neném | 120 | 5 | Horta Caprichada | Quintal |
| `casa_quintal_026` | Composteira de madeira | Comum | Loja: Viveiro da Comadre Neném | 26 | 2 | Horta Caprichada | Quintal |
| `casa_quintal_027` | Regador de lata | Comum | Loja: Forja do Mestre Tonho | 18 | 1 | Horta Caprichada | Quintal |
| `casa_quintal_028` | Carrinho de mão de madeira | Comum | Loja: Forja do Mestre Tonho | 18 | 1 | — | Quintal |
| `casa_quintal_029` | Pergolado com maracujá | Raro | Loja: Viveiro da Comadre Neném | 140 | 5 | — | Quintal |
| `casa_quintal_030` | Chafariz de pedra-sabão | Épico | Loja: Mascate Seu Bento | 335 | 9 | — | Quintal |
| `casa_quintal_031` | Rede entre dois coqueiros | Raro | Loja: Costureira Dona Fátima | 180 | 6 | — | Quintal |
| `casa_quintal_032` | Coqueiro anão | Incomum | Loja: Viveiro da Comadre Neném | 50 | 4 | — | Quintal |
| `casa_quintal_033` | Pedra de amolar de manivela | Incomum | Loja: Forja do Mestre Tonho | 80 | 4 | — | Quintal |
| `casa_quintal_034` | Laguinho de peixes com vitória-régia | Épico | Loja: Viveiro da Comadre Neném | 635 | 8 | — | Quintal |
| `casa_quintal_035` | Caminho de pedrinhas | Comum | Loja: Olaria da Dona Rosa | 11 | 1 | — | Quintal |
| `casa_quintal_036` | Caramanchão de buganvília | Raro | Loja: Viveiro da Comadre Neném | 150 | 6 | — | Quintal |
| `casa_quintal_037` | Boneco de treino de palha | Comum | Loja: Armazém Geral | 27 | 1 | — | Quintal |
| `casa_quintal_038` | Árvore de raízes expostas para escalar | Épico | Loja: Viveiro da Comadre Neném | 320 | 8 | Raízes do Joaquim | Quintal |
| `casa_quintal_039` | Alvo de palha para treino de arco | Incomum | Loja: Armazém Geral | 90 | 4 | Ventania da Maithe | Quintal |

<a id="curral"></a>
## Curral e animais (27)

| ID | Nome | Raridade | Origem | Preço (ouro) | Conforto | Conjunto | Encaixe |
|---|---|---|---|---|---|---|---|
| `casa_curral_001` | Casinha de cachorro do Caramelo | Incomum | Missão da Casa: Um Cantinho Seu | — | 4 | Cantinho do Caramelo | Quintal |
| `casa_curral_002` | Caminha de pano do Caramelo | Comum | Loja: Armarinho da Dona Cida | 20 | 2 | Cantinho do Caramelo | Piso |
| `casa_curral_003` | Tigela de ágata do Caramelo | Comum | Loja: Mercearia do Seu Lázaro | 20 | 2 | Cantinho do Caramelo | Piso |
| `casa_curral_004` | Bolinha de tênis babada do Caramelo | Comum | Loja: Armarinho da Dona Cida | 16 | 2 | Cantinho do Caramelo | Piso |
| `casa_curral_005` | Osso de brinquedo de corda | Comum | Loja: Feira livre | 17 | 1 | Cantinho do Caramelo | Piso |
| `casa_curral_006` | Placa "Cuidado: cão carinhoso" | Comum | Loja: Armarinho da Dona Cida | 17 | 1 | Cantinho do Caramelo | Quintal |
| `casa_curral_007` | Galinheiro de varas | Comum | Loja: Marcenaria do Mestre Damião | 15 | 1 | — | Quintal |
| `casa_curral_008` | Ninho de palha com ovos | Comum | Loja: Feira livre | 30 | 1 | — | Quintal |
| `casa_curral_009` | Chiqueirinho da porca Mimosa | Incomum | Loja: Feira livre | 75 | 3 | — | Quintal |
| `casa_curral_010` | Curral de aroeira | Incomum | Loja: Marcenaria do Mestre Damião | 65 | 3 | Tropeiro do Vô Chico | Quintal |
| `casa_curral_011` | Cocho de madeira escavada | Comum | Loja: Marcenaria do Mestre Damião | 17 | 2 | — | Quintal |
| `casa_curral_012` | Bebedouro de pedra | Comum | Loja: Olaria da Dona Rosa | 28 | 1 | — | Quintal |
| `casa_curral_013` | Sela de couro trabalhada no cavalete | Raro | Loja: Mascate Seu Bento | 160 | 6 | Tropeiro do Vô Chico | Quintal |
| `casa_curral_014` | Cangalha de mula | Incomum | Loja: Mascate Seu Bento | 90 | 4 | Tropeiro do Vô Chico | Quintal |
| `casa_curral_015` | Carro de boi enfeitado | Épico | Loja: Mascate Seu Bento | 395 | 8 | Tropeiro do Vô Chico | Quintal |
| `casa_curral_016` | Galinha-d'angola ciscando | Incomum | Loja: Feira livre | 85 | 3 | — | Quintal |
| `casa_curral_017` | Pato marreco do tanque | Comum | Loja: Feira livre | 9 | 1 | — | Quintal |
| `casa_curral_018` | Bode Ioiô de estimação | Raro | Loja: Mascate Seu Bento | 165 | 6 | — | Quintal |
| `casa_curral_019` | Jumento Sereno no cercado | Raro | Loja: Mascate Seu Bento | 115 | 5 | — | Quintal |
| `casa_curral_020` | Papagaio no poleiro que fala "rapadura" | Raro | Loja: Mascate Seu Bento | 195 | 5 | — | Piso |
| `casa_curral_021` | Gaiola aberta de passarinho liberto | Comum | Loja: Feira livre | 14 | 1 | — | Parede |
| `casa_curral_022` | Comedouro de beija-flor | Comum | Loja: Viveiro da Comadre Neném | 19 | 2 | Horta Caprichada | Quintal |
| `casa_curral_023` | Casinha de joão-de-barro | Incomum | Loja: Viveiro da Comadre Neném | 85 | 3 | Mata Fechada | Quintal |
| `casa_curral_024` | Chocalho de vaca no prego | Comum | Loja: Forja do Mestre Tonho | 15 | 2 | Tropeiro do Vô Chico | Parede |
| `casa_curral_025` | Baia da mula Teimosa | Raro | Segredo do mapa (casa de farinha do Vô Chico) | — | 6 | Tropeiro do Vô Chico | Quintal |
| `casa_curral_026` | Manjedoura de feno | Comum | Loja: Feira livre | 9 | 2 | — | Quintal |
| `casa_curral_027` | Ferradura da sorte na porteira | Incomum | Loja: Forja do Mestre Tonho | 60 | 4 | Tropeiro do Vô Chico | Quintal |

<a id="luz"></a>
## Iluminação (30)

| ID | Nome | Raridade | Origem | Preço (ouro) | Conforto | Conjunto | Encaixe |
|---|---|---|---|---|---|---|---|
| `casa_luz_001` | Lampião de querosene | Comum | Loja: Lamparinas da Benedita | 13 | 1 | — | Superfície |
| `casa_luz_002` | Lamparina de lata | Comum | Loja: Lamparinas da Benedita | 9 | 1 | — | Superfície |
| `casa_luz_003` | Candeeiro de azeite de barro | Comum | Loja: Olaria da Dona Rosa | 13 | 1 | — | Superfície |
| `casa_luz_004` | Lustre de roda de carroça | Raro | Loja: Forja do Mestre Tonho | 185 | 6 | Tropeiro do Vô Chico | Teto |
| `casa_luz_005` | Luminária de cabaça recortada | Incomum | Loja: Feira livre | 35 | 3 | Duna Dourada | Teto |
| `casa_luz_006` | Abajur de cúpula plissada | Comum | Loja: Marcenaria do Mestre Damião | 18 | 2 | — | Superfície |
| `casa_luz_007` | Varal de lâmpadas de festa | Incomum | Loja: Lamparinas da Benedita | 50 | 4 | — | Teto |
| `casa_luz_008` | Vela de cera de abelha | Comum | Loja: Viveiro da Comadre Neném | 23 | 2 | — | Superfície |
| `casa_luz_009` | Castiçal de três velas de latão | Incomum | Loja: Forja do Mestre Tonho | 55 | 3 | — | Superfície |
| `casa_luz_010` | Lanterna de vagalume | Raro | Drop: Aranha da Floresta | — | 5 | Mata Fechada | Superfície |
| `casa_luz_011` | Luminária de cristal das cavernas | Raro | Drop: Enxame de Morcegos | — | 5 | Cristal das Cavernas | Superfície |
| `casa_luz_012` | Lampião de ferroviário | Incomum | Loja: Mascate Seu Bento | 90 | 4 | — | Superfície |
| `casa_luz_013` | Lamparina da Benedita | Raro | Missão: A Última Lamparina | — | 6 | Cristal das Cavernas | Superfície |
| `casa_luz_014` | Lanterna de papel de seda | Comum | Loja: Lamparinas da Benedita | 11 | 2 | — | Teto |
| `casa_luz_015` | Lustre de garrafas coloridas | Incomum | Loja: Lamparinas da Benedita | 60 | 4 | — | Teto |
| `casa_luz_016` | Luminária de engrenagens | Raro | Crafting: Bancada | — | 6 | Oficina do Ícaro | Superfície |
| `casa_luz_017` | Poste de luz de quintal | Comum | Loja: Lamparinas da Benedita | 21 | 2 | — | Quintal |
| `casa_luz_018` | Tocha de parede de ferro | Comum | Loja: Forja do Mestre Tonho | 23 | 2 | Pico Nevado | Parede |
| `casa_luz_019` | Arandela de vidro âmbar | Incomum | Loja: Lamparinas da Benedita | 85 | 3 | — | Parede |
| `casa_luz_020` | Luz noturna de lua minguante | Incomum | Loja: Lamparinas da Benedita | 80 | 4 | Noite Rubra da Lavine | Parede |
| `casa_luz_021` | Lamparina de azeite do oratório | Comum | Loja: Lamparinas da Benedita | 8 | 1 | Luz da Maria | Superfície |
| `casa_luz_022` | Lustre de cristal lapidado | Épico | Loja: Lamparinas da Benedita | 315 | 9 | Cristal das Cavernas | Teto |
| `casa_luz_023` | Fogo-fátuo engarrafado | Épico | Drop: Habitante Ancestral (chefe) | — | 9 | Noite Rubra da Lavine | Superfície |
| `casa_luz_024` | Lâmpada de pedra-sol | Épico | Conquista: A Luz Volta para Casa | — | 9 | Cristal do Sol | Superfície |
| `casa_luz_025` | Lanterna do Guardião da Ruína | Épico | Drop: Guardião da Ruína (chefe) | — | 8 | Duna Dourada | Superfície |
| `casa_luz_026` | Lampião de mina do Mestre Tonho | Raro | Missão: A Passagem da Mina | — | 5 | Pico Nevado | Superfície |
| `casa_luz_027` | Holofote de dínamo a manivela | Raro | Crafting: Bancada | — | 5 | Oficina do Ícaro | Piso |
| `casa_luz_028` | Vaso de luz de cogumelo | Incomum | Drop: Enxame de Morcegos | — | 4 | Cristal das Cavernas | Superfície |
| `casa_luz_029` | Candeeiro de vidro verde de botica | Incomum | Loja: Armarinho da Dona Cida | 45 | 3 | — | Superfície |
| `casa_luz_030` | Lanterninha giratória de estrelas | Raro | Loja: Lamparinas da Benedita | 240 | 5 | Noite Rubra da Lavine | Superfície |

<a id="parede"></a>
## Paredes e quadros (38)

| ID | Nome | Raridade | Origem | Preço (ouro) | Conforto | Conjunto | Encaixe |
|---|---|---|---|---|---|---|---|
| `casa_parede_001` | Relógio cuco de madeira | Raro | Loja: Mascate Seu Bento | 245 | 6 | — | Parede |
| `casa_parede_002` | Xilogravura do sertão em flor | Incomum | Loja: Armarinho da Dona Cida | 75 | 3 | Cordel e Xilogravura | Parede |
| `casa_parede_003` | Xilogravura da asa-branca | Incomum | Loja: Armarinho da Dona Cida | 60 | 4 | Cordel e Xilogravura | Parede |
| `casa_parede_004` | Xilogravura do Lobo Alfa liberto | Raro | Drop: Lobo Alfa (chefe) | — | 6 | Cordel e Xilogravura | Parede |
| `casa_parede_005` | Folhetos de cordel no barbante | Comum | Loja: Armarinho da Dona Cida | 15 | 1 | Cordel e Xilogravura | Parede |
| `casa_parede_006` | Calendário de parede da mercearia | Comum | Loja: Mercearia do Seu Lázaro | 26 | 2 | — | Parede |
| `casa_parede_007` | Espelho de moldura de sisal | Comum | Loja: Feira livre | 21 | 1 | Duna Dourada | Parede |
| `casa_parede_008` | Bordado emoldurado "Lar doce lar" | Comum | Loja: Armarinho da Dona Cida | 16 | 1 | Varanda da Vó Zefa | Parede |
| `casa_parede_009` | Prato de parede pintado à mão | Comum | Loja: Olaria da Dona Rosa | 15 | 1 | — | Parede |
| `casa_parede_010` | Carranca de proa do rio | Raro | Loja: Mascate Seu Bento | 120 | 5 | — | Parede |
| `casa_parede_011` | Mapa da ilha desenhado pela Maithe | Raro | Conquista: Desbravador | — | 5 | Ventania da Maithe | Parede |
| `casa_parede_012` | Quadro de engrenagens de latão | Incomum | Crafting: Bancada | — | 4 | Oficina do Ícaro | Parede |
| `casa_parede_013` | Planta-baixa de invenção maluca | Incomum | Loja: Mascate Seu Bento | 40 | 4 | Oficina do Ícaro | Parede |
| `casa_parede_014` | Prateleira de pratos de ágata | Comum | Loja: Mercearia do Seu Lázaro | 17 | 1 | Cozinha da Vó | Parede |
| `casa_parede_015` | Chapéu de palha no prego | Comum | Loja: Costureira Dona Fátima | 10 | 2 | — | Parede |
| `casa_parede_016` | Desenhos dos primos em giz de cera | Comum | Diária: Noite de Fogueira | — | 2 | Varanda da Vó Zefa | Parede |
| `casa_parede_017` | Desenho da Lavine com cinco bonecos de palito | Raro | Diária: A Lavine e os Morcegos | — | 6 | Noite Rubra da Lavine | Parede |
| `casa_parede_018` | Tapeçaria de estrelas da Noite Rubra | Épico | Missão pessoal da Lavine (`quest_lavine_redtide`) | — | 9 | Noite Rubra da Lavine | Parede |
| `casa_parede_019` | Arco e aljava de parede da Maithe | Raro | Missão pessoal da Maithe (`quest_maithe_windtrail`) | — | 6 | Ventania da Maithe | Parede |
| `casa_parede_020` | Escudo de madeira decorativo do Joaquim | Raro | Missão pessoal do Joaquim (`quest_joaquim_echoes_of_home`) | — | 6 | Raízes do Joaquim | Parede |
| `casa_parede_021` | Vitral de luz dourada da Maria | Épico | Missão pessoal da Maria (`quest_maria_fading_light`) | — | 9 | Luz da Maria | Parede |
| `casa_parede_022` | Quadro de mosaico de azulejo | Incomum | Loja: Olaria da Dona Rosa | 85 | 3 | — | Parede |
| `casa_parede_023` | Azulejo azul pintado à mão | Comum | Loja: Olaria da Dona Rosa | 28 | 2 | — | Parede |
| `casa_parede_024` | Painel de fitinhas coloridas | Comum | Loja: Armarinho da Dona Cida | 24 | 1 | — | Parede |
| `casa_parede_025` | Mandala de palha de buriti | Incomum | Loja: Feira livre | 75 | 3 | — | Parede |
| `casa_parede_026` | Quadro de borboletas de papel | Comum | Loja: Armarinho da Dona Cida | 9 | 1 | Mata Fechada | Parede |
| `casa_parede_027` | Pôster do Cristal do Sol | Comum | Loja: Armazém Geral | 22 | 2 | Cristal do Sol | Parede |
| `casa_parede_028` | Flâmula do time de futebol da vila | Comum | Loja: Armazém Geral | 22 | 1 | — | Parede |
| `casa_parede_029` | Placa entalhada com o nome do dono | Comum | Loja: Marcenaria do Mestre Damião | 22 | 1 | Varanda da Vó Zefa | Parede |
| `casa_parede_030` | Estante de parede de caixotes | Comum | Loja: Marcenaria do Mestre Damião | 27 | 2 | — | Parede |
| `casa_parede_031` | Relógio de parede de padaria | Incomum | Loja: Mercearia do Seu Lázaro | 85 | 3 | — | Parede |
| `casa_parede_032` | Filtro dos sonhos de sisal | Comum | Loja: Feira livre | 11 | 2 | — | Parede |
| `casa_parede_033` | Quadro do galo-de-campina | Incomum | Loja: Armarinho da Dona Cida | 90 | 4 | — | Parede |
| `casa_parede_034` | Lousa de giz com a receita do dia | Comum | Loja: Mercearia do Seu Lázaro | 22 | 2 | Cozinha da Vó | Parede |
| `casa_parede_035` | Ponto-cruz do Caramelo | Incomum | Loja: Armarinho da Dona Cida | 65 | 3 | Cantinho do Caramelo | Parede |
| `casa_parede_036` | Placa "Primeira aventura" | Comum | Conquista: Primeiro Sangue | — | 2 | — | Parede |
| `casa_parede_037` | Medalha de lata do Caçador do Sertão | Incomum | Conquista: Caçador do Sertão | — | 3 | — | Parede |
| `casa_parede_038` | Pergaminho do Dom Aprimorado | Raro | Conquista: Dom Aprimorado | — | 6 | — | Parede |

<a id="tapete"></a>
## Tapetes (23)

| ID | Nome | Raridade | Origem | Preço (ouro) | Conforto | Conjunto | Encaixe |
|---|---|---|---|---|---|---|---|
| `casa_tapete_001` | Tapete de retalhos redondo | Comum | Loja: Costureira Dona Fátima | 14 | 2 | Varanda da Vó Zefa | Piso |
| `casa_tapete_002` | Tapete de sisal trançado | Comum | Loja: Feira livre | 15 | 2 | Duna Dourada | Piso |
| `casa_tapete_003` | Tapete de crochê de barbante cru | Comum | Loja: Armarinho da Dona Cida | 25 | 1 | — | Piso |
| `casa_tapete_004` | Tapete de vaqueiro de couro cru | Incomum | Loja: Mascate Seu Bento | 90 | 4 | Tropeiro do Vô Chico | Piso |
| `casa_tapete_005` | Esteira de palha de carnaúba | Comum | Loja: Feira livre | 12 | 1 | — | Piso |
| `casa_tapete_006` | Tapete de chita florida | Comum | Loja: Costureira Dona Fátima | 11 | 1 | Feira Livre | Piso |
| `casa_tapete_007` | Tapete arraiolo bordado | Raro | Loja: Costureira Dona Fátima | 185 | 5 | — | Piso |
| `casa_tapete_008` | Passadeira de juta | Comum | Loja: Feira livre | 24 | 2 | — | Piso |
| `casa_tapete_009` | Tapete de fuxico colorido | Incomum | Loja: Armarinho da Dona Cida | 50 | 4 | — | Piso |
| `casa_tapete_010` | Tapete de pelúcia de nuvem | Incomum | Loja: Costureira Dona Fátima | 50 | 4 | Luz da Maria | Piso |
| `casa_tapete_011` | Tapete de folhas da mata | Incomum | Loja: Costureira Dona Fátima | 70 | 4 | Mata Fechada | Piso |
| `casa_tapete_012` | Tapete de dunas ondulado | Incomum | Loja: Costureira Dona Fátima | 45 | 3 | Duna Dourada | Piso |
| `casa_tapete_013` | Tapete de lã de ovelha serrana | Raro | Drop: Golem de Pedra | — | 6 | Pico Nevado | Piso |
| `casa_tapete_014` | Tapete de musgo de caverna | Raro | Drop: Enxame de Morcegos | — | 5 | Cristal das Cavernas | Piso |
| `casa_tapete_015` | Tapete de estrelas da meia-noite | Raro | Loja: Costureira Dona Fátima | 150 | 6 | Noite Rubra da Lavine | Piso |
| `casa_tapete_016` | Tapete de amarelinha pintado | Comum | Loja: Armarinho da Dona Cida | 13 | 1 | — | Piso |
| `casa_tapete_017` | Piso de ladrilho hidráulico | Incomum | Loja: Olaria da Dona Rosa | 90 | 4 | — | Piso |
| `casa_tapete_018` | Capacho "Limpe os pés, menino" | Comum | Loja: Armazém Geral | 8 | 1 | Varanda da Vó Zefa | Piso |
| `casa_tapete_019` | Tapete persa da cidade grande | Épico | Loja: Mascate Seu Bento | 350 | 9 | — | Piso |
| `casa_tapete_020` | Tapete de rosa-dos-ventos | Raro | Loja: Costureira Dona Fátima | 130 | 5 | Ventania da Maithe | Piso |
| `casa_tapete_021` | Tapete de raízes entrelaçadas | Raro | Crafting: Bancada | — | 5 | Raízes do Joaquim | Piso |
| `casa_tapete_022` | Tapete de circuito impresso | Incomum | Crafting: Bancada | — | 3 | Oficina do Ícaro | Piso |
| `casa_tapete_023` | Tapete do sol nascente dourado | Épico | Drop: Habitante Ancestral (chefe) | — | 8 | Cristal do Sol | Piso |

<a id="cortina"></a>
## Cortinas (18)

| ID | Nome | Raridade | Origem | Preço (ouro) | Conforto | Conjunto | Encaixe |
|---|---|---|---|---|---|---|---|
| `casa_cortina_001` | Cortina de chita florida | Comum | Loja: Costureira Dona Fátima | 20 | 2 | Feira Livre | Parede |
| `casa_cortina_002` | Cortina de renda de bilro | Raro | Loja: Costureira Dona Fátima | 210 | 5 | Varanda da Vó Zefa | Parede |
| `casa_cortina_003` | Cortina de fuxico | Incomum | Loja: Armarinho da Dona Cida | 40 | 4 | — | Parede |
| `casa_cortina_004` | Cortina de miçangas coloridas | Comum | Loja: Armarinho da Dona Cida | 21 | 2 | — | Parede |
| `casa_cortina_005` | Cortina de palha de bambu | Comum | Loja: Feira livre | 24 | 1 | — | Parede |
| `casa_cortina_006` | Cortina de voal branco | Comum | Loja: Costureira Dona Fátima | 12 | 2 | Luz da Maria | Parede |
| `casa_cortina_007` | Cortina de saco de farinha alvejado | Comum | Loja: Feira livre | 27 | 2 | Casa de Farinha | Parede |
| `casa_cortina_008` | Cortina de tule com estrelas | Incomum | Loja: Costureira Dona Fátima | 35 | 3 | Noite Rubra da Lavine | Parede |
| `casa_cortina_009` | Cortina de linho cru com barrado | Comum | Loja: Costureira Dona Fátima | 23 | 1 | — | Parede |
| `casa_cortina_010` | Cortina de algodão xadrez | Comum | Loja: Costureira Dona Fátima | 27 | 2 | — | Parede |
| `casa_cortina_011` | Cortina estampada de folhas de bananeira | Incomum | Loja: Costureira Dona Fátima | 75 | 3 | Mata Fechada | Parede |
| `casa_cortina_012` | Cortina de tecido cor de areia | Incomum | Loja: Costureira Dona Fátima | 65 | 3 | Duna Dourada | Parede |
| `casa_cortina_013` | Cortina de feltro grosso da serra | Incomum | Loja: Costureira Dona Fátima | 75 | 3 | Pico Nevado | Parede |
| `casa_cortina_014` | Cortina de contas de cristal | Raro | Loja: Lamparinas da Benedita | 110 | 6 | Cristal das Cavernas | Parede |
| `casa_cortina_015` | Cortina de veludo vinho de teatro | Raro | Loja: Mascate Seu Bento | 170 | 6 | — | Parede |
| `casa_cortina_016` | Cortina de fitas do arraial | Incomum | Loja: Armarinho da Dona Cida | 50 | 4 | — | Parede |
| `casa_cortina_017` | Persiana de taquara | Comum | Loja: Marcenaria do Mestre Damião | 25 | 1 | — | Parede |
| `casa_cortina_018` | Cortina blecaute de lona | Comum | Crafting: Bancada | — | 1 | Oficina do Ícaro | Parede |

<a id="planta"></a>
## Plantas (31)

| ID | Nome | Raridade | Origem | Preço (ouro) | Conforto | Conjunto | Encaixe |
|---|---|---|---|---|---|---|---|
| `casa_planta_001` | Samambaia de metro | Comum | Loja: Viveiro da Comadre Neném | 24 | 1 | — | Teto |
| `casa_planta_002` | Espada-de-são-jorge no vaso de lata | Comum | Loja: Viveiro da Comadre Neném | 21 | 2 | — | Piso |
| `casa_planta_003` | Pé de arruda da rezadeira | Comum | Loja: Viveiro da Comadre Neném | 17 | 1 | Luz da Maria | Piso |
| `casa_planta_004` | Vaso de alecrim | Comum | Loja: Viveiro da Comadre Neném | 13 | 1 | Horta Caprichada | Superfície |
| `casa_planta_005` | Mandacaru no vaso de barro | Comum | Loja: Viveiro da Comadre Neném | 19 | 2 | Duna Dourada | Piso |
| `casa_planta_006` | Xique-xique florido | Incomum | Loja: Viveiro da Comadre Neném | 50 | 4 | Duna Dourada | Piso |
| `casa_planta_007` | Coroa-de-frade | Incomum | Drop: Escorpião das Dunas | — | 4 | Duna Dourada | Piso |
| `casa_planta_008` | Palma forrageira | Comum | Loja: Viveiro da Comadre Neném | 16 | 2 | Duna Dourada | Piso |
| `casa_planta_009` | Orquídea da mata | Raro | Drop: Aranha da Floresta | — | 6 | Mata Fechada | Superfície |
| `casa_planta_010` | Bromélia de tronco | Incomum | Loja: Viveiro da Comadre Neném | 55 | 3 | Mata Fechada | Parede |
| `casa_planta_011` | Avenca na meia-lua | Comum | Loja: Viveiro da Comadre Neném | 22 | 2 | — | Parede |
| `casa_planta_012` | Violeta na lata de goiabada | Comum | Loja: Viveiro da Comadre Neném | 27 | 2 | Feira Livre | Superfície |
| `casa_planta_013` | Girassol no balde | Comum | Loja: Viveiro da Comadre Neném | 20 | 1 | — | Piso |
| `casa_planta_014` | Lírio-da-paz | Comum | Loja: Viveiro da Comadre Neném | 25 | 1 | Luz da Maria | Piso |
| `casa_planta_015` | Costela-de-adão | Incomum | Loja: Viveiro da Comadre Neném | 45 | 3 | Mata Fechada | Piso |
| `casa_planta_016` | Bonsai de ipê-amarelo | Raro | Loja: Viveiro da Comadre Neném | 210 | 5 | — | Superfície |
| `casa_planta_017` | Ipê-roxo pequeno | Raro | Loja: Viveiro da Comadre Neném | 235 | 5 | — | Piso |
| `casa_planta_018` | Terrário de musgo de caverna | Incomum | Drop: Enxame de Morcegos | — | 4 | Cristal das Cavernas | Superfície |
| `casa_planta_019` | Cogumelo luminoso de vaso | Raro | Drop: Habitante Ancestral (chefe) | — | 6 | Cristal das Cavernas | Piso |
| `casa_planta_020` | Flor-de-pedra da serra | Incomum | Drop: Golem de Pedra | — | 4 | Pico Nevado | Superfície |
| `casa_planta_021` | Sempre-viva da serra | Raro | Drop: Senhor da Guerra da Montanha (chefe) | — | 5 | Pico Nevado | Superfície |
| `casa_planta_022` | Dioneia de vaso | Raro | Loja: Mascate Seu Bento | 110 | 5 | — | Superfície |
| `casa_planta_023` | Rainha-da-noite em flor | Épico | Loja: Mascate Seu Bento | 600 | 8 | Noite Rubra da Lavine | Piso |
| `casa_planta_024` | Vaso de lavanda | Comum | Loja: Viveiro da Comadre Neném | 12 | 2 | — | Superfície |
| `casa_planta_025` | Babosa no pote | Comum | Loja: Viveiro da Comadre Neném | 27 | 1 | Luz da Maria | Superfície |
| `casa_planta_026` | Bambu-da-sorte | Comum | Loja: Viveiro da Comadre Neném | 23 | 1 | — | Superfície |
| `casa_planta_027` | Jiboia pendente | Comum | Loja: Viveiro da Comadre Neném | 21 | 1 | — | Teto |
| `casa_planta_028` | Pimenteira ornamental | Comum | Loja: Viveiro da Comadre Neném | 23 | 2 | Horta Caprichada | Piso |
| `casa_planta_029` | Flor de maracujá em treliça | Incomum | Loja: Viveiro da Comadre Neném | 85 | 3 | Horta Caprichada | Parede |
| `casa_planta_030` | Vaso de flores de papel crepom | Comum | Loja: Armarinho da Dona Cida | 10 | 2 | — | Superfície |
| `casa_planta_031` | Muda de cana-caiana | Incomum | Loja: Viveiro da Comadre Neném | 80 | 4 | Casa de Farinha | Piso |

<a id="musica"></a>
## Instrumentos musicais (23)

| ID | Nome | Raridade | Origem | Preço (ouro) | Conforto | Conjunto | Encaixe |
|---|---|---|---|---|---|---|---|
| `casa_musica_001` | Sanfona de oito baixos | Raro | Loja: Casa de Música do Seu Bené | 130 | 5 | Arraiá | Superfície |
| `casa_musica_002` | Sanfona vermelha de cento e vinte baixos | Épico | Loja: Casa de Música do Seu Bené | 440 | 9 | — | Piso |
| `casa_musica_003` | Triângulo de ferro | Comum | Loja: Casa de Música do Seu Bené | 19 | 2 | Arraiá | Parede |
| `casa_musica_004` | Zabumba de couro | Incomum | Loja: Casa de Música do Seu Bené | 75 | 3 | Arraiá | Piso |
| `casa_musica_005` | Pandeiro de platinela | Comum | Loja: Casa de Música do Seu Bené | 13 | 2 | — | Parede |
| `casa_musica_006` | Viola caipira de dez cordas | Raro | Loja: Casa de Música do Seu Bené | 135 | 6 | — | Parede |
| `casa_musica_007` | Rabeca de cabaça | Raro | Loja: Casa de Música do Seu Bené | 180 | 6 | — | Parede |
| `casa_musica_008` | Pífano de taquara | Comum | Loja: Casa de Música do Seu Bené | 22 | 2 | — | Parede |
| `casa_musica_009` | Berimbau de verga | Incomum | Loja: Casa de Música do Seu Bené | 75 | 3 | — | Parede |
| `casa_musica_010` | Agogô de lata | Comum | Loja: Casa de Música do Seu Bené | 8 | 1 | Frevo e Maracatu | Superfície |
| `casa_musica_011` | Alfaia de maracatu | Raro | Loja: Casa de Música do Seu Bené | 200 | 6 | Frevo e Maracatu | Piso |
| `casa_musica_012` | Ganzá de lata | Comum | Loja: Casa de Música do Seu Bené | 21 | 1 | — | Superfície |
| `casa_musica_013` | Caxixi de palha | Comum | Loja: Casa de Música do Seu Bené | 8 | 1 | — | Superfície |
| `casa_musica_014` | Cavaquinho de cedro | Incomum | Loja: Casa de Música do Seu Bené | 80 | 4 | — | Parede |
| `casa_musica_015` | Violão de cordas de aço | Incomum | Loja: Casa de Música do Seu Bené | 50 | 4 | — | Parede |
| `casa_musica_016` | Gaita de boca de lata | Comum | Loja: Casa de Música do Seu Bené | 16 | 2 | Tropeiro do Vô Chico | Superfície |
| `casa_musica_017` | Marimba de garrafas | Raro | Crafting: Bancada | — | 6 | Oficina do Ícaro | Piso |
| `casa_musica_018` | Harpa de vento da Maithe | Épico | Conquista: Olho do Vento | — | 9 | Ventania da Maithe | Quintal |
| `casa_musica_019` | Sininhos de vento de bambu | Comum | Loja: Feira livre | 24 | 1 | — | Teto |
| `casa_musica_020` | Tambor de pedra da montanha | Raro | Drop: Senhor da Guerra da Montanha (chefe) | — | 6 | Pico Nevado | Piso |
| `casa_musica_021` | Flauta de cristal que assobia sozinha | Épico | Drop: Habitante Ancestral (chefe) | — | 9 | Cristal das Cavernas | Superfície |
| `casa_musica_022` | Matraca de madeira | Comum | Loja: Casa de Música do Seu Bené | 23 | 2 | — | Superfície |
| `casa_musica_023` | Metrônomo de sintonia | Incomum | Conquista: Em Sintonia | — | 4 | — | Superfície |

<a id="brinquedo"></a>
## Brinquedos (31)

| ID | Nome | Raridade | Origem | Preço (ouro) | Conforto | Conjunto | Encaixe |
|---|---|---|---|---|---|---|---|
| `casa_brinquedo_001` | Pião de madeira pintado | Comum | Loja: Feira livre | 20 | 2 | — | Superfície |
| `casa_brinquedo_002` | Bilboquê de lata | Comum | Loja: Feira livre | 12 | 1 | — | Superfície |
| `casa_brinquedo_003` | Boneca de pano de retalhos | Comum | Loja: Costureira Dona Fátima | 29 | 1 | — | Superfície |
| `casa_brinquedo_004` | Carrinho de lata de sardinha | Comum | Loja: Feira livre | 23 | 2 | — | Piso |
| `casa_brinquedo_005` | Pipa de papel de seda com rabiola | Comum | Loja: Armarinho da Dona Cida | 23 | 2 | Ventania da Maithe | Parede |
| `casa_brinquedo_006` | Peteca de palha de milho | Comum | Loja: Feira livre | 29 | 1 | — | Superfície |
| `casa_brinquedo_007` | Bola de meia remendada | Comum | Loja: Armarinho da Dona Cida | 19 | 1 | — | Piso |
| `casa_brinquedo_008` | Cavalinho de pau | Comum | Loja: Marcenaria do Mestre Damião | 30 | 1 | — | Piso |
| `casa_brinquedo_009` | Cavalo de balanço de madeira | Incomum | Loja: Marcenaria do Mestre Damião | 90 | 4 | — | Piso |
| `casa_brinquedo_010` | Pote de bolinhas de gude | Comum | Loja: Armazém Geral | 17 | 1 | — | Superfície |
| `casa_brinquedo_011` | Jogo de varetas | Comum | Loja: Armazém Geral | 21 | 1 | — | Superfície |
| `casa_brinquedo_012` | Tabuleiro de damas de tampinha | Comum | Loja: Feira livre | 17 | 2 | — | Superfície |
| `casa_brinquedo_013` | Corda de pular com cabo de madeira | Comum | Loja: Feira livre | 28 | 2 | — | Parede |
| `casa_brinquedo_014` | Perna de pau | Comum | Loja: Marcenaria do Mestre Damião | 10 | 2 | — | Parede |
| `casa_brinquedo_015` | Estilingue de forquilha de goiabeira | Comum | Loja: Feira livre | 29 | 1 | — | Parede |
| `casa_brinquedo_016` | Mané-gostoso acrobata | Incomum | Loja: Feira livre | 65 | 3 | — | Superfície |
| `casa_brinquedo_017` | Rói-rói de lata | Comum | Loja: Feira livre | 24 | 1 | — | Superfície |
| `casa_brinquedo_018` | Barquinho de papel na bacia | Comum | Loja: Armarinho da Dona Cida | 22 | 1 | — | Piso |
| `casa_brinquedo_019` | Robô de lata a corda | Raro | Conquista: Engrenagens Antigas | — | 6 | Oficina do Ícaro | Superfície |
| `casa_brinquedo_020` | Trenzinho de madeira com trilho | Incomum | Loja: Marcenaria do Mestre Damião | 75 | 3 | — | Piso |
| `casa_brinquedo_021` | Casinha de bonecas de taipa | Raro | Loja: Marcenaria do Mestre Damião | 125 | 5 | Varanda da Vó Zefa | Piso |
| `casa_brinquedo_022` | Teatro de mamulengo | Épico | Loja: Mascate Seu Bento | 515 | 8 | Cordel e Xilogravura | Piso |
| `casa_brinquedo_023` | Bonequinhos de barro de Caruaru | Incomum | Loja: Olaria da Dona Rosa | 55 | 3 | Cordel e Xilogravura | Superfície |
| `casa_brinquedo_024` | Cata-vento de papel colorido | Comum | Loja: Armarinho da Dona Cida | 11 | 2 | — | Superfície |
| `casa_brinquedo_025` | Quebra-cabeça do mapa da ilha | Incomum | Loja: Armazém Geral | 90 | 4 | — | Superfície |
| `casa_brinquedo_026` | Telefone de lata com barbante | Comum | Loja: Armarinho da Dona Cida | 9 | 1 | Oficina do Ícaro | Parede |
| `casa_brinquedo_027` | Pelúcia de lobinho de olhos sonolentos | Raro | Drop: Lobo da Mata | — | 5 | Mata Fechada | Superfície |
| `casa_brinquedo_028` | Pelúcia de escorpião sorridente | Raro | Drop: Escorpião das Dunas | — | 6 | Duna Dourada | Superfície |
| `casa_brinquedo_029` | Pelúcia de morceguinho de cabeça pra baixo | Raro | Drop: Enxame de Morcegos | — | 5 | Cristal das Cavernas | Teto |
| `casa_brinquedo_030` | Golenzinho de pedra que fica de pé | Raro | Drop: Golem de Pedra | — | 5 | Pico Nevado | Superfície |
| `casa_brinquedo_031` | Aranhinha de crochê | Incomum | Drop: Aranha da Floresta | — | 3 | Mata Fechada | Superfície |

<a id="regional"></a>
## Decoração regional por bioma (48)

| ID | Nome | Raridade | Origem | Preço (ouro) | Conforto | Conjunto | Encaixe |
|---|---|---|---|---|---|---|---|
| `casa_regional_001` | Tronco oco de jequitibá | Raro | Drop: Lobo da Mata | — | 5 | Mata Fechada | Piso |
| `casa_regional_002` | Totem de cipó entrelaçado | Incomum | Drop: Aranha da Floresta | — | 3 | Mata Fechada | Piso |
| `casa_regional_003` | Banco de raiz de gameleira | Incomum | Loja: Marcenaria do Mestre Damião | 90 | 4 | Mata Fechada | Piso |
| `casa_regional_004` | Ninho de passarinho na forquilha | Comum | Drop: Lobo da Mata | — | 2 | Mata Fechada | Superfície |
| `casa_regional_005` | Tora com orelha-de-pau | Comum | Drop: Aranha da Floresta | — | 2 | Mata Fechada | Piso |
| `casa_regional_006` | Teia de orvalho emoldurada | Raro | Drop: Aranha da Floresta | — | 5 | Mata Fechada | Parede |
| `casa_regional_007` | Pegada de lobo no gesso | Incomum | Drop: Lobo da Mata | — | 3 | Mata Fechada | Parede |
| `casa_regional_008` | Casinha de árvore em miniatura | Raro | Loja: Marcenaria do Mestre Damião | 210 | 5 | Mata Fechada | Piso |
| `casa_regional_009` | Ramo de cajá com frutas | Comum | Loja: Feira livre | 25 | 1 | Mata Fechada | Superfície |
| `casa_regional_010` | Pote de vagalumes do riacho | Incomum | Drop: Lobo da Mata | — | 4 | Mata Fechada | Superfície |
| `casa_regional_011` | Placa da trilha da Floresta | Comum | Loja: Armazém Geral | 18 | 2 | Mata Fechada | Quintal |
| `casa_regional_012` | Pedra de musgo da Floresta | Comum | Drop: Lobo da Mata | — | 1 | Mata Fechada | Quintal |
| `casa_regional_013` | Pote de areia colorida em camadas | Comum | Loja: Feira livre | 9 | 2 | Duna Dourada | Superfície |
| `casa_regional_014` | Escultura de mandacaru seco | Incomum | Drop: Escorpião das Dunas | — | 4 | Duna Dourada | Piso |
| `casa_regional_015` | Coluna tombada da ruína | Raro | Drop: Guardião da Ruína (chefe) | — | 6 | Duna Dourada | Quintal |
| `casa_regional_016` | Painel de engrenagens da ruína | Épico | Drop: Guardião da Ruína (chefe) | — | 9 | Duna Dourada | Parede |
| `casa_regional_017` | Rosa-do-deserto de areia cristalizada | Raro | Drop: Escorpião das Dunas | — | 5 | Duna Dourada | Superfície |
| `casa_regional_018` | Cantil de couro do tropeiro | Incomum | Missão: Reserva pra Travessia | — | 4 | Tropeiro do Vô Chico | Parede |
| `casa_regional_019` | Jarro de barro rachado das dunas | Comum | Drop: Escorpião das Dunas | — | 2 | Duna Dourada | Piso |
| `casa_regional_020` | Tenda de lona de caravana | Raro | Loja: Mascate Seu Bento | 145 | 6 | Duna Dourada | Quintal |
| `casa_regional_021` | Ampulheta de areia dourada | Incomum | Loja: Mascate Seu Bento | 35 | 3 | Duna Dourada | Superfície |
| `casa_regional_022` | Placa "Cuidado com os ferrões" | Comum | Missão: Ferrões no Caminho | — | 2 | Duna Dourada | Quintal |
| `casa_regional_023` | Cacto em forma de coração | Incomum | Loja: Viveiro da Comadre Neném | 45 | 3 | Duna Dourada | Superfície |
| `casa_regional_024` | Pedra lascada com desenho rupestre | Raro | Drop: Guardião da Ruína (chefe) | — | 6 | Duna Dourada | Piso |
| `casa_regional_025` | Picaretas cruzadas do mineiro | Incomum | Missão: Pedras que Andam | — | 4 | Pico Nevado | Parede |
| `casa_regional_026` | Vagonete de mina com cristais | Raro | Drop: Golem de Pedra | — | 5 | Pico Nevado | Piso |
| `casa_regional_027` | Boneco de neve de pedra | Comum | Drop: Golem de Pedra | — | 2 | Pico Nevado | Quintal |
| `casa_regional_028` | Bandeirola do acampamento do pico | Comum | Loja: Armazém Geral | 26 | 1 | Pico Nevado | Parede |
| `casa_regional_029` | Pedrinha que anda, sonolenta | Épico | Drop: Golem de Pedra | — | 8 | Pico Nevado | Piso |
| `casa_regional_030` | Estandarte desbotado do Senhor da Guerra | Raro | Drop: Senhor da Guerra da Montanha (chefe) | — | 6 | Pico Nevado | Parede |
| `casa_regional_031` | Tora de lareira com neve | Comum | Loja: Feira livre | 12 | 1 | Pico Nevado | Piso |
| `casa_regional_032` | Corda de escalada enrolada | Comum | Loja: Forja do Mestre Tonho | 30 | 1 | Pico Nevado | Parede |
| `casa_regional_033` | Barraca de lona do pico | Incomum | Loja: Mascate Seu Bento | 75 | 3 | Pico Nevado | Quintal |
| `casa_regional_034` | Mapa dos veios de minério | Incomum | Loja: Forja do Mestre Tonho | 85 | 3 | Pico Nevado | Parede |
| `casa_regional_035` | Geodo de ametista aberto | Raro | Drop: Golem de Pedra | — | 6 | Pico Nevado | Superfície |
| `casa_regional_036` | Sino de pedra da serra | Raro | Drop: Senhor da Guerra da Montanha (chefe) | — | 6 | Pico Nevado | Teto |
| `casa_regional_037` | Estalactite de cristal suspensa | Raro | Drop: Enxame de Morcegos | — | 5 | Cristal das Cavernas | Teto |
| `casa_regional_038` | Estalagmite de jardim | Comum | Drop: Enxame de Morcegos | — | 2 | Cristal das Cavernas | Quintal |
| `casa_regional_039` | Lago subterrâneo na bacia | Incomum | Drop: Enxame de Morcegos | — | 4 | Cristal das Cavernas | Piso |
| `casa_regional_040` | Lamparina apagada reacesa | Incomum | Missão: Lamparinas Apagadas | — | 4 | Cristal das Cavernas | Superfície |
| `casa_regional_041` | Concha fóssil de quando ali era mar | Raro | Drop: Habitante Ancestral (chefe) | — | 5 | Cristal das Cavernas | Superfície |
| `casa_regional_042` | Casinha de morcego de madeira | Comum | Loja: Lamparinas da Benedita | 28 | 1 | Cristal das Cavernas | Quintal |
| `casa_regional_043` | Bolota de luz devolvida | Épico | Drop: Habitante Ancestral (chefe) | — | 9 | Cristal das Cavernas | Superfície |
| `casa_regional_044` | Pintura rupestre de mãozinhas | Incomum | Drop: Habitante Ancestral (chefe) | — | 4 | Cristal das Cavernas | Parede |
| `casa_regional_045` | Cristal cantante | Raro | Drop: Enxame de Morcegos | — | 5 | Cristal das Cavernas | Superfície |
| `casa_regional_046` | Pedra fria de sombra apaziguada | Raro | Drop: Habitante Ancestral (chefe) | — | 6 | Noite Rubra da Lavine | Superfície |
| `casa_regional_047` | Mapa dos túneis da Benedita | Incomum | Loja: Lamparinas da Benedita | 35 | 3 | Cristal das Cavernas | Parede |
| `casa_regional_048` | Raiz que brilha do teto da caverna | Incomum | Drop: Enxame de Morcegos | — | 4 | Cristal das Cavernas | Teto |

<a id="trofeu"></a>
## Troféus de chefes (24)

| ID | Nome | Raridade | Origem | Preço (ouro) | Conforto | Conjunto | Encaixe |
|---|---|---|---|---|---|---|---|
| `casa_trofeu_001` | Placa "O Lobo Alfa voltou pra mata" | Incomum | Drop: Lobo Alfa (chefe) | — | 3 | — | Parede |
| `casa_trofeu_002` | Coleira de couro do Lobo Alfa | Raro | Drop: Lobo Alfa (chefe) | — | 6 | — | Parede |
| `casa_trofeu_003` | Concha que guarda o uivo do Lobo Alfa | Raro | Drop: Lobo Alfa (chefe) | — | 6 | — | Superfície |
| `casa_trofeu_004` | Pedrinha escura desfeita da Floresta | Épico | Drop: Lobo Alfa (chefe) | — | 8 | — | Superfície |
| `casa_trofeu_005` | Cabeça entalhada do Lobo Alfa | Épico | Drop: Lobo Alfa (chefe) | — | 9 | — | Parede |
| `casa_trofeu_006` | Estátua do Lobo Alfa dormindo | Lendário | Drop: Lobo Alfa (chefe) | — | 14 | — | Piso |
| `casa_trofeu_007` | Placa "O Guardião descansa" | Incomum | Drop: Guardião da Ruína (chefe) | — | 3 | — | Parede |
| `casa_trofeu_008` | Elmo rachado do Guardião da Ruína | Raro | Drop: Guardião da Ruína (chefe) | — | 5 | — | Parede |
| `casa_trofeu_009` | Frasco de areia dourada do rastro | Raro | Drop: Guardião da Ruína (chefe) | — | 6 | — | Superfície |
| `casa_trofeu_010` | Engrenagem-coração do Guardião | Épico | Drop: Guardião da Ruína (chefe) | — | 8 | — | Superfície |
| `casa_trofeu_011` | Cabeça entalhada do Guardião da Ruína | Épico | Drop: Guardião da Ruína (chefe) | — | 9 | — | Parede |
| `casa_trofeu_012` | Miniatura do Guardião de vigia | Lendário | Drop: Guardião da Ruína (chefe) | — | 12 | — | Piso |
| `casa_trofeu_013` | Placa "Obrigado, crianças" | Incomum | Drop: Senhor da Guerra da Montanha (chefe) | — | 4 | — | Parede |
| `casa_trofeu_014` | Capa remendada do Senhor da Guerra | Raro | Drop: Senhor da Guerra da Montanha (chefe) | — | 6 | — | Parede |
| `casa_trofeu_015` | Elmo com pena da serra | Raro | Drop: Senhor da Guerra da Montanha (chefe) | — | 5 | — | Parede |
| `casa_trofeu_016` | Espada embainhada do Senhor da Guerra | Épico | Drop: Senhor da Guerra da Montanha (chefe) | — | 8 | — | Parede |
| `casa_trofeu_017` | Cabeça entalhada do Senhor da Guerra | Épico | Drop: Senhor da Guerra da Montanha (chefe) | — | 9 | — | Parede |
| `casa_trofeu_018` | Estátua do Senhor da Guerra ajoelhado em paz | Lendário | Drop: Senhor da Guerra da Montanha (chefe) | — | 12 | — | Piso |
| `casa_trofeu_019` | Placa "Dá pra dividir" | Incomum | Drop: Habitante Ancestral (chefe) | — | 4 | — | Parede |
| `casa_trofeu_020` | Manta de sombra macia | Raro | Drop: Habitante Ancestral (chefe) | — | 5 | Noite Rubra da Lavine | Superfície |
| `casa_trofeu_021` | Lanterna que o Habitante aprendeu a dividir | Raro | Drop: Habitante Ancestral (chefe) | — | 6 | — | Superfície |
| `casa_trofeu_022` | Lasquinha do Cristal ainda morna | Épico | Drop: Habitante Ancestral (chefe) | — | 8 | Cristal do Sol | Superfície |
| `casa_trofeu_023` | Cabeça entalhada do Habitante Ancestral | Épico | Drop: Habitante Ancestral (chefe) | — | 9 | — | Parede |
| `casa_trofeu_024` | Estátua do Habitante abraçando a luz | Lendário | Drop: Habitante Ancestral (chefe) | — | 15 | Cristal do Sol | Piso |

<a id="sazonal"></a>
## Itens sazonais (45)

| ID | Nome | Raridade | Origem | Preço (ouro) | Conforto | Conjunto | Encaixe |
|---|---|---|---|---|---|---|---|
| `casa_sazonal_001` | Bandeirinhas de papel de seda | Comum | Evento: Festa Junina | — (7 prendas) | 1 | Arraiá | Teto |
| `casa_sazonal_002` | Fogueira de São João cenográfica | Incomum | Evento: Festa Junina | — (23 prendas) | 3 | Arraiá | Quintal |
| `casa_sazonal_003` | Balão de papel enfeitado, sem fogo | Comum | Evento: Festa Junina | — (9 prendas) | 1 | Arraiá | Teto |
| `casa_sazonal_004` | Barraca de pescaria | Raro | Evento: Festa Junina | — (50 prendas) | 5 | Arraiá | Quintal |
| `casa_sazonal_005` | Barraca de canjica | Incomum | Evento: Festa Junina | — (25 prendas) | 4 | Arraiá | Quintal |
| `casa_sazonal_006` | Barraca de pamonha | Incomum | Evento: Festa Junina | — (21 prendas) | 3 | Arraiá | Quintal |
| `casa_sazonal_007` | Mastro de São João com fitas | Raro | Evento: Festa Junina | — (38 prendas) | 6 | Arraiá | Quintal |
| `casa_sazonal_008` | Casal de espantalhos caipiras | Incomum | Evento: Festa Junina | — (15 prendas) | 3 | Arraiá | Quintal |
| `casa_sazonal_009` | Caixinha do correio elegante | Comum | Evento: Festa Junina | — (10 prendas) | 2 | Arraiá | Superfície |
| `casa_sazonal_010` | Chapéu de palha remendado do arraiá | Comum | Evento: Festa Junina | — (5 prendas) | 1 | Arraiá | Parede |
| `casa_sazonal_011` | Mesa de comidas típicas | Raro | Evento: Festa Junina | — (32 prendas) | 6 | Arraiá | Piso |
| `casa_sazonal_012` | Travessa de pé-de-moleque | Comum | Evento: Festa Junina | — (6 prendas) | 2 | Arraiá | Superfície |
| `casa_sazonal_013` | Milho assado na brasa | Comum | Evento: Festa Junina | — (10 prendas) | 2 | Arraiá | Superfície |
| `casa_sazonal_014` | Arco de bambu com balõezinhos | Incomum | Evento: Festa Junina | — (24 prendas) | 3 | Arraiá | Quintal |
| `casa_sazonal_015` | Palco da quadrilha | Épico | Evento: Festa Junina | — (86 prendas) | 8 | Arraiá | Quintal |
| `casa_sazonal_016` | Boi-bumbá de pano | Épico | Evento: Festa Junina | — (85 prendas) | 9 | Arraiá | Piso |
| `casa_sazonal_017` | Rabeca enfeitada de fitas | Raro | Evento: Festa Junina | — (44 prendas) | 6 | Arraiá | Parede |
| `casa_sazonal_018` | Cadeia de mentirinha do arraiá | Incomum | Evento: Festa Junina | — (24 prendas) | 4 | Arraiá | Quintal |
| `casa_sazonal_019` | Presépio de barro | Raro | Evento: Natal Sertanejo | — (43 prendas) | 6 | Natal Sertanejo | Piso |
| `casa_sazonal_020` | Árvore de Natal de mandacaru | Incomum | Evento: Natal Sertanejo | — (19 prendas) | 4 | Natal Sertanejo | Piso |
| `casa_sazonal_021` | Estrela-guia de palha | Comum | Evento: Natal Sertanejo | — (9 prendas) | 1 | Natal Sertanejo | Teto |
| `casa_sazonal_022` | Guirlanda de folhas de cajueiro | Comum | Evento: Natal Sertanejo | — (5 prendas) | 1 | Natal Sertanejo | Parede |
| `casa_sazonal_023` | Papai Noel de chapéu de couro | Raro | Evento: Natal Sertanejo | — (38 prendas) | 6 | Natal Sertanejo | Piso |
| `casa_sazonal_024` | Meia de Natal de chita | Comum | Evento: Natal Sertanejo | — (6 prendas) | 2 | Natal Sertanejo | Parede |
| `casa_sazonal_025` | Pisca-pisca de lamparinas | Incomum | Evento: Natal Sertanejo | — (24 prendas) | 3 | Natal Sertanejo | Teto |
| `casa_sazonal_026` | Mesa da ceia com rabanada | Raro | Evento: Natal Sertanejo | — (31 prendas) | 5 | Natal Sertanejo | Piso |
| `casa_sazonal_027` | Estandarte do pastoril azul e encarnado | Incomum | Evento: Natal Sertanejo | — (15 prendas) | 3 | Natal Sertanejo | Parede |
| `casa_sazonal_028` | Sino de Natal de lata | Comum | Evento: Natal Sertanejo | — (9 prendas) | 1 | Natal Sertanejo | Teto |
| `casa_sazonal_029` | Carroça do Noel puxada por jumento | Épico | Evento: Natal Sertanejo | — (86 prendas) | 9 | Natal Sertanejo | Quintal |
| `casa_sazonal_030` | Neve de algodão no peitoril | Comum | Evento: Natal Sertanejo | — (10 prendas) | 2 | Natal Sertanejo | Superfície |
| `casa_sazonal_031` | Bonecos do reisado | Raro | Evento: Natal Sertanejo | — (41 prendas) | 5 | Natal Sertanejo | Piso |
| `casa_sazonal_032` | Panetone de rapadura | Comum | Evento: Natal Sertanejo | — (6 prendas) | 2 | Natal Sertanejo | Superfície |
| `casa_sazonal_033` | Sombrinha de frevo | Comum | Evento: Carnaval | — (6 prendas) | 2 | Frevo e Maracatu | Parede |
| `casa_sazonal_034` | Boneco gigante de Olinda | Épico | Evento: Carnaval | — (83 prendas) | 8 | Frevo e Maracatu | Piso |
| `casa_sazonal_035` | Estandarte de maracatu | Raro | Evento: Carnaval | — (36 prendas) | 5 | Frevo e Maracatu | Parede |
| `casa_sazonal_036` | Máscara de papangu | Incomum | Evento: Carnaval | — (18 prendas) | 3 | Frevo e Maracatu | Parede |
| `casa_sazonal_037` | Serpentinas e confetes no lustre | Comum | Evento: Carnaval | — (7 prendas) | 1 | Frevo e Maracatu | Teto |
| `casa_sazonal_038` | Coroa da rainha do maracatu | Raro | Evento: Carnaval | — (35 prendas) | 6 | Frevo e Maracatu | Superfície |
| `casa_sazonal_039` | Fantasia de pirata no cabide | Comum | Evento: Carnaval | — (9 prendas) | 1 | Frevo e Maracatu | Piso |
| `casa_sazonal_040` | Calunga de maracatu | Incomum | Evento: Carnaval | — (16 prendas) | 4 | Frevo e Maracatu | Superfície |
| `casa_sazonal_041` | Trio elétrico em miniatura | Raro | Evento: Carnaval | — (44 prendas) | 5 | Frevo e Maracatu | Superfície |
| `casa_sazonal_042` | Lança enfitada de caboclo | Incomum | Evento: Carnaval | — (25 prendas) | 4 | Frevo e Maracatu | Parede |
| `casa_sazonal_043` | Bandeira do bloco "Primos na Folia" | Comum | Evento: Carnaval | — (7 prendas) | 1 | Frevo e Maracatu | Parede |
| `casa_sazonal_044` | Sombrinhas penduradas no teto | Incomum | Evento: Carnaval | — (18 prendas) | 3 | Frevo e Maracatu | Teto |
| `casa_sazonal_045` | Chapéu de caboclo de lança | Raro | Evento: Carnaval | — (40 prendas) | 6 | Frevo e Maracatu | Parede |

<a id="familia"></a>
## Raros e lendários com história de família (27)

| ID | Nome | Raridade | Origem | Preço (ouro) | Conforto | Conjunto | Encaixe |
|---|---|---|---|---|---|---|---|
| `casa_familia_001` | Retrato de casamento da Vó Zefa e do Vô Chico | Lendário | Missão da Casa: Casa Cheia | — | 14 | Varanda da Vó Zefa | Parede |
| `casa_familia_002` | Rede onde a Vó Zefa embalou os cinco netos | Lendário | Missão da Casa: Tijolo por Tijolo | — | 12 | Varanda da Vó Zefa | Parede |
| `casa_familia_003` | Baú de lata do tropeiro Vô Chico | Lendário | Segredo do mapa (casa de farinha do Vô Chico) | — | 13 | Tropeiro do Vô Chico | Piso |
| `casa_familia_004` | Pote de rapadura que nunca esvazia | Lendário | Conquista: Acender de Novo | — | 12 | Cozinha da Vó | Superfície |
| `casa_familia_005` | Livro de receitas escrito à mão da Vó Zefa | Épico | Diária: Um Prato Quente | — | 9 | Cozinha da Vó | Superfície |
| `casa_familia_006` | Caixa de costura de madrepérola da Vó Zefa | Épico | Diária: A Cesta de Ervas da Maria | — | 9 | Varanda da Vó Zefa | Superfície |
| `casa_familia_007` | Medalhão de luz da Maria em redoma | Épico | Conquista: Luz que Não se Apaga | — | 9 | Luz da Maria | Superfície |
| `casa_familia_008` | Raiz-mãe do Escudo das Raízes | Épico | Conquista: Ecos de Casa | — | 8 | Raízes do Joaquim | Piso |
| `casa_familia_009` | Fita vermelha da coragem emoldurada | Épico | Conquista: Coragem da Caçula | — | 8 | Noite Rubra da Lavine | Parede |
| `casa_familia_010` | Drone pintado com as cores do arco | Épico | Conquista: Vento e Engrenagem | — | 9 | Oficina do Ícaro | Teto |
| `casa_familia_011` | Escudo dourado de luz e raiz | Épico | Conquista: Escudo e Luz | — | 8 | Raízes do Joaquim | Parede |
| `casa_familia_012` | Sela de prata da mula Teimosa | Lendário | Drop: Guardião da Ruína (chefe) | — | 12 | Tropeiro do Vô Chico | Piso |
| `casa_familia_013` | Carta de amor do Vô Chico emoldurada | Épico | Segredo do mapa (casa de farinha do Vô Chico) | — | 9 | Tropeiro do Vô Chico | Parede |
| `casa_familia_014` | Relógio de bolso do Vô Chico parado às seis | Lendário | Drop: Habitante Ancestral (chefe) | — | 15 | Tropeiro do Vô Chico | Superfície |
| `casa_familia_015` | Moinho de vento em miniatura do bisavô | Épico | Loja: Mascate Seu Bento | 350 | 9 | Casa de Farinha | Superfície |
| `casa_familia_016` | Primeira rapadura da casa de farinha, petrificada | Lendário | Segredo do mapa (casa de farinha do Vô Chico) | — | 13 | Casa de Farinha | Superfície |
| `casa_familia_017` | Cristal do Sol em miniatura | Lendário | Drop: Habitante Ancestral (chefe) | — | 14 | Cristal do Sol | Superfície |
| `casa_familia_018` | Mantinha de batizado da Lavine bordada de lua | Épico | Missão pessoal da Lavine (`quest_lavine_redtide`) | — | 8 | Noite Rubra da Lavine | Superfície |
| `casa_familia_019` | Caderno de invenções do Ícaro | Épico | Missão pessoal do Ícaro (`quest_icaro_ancient_gears`) | — | 8 | Oficina do Ícaro | Superfície |
| `casa_familia_020` | Mochila remendada da Maithe | Épico | Missão: A Reunião (`quest_convergence_the_gathering`) | — | 9 | Ventania da Maithe | Parede |
| `casa_familia_021` | Bolo de milho eterno de cerâmica | Raro | Loja: Olaria da Dona Rosa | 165 | 5 | Varanda da Vó Zefa | Superfície |
| `casa_familia_022` | Panela de barro centenária da família | Épico | Loja: Mascate Seu Bento | 410 | 9 | Cozinha da Vó | Superfície |
| `casa_familia_023` | Chapéu de couro do bisavô vaqueiro | Épico | Loja: Mascate Seu Bento | 550 | 9 | Tropeiro do Vô Chico | Parede |
| `casa_familia_024` | Oratório da bisavó com santinho de madeira | Lendário | Evento: Natal Sertanejo | — | 12 | Luz da Maria | Parede |
| `casa_familia_025` | Bandeira do primeiro São João do sítio | Lendário | Evento: Festa Junina | — | 14 | Arraiá | Parede |
| `casa_familia_026` | Máscara de frevo da juventude da Vó Zefa | Lendário | Evento: Carnaval | — | 15 | Frevo e Maracatu | Parede |
| `casa_familia_027` | Primeiro arco de bambu da Maithe | Épico | Missão pessoal da Maithe (`quest_maithe_windtrail`) | — | 8 | Ventania da Maithe | Parede |

<a id="util"></a>
## Utilitários (baús, prateleiras, bancadas) (31)

| ID | Nome | Raridade | Origem | Preço (ouro) | Conforto | Conjunto | Encaixe |
|---|---|---|---|---|---|---|---|
| `casa_util_001` | Baú de madeira simples | Comum | Missão da Casa: Um Cantinho Seu | — | 0 | — | Piso |
| `casa_util_002` | Baú de couro tacheado | Incomum | Loja: Marcenaria do Mestre Damião | 85 | 1 | Tropeiro do Vô Chico | Piso |
| `casa_util_003` | Baú de ferro reforçado | Raro | Loja: Forja do Mestre Tonho | 120 | 2 | — | Piso |
| `casa_util_004` | Arca de cedro com cadeado de segredo | Épico | Loja: Forja do Mestre Tonho | 565 | 4 | — | Piso |
| `casa_util_005` | Baú-correio da Vó Zefa | Épico | Missão da Casa: Tijolo por Tijolo | — | 4 | Varanda da Vó Zefa | Piso |
| `casa_util_006` | Guarda-roupa de duas portas | Comum | Loja: Marcenaria do Mestre Damião | 15 | 0 | — | Piso |
| `casa_util_007` | Guarda-roupa de jacarandá com espelho | Raro | Loja: Marcenaria do Mestre Damião | 230 | 3 | — | Piso |
| `casa_util_008` | Cabideiro de provador | Comum | Loja: Costureira Dona Fátima | 19 | 0 | — | Piso |
| `casa_util_009` | Manequim de costura | Incomum | Loja: Costureira Dona Fátima | 45 | 1 | — | Piso |
| `casa_util_010` | Espelho de provador de três folhas | Incomum | Loja: Marcenaria do Mestre Damião | 65 | 1 | — | Piso |
| `casa_util_011` | Bancada de marceneiro | Incomum | Loja: Marcenaria do Mestre Damião | 80 | 2 | — | Piso |
| `casa_util_012` | Bancada de mestre artesão | Épico | Conquista: Mestre Artesão | — | 4 | — | Piso |
| `casa_util_013` | Forja de quintal | Raro | Loja: Forja do Mestre Tonho | 195 | 2 | — | Quintal |
| `casa_util_014` | Mesa de alquimia da rezadeira | Raro | Loja: Armarinho da Dona Cida | 105 | 2 | Luz da Maria | Piso |
| `casa_util_015` | Tear de madeira | Raro | Loja: Costureira Dona Fátima | 155 | 3 | — | Piso |
| `casa_util_016` | Bacia de tingimento de anil | Incomum | Loja: Armarinho da Dona Cida | 65 | 1 | — | Piso |
| `casa_util_017` | Prateleira de tábua simples | Comum | Loja: Marcenaria do Mestre Damião | 14 | 0 | — | Parede |
| `casa_util_018` | Prateleira flutuante de cedro | Incomum | Loja: Marcenaria do Mestre Damião | 70 | 2 | — | Parede |
| `casa_util_019` | Estante do Caderno da Vó | Raro | Conquista: Caderno da Vó, de Capa a Capa | — | 2 | — | Piso |
| `casa_util_020` | Vitrine das doze relíquias | Lendário | Conquista: Relíquias da família (`ach_relics_all`) | — | 6 | — | Piso |
| `casa_util_021` | Pedestal dos Gigantes Tombados | Raro | Conquista: Gigante Tombado | — | 3 | — | Piso |
| `casa_util_022` | Parede de troféus de madeira | Incomum | Loja: Marcenaria do Mestre Damião | 85 | 1 | — | Parede |
| `casa_util_023` | Livro de visitas de capa de couro | Comum | Loja: Armarinho da Dona Cida | 19 | 1 | — | Superfície |
| `casa_util_024` | Caixa de correio de lata | Comum | Loja: Forja do Mestre Tonho | 20 | 1 | — | Quintal |
| `casa_util_025` | Cofrinho de porquinho de barro cheio | Raro | Conquista: Bom de Negócio | — | 3 | — | Superfície |
| `casa_util_026` | Sino da porteira | Comum | Loja: Forja do Mestre Tonho | 28 | 1 | Varanda da Vó Zefa | Quintal |
| `casa_util_027` | Quadro de avisos da família | Comum | Loja: Armarinho da Dona Cida | 20 | 1 | — | Parede |
| `casa_util_028` | Caixote de ferramentas | Comum | Loja: Forja do Mestre Tonho | 17 | 1 | — | Piso |
| `casa_util_029` | Gaveteiro de sementes | Incomum | Loja: Viveiro da Comadre Neném | 75 | 1 | Horta Caprichada | Piso |
| `casa_util_030` | Regador do ajudante com placa de horário | Comum | Loja: Viveiro da Comadre Neném | 24 | 1 | Horta Caprichada | Quintal |
| `casa_util_031` | Balcão de vendas do ajudante | Raro | Loja: Marcenaria do Mestre Damião | 150 | 2 | — | Piso |

<a id="conjuntos"></a>
## Conjuntos (resumo)

Bônus de conjunto em Housing.md §6.2. Peças por conjunto:

| Conjunto | Tema | Peças | Lendárias/Épicas |
|---|---|---|---|
| Varanda da Vó Zefa | aconchego clássico do sítio | 26 | 4 |
| Tropeiro do Vô Chico | couro, estrada e lembrança do avô | 25 | 6 |
| Casa de Farinha | pilão, tacho e mó da velha casa de farinha | 11 | 4 |
| Cozinha da Vó | fogão a lenha, ágata e barro | 16 | 3 |
| Oficina do Ícaro | engrenagens, fios e invenções | 17 | 4 |
| Ventania da Maithe | arco, pipa e rosa-dos-ventos | 9 | 4 |
| Luz da Maria | branco, dourado e ervas de cura | 13 | 4 |
| Raízes do Joaquim | madeira de lei, raiz e escudo | 8 | 4 |
| Noite Rubra da Lavine | estrelas, lua e sombra macia | 14 | 6 |
| Cantinho do Caramelo | tudo do cachorro da família | 7 | 0 |
| Mata Fechada | Floresta | 29 | 0 |
| Duna Dourada | Deserto | 24 | 2 |
| Pico Nevado | Montanha | 22 | 2 |
| Cristal das Cavernas | Cavernas | 22 | 4 |
| Feira Livre | chita, réstias e balança de feira | 12 | 1 |
| Cordel e Xilogravura | arte popular de papel e barro | 7 | 1 |
| Horta Caprichada | canteiros, abelhas e regador | 14 | 0 |
| Cristal do Sol | a luz que voltou para casa | 6 | 5 |
| Arraiá | Festa Junina | 22 | 3 |
| Natal Sertanejo | Natal | 14 | 1 |
| Frevo e Maracatu | Carnaval | 16 | 2 |

<a id="drops"></a>
## Itens de casa exclusivos de drop, por inimigo

Quantos itens de cada raridade estão no sorteio próprio de cada inimigo (além do sorteio regional da loja do bioma). Chances em Housing.md §7.2.

| Inimigo | ID | Comum | Incomum | Raro | Épico | Lendário | Total |
|---|---|---|---|---|---|---|---|
| Lobo da Mata | `enemy_forest_wolf` | 2 | 2 | 2 | 0 | 0 | 6 |
| Aranha da Floresta | `enemy_forest_spider` | 1 | 2 | 3 | 0 | 0 | 6 |
| Lobo Alfa (chefe) | `enemy_forest_alpha_wolf` | 0 | 1 | 3 | 2 | 1 | 7 |
| Escorpião das Dunas | `enemy_desert_scorpion` | 1 | 2 | 2 | 0 | 0 | 5 |
| Guardião da Ruína (chefe) | `enemy_desert_ruin_guardian` | 0 | 1 | 4 | 4 | 2 | 11 |
| Golem de Pedra | `enemy_mountain_stone_golem` | 1 | 2 | 4 | 1 | 0 | 8 |
| Senhor da Guerra da Montanha (chefe) | `enemy_mountain_warlord` | 0 | 1 | 6 | 2 | 1 | 10 |
| Enxame de Morcegos | `enemy_cave_bat_swarm` | 1 | 4 | 5 | 0 | 0 | 10 |
| Habitante Ancestral (chefe) | `enemy_cave_dweller` | 0 | 2 | 5 | 7 | 3 | 17 |
