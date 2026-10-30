# Rapadura — Casa própria, lojas e guarda-roupa

> Documento de design da **Fase 15** do `TODO.md` (pedido do usuário, 2026-10-29). Só design: nada disto existe em código ainda.
> Catálogos: [`Docs/Housing/CatalogoCasa.md`](Housing/CatalogoCasa.md) (580 itens de casa) e [`Docs/Housing/CatalogoRoupas.md`](Housing/CatalogoRoupas.md) (234 roupas, acessórios, tinturas e visuais de arma).
> Os catálogos são o **conjunto inicial de lançamento**, não a lista de tudo o que pode existir. Itens novos entram por temporada/evento com ID novo.
>
> Marcadores: **[decidido]** = decisão do usuário; **[proposto]** = proposta deste documento, à espera de aprovação; **[online]** = só faz sentido no multiplayer (Fase 12); **[offline]** = vale para a campanha single-player atual.

Sumário: 1. Cidades e zona segura · 2. A casa própria · 3. Modelo 3D da casa · 4. Quem entra · 5. Dentro da casa · 6. Conforto, conjuntos e sazonais · 7. De onde vêm os itens de casa · 8. Lojas, NPCs e moedas · 9. Equipamento e aparência · 10. Economia · 11. Ideias inspiradas em MMOs clássicos · 12. Modelo de dados · 13. Plano de implementação · 14. Decisões pendentes

---

## 1. Cidades e zona segura **[decidido]**

Cada região do mapa tem **uma cidade que é zona segura**. Lá dentro:

- monstro não ataca e não entra (o spawn e o caminho da IA param na divisa; encontros aleatórios do `RandomEncounterSystem` não rolam enquanto o token está na cidade);
- jogador não ataca monstro, outro jogador nem a si mesmo (dano de skill, DoT, espinhos e invocações ficam desligados; skills de cura e buffs continuam);
- a **casa fica sempre dentro de uma cidade**, então a casa é sempre zona segura.

| Região | Cidade | Quem já mora lá (NPCs do jogo) | Lojas da cidade (§8) |
|---|---|---|---|
| Floresta | **Vila da Vó Zefa** (o sítio e a vila em volta) | Vó Zefa, Dona Cida (a rezadeira) | Armazém Geral (`shop_general_store`, já existe), Marcenaria, Armarinho, Mercearia, Costureira, Viveiro, Casa de Música, Feira livre (fim de semana), Casa de Leilões **[online]** |
| Deserto | **Povoado de Areia Branca** **[proposto: nome]** | Seu Bento, o tropeiro | Filial do Armazém, Olaria da Dona Rosa, base do Mascate Seu Bento, Feira livre (quarta) |
| Montanha | **Vila da Serra Alta** **[proposto: nome]** | Mestre Tonho, o mineiro | Filial do Armazém, Forja do Mestre Tonho, Feira livre (quinta) |
| Cavernas | **Vila das Lamparinas** **[proposto: nome]** | Benedita das Lamparinas | Filial do Armazém, Lamparinas da Benedita, Feira livre (sexta) |

Regras ligadas à casa:

- **[offline]** a campanha tem uma casa só, na Vila da Vó Zefa. As outras cidades abrem conforme o chefe da região anterior cai (mesma regra de `WorldMapProgress`); nelas há lojas, mas não terreno.
- **[online]** cada cidade tem uma **rua de terrenos** (ver o quadro de terrenos, §11.13). Cada conta tem **uma casa principal**; a mudança de cidade custa uma taxa (§10.4). O pé de cajá do terreno (§2.1) vai junto na mudança.
- A cidade é o único lugar em que se entra na casa: no mapa mundi, o botão "Minha casa" aparece quando o token está parado no nó da cidade, igual ao "Enfrentar o chefe" de hoje.

---

## 2. A casa própria

### 2.1 Como se ganha: o pé de cajá **[proposto]**

**[decidido]** Todo jogador começa com uma **casa de madeira**, que vai melhorando a partir daí.

História (Missão da Casa 1, "Um Cantinho Seu"): logo depois do prólogo, na primeira volta à vila (depois das 3 primeiras vitórias na Floresta, antes do Lobo Alfa), a Vó Zefa leva o primo até o fundo do sítio:

> "O seu avô plantava um pé de cajá pra cada neto que nascia. Este aqui é o seu. A terra em volta dele é sua também — e a casinha de madeira que ele levantou pra descansar nas viagens de tropeiro agora é sua casa. Tem goteira, tem cupim, mas tem amor."

- O pé de cajá é decoração fixa do quintal (não sai do lugar, não conta no limite de itens). Ele cresce junto com a casa: muda em N1, árvore com frutas em N5.
- **[offline]** a casa é "a casa dos primos": é do save, e os cinco moram nela. O primo ativo é quem aparece; os outros quatro aparecem como NPCs de visita (§4.6).
- **[online]** quem não é um dos cinco primos (se o usuário escolher personagem próprio, ver Fase 12.1) é "neto de coração" da Vó Zefa: a fala troca para "Aqui todo mundo que chega vira família".

Recompensas da missão: a casa N1, `casa_util_001` Baú de madeira simples, `casa_util_006` Guarda-roupa de duas portas, `casa_quarto_016` Cama de vento dobrável, `casa_curral_001` Casinha de cachorro do Caramelo e o `casa_util_023` Livro de visitas.

### 2.2 Níveis da casa **[decidido: madeira = nível 1; escada proposta]**

| Nível | Nome | Cômodos | Grade interna (células de 0,5 m) | Quintal (células de 1 m) | Limite de itens (dentro + quintal) | Baú da casa | Visitantes ao mesmo tempo **[online]** | Para subir |
|---|---|---|---|---|---|---|---|---|
| **N1** | Casa de madeira | 1 cômodo (sala-quarto-cozinha) + alpendre pequeno | 10 × 8 (5 × 4 m) | 8 × 8 | 40 + 15 | 20 espaços | 4 | começa com ela |
| **N2** | Casa de madeira com varanda | sala, quarto, varanda na frente | 14 × 10 (7 × 5 m) + varanda 14 × 4 | 10 × 10 (+ horta) | 70 + 25 | 30 | 6 | nível 8, Lobo Alfa derrotado, **150 ouro** + 30 madeira + 15 fibra + 5 ferro |
| **N3** | Casa de taipa e telha | sala, quarto, cozinha, varanda | 18 × 12 (9 × 6 m) | 12 × 12 (+ poço e curral) | 110 + 40 | 45 | 8 | nível 18, Guardião da Ruína derrotado, **400 ouro** + 40 madeira + 30 pedra + 20 fibra |
| **N4** | Casa de alvenaria | sala, 2 quartos, cozinha, banheiro, oficina, varanda | 22 × 16 (11 × 8 m) | 14 × 14 | 160 + 60 | 60 | 10 | nível 28, Senhor da Guerra derrotado, **1.000 ouro** + 60 pedra + 20 ferro (lingote) + 30 madeira |
| **N5** | Casarão / sede de fazenda | sala grande, 3 quartos, cozinha, banheiro, oficina, sala de troféus, varanda em L, sótão | 30 × 20 (15 × 10 m, em duas alas) | 18 × 18 (+ curral grande) | 240 + 90 | 90 | 12 | nível 38, Habitante Ancestral derrotado, **2.500 ouro** + 100 pedra + 40 ferro + 60 madeira + 1 lembrança de cada chefe |

- "Lembrança de chefe" é o troféu-placa garantido na primeira vitória contra cada chefe (§7.2). Ele não é gasto: só precisa estar na casa.
- A subida é instantânea, com uma cena curta da família ajudando (Joaquim carregando tijolo, Ícaro "otimizando" a betoneira, Maithe no telhado, Maria servindo água, Lavine pintando a porta). Na N3, N4 e N5 a cena é a Missão da Casa correspondente (§7.3).
- **Nada se perde na subida.** O que estava na casa velha fica na mesma posição relativa se couber; o que não couber vai para o **Depósito** (§2.4).
- Não há rebaixamento de nível, nem por falta de pagamento (§11.1).

### 2.3 Áreas da casa

- **Cômodos internos:** cada um tem piso, paredes e teto próprios. O jogo sabe em que cômodo cada item está (para o conforto e para as listas de acesso por porta, §11.2).
- **Varanda/alpendre:** conta como piso interno coberto: aceita rede, cadeira de balanço e planta.
- **Quintal:** grade externa de 1 m. Aceita só itens com encaixe Quintal ou Piso "de fora" (bancos, mesas de tora).
  - **Horta (N2+):** canteiros plantáveis (§11.9).
  - **Poço (N3+):** dá água para o regador e para a horta. Vem da Missão "Água Fresca".
  - **Curral (N3+):** área cercada para os animais decorativos e para o Caramelo dormir fora no calor.
  - **Galinheiro e pé de cajá:** o galinheiro é comprado; o cajá é fixo.

