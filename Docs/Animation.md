# Animação dos heróis: peso, inércia e personalidade

> Como o movimento dos heróis é montado em camadas, quais parâmetros o Animator recebe, como cada
> herói tem um "jeito de se mexer" próprio (perfil de movimento) e como ajustar. Código em
> `Assets/Scripts/Gameplay/Player/`; o controller é gerado por `Assets/Scripts/Editor/HeroAnimatorRebuilder.cs`.

## 1. Camadas, de baixo para cima

1. **PlayerMotor** (jogabilidade): aceleração, frenagem, curvas e frenagem em curva fechada. Os
   tempos vêm do perfil do herói (`ApplyMotionProfile`). As velocidades (andar/correr) continuam
   sendo balanceamento e não mudam com o perfil.
2. **Animator**: blend tree de locomoção com a velocidade do clipe casada à velocidade real, para o pé
   não patinar. Ícaro, Joaquim e Lavine usam mocap do CMU gravado no próprio Avatar (`Mocap_WalkKid`,
   `Mocap_Jog`, `Mocap_Run`, `Mocap_Idle`; chave **Rapadura ▸ Animation ▸ Use Mocap Locomotion**, ver
   `Docs/Mocap.md` §5). Maria (Generic) e Maithe usam os clipes Walk/Run da Meshy, que também são o
   fallback quando a chave está desligada ou um clipe mocap falta ou sai da faixa.
3. **PlayerFootIK** (só Humanoid: Ícaro, Joaquim, Lavine): pés seguindo o chão, pés presos ao parar
   ou girar no lugar (com passo de reposição), joelhos dobrando no ar e pelve com balanço, mergulho
   na aterrissagem e troca de peso sem tirar o pé do chão.
4. **PlayerProceduralAnimator** (LateUpdate, aditivo): inclinação, contra-rotação do tronco, braço
   oposto à perna, respiração, troca de peso, micro-movimento, olhadas, *overlap* com molas,
   antecipação e *follow-through* do ataque, magia, impacto de golpe, morte, *squash & stretch*.

Tudo usa tempo escalado (pausar congela), não aloca por frame e funciona no celular (2 raycasts por
frame no herói).

## 2. Animator (menu **Rapadura ▸ Animation ▸ Rebuild Hero Animators**)

### Parâmetros

| Parâmetro | Tipo | Quem escreve | Observação |
|---|---|---|---|
| `Speed` | float | `PlayerAnimatorSpeedDriver` | m/s, `SetFloat` com dampTime 0,1 s |
| `MoveBlend` | float | idem | 0 parado, 0,5 andar, 1 correr (SmoothDamp 0,1 s) |
| `MoveSpeedMul` | float | idem | velocidade do estado Locomotion (casamento de passada) |
| `WalkClipSpeed` / `RunClipSpeed` | float | rebuilder (dado) | velocidade medida de cada clipe |
| `JogClipSpeed` | float | rebuilder (dado) | só com a locomoção mocap: velocidade do `Mocap_Jog` (filho 0,75 do blend tree) |
| `TurnAngle` | float | driver | ângulo com sinal (+ = direita) entre a frente e a direção desejada, dampTime 0,12 s |
| `IsGrounded` | bool | driver | motor no chão |
| `VerticalSpeed` | float | driver | m/s (+ = sobe), dampTime 0,05 s no ar; ao tocar o chão segura a velocidade do impacto por 0,12 s |
| `FootPlantL` / `FootPlantR` | float | rebuilder (dado) | tempo normalizado (0..1) da pisada de cada pé num ciclo do estado Locomotion, medido no clipe Walk |
| `FootStrides` | float | rebuilder (dado) | passadas (par esquerdo+direito) por ciclo do clipe (1 nos mocaps) |

### Estados da camada base

```
Locomotion (blend 1D: IdlePose 0 / Walk 0,5 / [Jog 0,75, só mocap] / Run 1)
   │  !IsGrounded && VerticalSpeed > 1      (0,08 s, interrupção: Destination)
   ├──────────────► Jump ──VerticalSpeed < 0 (0,15 s, interrupção: Source)──► Fall (loop)
   │  !IsGrounded && VerticalSpeed < -4      (0,15 s, interrupção: Destination)       │
   └──────────────► Fall                                                               │
Jump/Fall: IsGrounded && VerticalSpeed < -5 ──(0,08 s)──► Land ──exit time 0,9 (0,15 s)──► Locomotion
Jump/Fall: IsGrounded && VerticalSpeed > -5 ──(0,12 s)──► Locomotion   (aterrissagem leve: sem Land)
Land: !IsGrounded && VerticalSpeed > 1 ──(0,08 s)──► Jump               (pular de novo sem esperar)
```

