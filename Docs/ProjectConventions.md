# Rapadura — Convenções de Projeto

> Documento vivo. Cobre três decisões da FASE 0 → "Infraestrutura de Projeto" do `TODO.md`
> que não dependem do Unity Editor: estratégia de branches, Company/Product Name, e
> estrutura de pastas definitiva. Não duplica `CONTRIBUTING.md` — só referencia e detalha
> o que lá está resumido.

---

## 1. Estratégia de branches

**Decisão: trunk-based simplificado.**

- `main` é sempre estável e buildável. Nunca fica em estado quebrado por mais que o
  tempo de um PR aberto.
- Trabalho novo entra em branches curtas de vida curta: `feature/<nome-curto>` ou
  `fix/<nome-curto>` (já descrito em `CONTRIBUTING.md`).
- Sem push direto na `main` — toda mudança entra via Pull Request, mesmo em modo solo
  (autorrevisão documentada é aceitável, ver Definition of Done em `CONTRIBUTING.md`).
- Sem branches de longa duração tipo `develop`, `release/*` ou `hotfix/*`. Não há
  necessidade de coordenar múltiplas versões em produção simultaneamente — o projeto
  ainda não lançou.

### Por quê, e não GitFlow

GitFlow (branches `develop`/`release`/`hotfix` separadas de `main`) resolve um problema
que este projeto não tem: múltiplas versões em produção precisando de patches
paralelos, e times grandes que precisam isolar trabalho por muito tempo antes de
integrar. Para um time pequeno (solo ou poucos devs) em fase de MVP:

- Menos branches de longa duração = menos merge conflicts acumulados = menos tempo
  gasto em reconciliação e mais em desenvolvimento.
- Integração contínua e frequente na `main` expõe problemas cedo (o próprio objetivo
  do trunk-based), o que importa mais agora do que isolar features "arriscadas".
  Ainda não há usuários em produção, então o custo de uma `main` momentaneamente
  instável é baixo (compensado por PR obrigatório + testes).
- Menos processo/overhead para times pequenos: não há necessidade de sincronizar
  `develop` com `main` a cada release quando ainda não existe cadência de release.

### Quando reconsiderar

Reavaliar para algo mais próximo de GitFlow (ou trunk-based com feature flags mais
formais) se/quando:
- A equipe crescer o suficiente para ter múltiplos PRs concorrentes de forma constante
  colidindo na `main`.
- Existirem builds em produção (loja) que precisam de hotfix isolado enquanto uma
  versão maior está em desenvolvimento (situação típica pós-lançamento — ver FASE 14
  do `TODO.md`).
- Precisar suportar múltiplas versões (ex: uma branch de patch para a versão da loja
  atual, outra para a próxima feature grande).

Até lá, trunk-based simplificado é a opção com menor custo de manutenção para o
tamanho e fase atual do projeto.

---

## 2. Company Name / Product Name

**Product Name:** `Rapadura` — já é o nome usado em todo o repositório
(`Docs/GDD.md`, `TODO.md`, namespaces `Rapadura.*`). Confirmado, sem necessidade de
alternativa.

**Company Name:** `Engenho Estúdio`

Justificativa: "engenho" remete diretamente ao processo de fabricação da própria
rapadura (o engenho de cana é onde ela é produzida), reforçando a identidade do nome
do jogo, e carrega o tom brasileiro/regional coerente com o universo de fantasia do
GDD sem soar como um nome de empresa genérico. Funciona como nome plausível de uma
software house pequena/indie, que é o contexto real do projeto.

### Aplicação

Esta é uma decisão de nomenclatura, não a configuração em si. A aplicação real
(Edit → Project Settings → Player → Company Name / Product Name, mais o ícone da
aplicação) exige abrir o projeto no Unity Editor — que é o bloqueio já documentado em
outros pontos do `TODO.md` (ex.: nota da FASE 1 sobre `ProjectSettings` vazio, sem
`ProjectVersion.txt`/`ProjectSettings.asset`). Quando o projeto for aberto pela
primeira vez no Editor, usar:

- **Company Name:** `Engenho Estúdio`
- **Product Name:** `Rapadura`
- **Ícone:** ainda não existe arte definida (depende da FASE 11 — Arte). Até lá, usar
  o ícone padrão do Unity é aceitável; não é um bloqueador para builds internas/teste.

---

## 3. Estrutura de pastas definitiva

Baseada na estrutura de `Assets/Scripts/*` já existente (que deve continuar sendo a
referência para organização de código) e estendida para os tipos de asset que ainda
não existem no projeto, mas vão aparecer a partir da FASE 11 (Arte/Animação) e da
FASE 10 (Addressables).

### Código (já em uso — manter como está)

```
Assets/Scripts/
  Core/           # infraestrutura genérica, reutilizável fora do domínio do jogo
                  # (managers, DI, EventBus, logging, pooling, localization, ...)
  Gameplay/       # regras e sistemas específicos do jogo (Combat, Inventory, Skills,
                  # Characters, Building, Quests, World, ...)
  UI/             # controllers de UI Toolkit (HUD, Menus, SkillTree, Dialogue, Common)
  Editor/         # ferramentas de editor (janelas, seeders, wizards) — nunca entra
                  # no build final
  Save/           # persistência (SaveManager, SaveData)
  Audio/          # (reservado — hoje áudio vive em Scripts/Core/Audio; consolidar
                  # aqui se o volume de código de áudio crescer)
  Animation/      # scripts relacionados a animação (não os assets de animação em si)
  Utilities/      # helpers genéricos sem estado de domínio
Assets/Tests/
  EditMode/       # testes que não exigem Play Mode (a maioria hoje)
  PlayMode/       # (criar quando houver testes que exigem o loop de Play Mode rodando)
```

