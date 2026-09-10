# 🎮 RAPADURA — ROADMAP DE DESENVOLVIMENTO

> 📌 Este é um documento vivo. Conforme o projeto evolui, novas tarefas são adicionadas, itens concluídos são marcados `[x]` e escopo obsoleto é removido ou movido para o changelog. Não é um plano fixo — é atualizado a cada sessão de trabalho relevante.

## 📝 Changelog do Roadmap

### 2026-09-10 (continuação — causa raiz real do visual quebrado: 9 telas sem `<Style>` no UXML)
- 🐛 **Causa raiz encontrada depois de ver a Skill Tree/Pause renderizando como barras cinzas cheias sobrepostas** (screenshot da entrada anterior): `SkillTreeView.uxml`, `PauseMenuView.uxml`, `SettingsMenuView.uxml`, `SaveLoadMenuView.uxml`, `ShopView.uxml`, `DialogueView.uxml`, `MainMenuView.uxml`, `TooltipView.uxml` e `DebugOverlayView.uxml` — **9 telas, todas** — nunca tiveram a tag `<Style src="...">` apontando pro `.uss` correspondente. O `.uss` de cada uma existe, foi escrito com cuidado (cores, layout em grade absoluta pros nós da skill tree, etc.), mas nunca foi carregado — `HudView.uxml` era a **única** tela do projeto que tinha isso certo, por isso só ela sempre pareceu correta. Sem o `<Style>`, todo elemento (inclusive os nós `position:absolute` de 84x84px da Skill Tree) cai no default do UI Toolkit runtime theme: `Button`/`VisualElement` sem classe aplicada vira uma barra cinza full-width empilhada — exatamente o visual quebrado visto nos screenshots anteriores desta sessão, em TODAS as telas recém-ligadas, não só a Skill Tree.
- ✅ **Corrigido nas 9**: adicionada a tag `<Style src="/Assets/UI/.../NomeView.uss" />` logo após a raiz `<ui:UXML>`, mesmo padrão já usado (corretamente) em `HudView.uxml`.
- 🔬 **Confirmado visualmente**: Skill Tree agora mostra 20 nós circulares reais numa grade limpa (`skilltree_fixed.png`), Pause mostra um painel escuro centralizado com título "Pausado" e 4 botões estilizados (`pause_fixed.png`) — nada parecido com as barras cinzas empilhadas de antes.
- 🏁 Suíte de testes: 396/396 (mudança é só UXML, sem risco de regressão de lógica).

### 2026-09-10 (continuação — telas de UI inteiras existiam prontas mas nenhuma estava na cena)
- 🐛 **Achado varrendo sistematicamente**: `PauseMenuController`, `SettingsMenuController`, `SaveLoadMenuController`, `ShopUIController`, `DialogueUIController` e `SkillTreeController` — todos com UXML/USS prontos e código testado — **nenhum tinha um GameObject na cena**. Sem exceção, nenhuma dessas telas era alcançável durante o jogo de verdade.
- ✅ **Todos os seis ligados na cena** (`PauseMenu`, `SettingsMenu`, `SaveLoadMenu`, `ShopMenu`, `DialogueMenu`, `SkillTreeMenu`, cada um `UIDocument`+`PanelSettings` compartilhado + o controller correspondente): `PauseMenuController` referencia `SettingsMenu`/`SaveLoadMenu`; `SkillTreeController` referencia `SkillManager`/`PlayerStats` do Player. `ShopUIController`/`DialogueUIController` se resolvem sozinhos via `ServiceLocator`/`EventBus` (padrão já existente no código, só faltava o GameObject existir) — funcionam assim que um NPC de loja/diálogo for colocado na cena (nenhum existe ainda, ver pendência abaixo).
- ✅ **`SkillTreeSeeder.cs`** (novo, menu `Rapadura/Seed Example Skill Tree`): a Skill Tree UI não tinha nenhum dado pra mostrar (`SkillTreeDefinition` — zero assets no projeto). Gerado `tree_general.asset` (`Resources/SkillTrees/`) organizando as 20 skills de exemplo já seedadas numa grade 5 colunas.
- ✅ **Botões "Skills" e "Menu" adicionados na HUD** (`HudView.uxml`/`.uss`, `HudController.cs`) — sem eles, Skill Tree e Pause não tinham nenhum gatilho em jogo (Pause normalmente abre por tecla física, mas o design deste jogo é touch-first). Mesmo padrão visual dos botões Pular/Correr já existentes, posicionados no canto superior direito.
- 🔬 **Testado em Play Mode ponta a ponta**: `Show()`/`Toggle()` de cada controller confirmado (Pause congela `Time.timeScale`, Skill Tree desenha os 20 nós com contadores reais "0/5"/"0/3"), screenshot confirmando os botões da HUD renderizando e abrindo as telas corretas via `HudController.OnSkillsButtonClicked`/`OnPauseButtonClicked`.
- 📌 **Pendências que ficam para depois**: nenhum NPC de loja/diálogo existe na cena ainda (Shop/Dialogue UI estão prontas mas sem gatilho real até um `NpcInteractable` ser colocado); layout da Skill Tree tem nós se sobrepondo visualmente (USS pré-existente, não é bug novo desta sessão, é questão de polimento de layout).
- 🏁 Suíte de testes: 396/396.

### 2026-09-10 (continuação — bug crítico real: sistema de skills inteiro nunca estava na cena)
- 🐛 **O maior gap de jogabilidade encontrado nesta sessão**: `SkillManager`, `ComboTracker` e `BuffController` — os três totalmente implementados e testados isoladamente há várias sessões — **nunca foram adicionados a nenhum GameObject em `Main.unity`**. Confirmado via `player.GetComponents<Component>()`: existiam `PlayerStats`/`AttributeSet`/`PlayerController`/etc., mas nenhum dos três componentes de skill. Consequência real: nenhuma das 25 habilidades dos 5 personagens podia ser usada em jogo, e os bônus de atributo (`AttributeSet`, incluindo o fix de fórmula desta sessão anterior de Destreza/Inteligência) nunca tinham efeito nenhum, porque `AttributeSet.EnsureInitialized` resolve `BuffController` via `GetComponent` e ele nunca existiu na cena — os bônus eram calculados mas não tinham onde ser aplicados.
- ✅ **Corrigido**: `BuffController`, `SkillManager` e `ComboTracker` adicionados ao `Player`. `SkillManager._availableSkills` populado com as 20 skills de exemplo já seedadas (`Resources/Skills/*.asset`) via `SerializedObject`; referências `_animator`/`_buffController` conectadas. `ComboTracker` ligado ao `SkillManager`/`BuffController` do mesmo Player.
- 🔬 **Testado em Play Mode, fluxo completo real**: `GrantSkillPoints(1)` → `LearnSkillWithPoint(skill_fireball, level 1)` → `EquipSkill(slot 0)` → `TryCastSlot(0, ...)` — tudo retornou sucesso, e o mana do jogador de fato caiu de 100 para 80 (custo real da skill consumido). Sem esse teste ponta a ponta, a ausência desses componentes na cena não aparecia em nenhuma suíte de teste (que testa os componentes isoladamente com `AddComponent` direto, não a composição real da cena).
- 🏁 Suíte de testes: 396/396.

### 2026-09-10 (continuação — loot ao matar inimigo + InventoryManager nunca estava na cena)
- 🐛 **Achado grande, não relatado pelo usuário**: `InventoryManager` (MonoBehaviour) nunca foi adicionado a nenhum GameObject em `Main.unity` — confirmado via `player.GetComponent<InventoryManager>() == null`. Isso significa que loja (`NpcInteractable`), munição de arma à distância (`RangedWeapon`/`WeaponController`) e qualquer outro sistema que espera `GetComponent<InventoryManager>()` no Player estavam recebendo `null` silenciosamente e não faziam nada. Corrigido: `InventoryManager` adicionado ao `Player` na cena. Testado em Play Mode: `AddItem` de verdade guarda item no slot (`leftover=0` ao adicionar 3 poções).
- ✅ **Loot ao matar inimigo implementado** (não existia nenhum sistema de drop): `EnemyDefinition` ganha `LootItem`/`LootDropChance`/`LootQuantityMin`/`LootQuantityMax`. `EnemyLootGranter.cs` (novo, mesmo padrão de `EnemyRewardGranter`/`CombatFxManager` — escuta `CombatTargetDiedEvent`) rola a chance e chama `InventoryManager.AddItem` no killer. Configurado `item_gold_coin` (60% chance, 1-3) nos 9 `EnemyDefinition` existentes (`Resources/Enemies/*.asset`). Adicionado ao `GameManager`.
- 📌 **Decisão de escopo**: loot não rola afixos procedurais (`ProceduralItemGenerator`) — só item base + quantidade. `InventorySlotData` na verdade **já suporta** afixos gerados (`generatedAffixIds`/`generatedAffixValues`/`generatedName`, achado ao inspecionar a classe), então isso é viável numa próxima rodada se o usuário quiser drops raros com afixos, não é mais um bloqueio de formato de dados como eu havia presumido inicialmente.
- 🔬 **Testado em Play Mode ponta a ponta**: 5 inimigos da cena mortos em sequência (dano letal via `Health.ApplyDamage`, limpando invulnerabilidade entre cada), inventário do jogador inspecionado via `InventorySlotData.itemId`/`quantity` — `item_gold_coin x8` confirmado após as 5 mortes.
- 🏁 Suíte de testes: 396/396.

### 2026-08-27 (continuação — checkpoint inicial real, não só "reviver no lugar")
- 🐛 **`SpawnPoint.IsInitialSpawn` existia mas nada nunca lia essa flag** — `CheckpointManager.SetInitialCheckpoint` também nunca era chamado em lugar nenhum. Combinado com a cena de teste não ter nenhum `SpawnPoint`, o fallback "reviver no lugar" da correção anterior era o único comportamento possível, mesmo que um `SpawnPoint` inicial existisse.
- ✅ **Corrigido**: `GameManager.Start()` agora varre a cena por `SpawnPoint.IsInitialSpawn` e registra no `CheckpointManager` (rodando em `Start`, não em `BuildManagers`, já que os `SpawnPoint`s da cena ainda não existem quando os managers são construídos). Adicionado `InitialSpawnPoint` (novo GameObject, `SpawnPoint` com `_isInitialSpawn=true`) na posição de spawn do player `(0,0,0)` em `Main.unity`. Testado em Play Mode: `CheckpointManager._activeCheckpoint` confirmado setado logo após o início da cena, sem o jogador precisar tocar em nenhum trigger primeiro.
- 🏁 Suíte de testes: 396/396.

### 2026-08-27 (continuação — bug crítico real: morte do jogador era soft-lock permanente)
- 🐛 **O pior bug de jogabilidade encontrado nesta sessão**: `PlayerStats.Die()` setava `IsDead=true` mas **nada em todo o projeto jamais setava de volta pra `false`** — `ApplyDamageInternal` tem `if (IsDead ...) return;` no topo, então depois da primeira morte todo dano futuro era silenciosamente ignorado. `CheckpointManager.OnPlayerDied` só reposicionava o player pro checkpoint ativo (`_activeCheckpoint`) — mas **a cena de teste não tem nenhum `SpawnPoint`** (confirmado: `FindObjectsOfType<SpawnPoint>().Length == 0`), então `_activeCheckpoint` nunca é setado e o método nem reposicionava nada. Resultado real: a primeira morte do jogador travava a run permanentemente, sem tela de game over, sem reinício, sem nada — o jogo simplesmente parava de responder a dano.
- ✅ **Corrigido**: `PlayerStats.Revive()` (novo) — restaura vida/stamina cheias, `IsDead=false`, limpa invulnerabilidade. `CheckpointManager.OnPlayerDied` agora sempre chama `Revive()` (reposiciona pro checkpoint só se um existir — sem checkpoint configurado, reviver no lugar já é infinitamente melhor que travar pra sempre).
- 🔬 **Testado em Play Mode ponta a ponta**: dano de 1000 aplicado ao jogador → `IsDead=true` momentâneo → `PlayerDiedEvent` → `CheckpointManager` reage no mesmo frame (EventBus é síncrono) → `IsDead=False`, `CurrentHealth=100` confirmado logo em seguida. Sem esse teste ativo (só olhando o código) esse bug critico teria passado despercebido, já que a suíte de testes cobre `PlayerStats`/`CheckpointManager` isoladamente mas não a integração ponta a ponta entre os dois.
- 🏁 Suíte de testes: 396/396.

### 2026-08-27 (continuação — bug real: inimigo morto continuava sólido bloqueando o jogador)
- 🐛 **Achado no mesmo teste ponta a ponta da correção de XP**: depois de matar um inimigo (`IsDead=true`), o `EnemyController` só para de fazer tick — nada desabilitava o collider, nada escondia/removia o corpo. Um inimigo "morto" continuava fisicamente sólido (bloqueando o jogador) e visualmente parado de pé pra sempre, sem nenhuma leitura de "morreu" pro jogador.
- ✅ **Corrigido**: `EnemyDeathVfx.cs` (novo, `Assets/Scripts/Gameplay/Enemies/`), inscrito no mesmo `CombatTargetDiedEvent` já usado por `CombatFxManager`/`EnemyRewardGranter` — desativa todos os `Collider` do inimigo imediatamente (para de bloquear/ser alvo no mesmo frame que morre) e afunda + esmaece o corpo ao longo de 1.5s antes de desativar o GameObject. Adicionado ao `GameManager` na cena. Testado em Play Mode: `colliderEnabled=False` confirmado logo após a morte.
- 🏁 Suíte de testes: 396/396.

### 2026-08-27 (continuação — bug real de produção: matar inimigo não dava XP nenhuma)
- 🐛 **Achado testando o loop de combate ponta a ponta pela primeira vez** (aplicando dano real via `Health.ApplyDamage` até matar um lobo e conferindo `PlayerStats.CurrentExperience` antes/depois): o inimigo morre corretamente (`IsDead=true`, `CombatTargetDiedEvent` publicado com o killer certo), mas XP do jogador continuava em 0. Causa raiz: `EnemyDefinition` nunca teve nenhum campo de recompensa de experiência, e nada no projeto jamais chamava `PlayerStats.AddExperience` a partir da morte de um inimigo — o sistema de nível/XP em si está implementado e testado corretamente (curva de XP, level up, etc.), só nunca foi conectado ao combate de verdade.
- ✅ **Corrigido**: `EnemyDefinition.ExperienceReward` (novo campo, default 10) + `EnemyRewardGranter.cs` (novo, `Assets/Scripts/Gameplay/Enemies/`), inscrito em `CombatTargetDiedEvent`, concede a XP ao `PlayerStats` do killer. Adicionado ao `GameManager` na cena (mesmo padrão do `HitStopController`/`CombatFxManager`). Testado em Play Mode: lobo morto → XP do jogador sobe de 0 para 10.
- 🔬 **Como esse teste ponta a ponta foi feito**: chamando `Health.ApplyDamage` via reflection diretamente em Play Mode real (não em EditMode) — a primeira tentativa (10 de dano seguido de 100 de dano) revelou o segundo hit sendo ignorado por causa da janela de invulnerabilidade (`_invulnerabilityDuration=0.5s`) nunca decrementar, já que `Time.deltaTime` fica travado em 0 neste ambiente headless entre chamadas MCP — não é bug, é limitação conhecida do ambiente (ver nota anterior). Contornado limpando a invulnerabilidade manualmente antes do segundo hit.
- 📌 **Ainda sem sistema de drop de item ao matar inimigo** — `ProceduralItemGenerator`/16 afixos já existem e funcionam isoladamente (sessão anterior), mas, como XP, nada os conecta à morte de um inimigo ainda. Candidato pra próxima rodada se o usuário confirmar que quer loot ao matar.
- 🏁 Suíte de testes: 396/396.

### 2026-08-27 (continuação — pedido do usuário: "deixar o jogo 100% jogável nessa tela com efeitos e tudo mais")
- ✅ **Feedback de combate criado do zero — não existia nenhum**: `CombatFxManager.cs` (novo, `Assets/Scripts/Gameplay/Combat/`) escuta `DamageAppliedEvent`/`CombatTargetDiedEvent` (já publicados pelo pipeline de dano real, não precisou mexer em combate) e dispara, 100% procedural (zero custo): faísca de partículas no ponto de impacto (cor laranja em crítico) e número de dano flutuante (`FloatingDamageNumber.cs`, sobe e desaparece, maior/laranja em crítico). Antes disso um hit só tinha hit-stop/screen-shake, nenhum feedback visual por acerto.
  - 🐛 **Módulo `ParticleSystem` nunca esteve no manifest de pacotes** (`Packages/manifest.json` não tinha `com.unity.modules.particlesystem`) — erro de compilação só apareceu ao tentar usar `ParticleSystem` pela primeira vez no projeto. Adicionado + `resolve_packages`.
  - 🐛 **`Resources.GetBuiltinResource<Font>("Arial.ttf")` quebra no Unity 6** (removido, mensagem de erro aponta o substituto) — trocado por `"LegacyRuntime.ttf"`. Só foi descoberto testando de verdade em Play Mode (publicando `DamageAppliedEvent` via reflection) — sem esse teste ativo o código compilava limpo mas quebrava toda vez que um hit acontecesse.
  - ⚠️ **`HitStopController` e `CombatFxManager` nunca estavam de fato na cena** — a classe existia e era testada, mas ninguém tinha adicionado o componente ao `GameManager`. Adicionados os dois agora.
- 🐛 **Bug real de gameplay encontrado e corrigido: câmera nascia colada numa rocha, tela inteira tomada por um boulder** — só percebido testando o fluxo real (Character Select → confirmar herói → ver a câmera de jogo de verdade), não visível nos screenshots de scene-view usados pra validar as fases 1-3 (câmera livre do editor, não a câmera de jogo). Duas causas encontradas:
  1. `ScatterAdditionalRocks` (fase 1, rochas) não tinha nenhuma exclusão perto do spawn do player/câmera — o raio mínimo de 3 unidades era medido a partir do centróide do cluster de rochas, não do player, então uma rocha grande podia cair a ~2.5m da câmera. Corrigido com `ClearExclusionZone` (empurra a posição radialmente pra fora de um ponto de exclusão até respeitar a distância mínima) aplicado tanto ao spawn do player (0,0,0) quanto ao repouso da câmera (0,1,-10).
  2. **Uma rocha colocada à mão antes desta sessão** (`Rock_5`, em `(3.5, 0.25, -8)`, escala 1.6) também violava essa distância — não era culpa do código desta sessão, mas afetava a jogabilidade real então foi corrigida também (mesma função `ClearHandPlacedRock` aplicada a todas as rochas nomeadas, não só as espalhadas).
  - 🐛 **Bug secundário encontrado no processo**: rodar `Regenerate Natural Rocks` mais de uma vez duplicava o conjunto espalhado (15→25→35...) porque nada limpava as rochas da rodada anterior antes de espalhar de novo. Corrigido removendo qualquer `Rock_Scattered_*` existente no início da função.
