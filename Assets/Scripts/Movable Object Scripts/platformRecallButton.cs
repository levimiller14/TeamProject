using UnityEngine;

public class platformRecallButton : MonoBehaviour
{
    [SerializeField] GameObject connectedObject;

    movableObject movable;

    float originalDistance;
    float currentDistance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        movable = connectedObject.GetComponent<movableObject>();
        float firstPositionDistance = Vector3.Distance(transform.position, movable.GetPlatformFirstPosition());
        float secondPositionDistance = Vector3.Distance(transform.position, movable.GetPlatformSecondPosition());
        if(firstPositionDistance < secondPositionDistance)
        {
            originalDistance = firstPositionDistance;
        }
        else
        {
            originalDistance = secondPositionDistance;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.CompareTag("Player") && movable != null)
        {
            currentDistance = Vector3.Distance(transform.position, movable.GetPlatformActivePosition());
            if(currentDistance > originalDistance)
            {
                if (!movable.GetIsMoving() && movable.GetDelayTimer() >= movable.GetDelayAmount())
                {
                    connectedObject.GetComponent<movableObject>().SetIsMoving(true);
                }
            }
        }
    }
}
