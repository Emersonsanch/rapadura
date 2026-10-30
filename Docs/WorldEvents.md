# Rapadura — Chefes do mundo e Invasão do Cangaço

> Pedido (2026-10-30): chefes do mundo, um por região, com horário fixo (e forma da noite), anunciados no mapa com
> contagem regressiva; invasões com horário ("Invasão do Cangaço") que tomam uma região por um tempo e pagam
> "Baú do Cangaço +1…+5" pela participação; falas dos primos. Inspiração: MU Online (chefes de mapa com horário,
> Golden Invasion, Box of Kundun +1…+5) e Tibia (raids anunciadas). Nomes, textos e arte são nossos.
> Código: `Assets/Scripts/Gameplay/WorldEvents/` e `Assets/Scripts/UI/WorldEvents/`. Testes: `WorldEventsTests`.
> **Números são estimativa (fora do modelo r8/r9, como as salas de evento §22 do Balance).**

## 1. História (Luz × Trevas)

Os chefes do mundo são **arcontes do Príncipe das Trevas (Ahriman)**: cada um tomou o corpo de um bicho da região e o fez
gigante, com uma coroa de mandacaru que brilha. Vencer o arconte devolve a luz que ele roubou e o bicho volta a ser
bicho. O bando da Invasão é comandado por **Az**, a arconte da cobiça: ela "enfeitiça" os bichos com chapéus de couro
de cangaceiro encantados; derrubar o chapéu liberta o bicho. Os nomes novos seguem a regra do usuário (mitologia
maniqueísta, sem "Seu/Dona"); os avós são Vó Edna e Vô Jorge.

| Região | Dia (modelo) | Noite (modelo) | Nível mín. | Montaria rara (dia / noite) |
|---|---|---|---|---|
| Floresta | **Saklas, o Lobo-Guará Encantado** (lobo) | **Saklas, o Lobisomem-Rei da Mata** (lobisomem) | 3 | Onça de Luz 2% / 4% |
| Deserto | **Namrael, o Escorpião-Rei** (escorpião) | **Namrael da Lua Cheia** (escorpião recolorido) | 6 | Ema de Fogo 1,5% / 3% |
| Montanha | **Mazan, o Gigante da Serra** (golem) | **Mazan, o Gigante da Pedra da Lua** (golem recolorido) | 9 | Tigre-das-Neves 2% / 4% |
| Cavernas | **Príncipe das Trevas, Rei da Revoada** (enxame) | **Príncipe das Trevas, o Morcegão-Vampiro** (morcego-vampiro) | 13 | Kirin 0,8% / 1,6% |

## 2. Calendário (relógio real, igual para todo mundo)

`WorldEventCalendar`: tudo é função do instante UTC e do fuso **fixo do servidor (Brasília, UTC−3)**, nunca do fuso do
aparelho. O relógio é `WorldEventCalendar.UtcNow` (os testes injetam; online é a hora do servidor).

- **Chefes:** cada um a cada **3 h**, aberto **30 min**, defasados 45 min: Floresta 0h/3h/6h…, Deserto 0h45/3h45…,
  Montanha 1h30…, Cavernas 2h15… → aparece um chefe em algum lugar a cada 45 min e nunca dois ao mesmo tempo.
- **Forma da noite:** se a janela **começa** entre 18h e 6h (Brasília). Decidida no início, nunca muda no meio da luta.
- **Invasão:** às **2h, 6h, 10h, 14h, 18h e 22h**, 30 min, numa região por vez em rodízio (Floresta → Deserto →
  Montanha → Cavernas); com 6 por dia o rodízio anda um passo por dia.
- **Prêmio uma vez por janela**: chefe vencido ou baú ganho fecham a janela para aquele jogador. Perdeu? Pode tentar
  de novo enquanto ela estiver aberta. A luta que já começou não é cortada se a janela fechar.
- **Sem sorteio do cliente:** todo prêmio extra sai de `RewardSeed(tipo, chefe, janela, jogador)` (FNV-1a), então a
  mesma janela e o mesmo jogador sempre ganham o mesmo, e o servidor pode refazer a conta. O saque normal de cada
  bicho (materiais, ouro do bicho) continua no sistema de sempre.

## 3. Chefe do mundo (`WorldBossRules`, `WorldEventDirector`)

- **Modelo:** reaproveita o bicho comum da região (modelo procedural, passada das pernas, IA e ataque dele) e o
  `WorldEventEnemyTag` o deixa ×1,9 (noite ×2,05), põe a **coroa de mandacaru** feita em código (`WorldEventRegalia`,
  mesma paleta/material dos inimigos, cristais que brilham à noite) presa na cabeça (balança com o andar) e, na forma
  recolorida, o tom de luar e os olhos amarelos da variante noturna (`NightVariantTag.TintModel/AddEyes`). Status pelo
  `EventRoomEnemyScaler` das salas. Não é `IsBoss` (sem registro de chefe de região, sem trava de único).
- **Status:** vida = 15 comuns da região (1.050 / 1.650 / 2.100 / 2.550 ≈ 2× o chefe da região), noite ×1,25; dano
  +40% (noite +60%); XP = 12 comuns (o chefe da região dá 8), noite ×1,3.
- **Luta (5 min):** chama 2 comuns da região a cada 22 s (o primeiro chamado aos 12 s; no máximo 3 ajudantes vivos);
  com metade da vida fica **furioso** uma vez e chama 3. Tempo acabou → "foi embora" (sem prêmio, pode voltar).
