# Progressão até o nível 500 (estilo MU), montarias que crescem com o herói e respawn na cidade

> **Status:** proposta de design (2026-10-30). **Só documentação:** nada daqui existe no código ainda.
> Pedido do usuário: "as montarias ... quero que elas evoluam também conforme os heróis forem evoluindo, porém elas têm
> limite até lvl 20 e os heróis podem chegar até nível 500; quero que você também crie no md para desenvolvimento até
> 100, mas escreva os 100 modelos de mapas no md para irmos fazendo com o tempo; e outra coisa: o respawn sempre volta
> para a cidade."
> **Lê junto:** `Docs/Balance/README.md` (§1 curva, §10 modelo, §11 NG+, §14 pilha de poder, §18–19 morte, §25
> durabilidade, §27 talentos), `Docs/ClassEvolution.md`, `Docs/Mounts.md` (§2.4 orçamento, §4 nível/comida/afeição),
> `Docs/Cities.md`, `Docs/WorldBible_Maniqueismo.md` e o catálogo de mapas **`Docs/Mapas100.md`**.
> **Tom:** vale a bíblia do mundo (§0): cair e levantar, nunca morrer de verdade; nada de gore.

Sumário: 0. Resumo · 1. Curva de XP até 500 · 2. Pontos por nível · 3. Graduações, Mestre e talentos ·
4. Orçamento de poder (nível do bicho) · 5. Escopo "até 100" · 6. Montarias que crescem com o herói ·
7. Respawn sempre na cidade · 8. O que muda no código · 9. Decisões para o usuário

---

## 0. Resumo (uma tela)

| Tema | Regra proposta |
|---|---|
| Teto do herói | **500** (hoje 50). Do 1 ao 50 a curva **não muda** (é a do `progression.csv`). |
| Curva | `xp(n) = round(100 × 1,10^(n−1))` até o 50; depois `round(10 672 × (n/50)³ × W(n))`, com `W = 1` até o 300 e `1 + ((n−300)/200)²` depois. |
| Ritmo | 1→50 ≈ **8 h** de jogo; 1→100 ≈ **45 h**; 1→500 ≈ **860 h** (MMO de longo prazo, como o MU). |
| Nível do bicho | todo bicho ganha um **nível** (o do mapa). Vida e dano escalam com o "herói de referência" daquele nível, então as metas de tempo (comuns 4–7 s, chefes 15–22 s) valem em qualquer nível. |
| Graduações | 8 / 14 / 25 **ficam**; novas no **150** (G4, "do Elemento") e no **350** (G5, "da Coluna"). |
| Nível de Mestre | sai do 50 e vai para o **500** (continua sendo "no teto, o XP vai para a barra de mestre"). |
| Montaria | continua com teto **20**, mas o teto **libera com o nível do herói**: `teto = clamp(⌊2·√nível do herói⌋, 1, 20)` → chega a 20 no **herói 100**. |
| Despertar da montaria | montaria 20 + herói **100 / 200 / 300 / 500** → Despertar I–IV: visual e habilidade melhor, **nenhum nível acima de 20**, nenhuma velocidade a mais, nenhum dano. |
| Respawn | morrer **sempre** acorda o herói na **praça da cidade de retorno do mapa** (cada mapa do `Mapas100.md` tem uma). Nunca no lugar, nunca no checkpoint da arena. |
| Escopo agora | **"até 100"**: teto de conteúdo 100, mapas **01–42** do `Mapas100.md`, 12 cidades, Despertar I. O resto fica escrito e entra por ondas. |

---

## 1. Curva de XP até 500

### 1.1 Fórmula

```
xp_para_o_próximo(n) =
    round(100 × 1,10^(n−1))                          se n ≤ 50      (igual ao progression.csv de hoje)
    round(10 672 × (n / 50)^3 × W(n))                se n > 50
W(n) = 1                                             se n ≤ 300
W(n) = 1 + ((n − 300) / 200)^2                       se n > 300     ("parede" final do MU: 400 = ×1,25, 500 = ×2)
```

- **Por que trocar de forma no 50:** 1,10ⁿ é exponencial; se continuasse, o nível 100 pediria 1,2 milhão e o 500
  passaria de 10²⁰ (impossível). A cúbica cresce ~6% por nível logo depois do 50 (a exponencial crescia 10%), então
  a emenda é suave: 49 → 9 702, 50 → 10 672, 51 → 11 325.
- **O 1–50 fica idêntico:** o `progression.csv`, o `ProgressionCurveTests` e o r8/r9 do README continuam valendo sem
  mexer em nada. O que muda no 1–50 é só o que vem depois (§2–4).

### 1.2 XP de um abate (bicho comum do mesmo nível)

```
xp_abate(M) = round(10 × ((M + 1) / 3)^0,9)          se M ≤ 50      (bate com os bichos de hoje)
xp_abate(M) = round(128 × (M / 50)^2,4)              se M > 50
chefe de mapa = 9 × comum · chefe do mundo = 30 × comum · elite = 3 × comum
```

Conferência com o jogo de hoje: M 2 → **10** (Lobo do Tugai: 10), M 5 → **19** (Escorpião: 18), M 9 → **30**
(Ushabti: 28), M 13 → **40** (Morcegos: 40). O chefe ×9 bate com o Lobo Alfa (90). Ou seja: **até o nível ~15 o jogo
continua exatamente no ritmo de hoje**; daí em diante os bichos dos mapas novos dão o XP da tabela.

