using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Variables")]
    public Food m_FoodTarget;
    public float m_EnemySpeed;

    [Space]
    [SerializeField] Enemy m_EnemyPrefab;

    [Header("Spawner Controls")]
    [SerializeField] float m_SpawnRadius;
    [SerializeField] float m_SpawnDelay;
    [SerializeField] float m_DelayCountDown;

    void Awake()
    {
        
        m_DelayCountDown = m_SpawnDelay;
    }

    void Update(){

        m_DelayCountDown = m_DelayCountDown - Time.deltaTime;
        
        if(m_DelayCountDown <= 0){

            SpawnEnemy();
            m_DelayCountDown = m_SpawnDelay;
        }
    }

    void SpawnEnemy(){
        
        Enemy enemy = Instantiate(m_EnemyPrefab, 
            new Vector3(Random.insideUnitSphere.x, 0f, Random.insideUnitSphere.z) * m_SpawnRadius,  
                Quaternion.identity);
        
        enemy.m_FoodTarget = m_FoodTarget;
        enemy.m_Speed = m_EnemySpeed;
        enemy.transform.SetParent(transform);

    }
}
