using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : Manager<SoundManager>
{
    private AudioSource m_AudioSource;

    [Header("Background Music")]
    [SerializeField, Tooltip("Track IDs are the clips' indices in this list.")]
    private List<AudioClip> m_BackgroundMusic = new List<AudioClip>();

    [Header("UI Sounds")]
    [SerializeField]
    private List<AudioClip> m_ButtonClickSFX = new List<AudioClip>();

    [SerializeField]
    private List<AudioClip> m_ButtonSelectSFX = new List<AudioClip>();

    private int m_CurrentMusicId = -1;
    private float m_SFXVolume = 1f;
    private float m_MusicVolume = 1f;

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

        if (Instance.m_AudioSource == null)
        {
            Debug.LogError("Cannot start music because the SoundManager has no AudioSource.");
            return;
        }

        if (Instance.m_CurrentMusicId == musicId && Instance.m_AudioSource.isPlaying) { return; }

        Instance.m_AudioSource.clip = music;
        Instance.m_AudioSource.loop = true;
        Instance.m_AudioSource.volume = Instance.m_MusicVolume;
        Instance.m_AudioSource.pitch = 1f;
        Instance.m_AudioSource.Play();
        Instance.m_CurrentMusicId = musicId;
    }

    public static void StopMusic(int musicId)
    {
        if (Instance.m_BackgroundMusic == null || musicId < 0 || musicId >= Instance.m_BackgroundMusic.Count)
        {
            Debug.LogWarning($"Cannot stop music: track ID {musicId} is out of range.");
            return;
        }

        if (Instance.m_CurrentMusicId != musicId) { return; }

        if (Instance.m_AudioSource != null)
        {
            Instance.m_AudioSource.Stop();
            Instance.m_AudioSource.clip = null;
            Instance.m_AudioSource.loop = false;
        }

        Instance.m_CurrentMusicId = -1;
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

        m_AudioSource = GetComponent<AudioSource>();

        if (m_AudioSource == null)
        {
            Debug.LogError("SoundManager requires an AudioSource component.");
        }
    }

    private void FixedUpdate()
    {
        while (m_SoundQueue.Count > 0)
        {
            SoundRequest request = m_SoundQueue.Dequeue();

            if (request.randomizePitch) { m_AudioSource.pitch = RandomPitch; }
            else { m_AudioSource.pitch = 1f; }

            AudioSource.PlayClipAtPoint(request.source, request.position, m_SFXVolume);
        }
    }
}