- Todas as transições têm *fixed duration* e são sem *exit time*, menos `Land → Locomotion`.
- Entre estados de locomoção (quando existe o estado `Idle` de mocap): 0,25 s para entrar e 0,2 s para sair.
- **Não há clipes de pulo, queda ou aterrissagem.** Os três estados usam a pose parada (`<Herói>_IdlePose`)
  como marcador, e a camada procedural e o IK fazem o agachamento, o encolher das pernas, o arco e o
  *squash*. Para usar um clipe, arraste-o para o campo *Motion* do estado. Não precisa mexer no código.
- **IK Pass** fica ligado na camada base dos controllers Humanoid (Maria é Generic e fica sem IK).
- O código não chama mais `Animator.Play` nesses estados. `PlayerAnimatorHashes.TryPlay` não faz nada
  quando o controller tem `IsGrounded`, porque são as transições que decidem.
- A camada de ações do mocap CMU continua **desligada** (`IncludeMocapActions`). A causa da deformação foi corrigida
  no importador (`Docs/Mocap.md` §1), mas as ações ainda são gravadas só no Ícaro. Reimporte o conjunto e confira
  no Console (`MocapClipSanity`) antes de usar o menu "(with mocap actions)".

### 2.1 Fase dos pés no blend tree e passos (2026-10-30)

- **Pés em fase no blend:** cada clipe de loop começa num quadro diferente (o corte do `bvh_prep.py` pega a
  janela de loop mais limpa, não um pé específico). Sem correção, o cruzamento Walk↔Jog↔Run misturava uma pose
  "pé esquerdo na frente" com uma "pé direito na frente": as pernas "tesouravam" e o pé patinava no meio da
  transição, e isso parecia andar duro/estranho. Agora o rebuild mede a pisada de cada pé em cada clipe
  (`FootstepMath.PlantPhase`: o primeiro quadro no chão do apoio que contém o ponto mais à frente do pé) e põe
  um `cycleOffset` em cada filho para as pisadas caírem junto com as do Walk (`FootstepMath.AlignmentOffset`,
  média circular dos dois pés). Clipes com número de passadas diferente não são alinhados, e o relatório avisa.
  O relatório do rebuild mostra, por herói, `feet: plants L … / R … xN, Mocap_Jog offset …, Mocap_Run offset …`.
- **Passos (som + poeira):** `PlayerFootsteps` (adicionado sozinho ao Player pelo `PlayerProceduralAnimator`)
  publica `PlayerFootstepEvent` a cada pisada, acima de 0,4 m/s e no chão. A ordem de fontes é:
  1. fases gravadas no controller (`FootPlantL/R`, comparadas com o `normalizedTime` do estado Locomotion,
     `FootstepMath.CrossedPhase`). É exato e vale para o blend inteiro por causa do alinhamento acima;
  2. sem fases gravadas (controller antigo), os extremos do afastamento entre os pés animados
     (`FootstepMath.StrideExtremum`);
  3. sem rig (Maithe), a cada π da fase de passo procedural.

  O `SfxEventRouter` toca `hero.step` (gatilho `HeroFootstep` = 42, *Thud* bem baixo, 0,09, com variação de tom e de
  volume). O `PlayerFootstepDust` solta uma nuvem pequena de poeira no pé quando a intensidade passa de 0,5
  (trote/corrida). **Não se usam AnimationEvents:** o mesmo clipe toca em outros Animators (prévia, foto,
  seleção de herói), e um evento sem receptor enche o Console de erros. Os dados no controller fazem o mesmo papel.
- **Sincronia do golpe:** já vinha dos clipes. `AttackStrikeTime`/`AttackStrikeTimeB` (rebuild com ações mocap)
  guardam o tempo real da mão mais rápida, que o `PlayerBasicAttack` espera antes do dano. Sem clipes de ação, a
  curva procedural do braço cai exatamente em `StrikeDelay` (§3).
- **Viradas, partida e parada:** não há clipes de virada, partida ou parada no conjunto (o `Mocap_TurnLeft90`
  tem root motion e não entra na locomoção). O blend tree continua 1D por velocidade. A inclinação na curva, o
  peito puxando a virada (`TurnAngle`), o freio em curva fechada, a inclinação por aceleração e o *follow-through*
  de cabeça e tronco ficam na camada procedural. O parâmetro `TurnAngle` já está no controller para um blend 2D
  futuro, se aparecerem clipes de virada.

