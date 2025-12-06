using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class LightOptimization : MonoBehaviour
{
    public string playerTag = "Player";

    [Header("---Distances---")]
    [SerializeField] float fullQualityDistance = 10f;
    [SerializeField] float lowQualityDistance = 18f;
    [SerializeField] float offDistance = 25f;

    public Light lightSource;
    private Transform player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lightSource = GetComponent<Light>();

        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            player = playerObj.transform;
            StartCoroutine(DistanceCheckRoutine());
        }
    }

    IEnumerator DistanceCheckRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.25f);

        while (true)
        {
            if (player == null)
            {
                yield break;
            }

            float dist = Vector3.Distance(player.position, transform.position);

            if (dist <= fullQualityDistance)
            {
                lightSource.enabled = true;
                lightSource.shadows = LightShadows.Soft;
            }
            else if (dist <= lowQualityDistance)
            {
                lightSource.enabled = true;
                lightSource.shadows = LightShadows.None;
            }
            else if (dist > offDistance)
            {
                lightSource.enabled = false;
            }

            yield return wait;
        }
    }

}
