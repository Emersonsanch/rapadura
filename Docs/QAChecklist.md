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
- [ ] Afixos Ágil/do Frenesi (`AttackSpeed`): ataque básico recarrega mais rápido, sem começar golpe antes do anterior acertar. Espinhosa/do Mandacaru (`Thorns`): atacante corpo a corpo leva dano de volta (até 50%), sem recursão; tooltips mostram os afixos.

## 5. Morte e checkpoints
- [ ] Morte mostra tela com fade e contagem regressiva; "Continuar" e "Carregar" funcionam; auto-continuar ao zerar.
- [ ] Renascer no checkpoint ativo (ou no spawn inicial); sem soft-lock, input do jogador restaurado.

## 6. Itens, inventário, crafting e lojas
- [ ] Coletar, empilhar, equipar, usar e descartar itens; peso respeitado; tela Mochila consistente.
- [ ] Itens procedurais mostram afixos no tooltip e os bônus realmente mudam os atributos.
- [ ] Tooltips por mouse, toque longo e foco de gamepad, sem sair da tela.
- [ ] Crafting: receita com materiais suficientes cria o item e consome materiais; sem materiais fica bloqueada.
- [ ] Loja: comprar/vender altera moeda e inventário corretamente; sem moeda suficiente bloqueia.
- [ ] Loot por bioma: Floresta solta madeira/fibra/erva, Montanha minério/lingote, Deserto e Cavernas os seus; "+2 Madeira" sobre o corpo; chefe solta 4 materiais + item bônus Raro/Épico. Com a mochila cheia aparece o aviso "Mochila cheia" e nada some em silêncio.
- [ ] Vila (botão "Vila", só na Floresta com o token parado): moradores com "(!)" quando há missão, ponto de encontro, Loja e Bancada. Fechar Loja/Bancada volta ao painel; comprar e criar no mapa → itens no próximo encontro e no save (salvar, sair, "Continuar").

## 7. Narrativa: diálogos e quests
- [ ] Diálogo com escolhas a partir de NPC; escolhas mudam o ramo; fechar/reabrir não quebra estado.
- [ ] Quests de matar, coletar e conversar progridem e completam; recompensas (itens/XP) entregues.
- [ ] Log de missões e rastreador na tela atualizam em tempo real.
- [ ] Missões regionais: "Conversar com Dona Cida / Seu Bento / Mestre Tonho / Benedita das Lamparinas" só aparece em região desbloqueada com o token parado; "(!)" quando há missão ou chefe; 2 missões encadeadas por região, fala por primo, dica antes do chefe, agradecimento depois. Recompensa concluída no mapa chega no próximo encontro.
- [ ] Campanha: Novo jogo → prólogo no primeiro mapa (painel do herói escolhido); toque/"Continuar"/Espaço/Enter/A avançam, "Pular" encerra e conta como visto. Interlúdio ao voltar ao mapa após Lobo Alfa, Guardião da Ruína e Senhor da Guerra; epílogo após o Habitante Ancestral → créditos → mapa pós-jogo + conquista "A Luz Volta para Casa". Sair entre o chefe e o mapa e "Continuar" ainda mostra o beat; save antigo não repete o prólogo.

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
- [ ] Perfil novo (ou "Reiniciar tutorial"): cada dica aparece uma vez — mapa ("Explorar"), primeiro inimigo, skills, loot, missão, nível 2, morte, noite, receita possível, "Conversar com…" (contorna o botão), "Vila" na Floresta depois do prólogo (contorna o botão), primeiro material ("Materiais!") e primeiro atordoamento ("Atordoado!", só em encontro).
- [ ] Nenhuma dica aparece sobre o prólogo/interlúdios/epílogo, o painel da Vila, a Loja/Bancada, uma conversa ou o banner "Prepare-se!"; a dica volta quando a tela fecha.
- [ ] "Mostrar dicas" desligado: nada aparece; ligar de novo e "Reiniciar tutorial" trazem as dicas. Um `tutorial.json` de versão anterior carrega sem repetir as 9 dicas antigas (só as novas aparecem).
