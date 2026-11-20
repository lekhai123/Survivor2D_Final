using UnityEngine;

public class ExpGem : MonoBehaviour
{
    [SerializeField] private int expValue = 10;
    [SerializeField] private float magnetRadius = 3f;
    [SerializeField] private float moveSpeed = 8f;
    private Transform playerTransform;

    private void OnEnable()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }
    }

    private void Update()
    {
        if (playerTransform == null) return;
        float dist = Vector2.Distance(transform.position, playerTransform.position);
        if (dist <= magnetRadius)
        {
            transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, moveSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            LevelSystem.Instance.AddExp(expValue);
            gameObject.SetActive(false); // Trả về Pool
        }
    }
}