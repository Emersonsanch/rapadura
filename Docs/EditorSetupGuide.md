# Guia de Setup no Unity Editor

Este guia é para quem vai abrir o projeto **Rapadura** pela primeira vez no Unity Editor. O projeto nunca foi aberto antes: não existe `Library/`, `ProjectSettings/ProjectVersion.txt`, `ProjectSettings.asset`, Scenes ou Prefabs. Existe apenas código C# (`Assets/Scripts`), ScriptableObjects gerados por seeders (menu items do Editor) e testes EditMode. Este documento leva esse estado até um Play Mode minimamente jogável.

---

## 1. Pré-requisitos

Leia `Packages/manifest.json` — os pacotes relevantes são:

| Pacote | Versão | Observação |
|---|---|---|
| `com.unity.render-pipelines.universal` (URP) | 17.0.3 | URP major 17 é a versão que acompanha o **Unity 6 (6000.0.x)** |
| `com.unity.inputsystem` | 1.11.2 | Novo Input System — o projeto usa exclusivamente `PlayerInput` |
| `com.unity.addressables` | 2.2.2 | |
| `com.unity.textmeshpro` | 3.0.9 | |
| `com.unity.ugui` | 2.0.0 | |
| `com.unity.modules.ai/animation/audio/physics/uielements` | 1.0.0 | módulos padrão |

Não há `com.unity.cinemachine` nem `com.unity.test-framework` explicitamente no manifest. O Test Framework normalmente já vem embutido nos templates do Unity 6 — se o Test Runner não aparecer, adicione-o pelo Package Manager (`Window > Package Manager > Unity Registry > Test Framework`).

**Instale via Unity Hub**: Unity 6000.0 LTS (a versão estável mais recente da série 6000.0). Instale os módulos de build para as plataformas alvo do projeto — o TODO.md indica que mobile é plataforma obrigatória, então inclua os módulos Android e/ou iOS Build Support.

---

## 2. Abrir o projeto

1. No Unity Hub, `Add` > selecione a pasta `D:\Projetos\rapadura`.
2. Ao abrir, escolha a versão 6000.0 LTS instalada.
3. A primeira importação vai **demorar bastante** (pode levar de 10 a 30+ minutos): o Editor precisa gerar `Library/`, importar todos os assets, e compilar todos os scripts pela primeira vez. Não feche o Editor durante esse processo.
4. Após a importação, abra o **Console** (`Window > General > Console`) e verifique se há **erros de compilação** antes de continuar. Erros comuns nesse ponto:
   - Referências de pacote faltando (verifique se URP, Input System e Addressables realmente resolveram no Package Manager).
   - Warnings sobre falta de um Render Pipeline Asset atribuído em `Graphics Settings` — normal em projeto recém-criado; será resolvido ao configurar URP (`Edit > Project Settings > Graphics`), mas não impede os passos seguintes.
5. Em `Edit > Project Settings > Player`, confirme/ajuste Company Name = `Engenho Estúdio` e Product Name = `Rapadura` (conforme `Docs/ProjectConventions.md`).

Se o Console estiver limpo (sem erros vermelhos), prossiga.

---

## 3. Rodar os testes automatizados

1. `Window > General > Test Runner`.
2. Selecione a aba **EditMode**.
3. Clique em **Run All**.

Isso roda todos os testes EditMode do projeto sem precisar de nenhuma Scene, Prefab ou GameObject montado manualmente — é a forma mais rápida de validar se a base de código compilou corretamente e se a lógica central (managers, stats, combate, etc.) ainda se comporta como esperado antes de investir tempo montando a Scene. Rode isso **antes** de qualquer outro passo abaixo; se houver falhas aqui, é sinal de problema na importação/compilação, não na montagem da Scene.

---

## 4. Rodar os seeders de conteúdo

Os seeders são menu items em `Rapadura > ...`, implementados em `Assets/Scripts/Editor/*.cs`. Eles criam ScriptableObjects em `Assets/Resources/...`. Execute-os **nesta ordem**, pois alguns dependem de dados criados por outros:

| Ordem | Menu Item | Arquivo | Cria | Depende de |
|---|---|---|---|---|
| 1 | `Rapadura > Seed Example Items` | `ItemDatabaseSeeder.cs` | 19 itens em `Assets/Resources/Items/` | — |
| 2 | `Rapadura > Seed Crafting Resources` | `ResourceItemSeeder.cs` | item_wood, item_stone, item_iron, item_gold em `Assets/Resources/Items/` | — |
| 3 | `Rapadura > Seed Example Skills` | `SkillDatabaseSeeder.cs` | 20 skills em `Assets/Resources/Skills/` | — |
| 4 | `Rapadura > Seed Example Dialogues` | `DialogueSeeder.cs` | 2 diálogos em `Assets/Resources/Dialogue/` | — |
| 5 | `Rapadura > Seed Example Shop` | `ShopSeeder.cs` | 1 loja em `Assets/Resources/Shops/` | passo 2 (item_wood/stone/iron/gold) |
| 6 | `Rapadura > Seed Example Biomes` | `BiomeSeeder.cs` | 4 biomas em `Assets/Resources/Biomes/` | passo 1 (referências de loot) |
| 7 | `Rapadura > Seed Example Enemies` | `EnemyDatabaseSeeder.cs` | 8 inimigos em `Assets/Resources/Enemies/`; também tenta popular as spawn tables dos biomas criados no passo 6 | passo 6 |

