using UnityEngine;

public class TreasureChest : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && ChestSystem.Instance != null)
        {
            ChestSystem.Instance.OpenChest();
            gameObject.SetActive(false);
        }
    }
}