# Montarias — Rapadura

> Design e catálogo das montarias. Pedido do usuário (2026-10-29): "vários tipos de montaria, unicórnio, pônei,
> cachorro e uns 20 animais mitológicos, tigre, mamute, cada um com habilidade especial e velocidade, defesa e
> atributos diferentes; umas compradas em loja, outras dropadas, outras muito raras".
> **Só documentação.** Nada daqui existe no código ainda. Números conferidos com o código e os dados de hoje
> (`PlayerMotor`, `PlayerStats`, `BuffController`, `DamageCalculator`, `StunRules`, `ItemRarity`, `ShopDefinition`,
> `Docs/Balance/README.md` §4, §7, §8, §10 e §11, `Docs/Housing/CatalogoCasa.md`).
> `Docs/Housing.md` ainda não existe (outro agente está escrevendo). O estábulo (§7) usa só o que o
> `CatalogoCasa.md` já mostra: os níveis da casa e os itens do curral. Quando o Housing.md sair, confira §7 e §9 com ele.

**Total: 30 montarias**, sendo 20 mitológicas ou do folclore e 10 bichos "de verdade" (alguns gigantes).

| Raridade (`ItemRarity`) | Montarias | Como se consegue |
|---|---|---|
| Comum | 6 | loja, com ouro (uma também é presente de missão) |
| Incomum | 8 | 2 na loja, 3 por drop de bicho comum (0,4%), 1 por drop de chefe, 2 por missão ou conquista |
| Raro | 8 | drop de chefe (1,5–2%), drop raríssimo de bicho comum (0,1–0,15%), missão ou evento |
| Épico | 5 | drop de chefe (0,8%), segredo do mapa, conquista longa ou evento (prendas) |
| Lendário | 3 | drop de chefe (0,2–0,3%) ou conquista muito longa. **Nunca vendido**, nunca trocado |

| Tipo | Montarias |
|---|---|
| Terrestre | 21 |
| Voador | 5 |
| Aquático | 2 |
| Escavador | 2 |

---

## 1. Números de partida (do código de hoje)

| O quê | Valor hoje | Onde |
|---|---|---|
| Andar / correr / agachado | 2,5 / 5,5 / 1,4 m/s | `PlayerMotor._walkSpeed/_runSpeed/_crouchSpeed` |
| Fôlego do herói | 100, gasta 15/s correndo, volta 10/s, mínimo 5 para correr | `PlayerStats` |
| Vida do herói | 100 no nível 1, 172 no 10, 292 no 25, 492 no 50 | `progression.csv` |
| Multiplicador de velocidade | `GetModifiedValue(MoveSpeed, 1)` com limite **[0,1; 2,5]** | `BuffController.GetMoveSpeedMultiplier` (README §4.2) |
| Defesa percentual | soma das fontes, limitada a **[−1; +0,8]** | `DamageCalculator` (README §4.1) |
| Atordoamento em chefe | ×0,5 | `StunRules` (README §7.1) |
| Ouro por luta | ~1,8 (`item_gold` 1–3 a 60%, 1,5 bicho por luta); chefe 4–16 | README §8, §10.2 |
| Faixas de preço da casa | Comum 8–30, Incomum 35–90, Raro 100–250, Épico 300–700 | `CatalogoCasa.md` |
| Moeda de evento | prendas | `CatalogoCasa.md`, `CatalogoRoupas.md` |
| Atributos | Vitalidade, Espírito, Destreza, Inteligência, Poder Arcano (não existe Força) | README §9 |

Regra de ouro das montarias: **a velocidade montada nunca passa de 2,16× a corrida** (5,5 × 1,80 × 1,20 de galope
= 11,9 m/s). Isso cabe dentro do teto de 2,5 que o `BuffController` já tem, então nenhum teto do jogo precisa mudar.

---

## 2. Sistema

### 2.1 Chamar e descer

- **Chamar (assobio):** segurar a tecla da montaria (ou o botão de toque) por **1,5 s**. O primo assobia, a
  montaria chega correndo (voando, nadando ou saindo da terra, conforme o tipo) e ele sobe. A conjuração **quebra**
  se o herói levar dano, atacar, usar skill ou pular.
- **Fora de combate:** só dá para chamar com **5 s sem dar nem levar dano** (o mesmo "em combate" que o online vai
  usar para a zona segura, FASE 12.3a).
- **Descer:** na hora, com a mesma tecla. Depois de descer, **10 s** de recarga para chamar de novo (não dá para
  ficar sobe-e-desce para fugir).
- **Trocar de montaria:** só pelo estábulo da casa ou pela estrebaria da cidade (§7). No campo você chama a
  "montaria ativa".
- **Montaria cansada:** se a vida da montaria chegar a 0 (§2.3), o primo desce rolando no chão sem se machucar, e
  a montaria **volta sozinha para casa para descansar**. Ninguém morre: ela fica "cansada" por **120 s** (offline:
  2 lutas). Um Chá de Capim-Santo do tratador corta o descanso na hora por 5 ouro (§5.3).
- **Offline (antes do online):** a montaria vale no **mapa do mundo (`WorldMap.unity`) e na Vila**. O encontro na
  arena (`Main.unity`) começa sempre a pé. A montaria espera na beira da arena e volta com você quando a luta
  acaba. Assim, o combate de hoje e o balanceamento do README não mudam nada na Fase M1.

### 2.2 Combate montado

Na Fase M1 (offline) não se luta montado: a arena começa a pé. As regras abaixo entram com os mapas abertos (Fase M4).

| Ação montado | Pode? | Regra |
|---|---|---|
| Ataque básico | Sim | −20% de dano ("montado a mira balança"). **Montaria de guerra** (coluna própria no §3) não perde dano |
| Skill instantânea ou projétil (flecha, raio, granada, cura, buff) | Sim | custo e recarga normais |
| Skill canalizada, invocação (drone, torreta, exército), transformação, dash/teleporte do herói | Não | usar **desmonta** primeiro e a skill sai a pé (não perde a conjuração) |
| Habilidade especial da montaria | Sim | uma por montaria, recarga própria (§3) |
| Levar dano | Sim | não derruba. Metade do dano vai para a vida da montaria (§2.3) |
| Pular | Sim | o pulo é da montaria (o Bode tem pulo duplo, por exemplo) |
| Coletar, conversar, abrir baú | Sim | sem descer |

O efeito da montaria de **atordoar** obedece o `StunRules` (chefe leva metade). Efeitos de **medo/empurrão** não
valem em chefe. As habilidades da montaria **não dão dano de verdade**: o máximo é um dano leve (Ema de Fogo,
Boitatá, Dragão Filhote), sempre menor que um ataque básico por segundo. A montaria é para viajar, fugir, explorar e
ajudar o grupo. Quem mata o bicho é o primo.

### 2.3 Atributos de cada montaria

