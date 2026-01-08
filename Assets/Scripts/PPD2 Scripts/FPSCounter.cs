using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    float deltaTime;
    GUIStyle style;

    void Start()
    {
        style = new GUIStyle();
        style.alignment = TextAnchor.UpperRight;
        style.fontSize = 14;
        style.normal.textColor = Color.white;
        style.fontStyle = FontStyle.Bold;
    }

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    void OnGUI()
    {
        int fps = Mathf.RoundToInt(1f / deltaTime);
        Rect rect = new Rect(Screen.width - 60, 10, 50, 20);
        GUI.Label(rect, $"{fps} FPS", style);
    }
}
