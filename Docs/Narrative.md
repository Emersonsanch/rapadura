# Rapadura — Bíblia narrativa

> Fonte de verdade da história, das vozes dos primos e de quem sabe o quê em cada ponto da campanha.
> Toda fala nova (missão, NPC, evento, conquista, relíquia, conversa de estrada) deve bater com este documento.
> Idioma: **pt-BR primeiro**, en depois. Tom: aventura de família, infantil e calorosa — perigo de verdade, mas sem
> morte, sangue ou luto pesado. Árvore genealógica resumida também em `Docs/GDD.md` ("Árvore genealógica").

---

## 1. Premissa

Toda férias a família inteira se junta no **sítio da Vó Zefa**, na beira da Floresta, ao lado da **vila**. Neste ano o
**Cristal do Sol** da vila amanheceu apagado: os bichos ficaram bravos, as estradas ficaram perigosas e uma sombra fria
começou a escorrer das Cavernas. Os cinco primos partem juntos para trazer a luz de volta, cruzando
**Floresta → Deserto → Montanha → Cavernas**.

A verdade, revelada aos poucos: há muito tempo os antigos **prenderam no fundo das Cavernas uma criatura com medo do
escuro**, o **Habitante Ancestral**, e construíram **guardiões** (o Guardião da Ruína, os golens de pedra) para vigiar o
selo. O selo está rachando; a sombra do Habitante "apaga a luz" de quem toca — o Lobo Alfa, o Guardião, o Senhor da
Guerra — e ele mesmo rouba toda luz que acha, porque está sozinho no escuro há tempo demais. Ninguém é mau de verdade:
os chefes são **apagados**, não vilões. Vencer = devolver a luz e a memória deles.

**"Rapadura" é o título do jogo, não o nome do mundo.** O mundo é **o sertão** — no mapa-múndi, uma **ilha do sertão**
(a ilha low-poly do mapa, `Gameplay/World/Map/Dressing`), com a vila, o sítio da Vó e as quatro regiões. Textos falam
"o sertão" (en: "the sertão"); "ilha" só quando se fala do mapa (ex.: segredos "pela ilha", `SecretTexts`). Nunca "o mundo
de Rapadura", "o continente" ou "o equilíbrio de Rapadura" (corrigido nas lores, missões pessoais e Convocação em 2026-10-30).

### A convenção do mapa ("um primo na frente, os outros no rádio")

Na história os cinco viajam **juntos** (prólogo, interlúdios e epílogo falam dos cinco). No mapa-múndi o jogador controla
**um** primo: é quem vai **na frente**, batendo a estrada; os outros vêm atrás, acampam na região ou cuidam de outra
coisa, e conversam pelos **radinhos de pilha** que o Ícaro montou com as sobras do rádio da Vó Zefa (prólogo). Por isso:

- eventos da estrada e o Caderno trazem a fala **só do primo ativo**;
- as "Conversas entre primos na estrada" usam o rádio ("Maithe (no rádio): …") ou uma lembrança;
- a Vó Zefa fica sabendo de tudo **pelo rádio** e anota no Caderno (bestiário);
- linhas como "Tem alguém aí? Ah, é, sou só eu" (Lavine na tempestade de areia) são piada, não contradição.

---

## 2. Família

```
                    Vó Zefa ── Vô Chico (falecido, "Deus o tenha")
                                  │
        ┌─────────────────────────┼─────────────────────────┐
   (pais de Maithe e Ícaro)  (pais de Maria e Joaquim)  (pais de Lavine)
        │                         │                         │
  Maithe (14) · Ícaro (11)   Maria (13) · Joaquim (12)    Lavine (9)
```

- **Irmãos:** Maithe e Ícaro; Maria e Joaquim. **Lavine é prima de todos.** Todos os outros pares são primos.
- **Ordem de nascimento (canônica):** Maithe > Maria > Joaquim > Ícaro > Lavine. Maria é só **três meses** mais nova que
  Maithe (piada recorrente: "três meses e onze dias"), mas as duas nasceram em **anos diferentes**: Maithe no fim de um ano
  (perto do Natal), Maria no começo do ano seguinte (março). Por isso na escola ficam em séries diferentes e a Maithe
  "faz 14" meses antes; em idade "de documento" a diferença é de um ano. Joaquim nasceu logo depois de Maria. Ícaro é 2 anos mais velho que
  Lavine.
