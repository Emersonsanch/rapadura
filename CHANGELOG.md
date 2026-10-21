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
- `FrameRateManager` + `FrameRatePolicy` (`Core/Performance`): economia de bateria/temperatura — no mobile 30 fps em menus, mapa mundi e pausa (pausa renderiza a cada 2 frames via `OnDemandRendering`), 60 fps em combate (opção "60 FPS em combate" persistida em PlayerPrefs, `Performance.HighFrameRateInCombat`); no desktop, vSync ligado.
- `BuildVersion` (`Core/Versioning`): helper SemVer em runtime sobre `Application.version`.
- Tela de Créditos (UI Toolkit, `CreditsView.uxml/.uss` + `CreditsController`), aberta pelo botão "Créditos" do menu principal.
- `Docs/ThirdPartyLicenses.md`, `Docs/QAChecklist.md` e seção "Orçamento de performance" em `Docs/PlatformRequirements.md`.

### Modificado
- Tela de morte só reconstrói o texto da contagem regressiva quando o segundo exibido muda (antes alocava uma string por frame).
- Anel do clique-para-mover usa `sharedMaterial` (antes `.material` clonava o material criado em runtime no primeiro pulso).

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

### Modificado
- Pipeline migrado de Built-in para URP; color space de Gamma para Linear.
- UI escala com a tela (`ScaleWithScreenSize`) em vez de tamanho físico constante.

### Corrigido
- Dezenas de bugs encontrados durante a integração na cena (ver histórico do git e `TODO.md`), entre eles: recompensas de quest nunca entregues, objetivos de matar/conversar inalcançáveis, afixos sem efeito, morte do jogador causando soft-lock, inimigos mortos bloqueando passagem, XP não concedida, telas de UI sem `<Style>` no UXML, sistema de skills ausente da cena e race conditions de resolução de managers no `OnEnable`.

[Não lançado]: https://github.com/Emersonsanch/rapadura/compare/v0.1.0...HEAD
[0.1.0]: https://github.com/Emersonsanch/rapadura/releases/tag/v0.1.0
