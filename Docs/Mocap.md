# Mocap: animações Humanoid a partir do CMU Motion Capture Database

> Fonte principal: o **CMU Graphics Lab Motion Capture Database**, com captura óptica (Vicon) de pessoas reais.
> O uso comercial é liberado: *"free for use in research and commercial projects worldwide"* e
> *"may be copied, modified, or redistributed without permission"*. Os trials, o texto exato da licença
> e a atribuição estão em `tools/mocap/cmu/SOURCES.md`. Crédito sugerido para a tela de Créditos:
> **"Motion capture data from mocap.cs.cmu.edu (NSF EIA-0196217)"**.

## 1. Visão geral

1. `tools/mocap/cmu/*.bvh`: os BVH brutos (conversão cgspeed, 120 fps, T-pose no frame 0).
2. `tools/mocap/bvh_prep.py` (Python 3.11 em venv `uv`) corta, acha a janela de loop, reamostra para 30 fps,
   converte para metros, vira para +Z, põe os pés no chão, tira a deriva (in-place) e grava em
   `tools/mocap/out/` + `manifest.json`.
3. Unity, menu **Rapadura ▸ Animation ▸ Import CMU Mocap Set**: para cada clipe do manifest, uma
   instância oculta do `Icaro_Rigged_1` é posada frame a frame e `HumanPoseHandler.GetHumanPose` lê
   o espaço de músculos. O resultado é gravado em `Assets/Animations/Mocap/Mocap_*.anim`.
4. Por ser **Humanoid**, o mesmo `.anim` serve para Ícaro, Joaquim, Lavine, Maria (e Maithe, quando tiver rig).

### Como o retarget funciona (`MocapMath.cs`)

- **Espaço:** o BVH é destro (Y para cima, frente +Z) e o Unity é canhoto. A conversão espelha X:
  posições `(−x, y, z)` e rotações `(x, −y, −z, w)`. O lado esquerdo do personagem continua sendo o esquerdo.
- **Poses de repouso:** a T-pose do BVH (frame 0) e a T-pose do Avatar (`humanDescription.skeleton`) são
  normalizadas para uma T-pose ideal. Braços ficam em ±X, pernas para baixo, coluna para cima e pés para a frente.
  Por isso tanto um rig em A-pose quanto um em T-pose casam.
- **Por osso:** `rotaçãoUnity(t) = rotaçãoBVH(t) · inverso(repousoBVH) · repousoUnity`, em espaço de mundo,
  dos pais para os filhos. A altura do quadril é escalada pela razão entre as alturas de quadril do herói e do ator.
- **Mapeamento CMU → Humanoid:** Hips, LeftUpLeg/LeftLeg/LeftFoot/LeftToeBase → UpperLeg/LowerLeg/Foot/Toes;
  LowerBack/Spine/Spine1 → Spine/Chest/UpperChest (sem UpperChest, Spine1 dirige o Chest); Neck, Head;
  LeftShoulder/LeftArm/LeftForeArm/LeftHand → Shoulder/UpperArm/LowerArm/Hand (e o mesmo para o lado direito). Dedos ficam neutros.
- **Clipe:** 95 curvas de músculo + `RootT`/`RootQ`. Clipes em loop recebem *Loop Time* + *Loop Pose*, e o
  erro final→inicial é distribuído ao longo do clipe. *Root Transform Rotation/Position* ficam "Bake Into Pose",
  com base em "Original". A exceção são clipes `root_motion` (a virada de 90°).

## 2. Clipes gerados

| Clipe | Trial CMU | Uso sugerido no jogo |
|---|---|---|
| `Mocap_Idle` | 137_28 Normal Wait | Idle fora de combate (loop) |
| `Mocap_IdleCombat` | 77_03 ready stance | Idle em combate / guarda (loop) |
| `Mocap_Walk` | 35_01 walk | Locomoção, andar (loop, no lugar) |
| `Mocap_Jog` | 35_17 run/jog | Locomoção, trote (loop, no lugar) |
| `Mocap_Run` | 09_02 run | Locomoção, corrida/sprint (loop, no lugar) |
| `Mocap_Strafe` | 139_14 Sneak Sideways | Passo lateral (loop, no lugar) |
| `Mocap_TurnLeft90` | 16_17 walk, 90° left turn | Virada com root motion (espelhar para a direita: *Mirror* no estado) |
| `Mocap_Jump` | 16_01 jump | Pulo parado |
| `Mocap_AttackPunch` | 143_23 Punching | Ataque corpo a corpo (Joaquim/Lavine) |
| `Mocap_AttackSword` | 02_07 swordplay | Golpe com arma de duas mãos |
| `Mocap_Kick` | 135_04 Front Kick | Ataque alternativo / habilidade |
| `Mocap_Block` | 144_26 Right_Blocks | Defender |
| `Mocap_Cast` | 120_03 cast spell | Magia / cura (Maria, Maithe) |
| `Mocap_HitReact` | 77_09 duck (1,6×) | Reação a dano / esquiva |
| `Mocap_DeathForward` | 90_16 fall on face | Morte (cai para a frente) |
| `Mocap_DeathBackward` | 90_18 RugPullFall | Morte (cai de costas) |

O CMU não tem uma "reação a golpe" pura nem um "cast" épico. Há candidatos alternativos em `SOURCES.md`.
Para trocar um clipe, edite `tools/mocap/clips.json` (trial, `start`/`end`, `speed`) e rode de novo os passos 3 e 4.

## 3. Rodar o Python (só quando mudar `clips.json`)

`tools/mocap/out/` já vem processado, então este passo é opcional.