### 2.2 Decisão: movido pelo motor, não por root motion

**Os heróis são movidos pelo `PlayerMotor` (CharacterController), e o Animator só acompanha.** `Apply Root
Motion` fica desligado (`PlayerAnimatorSpeedDriver._disableRootMotion`), e os clipes de locomoção são *in-place*.
Motivos:

1. **Multiplayer (Fase 12, estilo MU Online):** a posição é regra de jogo. Ela tem de sair de números (velocidade,
   aceleração, colisão) que o servidor consegue rodar e validar sem Animator. Com root motion, a posição dependeria
   do clipe e do *frame rate*.
2. **Resposta no celular:** clique para andar e joystick precisam partir e parar no tempo do perfil (0,1–0,25 s).
   Um clipe de partida com root motion atrasaria a resposta ou teria de ser cortado.
3. **Cinco corpos, um conjunto de clipes:** o mesmo mocap toca em heróis de alturas diferentes, e as velocidades
   de andar/correr são balanceamento (atributos, montarias, lentidão). O casamento de passada
   (`MoveSpeedMul`) ajusta a cadência à velocidade real.
4. **Patinação controlada:** o pé não patina por outros caminhos: velocidade do clipe medida e casada, fases dos
   filhos alinhadas, pés presos parado ou girando no lugar, e IK no chão.

Custo aceito: a passada não "puxa" o corpo nem acelera sozinha. O peso vem do perfil (curva de aceleração,
inclinação, molas). Root motion continua possível em ações sem efeito de regra (emotes, cutscenes), ligando
`applyRootMotion` só durante elas.

## 3. Perfil de movimento (`HeroMotionProfile`)

Os valores padrão de cada herói ficam em `HeroMotionProfile.For(CharacterId)`. Para sobrescrever:
**Create ▸ Rapadura ▸ Animation ▸ Hero Motion Profile** e arraste no campo `motionProfile` da entrada
do herói no `PlayerVisualSwapper`. Durante o Play Mode, a cópia viva aparece no `PlayerProceduralAnimator`
(seção *Motion profile*) e dá para mexer em tempo real, mas a alteração se perde ao sair do Play.

| Campo | Joaquim | Maria | Maithe | Ícaro | Lavine | Padrão |
|---|---|---|---|---|---|---|
| Personalidade | pesado, firme | calma, suave | leve, ágil | inquieto | nervosa | — |
| `weight` | 1,35 | 1,0 | 0,75 | 0,9 | 0,8 | 1,0 |
| `accelerationTime` (s) | 0,24 | 0,18 | 0,12 | 0,14 | 0,13 | 0,16 |
| `decelerationTime` (s) | 0,18 | 0,14 | 0,09 | 0,11 | 0,10 | 0,12 |
| `directionTurnSpeed` (°/s) | 620 | 760 | 1080 | 960 | 1000 | 900 |
| `turnSpeedMoving` / `Stopped` (°/s) | 520/760 | 640/900 | 840/1150 | 760/1080 | 800/1100 | 720/1000 |
| `turnBrakeAngle` (°) / velocidade mantida | 100 / 0,45 | 105 / 0,55 | 120 / 0,75 | 110 / 0,65 | 110 / 0,65 | 110 / 0,6 |
| inclinação máx. por aceleração (°) | 9 | 9 | 13 | 11 | 12 | 12 |
| `bobAmplitude` (m) | 0,035 | 0,022 | 0,03 | 0,028 | 0,03 | 0,025 |
| `strideScale` (passada; < 1 = passos mais curtos e rápidos, com deslize de 1 − valor) | 1 | 1 | 1 | 1 | 1 | 1 |
| respiração (Hz / °) | 0,22 / 2,0 | 0,24 / 1,4 | 0,30 / 1,4 | 0,30 / 1,6 | 0,36 / 1,8 | 0,28 / 1,6 |
| troca de peso (° / m / período s) | 1,6 / 0,02 / 6,5 | 1,0 / 0,012 / 7,5 | 1,0 / 0,012 / 4,5 | 1,4 / 0,016 / 4,0 | 1,8 / 0,018 / 3,2 | 1,2 / 0,015 / 5 |
| micro-movimento (° / ritmo) | 0,8 / 0,4 | 0,6 / 0,35 | 1,0 / 0,6 | 1,8 / 0,9 | 2,2 / 1,1 | 1,0 / 0,6 |
| olhada (intervalo s / ° / duração s) | 7 / 25 / 1,2 | 6 / 30 / 1,3 | 4 / 40 / 0,8 | 3,5 / 45 / 0,6 | 2,8 / 50 / 0,5 | 5 / 35 / 0,9 |
| ataque: recuperação (s) / braço (°) / torção (°) / avanço (m) | 0,34 / 105 / 26 / 0,25 | 0,30 / 95 / 18 / 0,20 | 0,22 / 110 / 20 / 0,32 | 0,26 / 115 / 22 / 0,28 | 0,24 / 120 / 24 / 0,26 | 0,28 / 110 / 22 / 0,3 |
| squash na decolagem / aterrissagem | 0,05 / 0,06 | 0,035 / 0,045 | 0,04 / 0,05 | 0,04 / 0,055 | 0,045 / 0,055 | 0,04 / 0,06 |
| mergulho da pelve ao aterrissar (m) / encolher no ar (m) | 0,14 / 0,10 | 0,09 / 0,12 | 0,07 / 0,16 | 0,09 / 0,14 | 0,08 / 0,16 | 0,1 / 0,12 |
| mola: frequência (Hz) / amortecimento / ganho | 2,2 / 0,55 / 1,4 | 2,6 / 0,75 / 0,9 | 3,4 / 0,5 / 1,0 | 3,2 / 0,45 / 1,2 | 3,0 / 0,4 / 1,3 | 2,8 / 0,55 / 1,0 |
| `flinchScale` | 0,75 | 1,0 | 1,2 | 1,1 | 1,3 | 1,0 |
| `proceduralOverlap` (liga/desliga a camada de *overlap*) | sim | sim | sim | sim | sim | sim |
| `turnFollowThrough` / `turnLagMaxDegrees` (°) | 0,45 / 10 | 0,3 / 7 | 0,25 / 6 | 0,35 / 8 | 0,4 / 9 | 0,35 / 8 |

