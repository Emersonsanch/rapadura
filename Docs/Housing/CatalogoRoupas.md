# Catálogo de roupas, armaduras e aparência — Rapadura

> Design do guarda-roupa, slots e regras de aparência em [`Docs/Housing.md`](../Housing.md) §9. **Conjunto inicial de lançamento, não a lista de tudo o que pode existir:** novas peças entram por temporada/evento com ID novo.

**Total: 234 entradas** (120 peças de conjuntos dos primos, 28 roupas comuns a todos, 36 acessórios, 25 tinturas, 25 visuais de arma). Checagem automática: 234 IDs distintos, 234 nomes distintos (ignorando acento/maiúscula/pontuação), nenhuma duplicata.

| Raridade | Entradas |
|---|---|
| Comum | 63 |
| Incomum | 54 |
| Raro | 61 |
| Épico | 29 |
| Lendário | 27 |

## Como ler

- **Encaixe** usa os slots reais do `EquipmentSlot` (Cabeça = `Head`, Peito = `Chest`, Pernas = `Legs`, Pés = `Boots`, Acessório = `Accessory`) mais três encaixes **só de aparência** novos: Rosto/Pescoço, Costas e Caramelo (roupinha do cachorro).
- **Nível** é a faixa de nível do personagem (teto atual 50, `progression.csv`) em que a peça aparece. Os atributos de cada peça vêm do sistema atual de equipamento (item base + raridade + afixos procedurais); o conjunto acrescenta só o bônus de conjunto (tabela no fim).
- **Festivo** é cosmético puro (raridade Rara, sem atributos): pode ser usado por cima de qualquer peça pelo guarda-roupa.
- **Preço** em ouro só para loja; "—" não se compra; eventos em prendas.

## Maithe

| ID | Nome | Encaixe | Raridade | Nível | Origem | Preço (ouro) | Conjunto |
|---|---|---|---|---|---|---|---|
| `roupa_maithe_001` | Chapéu de palha de aba curta | Cabeça | Comum | 1–10 | Loja: Costureira Dona Fátima | 21 | Batedora da Mata |
| `roupa_maithe_002` | Gibão de algodão cru | Peito | Comum | 1–10 | Loja: Costureira Dona Fátima | 23 | Batedora da Mata |
| `roupa_maithe_003` | Calça de brim remendada | Pernas | Comum | 1–10 | Loja: Costureira Dona Fátima | 15 | Batedora da Mata |
| `roupa_maithe_004` | Alpargatas de corda | Pés | Comum | 1–10 | Loja: Armazém Geral | 20 | Batedora da Mata |
| `roupa_maithe_005` | Lenço de cabeça verde-musgo | Cabeça | Incomum | 10–20 | Loja: Costureira Dona Fátima | 30 | Rastreadora do Riacho |
| `roupa_maithe_006` | Colete de couro com porta-flechas | Peito | Incomum | 10–20 | Drop: inimigos comuns da Floresta e do Deserto | — | Rastreadora do Riacho |
| `roupa_maithe_007` | Bermuda de lona de trilha | Pernas | Incomum | 10–20 | Loja: Costureira Dona Fátima | 70 | Rastreadora do Riacho |
| `roupa_maithe_008` | Botina de cano curto da trilha | Pés | Incomum | 10–20 | Missão regional da Floresta (2ª) | — | Rastreadora do Riacho |
| `roupa_maithe_009` | Tiara de penas de gavião | Cabeça | Raro | 20–30 | Loja: Costureira Dona Fátima | 140 | Pena de Gavião |
| `roupa_maithe_010` | Túnica de linho verde bordada de vento | Peito | Raro | 20–30 | Drop: Lobo Alfa (chefe) | — | Pena de Gavião |
| `roupa_maithe_011` | Calça justa de caçadora | Pernas | Raro | 20–30 | Loja: Costureira Dona Fátima | 120 | Pena de Gavião |
| `roupa_maithe_012` | Botas de couro macio de passo leve | Pés | Raro | 20–30 | Diária: Ronda da Manhã | — | Pena de Gavião |
| `roupa_maithe_013` | Capuz do Olho do Vento | Cabeça | Épico | 30–40 | Missão pessoal da Maithe | — | Olho do Vento |
| `roupa_maithe_014` | Manto de correnteza de ar | Peito | Épico | 30–40 | Drop: Lobo Alfa (chefe) | — | Olho do Vento |
| `roupa_maithe_015` | Perneiras de vento trançado | Pernas | Épico | 30–40 | Crafting: Tear (tecido + material de chefe) | — | Olho do Vento |
| `roupa_maithe_016` | Sandálias aladas de tira | Pés | Épico | 30–40 | Drop: Senhor da Guerra da Montanha (chefe) | — | Olho do Vento |
| `roupa_maithe_017` | Coroa de nuvem de tempestade | Cabeça | Lendário | 40–50 | Conquista: Olho do Vento | — | Tempestade de Mil Flechas |
| `roupa_maithe_018` | Couraça de penas de trovão | Peito | Lendário | 40–50 | Drop: Habitante Ancestral (chefe) | — | Tempestade de Mil Flechas |
| `roupa_maithe_019` | Saia-calça de relâmpago | Pernas | Lendário | 40–50 | Crafting: Tear de mestre (4 lembranças de chefe) | — | Tempestade de Mil Flechas |
| `roupa_maithe_020` | Botas do vendaval | Pés | Lendário | 40–50 | Conquista: Acender de Novo (Nova Jornada+) | — | Tempestade de Mil Flechas |
| `roupa_maithe_021` | Chapéu de palha com flores do campo | Cabeça | Raro | qualquer | Evento: Festa Junina | — (30 prendas) | Rainha do Arraiá da Mata |
| `roupa_maithe_022` | Vestido xadrez verde de babados | Peito | Raro | qualquer | Evento: Festa Junina | — (40 prendas) | Rainha do Arraiá da Mata |
| `roupa_maithe_023` | Anágua de renda de quadrilha | Pernas | Raro | qualquer | Evento: Festa Junina | — (30 prendas) | Rainha do Arraiá da Mata |
| `roupa_maithe_024` | Sapatilha de pano florida | Pés | Raro | qualquer | Evento: Festa Junina | — (25 prendas) | Rainha do Arraiá da Mata |

