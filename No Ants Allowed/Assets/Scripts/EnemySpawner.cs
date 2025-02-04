using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(AudioSource))]
public class EnemySpawner : MonoBehaviour
{

    public static EnemySpawner Instance;

    [Header("Enemy Variables")]
    public Food[] m_FoodTargets;
    [SerializeField] float m_FoodHealth;

    [Space]
    [Header("Enemy Types")]
    [SerializeField] Enemy m_BlackAntPrefab;
    [SerializeField] Enemy m_RedAntPrefab;
    [SerializeField] Enemy m_WorkerAntPrefab;
    [SerializeField] bool m_RedAntsAllowed;
    [SerializeField] bool m_WorkerAntsAllowed;
    [SerializeField] bool m_RandomBetweenRedAndWorker;


    [Header("Spawner Controls")]
    public int m_TotalWaves;
    public int m_CurrentWave;
    [SerializeField] float m_SpawnRadius;
    [SerializeField] float m_SpawnDelay;
    [SerializeField] int m_EnemiesPerWave;
    [SerializeField] float m_SpawnDelayModifierPerWave;
    [SerializeField] float m_EnemySpeedModifier;
    [SerializeField] int m_EnemyCountIncreaseModifier;

    [Header("Audio Choices")]
    [SerializeField] AudioClip[] antAudioClips;
    [SerializeField] AudioSource audioSourceForAntBackground;
    
    private int m_EnemiesSpawned = 0;
    public int m_EnemiesRemaining = 0;


    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start(){

        foreach (var food in m_FoodTargets)
        {
            food.m_Health = m_FoodHealth;
        }
    }

    public void StartGame()
    {
        
        m_EnemiesSpawned = 0;
        m_EnemiesRemaining = GetEnemiesForWave();
        StartCoroutine(SpawnWaves());
    }

    Enemy ChooseEnemyType()
    {
        float rand = Random.value;

        if (rand > 0.65f && m_WorkerAntsAllowed) return m_WorkerAntPrefab;
        if (rand > 0.35f && m_RedAntsAllowed) return m_RedAntPrefab;
        
        if (m_RandomBetweenRedAndWorker && m_CurrentWave < m_TotalWaves - 1)
        {
            return (Random.value > 0.5f) ? m_RedAntPrefab : m_WorkerAntPrefab;
        }

        return m_BlackAntPrefab;

    }

    void SpawnEnemy()
    {
        if (m_CurrentWave >= m_TotalWaves)
        {
            GameManager.Instance.GameOver(true);
            return;
        }

        Enemy enemyToSpawn = ChooseEnemyType();
        
        CustomSpawn(enemyToSpawn);
    }

    void CustomSpawn(Enemy enemyToSpawn){

        Vector3 spawnPosition = transform.position + new Vector3(
            Random.onUnitSphere.x * m_SpawnRadius,
            0f,
            Random.onUnitSphere.z * m_SpawnRadius
        );

        Enemy enemy = Instantiate(enemyToSpawn, spawnPosition, Quaternion.identity);
        enemy.m_FoodTarget = m_FoodTargets[Random.Range(0, m_FoodTargets.Length)];
        enemy.transform.SetParent(transform);
        enemy.m_Speed *= m_EnemySpeedModifier;

        AudioClip clip = antAudioClips[Random.Range(0, antAudioClips.Length)];
        enemy.audioSource.PlayOneShot(clip);

        PlayaudioSourceForAntBackground();

        m_EnemiesSpawned++;
        GameManager.Instance.TrackEnemySpawn();
    }

    IEnumerator SpawnWaves()
    {   
        for (int i = 0; i < GetEnemiesForWave(); i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(m_SpawnDelay);
        }    
    }

    public void OnEnemyKilled()
    { 
        if (m_CurrentWave >= m_TotalWaves)
        {
            GameManager.Instance.GameOver(true);
            return;
        }

        m_EnemiesRemaining--;
        GameManager.Instance.TrackEnemyKill();

        if (m_EnemiesRemaining <= 0 && !GameManager.Instance.isGameOver)
        {
           NextWave();
        }
    }

    public void OnEnemyEscaped()
    {
        if (m_CurrentWave >= m_TotalWaves)
        {
            GameManager.Instance.GameOver(true);
            return;
        }

        m_EnemiesRemaining--;
        GameManager.Instance.TrackEnemyEscape();

        if (m_EnemiesRemaining <= 0 && !GameManager.Instance.isGameOver)
        {
           NextWave();
        }
    }

    void NextWave(){

        m_CurrentWave++;
        m_SpawnDelay -= m_SpawnDelayModifierPerWave;

        if (m_CurrentWave >= m_TotalWaves)
        {
            GameManager.Instance.GameOver(true);
            return;
        }

        GameManager.Instance.NextWave();  
    }

    public int GetEnemiesForWave()
    {
        if (m_CurrentWave >= m_TotalWaves)
        {
            return 0;
        }
        
        return m_EnemiesPerWave + m_CurrentWave * m_EnemyCountIncreaseModifier;
    }

    public void KillAllAnts(){

        foreach (var item in transform.GetComponentsInChildren<Enemy>())
        {
            if(item.canMove && item.canBeKilled){

                item.ExecuteDeath();
            }
            
        }
    }

    public void AddFoodHealth(float value){

        Food lowestHealthFood = null;

        foreach (var food in m_FoodTargets)
        {
            if (lowestHealthFood == null || food.m_Health < lowestHealthFood.m_Health)
            {
                lowestHealthFood = food;
            }
        }

        if (lowestHealthFood != null)
        {
            lowestHealthFood.AddHealth(value);
        }
    }

    void PlayaudioSourceForAntBackground(){

        audioSourceForAntBackground.Play();
    }
}
