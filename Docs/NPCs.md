# NPCs — aparência procedural a partir da história

> Código: `Assets/Scripts/Gameplay/NPC/Visuals/` (`NpcRecipes` = uma receita por NPC com o porquê de cada escolha em
> comentário; `NpcModelBuilder` = corpo/cabeça/roupa/props; `NpcFigure` = renderer + idle; `NpcSceneDresser` = veste as
> cápsulas da cena em runtime). Testes: `Assets/Tests/EditMode/NpcVisualsTests.cs`. Prévia: **Rapadura ▸ NPCs ▸ Preview All NPCs**.
> Fontes de lore: `Docs/WorldBible_Maniqueismo.md` §0, §2 (paletas), §3.1 (elenco); textos das cidades/quests; folhas de
> conceito em `Assets/Textures/NPCs` (Mariam, Adamas, Ammo, Parthenos, Vó Edna) usadas só como referência.

## 1. O sistema

- **Proporção chibi como os primos:** cabeça grande (~4,5 cabeças de altura). Idade muda altura e tamanho da cabeça
  (criança 1,15 m · jovem 1,5 m · adulto 1,75 m · idoso 1,66 m), `Height` ajusta ±25 %.
- **Corpo paramétrico:** porte (magro, médio, barrigudo, ombros largos), postura (ereta, orgulhosa = peito para fora,
  curvada = idoso, tímida = cabeça baixa, relaxada). Ombros por porte (largo 1,34× — ferreiro/marceneiro são uma cunha;
  magro 0,9×; idosos um pouco mais estreitos). Túnica longa, cafetã e vestido afinam até o joelho e **abrem na barra**
  (perfil cintura → joelho → barra em 3 anéis), o chapan abre um pouco.
- **Braços com volume:** ombro arredondado na cor da roupa, braço que afina até o cotovelo, antebraço com **punho** na cor
  de acabamento (mangas largas "de sino" em túnica longa/cafetã; braço nu = manga curta da roupa + pele), mão-luva
  (punho fechado + polegar). Postos por **IK de dois ossos** até um *encaixe da mão* por pose (`NpcArmPose`), medido no
  próprio corpo (barriga, saia, bolsa): `Hang` cai ao lado, cotovelo solto para trás, mão à frente da coxa (sem T/A-pose);
  `HoldFront` mãos à frente da barriga com a folga do objeto; `HoldStaff` punho ao lado/à frente, à altura do peito (bengala
  à altura do quadril); `HipRest`, `Carry`, `HoldUp`, `Crook` idem.
- **Encaixe na mão** (`NpcModel.HandL/HandR` + eixo de pegada): cada tipo de prop tem o seu jeito de segurar — cajado,
  poste, cajado de pastor e bengala ficam **verticais passando pelo punho** até o chão (o idle não balança esse braço,
  `NpcModel.GroundedR`); cabo do martelo/escova ao longo do antebraço/pegada; cesto **pendurado no antebraço** (alça por
  cima da manga); lamparina e pilão **na palma**; jarra, xícara, tábuas e lamparina de duas mãos **entre as palmas**;
  balança, jarro, almotolia e fuso **pendurados abaixo da mão**; lanterna pendurada num cordão no gancho do poste;
  picareta e tora **no ombro**, com a mão segurando à frente; flauta passando pelos dois punhos.
- **Cabeça:** rosto feito para ler a ~10 m na câmera iso: **olhos grandes** (branco + íris escura olhando um pouco para
  dentro + brilho; olhos de riso "^", sonhadores semicerrados, calmos em arco), **sobrancelhas grossas em todos** (o humor
  vem delas), nariz em cunha, boca pelo humor (riso aberto com língua, sorriso em "U", linha séria), lábio rosado em pele
  escura, **bochechas coradas** (`NpcRecipe.RosyCheeks`, nulo = criança/risonho/tímido/alegre/gentil). Capuz e lenço têm
  **abertura no rosto** (puxados para trás) e a franja fica acima das sobrancelhas. Cabelo (curto, longo, coque, trança, duas tranças, rabo,
  crespo, topete, espetado, careca com tufos), barba (bigode, cavanhaque, curta, cheia, longa, longa trançada com anéis).
