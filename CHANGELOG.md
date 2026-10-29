# Changelog

Todas as mudanças relevantes do Rapadura são registradas aqui.

O formato segue o [Keep a Changelog 1.1.0](https://keepachangelog.com/pt-BR/1.1.0/) e o projeto adota [Versionamento Semântico 2.0.0](https://semver.org/lang/pt-BR/):

- **MAJOR** (`X.0.0`) — quebra de compatibilidade de save ou mudança de escopo que exige reinstalação/migração.
- **MINOR** (`0.X.0`) — novos sistemas, conteúdo ou telas, mantendo saves compatíveis.
- **PATCH** (`0.0.X`) — correções de bug e ajustes de balanceamento/performance.
- Enquanto o jogo estiver em `0.x`, qualquer MINOR pode mudar mecânicas; `1.0.0` = primeiro lançamento público nas lojas.

**Fonte da verdade da versão:** `PlayerSettings.bundleVersion` (exposto em runtime como `Application.version` e lido por `Rapadura.Core.Versioning.BuildVersion`). O valor atual `1.0` em `ProjectSettings/ProjectSettings.asset` é o placeholder padrão da Unity — a versão pretendida é **0.1.0**, e `BuildVersion.Current` já reporta `0.1.0` enquanto o placeholder não for trocado no Editor. Ao gerar uma build de release: atualizar `bundleVersion` (e `Android bundleVersionCode` / `iOS buildNumber`, sempre incrementais), mover os itens de `[Não lançado]` para a nova seção com data e criar a tag git `vX.Y.Z`.

## [Não lançado]

### Adicionado
- Fluxo completo do jogo: menu principal → seleção de herói → mapa mundi ⇄ encontro, com menu, configurações, créditos e seleção na cena `WorldMap` (`GameFlowService`, `GameFrontEndController`).
- Acerto crítico (1.5x) em ataque básico e skills (`OutgoingDamage`, `PlayerCombatBalance`).
- Ficha do personagem para gastar pontos de atributo (botão com selo de pontos na HUD, atalho C).
- HUD do mapa mundi (herói, região, relógio, clima, rótulos das regiões, menu) e chefe de região: a próxima região só abre depois de derrotá-lo.
- Localização dos textos fixos da UI (`UiDocumentLocalizer`, 90 chaves) e do conteúdo (itens, skills, inimigos, heróis; 181 entradas en/pt-BR), nomes de itens gerados compostos no idioma atual e idioma escolhido salvo.
- Mochila: arrastar e soltar, botão Organizar; navegação por gamepad/teclado em 6 telas (`FocusNavigator`).
- Voltar ao título (com confirmação) com reset completo da sessão (`NewSessionReset`).
- Arena de encontro decorada por bioma (cactos, estalagmites, rochas com neve) e VFX de ambiente por bioma, fase do dia e clima.
- Tutorial de primeira vez: 9 dicas por evento, salvas por perfil, com "Mostrar dicas" e "Reiniciar tutorial" na aba Acessibilidade.
- Áudio: 44 sons procedurais por evento e música por contexto (título, mapa, bioma, chefe), como placeholders até os assets reais (ver `Docs/Audio.md`); vibração no celular.
- Controles por toque: tocar e segurar para andar, joystick virtual opcional (Configurações > Controles), botão Atacar, botões de girar câmera e botão Voltar do Android abrindo a pausa.
- `FrameRateManager` + `FrameRatePolicy` (`Core/Performance`): economia de bateria/temperatura — no mobile 30 fps em menus, mapa mundi e pausa (pausa renderiza a cada 2 frames via `OnDemandRendering`), 60 fps em combate (opção "60 FPS em combate" persistida em PlayerPrefs, `Performance.HighFrameRateInCombat`); no desktop, vSync ligado.
- `BuildVersion` (`Core/Versioning`): helper SemVer em runtime sobre `Application.version`.
- Tela de Créditos (UI Toolkit, `CreditsView.uxml/.uss` + `CreditsController`), aberta pelo botão "Créditos" do menu principal.
- `Docs/ThirdPartyLicenses.md`, `Docs/QAChecklist.md`, `Docs/Audio.md` e seção "Orçamento de performance" em `Docs/PlatformRequirements.md`.
- 14 smoke tests PlayMode (cenas, telas de UI, encontro e XP no encontro); suíte EditMode com 1372 testes.
- CI completo (`.github/workflows/ci.yml`): `precheck` da licença Unity (jobs ficam pulados sem os secrets), testes EditMode + PlayMode com cache e LFS, builds de desenvolvimento Windows x64 e Android em push/manual, assinatura Android opcional; configuração em `Docs/CI.md`.
- Overlay de debug (só Editor/Development build): FPS, frame time, GC por frame, memória, inimigos, fluxo, pausa, bioma/clima; F1 ou toque com 3 dedos (`Core/Debug/DebugOverlay*`).
- Contagem "Prepare-se/Já!" no início do encontro: inimigos nascem em arco a 6-9 m e seguram o ataque por 1.75s (`EncounterSpawnLayout`).
- Skills miram o inimigo mais próximo num cone de 70° ou no alcance (`SkillTargeting`).
- Defesa funcional para herói e inimigos: % de skills reduz/aumenta o dano (teto 80% / 2x) e defesa plana pela curva de armadura; Velocidade como multiplicador [0.1, 2.5] no movimento do herói e dos inimigos (fórmulas em `Docs/Balance/README.md` §4).
- Herói corre sozinho ao perseguir um alvo distante; encontros forçam o aggro dos inimigos depois da contagem.

- Vento: árvores, tufos de grama e cactos balançam com rajadas que atravessam o campo (`Environment/Wind`: `WindSystem` + `WindSway`, força pelo clima — brisa no céu limpo, forte na chuva/neve, máxima na tempestade de areia — e direção que vira devagar). Puro transform, sem shader nem rig; anexado automaticamente na carga da cena por `EnvironmentPropAttacher` (regras de nome em `EnvironmentPropRules`).
- Pedras reagem a impactos no chão (`Environment/Impact`: `GroundImpactSystem` + `GroundImpactShaker`): skills de Terra com área (Terremoto, Muralha de Pedra) chacoalham as rochas ao redor e fazem pedrinhas/gravetos pularem; pisada pesada de golem/senhor da guerra e queda de chefe também.
- Clipes de mocap de ações (ataque espada/soco alternados, conjurar, reação a dano, pulo, morte para frente/trás) ligados aos heróis Humanoid: camada "Actions" com máscara de tronco (pernas continuam andando) e estados Jump/Death na base, montados por `Rapadura/Animation/Rebuild Hero Animators` (`HeroAnimatorRebuilder.Actions.cs`), disparados por `PlayerActionAnimator` a partir dos eventos de jogo. Cada clipe é acelerado para que o pico (mão mais rápida) caia em 0,22 s (`HeroActionTiming`).
- Ataque básico com preparo: o dano cai 0,22 s depois do input, no quadro do golpe (`PlayerBasicAttack.StrikeDelay`, usa o `AttackStrikeTime` gravado no controller quando há clipe); a virada durante o preparo é respeitada e um golpe em voo some se o herói morrer.

- Log rolante no overlay de debug (Editor e Development Build): `GameLogger.MessageLogged` alimenta um buffer circular de 50 entradas (`LogRingBuffer`/`DebugLogHistory`, também com exceções não tratadas); o overlay mostra as 12 mais recentes e F2 liga/desliga só o log.

- Campanha com começo e fim: prólogo dos cinco primos no Novo jogo (com um painel do herói escolhido), interlúdio depois de cada chefe de região e epílogo ao vencer o Habitante Ancestral, seguido dos créditos e da volta ao mapa para o pós-jogo (`Gameplay/Story`, `UI/Story/StoryPanelController`; chave de save "Story"; saves antigos não repetem o que já ficou para trás). Conquista "A Luz Volta para Casa".
- Missões regionais no mapa mundi: botão "Conversar com…" (com "(!)") abre a personagem da região — Dona Cida (Floresta), Seu Bento (Deserto), Mestre Tonho (Montanha) e Benedita das Lamparinas (Cavernas) — com 8 missões secundárias encadeadas, saudação própria para cada primo, fala de aceite especial, dica antes do chefe e agradecimento quando a região é limpa (`RegionQuestCatalog`, `RegionNpcDialogues`, `RegionQuestTexts`).
- Atordoamento: Correntes das Trevas (1 s), Terremoto (0,8 s) e Granada Elétrica (0,6 s) atordoam; o inimigo para, não ataca e perde o golpe que preparava; chefes levam metade (`StunRules`); faíscas azuis + "Atordoado!".
- `StatType.AttackSpeed` (recarga do ataque básico `0,5 s / (1 + bônus)`, sem começar um golpe antes do anterior acertar) e `StatType.Thorns` (devolve até 50% do dano corpo a corpo tomado, sem recursão), com 4 afixos novos: Ágil, do Frenesi, Espinhosa e do Mandacaru (16 → 20). Fórmulas em `Docs/Balance/README.md` §7.
- Botão "Carregar" no menu principal (abre Salvar/Carregar em modo carregamento; desabilitado sem save manual) e navegação por controle/teclado na tela de Salvar/Carregar.
- Movimento com peso e personalidade (`HeroMotionProfile` por herói: Joaquim pesado, Maria calma, Maithe leve, Ícaro inquieto, Lavine nervosa): idle que respira, troca o apoio e olha em volta; tronco contra-rotacionando e braços opostos às pernas; antecipação do ataque terminando no frame do dano, follow-through e recuperação; pulo com agachamento visual, esticada no ar e squash na aterrissagem pela velocidade da queda; freada antes do pivô em curvas fechadas; empurrão do golpe para longe do atacante; molas de overlap em cabeça, tronco e braços. IK de pé nos heróis Humanoid (`PlayerFootIK`: pés seguem o chão, presos ao parar/girar, passos de reposição). Animator ganhou `TurnAngle`, `IsGrounded`, `VerticalSpeed` e os estados Jump/Fall/Land. Guia em `Docs/Animation.md`.
- Loot de encontro por bioma (`EnemyLootRoller`): madeira, fibra e erva na Floresta, minério e lingote na Montanha, e os materiais do Deserto e das Cavernas; 2 rolagens por inimigo comum e 4 garantidas por chefe, que também solta um item bônus Raro/Épico. Texto "+2 Madeira" sobre o corpo. Taxas em `Docs/Balance/README.md` §8.
- Vila da Vó Zefa no mapa mundi: botão "Vila" na Floresta com os moradores de história pessoal de cada primo, o ponto de encontro, a Loja e a Bancada (antes inalcançáveis na `Main.unity`); compras e itens criados no mapa chegam ao próximo encontro e ao save (`CarriedInventorySession`).
- Tela de título e seleção de heróis com identidade própria (fim de tarde no engenho, logo dourado, botões temáticos, entrada suave; heróis com cor, inicial, papel, idade, laço de família e as 5 habilidades; preview 3D que nunca abre vazio). Tema compartilhado em `UI/Common/RapaduraTheme.uss`.
- Mapa mundi como ilha low-poly gerada por semente (`WorldMapDressing`): chão e props por região, marcos (vilinha, ruína, acampamento no pico, boca da caverna), estradas pelo grafo de viagem, rio com ponte, praia, mar e nuvens; neblina em região bloqueada e bandeira em região limpa.
- Inimigos com peso e personalidade (`EnemyMotionProfile` por arquétipo, escalado pelo tamanho/massa): ease ao arrancar/parar, inclinação e freada nas curvas, antecipação com squash e golpe esticado no frame do dano (windup e janela de esquiva inalterados), empurrão de mola ao apanhar, quique na morte, overshoot ao surgir e idle com personalidade.
- Opção de acessibilidade "Reduzir Movimento": metade do squash/overlap/balanço de heróis e inimigos, metade do tremor de câmera e entradas instantâneas no título, história, chefe e tutorial. "Reduzir Flashes" deixa de alterar a animação dos heróis.
### Alterado
- "Continuar" carrega o save mais recente entre o auto-save e os slots 0–2 (pela data gravada; ignora slot ausente ou corrompido), em vez de só auto-save/slot 0.
- `PlayerProceduralAnimator`: com clipes reais de ataque/magia/morte, a camada procedural para de girar os braços e de tombar o corpo (evitando golpe duplo) e mantém só inclinação, olhar, recuo, flash e afundar.
- `SkillManager` só dispara o trigger por skill (`CastQuake` etc.) se o controller o declarar — antes cada conjuração logava "Parameter does not exist".
- Rebalanceamento por dados: curva de XP 1.25→1.10, +8 HP por nível, XP por bioma 10/18/28/40, chefes x8, ultimates ~2.5x e skills fracas reforçadas (tabelas em `Docs/Balance/README.md`).
- Pausa única com contagem de donos (`GameplayPause`) em todas as telas modais e no menu de pausa.
- Estoque da loja salvo por save e reposto ao amanhecer; conquistas em arquivo de perfil próprio.
- Música procedural gerada em thread separada (sem engasgo), com cache de 2 clipes; `StopMusic` esmaece em vez de cortar.
- Tela de morte só reconstrói o texto da contagem regressiva quando o segundo exibido muda (antes alocava uma string por frame).
- Anel do clique-para-mover usa `sharedMaterial` (antes `.material` clonava o material criado em runtime no primeiro pulso).
- Build de 358 MB para 192 MB sem perda visual: malhas com compressão Medium (High em árvores) e não legíveis, animação Optimal, texturas de personagens/inimigos 1024 com crunch e de ambiente 512, override Android ASTC, base color das árvores extraída do glTF para PNG 512 com mipmaps.
- `EnemyPool` passa a usar `GenericObjectPool`.
- Chuva, neve e areia desenhadas como riscos finos que seguem o jogador.
- Subir de nível recupera vida e mana; fundos de telas modais opacos; textos do mapa refletem o chefe de região.
- Configurações com aba ativa sublinhada e nomes traduzidos; Mochila separa ouro de itens; herói selecionado destacado.
- `UiPointerQuery` com cache de `UIDocument` (zero alocação por consulta); APIs obsoletas trocadas (`style.scale`, `FindObjectsByType`).
- Rebalanceamento r6/r7 (playtests automatizados 5 heróis × 4 biomas): comuns com cerca de 2x vida, chefes 430/505/580/700 HP com ataques mais lentos e pesados, dano do herói +2% por nível, Purificação 24/5, Muralha 0.5 / 16 s, Lobo Alfa 18→14, Escorpião 9→11, Cura Divina 6→9 s. Lutas comuns de 4.3-7.4 s e de chefe de 15-22 s (`Docs/Balance/README.md` §5–§6).
- Inimigo foge no máximo 3s a 0.8x da corrida do herói e volta.
- Visual dos biomas: Montanha nevada, Caverna escura, noite legível, tempestade de areia visível, Lobo Alfa avermelhado; banners, card de missão e pulso do rastreador opacos sobre areia/neve.
- Objetos de teste da `Main` num root `Sandbox` desligado em encontros; props procedurais começam a 11 m do centro.
- Dica do tutorial espera o "Prepare-se/Já!"; trocar de herói revoga as skills do anterior.

- Removido `EnemyDefinition._contactDamage` (semeado mas nunca usado): todo ataque de inimigo já é corpo a corpo e telegrafado; dano por encostar dobraria o DPS e puniria o tocar-para-andar.

### Corrigido
- Missões concluídas no mapa mundi (sem Player na cena) perdiam a recompensa; agora ela fica na fila (salva) e é entregue no próximo encontro (`QuestManager.DeliverPendingRewards`).
- `.meta` de UXML/USS escrito à mão com o guid do próprio asset no importer fazia a tela sumir em silêncio; guardado por `AssetMetaIntegrityTests`.
- Carregar um save pelo menu de pausa no mapa mundi deixava região, token e visuais da sessão anterior (Explorar e o desafio de chefe usavam e salvavam a região errada); o `WorldMapController` agora ressincroniza no `GameLoadedEvent`.
- Vencer um encontro e morrer nos 1,5 s antes da volta ao mapa fazia a tela de morte concluir como Derrota, perdendo a vitória (e, no chefe, a liberação da próxima região); a vitória decidida pela arena prevalece (`SceneTransitionService.DecideOutcome`).
- Matar inimigo no fluxo real (mapa → encontro) não dava XP, loot, números de dano nem VFX: o `GameManager` duplicado era destruído junto com os componentes de recompensa (`SingletonBehaviour` agora remove só o componente duplicado).
- Vitalidade, Espírito, Destreza e o afixo `affix_vital` não tinham efeito; buffs de fontes diferentes (ou sem fonte) no mesmo atributo se substituíam em vez de somar.
- Nível, inventário e skills do jogador se perdiam a cada encontro.
- Carregar um save antigo mantinha herói, chefes e relógio da sessão anterior.
- O estoque da loja alterava o asset da loja.
- Menu de pausa devolvia a velocidade normal por cima de outra tela aberta.
- Clicar em UI atacava; teclas 1-5 lançavam skill com menu aberto.
- Bônus de equipamento (base + afixo, fixo + percentual) se anulavam.
- Hit-stops sobrepostos deixavam o jogo em câmera lenta permanente.
- Cura grátis ao trocar de cena; quest rastreada vazava entre saves; `Load` não era à prova de exceção.
- Menu principal e seleção de herói ficavam sem música; a música do mapa continuava depois de voltar ao título.
- Tocar para andar falhava se o dedo demorasse mais de 0.2s ou se mexesse 5px; pinça virava tap-to-move; Correr ficava preso depois de gesto do sistema.
- Preview da seleção de herói (heróis rigados apareciam deitados), ordem de camadas das telas e textos ilegíveis nas configurações e barras da HUD.
- Inimigos de encontro nasciam 1 m abaixo do chão (invisíveis): o pivô fica no centro da cápsula; NPCs da vila apareciam como cápsulas durante encontros; inimigos nasciam uns sobre os outros.
- Inimigo reutilizado do pool voltava invisível/sem colisão; o afundar da morte desativava um inimigo já re-spawnado; trocar o prefab destruía inimigos vivos.
- Inimigos nasciam a 1 m e tiravam 34 HP em 2s; inimigo ganhava golpe grátis depois de qualquer pausa.
- Skills de área não davam dano em colliders de objetos filhos.
- Toque: Correr/joystick bloqueavam o tocar-para-andar; joystick e Correr ficavam presos ao trocar de app; Esc pausava sem estar ligado ao menu de pausa.
- Rótulos do mapa sobrepostos; dica de loot disparava ao carregar save; dica do tutorial vazava sobre a tela de morte e modais.
- Pedestal visível no preview do Joaquim.
- Skills lançadas logo depois do ataque básico não causavam dano (cerca de 40% eram engolidas pela invulnerabilidade do inimigo, agora por atacante + canal); DoT agora credita quem aplicou (XP/loot).
- Inimigos não tinham `BuffController`: debuffs, lentidão e dano contínuo de skills eram ignorados. `AttackDamage` de inimigo, regen de vida/mana/stamina e `MaxStamina` de afixos não eram lidos.
- Defesa e Velocidade (`Defense`/`MoveSpeed`) não tinham efeito.
- Morcegos nunca atacavam: o alcance 3D não fechava porque pairam acima do herói (alcance agora horizontal, com tolerância vertical de 2 m).
- XP ganho logo depois de entrar na `Main`, antes da restauração do estado carregado, era perdido.
- Herói flutuava na Montanha (gravidade no idle e snap ao chão no spawn).
- Luz direcional da prévia de heróis iluminava a arena inteira (noite e caverna claras); virou spot.
- Bancada de teste abria o crafting e paredes de teste travavam a luta durante encontros; rochas nasciam sobre o spawn do golem.

## [0.1.0] - 2026-10-21

Primeira versão rastreada: protótipo jogável (vertical slice de sistemas) em Unity 6 (6000.3) + URP 17, mobile-first (Android/iOS) com PC como plataforma adicional.

### Adicionado
- **Núcleo:** `GameManager` com `ServiceLocator`/`IManager`, `EventBus` tipado, `StateMachine`, `GenericObjectPool`, `GameLogger`, overlay de debug (FPS/memória).
- **Personagens:** os 5 heróis jogáveis — Maithe, Maria, Joaquim, Ícaro e Lavine — com modelos 3D estilo chibi (Meshy), rig humanoide e animações Idle/Walk/Run para Maria, Joaquim, Ícaro e Lavine (Meshy Rigging API); tela de seleção de personagem com preview; troca de personagem jogável; passivas e conjuntos de skills por herói.
- **Movimento e câmera:** controle por teclado/gamepad/toque, clique/toque-para-mover com indicador no chão, botões de Correr e Pular na HUD, câmera isométrica fixa (estilo MU Online) com zoom por pinça e colisão.
- **Combate em tempo real:** ataque básico, clicar-no-inimigo-ataca, hitbox/hurtbox, knockback, armas corpo a corpo e à distância com projéteis, durabilidade, elementos e resistências, controle de grupo, números de dano flutuantes, shake de câmera, VFX de combate.
- **Skills:** 25 habilidades de assinatura (5 por herói), árvores de skill por herói, combos, buffs/debuffs e DoT (Queimadura), respec, hotbar com teclas 1–5, gamepad e botões de toque.
- **Inimigos:** IA por máquina de estados (patrulha, perseguição, ataque, fuga), lobo/escorpião/aranha, spawner com pooling e ondas, bônus de chefe, loot e XP ao morrer.
- **Progressão:** atributos e level-up, curva de XP, regeneração de vida/mana/stamina.
- **Itens:** inventário com peso e durabilidade, gerador procedural de itens com 16 afixos, tela Mochila, tooltips (mouse, toque longo e foco).
- **Crafting e construção:** `CraftingManager` com receitas de exemplo e painel de crafting; `BuildingManager`.
- **Mundo:** mapa mundi com regiões/biomas desbloqueáveis, encontros aleatórios e transição para a cena de combate, checkpoints e ponto de spawn inicial.
- **Narrativa:** sistema de diálogo com escolhas, NPCs interagíveis, quests (matar, coletar, conversar) com recompensas, log de missões e rastreador na tela, conteúdo narrativo dos 5 heróis e diálogos de convergência da família.
- **Economia:** lojas com compra/venda.
- **Save:** save versionado em slots + auto-save, menu de salvar/carregar.
- **UI (UI Toolkit):** HUD com barras e safe area (notch), menu principal, pausa, configurações (gráficos, áudio, idioma), tela de morte com fade e contagem regressiva.
- **Áudio:** `AudioManager` com categorias, volume persistido, crossfade/ducking e pool de fontes.
- **Localização:** `LocalizationManager` (pt-BR/en) com tabelas CSV.
- **Acessibilidade:** escala de fonte, alto contraste, paletas para daltonismo, intensidade de shake, redução de flashes, remapeamento de controles (backend).
- **Analytics:** `AnalyticsManager` local baseado em eventos.
- **Ambiente:** árvores Meshy decimadas com LOD, rochas procedurais, textura de chão procedural com normal map, dispersão de detalhes.
- **Ferramentas de editor:** seeders de dados (itens, skills, inimigos, quests, diálogos, receitas, lojas, biomas), editores de itens/skills, bootstrapper da cena principal, decimador de malha.
- **Qualidade:** suíte de testes EditMode (Unity Test Framework) e CI no GitHub Actions.

### Alterado
- Pipeline migrado de Built-in para URP; color space de Gamma para Linear.
- UI escala com a tela (`ScaleWithScreenSize`) em vez de tamanho físico constante.

### Corrigido
- Dezenas de bugs encontrados durante a integração na cena (ver histórico do git e `TODO.md`), entre eles: recompensas de quest nunca entregues, objetivos de matar/conversar inalcançáveis, afixos sem efeito, morte do jogador causando soft-lock, inimigos mortos bloqueando passagem, XP não concedida, telas de UI sem `<Style>` no UXML, sistema de skills ausente da cena e race conditions de resolução de managers no `OnEnable`.

[Não lançado]: https://github.com/Emersonsanch/rapadura/compare/v0.1.0...HEAD
[0.1.0]: https://github.com/Emersonsanch/rapadura/releases/tag/v0.1.0
