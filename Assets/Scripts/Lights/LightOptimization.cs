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

    [Header("---Occlusion---")]
    [SerializeField] LayerMask obstacleMask;
    [SerializeField] float playerEyeHeight = 1.7f;

    public Light lightSource;
    private Transform player;
    private LightShadows originalShadowMode;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lightSource = GetComponent<Light>();
        originalShadowMode = lightSource.shadows;

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
        float sqrOff = offDistance * offDistance;

        while (true)
        {
            if (player == null || lightSource == null)
            {
                yield break;
            }

            Vector3 origin = player.position + Vector3.up * playerEyeHeight;
            Vector3 toLight = transform.position - origin;
            float sqrDist = toLight.sqrMagnitude;

            // beyond max distance - disable
            if (sqrDist > sqrOff)
            {
                lightSource.enabled = false;
                yield return wait;
                continue;
            }

            // within range - check occlusion
            float dist = Mathf.Sqrt(sqrDist);
            Ray ray = new Ray(origin, toLight.normalized);
            bool occluded = Physics.Raycast(ray, dist, obstacleMask, QueryTriggerInteraction.Ignore);

            lightSource.enabled = !occluded;

            yield return wait;
        }
    }

}
