using UnityEngine;
using System;

public class securityCamera : MonoBehaviour
{
    [Header("----- Detection Settings -----")]
    [SerializeField] float detectionRadius = 10f;
    [SerializeField] float fieldOfView = 90f;
    [SerializeField] LayerMask obstacleMask;

    [Header("----- Rotation Settings -----")]
    [SerializeField] float rotationSpeed = 3f;
    [SerializeField] bool rotateTowardsPlayer = true;

    [Header("----- Patrol Settings -----")]
    [SerializeField] bool patrolWhenIdle = true;
    [SerializeField] float patrolAngle = 45f;
    [SerializeField] float patrolSpeed = 1f;

    public static event Action<securityCamera> OnAlarmTriggered;
    public static event Action<securityCamera> OnAlarmCleared;

    public static bool isAlarmActive { get; private set; }

    public bool isPlayerDetected { get; private set; }

    Quaternion startRotation;
    float patrolTimer;
    Transform playerTransform;

    void Start()
    {
        startRotation = transform.rotation;
    }

    void Update()
    {
        if (gameManager.instance == null || gameManager.instance.player == null)
            return;

        if (playerTransform == null)
            playerTransform = gameManager.instance.player.transform;

        bool wasDetected = isPlayerDetected;
        isPlayerDetected = CheckForPlayer();

        if (isPlayerDetected)
        {
            if (!wasDetected)
                TriggerAlarm();

            if (rotateTowardsPlayer)
                RotateTowardsPlayer();
        }
        else
        {
            if (wasDetected)
                ClearAlarm();

            if (patrolWhenIdle)
                Patrol();
        }
    }

    bool CheckForPlayer()
    {
        Vector3 dirToPlayer = playerTransform.position - transform.position;
        float sqrDistToPlayer = dirToPlayer.sqrMagnitude;
        float sqrDetectionRadius = detectionRadius * detectionRadius;

        if (sqrDistToPlayer > sqrDetectionRadius)
            return false;

        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        if (angle > fieldOfView / 2f)
            return false;

        float distToPlayer = Mathf.Sqrt(sqrDistToPlayer);
        if (Physics.Raycast(transform.position, dirToPlayer / distToPlayer, out RaycastHit hit, distToPlayer, obstacleMask))
        {
            if (!hit.collider.CompareTag("Player"))
                return false;
        }

        return true;
    }

    void RotateTowardsPlayer()
    {
        Vector3 dirToPlayer = playerTransform.position - transform.position;
        dirToPlayer.y = 0;

        if (dirToPlayer.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dirToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
    }

    void Patrol()
    {
        patrolTimer += Time.deltaTime * patrolSpeed;
        float angle = Mathf.Sin(patrolTimer) * patrolAngle;
        transform.rotation = startRotation * Quaternion.Euler(0, angle, 0);
    }

    void TriggerAlarm()
    {
        isAlarmActive = true;
        OnAlarmTriggered?.Invoke(this);
    }

    void ClearAlarm()
    {
        isAlarmActive = false;
        OnAlarmCleared?.Invoke(this);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = isPlayerDetected ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Vector3 leftBound = Quaternion.Euler(0, -fieldOfView / 2f, 0) * transform.forward * detectionRadius;
        Vector3 rightBound = Quaternion.Euler(0, fieldOfView / 2f, 0) * transform.forward * detectionRadius;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + leftBound);
        Gizmos.DrawLine(transform.position, transform.position + rightBound);
    }
}
