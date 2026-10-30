using System;
using System.Collections;
using System.IO;
using Rapadura.Gameplay.Characters;
using UnityEngine;

namespace Rapadura.Gameplay.PhotoMode
{
    /// <summary>
    /// "Salvar foto": grabs the rendered frame (after the photo UI hid itself) and writes a PNG to
    /// <c>Application.persistentDataPath/Fotos</c>, named <c>rapadura_20261029_213045_maithe.png</c>.
    /// Same folder on every platform for now — exporting to the Android/iOS gallery (MediaStore /
    /// Photos permission) is a later step. Naming rules are pure and EditMode-tested.
    /// </summary>
    public static class PhotoCaptureService
    {
        public const string FolderName = "Fotos";
        public const string FilePrefix = "rapadura_";
        public const string Extension = ".png";

        /// <summary>Upper bound of "_2", "_3"… suffixes tried when several photos share a second.</summary>
        public const int MaxNameAttempts = 99;

        public static string FolderPath => Path.Combine(Application.persistentDataPath, FolderName);

        /// <summary><c>rapadura_yyyyMMdd_HHmmss[_hero].png</c> (invariant digits, lower-case hero id).</summary>
        public static string BuildFileName(DateTime time, CharacterId? hero)
        {
            string stamp = time.ToString("yyyyMMdd_HHmmss", System.Globalization.CultureInfo.InvariantCulture);
            string suffix = hero.HasValue ? "_" + hero.Value.ToString().ToLowerInvariant() : string.Empty;
            return FilePrefix + stamp + suffix + Extension;
        }

        /// <summary>
        /// <paramref name="fileName"/> inside <paramref name="folder"/>, or the first free "_2", "_3"…
        /// variant when <paramref name="exists"/> says it is taken.
        /// </summary>
        public static string BuildUniquePath(string folder, string fileName, Func<string, bool> exists)
        {
            string path = Path.Combine(folder, fileName);
            if (exists == null || !exists(path))
            {
                return path;
            }

            string stem = Path.GetFileNameWithoutExtension(fileName);
            string extension = Path.GetExtension(fileName);
            for (int i = 2; i <= MaxNameAttempts; i++)
            {
                string candidate = Path.Combine(folder, stem + "_" + i + extension);
                if (!exists(candidate))
                {
                    return candidate;
                }
            }

            return Path.Combine(folder, stem + "_" + Guid.NewGuid().ToString("N").Substring(0, 6) + extension);
        }

        /// <summary>
        /// Coroutine: waits for the end of the frame (the caller hid its UI the frame before), reads the
        /// screen and writes the PNG. <paramref name="onDone"/> gets the full path (null on failure).
        /// Runs in real time — the game is paused while it runs.
        /// </summary>
        public static IEnumerator CaptureRoutine(CharacterId? hero, Action<string> onDone)
        {
            yield return new WaitForEndOfFrame();

            string path = null;
            Texture2D texture = null;
            try
            {
                Directory.CreateDirectory(FolderPath);
                path = BuildUniquePath(FolderPath, BuildFileName(DateTime.Now, hero), File.Exists);
                texture = ScreenCapture.CaptureScreenshotAsTexture();
                File.WriteAllBytes(path, texture.EncodeToPNG());
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[PhotoMode] Could not save the photo: {exception.Message}");
                path = null;
            }
            finally
            {
                if (texture != null)
                {
                    UnityEngine.Object.Destroy(texture);
                }
            }

            onDone?.Invoke(path);
        }
    }
}
