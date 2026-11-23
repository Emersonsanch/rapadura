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

## 10. Grupo (party): regras puras prontas (2026-11-23)

Código em `Assets/Scripts/Gameplay/Party/`, C# puro (só `Vector3` da Unity) e sem rede. O servidor mantém um `PartyState` por grupo e chama as mesmas funções. O cliente só mostra o texto do motivo (`PartyTexts`). Testes: `PartyRulesTests`.

| Regra | Arquivo | Decisão |
|---|---|---|
| Convite, aceite, recusa e expiração; sair, expulsar, passar a liderança | `PartyRules` | Até 5 membros. Convite vale 60 s. Só o líder convida, expulsa, passa a liderança e escolhe o loot. Quando o líder sai, assume o membro mais antigo. Grupo com menos de 2 membros e sem convite pendente se desfaz. Nada lança exceção: o resultado traz um `PartyRefusal`. |
| XP do grupo | `PartyXpRules.Split` | Ganha quem está vivo, no mesmo mapa e a até **40 m** do abate. Pote = XP do abate × **1,0 / 1,2 / 1,5 / 1,8 / 2,2** (1 a 5 presentes), dividido **pelo nível** como no MU. Depois vem o corte de sempre por diferença de nível (`EnemyLevelScaling`), por membro. **Faixa de 20 níveis:** quem está mais de 20 acima do menor presente não ganha nada, e o nível dele continua na soma (a parte queima). Assim, carregar um personagem baixo não rende para ninguém. Um grupo de 1 dá exatamente a XP do single-player, que não muda. Os multiplicadores pessoais (fôlego, descansado, NG+, mestre) o servidor aplica depois, por jogador. |
| Loot do grupo | `PartyLootRules` | Os drops continuam sorteados uma vez por abate (`EnemyLootRoller`); a regra só escolhe o dono de cada um. **Livre**: qualquer presente pega. **Rodízio**: o próximo presente pela ordem de entrada, com cursor no `PartyState`. **Por necessidade**: Necessidade > Ganância, rolagem de 1 a 100 com seed do servidor + índice do drop; empate fica com o mais antigo; se todos passam, fica livre. **O líder distribui**: a escolha do líder se o escolhido estiver presente, senão o líder guarda; líder ausente cai no rodízio. Depois de 30 s qualquer presente pode pegar um drop com dono. |
| Bônus de família | `PartyFamilyRules` | O membro tem um "primo da classe" opcional, porque a decisão 12.1 segue aberta. **Família Completa** (os 5 primos presentes na luta): +10% de XP para todos. **Laço de Irmãos** (Maithe+Ícaro ou Joaquim+Maria presentes): +5% para os dois. Os bônus acumulam (no máximo +15,5%). |
| Sem fogo amigo | `PartyCombatRules.CanHarm` | `SafeZoneRules.CanAttack` e, além dele, jogador nunca fere membro do próprio grupo, nem em área de PvP. Para duelar, é preciso sair do grupo. |

Falta: mensagens de rede, HUD dos membros, chat de grupo, procurar grupo, reviver companheiro, entrar junto em chefe e evento, e o Golpe em dupla com um membro real.

**Reset:** a faixa de 20 níveis, o menor presente e o peso na soma usam o **nível efetivo** do resetado (`PartyKillMember.EffectiveLevel`, §15). A XP do próprio membro continua pelo nível atual.

## 11. Troca entre jogadores: regras puras (2026-11-23)

