using UnityEngine;

public class cameraController : MonoBehaviour
{
    // Levi edits/additions
    // changing Serialized Fields to public so I can access in settingsManager.cs
    public int sens;
    public bool invertY;
    //[SerializeField] int sens;
    //[SerializeField] bool invertY;
    [SerializeField] int lockVertMin, lockVertMax;

    public float wallTiltZ;
    public float slideTiltZ;

    [Header("----- Movement Tilt -----")]
    [SerializeField] float moveTiltAmount = 2f;
    [SerializeField] float moveTiltSpeed = 8f;

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
}
