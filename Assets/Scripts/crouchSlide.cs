using UnityEngine;

public class crouchSlide : MonoBehaviour
{
    [Header("References")]
    [SerializeField] CharacterController controller;
    [SerializeField] Transform cam;
    [SerializeField] Transform playerModel;

    [Header("Crouch Settings")]
    [SerializeField] float crouchHeight = 1f;
    [SerializeField] float standHeight = -1f; // -1 means auto-detect from CharacterController
    [SerializeField] float crouchSpeed = 2f;
    [SerializeField] float crouchTransitionSpeed = 10f;
    [SerializeField] KeyCode crouchKey = KeyCode.C;

    [Header("Slide Settings")]
    [SerializeField] float minSlideSpeed = 5f;
    [SerializeField] float slideBoost = 2f;
    [SerializeField] float slideFriction = 6f;
    [SerializeField] float slideSlopeFriction = 3f;
    [SerializeField] float maxSlideDuration = 1.5f;
    [SerializeField] float slideCooldown = 0.5f;

    [Header("Camera Tilt")]
    [SerializeField] float slideTiltAngle = -8f;
    [SerializeField] float tiltSpeed = 10f;

    [Header("Slide Sparks")]
    [SerializeField] ParticleSystem sparkParticles;
    [SerializeField] Transform sparkSpawnPoint;
    [SerializeField] float sparkEmissionRate = 50f;
    [SerializeField] bool autoCreateSparks = true;
    [SerializeField] Color sparkColor = new Color(1f, 0.6f, 0.2f, 1f);
    [SerializeField] Color sparkColorEnd = new Color(1f, 0.3f, 0f, 0f);
    [SerializeField] Material sparkMaterial;

    bool isCrouching;
    bool isSliding;
    float slideTimer;
    float cooldownTimer;
    float currentHeight;
    float currentTilt;
    Vector3 slideDirection;
    float slideSpeed;

    cameraController camController;
    playerController playerControl;

    public bool IsCrouching => isCrouching;
    public bool IsSliding => isSliding;

    void Awake()
    {
        if (controller == null)
            controller = GetComponent<CharacterController>();

        playerControl = GetComponent<playerController>();

        if (cam == null && Camera.main != null)
            cam = Camera.main.transform;

        if (cam != null)
            camController = cam.GetComponent<cameraController>();
    }

    void Start()
    {
        isCrouching = false;
        isSliding = false;

        // grab stand height from controller as-is
        if (standHeight <= 0 && controller != null)
            standHeight = controller.height;

        if (standHeight <= 0)
            standHeight = 2f;

        currentHeight = standHeight;

        if (sparkParticles == null && autoCreateSparks)
            CreateSparkParticles();

        if (sparkParticles != null)
        {
            var emission = sparkParticles.emission;
            emission.enabled = false;
        }
    }

    void CreateSparkParticles()
    {
        GameObject sparkObj = new GameObject("SlideSparks");
        sparkObj.transform.SetParent(transform);
        sparkObj.transform.localPosition = Vector3.down * 0.5f;

        sparkParticles = sparkObj.AddComponent<ParticleSystem>();

        var main = sparkParticles.main;
        main.startLifetime = 0.3f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(8f, 15f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.06f);
        main.startColor = sparkColor;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 200;
        main.gravityModifier = 2f;

        var emission = sparkParticles.emission;
        emission.rateOverTime = 0f;
        emission.enabled = false;

        var shape = sparkParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 25f;
        shape.radius = 0.1f;

        var colorOverLifetime = sparkParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(sparkColor, 0f),
                new GradientColorKey(sparkColorEnd, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.8f, 0.3f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = grad;

        var sizeOverLifetime = sparkParticles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        var trails = sparkParticles.trails;
        trails.enabled = true;
        trails.lifetime = 0.1f;
        trails.widthOverTrail = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(1f, 0f)
        ));
        trails.inheritParticleColor = true;
        trails.dieWithParticles = true;

