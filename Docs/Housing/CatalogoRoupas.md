# Catálogo de roupas, armaduras e aparência — Rapadura

> Design do guarda-roupa, slots e regras de aparência em [`Docs/Housing.md`](../Housing.md) §9. **Conjunto inicial de lançamento, não a lista de tudo o que pode existir:** novas peças entram por temporada/evento com ID novo.
>
> **Este arquivo é dado de jogo:** as tabelas viram `Assets/Scripts/Gameplay/Wardrobe/WardrobeCatalogData.g.cs` pelo menu "Rapadura ▸ Housing ▸ Import Wardrobe Catalog" (nomes em inglês em `CatalogoRoupas.en.tsv`). Mexeu numa linha, rode o import: o `WardrobeTests` falha se a tabela gerada ficar velha. Estado do guarda-roupa no código: `Docs/Housing.md` §9.8.

**Total: 234 entradas** (120 peças de conjuntos dos primos, 28 roupas comuns a todos, 36 acessórios, 25 tinturas, 25 visuais de arma). Checagem automática: 234 IDs distintos, 234 nomes distintos (ignorando acento/maiúscula/pontuação), nenhuma duplicata.

| Raridade | Entradas |
|---|---|
| Comum | 63 |
| Incomum | 54 |
| Raro | 61 |
| Épico | 29 |
| Lendário | 27 |

## Como ler

- **Encaixe** usa os slots reais do `EquipmentSlot` (Cabeça = `Head`, Peito = `Chest`, Pernas = `Legs`, Pés = `Boots`, Acessório = `Accessory`) mais três encaixes **só de aparência** novos: Rosto/Pescoço, Costas e Ari (roupinha do cachorro; no código o encaixe continua `Caramelo`).
- **Nível** é a faixa de nível do personagem (teto atual 50, `progression.csv`) em que a peça aparece. Os atributos de cada peça vêm do sistema atual de equipamento (item base + raridade + afixos procedurais); o conjunto acrescenta só o bônus de conjunto (tabela no fim).
- **Festivo** é cosmético puro (raridade Rara, sem atributos): pode ser usado por cima de qualquer peça pelo guarda-roupa.
- **Preço** em ouro só para loja; "—" não se compra; eventos em oferendas.

## Maithe

| ID | Nome | Encaixe | Raridade | Nível | Origem | Preço (ouro) | Conjunto |
|---|---|---|---|---|---|---|---|
| `roupa_maithe_001` | Chapéu de palha de aba curta | Cabeça | Comum | 1–10 | Loja: Costureira Mestra Narisah | 21 | Batedora da Mata |
| `roupa_maithe_002` | Chapan de algodão cru | Peito | Comum | 1–10 | Loja: Costureira Mestra Narisah | 23 | Batedora da Mata |
| `roupa_maithe_003` | Calça de brim remendada | Pernas | Comum | 1–10 | Loja: Costureira Mestra Narisah | 15 | Batedora da Mata |
| `roupa_maithe_004` | Alpargatas de corda | Pés | Comum | 1–10 | Loja: Armazém Geral | 20 | Batedora da Mata |
| `roupa_maithe_005` | Lenço de suzani verde-musgo | Cabeça | Incomum | 10–20 | Loja: Costureira Mestra Narisah | 30 | Rastreadora do Riacho |
| `roupa_maithe_006` | Colete de couro com porta-flechas | Peito | Incomum | 10–20 | Drop: inimigos comuns da Floresta e do Deserto | — | Rastreadora do Riacho |
| `roupa_maithe_007` | Bermuda de lona de trilha | Pernas | Incomum | 10–20 | Loja: Costureira Mestra Narisah | 70 | Rastreadora do Riacho |
| `roupa_maithe_008` | Botina de cano curto da trilha | Pés | Incomum | 10–20 | Missão regional da Floresta (2ª) | — | Rastreadora do Riacho |
| `roupa_maithe_009` | Tiara de penas de gavião | Cabeça | Raro | 20–30 | Loja: Costureira Mestra Narisah | 140 | Pena de Gavião |
| `roupa_maithe_010` | Túnica de linho verde bordada de vento | Peito | Raro | 20–30 | Drop: Kök-Böri (chefe) | — | Pena de Gavião |
| `roupa_maithe_011` | Calça justa de caçadora | Pernas | Raro | 20–30 | Loja: Costureira Mestra Narisah | 120 | Pena de Gavião |
| `roupa_maithe_012` | Botas de couro macio de passo leve | Pés | Raro | 20–30 | Diária: Ronda da Manhã | — | Pena de Gavião |
| `roupa_maithe_013` | Capuz do Olho do Vento | Cabeça | Épico | 30–40 | Missão pessoal da Maithe | — | Olho do Vento |
| `roupa_maithe_014` | Manto de correnteza de ar | Peito | Épico | 30–40 | Drop: Kök-Böri (chefe) | — | Olho do Vento |
| `roupa_maithe_015` | Perneiras de vento trançado | Pernas | Épico | 30–40 | Crafting: Tear (tecido + material de chefe) | — | Olho do Vento |
| `roupa_maithe_016` | Sandálias aladas de tira | Pés | Épico | 30–40 | Drop: Hahya, o Gigante Guerreiro (chefe) | — | Olho do Vento |
| `roupa_maithe_017` | Coroa de nuvem de tempestade | Cabeça | Lendário | 40–50 | Conquista: Olho do Vento | — | Tempestade de Mil Flechas |
| `roupa_maithe_018` | Couraça de penas de trovão | Peito | Lendário | 40–50 | Drop: Tages, o Habitante Ancestral (chefe) | — | Tempestade de Mil Flechas |
| `roupa_maithe_019` | Saia-calça de relâmpago | Pernas | Lendário | 40–50 | Crafting: Tear de mestre (4 lembranças de chefe) | — | Tempestade de Mil Flechas |
| `roupa_maithe_020` | Botas do vendaval | Pés | Lendário | 40–50 | Conquista: Acender de Novo (Nova Jornada+) | — | Tempestade de Mil Flechas |
| `roupa_maithe_021` | Coroa de flores de damasqueiro | Cabeça | Raro | qualquer | Evento: Bema | — (30 oferendas) | Cantora do Bema |
| `roupa_maithe_022` | Vestido de ikat verde com fitas | Peito | Raro | qualquer | Evento: Bema | — (40 oferendas) | Cantora do Bema |
| `roupa_maithe_023` | Anágua de renda do Bema | Pernas | Raro | qualquer | Evento: Bema | — (30 oferendas) | Cantora do Bema |
| `roupa_maithe_024` | Sapatilha de pano florida | Pés | Raro | qualquer | Evento: Bema | — (25 oferendas) | Cantora do Bema |

