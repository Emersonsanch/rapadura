using System;
using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Player;
using Rapadura.UI.Common;

namespace Rapadura.UI.CharacterSheet
{
    /// <summary>
    /// Localization keys (en + pt-BR) for the Ficha do personagem screen (<see cref="CharacterSheetController"/>,
    /// Resources/UI/CharacterSheetView.uxml) and the HUD "Ficha" button / unspent-points badge. Same
    /// self-contained pattern as <c>TutorialTexts</c>/<c>ReturnToTitleTexts</c>: merged into the live
    /// <see cref="LocalizationManager"/> on first use (keys it already has win), pt-BR fallback when no
    /// manager is registered. Attribute names reuse the pt-BR labels the lore already uses
    /// (<c>PlayableCharacter.PrimaryAttribute</c>: Vitalidade/Espírito/Destreza/Inteligência/Poder Arcano).
    /// </summary>
    public static class CharacterSheetTexts
    {
        public const string Title = "ui.sheet.title";
        public const string Close = "ui.sheet.close";
        public const string Level = "ui.sheet.level";
        public const string Experience = "ui.sheet.experience";
        public const string PointsAvailable = "ui.sheet.points_available";
        public const string NoPoints = "ui.sheet.no_points";
        public const string AttributesHeader = "ui.sheet.attributes";
        public const string DerivedHeader = "ui.sheet.derived";
        public const string EquipmentHeader = "ui.sheet.equipment";
        public const string EquipmentEmpty = "ui.sheet.equipment_empty";
        public const string NoHero = "ui.sheet.no_hero";
        public const string PrimaryTag = "ui.sheet.primary";
        public const string TalentBonus = "ui.sheet.talent_bonus";
        public const string SpendButton = "ui.sheet.spend";
        public const string SpendButtonA11y = "ui.sheet.spend_a11y";

        public const string StatMaxHealth = "ui.sheet.stat.max_health";
        public const string StatMaxMana = "ui.sheet.stat.max_mana";
        public const string StatMaxStamina = "ui.sheet.stat.max_stamina";
        public const string StatCritChance = "ui.sheet.stat.crit_chance";
        public const string StatCritMultiplier = "ui.sheet.stat.crit_multiplier";
        public const string StatCooldownReduction = "ui.sheet.stat.cooldown_reduction";
        public const string StatSkillDamage = "ui.sheet.stat.skill_damage";
        public const string StatAttackSpeed = "ui.sheet.stat.attack_speed";
        public const string StatThorns = "ui.sheet.stat.thorns";

        public const string PreviewVitality = "ui.sheet.preview.vitality";
        public const string PreviewSpirit = "ui.sheet.preview.spirit";
        public const string PreviewDexterity = "ui.sheet.preview.dexterity";
        public const string PreviewIntelligence = "ui.sheet.preview.intelligence";
        public const string PreviewArcanePower = "ui.sheet.preview.arcane_power";

        /// <summary>HUD cluster button (HudView.uxml, view-data-key) and the tutorial level-up outline target.</summary>
        public const string HudButton = "ui.hud.character_sheet";

        /// <summary>Prefix of the per-attribute name keys (see <see cref="AttributeNameKey"/>).</summary>
        public const string AttributeKeyPrefix = "ui.sheet.attribute.";

        public static string AttributeNameKey(AttributeType type) => AttributeKeyPrefix + TooltipLocalization.ToSnakeCase(type.ToString());

