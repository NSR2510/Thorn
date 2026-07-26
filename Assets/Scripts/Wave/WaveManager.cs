using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject gnomePrefab;
    [SerializeField] private GameObject snakePrefab;
    [SerializeField] private GameObject spiderPrefab;
    [SerializeField] private GameObject bossPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private Vector2 spawnCenter = new Vector2(6.5f, -1.5f);
    [SerializeField] private Vector2 spawnRange = new Vector2(1.0f, 2.0f);

    [Header("Wave Settings")]
    [SerializeField] private float waveCooldown = 10f;
    [SerializeField] private int totalWaves = 18;

    [Header("UI Reference")]
    [SerializeField] private WaveUIDisplay uiDisplay;
    [SerializeField] private BossUpgradeUI bossUpgradeUI;

    [Header("Events")]
    public UnityEngine.Events.UnityEvent onGameWon;

    private int currentWave = 0;
    private int activeEnemiesCount = 0;
    private float cooldownTimer = 0f;
    private bool isCooldownActive = false;
    private bool isWaveInProgress = false;
    private Health playerHealth;

    private void Start()
    {
        if (uiDisplay == null)
        {
            uiDisplay = UnityEngine.Object.FindAnyObjectByType<WaveUIDisplay>();
        }

        if (bossUpgradeUI == null)
        {
            bossUpgradeUI = UnityEngine.Object.FindAnyObjectByType<BossUpgradeUI>();
        }

        playerHealth = UnityEngine.Object.FindAnyObjectByType<Health>();

        // Start the wave system with the cooldown before Wave 1
        StartCoroutine(StartFirstWaveDelayed());
    }

    private IEnumerator StartFirstWaveDelayed()
    {
        // Set wave to 0 initially
        currentWave = 0;
        if (uiDisplay != null)
        {
            uiDisplay.UpdateWave(currentWave);
            uiDisplay.UpdateEnemyCount(0);
        }

        // Start 20s cooldown before Wave 1 starts
        yield return StartCooldown();
    }

    private void Update()
    {
        if (isCooldownActive)
        {
            cooldownTimer -= Time.deltaTime;
            if (uiDisplay != null)
            {
                uiDisplay.UpdateTimer(cooldownTimer, true);
            }

            if (cooldownTimer <= 0f)
            {
                isCooldownActive = false;
                if (uiDisplay != null)
                {
                    uiDisplay.UpdateTimer(0f, false);
                }
                StartNextWave();
            }
        }
    }

    private void StartNextWave()
    {
        currentWave++;
        if (currentWave > totalWaves)
        {
            Debug.Log("Congratulations! All waves completed.");
            if (uiDisplay != null)
            {
                uiDisplay.UpdateEnemyCount(0);
            }
            onGameWon?.Invoke();
            return;
        }

        if (uiDisplay != null)
        {
            uiDisplay.UpdateWave(currentWave);
        }

        SpawnWaveEnemies();
    }

    private void SpawnWaveEnemies()
    {
        isWaveInProgress = true;
        activeEnemiesCount = 0;

        // Determine wave type and enemy count
        bool isBossWave = (currentWave == 6 || currentWave == 12 || currentWave == 18);

        if (isBossWave)
        {
            // Boss spawns alone
            SpawnBoss();
        }
        else
        {
            // Standard wave spawning
            GameObject prefabToSpawn = null;

            if (currentWave >= 1 && currentWave <= 5)
            {
                prefabToSpawn = gnomePrefab;
            }
            else if (currentWave >= 7 && currentWave <= 11)
            {
                prefabToSpawn = snakePrefab;
            }
            else if (currentWave >= 13 && currentWave <= 17)
            {
                prefabToSpawn = spiderPrefab;
            }

            if (prefabToSpawn != null)
            {
                // Calculate wave index relative to the start of the current tier (1 to 5)
                int relativeWaveIndex = currentWave;
                if (currentWave >= 7 && currentWave <= 11)
                {
                    relativeWaveIndex = currentWave - 6;
                }
                else if (currentWave >= 13 && currentWave <= 17)
                {
                    relativeWaveIndex = currentWave - 12;
                }

                // Spawns 5, 10, 15, 20, 25 enemies dynamically per tier
                int spawnCount = 5 * relativeWaveIndex;
                SpawnEnemies(prefabToSpawn, spawnCount);
            }
        }

        if (uiDisplay != null)
        {
            uiDisplay.UpdateEnemyCount(activeEnemiesCount);
        }
    }

    private void SpawnEnemies(GameObject prefab, int count)
    {
        for (int i = 0; i < count; i++)
        {
            float randomX = Random.Range(spawnCenter.x - spawnRange.x, spawnCenter.x + spawnRange.x);
            float randomY = Random.Range(spawnCenter.y - spawnRange.y, spawnCenter.y + spawnRange.y);
            Vector3 spawnPos = new Vector3(randomX, randomY, 0f);

            GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);
            activeEnemiesCount++;

            // Bind death listener
            EnemyHealth health = enemy.GetComponent<EnemyHealth>();
            if (health != null)
            {
                health.onDeath.AddListener(() => OnEnemyKilled());
            }
        }
    }

    private void SpawnBoss()
    {
        Vector3 spawnPos = new Vector3(spawnCenter.x, spawnCenter.y, 0f);
        GameObject boss = Instantiate(bossPrefab, spawnPos, Quaternion.identity);
        activeEnemiesCount = 1;

        // Calculate Boss Max HP dynamically based on round:
        // Round 6: 200 HP
        // Round 12: 300 HP
        // Round 18: 400 HP (+100 HP per boss wave after Round 6)
        float bossMaxHP = 200f;
        if (currentWave >= 6)
        {
            bossMaxHP = 200f + Mathf.RoundToInt((currentWave - 6) / 6f) * 100f;
        }

        // Bind death listener and configure HP
        Boss_HP health = boss.GetComponent<Boss_HP>();
        if (health != null)
        {
            health.SetMaxHealth(bossMaxHP);
            health.onDeath.AddListener(() => OnEnemyKilled());
        }
    }

    private void OnEnemyKilled()
    {
        activeEnemiesCount = Mathf.Max(0, activeEnemiesCount - 1);
        
        if (uiDisplay != null)
        {
            uiDisplay.UpdateEnemyCount(activeEnemiesCount);
        }

        if (activeEnemiesCount <= 0 && isWaveInProgress)
        {
            isWaveInProgress = false;

            bool isBossWave = (currentWave == 6 || currentWave == 12 || currentWave == 18);

            // Heal player upon wave completion
            if (playerHealth != null)
            {
                if (isBossWave)
                {
                    playerHealth.Heal(100f);
                }
                else
                {
                    playerHealth.Heal(70f);
                }

                // Survived round rewards: +10 Max HP, +1 Damage
                playerHealth.IncreaseMaxHealth(10f);
                PlayerCombat combat = playerHealth.GetComponent<PlayerCombat>();
                if (combat != null)
                {
                    combat.IncreaseDamage(1f);
                }
            }

            if (isBossWave)
            {
                if (bossUpgradeUI == null)
                {
                    bossUpgradeUI = UnityEngine.Object.FindAnyObjectByType<BossUpgradeUI>();
                }

                if (bossUpgradeUI != null)
                {
                    bossUpgradeUI.ShowUpgradeScreen(() => {
                        StartCoroutine(StartCooldown());
                    });
                }
                else
                {
                    Debug.LogWarning("WaveManager: BossUpgradeUI not found! Starting cooldown directly.");
                    StartCoroutine(StartCooldown());
                }
            }
            else
            {
                // Cooldown starts after wave is cleared
                StartCoroutine(StartCooldown());
            }
        }
    }

    private IEnumerator StartCooldown()
    {
        yield return new WaitForSeconds(0.5f); // Brief delay for game feel before timer starts
        cooldownTimer = waveCooldown;
        isCooldownActive = true;
    }

    /// <summary>
    /// Starts the Special Infinite cascade mode (Wave 99).
    /// </summary>
    public void StartSpecialMode()
    {
        StopAllCoroutines();
        isCooldownActive = false;
        isWaveInProgress = true;
        currentWave = 99;

        if (uiDisplay != null)
        {
            uiDisplay.UpdateWave(99);
        }

        activeEnemiesCount = 0;
        
        // Spawn the first 1 HP gnome in the center of the arena
        Vector3 spawnPos = new Vector3(spawnCenter.x, spawnCenter.y, 0f);
        SpawnSpecialGnome(spawnPos);
        activeEnemiesCount = 1;

        if (uiDisplay != null)
        {
            uiDisplay.UpdateEnemyCount(activeEnemiesCount);
        }

        Debug.Log("Special Infinite Mode (Wave 99) Started!");
    }

    private void SpawnSpecialGnome(Vector3 pos)
    {
        GameObject enemy = Instantiate(gnomePrefab, pos, Quaternion.identity);
        
        // Set HP to 1 dynamically
        EnemyHealth health = enemy.GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.SetMaxHealth(1f);
            health.onDeath.AddListener(() => OnSpecialGnomeKilled(enemy.transform.position));
        }

        // Apply a cool visual effect so they stand out as Special Gnomes
        // Tint them dark purple/magenta and shrink their size
        SpriteRenderer sr = enemy.GetComponent<SpriteRenderer>();
        if (sr == null) sr = enemy.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = new Color(0.4f, 0.15f, 0.6f, 1f); // Cool neon dark purple
        }

        // Shrink them to 75% scale to look like swarm minions
        enemy.transform.localScale = enemy.transform.localScale * 0.75f;
    }

    private void OnSpecialGnomeKilled(Vector3 deathPos)
    {
        activeEnemiesCount = Mathf.Max(0, activeEnemiesCount - 1);

        // Spawn 2 new special 1 HP gnomes near the death position with a small offset
        float offsetRange = 0.6f;
        for (int i = 0; i < 2; i++)
        {
            float randomX = Random.Range(-offsetRange, offsetRange);
            float randomY = Random.Range(-offsetRange, offsetRange);
            Vector3 spawnPos = deathPos + new Vector3(randomX, randomY, 0f);

            SpawnSpecialGnome(spawnPos);
            activeEnemiesCount++;
        }

        if (uiDisplay != null)
        {
            uiDisplay.UpdateEnemyCount(activeEnemiesCount);
        }
    }
}
