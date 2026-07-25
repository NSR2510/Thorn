using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 5f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private PlayerCombat combat;
    private InputAction moveAction;
    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        combat = GetComponent<PlayerCombat>();

        if (InputSystem.actions != null)
        {
            moveAction = InputSystem.actions.FindAction("Player/Move");
            if (moveAction != null)
            {
                moveAction.actionMap.Enable();
            }
        }
    }

    void Update()
    {
        // 1. Read input from Input System Actions
        if (moveAction != null && moveAction.enabled)
        {
            moveInput = moveAction.ReadValue<Vector2>();
        }
        else
        {
            moveInput = Vector2.zero;
        }

        // 2. Robust direct keyboard reading fallback
        if (moveInput.sqrMagnitude < 0.01f)
        {
            var kb = Keyboard.current;
            if (kb != null)
            {
                float x = 0;
                float y = 0;
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) y = 1;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) y = -1;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) x = -1;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x = 1;
                moveInput = new Vector2(x, y).normalized;
            }
        }

        // 3. Flip Sprite based on horizontal input
        if (spriteRenderer != null && moveInput.x != 0)
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

        // 4. Handle direct animation playback (avoiding missing parameters/transitions)
        if (animator != null)
        {
            // Only play movement animations if we aren't currently attacking or guarding
            bool isCombatActive = combat != null && (combat.IsAttacking || combat.IsGuarding);
            if (!isCombatActive)
            {
                if (moveInput.sqrMagnitude > 0.01f)
                {
                    animator.Play("Run");
                }
                else
                {
                    animator.Play("Idle");
                }
            }
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveInput * speed;
    }
}