**Diferença de nível** (estilo MU, evita "farmar" o mapa fácil):

| Bicho em relação ao herói | XP |
|---|---|
| até 10 níveis abaixo, ou acima | 100% |
| 11–20 abaixo | −5% por nível além do 10 (20 abaixo = 50%) |
| mais de 20 abaixo | 10% (e sem drop de equipamento; materiais continuam) |
| mais de 15 acima | 100% de XP, mas o bicho acerta ×1,25 e o herói ×0,8 (aviso "Perigoso" no nome) |

### 1.3 Marcos

"Horas de abate" = só matando no mapa certo a **200 abates por hora** (o ritmo de hoje: ~3,5 abates por minuto
incluindo andar). "Horas de jogo" = ×1,4 (cidade, viagem, morte e volta, eventos). As missões regionais, diárias e de
caça (~20–30% do XP no campo) entram como **folga**: quem faz missão chega antes.

| Nível | XP para o próximo | XP total para chegar | XP por abate (mesmo nível) | Abates por nível | Horas de abate (acum.) | **Horas de jogo (acum.)** |
|---|---|---|---|---|---|---|
| 1 | 100 | 0 | 7 | 14 | 0 | **0** |
| 10 | 236 | 1 357 | 32 | 7 | 0,4 | **0,6** |
| 25 | 985 | 8 848 | 70 | 14 | 1,1 | **1,5** |
| 50 | 10 672 | 105 715 | 128 | 83 | 5,7 | **8** |
| 100 | 85 376 | 2 069 521 | 676 | 126 | 32 | **45** |
| 150 | 288 144 | 10 639 402 | 1 788 | 161 | 68 | **95** |
| 200 | 683 008 | 33 787 346 | 3 566 | 192 | 112 | **157** |
| 300 | 2 305 152 | 171 713 342 | 9 436 | 244 | 221 | **310** |
| 400 | 6 830 080 | 580 789 956 | 18 820 | 363 | 367 | **513** |
| 500 | — (teto: vai para o Mestre) | 1 853 225 538 | 32 152 | — | 614 | **860** |

Leitura rápida:
- **Começo (1–50) perto de hoje:** 2–4 min por nível até o 25 (hoje: 1,6–2,9 min/nível, §10.4), ~25 min no 50.
- **Meio (50–200):** de 25 min a ~1 h por nível. **Fim (300–500):** 1,2 h → 3,3 h por nível; o 499→500 pede ~660
  abates.
- O **Fôlego diário** (§23 do README: 3 h com +30%) e o "Descansado" da casa continuam por cima, e são o freio
  antivício: as horas acima são de jogo **ativo**, espalhadas por meses.
- **Cuidado de código:** o XP total passa de **1,8 bilhão** (perto do teto do `int`, 2,1 bi). O XP **dentro do nível**
  cabe em `int` (máximo 21,3 milhões), mas qualquer soma acumulada (estatística, ranking, servidor) deve ser `long`.

### 1.4 Planilha

`progression.csv` ganha as linhas 51–500 (gerar pelo menu *Regenerate Progression Table From Formula* depois de pôr a
fórmula por partes no `ProgressionCurve`). Para o escopo "até 100" (§5) basta gerar até o **100**.

---

## 2. Pontos por nível

| O quê | Hoje (1–50) | Proposta (1–500) | No 100 / 500 |
|---|---|---|---|
| Pontos de atributo | 2 por nível | **2 por nível sempre** (+ Melão de Luz, até 2) | 198 / 998 |
| Vida da curva | 100 + 8 por nível | igual, continua | 892 / 4 092 |
| `damage_mult` | 1 + 0,02 × (n−1) | igual, continua | 2,98 / 10,98 |
| Pontos de skill | 1 por nível | 1 por nível até o 50; **1 a cada 5 níveis** do 51 ao 500 (as skills chegam ao teto 5 antes do 50) | 59 / 139 |
| Roda de Talentos | abre no 12, +1 por nível (39 no 50) | +1 por nível até o 50; **+1 a cada 10 níveis** do 51 ao 500 | 44 / 84 |
| Treino pelo uso (§27.1) | 0–20 | **não muda** (o teto 20 e a curva 1,25ⁿ seguram sozinhos) | — |

**Por que os atributos não quebram nada:** o "herói de referência" do modelo (§4) já inclui os 2 pontos por nível
gastos do jeito guloso. Os bichos de cada nível são medidos contra ele. Se 998 pontos parecerem muito na Ficha, a
saída é **diminuir o efeito por ponto** (ex.: Vitalidade dá vida percentual da curva, não vida plana), nunca dar menos
pontos: subir de nível e distribuir pontos é o prazer do MU.

**Roda de Talentos maior:** a roda de hoje cabe 48 pontos e o herói teria 84 no 500. Proposta: um **4º anel**
("Anel do Despertar", 2 nós por quarto, 3 graus cada, abre no herói **150** junto com a G4) → capacidade **72**,
e uma **2ª pedra-chave** por quarto no herói **350** (custa 3) → **84**. No 500 a roda enche exatamente; antes disso é
escolha. Os nós novos seguem a regra do §27.3: ataque em décimos de %, o "gostoso" em Apoio e Utilidade.

