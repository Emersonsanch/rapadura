# Animação dos heróis: peso, inércia e personalidade

> Como o movimento dos heróis é montado em camadas, quais parâmetros o Animator recebe, como cada
> herói tem um "jeito de se mexer" próprio (perfil de movimento) e como ajustar. Código em
> `Assets/Scripts/Gameplay/Player/`; o controller é gerado por `Assets/Scripts/Editor/HeroAnimatorRebuilder.cs`.

## 1. Camadas, de baixo para cima

1. **PlayerMotor** (jogabilidade): aceleração, frenagem, curvas e frenagem em curva fechada. Os
   tempos vêm do perfil do herói (`ApplyMotionProfile`). As velocidades (andar/correr) continuam
   sendo balanceamento e não mudam com o perfil.
2. **Animator** (clipes Walk/Run da Meshy): blend tree de locomoção com a velocidade do clipe casada
   à velocidade real, para o pé não patinar.
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
| `TurnAngle` | float | driver | ângulo com sinal (+ = direita) entre a frente e a direção desejada, dampTime 0,12 s |
| `IsGrounded` | bool | driver | motor no chão |
| `VerticalSpeed` | float | driver | m/s (+ = sobe), dampTime 0,05 s no ar; ao tocar o chão segura a velocidade do impacto por 0,12 s |

### Estados da camada base

```
Locomotion (blend 1D: IdlePose 0 / Walk 0,5 / Run 1)
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
- A camada de ações do mocap CMU continua **desligada** (`IncludeMocapActions`), porque os clipes deformam os heróis.

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
| respiração (Hz / °) | 0,22 / 2,0 | 0,24 / 1,4 | 0,30 / 1,4 | 0,30 / 1,6 | 0,36 / 1,8 | 0,28 / 1,6 |
| troca de peso (° / m / período s) | 1,6 / 0,02 / 6,5 | 1,0 / 0,012 / 7,5 | 1,0 / 0,012 / 4,5 | 1,4 / 0,016 / 4,0 | 1,8 / 0,018 / 3,2 | 1,2 / 0,015 / 5 |
| micro-movimento (° / ritmo) | 0,8 / 0,4 | 0,6 / 0,35 | 1,0 / 0,6 | 1,8 / 0,9 | 2,2 / 1,1 | 1,0 / 0,6 |
| olhada (intervalo s / ° / duração s) | 7 / 25 / 1,2 | 6 / 30 / 1,3 | 4 / 40 / 0,8 | 3,5 / 45 / 0,6 | 2,8 / 50 / 0,5 | 5 / 35 / 0,9 |
| ataque: recuperação (s) / braço (°) / torção (°) / avanço (m) | 0,34 / 105 / 26 / 0,25 | 0,30 / 95 / 18 / 0,20 | 0,22 / 110 / 20 / 0,32 | 0,26 / 115 / 22 / 0,28 | 0,24 / 120 / 24 / 0,26 | 0,28 / 110 / 22 / 0,3 |
| squash na decolagem / aterrissagem | 0,05 / 0,06 | 0,035 / 0,045 | 0,04 / 0,05 | 0,04 / 0,055 | 0,045 / 0,055 | 0,04 / 0,06 |
| mergulho da pelve ao aterrissar (m) / encolher no ar (m) | 0,14 / 0,10 | 0,09 / 0,12 | 0,07 / 0,16 | 0,09 / 0,14 | 0,08 / 0,16 | 0,1 / 0,12 |
| mola: frequência (Hz) / amortecimento / ganho | 2,2 / 0,55 / 1,4 | 2,6 / 0,75 / 0,9 | 3,4 / 0,5 / 1,0 | 3,2 / 0,45 / 1,2 | 3,0 / 0,4 / 1,3 | 2,8 / 0,55 / 1,0 |
| `flinchScale` | 0,75 | 1,0 | 1,2 | 1,1 | 1,3 | 1,0 |

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
| Acessibilidade | com "Reduzir Flashes" ligado, o *squash* e o *overlap* ficam pela metade (`_reducedMotionScale`) |

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
5. Testes: `HeroMotionFeelTests` e `HeroLocomotionTests` (EditMode).