### 2.4 Regras de encaixe

Reaproveita o `GridPlacementUtility` que já existe (`SnapToGrid`, `GetFootprintBounds` com `rotationSteps`, `Overlaps` sem física). Por isso tudo é testável em EditMode e roda igual no servidor.

| Camada | Grade | Itens | Rotação |
|---|---|---|---|
| **Piso** | células de 0,5 m por cômodo | móveis, tapetes (tapete fica numa subcamada própria, por baixo dos móveis) | 90° |
| **Parede** | cada trecho de parede vira uma grade de 0,5 m de largura × 5 linhas (2,5 m de altura) | quadros, prateleiras, cortinas (só em janela), redes (ocupam duas paredes opostas ou dois ganchos) | não gira; espelha |
| **Teto** | mesma grade do piso | lustres, mosquiteiro, varal de lâmpadas, plantas penduradas | 90° |
| **Superfície** | cada móvel com superfície oferece N vagas (ex.: mesa 2 × 1 = 4 vagas; estante = 3 prateleiras × 3) | itens pequenos (pote, rádio, porta-retrato) | 45° |
| **Quintal** | células de 1 m | árvores, poço, curral, barracas sazonais | 90° |

- Toda porta tem **1 célula livre** na frente dos dois lados. Antes de confirmar, um *flood fill* garante que dá para ir da porta de entrada até todas as portas internas: ninguém se tranca e o visitante sempre entra.
- Itens com função (baú, cama, bancada) precisam de 1 célula livre na frente para a interação.
- Um item conta no limite assim que está colocado. Item guardado vai para o **Depósito da casa**.
- **Depósito da casa:** guarda tudo o que é item de casa e não está colocado. Não pesa e não ocupa a mochila. Limite: 300 entradas **[offline]**, 500 **[online]**; cada entrada empilha cópias do mesmo ID.
- Item de casa nunca vai para a mochila. Quando cai de um bicho, vai direto para o Depósito, como "encomenda" (§7.2). A mochila de 100 de peso e 30 espaços (Balance §9) não muda.
- Controles: **[offline/PC]** botão "Arrumar a casa" → escolher item do Depósito → mover com mouse → Q/E gira → clique confirma → Del guarda de volta. **[mobile]** arrastar com um dedo, girar com dois, botões grandes de ✓ e ↺.

---

## 3. O modelo 3D da casa **[proposto]**

### 3.1 Estilo

A casa segue o mesmo low-poly *flat-shaded* que já é feito por código no jogo:

- `EnemyMeshBuilder` e o `WorldMapMeshBuilder` do mapa montam caixas, troncos de cone, icosferas e pedras numa lista só de vértices.
- Cada face recebe a cor por uma **célula de UV de uma paleta** (`EnemyModelPalette`), num material compartilhado. O resultado é 1 malha por peça e 1 material para tudo.
- É determinístico: as mesmas chamadas sempre dão a mesma malha.
- O `WorldMapDressing` junta a decoração estática numa malha só para economizar *draw call*.

A proposta é um **`HouseMeshBuilder`** (irmão desses dois) que monta a casa a partir de **módulos** em *sockets* fixos. Assim, subir de nível **troca módulos**, sem trocar o modelo inteiro. O casco montado é juntado numa malha estática só, como o `WorldMapDressing` faz. Os móveis são prefabs separados, com o mesmo material de paleta.

### 3.2 Módulos (kit)

| Socket | N1 madeira | N2 madeira c/ varanda | N3 taipa e telha | N4 alvenaria | N5 casarão |
|---|---|---|---|---|---|
| Fundação | estacas de tora | estacas + tablado | baldrame de pedra | alicerce de tijolo com degraus | porão de pedra + escadaria |
| Piso | tábua crua | tábua corrida | chão batido + tijoleira | cerâmica vermelha | ladrilho hidráulico (pátio) + tábua (quartos) |
| Parede cheia | tábua vertical com mata-junta | tábua pintada | taipa de pilão ocre, rebocada no meio | tijolo rebocado e caiado com barrado colorido | alvenaria caiada, cunhais de pedra |
| Parede c/ janela | janela de tábua com tramela | janela de duas folhas | janela de madeira com caixilho | janela com vidro e veneziana | janela alta com bandeira e grade de ferro |
| Parede c/ porta | porta de tábua | porta de tábua com batente | porta almofadada | porta almofadada pintada | porta dupla com bandeira em leque |
| Telhado | zinco remendado + palha de buriti | tábua com cumeeira | telha colonial capa-canal | telha com beiral e cumeeira | 4 águas, beiral com lambrequim, mansarda do sótão |
| Varanda | — (alpendre de 1 tora) | pilares de tora, guarda-corpo de ripa | pilares de madeira torneada | pilares de alvenaria com arcos | varanda em L, colunas e piso de ladrilho |
| Chaminé | — | — | de barro (fogão a lenha) | de tijolo | de tijolo, dupla |
| Extras | varal, toco | banco de varanda | calha de bambu, cisterna | caixa d'água, muro baixo | cata-vento, sino da porteira, pergolado |

- Cada socket tem 1 a 3 variantes por nível, à escolha do jogador. Pintar porta, janela e barrado usa as **tintas** do guarda-roupa (§9.4; também é dreno de ouro).
- Tamanho do kit: ~30 módulos de casco × 5 níveis, a maioria só uma variação de comprimento e paleta.

### 3.3 Planta, materiais e cores por nível

- **N1 — Casa de madeira** (5 × 4 m + alpendre de 1,5 m). Um cômodo só, com a cama num canto e o fogareiro no outro. Madeira crua `#B08A5A`, junta escura `#6B4E31`, zinco cinza `#9AA3A8` com remendo de palha `#C9A55B`. Porta e janela sem pintura. A casa é acolhedora e simples, e não é triste: tem flor na janela desde o começo.
- **N2 — Madeira com varanda** (7 × 5 m + varanda de 7 × 2 m). Sala e quarto. Tábua pintada de branco-gelo `#EDE6D6` ou verde-água `#8FC1B5` (escolha). Porta e janela em **verde-bandeira** `#3F7D4E` ou **azul-anil** `#2F5D8A`. Rede na varanda já de fábrica.
- **N3 — Taipa e telha** (9 × 6 m). Sala, quarto e cozinha com chaminé de barro. Taipa ocre `#C7955A`, reboco `#E2CBA4`, telha `#B5532E`, madeira escura `#5A3E2B`. Barrado de 60 cm na cor que o jogador quiser.
- **N4 — Alvenaria** (11 × 8 m). Sala, 2 quartos, cozinha, banheiro e oficina. Cal branca `#F1EBDD`, barrado e esquadrias coloridas (paleta de casa de interior: azul, verde, amarelo-ouro `#E1B23C`, rosa `#D98C9A`), telha `#B5532E`, calçada de cimento `#BDB6A8`.
- **N5 — Casarão / sede de fazenda** (15 × 10 m em L). Ala social (sala grande, sala de troféus, cozinha) e ala íntima (3 quartos, banheiro), mais oficina nos fundos e sótão. Cal `#F4EFE3`, cunhais e soleiras de pedra `#8E8A80`, lambrequim de madeira branca, telha `#A84B2A`, portas de madeira escura envernizada `#4A3222`. Varanda em L com ladrilho hidráulico.

### 3.4 Interior e câmera

- Dentro da casa, a câmera fica em 3/4 de cima, como na exploração. **Telhado e paredes do lado da câmera somem** (*cutaway*) quando o jogador está dentro e voltam quando ele sai.
- A cor e o tipo de piso vêm do nível. A grade interna é a da tabela §2.2 e só aparece no modo "Arrumar a casa", como linhas finas.

### 3.5 Orçamento técnico (mobile primeiro)

| | N1 | N2 | N3 | N4 | N5 |
|---|---|---|---|---|---|
| Casco (triângulos) | ≤ 2.500 | ≤ 4.000 | ≤ 6.000 | ≤ 9.000 | ≤ 14.000 |
| Draw calls do casco | 1 | 1 | 1 | 1–2 | 2 |

- Móvel Comum ≤ 300 triângulos; Raro ≤ 800; Lendário/troféu ≤ 2.000.
- Todos no material de paleta, com GPU instancing para itens repetidos.
- LOD: de fora da casa, os móveis internos não são desenhados.
- **[online]** Na rua de terrenos, as casas dos outros aparecem como casco + 3 itens de quintal escolhidos pelo dono (a "fachada"). O interior só carrega dentro da instância.

### 3.6 No mapa mundi

- O nó da Vila (marco do `WorldMapDressing`) ganha uma casinha pequena ao lado do marco, no nível atual da casa do jogador: mesmo kit, só que em LOD baixo (≤ 300 triângulos).
- **[online]** as outras cidades mostram um marco de "rua de terrenos".

---

