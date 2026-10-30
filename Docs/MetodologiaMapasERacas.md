# 🗺️ Metodologia de Níveis de Mapas Estilo MU Online & Grande Compêndio de Raças da Ilha da Aurora e Além

> **Documento Mestre de Arquitetura de Mundo & Progressão**  
> **Status:** Ativo / Fonte da Verdade para Regras de Acesso a Mapas, Teleporte (M), Restrições de Equipamento e Enciclopédia de Raças.  
> **Inspiração:** *MU Online* (sistemas de Warp, Walk Level, restrições por Asas/Dinorant, penalidades de nível de monstro, PK restrictions) combinado com o folclore maniqueísta e multicultural da Ilha da Aurora (Rota da Seda, África antiga, Pérsia, tradições Védicas/Índicas, lendas Nórdicas e universos fantásticos).

---

# PARTE I — A METODOLOGIA DE NÍVEIS ESTILO MU ONLINE

No *MU Online*, os mapas não são apenas áreas contínuas; eles são **degraus rígidos de progressão e sobrevivência**. Entrar em um mapa antes da hora é garantia de morte rápida por acerto crítico dos monstros ou incapacidade de causar dano.

No *Rapadura*, adaptamos essa metodologia para a progressão de Níveis 1 a 500 (com foco imediato no escopo 1–100):

## 1. As Três Barreiras de Acesso a Mapas

Todo mapa do jogo possui três níveis de corte:

1. **Nível de Caminhada a Pé (`WalkLevel`):**
   * O nível mínimo que o herói precisa ter para atravessar o portal físico, ponte ou fronteira entre o mapa anterior e o novo mapa.
   * É sempre **mais baixo** que o nível de teleporte, incentivando aventureiros ousados a irem a pé se quiserem se arriscar mais cedo.
2. **Nível de Teleporte / Janela de Movimento M (`WarpLevel`):**
   * O nível necessário para abrir o menu de viagem rápida (tecla **M** no teclado ou botão de bússola na HUD mobile) e se teleportar diretamente para a zona segura do mapa via Barcas de Luz.
   * Geralmente é **`WalkLevel + 5`** ou **`WalkLevel + 10`** níveis acima da caminhada a pé.
3. **Custo de Ouro por Teleporte (`WarpFee`):**
   * Fórmula estilo MU Online:  
     $$\text{Custo (Ouro)} = \text{BaseFee} + (\text{WarpLevel} \times 15)$$
   * Se o jogador estiver com a mochila sobrecarregada (>100% de capacidade), o custo de teleporte dobra (+100%).

---

## 2. Exigências Especiais de Equipamento (Gated Content)

Exatamente como em *MU Online* (onde você precisa de **Asas / Dinorant** para entrar em *Icarus*, ou penalidade de lentidão severa em *Atlans* sem luvas/montaria aquática), o *Rapadura* possui barreiras ambientais rígidas:

| Exigência Especial | Efeito no Acesso | Mapas Afetados | Exemplo de Item Necessário |
|---|---|---|---|
| **Asas da Luz / Montaria Alada** | Impossível pisar no mapa sem estar com Asas equipadas ou montado em criatura alada (queda no abismo). | Mapas celestiais e pontes de vento (ex: *Forte dos Ventos*, *Picos de Farnkesh*, *Ilhas Flutuantes*). | Asas de Plumas da Aurora (+0 a +15), Hipogrifo, Pégaso. |
| **Lamparina Rúnica Acesa** | Escuridão total (visão zero, dano de sombra por segundo contínuo de 2% HP/s). | Masmorras profundas, Catacumbas e Salões Subterrâneos. | Lamparina de Alabastro de Parthenos, Óleo de Fogo Eterno. |
| **Proteção contra Calor Escaldante** | Perda constante de estamina (-10/s) e redução de 30% na velocidade sem manto de linho ou sela térmica. | Dunas profundas e Cânions de Lava. | Túnica de Seda do Deserto, Sela de Deserto de Vandak. |
| **Resistência à Nevasca Extrema** | Congelamento progressivo (estranhamento e lentidão até paralisia total). | Geleiras e passos do Teto do Mundo (acima do Lv 80). | Manta de Lã da Tecelã Coti, Bebida de Gengibre do Tandir. |
| **Botas de Raiz / Bóias de Lodo** | O lodo profundo afunda o herói (velocidade reduzida em 70% e impossibilidade de pular). | Brejos do Delta de Jhilpur e Charco do Ninki Nanka. | Botas Trançadas de Raiz Viva, Montaria Capivara/Cavalo-Marinho. |
| **Chave Rúnica / Ticket de Entrada** | Portão bloqueado até consumir o item correspondente. | Masmorras fechadas e Criptas secretas. | *Chave de Osso Rúnica* (`item_bone_key`). |

