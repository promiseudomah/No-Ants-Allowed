using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class PowerUpManager : MonoBehaviour
{

    public static PowerUpManager Instance;

    [Header("Power-Up Settings")]
    [SerializeField] private float boomCooldown = 10f;
    [SerializeField] private float healCooldown = 8f;
    [SerializeField] private int healAmount = 10;

    [Header("UI Elements")]
    [SerializeField] private Image boomLoader;
    [SerializeField] private Image healLoader;

    private bool boomAvailable;
    private bool healAvailable;

    [Header("VFX")]     
    [SerializeField] private CameraShake cameraShake;
    [SerializeField] private Volume postProcessingVolume; // Reference to global post-processing

    [Header("SFX")] 
    [SerializeField] AudioClip boomClip;
    [SerializeField] AudioClip healClip;

    
    private Vignette vignette;

    EnemySpawner enemySpawner;
    GameManager gameManager;
    AudioSource audioSource;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {

        audioSource = GetComponent<AudioSource>();
        boomLoader.fillAmount = 1f;
        healLoader.fillAmount = 1f;

        if (postProcessingVolume.profile.TryGet(out vignette))
        {
            vignette.intensity.Override(0.125f);
        }

        enemySpawner = EnemySpawner.Instance;
        gameManager = GameManager.Instance;

        gameManager.OnGameStarted.AddListener(InitPowerUps);
    }

    void OnDisable()
    {
        gameManager.OnGameStarted.RemoveListener(InitPowerUps);
    }

    void InitPowerUps(){
        
        StartCoroutine(Cooldown(boomLoader, boomCooldown, () => boomAvailable = true));
        StartCoroutine(Cooldown(healLoader, healCooldown, () => healAvailable = true));
    }

    public void UseBoom()
    {
        if (!boomAvailable) return;

        boomAvailable = false;
        StartCoroutine(ActivateBoom());
        StartCoroutine(Cooldown(boomLoader, boomCooldown, () => boomAvailable = true));
    }

    public void UseHeal()
    {
        if (!healAvailable) return;

        healAvailable = false;
        StartCoroutine(ActivateHeal());
        StartCoroutine(Cooldown(healLoader, healCooldown, () => healAvailable = true));
    }

    private IEnumerator ActivateBoom()
    {

        if (vignette != null)
        {
            vignette.color.Override(Color.black);
            yield return StartCoroutine(ChangeVignetteIntensity(0.25f, 0.2f));
            audioSource.PlayOneShot(boomClip);
            cameraShake.TriggerShake();
            enemySpawner.KillAllAnts();
            yield return StartCoroutine(DefaultVignette());
        }
        
        yield return null;
    }

    private IEnumerator ActivateHeal()
    {
    
        if (vignette != null)
        {
            vignette.color.Override(Color.green);
            yield return StartCoroutine(ChangeVignetteIntensity(0.085f, 0.2f));
            audioSource.PlayOneShot(healClip);
            enemySpawner.AddFoodHealth(healAmount);
            yield return new WaitForSeconds(0.05f);
            yield return StartCoroutine(DefaultVignette());
        }

        yield return null;
    }

    public void PlayDamageVignette(){

        StartCoroutine(ActivateDamageVignette());
    }

    IEnumerator ActivateDamageVignette()
    {
    
        if (vignette != null)
        {
            vignette.color.Override(Color.red);
            yield return StartCoroutine(ChangeVignetteIntensity(0.085f, 0.2f));
            yield return new WaitForSeconds(0.1f);
            yield return StartCoroutine(DefaultVignette());  
        }

        yield return null;
    }

    private IEnumerator Cooldown(Image loaderImage, float cooldownTime, System.Action onCooldownComplete)
    {
        float elapsedTime = cooldownTime;
        while (elapsedTime > 0)
        {
            elapsedTime -= Time.deltaTime;
            loaderImage.fillAmount = elapsedTime / cooldownTime;
            yield return null;
        }
        
        loaderImage.fillAmount = 0f;
        onCooldownComplete?.Invoke();
    }

    private IEnumerator ChangeVignetteIntensity(float targetIntensity, float duration)
    {
        float startIntensity = vignette.intensity.value;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            vignette.intensity.Override(Mathf.Lerp(startIntensity, targetIntensity, elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        vignette.intensity.Override(targetIntensity);
    }

    private IEnumerator DefaultVignette(){

        vignette.color.Override(Color.black);
        yield return StartCoroutine(ChangeVignetteIntensity(0.125f, 0.2f));
        yield return null;
    }


}