**Camada de *overlap* por herói:** com `proceduralOverlap` desligado, o herói fica só com o clipe, a inclinação e
o IK dos pés (sem molas de cabeça, braços e tronco, sem *follow-through* na virada e sem o balanço extra da
pelve). O *follow-through* da virada é uma mola de giro (yaw) na cabeça, no pescoço e no peito alto, que recebe
o contrário da aceleração de giro do corpo (`MotionFeelMath.TurnLagDrive`): ao começar a virar, a cabeça fica
para trás; ao parar de virar, passa um pouco do ponto e volta. Numa virada constante não há atraso (o peito
continua puxando a virada). "Reduzir Movimento" corta pela metade, como as outras molas.

### Curvas (dá para ajustar no Inspector)

- `accelerationCurve`: multiplicador da aceleração pela fração da velocidade alvo. Começa acima de 1
  (um primeiro impulso forte) e termina abaixo de 1 (chega devagar na velocidade máxima). Joaquim
  começa em 1,25 e Maithe em 1,9.
- `leanByAcceleration`: graus de inclinação para a frente em função da aceleração (m/s²).
- `landingSquashByFallSpeed`: de 0 a 1 do `landingSquash`, pela velocidade de queda (m/s). Abaixo de
  2 m/s não achata. O pulo padrão (1,2 m) toca o chão a ~6,6 m/s.
- `attackArmCurve`: o braço ao longo do eixo do ataque. **De 0 a 0,5 é a antecipação, e 0,5 cai
  exatamente no frame do dano** (`PlayerBasicAttack.StrikeDelay`, 0,22 s hoje). De 0,5 a 1 vem o
  *follow-through* e a recuperação, que dura `attackRecoverTime`. O padrão arma o braço alto em 0,4 e
  desce mais rápido justo em 0,5.

## 4. Mapa do pedido