## Maria

| ID | Nome | Encaixe | Raridade | Nível | Origem | Preço (ouro) | Conjunto |
|---|---|---|---|---|---|---|---|
| `roupa_maria_001` | Touca de linho branco | Cabeça | Comum | 1–10 | Loja: Costureira Mestra Narisah | 11 | Ajudante do Templo |
| `roupa_maria_002` | Bata de algodão alvejado | Peito | Comum | 1–10 | Loja: Costureira Mestra Narisah | 24 | Ajudante do Templo |
| `roupa_maria_003` | Saia de linho cru | Pernas | Comum | 1–10 | Loja: Costureira Mestra Narisah | 24 | Ajudante do Templo |
| `roupa_maria_004` | Sandálias de couro cru | Pés | Comum | 1–10 | Loja: Armazém Geral | 20 | Ajudante do Templo |
| `roupa_maria_005` | Véu de linho bordado de suzani | Cabeça | Incomum | 10–20 | Loja: Costureira Mestra Narisah | 40 | Curandeira Aprendiz |
| `roupa_maria_006` | Xale de seda sogdiana dourada | Peito | Incomum | 10–20 | Drop: inimigos comuns da Floresta e do Deserto | — | Curandeira Aprendiz |
| `roupa_maria_007` | Saia rodada de ikat amarelo | Pernas | Incomum | 10–20 | Loja: Costureira Mestra Narisah | 60 | Curandeira Aprendiz |
| `roupa_maria_008` | Chinelo de couro trançado | Pés | Incomum | 10–20 | Missão regional da Floresta (2ª) | — | Curandeira Aprendiz |
| `roupa_maria_009` | Diadema de latão polido | Cabeça | Raro | 20–30 | Loja: Costureira Mestra Narisah | 145 | Luz de Candeia |
| `roupa_maria_010` | Túnica de cetim dourado | Peito | Raro | 20–30 | Drop: Hahya, o Gigante Guerreiro (chefe) | — | Luz de Candeia |
| `roupa_maria_011` | Saia de linho com barrado de sol | Pernas | Raro | 20–30 | Loja: Costureira Mestra Narisah | 90 | Luz de Candeia |
| `roupa_maria_012` | Botinhas de camurça clara | Pés | Raro | 20–30 | Diária: A Cesta de Ervas da Maria | — | Luz de Candeia |
| `roupa_maria_013` | Auréola de sol nascente | Cabeça | Épico | 30–40 | Missão pessoal da Maria | — | Templo Solar |
| `roupa_maria_014` | Manto do Templo Solar | Peito | Épico | 30–40 | Drop: Hahya, o Gigante Guerreiro (chefe) | — | Templo Solar |
| `roupa_maria_015` | Saia de raios dourados | Pernas | Épico | 30–40 | Crafting: Tear (tecido + material de chefe) | — | Templo Solar |
| `roupa_maria_016` | Sandálias de luz de vela | Pés | Épico | 30–40 | Drop: Hahya, o Gigante Guerreiro (chefe) | — | Templo Solar |
| `roupa_maria_017` | Coroa de estrelas da manhã | Cabeça | Lendário | 40–50 | Conquista: Luz que Não se Apaga | — | Milagre Celestial |
| `roupa_maria_018` | Vestes do Milagre Celestial | Peito | Lendário | 40–50 | Drop: Tages, o Habitante Ancestral (chefe) | — | Milagre Celestial |
| `roupa_maria_019` | Saia de aurora | Pernas | Lendário | 40–50 | Crafting: Tear de mestre (4 lembranças de chefe) | — | Milagre Celestial |
| `roupa_maria_020` | Passos de luz celestial | Pés | Lendário | 40–50 | Conquista: Acender de Novo (Nova Jornada+) | — | Milagre Celestial |
| `roupa_maria_021` | Gorro de feltro com fita de Yalda | Cabeça | Raro | qualquer | Evento: Noite de Yalda | — (30 oferendas) | Portadora da Lâmpada |
| `roupa_maria_022` | Corpete azul de lamparineira | Peito | Raro | qualquer | Evento: Noite de Yalda | — (40 oferendas) | Portadora da Lâmpada |
| `roupa_maria_023` | Saia de tule azul | Pernas | Raro | qualquer | Evento: Noite de Yalda | — (30 oferendas) | Portadora da Lâmpada |
| `roupa_maria_024` | Sapatilha de cetim azul | Pés | Raro | qualquer | Evento: Noite de Yalda | — (25 oferendas) | Portadora da Lâmpada |

## Joaquim

