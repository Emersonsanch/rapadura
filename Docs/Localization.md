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
| Helper | `UI/Menus/SettingsMenuTexts.cs` | ~59 (`ui.settings.*`, inclusive abas/rótulos do UXML e nomes dos idiomas) | `SettingsMenuController` |
| Helper | `UI/Common/UiTexts.cs` | 73 (`ui.hud.*`, `ui.shop.*`, `ui.pause.*`, `ui.menu.*`, `ui.crafting.*`, `ui.skilltree.*`, `ui.saveload.*`, `ui.credits.*`, `ui.death.*`, `ui.character_select.*`) | `UiDocumentLocalizer` / controllers e presenters dessas telas |
| Helper | `Gameplay/Content/ContentTexts.cs` | 181 (`item.*`, `skill.*`, `enemy.*`, `recipe.*`, `combo.*`, `tree.*`, `character.*`) | `ContentText.Resolve` na primeira consulta (ver 1.1) |
| Helper | `UI/Bestiary/BestiaryTexts.cs` | 88 (`ui.bestiary.*`, `bestiary.<inimigo>.lore` e `bestiary.<inimigo>.comment.<primo>`) | `BestiaryController` (Caderno da Vó Edna) |
| Helper | `UI/PhotoMode/PhotoModeTexts.cs` | 32 (`ui.photo.*`: painel, dicas, toasts, nomes dos filtros e das poses) | `PhotoModeController` (Modo foto) |

### 1.1 Conteúdo de ScriptableObject (`ContentText`)

Itens, skills, inimigos, receitas, combos, árvores e os 5 personagens **não** ganharam campo
novo de chave: a chave é derivada do id e o texto do asset continua sendo o pt-BR e o fallback.

| Tipo | Chaves | Id usado |
|---|---|---|
| `ItemDefinition` | `item.<itemId>.name` / `.description` | `ItemId` |
| `SkillDefinition` | `skill.<skillId>.name` / `.description` | `SkillId` |
| `EnemyDefinition` | `enemy.<nome do asset>.name` | `name` do asset (não há campo de id) |
| `RecipeDefinition` | `recipe.<recipeId>.name` / `.description` | `RecipeId` |
| `ComboDefinition` | `combo.<comboId>.name` / `.description` | `ComboId` |
| `SkillTreeDefinition` | `tree.<treeId>.name` / `.description` | `TreeId` |
| `PlayableCharacter` | `character.<id>.name` / `.class` / `.role` / `.lore` | `CharacterId` em minúsculas (`maithe`, `icaro`...) |

`Gameplay/Content/ContentText.cs` expõe extensões `LocalizedName()` / `LocalizedDescription()`
(e `LocalizedClassName/Role/Lore()` para personagens). Elas resolvem pelo `LocalizationManager`
do `ServiceLocator` quando ele tem a chave, senão devolvem o texto do asset. A sobrecarga com
`LocalizationManager` é para presenters puros: `null` ali significa "texto pt-BR do asset".
Asset sem entrada (ou cena sem manager) mostra exatamente o que mostrava antes.

**Só a UI usa os acessores.** Lógica de jogo continua no `DisplayName` cru:
`CharacterSkillSet` (casa nome de skill com `AbilityNames`), ordenação do inventário
(`InventoryManager.GetSortName`), `ProceduralItemGenerator` (o nome gerado vai para o save) e
`EnemyVisualSwapper`.

**Nome de item procedural (`Gameplay/Items/Procedural/GeneratedItemName.cs`):** a UI monta o nome
na hora de exibir, a partir do item base + `InventorySlotData.generatedAffixIds`, no idioma atual.
`GeneratedItemName.Compose(slot, manager)` (ou `Compose(item, slot, manager, resolveAffix)`) usa
`item.LocalizedName(manager)` e o texto de cada afixo pela chave `affix.*` (fallback:
`_fallbackDisplayText` pt-BR). Ordem por idioma (`ComposeParts`):

