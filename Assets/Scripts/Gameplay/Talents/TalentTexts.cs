using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Skills;

namespace Rapadura.Gameplay.Talents
{
    /// <summary>
    /// Localization (en + pt-BR) of the trained proficiencies ("Treino") and the "Roda de Talentos": node, quadrant and
    /// proficiency names, stat labels and the wheel window. Self-contained like <c>HuntingTexts</c>: merged into the live
    /// <see cref="LocalizationManager"/> on first use (keys it already has win), pt-BR fallback without a manager.
    /// </summary>
    public static class TalentTexts
    {
        public const string Title = "ui.talent.title";
        public const string OpenButton = "ui.talent.open";
        public const string Close = "ui.talent.close";
        public const string Intro = "ui.talent.intro";
        public const string Points = "ui.talent.points";
        public const string LockedLevel = "ui.talent.locked";
        public const string NoHero = "ui.talent.no_hero";
        public const string Respec = "ui.talent.respec";
        public const string RespecNothing = "ui.talent.respec_nothing";
        public const string RespecNoGold = "ui.talent.respec_no_gold";
        public const string RespecDone = "ui.talent.respec_done";
        public const string Rank = "ui.talent.rank";
        public const string Keystone = "ui.talent.keystone";
        public const string NeedsNode = "ui.talent.needs_node";
        public const string NeedsQuadrant = "ui.talent.needs_quadrant";
        public const string NoPoints = "ui.talent.no_points";
        public const string Spent = "ui.talent.spent";
        public const string QuadrantPoints = "ui.talent.quadrant_points";
        public const string Selected = "ui.talent.selected";
        public const string SpendButton = "ui.talent.spend";

        public const string TrainingHeader = "ui.talent.training.header";
        public const string TrainingRow = "ui.talent.training.row";
        public const string TrainingMax = "ui.talent.training.max";

        public static string NodeNameKey(string nodeId) => "talent.node." + nodeId + ".name";
        public static string QuadrantKey(TalentQuadrant quadrant) => "talent.quadrant." + quadrant.ToString().ToLowerInvariant();
        public static string ProficiencyKey(ProficiencyKind kind) => "talent.proficiency." + kind.ToString().ToLowerInvariant();
        public static string StatKey(StatType stat) => "talent.stat." + stat.ToString().ToLowerInvariant();

        /// <summary>Stats a node or a proficiency can give (each needs a <see cref="StatKey"/>).</summary>
        public static readonly StatType[] UsedStats =
        {
            StatType.AttackDamage, StatType.CriticalChance, StatType.CriticalDamage, StatType.CooldownReduction,
            StatType.MaxHealth, StatType.Defense, StatType.Thorns, StatType.MaxMana, StatType.HealthRegen,
            StatType.ManaRegen, StatType.ResourceCostReduction, StatType.MoveSpeed, StatType.MaxStamina,
            StatType.StaminaRegen, StatType.AttackSpeed,
        };

        private static LocalizationManager _mergedInto;
        private static Dictionary<string, string> _fallbackByKey;