| ID | Nome | Encaixe | Raridade | Nível | Origem | Preço (ouro) | Conjunto |
|---|---|---|---|---|---|---|---|
| `roupa_joaquim_001` | Gorro de feltro surrado | Cabeça | Comum | 1–10 | Loja: Costureira Mestra Narisah | 11 | Menino do Pomar |
| `roupa_joaquim_002` | Camisa de flanela xadrez | Peito | Comum | 1–10 | Loja: Costureira Mestra Narisah | 19 | Menino do Pomar |
| `roupa_joaquim_003` | Calça de brim com joelheira | Pernas | Comum | 1–10 | Loja: Costureira Mestra Narisah | 22 | Menino do Pomar |
| `roupa_joaquim_004` | Botina do pomar | Pés | Comum | 1–10 | Loja: Armazém Geral | 20 | Menino do Pomar |
| `roupa_joaquim_005` | Gorro de feltro de cavaleiro com barbicacho | Cabeça | Incomum | 10–20 | Loja: Forja do Mestre Mihr | 30 | Cavaleirinho da Estepe |
| `roupa_joaquim_006` | Chapan de couro de cavaleiro | Peito | Incomum | 10–20 | Drop: inimigos comuns da Floresta e do Deserto | — | Cavaleirinho da Estepe |
| `roupa_joaquim_007` | Perneiras de couro cru | Pernas | Incomum | 10–20 | Loja: Forja do Mestre Mihr | 55 | Cavaleirinho da Estepe |
| `roupa_joaquim_008` | Botas de montaria sem espora | Pés | Incomum | 10–20 | Missão regional da Floresta (2ª) | — | Cavaleirinho da Estepe |
| `roupa_joaquim_009` | Elmo de casca de choupo | Cabeça | Raro | 20–30 | Loja: Forja do Mestre Mihr | 140 | Casca de Choupo |
| `roupa_joaquim_010` | Peitoral de tábuas de choupo | Peito | Raro | 20–30 | Drop: O Grande Ushabti (chefe) | — | Casca de Choupo |
| `roupa_joaquim_011` | Coxotes de madeira de lei | Pernas | Raro | 20–30 | Loja: Forja do Mestre Mihr | 195 | Casca de Choupo |
| `roupa_joaquim_012` | Botas de raiz batida | Pés | Raro | 20–30 | Diária: Vigia dos Escorpiões | — | Casca de Choupo |
| `roupa_joaquim_013` | Elmo de granito polido | Cabeça | Épico | 30–40 | Missão pessoal do Joaquim | — | Muralha de Pedra |
| `roupa_joaquim_014` | Couraça de pedra encaixada | Peito | Épico | 30–40 | Drop: O Grande Ushabti (chefe) | — | Muralha de Pedra |
| `roupa_joaquim_015` | Grevas de laje | Pernas | Épico | 30–40 | Crafting: Forja (lingotes + material de chefe) | — | Muralha de Pedra |
| `roupa_joaquim_016` | Botas de pedra-sabão | Pés | Épico | 30–40 | Drop: Hahya, o Gigante Guerreiro (chefe) | — | Muralha de Pedra |
| `roupa_joaquim_017` | Elmo da Raiz Eterna | Cabeça | Lendário | 40–50 | Conquista: Ecos de Casa | — | Última Resistência |
| `roupa_joaquim_018` | Armadura das Raízes Eternas | Peito | Lendário | 40–50 | Drop: Tages, o Habitante Ancestral (chefe) | — | Última Resistência |
| `roupa_joaquim_019` | Grevas do tronco milenar | Pernas | Lendário | 40–50 | Crafting: Tear de mestre (4 lembranças de chefe) | — | Última Resistência |
| `roupa_joaquim_020` | Botas que não arredam pé | Pés | Lendário | 40–50 | Conquista: Acender de Novo (Nova Jornada+) | — | Última Resistência |
| `roupa_joaquim_021` | Chapéu de fitas de lanterna | Cabeça | Raro | qualquer | Evento: Festa das Lanternas | — (30 oferendas) | Dançarino das Fitas |
| `roupa_joaquim_022` | Gola de lantejoulas | Peito | Raro | qualquer | Evento: Festa das Lanternas | — (40 oferendas) | Dançarino das Fitas |
| `roupa_joaquim_023` | Calça de cetim listrada | Pernas | Raro | qualquer | Evento: Festa das Lanternas | — (30 oferendas) | Dançarino das Fitas |
| `roupa_joaquim_024` | Tênis enfeitado de fitas | Pés | Raro | qualquer | Evento: Festa das Lanternas | — (25 oferendas) | Dançarino das Fitas |

## Ícaro