| Atributo | Faixa | Como funciona |
|---|---|---|
| **Velocidade** (× corrida) | 1,30–1,80 | multiplica os 5,5 m/s da corrida. Pônei 1,45 = 8,0 m/s; Sleipnir 1,80 = 9,9 m/s. **Não sobe com nível** (o servidor compara com um teto fixo por espécie) |
| **Galope** | ×1,20 | correr montado. Gasta o fôlego **da montaria**: 12/s, volta 15/s quando para de galopar |
| **Fôlego** | 100–200 | fôlego da montaria para o galope (100 = 8,3 s de galope) |
| **Defesa** (+%) | 0–15% | entra como `Defense` **PercentAdditive** no `BuffController` (fonte `mount`), igual às skills. Soma com o resto e o teto de 80% continua |
| **Vida** | 100–375 (nível 1) | **metade** do dano que o primo leva (depois da defesa) sai da vida da montaria em vez da dele. Montaria a 0 = cansada (§2.1). Sobe com nível (§4.1) |
| **Habilidade especial** | 1 por montaria | efeito único e recarga própria (§3) |

**Velocidade e buffs:** montado, os buffs de `MoveSpeed` **acima** de 1 são ignorados (o Passo do Vento e o dash
não somam com a montaria). As **lentidões continuam valendo** (Congelado, Correntes): quem está lento anda lento,
montado ou não. Assim não existe combinação que passe do teto de 11,9 m/s.

**Fome:** abaixo de 30 de saciedade a montaria anda a **×1,15** e não galopa (§4.2). Ela nunca para de andar.

### 2.4 Orçamento de poder

Cada montaria gasta pontos de atributo conforme a raridade. A base (0 ponto) é velocidade 1,30, defesa 0%, vida 100
e fôlego 100:

```
pontos = (velocidade − 1,30) / 0,05  +  defesa%  +  (vida − 100) / 25  +  (fôlego − 100) / 10
```

| Raridade | Pontos de atributo | Peso da habilidade |
|---|---|---|
| Comum | **8** | utilidade simples (um movimento ou um sentido) |
| Incomum | **12** | utilidade com escolha (furtividade, imunidade curta, rastro) |
| Raro | **16** | efeito de grupo ou de combate leve (cura, atordoar, planar) |
| Épico | **21** | muda o caminho (voo livre, teleporte curto, buff de grupo) |
| Lendário | **26** | um "milagre" por cena (renascer, invulnerável, sopro) com recarga longa |

Toda montaria do §3 bate **exatamente** no orçamento da raridade dela (conferido por script). O formato muda: o
Sleipnir põe quase tudo em velocidade, o Jabuti Ancião em defesa e vida. Mais raridade dá mais pontos no total, mas
nenhuma montaria é melhor que outra em tudo. A Onça de Luz (Rara) é mais rápida que o Boi-Bumbá (Épico), por exemplo.

### 2.5 Cidade (zona segura), arena e mapas abertos

| Lugar | Montaria |
|---|---|
| **Cidade / zona segura** (FASE 12.3a) | pode andar montado, mas só no **trote** (×1,00, sem galope) para não atropelar a multidão. **Nenhuma habilidade** funciona, só a pose de exibição (§6). Montarias não colidem com outros jogadores (atravessam) |
| Dentro de casa | não entra. Desce sozinho na porta, e a montaria vai para a baia do estábulo |
| Estrebaria da cidade | NPC tratador em cada cidade: troca a montaria ativa, guarda as que não cabem na casa, vende ração |
| **Arena de encontro (offline)** | começa a pé (§2.1) |
| **Mapas abertos (online)** | combate montado do §2.2 |
| **Arena PvP ranqueada** | sem montaria (justiça entre jogadores) |
| Duelo consentido | os dois escolhem antes ("com montaria" ou "a pé") |
| Eventos agendados (Noite do Lobisomem, Cerco ao Engenho) | cada evento decide. Padrão: montaria sim, voo **não** |
| Sala de chefe / chefe mundial | entra montado, mas o voo é cortado (voador vira terrestre). Habilidades de controle valem a metade no chefe |
| Divisa da zona segura | quem entra em combate na cidade desce da montaria e só chama de novo depois de 5 s fora de combate (evita usar a cidade de esconderijo galopando) |

### 2.6 Tipos

| Tipo | O que muda sempre (passivo) |
|---|---|
| **Terrestre** | anda no chão. Para em água funda (desce e o primo nada) |
| **Voador** | flutua 1 m acima do chão: atravessa água rasa e buracos pequenos, sem dano de queda. **Voar de verdade só com a habilidade** (Pégaso, Grifo, Hipogrifo) |
| **Aquático** | nada em água funda na velocidade cheia. Na terra anda a ×0,9 da velocidade da ficha |
| **Escavador** | atravessa areia fofa e terra solta sem lentidão e entra em tocas marcadas no mapa (atalhos) |

---

## 3. Catálogo (30 montarias)

**Como ler:**

- **Velocidade** multiplica a corrida (5,5 m/s). **Defesa** é % de dano a menos. **Vida** e **Fôlego** valem para o
  nível 1 da montaria.
- **Pts** = orçamento do §2.4.
- **Guerra** = montaria de guerra, que não perde dano de ataque básico montado.
- **Drop** é chance **por morte**, sempre rolada no servidor (offline: `EnemyLootRoller` com RNG injetável). A
  montaria cai como item **"Apito de …"** (peso 0), que vai para o inventário. Usar o apito leva a montaria para o
  estábulo. Explicação para criança: o bicho estava preso ou assustado perto do chefe e, depois da luta, decide
  seguir os primos.
- **Lojas novas** (nomes provisórios, ver Decisão 4): **Haras da Vila** (Vila da Vó Zefa, Floresta),
  **Feira de Bichos do Oásis** (cidade do Deserto), **Curral do Pico** (cidade da Montanha) e **Toca do Tratador**
  (cidade das Cavernas). "Lutas" = quantas lutas de ouro o preço custa (~1,8 ouro por luta).

### 3.1 Comuns (loja)

| ID | Nome | Tipo | Raridade | Veloc. | Defesa | Vida | Fôlego | Pts | Guerra | Habilidade especial | Origem | Região | Primo favorito |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `mount_pony` | Pônei Pé-de-Pano | terrestre | Comum | 1,45 | +3% | 125 | 110 | 8 | — | **Arrancada:** disparo de 6 m para a frente em 0,3 s. Recarga 12 s | Missão: Primeira Sela (presente da Vó Zefa depois do Lobo Alfa) / Loja: Haras da Vila, 40 ouro (~22 lutas) | Floresta | Lavine |
| `mount_big_dog` | Cachorrão Paçoca | terrestre | Comum | 1,50 | +2% | 125 | 110 | 8 | — | **Faro de Tesouro:** puxa todo loot do chão num raio de 8 m e marca segredos a até 20 m por 10 s. Recarga 40 s | Loja: Haras da Vila, 60 ouro (~33 lutas) | Floresta | — |
| `mount_capybara` | Capivara Gigante | aquático | Comum | 1,35 | +4% | 150 | 110 | 8 | — | **Banho de Lama:** tira lentidão, veneno e queimadura do primo e deixa ele imune a eles por 3 s. Recarga 35 s | Loja: Haras da Vila, 55 ouro (~31 lutas) | Floresta | — |
| `mount_mountain_goat` | Bode Saltador | terrestre | Comum | 1,40 | +3% | 150 | 110 | 8 | — | **Pulo Duplo:** um segundo pulo no ar (alcança bordas de 2,5 m). Recarrega 6 s depois de usado | Loja: Curral do Pico, 70 ouro (~39 lutas) | Montanha | — |
| `mount_pack_donkey` | Jegue Carregador | terrestre | Comum | 1,30 | +4% | 175 | 110 | 8 | — | **Zurro Espanta-Bicho:** bichos comuns a até 6 m fogem por 3 s (chefe ignora). Passivo: +20 de peso na mochila enquanto montado. Recarga 45 s | Loja: Feira de Bichos do Oásis, 65 ouro (~36 lutas) | Deserto | — |
| `mount_rhea` | Ema Corredora | terrestre | Comum | 1,55 | +0% | 100 | 130 | 8 | — | **Fôlego de Ema:** galopa 8 s sem gastar fôlego. Recarga 30 s | Loja: Feira de Bichos do Oásis, 75 ouro (~42 lutas) | Deserto | — |

