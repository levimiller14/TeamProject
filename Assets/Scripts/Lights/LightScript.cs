using System.Collections;
using UnityEngine;

public class LightScript : MonoBehaviour
{
    public Light lightSource;
    public float minDelay = 0.5f;
    public float maxDelay = 2f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(lightSource == null)
        {
            lightSource = GetComponent<Light>();
        }

        StartCoroutine(FlickerRoutine());
    }

    IEnumerator FlickerRoutine()
    {
        while (true)
        {
            if (lightSource == null)
            {
                yield break;
            }

            lightSource.enabled = !lightSource.enabled;

            float delay = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(delay);
        }
    }
}
