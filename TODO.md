# 🎮 RAPADURA — ROADMAP DE DESENVOLVIMENTO

> 📌 Este é um documento vivo. Conforme o projeto evolui, novas tarefas são adicionadas, itens concluídos são marcados `[x]` e escopo obsoleto é removido ou movido para o changelog. Não é um plano fixo — é atualizado a cada sessão de trabalho relevante.

## 📝 Changelog do Roadmap
- **2026-08-17** — Estrutura inicial em 10 fases + Meta do MVP.
- **2026-08-17** — Adicionada Fase 0 (pré-produção/fundação técnica), itens de engenharia profissional (testes, CI/CD, logging, acessibilidade, localização, compliance de loja) espalhados pelas fases existentes.
- **2026-08-17** — Adicionadas Fases 11–14 (Arte/Animação/Cinemáticas, Multiplayer opcional, Marketing/Comunidade, Pós-lançamento/Live Ops).
- **2026-08-17** — Definido o roster de 5 personagens jogáveis (Joaquim, Maria, Maithe, Ícaro, Lavine) com lore, classe, função e habilidades; classes C# criadas em `Assets/Scripts/Gameplay/Characters/`.
- **2026-08-17** — Definido que **mobile é plataforma obrigatória** (não opcional) — adicionada checklist específica de mobile na Fase 0; câmera e sistemas novos devem sempre considerar touch desde o início, não como adaptação posterior.
- **2026-08-17** — Iniciada a Fase 0 (Git/`.gitignore`/`.gitattributes`, `.editorconfig`, `Docs/GDD.md`, `CONTRIBUTING.md`, `GameLogger`, testes automatizados EditMode, workflow de CI) e código da Fase 1 (estados Fall/Slide, câmera com zoom/shake, checkpoints/respawn).
- **2026-08-17** — Auditoria de código vs. roadmap: Fases 3 (RPG) e 4 (Skills) estavam com o código muito mais avançado do que os checkboxes indicavam (Inventário, Itens, XP/Níveis, SkillManager, Skill Tree UI já implementados). Checkboxes corrigidos para refletir o estado real. Disparadas 3 frentes em paralelo: Combate base (Fase 2), Localização (Fase 6) e Save versionado/anti-corrupção (Fase 7).
- **2026-08-19** — Marco importante: o projeto foi aberto com sucesso pela primeira vez no Unity Editor (6000.3.22f1), gerando `Library/`/`.meta` reais. Primeiro erro de compilação real corrigido (`InputRebindManager.cs`, tipo aninhado `RebindingOperation` sem qualificação). Conteúdo narrativo dos 5 personagens completo (quests + diálogos pessoais + quest de convergência), árvore genealógica da família documentada no GDD. `Docs/EditorSetupGuide.md` criado para orientar o setup local.
- **2026-08-19** — Conectado o **Unity MCP** (`CoplayDev/unity-mcp`), dando acesso direto ao Editor (Console, compilação, testes) em vez de depender só de cópia/cola manual de erros. Com isso: corrigido bug real de conflito namespace/classe (`Rapadura.Core.EventBus` renomeado pra `Rapadura.Core.Events`), corrigida falha de referência de assembly nos testes (`Assets/Scripts` e `Assets/Scripts/Editor` viraram assemblies nomeadas `Rapadura.Runtime`/`Rapadura.Editor` em vez de depender da implícita `Assembly-CSharp`, que não resolvia de forma confiável a partir de outro `.asmdef`), e habilitado de fato o New Input System. Suíte de testes rodou pela primeira vez de verdade (394 testes via Unity MCP `run_tests`) — projeto agora **compila com 0 erros**. `AttributeSet` corrigido (inicialização lazy). ⚠️ Ainda restam falhas reais de teste a investigar: `GameManagerTests` (parece ligado ao padrão Singleton + testes em sequência não isolando `Instance` corretamente entre casos), `BuildingManagerTests`/`CraftingManagerTests` (NullReferenceException), `CombatCameraShakeRelayTests`/`CrowdControlAndResistanceTests`/`EnemyControllerStateTests` (mesma família de sintoma — provavelmente dependem de managers registrados via `GameManager` que não rodou a tempo), `AccessibilityTests.SaveAndLoadBindingOverrides` (round-trip de binding falhando).

---

## 🧭 FASE 0 — Pré-produção / Fundação Técnica
> Sem isso, tudo que vier depois custa mais caro para corrigir mais tarde.

### Documentação e Escopo
- [x] Game Design Document (GDD) enxuto: `Docs/GDD.md`
- [x] Plataforma-alvo definida: **Mobile é obrigatório** (jogo precisa funcionar em celular); PC como plataforma adicional
- [x] Requisitos mínimos de hardware mobile (RAM/GPU/OS mínimo Android/iOS) — `Docs/PlatformRequirements.md`
- [x] Definir público-alvo e classificação indicativa (ESRB/PEGI) — `Docs/PlatformRequirements.md` (recomendação: ESRB Teen / PEGI 12, sujeito à submissão oficial)