## 4. Quem entra: acesso e permissões

### 4.1 Regra base **[decidido]**

**A casa é trancada para todo mundo.** Só o dono entra, a não ser que convide alguém; a pessoa convidada pode entrar na casa do outro.

### 4.2 Convites **[online]**

| Tipo | Como funciona | Dura |
|---|---|---|
| **Convite de visita** | O dono, dentro da casa ou na porteira, escolhe um jogador próximo ou da lista de amigos → "Convidar". O convidado recebe um aviso e entra pela porteira. | Até o dono sair da casa, ou 2 horas. |
| **Lista da casa (permanente)** | O dono põe amigos na lista da casa, com um nível de permissão. Eles entram mesmo com o dono offline (se o usuário aprovar, ver §14). | Até o dono tirar a pessoa da lista. |
| **Família da conta** | Personagens da mesma conta entram sempre. | Sempre. |

- Só se convida quem é **amigo mútuo** (os dois aceitaram a amizade), ou quem está no mesmo grupo naquele momento. Convite de desconhecido não chega. É uma regra de segurança infantil.
- Revogar é imediato: quem estava dentro é levado para a porteira (como na expulsão, §4.4).
- Limite de 3 convites por minuto, para evitar spam.

### 4.3 Níveis de permissão

| Permissão | Visitar | Mexer na decoração | Usar a bancada / fogão / tear |
|---|---|---|---|
| Andar, sentar, deitar na rede, tocar instrumento, ver troféus e relíquias | ✓ | ✓ | ✓ |
| Assinar o livro de visitas | ✓ | ✓ | ✓ |
| Fazer carinho no Caramelo | ✓ | ✓ | ✓ |
| Mover, girar e guardar **itens do dono** no Depósito | — | ✓ | — |
| Colocar itens **do Depósito do dono** | — | ✓ | — |
| Usar bancada, fogão, tear e forja **com os próprios materiais** | — | — | ✓ |
| Abrir **qualquer baú** (Baú da casa, baús extras, Baú-correio) | ✗ nunca | ✗ nunca | ✗ nunca |
| Pegar item de casa para si / vender / jogar fora | ✗ nunca | ✗ nunca | ✗ nunca |
| Subir o nível da casa, mudar de cidade, mexer na lista de acesso | ✗ (só dono e co-dono, §11.3) | ✗ | ✗ |

- "Mexer na decoração" e "Usar a bancada" são independentes: dá para marcar as duas.
- **O visitante nunca abre o baú.** Isso vale para todos os níveis de permissão e não tem configuração.
- O que o visitante fabrica na bancada usa os materiais da mochila dele, e o resultado vai para a mochila dele.

### 4.4 Expulsar **[online]**

- O dono (ou co-dono) toca no nome → "Pedir para sair". O visitante é teleportado para a porteira e não entra de novo por 10 minutos (ou até ser convidado outra vez).
- Botão "Esvaziar a casa": tira todos os visitantes de uma vez.
- Bloquear um jogador (Fase 12.3, moderação) também o tira da casa e da lista.

### 4.5 Contra vandalismo **[online]**

- **Foto da casa:** quando o primeiro visitante com "mexer na decoração" entra, o servidor guarda um snapshot da casa. No fim da visita, o dono vê "O Fulano mexeu em 7 itens" com os botões **"Tá ótimo"** e **"Desfazer tudo"**, que volta ao snapshot.
- Fica um registro (*log*) de quem moveu cada item; guarda as últimas 200 ações.
- O decorador não tira nada da casa: só move ou guarda no Depósito do dono.
- O servidor valida toda colocação com o mesmo `GridPlacementUtility` e com o *flood fill* das portas.
- Um item é travado enquanto alguém o está arrastando, para que duas pessoas não o movam ao mesmo tempo.

### 4.6 Sem internet (campanha) **[offline]**

- Não há convite de verdade. A casa é do save.
- **Visitas de NPC:** de vez em quando (1 por dia de jogo, no máximo), um dos quatro primos que não estão ativos, a Vó Zefa ou um NPC das missões regionais (Dona Cida, Seu Bento, Mestre Tonho, Benedita) aparece de visita. Eles sentam, comentam um item ("Essa rede é igualzinha à da Vó!") e **assinam o livro de visitas**. As falas mudam com o progresso da campanha, como as "conversas entre primos na estrada" do TODO.
- Quando o online chegar, a casa do save pode ser importada para a conta (migração opcional, Fase 12.2).

### 4.7 Servidor **[online]**

- **Casa instanciada:** cada casa é uma instância pequena, que sobe quando alguém entra e desce depois de 5 minutos vazia. A rua de terrenos da cidade mostra só as fachadas.
- **Persistência:** tudo no banco do personagem/conta (§12.3). A escrita é transacional por ação, e o snapshot de visita fica à parte.
- Tudo o que muda a casa é **intenção do cliente validada no servidor**: colocar, mover, guardar, comprar, subir de nível.
- A colocação é limitada a 5 ações por segundo, e há um teto de itens por nível (§2.2).

---

## 5. Dentro da casa: os móveis que fazem coisas

| Móvel | ID (catálogo) | O que faz |
|---|---|---|
| **Baú da casa** | `casa_util_001` (o primeiro vem com a casa) | Guarda itens comuns do jogo (não itens de casa). Sem peso, como o Baú da Vó Zefa. Capacidade pelo nível: 20/30/45/60/90. **Baús extras** somam espaços: couro `casa_util_002` +10, ferro `casa_util_003` +15, arca `casa_util_004` +20; no máximo 1 extra no N1–N2, 2 no N3–N4 e 3 no N5. Só o dono abre. |
| **Baú-correio da Vó Zefa** | `casa_util_005` (Épico, Missão "Tijolo por Tijolo") | Abre o **Baú da Vó Zefa** (`VillageChestStorage`, 60 espaços, chave de save `"VillageChest"`) de dentro de casa, sem ir até a Vila. |
| **Guarda-roupa** | `casa_util_006`, `casa_util_007` | Abre a tela de aparência (§9). Também guarda até 3 trajes salvos (o de jacarandá guarda 6). |
| **Espelho de provador / manequim** | `casa_util_010`, `casa_util_009` | O espelho mostra o primo de corpo inteiro com a câmera girando. O manequim expõe um traje para as visitas verem. |
| **Bancada** | `casa_util_011`, `casa_util_012` | Mesma lógica das receitas de hoje (`CraftingManager`, `Resources/Recipes`) mais as receitas de móveis (§7.6). A de mestre artesão faz os móveis Épicos. |
| **Fogão a lenha** | `casa_cozinha_001` | A **Cozinha da Vó Zefa que já existe** (`MealService`, `MealCatalog`: cuscuz, café coado, pamonha, rapadura, baião de dois, queijo coalho) também funciona de casa, com os mesmos pratos e buffs. |
| **Forja de quintal / Tear / Mesa de alquimia** | `casa_util_013`, `casa_util_015`, `casa_util_014` | A forja faz receitas de ferro (`recipe_iron_ingot`, `recipe_iron_sword`) e peças de armadura metálica. O tear faz roupas e tintas. A mesa de alquimia faz poções (`recipe_health_potion`). |
| **Bacia de tingimento** | `casa_util_016` | Aplica tintas em roupas e na fachada (§9.4). |
| **Parede de troféus / Pedestal dos Gigantes Tombados** | `casa_util_022`, `casa_util_021` | Encaixes especiais onde os **troféus de chefe** (categoria Troféus) ficam com luz própria e uma plaquinha que mostra a data da primeira vitória e com qual primo. |
| **Vitrine das doze relíquias** | `casa_util_020` (conquista das relíquias) | Mostra as 12 relíquias de `RelicCatalog` (Gaita do Vô Chico, Chapéu de couro…). Cada uma acende quando é achada, e tocar nela mostra a lore (`relic.<id>.lore`). A vitrine é só exposição: a relíquia continua dando o passivo normal. |
| **Estante do Caderno da Vó** | `casa_util_019` | Abre o **Caderno da Vó Zefa** (o bestiário que já existe). Cada página completa põe um bichinho de madeira na estante. |
| **Caminha / casinha do Caramelo** | `casa_curral_002`, `casa_curral_001` | Onde o **Caramelo** (`CarameloCompanion`) dorme quando o jogador está em casa. Com a tigela (`casa_curral_003`) cheia, o carinho (`CarameloCuddle`) ganha uma animação nova. As roupinhas do Caramelo ficam no guarda-roupa (catálogo de roupas). |
| **Livro de visitas** | `casa_util_023` | Quem visita escreve uma linha. No online, o texto é escolhido de uma lista de frases prontas e emojis, mais uma frase livre com filtro de palavras (regra do chat, Fase 12.3). Guarda as últimas 100 assinaturas. |
| **Cama** | qualquer cama da categoria Quarto | Dormir carrega o **descanso** (§6.1). **[online]** sair do jogo deitado é um logout seguro (§11.5). |
| **Caixa de correio** | `casa_util_024` | Cartas e encomendas (§11.6) **[online]**. |
| **Quadro de avisos da família** | `casa_util_027` | Mostra as 3 diárias do dia (`DailyQuestService`) e o estoque do Mascate. |
| **Sino da porteira** | `casa_util_026` | **[online]** Quem está de fora toca o sino para pedir para entrar, e o dono recebe o aviso. **[offline]** É assim que as visitas de NPC chegam. |
| **Boneco de treino de palha** | `casa_quintal_037` | Treino tranquilo, com limite (§11.8). |
| **Poço** | `casa_quintal_001` | Água para a horta (§11.9). |
| **Balcão de vendas do ajudante** | `casa_util_031` | O ajudante que vende enquanto você está offline (§11.7) **[online] [proposto]**. |