---

## 3. Modificadores de Classe no Nível de Acesso (Estilo Magic Gladiator / Dark Lord)

No *MU Online*, classes avançadas como MG e DL entram nos mapas muito antes das classes básicas (cerca de 2/3 do nível). No *Rapadura*, o avanço de classe e afinidades elementares conferem bônus de entrada antecipada:

* **Ícaro (Engenheiro das Máquinas):** Pode entrar em mapas de ruínas e engenhos com **-5 níveis de exigência** graças ao seu decodificador de portas antigas.
* **Maithe (Arqueira dos Ventos):** Exigência de nível para mapas de serra, montanha e pontes suspensas reduzida em **-5 níveis** (passo leve sobre terrenos acidentados).
* **Joaquim (Guardião da Terra):** Imunidade natural à penalidade de lentidão de lodo e pântanos; resistência a armadilhas de masmorra.
* **Maria (Curandeira da Luz):** Redução de 50% no dano recebido por escuridão em mapas subterrâneos mesmo com a lamparina fraca.
* **Lavine (Maga do Ar Sereno):** Imunidade a névoas venenosas e ilusões de mapas assombrados.
* **Personagens em Nova Jornada+ (NG+):** Mantêm o teleporte liberado para todas as capitais das regiões já concluídas na jornada anterior.

---

## 4. O Sistema de Perigo de Monstro (Monster Level Delta)

Quando o herói entra em um mapa com nível significativamente inferior ao dos monstros do local:
* **Diferença de +10 Níveis do Monstro:** O monstro tem **+25% de chance de acerto crítico** contra o jogador.
* **Diferença de +20 Níveis do Monstro:** O jogador tem **-40% de taxa de acerto (Hit Rate)**, gerando mensagens frequentes de "Errou!" (*Miss*), e os danos recebidos aumentam em 1.5x.
* **Diferença de +30 Níveis:** Dano sofrido é letal (quase hit-kill), desencorajando jogadores a pular degraus de conteúdo.

---

## 5. Renascer Canônico & Cidades de Retorno (Respawn Rules)

1. Morrer dentro de uma masmorra ou mapa perigoso **NUNCA** ressuscita o jogador dentro do mesmo mapa.
2. O jogador é imediatamente teleportado para a **Praça Central da Cidade de Retorno** designada para aquela região (onde fica a Lâmpada Protetora de Ramrat).
3. Todo mapa possui em sua tabela a sua cidade canônica de vínculo.

---

# PARTE II — TABELA METODOLÓGICA DOS 100 MAPAS (NÍVEIS 1 A 500)

Abaixo está o catálogo completo de acesso de todos os mapas planejados, estruturados rigorosamente pelo modelo de progressão:

### Região 1: Bosque do Zarafshan (Níveis 1–32) — *Luz e Terra*
| # | Mapa | Faixa Monstro | Walk Level | Warp (M) Level | Custo Ouro | Exigência Especial | Cidade de Retorno | Raça Predominante |
|---|---|---|---|---|---|---|---|---|
| **01** | Pomar da Vó Edna | 1–3 | 1 | 1 | 0 | Nenhuma (Tutorial) | Vila da Vó Edna | Humanos (Sogdianos) |
| **02** | Tugai do Rio Dourado | 2–5 | 2 | 5 | 50 | Nenhuma | Vila da Vó Edna | Humanos |
| **03** | Brejo do Lodo Escuro | 16–22 | 14 | 18 | 180 | Botas de Couro | Vila da Vó Edna | Monstros Apagados |
| **04** | Bosque das Amoreiras | 18–24 | 16 | 20 | 220 | Nenhuma | Vila da Vó Edna | Humanos / Elfos da Mata |
| **05** | Copas de Garajonay | 20–26 | 18 | 22 | 300 | Plataformas Elevadas | Copas de Garajonay | **Elfos-da-Mata** |
| **06** | Jardim de Mihrshah | 26–32 | 24 | 28 | 450 | Chave do Jardim | Copas de Garajonay | Espíritos do Jardim |

### Região 2: Areias Flamejantes (Níveis 5–38) — *Fogo Vivo e Ruínas*
| # | Mapa | Faixa Monstro | Walk Level | Warp (M) Level | Custo Ouro | Exigência Especial | Cidade de Retorno | Raça Predominante |
|---|---|---|---|---|---|---|---|---|
| **07** | Estrada das Caravanas | 5–7 | 5 | 8 | 100 | Nenhuma | Oásis de Qocho | Humanos Caravaneiros |
| **08** | Ruínas dos Artífices | 6–9 | 6 | 10 | 150 | Nenhuma | Oásis de Qocho | Construtos Ushabtis |
| **09** | Galerias do Karez | 20–26 | 18 | 22 | 320 | Nenhuma (Subterrâneo) | Oásis de Qocho | Homens-Toupeira |
| **10** | Dunas que Cantam | 22–28 | 20 | 25 | 400 | Água Fresca no Alforje | Oásis de Qocho | Serpentes e Nômades |
| **11** | Ermos do Tambor Rubro | 24–32 | 22 | 28 | 500 | Nenhuma | Tambor Rubro | **Orcs do Deserto** |
| **12** | Trono de Namrael | 30–38 | 28 | 32 | 650 | Proteção a Veneno | Tambor Rubro | Arcontes Escorpião |

### Região 3: Serra de Kaggen (Níveis 9–44) — *Vento Vivo e Pedra*
| # | Mapa | Faixa Monstro | Walk Level | Warp (M) Level | Custo Ouro | Exigência Especial | Cidade de Retorno | Raça Predominante |
|---|---|---|---|---|---|---|---|---|
| **13** | Trilha dos Rondavéis | 9–11 | 8 | 12 | 160 | Nenhuma | Vila de Litema | Humanos da Montanha |
| **14** | Passo do Hahya | 10–13 | 9 | 14 | 200 | Manta de Lã | Vila de Litema | Gigantes / Golens |
| **15** | Abrigos Pintados | 26–32 | 24 | 28 | 480 | Nenhuma | Vila de Litema | Pintores Rupestres |
| **16** | Muralhas de Pedra Seca | 28–34 | 26 | 30 | 550 | Nenhuma | Vila de Litema | Construtores Ancestrais |
| **17** | Salões de Bronze Napirasu | 30–38 | 28 | 34 | 700 | Picareta / Tocha | Salões de Bronze | **Anões Forjadores** |
| **18** | Cume dos Mazans | 36–44 | 34 | 40 | 900 | Resistência à Nevasca | Salões de Bronze | Gigantes da Serra |