### Requisitos específicos de Mobile (obrigatório, não opcional)
- [ ] Controles em tela (joystick virtual + botões) — `PlayerInputHandler` já lê `Touchscreen.current`, falta a UI dos controles
- [ ] Câmera: suportar pinch-to-zoom além do scroll do mouse
- [ ] Performance: testar em dispositivo de referência low/mid-end, não só no Editor
- [ ] UI escalável para diferentes resoluções/aspect ratios (safe area para notch)
- [ ] Build Android (`.apk`/`.aab`) e iOS testados cedo, não só no fim do projeto (evita retrabalho)
- [ ] Tamanho de build/downloads dentro do limite de lojas mobile (Addressables ajuda aqui)
- [ ] Bateria/thermal: evitar Update() caro rodando sempre a 60fps sem necessidade

### Infraestrutura de Projeto
- [ ] Inicializar Git + `.gitignore` para Unity + Git LFS para binários (texturas, áudio, modelos)
- [x] Definir estratégia de branches (ex: trunk-based ou GitFlow simplificado) — `Docs/ProjectConventions.md` (trunk-based simplificado)
- [ ] Configurar CI/CD (build automático, testes, lint) — GitHub Actions / Jenkins / Unity Cloud Build
- [~] Definir Company/Product Name e ícone em Player Settings — decidido em `Docs/ProjectConventions.md` (Product: Rapadura, Company: Engenho Estúdio); aplicar de fato exige Unity Editor
- [x] Configurar `.editorconfig` + convenções de código (naming, namespaces `Rapadura.*`) — já existia
- [x] Definir estrutura de pastas definitiva (Assets, Addressables, StreamingAssets) — `Docs/ProjectConventions.md`

### Qualidade de Engenharia
- [ ] Adicionar testes automatizados (Unity Test Framework — EditMode e PlayMode)
- [ ] Definir "Definition of Done" por feature (código + teste + review + doc)
- [ ] Processo de code review (PR obrigatório, sem push direto na main)
- [ ] Sistema de logging estruturado (níveis: Debug/Info/Warning/Error, com toggle por build)
- [ ] Error/crash reporting (Sentry, Unity Cloud Diagnostics ou similar)

---

## 👥 Personagens Jogáveis (Roster)
> Classes C# criadas em `Assets/Scripts/Gameplay/Characters/` (`PlayableCharacter` base + `CharacterRegistry`). Habilidades listadas aqui ainda são apenas nomes/placeholders — a implementação real de cada uma entra na Fase 4 (Skills).

### 🛡️ Joaquim — O Guardião das Raízes
- **Classe:** Guardião · **Função:** Tanque / Defesa / Proteção do grupo · **Atributo Principal:** Vitalidade
- **História:** Nasceu em uma pequena vila cercada por florestas antigas e aprendeu a protegê-la dos perigos noturnos. Após a vila ser destruída por criaturas sombrias vindas das profundezas de Rapadura, jurou proteger todos os inocentes. Carrega o Escudo das Raízes Eternas, artefato ancestral que absorve parte do dano sofrido pelos aliados.
- **Habilidades:** Muralha de Pedra · Provocação Heroica · Escudo das Raízes · Terremoto · **Última Resistência (Ultimate)**

### ✨ Maria — A Curandeira da Luz
- **Classe:** Sacerdotisa · **Função:** Suporte / Cura / Buffs · **Atributo Principal:** Espírito
- **História:** Criada no Templo Solar, aprendeu os segredos da energia vital. Quando os cristais sagrados começaram a perder o brilho, recebeu uma visão de que só um grupo de heróis poderia restaurar o equilíbrio do mundo, e parte em jornada para reuni-los.
- **Habilidades:** Cura Divina · Benção da Vida · Escudo Sagrado · Purificação · **Milagre Celestial (Ultimate)**

### 🏹 Maithe — A Arqueira dos Ventos
- **Classe:** Caçadora · **Função:** DPS à distância · **Atributo Principal:** Destreza
- **História:** Cresceu explorando montanhas e florestas proibidas, tornando-se lenda entre os caçadores. Encontrou o artefato Olho do Vento, que lhe permite enxergar ameaças invisíveis, e busca descobrir quem criou os monstros que ameaçam os reinos.
- **Habilidades:** Flecha Precisa · Chuva de Flechas · Disparo Explosivo · Passo do Vento · **Tempestade de Mil Flechas (Ultimate)**

### ⚙️ Ícaro — O Mestre das Máquinas
- **Classe:** Inventor · **Função:** Controle / Dano Tecnológico · **Atributo Principal:** Inteligência
- **História:** Acredita que a tecnologia pode mudar o mundo. Construiu máquinas em vez de estudar magia e, após encontrar fragmentos de uma civilização perdida, desenvolveu equipamentos capazes de rivalizar com os maiores magos. Busca desvendar segredos tecnológicos que podem salvar ou destruir Rapadura.
- **Habilidades:** Torreta Automática · Granada Elétrica · Drone de Ataque · Campo Magnético · **Exército Mecânico (Ultimate)**