Depois do passo 7, se as spawn tables dos biomas não tiverem sido preenchidas automaticamente, rode `Rapadura > Seed Example Biomes` novamente para forçar a religação.

Há também duas janelas de edição (não são seeders, não criam nada sozinhas, úteis para balanceamento depois):
- `Rapadura > Skill Editor` (`SkillEditorWindow.cs`)
- `Rapadura > Item Editor` (`ItemEditorWindow.cs`)

Confirme em `Assets/Resources/` que as pastas `Items`, `Skills`, `Dialogue`, `Shops`, `Biomes`, `Enemies` foram populadas antes de seguir para o próximo passo.

---

## 5. Montar a Scene principal (`Main.unity`)

Crie uma nova Scene em `Assets/Scenes/Main.unity` (`File > New Scene`, depois `File > Save As`). Monte os seguintes GameObjects:

### 5.1 GameManager
- GameObject vazio, nome `GameManager`.
- Componente: `GameManager` (`Assets/Scripts/Core/Managers/GameManager.cs`).
- Não precisa de mais nada nele — é um singleton que constrói e registra no `ServiceLocator`: SaveManager, CheckpointManager, LocalizationManager, CraftingManager, BuildingManager, AudioManager, AccessibilitySettings, AnalyticsManager, DialogueManager, QuestManager, ShopManager.
- `InventoryManager` e `SkillManager` **não** são criados aqui — são componentes por-jogador (ver Player abaixo, se existirem como componentes separados no seu setup atual, adicione-os ao Player).

### 5.2 Player
GameObject nome `Player`, **tag `"Player"`** (obrigatório — `EnemyController` procura o alvo via `GameObject.FindGameObjectWithTag("Player")`). Componentes, nesta ordem lógica:

1. `CharacterController` (exigido por `PlayerMotor`).
2. `Animator` (referenciado por `PlayerController`; pode ficar sem Animator Controller por enquanto).
3. `PlayerInput` (exigido por `PlayerInputHandler`) — atribua um `.inputactions` asset com as actions **exatas**: `Move`, `Look`, `Jump`, `Run`, `Crouch`, `Interact`. Use/crie `Assets/InputActions/PlayerControls.inputactions` conforme `Docs/ProjectConventions.md`.
4. `PlayerInputHandler` (`Assets/Scripts/Gameplay/Player/PlayerInputHandler.cs`).
5. `PlayerMotor` (`Assets/Scripts/Gameplay/Player/PlayerMotor.cs`).
6. `PlayerStats` (`Assets/Scripts/Gameplay/Player/PlayerStats.cs`).
7. `PlayerController` (`Assets/Scripts/Gameplay/Player/PlayerController.cs`) — arraste o `Animator` e a `PlayerCamera` (ver 5.3) nos campos serializados, e o `_cameraLookTransform` se aplicável.
8. Opcional: `BuffController`, `AttributeSet` (`Assets/Scripts/Gameplay/Player/AttributeSet.cs`), `ElementResistance`.

### 5.3 Câmera do jogador
GameObject de câmera (pode ser o `Main Camera` da Scene), com:
- Componente `Camera`.
- Componente `PlayerCamera` (`Assets/Scripts/Gameplay/Player/PlayerCamera.cs`) — configure `_collisionMask`. O `_pivot` é setado automaticamente em runtime pelo `PlayerController.Start()` via `SetPivot(...)`, não precisa configurar manualmente.
- Arraste esta câmera no campo de `PlayerController` que referencia `PlayerCamera`.

### 5.4 UI (UIDocuments)

Para cada item da tabela, crie um GameObject com componente `UIDocument` + o controller correspondente (todos exigem `UIDocument` via `[RequireComponent]`) e atribua o `.uxml` em `UIDocument > Source Asset`. Configure também um `PanelSettings` compartilhado (crie um asset `Assets/Settings/UIPanelSettings.asset` via `Create > UI Toolkit > Panel Settings Asset` e reutilize em todos).

| GameObject sugerido | Source Asset (UXML) | Componente Controller | Root element |
|---|---|---|---|
| `HUD` | `Assets/UI/HUD/HudView.uxml` | `HudController` | `hud-root` |
| `MainMenu` | `Assets/UI/Menus/MainMenuView.uxml` | `MainMenuController` | `main-menu-root` |
| `PauseMenu` | `Assets/UI/Menus/PauseMenuView.uxml` | `PauseMenuController` | `pause-menu-root` |
| `SettingsMenu` | `Assets/UI/Menus/SettingsMenuView.uxml` | `SettingsMenuController` | `settings-menu-root` |
| `SaveLoadMenu` | `Assets/UI/Menus/SaveLoadMenuView.uxml` | `SaveLoadMenuController` | `save-load-menu-root` |
| `SkillTree` | `Assets/UI/SkillTree/SkillTreeView.uxml` | `SkillTreeController` | `skill-tree-root` |
| `Dialogue` | `Assets/UI/Dialogue/DialogueView.uxml` | `DialogueUIController` | `dialogue-root` |
| `Shop` | `Assets/UI/Shop/ShopView.uxml` | `ShopUIController` | `shop-root` |
| `Tooltip` | `Assets/UI/Common/TooltipView.uxml` | `TooltipController` | `tooltip-root` |

