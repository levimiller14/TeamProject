using UnityEngine;
using System.Collections;
using System.Reflection;

public class movableObject : MonoBehaviour
{
    [Header("----- Settings -----")]
    [SerializeField] Transform secondPosition;
    [SerializeField] GameObject objectToMove;
    [SerializeField] float movementTime;
    [SerializeField] float delayAmount;
    [SerializeField] bool isReversible;
    
    bool atOppositeEnd;
    bool isMoving;

    Vector3 firstPosition;
    Vector3 direction;
    Vector3 targetPosition;

    float delayTimer;
    float speed;

    private void OnValidate()
    {      
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
