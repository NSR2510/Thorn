using UnityEngine;

public class Spider_AI : Enemy_AI
{
    [Header("Spider Stats")]
    [SerializeField] private float maxHealth = 50f;
    private float currentHealth;

    [Header("Spider Combat")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 2f;
    private float lastAttackTime;

    private Animator animator;

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
    }

    protected override void Update()
    {
        if (isStaggered) return;

        base.Update();

        if (isChasing && player != null)
        {
            float distance = Vector2.Distance(transform.position, player.position);
            if (distance <= attackRange && Time.time >= lastAttackTime + attackCooldown)
            {
                PerformAttack();
            }
        }

        UpdateAnimations();
    }

    private void PerformAttack()
    {
        lastAttackTime = Time.time;
        if (animator != null) animator.SetTrigger("Attack");
        Debug.Log("Spider Attacks!");
    }

    public override void HandleDamage(Vector2 knockbackDirection)
    {
        currentHealth -= 10f; 
        
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // We skip animator.SetTrigger("Hit") as it doesn't exist. 
            // Parent's HandleDamage handles the animator pause.
            base.HandleDamage(knockbackDirection);
        }
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;
        
        // Use "Run" if moving, otherwise let it default to Idle/Attack
        animator.SetBool("IsWalking", isChasing || Vector2.Distance(transform.position, patrolTarget) > 0.2f);
    }

    private void Die()
    {
        // No death animation, so we just destroy the object
        Destroy(gameObject);
    }
}
