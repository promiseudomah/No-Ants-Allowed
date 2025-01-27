using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public Food m_FoodTarget;

    [SerializeField] Enemy m_EnemyPrefab;
    [SerializeField] float m_SpawnDelay;
    float m_DelayCountDown;

    void Start()
    {
        
        m_DelayCountDown = m_SpawnDelay;
    }

    void Update(){

        m_DelayCountDown =- Time.deltaTime;
        
        if(m_DelayCountDown <= 0){

            SpawnEnemy();
            m_DelayCountDown = m_SpawnDelay;
        }
    }

    void SpawnEnemy(){
        
        Enemy enemy = Instantiate(m_EnemyPrefab, transform);
    }
}
