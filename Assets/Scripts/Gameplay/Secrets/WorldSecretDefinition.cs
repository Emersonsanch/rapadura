using System.Collections.Generic;
using Rapadura.Gameplay.Story;
using Rapadura.Gameplay.World.Biomes;

namespace Rapadura.Gameplay.Secrets
{
    /// <summary>What makes a world-map secret show itself while the hero token stands on its region.</summary>
    public enum SecretRevealRule
    {
        /// <summary>The token stays idle on the region for <see cref="WorldSecretDefinition.IdleSeconds"/> (real time, HUD up, not paused).</summary>
        Idle = 0,
        /// <summary>It is night (<c>TimeOfDayPhase.Night</c>) while the token is on the region.</summary>
        Night = 1,
        /// <summary>The story beat <see cref="WorldSecretDefinition.Beat"/> was already seen.</summary>
        StoryBeat = 2,
    }

    /// <summary>What "Investigar" does once a secret is revealed.</summary>
    public enum SecretVisitKind
    {
        /// <summary>The relic (and the bonus gold) is handed over right there on the map.</summary>
        Reward = 0,
        /// <summary>A small fight in the region's arena guards it; the relic is granted on victory.</summary>
        Encounter = 1,
    }

    /// <summary>The visible hint prop placed next to the region's node (see <see cref="WorldMapSecretHints"/>).</summary>
    public enum SecretHintKind
    {
        FlourHouseRuin = 0,
        HollowTree = 1,
        MandacaruChest = 2,
        DryWell = 3,
        WaterfallCave = 4,
        TropeiroCairn = 5,
        ForgottenWorkshop = 6,
        RockPaintings = 7,
    }

    /// <summary>
    /// A hidden spot on the world map. Its hint prop is always on the map (a ruin, a hollow tree, a waterfall…);
    /// once <see cref="Rule"/> is met with the token on <see cref="Region"/> it is revealed (a gold marker bobs over
    /// it and "Investigar" appears), and investigating hands out <see cref="RelicId"/> plus <see cref="BonusGold"/>.
    /// Texts: <c>secret.&lt;id&gt;.name/.reveal</c> in <see cref="SecretTexts"/>. Ids are persisted.
    /// </summary>
    public sealed class WorldSecretDefinition
    {
        public readonly string Id;
        public readonly BiomeType Region;
        public readonly SecretRevealRule Rule;
        public readonly float IdleSeconds;
        public readonly StoryBeat Beat;
        public readonly SecretVisitKind Visit;
        public readonly SecretHintKind Hint;
        public readonly string RelicId;
        public readonly int BonusGold;

        /// <summary>Direction (degrees from +x, +90 = away from the camera) the hint prefers around the node.</summary>
        public readonly float PreferredAngle;

        public WorldSecretDefinition(string id, BiomeType region, SecretRevealRule rule, float idleSeconds, StoryBeat beat,
            SecretVisitKind visit, SecretHintKind hint, string relicId, int bonusGold, float preferredAngle)
        {
            Id = id;
            Region = region;
            Rule = rule;
            IdleSeconds = idleSeconds;
            Beat = beat;
            Visit = visit;
            Hint = hint;
            RelicId = relicId;
            BonusGold = bonusGold;
            PreferredAngle = preferredAngle;
        }

        public string NameKey => "secret." + Id + ".name";
        public string RevealKey => "secret." + Id + ".reveal";
    }

    /// <summary>
    /// Two hidden spots per region (Floresta → Deserto → Montanha → Cavernas), one relic each: in every region one
    /// opens to patience (idle), the other to the night or to coming back after the region's boss (story beat),
    /// so the map rewards stopping to look and returning to places already cleared.
    /// </summary>
    public static class WorldSecretCatalog
    {
        public const string CasaDeFarinha = "secret_casa_de_farinha";
        public const string JatobaOco = "secret_jatoba_oco";
        public const string BauMandacaru = "secret_bau_mandacaru";
        public const string CacimbaSeca = "secret_cacimba_seca";
        public const string GrutaCachoeira = "secret_gruta_cachoeira";
        public const string MarcoTropeiros = "secret_marco_tropeiros";
        public const string OficinaEsquecida = "secret_oficina_esquecida";
        public const string PinturasRocha = "secret_pinturas_rocha";

