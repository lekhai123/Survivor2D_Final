using MyPooler;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3.2f;
    [SerializeField] private float maxHp = 12f;
    [SerializeField] private float contactDamage = 8f;

    [Header("AI Swarm Separation")]
    [SerializeField] private float separationRadius = 1.6f; // Bán kính giữ khoảng cách giữa các con quái
    [SerializeField] private float separationForce = 3.5f;  // Lực đẩy tách đàn
    [SerializeField] private LayerMask enemyLayer;          // Chọn Layer "Enemy" trên Prefab

    private float currentHp;
    private float currentMoveSpeed;
    private float currentDamage;
    private bool isEliteEnemy;
    private bool isBossEnemy;
    private bool isDead;

    private float approachAngleOffset; // Góc tiếp cận riêng của từng con để bao vây rộng
    private Transform playerTransform;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        SetupSpecialEnemy(false, false);
        isDead = false;

        // Mỗi con quái có 1 góc tiếp cận lệch từ -55 đến +55 độ khi ở xa, tạo thế bao vây rộng thay vì xếp 1 hàng dọc
        approachAngleOffset = Random.Range(-55f, 55f);

        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTransform = playerObj.transform;
        }
    }

    public void SetupSpecialEnemy(bool isElite, bool isBoss)
    {
        isEliteEnemy = isElite;
        isBossEnemy = isBoss;

        int playerLv = LevelSystem.Instance != null ? LevelSystem.Instance.CurrentLevel : 1;
        float timeMult = WaveDirector.Instance != null ? WaveDirector.Instance.timeMultiplier : 1f;

        float hpScale = (1f + (playerLv - 1) * 0.15f) * timeMult;
        float speedScale = Mathf.Min(1.3f, 1f + (playerLv - 1) * 0.03f);

        if (isBoss)
        {
            hpScale *= 15f;
            speedScale *= 0.9f;
            transform.localScale = Vector3.one * 2.8f;
            if (spriteRenderer != null) spriteRenderer.color = Color.black;
        }
        else if (isElite)
        {
            hpScale *= 3.5f;
            transform.localScale = Vector3.one * 1.5f;
            if (spriteRenderer != null) spriteRenderer.color = new Color(0.7f, 0.2f, 1f);
        }
        else
        {
            transform.localScale = Vector3.one;
            if (spriteRenderer != null) spriteRenderer.color = Color.red;
        }

        currentHp = maxHp * hpScale;
        currentMoveSpeed = moveSpeed * speedScale;
        currentDamage = contactDamage * (1f + (timeMult - 1f) * 0.5f);
    }

    private void Update()
    {
        if (playerTransform == null) return;

        float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // 1. Hướng truy đuổi: ở xa (> 3.5m) thì chạy vòng theo góc lệch để dàn đội hình; áp sát (< 3.5m) thì lao thẳng vào Player
        Vector2 seekDir = (playerTransform.position - transform.position).normalized;
        if (distToPlayer > 3.5f && !isBossEnemy)
        {
            float fadeFactor = Mathf.Clamp01((distToPlayer - 3.5f) / 4f);
            seekDir = Quaternion.Euler(0, 0, approachAngleOffset * fadeFactor) * seekDir;
        }

        // 2. Lực tách đàn (Separation): đẩy lùi khỏi các con quái khác trong bán kính 1.6m để không bao giờ dính chùm
        Vector2 separationDir = Vector2.zero;
        Collider2D[] neighbors = Physics2D.OverlapCircleAll(transform.position, separationRadius, enemyLayer);
        foreach (var neighbor in neighbors)
        {
            if (neighbor.gameObject == gameObject) continue;
            Vector2 away = (Vector2)(transform.position - neighbor.transform.position);
            float d = away.magnitude;
            if (d > 0.001f)
            {
                // Càng gần nhau lực đẩy càng mạnh
                separationDir += away.normalized * ((separationRadius - d) / separationRadius);
            }
        }

        // Tổng hợp hướng chạy + hướng tách đàn
        Vector2 finalMove = (seekDir * currentMoveSpeed) + (separationDir * separationForce);
        transform.position += (Vector3)(finalMove * Time.deltaTime);
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return; // Chặn 10 viên đạn trúng cùng lúc kích hoạt Die() nhiều lần
        currentHp -= amount;
        if (currentHp <= 0)
        {
            isDead = true;
            Die();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.TakeDamage(currentDamage);
        }
    }

    private void Die()
    {
        int gemCount = (isEliteEnemy || isBossEnemy) ? Random.Range(5, 9) : Random.Range(1, 4);

        for (int i = 0; i < gemCount; i++)
        {
            Vector2 scatter = Random.insideUnitCircle * 0.4f;
            ObjectPooler.Instance.GetFromPool("EXP_Gem", transform.position + (Vector3)scatter, Quaternion.identity);
        }

        // Quái Elite/Boss rớt 100%, quái thường có 30% tỷ lệ rớt bình máu
        if (isEliteEnemy || isBossEnemy || Random.value < 0.30f)
        {
            ObjectPooler.Instance.GetFromPool("Health_Orb", transform.position, Quaternion.identity);
        }

        if ((isEliteEnemy || isBossEnemy) && WaveDirector.Instance != null)
        {
            WaveDirector.Instance.DropChestAt(transform.position);
        }
        if (isBossEnemy && PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.TriggerVictory();
        }
        gameObject.SetActive(false);
    }
}