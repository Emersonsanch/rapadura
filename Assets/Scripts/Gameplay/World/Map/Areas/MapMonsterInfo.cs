using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Areas
{
    /// <summary>
    /// Tag on a monster spawned by a <see cref="MapMonsterSpawner"/>: its level inside the zone's band (Docs/Progressao500.md
    /// §4, the MU-style "nível do bicho") and where it came from. Read by the spawner (passive night watchers) and
    /// available to nameplates and to <see cref="MapAccessRules.GetMonsterLevelDeltaPenalty"/> once combat applies it.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MapMonsterInfo : MonoBehaviour
    {
        public int Level { get; private set; } = 1;
        public string MapId { get; private set; }
        public string ZoneId { get; private set; }
        public string SpeciesId { get; private set; }

        public void Configure(int level, string mapId, string zoneId, string speciesId)
        {
            Level = Mathf.Max(1, level);
            MapId = mapId;
            ZoneId = zoneId;
            SpeciesId = speciesId;
        }
    }
}
