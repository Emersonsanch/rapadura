# Rapadura — Localização (inventário e política)

> Documento vivo. Auditoria de 2026-10-21. Idioma principal: **pt-BR**; suporte a **en**.
> Guardado por `Assets/Tests/EditMode/LocalizationCoverageTests.cs`. Rode a suíte EditMode
> depois de mexer em qualquer texto.

---

## 1. Como o texto chega na tela

`LocalizationManager` (`Assets/Scripts/Core/Localization/`) guarda um dicionário
`chave → (En, PtBR)`. `Get(key)` devolve o texto no idioma atual. Se faltar a tradução, cai
para o **inglês**. Se a chave nem existir, devolve `"[key]"` (fica visível de propósito).

As entradas vêm de duas camadas:

| Camada | Arquivo | Entradas | Quando é carregada |
|---|---|---|---|
| Tabela central | `Core/Localization/LocalizationManager.cs` → `DefaultEntries.All` | ~134 (`ui.pause/menu`, `dialogue.*`, `quest.*` dos 5 heróis + convergência, `shop.*`, `biome.*`, `affix.*`) | `Initialize()` |
| Helper | `UI/Quests/QuestLogTexts.cs` | 25 (`ui.quest.*`, `quest.*.journal`) | `QuestLogDataSource` (Quest Log/Tracker) |
| Helper | `Gameplay/Dialogue/QuestGiverTexts.cs` | 72 (`dialogue.*` dos NPCs que dão missão, `maria_gathering_point`) | `DialogueManager` ao iniciar um diálogo |
| Helper | `Gameplay/Quests/DailyQuestTexts.cs` | 36 (`quest.daily_*`) | `DailyQuestService.Initialize` |
| Helper | `UI/Achievements/AchievementTexts.cs` | 48 (`ui.achievement.*`, `achievement.<id>.*`) | Lista/toast de conquistas |
| Helper | `UI/Common/TooltipLocalization.cs` | 69 (`ui.tooltip.*`, `ui.inventory.*`) | `TooltipController.GetLocalizer` |
| Helper | `UI/Menus/SettingsMenuTexts.cs` | ~38 (`ui.settings.*`) | `SettingsMenuController` |

**Regra dos helpers:** ao carregar, cada helper só adiciona as chaves que ainda **não**
existem (`LoadEntries(x.Where(e => !HasKey(e.Key)))`). Um CSV/`LocalizationTable` externo
sempre ganha, e nenhum helper sobrescreve a tabela central. `TooltipLocalization` e
`SettingsMenuTexts` antes carregavam a lista inteira sem filtro, guardados só por uma chave
sentinela. Foram alinhados ao padrão nesta auditoria.

**Idioma inicial:** `GameManager.BuildManagers()` chama
`LocalizationManager.SetLanguage(LocalizationManager.ResolveStartupLanguage(Application.systemLanguage))`.
Um aparelho em inglês começa em `en`. Qualquer outro idioma (português, espanhol, desconhecido...)
começa em **pt-BR**. Um `new LocalizationManager()` sem `GameManager` (testes, ferramentas)
continua começando em `en`. A troca manual fica em Configurações → Idioma.

## 2. Inventário de fontes de texto

### 2.1 Localizadas (passam por `LocalizationManager`)

| Fonte | Campos | Onde estão as chaves |
|---|---|---|
| `QuestDefinition` (`Resources/Quests`, `Resources/Quests/Daily`) | `_titleKey`, `_descriptionKey`, `QuestObjective.DescriptionKey` | Tabela central + `DailyQuestTexts`; diário em `QuestLogTexts` |
| `DialogueDefinition` (`Resources/Dialogue`) | `DialogueNode.TextKey`, `SpeakerNameKey`, `DialogueChoice.TextKey` | Tabela central + `QuestGiverTexts` |
| `AchievementDefinition` (`Resources/Achievements`) | `TitleKey`/`DescriptionKey` (padrão `achievement.<id>.title/description`) | `AchievementTexts` |
| `ShopDefinition` (`Resources/Shops`) | `_displayNameKey` | Tabela central |
| `BiomeDefinition` (`Resources/Biomes`) | `_nameLocalizationKey` | Tabela central (adicionado nesta auditoria) |
| `ItemAffixDefinition` (`Resources/Affixes`) | `_nameLocalizationKey` (fallback: `_fallbackDisplayText` pt-BR) | Tabela central (adicionado nesta auditoria) |
| Tooltip / Inventário | enums `ItemRarity`, `ItemType`, `EquipmentSlot`, `StatType`, `ElementType` → `ui.tooltip.<grupo>.<snake_case>` | `TooltipLocalization` |
| Quest Log / Tracker, Conquistas, Configurações (rótulos criados em código) | constantes `*Key` | Helpers correspondentes |