Base da troca da Fase 12.3, sem rede, em `Assets/Scripts/Gameplay/Trade/` (C# puro, só lê `ItemDefinition` e as regras de mochila; testes em `Assets/Tests/EditMode/TradeTests.cs`). O servidor guarda uma `TradeSession` por troca e passa nela as intenções dos dois jogadores com o relógio dele; nada no cliente decide.

| Peça | O que faz |
|---|---|
| `TradeRules` | Abrir: os dois na zona segura da **mesma cidade** (até 40 m) ou cara a cara (até 5 m) e, fora da cidade, sem luta nos últimos `SafeZoneRules.PlayerCombatTagSeconds` (10 s). Nada consigo mesmo, nem com quem já está em outra troca. Limites: 12 linhas de item e 1.000.000 de ouro por lado, ninguém acaba com mais de 2.000.000.000. A janela fecha após 120 s parada ou 600 s no total |
| `TradeSession` | Pôr e tirar itens (uma linha por entrada da mochila, o mesmo stack duas vezes é recusado) e ouro (só pelo campo de ouro; `item_gold` não entra como item). **Qualquer mudança desfaz as duas travas e as duas confirmações** e sobe a `Revision`. Travar nos dois lados dá o resumo (`BuildSummary`), e a confirmação final cita a revisão que viu (resumo velho é recusado). Cancelar e expirar fecham de vez |
| `TradeItemRules` | Não trocam: itens de história (`QuestItem` e `story_*`), relíquias da família (Patuá da Vó Edna, `relic_*`, `item_reliquia_*`), itens de conquista (`conquista_*`), montarias (apitos `item_whistle_*`), itens da casa (`casa_*`), outras moedas e tudo com `IsBound` (o mesmo campo que já protege da queda ao morrer). Nenhum asset mudou. Aviso de item raro: raridade Rara ou acima, +7 ou mais, opção excelente ou set antigo |
| `TradeSettlement` | Conclui tudo ou nada sobre cópias e devolve as mochilas novas para o servidor gravar numa transação só. Confere de novo cada linha pela "impressão" da entrada (id, durabilidade, afixos, refino, opções), as quantidades e o ouro, o limite de ouro, as contas novas, o espaço na grade (quem entrega libera espaço antes de receber) e o peso (não passa da carga, a não ser que fique mais leve). Se falhar, nada se move e a troca volta destravada. Concluir duas vezes é recusado |
| `TradeAccountProtection` | Conta nova = menos de 72 h **ou** herói abaixo do nível 30: por dia UTC, até 20.000 de ouro e 20 linhas de item recebidos, e o mesmo entregues. O servidor guarda o `TradeDailyTally` de cada conta |
| `TradeRecord` | Registro serializável (id da troca, os dois jogadores, hora UTC, itens e ouro de cada lado, impressão de cada item) com `ToLogLine()` para o log de economia |
| `TradeTexts` | Textos PT/EN de cada recusa e aviso e dos rótulos do resumo |

Falta (online): mensagens de rede para cada intenção, a janela de troca na UI, gravar o resultado e o registro no banco na mesma transação, e travar a mochila do jogador (vender, largar, usar) enquanto a troca está aberta, para reduzir as recusas na conclusão.

## 12. Guildas (clãs): regras puras prontas (2026-11-23)

Código em `Assets/Scripts/Gameplay/Guild/`, C# puro (sem Unity) e sem rede. O servidor mantém um `GuildState` por guilda e um `GuildWar` por guerra, e chama as mesmas funções. Nada lança exceção: toda ação devolve um `GuildRefusal`, e o cliente só mostra o texto (`GuildTexts`, PT/EN). O tempo é em segundos do servidor; os limites diários viram no dia UTC (`GuildRules.DayIndex`). Testes: `GuildRulesTests`.

| Regra | Arquivo | Decisão |
|---|---|---|
| Fundar | `GuildRules.Create` | Nível **30** e **5000 de ouro** (dreno de ouro; o servidor desconta `GoldCost`). Quem saiu de uma guilda espera **24 h** para fundar ou entrar em outra (contra pular de guilda em guerra ou banco). Lema de até 60 letras. |
| Nome e filtro | `GuildNameRules` | 3 a 16 letras depois de tirar espaços sobrando. Letras com ou sem acento (só o alfabeto latino, para letras parecidas não driblarem o filtro), espaços e até 3 números; começa com letra. É único pela `NameKey`, sem diferença de maiúsculas, acentos e espaços. O filtro baixa a caixa, tira os acentos e desfaz o leetspeak (0→o, 1→i, 3→e, 4→a, 5→s, 7→t, 8→b, @→a, $→s, !→i). Também junta letras repetidas ("merdaaa") e letras soltas ("p u t a", "f.u.c.k"). Há duas listas pequenas e extensíveis (`AddContainsWord`, `AddWholeWord`): palavras longas pegas em qualquer lugar e curtas só como palavra inteira, para "Escudo", "Espora", "Disputa" e "As Barcas" passarem. Nomes que imitam a equipe (GM, Admin, Moderador, Oficial…) são recusados. O mesmo filtro vale para lema, mural e mensagem do dia. |
| Brasão | `GuildEmblem` | Só dado, sem arte: forma (6), cor do campo, cor da borda, símbolo (13, do imaginário maniqueu: Sol, Lua, Barca de Luz, Coluna da Glória, Pérola, Árvore da Vida, Roda da Luz…) e cor do símbolo, numa paleta fixa de 12 cores. Válido quando tudo é valor definido, a borda difere do campo e o símbolo difere do campo. Vai pela rede como `ToCode()` ("0-0-1-1-1"), lido por `TryParse`. |
| Cargos e permissões | `GuildPermissions`, `GuildRules` | Recruta: deposita. Membro: + saca. Oficial: + convida, expulsa, mural. Vice: + promove, rebaixa, mensagem do dia, guerra. Líder: + lema/brasão e desfazer. Ninguém expulsa, promove ou rebaixa cargo igual ou acima do seu; a promoção para um cargo abaixo do de quem promove. Até **2 vices** e **8 oficiais**. Quem entra vem como Recruta; o convite vale 5 min. |
| Liderança | `GuildRules` | Transferir: só para quem não é Recruta; o antigo líder vira Vice (ou Oficial, se os vices estão cheios). Se o líder sai, assume o cargo mais alto e, no empate, o mais antigo. Se sai o último membro, a guilda acaba. Se o líder some **30 dias** (`CheckLeaderInactivity`, verificação diária), a liderança vai para o não-Recruta de cargo mais alto (e mais antigo) ativo nesses 30 dias, e o antigo líder vira Membro. Sem ninguém ativo, nada muda. Para desfazer a guilda, o banco tem de estar vazio e não pode haver guerra aberta. |
| Banco | `GuildBankRules` | Ouro (teto de 1 bilhão) e pilhas de itens (30 tipos no nível 1, +10 por nível). Itens vinculados não entram. Saque por dia: ouro 1000 / 5000 / 20000 (membro / oficial / vice), itens 20 / 50 / 200; Recruta não saca; líder sem limite, mas registrado. Cada movimento devolve um `GuildBankLogEntry` (hora, jogador, cargo na hora, ação, item, quantidade, saldo depois) para o log do servidor; a guilda guarda os últimos 200. |
| Contribuição e nível | `GuildBankRules.Contribute`, `GuildLevelRules` | Ouro doado entra no banco e dá 1 ponto a cada 10, até **200 pontos por membro por dia**. Depósito comum não conta. Outras fontes (missões semanais, eventos) entram por `AddContributionPoints`. Os níveis 1 a 10 começam em 0 / 1k / 3k / 7k / 15k / 30k / 55k / 90k / 140k / 200k pontos. Bônus baixos de propósito, sem pay-to-win: XP +nível/3 % (máx. **+3%**), ouro achado +nível/5 % (máx. **+2%**), 20 + 5 vagas de membro por nível (65 no 10) e mais espaço no banco. Nenhum atributo de combate e nada de drop raro. Os pontos ficam na guilda mesmo quando o membro sai. |
| Mural e mensagem do dia | `GuildBoardRules` | Aviso com até 300 letras, 20 avisos (o mais velho sai sozinho); mensagem do dia com até 120 letras, vazia apaga. Os dois passam pelo filtro. |
| Guerra | `GuildWarRules` | Declarar → aceitar → aquecimento de **10 min** → ativa → fim com placar. Quem decide é o líder ou um vice de cada lado. Termos: 1 a 72 h e meta opcional de até 500 abates, que encerra antes. Cada guilda precisa de 5 membros e tem no máximo 3 guerras abertas. A declaração vence em 24 h sem resposta. O mesmo par só volta a guerrear 24 h depois do fim. Cada abate vale 1 ponto, mas o mesmo matador sobre a mesma vítima só pontua de novo depois de 10 min. Rendição dá a vitória ao outro lado; empate no tempo fica sem vencedor. |
| "A pode ferir B?" | `GuildWarRules.CanHarm` | A guerra em curso é o consentimento, e só entre as duas guildas. Depois vem o `PartyCombatRules.CanHarm` (e com ele o `SafeZoneRules.CanAttack`): **nunca na cidade**, nunca em membro do próprio grupo (mesmo de guilda inimiga) e nunca em colega de guilda. Outro consentimento que o servidor já tenha (duelo, área PvP) passa sem mudança. |

Só anotado: **alianças** (até 3 guildas, sem fogo amigo entre aliadas, banco e mural separados, e uma guerra contra uma aliada puxa as outras se elas aceitarem) e **castelo por temporada** (cerco semanal entre as guildas mais altas no ranking de pontos, com bônus só cosmético e de imposto da cidade para a dona).

Falta (online): mensagens de rede, telas da guilda, chat da guilda e lista de online, gravar o banco e o inventário do jogador na mesma transação, missões semanais, sede (Fase 15), alianças e castelo.

## 13. PvP, arena ranqueada e ranking: regras puras prontas (2026-11-23)

Código em `Assets/Scripts/Gameplay/PvP/`, C# puro (só `Vector3` da Unity) e sem rede, no mesmo molde do Grupo e da Troca: o resultado traz um `PvpRefusal` e nada lança exceção. O servidor guarda um `DuelState` por duelo, um `PkState` e um `ArenaRating` por personagem (um por modalidade) e uma `ArenaQueue`. Tudo usa o relógio do servidor, e a fila usa a seed dele. Textos PT/EN em `PvpTexts`. Testes: `PvpRulesTests` (22).

| Regra | Arquivo | Decisão |
|---|---|---|
| Duelo consentido | `DuelRules` | Desafio fora da cidade, a até **10 m**, nunca com membro do próprio grupo (é preciso sair do grupo antes) e nunca com quem já está ocupado (outro duelo, arena) ou recusa duelos. O desafio vale **30 s**. Aceitar confere tudo de novo, fixa o centro da área no meio dos dois e começa uma contagem de **5 s**. A luta dura **180 s**. Sair da área (**25 m** do centro), mesmo durante a contagem, perde na hora. Desconectar conta como desistência. Um golpe nunca deixa a vida abaixo de 1, e quem chega a 1 perde. **Ninguém morre: o duelo não tira XP, item nem durabilidade e não deixa mancha.** No fim do tempo, vence quem tem a maior fração de vida; se forem iguais, empata. Se um monstro derrubar um dos duelistas, o duelo é interrompido sem vencedor. Vale em todas as regiões, até nas iniciais, porque é consentido. |
| "Pode ferir?" entre jogadores | `PvpCombatRules` | Uma base por golpe, na ordem: **Arena** (só o outro time da mesma partida), **Duelo** (selado na contagem e na luta: os dois só ferem um ao outro, e só na luta; ninguém de fora fere, cura ou fortalece um duelista), **Guerra de guilda** (fora da cidade e fora das regiões protegidas) e **Área aberta**. Depois vem `PartyCombatRules.CanHarmPlayer`, ou seja, nada na cidade e nada em membro do próprio grupo. A frente de Guildas passa `atGuildWar` = `GuildWarRules.AreAtWar(...)`. |
| Áreas de PvP aberto | `PvpAreaRules` | **Protegidas**: Regiões 1–4 (Zarafshan, Areias Flamejantes, Serra de Kaggen, Grutas do Limiar), o primeiro ato inteiro (até ~nível 50). Ali nunca há PvP aberto. **Fronteira**: Regiões 5–13, 15 e 16, com PvP aberto só se **os dois** tiverem nível 80 ou mais, então quem está de passagem não é alvo nem caçador. Matar ali deixa mancha. **Terras de Disputa**: Região 14, Margens da Mistura (bíblia §3.11), com PvP aberto a partir do nível 80 e sem mancha. Mapa desconhecido conta como protegido. |
| Fora da lei (PK) | `PkRules` | Matar na Fronteira deixa **1 mancha** no matador. Não deixa mancha quem age em legítima defesa (a vítima bateu primeiro nos últimos 60 s), quem mata alguém já Fora da lei e quem mata em duelo, arena, guerra ou Terras de Disputa. **1 mancha**: nome laranja e mais nada. **2–4 (Fora da lei)**: nome vermelho, loja +20%, XP perdida ao morrer ×1,5. **5+ (Assassino)**: loja +40%, XP perdida ×2 e sem arena ranqueada. O teto é de 10 manchas. Cada mancha some com **1 h jogada online**, e uma nova mancha zera o relógio. A **fiança** custa 20.000 de ouro por mancha e é paga numa cidade. Nunca cai mais item, nada é perdido de vez e a conta não é bloqueada. |
| Arena ranqueada | `ArenaRules`, `ArenaQueue` | Elo inteiro e separado por modalidade (1x1, 2x2, 3x3), com nível mínimo 30. Na equipe, todos precisam estar no mesmo grupo. Todo mundo começa em 1500. As **5 partidas de colocação** usam K 64. Depois, K 32 abaixo de 1800, 24 abaixo de 2100 e 16 acima. O piso é 100. Na equipe, o esperado vem das médias e cada membro usa o próprio K. A fila tem faixa de **±100**, que cresce **25 a cada 10 s** de espera até ±500. Dois times se enfrentam quando a diferença cabe na maior das duas faixas. Quem espera há mais tempo escolhe primeiro e pega o rating mais próximo. O lado e o campo (3 arenas) saem da seed do servidor. **Temporada**: rating → 1500 + (rating − 1500) × 0,5, nova colocação e o recorde zera. A recompensa sai pelo recorde da temporada, com no mínimo 10 partidas: Centelha (< 1400), Chama (1400), Lâmpada (1600), Estrela (1800), Coluna da Glória (2000+). **Só título e cosmético** (`cosmetic_arena_s{temporada}_{modalidade}_{faixa}`), nunca atributo. |
| Ranking | `RankingRules` | Três quadros: **Nível** (nível, depois XP total), **Arena** (rating, vitórias e menos partidas; só quem terminou a colocação) e **Eventos** (pontos). A ordem é total e estável: pontuação, depois quem chegou primeiro (`ReachedAt`), depois o id. O empate de pontuação divide o número ("1, 2, 2, 4"). Arena e eventos zeram com a temporada, e o nível nunca zera. |

Ponto para alinhar com Guildas: `GuildWarRules.CanHarm` aceita guerra em qualquer lugar fora da cidade, mas `PvpCombatRules` só a aceita fora das Regiões 1–4. Fica a decisão de qual dos dois manda (sugestão: não guerrear nas regiões iniciais, como pede "nada de PvP forçado").

Falta (online): mensagens de rede (desafio, contagem, fim, fila, resultado), HUD do duelo com o círculo da área, nome colorido pela mancha, aplicar o multiplicador da loja e da XP de morte onde o servidor já calcula, o registro do "bateu primeiro" para a legítima defesa, a instância da arena e os quadros do ranking na UI.

**Reset:** o mínimo e a fila da arena e o gate do PvP aberto (nível 80 nos dois) usam o **nível efetivo** (`ArenaQueueMember.EffectiveLevel`, `PvpCombatant.EffectiveLevel`; §15), e não o nível atual de quem fez reset.

## 14. Eventos agendados: regras puras prontas (2026-11-23)

Base dos eventos com horário da Fase 12.3, no espírito de Blood Castle e Devil Square, sem rede. Código em `Assets/Scripts/Gameplay/TimedEvents/`, C# puro (sem Unity). Nada lança exceção: a entrada devolve um `TimedEventRefusal`, e o cliente só mostra o texto (`TimedEventTexts`, PT/EN). Tudo é função do instante UTC que o servidor passa, no fuso fixo de Brasília do `WorldEventCalendar`; nada lê o relógio do sistema. Os sorteios usam `TimedEventAgenda.Seed` (FNV-1a do evento, ocorrência, sala e jogador, o mesmo do `RewardSeed`) e `Roll` (SplitMix64), então o servidor refaz qualquer conta. Testes: `TimedEventsTests` (33).

Nomes novos (tema maniqueu, no lugar de "Noite do Lobisomem", "Cerco ao Engenho", "Caçada ao Habitante Ancestral" e "Festa Junina"):

- **Vigília da Coluna** (sobreviver a ondas): a Coluna da Glória puxa a luz do dia para a Lua, e a Treva manda 8 ondas para apagar as lamparinas.
- **Cerco da Treva** (defesa): a Treva vem pela cidade atrás da **Lâmpada do Portão**.
- **Caçada a Ohyah** (chefe mundial com várias pessoas): Ohyah, o gigante antigo do *Livro dos Gigantes*, irmão de Hahyah.
- **Bema** (sazonal): o mesmo junho do Bema offline (`SeasonCalendar`).

| Regra | Arquivo | Decisão |
|---|---|---|
| Agenda | `TimedEventCatalog`, `TimedEventAgenda` | Horários de Brasília. **Vigília** às 1h, 5h, 9h, 13h, 17h e 21h (no Bema, mais uma às 23h), com aviso 15 min antes, portão 5 min antes e 15 min de luta. **Cerco** às 8h30, 14h30 e 20h30, com aviso 15 min, portão 5 min e 20 min de luta. **Caçada** às 11h e 22h, com aviso 30 min, portão 10 min e 30 min de luta. Do portão ao fim, nenhum evento cruza outro (teste). `StatusAt` dá a fase (Fechado → Anunciado → Portão aberto → Acontecendo) e os segundos até a próxima mudança. `Next` dá a ocorrência atual ou a próxima, e `NextNotYetOpen`, a próxima que ainda não abriu. A chave da ocorrência é dia local × 32 + horário do dia. |
| Salas por nível | `TimedEventRoomBand` | Vigília e Cerco têm 7 salas como as do Blood Castle: 15–79, 80–149, 150–219, 220–289, 290–359, 360–429 e 430–500. A Caçada tem 3 forças de Ohyah: 100–249, 250–399 e 400–500. Abaixo da primeira sala, a recusa é `LevelTooLow`. |
| Ingresso | `TimedEventRules.Cost` | A Vigília pede 1 **Vela da Vigília** ou ouro, que custa de 2.000 (sala 1) a 120.000 (sala 7). A Caçada pede 1 **Marca do Caçador** ou de 25.000 a 120.000 de ouro. O Cerco é de graça, porque defender a cidade é dever. O ingresso sai do jogo e nunca é vendido por dinheiro real. O servidor tira o que vem em `Payments` na mesma transação. |
| Tamanho da sala | `TimedEventRoomSet` | Cabem 10 (Vigília), 20 (Cerco) ou 40 (Caçada). Sala cheia abre outra instância da mesma faixa, até **20 instâncias por faixa**; depois disso, a recusa é `RoomFull`. |
| Entrada por dia | `TimedEventEntryLedger` | **1 entrada por evento por personagem por dia local**. No Bema, a Vigília tem 2. Quem cai volta para a própria sala de graça (`TryRejoin`) enquanto o evento não acaba, sem gastar entrada. |
| Entrar com o grupo | `TimedEventRules.TryEnterParty` | Só o líder leva o grupo (`PartyState`). Entram os membros que estão no portão; os outros ficam. Ou entram todos ou ninguém: se um não pode entrar, o resultado diz quem e por quê (`FailedPlayerId`, `MemberRefusal`), e ninguém paga nem gasta entrada. Todos precisam cair na mesma faixa (`PartyRoomMismatch`). O grupo vai para a mesma instância e nunca é dividido. |
| Ondas | `TimedWaveRules`, `TimedVigiliaRules` | A onda tem um orçamento de (6 + 2 × onda) × (1 + 0,35 × jogador extra), gasto com seed em papéis, não em assets: Sombra 1, Corredor 1, Sussurrador 2, Bruto 3 e Arauto 6. Os papéis entram por onda: o Corredor a partir da 2ª, o Sussurrador da 3ª (com peso dobrado nas salas 4+), o Bruto da 4ª e o Arauto da 6ª. A 8ª onda traz o **Arconte da Vigília**. O nível é o meio da faixa − 6, mais 2 por onda, até 500. A próxima onda vem com ≤ 2 vivos ou depois de 100 s. Vitória: limpar a 8ª onda ou chegar ao fim com alguém de pé. Derrota: todos caídos. |
| Defesa | `TimedCercoRules`, `TimedLampState` | São 6 assaltos por 3 portões. O orçamento é (10 + 4 × assalto) × (1 + 0,35 × jogador extra), e o último traz o **Arconte do Cerco**. A Lâmpada tem 4.000 de vida, +60% por sala acima da 1ª e +15% por jogador extra. Cada golpe na lâmpada tira uma fração da vida máxima: Sombra 1%, Corredor 0,75%, Sussurrador 1,5%, Bruto 3%, Arauto 6% e Arconte 12%. **Reacender** devolve 4%, uma vez a cada 20 s por jogador, sem passar do máximo; lâmpada apagada não volta. **Derrota:** a lâmpada apaga, ou fica **45 s sem ninguém de pé** (portão abandonado). **Vitória:** limpar o 6º assalto ou chegar aos 20 min com a lâmpada acesa. Com ≥ 75% no fim, a sala ganha o bônus "Lâmpada intacta". |
| Chefe com várias pessoas | `TimedCacadaRules` | Ohyah tem 60.000 de vida por caçador (conta no mínimo 5), ×1,8 por sala acima da 1ª. A contribuição soma **dano ×1, cura efetiva ×1,2 e dano tankado ×0,8**; o servidor conta tudo pelo próprio log, sem cura que passou do máximo e sem dano além da vida do alvo. **Parado não ganha nada:** quem tem menos de 45 s de ações, menos de 30% do tempo na arena com alguma ação ou contribuição zero fica de fora. **Piso:** todo caçador ativo ganha o prêmio de participação, mesmo se Ohyah escapar. Com Ohyah vencido, caem 3 itens (Baú de Ohyah) + 1 a cada 5 ativos, até 10. É no máximo 1 por pessoa, sorteado pela parte de cada um, com peso mínimo de 2% e seed. Quem mais causou dano, quem mais curou e quem mais tankou ganham cada um uma **Marca de Ohyah**, um troféu sem atributo. |
| Placar | `TimedEventScoring`, `TimedScoreSheet` | Pontos por abate: Sombra e Corredor 5, Sussurrador 10, Bruto 15, Arauto 40 e Arconte 150. Também valem onda limpa 20, reacender 15, **reviver aliado 25** (apoio conta), assistência 2, Lâmpada intacta 60 e vitória 50. Morte tira 20, e o total nunca fica abaixo de 0. Na Caçada vale 1 ponto por 0,1% da contribuição, +50 por Marca. O ranking da sala segue pontos, depois menos mortes, depois quem entrou antes, depois o id. Empate de pontos e mortes divide a posição ("1, 2, 2, 4"), e quem ficou parado vai para o fim sem faixa. Os pontos entram no quadro **Eventos** do `RankingRules` (§13). |
| Faixas e recompensas | `TimedEventRewards` | **Ouro:** top 10%, e o 1º sempre. **Prata:** top 35%. **Bronze:** o resto com ≥ 60 pontos. **Participação:** ativo, mas abaixo disso. Base: 40 abates-comuns de XP da sala e 60 de ouro × sala, multiplicados por 1 / 0,75 / 0,5 / 0,25. As Pedras-Faísca são 3 / 2 / 1 / 0, e a Pedra da Alma Viva vai para o Ouro da sala 3+. Há ainda um material do evento: Cinza da Vigília, Óleo da Lâmpada ou Fragmento de Ohyah. Sala perdida paga metade de XP e ouro, sem pedras e sem cosmético. O prêmio raro é **só cosmético** (título, aura ou visual, ids `cosm_*`), com chance de 3% no Ouro, 1% na Prata e 0,3% no Bronze. **Sem pay-to-win:** nada se compra com dinheiro real, e nenhum prêmio exclusivo tem atributo. |
| Bema sazonal | `TimedBemaRules` | Vale o **junho inteiro pela data de Brasília**, como o `SeasonCalendar`. **Trégua do Bema:** o Cerco da Treva não vem no mês, e a entrada é recusada com `SuspendedForBema`. **Vigília do Bema:** uma Vigília a mais às 23h e 2 entradas por dia. Todo prêmio de evento dá XP ×1,1 e oferendas (6 / 4 / 2 / 1 por faixa, as mesmas `Prendas` do Bema offline). A **Lamparina do Bema** (cosmético) entra na tabela de todos os eventos. No **Grande Bema** (30 de junho), a chance de cosmético dobra. O visual fica para depois. |

Falta (online): mensagens de rede (aviso, portão, entrada do grupo, placar e resultado), as instâncias de verdade no servidor, ligar os papéis aos bichos de cada região e os ids de item e cosmético aos assets, gravar o pagamento e o prêmio na mesma transação, a tela da agenda e do placar e o visual do Bema online. As salas offline (`EventRoomRules`: Pátio dos Mazans e Torre da Roda) continuam como estão.

**Reset:** a sala é escolhida pelo **nível efetivo** (`TimedEventEntrant.EffectiveLevel`, §15). Um resetado no nível 1 com o equipamento do 400 entra na sala do 400, sozinho ou no grupo.

## 15. Reset estilo MU ("Volta da Luz"): regras puras prontas (2026-11-23)

Código em `Assets/Scripts/Gameplay/Rebirth/`, C# puro e sem rede, no molde do Grupo, da Troca e do PvP: o servidor guarda um `RebirthState` por personagem (resets do ciclo, grandes resets, nível do último reset, hora do último) e monta um `RebirthCandidate` com os dados dele. O resultado traz um `RebirthRefusal`, e nada lança exceção. Textos PT/EN em `RebirthTexts`. Testes: `RebirthRulesTests`.

**NG+ e reset.** A Nova Jornada+ (`NewGamePlusRules`/`NewGamePlusProgress`) zera o **mundo** do save (história, chefes, missões de região) e escala os bichos pelo ciclo do save. Num mundo compartilhado as duas coisas são impossíveis por jogador. Por isso, no online, o reset por personagem **substitui** a NG+ (item 12.4). No offline a NG+ continua igual, e nada nela mudou (`RebirthRules.UsesNewGamePlus(online)`).

| Regra | Arquivo | Decisão |
|---|---|---|
| Requisitos | `RebirthRules.Check`, `NextRequirement` | Reset n do ciclo (1–10): nível **400 + 10·(n−1)** (400 … 490). Ouro do e-ésimo reset no total: **20.000 + 2.000·(e−1)**, com teto de 100.000 (≈ 20–100 h de caça a ~1.000 de ouro/h, Balanco100a500 §5.1). Só junto do Cristal, na zona segura da **Vila da Vó Edna**. Vivo, sem nenhuma mancha de PK (paga a fiança ou espera sumir) e fora de troca, duelo, arena e fila. |
| Grande Reset ("Subida da Coluna") | `RebirthRules.TryGrandReset` | Depois dos 10 resets do ciclo: nível **500**, **200.000 + 50.000·g** de ouro, no **Cais da Coluna**, 7 dias depois do último reset. Os resets voltam a 0, o grande sobe 1 e o nível vai a 1. Os pontos bônus ficam (a contagem efetiva não muda), e o próximo ciclo recomeça no nível 400. Teto: **5 grandes** e 10 resets depois do último, o que dá **60 resets efetivos**. |
| Contra abuso | `RebirthRules.Check` | **24 h** entre resets. A confirmação cita a contagem que o resumo mostrou (`ExpectedEffectiveResets`): clique duplo, replay ou duas abas dão `StaleRequest`. A ordem das checagens é fixa, e quem recusa não muda nada. O ouro sai na mesma transação que o nível. |
| O que zera e o que fica | `RebirthRules.Cleared` / `Kept` (`RebirthPart`) | **Zera:** nível (vai a 1), XP, pontos de atributo, pontos de skill e de talento (redistribuição grátis; os pontos voltam com os níveis). **Fica:** itens, equipamento e Baú; o ouro (menos o preço); missões feitas e história (não são oferecidas de novo, para ninguém farmar XP de missão resetando); Caderno e Lembranças; conquistas, montarias, casa e guilda; graduações (G4/G5 voltam a valer no 150/350); Nível de Mestre (volta a valer no 500); frutas de crescimento já comidas; rating da arena. |
| Pontos bônus | `BonusPointsFor`, `TotalBonusPoints`, `UnlockedBonusPoints`, `BonusPointsToGrant` | Curva decrescente: resets 1–5 dão **20**, 6–10 dão **10**, 11–20 dão **5**, 21–40 dão **3** e 41–60 dão **2**. São 150 no 1º ciclo e **300** no teto, contra os 998 que o herói 500 ganha subindo (Progressao500 §2). Os pontos **acordam com o nível**: ⌊total × min(nível, 400) / 400⌋. Assim, o herói recém-resetado no nível 1 é um herói de nível 1, e o servidor entrega a diferença a cada nível. |
| Eco da Luz | `ScaleKillExperience` | Abaixo do nível do último reset, a XP de **abate** é **×4**. Isso faz a volta ao 400 levar ~120 h de caça, em vez de ~460 h. Vale só para abate, e não para missão. Para no nível antigo, então nunca leva ninguém além de onde já estava. O grupo continua não carregando um resetado: a parte dele é dividida pelo nível atual sobre a soma dos níveis efetivos (`PartyXpRules`, ver o nível efetivo abaixo). |
| Balanço | `RebirthBalance` | Herói de referência: vida 100 + 8·(L−1), `damage_mult` 1 + 0,02·(L−1), 2 pontos por nível divididos meio a meio entre Poder Arcano (+1% de dano) e Vitalidade (+10 de vida) (`AttributeSet`). Poder = √(dano × vida). Equipamento e skills ficam de fora de propósito, porque iguais nos dois lados só aumentariam a vantagem aparente. **Faixa testada** em todo nível de 1 a 500: 1 reset < +3% (máx. +1,7%), 1 ciclo ≤ +15% (máx. +12,6%), teto de 60 ≤ **+30%** (máx. +25,2% no 400, +20,7% no 500). Mesmo no teto, o resetado não alcança um herói **75 níveis** acima sem reset. |
| Marcos (sem pay-to-win) | `RebirthRules.Milestones` | Só título e cosmético, nunca atributo e nunca à venda. Faísca que Voltou (1), Lamparina Acesa (3), Lua Crescente (5), Lua Cheia (10), Passageiro da Barca do Sol (25), Décima Segunda Jarra (40), O Caminho de Casa (60). Em cada grande reset, um degrau: Primeiro … Quarto Degrau da Coluna e Alto da Coluna, com aura que sobe. Ids: `title_rebirth_*` / `cosmetic_rebirth_*`. |
| Tema e falas | `RebirthTexts` | A luz juntada na estrada é devolvida à Barca da Lua (bíblia §1.5), e o herói recomeça "levinho", com um pouco dela guardada. Falam a Vó Edna (oferta junto do Cristal; depois do grande, pelo rádio, lembrando o Vô Jorge e a cadeira de balanço) e cada um dos 5 primos, com os tiques da bíblia §4.4. Não há palavras religiosas (§0). |
| Ranking | `RankingRules` (por acréscimo) | `RankingEntry` ganhou `Resets` e `GrandResets` (opcionais, 0 por padrão). O quadro **Nível** ordena por grandes, depois resets, nível e XP, como no MU. Sem resets, a ordem é a de antes, e `PvpRulesTests` não mudou. |

**Nível efetivo (decidido em 2026-11-23).** Os itens continuam **sem nível mínimo** (`ItemDefinition`), porque um requisito mexeria em todo o balanço offline. Um resetado no nível 1 segue com o equipamento do 400, e por isso tudo o que filtra jogadores por nível no online lê o **nível efetivo**: `RebirthState.EffectiveLevel(nível)` = **max(nível atual, `PeakLevel`)** para quem já fez reset, e o próprio nível para quem nunca fez (`RebirthRules.EffectiveLevel(state, nível)` aceita `state` nulo). `PeakLevel` já guardava o maior nível alcançado antes de um reset e nunca desce, nem no Grande Reset, que também mantém o equipamento. Por isso não precisou de campo novo. Quem lê: a fila e o mínimo da arena (`ArenaQueueMember.EffectiveLevel`, §13), o gate do PvP aberto nos dois lados (`PvpCombatant.EffectiveLevel`, §13), a sala dos eventos, sozinho ou em grupo (`TimedEventEntrant.EffectiveLevel`, §14), e a faixa de 20 níveis do grupo (`PartyKillMember.EffectiveLevel`, §10). No grupo, o nível efetivo vale para a faixa, para o menor presente e para o peso na soma dos níveis. A parte do próprio membro (o numerador) e o corte por diferença de nível continuam pelo nível atual. Assim, um resetado que carrega um nível 10 queima como o 400 que é (o 10 fica com 10/410, não 10/11), e os amigos 400 dele já não são cortados pela faixa por causa do nível 1. Cada um é um parâmetro opcional `effectiveLevel` no construtor, que vale o próprio nível quando omitido e nunca fica abaixo dele. Quem não fez reset continua exatamente como antes. Testes: `EffectiveLevelTests`. XP, atributos e o Eco da Luz seguem pelo nível atual.

Falta (online): mensagens de rede (pedir o resumo, confirmar, resultado), a janela do Cristal na UI, gravar o reset na mesma transação (ouro, nível, pontos, marcos), o servidor chamar `BonusPointsToGrant` no level-up e `ScaleKillExperience` no abate (junto dos outros multiplicadores pessoais), e a arte dos 12 cosméticos.

## 16. Chat e moderação: regras puras prontas (2026-11-23)

Base do chat e da moderação da Fase 12.3, sem rede, em `Assets/Scripts/Gameplay/Social/` (C# puro, sem exceções; testes em `Assets/Tests/EditMode/SocialRulesTests.cs`). O servidor chama `ChatRules.TrySend` a cada mensagem com o relógio dele (segundos UTC) e um `ChatContext` preenchido pelos registros dele (grupo, guilda, quem está online e em que região, listas de bloqueio); o cliente só manda o texto e o alvo do sussurro, e só mostra o texto do motivo (`SocialTexts`).

| Canal | Quem recebe (`ChatRouting.Recipients`) | Tamanho | Anti-flood (rajada, depois 1 a cada) |
|---|---|---|---|
| Geral | Todos online na **mesma região** do remetente (o mundo é contínuo, então "o mapa" não tem borda visível). Só a partir do nível 5, contra contas novas de robô | 120 | 3, depois 6 s (10 por minuto) |
| Grupo | Membros online do `PartyState` do remetente | 200 | 6, depois 2 s |
| Guilda | Membros online do `GuildState`, recrutas inclusos | 200 | 5, depois 3 s |
| Sussurro | Só o alvo, se estiver online e não tiver bloqueado o remetente | 200 | 5, depois 2 s |
| Sistema | Só o servidor escreve (`ChatRules.SystemMessage`), para todos ou para uma região; sem limite, sem filtro, sem bloqueio | 300 | — |

| Peça | O que faz |
|---|---|
| `ChatRules` + `ChatSpamState` | Ordem: silêncio da equipe → silêncio automático → texto (limpo de espaços e caracteres invisíveis) → acesso ao canal → repetição → token bucket por canal. **Repetição:** a mesma mensagem (sem diferença de maiúscula, acento, leetspeak ou "!!!") no mesmo canal em 30 s é recusada e não gasta ficha. **Silêncio automático:** cada recusa por flood ou repetição é uma falta; 4 faltas em 60 s silenciam todos os canais por 1 min, depois 5 min, 15 min e 1 h; uma hora limpa depois do fim do silêncio volta para 1 min. O remetente sempre recebe o eco. A `ChatMessage` tem `text` (mascarado, vai para os outros) e `raw` (como foi digitado, fica só no servidor) e `ToLogLine()` |
| `ChatFilter` | **Mascara com `*`, nunca recusa**, e mantém o tamanho. Usa as listas do `GuildNameRules` (as mesmas do nome, lema, mural e mensagem do dia da guilda; palavra que o servidor acrescenta lá vale no chat também) e mais alguns insultos só do chat, extensíveis. Mesma normalização da guilda (acentos, leetspeak, letras repetidas, "p u t a"), mas a palavra "contida" é procurada **dentro de uma palavra só**, nunca juntando duas: "o pé do filho" não vira "pedofilho". O "!" só vale como "i" no meio da palavra. Conferido sem nenhum falso positivo nos 25.700 textos PT/EN do jogo (`ContentTexts`, `CityTexts` e as demais tabelas) |
| `ChatEmotes` | 8 emotes, uma roda no celular: acenar, obrigado, rir, comemorar, socorro, siga-me, esperem e "Que a Luz te guarde". Passam pelas mesmas regras de canal e anti-flood. A mensagem leva só o id, e cada cliente mostra a fala no idioma dele e, se o remetente joga com um primo, na voz desse primo (Maithe, Ícaro, Joaquim, Maria ou Lavine) |
| `PlayerBlockList` + `PlayerBlockRules` | Até 100 bloqueados, num sentido só. Quem eu bloqueei não me sussurra, some das minhas mensagens de geral, grupo e guilda, e não me convida para grupo, guilda, troca ou duelo. **Quem foi bloqueado nunca fica sabendo:** cada regra responde com o motivo de "jogador indisponível" que já tinha. Ligado por parâmetro opcional no fim (null = comportamento de antes) em `PartyRules.Invite`, `GuildRules.Invite`, `TradeRules.CanOpen` e `TradeSession.TryOpen`. O duelo já tinha o `targetBlocksDuels`, e o servidor passa `PlayerBlockRules.TargetRefusesDuel` |
| `ReportRules` | Motivos: spam, linguagem ofensiva, assédio, trapaça, robô, golpe, nome impróprio e outro (este pede comentário). Prova: as últimas 10 mensagens do denunciado que o denunciante recebeu, mais os sussurros entre os dois, tiradas do log do servidor e nunca enviadas pelo cliente. Até 5 denúncias por dia UTC e 1 por jogador por dia (`ReportDailyTally`). `PlayerReport.ToLogLine()`. A denúncia sozinha não pune ninguém: vai para a equipe |
| `GmRules` | Comandos validados por cargo. **Moderador:** silenciar (1 min a 24 h), tirar o silêncio, expulsar, ler o log de chat e de denúncias. **GM:** tudo isso, silenciar até 7 dias, banir (1 h a 30 dias, ou para sempre), tirar o banimento, teleporte (ir até o jogador, trazer o jogador, mandar para um ponto de um mapa) e ler todos os logs (chat, denúncias, trocas, moderação). Ninguém age sobre si mesmo nem sobre cargo igual ou maior. Silenciar, expulsar e banir pedem motivo. Uma consulta de log cobre até 7 dias e 500 linhas. `TryIssue` sempre devolve o `GmCommandRecord`, também das tentativas recusadas (é assim que aparece uma conta da equipe roubada), com `expiresAtUnix` e `ToLogLine()`. A execução fica com o servidor |
| `SocialTexts` | Textos PT/EN dos canais, de todas as recusas (chat, bloqueio, denúncia, GM), dos motivos de denúncia e dos emotes |

Falta (online): mensagens de rede do chat e das intenções de bloqueio, denúncia e comando, guardar `ChatSpamState`, listas de bloqueio, contagem de denúncias e silêncios/banimentos por conta, a janela de chat e a roda de emotes na UI, a fila de denúncias e o painel da equipe, e executar no servidor os comandos aceitos.
