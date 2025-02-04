using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Player : MonoBehaviour
{
    [SerializeField] LayerMask m_EnemyLayer;
    float holdTimeRequired = 0.45f;


    [Space, Header("Custom Cursor & UI")]
    [SerializeField] Image cursorImage;
    [SerializeField] Slider holdBar;
    [SerializeField] AudioClip swatterClip;
    private Vector3 offset = new Vector3(20, -30, 0);

    private float clickStartTime;
    private bool isClicking;
    private Enemy targetEnemy;
    private bool isHolding;

    AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {

        UpdateCursorPosition();

        if (Input.GetMouseButtonDown(0))
        {
            clickStartTime = Time.time;
            isClicking = true;

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 100f, m_EnemyLayer))
            {
                targetEnemy = hit.collider.GetComponent<Enemy>();
                if (targetEnemy != null && targetEnemy.CanDodge() && targetEnemy.canMove)
                {
                    isHolding = true;
                    StartCoroutine(HoldToKill(targetEnemy));
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isClicking = false;

            if (isHolding)
            {
                isHolding = false;

                holdBar.value = 0f;
                holdBar.gameObject.SetActive(false);

                targetEnemy.canMove = true;
                if (targetEnemy != null) targetEnemy.Dodge();
            }
            else
            {
                KillEnemy();
            }
        }
    }

    void KillEnemy()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, 100f, m_EnemyLayer))
        {
            Enemy enemyScript = hit.collider.GetComponent<Enemy>();
            if (enemyScript != null && targetEnemy.canBeKilled)
            {
                audioSource.PlayOneShot(swatterClip);
                enemyScript.Death();
                PlayKillSFX();
            }
        }
    }

    IEnumerator HoldToKill(Enemy enemy)
    {
        float elapsedTime = 0f;

        while (isHolding && elapsedTime < holdTimeRequired)
        {
            enemy.canMove = false;
            elapsedTime += Time.deltaTime;

            holdBar.value = elapsedTime/holdTimeRequired;
            holdBar.gameObject.SetActive(true);
            yield return null;
        }

        if (isHolding)
        {
            audioSource.PlayOneShot(swatterClip);
            enemy.ExecuteDeath();
            isHolding = false;

            holdBar.value = 0f;
            holdBar.gameObject.SetActive(false);

            PlayKillSFX();
        }
    }

    void UpdateCursorPosition()
    {
        if (cursorImage != null)
        {
            Vector3 cursorPos = Input.mousePosition + offset;
            cursorImage.transform.position = cursorPos;
        }
    }

    void PlayKillSFX(){

        audioSource.Play();
    }
}