### Região 4: Grutas do Limiar & Catacumbas (Níveis 13–50) — *Sombra e Marfim*
| # | Mapa | Faixa Monstro | Walk Level | Warp (M) Level | Custo Ouro | Exigência Especial | Cidade de Retorno | Raça Predominante |
|---|---|---|---|---|---|---|---|---|
| **19** | Lago do Limiar | 13–15 | 12 | 16 | 240 | Lamparina | Vila das Lamparinas | Povo das Grutas |
| **20** | Salão de Tages | 14–18 | 13 | 18 | 300 | Lamparina | Vila das Lamparinas | O Habitante Ancestral |
| **21** | Campo das Pedras-Dragão | 32–38 | 30 | 35 | 750 | Lamparina Rúnica | Vila das Lamparinas | Vishaps de Pedra |
| **22** | Mercado dos Túneis | 34–42 | 32 | 38 | 850 | Nenhuma | Mercado dos Túneis | **Kobolds Comerciantes** |
| **23** | Cânion da Roda | 38–46 | 36 | 42 | 1000 | Óleo de Fogo Eterno | Mercado dos Túneis | Construtos das Jarras |
| **24** | Ninho de Mahaway | 42–50 | 40 | 46 | 1200 | Asas ou Salto Duplo | Vila das Lamparinas | Harpias das Trevas |
| **24b**| **Catacumbas de Marfim** | 35–45 | 32 | 38 | 800 | **Chave de Osso Rúnica** | Vila das Lamparinas | **Esqueletos / Caveiras** |

### Região 5: Costa das Monções (Níveis 45–72) — *Água e Tempestade*
| # | Mapa | Faixa Monstro | Walk Level | Warp (M) Level | Custo Ouro | Exigência Especial | Cidade de Retorno | Raça Predominante |
|---|---|---|---|---|---|---|---|---|
| **25** | Taa Pwani e o Recife | 45–50 | 42 | 48 | 1300 | Barco / Cais | Taa Pwani | **Humanos Suaílis** |
| **26** | Savana dos Auroques | 48–55 | 45 | 52 | 1500 | Nenhuma | Aratta | **Povo-Touro (Aratta)** |
| **27** | Pirâmides de Meroé | 52–58 | 50 | 56 | 1700 | Relíquia dos Artífices | Aratta | Ushabtis Reais |
| **28** | Cidade-Recife de Kirta | 55–62 | 52 | 60 | 2000 | Concha Respirável | Kirta | **Povo-do-Mar** |
| **29** | Kilwa Afundada | 60–66 | 58 | 65 | 2400 | Bolha de Ar / Natação | Kirta | Fantasmas do Mar |
| **30** | Mangues de Liyongo | 64–72 | 60 | 70 | 2800 | Botas de Raiz | Taa Pwani | Crocodilianos |

### Região 6: Delta de Jhilpur (Níveis 60–88) — *Tradição Védica e Raiz Viva*
| # | Mapa | Faixa Monstro | Walk Level | Warp (M) Level | Custo Ouro | Exigência Especial | Cidade de Retorno | Raça Predominante |
|---|---|---|---|---|---|---|---|---|
| **31** | Jhilpur nas Raízes | 60–66 | 58 | 65 | 2500 | Nenhuma (Capital) | Jhilpur | **Humanos Védicos / Nagas** |
| **32** | Pontes de Raiz Viva | 64–70 | 62 | 68 | 2800 | Equilíbrio em Pontes | Jhilpur | Homens-Macaco (Vanaras)|
| **33** | Palafitas de Tarchon | 70–78 | 66 | 74 | 3300 | Barco de Junco | Palafitas de Tarchon| **Trols do Brejo** |
| **34** | Charco do Ninki Nanka | 76–82 | 72 | 80 | 3800 | Amuleto Anti-Veneno | Palafitas de Tarchon| Dragões do Lodo |
| **35** | Coração do Lodo | 82–88 | 78 | 85 | 4500 | Fruta Purificadora | Jhilpur | Hidras Antigas |

