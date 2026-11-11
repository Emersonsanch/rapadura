# As Cartas da Estrada — história principal das Regiões 9–16

> Cadeia principal que liga a Estepe dos Nartes à **Varanda do Tempo de Depois** (mapa 100). Fonte de texto:
> `Assets/Scripts/Gameplay/Quests/LateStoryTexts.cs`; dados: `LateStoryCatalog.cs`; testes:
> `Assets/Tests/EditMode/LateStoryCatalogTests.cs`. Tom, vozes e proibições: `Docs/Narrative.md` e
> `Docs/WorldBible_Maniqueismo.md`. Passa depois do epílogo (Tages e o selo, nível ~20); a primeira etapa é de nível 100.

## O arco

Depois do selo refeito, a Vó Edna acha no forro do Caderno um bilhete com a letra torta do Vô Jorge: **oito cartas deixadas
com amigos na estrada do norte, "para quem chegar longe"**. Os cinco primos sobem atrás delas, região por região. Em cada lugar
alguém guarda ou perdeu uma folha; a Vó Edna ouve tudo pelo radinho e **corrige o marido** (era dois potros, ele não levou
meia nenhuma, atravessou a ponte de joelhos). É comédia de casal, e por baixo dela vai crescendo a saudade.

- **Regiões 9–12:** cartas engraçadas, cada uma sobre uma trapalhada do Vô Jorge moço.
- **Região 13:** a carta endereçada aos cinco pelo nome, **escrita antes da Lavine nascer**. Primeira vez que ninguém ri.
- **Região 14:** a primeira carta sem piada: **a sombra vai subir atrás de vocês**.
- **Região 15:** o carteiro Makarios entrega na Beira do Abismo a carta que manda **subir, não descer**, porque "guardou um lugar".
- **Região 16:** o Rio de Luz, os Pés da Coluna da Glória, onde o último resto do escuro os segue pelos degraus, e a última
  carta, de uma linha: **a cadeira fica à esquerda, e não é para sentar.**
- **Fim, na Varanda:** cadeira vazia balançando um pouquinho, pomar de luz, o rádio perto da cadeira, a Vó Edna canta mal, do jeito
  dele, os cinco cantam junto e **uma faísca sobe**. O Vô Jorge nunca aparece; o Tempo de Depois continua sem ser mostrado (bíblia §1).

## As etapas

Toda etapa exige a anterior e um nível maior; o doador só entrega a partir do nível da tabela. Local = zona do overworld
(objetivo ReachLocation, relatado por `LateStoryLocationReporter`); inimigo = `enemy_*` não-chefe da fauna dessa zona.

| # | Região | Etapa | Quem entrega (cidade) | Local (mapa) | Nível | Objetivo |
|---|---|---|---|---|---|---|
| 1 | 9 Estepe dos Nartes | A Primeira Carta | Soslan (Acampamento) | Pistas de Corrida (47) | 100 | 5 grifos jovens |
| 2 | 9 | Debaixo do Kurgan de Ouro | Satana | Kurgans de Ouro (48) | 106 | 6 lobos de bronze |
| 3 | 10 Picos de Farnkesh | Amarrada ao Vento | Tadi (Farnkesh) | Pontes de Vento (55) | 148 | 6 ushabtis de corda |
| 4 | 10 | O Sino Que Guardou Uma Carta | Shad-Ohrmizd | Pináculo do Sino (56) | 156 | 6 lanternas quebradas |
| 5 | 11 Mar Gelado | O Lúcio e a Lata | Yirkap (Porto da Lua Branca) | Canais de Gelo (60) | 184 | 6 lúcios gigantes |
| 6 | 11 | Onde os Mamutes Dormem | Pera | Galerias dos Mamutes (62) | 200 | 4 mamutes |
| 7 | 12 Hinokura | Carta na Laca | Mestra Akane (Hinokura) | Jardins de Lava (66) | 224 | 6 shisas apagadas |
| 8 | 12 | Nove Fornalhas, Uma Carta | Sango | Pátio das Fornalhas (69) | 254 | 4 ogros de fornalha |
| 9 | 13 Rio das Doze Horas | Escaravelhos no Papiro | Sinuhe (Cais de Mesektet) | Margem de Papiro (71) | 258 | 8 escaravelhos de sombra |
| 10 | 13 | O Que os Remadores Guardam | Djedi | Praia dos Remadores (71) | 262 | 6 íbis apagadas |
| 11 | 14 Margens da Mistura | Gafanhotos na Cevada | Utnapishtim (Dur-Jarras) | Terraços de Cevada (77) | 300 | 8 gafanhotos de betume |
| 12 | 14 | A Tábua no Portão | Sin-leqi-unninni | Portão de Tijolo (77) | 302 | 6 leões de tijolo |
| 13 | 15 Limiar Profundo | As Últimas Lâmpadas | Pamour (Última Lâmpada) | Muralha em Arco (83) | 340 | 6 ushabtis de laca |
| 14 | 15 | O Carteiro na Beira | Makarios | Beira do Abismo (83) | 344 | 8 sombras mordedoras |
| 15 | 16 Reinos do Céu | O Rio de Luz | Adda (Cais da Coluna) | Rio de Luz (92) | 426 | 7 peixes de luz apagados |
| 16 | 16 | Os Pés da Coluna | Adda | Pés da Coluna da Glória (98) | 482 | 8 de escuro rastejante |
| 17 | 16 | A Varanda do Tempo de Depois | Adda | Varanda (100) | 495 | sentar ao lado da cadeira |

Recompensas: 2300 a 4600 de XP, poções, 180 a 300 de ouro e, em etapas pares, um item que a região já paga (lamparina rúnica,
runas, roupa térmica, barco de junco, capa de tempestade). Mesma faixa das missões de cidade.

## Como funciona no jogo

- **Quem fala:** ao chegar perto do morador (`CityResidentPost.Interact`, regras em `CityResidentTalk`), a oferta e o andamento da história
  vêm antes das missões de cidade; as falas que não entregam nada ("nível baixo", a fala final do Adda) vêm depois delas, para não esconder uma
  missão de cidade. Lojista com fala pendente mostra o diálogo com a opção de ver a loja; sem nada, a loja abre direto (depois da primeira visita de cada primo).
- **Falas:** cada etapa tem uma fala de oferta **por primo**, escrita para ele (pt-BR e en), o título, a descrição (a carta do Vô Jorge),
  os objetivos, o diário (a Vó Edna no rádio) e a fala de andamento. Chaves `quest.late_story_*`, carregadas junto com `CityQuestTexts`.
- **Tipo:** `QuestType.MainStory` (aparece como principal no diário de missões).
- **Local:** `AreaChangedEvent` do overworld → `QuestManager.ReportLocationReached(zona)`. O local nunca é a zona da própria cidade do doador.
  A última etapa conclui com `RockingChairSatEvent`, ao sentar ao lado da cadeira da Varanda (`CeuMechanicsDriver`), e não ao chegar na zona.

## Pendências

- A fala do morador é o texto flutuante de sempre; as cartas ficam longas no diário, onde cabem.
