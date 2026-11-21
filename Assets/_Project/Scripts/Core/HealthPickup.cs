using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [SerializeField] private float healAmount = 20f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.Heal(healAmount);
            gameObject.SetActive(false);
        }
    }
}