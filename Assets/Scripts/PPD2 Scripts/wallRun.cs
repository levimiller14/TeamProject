using UnityEngine;

public class wallRun : MonoBehaviour
{
    [Header("References")]
    public CharacterController controller;
    public Transform orientation;
    public Transform cam;

    [Header("Wall Detection")]
    [SerializeField] LayerMask wallRunLayer;
    [SerializeField] float wallCheckDistance = 1.2f;
    [SerializeField] float wallCheckRadius = 0.3f;
    [SerializeField] int wallCheckRays = 5;
    [SerializeField] float minWallRunHeight = 1.5f;

    [Header("Entry Requirements")]
    [SerializeField] float minEntrySpeed = 4f;
    [SerializeField] float maxWallApproachAngle = 70f;
    [SerializeField] float minWallApproachAngle = 10f;

    [Header("Wall Run Physics")]
    [SerializeField] float wallRunFriction = 2f;
    [SerializeField] float wallRunGravity = 8f;
    [SerializeField] float maxWallRunDuration = 2.5f;
    [SerializeField] float initialVerticalBoost = 0.15f;
    [SerializeField] float maxInitialVerticalSpeed = 6f;
    [SerializeField] float momentumTransferRatio = 0.85f;

    [Header("Wall Jump")]
    [SerializeField] float wallJumpForce = 10f;
    [SerializeField] float wallJumpOutwardRatio = 0.6f;
    [SerializeField] float wallJumpUpwardRatio = 0.8f;

    [Header("Camera Tilt")]
    [SerializeField] float tiltAngle = 15f;
    [SerializeField] float tiltSpeed = 8f;

    [SerializeField] playerController playerControl;

    bool isWallRunning;
    float wallRunTimer;
    float currentTilt;

    Vector3 wallNormal;
    Vector3 wallRunVelocity;
    float wallRunVerticalVelocity;
    int wallSide; // -1 left, 0 none, 1 right

    RaycastHit currentWallHit;
    Vector3 lastValidWallPoint;

    public bool IsWallRunning => isWallRunning;
    public Vector3 WallNormal => wallNormal;

    void Awake()
    {
        if (cam == null && Camera.main != null)
            cam = Camera.main.transform;
    }

    public void ProcessWallRun(ref Vector3 moveDir, ref Vector3 playerVel, bool grounded, Vector3 currentVelocity)
    {
        if (grounded)
        {
            if (isWallRunning)
                ExitWallRun(ref playerVel);
            return;
        }

        if (!CanWallRun())
        {
            if (isWallRunning)
                ExitWallRun(ref playerVel);
            return;
        }

        if (isWallRunning)
        {
            ContinueWallRun(ref moveDir, ref playerVel);
        }
        else
        {
            TryStartWallRun(currentVelocity, ref moveDir, ref playerVel);
        }
    }

    void TryStartWallRun(Vector3 incomingVelocity, ref Vector3 moveDir, ref Vector3 playerVel)
    {
        Vector3 horizontalVel = new Vector3(incomingVelocity.x, 0, incomingVelocity.z);
        float horizontalSpeed = horizontalVel.magnitude;

        if (horizontalSpeed < minEntrySpeed)
            return;

        if (!DetectWall(horizontalVel.normalized, out RaycastHit hit, out int side))
            return;

        float approachAngle = Vector3.Angle(-hit.normal, horizontalVel.normalized);

        if (approachAngle < minWallApproachAngle || approachAngle > maxWallApproachAngle)
            return;

        StartWallRun(hit, side, incomingVelocity, ref playerVel);
    }

    bool DetectWall(Vector3 moveDirection, out RaycastHit bestHit, out int side)
    {
        bestHit = default;
        side = 0;

        float bestScore = float.MaxValue;
        bool foundWall = false;

        float angleStep = 180f / (wallCheckRays - 1);

        for (int i = 0; i < wallCheckRays; i++)
        {
            float angle = -90f + (angleStep * i);
            Vector3 checkDir = Quaternion.Euler(0, angle, 0) * orientation.forward;

            if (Physics.SphereCast(transform.position, wallCheckRadius, checkDir,
                out RaycastHit hit, wallCheckDistance, wallRunLayer))
            {
                float distScore = hit.distance;
                float angleToMovement = Vector3.Angle(-hit.normal, moveDirection);
                float angleScore = angleToMovement / 90f;

                float totalScore = distScore + angleScore;

                if (totalScore < bestScore)
                {
                    bestScore = totalScore;
                    bestHit = hit;
                    foundWall = true;

                    float sideAngle = Vector3.SignedAngle(orientation.forward, -hit.normal, Vector3.up);
                    side = sideAngle > 0 ? 1 : -1;
                }
            }
        }

        return foundWall;
    }