---

## 3. Graduações, Nível de Mestre e talentos num teto de 500

### 3.1 Onde fica cada coisa

| Marco | Nível | O quê | Muda o código? |
|---|---|---|---|
| G1 | 8 | graduação 1 (como hoje) | não |
| G2 + combo da classe | 14 | graduação 2 (como hoje) | não |
| G3 | 25 | graduação 3, Vó Edna (como hoje) | não |
| Roda de Talentos | 12 | abre (como hoje) | só a regra de pontos (§2) |
| Despertar I da montaria | 100 | §6.4 | novo |
| **G4 "do Elemento"** | **150** | missão na **capital do elemento** do primo (`Mapas100.md`, cidades-capitais) | `MaxTier` 3 → 5 |
| 4º anel da roda | 150 | §2 | novo |
| Despertar II | 200 | §6.4 | novo |
| Despertar III | 300 | §6.4 | novo |
| **G5 "da Coluna"** | **350** | missão da família inteira nos Reinos do Céu (mapas 91–100) | idem |
| 2ª pedra-chave | 350 | §2 | novo |
| **Nível de Mestre** | **500** (teto) + G5 | o XP de abate vai para a barra de mestre (a mesma regra de hoje, só o número muda) | `RequiredLevel` 50 → 500 |
| Despertar IV | 500 | §6.4 | novo |

Por que assim: as três graduações de hoje são a **campanha** (história da família, 8–25) e não devem mudar. O MU dá a
2ª classe no 150 e a 3ª no 400; aqui a G4 vem no 150 (o herói já conhece o mundo grande) e a G5 no 350 (para chegar
na reta final já graduado). O Mestre continua sendo "o que vem depois do teto".

### 3.2 Títulos (proposta; textos finais pelo agente de classes)

| Primo (elemento) | G4 (150) — capital do elemento | G5 (350) |
|---|---|---|
| Maithe (Vento Vivo) | **Sopro de Farnkesh** (Picos de Farnkesh, mapa 54) | **Vento da Coluna** |
| Maria (Luz) | **Lâmpada de Mingcheng** (mapa 41) | **Aurora da Coluna** |
| Joaquim (Água Viva) | **Raiz de Jhilpur** (mapa 31) | **Rio da Coluna** |
| Ícaro (Fogo Vivo) | **Forja de Hinokura** (mapa 65) | **Engenho da Coluna** |
| Lavine (Ar Sereno) | **Lua Branca** (Porto da Lua Branca, mapa 59) | **Silêncio da Coluna** |

- **G4** dá: o buff de grupo do tier 1 com raio 10 → 14 e a skill do tier 2 com um efeito secundário de controle
  (sem dano a mais). **G5** dá: uma 2ª skill de maestria em área (recarga 30 s) **com o mesmo orçamento** da do tier 3
  (§6 de ClassEvolution: entra no `power_stack.py` como mais uma skill de rotação). As missões usam o mesmo fluxo
  `ClassEvolutionRules` (Accept → RecordKill → TurnIn).
- G4 e G5 ficam **fora do escopo "até 100"** (§5); entram com os mapas.

### 3.3 Nível de Mestre

- Abre no **500** com a G5. `MaxMasterLevel` 30 → **100** (MU tem 200+; 100 é o bastante e cabe numa tela).
  XP `25 000 × (1 + 0,15 × nível)` vira `xp(500) × 0,5 × (1 + 0,05 × nível)` (≈ 10,7 milhões no mestre 1): a barra de
  mestre tem o mesmo "peso" de um nível do fim do jogo.
- Árvore de Mestre: as 6 linhas de hoje (30 pontos) ficam; somam-se **3 linhas novas** (Apoio, Utilidade e "Laço",
  bônus para a montaria e o Ari, nunca dano) até 100 pontos. Teto de orçamento continua o do ClassEvolution §6:
  "árvore cheia por cima: chefe ≥ 95% de sem ela".

---

## 4. Orçamento de poder: nível do bicho e escala

### 4.1 A ideia

Hoje o orçamento (`tools/balance`, README §10/§14/§25/§27) mede **4 biomas** contra o **herói de referência** do
nível do chefe (3 / 7 / 10 / 15). Para 500 níveis isso vira uma **função do nível**:

1. Todo bicho ganha `nível` (o do mapa; comum = faixa do mapa, chefe = topo da faixa + 2).
2. O modelo calcula o **herói de referência** em qualquer nível `L` (curva + atributos gulosos + pontos de skill +
   equipamento médio da faixa + graduações já liberadas + roda de talentos), igual faz hoje para 3/7/10/15.
3. Vida e dano do bicho saem de um **arquétipo** (os números da campanha, nível 15) multiplicados pela razão entre o
   herói de referência no nível do bicho e o do nível 15. Assim **as metas não mudam em nível nenhum**:
   comuns 4–7 s, chefes 15–22 s, o chefe mata o herói parado em 12–40 s, só no ataque básico o chefe vence, pilha
   completa ≤ 22% mais rápida no chefe.

