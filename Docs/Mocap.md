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
- **Poses de repouso (`MocapRetargetRig`):** a T-pose do BVH (frame 0) corresponde à pose de bind do Avatar
  (`humanDescription.skeleton`), osso a osso. Só os **braços** (UpperArm/LowerArm) são normalizados para a T-pose
  ideal (±X), para um rig em A-pose casar com o BVH em T-pose.
- **Por osso:** `rotaçãoUnity(t) = rotaçãoBVH(t) · inverso(repousoBVH) · repousoUnity`, em espaço de mundo,
  dos pais para os filhos. A altura do quadril é escalada pela razão entre as alturas de quadril do herói e do ator.
- **Por que os heróis deformavam (corrigido em 2026-10-29):** o importador normalizava *todos* os ossos para a
  T-pose ideal, nos dois esqueletos. Com isso, a geometria do esqueleto do CMU era copiada para os rigs da Meshy:
  clavícula do CMU subindo ~25° a partir do esterno, "tornozelo→dedo" 13° para baixo (na Meshy é 35–68°),
  "joelho→tornozelo" da Meshy inclinado 22–40° para trás e colunas com curvaturas diferentes. Medido a partir dos
  esqueletos dos `.fbx.meta`, o frame de T-pose caía longe da pose de bind:

  | Herói | Coluna/peito/pescoço | Clavículas | Coxas | Canelas | Pés |
  |---|---|---|---|---|---|
  | Ícaro | 12–19° | 24° / 24° | 4° / 10° | 34° / 40° | 29° / 28° |
  | Joaquim | 21–25° | 13° / 27° | 13° / 13° | 23° / 22° | 26° / 24° |
  | Lavine | 5–6° | 17° / 20° | 14° / 15° | 24° / 25° | 55° / — |

  Nos clipes, isso aparecia como `Shoulder Front-Back` 1,23 e `Arm Down-Up` −1,1 constantes no `Mocap_Walk`
  (ombros encolhidos e braços para dentro do corpo), `Lower Leg Stretch` até 1,34 (joelho esticado além do limite)
  e `Upper Leg Twist` de −2 a ±3 no `Mocap_Run`/mortes (o Unity simulando a dobra do joelho com torção da coxa).
  Além disso, o clipe era gravado só no Ícaro e tocado no Joaquim e na Lavine, cujas poses de bind diferem da dele
  em 20–50° por membro. Agora a locomoção é gravada no próprio Avatar de cada herói (ver §4).
  Testes: `MocapRetargetTests` (o caso antigo continua reproduzível com `RetargetPolicy.NormalizeAll`).
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
| `Mocap_Walk` | 35_01 walk | Andar adulto (loop, no lugar); fica como alternativa |
| `Mocap_WalkKid` | 142_14 Rushed | **Andar dos heróis**: passos curtos e rápidos (~200 passos/min), tronco reto (loop, no lugar) |
| `Mocap_WalkHappy` | 142_08 Happy | Andar alegre, braço solto (alternativa ao `WalkKid`) |
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
   `Assets/Models/Characters/Icaro_RO/Rigged/Icaro_Rigged_1.fbx` como referência e grava 18 clipes em
   `Assets/Animations/Mocap/`. Os clipes com `"per_hero": true` em `clips.json` (Idle, Walk, WalkKid, WalkHappy,
   Jog, Run) também são gravados no Avatar de cada herói Humanoid, em `Assets/Animations/Mocap/<Herói>/`
   (Icaro, Joaquim, Lavine). O Console mostra uma linha por clipe; um clipe com músculo fora de ±1,2 ganha o sufixo
   "— out of range: …" (`MocapClipSanity`). Para reimportar, rode de novo: os `.anim` são sobrescritos no lugar e
   mantêm o GUID, então os Animators continuam ligados.
3. Para um BVH avulso, use **Rapadura ▸ Animation ▸ Import Mocap BVH…**. Lá dá para escolher o modelo, o arquivo,
   o nome, a escala (botão "CMU bruto" para BVH direto do CMU), Loop, In-place e Root motion.
4. Testes: **Window ▸ General ▸ Test Runner ▸ EditMode**, assembly `Rapadura.Tests.Mocap` (`MocapMathTests` e
   `MocapRetargetTests`, que usam os heróis reais e `tools/mocap/out/Mocap_Walk.bvh` e escrevem o erro por osso no
   resultado do teste).
5. Linha de comando (CI): `Unity.exe -batchmode -projectPath . -executeMethod Rapadura.Editor.Mocap.MocapImportWindow.ImportSetBatch -quit`.

Para conferir: arraste um `Mocap_*.anim` para a janela **Animation Preview** do Inspector com um herói Humanoid,
ou crie um estado temporário no Animator. Use o clipe da pasta do próprio herói. O retarget casa a T-pose do BVH
com a pose de bind gravada no Avatar, então não mexa no *Configure Avatar ▸ Pose* dos heróis sem reimportar o conjunto.

## 5. Ligar nos Animators dos heróis

Os controllers `*_AnimatorController.controller` têm o blend tree **Locomotion** (parâmetro `MoveBlend`:
0 = idle, 0,5 = andar, 1 = correr, e `Speed` alimentado por `PlayerAnimatorSpeedDriver`).