| Idioma | Ordem | Exemplo |
|---|---|---|
| pt-BR | base + prefixos + sufixos | "Espada de Ferro Afiada do Vento" |
| en | prefixos + base + sufixos | "Sharp Iron Sword of the Wind" |

Dentro de cada grupo vale a ordem do roll. Sem manager = pt-BR (texto do asset, ordem pt-BR).
Slot sem afixos mostra só o nome base localizado. **Compatibilidade:** `generatedName` continua
no save (pt-BR, gravado por `ProceduralItemGenerator`, que agora usa a mesma ordem pt-BR via
`ComposeParts`; antes saía "Afiada Espada"). Ele só aparece quando o nome não pode ser montado:
algum id de afixo que não existe mais (save antigo, afixo removido) ou item base desconhecido.
Ordenação do inventário e logs continuam no `generatedName`. Usado por `TooltipContentBuilder`
(título; a loja e o equipamento passam por ele) e pelos nomes do inventário/equipamento
(`InventoryUIController`). Limite conhecido: o adjetivo pt-BR do asset não concorda em gênero com
o item ("Afiada" num "Machado"); resolver isso exige variantes por gênero nas chaves `affix.*`.

Telas ligadas: tooltip de item/skill (`TooltipContentBuilder`), nomes do inventário
(`InventoryUIController`), linhas de compra/venda da loja, lista/detalhe/ingredientes/resultado
da criação, título/descrição da árvore e detalhe da skill, nome da skill no slot do HUD,
recompensas do Quest Log (`QuestLogPresenter`), seleção de personagem (botões, nome,
classe — papel, lore, "Continuar com {0}") e o nome do buff de combo (`ComboTracker`).
A seleção de personagem refaz tudo isso no `LanguageChanged` do seu `UiDocumentLocalizer` (e ao
reabrir, caso o idioma tenha mudado com a tela escondida): troca o texto dos botões existentes e
do herói destacado sem recriar nada, então seleção, foco e a pausa não mudam. As regras de texto
são estáticas e puras (`CharacterSelectController.BuildRosterButtonText/BuildClassRoleText/BuildLoreText`).
Inimigo ainda não tem nome na UI; o acessor existe para quando tiver.

**Ao editar texto de conteúdo:** mude o asset (ou a classe do personagem) **e** a linha pt-BR em
`ContentTexts`; o teste `ContentTexts_PtBRMatchesAssetText` falha se divergirem. Asset novo
precisa de entrada nova (`ContentAssets_AllHaveEnAndPtBR`); asset renomeado/removido deixa chave
órfã (`ContentTexts_HasNoOrphanKeys`).

**Regra dos helpers:** ao carregar, cada helper só adiciona as chaves que ainda **não**
existem (`LoadEntries(x.Where(e => !HasKey(e.Key)))`). Um CSV/`LocalizationTable` externo
sempre ganha, e nenhum helper sobrescreve a tabela central. `TooltipLocalization` e
`SettingsMenuTexts` antes carregavam a lista inteira sem filtro, guardados só por uma chave
sentinela. Foram alinhados ao padrão nesta auditoria.

**Idioma inicial:** `GameManager.BuildManagers()` chama
`LocalizationManager.SetLanguage(LocalizationManager.ResolveStartupLanguage(Application.systemLanguage, LocalizationManager.LoadSavedLanguage()))`.
A escolha salva em Configurações → Idioma (PlayerPrefs `Localization.Language`, gravada por
`LocalizationManager.SaveLanguagePreference`) sempre ganha. Sem escolha salva (ou com valor
inválido), um aparelho em inglês começa em `en` e qualquer outro idioma (português, espanhol,
desconhecido...) começa em **pt-BR**. Um `new LocalizationManager()` sem `GameManager` (testes,
ferramentas) continua começando em `en`. O dropdown de idioma mostra "Português (Brasil)" e
"English" (nunca os códigos crus) e, sem manager, marca pt-BR.

