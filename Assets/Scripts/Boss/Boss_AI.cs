using UnityEngine;

[RequireComponent(typeof(Rigidbody2D)), RequireComponent(typeof(Boss_HP))]
public class Boss_AI : MonoBehaviour
{
    private enum BossState
    {
        Chasing,
        Windup,
        Attacking,
        Recovery,
        Dead
    }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float attackRange = 1.5f;

    [Header("Combat")]
    [SerializeField] private float windupDuration = 2f;
    [SerializeField] private float attackWindowDuration = 3f;
    [SerializeField] private float attackAnimationDuration = 0.6f;
    [SerializeField] private float firstSwingTime = 0.2f;
    [SerializeField] private float secondSwingTime = 0.5f;
    [SerializeField] private float recoveryDuration = 5f;
    [SerializeField] private float damageAmount = 20f;

    [Header("Hitboxes")]
    [SerializeField] private Collider2D leftAttackHitbox;
    [SerializeField] private Collider2D rightAttackHitbox;

    [Header("Animations")]
    [SerializeField] private string walkAnimationName = "Walk";
    [SerializeField] private string windupAnimationName = "Wind-up";
    [SerializeField] private string attackAnimationName = "Attack";
    [SerializeField] private string recoveryAnimationName = "Recovery";
    [SerializeField] private string deathAnimationName = "Dead";

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Boss_HP health;
    private Transform player;
    private BossState currentState = BossState.Chasing;
    private Vector2 moveDirection = Vector2.zero;
    private float stateTimer;
    private float attackCycleTimer;
    private bool hasFirstSwingTriggered;
    private bool hasSecondSwingTriggered;
    private bool isDead;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        health = GetComponent<Boss_HP>();

        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    /// <summary>
    /// Set the boss's attack damage amount dynamically.
    /// </summary>
    public void SetDamageAmount(float amount)
    {
        damageAmount = amount;
    }

    void Start()
    {
        if (health != null)
        {
            health.onDeath.AddListener(OnDeath);
        }


        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        currentState = BossState.Chasing;
        PlayAnimation(walkAnimationName);
    }

    void Update()
    {
        if (isDead)
        {
            return;
        }

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player == null)
            {
                return;
            }
        }

        switch (currentState)
        {
            case BossState.Chasing:
                UpdateChasing();
                break;
            case BossState.Windup:
                UpdateWindup();
                break;
            case BossState.Attacking:
                UpdateAttacking();
                break;
            case BossState.Recovery:
                UpdateRecovery();
                break;
        }

        UpdateHitboxState();
    }

    private void FixedUpdate()
    {
        if (rb == null || isDead)
        {
            return;
        }

        rb.linearVelocity = moveDirection * moveSpeed;
    }

    private void UpdateChasing()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange)
        {
            moveDirection = Vector2.zero;
            StartWindup();
            return;
        }

        Vector2 direction = (player.position - transform.position).normalized;
        moveDirection = direction;
        UpdateFacing(direction.x);
        PlayAnimation(walkAnimationName);
    }

    private void StartWindup()
    {
        currentState = BossState.Windup;
        stateTimer = windupDuration;
        moveDirection = Vector2.zero;
        PlayAnimation(windupAnimationName);
    }

    private void UpdateWindup()
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            StartAttack();
        }
    }

    private void StartAttack()
    {
        currentState = BossState.Attacking;
        stateTimer = attackWindowDuration;
        attackCycleTimer = 0f;
        hasFirstSwingTriggered = false;
        hasSecondSwingTriggered = false;
        PlayAnimation(attackAnimationName);
    }

    private void UpdateAttacking()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        moveDirection = direction;
        UpdateFacing(direction.x);
        PlayAnimation(attackAnimationName);

        stateTimer -= Time.deltaTime;
        attackCycleTimer += Time.deltaTime;

        if (!hasFirstSwingTriggered && attackCycleTimer >= firstSwingTime)
        {
            DealAttackDamage();
            hasFirstSwingTriggered = true;
        }

        if (!hasSecondSwingTriggered && attackCycleTimer >= secondSwingTime)
        {
            DealAttackDamage();
            hasSecondSwingTriggered = true;
        }

        if (attackCycleTimer >= attackAnimationDuration)
        {
            attackCycleTimer -= attackAnimationDuration;
            hasFirstSwingTriggered = false;
            hasSecondSwingTriggered = false;
        }

        if (stateTimer <= 0f)
        {
            StartRecovery();
        }
    }

    private void StartRecovery()
    {
        currentState = BossState.Recovery;
        stateTimer = recoveryDuration;
        moveDirection = Vector2.zero;
        PlayAnimation(recoveryAnimationName);
    }

    private void UpdateRecovery()
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            currentState = BossState.Chasing;
            PlayAnimation(walkAnimationName);
        }
    }

    private void DealAttackDamage()
    {
        Collider2D activeHitbox = rightAttackHitbox != null && rightAttackHitbox.enabled ? rightAttackHitbox : leftAttackHitbox;
        if (activeHitbox == null)
        {
            return;
        }

        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;
        filter.useLayerMask = false;
        filter.useDepth = false;

        Collider2D[] results = new Collider2D[8];
        int count = activeHitbox.Overlap(filter, results);
        for (int i = 0; i < count; i++)
        {
            Collider2D hit = results[i];
            if (hit == null || hit.gameObject == gameObject)
            {
                continue;
            }

            Health playerHealth = hit.GetComponentInParent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
                Debug.Log($"Boss {name} hit {playerHealth.name} for {damageAmount} damage.");
            }
        }
    }

    private void UpdateHitboxState()
    {
        if (currentState == BossState.Attacking)
        {
            if (leftAttackHitbox != null)
            {
                leftAttackHitbox.enabled = spriteRenderer != null && spriteRenderer.flipX;
            }

            if (rightAttackHitbox != null)
            {
                rightAttackHitbox.enabled = spriteRenderer == null || !spriteRenderer.flipX;
            }

            return;
        }

        if (leftAttackHitbox != null)
        {
            leftAttackHitbox.enabled = false;
        }

        if (rightAttackHitbox != null)
        {
            rightAttackHitbox.enabled = false;
        }
    }

    private void UpdateFacing(float xDirection)
    {
        if (spriteRenderer == null)
        {
            return;
        }

        if (xDirection > 0f)
        {
            spriteRenderer.flipX = false;
        }
        else if (xDirection < 0f)
        {
            spriteRenderer.flipX = true;
        }
    }

    private void PlayAnimation(string animationName)
    {
        if (animator == null || string.IsNullOrEmpty(animationName))
        {
            return;
        }

        animator.Play(animationName);
    }

    private void OnDeath()
    {
        isDead = true;
        currentState = BossState.Dead;
        moveDirection = Vector2.zero;
        stateTimer = 2f;
        PlayAnimation(deathAnimationName);
        DisableHitboxes();
        Invoke(nameof(Despawn), stateTimer);
    }

    private void DisableHitboxes()
    {
        if (leftAttackHitbox != null)
        {
            leftAttackHitbox.enabled = false;
        }

        if (rightAttackHitbox != null)
        {
            rightAttackHitbox.enabled = false;
        }
    }

    private void Despawn()
    {
        Destroy(gameObject);
    }
}
