using UnityEngine;

public class PrefabWarmer : MonoBehaviour
{
    [SerializeField] GameObject[] prefabsToWarm;

    void Awake()
    {
        WarmPrefabs();
    }

    void WarmPrefabs()
    {
        if (prefabsToWarm == null) return;

        foreach (var prefab in prefabsToWarm)
        {
            if (prefab == null) continue;

            // instantiate off-screen, then destroy immediately
            // this forces Unity to compile all shaders for the prefab
            GameObject instance = Instantiate(prefab, Vector3.one * -9999f, Quaternion.identity);
            Destroy(instance);
        }
    }
}
