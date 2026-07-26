using UnityEngine;

public class ElementalEffect : MonoBehaviour
{
    private PlayerElementalManager.ElementType elementType;
    private float damagePerSecond;
    private float duration;
    private float timer;
    private float tickTimer;
    private bool canSpread;

    private EnemyHealth enemyHealth;
    private Boss_HP bossHealth;
    private GameObject indicatorObj;

    public void Configure(PlayerElementalManager.ElementType type, float dps, float durationSec, bool shouldSpread)
    {
        elementType = type;
        damagePerSecond = dps;
        duration = durationSec;
        timer = durationSec;
        canSpread = shouldSpread;
    }

    private void Start()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        bossHealth = GetComponent<Boss_HP>();

        // 1. Instantiating Visual Indicator
        SpawnIndicator();

        // 2. Spread behavior if enabled
        if (canSpread)
        {
            SpreadToRadius();
        }

        tickTimer = 1.0f; // Deal first tick after 1s
    }

    private void Update()
    {
        if (IsHostDead())
        {
            DestroyEffect();
            return;
        }

        // Update timer
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            DestroyEffect();
            return;
        }

        // Deal DPS damage every second
        tickTimer -= Time.deltaTime;
        if (tickTimer <= 0f)
        {
            ApplyTickDamage();
            tickTimer = 1.0f;
        }

        // Keep indicator positioned on top of head
        if (indicatorObj != null)
        {
            float yOffset = (bossHealth != null) ? 2.5f : 1.0f;
            indicatorObj.transform.position = transform.position + Vector3.up * yOffset;
        }
    }

    private void OnDestroy()
    {
        if (indicatorObj != null)
        {
            Destroy(indicatorObj);
        }
    }

    public void RefreshDuration()
    {
        timer = duration;
    }

    private bool IsHostDead()
    {
        if (enemyHealth != null && enemyHealth.IsDead) return true;
        if (bossHealth != null && bossHealth.IsDead) return true;
        return false;
    }

    private void ApplyTickDamage()
    {
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damagePerSecond);
        }
        else if (bossHealth != null)
        {
            bossHealth.TakeDamage(damagePerSecond);
        }
    }

    private void SpawnIndicator()
    {
        PlayerElementalManager manager = PlayerElementalManager.Instance;
        if (manager == null) return;

        RuntimeAnimatorController controller = null;
        string indicatorName = "";

        switch (elementType)
        {
            case PlayerElementalManager.ElementType.Poison:
                controller = manager.PoisonController;
                indicatorName = "PoisonIndicator";
                break;
            case PlayerElementalManager.ElementType.Dark:
                controller = manager.DarkController;
                indicatorName = "DarkIndicator";
                break;
            case PlayerElementalManager.ElementType.Lightning:
                controller = manager.LightningController;
                indicatorName = "LightningIndicator";
                break;
        }

        if (controller == null) return;

        // Create indicator GameObject with SpriteRenderer & Animator
        indicatorObj = new GameObject(indicatorName);
        SpriteRenderer sr = indicatorObj.AddComponent<SpriteRenderer>();
        
        // Ensure proper sorting layer so it displays on top of enemies
        sr.sortingOrder = 10;

        Animator anim = indicatorObj.AddComponent<Animator>();
        anim.runtimeAnimatorController = controller;

        // Position on top of head
        float yOffset = (bossHealth != null) ? 2.5f : 1.0f;
        indicatorObj.transform.position = transform.position + Vector3.up * yOffset;

        // Scale visual indicator appropriately so it looks good (e.g. standard size of 1f, or adjusted if needed)
        indicatorObj.transform.localScale = Vector3.one;
    }

    private void SpreadToRadius()
    {
        float spreadRadius = 3.0f;
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, spreadRadius);

        foreach (Collider2D col in hitColliders)
        {
            if (col == null || col.gameObject == gameObject) continue;

            // Check if it's an enemy
            EnemyHealth otherEnemy = col.GetComponentInParent<EnemyHealth>();
            Boss_HP otherBoss = col.GetComponentInParent<Boss_HP>();

            GameObject targetObj = null;
            if (otherEnemy != null) targetObj = otherEnemy.gameObject;
            else if (otherBoss != null) targetObj = otherBoss.gameObject;

            if (targetObj != null && targetObj != gameObject)
            {
                // Spread the status (spread-spawned effects do not spread further to prevent infinite loops)
                PlayerElementalManager.Instance.TryApplyElemental(targetObj, isDirectHit: false);
            }
        }
    }

    private void DestroyEffect()
    {
        Destroy(this);
    }
}