### 3.2 Incomuns (loja cara, drop de bicho comum, missão)

| ID | Nome | Tipo | Raridade | Veloc. | Defesa | Vida | Fôlego | Pts | Guerra | Habilidade especial | Origem | Região | Primo favorito |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `mount_pinto_horse` | Cavalo Pampa do Vaqueiro | terrestre | Incomum | 1,60 | +3% | 150 | 110 | 12 | Sim | **Investida do Vaqueiro:** avança 8 m e empurra 3 m os bichos no caminho (sem dano, chefe não é empurrado). Recarga 20 s | Loja: Haras da Vila, 150 ouro (~83 lutas), a partir do nível 10 | Floresta | — |
| `mount_enchanted_donkey` | Jumento Encantado do Sertão | terrestre | Incomum | 1,40 | +5% | 200 | 110 | 12 | — | **Teimosia Santa:** imune a empurrão, atordoamento e lentidão por 5 s. Recarga 40 s | Missão (nova, regional do Deserto): Água pro Jumento | Deserto | — |
| `mount_giant_armadillo` | Tatu-Bola Gigante | escavador | Incomum | 1,35 | +8% | 175 | 100 | 12 | — | **Cavar Túnel:** some debaixo da terra por 4 s andando a ×1,0. Não leva dano e os bichos perdem o alvo. Recarga 30 s | Drop: Enxame de Morcegos 0,4% | Cavernas | Ícaro |
| `mount_maned_wolf` | Lobo-Guará Encantado | terrestre | Incomum | 1,60 | +2% | 125 | 130 | 12 | — | **Olhos do Cerrado:** visão noturna por 30 s (a noite clareia na tela) e bichos a até 30 m aparecem no minimapa. Recarga 60 s | Drop: Lobo da Mata 0,4% (0,8% à noite) | Floresta | — |
| `mount_caipora_peccary` | Caititu da Caipora | terrestre | Incomum | 1,50 | +4% | 150 | 120 | 12 | — | **Trilha da Caipora:** deixa um rastro de folhas por 6 s. Quem do grupo pisa nele ganha +15% de velocidade (dentro do teto). Recarga 30 s | Conquista (nova): Amigo da Caipora (achar os segredos da Floresta) | Floresta | — |
| `mount_river_seahorse` | Cavalo-Marinho Encantado do Rio | aquático | Incomum | 1,50 | +3% | 150 | 130 | 12 | — | **Andar nas Águas:** galopa por cima de rio, açude e lago por 30 s. Recarga 30 s | Missão (nova, Floresta): O Segredo do Açude | Floresta | — |
| `mount_giant_anteater` | Tamanduá-Bandeira Gigante | terrestre | Incomum | 1,40 | +5% | 175 | 120 | 12 | — | **Língua Comprida:** pega um item, abre um baú ou puxa uma alavanca a até 12 m. Recarga 20 s | Loja: Toca do Tratador, 140 ouro (~78 lutas) | Cavernas | — |
| `mount_golden_scarab` | Escaravelho Dourado | escavador | Incomum | 1,40 | +7% | 150 | 110 | 12 | — | **Escudo de Areia:** 60 s imune à tempestade de areia (sem lentidão e sem perder visão) e ao calor do deserto. Recarga 90 s | Drop: Escorpião das Dunas 0,4% / Guardião da Ruína (chefe) 4% | Deserto | — |

### 3.3 Raras (drop de chefe, drop raríssimo, missão, evento)

| ID | Nome | Tipo | Raridade | Veloc. | Defesa | Vida | Fôlego | Pts | Guerra | Habilidade especial | Origem | Região | Primo favorito |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `mount_snow_tiger` | Tigre-das-Neves | terrestre | Raro | 1,70 | +3% | 150 | 130 | 16 | Sim | **Rugido do Tigre:** bichos a até 8 m dão −20% de dano por 6 s. Recarga 35 s | Drop: Senhor da Guerra da Montanha (chefe) 2% | Montanha | — |
| `mount_woolly_mammoth` | Mamute Lanoso | terrestre | Raro | 1,30 | +10% | 250 | 100 | 16 | Sim | **Pisão do Mamute:** atordoa por 1,2 s tudo a até 4 m (chefe 0,6 s, `StunRules`). Recarga 25 s | Drop: Senhor da Guerra da Montanha (chefe) 1,5% / Golem de Pedra 0,15% | Montanha | Joaquim |
| `mount_unicorn` | Unicórnio da Mata | terrestre | Raro | 1,55 | +4% | 175 | 140 | 16 | — | **Aura Curativa:** cura 2% da vida máxima por segundo do primo e do grupo a até 6 m, por 6 s. Recarga 45 s | Drop: Lobo Alfa (chefe) 2% | Floresta | Maria |
| `mount_hippogriff` | Hipogrifo Ventania | voador | Raro | 1,60 | +3% | 150 | 150 | 16 | — | **Planar:** depois de um pulo ou de cair de um lugar alto, plana até 25 m perdendo altura devagar. Recarga 10 s | Missão (nova, pessoal da Maithe): Asas da Ventania | Montanha | Maithe |
| `mount_griffin` | Grifo das Ruínas | voador | Raro | 1,65 | +4% | 150 | 130 | 16 | — | **Voo Curto:** sobe 6 m e voa 15 m por cima de muro, pedra ou vão, e pousa. Recarga 25 s | Drop: Guardião da Ruína (chefe) 2% | Deserto | — |
| `mount_light_jaguar` | Onça-Pintada de Luz | terrestre | Raro | 1,75 | +2% | 125 | 140 | 16 | Sim | **Salto da Onça:** pula num bicho a até 12 m e o marca: o próximo golpe do primo nele é crítico garantido. Recarga 18 s | Drop: Aranha da Floresta 0,1% / Lobo da Mata 0,1% (só à noite) | Floresta | — |
| `mount_carbuncle` | Carbúnculo das Grutas | terrestre | Raro | 1,45 | +6% | 200 | 130 | 16 | — | **Joia Acesa:** a joia da testa ilumina 15 m em volta por 60 s e revela armadilhas e passagens escondidas. Recarga 90 s | Drop: Habitante Ancestral (chefe) 1,5% / Enxame de Morcegos 0,1% | Cavernas | — |
| `mount_fire_rhea` | Ema de Fogo | terrestre | Raro | 1,70 | +1% | 125 | 160 | 16 | — | **Corrida de Brasa:** deixa um rastro de brasa por 6 s. Bicho que pisa queima 4 de dano por segundo por 3 s. Recarga 30 s | Evento: Festa Junina, 90 prendas | Deserto | — |

