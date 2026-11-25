using System.Collections.Generic;
using UnityEngine;

public class SoundManager : Manager<SoundManager>
{
    private struct SoundRequest
    {
        public AudioClip source;
        public Vector3 position;

        public SoundRequest(AudioClip source, Vector3 position)
        {
            this.source = source;
            this.position = position;
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

        SoundManager manager = Instance;
        if (manager == null)
        {
            GameObject managerObject = new GameObject(nameof(SoundManager));
            manager = managerObject.AddComponent<SoundManager>();
        }

        manager.m_SoundQueue.Enqueue(new SoundRequest(source, position));
    }

    private void Update()
    {
        while (m_SoundQueue.Count > 0)
        {
            SoundRequest request = m_SoundQueue.Dequeue();
            AudioSource.PlayClipAtPoint(request.source, request.position);
        }
    }
}