### Região 7: Teto do Mundo (Níveis 70–98) — *Gelo Eterno e Vento*
| # | Mapa | Faixa Monstro | Walk Level | Warp (M) Level | Custo Ouro | Exigência Especial | Cidade de Retorno | Raça Predominante |
|---|---|---|---|---|---|---|---|---|
| **36** | Sangkent a Escadaria | 70–76 | 68 | 75 | 3500 | Roupa Térmica Refinada| Sangkent | **Monges das Alturas** |
| **37** | Calhas de Pedra | 74–80 | 70 | 78 | 4000 | Nenhuma | Sangkent | Golens de Gelo |
| **38** | Geleira Ancestral | 80–86 | 76 | 84 | 4600 | Grampos de Escalada | Sangkent | Ursos Polares Antigos |
| **39** | Forte dos Ventos | 84–92 | 80 | 88 | 5200 | **Asas / Montaria** | Sangkent | Grifos da Neve |
| **40** | Passo de Wakhan | 90–98 | 86 | 94 | 6000 | Proteção a Tempestade | Sangkent | Centauros do Frio |

### Região 8: Planície de Mingcheng (Níveis 90–125) — *A Luz Imperial*
| # | Mapa | Faixa Monstro | Walk Level | Warp (M) Level | Custo Ouro | Exigência Especial | Cidade de Retorno | Raça Predominante |
|---|---|---|---|---|---|---|---|---|
| **41** | Mingcheng dos Nove Anéis| 90–96 | 88 | 95 | 6500 | Nenhuma (Capital) | Mingcheng | **Humanos Imperiais** |
| **42** | Torres Brancas Amanirenas| 95–102| 92 | 100 | 7500 | Selo do Sol | Torres Brancas | **Altos-Elfos da Luz** |
| **43** | Canal das Lanternas | 102–110| 98 | 106 | 8500 | Barco de Bambu | Mingcheng | Soldados Espelho |
| **44** | Arrozais Espelhados | 108–116| 104| 112 | 9600 | Nenhuma | Mingcheng | Gandharvas (Músicos) |
| **45** | Muralha das Mil Torres| 115–125| 110| 120 | 11000| Tocha Imperial | Torres Brancas | Arcontes de Bronze |

---

# PARTE III — O GRANDE COMPÊNDIO DE RAÇAS DO MUNDO

O mundo de *Rapadura* não possui raças "do mal por natureza". Todas as raças carregam partículas da **Luz Viva** presas na matéria. Abaixo está a enciclopédia detalhada de todas as raças do universo:

---

### 1. Povos Humanos e Culturas da Estrada

#### 1.1 Sogdianos da Alvorada (A Gente da Vila da Vó Edna)
* **Local de Origem:** Bosque do Zarafshan (Vila da Vó Edna, Tugai).
* **Aparência:** Roupas de linho cru com golas altas bordadas em padrões geométricos, turbantes e xales suaves de feltro. Peles morenas claras, olhos expressivos.
* **Cultura & Sociedade:** Agricultores de damascos, romãs e melões. Comerciantes honrados e mestres do forno *tandir*. Valorizam a família acima de tudo; as reuniões de férias em torno da comida são sagradas.
* **Afinidade:** Terra e Água Viva.
* **NPCs Notáveis:** Vó Edna, Mestre Adamas, Mariam a Ervateira, Zaku o Merceeiro.

#### 1.2 Nômades das Areias de Qocho
* **Local de Origem:** Oásis de Qocho, Dunas que Cantam.
* **Aparência:** Túnicas multicamadas em tons terracota e areia, véus faciais protetores (*tagelmust*), óculos com aros de latão contra a poeira e botas macias para não afundar nas dunas.
* **Cultura:** Conhecedores dos caminhos invisíveis do vento. Domesticadores de camelos bactrianos e escaravelhos gigantes. Amantes das histórias contadas ao redor da fogueira à noite.
* **NPCs Notáveis:** Ammo o Caravaneiro, Vandak o Seleiro.

#### 1.3 Homens das Muralhas de Kaggen (Tradição San / Basotho)
* **Local de Origem:** Serra de Kaggen, Vila de Litema.
* **Aparência:** Mantas grossas de lã pura com desenhos geométricos arrojados, gorros cônicos de palha (*mokorotlo*) e peles curtidas.
* **Cultura:** Vivem em casas circulares de pedra com telhado cônico de palha (*rondavéis*). Exímios escaladores e mestres na técnica de empilhar pedras secas sem argamassa. Falam com sabedoria pausada e cantam em coro polifônico.
* **NPCs Notáveis:** Coti a Tecelã, Mestre Mihr o Mineiro.

