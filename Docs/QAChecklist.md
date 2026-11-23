# Rapadura — Checklist de Regressão (pré-release)

> Rodar antes de **toda** build marcada como release (tag `vX.Y.Z`, ver `CHANGELOG.md`). Marcar cada item em pelo menos **1 Android de faixa mínima** (ver `Docs/PlatformRequirements.md`), **1 iOS** quando houver build iOS e **1 PC**. Registrar aparelho, versão (`BuildVersion.Current` aparece nos Créditos) e resultado no PR/issue da release. Qualquer item vermelho em "Bloqueante" impede a publicação.

## 0. Build e automação (Bloqueante)
- [ ] Projeto abre sem erros no Console (Unity 6000.3, sem `error CS`).
- [ ] Suíte EditMode inteira verde (Test Runner) e CI do GitHub Actions verde no commit da release.
- [ ] Teste de resistência verde no commit da release (`SoakPlaythroughTests`, categoria `Soak`, 30 min de jogo; ver `Docs/CI.md` §6) e `Logs/SoakReport.txt` anexado ao PR/issue da release.
- [ ] `PlayerSettings.bundleVersion` = versão do `CHANGELOG.md`; `bundleVersionCode` (Android) / `buildNumber` (iOS) incrementados.
- [ ] Seção `[Não lançado]` do CHANGELOG movida para a versão nova com data.
- [ ] `Docs/ThirdPartyLicenses.md` revisado (nenhum asset/pacote novo sem origem); pacote `com.coplaydev.unity-mcp` fora da build de release.
- [ ] Build de desenvolvimento desligada (`Development Build` off) — o rótulo nos Créditos não pode mostrar "(dev)".
- [ ] Tamanho do APK/AAB/IPA dentro do orçamento (ver "Orçamento de performance").

## 1. Inicialização e menus
- [ ] Primeiro boot sem save: menu principal aparece, "Continuar" desabilitado, foco inicial em "Novo Jogo".
- [ ] Navegação completa só com gamepad/teclado (sem mouse) em todos os menus; indicador de foco visível.
- [ ] Botão "Créditos": abre a tela, rola o texto, mostra versão correta, "Voltar"/Esc/B/voltar do Android fecham e o foco volta ao botão "Créditos".
- [ ] Configurações: qualidade gráfica, tela cheia (PC), volumes (Master/Música/SFX/UI) e idioma persistem após fechar e reabrir o jogo.
- [ ] Troca de idioma pt-BR ⇄ en atualiza menus, HUD, tooltips, diálogos e quests sem chaves cruas (`ui.xxx`) aparecendo.
- [ ] "Sair" fecha o app (PC) / volta ao sistema (mobile) sem travar.
- [ ] Título: fim de tarde no engenho, logo dourado sobre a placa, botões temáticos e entrada suave (instantânea com "Reduzir Movimento").
- [ ] "Continuar" carrega o save **mais recente** entre auto-save e slots 0–2 (testar com o slot 2 mais novo que o auto-save; slot corrompido é ignorado). "Carregar" fica desabilitado sem save manual e abre Salvar/Carregar em modo carregamento, navegável por controle/teclado.

## 2. Seleção de personagem
- [ ] Os 5 heróis (Maithe, Maria, Joaquim, Ícaro, Lavine) aparecem com preview girando e cor/nome corretos.
- [ ] Herói escolhido entra no jogo com o modelo, passiva e conjunto de skills dele; troca de personagem jogável funciona.
- [ ] Heróis com rig (Maria, Joaquim, Ícaro, Lavine) animam Idle/Walk/Run; Maithe usa a animação procedural sem erros.
- [ ] Cada herói mostra cor, inicial, papel, idade, laço de família e as 5 habilidades certas; trocar rápido entre heróis nunca deixa o preview 3D vazio.

