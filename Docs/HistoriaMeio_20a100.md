# A Lua que Não Enche — história principal do nível ~20 ao 100

> Cadeia principal que liga o fim da campanha (Ato V: Tages e o selo, nível ~20) à primeira das **Cartas da Estrada**
> (`Docs/HistoriaFinal_Cartas.md`, Soslan, nível 100). Dados: `Assets/Scripts/Gameplay/Quests/MidStoryCatalog.cs`; textos:
> `Assets/Scripts/Gameplay/Quests/MidStoryTexts.cs` (en + pt-BR, carregados junto com os das Cartas); testes:
> `Assets/Tests/EditMode/MidStoryCatalogTests.cs`. Tom, vozes e proibições: `Docs/Narrative.md` e `Docs/WorldBible_Maniqueismo.md`.

## O arco

No mito do Vô Jorge, a **Barca da Lua** junta a luz libertada (por isso a lua cresce), passa para a **Barca do Sol**, e o
Sol leva tudo para casa. Depois do selo refeito, toda faísca solta devia subir e a lua devia engordar noite após noite. A Vó
Edna olha do iwan há uma semana e **a lua continua uma unha**.

- **Região 4 (as Grutas):** os primos descem para perguntar ao Tages, que agora é vizinho com medo do escuro. Ele conta que,
  nas estradas de longe, ainda há bichos com a **fome velha da Āz** que pegam a luz no caminho para a Lua, e diz quatro lugares.
- **Segundo anel das Regiões 1–4 (nv 24–40):** o jardim de Mihrshah, o trono de sal do Namrael, o cume dos Mazans e os
  aquedutos profundos. A cada lugar a lua "engorda um fio", e a Vó confere duas vezes. Nos aquedutos a água leva parte das
  faíscas para o mar: o Takhsich paga a passagem de dhow.
- **Região 5 (Costa das Monções):** gaivotas ladras, babuínos que guardam faíscas no oco dos baobás e crocodilos do mangue
  (o Hedammu dorme mais para dentro: "deixem ele dormir"). A lua passa da metade; o resto da luz vai rio acima com a chuva.
- **Região 6 (Delta de Jhilpur):** pítons enroladas na chuva das cascatas e trolls que embolsam faíscas na mata morta, perto
  do Coração do Lodo (o Lodo-que-Engole fica "para outro dia"). Falta uma fatia da lua, e ela aponta para o norte.
- **Região 7 (Teto do Mundo):** ursos dormindo em montes de faísca nos zimbros e os almas que empilharam a luz das montanhas
  nos picos gelados. Do alto se vê, a leste, a planície dos nove anéis. O Aydahar dorme no passo: os primos dão a volta.
