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
- 14 smoke tests PlayMode (cenas, telas de UI, encontro e XP no encontro); suíte EditMode com 1300 testes.
- CI completo (`.github/workflows/ci.yml`): `precheck` da licença Unity (jobs ficam pulados sem os secrets), testes EditMode + PlayMode com cache e LFS, builds de desenvolvimento Windows x64 e Android em push/manual, assinatura Android opcional; configuração em `Docs/CI.md`.
- Overlay de debug (só Editor/Development build): FPS, frame time, GC por frame, memória, inimigos, fluxo, pausa, bioma/clima; F1 ou toque com 3 dedos (`Core/Debug/DebugOverlay*`).
- Contagem "Prepare-se/Já!" no início do encontro: inimigos nascem em arco a 6-9 m e seguram o ataque por 1.75s (`EncounterSpawnLayout`).
- Skills miram o inimigo mais próximo num cone de 70° ou no alcance (`SkillTargeting`).

### Alterado
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

### Corrigido
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
