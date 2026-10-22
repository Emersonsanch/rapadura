# Rapadura

RPG de ação em desenvolvimento na Unity, com 5 personagens jogáveis, combate em tempo real, sistema de skills/skill tree, inventário e crafting/construção. Mobile é plataforma obrigatória (Android/iOS), com PC como plataforma adicional.

## Documentação

- [`Docs/GDD.md`](Docs/GDD.md) — Game Design Document
- [`Docs/PlatformRequirements.md`](Docs/PlatformRequirements.md) — requisitos mínimos de hardware, público-alvo e classificação indicativa
- [`Docs/ProjectConventions.md`](Docs/ProjectConventions.md) — estratégia de branches, naming e estrutura de pastas
- [`Docs/EditorSetupGuide.md`](Docs/EditorSetupGuide.md) — setup local no Unity Editor
- [`Docs/Balance/`](Docs/Balance/README.md) — curva de progressão, inimigos e skills em CSV (`progression.csv`, `enemies.csv`, `skills.csv`; fonte única do balanceamento, com tabelas antes/depois do rebalanceamento)
- [`Docs/Localization.md`](Docs/Localization.md) — inventário e plano de localização (pt-BR/en)
- [`Docs/Audio.md`](Docs/Audio.md) — cues de som, música por contexto, vibração e como trocar os placeholders procedurais por áudio real
- [`Docs/QAChecklist.md`](Docs/QAChecklist.md) — checklist de regressão antes de cada release
- [`Docs/ThirdPartyLicenses.md`](Docs/ThirdPartyLicenses.md) — licenças de assets e pacotes de terceiros
- [`CHANGELOG.md`](CHANGELOG.md) — histórico de versões (SemVer)
- [`CONTRIBUTING.md`](CONTRIBUTING.md) — como contribuir
- [`TODO.md`](TODO.md) — roadmap de desenvolvimento, atualizado a cada sessão

## Arquitetura

Namespaces `Rapadura.*`, organizados em:

- `Assets/Scripts/Core/` — infraestrutura (EventBus, StateMachine, ServiceLocator/DI, managers, logging, localização, áudio, acessibilidade, analytics, pooling)
- `Assets/Scripts/Gameplay/` — player, câmera, combate, inimigos, skills, itens, inventário, crafting, construção, diálogo, quests, personagens jogáveis
- `Assets/Scripts/UI/` — telas em UI Toolkit (HUD, menus, skill tree, diálogo)
- `Assets/Scripts/Editor/` — ferramentas de editor (seeders, editor windows)
- `Assets/Tests/EditMode/` — testes automatizados EditMode (Unity Test Framework)
- `Assets/Tests/PlayMode/` — smoke tests PlayMode (assembly `Rapadura.Tests.PlayMode`)

Managers seguem o padrão `IManager` registrado via `ServiceLocator` a partir de `Core/Managers/GameManager.cs`; comunicação entre sistemas usa o `EventBus` (`Core/EventBus`).

## Estado atual (2026-10-22)

O projeto roda no Unity Editor (6000.3) com duas cenas no Build Settings: `WorldMap.unity` (0) e `Main.unity` (1). Fluxo de jogo: menu principal → seleção de herói → mapa mundi ⇄ encontro aleatório → combate em tempo real na arena → volta ao mapa.

Já funciona:

- **Fluxo completo**: menu principal → seleção de herói → mapa ⇄ encontro → volta ao mapa, com pausa, voltar ao título (reset de sessão) e tutorial de primeira vez.
- **5 heróis jogáveis**, cada um com 5 skills próprias (25 no total), árvore de skills por herói e hotbar (teclas 1-5, gamepad ou toque na HUD). Ficha do personagem para gastar pontos de atributo.
- **Mapa mundi** com 4 regiões (Floresta, Deserto, Montanha, Cavernas), HUD própria, encontros aleatórios, arena decorada por bioma e chefe de região que desbloqueia a próxima região.
- **Combate**: ataque básico, clique no inimigo para atacar, acerto crítico, buffs/debuffs, XP, loot com raridade e afixos, bônus de chefe, tela de morte com respawn. Balanceamento por dados em `Docs/Balance`.
- **RPG**: Mochila (inventário + equipamentos) com arrastar e soltar, Organizar e tooltips; crafting com bancada, loja com estoque salvo, diálogos ramificados com NPCs que dão quests, log de missões + rastreador, missões diárias, conquistas.
- **Mundo**: ciclo dia/noite, clima por bioma, VFX de skills e de ambiente.
- **Áudio**: sons por evento e música por contexto (placeholders procedurais, ver `Docs/Audio.md`) e vibração no celular.
- **Mobile**: tocar e segurar para andar, joystick virtual opcional, botões de atacar/correr/pular/girar câmera, botão Voltar do Android.
- **Save**: slots manuais + auto save, save do mundo (herói, construções, receitas, bosses, hora/clima, estoque da loja), formato v2 compatível com saves v1, escrita atômica com backup.
- **Configurações**: áudio, gráficos, idioma (pt-BR/en, toda a UI e o conteúdo traduzidos, idioma salvo), rebind de controles, navegação por gamepad/teclado, acessibilidade (escala de fonte, daltonismo, alto contraste, reduzir flashes/shake), limite de FPS para poupar bateria.
- **Build**: Windows x64 gerada sem erros, com cerca de 192 MB (era 358 MB, antes da compressão de malhas/texturas e do override Android ASTC). Build Android local depende do módulo Android Build Support no Unity Hub.
- **Debug**: overlay de FPS, memória, GC e estado do jogo (só Editor/Development build), F1 ou toque com 3 dedos.

Próximos itens de código: decimar malhas de heróis/inimigos, deduplicar malhas rigadas e limpas, Addressables, carga de ultimate e terreno real por bioma. Pendências que dependem de decisão do usuário (módulo Android, bundle id e ícone do app, rig da Maithe, secrets do CI, aparelho físico e builds de loja, licenças, áudio e ícones reais) estão em `TODO.md` → "Próximos passos".

## Testes

No Unity Editor: **Window → General → Test Runner**.

- **EditMode** (`Assets/Tests/EditMode`, 1300 testes): aba *EditMode* → *Run All*. Os testes de save usam uma pasta temporária isolada e não tocam nos saves reais.
- **PlayMode** (`Assets/Tests/PlayMode`, 14 smoke tests das cenas `Main`/`WorldMap`, das telas de UI e do encontro com XP): aba *PlayMode* → *Run All*. As cenas precisam estar no Build Settings (já estão).

Pela linha de comando (o Editor precisa estar fechado):

```sh
Unity -batchmode -projectPath . -runTests -testPlatform EditMode -testResults results-editmode.xml
Unity -batchmode -projectPath . -runTests -testPlatform PlayMode -testResults results-playmode.xml
```

O CI (`.github/workflows/ci.yml`, branch `master`) roda EditMode e PlayMode via game-ci e, em push/manual, gera builds Windows e Android. Sem os secrets da licença Unity os jobs ficam pulados; veja [`Docs/CI.md`](Docs/CI.md) para configurar.

## Requisitos

- Ver `Docs/PlatformRequirements.md` para versões-alvo de plataforma
- Unity 6 (6000.3.x)
- Git LFS (modelos e texturas já estão em LFS)
