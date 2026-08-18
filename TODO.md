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
- [ ] Definir estratégia de branches (ex: trunk-based ou GitFlow simplificado)
- [ ] Configurar CI/CD (build automático, testes, lint) — GitHub Actions / Jenkins / Unity Cloud Build
- [ ] Definir Company/Product Name e ícone em Player Settings
- [ ] Configurar `.editorconfig` + convenções de código (naming, namespaces `Rapadura.*`)
- [ ] Definir estrutura de pastas definitiva (Assets, Addressables, StreamingAssets)

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
- [ ] Instanciar `PlayableCharacter.ApplyPassive` no spawn do player (hoje é placeholder — bônus reais dependem do sistema de Atributos da Fase 3)
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

> ⚠️ **Bloqueio conhecido:** este projeto nunca foi aberto no Unity Editor (pasta `ProjectSettings` está vazia — sem `ProjectVersion.txt`/`ProjectSettings.asset`). Scripts, testes e configs de repositório podem ser criados por fora, mas **Scene e Prefabs precisam ser montados manualmente abrindo o projeto no Editor** — arquivos `.unity`/`.prefab` são YAML com GUIDs gerados pelo Editor e não é seguro criá-los à mão. Próximo passo manual: abrir no Unity Hub (define a versão/`ProjectVersion.txt`), criar `Main.unity`, montar o prefab do Player com os componentes já existentes (`PlayerController`, `PlayerInputHandler`, `PlayerMotor`, `PlayerStats`, `CharacterController`, `Animator`, `PlayerCamera`).

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
- [ ] Arma corpo a corpo
- [ ] Arma de longo alcance
- [ ] Sistema de munição
- [ ] Sistema de recarga
- [ ] Durabilidade (opcional, avaliar se combina com o jogo) — `ItemDefinition`/`InventorySlotData` já têm campos de durabilidade e dano de arma, mas nada no loop de combate os usa ainda

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
- [ ] Atordoamento (stun) — falta flag de "não pode agir" consumida por `PlayerController`/State Machine
- [ ] Sistema de resistências/imunidades por tipo de dano

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
- [ ] Sistema de geração procedural de itens (afixos), se aplicável ao gênero

### Progressão
- [x] XP — `PlayerStats.AddExperience`, evento `PlayerExperienceChangedEvent`
- [x] Níveis — `PlayerStats.Level`
- [ ] Atributos — só há vitais (vida/stamina/mana); não há força/destreza/etc. (`PlayableCharacter.ApplyPassive` ainda é placeholder)
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
- [ ] Madeira
- [ ] Pedra
- [ ] Ferro
- [ ] Ouro

### Crafting
- [ ] Bancada
- [ ] Receitas
- [ ] Sistema de desbloqueio

### Construção
- [ ] `BuildingManager`
- [ ] Construção modular
- [ ] Upgrade de estruturas
- [ ] Validação de colocação (colisão, terreno válido, grid snapping)

---

## 🎨 FASE 6 — Interface

### HUD
- [ ] Vida
- [ ] Stamina
- [ ] Mana
- [ ] Barra de XP
- [ ] HUD adaptativa (escala/posição configurável — acessibilidade)

### Inventário
- [ ] Janela principal
- [ ] Equipamentos
- [ ] Crafting

### Menus
- [ ] Main Menu
- [ ] Pause
- [ ] Configurações (gráficos, áudio, controles, idioma)
- [ ] Save/Load

### UX
- [ ] Tooltips
- [ ] Feedback visual
- [ ] Sons de interface
- [ ] Navegação 100% por controle/teclado (sem depender só de mouse)

### Acessibilidade
- [ ] Escala de fonte / alto contraste
- [ ] Suporte a colorblind (paletas alternativas)
- [ ] Remapeamento de controles
- [ ] Opção de reduzir screen shake / efeitos de flash

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
- [ ] Sistema de diálogo
- [ ] Lojas
- [ ] Missões

### Quests
- [ ] Principal
- [ ] Secundárias
- [ ] Diárias

### Biomas
- [ ] Floresta
- [ ] Deserto
- [ ] Montanha
- [ ] Cavernas

---

## 🔊 FASE 9 — Áudio

### AudioManager
- [ ] Música ambiente
- [ ] Sons de combate
- [ ] Sons do jogador
- [ ] Sons da interface

### Sistema Avançado
- [ ] Áudio espacial
- [ ] Mixagem (Audio Mixer com grupos: Master/Música/SFX/UI)
- [ ] Controle de volume por categoria
- [ ] Ducking dinâmico (ex: abaixar música durante diálogo)

---

## 🚀 FASE 10 — Polimento

### Performance
- [ ] Object Pooling
- [ ] Addressables
- [ ] Occlusion Culling
- [ ] LODs
- [ ] Profiling recorrente (Unity Profiler / Frame Debugger) com metas de FPS por plataforma
- [ ] Orçamento de memória e draw calls documentado

### Qualidade
- [ ] Sistema de Analytics (retenção, funil, eventos de gameplay)
- [ ] Sistema de Logs
- [ ] Debug Overlay
- [ ] Testes automatizados (cobertura de sistemas críticos: save, combate, inventário)
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
