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

    private void Start()
    {
        if(sensitivitySlider != null && gameManager.instance.player != null)
        {
            cameraController camScript = gameManager.instance.player.GetComponentInChildren<cameraController>();
            if(camScript != null)
            {
                sensitivitySlider.value = camScript.sens;
            }
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
