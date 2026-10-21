# Rapadura — Áudio e vibração (cues, música, placeholders)

> Documento vivo, 2026-10-21. Para o compositor / designer de som: a lista de **cue ids** abaixo é
> tudo o que precisa ganhar som de verdade. Hoje todos tocam um **placeholder procedural**
> (sintetizado em código), então o jogo já tem som — trocar por arquivo real não exige código.
> Guardado por `Assets/Tests/EditMode/AudioEventWiringTests.cs`.

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

---

## 3. Música

Contextos (`MusicContext`) e quando entram:

| Contexto | Variante | Quando | Sugestão |
|---|---|---|---|
| `Title` | — | Menu principal e seleção de herói | Tema principal calmo (viola caipira/sanfona), 72 bpm. |
| `WorldMap` | — | Mapa-múndi | Tema de viagem, maior, 90 bpm. |
| `Encounter` | `forest`, `desert`, `mountain`, `caves` (e genérico) | Encontro por bioma | Combate leve com cor do bioma (zabumba na floresta, pífano no sertão, ecos nas cavernas), ~90–112 bpm. |
| `Boss` | (bioma opcional) | Encontro com algum inimigo `IsBoss` | Tema de chefe, menor, 140 bpm. |

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
- Pitch aleatório por cue (variação de golpes) não existe: grave 3–4 variações e, se quiser,
  adicione um campo `pitchJitter` ao cue e aplique no `AudioManager`.
- `item.pickup` é detectado pelo aumento da contagem de itens na mochila do jogador
  (`InventoryChangedEvent` não diz o motivo); compras/criações nos 0,3 s anteriores não tocam o pickup.