## 3. Movimento, câmera e controles
- [ ] Movimento por teclado, gamepad, joystick de toque e toque/clique-para-mover (anel aparece e some).
- [ ] Botões de toque Correr e Pular respondem; nada da HUD fica sob notch/barra de gestos (safe area).
- [ ] Zoom por pinça e roda do mouse dentro dos limites; câmera não atravessa rochas/paredes.
- [ ] Remapeamento de controles (se exposto) persiste.
- [ ] Movimento com peso por herói: Joaquim pesado, Maria calma, Maithe leve, Ícaro inquieto, Lavine nervosa. Idle respira e troca o apoio; curva fechada freia antes do pivô; pulo agacha, estica no ar e amassa na aterrissagem; pés seguem o chão em rampa (IK só nos Humanoid; Maria e Maithe sem erro).
- [ ] Inimigos: arrancam/freiam com ease, antecipam o golpe com squash e acertam no quadro do dano (janela de esquiva igual), recuam ao apanhar, quicam na morte, surgem com overshoot.
- [ ] "Reduzir Movimento" (Acessibilidade): metade do squash/balanço de heróis e inimigos e do tremor de câmera, entradas instantâneas no título, história, banner de chefe e dicas; persiste ao reabrir. "Reduzir Flashes" não muda mais a animação dos heróis.

## 4. Combate
- [ ] Ataque básico e clicar/tocar no inimigo para atacar causam dano, números flutuantes e shake (respeitando a intensidade de acessibilidade).
- [ ] Skills 1–5 via teclado, gamepad e hotbar de toque: custo de mana/energia, cooldown visível, efeitos (buff/debuff/DoT Queimadura) aplicados.
- [ ] Combo de exemplo completa dentro da janela de tempo e não completa fora dela.
- [ ] Inimigos (lobo, escorpião, aranha) patrulham, perseguem, atacam e fogem; morto não bloqueia passagem.
- [ ] Matar inimigo concede XP e loot; level-up atualiza HUD e atributos; bônus de chefe aplicado.
- [ ] Armas: corpo a corpo, à distância (projétil, munição do inventário), durabilidade e quebra.
- [ ] Atordoamento: Correntes das Trevas (1 s), Terremoto (0,8 s) e Granada Elétrica (0,6 s) param o inimigo, cancelam o golpe preparado, mostram faíscas azuis + "Atordoado!"; chefes ficam metade do tempo.
- [ ] Afixos Ágil/do Frenesi (`AttackSpeed`): ataque básico recarrega mais rápido, sem começar golpe antes do anterior acertar. Espinhosa/do Saxaul (`Thorns`): atacante corpo a corpo leva dano de volta (até 50%), sem recursão; tooltips mostram os afixos.

## 5. Morte e checkpoints
- [ ] Morte mostra tela com fade e contagem regressiva; "Continuar" e "Carregar" funcionam; auto-continuar ao zerar.
- [ ] Renascer no checkpoint ativo (ou no spawn inicial); sem soft-lock, input do jogador restaurado.
- [ ] Queda ao morrer (9%): sem Tumar nem Lâmpada Guardiã, morrer várias vezes com uma pilha de poções, ouro, item de missão e equipamento. Em ~1 a cada 11 mortes a tela mostra "Na pressa, você deixou cair: <item>" + "Perdeu de vez. Um Tumar da Vó Edna ou a Lâmpada Guardiã teria guardado."; sai **só 1 unidade** (pilha de 6 vira 5) e ela não volta (não há mais Achados e Perdidos). Nunca cai ouro, item de missão, item equipado nem item vinculado. Na maioria das mortes nada aparece. Com Tumar ou Lâmpada ativos, nunca cai nada.
- [ ] Save antigo com itens nos antigos "Achados e Perdidos": "Continuar" carrega normal, o Baú abre sem a faixa antiga e os itens dela não aparecem (perdidos de vez). (Online, FASE 12: o item cai no chão para qualquer um pegar.)