        public static readonly List<LocalizationEntry> Entries = new List<LocalizationEntry>
        {
            new LocalizationEntry(Title, "Talent Wheel", "Roda de Talentos"),
            new LocalizationEntry(OpenButton, "Talent Wheel", "Roda de Talentos"),
            new LocalizationEntry(Close, "Back", "Voltar"),
            new LocalizationEntry(Intro, "Each level from {0} on gives 1 point. Inner nodes open the outer ones; the keystone needs {1} points in its quarter.",
                "Cada nível a partir do {0} dá 1 ponto. Os nós de dentro abrem os de fora; a pedra-chave pede {1} pontos no seu quarto."),
            new LocalizationEntry(Points, "Points to spend: {0}", "Pontos para gastar: {0}"),
            new LocalizationEntry(LockedLevel, "The wheel opens at level {0}.", "A roda abre no nível {0}."),
            new LocalizationEntry(NoHero, "Pick a cousin first.", "Escolha um primo primeiro."),
            new LocalizationEntry(Respec, "Start over ({0} gold)", "Recomeçar ({0} de ouro)"),
            new LocalizationEntry(RespecNothing, "Nothing to reset yet.", "Nada para recomeçar ainda."),
            new LocalizationEntry(RespecNoGold, "Not enough gold: {0} needed.", "Ouro insuficiente: precisa de {0}."),
            new LocalizationEntry(RespecDone, "Wheel cleared: {0} points back.", "Roda limpa: {0} pontos de volta."),
            new LocalizationEntry(Rank, "{0}/{1}", "{0}/{1}"),
            new LocalizationEntry(Keystone, "Keystone", "Pedra-chave"),
            new LocalizationEntry(NeedsNode, "Needs {0} in {1}.", "Precisa de {0} em {1}."),
            new LocalizationEntry(NeedsQuadrant, "Needs {0} points in {1}.", "Precisa de {0} pontos em {1}."),
            new LocalizationEntry(NoPoints, "No points left.", "Sem pontos."),
            new LocalizationEntry(Spent, "Learned: {0}", "Aprendido: {0}"),
            new LocalizationEntry(QuadrantPoints, "{0}: {1}", "{0}: {1}"),
            new LocalizationEntry(Selected, "{0} — {1} ({2})", "{0} — {1} ({2})"),
            new LocalizationEntry(SpendButton, "Learn", "Aprender"),

            new LocalizationEntry(TrainingHeader, "Training (rises with use)", "Treino (sobe com o uso)"),
            new LocalizationEntry(TrainingRow, "{0} {1} ({2}%)", "{0} {1} ({2}%)"),
            new LocalizationEntry(TrainingMax, "{0} {1} (max)", "{0} {1} (máx.)"),

            new LocalizationEntry("talent.quadrant.offence", "Attack", "Ataque"),
            new LocalizationEntry("talent.quadrant.defence", "Defence", "Defesa"),
            new LocalizationEntry("talent.quadrant.support", "Support", "Apoio"),
            new LocalizationEntry("talent.quadrant.utility", "Utility", "Utilidade"),

            new LocalizationEntry("talent.proficiency.weapon", "Weapon", "Golpe"),
            new LocalizationEntry("talent.proficiency.magic", "Magic", "Magia"),
            new LocalizationEntry("talent.proficiency.shielding", "Guard", "Defesa"),

            new LocalizationEntry("talent.stat.attackdamage", "damage", "dano"),
            new LocalizationEntry("talent.stat.criticalchance", "crit chance", "chance de crítico"),
            new LocalizationEntry("talent.stat.criticaldamage", "crit damage", "dano crítico"),
            new LocalizationEntry("talent.stat.cooldownreduction", "cooldown reduction", "recarga menor"),
            new LocalizationEntry("talent.stat.maxhealth", "max HP", "vida máx."),
            new LocalizationEntry("talent.stat.defense", "defence", "defesa"),
            new LocalizationEntry("talent.stat.thorns", "thorns", "espinhos"),
            new LocalizationEntry("talent.stat.maxmana", "max mana", "mana máx."),
            new LocalizationEntry("talent.stat.healthregen", "HP regen", "regeneração de vida"),
            new LocalizationEntry("talent.stat.manaregen", "mana regen", "regeneração de mana"),
            new LocalizationEntry("talent.stat.resourcecostreduction", "cheaper skills", "habilidades mais baratas"),
            new LocalizationEntry("talent.stat.movespeed", "move speed", "velocidade"),
            new LocalizationEntry("talent.stat.maxstamina", "max stamina", "fôlego máx."),
            new LocalizationEntry("talent.stat.staminaregen", "stamina regen", "recuperação de fôlego"),
            new LocalizationEntry("talent.stat.attackspeed", "attack speed", "velocidade de ataque"),

            new LocalizationEntry("talent.node.mao_certeira.name", "Steady Hand", "Mão Certeira"),
            new LocalizationEntry("talent.node.olho_de_gaviao.name", "Hawk Eye", "Olho de Gavião"),
            new LocalizationEntry("talent.node.golpe_pesado.name", "Heavy Blow", "Golpe Pesado"),
            new LocalizationEntry("talent.node.aboio_de_guerra.name", "War Cattle Call", "Aboio de Guerra"),
            new LocalizationEntry("talent.node.prece_ardente.name", "Burning Prayer", "Prece Ardente"),
            new LocalizationEntry("talent.node.flecha_do_carcara.name", "Caracara Arrow", "Flecha do Carcará"),
            new LocalizationEntry("talent.node.engrenagem_afiada.name", "Sharp Gear", "Engrenagem Afiada"),
            new LocalizationEntry("talent.node.chama_do_vagalume.name", "Firefly Flame", "Chama do Vaga-lume"),

            new LocalizationEntry("talent.node.couro_grosso.name", "Thick Hide", "Couro Grosso"),
            new LocalizationEntry("talent.node.pele_de_mandacaru.name", "Mandacaru Skin", "Pele de Mandacaru"),
            new LocalizationEntry("talent.node.espinho_de_xiquexique.name", "Xique-xique Thorn", "Espinho de Xique-xique"),
            new LocalizationEntry("talent.node.gibao_de_couro.name", "Leather Doublet", "Gibão de Couro"),
            new LocalizationEntry("talent.node.manto_de_reza.name", "Prayer Mantle", "Manto de Reza"),
            new LocalizationEntry("talent.node.pena_ligeira.name", "Swift Feather", "Pena Ligeira"),
            new LocalizationEntry("talent.node.blindagem_de_lata.name", "Tin Plating", "Blindagem de Lata"),
            new LocalizationEntry("talent.node.luz_que_protege.name", "Guarding Light", "Luz que Protege"),

            new LocalizationEntry("talent.node.cura_de_quintal.name", "Backyard Remedy", "Cura de Quintal"),
            new LocalizationEntry("talent.node.folego_de_reza.name", "Prayer Breath", "Fôlego de Reza"),
            new LocalizationEntry("talent.node.mente_mansa.name", "Calm Mind", "Mente Mansa"),
            new LocalizationEntry("talent.node.berrante_amigo.name", "Friendly Horn", "Berrante Amigo"),
            new LocalizationEntry("talent.node.benzedura_forte.name", "Strong Blessing", "Benzedura Forte"),
            new LocalizationEntry("talent.node.ninho_seguro.name", "Safe Nest", "Ninho Seguro"),
            new LocalizationEntry("talent.node.pilha_reserva.name", "Spare Battery", "Pilha Reserva"),
            new LocalizationEntry("talent.node.vaga_lume_sereno.name", "Serene Firefly", "Vaga-lume Sereno"),

            new LocalizationEntry("talent.node.pe_ligeiro.name", "Light Feet", "Pé Ligeiro"),
            new LocalizationEntry("talent.node.vigor_sertanejo.name", "Backlands Vigour", "Vigor Sertanejo"),
            new LocalizationEntry("talent.node.folego_largo.name", "Deep Breath", "Fôlego Largo"),
            new LocalizationEntry("talent.node.trote_de_vaqueiro.name", "Cowboy Trot", "Trote de Vaqueiro"),
            new LocalizationEntry("talent.node.romeira_incansavel.name", "Tireless Pilgrim", "Romeira Incansável"),
            new LocalizationEntry("talent.node.voo_rasante.name", "Low Swoop", "Voo Rasante"),
            new LocalizationEntry("talent.node.bota_de_mola.name", "Spring Boots", "Bota de Mola"),
            new LocalizationEntry("talent.node.rastro_de_luz.name", "Trail of Light", "Rastro de Luz"),
        };

        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(Title)))
            {
                return;
            }

            manager.LoadEntries(Entries.Where(entry => !manager.HasKey(entry.Key)).ToList());
            _mergedInto = manager;
        }

        public static LocalizationManager Live => ServiceLocator.TryGet(out LocalizationManager manager) ? manager : null;

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                EnsureRegistered(manager);
                if (manager.HasKey(key))
                {
                    return manager.Get(key);
                }
            }

            return GetFallback(key) ?? key;
        }

        public static string Format(LocalizationManager manager, string key, params object[] args)
        {
            string format = Get(manager, key);
            try
            {
                return string.Format(CultureInfo.InvariantCulture, format, args);
            }
            catch (System.FormatException)
            {
                return format;
            }
        }

        public static string GetFallback(string key)
        {
            if (_fallbackByKey == null)
            {
                _fallbackByKey = new Dictionary<string, string>();
                foreach (LocalizationEntry entry in Entries)
                {
                    _fallbackByKey[entry.Key] = entry.PtBR;
                }
            }

            return key != null && _fallbackByKey.TryGetValue(key, out string text) ? text : null;
        }

        /// <summary>A bonus as "+0,6% dano" (pt-BR) / "+0.6% damage" (en): every wheel/training stat is a 0-based fraction.</summary>
        public static string FormatBonus(LocalizationManager manager, StatType stat, float value)
        {
            float percent = (float)System.Math.Round(value * 1000f) / 10f;
            string number = percent.ToString("0.#", CultureInfo.InvariantCulture);
            bool english = manager != null && manager.CurrentLanguage == LanguageCode.en;
            return "+" + (english ? number : number.Replace('.', ',')) + "% " + Get(manager, StatKey(stat));
        }
    }
}
