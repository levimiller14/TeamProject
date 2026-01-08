using UnityEngine;

public class cameraController : MonoBehaviour
{
    // Levi edits/additions
    // changing Serialized Fields to public so I can access in settingsManager.cs
    public float sens;
    public bool invertY = false;
    //[SerializeField] int sens;
    //[SerializeField] bool invertY;
    [SerializeField] int lockVertMin, lockVertMax;

    public float wallTiltZ;
    public float slideTiltZ;

    [Header("----- Movement Tilt -----")]
    [SerializeField] float moveTiltAmount = 0f; // Levi change - off by default
    [SerializeField] float moveTiltSpeed = 8f;

    public bool IsMovementTiltEnabled => moveTiltAmount > 0f;

    float camRotX;
    float currentMoveTilt;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * sens * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sens * Time.deltaTime;

        if (invertY)
        {
            camRotX += mouseY;
        }
        else
        {
            camRotX -= mouseY;
        }

        camRotX = Mathf.Clamp(camRotX, lockVertMin, lockVertMax);

        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float targetMoveTilt = -horizontalInput * moveTiltAmount;
        currentMoveTilt = Mathf.Lerp(currentMoveTilt, targetMoveTilt, Time.deltaTime * moveTiltSpeed);

        transform.localRotation = Quaternion.Euler(camRotX, 0, wallTiltZ + slideTiltZ + currentMoveTilt);

        transform.parent.Rotate(Vector3.up * mouseX);
    }

    public void SetMovementTilt(bool enabled)
    {
        moveTiltAmount = enabled ? 2f : 0;
    }

    public void AdjustPitch(float delta)
    {
        camRotX += delta;
        camRotX = Mathf.Clamp(camRotX, lockVertMin, lockVertMax);
    }
}
