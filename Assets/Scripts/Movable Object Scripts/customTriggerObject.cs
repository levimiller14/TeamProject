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
            if(!movable.GetIsMoving() && movable.GetDelayTimer() >= movable.GetDelayAmount())
            {
                connectedObject.GetComponent<movableObject>().SetIsMoving(true);
            }
        }
    }

    //public void SetConnectedObject(GameObject obj)
    //{
    //    connectedObject = obj;
    //}
}
