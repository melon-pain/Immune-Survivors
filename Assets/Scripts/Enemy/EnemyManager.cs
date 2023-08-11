using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager instance;

    [SerializeField] private List<EnemyPool> enemyPools = new();
    public List<GameObject> activeEnemies = new();

    public int InfectionRate => activeEnemies.FindAll(enemy => enemy.activeInHierarchy).Count;
    [field: Header("Infection")]
    [field: SerializeField]
    public int MinInfectionRate { get; private set; }
    [field: SerializeField]
    public int MaxInfectionRate { get; private set; }

    [SerializeField] private float maxSpawnDistance;

    [field: Header("Wave")]
    [SerializeField] LevelWaveData level;

    [SerializeField] private int waveIndex = 0;
    [SerializeField] private Wave currentWave;

    public System.Action OnMinInfectionReached;
    public System.Action OnMaxInfectionReached;
    public System.Action OnInfectionRateChanged;

    [System.Serializable]
    public class EnemyPool
    {
        public string Name;
        public ObjectPool enemyPool;
    }

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
        {
            Destroy(instance.gameObject);
            instance = this;
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        // Set the current wave with the firt wave in the wavelist
        InitalizeCurrentWave(waveIndex);

        // Calculate the quota of the current wave
        CalculateWaveQuota();

        // Start couroutine for wave and spawning
        StartCoroutine(WaveCoroutine());
        StartCoroutine(BasicEnemySpawnCoroutine());
    }

    private void OnDestroy()
    {
        instance = null;
    }
    private IEnumerator WaveCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(level.waveInterval);

            // Spawn a Boss
            SpawnWaveEnemyBoss();
            // Increment wave number if there is a next wave
            if (waveIndex < level.waveList.Count-1)
            {
                waveIndex++;
                InitalizeCurrentWave(waveIndex);
                CalculateWaveQuota();

            }
            else
            {
                Debug.Log(" Last Wave Ended");
            }

        }

    }
    private IEnumerator BasicEnemySpawnCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(currentWave.spawnInterval);

            int totalEnemyCount = EnemyManager.instance.activeEnemies.Count;

           
            // Spawn enemy of each type if the number of enemies present in below the wave quota
            if (currentWave.waveSpawnCounter < currentWave.waveSpawnQuota && totalEnemyCount < level.maxActiveEnemyThreshold)
            {
                // Spawn each type of enemy
                foreach(EnemyGroup eg in currentWave.enemyGroups)
                {
                    // Spawn until quota is reached
                    if(eg.enemySpawnCounter < eg.enemyQuota)
                    {
                        SpawnEnemyBatch(1, eg.poolName);
                        eg.enemySpawnCounter++;
                        currentWave.waveSpawnCounter++;

                    }
                }
            }
            // Spawn random enemy if the enemies present are more than the wave quota
            else if (currentWave.waveSpawnCounter >= currentWave.waveSpawnQuota && totalEnemyCount < level.maxActiveEnemyThreshold)
            {
                int type = Random.Range(0, currentWave.enemyGroups.Count);

                // spawn this type
                SpawnEnemyBatch(1, currentWave.enemyGroups[type].poolName);
                currentWave.enemyGroups[type].enemySpawnCounter++;
                //currentWave.excessSpawnCounter++;

            }
            // Trigger events when the number of active enemies exceeds the maxEnemyCount
            else if (totalEnemyCount > level.maxActiveEnemyThreshold)
            {
                Debug.Log(" Trigger Special Events: Symptoms?");
                //spawn boss or trigger
            }
        }
    }

    private void SpawnWaveEnemyBoss()
    {
        foreach(BossEnemyGroup eb in currentWave.BossEnemyGroups)
        {
            for (int i = 0; i < eb.count; i++)
            {
                SpawnEnemyBatch(1, eb.poolName);
                Debug.Log(" Boss has been spawned");

            }
        }
    }

    private void SpawnEnemyBatch(int amount, string poolName)
    {
        GameObject player = GameManager.instance.Player;
        for (int i = 0; i < amount; i++)
        {
            // spawn point around the player
            float angle = Random.Range(0f, 360f);
            Vector3 dir = new(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            Vector3 spawnPoint = player.transform.position + dir * Random.Range(30f, 40f);

            // Limit spawn position
            if (spawnPoint.sqrMagnitude > Mathf.Pow(maxSpawnDistance, 2))
            {
                spawnPoint = spawnPoint.normalized * maxSpawnDistance;
            }

            //float x = Random.Range(-50f, 50f);
            //float z = Random.Range(-50f, 50f);

            SpawnEnemy(spawnPoint, poolName);
        }
    }

    public void SpawnEnemy(Vector3 position, string poolName)
    {
        GameObject enemy = RequestFromPool(position, poolName);
//        
        if (!enemy)
        {
            Debug.LogWarning("No enemy found in object pool!");
            return;
        }

        if (enemy.TryGetComponent<Collider>(out Collider cc))
        {
            cc.enabled = true;
        }


        //Debug.Log(position);

        //enemy.transform.position = position;

        activeEnemies.Add(enemy);
        //InfectionRate++;
        OnInfectionRateChanged?.Invoke();

        enemy.GetComponent<Enemy>().OnDeath += delegate 
        { 
            activeEnemies.Remove(enemy);
            //InfectionRate--;
            OnInfectionRateChanged?.Invoke();

            // Win Condition
            if (InfectionRate <= MinInfectionRate)
            {
                OnMinInfectionReached?.Invoke();
            }
        };

        // Lose Condition
        if (InfectionRate >= MaxInfectionRate)
        {
            OnMaxInfectionReached?.Invoke();
        }
    }

    public GameObject RequestFromPool(Vector3 position, string poolName)
    {
        foreach(EnemyPool enpool in enemyPools )
        {
            if (enpool.Name == poolName)
            {
                return enpool.enemyPool.RequestPoolable(position);
            }
        }
        return null;
    }
    private void InitalizeCurrentWave(int waveNum)
    {
        level.waveList[waveNum].waveSpawnCounter = 0;
        foreach (EnemyGroup eg in level.waveList[waveNum].enemyGroups)
        {
            eg.enemySpawnCounter = 0;
        }
        currentWave = level.waveList[waveNum];
    }
    private void CalculateWaveQuota()
    {
        
        int currentWaveQuota = 0;

        // Current wave quota  = accumulative sum of the enemy groups in the current wave
        foreach (EnemyGroup eg in currentWave.enemyGroups)
        {
            currentWaveQuota += eg.enemyQuota;
        }

        currentWave.waveSpawnQuota = currentWaveQuota;
        Debug.Log("Wave " + currentWave.waveName + " Quota:" + currentWaveQuota);
    }
    public GameObject GetNearestEnemy(Vector3 position, float limit = float.MaxValue)
    {
        if (activeEnemies.Count < 1)
        {
            return null;
        }

        float distance;
        
        GameObject nearest = null;
        distance = limit;

        foreach (var unit in activeEnemies)
        {
            bool isDead = unit.GetComponent<Enemy>().IsDead;
            // Skip if unit is inactive or is dead
            if (!unit.activeInHierarchy || isDead)
                continue;

            float dist = Vector3.Distance(unit.transform.position, position);
            if (dist < distance)
            {
                nearest = unit;
                distance = dist;
            }
        }

        return nearest;
    }

    public GameObject GetFurthestEnemy(Vector3 position, float limit = float.MaxValue)
    {
        if (activeEnemies.Count < 1)
        {
            return null;
        }

        float distance;

        GameObject furthest = null;
        distance = 0f;

        foreach (var unit in activeEnemies)
        {
            bool isDead = unit.GetComponent<Enemy>().IsDead;
            // Skip if unit is inactive or is dead
            if (!unit.activeInHierarchy || isDead)
                continue;

            float dist = Vector3.Distance(unit.transform.position, position);
            if (dist > distance && dist <= limit)
            {
                furthest = unit;
                distance = dist;
            }
        }

        return furthest;
    }
}
