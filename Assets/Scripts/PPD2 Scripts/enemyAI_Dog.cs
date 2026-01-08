using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using static enemyAI_Guard;
using static enemyAI_Guard_Handler;

public class enemyAI_Dog : MonoBehaviour, IDamage, IHeal
{
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Animator anim;

    [SerializeField] enemyAI_Guard doghandler;
    [SerializeField] int HP;
    [SerializeField] int maxHP;
    [SerializeField] int faceTargetSpeed;
    [SerializeField] int FOV;
    [SerializeField] float alertRadius;
    [SerializeField] float barkCooldown;
    [SerializeField] int roamDist;
    [SerializeField] int roamPauseTime;
    [SerializeField] float alertDur;

    //Speeds for changing animation for Dog
    [SerializeField] float roamSpeed;
    [SerializeField] float chaseSpeed;
    [SerializeField] float animTranSpeed;

    Color colorOrig;
    MaterialPropertyBlock propBlock;
    static readonly int colorId = Shader.PropertyToID("_BaseColor");

    //Range in which dog can smell player
    bool playerInScentRange;
    bool playerInSightRange;

    float roamTimer;
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
        propBlock = new MaterialPropertyBlock();
        model.GetPropertyBlock(propBlock);
        colorOrig = propBlock.GetColor(colorId);
        if (colorOrig == Color.clear)
            colorOrig = model.sharedMaterial.color;

        if(difficultyManager.instance != null)
        {
            HP = Mathf.RoundToInt(HP * difficultyManager.instance.GetHealthMultiplier());
            alertRadius *= difficultyManager.instance.GetDogDetectionMultiplier();

            SphereCollider triggerCollider = GetComponent<SphereCollider>();
            if(triggerCollider != null && triggerCollider.isTrigger)
            {
                triggerCollider.radius *= difficultyManager.instance.GetDogDetectionMultiplier();
            }
        }

        startingPos = (doghandler != null) ? doghandler.transform.position : transform.position;
        stoppingDistOrig = agent.stoppingDistance;
        if (gameManager.instance.player != null)
            playerTransform = gameManager.instance.player.transform;
    }

    void Update()
    {
        applyStateMovement();
        locomotionAnim();

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
    }

    void locomotionAnim()
    {
        float agentCurSpeed = agent.velocity.magnitude / agent.speed;
        float agentSpeedAnim = anim.GetFloat("Speed");
        anim.SetFloat("Speed", Mathf.Lerp(agentSpeedAnim, agentCurSpeed, Time.deltaTime * animTranSpeed));
    }

    void applyStateMovement()
    {
        switch(state)
        {
            case dogState.Idle:
                agent.speed = roamSpeed;
                break;

            case dogState.Alerted:

                agent.speed = roamSpeed;
                break;

            case dogState.Chase:
                agent.speed = chaseSpeed;
                break;
        }
    }
    void IdleBehavior()
    {
        if (agent.remainingDistance < 0.01f)
            roamTimer += Time.deltaTime;
        
        checkRoam();

        if (playerInScentRange)
        {
            state = dogState.Alerted;
            barkTimer = 0;
            return;
        }

        if (canSeePlayer())
        {
            state = dogState.Chase;
            return;
        }
    }
    void checkRoam()
    {
        if (agent.remainingDistance < 0.01f && roamTimer >= roamPauseTime)
        {
            roam();
        }
    }
    void roam()
    {
        roamTimer = 0;
        agent.stoppingDistance = 0;

        Vector3 ranPos = Random.insideUnitSphere * roamDist;
        ranPos += startingPos;

        NavMeshHit hit;
        NavMesh.SamplePosition(ranPos, out hit, roamDist, 1);
        agent.SetDestination(hit.position);
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
            //gameManager.instance.UpdateGameGoal(-1);

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
        propBlock.SetColor(colorId, Color.red);
        model.SetPropertyBlock(propBlock);
        yield return new WaitForSeconds(0.1f);
        propBlock.SetColor(colorId, colorOrig);
        model.SetPropertyBlock(propBlock);
    }

    void bark()
    {
        anim.SetTrigger("Bark");
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

    // Tazed effect
    public void taze(/*int damage,*/ float duration)
    {
        //takeDamage(damage);
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
        propBlock.SetColor(colorId, new Color(0.4f, 1f, 0.4f));
        model.SetPropertyBlock(propBlock);
        yield return new WaitForSeconds(0.1f);
        propBlock.SetColor(colorId, colorOrig);
        model.SetPropertyBlock(propBlock);
    }
}
