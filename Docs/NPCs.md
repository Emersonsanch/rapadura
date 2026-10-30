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
  curvada = idoso, tímida = cabeça baixa, relaxada).
- **Cabeça:** rosto desenhado com decalques planos (olhos abertos com brilho, olhos fechados de riso, olhos sonhadores,
  sobrancelhas severas/erguidas, boca, bochechas coradas, nariz), cabelo (curto, longo, coque, trança, duas tranças, rabo,
  crespo, topete, espetado, careca com tufos), barba (bigode, cavanhaque, curta, cheia, longa, longa trançada com anéis).
- **Na cabeça:** lenço amarrado (Vó Edna), capuz com gola, turbante de 3 voltas, envoltório do deserto com cauda,
  gorro alto de feltro (kalpak), doppa de 4 lados, bandana de forja, chapéu cônico de palha (Litema), capacete de mineiro
  com lamparina (acesa ou fria), tiara, coroa de flores, tiara com disco de sol.
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
  cache por receita (todos os iguais dividem a mesma), **um único material** (`NpcPalette`, textura de paleta 8×8 com
  emissão nas células de luz), 600–1500 triângulos, idle parado fora da tela.
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
| **Patig, o Rastreador** (Barraca de caça) | paciente, mestre de pegadas, respeita os bichos | ombros para frente e cabeça baixa lendo o chão, verde de mata, capa marrom, penas na tiara, arco e aljava | varre o chão com o olhar |
| **Mestre Xroshtag** (casa de música) | o Chamado, voz de trovão | grande e barrigudo, boca aberta, turbante romã, cafetã açafrão, dutar nas costas | ergue o braço chamando de longe |
| **Padwaxtag** | a Resposta, tímido, voz fininha | jovem de cabeça baixa e bochechas coradas, cores do mestre em menor, flauta de junco | espia de vez em quando |
| **Ramrat, a Guardiã da Lâmpada** | serena, acende uma lâmpada para quem sai | toda de branco com capuz e faixa dourada, lamparina nas duas mãos | ergue a lamparina devagar |
| **Ammo, o Caravaneiro** (`bento`, mascate) | velho contador de causos, amigo de estrada do Vô Jorge | barba branca curta e riso, envoltório e cachecol azuis com óculos de areia, cafetã ferrugem, mochila e rolo de dormir, sino da Teimosa, cajado de latão com sinos | apoia-se no cajado |
| **Náfsha, a Oleira** | mãos sujas de barro, canta no torno | mãos cor de barro e mangas arregaçadas, avental manchado, vestido rosa, jarra nas mãos, boca aberta cantando | amassa o barro |
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
| aldeões genéricos | moradores de cada cidade | sorteio estável por lote na paleta da região: Zarafshan (damasco, choupo, romã, índigo; doppa, kalpak, lenço), Qocho (arenito, rosa, ouro, azul; envoltórios), Litema (barro, cal, cobertores; chapéu cônico), Lamparinas (branco, tufo, cobre; capuz, lamparina) | variado |

## 3. Notas

- Nenhum NPC é sagrado ou fundador (bíblia §0.2): os nomes vêm dos contos, a roupa vem de culturas vivas **com
  respeito** (mokorotlo, cobertor basotho, doppa, kalpak, turbante) — só beleza emprestada, nunca inimigo (§2.5).
- `Assets/Models/NPCs/VoEdna` (OBJ/GLB de outra sessão) **não** foi ligado: a Vó Edna procedural mantém o mesmo
  material/escala dos outros; trocar pelo modelo é só criar a figura com ele no lugar de `NpcFigure`.
- Ids antigos continuam valendo (aliases em `NpcRecipes`), nada de save muda.