### 🔮 Lavine — A Feiticeira das Sombras
- **Classe:** Feiticeira · **Função:** Dano Mágico / Controle de Multidão · **Atributo Principal:** Poder Arcano
- **História:** Nasceu durante o eclipse raro conhecido como Noite Rubra, com uma ligação incomum a energias proibidas. Temida e incompreendida, luta contra a corrupção que tenta dominar sua alma e quer provar que seu poder pode proteger o mundo.
- **Habilidades:** Orbe Sombria · Correntes das Trevas · Névoa da Ilusão · Explosão Arcana · **Eclipse Eterno (Ultimate)**

### Tarefas de implementação do roster
- [x] Classe `PlayableCharacter` (base) + `CharacterId` (enum) + `CharacterRegistry`
- [x] Classes `Joaquim`, `Maria`, `Maithe`, `Icaro`, `Lavine` com lore, classe, função, atributo principal e nomes de habilidades
- [ ] Tela de seleção de personagem (UI)
- [x] `PlayableCharacter.ApplyPassive` implementado de verdade (não é mais placeholder) usando `AttributeSet`
- [ ] Chamar `ApplyPassive` no spawn do player — bloqueado: não existe código de spawn de player ainda (depende de Scene/Prefab, exige Unity Editor)
- [ ] Implementar as 25 habilidades (5 personagens × 5) como `SkillDefinition` reais na Fase 4
- [ ] Modelos/animações/rig únicos por personagem (Fase 11 — Arte e Animação)
- [ ] Voice over / dublagem (se houver, Fase 11 — Cinemáticas)

---

## 🔥 FASE 1 — MVP Jogável (Prioridade Máxima)

### Player
- [ ] Criar Scene principal (`Main.unity`) — **precisa ser feito no Unity Editor** (projeto ainda não foi aberto no Editor; não há `Library`/`ProjectSettings.asset` gerados, então cena/prefab não podem ser criados com segurança só por edição de arquivo)
- [x] Implementar bootstrap do `GameManager` (`Core/Managers/GameManager.cs`, já registra `SaveManager` e `CheckpointManager`)
- [ ] Criar prefab definitivo do Player — depende da Scene existir (Unity Editor)
- [x] Configurar sistema de Input básico (New Input System, `PlayerControls.inputactions`, com leitura de touch já implementada em `PlayerInputHandler`)
- [ ] Rebind de teclas/gamepad em runtime (UI de configurações — Fase 6)
- [x] Validar State Machine (código implementado e registrado em `PlayerController.BuildStateMachine`):
  - [x] Idle
  - [x] Walk
  - [x] Run
  - [x] Jump
  - [x] Crouch
  - [x] Fall (novo — `PlayerFallState`, separado do Jump quando o player cai de uma borda)
  - [x] Slide (novo — `PlayerSlideState`, disparado ao agachar durante a corrida)
  - [x] Sprint (coberto pelo `PlayerRunState`, que já drena stamina — não há um estado "Sprint" redundante por decisão de design)
- [x] Testes de unidade para transições de estado (`Assets/Tests/EditMode/StateMachineTests.cs`)

### Câmera
- [x] Sistema de câmera livre (`PlayerCamera`, orbital em torno de um pivot)
- [x] Camera Follow (segue o pivot em `LateUpdate`)
- [x] Camera Collision (`SphereCast` contra `_collisionMask`)
- [x] Zoom dinâmico (scroll do mouse **e pinch de dois dedos no touch**, ver `HandleZoom()`)
- [x] Camera Shake (`PlayerCamera.Shake(duration, magnitude)` — pronto para ser chamado pelo sistema de combate na Fase 2)
- [ ] Suporte a Cinemachine (avaliar se compensa migrar a solução manual atual — `com.unity.cinemachine` não está no `manifest.json` ainda)

### Mundo
- [ ] Criar mapa inicial (greybox antes de arte final) — Unity Editor
- [x] Spawn points (`Gameplay/World/SpawnPoint.cs`)
- [x] Sistema de checkpoints (`Gameplay/World/CheckpointManager.cs`, registrado no `GameManager`)
- [x] Sistema de respawn (`CheckpointManager` escuta `PlayerDiedEvent` e reposiciona o player — hoje é instantâneo, falta tela de morte/fade quando a Fase 2/6 existirem)
- [ ] Streaming/otimização de cena para mapas grandes (se aplicável)

> ✅ Bloqueio resolvido em 2026-08-19: projeto aberto no Unity Editor (6000.3.22f1) via Unity MCP. `Assets/Scenes/Main.unity` já existe com GameManager + Player + Câmera montados e testados em Play Mode (ver changelog).

### Tela de Seleção de Personagem (novo — decisão de design 2026-08-19)
- [ ] Tela de seleção de personagem no início do jogo (`UI/CharacterSelect/`), mostrando os 5 heróis (`CharacterRegistry`) com lore/classe/função, escolha chama `PlayableCharacter.ApplyPassive(stats)` no spawn do player
- [ ] Ainda não existe — próxima tarefa