| ID | Nome | Encaixe | Raridade | Nível | Origem | Preço (ouro) | Conjunto |
|---|---|---|---|---|---|---|---|
| `roupa_icaro_001` | Boné de mecânico | Cabeça | Comum | 1–10 | Loja: Costureira Mestra Narisah | 24 | Aprendiz de Oficina |
| `roupa_icaro_002` | Macacão de brim manchado de graxa | Peito | Comum | 1–10 | Loja: Costureira Mestra Narisah | 20 | Aprendiz de Oficina |
| `roupa_icaro_003` | Bermuda de bolsos de ferramenta | Pernas | Comum | 1–10 | Loja: Costureira Mestra Narisah | 25 | Aprendiz de Oficina |
| `roupa_icaro_004` | Tênis de lona remendado | Pés | Comum | 1–10 | Loja: Armazém Geral | 13 | Aprendiz de Oficina |
| `roupa_icaro_005` | Fones de rádio de lata | Cabeça | Incomum | 10–20 | Loja: Forja do Mestre Mihr | 60 | Rádio Desmontado |
| `roupa_icaro_006` | Colete de fios coloridos | Peito | Incomum | 10–20 | Drop: inimigos comuns da Floresta e do Deserto | — | Rádio Desmontado |
| `roupa_icaro_007` | Calça cargo de antenas | Pernas | Incomum | 10–20 | Loja: Forja do Mestre Mihr | 60 | Rádio Desmontado |
| `roupa_icaro_008` | Botas com sola de pneu | Pés | Incomum | 10–20 | Missão regional da Floresta (2ª) | — | Rádio Desmontado |
| `roupa_icaro_009` | Óculos de solda de latão | Cabeça | Raro | 20–30 | Loja: Forja do Mestre Mihr | 170 | Engrenagem de Latão |
| `roupa_icaro_010` | Jaqueta de engrenagens | Peito | Raro | 20–30 | Drop: O Grande Ushabti (chefe) | — | Engrenagem de Latão |
| `roupa_icaro_011` | Calça de couro com rebites | Pernas | Raro | 20–30 | Loja: Forja do Mestre Mihr | 135 | Engrenagem de Latão |
| `roupa_icaro_012` | Botas de mola | Pés | Raro | 20–30 | Diária: Peças de Reposição | — | Engrenagem de Latão |
| `roupa_icaro_013` | Capacete de bobina de cobre | Cabeça | Épico | 30–40 | Missão pessoal do Ícaro | — | Campo Magnético |
| `roupa_icaro_014` | Peitoral de ímãs giratórios | Peito | Épico | 30–40 | Drop: O Grande Ushabti (chefe) | — | Campo Magnético |
| `roupa_icaro_015` | Calça de malha condutora | Pernas | Épico | 30–40 | Crafting: Forja (lingotes + material de chefe) | — | Campo Magnético |
| `roupa_icaro_016` | Botas de levitação magnética | Pés | Épico | 30–40 | Drop: Hahya, o Gigante Guerreiro (chefe) | — | Campo Magnético |
| `roupa_icaro_017` | Elmo do Comandante das Máquinas | Cabeça | Lendário | 40–50 | Conquista: Engrenagens Antigas | — | Exército Mecânico |
| `roupa_icaro_018` | Exoarmadura de cobre e vapor | Peito | Lendário | 40–50 | Drop: Tages, o Habitante Ancestral (chefe) | — | Exército Mecânico |
| `roupa_icaro_019` | Pernas mecânicas assistidas | Pernas | Lendário | 40–50 | Crafting: Tear de mestre (4 lembranças de chefe) | — | Exército Mecânico |
| `roupa_icaro_020` | Botas a jato de torrão dourado | Pés | Lendário | 40–50 | Conquista: Acender de Novo (Nova Jornada+) | — | Exército Mecânico |
| `roupa_icaro_021` | Chapéu de palha com antena | Cabeça | Raro | qualquer | Evento: Bema | — (30 oferendas) | Inventor do Bema |
| `roupa_icaro_022` | Camisa de ikat de lampadinhas | Peito | Raro | qualquer | Evento: Bema | — (40 oferendas) | Inventor do Bema |
| `roupa_icaro_023` | Calça com remendo de circuito | Pernas | Raro | qualquer | Evento: Bema | — (30 oferendas) | Inventor do Bema |
| `roupa_icaro_024` | Botina de pisca-pisca | Pés | Raro | qualquer | Evento: Bema | — (25 oferendas) | Inventor do Bema |

## Lavine

| ID | Nome | Encaixe | Raridade | Nível | Origem | Preço (ouro) | Conjunto |
|---|---|---|---|---|---|---|---|
| `roupa_lavine_001` | Laço de fita roxa | Cabeça | Comum | 1–10 | Loja: Costureira Mestra Narisah | 19 | Caçulinha Curiosa |
| `roupa_lavine_002` | Vestidinho de ikat roxo | Peito | Comum | 1–10 | Loja: Costureira Mestra Narisah | 11 | Caçulinha Curiosa |
| `roupa_lavine_003` | Meia-calça listrada | Pernas | Comum | 1–10 | Loja: Costureira Mestra Narisah | 23 | Caçulinha Curiosa |
| `roupa_lavine_004` | Sapatinho de verniz | Pés | Comum | 1–10 | Loja: Armazém Geral | 21 | Caçulinha Curiosa |
| `roupa_lavine_005` | Chapéu pontudo de papel de seda | Cabeça | Incomum | 10–20 | Loja: Costureira Mestra Narisah | 55 | Contadora de Estrelas |
| `roupa_lavine_006` | Capinha de veludo lilás | Peito | Incomum | 10–20 | Drop: inimigos comuns da Floresta e do Deserto | — | Contadora de Estrelas |
| `roupa_lavine_007` | Saia de estrelinhas | Pernas | Incomum | 10–20 | Loja: Costureira Mestra Narisah | 65 | Contadora de Estrelas |
| `roupa_lavine_008` | Botinhas de feltro | Pés | Incomum | 10–20 | Missão regional da Floresta (2ª) | — | Contadora de Estrelas |
| `roupa_lavine_009` | Capuz de sombra macia | Cabeça | Raro | 20–30 | Loja: Costureira Mestra Narisah | 90 | Sombra Macia |
| `roupa_lavine_010` | Túnica de crepúsculo | Peito | Raro | 20–30 | Drop: Tages, o Habitante Ancestral (chefe) | — | Sombra Macia |
| `roupa_lavine_011` | Saia de névoa | Pernas | Raro | 20–30 | Loja: Costureira Mestra Narisah | 150 | Sombra Macia |
| `roupa_lavine_012` | Pantufas silenciosas | Pés | Raro | 20–30 | Diária: A Lavine e os Morcegos | — | Sombra Macia |
| `roupa_lavine_013` | Tiara de lua vermelha | Cabeça | Épico | 30–40 | Missão pessoal da Lavine | — | Noite Rubra |
| `roupa_lavine_014` | Manto da Noite Rubra | Peito | Épico | 30–40 | Drop: Tages, o Habitante Ancestral (chefe) | — | Noite Rubra |
| `roupa_lavine_015` | Saia de eclipse | Pernas | Épico | 30–40 | Crafting: Tear (tecido + material de chefe) | — | Noite Rubra |
| `roupa_lavine_016` | Botas de brasa fria | Pés | Épico | 30–40 | Drop: Hahya, o Gigante Guerreiro (chefe) | — | Noite Rubra |
| `roupa_lavine_017` | Coroa do Eclipse Eterno | Cabeça | Lendário | 40–50 | Conquista: Coragem da Caçula | — | Eclipse Eterno |
| `roupa_lavine_018` | Vestes da Lua Escondida | Peito | Lendário | 40–50 | Drop: Tages, o Habitante Ancestral (chefe) | — | Eclipse Eterno |
| `roupa_lavine_019` | Saia do horizonte apagado | Pernas | Lendário | 40–50 | Crafting: Tear de mestre (4 lembranças de chefe) | — | Eclipse Eterno |
| `roupa_lavine_020` | Passos de meia-noite | Pés | Lendário | 40–50 | Conquista: Acender de Novo (Nova Jornada+) | — | Eclipse Eterno |
| `roupa_lavine_021` | Coroa de lanterna com espelhinhos | Cabeça | Raro | qualquer | Evento: Noite de Yalda | — (30 oferendas) | Estrelinha das Lanternas |
| `roupa_lavine_022` | Bata de Yalda com fitas | Peito | Raro | qualquer | Evento: Noite de Yalda | — (40 oferendas) | Estrelinha das Lanternas |
| `roupa_lavine_023` | Calça de cetim vermelho | Pernas | Raro | qualquer | Evento: Noite de Yalda | — (30 oferendas) | Estrelinha das Lanternas |
| `roupa_lavine_024` | Sapatilha de lantejoulas | Pés | Raro | qualquer | Evento: Noite de Yalda | — (25 oferendas) | Estrelinha das Lanternas |

