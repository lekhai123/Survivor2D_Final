using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 18f; // Tăng tốc độ bay từ 12 lên 18 để quét ngang dính hơn
    [SerializeField] private float defaultDamage = 10f;
    [SerializeField] private float lifeTime = 2f;

    private Vector2 moveDirection;
    private float currentLifeTime;
    private float currentDamage;

    private void OnEnable()
    {
        currentLifeTime = lifeTime;
        currentDamage = defaultDamage;
    }

    // Nhận cả hướng bay và sát thương thực tế của vũ khí
    public void SetDirection(Vector2 dir, float weaponDamage = -1f)
    {
        moveDirection = dir.normalized;
        if (weaponDamage > 0f)
        {
            currentDamage = weaponDamage;
        }
    }

    private void Update()
    {
        transform.position += (Vector3)(moveDirection * speed * Time.deltaTime);

        currentLifeTime -= Time.deltaTime;
        if (currentLifeTime <= 0f)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        EnemyController enemy = other.GetComponent<EnemyController>();
        if (enemy != null)
        {
            float bonus = PlayerStats.Instance != null ? PlayerStats.Instance.bonusDamage : 0f;
            enemy.TakeDamage(currentDamage + bonus);

            // Đẩy lùi quái nhẹ theo hướng bay của viên đạn để mở đường thoát
            enemy.transform.position += (Vector3)(moveDirection * 0.25f);

            gameObject.SetActive(false);
        }
    }
}