using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class enemyAI : MonoBehaviour, IDamage, IHeal
{
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Animator anim;
    [SerializeField] Collider weaponCol;

    [SerializeField] int HP;
    [SerializeField] int maxHP;
    [SerializeField] int faceTargetSpeed;
    [SerializeField] int FOV;
    [SerializeField] int roamDist;
    [SerializeField] int roamPauseTime;
    [SerializeField] int animTranSpeed;

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

    // status effects
    private Coroutine poisoned;
    private bool tazed;

    bool playerInRange;

    Vector3 playerDir;
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

        startingPos = transform.position;
        stoppingDistOrig = agent.stoppingDistance;

        if (gameManager.instance.player != null)
            playerTransform = gameManager.instance.player.transform;
    }

    // Update is called once per frame
    void Update()
    {
        shootTimer += Time.deltaTime;
        locomotionAnim();

        if(agent.remainingDistance < 0.01f)
        {
            roamTimer += Time.deltaTime;
        }

        if(playerInRange && canSeePlayer())
        {
            checkRoam();
        }
        else if(!playerInRange)
        {
            checkRoam();
        }
    }

    void locomotionAnim()
    {
        float agentSpeedCur = agent.velocity.normalized.magnitude;
        float agentSpeedAnim = anim.GetFloat("Speed");

        anim.SetFloat("Speed", Mathf.MoveTowards(agentSpeedAnim, agentSpeedCur, Time.deltaTime * animTranSpeed));
    }

    void checkRoam()
    {
        if(agent.remainingDistance < 0.01f && roamTimer >= roamPauseTime)
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

                agent.stoppingDistance = stoppingDistOrig;
                return true;
            }
        }
        agent.stoppingDistance = stoppingDistOrig;
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
            anim.SetTrigger("Shoot");
        }
    }
    
    public void createBullet()
    {
        Instantiate(bullet, shootPos.position, transform.rotation);
    }

    public void weaponColOn()
    {
        weaponCol.enabled = true;
    }

    public void weaponColOff()
    {
        weaponCol.enabled = false;
    }

    public void takeDamage(int amount)
    {
        HP -= amount;
        if (playerTransform != null)
            agent.SetDestination(playerTransform.position);

        if (HP <= 0)
        {
            //gameManager.instance.UpdateGameGoal(-1);
            if(statTracker.instance != null)
            {
                statTracker.instance.IncrementEnemiesDefeated();
            }
            if(dropItem != null)
            {
                dropItem.transform.position = new Vector3(transform.position.x, 2, transform.position.z);
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
        if(agent != null)
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
