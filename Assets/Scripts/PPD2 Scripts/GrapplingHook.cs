using UnityEngine;

[RequireComponent(typeof(playerController))]
[RequireComponent(typeof(CharacterController))]
public class GrapplingHook : MonoBehaviour
{
    [Header("----- Grapple Settings -----")]
    [SerializeField] float maxGrappleDistance = 50f;
    [SerializeField] float grappleSpeed = 20f;
    [SerializeField] float pullAcceleration = 35f;
    [SerializeField] float airControlStrength = 20f;
    [SerializeField] float swingResponsiveness = 12f;
    [SerializeField] float momentumRetention = 0.9f;
    [SerializeField] float minDistanceToDetach = 2f;
    [SerializeField] LayerMask grappleableLayers = ~0;

    [Header("----- Line Settings -----")]
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] Transform grappleOrigin;
    [SerializeField] float lineExtendSpeed = 150f;
    [SerializeField] float lineWidth = 0.04f;

    playerController player;
    CharacterController controller;
    Camera mainCam;

    Vector3 grapplePoint;
    Vector3 grappleVelocity;
    Vector3 swingVelocity;
    float currentLineLength;
    float ropeLength;
    bool isGrappling;
    bool lineExtending;

    void Reset()
    {
        SetupLineRenderer();
    }

    void Awake()
    {
        player = GetComponent<playerController>();
        controller = GetComponent<CharacterController>();

        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                SetupLineRenderer();
            }
        }

        lineRenderer.enabled = false;
    }

    void Start()
    {
        mainCam = Camera.main;

        if (grappleOrigin == null)
            grappleOrigin = mainCam.transform;
    }

    void SetupLineRenderer()
    {
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();

        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.positionCount = 2;
        lineRenderer.numCapVertices = 4;
        lineRenderer.numCornerVertices = 4;

        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.black;
        lineRenderer.endColor = Color.black;
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;
    }

    void Update()
    {
        if (gameManager.instance != null && gameManager.instance.isPaused)
            return;

        HandleInput();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (isGrappling || lineExtending)
                StopGrapple(false);
            else
                StartGrapple();
        }

        if (Input.GetKeyUp(KeyCode.Q) && (isGrappling || lineExtending))
        {
            StopGrapple(false);
        }

        // space to escape grapple with a jump boost
        if (Input.GetButtonDown("Jump") && isGrappling)
        {
            StopGrapple(true);
        }
    }

    void FixedUpdate()
    {
        if (lineExtending)
        {
            ExtendLine();
        }

        if (isGrappling && !lineExtending)
        {
            ApplyGrappleMovement();
        }
    }

    void LateUpdate()
    {
        if (lineRenderer.enabled)
        {
            UpdateLinePosition();
        }
    }

    void StartGrapple()
    {
        RaycastHit hit;
        if (Physics.Raycast(mainCam.transform.position, mainCam.transform.forward, out hit, maxGrappleDistance, grappleableLayers))
        {
            grapplePoint = hit.point;
            lineExtending = true;
            currentLineLength = 0f;
            ropeLength = Vector3.Distance(transform.position, grapplePoint);
            lineRenderer.enabled = true;

            // inherit current velocity for smooth transition
            grappleVelocity = player.GetVelocity() * 0.5f;
            swingVelocity = Vector3.zero;
        }
    }

    void ExtendLine()
    {
        float targetLength = Vector3.Distance(grappleOrigin.position, grapplePoint);
        currentLineLength += lineExtendSpeed * Time.fixedDeltaTime;

        if (currentLineLength >= targetLength)
        {
            currentLineLength = targetLength;
            lineExtending = false;
            isGrappling = true;
        }
    }

    void UpdateLinePosition()
    {
        Vector3 startPos = grappleOrigin.position;
        Vector3 endPos;

        if (lineExtending)
        {
            Vector3 direction = (grapplePoint - startPos).normalized;
            endPos = startPos + direction * currentLineLength;
        }
        else
        {
            endPos = grapplePoint;
        }

        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);
    }

    void ApplyGrappleMovement()
    {
        // detach if we land while grappling - clear all velocity
        if (controller.isGrounded)
        {
            StopGrapple(false, true);
            return;
        }

        Vector3 toGrapple = grapplePoint - transform.position;
        float distanceToGrapple = toGrapple.magnitude;

        if (distanceToGrapple < minDistanceToDetach)
        {
            StopGrapple(false);
            return;
        }

        Vector3 grappleDir = toGrapple.normalized;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool hasInput = Mathf.Abs(horizontal) > 0.05f || Mathf.Abs(vertical) > 0.05f;

        // calculate swing plane (perpendicular to rope)
        Vector3 right = Vector3.Cross(Vector3.up, grappleDir).normalized;
        if (right.magnitude < 0.1f)
            right = Vector3.Cross(Vector3.forward, grappleDir).normalized;
        Vector3 up = Vector3.Cross(grappleDir, right).normalized;

        if (hasInput)
        {
            Vector3 inputWorld = transform.right * horizontal + transform.forward * vertical;
            inputWorld.Normalize();

            float rightComponent = Vector3.Dot(inputWorld, right);
            float upComponent = Vector3.Dot(inputWorld, up);
            float forwardComponent = Vector3.Dot(inputWorld, grappleDir);

            Vector3 swingForce = (right * rightComponent + up * upComponent) * airControlStrength;

            // forward/backward input affects pull speed
            float pullModifier = 1f + forwardComponent * 0.5f;

            swingVelocity += swingForce * Time.fixedDeltaTime * swingResponsiveness;

            grappleVelocity += grappleDir * pullAcceleration * pullModifier * Time.fixedDeltaTime;
        }
        else
        {
            // no input - direct pull, let swing decay naturally
            grappleVelocity += grappleDir * pullAcceleration * Time.fixedDeltaTime;
            swingVelocity *= 0.95f;
        }

        // combine velocities
        Vector3 totalVelocity = grappleVelocity + swingVelocity;

        Vector3 projectedPos = transform.position + totalVelocity * Time.fixedDeltaTime;
        Vector3 toProjected = projectedPos - grapplePoint;
        if (toProjected.magnitude > ropeLength)
        {
            Vector3 correctedPos = grapplePoint + toProjected.normalized * ropeLength;
            Vector3 correction = correctedPos - projectedPos;
            totalVelocity += correction * 5f;
        }

        ropeLength = Mathf.Min(ropeLength, distanceToGrapple + 1f);

        float maxSpeed = grappleSpeed + Mathf.Max(0, distanceToGrapple - 5f) * 0.8f;
        if (totalVelocity.magnitude > maxSpeed)
        {
            totalVelocity = totalVelocity.normalized * maxSpeed;
            grappleVelocity = totalVelocity * 0.7f;
            swingVelocity = totalVelocity * 0.3f;
        }

        totalVelocity.y -= 5f * Time.fixedDeltaTime;

        controller.Move(totalVelocity * Time.fixedDeltaTime);
        player.SetExternalVelocity(totalVelocity);
    }

    void StopGrapple(bool withJump, bool clearVelocity = false)
    {
        if (!isGrappling && !lineExtending)
            return;

        Vector3 exitVelocity = (grappleVelocity + swingVelocity) * momentumRetention;

        isGrappling = false;
        lineExtending = false;
        lineRenderer.enabled = false;

        if (clearVelocity)
        {
            player.SetExternalVelocity(Vector3.zero);
        }
        else if (withJump)
        {
            player.GrappleJump(exitVelocity);
        }
        else
        {
            player.SetExternalVelocity(exitVelocity);
        }

        grappleVelocity = Vector3.zero;
        swingVelocity = Vector3.zero;
    }

    public bool IsGrappling()
    {
        return isGrappling;
    }

    public bool IsLineExtending()
    {
        return lineExtending;
    }

    public Vector3 GetGrapplePoint()
    {
        return grapplePoint;
    }

    public void CancelGrapple()
    {
        StopGrapple(false, true);
    }
}