## Maria

| ID | Nome | Encaixe | Raridade | Nível | Origem | Preço (ouro) | Conjunto |
|---|---|---|---|---|---|---|---|
| `roupa_maria_001` | Touca de linho branco | Cabeça | Comum | 1–10 | Loja: Costureira Dona Fátima | 11 | Ajudante do Templo |
| `roupa_maria_002` | Bata de algodão alvejado | Peito | Comum | 1–10 | Loja: Costureira Dona Fátima | 24 | Ajudante do Templo |
| `roupa_maria_003` | Saia de linho cru | Pernas | Comum | 1–10 | Loja: Costureira Dona Fátima | 24 | Ajudante do Templo |
| `roupa_maria_004` | Sandálias de couro cru | Pés | Comum | 1–10 | Loja: Armazém Geral | 20 | Ajudante do Templo |
| `roupa_maria_005` | Véu de renda de bilro | Cabeça | Incomum | 10–20 | Loja: Costureira Dona Fátima | 40 | Rezadeira Aprendiz |
| `roupa_maria_006` | Xale de fuxico dourado | Peito | Incomum | 10–20 | Drop: inimigos comuns da Floresta e do Deserto | — | Rezadeira Aprendiz |
| `roupa_maria_007` | Saia rodada de chita amarela | Pernas | Incomum | 10–20 | Loja: Costureira Dona Fátima | 60 | Rezadeira Aprendiz |
| `roupa_maria_008` | Chinelo de couro trançado | Pés | Incomum | 10–20 | Missão regional da Floresta (2ª) | — | Rezadeira Aprendiz |
| `roupa_maria_009` | Diadema de latão polido | Cabeça | Raro | 20–30 | Loja: Costureira Dona Fátima | 145 | Luz de Candeia |
| `roupa_maria_010` | Túnica de cetim dourado | Peito | Raro | 20–30 | Drop: Senhor da Guerra da Montanha (chefe) | — | Luz de Candeia |
| `roupa_maria_011` | Saia de linho com barrado de sol | Pernas | Raro | 20–30 | Loja: Costureira Dona Fátima | 90 | Luz de Candeia |
| `roupa_maria_012` | Botinhas de camurça clara | Pés | Raro | 20–30 | Diária: A Cesta de Ervas da Maria | — | Luz de Candeia |
| `roupa_maria_013` | Auréola de sol nascente | Cabeça | Épico | 30–40 | Missão pessoal da Maria | — | Templo Solar |
| `roupa_maria_014` | Manto do Templo Solar | Peito | Épico | 30–40 | Drop: Senhor da Guerra da Montanha (chefe) | — | Templo Solar |
| `roupa_maria_015` | Saia de raios dourados | Pernas | Épico | 30–40 | Crafting: Tear (tecido + material de chefe) | — | Templo Solar |
| `roupa_maria_016` | Sandálias de luz de vela | Pés | Épico | 30–40 | Drop: Senhor da Guerra da Montanha (chefe) | — | Templo Solar |
| `roupa_maria_017` | Coroa de estrelas da manhã | Cabeça | Lendário | 40–50 | Conquista: Luz que Não se Apaga | — | Milagre Celestial |
| `roupa_maria_018` | Vestes do Milagre Celestial | Peito | Lendário | 40–50 | Drop: Habitante Ancestral (chefe) | — | Milagre Celestial |
| `roupa_maria_019` | Saia de aurora | Pernas | Lendário | 40–50 | Crafting: Tear de mestre (4 lembranças de chefe) | — | Milagre Celestial |
| `roupa_maria_020` | Passos de luz celestial | Pés | Lendário | 40–50 | Conquista: Acender de Novo (Nova Jornada+) | — | Milagre Celestial |
| `roupa_maria_021` | Chapéu de pastorinha com fita | Cabeça | Raro | qualquer | Evento: Natal Sertanejo | — (30 prendas) | Pastorinha do Cordão Azul |
| `roupa_maria_022` | Corpete azul de pastoril | Peito | Raro | qualquer | Evento: Natal Sertanejo | — (40 prendas) | Pastorinha do Cordão Azul |
| `roupa_maria_023` | Saia de tule azul | Pernas | Raro | qualquer | Evento: Natal Sertanejo | — (30 prendas) | Pastorinha do Cordão Azul |
| `roupa_maria_024` | Sapatilha de cetim azul | Pés | Raro | qualquer | Evento: Natal Sertanejo | — (25 prendas) | Pastorinha do Cordão Azul |

## Joaquim

