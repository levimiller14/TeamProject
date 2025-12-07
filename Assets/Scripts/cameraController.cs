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

    float camRotX;

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

        transform.localRotation = Quaternion.Euler(camRotX, 0, wallTiltZ);

        transform.parent.Rotate(Vector3.up * mouseX);
    }
}
