using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Dùng GetAxisRaw để phản hồi tức thì, không bị trượt
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(moveX, moveY).normalized;
    }

    private void FixedUpdate()
    {
        float moveMult = PlayerStats.Instance != null ? PlayerStats.Instance.moveSpeedMultiplier : 1f;
        rb.linearVelocity = moveInput * (moveSpeed * moveMult);
    }
}