- **Na cabeça:** lenço amarrado (Vó Edna), capuz com gola, turbante de 3 voltas com copa, envoltório do deserto com cauda,
  gorro alto de feltro (kalpak, torre afinando com abas viradas), doppa de 4 lados, bandana de forja, chapéu cônico de
  palha largo (mokorotlo de Litema), capacete de mineiro com aba e lamparina (acesa ou fria), tiara, coroa de flores,
  tiara com disco de sol. Barbas com volume: cheia/longa com costeletas ligando ao cabelo, longa em dois andares.
- **Silhueta da roupa:** túnica longa, cafetã (faixa frontal e barra), chapan até o joelho sobre calça, túnica curta,
  vestido rodado. **Camadas:** avental, colete de feltro/couro, faixa (com ponta), cinto com fivela, xale, capa,
  cobertor listrado (Litema), cachecol, bolsa a tiracolo, remendos.
- **Props de ofício:** martelo com brasa, tenaz, cesto de ervas, pilão, cajado de caravana com sinos, rolo de dormir e
  mochila, sino de camelo, rolo de corda, escova, arco, aljava, penas, poste de lanterna com gancho, lamparina de barro,
  tambor, lira, dutar nas costas, flauta, jarra, peça de tecido, carretéis, picareta, almotolia, feixe de pavios, tora no
  ombro, bolsa de moedas, balança, cesto de flores, carta e pena, jarro de cobre, chaves, cajado de pastor, fuso e lã,
  tábuas de argila, porta-pergaminho, pergaminho, bengala, pingente de cristal/sol, xícara, cajado da lua, óculos de
  areia, boneco da aranha Ananse, trouxa, louva-a-deus no ombro.
- **Barato para mobile:** uma `SkinnedMeshRenderer` por NPC (5 ossos rígidos: raiz, corpo, cabeça, braços), malha em
  cache por receita (todos os iguais dividem a mesma), **um único material** (`NpcPalette`, textura de paleta 8×9 com
  emissão nas células de luz; a linha 9 tem branco do olho, lábio, língua, rubor para pele escura, sobrancelha
  grisalha), 750–1350 triângulos (teto 1500), idle parado fora da tela.
- **Idle procedural** (`NpcIdleMath`): respiração; gesto do ofício; cabeça vira para o jogador (±65°, até 7 m) e, bem
  perto, o corpo todo gira devagar para encará-lo.
- **Onde aparece:** lojistas das cidades andáveis (`CityTownView` → `CityResidentPost`), Adda na Torre (`TorreProps`), e
  toda `NpcInteractable` só com cápsula/primitiva (Sandbox da `Main.unity`) — o renderer da cápsula é desligado e o
  colisor/gatilho/interação ficam como estavam. Moradores sem nome viram aldeões variados com a paleta da região.

## 2. Elenco (traço da história → visual)

