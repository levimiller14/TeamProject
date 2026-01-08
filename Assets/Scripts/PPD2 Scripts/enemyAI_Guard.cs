using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using static enemyAI_Dog;
using static enemyAI_Guard_Handler;

public class enemyAI_Guard : MonoBehaviour, IDamage, IHeal
{
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Animator anim;

    [SerializeField] int animTranSpeed;
    [SerializeField] int HP;
    [SerializeField] int maxHP;
    [SerializeField] int faceTargetSpeed;
    [SerializeField] int FOV;
    [SerializeField] int roamDist;
    [SerializeField] int roamPauseTime;
    [SerializeField] float alertDur;
    [SerializeField] guardType type;

    [SerializeField] float roamSpeed;
    [SerializeField] float chaseSpeed;

    //Dogs for use for Hanndler 1 and 2 (2 is for elites)
    [SerializeField] enemyAI_Dog dog;
    [SerializeField] enemyAI_Dog dog2;

    //Allied Guards
    [SerializeField] enemyAI_Guard guard;
    [SerializeField] enemyAI_Guard guard2;

    //[SerializeField] int turnSpeed;
    [SerializeField] GameObject bullet;
    [SerializeField] float shootRate;
    [SerializeField] Transform shootPos;
    [SerializeField] GameObject dropItem;

    Color colorOrig;

    MaterialPropertyBlock propBlock;
    static readonly int colorId = Shader.PropertyToID("_BaseColor");

    float shootTimer;
    float roamTimer;
    float angleToPlayer;
    float stoppingDistOrig;
    float alertedTimer;

    //status effects
    private Coroutine poisoned;
    private bool tazed;

    //States for the Guards to switch through as we need them
    public enum guardState
    {
        Idle,
        Patrol,
        Alerted,
        Chase
    }

    public guardState state = guardState.Idle;

    public enum guardType
    {
        Guard,
        Handler,
        EliteGuard,
        EliteHandler
    }

    //Is guard Handler or Guard?
    bool isHandler => type == guardType.Handler || type == guardType.EliteHandler;
    //Is guard elite or not??
    bool isElite => type == guardType.EliteGuard || type == guardType.EliteHandler;

    //Range in which guard can see player to shoot
    bool playerInSightRange;

    Vector3 playerDir;
    Vector3 alertTargetPos;
    Vector3 alertLookDir;
    Vector3 lastAlertPosition;
    Vector3 startingPos;

