using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [ReadOnly]
    public Food m_FoodTarget;
    [SerializeField] float m_Speed;

    void Update(){

        //Vector3.Lerp();

        //See how to lerp and get this to move this object from a to b.
    }
}