| NPC (ids antigos) | Traço da história | Visual | Gesto |
|---|---|---|---|
| **Vó Edna** (`vo_edna`, ponto de encontro) | âncora da família, chá de hortelã, guarda o Cristal do Sol | idosa baixinha e redonda, lenço laranja estampado, vestido terracota com avental, xícara nas duas mãos, pingente âmbar que brilha | bebe o chá |
| **Mariam, a Ervateira** (`cida`, `forest_rezadeira`) | calma, "avó de todo mundo", cheira a hortelã, chás e ervas | idosa de cabelo prateado sob capuz verde-musgo, avental de linho, cesto de ervas e romãs no braço, pilão na mão | mói no pilão |
| **Mestre Adamas** (`severino`, `blacksmith`) | ferreiro forte que ri alto, martelo "meu amigo" | ombros largos e peito estufado, rosto rindo, barba longa trançada com anéis, bandana, braços nus e luvas, avental de couro, tenaz, martelo com brasa | ergue e bate o martelo |
| **Mestre Omóforo** (`damiao`) | carrega o peso nos ombros sem reclamar | alto e largo, tora no ombro, outra mão na cintura, rosto sereno | balanço lento sob o peso |
| **Mestra Narisah** (`fatima`) | elegante, vaidosa do bom gosto, bordados sogdianos | magra e alta, postura orgulhosa, cafetã turquesa com faixas douradas, coque com tiara, tecido de seda no braço, carretéis na faixa, mão na cintura | inclina a cabeça, admirando |
| **Zaku, o Merceeiro** (`lazaro`, `shopkeeper`) | econômico, confere tudo duas vezes | magro, chapan índigo, doppa, cavanhaque, bolsa de moedas, balança erguida | conta com a mão, duas vezes |
| **Nana, a Feirante** (`nenem`) | tagarela, sabe de tudo, ama flores | redonda e alegre, duas tranças ruivas, coroa de flores, vestido romã, cesto de flores | fala com as duas mãos |
| **Srosh, o Tratador** (`tonico_haras`) | escuta bicho e gente, fala pouco | sem chapéu (orelhas à mostra), rabo de cavalo, colete de couro, rolo de corda, escova | afaga devagar |
| **Patig, o Rastreador** (`patig`, boca da rua Oeste da Vila) | paciente, mestre de pegadas, respeita os bichos | ombros para frente e cabeça baixa lendo o chão, verde de mata, capa marrom, penas na tiara, arco e aljava | varre o chão com o olhar |
| **Mestre Xroshtag** (`xroshtag`, praça da Vila) | o Chamado, voz de trovão | grande e barrigudo, boca aberta, turbante romã, cafetã açafrão, dutar nas costas | ergue o braço chamando de longe |
| **Padwaxtag** (`padwaxtag`, ao lado do mestre) | a Resposta, tímido, voz fininha | jovem de cabeça baixa e bochechas coradas, cores do mestre em menor, flauta de junco | espia de vez em quando |
| **Ramrat, a Guardiã da Lâmpada** (`socorro_vila`, diante da casa da lâmpada) | serena, acende uma lâmpada para quem sai | toda de branco com capuz e faixa dourada, lamparina nas duas mãos | ergue a lamparina devagar |
| **Ammo, o Caravaneiro** (`bento`, mascate) | velho contador de causos, amigo de estrada do Vô Jorge | barba branca curta e riso, envoltório e cachecol azuis com óculos de areia, cafetã ferrugem, mochila e rolo de dormir, sino da Teimosa, cajado de latão com sinos | apoia-se no cajado |
| **Náfsha, a Oleira** (`rosa`, olaria de Qocho) | mãos sujas de barro, canta no torno | mãos cor de barro e mangas arregaçadas, avental manchado, vestido rosa, jarra nas mãos, boca aberta cantando | amassa o barro |
| **Vandak, o Seleiro** (`vava`) | mercador esperto, escreve cartas à família | chapan violeta, gorro de feltro, bigode, avental de seleiro, carta e pena | rabisca |
| **Yardena, a da Cisterna** (`socorro`) | prática, mandona, guarda cada gota | peito para fora, sobrancelhas severas, mão na cintura, túnica azul-água, envoltório turquesa, jarro de cobre, chaves no cinto | inclina o jarro medindo |
| **Uzzai, o Pastor** (`tiao`) | brincalhão, imita bicho | jovem rindo, cabelo crespo, colete de feltro, cajado de pastor, sino de camelo | bate os braços imitando bicho |
| **Mestre Mihr, o Mineiro** (`tonho`, `mountain_miner`) | grave, de poucas palavras, coração mole | largo e alto, rosto sério, capacete com lamparina acesa, cobertor listrado azul/amarelo, picareta no ombro | carrega firme |
| **Coti, a Tecelã** (`graca`) | fala com o louva-a-deus da janela; mantas para a neve | lenço amarelo, vestido azul-cobertor, cobertor listrado, fuso e lã, **louva-a-deus no ombro** | gira o fuso |
| **Innaios, o Tocador** (`quinzinho`) | animado, desafina de propósito | magro rindo, chapéu cônico de palha, túnica amarela, lira erguida, tambor no quadril | batuca no tambor |
| **Parthenos, a das Lamparinas** (`benedita`, `caves_lampkeeper`) | jovem doce e corajosa, não teme o escuro | postura orgulhosa, trança saindo do capuz azul, colete, bolsa e capa, poste de lanterna com gancho | ergue a lanterna |
| **Mihrshah, o Velho Mineiro** (`candinho`) | sonhador, esquecido, viu um jardim de cristais | idoso curvado, olhos sonhadores, barba longa branca, capacete com a lamparina fria (aposentado), chapan remendado, bengala, cristal brilhando no peito | apoia-se, sonhando |
| **Aso, a Contadora** (`fia`) | engraçada, faz voz de bicho, histórias de Ananse | redonda e risonha, turbante açafrão, vestido violeta-noite, xale, **boneco da aranha Ananse** e tambor | gestos largos contando |
| **Gabryab, o do Pavio** (`zezinho`) | menino rápido, quer ser amigo da Lavine | criança de cabelo espetado, almotolia de cobre, feixe de pavios brancos | pulinhos inquietos |
| **Adda, o Vigia da Torre** (Torre) | velho e emotivo, chora quando a jarra volta | idoso curvado, barba branca, gorro alto de feltro branco, túnica índigo com faixa clara, poste com lanterna | enxuga uma lágrima |
| **Sobrevivente da Vila** (`village_survivor`, Joaquim) | fugiu da onda de sombra com o que pôde | idoso curvado, chapan marrom remendado, trouxa nas costas, bengala | apoia-se na bengala |
| **Aprendiz do Sol** (`temple_acolyte`, Maria) | acende as candeias da sala de cura | jovem de branco, faixa açafrão, disco de sol na testa, lamparina erguida | ergue a luz |
| **Estudioso das Trilhas** (`windtrail_scholar`, Maithe) | curioso, lê a espiral dos ushabtis na serra | cachecol e capa turquesa ao vento, porta-pergaminho nas costas, pergaminho e bastão | lê e olha para cima |
| **Arquivista das Ruínas** (`ruins_archivist`, Ícaro) | decifra as tábuas dos Artífices | idoso redondo, turbante creme, cafetã dourado, bolsa de notas, duas tábuas de argila | lê as tábuas |
| **Eremita Místico** (`hermit_mystic`, Lavine) | a Noite Rubra; a névoa lilás é filtro para a luz | alto e curvado, capuz e túnica violeta-noite com lilás, faixa vermelha (lua rubra), barba branca longa, cajado com lua crescente e pedra que brilha | mão no queixo, pensando |
| **Gara, a Guardiã das Copas** (`gara`, Copas de Garajonay) | guardiã das copas, dá as missões da cidade, a névoa fala com ela | elfa alta e serena, **orelhas longas**, pele cor de casca, cabelo outonal solto, coroa de urze, túnica de louro com barra de névoa, capa de musgo, lanterna de casulo no poste | ergue a lanterna devagar |
| **Jonay, o Arqueiro** (`jonay`) | treina o tiro; a Maithe adora | elfo ereto e de olhar vivo, rabo de cavalo outonal, tiara de louro, túnica musgo, cachecol de urze, arco e aljava | chama os aprendizes de longe |
| **Dácil, a Ervateira** (`dacil`, Ervaria) | ervas e poções de névoa, a loja da cidade | jovem elfa gentil, duas tranças outonais, coroa de musgo, vestido de louro, avental de névoa e bolsa, cesto de ervas e jarra de água de névoa | verte a gota medida |
| **Bencomo, o Velho Chefe** (`bencomo`) | graduações e convites; amigo do Vô Jorge | elfo idoso largo e orgulhoso, cabelo e barba longos brancos, faixa dourada na testa, cafetã de casca com ouro, capa de louro, cajado e pergaminho do convite | apoia-se e assente |
| **Tinguaro, o Rastreador** (`tinguaro`) | lê rastro no musgo, tarefas de caça | elfo calmo de coque escuro e cavanhaque, chapan de casca, colete de couro, bolsa de laços, rolo de corda, bastão de rastreio | varre o musgo com o olhar |
| **Capitão Liyongo, o Nakhoda** (`liyongo`, Taa Pwani) | capitão dos dhows, orgulhoso e risonho, dá as missões da cidade; leva o nome do arqueiro-poeta da costa | largo e de peito aberto, barba cheia, kofia de coral com bordado dourado, kanzu branco sob colete índigo com fio de ouro, faixa, rolo de corda no ombro e estojo de cartas náuticas | mão na cintura, chama a tripulação |
| **Mwana, a Mercadora de Pérolas** (`mwana`, Bazar de Pérolas) | esperta, conta pérola duas vezes; vendeu pérola ao Vô Jorge | kanga vermelha na cabeça, vestido índigo com barra de madrepérola, xale de kanga, pés de hena, balança de latão erguida e bolsa de moedas | conta com a mão |
| **Nuru, a Guardiã do Farol** (`nuru`, ao pé do Farol de Coral) | velha serena; *nuru* = luz; acende a lâmpada que traz os dhows de volta | um pouco curvada, capuz e túnica de pano de vela com barra cor de mar, xale azul-mar, poste de lanterna e lamparina | ergue a lâmpada ao entardecer |
| **Zawadi, o Trançador de Raízes** (`zawadi`) | jovem rápido, "pole pole"; trança as Botas de Raiz Viva, dá a 2ª missão | cabeça descoberta e cavanhaque, túnica de laterita, avental de pano de vela, cinto, feixe de raiz de mangue no braço e fuso | fia, sempre |
| **Mbaraka, o Navegador das Estrelas** (`mbaraka`) | velho marinheiro, lê as estrelas da Barca da Lua e conta as monções; ensinou o Vô Jorge | curvado, barba longa branca, kilemba de coral com faixa azul-mar, kanzu de pano de vela sob capa de teca, bastão da lua e estojo de cartas | olha o céu pensando |
| **Gurparanzah, a Chefe-Arqueira** (`gurparanzah`, Tambor Rubro) | herói dos contos hurritas; palavra dada vale mais que machado; dá as missões da cidade e a prova G4 do Ícaro | orc alta e orgulhosa, pele verde-oliva, presinhas, trança longa, tiara de bronze, túnica ocre-rubra sob colete de couro e faixa amarela, arco e aljava | vigia o horizonte |
| **Kessi, o Caçador** (`kessi`) | caçador do conto hurrita, só caça o que o acampamento come; dá a 2ª missão | orc jovem e magro, pele cinza, capuz de couro, túnica de arenito, capa de couro e cinto, rolo de corda de laço e feixe de couros | encostado, calmo |
| **Appu, o Mercador Gentil** (`appu`) | do conto hurrita: sonhava com filhos e adota todos os órfãos do acampamento; o Vô Jorge pagava em dobro | orc idoso e gordo, careca de barba cheia grisalha, gorro de feltro ocre, cafetã de arenito com faixa amarela, bolsa, xícara de chá e bolsa de moedas | conversa com todo mundo |
| **Ashmunikal, a Matriarca dos Tambores** (`ashmunikal`) | rainha hitita (histórica); guarda a Lâmpada Guardiã no alto da torre do tambor, toca as horas | orc idosa e ereta, pele cinza, cabelo longo grisalho sob turbante ocre, túnica longa ocre-rubra com xale de couro, tambor de mão e lamparina | bate o tambor |
| **Tushratta, o Ferreiro do Tambor** (`tushratta`, Forja de Tambor) | rei de Mitanni (histórico); cobre, bronze e ferro na batida do tambor; a loja da cidade | orc largo, coque no alto e barba curta, braços à mostra, túnica ocre-rubra, avental de couro e cinto, martelo e tenaz | martela |
| **Napirasu, a Mestra Fundidora** (`napirasu`, Salões de Bronze) | rainha elamita (a estátua de bronze do Louvre); refino e fusão; guarda o cristal apagado da Grande Forja; a Vó Edna mandou doce de damasco no inverno em que a forja apagou | anã forte e altiva, duas tranças ruivas sob tiara de bronze, vestido de cobre esverdeado, avental de couro e cinto, tenaz e martelo | martela |
| **Untash, o Mestre de Obras** (`untash`) | Untash-Napirisha, o construtor do zigurate; dá as missões da cidade; mede duas vezes; a dobradiça de barbante do Vô Jorge o irrita há quarenta anos | anão largo e careca, barba longa trançada com contas de bronze, túnica de granito, colete de couro e cinto, planta enrolada e vara de medir | pensa, mão no queixo |
| **Puzur, o Guardião das Runas** (`puzur`, Portões de Runa) | Puzur-Inshushinak, o rei da escrita linear elamita; a loja da cidade; grava as runas que dão instruções à Luz | anão idoso e curvado, barba longa branca, capuz de cobre esverdeado, túnica longa de granito com xale debruado de bronze, tabuletas e pena | escreve |
| **Shilhak, o Minerador** (`shilhak`) | Shilhak-Inshushinak (rei elamita); canta para a pedra antes da picareta; dá a 2ª missão e paga com chá de gengibre | anão jovem e forte, barba curta, capacete de lamparina, túnica de brasa fria, avental de couro e bornal, picareta e lamparina | carrega |
| **Kidin, o Cozinheiro do Salão** (`kidin`) | Kidin-Hutran (rei elamita); o forno dele é o lugar mais quente desde que a forja apagou; faz o pão de damasco da Vó Edna | anão gordo e risonho, barba cheia ruiva, gorro de feltro, cafetã de granito, avental branco, xícara e jarra | serve |
| **Miwnay, a Mestra da Feira** (`miwnay`, Feira da Miwnay) | a carta de Miwnay (*Cartas Antigas* sogdianas); engarrafa o Ar Sereno; a loja da cidade | kobold de escamas ferrugem e focinho, coque sob turbante de lona, cafetã de cobre com faixa lilás e bornal, jarra e balança | chama os fregueses |
| **Shayn, a Consertadora** (`shayn`) | a filha de Miwnay nas cartas; conserta e recicla sucata ("nada está quebrado, só parou de ser o que era") | kobold jovem e magra de escamas ardósia, rabo de cavalo sob bandana verde, óculos de proteção, túnica de lona, avental de couro com remendos, martelo e carretéis de fio | martela |
| **Farnkhund, o Vagoneiro** (`farnkhund`) | nome sogdiano dos documentos de Mug; conhece cada desvio pelo rangido; dá a 2ª missão e paga com o Óleo do Fogo Eterno | kobold largo de escamas ferrugem, gorro de feltro, chapan de basalto com cachecol verde e cinto, molho de chaves e vara de lanterna | ergue a lanterna |
| **Takhsich, o Inventor** (`takhsich`) | nome sogdiano dos documentos de Mug; faz os cata-ventos que puxam o Ar Sereno; dá as missões da cidade (o favorito do Ícaro) | kobold magro de escamas ardósia, crista espetada, óculos de proteção, túnica de vidro verde sob colete de cobre remendado, almotolia e carta de planos | imita o cata-vento |
| **Enmerkar, o Chefe Construtor** (`enmerkar`, Aratta) | o épico sumério de Enmerkar e o Senhor de Aratta (inventou a escrita na argila para responder ao rival); dá as missões da cidade; escreveu o nome dos primos numa tabuinha | povo-touro largo e altivo (1,2×), pelagem escura, chifres largos, focinho com argola de latão, barba suméria cheia, tiara de fita azul, túnica longa de junco com barra de barro e faixa azul, tabuletas e vara de medir | explica com as duas mãos |
| **Lugalbanda, o Andarilho** (`lugalbanda`, Alforje do Lugalbanda) | o épico de Lugalbanda (perdeu-se doente nas montanhas e voltou); mapas secretos; a loja da cidade, com a Relíquia dos Artífices; o Vô Jorge lhe deu meio pão na estrada | povo-touro magro e sonhador, pelagem castanha-fulva, rabo de cavalo, barba curta, capuz de junco, chapan de palha com remendos e bornal, rolo de mapa erguido, cajado e trouxa nas costas | vasculha o horizonte |
| **Shulgi, o Corredor** (`shulgi`) | o Hino de Shulgi (correu de Nippur a Ur num dia); treina a Corrida de Shulgi; dá a 2ª missão | povo-touro jovem e leve, coque no alto, tiara dourada com fita azul, túnica curta dourada com faixa azul, braços à mostra, fita de corrida erguida e jarra d'água | pulinhos inquietos |
| **Ensuhkeshdanna, o Rival Amigo** (`ensuhkeshdanna`) | o senhor de Aratta que perdeu a disputa de charadas para Enmerkar; hoje faz as charadas e chama os duelos amistosos da praça | o maior do povo-touro (1,24×), corpulento, pelagem fulva, barba longa trançada, chapan de barro vermelho com capa azul e cinto, mão no quadril e tambor de mão | bate o tambor |
| **Kirta, o Rei do Recife** (`kirta`, Kirta) | o épico ugarítico de Kirta (perdeu a família no mar e reconstruiu tudo); dá as missões da cidade; o Vô Jorge chorou ao ver o coral aceso | povo-do-mar de pele turquesa, barbatanas no lugar das orelhas, guelras, cabelo longo e barba curta, diadema de coral, cafetã de madrepérola com barra de coral e faixa de alga, cajado e pingente | chama os mergulhadores |
| **Pughat, a Que Cuida de Todos** (`pughat`) | a heroína do épico de Aqhat; missões de resgate ("três entram, três saem"); dá a 2ª missão e paga com a Bolha de Ar | povo-do-mar jovem de pele de madrepérola, crista de barbatana turquesa, rabo de cavalo, túnica turquesa com faixa de coral e cinto, rolo de corda no ombro e jarra d'água | pulinhos, pronta para mergulhar |
| **Danel, o Juiz Justo** (`danel`, Ferramentas de Mergulho) | o pai de Aqhat, juiz justo na porta da cidade; uma bolha de ar por mergulhador, nunca duas; a loja da cidade (Concha Respirável, Bolha de Ar) | povo-do-mar idoso e curvado, pele turquesa, barba longa branca, capuz de madrepérola, manto azul-mar com xale de madrepérola, balança erguida e chaves das prateleiras de concha | pensa antes de falar |
| **Aqhat, o Arqueiro** (`aqhat`) | o herói do épico de Aqhat, dono do arco presente dos artesãos (aqui, do recife); treinador; prometeu à Vó Edna nunca atirar em gaivota | povo-do-mar largo de pele de madrepérola, cabelo curto e crista de barbatana turquesa, túnica de alga sob colete de coral, arco e aljava | imita a puxada do arco |
| **Salmaios, o Guardião da Raiz-Mãe** (`salmaios`, Jhilpur) | o asceta do Códice de Colônia; aqui, o velho que cuida da Raiz-Mãe e come uma tigela de arroz por dia; dá as missões da cidade; guarda o nome do Vô Jorge entalhado numa raiz | idoso magro e à vontade, barba longa branca, turbante de sândalo com faixa de Água Viva, cafetã açafrão sob capa verde-raiz, cajado erguido e estojo de cartas | pensa, depois sorri |
| **Nuhzadag, o Barqueiro** (`nuhzadag`, Barcos e Trocas) | o intérprete de Mani; aqui, o barqueiro que fala as onze línguas do delta; a loja da cidade (Vara de Equilíbrio, Barco de Junco, Amuleto Antiveneno, Fruta Purificadora) | adulto de barba cheia, solidéu anil, cafetã verde-água-turva sob colete garança e faixa açafrão, rolo de corda no ombro e estojo de papéis de troca | chama do outro lado do canal |
| **Abiesous, o Mestre da Escola de Barco** (`abiesous`) | o mestre do Códice de Colônia; aqui, o professor da escola de barco (na monção as ruas viram canais); dá a 2ª missão e paga com a Fruta Purificadora; o Vô Jorge foi o pior aluno dele | idoso ereto de cabelo longo grisalho sob capuz de folha de lótus, manto anil com xale verde-raiz, vara de empurrar barco com lanterna e lamparina | ergue a lanterna |
| **Tehat, a Erveira** (`tehat`) | uma mulher das cartas de Kellis (escrevia à irmã toda semana); aqui, a erveira do bairro de ervas: nim, açafrão-da-terra e lótus; a hortelã da Vó Edna cresce na janela dela | adulta magra de coque sob turbante garança, vestido rosa-lótus com barra açafrão, xale e faixa de sândalo, balança erguida e bolsinha de sementes | conta as folhas duas vezes |
| aldeões genéricos | moradores de cada cidade | sorteio estável por lote na paleta da região: Zarafshan (damasco, choupo, romã, índigo; doppa, kalpak, lenço), Qocho (arenito, rosa, ouro, azul; envoltórios), Litema (barro, cal, cobertores; chapéu cônico), Lamparinas (branco, tufo, cobre; capuz, lamparina) | variado |

