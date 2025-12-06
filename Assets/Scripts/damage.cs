using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using Unity.VisualScripting;

public class damage : MonoBehaviour
{
    enum damageType {moving, stationary, DOT, homing, poison}
    [SerializeField] damageType type;
    [SerializeField] Rigidbody rb;

    [SerializeField] int damageAmount;
    [SerializeField] float damageRate;
    [SerializeField] float duration;
    [SerializeField] int speed;
    [SerializeField] int destroyTime;

    bool isDamaging;
    float lastTick;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(type == damageType.moving || type == damageType.homing)
        {
            Destroy(gameObject, destroyTime);

            if(type == damageType.moving)
            {
                rb.linearVelocity = transform.forward * speed;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(type == damageType.homing)
        {
            rb.linearVelocity = (gameManager.instance.player.transform.position - transform.position).normalized * speed * Time.deltaTime * 35;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
            return;

        IDamage dmg = other.GetComponent<IDamage>();

        if(dmg != null && type != damageType.DOT)
        {
            dmg.takeDamage(damageAmount);
        }

        if(type == damageType.homing || type == damageType.moving)
        {
            Destroy(gameObject);
        }

    }

    private void OnTriggerStay(Collider other) // DOT and Poison
    {
        if (other.isTrigger)
            return;

        IDamage dmg = other.GetComponent<IDamage>();

        if (dmg != null)
        {
            if(type == damageType.DOT && !isDamaging)
            {
                StartCoroutine(damageOther(dmg));
            }
            if(type == damageType.poison && !isDamaging)
            {
                StartCoroutine(poisonOther(dmg));
            }    
        }
    }

    IEnumerator damageOther(IDamage d)
    {
        isDamaging = true;
        d.takeDamage(damageAmount);
        yield return new WaitForSeconds(damageRate);
        isDamaging = false;
    }
    IEnumerator poisonOther(IDamage d)
    {
        isDamaging = true;
        d.poison(damageAmount, damageRate, duration);
        yield return new WaitForSeconds(damageRate);
        isDamaging = false;
    }
}