    void StartWallRun(RaycastHit hit, int side, Vector3 incomingVelocity, ref Vector3 playerVel)
    {
        isWallRunning = true;
        wallRunTimer = 0f;
        wallNormal = hit.normal;
        wallSide = side;
        currentWallHit = hit;
        lastValidWallPoint = hit.point;

        Vector3 wallForward = Vector3.Cross(Vector3.up, wallNormal);

        Vector3 horizontalVel = new Vector3(incomingVelocity.x, 0, incomingVelocity.z);
        if (Vector3.Dot(wallForward, horizontalVel) < 0)
            wallForward = -wallForward;

        float horizontalSpeed = horizontalVel.magnitude;
        wallRunVelocity = wallForward * horizontalSpeed * momentumTransferRatio;

        // clamp incoming vertical momentum - don't let big jumps carry through fully
        float verticalMomentum = Mathf.Clamp(incomingVelocity.y, 0f, maxInitialVerticalSpeed * 0.5f);
        float speedBasedBoost = horizontalSpeed * initialVerticalBoost;
        wallRunVerticalVelocity = Mathf.Min(verticalMomentum + speedBasedBoost, maxInitialVerticalSpeed);

        playerVel = Vector3.zero;
    }

    void ContinueWallRun(ref Vector3 moveDir, ref Vector3 playerVel)
    {
        wallRunTimer += Time.deltaTime;

        if (!StillOnWall())
        {
            ExitWallRun(ref playerVel);
            return;
        }

        if (wallRunTimer >= maxWallRunDuration)
        {
            ExitWallRun(ref playerVel);
            return;
        }

        float speed = wallRunVelocity.magnitude;
        speed -= wallRunFriction * Time.deltaTime;

        if (speed <= 0.5f)
        {
            ExitWallRun(ref playerVel);
            return;
        }

        wallRunVelocity = wallRunVelocity.normalized * speed;
        wallRunVerticalVelocity -= wallRunGravity * Time.deltaTime;

        Vector3 pushToWall = -wallNormal * 2f;

        Vector3 finalVelocity = wallRunVelocity + Vector3.up * wallRunVerticalVelocity + pushToWall;

        controller.Move(finalVelocity * Time.deltaTime);

        moveDir = Vector3.zero;
        playerVel = Vector3.zero;

        if (Input.GetButtonDown("Jump"))
        {
            WallJump(ref playerVel);
        }
    }

    bool StillOnWall()
    {
        Vector3 checkDir = -wallNormal;

        if (Physics.SphereCast(transform.position, wallCheckRadius * 0.5f, checkDir,
            out RaycastHit hit, wallCheckDistance * 1.2f, wallRunLayer))
        {
            float normalDiff = Vector3.Angle(wallNormal, hit.normal);
            if (normalDiff < 45f)
            {
                wallNormal = Vector3.Lerp(wallNormal, hit.normal, Time.deltaTime * 5f);
                currentWallHit = hit;
                lastValidWallPoint = hit.point;

                Vector3 wallForward = Vector3.Cross(Vector3.up, wallNormal);
                if (Vector3.Dot(wallForward, wallRunVelocity) < 0)
                    wallForward = -wallForward;

                float currentSpeed = wallRunVelocity.magnitude;
                wallRunVelocity = wallForward * currentSpeed;

                return true;
            }
        }

        return false;
    }

    void WallJump(ref Vector3 playerVel)
    {
        Vector3 jumpDir = wallNormal * wallJumpOutwardRatio + Vector3.up * wallJumpUpwardRatio;
        jumpDir.Normalize();

        float currentSpeed = wallRunVelocity.magnitude;
        Vector3 forwardMomentum = wallRunVelocity.normalized * currentSpeed * 0.5f;

        playerVel = jumpDir * wallJumpForce + forwardMomentum;

        ExitWallRun(ref playerVel, false);
    }

    void ExitWallRun(ref Vector3 playerVel, bool transferMomentum = true)
    {
        if (!isWallRunning)
            return;

        if (transferMomentum)
        {
            playerVel = wallRunVelocity + Vector3.up * wallRunVerticalVelocity;
        }

        isWallRunning = false;
        wallRunTimer = 0f;
        wallNormal = Vector3.zero;
        wallSide = 0;
        wallRunVelocity = Vector3.zero;
        wallRunVerticalVelocity = 0f;
    }

    public void ForceExitWallRun()
    {
        if (!isWallRunning)
            return;

        isWallRunning = false;
        wallRunTimer = 0f;
        wallNormal = Vector3.zero;
        wallSide = 0;
        wallRunVelocity = Vector3.zero;
        wallRunVerticalVelocity = 0f;
    }

    bool CanWallRun()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minWallRunHeight);
    }

    public void UpdateCameraTilt()
    {
        if (cam == null)
        {
            var main = Camera.main;
            if (main == null)
                return;
            cam = main.transform;
        }

        float targetTilt = 0f;

        if (isWallRunning)
            targetTilt = tiltAngle * wallSide;

        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSpeed);

        var camController = cam.GetComponent<cameraController>();
        if (camController != null)
        {
            camController.wallTiltZ = currentTilt;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!orientation) return;

        Gizmos.color = Color.cyan;
        float angleStep = 180f / (wallCheckRays - 1);
        for (int i = 0; i < wallCheckRays; i++)
        {
            float angle = -90f + (angleStep * i);
            Vector3 checkDir = Quaternion.Euler(0, angle, 0) * orientation.forward;
            Gizmos.DrawLine(transform.position, transform.position + checkDir * wallCheckDistance);
        }

        if (isWallRunning)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + wallNormal * 2f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + wallRunVelocity.normalized * 2f);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * minWallRunHeight);
    }
}
