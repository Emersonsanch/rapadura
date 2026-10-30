# Rapadura — Áudio e vibração (cues, música, placeholders)

> Documento vivo, 2026-10-21 (cues novos em 2026-10-29). Para o compositor / designer de som: a lista de **cue ids** abaixo é
> tudo o que precisa ganhar som de verdade. Hoje todos tocam um **placeholder procedural**
> (sintetizado em código), então o jogo já tem som — trocar por arquivo real não exige código.
> Guardado por `Assets/Tests/EditMode/AudioEventWiringTests.cs` e `AudioNewCuesTests.cs`.

---

## 1. Como funciona

```
EventBus (combate, skills, missões, loja, pausa, encontro, clima...)
   │
   ▼
SfxEventRouter ──► SfxEventMap (Resources/Audio/SfxEventMap.asset)
   │                 gatilho + variante → cue id → clip / volume / cooldown / vozes
   ▼
AudioManager (pool de AudioSources, volumes Geral/Música/Efeitos/Interface)
```

| Peça | Arquivo | Papel |
|---|---|---|
| `SfxEventMap` | `Scripts/Audio/SfxEventMap.cs` (+ `.Defaults.cs`), asset em `Resources/Audio/SfxEventMap.asset` | Tabela de dados: bindings (gatilho+variante → cue), cues (clip, categoria, volume, cooldown, vozes, pausa, 3D, loop, ducking, placeholder) e música por contexto. |
| `SfxEventRouter` | `Scripts/Audio/SfxEventRouter.cs` | Assina os eventos, resolve o cue e toca via `AudioManager`. Cooldown por cue, limite de vozes por cue e global (12), respeita volume (cue com volume efetivo 0 nem toca) e `GameplayPause` (cues `pausable` são descartados e o loop de clima pausa). Loop de ambiente (chuva/vento) com fade de 2 s, só no mapa/encontro. |
| `MusicDirector` | `Scripts/Audio/MusicDirector.cs` | Escolhe a música por contexto e faz crossfade (`AudioManager.PlayMusic`, 1,5 s). |
| `ProceduralSfxSynth` | `Scripts/Audio/ProceduralSfxSynth.cs` | Gera os placeholders com `AudioClip.Create` (22,05 kHz mono, uma vez por cue, cacheado). |
| `UiClickSoundBinder` | `Scripts/Audio/UiClickSoundBinder.cs` | Pendura um `ClickEvent` em cada `UIDocument` da cena: todo Button/Toggle/Dropdown toca `ui.click`, sem mexer em cada tela. |
| `UiSoundHooks` | `Scripts/UI/Common/UiSoundHooks.cs` | `PlayClick/PlayHover/PlayError` agora passam pelo router (com cooldown e placeholder). |
| `HapticsService` | `Scripts/Audio/HapticsService.cs` | Vibração (ver seção 4). |

Registrados em `GameManager.BuildManagers()` logo após o `AudioManager`.

**Regra de resolução de clip:** `clip` atribuído no cue → senão placeholder procedural (se
`_proceduralSfx` ligado no asset e `preset != None`) → senão silêncio.

**Variantes** (minúsculas): `SkillCast` tenta `ultimate` primeiro (se a skill for Ultimate), depois o
elemento (`fire`, `ice`...), depois a categoria visual (`heal`, `buff`, `projectile`...), depois o
genérico. `EncounterStart` recebe o bioma; `TimeOfDayPhase` a fase; `WeatherLoop` o clima. Binding
com cue vazio = silêncio proposital (ex.: `clear`, `fog`).

---

## 2. Cues para substituir (lista para o compositor)

Formatos sugeridos: **OGG Vorbis** (mobile), mono para SFX, estéreo para música/ambiente;
one-shots com pico em -3 dBFS e sem silêncio no começo. Coloque em `Assets/Audio/Sfx/...` e arraste
no campo `clip` do cue correspondente em `Resources/Audio/SfxEventMap.asset`.

