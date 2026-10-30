# Rapadura — Bíblia narrativa

> Fonte de verdade da história, das vozes dos primos e de quem sabe o quê em cada ponto da campanha.
> Toda fala nova (missão, NPC, evento, conquista, relíquia, conversa de estrada) deve bater com este documento.
> **Reescrito em 2026-10-30 a partir de `Docs/WorldBible_Maniqueismo.md`** (re-tema maniqueísta: sai o sertão, entra a
> **Ilha da Aurora**). Para tabelas completas de-para (NPCs, bichos, itens, montarias, casa, roupas) e as fontes de cada
> nome, a bíblia do mundo manda; este documento cuida da **história, das vozes e da ordem do que se sabe**.
> Idioma: **pt-BR primeiro**, en depois. Tom: conto de fadas de família, infantil e caloroso. Perigo de verdade, mas sem
> morte, sangue ou luto pesado. Árvore genealógica resumida também em `Docs/GDD.md` ("Árvore genealógica").
> **Ids não mudam** (`CharacterId`, `enemy_*`, chaves de localização, classes C#, chaves de save): só o texto visível.

---

## 0. Regras de tom (resumo da bíblia do mundo §0)

1. **É conto de fadas, não doutrina.** O mito é o que o **Vô Jorge contava na varanda** e a **Vó Edna** continua
   contando. Ninguém reza, prega ou se converte. Nas falas não aparecem "religião", "fé", "pecado", "salvação", "igreja",
   "templo de culto", "deus", "profeta", "rezar", "orar", "amém".
2. **Nenhuma figura sagrada viva ou fundadora é personagem** (Mani, Jesus, Zoroastro, Buda, orixás, voduns etc.). O lado
   mais alto da Luz é só "**a Luz**".
3. **NPCs são gente comum com nomes dos contos** (o ferreiro Adamas tem o nome do guerreiro de luz, mas é só um ferreiro).
4. **Ninguém é mau de verdade, exceto a Treva sem rosto.** Bichos e chefes estão **apagados**; vencer é **libertar a
   faísca** e devolver a memória. Arcontes são gigantes de conto (gulosos, vaidosos, briguentos). O **Príncipe das
   Trevas** é uma sombra grande, só silhueta, sem sangue.
5. **Culturas vivas com respeito.** Cidades, roupas e festas podem vir de povos reais; **inimigos e chefes nunca**.
6. **Calor de família primeiro:** os cinco primos, a Vó Edna e a cadeira de balanço vazia do Vô Jorge.

---

## 1. Premissa

Toda férias a família inteira se junta na **casa da Vó Edna**, o **pomar** de damasqueiros e romãzeiras na **Vila da Vó
Edna**, à beira do **Bosque do Zarafshan**. Neste ano o **Cristal do Sol** da vila amanheceu apagado: a luz dele se
espalhou em mil **faíscas** pela ilha, os bichos ficaram bravos, as estradas ficaram perigosas e uma sombra fria começou
a escorrer das **Grutas do Limiar**. Os cinco primos partem juntos para trazer a luz de volta, cruzando
**Bosque do Zarafshan → Areias Flamejantes → Serra de Kaggen → Grutas do Limiar**
(ids de bioma iguais: Floresta → Deserto → Montanha → Cavernas).

A verdade, revelada aos poucos: há muito tempo os **Artífices** (o povo antigo que construía **engenhos de luz**)
fecharam com um **selo** a porta do fundo das Grutas, o **Limiar** entre a ilha e a **Terra da Treva**. Do lado de cá,
no escuro, ficou **Tages**, o **Habitante Ancestral**: um ser antigo com rosto de criança e sabedoria de velho, que brotou
da terra ali perto e **tem medo do escuro**. Para vigiar o selo, os Artífices fizeram guardiões que "respondem", os
**ushabtis** (o Grande Ushabti, os Ushabtis de Pedra). O selo está rachando: a sombra da Treva escorre pela fresta e
**apaga a luz** de quem toca (Kök-Böri, o Grande Ushabti, Hahya), e Tages, apavorado com o escuro que sobe, agarra toda
luz que acha, porque está sozinho há tempo demais. Ninguém é mau de verdade: os chefes são **apagados**, não vilões.
Vencer = libertar a faísca e devolver a luz e a memória deles.

Por trás de tudo está **a Treva sem rosto**, o **Príncipe das Trevas**: a sombra grande do outro lado do Limiar. Ele é o
**vilão final invencível**. Na campanha ele nunca é chefe vencível: no fim, os primos **refazem o selo** e ele é
**empurrado de volta**, não destruído. (No online ele pode virar a raide de servidor "**A Grande Treva**", que nunca
"morre", só recua por uma temporada; bíblia do mundo §1.4.)

**"Rapadura" é o título do jogo, não o nome do mundo.** O mundo é a **Ilha da Aurora** (en: *Dawn Isle*), a ilha
low-poly do mapa (`Gameplay/World/Map/Dressing`), com a vila, a casa da Vó e as quatro regiões. Textos falam "a Ilha da
Aurora" ou "a ilha" (ex.: segredos "pela ilha", `SecretTexts`). Nunca "o sertão", "o mundo de Rapadura", "o continente"
ou "o equilíbrio de Rapadura". "Rapadura" só aparece no título, no logo e nos créditos (o doce da Cozinha virou
**Torrão Dourado**).

### 1.1 A cosmologia em uma página (como a criança ouve)

Detalhes e fontes: bíblia do mundo §1.

- **As duas Terras:** a **Terra da Luz** (lá em cima e longe, clara e musical, nunca visitada) e a **Terra da Treva**
  (lá embaixo: fumaça, barulho, gente que só quer mais). As Grutas do Limiar são a borda dela.
- **Os Três Tempos:** *Tempo de Antes* (as Terras separadas) → *Tempo do Meio*, ou **a Mistura** (agora: luz presa em
  cada bicho, pedra, fruta e guardião; é o jogo) → *Tempo de Depois* (a luz volta para casa; só o fim do conto do Vô,
  nunca mostrado).
- **Faíscas:** a luz presa. Quando um bicho apagado é vencido, a faísca sai, sobe e ele volta a ser ele mesmo. Todas as
  faíscas juntas são **a Alma Viva** (nome usado com parcimônia).
- **O Chamado e a Resposta (o motor da história):** no conto do Vô, um **Primeiro Viajante** desceu com cinco luzes no
  peito, perdeu para a Treva e dormiu lá embaixo; um grito o **chamou** e ele **respondeu**. No jogo, o **Chamado** é o
  sonho da Maria; a **Resposta** são os cinco primos. A frase do Vô Jorge, "**quando a luz chama, a gente responde**", é
  a moral da família. O Primeiro Viajante **não é personagem**, e os primos não são ele: são **os que responderam**.
  Os ushabtis ("os que respondem", em egípcio) foram feitos para responder ao chamado da luz e esqueceram como.
- **A Coluna da Glória:** o caminho de luz por onde as faíscas libertadas sobem. Depois de cada chefe, um fio de luz
  discreto sobe sobre a Vila; no fim, a Coluna acende inteira (outro "A Coluna da Glória").
- **As Barcas de Luz:** a **Barca da Lua** junta a luz libertada (por isso a lua cresce), passa para a **Barca do Sol**,
  que leva tudo para casa. No jogo, são a **viagem rápida** (substituem o carro de boi).
- **O Relógio de Zurvan:** o relógio de sol da praça da Vila (Zurvan é o "Tempo sem fim" dos contos; é só um relógio,
  nunca uma pessoa).

### 1.2 Os cinco elementos de luz (um por primo)

No conto, as cinco luzes do Primeiro Viajante eram cinco; quem respondeu também. **O Vô Jorge dizia, na hora de dormir,
que cada neto "tinha uma das cinco luzes"**: os primos sabem disso desde pequenos, e é **piada e orgulho** entre eles
("o meu é vento, o teu é água, empata"), nunca rivalidade séria. O elemento é **sabor** (cor de VFX, nome de set, falas,
tema da região), **não** cria tipo de dano novo.

| Primo | Elemento de luz | Contraparte da Treva | Região onde essa luz está presa | Visual |
|---|---|---|---|---|
| **Lavine** | **Ar Sereno** / *Pure Air* | Fumaça | Vila / céu (a Coluna) | lilás e prata, estrelinhas |
| **Maithe** | **Vento Vivo** / *Living Wind* | Vento Destruidor | Serra de Kaggen | branco-azulado, rajadas |
| **Maria** | **Luz** / *Light* | Treva | Grutas do Limiar | dourado-claro |
| **Joaquim** | **Água Viva** / *Living Water* | Lodo | Bosque do Zarafshan | verde-água |
| **Ícaro** | **Fogo Vivo** / *Living Fire* | Fogo Devorador | Areias Flamejantes | laranja-cobre, faíscas |

### 1.3 As regiões e as cidades

| Região (id) | Nome (pt / en) | Cidade | Gente de referência | Chefe da região (apagado) |
|---|---|---|---|---|
| Floresta | **Bosque do Zarafshan** / *Zarafshan Woods* | **Vila da Vó Edna** (iwan, adobe, pomar, aryk) | Mariam, a Ervateira | **Kök-Böri, o Lobo Celeste** (`enemy_forest_alpha_wolf`) |
| Deserto | **Areias Flamejantes** / *Flaming Sands* | **Oásis de Qocho** (taipa rosa, karez; perto, as **Ruínas dos Artífices**, pirâmides estilo Meroé) | Ammo, o Caravaneiro | **O Grande Ushabti** (`enemy_desert_ruin_guardian`) |
| Montanha | **Serra de Kaggen** / *Kaggen Range* | **Vila de Litema** (rondavéis, muralhas de pedra seca, rocha pintada) | Mestre Mihr, o Mineiro | **Hahya, o Gigante Guerreiro** (`enemy_mountain_warlord`) |
| Cavernas | **Grutas do Limiar** / *Threshold Caves* | **Vila das Lamparinas** (casas no tufo rosa, pontes de corda) | Parthenos, a das Lamparinas | **Tages, o Habitante Ancestral** (`enemy_cave_dweller`) |

Chefes do mundo e eventos (vilões de evento, os **arcontes**, gigantes de conto que **engoliram luz demais**): Saklas, o
Lobo de Cinzas (Floresta); Namrael, o Escorpião-Rei (Deserto); Mazan, o Gigante da Serra (Montanha); **Mahaway, o Rei da
Revoada** (Cavernas); e a **Invasão de Āz**, a Capitã Cobiçosa (sempre feminina), que põe coroinhas de lata dourada nos
bichos ("tudo o que brilha é meu") e esconde a luz em arcas. Nenhum deles é o Príncipe das Trevas. Ohya, irmão de Hahya,
fica de reserva para a NG+ e o online.

### A convenção do mapa ("um primo na frente, os outros no rádio")

Na história os cinco viajam **juntos** (prólogo, interlúdios e epílogo falam dos cinco). No mapa-múndi o jogador controla
**um** primo: é quem vai **na frente**, batendo a estrada; os outros vêm atrás, acampam na região ou cuidam de outra
coisa, e conversam pelos **radinhos de pilha** que o Ícaro montou com as sobras do rádio da Vó Edna (prólogo). Por isso:

- eventos da estrada e o Caderno trazem a fala **só do primo ativo**;
- as "Conversas entre primos na estrada" usam o rádio ("Maithe (no rádio): …") ou uma lembrança;
- a Vó Edna fica sabendo de tudo **pelo rádio** e anota no Caderno (bestiário);
- linhas como "Tem alguém aí? Ah, é, sou só eu" (Lavine na tempestade de areia) são piada, não contradição.

---

## 2. Família

```
                    Vó Edna ── Vô Jorge (já se foi; lembrado com carinho, a cadeira de balanço dele fica no iwan)
                                  │
        ┌─────────────────────────┼─────────────────────────┐
   (pais de Maithe e Ícaro)  (pais de Maria e Joaquim)  (pais de Lavine)
        │                         │                         │
  Maithe (14) · Ícaro (11)   Maria (13) · Joaquim (12)    Lavine (9)
```

- **Irmãos:** Maithe e Ícaro; Maria e Joaquim. **Lavine é prima de todos.** Todos os outros pares são primos.
- **Ordem de nascimento (canônica):** Maithe > Maria > Joaquim > Ícaro > Lavine. Maria é só **três meses** mais nova que
  Maithe (piada recorrente: "três meses e onze dias"), mas as duas nasceram em **anos diferentes**: Maithe no fim de um ano,
  Maria no começo do ano seguinte (março). Por isso na escola ficam em séries diferentes e a Maithe "faz 14" meses antes;
  em idade "de documento" a diferença é de um ano. Joaquim nasceu logo depois de Maria. Ícaro é 2 anos mais velho que
  Lavine.
- **Vô Jorge**: o **contador**. Tudo o que os primos sabem do mito, ouviram dele na varanda (o iwan): "o Primeiro
  Viajante", "as cinco luzes", "as Barcas", "quando a luz chama, a gente responde". Quando moço, viajou com as
  **caravanas** pelas Areias Flamejantes com o **Ammo, o Caravaneiro**, levando a camela bactriana **Teimosa**; tocava
  **flauta ney** desafinado na varanda e escondeu lembranças pelo mapa para os netos (**Lembranças do Vô Jorge**). Já se
  foi antes do jogo; é lembrado com carinho, nunca com luto pesado. Sobre ele se diz "o Vô Jorge ia adorar isso", nunca
  "Deus o tenha". A **cadeira de balanço vazia** dele fica no iwan (e vai para a praça no Bema: "a cadeira dele fica ali,
  e a gente canta perto").
- **Vó Edna**: a âncora e a **guardiã do Cristal**. O **tandir** (o forno de barro, "o forno da Vó"), chá quentinho,
  Torrão Dourado no pote, o **Caderno** (bestiário), o **Baú**, a **Uasamonga** (a taça-caldeirão de cobre que "sabe quem
  merece"). Recebe as notícias pelo rádio. Não é sacerdotisa: é avó. Fala **calorosa e neutra** (sem "tu tem" nordestino)
  e chama os netos de "meus netos", "minha mais velha", "minha caçula". Exemplo: "Essa carinha é de quem quer samsa, minha
  caçula. Senta, que o chá tá quentinho."
- **Ari** (antigo Caramelo; ids `CarameloTexts`, `pet.caramelo.*`, `CompanionPreferences` ficam): cachorro de pelo cor de
  caramelo que apareceu no **portão do pomar** **no dia em que o Cristal apagou** e não arredou mais o pé. Quem deu o nome
  foi a **Lavine** ("ele tem cara de Ari"). Late antes do perigo, vai com os primos. **Fica no ar** (nunca confirmado) que
  ele é um **aralez**, o cão-espírito dos contos que lambia o machucado do herói caído até ele levantar: combina com cair e
  levantar na cidade, e no outro a sombra dele na parede tem asinhas. "Ari" é curto, igual em pt e en, fácil de gritar.
- **A vila velha**: onde Joaquim (e Maria) nasceram, à beira do Bosque do Zarafshan. Anos atrás uma noite de sombras
  expulsou os moradores; a família se mudou para perto da Vó Edna. As ruínas são o palco da missão pessoal do Joaquim.
- **Tages** não é família, mas vira **vizinho** no pós-jogo (§4).

### Regras de tratamento (como cada um chama o outro)

| Quem fala → com quem | Pode | Nunca |
|---|---|---|
| Irmão(ã) mais velho(a) → mais novo(a) | "maninho/maninha", "pestinha" (Maithe→Ícaro), nome | — |
| Irmão(ã) mais novo(a) → mais velho(a) | "mana" (Ícaro→Maithe, Joaquim→Maria), "Sis" em en | "maninha/maninho" (implica mais novo) |
| Primo → prima/primo | "prima", "primo", nome | "irmã/irmão", "mana" |
| Qualquer um → Lavine | nome; "caçula" só com carinho | "pequena"/"bebê" (ela odeia; exceção: Maithe no pós-jogo, com permissão) |
| Netos → avó | "Vó", "Vó Edna" (en: "Grandma", "Grandma Edna") | "Vovó" |
| Netos → avô | "Vô Jorge" (en: "Grandpa Jorge") | — |
| Crianças → adultos da ilha | "tio/tia" + nome, ou o título do NPC ("Mestre Adamas") | "Seu/Dona", "compadre/comadre" |

---

## 3. Os cinco primos

### Maithe — a mais velha (14), Arqueira dos Ventos · **Vento Vivo**
- **Artefato:** Olho do Vento (vê ameaças invisíveis). Primeiro arco: entalhado pelo Vô de **galho de damasqueiro**.
- **Personalidade:** protetora, competitiva, seca e segura; "conta os primos" sem parar. Implica com o irmão mas morre de
  orgulho dele. Rivalidade amistosa com Maria (os três meses). Confia no **vento** ("meu vento não mente").
- **Voz:** frases curtas, imperativas ("Fica atrás de mim", "Bora"), humor seco. Vocabulário de caça (rastro, mira, vento).
- **Arco:** quer descobrir "quem fez os monstros" e não consegue soltar o controle. Bosque: lidera. Areias: descobre com
  o Ícaro que os guardiões (os ushabtis) foram **construídos** pelos Artífices. Serra: o **Vento Destruidor** (a neblina)
  "mente" para o Olho, e ela aprende a confiar nos primos. Grutas: deixa a Maria ir na frente com a luz. **Epílogo:** a
  resposta é "ninguém. Eram guardiões e bichos que a sombra fez esquecer"; pela primeira vez deixa alguém carregar a
  mochila dela.

### Maria — 13, Curandeira da Luz · **Luz**
- **Artefato:** a luz do **Templo Solar** (fica: é uma "sala de luz" e de cura, não de culto); medalhinha de sol que a Vó
  pendurou no primeiro dia de Templo.
- **Personalidade:** sonhadora que **decide**: foi ela quem sonhou o Cristal apagando, ouviu **o Chamado** e chamou todo
  mundo. Carinhosa, firme, **acende uma lâmpada** pelo irmão. Mão boa para **pão de tâmara**.
- **Voz:** calma, acolhedora ("Respira", "**Vai com luz**"), fala de sonho e luz; dá bronca no Joaquim; chama-o de "maninho".
- **Arco:** acredita que todo bicho "apagado" pode ser aceso de novo (Kök-Böri: "Ele não era mau"). Grutas: acende a
  luz devagarinho para Tages. **Epílogo:** "no sonho tinha cinco primos, e cinco voltaram"; guarda uma lasquinha do
  Cristal (que esfria no começo da Nova Jornada+).

### Joaquim — 12, Guardião das Raízes · **Água Viva**
- **Artefato:** Escudo das Raízes (Eternas), herança da família, grande demais pra ele. "As raízes bebem água": o escudo é
  "o rio que não deixa passar" ("a minha água não deixa passar"). Primeiro escudo: tampa de barril aos seis anos, "pra
  proteger a Maria dos gansos".
- **Personalidade:** teimoso, impulsivo, sempre na frente; o mais velho dos meninos, banca o protetor do Ícaro e da
  Lavine. Come muito (o **plov da Vó**, "sempre repete"). Finge que não está cansado.
- **Voz:** exclamações ("Pode vir!", "Deixa comigo!"), chama a irmã de "mana". Bravata que esconde carinho.
- **Arco:** proteger = ficar na frente → proteger também é **saber sair da frente** (lição da Serra). **Epílogo:**
  encosta o escudo na parede "sem pressa de pegar de volta" e admite: "Ainda bem que vocês estavam atrás de mim. Ou na
  frente."

### Ícaro — 11, Mestre das Máquinas · **Fogo Vivo**
- **Artefato:** drone, torreta, medidor de luz, os radinhos de pilha, o caderno de anotações dele (≠ Caderno da Vó).
  Primeira engrenagem: da máquina de **descascar romã** que explodiu duas vezes, aos cinco anos.
- **Personalidade:** cientista mirim, curioso, falante; quer impressionar a irmã ("Não conta pra Maithe"). Fica louco com
  os **Engenhos da Luz** e os **ushabtis**: acha peças de engenho dos Artífices e as remonta (por isso a **Torreta
  Ushabti** "responde" quando chamada). Sonha em ser "herdeiro de **Kurdalægon**", o ferreiro do céu dos contos.
- **Voz:** números e método ("Viu? Ciência", "Conclusão científica", "Estatisticamente"), chama Maithe de "mana".
- **Arco:** acha que tudo se resolve com invenção → descobre no painel do Grande Ushabti que os Artífices também deixaram
  um **aviso**; entende que a melhor invenção é a família. **Epílogo:** última página do caderno: "Melhor invenção de
  todas: os primos" (escondeu da Maithe, que leu mesmo assim; conversa de estrada `maithe_icaro_last_page`).

### Lavine — 9, a caçula, Feiticeira das Sombras · **Ar Sereno**
- **Artefato:** a **sombra macia** e a **Noite Rubra** (o eclipse em que nasceu); fita vermelha que a Vó amarrou no
  pulso dela "pro escuro saber que ela é nossa". O elemento dela é o mais leve: o ar quieto do céu da noite. A sombra dela
  **não é a Treva**: é sombra macia, de proteger (a Treva é fria, pegajosa e barulhenta).
- **Personalidade:** séria, honesta até demais ("Ele é feio. Eu falo"), quer ser levada a sério ("Eu não sou pequena. Sou a
  mais nova. É diferente."). Tem medo **da própria sombra** no começo. Deu o nome ao **Ari**. Se dá bem com a **Barca da
  Lua** ("A Barca da Lua é o sapato do Kaggen. Eu li.").
- **Voz:** frases curtas, humor seco infantil, contraria quem a protege demais. Chama os outros de "prima/primo".
- **Arco:** medo da noite → "o poder dela é para proteger". Bosque/Areias: friozinho na barriga à noite. Serra:
  dorme no ombro da Maithe na fogueira. Grutas: "o escuro não me assusta" e mostra a Tages que "a sombra também pode ser
  macia". **Epílogo:** volta de cabeça erguida; ninguém mais diz que ela é pequena demais.
- **Segredo opcional:** os **Ouros do Céu** (quatro objetos de ouro que caíram do céu, quentes demais para todos e frios
  só nas mãos **do mais novo**): só a Lavine consegue pegar. Cena de "eu não sou pequena".

---

## 4. Linha do tempo

### Campanha (ciclo 0)
| # | Momento | Gatilho no código | O que acontece / passa a ser sabido |
|---|---|---|---|
| 0 | **Prólogo** "o Cristal apagado" (depois da intro "O Chamado") | `StoryBeat.Prologue` (1º mapa) | Cristal apagado; cada primo tem seu motivo (painel do herói); a Vó dá **Torrão Dourado**, os **radinhos** e pede para contarem cada bicho para o **Caderno**. Partem para o Bosque do Zarafshan. |
| 0b | Ari apresentado | 1º "Fazer carinho" na vila (`CompanionPreferences.Introduced`) | Vó Edna: ele apareceu no portão do pomar no dia em que o Cristal apagou, ainda sem nome. A Lavine: "Ele tem cara de Ari." |
| 1 | Bosque: **Mariam, a Ervateira**; missões dos lobos e aranhas | região Floresta | Bichos "bravos", com pegadas de lama escura (Lodo); ninguém sabe ainda por quê. |
| 2 | **Kök-Böri, o Lobo Celeste** vencido → interlúdio "O Bosque respira de novo" | `InterludeForest` | **Pedrinha escura** na coleira; a faísca sobe (primeiro fio de luz sobre a Vila); Maria: "alguém apagou a luz dele"; areia dourada aponta as Areias Flamejantes (Ari espirra). |
| 3 | Areias: **Ammo, o Caravaneiro** (amigo de estrada do Vô Jorge) | região Deserto | Caravana travada por escorpiões perto do Oásis de Qocho. |
| 4 | **O Grande Ushabti** → "Segredos na areia" | `InterludeDesert` | Ícaro lê o **painel dos Artífices** nas ruínas: um **selo** fecha o fundo das Grutas, **algo faminto de luz** espera atrás dele, os ushabtis foram feitos para vigiar, e o **selo está rachando**. Vó anota pelo rádio. |
| 5 | Serra: **Mestre Mihr, o Mineiro** (Vila de Litema) | região Montanha | Ushabtis de Pedra descem a encosta; o sino da mina calado; a neblina do Vento Ruim. |
| 6 | **Hahya, o Gigante Guerreiro** → "No alto da Serra" | `InterludeMountain` | Ele volta a si, agradece; conta que **quem mora no fundo das Grutas tem medo do escuro** e que a sombra fria vem **de trás** do selo, não dele. Fogueira: Torrão Dourado em cinco (+ um pedacinho pro Ari). |
| 7 | Grutas: **Parthenos, a das Lamparinas** (Vila das Lamparinas) | região Cavernas | Morcegos apagam as lamparinas. |
| 8 | **Tages, o Habitante Ancestral** → Epílogo "a luz volta para casa" + outro + créditos | `Ending` | Tages só estava sozinho e com medo; Maria e Lavine dividem luz e sombra macia; os cinco **refazem o selo** e a Treva **recua** (o Príncipe das Trevas aparece só como silhueta que se afasta, pela voz do narrador); o Cristal acende e a Coluna da Glória sobe. Arcos fecham; Vó, Ari e a cadeira do Vô Jorge no iwan. |

Missões pessoais (vila, `VillageNpcCatalog`) e a Convocação da Maria (`quest_convergence_the_gathering`) correm em
paralelo: Joaquim (vila velha/Bosque), Ícaro (Ruínas dos Artífices/Areias), Maithe (ushabtis marcados/Serra), Maria
(fragmento nas grutas da serra), Lavine (Enxame de Faíscas Presas da Noite Rubra/Grutas) → ponto de encontro → **Tages
"em grupo"** (objetivo `face_the_ancient_dweller`, `enemy_cave_dweller`). A trilha termina onde a ordem das regiões
termina (Bosque → Areias → Serra → Grutas): a última luta da Convocação é a mesma do epílogo; um chefe vencido antes de
aceitar a missão é creditado (`QuestManager.CreditDefeatedUniqueEnemies`). A fala da Maria no ponto de encontro cita só o
que o prólogo libera ("o frio vinha do fundo das Grutas"), porque as Grutas abrem por vitórias, não pelo interlúdio.

### Pós-jogo (ciclo 0 completo)
- NPCs de região cumprimentam com as falas "depois do chefe" (`greet_after_*`).
- Tages vira "vizinho com medo do escuro" que pede lamparina emprestada (Parthenos, conversas de estrada).
- Lembranças do Vô Jorge, Caderno completo, conquistas; botão **Nova Jornada+**.

### Nova Jornada+ (ciclo 1+)
- Prólogo trocado por "o Cristal pisca de novo" (`story.new_journey.*`): poucos dias depois do Bema; Tages manda recado
  **por um vaga-lume**: sombras mais antigas acordaram com o barulho da luz voltando (gancho para Ohya e os arcontes; o
  Príncipe das Trevas continua só uma sombra que nunca é vencida).
- Maithe "conta os primos quatro vezes"; lasquinha da Maria esfriou; Ícaro: "Experimento número dois"; Lavine passa
  primeiro pelo portão do pomar.
- História, missões de região e chefes recomeçam (`StoryProgress.BeginNewJourney`); o que foi ouvido nas conversas de
  estrada continua ouvido, e entram as conversas exclusivas da Nova Jornada+ (`MinCycle = 1`).

---

## 5. Quem sabe o quê, e quando

| Fato | Quem sabe primeiro | Todos sabem a partir de |
|---|---|---|
| O conto do Vô (Primeiro Viajante, cinco luzes, Barcas, "quando a luz chama, a gente responde"); cada primo "tem uma luz" | todos (desde pequenos) | sempre (é conto de família, não revelação) |
| Cristal apagado; luz espalhada em faíscas; sombra vem das Grutas | todos (Maithe pelo Olho, Maria pelo sonho do Chamado, Ícaro pelo medidor, Lavine pelo frio) | Prólogo |
| Ari chegou no dia do apagão; foi a Lavine quem deu o nome | Vó Edna / Lavine | apresentação do Ari |
| Ari talvez seja um aralez | ninguém confirma; a Vó conta o conto dos aralezes | fica no ar (nunca afirmado) |
| Os chefes estão "apagados", não são maus; vencer solta a faísca | Maria (intuição) | Interlúdio do Bosque |
| Existe um selo, guardiões e algo faminto de luz atrás dele | Ícaro (painel dos Artífices) | Interlúdio das Areias |
| Os ushabtis foram **construídos** como guardiões | Maithe (missão pessoal, estudioso) | Interlúdio das Areias |
| Quem mora nas Grutas tem medo do escuro; a sombra vem de trás do selo | Hahya | Interlúdio da Serra |
| Tages não é monstro, só está sozinho; a Treva foi empurrada de volta, não vencida | os cinco | Epílogo |
| Novas sombras antigas acordaram | Tages (vaga-lume) → Vó Edna | Prólogo da Nova Jornada+ |

Regra prática: **uma fala gateada por estágio só pode citar o que a coluna "Todos sabem" já liberou**. As conversas de
estrada usam exatamente esses estágios (`RoadChatterStage` = interlúdios vistos). Falas de NPC não gateadas (saudações
normais) não citam nada além do prólogo. **O nome "Príncipe das Trevas" é do narrador e dos contos antigos**: os primos
dizem "a Treva" ou "a sombra fria", e só raramente, com peso, o nome inteiro (o gancho do selo e o mito ficam com o
narrador).

---

## 6. Motivos recorrentes

- **Cristal do Sol**: a luz da vila; apaga e se espalha em faíscas (prólogo), acende (epílogo), pisca (NG+). Lasquinha da
  Maria = memória dele.
- **O Chamado e a Resposta**: o sonho da Maria chama, os cinco respondem; os ushabtis "respondem"; nos Hinos do Bema o
  Mestre Xroshtag chama e o jogador responde. "Quando a luz chama, a gente responde."
- **Faíscas e a Coluna da Glória**: cada chefe libertado solta uma faísca que sobe; um fio de luz no céu do mapa a cada
  região; a Coluna inteira no outro.
- **Vó Edna**: o tandir (Cozinha), Baú, Caderno, Uasamonga, beijo na testa, "Fiquem juntos", chá e Torrão Dourado. Recebe
  as notícias pelo **radinho** e anota.
- **Caderno da Vó Edna** (bestiário): cada bicho vencido, contado pelo rádio. Cita o Vô Jorge ("lobo respeita quem não
  corre"). As páginas dos chefes falam do que eles **eram** antes da sombra (Kök-Böri guiava viajantes no tugai; o Grande
  Ushabti vigiava o selo; Hahya virou guardião da serra depois de sonhar com destruição e se arrepender).
- **Radinhos de pilha**: feitos pelo Ícaro com as sobras do rádio da Vó; ligam primos e Vó durante a viagem.
- **Ari**: chegou no apagão; fareja a areia dourada; ganha pedacinho de Torrão Dourado na fogueira; corre na frente no
  epílogo; a sombra com asinhas no outro. (O companheiro pode ser desligado nas opções; na história ele existe sempre.)
- **Torrão Dourado** (o doce que era "rapadura"): briga pelo último pedaço (prólogo, canivete do Vô), dividido em cinco na
  fogueira, pote cheio no fim. "Nos contos, doce guarda luz do sol."
- **Contar os primos**: tique da Maithe; "uma, duas, três vezes" → "quatro" na NG+.
- **A cadeira de balanço vazia do Vô Jorge**: no iwan, no epílogo e no outro; no Bema vai para a praça, perto da cadeira
  grande e vazia dos cinco degraus. Nunca dito com peso: "a cadeira dele fica ali, e a gente canta perto".
- **Relíquias de família**: Olho do Vento (Maithe), Escudo das Raízes (Joaquim), luz do Templo e lasquinha (Maria), caderno
  e engrenagens (Ícaro), Noite Rubra e fita vermelha (Lavine); e as **Lembranças do Vô Jorge** (flauta ney, cartas para a
  Vó Edna, gorro de feltro, sino da camela Teimosa, canivete, pião de Maria e Joaquim, retrato dos cinco). Textos de
  história não podem **depender** de uma lembrança escondida ter sido achada (o epílogo cita a cadeira de balanço do Vô,
  não a flauta).

---

## 7. Glossário da Ilha

Palavras que **saíram** (sertão, sítio, porteira, tropeiro, rezadeira, cangaço, arraiá, Caramelo, Zefa, Chico, "tu tem",
"Ô de casa!", "Deus o tenha"...): lista completa na bíblia do mundo §4.2. Tabelas de-para de nomes: §3.

| Palavra | Sentido (como explicar à criança) | Onde aparece |
|---|---|---|
| Ilha da Aurora / *Dawn Isle* | o mundo do jogo ("onde o sol nasce") | tutorial, créditos, mapa |
| a Luz / a Treva | os dois lados do conto; a Treva é fria, pegajosa e barulhenta | história, Caderno |
| faísca / *spark* | luz presa num bicho, pedra ou guardião; sai quando ele é vencido | tudo |
| apagado / acender | bicho ou guardião com a luz presa e a memória esquecida / devolver a luz | chefes, Maria |
| o Chamado / a Resposta | o sonho da Maria / os cinco que responderam | intro, outro, Bema |
| a Mistura | por que o mundo tem luz e sombra juntas (usar pouco) | Caderno, Uasamonga |
| Coluna da Glória / *Column of Glory* | o caminho de luz por onde as faíscas sobem | outro, mapa |
| Barca do Sol / Barca da Lua | as barcas de luz da viagem rápida (dia / noite) | Cais da Barca |
| os Artífices | o povo antigo que fez os engenhos, os ushabtis e o selo | Ícaro, Areias |
| ushabti | guardião construído; quer dizer "o que responde" | Areias, Serra, Ícaro |
| Engenhos da Luz | as máquinas antigas dos Artífices que o Ícaro remonta | Ícaro |
| arconte | gigante de conto que engoliu luz demais (só chefes de evento) | chefes do mundo |
| Āz | a capitã cobiçosa da invasão (na UI, "Az" se a fonte não tiver o "ā") | invasão |
| o Príncipe das Trevas | a sombra grande sem rosto atrás do selo; nunca vencido | narrador, raramente |
| iwan | o alpendre fundo de arco ("a varanda da Vó"); explicar na 1ª vez | casa da Vó, créditos |
| tandir | o forno de barro ("o forno da Vó") | Cozinha |
| Uasamonga | a taça-caldeirão de cobre da Vó que "sabe quem merece" (antigo Tacho) | refino |
| pomar | o pomar da Vó (damasco, romã, amora, uva) | história |
| caravana / caravançará | grupo de viajantes com camelos / pousada de caravana | Areias, Ammo |
| Teimosa | a camela bactriana do Vô Jorge | lembranças |
| aralez | o cão-espírito dos contos que lambe o machucado do herói até ele levantar | Ari (só insinuado) |
| Bema | o festival da cadeira vazia nos cinco degraus (junho) | festival |
| lâmpada / lamparina / Lâmpada Guardiã | luz de cuidado ("acendemos uma lâmpada por você"), sem prece | Parthenos, Ramrat |
| Tumar | amuleto triangular de pano bordado da Vó (antigo Patuá) | proteção |
| plov, samsa, nan, pão de tâmara, chá de karkadé, bolo de damasco | comidas da Vó | Cozinha, NPCs |
| Torrão Dourado | o doce dourado em tijolo (antigo "rapadura" prato) | Cozinha, história |
| Ar Sereno, Vento Vivo, Luz, Água Viva, Fogo Vivo | as cinco luzes, uma por primo (§1.2) | falas, VFX |
| Noite Rubra | o eclipse vermelho em que Lavine nasceu (**nome próprio, igual em en**) | Lavine |
| Olho do Vento, Escudo das Raízes, Templo Solar | nomes próprios; en: Eye of the Wind, Shield of Roots, Solar Temple | heróis |
| Mestre / Mestra, tio / tia | tratamento de ofício / como as crianças chamam os adultos | NPCs |
| "Tem alguém em casa?", "Licença!" | saudação ao chegar numa casa | NPCs |
| "Boa estrada", "Volta com luz", "Vai com luz" | despedidas | NPCs, Maria |
| caçula | o neto mais novo | Lavine |
| mana / maninho(a) | irmã / irmãozinho(a), ver regras de tratamento | irmãos |

---

## 8. Mapa das fontes de texto

Nomes de arquivo, classes e chaves **não mudam** com o re-tema (ex.: `CarameloTexts` guarda as falas do Ari). Quem
reescreve cada pasta está na bíblia do mundo §5.

| Arquivo | Batidas / conteúdo | Gatilho |
|---|---|---|
| `Gameplay/Story/StoryTexts.cs` + `StoryScript.cs` | prólogo, 3 interlúdios, epílogo (painel por herói), prólogo da Nova Jornada+ | `StoryBeat` vistos, ciclo NG+ |
| `Gameplay/Quests/RegionQuestTexts.cs` + `Gameplay/Dialogue/RegionNpcDialogues.cs` | Mariam, Ammo, Mestre Mihr, Parthenos: saudação por primo, 2 missões, dica do chefe (fala especial de um primo), "o chefe caiu", **saudação depois do chefe** | `ActiveCharacter`, estado das missões, flags `boss_ready`/`region_cleared` |
| `Gameplay/Dialogue/QuestGiverTexts.cs` + `Core/Localization/LocalizationManager.cs` (`quest.*`/`dialogue.*` das missões pessoais) + `Resources/Dialogue/*.asset` + `Resources/Quests/*.asset` + seeders `Editor/*DialogueSeeder*.cs` | missões pessoais dos 5 (vila velha, Templo, ushabtis marcados, Ruínas dos Artífices, Noite Rubra), Convocação e primeiro encontro dos cinco | vila (`VillageNpcCatalog`), estado das missões |
| `Gameplay/Dialogue/VillageNpcCatalog.cs` | quem é quem na vila (um NPC por primo + ponto de encontro) | hub "Vila" |
| `Gameplay/Pets/CarameloTexts.cs` (+ `CarameloCuddle.cs`) | apresentação do **Ari** pela Vó (e o nome dado pela Lavine), 2 falas de carinho por primo | botão "Fazer carinho" na vila |
| `UI/WorldMap/KitchenTexts.cs` | Vó Edna no tandir: saudação e aula por primo, pratos favoritos | hub da vila → Forno da Vó Edna |
| `UI/WorldMap/WorldEventTexts.cs` (+ `Gameplay/World/Events/WorldEventCatalog.cs`) | 8 eventos de estrada com fala do primo ativo (caravana ambulante, chuva de damascos, cantores na estrada, histórias de Ananse na fogueira, tempestade de areia, neblina do Vento Ruim, eco na caverna...) | viagem no mapa (região, hora) |
| `Gameplay/World/Chatter/RoadChatterCatalog.cs` + `RoadChatterTexts.cs` | **Conversas entre primos na estrada** (46 trocas, todos os pares) | viagem; estágio = interlúdios vistos; ciclo NG+ |
| `UI/Bestiary/BestiaryTexts.cs` | Caderno da Vó Edna: página e comentário de cada primo por bicho/chefe | inimigo derrotado |
| `UI/Achievements/AchievementTexts.cs` | conquistas (pessoais, irmãos, família, campanha, NG+, Caderno) | `AchievementManager` |
| `UI/WorldMap/NewGamePlusTexts.cs` | convite da Vó para a Nova Jornada+ por primo | botão Nova Jornada+ |
| `Gameplay/Secrets/SecretTexts.cs` | segredos do mapa, Lembranças do Vô Jorge e os Ouros do Céu (opcional) | segredos/arena |
| `Gameplay/Characters/*.cs` + `Gameplay/Content/ContentTexts.cs` | lore da seleção de herói, descrições das habilidades | tela de seleção, árvore de skills |
| `UI/CharacterSelect/HeroFamily.cs` | linha de parentesco ("Irmã do Ícaro · a mais velha") | seleção |
| `Gameplay/Quests/DailyQuestTexts.cs` | missões diárias da vila (falas da família: "a Vó Edna chamou para o chá") | quadro de diárias |
| `UI/Tutorial/TutorialTexts.cs` | dicas de primeira vez | `TutorialManager` |
| `UI/Common/UiTexts.cs` (Credits*) + `UI/Menus/CreditsContent.cs` | créditos: "Os primos", agradecimento, iwan da Vó | após o epílogo / menu |
| `Gameplay/Story/CutsceneScript.cs` + `CutsceneTexts.cs` | intro "O Chamado", outro "A Coluna da Glória", ferrões de chefe (§10) | novo jogo, fim, banner de chefe |
| `Docs/WorldBible_Maniqueismo.md` | tabelas de-para do re-tema e fontes de cada nome | — |
| `Docs/GDD.md`, `TODO.md` (Roster) | resumo de design e lore longa | — |

---

## 9. Conexões entre sistemas (feitas) e pendências

Os registros abaixo são históricos; os nomes visíveis já estão no tema novo (ids iguais).

**Feitas (2026-10-29):**
- Prólogo: Vó entrega os **radinhos** e pede que contem os bichos para o **Caderno** (motiva o bestiário e o rádio).
- Interlúdios: Ari fareja a areia (Bosque); Vó anota pelo rádio (Areias); pedacinho de Torrão Dourado pro Ari (Serra).
- Epílogo: cada painel de herói fecha o arco (Olho do Vento sem ameaça e a resposta da Maithe; sonho dos cinco da Maria;
  escudo encostado do Joaquim); iwan com Caderno, Ari e a cadeira do Vô Jorge.
- NPCs de região: **saudação depois do chefe** por primo (24 falas, flag `region_cleared`); Ammo conheceu os primos na
  casa da Vó e viajou em caravana com o Vô Jorge; Mestre Mihr recebeu recado do Ícaro pelo Ammo; Parthenos recebeu as
  lanternas do Ícaro pelo Mestre Mihr.
- Caderno: cita o Vô Jorge; páginas de Kök-Böri e de Hahya lembram o que aconteceu nos interlúdios.
- Missões pessoais reconciliadas com a campanha: "vila velha" (não a vila da Vó), ushabtis = guardiões construídos,
  Hahya = apagado (não o mandante).

**Feitas (2026-10-30):**
- **Região de onde o primo voltou** = região do último interlúdio visto (`StoryBeats.LastRegionCleared`: Floresta,
  Deserto, Montanha; o epílogo conta como Cavernas). Base das três conexões abaixo.
- **Cozinha da Vó lembra a região:** `KitchenPresenter.GrandmaLine(..., cameFrom)` usa
  `ui.kitchen.grandma.after_<região>_<primo>` (20 falas) ou `after_<região>` sem primo; a aula de prato favorito ainda
  vence. Na sessão, a 1ª, 3ª, 5ª... abertura do forno lembra a viagem e as outras mantêm a saudação de sempre
  (`KitchenPresenter.CameFromForVisit`).
- **Ari lembra a viagem:** `pet.caramelo.cuddle.<primo>.after_<região>` (20 falas) entra no rodízio de carinho
  em primeiro lugar (fala da etapa → fala 0 → fala 1 → ...).
- **Condição de diálogo `StoryBeatSeen`** (`DialogueConditionType` = 4, chave = id do beat, ex. `interlude_desert`):
  lida por `IStoryDialogueContext` (`QuestDialogueContext` → `StoryProgress`). Os cinco NPCs da vila ganham a opção
  "Soube das novidades da estrada?" depois do interlúdio do Bosque, com uma fala por beat (o mais novo vence):
  `VillageStoryNews` / `VillageStoryNewsTexts` (20 falas), montado em cima do asset sem editá-lo.
- **Conversas de estrada repetem:** quando todas as elegíveis já foram ouvidas, repetem a ouvida há mais tempo, com o
  mesmo cooldown (`RoadChatterRules.Pick(..., heardRank)`, ordem salva em `RoadChatterSaveState.heardOrder`).
- Convocação termina em Tages (ver §4); Maithe e Maria em anos diferentes (§2).
- **Re-tema maniqueísta aprovado** (`Docs/WorldBible_Maniqueismo.md`): mundo = Ilha da Aurora; Vó Edna / Vô Jorge;
  Caramelo → Ari; Príncipe das Trevas = vilão final invencível (empurrado de volta, nunca vencido); chefe do mundo das
  Cavernas = Mahaway. Esta bíblia narrativa foi reescrita a partir dela (onda 0).

**Pendências de texto:**
- **Onda 1 do re-tema** (código e assets, por pasta; bíblia do mundo §5): até ela fechar, o jogo ainda mostra nomes antigos
  (Zefa, Chico, Caramelo, sertão, Lobo Alfa...). Os textos novos devem seguir este documento.
- `SecretTexts.PanelSubtitle`: "pela ilha" / "around the island" está **certo** (o mapa é a Ilha da Aurora; ver §1).
- `SecretTexts` (Fita vermelha da Lavine, en): "Noite Rubra" já padronizado.
- Confirmar fontes marcadas **[verificar]** na bíblia do mundo (Uzzai, *mehrmohre*, *արի* "vem!") antes de publicar.

---

## 10. Cutscenes (intro, outro, ferrões de chefe)

Código: `Gameplay/Story/CutsceneScript.cs` (lista de planos em C# puro, sem ScriptableObject), `CutsceneSequence.cs`
(relógio, regras, câmera), `CutsceneTexts.cs` (en + pt-BR), apresentador `UI/Story/CutscenePlayerController.cs`, visual
`Resources/UI/Cutscene.uss`. **Sem Timeline:** `com.unity.timeline` não está no `Packages/manifest.json` e o mapa-múndi é
montado em código (não há cena para autorar uma Timeline), então um player pequeno guiado por dados move a `Camera.main`
entre enquadramentos (órbita: foco, yaw, pitch, distância, FOV) ancorados nos nós de região do mapa (Vila = nó da Floresta).

**Moldura mítica (maniqueísta, em tom de conto infantil, não doutrina):** Luz × Treva; faíscas de luz presas na matéria
(bichos, pedras, guardiões) que os primos libertam; o **Chamado** (o sonho da Maria) e a **Resposta** (os cinco); a sombra
do **Príncipe das Trevas** escorrendo pela fresta do selo das Grutas do Limiar (só silhueta das cinco formas juntas,
bíblia do mundo §1.4, nunca nítida); a **Coluna da Glória** por onde a luz liberta sobe no fim. Quem
conta o mito é o **Vô Jorge** (lembrado pela Vó Edna); o coração humano são os cinco primos, a Vó Edna e a cadeira do Vô
no iwan.

| Cutscene | Quando | Planos (resumo) |
|---|---|---|
| **Intro "O Chamado"** | Novo jogo (ciclo 0), **antes** dos painéis do prólogo | Ilha da Aurora inteira → chegada na casa da Vó, pelo portão do pomar (Lavine "cheguei primeiro", Joaquim, Maithe conta os primos) → iwan (Vó Edna, chá e bolo de damasco) → Ícaro nota o Cristal apagado → a câmera cruza o mapa até as Grutas do Limiar: a porta selada velha racha (o **gancho do selo**, só narrado; os primos ainda não sabem do selo) e a luz vira mil faíscas presas → Maria: o sonho chamando → Vó Edna: "o Vô Jorge dizia: quando a luz chama, a gente responde". Tinta de sombra cresce no gancho. |
| **Outro "A Coluna da Glória"** | Depois dos painéis do epílogo, **antes dos créditos** | Ilha acesa, a luz sobe em coluna e a sombra recua para trás do selo refeito (narrador: "empurrada de volta, não vencida") → Grutas (Lavine se despede de Tages, "vizinho") → Serra/Areias (Ícaro: medidor no máximo) → Bosque/Vila (Joaquim, bolo de damasco; Maithe conta seis com o Ari, cuja sombra na parede tem asinhas) → **iwan da Vó Edna** (Maria: "ouvimos o chamado e respondemos"; Vó Edna; narrador: a cadeira de balanço do Vô Jorge e a Coluna da Glória). Termina no iwan, como os créditos. |
| **Ferrão de chefe** | Banner "Chefe da região" (`BossIntroBannerController`) | Linha extra sob o nome do chefe (Kök-Böri, o Grande Ushabti, Hahya, Tages): a fala do **primo ativo** por região (`cutscene.stinger.<região>.<herói>`), ou uma linha do narrador sem herói. Fica +1,5 s no banner. |

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
- Falas de cutscene seguem a §5: personagens só citam o que já foi liberado; o gancho do selo, o Príncipe das Trevas e o
  mito ficam com o narrador.