---

## 6. Conforto, conjuntos e itens sazonais

### 6.1 Conforto e descanso **[proposto]**

Cada item colocado soma os pontos de **Conforto** do catálogo (Comum 1–2, Incomum 3–4, Raro 5–6, Épico 8–9, Lendário 12–15; utilitários valem metade).

- Cópias do mesmo ID contam no máximo **2 vezes**: não adianta encher a casa de banquinho.
- Itens no Depósito não contam.

| Faixa | Conforto | Nome na tela | Bônus "Descansado" |
|---|---|---|---|
| 0 | 0–19 | Casa arrumadinha | +2% XP por 10 lutas |
| 1 | 20–59 | Aconchegante | +3% XP por 15 lutas |
| 2 | 60–149 | Caprichada | +5% XP por 20 lutas |
| 3 | 150–299 | Casa de Vó | +7% XP por 25 lutas |
| 4 | 300+ | Lar dos sonhos | +10% XP por 30 lutas |

- Para ganhar o bônus, é só **dormir numa cama da própria casa**. Ele se recarrega **2 vezes por dia** (mesmo relógio das diárias, `IDailyClock`), e usar não acumula para depois.
- O bônus aparece como buff no HUD ("Descansado").
- Só XP. Não mexe em dano nem em loot, então não é pay-to-win.
- Teto prático: a N1 com 40 itens bem escolhidos chega perto de 150. A faixa 4 pede N3 ou mais com Raros e Épicos.

### 6.2 Conjuntos **[proposto]**

São 21 conjuntos no catálogo, cada um com um tema (Varanda da Vó Zefa, Tropeiro do Vô Chico, Oficina do Ícaro, Noite Rubra da Lavine, Arraiá…). A lista e a contagem de peças estão no fim do `CatalogoCasa.md`.

| Peças do mesmo conjunto colocadas | Bônus |
|---|---|
| 3 | +5 de conforto |
| 6 | +12 de conforto + **efeito de ambiente** (ex.: Arraiá toca forró baixinho; Noite Rubra põe vagalumes lilases à noite; Oficina do Ícaro faz faíscas na bancada; Cantinho do Caramelo deixa o Caramelo abanar o rabo mais) |
| Todas | +25 de conforto + título no livro de visitas ("Casa de tropeiro", "Oficina maluca"…) e, em 4 conjuntos, um mimo pequeno: **Cozinha da Vó** faz os pratos do Fogão durarem +10%; **Horta Caprichada** dá +1 colheita por canteiro; **Oficina do Ícaro** tem 10% de chance de a bancada devolver 1 madeira; **Luz da Maria** faz o "Descansado" durar +5 lutas |

### 6.3 Itens sazonais

- Os itens sazonais vêm de três eventos: **Festa Junina** (junho, ligada à ideia "Festa Junina sazonal" do TODO), **Natal Sertanejo** (dezembro) e **Carnaval** (fevereiro/março). São 45 itens nos conjuntos Arraiá, Natal Sertanejo e Frevo e Maracatu, mais 3 Lendários de família.
- Só se **ganham** durante o evento, mas **ficam para sempre**. Opção "guardar enfeites sazonais fora de época": um toque manda tudo para o Depósito, e o mesmo toque traz de volta, na mesma posição.
- O balão junino é de papel, **sem fogo** (o texto do item diz isso).
- Na volta do evento no ano seguinte, os itens antigos voltam à barraca e entram 5 a 10 itens novos.

---

## 7. De onde vêm os itens de casa

Resumo do catálogo (580 itens):

| Origem | Itens |
|---|---|
| Lojas | 376 |
| Drop de inimigo ou chefe | 80 |
| Eventos | 48 |
| Missões | 34 |
| Conquistas | 24 |
| Crafting | 12 |
| Segredos do mapa | 6 |

Um item pode ter mais de uma origem: a coluna "Origem" mostra a principal, e as tabelas abaixo somam as outras.

### 7.1 Lojas

Ver §8. Os itens de loja **também caem de bicho**, só no bioma da cidade da loja e até Raro.

### 7.2 Drop de inimigos e chefes **[proposto]**

O sorteio de item de casa é uma **rolagem a mais**, separada do loot de hoje (`EnemyLootRoller`, Balance §8.1). Não mexe nas tabelas de material, ouro e equipamento.

**Inimigo comum** (Lobo da Mata, Aranha da Floresta, Escorpião das Dunas, Golem de Pedra, Enxame de Morcegos):

| Passo | Valor |
|---|---|
| Chance de cair item de casa por morte | **4%** (5% à noite, junto com o "noite = mais encontros" do `TimeOfDayManager`) |
| Por luta (1,5 inimigo comum, Balance §8.2) | ~6% → ~1 item a cada 17 lutas |
| Raridade sorteada | Comum 62 / Incomum 28 / Raro 9 / Épico 1 |
| Grupo sorteado | itens "Drop: <este inimigo>" do catálogo (peso 3 cada) + itens de loja da cidade do bioma, Comum a Raro (peso 1 cada) |
| Sem item na raridade sorteada | cai um nível de raridade até achar |

**Chefe** (Lobo Alfa, Guardião da Ruína, Senhor da Guerra da Montanha, Habitante Ancestral; a vitória no desafio do chefe):

| Passo | Valor |
|---|---|
| **Primeira vitória** | o troféu-placa garantido: "O Lobo Alfa voltou pra mata" `casa_trofeu_001`, "O Guardião descansa" `casa_trofeu_007`, "Obrigado, crianças" `casa_trofeu_013`, "Dá pra dividir" `casa_trofeu_019`. É a "lembrança" que a N5 pede. |
| Toda vitória | 1 rolagem garantida + 25% de uma segunda |
| Raridade | Incomum 40 / Raro 37 / Épico 19 / Lendário 4 |
| Proteção de azar | 25 vitórias seguidas sem Lendário daquele chefe → a próxima garante um |
| **[online]** chefe mundial | os 50% que mais contribuíram ganham uma rolagem cada (Fase 12.3, loot por contribuição) |

Quantos itens exclusivos cada inimigo tem, por raridade, está no fim do `CatalogoCasa.md`. Os chefes concentram os Épicos e Lendários. O Habitante Ancestral, último chefe, tem o maior grupo (17 itens, 3 Lendários).

**Entrega:** o item vai para o **Depósito da casa** como "encomenda", com um texto dourado "+ Encomenda: Coleira de couro do Lobo Alfa" e um aviso. Ele **nunca** concorre com a mochila e nunca some (mesma filosofia do `LootGrantRules`, Balance §9.2). O evento novo `HouseItemGrantedEvent` alimenta o toast e as conquistas.

**Tom infantil:** os chefes não morrem; eles são libertados da sombra (texto da história). Por isso os troféus são lembranças, placas, estátuas em paz e **cabeças entalhadas em madeira**, no estilo das carrancas, e nunca bicho empalhado (§11.10).

### 7.3 Missões

**Missões da Casa** (cadeia nova da Vó Zefa, **[proposto]**):

| # | Missão | Quando | Objetivo | Recompensa |
|---|---|---|---|---|
| 1 | Um Cantinho Seu | 1ª volta à vila | conversar com a Vó Zefa e entrar na casa | Casa N1 + baú, guarda-roupa, cama de vento, casinha do Caramelo, livro de visitas |
| 2 | Água Fresca | depois da 1 | 10 pedra + 5 madeira | `casa_quintal_001` Poço de pedra com sarilho |
| 3 | Fogo na Cozinha | depois do Lobo Alfa | 3 carne assada + 8 madeira | `casa_cozinha_001` Fogão a lenha de tijolo + `casa_cozinha_002` Filtro de barro da Vó Zefa |
| 4 | Parede de Barro | subir para a N3 | a subida | `casa_quarto_006` Colcha de retalhos da Vó Zefa + Suéter de tricô da Vó Zefa (roupa) |
| 5 | Tijolo por Tijolo | subir para a N4 | a subida | `casa_familia_002` Rede onde a Vó Zefa embalou os cinco netos + `casa_util_005` Baú-correio |
| 6 | Casa Cheia | subir para a N5 + receber 4 visitas que assinem o livro (offline: os 4 primos NPC) | — | `casa_familia_001` Retrato de casamento da Vó Zefa e do Vô Chico + Medalhinha de latão da Vó Zefa (acessório) |

