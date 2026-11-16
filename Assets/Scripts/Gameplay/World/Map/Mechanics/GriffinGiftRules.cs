using Rapadura.Gameplay.Characters;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>What [E] at the old griffin's hoard does (<see cref="GriffinGiftRules.Check"/>).</summary>
    public enum GriffinGiftCheck
    {
        /// <summary>The griffin still guards its gold (not freed yet): nothing to take.</summary>
        Guarded = 0,
        Take = 1,

        /// <summary>This cousin already took their present.</summary>
        Taken = 2,
    }

    /// <summary>One cousin's present from the old griffin: the item and how many.</summary>
    public readonly struct GriffinGift
    {
        public readonly string ItemId;
        public readonly int Quantity;

        public GriffinGift(string itemId, int quantity)
        {
            ItemId = itemId;
            Quantity = quantity;
        }
    }

    /// <summary>
    /// The presents of O Grifo Velho do Kurgan (map 48, Docs/Mapas100.md: "libertado, deixa um presente para cada primo"), pure and
    /// server-runnable. Once the griffin is freed (the boss defeated, kept by the save's defeated registry) its five little pieces of gold lie on
    /// the hoard, the 8 m ring where it was found; each cousin takes their own once, with [E] at the ring's centre:
    /// <code>
    /// Maithe: the gold panther plaque + 2 Runas do Vento (she hears the Living Wind)
    /// Ícaro: the gold comb of the riders + 1 Pedra do Caos (for his tinkering)
    /// Joaquim: the gold stag + 2 Grandes Runas de Cura (his Living Water)
    /// Maria: the gold bell of the horse + the Amuleto da Sorte
    /// Lavine: the gold cup of the sky + 2 Pedras da Bênção (the youngest, who lifts the Ouros do Céu)
    /// reach 3 m from the ring's centre · once per cousin, ever (a mark in the save per cousin, set when taken)
    /// </code>
    /// Decision (2026-11-16): the presents are items the game already has, chosen for each cousin's way of fighting, so each one is worth the
    /// trip back with that cousin; the gold piece itself is the story (named in the line), not a new item.
    /// </summary>
    public static class GriffinGiftRules
    {
        public const string GriffinEnemyId = "enemy_mountain_old_kurgan_griffin";
        public const float Reach = 3f;
        private const string KeyPrefix = "nartes.griffin_gift.";

        /// <summary>The present of <paramref name="hero"/>.</summary>
        public static GriffinGift GiftFor(CharacterId hero)
        {
            switch (hero)
            {
                case CharacterId.Maithe: return new GriffinGift("item_runa_vento", 2);
                case CharacterId.Icaro: return new GriffinGift("item_pedra_caos", 1);
                case CharacterId.Joaquim: return new GriffinGift("item_runa_cura_grande", 2);
                case CharacterId.Maria: return new GriffinGift("item_amuleto_sorte", 1);
                default: return new GriffinGift("item_pedra_bencao", 2);
            }
        }

        /// <summary>The save key (a time mark: 0 = not taken) of <paramref name="hero"/>'s present.</summary>
        public static string TakenKey(CharacterId hero) => KeyPrefix + hero.ToString().ToLowerInvariant();

        public static GriffinGiftCheck Check(bool griffinFreed, bool taken)
        {
            if (!griffinFreed)
            {
                return GriffinGiftCheck.Guarded;
            }

            return taken ? GriffinGiftCheck.Taken : GriffinGiftCheck.Take;
        }

        /// <summary>Whether <paramref name="p"/> is at the hoard (<paramref name="hoard"/>, the ring's centre).</summary>
        public static bool AtHoard(Vector2 hoard, Vector2 p) => (hoard - p).sqrMagnitude <= Reach * Reach;
    }
}