- **Vô Chico**: tropeiro, tocou tropa pelo Deserto com o **Seu Bento**, tinha a mula **Teimosa**, tocava gaita desafinada
  na varanda, escondeu lembranças pelo mapa para os netos (Relíquias do Sertão). Morreu antes do jogo; é lembrado com
  carinho, nunca com tristeza pesada.
- **Vó Zefa**: a âncora. Fogão a lenha, café coado no pano, pote de rapadura, o **Caderno** (bestiário), o **Baú**. Fala
  com "tu" nordestino ("tu tem", "teu") e chama os netos de "meus netos", "minha mais velha", "minha caçula".
- **Caramelo**: vira-lata caramelo que apareceu na porteira **no dia em que o Cristal apagou** e não arredou mais o pé.
  Late antes do perigo. Vai com os primos.
- **A vila velha**: onde Joaquim (e Maria) nasceram, na beira da Floresta. Anos atrás uma noite de sombras expulsou os
  moradores; a família se mudou para perto da Vó Zefa. As ruínas são o palco da missão pessoal do Joaquim.

### Regras de tratamento (como cada um chama o outro)

| Quem fala → com quem | Pode | Nunca |
|---|---|---|
| Irmão(ã) mais velho(a) → mais novo(a) | "maninho/maninha", "pestinha" (Maithe→Ícaro), nome | — |
| Irmão(ã) mais novo(a) → mais velho(a) | "mana" (Ícaro→Maithe, Joaquim→Maria), "Sis" em en | "maninha/maninho" (implica mais novo) |
| Primo → prima/primo | "prima", "primo", nome | "irmã/irmão", "mana" |
| Qualquer um → Lavine | nome; "caçula" só com carinho | "pequena"/"bebê" (ela odeia — exceção: Maithe no pós-jogo, com permissão) |
| Netos → avó | "Vó", "Vó Zefa" (en: "Grandma", "Grandma Zefa") | "Vovó" |
| Netos → avô | "Vô Chico" (en: "Grandpa Chico") | — |

---

## 3. Os cinco primos

### Maithe — a mais velha (14), Arqueira dos Ventos
- **Artefato:** Olho do Vento (vê ameaças invisíveis). Primeiro arco: entalhado pelo Vô de galho de goiabeira.
- **Personalidade:** protetora, competitiva, seca e segura; "conta os primos" sem parar. Implica com o irmão mas morre de
  orgulho dele. Rivalidade amistosa com Maria (os três meses).
- **Voz:** frases curtas, imperativas ("Fica atrás de mim", "Bora"), humor seco. Vocabulário de caça (rastro, mira, vento).
- **Arco:** quer descobrir "quem fez os monstros" e não consegue soltar o controle. Floresta: lidera. Deserto: descobre com
  o Ícaro que os guardiões foram construídos. Montanha: o vento "mente" para o Olho — aprende a confiar nos primos.
  Cavernas: deixa a Maria ir na frente com a luz. **Epílogo:** a resposta é "ninguém — eram guardiões que a sombra fez
  esquecer"; pela primeira vez deixa alguém carregar a mochila dela.

### Maria — 13, Curandeira da Luz
- **Artefato:** a luz do Templo Solar; medalhinha de sol que a Vó pendurou no primeiro dia de Templo.
- **Personalidade:** sonhadora que **decide**; foi ela quem sonhou os cristais apagando e chamou todo mundo. Carinhosa,
  firme, reza pelo irmão. Mão boa pra pamonha.
- **Voz:** calma, acolhedora ("Respira", "Vai com Deus"), fala de sonho e luz; dá bronca no Joaquim; chama-o de "maninho".
- **Arco:** acredita que todo bicho "apagado" pode ser aceso de novo (Lobo Alfa: "Ele não era mau"). Cavernas: acende a
  luz devagarinho para o Habitante. **Epílogo:** "no sonho tinha cinco primos, e cinco voltaram"; guarda uma lasquinha do
  Cristal (que esfria no começo da Nova Jornada+).

