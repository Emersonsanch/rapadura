using System.Collections.Generic;
using Rapadura.Core.Events;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities
{
    /// <summary>Raised when the walkable city of <see cref="CityId"/> starts loading (SceneTransitionService.BeginCityVisit).</summary>
    public readonly struct CityVisitStartedEvent : IGameEvent
    {
        public readonly string CityId;

        public CityVisitStartedEvent(string cityId)
        {
            CityId = cityId;
        }
    }

    /// <summary>Raised when the hero leaves the walkable city (a gate, or the Cais da Barca: <see cref="ToFastTravel"/>).</summary>
    public readonly struct CityVisitEndedEvent : IGameEvent
    {
        public readonly string CityId;
        public readonly bool ToFastTravel;

        public CityVisitEndedEvent(string cityId, bool toFastTravel)
        {
            CityId = cityId;
            ToFastTravel = toFastTravel;
        }
    }

    /// <summary>An actor crossed a city's border (Docs/Cities.md §3.1): from <see cref="From"/> to <see cref="To"/>.</summary>
    public readonly struct CityZoneChangedEvent : IGameEvent
    {
        public readonly GameObject Actor;
        public readonly string CityId;
        public readonly ZoneKind From;
        public readonly ZoneKind To;

        public CityZoneChangedEvent(GameObject actor, string cityId, ZoneKind from, ZoneKind to)
        {
            Actor = actor;
            CityId = cityId;
            From = from;
            To = to;
        }
    }

    /// <summary>
    /// Which ground is a city (Docs/Cities.md §3.1): every <see cref="CitySafeZoneVolume"/> registers its box here, and
    /// combat code asks <see cref="ZoneAt"/> for the <see cref="ZoneKind"/> of a position (then the pure
    /// <see cref="SafeZoneRules"/> decide). With no walkable city loaded (arena, map) the list is empty and every answer
    /// is <see cref="ZoneKind.Wilds"/> at no cost, so combat is exactly what it was. Online, the server keeps the same
    /// boxes and validates positions itself.
    /// </summary>
    public static class CitySafeZones
    {
        private static readonly List<CitySafeZoneVolume> Volumes = new List<CitySafeZoneVolume>();

        public static int Count => Volumes.Count;

        public static void Register(CitySafeZoneVolume volume)
        {
            if (volume != null && !Volumes.Contains(volume))
            {
                Volumes.Add(volume);
            }
        }

        public static void Unregister(CitySafeZoneVolume volume) => Volumes.Remove(volume);

        public static ZoneKind ZoneAt(Vector3 position)
        {
            for (int i = 0; i < Volumes.Count; i++)
            {
                if (Volumes[i] != null && Volumes[i].Contains(position))
                {
                    return ZoneKind.City;
                }
            }

            return ZoneKind.Wilds;
        }

        /// <summary>The city whose safe zone holds <paramref name="position"/>, or null.</summary>
        public static string CityAt(Vector3 position)
        {
            for (int i = 0; i < Volumes.Count; i++)
            {
                if (Volumes[i] != null && Volumes[i].Contains(position))
                {
                    return Volumes[i].CityId;
                }
            }

            return null;
        }

        /// <summary>Nothing can be hurt here (<see cref="SafeZoneRules.CanTakeDamage"/> for a player or monster).</summary>
        public static bool IsSafeAt(Vector3 position)
        {
            return Volumes.Count > 0 && !SafeZoneRules.CanTakeDamage(ZoneActor.Player, ZoneAt(position));
        }
    }
}