### 2.2 Só pt-BR por enquanto (texto direto, sem chave)

| Fonte | Campos / arquivos | Observação |
|---|---|---|
| `ItemDefinition` (20) | `_displayName`, `_description` | O nome do item procedural junta `DisplayName` com o afixo, então o nome final é pt-BR mesmo em `en` |
| `SkillDefinition` (45), `SkillTreeDefinition` (6), `ComboDefinition` (2) | `_displayName`, `_description` | HUD, árvore de skills e tooltip mostram direto |
| `EnemyDefinition` (9) | `_enemyName` | |
| `RecipeDefinition` (3), `StructureDefinition` | `_displayName`, `_description` | |
| `BiomeDefinition` | `_narrativeDescription` | Não aparece na UI hoje |
| Personagens | `Gameplay/Characters/*.cs` (`CharacterRegistry`, nome/classe/lore) | Tela de seleção |
| Créditos | `UI/Menus/CreditsContent.cs` | Texto de autoria. Pode continuar pt-BR |
| Literais em controllers/presenters | `HudController` (`"Nv. {0}"`), `SkillTreeController` (`"Pontos disponíveis"`, `"Custo: ..."`, `"Selecione uma skill"`, `"Nível máximo"`), `CraftingRecipePresenter`/`CraftingUIController` (`"Requer:"`, `"Resultado:"`, `"Criado!"`, motivos de bloqueio), `SaveSlotPresenter`/`SaveLoadMenuController` (`"Slot"`, `"Vazio"`, `"Salvar"`, `"Carregar"`, `"Excluir"`), `DeathScreenController` (`_title`, `_message`, `"Continuando em {0}s"`), `CharacterSelectController` (`"Continuar com {0}"`) | |
| UXML com texto fixo | ver §2.3 | |

### 2.3 UXML (`Assets/UI/**`)

Quando o controller sobrescreve o texto em runtime, o valor do UXML vale só como placeholder.
Quando não sobrescreve, é texto fixo em pt-BR.

| Arquivo | Texto fixo que fica na tela | Sobrescrito em runtime |
|---|---|---|
| `HUD/HudView.uxml` | Vida, Stamina, Mana, Correr, Pular, Missões, Mochila, Skills, Menu | valores das barras, nível, slots |
| `Menus/SettingsMenuView.uxml` | Configurações, Fechar, abas, rótulos de gráficos/áudio/acessibilidade, Idioma | cabeçalhos/status/linhas de Controles (`SettingsMenuTexts`) |
| `Menus/PauseMenuView.uxml` | Pausado, Retomar, Configurações, Salvar, Conquistas, Criação, Menu Principal | — (`ui.pause.*` existe na tabela, mas ninguém usa) |
| `Menus/MainMenuView.uxml` | Novo Jogo, Continuar, Configurações, Créditos, Sair | — (`ui.menu.*` existe na tabela, mas ninguém usa) |
| `Shop/ShopView.uxml` | **Em inglês:** "Gold:", "Buy", "Sell" | nome da loja (`shop.*`) |
| `Crafting/CraftingView.uxml` | Criação, Receitas, Materiais, Criar, "Nenhuma receita conhecida." | receita/resultado (pt-BR em código) |
| `SkillTree/SkillTreeView.uxml` | Fechar, Aprender | título (nome da árvore), pontos, detalhe |
| `Death/DeathScreenView.uxml` | Continuar, Carregar jogo | título/mensagem (campos serializados pt-BR) |
| `CharacterSelect/CharacterSelectView.uxml` | "Escolha seu Herói", Confirmar, Continuar | detalhe do herói |
| `Menus/SaveLoadMenuView.uxml`, `Menus/CreditsView.uxml` | Fechar, Créditos, Voltar | título, slots, versão |
| `Quests/QuestLogView.uxml`, `Achievements/*.uxml`, `Inventory/InventoryView.uxml` | — | tudo pelos helpers (UXML é só placeholder) |
| `Dialogue/DialogueView.uxml` | — | placeholder em inglês ("Speaker", "Line text goes here.") sempre sobrescrito |

