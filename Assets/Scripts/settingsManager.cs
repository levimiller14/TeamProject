using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class settingsManager : MonoBehaviour
{
    [Header("----Sensitivity Settings----")]
    [SerializeField] Slider sensitivitySlider;
    [SerializeField] int minSens = 50;
    [SerializeField] int maxSens = 500;

    [Header("----Invert Y Settings----")]
    [SerializeField] Toggle invertYToggle;

    // screen tilt toggle update
    [Header("----Screen Tilt Settings----")]
    [SerializeField] Toggle screenTiltToggle;

    private void Start()
    {
        InitializeSettings();
    }

    private void OnEnable()
    {
        // refresh settings when menu opened
        InitializeSettings();
    }

    private void InitializeSettings()
    {
        if (gameManager.instance.player == null) return;

        // slider
        if(sensitivitySlider != null)
        {
            cameraController camScriptp = gameManager.instance.player.GetComponentInChildren<cameraController>();
            if (camScriptp != null)
            {
                sensitivitySlider.SetValueWithoutNotify(camScriptp.sens);
            }
        }

        // invert Y toggle
        if(invertYToggle != null)
        {
            cameraController camScript = gameManager.instance.player.GetComponentInChildren<cameraController>();
            if(camScript != null)
            {
                invertYToggle.SetIsOnWithoutNotify(camScript.invertY);
            }
        }

        // screen tilt toggle
        if(screenTiltToggle != null)
        {
            cameraController camScript = gameManager.instance.player.GetComponentInChildren<cameraController>();
            if(camScript != null)
            {
                screenTiltToggle.SetIsOnWithoutNotify(camScript.IsMovementTiltEnabled);
            }
        }
    }

    // screen tilt toggle capability
    public void UpdateScreenTilt()
    {
        if (screenTiltToggle == null || gameManager.instance.player == null) return;

        bool enableTilt = screenTiltToggle.isOn;
        cameraController camScript = gameManager.instance.player.GetComponentInChildren<cameraController>();
        if(camScript != null)
        {
            camScript.SetMovementTilt(enableTilt);
        }
    }

    public void UpdateSensitivity()
    {
        float newSens = sensitivitySlider.value;
        cameraController camScript = gameManager.instance.player.GetComponentInChildren<cameraController>();
        if(camScript != null)
        {
            camScript.sens = newSens;
        }
    }

    public void UpdateInvertY()
    {
        bool invert = invertYToggle.isOn;
        cameraController camScript = gameManager.instance.player.GetComponentInChildren<cameraController>();
        if(camScript != null)
        {
            camScript.invertY = invert;
        }
    }
}
//