| Pedido | Onde |
|---|---|
| Idle nunca parado | respiração (peito e ombros), troca de peso (pelve rola e desloca de lado, pés presos), ruído na cabeça e nos ombros, olhadas ocasionais (`IdleGlanceYaw`) |
| Andar e correr | clipe com passada casada, mais balanço da pelve (IK), contra-rotação do tronco e braço oposto lidos dos pés animados, e inclinação |
| Ataque | antecipação até o dano, impacto (descida mais rápida do braço e avanço do corpo), *follow-through* (torção passa do centro), recuperação; recomeço suave se atacar de novo no meio |
| Pulo | agachamento só visual na decolagem (compressão instantânea que a mola solta em ~60 ms, sem atrasar o pulo), esticada no ar, joelhos encolhidos, *squash* e mergulho na aterrissagem conforme a queda, cabeça e braços acompanhando |
| Mudança de direção | acima de `turnBrakeAngle` o motor freia por um instante; o peito puxa a virada (`TurnAngle`), o corpo inclina e a pelve abaixa no apoio |
| Impacto e morte | a mola leva um chute para longe do atacante e volta passando um pouco do ponto; na morte, o corpo cai em t², quica com *squash* ao bater no chão e afunda |
| Peso por herói | perfil acima |
| Secundário | molas de *overlap* em cabeça, pescoço, peito e braços. Ossos de cabelo, capa, arma etc. são detectados pelo nome se um dia existirem. Piscar só funciona se houver blendshape "blink" |
| Pés firmes | casamento de passada, pés presos com passo de reposição ao girar e IK no chão |
| Acessibilidade | com "Reduzir Movimento" ligado (aba Acessibilidade), o *squash*, o *overlap* e o balanço do passo ficam pela metade (`_reducedMotionScale`, padrão `AccessibilitySettings.ReducedMotionScale` = 0,5). "Reduzir Flashes" não mexe mais na animação |

## 5. Limitações conhecidas

- Os rigs da Meshy só têm os 24 ossos humanoides padrão (`Hips … Head, head_end, headfront`). **Não
  há ossos de cabelo, roupa, capa, olhos ou mandíbula, nem blendshapes.** Por isso não existe
  micro-expressão facial nem *overlap* de cabelo de verdade. O código já procura esses ossos e
  blendshapes, então passa a funcionar sozinho em modelos futuros.
- Maria é **Generic**: não tem IK de pé nem pés presos, mas tem todo o resto. Maithe não tem rig e usa o
  modo de corpo inteiro.
- O IK, os parâmetros novos e os estados de ar só funcionam **depois de rodar o rebuild**.

## 6. Como ajustar

1. Entre no Play Mode com o herói e abra o `Player`.
2. No `PlayerProceduralAnimator ▸ Motion profile`, ajuste as molas (frequência baixa deixa o movimento
   mais solto e pesado; amortecimento baixo dá mais balanço), a antecipação (`attackArmCurve`) e o idle.
   Para mexer na inércia ao vivo, use os campos do `PlayerMotor`, que recebem o perfil quando o herói é trocado.
3. Copie os valores para um asset *Hero Motion Profile* (ou para `HeroMotionProfile.For`) para guardá-los.
4. No `PlayerFootIK` (que fica no objeto do Animator do herói) ficam os ajustes do IK: raio de
   reposição (`_lockMaxDrift`), torção máxima (`_lockMaxTwist`) e duração e altura do passo.
5. Testes: `HeroMotionFeelTests`, `HeroLocomotionTests` e `FootstepMathTests` (EditMode).
6. Passos: no `PlayerFootsteps` do Player, `Source` mostra de onde vem o tempo da pisada (`Baked` depois do
   rebuild) e `StepCount` conta os passos. Velocidade mínima e intervalo ficam no mesmo componente, e a poeira no
   `PlayerFootstepDust`.

## 7. Inimigos: mesmo peso, sem rig

Os inimigos não têm esqueleto. `EnemyProceduralAnimator` posa o objeto visual inteiro (`EnemyVisual`) e lê
o perfil do arquétipo (`EnemyMotionProfile`, em `Gameplay/Enemies/EnemyMotion.cs`). O perfil é escalado
pelo tamanho real do modelo (altura ÷ `ReferenceHeight`) com `ScaledForBody`: inércia = √tamanho (×1,15
para chefes). Corpo maior tem mais massa (tamanho³, ×1,5 para chefe), molas mais lentas, *ease* e
recuperação mais longos, empurrão menor e menos *overshoot* ao surgir. **Os campos de jogabilidade nunca
são escalados** (aceleração, giro e `BaseWindup`), e `EnemyController.Motion` continua entregando os
valores do arquétipo.

