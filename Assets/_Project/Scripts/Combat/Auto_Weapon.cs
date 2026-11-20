using UnityEngine;
using MyPooler;

public class AutoWeapon : MonoBehaviour
{
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private float detectRadius = 8f;
    [SerializeField] private LayerMask enemyLayer;

    private float fireTimer;

    private void Update()
    {
        if (fireTimer > 0f)
        {
            fireTimer -= Time.deltaTime;
        }

        if (fireTimer <= 0f)
        {
            Transform nearestEnemy = FindNearestEnemy();
            if (nearestEnemy != null)
            {
                Shoot(nearestEnemy);

                // Áp dụng hệ số tăng tốc độ bắn từ PlayerStats
                float speedMult = PlayerStats.Instance != null ? PlayerStats.Instance.fireRateMultiplier : 1f;
                fireTimer = fireRate / speedMult;
            }
        }
    }

    private Transform FindNearestEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectRadius, enemyLayer);
        Transform nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider2D hit in hits)
        {
            float dist = Vector2.Distance(transform.position, hit.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = hit.transform;
            }
        }
        return nearest;
    }

    private void Shoot(Transform target)
    {
        Vector2 direction = (target.position - transform.position).normalized;
        GameObject bulletObj = ObjectPooler.Instance.GetFromPool("Bullet_Basic", transform.position, Quaternion.identity);

        if (bulletObj != null)
        {
            Bullet bullet = bulletObj.GetComponent<Bullet>();
            if (bullet != null)
            {
                bullet.SetDirection(direction);
            }
        }
    }
}