## 3. Notas

- Nenhum NPC é sagrado ou fundador (bíblia §0.2): os nomes vêm dos contos, a roupa vem de culturas vivas **com
  respeito** (mokorotlo, cobertor basotho, doppa, kalpak, turbante) — só beleza emprestada, nunca inimigo (§2.5).
- **Modelo importado da Vó Edna** (`Assets/Models/NPCs/VoEdna`, OBJ/GLB de outra sessão, arquivos **não alterados**):
  ligado só atrás de um flag, **desligado por padrão** — **Rapadura ▸ NPCs ▸ Use Imported Vó Edna Model (experimental)**
  (EditorPrefs). `NpcImportedModels` (runtime) é o adaptador: escala o OBJ (1 unidade de altura, centrado) para a altura da
  receita (1,53 m) com os pés em y = 0, gira para +z (`Yaw`, 0 — o OBJ já olha para +z; se aparecer de costas, 180 em
  `NpcImportedModelsEditor.Sources`) e usa um material URP Lit montado como o da paleta (`EnemyModelPalette.CreateMaterial`)
  com a textura `VoEdna_tex.png`. Ele pendura no osso Body da figura (respira, balança e se vira para o jogador), mas é
  estático (sem gesto nem cabeça virando) e pesa ~14 400 triângulos e um material a mais — por isso fica no flag. Só o
  editor registra o modelo (`NpcImportedModelsEditor`, via `AssetDatabase`); build de player continua procedural. Para
  usar em build: rigar (o zip "Mixamo" só tem o OBJ, sem esqueleto), reduzir para ~1500 tris e referenciar por um
  catálogo/Resources.
