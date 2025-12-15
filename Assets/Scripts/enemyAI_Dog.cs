using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using static enemyAI_Guard_Handler;

public class enemyAI_Dog : MonoBehaviour, IDamage, IHeal
{
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;

    [SerializeField] enemyAI_Guard_Handler doghandler;
    [SerializeField] int HP;
    [SerializeField] int maxHP;
    [SerializeField] int faceTargetSpeed;
    [SerializeField] int FOV;
    [SerializeField] float alertRadius;
    [SerializeField] float barkCooldown;

    Color colorOrig;

   

    //Range in which dog can smell player
    bool playerInScentRange;
    bool playerInSightRange;

    float angleToPlayer;
    float barkTimer;
    float stoppingDistOrig;
    public Transform forwardAnchor;

    //States of dog for use in transitioning the dog behavior
    public enum dogState
    {
        Idle,
        Patrol,
        Alerted,
        Chase
    }

    public dogState state = dogState.Idle;

    Vector3 playerDir;
    Vector3 startingPos;
    Vector3 roamCenter;

    Transform playerTransform;

    // status effects
    private Coroutine poisoned;
    private bool tazed;


    void Start()
    {
        maxHP = HP;
        colorOrig = model.material.color;
        // difficulty mults
        if(difficultyManager.instance != null)
        {
            HP = Mathf.RoundToInt(HP * difficultyManager.instance.GetHealthMultiplier());
            alertRadius *= difficultyManager.instance.GetDogDetectionMultiplier();

            // scale trigger collider for scent detection
            SphereCollider triggerCollider = GetComponent<SphereCollider>();
            if(triggerCollider != null &&triggerCollider.isTrigger)
            {
                triggerCollider.radius *= difficultyManager.instance.GetDogDetectionMultiplier();
            }
        }

        //gameManager.instance.UpdateGameGoal(1);
        startingPos = (doghandler != null) ? doghandler.transform.position : transform.position;
        stoppingDistOrig = agent.stoppingDistance;
        if (gameManager.instance.player != null)
            playerTransform = gameManager.instance.player.transform;
    }

    void Update()
    {
        switch (state)
        {
            case dogState.Idle:
                IdleBehavior();
                break;

            case dogState.Alerted:
                AlertedBehavior();
                break;

            case dogState.Chase:
                ChaseBehavior();
                break;
        }
        //if (playerInScentRange)
        //{
        //    barkTimer -= Time.deltaTime;
        //    if(barkTimer <= 0)
        //    {
        //        bark();
        //        barkTimer = barkCooldown;
        //    }
        //}
    }

    void IdleBehavior()
    {
        if (playerInScentRange)
        {
            state = dogState.Alerted;
            barkTimer = 0;
            return;
        }

        if(canSeePlayer())
        {
            state = dogState.Chase;
            return;
        }
    }
    void ChaseBehavior()
    {
        if(canSeePlayer())
        {
            return;
        }
        state = playerInScentRange ? dogState.Alerted : dogState.Idle;
    }
    bool canScentPlayer()
    {
        return playerInScentRange;
    }
    bool canSeePlayer()
    {
        if (playerTransform == null) return false;

        Vector3 playerPos = playerTransform.position;
        playerDir = playerPos - transform.position;
        angleToPlayer = Vector3.Angle(playerDir, transform.forward);

        RaycastHit hit;
        if (Physics.Raycast(transform.position, playerDir, out hit))
        {
            if (angleToPlayer <= FOV && hit.collider.CompareTag("Player"))
            {
                agent.SetDestination(playerPos);
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    facePlayer();
                }

                return true;
            }
        }
        return false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInScentRange = true;
            barkTimer = 0;

            state = dogState.Alerted;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInScentRange = false;
        }
    }
    void facePlayer()
    {
        Quaternion rot = Quaternion.LookRotation(new Vector3(playerDir.x, 0, playerDir.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * faceTargetSpeed);
    }
    public void onGuardHit(Vector3 alertPosition)
    {
        agent.SetDestination(alertPosition);
    }

    public void takeDamage(int amount)
    {
        HP -= amount;
        if (playerTransform != null)
            agent.SetDestination(playerTransform.position);

        if(doghandler != null)
        {
            doghandler.onDogHit(transform.position);
        }

        if (HP <= 0)
        {
            gameManager.instance.UpdateGameGoal(-1);

            // incrementing enemies defeated in stats
            if (statTracker.instance != null)
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

    void bark()
    {
        if (playerTransform == null) return;

        Vector3 pDir = playerTransform.position;
        Vector3 dir = pDir - transform.position;
        dir.y = 0;

        if(dir.sqrMagnitude > 0.0f)
        {
            transform.rotation = Quaternion.LookRotation(dir);
        }

        gameManager.instance.alertSys.raiseAlert(forwardAnchor.position, forwardAnchor.forward, alertRadius);
    }

    void AlertedBehavior()
    {
        if(canSeePlayer())
        {
            state = dogState.Chase;
            return;
        }

        if (playerInScentRange)
        {
            barkTimer -= Time.deltaTime;
            if (barkTimer <= 0)
            {
                bark();
                barkTimer = barkCooldown;
            }
        }
        else
        {
            state = dogState.Idle;
        }
    }
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

    // tazed effect
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
        if (agent != null)
        {
            agent.isStopped = false;
        }
    }

    public void heal(int healAmount)
    {
        if (healAmount <= 0)
            return;
        if (HP <= 0)
            return;

        int origHP = HP;

        HP = Mathf.Min(HP + healAmount, maxHP);

        if (HP > origHP)
        {
            StartCoroutine(flashGreen());
        }
    }

    IEnumerator flashGreen()
    {
        model.material.color = new Color(0.4f, 1f, 0.4f);
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;
    }
}