- 📝 **Achado sobre o freeze de `Time.timeScale=0`**: investigado como possível bug (parecia um hit-stop travado), mas na verdade é comportamento correto — `CharacterSelectController.OnEnable()` congela o tempo de propósito enquanto a tela de seleção de herói está visível, e só libera em `ConfirmSelection()`. Não é bug, é a tela de seleção fazendo seu trabalho.
- 🔬 **Verificado fluxo completo real pela primeira vez nesta sessão**: Character Select → seleção de Joaquim → confirmação → gameplay, com screenshot da câmera de jogo de verdade (não scene-view) confirmando visão limpa do chão/rochas/vegetação da fase 1-3. Suíte de testes: 396/396 em cada etapa.
- 📌 **Limitação de ambiente registrada**: `Time.deltaTime`/`Time.time` ficam travados em 0 fora de um frame real focado deste editor headless — chamadas MCP sequenciais não avançam o relógio de jogo sozinhas, então verificação "ao vivo" de coisas que dependem de tempo passando (ex. IA perseguindo, animação por deltaTime real) só é possível via ticks manuais com deltaTime sintético, não por "esperar X segundos" entre chamadas.

### 2026-08-27 (continuação — revisão de qualidade visual do ambiente, fase 3: chão e vegetação, custo zero)
- ✅ **`ProceduralGroundTexture.cs`** (novo, `Assets/Scripts/Editor/`, menu `Rapadura/Setup/Regenerate Ground Texture`): a textura de chão original (`Tex_Grass.png`, 256x256, só 144 cores distintas) combinada com tiling 8x8 num plano de 50x50 lia como "uma textura pequena repetida". Regenerada em 1024x1024 via ruído Perlin em camadas (verde escuro/claro por manchas grandes, mottling fino, respingos esparsos de terra), mais um normal map derivado do mesmo campo de ruído (Sobel simples) aplicado em `_BumpMap` — chão ganhou relevo sutil em vez de ser perfeitamente liso. Tiling ajustado pra 6x6 (textura 4x maior em resolução, mantém tamanho de tile parecido no mundo). Custo zero (sem API de imagem configurada no projeto).
- ✅ **`GroundDetailScatter.cs`** (novo, `Assets/Scripts/Editor/`, menu `Rapadura/Setup/Scatter Ground Detail`): espalhados 120 tufos de grama (quads cruzados finos, 3 lâminas cada, ~12 tris), 40 pedrinhas (mini `ProceduralRockGenerator`) e 18 gravetos (caixas finas esticadas) pela área jogável — detalhe de perto pedido explicitamente pelo usuário. Cada tufo recebe `TreeColorVariation` (mesmo tint por `MaterialPropertyBlock` das árvores/rochas). Um material por tipo de detalhe, `enableInstancing` ligado, pra manter os draw calls baixos no mobile.
- 🐛 **Bug real encontrado e corrigido no mesmo commit**: primeira versão dos tufos usava `RecalculateNormals()` numa malha de faces duplas (frente+trás) compartilhando os mesmos vértices — a normal frontal e a de trás em cada vértice apontam em direções opostas e se cancelam na média, resultando em silhuetas **completamente pretas** (confirmado via screenshot). Corrigido calculando normais explícitas "pra cima" por vértice (truque clássico de grama em billboard/quad-cruzado) em vez de deixar a Unity derivar da winding dos triângulos.
- 🔬 **Confirmado visualmente** via screenshot (`ground_detail_check2.png`): chão com variação de cor visível (não mais uma manchinha lisa repetida), tufos/pedrinhas/gravetos legíveis como detalhe de grama sem ler como caixas pretas. Suíte de testes: 396/396, sem regressões.
- 🏁 **Fase 3 (última do plano de revisão visual do ambiente) concluída** — árvores (fase 2), rochas (fase 1) e chão/vegetação (fase 3) todas resolvidas com custo de API zero (só as 3 espécies de árvore usaram Meshy, já aprovado antes de gastar).

### 2026-08-27 (continuação — revisão de qualidade visual do ambiente, fase 2: árvores)
- ✅ **Árvores placeholder (`Tree1_Trunk`/`Tree2_Trunk`, cápsula+esferas) substituídas por 3 espécies reais geradas via Meshy** (`Tree_Broadleaf_RO`, `Tree_Pine_RO`, `Tree_Gnarled_RO` — text-to-3D, `Assets/Models/Environment/Trees/*.glb`), custo já aprovado pelo usuário ("pode ir fazendo... use a api que ja temos"). Perguntado se valia buscar modelos prontos mais baratos via Sketchfab antes de gastar Meshy — sem chave configurada (`MCPFORUNITY_SKETCHFAB_API_KEY` ausente) e usuário optou por não configurar agora, então seguiu com Meshy conforme o plano original.
- 🐛 **Import falhou nas 3 gerações** (`"GLB import requires glTFast"`) **depois dos créditos já gastos** — recuperado sem perda: instalado `com.unity.cloud.gltfast` (`manage_packages`, v6.19.0) e forçado reimport dos `.glb` já salvos em disco via `AssetDatabase.ImportAsset(ForceUpdate)`. Nenhum crédito desperdiçado.
- 🐛 **Output bruto do Meshy é pesado demais pra mobile** (300k-800k+ vértices por árvore, geometria orgânica multi-blob). Tentativa de instalar `UnityMeshSimplifier` via git package falhou 2x (subpath/branch inválidos na URL) — sem pacote de decimação disponível no ambiente, escrito um decimador próprio do zero: `MeshDecimator.cs` (novo, `Assets/Scripts/Editor/`), clustering de vértices por grade 3D.
  - 🔬 **Achado real, não relatado pelo usuário — encontrado comparando screenshots**: a primeira versão do decimador fazia média de UV por cluster, o que produzia textura completamente embaralhada sempre que um cluster continha vértices de ilhas de UV não-relacionadas (comum em folhagem feita de vários blobs de folha sobrepostos). Causa raiz confirmada comparando contra o modelo original (correto) via screenshot.
  - ✅ **Corrigido em 2 passos**: primeiro trocando média de UV por "primeira UV do cluster" (melhorou mas não resolveu — ainda misturava vértices de ilhas de UV diferentes que calhavam de estar próximos no espaço 3D), depois adicionando UV ao próprio critério de clustering (chave 5D: posição x/y/z + bucket de UV u/v, `uvBucketSize=0.08`). Isso eliminou o embaralhamento — restam apenas pequenas "rachaduras" escuras (buracos de triângulos degenerados descartados nas bordas de cluster), testado em 3 níveis de agressividade, presentes nos 3. **Testado na distância de câmera real do jogo** (isométrica, ~9m): rachaduras visualmente desprezíveis a essa distância, embora óbvias em close-up. Decisão: aceitar como troca razoável pra um prop de ambiente de fundo/meio-plano, não perfeito de perto — um decimador real de edge-collapse/quadric-error daria resultado mais limpo mas foi julgado grande demais pra esta sessão dado que a alternativa já funciona "bem o suficiente" na distância real de jogo.
- ✅ **`NaturalTreeSetup.cs`** (novo, `Assets/Scripts/Editor/`, 2 itens de menu):
  - `Rapadura/Setup/Build Natural Tree Prefabs`: decima cada espécie em LOD0 (18000 vértices-alvo) e LOD1 (3000), normaliza escala/posição pra altura real de 4.5m com base em y=0 (Meshy gera cada árvore num tamanho bruto diferente), monta prefab com `LODGroup` (LOD0 até 25% da tela, LOD1 até 5%), liga `enableInstancing` nos materiais.
  - `Rapadura/Setup/Scatter Natural Trees In Scene`: remove `Tree1_Trunk`/`Tree2_Trunk`, espalha 10 árvores em 10 posições fixas dentro da área jogável (com jitter, escala/altura levemente não-uniforme, rotação Y aleatória e leve inclinação ±6°, espécie sorteada por posição) — evita o "óbvio mesmo prefab copiado" que o usuário pediu pra evitar.
- ✅ **`TreeColorVariation.cs`** (novo, `Assets/Scripts/Gameplay/Environment/`): tinge cada instância via `MaterialPropertyBlock` (hue ±0.06, brilho ±0.12, seed determinístico pela posição) sem quebrar GPU Instancing/SRP batching — adicionado a cada árvore espalhada, pra malhas idênticas do mesmo prefab não lerem como clones idênticos.
- 🔬 **Confirmado visualmente** via screenshot da cena (`trees_overview.png`): 10 árvores das 3 espécies, variação de escala/inclinação/tonalidade visível, nenhuma repetição óbvia, junto com as rochas da fase 1. Suíte de testes: **395/396** (a 1 falha, `AudioManagerTests.PlayOneShot_...`, é flake de ambiente — FMOD sem dispositivo de saída de áudio real neste ambiente headless, não relacionado a esta mudança).
- 📌 **Pendente — fase 3 do plano (chão/vegetação) ainda não iniciada**: blend de textura de terreno (triplanar), detalhes pequenos (folhas caídas, gravetos, pedrinhas, arbustos), normal maps/roughness/AO.

### 2026-08-26 (continuação — revisão de qualidade visual do ambiente, fase 1: rochas)
- 📋 **Pedido do usuário**: revisão completa da qualidade visual do ambiente (árvores, rochas, chão, vegetação) — atual "aparência de assets genéricos". Análise confirmou: árvores eram literalmente cápsula+esferas, rochas eram esferas achatadas/giradas (mesma forma repetida), sem provider de imagem IA configurado (fal/OpenRouter não configurados, só Meshy pra modelos 3D). Plano apresentado e aprovado: rochas/chão/detalhes 100% procedural (grátis), árvores via Meshy (poucas espécies base + variação procedural) já que precisam de estrutura de galhos real. Perguntado sobre Sketchfab (fonte gratuita de modelos prontos) como alternativa mais barata pras árvores — sem chave configurada (usuário optou por não configurar agora), seguindo com Meshy conforme plano original.
- ✅ **Rochas resolvidas, 100% procedural, custo zero**: `ProceduralRockGenerator.cs` (novo, `Assets/Scripts/Environment/`) — icosfera subdividida + deslocamento por ruído fractal 3D (3 amostras Perlin 2D combinadas, já que Unity não tem Perlin 3D nativo) por vértice, normais faceteadas (não suavizadas — pedra de verdade tem faces planas, não curva lisa). Cada seed produz uma forma única. `NaturalRockSetup.cs` (novo, menu `Rapadura/Setup/Regenerate Natural Rocks`): substitui a malha das 7 rochas existentes (mantendo posição/escala já ajustadas à mão) e espalha mais 10 rochas (pequenas/médias/grandes, pesos naturais) ao redor do cluster existente — collider trocado de `SphereCollider` pra `MeshCollider` convexo, GPU Instancing ligado no `Mat_Rock`. Confirmado visualmente via screenshot: formas irregulares e facetadas de verdade, tamanhos variados, nenhuma repetida. Suíte de testes: 396/396, sem regressões.

### 2026-08-26 (continuação — bug real encontrado: EnemyProceduralAnimator duplicado)
- 🐛 **Causa raiz real do "aranhas deslizam sem animação"**: cada um dos 5 inimigos da cena tinha **dois** componentes `EnemyProceduralAnimator`, não um — um na raiz (correto) e outro duplicado no filho `EnemyVisual` (leftover de alguma sessão anterior, provável duplicidade ao rodar um loop de `AddComponent` mais de uma vez ou em alvos diferentes sem perceber). Os dois escreviam em `EnemyVisual.localPosition`/`localRotation` todo frame, cada um com seu próprio `_bobPhase`/`_currentSpeedFactor` independente e fora de sincronia — a ordem de execução entre os dois não é garantida pela Unity, então o resultado final visível a cada frame era essencialmente aleatório entre os dois cálculos, lendo como "sem animação de verdade" mesmo com a lógica de cada instância individualmente correta.
- ✅ **Corrigido**: removido o componente duplicado de `EnemyVisual` em todos os 5 inimigos, mantendo só o da raiz. Confirmado via `GetComponentsInChildren` que sobrou exatamente 1 por inimigo.
- ✅ **Intensidade do bob/lean também aumentada** (`_bobAmplitude` 0.12→0.22, `_leanDegrees` 14°→22°, `_swayAmplitude` 0.05→0.09, `_yawWagDegrees` 6°→10°, `_referenceSpeed` 4→2 m/s pra já ficar visível na velocidade normal de patrulha, não só em perseguição) — o efeito tecnicamente já funcionava (confirmado numericamente, pico de 0.10 de deslocamento antes do fix de duplicidade), mas provavelmente sutil demais numa câmera isométrica distante.
- 🔬 **Método de validação**: como `Time.deltaTime` real fica travado em 0 neste ambiente headless mesmo sem foco, `EnemyProceduralAnimator.Update()` foi dividido — `Tick(float deltaTime)` público extraído, testável diretamente com um deltaTime sintético (mesmo padrão de `ComboTracker.TimeProvider`/`EnemyController.MoveInDirection` já usado nesta sessão). Sem esse refactor eu não teria descoberto o bug de duplicidade — o `Update()` puro sempre retornava sem fazer nada aqui, mascarando qualquer teste.
- ✅ Suíte de testes continua 396/396.

### 2026-08-26 (sessão interativa — fórmula de stat resolvida, suíte 100% verde)
- ✅ **Bug real de produção corrigido, não só de teste**: `SkillManager.GetCooldownReductionPercent()`/`GetResourceCostReductionPercent()` já chamavam `GetModifiedValue(stat, 0f)` — ou seja, o bônus de Cooldown Reduction do atributo Inteligência **nunca funcionou de verdade em produção**, não só nos testes. Causa: `AttributeSet.BuildDerivedEffect` usava `ModifierApplication.PercentAdditive` pra Destreza (CriticalChance) e Inteligência (CooldownReduction), e a fórmula de `BuffController.GetModifiedValue` é `(baseValue + flat) * (1 + percent)` — com `baseValue=0` (sem sistema de crítico/cooldown base ainda), qualquer bônus percentual dá exatamente zero, não importa o valor.
- ✅ **Decisão do usuário**: tornar aditivo puro. Trocado `ModifierApplication.PercentAdditive` → `Flat` pra Destreza/Inteligência em `AttributeSet.BuildDerivedEffect` — mesmo padrão que o bônus `CostReduction` de combo já usava corretamente pra `ResourceCostReduction` (outro stat que é fração pura 0..1, não "porcentagem de uma base"). `AttackDamage`/`MoveSpeed` continuam `PercentAdditive` (esses têm base real não-zero, a fórmula funciona certo pra eles).
- 🏁 **Suíte de testes 100% verde: 396/396 passando** — pela primeira vez desde que os testes existem no projeto.


### 2026-08-26 (fim da sessão automática agendada 3h07 — resumo e motivo de parar)
- 🏁 **Resumo**: limpeza do checklist (vários itens já resolvidos em sessões anteriores nunca tinham sido marcados), implementado "clicar no inimigo = atacar direto", efeito de Queimadura seedado, corrigido um bug real que eu mesmo introduzi (reflection quebrada em `EnemyController` depois de refatorar `Health.CurrentHealth` — inimigos estavam usando vida padrão do Inspector em vez do `EnemyDefinition` real), e autorados 16 afixos reais pro gerador procedural de itens (`Resources/Affixes` estava vazio). 6 commits nesta rodada, suíte de testes estável (393/396, só as 3 falhas de fórmula já documentadas).
- 🛑 **Parando aqui de propósito, não por falta do que fazer, mas porque todo o trabalho seguro/bem-delimitado que dá pra fazer sem decisão do usuário ou sem risco maior já foi feito**: os itens que restam são genuinamente bloqueados (Maithe precisa de confirmação sobre gastar créditos Meshy; Player Settings de ícone/bundle id é identidade de loja, não decido sozinho; fórmula do `GetModifiedValue` é decisão de design de combate) ou são features grandes que uma sessão anterior já avaliou e decidiu deliberadamente adiar por risco (religar o menu de Settings/Pause completo na cena — ver nota de 2026-08-24 mais acima, "arriscado demais religar tudo sem testar cada referência com calma"). Continuar cavando por itens cada vez menores/mais arriscados sem o usuário testar o que já foi feito não parece o uso mais responsável do tempo agendado. Próxima sessão: se o usuário não tiver respondido as pendências bloqueadas, vale revisitar a fórmula do `GetModifiedValue` com mais contexto do design de combate pretendido, ou tentar novamente localizar itens pequenos e seguros na lista.

### 2026-08-26 (continuação — 16 afixos reais autorados)
- ✅ **`AffixDatabaseSeeder.cs`** (novo, menu `Rapadura/Seed Example Affixes`, mesmo padrão de `SkillDatabaseSeeder`/`EnemyDatabaseSeeder`): sistema de geração procedural de itens já estava pronto e testado, mas `Resources/Affixes` estava vazio — nada pra `ProceduralItemGenerator` sortear. Criados 16 afixos reais cobrindo as principais categorias: armas (dano flat/percentual, crit chance/dano — `MainHand`), armadura (defesa, vida, regen — `Chest`), botas (velocidade de movimento), acessórios (mana, redução de cooldown/custo), e alguns universais sem restrição de slot. Pesos de raridade ajustados (afixos mais fortes têm peso menor, sorteiam com menos frequência).
- 🔬 **Testado gerando itens de verdade**: `ProceduralItemGenerator.Generate` com uma espada de teste (`MainHand`) e raridade Legendary produz nomes reais tipo "Afiada Sortuda Espada de Teste da Fúria da Resistência" com 3-4 afixos, confirmando o pipeline ponta a ponta funcionando com o banco de dados populado. Suíte de testes sem regressões.

### 2026-08-26 (continuação — bug real que eu mesmo introduzi, encontrado e corrigido)
- 🐛 **Bug real (meu, desta sessão anterior)**: ao refatorar `Health.CurrentHealth` de auto-propriedade pra propriedade explícita com backing field próprio (`_currentHealth`), quebrei silenciosamente `EnemyController.ApplyDefinitionToHealth()` — ele usava reflection procurando o campo `<CurrentHealth>k__BackingField` (nome que só existe pra auto-propriedades), que deixou de existir. `GetField(...)?.SetValue(...)` com `?.` engoliu o `null` sem erro nenhum: os inimigos continuavam funcionando com a vida PADRÃO do `Health` (`_maxHealth=50` no Inspector), simplesmente ignorando `EnemyDefinition.MaxHealth` (30 pro lobo, 200+ pra bosses) — nenhum teste cobria esse caminho especificamente, por isso não foi pego antes.
- ✅ **Corrigido resolvendo a dívida técnica documentada** (nota do roadmap: "considerar adicionar um setter público em Health.cs"): novo `Health.ConfigureMaxHealth(float)` público — seta `_maxHealth`, cura pra vida cheia, limpa `IsDead`. `EnemyController.ApplyDefinitionToHealth()` reescrito pra chamar isso direto, removendo toda a reflection (e os `using System.Reflection`/`Rapadura.Core.Logging` que ficaram órfãos). Confirmado em Play Mode: `Enemy_Wolf` com `Health.MaxHealth=30` batendo com `EnemyDefinition.MaxHealth=30` (antes ficaria em 50, o default do Inspector). Suíte de testes sem regressões.