| ID | Nome | Encaixe | Raridade | Nível | Origem | Preço (ouro) | Conjunto |
|---|---|---|---|---|---|---|---|
| `roupa_joaquim_001` | Chapéu de couro surrado | Cabeça | Comum | 1–10 | Loja: Costureira Dona Fátima | 11 | Guri da Roça |
| `roupa_joaquim_002` | Camisa de flanela xadrez | Peito | Comum | 1–10 | Loja: Costureira Dona Fátima | 19 | Guri da Roça |
| `roupa_joaquim_003` | Calça de brim com joelheira | Pernas | Comum | 1–10 | Loja: Costureira Dona Fátima | 22 | Guri da Roça |
| `roupa_joaquim_004` | Botina de roça | Pés | Comum | 1–10 | Loja: Armazém Geral | 20 | Guri da Roça |
| `roupa_joaquim_005` | Chapéu de vaqueiro com barbicacho | Cabeça | Incomum | 10–20 | Loja: Forja do Mestre Tonho | 30 | Vaqueirinho |
| `roupa_joaquim_006` | Gibão de couro de vaqueiro | Peito | Incomum | 10–20 | Drop: inimigos comuns da Floresta e do Deserto | — | Vaqueirinho |
| `roupa_joaquim_007` | Perneiras de couro cru | Pernas | Incomum | 10–20 | Loja: Forja do Mestre Tonho | 55 | Vaqueirinho |
| `roupa_joaquim_008` | Botas de vaqueiro sem espora | Pés | Incomum | 10–20 | Missão regional da Floresta (2ª) | — | Vaqueirinho |
| `roupa_joaquim_009` | Elmo de casca de ipê | Cabeça | Raro | 20–30 | Loja: Forja do Mestre Tonho | 140 | Casca de Ipê |
| `roupa_joaquim_010` | Peitoral de tábuas de ipê | Peito | Raro | 20–30 | Drop: Guardião da Ruína (chefe) | — | Casca de Ipê |
| `roupa_joaquim_011` | Coxotes de madeira de lei | Pernas | Raro | 20–30 | Loja: Forja do Mestre Tonho | 195 | Casca de Ipê |
| `roupa_joaquim_012` | Botas de raiz batida | Pés | Raro | 20–30 | Diária: Vigia dos Escorpiões | — | Casca de Ipê |
| `roupa_joaquim_013` | Elmo de granito polido | Cabeça | Épico | 30–40 | Missão pessoal do Joaquim | — | Muralha de Pedra |
| `roupa_joaquim_014` | Couraça de pedra encaixada | Peito | Épico | 30–40 | Drop: Guardião da Ruína (chefe) | — | Muralha de Pedra |
| `roupa_joaquim_015` | Grevas de laje | Pernas | Épico | 30–40 | Crafting: Forja (lingotes + material de chefe) | — | Muralha de Pedra |
| `roupa_joaquim_016` | Botas de pedra-sabão | Pés | Épico | 30–40 | Drop: Senhor da Guerra da Montanha (chefe) | — | Muralha de Pedra |
| `roupa_joaquim_017` | Elmo da Raiz Eterna | Cabeça | Lendário | 40–50 | Conquista: Ecos de Casa | — | Última Resistência |
| `roupa_joaquim_018` | Armadura das Raízes Eternas | Peito | Lendário | 40–50 | Drop: Habitante Ancestral (chefe) | — | Última Resistência |
| `roupa_joaquim_019` | Grevas do tronco milenar | Pernas | Lendário | 40–50 | Crafting: Tear de mestre (4 lembranças de chefe) | — | Última Resistência |
| `roupa_joaquim_020` | Botas que não arredam pé | Pés | Lendário | 40–50 | Conquista: Acender de Novo (Nova Jornada+) | — | Última Resistência |
| `roupa_joaquim_021` | Chapéu de fitas de caboclo | Cabeça | Raro | qualquer | Evento: Carnaval | — (30 prendas) | Caboclo de Lança Mirim |
| `roupa_joaquim_022` | Gola de lantejoulas | Peito | Raro | qualquer | Evento: Carnaval | — (40 prendas) | Caboclo de Lança Mirim |
| `roupa_joaquim_023` | Calça de cetim listrada | Pernas | Raro | qualquer | Evento: Carnaval | — (30 prendas) | Caboclo de Lança Mirim |
| `roupa_joaquim_024` | Tênis enfeitado de fitas | Pés | Raro | qualquer | Evento: Carnaval | — (25 prendas) | Caboclo de Lança Mirim |

## Ícaro

