# Rapadura

RPG de ação em desenvolvimento na Unity, com 5 personagens jogáveis, combate em tempo real, sistema de skills/skill tree, inventário e crafting/construção. Mobile é plataforma obrigatória (Android/iOS), com PC como plataforma adicional.

## Documentação

- [`Docs/GDD.md`](Docs/GDD.md) — Game Design Document
- [`Docs/PlatformRequirements.md`](Docs/PlatformRequirements.md) — requisitos mínimos de hardware, público-alvo e classificação indicativa
- [`Docs/ProjectConventions.md`](Docs/ProjectConventions.md) — estratégia de branches, naming e estrutura de pastas
- [`CONTRIBUTING.md`](CONTRIBUTING.md) — como contribuir
- [`TODO.md`](TODO.md) — roadmap de desenvolvimento, atualizado a cada sessão

## Arquitetura

Namespaces `Rapadura.*`, organizados em:

- `Assets/Scripts/Core/` — infraestrutura (EventBus, StateMachine, ServiceLocator/DI, managers, logging, localização, áudio, acessibilidade, analytics, pooling)
- `Assets/Scripts/Gameplay/` — player, câmera, combate, inimigos, skills, itens, inventário, crafting, construção, diálogo, quests, personagens jogáveis
- `Assets/Scripts/UI/` — telas em UI Toolkit (HUD, menus, skill tree, diálogo)
- `Assets/Scripts/Editor/` — ferramentas de editor (seeders, editor windows)
- `Assets/Tests/EditMode/` — testes automatizados (Unity Test Framework)

Managers seguem o padrão `IManager` registrado via `ServiceLocator` a partir de `Core/Managers/GameManager.cs`; comunicação entre sistemas usa o `EventBus` (`Core/EventBus`).

## Estado atual

⚠️ Este projeto ainda não foi aberto no Unity Editor — não há `Library/`, `ProjectSettings.asset` nem Scenes/Prefabs. Todo o código, dados (ScriptableObjects via seeders) e testes EditMode foram desenvolvidos por fora; abrir o projeto no Editor e montar a cena principal + prefab do player é o próximo passo manual necessário para rodar o jogo. Veja `TODO.md` para o detalhamento fase a fase do que está pronto, parcial ou bloqueado.

## Requisitos

- Unity (ver `Docs/PlatformRequirements.md` para versões-alvo de plataforma)
- Git LFS recomendado para binários (texturas, áudio, modelos) quando forem adicionados
