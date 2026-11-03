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
- **Elenco de lore nas 4 cidades** (TODO.md "História e Arcos dos NPCs"): todos têm figura e porta. Quem não tem casa
  própria na receita fica num **ponto de pé** da praça (`CityTownRecipes.AddStandingSpot`: lote `Market` de 1 × 1 m,
  sem parede nem chão), conferido por `CityGarajonayTests.LoreCast_OfTheFirstFourCities_StandsInTheirTowns_OutsideEverySolid`.
