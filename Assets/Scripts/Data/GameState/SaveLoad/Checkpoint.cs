using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;

    public Transform SpawnPoint => spawnPoint ? spawnPoint : transform;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SaveManager.Instance.SaveNow();
            Debug.Log("Checkpoint reached and game saved");
        }
    }


}