#### 1.4 Marinheiros e Mercadores Suaílis (Taa Pwani)
* **Local de Origem:** Costa das Monções, Porto de Taa Pwani.
* **Aparência:** Kanzus brancos impecáveis, coletes bordados com fios dourados e sandálias de couro trançado.
* **Cultura:** Povo do comércio marítimo e dos barcos de vela triangular (*dhows*). Navegam guiados pelas estrelas da Barca da Lua. Exportadores de especiarias, incenso, pérolas e tecidos tingidos de anil.
* **NPCs Notáveis:** Capitão Liyongo, Mwana a Mercadora de Pérolas.

#### 1.5 Cavaleiros Nartes das Estepes
* **Local de Origem:** Estepe dos Nartes, Acampamento de Soslan.
* **Aparência:** Túnicas de feltro grosso com cintos ornados de placas de prata e botas altas flexíveis. Cabelos trançados com contas de turquesa.
* **Cultura:** Montadores natos; dizem que a criança Narte aprende a cavalgar antes de aprender a andar. Respeito inviolável pela palavra dada e duelos de arquearia a galope.

---

### 2. Povos Élficos (Os Guardiões da Luz Vegetal)

#### 2.1 Elfos-da-Mata de Garajonay (Copas de Garajonay)
* **Aparência:** Orelhas longas e afiladas voltadas para trás, cabelos em tons de folhas de outono e casca de vidoeiro. Roupas confeccionadas com seda crua de amoreiras e fibras vegetais vivas.
* **Cultura:** Não derrubam árvores. Suas casas são suspensas por pontes de cipós nos troncos gigantescos. Praticam a arquearia de precisão e a linguagem silvada (*silbo*), que ecoa a quilômetros entre os vales sem assustar os pássaros.
* **Relação com os Humanos:** Relação fraterna com Maithe e os primos; admiram quem sabe ouvir o farfalhar das folhas.

#### 2.2 Altos-Elfos Solares de Amanirenas (Torres Brancas de Mingcheng)
* **Aparência:** Estatura esguia e alta, porte majestoso, olhos dourados sem pupilas visíveis que brilham sob a luz direta do meio-dia. Vestes de seda branca imaculada com placas de ouro polido.
* **Cultura:** Guardiões da matemática astronômica e dos prismas solares. Construtores de obeliscos e torres que canalizam a luz para os arrozais e cidades. Valorizam a harmonia e a etiqueta milenar.

---

### 3. Povos Órquicos e Clãs dos Ermos

#### 3.1 Orcs do Tambor Rubro (Os Filhos da Argila Vermelha)
* **Local:** Cânions dos Ermos do Tambor Rubro.
* **Aparência:** Pele em tonalidades de terracota avermelhada e ocre, presas inferiores proeminentes, musculatura densa e cicatrizes rituais que brilham levemente ao entoarem cantos de guerra.
* **Cultura:** Não são vilões selvagens. São mestres metalúrgicos do cobre e bronze pesado, fabricantes dos maiores tambores de ressonância do mundo, cujo som espanta os arcontes do deserto. Sua honra baseia-se na força do trabalho e na hospitalidade das tendas de couro.

---

### 4. Povos Mortos-Vivos & Ossadas Ancestrais (Os Despertos do Limiar)

#### 4.1 Guerreiros do Ossuário de Marfim
* **Local:** Catacumbas de Marfim, Criptas Subterrâneas.
* **Aparência:** Esqueletos completos preservados em pó de alabastro, vestindo fragmentos de cotas de malha de bronze e elmos rúnicos antigos. Órbitas iluminadas por chamas de fogo-fátuo azul.
* **Cultura:** Eram os guerreiros voluntários que se fecharam nas catacumbas para guardar o Selo dos Artífices. Com o enfraquecimento do Cristal, acordaram desmemoriados. Ao serem derrotados em combate leal, sua memória volta por alguns instantes, agradecendo os heróis antes de suas faíscas subirem em paz para a Coluna da Glória.

