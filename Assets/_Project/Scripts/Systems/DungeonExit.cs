using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DungeonExit : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Let the GameManager handle the scene transition
            GameManager.HandleDungeonExit();
        }
    }
}
