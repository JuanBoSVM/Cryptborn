using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : Manager<SoundManager>
{
    private AudioSource m_AudioSource;

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