```
vida(M)  = vida_arquétipo × Rd(M) / Rd(15)
dano(M)  = dano_arquétipo × Rh(M) / Rh(15)
Rd(L) = dano por segundo do herói de referência no nível L     (o modelo mede; proposta inicial abaixo)
Rh(L) = vida efetiva do herói de referência no nível L
Proposta inicial (para os assets antes do modelo rodar):
Rd(L) = (1 + 0,02·(L−1)) × (1 + 0,01·(L−1))       ← damage_mult × (atributos + equipamento por faixa)
Rh(L) = (1 + 0,08·(L−1)) × (1 + 0,005·(L−1))      ← curva de vida × (Vitalidade + defesa por faixa)
```

| Nível do bicho | ×vida | ×dano | Chefe-arquétipo (Tages: 1 180 / 42) | Comum-arquétipo (Morcegos: 170) | Vida do herói (curva) |
|---|---|---|---|---|---|
| 15 | 1,00 | 1,00 | 1 180 / 42 | 170 | 212 |
| 25 | 1,26 | 1,44 | 1 484 / 61 | 214 | 292 |
| 50 | 2,02 | 2,70 | 2 386 / 113 | 344 | 492 |
| 100 | 4,06 | 5,88 | 4 796 / 247 | 691 | 892 |
| 150 | 6,79 | 9,94 | 8 014 / 417 | 1 155 | 1 292 |
| 200 | 10,2 | 14,9 | 12 041 / 625 | 1 735 | 1 692 |
| 300 | 19,1 | 27,4 | 22 521 / 1 151 | 3 245 | 2 492 |
| 400 | 30,7 | 43,5 | 36 236 / 1 826 | 5 220 | 3 292 |
| 500 | 45,1 | 63,1 | 53 186 / 2 648 | 7 662 | 4 092 |

Os números cabem folgados em `float`/`int` (o maior golpe fica na casa dos milhares). Abaixo do 15 a tabela não é
usada: os 4 biomas da campanha **ficam com os números de hoje** (r8/r9), o que garante que o 1–15 não muda.

### 4.2 Por que funciona com o modelo de hoje

- O `power_stack.py` hoje roda "bioma × ciclo". Ele ganha um eixo **"nível do mapa"**: para cada mapa do
  `Mapas100.md` (ou só um a cada 10 níveis, para ser rápido) roda o mesmo Monte Carlo com o herói de referência
  daquele nível e confere **as mesmas invariantes** (`power_stack.violations`). Fonte nova de poder (G4, G5, anel
  novo, Despertar, mestre) entra igual às de hoje: lida do C#, testada no pytest.
- Como a escala é **relativa ao herói de referência**, uma fonte de poder que cresce com o nível (atributos, vida,
  `damage_mult`) é absorvida pelo bicho; o que o orçamento vigia é o que **não** cresce (refeição +8%, relíquia +1
  plano, asas, refino). Detalhe importante: **bônus planos** (Canivete +1, afixo Afiada 1–3) perdem peso nos níveis
  altos; bônus **percentuais** mantêm. Para o equipamento de faixa alta, os afixos planos devem rolar **por faixa**
  (`valor × Rd(L)/Rd(15)`), senão o loot do nível 300 não vale nada.
- **Arquétipos por mapa:** cada mapa do `Mapas100.md` diz o arquétipo (rápido-frágil, tanque, enxame, à distância,
  elite). Os 9 inimigos de hoje viram os 9 arquétipos-base; bicho novo = arquétipo + visual + uma regra especial.

### 4.3 NG+ num jogo de 500 níveis

- A **Nova Jornada+** continua sendo "rejogar a campanha" (os 4 biomas da campanha, mapas 01, 02, 07, 08, 13, 14,
  19, 20) com o ciclo de hoje (README §11), mas com um **piso de nível**: na NG+ `c`, o bicho da campanha tem nível
  `max(nível de hoje, 15 + 20·c)` (NG+5 → nível 115) e usa a escala do §4.1 no lugar dos multiplicadores fixos.
  Assim um herói 200 que entra na NG+ não atropela tudo, e a NG+ vira o jeito de rever a história com recompensa.
- No escopo "até 100" (§5) a NG+ **fica como está** (multiplicadores de hoje); o piso entra com o teto 500.

### 4.4 Montarias, despertares e mestre no orçamento

- Montaria **nunca dá dano** (Mounts §2.2) nem velocidade a mais; o Despertar idem. Ela entra no orçamento só pelo
  lado defensivo (vida que divide o dano), e a regra "parado, o chefe mata em 12–40 s" passa a ser medida **montado
  e a pé** nos mapas abertos (offline, a arena continua a pé, então nada muda no NG+0).
- A vida da montaria precisa acompanhar o cavaleiro (senão, no 300, um golpe derruba a montaria): ver §6.3.

---

## 5. Escopo "até 100": o que se constrói agora e o que fica para depois

