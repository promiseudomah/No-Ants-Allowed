using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [HideInInspector] public Food m_FoodTarget;
    [HideInInspector] public float m_Speed;

    bool hasArrived;
    Vector3 m_MyPosition;
    Vector3 m_RandomEscapeDirection;
    float deathForce = 3f;
    Rigidbody[] bodies;
    BoxCollider[] bodyColliders;

    void Awake(){

        GenerateRandomEscapeRoute();

        InitBodiesAndColliders();
    }

    void InitBodiesAndColliders(){

        bodies = GetComponentsInChildren<Rigidbody>();
        bodyColliders = GetComponentsInChildren<BoxCollider>();

        foreach (var item in bodies)
        {
            item.isKinematic = true;
        }
    }

    void Update()
    {

        float moveStep =  m_Speed * Time.deltaTime;

        m_MyPosition 
            = transform.position;
            
        Vector3 m_FoodTargetPosition 
            = new Vector3(m_FoodTarget.transform.position.x, 0.044f, m_FoodTarget.transform.position.z);

        if(!hasArrived){

            MoveTowardsTarget(moveStep, m_FoodTargetPosition);
        }

        else{

            MoveTowardsRandomTarget(moveStep);
        }   
    }

    void OnMouseDown(){

        Death();
    }

    void MoveTowardsTarget(float moveStep, Vector3 target){

        Vector3 targetDirection = target - m_MyPosition;

        Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, moveStep * 5, 0f);

        Vector3 newPosition = Vector3.MoveTowards(m_MyPosition, target, moveStep);
        
        transform.position = newPosition;

        transform.rotation = Quaternion.LookRotation(newDirection);

        if (Vector3.Distance(m_MyPosition, target) < 0.001f)
        {
            hasArrived = true; 
        }
    }

    void MoveTowardsRandomTarget(float moveStep){

        MoveTowardsTarget(moveStep * 10, m_RandomEscapeDirection);

        if (Vector3.Distance(m_MyPosition, m_RandomEscapeDirection) < 0.001f)
        {
            Destroy(gameObject, 0.75f);
        }
    }

    void GenerateRandomEscapeRoute(){

        float ran = Random.value;

        if(ran > 0.5){
            m_RandomEscapeDirection = new Vector3(Random.Range(-20,-10), 0, Random.Range(20,10));
        }
        else{
            m_RandomEscapeDirection = new Vector3(Random.Range(20,10), 0, Random.Range(-20,-10));
        }
    }

    void Death(){

        foreach (var item in bodies)
        {   
            item.isKinematic = false;
            item.transform.SetParent(null);
            item.AddForce(new Vector3
                (Random.Range(-deathForce, deathForce), deathForce, Random.Range(deathForce, -deathForce)), 
                    ForceMode.Impulse);

            Invoke(nameof(DelayedDestroy), 1f);
        }
    }

    void DelayedDestroy(){

        foreach (var item in bodyColliders)
        {   
            item.enabled = false;
        }

        Destroy(gameObject, 1);
    }


}