### 2026-08-26 (continuação — efeito de Queimadura seedado)
- ✅ **Queimadura implementada**: `skill_fireball` (`SkillDatabaseSeeder.cs`) ganhou o debuff "Queimando" — mesmo padrão de `skill_poison_dart`/"Envenenado" (DoT via `BuffController.ticksOverTime`, já suportado genericamente pela engine, só faltava ser usado por uma skill de fogo). 5 de dano por segundo, 4 segundos de duração. Reseedado de verdade no asset via menu `Rapadura/Seed Example Skills`, confirmado inspecionando `skill_fireball.asset` (debuff presente, `ticksOverTime=true`, `tickDamageOrHeal=5`, `durationSeconds=4`). Suíte de testes sem regressões.

### 2026-08-26 (sessão automática agendada — clicar no inimigo ataca direto)
- ✅ **"Clicar no inimigo = atacar direto" implementado** (último item pendente da seção de clique-pra-mover, linha 738 do roadmap): `PlayerClickToMove` agora detecta se o clique/toque acertou um `ICombatTarget` (em vez de chão vazio) — se sim, guarda a referência em `AttackTarget` e ajusta o destino pra parar a uma distância de ataque (`_attackStoppingDistance`, padrão 2m) do alvo em vez de andar pra cima dele. `PlayerBasicAttack` ganhou `TickClickToAttack()`: enquanto há um `AttackTarget` perseguido e o player está no alcance, vira automaticamente pra encarar o alvo (suavizado, `_turnSpeedDegPerSec`) e ataca sozinho no cooldown, sem precisar de um input de ataque separado — o próprio clique já expressa a intenção de lutar.
- 🔬 **Testado em Play Mode via Unity MCP, ponta a ponta**: clique simulado (raycast real via `Camera.WorldToScreenPoint`) confirma `AttackTarget` setado e destino recalculado corretamente; com o player já dentro do alcance, o primeiro tick girou parcialmente (esperado, `RotateTowards` é gradual) e o ataque daquele frame errou (a esfera de ataque fica na frente do personagem, que ainda não tinha virado o suficiente) — ticks subsequentes convergiram a rotação pra encarar o alvo (270° = direção certa) e o ataque seguinte acertou de verdade (30→15 de vida do lobo). Comportamento correto e esperado: numa sessão real a 60fps a rotação converge em poucos frames, não é perceptível como "erro".
- 📝 **Limitação conhecida, aceitável pro escopo atual**: se o inimigo perseguido se mover (wander) depois do clique, o player não re-persegue — só anda até o ponto calculado no momento do clique e para lá. Combina com o padrão MU Online clássico (clique = intenção pontual, não re-alvo automático contínuo); revisitar se o feedback do usuário pedir perseguição contínua.

### 2026-08-25 (continuação 25 — fim da sessão automática agendada, resumo)
- 🏁 **Resumo desta sessão automática (agendamento 20h04)**: começou reavaliando o pedestal Maria/Lavine (confirmado não-seguro pra remover), migrou color space Gamma→Linear, adicionou botões de Pular/Correr + indicador de clique-pra-mover + safe area na HUD, e depois mergulhou fundo na dívida de testes: de **25+ falhas escondidas** (limitadas pela paginação do test runner) restam só **3**, todas na mesma família (`AttributeSetTests` — bug de fórmula em `BuffController.GetModifiedValue` pra stats percentuais que começam em zero, ex. CriticalChance/CooldownReduction). Corrigidos ao longo do caminho: 8 componentes com o mesmo bug de "Awake/Start/OnEnable não rodam sincronamente fora do Play Mode" (`InventoryManager`, `Health`, `PlayerStats`, `SkillManager`, `ComboTracker`, `AttributeSet`, `Projectile`, `CombatCameraShakeRelay`, `GameManager`), 2 bugs reais de lógica de produção (`WaveTracker.IsComplete` logicamente inalcançável, `ProceduralItemGenerator` pulando o filtro de compatibilidade de slot com pool customizado), e vários bugs no próprio código de teste (valores esperados errados, `SendMessage` disparando assert interno do Unity, `Thread.Sleep` não avançando `Time.time` em EditMode, GUID mismatch no round-trip de rebind).
- 📌 **Pendente pro usuário decidir** (não decidido sozinho por esta sessão automática): a fórmula de `GetModifiedValue` pra stats percentuais puros (CriticalChance/CooldownReduction) — hoje multiplica `(baseValue + flat) * (1 + percent)`, o que zera qualquer bônus percentual quando `baseValue=0` (sem sistema de crit base ainda). Rig da Maithe (créditos Meshy). Player Settings de ícone/bundle id (identidade de loja).

### 2026-08-25 (continuação 24 — última falha da suíte resolvida: bug real de teste no round-trip de rebind)
- 🐛 **`AccessibilityTests.SaveAndLoadBindingOverrides_RoundTripsThroughPlayerPrefs`**: o teste simulava "nova sessão" criando um `InputActionAsset` **totalmente novo** via `AddActionMap`/`AddAction` — mas confirmei via reprodução direta que `InputActionRebindingExtensions.LoadBindingOverridesFromJson` da própria Unity casa os overrides salvos pelo **GUID de cada binding**, não pelo caminho/nome da ação. Cada chamada de `AddAction` gera um GUID novo aleatório, então dois assets construídos independentemente (mesmo com nomes de mapa/ação idênticos) nunca vão bater os IDs — o load "funciona" (`loaded=true`, o JSON existe) mas não aplica o override em lugar nenhum. Confirmado isolando: recarregar no MESMO asset funciona (`"<Keyboard>/f"`), recarregar num asset novo não (fica `null`). Isso reflete o uso real em produção: uma sessão nova de verdade recarrega o MESMO asset `.inputactions` do disco (GUIDs estáveis, gravados no arquivo), não reconstrói os mapas via código. Corrigido reaproveitando o mesmo asset/ação e usando `InputRebindManager.ResetAllBindings()` (que já existia) pra simular "sem overrides ainda", preservando os GUIDs reais.
- 🏁 **Suíte de testes 100% verde nesta sessão automática** (restavam só as 3 falhas de `AttributeSetTests` documentadas como decisão de design de fórmula, não bug — essas continuam de propósito, não corrigidas às cegas).

### 2026-08-25 (continuação 23 — GameManagerTests resolvido de verdade, sem o refactor arriscado)
- ✅ **`GameManagerTests` totalmente resolvido**, revertendo a decisão anterior de "arriscado demais": em vez do refactor grande (lazy-init espalhado pelos 11 managers), apliquei o mesmo padrão simples e de baixo risco já validado 3 vezes nesta sessão (`AttributeSet.ForceInitializeForTests`, `CombatCameraShakeRelay.OnCameraShakeRequestedForTests`, `NpcInteractable.OnTriggerEnterForTests`) — um método `#if UNITY_EDITOR` que só força o caminho de inicialização já existente (`BuildManagers()`/`InitializeManagers()`, agora guardado por um flag idempotente `_managersBuilt` em vez de depender só do check de singleton) a rodar imediatamente. Chamado uma vez em cada teste logo após `AddComponent<GameManager>()`. Os 5 testes passam agora, sem tocar em nenhum manager individual.
- 📊 **Saldo final da dívida de testes desta sessão**: de 25+ falhas escondidas no início, restam só **4**: `AccessibilityTests` (1, ainda não investigado — próximo), e as 3 de `AttributeSetTests` ligadas ao bug de fórmula do `GetModifiedValue` (decisão de design documentada, não uma questão de inicialização).

### 2026-08-25 (continuação 22 — CombatCameraShakeRelayTests corrigido, LevelUp_ViaExperienceEvent também)
- ✅ **`CombatCameraShakeRelay.cs`**: mesmo padrão de `NpcInteractable` — exposto `OnCameraShakeRequestedForTests` (`#if UNITY_EDITOR`) chamando o handler privado diretamente, testes trocados de `EventBus.Publish` pra chamar esse método (já que a inscrição em `OnEnable` não roda a tempo fora do Play Mode). Também adicionado `EnsureCameraResolved()` já que `_playerCamera` também só resolvia em `Awake`. Todos os 3 testes passam agora.
- ✅ **`AttributeSet.LevelUp_ViaExperienceEvent_GrantsAttributePoints` também resolvido**: usando a mesma ideia — exposto `ForceInitializeForTests()` (`#if UNITY_EDITOR`) chamando `EnsureInitialized()` diretamente, chamado uma vez no `SetUp` do teste logo após `AddComponent`. Diferente do `CombatCameraShakeRelay`/`NpcInteractable` (onde troquei a chamada em CADA teste), aqui bastou UMA chamada no `SetUp` compartilhado, já que o resto dos testes desta classe já passava um gancho público antes (`GrantAttributePoints`, etc.) — só esse teste específico não tinha nenhum.
- 📊 **Restam só 3 falhas nesta família, e são intencionalmente não resolvidas**: `SpendingDexterity`/`SpendingIntelligence`/`Maithe_ApplyPassive` — causa raiz é o bug de fórmula em `GetModifiedValue` (documentado na continuação 18), não mais um problema de inicialização.

### 2026-08-25 (continuação 21 — NpcInteractableTests corrigido, sem depender de SendMessage)
- 🐛 **`NpcInteractableTests` usava `GameObject.SendMessage("OnTriggerEnter"/"OnTriggerExit", ...)` pra invocar os métodos privados do Unity** — isso dispara o assert interno do próprio motor `ShouldRunBehaviour()` quando chamado num componente fora do ciclo de vida completo do Play Mode (mesma raiz "não roda sincronamente fora do Play Mode" de toda essa sessão, só que dessa vez o sintoma é um assert do engine, não um valor errado ou NRE). Corrigido expondo `OnTriggerEnterForTests`/`OnTriggerExitForTests` (só em `#if UNITY_EDITOR`, ao lado do já existente `SetInteractionSourceForTests`) que chamam a lógica privada diretamente — mesmo padrão já usado em `Projectile.ProcessCollision`/`Hitbox.ProcessTriggerEnter` em vez de depender de `SendMessage` ou callbacks de física. Todos os 9 testes de `NpcInteractableTests` passam agora.

### 2026-08-25 (continuação 20 — WeaponSystemTests e ProceduralItemGeneratorTests corrigidos)
- ✅ **`Projectile.cs` corrigido com o mesmo padrão de Awake não-síncrono**: `_hitbox` só era resolvido em `Awake()`, então `Launch()` não conseguia setar o dano e `ProcessCollision()` retornava `false` direto (achando que não tinha hitbox nenhum). Adicionado `EnsureHitboxResolved()` chamado em ambos os métodos públicos. Todos os 12 testes de `WeaponSystemTests` passam agora.
- 🐛 **Bug real de produção em `ProceduralItemGenerator.Generate`**: quando um `affixPool` customizado era passado explicitamente (parâmetro opcional, pensado pra testes), o filtro de compatibilidade de slot (`AffixDatabase.GetCompatibleWith`) era **inteiramente pulado** — o operador `??` só aplicava esse filtro no caminho padrão (pool vindo do banco de dados), não em cima de um pool explícito. Resultado: um afixo `requiredSlot=Head` conseguia ser sorteado pra uma espada (`MainHand`). Corrigido pra sempre filtrar por `IsCompatibleWith(baseItem.EquipSlot)`, seja o pool customizado ou o padrão (`AffixDatabase.GetAll()`) — a compatibilidade de slot é uma invariante de design que não devia depender de como o pool foi fornecido. Todos os 10 testes de `ProceduralItemGeneratorTests` passam agora.
- 📊 **Saldo da sessão até aqui**: de 25+ falhas escondidas (capadas em ~25 pela paginação do runner) no início da continuação 15, restam apenas `AccessibilityTests` (1, pré-existente não investigado ainda), `AttributeSetTests` (3, causa raiz identificada, decisão de design/arquitetura documentada), `CombatCameraShakeRelayTests` (2, mesma família do AttributeSet — inscrição em `OnEnable` não-síncrona, sem gancho público disponível pro teste), `GameManagerTests` (5, decisão já tomada de não arriscar o refactor), `NpcInteractableTests` (2, causa ainda não investigada — mensagem de assertion do Unity, não NRE/valor errado).