## Roupas comuns a todos os primos

| ID | Nome | Encaixe | Raridade | Origem | Preço (ouro) |
|---|---|---|---|---|---|
| `roupa_comum_001` | Camisa de ikat de manga curta | Peito | Comum | Loja: Costureira Mestra Narisah | 24 |
| `roupa_comum_002` | Vestido de suzani rodado | Peito | Comum | Loja: Costureira Mestra Narisah | 11 |
| `roupa_comum_003` | Camiseta "Eu amo a Ilha da Aurora" | Peito | Comum | Loja: Armazém Geral | 23 |
| `roupa_comum_004` | Camisa do time de futebol da vila | Peito | Comum | Loja: Armazém Geral | 20 |
| `roupa_comum_005` | Jardineira jeans com bolso de canguru | Peito | Incomum | Loja: Costureira Mestra Narisah | 45 |
| `roupa_comum_006` | Pijama de flanela de carneirinho | Peito | Incomum | Loja: Costureira Mestra Narisah | 40 |
| `roupa_comum_007` | Capa de chuva amarela de botão | Peito | Comum | Loja: Armazém Geral | 25 |
| `roupa_comum_008` | Bata bordada de suzani | Peito | Raro | Loja: Costureira Mestra Narisah | 90 |
| `roupa_comum_009` | Chapan de couro de caravaneiro adulto, grande demais | Peito | Incomum | Loja: Caravana do Ammo | 65 |
| `roupa_comum_010` | Suéter de tricô da Vó Edna | Peito | Raro | Missão da Casa: Parede de Barro | — |
| `roupa_comum_011` | Camisa de festa de linho branco | Peito | Incomum | Loja: Costureira Mestra Narisah | 60 |
| `roupa_comum_012` | Colete de caravaneiro com bordado de estrada | Peito | Raro | Missão: Reserva pra Travessia | — |
| `roupa_comum_013` | Agasalho de lã da serra | Peito | Incomum | Loja: Forja do Mestre Mihr | 35 |
| `roupa_comum_014` | Avental de cozinha da Vó Edna | Peito | Comum | Missão da Casa: Fogo na Cozinha | — |
| `roupa_comum_015` | Bermuda de tactel colorida | Pernas | Comum | Loja: Armazém Geral | 21 |
| `roupa_comum_016` | Calça de pijama listrada | Pernas | Comum | Loja: Costureira Mestra Narisah | 20 |
| `roupa_comum_017` | Saia de ikat com bolsos | Pernas | Comum | Loja: Costureira Mestra Narisah | 22 |
| `roupa_comum_018` | Calça de brim do bazar | Pernas | Comum | Loja: Bazar da Nana | 11 |
| `roupa_comum_019` | Saia de babados do Bema | Pernas | Incomum | Evento: Bema | — (20 oferendas) |
| `roupa_comum_020` | Calça de lã xadrez da serra | Pernas | Incomum | Loja: Forja do Mestre Mihr | 55 |
| `roupa_comum_021` | Alpargatas de lona azul | Pés | Comum | Loja: Bazar da Nana | 13 |
| `roupa_comum_022` | Chinelo de dedo de borracha | Pés | Comum | Loja: Armazém Geral | 17 |
| `roupa_comum_023` | Sandália de couro de rabicho | Pés | Incomum | Loja: Caravana do Ammo | 60 |
| `roupa_comum_024` | Pantufa de cachorrinho do Ari | Pés | Raro | Conquista: Juntos, ou Nada Feito | — |
| `roupa_comum_025` | Galochas de chuva verdes | Pés | Comum | Loja: Armazém Geral | 18 |
| `roupa_comum_026` | Bota de couro do caravaneiro | Pés | Raro | Drop: O Grande Ushabti (chefe) | — |
| `roupa_comum_027` | Tamanco de madeira do bazar | Pés | Incomum | Loja: Bazar da Nana | 65 |
| `roupa_comum_028` | Pés descalços sujos de terra | Pés | Comum | Desbloqueado desde o começo | — |

## Acessórios (inclui roupinhas do Ari)

