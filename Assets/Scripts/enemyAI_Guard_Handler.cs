using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static enemyAI_Guard;

public class enemyAI_Guard_Handler : MonoBehaviour, IDamage, IHeal
{
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;

    [SerializeField] enemyAI_Dog dog;
    [SerializeField] int HP;
    [SerializeField] int maxHP;
    [SerializeField] int faceTargetSpeed;
    [SerializeField] int FOV;
    [SerializeField] int roamDist;
    [SerializeField] int roamPauseTime;
    [SerializeField] float alertDur;

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

    //States for the Guards to switch through as we need them
    public enum guardHandlerState
    {
        Idle,
        Patrol,
        Alerted,
        Chase
    }

    public guardHandlerState state = guardHandlerState.Idle;
    // status effects
    private Coroutine poisoned;
    private bool tazed;

    //Range in which guard can see player to shoot
    bool playerInSightRange;

    Vector3 playerDir;
    Vector3 alertTargetPos;
    Vector3 alertLookDir;
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
        shootTimer += Time.deltaTime;

        switch (state)
        {
            case guardHandlerState.Idle:
                IdleBehavior();
                break;

            case guardHandlerState.Alerted:
                AlertedBehavior();
                break;

            case guardHandlerState.Chase:
                ChaseBehavior();
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
            state = guardHandlerState.Chase;
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
        if (!canSeePlayer())
        {
            state = guardHandlerState.Alerted;
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
            shootTimer = 0;

            // Levi addition damage multiplier
            GameObject bulletObj = Instantiate(bullet, shootPos.position, transform.rotation);

            // appply dmg mult
            damage bulletDmg = bulletObj.GetComponent<damage>();

            if (bulletDmg != null && difficultyManager.instance != null)
            {
                bulletDmg.ApplyDifficultyMultiplier(difficultyManager.instance.GetDamageMultiplier());
            }
        }
    }

    public void takeDamage(int amount)
    {
        HP -= amount;
        if (playerTransform != null)
            agent.SetDestination(playerTransform.position);

        if (dog != null)
        {
            dog.onGuardHit(transform.position);
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
    public void onBarkAlert(Vector3 alertPosition, Vector3 alertForward)
    {
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
        state = guardHandlerState.Alerted;
        alertedTimer = 0;
    }

    public void onDogHit(Vector3 alertPosition)
    {
        agent.SetDestination(alertPosition);
    }

    void AlertedBehavior()
    {
        if (canSeePlayer())
        {
            state = guardHandlerState.Chase;
            return;
        }
        if (agent.remainingDistance <= 0.1f)
        {
            alertedTimer += Time.deltaTime;

            if (alertedTimer >= alertDur)
            {
                state = guardHandlerState.Idle;
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
