using UnityEngine;
using System.Collections;

public class healPickup : MonoBehaviour
{
    [SerializeField] int healAmount;
    //[SerializeField] float healSpeed; //for implementing hot

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