- **Locomoção (automático):** com **Rapadura ▸ Animation ▸ Use Mocap Locomotion** marcado (padrão), o
  **Rebuild Hero Animators** monta, para Ícaro, Joaquim e Lavine, o blend tree `IdlePose` 0 / `Mocap_WalkKid` 0,5 /
  `Mocap_Jog` 0,75 / `Mocap_Run` 1 e o estado `Idle` com `Mocap_Idle`, usando a versão gravada no próprio herói.
  A velocidade de cada clipe é medida no modelo do herói (`WalkClipSpeed`/`JogClipSpeed`/`RunClipSpeed`), e o
  `PlayerAnimatorSpeedDriver` casa a passada com a velocidade do `PlayerMotor`. Se faltar um clipe ou ele não
  passar no `MocapClipSanity`, o herói volta para o Walk/Run da Meshy, e o relatório diz por quê. Maria (Generic)
  e Maithe (sem rig) ficam sempre com os clipes da Meshy / modo de corpo inteiro. Os clipes são *in-place*:
  `Apply Root Motion` fica desligado. Os nomes estão em `HeroAnimatorRebuilder.MocapWalkClip` etc.
  (troque para `Mocap_WalkHappy` para testar o andar alegre).
- **Passada de criança:** a cadência vem do clipe (`Mocap_WalkKid`, ~200 passos/min, a ~1,1× para os 2,5 m/s
  do jogo; o andar adulto `Mocap_Walk` precisaria de ~1,9×). `HeroMotionProfile.strideScale` (padrão 1) encurta
  ainda mais o passo, mas o pé de apoio desliza na proporção (1 − valor): use 0,95 no mínimo.
- **Combate:** crie estados `Attack` (`Mocap_AttackPunch`/`Mocap_AttackSword`), `Cast`, `Block`, `Hit` e `Death`
  disparados por Triggers. Transições de saída com *Has Exit Time* ≈ 0,85 e duração 0,1–0,15 s.
  `Death` não tem transição de saída.
- **Layers:** para atacar andando, use uma layer "UpperBody" com Avatar Mask só do tronco e dos braços.
- **Espelho:** em clipes Humanoid, a opção *Mirror* do estado faz a versão esquerda/direita sem gerar outro clipe.

Por que Humanoid: os rigs da Meshy usam nomes diferentes entre si (`Spine02` como Spine etc.). O retarget por
músculos do Unity resolve isso, e um único conjunto de clipes serve para os heróis Humanoid. Para a locomoção,
onde cada grau aparece, o clipe é gravado em cada herói (§4).

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

## 7. BVH de outras ferramentas (mocap de vídeo, Mixamo…)

O importador só conhece os nomes de junta do CMU. `bvh_prep.py --joint-map` renomeia os outros esqueletos
(`tools/mocap/joint_maps.py`; testes em `tests/test_joint_maps.py`):

| `--joint-map` | Esqueleto | Coluna → CMU | Pernas → CMU |
|---|---|---|---|
| `cmu` (padrão) | CMU / cgspeed | — | — |
| `mixamo` | Mixamo, e DeepMotion/Plask quando exportam "Mixamo compatible" (prefixo `mixamorig:` é removido) | Spine/Spine1/Spine2 → LowerBack/Spine/Spine1 | UpLeg/Leg/Foot/ToeBase |
| `rokoko` | Rokoko Studio / Rokoko Video (perfil Newton) | Spine1/Spine2/Spine4 → LowerBack/Spine/Spine1 | Thigh/Shin/Foot/Toe |
| `humanoid` | nomes estilo Unity Humanoid | Spine/Chest/UpperChest | UpperLeg/LowerLeg/Foot/Toes |
| `meu_mapa.json` | qualquer outro (ex. FreeMoCap exportado pelo Blender) | `{"nome no arquivo": "nome CMU"}` | |

```powershell
.venv\Scripts\python.exe bvh_prep.py video.bvh --out out\Mocap_WalkVideo.bvh --joint-map mixamo `
    --unit-scale 0.01 --no-tpose --cycle --min-len 0.5 --max-len 1.4 --in-place --facing travel
```

- Os nomes exatos mudam com a versão e o perfil de exportação. Confira a `HIERARCHY` do arquivo; juntas que
  faltarem saem em `missing_joints` no `manifest.json`/saída e ficam paradas no Unity.
- `--no-tpose`: esses BVH não têm a T-pose extra no frame 0. Rotação zero precisa ser a pose de bind em T
  (é assim no Mixamo e no Rokoko). `--unit-scale 0.01` se o arquivo estiver em centímetros.
- Para entrar no conjunto, adicione uma entrada em `clips.json` com `joint_map`, `unit_scale` e
  `"tpose_frame0": false`.
- **Licenças, conferir antes de usar na build:** Rokoko Video, DeepMotion Animate 3D e Plask têm termos próprios
  por plano (uso comercial costuma depender do plano pago). FreeMoCap é AGPL-3.0 (o *software*; o movimento que
  você grava é seu, mas não embuta o código). Os modelos de pose por vídeo da §6 com SMPL/AMASS são não comerciais.
  Não há gravação do usuário prevista: hoje a fonte real de movimento é o CMU.
