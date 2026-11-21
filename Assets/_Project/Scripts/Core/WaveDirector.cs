using UnityEngine;
using MyPooler;

public class WaveDirector : MonoBehaviour
{
    public static WaveDirector Instance { get; private set; }

    [Header("Spawn Settings")]
    [SerializeField] private string normalEnemyTag = "Enemy_Normal";
    [SerializeField] private string chestPoolTag = "TreasureChest";
    [SerializeField] private float baseSpawnInterval = 2f;
    [SerializeField] private float spawnRadius = 15f;

    [Header("3-Minute Test Cycle (Seconds)")]
    [SerializeField] private float evolutionStepSeconds = 15f; // Mỗi 15s quái tiến hóa
    [SerializeField] private float dpsCheckStepSeconds = 45f;  // Mỗi 45s đàn quái + Elite rớt rương
    [SerializeField] private float bossSpawnSeconds = 180f;    // Phút thứ 3 (180s) ra Boss cuối

    [Header("Progression Scaling")]
    public float timeMultiplier = 1f;

    private Transform playerTransform;
    private float elapsedTime;
    private float spawnTimer;

    private int lastEvoStep = 0;
    private int lastDpsStep = 0;
    private bool bossSpawned = false;

    private void Awake() => Instance = this;

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTransform = playerObj.transform;
    }

    private void Update()
    {
        if (playerTransform == null) return;

        elapsedTime += Time.deltaTime;

        // 1. MỖI 15 GIÂY: Quái tiến hóa sức mạnh & rút ngắn nhịp đẻ quái
        int currentEvoStep = Mathf.FloorToInt(elapsedTime / evolutionStepSeconds);
        if (currentEvoStep > lastEvoStep)
        {
            lastEvoStep = currentEvoStep;
            timeMultiplier += 0.25f;
            baseSpawnInterval = Mathf.Max(0.25f, baseSpawnInterval * 0.85f);
            Debug.Log($"[WaveDirector - {elapsedTime:F0}s] Quái tiến hóa! Hệ số x{timeMultiplier:F2}");
        }

        // 2. MỖI 45 GIÂY: DPS Check + Quái Elite rớt rương
        int currentDpsStep = Mathf.FloorToInt(elapsedTime / dpsCheckStepSeconds);
        if (currentDpsStep > lastDpsStep && elapsedTime < bossSpawnSeconds)
        {
            lastDpsStep = currentDpsStep;
            TriggerDpsCheckWave(16 + currentDpsStep * 6);
        }

        // 3. ĐÚNG 3 PHÚT (180s): Boss cuối xuất hiện & ngừng sinh toàn bộ quái nhỏ
        if (elapsedTime >= bossSpawnSeconds && !bossSpawned)
        {
            bossSpawned = true;
            SpawnFinalBoss();
        }

        // Khi Boss cuối đã ra sân thì không đẻ thêm bot nhỏ bảo kê nữa
        if (bossSpawned) return;

        // Sinh quái thường liên tục trước phút thứ 3
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnEnemyAroundPlayer(false);
            spawnTimer = baseSpawnInterval;
        }
    }

    private void TriggerDpsCheckWave(int swarmCount)
    {
        // Giảm số lượng đàn vây vừa phải (12 - 14 con) và chừa góc mở 70 độ để người chơi có lối thoát
        int actualCount = Mathf.Min(14, swarmCount);
        float startAngle = Random.Range(0f, 360f);
        float arcSpan = 260f; // Chỉ bao vây 260 độ, chừa 100 độ khe hở

        for (int i = 0; i < actualCount; i++)
        {
            float angle = startAngle + i * (arcSpan / actualCount);
            Vector2 offset = Quaternion.Euler(0, 0, angle) * Vector2.right * spawnRadius;
            SpawnAtPosition((Vector2)playerTransform.position + offset, false);
        }
        SpawnEnemyAroundPlayer(true);
    }
    private void SpawnFinalBoss()
    {
        Debug.Log("[WaveDirector - 180s] FINAL BOSS XUẤT HIỆN!");
        Vector2 randomDir = Random.insideUnitCircle.normalized * spawnRadius;
        GameObject boss = ObjectPooler.Instance.GetFromPool(normalEnemyTag, (Vector2)playerTransform.position + randomDir, Quaternion.identity);
        if (boss != null && boss.TryGetComponent(out EnemyController enemy))
        {
            enemy.SetupSpecialEnemy(isElite: true, isBoss: true);
        }
    }

    private void SpawnEnemyAroundPlayer(bool isElite)
    {
        Vector2 randomDir = Random.insideUnitCircle.normalized * spawnRadius;
        SpawnAtPosition((Vector2)playerTransform.position + randomDir, isElite);
    }

    private void SpawnAtPosition(Vector2 pos, bool isElite)
    {
        GameObject obj = ObjectPooler.Instance.GetFromPool(normalEnemyTag, pos, Quaternion.identity);
        if (obj != null && obj.TryGetComponent(out EnemyController enemy))
        {
            enemy.SetupSpecialEnemy(isElite, false);
        }
    }

    public void DropChestAt(Vector3 pos)
    {
        ObjectPooler.Instance.GetFromPool(chestPoolTag, pos, Quaternion.identity);
    }
}