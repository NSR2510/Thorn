using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Combat Settings")]
    [SerializeField] private float comboWindow = 0.6f; // time to chain Attack 1 -> Attack 2
    [SerializeField] private float attackDuration = 0.35f; // how long the attack state/animation lasts

    private Animator animator;
    private InputAction attackAction;
    private InputAction guardAction;

    private bool comboReady = false;
    private float comboTimer = 0f;
    private float attackTimer = 0f;
    private bool isGuarding = false;

    public bool IsAttacking => attackTimer > 0f;
    public bool IsGuarding => isGuarding;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (InputSystem.actions != null)
        {
            attackAction = InputSystem.actions.FindAction("Player/Attack");
            guardAction = InputSystem.actions.FindAction("Player/Guard");

            if (attackAction != null)
            {
                attackAction.actionMap.Enable();
                attackAction.performed += OnAttack;
            }
        }
    }

    void OnDestroy()
    {
        if (attackAction != null)
        {
            attackAction.performed -= OnAttack;
        }
    }

    void OnAttack(InputAction.CallbackContext ctx)
    {
        PerformAttack();
    }

    private void PerformAttack()
    {
        // Don't allow attacking while guarding
        if (isGuarding) return;

        if (comboReady)
        {
            animator.Play("Attack 2");
            comboReady = false;
        }
        else
        {
            animator.Play("Attack 1");
            comboReady = true;
            comboTimer = comboWindow;
        }

        attackTimer = attackDuration;
    }

    void Update()
    {
        // Fallback for Left Click if action is not triggered/enabled
        if (attackAction == null || !attackAction.enabled)
        {
            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                PerformAttack();
            }
        }

        // Combo window countdown
        if (comboReady)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f)
            {
                comboReady = false;
            }
        }

        // Active attack state countdown
        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
        }

        // Guard / Block logic
        isGuarding = false;
        if (guardAction != null)
        {
            isGuarding = guardAction.IsPressed();
        }
        else
        {
            var mouse = Mouse.current;
            if (mouse != null)
            {
                isGuarding = mouse.rightButton.isPressed;
            }
        }

        if (animator != null)
        {
            if (isGuarding && !IsAttacking)
            {
                animator.Play("Guard");
            }
        }
    }
}
