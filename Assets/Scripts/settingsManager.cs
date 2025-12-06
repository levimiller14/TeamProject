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

    public void UpdateSensitivity()
    {
        int newSens = (int)sensitivitySlider.value;
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