| Agora ("até 100") | Depois (101–500) |
|---|---|
| Teto **100** no `ProgressionCurve` (`DefaultMaxLevel` 50 → 100); XP acima do teto fica **guardado** até 1 nível ("barra cheia") e entra quando o teto subir | teto 500, Mestre no 500 |
| Curva 51–100, XP por abate, diferença de nível (§1) | parede `W(n)` do 300+ |
| Nível de bicho + escala relativa (§4.1) e o eixo "nível do mapa" no `tools/balance` | piso de nível da NG+ (§4.3) |
| **Mapas 01–42** do `Mapas100.md` (os 4 biomas da campanha com 6 mapas cada, Costa das Monções, Delta de Jhilpur, Sangkent, o começo de Mingcheng) | mapas 43–100 em ondas de 6 |
| **Cidades:** Vila da Vó Edna, Qocho, Litema, Lamparinas, Taa Pwani, Jhilpur, Sangkent, Mingcheng + as cidades de raça 05, 11, 17, 22, 26, 28, 33, 42 | Farnkesh, Porto da Lua Branca, Hinokura, Cais de Mesektet, Dur-Jarras, Fortaleza da Última Lâmpada, Cais da Coluna + raças 47, 49, 50, 53 |
| Pontos (§2) até o 100; Roda +1/10 níveis depois do 50 | 4º anel (150), 2ª pedra-chave (350) |
| G1–G3 como hoje | G4 (150), G5 (350), Mestre (500) |
| **Montaria: teto pelo herói (§6.1), XP de cavalgar e lutar (§6.2), crescimento por raridade (§6.3), Despertar I (§6.4)** | Despertar II–IV |
| **Respawn sempre na cidade (§7)**, já para os 4 biomas de hoje | — |
| Chefes de mapa 1–42 e os chefes do mundo de hoje (Saklas, Namrael, Mazan, Mahaway) + Hedammu (30), Aydahar (40) | chefes do mundo 46+ e a raide "A Grande Treva" (90/100) |

Ordem sugerida dentro do "até 100": (1) respawn na cidade e teto 100 com a curva (rápido, sem conteúdo novo);
(2) nível do bicho + escala + eixo no `tools/balance`; (3) montaria ligada ao herói; (4) mapas 03–06, 09–12, 15–18,
21–24 (o "segundo anel" dos biomas de hoje, nível 16–50); (5) Costa das Monções e Jhilpur (45–90); (6) Sangkent e
Mingcheng (70–102); (7) Despertar I.

---

## 6. Montarias que crescem com o herói (teto 20)

### 6.1 Teto de nível da montaria = função do nível do herói

```
teto_da_montaria(h) = clamp( floor(2 × √h), 1, 20 )      h = nível do cavaleiro (o primo que monta)
```

| Herói | 1 | 4 | 9 | 16 | 25 | 36 | 49 | 64 | 81 | **100** | 100+ |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Teto da montaria | 2 | 4 | 6 | 8 | 10 | 12 | 14 | 16 | 18 | **20** | 20 |

- **Rápido no começo** (criança vê a montaria subir logo: herói 15 no fim da campanha → teto 7) e **completa no 100**,
  o mesmo marco do Despertar I. Depois do 100 a montaria não sobe mais de nível: cresce pelos **Despertares** (§6.4).
- **Qual herói conta:** o primo que está montando. Offline os 5 primos dividem a coleção (Mounts Decisão 5): a
  montaria guarda o nível dela; se um primo de nível baixo monta uma montaria nível 18, ela **funciona no teto dele**
  (o "nível efetivo" = `min(nível, teto(h))`) e não perde nada. Online (por personagem) é o nível do personagem.
- **Teto atingido:** o XP que sobra vai para o **Brilho** (barra de 0 a 5 000, o "XP guardado" da montaria). Quando o
  herói sobe e o teto libera, o Brilho entra primeiro. No 20, o Brilho cheio é **requisito do Despertar** (§6.4).
- A curva de hoje fica: `xp_para_o_próximo = 60 + 20 × nível` (total 4 940 até o 20).

### 6.2 De onde vem o XP da montaria

| Fonte | XP | Nota |
|---|---|---|
| Cavalgar | 1 por 20 m (como hoje, Mounts §4.1) | o servidor conta a distância validada |
| **Lutar com o herói** | **2 + 1% do XP do abate**, teto **30 por abate** | montada ou "por perto" (offline: esperando na beira da arena, no lugar dos +5 por luta de hoje; online: a até 30 m do herói) |
| Chefe de mapa / do mundo | 40 / 120 | uma vez por chefe por dia |
| Comida (Mounts §4.2) | a da tabela de hoje (Nan +40, Plov +60, Torrão Dourado +20...) | não muda |
| Aulas de doma (Srosh, Estrebaria) | 100 por aula, 1 por dia | nova, 10 ouro |

Ritmo: no começo (abate de 10 XP) a luta dá ~2 por abate e o grosso vem de andar e comer, como hoje. No nível 50
(abate de 128) dá ~3 por abate, ~600 por hora de caça: a montaria acompanha o herói sem esforço e **o teto é o que
segura**, que é a intenção do pedido. O teto de 30 por abate evita que um abate do nível 300 encha a barra inteira.

### 6.3 Crescimento por nível, por raridade

Hoje (Mounts §4.1) todas crescem igual: vida +5% da base por nível, fôlego +1, recarga −0,5%. Proposta: a raridade
muda **só o ritmo da vida e do fôlego** (a Rara fica exatamente como hoje); velocidade e defesa **nunca** sobem.