### Combate

| Cue id | Quando toca | Sugestão de som |
|---|---|---|
| `combat.hit` | Inimigo leva dano normal (3D) | Impacto seco e curto (soco/porrete na madeira), 80–150 ms. Gravar 3–4 variações. |
| `combat.crit` | Dano crítico (3D) | Impacto mais pesado + "crack" brilhante, 150–250 ms. |
| `combat.player_hurt` | Herói leva dano | Baque surdo + respiração/gemido curto (por herói, se possível). |
| `combat.enemy_death` | Inimigo morre (3D) | Queda/dissipação curta, 300–500 ms. |
| `combat.boss_death` | Chefe derrotado (abaixa a música) | Explosão/desmoronamento grave com cauda longa, 1–2 s. |
| `player.death` | Herói cai | Acorde descendente melancólico (sanfona/viola), 1–1,5 s. |
| `player.level_up` | Subiu de nível | Fanfarra curta e alegre (triângulo + sanfona), ~1 s. |

### Skills (por elemento / categoria)

| Cue id | Sugestão de som |
|---|---|
| `skill.cast` | Genérico: "whoosh" médio. |
| `skill.cast.physical` | Golpe de ar pesado, couro/tecido. |
| `skill.cast.fire` | Chama acendendo, crepitar. |
| `skill.cast.ice` | Cristais tilintando, estalo de gelo. |
| `skill.cast.lightning` | Estalo elétrico, zap descendente. |
| `skill.cast.poison` | Borbulhar viscoso. |
| `skill.cast.earth` | Pedra rachando, baque grave. |
| `skill.cast.wind` | Rajada assobiando. |
| `skill.cast.holy` | Sino/coral curto e brilhante. |
| `skill.cast.dark` | Sussurro grave, acorde menor descendente. |
| `skill.cast.arcane` | Brilhos mágicos, glissando agudo. |
| `skill.cast.heal` | Sino suave + "respiro" ascendente. |
| `skill.cast.buff` | Pad que cresce (swell) de ~0,8 s. |
| `skill.cast.ultimate` | Golpe épico: grave + brilho, abaixa a música, 1–1,5 s. |

### Progressão, loja e interface

| Cue id | Quando toca | Sugestão de som |
|---|---|---|
| `item.pickup` | Itens entram na mochila (loot) | "Plim" curto de moedinha/couro. |
| `shop.purchase` | Compra na loja | Moedas caindo no balcão. |
| `shop.sell` | Venda | Moeda única, mais grave. |
| `craft.complete` | Receita criada | Martelada + brilho. |
| `quest.started` | Missão aceita | Duas notas ascendentes (pífano/rabeca). |
| `quest.completed` | Missão concluída (abaixa a música) | Fanfarra curta de vitória. |
| `achievement.unlocked` | Conquista (abaixa a música) | Brilho + acorde maior. |
| `ui.open` | Uma tela abre (pausa) / loja abre | "Whoosh" de papel/couro abrindo, 100 ms. |
| `ui.close` | Tela fecha | Mesmo som invertido/mais grave. |
| `ui.click` | Qualquer botão/toggle/dropdown | Clique de madeira seco, 30–50 ms. |
| `ui.hover` | Foco/hover (opcional; chamado por `UiSoundHooks.PlayHover`) | Tique bem baixo. |
| `ui.error` | Ação inválida (`UiSoundHooks.PlayError`) | Buzz curto e grave, sem ser irritante. |

### Encontros e ambiente

