using System.Collections;
using UnityEngine;

public class customTriggerObject : MonoBehaviour
{
    [SerializeField] GameObject connectedObject;

    movableObject movable;

    private void Start()
    {
        movable = connectedObject.GetComponent<movableObject>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if(other.gameObject.transform.parent != transform.parent)
            {
                other.gameObject.transform.parent = transform.parent;
            }  
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.CompareTag("Player") && movable != null)
        {
            if (!movable.GetIsMoving() && movable.GetDelayTimer() >= movable.GetDelayAmount())
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