        var renderer = sparkObj.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.trailMaterial = CreateSparkMaterial();
        renderer.material = CreateSparkMaterial();
    }

    Material CreateSparkMaterial()
    {
        // use serialized material if assigned to avoid Shader.Find stutter
        if (sparkMaterial != null)
            return sparkMaterial;

        // fallback: use default particle material from renderer settings
        Material mat = new Material(Shader.Find("Particles/Standard Unlit"));
        mat.SetColor("_Color", Color.white);
        mat.SetFloat("_Mode", 1);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.renderQueue = 3000;

        return mat;
    }

    void Update()
    {
        cooldownTimer -= Time.deltaTime;

        HandleInput();
        UpdateHeight();
        UpdateCameraTilt();
        UpdateSparks();
    }

    void HandleInput()
    {
        bool crouchPressed = Input.GetKeyDown(crouchKey);
        bool crouchHeld = Input.GetKey(crouchKey);
        bool crouchReleased = Input.GetKeyUp(crouchKey);

        if (isSliding)
        {
            ContinueSlide();

            bool shouldEndSlide = slideTimer >= maxSlideDuration || slideSpeed < 1f || !crouchHeld;

            if (shouldEndSlide)
            {
                EndSlide();
                // only stay crouched if still holding the key
                if (crouchHeld)
                    StartCrouch();
            }
            return;
        }

        if (crouchPressed)
        {
            Vector3 currentVelocity = GetCurrentVelocity();
            float horizontalSpeed = new Vector3(currentVelocity.x, 0, currentVelocity.z).magnitude;

            if (horizontalSpeed >= minSlideSpeed && controller.isGrounded && cooldownTimer <= 0)
            {
                StartSlide(currentVelocity);
            }
            else
            {
                StartCrouch();
            }
        }
        else if (!crouchHeld && isCrouching)
        {
            // released crouch key while crouching
            if (CanStand())
                EndCrouch();
        }
    }

    Vector3 GetCurrentVelocity()
    {
        if (playerControl != null)
            return playerControl.GetVelocity();
        return Vector3.zero;
    }

    void StartCrouch()
    {
        isCrouching = true;
    }

    void EndCrouch()
    {
        isCrouching = false;
    }

    void StartSlide(Vector3 velocity)
    {
        isSliding = true;
        isCrouching = true;
        slideTimer = 0f;

        slideDirection = new Vector3(velocity.x, 0, velocity.z).normalized;
        if (slideDirection.magnitude < 0.1f)
            slideDirection = transform.forward;

        float horizontalSpeed = new Vector3(velocity.x, 0, velocity.z).magnitude;
        slideSpeed = horizontalSpeed + slideBoost;
    }

    void ContinueSlide()
    {
        slideTimer += Time.deltaTime;

        float friction = slideFriction;

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, currentHeight + 0.5f))
        {
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            Vector3 slopeDir = Vector3.ProjectOnPlane(Vector3.down, hit.normal).normalized;
            float slopeDot = Vector3.Dot(slideDirection, slopeDir);

            if (slopeDot > 0.1f && slopeAngle > 5f)
            {
                friction = slideSlopeFriction;
                slideSpeed += slopeAngle * 0.05f * Time.deltaTime;
            }
        }

        slideSpeed -= friction * Time.deltaTime;
        slideSpeed = Mathf.Max(slideSpeed, 0f);

        Vector3 slideMove = slideDirection * slideSpeed;
        controller.Move(slideMove * Time.deltaTime);
    }

    void EndSlide()
    {
        isSliding = false;
        isCrouching = false;
        cooldownTimer = slideCooldown;

        // calculate how much we need to push up
        float heightDiff = standHeight - controller.height;

        // hard reset height
        currentHeight = standHeight;
        controller.height = standHeight;

        // push transform up to compensate for height change
        if (heightDiff > 0)
        {
            transform.position += Vector3.up * (heightDiff / 2f);
        }

        if (playerControl != null)
        {
            Vector3 exitVelocity = slideDirection * slideSpeed;
            playerControl.SetExternalVelocity(exitVelocity);
        }
    }

    void UpdateHeight()
    {
        // only touch height when actively crouching/sliding or transitioning back
        if (!isCrouching && !isSliding && Mathf.Abs(currentHeight - standHeight) < 0.01f)
            return;

        float targetHeight = (isCrouching || isSliding) ? crouchHeight : standHeight;
        currentHeight = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * crouchTransitionSpeed);

        controller.height = currentHeight;
        // don't touch center - leave it at whatever Unity set it to
    }

    void UpdateCameraTilt()
    {
        if (camController == null)
            return;

        float targetTilt = isSliding ? slideTiltAngle : 0f;
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSpeed);

        camController.slideTiltZ = currentTilt;
    }

    void UpdateSparks()
    {
        if (sparkParticles == null)
            return;

        var emission = sparkParticles.emission;

        if (isSliding && slideSpeed > 2f)
        {
            emission.enabled = true;
            emission.rateOverTime = sparkEmissionRate * (slideSpeed / minSlideSpeed);

            if (sparkSpawnPoint != null)
                sparkParticles.transform.position = sparkSpawnPoint.position;
            else
                sparkParticles.transform.position = transform.position + Vector3.down * (currentHeight / 2f) + slideDirection * 0.5f;

            sparkParticles.transform.rotation = Quaternion.LookRotation(-slideDirection);
        }
        else
        {
            emission.enabled = false;
        }
    }

    bool CanStand()
    {
        float checkHeight = standHeight - crouchHeight;
        Vector3 checkPos = transform.position + Vector3.up * crouchHeight;
        return !Physics.SphereCast(checkPos, controller.radius * 0.9f, Vector3.up, out _, checkHeight);
    }

    public float GetSlideSpeedMultiplier()
    {
        if (isSliding)
            return 0f;
        if (isCrouching)
            return crouchSpeed / 5f;
        return 1f;
    }

    void OnDrawGizmosSelected()
    {
        if (isSliding)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + slideDirection * 2f);
        }

        Gizmos.color = Color.green;
        float height = Application.isPlaying ? currentHeight : standHeight;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * height, 0.3f);
    }
}
