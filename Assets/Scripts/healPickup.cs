using UnityEngine;

public class healPickup : MonoBehaviour
{
    [SerializeField] int healAmount;

    private void OnTriggerEnter(Collider other)
    {
        IHeal _heal = other.GetComponent<IHeal>();

        if(_heal != null)
        {
            _heal.heal(healAmount);
            Destroy(gameObject);
        }
    }
}
