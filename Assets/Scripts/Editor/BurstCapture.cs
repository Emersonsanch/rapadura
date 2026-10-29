using System.IO;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Editor
{
    /// <summary>
    /// Menu <c>Rapadura/Debug/Capture Burst (F9)</c>: while in Play Mode, saves 30 frames of the
    /// Game view at ~10 fps to <c>Assets/Screenshots/burst/</c> (overwriting the previous burst), so
    /// an animation can be reviewed frame by frame outside the Editor. Uses ScreenCapture, so what is
    /// saved is exactly what the Game view renders.
    /// </summary>
    public static class BurstCapture
    {
        private const string Folder = "Assets/Screenshots/burst";
        private const int FrameCount = 30;
        private const float Interval = 0.1f;

        private static int _remaining;
        private static double _nextTime;
        private static int _index;

        [MenuItem("Rapadura/Debug/Capture Burst %#F9")]
        public static void Start()
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.LogWarning("[BurstCapture] Only in Play Mode.");
                return;
            }

            Directory.CreateDirectory(Folder);
            foreach (string old in Directory.GetFiles(Folder, "frame_*.png"))
            {
                File.Delete(old);
            }

            _remaining = FrameCount;
            _index = 0;
            _nextTime = EditorApplication.timeSinceStartup;
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
            Debug.Log($"[BurstCapture] Capturing {FrameCount} frames to {Folder}...");
        }

        private static void Tick()
        {
            if (_remaining <= 0 || !EditorApplication.isPlaying)
            {
                EditorApplication.update -= Tick;
                Debug.Log("[BurstCapture] Done.");
                return;
            }

            if (EditorApplication.timeSinceStartup < _nextTime)
            {
                return;
            }

            _nextTime += Interval;
            ScreenCapture.CaptureScreenshot($"{Folder}/frame_{_index:00}.png");
            _index++;
            _remaining--;
        }
    }
}
