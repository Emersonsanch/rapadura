# Rapadura — Game Design Document (enxuto)

> Documento vivo — atualizar conforme decisões de design forem tomadas. Não é uma bíblia completa, é a fonte única de verdade para as decisões que já foram batidas.

## Pilares do Jogo
1. **Ação com identidade** — 5 personagens jogáveis com classes, funções e habilidades distintas (tanque, suporte, DPS à distância, controle tecnológico, dano mágico/CC).
2. **Progressão significativa** — inventário, itens com raridade, atributos e skill tree conectados entre si.
3. **Mundo vivo** — biomas, NPCs, quests e ciclo dia/noite dão motivo para explorar, não só para lutar.

## Gênero
RPG de ação em terceira pessoa, com combate em tempo real, progressão de personagem, crafting/construção e exploração.

## Plataforma-alvo
- [ ] Definir prioridade: PC (Steam) primeiro, depois mobile/console — **decisão pendente**, ver TODO.md Fase 0.
- Requisitos mínimos a definir após protótipo de performance (Fase 10).

## Público-alvo e classificação
- [ ] Definir faixa etária e classificação indicativa (ESRB/PEGI) — pendente. Influencia intensidade de violência/sangue nos efeitos de combate.

## Roster de Personagens
Ver `TODO.md` → seção "👥 Personagens Jogáveis (Roster)" para lore completo, classes, funções e habilidades de Joaquim, Maria, Maithe, Ícaro e Lavine.
Implementação em código: `Assets/Scripts/Gameplay/Characters/`.

### Árvore genealógica
Os 5 personagens jogáveis são todos primos entre si — uma família só, não um grupo de estranhos que se conheceu por acaso. A estrutura:
- **Maithe** e **Ícaro** são irmãos.
- **Joaquim** e **Maria** são irmãos.
- **Lavine** é prima dos outros quatro (filha do irmão/irmã dos pais dos outros dois pares).

Ordem de idade, do mais velho para o mais novo:
1. **Maithe** (mais velha)
2. **Maria** (~3 meses mais nova que Maithe)
3. **Joaquim** (nasceu logo depois de Maria)
4. **Ícaro** (~3 anos mais novo que Joaquim)
5. **Lavine** (mais nova, ~3 anos mais nova que Ícaro)

Essa relação de parentesco é canônica e deve informar diálogos, quests e lore futuros — os personagens se conhecem a vida inteira como família, não são aliados recém-formados.

### Habilidades de assinatura
Cada herói tem 5 habilidades próprias (4 ativas + 1 Ultimate) como `SkillDefinition` em `Assets/Resources/Skills/Characters/<CharacterId>/`, na mesma ordem de `PlayableCharacter.AbilityNames`, mais uma árvore própria em `Assets/Resources/SkillTrees/tree_<id>.asset`. Ao confirmar o herói na seleção, `CharacterSkillSet.GrantTo` aprende e equipa as 4 ativas; a Ultimate exige nível 10 e a primeira habilidade, e é comprada na árvore. Todas usam só os mecanismos já existentes (dano/cura/área, buffs no próprio herói, debuffs/DoT nos alvos).
- **Maithe** (Vento/Físico, crítico e alcance): Flecha Precisa · Chuva de Flechas · Disparo Explosivo · Passo do Vento · *Tempestade de Mil Flechas*. A prima mais velha, protetora de todos; o Disparo Explosivo usa uma bombinha inventada pelo irmão Ícaro.
- **Maria** (Sagrado, cura e proteção): Cura Divina · Benção da Vida · Escudo Sagrado · Purificação · *Milagre Celestial*. Aprendeu o Escudo Sagrado de tanto ver o irmão Joaquim se jogar no perigo; a Purificação é a luz com que acalma a prima Lavine.
- **Joaquim** (Terra, defesa e provocação): Muralha de Pedra · Provocação Heroica · Escudo das Raízes · Terremoto · *Última Resistência*. O "protetor" teimoso da família, que não cai enquanto houver um primo atrás dele.
- **Ícaro** (Elétrico, torretas/drones e controle): Torreta Automática · Granada Elétrica · Drone de Ataque · Campo Magnético · *Exército Mecânico*. O drone é pintado com as cores do arco de Maithe, para impressionar a irmã mais velha.
- **Lavine** (Trevas/Arcano, dano mágico e controle de multidão): Orbe Sombria · Correntes das Trevas · Névoa da Ilusão · Explosão Arcana · *Eclipse Eterno*. A caçula que quer ser levada a sério; o Eclipse Eterno revive a Noite Rubra em que nasceu, agora usado para proteger.