| ID | Nome | Para | Encaixe | Raridade | Origem | Preço (ouro) |
|---|---|---|---|---|---|---|
| `acess_001` | Gorro de feltro de aba virada | Todos | Cabeça | Raro | Loja: Caravana do Ammo | 90 |
| `acess_002` | Réplica do gorro de feltro do Vô Jorge | Todos | Cabeça | Épico | Segredo do mapa (relíquia `relic_chapeu_couro` encontrada) | — |
| `acess_003` | Chapéu de meia-lua de feltro | Todos | Cabeça | Raro | Drop: O Grande Ushabti (chefe) | — |
| `acess_004` | Chapéu de palha desfiado | Todos | Cabeça | Comum | Loja: Bazar da Nana | 25 |
| `acess_005` | Boné virado para trás | Todos | Cabeça | Comum | Loja: Armazém Geral | 15 |
| `acess_006` | Coroa de flores de romã | Todos | Cabeça | Incomum | Loja: Viveiro da Nana | 55 |
| `acess_007` | Tiara de flor de amendoeira | Todos | Cabeça | Incomum | Loja: Viveiro da Nana | 55 |
| `acess_008` | Gorro de lã com pompom | Todos | Cabeça | Comum | Loja: Forja do Mestre Mihr | 19 |
| `acess_009` | Lenço de pescoço vermelho de caravaneiro | Todos | Rosto/Pescoço | Comum | Loja: Bazar de Linhas da Mariam | 25 |
| `acess_010` | Lenço de seda estampado de romãs | Todos | Rosto/Pescoço | Incomum | Loja: Bazar de Linhas da Mariam | 30 |
| `acess_011` | Óculos redondos de aro de arame | Todos | Rosto/Pescoço | Incomum | Loja: Caravana do Ammo | 35 |
| `acess_012` | Colar de caroços de damasco | Todos | Rosto/Pescoço | Comum | Loja: Bazar da Nana | 15 |
| `acess_013` | Colar de contas de cristal das grutas | Todos | Rosto/Pescoço | Raro | Loja: Lamparinas da Parthenos | 185 |
| `acess_014` | Medalhinha de latão da Vó Edna | Todos | Rosto/Pescoço | Épico | Missão da Casa: Casa Cheia | — |
| `acess_015` | Bigode falso do Bema | Todos | Rosto/Pescoço | Comum | Evento: Bema | — (10 oferendas) |
| `acess_016` | Pintinhas de sarda pintadas a carvão | Todos | Rosto/Pescoço | Comum | Evento: Bema | — (10 oferendas) |
| `acess_017` | Máscara de leão de papel machê | Todos | Rosto/Pescoço | Incomum | Evento: Festa das Lanternas | — (20 oferendas) |
| `acess_018` | Alforje de couro cru | Todos | Costas | Comum | Loja: Costureira Mestra Narisah | 23 |
| `acess_019` | Mochila de lona com remendos | Todos | Costas | Comum | Loja: Armazém Geral | 19 |
| `acess_020` | Cabaça-cantil a tiracolo | Todos | Costas | Incomum | Loja: Caravana do Ammo | 35 |
| `acess_021` | Capa de super-herói de toalha de banho | Todos | Costas | Incomum | Conquista: Mão Amiga | — |
| `acess_022` | Asas de papel de seda | Todos | Costas | Raro | Evento: Festa das Lanternas | — (35 oferendas) |
| `acess_023` | Lanterna-peixe nas costas | Todos | Costas | Raro | Evento: Festa das Lanternas | — (35 oferendas) |
| `acess_024` | Rede de seda enrolada nas costas | Todos | Costas | Incomum | Loja: Costureira Mestra Narisah | 55 |
| `acess_025` | Ari de pelúcia na mochila | Todos | Costas | Épico | Conquista: Prima de Todos | — |
| `acess_026` | Pulseira de fitinhas coloridas | Todos | Acessório | Comum | Loja: Bazar de Linhas da Mariam | 25 |
| `acess_027` | Relógio de pulso de corda | Todos | Acessório | Incomum | Loja: Caravana do Ammo | 65 |
| `acess_028` | Anel de tampinha de garrafa | Todos | Acessório | Comum | Loja: Bazar da Nana | 18 |
| `acess_029` | Broche de torrão dourado | Todos | Acessório | Raro | Conquista: Bom de Negócio | — |
| `acess_030` | Lanterninha de pendurar no cinto | Todos | Acessório | Incomum | Loja: Lamparinas da Parthenos | 40 |
| `acess_031` | Lenço vermelho de pescoço do Ari | Ari | Ari | Comum | Loja: Bazar de Linhas da Mariam | 22 |
| `acess_032` | Coleira de couro com plaquinha de nome | Ari | Ari | Comum | Loja: Bazar de Linhas da Mariam | 20 |
| `acess_033` | Chapeuzinho de palha do Ari | Ari | Ari | Incomum | Evento: Bema | — (20 oferendas) |
| `acess_034` | Gorrinho de Yalda do Ari | Ari | Ari | Incomum | Evento: Noite de Yalda | — (20 oferendas) |
| `acess_035` | Capinha de chuva amarela do Ari | Ari | Ari | Incomum | Loja: Costureira Mestra Narisah | 55 |
| `acess_036` | Bandana de engrenagens do Ari | Ari | Ari | Raro | Conquista: Vento e Engrenagem | — |

## Tinturas

Tinta é **consumível**: cada aplicação gasta 1 unidade e pinta uma peça (canal principal ou secundário). Ver Housing.md §9.4.