**Texto fixo de UXML (`UiDocumentLocalizer`, `UI/Common/`):** o elemento recebe a classe USS `loc`
e a chave em `view-data-key` (id de string nativo do UI Toolkit; a persistência de view data só
existe em janelas de editor, então é inerte em runtime):

```xml
<ui:Button name="save-button" text="Salvar" class="pause-menu-button loc" view-data-key="ui.pause.save" />
```

Labels e Buttons recebem `text`; Toggle/Slider/DropdownField/TextField recebem `label`. O texto
do UXML continua em pt-BR legível: é o que aparece sem `LocalizationManager` ou com chave
desconhecida (nunca vira `[key]`). Cada controller tem um `UiDocumentLocalizer`: `Attach` no
`OnEnable`, `Refresh` no `Show`/`Start` (o manager pode ser registrado depois do `OnEnable`) e
`Detach` no `OnDisable`. Ele reaplica em `LanguageChanged` e chama o callback do controller para
refazer o texto montado em código. O resolver padrão é `UiTexts.Get`; Configurações passa
`SettingsMenuTexts.Get`.

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
| Item procedural | nome montado na exibição (`GeneratedItemName`, ver 1.1); `generatedName` pt-BR só como fallback de save antigo | `item.*` (`ContentTexts`) + `affix.*` (tabela central) |
| Tooltip / Inventário | enums `ItemRarity`, `ItemType`, `EquipmentSlot`, `StatType`, `ElementType` → `ui.tooltip.<grupo>.<snake_case>` | `TooltipLocalization` |
| Quest Log / Tracker, Conquistas, Configurações (rótulos criados em código) | constantes `*Key` | Helpers correspondentes |
| HUD, Loja, Pausa, Menu Principal, Configurações, Criação, Árvore de skills, Salvar/Carregar, Créditos, Morte, Seleção de personagem | UXML (`class="loc"` + `view-data-key`) e literais dos controllers/presenters | `UiTexts` / `SettingsMenuTexts` |
| Títulos das seções dos créditos | `CreditsSection.TitleKey` (`ui.credits.section.*`; `Title` é o fallback pt-BR) | `UiTexts` |
| `ItemDefinition` (20), `SkillDefinition` (45), `EnemyDefinition` (9), `RecipeDefinition` (3), `ComboDefinition` (2), `SkillTreeDefinition` (6), personagens (5) | nome/descrição (personagem: nome/classe/papel/lore), chave derivada do id (ver 1.1) | `ContentTexts` |

### 2.2 Só pt-BR por enquanto (texto direto, sem chave)

| Fonte | Campos / arquivos | Observação |
|---|---|---|
| `StructureDefinition` | `_displayName`, `_description` | |
| `BiomeDefinition` | `_narrativeDescription` | Não aparece na UI hoje |
| Personagens | `AbilityNames` | Só usado para casar com as skills (`CharacterSkillSet`); a UI mostra o nome da skill |
| Créditos | `UI/Menus/CreditsContent.cs`, linhas das seções | Texto de autoria (nomes, ferramentas, licenças, agradecimentos). Os títulos das seções já são chave |

### 2.3 UXML (`Assets/UI/**`)

Quando o controller sobrescreve o texto em runtime, o valor do UXML vale só como placeholder.
Quando não sobrescreve, é texto fixo em pt-BR.