---

### 5. Povos de Inspiração Védica & Índica (Delta de Jhilpur)

#### 5.1 Jhilpuris (Os Homens das Raízes Sagradas)
* **Local:** Jhilpur, Cidade nas Raízes.
* **Aparência:** Dhotis e saris de algodão tingido em açafrão, anil e cúrcuma. Marcas na testa de pasta de sândalo aromático.
* **Cultura:** Vivem em perfeita simbiose com figueiras banyans colossais. Direcionam raízes aéreas por gerações para formar pontes, templos e moradias sem cortar uma única tora.
* **Afinidade:** Água Viva e Vida Vegetal.

#### 5.2 Nagas (O Povo-Serpente do Rio Dourado)
* **Aparência:** Metade superior humanoide de traços nobres com escamas furta-cor nos braços e nuca; metade inferior em cauda de serpente graciosa de escamas verde-água e douradas.
* **Cultura:** Guardiões das profundezas dos rios e canais subterrâneos. Mestres na manipulação das correntes de água e no encantamento de pérolas mágicas.

#### 5.3 Vanaras (Os Homens-Macaco Acrobatas)
* **Aparência:** Humanoides com traços simiescos ágeis, cauda preênsil forte e pelos castanhos dourados.
* **Cultura:** Construtores de passarelas suspensas sobre o charco. Brincalhões, adoradores de frutas doces e mestres de artes marciais usando bastões de bambu temperado.

#### 5.4 Gandharvas (Os Músicos do Éter)
* **Aparência:** Seres graciosos que parecem flutuar sobre os espelhos d'água dos arrozais, vestindo sedas transparentes que nunca tocam a lama.
* **Cultura:** Não empunham armas cortantes; canalizam faíscas de luz através de sitares e cítaras, cujas notas curam a fadiga de quem escuta.

---

### 6. Outros Povos e Raças Fantásticas

#### 6.1 Anões de Napirasu (Os Mestres da Forja de Bronze)
* **Local:** Salões de Bronze de Napirasu (Serra de Kaggen).
* **Aparência:** Estatura baixa e compacta, barbas densas presas por anéis de metal forjado, aventais de placas de amianto e óculos com lentes de quartzo fumê.
* **Cultura:** Mineradores que conhecem cada veio de minério pelo cheiro da rocha. Especialistas em ligas de bronze estelar e engrenagens movidas a vapor d'água de fontes termais.

#### 6.2 Kobolds das Galerias Subterrâneas
* **Local:** Mercado dos Túneis (Grutas do Limiar).
* **Aparência:** Pequenos humanoides escamosos com olhos grandes adaptados à penumbra, focinhos expressivos e orelhas em abano.
* **Cultura:** Os maiores comerciantes clandestinos da ilha. Vendem cogumelos fosforescentes, peças de engenho encontradas no entulho e gemas brutas. Fascinados por tudo que tilinta.

#### 6.3 Povo-Touro de Aratta (Auroques das Savanas)
* **Local:** Savana dos Auroques (Costa das Monções).
* **Aparência:** Humanoides bovinos majestosos de 2.4 metros de altura, chifres curvados imponentes e pelagem espessa castanha ou preta.
* **Cultura:** Pastores de auroques selvagens e guerreiros de labrys (machados de lâmina dupla). Respeitam profundamente as leis da hospitalidade: quem partilha o pão e o sal em suas tendas está sob a proteção de todo o clã.