```powershell
cd D:\Projetos\rapadura\tools\mocap
uv venv --python 3.11 .venv                                   # primeira vez
uv pip install --python .venv\Scripts\python.exe -r requirements.txt
.venv\Scripts\python.exe bvh_prep.py --set clips.json --out-dir out
.venv\Scripts\python.exe -m pytest -q tests                   # opcional
```

## 4. Importar no Unity

1. Abra o projeto e espere compilar.
2. Menu **Rapadura ▸ Animation ▸ Import CMU Mocap Set**. Ele lê `tools/mocap/out/manifest.json`, usa
   `Assets/Models/Characters/Icaro_RO/Rigged/Icaro_Rigged_1.fbx` como referência e grava 16 clipes em
   `Assets/Animations/Mocap/`. O Console mostra uma linha por clipe e o total no fim.
   Para reimportar, rode de novo: os `.anim` são sobrescritos no lugar e mantêm o GUID, então os Animators continuam ligados.
3. Para um BVH avulso, use **Rapadura ▸ Animation ▸ Import Mocap BVH…**. Lá dá para escolher o modelo, o arquivo,
   o nome, a escala (botão "CMU bruto" para BVH direto do CMU), Loop, In-place e Root motion.
4. Testes: **Window ▸ General ▸ Test Runner ▸ EditMode**, assembly `Rapadura.Tests.Mocap` (`MocapMathTests`).
5. Linha de comando (CI): `Unity.exe -batchmode -projectPath . -executeMethod Rapadura.Editor.Mocap.MocapImportWindow.ImportSetBatch -quit`.

Para conferir: arraste um `Mocap_*.anim` para a janela **Animation Preview** do Inspector com um herói Humanoid,
ou crie um estado temporário no Animator. Se um membro torcer errado, o problema provavelmente está na T-pose do
Avatar (*Configure Avatar ▸ Pose ▸ Enforce T-Pose*) e não no clipe.

## 5. Ligar nos Animators dos heróis

Os controllers `*_AnimatorController.controller` têm o blend tree **Locomotion** (parâmetro `MoveBlend`:
0 = idle, 0,5 = andar, 1 = correr, e `Speed` alimentado por `PlayerAnimatorSpeedDriver`).

- **Locomoção:** no blend tree, troque os Motions por `Mocap_Idle` (0), `Mocap_Walk` (0,5) e `Mocap_Run` (1).
  O `Mocap_Jog` pode entrar como um threshold intermediário (ex.: 0,75). Os clipes são *in-place*, então
  `Apply Root Motion` fica **desligado** e o `PlayerMotor` continua movendo o personagem. Ajuste
  `WalkClipSpeed`/`RunClipSpeed` para os pés não deslizarem: o andar do CMU anda ~1,3 m/s, o trote ~3,1 m/s
  e a corrida ~3,5 m/s, na escala do ator.
- **Combate:** crie estados `Attack` (`Mocap_AttackPunch`/`Mocap_AttackSword`), `Cast`, `Block`, `Hit` e `Death`
  disparados por Triggers. Transições de saída com *Has Exit Time* ≈ 0,85 e duração 0,1–0,15 s.
  `Death` não tem transição de saída.
- **Layers:** para atacar andando, use uma layer "UpperBody" com Avatar Mask só do tronco e dos braços.
- **Espelho:** em clipes Humanoid, a opção *Mirror* do estado faz a versão esquerda/direita sem gerar outro clipe.

Por que Humanoid: os rigs da Meshy usam nomes diferentes entre si (`Spine02` como Spine etc.). O retarget por
músculos do Unity resolve isso, e um único conjunto de clipes serve para os 5 heróis.

## 6. Outras fontes de movimento (secundárias)

| Opção | Qualidade / esforço | Licença (conferir antes de usar) |
|---|---|---|
| **Mais trials do CMU** (`fetch_cmu.py --search`) | Boa (mocap real); ~2.500 trials | **Comercial OK** |
| Mixamo (Adobe) | Boa, com muitos golpes de jogo prontos | Royalty-free em jogos comerciais; não pode redistribuir os arquivos avulsos. Precisa de conta Adobe, download manual |
| **Text-to-motion**: MoMask, MDM, T2M-GPT, MotionGPT | Média; gera BVH/SMPL a partir de texto, roda na RTX 5080 | ⚠️ **NÃO COMERCIAL:** os pesos são treinados no HumanML3D/KIT-ML, que derivam do **AMASS** (licença só para pesquisa), e a saída usa o corpo **SMPL** (licença MPI não comercial). O código pode ser MIT, mas os pesos/dados não liberam uso comercial |
| **Text-to-video local** (Wan 2.2, Apache-2.0) + mocap de vídeo | Baixa/média: vídeo gerado tem anatomia instável; último recurso | Wan 2.2: Apache-2.0. Para o mocap de vídeo: **MediaPipe Pose Landmarker** (Apache-2.0, **comercial OK**, roda na CPU, só posições 3D, qualidade menor). GVHMR, WHAM, 4D-Humans/HMR2.0, TRAM e MotionBERT têm resultado melhor, mas são ⚠️ **não comerciais** (pesos de pesquisa + SMPL/AMASS) |
| Filmar uma pessoa (celular, de lado, corpo inteiro, 5–10 s, 30/60 fps) + MediaPipe | Média | Comercial OK com MediaPipe. Não implementado agora, porque o usuário não pode gravar. O caminho seria MediaPipe `pose_world_landmarks` → suavização (One Euro) → mirar os ossos pelas direções das juntas → mesmo `HumanPoseHandler` |

Para um jogo comercial, fique no **CMU** (ou Mixamo). Use text-to-motion e mocap com SMPL só para
protótipo/referência interna e nunca entregue esses clipes na build.
