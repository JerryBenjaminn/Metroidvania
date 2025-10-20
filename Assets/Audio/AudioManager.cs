using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    [Range(0f, 1f)] public float globalSfxVolume = 0.5f;

    [Header("Mixer")]
    public AudioMixer mixer;
    public AudioMixerGroup musicGroup;
    public AudioMixerGroup sfxGroup;
    public AudioMixerGroup uiGroup;

    [Header("Pool")]
    public int sfxPoolSize = 16;
    public bool dontDestroyOnLoad = true;

    [Header("Music")]
    public float defaultMusicFade = 0.6f;

    AudioSource[] sfxPool;
    int sfxIndex;

    AudioSource musicA, musicB;
    AudioSource uiSource;

    readonly Dictionary<SoundEvent, float> lastPlay = new();
    readonly Dictionary<SoundEvent, int> liveCount = new();

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);

        // SFX pool
        sfxPool = new AudioSource[sfxPoolSize];
        for (int i = 0; i < sfxPoolSize; i++)
        {
            var go = new GameObject("SFX_" + i);
            go.transform.SetParent(transform);
            var a = go.AddComponent<AudioSource>();
            a.playOnAwake = false;
            a.outputAudioMixerGroup = sfxGroup;
            a.rolloffMode = AudioRolloffMode.Linear;
            sfxPool[i] = a;
        }

        // UI source
        var uiGo = new GameObject("UI_Audio");
        uiGo.transform.SetParent(transform);
        uiSource = uiGo.AddComponent<AudioSource>();
        uiSource.playOnAwake = false;
        uiSource.outputAudioMixerGroup = uiGroup;
        uiSource.spatialBlend = 0f;

        // Music A/B
        musicA = MakeMusicSource("Music_A");
        musicB = MakeMusicSource("Music_B");
    }

    AudioSource MakeMusicSource(string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform);
        var a = go.AddComponent<AudioSource>();
        a.playOnAwake = false;
        a.loop = true;
        a.spatialBlend = 0f;
        a.outputAudioMixerGroup = musicGroup;
        return a;
    }

    // ---------- PUBLIC API ----------

    public void PlayUI(AudioClip clip, float vol = 1f, float pitch = 1f)
    {
        if (!clip) return;
        uiSource.pitch = pitch;
        uiSource.PlayOneShot(clip, vol);
    }

    public void Play(SoundEvent ev) => Play(ev, null);
    public void Play(SoundEvent ev, Vector3 position) => Play(ev, (Vector3?)position);

    public void Play(SoundEvent ev, Vector3? position, bool is2D = false)
    {
        if (!ev) return;

        // cooldown
        if (ev.cooldown > 0f && lastPlay.TryGetValue(ev, out var t) && Time.unscaledTime - t < ev.cooldown)
            return;

        // simultaneous limit
        if (liveCount.TryGetValue(ev, out var c) && c >= ev.maxSimultaneous)
            return;

        var clip = ev.PickClip();
        if (!clip) return;

        var src = NextSource();
        var group = ev.outputOverride ? ev.outputOverride :
                    (position.HasValue ? sfxGroup : uiGroup);

        src.outputAudioMixerGroup = group;
        src.transform.position = position ?? src.transform.position;
        src.spatialBlend = position.HasValue ? ev.spatialBlend : 0f;
        src.minDistance = ev.minDistance;
        src.maxDistance = ev.maxDistance;
        src.pitch = Mathf.Clamp(ev.pitch + Random.Range(-ev.pitchVariance, ev.pitchVariance), -3f, 3f);
        src.volume = ev.volume * globalSfxVolume;
        src.loop = false;
        src.clip = clip;

        if (is2D)
        {
            src.spatialBlend = 0f;
            src.transform.position = Vector3.zero;
        }
        else
        {
            src.spatialBlend = 1f;
            src.transform.position = transform.position;
        }

        StartCoroutine(CoPlayCount(ev, src));

        src.Play();
        lastPlay[ev] = Time.unscaledTime;
    }

    public void PlayMusic(AudioClip clip, float fade = -1f, bool loop = true, float targetVol = 1f)
    {
        if (!clip) return;
        if (fade < 0f) fade = defaultMusicFade;

        var from = musicA.isPlaying ? musicA : (musicB.isPlaying ? musicB : musicA);
        var to = (from == musicA) ? musicB : musicA;

        to.clip = clip;
        to.loop = loop;
        to.volume = 0f;
        to.Play();

        StopAllCoroutines();
        StartCoroutine(CoCrossfade(from, to, fade, targetVol));
    }

    public void StopMusic(float fade = 0.25f)
    {
        StopAllCoroutines();
        StartCoroutine(CoCrossfade(musicA, musicB, fade, 0f, stopAtEnd: true));
    }

    // ---------- Internals ----------

    AudioSource NextSource()
    {
        var a = sfxPool[sfxIndex];
        sfxIndex = (sfxIndex + 1) % sfxPool.Length;
        return a;
    }

    System.Collections.IEnumerator CoPlayCount(SoundEvent ev, AudioSource src)
    {
        liveCount[ev] = liveCount.TryGetValue(ev, out var v) ? v + 1 : 1;
        // odota että soitto päättyy
        while (src.isPlaying) yield return null;
        liveCount[ev] = Mathf.Max(0, liveCount[ev] - 1);
    }

    System.Collections.IEnumerator CoCrossfade(AudioSource from, AudioSource to, float time, float toVol, bool stopAtEnd = false)
    {
        float t = 0f;
        float fromStart = from ? from.volume : 0f;
        float toStart = to.volume;
        while (t < time)
        {
            t += Time.unscaledDeltaTime;
            float k = time <= 0f ? 1f : t / time;
            if (from) from.volume = Mathf.Lerp(fromStart, 0f, k);
            if (to) to.volume = Mathf.Lerp(toStart, toVol, k);
            yield return null;
        }
        if (from) { from.volume = 0f; if (stopAtEnd) from.Stop(); }
        if (to) to.volume = toVol;
    }
    void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    [System.Serializable]
    public struct SceneMusic
    {
        public string sceneName;
        public AudioClip clip;
        public float fade;
        public bool loop;
    }
    public SceneMusic[] sceneMusic; // aseta Inspectorissa

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        for (int i = 0; i < sceneMusic.Length; i++)
        {
            if (sceneMusic[i].sceneName == scene.name && sceneMusic[i].clip)
            {
                PlayMusic(sceneMusic[i].clip, sceneMusic[i].fade, sceneMusic[i].loop);
                return;
            }
        }
        StopMusic(0.3f);
    }


}
