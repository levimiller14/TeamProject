using UnityEngine;

public class upgradeShopTrigger : MonoBehaviour
{
    [SerializeField] upgradeShop shop;

    bool playerInside = false;
    playerController playerRef;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            playerInside = true;
            playerRef = other.GetComponent<playerController>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            playerRef = null;
        }
    }

    private void Update()
    {
        if (playerInside && Input.GetButtonDown("Open Key") && playerRef != null)
        {
            shop.OpenShop(playerRef);
        }
    }
}