### Mapa Mundi + Encontros Aleatórios (novo — decisão de design 2026-08-19)
> 📌 O usuário pediu um jogo "estilo Final Fantasy". Esclarecido via pergunta direta: **mantém o combate em tempo real já implementado** (não vira turno/ATB), mas adiciona uma camada de **mapa do mundo** (visão de cima, navegar entre regiões/biomas) com **batalhas acionadas por encontro aleatório** enquanto anda pelo mapa — like o overworld clássico de FF I-VI. Sistema de batalha em si (o que acontece ao entrar em combate) fica a decidir depois; por enquanto é só a camada de overworld + gatilho de encontro.
- [ ] `WorldMapScene`/`WorldMapController`: visão de cima, player anda entre regiões (reaproveitar os 4 biomas já seedados: Floresta, Deserto, Montanha, Cavernas)
- [ ] Sistema de encontro aleatório (chance por passo/tempo, usando `BiomeDefinition.RollEnemy` já implementado para decidir qual inimigo aparece)
- [ ] Transição overworld → cena/área de combate ao disparar um encontro
- [ ] Definir se o mapa mundi é uma Scene separada da `Main.unity` (mais provável) ou um modo dentro dela

---

## ⚔️ FASE 2 — Combate

### Sistema Base
- [x] Sistema de vida — `Combat/Health.cs` (genérico, para não-players) + `PlayerStats.cs` (player); ver nota abaixo
- [x] Sistema de dano (com fórmula documentada e balanceável via dados externos) — `Combat/DamageInfo.cs`, `Combat/DamageCalculator.cs` (fórmula documentada em XML doc), `Combat/DamageBalanceConfig.cs` (ScriptableObject tunável)
- [x] Hitboxes — `Combat/Hitbox.cs`
- [x] Hurtboxes — `Combat/Hurtbox.cs`
- [x] Knockback — `Combat/KnockbackReceiver.cs`
- [x] Invulnerability Frames — timer em `Combat/Health.cs` (`GrantInvulnerability`/`ClearInvulnerability`)
- [x] Hit-stop / Game Feel (frame freeze, screen shake no impacto) — `Combat/HitStopController.cs` + `Combat/CombatCameraShakeRelay.cs` (chama `PlayerCamera.Shake()` via evento)

> ⚠️ Nota: `PlayerStats` ainda implementa `ICombatTarget` diretamente e não usa `Health`, então o player recebe dano das novas hitboxes mas sem os benefícios de i-frames/hit-stop do novo sistema ainda — migrar `PlayerStats` para usar `Health` (ou adicionar i-frames nele) é o próximo passo. Também não validado em Play Mode (projeto nunca aberto no Editor), só via testes EditMode.

### Armas
- [x] Arma corpo a corpo — `Combat/Weapons/MeleeWeapon.cs` (ativa `Hitbox` por janela de tempo)
- [x] Arma de longo alcance — `Combat/Weapons/RangedWeapon.cs` + `Projectile.cs`
- [x] Sistema de munição — ligado a `InventoryManager` (consome item de munição real)
- [x] Sistema de recarga — temporizador bloqueia disparo durante reload
- [x] Durabilidade — decrementa `InventorySlotData.currentDurability`, quebra/desequipa em zero

> ✅ Corrigido: `UnequipSlot` agora preserva `currentDurability` real ao devolver o item ao inventário (não reseta mais para o máximo).

### Inimigos
- [x] IA básica (Behaviour Tree ou State Machine reutilizando `Core/StateMachine`) — `Enemies/EnemyController.cs` + estados em `Enemies/States/`
- [x] Patrulha — `EnemyPatrolState.cs`
- [x] Perseguição — `EnemyChaseState.cs`
- [x] Ataque — `EnemyAttackState.cs` (usa `Hitbox`/`ICombatTarget`)
- [x] Fuga — `EnemyFleeState.cs`
- [x] Boss inicial — `EnemyDefinition.IsBoss` (config via ScriptableObject, sem exigir prefab dedicado)
- [x] Sistema de spawn/wave (pooling de inimigos) — `EnemyPool.cs` + `EnemySpawner.cs` + `WaveTracker.cs`

> ⚠️ Nota técnica: `EnemyController.ApplyDefinitionToHealth()` usa reflection para aplicar stats de `EnemyDefinition` em `Health` porque `Health.cs` não tem setter público de vida máxima — considerar adicionar um setter público em `Health.cs` para remover essa gambiarra.

### Efeitos
- [x] Buffs — `BuffController`/`ActiveStatEffect`/`StatModifierDefinition` completos (modificadores flat/percent, duração)
- [x] Debuffs — mesma engine, flag de debuff em `BuffController.cs`
- [x] Veneno — seedado em `SkillDatabaseSeeder.cs` (DoT via `BuffController`)
- [ ] Queimadura — engine suporta DoT genérico, mas não há efeito de queimadura seedado especificamente
- [x] Congelamento — seedado em `SkillDatabaseSeeder.cs` (slow)
- [x] Atordoamento (stun) — `BuffController.IsStunned`/`ApplyStun`, `PlayerController` bloqueia `Tick`/`FixedTick` enquanto atordoado (câmera continua livre)
- [x] Sistema de resistências/imunidades por tipo de dano — `Combat/ElementResistance.cs` + overload de `DamageCalculator.ComputeDamage`, 100% = imune

---

## 🎒 FASE 3 — RPG

