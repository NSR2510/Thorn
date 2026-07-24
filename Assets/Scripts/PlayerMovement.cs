using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 5f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private InputAction moveAction;
    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Ensure we have access to the New Input System actions
        if (InputSystem.actions != null)
        {
            moveAction = InputSystem.actions.FindAction("Player/Move");
            if (moveAction != null)
            {
                moveAction.actionMap.Enable();
            }
            else
            {
                Debug.LogError("Player/Move action not found in Input Actions asset.");
            }
        }
        else
        {
            Debug.LogError("No project-wide Input Action asset assigned in Project Settings.");
        }
    }

    void Update()
    {
        if (moveAction != null)
        {
            moveInput = moveAction.ReadValue<Vector2>();
        }

        // Flip sprite based on direction of horizontal movement
        if (spriteRenderer != null)
        {
            if (moveInput.x > 0.01f)
            {
                spriteRenderer.flipX = false;
            }
            else if (moveInput.x < -0.01f)
            {
                spriteRenderer.flipX = true;
            }
        }
    }

    void FixedUpdate()
    {
        // Apply movement vector directly to velocity
        rb.linearVelocity = moveInput * speed;
    }
}