| ID | Nome | Cor | Raridade | Origem | Preço por unidade (ouro) |
|---|---|---|---|---|---|
| `tinta_001` | Tinta de índigo | Azul | Comum | Loja: Bazar de Linhas da Mariam | 7 |
| `tinta_002` | Tinta de garança | Vermelho-alaranjado | Comum | Loja: Bazar da Nana | 5 |
| `tinta_003` | Tinta de açafrão | Amarelo | Comum | Loja: Mercearia do Zaku | 5 |
| `tinta_004` | Tinta de noz-de-galha | Preto-azulado | Incomum | Loja: Viveiro da Nana | 16 |
| `tinta_005` | Tinta de barro vermelho | Terracota | Comum | Loja: Olaria da Náfsha | 5 |
| `tinta_006` | Tinta de carvão do tandir | Cinza-carvão | Comum | Crafting: Tandir | — |
| `tinta_007` | Tinta de casca de nogueira | Marrom | Comum | Loja: Bazar da Nana | 8 |
| `tinta_008` | Tinta de romã | Vermelho intenso | Raro | Loja: Caravana do Ammo | 30 |
| `tinta_009` | Tinta de folha de amoreira | Verde-folha | Comum | Loja: Viveiro da Nana | 6 |
| `tinta_010` | Tinta de casca de cebola | Dourado-palha | Comum | Loja: Mercearia do Zaku | 7 |
| `tinta_011` | Tinta de beterraba | Rosa | Comum | Loja: Bazar da Nana | 7 |
| `tinta_012` | Tinta de cal | Branco | Comum | Loja: Olaria da Náfsha | 5 |
| `tinta_013` | Tinta de ocre de Litema | Ocre | Incomum | Loja: Forja do Mestre Mihr | 13 |
| `tinta_014` | Tinta de areia dourada | Dourado-areia | Incomum | Drop: Escorpião das Areias Flamejantes | — |
| `tinta_015` | Tinta de turquesa das grutas | Azul-cristal | Raro | Drop: Enxame de Morcegos do Limiar | — |
| `tinta_016` | Tinta verde-musgo do tugai | Verde-musgo | Incomum | Drop: Aranha-Tecelã Apagada | — |
| `tinta_017` | Tinta azul de Yalda | Azul-celeste | Incomum | Evento: Noite de Yalda | — (20 oferendas) |
| `tinta_018` | Tinta rosa do Bema | Rosa-choque | Incomum | Evento: Bema | — (20 oferendas) |
| `tinta_019` | Tinta de cobre polido | Cobre | Raro | Crafting: Forja | — |
| `tinta_020` | Tinta lilás de crepúsculo | Lilás | Raro | Loja: Lamparinas da Parthenos | 36 |
| `tinta_021` | Tinta cinza de pedra | Cinza-pedra | Comum | Drop: Ushabti de Pedra | — |
| `tinta_022` | Tinta preta da noite | Preto | Incomum | Loja: Lamparinas da Parthenos | 21 |
| `tinta_023` | Tintura arco-íris de ikat | Estampa de ikat (multicor) | Épico | Evento: Festa das Lanternas | — (70 oferendas) |
| `tinta_024` | Tinta da Lua Rubra | Vermelho-escuro com brilho | Lendário | Drop: Tages, o Habitante Ancestral (chefe) | — |
| `tinta_025` | Tinta do sol nascente | Dourado brilhante | Lendário | Conquista: A Luz Volta para Casa | — |

## Visuais de arma (skins)

Só aparência: aplicados no guarda-roupa sobre a arma equipada do mesmo tipo (arco da Maithe, cajado da Maria, escudo do Joaquim, ferramenta do Ícaro, grimório/orbe da Lavine).

| ID | Nome | Primo | Raridade | Origem | Preço (ouro) |
|---|---|---|---|---|---|
| `visual_arma_maithe_001` | Arco de bambu de pipa | Maithe | Comum | Loja: Bazar da Nana | 21 |
| `visual_arma_maithe_002` | Arco de galho de damasqueiro | Maithe | Incomum | Loja: Marcenaria do Mestre Omóforo | 70 |
| `visual_arma_maithe_003` | Arco de saxaul florido | Maithe | Raro | Drop: Escorpião das Areias Flamejantes | — |
| `visual_arma_maithe_004` | Arco do vento cantante | Maithe | Épico | Missão pessoal da Maithe | — |
| `visual_arma_maithe_005` | Arco do Cristal do Sol | Maithe | Lendário | Drop: Tages, o Habitante Ancestral (chefe) | — |
| `visual_arma_maria_001` | Cajado de galho de tamargueira | Maria | Comum | Loja: Viveiro da Nana | 22 |
| `visual_arma_maria_002` | Cajado de lamparina | Maria | Incomum | Loja: Lamparinas da Parthenos | 45 |
| `visual_arma_maria_003` | Cajado de girassol | Maria | Raro | Diária: A Cesta de Ervas da Maria | — |
| `visual_arma_maria_004` | Cajado de vidro colorido | Maria | Épico | Missão pessoal da Maria | — |
| `visual_arma_maria_005` | Cajado da aurora | Maria | Lendário | Conquista: Luz que Não se Apaga | — |
| `visual_arma_joaquim_001` | Escudo de tampa de panela | Joaquim | Comum | Loja: Mercearia do Zaku | 25 |
| `visual_arma_joaquim_002` | Escudo de roda de carroça | Joaquim | Incomum | Loja: Marcenaria do Mestre Omóforo | 65 |
| `visual_arma_joaquim_003` | Escudo de mandala de vime | Joaquim | Raro | Drop: Aranha-Tecelã Apagada | — |
| `visual_arma_joaquim_004` | Escudo de pedra-sabão entalhada | Joaquim | Épico | Drop: Hahya, o Gigante Guerreiro (chefe) | — |
| `visual_arma_joaquim_005` | Escudo dourado da Raiz Eterna | Joaquim | Lendário | Conquista: Ecos de Casa | — |
| `visual_arma_icaro_001` | Chave inglesa de brinquedo | Ícaro | Comum | Loja: Armazém Geral | 19 |
| `visual_arma_icaro_002` | Lançador de garrafa de refrigerante | Ícaro | Incomum | Crafting: Bancada | — |
| `visual_arma_icaro_003` | Chave de engrenagem de latão | Ícaro | Raro | Drop: O Grande Ushabti (chefe) | — |
| `visual_arma_icaro_004` | Bobina elétrica de lata | Ícaro | Épico | Missão pessoal do Ícaro | — |
| `visual_arma_icaro_005` | Canhão de confete de torrão dourado | Ícaro | Lendário | Evento: Festa das Lanternas (ranking do evento) | — |
| `visual_arma_lavine_001` | Grimório de caderno de escola | Lavine | Comum | Loja: Bazar de Linhas da Mariam | 11 |
| `visual_arma_lavine_002` | Orbe de bolha de sabão | Lavine | Incomum | Loja: Bazar da Nana | 50 |
| `visual_arma_lavine_003` | Livro de iluminuras encantado | Lavine | Raro | Diária: A Lavine e os Morcegos | — |
| `visual_arma_lavine_004` | Orbe da Lua Rubra | Lavine | Épico | Missão pessoal da Lavine | — |
| `visual_arma_lavine_005` | Grimório do Eclipse Eterno | Lavine | Lendário | Conquista: Coragem da Caçula | — |