### Inventário
- [x] Integrar `InventoryManager` — `InventoryManager.cs` completo (slots, peso, stacking, equip, consumíveis, save/load)
- [x] Sistema de slots — `InventoryManager.cs` + `InventorySlotData.cs`
- [x] Equipamentos — `EquipFromSlot`/`UnequipSlot` aplicam modificadores via `BuffController`
- [x] Consumo de itens — `UseItemInSlot`
- [~] Drag and Drop — backend suporta mover/mesclar slots (`MoveOrMergeSlot`), mas não há UI ainda (depende da Fase 6)
- [~] Ordenação/empilhamento automático — empilhar ao adicionar já é automático; falta função de "auto-organizar" o inventário inteiro

### Itens
- [x] `ItemDefinition`
- [x] Banco de itens — `ItemDatabase.cs` + `ItemDatabaseSeeder.cs` (19 itens de exemplo)
- [x] Raridades:
  - [x] Comum
  - [x] Raro
  - [x] Épico
  - [x] Lendário
- [~] Sistema de geração procedural de itens (afixos), se aplicável ao gênero — `Items/Procedural/{ItemAffixDefinition,AffixDatabase,ProceduralItemGenerator}.cs` prontos e testados; falta autoria de afixos reais em `Resources/Affixes` e exibição na UI de inventário/tooltip (Fase 6)

### Progressão
- [x] XP — `PlayerStats.AddExperience`, evento `PlayerExperienceChangedEvent`
- [x] Níveis — `PlayerStats.Level`
- [x] Atributos — `Player/AttributeSet.cs` (Vitalidade/Espírito/Destreza/Inteligência/Poder Arcano, pontos por level-up, fórmulas documentadas), `PlayableCharacter.ApplyPassive` implementado nos 5 personagens
- [~] Talentos — se sobrepõe à skill tree da Fase 4; não há sistema de "talento" separado de skill
- [ ] Curva de balanceamento documentada em planilha (fonte única de verdade) — curva hoje é hardcoded em `PlayerStats.cs` (`base * multiplier^(nível-1)`)

---

## 🌳 FASE 4 — Skills

### Backend
- [x] Integrar `SkillManager`
- [x] Cooldowns — `SkillInstance.TickCooldown/StartCooldown`, reduzido por CDR de buffs
- [x] Custos de recursos — mana/energy via `ISkillResourceProvider` (implementado por `PlayerStats`)
- [x] Sistema de combos — `Skills/ComboDefinition.cs` (ScriptableObject data-driven) + `ComboTracker.cs`, ouve `SkillCastEvent` já existente

### Skill Tree
- [x] Interface visual — `SkillTreeController.cs` + `SkillTreeView.uxml/uss` (UI Toolkit real: grid, conectores, painel de detalhe, botões de aprender/upar)
- [x] Desbloqueio de habilidades — `SkillManager.LearnSkillWithPoint`/`CanLearnSkill`, economia de pontos de skill
- [x] Dependência entre habilidades — `SkillRequirement`, visualizado como linhas conectoras na UI
- [x] Reset de pontos (respec) — `SkillManager.RespecSkill(id)` / `ResetSkillPoints()`, respeita dependências entre skills

### Habilidades
- [~] Ativas — funcionam via `TryCast`
- [x] Passivas — `SkillManager.LearnSkill`/`RestoreState` aplicam o buff automaticamente ao aprender (`ApplyPassiveEffectIfNeeded`), sem esperar cast
- [~] Ultimate — existem no `SkillType`/seeder, mas sem lógica distinta de custo/uso de ultimate (decisão de design pendente: carga/recurso separado?)
- [x] Ferramenta de balanceamento (testar dano/custo sem rebuild) — `SkillEditorWindow.cs`/`ItemEditorWindow.cs` + seeders

---

## 🏗️ FASE 5 — Construção e Crafting

### Recursos
- [x] Madeira — item `item_wood` via `Editor/ResourceItemSeeder.cs`
- [x] Pedra — `item_stone`
- [x] Ferro — `item_iron`
- [x] Ouro — `item_gold` (nova categoria `ItemCategory.Gold` adicionada)

### Crafting
- [x] Bancada — `CraftingStationType`/`RequiresCraftingStation` na receita
- [x] Receitas — `Crafting/RecipeDefinition.cs` + `RecipeDatabase.cs`
- [x] Sistema de desbloqueio — `CraftingManager.UnlockRecipe`/`IsRecipeKnown`

### Construção
- [x] `BuildingManager` — `Building/BuildingManager.cs`
- [~] Construção modular — footprint/grid/níveis prontos; kit modular de prefab real depende do Editor
- [x] Upgrade de estruturas — `BuildingManager.UpgradeStructure` + `StructureLevelData`
- [~] Validação de colocação (colisão, terreno válido, grid snapping) — snap/overlap via AABB puro prontos (`GridPlacementUtility.cs`); validação de terreno real (altura/inclinação) depende de terreno no Editor

> ✅ `CraftingManager`/`BuildingManager` registrados em `GameManager.BuildManagers()`.

---

## 🎨 FASE 6 — Interface