## 6. Itens, inventário, crafting e lojas
- [ ] Coletar, empilhar, equipar, usar e descartar itens; tela Mochila consistente.
- [ ] Mochila em grade 8×8 (estilo MU): itens desenhados no tamanho certo (poção/anel/material 1×1, espada 1×3, gibão 2×3, chapéu/botas 2×2, arco 2×4), borda na cor da raridade, quantidade nas pilhas, chip "Espaço: usado/64". Peso não limita mais nada.
- [ ] PC: arrastar um item mostra o fantasma do tamanho dele (verde onde cabe, vermelho onde não cabe); soltar em área livre move, em pilha igual junta, em um item troca de lugar, sobre dois itens não faz nada. Soltar no slot de equipamento equipa; arrastar o equipado para a grade desequipa ali. Botão direito usa/equipa.
- [ ] Mobile: tocar num item pega (fica apagado + fantasma), tocar numa casa solta (vermelho e continua segurando se não couber), tocar de novo no item segurado usa/equipa, segurar mostra o tooltip. Gamepad: A pega, setas até uma casa livre/item, A solta.
- [ ] "Organizar" empacota a partir do canto de cima à esquerda (equipamento primeiro) sem sobrepor nada; numa mochila quase cheia e fragmentada não perde nada.
- [ ] Cheia de verdade: loot que não cabe mostra "Mochila cheia"; loja recusa a compra sem tirar ouro; Bancada não consome material se o resultado não couber; baú "Pegar o que as missões pedem" leva só o que cabe; recompensa de missão que não coube aparece no Baú.
- [ ] Save antigo (de antes da grade): carregar → tudo auto-posicionado, nada sobreposto; o que não coube está no Baú da Vó Edna (chip do baú mostra "N item(ns) esperando lugar" se o baú também estiver cheio). Salvar e carregar de novo mantém cada item na mesma casa.
- [ ] Itens procedurais mostram afixos no tooltip e os bônus realmente mudam os atributos.
- [ ] Tooltips por mouse, toque longo e foco de gamepad, sem sair da tela.
- [ ] Crafting: receita com materiais suficientes cria o item e consome materiais; sem materiais fica bloqueada.
- [ ] Loja: comprar/vender altera moeda e inventário corretamente; sem moeda suficiente bloqueia.
- [ ] Loot por bioma: Bosque do Zarafshan solta madeira/fibra/erva, Serra de Kaggen minério/lingote, Areias Flamejantes e Grutas do Limiar os seus; "+2 Madeira" sobre o corpo; chefe solta 4 materiais + item bônus Raro/Épico. Com a mochila cheia aparece o aviso "Mochila cheia" e nada some em silêncio.
- [ ] Vila (botão "Vila", só no Bosque do Zarafshan com o token parado): moradores com "(!)" quando há missão, ponto de encontro, Loja e Bancada. Fechar Loja/Bancada volta ao painel; comprar e criar no mapa → itens no próximo encontro e no save (salvar, sair, "Continuar").

## 7. Narrativa: diálogos e quests
- [ ] Diálogo com escolhas a partir de NPC; escolhas mudam o ramo; fechar/reabrir não quebra estado.
- [ ] Quests de matar, coletar e conversar progridem e completam; recompensas (itens/XP) entregues.
- [ ] Log de missões e rastreador na tela atualizam em tempo real.
- [ ] Missões regionais: "Conversar com Mariam / Ammo / Mestre Mihr / Parthenos, a das Lamparinas" só aparece em região desbloqueada com o token parado; "(!)" quando há missão ou chefe; 2 missões encadeadas por região, fala por primo, dica antes do chefe, agradecimento depois. Recompensa concluída no mapa chega no próximo encontro.
- [ ] Campanha: Novo jogo → prólogo no primeiro mapa (painel do herói escolhido); toque/"Continuar"/Espaço/Enter/A avançam, "Pular" encerra e conta como visto. Interlúdio ao voltar ao mapa após Kök-Böri, Grande Ushabti e Senhor da Guerra; epílogo após o Tages, o Habitante Ancestral → créditos → mapa pós-jogo + conquista "A Luz Volta para Casa". Sair entre o chefe e o mapa e "Continuar" ainda mostra o beat; save antigo não repete o prólogo.

## 8. Mapa mundi e encontros
- [ ] Mapa mundi: viajar entre regiões desbloqueadas; regiões bloqueadas recusam viagem.
- [ ] Encontro aleatório carrega a cena de combate com os inimigos do bioma; vitória/fuga voltam ao mapa e registram progresso.
- [ ] Auto-save após o encontro.
- [ ] Ilha low-poly: chão e props por região, marcos (vilinha, ruína, acampamento, caverna), estradas pelo grafo, rio com ponte, praia, mar e nuvens; neblina em região bloqueada e bandeira em região limpa; a mesma ilha volta após salvar/carregar; com o HUD aberto o mapa não fica esbranquiçado.

## 9. Save/Load (Bloqueante)
- [ ] Salvar em slot manual, sair do app, reabrir, "Continuar"/carregar: posição, herói, inventário, equipamento, skills/hotbar, quests, progresso do mapa, construções e inimigos derrotados restaurados.
- [ ] Save de versão anterior (guardar um da release passada) carrega sem perda ou migra corretamente.
- [ ] Matar o app durante o auto-save não corrompe o slot (no pior caso cai no save anterior).