- Ids antigos continuam valendo (aliases em `NpcRecipes`), nada de save muda.
- **Elfos-da-mata** (Copas de Garajonay, 2026-11-03): `NpcRecipe.ElfEars` desenha a orelha longa (um cone achatado de 4
  lados de cada lado da cabeça, varrido para cima e para trás, atravessando o cabelo; substitui a orelha redonda) e a
  paleta ganhou a linha 10, de Garajonay (`SkinBark`, `Laurel`, `MossLeaf`, `Bark`, `Mist`, `MistWater` com brilho leve,
  `Heather`, `HairAutumn`, Docs/Mapas100.md §3.3). Nomes da ficha 05 (lenda de Gara e Jonay; Bencomo, Tinguaro e
  Dácil de Tenerife), nunca "Seu/Dona".
- **Marinheiros e mercadores suaílis** (Taa Pwani, 2026-11-03): a paleta ganhou a linha 11 (`Coral`, `Teak`, `SeaBlue`,
  `Sail`, `Laterite`, `Pearl`, `KangaRed`, `Henna`, Docs/Mapas100.md região 5). Kanzus brancos, coletes bordados com fio
  de ouro, kofia e kilemba, kangas e índigo. Nomes da ficha 25 (Capitão Liyongo, Mwana) e palavras suaílis (*nuru* =
  luz, *zawadi* = presente, *mbaraka* = bênção), nunca "Seu/Dona".
