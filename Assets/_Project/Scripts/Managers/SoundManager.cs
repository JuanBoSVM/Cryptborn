using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : Manager<SoundManager>
{
    private AudioSource m_SFXAudioSource;
    private AudioSource m_MusicAudioSource;
    private AudioSource m_SecondaryMusicAudioSource;
    private Coroutine m_MusicFadeCoroutine;

    [Header("Background Music")]
    [SerializeField, Tooltip("Track IDs are the clips' indices in this list.")]
    private List<AudioClip> m_BackgroundMusic = new List<AudioClip>();

    [SerializeField, Min(0f)]
    private float m_MusicCrossfadeDuration = 1f;

    [Header("UI Sounds")]
    [SerializeField]
    private List<AudioClip> m_ButtonClickSFX = new List<AudioClip>();

    [SerializeField]
    private List<AudioClip> m_ButtonSelectSFX = new List<AudioClip>();

    private const string MasterVolumeKey = "Cryptborn.Audio.MasterVolume";
    private const string MusicVolumeKey = "Cryptborn.Audio.MusicVolume";
    private const string SFXVolumeKey = "Cryptborn.Audio.SFXVolume";

    private int m_CurrentMusicId = -1;
    private float m_MasterVolume = 1f;
    private float m_SFXVolume = 1f;
    private float m_MusicVolume = 1f;

    public static float MasterVolume => Instance.m_MasterVolume;
    public static float MusicVolume => Instance.m_MusicVolume;
    public static float SFXVolume => Instance.m_SFXVolume;

    private static float RandomPitch => Random.Range(0.8f, 1.2f);

    private struct SoundRequest
    {
        public AudioClip source;
        public Vector3 position;
        public bool randomizePitch;

        public SoundRequest(AudioClip source, Vector3 position, bool randomizePitch = true)
        {
            this.source = source;
            this.position = position;
            this.randomizePitch = randomizePitch;
        }
    }

    private readonly Queue<SoundRequest> m_SoundQueue = new Queue<SoundRequest>();

    public static void QueueSound(AudioClip source, Vector3 position)
    {
        if (source == null)
        {
            Debug.LogWarning("Cannot queue a sound because its AudioClip source is null.");
            return;
        }

        Instance.m_SoundQueue.Enqueue(new SoundRequest(source, position));
    }

    public static void SetMasterVolume(float volume)
    {
        SoundManager manager = Instance;
        manager.m_MasterVolume = Mathf.Clamp01(volume);
        AudioListener.volume = manager.m_MasterVolume;
        PlayerPrefs.SetFloat(MasterVolumeKey, manager.m_MasterVolume);
        PlayerPrefs.Save();
    }

    public static void SetMusicVolume(float volume)
    {
        SoundManager manager = Instance;
        manager.m_MusicVolume = Mathf.Clamp01(volume);
        if (manager.m_MusicFadeCoroutine == null)
        {
            if (manager.m_MusicAudioSource != null) { manager.m_MusicAudioSource.volume = manager.m_MusicVolume; }
            if (manager.m_SecondaryMusicAudioSource != null) { manager.m_SecondaryMusicAudioSource.volume = manager.m_MusicVolume; }
        }

        PlayerPrefs.SetFloat(MusicVolumeKey, manager.m_MusicVolume);
        PlayerPrefs.Save();
    }

    public static void SetSFXVolume(float volume)
    {
        SoundManager manager = Instance;
        manager.m_SFXVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(SFXVolumeKey, manager.m_SFXVolume);
        PlayerPrefs.Save();
    }

    public static AudioClip GetRandomUISound()
    {
        return GetRandomClip(Instance.m_ButtonClickSFX, Instance.m_ButtonSelectSFX);
    }

    public static AudioClip GetRandomButtonClickSound()
    {
        return GetRandomClip(Instance.m_ButtonClickSFX, null);
    }

    public static AudioClip GetRandomButtonSelectSound()
    {
        return GetRandomClip(Instance.m_ButtonSelectSFX, null);
    }

    public static void StartMusic(int musicId)
    {
        if (Instance.m_BackgroundMusic == null || musicId < 0 || musicId >= Instance.m_BackgroundMusic.Count)
        {
            Debug.LogWarning($"Cannot start music: track ID {musicId} is out of range.");
            return;
        }

        AudioClip music = Instance.m_BackgroundMusic[musicId];
        if (music == null)
        {
            Debug.LogWarning($"Cannot start music: track ID {musicId} has no AudioClip assigned.");
            return;
        }

        if (Instance.m_MusicAudioSource == null || Instance.m_SecondaryMusicAudioSource == null)
        {
            Debug.LogError("Cannot start music because the SoundManager's Music AudioSources are not initialized.");
            return;
        }

        if (Instance.m_CurrentMusicId == musicId &&
            ((Instance.m_MusicAudioSource.isPlaying && Instance.m_MusicAudioSource.clip == music) ||
             (Instance.m_SecondaryMusicAudioSource.isPlaying && Instance.m_SecondaryMusicAudioSource.clip == music)))
        {
            return;
        }

        if (Instance.m_MusicFadeCoroutine != null)
        {
            Instance.StopCoroutine(Instance.m_MusicFadeCoroutine);
            Instance.m_MusicFadeCoroutine = null;

            if (Instance.m_SecondaryMusicAudioSource.isPlaying &&
                (!Instance.m_MusicAudioSource.isPlaying || Instance.m_SecondaryMusicAudioSource.volume > Instance.m_MusicAudioSource.volume))
            {
                AudioSource previousSource = Instance.m_MusicAudioSource;
                Instance.m_MusicAudioSource = Instance.m_SecondaryMusicAudioSource;
                Instance.m_SecondaryMusicAudioSource = previousSource;
            }

            Instance.m_SecondaryMusicAudioSource.Stop();
            Instance.m_SecondaryMusicAudioSource.clip = null;
            Instance.m_SecondaryMusicAudioSource.loop = false;
            Instance.m_MusicAudioSource.volume = Instance.m_MusicVolume;
            Instance.m_SecondaryMusicAudioSource.volume = Instance.m_MusicVolume;
        }

        Instance.m_CurrentMusicId = musicId;
        Instance.m_MusicFadeCoroutine = Instance.StartCoroutine(
            Instance.CrossfadeMusic(Instance.m_MusicAudioSource, Instance.m_SecondaryMusicAudioSource, music));
    }

    public static void StopMusic(int musicId)
    {
        if (Instance.m_BackgroundMusic == null || musicId < 0 || musicId >= Instance.m_BackgroundMusic.Count)
        {
            Debug.LogWarning($"Cannot stop music: track ID {musicId} is out of range.");
            return;
        }

        if (Instance.m_CurrentMusicId != musicId) { return; }

        if (Instance.m_MusicFadeCoroutine != null)
        {
            Instance.StopCoroutine(Instance.m_MusicFadeCoroutine);
            Instance.m_MusicFadeCoroutine = null;
        }

        Instance.StopMusicSource(Instance.m_MusicAudioSource);
        Instance.StopMusicSource(Instance.m_SecondaryMusicAudioSource);
        Instance.m_CurrentMusicId = -1;
    }

    private IEnumerator CrossfadeMusic(AudioSource outgoingSource, AudioSource incomingSource, AudioClip music)
    {
        incomingSource.clip = music;
        incomingSource.loop = true;
        incomingSource.pitch = 1f;
        incomingSource.volume = 0f;
        incomingSource.Play();

        float duration = m_MusicCrossfadeDuration;
        if (duration <= 0f)
        {
            outgoingSource.Stop();
            outgoingSource.clip = null;
            outgoingSource.loop = false;
            outgoingSource.volume = m_MusicVolume;
            incomingSource.volume = m_MusicVolume;
        }
        else
        {
            float elapsed = 0f;
            float outgoingStartVolume = outgoingSource.isPlaying ? outgoingSource.volume : 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float fadeProgress = Mathf.Clamp01(elapsed / duration);
                outgoingSource.volume = Mathf.Lerp(outgoingStartVolume, 0f, fadeProgress);
                incomingSource.volume = Mathf.Lerp(0f, m_MusicVolume, fadeProgress);
                yield return null;
            }

            outgoingSource.Stop();
            outgoingSource.clip = null;
            outgoingSource.loop = false;
            incomingSource.volume = m_MusicVolume;
        }

        m_MusicAudioSource = incomingSource;
        m_SecondaryMusicAudioSource = outgoingSource;
        m_MusicFadeCoroutine = null;
    }

    private void StopMusicSource(AudioSource source)
    {
        if (source == null) { return; }

        source.Stop();
        source.clip = null;
        source.loop = false;
        source.volume = m_MusicVolume;
    }

    private static AudioClip GetRandomClip(List<AudioClip> firstList, List<AudioClip> secondList)
    {
        int validClipCount = CountValidClips(firstList) + CountValidClips(secondList);
        if (validClipCount == 0)
        {
            Debug.LogWarning("No UI sounds are assigned to the SoundManager.");
            return null;
        }

        int selectedIndex = Random.Range(0, validClipCount);
        return GetClipAtValidIndex(firstList, secondList, selectedIndex);
    }

    private static int CountValidClips(List<AudioClip> clips)
    {
        if (clips == null) { return 0; }

        int count = 0;
        foreach (AudioClip clip in clips)
        {
            if (clip != null) { count++; }
        }

        return count;
    }

    private static AudioClip GetClipAtValidIndex(List<AudioClip> firstList, List<AudioClip> secondList, int selectedIndex)
    {
        if (TryGetClipAtValidIndex(firstList, ref selectedIndex, out AudioClip clip)) { return clip; }
        TryGetClipAtValidIndex(secondList, ref selectedIndex, out clip);
        return clip;
    }

    private static bool TryGetClipAtValidIndex(List<AudioClip> clips, ref int selectedIndex, out AudioClip selectedClip)
    {
        if (clips != null)
        {
            foreach (AudioClip clip in clips)
            {
                if (clip == null) { continue; }
                if (selectedIndex == 0)
                {
                    selectedClip = clip;
                    return true;
                }
                selectedIndex--;
            }
        }

        selectedClip = null;
        return false;
    }

    protected override void Awake()
    {
        base.Awake();

        m_MasterVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MasterVolumeKey, 1f));
        m_MusicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MusicVolumeKey, 1f));
        m_SFXVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(SFXVolumeKey, 1f));
        AudioListener.volume = m_MasterVolume;

        GameObject sfxObject = new GameObject("SFX");
        sfxObject.transform.SetParent(transform, false);
        m_SFXAudioSource = sfxObject.AddComponent<AudioSource>();
        m_SFXAudioSource.playOnAwake = false;

        GameObject musicObject = new GameObject("Music");
        musicObject.transform.SetParent(transform, false);
        m_MusicAudioSource = musicObject.AddComponent<AudioSource>();
        m_MusicAudioSource.playOnAwake = false;
        m_MusicAudioSource.volume = m_MusicVolume;

        m_SecondaryMusicAudioSource = musicObject.AddComponent<AudioSource>();
        m_SecondaryMusicAudioSource.playOnAwake = false;
        m_SecondaryMusicAudioSource.volume = m_MusicVolume;
    }

    private void FixedUpdate()
    {
        while (m_SoundQueue.Count > 0)
        {
            SoundRequest request = m_SoundQueue.Dequeue();

            m_SFXAudioSource.transform.position = request.position;
            if (request.randomizePitch) { m_SFXAudioSource.pitch = RandomPitch; }
            else { m_SFXAudioSource.pitch = 1f; }

            m_SFXAudioSource.PlayOneShot(request.source, m_SFXVolume);
        }
    }
}