**Missões regionais que já existem** (`RegionQuestCatalog`): proposta de acrescentar 1 item de casa a cada recompensa atual, sem tirar nada.

| Missão | Recompensa hoje | Item de casa a mais |
|---|---|---|
| Uivos na Trilha (`quest_region_forest_howling_trail`) | 2 poção de vida | `casa_regional_011` Placa da trilha da Floresta |
| Teias no Roçado (`quest_region_forest_spider_webs`) | capacete de couro | `casa_regional_005` Tora com orelha-de-pau |
| Ferrões no Caminho (`quest_region_desert_stinger_road`) | 2 vida + 1 mana | `casa_regional_022` Placa "Cuidado com os ferrões" |
| Reserva pra Travessia (`quest_region_desert_crossing_reserve`) | botas de couro | `casa_regional_018` Cantil de couro do tropeiro |
| Pedras que Andam (`quest_region_mountain_walking_stones`) | 2 mana + 15 ouro | `casa_regional_025` Picaretas cruzadas do mineiro |
| A Passagem da Mina (`quest_region_mountain_mine_pass`) | espada de ferro | `casa_luz_026` Lampião de mina do Mestre Tonho |
| Lamparinas Apagadas (`quest_region_caves_dark_lamps`) | 2 vida + 2 mana | `casa_regional_040` Lamparina apagada reacesa |
| A Última Lamparina (`quest_region_caves_last_lantern`) | peitoral de couro | `casa_luz_013` Lamparina da Benedita |

**Missões pessoais e de convergência:** cada missão pessoal dá a cama-tema do primo (por exemplo, a Cama da Noite Rubra na da Lavine) e mais 1 ou 2 itens Épicos de família. "A Reunião" dá a Cadeira de balanço da Vó Zefa e a Mochila remendada da Maithe.

**Diárias** (`DailyQuestService`, 3 por dia): cada diária completa dá uma **"Caixinha da Vó"**, um item de casa aleatório da cidade da diária (Comum 80 / Incomum 18 / Raro 2). Quatro diárias também têm um item fixo na primeira vez, por exemplo "Um Prato Quente" → Livro de receitas escrito à mão da Vó Zefa.

### 7.4 Conquistas

São 24 itens de casa, um para cada uma das 24 conquistas que já existem em `Resources/Achievements`. Exemplos: "Caderno da Vó, de Capa a Capa" → Estante do Caderno da Vó; "Gigante Tombado" → Pedestal dos Gigantes Tombados; "Bom de Negócio" → Cofrinho de porquinho; "Acender de Novo" (Nova Jornada+) → Pote de rapadura que nunca esvazia. Item de conquista é **preso à conta**: não se vende nem se troca.

### 7.5 Eventos

| Evento | Itens de casa | Como se ganha |
|---|---|---|
| Festa Junina | 18 + 1 Lendário | prendas ganhas na quadrilha (minijogo de ritmo do TODO), na pescaria e nas diárias juninas, gastas na **Barraca de Prendas** |
| Natal Sertanejo | 14 + 1 Lendário | prendas do "Cerco ao Engenho" e do pastoril |
| Carnaval | 13 + 1 Lendário | prendas do bloco "Primos na Folia" |

O Lendário de cada evento não se compra com prendas: vem de completar todas as tarefas do evento, ou do ranking do evento **[online]**.

### 7.6 Crafting

O crafting usa a Bancada, a Forja e o Fogão com os **materiais reais dos biomas** (Balance §8.2: madeira, fibra, erva, pedra, minério, lingote). Receitas de móveis propostas:

| Receita | Insumos | Bancada |
|---|---|---|
| Banquinho torto feito à mão | 4 madeira | qualquer (é a da conquista "Feito à Mão") |
| Fogueira de quintal com toras | 6 madeira + 2 pedra | Bancada |
| Cisterna de placas | 20 pedra + 4 lingote | Bancada |
| Lareira de pedra da serra | 30 pedra + 10 madeira + 1 lembrança do Senhor da Guerra | Bancada de mestre |
| Chuveiro solar de serpentina | 3 lingote + 4 fibra | Bancada |
| Forno de torrar farinha | 6 lingote + 10 pedra | Forja |
| Tapete de raízes entrelaçadas | 12 fibra + 6 madeira | Bancada |

Toda receita de móvel conta para "Mestre Artesão".

### 7.7 Mascate (estoque rotativo) **[proposto]**

- **Seu Bento, o tropeiro** (NPC da missão do Deserto) vira o Mascate depois de "Reserva pra Travessia". Ele **viaja entre as 4 cidades**: fica 2 dias de jogo em cada uma, na ordem Vila → Areia Branca → Serra Alta → Lamparinas.
- Estoque de 6 itens por parada: 3 do grupo dele ("Loja: Mascate Seu Bento" no catálogo), 2 Raros sorteados de qualquer loja e, com 50% de chance, 1 Épico. Cada item tem 1 unidade por jogador.
- O sorteio usa semente pela data, como o `DailyQuestService` faz hoje. **[online]** É o mesmo estoque para o servidor inteiro, com limite por conta.

---

## 8. Lojas, NPCs e moedas

Hoje existe **uma** loja: `shop_general_store` (Armazém Geral). Ela vende pelo `ItemDefinition.Value`, ou pelo `_priceOverride`, e recompra a `_sellBackRate` 0,5. As lojas novas são `ShopDefinition` iguais, com dois campos novos: rotação e trava de progresso (§12.1).

### 8.1 Lojas novas

| Loja (NPC) | Cidade | Vende | Rotação de estoque | Libera |
|---|---|---|---|---|
| **Armazém Geral** (já existe) + filiais | todas | poções, comida, ferramentas; itens de casa básicos ("Loja: Armazém Geral"); cada filial vende 4 itens Comuns ou Incomuns do conjunto regional da sua cidade (Mata Fechada, Duna Dourada, Pico Nevado, Cristal das Cavernas) | fixo, com estoque que se repõe todo dia | já aberto |
| **Marcenaria do Mestre Damião** **[proposto: NPC novo]** | Vila da Vó Zefa | móveis de madeira (sala, quarto, baús, guarda-roupas, bancada) | 10 itens por dia, sorteados por data; 1 Épico por semana | Comum já; Incomum depois do Lobo Alfa; Raro depois do Guardião; Épico depois do Senhor da Guerra |
| **Armarinho da Dona Cida** | Vila da Vó Zefa | linhas, fitas, quadros, almofadas, crochê, tintas básicas, livro de visitas; coisas de rezadeira (sabonetes de arruda, mesa de alquimia) | 8 por dia | Dona Cida já é a rezadeira da missão da Floresta; o armarinho abre depois de "Teias no Roçado" |
| **Mercearia do Seu Lázaro** **[proposto]** | Vila da Vó Zefa | ágata, potes, cozinha, comida de enfeite, tintas de cozinha (açafrão, cebola) | 8 por dia | já aberto |
| **Costureira Dona Fátima** **[proposto]** | Vila da Vó Zefa | roupas Comuns, Incomuns e Raras de tecido, tapetes, cortinas, colchas, redes | 6 roupas + 6 de casa por dia; roupas por faixa de nível | Raro depois do Guardião |
| **Viveiro da Comadre Neném** **[proposto]** | Vila da Vó Zefa | plantas, árvores de quintal, sementes, abelhas, regador | 8 por dia; flores mudam com o clima (`WeatherSystem`) | já aberto |
| **Casa de Música do Seu Bené** **[proposto]** | Vila da Vó Zefa | instrumentos | fixo, com 1 instrumento Épico em rodízio semanal | depois do Lobo Alfa |
| **Feira livre** | uma cidade por dia: Vila (sáb/dom), Areia Branca (qua), Serra Alta (qui), Lamparinas (sex) | itens "Loja: Feira livre" + 4 itens locais da cidade do dia + tintas naturais | todo dia muda | já aberta |
| **Olaria da Dona Rosa** **[proposto]** | Areia Branca | barro: talha, moringa, filtro, pratos, azulejos, ladrilho | 8 por dia | Areia Branca aberta |
| **Mascate Seu Bento** | itinerante (§7.7) | raros e curiosidades | 6 por parada | depois de "Reserva pra Travessia" |
| **Forja do Mestre Tonho** | Serra Alta | ferragens, lampiões de ferro, baús reforçados, armaduras e roupas de metal (Joaquim e Ícaro), forja de quintal | 8 por dia | Mestre Tonho abre a forja depois de "A Passagem da Mina" |
| **Lamparinas da Benedita** | Lamparinas | iluminação, cristais, cortinas de contas, tintas escuras | 8 por dia | depois de "Lamparinas Apagadas" |
| **Barraca de Prendas** | Vila da Vó Zefa, só durante evento | itens sazonais por prendas | por evento | evento ativo |
| **Casa de Leilões** **[online]** | Vila (com um quadro em cada cidade) | itens que os jogadores põem à venda | — | nível 10 |