- **Região 8 (Mingcheng):** os leões de pedra do caminho dos espíritos viraram a cabeça e mordem a luz; as tartarugas de pedra
  travam a **Eclusa da Lua**; e nos fossos de terracota os soldados de barro guardam as últimas faíscas. Quando caem, **a lua
  enche**. A Vó Edna fica um tempão calada no rádio e depois conta que a luz da lua caiu no Caderno, bem no forro, e que o forro
  tem **uma costura que não é dela: a do Vô Jorge**. É dali que sai o bilhete da primeira carta ("No forro do Caderno, a Vó Edna
  achou um bilhete…"), e a cadeia passa para o Soslan.

O fio de cada etapa é o mesmo das Cartas, de propósito: alguém da cidade pede, os primos vão a uma zona do mundo contínuo e
"soltam" os bichos apagados (as faíscas sobem), e a Vó Edna comenta pelo radinho, sempre com uma lembrança do Vô Jorge. Os
chefes do mundo das Regiões 5–7 (Hedammu, Lodo-que-Engole, Aydahar) só são **citados** como perigo ao lado: chefe é luta de uma
vez, e a cadeia não depende de vencê-los.

## As etapas

Toda etapa exige a anterior e um nível maior; o doador só entrega a partir do nível da tabela. Local = zona do mundo contínuo
(objetivo ReachLocation, relatado por `LateStoryLocationReporter`); inimigo = `enemy_*` não-chefe da fauna dessa zona.

| # | Região | Etapa | Quem entrega (cidade) | Local (mapa) | Nível | Objetivo | Recompensa (além de XP e ouro) |
|---|---|---|---|---|---|---|---|
| 1 | 4 Grutas do Limiar | A Lua que Não Enche | Vó Edna (Vila) | Salão de Tages (20) | 20 | o selo com o Tages (creditado se ele já caiu) | 3 poções de vida e 3 de mana |
| 2 | 1 Bosque do Zarafshan | Faíscas no Jardim | Gara (Copas de Garajonay) | Jardim de Mihrshah (06) | 24 | 6 golens de pedra | teriaga (veneno do trono) |
| 3 | 2 Areias Flamejantes | O Trono de Sal | Ashmunikal (Tambor Rubro) | Trono de Namrael (12) | 30 | 6 lanternas quebradas | chá de gengibre (frio do cume) |
| 4 | 3 Serra de Kaggen | Névoa no Cume | Napirasu (Salões de Bronze) | Cume dos Mazans (18) | 36 | 6 névoas do Vento Ruim | óleo do fogo eterno |
| 5 | 4 Grutas do Limiar | Os Aquedutos Profundos | Takhsich (Mercado dos Túneis) | Aquedutos Profundos (23) | 40 | 6 peixes-lanterna | passagem de dhow |
| 6 | 5 Costa das Monções | Ladrões na Praia | Nuru (Taa Pwani) | Praia dos Dhows (25) | 46 | 7 gaivotas ladras | concha de respirar |
| 7 | 5 | Os Baobás Ocos | Ensuhkeshdanna (Aratta) | Bosque dos Baobás (26) | 52 | 6 babuínos | botas de raiz |
| 8 | 5 | Dentes no Mangue | Capitão Liyongo (Taa Pwani) | Mangue Raso (30) | 60 | 6 crocodilos do mangue | vara de equilíbrio |
| 9 | 6 Delta de Jhilpur | Anéis na Chuva | Salmaios (Jhilpur) | Cascatas da Chuva (32) | 64 | 6 pítons apagadas | amuleto antiveneno |
| 10 | 6 | A Mata Morta | Ramtha (Palafitas de Tarchon) | Mata Morta (35) | 74 | 5 trolls apagados | roupa térmica (frio do Teto) |
| 11 | 7 Teto do Mundo | Ursos nos Zimbros | Abā (Sangkent) | Encosta dos Zimbros (37) | 78 | 6 ursos-pardos | grampos de escalada |
| 12 | 7 | O Quinto Vale | Baraies (Sangkent) | Picos Gelados (38) | 86 | 5 almas | capa de tempestade |
| 13 | 8 Planície de Mingcheng | O Caminho dos Espíritos | Bögü (Mingcheng) | Caminho dos Leões (41) | 90 | 6 leões de pedra | selo do sol |
| 14 | 8 | A Eclusa da Lua | Lin Deng (Mingcheng) | Eclusa da Lua (43) | 94 | 5 tartarugas de pedra | lamparina rúnica |
| 15 | 8 | A Lua Cheia | Fuduodan (Mingcheng) | Fossos de Terracota (41) | 97 | 6 soldados de terracota | 5 poções de vida e 5 de mana |

XP de 300 a 2150 (crescente, abaixo dos 2300 da primeira carta) e 40 a 180 de ouro. Os itens são de `Resources/Items`, quase
todos o que a etapa seguinte, ou a região seguinte, pede para entrar (teriaga para o veneno do trono, chá de gengibre para o
frio do cume, passagem de dhow para a Costa, roupa térmica para o Teto…).

## Falas

- **Por primo:** cada doador tem uma fala diferente para Joaquim, Maria, Maithe, Ícaro e Lavine, puxada da saudação que esse
  morador já tinha (`CityTexts`) e da voz do primo (`Docs/Narrative.md` §3): o Joaquim que fica na frente e aprende a sair dela,
  a Maria que "acende" os bichos, a Maithe que conta, o Ícaro que mede, a Lavine que "não é pequena".
- **Vó Edna** está em todo diário (o radinho), sempre com uma lembrança do **Vô Jorge** (o chiclete na lanterna, a charada
  perdida em Aratta, o tombo no canal de Mingcheng, "a lua envergonhada").
- **"Ainda não"** tem linhas próprias desta cadeia (`quest.mid_story.too_low.*`), sem falar de carta, porque as cartas ainda não
  foram achadas.

## Como funciona no código

- `MidStoryCatalog.Steps` usa o mesmo `LateStoryStep` das Cartas; `LateStoryCatalog.WholeStory` = esta cadeia + as Cartas, e
  `Decide`, `Talk`, `GetQuest`, `FindStep`, `StepsOf` e `GetAllQuests` olham para as duas. Moradores, diário, save e
  `CityQuestCatalog.GetQuest` não precisaram de nada novo.
- A primeira carta (`quest_late_story_r09_foals`) exige `quest_mid_story_r08_full_moon`.
- A raiz (Vó Edna) é a única etapa sem pré-requisito e **fica calada abaixo do nível 20**: os primos falam com a Vó desde o
  nível 1, e ela não deve dizer "ainda não" a quem acabou de chegar.
- O selo é um objetivo de matar o Tages (`enemy_cave_dweller`, 1): o `QuestManager` credita chefe já vencido ao começar a missão
  (`CreditDefeatedUniqueEnemies`), então quem terminou a campanha só precisa descer ao salão.

## O que só dá para ver jogando

- Se o ritmo de nível fecha: as etapas pulam de 5 a 10 níveis, e entre elas ficam as missões de cidade e os mapas.
- Se as zonas abrem na hora certa: o mapa 43 (Canal das Lanternas) pede nível 98 para viajar de barca, mas a zona da Eclusa
  pede 90 a pé; o mapa 35 pede 78, a Mata Morta 72.
- Se cada exigência especial (Chave do Jardim, veneno, frio, tocha) já está na mão quando a etapa manda ir.