| ID | Nome | Encaixe | Raridade | Nível | Origem | Preço (ouro) | Conjunto |
|---|---|---|---|---|---|---|---|
| `roupa_icaro_001` | Boné de mecânico | Cabeça | Comum | 1–10 | Loja: Costureira Dona Fátima | 24 | Aprendiz de Oficina |
| `roupa_icaro_002` | Macacão de brim manchado de graxa | Peito | Comum | 1–10 | Loja: Costureira Dona Fátima | 20 | Aprendiz de Oficina |
| `roupa_icaro_003` | Bermuda de bolsos de ferramenta | Pernas | Comum | 1–10 | Loja: Costureira Dona Fátima | 25 | Aprendiz de Oficina |
| `roupa_icaro_004` | Tênis de lona remendado | Pés | Comum | 1–10 | Loja: Armazém Geral | 13 | Aprendiz de Oficina |
| `roupa_icaro_005` | Fones de rádio de lata | Cabeça | Incomum | 10–20 | Loja: Forja do Mestre Tonho | 60 | Rádio Desmontado |
| `roupa_icaro_006` | Colete de fios coloridos | Peito | Incomum | 10–20 | Drop: inimigos comuns da Floresta e do Deserto | — | Rádio Desmontado |
| `roupa_icaro_007` | Calça cargo de antenas | Pernas | Incomum | 10–20 | Loja: Forja do Mestre Tonho | 60 | Rádio Desmontado |
| `roupa_icaro_008` | Botas com sola de pneu | Pés | Incomum | 10–20 | Missão regional da Floresta (2ª) | — | Rádio Desmontado |
| `roupa_icaro_009` | Óculos de solda de latão | Cabeça | Raro | 20–30 | Loja: Forja do Mestre Tonho | 170 | Engrenagem de Latão |
| `roupa_icaro_010` | Jaqueta de engrenagens | Peito | Raro | 20–30 | Drop: Guardião da Ruína (chefe) | — | Engrenagem de Latão |
| `roupa_icaro_011` | Calça de couro com rebites | Pernas | Raro | 20–30 | Loja: Forja do Mestre Tonho | 135 | Engrenagem de Latão |
| `roupa_icaro_012` | Botas de mola | Pés | Raro | 20–30 | Diária: Peças de Reposição | — | Engrenagem de Latão |
| `roupa_icaro_013` | Capacete de bobina de cobre | Cabeça | Épico | 30–40 | Missão pessoal do Ícaro | — | Campo Magnético |
| `roupa_icaro_014` | Peitoral de ímãs giratórios | Peito | Épico | 30–40 | Drop: Guardião da Ruína (chefe) | — | Campo Magnético |
| `roupa_icaro_015` | Calça de malha condutora | Pernas | Épico | 30–40 | Crafting: Forja (lingotes + material de chefe) | — | Campo Magnético |
| `roupa_icaro_016` | Botas de levitação magnética | Pés | Épico | 30–40 | Drop: Senhor da Guerra da Montanha (chefe) | — | Campo Magnético |
| `roupa_icaro_017` | Elmo do Comandante das Máquinas | Cabeça | Lendário | 40–50 | Conquista: Engrenagens Antigas | — | Exército Mecânico |
| `roupa_icaro_018` | Exoarmadura de cobre e vapor | Peito | Lendário | 40–50 | Drop: Habitante Ancestral (chefe) | — | Exército Mecânico |
| `roupa_icaro_019` | Pernas mecânicas assistidas | Pernas | Lendário | 40–50 | Crafting: Tear de mestre (4 lembranças de chefe) | — | Exército Mecânico |
| `roupa_icaro_020` | Botas a jato de rapadura | Pés | Lendário | 40–50 | Conquista: Acender de Novo (Nova Jornada+) | — | Exército Mecânico |
| `roupa_icaro_021` | Chapéu de palha com antena | Cabeça | Raro | qualquer | Evento: Festa Junina | — (30 prendas) | Inventor do Arraiá |
| `roupa_icaro_022` | Camisa xadrez de lampadinhas | Peito | Raro | qualquer | Evento: Festa Junina | — (40 prendas) | Inventor do Arraiá |
| `roupa_icaro_023` | Calça com remendo de circuito | Pernas | Raro | qualquer | Evento: Festa Junina | — (30 prendas) | Inventor do Arraiá |
| `roupa_icaro_024` | Botina de pisca-pisca | Pés | Raro | qualquer | Evento: Festa Junina | — (25 prendas) | Inventor do Arraiá |

## Lavine

| ID | Nome | Encaixe | Raridade | Nível | Origem | Preço (ouro) | Conjunto |
|---|---|---|---|---|---|---|---|
| `roupa_lavine_001` | Laço de fita roxa | Cabeça | Comum | 1–10 | Loja: Costureira Dona Fátima | 19 | Caçulinha Curiosa |
| `roupa_lavine_002` | Vestidinho de chita roxa | Peito | Comum | 1–10 | Loja: Costureira Dona Fátima | 11 | Caçulinha Curiosa |
| `roupa_lavine_003` | Meia-calça listrada | Pernas | Comum | 1–10 | Loja: Costureira Dona Fátima | 23 | Caçulinha Curiosa |
| `roupa_lavine_004` | Sapatinho de verniz | Pés | Comum | 1–10 | Loja: Armazém Geral | 21 | Caçulinha Curiosa |
| `roupa_lavine_005` | Chapéu pontudo de papel de seda | Cabeça | Incomum | 10–20 | Loja: Costureira Dona Fátima | 55 | Contadora de Estrelas |
| `roupa_lavine_006` | Capinha de veludo lilás | Peito | Incomum | 10–20 | Drop: inimigos comuns da Floresta e do Deserto | — | Contadora de Estrelas |
| `roupa_lavine_007` | Saia de estrelinhas | Pernas | Incomum | 10–20 | Loja: Costureira Dona Fátima | 65 | Contadora de Estrelas |
| `roupa_lavine_008` | Botinhas de feltro | Pés | Incomum | 10–20 | Missão regional da Floresta (2ª) | — | Contadora de Estrelas |
| `roupa_lavine_009` | Capuz de sombra macia | Cabeça | Raro | 20–30 | Loja: Costureira Dona Fátima | 90 | Sombra Macia |
| `roupa_lavine_010` | Túnica de crepúsculo | Peito | Raro | 20–30 | Drop: Habitante Ancestral (chefe) | — | Sombra Macia |
| `roupa_lavine_011` | Saia de névoa | Pernas | Raro | 20–30 | Loja: Costureira Dona Fátima | 150 | Sombra Macia |
| `roupa_lavine_012` | Pantufas silenciosas | Pés | Raro | 20–30 | Diária: A Lavine e os Morcegos | — | Sombra Macia |
| `roupa_lavine_013` | Tiara de lua vermelha | Cabeça | Épico | 30–40 | Missão pessoal da Lavine | — | Noite Rubra |
| `roupa_lavine_014` | Manto da Noite Rubra | Peito | Épico | 30–40 | Drop: Habitante Ancestral (chefe) | — | Noite Rubra |
| `roupa_lavine_015` | Saia de eclipse | Pernas | Épico | 30–40 | Crafting: Tear (tecido + material de chefe) | — | Noite Rubra |
| `roupa_lavine_016` | Botas de brasa fria | Pés | Épico | 30–40 | Drop: Senhor da Guerra da Montanha (chefe) | — | Noite Rubra |
| `roupa_lavine_017` | Coroa do Eclipse Eterno | Cabeça | Lendário | 40–50 | Conquista: Coragem da Caçula | — | Eclipse Eterno |
| `roupa_lavine_018` | Vestes da Lua Escondida | Peito | Lendário | 40–50 | Drop: Habitante Ancestral (chefe) | — | Eclipse Eterno |
| `roupa_lavine_019` | Saia do horizonte apagado | Pernas | Lendário | 40–50 | Crafting: Tear de mestre (4 lembranças de chefe) | — | Eclipse Eterno |
| `roupa_lavine_020` | Passos de meia-noite | Pés | Lendário | 40–50 | Conquista: Acender de Novo (Nova Jornada+) | — | Eclipse Eterno |
| `roupa_lavine_021` | Coroa de reisado com espelhinhos | Cabeça | Raro | qualquer | Evento: Natal Sertanejo | — (30 prendas) | Estrelinha do Reisado |
| `roupa_lavine_022` | Bata de reisado com fitas | Peito | Raro | qualquer | Evento: Natal Sertanejo | — (40 prendas) | Estrelinha do Reisado |
| `roupa_lavine_023` | Calça de cetim vermelho | Pernas | Raro | qualquer | Evento: Natal Sertanejo | — (30 prendas) | Estrelinha do Reisado |
| `roupa_lavine_024` | Sapatilha de lantejoulas | Pés | Raro | qualquer | Evento: Natal Sertanejo | — (25 prendas) | Estrelinha do Reisado |