## 3. Política

1. **Texto narrativo e de missão** (quests, diálogos, conquistas, diário) é sempre por chave
   e precisa existir em en **e** pt-BR. O teste falha se faltar alguma das duas.
2. **Chave nova** entra no helper da feature dona (padrão `XxxTexts.All` com `EnsureLoaded`,
   que só adiciona o que falta) ou na tabela central, se for conteúdo seedado sem dono de UI.
   Um helper novo precisa entrar em `LocalizationCoverageTests.Sources()`.
3. **Nunca** definir a mesma chave em dois lugares com textos diferentes, nem redefinir no
   helper uma chave da tabela central. Os dois casos são testados.
4. **Placeholders** (`{0}`, `{1:N0}`) precisam ser iguais nos dois idiomas. Também é testado.
5. **Fallback sem `LocalizationManager`** (cena de teste): os helpers devolvem pt-BR.
   Exceção: `TooltipLocalization.GetEnglishFallback` devolve inglês (ver §4, item 6).
6. **Só pt-BR por enquanto (aceito):** nomes e descrições de itens, skills, inimigos, receitas,
   combos e árvores; lore dos personagens; créditos; e o texto fixo dos UXML listado em §2.3.
   Em `en`, essas telas ficam com o texto em português. Isso é conhecido e está na fila abaixo.
   **Não converter em massa.** Converter tela por tela, junto com o dono da tela.

## 4. Fila de conversão (prioridade)

1. **`ShopView.uxml`**: "Gold:"/"Buy"/"Sell" estão em inglês num jogo pt-BR. Os dois idiomas
   saem errados hoje. Criar `ui.shop.gold/buy/sell` e preencher no `ShopUIController`.
2. **HUD** (`HudView.uxml` + `"Nv. {0}"`): é a tela sempre visível e a causa do "HUD em
   português, missão em inglês" quando o idioma é `en`. Criar `ui.hud.*` (rótulos das barras,
   botões, nível) com um helper `HudTexts` e reaplicar em `LanguageChanged`.
3. **Pause / Main Menu**: as chaves `ui.pause.*`/`ui.menu.*` já existem em parte na tabela
   central. Falta ligar nos controllers e completar (salvar, conquistas, criação, menu
   principal, novo jogo, créditos, continuar).
4. **Configurações**: abas e rótulos fixos do UXML. O dropdown de idioma mostra os códigos
   crus `en`/`ptBR` (trocar por "English"/"Português (Brasil)"), cai em `en` quando não há
   manager, e **a escolha de idioma não é persistida** (volta ao idioma do sistema a cada
   execução). Persistir junto com as outras configurações.
5. **Morte / Salvar-Carregar / Seleção de personagem / Créditos**: literais em controllers e
   presenters (§2.2).
6. **Crafting / Skill Tree**: literais (`"Requer:"`, `"Custo: ..."`, `"Nível máximo"`...).
   Alinhar também o fallback em inglês de `TooltipLocalization.GetEnglishFallback` com os
   outros helpers (pt-BR).
7. **Conteúdo de ScriptableObject** (itens → skills → inimigos → receitas/combos/árvores →
   lore dos personagens): adicionar `_nameKey`/`_descriptionKey` opcionais, usando o campo
   atual como fallback pt-BR, no mesmo modelo de `ItemAffixDefinition`
   (`_nameLocalizationKey` + `_fallbackDisplayText`). Os afixos procedurais já têm chave e
   tradução, mas `ProceduralItemGenerator` ainda é chamado sem `localizeName`. Quando for
   ligado, revisar a ordem prefixo/sufixo por idioma ("Afiada Espada" × "Sharp Sword").
8. **Pipeline**: `LocalizationCsv` existe, mas nenhum CSV/`LocalizationTable` real é
   carregado. Quando houver, os helpers já cedem a ele (regra de "só adiciona o que falta").