Namespaces reais: `Rapadura.UI.HUD`, `Rapadura.UI.Menus`, `Rapadura.UI.SkillTree`, `Rapadura.UI.Dialogue`, `Rapadura.UI.Shop`, `Rapadura.UI.Common`.

Depois de criar os GameObjects, faça a religação cruzada nos campos serializados:
- `HudController` precisa de uma referência a `_playerStats` → arraste o `PlayerStats` do Player.
- `PauseMenuController` precisa de referências ao `SettingsMenuController` e ao `SaveLoadMenuController` → arraste os GameObjects `SettingsMenu` e `SaveLoadMenu`.
- `MainMenuController` também referencia `SettingsMenuController` → arraste `SettingsMenu`.
- `SkillTreeController` precisa de `SkillManager`, `PlayerStats` e um array de `SkillTreeDefinition` (os assets gerados pelo seeder de skills, em `Assets/Resources/Skills/`).

Todos os controllers de menu (exceto o HUD) chamam `Hide()` no próprio `OnEnable`, então podem ficar ativos na Scene desde o início — eles se escondem sozinhos até algo chamar `Show()`.

O `DebugOverlayController` (`Assets/UI/Debug/DebugOverlayView.uxml`) é opcional — não há necessidade de colocá-lo na Scene principal agora; é documentado como fora de escopo até o momento.

---

## 6. Testar em Play Mode

1. Salve a Scene (`Ctrl+S`) e confirme que ela está em `Assets/Scenes/Main.unity` e adicionada em `File > Build Profiles > Scene List` (ou `Build Settings`), como Scene 0.
2. Entre em Play Mode e verifique, nesta ordem:
   - **Movimento**: o Player responde às actions Move/Jump/Run/Crouch sem erros no Console.
   - **Câmera**: a `PlayerCamera` segue o pivot do Player sem clipar através de paredes (se houver colisores no cenário de teste).
   - **HUD**: os valores de vida/stats no `HudController` atualizam quando `PlayerStats` muda (dano de teste, cura, etc.).
   - **Menu de pause**: a action de pause (verifique se está mapeada no `.inputactions`) abre o `PauseMenu` e ele some corretamente ao fechar.
3. Se algo não aparecer visualmente, confira o `PanelSettings` (Screen Match Mode, Sort Order) e se o `UIDocument` de cada elemento está habilitado.

---

## 7. Problemas conhecidos (revisar quando o Editor estiver disponível)

1. **Reflection em `EnemyController.ApplyDefinitionToHealth`** (`Assets/Scripts/Gameplay/Enemy/EnemyController.cs`, método por volta das linhas 164–191): usa `FieldInfo.SetValue` para escrever diretamente nos campos privados de `Health` (`_maxHealth`, `<CurrentHealth>k__BackingField`, `<IsDead>k__BackingField`), porque `Health` não expõe setter público para vida máxima. Falha silenciosamente (com log) se os nomes dos campos mudarem. Correção recomendada: adicionar um setter público em `Health.cs` (já anotado como pendência na Fase 2 do TODO.md).
2. **Audio Mixer lógico, não real**: `AudioManager.cs` simula categorias de volume multiplicando `AudioSource.volume` em C#, sem um asset real de Unity Audio Mixer com grupos/roteamento. Criar o `.mixer` asset é trabalho exclusivo do Editor, ainda pendente.
3. **Sem NavMesh**: `EnemyController` se move via `CharacterController` (ou translação direta de Transform quando não há `CharacterController`), por decisão de design documentada em comentário — funciona sem bake de NavMesh, mas é uma limitação de pathfinding a reavaliar conforme a complexidade dos cenários crescer.
4. **`PlayerStats` implementa `ICombatTarget` diretamente** em vez de usar `Health`: o jogador recebe dano de hitboxes mas não tem os benefícios de i-frames/hit-stop do sistema mais novo baseado em `Health`. Anotado como pendência na Fase 2 do TODO.md.
5. **Aba "Controles" do menu de Settings é placeholder**: existe `InputRebindManager.cs` com API real de rebind do Input System, mas ainda não está conectado à UI de `SettingsMenuController`.
6. **`DebugOverlayController`**: sem posicionamento automático em Scene/Prefab — colocação manual fica a critério de quem for depurar.

---

Depois de seguir os passos 1–6, o projeto deve compilar sem erros, os testes EditMode devem passar, o conteúdo básico (itens, skills, diálogos, loja, biomas, inimigos) deve existir como ScriptableObjects, e a Scene `Main.unity` deve permitir mover o personagem, ver o HUD reagir e abrir/fechar o menu de pause em Play Mode.