## Roupas comuns a todos os primos

| ID | Nome | Encaixe | Raridade | Origem | Preço (ouro) |
|---|---|---|---|---|---|
| `roupa_comum_001` | Camisa de chita de manga curta | Peito | Comum | Loja: Costureira Dona Fátima | 24 |
| `roupa_comum_002` | Vestido de chita rodado | Peito | Comum | Loja: Costureira Dona Fátima | 11 |
| `roupa_comum_003` | Camiseta "Eu amo rapadura" | Peito | Comum | Loja: Armazém Geral | 23 |
| `roupa_comum_004` | Camisa do time de futebol da vila | Peito | Comum | Loja: Armazém Geral | 20 |
| `roupa_comum_005` | Jardineira jeans com bolso de canguru | Peito | Incomum | Loja: Costureira Dona Fátima | 45 |
| `roupa_comum_006` | Pijama de flanela de carneirinho | Peito | Incomum | Loja: Costureira Dona Fátima | 40 |
| `roupa_comum_007` | Capa de chuva amarela de botão | Peito | Comum | Loja: Armazém Geral | 25 |
| `roupa_comum_008` | Bata de renda renascença | Peito | Raro | Loja: Costureira Dona Fátima | 90 |
| `roupa_comum_009` | Gibão de couro de vaqueiro adulto, grande demais | Peito | Incomum | Loja: Mascate Seu Bento | 65 |
| `roupa_comum_010` | Suéter de tricô da Vó Zefa | Peito | Raro | Missão da Casa: Parede de Barro | — |
| `roupa_comum_011` | Camisa de festa de linho branco | Peito | Incomum | Loja: Costureira Dona Fátima | 60 |
| `roupa_comum_012` | Colete de tropeiro com bordado de estrada | Peito | Raro | Missão: Reserva pra Travessia | — |
| `roupa_comum_013` | Agasalho de lã da serra | Peito | Incomum | Loja: Forja do Mestre Tonho | 35 |
| `roupa_comum_014` | Avental de cozinha da Vó Zefa | Peito | Comum | Missão da Casa: Fogo na Cozinha | — |
| `roupa_comum_015` | Bermuda de tactel colorida | Pernas | Comum | Loja: Armazém Geral | 21 |
| `roupa_comum_016` | Calça de pijama listrada | Pernas | Comum | Loja: Costureira Dona Fátima | 20 |
| `roupa_comum_017` | Saia de chita com bolsos | Pernas | Comum | Loja: Costureira Dona Fátima | 22 |
| `roupa_comum_018` | Calça de brim de feira | Pernas | Comum | Loja: Feira livre | 11 |
| `roupa_comum_019` | Saia de quadrilha de babados | Pernas | Incomum | Evento: Festa Junina | — (20 prendas) |
| `roupa_comum_020` | Calça de lã xadrez da serra | Pernas | Incomum | Loja: Forja do Mestre Tonho | 55 |
| `roupa_comum_021` | Alpargatas de lona azul | Pés | Comum | Loja: Feira livre | 13 |
| `roupa_comum_022` | Chinelo de dedo de borracha | Pés | Comum | Loja: Armazém Geral | 17 |
| `roupa_comum_023` | Sandália de couro de rabicho | Pés | Incomum | Loja: Mascate Seu Bento | 60 |
| `roupa_comum_024` | Pantufa de cachorrinho caramelo | Pés | Raro | Conquista: Juntos, ou Nada Feito | — |
| `roupa_comum_025` | Galochas de chuva verdes | Pés | Comum | Loja: Armazém Geral | 18 |
| `roupa_comum_026` | Bota de couro do tropeiro | Pés | Raro | Drop: Guardião da Ruína (chefe) | — |
| `roupa_comum_027` | Tamanco de madeira da feira | Pés | Incomum | Loja: Feira livre | 65 |
| `roupa_comum_028` | Pés descalços sujos de terra | Pés | Comum | Desbloqueado desde o começo | — |

