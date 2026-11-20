using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float lifeTime = 2f;

    private Vector2 moveDirection;
    private float currentLifeTime;

    private void OnEnable()
    {
        currentLifeTime = lifeTime;
    }

    public void SetDirection(Vector2 dir)
    {
        moveDirection = dir.normalized;
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
            enemy.TakeDamage(damage + bonus);
            gameObject.SetActive(false);
        }
    }
}