        public static string PreviewKey(AttributeType type)
        {
            switch (type)
            {
                case AttributeType.Vitality: return PreviewVitality;
                case AttributeType.Spirit: return PreviewSpirit;
                case AttributeType.Dexterity: return PreviewDexterity;
                case AttributeType.Intelligence: return PreviewIntelligence;
                case AttributeType.ArcanePower: return PreviewArcanePower;
                default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public static readonly List<LocalizationEntry> Entries = new List<LocalizationEntry>
        {
            new LocalizationEntry(Title, "Character sheet", "Ficha do personagem"),
            new LocalizationEntry(Close, "Close", "Fechar"),
            new LocalizationEntry(Level, "Level {0}", "Nível {0}"),
            new LocalizationEntry(Experience, "XP {0}/{1}", "XP {0}/{1}"),
            new LocalizationEntry(PointsAvailable, "Points to spend: {0}", "Pontos para distribuir: {0}"),
            new LocalizationEntry(NoPoints, "Level up to earn more points.", "Suba de nível para ganhar mais pontos."),
            new LocalizationEntry(AttributesHeader, "Attributes", "Atributos"),
            new LocalizationEntry(DerivedHeader, "Stats", "Status"),
            new LocalizationEntry(EquipmentHeader, "Equipped", "Equipado"),
            new LocalizationEntry(EquipmentEmpty, "Nothing equipped yet.", "Nada equipado ainda."),
            new LocalizationEntry(NoHero, "Hero", "Herói"),
            new LocalizationEntry(PrimaryTag, "Main", "Principal"),
            new LocalizationEntry(TalentBonus, "(+{0} talent)", "(+{0} talento)"),
            new LocalizationEntry(SpendButton, "+", "+"),
            new LocalizationEntry(SpendButtonA11y, "Add a point to {0}", "Pôr um ponto em {0}"),

            new LocalizationEntry(AttributeNameKey(AttributeType.Vitality), "Vitality", "Vitalidade"),
            new LocalizationEntry(AttributeNameKey(AttributeType.Spirit), "Spirit", "Espírito"),
            new LocalizationEntry(AttributeNameKey(AttributeType.Dexterity), "Dexterity", "Destreza"),
            new LocalizationEntry(AttributeNameKey(AttributeType.Intelligence), "Intelligence", "Inteligência"),
            new LocalizationEntry(AttributeNameKey(AttributeType.ArcanePower), "Arcane Power", "Poder Arcano"),

            new LocalizationEntry(PreviewVitality, "+{0} Max HP", "+{0} Vida máx."),
            new LocalizationEntry(PreviewSpirit, "+{0} Max mana", "+{0} Mana máx."),
            new LocalizationEntry(PreviewDexterity, "+{0}% Crit chance", "+{0}% Chance de crítico"),
            new LocalizationEntry(PreviewIntelligence, "+{0}% Cooldown reduction", "+{0}% Redução de recarga"),
            new LocalizationEntry(PreviewArcanePower, "+{0}% Skill damage", "+{0}% Dano de habilidade"),

            new LocalizationEntry(StatMaxHealth, "Max HP", "Vida máx."),
            new LocalizationEntry(StatMaxMana, "Max mana", "Mana máx."),
            new LocalizationEntry(StatMaxStamina, "Max stamina", "Stamina máx."),
            new LocalizationEntry(StatCritChance, "Crit chance", "Chance de crítico"),
            new LocalizationEntry(StatCritMultiplier, "Crit damage", "Dano crítico"),
            new LocalizationEntry(StatCooldownReduction, "Cooldown reduction", "Redução de recarga"),
            new LocalizationEntry(StatSkillDamage, "Skill damage", "Dano de habilidade"),
            new LocalizationEntry(StatAttackSpeed, "Attack speed", "Velocidade de ataque"),
            new LocalizationEntry(StatThorns, "Thorns", "Espinhos"),

            new LocalizationEntry(HudButton, "Sheet", "Ficha"),
        };

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        /// <summary>True if <paramref name="key"/> is defined here (used by the HUD's resolver for its "Ficha" button).</summary>
        public static bool IsOwnKey(string key) => GetFallback(key) != null;

        /// <summary>Merges <see cref="Entries"/> into <paramref name="manager"/>, skipping keys it already has. Safe to call repeatedly.</summary>
        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(Title)))
            {
                return;
            }

            manager.LoadEntries(Entries.Where(entry => !manager.HasKey(entry.Key)).ToList());
            _mergedInto = manager;
        }

        /// <summary>
        /// Localized text via <paramref name="manager"/> when available, else the pt-BR fallback. Keys
        /// that aren't ours go through <see cref="UiTexts.Get"/>, so this also works as a
        /// <see cref="UiDocumentLocalizer"/> resolver for a tree mixing both sets of keys.
        /// </summary>
        public static string Get(LocalizationManager manager, string key)
        {
            if (!IsOwnKey(key))
            {
                return UiTexts.Get(manager, key);
            }

            if (manager != null)
            {
                EnsureRegistered(manager);
                if (manager.HasKey(key))
                {
                    return manager.Get(key);
                }
            }

            return GetFallback(key);
        }

        /// <summary><see cref="Get"/> + <see cref="string.Format(string, object[])"/>, tolerant of malformed translations.</summary>
        public static string Format(LocalizationManager manager, string key, params object[] args)
        {
            string pattern = Get(manager, key);
            try
            {
                return string.Format(pattern, args);
            }
            catch (FormatException)
            {
                return string.Format(GetFallback(key) ?? key, args);
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
    }
}