Estoque diário: Comum 5 unidades, Incomum 3, Raro 1. A rotação usa o mesmo relógio injetável das diárias (`IDailyClock`), para dar para testar.

### 8.2 Moedas

- **Ouro** (`item_gold`): a moeda de tudo. Não pesa e empilha até 9999 (Balance §9.1).
- **Moeda de ouro** (`item_gold_coin`, drop do Deserto): continua item vendável a 1 de valor. **[proposto]** O Mascate troca 2 moedas de ouro por 1 ouro.
- **Prendas** **[proposto]**: moeda de evento.
  - Ganha **só jogando** o evento.
  - Não se troca, não se vende e **não se compra com dinheiro real**.
  - Fica de um ano para o outro.
  - Não pesa.

### 8.3 Sem pay-to-win

Este design **não tem moeda premium**. Se o usuário quiser uma loja paga só de cosméticos (Fase 12.1, modelo de negócio), a regra proposta é:

- nenhum item de casa ou roupa é exclusivo da loja paga: tudo também se ganha jogando;
- nada dá conforto acima do equivalente que se ganha jogando;
- nada dá atributo.

---

## 9. Equipamento e aparência

### 9.1 Slots

Os slots de verdade são os do `EquipmentSlot`: `Head`, `Chest`, `Legs`, `Boots`, `MainHand`, `OffHand`, `Tool` e `Accessory`. Todos dão atributo.

Três camadas novas são **só de aparência**: **Rosto/Pescoço** (lenço, óculos, máscara), **Costas** (bornal, mochila, asas, capa) e **Caramelo** (roupinha do cachorro).

### 9.2 Conjuntos por primo

Cada primo tem **6 conjuntos de 4 peças** (Cabeça, Peito, Pernas, Pés), um por raridade, mais um festivo. São 120 peças, listadas no `CatalogoRoupas.md`.

| Raridade | Nível | De onde vem |
|---|---|---|
| Comum | 1–10 | Costureira / Armazém |
| Incomum | 10–20 | Costureira ou Forja + drop da Floresta/Deserto + 2ª missão da Floresta |
| Raro | 20–30 | loja + **chefe do primo** (Maithe → Lobo Alfa; Joaquim e Ícaro → Guardião da Ruína; Maria → Senhor da Guerra; Lavine → Habitante Ancestral) + diária do primo |
| Épico | 30–40 | missão pessoal (cabeça) + drop de chefe + crafting na Forja/Tear com material de chefe |
| Lendário | 40–50 | conquista do herói + Habitante Ancestral + Tear de mestre + Nova Jornada+ |
| Festivo | qualquer | evento (Junina, Natal, Carnaval), cosmético puro |

- Os nomes seguem quem cada um é. Maithe vai de "Batedora da Mata" até "Tempestade de Mil Flechas". O Joaquim vai de "Guri da Roça" até "Última Resistência", e o Ícaro, de "Aprendiz de Oficina" até "Exército Mecânico" (com "Botas a jato de rapadura").
- Os conjuntos usam os nomes das habilidades de assinatura (GDD).
- **Bônus de conjunto:** 2 e 4 peças **reais** equipadas. Só stats que já funcionam: vida máx., vigor, atributo principal, dano de habilidade, recarga. A tabela está no fim do catálogo.

### 9.3 Aparência separada dos atributos (guarda-roupa)

- **Ganhar uma peça desbloqueia a aparência dela para sempre** na **Coleção** da conta, mesmo que depois a peça seja vendida. Isso vale para loot, compra, missão e crafting.
- No guarda-roupa, cada slot escolhe "usar a aparência de…" qualquer peça desbloqueada **do mesmo slot**. Os atributos continuam os da peça equipada.
- Os conjuntos dos primos servem só para o primo dono, porque a modelagem é do corpo dele. As roupas comuns servem para todos.
- **Itens procedurais** (`ProceduralItemGenerator`, com afixos): o nome e os afixos continuam procedurais. A aparência padrão é a peça do conjunto do primo que tem o mesmo slot e a mesma raridade. Por exemplo, um "Peitoral de Couro" Raro numa Maithe aparece como "Túnica de linho verde bordada de vento". Assim, o loot de hoje já ganha visual sem nenhum item novo.
- Trocar de aparência é **grátis**, e é possível salvar trajes (3, ou 6 com o guarda-roupa de jacarandá).
- "Esconder chapéu" é uma opção por slot.

### 9.4 Tintas

- Cada peça tem 2 canais de cor (principal e detalhe). **Uma tinta pinta um canal e é gasta.**
- O "sabão de coco" volta à cor original e é grátis.
- As tintas também pintam porta, janela e barrado da casa (§3.2).
- São 25 tintas: anil, urucum, jenipapo, pau-brasil… até as Lendárias "Lua Rubra" e "Sol nascente".

### 9.5 Visuais de arma

- São 5 por primo (25 no total): arco da Maithe, cajado da Maria, escudo do Joaquim, ferramenta/lançador do Ícaro e grimório/orbe da Lavine.
- Só aparência, aplicada sobre a arma equipada do mesmo tipo.

### 9.6 Nota de arte

Os heróis são modelos únicos (os `*_RO`, vindos do Meshy). Trocar o corpo inteiro exige malha modular por peça, o que é caro. Proposta em duas etapas:

1. **v1:** troca de cor e de textura por raridade, mais **peças presas a ossos** (chapéu na cabeça, capa e mochila na coluna, lenço no pescoço, escudo na mão).
2. **v2:** malhas modulares de tronco, pernas e pés por conjunto, quando houver orçamento de arte (Fase 11).

### 9.7 De onde vem cada coisa

Está no catálogo, coluna "Origem". Resumo das 234 entradas:

| Raridade | Entradas |
|---|---|
| Comum | 63 |
| Incomum | 54 |
| Raro | 61 |
| Épico | 29 |
| Lendário | 27 |

---

## 10. Economia

### 10.1 Quanto ouro entra hoje (dados de `Docs/Balance/README.md`)

- **Drop de ouro:** inimigo comum solta `item_gold` 1–3 com 60%, ou seja, 1,2 por morte (§8.2). Com 1,5 inimigo por luta, dá **~1,8 ouro por luta** só de ouro.
- **Venda de material** a 50% (`_sellBackRate` 0,5) do `_value`: madeira 2, fibra 1, erva 3, carne assada 6, minério 5, ferro 8, pedra 1, poção 15, moeda de ouro 1.
  - Floresta: 1,30 madeira + 1,04 fibra + 1,04 erva + 0,29 carne por luta ≈ **+4,2 ouro por luta**.
  - Deserto: 0,90 minério + 0,45 fibra + 0,12 ferro + 0,18 pedra + 1,26 moeda + 0,18 poção ≈ **+4,8 ouro por luta**.
- **Chefes:** 4–8 (Lobo Alfa), 6–10 (Guardião), 8–14 (Senhor da Guerra), 10–16 (Habitante).
- **Missões:** "Pedras que Andam" e "Reserva pra Travessia" (que pede juntar 12) giram em torno de 15 ouro.
- **Renda de referência: ~6 ouro por luta**, vendendo tudo; ~2 se o jogador guarda os materiais.
- **Preços de hoje para comparar:** poção 15, peitoral de couro 20, machado/picareta 18, espada de ferro 45.

### 10.2 Faixas de preço por raridade

| Raridade | Preço de loja (ouro) | Lutas equivalentes (~6/luta) | Prendas (evento) | Recompra pela loja |
|---|---|---|---|---|
| Comum | 8–30 | 1–5 | 5–10 | 25% |
| Incomum | 35–90 | 6–15 | 15–25 | 25% |
| Raro | 100–250 | 17–42 | 30–50 | 25% |
| Épico | 300–700 | 50–117 | 60–90 | 25% |
| Lendário | não se vende | — | não se vende | não se vende |

- Item de casa e roupa volta para a loja a **25%** do preço de referência (o meio da faixa, quando o item não tem preço de loja), não aos 50% do Armazém. É "móvel usado", e isso evita farmar drop para virar ouro.
- Item de conquista, de missão da casa e troféu de chefe **não se vende**.
- Roupa e arma (§9) usam as faixas atuais de equipamento: Comum 10–25, Incomum 30–70, Raro 90–200.
- Tintas custam 5–50 por unidade.
- Os preços estão em "lutas equivalentes" de propósito: quando o servidor rebalancear o ouro do online (Fase 12), basta multiplicar a coluna de ouro pelo mesmo fator.