| Arquivo | Texto fixo que fica na tela | Sobrescrito em runtime |
|---|---|---|
| `HUD/HudView.uxml` | ✅ localizado (`ui.hud.*`, nível `ui.hud.level`) | valores das barras, slots |
| `Menus/SettingsMenuView.uxml` | ✅ localizado (`ui.settings.title/close/tab.*/graphics.*/audio.*/language.*/accessibility.*`) | cabeçalhos/status/linhas de Controles (`SettingsMenuTexts`) |
| `Menus/PauseMenuView.uxml` | ✅ localizado (`ui.pause.*` + `ui.menu.settings`; "Retomar" passou a "Continuar", texto da chave existente) | — |
| `Menus/MainMenuView.uxml` | ✅ localizado (`ui.menu.*`; primeiro botão "Novo jogo"/"New game" por `ui.menu.new_game`, porque a tela também tem "Continuar". A chave central `ui.menu.start` "Iniciar Jogo" continua existindo, sem uso no UXML) | — |
| `Shop/ShopView.uxml` | ✅ localizado (`ui.shop.gold/buy/sell`; pt-BR "Ouro:/Comprar/Vender") | nome da loja (`shop.*`), linhas (`ui.shop.buy_row/sell_row`) |
| `Crafting/CraftingView.uxml` | ✅ localizado (`ui.crafting.*`) | receita/resultado/motivos (`ui.crafting.*`) |
| `SkillTree/SkillTreeView.uxml` | ✅ Fechar (`ui.skilltree.close`) | título (nome da árvore, `tree.*` via `ContentText`), pontos, detalhe (`ui.skilltree.*`) |
| `Death/DeathScreenView.uxml` | ✅ localizado (`ui.death.title/message/continue/load`) | "Voltar ao mapa" em encontro (`ui.death.return_to_map`), contagem (`ui.death.countdown`); `_title`/`_message`/`_encounterContinueText` só valem se mudados no Inspector |
| `CharacterSelect/CharacterSelectView.uxml` | ✅ localizado (`ui.character_select.title/confirm`) | dica sem herói (`ui.character_select.prompt`), "Continuar com {0}" (`ui.character_select.continue_with`); nome/classe/papel/lore do herói por `character.*` (`ContentText`) |
| `Menus/SaveLoadMenuView.uxml`, `Menus/CreditsView.uxml` | ✅ localizado (`ui.saveload.*`, `ui.credits.*`) | título, slots (`ui.saveload.*`), versão |
| `Quests/QuestLogView.uxml`, `Achievements/*.uxml`, `Inventory/InventoryView.uxml` | — | tudo pelos helpers (UXML é só placeholder) |
| `Resources/UI/DialogueView.uxml` | — | placeholder em inglês ("Speaker", "Line text goes here.") sempre sobrescrito |

## 3. Política

1. **Texto narrativo e de missão** (quests, diálogos, conquistas, diário) é sempre por chave
   e precisa existir em en **e** pt-BR. O teste falha se faltar alguma das duas.
2. **Chave nova** entra no helper da feature dona (padrão `XxxTexts.All` com `EnsureLoaded`,
   que só adiciona o que falta) ou na tabela central, se for conteúdo seedado sem dono de UI.
   Um helper novo precisa entrar em `LocalizationCoverageTests.Sources()`.
3. **Nunca** definir a mesma chave em dois lugares com textos diferentes, nem redefinir no
   helper uma chave da tabela central. Os dois casos são testados.
4. **Placeholders** (`{0}`, `{1:N0}`) precisam ser iguais nos dois idiomas. Também é testado.
5. **Fallback sem `LocalizationManager`** (cena de teste): os helpers devolvem pt-BR, inclusive
   `TooltipLocalization.GetFallback` (usado por `TooltipContentBuilder`/`InventoryUIController`;
   `GetEnglishFallback` continua existindo só para consulta). Presenters puros
   (`SaveSlotPresenter`, `CraftingRecipePresenter`) recebem um `LocalizationManager` opcional e,
   sem ele, devolvem o mesmo pt-BR de antes.
6. **Só pt-BR por enquanto (aceito):** `StructureDefinition`,
   linhas do corpo dos créditos (`CreditsContent`, os títulos das seções já são chave).
   Em `en`, esses textos ficam em português. Isso é conhecido e está na fila abaixo.
   **Não converter em massa.** Converter tela por tela, junto com o dono da tela.
7. **Conteúdo de ScriptableObject** vai por `ContentText` (seção 1.1): o asset é a fonte do pt-BR,
   `ContentTexts` guarda o en e uma cópia do pt-BR vigiada pelo teste de divergência.

## 4. Fila de conversão (prioridade)