| Cue id | Quando toca | Sugestão de som |
|---|---|---|
| `encounter.start` | Encontro começa | Gongo/zabumba + pratos, 1 s. |
| `encounter.victory` | Vitória | Fanfarra de forró curta (sanfona + triângulo), 1–2 s. |
| `encounter.defeat` | Derrota | Acorde menor descendente. |
| `encounter.fled` | Fuga | "Whoosh" + passos correndo. |
| `ambience.dawn` | Amanhecer | Galo/passarinhos (canção curta). |
| `ambience.day` | Dia | Pad quente curtinho (ou cigarras). |
| `ambience.dusk` | Entardecer | Sino de igreja distante. |
| `ambience.night` | Noite | Grilos/sapos. |
| `ambience.rain` | **Loop** com chuva | Chuva no telhado/folhas, loop sem emenda de 10–30 s. |
| `ambience.wind` | **Loop** com tempestade de areia | Vento forte com areia, loop 10–30 s. |
| `ambience.wind_soft` | **Loop** com neve | Vento frio suave, loop 10–30 s. |

### Novos cues (2026-10-29): atordoamento, loot, história, vila, campanha

Todos com placeholder procedural, **mixados abaixo dos existentes** (volume ≤ 0,75) e com cooldown;
os que se repetem muito têm variação de pitch/volume (`pitchJitter` / `volumeJitter` do cue, sorteio
semeado). Para trocar por arquivo real basta arrastar o `clip` no cue — o jitter continua valendo
(zere-o se o arquivo já tiver variações).

| Cue id | Evento (quem dispara) | Placeholder hoje → sugestão de som real | Barramento | Vol. | Cooldown / vozes | Jitter (pitch/vol) |
|---|---|---|---|---|---|---|
| `combat.stun` | `StunAppliedEvent` (3D no alvo) | `Dizzy`: tom que cai "ondulando" → "estrelinhas"/passarinhos girando, 300–400 ms. | Efeitos | 0,40 | 0,25 s / 2 | 6% / 10% |
| `loot.drop` | `EnemyLootDroppedEvent` (substitui o `item.pickup` daquela morte) | `Clink`: dois "plins" de moeda na madeira → moedas/couro caindo na sacola, ~200 ms. | Efeitos | 0,35 | **0,3 s / 1** (anti-spam: AoE com várias mortes no mesmo frame = um som só) | 8% / 15% |
| `loot.bag_full` | `LootOverflowEvent` ("Mochila cheia") | `SackThump`: baque abafado de saco cheio + pano → saco de estopa batendo cheio, 300 ms. Não é o `ui.error`, para não soar como erro. | Interface | 0,45 | 1,5 s / 1 | 3% / — |
| `story.open` | `StoryPanelChangedEvent` Opened (1º painel) | `PageRustle` (2,4 kHz): farfalhar de papel → caderno/cordel abrindo, 300 ms. | Interface | 0,40 | 0,3 s / 1 | 5% / — |
| `story.page` | `StoryPanelChangedEvent` Advanced | `PageRustle` (3 kHz): virada de página, 250–350 ms. Gravar 3–4 viradas. | Interface | 0,30 | 0,15 s / 2 | 10% / 20% |
| `story.skip` | `StoryPanelChangedEvent` Skipped | `Whoosh` curto → folhas passando rápido (folhear). | Interface | 0,30 | 0,3 s / 1 | 5% / — |
| `story.close` | `StoryPanelChangedEvent` Finished (fim lido, não é o final) | `PageRustle` (1,8 kHz): página fechando, mais grave. | Interface | 0,32 | 0,3 s / 1 | 5% / — |
| `story.credits` | Final (`Ending`) lido ou pulado → créditos | `Chime` (Sol): sino suave → acorde de viola caipira se abrindo, ~1 s. | Interface | 0,45 | 2 s / 1 | — |
| `campaign.completed` | `CampaignCompletedEvent`, **1,6 s depois** (deixa a fanfarra de vitória do mesmo frame tocar) | `ForroFlourish`: 2 compassos de baião (zabumba nos tempos 1 / "e" do 2, triângulo abafado/aberto) + arpejo maior → vinheta de forró pé-de-serra (zabumba + triângulo + sanfona), 2 s. Abaixa a música. | Interface | 0,75 | 5 s / 1 | — |
| `village.open` | Vila (`VillageHubButton`) pega a pausa | `WoodCreak`: rangido de madeira + "toc" → porta/baú de madeira abrindo, 400–500 ms. Substitui o `ui.open`. | Interface | 0,50 | 0,4 s / 1 | 5% / 10% |
| `village.close` | Vila solta a pausa | `WoodKnock`: dois "tocs" ocos → tampa do baú/tramela fechando, 300 ms. Substitui o `ui.close`. | Interface | 0,45 | 0,4 s / 1 | 5% / — |
| `npc.talk` | `DialogueStartedEvent` (NPC da região, da vila e do Main.unity) | `Murmur`: "hm-hm" murmurado → interjeição curta por personagem ("Ô!", "Ei!", "Hum…"), 300 ms. A caixa de diálogo não toca `ui.open`. | Interface | 0,40 | 0,5 s / 1 | 8% / 10% |
| `combat.thorns` | Dano refletido por Espinhos (`ThornsRules.IsReflecting`, canal próprio) — substitui o `combat.hit` desse golpe | `Prick`: picada metálica + raspado → espinho de mandacaru/arame farpado, 150 ms. | Efeitos | 0,35 | 0,12 s / 2 | 10% / 15% |
| `enemy.windup` | `EnemyAttackWindupEvent` (agachada de antecipação, 3D) — sutil | `Inhale`: respiro/couro "juntando força", 250 ms. | Efeitos | 0,20 | 0,3 s / 2 | 12% / 20% |
| `hero.land` | `PlayerLandedEvent` (squash de aterrissagem ≥ 20%) — sutil | `Thud` grave → pé na terra batida/poeira, 100–150 ms. | Efeitos | 0,22 | 0,25 s / 1 | 8% / 20% |
| `ambience.title` | **Loop** na tela de título e na seleção de herói (`MainMenu` / `CharacterSelect`), fade de 2,5 s | `CountrysideLoop`: grilos em grupos de três sobre brisa leve → entardecer no sertão (grilos, vento, um bem-te-vi distante), loop estéreo de 20–40 s. Não pausa. | Efeitos | 0,16 | — | — |

