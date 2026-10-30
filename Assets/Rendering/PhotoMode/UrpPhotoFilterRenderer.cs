using Rapadura.Gameplay.PhotoMode;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rapadura.Rendering.PhotoMode
{
    /// <summary>
    /// URP side of the photo-mode filters (<see cref="PhotoFilterPresets"/>). Lives in its own assembly
    /// (Rapadura.PhotoMode.Urp, outside Assets/Scripts) because Rapadura.Runtime has no URP reference;
    /// it registers itself into <see cref="PhotoFilterRendering.Renderer"/> at startup.
    ///
    /// <see cref="Apply"/> builds, once, a runtime global <see cref="Volume"/> (priority 1000, above the
    /// scene's GlobalPostProcessVolume) with a code-created <see cref="VolumeProfile"/> holding the URP
    /// 17 overrides ColorAdjustments, WhiteBalance, Vignette, FilmGrain and Bloom, and turns on the
    /// camera's <c>renderPostProcessing</c> (gameplay cameras keep it off; the old value comes back in
    /// <see cref="Clear"/>). Effects a look doesn't use are deactivated, so they cost nothing. Needs the
    /// active renderer to carry PostProcessData (Mobile_Renderer.asset references URP's default one) —
    /// <see cref="IsSupported"/> is false otherwise and the photo mode hides the filter choice.
    /// </summary>
    public sealed class UrpPhotoFilterRenderer : IPhotoFilterRenderer
    {
        public const float VolumePriority = 1000f;

        private GameObject _host;
        private VolumeProfile _profile;
        private ColorAdjustments _color;
        private WhiteBalance _whiteBalance;
        private Vignette _vignette;
        private FilmGrain _grain;
        private Bloom _bloom;

        private UniversalAdditionalCameraData _cameraData;
        private bool _savedPostProcessing;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Register()
        {
            PhotoFilterRendering.Renderer = new UrpPhotoFilterRenderer();
        }

        public bool IsSupported
        {
            get
            {
                if (!(GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset asset))
                {
                    return false;
                }

                foreach (ScriptableRendererData data in asset.rendererDataList)
                {
                    if (data is UniversalRendererData universal && universal.postProcessData != null)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public void Apply(Camera camera, PhotoFilterSettings settings)
        {
            if (camera == null)
            {
                return;
            }

            if (!settings.Enabled)
            {
                Clear();
                return;
            }

            EnsureVolume();
            EnsureCamera(camera);

            _color.active = true;
            _color.postExposure.Override(settings.PostExposure);
            _color.contrast.Override(settings.Contrast);
            _color.saturation.Override(settings.Saturation);
            _color.colorFilter.Override(settings.ColorFilter);

            _whiteBalance.active = Mathf.Abs(settings.Temperature) > 0.01f || Mathf.Abs(settings.Tint) > 0.01f;
            _whiteBalance.temperature.Override(settings.Temperature);
            _whiteBalance.tint.Override(settings.Tint);

            _vignette.active = settings.VignetteIntensity > 0.001f;
            _vignette.intensity.Override(settings.VignetteIntensity);
            _vignette.smoothness.Override(Mathf.Clamp(settings.VignetteSmoothness, 0.01f, 1f));
            _vignette.color.Override(settings.VignetteColor);

            _grain.active = settings.GrainIntensity > 0.001f;
            _grain.type.Override(FilmGrainLookup.Medium3);
            _grain.intensity.Override(settings.GrainIntensity);
            _grain.response.Override(0.8f);

            _bloom.active = settings.BloomIntensity > 0.001f;
            _bloom.intensity.Override(settings.BloomIntensity);
            _bloom.threshold.Override(settings.BloomThreshold);
            _bloom.tint.Override(settings.BloomTint);
        }

        public void Clear()
        {
            if (_cameraData != null)
            {
                _cameraData.renderPostProcessing = _savedPostProcessing;
                _cameraData = null;
            }

            if (_host != null)
            {
                Object.Destroy(_host);
            }

            if (_profile != null)
            {
                Object.Destroy(_profile);
            }

            _host = null;
            _profile = null;
            _color = null;
            _whiteBalance = null;
            _vignette = null;
            _grain = null;
            _bloom = null;
        }

        private void EnsureCamera(Camera camera)
        {
            UniversalAdditionalCameraData data = camera.GetUniversalAdditionalCameraData();
            if (data == _cameraData)
            {
                return;
            }

            if (_cameraData != null)
            {
                _cameraData.renderPostProcessing = _savedPostProcessing;
            }

            _cameraData = data;
            _savedPostProcessing = data.renderPostProcessing;
            data.renderPostProcessing = true;
        }

        private void EnsureVolume()
        {
            if (_host != null && _profile != null)
            {
                return;
            }

            Clear();
            _host = new GameObject("PhotoModeVolume");
            Object.DontDestroyOnLoad(_host);
            _profile = ScriptableObject.CreateInstance<VolumeProfile>();
            _profile.name = "PhotoModeProfile (runtime)";
            _color = _profile.Add<ColorAdjustments>(false);
            _whiteBalance = _profile.Add<WhiteBalance>(false);
            _vignette = _profile.Add<Vignette>(false);
            _grain = _profile.Add<FilmGrain>(false);
            _bloom = _profile.Add<Bloom>(false);

            Volume volume = _host.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = VolumePriority;
            volume.weight = 1f;
            volume.sharedProfile = _profile;
        }
    }
}