- **Prêmio da 1ª vitória da janela:** ouro 20 × (1 + 0,5 × região) (noite ×1,25), 2 Pedras da Bênção (+1 à noite),
  1 Pedra da Alma do Deserto em diante, **50% (noite 65%) de um item da casa** do grupo "Drop:" do chefe da região
  (`HouseDropRules.BossPool`, raridades de chefe, vai para o Depósito) e a **montaria rara** da tabela acima
  (`MountCollection.Grant`, fonte `event`; só espécies jogáveis).

## 4. Invasão do Cangaço (`InvasionRules`)

- **Botão "Defender do Cangaço"** só na região invadida, nível 3+. 4 min de luta.
- **Ondas:** 3, 4, 5 e 6 bichos da região com chapéu de couro (vida/XP ×1,2, +10% de dano); a próxima vem com ≤ 1
  vivo ou a cada 25 s (teto 6 vivos). Depois das 4 ondas, com o campo limpo, chega **Az, capitã do bando**: o bicho
  base do chefe do mundo da região, chapéu maior, ×6 vida/XP, +35% de dano, ×1,5.
- **Pontos:** 10 por bicho; Az 80 + 40 por libertar a região + 1 ponto a cada 2 s que sobraram.
- **Baú por pontos:** +1 ≥ 30, +2 ≥ 90, +3 ≥ 160 (todas as ondas sem Az ≈ 180), +4 ≥ 260 (precisa vencer Az),
  +5 ≥ 330 (vencer Az com ~1 min sobrando). Morrer paga o que os pontos já valem.

| Baú | XP (comuns da região) | Ouro (Floresta; +50% por região) | Pedras | Item da casa | Cavalo Pampa |
|---|---|---|---|---|---|
| +1 | 3 | 6 | — | — | — |
| +2 | 6 | 12 | 1 Bênção | 10% | — |
| +3 | 10 | 20 | 1 Bênção | 25% | — |
| +4 | 15 | 32 | 2 Bênção | 40% | 0,5% |
| +5 | 22 | 45 | 2 Bênção + 1 Alma | 60% | 2% |

O item da casa vem do grupo comum da região (`HouseDropRules.CommonPool`). O Cavalo Pampa só se os primos ainda
não tiverem um. Hoje o baú é aberto na hora (aparece no cartão de resultado); online ele vira item negociável.

## 5. Orçamento

Nada aqui dá poder de combate direto (nenhum buff, nenhum stat): só XP, ouro, pedras de refino, item da casa e
montaria, então a pilha de poder (Balance §14) não muda e nenhum número lido pelo `tools/balance` foi tocado. As
pedras somam no máximo ~3 Bênçãos por chefe e 2 + 1 Alma por invasão, limitadas pelas janelas (≤ 8 chefes da
mesma região e 6 invasões por dia, uma vez cada) — da ordem das salas de evento (§22.5). O XP de uma vitória de chefe
(12–15 comuns) fica entre o chefe da região e uma sala Ouro. Se o r10 mostrar a curva andando rápido, cortar
`InvasionRules.ChestExperienceCommons` e `WorldBossRules.ExperienceCommons` primeiro.

## 6. Mapa, HUD e falas

- **Mapa (`WorldEventMapPanel`, sem editar cena):** pílula sob a das salas: "Chefe do mundo: Saklas… está na
  Floresta! Vai embora em 12:04" ou "Próximo chefe do mundo: … no Deserto, em 0:41"; a mesma coisa para a invasão.
  Quando uma janela abre com o mapa na tela, um primo avisa (fala de 6 s). Botões "Enfrentar: …" / "Defender do
  Cangaço" na fileira de ações quando o herói está na região certa e a janela ainda não pagou.
- **Luta (`WorldEventHud`):** nome do chefe ou "Invasão do Cangaço", relógio (vermelho nos últimos 30 s), barra de
  vida do chefe/de Az, pontos e baú até agora, objetivo, faixa de avisos e o cartão de resultado com a fala do primo.
  Estilo em `Resources/UI/WorldEvents.uss` (importa o tema).
- **Falas (`WorldBossTexts`):** cada primo tem uma fala para 9 momentos (chefe apareceu, início de dia, início de
  noite, vitória, derrota; invasão apareceu, início, vitória, derrota), en + pt-BR.
- **Save `ScheduledWorldEvents`** (não `WorldEvents`, que é dos eventos da estrada): vitórias por chefe (e de noite), última janela paga, baús por nível, melhor baú, invasões
  defendidas. Save antigo carrega vazio. O calendário não é salvo (vem do relógio).

## 7. Encaixe no código

- `EncounterPayload.worldEvent/worldEventWindow/worldEventNight` + `IsDirected` (= sala de evento ou evento do mundo):
  a arena entra em modo dirigido, `SceneTransitionService` não conta vitória regional, `WorldMapController.TryStartEventRoom`
  aceita os dois.
- `WorldEventDirector` (Main.unity, auto-bootstrap como o `EventRoomDirector`), `WorldEventProgress` (GameManager:
  `ScheduledEvents`, SaveManager, NewSessionReset).

## 8. Online (FASE 12) e pendências

- Online o chefe vira **um monstro compartilhado por região** (estilo MU): vida × número de lutadores, prêmio por
  jogador com o mesmo seed (o servidor rola); a invasão vira ondas no mapa aberto da região, pontos por jogador
  (abate para quem matou, ondas/Az para todos presentes). O servidor é dono do relógio (anti-trapaça de relógio do
  aparelho, que offline ainda dá para adiantar).
- Ainda não feito: golpes próprios dos chefes (só a IA do bicho base, maior), placar/ranking por janela, conquistas,
  o baú como item guardável, efeito da invasão nos encontros comuns da região, som próprio do anúncio.
