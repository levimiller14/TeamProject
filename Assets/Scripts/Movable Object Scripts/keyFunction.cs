using UnityEngine;

public class keyFunction : MonoBehaviour
{
    [SerializeField] GameObject connectedObject;
    [SerializeField] bool revealKey;
    [SerializeField] bool moveKey;

    movableObject movable;

    float rotateSpeed = 45; // Degrees per second.

    Vector3 rotationAxis = Vector3.right;

    private void OnValidate()
    {
//#if UNITY_EDITOR
//        if (revealKey && connectedObject.activeSelf == true)
//        {
//            connectedObject.SetActive(false);
//        }
//        else if(!revealKey && connectedObject.activeSelf == false)
//        {
//            connectedObject.SetActive(true);
//        }
//#endif
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(moveKey)
        {
            movable = connectedObject.GetComponent<movableObject>();
        }
    }

    void Update()
    {
        transform.Rotate(rotationAxis.normalized * rotateSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            if(moveKey && movable != null)
            {
                if (!movable.GetIsMoving() && movable.GetDelayTimer() >= movable.GetDelayAmount())
                {
                    connectedObject.GetComponent<movableObject>().SetIsMoving(true);
                }
            }
            else if(revealKey)
            {
                connectedObject.SetActive(true);
            }

            Destroy(gameObject);
        }
    }
}
