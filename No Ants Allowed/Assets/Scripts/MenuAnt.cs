using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class MenuAnt : Enemy
{

    void Awake(){

        GenerateRandomEscapeRoute();
    }

    void Update()
    {

        float moveStep =  m_Speed * Time.deltaTime;

        m_MyPosition 
            = new Vector3(transform.position.x, 0.044f, transform.position.z);

        if(!hasArrived){

            MoveTowardsTarget(moveStep * 2, m_RandomEscapeDirection);
        }

        else{

            GenerateRandomEscapeRoute();
            hasArrived = false;
        }
    }

    public override void MoveTowardsTarget(float moveStep, Vector3 target){

        Vector3 targetDirection = target - m_MyPosition;
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, moveStep * 5, 0f);
        Vector3 newPosition = Vector3.MoveTowards(m_MyPosition, target, moveStep);
        
        transform.position = newPosition;

        transform.rotation = Quaternion.LookRotation(newDirection);

        if (Vector3.Distance(m_MyPosition, target) < 2f)
        {
            
            hasArrived = true; 
        }
    }
}
