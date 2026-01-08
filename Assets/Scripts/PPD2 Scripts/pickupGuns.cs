using UnityEngine;

public class pickupGuns : MonoBehaviour
{

    [SerializeField] gunStats gun;


    private void OnTriggerEnter(Collider other) //passthrough trigger
    {
        IPickup pik = other.GetComponent<IPickup>(); //derives from ipickup

        if (pik != null)
        {
            gun.ammoCur = gun.ammoMax; //set ammo to max on pickup
            pik.getGunStats(gun);
            Destroy(gameObject);
        }
    }
}

