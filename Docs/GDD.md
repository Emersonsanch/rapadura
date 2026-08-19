# Rapadura — Game Design Document (enxuto)

> Documento vivo — atualizar conforme decisões de design forem tomadas. Não é uma bíblia completa, é a fonte única de verdade para as decisões que já foram batidas.

## Pilares do Jogo
1. **Ação com identidade** — 5 personagens jogáveis com classes, funções e habilidades distintas (tanque, suporte, DPS à distância, controle tecnológico, dano mágico/CC).
2. **Progressão significativa** — inventário, itens com raridade, atributos e skill tree conectados entre si.
3. **Mundo vivo** — biomas, NPCs, quests e ciclo dia/noite dão motivo para explorar, não só para lutar.

## Gênero
RPG de ação em terceira pessoa, com combate em tempo real, progressão de personagem, crafting/construção e exploração.

## Plataforma-alvo
- [ ] Definir prioridade: PC (Steam) primeiro, depois mobile/console — **decisão pendente**, ver TODO.md Fase 0.
- Requisitos mínimos a definir após protótipo de performance (Fase 10).

## Público-alvo e classificação
- [ ] Definir faixa etária e classificação indicativa (ESRB/PEGI) — pendente. Influencia intensidade de violência/sangue nos efeitos de combate.

## Roster de Personagens
Ver `TODO.md` → seção "👥 Personagens Jogáveis (Roster)" para lore completo, classes, funções e habilidades de Joaquim, Maria, Maithe, Ícaro e Lavine.
Implementação em código: `Assets/Scripts/Gameplay/Characters/`.

### Árvore genealógica
Os 5 personagens jogáveis são todos primos entre si — uma família só, não um grupo de estranhos que se conheceu por acaso. A estrutura:
- **Maithe** e **Ícaro** são irmãos.
- **Joaquim** e **Maria** são irmãos.
- **Lavine** é prima dos outros quatro (filha do irmão/irmã dos pais dos outros dois pares).

Ordem de idade, do mais velho para o mais novo:
1. **Maithe** (mais velha)
2. **Maria** (~3 meses mais nova que Maithe)
3. **Joaquim** (nasceu logo depois de Maria)
4. **Ícaro** (~3 anos mais novo que Joaquim)
5. **Lavine** (mais nova, ~3 anos mais nova que Ícaro)

Essa relação de parentesco é canônica e deve informar diálogos, quests e lore futuros — os personagens se conhecem a vida inteira como família, não são aliados recém-formados.

## Loop de Gameplay (macro)
Explorar → combater/coletar recursos → craftar/equipar → evoluir personagem (skills/atributos) → avançar no mundo/enfrentar bosses → repetir com poder crescente.

## Referências / Inspirações
- [ ] Preencher com jogos de referência (gênero, combate, arte) conforme forem definidos.

## Escopo fora do MVP
Multiplayer, live-ops, DLC e conteúdo de temporada são explicitamente pós-lançamento (ver Fases 12–14 do `TODO.md`) e não bloqueiam o MVP.

---
*Ver `TODO.md` na raiz do projeto para o roadmap de desenvolvimento fase a fase.*