1. ✅ **`ShopView.uxml`**: `ui.shop.gold/buy/sell` no UXML, linhas de compra/venda por
   `ui.shop.buy_row/sell_row` (o "ea." em inglês virou "cada"/"each").
2. ✅ **HUD**: `HudView.uxml` marcado com `ui.hud.*`; nível por `ui.hud.level` ("Nv. {0}"/"Lv. {0}"),
   refeito em `LanguageChanged`. (Ficou em `UiTexts`, não num `HudTexts` separado.)
3. ✅ **Pause / Main Menu**: ligados às chaves centrais `ui.pause.title/resume` e
   `ui.menu.start/settings/quit`, completadas em `UiTexts` (`ui.pause.save/achievements/crafting/main_menu`,
   `ui.menu.continue/credits`).
4. ✅ **Configurações**: abas e rótulos do UXML localizados; dropdown com "Português (Brasil)" /
   "English" (pt-BR primeiro, pt-BR sem manager); escolha **persistida** em PlayerPrefs e aplicada
   no boot antes do idioma do sistema.
5. ✅ **Salvar-Carregar / Créditos**: `ui.saveload.*` (título, slots, botões) e `ui.credits.title/back`.
   ✅ **Morte / Seleção de personagem**: `ui.death.*` (título, mensagem, Continuar, Carregar jogo,
   "Voltar ao mapa" em encontro, contagem "Continuando em {0}s") e `ui.character_select.*` (título,
   Confirmar, dica sem herói, "Continuar com {0}"). Os campos serializados do `DeathScreenController`
   só ganham da chave quando alterados no Inspector (no valor padrão pt-BR, vale a tradução).
   Menu principal: "Novo jogo" por `ui.menu.new_game` (pt-BR "Novo jogo", en "New game").
   Créditos: títulos das seções por `ui.credits.section.*` (`CreditsSection.TitleKey`/`GetTitle`),
   refeitos em `LanguageChanged`; as linhas continuam texto de autoria.
6. ✅ **Crafting / Skill Tree**: `ui.crafting.*` (inclusive motivos de bloqueio e nomes de estação
   `ui.crafting.station.*`, antes o enum cru "Forge") e `ui.skilltree.*` (pontos, custo, botões).
   Só os rótulos da árvore; a lógica de seleção não foi tocada. O fallback de
   `TooltipLocalization` agora é pt-BR.
6b. ⏳ **Ainda com texto fixo/sem chave:** Quest Log/Tracker e Diálogo (UXML só placeholder,
   já passam pelos helpers; não tocados porque estavam em edição), linhas do corpo dos créditos,
   placeholders sobrescritos em runtime (nome da loja,
   "Aprender" no detalhe da skill). O título da árvore já vem de `tree.*` (item 7).
7. ✅ **Conteúdo de ScriptableObject** (itens, skills, inimigos, receitas, combos, árvores, nome/
   classe/papel/lore dos personagens): em vez de `_nameKey` por asset, chave derivada do id
   (`item.<itemId>.name` etc.) resolvida por `ContentText`, com o texto do asset como fallback
   pt-BR. Nenhum asset foi editado. 181 entradas em `ContentTexts` (en novo + pt-BR igual ao
   asset).
7b. ✅ **Nome de item procedural**: montado na exibição por `GeneratedItemName` (item base +
   ids de afixo, ordem por idioma: "Espada de Ferro Afiada" × "Sharp Iron Sword"). O save
   continua gravando `generatedName` em pt-BR, usado só quando o nome não pode ser montado (ver 1.1).
   Pendente: concordância de gênero dos afixos pt-BR.
7c. ✅ **Seleção de personagem**: troca de idioma com a tela aberta refaz botões, nome,
   classe — papel e lore do herói destacado (ou a dica, sem herói).
8. **Pipeline**: `LocalizationCsv` existe, mas nenhum CSV/`LocalizationTable` real é
   carregado. Quando houver, os helpers já cedem a ele (regra de "só adiciona o que falta").
