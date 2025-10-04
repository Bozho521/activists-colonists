using System.Collections;
using System.Collections.Generic;
using Data;
using UnityEngine;

namespace GameControllers
{
    [DefaultExecutionOrder(-50)]
    public class SFXManager : MonoBehaviour
    {
        public static SFXManager Instance { get; private set; }

        [Header("Catalog + Music")]
        [SerializeField] private AudioCatalog catalog;
        [SerializeField] private AudioSource musicSource;

        [Header("Defaults")]
        [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float musicVolume = 0.8f;
        [SerializeField] private float sfxSpatialBlend3D = 0.95f;

        readonly Dictionary<string, AudioClip> _clipCache = new();
        readonly Dictionary<string, AudioClip[]> _groupCache = new();

        void Awake()
        {
            if (Instance && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (catalog) catalog.Build();

            if (catalog)
            {
                foreach (var kv in catalog.GetClipPairs())   _clipCache[kv.Key] = kv.Value;
                foreach (var kv in catalog.GetGroupPairs())  _groupCache[kv.Key] = kv.Value;
            }

            if (!musicSource)
            {
                var go = new GameObject("MusicSource");
                go.transform.SetParent(transform, false);
                musicSource = go.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }
            musicSource.volume = musicVolume * masterVolume;

            if (catalog && catalog.defaultMusic)
                PlayMusicClip(catalog.defaultMusic, loop: true, fade: 0f);
        }

        public static void SetMasterVolume(float v)
        {
            if (!Instance) return;
            Instance.masterVolume = Mathf.Clamp01(v);
            if (Instance.musicSource) Instance.musicSource.volume = Instance.musicVolume * Instance.masterVolume;
        }

        public static void SetMusicVolume(float v)
        {
            if (!Instance) return;
            Instance.musicVolume = Mathf.Clamp01(v);
            if (Instance.musicSource) Instance.musicSource.volume = Instance.musicVolume * Instance.masterVolume;
        }

        public static void SetSfxVolume(float v)
        {
            if (!Instance) return;
            Instance.sfxVolume = Mathf.Clamp01(v);
        }

        public static void PlayMusic(string key, bool loop = true, float fade = 0.25f)
        {
            if (!Instance) return;
            if (!Instance._clipCache.TryGetValue(key, out var clip) || !clip) return;
            PlayMusicClip(clip, loop, fade);
        }

        public static void PlayMusicClip(AudioClip clip, bool loop = true, float fade = 0.25f)
        {
            if (!Instance || !clip) return;
            Instance.InternalPlayMusicClip(clip, loop, fade);
        }

        void InternalPlayMusicClip(AudioClip clip, bool loop, float fade)
        {
            if (!musicSource) return;

            if (fade <= 0f)
            {
                musicSource.clip = clip;
                musicSource.loop = loop;
                musicSource.volume = musicVolume * masterVolume;
                musicSource.Play();
                return;
            }

            StartCoroutine(FadeMusicThenSwap(clip, loop, fade));
        }

        IEnumerator FadeMusicThenSwap(AudioClip next, bool loop, float fade)
        {
            float half = fade * 0.5f;
            float t = 0f;
            float startVol = musicSource.volume;
            while (t < half)
            {
                t += Time.unscaledDeltaTime;
                musicSource.volume = Mathf.Lerp(startVol, 0f, t / half);
                yield return null;
            }

            musicSource.clip = next;
            musicSource.loop = loop;
            musicSource.Play();

            t = 0f;
            while (t < half)
            {
                t += Time.unscaledDeltaTime;
                musicSource.volume = Mathf.Lerp(0f, musicVolume * masterVolume, t / half);
                yield return null;
            }
            musicSource.volume = musicVolume * masterVolume;
        }

        public static void StopMusic(float fade = 0.25f)
        {
            if (!Instance || !Instance.musicSource) return;
            if (fade <= 0f) { Instance.musicSource.Stop(); return; }
            Instance.StartCoroutine(Instance.FadeOutAndStop(fade));
        }

        IEnumerator FadeOutAndStop(float fade)
        {
            float t = 0f;
            float startVol = musicSource.volume;
            while (t < fade)
            {
                t += Time.unscaledDeltaTime;
                musicSource.volume = Mathf.Lerp(startVol, 0f, t / fade);
                yield return null;
            }
            musicSource.Stop();
            musicSource.volume = musicVolume * masterVolume;
        }

        public static void PlaySFX(string key, float volume = 1f, Vector3? worldPos = null, float pitchVariance = 0f)
        {
            if (!Instance) return;
            if (!Instance._clipCache.TryGetValue(key, out var clip) || !clip) return;
            Instance.SpawnOneShot(clip, volume, worldPos, pitchVariance);
        }

        public static void PlaySFXClip(AudioClip clip, float volume = 1f, Vector3? worldPos = null, float pitchVariance = 0f)
        {
            if (!Instance || !clip) return;
            Instance.SpawnOneShot(clip, volume, worldPos, pitchVariance);
        }

        public static void PlayRandomSFX(string groupKey, float volume = 1f, Vector3? worldPos = null, float pitchVariance = 0.05f)
        {
            if (!Instance) return;
            if (!Instance._groupCache.TryGetValue(groupKey, out var arr) || arr == null || arr.Length == 0) return;
            var clip = arr[Random.Range(0, arr.Length)];
            if (!clip) return;
            Instance.SpawnOneShot(clip, volume, worldPos, pitchVariance);
        }

        void SpawnOneShot(AudioClip clip, float volume, Vector3? worldPos, float pitchVariance)
        {
            var go = new GameObject($"SFX:{clip.name}");
            go.transform.SetParent(transform, false);

            if (worldPos.HasValue)
                go.transform.position = worldPos.Value;

            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = false;
            src.clip = clip;

            float pitch = 1f;
            if (pitchVariance > 0f)
            {
                float v = Mathf.Clamp(pitchVariance, 0f, 0.99f);
                pitch = Random.Range(1f - v, 1f + v);
            }
            src.pitch = pitch;

            bool is3D = worldPos.HasValue;
            src.spatialBlend = is3D ? sfxSpatialBlend3D : 0f;
            src.volume = volume * sfxVolume * masterVolume;

            src.Play();
            Destroy(go, clip.length / Mathf.Max(0.01f, Mathf.Abs(pitch)));
        }

        public static bool TryGetClip(string key, out AudioClip clip)
        {
            clip = null;
            if (!Instance) return false;
            return Instance._clipCache.TryGetValue(key, out clip) && clip;
        }

        public static bool TryGetGroup(string key, out AudioClip[] clips)
        {
            clips = null;
            if (!Instance) return false;
            return Instance._groupCache.TryGetValue(key, out clips) && clips != null && clips.Length > 0;
        }
    }
}
