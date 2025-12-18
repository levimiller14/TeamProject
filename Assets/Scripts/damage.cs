using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using Unity.VisualScripting;

public class damage : MonoBehaviour
{
    enum damageType {moving, stationary, DOT, homing, poison, frost, shock, laser}
    [SerializeField] damageType type;
    [SerializeField] Rigidbody rb;

    [SerializeField] public int damageAmount; // Levi change here, publicizing damageAmount
    [SerializeField] float damageRate;
    [SerializeField] float duration;
    [SerializeField] int speed;
    [SerializeField] int destroyTime;
    [Range(1, 0.1f)] [SerializeField] float playerSlowedSpeed;
    [SerializeField] ParticleSystem hitEffect;

    bool isDamaging;
    bool hasDealtDamage;
    float laserTimer;
    public float slowedSpeed;

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
        if(type == damageType.frost)
        {
            slowedSpeed = playerSlowedSpeed;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(type == damageType.laser)
        {
            laserTimer = Time.deltaTime;
        }
        if(type == damageType.homing)
        {
            rb.linearVelocity = (gameManager.instance.player.transform.position - transform.position).normalized * speed * Time.deltaTime * 35;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
            return;

        // prevent multiple damage from same projectile hitting multiple colliders
        if (hasDealtDamage && (type == damageType.moving || type == damageType.homing))
            return;

        IDamage dmg = other.GetComponent<IDamage>();

        if(dmg != null && type != damageType.DOT && type != damageType.frost)
        {
            dmg.takeDamage(damageAmount);
            hasDealtDamage = true;

            if (type == damageType.shock)
            {
                dmg.taze(duration);
            }
        }

        if(type == damageType.homing || type == damageType.moving)
        {
            if(hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }

        if (type == damageType.frost && dmg != null)
        {
            dmg.takeDamage(damageAmount);
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
            if(type == damageType.laser && !isDamaging && laserTimer >= damageRate)
            {
                laserTimer = 0;
                damageOther(dmg);
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

    // Levi addition
    public void ApplyDifficultyMultiplier(float multiplier)
    {
        damageAmount = Mathf.RoundToInt(damageAmount * multiplier);
    }
}

//