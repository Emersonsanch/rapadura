# Rede e multiplayer: escolha da stack

> Item "Escolher a stack de rede" da Fase 12.2 do `TODO.md`. Pesquisa de **2026-11-09**, Unity do projeto **6000.3.22f1**.
> Status: **recomendação, ainda sem protótipo**. O item do TODO só fecha depois da etapa 7 do plano (§7).

## 1. Recomendação

| Camada | Escolha |
|---|---|
| Rede em tempo real | **Mirror** (MIT) com transporte KCP (UDP confiável) |
| Servidor de jogo | Build **Linux Dedicated Server** da própria Unity (`UNITY_SERVER`), autoritativo |
| Contas, guildas, chat, amigos, dados | **Nakama** auto-hospedado (Apache-2.0) |
| Interest management | `SpatialHashingInterestManagement` em XZ, `visRange = 96` (células de 48 m) |
| Modelo a imitar | Separação Login / Mundo / Mapa do **FishMMO** (MIT), adaptada para Mirror |

**Plano B:** FishNet + FishMMO, se o Mirror reprovar nos critérios da etapa 7.

Por que Mirror neste jogo:

1. **A grade dele coincide com a do mundo.** No Mirror a célula é `visRange / 2` e cada cliente enxerga a vizinhança 3×3 ([código](https://raw.githubusercontent.com/MirrorNetworking/Mirror/master/Assets/Mirror/Components/InterestManagement/SpatialHashing/SpatialHashingInterestManagement.cs)). Com `visRange = 96`, a célula tem exatamente os 48 m do nosso pedaço (`OverworldLayout.ChunkSize`), e 96 é o `OverworldRules.LoadRadius` que já usamos. A checagem só olha XZ, o que serve porque as regiões não se empilham em altura.
2. **Licença sem pegadinha.** MIT "sem restrições" ([README](https://raw.githubusercontent.com/MirrorNetworking/Mirror/master/README.md)). A do FishNet é própria: livre para jogos, mas o Pro tem teto de distribuição de 20 pessoas ([licença](https://raw.githubusercontent.com/FirstGearGames/FishNet/main/LICENSE.md)).
3. **Tem MMORPG lançado.** O README lista Inferna (mais de 1.000 CCU em várias instâncias), Another Dungeon e Empires Mobile (5.000 CCU de pico), Naïca e Laurum Online. São números declarados pelo próprio projeto, e ele mesmo descreve o alvo como "small MMORPGs". É o tamanho certo para começar, não um teto de MMO grande.
4. **O modelo MU não precisa do melhor recurso do FishNet.** Clique-para-andar com o servidor validando o caminho aceita interpolação e latência de 150–200 ms (`TODO.md` 12.2). No FishNet a compensação de lag é só do Pro ([docs](https://fish-networking.gitbook.io/docs/overview/readme/pro-projects-and-support)).
5. **Grátis e sem cobrança por jogador.** O custo fica em servidor e banda, que estão sob nosso controle.

O que pesa contra, sem disfarce:

- O README do Mirror cita Unity até **6000.1**. O suporte a Unity 6 entrou em [outubro de 2024](https://mirror-networking.gitbook.io/docs/manual/general/changelog/2024-change-log), mas não achei confirmação explícita para **6000.3**. A etapa 1 testa isso primeiro. O FishNet declara compilar limpo em 6.6.
- Mirror **não faz server meshing**: um mundo contínuo dividido em vários processos exige handoff feito por nós (§4).
- O benchmark que favorece o FishNet ([92,6% contra 16,8% a 200 CCU](https://fish-networking.gitbook.io/docs/overview/readme/features/performance/fish-networking-vs-mirror)) é do próprio FishNet, e a página admite que é antigo. Não decide, mas é motivo para medir na etapa 7.

## 2. Rede em tempo real

| | **Mirror** | **FishNet** | **NGO** | **Photon Fusion 2** |
|---|---|---|---|---|
| Versão | [v96.11.4](https://github.com/MirrorNetworking/Mirror/releases/latest) | [4.7.4](https://github.com/FirstGearGames/FishNet/releases/latest) | [3.2.0](https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/releases/latest) (doc já em 3.2.1) | Fusion 2 |
| Última release | 2026-11-07 | 2026-10-16 | 2026-11-03 | não verificado |
| Licença | MIT | Própria; royalty-free para jogos | Própria da Unity (GitHub não identifica SPDX) | Comercial, fechada |
| Unity 6 | Sim; 6000.3 **a confirmar** | Sim (até 6.6) | [Unity 6.0+](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@3.2/manual/index.html); [6000.3+ só aceita v2 ou superior](https://discussions.unity.com/t/netcode-for-gameobjects-1-x-deprecation-in-6000-3-editor/1674997) | não verificado |
| Servidor headless | Sim | Sim | Sim (build Dedicated Server da Unity) | Sim (Server Mode), mas conta CCU |
| Interest management | Pronto: spatial hash e hex ([docs](https://mirror-networking.gitbook.io/docs/manual/interest-management/spatial-hashing)) | Pronto: [GridCondition + HashGrid](https://fish-networking.gitbook.io/docs/fishnet-building-blocks/scriptableobjects/observerconditions/gridcondition) | **Manual**: `CheckObjectVisibility` e `NetworkShow` ([doc 2.7](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.7/manual/basics/object-visibility.html)); nada novo achado na 3.2 | Pronto, [grade 3D](https://doc.photonengine.com/fusion/current/manual/advanced/interest-management) |
| Jogadores por servidor | 400–800 CCU em benchmark de 2022; picos declarados de 1.000–5.000 em várias instâncias | Sem número oficial atual | Sem número oficial achado | [Amostra de 200 por sala](https://doc.photonengine.com/fusion/current/game-samples/br200/performance-optimizations) |
| Custo da biblioteca | Grátis | Grátis; [Pro US$ 50 por assento](https://assetstore.unity.com/packages/tools/network/fishnet-pro-networking-evolved-287711) | Grátis | [Por CCU](https://www.photonengine.com/fusion/pricing): 100 grátis, 500 = US$ 125/mês, 1.000 = US$ 250/mês, 2.000 = US$ 500/mês |
| Veredito | **Escolhido** | Plano B | Falta interest management pronto, e o foco recente é sessão com autoridade distribuída | Cobra por jogador mesmo com servidor próprio |

Sobre o Fusion: segundo a [equipe do Photon](https://forum.photonengine.com/discussion/19531/photon-fusion-architecture-modes-and-pricing), todos os modos exceto single-player contam CCU. O servidor dedicado é pago à parte, e a banda na América do Sul custa o dobro (US$ 0,10/GB). Para mundo persistente sempre ligado, é pagar duas vezes.

Sobre hospedagem gerenciada da Unity: o [Multiplay encerrou em 2026-03-31](https://discussions.unity.com/t/end-of-services/1698428), o que é mais um motivo para não amarrar o servidor a serviços da Unity.

## 3. Contas e dados

| | **Nakama** | **PlayFab** | **UGS** | **Backend próprio** |
|---|---|---|---|---|
| Licença | [Apache-2.0](https://github.com/heroiclabs/nakama), v3.42.0 (2026-11-03) | Serviço Microsoft | Serviço Unity | Nosso |
| Auto-hospedar (dados no Brasil, LGPD) | Sim | Não | Não | Sim |
| Guildas e chat prontos | [Sim](https://heroiclabs.com/docs/nakama/concepts/groups/) | Parcial | Parcial | Fazer tudo |
| Transação atômica (anti-duplicação) | [`multiUpdate`](https://heroiclabs.com/docs/nakama/server-framework/introduction/best-practices/index.html): storage + carteira | não verificado | não verificado | SQL direto |
| Custo | Grátis + servidor e banco; [Heroic Cloud sob consulta](https://toolradar.com/compare/nakama-vs-playfab) | [Grátis; pago a partir de US$ 99/mês](https://toolradar.com/compare/nakama-vs-playfab) (fonte de terceiros; o site oficial bloqueou a leitura) | [Medido](https://unity.com/products/gaming-services/pricing): Auth grátis; Cloud Save 5 GiB e 1 M escritas/mês grátis | Só infraestrutura e muito trabalho |

**Escolha: Nakama auto-hospedado.** Entrega login, grupos, chat, amigos e placares, que estão na Fase 12.3, sem cobrança por usuário e com os dados em servidor nosso. O servidor Mirror valida o token de sessão e fala com o Nakama servidor a servidor. O cliente nunca grava inventário.

**Atenção ao banco.** O Nakama [só apoia oficialmente CockroachDB](https://heroiclabs.com/docs/nakama/getting-started/install/linux/); PostgreSQL é "não oficial, para desenvolvimento". O CockroachDB deixou de ter edição Core e hoje é [grátis abaixo de US$ 10 M/ano de receita](https://sdtimes.com/os/cockroachdb-retires-self-hosted-core-offering-makes-enterprise-version-free-for-companies-under-10m-in-annual-revenue/), renovado anualmente. Usar PostgreSQL no protótipo e só decidir o banco de produção depois.

## 4. Como isso encaixa neste mundo

- **Tamanho.** 7200×15600 m em pedaços de 48 m dá 150×325 = **48.750 células**. Só as com jogador por perto têm custo, e o spatial hash funciona por célula ocupada.
- **Servidor constrói só colisão.** `OverworldView` já separa `BuildColliderMesh`/`BuildColliders` da malha visual, mas decide o que carregar em volta de um único herói. No servidor o conjunto vira a união dos pedaços em volta de **todos** os jogadores, sem malha visual.
- **Os ~100 mapas são zonas do mesmo mundo**, não cenas. Na fase 1, um processo roda o mundo inteiro (física só onde há gente).
- **Crescimento.** Quando um processo saturar, dividir por região (são 16). Passar pela divisa exige transferência de jogador entre processos, trabalho nosso e **o maior risco técnico**. O FishMMO só serve de modelo para Login/Mundo: os servidores de cena dele trocam de cena, o que não é contínuo.
- **Masmorras e salas de evento** viram instâncias em processo ou cena separada.
- **Precisão.** Em 15.600 m o `float` ainda resolve cerca de 1 mm. Não precisa de origem flutuante por causa da rede.
- **Banda 4G (estimativa minha, a medir).** Com 30 entidades visíveis, cerca de 10 bytes por entidade e 10 Hz, dá ~3 KB/s; em cidade cheia com 100 entidades, ~10 KB/s. Meta proposta: **até 15 KB/s por cliente em descida**. Se a cidade estourar, reduzir a frequência dos mais distantes.

**Regras prontas para o servidor.** Há 100 arquivos `*Rules.cs` em `Assets/Scripts`. Exemplos: `Combat/LevelDangerRules.cs` (C# puro, com sorteios injetados, o comentário já diz "o servidor roda o mesmo arquivo"), `World/Cities/SafeZoneRules.cs` e `World/Map/Mechanics/ThinIceRules.cs`, `RioHourRules.cs`, `MoonCanalRules.cs`. O acoplamento está nos *Drivers* e controladores em volta delas (§6). `OverworldRules` usa `Vector2` da Unity, o que é aceitável porque o servidor também é build Unity.

## 5. Projetos abertos para aproveitar ou imitar

| Projeto | Licença | Atividade | O que aproveitar |
|---|---|---|---|
| [FishMMO](https://github.com/jimdroberts/FishMMO) (FishNet) | MIT | 2026-11-09, 120 estrelas | **Melhor referência.** Login/World/Scene num só executável por argumento, PostgreSQL com EF Core, instalador de banco, patcher, monitor de processos. Imitar a arquitetura; é também a base do plano B |
| [UnityMultiplayerARPG_MMO](https://github.com/insthync/UnityMultiplayerARPG_MMO) | MIT | 2026-10-31, 500 estrelas | ARPG virando MMO: estudar. Não confirmei a arquitetura, a biblioteca de rede nem o que é pago na base |
| [Mirror, exemplos](https://github.com/MirrorNetworking/Mirror) | MIT | ativo | Exemplo Benchmark (mede interest management) e o de spatial hashing |
| uMMORPG (autor do Mirror) | Comercial | [loja](https://michmael.itch.io/ummorpg) | Pago, e a página encontrada descreve versão antiga. Só se o dono quiser comprar um modelo pronto |
| [Free-MMORPG](https://github.com/Assambra/Free-MMORPG) | não verificado | não verificado | Unity por sala + EzyFox/MongoDB. Outra arquitetura, baixa prioridade |

## 6. O que hoje assume um jogador só

Levantamento com busca em `Assets/Scripts`. Os totais são por arquivo.

| O quê | Onde | O que muda |
|---|---|---|
| Pausa global | `Core/Systems/GameplayPause.cs` (45 chamadas a `Acquire` no projeto) | Online nunca escreve `Time.timeScale`. Telas viram locais e só bloqueiam a entrada do jogador |
| Outros escritores de `timeScale` | `Combat/HitStopController.cs`, `Duo/DuoStrikeController.cs`, `Player/PlayerInputHandler.cs`, `UI/Death/DeathScreenController.cs`, `UI/CharacterSelect/CharacterSelectController.cs` | **Hit-stop vira efeito visual só do cliente**, senão congela o servidor inteiro. Morte não pausa |
| `MountController.Active` estático | `Mounts/MountController.cs:74`; lido em 7 arquivos (`EncumbranceTracker`, `MountHud`, drivers de MarGelado, Mingcheng, Ceu, Rio e Nartes) | Uma montaria por jogador, consultada pelo próprio jogador |
| Outros "atual" estáticos | `WorldMapMountRider.Current`, `CarameloCompanion.Instance`, `EncumbranceTracker.Current`, `DuoStrikeController.Active`, `MountController.SafeZoneProbe` | Por jogador |
| Diretores estáticos | `WorldEventDirector`, `DungeonDirector`, `EventRoomDirector`, `WorldMapSecretsDirector`, `MapModeController`, `CityModeController`, `WorldMapCityPresence` | Um por mundo ou instância; os de modo viram estado do cliente |
| `ServiceLocator` em gameplay | 110 arquivos em `Gameplay/` (`TimeOfDayManager` etc.) | Serviços globais são válidos para o mundo; os por jogador (inventário, stats) precisam ser resolvidos a partir do jogador |
| Busca do herói | `Overworld/OverworldView.cs:640` (`FindGameObjectWithTag`) e 13 outros arquivos | No servidor, iterar a lista de jogadores |
| Streaming e spawners centrados em um herói | `OverworldView.Update`, `UpdateSpawners` | União por jogador; bicho nasce uma vez para todos |
| Save local | `Save/SaveManager.cs` (JSON em `persistentDataPath`, `ISaveable`) | Banco no servidor. O formato `ISaveable` pode ser reaproveitado como serialização do personagem. Campanha offline é decisão do dono |
| `PlayerPrefs` com efeito em jogo | `DailyStaminaService`, `DailyLoginService` | O que dá recompensa vai para o servidor; preferência de tela fica |
| Relógio de jogo local | `Environment/TimeOfDayManager.cs`, salvo e parado na pausa | Um relógio do servidor, sem pausa, já previsto em `Docs/DayNight.md` §6 |
| **Dois relógios misturados** | UTC direto: drivers de `Rio`, `Nartes`, `Hinokura`, `CostaDeltaTeto`, `Ceu`. `TimeOfDayManager`: `Profundo`, `Mistura`, `Mingcheng`, `MarGelado`, `Farnkesh`, `Ceu` (usa os dois) | Criar uma fonte única injetável, como já faz `WorldEventCalendar.UtcNow`. UTC do cliente é manipulável e pode divergir |
| Sorteios no cliente | `Combat/OutgoingDamage.cs:24` (`UnityRandom`, já injetável); `UnityEngine.Random` em cerca de 8 arquivos de gameplay, em parte visual | Dano e saque só no servidor; visual fica. Revisar um a um (`HuntingProgress`, `CombatFxManager`, `EnemyPatrolState`) |
| Dia e estamina com relógio do aparelho | `Daily/DailyLoginService.cs`, `Daily/DailyStaminaService.cs` | Relógio do servidor |
| Encontros em arena | `EncounterArenaController` e fluxo do mapa | Mapas compartilhados ou instâncias |

## 7. Plano do protótipo mínimo

Meta: **dois jogadores andando no mesmo pedaço e um bicho com vida sincronizada**. Em branch separada, sem tocar no fluxo single-player, em uma cena e `asmdef` próprios. Não custa nada: roda em um PC.

1. **Compatibilidade.** Instalar Mirror em uma cópia do projeto em 6000.3.22f1. Passa se compilar, rodar o exemplo Benchmark e gerar build Linux Server. Falhou sem correção simples: ir ao plano B.
2. **Cena de teste.** `NetworkManager` + KCP, um pedaço de 48 m construído pelo `OverworldChunkBuilder`, câmera do jogo.
3. **Dois jogadores.** Prefab mínimo com `PlayerMotor`. Cliente envia a intenção (destino do clique) via `[Command]`; servidor valida distância e velocidade, move e replica. Passa com duas janelas se vendo andar.
4. **Bicho no servidor.** `EnemyController` e `Health` ativos só no servidor, com `SyncVar` de vida atual e máxima em um adaptador, sem alterar `Health`. Cliente vê posição interpolada e barra de vida.
5. **Ataque autoritativo.** Cliente envia "atacar alvo X"; servidor checa alcance e recarga, chama o cálculo existente (`LevelDangerRules` incluída) e aplica. Com dois clientes, os dois veem a mesma vida. Passa também se o cliente mentir o dano: o servidor ignora.
6. **Interest management.** `visRange = 96`, afastar um jogador por mais de 144 m: o outro e o bicho somem na tela dele e voltam ao retornar.
7. **Medir em build Linux Server com bots.** Reaproveitar o autopiloto do teste de 30 min, com 2, 50 e 200 clientes simulados, latência de 200 ms e 2% de perda.

**Critérios para confirmar o Mirror:** tick de servidor estável em 30 Hz com 200 bots no mesmo processo; até 15 KB/s por cliente em descida; sem teleporte visível a 200 ms. Se reprovar, repetir as etapas 3–7 em FishNet (assim o TODO ganha seu protótipo comparativo) e decidir pelos números.

Depois do protótipo, nesta ordem: login no Nakama, personagem no banco, relógio único, pausa local.

## 8. Depende do dono (custa dinheiro ou contas; só listado, não decidido)

Já abertas em `TODO.md` 12.1, e que mudam o dimensionamento:

- [ ] **Escala:** MMO persistente ou salas de 4–8 com cidade compartilhada. A stack serve aos dois; o custo, não.
- [ ] **Modelo de negócio:** pago, free-to-play cosmético ou passe.
- [ ] **Plataformas e cross-play** no primeiro lançamento (loja Google e Apple têm conta paga).
- [ ] **Single-player offline** continua separado do online?
- [ ] **Os 5 primos** como classes ou como NPCs.

Dinheiro e contas:

- [ ] **Hospedagem e orçamento mensal**, região Brasil. Preços para ordem de grandeza, de sites de terceiros, a confirmar: Vultr São Paulo [2 vCPU dedicadas e 4 GB por cerca de US$ 60/mês](https://valebyte.com/en/vps/brazil/?provider=vultr); servidor dedicado em São Paulo [a partir de US$ 150–200/mês](https://valebyte.com/en/blog/dedicated-server-rental-in-brazil-sao-paulo-prices-and-overview).
- [ ] **Banda.** Conta minha: 500 jogadores simultâneos a 8 KB/s, 24 h por dia, dariam ~10 TB/mês, um teto pessimista. Na AWS São Paulo, a [cerca de US$ 0,15/GB](https://costgoat.com/pricing/aws-data-transfer) (não confirmado na página oficial), seriam ~US$ 1.500/mês só de banda. Planos com banda incluída mudam muito essa conta.
- [ ] **Proteção DDoS.** Ataques UDP a servidores de jogo são um risco conhecido; confirmar o que cada fornecedor inclui e quanto custa.
- [ ] **Banco de produção** e seu custo (CockroachDB tem limite de receita para a licença grátis).
- [ ] **Login Google e Apple:** contas de desenvolvedor. Apple é paga.
- [ ] **LGPD:** termos, política de privacidade, canal de exclusão; pode exigir revisão jurídica.
- [ ] **Opcionais:** FishNet Pro (só no plano B), compra do uMMORPG como referência, ou uso do servidor próprio do WhaTicket para um teste real em 4G.

## 9. Fontes principais

**Bibliotecas:** [Mirror releases](https://github.com/MirrorNetworking/Mirror/releases/latest) · [Mirror README](https://raw.githubusercontent.com/MirrorNetworking/Mirror/master/README.md) · [Mirror spatial hashing](https://mirror-networking.gitbook.io/docs/manual/interest-management/spatial-hashing) · [FishNet releases](https://github.com/FirstGearGames/FishNet/releases/latest) · [licença do FishNet](https://raw.githubusercontent.com/FirstGearGames/FishNet/main/LICENSE.md) · [FishNet Pro](https://fish-networking.gitbook.io/docs/overview/readme/pro-projects-and-support) · [NGO releases](https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/releases/latest) · [preços do Fusion](https://www.photonengine.com/fusion/pricing) · [interest management do Fusion](https://doc.photonengine.com/fusion/current/manual/advanced/interest-management)

**Backend e servidor:** [Nakama](https://github.com/heroiclabs/nakama) · [instalação e bancos](https://heroiclabs.com/docs/nakama/getting-started/install/linux/) · [preços da UGS](https://unity.com/products/gaming-services/pricing) · [Unity Dedicated Server](https://docs.unity.com/en-us/engine/6000.3/manual/platform-specific/dedicated-server/build) · [Licenciamento do CockroachDB](https://www.cockroachlabs.com/docs/dev/licensing-faqs)

**Limites desta pesquisa:** comparativos de preço de PlayFab, VPS e banda vêm de sites de terceiros. Números de CCU são declarados pelos próprios projetos. Nenhum desempenho foi medido neste jogo ainda.
