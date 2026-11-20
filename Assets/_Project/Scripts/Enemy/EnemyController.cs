using MyPooler;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4.2f; // Khởi điểm gần bằng tốc độ Player (5.0)
    [SerializeField] private float maxHp = 15f;
    [SerializeField] private float contactDamage = 10f;

    private float currentHp;
    private float currentMoveSpeed;
    private Transform playerTransform;

    private void OnEnable()
    {
        int playerLv = LevelSystem.Instance != null ? LevelSystem.Instance.CurrentLevel : 1;

        // Máu tăng 25% mỗi cấp của Player
        float hpScale = 1f + (playerLv - 1) * 0.25f;
        // Tốc độ tăng 5% mỗi cấp, đạt ngang và nhỉnh hơn Player (5.0 -> 5.8) ở các cấp sau
        float speedScale = Mathf.Min(1.4f, 1f + (playerLv - 1) * 0.05f);

        currentHp = maxHp * hpScale;
        currentMoveSpeed = moveSpeed * speedScale;

        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTransform = playerObj.transform;
        }
    }

    public void Init(Transform target)
    {
        playerTransform = target;
    }

    private void Update()
    {
        if (playerTransform == null) return;
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        transform.position += direction * currentMoveSpeed * Time.deltaTime;
    }

    public void TakeDamage(float amount)
    {
        currentHp -= amount;
        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.TakeDamage(contactDamage);
        }
    }

    private void Die()
    {
        ObjectPooler.Instance.GetFromPool("EXP_Gem", transform.position, Quaternion.identity);
        gameObject.SetActive(false);
    }
}