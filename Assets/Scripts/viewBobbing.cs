using UnityEngine;

public class viewBobbing : MonoBehaviour
{
    [Header("----- References -----")]
    [SerializeField] CharacterController controller;

    [Header("----- Bobbing Settings -----")]
    [SerializeField] float walkBobSpeed = 12f;
    [SerializeField] float walkBobAmount = 0.15f;
    [SerializeField] float sprintBobMultiplier = 1.4f;
    [SerializeField] float smoothTime = 0.08f;

    Vector3 originalLocalPos;
    float bobTimer;
    float currentBobAmount;
    float bobVelocity;

    void Start()
    {
        originalLocalPos = transform.localPosition;
    }

    void Update()
    {
        if (controller == null) return;

        float inputMagnitude = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).magnitude;
        bool isMoving = inputMagnitude > 0.1f && controller.isGrounded;

        float targetBobAmount = isMoving ? walkBobAmount : 0f;

        if (Input.GetButton("Sprint") && isMoving)
        {
            targetBobAmount *= sprintBobMultiplier;
        }

        currentBobAmount = Mathf.SmoothDamp(currentBobAmount, targetBobAmount, ref bobVelocity, smoothTime);

        if (currentBobAmount > 0.001f)
        {
            bobTimer += Time.deltaTime * walkBobSpeed;

            float bobOffsetY = Mathf.Sin(bobTimer) * currentBobAmount;
            float bobOffsetX = Mathf.Cos(bobTimer * 0.5f) * currentBobAmount * 0.5f;

            transform.localPosition = originalLocalPos + new Vector3(bobOffsetX, bobOffsetY, 0f);
        }
        else
        {
            bobTimer = 0f;
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalLocalPos, Time.deltaTime * 10f);
        }
    }
}