### 3.4 Épicas (chefe a 0,8%, segredo, conquista longa, evento)

| ID | Nome | Tipo | Raridade | Veloc. | Defesa | Vida | Fôlego | Pts | Guerra | Habilidade especial | Origem | Região | Primo favorito |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `mount_pegasus` | Pégaso Nuvem-Branca | voador | Épico | 1,70 | +4% | 200 | 150 | 21 | — | **Voo Livre:** voa de verdade por 12 s, até 15 m de altura (cruza vale, rio e serra). Recarga 60 s | Drop: Senhor da Guerra da Montanha (chefe) 0,8% | Montanha | — |
| `mount_sleipnir` | Sleipnir, o Corcel de Oito Patas | terrestre | Épico | 1,80 | +3% | 175 | 150 | 21 | Sim | **Galope Entre Mundos:** some e reaparece 12 m à frente, atravessando cerca, porteira ou bicho (não atravessa parede de mapa). Recarga 20 s | Conquista (nova): Viajante dos Quatro Cantos (vencer os 4 chefes com cada um dos 5 primos) | todas | — |
| `mount_kirin` | Kirin do Orvalho | terrestre | Épico | 1,65 | +6% | 200 | 140 | 21 | — | **Passo Sem Pegada:** não pisa na grama por 10 s. Os bichos só percebem o primo a 20% da distância normal (passa escondido). Atacar cancela. Recarga 60 s | Drop: Habitante Ancestral (chefe) 0,8% | Cavernas | — |
| `mount_boitata` | Boitatá, Guardião da Mata | terrestre | Épico | 1,60 | +8% | 225 | 120 | 21 | Sim | **Anel de Fogo Protetor:** um círculo de fogo de 5 m em volta do primo por 6 s. Bicho que entra é empurrado para fora e queima 5 de dano por segundo (chefe não é empurrado). Recarga 40 s | Segredo do mapa (novo): A Mata que Não Queima | Floresta | — |
| `mount_boi_bumba` | Boi-Bumbá Estrelado | terrestre | Épico | 1,45 | +10% | 275 | 110 | 21 | Sim | **Toada do Boi:** o boi dança e o grupo a até 10 m ganha +10% de dano e +10% de velocidade de ataque por 10 s. Recarga 60 s | Evento: Festa Junina, 250 prendas | Floresta (Vila) | — |

### 3.5 Lendárias (chance mínima ou conquista muito longa)

| ID | Nome | Tipo | Raridade | Veloc. | Defesa | Vida | Fôlego | Pts | Guerra | Habilidade especial | Origem | Região | Primo favorito |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `mount_sertao_phoenix` | Fênix do Sertão | voador | Lendário | 1,75 | +5% | 225 | 170 | 26 | — | **Renascer das Cinzas:** se o primo cair (vida 0) montado, ele volta na hora com 30% da vida e a montaria fica cansada. Recarga 10 min | Drop: Habitante Ancestral (chefe) 0,2% / online: Caçada ao Habitante Ancestral 0,5% para os 10 que mais ajudaram | Cavernas | — |
| `mount_dragon_hatchling` | Dragão Filhote Brasinha | voador | Lendário | 1,70 | +8% | 250 | 140 | 26 | Sim | **Sopro Morno:** cone de fogo de 6 m por 1,5 s. Dá 150% de um ataque básico, dividido entre quem estiver no cone. Recarga 25 s | Drop: Guardião da Ruína (chefe) 0,3%, como Ovo de Dragão (choca no estábulo depois de 10 lutas) | Deserto | — |
| `mount_elder_tortoise` | Jabuti Ancião | terrestre | Lendário | 1,30 | +15% | 375 | 100 | 26 | Sim | **Casco de Pedra:** primo e jabuti se fecham no casco por 3 s. Ninguém leva dano, mas também não anda nem ataca. Recarga 90 s | Conquista (nova): Paciência de Jabuti (fechar 100 dias de missões diárias) | todas | — |

### 3.6 Origem por fonte (resumo)

| Fonte | Montarias |
|---|---|
| Loja (ouro) | Pônei 40, Capivara 55, Cachorrão 60, Jegue 65, Bode 70, Ema 75, Tamanduá 140, Cavalo Pampa 150 |
| Bicho comum | Lobo da Mata (Lobo-Guará 0,4%, Onça 0,1% à noite), Aranha (Onça 0,1%), Escorpião (Escaravelho 0,4%), Golem (Mamute 0,15%), Morcegos (Tatu-Bola 0,4%, Carbúnculo 0,1%) |
| Lobo Alfa | Unicórnio 2% |
| Guardião da Ruína | Escaravelho 4%, Grifo 2%, Ovo de Dragão 0,3% |
| Senhor da Guerra da Montanha | Tigre-das-Neves 2%, Mamute 1,5%, Pégaso 0,8% |
| Habitante Ancestral | Carbúnculo 1,5%, Kirin 0,8%, Fênix 0,2% |
| Missão | Pônei (presente), Jumento Encantado, Cavalo-Marinho, Hipogrifo |
| Conquista | Caititu, Sleipnir, Jabuti Ancião |
| Segredo do mapa | Boitatá |
| Evento (prendas) | Ema de Fogo 90, Boi-Bumbá 250 |

**Quanto demora, em média:** 0,4% num bicho comum ≈ 1 em 250 mortes ≈ 170 lutas (1,5 bicho por luta). 2% num chefe
= 1 em 50 vitórias. 0,2% = 1 em 500, por isso existe a proteção contra azar abaixo.

**Proteção contra azar (Fitas do Laço):** toda vitória contra um chefe dá **1 Fita do Laço** daquele chefe
(item sem peso). No Haras, **25 fitas** de um chefe viram a montaria **Rara** daquele chefe (Unicórnio, Grifo, Tigre
ou Carbúnculo), e **80 fitas** viram a **Épica** (Pégaso ou Kirin; o Guardião e o Lobo Alfa não têm Épica). Lendário
**nunca** sai por fita: continua sendo sorte ou conquista. Isso exige que o chefe possa ser enfrentado de novo
offline (Decisão 2).

**Folclore deixado de fora de propósito** (assusta criança ou tem tema adulto): Capelobo, Mula sem Cabeça,
Mapinguari, Lobisomem, Cuca, Pé-de-Garrafa, Boto. A Caipora aparece só como dona do Caititu, e o Boitatá como
**protetor** da mata (a versão boa da lenda: assusta quem quer queimar a floresta).

