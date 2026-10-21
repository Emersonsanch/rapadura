# Rapadura — Checklist de Regressão (pré-release)

> Rodar antes de **toda** build marcada como release (tag `vX.Y.Z`, ver `CHANGELOG.md`). Marcar cada item em pelo menos **1 Android de faixa mínima** (ver `Docs/PlatformRequirements.md`), **1 iOS** quando houver build iOS e **1 PC**. Registrar aparelho, versão (`BuildVersion.Current` aparece nos Créditos) e resultado no PR/issue da release. Qualquer item vermelho em "Bloqueante" impede a publicação.

## 0. Build e automação (Bloqueante)
- [ ] Projeto abre sem erros no Console (Unity 6000.3, sem `error CS`).
- [ ] Suíte EditMode inteira verde (Test Runner) e CI do GitHub Actions verde no commit da release.
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

## 2. Seleção de personagem
- [ ] Os 5 heróis (Maithe, Maria, Joaquim, Ícaro, Lavine) aparecem com preview girando e cor/nome corretos.
- [ ] Herói escolhido entra no jogo com o modelo, passiva e conjunto de skills dele; troca de personagem jogável funciona.
- [ ] Heróis com rig (Maria, Joaquim, Ícaro, Lavine) animam Idle/Walk/Run; Maithe usa a animação procedural sem erros.

## 3. Movimento, câmera e controles
- [ ] Movimento por teclado, gamepad, joystick de toque e toque/clique-para-mover (anel aparece e some).
- [ ] Botões de toque Correr e Pular respondem; nada da HUD fica sob notch/barra de gestos (safe area).
- [ ] Zoom por pinça e roda do mouse dentro dos limites; câmera não atravessa rochas/paredes.
- [ ] Remapeamento de controles (se exposto) persiste.

## 4. Combate
- [ ] Ataque básico e clicar/tocar no inimigo para atacar causam dano, números flutuantes e shake (respeitando a intensidade de acessibilidade).
- [ ] Skills 1–5 via teclado, gamepad e hotbar de toque: custo de mana/energia, cooldown visível, efeitos (buff/debuff/DoT Queimadura) aplicados.
- [ ] Combo de exemplo completa dentro da janela de tempo e não completa fora dela.
- [ ] Inimigos (lobo, escorpião, aranha) patrulham, perseguem, atacam e fogem; morto não bloqueia passagem.
- [ ] Matar inimigo concede XP e loot; level-up atualiza HUD e atributos; bônus de chefe aplicado.
- [ ] Armas: corpo a corpo, à distância (projétil, munição do inventário), durabilidade e quebra.

## 5. Morte e checkpoints
- [ ] Morte mostra tela com fade e contagem regressiva; "Continuar" e "Carregar" funcionam; auto-continuar ao zerar.
- [ ] Renascer no checkpoint ativo (ou no spawn inicial); sem soft-lock, input do jogador restaurado.

## 6. Itens, inventário, crafting e lojas
- [ ] Coletar, empilhar, equipar, usar e descartar itens; peso respeitado; tela Mochila consistente.
- [ ] Itens procedurais mostram afixos no tooltip e os bônus realmente mudam os atributos.
- [ ] Tooltips por mouse, toque longo e foco de gamepad, sem sair da tela.
- [ ] Crafting: receita com materiais suficientes cria o item e consome materiais; sem materiais fica bloqueada.
- [ ] Loja: comprar/vender altera moeda e inventário corretamente; sem moeda suficiente bloqueia.

## 7. Narrativa: diálogos e quests
- [ ] Diálogo com escolhas a partir de NPC; escolhas mudam o ramo; fechar/reabrir não quebra estado.
- [ ] Quests de matar, coletar e conversar progridem e completam; recompensas (itens/XP) entregues.
- [ ] Log de missões e rastreador na tela atualizam em tempo real.

## 8. Mapa mundi e encontros
- [ ] Mapa mundi: viajar entre regiões desbloqueadas; regiões bloqueadas recusam viagem.
- [ ] Encontro aleatório carrega a cena de combate com os inimigos do bioma; vitória/fuga voltam ao mapa e registram progresso.
- [ ] Auto-save após o encontro.

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

## 12. Loja/Compliance (antes da submissão)
- [ ] Classificação indicativa revisada (`Docs/PlatformRequirements.md`).
- [ ] Ícone, nome do app, bundle id e splash definitivos.
- [ ] Créditos e avisos de licença conferidos contra `Docs/ThirdPartyLicenses.md`.
