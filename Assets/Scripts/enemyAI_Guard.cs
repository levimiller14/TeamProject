using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class enemyAI_Guard : MonoBehaviour, IDamage
{
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;

    [SerializeField] int HP;
    [SerializeField] int faceTargetSpeed;
    [SerializeField] int FOV;
    //[SerializeField] int turnSpeed;
    [SerializeField] GameObject bullet;
    [SerializeField] float shootRate;
    [SerializeField] Transform shootPos;

    Color colorOrig;

    float shootTimer;
    float angleToPlayer;

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

    //Range in which guard can see player to shoot
    bool playerInSightRange;

    Vector3 playerDir;
    Vector3 alertTargetPos;
    Vector3 alertLookDir;
    Vector3 lastAlertPosition;
    Transform playerTransform;

    void Start()
    {
        colorOrig = model.material.color;
        //gameManager.instance.UpdateGameGoal(1);
        if (gameManager.instance.player != null)
            playerTransform = gameManager.instance.player.transform;
    }

    void Update()
    {
        shootTimer += Time.deltaTime;
        if (playerInSightRange && canSeePlayer())
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
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;
    }
    public void onAlert(Vector3 alertPosition, Vector3 alertForward)
    {
        alertTargetPos = alertPosition;

        Vector3 playerDir = alertForward;
        playerDir.y = 0;

        if (playerDir.sqrMagnitude > 0.01f)
        {
            Quaternion rot = Quaternion.LookRotation(playerDir);
            transform.rotation = rot;
        }
        state = guardState.Alerted;
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
}


