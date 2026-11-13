# Rapadura — Bíblia do Mundo: a Luz Presa (re-tema maniqueísta)

> **Status:** aprovada pelo usuário em 2026-10-30 (decisões em §6). Ainda não aplicada ao código.
> **Escopo:** o mundo, a história e o conteúdo **visível** saem do tema sertão/folclore brasileiro e entram no tema da
> **mitologia maniqueísta**. A base é o mundo da Rota da Seda onde o maniqueísmo realmente existiu, com mitologias pouco
> conhecidas por cima: sogdiana, uigur, tocária, parta, Mingjiao chinesa, zurvanita, elamita, hurrita, osseta (Nartes),
> armênia, georgiana, cita, komi, guanche, etrusca e **africanas antigas** (kushita/núbia, Kemet, San, dogon, bambara,
> akan, malgaxe, costa suaíli, basotho/zulu).
> **Não muda:** os 5 primos e a família, a **Vó Edna** (que cuida da vila) e o **Vô Jorge** (o avô, lembrado com carinho),
> e **todos os ids internos** (`CharacterId`, `enemy_*`, `mount_*`, `casa_*`, `roupa_*`, `relic_*`, chaves de localização,
> nomes de classe C#, chaves de save). Só mudam os **textos que o jogador vê** (en + pt-BR) e os documentos.
> **Fonte de verdade:** quando este documento for aprovado, `Docs/Narrative.md` deve ser reescrito a partir dele.

Sumário: 0. Regras de tom · 1. Cosmologia · 2. Mapa do mundo · 3. Tabelas de-para · 4. Guia de reescrita ·
5. Ordem de implementação · 6. Decisões do usuário · 7. Fontes

---

## 0. Regras de tom (valem para tudo abaixo)

1. **É conto de fadas, não doutrina.** O mito é o pano de fundo das histórias que o **Vô Jorge contava na varanda** e que a
   **Vó Edna** continua contando. Ninguém no jogo reza, prega, se converte ou diz qual fé é a certa. As palavras "religião",
   "pecado", "salvação", "igreja", "templo de fé", "deus" e "profeta" não aparecem em falas.
2. **Nenhuma figura sagrada viva ou fundadora aparece como personagem.** Mani, Jesus, Zoroastro, Buda, Maomé, orixás,
   voduns, Nyame, Amma, Unkulunkulu, Mawu-Lisa e outras divindades de fés vivas **não aparecem, não falam e não são
   inimigos**. O Pai da Grandeza (o "lado da Luz" mais alto) também não aparece: no jogo ele é só "a Luz".
3. **Os NPCs são gente comum com nomes dos contos.** Como uma criança que recebe o nome de um herói antigo, o ferreiro
   **Adamas** tem o nome do guerreiro de luz do mito, mas é só um ferreiro bonachão. Isso deixa os nomes reais sem
   transformar ninguém em deus.
4. **Ninguém é mau de verdade, exceto a Treva sem rosto.** Bichos e chefes estão **apagados** (com a luz presa e esquecidos de
   quem são). Vencer é **libertar a faísca** e devolver a memória. Os arcontes são tratados como gigantes de conto
   (gulosos, briguentos, vaidosos) e nunca de forma assustadora demais. O **Príncipe das Trevas** é uma sombra grande, sem
   sangue e sem gore.
5. **Culturas vivas com respeito.** Inspiração mítica, nunca paródia. Nada de "tribo exótica", nada de sotaque de
   brincadeira e nada de ritual de fé viva virando minijogo. Inimigos e chefes **nunca** recebem o nome de um povo real
   (veja §2.5).
6. **Calor de família primeiro.** O coração do jogo continua sendo os cinco primos, a Vó Edna e a cadeira de balanço vazia
   do Vô Jorge.

---

## 1. Cosmologia para o jogo

### 1.1 As duas Terras e os Três Tempos

| Conceito no jogo (pt-BR / en) | O que é para a criança | Fonte (uma linha) |
|---|---|---|
| **A Terra da Luz** / *the Land of Light* | Lá em cima e longe: tudo claro, calmo, cheiroso, musical. Nunca visitada no jogo. | Princípio da Luz no mito maniqueísta (Kephalaia; Teodoro bar Konai, *Scholion* XI). |
| **A Terra da Treva** / *the Land of Darkness* | Lá embaixo: fumaça, barulho, gente que só quer mais. As Grutas do Limiar são a borda dela. | Princípio da Treva; as "cinco covas" da Treva (Teodoro bar Konai). |
| **Os Três Tempos** / *the Three Times* | **Tempo de Antes** (as Terras separadas) → **Tempo do Meio**, ou **a Mistura** (agora: luz presa nas coisas, é o jogo) → **Tempo de Depois** (a luz volta para casa pela Coluna). | Doutrina dos "três tempos" (*sẖ zmn* / *three moments*), fórmula parta e chinesa (*Compêndio* Mingjiao, 731). |
| **A Mistura** / *the Mixture* | Por que o mundo tem luz e sombra juntas: um pouco de luz ficou presa em cada bicho, pedra, fruta e guardião. | Mistura (*gumēzišn*, médio-persa). |
| **Faíscas** / *sparks* | A luz presa. Quando um bicho apagado é vencido, a faísca sai, sobe e ele volta a ser ele mesmo. É a palavra do jogo para "luz libertada". | "Partículas de luz" / Alma Viva presa na matéria (Agostinho, *Contra Faustum*; Kephalaia). |
| **A Alma Viva** / *the Living Soul* | O nome bonito para todas as faíscas juntas. Usado com parcimônia (nome de pedra de refino, §3.7). | *Nafšā ḥayyā* (siríaco) / *grīw zīndag* (médio-persa). |
| **Relógio de Zurvan** / *Zurvan's Clock* | O relógio de sol da praça da Vila, que marca dia, entardecer e noite. Zurvan é o "Tempo sem fim" dos contos persas. | Zurvan: o Tempo nos textos zurvanitas; os maniqueus persas usavam "Zurvan" para o Pai da Grandeza (por isso é só um relógio, nunca uma pessoa). |

### 1.2 O Chamado e a Resposta (o motor da história)

No mito, o **Homem Primordial** desce para enfrentar a Treva vestido com os **cinco elementos de luz**. Ele perde, adormece
no fundo e as cinco luzes são engolidas. Então o **Espírito Vivo** grita, e o grito vira um ser, o **Chamado**
(*Xrōštag*). O Homem Primordial responde, e a resposta vira outro ser, a **Resposta** (*Padwāxtag*). Chamado e Resposta
sobem juntos.

**No jogo:**
- O **Chamado** é o sonho da Maria (a intro "O Chamado", já escrita): o Cristal do Sol apagou, a luz se espalhou em mil
  faíscas, e o sonho chama.
- A **Resposta** são os cinco primos. A frase-tema do Vô Jorge, "**quando a luz chama, a gente responde**", fica como está:
  ela é a moral da família.
- O Homem Primordial **não é personagem**. É o "**Primeiro Viajante**" das histórias do Vô Jorge ("um que desceu primeiro,
  com cinco luzes no peito, e dormiu lá embaixo"). Os primos não são ele: são **os que responderam**.
- Ponte bonita: os guardiões antigos chamam-se **ushabtis**, palavra egípcia que quer dizer **"os que respondem"**
  (§1.6). Os guardiões foram feitos para responder ao chamado da luz e esqueceram como.

Fonte: *Xrōštag* e *Padwāxtag* (Chamado e Resposta), textos partas e médio-persas de Turfan (M 7, M 10); Teodoro bar
Konai; *ushabti* < egípcio *wšb* "responder" (Livro dos Mortos, cap. 6).

### 1.3 Os Cinco Elementos de Luz × os cinco da Treva (e os cinco primos)

No mito, as cinco luzes eram a armadura e os "filhos" do Homem Primordial (*panj amahraspand*). No jogo, **cada primo
carrega a afinidade de um elemento**, e cada região guarda presa uma dessas luzes. É assim que "cinco primos" vira mito sem
virar doutrina: as luzes eram cinco, e quem respondeu também.

| Elemento de luz | Contraparte da Treva | Primo | Por quê | Região (onde a luz está presa) | Visual (VFX/UI) |
|---|---|---|---|---|---|
| **Ar Sereno** (éter) / *Pure Air* | **Fumaça** / *Smoke* | **Lavine** | A sombra macia e o céu da noite são "ar quieto". Ela é a caçula e o elemento mais leve. | Vila / céu (a Coluna) | lilás e prata, estrelinhas |
| **Vento Vivo** / *Living Wind* | **Vento Destruidor** / *Ruinous Wind* | **Maithe** | O Olho do Vento. | Serra de Kaggen (Montanha) | branco-azulado, rajadas |
| **Luz** / *Light* | **Treva** / *Darkness* | **Maria** | O Templo Solar, o sonho, a cura. | Grutas do Limiar (Cavernas) | dourado-claro |
| **Água Viva** / *Living Water* | **Lodo** / *Mire* | **Joaquim** | As raízes bebem água. O escudo é "o rio que não deixa passar". | Bosque do Zarafshan (Floresta) | verde-água |
| **Fogo Vivo** / *Living Fire* | **Fogo Devorador** / *Devouring Fire* | **Ícaro** | As máquinas, o relâmpago, a forja. | Areias Flamejantes (Deserto) | laranja-cobre, faíscas |

Fonte: os cinco elementos luminosos (ar/éter, vento, luz, água, fogo) e os cinco tenebrosos (fumaça, fogo devorador, vento
destruidor, lodo/água turva, treva), em Teodoro bar Konai e nos Kephalaia coptas. A ligação primo ↔ elemento é invenção
nossa.

> Regra prática: o elemento é **sabor**, não sistema novo. Ele serve para cor de VFX, nome de set, falas ("meu vento",
> "a minha água não deixa passar") e o tema de cada região. Não cria tipos de dano novos.

### 1.4 Os arcontes (os "gulosos" da Treva)

Arconte é um gigante ou demônio de conto que **engoliu luz demais** e não quer devolver. São os vilões de evento (chefes do
mundo, invasão). Os chefes de região **não** são arcontes: são guardiões apagados (§3.3).

| Arconte | No jogo | Fonte |
|---|---|---|
| **Saklas** | Chefe do mundo da Floresta: tomou o corpo de um lobo. Vaidoso, uiva para se ouvir. | Saklas/Ašaqlūn, arconte que molda Adão no mito (Teodoro bar Konai; *Saklas* também nos textos de Nag Hammadi). |
| **Namrael** | Chefe do mundo do Deserto: tomou um escorpião-rei. Parceira de Saklas, ciumenta. | Namraēl/Nebrōēl, a arconte companheira de Saklas (Teodoro bar Konai). |
| **Mazan** (os Mazans) | Chefe do mundo da Montanha (já no código): gigantes de pedra briguentos. | *māzan* / *mazanān*, "gigantes, monstros" nos textos médio-persas maniqueus. |
| **Āz** | Capitã da Invasão: a **Cobiça**. Quer tudo para ela, enfeitiça os bichos e "esconde" a luz em arcas. | Āz, demônia da cobiça e mãe dos demônios nos textos médio-persas (M 7980 etc.). |
| **Hahya** e **Ohya** | Dois irmãos gigantes do *Livro dos Gigantes*. **Hahya** é o chefe da Montanha (apagado, volta a si). **Ohya** (2026-11-09) é o chefe do mundo da Estepe dos Nartes (mapa 52, §3.4.1): ainda não voltou a si, acorrentou-se à montanha "para não brigar" e briga com quem sobe; vencido, desce para morar com o Hahya. | *Livro dos Gigantes* maniqueu (fragmentos de Turfan e Qumran: ʾOhyā e Hahyā). |
| **Mahaway** | Chefe do mundo das Cavernas (**decidido**, ver abaixo): o gigante que **voa** como redemoinho, agora "Rei da Revoada". | Mahaway, o gigante mensageiro que voa, no *Livro dos Gigantes*. |
| **O Príncipe das Trevas** (Ahriman, nos textos persas) | **O vilão final, sem rosto.** É a sombra que escorre das Grutas. Tem cinco formas juntas (cabeça de leão, asas de águia, corpo de dragão, cauda de peixe, pés de demônio), mostradas só em silhueta. | Descrição do "Rei da Treva" em Teodoro bar Konai e nos Kephalaia (*Kephalaion* 6); "Ahriman" nos textos médio-persas maniqueus. |

**Decisão (aprovada pelo usuário):** o chefe do mundo das Cavernas deixa de ser "Príncipe das Trevas, Rei da Revoada" e
passa a ser **"Mahaway, o Rei da Revoada"** (forma da noite: **"Mahaway, o Morcegão da Treva"**). O Príncipe das Trevas fica
guardado como o **vilão final** que nunca aparece como chefe vencível na campanha. No fim da campanha ele é **empurrado de
volta** (selo refeito), não destruído. No online ele pode virar uma **raide de servidor** ("A Grande Treva"), que nunca
"morre", só recua por uma temporada. Isso também tira a duplicidade "Ahriman = Príncipe" que existia antes, quando o chefe
da Montanha se chamava Ahriman (hoje Mazan).

### 1.5 A Coluna da Glória, as Barcas de Luz e o fim

| Conceito | No jogo | Fonte |
|---|---|---|
| **Coluna da Glória** / *Column of Glory* | O caminho de luz por onde as faíscas libertadas sobem. Aparece **acesa** na outro "A Coluna da Glória" (já existe) e, discreta, sobre a Vila depois de cada chefe (um fio de luz no céu do mapa). | *Srōšahrāy* (médio-persa), *Stylos tēs doxēs* (grego; Acta Archelai; Kephalaia). |
| **Barca do Sol** e **Barca da Lua** | No mito, a Lua junta a luz libertada (por isso ela cresce), passa a luz para o Sol, e o Sol leva tudo para casa. No jogo, são a **viagem rápida** (§3.16). | Sol e Lua como "navios de luz" (Kephalaia; Acta Archelai 8). |
| **A Roda das Doze Jarras** | Uma roda d'água gigante com doze jarras que "tira luz do mundo como água do poço". No jogo, é a sala de evento das torres (§3.13). | Acta Archelai 8: a roda com doze vasos (*kadoi*) que sobe as almas. |
| **O Tempo de Depois** | No fim do conto, a luz volta toda e a Treva fica fechada num **novelo** (*bōlos*). No jogo, é só o fim da história do Vô Jorge, nunca mostrado. O epílogo do jogo é uma **vitória pequena**: a vila acesa. | *Bōlos* / "o globo" final (Kephalaia; Homilias coptas). |

### 1.6 Ícaro e a tecnologia perdida: os **Engenhos da Luz**

Ícaro inventa a Torreta, a Granada Elétrica e o Drone "achando tecnologia antiga perdida no mundo". No re-tema:

- No Tempo do Meio, para ajudar a juntar as faíscas, os **Artífices** (o povo antigo que o jogo chamava de "os antigos")
  construíram **engenhos**: os **ushabtis** (guardiões que "respondem"), a **Roda das Doze Jarras** e as **Barcas**.
  Com a Mistura, os engenhos quebraram, se espalharam ou foram **apagados** pela sombra (o Grande Ushabti, os golens).
- Ícaro acha **peças de engenho** (*Engenhos da Luz*) e as remonta. Por isso a Torreta "responde" quando chamada.
- O ferreiro do céu dos Artífices tem um nome de conto: **Kurdalægon**, o ferreiro celeste dos Nartes ossetas, que forjava
  armas e consertava heróis. Ícaro sonha em ser "herdeiro de Kurdalægon" (título da 3ª graduação, §3.19).
- Toque africano: a **lenda dogon** do ferreiro ancestral que desceu do céu num **celeiro celeste**, trazendo a forja e as
  sementes. As peças maiores dos engenhos caíram do céu "como um celeiro". Use só a imagem (o celeiro de barro com degraus);
  sem nomes de divindades dogon.
- Os **Ouros do Céu** (segredo do mapa, opcional): quatro objetos de ouro que caíram do céu (arado com canga, machado,
  taça), quentes demais para os irmãos mais velhos e frios só nas mãos **do mais novo**. Na lenda cita, só o caçula
  Kolaxais consegue pegá-los. No jogo, **só a Lavine** consegue pegar. Dá uma cena ótima de "eu não sou pequena".

Fontes: ushabti/shabti (Kemet, Novo Reino); Kurdalægon (sagas dos Nartes, Ossétia); celeiro celeste do ferreiro (relatos
dogon coletados por Griaule e Dieterlen, versão contestada pela etnografia posterior, por isso só a imagem); ouros do céu
de Targitau e Kolaxais (Heródoto, *Histórias* IV.5–7).

### 1.7 A família como coração humano

| Personagem | Papel no mito do jogo | Nota |
|---|---|---|
| **Vô Jorge** | O **contador**. Tudo o que os primos sabem do mito, ouviram dele na varanda: "o Primeiro Viajante", "as cinco luzes", "as Barcas". Viajou com caravanas (com **Ammo**, o caravaneiro) e escondeu lembranças pelo mapa para os netos. | Lembrado com carinho, nunca com luto pesado. A **cadeira de balanço vazia** ecoa o trono vazio do Bema (§3.15) com delicadeza: "a cadeira dele fica ali, e a gente canta perto". |
| **Vó Edna** | A **âncora e a guardiã do Cristal**. Forno de barro (tandir), Baú, Caderno (bestiário), o Uasamonga (o antigo Tacho, §3.8). Recebe as notícias pelo rádio. Não é sacerdotisa: é avó. | Fala calorosa e neutra (sem "tu tem" nordestino, §4). |
| **Os cinco primos** | **A Resposta.** Cada um com um elemento (§1.3). | Dinâmicas de família intactas (§4.4). |
| **Ari** (antigo Caramelo) | O cachorro cor de caramelo que apareceu no portão **no dia em que o Cristal apagou**. Fica no jogo, mas com **nome novo: Ari** (decidido, §6). Fica no ar que ele é um **aralez**, o cão-espírito armênio que lambe as feridas dos heróis caídos para eles se levantarem. Isso combina com a mecânica de cair e levantar na cidade. Na outro, a sombra dele na parede tem asinhas. Quem deu o nome foi a Lavine ("ele tem cara de Ari"). A Vó Edna conta depois que, no conto do Vô, os aralezes eram "os cachorros que lambiam o machucado do herói até ele levantar". | Fonte: aralez (mitologia armênia; Moisés de Corene, *História* I.15, a lenda de Ara, o Belo). Por que "Ari": *aralez* = *Ara* + *lez-* ("lamber"), "os que lambem Ara"; "Ari" vem das primeiras letras, é curto, igual em pt e en e fácil de gritar; em armênio coloquial *արի* (ari) quer dizer "vem!" **[verificar]**, perfeito para chamar cachorro. Ids e chaves (`CarameloTexts`, `pet.caramelo.*`, `CompanionPreferences`) ficam. |
| **Tages** (o antigo Habitante Ancestral) | O ser antigo das Grutas que **tem medo do escuro** e roubava luz por solidão. Vira vizinho. | Fonte: Tages, o ser com rosto de criança e sabedoria de velho que brotou de um sulco de terra (mito etrusco; Cícero, *De Divinatione* II.50). |

---

## 2. Mapa do mundo

### 2.0 O nome do mundo

- **Decidido:** o mundo deixa de ser "o sertão" e passa a ser a **Ilha da Aurora** (en: *Dawn Isle*). O nome vem de
  *Xwarāsān* (parto/médio-persa: "onde o sol nasce, o Oriente"), a terra para onde o caravaneiro Mār Ammō levou os hinos.
  Nos textos: "a Ilha da Aurora", ou "a ilha" quando se fala do mapa. Nunca "o mundo de Rapadura".
- A ilha é um **mosaico mítico**: cada região mistura duas ou três culturas antigas. Não é um mapa histórico, e nenhuma
  região "é" um país de hoje.

### 2.1 Floresta → **Bosque do Zarafshan** · cidade: **Vila da Vó Edna**

| Campo | Proposta |
|---|---|
| Nome (pt / en) | **Bosque do Zarafshan** / *Zarafshan Woods*. Cidade: **Vila da Vó Edna** / *Grandma Edna's Village* (substitui "Vila da Vó Zefa"). |
| Significado | *Zarafšān* = "o que espalha ouro" (persa), o rio do vale da Sogdiana. As folhas de choupo no outono parecem ouro espalhado. |
| Fonte | Vale do Zarafshan, coração da Sogdiana (Samarcanda, Panjakent), onde mercadores sogdianos levaram o maniqueísmo para o leste. |
| Elemento | **Água Viva** (Joaquim). Lodo como contraparte: os bichos apagados deixam pegadas de lama escura. |
| Culturas | Sogdiana e parta (base), com toques ossetas/georgianos na mata e o **jardim de Mihrshah** (no conto, Mani mostra ao príncipe Mihrshah um jardim de luz; no jogo, o pomar da Vó é "bonito como o jardim do conto"). |
| Arquitetura (kit 3D) | Casas de **adobe** (tijolo cru) cor de damasco, de 1 a 2 pisos, com **teto plano** e parapeito. **Iwan** (o alpendre fundo de arco, que faz o papel da *varanda* da Vó). **Colunas de madeira entalhada** sogdianas (fuste fino, capitel em leque), portas de madeira com relevo de rosetas, pátio interno com **aryk** (canal d'água estreito). Casa de Panjakent como referência: paredes pintadas por dentro com cenas de caça e banquete. |
| Paleta | adobe damasco `#D9A066`, ocre `#B7793F`, madeira de nogueira `#6B4226`, verde-choupo `#7FA650`, turquesa de azulejo (só detalhe) `#3FA7A3`, céu pêssego no entardecer. |
| Vegetação | **Tugai** (floresta de beira de rio da Ásia Central): choupo-turanga (*Populus euphratica*), tamargueira rosa, espinheiro-marítimo laranja, junco. Pomar: **damasqueiro, amoreira (bicho-da-seda), romãzeira, videira em latada, melão no canteiro**. |
| Props | **Tandir** (forno de barro), tapetes e *suzani* bordados no iwan, tear de seda, latada de uva, canal aryk com pontezinha, **Relógio de Zurvan** (relógio de sol na praça), pedestal do **Cristal do Sol**, lanternas de papel (Bema), banquinho baixo (*supa* / *tapchan*, o estrado de sentar), cântaros de cobre. |
| Bichos (sabor) | lobo, javali do tugai, veado-de-bukhara, faisão, cegonha no telhado. |

### 2.2 Deserto → **Areias Flamejantes** · cidade: **Oásis de Qocho**

| Campo | Proposta |
|---|---|
| Nome (pt / en) | **Areias Flamejantes** / *Flaming Sands*. Cidade: **Oásis de Qocho** / *Qocho Oasis* (substitui "Povoado de Areia Branca"). |
| Significado | As **Montanhas Flamejantes** de arenito vermelho que "ardem" ao sol. **Qocho** foi a capital do reino uigur de Qocho, onde o maniqueísmo foi fé da corte e onde se acharam os livros iluminados maniqueus. |
| Fonte | Huoyan Shan (Turfan); Qocho/Gaochang (sécs. IX–XIII, reino uigur); o canato uigur adotou o maniqueísmo em 762. **Cuidado:** usar só nomes históricos/míticos; nada que fale da região hoje. |
| Elemento | **Fogo Vivo** (Ícaro). Contraparte: Fogo Devorador (o calor que "come" a sombra das pessoas). |
| Culturas | Uigur antiga e tocária (o oásis), **kushita/núbia** e Kemet (as **Ruínas dos Artífices**: pirâmides íngremes estilo Meroé e ushabtis). A caravana é sogdiana (o caravaneiro Ammo). |
| Arquitetura (kit 3D) | Cidade: **taipa e adobe** rosados, muralha baixa de terra batida com cantos arredondados, **casas de secar uva** (paredes de tijolo vazado em xadrez, como as de Turfan), **cúpulas de cisterna** e bocas de **karez** (galerias de água subterrânea, com poços em fila). Ruínas: **pirâmides pequenas e pontudas** com capela na frente (Meroé), colunas com capitel de lótus quebradas, estátuas de ushabti em fila. |
| Paleta | arenito vermelho `#B5482F`, rosa-taipa `#D7A28A`, areia dourada `#E7C27D`, sombra violeta `#6D4C7D`, verde de parreira `#8DAA3A`, ouro velho das ruínas `#C9A23F`. |
| Vegetação | parreiras, tamareira, **tamargueira**, choupo-do-deserto, espinheiro-de-camelo (*Alhagi*), touceiras secas. Oásis com melancia e melão. |
| Props | poço de karez, camelo bactriano de carga (a Teimosa), fardos de seda, jarras de barro, **lanternas de bronze**, painel de pedra dos Artífices com inscrição (o "aviso"), pequenas pirâmides, ushabtis caídos, tapetes de feltro, *dutar* (alaúde de duas cordas) encostado na parede. |

### 2.3 Montanha → **Serra de Kaggen** · cidade: **Vila de Litema**

| Campo | Proposta |
|---|---|
| Nome (pt / en) | **Serra de Kaggen** / *Kaggen Range*. Cidade: **Vila de Litema** / *Litema Village* (substitui "Vila da Serra Alta"). |
| Significado | **\|Kaggen** é o louva-a-deus esperto dos contos dos San \|xam, que jogou o **sapato no céu** e o sapato virou a **Lua**. (Ótimo para as Barcas: a "Barca da Lua" é o sapato do Kaggen, diz a Lavine.) **Litema** são as pinturas geométricas que as mulheres basotho fazem nas paredes de barro. |
| Fonte | Contos \|xam San coletados por Bleek e Lloyd (séc. XIX), "como \|Kaggen fez a Lua"; *litema*, arte mural do Lesoto. Na UI, escrever **Kaggen** sem o clique "\|" (o clique fica na nota de rodapé e no Caderno). |
| Elemento | **Vento Vivo** (Maithe). Contraparte: Vento Destruidor (a neblina que "mente" para o Olho do Vento, já no arco dela). |
| Culturas | **Africanas das terras altas** (base): San (pinturas de rocha, eland), basotho (cobertor, chapéu *mokorotlo*, litema), muralhas de pedra seca do **Grande Zimbábue** (shona) e estelas de **Aksum**. Gigantes do *Livro dos Gigantes* como os apagados. |
| Arquitetura (kit 3D) | **Rondavéis** (casas redondas de pedra e barro com **teto cônico de palha**) com faixas de litema em relevo; **muralhas curvas de pedra seca** sem argamassa (com um friso em espinha de peixe, como no Grande Zimbábue); **abrigos de rocha** com pinturas de eland e gente dançando; uma **estela alta** entalhada com "janelinhas" falsas (estilo Aksum) na praça; pontes de corda nos vales; neve no topo. |
| Paleta | barro ocre-vermelho `#A0522D`, branco de cal `#EFE6D8`, pedra cinza-granito `#8A8A84`, palha `#C8A85A`, **azul-cobertor** `#2F4F8F` e **amarelo-cobertor** `#E3B23C` (padrões basotho), neve azulada `#DDE8F0`. |
| Vegetação | capim alto dourado, **protea**, **aloé** em flor vermelha, babosa-de-árvore, cedros-de-montanha retorcidos e líquen. No alto, neve. |
| Props | cobertores listrados pendurados, chapéu cônico *mokorotlo* como enfeite, cestos de palha, potes de barro com tampa, **rocha pintada** (eland, louva-a-deus), **pilão de pedra**, tambor de pele, forja do Mihr (o mineiro) na boca da mina, estela, sino da mina. |
| Respeito | San e basotho são povos vivos. Usar arquitetura, cores e **contos de bicho** (o louva-a-deus, o eland, a lua). Nunca usar ritual de cura (dança de transe) como minijogo, e nunca botar inimigo "com cara de" San. Os inimigos daqui são **gigantes e ushabtis**, que não são deste povo. |

### 2.4 Cavernas → **Grutas do Limiar** · cidade: **Vila das Lamparinas**

| Campo | Proposta |
|---|---|
| Nome (pt / en) | **Grutas do Limiar** / *Threshold Caves*. Cidade: **Vila das Lamparinas** (fica igual: "lamparina" é luz e não é folclore). En: *Lampkeepers' Village*. |
| Significado | O limiar, a porta entre a Ilha e a **Terra da Treva**. A vila é de gente que **mantém as lâmpadas acesas** para a sombra não subir. |
| Fonte | Borda da Terra da Treva (mito maniqueísta). Casas de gruta da Armênia e Anatólia (Khndzoresk, com a ponte suspensa; Capadócia). *Vishapakar* (pedras-dragão armênias pré-cristãs). Os "Gabryab" e "Mihrshah" do elenco vêm de missões maniqueias na Armênia (§3.1). |
| Elemento | **Luz** (Maria). Contraparte: a Treva. |
| Culturas | Armênia e urartiana (casas na rocha, tufo rosa, pedras-dragão), hurrita (o gigante de diorito na torre, §3.13), um toque mandeu na "água viva" das fontes da gruta (só a ideia de água corrente e roupa branca nos lamparineiros, **sem rito**). |
| Arquitetura (kit 3D) | Casas **escavadas no tufo rosa**, com portinha e janelinha redonda, em vários andares ligados por escadas e **pontes suspensas de corda**. Fachadas com arco simples de pedra. **Nichos de lâmpada** em toda parede. Fonte subterrânea com bica de pedra. Estalactites. **Vishapakar** (pedra comprida com cabeça de peixe/dragão entalhada) na entrada. |
| Paleta | tufo rosa `#C98F86`, basalto `#3B3B44`, índigo da sombra `#232046`, **âmbar das lâmpadas** `#FFB347`, branco das roupas `#F2EEE6`, cobre `#B06B3A`. |
| Vegetação | musgo fluorescente verde-claro, cogumelos de luz azul, samambaias de gruta. Na boca da gruta: zimbro e roseira-brava. |
| Props | lamparinas de barro e de cobre (de todo tamanho), corrente de lâmpadas penduradas, cesto de pavios, ponte de corda, sino pequeno, carretel de corda do minerador velho, jarras de óleo, vishapakar, **porta selada antiga** (o selo dos Artífices, rachado). |

### 2.5 Regra de ouro para quem faz cidades e inimigos

- **Culturas emprestam beleza, não vilões.** Cidades, NPCs, roupas e festas podem vir de povos reais. **Inimigos e chefes**
  vêm só do lado mítico (arcontes, gigantes, sombra, bichos apagados, ushabtis).
- **Cada região, uma assinatura de forma** que o kit pode repetir: Zarafshan = iwan + coluna entalhada; Qocho = parede
  vazada + cúpula de cisterna; Litema = rondavel + muralha curva; Lamparinas = porta no tufo + ponte de corda.
- Low-poly: os padrões (litema, suzani, rosetas sogdianas) vão em **textura de paleta**, não em geometria.

---

## 3. Tabelas de-para (antigo → novo)

Convenção: **ids não mudam.** Coluna "Fonte" = uma linha. **[verificar]** = a fonte deve ser conferida antes de publicar.

### 3.1 NPCs

Os tratamentos "Seu/Dona/Comadre" saem. Os novos são **Mestre/Mestra** para ofício, ou nome + epíteto ("Mariam, a
Ervateira"). As crianças chamam os mais velhos de "tio/tia", que é neutro e carinhoso. Mudanças em relação à lista do
usuário vêm marcadas com ⚑, com o motivo.

| Antigo (id) | Novo | Papel | Personalidade | Fonte |
|---|---|---|---|---|
| Dona Cida (`cida`, `forest_rezadeira`) | **Mariam, a Ervateira** | ervas e cura, bazar de linhas, missões da Floresta | calma, avó de todo mundo, cheira a hortelã | Maryam, mãe de Mani numa das tradições (al-Nadim, *Fihrist*). |
| Seu Severino (`severino`) | **Mestre Adamas** | ferreiro (Ferraria), refino | forte, ri alto, chama martelo de "meu amigo" | Adamas da Luz, guerreiro que pisa o monstro da Treva (Teodoro bar Konai). |
| Mestre Damião (`damiao`) | **Mestre Omóforo** | marceneiro (móveis) | segura tudo nos ombros, nunca reclama do peso | Omóforo (*Omophoros*), o que carrega a terra nos ombros (Kephalaia). |
| Dona Fátima (`fatima`) | **Mestra Narisah** | costureira (roupas) | elegante, vaidosa do bom gosto, elogia todo mundo | Narisah (*Narisah yazd*), o "Terceiro Mensageiro" que se mostra belo (textos médio-persas). |
| Seu Lázaro (`lazaro`) | **Zaku, o Merceeiro** | mercearia | econômico, conta tudo duas vezes | Mār Zakū, líder da igreja parta (fragmentos partas de Turfan). |
| Comadre Neném (`nenem`) | ⚑ **Nana, a Feirante** | feira e viveiro de plantas | tagarela, sabe de tudo da vila, ama flores | ⚑ era "Sisin" (homem; Neném é mulher). Nanaia/Nana, deusa sogdiana muito amada nas cidades da Rota da Seda (pinturas de Panjakent). |
| Seu Bento (`bento`, `desert_tropeiro`) | **Ammo, o Caravaneiro** | caravaneiro itinerante (o antigo Mascate), amigo de estrada do Vô Jorge | contador de causos, saudoso, generoso | Mār Ammō, que levou os hinos para Xwarāsān e falava parto (texto parto de missão, M 2). |
| Seu Tonico do Haras (`tonico_haras`) | **Srosh, o Tratador** | estrebaria (montarias), "Primeira Sela" | escuta tudo (bicho e gente), fala pouco | Srōš, o "Ouvir/Obedecer" persa; *Srōšahrāy* = Coluna da Glória. |
| Seu Zé Caçador (Barraca de caça, `Gameplay/Hunting`) | **Patig, o Rastreador** | tarefas de caça, presa do dia | paciente, mestre de pegadas, respeita os bichos | Pattig/Pattikios, pai de Mani (*Fihrist*; Codex de Colônia). |
| Dona Rosa (olaria) | **Náfsha, a Oleira** | olaria (Qocho) | mãos sempre sujas de barro, canta enquanto gira o torno | *nafšā*, "alma" em siríaco/aramaico. |
| Seu Bené (casa de música) | **Mestre Xroshtag** | casa de instrumentos, puxa os hinos do Bema | voz enorme, chama todo mundo de longe | *Xrōštag*, o "Chamado" personificado (textos partas). |
| — (novo, opcional) | **Padwaxtag** | aprendiz do Xroshtag, responde aos chamados dele no Bema | tímido, voz fininha | *Padwāxtag*, a "Resposta" personificada. |
| Seu Malaquias (vigia do engenho) | ⚑ **Adda, o Vigia da Torre** | guardião da Torre da Roda das Doze Jarras (sala de evento) | velho, emotivo, chora quando a jarra volta | ⚑ era "Addai", mas Addai é o apóstolo cristão de Edessa. Mār Addā é o missionário maniqueu do oeste (Codex de Colônia; texto parto M 2). |
| Dona Fé (capelinha) | **Ramrat, a Guardiã da Lâmpada** | lâmpada guardiã da Vila (proteção contra a perda ao morrer) | serena, sempre acende uma lâmpada para quem sai | *Rāmratūx*, nome médio-persa da Mãe da Vida. |
| Seu Vavá (`vava`, selaria) | ⚑ **Vandak, o Seleiro** | selaria (Qocho) | mercador esperto, escreve cartas para a família longe | ⚑ era "Zurvan" (nome do Pai da Grandeza nos textos persas, alto demais para um seleiro). Nanai-vandak, mercador sogdiano das *Cartas Antigas* (c. 313 d.C.). |
| Dona Socorro (`socorro`, cisterna) | ⚑ **Yardena, a da Cisterna** | cuida da cisterna e da lâmpada de Qocho | prática, mandona, guarda cada gota | ⚑ "Abzakhya" está sem fonte confirmada **[verificar]**. *Yardnā*, "água corrente, viva" (mandeu/aramaico). |
| Tião das Cabras (`tiao`) | **Uzzai, o Pastor** | pastor de cabras e camelos (Qocho) | brincalhão, imita bicho | Uzzai, discípulo nomeado em fragmentos maniqueus **[verificar]**. |
| Mestre Tonho (`tonho`, `mountain_miner`) | **Mestre Mihr, o Mineiro** | forja e mina (Litema), missões da Montanha | grave, de poucas palavras, coração mole | Mihr/Mihryazd, nome persa do Espírito Vivo, o que "construiu o mundo" (textos médio-persas). |
| Dona Graça (`graca`, fiandeira) | ⚑ **Coti, a Tecelã** | fiandeira e tecelã de cobertores, lâmpada de pedra | fala com o louva-a-deus da janela | ⚑ era "Vahman" (a Mente de Luz, masculino e doutrinal). Coti, esposa de \|Kaggen nos contos \|xam (Bleek e Lloyd). |
| Seu Quinzinho (`quinzinho`, sanfona) | **Innaios, o Tocador** | músico da vila (tambor e lira) | animado, desafina de propósito para fazer rir | Innaios, sucessor de Sisinnios na liderança maniqueia (fontes coptas). Alternativa local: Kwammang-a, o arco-íris dos contos San. |
| Benedita das Lamparinas (`benedita`, `caves_lampkeeper`) | **Parthenos, a das Lamparinas** | guardiã das lâmpadas, missões das Grutas | doce e corajosa, não tem medo do escuro | *Parthenos tou phōtos*, a Virgem da Luz (Kephalaia). |
| Seu Candinho (`candinho`, mineiro velho) | **Mihrshah, o Velho Mineiro** | mineiro aposentado, conta do jardim de luz que viu quando jovem | sonhador, esquecido, gentil | Mihrshah, irmão de Shapur I, a quem Mani mostra um jardim de luz (texto médio-persa M 47). |
| Dona Fia (`fia`, causos) | ⚑ **Aso, a Contadora** | conta as histórias de **Ananse** (a aranha esperta) para as crianças | engraçada, faz voz de bicho | ⚑ era "Nuhzadag" (o intérprete de Mani, homem). Aso, esposa de Ananse nos contos akan. Os contos de Ananse são da tradição oral akan e caribenha. |
| Zezinho do Pavio (`zezinho`) | **Gabryab, o do Pavio** | menino que corta pavios | rápido, quer ser amigo da Lavine | Mār Gabryab, missionário que curou a filha do rei em Revan (Erevan), texto sogdiano/parto (M 1306). |
| Vó Zefa (texto antigo) | **Vó Edna** | (já decidido) | — | família, mantém |
| Vô Chico (texto antigo) | **Vô Jorge** | (já decidido) | — | família, mantém |

Grafia en: Mariam the Herbalist, Master Adamas, Master Omophoros, Mistress Narisah, Zaku the Grocer, Nana the Market Lady,
Ammo the Caravaneer, Srosh the Stablekeeper, Patig the Tracker, Nafsha the Potter, Master Xroshtag, Adda the Tower
Keeper, Ramrat the Lampkeeper, Vandak the Saddler, Yardena of the Cistern, Uzzai the Herder, Master Mihr the Miner, Coti
the Weaver, Innaios the Player, Parthenos of the Lamps, Old Mihrshah, Aso the Storyteller, Gabryab of the Wick.

#### 3.1.1 Quem dá a história do meio, "A Lua que Não Enche" (2026-11-09)

A cadeia do nível 20 ao 97 (`Docs/HistoriaMeio_20a100.md`, `MidStoryCatalog`/`MidStoryTexts`) começa na **Vó Edna** e passa
por 14 moradores das cidades das Regiões 1–8, cada um com uma fala por primo e uma lembrança do Vô Jorge no diário. Ficha
completa (figura, casa, fonte) em `Docs/NPCs.md`. Regra dos nomes: nas **cidades humanas** o nome vem da história maniqueia;
nas **cidades de raça** (elfos, orcs, anões, kobolds, povo-touro, trols, suaílis) vem da lenda ou da história da região da
ficha (`Docs/MetodologiaMapasERacas.md`), sempre com fonte e nunca "Seu/Dona". Divindades de fé viva não entram (§0).

| Etapa | Quem | Cidade | Fonte do nome |
|---|---|---|---|
| 1 | **Vó Edna** | Vila da Vó Edna | família (§1.7). |
| 2 | **Gara, a Guardiã das Copas** | Copas de Garajonay (elfos-da-mata) | lenda canária de Gara e Jonay (La Gomera). |
| 3 | **Ashmunikal, a Matriarca dos Tambores** | Tambor Rubro (orcs) | rainha hitita (histórica). |
| 4 | **Napirasu, a Mestra Fundidora** | Salões de Bronze (anões) | rainha elamita (a estátua de bronze do Louvre). |
| 5 | **Takhsich, o Inventor** | Mercado dos Túneis (kobolds) | nome sogdiano dos documentos de Mug. |
| 6 | **Nuru, a Guardiã do Farol** | Taa Pwani (suaílis) | *nuru*, "luz" em suaíli. |
| 7 | **Ensuhkeshdanna, o Rival Amigo** | Aratta (povo-touro) | o senhor de Aratta que perde as charadas para Enmerkar (épica suméria). |
| 8 | **Capitão Liyongo, o Nakhoda** | Taa Pwani | Fumo Liyongo, herói-arqueiro da épica suaíli. |
| 9 | **Salmaios, o Guardião da Raiz-Mãe** | Jhilpur | o asceta do Códice Maniqueu de Colônia. |
| 10 | **Ramtha, a Curandeira** | Palafitas de Tarchon (trols) | nome etrusco de Tarquínia. |
| 11 | **Abā, o Chefe das Calhas** | Sangkent | Abā, missionário de Mani. |
| 12 | **Baraies, o Professor** | Sangkent | Baraies, o mestre do Códice de Colônia. |
| 13 | **Bögü, o Chefe da Guarda dos Anéis** | Mingcheng | Bögü Qaghan, khagan uigur que adotou o maniqueísmo em 762. |
| 14 | **Lin Deng, o Mestre do Canal** | Mingcheng | Lin Deng, maniqueu de Fujian (era Song). |
| 15 | **Fuduodan, o Leitor** | Mingcheng | o *fuduodan* persa que levou o *Livro dos Dois Princípios* à corte chinesa em 694. |

Também citados: **Tages** (Salão de Tages, que agora é vizinho e diz os quatro lugares) e **Mihrshah** (o jardim do mapa 06).
Os chefes do mundo das Regiões 5–7 aparecem só como perigo ao lado (§3.4.1). No fim, a luz da lua cai no forro do Caderno,
numa costura do Vô Jorge, e a cadeia passa para o Soslan e as Cartas da Estrada (`Docs/HistoriaFinal_Cartas.md`).

### 3.2 Inimigos comuns

| Antigo (`id`) | Novo (pt / en) | Fonte |
|---|---|---|
| Lobo da Mata (`enemy_forest_wolf`) | **Lobo do Tugai** / *Tugai Wolf* | lobos das florestas ribeirinhas da Ásia Central (bicho real). |
| Aranha da Floresta (`enemy_forest_spider`) | **Aranha-Tecelã Apagada** / *Dimmed Weaver Spider* | Aso diz: "nem toda aranha é ruim, lembra de Ananse". Ananse é o herói, nunca o inimigo. |
| Escorpião das Dunas (`enemy_desert_scorpion`) | **Escorpião das Areias Flamejantes** / *Flaming Sands Scorpion* | bicho real; Serket, a deusa-escorpião protetora de Kemet, só como sabor de lore (o Caderno diz que o escorpião "esqueceu que já foi protetor"). |
| Golem de Pedra (`enemy_mountain_stone_golem`) | **Ushabti de Pedra** / *Stone Ushabti* | ushabti = "o que responde" (Kemet). São os guardiões construídos pelos Artífices (já na missão da Maithe). |
| Enxame de Morcegos (`enemy_cave_bat_swarm`) | **Enxame de Morcegos do Limiar** / *Threshold Bat Swarm* | bicho real. |
| Variante "(noturno)" | **"(da noite)"** / *(night)* | igual. Evitar "(da Treva)", que soa vilão demais para bicho comum. |

#### 3.2.1 Bichos das fichas dos mapas (2026-11-03: os que os mundos contínuos faziam com substitutos)

Definições em `Resources/Enemies` (bioma de casa com peso 0: só entram onde o layout do mapa/overworld põe), números da
faixa da região (vida/XP iguais aos comuns dela; o nível vem da área, `EnemyLevelScaling`), modelo procedural
(`OverworldBeastRecipes` / `OverworldOddityRecipes`), página do Caderno em `OverworldBestiaryTexts` (lore da Vó + uma fala
por primo, en/pt-BR). Testes: `OverworldBeastsTests`.

| `id` | pt / en | Ficha (Mapas100) | Tipo | Fonte / lore |
|---|---|---|---|---|
| `enemy_desert_jackal` | **Chacal Apagado** / *Dimmed Jackal* | 07 (e o "chacal" de toda a Areias) | [rápido], foge ferido | bicho real; os "primos arrumadinhos" das caravanas, apagados pelo Fogo Devorador. |
| `enemy_desert_sand_snake` | **Cobra-de-Areia Apagada** / *Dimmed Sand Snake* | 08 | [rápido] | víbora-chifruda das dunas (bicho real); a sombra bebeu o calor das escamas. |
| `enemy_desert_crowned_orc` | **Orc Coroado de Āz** / *Āz-Crowned Orc* | 11 (Clã Coroado) | [elite], pesado | coroinha de lata de Āz (§3.4); **nunca a raça**: vencido, a coroa cai e ele volta para o Tambor Rubro. |
| `enemy_mountain_rolling_stone` | **Pedra que Rola** / *Rolling Stone* | 13 (e a Pedra-Rolante Ushabti do 11) | [controle], pesado | ushabti-mensageiro dos Artífices que se enrola em bola; esqueceu o recado. |
| `enemy_mountain_ill_wind_mist` | **Neblina do Vento Ruim** / *Ill-Wind Mist* | 14, 18 | [controle], voa | o Vento Destruidor (§1.3) preso nas pedras, juntando neblina até ganhar olhos. |
| `enemy_mountain_baboon` | **Babuíno Apagado** / *Dimmed Baboon* | 13 (e o Babuíno Ladrão do 15) | [rápido], foge ferido | bicho real; família enxerida que deu de roubar sacola. |
| `enemy_cave_ooze` | **Lodo de Gruta** / *Cave Ooze* | 19 | [tanque], pesado | o Lodo (contrário da Água Viva) que levantou da margem; engole luz (cristais à mostra). |
| `enemy_cave_dark_crow` | **Corvo-da-Treva** / *Crow of the Dark* | 24 | [distância] (pena-dardo, mesmo tiro do Gavião) | corvos-mensageiros do Ninho de Mahaway com a Treva nas pontas das penas. |
| `enemy_forest_thief_gull` | **Gaivota Ladra** / *Thief Gull* | 25 (missão do Liyongo) | [rápido], voa | bicho real, apagada pela gula de Āz: rasga velas atrás de moeda. Bioma Floresta (Água Viva) até a Costa ter o seu. |
| `enemy_forest_coconut_crab` | **Caranguejo-dos-Coqueiros Gigante** / *Giant Coconut Crab* | 25 (missão do Zawadi) | [tanque] | bicho real; o Lodo entrou nas raízes do mangue. Bioma Floresta até a Costa ter o seu. |

**Segunda leva (2026-11-03): Costa das Monções, Delta de Jhilpur e Teto do Mundo (Mapas100 25–40).** Mesmo contrato
(peso 0 no bioma, nível da área, números do bioma). Costa e Delta ficam no bioma Floresta (Água Viva) e o Teto no
Montanha, como os layouts `OverworldLayouts.Costa/Delta/Teto` já declaram. Modelos em `CoastDeltaBeastRecipes` /
`PamirBeastRecipes` (o crocodilo, o leão, o urso e o irbis abrem a mandíbula no bote; o rinoceronte e o iaque baixam a
cabeça para a carga; a píton serpenteia como a cobra-de-areia; o trol e o almas erguem os braços), Caderno em
`LateRegionBestiaryTexts`. Testes: `LateRegionBeastsTests`. Já nos overworlds da Costa, do Delta e do Teto no lugar dos
substitutos (testes: `LateRegionBeastsPlacementTests`).

| `id` | pt / en | Ficha (Mapas100) | Tipo | Fonte / lore |
|---|---|---|---|---|
| `enemy_forest_mangrove_crocodile` | **Crocodilo do Mangue Apagado** / *Dimmed Mangrove Crocodile* | 30 (e o Gavial do Lodo, o Crocodilo de Musgo e o Gavial-Sombra de 31–35) | [tanque], pesado | bicho real; o Lodo subiu pelos canais e encheu a barriga de frio: engole faísca, rede e remo. |
| `enemy_forest_grass_lion` | **Leão do Capim Apagado** / *Dimmed Grass Lion* | 26 | [rápido] | bicho real; o Fogo Devorador passou pelo capim seco e ficou nos olhos (e na juba, em brasinhas). |
| `enemy_forest_dust_rhino` | **Rinoceronte-de-Pó** / *Dust Rhino* | 26 | [tanque], carga | bicho real; uma rachadura de luz presa nas costas dói, então ele investe em tudo o que se mexe. |
| `enemy_forest_thief_monkey` | **Macaco Ladrão** / *Thief Monkey* | 32 (e os macacos do Figueiral e da Floresta de Maré) | [rápido], foge ferido | langur das pontes de raiz que ajudava a levar cipó; a gula de Āz o fez roubar pulseira e sininho. |
| `enemy_forest_thief_otter` | **Lontra Ladra** / *Thief Otter* | 31 | [rápido], foge ferido | bicho real; dividia o peixe com os barqueiros, o Lodo a deixou gulosa (o peixe iluminado na boca). |
| `enemy_forest_dimmed_python` | **Píton Apagada** / *Dimmed Python* | 32 | [controle] | bicho real; esquentava as raízes das pontes à noite, agora abraça quem passa, apertado e frio. |
| `enemy_forest_dimmed_troll` | **Trol Apagado** / *Dimmed Troll* | 33 (e a Mata Morta) | [tanque], pesado | **nunca a raça**: pescador de Tarchon que foi pegar um fogo-fátuo e esqueceu o caminho; a faísca presa na gaiola das costas sussurra. Vencido, a gaiola abre e ele volta para casa. |
| `enemy_mountain_brown_bear` | **Urso-Pardo do Pamir Apagado** / *Dimmed Pamir Brown Bear* | 36 | [tanque], pesado | bicho real; o frio da sombra entrou no sono de inverno, acordou cedo, emburrado, com geada que não derrete. Tira do dia o corpo do lobisomem (espécie da noite). |
| `enemy_mountain_wild_yak` | **Iaque Selvagem Apagado** / *Dimmed Wild Yak* | 38 (e o Argali Apagado do 40 até ter o seu) | [tanque], carga | bicho real; a sombra gelou o calor do coração e ele investe nas caravanas. |
| `enemy_mountain_irbis` | **Irbis Apagado** / *Dimmed Irbis* | 40 | [rápido] | o leopardo-das-neves (bicho real), atrevido pela sombra; pula dos penhascos no Passo de Wakhan. |
| `enemy_mountain_almas` | **Almas** / *Almas* | 38 | [elite], tímido, foge ferido | o homem selvagem dos contos mongóis e da Ásia Central; assustado pela sombra, joga bola de neve socada (no corpo a corpo por enquanto). |
| `enemy_mountain_giant_marmot` | **Marmota Gigante** / *Giant Marmot* | 36, 37 | [enxame], foge ferido | marmota do Pamir (bicho real) que cresceu do tamanho de um cachorro; o assobio para quando ela vem. |

**Terceira leva (2026-11-06): Planície de Mingcheng e Estepe dos Nartes (Mapas100 41–52).** Mesmo contrato (peso 0 no
bioma, nível da área, números do bioma). Mingcheng fica no bioma Floresta (Água Viva) e os Nartes no Montanha, como os
layouts `OverworldLayouts.Mingcheng/Nartes` já declaram. Modelos em `MingchengBeastRecipes` / `NartesBeastRecipes` (o
leão de pedra, a tartaruga, o filhote de nian, o lobo de bronze, a raposa e o grifo abrem a boca/bico no bote; a raposa
acende a lanterna do rabo e o grifo abre as asas; o búfalo e a saiga baixam a cabeça para a carga; o soldado ergue a
alabarda, o gigante e o ochokochi erguem os dois braços; o besouro abre as pinças), Caderno em
`MingchengNartesBestiaryTexts`. Testes: `MingchengNartesBeastsTests`. Já nos overworlds de Mingcheng e dos Nartes no
lugar dos substitutos, nos grupos que lideram e como acompanhantes (mesmas clareiras, níveis e variantes noturnas; os
búfalos seguem com a vara de queixadas à noite; o Grifo Jovem também faz o Grifo Guardião, os abutres seguem abutres). O
Urso do Cáucaso (52) passou a ser o `enemy_mountain_brown_bear` (o lobisomem só à noite) e os macacos do
bambu/calcário/névoa o `enemy_forest_thief_monkey` (o Filhote de Nian só nos Picos e no Portão do Nian). Testes:
`MingchengNartesBeastsPlacementTests`. Carpa-Dragão, Cavalo de Terracota, Grou de Papel, Espelho Quebrado, Cisne de
Feltro, Cervo de Ouro, Cabra-Montês, Lince e Garanhão seguem com substitutos.

| `id` | pt / en | Ficha (Mapas100) | Tipo | Fonte / lore |
|---|---|---|---|---|
| `enemy_forest_stone_lion` | **Leão de Pedra Apagado** / *Dimmed Stone Lion* | 41 (e o Guardião de Cal do 42) | [tanque], pesado | os leões de guarda dos portões chineses (forma); o frio da sombra entrou por uma rachadura e os acordou: guardam a estrada contra todos. |
| `enemy_forest_lantern_fox` | **Raposa de Cauda Lanterna** / *Lantern-Tail Fox* | 41 | [rápido], foge ferida | a raposa esperta dos contos chineses; levava um pouco de luz das lanternas na ponta do rabo, a gula de Āz a fez roubar todas. |
| `enemy_forest_terracotta_soldier` | **Soldado de Terracota Apagado** / *Dimmed Terracotta Soldier* | 41, 45 | [tanque], pesado | o exército de barro de Xi'an como imagem; a sombra fez o barro marchar. Quebrado, sobe uma faísca: o carinho do oleiro. |
| `enemy_forest_stone_turtle` | **Tartaruga de Pedra** / *Stone Turtle* | 43 | [tanque], pesado | a tartaruga que carrega estelas (bixi, forma); jurou nunca largar a pedra com os nomes de quem cavou o canal, a sombra lhe deu medo de que a tomem. |
| `enemy_forest_mud_buffalo` | **Búfalo de Lama** / *Mud Buffalo* | 44 | [tanque], carga | búfalo-d'água (bicho real); o frio subiu dos terraços alagados até as pernas, não aguenta ninguém perto da água. |
| `enemy_forest_nian_cub` | **Filhote de Nian** / *Nian Cub* | 45 | [elite] | filhote do *nian* do Ano-Novo chinês (monstro de conto, não divindade); foge de vermelho, luz e barulho (a granada do Ícaro). |
| `enemy_mountain_young_griffin` | **Grifo Jovem** / *Young Griffin* | 47 (e o Grifo Guardião do 48) | [elite] | os grifos que guardam o ouro (Heródoto IV, ouros citas); a sombra pôs nos novos a gula de ouro dos velhos. Corpo a corpo. |
| `enemy_mountain_saiga` | **Saiga Assustada** / *Frightened Saiga* | 47 | [enxame], carga, foge ferida | bicho real; a manada mais nervosa da estepe, apavorada pela sombra. |
| `enemy_mountain_bronze_wolf` | **Lobo de Bronze** / *Bronze Wolf* | 48 | [tanque], pesado | os bichos de bronze enrolados do estilo animal cita; a sombra desceu num kurgan aberto e os acordou. |
| `enemy_mountain_sleepwalker_giant` | **Gigante Sonâmbulo** / *Sleepwalking Giant* | 49 | [tanque], pesado | gigante-da-colina (*Livro dos Gigantes*) preso num sonho ruim fugido das tábuas do Virogdad; vencido, acorda e volta para casa. |
| `enemy_mountain_bark_beetle` | **Besouro-Casca** / *Bark Beetle* | 50 | [tanque] | besouro da taiga que só comia madeira morta; Āz lhe deu uma fome sem fim e ele rói as casas do povo-tronco. |
| `enemy_mountain_ochokochi` | **Ochokochi** / *Ochokochi* | 52 | [elite], pesado | o homem da mata do folclore georgiano, com a lâmina de osso no peito; sozinho e bravo pela sombra, procura alguém para "abraçar". |

**Quarta leva (2026-11-06): Picos de Farnkesh (Mapas100 53–58).** Mesmo contrato (peso 0 no bioma, nível da área,
números do bioma). O Vale dos Sátiros (53) fica no bioma Floresta (Água Viva) e os picos (54–58) no Montanha, como as
áreas de `OverworldLayouts.Farnkesh` já declaram (`forest` / `rock`). Modelos em `FarnkeshBeastRecipes` (o bode, o takin e
a cabra-azul baixam a cabeça para a carga; a raposa e o gato-de-pallas abrem a boca, o pássaro-flauta, a gralha, a águia e a
harpia abrem o bico com as asas paradas; a abelha zune e passa o ferrão por baixo; a vela do morcego cresce antes da
mordida; o ushabti ergue os dois braços com a enxada), Caderno em `FarnkeshBestiaryTexts`. O Pássaro-Flauta e a Águia da
Tempestade são os [distância] das fichas: jogam pena dura como o gavião (`EnemyProjectileKind.Feather`, mesmo som; entram em
`RangedEnemiesTests`). Testes: `FarnkeshBeastsTests`. Já no overworld de Farnkesh no lugar dos substitutos, nos grupos que
lideram e como acompanhantes (mesmas clareiras, níveis e variantes noturnas): no Vale o bode, a abelha, o pássaro-flauta e a
raposa (os bodes dormem de noite, as raposas seguem nos grupos deles); nos picos a gralha (também as Gralhas das Ermidas, antes
morcegos de gruta), o takin, a cabra-azul, o morcego-vela, o ushabti, o gato-de-pallas (também nos pináculos e na escarpa), a
águia na Gruta Abrigo e como acompanhante na Garganta e na escarpa, a harpia nas Ilhas de Nuvem; o Grifo Selvagem do Ninho da
Huma passou a ser o `enemy_mountain_young_griffin`. Testes: `FarnkeshBeastsPlacementTests`. Seguem com substitutos: Abutre do
Pico (`enemy_desert_ember_vulture`), Urso, Aranha, Marmota e Macaco do pé dos picos (já são bichos próprios de outras
regiões), Pipa-Sombra / Sino Rachado / Raio Preso (`enemy_desert_broken_lantern`), Neblina do Vento Ruim / Eco Apagado / Ays /
Redemoinho / Ventania (`enemy_mountain_ill_wind_mist`) e Pedra Voadora (`enemy_mountain_rolling_stone`).

| `id` | pt / en | Ficha (Mapas100) | Tipo | Fonte / lore |
|---|---|---|---|---|
| `enemy_forest_smoke_goat` | **Bode de Fumaça** / *Smoke Goat* | 53 | [tanque], carga | as cabras dos sátiros (os bodes do Vale frígio); a sombra desceu pelo Pactolo e a lã virou fumaça com brasa; não lembram o nome e dão cabeçada em quem chama. Dormem de noite. |
| `enemy_forest_golden_bee` | **Abelha Dourada** / *Golden Bee* | 53 | [enxame] | o toque de Midas (mito frígio, como imagem); Āz tocou as abelhas e elas viraram ouro que nunca acha que tem o bastante; a picada doura (lava no Pactolo). Corpo a corpo. |
| `enemy_forest_flute_bird` | **Pássaro-Flauta Apagado** / *Dimmed Flute Bird* | 53 | [distância] | invenção sobre a flauta de Mársias: o bico oco feito caniço; a sombra apagou a música, joga as penas duras do rabo de longe. |
| `enemy_forest_vine_fox` | **Raposa da Vinha** / *Vine Fox* | 53 | [rápido], foge ferida | raposa-vermelha (bicho real); o Midas deixava os cachos de baixo para elas, a sombra fez querer todos. |
| `enemy_mountain_alpine_chough` | **Gralha-Alpina** / *Alpine Chough* | 54 (e as gralhas do 56) | [rápido] | bicho real dos picos altos; tocava os sinos de vento de brincadeira, a sombra deu gula por coisa brilhante. |
| `enemy_mountain_takin` | **Takin Apagado** / *Dimmed Takin* | 54 | [tanque], carga | bicho real do Himalaia; a lã dele está nas cordas do Sisin; a sombra o fez achar que toda ponte é armadilha. |
| `enemy_mountain_blue_sheep` | **Cabra-Azul** / *Bharal* | 54 | [rápido], carga, foge ferida | o bharal (bicho real); mostrava os caminhos seguros dos pináculos, agora dá cabeçada primeiro. |
| `enemy_mountain_candle_bat` | **Morcego-Vela** / *Candle Bat* | 55 | [rápido] | invenção: os morcegos das pontes roubaram as velas das lanternas e as usam grudadas na cabeça; de noite dobram. |
| `enemy_mountain_rope_ushabti` | **Ushabti de Corda** / *Rope Ushabti* | 55 | [tanque], pesado | os *ushabti* do Egito (servos de figura para o outro mundo, como forma); o Sisin os fez de corda e barro; a sombra desamarrou as ordens. |
| `enemy_mountain_pallas_cat` | **Gato-de-Pallas Apagado** / *Dimmed Pallas's Cat* | 56 | [rápido] | o manul (bicho real); amigo do silêncio das ermidas, a sombra o deixou sobressaltado: pula no menor barulho. |
| `enemy_mountain_storm_eagle` | **Águia da Tempestade** / *Storm Eagle* | 57 | [distância] | invenção sobre os *ays* armênios: as águias brigaram com o vento ruim e voltaram com raio na ponta das asas. |
| `enemy_mountain_smoke_harpy` | **Harpia de Fumaça** / *Smoke Harpy* | 58 | [rápido] | a harpia grega (as "que agarram") só como bicho; ave de rapina meio fumaça nas mãos do Vento Destruidor. |

**Quinta leva (2026-11-06): Mar Gelado (Mapas100 59–64).** Mesmo contrato (peso 0 no bioma, nível da área, números do
bioma). A região inteira fica no bioma Montanha (Ar Sereno), como `OverworldLayouts.MarGelado` declara
(`w.World.Biome = BiomeType.Mountain`). Modelos em `MarGeladoBeastRecipes` (a morsa, a rena de barro, a lebre-pedra e o mamute
baixam a cabeça para a carga; as duas focas, a raposa, o urso, o lobo-do-mar e o lúcio abrem a boca, com as nadadeiras de trás
das focas, o rabo da raposa e do lobo e a cauda do lúcio balançando e as barbatanas do lúcio remando no ar frio; a gaivota e a
ave-graveto abrem o bico com as asas paradas), Caderno em `MarGeladoBestiaryTexts`. A Gaivota-do-Gelo e a Ave-Graveto de Omöl
são os [distância] das fichas: jogam pena congelada / graveto como o gavião (`EnemyProjectileKind.Feather`, mesmo som; entram
em `RangedEnemiesTests`). Testes: `MarGeladoBeastsTests`. Já no overworld do Mar Gelado no lugar dos substitutos, nos grupos
que lideram e como acompanhantes (mesmas clareiras, níveis e variantes noturnas): no porto, nas baías, no passo e nas colinas
a foca-leopardo, a morsa, a gaivota e a raposa; o urso-polar nos canais, no gelo rachado e nos fiordes; na tundra e nos lagos
de Omöl a rena de barro (também nos Pastos de Rena), a lebre-pedra e a ave-graveto (de noite segue a Ave-Graveto da
mãe-da-lua); o mamute nas galerias e no Fiorde das Presas; o lobo-do-mar nas banquisas, no farol e nos Pastos de Rena; a
foca-leopardo também nos narvais, a gaivota também nas Gaivotas-Tempestade, o lúcio também no Peixe-Gelo dos canais; a
foca-sombra no lago do Kul, no gelo negro e no Fiorde do Kul. Testes: `MarGeladoBeastsPlacementTests`. Respeito (ficha da
região): só bichos e a sombra; Yen, Omöl, o mamute de baixo da terra e os *kul* só como os contos komi entram nas fichas.
Seguem com substitutos: Enguia-Lanterna (`enemy_desert_sand_snake`), Rachadura / Raiz-Gelo / Peixe-Bruxa
(`enemy_cave_ooze`), Mosquito da Tundra / Kul Pequeno (o enxame de vaga-lumes), Toupeira-Gelo
(`enemy_mountain_giant_marmot`), Cristal Vivo (`enemy_desert_broken_lantern`), Gelo Flutuante
(`enemy_mountain_ill_wind_mist`) e o irbis dos fiordes (já é bicho próprio).

| `id` | pt / en | Ficha (Mapas100) | Tipo | Fonte / lore |
|---|---|---|---|---|
| `enemy_mountain_leopard_seal` | **Foca-Leopardo Apagada** / *Dimmed Leopard Seal* | 59 (e os narvais do 63) | [rápido] | bicho real; seguia os barcos da lua na esteira; a sombra espalhou as pintas feito tinta e deu a fome da Āz. |
| `enemy_mountain_smoke_walrus` | **Morsa de Fumaça** / *Smoke Walrus* | 59 | [tanque], carga | as morsas velhas da Baía das Focas; a sombra entrou debaixo do couro e elas fumegam com brasa nas dobras (fogo que come, não calor). |
| `enemy_mountain_ice_gull` | **Gaivota-do-Gelo** / *Ice Gull* | 59 (e a Gaivota-Tempestade do 63) | [distância] | a gaivota ladra dos varais do Pamour; a sombra congelou as penas e deu gula por coisa brilhante; joga pena congelada de longe. |
| `enemy_mountain_arctic_fox` | **Raposa-do-Ártico Ladra** / *Thieving Arctic Fox* | 59 | [rápido], foge ferida | bicho real; entra no bairro da pesca; a sombra fez querer todo peixe e toda lamparina, foge com o roubo amarrado no rabo. |
| `enemy_mountain_polar_bear` | **Urso-Polar Apagado** / *Dimmed Polar Bear* | 60 | [tanque] | bicho real; os barqueiros dividiam a pesca com ele; a sombra pôs listras cinzentas e fez achar que os canais são só dele. |
| `enemy_mountain_clay_reindeer` | **Rena de Barro** / *Clay Reindeer* | 61 | [tanque], carga | criatura desajeitada de Omöl (conto komi de Yen e Omöl): barro da tundra, uma perna mais comprida, chifres que não combinam; a sombra encheu as rachaduras de raiva. Dorme em pé de noite. |
| `enemy_mountain_stone_hare` | **Lebre-Pedra** / *Stone Hare* | 61 | [rápido], carga, foge ferida | criatura de Omöl: lebre feita de pedra redonda que ele esqueceu de fazer leve; antes fazia os pastores rirem. |
| `enemy_mountain_twig_bird` | **Ave-Graveto de Omöl** / *Omöl's Twig Bird* | 61 | [distância] | a mais desajeitada de Omöl: casca, graveto e líquen, olhos grandes demais, uma asa mais curta; a sombra a emburrou e ela joga os gravetos do rabo. |
| `enemy_mountain_burrow_mammoth` | **Mamute de Baixo da Terra** / *Underground Mammoth* | 62 | [elite], pesado, carga | o mamute dos contos komi e mansi que vive debaixo da terra e cava os vales; cristal de gelo nas costas; a sombra o fez esquecer o caminho para cima. |
| `enemy_mountain_sea_wolf` | **Lobo-do-Mar** / *Sea Wolf* | 63 (e os lobos dos Pastos de Rena) | [enxame] | invenção da ficha (lobo que nada): pata com membrana, barbatana nas costas, rabo-leme; a sombra os mandou subir no gelo em bando. |
| `enemy_mountain_giant_pike` | **Lúcio Gigante** / *Giant Pike* | 64 (e o Peixe-Gelo do 60) | [rápido] | bicho real dos lagos frios; os *kul* guardam os peixes (conto komi); o Velho Kul azedou e os lúcios dele nadam no ar frio sobre os buracos. |
| `enemy_mountain_shadow_seal` | **Foca-Sombra** / *Shadow Seal* | 64 | [tanque] | a foca-anelada (bicho real) escurecida feito a água debaixo do gelo, anéis que brilham; deita em cima dos buracos de pesca porque a Āz disse que todo peixe é dela. |

**Sexta leva (2026-11-09): Arquipélago de Hinokura (Mapas100 65–70).** Mesmo contrato (peso 0 no bioma, nível da área, números do
bioma). A região inteira fica no bioma Montanha (Fogo Vivo no tema, mas `OverworldLayouts.Hinokura` declara
`w.World.Biome = BiomeType.Mountain`), daí o prefixo `enemy_mountain_`. Modelos em `HinokuraBeastRecipes` (o caranguejo abre as
pinças; o lagarto, a shisa, o tanuki, a tibicena, o lagarto-gigante, o peixe-lâmina e o coral abrem a boca com a cauda balançando,
as barbatanas do peixe remando no vapor e os ramos do coral mexendo devagar; o ushabti, o ogro e o gigante erguem os dois braços;
a gaivota abre o bico com as asas paradas), Caderno em `HinokuraBestiaryTexts`. A Gaivota-Brasa e o Coral de Brasa são os
[distância] das fichas: cospem brasa como o abutre (`EnemyProjectileKind.Ember`, entram em `RangedEnemiesTests`). Testes:
`HinokuraBeastsTests`. Já no overworld de Hinokura no lugar dos substitutos, nos grupos que lideram e como acompanhantes (mesmas
clareiras, níveis e variantes noturnas): o caranguejo, o lagarto de lava e a gaivota na ilha da forja, na praia, nas fumarolas, no
estreito, nos canais de lava e nas fontes; a shisa e o tanuki nos jardins, nos pinhais e nas pontes; a tibicena (o dobro de noite) na
retama, no Echeyde e no malpaís, com o lagarto-gigante no malpaís; o peixe-lâmina e o coral nos canais quentes, no recife e nas
ilhotas; o ushabti e o ogro nas muralhas, no castelo, no pátio e na escória; o gigante de escória na caldeira, na encosta, na borda,
nos campos e no mar de cinza. Testes: `HinokuraBeastsPlacementTests`. Respeito (ficha da região): a shisa de Okinawa, que segue viva nos telhados, só como guardiã apagada e
nunca ridicularizada; os tibicenas guanches só como o conto os dá, contado com cuidado; o tanuki só como o bicho brincalhão dos
contos. Seguem com substitutos: Pedra-Pomes Voadora (`enemy_mountain_rolling_stone`), Carpa de Brasa (`enemy_cave_lantern_fish`),
Bambu de Fumaça / Fumarola / Fole Vivo (`enemy_mountain_ill_wind_mist`), Bomba Vulcânica (`enemy_desert_ember_vulture`), Polvo de
Obsidiana / Cinza Viva (`enemy_cave_ooze`), Ouriço de Vidro (`enemy_forest_stone_turtle`), Faísca Raivosa / Chama Faminta (o enxame
de vaga-lumes) e Salamandra Devoradora (`enemy_desert_sand_snake`).

| `id` | pt / en | Ficha (Mapas100) | Tipo | Fonte / lore |
|---|---|---|---|---|
| `enemy_mountain_volcano_crab` | **Caranguejo-Vulcão** / *Volcano Crab* | 65 | [tanque] | caranguejos das praias negras; a sombra fez a casca criar um cone que fumega e cada um achar que é dono de uma montanha (Āz: mais e mais nas costas). |
| `enemy_mountain_lava_lizard` | **Lagarto de Lava** / *Lava Lizard* | 65 | [rápido] | lagartinhos dos canais que comiam as moscas das forjas; a sombra acendeu a listra das costas. |
| `enemy_mountain_ember_gull` | **Gaivota-Brasa** / *Ember Gull* | 65 | [distância] | as gaivotas dos barcos de minério comeram as faíscas das chaminés; cospem brasa de longe, gulosas demais pra chegar perto. |
| `enemy_mountain_dimmed_shisa` | **Shisa Apagada** / *Dimmed Shisa* | 66 | [elite] | a *shisa* de Okinawa (leão-cão de telhado, tradição viva) só como forma: os guardiões dos palácios de Hinokura apagaram, desceram e não lembram pra quem era a casa. |
| `enemy_mountain_tanuki` | **Tanuki Ladrão** / *Thieving Tanuki* | 66 | [rápido], foge ferido | bicho real (cachorro-guaxinim) e o trapaceiro dos contos japoneses; a sombra tirou a brincadeira e deixou só o roubo. |
| `enemy_mountain_tibicena` | **Tibicena** / *Tibicena* | 67 | [rápido] | os cães negros do conto guanche do Echeyde (cultura extinta, contado com cuidado); magros, só costela e fome; de noite são o dobro. |
| `enemy_mountain_giant_lava_lizard` | **Lagarto-Gigante de Lava** / *Giant Lava Lizard* | 67 | [tanque] | lagartos do malpaís do tamanho de carroça, que dormiam cem anos; a sombra entrou pelas frestas das placas de basalto. |
| `enemy_mountain_blade_fish` | **Peixe-Lâmina** / *Blade Fish* | 68 | [rápido] | invenção: peixes dos canais quentes que se esfregaram no vidro preto até ficarem afiados; nadam no vapor. |
| `enemy_mountain_ember_coral` | **Coral de Brasa** / *Ember Coral* | 68 | [distância] | invenção: o coral das fontes quentes, aldeia de bichinhos; a sombra disse que a aldeia era pequena e ele soltou da pedra. |
| `enemy_mountain_lacquer_ushabti` | **Ushabti de Laca** / *Lacquer Ushabti* | 69 | [tanque] | o ushabti de Kemet como ideia copiada pelos ferreiros: figuras de laca vermelha que seguem passando o ferro quente sem saber por quê. |
| `enemy_mountain_furnace_ogre` | **Ogro-Fornalha** / *Furnace Ogre* | 69 | [elite] | invenção: os foguistas que cantavam nas Nove Fornalhas; a sombra disse pra guardar o fogo e a fornalha foi parar na barriga. |
| `enemy_mountain_slag_giant` | **Gigante de Escória** / *Slag Giant* | 70 | [tanque] | o monte de tudo que as forjas jogaram fora; o Fogo Devorador o fez andar, querendo voltar a ser alguma coisa. |

**Sétima leva (2026-11-09): Rio das Doze Horas (Mapas100 71–74).** Mesmo contrato (peso 0 no bioma, nível da área, números do
bioma). A região inteira fica no bioma Floresta (`OverworldLayouts.Rio` declara `w.World.Biome = BiomeType.Forest`), daí o prefixo
`enemy_forest_` e os números de Floresta (vida 72–108, XP 10). Modelos em `RioBeastRecipes` (o crocodilo, o hipopótamo, o mordedor e o
junco-laço abrem a boca com a cauda balançando, os laços do junco mexendo devagar; a íbis e o gafanhoto abrem o bico com as asas
paradas; o escaravelho e o escorpião abrem as pinças, e o ferrão recua; a cobra e a serpente fazem a onda na cauda, e a serpente mexe as
duas cabeças laterais; o ushabti ergue os dois braços; a treva achata e o ouro engolido brilha), Caderno em `RioBestiaryTexts`. Nenhum é
[distância]: as fichas 71–74 não pedem (o abutre-brasa da Foz é bicho do layout, não da ficha). Testes: `RioBeastsTests`. Respeito
(§3.16): só os bichos dos contos de Kemet, nunca os deuses; o escaravelho é besouro e as serpentes são cobras. Já no overworld do Rio
no lugar dos substitutos (`OverworldLayouts.Rio.cs`, classe `Rio`), nos grupos que lideram e como acompanhantes só onde a ficha os cita
(mesmas clareiras, níveis e variantes noturnas): o crocodilo, a íbis e o junco-laço nas margens do Cais (71); o gafanhoto, o hipopótamo e
a cobra nos campos, canteiros, trigais e canais (72); a serpente, o escorpião e o ushabti de areia no areal, nas portas, no caminho e no
zigue-zague (73); o mordedor e a treva nos anéis, no anel externo e no nó (74); o escaravelho em 71, 73 e 76. Testes:
`RioBeastsPlacementTests`. Seguem com substitutos: Ganso Ladrão (`enemy_forest_thief_gull`), Chama Faminta (o enxame de vaga-lumes),
Salamandra de Sombra (`enemy_desert_sand_snake`), Filhos de Apep (`enemy_forest_dimmed_python`), Escorpião [tanque] do 74
(`enemy_desert_scorpion`), Ushabti Apagado (`enemy_mountain_rope_ushabti`), Ushabti Rachado (`enemy_forest_terracotta_soldier`), Sombra
Fervida e Areia Movediça (`enemy_cave_ooze`) e Crocodilo de Lodo (`enemy_forest_mangrove_crocodile`).

| `id` | pt / en | Ficha (Mapas100) | Tipo | Fonte / lore |
|---|---|---|---|---|
| `enemy_forest_dark_river_crocodile` | **Crocodilo do Rio Escuro** / *Dark River Crocodile* | 71 | [tanque] | bicho real; olhava a barca passar, cada um na sua hora, e a sombra ensinou que o que boia é de quem está embaixo. |
| `enemy_forest_dimmed_ibis` | **Íbis Apagada** / *Dimmed Ibis* | 71 | [rápido] | ave real da margem; contava as horas, perdeu o branco e agora só pega o que brilha. |
| `enemy_forest_shadow_scarab` | **Escaravelho de Sombra** / *Shadow Scarab* | 71, 73, 76 | [enxame] | besouro de esterco real (se guia pelas estrelas), não o deus; a sombra fez querer uma bola cada vez maior. |
| `enemy_forest_reed_snare` | **Junco-Laço** / *Reed Snare* | 71 | [controle] | invenção: feixes de papiro largados na margem, que agora querem amarrar alguma coisa. |
| `enemy_forest_shadow_locust` | **Gafanhoto de Sombra** / *Shadow Locust* | 72 | [enxame] | gafanhoto real e a praga que come os campos de luz; cada um só tem fome, mas são fome demais. |
| `enemy_forest_mud_hippo` | **Hipopótamo de Lodo** / *Mud Hippo* | 72 | [tanque] | bicho real, roçador dos canais; a sombra disse que os canais eram dele. |
| `enemy_forest_reed_snake` | **Cobra do Junco** / *Reed Snake* | 72 | [rápido] | cobra que espantava os ratos dos campos; a sombra tirou a paciência e deixou a cor. |
| `enemy_forest_three_headed_serpent` | **Serpente de Três Cabeças** / *Three-Headed Serpent* | 73 | [elite] | as serpentes do *Amduat* só como bicho; cada cabeça acha que só ela puxa a barca. |
| `enemy_forest_black_scorpion` | **Escorpião Negro** / *Black Scorpion* | 73 | [rápido] | bicho real; a sombra virou a paciência dele em pressa. |
| `enemy_forest_sand_ushabti` | **Ushabti de Areia** / *Sand Ushabti* | 73 | [tanque] | o servo de tumba de Kemet na versão mais barata, ainda esperando a ordem nas portas fechadas. |
| `enemy_forest_shadow_biter` | **Mordedor de Sombra** / *Shadow Biter* | 74 | [rápido] | as sombras que mordem Mehen: pedaços de treva arrancados, só boca. |
| `enemy_forest_creeping_dark` | **Treva Rastejante** / *Creeping Dark* | 74 | [enxame] | o que sobra nos anéis de manhã, gulosa das menores coisas (Āz): arranca lascas do ouro de Mehen. |

**Oitava leva (2026-11-09): Margens da Mistura (Mapas100 77–82).** Mesmo contrato (peso 0 no bioma, nível da área, números do bioma). A região
inteira fica no bioma Floresta (`OverworldLayouts.Mistura` declara `w.World.Biome = BiomeType.Forest`), daí o prefixo `enemy_forest_` e os
números de Floresta (vida 72–108, XP 10). Modelos em `MisturaBeastRecipes` (o leão, o mušḫuššu e a sombra atrasada abrem a boca com a cauda
balançando; a sombra e o manto mexem as mangas devagar; o gafanhoto e a garça abrem o bico com as asas paradas; o betume achata e as brasas
brilham; a ampulheta, o cedro, o filho de Ullikummi e o jardim-sombra erguem os dois braços; a tábua inclina para trás e a linha acesa
brilha), Caderno em `MisturaBestiaryTexts`. Dois são [distância], voam e atiram: a Garça de Piche (gosma, `LureGlob`) e a Tábua Quebrada
(raio de palavras, `LanternBolt`; `RangedEnemiesTests`). Respeito (§1.1 e §3.16): Zurvan só como relógio; Upelluri e Ullikummi só como
pedra e ombro; o Hobabish é o guardião que se apagou, nunca vilão. Testes: `MisturaBeastsTests`. Já no overworld das Margens no lugar dos
substitutos (`OverworldLayouts.Mistura.cs`, classe `Mistura`), nos grupos que lideram e como acompanhantes só onde a ficha os cita (mesmas
clareiras, níveis e variantes noturnas): o leão, o mušḫuššu e o gafanhoto em volta de Dur-Jarras (77); a ampulheta e a sombra no deserto (78);
o betume e a garça nos poços (79); o cedro e o manto no cedral (80); o filho de Ullikummi no diorito e no beiral (81); a tábua e o jardim-sombra
no campo (82). Testes: `MisturaBeastsPlacementTests`. Seguem com substitutos, sem bicho próprio nesta leva: Homem-Escorpião
(`enemy_desert_scorpion`), Lagarto do Instante (`enemy_mountain_lava_lizard`), Ushabti de Relógio (`enemy_mountain_lacquer_ushabti`), Bagre Negro
(`enemy_mountain_giant_pike`), Fumaça de Piche (`enemy_mountain_smoke_harpy`), Lince do Cedro (`enemy_mountain_irbis`), Abelha-Resina
(`enemy_forest_golden_bee`), Pedra de Diorito (`enemy_mountain_rolling_stone`), Ave do Ombro (`enemy_mountain_storm_eagle`), Tremor
(`enemy_mountain_stone_golem`), Pássaro-Sonho (`enemy_forest_shroud_owl`) e Água que Apaga (`enemy_mountain_shadow_seal`).

| `id` | pt / en | Ficha (Mapas100) | Tipo | Fonte / lore |
|---|---|---|---|---|
| `enemy_forest_brick_lion` | **Leão de Tijolo Apagado** / *Dimmed Brick Lion* | 77 | [tanque] | os leões vidrados das ruas de procissão; a sombra apagou o azul por dentro. |
| `enemy_forest_young_mushussu` | **Mušḫuššu Jovem** / *Young Mushussu* | 77 | [elite] | o dragão-serpente dos portões da Babilônia, só como bicho; novinho, ainda não sabe qual metade vai na frente. |
| `enemy_forest_bitumen_locust` | **Gafanhoto de Betume** / *Bitumen Locust* | 77 | [enxame] | gafanhoto real que caiu nos poços; ninguém mais come ele, e virou muitos. |
| `enemy_forest_living_hourglass` | **Ampulheta Viva** / *Living Hourglass* | 78 | [tanque] | invenção: ampulhetas da sala que media a hora mais comprida, atrás da hora roubada. |
| `enemy_forest_late_shadow` | **Sombra Atrasada** / *Late Shadow* | 78 | [controle] | invenção da ficha: a sombra que ficou sem dono no deserto e chega um instante depois. |
| `enemy_forest_living_tar` | **Betume Vivo** / *Living Tar* | 79 | [controle] | o betume de Elam que calafetou o barco de Atrahasis, agora com vontade própria. |
| `enemy_forest_tar_heron` | **Garça de Piche** / *Tar Heron* | 79 | [distância] | garça real que ficou parada na poça errada e agora cospe o que não engole. |
| `enemy_forest_angry_cedar` | **Cedro Raivoso** / *Angry Cedar* | 80 | [tanque] | os cedros do conto de Gilgamesh; a sombra disse que todo mundo traz machado. |
| `enemy_forest_glow_mantle` | **Manto de Brilho** / *Mantle of Radiance* | 80 | [controle] | os sete mantos de brilho de Humbaba/Hobabish, vazios, procurando ombros. |
| `enemy_forest_ullikummi_son` | **Filho de Ullikummi** / *Son of Ullikummi* | 81 | [elite] | lascas do diorito da *Canção de Ullikummi* que herdaram só a vontade de crescer. |
| `enemy_forest_broken_tablet` | **Tábua Quebrada** / *Broken Tablet* | 82 | [distância] | as tábuas do sonho de Ohya e Hahya (*Livro dos Gigantes*); joga as palavras que soltaram. |
| `enemy_forest_garden_shadow` | **Jardim-Sombra** / *Garden Shadow* | 82 | [tanque] | as árvores cortadas do mesmo sonho, andando nos tocos sem saber para que servem. |

**Nona leva (2026-11-09): Limiar Profundo (Mapas100 84–90).** Mesmo contrato (peso 0 no bioma, nível da área, números do bioma). A região
inteira fica no bioma Floresta (`OverworldLayouts.Profundo` declara `w.World.Biome = BiomeType.Forest`), daí o prefixo `enemy_forest_` e os
números de Floresta (vida 75–108, XP 10). Dois bichos por cova, mais o Arconte do Portão. Modelos em `ProfundoBeastRecipes` (o lobo, o leão, o
dragão e o arqueiro abrem a boca com a cauda balançando; o peixe rema as barbatanas; o lagarto abre a boca com a cauda devagar; a águia e a
harpia abrem o bico com as asas paradas; o polvo achata e o âmbar brilha; a cópia, o gigante e o arconte erguem os dois braços), Caderno em
`ProfundoBestiaryTexts`. Dois são [distância], voam e atiram: o Arqueiro de Fumaça (`LanternBolt`, flecha de fumaça quente que entorta) e a
Harpia de Vento (`Feather`; `RangedEnemiesTests`). Respeito (§1.1, §1.4 e §4.3): frio, pegajoso, barulhento e nunca gore; o Príncipe só como
silhueta atrás do selo (a Águia e o Dragão-Sombra deixam claro que **não** são as formas dele); o Arconte Menor é gigante de conto que
engoliu luz demais, com a luz ainda lá dentro. Testes: `ProfundoBeastsTests`. Já no overworld do Limiar no lugar dos
substitutos (`OverworldLayouts.Profundo.cs`, classe `Profundo`), nos grupos que lideram e como acompanhantes só onde a ficha os cita (mesmas
clareiras, níveis e variantes noturnas; a Ponte do Novelo, 89, mistura os das cinco covas). A Águia Negra é corpo a corpo e segue [rápido]; o
Arqueiro e a Harpia lideram os grupos [distância] (84, 89 e 86). Testes: `ProfundoBeastsPlacementTests`. Seguem com substitutos, sem bicho próprio nesta leva: Sombra Pequena, Fumaça, Morcego da Treva, Ushabti Apagado, Fumaça Sufocante, Touro de Cinza, Chama Faminta,
Morcego-Lâmina, Redemoinho, Enguia Grande, Cardume Escuro, Serpente Cega, Rastejante e Ushabti Apagado Gigante (ver `OverworldLayouts.Profundo.cs`).

| `id` | pt / en | Ficha (Mapas100) | Tipo | Fonte / lore |
|---|---|---|---|---|
| `enemy_forest_copy_shadow` | **Sombra-Cópia** / *Copy Shadow* | 84 | [rápido] | a Fumaça guarda os de duas pernas (Teodoro bar Konai, *Scholion* XI); sem forma própria, pega emprestada a de quem chega. |
| `enemy_forest_smoke_giant` | **Gigante de Fumaça** / *Smoke Giant* | 84 | [tanque] | anéis de fumaça que se empilharam e esqueceram de descer; vento desfaz, lâmina não. |
| `enemy_forest_smoke_archer` | **Arqueiro de Fumaça** / *Smoke Archer* | 84 | [distância] | invenção: o resto dos soldados dos Artífices soprados pelos ventiladores, que ainda lembra do arco. |
| `enemy_forest_ember_wolf` | **Lobo de Brasa** / *Ember Wolf* | 85 | [rápido] | o Fogo Devorador guarda os de quatro patas; lobo real que só sobrou o quentinho. |
| `enemy_forest_hot_smoke_lion` | **Leão de Fumaça Quente** / *Hot Smoke Lion* | 85 | [elite] | basalto por fora, fornalha por dentro; apaga pavios só de respirar. |
| `enemy_forest_black_eagle` | **Águia Negra** / *Black Eagle* | 86 | [rápido] | águia comum que voou perto demais e saiu com a cor; **não** é a forma do Príncipe (§1.4). |
| `enemy_forest_wind_harpy` | **Harpia de Vento** / *Wind Harpy* | 86 | [distância] | o que sobra quando o vento leva o que queria; atira as penas que ficaram. |
| `enemy_forest_mud_octopus` | **Polvo de Lodo** / *Mud Octopus* | 87 | [tanque] | o Lodo guarda os que nadam; este desistiu e virou o chão da cova, com óleo de lâmpada na barriga. |
| `enemy_forest_dark_fish` | **Peixe da Treva** / *Dark Fish* | 87 | [rápido] | a água sobe e desce pelas comportas; este continuou nadando sem ela. |
| `enemy_forest_shadow_lizard` | **Lagarto-Sombra** / *Shadow Lizard* | 88 | [tanque] | a Treva guarda os que rastejam; os olhos encolheram e o resto se espalhou; sem luz o golpe entra pela metade. |
| `enemy_forest_lesser_shadow_dragon` | **Dragão-Sombra Menor** / *Lesser Shadow Dragon* | 88 | [elite] | retalho do corpo de dragão preso atrás do selo, que se lembra de ser enorme; **não** é a forma do Príncipe (§1.4). |
| `enemy_forest_lesser_archon` | **Arconte Menor** / *Lesser Archon* | 90 | [elite] | arcontes pequenos (§1.4) que chegaram tarde e engoliram o que os grandes deixaram cair; a luz ainda é luz. |

**Décima leva (2026-11-09): Reinos do Céu (Mapas100 91–99).** Mesmo contrato (peso 0 no bioma, nível da área, números do bioma). A região inteira
fica no bioma Floresta (`OverworldLayouts.Ceu` declara `w.World.Biome = BiomeType.Forest`), daí o prefixo `enemy_forest_` e os números de Floresta (vida 75–108, XP 10).
Modelos em `CeuBeastRecipes` (a ave e a gaivota abrem o bico com as asas paradas; o peixe rema as barbatanas; o clandestino, o Um dos Doze e a Pedra abrem a boca com a cauda balançando;
a corda solta as pontas devagar; a sombra do porão achata e o lampião brilha; a jarra inclina e a rachadura brilha; o arconte, o balão e o ladrão erguem os dois braços), Caderno em
`CeuBestiaryTexts`. Dois são [distância], voam e atiram: a Jarra Rachada (`LanternBolt`) e a Gaivota de Luz Apagada (`Ember`; `RangedEnemiesTests`). Respeito (§0.2, §1.4 e §4.3):
quieto, pálido e meio bobo, nunca gore; os arcontes são gigantes de conto que engoliram luz demais, com a luz ainda lá dentro, e o Pai da Grandeza não aparece. Testes: `CeuBeastsTests`. Já no overworld dos Reinos do Céu no lugar dos
substitutos (`OverworldLayouts.Ceu.cs`, classe `Ceu`), nos grupos que lideram e como acompanhantes só onde a ficha os cita (mesmas clareiras, níveis e variantes noturnas). A Jarra e a Gaivota lideram os grupos [distância] (94 e 97). A Pedra do Céu também fica na Varanda (100), como pede a ficha ("fora do pomar, os bichos do 99"). O Arconte Fujão Menor (91), o Arconte Menor (98) e o Arconte Fugido (99) usam o `enemy_forest_lesser_archon` da nona leva. Testes: `CeuBeastsPlacementTests`. Sem bicho próprio nesta leva, seguem com substitutos: Corda de Fumaça (97, o junco-laço; a Corda Viva fica só na Barca da Lua),
Faísca Confusa, Sombra Clandestina e Sombra que Pisa as Flores, Sombra Nadadora e do Meio-Dia, Corrente Errada e Rachadura do Céu, Lanterna Fugitiva, Faísca Teimosa, Gota de Treva, Fumaça de Esfera,
Vento de Órbita e do Alto, Espinho de Fumaça, Clandestino de Brasa e Última Sombra.

| `id` | pt / en | Ficha (Mapas100) | Tipo | Fonte / lore |
|---|---|---|---|---|
| `enemy_forest_dimmed_cloud_bird` | **Ave de Nuvem Apagada** / *Dimmed Cloud Bird* | 91 | [rápido] | as aves de nuvem que levam luz dos píeres às barcas; esta esqueceu onde deixou a sua. |
| `enemy_forest_dimmed_light_fish` | **Peixe de Luz Apagado** / *Dimmed Light Fish* | 92 | [rápido] | o rio é estrada (bíblia §3.16); o peixe parou de carregar, sobrou uma gota pendurada. |
| `enemy_forest_dark_stowaway` | **Clandestino de Treva** / *Dark Stowaway* | 93 | [rápido] | sombra que subiu escondida numa barca, com a etiqueta de embarque de outro. |
| `enemy_forest_hold_shadow` | **Sombra do Porão** / *Hold Shadow* | 93 | [tanque] | embarcou no fundo do mundo e sentou em cima da bagagem, com o lampião preso nela. |
| `enemy_forest_living_rope` | **Corda Viva** / *Living Rope* | 93 | [controle] | corda que ficou amarrada numa sombra e pegou mania de agarrar. |
| `enemy_forest_toy_archon` | **Um dos Doze** / *One of the Twelve* | 94 | [elite] | os doze arcontes presos na Roda (*Acta Archelai* 8), gastos em brinquedo de corda. |
| `enemy_forest_cracked_jar` | **Jarra Rachada** / *Cracked Jar* | 94 | [distância] | jarra da Roda que levou uma pancada: não derrama, vaza de lado. |
| `enemy_forest_chained_archon` | **Arconte Acorrentado** / *Chained Archon* | 95 | [tanque] | o Espírito Vivo prende os arcontes nas esferas (Teodoro bar Konai); empurra e resmunga. |
| `enemy_forest_balloon_giant` | **Gigante-Balão** / *Balloon Giant* | 95 | [elite] | o arconte que segurou tanto o que engoliu que inchou redondo, como na ficha. |
| `enemy_forest_az_thief` | **Ladrão de Āz** / *Thief of Āz* | 96 | [elite] | Āz, a Cobiça, quer o jardim numa arca (M 47) e manda gente de capuz colher. |
| `enemy_forest_dimmed_light_gull` | **Gaivota de Luz Apagada** / *Dimmed Light Gull* | 97 | [distância] | gaivota que seguiu a barca do Sol de perto demais e cospe a última brasa. |
| `enemy_forest_sky_stone` | **Pedra do Céu** / *Sky Stone* | 99 (e fora do pomar, 100) | [tanque] | pedaço do teto do céu caído das rachaduras; quer voltar para casa. |

**Décima primeira leva (2026-11-09): os substitutos mais visíveis das Regiões 12–16.** Mesmo contrato (peso 0 no bioma, nível da área, números do bioma), no máximo três por região, prefixo conforme o bioma do layout (`enemy_mountain_` em Hinokura, `enemy_forest_` nas demais). Modelos em `LateStandInBeastRecipes`, Caderno em `LateStandInBestiaryTexts`. Dois são [distância], voam e atiram: a Bomba Vulcânica (`Ember`) e o Homem-Escorpião (`Feather`; `RangedEnemiesTests`), que paira um palmo acima do chão porque só quem voa atira. Respeito (§1.1 e §3.16): os Filhos de Apep são só cobras; o Homem-Escorpião é o *girtablullu* só como o conto o dá, nunca deus. Testes: `LateStandInBeastsTests` e `LateStandInBeastsPlacementTests`. Já no overworld no lugar dos substitutos, com as mesmas clareiras, níveis e variantes noturnas (classes `Fogo`, `Rio`, `Mistura`, `Profundo` e `Ceu`; `Rio.Quicksand` é constante nova, e `Rio.Ooze` segue fazendo a Sombra Fervida). Estes doze saem das listas de "seguem com substitutos" das levas anteriores; os demais papéis citados nelas continuam com substituto.

| `id` | pt / en | Ficha (Mapas100) | Tipo | Fonte / lore |
|---|---|---|---|---|
| `enemy_mountain_devouring_salamander` | **Salamandra Devoradora** / *Devouring Salamander* | 70 | [rápido] | invenção: comeu o fogo da borda da caldeira; quanto mais engole, mais fome (Fogo Devorador). |
| `enemy_mountain_volcanic_bomb` | **Bomba Vulcânica** / *Volcanic Bomb* | 67 | [distância] | pedra que o Echeyde jogou e nunca desceu; cospe o que ainda tem dentro. |
| `enemy_forest_thief_goose` | **Ganso Ladrão** / *Thief Goose* | 72 | [rápido] | bicho real; a sombra só deu um empurrãozinho no mau gênio. |
| `enemy_forest_child_of_apep` | **Filho de Apep** / *Child of Apep* | 76 | [rápido] | cobras que estavam deitadas em Apep quando ela passou e saíram achando que eram ela; só bicho. |
| `enemy_forest_quicksand` | **Areia Movediça** / *Quicksand* | 76 | [controle] | banco de areia a quem a sombra disse que podia ficar com o que pisasse nele. |
| `enemy_forest_scorpion_man` | **Homem-Escorpião** / *Scorpion-Man* | 77 | [distância] | *girtablullu*, guarda das montanhas no conto de Gilgamesh; ficou de guarda no portão de tijolo e esqueceu para quê. |
| `enemy_forest_cedar_lynx` | **Lince do Cedro** / *Cedar Lynx* | 80 | [rápido] | lince real que se esfregou nos cedros raivosos até a resina colar no pelo. |
| `enemy_forest_resin_bee` | **Abelha-Resina** / *Resin Bee* | 80 | [enxame] | a abelha que levou resina para casa em vez de mel. |
| `enemy_forest_blade_bat` | **Morcego-Lâmina** / *Blade Bat* | 86 | [enxame] | o Vento Destruidor gastou tudo até virar fio. |
| `enemy_forest_great_eel` | **Enguia Grande** / *Great Eel* | 87 | [controle] | continuou deitada onde a água estava; nada no ar dos salões alagados. |
| `enemy_forest_ash_bull` | **Touro de Cinza** / *Ash Bull* | 85 | [tanque] | queimou de ponta a ponta sem perceber; cinza por fora, mau gênio por dentro. |
| `enemy_forest_smoke_rope` | **Corda de Fumaça** / *Smoke Rope* | 97 | [controle] | a corda da barca do Sol queimou; a fumaça não ficou sabendo e segue amarrando. |

**Décima segunda leva (2026-11-11): os substitutos mais visíveis das Regiões 1–4 (Mapas100 02–06, 10, 14–16, 20).** Mesmo
contrato (peso 0 no bioma, nível da área, números do bioma), prefixo conforme o bioma do layout (`enemy_forest_` no
Zarafshan, `enemy_desert_` nas Areias, `enemy_mountain_` em Kaggen, `enemy_cave_` no Limiar). Modelos em
`EarlyStandInBeastRecipes`, Caderno em `EarlyStandInBestiaryTexts`. Seis são [distância] e atiram: a cegonha, o pombo e a
gralha (`Feather`), o sapo-bolha e a mariposa (`LureGlob`) e o pavão (`Ember`). Respeito: o ubır só como os contos tártaros
o dão (§3.5), um espírito faminto, e aqui ainda novinho; os abrigos pintados são as pinturas de eland dos San, sem nenhuma
figura sagrada (o Borrão é invenção). Testes: `EarlyStandInBeastsTests` e `EarlyStandInBeastsPlacementTests`. Já no
overworld (`OverworldLayouts.Zarafshan/Areias/Kaggen/Limiar`) no lugar dos substitutos, com as mesmas clareiras, níveis e
variantes noturnas; entre parênteses, o substituto que saiu. Estes doze saem das listas de "seguem com substitutos".

| `id` | pt / en | Ficha (Mapas100) | Tipo | Fonte / lore |
|---|---|---|---|---|
| `enemy_forest_dimmed_stork` | **Cegonha Apagada** / *Dimmed Stork* | 02 Prado das Cegonhas e várzea (era a Ave-Graveto) | [distância], voa | bicho real; voltou cinzenta nas bordas, "como se alguém tivesse soprado o branco"; joga os gravetos do ninho. |
| `enemy_forest_bubble_toad` | **Sapo-Bolha do Lodo** / *Mire Bubble Toad* | 03 (era o enxame de morcegos) | [distância] | o brejo bebeu um pouco da Treva e os sapos beberam o brejo; sopra bolha de lama que gruda nos pés (a água do Joaquim lava). |
| `enemy_forest_smoke_mosquito` | **Mosquito de Fumaça** / *Smoke Mosquito* | 03 (era o enxame de morcegos) | [enxame], voa | a Fumaça, 1º elemento da Treva (§1.3), juntou-se em mosquitos; o que bebem brilha na barriga. |
| `enemy_forest_silk_moth` | **Mariposa de Seda Gigante** / *Giant Silk Moth* | 04 (era a Ave-Graveto) | [distância], voa | a mariposa da seda que comeu as amoreiras dos quintais de tear e nunca fiou; o pó das asas dá sono. |
| `enemy_forest_laurel_pigeon` | **Pombo-do-Louro Apagado** / *Dimmed Laurel Pigeon* | 05 (era a Ave-Graveto) | [distância], voa | o pombo das laurisilvas das Canárias (bicho real); a névoa tirou a cor, menos a faixa verde; cospe caroço das copas. |
| `enemy_forest_laurel_lizard` | **Lagarto-Gigante do Louro** / *Giant Laurel Lizard* | 05 (era o Lobo do Tugai) | [rápido] | os lagartos-gigantes das Canárias (bicho real); as fontes secaram e ele subiu para as copas. |
| `enemy_forest_smoke_peacock` | **Pavão de Fumaça** / *Smoke Peacock* | 06 (era a Ave-Graveto) | [distância], voa | o último pavão do jardim de Mihrshah, quase todo fumaça; joga os olhos acesos da cauda. "Quando os canais correrem de novo, ele vai lembrar." |
| `enemy_desert_thorn_lizard` | **Lagarto-Espinho** / *Thorn Lizard* | 10 (era o Chacal Apagado) | [rápido] | o lagarto que bebe o orvalho pelos sulcos das costas (bicho real); o **Fogo Devorador** secou as dunas e ele aprendeu a beber faísca. O Vô Jorge mostrou um para a Vó na estrada das caravanas. |
| `enemy_mountain_dimmed_lynx` | **Lince Apagado** / *Dimmed Lynx* | 14 Passo do Hahya (era o Lobo do Tugai) | [rápido] | bicho real; a mancha escura no flanco engole a neve: "um buraco no branco". |
| `enemy_mountain_smudge` | **Borrão** / *Smudge* | 15 Abrigos Pintados (era o Lobo do Tugai) | [rápido], voa | invenção: a coisa que comeu as pinturas uma cor de cada vez; vencido, as cores voltam para a parede. |
| `enemy_mountain_stone_jackdaw` | **Gralha de Pedra** / *Stone Jackdaw* | 16 Muralhas de Pedra Seca (era a Ave-Graveto) | [distância], voa | bicho real; forrava o ninho com lasquinhas e passou a levar o muro, pedra por pedra. |
| `enemy_cave_young_ubir` | **Ubır Jovem** / *Young Ubır* | 20 Terraços de Tufo (era a Aranha-Tecelã) | [rápido], voa | o ubır (§3.5) que ainda não aprendeu a se esconder em ninguém: bola de cabelo preto com brasa na boca; tem fome de luz. |

**Décima quarta leva (2026-11-12): os substitutos mais vistos das Regiões 10–16 (Mapas100 54, 55, 63, 64, 70, 75, 81–83, 85, 92, 95).**
(A décima terceira, Regiões 5–9, é `MidStandInBeastRecipes`.) Mesmo contrato (peso 0 no bioma, nível da área, números do bioma),
prefixo conforme o bioma do layout (`enemy_mountain_` em Farnkesh, no Mar Gelado e em Hinokura, `enemy_forest_` na Mistura, no
Profundo e no Céu). Modelos em `FarStandInBeastRecipes` (as aves usam o esqueleto de `MidStandInBeastRecipes`), Caderno em
`FarStandInBestiaryTexts`. Dois são [distância], voam e atiram: o Abutre do Pico (`Feather`) e a Pipa-Sombra (`LanternBolt`;
`RangedEnemiesTests`). A Chama Faminta é um bicho só para as três fogueiras das fichas (70, 75 e 85). Os ids ficam em
`OverworldLayouts.FarBeasts`; as constantes de região que eram só daquele papel passaram a apontar para lá (`Fark.Vulture`,
`Gelo.Floe`, `Rio.Locust`, `Mistura.Diorite/ShoulderBird/DreamBird`, `Profundo.SmallShadow/DarkBat/HungryFlame`,
`Ceu.OrbitWind/RunawayLantern`), e as que outros papéis dividiam ganharam constante nova (`Fark.ShadowKite`, `Gelo.Kul`,
`Fogo.HungryFlame`). Respeito: os *kul* só como os contos komi os dão (o povo da água que guarda os peixes); Ullikummi e
Upelluri só como a canção hurrita os dá, sem deus nenhum (a Pedra de Diorito são lascas da criança de pedra, a Ave do Ombro
mora no ombro do gigante); o Fogo Devorador é o fogo da Treva (§1.3); as Barcas de Luz e a Coluna da Glória como em §1.5.
Testes: `FarStandInBeastsTests` e `FarStandInBeastsPlacementTests`. Já no overworld no lugar dos substitutos, com as mesmas
clareiras, níveis e variantes noturnas; entre parênteses, o substituto que saiu. Estes doze saem das listas de "seguem com
substitutos"; o Sino Rachado e o Raio Preso (lanterna quebrada), o Mosquito da Tundra e a Faísca Raivosa (enxame de vaga-lumes)
continuam com substituto.

| `id` | pt / en | Ficha (Mapas100) | Tipo | Fonte / lore |
|---|---|---|---|---|
| `enemy_mountain_peak_vulture` | **Abutre do Pico** / *Peak Vulture* | 54 Escadaria dos Cestos, Mar de Nuvens e Bosque de Pináculos (era o Abutre de Brasa) | [distância], voa | os abutres que limpavam os picos e ganhavam as sobras nas pedras altas; o vento ruim se aninhou na gola de nuvem e agora guardam as escadas dos cestos. |
| `enemy_mountain_shadow_kite` | **Pipa-Sombra** / *Shadow Kite* | 55 Pináculos do Leste e Ermida Baixa (era a Lanterna Quebrada) | [distância], voa | invenção: as pipas de papel das crianças de Farnkesh, com rosto pintado, que seguiram voando sozinhas quando cortaram as cordas das pontes; cospem a luz das lamparinas que roubaram. |
| `enemy_mountain_drifting_floe` | **Gelo Flutuante** / *Drifting Floe* | 63 Banquisas do Oeste e Mar das Auroras (era a Neblina do Vento Ruim) | [controle], pesado | um pedaço da banquisa que se soltou com uma fita da aurora congelada dentro; vai atrás dos barcos e encosta o peso no que é quente. |
| `enemy_mountain_little_kul` | **Kul Pequeno** / *Little Kul* | 64 Fundo do Velho Kul e Lago do Kul (era o enxame de vaga-lumes) | [enxame], voa | os *kul* dos contos komi, o povo da água que guarda os peixes; os pequenos subiram pelos buracos de pesca atrás das lamparinas quando o Velho Kul azedou. |
| `enemy_mountain_hungry_flame` | **Chama Faminta** / *Hungry Flame* | 70 Caldeira, Borda Fria e Campos de Cinza; 75 Lago de Fogo; 85 Cova do Fogo Devorador e Chão que Acende (era o enxame de vaga-lumes) | [enxame], voa | linguinhas do Fogo Devorador (§1.3), o fogo que come e não esquenta; a luz que comeram arde no meio. |
| `enemy_forest_diorite_stone` | **Pedra de Diorito** / *Diorite Stone* | 81 Ombro de Upelluri, Clavícula e Diorito Crescente (era a Pedra que Rola) | [tanque], pesado | lascas de Ullikummi, a criança de diorito da canção hurrita que cresceu no ombro de Upelluri; andam em tocos procurando o resto de si. |
| `enemy_forest_shoulder_bird` | **Ave do Ombro** / *Shoulder Bird* | 81 Ombro, Dobras de Pedra e Beiral do Céu (era a Águia da Tempestade) | [rápido], voa | os pássaros que fazem ninho no ombro do gigante sem ele sentir; pegaram a cor e o gênio da pedra, com líquen nas costas. |
| `enemy_forest_dream_bird` | **Pássaro-Sonho** / *Dream Bird* | 82 Campo das Tábuas, Jardim Cortado e Tábuas do Sonho (era a Coruja-de-Fumaça) | [rápido], voa | invenção: o pássaro que saiu de um sonho escrito em barro e ficou com a escrita nas penas; a sombra quebrou as tábuas e ele perdeu o caminho de volta. |
| `enemy_forest_little_shadow` | **Sombra Pequena** / *Little Shadow* | 83 Fortaleza, Muralha em Arco, Cisternas e Beira do Abismo (era o Mordedor de Sombra) | [rápido] | invenção: as sombras que as lâmpadas da fortaleza jogam na parede e que saíram andando; roubam pavio porque é a única coisa de que têm medo. |
| `enemy_forest_dark_bat` | **Morcego da Treva** / *Bat of the Dark* | 83 Fortaleza, Nichos de Basalto e Beira do Abismo (era o Ubır, o Morcegão Bebe-Luz) | [enxame], voa | os morcegos das seteiras da última fortaleza que bebem as lâmpadas uma por uma, devagar; vêm em nuvem. |
| `enemy_forest_orbit_wind` | **Vento de Órbita** / *Orbit Wind* | 95 Órbitas Vazias e Esferas dos Arcontes, 99 Mirante, 100 Varanda (era a Águia da Tempestade) | [rápido], voa | os ventos que levam as luzes ao redor das esferas até subirem pela Coluna (§1.5); os arcontes tiraram este da órbita e ele gira em volta de quem chega. |
| `enemy_forest_runaway_lantern` | **Lanterna Fugitiva** / *Runaway Lantern* | 92 Ilhotas de Nuvem, Margens da Corrente e Foz Leste (era a Lanterna Quebrada) | [enxame], voa | uma lanterna de papel de uma Barca de Luz (§3.16) que não quis subir, fugiu do convés e queimou dois furinhos pra enxergar; apagada com jeito, a Barca volta pra buscar a luz. |

**Décima quinta leva (2026-11-12): os substitutos mais vistos que sobravam nas Regiões 5–16 (Mapas100 29, 31, 33, 34, 41, 42, 44, 47, 51, 91, 94).**
Mesmo contrato (peso 0 no bioma, nível da área, números do bioma). Modelos em `WideStandInBeastRecipes` (as aves usam o esqueleto de
`MidStandInBeastRecipes`), Caderno em `WideStandInBestiaryTexts`. Três são [distância], voam e atiram: o Abutre da Estepe e o Grou de
Papel (`Feather`) e o Espelho Quebrado (`LanternBolt`; `RangedEnemiesTests`). **Bioma:** os de Delta, Costa e Nartes seguem o bioma
do layout; o Abutre da Estepe, os três de Mingcheng e os dois do Céu ficam no bioma do substituto que trocaram (Deserto, Montanha,
Cavernas), para a economia de ouro e de Pedras da Alma das faixas 100–140 e 420–500 (`tools/balance`, afinada nos substitutos)
não mudar. Os ids ficam como texto nas constantes de cada região (o `tools/balance` lê as espécies de cada layout pelos textos
dele) e `OverworldLayouts.WideBeasts` aponta para elas. Os sapos-boi passaram a ter a própria variante noturna no lugar da manada de
queixadas. Respeito: os fogos-fátuos de Tarchon são só luzes do pântano presas em gaiola; os ushabtis de coral são os dos
Artífices (§2.2); a Fumaça é o 1º elemento da Treva (§1.3); as Barcas de Luz e a Roda como em §1.5 e §3.16. Testes:
`WideStandInBeastsTests` e `WideStandInBeastsPlacementTests`. Missões que apontavam para o substituto agora apontam para o bicho:
Tarchon "free_the_wisps" (Fogo-Fátuo), Torres Brancas "break_the_mirrors" (Espelho Quebrado). Seguem com substituto os papéis que
dividiam a constante: o Ushabti de Bronze Kushita, o Espantalho e os Arqueiros de Terracota, a Íbis de Luz, os linces, garanhões e
lobos-cinzentos/de neve dos Nartes, a Neblina do Rio e o Pesadelo de Fumaça.

| `id` | pt / en | Ficha (Mapas100) | Tipo | Fonte / lore |
|---|---|---|---|---|
| `enemy_desert_steppe_vulture` | **Abutre da Estepe** / *Steppe Vulture* | 51 (e o 50 das saigas; era o Abutre de Brasa) | [distância], voa | os abutres que seguiam manadas e cavaleiros; a sombra veio no vento das papoulas e agora seguem quem ainda anda. |
| `enemy_mountain_steppe_wolf` | **Lobo da Estepe Apagado** / *Dimmed Steppe Wolf* | 47 Vau, Pistas, Estepe de Soslan, Bétulas (era o lobo) | [rápido] | corriam ao lado dos cavalos do Soslan por alegria; a sombra os afinou e os ensinou a correr contra os cavalos. |
| `enemy_mountain_grass_storm` | **Tempestade de Capim** / *Grass Storm* | 51 Papoulas, Mil Cavalos e a Estepe dos Ventos (era a Neblina do Vento Ruim) | [controle], voa | o vento que levantava o capim de tarde e não o pôs mais de volta; uma faísca do último raio no meio. |
| `enemy_forest_marsh_wisp` | **Fogo-Fátuo Preso** / *Caged Marsh Light* | 33 Ilhas de Junco, Pântano, Tarchon, Ilhas que Andam (era a Lanterna Quebrada) | [enxame], voa | as luzes do pântano que os pescadores de Tarchon prendiam em gaiolas de junco; a Treva entrou nas gaiolas. |
| `enemy_forest_bull_frog` | **Sapo-Boi** / *Bullfrog* | 34 Brejo das Palmeiras, Charco do Ninki Nanka, Toca da Dragoa (era a queixada) | [tanque], pesado | cantavam a chuva toda tarde; o lodo entrou na garganta e a canção puxa os viajantes para a lama. |
| `enemy_forest_mudskipper` | **Peixe-Saltador de Lodo** / *Mudskipper* | 31 Lago e Bancos de Jhilpur, Charcos de Lótus (era o Peixe-Lanterna) | [enxame] | bicho real; anda na lama com as barbatanas, olhos em cima da cabeça; o lodo o ensinou a pular nas pessoas. |
| `enemy_desert_broken_mirror` | **Espelho Quebrado** / *Broken Mirror* | 42 Jardins em Terraço, Torres Brancas, Colinas dos Espelhos (era a Lanterna Quebrada) | [distância], voa | os espelhos de bronze que mandavam o luar para dentro das casas; rachados, mandam a lua em quem passa. |
| `enemy_mountain_paper_crane` | **Grou de Papel** / *Paper Crane* | 41 Anéis, Vilas do Anel, Colinas de Chá (era o Gavião-de-Penacho) | [distância], voa | os mil grous dobrados para um pedido que a sombra comeu; jogam dobras afiadas de papel. |
| `enemy_cave_smoke_locust` | **Gafanhoto de Fumaça** / *Smoke Locust* | 44 Painço, Arrozais Espelhados, Colinas de Chá (era o enxame de morcegos) | [enxame], voa | a Fumaça (§1.3) pousou onde havia fome e tomou a forma de gafanhoto, "fome com perna". |
| `enemy_forest_coral_ushabti` | **Ushabti de Coral** / *Coral Ushabti* | 29 Gedi, Kilwa Afundada, Husuni (era o Golem de Pedra) | [tanque], pesado | os respondedores dos Artífices deixados na cidade afogada; o coral cresceu por cima e eles esqueceram por quem esperavam. |
| `enemy_desert_confused_spark` | **Faísca Confusa** / *Confused Spark* | 91 Beira Oeste, Cais da Coluna, Píer do Sol (era o enxame de vaga-lumes) | [enxame], voa | faíscas que esperaram tanto a vez nas Barcas que esqueceram o que esperavam. |
| `enemy_cave_dark_drop` | **Gota de Treva** / *Drop of the Dark* | 94 Engrenagens e Jarras da Roda (era o Lodo de Gruta) | [rápido], pesado | a gota da Treva que sobe grudada nas jarras da Roda e desce atrás de mais luz para cair em cima. |

**Décima sexta leva (2026-11-13): papéis das Regiões 7–16 que ainda usavam bicho de outra região (Mapas100 38, 62, 65, 68, 78, 79, 81, 82, 91, 92, 99).**
Mesmo contrato da décima quinta. Modelos em `BorrowedRoleBeastRecipes` (`ProceduralEnemyKind` 324–335), Caderno em `BorrowedRoleBestiaryTexts`.
**Bioma e números:** todos ficam no bioma do substituto que trocaram e copiam vida, dano, recarga, XP e loot base dele (só o modelo, o nome
e o arquétipo de movimento da Pedra-Pomes mudam), para a economia das faixas 80–500 (`tools/balance`) não mudar. Nenhum atira. Os ids ficam
como texto nas constantes de cada região e `OverworldLayouts.BorrowedBeasts` aponta para elas. Missões que apontavam para o substituto agora
apontam para o bicho: Cais da Coluna "chase_the_stowaways" (Sombra Clandestina; era a Treva Rastejante) e "chase_the_swimming_shadows"
(Sombra Nadadora; era a Sombra Atrasada). Seguem com substituto os papéis que dividiam o bicho: a Sombra que Pisa as Flores e a Sombra do
Meio-Dia no Céu, a Rachadura e a Raiz-Gelo do Mar Gelado, o Polvo de Obsidiana e a Cinza Viva de Hinokura. Testes: `BorrowedRoleBeastsTests` e
`BorrowedRoleBeastsPlacementTests`.

| `id` | pt / en | Ficha (Mapas100) | Tipo | Fonte / lore |
|---|---|---|---|---|
| `enemy_cave_living_crevasse` | **Fenda Viva** / *Living Crevasse* | 38 Geleira e Campo de Seracs (era o Lodo de Gruta) | [controle], pesado | a geleira racha e fecha todo ano e os pastores sabiam onde; desde a sombra as rachaduras andam, deitam no caminho e fecham. |
| `enemy_mountain_ice_mole` | **Toupeira-Gelo** / *Ice Mole* | 62 Terra Congelada, Galerias, Fiorde das Presas (era a Marmota Gigante) | [enxame] | cavavam os túneis pequenos ao lado dos do mamute e seguravam as galerias; o frio entrou nas unhas. |
| `enemy_mountain_flying_pumice` | **Pedra-Pomes Voadora** / *Flying Pumice* | 65 Costa das Fumarolas e Canais de Lava (era a Pedra que Rola) | [enxame], voa | a pomes que os respiros sopram e caía de volta como areia dos polidores; desde a sombra não desce, boia em bando com a brasa nos furos. |
| `enemy_forest_glass_urchin` | **Ouriço de Vidro** / *Glass Urchin* | 68 Ilhotas de Vidro e Recife de Obsidiana (era a Tartaruga de Pedra) | [tanque], pesado | a lava que entrou no mar virou vidro preto e um pouco dela continuou andando; o calor do vulcão ainda dentro. |
| `enemy_mountain_instant_lizard` | **Lagarto do Instante** / *Instant Lizard* | 78 Deserto do Relógio e Areia das Horas (era o Lagarto de Lava) | [rápido] | vive no instante de atraso das sombras de Zurvan (só relógio, §1.1); o relógio de sol nas costas é o único certo do deserto. |
| `enemy_mountain_black_catfish` | **Bagre Negro** / *Black Catfish* | 79 Poços de Betume (era o Lúcio Gigante) | [tanque] | mantinha o piche mole para os calafates dos barcos de junco (Atrahasis); o piche escureceu e ele junto, vira as balsas. |
| `enemy_mountain_tremor` | **Tremor** / *Tremor* | 81 Dobras de Pedra e Clavícula do Gigante (era o Ushabti de Pedra) | [controle], pesado | o arrepio da pele de pedra de Upelluri onde o diorito cresce, que ganhou forma e anda derrubando gente. |
| `enemy_mountain_erasing_water` | **Água que Apaga** / *Erasing Water* | 82 Água Parada e Tábuas Quebradas (era a Foca-Sombra) | [controle], pesado | a água parada que sobe entre as tábuas do sonho e lava os sinais até ficarem em branco. |
| `enemy_forest_stowaway_shadow` | **Sombra Clandestina** / *Stowaway Shadow* | 91 Píer da Lua, Fresta das Luzinhas, Beira Oeste, Cais (era o Jardim-Sombra) | [controle], pesado | a sombra que se esconde no porão de uma Barca (§1.5) debaixo de um trapo de vela para subir sem pagar. |
| `enemy_forest_swimming_shadow` | **Sombra Nadadora** / *Swimming Shadow* | 92 Foz Oeste, Margens da Corrente, Ilhotas (era a Sombra Atrasada) | [controle], voa | sombra não nada na água, mas nada na luz: desliza sob o Rio de Luz (§3.16) e bebe o brilho dos barcos. |
| `enemy_mountain_wrong_current` | **Corrente Errada** / *Wrong Current* | 92 Rio de Luz, Foz Leste, Margens (era a Neblina do Vento Ruim) | [controle], voa | um fio do Rio de Luz que corre ao contrário, de volta para baixo, enrolado no nó onde virou. |
| `enemy_mountain_sky_crack` | **Rachadura do Céu** / *Sky Crack* | 99 Rachaduras do Oeste e Varanda (era a Neblina do Vento Ruim) | [controle], voa | as rachaduras pequenas do céu que se soltaram com as beiradas douradas e andam pelo mirante querendo deixar entrar mais sombra. |

**Décima sétima leva (2026-11-13): os papéis da Estepe dos Nartes (Região 9) que ainda usavam bicho de outra região (Mapas100 47–52).**
Mesmo contrato da décima sexta. Modelos em `LentRoleBeastRecipes` (`ProceduralEnemyKind` 360–371), Caderno em `LentRoleBestiaryTexts`.
**Bioma e números:** cada um fica no bioma do substituto e copia vida, dano, recarga, XP e loot base dele, para a economia da faixa 100–150
(`tools/balance`) não mudar. As duas águias, o pica-pau e o corvo de fumaça atiram a pena do gavião e do corvo que trocaram (os outros batem).
Os ids ficam como texto nas constantes do `Nart` e `OverworldLayouts.LentBeasts` aponta para elas. Na estepe não sai mais de dia o gavião,
o chacal, o corvo, o lodo, a queixada nem o urso do Pamir (a manada de queixadas segue à noite nos javalis). Missões que apontavam para o
substituto agora apontam para o bicho: Soslan "chase_the_eagles" (Águia-Dourada; era o gavião), Fortim "chase_the_boars" (Javali da Colina;
era a queixada) e "chase_the_crows" (Corvo-Mensageiro; era o Corvo-da-Treva), Arvoredo "chase_the_smoke_birds" (Pica-Pau de Fumaça; era a
mãe-da-lua). Seguem com substituto: o Lobo-Cinzento (51, o lobo) e o Grifo Guardião (48, o grifo jovem). Testes: `LentRoleBeastsTests` e
`LentRoleBeastsPlacementTests`.

| `id` | pt / en | Ficha (Mapas100) | Tipo | Fonte / lore |
|---|---|---|---|---|
| `enemy_mountain_dimmed_golden_eagle` | **Águia-Dourada Apagada** / *Dimmed Golden Eagle* | 47 Acampamento e Estepe dos Ventos (era o Gavião-de-Penacho) | [distância], voa | as águias de punho dos cavaleiros de Soslan; a sombra assentou no dourado da nuca como cinza e elas mergulham nos cavaleiros. |
| `enemy_desert_golden_stag` | **Cervo de Ouro Apagado** / *Dimmed Golden Stag* | 48 Kurgans de Ouro (era o Chacal Apagado) | [rápido] | os cervos de ouro do estilo animal enterrados nos kurgans, galhada em espiral; a sombra entrou nas covas e alguns levantaram e correm, frios e foscos. |
| `enemy_desert_giant_ibex` | **Cabra-Montês Gigante** / *Giant Ibex* | 49 Pastos dos Gigantes (era o Chacal Apagado) | [rápido] | as cabras do tamanho das casas redondas que os gigantes ordenham em panelas de pedra; saíram dos cercados dando chifrada em tudo menor que gigante. |
| `enemy_forest_hill_boar` | **Javali da Colina** / *Hill Boar* | 49 Pastos e Fortim (era a Queixada) | [tanque] | fuçavam as bolotas que os gigantes deixavam; desde a sombra descem de noite e comem a despensa do Fortim, panela e tudo. |
| `enemy_cave_smoke_messenger` | **Corvo-Mensageiro de Fumaça** / *Smoke Messenger Crow* | 49 Fortim, Tábuas, e 52 Torres Svan (era o Corvo-da-Treva) | [distância], voa | como os gigantes do *Livro dos Gigantes* que levam os sonhos para serem lidos, os corvos levavam os sonhos pequenos numa tabuinha; viraram fumaça e trazem os ruins. |
| `enemy_mountain_smoke_woodpecker` | **Pica-Pau de Fumaça** / *Smoke Woodpecker* | 50 Arvoredo de Haripushpa (era o Gavião-de-Penacho) | [distância], voa | tiravam os besouros da casca do povo-tronco; desde a sombra o batuque faz a casca fumegar e eles batucam nos viajantes. |
| `enemy_cave_dimmed_fungus` | **Fungo Apagado** / *Dimmed Fungus* | 50 Arvoredo, Taiga e Lago de Degelo (era o Lodo de Gruta) | [controle], pesado | o fungo que devolve a madeira morta em terra; a sombra deu fome de árvore viva e ele anda no toco e solta esporos que pesam as pernas. |
| `enemy_forest_taiga_lynx` | **Lince da Taiga** / *Taiga Lynx* | 50 Taiga de Lariços e Lago de Degelo (era o Lobo do Tugai) | [rápido] | velhos e quietos como os lariços; a sombra não deu barulho, deu fome, e eles esperam o povo-tronco na beira das trilhas de casca. |
| `enemy_forest_dimmed_stallion` | **Garanhão Apagado** / *Dimmed Stallion* | 51 Pradaria dos Mil Cavalos (era o Lobo do Tugai) | [rápido] | os garanhões que guiam as manadas até a taça dos Nartes; a crina virou fumaça e eles levam o rebanho para longe do poço. |
| `enemy_mountain_iron_eagle` | **Águia de Ferro** / *Iron Eagle* | 52 Torre de Vigia, Corrente e Cume (era o Gavião-de-Penacho) | [distância], voa | fazem ninho na corrente do herói acorrentado e forram com a ferrugem dela; a sombra fez da ferrugem pena, com um elo na perna. |
| `enemy_mountain_caucasus_bear` | **Urso do Cáucaso Apagado** / *Dimmed Caucasus Bear* | 52 Torre de Vigia e Cume de Ohya (era o Urso-Pardo do Pamir) | [tanque], pesado | dormem o inverno debaixo da corrente e ganham mel das torres svan; a sombra acordou um cedo e bravo, com um elo quebrado no pescoço. |
| `enemy_forest_snow_wolf` | **Lobo de Neve** / *Snow Wolf* | 52 Cume de Ohya (era o Lobo do Tugai) | [rápido] | só desciam das torres svan nos piores invernos; com a sombra e Ohya no cume todo inverno é o pior, e eles caçam no branco quem sobe a corrente. |

### 3.3 Chefes de região (apagados, nunca vilões)

| Antigo (`id`) | Novo | O que era antes da sombra | Fonte |
|---|---|---|---|
| Lobo Alfa (`enemy_forest_alpha_wolf`) | **Kök-Böri, o Lobo Celeste** / *Kök-Böri, the Sky Wolf* | o lobo que guiava os viajantes no tugai | *kök böri*, "lobo azul/celeste", lobo ancestral das lendas túrquicas e uigures (Asena; lenda uigur do lobo, Wei Shu). |
| Guardião da Ruína (`enemy_desert_ruin_guardian`) | **O Grande Ushabti** / *The Great Ushabti* | o maior respondedor dos Artífices, que vigiava o selo; o painel dele traz o **aviso** | ushabti (Kemet). Pirâmides de Meroé como cenário da arena. |
| Senhor da Guerra da Montanha (`enemy_mountain_warlord`) | **Hahya, o Gigante Guerreiro** / *Hahya the Giant Warrior* | gigante que sonhou com a destruição, se arrependeu e virou guardião da serra; a sombra o fez esquecer | *Livro dos Gigantes*: Hahya tem sonhos e procura quem os explique. |
| Habitante Ancestral (`enemy_cave_dweller`) | **Tages, o Habitante Ancestral** / *Tages the Ancient Dweller* | criança-velha antiga que brotou da terra; tem medo do escuro e roubava luz por solidão | Tages (mito etrusco). |

### 3.4 Chefes do mundo e Invasão (arcontes)

| Antigo (`id`) | Novo (dia / noite) | Fonte |
|---|---|---|
| `forest_*` Saklas, o Lobo-Guará Encantado / o Lobisomem-Rei da Mata | **Saklas, o Lobo de Cinzas** / **Saklas, o Rei Mardagayl** | Saklas (arconte); mardagayl (§3.5). O lobo-guará é brasileiro e sai. |
| `escorpiao_rei` Namrael, o Escorpião-Rei / da Lua Cheia | **mantém** | Namrael (arconte). |
| `gigante_da_serra` Mazan (já no código) | **mantém Mazan**: "Mazan, o Gigante da Serra" / "Mazan, o Gigante da Pedra-Lua" | *māzan*, gigante/monstro (médio-persa maniqueu). |
| `rei_da_revoada` Príncipe das Trevas, Rei da Revoada / o Morcegão-Vampiro | **Mahaway, o Rei da Revoada** / **Mahaway, o Morcegão da Treva** | Mahaway, o gigante voador do *Livro dos Gigantes*. O Príncipe vira o vilão final (§1.4). |
| Invasão do Cangaço | **Invasão de Āz** / *Āz's Raid* | Āz (cobiça). |
| Az, capitã/capitão do bando | **Āz, a Capitã Cobiçosa** / *Āz, the Greedy Captain* (sempre feminino: no código há "capitão" e "capitã" misturados, corrigir) | Āz é feminina nos textos. |
| "{0} do bando" | **"{0} do bando de Āz"** | — |
| Bandidos "com chapéu de couro" | bichos com **coroinhas de lata dourada** que Āz põe neles ("tudo o que brilha é meu") | invenção. |
| Botão "Defender do Cangaço" | **"Defender de Āz"** | — |

#### 3.4.1 Chefes do mundo das Regiões 5–16 (2026-11-09 a 11-11)

Um chefe por região, no último mapa dela (`RegionBossRules.MidSpawns` para 5–9, `Spawns` para 10–16; Caderno e falas de
luta em `RegionBossTexts`; nome em `ContentTexts`). Arena plana com anel de pedra; o chefe acorda a 90 m e dorme (volta com
vida cheia) a 125 m. Mesma regra dos arcontes (§1.4): **são apetites, não deuses.** Os "que nunca morrem" só **recuam**
(cospem a luz e voltam ao fundo, menores), como o Príncipe. Nenhum chefe é necessário para a história do meio
(`Docs/HistoriaMeio_20a100.md`): os doadores só **avisam** que ele está ali ("deixem ele dormir").

| `id` | pt / en | Mapa · Nv | Golpe próprio | Fonte / no jogo |
|---|---|---|---|---|
| `enemy_mountain_hedammu` | **Hedammu, a Serpente Gulosa do Mar** / *Hedammu, the Gluttonous Sea Serpent* | 30 Mangues de Liyongo · 72 | mordida em cone à frente ("pro lado dela!"); recua | a serpente do mar que comia tudo e nunca enchia, da *Canção de Hedammu* (ciclo hurrita/hitita de Kumarbi). **Feminina.** Āz gosta dela. Vencida, cospe as faíscas e volta ao fundo, menor. |
| `enemy_mountain_swallowing_mire` | **O Lodo-que-Engole** / *The Swallowing Mire* | 35 Coração do Lodo (área `olho_do_lodo`) · 88 | puxão: fica parado, a lama brilha, e engole quem está nela; mais forte à noite; recua | o Lodo, quarto dos cinco mundos da Treva, o da água turva e "dos que nadam" (Teodoro bar Konai). Lama sem rosto e sem osso: não se vence de vez. Só surge com as **5 Fontes do Lodo** acesas (§3.4.2), e cada fonte que apaga na luta o fortalece 10%. |
| `enemy_mountain_aydahar` | **Aydahar, o Dragão do Passo** / *Aydahar, the Dragon of the Pass* | 40 Passo de Wakhan · 98 | sopro em linha reta passo abaixo ("um passo pro lado"); recua | *aydahar*, o dragão comprido de bigode dos contos cazaques e da estepe (do persa *aždahā*). Deitado no ouro de Āz sem saber por quê, como cachorro que guarda porta que ninguém usa. Libertado, voa embora e o passo abre para as caravanas. |
| `enemy_mountain_four_gluttons` | **Os Quatro Gulosos** / *The Four Gluttons* | 46 Cidadela dos Quatro Gulosos · 140 | um apetite manda por semana e outro de noite: **Taotie** come o chão (sair da marca) e as oferendas de moeda que o vaso derrama (pega já, ou ele devolve em dobro ao cair); **Hundun** apaga a luz fora da marca (ficar); **Qiongqi** troca os passos por 3 s com aviso (chegar perto); **Taowu** bate em volta (ficar longe) | os "quatro perigos" (*sì xiōng*) das lendas chinesas antigas (*Zuo Zhuan*), num corpo de bronze só, cada um um apetite de Āz sozinho; vencidos, voltam a ser figuras nos vasos (o *taotie* dos bronzes antigos). |
| `enemy_mountain_ohya` | **Ohya, o Gigante Irmão** / *Ohya, the Giant Brother* | 52 Corrente de Amirani (área `cume_de_ohya`) · 150 | gira a corrente no comprimento todo ("bem perto ou bem longe") | ʾOhyā, irmão de Hahya no *Livro dos Gigantes* (§1.4). A corrente lembra o Amirani acorrentado no Cáucaso (épica georgiana), só como cenário. Vencido, solta a corrente e vai morar com o Hahya ("brigam por causa da comida"). |
| `enemy_mountain_wind_king` | **O Rei-Vendaval** / *The Gale King* | 58 Ninho da Huma · 200 | — | o Vento Destruidor (§1.3), não deus. |
| `enemy_mountain_old_kul` | **O Velho Kul** / *Old Kul* | 64 Fundo do Velho Kul · 240 | — | o *kul* das águas dos contos komi, azedo pela sombra. |
| `enemy_mountain_ember_glutton` | **O Glutão de Brasa** / *The Ember Glutton* | 70 Caldeira do Fogo Devorador · 280 | — | o Fogo Devorador (§1.3). |
| `enemy_mountain_apep` | **Apep, a Serpente que Bebe o Rio** / *Apep, the Serpent Who Drinks the River* | 76 Banco de Areia de Apep · 320 | bebe o rio; recua e volta na noite seguinte | a serpente que ataca a barca do Sol (Kemet), só como o conto dá. Ver a barca encalhada (§3.4.2). |
| `enemy_mountain_dream_of_destruction` | **O Sonho da Destruição** / *The Dream of Destruction* | 82 Campo das Tábuas do Sonho · 360 | tábuas que caem | o sonho de Ohya e Hahya no *Livro dos Gigantes*. |
| `enemy_mountain_great_dark` | **A Grande Treva** / *The Great Darkness* | 90 Portão da Terra da Treva · 440 | as cinco formas | a sombra do Príncipe (§1.4); recua, nunca morre. |
| `enemy_mountain_az_captain` | **Āz, a Capitã Cobiçosa** / *Āz, the Greedy Captain* | 96 Jardim de Luz · 478 | a Arca leva do que o herói tem; recua | Āz (§1.4). Vencida, devolve a arca, envergonhada. |

**Loot e troféus (2026-11-11).** Cada chefe solta um loot próprio (`item_chefe_*`, ícone e texto em `ContentTexts`), que
destrava as 10 peças de roupa de chefe (Tear, Forja, Tear de mestre) e um **troféu de casa** feito na Forja depois de
vencê-lo (`casa_trofeu_025`–`036`, `HouseCraftRules.WorldBossTrophies`; `Docs/Housing.md`). Regra do texto: o loot é a luz
**devolvida**, nunca pedaço arrancado ("não prende mais ninguém", "é fogo vivo, não fogo devorador").

| Chefe | Loot (`id`) | Troféu |
|---|---|---|
| Hedammu | Escama de Hedammu (`item_chefe_escama_hedammu`) | Cabeça entalhada de Hedammu (`casa_trofeu_025`) |
| o Lodo-que-Engole | Lama Limpa do Lodo (`item_chefe_lama_limpa`): lama que só segura Água Viva | Pedestal da lama limpa do Lodo-que-Engole (`026`) |
| Aydahar | Escama de Aydahar (`item_chefe_escama_aydahar`) | Estandarte de escamas de Aydahar (`027`) |
| os Quatro Gulosos | Bronze Antigo dos Vasos (`item_chefe_bronze_antigo`) | Vaso de bronze dos Quatro Gulosos (`028`) |
| Ohya | Elo de Ferro Antigo de Ohya (`item_chefe_ferro_antigo`) | Corrente partida de Ohya (`029`) |
| o Rei-Vendaval | Pena do Rei-Vendaval (`item_chefe_pena_vendaval`): aponta para onde a Huma voa | Leque de penas do Rei-Vendaval (`030`) |
| o Velho Kul | Barbatana-Prata do Velho Kul (`item_chefe_barbatana_kul`) | Quadro da barbatana-prata do Velho Kul (`031`) |
| o Glutão de Brasa | Brasa Viva do Glutão (`item_chefe_brasa_viva`) | Braseiro da brasa viva do Glutão (`032`) |
| Apep | Escama de Apep (`item_chefe_escama_apep`) | Barca de luz vencendo Apep, em miniatura (`033`) |
| o Sonho da Destruição | Tábua do Sonho (`item_chefe_tabua_sonho`): "o fim é bom" | Painel das Tábuas do Sonho (`034`) |
| a Grande Treva | Lasca do Selo da Treva (`item_chefe_lasca_selo`) | Estátua do Selo da Treva remendado de luz (`035`, Lendário) |
| Āz | Semente de Luz de Āz (`item_chefe_semente_luz`) | Estátua de Āz devolvendo a arca (`036`, Lendário) |

#### 3.4.2 Mecânicas que mudam o caminho (onde ficam no mundo)

| Mecânica | Onde | No jogo | Tema |
|---|---|---|---|
| **Poços de vento** | 24 Ninho de Mahaway (Grutas do Limiar) | com asas, [E] sobre o poço: a corrente ergue e o herói **plana de borda a borda** do abismo, em vez de 250–450 m pela Última Ponte de Corda. Sem asas, o caminho longo é a ponte. A **revoada de Mahaway** bica quem fica no ar; o pouso dá uma rajada que empurra os bichos. O poço "prende o fôlego" entre um voo e outro. (`LimiarWindWellRules`) | o Vento Vivo (Maithe) contra a revoada de Mahaway (§1.4). |
| **A barca encalhada de Apep** | 76 Banco de Areia de Apep, posto Proa Encalhada (Rio das Doze Horas) | [E] chama os remadores: 90 s protegendo o casco dos bichos e segurando a **proa** quando Apep "bebe o rio" (puxão). Solta, a barca navega pelo banco até o fim do dia. Se o casco cede ou o herói sai, a barca volta para a areia. (`RioBarqueDefenseRules`) | a barca do Sol contra Apep: a luz segue viagem mesmo de noite. |
| **Os cinco portões das covas** | 83 Fortaleza da Última Lâmpada (Limiar Profundo) | um portão para cada cova: **da Fumaça, do Fogo Devorador, do Vento Destruidor, do Lodo e da Treva** (os cinco da Treva, §1.3). Cada portão está selado do outro lado e só abre depois de o herói chegar **a pé** à lâmpada da cova; aí a Fortaleza encurta o caminho (600–1900 m). Na **Maré de Treva** os guardiões trancam todos. A lâmpada do portão queima os bichos da porta. (`ProfundoGateRules`) | as cinco covas da Terra da Treva, cada uma com a sua lâmpada guardiã. |
| **As 5 Fontes do Lodo** | 35 Coração do Lodo (Delta de Jhilpur), em espiral até o **Olho do Lodo** | o lodo preto morde, puxa para o Olho e encolhe a cada fonte purificada (3 s parado com [E]/Usar). Fonte limpa vira **Água Viva**: zona segura por 2 min que fere os bichos. Com as 5 acesas o **Lodo-que-Engole** sobe no Olho (§3.4.1). Prêmio diário: 1 Pedra da Água Viva. (`LodoFountainRules`) | Água Viva (Joaquim) contra o Lodo. |

### 3.5 Espécies da noite

| Antigo (`id`) | Novo (pt / en) | Silhueta (mantém) | Fonte |
|---|---|---|---|
| Lobisomem-do-sertão (`enemy_forest_werewolf`) | **Mardagayl, o Lobo-das-Sete-Noites** / *Mardagayl, the Seven-Night Wolf* | lobão desgrenhado que empina | *mardagayl*, "homem-lobo" do folclore armênio (pessoa encantada em lobo por sete anos; no jogo são "sete noites", e a luz desfaz o encanto). |
| Morcego-vampiro gigante (`enemy_cave_vampire_bat`) | **Ubır, o Morcegão Bebe-Luz** / *Ubır, the Light-Drinker Bat* | morcegão vinho | *ubır*, espírito "sugador" do folclore tártaro/tchuvache. No jogo ele bebe **luz**, não sangue. |
| Coruja-rasga-mortalha (`enemy_forest_shroud_owl`) | **Coruja-de-Fumaça** / *Smoke Owl* | cara branca de coração | Fumaça é o 1º elemento da Treva (§1.3). Corujas são boas em várias culturas (Ainu, Atena), então o nome fica descritivo, sem mito. |
| Enxame de vaga-lumes-de-fogo (`enemy_desert_firefly_swarm`) | **Enxame de Faíscas Presas** / *Swarm of Trapped Sparks* | 9 lanternas que piscam | mecânica central: luz presa que a sombra fez brigar. Ao vencer, as faíscas **sobem** (VFX bonito). |
| Queixada da Caipora (`enemy_forest_peccary`) | **Javali do Lodo** / *Mire Boar* (bando) | porco escuro de crista | Lodo, o elemento da Treva contrário à Água Viva; javalis do tugai (bicho real). |
| Mãe-da-lua (`enemy_mountain_potoo`) | **Ave-Graveto de Omöl** / *Omöl's Twig-Bird* | ave-graveto de bocão rosa | Yen e Omöl, irmãos criadores bom/trapalhão do mito komi (fino-úgrico, dualista). As "criaturas desajeitadas" são de Omöl. |

### 3.6 Comidas (Cozinha da Vó → **Forno da Vó Edna**)

Efeitos e ids iguais. O "Fogão a lenha" vira o **tandir** (forno de barro), em pt "o forno da Vó".

| Antigo | Novo (pt / en) | Favorito de | Fonte |
|---|---|---|---|
| Cuscuz | **Nan do Tandir** / *Tandir Bread* | — | pão redondo assado na parede do tandir (sogdiano/uigur). |
| Café coado | **Chá de Karkadé** / *Karkadé Tea* | Maithe | chá vermelho de hibisco da Núbia e de Kemet. |
| Pamonha | **Pão de Tâmara** / *Date Bread* | Maria | tâmaras dos oásis, presentes em toda a Rota da Seda e no Nilo. |
| Rapadura (prato) | **Torrão Dourado** / *Golden Brick* (decidido: o título do jogo continua **Rapadura**, mas o prato vira Torrão Dourado) | Ícaro | doce de cana/mel em tijolo. Nos contos, doces e frutas guardam "luz do sol". |
| Baião de dois | **Plov da Vó** / *Grandma's Plov* | Joaquim ("sempre repete") | arroz com cenoura e carne da Ásia Central (*polo* uigur, *osh* sogdiano). |
| Queijo coalho assado | **Samsa Assada** / *Baked Samsa* | Lavine | pastel de forno de tandir (uigur/uzbeque). |
| Bolo de milho (cutscenes) | **Bolo de Damasco** | — | damascos da Sogdiana. |
| Café (genérico nas falas) | **chá** | — | — |
| Paçoca, cocada, queijo, pamonha (NPCs) | melão em fatias, damasco seco, romã, halva, *kumis* para os adultos (fica fora das falas para crianças), iogurte com mel | Rota da Seda. |

### 3.7 Pedras de refino ("Pedras do Sertão" → **Pedras da Luz Presa**)

| Antigo (`id`) | Novo (pt / en) | Fonte |
|---|---|---|
| Pedra da Bênção (`item_pedra_bencao`) | **Pedra-Faísca** / *Spark Stone* | faísca = luz presa (§1.1). A comum, que sobe até +6 sem falhar. |
| Pedra da Alma (`item_pedra_alma`) | **Pedra da Alma Viva** / *Living Soul Stone* | Alma Viva (*nafšā ḥayyā*). |
| Pedra da Vida | **Pedra da Água Viva** / *Living Water Stone* | Água Viva (elemento de luz; *mia hiia* mandeu como eco). |
| Pedra do Caos | **Pedra da Mistura** / *Mixture Stone* | a Mistura, o Tempo do Meio. Junta luz e sombra, por isso só o Uasamonga a usa. |
| Amuleto da Sorte | **Amuleto de Bes** / *Bes Charm* | Bes, protetor das casas e das crianças em Kemet; amuletos dele eram de sorte e proteção. |

### 3.8 Tacho Encantado → **Uasamonga da Vó Edna**

| Antigo | Novo | Fonte |
|---|---|---|
| Tacho Encantado da Vó (janela, `VillageTachoPanel`) | **Uasamonga da Vó Edna** / *Grandma Edna's Uasamonga*. É uma taça-caldeirão mágica de cobre: "ela sabe quem merece". | *Wasamonga/Uatsamongæ*, a taça mágica dos Nartes ossetas, que se enche sozinha e revela a verdade (sagas dos Nartes). |
| "Proteção do Caos" | **Proteção da Mistura** | §3.7 |
| Frase da regra infantil | "Se a Uasamonga não quiser, os materiais vão embora, mas a sua peça fica do jeitinho que estava." | — |

**Asas (5 estilos, ids iguais `item_asas_*`, `item_capa_vaqueiro_*`):**

| Antigo | Novo | Primo | Fonte |
|---|---|---|---|
| Asas de Carcará | **Asas de Huma** / *Huma Wings* | Maithe | Huma, ave persa que nunca pousa; a sombra dela dá sorte. |
| Capa de Vaqueiro Encantada | **Capa de Feltro dos Nartes** / *Nart Felt Cloak* | Joaquim | capa de feltro (*burka*) dos heróis nartes. |
| Asas de Beija-flor | **Asas de Aralez** / *Aralez Wings* | Maria | aralez, cão-espírito alado que cura (Armênia). |
| Asas de Asa-branca | **Asas de Bronze de Kurdalægon** / *Kurdalægon's Bronze Wings* | Ícaro | o ferreiro celeste (Nartes). |
| Asas de Vaga-lume | **Asas de Vaga-lume** (mantém) | Lavine | vaga-lume não é folclore regional e combina com "faíscas". |

### 3.9 Frutas ("Frutas do Sertão" → **Frutas de Luz**)

| Antigo | Novo | Fonte |
|---|---|---|
| Fruta do Mandacaru (+1 ponto, até 2) | **Melão de Luz** / *Light Melon* | os maniqueus diziam que melões e pepinos guardam muita luz (Agostinho, *De moribus Manichaeorum* 16). Dito como "curiosidade do Vô". |
| Fruta do Umbu (devolve 1 ponto) | **Romã do Recomeço** / *Pomegranate of Second Chances* | romã, fruta-símbolo da Pérsia e da Armênia (no conto armênio, "caíram três romãs do céu"). |

### 3.10 Patuá e Bênção (proteções ao morrer)

| Antigo (`id`) | Novo | Fonte |
|---|---|---|
| Patuá da Vó Zefa (`item_patua_vo_zefa`) | **Tumar da Vó Edna** / *Grandma Edna's Tumar*: amuleto triangular de pano bordado | *tumar*, amuleto triangular costurado da Ásia Central e do mundo túrquico. |
| Bênção da Romaria (`RomariaBlessing`) | **Lâmpada Guardiã** / *Guardian Lamp*: "acendemos uma lâmpada por você" | lâmpadas acesas como cuidado; sem prece. |
| Oratório de cada cidade | **Nicho da Lâmpada**: Vila (Ramrat), Qocho (Yardena, na cisterna), Litema (Coti, nicho de pedra), Lamparinas (Parthenos, a "Lâmpada-Mãe") | §3.1 |
| Ícone HUD "Bênção" | **"Lâmpada"** | — |
| Frase de evento de sala "Uma bênção pra coragem..." | "Uma lâmpada pra coragem e outra pra sorte. Vamos!" | — |

### 3.11 Facções do servidor

| Antigo | Novo | Fonte |
|---|---|---|
| Bandeirinhas do Sol (dia, amarelo/vermelho) | **Remadores do Sol** / *Sun Rowers*: dourado/laranja, emblema de barca com sol | Barca do Sol (§1.5). |
| Vaga-lumes da Lua (noite, azul/prata) | **Remadores da Lua** / *Moon Rowers*: azul/prata, emblema de barca crescente | Barca da Lua; a lua-sapato do Kaggen como piada da Lavine. |
| Escolha com a Vó Zefa no nível 20 | escolha com a **Vó Edna** no nível 20 | — |
| Ideia de emblema (opcional) | as duas facções são **as duas metades de uma cabaça**: juntas, o mundo inteiro | imagem da cabaça-mundo da cosmogonia fon. Só a imagem, sem nomes de vodun. |
| "Terras de Disputa" | **Margens da Mistura** | — |

### 3.12 Fora da lei e Procurado

| Antigo | Novo | Fonte |
|---|---|---|
| "Fora da lei" | **"Tocado por Āz"** / *Āz-Touched*: quem derruba outro jogador por cobiça | Āz = cobiça. |
| Cartaz "PROCURADO" (faroeste) | **"AVISO DA CARAVANA"**, com rosto, nome e recompensa | caravançarás da Rota da Seda tinham quadros de avisos e guardas. |
| Nome vermelho | **nome com fumaça cinza-violeta** (o vermelho fica para a Noite Rubra da Lavine) | Fumaça = Treva. |
| Delegacia / cadeia / fiança | **Posto da Guarda do Caravançará** / "pagar a **reparação**" | — |
| Caçador de recompensa | **Rastreador de recompensa** | Patig ensina. |

### 3.13 Salas de evento

| Antigo (`id`) | Novo | Fonte |
|---|---|---|
| Terreiro das Assombrações (ondas, Praça do Diabo do MU) | **Pátio dos Mazans** / *Mazan Yard*: ondas de bichos apagados | *mazanān*. |
| Convite (`item_convite_terreiro`) | **Convite do Pátio** | — |
| Torre do Engenho Velho (Castelo de Sangue do MU) | **Torre da Roda das Doze Jarras** / *Tower of the Twelve-Jar Wheel* (curto: "Torre das Jarras") | roda com doze vasos (Acta Archelai 8); torre em degraus inspirada no zigurate elamita de Chogha Zanbil. |
| Chave (`item_chave_engenho`) | **Chave da Torre** | — |
| Portão do Engenho | **Portão da Torre** | — |
| Ponte que baixa | mantém | — |
| Guardião da Torre | **Ullikummi, o Gigante de Diorito** / *Ullikummi, the Diorite Giant* | *Canção de Ullikummi* (hurrita/hitita): gigante de pedra que cresce sobre o ombro de Upelluri. |
| Moenda de Ouro (o item a entregar) | **Jarra de Ouro** / *Golden Jar*: "a 12ª jarra da roda" | Acta Archelai. |
| Seu Malaquias, o vigia do engenho | **Adda, o Vigia da Torre** | §3.1 |
| "O engenho vai cantar de novo" | "A roda vai girar de novo!" | — |

### 3.14 Baús e depósitos

| Antigo | Novo | Fonte |
|---|---|---|
| Baú da Vó Zefa | **Baú da Vó Edna** ("baú" é neutro, fica) | — |
| Baú do Cangaço +1…+5 | **Arca Resgatada +1…+5** / *Rescued Chest*: o que Āz escondeu, devolvido | — |
| Achados e Perdidos | já removido por decisão do usuário, não volta | — |
| Depósito da casa | mantém | — |
| Bornal de couro (`item_bornal_couro`, +2 linhas) | **Alforje de Feltro** / *Felt Saddlebag* | alforje de feltro dos nômades da estepe. |

### 3.15 Festival: **o Bema** (substitui a Festa Junina, ids `FestaJunina*` ficam)

**O que é, em linguagem de criança:** uma vez por ano a vila monta no meio da praça uma **cadeira grande e vazia no alto
de cinco degraus**, coberta de panos, flores e lâmpadas. "É para lembrar de quem ensinou os hinos antigos", diz a Vó Edna,
"e de todo mundo que já não está aqui para cantar com a gente." Ninguém senta nela. Todo mundo canta em volta, traz
**oferendas de luz** (lâmpadas, frutas claras, flores) e dança. Os primos sabem, sem ninguém dizer, que é também um pouco
para o Vô Jorge, e a cadeira de balanço dele vai para a praça nesse dia.

Fonte: festa do *Bēma* (grego "tribuna"), a maior festa maniqueia, na primavera (mês de Adar), com o trono vazio sobre
cinco degraus e salmos próprios (*Salmos do Bema*, Saltério copta de Medinet Madi; hinos partas de Turfan). No jogo **não
se nomeia a pessoa homenageada.**

| Antigo | Novo |
|---|---|
| Festa Junina / "Arraiá da Vó Edna" | **Bema** / "**O Bema da Vó Edna**" (en: *the Bema Festival*, *Grandma Edna's Bema*) |
| Botão "Arraiá" | **"Bema"** |
| Janela: junho inteiro (`SeasonCalendar`) | **Decidido: fica junho** (é perto do solstício, "o mês de mais luz do ano", e não mexe no código). |
| Decoração: 8 postes com bandeirinhas + fogueira | **8 postes com cordões de lanternas de papel e fitas nas cinco cores dos elementos** (lilás, branco-azulado, dourado, verde-água, laranja) + **o Bema**: estrado de **cinco degraus** com a cadeira vazia, panos bordados, flores e lâmpadas (no lugar da fogueira, mesmo ponto `FireAngle`) |
| Quadrilha (ritmo, 16 passos) | **Hinos do Bema** / *Bema Hymns*: o **Mestre Xroshtag** chama e o jogador **responde** (Chamado e Resposta). Tambor falante e lira acompanham. |
| "Anarriê!" (←) | **"Luz!"** |
| "Balancê!" (↑) | **"Vento!"** |
| "Olha a cobra!" (↓) | **"Água!"** |
| "Caminho da roça!" (→) | **"Fogo!"** |
| (o 5º elemento) | o **Ar Sereno** é o "passo perfeito": cada **perfeito** solta uma estrelinha lilás. É só visual, sem botão novo. |
| Dança tranquila | **Hino tranquilo** |
| Prendas (moeda) | **Oferendas** / *Offerings* (ícone: lamparininha) |
| Barraca de Prendas | **Tenda das Oferendas** |
| Pescaria do arraiá | **Pescaria do Bema** |
| Lendário "Bandeira do primeiro São João do sítio" (`casa_familia_025`) | **"Estandarte do primeiro Bema da Vila"** |
| Mounts do evento (Ema de Fogo 90, Boi-Bumbá 250) | Avestruz de Brasa 90, Boi das Canções 250 (§3.17) |
| Falas dos primos (`FestaJuninaTexts`) | lembram a **Vó Edna puxando o primeiro hino** e o **Vô Jorge que acendia a primeira lâmpada do Bema** |
| Instrumentos (sabor) | **tambor falante** (*dùndún*, iorubá; o instrumento, não um rito), **lira de tigela** da Núbia (*kissar*), **dutar** uigur, **flauta ney** |

Fonte dos instrumentos: dùndún (tambor de axila iorubá que "fala" imitando os tons da língua); kissar/tanbur (lira núbia);
dutar (alaúde de duas cordas da Ásia Central).

### 3.16 Viagem rápida: **as Barcas de Luz** (substitui o carro de boi)

**A imagem:** sobre as estradas do mapa corre, para quem sabe ver, um **rio de luz**. Nele navegam duas barcas
compridas de proa alta e uma vela triangular: a **Barca do Sol** de dia, dourada, e a **Barca da Lua** à noite,
prateada. Elas levam as faíscas libertadas para a Coluna da Glória e **dão carona** a quem respondeu ao Chamado.

| Antigo | Novo |
|---|---|
| Carro de boi (`CityFastTravelPanel`, `FastTravelRules`) | **Barcas de Luz** / *Light Ships* |
| Botão/painel "Carro de boi" | **"Cais da Barca"** / *Ship Dock* |
| Qual barca | **Barca do Sol** nas fases Dia/Amanhecer, **Barca da Lua** no Entardecer/Noite (`TimeOfDayManager`), só no texto e na cor do fade. Mesmas regras. |
| Custo "2 ouro por estrada" | **"passagem: 2 ouro por estrada"** (o remador cobra "pela vela") |
| Para só onde o chefe caiu | igual. Texto: "A barca só atraca onde a luz já foi libertada." (ótimo motivo no mito) |
| Bloqueada durante eventos | igual. Texto: "A barca não sai com encrenca na estrada." |
| Transição "O carro de boi vai rangendo pela estrada até …" | **"A Barca do Sol desliza pelo rio de luz até …"** / "A Barca da Lua desliza em silêncio até …" |
| Visual (fade) | tela escurece → **um fio de luz atravessa** (dourado ou prata) → clareia. Com Reduzir Movimento: corte seco, como hoje. |
| Cais em cada cidade (prop do kit) | píer curto de madeira **sem água**, sobre um sulco de luz no chão, com um poste de lanterna. A barca só aparece na transição. |
| Desenho da barca (kit) | casco comprido com proa e popa curvas para cima (inspiração: as barcas solares de Kemet, **Mandjet** da manhã e **Mesektet** da noite); vela latina triangular de *dhow* suaíli; na Barca da Lua, a serpente **Mehen** enrolada na borda como guarda-corpo. |
| Remadores | silhuetas de luz sem rosto (NPC não falante); o "capitão" de cada cais pode ser o NPC da cidade que já existe |
| Falas | Lavine: "A Barca da Lua é o sapato do Kaggen. Eu li." Ícaro: "Propulsão por luz. Eu PRECISO ver o motor." |

Fontes: navios do Sol e da Lua (Kephalaia; Acta Archelai 8); barcas Mandjet/Mesektet e Mehen (Livro do Amduat,
textos de Kemet); vela de dhow (costa suaíli); Saulė, a deusa-sol báltica que à noite navega um barco de ouro (*dainas*
lituanas), como eco; a lua-sapato (\|xam San).

### 3.17 Montarias (as 30 de `Docs/Mounts.md`)

Ids, números e habilidades iguais; só mudam nome e nome da habilidade. "Apito de …" vira **"Apito de …"** (fica).
Lojas: Haras da Vila → **Estrebaria do Srosh**; Feira de Bichos do Oásis → **Caravançará de Qocho**; Curral do Pico →
**Curral de Litema**; Toca do Tratador → **Toca das Lamparinas**.

| `id` | Antigo | Novo (pt / en) | Habilidade (novo nome) | Fonte |
|---|---|---|---|---|
| `mount_pony` | Pônei Pé-de-Pano | **Pônei das Estepes** / *Steppe Pony* | Arrancada | pôneis das estepes da Ásia Central. |
| `mount_big_dog` | Cachorrão Paçoca | **Alabai Fofão** / *Fluffy Alabai* | Faro de Tesouro | alabai, o pastor-da-ásia-central. |
| `mount_capybara` | Capivara Gigante | **Hipopotinha de Taweret** / *Taweret's Pygmy Hippo* | Banho de Lama | hipopótamo-pigmeu (África Ocidental); Taweret, a hipopótama protetora das crianças em Kemet. |
| `mount_mountain_goat` | Bode Saltador | **Íbex-da-Walia** / *Walia Ibex* | Pulo Duplo | íbex-da-walia, das montanhas Simien (Etiópia). |
| `mount_pack_donkey` | Jegue Carregador | **Asno de Caravana** / *Caravan Donkey* | Zurro Espanta-Bicho | asnos de carga da Rota da Seda. |
| `mount_rhea` | Ema Corredora | **Avestruz Corredora** / *Running Ostrich* | Fôlego de Avestruz | avestruz (África; penas em túmulos kushitas). |
| `mount_pinto_horse` | Cavalo Pampa do Vaqueiro | **Cavalo Celeste de Fergana** / *Heavenly Horse of Ferghana* | Investida da Estepe | os "cavalos celestes" de Fergana pedidos pela China Han (Shiji 123). |
| `mount_enchanted_donkey` | Jumento Encantado do Sertão | **Asno de Três Patas** / *Three-Legged Ass* | Firmeza das Três Patas | o asno de três patas que purifica as águas (Bundahishn, persa). |
| `mount_giant_armadillo` | Tatu-Bola Gigante | **Pangolim Gigante** / *Giant Pangolin* | Cavar Túnel | pangolim-gigante (África), que se enrola em bola. |
| `mount_maned_wolf` | Lobo-Guará Encantado | **Chacal de Upuaut** / *Wepwawet's Jackal* | Abre-Caminhos (visão noturna) | Wepwawet/Upuaut, "o que abre os caminhos" (Kemet). |
| `mount_caipora_peccary` | Caititu da Caipora | **Facóquero Trilheiro** / *Trailblazer Warthog* | Trilha de Folhas | facóquero (savana africana). |
| `mount_river_seahorse` | Cavalo-Marinho Encantado do Rio | **Cavalo-d'Água do Lago Van** / *Lake Van Water Horse* | Andar nas Águas | cavalos-d'água (*tsovi dzi*) que saem do lago Van na lenda armênia. |
| `mount_giant_anteater` | Tamanduá-Bandeira Gigante | **Porco-Formigueiro Gigante** / *Giant Aardvark* | Língua Comprida | aardvark (África). |
| `mount_golden_scarab` | Escaravelho Dourado | **Escaravelho de Khepri** / *Khepri's Scarab* | Escudo de Areia | Khepri, o escaravelho que rola o sol nascente (Kemet). |
| `mount_snow_tiger` | Tigre-das-Neves | **Irbis, o Leopardo-das-Neves** / *Irbis the Snow Leopard* | Rugido do Irbis | *irbis*, o leopardo-das-neves do Tian Shan (símbolo quirguiz e cazaque). |
| `mount_woolly_mammoth` | Mamute Lanoso | **Mamute de Komi** / *Komi Mammoth* | Pisão do Mamute | no folclore komi e mansi, o mamute é um bicho que vive debaixo da terra. |
| `mount_unicorn` | Unicórnio da Mata | **Shadhavar Cantante** / *Singing Shadhavar* | Aura Curativa ("o chifre toca e cura") | shadhavar, o bicho de chifre oco que toca música ao vento (bestiários persas, al-Qazwini). |
| `mount_hippogriff` | Hipogrifo Ventania | **Senmurv Ventania** / *Gale Senmurv* | Planar | senmurv, cão-ave alado da arte sogdiana e sassânida. |
| `mount_griffin` | Grifo das Ruínas | **Grifo de Ouro Cita** / *Scythian Gold Griffin* | Voo Curto | grifos dos ouros citas (kurgans de Pazyryk). |
| `mount_light_jaguar` | Onça-Pintada de Luz | **Leão de Luz de Apedemak** / *Apedemak's Light Lion* | Salto do Leão | Apedemak, o deus-leão guerreiro de Meroé (Kush). A montaria é "o leão dos relevos", não o deus. |
| `mount_carbuncle` | Carbúnculo das Grutas | **Raposa-Joia do Limiar** / *Threshold Jewel Fox* | Joia Acesa | bicho de joia na testa dos bestiários persas e árabes (*mehrmohre*, a pedra-da-serpente) **[verificar]**. |
| `mount_fire_rhea` | Ema de Fogo | **Avestruz de Brasa** / *Ember Ostrich* | Corrida de Brasa | par da Avestruz Corredora (evento Bema, 90 oferendas). |
| `mount_pegasus` | Pégaso Nuvem-Branca | **Tulpar Nuvem-Branca** / *Cloud-White Tulpar* | Voo Livre | tulpar, o cavalo alado das lendas cazaques, quirguizes e tártaras. |
| `mount_sleipnir` | Sleipnir, o Corcel de Oito Patas | **mantém** (decidido: é nórdico, não é regional brasileiro) | Galope Entre Mundos | Edda (nórdico). |
| `mount_kirin` | Kirin do Orvalho | **Qilin do Orvalho** / *Dew Qilin* | Passo Sem Pegada | qilin (China; o mundo da Mingjiao). |
| `mount_boitata` | Boitatá, Guardião da Mata | **Mehen, a Serpente Guardiã** / *Mehen the Guardian Coil* | Anel de Fogo Protetor | Mehen, a serpente que se enrola em volta do sol para protegê-lo à noite (Kemet). |
| `mount_boi_bumba` | Boi-Bumbá Estrelado | **Boi das Canções** / *Song Ox* | Canção do Boi | a tradição dos dinka e dos nuer (Sudão do Sul) de compor canções para o boi de cada um (o povo não vira nome de bicho). Evento Bema, 250. |
| `mount_sertao_phoenix` | Fênix do Sertão | **Bennu, a Ave do Recomeço** / *Bennu, Bird of Renewal* | Renascer das Cinzas → **Renascer da Luz** | Bennu, a garça-fênix de Heliópolis (Kemet). |
| `mount_dragon_hatchling` | Dragão Filhote Brasinha | **Vishap Filhote Brasinha** / *Little Ember Vishap* | Sopro Morno | vishap, o dragão armênio das pedras *vishapakar*. "Brasinha" fica. |
| `mount_elder_tortoise` | Jabuti Ancião | **Ijapa, a Tartaruga Anciã** / *Ijapa the Elder Tortoise* | Casco de Pedra | Ìjàpá, a tartaruga esperta dos contos iorubás (personagem de fábula, não divindade). |

Tipos continuam: 21 terrestres, 5 voadores, 2 aquáticos, 2 escavadores.

### 3.18 Relíquias ("Relíquias do Sertão" → **Lembranças do Vô Jorge**)

| `id` | Antigo | Novo | Fonte/nota |
|---|---|---|---|
| `relic_gaita_vo_chico` | Gaita do Vô Chico | **Flauta Ney do Vô Jorge** | ney, flauta de cana persa/sogdiana. Ele tocava desafinado na varanda. |
| `relic_cartas_vo_chico` | Cartas do Vô Chico para a Vó Zefa | **Cartas do Vô Jorge para a Vó Edna** | eco das **cartas de Kellis**: cartas carinhosas de famílias maniqueias do oásis de Dakhla (Egito, séc. IV). |
| `relic_chapeu_couro` | Chapéu de couro do Vô Chico | **Gorro de Feltro do Vô Jorge** | gorro de feltro de caravaneiro (sogdiano). |
| `relic_chocalho_teimosa` | Chocalho da mula Teimosa | **Sino da camela Teimosa** | a Teimosa vira **camela bactriana** (sino de caravana). |
| `relic_canivete_vo_chico` | Canivete do Vô Chico | **Canivete do Vô Jorge** | neutro. |
| `relic_piao_irmaos` | Pião de Maria e Joaquim | mantém | neutro. |
| `relic_retrato_primos` | Retrato dos cinco primos | mantém | neutro. |
| `relic_arco_maithe` | Primeiro arco da Maithe (goiabeira) | mantém o nome; **de galho de damasqueiro** | — |
| `relic_escudinho_joaquim` | Escudinho de tampa de barril | mantém | — |
| `relic_engrenagem_icaro` | Primeira engrenagem (descascador de cajá) | mantém; **descascador de romã** | — |
| `relic_medalha_maria` | Medalhinha de sol da Maria | mantém | — |
| `relic_fita_lavine` | Fita vermelha da Lavine | mantém | — |
| (casa) Vitrine das doze relíquias | **Vitrine das Lembranças** | — |

Segredo extra opcional: **Ouros do Céu** (§1.6), só a Lavine pega.

### 3.19 Graduações de classe

| Primo | G1 (antes → depois) | G2 | G3 |
|---|---|---|---|
| Maithe | Rastreadora do Carcará → **Rastreadora do Falcão** (falcão-sacre das estepes) | Flecheira da Ventania (mantém) | Olho do Sertão → **Olho de Liyongo** (Fumo Liyongo, herói-arqueiro e poeta da costa suaíli) |
| Maria | Rezadeira do Sol → **Curandeira do Sol** | Benzedeira da Aurora → **Guardiã da Aurora** | Candeia do Sertão → **Candeia da Glória** (Coluna da Glória) |
| Joaquim | Guardião das Raízes (mantém) | Guardião da Caatinga → **Guardião do Tugai** | Mestre das Raízes (mantém) |
| Ícaro | Engenhoqueiro → **Artífice** (os Artífices, §1.6) | Mestre das Máquinas (mantém) | Cientista do Sertão → **Herdeiro de Kurdalægon** |
| Lavine | Feiticeira da Noite Rubra (mantém) | Guardiã da Sombra Macia (mantém) | Lua do Sertão → **Barqueira da Lua** |

Missões: "O Arco de Goiabeira" → **"O Arco de Damasqueiro"**. As outras ficam. Buffs: Olhar do Carcará → **Olhar do
Falcão**; Bênção do Terreiro → **Calor da Lâmpada**; Raiz que Segura, Engenhoca de Reforço e Lua Guardiã ficam
("Engenhoca" → **"Engenho de Reforço"**). Acessórios visuais: "chapéu, lenço" → "gorro de feltro, lenço bordado".

**Ícaro, skills:** Torreta Automática → **Torreta Ushabti** ("ela responde!"); Granada Elétrica fica; Drone de Ataque →
**Drone Senmurv**. Os nomes das outras skills só mudam se tiverem palavra regional (ver §4).

### 3.20 Casa: categorias e conjuntos (580 itens) e roupas (234)

**Regras para renomear item por item:**
1. **id, raridade, preço, conforto, encaixe e origem ficam.** Só `Nome` (pt) e a coluna en do TSV mudam. Os nomes
   continuam únicos (a checagem automática de 580/234 distintos precisa passar).
2. **Material e forma valem mais que o tema.** "Banco de madeira comprido" fica. Só troca o que é marca regional:
   palhinha → vime; chita → **ikat/suzani**; rede de varanda → **estrado de iwan (tapchan)** ou rede de seda; ágata (louça
   esmaltada) → **cobre batido/cerâmica turquesa**; barro (fica); fogão a lenha → **tandir**; moringa → **jarra de
   barro**; lamparina (fica); cangalha/arreio de jegue → **arreio de camelo**; chapéu de couro → **gorro de feltro**;
   xilogravura → **iluminura**; cordel → **rolo de hinos iluminado**; santo/oratório → **nicho de lâmpada**; São
   João/bandeirinha → Bema/lanterna; mandacaru → **saxaul** ou **aloé**; caju/cajá/umbu → damasco/romã/amora; carro de
   boi → **barquinha de luz** de enfeite.
3. **Nomes de NPC em "Origem"** seguem §3.1 ("Loja: Marcenaria do Mestre Damião" → "Loja: Marcenaria do Mestre Omóforo").
   O importador lê a origem só como texto. Se a origem for chave de loja, trocar só a exibição.
4. **Drops de chefe** seguem §3.3 ("Troféu do Lobo Alfa" → "Troféu de Kök-Böri").
5. **Nunca** nome de divindade de fé viva em item. Kemet, sogdiano, nartes e etrusco podem (são mitos históricos).
6. **Sazonais:** Arraiá → Bema; Natal Sertanejo → **Noite de Yalda**; Carnaval → **Festa das Lanternas**.

| Conjunto (antigo) | Novo | Tema | Exemplos (antigo → novo) |
|---|---|---|---|
| Varanda da Vó Zefa | **Iwan da Vó Edna** | o alpendre de arco, tapetes, almofadas | Sofá de palhinha da varanda → Sofá de vime do iwan; Rede de varanda bordada → Estrado de iwan bordado; Cadeira de balanço da Vó Zefa → **Cadeira de balanço do Vô Jorge** (é a cadeira da história) |
| Tropeiro do Vô Chico | **Caravana do Vô Jorge** | feltro, couro, sino de camelo | Poltrona de couro curtido (fica); Cangalha decorativa → Sela de camelo decorativa |
| Casa de Farinha | **Moinho do Pomar** | pilão, mó, peneiras | Mó de casa de farinha → Mó de pedra do moinho; Tacho de cobre → Caldeirão de cobre |
| Cozinha da Vó | **Cozinha da Vó** (fica) | tandir, cobre, barro | Fogão a lenha → Tandir de barro; Panela de ágata → Panela de cobre batido |
| Oficina do Ícaro | mantém | + peças de engenho | Rádio de válvula (fica); "Cabeça de ushabti consertada" (novo nome para um Raro já existente) |
| Ventania da Maithe | mantém | arco, pipa, rosa-dos-ventos | pipa → pipa de papel de seda |
| Luz da Maria | mantém | branco, dourado, ervas | sabonete de arruda → sabonete de hortelã |
| Raízes do Joaquim | mantém | madeira, raiz, escudo | ipê → choupo |
| Noite Rubra da Lavine | mantém | estrelas, lua, sombra macia | — |
| Cantinho do Caramelo | **Cantinho do Ari** | tudo do cachorro da família | Casinha do Caramelo → Casinha do Ari; roupinhas "do Caramelo" (guarda-roupa, encaixe *Caramelo*) → "do Ari" (o nome do encaixe no código fica) |
| Mata Fechada | **Tugai do Zarafshan** | Floresta | Toco de jatobá → Toco de choupo-turanga |
| Duna Dourada | **Areias Flamejantes** | Deserto | Vaso de mandacaru → Vaso de saxaul; + "Mini-pirâmide de Meroé" |
| Pico Nevado | **Picos de Litema** | Montanha | Tapete de lã de ovelha → Cobertor listrado da serra; + "Pedra pintada do eland" |
| Cristal das Cavernas | **Tufo Rosa das Lamparinas** | Cavernas | Estalactite de cristal (fica); + "Vishapakar de jardim" |
| Feira Livre | **Bazar da Nana** | ikat, réstias, balança | Toalha de chita → Toalha de ikat |
| Cordel e Xilogravura | **Iluminuras e Papel Recortado** | arte de papel | Folheto de cordel → Folha de hinos iluminada (os livros maniqueus de Turfan eram famosos pelo ouro e pelas cores); Xilogravura → Papel recortado de lanterna |
| Horta Caprichada | **Horta dos Melões** | canteiros, abelhas | — |
| Cristal do Sol | mantém | a luz que voltou | — |
| Arraiá | **Bema** | o festival | Bandeirinhas de São João → Cordão de lanternas de cinco cores; Fogueira junina → Bema de cinco degraus (miniatura) |
| Natal Sertanejo | **Noite de Yalda** | a noite mais longa do ano, quando "a luz começa a voltar" | Presépio → Mesa de Yalda (romãs, melancia, velas); Lapinha → Lanterna de Yalda |
| Frevo e Maracatu | **Festa das Lanternas** | lanternas de papel de seda chinesas | Sombrinha de frevo → Lanterna-peixe; Coroa de maracatu → Coroa de papel dourado |

Fontes dos sazonais: Yalda/Shab-e Chelleh (solstício de inverno persa, costume familiar que não é rito de fé); Festa das
Lanternas (Yuanxiao, China). Mwaka Kogwa (Ano-Novo xirazi-suaíli de Zanzibar) fica como alternativa africana para uma
temporada futura.

**Roupas (234), regras:**
- Os conjuntos de nível (Batedora da Mata, Rastreadora do Riacho, Pena de Gavião, Olho do Vento, Tempestade de Mil Flechas,
  Ajudante do Templo, Luz de Candeia, Templo Solar, Milagre Celestial, Muralha de Pedra, Última Resistência, Aprendiz de
  Oficina, Rádio Desmontado, Engrenagem de Latão, Campo Magnético, Exército Mecânico, Caçulinha Curiosa, Contadora de
  Estrelas, Sombra Macia, Noite Rubra, Eclipse Eterno) **ficam**: não são regionais.
- Trocar: Rezadeira Aprendiz → **Curandeira Aprendiz**; Guri da Roça → **Menino do Pomar**; Vaqueirinho → **Cavaleirinho da
  Estepe**; Casca de Ipê → **Casca de Choupo**.
- Festivos (só visual): Rainha do Arraiá da Mata → **Cantora do Bema**; Pastorinha do Cordão Azul → **Portadora da
  Lâmpada**; Caboclo de Lança Mirim → **Dançarino das Fitas**; Inventor do Arraiá → **Inventor do Bema**; Estrelinha do
  Reisado → **Estrelinha das Lanternas**. Animação "passinho de quadrilha, frevo, reisado" → "**roda do hino, dança das
  fitas, palmas de lanterna**".
- Peças: chapéu de palha (fica) ou **gorro de feltro**; gibão de algodão cru → **chapan** (casaco acolchoado da Ásia
  Central) de algodão cru; alpargatas (ficam); calça de brim (fica); botina (fica); lenço (fica, "bordado de suzani");
  "chapéu de couro de vaqueiro" → **gorro de feltro de caravaneiro**; **cobertor basotho** e **mokorotlo** só como
  peças da Serra (Raro, com nota de respeito).
- **Tinturas:** nomes de cor regionais (urucum, jenipapo, caju) → **açafrão, índigo, romã, damasco, turquesa, ocre de
  Litema, tufo rosa**.

### 3.21 Moedas e tiers de loja

| Antigo | Novo | Fonte |
|---|---|---|
| Ouro (`item_gold`) | **Ouro** (fica) | — |
| Moeda de ouro (`item_gold_coin`, drop do Deserto) | **Moeda Furada de Qocho** / *Qocho Holed Coin* | moedas de bronze com furo quadrado, que circulavam em Turfan/Qocho. |
| Prendas | **Oferendas** | §3.15 |
| Pontos de Caça, Pontos do Caderno | ficam | — |
| Freguesia: Freguês novo → Freguês → Freguês de casa → Compadre | **Visitante → Freguês → Amigo da Casa → Companheiro de Caravana** | caravanas da Rota da Seda. |
| Mascate | **Caravaneiro** | §3.1 |

### 3.22 Outros vocabulários com tema

| Antigo | Novo |
|---|---|
| sítio da Vó | **casa da Vó** / **o pomar da Vó** |
| porteira | **portão do pomar** (nas cidades: **portal**; os "mourões" do mapa viram dois **pilares de adobe**) |
| Caderno da Vó Zefa | **Caderno da Vó Edna** |
| radinhos de pilha (Ícaro) | ficam (são invenção do Ícaro) |
| Cristal do Sol | fica |
| Eventos de estrada: Mascate ambulante; Chuva de caju; Romeiros; Cantoria ao pé da fogueira; Tempestade de areia; Neblina na serra; Eco na caverna | **Caravana ambulante; Chuva de damascos** (o vento sacode o pomar); **Cantores na estrada; Histórias de Ananse na fogueira** (Aso); Tempestade de areia (fica); **Neblina do Vento Ruim**; Eco na caverna (fica) |
| Pesca: Açude / Riacho / Lago Subterrâneo | **Remanso de Faro** (Faro, espírito da água dos bambara) / **Riacho do Tugai** / **Lago do Limiar** |
| Peixes (tilápia, piaba, traíra, pacu, curimatã, tucunaré, peixe-dourado, lambari, piau, mandi, cascudo, surubim, bagre-cego, piaba-lanterna) | trocar os sul-americanos por peixes reais do Velho Mundo: tilápia (fica; é do Nilo), **carpa-dourada, lúcio, perca-do-Nilo, peixe-gato-do-Nilo, barbo, esturjão-miúdo, truta-do-Tian-Shan, peixe-tigre, enguia**; bagre-cego e **peixe-lanterna** ficam. O Lendário **Peixe-Dourado** fica. |
| Casa: "lenha" | fica |
| Tarefas da Vó ("a Vó Zefa chamou para o café") | "a Vó Edna chamou para o chá" |

---

## 4. Guia de reescrita (para os agentes de texto)

### 4.1 Vocabulário para usar

luz, faísca, apagado/acender, Chamado, responder, Coluna da Glória, Barca do Sol/da Lua, a Mistura (pouco), lâmpada,
lamparina, pomar, iwan (com explicação na 1ª vez: "o alpendre da Vó"), tandir ("o forno da Vó"), caravana, caravançará,
bazar, feltro, seda, damasco, romã, melão, tâmara, chá, os Artífices, ushabti ("o que responde"), arconte (só para chefes
de evento), Āz, a Treva, o Príncipe das Trevas (raramente, com peso), Ar Sereno/Vento Vivo/Luz/Água Viva/Fogo Vivo.

### 4.2 Vocabulário para evitar (procurar e trocar)

sertão, sertanejo, nordestino, caatinga, sítio (no sentido de fazenda), porteira, roçado, tropeiro, tropa, vaqueiro,
gibão, cangaço/cangaceiro, mascate, romaria/romeiro, rezadeira, benzedeira, benzer, bendito, novena, capelinha, santo,
"Deus o tenha", "Vai com Deus", "se Deus quiser", "Ô de casa!", "oxente/vixe/arretado", "tu tem/tu vai" (concordância
nordestina da Vó), cordel, xilogravura, repente, viola, sanfona, forró, quadrilha, arraiá, São João, bandeirinha, prenda,
fogueira junina, carro de boi, engenho, moenda, casa de farinha, rapadura como comida (o título **Rapadura** fica; o doce vira Torrão Dourado, e "rapadura" só aparece no título e nos créditos), cuscuz, pamonha,
baião, queijo coalho, cocada, paçoca, café coado, fogão a lenha, mandacaru, umbu, caju, cajá, jatobá, juazeiro, ipê,
carcará, asa-branca, lobisomem, caipora, curupira, saci, boitatá, mula-sem-cabeça, mãe-da-lua, rasga-mortalha,
boi-bumbá, frevo, maracatu, reisado, pastoril, cacimba, açude, lajedo, patuá, compadre/comadre, Seu/Dona como título.

Também evitar: "religião", "fé", "pecado", "salvação", "profeta", "igreja/templo de culto", "rezar", "orar", "amém".
O **Templo Solar** da Maria fica (decidido), porque é um lugar de luz e de cura, não de culto. Ao descrever, dizer "sala de luz".
O nome **Caramelo** sai dos textos e vira **Ari** (a cor "caramelo" pode continuar na descrição do pelo).

### 4.3 Como as falas soam agora

- **pt-BR coloquial, caloroso e neutro**, sem sotaque regional marcado. A família fala como uma família brasileira de
  hoje (eles **são** os primos de sempre). **Quem muda é o mundo**, não o jeito deles.
- **NPCs da ilha** têm um sabor leve de conto: frases um pouco mais antigas ("Bons ventos te tragam", "Que a tua
  lâmpada não apague", "Senta, que a história é comprida"), **sem** parecer oração. Uma por fala, no máximo.
- **Saudações:** "Ô de casa!" → "Tem alguém em casa?" / "Licença!". "Vai com Deus" → "Boa estrada" / "Volta com
  luz". "Deus o tenha" (sobre o Vô) → "o Vô Jorge ia adorar isso".
- **O mito se conta, não se ensina.** Sempre pela boca do Vô Jorge lembrado ("o Vô dizia que...") ou de um NPC velho.
  Nunca em tom de lição: "Diz o conto que a Lua junta a luz..." está bom. "Saiba que a alma deve se libertar da matéria"
  está proibido.
- **Medo na medida:** a Treva é "fria", "pegajosa", "barulhenta". Bichos apagados ficam "tristes", "bravos",
  "esquecidos". Sem morte, sangue ou luto pesado. "Cair" e "levantar" no lugar de "morrer".
- **en:** mesmos nomes próprios (Kök-Böri, Uasamonga, Bema). Explicar no 1º encontro com um aposto curto ("the Bema, the
  festival of the empty seat").

**Exemplo** (Vó Edna na cozinha, antes e depois):
- antes: "Tu tem cara de quem quer cuscuz, minha caçula. Senta que o café tá coado."
- depois: "Essa carinha é de quem quer samsa, minha caçula. Senta, que o chá tá quentinho."

### 4.4 Dinâmicas de família (não mexer)

- Irmãos: **Maithe ↔ Ícaro**, **Maria ↔ Joaquim**. **Lavine é prima de todos.** Ordem: Maithe > Maria > Joaquim > Ícaro >
  Lavine. A piada dos "três meses e onze dias" entre Maithe e Maria fica.
- Tratamento (`Docs/Narrative.md` §2): "mana", "maninho(a)", "pestinha", "prima/primo", "Vó"/"Vó Edna",
  "Vô Jorge". A Lavine odeia "pequena".
- Tiques: Maithe conta os primos; Ícaro "Viu? Ciência"; Joaquim "Deixa comigo!" e repete o prato; Maria "Respira" e
  sonha; Lavine é seca e honesta.
- Ajustes de fala para o tema novo: a Maria troca "Vai com Deus" por **"Vai com luz"**; o Joaquim ama o **plov**; o Ícaro
  fica louco com os **engenhos** e os **ushabtis**; a Maithe confia no **vento** (o elemento dela); a Lavine se dá bem com
  a **Barca da Lua** e o Kaggen.
- Os elementos são **piada e orgulho** entre eles ("o meu é vento, o teu é água, empata"), nunca rivalidade séria.

---

## 5. Ordem de implementação (onda de renomeação do código)

**Regra de ferro: ids não mudam.** Ficam iguais: chaves de localização, `id`s de asset/item/inimigo/montaria/casa/roupa/
relíquia, nomes de classe e de arquivo C# (`FestaJuninaPanel`, `TachoRules`, `RomariaBlessing`, `CarameloTexts`...),
enums, chaves e campos de save, nomes de menu de debug que os testes chamam. Só mudam **os valores en/pt-BR** e os
documentos. Se um `const` de id parecer "feio" (`tonico_haras`, `item_patua_vo_zefa`), ele fica.

Cada onda é dividida por **pasta** para os agentes não editarem o mesmo arquivo. Um arquivo tem **um dono** por onda.

| Onda | Agente | Pastas e arquivos (dono exclusivo) | O quê |
|---|---|---|---|
| 0 | lore (docs) | `Docs/Narrative.md` (reescrever a partir desta bíblia), `Docs/GDD.md` (família e mundo) | fonte de verdade antes do código |
| 1A | história | `Gameplay/Story/*` (StoryTexts, CutsceneTexts), `Gameplay/World/Chatter/*`, `UI/Menus/CreditsContent.cs` | prólogo, interlúdios, epílogo, cutscenes, 46 conversas de estrada, créditos |
| 1B | missões e diálogos | `Gameplay/Quests/*`, `Gameplay/Dialogue/*`, `Resources/Dialogue/*.asset`, `Resources/Quests/*.asset`, `Editor/*DialogueSeeder*.cs`, **`Core/Localization/LocalizationManager.cs`** (só este agente) | NPCs de região, missões pessoais, Convocação, diárias |
| 1C | cidades e viagem | `Gameplay/World/Cities/*`, `UI/WorldMap/CityTexts.cs`, `CityShopTexts.cs`, `VillageHubTexts.cs`, `CityFastTravelPanel.cs`, `FastTravelTransition` | nomes de cidade, moradores, lojas, tiers de freguesia, **Barcas de Luz**, alforje |
| 1D | Bema | `Gameplay/Seasons/*` (FestaJuninaTexts, Decor, Panel, QuadrilhaSession: só texto e cores) | §3.15 |
| 1E | bichos | `UI/Bestiary/*`, `Gameplay/Night/*`, `Gameplay/WorldEvents/*`, `UI/Encounter/BossTexts.cs`, `Resources/Enemies/*.asset` (só `displayName`), `Docs/Balance/enemies.csv` (só a coluna `name`) | inimigos, noite, chefes, chefes do mundo, Āz, Mahaway |
| 1F | montarias | `Gameplay/Mounts/*` (MountTexts, MountWaveTexts, MountCatalog: só nomes) + `Docs/Mounts.md` | §3.17 |
| 1G | itens e sistemas | `Gameplay/Refining/*`, `Gameplay/Tacho/*`, `Gameplay/Inventory/DeathProtectionTexts.cs`, `RomariaBlessing.cs` (só texto), `Resources/Items/*.asset` (só nome e descrição) | pedras, Uasamonga, asas, frutas, Tumar, Lâmpada Guardiã |
| 1H | casa da Vó | `UI/WorldMap/KitchenTexts.cs`, `Gameplay/Pets/CarameloTexts.cs`, `Gameplay/Secrets/SecretTexts.cs`, `UI/WorldMap/VillageChestTexts.cs`, `NewGamePlusTexts.cs` | forno, Ari (ex-Caramelo), relíquias, baú, NG+ |
| 1I | classes e heróis | `Gameplay/ClassEvolution/*`, `Gameplay/Talents/*`, `Gameplay/Content/ContentTexts.cs`, `Gameplay/Characters/*` (só lore e nomes de skill), `UI/CharacterSelect/HeroFamily.cs` | §3.19 |
| 1J | salas e diversos | `Gameplay/EventRooms/*`, `Gameplay/Hunting/*`, `Gameplay/Fishing/*`, `Gameplay/Daily/*`, `Gameplay/Duo/*` | §3.13, Patig, pesca |
| 1K | UI geral | `UI/Achievements/*`, `UI/Tutorial/*`, `UI/Common/UiTexts.cs`, `UI/Housing/*Texts.cs`, `UI/WorldMap/WorldMapTexts.cs`, `WorldEventTexts.cs` | conquistas, dicas, mundo "Ilha da Aurora" |
| 2A | casa (dados) | `Docs/Housing/CatalogoCasa.md` + `.en.tsv` → importar `HouseCatalogData.g.cs` (menu do Unity) | 580 nomes (§3.20) |
| 2B | roupas (dados) | `Docs/Housing/CatalogoRoupas.md` + `.en.tsv` → importar `WardrobeCatalogData.g.cs` | 234 nomes |
| 2C | visual | `Gameplay/Housing/Visuals/*`, `FestaJuninaDecor`, `Gameplay/World/Map/Dressing` (porteira → pilares de adobe) | só se a malha tiver "cara" de sertão |
| 3 | testes e varredura | `Assets/Tests/EditMode/*` com texto literal (já vistos: `BestiaryTests`, `CarameloCompanionTests`, `CityShopTests`, `CityTests`, `CookingTests`, `CreditsRollTests`, `DailyStaminaTests`, `DeathPenaltyTests`, `HouseCatalogTests`, `BagGridTests`, ...) | os testes devem comparar **chaves**, não texto. Onde comparam texto, atualizar o texto esperado. Rodar `LocalizationCoverageTests` e a checagem de nomes únicos. |
| 3 | varredura final | todo o repositório | grep da lista de §4.2 (pt e en: "sertão", "Zefa", "Chico", "Cangaço", "Junina", "Caramelo" como nome...) e zero resultados visíveis |
| 4 | docs restantes | `Docs/Cities.md`, `DayNight.md`, `Seasons.md`, `WorldEvents.md`, `Housing.md`, `ClassEvolution.md`, `Referencias_MMO.md`, `Balance/README.md` (só nomes, nunca números) | — |

Notas:
- **Caramelo → Ari** (nome visível, en e pt) entra na 1H (Pets), 1A (história/cutscenes), 1B (diálogos), 2A/2B (casa e roupinhas) e 1K (conquistas).
- **Vó Zefa → Vó Edna e Vô Chico → Vô Jorge** ainda aparecem em muitos textos e no `Narrative.md`. Entram na 1A/1B/1H,
  cada agente na sua pasta.
- `HouseCatalogData.g.cs` e `WardrobeCatalogData.g.cs` são **gerados**: editar o `.md`/`.tsv` e reimportar, nunca à mão.
- Áudio (`SfxEventMap`): os ids de som não mudam. Se um nome de som tiver "junina" ou "carro de boi", só muda a descrição.
- Compilar offline (Roslyn) depois de cada agente. Uma onda só fecha com o EditMode verde.

---

## 6. Decisões do usuário (2026-10-30)

Todas as perguntas abertas foram respondidas. As tabelas acima já estão de acordo com isto.

| # | Pergunta | Decisão |
|---|---|---|
| 1 | Título "Rapadura" | **Fica.** O prato "Rapadura" da Cozinha vira **Torrão Dourado** (§3.6). "Rapadura" só aparece no título, no logo e nos créditos. |
| 2 | Caramelo | **Fica como personagem, com nome novo: Ari** (§1.7). Vem de *aralez* (*Ara* + *lez-*, "os que lambem Ara"), é curto, igual em pt e en, e em armênio coloquial quer dizer "vem!". Ids, chaves e o modelo ficam; o conjunto vira "Cantinho do Ari" (§3.20). |
| 3 | Nome do mundo | **Ilha da Aurora** / *Dawn Isle* (§2.0). |
| 4 | Data do Bema | **Junho** (o `SeasonCalendar` não muda) (§3.15). |
| 5 | 5ª região africana | **Não por agora.** A parte africana fica na Serra de Kaggen (Montanha), nas Ruínas dos Artífices (Deserto), nas montarias, no Bema e nos contos de Ananse. Fica como ideia para depois (savana ou costa suaíli). |
| 6 | Seis NPCs trocados | **Aceitos:** Nana, Vandak, Yardena, Coti, Aso, Adda (§3.1). |
| 7 | Príncipe das Trevas | **Vilão final invencível:** na campanha ele é empurrado de volta (selo refeito), nunca vencido. Mais tarde vira a raide online "A Grande Treva", que só recua por uma temporada. O chefe do mundo das Cavernas é **Mahaway** (§1.4, §3.4). |
| 8 | Sleipnir | **Fica** (§3.17). |
| 9 | "Seu/Dona" | **Saem.** No lugar: Mestre/Mestra para ofício, ou nome + epíteto. As crianças dizem "tio/tia" (§3.1, §4.2). |
| 10 | Templo Solar da Maria | **Fica** (§4.2). |

---

## 7. Fontes principais (para conferir)

- **Maniqueísmo:** Teodoro bar Konai, *Liber Scholiorum* XI (cosmogonia); *Kephalaia* coptas (Medinet Madi); *Saltério
  Maniqueu copto* (Salmos do Bema); *Acta Archelai* (Hegemônio); Codex Maniqueu de Colônia; textos partas, médio-persas e
  sogdianos de Turfan (M 2, M 7, M 47, M 1306...); *Compêndio da Doutrina de Mani, o Buda da Luz* (Mingjiao, 731);
  al-Nadim, *Fihrist*; cartas de Kellis (Dakhla); Agostinho, *Contra Faustum* e *De moribus Manichaeorum*. Leitura:
  Gardner e Lieu, *Manichaean Texts from the Roman Empire*; Klimkeit, *Gnosis on the Silk Road*; Sundermann sobre os
  missionários; Henning e Reeves sobre o *Livro dos Gigantes*.
- **Rota da Seda:** Cartas Antigas sogdianas (Nanai-vandak); pinturas de Panjakent; reino uigur de Qocho; Shiji 123
  (cavalos de Fergana).
- **Outras:** sagas dos Nartes (Ossétia: Kurdalægon, Uasamonga); Moisés de Corene (aralez); folclore armênio
  (mardagayl, vishap, cavalos do lago Van); *Canção de Ullikummi* (hurrita/hitita); Tages (Cícero, *De Divinatione*);
  Heródoto IV (ouros citas); mito komi de Yen e Omöl; *dainas* bálticas (Saulė); Bundahishn (asno de três patas);
  al-Qazwini (shadhavar).
- **África antiga e oral:** Kemet (ushabti, Bes, Taweret, Khepri, Wepwawet, Mehen, Bennu, barcas Mandjet e Mesektet);
  Kush/Meroé (Apedemak, pirâmides); Bleek e Lloyd, *Specimens of Bushman Folklore* (\|Kaggen, Coti, Kwammang-a, a
  lua-sapato); tradição akan de Ananse e Aso; contos iorubás de Ìjàpá; Faro dos bambara; litema basotho; Grande Zimbábue;
  estelas de Aksum; Fumo Liyongo (épica suaíli); canções de boi dos dinka e dos nuer.
- **[verificar] antes de publicar texto:** Uzzai, a "raposa-joia" (*mehrmohre*), e qualquer grafia com diacrítico na UI
  (Āz → "Az" se a fonte do jogo não tiver o "ā").
