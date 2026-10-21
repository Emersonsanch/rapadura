using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.Environment
{
    /// <summary>
    /// Tiny hidden, persistent MonoBehaviour that forwards <c>Update</c> to plain C# managers
    /// (<see cref="TimeOfDayManager"/>, <see cref="WeatherSystem"/>) — IManager has no per-frame
    /// hook and GameManager has no Update. Created lazily the first time something registers in
    /// Play Mode; EditMode tests never create it and drive <c>Tick</c> directly instead.
    /// Iterates by index over a pre-sized list, so the per-frame path does not allocate.
    /// </summary>
    public class EnvironmentTickRunner : MonoBehaviour
    {
        private static EnvironmentTickRunner _instance;
        private static readonly List<Action<float>> Tickers = new List<Action<float>>(4);

        /// <summary>Adds a per-frame callback (receives scaled delta time). No-op outside Play Mode.</summary>
        public static void Register(Action<float> tick)
        {
            if (tick == null || !Application.isPlaying)
            {
                return;
            }

            if (!Tickers.Contains(tick))
            {
                Tickers.Add(tick);
            }

            if (_instance == null)
            {
                var go = new GameObject("[EnvironmentTick]") { hideFlags = HideFlags.HideInHierarchy };
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<EnvironmentTickRunner>();
            }
        }

        public static void Unregister(Action<float> tick)
        {
            if (tick != null)
            {
                Tickers.Remove(tick);
            }
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            for (int i = Tickers.Count - 1; i >= 0; i--)
            {
                if (i < Tickers.Count)
                {
                    Tickers[i](deltaTime);
                }
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