### Joaquim — 12, Guardião das Raízes
- **Artefato:** Escudo das Raízes (Eternas), herança da família, grande demais pra ele. Primeiro escudo: tampa de barril
  aos seis anos, "pra proteger a Maria dos gansos".
- **Personalidade:** teimoso, impulsivo, sempre na frente; o mais velho dos meninos — banca o protetor do Ícaro e da
  Lavine. Come muito (baião de dois, "sempre repete"). Finge que não está cansado.
- **Voz:** exclamações ("Pode vir!", "Deixa comigo!"), chama a irmã de "mana". Bravata que esconde carinho.
- **Arco:** proteger = ficar na frente → proteger também é **saber sair da frente** (lição da Montanha). **Epílogo:**
  encosta o escudo na parede "sem pressa de pegar de volta" e admite: "Ainda bem que vocês estavam atrás de mim. Ou na
  frente."

### Ícaro — 11, Mestre das Máquinas
- **Artefato:** drone, torreta, medidor de luz, os radinhos de pilha, o caderno de anotações dele (≠ Caderno da Vó).
  Primeira engrenagem: da máquina de descascar cajá que explodiu duas vezes, aos cinco anos.
- **Personalidade:** cientista mirim, curioso, falante; quer impressionar a irmã ("Não conta pra Maithe").
- **Voz:** números e método ("Viu? Ciência", "Conclusão científica", "Estatisticamente"), chama Maithe de "mana".
- **Arco:** acha que tudo se resolve com invenção → descobre no painel do Guardião que os antigos também deixaram um
  **aviso**; entende que a melhor invenção é a família. **Epílogo:** última página do caderno: "Melhor invenção de todas:
  os primos" (escondeu da Maithe — que leu mesmo assim, ver conversa de estrada `maithe_icaro_last_page`).

### Lavine — 9, a caçula, Feiticeira das Sombras
- **Artefato:** a sombra/Noite Rubra (eclipse em que nasceu); fita vermelha que a Vó amarrou no pulso dela "pro escuro
  saber que ela é nossa".