## Loop de Gameplay (macro)
Explorar → combater/coletar recursos → craftar/equipar → evoluir personagem (skills/atributos) → avançar no mundo/enfrentar bosses → repetir com poder crescente.

## Mapa mundi e encontros
Estrutura estilo Final Fantasy clássico: um **mapa mundi** visto de cima, onde o jogador move um token entre as regiões dos 4 biomas (Floresta → Deserto → Montanha → Cavernas), com **encontros aleatórios** enquanto anda. O combate em si continua **em tempo real** (Hitbox/Health/SkillManager), sem turnos/ATB.

- **Decisão técnica (2026-10-21): o mapa mundi é uma Scene separada (`Assets/Scenes/WorldMap.unity`)**, não um modo dentro da `Main.unity`. Motivo: câmera, iluminação, input e UI do mapa ficam isolados, sem ligar/desligar metade da cena de combate. A `Main.unity` passa a ser a área de combate.
- **Fluxo:** `WorldMapController` (cena do mapa) → `RandomEncounterSystem` sorteia o encontro (chance por passo/tempo, com período de graça; inimigos via `BiomeDefinition.RollEnemy`) → `SceneTransitionService.BeginEncounter` guarda o `EncounterPayload` (bioma + ids dos inimigos) e carrega a `Main.unity` → `EncounterArenaController` spawna os inimigos e, ao fim (vitória / fuga / derrota), chama `CompleteEncounter`, que registra a vitória, faz auto-save e volta para o mapa.
- **Progresso persistido** (`WorldMapProgress`, chave de save `"WorldMap"`): região atual, regiões desbloqueadas e vitórias por região. Uma região abre depois de N vitórias na anterior (padrão 3, configurável por nó do mapa) **e** do chefe dessa região anterior derrotado (ver abaixo); também guarda as regiões limpas.
- **Chefe da região (2026-10-21):** cada bioma tem um chefe na tabela de inimigos (Lobo Alfa na Floresta, Guardião da Ruína no Deserto, Senhor da Guerra na Montanha, Habitante das Cavernas). Ele **não sai mais em encontros aleatórios** (`RegionBossProgress.CanRollInNormalEncounter` é o `EnemyFilter` do `RandomEncounterSystem`, junto com o filtro de chefes já derrotados do `DefeatedEnemyRegistry`).
  - Depois das N vitórias na região (o mesmo N da regra de desbloqueio do nó seguinte; padrão 3; Cavernas, que não abre nada, usa 3) o HUD do mapa mostra **"Enfrentar o chefe"** enquanto o token está parado na região. O botão chama `WorldMapController.TryChallengeBoss`, que monta um `EncounterPayload` com o chefe + 0–2 lacaios comuns do bioma e `isBossChallenge = true`.
  - Na arena, um **banner curto** ("Chefe da região" + nome localizado do chefe via `ContentText`) aparece quando o encontro começa (`BossEncounterIntroEvent` → `BossIntroBannerController`, sem fade com "Reduzir Flashes"); a música já troca para o contexto Boss (`MusicDirector` vê `IsBoss` no payload).
  - **Vencer o desafio limpa a região** (`WorldMapProgress.MarkRegionCleared`, salvo em `clearedRegions`) e **só então** a região seguinte abre: o desbloqueio exige as N vitórias **e** o chefe derrotado. Fugir ou perder não conta; uma vitória comum também não.
  - Compatibilidade com saves antigos: regiões já desbloqueadas continuam desbloqueadas (desbloqueio nunca é desfeito); um chefe que já está no `DefeatedEnemyRegistry` conta como região limpa (e é copiado para `clearedRegions` ao abrir o mapa); uma região sem chefe na tabela continua exigindo só vitórias.
  - API para a UI: `WorldMapController.RegionBosses` (`RegionBossProgress`: `BossFor`, `CanChallengeBoss`, `IsRegionCleared`, `VictoriesToChallenge`), `WorldMapController.CanChallengeCurrentBoss` e os eventos `RegionBossChallengeStateChangedEvent` / `WorldMapRegionClearedEvent`.
- `SceneTransitionService` e `WorldMapProgress` são managers do `GameManager` (que sobrevive à troca de cena), por isso o payload atravessa o carregamento da cena em memória.

## Referências / Inspirações
- [ ] Preencher com jogos de referência (gênero, combate, arte) conforme forem definidos.

## Escopo fora do MVP
Multiplayer, live-ops, DLC e conteúdo de temporada são explicitamente pós-lançamento (ver Fases 12–14 do `TODO.md`) e não bloqueiam o MVP.

---
*Ver `TODO.md` na raiz do projeto para o roadmap de desenvolvimento fase a fase.*