    Transform playerTransform;

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
        }

        startingPos = transform.position;
        stoppingDistOrig = agent.stoppingDistance;

        if (gameManager.instance.player != null)
            playerTransform = gameManager.instance.player.transform;
    }

    void Update()
    {
        applyStateMovement();
        locomotionAnim();

        shootTimer += Time.deltaTime;

        switch(state)
        {
            case guardState.Idle:
                IdleBehavior();
                break;

            case guardState.Alerted:
                AlertedBehavior();
                break;

            case guardState.Chase:
                ChaseBehavior();
                break;
        }
     
    }
    void locomotionAnim()
    {
        float agentSpeedCur = agent.velocity.magnitude / agent.speed;
        float agentSpeedAnim = anim.GetFloat("Speed");

        anim.SetFloat("Speed", Mathf.Lerp(agentSpeedAnim, agentSpeedCur, Time.deltaTime * animTranSpeed));
    }
    void applyStateMovement()
    {
        switch (state)
        {
            case guardState.Idle:
                agent.speed = roamSpeed;
                break;

            case guardState.Alerted:

                agent.speed = roamSpeed;
                break;

            case guardState.Chase:
                agent.speed = chaseSpeed;
                break;
        }
    }
    void IdleBehavior()
    {
        if (agent.remainingDistance < 0.01f)
            roamTimer += Time.deltaTime;

        if (playerInSightRange && canSeePlayer())
        {
            checkRoam();
        }
        else if (!playerInSightRange)
        {
            checkRoam();
        }
        else
        {
            state = guardState.Chase;
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
        if(!canSeePlayer())
        {
            state = guardState.Alerted;
        }
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

                if (shootTimer >= shootRate)
                {
                    shoot();
                }
                agent.stoppingDistance = stoppingDistOrig;
                return true;
            }
        }

        return false;
    }
        private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInSightRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInSightRange = false;
        }
    }
    void facePlayer()
    {
        Quaternion rot = Quaternion.LookRotation(new Vector3(playerDir.x, transform.position.y, playerDir.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * faceTargetSpeed);
    }
    void shoot()
    {
        if (!tazed)
        {
            anim.SetTrigger("Shoot");
            shootTimer = 0;
        }
    }
    
    public void createBullet()
    {
        // Levi addition damage multiplier
        GameObject bulletObj = Instantiate(bullet, shootPos.position, transform.rotation);

        // appply dmg mult
        damage bulletDmg = bulletObj.GetComponent<damage>();

        if (bulletDmg != null && difficultyManager.instance != null)
        {
            bulletDmg.ApplyDifficultyMultiplier(difficultyManager.instance.GetDamageMultiplier());
        }
    }

    public void takeDamage(int amount)
    {
        HP -= amount;
        if (playerTransform != null)
            agent.SetDestination(playerTransform.position);

        //if Guard is Handler, Dogs will rush player on Guard Hit
        if(isHandler)
        {
            if(dog != null)
            {
                dog.onGuardHit(playerTransform.position);
            }
            //If guard is ELITE Handler, Second dog will rush player on hit.
            if(isElite && dog2 != null)
            {
                dog2.onGuardHit(playerTransform.position);
            }
        }

            if (guard != null)
            {
                guard.onAllyHit(playerTransform.position);
            }
            if (guard2 != null)
            {
            guard2.onAllyHit(playerTransform.position);
            }

        if (HP <= 0)
        {
            //gameManager.instance.UpdateGameGoal(-1);

            // incrementing enemies defeated in stats
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
        propBlock.SetColor(colorId, Color.red);
        model.SetPropertyBlock(propBlock);
        yield return new WaitForSeconds(0.1f);
        propBlock.SetColor(colorId, colorOrig);
        model.SetPropertyBlock(propBlock);
    }

    public void onBarkAlert(Vector3 alertPosition, Vector3 alertForward)
    {
        if (difficultyManager.instance != null)
        {
            if (difficultyManager.instance.currentDifficulty == difficultyManager.Difficulty.Easy)
            {
                if (type != guardType.Handler)
                {
                    return;
                }
            }
            if (difficultyManager.instance.currentDifficulty == difficultyManager.Difficulty.Normal)
            {
                if (type == guardType.EliteHandler && type == guardType.EliteGuard)
                {
                    return;
                }
            }
        }
            alertTargetPos = alertPosition;

            Vector3 playerDir = alertForward;
            playerDir.y = 0;

            if (playerDir.sqrMagnitude > 0.01f)
            {
                Quaternion rot = Quaternion.LookRotation(playerDir);
                transform.rotation = rot;
            }
                //moves guard toward anchor
                agent.stoppingDistance = 0;
                agent.SetDestination(alertTargetPos);
                //sets state to alerted
                state = guardState.Alerted;
                alertedTimer = 0;
    }
    public void onAllyHit(Vector3 alertPosition)
    {
        agent.SetDestination(alertPosition);
    }
    public void onDogHit(Vector3 alertPosition)
    {
        if(dog != null || dog != null && dog2 != null)
            agent.SetDestination(alertPosition);
    }

    void AlertedBehavior()
    {
        if(canSeePlayer())
        {
            state = guardState.Chase;
            return;
        }
        if(agent.remainingDistance <= 0.1f)
        {
            alertedTimer += Time.deltaTime;

            if(alertedTimer >= alertDur)
            {
                state = guardState.Idle;
            }
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