- **Personalidade:** séria, honesta até demais ("Ele é feio. Eu falo"), quer ser levada a sério ("Eu não sou pequena. Sou a
  mais nova. É diferente."). Tem medo **da própria sombra** no começo.
- **Voz:** frases curtas, humor seco infantil, contraria quem a protege demais. Chama os outros de "prima/primo".
- **Arco:** medo da noite → "o poder dela é para proteger". Floresta/Deserto: friozinho na barriga à noite. Montanha:
  dorme no ombro da Maithe na fogueira. Cavernas: "o escuro não me assusta" e mostra que "a sombra também pode ser macia".
  **Epílogo:** volta de cabeça erguida; ninguém mais diz que ela é pequena demais.

---

## 4. Linha do tempo

### Campanha (ciclo 0)
| # | Momento | Gatilho no código | O que acontece / passa a ser sabido |
|---|---|---|---|
| 0 | **Prólogo** "o Cristal apagado" | `StoryBeat.Prologue` (1º mapa) | Cristal apagado; cada primo tem seu motivo (painel do herói); Vó dá rapadura, os **radinhos** e pede para contarem cada bicho para o **Caderno**. Partem para a Floresta. |
| 0b | Caramelo apresentado | 1º "Fazer carinho" na vila (`CompanionPreferences.Introduced`) | Vó Zefa: ele apareceu na porteira no dia do apagão. |
| 1 | Floresta: Dona Cida (rezadeira), missões dos lobos e aranhas | região Floresta | Bichos "bravos"; ninguém sabe ainda por quê. |
| 2 | **Lobo Alfa** vencido → interlúdio "A Floresta respira de novo" | `InterludeForest` | **Pedrinha escura** na coleira; Maria: "alguém apagou a luz dele"; areia dourada aponta o Deserto (Caramelo espirra). |
| 3 | Deserto: Seu Bento (tropeiro, amigo do Vô Chico) | região Deserto | Caravana travada por escorpiões. |
| 4 | **Guardião da Ruína** → "Segredos na areia" | `InterludeDesert` | Ícaro lê o **painel dos antigos**: criatura faminta de luz presa nas Cavernas, guardiões vigiando o selo, **selo rachando**. Vó anota pelo rádio. |
| 5 | Montanha: Mestre Tonho (mineiro) | região Montanha | Golens descem a encosta; sino da mina calado. |
| 6 | **Senhor da Guerra** → "No alto da Montanha" | `InterludeMountain` | Ele volta a si, agradece; revela que o que mora nas Cavernas **tem medo do escuro**. Fogueira: rapadura em cinco (+ um pedacinho pro Caramelo). |
| 7 | Cavernas: Benedita das Lamparinas | região Cavernas | Morcegos apagam as lamparinas. |
| 8 | **Habitante Ancestral** → Epílogo "a luz volta para casa" + créditos | `Ending` | Ele só estava sozinho; Maria e Lavine dividem luz e sombra; Cristal acende. Arcos fecham; Vó, Caramelo e a cadeira do Vô Chico na varanda. |

Missões pessoais (vila, `VillageNpcCatalog`) e a Convocação da Maria (`quest_convergence_the_gathering`) correm em
paralelo: Joaquim (vila velha/Floresta), Ícaro (ruínas/Deserto), Maithe (golens marcados/Montanha), Maria (fragmento nas
cavernas da montanha), Lavine (enxame da Noite Rubra/Cavernas) → ponto de encontro → **Habitante Ancestral "em grupo"**
(objetivo `face_the_ancient_dweller`, `enemy_cave_dweller`). A trilha termina onde a ordem das regiões termina
(Floresta → Deserto → Montanha → Cavernas): a última luta da Convocação é a mesma do epílogo; um chefe vencido antes de
aceitar a missão é creditado (`QuestManager.CreditDefeatedUniqueEnemies`). A fala da Maria no ponto de encontro cita só o
que o prólogo libera ("o frio vinha do fundo das Cavernas"), porque as Cavernas abrem por vitórias, não pelo interlúdio.

### Pós-jogo (ciclo 0 completo)
- NPCs de região cumprimentam com as falas "depois do chefe" (`greet_after_*`).
- Habitante vira "vizinho com medo do escuro" que pede lamparina emprestada (Benedita, conversas de estrada).
- Relíquias, Caderno completo, conquistas; botão **Nova Jornada+**.

### Nova Jornada+ (ciclo 1+)
- Prólogo trocado por "o Cristal pisca de novo" (`story.new_journey.*`): poucos dias depois da festa; o Habitante manda
  recado **por um vaga-lume**: sombras mais antigas acordaram com o barulho da luz voltando.
- Maithe "conta os primos quatro vezes"; lasquinha da Maria esfriou; Ícaro: "Experimento número dois"; Lavine passa
  primeiro pela porteira.
- História, missões de região e chefes recomeçam (`StoryProgress.BeginNewJourney`); o que foi ouvido nas conversas de
  estrada continua ouvido, e entram as conversas exclusivas da Nova Jornada+ (`MinCycle = 1`).

---

## 5. Quem sabe o quê, e quando

| Fato | Quem sabe primeiro | Todos sabem a partir de |
|---|---|---|
| Cristal apagado; sombra vem das Cavernas | todos (Maithe pelo Olho, Maria pelo sonho, Ícaro pelo medidor, Lavine pelo frio) | Prólogo |
| Caramelo chegou no dia do apagão | Vó Zefa | apresentação do Caramelo |
| Os chefes estão "apagados", não são maus | Maria (intuição) | Interlúdio da Floresta |
| Existe um selo, guardiões e uma criatura faminta de luz | Ícaro (painel) | Interlúdio do Deserto |
| Os golens foram **construídos** como guardiões | Maithe (missão pessoal, estudioso) | Interlúdio do Deserto |
| A criatura tem medo do escuro | Senhor da Guerra | Interlúdio da Montanha |
| O Habitante não é monstro, só está sozinho | os cinco | Epílogo |
| Novas sombras antigas acordaram | Habitante (vaga-lume) → Vó Zefa | Prólogo da Nova Jornada+ |

Regra prática: **uma fala gateada por estágio só pode citar o que a coluna "Todos sabem" já liberou**. As conversas de
estrada usam exatamente esses estágios (`RoadChatterStage` = interlúdios vistos). Falas de NPC não gateadas (saudações
normais) não citam nada além do prólogo.

---

## 6. Motivos recorrentes

- **Cristal do Sol** — a luz da vila; apaga (prólogo), acende (epílogo), pisca (NG+). Lasquinha da Maria = memória dele.
- **Vó Zefa** — fogão a lenha (Cozinha), Baú, Caderno, beijo na testa, "Fiquem juntos", rapadura. Recebe as notícias pelo
  **radinho** e anota.
- **Caderno da Vó Zefa** (bestiário) — cada bicho vencido, contado pelo rádio. Cita o Vô Chico ("lobo respeita quem não
  corre"). As páginas dos chefes falam do que eles **eram** antes da sombra.
- **Radinhos de pilha** — feitos pelo Ícaro com as sobras do rádio da Vó; ligam primos e Vó durante a viagem.
- **Caramelo** — chegou no apagão; fareja a areia dourada; ganha pedacinho de rapadura na fogueira; corre na frente no
  epílogo. (O companheiro pode ser desligado nas opções; na história ele existe sempre.)
- **Rapadura** — briga pelo último pedaço (prólogo, canivete do Vô), dividida em cinco na fogueira, pote cheio no fim.
- **Contar os primos** — tique da Maithe; "uma, duas, três vezes" → "quatro" na NG+.
- **Relíquias de família** — Olho do Vento (Maithe), Escudo das Raízes (Joaquim), luz do Templo e lasquinha (Maria), caderno
  e engrenagens (Ícaro), Noite Rubra e fita vermelha (Lavine); e as **Relíquias do Sertão** do Vô Chico (gaita, cartas,
  chapéu, chocalho da Teimosa, canivete, pião de Maria e Joaquim, retrato dos cinco). Textos de história não podem
  **depender** de uma relíquia escondida ter sido achada (o epílogo cita a cadeira de balanço do Vô, não a gaita).

---

## 7. Glossário regional

| Palavra | Sentido | Onde aparece |
|---|---|---|
| sertão | o interior seco do Nordeste; aqui, o mundo do jogo | tutorial, créditos, morte |
| sítio | pequena propriedade rural (o da Vó Zefa) | história |
| porteira | portão de fazenda | prólogo, NG+ |
| varanda | alpendre da casa | história, créditos |
| rezadeira | benzedeira que reza e cura com ervas (Dona Cida) | Floresta |
| benzer / bênção | rezar sobre alguém/algo para proteger | Maria, romeiros |
| tropeiro / tocar tropa | condutor de tropa de mulas/jumentos (Seu Bento, Vô Chico) | Deserto, relíquias |
| romeiros | peregrinos que andam até um santuário | evento de estrada |
| benditos | cantos religiosos populares | evento de estrada |
| repente / viola | improviso cantado com viola caipira | evento de estrada |
| mascate | vendedor ambulante | evento de estrada |
| cacimba | poço raso cavado na terra | segredos |
| lajedo | grande laje de pedra no chão | evento de estrada |
| mandacaru | cacto grande do sertão | segredos |
| jatobá, cajá, caju, mangueira, goiabeira | árvores/frutas | vários |
| roçado | plantação pequena | Floresta |
| serra | montanha, cadeia de montes | Montanha |
| lamparina | lampião pequeno a óleo (Benedita) | Cavernas |
| rapadura | doce de cana em tijolo | tudo |
| cuscuz, pamonha, baião de dois, queijo coalho, café coado, cocada, bolo de fubá/milho | comidas | Cozinha, NPCs |
| tacho | panela grande de cobre (rapadura) | Cozinha |
| casa de farinha | galpão onde se faz farinha de mandioca | segredos |
| chocalho | sino de pescoço de animal | relíquias |
| Noite Rubra | o eclipse vermelho em que Lavine nasceu — **nome próprio, igual em en** | Lavine |
| Olho do Vento, Escudo das Raízes, Templo Solar | nomes próprios; em en: Eye of the Wind, Shield of Roots, Solar Temple | heróis |
| "Ô de casa!" | saudação ao chegar numa casa | NPCs |
| "Deus o tenha" | dito sobre quem já morreu | Seu Bento |
| caçula | o filho/neto mais novo | Lavine |
| mana / maninho(a) | irmã / irmãozinho(a) — ver regras de tratamento | irmãos |
| de vereda | imediatamente | Caderno |
| "tu tem", "teu" | fala nordestina da Vó Zefa | Cozinha |

---

## 8. Mapa das fontes de texto

| Arquivo | Batidas / conteúdo | Gatilho |
|---|---|---|
| `Gameplay/Story/StoryTexts.cs` + `StoryScript.cs` | prólogo, 3 interlúdios, epílogo (painel por herói), prólogo da Nova Jornada+ | `StoryBeat` vistos, ciclo NG+ |
| `Gameplay/Quests/RegionQuestTexts.cs` + `Gameplay/Dialogue/RegionNpcDialogues.cs` | Dona Cida, Seu Bento, Mestre Tonho, Benedita: saudação por primo, 2 missões, dica do chefe (fala especial de um primo), "o chefe caiu", **saudação depois do chefe** | `ActiveCharacter`, estado das missões, flags `boss_ready`/`region_cleared` |
| `Gameplay/Dialogue/QuestGiverTexts.cs` + `Core/Localization/LocalizationManager.cs` (`quest.*`/`dialogue.*` das missões pessoais) + `Resources/Dialogue/*.asset` + `Resources/Quests/*.asset` + seeders `Editor/*DialogueSeeder*.cs` | missões pessoais dos 5 (vila velha, Templo, golens marcados, ruínas, Noite Rubra), Convocação e primeiro encontro dos cinco | vila (`VillageNpcCatalog`), estado das missões |
| `Gameplay/Dialogue/VillageNpcCatalog.cs` | quem é quem na vila (um NPC por primo + ponto de encontro) | hub "Vila" |
| `Gameplay/Pets/CarameloTexts.cs` (+ `CarameloCuddle.cs`) | apresentação do Caramelo pela Vó, 2 falas de carinho por primo | botão "Fazer carinho" na vila |
| `UI/WorldMap/KitchenTexts.cs` | Vó Zefa no fogão: saudação e aula por primo, pratos favoritos | hub da vila → Fogão a lenha |
| `UI/WorldMap/WorldEventTexts.cs` (+ `Gameplay/World/Events/WorldEventCatalog.cs`) | 8 eventos de estrada com fala do primo ativo | viagem no mapa (região, hora) |
| `Gameplay/World/Chatter/RoadChatterCatalog.cs` + `RoadChatterTexts.cs` | **Conversas entre primos na estrada** (46 trocas, todos os pares) | viagem; estágio = interlúdios vistos; ciclo NG+ |
| `UI/Bestiary/BestiaryTexts.cs` | Caderno da Vó Zefa: página e comentário de cada primo por bicho/chefe | inimigo derrotado |
| `UI/Achievements/AchievementTexts.cs` | conquistas (pessoais, irmãos, família, campanha, NG+, Caderno) | `AchievementManager` |
| `UI/WorldMap/NewGamePlusTexts.cs` | convite da Vó para a Nova Jornada+ por primo | botão Nova Jornada+ |
| `Gameplay/Secrets/SecretTexts.cs` | segredos do mapa e Relíquias do Sertão do Vô Chico | segredos/arena |
| `Gameplay/Characters/*.cs` + `Gameplay/Content/ContentTexts.cs` | lore da seleção de herói, descrições das habilidades | tela de seleção, árvore de skills |
| `UI/CharacterSelect/HeroFamily.cs` | linha de parentesco ("Irmã do Ícaro · a mais velha") | seleção |
| `Gameplay/Quests/DailyQuestTexts.cs` | missões diárias da vila (falas da família) | quadro de diárias |
| `UI/Tutorial/TutorialTexts.cs` | dicas de primeira vez | `TutorialManager` |
| `UI/Common/UiTexts.cs` (Credits*) + `UI/Menus/CreditsContent.cs` | créditos: "Os primos", agradecimento, varanda da Vó | após o epílogo / menu |
| `Docs/GDD.md`, `TODO.md` (Roster) | resumo de design e lore longa | — |

---

## 9. Conexões entre sistemas (feitas) e pendências

**Feitas (2026-10-29):**
- Prólogo: Vó entrega os **radinhos** e pede que contem os bichos para o **Caderno** (motiva o bestiário e o rádio).
- Interlúdios: Caramelo fareja a areia (Floresta); Vó anota pelo rádio (Deserto); pedacinho de rapadura pro Caramelo (Montanha).
- Epílogo: cada painel de herói fecha o arco (Olho do Vento sem ameaça e a resposta da Maithe; sonho dos cinco da Maria;
  escudo encostado do Joaquim); varanda com Caderno, Caramelo e a cadeira do Vô Chico.
- NPCs de região: **saudação depois do chefe** por primo (24 falas, flag `region_cleared`); Seu Bento conheceu os primos
  no sítio e tocou tropa com o Vô Chico; Mestre Tonho recebeu recado do Ícaro pelo Seu Bento; Benedita recebeu as
  lanternas do Ícaro pelo Mestre Tonho.
- Caderno: cita o Vô Chico; páginas do Lobo Alfa e do Senhor da Guerra lembram o que aconteceu nos interlúdios.
- Missões pessoais reconciliadas com a campanha: "vila velha" (não a vila da Vó), golens = guardiões construídos,
  Senhor da Guerra = corrompido (não o mandante).

**Feitas (2026-10-30):**
- **Região de onde o primo voltou** = região do último interlúdio visto (`StoryBeats.LastRegionCleared`: Floresta,
  Deserto, Montanha; o epílogo conta como Cavernas). Base das três conexões abaixo.
- **Cozinha da Vó lembra a região:** `KitchenPresenter.GrandmaLine(..., cameFrom)` usa
  `ui.kitchen.grandma.after_<região>_<primo>` (20 falas) ou `after_<região>` sem primo; a aula de prato favorito ainda
  vence. Na sessão, a 1ª, 3ª, 5ª... abertura do fogão lembra a viagem e as outras mantêm a saudação de sempre
  (`KitchenPresenter.CameFromForVisit`).
- **Caramelo lembra a viagem:** `pet.caramelo.cuddle.<primo>.after_<região>` (20 falas) entra no rodízio de carinho
  em primeiro lugar (fala da etapa → fala 0 → fala 1 → ...).
- **Condição de diálogo `StoryBeatSeen`** (`DialogueConditionType` = 4, chave = id do beat, ex. `interlude_desert`):
  lida por `IStoryDialogueContext` (`QuestDialogueContext` → `StoryProgress`). Os cinco NPCs da vila ganham a opção
  "Soube das novidades da estrada?" depois do interlúdio da Floresta, com uma fala por beat (o mais novo vence):
  `VillageStoryNews` / `VillageStoryNewsTexts` (20 falas), montado em cima do asset sem editá-lo.
- **Conversas de estrada repetem:** quando todas as elegíveis já foram ouvidas, repetem a ouvida há mais tempo, com o
  mesmo cooldown (`RoadChatterRules.Pick(..., heardRank)`, ordem salva em `RoadChatterSaveState.heardOrder`).
- Convocação termina no Habitante Ancestral (ver §4); mundo = sertão (ver §1); Maithe e Maria em anos diferentes (§2).

**Pendências de texto:**
- `SecretTexts.PanelSubtitle`: "pela ilha" / "around the island" está **certo** (o mapa é uma ilha; ver §1).
- `SecretTexts` (Fita vermelha da Lavine, en): "Noite Rubra" já padronizado.
- Ordem da trilha pessoal/Convocação: resolvida (Convocação termina no Habitante Ancestral, §4).

---

## 10. Cutscenes (intro, outro, ferrões de chefe)

Código: `Gameplay/Story/CutsceneScript.cs` (lista de planos em C# puro — sem ScriptableObject), `CutsceneSequence.cs`
(relógio, regras, câmera), `CutsceneTexts.cs` (en + pt-BR), apresentador `UI/Story/CutscenePlayerController.cs`, visual
`Resources/UI/Cutscene.uss`. **Sem Timeline:** `com.unity.timeline` não está no `Packages/manifest.json` e o mapa-múndi é
montado em código (não há cena para autorar uma Timeline), então um player pequeno guiado por dados move a `Camera.main`
entre enquadramentos (órbita: foco, yaw, pitch, distância, FOV) ancorados nos nós de região do mapa (Vila = nó da Floresta).

**Moldura mítica (maniqueísta, em tom de conto infantil — não doutrina):** Luz × Treva; faíscas de luz presas na matéria
(bichos, pedras, guardiões) que os primos libertam; o **Chamado** (o sonho da Maria) e a **Resposta** (os cinco); a sombra
do **Príncipe das Trevas** escorrendo das Cavernas; a **Coluna da Glória** por onde a luz liberta sobe no fim. Quem conta o
mito é o **Vô Jorge** (lembrado pela Vó Edna) — o coração humano são os cinco primos, a Vó Edna e a cadeira do Vô na varanda.

| Cutscene | Quando | Planos (resumo) |
|---|---|---|
| **Intro "O Chamado"** | Novo jogo (ciclo 0), **antes** dos painéis do prólogo | Ilha inteira → chegada no sítio (Lavine "cheguei primeiro", Joaquim, Maithe conta os primos) → varanda (Vó Edna, café e bolo de milho) → Ícaro nota o Cristal apagado → a câmera cruza o mapa até as Cavernas: a pedra velha racha (o **gancho do selo**, só narrado — os primos ainda não sabem do selo) e a luz vira mil faíscas presas → Maria: o sonho chamando → Vó Edna: "quando a luz chama, a gente responde". Tinta de sombra cresce no gancho. |
| **Outro "A Coluna da Glória"** | Depois dos painéis do epílogo, **antes dos créditos** | Ilha acesa, a luz sobe em coluna → Cavernas (Lavine se despede do Habitante, "vizinho") → Montanha/Deserto (Ícaro: medidor no máximo) → Floresta/Vila (Joaquim, bolo de milho; Maithe conta seis com o Caramelo) → **varanda da Vó Edna** (Maria: "ouvimos o chamado e respondemos"; Vó Edna; narrador: cadeira de balanço do Vô Jorge e a Coluna da Glória). Termina na varanda, como os créditos. |
| **Ferrão de chefe** | Banner "Chefe da região" (`BossIntroBannerController`) | Linha extra sob o nome do chefe: a fala do **primo ativo** por região (`cutscene.stinger.<região>.<herói>`), ou uma linha do narrador sem herói. Fica +1,5 s no banner. |

Regras de apresentação:
- **Pulável sempre:** botão "Pular", Esc, gamepad Leste/Start. Toque / Espaço / Enter / gamepad Sul = próximo plano; cada
  plano avança sozinho após o tempo de leitura (`CutsceneRules.ReadingSeconds`, 14 caracteres/s, 2,5–14 s).
- **Reduzir Movimento / Flashes:** cada plano é um corte parado (sem travelling), barras e textos sem deslizar/fade, tintas
  pela metade.
- Letterbox, título no 1º plano, legenda com medalha de retrato (inicial + cor do herói; Vó Edna com cor própria; narrador
  sem retrato, itálico). Esconde os outros UIDocuments e segura uma `GameplayPause`; devolve a câmera ao terminar.
- **Música:** `StoryPanelController.IsShowing` inclui a cutscene, então o `MusicDirector` toca o contexto Story (intro) e
  mantém a variante do epílogo (outro → créditos).
- Toca **uma vez por sessão** por cutscene (sair no meio dos painéis não cria loop); Nova Jornada+ não tem intro (tem o
  próprio prólogo). **Testes PlayMode:** `SmokeTestBase` desliga `CutscenePlayerController.AutoPlay`, então os fluxos
  (`WorldMapFlowTestBase`, `CampaignPlaythroughTests`) nunca esperam por cutscenes.
- Falas de cutscene seguem a §5: personagens só citam o que já foi liberado; o gancho do selo e o mito ficam com o narrador.