| Raridade | Vida por nível (da base) | Vida no 20 | Fôlego por nível | Fôlego extra no 20 | Recarga da habilidade por nível | No 20 |
|---|---|---|---|---|---|---|
| Comum | +4% | ×1,76 | +1 | +19 | −0,5% | −9,5% |
| Incomum | +4,5% | ×1,86 | +1 | +19 | −0,5% | −9,5% |
| Raro | +5% (hoje) | ×1,95 | +1 (hoje) | +19 | −0,5% | −9,5% |
| Épico | +5,5% | ×2,05 | +1,5 | +28 | −0,5% | −9,5% |
| Lendário | +6% | ×2,14 | +2 | +38 | −0,5% | −9,5% |

**Vida acompanha o cavaleiro (novo):** a vida da ficha é a do "nível 15". Em combate ela é multiplicada por
`Rh(h)/Rh(15)` (§4.1, mínimo 1): o mesmo fator que faz o dano dos bichos crescer. Assim a montaria protege a mesma
**fração** do herói no nível 20 e no 400 (sem isso, no 300 um golpe a cansaria) e o orçamento não muda de forma.

**Orçamento (Mounts §2.4):** os pontos de raridade (8/12/16/21/26) contam **no nível 1**, como hoje. O crescimento
é igual dentro da raridade, então "nenhuma montaria é melhor em tudo" continua valendo. O ganho de vida extra das
raridades altas (×2,14 contra ×1,76) é defensivo e pequeno perto da vida do herói: passa no "parado, o chefe mata em
12–40 s" com folga (o README §14 mostra que o lado defensivo não quebra nada). O `power_stack.py` deve somar a
montaria mais defensiva (Ijapa Lendária, nível 20, Despertar IV) só nos mapas abertos.

### 6.4 Despertar (herói 100 / 200 / 300 / 500)

O Despertar é a "evolução" do pedido: **o nível nunca passa de 20**, mas a montaria muda de cara e a habilidade fica
melhor. Feito na **Estrebaria do Srosh** (Vila) ou na estrebaria de qualquer capital, com uma missão curta do Srosh
("ela já ouve você; agora vamos ver se você ouve ela").

| Despertar | Pede | Visual | Habilidade e ficha | Nunca |
|---|---|---|---|---|
| **I — Faísca** | herói **100**, montaria 20, Brilho cheio, **3 corações**, 20 Pedras-Faísca + 5 partes de bicho do mapa onde ela foi achada | aura suave na cor do **elemento do cavaleiro** (lilás, branco-azulado, dourado, verde-água, laranja; §1.3 da bíblia), olhos com brilho | habilidade **+1 grau** (tabela abaixo); vida +5% | velocidade, defesa, dano |
| **II — Chama** | herói **200**, Despertar I, Brilho cheio de novo, **4 corações**, 40 Pedras-Faísca + 1 Pedra da Alma Viva | **detalhe novo de modelo** pela espécie (crina de luz, chifre de cristal, penas de brasa, casco de vidro) | habilidade ganha um **efeito secundário** de utilidade (ex.: Arrancada deixa rastro que tira lentidão do grupo); fôlego +10 | idem |
| **III — Coluna** | herói **300**, Despertar II, **5 corações**, 1 Pedra da Mistura + item do chefe do mundo da região de origem | um fio de luz sobe da montaria quando ela para (a Coluna da Glória "em miniatura") | **passivo de grupo fora de combate**: aliados a pé num raio de 8 m ganham +5% de velocidade de andar (não soma com montaria, Mounts §2.3) e a habilidade tem −10% de recarga | idem |
| **IV — Forma de Luz** | herói **500**, Despertar III, **5 corações** e 1 000 km montado com ela | **forma translúcida de luz** do elemento (liga e desliga no estábulo), passo deixa faíscas | **2ª carga** da habilidade (duas seguidas, mesma recarga total) e o título "<apelido>, a Desperta" | idem |

**"+1 grau" por tipo de habilidade** (`MountAbilityDefinition._kind`): deslocamento (Dash, Blink, Burrow) +15% de
distância até o teto do servidor (`_serverMaxDistance`); duração (Glide, Stealth, CCImmunity, WeatherImmunity,
Invulnerable) +15%; raio (HealAura, RepelRing, PartyBuff, AreaStun, Fear) +1 m; magnitude (HealAura, PartyBuff) +10%
(continua dentro do orçamento da raridade porque é aplicada **depois** do teto do `_bossMultiplier`); voo (ShortFlight,
FreeFlight) +15% de duração, **nunca** altitude ou velocidade.

**Como se encaixa no §4 do Mounts.md:**
- **Alimentação:** igual. Comida dá XP; no teto, o XP vira **Brilho**. A fome (×1,15, sem galope) vale também para a
  montaria desperta.
- **Afeição:** continua dando **conforto, não poder** (1–5 corações). O que muda: os corações viram **requisito** do
  Despertar (3 / 4 / 5 / 5). Isso dá sentido a cuidar da montaria depois do nível 20.
- **Favorito do primo** (Mounts §8): afeição 25% mais rápida, como hoje; o Despertar na cor do elemento do favorito
  ganha uma fala especial.
