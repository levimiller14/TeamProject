using UnityEngine;

public class deadEndCap : MonoBehaviour
{
    [Header("----Cap Settings----")]
    public connectionDirection facingDirection;

    [Header("----Visual Variants----")]
    [SerializeField] GameObject[] visualVariants;

    void Start()
    {
        selectRandomVariant();
    }

    void selectRandomVariant()
    {
        if (visualVariants == null || visualVariants.Length == 0) return;

        int index = Random.Range(0, visualVariants.Length);
        for (int i = 0; i < visualVariants.Length; i++)
        {
            if (visualVariants[i] != null)
            {
                visualVariants[i].SetActive(i == index);
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 dir = roomConnectionPoint.getDirectionVector(facingDirection);
        Gizmos.DrawRay(transform.position, dir * 1f);
    }
}
