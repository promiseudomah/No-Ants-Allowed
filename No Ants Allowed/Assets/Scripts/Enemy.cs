using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    
    public Food m_FoodTarget;
    [Space] 
    public float m_Speed;
    public float m_DamageAmount;
    public float m_DodgeProbability;


    [Space]
    [SerializeField] Color m_EscapeColor;
    [SerializeField] GameObject m_Splash;
   

    [HideInInspector] public bool canMove = true;
    [HideInInspector] public bool canBeKilled = true;
    protected bool hasArrived;
    bool isDodging;
    bool requiresHoldToKill;
    float deathForce = 2f;
    protected Vector3 m_MyPosition;
    protected Vector3 m_RandomEscapeDirection;
    Vector3 m_DodgeDirection;
    Rigidbody[] bodies;
    BoxCollider[] bodyColliders;
    [HideInInspector] public AudioSource audioSource;    

    void Awake(){

        requiresHoldToKill = m_DodgeProbability > 0;

        GenerateRandomEscapeRoute();

        InitBodiesAndColliders();

        audioSource = GetComponent<AudioSource>();
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

        if (isDodging) return;

        float moveStep =  m_Speed * Time.deltaTime;

        m_MyPosition 
            = new Vector3(transform.position.x, 0.044f, transform.position.z);
                
        Vector3 m_FoodTargetPosition 
            = new Vector3(m_FoodTarget.transform.position.x, 0.044f, m_FoodTarget.transform.position.z);

        if(canMove){

            if(!hasArrived){

                MoveTowardsTarget(moveStep, m_FoodTargetPosition);
            }

            else{

                MoveTowardsRandomTarget(moveStep);
            }
        }
    }

    public virtual void MoveTowardsTarget(float moveStep, Vector3 target){

        Vector3 targetDirection = target - m_MyPosition;
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, moveStep * 5, 0f);
        Vector3 newPosition = Vector3.MoveTowards(m_MyPosition, target, moveStep);
        
        transform.position = newPosition;

        transform.rotation = Quaternion.LookRotation(newDirection);

        if (Vector3.Distance(m_MyPosition, target) < 0.001f && canBeKilled)
        {
            
            hasArrived = true; 
            canBeKilled = false;

            DamageFood();
            ChangeBodyColor(m_EscapeColor);  
        }
    }

    public virtual void MoveTowardsRandomTarget(float moveStep){

        MoveTowardsTarget(moveStep * 3, m_RandomEscapeDirection);

        if (Vector3.Distance(m_MyPosition, m_RandomEscapeDirection) < 2f)
        {
            DelayedDestroy();
        }
    }

    void DamageFood(){

        m_FoodTarget.ReduceHealth(m_DamageAmount);
        PowerUpManager.Instance.PlayDamageVignette();
        EnemySpawner.Instance.OnEnemyEscaped();
    }
    
    protected void GenerateRandomEscapeRoute(){

        float ran = Random.value;

        if(ran > 0.5){

            m_RandomEscapeDirection = new Vector3(Random.Range(-20,-10), 0, Random.Range(20,10));
        }
        else{

            m_RandomEscapeDirection = new Vector3(Random.Range(20,10), 0, Random.Range(-20,-10));
        }
    }

    public bool CanDodge()
    {
        return requiresHoldToKill;
    }

    public void Death(){

        if(!canBeKilled) return;

        if (Random.value < m_DodgeProbability)
        {
            Dodge();
            return;
        }
        
        ExecuteDeath();
    }

    public void ExecuteDeath()
    {

        canMove = false;
        canBeKilled = false;
        ChangeBodyColor(Color.grey);
        m_Splash.SetActive(true);

        foreach (var item in bodies)
        {
            item.isKinematic = false;
            item.transform.SetParent(null);
            item.AddForce(new Vector3(
                Random.Range(-deathForce, deathForce), 
                0, 
                Random.Range(-deathForce, -deathForce)), 
                ForceMode.Impulse);
        }

        EnemySpawner.Instance.OnEnemyKilled();

        Invoke(nameof(DelayedDestroy), 0.5f);
    }

    void OnMouseEnter()
    {
        if (Random.value < m_DodgeProbability && canBeKilled)
        {
            //Dodge();
        }
    }

    public void Dodge()
    {
        if (isDodging) return;

        isDodging = true;

        m_DodgeDirection = new Vector3(Random.Range(-0.75f, 0.75f), 0.044f, Random.Range(-0.75f, 0.75f));

        StartCoroutine(DodgeMovement());
    }
    
    IEnumerator DodgeMovement()
    {
        float dodgeTime = 0.25f;
        float elapsedTime = 0f;
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + m_DodgeDirection;

        ChangeAlpha(0f);

        while (elapsedTime < dodgeTime)
        {
            transform.position = 
                Vector3.Lerp(startPosition, targetPosition, elapsedTime / dodgeTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        ChangeAlpha(1f);

        isDodging = false;
    }

    void DelayedDestroy(){

        foreach (var item in bodyColliders)
        {   
            item.enabled = false;
            Destroy(item.gameObject, 0.5f);
        }
    }

    void ChangeBodyColor(Color intendedColor){

        MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();

        foreach (var renderer in meshRenderers)
        {
            Material matInstance = new Material(renderer.material);
            matInstance.color = intendedColor;
            renderer.material = matInstance;
        } 
    }

    void ChangeAlpha(float alpha)
    {
        MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();

        foreach (var renderer in meshRenderers)
        {
            foreach (Material mat in renderer.materials)
            {
                Color newColor = mat.color;
                newColor.a = alpha;
                mat.color = newColor;
            }
        }
    }
}