- **Montaria cansada:** igual (120 s / 2 lutas). Desperta ou não.
- **Orçamento:** nenhum Despertar dá velocidade, defesa ou dano. A única coisa de combate é o "+1 grau" e a 2ª carga;
  as de controle já são cortadas pela metade em chefe (`_bossMultiplier`) e medo/empurrão não valem em chefe.

### 6.5 Save e servidor

`OwnedMount` ganha `int glow` (Brilho, 0–5 000), `int awakening` (0–4) e `float kmRidden` (já dá para tirar do XP de
andar, mas o campo deixa o Despertar IV simples). Save antigo: `glow = 0`, `awakening = 0`, e **o nível salvo fica**
(uma montaria nível 20 de um herói nível 10 continua 20; só funciona no teto do herói até ele crescer, §6.1). O
servidor valida o teto pelo nível do personagem e o Despertar pelos mesmos números puros (`MountRules`).

---

## 7. Respawn: sempre na cidade

### 7.1 A regra

> **Caiu, acorda na praça da cidade.** Nunca no lugar onde caiu, nunca num checkpoint da arena, nunca "na porta do
> chefe". A cidade é a da **cidade de retorno do mapa** (cada mapa do `Mapas100.md` diz qual é; nos 4 biomas de hoje
> é a cidade da região). Postos avançados (zona segura sem cidade) **não** são ponto de respawn: servem para
> descansar, vender e consertar.

- Fala ao acordar (Ari, o aralez, §1.7 da bíblia): o Ari lambe a mão do primo na praça ("Ele te achou de novo."). Ótima
  ponte com o mito dos aralezes que levantam os heróis caídos.
- A **Barca de Luz** (viagem rápida) leva da cidade até o **cais do mapa** (cada mapa grande tem um cais na entrada),
  pela tarifa de hoje (2 ouro por estrada). Voltar a pé é o custo de tempo; voltar de barca é o custo de ouro. Isso é o
  peso novo da morte; por isso a perda de XP fica pequena (§7.2).
- **Chefe:** perder para o chefe do mapa também manda para a cidade. O chefe volta à vida cheia se ninguém estiver
  lutando com ele (online: 30 s sem jogador na arena dele).
- **Salas de evento** (Pátio dos Mazans, Torre das Jarras): cair = sair da sala; a medalha conta o que já foi feito.

### 7.2 Como fica com o que já existe

| Sistema | Hoje | Com o respawn na cidade |
|---|---|---|
| Queda de 9% (README §18.5) | rola na tela de morte; offline sai de vez | **igual**. Offline: a unidade some (decisão do usuário). Online: **fica no chão onde o herói caiu** (§7.3). |
| Perda de XP (§19.1) | 5% do `xp_to_next`, nunca desce de nível, sem perda abaixo do 3 e no chefe da NG+0 | 5% até o 100; **4%** 101–200; **3%** 201–300; **2%** 301–500 (assim a perda fica em ~7–13 abates em qualquer nível, em vez de crescer com a curva). As carências ficam. |
| Lâmpada Guardiã (ex-Bênção) | −60% da perda e 0% de queda; preço 5 + nível, teto 60 | igual; o **teto do preço** sobe para `60 + nível/2` acima do 100 (rever com a economia de ouro dos mapas novos) |
| Tumar da Vó Edna (ex-Patuá) | gasta-se ao morrer, protege tudo | igual |
| Durabilidade (§25.1) | −10% do máximo de tudo o que está vestido | igual. A cidade tem ferreiro, então o conserto fica no caminho. |
| Montaria | — | se o herói cai **montado**, a montaria fica **cansada** e vai para casa (Mounts §2.1); ele acorda a pé na praça |
| Debuffs | — | a entrada na cidade já limpa veneno, lentidão e atordoamento (`ShouldClearDebuffsOnEntry`) |
| Vida e mana | revive no lugar | acorda com **vida e mana cheias** (a praça é zona segura) |

### 7.3 Online (FASE 12)

- O servidor decide a cidade (mesma função pura) e manda o cliente para a praça.
- **O item que caiu fica no chão onde o herói caiu** (já decidido: qualquer jogador pode pegar). Como o dono agora
  acorda longe, a proposta muda: **prioridade do dono 90 s** (era 10 s) e o item some do chão em **5 min** (era 2 min).
  O dono vê um **marcador no mapa** ("deixou cair aqui") até o item sumir ou ser pego. A Barca até o cais do mapa
  costuma levar menos de 90 s.
- Em grupo (party), o item que caiu também é visível para o grupo como "do fulano" (só o dono pega na prioridade).
- Sem "ressuscitar no lugar" por skill de jogador nesta fase (a Maria cura, mas não levanta quem caiu). Se um dia
  existir, fica restrito a eventos e raides.

---

## 8. O que muda no código (lista para os implementadores)

**Progressão**
- `ProgressionCurve`: fórmula por partes (§1.1), `DefaultMaxLevel` 50 → **100** (escopo) → 500; "XP guardado" no
  teto de conteúdo. `progression.csv` + `ProgressionCurveTests` com as linhas novas (regenerar e exportar).
- XP acumulado como `long` onde houver soma (§1.3).
- `EnemyDefinition`: campo `_level` e `_archetype`; `EnemyLevelScaling` (puro): `HealthFor(arquétipo, nível)`,
  `DamageFor(...)`, `KillXp(nível, rank)` e `LevelDifferenceXpMultiplier(herói, bicho)`. `EnemyRewardGranter` usa o
  gancho `IKillExperienceScaler` que já existe (as salas de evento já usam).
