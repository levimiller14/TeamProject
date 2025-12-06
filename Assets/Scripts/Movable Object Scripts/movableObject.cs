using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Diagnostics;

public class movableObject : MonoBehaviour
{
    [Header("----- Settings -----")]
    [SerializeField] Transform secondPosition;
    [SerializeField] GameObject objectToMove;
    [SerializeField] float movementTime;
    [SerializeField] bool isReversible;
    [SerializeField] float delayAmount;
    //[SerializeField] bool isPlatform;
    //[SerializeField] bool isDoor;

    GameObject recallButton1;
    GameObject recallButton2;
    
    bool atOppositeEnd;
    bool isMoving;

    Vector3 firstPosition;
    Vector3 direction;
    Vector3 targetPosition;

    float delayTimer;
    float speed;

    private void OnValidate()
    {     
        //if(isPlatform && recallButton1 == null || recallButton2 == null)
        //{
        //    if (recallButton1 == null)
        //    {
        //        GameObject newButton = new GameObject("Platform Recall Button");
        //        if (newButton.CompareTag("PlatformRecallButton"))
        //        {
        //            newButton.transform.parent = this.transform;
        //            newButton.transform.position = Vector3.zero;
        //            newButton.name = "Recall Button 1";
        //            newButton.AddComponent<customTriggerObject>();
        //            Component trigger = newButton.GetComponent<customTriggerObject>();
        //            if (trigger != null)
        //            {
        //                newButton.GetComponent<customTriggerObject>().SetConnectedObject(transform.parent.gameObject);
        //            }
        //        }
        //    }
        //    if(recallButton2 == null)
        //    {

        //    }
        //}

        //if(secondPosition == null)
        //{
        //    GameObject newChild = new GameObject();
        //    newChild.transform.parent = this.transform;
        //    newChild.transform.localPosition = Vector3.zero;
        //    newChild.name = "Opposite Position";
        //    secondPosition = newChild.transform;
        //}
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        firstPosition = transform.position;
        direction = secondPosition.position - firstPosition;
        speed = direction.magnitude / movementTime;

        targetPosition = secondPosition.position;

        delayTimer += delayAmount;
    }

    // Update is called once per frame
    void Update()
    {
        if (isMoving)
        {
            move(targetPosition);
        }
        else if (isReversible)
        {
            delayTimer += Time.deltaTime;
        }
    }

    void move(Vector3 targetPosition)
    {
        objectToMove.transform.position = Vector3.MoveTowards(objectToMove.transform.position, targetPosition, speed * Time.deltaTime);

        if(objectToMove.transform.position == targetPosition)
        {
            delayTimer = 0;
            isMoving = false;

            if(!atOppositeEnd)
            {
                atOppositeEnd = true;
            }
            else
            {
                atOppositeEnd = false;
            }

            if (isReversible)
            {
                flipTarget();
            }
        }
    }
    
    void flipTarget()
    {
        if (!atOppositeEnd)
        {
            targetPosition = secondPosition.position;
        }
        else
        {
            targetPosition = firstPosition;
        }
    }

    public bool GetIsMoving()
    {
        return isMoving;
    }

    public void SetIsMoving(bool _isMoving)
    {
        isMoving = true;
    }

    public float GetDelayTimer()
    {
        return delayTimer;
    }

    public float GetDelayAmount()
    {
        return delayAmount;
    }
}
