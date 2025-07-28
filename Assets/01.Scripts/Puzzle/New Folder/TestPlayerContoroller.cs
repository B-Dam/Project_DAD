using System.Collections;
using UnityEngine;

public class TestPlayerController : MonoBehaviour
{
    public static TestPlayerController Instance;

    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 lastMoveDirection = Vector2.right;

    [Header("박스 밀기 관련")]
    public float boxPushHoldTime = 0.5f;
    private float pushTimer = 0f;
    private bool isTryingToPush = false;

    private void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        HandleMovementInput();
        CheckPushHoldAndTry();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void HandleMovementInput()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(moveX, moveY).normalized;

        if (moveInput != Vector2.zero)
        {
            lastMoveDirection = moveInput;
        }
    }

    private void MovePlayer()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }

    private void CheckPushHoldAndTry()
    {
        if (moveInput != Vector2.zero)
        {
            if (!isTryingToPush)
            {
                isTryingToPush = true;
                pushTimer = 0f;
            }

            pushTimer += Time.deltaTime;

            if (pushTimer >= boxPushHoldTime)
            {
                TryAutoPushBox();
                isTryingToPush = false;
            }
        }
        else
        {
            isTryingToPush = false;
            pushTimer = 0f;
        }
    }

    private void TryAutoPushBox()
    {
        Vector2 origin = transform.position;
        Vector2 dir = lastMoveDirection.normalized;
        float distance = 0.6f;

        RaycastHit2D hit = Physics2D.Raycast(origin, dir, distance, LayerMask.GetMask("Box"));
        if (hit.collider != null)
        {
            var box = hit.collider.GetComponent<BoxPush>();
            if (box != null)
            {
                box.TryPush(dir);
            }
        }
    }
    public void SetVelocity(Vector2 velocity)
    {
        rb.linearVelocity = velocity;
    }
}