## 10. Áudio, acessibilidade e localização
- [ ] Volumes por categoria aplicados; música faz crossfade; sem clipes faltando no log.
- [ ] Escala de fonte, alto contraste, paleta para daltonismo, redução de flashes e intensidade de shake têm efeito visível.

## 11. Performance, bateria e estabilidade (mobile)
- [ ] FPS: ~30 em menus/mapa/pausa e ~60 em combate com "60 FPS em combate" ligado; ~30 em tudo com a opção desligada (overlay de debug ou Android GPU Inspector).
- [ ] PC: vSync ativo, sem tearing nem FPS travado abaixo da taxa do monitor.
- [ ] Sessão de 30 min de combate em aparelho mínimo: sem throttling severo (FPS não cai abaixo do piso do orçamento), aparelho não ultrapassa ~43 °C ao toque, consumo de bateria ≤ ~12%/30 min.
- [ ] Memória estável após 10 transições mapa ⇄ combate (Memory Profiler/overlay; sem crescimento contínuo).
- [ ] Pausar com o app em segundo plano (home/trocar de app) e voltar: jogo pausado, sem perda de estado, áudio retoma.
- [ ] Sem `Exception`/`Error` no logcat/Player.log durante o roteiro inteiro.
- [ ] Relatório de falhas local: matar o app no meio de um encontro (Gerenciador de Tarefas / deslizar nos recentes **com o jogo em primeiro plano**) e reabrir → o título mostra "O jogo fechou de forma inesperada da última vez"; "Copiar relatório" põe o texto na área de transferência (versão, cena, estado, herói, aparelho, últimas linhas do log) **sem** nome de usuário, nome do aparelho, caminhos pessoais ou e-mail; "Fechar" some com o aviso e ele não volta na sessão. Fechar o jogo normalmente ou só mandá-lo para segundo plano não gera aviso. No máximo 5 relatórios em `persistentDataPath/CrashReports`.

## 12. Loja/Compliance (antes da submissão)
- [ ] Classificação indicativa revisada (`Docs/PlatformRequirements.md`).
- [ ] Ícone, nome do app, bundle id e splash definitivos.
- [ ] Créditos e avisos de licença conferidos contra `Docs/ThirdPartyLicenses.md`.

## 13. Dicas de primeira vez (tutorial)
- [ ] Perfil novo (ou "Reiniciar tutorial"): cada dica aparece uma vez — mapa ("Explorar"), primeiro inimigo, skills, loot, missão, nível 2, morte, noite, receita possível, "Conversar com…" (contorna o botão), "Vila" no Bosque do Zarafshan depois do prólogo (contorna o botão), primeiro material ("Materiais!") e primeiro atordoamento ("Atordoado!", só em encontro).
- [ ] Nenhuma dica aparece sobre o prólogo/interlúdios/epílogo, o painel da Vila, a Loja/Bancada, uma conversa ou o banner "Prepare-se!"; a dica volta quando a tela fecha.
- [ ] "Mostrar dicas" desligado: nada aparece; ligar de novo e "Reiniciar tutorial" trazem as dicas. Um `tutorial.json` de versão anterior carrega sem repetir as 9 dicas antigas (só as novas aparecem).

## Rodada 2026-11-23 (não testada no Unity)
> Commits `7b25f11`…`3159fbb` (6 checkpoints, 546 arquivos). Nada disso rodou no Editor. Ordem: compilar sem `error CS` → parte A → parte B. [E] = interagir (botão Usar no celular); 🌙 = de noite no relógio do jogo (esperar, ou `TimeOfDayManager.SetTime(22)`); M = janela de mapas para ir de uma zona a outra.