### 3.7 Checagem de unicidade

30 IDs distintos, 30 nomes distintos e 30 habilidades com nome e efeito distintos, conferidos por script (inclusive
ignorando acento e maiúscula). Cada efeito aparece uma vez só: arrancada, ímã de loot, purificação, pulo duplo,
medo, fôlego infinito, empurrão em linha, imunidade a controle, túnel, visão noturna, rastro de velocidade do grupo,
andar na água, alcance de interação, imunidade a clima, intimidação, atordoamento em área, aura de cura, planar, voo
curto, salto com crítico, luz em caverna, rastro de dano, voo livre, teleporte curto, furtividade, área que empurra,
buff de grupo, renascer, invulnerável parado e sopro em cone.

---

## 4. Nível, alimentação e afeição

### 4.1 Nível da montaria (1–20)

- **XP:** 1 por 20 m andados montado (o servidor conta a distância validada), +XP por comida (§4.2), +5 por luta
  vencida com a montaria esperando na arena (offline).
- **Por nível:** vida +5% da base (nível 20 = vida ×1,95), fôlego +1, recarga da habilidade −0,5% (nível 20 = −9,5%).
- **Nunca sobe:** velocidade e defesa. É o que deixa o teto de velocidade do servidor fixo por espécie, e a defesa
  fica longe do teto de 80%.
- Curva: `xp_para_o_próximo = 60 + 20 × nível` (nível 20 ≈ 4.940 XP no total ≈ 100 km montado; só andando, ~3,5 h de sela a 8 m/s, e bem menos com comida e aulas de doma).

### 4.2 Alimentação (liga com a Cozinha da Vó Zefa)

**Saciedade** 0–100. Cai 1 por minuto montado e não cai no estábulo. Abaixo de 30, a montaria anda a ×1,15 e não
galopa (§2.3). Cada montaria tem uma **dieta**: herbívora, carnívora ou encantada (as mitológicas, que comem de
tudo, mas adoram doce).

| Comida | De onde vem | Saciedade | XP | Afeição | Dieta |
|---|---|---|---|---|---|
| Feixe de capim | 2 `item_fiber` na bancada | +25 | +10 | — | herbívora, encantada |
| Ração do Haras | loja da estrebaria, 3 ouro | +40 | +15 | — | todas |
| Erva fresca | 1 `item_herb` | +15 | +10 | +1 | herbívora, encantada |
| Carne assada | 1 `item_cooked_meat` | +40 | +15 | +1 | carnívora (Cachorrão, Lobo-Guará, Tigre, Onça, Dragão) |
| Cuscuz (prato da Vó, README §10.2) | fogão a lenha | +60 | +40 | +2 | todas |
| Rapadura (prato da Vó) | fogão a lenha | +20 | +20 | **+5** (o petisco favorito) | todas |
| Baião de dois | fogão a lenha | +80 | +60 | +2 | carnívora, encantada |

Custo de manter uma montaria: ~60 de saciedade por hora montado ≈ 1,5 Ração ≈ **4,5 ouro por hora** (≈ 2,5 lutas).
Dá para pagar só com capim (fibra cai bem na Floresta e no Deserto, README §8.2), então ouro nunca impede de montar.

### 4.3 Afeição (0–5 corações)

Sobe com comida, carinho (segurar a interação perto dela, como o carinho no Caramelo do `CarameloCuddle`),
quilômetros montado e tempo no estábulo bem cuidado (§7). Cada coração libera **conforto, não poder**:

| Corações | Bônus |
|---|---|
| 1 | dar um apelido |
| 2 | recarga da habilidade −5% |
| 3 | pose de exibição na cidade (§6) e fala do primo ao montar |
| 4 | assobio de 0,75 s (em vez de 1,5 s) |
| 5 | rastro cosmético próprio da espécie e vida +5% |

O favorito do primo (§8) ganha afeição 25% mais rápido.

---

## 5. Economia

### 5.1 Preço por raridade

| Raridade | Ouro na loja | Em lutas | Outras moedas |
|---|---|---|---|
| Comum | 40–75 | 22–42 | — |
| Incomum | 140–150 (só 2 na loja) | 78–83 | — |
| Raro | **não vende** | — | 90 prendas (só a Ema de Fogo) / 25 Fitas do Laço |
| Épico | **não vende** | — | 250 prendas (só o Boi-Bumbá) / 80 Fitas do Laço |
| Lendário | **nunca** | — | — |

As faixas acompanham as da casa (Comum 8–30, Incomum 35–90, Raro 100–250). Uma montaria custa mais que um móvel da
mesma raridade porque **faz alguma coisa**. A primeira compra (Pônei 40) vem junto com o presente da Vó, então ela é
opcional. Se o `Docs/Housing.md` mudar a renda de ouro (online, por exemplo), multiplique estes preços pelo mesmo
fator.

**Venda de volta:** apito de montaria vendido à loja rende o `_sellBackRate` de sempre (0,5). Apito de Raro para cima
**não se vende à loja** (evita virar fábrica de ouro com drop).

### 5.2 Sumidouros de ouro (gold sink)

| Gasto | Preço | Frequência |
|---|---|---|
| Ração do Haras | 3 ouro | ~1,5 por hora montado |
| Chá de Capim-Santo (acaba o descanso da montaria cansada) | 5 ouro | quando ela cansar |
| Aula de doma no Haras (+100 XP de montaria) | 12 ouro | até 3 por dia |
| Apelido novo | 15 ouro | — |
| Baia extra no Haras (guarda além da casa) | 30 / 60 / 120 ouro | uma vez cada |
| Selas, arreios, mantas, chocalhos (§6) | 20–300 ouro | cosmético |
| Tingir sela ou manta | 10 ouro | cosmético |

### 5.3 Troca (online)

| Raridade | Troca entre jogadores |
|---|---|
| Comum, Incomum | livre |
| Raro | **uma vez**. Quem recebe fica com ela vinculada |
| Épico, Lendário | vinculada à conta, nunca troca (Decisão 6) |

---

## 6. Cosméticos

Só aparência, sem atributo. Encaixam em qualquer montaria do mesmo **porte** (pequeno: Pônei, Capivara, Bode; médio:
cavalos, cães, felinos e aves; grande: Mamute, Jabuti, Boi-Bumbá, Dragão). Lojas: **Selaria do Haras** e a
Costureira Dona Fátima (mantas).

| Encaixe | Exemplos | Raridade / preço |
|---|---|---|
| Sela | Sela de couro cru do tropeiro, Sela bordada de chita, Sela de vaqueiro com estrelas, Sela de couro trabalhada do Vô Chico | Comum 20–30 → Raro 150–250 |
| Arreio | Cabresto de sisal, Peitoral de couro com tachas, Arreio de fitas do Arraiá | Comum 20 → Épico 300 (prendas no evento) |
| Manta | Manta de chita florida, Manta xadrez de quadrilha, Manta de lã do Pico Nevado | Comum 25 → Raro 120 |
| Chocalho / sino | Chocalho de bode, Sininho de prata, Fitinha do Bonfim na crina | Comum 15 → Incomum 60 |
| Rastro | Pegadas de flor, Faíscas de fogueira, Poeira dourada | só por afeição 5 ou evento |
| Pose de exibição | empinar, deitar, dar a pata, rodopiar | afeição 3 |

