# tools/mocap — BVH (CMU) → clipes Humanoid do Unity

Este pipeline transforma capturas de movimento reais do **CMU Motion Capture Database** (uso comercial
liberado, ver `cmu/SOURCES.md`) em `AnimationClip` **Humanoid** para os heróis do Rapadura.
O guia completo, com a lista de golpes e como ligar os clipes nos Animators, está em `Docs/Mocap.md`.

```
cmu/*.bvh (120 fps, unidades CMU, T-pose no frame 0)
   │  python bvh_prep.py --set clips.json --out-dir out
   ▼
out/*.bvh + out/manifest.json   (30 fps, metros, cortado/loopado, virado p/ +Z, pés no chão)
   │  Unity: Rapadura ▸ Animation ▸ Import CMU Mocap Set
   ▼
Assets/Animations/Mocap/Mocap_*.anim   (Humanoid: curvas de músculo + RootT/RootQ)
```

## Instalação (uma vez)

O venv é isolado e fixo em Python 3.11 (`.python-version`). Precisa do [uv](https://docs.astral.sh/uv/).

```powershell
cd tools\mocap
uv venv --python 3.11 .venv
uv pip install --python .venv\Scripts\python.exe -r requirements.txt
```

## Uso

```powershell
cd tools\mocap
# processa o conjunto inteiro descrito em clips.json -> out\*.bvh + out\manifest.json
.venv\Scripts\python.exe bvh_prep.py --set clips.json --out-dir out

# um clipe avulso (ex.: achar 1 ciclo de corrida, no lugar)
.venv\Scripts\python.exe bvh_prep.py cmu\09_02.bvh --out out\Mocap_Run.bvh --cycle --min-len 0.5 --max-len 0.85 --in-place --facing travel

# procurar/baixar novos trials do CMU
.venv\Scripts\python.exe fetch_cmu.py --search "kick"
.venv\Scripts\python.exe fetch_cmu.py 86_01

# testes
.venv\Scripts\python.exe -m pytest -q tests
```

### Campos de `clips.json`

| Campo | Significado |
|---|---|
| `name` | Nome do clipe (arquivo `.bvh` em `out/` e `.anim` no Unity) |
| `source` / `cmu` | Arquivo BVH bruto / id do trial CMU |
| `start`, `end` | Corte em segundos, no tempo do arquivo original (sem contar a T-pose) |
| `speed` | Multiplicador de velocidade aplicado antes de reamostrar (ex.: 1.6 = 60% mais rápido) |
| `cycle`, `min_len`, `max_len` | Procura a janela entre `min_len` e `max_len` segundos cujo último frame é o mais parecido possível com o primeiro (pose + velocidade da raiz + frame seguinte) |
| `loop` | Marca *Loop Time* no Unity e fecha o loop das curvas (o erro restante é distribuído ao longo do clipe) |
| `in_place` | Tira a deriva XZ início→fim da raiz (o balanço do corpo continua) |
| `facing` | `start` usa o quadril nos primeiros frames; `travel` usa a direção do deslocamento. Nos dois casos o clipe é virado para +Z |
| `root_motion` | Não "assa" a rotação/XZ no pose (ex.: virada de 90° usada com *Apply Root Motion*) |
| `per_hero` | O Unity grava o clipe também no Avatar de cada herói Humanoid (`Assets/Animations/Mocap/<Herói>/`). Usado na locomoção |
| `joint_map` | Nomes das juntas de origem: `cmu` (padrão), `mixamo`, `rokoko`, `humanoid` ou um JSON (ver `joint_maps.py` e `Docs/Mocap.md` §7) |
| `unit_scale`, `tpose_frame0` | Unidade → metros (padrão CMU 0,05644; cm = 0,01) e se o frame 0 é uma T-pose (falso para BVH que não vem do cgspeed) |

## Arquivos

| Arquivo | O que faz |
|---|---|
| `bvh.py` | Lê/grava BVH e faz FK (numpy/scipy) |
| `prep.py` | Escala para metros, reamostragem (slerp), corte, orientação para +Z, pés no chão e detecção de loop (`find_loop`) |
| `bvh_prep.py` | Linha de comando (um clipe ou o conjunto `--set`) |
| `joint_maps.py` | Renomeia esqueletos Mixamo/Rokoko/Humanoid/JSON para os nomes CMU (`--joint-map`) |
| `fetch_cmu.py` | Busca no índice e download de trials do espelho GitHub |
| `clips.json` | Conjunto curado de 18 clipes |
| `cmu/` | BVH brutos, `SOURCES.md` (trials, descrições, licença) e o índice CMU |
| `out/` | Saída processada, que é a entrada do importador Unity |
| `tests/test_prep.py` | pytest: loop em sinal sintético, reamostragem, orientação, ida e volta de escrita/leitura, escala do CMU |
| `tests/test_joint_maps.py` | pytest: mapas Mixamo/Rokoko, colisão de nomes, JSON próprio |

O importador Unity fica em `Assets/Scripts/Editor/Mocap/`. A matemática testável está em `MocapMath.cs`
(o retarget de um frame em `MocapRetargetRig.cs`),
e os testes EditMode em `Assets/Tests/EditMode/Mocap/`.
