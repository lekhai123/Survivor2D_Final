using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance { get; private set; }

    [SerializeField] private float maxHp = 100f;
    [SerializeField] private Image hpFillBar;
    [SerializeField] private GameObject gameOverPanel;

    private float currentHp;
    private float invulnerableTimer;

    private void Awake() => Instance = this;

    private void Start()
    {
        currentHp = maxHp;
        UpdateHealthUI();
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    private void Update()
    {
        if (invulnerableTimer > 0f)
            invulnerableTimer -= Time.deltaTime;
    }
    public void Heal(float amount)
    {
        currentHp = Mathf.Min(maxHp, currentHp + amount);
        UpdateHealthUI();
    }
    public void TakeDamage(float amount)
    {
        if (invulnerableTimer > 0f) return;

        currentHp -= amount;
        invulnerableTimer = 0.3f;
        UpdateHealthUI();

        if (currentHp <= 0f)
        {
            currentHp = 0f;
            Die();
        }
    }

    private void UpdateHealthUI()
    {
        if (hpFillBar != null)
            hpFillBar.fillAmount = currentHp / maxHp;
    }
    public void TriggerVictory()
    {
        Time.timeScale = 0f;
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            TMPro.TextMeshProUGUI title = gameOverPanel.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (title != null)
            {
                title.text = "VICTORY!";
                title.color = Color.yellow;
            }
        }
    }
    private void Die()
    {
        Time.timeScale = 0f;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    // Gắn hàm này vào nút RestartButton
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}