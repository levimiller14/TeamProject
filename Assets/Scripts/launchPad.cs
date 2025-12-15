using JetBrains.Annotations;
using UnityEngine;

public class launchPad : MonoBehaviour
{

    [SerializeField] float force;

    private void OnTriggerEnter(Collider other)
    {
        playerController player = other.GetComponent<playerController>();

        if (player != null)
        {
            player.Launch(transform.forward, force);
        }
    }

}