- `NewGamePlusRules`: no escopo "até 100" **não muda**; com o teto 500, piso de nível (§4.3).
- `AttributeSet`/`SkillManager`: pontos de skill 1/5 níveis depois do 50. `TalentWheelRules`: +1/10 níveis depois do 50.
- `tools/balance`: eixo "nível do mapa" no `power_stack.py` e no modelo do herói de referência; pytest novo
  `test_level_scaling.py` (as invariantes de hoje nos níveis 25/50/75/100).

**Classes**
- `ClassEvolutionCatalog.MaxTier` 3 → 5 (G4/G5 depois do escopo), `MasterLevelRules.RequiredLevel` →
  `ProgressionCurve.MaxLevel` (não um 50 fixo), `MaxMasterLevel` 30 → 100 quando o teto for 500.

**Montarias**
- `MountRules`: `LevelCapFor(heroLevel)`, `EffectiveLevel(level, heroLevel)`, `CombatXp(killXp)`, `GlowAfter(...)`,
  `HealthGrowthFor(rarity)`, `StaminaGrowthFor(rarity)`, `RiderHealthScale(heroLevel)`, `CanAwaken(...)`,
  `AwakenedAbility(...)`. `MaxHealthAt`/`MaxStaminaAt` passam a receber a raridade. `MaxLevel = 20` **fica**.
- `OwnedMount`: `glow`, `awakening`, `kmRidden` (§6.5). `MountCollection` ouve o abate do herói para o XP de luta.
- Textos do Despertar e da missão do Srosh (`MountTexts`), visual por espécie (`MountWaveDecor` + aura por elemento).

**Respawn**
- `SafeZoneRules.RespawnRegion(BiomeType, hasCity, fallback)` → **`RespawnCity(mapId, returnCityOf, fallback)`**
  (puro; o mapa diz a cidade). Enquanto só existirem os 4 biomas, `mapId` = o bioma e a cidade = a da região (o que já
  acontece hoje no mapa-múndi).
- `CityProgress.ApplyDefeat`: passa a ser chamado para **toda** derrota (arena, cidade, mapa aberto), não só para o
  `EncounterEndedEvent` do mapa; marca a cidade como visitada e deixa o aviso (`PendingRespawnCity`).
- `DeathScreenController`: "Continuar" deixa de chamar `RespawnViaCheckpoint`; pede a volta para a cidade
  (`SceneTransitionService.BeginCityVisit(cityId)` ou mapa-múndi com o token na cidade) e só então revive.
  O caminho "revive no lugar" fica só como rede de segurança de cena de teste (sem `CityProgress` registrado).
- `CheckpointManager`: na cena de combate, `DeferRespawn` sempre ligado; o checkpoint vira só **ponto de entrada** da
  cena. `SpawnPoint` (auto-ativa checkpoint) deixa de mudar o lugar de respawn. Na cidade, o `CityModeController`
  continua marcando a praça (`Town.Respawn`): é o único "checkpoint de morte" que sobra.
- Arena da campanha (`Main.unity`) e salas de evento: derrota → cidade da região (hoje "volta ao mapa sem perda").
- `DeathXpLossRules`: percentual por faixa de nível (§7.2). `DeathPenaltyTests`, `CityTests` (renascimento) e os
  testes da tela de morte atualizados.
- Online: `ItemDroppedOnDeathEvent` vira drop no chão com prioridade de 90 s e 5 min de vida (§7.3).

---

## 9. Decisões para o usuário

**Decidido pelo usuário em 2026-10-30:** curva como proposta (~45 h até o 100, ~860 h até o 500); teto da montaria
`⌊2·√herói⌋` (20 no herói 100); renascer **só nas cidades** (posto avançado nunca é respawn); perda de XP diminui por
faixa (5% → 2%). Itens 3, 4 e 6 ficam com a proposta (4 Despertares; G4 no 150, G5 no 350, Mestre no 500; online 90 s
de prioridade e 5 min no chão) até o usuário dizer o contrário.

Perguntas originais:

1. **Curva:** ~45 h até o 100 e ~860 h até o 500 de jogo ativo. Está bom, mais rápido (público infantil) ou mais
   "MU" (mais lento)? O botão é o expoente da cúbica (3) e a parede `W(n)`.
2. **Teto da montaria:** `⌊2·√herói⌋` (20 no herói 100) ou linear (`1 + ⌊herói/5⌋`, 20 no herói 95, mais lento no
   começo)?
3. **Despertares:** 4 (100/200/300/500) ou 5 (com um no 400)?
4. **G4 e G5 no 150 e no 350**, Mestre no 500: ok? (Alternativa MU pura: G4 no 150, G5 no 400, Mestre no 400.)
5. **Respawn:** postos avançados nunca são respawn (só cidades). Ok, ou o posto com Nicho da Lâmpada também vale?
6. **Online:** prioridade do dono de 90 s e 5 min no chão para o item que caiu, com marcador no mapa.
7. **Perda de XP por faixa** (5% → 2%) para a morte não pesar mais nos níveis altos: ok?
