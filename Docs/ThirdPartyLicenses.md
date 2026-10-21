# Rapadura — Licenças de Terceiros

> Inventário de tudo que não foi escrito do zero pelo projeto: pacotes (`Packages/manifest.json` + dependências transitivas em `Packages/packages-lock.json`), ferramentas usadas no pipeline e origem de cada asset em `Assets/`. Atualizar sempre que um pacote ou asset entrar/sair. A tela de Créditos (`Assets/Scripts/UI/Menus/CreditsContent.cs`) mostra um resumo desta lista — manter os dois em sincronia.
>
> ⚠️ **Não é parecer jurídico.** Os termos abaixo são os conhecidos na data de revisão (2026-10-21). Itens marcados **[VERIFICAR]** precisam de confirmação do usuário antes da submissão às lojas.

## 1. Pacotes Unity (diretos, `Packages/manifest.json`)

| Pacote | Versão | Licença | Vai na build? |
|---|---|---|---|
| `com.unity.render-pipelines.universal` (URP) | 17.0.3 | Unity Companion License | Sim |
| `com.unity.inputsystem` | 1.11.2 | Unity Companion License | Sim |
| `com.unity.addressables` | 2.2.2 | Unity Companion License | Sim |
| `com.unity.textmeshpro` | 3.0.9 | Unity Companion License | Sim |
| `com.unity.ugui` | 2.0.0 | Unity Companion License | Sim |
| `com.unity.cloud.gltfast` | 6.19.0 | **Apache License 2.0** (exige manter aviso de copyright/NOTICE) | Sim, se algum `.glb` for carregado em runtime; hoje os `.glb` são importados no Editor |
| `com.unity.modules.*` (ai, animation, audio, particlesystem, physics, uielements) | 1.0.0 | Módulos built-in do Unity (cobertos pela licença do Editor/Runtime) | Sim |
| `com.coplaydev.unity-mcp` (MCP for Unity, git `#main`) | main | MIT (repositório CoplayDev/unity-mcp) **[VERIFICAR]** | **Não deveria** — ferramenta de desenvolvimento. Confirmar que nenhum código runtime do pacote entra na build de release (ou removê-lo do manifest no branch de release) e fixar um commit/tag em vez de `#main` |

## 2. Dependências transitivas relevantes (`packages-lock.json`)

| Pacote | Licença | Observação |
|---|---|---|
| `com.unity.render-pipelines.core`, `com.unity.shadergraph`, `com.unity.render-pipelines.universal-config` | Unity Companion License | Trazidos pelo URP |
| `com.unity.burst`, `com.unity.collections`, `com.unity.mathematics` | Unity Companion License | Burst inclui componentes LLVM (Apache 2.0 com exceção LLVM) — avisos já distribuídos pela Unity |
| `com.unity.scriptablebuildpipeline`, `com.unity.profiling.core` | Unity Companion License | Addressables |
| `com.unity.nuget.newtonsoft-json` | MIT (Json.NET, James Newton-King) | Incluir aviso MIT se for para a build |
| `com.unity.nuget.mono-cecil` | MIT | Somente Editor |
| `com.unity.test-framework`, `com.unity.ext.nunit`, `com.unity.test-framework.performance` | Unity Companion License / NUnit: MIT | Somente Editor/testes |
| `com.unity.searcher` | Unity Companion License | Somente Editor |

**Unity Companion License:** permite uso desses pacotes em projetos feitos com Unity sob a licença do Editor; não exige crédito na tela, mas a licença do Editor (Personal/Pro) precisa estar válida para o faturamento do estúdio. Splash screen "Made with Unity" segue as regras do plano Unity em uso **[VERIFICAR plano: Personal/Pro]**.

## 3. Modelos 3D e animações