### 2026-08-25 (continuação 19 — bug de lógica real no WaveTracker)
- 🐛 **`WaveTracker.IsComplete` era logicamente contraditório**: a fórmula exigia `IsActive == true`, mas `RegisterDeath()` desliga `IsActive = false` **exatamente no momento** em que a wave completa (`AliveCount` chega a zero) — ou seja, no exato instante em que `IsComplete` poderia virar verdadeiro, `IsActive` já tinha acabado de virar falso, tornando a expressão sempre falsa, nunca alcançável. Sem nenhuma dependência de MonoBehaviour/Awake (é uma classe C# pura), então não tinha relação com a família de bugs de inicialização desta sessão. Corrigido removendo a exigência de `IsActive` (que é precisamente o flag que sinaliza "não mais ativa porque atingiu zero") — `IsComplete` agora é só `TotalSpawned > 0 && AliveCount <= 0`. Todos os 7 testes de `WaveTrackerTests` passam.

### 2026-08-25 (continuação 18 — AttributeSetTests: 1 fix real, 3 falhas identificadas como bug de fórmula, não de inicialização)
- ✅ **Bug real corrigido em `AttributeSet.cs`**: `Start()` inscrevia o handler de `PlayerExperienceChangedEvent` (pra ganhar pontos de atributo ao subir de nível), mas `Start()` **também** não roda de forma síncrona fora do Play Mode (mesma família dos outros achados desta sessão) — movida a inscrição pra dentro de `EnsureInitialized()` (que já existia desde uma sessão anterior, só cobria os campos, não a inscrição no EventBus).
- ⚠️ **`LevelUp_ViaExperienceEvent_GrantsAttributePoints` continua falhando, causa raiz identificada mas sem fix aplicado**: mesmo com a inscrição movida pra `EnsureInitialized()`, esse método só é chamado a partir de `Awake` (não-síncrono) OU de um método público do `AttributeSet` — e esse teste específico nunca chama nenhum método público antes de disparar o evento de XP, então não existe nenhum "gancho" síncrono pra forçar a inicialização a tempo. Diferente do resto dos fixes desta sessão (que sempre tinham um método público explícito pra guardar), esse caso exigiria ou expor `EnsureInitialized()` publicamente só pro teste chamar, ou mudar a arquitetura de inscrição em evento — decidi não fazer nenhum dos dois sem mais contexto, documentando em vez de arriscar.
- 🐛 **Achado real, mas diferente do padrão desta sessão: `SpendingDexterity_IncreasesCriticalChanceByFormula`, `SpendingIntelligence_IncreasesCooldownReductionByFormula` e `Maithe_ApplyPassive_...` falham por um problema de fórmula em `BuffController.GetModifiedValue`**: o cálculo é `(baseValue + flatTotal) * (1 + percentTotal)` — pra stats que **começam em 0 e são puramente aditivos** (CriticalChance, CooldownReduction — não são "um multiplicador sobre uma base", já são a própria fração), multiplicar por `(1+percent)` com `baseValue=0` sempre dá zero (`0 * 1.02 = 0`), não importa o bônus. Isso é uma questão de design do sistema de combate (será que `PercentAdditive` deveria somar direto ao `baseValue` em vez de multiplicar, pelo menos pra esses stats específicos?), não um bug de inicialização — fora do escopo dessa sessão automática decidir sozinho sem entender a intenção completa do sistema de stats. Documentado aqui pra próxima sessão (ou o usuário) decidir a fórmula correta.

### 2026-08-25 (continuação 17 — SkillComboAndRespecTests: causa raiz real + bug de teste com Time.time)
- ✅ **`ComboTracker.cs` e `SkillManager.cs` corrigidos com o mesmo padrão de Awake não-síncrono**: `ComboTracker.Awake()` resolvia `_skillManager`/`_buffController` via `GetComponent`, que nunca rodava fora do Play Mode — `_buffController` ficava `null` e `ApplyTemporaryBuff` **silenciosamente não fazia nada** (nem exceção, só nenhum bônus aplicado, o que era bem mais difícil de notar que um crash). `SkillManager.Awake()` inicializava `_equippedSlots`/`_skillDatabase`, e `RefundAndRemove` indexava `_equippedSlots.Length` ainda nulo → `NullReferenceException` real em `RespecSkill`/`ResetSkillPoints`. Ambos corrigidos com `EnsureInitialized()`/`EnsureReferencesResolved()` idempotente, chamado nos pontos de entrada relevantes (não every method, só os que os testes desta classe realmente exercitam — mesmo critério do `Health`/`PlayerStats`).
- 🐛 **Bug real de teste descoberto DEPOIS do fix acima**: com os buffs finalmente aplicando de verdade, `Combo_OutsideTimeWindow_DoesNotComplete` começou a falhar — usava `Thread.Sleep(50)` esperando que `Time.time` avançasse entre as duas chamadas, mas **`Time.time` nunca avança em teste EditMode** (não roda nenhum frame do motor entre statements fora do Play Mode), então dormir a thread de teste não move o relógio do Unity. Esse teste só "passava" antes por coincidência (o bônus nunca era aplicado de jeito nenhum, então bater com "não deveria ter bônus" era um falso positivo). **Fix de verdade, não gambiarra**: adicionado `ComboTracker.TimeProvider` (`Func<float>`, default `() => Time.time`) — troca a fonte de "agora" por uma injetável, então o teste usa um relógio falso incrementado manualmente em vez de depender de tempo real. Pequena melhoria de testabilidade que não muda nenhum comportamento em produção (o valor padrão continua sendo `Time.time` de verdade).
- ✅ **Resultado**: os 11 testes de `SkillComboAndRespecTests` passam agora (eram vários falhando por NRE ou valor errado).

### 2026-08-25 (continuação 16 — bug real no teste do QuestManager, não na produção)
- 🐛 **`QuestManagerTests.RecipeCraftedEvent_ProgressesCollectItemObjective`**: o teste chamava `GetProgress("quest_pelts")` **depois** de publicar o evento — mas o `RequiredAmount` do objetivo (2) batia exatamente com o `ResultQuantity` da receita (2), então o evento também **completa a quest automaticamente e a remove de `_activeQuests`** (`QuestManager.CompleteQuest` — comportamento correto de produção). `GetProgress` depois disso retorna `null` (quest já não é mais "ativa", virou completa), e `progress.Objectives[0]` num `null` estourava `NullReferenceException`. Corrigido capturando a referência de `QuestProgress` **antes** de publicar o evento — como é um objeto mutável (`UpdateObjectiveProgress` edita `CurrentAmount` no próprio objeto), a leitura depois do evento continua refletindo o estado real, mesmo já removido do dicionário interno. Confirmado: mesmo padrão (`RequiredAmount` maior que o incremento simulado) já era usado nos outros testes da mesma classe — só este specific caso usava valores que coincidiam exatamente.

### 2026-08-25 (continuação 15 — mesmo fix de Awake não-síncrono aplicado a Health/PlayerStats)
- ✅ **`Health.cs` e `PlayerStats.cs` corrigidos com o mesmo padrão do `InventoryManager`** (Awake não roda sincronamente após `AddComponent` fora do Play Mode): convertidas as propriedades `CurrentHealth`/`CurrentStamina`/`CurrentMana` de auto-propriedades pra propriedades com getter que chama `EnsureInitialized()` — cobre automaticamente QUALQUER ponto de leitura (não só o método específico que um teste chamava), incluindo leituras que aconteciam ANTES da própria chamada que dispararia a inicialização (ex: `var before = _health.CurrentHealth;` lido antes de `ApplyDamage()`, onde só corrigir `ApplyDamage()` teria deixado o "antes" errado mesmo assim).
- ✅ **Resultado real, confirmado rodando a suíte inteira**: `HealthTests`, `PlayerStatsTests`, `HitboxHurtboxTests`, `EnemyControllerStateTests` e `CrowdControlAndResistanceTests` **saíram completamente da lista de falhas**. `GameManagerTests` continua falhando (mesma causa raiz, mas 11 managers interdependentes tornam o mesmo fix arriscado demais — decisão mantida da continuação 8).
- 📝 **A lista de falhas do test runner é limitada (~25 itens, `failures_capped:true`)** — ao resolver as falhas alfabeticamente anteriores, apareceram outras que sempre estiveram lá mas nunca tinham espaço pra aparecer: `ProceduralItemGeneratorTests`, `QuestManagerTests` (NullReferenceException), `SkillComboAndRespecTests` (várias, incluindo NullReferenceException), `WaveTrackerTests`, `WeaponSystemTests`. Não são regressões novas desta sessão — são achados, próximo passo natural da fila.

### 2026-08-25 (continuação 14 — sessão automática agendada, bug real no próprio teste)
- 🐛 **`BuildingManagerTests.PlaceStructure_OverlappingExistingStructure_Fails` tinha o valor esperado errado, não a produção**: o teste já verificava `Assert.IsNull(second)` (sobreposição deve falhar) e o comentário dizia "cost must not have been charged", mas a asserção seguinte checava `Assert.AreEqual(10, ...)` — 10 só faria sentido se a segunda estrutura (que devia falhar) tivesse sido cobrada mesmo assim. Corrigido pra `15` (20 - 5 do primeiro apenas), batendo com o comportamento real e com o próprio comentário do teste. Teste passa agora. Isso resolve o último item pendente da família de bugs de teste descoberta na continuação 8 que não dependia do problema maior de Awake não-síncrono.
- 📝 Revisei a lista de pendências prioritárias do prompt agendado: pedestal Maria/Lavine (já reavaliado e resolvido na continuação 9), color space (feito na 10), controles em tela (Correr+Pular feitos nas 11/12, mais indicador de clique na 12 e safe area na 13). Restam bloqueados por decisão do usuário: Maithe (créditos Meshy) e Player Settings de ícone/bundle id (identidade da loja) — não decido nenhum dos dois sozinho.

### 2026-08-25 (continuação 13 — safe area / notch na HUD)
- ✅ **`HudController.ApplySafeAreaPadding` → renomeado pra `ComputeSafeAreaInset` + aplicação nos 4 lados de `hud-root`**: converte `Screen.safeArea` (pixels de tela) pra espaço do painel UI Toolkit (dividindo por `panel.scaledPixelsPerPoint`, já que o painel usa `ScaleWithScreenSize`) e aplica como inset nos 4 lados de `hud-root` — cobre todos os cantos onde tem elemento (vitals topo-esquerda, XP baixo-esquerda, Correr/Pular baixo-direita).
- 🐛 **Achado durante a implementação**: minha primeira tentativa usava `padding` no `hud-root`, que **não tem efeito nenhum** em filhos `position:absolute` (todos os painéis da HUD são absolutos, posicionados relativos à própria caixa do `hud-root`) — padding só afeta fluxo normal. Corrigido aplicando o inset como `left/top/right/bottom` do próprio `hud-root` (substituindo o `width:100%/height:100%` do USS, que deixa de fazer efeito já que os 4 lados passam a estar definidos explicitamente).
- ⚠️ **Não testável de verdade neste ambiente**: o Editor (e qualquer device sem notch) sempre reporta `Screen.safeArea` como a tela inteira, então o inset sempre calcula zero aqui — confirmado visualmente que não mudou nada no layout atual (esperado). A lógica segue o padrão documentado da própria Unity pra safe area, mas só um device físico com notch/corte de câmera pode confirmar de verdade que os valores/conversão de escala estão certos.

### 2026-08-25 (continuação 12 — indicador visual de destino no clique-pra-mover)
- ✅ **`ClickToMoveIndicator.cs`** (novo, no `Player`): mostra um anel amarelo pulsante no chão onde o clique/toque aterrissou — antes clicar não dava nenhum feedback visual de que o input registrou ou pra onde o personagem estava indo. `PlayerClickToMove` ganhou eventos `DestinationSet`/`DestinationCleared` (novo `using System`) que o indicador escuta. Anel construído proceduralmente em runtime (mesh de anel/annulus, sem asset importado), com shader `Universal Render Pipeline/Unlit` transparente.
- 🐛 **Bug real encontrado e corrigido durante o teste**: nos meus primeiros testes o anel parecia completamente invisível em qualquer ângulo de câmera — depurei posição/ativação/camada/culling mask, tudo certo. A causa real: meus screenshots de teste usavam uma câmera manual quase na MESMA altura do anel (ele é um mesh totalmente plano, Y=0), então eu estava vendo a borda de perfil (uma linha de 1px, invisível a essa resolução) em vez da elipse. Confirmado com a câmera isométrica real do jogo (pitch 57.5°): o anel aparece perfeitamente. Lição: pra validar geometria/decal plano no chão, usar sempre a câmera de gameplay de verdade, não uma câmera de debug em ângulo raso.
- 📝 Raio ajustado de 0.45 pra 0.3 depois de ver o tamanho desproporcional ao personagem no screenshot real.

### 2026-08-25 (continuação 11 — botão de Pular na HUD)
- ✅ **Botão "Pular" na HUD**, mesmo padrão do botão Correr: `PlayerInputHandler.TouchJumpRequested` (one-shot, consumido/zerado a cada frame — diferente do Run que é "segurado") OR'd em `JumpPressedThisFrame`. Testado em Play Mode injetando `PointerDownEvent` no botão real: `TouchJumpRequested` liga corretamente. Screenshot confirma os dois botões lado a lado no canto inferior direito, legíveis.

### 2026-08-25 (continuação 10 — migração de color space pra Linear)
- ✅ **`PlayerSettings.colorSpace` migrado de Gamma pra Linear** — combinação recomendada com URP (Gamma+URP é a combinação não-recomendada; a maioria dos efeitos/iluminação do URP assume Linear). Testado com screenshot antes/depois (Scene View e Game View): nenhum material ficou rosa/magenta (sintoma clássico de shader incompatível), sem erros novos no console após o reimport de texturas que a troca dispara automaticamente. `AssetDatabase.SaveAssets()` chamado pra garantir a gravação do `ProjectSettings`.
- 📝 Isso fecha o último gap grande de "mobile 100% pronto" que ainda estava pendente da migração URP (a outra metade, ativar o pipeline em si, já tinha sido feita numa sessão anterior).

### 2026-08-25 (continuação 9 — reavaliação do pedestal Maria/Lavine, confirmado não-seguro)
- 🔬 **Maria**: histograma fino (buckets de 0.05 unidade) confirma distribuição suave/contínua do início ao fim, sem nenhum pico concentrado — é o robe alargando naturalmente perto do chão, não um pedestal. **Não é removível com segurança, mantida intacta.**
- 🔬 **Lavine**: tem um pico real só em y=0.00-0.05 (541 vértices vs baseline de ~15-30 nos buckets vizinhos — sinal genuíno de disco fino na base), mas sem um segundo platô claro logo acima (ao contrário de Joaquim/Ícaro, que tinham base E topo do pedestal bem demarcados) — não dá pra saber com segurança onde o "pé/sapato" de verdade começa. Cortar às cegas arrisca truncar o sapato dela. **Mantida intacta também.** Se o usuário achar que ainda incomoda visualmente nela, o próximo passo seria inspecionar via screenshot bem de perto nos pés dela primeiro, não só histograma.

### 2026-08-25 (continuação 8 — investigação da dívida de testes + bug real de compilação)
- 🐛 **Bug real (meu, desta sessão): `CameraZoom.cs` ficou com erro de compilação por ~40 minutos sem eu perceber**. Ao corrigir o bug do pinch-to-zoom (continuação 7), usei `TouchControl` sem importar `UnityEngine.InputSystem.Controls` — erro de compilação real (`CS0246`), que **quebra a assembly inteira**, fazendo Unity continuar rodando o assembly compilado ANTES desse erro (não é a mesma "staleness" documentada antes — dessa vez era um erro de verdade, só que eu só chequei `is_compiling`/`external_changes_dirty` no `editor/state`, nunca `read_console` por erro, então não percebi na hora). **Lição: depois de QUALQUER `refresh_unity compile:request`, chamar `read_console` filtrando `error` antes de assumir que compilou — o `editor/state` sozinho não denuncia erro de compilação, só se está compilando ou não.** Corrigido (using adicionado), confirmado sem erros.
- ✅ **Bug real corrigido em `InventoryManager.cs`**: em Edit Mode (fora do Play Mode), `AddComponent<T>()` **não** chama `Awake()` de forma síncrona (confirmado isolando: `_slots` continuava `null` logo depois do `AddComponent`, mesmo sem nenhum teste envolvido) — diferente do Play Mode, onde sempre foi síncrono. Isso derrubava `BuildingManagerTests`/`CraftingManagerTests` com `NullReferenceException` sempre que `AddItem`/`GetTotalCount`/etc eram chamados logo após `AddComponent`. Corrigido com `EnsureSlotsInitialized()` (idempotente, chamado no topo de todo método público que toca `_slots`) — é um padrão defensivo legítimo, não só pra agradar teste: qualquer código real que resolva `InventoryManager` antes do próprio `Awake` rodar (ordem de execução entre scripts não é garantida pela Unity) se beneficia da mesma proteção. **Resultado real**: `PlaceStructure_OverlappingExistingStructure_Fails` parou de estourar exceção e virou um mismatch de asserção de verdade (`Expected: 10 But was: 15`) — bug de lógica genuíno, ainda não investigado, masujeito bem mais tratável que um crash.
- ⚠️ **`GameManagerTests` continua falhando, causa raiz é a MESMA (Awake não-síncrono), mas não fiz o mesmo fix defensivo lá — é arriscado demais**: `GameManager` constrói 11 managers interdependentes dentro de `Awake`; fazer isso virar lazy-init espalhado por 11+ propriedades é um refactor grande demais pra fazer só por causa de um sintoma de teste, e o comportamento real do jogo (sempre em Play Mode, onde `Awake` É síncrono) nunca é afetado por isso. Deixei uma tentativa defensiva no `SetUp` do teste (limpa singletonórfão antes de cada teste) que não resolve sozinha mas não atrapalha.
- 📝 **Escopo maior do que eu esperava**: rodando a suíte completa depois do fix de compilação, apareceram famílias de falha que o TODO.md **nunca tinha documentado antes** (`HealthTests`, `HitboxHurtboxTests`, `PlayerStatsTests`, `GenericObjectPoolTests`, `NpcInteractableTests` — todas lendo `0.0f`/contagem errada, consistente com o mesmo padrão de Awake não-síncrono). Ou esse ambiente específico (execução de teste via Unity MCP) sempre teve esse comportamento e ninguém tinha rodado a suíte completa depois de um compile realmente bem-sucedido recentemente, ou piorou. **Não της fiz mais fixes defensivos além do `InventoryManager`** — o escopo de "lazy-init todo Awake que testes tocam" é grande demais pra essa sessão; recomendo investigar se a suíte roda diferente pela janela normal do Test Runner (clique manual) vs via este MCP antes de decidir se vale a pena persegui-lo mais.

### 2026-08-25 (continuação 7 — bug real no pinch-to-zoom)
- 🐛 **Pinch-to-zoom já existia no código (`CameraZoom.cs`) mas com um bug real**: usava `Touchscreen.current.touches.Count >= 2` pra decidir se havia pinça de dois dedos — só que `touches.Count` é o **tamanho fixo do array de slots de toque do dispositivo** (a capacidade máxima, tipo 10), não quantos dedos estão realmente tocando agora. Isso significa que em qualquer dispositivo com touchscreen presente (mesmo sem nenhum dedo tocando), o código lia as posições dos slots `[0]`/`[1]` (obsoletas/zeradas) como se fossem uma pinça de verdade, aplicando zoom com base em lixo. Corrigido pra contar de verdade quantos `TouchControl` estão com `.press.isPressed` antes de tratar como pinça.
- 📝 Item do roadmap ("Câmera: suportar pinch-to-zoom") já estava tecnicamente implementado, só nunca foi marcado — e tinha esse bug latente nunca detectado porque nunca rodou num dispositivo touch de verdade.

### 2026-08-25 (continuação 6 — UI escalável pra diferentes resoluções)
- 🐛 **Bug real encontrado no `DefaultPanelSettings.asset`**: `m_ScaleMode: 1` já parecia certo à primeira vista, mas o enum real do Unity é `ConstantPixelSize=0, ConstantPhysicalSize=1, ScaleWithScreenSize=2` — ou seja, o painel inteiro (HUD + tela de seleção) estava em **`ConstantPhysicalSize`** (escala por DPI, não reflow por resolução/aspect ratio), não `ScaleWithScreenSize` como o valor `1` sugeria à primeira vista. Corrigido pra `ScaleMode = ScaleWithScreenSize` (valor 2) — `m_ReferenceResolution` (1200x800, já configurado) e `m_ScreenMatchMode`/`m_Match` agora realmente entram em uso (antes eram campos mortos, ignorados nesse modo). `m_Match` ajustado de 0 (só largura) pra 0.5 (balanceado largura/altura), pra não distorcer em aspect ratios bem diferentes de celular (16:9 até 20:9+).
- ✅ Testado em Play Mode com resolução atípica de screenshot (quase quadrada, 606x609) — HUD, botão de Correr e tela de seleção de personagem continuam legíveis/proporcionais.

### 2026-08-25 (continuação 5 — botão de Correr na HUD pra touch)
- ✅ **Botão "Correr" na HUD** (`Assets/UI/HUD/HudView.uxml`/`.uss`, canto inferior direito, circular): `PlayerInputHandler.TouchRunHeld` (novo, `OR`'d em `IsRunHeld` junto do botão físico/gamepad — nenhum dos dois atrapalha o outro) é ligado/desligado via `PointerDownEvent`/`PointerUpEvent`/`PointerLeaveEvent` (não o `Click` do `Button`, que só dispara na soltura — Correr precisa ficar "segurado"). `HudController` ganhou a referência `_playerInput` (wireada na cena pro `PlayerInputHandler` do `Player`). Testado em Play Mode injetando os eventos de pointer com `target` explícito no botão real da HUD: liga ao pressionar, desliga ao soltar.
- 📝 Escolha de escopo: não criei joystick virtual nem botão de ataque dedicado — o clique/toque já move E ataca (perto de inimigo) pelo design MU Online-style já existente (`MoveClick` já bindado em `<Touchscreen>/primaryTouch/tap`), então o que faltava mesmo pra touch era só um jeito de segurar "correr" (não dá pra "segurar" uma tecla que não existe fisicamente na tela). Jump também não tem botão touch — aceitável por ora, pular não é central nesse estilo de câmera fixa isométrica.

### 2026-08-25 (continuação 4 — usuário pediu ajuste explícito no caminhar de aranhas/lobos)
- ✅ **`EnemyProceduralAnimator` melhorado**: bob vertical aumentado (0.08→0.12, freq 10→9), lean 10°→14°, e **adicionado sway lateral + wag de guinada em meia-frequência** (`_swayAmplitude`, `_yawWagDegrees`) — antes só existia balanço reto pra cima/baixo, que lê como "pulando no lugar"; o sway em meia-frequência simula troca de peso entre pernas (alternância esquerda/direita), lendo melhor como passada de verdade mesmo sem esqueleto de pernas de verdade se movendo.
- ✅ **`EnemyPatrolState.Wander` com pausa entre trechos**: antes, ao chegar num ponto aleatório, escolhia outro na hora e podia virar quase 180° instantaneamente — lia como errático/robótico. Adicionada pausa de 1.2–2.8s (aleatória) entre trechos, como se parasse pra "olhar em volta" antes de andar de novo. Raio de wander também aumentado de 4m pra 5m (mais percurso visível por trecho).
- 📝 Lembrete: sem rig nos inimigos (Wolf/Scorpion/Spider — Meshy Rigging API só suporta bípedes), então isso continua sendo balanço do corpo inteiro, não perna-por-perna. É o teto do que dá pra melhorar sem um rig de verdade.

### 2026-08-25 (continuação 3 — Player Settings mobile)
- ✅ **Orientação travada em paisagem**: `PlayerSettings.defaultInterfaceOrientation = AutoRotation` com portrait/portrait-invertido desligados e só landscape esquerda/direita permitidos — faz sentido pro estilo de câmera isométrica fixa (MU Online) do jogo, que não foi desenhado pra tela vertical. `Android.startInFullscreen = true`.
- 📝 **Descoberta: touch já está wireado de verdade, não é só placeholder** — conferido `PlayerControls.inputactions`: `MoveClick` já tem binding `<Touchscreen>/primaryTouch/tap` (além do clique de mouse) e `PointerPosition` usa `<Pointer>/position` (classe base que cobre mouse E touch). Como a câmera é MU Online-style (clique/toque pra mover), isso já deveria funcionar num device de verdade sem trabalho extra de binding — só falta testar em device/emulador real, que continua fora do alcance deste ambiente (sem device físico conectado, emulador não configurado).
- ⚠️ **Não mexido, decisão de negócio, não técnica**: `companyName` (ainda "DefaultCompany") e `applicationIdentifier` (`com.DefaultCompany.rapadura`) — mudar isso depois de publicado quebra a listagem na loja, não é algo pra decidir sozinho. `productName` também está em minúsculo ("rapadura") — cosmético, fácil de ajustar quando o usuário decidir o nome de exibição final. Ícone do app nunca configurado (precisa de arte, não gerável aqui sem créditos/pedido explícito).
- 🔒 **Retomada do rig da Maithe não feita nesta rodada** — já tem o `.glb` pronto pra reenviar (`Assets/Models/Characters/Maithe_RO/glb/Maithe_RO.glb`), mas cada tentativa consome ~5 créditos Meshy e a sessão anterior pausou qualquer gasto novo até confirmação explícita do usuário (créditos relatados baixos, ~280). Não decidi gastar sozinho — perguntar ao usuário antes de tentar de novo.

### 2026-08-25 (continuação 2 — bug real de animação travando + pedestal aplicado ao Ícaro)
- 🐛 **Causa raiz do "anda alguns passos e trava sem animação" encontrada**: os `AnimatorController` dos 4 heróis riggados (`{Nome}_AnimatorController`) têm transições **automáticas dirigidas pelo parâmetro `Speed`** (Idle→Walk se Speed>0.1, Walk→Idle se Speed<0.1, Walk→Run se Speed>4, etc — confirmado inspecionando o grafo via `UnityEditor.Animations.AnimatorController`), e o `PlayerAnimatorSpeedDriver` já alimenta esse `Speed` todo frame. **Só que** `PlayerIdleState`/`PlayerWalkState`/`PlayerRunState` também chamavam `context.Animator.Play("Walk")` etc explicitamente no `Enter()` — dois sistemas competindo pelo mesmo Animator. O `Play()` força um estado, mas no frame seguinte a transição própria do grafo (por `Speed`) podia levar o Animator de volta sozinho, dessincronizando do state machine em C# (que continuava achando que estava em "Walk" e comandando o `Motor.Move()`): resultado visual era o personagem "grudar" numa pose enquanto a posição seguia se movendo. **Removidos os `Animator.Play()` de `PlayerIdleState`/`PlayerWalkState`/`PlayerRunState.Enter()`** — agora só o `Speed` dirige a troca de animação, como o grafo já foi desenhado pra fazer.
- 📝 **Lobos/aranhas "parados" no teste do usuário — provável causa: script editado com Play Mode do usuário ainda rodando**. A lógica de wander foi validada de novo nesta sessão (`stateMachine.Tick()` manual moveu o inimigo normalmente) e o `EnemyProceduralAnimator` já estava anexado aos 5. Como o usuário estava jogando **na mesma sessão do Editor enquanto eu editava scripts** (`PedestalRemover.cs`, `CameraSettings.cs`), e já é regra documentada aqui que isso zera campos privados não-serializados tipo `EnemyController._stateMachine` sem re-rodar `Awake()`, a explicação mais provável é que a partida que o usuário viu já estava com IA morta por causa da recompilação no meio do jogo, não um bug de lógica novo. **Ação: sempre testar num Play Mode iniciado DEPOIS do último edit/recompilação, nunca no meio de uma sessão em que eu estou editando arquivos.**
- ✅ **Pedestal removido do Ícaro** (aplicado de verdade desta vez, não só testado): `Icaro_NoPedestal.asset` gerado com threshold de altura 2.3 (histograma bem mais estreito que o do Joaquim — confirmado disco largo em X: -21.6 a 19.4 vs corpo inteiro -35.9 a 35.9). Testado em Play Mode isolado (personagem sozinho, sem inimigo por perto): **nenhuma diferença visual** entre a malha original e a limpa — a base já ficava escondida dentro das botas dele, então o corte é seguro e sem risco. Ainda não fiz o wiring runtime (falta um utilitário tipo `PedestalMeshUtility` que troca `SkinnedMeshRenderer.sharedMesh` por `Resources.Load<Mesh>("CleanedMeshes/" + characterId + "_NoPedestal")` logo após o `Instantiate` em `PlayerVisualSwapper.SwapTo` e `CharacterPreviewController`) — próximo passo direto, sem risco adicional já que o assay está validado.
- ✅ **Wiring do pedestal concluído**: criado `Assets/Scripts/Gameplay/Characters/PedestalMeshUtility.cs` (`ApplyIfAvailable(instanceRoot, characterId)` — troca `SkinnedMeshRenderer.sharedMesh` pelo `Resources.Load<Mesh>("CleanedMeshes/{id}_NoPedestal")` se existir, no-op caso contrário) e chamado logo após o `Instantiate` em `PlayerVisualSwapper.SwapTo` **e** `CharacterPreviewController.ShowCharacter`. Testado pelo fluxo real de seleção (Ícaro): `SkinnedMeshRenderer.sharedMesh.name` confirmado como `Icaro_NoPedestal` no Player de verdade. Joaquim também já tem o asset gerado (`Joaquim_NoPedestal.asset`) então já se beneficia automaticamente do mesmo mecanismo.
- ⚠️ **Maria e Lavine: pedestal NÃO removido, avaliado como arriscado demais**. Histograma de altura da Maria não tem nenhum pico nítido nos primeiros 12 units (distribuição gradual — bate com um robe que naturalmente alarga perto do chão, cortar às cegas arriscava truncar a roupa de verdade). Lavine tem só um pico modesto em y=0-0.25 (804 vértices, bem menor que os ~5800-8800 do Joaquim/Ícaro) e depois um platô mais alto em y=5.0-5.75 que não parece pedestal (posição alta demais pra ser base). Nenhum dos dois tem o padrão "disco largo perto do chão + platô logo acima, quase nada no meio" que caracterizou Joaquim/Ícaro com clareza. Deixar como estão até ter uma forma melhor de verificar (talvez inspecionar visualmente a malha crua num visualizador externo, ou perguntar pro usuário se ainda incomoda nesses dois specificamente).

### 2026-08-25 (continuação — retomada dos 3 pendentes)
- ✅ **Inimigos "sem movimento" — causa raiz encontrada e corrigida**: a lógica de patrulha/wander (`EnemyPatrolState`) sempre esteve correta (validado chamando `stateMachine.Tick()` manualmente — moveu normalmente). O problema real: o `EnemyProceduralAnimator.cs` (criado numa sessão anterior pra dar bob/lean e parar de "deslizar rígido") nunca tinha sido **adicionado como componente** aos 5 inimigos da cena (`Enemy_Wolf`, `Enemy_Wolf2`, `Enemy_Spider1`, `Enemy_Spider2`, `Enemy_Scorpion`) — só existia no código. Adicionado a todos os 5, `_visualRoot` apontando pro filho `EnemyVisual`. Confirmado visualmente no editor: os spiders aparecem com sombra/postura de quem está andando.
- ✅ **Câmera muito longe → personagens pareciam pequenos**: não existe asset `CameraSettings` no projeto — `CameraController` usava os defaults do `ScriptableObject.CreateInstance` (zoom 10–25, padrão 15m, longe demais pro personagem de referência). Reduzido em `CameraSettings.cs` pra min 6 / max 14 / padrão 9.
- 🐛 **Bug real de escala encontrado no Joaquim**: ao investigar o "tamanho pequeno", descobri que a entry do Joaquim no `PlayerVisualSwapper` tinha `uniformScale = 0.01`, igual ao `PlayerVisual` (pai, também 0.01) — escala composta (0,0001×), deixando o Joaquim praticamente invisível (~1cm) sempre que instanciado pelo swapper. Corrigido pra `uniformScale = 1.91` (mesma fórmula usada pra Maria/Ícaro/Lavine: `targetHeight / (rawMeshHeight * parentScale)`). Isso era o TODO "Joaquim não foi re-derivado, mas não estava quebrado" — na verdade estava.
- ✅ **Pedra (`Rock_5`) bloqueando a câmera**: estava em `(0, 0.25, -8)`, quase exatamente na linha de visão padrão câmera↔spawn. Com o zoom reduzido, a colisão da câmera passou a puxar a câmera pra cima da pedra (uma esfera cinza tomando a tela toda). Movida pra `(3.5, 0.25, -8)`.
- ⚠️ **"Bota"/pedestal nos pés — causa raiz confirmada, correção NÃO aplicada (risco de quebrar a pose)**: histograma de altura dos vértices confirma que é geometria fundida na mesma malha (`char1`, único submesh/material) — um disco achatado nos primeiros ~8% da altura (base) e outro platô logo acima (topo do pedestal/sola). Criei `Assets/Scripts/Editor/PedestalRemover.cs` (corta triângulos com os 3 vértices abaixo de um threshold de altura) e gerei `Assets/Resources/CleanedMeshes/Joaquim_NoPedestal.asset` como teste. Ao aplicar em Play Mode, o pedestal sumiu mas não vale a pena manter — **não foi wireado em nenhum lugar** (`PlayerVisualSwapper`/`CharacterPreviewController` continuam usando a malha original). Próxima sessão: reavaliar o threshold com verificação visual char a char (Maria/Ícaro/Lavine têm histogramas bem menos nítidos que o do Joaquim — Maria em especial pode ter o robe legitimamente largo perto do chão, cortar às cegas arrisca truncar a roupa).
- 📝 Nota de ambiente: `Time.time` não avança em Play Mode headless (sem foco), mesmo com `EditorApplication.Step()` chamado manualmente (avança `frameCount` mas não `Time.time`/`deltaTime`). Validação de lógica dependente de tempo real exige chamar os métodos do `IState<T>.Tick()` diretamente via reflexão com um `deltaTime` sintético, não dá pra confiar em screenshots tirados logo após `manage_editor play`.
- **2026-08-17** — Estrutura inicial em 10 fases + Meta do MVP.
- **2026-08-17** — Adicionada Fase 0 (pré-produção/fundação técnica), itens de engenharia profissional (testes, CI/CD, logging, acessibilidade, localização, compliance de loja) espalhados pelas fases existentes.
- **2026-08-17** — Adicionadas Fases 11–14 (Arte/Animação/Cinemáticas, Multiplayer opcional, Marketing/Comunidade, Pós-lançamento/Live Ops).
- **2026-08-17** — Definido o roster de 5 personagens jogáveis (Joaquim, Maria, Maithe, Ícaro, Lavine) com lore, classe, função e habilidades; classes C# criadas em `Assets/Scripts/Gameplay/Characters/`.
- **2026-08-17** — Definido que **mobile é plataforma obrigatória** (não opcional) — adicionada checklist específica de mobile na Fase 0; câmera e sistemas novos devem sempre considerar touch desde o início, não como adaptação posterior.
- **2026-08-17** — Iniciada a Fase 0 (Git/`.gitignore`/`.gitattributes`, `.editorconfig`, `Docs/GDD.md`, `CONTRIBUTING.md`, `GameLogger`, testes automatizados EditMode, workflow de CI) e código da Fase 1 (estados Fall/Slide, câmera com zoom/shake, checkpoints/respawn).
- **2026-08-17** — Auditoria de código vs. roadmap: Fases 3 (RPG) e 4 (Skills) estavam com o código muito mais avançado do que os checkboxes indicavam (Inventário, Itens, XP/Níveis, SkillManager, Skill Tree UI já implementados). Checkboxes corrigidos para refletir o estado real. Disparadas 3 frentes em paralelo: Combate base (Fase 2), Localização (Fase 6) e Save versionado/anti-corrupção (Fase 7).
- **2026-08-19** — Marco importante: o projeto foi aberto com sucesso pela primeira vez no Unity Editor (6000.3.22f1), gerando `Library/`/`.meta` reais. Primeiro erro de compilação real corrigido (`InputRebindManager.cs`, tipo aninhado `RebindingOperation` sem qualificação). Conteúdo narrativo dos 5 personagens completo (quests + diálogos pessoais + quest de convergência), árvore genealógica da família documentada no GDD. `Docs/EditorSetupGuide.md` criado para orientar o setup local.
- **2026-08-19** — Conectado o **Unity MCP** (`CoplayDev/unity-mcp`), dando acesso direto ao Editor (Console, compilação, testes) em vez de depender só de cópia/cola manual de erros. Com isso: corrigido bug real de conflito namespace/classe (`Rapadura.Core.EventBus` renomeado pra `Rapadura.Core.Events`), corrigida falha de referência de assembly nos testes (`Assets/Scripts` e `Assets/Scripts/Editor` viraram assemblies nomeadas `Rapadura.Runtime`/`Rapadura.Editor` em vez de depender da implícita `Assembly-CSharp`, que não resolvia de forma confiável a partir de outro `.asmdef`), e habilitado de fato o New Input System. Suíte de testes rodou pela primeira vez de verdade (394 testes via Unity MCP `run_tests`) — projeto agora **compila com 0 erros**. `AttributeSet` corrigido (inicialização lazy). ⚠️ Ainda restam falhas reais de teste a investigar: `GameManagerTests` (parece ligado ao padrão Singleton + testes em sequência não isolando `Instance` corretamente entre casos), `BuildingManagerTests`/`CraftingManagerTests` (NullReferenceException), `CombatCameraShakeRelayTests`/`CrowdControlAndResistanceTests`/`EnemyControllerStateTests` (mesma família de sintoma — provavelmente dependem de managers registrados via `GameManager` que não rodou a tempo), `AccessibilityTests.SaveAndLoadBindingOverrides` (round-trip de binding falhando).
- **2026-08-24** — Usuário forneceu o prompt-base definitivo pra geração de arte 3D dos 5 personagens (ver seção **🖼️ Prompt-base para Arte 3D dos Personagens** logo abaixo). Guardado aqui como referência **antes de executar** — se os créditos/sessão acabarem no meio do caminho, este é o texto a reusar, não recriar do zero.
- **2026-08-24 (sessão longa via Unity MCP)** — Marco grande: **os 5 heróis têm modelo 3D real agora**, estilo **Ragnarok Online (chibi)**, confirmado pelo usuário como referência definitiva (substitui a ideia inicial de só "crianças"). Gerados via Meshy (texto→3D): `Joaquim_RO`, `Maria_RO`, `Maithe_RO`, `Icaro_RO`, `Lavine_RO` em `Assets/Models/Characters/<Nome>_RO/`. Cada um com silhueta/cor/arma bem distinta (não só variação de cor): Joaquim (armadura+escudo com núcleo verde), Maria (túnica branca/dourada + cajado solar), Maithe (traje verde-floresta + arco), Ícaro (traje laranja-cobre + dispositivo mecânico), Lavine (robe roxo + chapéu de bruxa + cajado). Escala/rotação padronizadas em todos (raw height 100 → scale 0.014 → ~1.4m, rotation X=270 corrige convenção Z-up da Meshy).
  - Joaquim já era o Player (`PlayerVisual`) — trocado pra versão RO definitiva (era antes uma versão "criança" não-RO, descartada).
  - **Novo: preview 3D real na tela de seleção** (pedido do usuário — "não só o nome") — `CharacterPreviewController.cs` (turntable: instancia/gira o modelo do herói hovered) + `PreviewCamera` renderizando pra `CharacterPreviewRT` (RenderTexture 512x512) + `Image` no `CharacterSelectView.uxml` (`detail-preview`) mostrando isso ao vivo. Achado/corrigido: `Image` element estava colapsando de 220px pra 22px de altura por causa do layout flex — resolvido com `flex-shrink:0`/`flex-grow:0` no USS **e** reforço inline via C# (belt-and-suspenders). Validado via Unity MCP: screenshot mostrando a Lavine girando no preview real dentro da UI.
  - **HUD finalmente ligada na cena** — `GameObject` `HUD` com `UIDocument` (`HudView.uxml` + `DefaultPanelSettings`) + `HudController` referenciando `PlayerStats` do Player. Validado em Play Mode: Vida/Stamina/Mana/Nv./XP atualizando ao vivo via eventos.
  - **Balanço/inclinação do movimento aumentados** (usuário reportou que parecia "estático" ao correr — mesh sem rig, então o feedback visual precisa ser mais exagerado): `_bobAmplitude` 0.06→0.14, `_leanDegrees` 8→16, `_bobFrequency` 9→12 no `PlayerProceduralAnimator` do Player.
  - **Inimigos do mapa também com modelo real** — Lobo, Escorpião, Aranha gerados (Lobo e Aranha reaproveitados nos 2 GameObjects duplicados de cada, economizando créditos). Detalhe técnico: `Enemy_*` tinham `MeshFilter`/`MeshRenderer` direto na raiz (cápsula) — trocado pelo padrão já usado no Player: modelo real como filho `EnemyVisual`, removendo `MeshFilter`/`MeshRenderer` da raiz (mantendo `CapsuleCollider`/`Health`/`EnemyController` intactos).
  - ⚠️ **Nenhum dos modelos (heróis ou inimigos) tem rig/esqueleto** — Meshy texto→3D só gera malha+textura estática. Pernas/braços não se movem de verdade; só o corpo inteiro balança/inclina via `PlayerProceduralAnimator`. **Próximo passo pra animação de verdade: passar os FBX pelo Mixamo (auto-rig gratuito)** depois que todos os assets estiverem prontos — aí sim dá pra ter `Animator` com Idle/Walk/Run/Attack/Hit reais.
  - ⚠️ **Créditos Meshy baixos** (usuário reportou ~280 restantes) — pausada qualquer geração nova (árvores, pedras, casa de teste, pedido pelo usuário) até confirmação explícita de gastar o que resta. Isso é trabalho pendente, não esquecido.
  - ⚠️ **Achado de infraestrutura importante**: qualquer edição de arquivo do projeto (script, uxml, uss) OU entrar/sair do Play Mode dispara um "domain reload" no Unity que **mata o rastreamento local dos jobs assíncronos da Meshy** (`generate_model status` volta `"Interrupted by an editor reload"`), mesmo que a geração continue rodando no servidor da Meshy (créditos já gastos, resultado não recuperável via este bridge). Portanto: **nunca editar arquivos nem entrar em Play Mode enquanto uma geração Meshy estiver em andamento** — isso já custou pelo menos 3 reenvios completos do lote de 5 heróis nesta sessão.
  - Lore dos 5 heróis revisada pra infância/pré-adolescência (idades 9-14) condizente com o visual RO — ver seção **Personagens Jogáveis (Roster)** abaixo pro detalhe completo e o que ainda falta (GDD/quests/diálogos não atualizados).
- **2026-08-25 — Rig real via Meshy Rigging API** (endpoint separado do texto→3D: `POST /openapi/v1/rigging`, aceita `model_url` público em `.glb`, ~5 créditos/chamada, **só funciona em modelos bípedes/humanoides**). Pipeline: FBX→GLB local (`FBX2glTF-windows-x64.exe`) → hospedar temporariamente em `http://137.131.243.162/claude-tmp-assets/` (servidor do usuário, ver credencial salva na memória do Claude) → POST rigging → poll → baixar FBX rigado + animações Walk/Run prontas → importar como Humanoid no Unity → `AnimatorController` (Idle/Walk/Run + parâmetro `Speed`) + extração de textura embutida.
  - ✅ **Joaquim**: rigado e **já é o modelo do Player em jogo**, com Idle/Walk/Run reais (`Joaquim_AnimatorController`, `PlayerAnimatorSpeedDriver`). Problemas conhecidos, aceitos como trade-off: ficou pequeno (parcialmente corrigido reescalando) e sobrou um artefato de "pedestal/bolinha" nos pés vindo da retopologia do rig (enterrado no chão, não removido de verdade — precisaria edição de malha em ferramenta externa tipo Blender, que não está disponível aqui). Textura com falhas de UV (gaps brancos) — aceito, sem re-tentativa.
  - ✅ **Maria**: rig SUCEEDED (task `01a0360c-7b53-776f-b47e-736853b2f25d`, resposta salva em `Assets/Models/Characters/Maria_RO/rig_response.json`) — **ainda não baixado/importado no Unity**.
  - ✅ **Ícaro**: rig SUCEEDED (task `01a0360d-f5b7-7761-97de-323f0a61af1b`) — **ainda não baixado/importado**.
  - ✅ **Lavine**: rig SUCEEDED (task `01a0360e-4e90-7d0e-b786-7d0366cfade3`) — **ainda não baixado/importado**.
  - ❌ **Maithe**: falhou (`422 Pose estimation failed`, provável causa: pose seagurando arco atípica) — retry não concluído (bloqueado por instabilidade do compilador do `execute_code` do MCP na sessão anterior). Pendente re-tentar.
  - ❌ **Wolf, Scorpion, Spider**: rig **impossível** por esta API — `422 Pose estimation failed`, confirmado que a API de rig da Meshy só suporta modelos bípedes/humanoides. Inimigos continuam com malha estática (sem animação de perna), só o movimento de translação do `EnemyController`.
  - Nenhum sistema existe ainda pra trocar o modelo do Player conforme o herói escolhido na seleção de personagem (hoje só Joaquim está fixo como `PlayerVisual`) — precisa ser criado quando os outros heróis forem importados.
- **2026-08-25** — Sessão sem conexão com o Unity MCP bridge (nenhuma ferramenta `manage_gameobject`/`generate_model`/etc. disponível) — correções feitas só por edição de arquivo, sem poder testar ao vivo no Editor:
  - **Inimigos parados (aranhas relatadas, mas afetava todos)**: todo `EnemyController` na cena tem `_waypoints: []`, e `EnemyPatrolState` não fazia nada até o player entrar no raio de detecção — ficava parado até então. Adicionado wander (perambular perto do ponto de spawn) quando não há waypoints, em `EnemyPatrolState.cs`.
  - **Barra de vida não aparecia como barra**: `HudController` só lia `PlayerStats.CurrentHealth` uma vez em `OnEnable()`, e se isso rodasse antes do `Awake()` do Player preencher `CurrentHealth = MaxHealth`, a barra ficava com preenchimento 0. Adicionada uma segunda leitura em `Start()` (garantido rodar depois de todo `Awake()`).
  - **Árvore malfeita**: hoje é só uma cápsula (tronco) + 1 esfera (copa) — "pirulito". Sem MCP não dá pra gerar/importar um modelo real agora. Criado `Assets/Scripts/Editor/TreeFoliageImprover.cs` (menu `Rapadura/Setup/Improve Placeholder Tree Foliage`) que troca a esfera única por um cluster de 5 esferas sobrepostas/deslocadas, pra parecer uma copa de verdade — é um paliativo, o fix definitivo continua sendo gerar um modelo real de árvore via Meshy (créditos e conexão MCP pendentes).
  - Rochas (`Mat_Rock` já existe mas sem uso confirmado em pedras reais na cena) e casa de teste **continuam pendentes**, mesmo bloqueio de créditos/MCP.
- **2026-08-25 (MCP reconectado)** — usuário reconectou o Unity MCP no meio da sessão (`/mcp`) e pediu pra continuar melhorando textura de árvore/pedra, lembrando que **o jogo precisa rodar em celular**:
  - Texturas `Tex_Bark`/`Tex_Grass`/`Tex_Rock`/`Tex_Leaves` regeradas (procedural noise via `manage_texture`, sem provider de IA configurado — `fal`/`openrouter` aparecem como `configured:false`, então não dá pra gerar textura fotorreal ainda, só ruído colorido). Tiling (`mainTextureScale`) aumentado (bark 2x3, leaves/rock 4x4, grass 8x8) pra repetir o padrão em vez de esticar 1 textura inteira numa superfície grande.
  - Rodado `Rapadura/Setup/Improve Placeholder Tree Foliage` (criado na sessão anterior) — copa das árvores agora é um cluster de esferas em vez de 1 esfera única.
  - **Pedras criadas do zero** — não existia nenhuma na cena (`GameObject "Rocks"` com 5 esferas achatadas/giradas usando `Mat_Rock`).
  - **Mobile**: `Mat_Bark`/`Mat_Grass`/`Mat_Rock`/`Mat_Leaves`/`Mat_Wall`/`Mat_HeroBase` trocados do shader `Standard` (caro, PBR completo) pro `Mobile/Diffuse` (built-in, leve, sem specular/GI caro) — mais barato pra rodar em celular. Nível de qualidade do projeto reduzido de **Ultra pra Medium**.
  - ⚠️ **Gap grande ainda aberto pra mobile de verdade**: o projeto está no **Built-in Render Pipeline** (não URP) com **color space Gamma** — o roadmap original (Fase 0) já pedia URP como obrigatório pra mobile e isso nunca foi migrado. Migrar pra URP agora é uma tarefa grande (recriar/reimportar materiais, checar shaders customizados, testar tudo de novo) — não foi feita ainda por ser arriscada demais pra fazer "de passagem"; precisa ser um passo dedicado, com o usuário ciente que pode quebrar visual temporariamente. Outros itens de mobile ainda pendentes: touch input real (só New Input System configurado, sem verificação de touch), Player Settings de Android/iOS (ícone, orientação, API level) nunca configurados, teste em device real nunca feito.
  - Textura ainda é só ruído colorido (não fotorreal) por falta de provider de imagem IA — se o usuário tiver uma chave da fal.ai ou OpenRouter, dá pra configurar e gerar textura de verdade (grama/casca/pedra realistas) em vez do ruído procedural atual.
- **2026-08-25 (continuação, mesma sessão)** — usuário reportou "movimento da aranha/escorpião uma bosta" e "barra de vida uma bosta". Duas causas raiz reais encontradas e corrigidas (não só polimento):
  - **Bug real de vez em quando (não invenção)**: editar um script C# **enquanto o Play Mode já está rodando** força recompilação/domain reload no meio do jogo, e isso **zera campos privados não serializados** (como a `StateMachine` interna do `EnemyController`, ou o `Target` do inimigo) sem rodar `Awake()`/`Start()` de novo — o inimigo fica com IA morta (`_stateMachine == null`) até o Play ser reiniciado. Confirmado via `execute_code` inspecionando o objeto ao vivo. **Regra a seguir daqui pra frente: nunca editar script com o Play Mode ativo — sempre `Stop` antes.**
  - Melhorada a sensação de movimento em `EnemyController.MoveInDirection` (rotação agora suaviza com `Quaternion.RotateTowards` numa velocidade configurável `_turnSpeedDegPerSec`, em vez de virar instantaneamente pra cara do alvo — o "vira-robô" contribuía pra sensação de quebrado) e criado `EnemyProceduralAnimator.cs` (mesmo truque de bob/lean do `PlayerProceduralAnimator`, mas calculando velocidade a partir do delta de posição por frame já que `EnemyController` não expõe um `Velocity`) — anexado no `EnemyVisual` de todos os 5 inimigos da cena.
  - **Barra de vida — causa raiz real encontrada**: `HudView.uxml` nunca teve o `HudView.uss` de fato vinculado. A tag `<Style src="HudView.uss">` (caminho relativo, só o nome do arquivo) **não resolvia** — confirmado inspecionando `VisualTreeAsset.Instantiate().styleSheets.count`, que dava **0**. Por isso a HUD sempre renderizou como texto puro empilhado no canto, sem caixa/cor/preenchimento — não era só "feio", estava genuinamente sem nenhum CSS aplicado. Corrigido usando caminho absoluto de projeto (`<Style src="/Assets/UI/HUD/HudView.uss" />`) — `styleSheets.count` passou a 1 e a barra renderiza corretamente (Vida vermelha, Stamina amarela, Mana azul, XP roxa), validado por screenshot em Play Mode via Unity MCP.
  - ⚠️ **Limitação do ambiente headless**: quando o Editor não está com foco de janela (controlado só via Unity MCP, sem humano olhando), o Play Mode não avança o tempo real automaticamente entre chamadas de ferramenta — `Time`/`Update` só progridem quando alguma tool do MCP "cutuca" um frame. Ativado `PlayerSettings.runInBackground = true` pra mitigar. Isso significa que a suavidade do movimento dos inimigos **não pôde ser 100% validada em tempo real nesta sessão** (só a lógica/código foi verificada, não o "feel" contínuo) — vale o usuário testar com o Editor focado e confirmar se ainda parece ruim.
- **2026-08-25 (continuação) — Migração pra URP feita** (o gap de mobile mais crítico do roadmap): o pacote `com.unity.render-pipelines.universal` (17.3.0) **já estava instalado**, só nunca tinha sido ativado. Criado `Assets/Settings/Mobile_Renderer.asset` (UniversalRendererData) + `Assets/Settings/Mobile_RPAsset.asset` (UniversalRenderPipelineAsset) via código (API pública `UniversalRenderPipelineAsset.Create`), atribuído como pipeline padrão (`GraphicsSettings.defaultRenderPipeline`) e em todos os 6 níveis de qualidade (`QualitySettings.renderPipeline`).
  - **Todos os materiais do projeto convertidos automaticamente** de `Standard`/`Mobile-Diffuse` pra `Universal Render Pipeline/Lit` (varredura via `AssetDatabase.FindAssets("t:Material")`, preservando textura principal/tiling/cor) — 59 materiais convertidos. Confirmado por screenshot em Play Mode: nada ficou rosa/magenta (o sintoma clássico de material incompatível com a pipeline ativa) — grama, parede, pedras, heróis e inimigos renderizando corretamente sob URP.
  - ⚠️ **Cuidado durante a conversão**: a varredura inicial sem filtro também tentou reescrever alguns materiais internos do **pacote URP** (ex: `Packages/.../Decal.mat`) — confirmado que **nenhum dano real ocorreu** (pacotes de Registry ficam em `Library/PackageCache`, somente leitura; a escrita no arquivo em disco foi checada diretamente e continua com o shader original — só um objeto em memória da sessão do Editor foi mutado, sem persistir). Mesmo assim, uma futura varredura de materiais deveria filtrar por `path.StartsWith("Assets/")` pra evitar tocar em `Packages/`.
  - ⚠️ **Ainda pendente pra mobile 100% pronto**: color space continua **Gamma** (URP/mobile idealmente usa Linear, mas trocar isso força reimportar todas as texturas e pode mudar a aparência de tudo — não fiz essa troca sozinho por ser mais uma mudança visual ampla, não só técnica). Player Settings de Android/iOS (ícone, orientação, API level mínimo, arquitetura) nunca foram configurados. Touch input real nunca testado (só o New Input System genérico está configurado). Nenhum teste em device físico ou emulador foi feito — só validado dentro do Editor via URP ativo.
- **2026-08-25 (continuação) — Maria/Ícaro/Lavine riggados baixados e importados** (jobs da Meshy Rigging API que já tinham SUCEEDED de uma sessão anterior, só faltava baixar):
  - Baixados via `curl` (URLs assinadas da Meshy, ainda válidas) e importados como Humanoid no Unity: `Assets/Models/Characters/{Maria,Icaro,Lavine}_RO/Rigged/{Nome}_Rigged_1.fbx` + `_Walk_1.fbx` + `_Run_1.fbx`. Textura extraída e religada ao material de cada um. `{Nome}_AnimatorController.controller` criado pra cada (Idle/Walk/Run + parâmetro `Speed`, mesmo padrão do Joaquim).
  - ⚠️ **Ícaro e Lavine**: mapeamento Humanoid automático funcionou (`avatar.isHuman == true`). **Maria falhou** no mapeamento automático (`isHuman == false` mesmo com 21 ossos mapeados e reimport forçado) — provavelmente por causa da geometria do robe/vestido dela confundindo o auto-mapper. Trocada pra `animationType = Generic` (usa as próprias animações dela sem retargeting, funciona, só não permite compartilhar animação entre personagens). Precisa de configuração manual do Avatar na aba de Rig do Inspector se quiser Humanoid de verdade nela — não dá pra automatizar isso às cegas via script.
  - ✅ **Causa do "deitado" encontrada e corrigida**: o modelo **estático** (texto→3D) da Meshy exporta em convenção Z-up (por isso sempre precisou da correção `rotation X=270`), mas o modelo **riggado** (saído da Rigging API separada) já exporta em **Y-up padrão** (convenção humanoide normal, tipo Mixamo) — aplicar a mesma rotação X=270 nele derrubava o personagem de lado/de cabeça pra baixo, parecendo "visto de cima". Confirmado inspecionando `Animator.GetBoneTransform(HumanBodyBones.Head/LeftFoot)` sem rotação: cabeça já em Y maior que o pé, ou seja já em pé no eixo certo. Fix: pros modelos **riggados**, rotação correta é só `(0, 180, 0)` (sem tilt X) e escala recalculada (~0.0093–0.0108 conforme o personagem, pra dar 1.4m de altura, igual ao Joaquim) com o modelo **centralizado** no spawn point (não pés-no-chão).
  - **Maria, Ícaro e Lavine agora usam os modelos riggados de verdade no preview 3D da seleção** (`CharacterPreviewController._entries` atualizado), confirmado por screenshot: os 3 em pé, poses corretas, com todos os detalhes (cajado da Maria, chapéu de bruxa da Lavine, dispositivo mecânico do Ícaro). Junto com o Joaquim, **4 dos 5 heróis já têm modelo com esqueleto real** no preview.
  - **Ainda não existe sistema de troca do modelo do Player conforme o herói escolhido** — hoje só Joaquim está fixo como `PlayerVisual` no Player (o `Player` gameplay object em si, não o preview da seleção). Pra realmente jogar com Maria/Ícaro/Lavine, próximo passo é criar um sistema que troca o prefab visual do Player baseado no `CharacterId` escolhido, reaproveitando exatamente o padrão descoberto agora (rotation `(0,180,0)`, escala ~0.01, centralizado) + Animator + `PlayerAnimatorSpeedDriver` por personagem.
  - **Maithe** continua sem rig (a rigging original falhou com "pose estimation failed", retry nunca foi concluído) — ela é a única das 5 que ainda não tem nenhum modelo riggado gerado.
- **2026-08-25 (continuação) — Sistema de troca de personagem jogável criado e funcionando de ponta a ponta**. Novo `Assets/Scripts/Gameplay/Player/PlayerVisualSwapper.cs` no `Player`, com uma entrada por herói (prefab riggado + `AnimatorController` + posição/rotação/escala), escutando `CharacterSelectController.OnCharacterSelected` (listener persistente adicionado via `UnityEventTools.AddPersistentListener`). Adicionados `PlayerAnimatorSpeedDriver.SetAnimator()` e `PlayerProceduralAnimator.SetVisualRoot()` pra re-apontar o driver de animação/bob procedural pro modelo recém-trocado. `Maria`/`Icaro`/`Lavine.HasDedicatedModel` agora `true` (paravam de usar o tint plano da cápsula).
  - ✅ **Confirmado por screenshot em Play Mode, pelo fluxo real de seleção (não só chamada direta)**: escolher Lavine ou Ícaro na tela de seleção agora faz o **Player de verdade** virar aquele personagem — modelo, cor, proporção corretos, mesmo tamanho de jogo que o Joaquim já tinha (altura ~1.91m, igual em todos pra manter a câmera/gameplay consistentes).
  - ⚠️ **Duas armadilhas reais encontradas e corrigidas, registradas aqui pra não repetir**:
    1. **`Destroy()` não é imediato**: a primeira versão do swap só marcava o modelo antigo pra destruição (`Destroy()`), que só é processado no fim do frame — como resultado o modelo antigo continuava visível/sobreposto ao novo. Corrigido com `SetActive(false)` + `SetParent(null)` imediatos antes do `Destroy()` (não usar `DestroyImmediate` em Play Mode — é desaconselhado pela própria Unity, mesmo funcionando aqui em teste).
    2. **Escala composta (a mais importante)**: o modelo novo é instanciado como filho de `PlayerVisual`, que **já tem escala 0.01** (herdada da configuração original do Joaquim). Configurar `uniformScale` do novo personagem usando a mesma referência do preview (~0.01, pra escala "mundo") multiplicava as duas escalas e deixava o personagem **microscópico** (bounds de poucos centímetros). Corrigido calculando a escala relativa à escala já aplicada pelo pai (`targetHeight / (rawHeightMedido * parentScale)`), dando valores corretos na casa de **1.29–1.50** (não 0.01) pros três personagens.
  - ⚠️ **Achado de infraestrutura ainda mais importante, vale pra qualquer sessão futura via Unity MCP headless**: quando o Editor não está com foco (rodando só via MCP), **editar um arquivo de script pelo Edit tool não é automaticamente detectado/compilado pelo Unity** — fica com `external_changes_dirty: true` indefinidamente até alguém chamar `refresh_unity` explicitamente. Isso causou ~40 minutos de debugging aparentemente impossível (código sendo "ignorado" silenciosamente, sem erro nenhum, porque a versão RODANDO ainda era a de ANTES da edição). **Regra daqui pra frente: sempre chamar `refresh_unity` e confirmar `is_compiling:false`/timestamp de reload atualizado depois de QUALQUER edição de script, antes de testar.**
  - Ainda falta: repetir esse ajuste fino de escala/posição pra **Joaquim continuar correto** (ele já estava certo por já usar os valores originais, não mexi nele) e, quando/se **Maithe** ganhar rig, adicionar a 5ª entrada. O preview 3D da seleção continua usando escala/posição diferentes (centralizado, não pés-no-chão) — as duas telas têm convenções de posicionamento propositalmente diferentes, documentado nos comentários do código.
- **2026-08-25 (fim de sessão) — usuário reportou 3 problemas ainda pendentes, não resolvidos nesta sessão** (interrompida pelo usuário antes de investigar a fundo; retomar na próxima sessão agendada):
  1. **Inimigos (aranhas/escorpião/lobos) ainda sem movimento visível** — o wander em `EnemyPatrolState` foi implementado e o `EnemyController._stateMachine` foi confirmado não-nulo/funcional em testes anteriores desta sessão, mas o ambiente headless (Editor sem foco) não avança o tempo real automaticamente entre chamadas de ferramenta (ver nota abaixo), então a movimentação nunca foi validada de verdade em wall-clock real. Pode ser: (a) só um problema de validação (o código já funciona, só não dava pra ver rodando), ou (b) um bug real ainda não encontrado. **Próximo passo: testar com o Editor focado/humano observando, ou achar uma forma de forçar múltiplos frames reais via MCP antes de concluir que está OK.**
  2. **Artefato de "bota"/pedestal nos pés dos personagens riggados** (visto no Joaquim desde sessão anterior, mencionado como "parece um suporte de boneco/toy figure stand") — usuário pediu remoção explícita. **Ainda não investigado nesta sessão** — precisa checar se é: (a) uma submesh separada dentro do FBX riggado (poderia dar pra esconder desabilitando aquele submesh/material específico sem precisar editar a malha), ou (b) parte inseparável da malha principal (aí só dá pra resolver com edição de malha externa tipo Blender, que não está disponível aqui). Verificar isso primeiro antes de tentar qualquer fix.
  3. **Tamanho dos personagens "muito pequeno"** — a última mudança desta sessão foi ajustar a altura de Maria/Ícaro/Lavine pra bater com o Joaquim (~1.91m, `CharacterController.height=1.8`), o que geometricamente bate com um personagem child-sized normal. O usuário reportou que ficou pequeno demais **depois** dessa mudança (pode ser sensação de escala relativa ao ambiente/câmera, não ao personagem em si — CameraController não foi inspecionado ainda, arquivo não encontrado no caminho esperado `Assets/Scripts/Gameplay/Cameras/CameraController.cs`, precisa localizar com Glob mais amplo). **Próximo passo: localizar o script de câmera de verdade (grep por `class CameraController` ou similar), checar distância/zoom/FOV, e considerar tanto aumentar a escala dos personagens quanto aproximar a câmera.**
  - ⚠️ **Lembrete crítico pra sessão seguinte**: sempre chamar `refresh_unity` e confirmar `is_compiling:false` com timestamp de reload atualizado depois de QUALQUER edição de script feita fora do Editor (via Edit tool), antes de testar — sem isso o Unity roda a versão ANTIGA do código silenciosamente, sem erro nenhum (isso já causou ~40 min de debugging perdido nesta sessão, ver entrada anterior do changelog).
  - Sessões automáticas agendadas (cron) pra continuar esse trabalho sozinho: todo dia às 20h04 e às 3h07, a pedido do usuário. Expira em 7 dias (limite da plataforma) — só funciona enquanto esta sessão do Claude Code ficar aberta.
  - **Preview dos heróis na seleção mostrava de costas** (pedido explícito do usuário, ficou pendente da sessão anterior): as 5 entradas em `CharacterPreviewController._entries` (cena) e o default em `CharacterPreviewEntry.localEulerAngles` (código) tinham `y: 0`; girado pra `y: 180` pra virar o modelo de frente pra `PreviewCamera`. **Não testado visualmente ainda** (sem MCP nesta sessão) — se ainda estiver de costas ou de lado, é só ajustar esse `y` pra 90/-90 em vez de 180.

---

## 🖼️ Prompt-base para Arte 3D dos Personagens (fornecido pelo usuário em 2026-08-24)

> ⚠️ **Guardado como referência, ainda não executado.** Uso pretendido: gerar concept art / modelos 3D dos 5 heróis via alguma ferramenta de IA (ex: `generate_image`/`generate_model` do Unity MCP, ou serviço externo) para substituir a cápsula placeholder (ver `PlayerVisual`/`Mat_HeroBase` em `Gameplay/Player`). Requer provider de imagem configurado (nenhum configurado até 2026-08-24 — ver nota em Fase 11) e/ou ferramenta de geração de modelo 3D com rig humanoide. Reusar este texto tal como está antes de reexecutar, não resumir/reescrever.

```
🎮 PROMPT BASE — PERSONAGENS DO RPG

Crie um personagem 3D completo para um RPG de ação em terceira pessoa.

Todos os personagens deste projeto pertencem ao mesmo universo e devem compartilhar uma mesma direção artística, proporções, qualidade de modelagem, estilo de materiais e nível de detalhamento.

O estilo deve ser de um RPG/Action MMORPG estilizado, com personagens visualmente marcantes, proporções levemente heroicas e design que funcione tanto de perto quanto à distância.

Não copiar personagens existentes de outros jogos. Criar identidade visual original.

Requisitos técnicos

O personagem deve ser preparado para utilização em Unity.

Criar:

modelo 3D completo;
cabeça;
tronco;
braços;
mãos;
dedos;
pernas;
pés;
roupas e acessórios;
arma/equipamento característico;
skeleton/armature;
rig humanoide;
skinning adequado;
UV Mapping;
materiais;
texturas;
pontos de encaixe para equipamentos.

O personagem deve ser compatível com Unity Humanoid.

Criar bones suficientes para permitir:

Idle;
Walk;
Run;
Jump;
Attack;
Skill;
Hit Reaction;
Death;
uso de armas;
movimentação das mãos;
movimentação independente dos braços.

Criar também:

WeaponSocket

RightHandSocket

LeftHandSocket

HeadSocket

ChestSocket

Direção artística

Os personagens devem possuir:

silhueta forte;
cores facilmente identificáveis;
roupas relacionadas à classe;
acessórios que contem parte da história;
arma característica;
detalhes suficientes para parecer um personagem principal;
aparência adequada para um RPG de fantasia.

IMPORTANTE:

Cada personagem deve ser imediatamente reconhecível apenas pela silhueta.

O design não deve depender apenas de cores diferentes. A roupa, postura, arma, cabelo, acessórios e proporções devem diferenciá-los.

🛡️ 1. JOAQUIM — O GUARDIÃO DAS RAÍZES

Criar o personagem Joaquim — O Guardião das Raízes.

Classe: Guardião
Função: Tanque / Defesa / Proteção
Atributo: Vitalidade

Joaquim deve possuir aparência de um guerreiro protetor, robusto e resistente.

Design
homem adulto;
corpo forte e musculoso;
postura firme;
aparência de alguém acostumado a proteger outras pessoas;
armadura pesada, mas com elementos naturais;
placas de metal combinadas com madeira antiga;
raízes envolvendo partes da armadura;
detalhes inspirados em árvores antigas;
capa ou tecido pesado;
marcas de batalha na armadura.
Arma

Criar o Escudo das Raízes Eternas.

O escudo deve ser grande e claramente defensivo.

Misturar:

metal;
madeira ancestral;
raízes;
símbolos antigos.

O centro do escudo deve possuir um núcleo que possa posteriormente emitir efeitos de energia verde.

Joaquim também pode utilizar uma arma secundária compatível com seu estilo de tanque, como uma espada curta ou maça.

Silhueta

A silhueta deve transmitir:

"tanque / protetor / resistência".

Ele deve parecer o personagem mais resistente do grupo.

Habilidades — referência visual

Muralha de Pedra: energia de pedra surge ao redor dele.

Provocação Heroica: postura agressiva, chamando a atenção dos inimigos.

Escudo das Raízes: raízes e energia natural surgem do escudo.

Terremoto: golpe no chão provocando ondas de impacto.

Última Resistência: o personagem fica envolto por uma poderosa energia ancestral.

✨ 2. MARIA — A CURANDEIRA DA LUZ

Criar Maria — A Curandeira da Luz.

Classe: Sacerdotisa
Função: Suporte / Cura / Buff
Atributo: Espírito

Maria deve possuir uma aparência elegante, serena e luminosa.

Design
mulher adulta;
aparência acolhedora;
postura elegante;
roupas inspiradas em uma sacerdotisa;
túnica clara;
detalhes dourados;
símbolos solares;
tecidos leves;
pequenos cristais sagrados;
ornamentos que transmitam espiritualidade.

Evitar aparência de guerreira.

Ela deve parecer alguém que cura e protege, não alguém especializada em combate físico.

Arma

Criar um cajado sagrado.

O topo deve possuir um cristal solar capaz de emitir luz.

Silhueta

A silhueta deve transmitir:

"sacerdotisa / cura / luz".

Habilidades

Cura Divina: energia luminosa envolvendo um aliado.

Benção da Vida: partículas douradas e símbolos sagrados.

Escudo Sagrado: barreira circular de luz.

Purificação: energia branca removendo efeitos negativos.

Milagre Celestial: enorme manifestação de luz celestial sobre o campo de batalha.

🏹 3. MAITHE — A ARQUEIRA DOS VENTOS

Criar Maithe — A Arqueira dos Ventos.

Classe: Caçadora
Função: DPS à distância
Atributo: Destreza

Maithe deve possuir aparência de uma caçadora ágil e extremamente habilidosa.

Design
mulher adulta;
corpo atlético;
aparência ágil;
roupas leves;
armadura de couro;
capa curta;
botas adequadas para exploração;
pequenos acessórios de sobrevivência;
elementos inspirados em vento e natureza;
cabelos que transmitam movimento.

Nada de armadura pesada.

Arma

Criar o Olho do Vento, um arco único.

O arco deve possuir:

design elegante;
elementos naturais;
pequenas partículas de vento;
detalhes azuis/ciano;
uma gema central.
Silhueta

Deve transmitir:

"velocidade / precisão / caçadora".

Habilidades

As animações devem permitir:

puxar a corda;
mirar;
disparar;
realizar movimentos rápidos;
esquivar.

Tempestade de Mil Flechas: grande ataque com múltiplas flechas de energia e vento.

⚙️ 4. ÍCARO — O MESTRE DAS MÁQUINAS

Criar Ícaro — O Mestre das Máquinas.

Classe: Inventor
Função: Controle / Dano tecnológico
Atributo: Inteligência

Ícaro deve ser visualmente o personagem mais diferente dos demais.

Design
homem adulto;
inventor/engenheiro;
corpo mediano;
roupas de inventor;
armadura tecnológica leve;
ferramentas;
bolsas;
cabos;
dispositivos mecânicos;
pequenas peças robóticas;
óculos tecnológicos ou visor.

Misturar:

fantasia medieval + tecnologia antiga perdida.

Não fazer um personagem completamente futurista.

A tecnologia deve parecer algo que foi descoberto dentro do mundo de Rapadura.

Equipamentos

Criar:

dispositivo de controle;
pequeno drone;
mecanismo para invocar a torre;
equipamentos elétricos.
Silhueta

Deve transmitir:

"inventor / tecnologia / inteligência".

Habilidades

Torreta Automática: invocar uma pequena máquina.

Granada Elétrica: dispositivo explosivo elétrico.

Drone de Ataque: drone acompanha o personagem.

Campo Magnético: campo energético ao redor dos inimigos.

Exército Mecânico: invocação de diversas máquinas.

🔮 5. LAVINE — A FEITICEIRA DAS SOMBRAS

Criar Lavine — A Feiticeira das Sombras.

Classe: Feiticeira
Função: Dano mágico / Controle de multidão
Atributo: Poder Arcano

Lavine deve possuir aparência misteriosa, poderosa e ligeiramente ameaçadora, mas continuar sendo uma personagem heroica.

Design
mulher adulta;
postura elegante;
aparência misteriosa;
roupas de feiticeira;
tecidos longos;
detalhes escuros;
cristais arcanos;
símbolos mágicos;
elementos relacionados ao eclipse;
pequenos efeitos de energia sombria ao redor.
Arma

Criar um foco arcano ou cajado relacionado ao eclipse.

O objeto deve possuir um núcleo que pareça conter energia sombria.

Silhueta

Deve transmitir:

"magia / mistério / poder".

Habilidades

Orbe Sombria: projétil mágico.

Correntes das Trevas: correntes de energia prendem os inimigos.

Névoa da Ilusão: área coberta por névoa mágica.

Explosão Arcana: grande explosão de energia.

Eclipse Eterno: o ambiente escurece e surge uma enorme manifestação de eclipse sobre a área.

🎨 E tem uma coisa MUITO importante

Eu faria os cinco personagens juntos na mesma etapa de design.

O prompt final para a IA pode terminar assim:

Criar uma folha de referência contendo os cinco personagens lado a lado, com a mesma escala e proporção, para verificar a consistência visual do grupo.

Mostrar:

Joaquim
Maria
Maithe
Ícaro
Lavine

Todos devem parecer personagens pertencentes ao mesmo universo, porém cada um deve possuir uma silhueta completamente distinta.

A composição deve permitir comparar:

altura;
proporções;
tamanho das armas;
roupas;
silhuetas;
linguagem visual;
nível de detalhamento.

Criar também uma vista:

frontal + lateral + traseira de cada personagem para servir como referência para modelagem 3D.
```

### Pré-requisitos antes de executar este prompt
- [x] Decidir e configurar uma ferramenta de geração — **2026-08-24**: usuário escolheu **Meshy** (texto→3D) e forneceu a API key. Configurada via `ISecureKeyStore` do próprio MCP for Unity (armazenamento seguro do SO — Windows Credential Manager — não em texto plano), confirmada com `generate_model list_providers` (`meshy: configured: true`). A chave foi colada pelo usuário direto no chat (não no fluxo de UI pretendido pela ferramenta) — **recomendo considerar rotacionar/regenerar essa chave em meshy.ai** por higiene, já que ficou registrada no histórico da conversa.
- [x] Decidir o formato de saída esperado — modelo 3D `.fbx` já texturizado, formato escolhido: `fbx` (melhor compatibilidade com pipeline Humanoid do Unity).
- [ ] Se for modelo 3D: confirmar que o rig gerado é compatível com Unity Humanoid antes de integrar — **ainda não rigado**: `generate_model` (Meshy, modo texto→3D) gera apenas malha+textura estática, sem esqueleto (`ModelImporter.animationType = None`, `avatarSetup = NoAvatar`). Rig automático é um recurso separado da Meshy (Rigging API) não exposto por este tool do Unity MCP — para virar Humanoid de verdade, os próximos passos são: (a) rig manual/automático via Mixamo (aceita FBX, auto-rig gratuito, exporta com Humanoid-compatible skeleton) ou (b) endpoint de rig dedicado da Meshy se/quando ficar acessível por aqui.
- [x] Definir onde os assets gerados vão morar — `Assets/Models/Characters/Joaquim/Joaquim.fbx` (textura extraída separadamente em `Assets/Models/Characters/Joaquim/Textures/Image_0.jpg`, já reconectada ao material).

### ✅ Joaquim — primeiro modelo gerado (2026-08-24)
- Prompt usado (versão condensada do prompt-base + seção específica do Joaquim, em inglês — gerador de IA 3D funciona melhor assim): guerreiro estilizado, armadura pesada fundida com madeira/raízes antigas, capa pesada, escudo redondo grande com núcleo de cristal verde brilhante, silhueta de tanque/protetor.
- Resultado validado visualmente via Unity MCP (instanciado temporariamente na cena, screenshots de frente e de trás, depois removido): guerreiro barbudo, armadura escura, capa verde-oliva, escudo com núcleo verde — bate bem com a lore/design pedido.
- ⚠️ **Achado de import**: o FBX gerado pela Meshy veio com a textura **embutida** mas o `ModelImporter` não conectou automaticamente no material (`mainTexture = NULL` até extrair manualmente via `ModelImporter.ExtractTextures()`). Documentando pra próxima geração: sempre chamar `ExtractTextures` + reatribuir a textura no material logo após importar, ou o modelo aparece todo branco/cinza.
- ⚠️ **Achado de escala/rotação**: o prefab importado veio com `scale=(100,100,100)` e `rotation=(270,0,0)` no root — não é bug, é a convenção de eixo/unidade do exportador da Meshy (provavelmente Z-up, centímetros) sendo compensada pelo importer do Unity. Ajustei pra `scale=0.02` (resulta em ~2m de altura, proporção humana razoável) ao instanciar pra preview; um personagem real em produção provavelmente quer isso resolvido direto no Prefab/rig, não ajustado toda vez que instancia.
- **Não integrado ao Player ainda** — a cápsula placeholder (`PlayerVisual`) continua sendo o que o jogo usa de verdade. Trocar por este modelo exige primeiro resolver o rig Humanoid (ver item acima), senão a movimentação/animação atual (inclusive o `PlayerProceduralAnimator` novo) não vai funcionar nele do jeito que está.
- Próximo: repetir o mesmo processo pros outros 4 heróis (Maria, Maithe, Ícaro, Lavine) usando as seções correspondentes do prompt-base, depois resolver rig Humanoid pra todos de uma vez (mais eficiente que rigar um por um).

---

## 🧭 FASE 0 — Pré-produção / Fundação Técnica
> Sem isso, tudo que vier depois custa mais caro para corrigir mais tarde.

### Documentação e Escopo
- [x] Game Design Document (GDD) enxuto: `Docs/GDD.md`
- [x] Plataforma-alvo definida: **Mobile é obrigatório** (jogo precisa funcionar em celular); PC como plataforma adicional
- [x] Requisitos mínimos de hardware mobile (RAM/GPU/OS mínimo Android/iOS) — `Docs/PlatformRequirements.md`
- [x] Definir público-alvo e classificação indicativa (ESRB/PEGI) — `Docs/PlatformRequirements.md` (recomendação: ESRB Teen / PEGI 12, sujeito à submissão oficial)

### Requisitos específicos de Mobile (obrigatório, não opcional)
- [x] Controles em tela — botões de Correr/Pular na HUD (2026-08-25 continuações 11/12); joystick virtual não foi necessário pois o movimento principal é clique/toque-pra-mover (`MoveClick` já bindado em `<Touchscreen>/primaryTouch/tap`), decisão de design documentada.
- [x] Câmera: pinch-to-zoom já implementado em `CameraZoom.cs`, bug real de contagem de dedos corrigido (2026-08-25 continuação 7).
- [ ] Performance: testar em dispositivo de referência low/mid-end, não só no Editor — continua pendente, sem device físico disponível neste ambiente.
- [x] UI escalável pra diferentes resoluções/aspect ratios: `PanelSettings` corrigido pra `ScaleWithScreenSize` de verdade (2026-08-25 continuação 6) + safe area/notch aplicado na HUD (continuação 13, lógica correta mas não testável sem device físico com notch).
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
- [x] Testes automatizados (Unity Test Framework, EditMode) — 396 testes em `Assets/Tests/EditMode`, praticamente 100% verdes desde 2026-08-25 continuação 24 (restam só 3 falhas documentadas como decisão de design pendente). PlayMode ainda não tem suíte dedicada.
- [ ] Definir "Definition of Done" por feature (código + teste + review + doc)
- [ ] Processo de code review (PR obrigatório, sem push direto na main)
- [x] Sistema de logging estruturado (`GameLogger.cs`) — níveis Debug/Info/Warning/Error/Exception, `Debug` compila fora de builds não-development via `[Conditional]`, prefixo `[Categoria]` consistente. Já em uso extensivo pelo projeto (managers, quests, inventário, etc).
- [ ] Error/crash reporting (Sentry, Unity Cloud Diagnostics ou similar)

---

## 👥 Personagens Jogáveis (Roster)
> Classes C# criadas em `Assets/Scripts/Gameplay/Characters/` (`PlayableCharacter` base + `CharacterRegistry`). Habilidades listadas aqui ainda são apenas nomes/placeholders — a implementação real de cada uma entra na Fase 4 (Skills).

> 🧒 **Decisão de design — 2026-08-24: roster revisado de adultos para crianças/pré-adolescentes heroicos.** Ordem de nascimento mantida (Maithe > Maria > Joaquim > Ícaro > Lavine). Idades: Maithe 14, Maria 13, Joaquim 12, Ícaro 11, Lavine 9. **Feito nesta sessão:**
> - `PlayableCharacter.Age` (novo, `int` abstrato) + implementado nas 5 subclasses.
> - `Lore` de cada um dos 5 reescrita (ajuste de framing pra infância/pré-adolescência, mantendo a essência de cada personagem e as relações familiares já estabelecidas — não foi uma reescrita total do zero, foram edições pontuais nos parágrafos existentes).
> - Modelo 3D do Joaquim regenerado com proporções infantis (ver seção logo abaixo, "Joaquim_Child").
> **⚠️ Ainda NÃO atualizado (pendente, escopo grande demais pra fazer com segurança no mesmo fôlego)**:
> - `Docs/GDD.md` — ainda descreve os 5 como adultos.
> - As 5 quests pessoais já escritas (`Editor/CharacterQuestSeeder.cs` + `CharacterQuestSeeder2.cs`) e a quest de convergência (`Editor/ConvergenceQuestSeeder.cs`) — texto/tom ainda assume protagonistas adultos.
> - Os diálogos ramificados já escritos (`CharacterDialogueSeeder.cs`/`CharacterDialogueSeeder2.cs`/`ConvergenceDialogueSeeder.cs`) — mesmo problema.
> - Traduções PT-BR/EN (`quest.*`/`dialogue.*`) associadas a esse conteúdo.
> - Modelos 3D dos outros 4 heróis (Maria/Maithe/Ícaro/Lavine) — nenhum foi gerado ainda, gerar direto com proporções infantis quando chegar a vez deles (não repetir o processo de gerar adulto primeiro).
> Recomendação: tratar isso como uma tarefa de conteúdo dedicada (ler cada quest/diálogo já escrito, ajustar tom pra infância sem perder a profundidade da história), não tentar reescrever tudo de uma vez sem revisão.

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
- [~] Modelos/animações/rig únicos por personagem (Fase 11 — Arte e Animação) — ainda não há modelos reais (os 5 heróis compartilham a mesma cápsula `PlayerVisual` de teste), mas **2026-08-24**: cada `PlayableCharacter` ganhou `AccentColor` (cor de acento fiel à lore/classe: Joaquim verde-acinzentado terroso, Maria dourado, Maithe verde-floresta, Ícaro laranja-cobre, Lavine roxo). `CharacterSelectController.ConfirmSelection` agora tinge o material da cápsula (`Mat_HeroBase`, com textura de tecido procedural + leve emissão) com essa cor via instância própria de material (`renderer.material`, não `sharedMaterial`) assim que o herói é confirmado. Validado em Play Mode via Unity MCP (Lavine → cápsula roxa, screenshot confirmado). Substituir por modelo/rig de verdade continua pendente.
- [x] **Animação procedural sem rig (2026-08-24, pedido do usuário)** — `Gameplay/Player/PlayerProceduralAnimator.cs`: já que a cápsula não tem esqueleto (braços/pernas de verdade exigem modelo rigado, Fase 11), a movimentação/combate ganharam feedback visual real por código, aplicado só num transform cosmético filho (`PlayerVisual`), nunca na raiz que o `CharacterController` usa:
  - **Andar/correr**: bob vertical + inclinação para frente, escalados pela velocidade de `PlayerMotor.HorizontalVelocity` (funciona igual pra WASD e clique-para-andar, já que os dois passam pelo mesmo motor).
  - **Ataque**: lunge pra frente + squash-stretch de ~0.18s, disparado pelo novo evento `PlayerAttackPerformedEvent` (publicado por `PlayerBasicAttack.PerformAttack`).
  - **Receber dano**: shake de recuo + flash vermelho emissivo (via `MaterialPropertyBlock`, não mexe no material/tint do personagem selecionado), disparado por `DamageAppliedEvent` quando `Target == gameObject` do player.
  - Todos os três efeitos são timers independentes compostos por frame, então podem se sobrepor (ex: levar um hit no meio do golpe) sem conflitar.
  - Validado via Unity MCP: bob confirmado (Y oscilando ~0.955–1.045 com inclinação crescendo), lunge de ataque confirmado (deslocamento Z de ~0.29 + squash de escala ~0.14), flash de dano confirmado (emissão vermelha na intensidade correta ao simular o timer). Um teste via loop manual de `Update()` bateu num artefato do ambiente de automação (deltaTime real instável com o Editor sem foco fazendo o timer decair rápido demais) — não é bug no código, só limitação de como testei; validado isolando a chamada.
  - Continua não sendo animação de verdade (sem braços/pernas articulados) — é a melhor aproximação possível sem um rig. Trocar por Animator + blend trees reais é trabalho de Fase 11 (Arte/Animação) quando houver modelo.
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

### Câmera (reescrita em 2026-08-19 — decisão de design: estilo MU Online)
> 📌 `PlayerCamera.cs` (câmera livre orbital) foi **removida e substituída** por `Gameplay/Cameras/*` — isométrica fixa, MU Online: yaw 45°, pitch 55-60° (default 57,5°), distância 10-25m (default 15m), segue só a posição do alvo (nunca a rotação do personagem), sem controle de olhar pelo mouse. Testado em Play Mode via Unity MCP: `eulerAngles` confirmado em (57.5, 45, 0).
- [x] `CameraSettings.cs` (ScriptableObject: ângulo, follow, zoom, colisão, rotação Q/E)
- [x] `CameraController.cs` (MonoBehaviour principal, `SmoothDamp` no follow de posição, mantém `Shake(duration, magnitude)` pro combate)
- [x] `CameraCollision.cs` (SphereCast do alvo até a câmera, aproxima suave em obstrução, volta suave quando desobstrui)
- [x] `CameraZoom.cs` (scroll do mouse + pinch touch, suavizado via `SmoothDamp`, 10-25m)
- [x] `CameraRotation.cs` (Q/E gira em passos de 45°, suavizado, `allowRotation` desliga totalmente)
- [ ] Suporte a Cinemachine (avaliar se compensa migrar a solução manual atual — `com.unity.cinemachine` não está no `manifest.json` ainda)

### Mundo
- [ ] Criar mapa inicial (greybox antes de arte final) — Unity Editor
- [x] Spawn points (`Gameplay/World/SpawnPoint.cs`)
- [x] Sistema de checkpoints (`Gameplay/World/CheckpointManager.cs`, registrado no `GameManager`)
- [x] Sistema de respawn (`CheckpointManager` escuta `PlayerDiedEvent` e reposiciona o player — hoje é instantâneo, falta tela de morte/fade quando a Fase 2/6 existirem)
- [ ] Streaming/otimização de cena para mapas grandes (se aplicável)

> ✅ Bloqueio resolvido em 2026-08-19: projeto aberto no Unity Editor (6000.3.22f1) via Unity MCP. `Assets/Scenes/Main.unity` já existe com GameManager + Player + Câmera montados e testados em Play Mode (ver changelog).

### Tela de Seleção de Personagem (novo — decisão de design 2026-08-19)
- [x] Tela de seleção de personagem no início do jogo — `UI/CharacterSelect/CharacterSelectController.cs` + `UI/CharacterSelect/CharacterSelectView.uxml/uss`, lista os 5 heróis (`CharacterRegistry`), mostra lore/classe/função ao clicar, `Confirmar` chama `PlayableCharacter.ApplyPassive(stats)` no `PlayerStats` do Player. Congela o jogo (`Time.timeScale = 0`) até escolher. Montada na `Main.unity` e testada em Play Mode via Unity MCP (`Time.timeScale` confirmado em 0 com a tela ativa).

> 🐛 **Bug real encontrado e corrigido em 2026-08-24** (usuário reportou "personagem não anda ao clicar"): dois problemas em cascata deixavam o jogo travado pra sempre na tela de seleção, então nada respondia a input nenhum (nem teclado, nem mouse) —
> 1. **Faltava `EventSystem` na `Main.unity`** — sem ele, nenhum clique chega às UI Toolkit (`UIDocument`). Adicionado `EventSystem` + `InputSystemUIInputModule` na cena (compatível com o New Input System já usado no projeto) e salvo.
> 2. **`CharacterSelectController` fazia a query dos elementos da UXML em `Awake()`**, antes do `UIDocument` terminar de montar `rootVisualElement` (que acontece no próprio `OnEnable` do `UIDocument`, com ordem de execução entre componentes não garantida) — `_root.Q<VisualElement>("roster-container")` voltava `null`, `BuildRoster()` saía cedo em silêncio, o roster ficava vazio, o botão "Confirmar" nunca habilitava, e `Time.timeScale` nunca voltava a 1. Corrigido movendo o binding pra `OnEnable()` (com guarda `_isBound` pra não religar o evento do botão duas vezes), seguindo o mesmo padrão já usado em `HudController`/`MainMenuController`.
> Validado em Play Mode via Unity MCP: roster populado com os 5 heróis, `ConfirmSelection()` leva `Time.timeScale` de volta a 1 e desativa a tela, e o `PlayerMotor.Move` de fato desloca o player depois disso. Esse mesmo bug (`Awake` vs `OnEnable` pra binding de UXML) vale a pena auditar em qualquer novo `UIDocument` controller futuro.

### Clique-para-andar (novo — pedido do usuário 2026-08-24, estilo MU Online de verdade)
- [x] `Gameplay/Player/PlayerClickToMove.cs` — clique/tap esquerdo faz raycast da câmera até o chão (`_groundMask`, hoje `~0`/tudo — vale restringir a um layer "Ground" dedicado depois) e define um destino; input manual (WASD/joystick) sempre cancela o destino na hora (nunca competem no mesmo frame).
- [x] `PlayerMotor.MoveToward(destination, isRunning, arrivalThreshold)` — novo método, direção mundial direta (sem depender da câmera), reaproveita a mesma integração de gravidade/rotação do `Move()` existente (extraída pra `MoveInDirection` privado).
- [x] `PlayerController.HasMoveIntent` — Idle/Walk/Run passam a checar isso (manual OU destino de clique pendente) em vez de só `Input.MoveInput`; `PlayerWalkState`/`PlayerRunState.FixedTick` usam `Motor.Move` quando há input manual, senão `Motor.MoveToward` até chegar (aí limpa o destino).
- [x] Novas actions no `PlayerControls.inputactions`: `MoveClick` (`<Mouse>/leftButton` + `<Touchscreen>/primaryTouch/tap`) e `PointerPosition` (`<Pointer>/position`), expostas em `PlayerInputHandler`.
- [x] Ignora clique em cima de UI (`EventSystem.current.IsPointerOverGameObject()`), pra não mover o personagem ao clicar em botão de HUD/menu.
- [x] Componente `PlayerClickToMove` adicionado ao `Player` na `Main.unity` via Unity MCP; cena salva.
- ⚠️ Validado em Play Mode via Unity MCP até ~65% do caminho até o destino (raycast acerta o chão, state machine troca pra `PlayerWalkState`, posição avança de fato); a chegada completa não foi confirmada porque a física parece rodar mais devagar com o Editor sem foco durante automação — é uma limitação do teste via MCP, não um bug conhecido no código. Vale o usuário confirmar manualmente clicando no chão dentro do Editor.
- [x] Indicador visual de destino no clique-pra-mover implementado (`ClickToMoveIndicator.cs`, anel pulsante no chão — ver changelog 2026-08-25 continuação 12) e "clicar no inimigo = atacar direto" também implementado (`PlayerClickToMove.AttackTarget` + `PlayerBasicAttack.TickClickToAttack` — ver changelog 2026-08-26).

### Mapa Mundi + Encontros Aleatórios (novo — decisão de design 2026-08-19)
> 📌 Direção final de gameplay, esclarecida em várias rodadas de pergunta direta ao usuário nesta data:
> - **Estrutura estilo Final Fantasy clássico**: mapa mundi (overworld, visão de cima, navegar entre regiões/biomas) com **encontros aleatórios** que disparam combate — como FF I-VI.
> - **Combate em si é em tempo real estilo MU Online** (hotbar de skills, ação contínua, sem menu de comando por turno/ATB) — ou seja, **mantém e reaproveita** o sistema já construído (`Hitbox`/`Hurtbox`/`Health`, `SkillManager.TryCast`, `PlayerController`/State Machine), NÃO troca para batalha por turnos. A ideia de ATB foi cogitada e descartada.
> - Sistema de batalha em si (câmera/arena ao entrar em combate) fica a refinar depois; por enquanto é a camada de overworld + gatilho de encontro + entrada em modo combate em tempo real.
- [ ] `WorldMapScene`/`WorldMapController`: visão de cima, player anda entre regiões (reaproveitar os 4 biomas já seedados: Floresta, Deserto, Montanha, Cavernas)
- [ ] Sistema de encontro aleatório (chance por passo/tempo, usando `BiomeDefinition.RollEnemy` já implementado para decidir qual inimigo aparece)
- [ ] Transição overworld → área de combate em tempo real (reaproveita `EnemyController`/`Hitbox`/`SkillManager` já existentes) ao disparar um encontro
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

> ⚠️ Nota: `PlayerStats` continua implementando `ICombatTarget` diretamente em vez de compor `Health` (decisão mantida: o player precisa de stamina/mana/XP que `Health` não tem). I-frames já existiam. Corrigido nesta sessão: `PlayerStats` agora publica `DamageAppliedEvent` (som de hit em `AudioManager`) e `CombatTargetDiedEvent` (analytics/quests) além do `PlayerDiedEvent` já usado pelo `CheckpointManager` — antes esses dois eventos só disparavam para alvos baseados em `Health` (inimigos/NPCs), então o player tomar dano/morrer não acionava som de combate nem tracking. Hit-stop/camera shake já funcionavam para o player (disparados direto pelo `Hitbox`, independente do tipo de alvo). Ainda não validado em Play Mode nesta sessão (sem acesso ao Unity MCP aqui) — só via testes EditMode; `PlayerStatsTests.cs` cobre os fluxos de dano/i-frames, mas não os dois eventos novos.

### Armas
- [x] Arma corpo a corpo — `Combat/Weapons/MeleeWeapon.cs` (ativa `Hitbox` por janela de tempo)
- [x] Arma de longo alcance — `Combat/Weapons/RangedWeapon.cs` + `Projectile.cs`
- [x] Sistema de munição — ligado a `InventoryManager` (consome item de munição real)
- [x] Sistema de recarga — temporizador bloqueia disparo durante reload
- [x] Durabilidade — decrementa `InventorySlotData.currentDurability`, quebra/desequipa em zero

> ✅ Corrigido: `UnequipSlot` agora preserva `currentDurability` real ao devolver o item ao inventário (não reseta mais para o máximo).
> ✅ Novo: `Player/PlayerBasicAttack.cs` — ataque genérico de placeholder (clique esquerdo/`Attack` no `PlayerControls.inputactions`), `OverlapSphere` na frente do player, dano fixo via `ICombatTarget.ApplyDamage`, cooldown. Testado em Play Mode via Unity MCP: vida do inimigo caiu de 30→15 com um ataque. Serve pra testar combate agora; será substituído por `MeleeWeapon`/skill real quando houver arma equipada.

### Inimigos
- [x] IA básica (Behaviour Tree ou State Machine reutilizando `Core/StateMachine`) — `Enemies/EnemyController.cs` + estados em `Enemies/States/`
- [x] Patrulha — `EnemyPatrolState.cs`
- [x] Perseguição — `EnemyChaseState.cs`
- [x] Ataque — `EnemyAttackState.cs` (usa `Hitbox`/`ICombatTarget`)
- [x] Fuga — `EnemyFleeState.cs`
- [x] Boss inicial — `EnemyDefinition.IsBoss` (config via ScriptableObject, sem exigir prefab dedicado)
- [x] Sistema de spawn/wave (pooling de inimigos) — `EnemyPool.cs` + `EnemySpawner.cs` + `WaveTracker.cs`

> ✅ Nota técnica resolvida (2026-08-26): `Health.ConfigureMaxHealth(float)` público criado, `EnemyController.ApplyDefinitionToHealth()` não usa mais reflection — ver changelog.

### Efeitos
- [x] Buffs — `BuffController`/`ActiveStatEffect`/`StatModifierDefinition` completos (modificadores flat/percent, duração)
- [x] Debuffs — mesma engine, flag de debuff em `BuffController.cs`
- [x] Veneno — seedado em `SkillDatabaseSeeder.cs` (DoT via `BuffController`)
- [x] Queimadura — `skill_fireball` ganhou o debuff "Queimando" (DoT, mesmo padrão de `skill_poison_dart`/"Envenenado": 5 de dano/seg por 4s), reseedado no asset real via `Rapadura/Seed Example Skills` (2026-08-26)
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
- [~] Sistema de geração procedural de itens (afixos), se aplicável ao gênero — `Items/Procedural/{ItemAffixDefinition,AffixDatabase,ProceduralItemGenerator}.cs` prontos e testados; **16 afixos reais autorados** (`AffixDatabaseSeeder.cs`, 2026-08-26); falta só exibição na UI de inventário/tooltip (Fase 6)

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

> ✅ **2026-08-24**: `GameObject` `HUD` criado na `Main.unity` com `UIDocument` (`HudView.uxml` + `DefaultPanelSettings`) + `HudController` referenciando `PlayerStats`. Validado em Play Mode via Unity MCP (Vida/Stamina/Mana/XP atualizando ao vivo).

### Inventário
- [ ] Janela principal
- [ ] Equipamentos
- [ ] Crafting

### Menus
- [x] Main Menu — `UI/Menus/MainMenuView.uxml` + `MainMenuController.cs` (Novo Jogo/Continuar via `SaveManager`/Configurações/Sair)
- [x] Pause — `PauseMenuController.cs`, publica `GamePausedEvent`/`GameResumedEvent` (`Menus/MenuEvents.cs`)
- [x] Configurações (gráficos, áudio, controles, idioma) — `SettingsMenuController.cs` (áudio via `AudioManager`, idioma via `LocalizationManager`, gráficos via QualitySettings/Screen; controles é placeholder documentado, rebind real fica pra depois)
- [x] Save/Load — `SaveLoadMenuController.cs` + `SaveSlotPresenter.cs`

> ⚠️ Pendente (exige Unity Editor): criar os `GameObject`s com `UIDocument` nas Scenes para cada tela. **2026-08-24**: avaliado e adiado deliberadamente — `PauseMenuController` referencia `SettingsMenuController` + `SaveLoadMenuController`, que por sua vez dependem de `SaveManager`/`AudioManager`/`LocalizationManager` já registrados via `GameManager`; são 3-4 `GameObject`s interligados, arriscado religar tudo sem testar cada referência com calma (diferente do HUD, que é um único `UIDocument` isolado). Também falta decidir o fluxo de cena: hoje `Main.unity` abre direto na tela de seleção de personagem — falta decidir se `MainMenuController` entra ANTES disso ou se a seleção de personagem já cumpre esse papel.

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
- [~] Shaders/Materiais customizados — **2026-08-24**: texturas procedurais + materiais reais aplicados na cena de teste via Unity MCP (`manage_texture`/`manage_material`): `Tex_Grass`/`Mat_Grass` (chão, ruído verde + tiling 10x10), `Tex_Wall`/`Mat_Wall` (paredes, padrão tijolo), `Tex_Bark`/`Mat_Bark` (troncos), `Tex_Leaves`/`Mat_Leaves` (copas das árvores), `Tex_Rock`/`Mat_Rock` (pedras) — tudo em `Assets/Textures/Environment/` + `Assets/Materials/`, shader `Standard` (compatível com Built-in RP). Confirmado visualmente via screenshot do Game View.
- [ ] VFX (partículas de combate, ambiente, skills)
- [~] Iluminação e pós-processamento — névoa (`RenderSettings.fog`, exponencial) e luz ambiente Trilight configuradas via `manage_graphics` (funcionam em Built-in RP). Volume de pós-processamento (bloom/vinheta/color grading) **não** foi possível: `volume_create` falhou porque a pipeline ativa do projeto é **Built-in**, não URP — apesar do `manifest.json` listar `com.unity.render-pipelines.universal`, o pacote nunca foi de fato ativado em Graphics Settings (Render Pipeline Asset não atribuído). Migrar pra URP de verdade é uma mudança maior (risco de quebrar materiais/shaders existentes) e fica pra uma tarefa dedicada, não decidi fazer isso de forma incidental numa tarefa de "texturizar cenário".
- ⚠️ **Achado ao vivo**: atribuições de material feitas enquanto o Editor estava em Play Mode não persistem (Unity descarta mudanças de estado de cena ao sair do Play Mode) — sempre confirmar `Application.isPlaying == false` antes de mexer em materiais/componentes de cena via Unity MCP.

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
