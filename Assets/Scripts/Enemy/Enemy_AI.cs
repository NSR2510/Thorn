using UnityEngine;
using System.Collections;

public abstract class Enemy_AI : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] protected float detectionRange = 5f;
    [SerializeField] protected float patrolRadius = 3f;
    [SerializeField] protected float patrolSpeed = 2f;
    [SerializeField] protected float chaseSpeed = 4f;

    [Header("Stagger & Knockback")]
    [SerializeField] protected float staggerDuration = 0.5f;
    [SerializeField] protected float knockbackForce = 5f;

    protected Transform player;
    protected Rigidbody2D rb;
    protected SpriteRenderer spriteRenderer;
    protected Vector2 patrolTarget;
    protected bool isStaggered = false;
    protected bool isChasing = false;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (rb == null)
        {
            Debug.LogError($"Enemy_AI: Rigidbody2D missing on {gameObject.name}! Please attach one or remove this script.");
        }
    }

    protected virtual void Start()
    {
        SetNewPatrolTarget();
    }

    protected virtual void Update()
    {
        if (isStaggered) return;

        float distanceToPlayer = player != null ? Vector2.Distance(transform.position, player.position) : float.MaxValue;

        if (distanceToPlayer <= detectionRange)
        {
            isChasing = true;
        }
        else
        {
            isChasing = false;
        }
    }

    protected virtual void FixedUpdate()
    {
        if (isStaggered || rb == null) return;

        if (isChasing)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    protected virtual void Patrol()
    {
        float distanceToTarget = Vector2.Distance(transform.position, patrolTarget);
        
        if (distanceToTarget < 0.2f)
        {
            rb.linearVelocity = Vector2.zero;
            SetNewPatrolTarget();
            return;
        }

        Vector2 direction = (patrolTarget - (Vector2)transform.position).normalized;
        FlipSprite(direction.x);
        rb.linearVelocity = direction * patrolSpeed;
    }

    protected virtual void ChasePlayer()
    {
        if (player == null) 
        {
            isChasing = false;
            return;
        }

        Vector2 direction = (player.position - transform.position).normalized;
        FlipSprite(direction.x);
        rb.linearVelocity = direction * chaseSpeed;
    }

    protected void FlipSprite(float moveX)
    {
        if (spriteRenderer == null) return;
        if (moveX > 0.01f) spriteRenderer.flipX = false;
        else if (moveX < -0.01f) spriteRenderer.flipX = true;
    }

    protected void SetNewPatrolTarget()
    {
        patrolTarget = (Vector2)transform.position + Random.insideUnitCircle * patrolRadius;
    }

    public virtual void HandleDamage(Vector2 knockbackDirection)
    {
        if (isStaggered) return;
        StartCoroutine(StaggerRoutine(knockbackDirection));
    }

    protected IEnumerator StaggerRoutine(Vector2 direction) 
    {
        isStaggered = true;
        
        // Pause animation to simulate "Hit" since no hit animation exists
        Animator anim = GetComponent<Animator>();
        if (anim != null) anim.speed = 0f;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
        }

        yield return new WaitForSeconds(staggerDuration);

        // Resume animation
        if (anim != null) anim.speed = 1f;

        if (rb != null) rb.linearVelocity = Vector2.zero;
        isStaggered = false;
    }
}
