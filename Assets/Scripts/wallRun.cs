using JetBrains.Annotations;
using UnityEngine;

public class wallRun : MonoBehaviour
{
    [Header("References")]
    public CharacterController controller;
    public Transform orientation;
    public Transform cam;

    [Header("Wall Run Settings")]
    [SerializeField] LayerMask wallRunLayer;
    public float wallCheckDistance = 1.0f;
    public float minWallRunHeight = 1.5f;
    public float wallRunSpeed = 12f;
    public float wallRunGravity = 4f;
    public float wallJumpForce = 12f;

    [Header("Camera Tilt")]
    public float tiltAngle = 15f;
    public float tiltSpeed = 7f;

    [SerializeField] playerController playerControl;

    bool isWallLeft;
    bool isWallRight;
    bool isWallRunning;

    RaycastHit leftWallHit;
    RaycastHit rightWallHit;

    float currentTilt = 0f;
    public bool IsWallRunning => isWallRunning;

    public void ProcessWallRun(ref Vector3 moveDir, ref Vector3 playerVel, bool grounded)
    {
        checkForWalls();

        bool movingForward = Input.GetAxis("Vertical") > 0.1f;
        bool canWallRun = !grounded && movingForward && CanWallRun();
        if (canWallRun && (isWallLeft || isWallRight))
        {
            if (!isWallRunning)
                StartWallRun(ref playerVel);

            Vector3 wallNormal = isWallRight ? rightWallHit.normal : leftWallHit.normal;

            Vector3 wallForward = Vector3.Cross(Vector3.up, wallNormal);
            if (Vector3.Dot(wallForward, orientation.forward) < 0)
                wallForward = -wallForward;

            moveDir = wallForward.normalized;

            playerVel.y = -wallRunGravity;

            if(Input.GetButtonDown("Jump"))
            {
                Vector3 jumpDir = wallNormal + Vector3.up;
                jumpDir.Normalize();
                playerVel = jumpDir * wallJumpForce;
                stopWallRun();
            }

            return;
        }

        if (isWallRunning)
            stopWallRun();
    }

    private void Awake()
    {
        if (cam == null && Camera.main != null)
            cam = Camera.main.transform;
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

        if (isWallRunning && isWallRight)
            targetTilt = tiltAngle;
        else if (isWallRunning && isWallLeft)
            targetTilt = -tiltAngle;

        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSpeed);
        var camController = cam.GetComponent<cameraController>();
        if (camController != null)
        {
            camController.wallTiltZ = currentTilt;
        }
        //else
        //{
         //   Vector3 cams = cam.localEulerAngles;
         //   cam.localRotation = Quaternion.identity;
        //}
    }

    void checkForWalls()
    {
        isWallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallCheckDistance, wallRunLayer);
        isWallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallCheckDistance, wallRunLayer);

    }
    bool CanWallRun()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minWallRunHeight);
    }

    void StartWallRun(ref Vector3 playerVel)
    {
        isWallRunning = true;
        if (playerVel.y < 0)
            playerVel.y = 0;

    }

    void stopWallRun()
    {
        isWallRunning = false;
    }

    void OnDrawGizmosSelected()
    {
        if (!orientation) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + orientation.right * wallCheckDistance);
        Gizmos.DrawLine(transform.position, transform.position - orientation.right * wallCheckDistance);
    }

}