## Acessórios (inclui roupinhas do Caramelo)

| ID | Nome | Para | Encaixe | Raridade | Origem | Preço (ouro) |
|---|---|---|---|---|---|---|
| `acess_001` | Chapéu de couro de aba virada | Todos | Cabeça | Raro | Loja: Mascate Seu Bento | 90 |
| `acess_002` | Réplica do chapéu de couro do Vô Chico | Todos | Cabeça | Épico | Segredo do mapa (relíquia `relic_chapeu_couro` encontrada) | — |
| `acess_003` | Chapéu de meia-lua de couro | Todos | Cabeça | Raro | Drop: Guardião da Ruína (chefe) | — |
| `acess_004` | Chapéu de palha desfiado | Todos | Cabeça | Comum | Loja: Feira livre | 25 |
| `acess_005` | Boné virado para trás | Todos | Cabeça | Comum | Loja: Armazém Geral | 15 |
| `acess_006` | Coroa de flores de ipê | Todos | Cabeça | Incomum | Loja: Viveiro da Comadre Neném | 55 |
| `acess_007` | Tiara de flor de maracujá | Todos | Cabeça | Incomum | Loja: Viveiro da Comadre Neném | 55 |
| `acess_008` | Gorro de lã com pompom | Todos | Cabeça | Comum | Loja: Forja do Mestre Tonho | 19 |
| `acess_009` | Lenço de pescoço vermelho de vaqueiro | Todos | Rosto/Pescoço | Comum | Loja: Armarinho da Dona Cida | 25 |
| `acess_010` | Lenço de seda estampado de cajus | Todos | Rosto/Pescoço | Incomum | Loja: Armarinho da Dona Cida | 30 |
| `acess_011` | Óculos redondos de aro de arame | Todos | Rosto/Pescoço | Incomum | Loja: Mascate Seu Bento | 35 |
| `acess_012` | Colar de sementes de açaí | Todos | Rosto/Pescoço | Comum | Loja: Feira livre | 15 |
| `acess_013` | Colar de contas de cristal das cavernas | Todos | Rosto/Pescoço | Raro | Loja: Lamparinas da Benedita | 185 |
| `acess_014` | Medalhinha de latão da Vó Zefa | Todos | Rosto/Pescoço | Épico | Missão da Casa: Casa Cheia | — |
| `acess_015` | Bigode falso de quadrilha | Todos | Rosto/Pescoço | Comum | Evento: Festa Junina | — (10 prendas) |
| `acess_016` | Pintinhas de sarda pintadas a carvão | Todos | Rosto/Pescoço | Comum | Evento: Festa Junina | — (10 prendas) |
| `acess_017` | Máscara de papangu de papel machê | Todos | Rosto/Pescoço | Incomum | Evento: Carnaval | — (20 prendas) |
| `acess_018` | Bornal de couro cru | Todos | Costas | Comum | Loja: Costureira Dona Fátima | 23 |
| `acess_019` | Mochila de lona com remendos | Todos | Costas | Comum | Loja: Armazém Geral | 19 |
| `acess_020` | Cabaça-cantil a tiracolo | Todos | Costas | Incomum | Loja: Mascate Seu Bento | 35 |
| `acess_021` | Capa de super-herói de toalha de banho | Todos | Costas | Incomum | Conquista: Mão Amiga | — |
| `acess_022` | Asas de papel crepom | Todos | Costas | Raro | Evento: Carnaval | — (35 prendas) |
| `acess_023` | Sombrinha de frevo nas costas | Todos | Costas | Raro | Evento: Carnaval | — (35 prendas) |
| `acess_024` | Rede enrolada nas costas | Todos | Costas | Incomum | Loja: Costureira Dona Fátima | 55 |
| `acess_025` | Caramelo de pelúcia na mochila | Todos | Costas | Épico | Conquista: Prima de Todos | — |
| `acess_026` | Pulseira de fitinhas coloridas | Todos | Acessório | Comum | Loja: Armarinho da Dona Cida | 25 |
| `acess_027` | Relógio de pulso de corda | Todos | Acessório | Incomum | Loja: Mascate Seu Bento | 65 |
| `acess_028` | Anel de tampinha de garrafa | Todos | Acessório | Comum | Loja: Feira livre | 18 |
| `acess_029` | Broche de rapadura dourada | Todos | Acessório | Raro | Conquista: Bom de Negócio | — |
| `acess_030` | Lanterninha de pendurar no cinto | Todos | Acessório | Incomum | Loja: Lamparinas da Benedita | 40 |
| `acess_031` | Lenço vermelho de pescoço do Caramelo | Caramelo | Caramelo | Comum | Loja: Armarinho da Dona Cida | 22 |
| `acess_032` | Coleira de couro com plaquinha de nome | Caramelo | Caramelo | Comum | Loja: Armarinho da Dona Cida | 20 |
| `acess_033` | Chapeuzinho de palha do Caramelo | Caramelo | Caramelo | Incomum | Evento: Festa Junina | — (20 prendas) |
| `acess_034` | Gorrinho de Natal do Caramelo | Caramelo | Caramelo | Incomum | Evento: Natal Sertanejo | — (20 prendas) |
| `acess_035` | Capinha de chuva amarela do Caramelo | Caramelo | Caramelo | Incomum | Loja: Costureira Dona Fátima | 55 |
| `acess_036` | Bandana de engrenagens do Caramelo | Caramelo | Caramelo | Raro | Conquista: Vento e Engrenagem | — |