### A. Testes EditMode a rodar (38 classes, ~536 `[Test]`)
`[U]` = só roda no Unity (usa `Resources.Load`/`LoadAll` de assets de inimigo, item ou missão). As outras são regras puras.
- [ ] **Online, Fase 12 (regras puras, `Docs/Network.md` §10–16):** `PartyRulesTests`, `TradeTests`, `GuildRulesTests`, `GuildBankOwnDataTests`, `PvpRulesTests`, `TimedEventsTests`, `RebirthRulesTests`, `EffectiveLevelTests`, `SocialRulesTests`. Não têm Play Mode: só a parte A.
- [ ] **Cidades:** `CityServicesTests`, `CityTrainingServicesTests`, `CityStallsRound3Tests`, `CityTownVariantTests`, `CityModeTownVariantTests`, `CityForgeAndRaceTests` [U], `KirtaTarchonTownTests` [U], `QuestTargetsSpawnTests` [U].
- [ ] **Mecânicas 01–56:** `PavilionDragonWispTremorTests` [U] (06, 20, 33, 34), `KirtaCoralTarchonNeighbourTests` [U] (28, 33), `PeaksSteppeRoundTests` (39, 51, 53, 56), `PeaksSteppeRound2Tests` (49, 52, 55, 56, 70).
- [ ] **Mecânicas 57–100:** `BaitAndWaterBombTests` (alterado; 64, 65), `ApepSpitRiverTests` (76), `LateFaltaRound2Tests` [U] (78, 79, 93, 99, 100).
- [ ] **Bichos próprios:** `WestOwn2BeastsTests` [U], `PeaksOwnBeastsTests` [U], `ProceduralEnemyModelsTests` [U] (alterado), `WestOwn2BeastsPlacementTests`, `PeaksOwnBeastsPlacementTests`; colocação alterada: `WestOwnBeastsPlacementTests`, `NorthOwnBeastsPlacementTests`, `HighlandOwnBeastsPlacementTests`, `MingchengNartesBeastsPlacementTests`, `LateRegionBeastsPlacementTests`, `FarStandInBeastsPlacementTests`.
- [ ] **Montarias:** `MountTests` (alterado), `MountReefTests`, `MountLegendTests` [U].
- [ ] Ainda sem commit (outro agente): `MarketTests`. Rodar se já estiver no commit testado.

### B. Roteiro de Play Mode (caminhada pelo mundo contínuo)
**Geral (em qualquer cidade do caminho)**
- [ ] 🌙 Kirta, Tarchon, Hinokura e Tambor Rubro: as luzes da cidade ficam sobre os postes e lanternas da própria cidade, não juntas na origem do mundo nem deslocadas (`CityNightLights` em coordenada local).
- [ ] Console sem `Exception` nem `MissingReference` ao entrar e sair de cada cidade abaixo.

**20 · Salão de Tages**
- [ ] 🌙 A cada 150 s: aviso "fique perto de uma lâmpada" e 5 s depois o tremor. Lâmpada sem herói a até 8 m perde 90 s; a que estava junto do herói segura.

**11 · Tambor Rubro (Forja de Tambor)**
- [ ] [E] no Tushratta: a primeira linha é "Consertar tudo: N peças (X de ouro, 20% mais barato aqui)", e o preço bate com 80% do Ferreiro do hub (arredondado para cima, mínimo 1).

**06 · Jardim de Mihrshah (pavilhão)**
- [ ] Com as 4 faíscas livres a porta (lado oeste) brilha. [E] a 3 m: 8 colunas acendem, caem 3 Pétalas de Luz e o primo fala.
- [ ] **Mochila cheia** antes do [E] na porta: aviso de mochila cheia e a porta **não** abre (nada se perde); liberar espaço e abrir.
- [ ] Altar (2,5 m): 1 Pétala por dia do jogo. Com a **mochila cheia**: aviso, e o dia não é gasto (esvaziar e pegar de novo no mesmo dia).
- [ ] Dentro do pavilhão aberto: cura 6%/s fora de combate. Com a porta fechada, o aviso "abra pelo lado oeste".
- [ ] 🌙 Sem pétala: bicho a mais de 9 m some. Comer a Pétala: +120 de vida e +60 de mana, e por 3 min todos ficam visíveis.
- [ ] **Play duas vezes depois da Pétala:** comer a Pétala, parar o Play e dar Play de novo à noite. Os bichos voltam a sumir além de 9 m (sem luz herdada da sessão anterior).

**17 · Salões de Bronze de Napirasu (Grande Forja)**
- [ ] [E] na boca da forja: a tela mostra a etapa atual. Vencer O Fole Apagado e entregar: refino +2%, com o prêmio na hora.
- [ ] **Mochila cheia** ao entregar uma etapa: aviso, e nada é cobrado. A entrega, a etapa e o prêmio ficam como estavam. Liberar espaço e entregar de novo.

