# Rapadura — Requisitos de Plataforma e Classificação Indicativa

> Documento vivo, complementa `Docs/GDD.md`. Cobre os dois itens pendentes da Fase 0 (Documentação e Escopo): requisitos mínimos de hardware mobile e definição de público-alvo/classificação indicativa. Ajustar quando o protótipo de performance da Fase 10 trouxer números reais de profiling.

## Base técnica considerada

Rapadura é um RPG de ação em terceira pessoa, tempo real, com câmera orbital, sistema de combate com hitbox/hurtbox/knockback/buffs, inventário completo, skill tree e save versionado — tudo já implementado em C# (ver `TODO.md`). Renderiza em **URP** (`com.unity.render-pipelines.universal 17.0.3`, `Packages/manifest.json`), não HDRP, o que já é a escolha certa para mobile. Ainda não há assets de arte final, VFX pesado, iluminação dinâmica complexa nem terrenos grandes — então os números abaixo são estimativas de engenharia para um RPG de ação URP com esse escopo de sistemas, não medições de profiling real (isso só vem na Fase 10). Mobile é plataforma obrigatória, então os mínimos foram calibrados para não empurrar o público para fora do alcance de aparelhos populares.

---

## Mobile — Android

### Mínimo
- **OS:** Android 8.0 (API 26) ou superior
- **RAM:** 3 GB
- **Chipset de referência:** Snapdragon 660 / Mediatek Helio G80 ou equivalente (GPU Adreno 512 / Mali-G52 classe)
- **Storage livre:** 2 GB
- **Requisitos:** suporte a OpenGL ES 3.1 ou Vulkan; touchscreen multi-touch (necessário para pinch-to-zoom da câmera)

### Recomendado
- **OS:** Android 11+
- **RAM:** 6 GB+
- **Chipset de referência:** Snapdragon 730G / Mediatek Dimensity 900 ou superior
- **Storage livre:** 4 GB (folga para updates e save data)

Justificativa: combate em tempo real com múltiplos hitboxes/buffs ativos, câmera 3D orbital e UI Toolkit (skill tree) pedem GPU de média faixa, não entry-level. API 26 é o piso realista para manter New Input System e Addressables funcionando sem workarounds de compatibilidade.

## Mobile — iOS

### Mínimo
- **OS:** iOS 14
- **Modelos de referência:** iPhone 8 / iPhone SE (2ª geração) ou superior
- **Storage livre:** 2 GB

### Recomendado
- **OS:** iOS 16+
- **Modelos de referência:** iPhone 11 ou superior
- **Storage livre:** 4 GB

Justificativa: iPhone 8 (chip A11) já tem GPU suficiente para URP com o escopo atual de efeitos; abaixo disso a base instalada é pequena e o custo de suporte não compensa.

---

## PC (adicional, não prioritário)

### Windows — Mínimo
- Windows 10 64-bit
- CPU dual-core 2.5 GHz+
- 8 GB RAM
- GPU com suporte a DirectX 11, 2 GB VRAM (ex: GTX 750 Ti / equivalente integrado recente)
- 4 GB de espaço em disco

### macOS — Mínimo
- macOS 12 (Monterey) ou superior
- Mac com Apple Silicon (M1) ou Intel com GPU dedicada
- 8 GB RAM
- 4 GB de espaço em disco

Recomendado em ambas as plataformas: dobrar RAM (16 GB) e usar SSD para tempos de load menores. Estes números serão revisados após profiling real (Fase 10); por ora refletem apenas "roda um RPG de ação URP com folga", sem medição no projeto.

---

## Público-alvo

- **Faixa etária primária:** 13–30 anos, jogadores de RPG de ação/fantasia (referência de gênero: hack-and-slash com progressão de personagem, multi-personagem jogável).
- **Perfil:** jogadores que gostam de progressão (itens, skill tree, atributos), combate em tempo real e exploração, tanto em sessões curtas (mobile, touch) quanto sessões longas (PC).
- **Conteúdo do jogo (base para a faixa etária):** violência estilizada de fantasia (combate contra criaturas sombrias, sem sangue explícito ou gore), sem conteúdo sexual, sem uso de drogas, sem linguagem imprópria pesada. Nada no GDD atual (`Docs/GDD.md`) indica conteúdo adulto — histórias dos 5 personagens tratam de perda, proteção e jornada heroica, tom apropriado para adolescentes.

Isso resolve o item pendente do GDD (`## Público-alvo e classificação`), que ainda estava marcado como decisão pendente — a intensidade de violência/efeitos de combate deve seguir esse recorte (estilizado, não gráfico) para manter a classificação abaixo.

---

## Classificação indicativa recomendada

> **Observação importante:** as classificações abaixo são a expectativa de design, não uma classificação oficial. A classificação final só é obtida no processo de submissão em cada loja/comitê (ESRB via publisher, PEGI via questionário PEGI Express na submissão do App Store/Google Play/Steam). Se o escopo de violência ou conteúdo mudar (ex: sangue mais explícito, temas mais pesados), esta seção precisa ser revisitada antes da submissão.

### ESRB: **Teen (13+)**
Critérios que sustentam essa faixa:
- Violência de fantasia contra criaturas (não humanos realistas), sem sangue/gore explícito
- Sem conteúdo sexual ou nudez
- Sem referência a drogas/álcool
- Linguagem leve ou ausente

### PEGI: **PEGI 12**
Critérios que sustentam essa faixa:
- Violência não-realista/fantasiosa contra criaturas fantásticas (critério central do PEGI 12 é justamente "violência em contexto fantasioso, dirigida a criaturas não-humanas")
- Sem conteúdo sexual, drogas ou jogos de azar
- Se o combate evoluir para mais realista/gráfico contra inimigos humanoides, reavaliar para PEGI 16

### Recomendação prática
Manter o design de combate consistente com Teen/PEGI 12 durante o desenvolvimento (efeitos de dano estilizados, sem desmembramento/sangue excessivo) evita ter que reduzir o público-alvo ou redesenhar VFX de combate perto do lançamento. Confirmar a classificação real apenas na submissão às lojas (Google Play, App Store, Steam), conforme já listado em `TODO.md` (Fase 10 — Legal/Compliance).
