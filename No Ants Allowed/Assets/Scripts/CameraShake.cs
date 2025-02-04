using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{

    float duration = 0.15f;
    float magnitude = 0.09f;

    public IEnumerator Shake()
    {
        Vector3 originalPosition = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = originalPosition + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPosition;
    }

    public void TriggerShake()
    {
        StartCoroutine(Shake());
    }
}
