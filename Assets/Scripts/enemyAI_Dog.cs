using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class enemyAI_Dog : MonoBehaviour, IDamage
{
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;

    [SerializeField] enemyAI_Guard_Handler doghandler;
    [SerializeField] int HP;
    [SerializeField] int faceTargetSpeed;
    [SerializeField] int FOV;
    [SerializeField] float alertRadius;
    [SerializeField] float barkCooldown;

    Color colorOrig;
    private Coroutine poisoned;

    //Range in which dog can smell player
    bool playerInScentRange;
    float barkTimer;

    Vector3 playerDir;
    Transform playerTransform;

    void Start()
    {
        colorOrig = model.material.color;
        gameManager.instance.UpdateGameGoal(1);
        if (gameManager.instance.player != null)
            playerTransform = gameManager.instance.player.transform;
    }

    void Update()
    {
        if(playerInScentRange)
        {
            barkTimer -= Time.deltaTime;
            if(barkTimer <= 0)
            {
                bark();
                barkTimer = barkCooldown;
            }
        }
        if(canScentPlayer() && playerTransform != null)
        {
            agent.SetDestination(playerTransform.position);
            if(agent.remainingDistance <= agent.stoppingDistance)
            {
                facePlayer();
            }
        }
    }

    bool canScentPlayer()
    {
        return playerInScentRange;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInScentRange = true;
            barkTimer = 0;
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

        gameManager.instance.alertSys.raiseAlert(transform.position, alertRadius);
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
}
