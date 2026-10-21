# CI (GitHub Actions)

Workflow: `.github/workflows/ci.yml` (+ ação local `.github/actions/lfs-pull/action.yml`).
Usa [game-ci](https://game.ci) com Unity **6000.3.22f1** (o mesmo de `ProjectSettings/ProjectVersion.txt`; se atualizar o Editor, atualize também `UNITY_VERSION` no topo do workflow).

## 1. Quando roda

| Evento | precheck | Testes EditMode/PlayMode | Builds Windows/Android |
|---|---|---|---|
| Push em `master` | sim | sim | sim (se os testes passarem) |
| Pull request para `master` | sim | sim | não |
| Manual (aba **Actions → CI → Run workflow**) | sim | sim | sim (se os testes passarem) |

- **Concorrência:** um novo push no mesmo branch/PR cancela a execução anterior ainda em andamento.
- **Sem licença configurada:** o job `precheck` termina verde com o aviso *"Unity CI desativado"* no resumo da execução, e os demais jobs aparecem como **pulados** (cinza), não como falha.

## 2. O que cada job faz

| Job | O que faz | Timeout |
|---|---|---|
| `precheck` | Verifica se os secrets de licença existem (secrets não podem ser usados direto no `if:` de um job, por isso há esse job intermediário). | 5 min |
| `test` (matriz `EditMode`, `PlayMode`) | Checkout + Git LFS (com cache de `.git/lfs`), restaura o cache de `Library/` (chave = hash de `Assets/`, `Packages/`, `ProjectSettings/`), roda `game-ci/unity-test-runner@v4`, publica um *check* "Resultados EditMode/PlayMode" no commit e envia os XML de resultado como artefato `test-results-<modo>` (14 dias). | 60 min |
| `build` (matriz `StandaloneWindows64`, `Android`) | Mesmo checkout/LFS/cache, roda `game-ci/unity-builder@v4` com **Development Build** (`-Development`). Android gera **APK** (`androidExportType: androidPackage`). Antes do Android, libera ~20 GB de disco no runner (`jlumbroso/free-disk-space`). Artefatos `Rapadura-StandaloneWindows64` e `Rapadura-Android` (14 dias). | 90 / 120 min |

Observações:
- O build de Windows é feito em Linux (imagem `unityci/editor ...-windows-mono`), portanto usa o backend **Mono**. IL2CPP para Windows exigiria um runner `windows-latest`.
- O unity-builder aplica versionamento semântico a partir das tags git (`v0.1.0` etc.; sem tags vira `0.0.<nº de commits>`) e define o `versionCode` do Android. Isso altera o `PlayerSettings` apenas dentro do container de CI, nada é commitado.
- O builder padrão do game-ci também executa o build de conteúdo do **Addressables** automaticamente, pois o pacote está instalado.
- Os testes PlayMode (`Assets/Tests/PlayMode`) carregam cenas reais em um Editor sem monitor (Linux). Se algum smoke test depender de GPU/janela, ele pode se comportar diferente do Editor local; veja os XML no artefato.

## 3. Secrets obrigatórios (licença Unity)

Cadastre em **GitHub → repositório → Settings → Secrets and variables → Actions → New repository secret**:

| Secret | Licença Personal (gratuita) | Licença Plus/Pro |
|---|---|---|
| `UNITY_LICENSE` | **sim**: conteúdo completo do arquivo `.ulf` | não |
| `UNITY_EMAIL` | **sim**: e-mail da conta Unity | **sim** |
| `UNITY_PASSWORD` | **sim**: senha da conta Unity | **sim** |
| `UNITY_SERIAL` | não | **sim**: serial da licença |

> A conta Unity não pode usar login só por Google/Apple/Facebook nem ter 2FA por app autenticador nesse fluxo; se for o caso, defina uma senha na conta (id.unity.com) e use 2FA por e-mail ou desative-a para essa conta.

### Como obter o `UNITY_LICENSE` (Personal), passo a passo

A Unity desativou a ativação manual por arquivo `.alf` para licenças Personal; o caminho atual recomendado pelo game-ci é reaproveitar o `.ulf` gerado pelo Unity Hub:

1. Abra o **Unity Hub** neste PC e entre com a conta Unity que será usada no CI.
2. Vá em **Preferences (engrenagem) → Licenses → Add → Get a free personal license** (se já existir uma licença Personal ativa, pule este passo).
3. Localize o arquivo de licença:
   - Windows: `C:\ProgramData\Unity\Unity_lic.ulf`
   - macOS: `/Library/Application Support/Unity/Unity_lic.ulf`
   - Linux: `~/.local/share/unity3d/Unity/Unity_lic.ulf`
4. Abra o arquivo em um editor de texto e copie **todo** o conteúdo (é um XML que começa com `<?xml` e termina com `</root>`).
5. No GitHub, crie o secret `UNITY_LICENSE` colando esse conteúdo inteiro.
6. Crie também `UNITY_EMAIL` e `UNITY_PASSWORD`.
7. Em **Actions → CI → Run workflow**, rode manualmente e confirme que `precheck` não mostra mais o aviso e que os testes rodam.

Se o CI começar a falhar com erro de ativação de licença depois de meses, a licença Personal foi renovada: repita os passos 2 a 5. Mantenha o `.ulf` só no secret, **nunca** o commite.

Referência: https://game.ci/docs/github/activation

## 4. Secrets opcionais (assinatura Android)

Sem eles, o APK sai assinado com a **chave de debug** (instala por sideload; não serve para a Play Store). Para assinar com a chave de release:

| Secret | Conteúdo |
|---|---|
| `ANDROID_KEYSTORE_BASE64` | arquivo `.keystore` em base64 |
| `ANDROID_KEYSTORE_PASS` | senha do keystore |
| `ANDROID_KEYALIAS_NAME` | nome do alias |
| `ANDROID_KEYALIAS_PASS` | senha do alias |

Gerar o base64 no Windows (PowerShell):

```powershell
[Convert]::ToBase64String([IO.File]::ReadAllBytes("C:\caminho\rapadura.keystore")) | Set-Clipboard
```

O workflow só ativa o keystore customizado quando `ANDROID_KEYSTORE_BASE64` existe. Guarde o keystore e as senhas fora do repositório (perder o keystore impede atualizar o app na Play Store). Para publicar na loja será preciso também AAB (`androidExportType: androidAppBundle`) e build não-development; isso fica para quando houver pipeline de release.

## 5. Pacote `com.coplaydev.unity-mcp` (MCP for Unity)

- O `Packages/manifest.json` aponta para `https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#main`. O repositório é público, o Editor do container resolve via git e usa o commit travado em `Packages/packages-lock.json` (`hash 4ce7dd3c...`). Ou seja, **o CI funciona sem ajuste**, desde que o `packages-lock.json` continue commitado.
- Atenção: o pacote tem um assembly `MCPForUnity.Runtime` (sem restrição de plataforma, `autoReferenced`), portanto **ele entra nos builds** de Windows e Android. São helpers pequenos, licença MIT, mas é código de ferramenta de desenvolvimento.
- **Decisão do usuário** (registrada em `Docs/ThirdPartyLicenses.md`): para builds de release, remover o pacote do manifest num branch de release ou fixar um commit/tag em vez de `#main`. Para os builds de desenvolvimento do CI, pode ficar como está.

## 6. Rodar localmente

**Testes pelo Editor:** `Window → General → Test Runner` → abas EditMode / PlayMode → *Run All*.

**Testes pela linha de comando** (com o Editor do projeto fechado):

```powershell
$unity = "C:\Program Files\Unity\Hub\Editor\6000.3.22f1\Editor\Unity.exe"
& $unity -batchmode -projectPath "D:\Projetos\rapadura" -runTests -testPlatform EditMode -testResults "$env:TEMP\editmode.xml" -logFile "$env:TEMP\editmode.log"
& $unity -batchmode -projectPath "D:\Projetos\rapadura" -runTests -testPlatform PlayMode -testResults "$env:TEMP\playmode.xml" -logFile "$env:TEMP\playmode.log"
```

(Não use `-quit` junto com `-runTests`; o Unity fecha sozinho ao terminar. Código de saída 0 = tudo passou.)

**Builds locais:** `File → Build Profiles` → Windows ou Android → marque *Development Build* → *Build*.

**Workflow inteiro localmente:** é possível com [`act`](https://github.com/nektos/act) + Docker passando os secrets por `--secret-file`, mas as imagens Unity têm vários GB; normalmente é mais prático usar o *Run workflow* manual no GitHub.

## 7. Durações esperadas (runner `ubuntu-latest`)

| Job | 1ª execução (sem cache) | Com cache de `Library/` |
|---|---|---|
| precheck | < 1 min | < 1 min |
| Testes EditMode | 15–30 min (download da imagem Unity + importação de todos os assets/LFS) | 5–10 min |
| Testes PlayMode | 15–30 min | 6–12 min |
| Build Windows | 25–45 min | 10–20 min |
| Build Android | 35–60 min | 15–30 min |

Testes rodam em paralelo; builds rodam em paralelo após os testes. Um push em `master` costuma levar **~30–40 min** com cache e mais de 1 h sem cache. Em repositório privado, isso consome minutos do plano gratuito do GitHub (2.000 min/mês); em repositório público é ilimitado. O cache do GitHub é limitado a 10 GB por repositório: caches antigos de `Library/` e LFS são descartados automaticamente.
