using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class Food : MonoBehaviour
{
    
    public float m_Health;
    [SerializeField] Slider m_HealthBar;
    float m_MAXHealth;
    BoxCollider foodCollider;
    AudioSource audioSource;

    void Start()
    {
        m_MAXHealth = m_Health;

        foodCollider = GetComponent<BoxCollider>();
        audioSource = GetComponent<AudioSource>();

        UpdateHealthBar();
    }

    public void ReduceHealth(float damageAmount){

        audioSource.Play();

        m_Health = Mathf.Clamp(m_Health - damageAmount, 0, m_MAXHealth);
        UpdateHealthBar();

        if(m_Health <= 0.0001){
            
            ChangeBodyColor(Color.gray);
            foodCollider.enabled = false;
            GameManager.Instance.GameOverImmediate(false);
        }
    }

    public void AddHealth(float amount){

        m_Health = Mathf.Clamp(m_Health + amount, 0, m_MAXHealth);
        UpdateHealthBar();
    }

    void UpdateHealthBar(){

        m_HealthBar.value = m_Health/m_MAXHealth;
    }

    public float GetMaxHealth(){
        
        return m_MAXHealth;
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

}
