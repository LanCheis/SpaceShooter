using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy")]
    public GameObject enemyPrefab;

    [Header("Grid Layout")]
    public int columns = 8;
    public int rows = 3;

    [Header("Formation Spacing")]
    [Tooltip("Horizontal gap between enemies")]
    public float columnSpacing = 1.5f;
    [Tooltip("Vertical gap between rows")]
    public float rowSpacing = 1.2f;
    [Tooltip("World Y of the formation center when spawned")]
    public float spawnCenterY = 4f;

    [Header("Movement Bounds (match PlayerController)")]
    public float minX = -8f;
    public float maxX = 8f;

    [Header("Wave Settings")]
    public int startCount = 2;
    public int countIncrement = 2;
    public float waveDelayMin = 2f;
    public float waveDelayMax = 3f;
    [Tooltip("Minimum seconds the formation moves in one direction before reversing")]
    public float minDirectionDuration = 0.5f;

    [Header("Difficulty Cap  (wave 1 → maxDifficultyWave)")]
    public int maxDifficultyWave = 20;

    [Header("Formation Move Speed Range")]
    public float moveSpeedMin = 1f;
    public float moveSpeedMax = 5f;

    [Header("Fire Interval Range")]
    public float fireIntervalMinStart = 4f;
    public float fireIntervalMinEnd   = 0.8f;
    public float fireIntervalMaxStart = 8f;
    public float fireIntervalMaxEnd   = 2.5f;

    // Read from a UI script to display HUD info
    public int CurrentWave  { get; private set; }
    public int AliveEnemies { get; private set; }

    // Formation state — owned entirely by the spawner
    private float formationCenterX;
    private float formationHalfWidth;
    private float groupMoveDir = 1f;
    private float lastReversalTime = -999f;
    private float currentMoveSpeed;
    private int   activeColumns;

    // Returns the world X position for a given column index this wave
    public float GetColumnX(int col)
    {
        return formationCenterX + (col - (activeColumns - 1) * 0.5f) * columnSpacing;
    }

    void Start()
    {
        StartCoroutine(SpawnWave());
    }

    void Update()
    {
        if (AliveEnemies == 0) return;

        float newCenter = formationCenterX + groupMoveDir * currentMoveSpeed * Time.deltaTime;
        float rightEdge = newCenter + formationHalfWidth;
        float leftEdge  = newCenter - formationHalfWidth;

        bool canReverse = (Time.time - lastReversalTime) >= minDirectionDuration;

        if (rightEdge >= maxX && canReverse)
        {
            newCenter = maxX - formationHalfWidth;
            groupMoveDir = -1f;
            lastReversalTime = Time.time;
        }
        else if (leftEdge <= minX && canReverse)
        {
            newCenter = minX + formationHalfWidth;
            groupMoveDir = 1f;
            lastReversalTime = Time.time;
        }

        formationCenterX = newCenter;
    }

    IEnumerator SpawnWave()
    {
        CurrentWave++;
        AliveEnemies = 0;

        // Reset formation to center, moving right
        formationCenterX = 0f;
        groupMoveDir     = 1f;
        lastReversalTime = -999f;

        float t = Mathf.Clamp01((float)(CurrentWave - 1) / Mathf.Max(1, maxDifficultyWave - 1));
        currentMoveSpeed = Mathf.Lerp(moveSpeedMin, moveSpeedMax, t);

        int waveCount  = Mathf.Min(columns * rows, startCount + (CurrentWave - 1) * countIncrement);
        activeColumns  = Mathf.Min(columns, waveCount);
        int activeRows = Mathf.CeilToInt((float)waveCount / activeColumns);

        // Half-width of the formation — used for wall detection in Update
        formationHalfWidth = (activeColumns - 1) * 0.5f * columnSpacing;

        int spawned = 0;
        for (int row = 0; row < activeRows && spawned < waveCount; row++)
        {
            // Center the rows vertically around spawnCenterY
            float y = spawnCenterY + (row - (activeRows - 1) * 0.5f) * rowSpacing;

            for (int col = 0; col < activeColumns && spawned < waveCount; col++)
            {
                float x = GetColumnX(col);
                GameObject go = Instantiate(enemyPrefab, new Vector3(x, y, 0f), Quaternion.identity);
                Enemy enemy = go.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.Init(col, this);
                    ApplyWaveScaling(enemy, t);
                }
                AliveEnemies++;
                spawned++;
            }
        }
        yield break;
    }

    public void OnEnemyDied()
    {
        AliveEnemies = Mathf.Max(0, AliveEnemies - 1);
        if (AliveEnemies == 0)
            StartCoroutine(NextWaveDelay());
    }

    IEnumerator NextWaveDelay()
    {
        yield return new WaitForSeconds(Random.Range(waveDelayMin, waveDelayMax));
        StartCoroutine(SpawnWave());
    }

    void ApplyWaveScaling(Enemy enemy, float t)
    {
        enemy.minFireInterval = Mathf.Lerp(fireIntervalMinStart, fireIntervalMinEnd, t);
        enemy.maxFireInterval = Mathf.Lerp(fireIntervalMaxStart, fireIntervalMaxEnd, t);
        // Uncomment to scale health per wave:
        // enemy.maxHealth = 3 + Mathf.RoundToInt(t * 7);
    }
}
