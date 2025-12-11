using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class enemyAI : MonoBehaviour, IDamage
{
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;

    [SerializeField] int HP;
    [SerializeField] int faceTargetSpeed;
    [SerializeField] int FOV;

    [SerializeField] GameObject bullet;
    [SerializeField] float shootRate;
    [SerializeField] Transform shootPos;

    Color colorOrig;

    float shootTimer;
    float angleToPlayer;

    // status effects
    private Coroutine poisoned;
    private bool tazed;

    bool playerInRange;

    Vector3 playerDir;
    Transform playerTransform;

    void Start()
    {
        colorOrig = model.material.color;
        gameManager.instance.UpdateGameGoal(1);

        if (gameManager.instance.player != null)
            playerTransform = gameManager.instance.player.transform;

    }

    // Update is called once per frame
    void Update()
    {
        shootTimer += Time.deltaTime;

        if(playerInRange && canSeePlayer())
        {
            
        }
    }

    bool canSeePlayer()
    {
        if (playerTransform == null) return false;

        Vector3 playerPos = playerTransform.position;
        playerDir = playerPos - transform.position;
        angleToPlayer = Vector3.Angle(playerDir, transform.forward);

        RaycastHit hit;
        if(Physics.Raycast(transform.position, playerDir, out hit))
        {
            if(angleToPlayer <= FOV && hit.collider.CompareTag("Player"))
            {
                agent.SetDestination(playerPos);

                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    faceTarget();
                }

                if (shootTimer >= shootRate)
                {
                    shoot();
                }

                return true;
            }
        }

        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    void faceTarget()
    {
        Quaternion rot = Quaternion.LookRotation(new Vector3(playerDir.x, transform.position.y, playerDir.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * faceTargetSpeed);
    }

    void shoot()
    {
        if (!tazed)
        {
            shootTimer = 0;
            Instantiate(bullet, shootPos.position, transform.rotation);
        }
    }

    public void takeDamage(int amount)
    {
        HP -= amount;
        if (playerTransform != null)
            agent.SetDestination(playerTransform.position);

        if (HP <= 0)
        {
            gameManager.instance.UpdateGameGoal(-1);
           if(statTracker.instance != null)
            {
                statTracker.instance.IncrementEnemiesDefeated();
            }
            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(flashRed());
        }
    }

    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;
    }

    // poison routines
    public void poison(int damage, float rate, float duration)
    {
        if (poisoned != null)
        {
            StopCoroutine(poisoned); // cuts off current poison, effective duration reset
        }
        poisoned = StartCoroutine(PoisonRoutine(damage, rate, duration));
    }

    private IEnumerator PoisonRoutine(int damage, float rate, float duration)
    {
        float timer = 0f;
        WaitForSeconds wait = new WaitForSeconds(rate);

        while (timer < duration)
        {
            takeDamage(damage);
            timer += rate;
            yield return wait;
        }
        poisoned = null;
    }

    // Tazed effect
    public void taze(int damage, float duration)
    {
        takeDamage(damage);
        if (!tazed)
        {
            StartCoroutine(StunRoutine(duration));
        }
    }

    private IEnumerator StunRoutine(float duration)
    {
        tazed = true;
        if (agent != null)
        {
            agent.isStopped = true;
        }
        yield return new WaitForSeconds(duration);
        tazed = false;
        if(agent != null)
        {
            agent.isStopped = false;
        }
    }

}