### HUD
- [x] Vida — `UI/HUD/HudView.uxml/uss` + `HudController.cs` (event-driven via `PlayerHealthChangedEvent`)
- [x] Stamina — idem (`PlayerStaminaChangedEvent`)
- [x] Mana — agora event-driven via `PlayerManaChangedEvent` (polling removido)
- [x] Barra de XP — `PlayerExperienceChangedEvent` + label de nível
- [x] HUD adaptativa (escala/posição configurável — acessibilidade) — `HudController` expõe escala/âncora/offset serializados + setters em runtime, pronto para tela de Configurações

> ⚠️ Pendente (exige Unity Editor): criar o `GameObject` com `UIDocument` na Scene e conectar `HudController`.

### Inventário
- [ ] Janela principal
- [ ] Equipamentos
- [ ] Crafting

### Menus
- [x] Main Menu — `UI/Menus/MainMenuView.uxml` + `MainMenuController.cs` (Novo Jogo/Continuar via `SaveManager`/Configurações/Sair)
- [x] Pause — `PauseMenuController.cs`, publica `GamePausedEvent`/`GameResumedEvent` (`Menus/MenuEvents.cs`)
- [x] Configurações (gráficos, áudio, controles, idioma) — `SettingsMenuController.cs` (áudio via `AudioManager`, idioma via `LocalizationManager`, gráficos via QualitySettings/Screen; controles é placeholder documentado, rebind real fica pra depois)
- [x] Save/Load — `SaveLoadMenuController.cs` + `SaveSlotPresenter.cs`

> ⚠️ Pendente (exige Unity Editor): criar os `GameObject`s com `UIDocument` nas Scenes para cada tela.

### UX
- [x] Tooltips — `UI/Common/TooltipController.cs` + `TooltipView.uxml/uss`
- [x] Feedback visual — `UiFeedbackUtility.cs` (Pulse/PulseScale via USS transition)
- [x] Sons de interface — `UiSoundHooks.cs` (PlayClick/PlayHover/PlayError via `AudioManager`)
- [~] Navegação 100% por controle/teclado (sem depender só de mouse) — `focusable="true"` em todos os elementos interativos dos menus/HUD; falta configurar grupos de navegação específicos de gamepad

### Acessibilidade
- [~] Escala de fonte / alto contraste — `Core/Accessibility/AccessibilitySettings.cs` (backend pronto, falta consumir na UI)
- [~] Suporte a colorblind (paletas alternativas) — `ColorblindPaletteMap.cs` (mapeia `ItemRarity`, falta consumir na UI)
- [~] Remapeamento de controles — `InputRebindManager.cs` (usa API real do Input System, falta UI na aba "Controles" do `SettingsMenuController`)
- [~] Opção de reduzir screen shake / efeitos de flash — `AccessibilitySettings.ScaleShakeMagnitude` pronto, falta plugar em `CombatCameraShakeRelay.OnCameraShakeRequested`/`PlayerCamera.Shake`

> ✅ `AccessibilitySettings` registrado em `GameManager.BuildManagers()` e multiplicador de shake plugado em `CombatCameraShakeRelay`. Pendente: consumir escala de fonte/contraste/paleta/rebind na UI de Configurações.

### Localização
- [x] Arquitetura de texto externalizada (não hardcoded em código) — `Core/Localization/LocalizationTable.cs` (ScriptableObject) + `LocalizationCsv.cs`
- [x] Suporte multi-idioma (mínimo PT-BR + EN) — `Core/Localization/LanguageCode.cs` (en/ptBR, fallback pra en) + `LocalizationManager.cs`
- [~] Pipeline de tradução (planilha/CSV ou serviço tipo Crowdin) — parser CSV pronto (`LocalizationCsv.cs`), falta decidir ferramenta de tradução real e automatizar import em CI

> ⚠️ Pendente (fora do escopo do agente que implementou, precisa de decisão/Editor): registrar `LocalizationManager` em `GameManager.BuildManagers()`; criar o asset `LocalizationTable` real ou CSV em `StreamingAssets/`; UI de seleção de idioma (Fase 6 — Menus).

---

## 💾 FASE 7 — Save System

### Persistência
- [x] `SaveManager`
- [x] `SaveData`
- [x] Auto Save — `SaveManager.AutoSave()`/`LoadAutoSave()`/`HasAutoSave()`
- [x] Manual Save — `SaveManager.Save(slot)`/`Load(slot)`
- [x] Versionamento de save (migração entre versões do jogo sem corromper save antigo) — `SaveData.saveVersion` + `SaveManager.CurrentSaveVersion` + pipeline de migrações incrementais
- [x] Proteção contra corrupção (backup do último save válido) — escrita atômica (`.tmp` verificado antes de sobrescrever) + `.bak` com fallback automático no `Load`

> ⚠️ Sugestão não aplicada (fora do escopo do agente): `CheckpointManager.ActivateCheckpoint` poderia chamar `SaveManager.AutoSave()` — hoje checkpoint só reposiciona o player, não salva.

### Dados Salvos
- [ ] Inventário
- [ ] Skills
- [ ] XP
- [ ] Missões
- [ ] Mundo

---

## 🌎 FASE 8 — Conteúdo