**22 · Mercado dos Túneis**
- [ ] [E] na Shayn (rua Leste): troca 3 → 2 do mesmo tipo, sem ouro. A 11ª troca do dia UTC é recusada.
- [ ] Ao entrar no mercado: aviso da galeria da feira e o facho âmbar a ~70 m. Os 2 pacotes saem ~30% mais baratos, uma vez por dia cada.

**25–27 · Costa, Savana dos Auroques, Meroé**
- [ ] Os bichos próprios aparecem no lugar dos emprestados: Chacal-Listrado Apagado (Dunas, Praia dos Dhows), Hiena de Fumaça (Acácias; 🌙 bando maior), Carneiro de Pedra (Meroé).
- [ ] Corrida de Shulgi: [E] entre os postes de chifre (rua Sul), 8 fitas azuis, chegada na estrada sul. Ouro ≤110 s / prata ≤135 s / bronze ≤170 s pagam 3/2/1 Fitas (+2 na primeira do dia). No poste do Shulgi, 25 Fitas compram o Auroque.

**28 · Kirta (corais e cidade)**
- [ ] 5 corais apagados nas trilhas do recife. [E] a 3,5 m acende o coral (anel de 8 m, cura 2%/s fora de combate). O primo fala no 1º coral, e a Kirta fala quando os 5 estão acesos.
- [ ] **Ordem dos corais:** o coral-mãe começa com 3 dos 7 galhos acesos, e cada coral acende mais um galho, **de leste para oeste**. Com os 5 acesos, o tronco e a borda do poço também acendem. O anel vai do cinza ao rosa.
- [ ] Acender um coral com Kirta carregada: em ≤0,5 s o coral-mãe muda sem engasgo e sem mexer em casa, NPC ou colisor.
- [ ] Sair do Play e entrar em Kirta pelo modo cidade (fora do mundo contínuo): o coral-mãe está igual ao save. A banca do Mascate continua lá.
- [ ] O facho verde-água marca a bolha do mergulhador do dia (a Pughat diz onde). Passar a até 5 m pelo fundo solta ele: 75 de ouro + Pedra da Bênção. Com a mochila cheia ele espera a próxima passada.
- [ ] Danel vende o Apito de Concha (145): o Hipocampo nada, e o Canto do Recife cura o grupo a 6 m.

**29 · Kilwa Afundada**
- [ ] Os dingoneks aparecem com uma Raia de Fumaça. 🌙 a forma da noite ronda as ruas das raias.

**33 · Palafitas de Tarchon**
- [ ] 5 trols apagados (anel cinza). [E] no Mastro dá a lanterna (120 s, uma por vez), e [E] ao lado de um trol acorda ele até o fim do dia. Com os 5 acordados, a Thanchvil paga 120.
- [ ] **Ordem dos trols:** com 3/10/20/35/55/80 vizinhos acordados no total voltam Aule, Seianti, Thana, Sethre, Hasti e Tite, nessa ordem, cada um com gesto e fala só 1×.
- [ ] As lanternas da praça: +2 no 2º trol que volta e +2 no 5º. Os colisores novos aparecem e os velhos continuam.
- [ ] **Postes com o herói em cima:** ficar parado onde nasce um poste novo na hora em que o 2º (ou 5º) trol volta. A troca espera, e o poste só aparece depois que o herói sai (ele nunca fica preso dentro).
- [ ] [E] na Cestaria: a Larthia paga 25 por fogo-fátuo (até 10 com o herói). Com 20 vendidos ela dá a Lanterna. A diária "Despertar os Vizinhos" aparece no diário.
- [ ] 🌙 A névoa lilás esconde bicho além de 18 m, e a Lanterna na bolsa devolve a visão. Vencer o Pescador: nuvem de 6 fogos-fátuos (mana 5%/s no anel por 120 s) e 10 min de noite limpa.

**34–35 · Ninki Nanka, Coração do Lodo**
- [ ] O golpe "não olhe" da Ninki deixa o herói a ×0,6 por 3 s (não acumula), e os outros golpes dela não deixam lento. O Crocodilo de Musgo e o Gavial-Sombra aparecem nos lugares deles.

**37–42 · Calhas, Geleira, Forte dos Ventos, Mingcheng, Amanirenas**
- [ ] 39: com 3 ou 4 cata-ventos para o mesmo lado, a rajada contra o vento se quebra e a rajada a favor vai a 20 m. O aviso diz qual das duas vem.
- [ ] Os bichos certos: o Ushabti de Calha (37), a Fenda Viva (38), a Lanterna de Charada nos Anéis (41) e o Guardião de Cal nas Torres (42).