Conjunto **Tropeiro do Vô Chico** (já existe na casa) ganha 4 peças de montaria. Quem completa ganha o título
"Neto de Tropeiro".

---

## 7. Estábulo da casa (FASE 15)

- Módulo de quintal da casa própria. O número de **baias** sobe com a casa: madeira **2**, madeira com varanda **3**,
  taipa e telha **4**, alvenaria **6**, casarão **8**. O que não cabe fica na estrebaria da cidade (o Haras guarda
  todas, de graça, mas é preciso ir lá trocar).
- Itens do curral que já existem no `CatalogoCasa.md` ganham função quando estão no quintal junto do estábulo:
  - Cocho de madeira escavada (`casa_curral_011`) e Manjedoura de feno (`casa_curral_026`): saciedade das montarias
    nas baias sobe +10 por luta;
  - Bebedouro de pedra (`casa_curral_012`): +1 de afeição por dia para cada montaria na baia;
  - Curral de aroeira (`casa_curral_010`): +1 baia;
  - Sela de couro trabalhada no cavalete (`casa_curral_013`): decoração (mostra a sela equipada da montaria ativa).
- No estábulo você troca a montaria ativa, dá comida, faz carinho, troca a sela e vê o Caderno de Montarias (lista
  de todas as 30 com silhueta das que faltam e a dica de onde aparecem, no estilo do Caderno de hoje).
- Visitante da casa vê as montarias nas baias, mas não monta nem alimenta (mesmas permissões da casa).
- O Caramelo (pet) dorme na casinha dele, não no estábulo. Quando o Cachorrão Paçoca está na baia, o Caramelo vai
  brincar com ele (animação de cheirar e correr em volta).

---

## 8. Os primos e as montarias

O favorito recebe **o mesmo pacote para todos**: habilidade com recarga −15%, afeição 25% mais rápida e uma fala ao
montar. Além disso, cada favorito tem **um detalhe pequeno** que combina com o primo. Como no prato favorito da
Cozinha (README §10.3), quem decide é o primo que está montado, não quem pegou a montaria.

| Primo | Favorito | Bônus extra | Por quê | Fala ao montar |
|---|---|---|---|---|
| **Maithe** (14, Caçadora) | Hipogrifo Ventania | planeio +30% de distância | explora serra e mata proibida, solta pipa, gosta de vento | "Segura firme, Ventania. Lá de cima eu enxergo o caminho todo!" |
| **Maria** (13, Sacerdotisa) | Unicórnio da Mata | Aura Curativa cura +25% | cura e luz, conjunto Luz da Maria (branco e dourado) | "Calma, bonito. A gente cuida de todo mundo junto." |
| **Joaquim** (12, Guardião) | Mamute Lanoso | Pisão com +1 m de raio | o tanque que protege o grupo, devagar e firme como ele | "Pode vir, bicho! Atrás do Mamute ninguém se machuca." |
| **Ícaro** (11, Inventor) | Tatu-Bola Gigante | túnel dura +1 s | acha que o tatu é "a melhor máquina de cavar que já inventaram" | "Ninguém fez uma broca melhor que você, sabia? Nem eu!" |
| **Lavine** (9, Feiticeira) | Pônei Pé-de-Pano | Arrancada recarrega 3 s mais rápido | a caçula. Foi ela que deu o nome ao pônei, que é do tamanho dela | "Pé-de-Pano, corre que a Maithe tá olhando!" |

**Histórias curtas para a campanha** (seguir a regra de escrever história ligada aos 5 primos):

- **Primeira Sela:** depois do Lobo Alfa, a Vó Zefa leva os primos ao Haras do Seu Zé, velho amigo de estrada do
  Vô Chico, e dá um pônei para os cinco. A Lavine escolhe o nome. A Maithe finge que não liga, mas é a primeira a
  pedir para montar.
- **Asas da Ventania (Maithe):** um filhote de hipogrifo caiu do ninho no Pico. A Maithe passa três dias subindo e
  descendo a serra levando comida até ele confiar nela. O Ícaro "ajuda" com um planador que não funciona.
- **Água pro Jumento (Deserto):** um jumento sabido guia os primos até um poço esquecido. O Joaquim e a Maria
  carregam a água, e a Maria descobre que o jumento é encantado ("ele não é teimoso, ele sabe o caminho").
- **A Mata que Não Queima (Boitatá):** segredo da Floresta. Quem apaga as três fogueiras abandonadas na mata vê o
  Boitatá aparecer, não para assustar, mas para agradecer. A Lavine é a única que não tem medo dele.
- **Cachorrão Paçoca:** o sobrinho grandalhão do Caramelo, de uma ninhada da vizinhança. O Caramelo ainda manda
  nele, apesar de ser metade do tamanho.

---

## 9. Online (FASE 12)

### 9.1 Sincronização e aparência

- A montaria **é parte da entidade do jogador**, não um objeto de rede próprio. O estado replicado é pequeno:
  `mountId` (2 bytes, índice no catálogo), `mounted` (1 bit), sela, arreio, manta e chocalho (4 × 2 bytes),
  `gallop` (1 bit) e `abilityEvent` (id + tempo, só quando dispara). Entra no interest management junto com o
  jogador.
- **Os outros veem** a espécie, os cosméticos, o apelido (opcional, desligável) e o efeito da habilidade. Animação e
  passos rodam no cliente de cada um, a partir da velocidade replicada.
- Chamar e descer são **intenções** do cliente. Quem decide é o servidor (está em combate? está na zona segura? a
  recarga já passou?).

### 9.2 Anti-speed-hack (o servidor manda)

- **Teto por espécie**, calculado só no servidor a partir do `MountDefinition`:

  ```
  teto_m_s = 5,5 × velocidade_da_espécie × (galopando ? 1,20 : 1,00) × lentidão_atual × 1,10   // 10% de folga para latência
  ```

  Na zona segura: `5,5 × 1,00 × 1,10`. Teto absoluto do jogo: 13,1 m/s (Sleipnir galopando + folga).
- Checagem: distância andada numa janela de 1 s (e de 5 s, para pegar quem trapaceia "devagar") contra o teto. Se
  passar, o servidor **corrige a posição** (rubber-band) e conta uma infração. Três seguidas viram log de anti-cheat
  (FASE 12.2).
- **Habilidades de movimento** são validadas uma a uma: Arrancada ≤ 6 m, Investida ≤ 8 m, Sleipnir ≤ 12 m sem
  cruzar parede de mapa, Voo Curto ≤ 15 m, Planar ≤ 25 m e queda ≥ 1 m para cada 3 m de planeio, Voo Livre ≤ 12 s
  e ≤ 15 m acima do navmesh, túnel ≤ 4 s. Fora disso, o servidor recusa e corrige.
- **Galope**: o fôlego da montaria é simulado no servidor. Cliente que diz que galopa sem fôlego anda no teto sem
  galope.
