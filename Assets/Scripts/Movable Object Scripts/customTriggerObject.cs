using System.Collections;
using UnityEngine;

public class customTriggerObject : MonoBehaviour
{
    [SerializeField] GameObject connectedObject;

    movableObject movable;

    private void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        movable = connectedObject.GetComponent<movableObject>();

        if (other.CompareTag("Player") && movable != null)
        {
            if(!transform.CompareTag("PlatformRecallButton") && other.gameObject.transform.parent != transform.parent)
            {
                other.gameObject.transform.parent = transform.parent;
            }

            if(!movable.GetIsMoving() && movable.GetDelayTimer() >= movable.GetDelayAmount())
            {
                connectedObject.GetComponent<movableObject>().SetIsMoving(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && other.gameObject.transform.parent != null)
        {
            other.gameObject.transform.parent = null;
        }
    }
}