**46–53 · Cidadela, Fortim, Pradaria, Corrente, Vale dos Sátiros**
- [ ] 46: o Servo de Taotie, o Filhote de Qiongqi (foge ferido) e a Bolha de Hundun.
- [ ] 49: com O Pesadelo vencido, o Virogdad dá a distância arredondada a 25 m e o baú dá 70 pepitas (1× por dia).
- [ ] 51: entregar cavalos no Poço da Taça. Na entrega que chega a 40 pontos sai o Cavalo Celeste certo, que nunca vem repetido.
- [ ] 52 🌙: os elos até a argola mais alta brilham e a rajada empurra pela metade. Acima deles empurra ×1,3, e andando contra o herói ainda segura.
- [ ] 53: [E] no Pactolo enche 3 derramadas. [E] a 6 m da estátua do Midas atordoa e desliga o toque de ouro por 6 s.
- [ ] 53 🌙: [E] no palco pede o Baile. São 12 notas, e apertar sem parar não ganha (o tropeço dá −1).

**55 · Pontes de Vento**
- [ ] Andar sem capa no meio da ponte durante uma rajada: cai na rede, volta 12 m para trás e perde 8% da vida (nunca abaixo de 25%).
- [ ] Parado segurando: não cai. Com a Capa de Tempestade, só cai quem está na beira. Sair da ponte andando, sem rajada, não conta como queda.
- [ ] Com O Cortador de Cordas libertado: o herói volta ao ponto da rajada, sem dano e sem recuo. O primo fala na 1ª vez.

**56 · Ermidas Suspensas**
- [ ] As skills silenciosas (ex.: Flecha Precisa, Bênção da Vida) baixam o medidor 8, e as barulhentas sobem 12.
- [ ] Encher o medidor: o Eco sai da pedra e persegue o herói (no máximo 2). Com O Sino vencido, o medidor enche mais devagar (~17,9 s correndo).

**59 · Porto da Lua Branca · 64 · Fundo do Velho Kul**
- [ ] [E] no Makarios: os 2 pacotes da lua, e a tela diz a lua de amanhã.
- [ ] Vencer o Kul: por 30 min, raro ×1,5, buraco descansando 15 s e, dono da pesca, 20% de peixe dobrado. Com a mochila cheia, o aviso.

**65 · Hinokura · 68 · Recife de Obsidiana · 70 · Caldeira**
- [ ] O Kurogane diz qual é a forja do dia e quanto falta. Com ela ardendo, o refino mostra +5% (somado à Grande Forja, teto de 95%).
- [ ] 68: em pé na casca da bomba de água sobre o transbordo, a lava não queima (20 s).
- [ ] 70 🌙: o calor começa mais perto da margem (aviso 1× por noite).
- [ ] **Glutão e forjas:** colado no Glutão de Brasa sem forja acesa, o medidor enche em ~20 s. Acender as 3 forjas de Hinokura e voltar: o calor dele cai a 1,25/s, o aviso da luta diz "3 forjas", e a 24 m dele o medidor esfria.

**71 · Mesektet · 76 · Apep**
- [ ] [E] na porta do Djedi: um círculo por vez, com a 1ª lição do dia de graça. [E] na porta do Setne: as prateleiras pelas páginas do Caderno.
- [ ] Vencer Apep: 1 Pedra da Mistura (2 se perto da meia-noite). Com a **mochila cheia** as pedras ficam devidas e entram quando houver espaço.

**78–79 · Deserto do Relógio, Poços de Betume**
- [ ] 78 🌙: o bicho que cai deixa uma Sombra Atrasada em 2,5 s (no máximo 4). De dia somem todas.
- [ ] 79 🌙: entrar na poça apagada cospe um Betume Vivo (no máximo 3). Uma skill de fogo na Fumaça de Piche estoura a fumaça e acende as poças a até 5 m.

**93 · 99 · 100 · Céu**
- [ ] 93: [E] numa Faísca Teimosa a 4 m e ficar 6 s sem bater convence ela. Ela segue até a lanterna da Proa, e a lua do convés cresce.
- [ ] 99 🌙: o pilar pede 24 s, e selar cura 40%. 100: levantar no meio da história e sentar de novo no mesmo dia continua da mesma fala.