- Voador **não pode** entrar voando em zona segura, sala de chefe, arena nem casa (o servidor derruba para terrestre
  na divisa).
- Drop de apito, Fitas do Laço, XP e afeição: **só no servidor** (como o loot e o ouro, FASE 12.2), com log de
  economia de cada apito criado, trocado ou usado.

### 9.3 Justiça (sem pay-to-win)

Montaria com atributo **nunca** vende por dinheiro de verdade. Se existir loja de dinheiro (Decisão 7), ela vende só
**aparência**: um visual alternativo de uma montaria que a pessoa já tem, ou selas e mantas, sempre sem atributo.

---

## 10. Acessibilidade

- **Movimento reduzido** (usa a opção de acessibilidade do jogo, ou liga junto com ela): sem balanço de câmera no
  galope, sem aumento de FOV na Arrancada, na Investida e no teleporte do Sleipnir, sem tremor de tela no Pisão e no
  Casco, câmera nivelada no voo (sem inclinar nas curvas), transições de teleporte com fade curto em vez de
  "esticar" a imagem.
- **Luz forte:** Joia Acesa, Anel de Fogo e Sopro Morno respeitam a opção de reduzir flashes (brilho máximo menor,
  sem piscar).
- **Controle:** galope em "segurar" ou "alternar"; assobio em "segurar" ou "tocar duas vezes"; seguir a estrada
  sozinho (o auto-run que já existe vale montado); botão grande de montaria no toque.
- **Leitura:** a habilidade da montaria tem ícone com forma própria (não só cor) e som de aviso quando recarrega
  (chocalho). Toda fala do §8 tem legenda.
- **Enjoo no voo:** opção "voo baixo", que limita a câmera a 6 m de altura mesmo no Pégaso.

---

## 11. Inspiração (só referência)

MU Online tem montarias que mudam o jeito de jogar: o **Dark Horse** (montaria de combate com investida, só de uma
classe), o **Dinorant** (voador que cruza mapas que a pé não dá) e o **Fenrir** (lobo raro, montado de peças
dropadas, com versões melhores). Pegamos só as **ideias de sistema**:

- montaria que serve para lutar ("montaria de guerra");
- tipo voador que abre caminho;
- montaria rara que se junta aos poucos (as Fitas do Laço, mas sem montar peça por peça);
- a montaria como símbolo de status na cidade.

Nenhum nome, visual, número ou habilidade foi copiado. Os bichos vêm do folclore brasileiro (Boitatá, Caipora,
Boi-Bumbá, Carbúnculo, jabuti das fábulas), de mitologias de domínio público (Pégaso, Grifo, Hipogrifo, Fênix, Kirin,
Sleipnir, Escaravelho egípcio, Unicórnio) e da fauna do Brasil (capivara, ema, lobo-guará, caititu, tamanduá,
tatu-bola, onça). O desenho de cada um segue o estilo low-poly sertanejo do jogo.

---

## 12. Modelo de dados (esboço)

### 12.1 `MountDefinition` (ScriptableObject, `Resources/Mounts/`)

Mesmo padrão "dado como asset" do `ItemDefinition`, `ShopDefinition` e `EnemyDefinition`.

| Campo | Tipo | Nota |
|---|---|---|
| `_mountId` | string | `mount_*`, nunca reaproveitar |
| `_displayNameKey`, `_descriptionKey`, `_loreKey` | string | chaves de localização (pt-BR e en) |
| `_type` | `MountType` { Terrestre, Voador, Aquatico, Escavador } | §2.6 |
| `_rarity` | `ItemRarity` | o enum que já existe |
| `_size` | `MountSize` { Pequeno, Medio, Grande } | encaixe de cosmético (§6) e câmera |
| `_speedMultiplier` | float [1,30; 1,80] | × `PlayerMotor._runSpeed` |
| `_gallopMultiplier` | float = 1,20 | igual para todas, deixado como campo para afinar |
| `_defensePercent` | float [0; 0,15] | `Defense` PercentAdditive, fonte `mount` |
| `_baseHealth`, `_baseStamina` | float | nível 1 |
| `_damageShareToMount` | float = 0,5 | §2.3 |
| `_isWarMount` | bool | sem −20% no ataque montado |
| `_ability` | `MountAbilityDefinition` | §12.2 |
| `_diet` | `MountDiet` flags { Herbivora, Carnivora, Encantada } | §4.2 |
| `_favoriteCharacter` | `CharacterId` + `_hasFavorite` bool | §8 |
| `_favoriteBonus` | `MountFavoriteBonus` (campo + valor) | ex.: `AbilityRadius +1` |
| `_favoriteLineKey` | string | fala ao montar |
| `_tradeRule` | `MountTradeRule` { Livre, UmaVez, Vinculada } | §5.3 |
| `_whistleItem` | `ItemDefinition` | o "Apito de …" (peso 0, `ItemCategory` novo ou `Misc`) |
| `_visualRecipe` | `ProceduralMountRecipe` | low-poly procedural, como `ProceduralEnemyBlueprint` e as `*Recipe` dos inimigos |
| `_saddleSocket`, `_riderSeatOffset`, `_riderPose` | Vector3 / enum | onde o primo senta (pônei baixo, mamute alto) |
| `_sfxCallKey`, `_sfxStepKey`, `_sfxAbilityKey` | string | chaves do `SfxEventMap` (som procedural no começo) |
| `_regionTag` | `BiomeId` | Caderno e dicas |
| `_sourceHint` | string (loc) | "Aparece perto do Lobo Alfa…" para o Caderno |

### 12.2 `MountAbilityDefinition` (ScriptableObject)

| Campo | Nota |
|---|---|
| `_abilityId`, `_nameKey`, `_descriptionKey`, `_icon` | — |
| `_kind` | enum: Dash, DoubleJump, Glide, ShortFlight, FreeFlight, Blink, Burrow, WaterWalk, LootMagnet, Cleanse, Fear, FreeGallop, Knockback, CCImmunity, NightVision, PartySpeedTrail, RangedInteract, WeatherImmunity, Intimidate, AreaStun, HealAura, PounceMark, Light, DamageTrail, Stealth, RepelRing, PartyBuff, Revive, Invulnerable, ConeBreath |
| `_cooldownSeconds`, `_durationSeconds`, `_distance`, `_radius`, `_magnitude` | parâmetros genéricos |
| `_statModifiers` | lista de `StatModifier` (reusa `StatType`/`BuffController`) para os efeitos de stat |
| `_allowedInSafeZone` | sempre false (§2.5) |
| `_bossMultiplier` | 0,5 para controle, 0 para medo e empurrão |
| `_serverMaxDistance`, `_serverMaxDuration`, `_serverMaxAltitude` | validação do §9.2 |

As regras ficam numa **classe pura testável** `MountRules`: pode chamar, recarga, teto de velocidade, dano dividido,
fome, nível, XP, afeição, favorito. É o mesmo padrão do `StunRules` e do `LootGrantRules`, e o servidor reaproveita a
classe.

### 12.3 Save (offline) e servidor

