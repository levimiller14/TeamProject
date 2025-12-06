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
        float sqrFullQuality = fullQualityDistance * fullQualityDistance;
        float sqrLowQuality = lowQualityDistance * lowQualityDistance;
        float sqrOff = offDistance * offDistance;

        while (true)
        {
            if (player == null)
            {
                yield break;
            }

            float sqrDist = (player.position - transform.position).sqrMagnitude;

            if (sqrDist <= sqrFullQuality)
            {
                lightSource.enabled = true;
                lightSource.shadows = LightShadows.Soft;
            }
            else if (sqrDist <= sqrLowQuality)
            {
                lightSource.enabled = true;
                lightSource.shadows = LightShadows.None;
            }
            else if (sqrDist > sqrOff)
            {
                lightSource.enabled = false;
            }

            yield return wait;
        }
    }

}