## Tinturas

Tinta é **consumível**: cada aplicação gasta 1 unidade e pinta uma peça (canal principal ou secundário). Ver Housing.md §9.4.

| ID | Nome | Cor | Raridade | Origem | Preço por unidade (ouro) |
|---|---|---|---|---|---|
| `tinta_001` | Tinta de anil | Azul | Comum | Loja: Armarinho da Dona Cida | 7 |
| `tinta_002` | Tinta de urucum | Vermelho-alaranjado | Comum | Loja: Feira livre | 5 |
| `tinta_003` | Tinta de açafrão-da-terra | Amarelo | Comum | Loja: Mercearia do Seu Lázaro | 5 |
| `tinta_004` | Tinta de jenipapo | Preto-azulado | Incomum | Loja: Viveiro da Comadre Neném | 16 |
| `tinta_005` | Tinta de barro vermelho | Terracota | Comum | Loja: Olaria da Dona Rosa | 5 |
| `tinta_006` | Tinta de carvão do fogão | Cinza-carvão | Comum | Crafting: Fogão a lenha | — |
| `tinta_007` | Tinta de casca de cajueiro | Marrom | Comum | Loja: Feira livre | 8 |
| `tinta_008` | Tinta de pau-brasil | Vermelho intenso | Raro | Loja: Mascate Seu Bento | 30 |
| `tinta_009` | Tinta de folha de mangueira | Verde-folha | Comum | Loja: Viveiro da Comadre Neném | 6 |
| `tinta_010` | Tinta de casca de cebola | Dourado-palha | Comum | Loja: Mercearia do Seu Lázaro | 7 |
| `tinta_011` | Tinta de beterraba | Rosa | Comum | Loja: Feira livre | 7 |
| `tinta_012` | Tinta de cal | Branco | Comum | Loja: Olaria da Dona Rosa | 5 |
| `tinta_013` | Tinta de ocre da serra | Ocre | Incomum | Loja: Forja do Mestre Tonho | 13 |
| `tinta_014` | Tinta de areia dourada | Dourado-areia | Incomum | Drop: Escorpião das Dunas | — |
| `tinta_015` | Tinta de cristal azul da caverna | Azul-cristal | Raro | Drop: Enxame de Morcegos | — |
| `tinta_016` | Tinta verde-musgo da mata | Verde-musgo | Incomum | Drop: Aranha da Floresta | — |
| `tinta_017` | Tinta azul de pastoril | Azul-celeste | Incomum | Evento: Natal Sertanejo | — (20 prendas) |
| `tinta_018` | Tinta rosa de quadrilha | Rosa-choque | Incomum | Evento: Festa Junina | — (20 prendas) |
| `tinta_019` | Tinta de cobre polido | Cobre | Raro | Crafting: Forja | — |
| `tinta_020` | Tinta lilás de crepúsculo | Lilás | Raro | Loja: Lamparinas da Benedita | 36 |
| `tinta_021` | Tinta cinza de pedra | Cinza-pedra | Comum | Drop: Golem de Pedra | — |
| `tinta_022` | Tinta preta da noite | Preto | Incomum | Loja: Lamparinas da Benedita | 21 |
| `tinta_023` | Tintura arco-íris de chita | Estampa de chita (multicor) | Épico | Evento: Carnaval | — (70 prendas) |
| `tinta_024` | Tinta da Lua Rubra | Vermelho-escuro com brilho | Lendário | Drop: Habitante Ancestral (chefe) | — |
| `tinta_025` | Tinta do sol nascente | Dourado brilhante | Lendário | Conquista: A Luz Volta para Casa | — |

## Visuais de arma (skins)

Só aparência: aplicados no guarda-roupa sobre a arma equipada do mesmo tipo (arco da Maithe, cajado da Maria, escudo do Joaquim, ferramenta do Ícaro, grimório/orbe da Lavine).