        public const float ShortIdleSeconds = 6f;
        public const float LongIdleSeconds = 9f;

        private static readonly WorldSecretDefinition[] AllSecrets =
        {
            new WorldSecretDefinition(CasaDeFarinha, BiomeType.Forest, SecretRevealRule.Idle, ShortIdleSeconds, StoryBeat.None,
                SecretVisitKind.Reward, SecretHintKind.FlourHouseRuin, RelicCatalog.GaitaVoChico, 25, 160f),
            new WorldSecretDefinition(JatobaOco, BiomeType.Forest, SecretRevealRule.Night, 0f, StoryBeat.None,
                SecretVisitKind.Reward, SecretHintKind.HollowTree, RelicCatalog.ArcoMaithe, 25, 20f),
            new WorldSecretDefinition(BauMandacaru, BiomeType.Desert, SecretRevealRule.Idle, LongIdleSeconds, StoryBeat.None,
                SecretVisitKind.Reward, SecretHintKind.MandacaruChest, RelicCatalog.CartasVoChico, 40, 150f),
            new WorldSecretDefinition(CacimbaSeca, BiomeType.Desert, SecretRevealRule.StoryBeat, 0f, StoryBeat.InterludeDesert,
                SecretVisitKind.Encounter, SecretHintKind.DryWell, RelicCatalog.EscudinhoJoaquim, 40, 30f),
            new WorldSecretDefinition(GrutaCachoeira, BiomeType.Mountain, SecretRevealRule.StoryBeat, 0f, StoryBeat.InterludeMountain,
                SecretVisitKind.Encounter, SecretHintKind.WaterfallCave, RelicCatalog.MedalhaMaria, 60, 140f),
            new WorldSecretDefinition(MarcoTropeiros, BiomeType.Mountain, SecretRevealRule.Idle, LongIdleSeconds, StoryBeat.None,
                SecretVisitKind.Reward, SecretHintKind.TropeiroCairn, RelicCatalog.ChocalhoTeimosa, 60, 30f),
            new WorldSecretDefinition(OficinaEsquecida, BiomeType.Caves, SecretRevealRule.Idle, LongIdleSeconds, StoryBeat.None,
                SecretVisitKind.Reward, SecretHintKind.ForgottenWorkshop, RelicCatalog.EngrenagemIcaro, 80, 150f),
            new WorldSecretDefinition(PinturasRocha, BiomeType.Caves, SecretRevealRule.Night, 0f, StoryBeat.None,
                SecretVisitKind.Encounter, SecretHintKind.RockPaintings, RelicCatalog.FitaLavine, 80, 30f),
        };

        private static Dictionary<string, WorldSecretDefinition> _byId;

        public static IReadOnlyList<WorldSecretDefinition> All => AllSecrets;

        public static WorldSecretDefinition Get(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            if (_byId == null)
            {
                var table = new Dictionary<string, WorldSecretDefinition>();
                foreach (WorldSecretDefinition secret in AllSecrets)
                {
                    table[secret.Id] = secret;
                }

                _byId = table;
            }

            return _byId.TryGetValue(id, out WorldSecretDefinition found) ? found : null;
        }

        /// <summary>The secrets of <paramref name="region"/>, in catalog order.</summary>
        public static List<WorldSecretDefinition> InRegion(BiomeType region)
        {
            var results = new List<WorldSecretDefinition>(2);
            foreach (WorldSecretDefinition secret in AllSecrets)
            {
                if (secret.Region == region)
                {
                    results.Add(secret);
                }
            }

            return results;
        }

        /// <summary>The map secret that hides <paramref name="relicId"/>, or null (arena relics).</summary>
        public static WorldSecretDefinition ForRelic(string relicId)
        {
            foreach (WorldSecretDefinition secret in AllSecrets)
            {
                if (secret.RelicId == relicId)
                {
                    return secret;
                }
            }

            return null;
        }
    }
}