### NPCs
- [~] Sistema de diálogo — `Gameplay/Dialogue/{DialogueNode,DialogueDefinition,DialogueManager}.cs` + `UI/Dialogue/DialogueUIController.cs` prontos, testados, `DialogueManager` registrado em `GameManager`, chaves `dialogue.*` traduzidas e `World/NpcInteractable.cs` pronto para disparar via trigger; falta colocar o componente num NPC real em Scene
- [~] Lojas — `Gameplay/Shop/{ShopDefinition,ShopManager}.cs` + `UI/Shop/ShopUIController.cs` prontos, testados, `ShopManager` registrado em `GameManager`, chaves `shop.*` traduzidas, gatilho via `NpcInteractable.cs`; falta colocar numa Scene

> ✅ Conteúdo narrativo dos 5 personagens completo: `Editor/CharacterQuestSeeder.cs` (Joaquim "Ecos da Vila Perdida", Maithe "Rastros do Vento", Lavine "A Maré Rubra") + `Editor/CharacterQuestSeeder2.cs` (Maria "O Brilho Que Se Apaga", Ícaro "Engrenagens do Passado"), cada um com diálogo ramificado ligado (`CharacterDialogueSeeder.cs`/`CharacterDialogueSeeder2.cs`), todas as traduções `quest.*`/`dialogue.*` reais em PT-BR/EN. Além disso: `Editor/ConvergenceQuestSeeder.cs` + `ConvergenceDialogueSeeder.cs` — quest final que exige as 5 quests pessoais como pré-requisito e reúne os 5 heróis (que são família: Maithe/Ícaro irmãos, Joaquim/Maria irmãos, Lavine prima de todos — ordem de idade Maithe>Maria>Joaquim>Ícaro>Lavine) para decidirem enfrentar juntos a ameaça maior de Rapadura.

> ⚠️ `Docs/GDD.md` e os comentários de lore em `Assets/Scripts/Gameplay/Characters/*.cs` ainda não foram atualizados com a árvore genealógica da família — fazer isso na próxima leva de conteúdo.
- [ ] Missões

### Quests
- [~] Principal — backend genérico pronto (`Gameplay/Quests/QuestManager.cs` + `QuestType.MainStory`), falta conteúdo autorado e UI
- [~] Secundárias — `QuestType.Side`, mesmo backend
- [~] Diárias — `QuestType.Daily`, mesmo backend

> ✅ `QuestManager` registrado em `GameManager.BuildManagers()` + `SaveManager.Register`. Pendente: chamar `QuestManager.SetRewardTargets` quando o player existir (Scene); criar assets `QuestDefinition` de conteúdo real; UI de log de quests.

### Biomas
- [~] Floresta — camada de dados pronta (`World/Biomes/BiomeDefinition.cs` + `BiomeSeeder.cs`); terreno/Scene real bloqueado (exige Unity Editor)
- [~] Deserto — idem
- [~] Montanha — idem
- [~] Cavernas — idem

> ✅ `Editor/EnemyDatabaseSeeder.cs` criado (8 inimigos, 2 por bioma incl. boss), ids batem com os já referenciados no `BiomeSeeder`. Ambos os seeders precisam rodar no Unity Editor pra gerar os assets de fato (`Assets/Resources/{Enemies,Biomes}/` ainda não existem neste checkout).

---

## 🔊 FASE 9 — Áudio

### AudioManager
- [x] Música ambiente — `Core/Audio/AudioManager.cs` (`PlayMusic`/`StopMusic` com crossfade)
- [x] Sons de combate — `CombatAudioCueMap.cs`, ouve `DamageAppliedEvent`/`CombatTargetDiedEvent`
- [x] Sons do jogador — API genérica `PlayOneShot`/`PlaySfx` pronta (ainda não chamada por código de player/UI)
- [x] Sons da interface — idem

### Sistema Avançado
- [x] Áudio espacial — `PlaySfxAtPosition` (spatialBlend=1)
- [~] Mixagem (Audio Mixer com grupos: Master/Música/SFX/UI) — categorias lógicas em C# multiplicando `AudioSource.volume`; asset `.mixer` real exige Unity Editor
- [x] Controle de volume por categoria — persistido via `PlayerPrefs`
- [x] Ducking dinâmico (ex: abaixar música durante diálogo) — `DialogueManager` já chama `AudioManager.DuckMusic(...)` em início/troca de linha/fim de diálogo

> ✅ `AudioManager` registrado em `GameManager.BuildManagers()`.

---

## 🚀 FASE 10 — Polimento

### Performance
- [~] Object Pooling — `Core/Pooling/GenericObjectPool.cs` pronto e reutilizável; `EnemyPool.cs` ainda não migrado para usá-lo
- [ ] Addressables
- [ ] Occlusion Culling
- [ ] LODs
- [ ] Profiling recorrente (Unity Profiler / Frame Debugger) com metas de FPS por plataforma
- [ ] Orçamento de memória e draw calls documentado

