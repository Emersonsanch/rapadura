using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.Content
{
    /// <summary>
    /// en + pt-BR text for every ScriptableObject content asset (Resources/Items, Skills, Enemies,
    /// Recipes, Combos, SkillTrees) and the five playable characters, under the keys derived by
    /// <see cref="ContentText"/>. Same self-contained pattern as the other *Texts helpers: merged into
    /// the live <see cref="LocalizationManager"/> on first use, only adding keys it doesn't have yet (an
    /// external CSV/LocalizationTable always wins).
    ///
    /// The pt-BR column is a copy of the asset/class text (the asset stays the source of truth and the
    /// fallback); <c>LocalizationCoverageTests</c> fails if they drift apart, so edit both together.
    /// English keeps the heroes' voice: five kids of Dawn Isle — Maithe and Ícaro are siblings,
    /// Maria and Joaquim are siblings, and Lavine, the youngest, is cousin to all four.
    /// </summary>
    public static class ContentTexts
    {
        public static readonly List<LocalizationEntry> Entries = new List<LocalizationEntry>
        {
            // -- Items (Resources/Items) --
            ItemName("item_axe", "Axe", "Machado"),
            ItemDescription("item_axe", "A tool for chopping trees and gathering wood faster.", "Ferramenta usada para cortar árvores e coletar madeira mais rápido."),
            ItemName("item_cooked_meat", "Roast Meat", "Carne Assada"),
            ItemDescription("item_cooked_meat", "Food that restores energy.", "Alimento que restaura energia."),
            ItemName("item_fiber", "Plant Fiber", "Fibra Vegetal"),
            ItemDescription("item_fiber", "Fibers pulled from plants, used for rope and cloth.", "Fibras retiradas de plantas, usadas em cordas e tecidos."),
            ItemName("item_gold", "Gold", "Ouro"),
            ItemDescription("item_gold", "A raw gold nugget. Used to craft rare and legendary items.", "Pepita de ouro bruto. Usada em crafting de itens raros/lendários."),
            ItemName("item_gold_coin", "Qocho Holed Coin", "Moeda Furada de Qocho"),
            ItemDescription("item_gold_coin", "A coin with a square hole, like the ones that changed hands in the Qocho bazaar. The common currency of the island.", "Moeda com furo quadrado, como as que corriam no bazar de Qocho. É a moeda corrente da ilha."),
            ItemName("item_hammer", "Building Hammer", "Martelo de Construção"),
            ItemDescription("item_hammer", "A tool for building and repairing structures.", "Ferramenta usada para construir e reparar estruturas."),
            ItemName("item_health_potion", "Health Potion", "Poção de Vida"),
            ItemDescription("item_health_potion", "Restores some health when consumed.", "Restaura uma quantidade de vida ao ser consumida."),
            ItemName("item_herb", "Medicinal Herb", "Erva Medicinal"),
            ItemDescription("item_herb", "A plant with healing properties, used in potions.", "Planta com propriedades curativas, usada em poções."),
            ItemName("item_hunting_bow", "Hunting Bow", "Arco de Caça"),
            ItemDescription("item_hunting_bow", "A light bow for ranged attacks.", "Arco leve usado para ataques à distância."),
            ItemName("item_iron", "Iron", "Ferro"),
            ItemDescription("item_iron", "An iron bar forged from ore. Used in building and advanced crafting.", "Barra de ferro forjada a partir de minério. Usada em construção e crafting avançado."),
            ItemName("item_iron_ore", "Iron Ore", "Minério de Ferro"),
            ItemDescription("item_iron_ore", "Raw ore dug out of rocky veins. Can be smelted into bars.", "Minério bruto extraído de veios rochosos. Pode ser fundido em barras."),
            ItemName("item_iron_sword", "Iron Sword", "Espada de Ferro"),
            ItemDescription("item_iron_sword", "A sword forged from iron, sturdy and sharp.", "Espada forjada em ferro, resistente e afiada."),
            ItemName("item_leather_boots", "Leather Boots", "Botas de Couro"),
            ItemDescription("item_leather_boots", "Basic protection for the feet.", "Proteção básica para os pés."),
            ItemName("item_leather_chest", "Leather Chestpiece", "Peitoral de Couro"),
            ItemDescription("item_leather_chest", "Basic protection for the torso.", "Proteção básica para o tronco."),
            ItemName("item_leather_helmet", "Leather Helmet", "Capacete de Couro"),
            ItemDescription("item_leather_helmet", "Basic protection for the head.", "Proteção básica para a cabeça."),
            ItemName("item_mana_potion", "Mana Potion", "Poção de Mana"),
            ItemDescription("item_mana_potion", "Restores some mana when consumed.", "Restaura uma quantidade de mana ao ser consumida."),
            ItemName("item_pickaxe", "Pickaxe", "Picareta"),
            ItemDescription("item_pickaxe", "A tool for mining stone and ore.", "Ferramenta usada para minerar pedras e minérios."),
            ItemName("item_stone", "Stone", "Pedra"),
            ItemDescription("item_stone", "Raw stone gathered from rocks. Used in building and crafting.", "Pedra bruta coletada de rochas. Usada em construção e crafting."),
            ItemName("item_wood", "Wood", "Madeira"),
            ItemDescription("item_wood", "A log gathered from trees. Used in building and crafting.", "Tora de madeira coletada de árvores. Usada em construção e crafting."),
            ItemName("item_wooden_sword", "Wooden Sword", "Espada de Madeira"),
            ItemDescription("item_wooden_sword", "A simple training sword made of wood.", "Uma espada simples de treino, feita de madeira."),

            // -- Skills (Resources/Skills) --
            SkillName("skill_arrow", "Arrow", "Flecha"),
            SkillDescription("skill_arrow", "Fires a precise long-range arrow at a single target.", "Dispara uma flecha precisa de longo alcance contra um único alvo."),
            SkillName("skill_berserk", "Berserk", "Berserk"),
            SkillDescription("skill_berserk", "Ultimate: flies into a rage, boosting attack damage at the cost of lower defense.", "Ultimate: entra em fúria, aumentando dano de ataque em troca de reduzir a defesa."),
            SkillName("skill_dash", "Dash", "Dash"),
            SkillDescription("skill_dash", "Propels the character forward quickly, briefly boosting speed.", "Impulsiona o personagem rapidamente para frente, aumentando a velocidade momentaneamente."),
            SkillName("skill_earthquake", "Earthquake", "Terremoto"),
            SkillDescription("skill_earthquake", "Shakes the ground around the caster, dealing area damage and knocking enemies down.", "Abala o solo ao redor do conjurador, causando dano em área e derrubando inimigos."),
            SkillName("skill_explosion", "Explosion", "Explosão"),
            SkillDescription("skill_explosion", "Sets off an instant arcane explosion around the caster.", "Detona uma explosão arcana instantânea ao redor do conjurador."),
            SkillName("skill_fireball", "Fireball", "Bola de Fogo"),
            SkillDescription("skill_fireball", "Hurls a flaming sphere that explodes on hitting the target, dealing fire damage in an area and leaving it burning for a while.", "Lança uma esfera flamejante que explode ao atingir o alvo, causando dano de fogo em área e deixando-o queimando por um tempo."),
            SkillName("skill_freeze", "Freeze", "Congelamento"),
            SkillDescription("skill_freeze", "Freezes the target, rooting it in place and drastically reducing its speed.", "Congela o alvo, imobilizando-o e reduzindo drasticamente sua velocidade."),
            SkillName("skill_heal", "Heal", "Cura"),
            SkillDescription("skill_heal", "Instantly restores some health.", "Restaura uma quantidade de vida instantaneamente."),
            SkillName("skill_icaro_campo_magnetico", "Engine Field", "Campo do Engenho"),
            SkillDescription("skill_icaro_campo_magnetico", "Ícaro switches on a portable Engine of Light that throws a magnetic field around him: enemies get stuck and slowed while metal shards are pulled away from him.", "Ícaro liga um Engenho da Luz portátil que cria um campo magnético ao redor: inimigos ficam presos e lentos enquanto estilhaços metálicos são desviados dele."),
            SkillName("skill_icaro_drone_de_ataque", "Senmurv Drone", "Drone Senmurv"),
            SkillDescription("skill_icaro_drone_de_ataque", "A little drone with the wings of the Senmurv, the bird from the old tales, painted in the colors of Maithe's bow (to impress his sister), chases a target, shooting at it and exposing its defenses.", "Um pequeno drone com asas de Senmurv, o pássaro dos contos, pintado com as cores do arco de Maithe (pra impressionar a irmã), persegue um alvo, disparando contra ele e expondo suas defesas."),
            SkillName("skill_icaro_exercito_mecanico", "Ushabti Army", "Exército de Ushabtis"),
            SkillDescription("skill_icaro_exercito_mecanico", "Ultimate: Ícaro calls every invention he has at once, a tiny army of rebuilt ushabtis that answer together and sweep the area with bursts of sparks. While they fight, he's already recharging the next batch.", "Ultimate: Ícaro chama todas as suas invenções de uma vez, um pequeno exército de ushabtis remontados que respondem juntos e varrem a área com descargas de faíscas. Enquanto eles lutam, ele já está recarregando os próximos."),
            SkillName("skill_icaro_granada_eletrica", "Spark Grenade", "Granada de Faísca"),
            SkillDescription("skill_icaro_granada_eletrica", "A homemade grenade that releases a burst of Living Fire sparks over an area, dealing damage and leaving enemies shocked and slowed.", "Uma granada caseira que solta uma chuva de faíscas de Fogo Vivo em área, causando dano e deixando os inimigos eletrocutados e lentos."),
            SkillName("skill_icaro_torreta_automatica", "Ushabti Turret", "Torreta Ushabti"),
            SkillDescription("skill_icaro_torreta_automatica", "Ícaro hastily rebuilds a turret from the parts of an ushabti, the Artificers' guardian \"that answers\", and it fires nonstop at enemies in the area for a few seconds.", "Ícaro remonta às pressas uma torreta com peças de um ushabti, o guardião \"que responde\" dos Artífices, e ela atira sem parar nos inimigos da área por alguns segundos."),
            SkillName("skill_ice_shard", "Ice", "Gelo"),
            SkillDescription("skill_ice_shard", "Fires a shard of ice that deals damage and slows the target's movement.", "Dispara um estilhaço de gelo que causa dano e reduz a velocidade de movimento do alvo."),
            SkillName("skill_invisibility", "Invisibility", "Invisibilidade"),
            SkillDescription("skill_invisibility", "Makes the caster stealthy, lowering the chance of being spotted by enemies.", "Torna o conjurador furtivo, reduzindo a chance de ser detectado por inimigos."),
            SkillName("skill_joaquim_escudo_das_raizes", "Shield of Roots", "Escudo das Raízes"),
            SkillDescription("skill_joaquim_escudo_das_raizes", "The artifact handed down by his ancestors awakens: roots wrap around Joaquim, soaking up part of the damage and slowly restoring his health.", "O artefato herdado dos ancestrais desperta: raízes envolvem Joaquim, absorvendo parte do dano e regenerando sua vida aos poucos."),
            SkillName("skill_joaquim_muralha_de_pedra", "Tide Wall", "Muralha da Maré"),
            SkillDescription("skill_joaquim_muralha_de_pedra", "Joaquim plants the Shield of Eternal Roots in the ground and raises a wall of Living Water in front of him, the river that lets nothing through, greatly boosting his defense for a few seconds.", "Joaquim finca o Escudo das Raízes Eternas no chão e ergue à sua frente uma muralha de Água Viva, o rio que não deixa passar, aumentando muito a defesa por alguns segundos."),
            SkillName("skill_joaquim_provocacao_heroica", "Heroic Taunt", "Provocação Heroica"),
            SkillDescription("skill_joaquim_provocacao_heroica", "A stubborn shout of \"come at me!\". Nearby enemies turn on Joaquim and, blinded by rage, hit weaker; he digs his heels in and holds out better.", "Um grito teimoso de \"venham pra cima de mim!\". Os inimigos ao redor se voltam contra Joaquim e, distraídos pela raiva, batem mais fraco; ele firma os pés e resiste melhor."),
            SkillName("skill_joaquim_terremoto", "Tidal Quake", "Maremoto"),
            SkillDescription("skill_joaquim_terremoto", "Joaquim slams his shield into the ground with all his might and a wave of Living Water rolls out around him, knocking struck enemies off balance.", "Joaquim bate o escudo no chão com toda a força e solta ao redor uma onda de Água Viva que desequilibra os inimigos atingidos."),
            SkillName("skill_joaquim_ultima_resistencia", "Last Stand", "Última Resistência"),
            SkillDescription("skill_joaquim_ultima_resistencia", "Ultimate: when all seems lost, Joaquim refuses to fall while there's a cousin behind him. He recovers health and gains huge defense and strength for a few seconds.", "Ultimate: quando tudo parece perdido, Joaquim se recusa a cair enquanto houver um primo atrás dele. Recupera vida e ganha defesa e força enormes por alguns segundos."),
            SkillName("skill_lavine_correntes_das_trevas", "Chains of Air", "Correntes de Ar"),
            SkillDescription("skill_lavine_correntes_das_trevas", "Chains of still night air drop from the sky and bind enemies in the area, nearly pinning them in place and leaving them more vulnerable.", "Correntes de ar quieto descem do céu da noite e prendem os inimigos da área, quase imobilizando-os e deixando-os mais vulneráveis."),
            SkillName("skill_lavine_eclipse_eterno", "Red Night Sky", "Céu da Noite Rubra"),
            SkillDescription("skill_lavine_eclipse_eterno", "Ultimate: the sky goes quiet and red like the Noite Rubra when Lavine was born. A wave of still, starry air wounds every enemy nearby and leaves them slowed and defenseless. It's proof that the youngest in the family can use this power to protect.", "Ultimate: o céu fica quieto e vermelho como na Noite Rubra em que Lavine nasceu. Uma onda de ar sereno e estrelado fere todos os inimigos ao redor e os deixa lentos e indefesos. É a prova de que a caçula da família pode usar esse poder para proteger."),
            SkillName("skill_lavine_explosao_arcana", "Starburst", "Explosão de Estrelas"),
            SkillDescription("skill_lavine_explosao_arcana", "Lavine gathers the Pure Air she can barely control and lets it all out at once in a high-damage area burst of silver stars.", "Lavine junta o Ar Sereno que mal consegue controlar e solta tudo de uma vez numa explosão de estrelinhas prateadas, de alto dano em área."),
            SkillName("skill_lavine_nevoa_da_ilusao", "Mist of Illusion", "Névoa da Ilusão"),
            SkillDescription("skill_lavine_nevoa_da_ilusao", "A purple mist full of illusions confuses enemies, who move slowly and miss their blows. The older cousins have fallen for this trick more than once.", "Uma névoa arroxeada cheia de ilusões confunde os inimigos, que se movem devagar e erram os golpes. Os primos mais velhos já caíram nessa brincadeira mais de uma vez."),
            SkillName("skill_lavine_orbe_sombria", "Pure Air Orb", "Orbe do Ar Sereno"),
            SkillDescription("skill_lavine_orbe_sombria", "Lavine throws a small lilac-and-silver sphere of Pure Air that deals magic damage when it hits its target.", "Lavine lança uma pequena esfera lilás e prateada de Ar Sereno que causa dano mágico ao atingir o alvo."),
            SkillName("skill_lightning_bolt", "Lightning", "Raio"),
            SkillDescription("skill_lightning_bolt", "Calls down an instant lightning bolt that strikes the target with high electric damage.", "Convoca um raio instantâneo que atinge o alvo com alto dano elétrico."),
            SkillName("skill_maithe_chuva_de_flechas", "Rain of Arrows", "Chuva de Flechas"),
            SkillDescription("skill_maithe_chuva_de_flechas", "Maithe looses a volley into the sky that falls over an area, wounding and pinning to the ground any enemy that tries to advance on the family.", "Maithe dispara uma saraivada para o alto que cai sobre uma área, ferindo e prendendo ao chão os inimigos que tentarem avançar sobre a família."),
            SkillName("skill_maithe_disparo_explosivo", "Explosive Shot", "Disparo Explosivo"),
            SkillDescription("skill_maithe_disparo_explosivo", "An arrow with one of her brother Ícaro's homemade firecrackers tied to the tip. It explodes on impact, dealing fire damage in an area and leaving targets burning.", "Uma flecha com uma das bombinhas improvisadas do irmão Ícaro amarrada na ponta. Explode no impacto, causando dano de fogo em área e deixando os alvos queimando."),
            SkillName("skill_maithe_flecha_precisa", "Precise Arrow", "Flecha Precisa"),
            SkillDescription("skill_maithe_flecha_precisa", "A single shot guided by the Eye of the Wind that hits a distant target with a veteran huntress's precision and makes Maithe's next shots more likely to land critical hits.", "Um único disparo guiado pelo Olho do Vento, que atinge um alvo distante com precisão de caçadora veterana e deixa os próximos tiros de Maithe mais propensos a acertos críticos."),
            SkillName("skill_maithe_passo_do_vento", "Wind Step", "Passo do Vento"),
            SkillDescription("skill_maithe_passo_do_vento", "Maithe lets the mountain wind carry her, gaining speed for a few seconds to slip away from danger and get back to shooting.", "Maithe se deixa levar pelo vento das montanhas, ganhando velocidade por alguns segundos para reposicionar-se longe do perigo e voltar a atirar."),
            SkillName("skill_maithe_tempestade_de_mil_flechas", "Storm of a Thousand Arrows", "Tempestade de Mil Flechas"),
            SkillDescription("skill_maithe_tempestade_de_mil_flechas", "Ultimate: Maithe empties her quiver into a whirlwind of wind and arrows that covers the battlefield, wounding and disorienting everything around. It's what the eldest cousin does when she decides nobody in the family is getting hurt today.", "Ultimate: Maithe esvazia a aljava num redemoinho de vento e flechas que cobre o campo de batalha, ferindo e desorientando tudo ao redor. É o que a prima mais velha faz quando decide que ninguém da família vai se machucar hoje."),
            SkillName("skill_maria_bencao_da_vida", "Glow of Life", "Brilho da Vida"),
            SkillDescription("skill_maria_bencao_da_vida", "A warm glow of vital energy that slowly restores health over several seconds and raises max health while it lasts.", "Um brilho morno de energia vital que regenera a vida aos poucos por vários segundos e aumenta a vida máxima enquanto dura."),
            SkillName("skill_maria_cura_divina", "Healing Light", "Luz que Cura"),
            SkillDescription("skill_maria_cura_divina", "Maria calls on the light of the Solar Temple and restores her own health, just as the elder healers taught her.", "Maria chama a luz do Templo Solar e restaura a própria vida, como as curandeiras mais velhas lhe ensinaram."),
            SkillName("skill_maria_escudo_sagrado", "Sun Shield", "Escudo Solar"),
            SkillDescription("skill_maria_escudo_sagrado", "A shield of golden light that greatly reduces damage taken. Maria learned to raise it after seeing her brother Joaquim throw himself in front of danger without thinking one too many times.", "Um escudo de luz dourada que reduz muito o dano recebido. Maria aprendeu a erguê-lo de tanto ver o irmão Joaquim se jogar na frente do perigo sem pensar."),
            SkillName("skill_maria_milagre_celestial", "Column of Light", "Coluna de Luz"),
            SkillDescription("skill_maria_milagre_celestial", "Ultimate: Maria calls the light of the Sun Crystal and is wrapped in a column of light that heals almost every wound, shields her and keeps healing for a few seconds.", "Ultimate: Maria chama a luz do Cristal do Sol e é envolvida por uma coluna de luz que cura quase todos os ferimentos, protege e continua curando por alguns segundos."),
            SkillName("skill_maria_purificacao", "Purification", "Purificação"),
            SkillDescription("skill_maria_purificacao", "A burst of light that wounds the dimmed creatures nearby and weakens their attacks, giving a little health back to Maria. It's the same light she uses to calm her cousin Lavine when the night gives her butterflies.", "Uma explosão de luz que fere os bichos apagados ao redor e enfraquece seus ataques, devolvendo um pouco de vida a Maria. É a mesma luz com que ela acalma a prima Lavine quando a noite dá friozinho na barriga."),
            SkillName("skill_meteor_shower", "Meteor Shower", "Chuva de Meteoros"),
            SkillDescription("skill_meteor_shower", "Ultimate: calls down a shower of meteors over a large area, dealing massive damage.", "Ultimate: invoca uma chuva de meteoros sobre uma grande área, causando dano massivo."),
            SkillName("skill_poison_dart", "Poison", "Veneno"),
            SkillDescription("skill_poison_dart", "Applies a toxin that deals damage over time.", "Aplica uma toxina que causa dano contínuo ao longo do tempo."),
            SkillName("skill_regeneration", "Regeneration", "Regeneração"),
            SkillDescription("skill_regeneration", "Drastically increases health regeneration for a while.", "Aumenta drasticamente a regeneração de vida por um período."),
            SkillName("skill_shield", "Shield", "Escudo"),
            SkillDescription("skill_shield", "Wraps the caster in a barrier that raises defense for a short time.", "Envolve o conjurador em uma barreira que aumenta a defesa por um curto período."),
            SkillName("skill_summon", "Summon", "Invocação"),
            SkillDescription("skill_summon", "Summons an allied creature to fight alongside the caster for a while.", "Invoca uma criatura aliada para lutar ao lado do conjurador por um tempo."),
            SkillName("skill_super_jump", "Super Jump", "Super Salto"),
            SkillDescription("skill_super_jump", "Grants a vertical jump far higher than normal.", "Concede um salto vertical muito acima do normal."),
            SkillName("skill_teleport", "Teleport", "Teleporte"),
            SkillDescription("skill_teleport", "Instantly teleports the caster to the targeted point.", "Teleporta o conjurador instantaneamente para o ponto mirado."),
            SkillName("skill_thorns", "Thorns", "Espinhos"),
            SkillDescription("skill_thorns", "Passive: reflects part of the damage taken back at the attacker through magic thorns.", "Passiva: reflete parte do dano recebido de volta ao atacante através de espinhos mágicos."),
            SkillName("skill_tornado", "Tornado", "Tornado"),
            SkillDescription("skill_tornado", "Creates a tornado that continuously drags in and damages nearby enemies.", "Cria um tornado que arrasta e danifica inimigos próximos continuamente."),

            // -- Enemies (Resources/Enemies) --
            EnemyName("enemy_cave_bat_swarm", "Threshold Bat Swarm", "Enxame de Morcegos do Limiar"),
            EnemyName("enemy_cave_dweller", "Tages the Ancient Dweller", "Tages, o Habitante Ancestral"),
            EnemyName("enemy_desert_ruin_guardian", "The Great Ushabti", "O Grande Ushabti"),
            EnemyName("enemy_desert_scorpion", "Flaming Sands Scorpion", "Escorpião das Areias Flamejantes"),
            EnemyName("enemy_forest_alpha_wolf", "Kök-Böri, the Sky Wolf", "Kök-Böri, o Lobo Celeste"),
            EnemyName("enemy_forest_spider", "Dimmed Weaver Spider", "Aranha-Tecelã Apagada"),
            EnemyName("enemy_forest_wolf", "Tugai Wolf", "Lobo do Tugai"),
            EnemyName("enemy_mountain_stone_golem", "Stone Ushabti", "Ushabti de Pedra"),
            EnemyName("enemy_mountain_warlord", "Hahya the Giant Warrior", "Hahya, o Gigante Guerreiro"),
            // Night-only species (Docs/DayNight.md).
            EnemyName("enemy_forest_werewolf", "Mardagayl, the Seven-Night Wolf", "Mardagayl, o Lobo-das-Sete-Noites"),
            EnemyName("enemy_cave_vampire_bat", "Ubır, the Light-Drinker Bat", "Ubır, o Morcegão Bebe-Luz"),
            EnemyName("enemy_forest_shroud_owl", "Smoke Owl", "Coruja-de-Fumaça"),
            EnemyName("enemy_desert_firefly_swarm", "Swarm of Trapped Sparks", "Enxame de Faíscas Presas"),
            EnemyName("enemy_forest_peccary", "Mire Boar", "Javali do Lodo"),
            EnemyName("enemy_mountain_potoo", "Omöl's Twig-Bird", "Ave-Graveto de Omöl"),

            // -- Recipes (Resources/Recipes) --
            RecipeName("recipe_health_potion", "Health Potion", "Poção de Vida"),
            RecipeDescription("recipe_health_potion", "Brews a healing potion from gathered herbs.", "Prepara uma poção de cura a partir de ervas colhidas."),
            RecipeName("recipe_iron_ingot", "Iron Ingot", "Lingote de Ferro"),
            RecipeDescription("recipe_iron_ingot", "Smelts iron ore into a usable ingot.", "Funde minério de ferro em um lingote utilizável."),
            RecipeName("recipe_iron_sword", "Iron Sword", "Espada de Ferro"),
            RecipeDescription("recipe_iron_sword", "Forges an iron sword from ingots and wood.", "Forja uma espada de ferro a partir de lingotes e madeira."),

            // -- Combos (Resources/Combos) --
            ComboName("combo_dash_strike", "Lightning Rush", "Investida Relâmpago"),
            ComboDescription("combo_dash_strike", "A Dash followed by a Lightning Strike lowers the cost of your next skills.", "Dash seguido de um Ataque Relâmpago reduz o custo das próximas habilidades."),
            ComboName("combo_fire_and_ice", "Fire and Ice", "Fogo e Gelo"),
            ComboDescription("combo_fire_and_ice", "Cast Fireball followed by Ice Shard to deal extra damage for a while.", "Conjure Bola de Fogo seguida de Estilhaço de Gelo pra causar dano extra por um tempo."),

            // -- Skill trees (Resources/SkillTrees) --
            TreeName("tree_general", "General Tree", "Árvore Geral"),
            TreeDescription("tree_general", "Every sample skill available, laid out in a grid.", "Todas as habilidades de exemplo disponíveis, organizadas em grade."),
            TreeName("tree_icaro", "Ícaro's Tree", "Árvore de Ícaro"),
            TreeDescription("tree_icaro", "Ícaro's 5 signature skills: 4 actives and the Ultimate, which requires level 10 and the first skill learned.", "As 5 habilidades de assinatura de Ícaro: 4 ativas e a Ultimate, que exige nível 10 e a primeira habilidade aprendida."),
            TreeName("tree_joaquim", "Joaquim's Tree", "Árvore de Joaquim"),
            TreeDescription("tree_joaquim", "Joaquim's 5 signature skills: 4 actives and the Ultimate, which requires level 10 and the first skill learned.", "As 5 habilidades de assinatura de Joaquim: 4 ativas e a Ultimate, que exige nível 10 e a primeira habilidade aprendida."),
            TreeName("tree_lavine", "Lavine's Tree", "Árvore de Lavine"),
            TreeDescription("tree_lavine", "Lavine's 5 signature skills: 4 actives and the Ultimate, which requires level 10 and the first skill learned.", "As 5 habilidades de assinatura de Lavine: 4 ativas e a Ultimate, que exige nível 10 e a primeira habilidade aprendida."),
            TreeName("tree_maithe", "Maithe's Tree", "Árvore de Maithe"),
            TreeDescription("tree_maithe", "Maithe's 5 signature skills: 4 actives and the Ultimate, which requires level 10 and the first skill learned.", "As 5 habilidades de assinatura de Maithe: 4 ativas e a Ultimate, que exige nível 10 e a primeira habilidade aprendida."),
            TreeName("tree_maria", "Maria's Tree", "Árvore de Maria"),
            TreeDescription("tree_maria", "Maria's 5 signature skills: 4 actives and the Ultimate, which requires level 10 and the first skill learned.", "As 5 habilidades de assinatura de Maria: 4 ativas e a Ultimate, que exige nível 10 e a primeira habilidade aprendida."),

            // -- Playable characters (Gameplay/Characters, CharacterSelect screen) --
            CharacterName(CharacterId.Maithe, "Maithe", "Maithe"),
            CharacterClass(CharacterId.Maithe, "Huntress", "Caçadora"),
            CharacterRole(CharacterId.Maithe, "Ranged DPS", "DPS à distância"),
            CharacterLore(CharacterId.Maithe, "At 14, Maithe already spends more time exploring forbidden mountains and forests than at home. A prodigy with the bow, she hits targets grown hunters would miss, and the whole neighborhood knows it. On one of her escapades she found an ancient artifact called the Eye of the Wind, which let her see invisible threats. Now she wants to find out who created the monsters threatening the kingdoms. She is the eldest of the five cousins and Ícaro's big sister, which makes her the family's natural lead whenever it's time to face danger head-on — even though she, too, is only a kid.", "Aos 14 anos, Maithe já passa mais tempo explorando montanhas e florestas proibidas do que em casa. Precoce com o arco, acerta alvos que caçadores adultos errariam, e já é conhecida na vizinhança por isso. Numa de suas escapadas encontrou um artefato antigo chamado Olho do Vento, que lhe permitiu enxergar ameaças invisíveis. Agora busca descobrir quem criou os monstros que ameaçam os reinos. É a mais velha entre os cinco primos e irmã mais velha de Ícaro, o que a torna a referência natural de toda a família quando o assunto é enfrentar o perigo de frente — mesmo sendo, ela também, só uma criança."),
            CharacterName(CharacterId.Maria, "Maria", "Maria"),
            CharacterClass(CharacterId.Maria, "Healer", "Curandeira"),
            CharacterRole(CharacterId.Maria, "Support / Healing / Buffs", "Suporte / Cura / Buffs"),
            CharacterLore(CharacterId.Maria, "Maria, 13, grew up among the halls of light of the Solar Temple, where the elder healers taught her the first secrets of vital energy. When the Sun Crystal went dark and its light scattered into a thousand sparks, she dreamed of a call: only a band of heroes could light up Dawn Isle again. Brave for someone so young, she answered the Call and set out to gather the ones who had to come — and they were, above all, her own family: her eldest cousin Maithe, her little brother Joaquim and her cousins Ícaro and Lavine.", "Maria, de 13 anos, cresceu entre as salas de luz do Templo Solar, onde as curandeiras mais velhas lhe ensinaram os primeiros segredos da energia vital. Quando o Cristal do Sol apagou e a luz se espalhou em mil faíscas, ela sonhou com um chamado: só um grupo de heróis poderia acender de novo a Ilha da Aurora. Corajosa mesmo sendo tão nova, respondeu ao Chamado e partiu para reunir quem precisava vir — e quem precisava vir era, antes de tudo, sua própria família: sua prima mais velha Maithe, seu irmão mais novo Joaquim e seus primos Ícaro e Lavine."),
            CharacterName(CharacterId.Joaquim, "Joaquim", "Joaquim"),
            CharacterClass(CharacterId.Joaquim, "Guardian", "Guardião"),
            CharacterRole(CharacterId.Joaquim, "Tank / Defense / Party Protection", "Tanque / Defesa / Proteção do grupo"),
            CharacterLore(CharacterId.Joaquim, "Joaquim is only 12, but he grew up listening to the elders of his village, ringed by ancient woods, tell how to protect others from the dangers that came at night — and he decided he would be the one to do it someday. When the shadow seeping out of the Caves put out the old village where he was born, his family moved close to Grandma Edna, and the boy swore, with all of a child's stubbornness, to protect every innocent. He carries the Shield of Eternal Roots, an artifact handed down by his ancestors — far too big for him, but it soaks up part of the damage his allies take. Roots drink Living Water, and the shield is the river that lets nothing through. Maria's little brother and cousin to Maithe, Ícaro and Lavine, he's the oldest of the boys in the family and insists on playing protector for his younger cousins, Ícaro and Lavine, too, even when nobody asked.", "Joaquim tem apenas 12 anos, mas cresceu ouvindo os mais velhos de sua vila, cercada por bosques antigos, contarem como proteger os outros dos perigos que surgiam durante a noite — e decidiu que seria ele a fazer isso um dia. Quando a sombra que escorre das Grutas apagou a vila velha onde ele nasceu, a família foi morar perto da Vó Edna, e o garoto jurou, com toda a teimosia de uma criança, proteger todos os inocentes. Carrega consigo o Escudo das Raízes Eternas, um artefato herdado de seus ancestrais — grande demais pra ele, mas que absorve parte do dano sofrido pelos aliados. As raízes bebem Água Viva, e o escudo é o rio que não deixa passar. Irmão mais novo de Maria e primo de Maithe, Ícaro e Lavine, é o mais velho entre os meninos da família e insiste em bancar o protetor também dos primos mais novos, Ícaro e Lavine, mesmo quando ninguém pediu."),
            CharacterName(CharacterId.Icaro, "Ícaro", "Ícaro"),
            CharacterClass(CharacterId.Icaro, "Inventor", "Inventor"),
            CharacterRole(CharacterId.Icaro, "Control / Tech Damage", "Controle / Dano Tecnológico"),
            CharacterLore(CharacterId.Icaro, "At 11, Ícaro already believes technology can change the world. While the other kids played, he took apart and rebuilt everything he could get his hands on. After finding parts of the Engines of Light, the old machines of the Artificers, in a vacant lot near home, he started building little gadgets that answer when he calls and can surprise even the island's grown-up mages. He dreams of being an heir of Kurdalægon, the sky smith of the old tales, and of unlocking the secrets of the ancient technology that could light up Dawn Isle again. Maithe's little brother and Lavine's older cousin, he's always trying to impress his big sister with his inventions.", "Com 11 anos, Ícaro já acredita que a tecnologia pode mudar o mundo. Enquanto as outras crianças brincavam, ele desmontava e remontava tudo que encontrava. Depois de achar peças dos Engenhos da Luz, as máquinas antigas dos Artífices, num terreno baldio perto de casa, começou a montar pequenos equipamentos que respondem quando ele chama, capazes de surpreender até os magos adultos da ilha. Sonha em ser herdeiro de Kurdalægon, o ferreiro do céu dos contos, e desvendar os segredos da tecnologia antiga que pode acender de novo a Ilha da Aurora. Irmão mais novo de Maithe e primo mais velho de Lavine, vive tentando impressionar a irmã mais velha com suas invenções."),
            CharacterName(CharacterId.Lavine, "Lavine", "Lavine"),
            CharacterClass(CharacterId.Lavine, "Sorceress", "Feiticeira"),
            CharacterRole(CharacterId.Lavine, "Magic Damage / Crowd Control", "Dano Mágico / Controle de Multidão"),
            CharacterLore(CharacterId.Lavine, "Lavine, the youngest of them all at 9, was born during a rare eclipse known as the Noite Rubra. Ever since she was a baby she has shown an unusual bond with the Pure Air, the still and starry air of the night sky, and with a soft shadow that is hers alone — something far too big for a child to carry. Feared by many and misunderstood by others, she never stops proving that her shadow is not the Darkness, which is cold, sticky and loud. Despite her serious manner and cold look, deep down she's still just a girl who wants to prove her power can be used to protect the world. She's the youngest in the whole family, cousin to Maithe, Maria, Joaquim and Ícaro, and longs to be taken as seriously as her older cousins.", "Lavine, a mais nova de todos com 9 anos, nasceu durante um raro eclipse conhecido como Noite Rubra. Desde bebê demonstrou uma ligação incomum com o Ar Sereno, o ar quieto e estrelado do céu da noite, e com uma sombra macia que é só dela — algo grande demais pra uma criança carregar. Temida por muitos e incompreendida por outros, ela vive provando que a sua sombra não é a Treva, que é fria, pegajosa e barulhenta. Apesar do jeito sério e da aparência fria, no fundo ainda é só uma menina que deseja provar que seu poder pode ser usado para proteger o mundo. É a mais nova de toda a família, prima de Maithe, Maria, Joaquim e Ícaro, e carrega o desejo de ser levada tão a sério quanto os primos mais velhos."),
        };

        private static LocalizationManager _mergedInto;

        /// <summary>Merges <see cref="Entries"/> into <paramref name="manager"/>, skipping keys it already has (external tables take precedence). Safe to call repeatedly.</summary>
        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(Entries[0].Key)))
            {
                return;
            }

            manager.LoadEntries(Entries.Where(entry => !manager.HasKey(entry.Key)).ToList());
            _mergedInto = manager;
        }

        private static LocalizationEntry ItemName(string id, string en, string ptBR) => new LocalizationEntry(ContentText.ItemNameKey(id), en, ptBR);
        private static LocalizationEntry ItemDescription(string id, string en, string ptBR) => new LocalizationEntry(ContentText.ItemDescriptionKey(id), en, ptBR);
        private static LocalizationEntry SkillName(string id, string en, string ptBR) => new LocalizationEntry(ContentText.SkillNameKey(id), en, ptBR);
        private static LocalizationEntry SkillDescription(string id, string en, string ptBR) => new LocalizationEntry(ContentText.SkillDescriptionKey(id), en, ptBR);
        private static LocalizationEntry EnemyName(string assetName, string en, string ptBR) => new LocalizationEntry(ContentText.EnemyNameKey(assetName), en, ptBR);
        private static LocalizationEntry RecipeName(string id, string en, string ptBR) => new LocalizationEntry(ContentText.RecipeNameKey(id), en, ptBR);
        private static LocalizationEntry RecipeDescription(string id, string en, string ptBR) => new LocalizationEntry(ContentText.RecipeDescriptionKey(id), en, ptBR);
        private static LocalizationEntry ComboName(string id, string en, string ptBR) => new LocalizationEntry(ContentText.ComboNameKey(id), en, ptBR);
        private static LocalizationEntry ComboDescription(string id, string en, string ptBR) => new LocalizationEntry(ContentText.ComboDescriptionKey(id), en, ptBR);
        private static LocalizationEntry TreeName(string id, string en, string ptBR) => new LocalizationEntry(ContentText.TreeNameKey(id), en, ptBR);
        private static LocalizationEntry TreeDescription(string id, string en, string ptBR) => new LocalizationEntry(ContentText.TreeDescriptionKey(id), en, ptBR);
        private static LocalizationEntry CharacterName(CharacterId id, string en, string ptBR) => new LocalizationEntry(ContentText.CharacterNameKey(id), en, ptBR);
        private static LocalizationEntry CharacterClass(CharacterId id, string en, string ptBR) => new LocalizationEntry(ContentText.CharacterClassKey(id), en, ptBR);
        private static LocalizationEntry CharacterRole(CharacterId id, string en, string ptBR) => new LocalizationEntry(ContentText.CharacterRoleKey(id), en, ptBR);
        private static LocalizationEntry CharacterLore(CharacterId id, string en, string ptBR) => new LocalizationEntry(ContentText.CharacterLoreKey(id), en, ptBR);
    }
}
