using UnityEngine;

public class Flashlight : MonoBehaviour
{
    [Header("----- Light Settings -----")]
    [SerializeField] float range = 30f;
    [SerializeField] float spotAngle = 45f;
    [SerializeField] float intensity = 2f;
    [SerializeField] Color lightColor = Color.white;

    [Header("----- Controls -----")]
    [SerializeField] KeyCode toggleKey = KeyCode.F;

    Light flashlightLight;
    bool isOn;

    void Start()
    {
        CreateFlashlight();
    }

    void Update()
    {
        if (gameManager.instance != null && gameManager.instance.isPaused)
            return;

        if (Input.GetKeyDown(toggleKey))
        {
            Toggle();
        }
    }

    void CreateFlashlight()
    {
        GameObject lightObj = new GameObject("FlashlightSpot");
        lightObj.transform.SetParent(transform);
        lightObj.transform.localPosition = Vector3.zero;
        lightObj.transform.localRotation = Quaternion.identity;

        flashlightLight = lightObj.AddComponent<Light>();
        flashlightLight.type = LightType.Spot;
        flashlightLight.range = range;
        flashlightLight.spotAngle = spotAngle;
        flashlightLight.intensity = intensity;
        flashlightLight.color = lightColor;
        flashlightLight.shadows = LightShadows.Soft;
        flashlightLight.enabled = false;

        isOn = false;
    }

    public void Toggle()
    {
        if (flashlightLight == null)
            return;

        isOn = !isOn;
        flashlightLight.enabled = isOn;
    }

    public void SetOn(bool state)
    {
        if (flashlightLight == null)
            return;

        isOn = state;
        flashlightLight.enabled = isOn;
    }

    public bool IsOn() => isOn;
}
