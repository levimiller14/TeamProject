using UnityEngine;

public class customTriggerObject : MonoBehaviour
{
    [SerializeField] GameObject connectedObject;

    public movableObject moveable;

    private void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        moveable = connectedObject.GetComponent<movableObject>();

        if (other.CompareTag("Player") && moveable != null)
        {
            if(!moveable.GetIsMoving() && moveable.GetDelayTimer() >= moveable.GetDelayAmount())
            {
                connectedObject.GetComponent<movableObject>().SetIsMoving(true);
            }
        }
    }
}