### Qualidade
- [~] Sistema de Analytics (retenção, funil, eventos de gameplay) — `Core/Analytics/AnalyticsManager.cs` registrado em `GameManager` (ouve eventos reais do EventBus, buffer em memória + log); falta backend real (decisão de produto/legal)
- [x] Sistema de Logs — `Core/Logging/GameLogger.cs` (já existia)
- [~] Debug Overlay — `Core/Debug/DebugOverlayController.cs` (FPS/memória prontos); falta log rolante (precisa de hook novo em `GameLogger.cs`) e colocar o `UIDocument` numa Scene
- [~] Testes automatizados (cobertura de sistemas críticos: save, combate, inventário) — cobertura ampla já existe (save, combate, inventário, skills, enemies, UI); ainda não roda de verdade num Unity Editor real (projeto nunca aberto)
- [ ] Playtesting estruturado com usuários externos + coleta de feedback
- [ ] QA pass dedicado (checklist de regressão antes de cada release)

### Build
- [ ] Git
- [ ] CI/CD
- [ ] Steam Build
- [ ] Android Build
- [ ] iOS Build
- [ ] Certificação/compliance de loja (Steamworks, Google Play, App Store guidelines)
- [ ] Sistema de versionamento semântico (SemVer) + changelog

### Legal / Compliance
- [ ] Termos de uso e política de privacidade (se houver conta online/telemetria)
- [ ] Licenças de assets de terceiros documentadas (evitar problema de copyright)
- [ ] Créditos/atribuições no jogo

---

## 🎬 FASE 11 — Arte, Animação e Apresentação

### Arte
- [ ] Style guide visual (paleta de cores, referências, mood board)
- [ ] Pipeline de import de assets (naming convention, escala, pivot padronizado)
- [ ] Shaders/Materiais customizados (URP/HDRP conforme render pipeline escolhido)
- [ ] VFX (partículas de combate, ambiente, skills)
- [ ] Iluminação e pós-processamento (Volume/Post-processing stack)

### Animação
- [ ] Rig padrão para personagens (humanoide/genérico)
- [ ] Animator Controllers com blend trees (locomoção fluida)
- [ ] Animation Events (sincronizar hit de arma, passos, efeitos sonoros)
- [ ] Root Motion vs. Motor-driven (decisão documentada e consistente)
- [ ] IK (Inverse Kinematics) para pés/mãos, se necessário

### Cinemáticas e Narrativa
- [ ] Cutscenes (Timeline do Unity)
- [ ] Sistema de diálogo com portraits/voz (se houver dublagem)
- [ ] Intro/outro do jogo

---

## 🌐 FASE 12 — Multiplayer (opcional — só se estiver no escopo)
- [ ] Definir se o jogo é single-player, co-op ou multiplayer competitivo
- [ ] Netcode (Unity Netcode for GameObjects, Mirror, Photon, etc.)
- [ ] Sincronização de estado (posição, combate, inventário)
- [ ] Matchmaking/lobby
- [ ] Anti-cheat básico (validação server-side de ações críticas)
- [ ] Testes de latência/rollback

---

## 📣 FASE 13 — Marketing e Comunidade
- [ ] Página de loja (Steam/App Store) com capturas e trailer
- [ ] Demo/Prólogo jogável para divulgação
- [ ] Presença em redes sociais / Discord da comunidade
- [ ] Plano de wishlist (Steam) antes do lançamento
- [ ] Kit de imprensa (press kit) com arte, logo, sinopse

---

## 🛠️ FASE 14 — Pós-lançamento / Live Ops
- [ ] Canal de suporte para bugs/feedback dos jogadores
- [ ] Plano de patches pós-lançamento (hotfix vs. update de conteúdo)
- [ ] Roadmap público de atualizações
- [ ] Telemetria de retenção pós-launch (D1/D7/D30)
- [ ] Planejamento de DLC/expansões (se aplicável)
- [ ] Cloud save / sincronização entre dispositivos (se multi-plataforma)

---

## ⭐ Recursos que aumentam MUITO a retenção
- [ ] Sistema de conquistas
- [ ] Missões diárias
- [ ] Sistema de pets
- [ ] Montarias
- [ ] Crafting avançado
- [ ] Sistema de ranking
- [ ] Eventos aleatórios
- [ ] Loot raro
- [ ] Bosses mundiais
- [ ] Ciclo dia/noite
- [ ] Clima dinâmico
- [ ] Segredos e áreas ocultas
- [ ] New Game+
- [ ] Sistema de temporadas

---

## 🎯 Meta do MVP

O jogo só pode sair da fase MVP quando tiver:
- [ ] Player completo
- [ ] Mapa jogável
- [ ] Combate funcional
- [ ] 3 inimigos diferentes
- [ ] Inventário funcional
- [ ] 10 itens
- [ ] 5 habilidades
- [ ] Save/Load funcionando
- [ ] HUD completa
- [ ] 30 minutos de gameplay contínuo sem bugs críticos
- [ ] Build assinada rodando fora do Editor (standalone), sem depender do Unity Editor

---

*Este roadmap combina o escopo de gameplay de um RPG de mercado com práticas de engenharia de jogos profissionais: testes automatizados, CI/CD, acessibilidade, localização, analytics e compliance de loja — para que o projeto não vire apenas um protótipo, mas um produto ship-ready.*