### 10.3 Subidas da casa

| Subida | Ouro | Lutas equivalentes |
|---|---|---|
| N1 → N2 | 150 | ~25 |
| N2 → N3 | 400 | ~65 |
| N3 → N4 | 1.000 | ~165 |
| N4 → N5 | 2.500 | ~415 |
| **Total** | **4.050** | |

O casarão é meta longa, no espírito MU Online. Como as subidas também pedem material, ainda puxam a coleta nos biomas.

### 10.4 Drenos de ouro (contra a inflação)

| Dreno | Valor |
|---|---|
| Subidas da casa | 4.050 ao todo |
| Tintas | consumíveis |
| Expansão do Baú da casa | +10 espaços por 200 / 400 / 800, no máximo 3 vezes |
| Mudança de cidade **[online]** | 100 × nível da casa |
| Plaquinha com o nome gravado | 20 |
| Casa de Leilões **[online]** | 5% para anunciar (devolvido se vender) + 5% sobre a venda |
| Conta da lenha semanal **[online] [proposto]** | §11.1 |
| Recompra baixa | 25% |

---

## 11. Ideias inspiradas em MMOs clássicos **[proposto]**

São mecânicas comuns em MMOs de casa do tipo que o usuário citou como inspiração. Aqui elas ganham regras próprias, ambientação sertaneja e tom infantil: nenhum nome, texto ou arte de outro jogo. Todas são **[proposto]**. A coluna "Precisa de online?" diz quais pressupõem multiplayer.

| # | Ideia | Como funcionaria no Rapadura | Precisa de online? |
|---|---|---|---|
| 11.1 | **Conta da lenha** (taxa semanal) | Toda semana sai automaticamente do ouro guardado (Baú da Vó Zefa/banco): N1 0, N2 5, N3 10, N4 20, N5 40 ouro. Se faltar ouro por 3 semanas, a casa fica **"fechada para descanso"**: ninguém entra, e os itens vão com segurança para os "Guardados da casa", uma seção sem limite do Baú da Vó Zefa. **Nada se perde, nada é rebaixado.** Pagou, um botão "Desembrulhar tudo" põe cada item de volta no lugar. O jogo avisa 1 semana antes (carta da Vó Zefa). | Sim (offline, desligado) |
| 11.2 | **Lista de convidados por porta** | Cada porta interna (quarto, oficina, sala de troféus) pode ter lista própria, que restringe mais que a da casa: ex.: "meu quarto só para a Maithe". A porta trancada aparece com um cadeado de lata e o nome da lista. | Sim |
| 11.3 | **Co-dono** | O dono nomeia 1 co-dono (2 no casarão), por exemplo irmãos que dividem a casa. O co-dono convida, expulsa, decora e usa as bancadas. Ele **não** sobe de nível, não muda de cidade, não tira o dono e não abre o Baú da casa: tem um **baú próprio** de 20 espaços dentro da casa. Sair da sociedade não leva nenhum item do dono. | Sim |
| 11.4 | **Móvel em kit (caixa de mudança)** | Todo móvel de loja vem numa caixa de papelão com o desenho do móvel. Desembrulhar dentro de casa põe o móvel em modo de colocação. **Embrulhar de novo** devolve a caixa ao Depósito, para levar na mudança ou para outra sala. A caixa é o item que aparece no correio e no leilão. É basicamente a apresentação do Depósito (§2.4). | Não (mas é o que permite troca no online) |
| 11.5 | **Cama: dormir é sair com segurança** | Deitar na cama da própria casa → "Dormir até amanhã?" = logout seguro **[online]** ou salvar e sair **[offline]**, e carrega o "Descansado" (§6.1). A **cama de casal** (`casa_quarto_001`) deixa o co-dono dormir também, cada um com o próprio descanso. | Parcial (o descanso vale offline; o logout seguro é do online) |
| 11.6 | **Caixa de correio** | Cartas e encomendas entre jogadores: carta de texto (frases prontas + texto filtrado) e encomenda de até 5 itens negociáveis ou ouro. A entrega leva "um dia de carroça" (1 hora real), o que freia troca suspeita. Item de conquista não vai pelo correio. A Vó Zefa também manda cartas (avisos do jogo). | Sim (offline: só cartas da Vó Zefa) |
| 11.7 | **Ajudante da casa** | Um parente da vila ("primo de segundo grau" ou "comadre ajudante") fica no balcão (`casa_util_031`). Ele guarda até 10 itens negociáveis e **vende pelo preço que o dono marcar** para visitantes enquanto o dono está offline, com taxa de 5%. **Riscos para a economia:** vira mercado paralelo à Casa de Leilões, facilita lavagem de ouro de bots e pode esvaziar as lojas de NPC. **Freios:** preço mínimo = 25% de referência e máximo = 3× referência; 10 vendas por dia; só para quem tem acesso à casa; tudo registrado. Alternativa mais simples: o ajudante só põe os itens no leilão por você. | Sim |
| 11.8 | **Boneco de treino de palha** | Treino de "postura" no quintal: o primo pratica golpes num boneco, sem dano de verdade (a cidade é zona segura, então não é combate). Dá XP lento: **10% da XP de uma luta comum do bioma por minuto, com teto de 30 minutos por dia**. Não dá loot nem ouro. **[online]** só conta com o jogador online e mexendo de vez em quando (contra bot parado). | Não |
| 11.9 | **Plantas que pedem água** | Plantas e canteiros têm um "humor": felizes até 3 dias sem água, depois **murcham** (ficam caídas, perdem metade do conforto). **Nunca morrem nem somem.** Uma regada com o regador (água do poço) volta tudo na hora. A chuva do `WeatherSystem` rega o quintal sozinha. Canteiros regados dão colheita (erva, ingredientes da cozinha). | Não |
| 11.10 | **Troféus de chefe na parede** | Em vez de bicho empalhado, **cabeças entalhadas em madeira** no estilo carranca, mais estátuas em paz e placas. Combina com a história (os chefes são libertados) e com o tom infantil. São 4 cabeças, uma por chefe, Épicas, no catálogo. | Não |
| 11.11 | **Casa em cada cidade + sede do clã** | Terrenos em todas as 4 cidades, com 1 casa principal por conta (§1) **[proposto]**. **Sede do clã:** um casarão especial por clã, com nível próprio, baú do clã por cargo, mural do brasão, sala de reunião e troféus de guerra de clãs. Cargos do clã substituem a lista de convidados. | Sim |
| 11.12 | **Passar ou vender casa** | Só online, só pela Casa de Leilões, e **só o terreno com a casa vazia**: os itens ficam com o dono antigo, no Depósito. Salvaguardas: conta com mais de 30 dias e nível 20; confirmação dupla; 3 dias de espera antes de concluir (dá para desistir); preço mínimo = custo das subidas × 50%; 1 venda por conta a cada 60 dias. | Sim |
| 11.13 | **Quadro de terrenos** | Na praça de cada cidade, um quadro de madeira mostra os terrenos: livre (placa "Vende-se"), ocupado (nome do dono e nível da casa) ou em leilão. Tocar num terreno ocupado mostra a fachada e um botão "Pedir para visitar" (manda um pedido; não entra direto). | Sim |

---

## 12. Modelo de dados (rascunho)

### 12.1 Definições (ScriptableObjects)

`HouseItemDefinition` (em `Resources/House/Items/casa_*.asset`, gerado de um CSV por um `HouseItemSeeder`, como os outros seeders do Editor):

| Campo | Tipo | Nota |
|---|---|---|
| `_itemId` | string | `casa_sala_001` |
| `_nameKey` / `_descKey` | string | `house.casa_sala_001.name` / `.desc` (en + pt-BR, padrão do `LocalizationManager`) |
| `_category` | `HouseItemCategory` | 18 valores, iguais às seções do catálogo |
| `_rarity` | `ItemRarity` | o enum que já existe |
| `_placement` | `HousePlacement` (flags) | Floor, Wall, Ceiling, Surface, Yard, Rug |
| `_footprint` | Vector2Int | em células; `_wallRows` para parede |
| `_rotationStep` | int | 0 (não gira), 45 ou 90 |
| `_surfaceSlots` | int | vagas que o móvel oferece por cima |
| `_function` | `HouseItemFunction` | None, Chest, Wardrobe, Bench, Stove, Forge, Loom, Alchemy, DyeTub, Bed, PetBed, TrophyStand, RelicCase, BestiaryShelf, GuestBook, Mailbox, TrainingDummy, Well, Planter, Helper, Bell, NoticeBoard |
| `_functionParam` | int | ex.: espaços a mais do baú |
| `_comfort` | int | |
| `_setId` | string | `set_varanda_vo_zefa` |
| `_basePrice` | int | 0 = não se vende em loja |
| `_eventPrice` | int | em prendas |
| `_sellable`, `_tradeable`, `_accountBound` | bool | conquista → preso à conta |
| `_minHouseLevel` | int | itens grandes (chafariz, carro de boi) pedem N3+ |
| `_sources` | `HouseItemSource[]` | {tipo: Shop/Drop/Quest/Achievement/Event/Craft/Secret, refId, peso} |
| `_seasonal` | `SeasonId` | None/Junina/Natal/Carnaval |
| `_prefabKey`, `_icon` | string / Sprite | prefab com material de paleta |

