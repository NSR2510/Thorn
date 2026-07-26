using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Combat Settings")]
    [SerializeField] private float comboWindow = 0.6f; // time to chain Attack 1 -> Attack 2
    [SerializeField] private float attackDuration = 0.4f; // how long the attack state/animation lasts
    [SerializeField] private float attackCooldown = 0.4f; // prevent m1 spam
    [SerializeField] private float attackDamageDelay = 0.2f; // damage lands at this time into the animation
    [SerializeField] private float attackDamage = 10f;

    [Header("Attack Hitboxes")]
    [SerializeField] private Collider2D attackLeftHitbox;
    [SerializeField] private Collider2D attackRightHitbox;

    [Header("Block Hitboxes")]
    [SerializeField] private Collider2D blockLeftHitbox;
    [SerializeField] private Collider2D blockRightHitbox;

    [Header("Block Durability Settings")]
    [SerializeField] private int maxBlockCharges = 3;
    [SerializeField] private float guardBreakDuration = 1.5f;

    [Header("Audio")]
    [SerializeField] private AudioClip swordSwingSound;

    private AudioSource audioSource;
    private int currentBlockCharges;
    private float guardBreakCooldownTimer = 0f;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private InputAction attackAction;
    private InputAction guardAction;

    private bool comboReady = false;
    private float comboTimer = 0f;
    private float attackTimer = 0f;
    private float attackCooldownTimer = 0f;
    private float attackDamageDelayTimer = 0f;
    private bool attackDamageApplied = false;
    private ContactFilter2D attackContactFilter;
    private Collider2D[] attackHitResults = new Collider2D[8];
    private bool isGuarding = false;

    public bool IsAttacking => attackTimer > 0f;
    public bool IsGuarding => isGuarding;
    public int CurrentBlockCharges => currentBlockCharges;
    public int MaxBlockCharges => maxBlockCharges;
    public bool IsGuardBroken => guardBreakCooldownTimer > 0f;
    public float AttackDamage => attackDamage;

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        attackContactFilter = new ContactFilter2D();
        attackContactFilter.useTriggers = true;
        attackContactFilter.useLayerMask = false;
        attackContactFilter.useDepth = false;

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

        UpdateAttackHitboxState();
        UpdateBlockHitboxState();
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
        // Don't allow attacking while guarding or spamming.
        if (isGuarding || attackCooldownTimer > 0f || attackTimer > 0f) return;

        if (audioSource != null && swordSwingSound != null)
        {
            audioSource.PlayOneShot(swordSwingSound);
        }

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
        attackCooldownTimer = attackCooldown;
        attackDamageDelayTimer = attackDamageDelay;
        attackDamageApplied = false;
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

        if (attackCooldownTimer > 0f)
        {
            attackCooldownTimer -= Time.deltaTime;
        }

        if (attackDamageDelayTimer > 0f)
        {
            attackDamageDelayTimer -= Time.deltaTime;
        }

        // Active attack state countdown
        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;

            if (!attackDamageApplied && attackDamageDelayTimer <= 0f)
            {
                ApplyPendingAttack();
            }

            if (attackTimer <= 0f)
            {
                attackTimer = 0f;
            }
        }

        UpdateAttackHitboxState();

        // Guard / Block logic
        bool wasGuarding = isGuarding;
        isGuarding = false;

        if (guardBreakCooldownTimer > 0f)
        {
            guardBreakCooldownTimer -= Time.deltaTime;
        }
        else
        {
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
        }

        // If we just started guarding, reset charges to max
        if (isGuarding && !wasGuarding)
        {
            currentBlockCharges = maxBlockCharges;
        }

        UpdateBlockHitboxState();

        if (animator != null)
        {
            if (isGuarding && !IsAttacking)
            {
                animator.Play("Guard");
            }
        }
    }

    public void OnBlockSuccessful()
    {
        if (!isGuarding) return;

        currentBlockCharges--;
        Debug.Log($"Attack blocked! Remaining block charges: {currentBlockCharges}/{maxBlockCharges}");

        if (currentBlockCharges <= 0)
        {
            BreakGuard();
        }
    }

    private void BreakGuard()
    {
        isGuarding = false;
        guardBreakCooldownTimer = guardBreakDuration;
        currentBlockCharges = 0;
        Debug.Log("Guard broken! Player block disabled for " + guardBreakDuration + " seconds.");

        if (animator != null)
        {
            animator.Play("Idle");
        }
    }

    private void ApplyPendingAttack()
    {
        attackDamageApplied = true;

        Collider2D hitbox = GetCurrentAttackHitbox();
        if (hitbox == null)
        {
            return;
        }

        int hitCount = hitbox.Overlap(attackContactFilter, attackHitResults);
        if (hitCount == 0)
        {
            Debug.Log($"{name} attack hitbox overlapped no targets.");
        }

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = attackHitResults[i];
            if (hit == null || hit.gameObject == gameObject)
            {
                continue;
            }

            EnemyHealth enemyHealth = hit.GetComponentInParent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(attackDamage);
                Debug.Log($"{name} hit enemy {enemyHealth.name} for {attackDamage} damage. Enemy HP: {enemyHealth.CurrentHealth}/{enemyHealth.MaxHealth}");
                if (PlayerElementalManager.Instance != null)
                {
                    PlayerElementalManager.Instance.TryApplyElemental(enemyHealth.gameObject);
                }
                continue;
            }

            Boss_HP bossHealth = hit.GetComponentInParent<Boss_HP>();
            if (bossHealth != null)
            {
                bossHealth.TakeDamage(attackDamage);
                Debug.Log($"{name} hit boss {bossHealth.name} for {attackDamage} damage. Boss HP: {bossHealth.CurrentHealth}/{bossHealth.MaxHealth}");
                if (PlayerElementalManager.Instance != null)
                {
                    PlayerElementalManager.Instance.TryApplyElemental(bossHealth.gameObject);
                }
                continue;
            }

            Debug.LogWarning($"{name} hit {hit.name} but no EnemyHealth or Boss_HP found on the target.");
        }
    }

    private Collider2D GetCurrentAttackHitbox()
    {
        if (attackLeftHitbox == null || attackRightHitbox == null)
        {
            return attackRightHitbox != null ? attackRightHitbox : attackLeftHitbox;
        }

        if (spriteRenderer != null && spriteRenderer.flipX)
        {
            return attackLeftHitbox;
        }

        return attackRightHitbox;
    }

    public Collider2D GetCurrentBlockHitbox()
    {
        if (blockLeftHitbox == null || blockRightHitbox == null)
        {
            return blockRightHitbox != null ? blockRightHitbox : blockLeftHitbox;
        }

        if (spriteRenderer != null && spriteRenderer.flipX)
        {
            return blockLeftHitbox;
        }

        return blockRightHitbox;
    }

    private void UpdateAttackHitboxState()
    {
        bool leftActive = IsAttacking && spriteRenderer != null && spriteRenderer.flipX;
        bool rightActive = IsAttacking && spriteRenderer != null && !spriteRenderer.flipX;

        if (attackLeftHitbox != null)
        {
            attackLeftHitbox.enabled = leftActive;
        }

        if (attackRightHitbox != null)
        {
            attackRightHitbox.enabled = rightActive;
        }
    }

    private void UpdateBlockHitboxState()
    {
        bool leftActive = isGuarding && spriteRenderer != null && spriteRenderer.flipX;
        bool rightActive = isGuarding && spriteRenderer != null && !spriteRenderer.flipX;

        if (blockLeftHitbox != null)
        {
            blockLeftHitbox.enabled = leftActive;
        }

        if (blockRightHitbox != null)
        {
            blockRightHitbox.enabled = rightActive;
        }
    }

    /// <summary>
    /// Increase player's attack damage.
    /// </summary>
    /// <param name="amount">Amount of damage to add.</param>
    public void IncreaseDamage(float amount)
    {
        attackDamage += amount;
        Debug.Log($"{name} attack damage increased to {attackDamage}");
    }
}
