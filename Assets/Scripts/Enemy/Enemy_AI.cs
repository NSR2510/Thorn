using UnityEngine;

[RequireComponent(typeof(Rigidbody2D)), RequireComponent(typeof(EnemyHealth))]
public class Enemy_AI : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float patrolDistance = 3f;
    [SerializeField] private float chaseRadius = 5f;
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float attackAnimationDuration = 0.8f;
    [SerializeField] private float attackDamageDelay = 0.4f;
    [SerializeField] private float damageAmount = 10f;
    [SerializeField] private float attackInterval = 1.5f;

    [Header("Hitboxes")]
    [SerializeField] private Collider2D leftHitbox;
    [SerializeField] private Collider2D rightHitbox;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private EnemyHealth health;
    private string currentAnimation;
    private Transform player;
    private Vector3 startPosition;
    private Vector2 patrolTarget;
    private float patrolTimer;
    [SerializeField] private float patrolMoveDuration = 1f;
    [SerializeField] private float patrolIdleDuration = 1.5f;
    [Header("Animation")]
    [SerializeField] private string idleAnimationName = "Idle";
    [SerializeField] private string runAnimationName = "Run";
    [SerializeField] private string attackAnimationName = "Attack";
    private bool patrolIdle;
    private float attackCooldownTimer;
    private float attackLockTimer;
    private float attackDamageDelayTimer;
    private bool attackDamageApplied;
    private bool isAttacking;
    private bool facingRight = true;
    private Vector2 moveDirection = Vector2.zero;
    private bool blockedLeft;
    private bool blockedRight;
    private bool blockedUp;
    private bool blockedDown;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        health = GetComponent<EnemyHealth>();
        startPosition = transform.position;
        patrolTarget = startPosition;
        patrolTimer = 0f;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void Start()
    {
        if (health != null)
        {
            health.onDeath.AddListener(Die);
        }

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        SelectNewPatrolTarget();
        patrolTimer = patrolMoveDuration;
        patrolIdle = false;
    }

    private void Update()
    {

        if (attackCooldownTimer > 0f)
        {
            attackCooldownTimer -= Time.deltaTime;
        }

        if (attackLockTimer > 0f)
        {
            attackLockTimer -= Time.deltaTime;
        }

        if (attackDamageDelayTimer > 0f)
        {
            attackDamageDelayTimer -= Time.deltaTime;
        }

        if (isAttacking)
        {
            moveDirection = Vector2.zero;

            if (!attackDamageApplied && attackDamageDelayTimer <= 0f)
            {
                ApplyPendingAttack();
            }

            if (attackLockTimer <= 0f)
            {
                EndAttack();
            }

            UpdateHitboxState();
            return;
        }

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        if (player == null)
        {
            Patrol();
            UpdateMovementAnimation();
            TryAttack();
            UpdateHitboxState();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= chaseRadius)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }

        UpdateMovementAnimation();
        TryAttack();
        UpdateHitboxState();
    }

    private void FixedUpdate()
    {
        if (rb == null)
        {
            return;
        }

        rb.linearVelocity = moveDirection * moveSpeed;
    }

    private void Patrol()
    {
        patrolTimer -= Time.deltaTime;

        if (patrolIdle)
        {
            moveDirection = Vector2.zero;

            if (patrolTimer <= 0f)
            {
                patrolIdle = false;
                patrolTimer = patrolMoveDuration;
                SelectNewPatrolTarget();
            }

            return;
        }

        Vector2 currentPosition = transform.position;
        float distanceToTarget = Vector2.Distance(currentPosition, patrolTarget);

        if (distanceToTarget < 0.25f || patrolTimer <= 0f)
        {
            moveDirection = Vector2.zero;
            patrolIdle = true;
            patrolTimer = patrolIdleDuration;
            return;
        }

        moveDirection = (patrolTarget - currentPosition).normalized;
        ApplyCollisionBlocks(ref moveDirection);

        if (Mathf.Abs(moveDirection.x) > 0.1f)
        {
            UpdateFacing(moveDirection.x);
        }
    }

    private void ChasePlayer()
    {
        if (player == null)
        {
            return;
        }

        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        moveDirection = new Vector2(directionToPlayer.x, directionToPlayer.y);
        ApplyCollisionBlocks(ref moveDirection);

        if (Mathf.Abs(directionToPlayer.x) > 0.1f)
        {
            UpdateFacing(directionToPlayer.x);
        }
    }

    private void Move(float direction)
    {
        if (Mathf.Abs(direction) < 0.01f)
        {
            moveDirection = Vector2.zero;
            return;
        }

        moveDirection = new Vector2(direction, 0f);
        UpdateFacing(direction);
    }

    private void TryAttack()
    {
        if (isAttacking || attackCooldownTimer > 0f || player == null)
        {
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        bool isInHitbox = IsPlayerInAnyHitbox();

        if (distanceToPlayer <= attackRange || isInHitbox)
        {
            StartAttack();
        }
    }

    private void StartAttack()
    {
        isAttacking = true;
        attackLockTimer = attackAnimationDuration;
        attackDamageDelayTimer = Mathf.Min(attackDamageDelay, attackAnimationDuration);
        attackCooldownTimer = attackInterval;
        attackDamageApplied = false;
        PlayAnimation(attackAnimationName);
    }

    private void UpdateMovementAnimation()
    {
        if (isAttacking)
        {
            return;
        }

        if (moveDirection.sqrMagnitude > 0.01f)
        {
            PlayAnimation(runAnimationName);
        }
        else
        {
            PlayAnimation(idleAnimationName);
        }
    }

    private void PlayAnimation(string animationName)
    {
        if (animator == null)
        {
            return;
        }

        if (animationName == attackAnimationName)
        {
            animator.Play(animationName, 0, 0f);
            currentAnimation = animationName;
            return;
        }

        if (currentAnimation == animationName)
        {
            return;
        }

        animator.Play(animationName);
        currentAnimation = animationName;
    }

    private void ApplyPendingAttack()
    {
        attackDamageApplied = true;

        if (player == null)
        {
            return;
        }

        PlayerCombat playerCombat = player.GetComponent<PlayerCombat>();
        Collider2D enemyAttackHitbox = leftHitbox != null && leftHitbox.enabled ? leftHitbox : rightHitbox;
        if (playerCombat != null && playerCombat.IsGuarding && enemyAttackHitbox != null)
        {
            Collider2D playerBlockHitbox = playerCombat.GetCurrentBlockHitbox();
            if (playerBlockHitbox == null)
            {
                Debug.LogWarning($"{player.name} is guarding, but no block hitbox is assigned.");
            }
            else
            {
                ContactFilter2D blockFilter = new ContactFilter2D();
                blockFilter.useTriggers = true;
                blockFilter.useLayerMask = false;
                blockFilter.useDepth = false;

                Collider2D[] overlapResults = new Collider2D[4];
                int overlapCount = playerBlockHitbox.Overlap(blockFilter, overlapResults);
                for (int i = 0; i < overlapCount; i++)
                {
                    Collider2D overlap = overlapResults[i];
                    if (overlap == enemyAttackHitbox)
                    {
                        Debug.Log($"{name} attack was blocked by {player.name}.");
                        return;
                    }
                }

                Debug.Log($"{player.name} is guarding with block hitbox active, but it did not overlap the enemy attack hitbox.");
            }
        }

        Health playerHealth = player.GetComponent<Health>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damageAmount);
            Debug.Log($"{name} hit {player.name} for {damageAmount} damage.");
        }
        else
        {
            Debug.LogWarning($"{name} could not damage {player.name} because no Health component was found.");
        }
    }

    private void EndAttack()
    {
        isAttacking = false;
    }


    private void Die()
    {
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        UpdateCollisionBlocks(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        UpdateCollisionBlocks(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        blockedLeft = blockedRight = blockedUp = blockedDown = false;
    }

    private void UpdateCollisionBlocks(Collision2D collision)
    {
        blockedLeft = blockedRight = blockedUp = blockedDown = false;

        foreach (var contact in collision.contacts)
        {
            Vector2 normal = contact.normal;

            if (Mathf.Abs(normal.x) > 0.7f)
            {
                if (normal.x > 0f)
                {
                    blockedLeft = true;
                }
                else
                {
                    blockedRight = true;
                }
            }

            if (Mathf.Abs(normal.y) > 0.7f)
            {
                if (normal.y > 0f)
                {
                    blockedDown = true;
                }
                else
                {
                    blockedUp = true;
                }
            }
        }
    }

    private void ApplyCollisionBlocks(ref Vector2 direction)
    {
        if (blockedLeft && direction.x < 0f)
        {
            direction.x = 0f;
        }

        if (blockedRight && direction.x > 0f)
        {
            direction.x = 0f;
        }

        if (blockedDown && direction.y < 0f)
        {
            direction.y = 0f;
        }

        if (blockedUp && direction.y > 0f)
        {
            direction.y = 0f;
        }
    }

    private bool IsPlayerInAnyHitbox()
    {
        if (player == null)
        {
            return false;
        }

        Collider2D playerCollider = player.GetComponent<Collider2D>();
        if (playerCollider == null)
        {
            return false;
        }

        bool inLeft = leftHitbox != null && leftHitbox.bounds.Contains(playerCollider.bounds.center);
        bool inRight = rightHitbox != null && rightHitbox.bounds.Contains(playerCollider.bounds.center);
        return inLeft || inRight;
    }

    private void UpdateHitboxState()
    {
        if (isAttacking)
        {
            if (leftHitbox != null)
            {
                leftHitbox.enabled = !facingRight;
            }

            if (rightHitbox != null)
            {
                rightHitbox.enabled = facingRight;
            }

            return;
        }

        if (leftHitbox != null)
        {
            leftHitbox.enabled = false;
        }

        if (rightHitbox != null)
        {
            rightHitbox.enabled = false;
        }
    }

    private void UpdateFacing(float direction)
    {
        if (direction > 0f)
        {
            facingRight = true;
        }
        else if (direction < 0f)
        {
            facingRight = false;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !facingRight;
        }
    }

    private void SelectNewPatrolTarget()
    {
        patrolTimer = patrolMoveDuration;
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        float radius = Random.Range(patrolDistance * 0.5f, patrolDistance);
        Vector2 offset = randomDirection * radius;
        patrolTarget = (Vector2)transform.position + offset;
    }
}
