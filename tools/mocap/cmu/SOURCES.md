# Fontes dos BVH (CMU Motion Capture Database)

## Origem

- **Dados originais:** CMU Graphics Lab Motion Capture Database — http://mocap.cs.cmu.edu
  (banco criado com financiamento da NSF EIA-0196217). O original é ASF/AMC a 120 fps.
- **Conversão BVH:** "MotionBuilder-friendly BVH conversion" (versão 2010) de Bruce Hahne / cgspeed.
  Cada arquivo tem uma **T-pose acrescentada no frame 0**, olhando para +Z; Frame Time = 0.0083333 (120 fps).
  As unidades são as do CMU: 1 unidade = (1/0.45) polegada = 0,05644 m.
- **Download:** espelho https://github.com/una-dinosauria/cmu-mocap
  (`data/<sujeito com 3 dígitos>/<trial>.bvh`), baixado em 2026-10-22 com `fetch_cmu.py`.
- `cmu-mocap-index.txt` é o índice de descrições que vem junto com a conversão cgspeed.

## Licença (texto exato)

FAQ de mocap.cs.cmu.edu ("How can I use this data?"), conferido em 2026-10-22:

> The motion capture data may be copied, modified, or redistributed without permission.

O README da conversão (READMEFIRST v1.1, B. Hahne, 26/06/2010), seção "USAGE RIGHTS":

> CMU places no restrictions on the use of the original dataset, and I (Bruce) place no additional
> restrictions on the use of this particular BVH conversion.
>
> Here's the relevant paragraph from mocap.cs.cmu.edu:
>
> Use this data! This data is free for use in research and commercial projects worldwide. If you
> publish results obtained using this data, we would appreciate it if you would send the citation to
> your published paper to jkh+mocap@cs.cmu.edu, and also would add this text to your acknowledgments
> section: "The data used in this project was obtained from mocap.cs.cmu.edu. The database was
> created with funding from NSF EIA-0196217."

**Resumo:** o uso comercial dentro do jogo é permitido. Crédito não é obrigatório, mas é pedido; pomos
"Motion capture data from mocap.cs.cmu.edu (NSF EIA-0196217)" nos Créditos (ver `Docs/ThirdPartyLicenses.md`).

## Trials usados

O corte/janela final de cada clipe está em `../clips.json`. As durações abaixo são do clipe a 30 fps, sem a T-pose.

| Clipe Unity | Trial | Descrição no índice CMU | Corte (s, tempo original) | Loop | Duração |
|---|---|---|---|---|---|
| Mocap_Idle | 137_28 | Normal Wait | janela automática em 0–9 | sim | 2,83 s |
| Mocap_IdleCombat | 77_03 | ready stance | janela automática em 1,5–8 | sim | 2,70 s |
| Mocap_Walk | 35_01 | walk | janela automática (1 ciclo) | sim, no lugar | 1,13 s |
| Mocap_Jog | 35_17 | run/jog | janela automática (1 ciclo) | sim, no lugar | 0,77 s |
| Mocap_Run | 09_02 | run | janela automática (1 ciclo) | sim, no lugar | 0,73 s |
| Mocap_Strafe | 139_14 | Sneak Sideways | janela automática em 1–8 | sim, no lugar | 1,47 s |
| Mocap_TurnLeft90 | 16_17 | walk, 90-degree left turn | inteiro | não, com root motion | 4,30 s |
| Mocap_Jump | 16_01 | jump | 0,2–2,2 | não | 1,97 s |
| Mocap_AttackPunch | 143_23 | Punching | ciclo automático em 0,6–4,2 | não | 1,80 s |
| Mocap_AttackSword | 02_07 | swordplay | ciclo automático em 0,3–5,0 | não | 1,97 s |
| Mocap_Kick | 135_04 | Front Kick | 2,2–3,9 | não | 1,67 s |
| Mocap_Block | 144_26 | Right_Blocks | 0,8–2,4 | não | 1,57 s |
| Mocap_Cast | 120_03 | Mickey cast spell | 4,2–6,2 (braços acima da cabeça) | não | 1,97 s |
| Mocap_HitReact | 77_09 | duck to avoid flying object | 1,2–4,0, acelerado 1,6× | não | 1,73 s |
| Mocap_DeathForward | 90_16 | fall on face | 1,8–5,0 | não | 3,17 s |
| Mocap_DeathBackward | 90_18 | RugPullFall | 0,0–2,6 | não | 2,57 s |

São 16 arquivos brutos, cerca de 9 MB. Observações:

- O CMU não tem uma "reação a golpe" de verdade. O `Mocap_HitReact` é a esquiva/encolhida do 77_09,
  acelerada. Outras opções no índice são 91_59 "360 smallStumble", 85_07 "KickFlipStumble" e 104_13 "StumbleWalk".
- O `Mocap_Cast` vem de uma atuação caricata ("Mickey"). Outras opções são 13_26 a 13_28 "direct traffic, wave",
  14_20 "reach up" e 144_30 "Sun Salutation".
- Para procurar outros trials: `python fetch_cmu.py --search "<palavra>"`. Para baixar: `python fetch_cmu.py 86_01`.