- **`HouseLevelDefinition`:** nível, módulos do casco por socket, grades interna e de quintal, limites, espaços do baú, visitantes máximos, custo (ouro + `RegionRewardSpec`-like de materiais), requisitos (nível do personagem, chefe derrotado via `RegionBossProgress`).
- **`HouseSetDefinition`:** id, nome, limiares {3, 6, todas}, conforto por limiar, efeito de ambiente, mimo.
- **`AppearanceDefinition`:** lookId, slot, primo (ou todos), raridade, malha/anexo, canais de tinta, origem.
- **`DyeDefinition`:** id, cor, raridade.
- **`ShopDefinition`** (já existe) + campos novos:
  - `_cityId`;
  - `_rotation` (Fixed/Daily/Weekly/Itinerant);
  - `_slotsPerRotation`;
  - `_stockByRarity`;
  - `_unlockRequirement` (chefe/missão);
  - `_houseSellBackRate` (0,25).

### 12.2 Save local **[offline]**

As chaves seguem o padrão do `SaveManager` ("VillageChest", "WorldMap"…):

- **`"House"`** → `HouseSaveState`:
  - `level`;
  - `cityId`;
  - `placed[]` {`instanceId`, `itemId`, `room`, `layer`, `cell`, `rotation`, `parentInstanceId` (para Superfície), `dyes[]`};
  - `depot` {`itemId` → quantidade};
  - `chest` (`InventorySlotData[]`, como o `VillageChestSaveState`) + `extraChests`;
  - `guestBook[]` {quem, frase, data};
  - `restCharges`, `restFightsLeft`, `lastRestDay`;
  - `plants[]` {`instanceId`, `lastWatered`};
  - `upkeepPaidUntil`;
  - `houseQuestStep`.
- **`"Wardrobe"`** → `unlockedLooks[]`, `outfits[]` por primo {slot → lookId + 2 tintas}, `dyeInventory` (ou tintas como item comum da mochila).
- **Compatibilidade:** save antigo sem "House" → nasce a casa N1 se a Missão da Casa 1 já devia ter acontecido (vitórias na Floresta ≥ 3), mesmo padrão das migrações de hoje (relíquias, Caderno, Fogão).

### 12.3 Servidor **[online]**

| Tabela | Campos |
|---|---|
| `houses` | `house_id`, `account_id`, `city_id`, `plot_id`, `level`, `upkeep_paid_until`, `status` (ativa/fechada para descanso/em leilão) |
| `house_items` | `house_id`, `instance_id`, `item_id`, `layer`, `cell_x`, `cell_y`, `rotation`, `parent`, `dyes`, `placed_by`, `placed_at` |
| `house_depot` | `house_id`, `item_id`, `qty` |
| `house_chest` | `house_id`, `slot`, `item`, `qty`, `affixes` |
| `house_access` | `house_id`, `player_id`, `level` (visit/decorate/bench/coowner), `door_id` (nulo = casa toda), `expires_at` |
| `house_guestbook` | `house_id`, `player_id`, `phrase_id`, `text`, `created_at` |
| `house_audit` | `house_id`, `actor_id`, `action`, `payload`, `at` |
| `house_snapshots` | visita com decorador |
| `wardrobe_unlocks` | `account_id`, `look_id` |
| `outfits` | `character_id`, `slot`, `look_id`, `dye1`, `dye2` |

Regras:

- Toda escrita passa por validação no servidor (encaixe, limites, permissão).
- O drop de item de casa é decidido **só no servidor**, como o resto do loot (Fase 12.2, anti-cheat).
- Os logs de economia incluem compra, venda, leilão, correio e ajudante.

---

## 13. Plano de implementação (em fases)

| Fase | Entrega | Testes |
|---|---|---|
| **H0 — Dados** | `HouseItemDefinition` + enums; `HouseItemSeeder` lendo um CSV gerado do catálogo; textos en/pt-BR; `ShopDefinition` com cidade e rotação | IDs e nomes únicos, raridade válida, toda origem aponta para algo que existe (inimigo, missão, conquista), preço dentro da faixa da raridade |
| **H1 — Casa N1 offline** | Cena ou área de instância da casa na Vila; `HouseMeshBuilder` com o kit N1; modo "Arrumar a casa" (piso/parede/teto/superfície/quintal) com `GridPlacementUtility`; Depósito; Baú da casa; cama; Missão da Casa 1; botão "Minha casa" no mapa | encaixe, rotação, sobreposição, *flood fill* das portas, limites, save/load do `"House"` e migração de save antigo |
| **H2 — Cidades e lojas** | Cidades como zona segura (sem encontro e sem dano); 11 lojas com NPC e rotação diária; filiais do Armazém; Feira itinerante; Mascate | zona segura bloqueia dano/encontro; rotação determinística por data; travas de progresso |
| **H3 — Fontes** | Rolagem de item de casa no `EnemyLootRoller` (grupo por inimigo + loja do bioma; chefes com placa garantida e proteção de azar); `HouseItemGrantedEvent` + toast; itens nas missões regionais/pessoais/diárias; conquistas; receitas de móveis | chances com RNG injetada, entrega no Depósito sem tocar na mochila, garantia de 1ª vitória, contador de azar |
| **H4 — Subidas N2–N5** | Kits de módulos N2–N5; custos e requisitos; cenas curtas da família; Missões da Casa 2–6 | nada se perde na subida; requisitos de chefe/nível |
| **H5 — Guarda-roupa** | Coleção de aparências; camadas Rosto/Costas/Caramelo; aparência padrão dos itens procedurais; tintas; visuais de arma; trajes salvos; v1 de arte (cor + anexos em osso) | aparência não muda stat; desbloqueio permanente; tinta gasta |
| **H6 — Vida na casa** | Conforto + "Descansado"; conjuntos e efeitos; parede de troféus; vitrine de relíquias; estante do Caderno; Caramelo em casa; livro de visitas; visitas de NPC; plantas e regador; boneco de treino | cálculo do conforto (limite de 2 cópias), faixas, recarga diária, teto do boneco |
| **H7 — Eventos** | Prendas; Barraca de Prendas; enfeites sazonais com guardar/voltar | evento abre e fecha pelo relógio; item fica após o evento |
| **H8 — Online** | Casas instanciadas; convites, lista, permissões, expulsar, snapshot/desfazer, *log*; co-dono; listas por porta; correio; Casa de Leilões; ajudante (se aprovado); conta da lenha; quadro de terrenos; sede do clã; venda de casa | matriz de permissões (visitante nunca abre baú), validação no servidor, concorrência de edição, carga com N casas |

---

## 14. Decisões pendentes (para o usuário)

1. **Escada de níveis:** madeira → madeira com varanda → taipa e telha → alvenaria → casarão está bom? E os custos de 150 / 400 / 1.000 / 2.500 ouro?
2. **Casa por conta ou por personagem** (online)? A proposta é **por conta**: os personagens da conta moram juntos, cada um com o próprio guarda-roupa.
3. **Baú da Vó Zefa no online:** vira o banco da conta (compartilhado entre os personagens), e o Baú da casa fica da casa? É a proposta.
4. **Lista da casa permanente:** amigo da lista entra **com o dono offline**, ou só com o dono online?
5. **Bônus de conforto:** só XP descansado (proposta) ou também algo como regeneração fora de combate?
6. **Conta da lenha** (§11.1): ligar no online? E os valores?
7. **Ajudante que vende offline** (§11.7): ligar, ligar só como "põe no leilão por mim", ou não ligar?
8. **Vender casa entre jogadores** (§11.12): permitir?
9. **Nomes das cidades novas:** Povoado de Areia Branca, Vila da Serra Alta, Vila das Lamparinas.
10. **NPCs novos:** Mestre Damião, Seu Lázaro, Dona Fátima, Comadre Neném, Seu Bené e Dona Rosa. Reaproveitar os NPCs que já existem (Dona Cida no Armarinho, Seu Bento como Mascate, Mestre Tonho na Forja, Benedita nas Lamparinas) está ok?
11. **Moeda de evento (prendas)** e **loja paga só de cosméticos** (Fase 12.1).
12. **Arte das roupas:** começar pela v1 (cor + anexos em osso) e deixar malhas modulares para depois?
13. **Boneco de treino:** dar XP lenta (proposta, com teto) ou ser só enfeite/animação?
