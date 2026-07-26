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
    [SerializeField] private int maxEnemiesPerWave = 20;

    [Header("UI Reference")]
    [SerializeField] private WaveUIDisplay uiDisplay;

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
            uiDisplay = Object.FindAnyObjectByType<WaveUIDisplay>();
        }

        playerHealth = Object.FindAnyObjectByType<Health>();

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
                // Linear progression: adds 5 more enemies each wave, capped at maxEnemiesPerWave
                int spawnCount = Mathf.Min(5 * currentWave, maxEnemiesPerWave);
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

        // Bind death listener
        Boss_HP health = boss.GetComponent<Boss_HP>();
        if (health != null)
        {
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

            // Heal player upon wave completion
            if (playerHealth != null)
            {
                bool isBossWave = (currentWave == 6 || currentWave == 12 || currentWave == 18);
                if (isBossWave)
                {
                    playerHealth.Heal(100f);
                }
                else
                {
                    playerHealth.Heal(70f);
                }
            }

            // Cooldown starts after wave is cleared
            StartCoroutine(StartCooldown());
        }
    }

    private IEnumerator StartCooldown()
    {
        yield return new WaitForSeconds(0.5f); // Brief delay for game feel before timer starts
        cooldownTimer = waveCooldown;
        isCooldownActive = true;
    }
}
