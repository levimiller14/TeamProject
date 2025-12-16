using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using static enemyAI_Guard;

public class enemyAI_Turrets : MonoBehaviour, IDamage
{
    [SerializeField] Renderer model;
    [SerializeField] Renderer modelHead;
    [SerializeField] Transform head;

    [SerializeField] int HP;
    [SerializeField] int facePlayerSpeed;
    [SerializeField] float FOV;

    [SerializeField] GameObject bullet;
    [SerializeField] float fireRate;
    [SerializeField] Transform firePos;

    public enum turretWeaponNumber
    {
        One,
        Two,
        Three
    }
    public enum turretWeaponTyoe
    {
        Standard,
        Homing,
        Shock,
        Poison
    }

    public enum turretState
    {
        Idle,
        Aggro
    }

    public turretState state = turretState.Idle;

    Color colorOrig;
    Color colorOrigHead;

    float fireTimer;
    float angleToPlayer;
    bool isAggro;

    // status effects
    private Coroutine poisoned;
    private bool tazed;

    Vector3 playerDir;

    bool playerInRange;
    Transform playerTransform;

    void Start()
    {
        colorOrig = model.material.color;
        colorOrigHead = modelHead.material.color;

        // levi addition - difficulty HP modifier
        if(difficultyManager.instance != null)
        {
            HP = Mathf.RoundToInt(HP * difficultyManager.instance.GetHealthMultiplier());
        }

        gameManager.instance.UpdateGameGoal(1);
        if (gameManager.instance.player != null)
            playerTransform = gameManager.instance.player.transform;
    }
    void Update()
    {
        fireTimer += Time.deltaTime;

        switch(state)
        {
            case turretState.Idle:
                IdleBehavior();
                break;

            case turretState.Aggro:
                AggroBehavior();
                break;
        }
    }

    void IdleBehavior()
    {

    }

    void AggroBehavior()
    {
        if (isAggro == true)
        {
            facePlayer();
            canSeePlayer();
        }
        if (playerInRange && canSeePlayer())
        {
            facePlayer();
        }
    }
    bool canSeePlayer()
    {
        if (playerTransform == null) return false;

        playerDir = playerTransform.position - transform.position;
        angleToPlayer = Vector3.Angle(playerDir, transform.forward);

        RaycastHit hit;
        if (Physics.Raycast(transform.position, playerDir, out hit))
        {
            if (angleToPlayer <= FOV && hit.collider.CompareTag("Player"))
            {

                if (fireTimer >= fireRate)
                {
                    fire();
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
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
    void facePlayer()
    {
        if(head == null || playerTransform == null)
        {
            return;
        }
        Vector3 targetDir = playerTransform.position;
        targetDir.y = 0;

        playerDir = targetDir - head.position;

        if(playerDir.sqrMagnitude < 0.001f)
        {
            return;
        }
        Quaternion rot = Quaternion.LookRotation(new Vector3(playerDir.x, head.transform.position.y, playerDir.z));

        head.rotation = Quaternion.Slerp(head.rotation, rot, facePlayerSpeed * Time.deltaTime);
    }

    void fire()
    {
        if (!tazed)
        {
            fireTimer = 0;

            // Levi addition damage multiplier
            GameObject bulletObj = Instantiate(bullet, firePos.position, head.transform.rotation);

            // apply dmg mult
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
        isAggro = true;

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
        modelHead.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;
        modelHead.material.color = colorOrigHead;
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
        // no movement in turrets
        tazed = true;
        //if (agent != null)
        //{
        //    agent.isStopped = true;
        //}
        yield return new WaitForSeconds(duration);
        tazed = false;
        //if (agent != null)
        //{
        //    agent.isStopped = false;
        //}
    }

}