#### 6.4 Povo-do-Mar de Kirta (Tritões dos Recifes)
* **Local:** Cidade-Recife de Kirta.
* **Aparência:** Pele em tons de madrepérola e turquesa, guelras delicadas no pescoço e membranas natatórias entre os dedos.
* **Cultura:** Vivem em palácios submarinos sob redomas de ar mágico geradas por corais gigantes. Mestres na forja de armas feitas de dentes de tubarões fósseis e conchas reforçadas.

#### 6.5 Trols das Palafitas de Tarchon
* **Local:** Palafitas de Tarchon (Delta de Jhilpur).
* **Aparência:** Gigantes de 3 metros com pele grossa e esverdeada como casca de árvore velha, sobre a qual crescem líquens e musgos.
* **Cultura:** Pacífico povo pescador que vive sobre palafitas gigantescas de madeira no brejo fundo. Alimentam-se de bagres gigantescos e raiz de lótus. Quando enfurecidos pela sombra, desferem golpes capazes de rachar barcos inteiros.

#### 6.6 Centauros de Soslan
* **Local:** Estepe dos Nartes.
* **Aparência:** Tronco humanoide musculoso sobre o corpo vigoroso de cavalo das estepes. Crinas longas decoradas com tranças e contas de osso.
* **Cultura:** Nômades velozes guiados pelas constelações da Barca do Sol. Seus líderes são os mestres do laço e da montaria sem sela.

---

# PARTE IV — DIRETRIZES DE IMPLEMENTAÇÃO C# NA UNITY

Para que o Claude e os desenvolvedores transformem essa metodologia em código executável, os seguintes scripts devem ser criados e integrados:

### 1. `MapAccessRules.cs` (`Assets/Scripts/Gameplay/World/Map/MapAccessRules.cs`)
```csharp
using System;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.World.Map
{
    public static class MapAccessRules
    {
        public static bool CanWalkIntoMap(int playerLevel, int walkLevel, CharacterId heroId, string regionType)
        {
            int required = walkLevel;
            // Bônus de classe
            if (heroId == CharacterId.Maithe && regionType == "Mountain") required = Math.Max(1, required - 5);
            if (heroId == CharacterId.Icaro && regionType == "Ruins") required = Math.Max(1, required - 5);

            return playerLevel >= required;
        }

        public static bool CanWarpToMap(int playerLevel, int warpLevel, int currentGold, int warpCost, out string reason)
        {
            if (playerLevel < warpLevel)
            {
                reason = $"Nível insuficiente para teleporte! Exige nível {warpLevel}.";
                return false;
            }
            if (currentGold < warpCost)
            {
                reason = $"Ouro insuficiente! Custo da Barca: {warpCost} moedas.";
                return false;
            }

            reason = string.Empty;
            return true;
        }

        public static int CalculateWarpCost(int warpLevel, bool isOverloaded)
        {
            int baseCost = 50 + (warpLevel * 15);
            return isOverloaded ? baseCost * 2 : baseCost;
        }
    }
}
```

### 2. Integração com a UI da Barca de Luz (`FastTravelController.cs`)
* O menu de viagem rápida lista todos os 100 mapas;
* Mapas com `playerLevel < WarpLevel` aparecem em cinza com o ícone de cadeado e o texto: *"Exige Nível {WarpLevel}"*;
* Ao clicar em um mapa bloqueado, uma dica de tela sugere: *"Você pode tentar ir a pé pelo mapa anterior a partir do nível {WalkLevel}"*.

---

# PARTE V — CHECKLIST DE TAREFAS NO TODO.MD

- [ ] Criar ScriptableObject `MapDefinition.cs` contendo `walkLevel`, `warpLevel`, `warpFee`, `specialGateRequirement` e `returnCityId`.
- [ ] Implementar classe utilitária `MapAccessRules.cs` com testes EditMode cobrindo bônus de classe e sobrecarga.
- [ ] Criar a interface de teleporte de mapas no mapa-múndi (`FastTravelWindow.uxml` e `FastTravelWindowController.cs`).
- [ ] Criar o catálogo de ScriptableObjects `RaceDefinition.cs` para suportar diálogos e NPCs das 25 raças detalhadas.