- **Orcs do Tambor Rubro** (2026-11-03): `NpcRecipe.Tusks` desenha duas presinhas de marfim saindo do lábio de baixo
  (lêem da câmera iso; entram na `SilhouetteKey` como "/orc") e a paleta ganhou a linha 12 (`SkinOrcOlive`,
  `SkinOrcGrey`, `OchreRed`, `RedSandstone`, `Hide`, `BannerYellow`, `Bronze`, `Ash`, Docs/Mapas100.md §3.3). Nada de
  pintura de guerra nem troféu: honra é trabalho e hospitalidade. Nomes dos contos hurritas (Gurparanzah, Kessi, Appu)
  e da história hitita/mitânia (Ashmunikal, Tushratta), nunca "Seu/Dona".
- **Anões dos Salões de Bronze e kobolds do Mercado dos Túneis** (2026-11-03): a paleta ganhou a linha 13
  (`SkinKoboldRust`, `SkinKoboldSlate`, `BronzeBright`, `Verdigris`, `RuneGold`, `ColdEmber`, `GlassGreen`, `SereneAir`;
  a runa, o vidro e o Ar Sereno brilham). Os anões são baixos (`Height` 0,78–0,84) e largos, barba trançada com contas
  (`NpcBeard.LongBraided`), granito, cobre esverdeado e bronze; nomes elamitas (Napirasu, Untash, Puzur, Shilhak, Kidin).
  Os kobolds ganharam `NpcRecipe.Snout` (focinho curto e arredondado com almofada escura e duas narinas; entra na
  `SilhouetteKey` como "/kobold"; as escamas lisas são a cor da pele, que conta como pele nos braços), são pequenos
  (0,75–0,8), de lona, cobre e vidro de garrafa; nomes das *Cartas Antigas* sogdianas e dos documentos de Mug (Miwnay,
  Shayn, Farnkhund, Takhsich). Nenhum é avarento nem bruto, nunca "Seu/Dona".
- **Elenco de lore nas 4 cidades** (TODO.md "História e Arcos dos NPCs"): todos têm figura e porta. Quem não tem casa
  própria na receita fica num **ponto de pé** da praça (`CityTownRecipes.AddStandingSpot`: lote `Market` de 1 × 1 m,
  sem parede nem chão), conferido por `CityGarajonayTests.LoreCast_OfTheFirstFourCities_StandsInTheirTowns_OutsideEverySolid`.