| ID | Nome | Primo | Raridade | Origem | Preço (ouro) |
|---|---|---|---|---|---|
| `visual_arma_maithe_001` | Arco de bambu de pipa | Maithe | Comum | Loja: Feira livre | 21 |
| `visual_arma_maithe_002` | Arco de galho de goiabeira | Maithe | Incomum | Loja: Marcenaria do Mestre Damião | 70 |
| `visual_arma_maithe_003` | Arco de mandacaru florido | Maithe | Raro | Drop: Escorpião das Dunas | — |
| `visual_arma_maithe_004` | Arco do vento cantante | Maithe | Épico | Missão pessoal da Maithe | — |
| `visual_arma_maithe_005` | Arco do Cristal do Sol | Maithe | Lendário | Drop: Habitante Ancestral (chefe) | — |
| `visual_arma_maria_001` | Cajado de galho de arruda | Maria | Comum | Loja: Viveiro da Comadre Neném | 22 |
| `visual_arma_maria_002` | Cajado de candeeiro | Maria | Incomum | Loja: Lamparinas da Benedita | 45 |
| `visual_arma_maria_003` | Cajado de girassol | Maria | Raro | Diária: A Cesta de Ervas da Maria | — |
| `visual_arma_maria_004` | Cajado de vitral | Maria | Épico | Missão pessoal da Maria | — |
| `visual_arma_maria_005` | Cajado da aurora | Maria | Lendário | Conquista: Luz que Não se Apaga | — |
| `visual_arma_joaquim_001` | Escudo de tampa de panela | Joaquim | Comum | Loja: Mercearia do Seu Lázaro | 25 |
| `visual_arma_joaquim_002` | Escudo de roda de carroça | Joaquim | Incomum | Loja: Marcenaria do Mestre Damião | 65 |
| `visual_arma_joaquim_003` | Escudo de mandala de cipó | Joaquim | Raro | Drop: Aranha da Floresta | — |
| `visual_arma_joaquim_004` | Escudo de pedra-sabão entalhada | Joaquim | Épico | Drop: Senhor da Guerra da Montanha (chefe) | — |
| `visual_arma_joaquim_005` | Escudo dourado da Raiz Eterna | Joaquim | Lendário | Conquista: Ecos de Casa | — |
| `visual_arma_icaro_001` | Chave inglesa de brinquedo | Ícaro | Comum | Loja: Armazém Geral | 19 |
| `visual_arma_icaro_002` | Lançador de garrafa de refrigerante | Ícaro | Incomum | Crafting: Bancada | — |
| `visual_arma_icaro_003` | Chave de engrenagem de latão | Ícaro | Raro | Drop: Guardião da Ruína (chefe) | — |
| `visual_arma_icaro_004` | Bobina elétrica de lata | Ícaro | Épico | Missão pessoal do Ícaro | — |
| `visual_arma_icaro_005` | Canhão de confete de rapadura | Ícaro | Lendário | Evento: Carnaval (ranking do evento) | — |
| `visual_arma_lavine_001` | Grimório de caderno de escola | Lavine | Comum | Loja: Armarinho da Dona Cida | 11 |
| `visual_arma_lavine_002` | Orbe de bolha de sabão | Lavine | Incomum | Loja: Feira livre | 50 |
| `visual_arma_lavine_003` | Livro de cordel encantado | Lavine | Raro | Diária: A Lavine e os Morcegos | — |
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
| Maithe | Rainha do Arraiá da Mata | Festivo | qualquer | só visual: 4 peças = animação de comemoração própria (passinho de quadrilha, frevo, reisado) |
| Maria | Ajudante do Templo | Comum | 1–10 | 2 peças: +2% vida máx. · 4 peças: +3% vigor máx. |
| Maria | Rezadeira Aprendiz | Incomum | 10–20 | 2: +3% vida máx. · 4: +4% regeneração de vigor |
| Maria | Luz de Candeia | Raro | 20–30 | 2: +2 no atributo principal · 4: +3% dano da habilidade de assinatura |
| Maria | Templo Solar | Épico | 30–40 | 2: +3 no atributo principal · 4: +5% dano da habilidade de assinatura e rastro visual |
| Maria | Milagre Celestial | Lendário | 40–50 | 2: +4 no atributo principal · 4: +3% redução de recarga da Ultimate e aura visual |
| Maria | Pastorinha do Cordão Azul | Festivo | qualquer | só visual: 4 peças = animação de comemoração própria (passinho de quadrilha, frevo, reisado) |
| Joaquim | Guri da Roça | Comum | 1–10 | 2 peças: +2% vida máx. · 4 peças: +3% vigor máx. |
| Joaquim | Vaqueirinho | Incomum | 10–20 | 2: +3% vida máx. · 4: +4% regeneração de vigor |
| Joaquim | Casca de Ipê | Raro | 20–30 | 2: +2 no atributo principal · 4: +3% dano da habilidade de assinatura |
| Joaquim | Muralha de Pedra | Épico | 30–40 | 2: +3 no atributo principal · 4: +5% dano da habilidade de assinatura e rastro visual |
| Joaquim | Última Resistência | Lendário | 40–50 | 2: +4 no atributo principal · 4: +3% redução de recarga da Ultimate e aura visual |
| Joaquim | Caboclo de Lança Mirim | Festivo | qualquer | só visual: 4 peças = animação de comemoração própria (passinho de quadrilha, frevo, reisado) |
| Ícaro | Aprendiz de Oficina | Comum | 1–10 | 2 peças: +2% vida máx. · 4 peças: +3% vigor máx. |
| Ícaro | Rádio Desmontado | Incomum | 10–20 | 2: +3% vida máx. · 4: +4% regeneração de vigor |
| Ícaro | Engrenagem de Latão | Raro | 20–30 | 2: +2 no atributo principal · 4: +3% dano da habilidade de assinatura |
| Ícaro | Campo Magnético | Épico | 30–40 | 2: +3 no atributo principal · 4: +5% dano da habilidade de assinatura e rastro visual |
| Ícaro | Exército Mecânico | Lendário | 40–50 | 2: +4 no atributo principal · 4: +3% redução de recarga da Ultimate e aura visual |
| Ícaro | Inventor do Arraiá | Festivo | qualquer | só visual: 4 peças = animação de comemoração própria (passinho de quadrilha, frevo, reisado) |
| Lavine | Caçulinha Curiosa | Comum | 1–10 | 2 peças: +2% vida máx. · 4 peças: +3% vigor máx. |
| Lavine | Contadora de Estrelas | Incomum | 10–20 | 2: +3% vida máx. · 4: +4% regeneração de vigor |
| Lavine | Sombra Macia | Raro | 20–30 | 2: +2 no atributo principal · 4: +3% dano da habilidade de assinatura |
| Lavine | Noite Rubra | Épico | 30–40 | 2: +3 no atributo principal · 4: +5% dano da habilidade de assinatura e rastro visual |
| Lavine | Eclipse Eterno | Lendário | 40–50 | 2: +4 no atributo principal · 4: +3% redução de recarga da Ultimate e aura visual |
| Lavine | Estrelinha do Reisado | Festivo | qualquer | só visual: 4 peças = animação de comemoração própria (passinho de quadrilha, frevo, reisado) |