| Campo | Lobo (Quadruped) | Aranha/Escorpião (Arthropod) | Morcego (Flyer) | Pesado/chefe (Heavy) |
|---|---|---|---|---|
| `Mass` (referência) | 1 | 0,4 | 0,25 | 4 |
| `GaitEaseTime` (s) | 0,18 | 0,08 | 0,30 | 0,40 |
| mola de inclinação (Hz / amortecimento) | 2,6 / 0,45 | 4,5 / 0,6 | 1,6 / 0,35 | 1,3 / 0,6 |
| inclinação por aceleração (°/(m/s²) / máx.) | 1,2 / 7 | 0,8 / 5 | 2,0 / 14 | 1,0 / 5 |
| *surge* (m/(m/s²) / máx. m) | 0,01 / 0,06 | 0,005 / 0,03 | 0,02 / 0,10 | 0,015 / 0,08 |
| freada em curva (°) | 8 | 4 | 0 (inclina na curva) | 5 |
| antecipação: *squash* / agachar (m) / recuo (m) | 0,05 / −0,05 / 0,15 | 0,04 / −0,03 / 0,10 | 0,035 / +0,30 (sobe) / 0,20 | 0,05 / −0,06 / 0,12 |
| golpe: *stretch* / avanço (m) / *follow-through* | 0,05 / 0,50 / 15 % | 0,04 / 0,42 / 10 % | 0,05 / 0,60 / 20 % | 0,035 / 0,35 / 8 % |
| *windup* de jogo (s, inalterado) | 0,42 | 0,40 | 0,36 | 0,65 |
| impacto: mola (Hz / amort.) / empurrão (m) / inclinação (°) | 3,2 / 0,38 / 0,22 / 14 | 4,5 / 0,42 / 0,20 / 12 | 2,6 / 0,30 / 0,30 / 20 | 2,2 / 0,50 / 0,08 / 6 |
| mola de *squash* (Hz / amort.) | 4 / 0,35 | 6 / 0,4 | 3,5 / 0,3 | 2,4 / 0,45 |
| quique da morte (m) / *overshoot* ao surgir | 0,08 / 1,7 | 0,06 / 2,0 | 0,12 / 1,4 | 0,05 / 1,1 |
| respiração (Hz / *squash*) | 0,45 / 0,02 | 0,6 / 0,012 | 0,3 / 0,015 | 0,25 / 0,03 |
| olhada (intervalo s / ° / duração s) | 4,5 / 22 / 0,9 | 3 / 10 / 0,25 | 5 / 12 / 0,6 | 7 / 15 / 1,4 |
| personalidade (`IdleFidgetDegrees`) | fareja (6°) e levanta o focinho ao olhar | tremidas (7°); pernas separadas dão piparotes | deriva lenta e inclina a cabeça (5°) | peito arfando e ombros rolando (1,5°) |

- **Partida, parada e curva:** a velocidade do passo segue a real por uma mola criticamente amortecida
  (`GaitEaseTime`). Molas de inclinação e *surge* fazem o corpo inclinar para a frente ao arrancar, ficar
  para trás do colisor, inclinar para trás ao frear e passar um pouco do ponto ao parar. Numa curva fechada
  em movimento (`TurnBrake01`), o corpo empina para trás e agacha no pivô.
- **Ataque:** o *windup* vem do `EnemyController` (a mesma duração e a mesma janela de esquiva de antes).
  Os primeiros 60 % da parte antes do golpe juntam a antecipação (agachar ou subir, recuar, inclinar para trás,
  *squash*), e o resto treme. O golpe ocupa os últimos `StrikeDuration` (no máximo metade do *windup*), acelera
  e chega ao pico esticado **exatamente** no momento do dano. Se o aviso do golpe atrasar um frame, o pico
  fica parado. A recuperação passa do pico (*follow-through*) e volta passando um pouco do repouso.
- **Impacto:** uma mola recebe um chute para longe do atacante, com o pico calibrado
  (`MotionFeelMath.SpringVelocityForPeak`), e volta passando um pouco do ponto.
- **Morte:** o tombo existente; ao bater no chão, *squash* e dois quiques (`ImpactBounce01`). O afundar
  continua no `EnemyDeathVfx`.
- **Surgir:** sobe esticado, passa do ponto (`SpawnRise01` com `SpawnOvershoot`) e acomoda com *squash*.
- **Squash:** preserva o volume e vai até 6 % (`MotionFeelMath.SquashScale`), igual ao dos heróis.
- **Pernas separadas (modelos antigos):** filhos chamados `leg`, `perna` ou `pata` (até 8) tremem no
  idle e correm com o passo, só nos artrópodes e só quando não há pernas articuladas (abaixo).
- **"Reduzir Movimento":** reduz pela metade o *squash*, o *overlap* (o quanto as molas passam do alvo; a
  inclinação em si fica), o balanço do passo, os quiques e o *overshoot*. O aviso do ataque e o tombo
  continuam legíveis. A mesma opção corta pela metade o tremor de câmera (`ScaleShakeMagnitude`) e, junto com
  "Reduzir Flashes", desliga as animações de entrada do título, da história, do chefe e do tutorial
  (`AccessibilitySettings.SuppressUiMotion` / `UiMotionPreferences`).
