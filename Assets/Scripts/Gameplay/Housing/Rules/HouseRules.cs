using System;

namespace Rapadura.Gameplay.Housing
{
    /// <summary>Steps of "Missão da Casa 1 — Um Cantinho Seu" (Housing.md §2.1, §7.3), saved as <c>houseQuestStep</c>.</summary>
    public enum HouseQuestStep
    {
        /// <summary>No house yet (new game, or an old save before the quest beat).</summary>
        NotStarted = 0,
        /// <summary>Vó Zefa showed the cajá and handed over the key: the house exists, the cousin has not gone in yet.</summary>
        HouseGiven = 1,
        /// <summary>The cousin went inside: quest done.</summary>
        Completed = 2,
    }

    /// <summary>Small pure rules of the house (quest beat, bed, access, chest), Unity-free.</summary>
    public static class HouseRules
    {
        public const string FirstQuestId = "house_quest_um_cantinho_seu";
        /// <summary>"Logo depois do prólogo, na primeira volta à vila (depois das 3 primeiras vitórias na Floresta)" (§2.1).</summary>
        public const int ForestVictoriesForFirstQuest = 3;
        public const float MorningHour = 7f;
        public const int HoursPerDay = 24;
        /// <summary>The account owner offline (and the only "account" of an offline save).</summary>
        public const string LocalAccountId = "local";

        /// <summary>The rewards of "Um Cantinho Seu" (§2.1): chest, wardrobe, folding bed, Caramelo's doghouse, guest book.</summary>
        public static readonly string[] FirstQuestRewards =
        {
            "casa_util_001", "casa_util_006", "casa_quarto_016", "casa_curral_001", "casa_util_023",
        };

        /// <summary>
        /// A small starter set on top of the quest rewards so every layer can be tried right away (H1 testability):
        /// rug, cuckoo clock (wall), hanging fern (ceiling), log centre table (surface spots), stool, kerosene lamp and
        /// rosemary pot (surface), raised vegetable bed (yard), chintz curtain (window).
        /// </summary>
        public static readonly string[] StarterItems =
        {
            "casa_tapete_001", "casa_parede_001", "casa_planta_001", "casa_sala_004", "casa_sala_008", "casa_luz_001", "casa_planta_004",
            "casa_quintal_002", "casa_cortina_001",
        };

        /// <summary>Whether the "Minha casa" entry is offered at all (house owned, or the quest beat reached).</summary>
        public static bool IsHouseOffered(HouseQuestStep step, int forestVictories)
        {
            return step != HouseQuestStep.NotStarted || forestVictories >= ForestVictoriesForFirstQuest;
        }

        public static bool HasHouse(HouseQuestStep step) => step != HouseQuestStep.NotStarted;

        /// <summary>"(!)" on the button: Vó Zefa is waiting, or the key is in hand and the cousin has not gone in.</summary>
        public static bool HasQuestNews(HouseQuestStep step, int forestVictories)
        {
            return step == HouseQuestStep.HouseGiven || (step == HouseQuestStep.NotStarted && forestVictories >= ForestVictoriesForFirstQuest);
        }

        /// <summary>In-game hours from <paramref name="currentHour"/> to the next <see cref="MorningHour"/> (sleeping in the bed).</summary>
        public static float HoursUntilMorning(float currentHour)
        {
            float hour = ((currentHour % HoursPerDay) + HoursPerDay) % HoursPerDay;
            float hours = MorningHour - hour;
            if (hours <= 0f)
            {
                hours += HoursPerDay;
            }

            return hours;
        }

        /// <summary>
        /// Who may come in (Housing.md §4, user decision 2026-10-29): the owner always; anyone else only while the
        /// owner is online and with an invite or a place on the house list. Owner offline = locked for everyone.
        /// </summary>
        public static bool CanEnter(bool isOwner, bool ownerOnline, bool invitedOrListed)
        {
            return isOwner || (ownerOnline && invitedOrListed);
        }

        /// <summary>The visitor never opens a chest, whatever the permission (§4.3).</summary>
        public static bool CanOpenChest(bool isOwner) => isOwner;

        /// <summary>House chest capacity: the level's slots plus the extra chests placed (§5).</summary>
        public static int ChestCapacity(HouseLevelSpec spec, int extraSlots) => Math.Max(1, (spec?.ChestSlots ?? 20) + Math.Max(0, extraSlots));

        /// <summary>An extra chest can only be put away if what is stored still fits without it.</summary>
        public static bool CanShrinkChest(int usedSlots, int capacityAfter) => usedSlots <= capacityAfter;
    }
}
