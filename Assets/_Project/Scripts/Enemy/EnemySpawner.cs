using UnityEngine;
using MyPooler;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private float spawnInterval = 1.5f;
    [SerializeField] private float spawnRadius = 10f;
    private Transform playerTransform;
    private float timer;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    private void Update()
    {
        if (playerTransform == null) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            SpawnEnemy();
            timer = spawnInterval;
        }
    }

    private void SpawnEnemy()
    {
        // Lấy ngẫu nhiên 1 điểm trên vòng tròn cách Player 10 đơn vị (ngoài màn hình)
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        Vector3 spawnPos = playerTransform.position + (Vector3)(randomDir * spawnRadius);

        // Lấy quái từ PoolManager
        ObjectPooler.Instance.GetFromPool("Enemy_Normal", spawnPos, Quaternion.identity);
    }
}