- Testes: `EnemyMotionFeelTests` e `EnemyProceduralMotionTests` (EditMode).

### 7.1 Pernas de verdade (passos procedurais)

O lobo, a aranha e o escorpião da Meshy eram uma malha estática só (sem esqueleto, FBX de 19 a 28 MB),
então foram trocados por modelos low-poly feitos em código (`WolfRecipe` com o Kök-Böri ×1,35,
`SpiderRecipe`, `ScorpionRecipe`), com articulações de verdade, como os outros inimigos procedurais. O
golem, o Senhor da Guerra, o Guardião da Ruína e o Tages, o Habitante Ancestral ganharam joelho e tornozelo.
Convenção de nomes (genérica, serve para o Ari ou outro bicho): quadril `Leg_<id>` → joelho
`Shin_<id>` → pé `Foot_<id>` (bípedes `L`/`R`, quadrúpedes `FL`/`FR`/`HL`/`HR`, artrópodes `L0..R3`).
*Follow-through*: `Tail` → `Tail_1..` → `Stinger`, `Head`, `Ear_L/R`, `Abdomen`, `Claw_L/R`.

- **Motor** (`Gameplay/Locomotion`, sem nada de inimigo): `LegGaitMath` (matemática pura),
  `LegGaitSolver` (máquina de estados por perna, sem transforms) e `ProceduralLegLocomotion` (liga os
  transforms, faz raycast e escreve as rotações). O `EnemyProceduralAnimator` o adiciona sozinho no
  visual que tem pernas articuladas e converte o perfil do arquétipo com `EnemyLegGait`.
- **Pé plantado:** o pé no chão nunca se mexe no mundo. Ele só levanta na janela do seu passo (fase do
  relógio + defasagem do *gait*) e se já se afastou do ponto de repouso sob o quadril. Fora do alcance,
  pisa na hora (giro, empurrão, investida). O pé cai onde o quadril vai estar no meio do apoio: a
  previsão usa velocidade, giro e meio apoio. IK de dois ossos dobra o joelho para o lado da pose de
  repouso (aranha para cima, jarrete do lobo para trás, joelho do golem para frente). O pé fica rente ao
  chão e inclina com a rampa.
- **Gaits:** quadrúpede anda (sequência lateral) → trota (diagonais juntas) → galopa (transverso),
  misturando pela velocidade. Aranha e escorpião usam tetrápode alternado com onda de trás para frente.
  Bípedes alternam e passam de andar (apoio duplo) para correr (fase de voo). A passada cresce com a
  velocidade e a cadência sobe com ela. Perna curta pisa mais rápido e perna longa mais devagar
  (pêndulo, √comprimento). Ao correr, o corpo agacha mais e o pé ganha alcance.
- **Corpo:** o balanço, a rolagem e o *pitch* vêm dos pés levantados (o corpo sobe no meio do passo e
  rola para o lado do apoio), em molas. O quadril nunca sobe além do alcance dos pés plantados (*clamp*
  da pelve): pulo de alerta, quique e golpe não arrastam pé. Pisada pesada publica `EnemyFootstepEvent`
  no pé que desceu, com *squash*.
- **Girar no lugar:** parado e girando acima de ~20–30 °/s, o relógio anda na cadência de repouso e os
  pés vão se reposicionando. Ao parar, os pés acomodam sob o corpo e o relógio para.
- **Ataque:** a antecipação continua do `EnemyController`/`EnemyAttackWindupEvent`. O agachamento com
  os pés plantados dobra os joelhos, e a investida dá um passo de reação se passar do alcance.
- **Cauda, orelhas, cabeça, abdômen, pinças:** molas de `MotionFeelMath` seguem o giro, o quique
  vertical e a velocidade. A cabeça compensa o *pitch* do corpo (olhar estável). Peças que o
  `ProceduralEnemyRig` já anima (cabeça do golem e do guardião, cabeça e orelhas do habitante) ficam com
  ele.
- **Morcegos:** sem pernas. A asa tem batida de força rápida e recuperação lenta, com planeios de asa
  aberta que ficam mais frequentes quando o enxame viaja (`ProceduralEnemyRig.BatWingAngle`).