## Conjuntos e bônus

Os números são propostas para o próximo passe de balanceamento (Docs/Balance) e usam só stats que já funcionam (`StatType`).

| Primo | Conjunto | Nível de raridade | Nível | Bônus |
|---|---|---|---|---|
| Maithe | Batedora da Mata | Comum | 1–10 | 2 peças: +2% vida máx. · 4 peças: +3% vigor máx. |
| Maithe | Rastreadora do Riacho | Incomum | 10–20 | 2: +3% vida máx. · 4: +4% regeneração de vigor |
| Maithe | Pena de Gavião | Raro | 20–30 | 2: +2 no atributo principal · 4: +3% dano da habilidade de assinatura |
| Maithe | Olho do Vento | Épico | 30–40 | 2: +3 no atributo principal · 4: +5% dano da habilidade de assinatura e rastro visual |
| Maithe | Tempestade de Mil Flechas | Lendário | 40–50 | 2: +4 no atributo principal · 4: +3% redução de recarga da Ultimate e aura visual |
| Maithe | Cantora do Bema | Festivo | qualquer | só visual: 4 peças = animação de comemoração própria (roda do hino, dança das fitas, palmas de lanterna) |
| Maria | Ajudante do Templo | Comum | 1–10 | 2 peças: +2% vida máx. · 4 peças: +3% vigor máx. |
| Maria | Curandeira Aprendiz | Incomum | 10–20 | 2: +3% vida máx. · 4: +4% regeneração de vigor |
| Maria | Luz de Candeia | Raro | 20–30 | 2: +2 no atributo principal · 4: +3% dano da habilidade de assinatura |
| Maria | Templo Solar | Épico | 30–40 | 2: +3 no atributo principal · 4: +5% dano da habilidade de assinatura e rastro visual |
| Maria | Milagre Celestial | Lendário | 40–50 | 2: +4 no atributo principal · 4: +3% redução de recarga da Ultimate e aura visual |
| Maria | Portadora da Lâmpada | Festivo | qualquer | só visual: 4 peças = animação de comemoração própria (roda do hino, dança das fitas, palmas de lanterna) |
| Joaquim | Menino do Pomar | Comum | 1–10 | 2 peças: +2% vida máx. · 4 peças: +3% vigor máx. |
| Joaquim | Cavaleirinho da Estepe | Incomum | 10–20 | 2: +3% vida máx. · 4: +4% regeneração de vigor |
| Joaquim | Casca de Choupo | Raro | 20–30 | 2: +2 no atributo principal · 4: +3% dano da habilidade de assinatura |
| Joaquim | Muralha de Pedra | Épico | 30–40 | 2: +3 no atributo principal · 4: +5% dano da habilidade de assinatura e rastro visual |
| Joaquim | Última Resistência | Lendário | 40–50 | 2: +4 no atributo principal · 4: +3% redução de recarga da Ultimate e aura visual |
| Joaquim | Dançarino das Fitas | Festivo | qualquer | só visual: 4 peças = animação de comemoração própria (roda do hino, dança das fitas, palmas de lanterna) |
| Ícaro | Aprendiz de Oficina | Comum | 1–10 | 2 peças: +2% vida máx. · 4 peças: +3% vigor máx. |
| Ícaro | Rádio Desmontado | Incomum | 10–20 | 2: +3% vida máx. · 4: +4% regeneração de vigor |
| Ícaro | Engrenagem de Latão | Raro | 20–30 | 2: +2 no atributo principal · 4: +3% dano da habilidade de assinatura |
| Ícaro | Campo Magnético | Épico | 30–40 | 2: +3 no atributo principal · 4: +5% dano da habilidade de assinatura e rastro visual |
| Ícaro | Exército Mecânico | Lendário | 40–50 | 2: +4 no atributo principal · 4: +3% redução de recarga da Ultimate e aura visual |
| Ícaro | Inventor do Bema | Festivo | qualquer | só visual: 4 peças = animação de comemoração própria (roda do hino, dança das fitas, palmas de lanterna) |
| Lavine | Caçulinha Curiosa | Comum | 1–10 | 2 peças: +2% vida máx. · 4 peças: +3% vigor máx. |
| Lavine | Contadora de Estrelas | Incomum | 10–20 | 2: +3% vida máx. · 4: +4% regeneração de vigor |
| Lavine | Sombra Macia | Raro | 20–30 | 2: +2 no atributo principal · 4: +3% dano da habilidade de assinatura |
| Lavine | Noite Rubra | Épico | 30–40 | 2: +3 no atributo principal · 4: +5% dano da habilidade de assinatura e rastro visual |
| Lavine | Eclipse Eterno | Lendário | 40–50 | 2: +4 no atributo principal · 4: +3% redução de recarga da Ultimate e aura visual |
| Lavine | Estrelinha das Lanternas | Festivo | qualquer | só visual: 4 peças = animação de comemoração própria (roda do hino, dança das fitas, palmas de lanterna) |