Regra: namespace `Rapadura.<Camada>.<Subsistema>` sempre espelha o caminho de pasta
(ex.: `Assets/Scripts/Gameplay/Combat/` → `Rapadura.Gameplay.Combat`). Já documentado
em `CONTRIBUTING.md` — reforçado aqui porque a estrutura de pastas e a de namespaces
são a mesma decisão.

### Assets não-código (parte já existe, parte a criar conforme a necessidade surgir)

```
Assets/
  Art/                  # NOVO — consolida assets visuais brutos/fonte, separado do
                         # que é consumido em runtime. Subpastas por tipo:
    Art/Characters/     # arte/rig específico de cada um dos 5 personagens
    Art/Environment/    # cenário, props, biomas (Fase 8)
    Art/VFX/            # partículas e efeitos visuais (Fase 11)
    Art/UI/             # ícones, sprites de interface (fonte, não o UXML/USS)
  Textures/              # (já existe) — texturas finais prontas para uso em materiais
  Materials/             # (já existe) — materiais/shaders aplicados
  Models/                # (já existe) — meshes/rigs importados
  Animations/            # (já existe) — AnimatorControllers, clips, blend trees
  Audio/                 # NOVO — todo asset de áudio (hoje não existe pasta própria)
    Audio/Music/         # trilha/ambiente
    Audio/SFX/           # efeitos sonoros (combate, UI, passos, etc.)
    Audio/Voice/         # dublagem, se houver (Fase 11)
  Prefabs/               # (já existe) — prefabs prontos para uso em cena
  Scenes/                 # (já existe) — arquivos .unity
  Resources/              # (já existe) — usar com moderação; preferir Addressables
                           # para qualquer asset carregado dinamicamente e não
                           # referenciado direto por cena/prefab
  Settings/               # (já existe) — assets de configuração (Render Pipeline,
                           # Input Actions asset, etc.)
  InputActions/           # (já existe) — `.inputactions`
  UI/                     # (já existe) — UXML/USS por área (HUD, Menus, SkillTree,
                           # Dialogue, Common) — separado de Scripts/UI, que tem só o
                           # código C# dos controllers
  Addressables/           # NOVO — grupos/configuração de Addressables (criado quando
                           # o pacote `com.unity.addressables` for adicionado ao
                           # manifest, ver FASE 10 do TODO.md). Assets referenciados
                           # por endereço (não por referência direta de cena) vivem
                           # fisicamente em suas pastas normais (Art/, Audio/, etc.)
                           # e só são *marcados* como Addressable — não precisam ser
                           # movidos para dentro desta pasta.
  StreamingAssets/         # NOVO — apenas arquivos que precisam ser lidos como
                           # arquivo bruto em runtime (ex.: CSV de localização
                           # consumido por `LocalizationCsv.cs`, ver FASE 6 do
                           # TODO.md). Não usar para nada que possa ser um
                           # ScriptableObject ou Addressable — StreamingAssets não
                           # passa pelo pipeline de import/compressão do Unity.
```

### Regras gerais

1. **Código nunca mistura com assets de arte/áudio.** `Assets/Scripts/**` só contém
   `.cs` (e `.asmdef`). Nenhum `.png`/`.fbx`/`.wav` solto em pasta de script.
2. **`Art/` é a fonte, `Textures/`/`Models/`/`Materials/`/`Animations/` são o
   resultado processado** quando fizer sentido separar (ex.: um `.psd` de origem não
   precisa ir para o projeto Unity; o que entra em `Art/` é o asset já exportado no
   formato que o Unity importa, mas ainda "cru" — antes de virar prefab/material
   final). Na dúvida, preferir a pasta específica por tipo (`Textures/`, `Models/`)
   já existente a criar uma nova.
3. **`StreamingAssets/` é exceção, não regra.** Só usar quando o código
   explicitamente precisa ler o arquivo por caminho de sistema de arquivos em
   runtime (ex.: `Application.streamingAssetsPath`), como é o caso do CSV de
   localização. Todo o resto (texturas, modelos, áudio, dados de configuração)
   deve ser referência direta de asset ou Addressable.
4. **`Resources/` é legado a evitar crescer.** Já existe e tem uso hoje, mas todo
   asset novo que precisar de carregamento dinâmico deve ir para Addressables
   (a partir do momento em que o pacote for instalado, FASE 10) em vez de crescer
   `Resources/` — Addressables permite build incremental e fica dentro do
   orçamento de tamanho de build mobile citado na FASE 0 do `TODO.md`.
5. **Nomenclatura de arquivo de asset:** `snake_case` ou `PascalCase` consistente
   por tipo (ex.: prefabs em `PascalCase` como as classes que eles carregam,
   texturas em `snake_case` como `character_joaquim_diffuse.png`). Pipeline de
   import formal (naming convention completa, escala, pivot padronizado) fica para
   a FASE 11 quando houver artista/pipeline de arte real — aqui só fixamos onde
   cada tipo de arquivo mora.

---

*Ver `TODO.md` para o roadmap geral e `CONTRIBUTING.md` para processo de PR, commits e
Definition of Done.*