| Asset | Origem | Termos conhecidos |
|---|---|---|
| `Assets/Models/Characters/{Maithe,Maria,Joaquim,Icaro,Lavine}_RO/*.fbx` e `glb/*.glb` (heróis estilo chibi) | Gerados por IA via **Meshy** (texto→3D) | Termos de Serviço da Meshy: assets gerados em plano **pago** são do usuário e liberados para uso comercial; em plano gratuito a licença é CC BY 4.0 (exige atribuição) e o uso comercial é limitado. **[VERIFICAR qual plano estava ativo em cada geração]** |
| `Assets/Models/Characters/Joaquim/Joaquim.fbx`, `Joaquim_Child/Joaquim_Child.fbx` | Meshy (versões anteriores do Joaquim) | Idem acima. Se não forem mais usados, remover antes do release |
| `Assets/Models/Characters/*_RO/Rigged/*_Rigged*.fbx`, `*_Walk*.fbx`, `*_Run*.fbx` (rig humanoide + animações Walk/Run) | **Meshy Rigging API** (Maria, Joaquim, Ícaro, Lavine). Maithe ainda sem rig | Mesmos termos da Meshy (o rig/animação é output gerado pela conta do usuário) **[VERIFICAR]** |
| Texturas embutidas extraídas dos FBX da Meshy (`Assets/Models/**/Textures/*`) | Meshy | Idem |
| `Assets/Models/Enemies/{Wolf,Scorpion,Spider}` | Meshy (texto→3D, sem rig) | Idem |
| `Assets/Models/Environment/Trees/Tree_{Broadleaf,Pine,Gnarled}_RO.glb` (+ LODs decimados em `Assets/Resources/CleanedMeshes`) | Meshy (texto→3D), decimados pelo `MeshDecimator.cs` do projeto | Idem |
| Rochas, chão, detalhes de vegetação | Geometria procedural do próprio projeto (`ProceduralRockGenerator.cs`, `GroundDetailScatter.cs`) | Próprio |
| **Mixamo (Adobe)** | **Não usado até agora.** Foi considerado no `TODO.md` como alternativa de auto-rig, mas os rigs atuais vieram da Meshy. Se for usado no futuro: animações e personagens Mixamo são livres de royalties para uso comercial em jogos (termos Adobe), mas não podem ser redistribuídos como assets avulsos | — |

## 4. Texturas, fontes e áudio

| Asset | Origem | Termos |
|---|---|---|
| `Assets/Textures/Environment/Tex_{Grass,Grass_Normal,Bark,Leaves,Rock,Wall}.png` | Geradas proceduralmente (ruído) pelo projeto — `ProceduralGroundTexture.cs` e `manage_texture` do MCP for Unity | Próprio |
| `Assets/Textures/Characters/Tex_HeroCloth.png` | Provavelmente procedural (mesma leva do MCP) **[VERIFICAR origem]** | — |
| Fontes | Nenhum arquivo `.ttf/.otf` no projeto. A UI usa a fonte padrão do tema runtime do UI Toolkit (`UnityDefaultRuntimeTheme.tss`) e TextMesh Pro sem os TMP Essentials importados | Fontes padrão distribuídas com a Unity. **Se uma fonte customizada entrar, registrar aqui (ex.: Google Fonts = SIL OFL 1.1)** |
| Áudio (música/SFX) | **Nenhum arquivo de áudio no projeto ainda.** `AudioManager`/`AudioCueDatabase` existem sem clipes | Registrar a origem/licença de cada clipe quando forem adicionados |

## 5. Ferramentas do pipeline (não vão na build)

| Ferramenta | Uso | Licença |
|---|---|---|
| Unity 6 (6000.3) | Engine/Editor | Licença Unity (plano do estúdio) |
| Meshy | Geração de modelos 3D e rigs | Termos de Serviço Meshy (ver seção 3) |
| FBX2glTF (Meta/Facebook) | Conversão FBX→GLB antes do rig | BSD 3-Clause |
| MCP for Unity (Coplay) | Automação do Editor | MIT **[VERIFICAR]** |
| Claude Code (Anthropic) | Assistência no desenvolvimento | Termos Anthropic (código gerado pertence ao usuário) |
| GitHub Actions | CI | Termos GitHub |

## 6. Pendências para o usuário antes do release

1. **Plano da Meshy** em cada geração (pago = uso comercial sem atribuição; gratuito = CC BY 4.0 com restrições). Se algum asset foi gerado no plano gratuito, regenerar em plano pago ou manter atribuição nos Créditos.
2. **`com.coplaydev.unity-mcp`**: garantir que não entra na build de release; fixar versão.
3. **Plano da Unity** (Personal/Pro) e regras da splash screen.
4. Origem de `Tex_HeroCloth.png`.
5. Remover modelos não usados (`Joaquim/`, `Joaquim_Child/`, `*_Rigged.fbx` sem sufixo `_1` se forem duplicatas) para reduzir tamanho e superfície de licença.
6. Gerar o arquivo de avisos (NOTICE/licenças MIT + Apache 2.0) acessível no app ou na página da loja, se glTFast/Newtonsoft forem para a build.