**Quem tocou a pausa:** `SfxEventRouter.ClassifyPauseOwners` olha os donos do `GameplayPause` no
`GamePausedEvent`: Vila → `village.open`/`village.close`; painel de história → silêncio (os cues
`story.*` falam por ele); caixa de diálogo → abre em silêncio (`npc.talk` já tocou) e fecha com
`ui.close`; o resto continua com `ui.open`/`ui.close`.

**Loot × pickup:** o router assina `CombatTargetDiedEvent` no boot (antes do `EnemyLootGranter`), então
os itens que entram na mochila no mesmo instante de uma morte ficam pendentes; o
`EnemyLootDroppedEvent` que vem logo depois toca `loot.drop` no lugar do `item.pickup`. Se ele não vier,
o `Tick` seguinte toca o `item.pickup` normal.

**Eventos novos (mínimos, só para áudio):** `StoryPanelChangedEvent` (publicado por
`StoryPanelSequence`), `EnemyAttackWindupEvent` (`EnemyProceduralAnimator`, no início da antecipação) e
`PlayerLandedEvent` (`PlayerProceduralAnimator`, na aterrissagem com squash). Nenhum sistema de jogo
depende deles.

---

## 3. Música

Contextos (`MusicContext`) e quando entram:

| Contexto | Variante | Quando | Sugestão |
|---|---|---|---|
| `Title` | — | Menu principal e seleção de herói | Tema principal calmo (viola caipira/sanfona), 72 bpm. |
| `WorldMap` | — | Mapa-múndi | Tema de viagem, maior, 90 bpm. |
| `Encounter` | `forest`, `desert`, `mountain`, `caves` (e genérico) | Encontro por bioma | Combate leve com cor do bioma (zabumba na floresta, pífano no sertão, ecos nas cavernas), ~90–112 bpm. |
| `Boss` | (bioma opcional) | Encontro com algum inimigo `IsBoss` | Tema de chefe, menor, 140 bpm. |
| `Story` | — / `ending` | **Sobreposição**: enquanto um painel de história (ou os créditos depois do final) está na tela; `ending` no epílogo | Tema íntimo e lento (viola solo/rabeca), maior, 60 bpm; `ending` mais caloroso, 66 bpm (Lá). |

