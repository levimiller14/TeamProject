using UnityEngine;

public class launchPad : MonoBehaviour
{
    [Header("----- Launch Settings -----")]
    [SerializeField] public float force = 20f;
    [Range(0f, 1f)]
    [SerializeField] float arcHeight = 0.3f;

    public Vector3 GetLaunchVelocity()
    {
        // combine forward direction with upward arc for that halo man cannon feel
        // arcHeight controls the ratio: 0 = pure forward, 1 = pure up
        Vector3 horizontalDir = transform.forward;
        Vector3 launchDir = Vector3.Lerp(horizontalDir, Vector3.up, arcHeight).normalized;
        return launchDir * force;
    }
}