```
MountCollectionSaveState            // ISaveable novo, registrado no GameManager
  int    version
  string activeMountGuid
  int    harasExtraSlots
  List<OwnedMount> mounts
  Dictionary<string,int> bossRibbons     // Fitas do Laço por enemyId
  List<string> codexSeen                 // montarias já vistas (silhueta no Caderno)

OwnedMount
  string guid               // instância (pode haver duas Capivaras)
  string mountId
  string nickname
  int    level, xp
  int    affection          // 0..500 (100 por coração)
  float  satiety            // 0..100
  float  currentHealth
  int    tiredUntilEncounter      // offline: conta lutas; online: tiredUntilUtc
  string saddleId, harnessId, blanketId, bellId, trailId
  int    stableSlot         // -1 = no Haras da cidade
  string source             // shop/drop/quest/achievement/event/secret/ribbon
  long   acquiredUtc
  bool   bound              // vinculada (UmaVez já usada, Épico, Lendário)
  int    hatchEncountersLeft      // só o Ovo de Dragão
```

**No servidor:** a mesma estrutura em tabela por personagem (ou por conta, Decisão 5), mais `tradeLockUntil`, um log
de auditoria (criado/trocado/usado/vendido) e o estado ao vivo que não vai para o save local (fôlego do galope,
recargas).

---

## 13. Plano de implementação por fases

**M1: offline, 3 montarias iniciais** (Pônei Pé-de-Pano, Cachorrão Paçoca, Jegue Carregador: equilibrado, rápido e
resistente)

1. `MountDefinition`, `MountAbilityDefinition` e `MountRules` (classe pura + testes EditMode: teto de velocidade,
   buffs ignorados e lentidão aplicada, recarga, chamar fora de combate, dano dividido, cansada, fome).
2. `MountController` no Player: assobio de 1,5 s, subir, descer, galope com fôlego próprio, velocidade no
   `PlayerMotor` pelo multiplicador da montaria (respeitando o clamp de 2,5 do `BuffController`), pose do primo
   sentado.
3. Modelos **low-poly procedurais** com o mesmo caminho dos inimigos (`EnemyMeshBuilder`, `ProceduralEnemyRig`,
   receitas como `StoneGolemRecipe`) e do Caramelo (`Pets/Visuals`): uma receita quadrúpede parametrizada (tamanho,
   pescoço, orelhas, rabo, crina) cobre pônei, cachorro e jegue. Andar, trotar e galopar procedurais como no
   `EnemyProceduralAnimator`.
4. Valer no `WorldMap` e na Vila; a arena começa a pé; trote na zona segura (`SafeZoneRules` quando existir).
5. Haras da Vila: `ShopDefinition` nova vendendo os apitos (`_priceOverride`). Missão "Primeira Sela" dá o Pônei.
6. Save (`MountCollectionSaveState`) e sons procedurais (`ProceduralSfxSynth`: assobio, cascos, chocalho).
7. Habilidades das 3: Arrancada, Faro de Tesouro e Zurro Espanta-Bicho.

**M2: cuidado e casa**: saciedade, comidas (feixe de capim na bancada; pratos da Cozinha), XP e nível, afeição e
carinho, estábulo na casa (depende da Fase 15), Caderno de Montarias, primeiros cosméticos (sela e manta), favoritos
dos primos com as falas.

**M3: catálogo** (27 restantes, em ondas):

1. Terrestres sem física nova: Capivara (sem nado ainda), Bode, Ema, Cavalo Pampa, Jumento, Lobo-Guará, Caititu,
   Tamanduá, Tigre, Mamute, Unicórnio, Onça, Carbúnculo, Ema de Fogo, Kirin, Boitatá, Boi-Bumbá, Jabuti.
2. Física nova: nado (Capivara, Cavalo-Marinho), túnel e areia (Tatu, Escaravelho), teleporte (Sleipnir).
3. Voadores: Hipogrifo (planar), Grifo (voo curto), Pégaso (voo livre), Fênix, Dragão (ovo e choco).
4. Drops nos chefes e bichos (`EnemyLootRoller`, bônus de chefe), Fitas do Laço, conquistas e missões novas, itens
   dos eventos Festa Junina.
5. Balanceamento: script que confere o orçamento do §2.4 e a unicidade do §3.7 (teste como o
   `BalanceInvariantsTests`).

**M4: online**: replicação (§9.1), teto e validação no servidor (§9.2), combate montado (§2.2) nos mapas abertos,
estrebaria em cada cidade, troca com as regras do §5.3, montarias nos eventos agendados e no chefe mundial.

---

## 14. Decisões para o usuário

1. **Montar em combate:** combate montado nos mapas abertos (proposta: sim, com −20% no ataque básico, exceto as
   montarias de guerra), ou montaria só para viajar, com o primo sempre lutando a pé?
2. **Revanche de chefe offline:** para a montaria de chefe (e as Fitas do Laço) fazer sentido offline, o chefe
   precisa poder ser enfrentado de novo depois de vencido. Liberar a revanche (proposta: sim, no mesmo lugar, com
   loot normal), ou deixar os drops de chefe só para o NG+ e o online?
3. **Chances de drop:** 0,4% (Incomum de bicho comum), 2% (Raro de chefe), 0,8% (Épico), 0,2–0,3% (Lendário), com
   as Fitas do Laço (25/80) como proteção. Está bom, mais raro (mais "MU") ou mais generoso (público infantil)?
4. **Nomes das lojas e das cidades:** Haras da Vila (Seu Zé, amigo do Vô Chico), Feira de Bichos do Oásis, Curral
   do Pico e Toca do Tratador são provisórios até as cidades do Deserto, da Montanha e das Cavernas terem nome
   (FASE 12.3a).
5. **De quem é a montaria:** da conta (todos os primos / personagens da conta usam, como o Baú) ou de cada
   personagem? Offline a proposta é compartilhar entre os 5 primos.
6. **Troca online:** Comum e Incomum livres, Raro uma vez, Épico e Lendário vinculados. Ou tudo vinculado, para
   evitar mercado paralelo?
7. **Dinheiro de verdade:** nenhuma montaria com atributo à venda (proposta). Pode haver só visual alternativo de
   montaria já conquistada, ou nada mesmo?
8. **Presente inicial:** o Pônei de graça na missão "Primeira Sela" (proposta), ou a primeira montaria também é
   comprada?
9. **Montaria cansada em vez de morte** e o Chá de Capim-Santo pago (5 ouro) para voltar logo: ok, ou o descanso
   deve ser sempre grátis?
10. **Velocidade máxima:** 1,80× a corrida (11,9 m/s galopando) como teto do jogo. Mapas abertos grandes podem pedir
    mais (o MU tem montarias bem rápidas), mas isso muda a viagem rápida e o servidor.
11. **Nomes escolhidos:** Cachorrão Paçoca (sobrinho do Caramelo), Pônei Pé-de-Pano, Hipogrifo Ventania, Dragão
    Filhote Brasinha. Trocar algum? E deixar de fora o folclore que assusta (Capelobo, Mula sem Cabeça, Mapinguari,
    Lobisomem) está ok?
