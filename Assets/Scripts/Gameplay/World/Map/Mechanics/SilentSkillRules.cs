using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>How loud a skill is over the hermitages (<see cref="SilentSkillRules.NoiseOf"/>).</summary>
    public enum SkillNoise
    {
        /// <summary>An ordinary skill: its cast is heard like a blow.</summary>
        Plain = 0,

        /// <summary>A silent skill: its cast calms the cliff and hushes the blows right after it.</summary>
        Quiet = 1,

        /// <summary>An explosion, a quake, a shout, a rain of arrows: the whole cliff hears it.</summary>
        Loud = 2,
    }

    /// <summary>
    /// The silent skills of the Ermidas Suspensas (Docs/Mapas100.md 56: "andar devagar e usar skills silenciosas (Lavine) ajuda"; "Falta: skills
    /// silenciosas além da Lavine"), pure and server-runnable, on top of <see cref="FarnkeshSilenceRules"/>. Every cousin has quiet skills
    /// (a precise arrow, a field that hums low, a shield of roots, a blessing, a mist) and loud ones (grenades, quakes, explosions, the rain of
    /// arrows, the hero's shout). Casting in the silence meter's area:
    /// <code>
    /// quiet cast: the meter −8 at once, and the blows given or taken in the next 3 s make no noise
    /// loud cast: +12 (× Lavine's and the night's quiet, like every gain) · any other skill: +3 (a blow)
    /// Maithe: Flecha Precisa, Passo do Vento · Ícaro: Campo Magnético · Joaquim: Escudo das Raízes, Última Resistência
    /// Maria: Bênção da Vida, Cura Divina, Purificação, Escudo Sagrado · Lavine: Névoa da Ilusão, Correntes das Trevas, Orbe Sombria
    /// </code>
    /// Decision (2026-11-23): each cousin gets at least one quiet skill so every class can cross the cliff fighting; Maria (support) has the
    /// most, Ícaro (machines) the fewest; Lavine keeps her quiet steps (×0.6) and gets three quiet skills. The cooldowns keep the −8 from being
    /// a way to empty the meter for free.
    /// </summary>
    public static class SilentSkillRules
    {
        public const float QuietCalm = -8f;
        public const float LoudNoise = 12f;
        public const float PlainNoise = 3f;
        public const float MuffleSeconds = 3f;

        private static readonly string[] Quiet =
        {
            "skill_maithe_flecha_precisa", "skill_maithe_passo_do_vento",
            "skill_icaro_campo_magnetico",
            "skill_joaquim_escudo_das_raizes", "skill_joaquim_ultima_resistencia",
            "skill_maria_bencao_da_vida", "skill_maria_cura_divina", "skill_maria_purificacao", "skill_maria_escudo_sagrado",
            "skill_lavine_nevoa_da_ilusao", "skill_lavine_correntes_das_trevas", "skill_lavine_orbe_sombria",
        };

        private static readonly string[] Loud =
        {
            "skill_maithe_chuva_de_flechas", "skill_maithe_disparo_explosivo", "skill_maithe_tempestade_de_mil_flechas",
            "skill_icaro_granada_eletrica", "skill_icaro_exercito_mecanico", "skill_icaro_torreta_automatica",
            "skill_joaquim_terremoto", "skill_joaquim_muralha_de_pedra", "skill_joaquim_provocacao_heroica",
            "skill_maria_milagre_celestial",
            "skill_lavine_explosao_arcana", "skill_lavine_eclipse_eterno",
        };

        public static SkillNoise NoiseOf(string skillId)
        {
            if (string.IsNullOrEmpty(skillId))
            {
                return SkillNoise.Plain;
            }

            if (System.Array.IndexOf(Quiet, skillId) >= 0)
            {
                return SkillNoise.Quiet;
            }

            return System.Array.IndexOf(Loud, skillId) >= 0 ? SkillNoise.Loud : SkillNoise.Plain;
        }

        /// <summary>What a cast of <paramref name="noise"/> adds to the meter (<paramref name="quiet"/> is Lavine's and the night's factor, on gains only).</summary>
        public static float CastNoise(SkillNoise noise, float quiet)
        {
            switch (noise)
            {
                case SkillNoise.Quiet: return QuietCalm;
                case SkillNoise.Loud: return LoudNoise * quiet;
                default: return PlainNoise * quiet;
            }
        }

        /// <summary>The blows that count <paramref name="sinceQuietCast"/> seconds after the last quiet cast: none within <see cref="MuffleSeconds"/>.</summary>
        public static int HitsHeard(int hits, float sinceQuietCast) => sinceQuietCast >= 0f && sinceQuietCast < MuffleSeconds ? 0 : Mathf.Max(0, hits);

        /// <summary>The meter after a cast, kept in 0–100.</summary>
        public static float AfterCast(float meter, SkillNoise noise, float quiet) =>
            Mathf.Clamp(meter + CastNoise(noise, quiet), 0f, FarnkeshSilenceRules.Max);
    }
}