O contexto `Story` é uma camada por cima do fluxo: o `MusicDirector` consulta
`StoryPanelController.IsShowing` a cada frame (`Tick`), faz crossfade para `Story` e, quando o painel
some, volta para o contexto que o fluxo pediu nesse meio-tempo (normalmente o `WorldMap`). Nada de
"ducking" fixo: a troca é o mesmo crossfade de 1,5 s dos outros contextos.

Loops de 1–3 min, OGG estéreo, ponto de loop sem clique. Arraste no `clip` da entrada em `_music`.
Vitória/derrota não trocam a música: tocam os stings `encounter.*` e o fluxo volta ao `WorldMap`.

**Placeholder procedural (decisão):** sem clip, o `MusicDirector` gera **uma vez por contexto** um
loop de 16 tempos (~8–13 s): drone (tônica uma oitava abaixo + quinta, "respirando") sob um arpejo
em colcheias numa progressão I–IV–V–I (maior) ou i–VI–iv–v (menor). Custo: poucos ms ao trocar de
contexto e ~0,5 MB por loop em memória; depois é reprodução de clip comum (zero CPU por frame).
Para silêncio, desligue `_proceduralMusic` no asset — os ganchos continuam iguais.

O campo `_ambientMusicCueId` de `BiomeDefinition` não é usado pelo `MusicDirector` (a música por
bioma vem do asset acima); pode ser removido ou ligado depois.

---

## 4. Vibração (`HapticsService`)

| Padrão | Quando | Duração |
|---|---|---|
| `Light` | Herói acerta um crítico | 20 ms (no máx. 1 a cada 0,15 s) |
| `Medium` | Herói leva dano | 45 ms (1 a cada 0,3 s) |
| `Heavy` | Herói morre | 160 ms |
| `Success` | Subiu de nível / vitória no encontro | 3 pulsos (35-70-35-70-90 ms) |

- Android: `Vibrator.vibrate(long[], -1)` via JNI (pulsos curtos); se falhar, `Handheld.Vibrate()`.
- iOS: só `Handheld.Vibrate()` (um zumbido longo), então só padrões com ≥ 80 ms ligados (morte,
  level up) vibram. Haptics finos do iOS precisariam de plugin nativo (`UIImpactFeedbackGenerator`).
- Editor/PC: não vibra; a linha "Vibração" mostra "(só no celular/tablet)".
- Liga/desliga: Configurações → Acessibilidade → **Vibração** (`HapticsSettingsSection`, depois das
  linhas do tutorial). Salvo por aparelho em `PlayerPrefs["Haptics.Enabled"]` (padrão: ligado).
  Textos em `UI/Menus/HapticsTexts.cs`.

---

## 5. Pendências conhecidas

- Não há `AudioMixer` real ainda (ver nota em `AudioManager`): volumes são multiplicados direto no
  `AudioSource.volume`. Os cues já carregam a categoria, então a migração não muda os dados.
- Pitch/volume aleatório por cue existe (`pitchJitter`/`volumeJitter`, aplicado pelo router e passado ao
  `AudioManager.PlaySfx(..., pitch)`), mas só os cues novos usam; os de combate antigos ainda tocam fixos —
  grave 3–4 variações ou ligue o jitter neles.
- Não existe configuração de acessibilidade específica de áudio (legendas de som, mono): os cues novos
  seguem só os sliders Geral/Música/Efeitos/Interface e a pausa.
- `item.pickup` é detectado pelo aumento da contagem de itens na mochila do jogador
  (`InventoryChangedEvent` não diz o motivo); compras/criações nos 0,3 s anteriores não tocam o pickup.