- **Desempenho (celular):** só há raycast quando um passo começa (e um de novo no meio do balanço), com
  teto global de 40 raycasts por frame (`ProceduralLegLocomotion.MaxRaycastsPerFrame`). Sem orçamento,
  usa o plano dos pés. LOD: a mais de 24 m da câmera (`_legsLodDistance`, com histerese de 3 m), fora da
  tela, surgindo do chão ou morto, as pernas voltam ao repouso em 0,25 s e o passo antigo do corpo
  inteiro volta. Ao reativar, os pés são replantados onde o inimigo está, o que funciona com o *pool*.
  Nada aloca por frame.
- **"Reduzir Movimento":** reduz o balanço, a rolagem e o *follow-through*. O passo em si não muda,
  porque é ele que impede o pé de patinar.
- **Rebake obrigatório:** **Rapadura ▸ Enemies ▸ Build Procedural Enemy Models** gera os novos prefabs
  (Wolf, AlphaWolf, Spider, Scorpion) e refaz os quatro bípedes com joelho. Também liga cada prefab ao
  `_visualPrefab` do seu `EnemyDefinition`. Sem o bake, o lobo, a aranha e o escorpião continuam com a
  malha da Meshy, e os bípedes já assados usam pernas de um osso só, que balançam até o pé. Para voltar à
  Meshy, recoloque o FBX no `_visualPrefab`.
- Testes: `LegGaitTests` (fases, *duty*, cadência, gatilho do passo, previsão, IK, invariante de "pé
  plantado não desliza" andando, galopando, correndo como aranha, pisando, parando e girando),
  `ProceduralLegLocomotionTests` (modelos reais, IK segurando o pé, animador de ponta a ponta, morte e
  *respawn*) e `ProceduralEnemyModelsTests` (cadeias `Leg_`/`Shin_`/`Foot_`).

### 7.2 Bichos da noite (modelos próprios, 2026-10-30)

Seis espécies noturnas ganharam receita própria em `Enemies/Visuals` (detalhes de jogo em `Docs/DayNight.md` §2.2):
`WerewolfRecipe` (Mardagayl, quadrúpede de 4 pernas no motor de passos), `PeccaryRecipe` (javali, quadrúpede),
`VampireBatRecipe`, `ShroudOwlRecipe` e `PotooRecipe` (voadores, asas `Wing_L/R`, sem pernas de passo) e
`FireflySwarmRecipe` (enxame: `Swarm` → `Firefly_n` → `Lantern`, como o dos morcegos). Todas com olhos nas células
que brilham da paleta (`WarEye`, `MoonEye`, `Ember`, `FireflyGlow`); a paleta cresceu de 8×4 para 8×6 (só acrescentou
células no fim — por isso o rebake abaixo é obrigatório para os prefabs já assados).

- **Mardagayl empina para bater:** o peito, a cabeça e as patas da frente ficam em `Torso`, com pivô no quadril.
  Durante a antecipação (`EnemyProceduralAnimator.AttackPhase == Windup`) o `ProceduralEnemyRig` gira o `Torso`
  52° para cima, ergue os ombros (`Shoulder_L/R`) e abre a mandíbula (`Jaw`); as patas da frente soltam o chão pela
  extensão aditiva `ProceduralLegLocomotion.FrontLegsWeight` (0..1, padrão 1 = comportamento antigo: multiplica o
  peso do IK só das pernas da metade da frente e tira essas pernas do "passo de reação" do quadril). Depois do
  golpe ele volta para as quatro patas. É telegrafia: "Reduzir Movimento" não diminui.
- **Ave-Graveto de Omöl:** o bico minúsculo abre num bocão rosa (`Jaw`, 48°) na antecipação; carrega o galho (`Branch`).
- **Coruja:** batida lenta com planeio; de vez em quando a cabeça gira ~80° (a cabeça fica com o rig, não com as
  molas do motor de passos: `ProceduralEnemyRig.PartsAnimatedBy`).
- **Vaga-lumes:** o enxame gira devagar, cada um flutua, e as lanternas piscam (escala 0,8–1,25, nunca apagam).
- **Som:** cada modelo noturno leva um `NightCreatureVoice` (chamado ao chegar/rondar + som no início de cada
  antecipação), ver `Docs/Audio.md`.
- **Rebake:** **Rapadura ▸ Enemies ▸ Build Procedural Enemy Models** (assa os 6 novos e refaz os antigos com a paleta
  nova); depois, se o Ari já tiver sido assado, **Rapadura ▸ Pets ▸ Build Caramelo Model**.
- Testes: `NightSpeciesModelsTests` (+ as checagens genéricas de `ProceduralEnemyModelsTests`).
